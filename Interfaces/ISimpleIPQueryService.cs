
using MeowMemoirsAPI.Models.DataBaseContext;
using MeowMemoirsAPI.Models.Http;
using MeowMemoirsAPI.Models.IP;
using MeowMemoirsAPI.Models.JWT;

namespace MeowMemoirsAPI.Interfaces
{
    /// <summary>
    /// 用户认证服务接口
    /// </summary>
    public interface ISimpleIPQueryService
    {
        IPInfo GetIPInfo(string ip);
    }
}
