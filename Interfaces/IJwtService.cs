using MeowMemoirsAPI.Models.DataBaseContext;
using MeowMemoirsAPI.Models.JWT;

namespace MeowMemoirsAPI.Interfaces
{
    /// <summary>
    /// JWT服务接口
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// 生成token
        /// </summary>
        /// <param name="sub"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        JwtTokenResult GenerateEncodedTokenAsync(string sub, User user);
    }
}
