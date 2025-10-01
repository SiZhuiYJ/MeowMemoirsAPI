using MaxMind.Db;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Exceptions;
using MeowMemoirsAPI.Interfaces;
using MeowMemoirsAPI.Models.IP;
using System.Net;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Runtime;
namespace MeowMemoirsAPI.Services
{
    // Services/IPQueryService.cs


    public class SimpleIPQueryService : ISimpleIPQueryService
    {
        private readonly string _originalServer;
        private readonly DatabaseReader _cityReader;
        private readonly DatabaseReader _asnReader;
        private readonly Reader _cnReader;
        private readonly ILogger<IPQueryService> _logger;


        // ASN映射字典
        private readonly Dictionary<int, string> _asnMap = new()
    {
        {9812, "东方有线"}, {9389, "中国长城"}, {17962, "天威视讯"}, {17429, "歌华有线"},
        {7497, "科技网"}, {24139, "华数"}, {9801, "中关村"}, {4538, "教育网"}, {24151, "CNNIC"},
        
        // 中国移动
        {38019, "中国移动"}, {139080, "中国移动"}, {9808, "中国移动"}, {24400, "中国移动"},
        {134810, "中国移动"}, {24547, "中国移动"}, {56040, "中国移动"}, {56041, "中国移动"},
        
        // 中国电信
        {4134, "中国电信"}, {4812, "中国电信"}, {23724, "中国电信"}, {136188, "中国电信"},
        
        // 云服务商
        {59019, "金山云"}, {135377, "优刻云"}, {45062, "网易云"}, {137718, "火山引擎"},
        {37963, "阿里云"}, {45102, "阿里云国际"}, {45090, "腾讯云"}, {132203, "腾讯云国际"},
        {55967, "百度云"}, {38365, "百度云"}, {58519, "华为云"}, {55990, "华为云"},
        
        // 国际服务商
        {8075, "微软云"}, {13335, "Cloudflare"}, {55960, "亚马逊云"}, {15169, "谷歌云"}
    };

        private readonly string[] _provinces = {
        "内蒙古", "黑龙江", "河北", "山西", "吉林", "辽宁", "江苏", "浙江", "安徽", "福建",
        "江西", "山东", "河南", "湖北", "湖南", "广东", "海南", "四川", "贵州", "云南",
        "陕西", "甘肃", "青海", "广西", "西藏", "宁夏", "新疆", "北京", "天津", "上海", "重庆"
    };

        public SimpleIPQueryService(ILogger<IPQueryService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _originalServer = configuration["FileStorage:OriginalPath"] ?? "/www/wwwroot/www.meowmemoirs.cn.api/UserFiles/Original/";
            try
            {
                // 初始化数据库读取器
                _cityReader = new DatabaseReader(Path.Combine(_originalServer, "StaticFile/IPDB/GeoLite2-City.mmdb"));
                _asnReader = new DatabaseReader(Path.Combine(_originalServer, "StaticFile/IPDB/GeoLite2-ASN.mmdb"));
                _cnReader = new Reader(Path.Combine(_originalServer, "StaticFile/IPDB/GeoCN.mmdb"));

                _logger.LogInformation("IP databases loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load IP databases");
                throw;
            }
        }

        public IPInfo GetIPInfo(string ip)
        {
            var info = new IPInfo { IP = ip };

            try
            {
                // 1. 使用MaxMind查询基础信息
                var maxmindInfo = GetMaxMindInfo(ip);
                if (maxmindInfo != null)
                {
                    info.AS = maxmindInfo.AS;
                    info.Addr = maxmindInfo.Addr;
                    info.Country = maxmindInfo.Country;
                    info.RegisteredCountry = maxmindInfo.RegisteredCountry;
                    info.Regions = maxmindInfo.Regions;
                }

                // 2. 如果是中国IP，使用GeoCN数据库增强信息
                if (info.Country?.Code == "CN" &&
                    (info.RegisteredCountry == null || info.RegisteredCountry.Code == "CN"))
                {
                    GetGeoCNInfo(ip, info);
                }

                return info;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting IP info for {IP}", ip);
                throw;
            }
        }

        private IPInfo GetMaxMindInfo(string ip)
        {
            var info = new IPInfo();

            try
            {
                // 查询ASN信息 - 修复类型转换
                var asnResponse = _asnReader.Asn(ip);
                if (asnResponse != null)
                {
                    // 修复：long到int的转换
                    var asnNumber = asnResponse.AutonomousSystemNumber ?? 0;
                    info.AS = new ASInfo
                    {
                        Number = (int)asnNumber, // 显式转换
                        Name = asnResponse.AutonomousSystemOrganization ?? "",
                        Info = GetASInfo((int)asnNumber) // 显式转换
                    };
                }

                // 查询城市信息
                var cityResponse = _cityReader.City(ip);
                if (cityResponse != null)
                {
                    // 修复：使用正确的网络信息获取方式
                    info.Addr = GetNetworkAddress(ip, 24); // 使用默认前缀长度

                    // 国家信息 - 修复类型问题
                    if (cityResponse.Country != null)
                    {
                        var countryName = GetCountryName(cityResponse.Country);
                        info.Country = new CountryInfo
                        {
                            Code = cityResponse.Country.IsoCode ?? "",
                            Name = countryName
                        };
                    }

                    // 注册国家信息
                    if (cityResponse.RegisteredCountry != null)
                    {
                        var registeredCountryName = GetCountryName(cityResponse.RegisteredCountry);
                        info.RegisteredCountry = new CountryInfo
                        {
                            Code = cityResponse.RegisteredCountry.IsoCode ?? "",
                            Name = registeredCountryName
                        };
                    }

                    // 地区信息
                    var regions = new List<string>();

                    // 添加行政区划
                    if (cityResponse.Subdivisions?.Any() == true)
                    {
                        foreach (var subdivision in cityResponse.Subdivisions)
                        {
                            var subdivisionName = GetLocalizedName(subdivision);
                            if (!string.IsNullOrEmpty(subdivisionName))
                                regions.Add(subdivisionName);
                        }
                    }

                    // 添加城市
                    if (cityResponse.City != null)
                    {
                        var cityName = GetLocalizedName(cityResponse.City);
                        if (!string.IsNullOrEmpty(cityName) &&
                            (regions.Count == 0 || !regions.Last().Contains(cityName)) &&
                            (info.Country?.Name?.Contains(cityName) != true))
                        {
                            regions.Add(cityName);
                        }
                    }

                    info.Regions = Deduplicate(regions);
                }
            }
            catch (AddressNotFoundException)
            {
                _logger.LogWarning("IP address not found in MaxMind database: {IP}", ip);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error querying MaxMind database for IP: {IP}", ip);
            }

            return info;
        }

        private void GetGeoCNInfo(string ip, IPInfo info)
        {
            try
            {
                var result = _cnReader.Find<Dictionary<string, object>>(IPAddress.Parse(ip));
                if (result != null)
                {
                    // 解析GeoCN数据
                    var geoCNInfo = ParseGeoCNData(result);

                    // 更新网段信息
                    if (geoCNInfo.ContainsKey("network") && geoCNInfo["network"] != null)
                    {
                        info.Addr = geoCNInfo["network"]?.ToString();
                    }

                    // 更新地区信息
                    var regions = new List<string>();
                    if (geoCNInfo.ContainsKey("province") && !string.IsNullOrEmpty(geoCNInfo["province"]?.ToString()))
                        regions.Add(geoCNInfo["province"]?.ToString());
                    if (geoCNInfo.ContainsKey("city") && !string.IsNullOrEmpty(geoCNInfo["city"]?.ToString()))
                        regions.Add(geoCNInfo["city"]?.ToString());
                    if (geoCNInfo.ContainsKey("districts") && !string.IsNullOrEmpty(geoCNInfo["districts"]?.ToString()))
                        regions.Add(geoCNInfo["districts"]?.ToString());

                    info.Regions = Deduplicate(regions.Where(r => !string.IsNullOrEmpty(r)).ToList());

                    // 生成简短地区信息
                    info.RegionsShort = GenerateShortRegions(geoCNInfo);

                    // 更新ISP信息
                    if (info.AS == null)
                        info.AS = new ASInfo();

                    if (geoCNInfo.ContainsKey("isp") && geoCNInfo["isp"] != null)
                        info.AS.Info = geoCNInfo["isp"]?.ToString();

                    // 网络类型
                    if (geoCNInfo.ContainsKey("net") && geoCNInfo["net"] != null)
                        info.Type = geoCNInfo["net"]?.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error querying GeoCN database for IP: {IP}", ip);
            }
        }

        private Dictionary<string, object> ParseGeoCNData(Dictionary<string, object> data)
        {
            var result = new Dictionary<string, object>();

            // 根据GeoCN数据库的实际字段名进行解析
            string[] fields = { "isp", "net", "province", "provinceCode", "city", "cityCode", "districts", "districtsCode", "network" };

            foreach (var field in fields)
            {
                if (data.ContainsKey(field) && data[field] != null)
                {
                    result[field] = data[field];
                }
            }

            return result;
        }

        private List<string> GenerateShortRegions(Dictionary<string, object> geoCNInfo)
        {
            var regions = new List<string>();

            if (geoCNInfo.ContainsKey("province") && geoCNInfo["province"] != null)
            {
                var province = geoCNInfo["province"]?.ToString();
                if (!string.IsNullOrEmpty(province))
                {
                    var shortProvince = ProvinceMatch(province);
                    if (!string.IsNullOrEmpty(shortProvince))
                        regions.Add(shortProvince);
                }
            }

            if (geoCNInfo.ContainsKey("city") && geoCNInfo["city"] != null)
            {
                var city = geoCNInfo["city"]?.ToString();
                if (!string.IsNullOrEmpty(city))
                    regions.Add(city.Replace("市", ""));
            }

            if (geoCNInfo.ContainsKey("districts") && geoCNInfo["districts"] != null)
            {
                var district = geoCNInfo["districts"]?.ToString();
                if (!string.IsNullOrEmpty(district))
                    regions.Add(district);
            }

            return Deduplicate(regions.Where(r => !string.IsNullOrEmpty(r)).ToList());
        }

        // 辅助方法
        private string GetASInfo(int asnNumber)
        {
            return _asnMap.ContainsKey(asnNumber) ? _asnMap[asnNumber] : null;
        }

        private string GetNetworkAddress(string ip, int prefixLength)
        {
            try
            {
                var network = IPNetwork.Parse($"{ip}/{prefixLength}");
                return network.ToString();
            }
            catch
            {
                return $"{ip}/{prefixLength}";
            }
        }

        // 修复：使用正确的参数类型
        private string GetCountryName(MaxMind.GeoIP2.Model.Country country)
        {
            var name = GetLocalizedName(country);
            if (name == "香港" || name == "澳门" || name == "台湾")
                return "中国" + name;
            return name;
        }

        // 修复：通用的本地化名称获取方法
        private string GetLocalizedName(dynamic entity)
        {
            try
            {
                if (entity == null) return "";

                // 检查是否有Names属性
                var namesProperty = entity.GetType().GetProperty("Names");
                if (namesProperty == null) return "";

                var names = namesProperty.GetValue(entity) as IDictionary<string, string>;
                if (names == null) return "";

                // 优先中文，其次英文
                if (names.ContainsKey("zh-CN"))
                    return names["zh-CN"];
                else if (names.ContainsKey("en"))
                    return names["en"];

                return "";
            }
            catch
            {
                return "";
            }
        }

        private string ProvinceMatch(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            foreach (var province in _provinces)
            {
                if (s.Contains(province))
                    return province;
            }
            return "";
        }

        private List<string> Deduplicate(List<string> regions)
        {
            return regions.Where(r => !string.IsNullOrEmpty(r))
                         .Distinct()
                         .ToList();
        }

        public void Dispose()
        {
            _cityReader?.Dispose();
            _asnReader?.Dispose();
            _cnReader?.Dispose();
        }
    }
}
