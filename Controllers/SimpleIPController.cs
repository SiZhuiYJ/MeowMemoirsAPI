using MeowMemoirsAPI.Interfaces;
using MeowMemoirsAPI.Models.DataBaseContext;
using MeowMemoirsAPI.Models.Http;
using MeowMemoirsAPI.Models.IP;
using MeowMemoirsAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;

namespace MeowMemoirsAPI.Controllers
{
    [ApiController]
    [Route("MeowMemoirs/SimpleIP")]
    public class SimpleIPController(ISimpleIPQueryService ipService, MyRainbowContext dbContext) : ControllerBase
    {
        private readonly ISimpleIPQueryService _ipService = ipService;
        private readonly MyRainbowContext _dbContext = dbContext;
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


        private async Task<string> SetClientIPAsync(string ip)
        {
            // 性能监控
            _stopwatch.Restart();

            // 1. 获取请求上下文信息
            var request = HttpContext.Request;
            var clientIP = ip;

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
                GeoLocation = JsonSerializer.Serialize(_ipService.GetIPInfo(clientIP))
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
                // 实际项目应使用ILogger
                Console.WriteLine($"Database error: {ex.Message}");
            }
            return clientIP;
        }
    }
}
