namespace MeowMemoirsAPI.Models.IP
{
    // Models/IPInfo.cs
    public class IPInfo
    {
        /// <summary>
        /// IP地址
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 自治系统信息
        /// </summary>
        public ASInfo AS { get; set; }
        /// <summary>
        /// IP地址对应的物理地址
        /// </summary>
        public string Addr { get; set; }
        /// <summary>
        /// 国家信息
        /// </summary>
        public CountryInfo Country { get; set; }
        /// <summary>
        /// 注册国家信息
        /// </summary>
        public CountryInfo RegisteredCountry { get; set; }
        /// <summary>
        /// 所属城市
        /// </summary>
        public List<string> Regions { get; set; }
        /// <summary>
        /// 所属城市（简写）
        /// </summary>
        public List<string> RegionsShort { get; set; }
        /// <summary>
        /// 所属城市（拼音）
        /// </summary>
        public string Type { get; set; }

    }

    public class ASInfo
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }
    }

    public class CountryInfo
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    // GeoCN数据库模型
    public class GeoCNInfo
    {
        public string Isp { get; set; }
        public string Net { get; set; }
        public string Province { get; set; }
        public string ProvinceCode { get; set; }
        public string City { get; set; }
        public string CityCode { get; set; }
        public string Districts { get; set; }
        public string DistrictsCode { get; set; }
    }
}
