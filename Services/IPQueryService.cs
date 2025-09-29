using MeowMemoirsAPI.Interfaces;
using MeowMemoirsAPI.Models.IP;
using MeowMemoirsAPI.Parser;

namespace MeowMemoirsAPI.Services
{
    public class IPQueryService: IIPQueryService
    {
        private readonly QQWryParser _qqwryParser;
        private readonly IPDBParser _ipdbParser;

        public IPQueryService()
        {
            var qqwryPath = @"/www/wwwroot/www.meowmemoirs.cn.api/UserFiles/Original/StaticFile/IPDB/qqwry.dat";
            var ipdbPath = @"D:/www/wwwroot/www.meowmemoirs.cn.api/UserFiles/Original/StaticFile/IPDB/ipv6wry.db";

            _qqwryParser = new QQWryParser(qqwryPath);
            _ipdbParser = new IPDBParser(ipdbPath);
        }

        public IPLocation Query(string ip)
        {
            if (System.Net.IPAddress.TryParse(ip, out var ipAddress))
            {
                IPLocation result;
                if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    result = _ipdbParser.Query(ip);
                }
                else
                {
                    result = _qqwryParser.Query(ip);
                }

                // 确保IP字段被设置
                if (result != null)
                {
                    result.IP = ip;
                }

                return result;
            }

            throw new ArgumentException("Invalid IP address");
        }
    }
}
