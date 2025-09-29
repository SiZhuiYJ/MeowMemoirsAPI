
using Microsoft.EntityFrameworkCore;
using MeowMemoirsAPI.Interfaces;
using MeowMemoirsAPI.Models.DataBaseContext;
using MeowMemoirsAPI.Models.Http;
using MeowMemoirsAPI.Models.JWT;
using Newtonsoft.Json;

namespace MeowMemoirsAPI.Services
{
    /// <summary>
    /// 用户登录服务
    /// </summary>
    /// <param name="env"></param>
    /// <param name="dB"></param>
    public class AuthService(IWebHostEnvironment env, MyRainbowContext dB) : IAuthService
    {
        private readonly IWebHostEnvironment _env = env;
        private readonly MyRainbowContext _DB = dB;

        #region 登录验证
        /// <summary>
        /// 根据登录类型查找用户
        /// </summary>
        /// <param name="loginType"></param>
        /// <param name="identifier"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<User?> FindUserByLoginTypeAsync(string loginType, string identifier, string password)
        {
            if (loginType == "UserEmail")
            {
                return await _DB.Users.FirstOrDefaultAsync(u => u.UserEmail == identifier && u.UserPwd == password);
            }
            else if (loginType == "RainbowId")
            {
                return await _DB.Users.FirstOrDefaultAsync(u => u.RainbowId == identifier && u.UserPwd == password);
            }
            else if (loginType == "UserPhome")
            {
                return await _DB.Users.FirstOrDefaultAsync(u => u.UserPhone == identifier && u.UserPwd == password);
            }
            return null;
        }
        #endregion

        #region 保存登录信息
        /// <summary>
        /// 保存登录会话
        /// </summary>
        /// <param name="user"></param>
        /// <param name="jwt"></param>
        /// <param name="deviceInfo"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public async Task<int> SaveLoginSessionAsync(User user, JwtTokenResult jwt, string deviceInfo = "", string ip = "")
        {
            // 检查是否已存在登录会话存在就更新登录会话

            var loginSession = new Loginsession
            {
                UserId = user.UserId,
                RefreshToken = jwt.Refresh_token,
                ExpireTime = DateTime.UtcNow.AddDays(7),
                CreateTime = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                DeviceInfo = deviceInfo,
                Ip = ip,
            };
            _DB.Loginsessions.Add(loginSession);
            return await _DB.SaveChangesAsync();
        }
        #endregion

        #region 菜单列表
        /// <summary>
        /// 获取菜单列表
        /// </summary>
        /// <param name="Permissions">权限</param>
        /// <returns>菜单列表</returns>
        public List<MenuItem> GetMenulist(string Permissions)
        {
            string filePath;
            if (Permissions == "v10")
                filePath = Path.Combine(_env.ContentRootPath, "menus_admin.json");
            else
                filePath = Path.Combine(_env.ContentRootPath, "menus_normal_user.json");
            if (!File.Exists(filePath))
                return [];
            string content = File.ReadAllText(filePath);

            // 修复 CS8603: 可能返回 null 引用。
            return JsonConvert.DeserializeObject<List<MenuItem>>(content) ?? [];
        }
        #endregion

        #region 权限列表
        /// <summary>
        /// 获取按钮权限列表
        /// </summary>
        /// <param name="rainbowId"></param>
        /// <returns></returns>
        public async Task<string[]> GetButtonPermissionsAsync(string rainbowId)
        {
            var user = await _DB.Users
                .Include(u => u.Userprofiles)
                .FirstOrDefaultAsync(u => u.RainbowId == rainbowId);
            if (user == null || user.Userprofiles == null)
            {
                return [];
            }
            // 获取用户的权限列表
            //var permissions = user.Userprofiles.Select(up => up.Permission).ToList();
            string[] buttonList = ["system:role:search", "system:role:list", "system:role:add", "system:role:delete", "system:role:update", "system:role:import", "system:role:export"];
            // 返回按钮权限列表
            return buttonList;
        }
        #endregion

        #region 黑名单验证
        /// <summary>
        /// 检查用户是否在黑名单中
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<(bool isBlacklisted, DateTime? expireTime)> IsBlacklistedAsync(string value)
        {
            var isBlacklisted = await _DB.Blacklists.AnyAsync(b => b.Value == value);

            var expireTime = await _DB.Blacklists
                .Where(b => b.Value == value)
                .Select(b => b.ExpireTime)
                .FirstOrDefaultAsync();
            // 检查用户是否在黑名单中
            return (isBlacklisted, expireTime);
        }
        #endregion
        /// <summary>
        /// 安全比较两个字符串（恒定时间比较）
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool SecureCompare(string a, string b)
        {
            if (a == null || b == null) return false;

            var minLength = Math.Min(a.Length, b.Length);
            var result = a.Length == b.Length;

            for (int i = 0; i < minLength; i++)
            {
                result &= (a[i] == b[i]);
            }

            return result;
        }
    }
}
