using MeowMemoirsAPI.Models.DataBaseContext;
namespace MeowMemoirsAPI.Middleware.auth
{
    /// <summary>
    /// HttpContext 扩展方法
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// 获取请求用户信息
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static (User? user, string type) GetRequestUser(this HttpContext context)
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
                return (null, "");

            var claims = context.User.Claims;
            var user = new User
            {
                RainbowId = claims.FirstOrDefault(c => c.Type == "rainbowid")?.Value ?? "",
                UserName = claims.FirstOrDefault(c => c.Type == "username")?.Value ?? "",
                Permissions = claims.FirstOrDefault(c => c.Type == "permissions")?.Value ?? ""
            };
            var typeClaim = claims.FirstOrDefault(c => c.Type == "token_type")?.Value ?? "";
            return (user, type: typeClaim);
        }
    }
}
