using MeowMemoirsAPI.Interfaces;
using MeowMemoirsAPI.Models.Http;
using MeowMemoirsAPI.Models.IP;
using MeowMemoirsAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MeowMemoirsAPI.Controllers
{
    [ApiController]
    [Route("MeowMemoirs/SimpleIP")]
    public class SimpleIPController : ControllerBase
    {
        private readonly ISimpleIPQueryService _ipService;

        public SimpleIPController(ISimpleIPQueryService ipService)
        {
            _ipService = ipService;
        }

        [HttpGet("myip")]
        public object GetMyIP()
        {
            var clientIP = GetClientIP();
            return Ok(new HttpData
            {
                Code = 200,
                Data = new
                {
                    ipInfo = _ipService.GetIPInfo(clientIP)
                },
                Message = "获取成功"
            });
        }

        [HttpGet("query")]
        public object QueryIP([FromQuery] string ip)
        {
            return Ok(new HttpData
            {
                Code = 200,
                Data = new
                {
                    ipInfo = _ipService.GetIPInfo(ip)
                },
                Message = "获取成功"
            });
        }

        [HttpGet("{ip}")]
        public object QueryIPFromPath(string ip)
        {
            return Ok(new HttpData
            {
                Code = 200,
                Data = new
                {
                    ipInfo = _ipService.GetIPInfo(ip)
                },
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
    }
}
