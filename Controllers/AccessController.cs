
using MeowMemoirsAPI.Interfaces;
using MeowMemoirsAPI.Models.Http;
using MeowMemoirsAPI.Models.IP;
using MeowMemoirsAPI.Models.Log;
using MeowMemoirsAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace MeowMemoirsAPI.Controllers
{

    [Route("MeowMemoirs/[controller]")]
    [ApiController]
    public class AccessController(ILogService logService,IIPQueryService ipQueryService, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly ILogService _logService = logService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IIPQueryService _ipQueryService = ipQueryService;

        /// <summary>
        /// 查询指定IP的地理位置
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        [HttpGet("query")]
        public ActionResult<IPLocation> QueryIP([FromQuery] string ip)
        {
            try
            {
                // 获取浏览器标识
                var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
                var result = _ipQueryService.Query(ip);
                if (result == null)
                    return NotFound($"IP {ip} not found in database");

                // 确保IP字段被设置
                result.IP = ip;
                _logService.LogLogin(new LogLogIn
                {
                    Token = userAgent,
                    Ip = ip ?? "",
                    DateTime = DateTime.Now,
                    Message = "访问记录",
                    RequestBody = JsonSerializer.Serialize(result)
                });
                return Ok(new HttpData
                {
                    Code = 200,
                    Data = new
                    {
                        ipLocation = result
                    },
                    Message = "获取成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error querying IP: {ex.Message}");
            }
        }

        /// <summary>
        /// 查询当前请求的客户端IP的地理位置
        /// </summary>
        /// <returns></returns>
        [HttpGet("myip")]
        public ActionResult<IPLocation> QueryMyIP()
        {
            var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            var clientIP = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(clientIP) || clientIP == "::1")
                clientIP = "127.0.0.1";

            try
            {
                var result = _ipQueryService.Query(clientIP);
                if (result == null)
                    return NotFound($"IP {clientIP} not found in database");

                _logService.LogLogin(new LogLogIn
                {
                    Token = userAgent,
                    Ip = ip ?? "",
                    DateTime = DateTime.Now,
                    Message = "访问记录",
                    RequestBody = JsonSerializer.Serialize(result)
                });
                // 确保IP字段被设置
                result.IP = clientIP;
                return Ok(new HttpData
                {
                    Code = 200,
                    Data = new
                    {
                        ipLocation = result,
                    },
                    Message = "获取成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error querying IP: {ex.Message}");
            }
        }

    }
}
