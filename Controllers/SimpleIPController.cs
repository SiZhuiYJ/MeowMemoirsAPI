using MeowMemoirsAPI.Interfaces;
using MeowMemoirsAPI.Middleware.auth;
using MeowMemoirsAPI.Models.DataBaseContext;
using MeowMemoirsAPI.Models.Http;
using MeowMemoirsAPI.Models.IP;
using MeowMemoirsAPI.Models.Log;
using MeowMemoirsAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text.Json;

namespace MeowMemoirsAPI.Controllers
{
    [ApiController]
    [Route("MeowMemoirs/SimpleIP")]
    public class SimpleIPController(ISimpleIPQueryService ipService, MyRainbowContext dbContext, ILogService logService, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly ISimpleIPQueryService _ipService = ipService;
        private readonly MyRainbowContext _dbContext = dbContext;
        private readonly ILogService _logService = logService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly Stopwatch _stopwatch = new Stopwatch();



        [HttpGet("myip")]
        public async Task<object> GetMyIPAsync()
        {
            var clientIP = await SetClientIPAsync(GetClientIP());
            return Ok(new HttpData
            {
                Code = 200,
                Data = new { ipInfo = _ipService.GetIPInfo(clientIP) },
                Message = "获取成功"
            });
        }

        [HttpGet("query")]
        public async Task<object> QueryIPAsync([FromQuery] string ip)
        {
            var clientIP = await SetClientIPAsync(ip);
            return Ok(new HttpData
            {
                Code = 200,
                Data = new { ipInfo = _ipService.GetIPInfo(clientIP) },
                Message = "获取成功"
            });
        }

        [HttpGet("{ip}")]
        public async Task<object> QueryIPFromPathAsync(string ip)
        {
            var clientIP = await SetClientIPAsync(ip);
            return Ok(new HttpData
            {
                Code = 200,
                Data = new { ipInfo = _ipService.GetIPInfo(clientIP) },
                Message = "获取成功"
            });
        }

        private string GetClientIP()
        {
            var xff = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xff))
                return xff.Split(',')[0].Trim();

            var realIP = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIP))
                return realIP;

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        }
        #region 使用统一方法处理Token验证
        private async Task<(User? user, string error)> ValidateAccessToken()
        {
            var (login, type) = HttpContext.GetRequestUser();
            if (type != "access" || login == null)
            {
                _logService.LogError(new LogError
                {
                    Token = JsonSerializer.Serialize(login),
                    Ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "",
                    DeviceInfo = Request.Headers.UserAgent.ToString(),
                    Name = nameof(ValidateAccessToken),
                    DateTime = DateTime.Now,
                    RequestBody = type,
                    Message = "非法token类型"
                });
                return (null, "非法token");
            }

            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.RainbowId == login.RainbowId &&
                    u.Permissions == login.Permissions &&
                    u.UserName == login.UserName);

            return user == null
                ? (null, "用户不存在或登录过期")
                : (user, "");
        }
        #endregion

        private async Task<string> SetClientIPAsync(string ip)
        {
            // 性能监控
            _stopwatch.Restart();

            // 1. 获取请求上下文信息
            var request = HttpContext.Request;
            var clientIP = ip;
            var (user, error) = await ValidateAccessToken();


            // 2. 构建日志实体
            var logEntry = new IpAccessLog
            {
                IpAddress = clientIP,
                UserAgent = request.Headers.UserAgent.ToString(),
                RequestTime = DateTime.UtcNow,
                RequestMethod = request.Method,
                RequestUrl = $"{request.Path}{request.QueryString}",
                HttpVersion = $"HTTP/{request.Protocol}",
                ResponseStatus = 200, // Ok() 默认返回200
                ResponseTimeMs = 0,  // 稍后计算
                Referer = request.Headers.Referer.ToString(),
                Headers = JsonSerializer.Serialize(request.Headers),
                GeoLocation = JsonSerializer.Serialize(_ipService.GetIPInfo(clientIP)),
                UserId = user?.UserId.ToString(),
            };

            // 3. 计算响应耗时
            _stopwatch.Stop();
            logEntry.ResponseTimeMs = (int)_stopwatch.ElapsedMilliseconds;

            // 4. 异步记录到数据库
            try
            {
                await _dbContext.IpAccessLogs.AddAsync(logEntry);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logService.LogError(new LogError { Token = "", Ip = ip ?? "", DeviceInfo = logEntry.UserAgent ?? "", Name = "BlogController.UploadBlog", DateTime = DateTime.Now, Message = ex.Message });
                // 实际项目应使用ILogger
                Console.WriteLine($"Database error: {ex.Message}");
            }
            return clientIP;
        }
    }
}
