
using MeowMemoirsAPI.Interfaces;
using MeowMemoirsAPI.Models.Http;
using MeowMemoirsAPI.Models.IP;
using MeowMemoirsAPI.Models.Log;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace MeowMemoirsAPI.Controllers
{
    /// <summary>
    /// 访问控制器，提供IP查询等功能
    /// </summary>
    /// <param name="ipQueryService"></param>
    [Route("MeowMemoirs/[controller]")]
    [ApiController]
    public class AccessController(IIPQueryService ipQueryService) : ControllerBase
    {
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
                var result = _ipQueryService.Query(ip);
                if (result == null)
                    return NotFound($"IP {ip} not found in database");

                // 确保IP字段被设置
                result.IP = ip;
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

    }
}
