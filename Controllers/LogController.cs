using MeowMemoirsAPI.Interfaces;
using MeowMemoirsAPI.Middleware.auth;
using MeowMemoirsAPI.Models.DataBaseContext;
using MeowMemoirsAPI.Models.Http;
using MeowMemoirsAPI.Models.Log;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Threading.Tasks;

namespace MeowMemoirsAPI.Controllers
{
    [Route("MeowMemoirs/[controller]")]
    [ApiController]
    public class LogController(ILogService logService, IAuthService authService, IHttpContextAccessor httpContextAccessor, MyRainbowContext dbContext) : ControllerBase
    {
        private readonly ILogService _logService = logService;
        private readonly IAuthService _authService = authService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly MyRainbowContext _dbContext = dbContext;



        #region 方法
        private (string? ip, string agent) GetClientInfo()
        {
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            var agent = Request.Headers.UserAgent.ToString();
            return (ip, agent);
        }
        // 使用统一方法处理Token验证
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


        /// <summary>
        /// 获取最新的IP访问日志
        /// </summary>
        /// <returns></returns>
        [HttpPost("ipaccesslogs")]
        public async Task<ActionResult> PostLatestIpAccessLogs()
        {
            var (ip, agent) = GetClientInfo();
            try
            {
                var (user, error) = await ValidateAccessToken();
                if (user == null)
                {
                    _logService.LogError(new LogError { Token = "", Ip = ip ?? "", DeviceInfo = agent ?? "", Name = "BlogController.UploadBlog", DateTime = DateTime.Now, Message = error });
                    return Unauthorized(new HttpData { Code = 401, Message = error });
                }
                var logs = _dbContext.IpAccessLogs;
                return Ok(new
                {
                    Code = 200,
                    Data = new
                    {
                        ipAccessLogs = logs
                    },
                    Message = "Success"
                });
            }
            catch (Exception ex)
            {
                _logService.LogError(new LogError { Token = "", Ip = ip ?? "", DeviceInfo = agent ?? "", Name = "BlogController.UploadBlog", DateTime = DateTime.Now, Message = ex.Message, });
                return StatusCode(500, "Internal server error");
            }

        }
    }
}
