
using MeowMemoirsAPI.Models.DataBaseContext;
using MeowMemoirsAPI.Models.Http;
using MeowMemoirsAPI.Models.JWT;

namespace MeowMemoirsAPI.Interfaces
{
    /// <summary>
    /// 用户认证服务接口
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// 用户登录验证
        /// </summary>
        /// <param name="loginType"></param>
        /// <param name="identifier"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<User?> FindUserByLoginTypeAsync(string loginType, string identifier, string password);
        /// <summary>
        /// 获取菜单列表
        /// </summary>
        /// <param name="Permissions"></param>
        /// <returns></returns>
        List<MenuItem> GetMenulist(string Permissions);

        /// <summary>
        /// 获取按钮权限
        /// </summary>
        /// <param name="rainbowId"></param>
        /// <returns></returns>
        Task<string[]> GetButtonPermissionsAsync(string rainbowId);

        /// <summary>
        /// 检查用户是否被黑名单
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Task<(bool isBlacklisted, DateTime? expireTime)> IsBlacklistedAsync(string value);

        /// <summary>
        /// 保存用户登录会话
        /// </summary>
        /// <param name="user"></param>
        /// <param name="jwt"></param>
        /// <param name="deviceInfo"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        Task<int> SaveLoginSessionAsync(User user, JwtTokenResult jwt, string deviceInfo = "", string ip = "");

        bool SecureCompare(string a, string b);
    }
}
