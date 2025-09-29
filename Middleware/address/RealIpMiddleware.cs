using System.Net;

namespace MeowMemoirsAPI.Middleware.address
{
    /// <summary>
    /// 中间件：获取真实IP地址
    /// </summary>
    /// <param name="next"></param>
    public class RealIpMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        /// <summary>
        /// 中间件：获取真实IP地址
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task Invoke(HttpContext context)
        {
            var headers = context.Request.Headers;
            if (headers.TryGetValue("X-Forwarded-For", out Microsoft.Extensions.Primitives.StringValues value))
            {
                context.Connection.RemoteIpAddress = IPAddress.Parse(value.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries)[0]);
            }
            return _next(context);
        }
    }
}
