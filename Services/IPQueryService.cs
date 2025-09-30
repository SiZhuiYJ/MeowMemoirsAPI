using MeowMemoirsAPI.Interfaces;
using MeowMemoirsAPI.Models.IP;
using MeowMemoirsAPI.Parser;

namespace MeowMemoirsAPI.Services
{
    public class IPQueryService : IIPQueryService
    {
        private readonly string _originalServer;
        private readonly QQWryParser _qqwryParser;
        private readonly IPDBParser _ipdbParser;

        public IPQueryService(IConfiguration configuration)
        {
            var qqwryPath = @"StaticFile/IPDB/qqwry.dat";
            var ipdbPath = @"StaticFile/IPDB/ipv6wry.db";
            _originalServer = configuration["FileStorage:OriginalPath"] ?? "/www/wwwroot/www.meowmemoirs.cn.api/UserFiles/Original/";

            _qqwryParser = new QQWryParser(Path.Combine(_originalServer, qqwryPath));
            _ipdbParser = new IPDBParser(Path.Combine(_originalServer, ipdbPath));
            Directory.CreateDirectory(_originalServer);
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
