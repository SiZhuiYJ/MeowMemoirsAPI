
using Microsoft.AspNetCore.Mvc;
using MeowMemoirsAPI.Interfaces;
using MeowMemoirsAPI.Models.IP;
using MeowMemoirsAPI.Models.Log;

namespace MeowMemoirsAPI.Controllers
{
    [Route("MeowMemoirs/[controller]")]
    [ApiController]
    public class AccessController(ILogService logService, IHttpContextAccessor httpContextAccessor, IIPQueryService ipQueryService) : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogService _logService = logService;
        private readonly IIPQueryService _ipQueryService = ipQueryService;

        // 提取公共方法：获取客户端IP和UserAgent
        private (string? ip, string agent) GetClientInfo()
        {
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            var agent = Request.Headers.UserAgent.ToString();
            return (ip, agent);
        }

        // 提取公共方法：记录错误日志
        private void LogError(string actionName, string token, string requestBody, Exception? ex)
        {
            var (ip, agent) = GetClientInfo();
            _logService.LogError(new LogError
            {
                Token = token,
                Ip = ip ?? "",
                DeviceInfo = agent,
                Name = $"AuthController.{actionName}",
                DateTime = DateTime.Now,
                Message = ex?.Message ?? "",
                RequestBody = requestBody
            });
        }
        [HttpGet("query")]
        public ActionResult<IPLocation> QueryIP([FromQuery] string ip)
        {
            try
            {
                var result = _ipQueryService.Query(ip);
                if (result == null)
                    return NotFound($"IP {ip} not found in database");

                // 确保IP字段被设置
                result.IP = ip;
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error querying IP: {ex.Message}");
            }
        }

        [HttpGet("myip")]
        public ActionResult<IPLocation> QueryMyIP()
        {
            var clientIP = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(clientIP) || clientIP == "::1")
                clientIP = "127.0.0.1";

            try
            {
                var result = _ipQueryService.Query(clientIP);
                if (result == null)
                    return NotFound($"IP {clientIP} not found in database");

                // 确保IP字段被设置
                result.IP = clientIP;
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error querying IP: {ex.Message}");
            }
        }

    }
}
