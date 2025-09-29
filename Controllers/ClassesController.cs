using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeowMemoirsAPI.Interfaces;
using MeowMemoirsAPI.Middleware.auth;
using MeowMemoirsAPI.Models.DataBaseContext;
using MeowMemoirsAPI.Models.Http;
using MeowMemoirsAPI.Models.Log;
using System.Text.Json;

namespace MeowMemoirsAPI.Controllers
{
    [Route("MeowMemoirs/[controller]")]
    [ApiController]
    public class ClassesController(ILogService logService, IHttpContextAccessor httpContextAccessor, MyRainbowContext DbContext) : ControllerBase
    {
        private readonly ILogService _logService = logService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly MyRainbowContext _dbContext = DbContext;

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
        /// 获取用户班级列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("PostClassesList")]
        public async Task<IActionResult> PostClassesList()
        {
            var (ip, agent) = GetClientInfo();
            try
            {
                var (user, error) = await ValidateAccessToken();
                if (user == null)
                {
                    _logService.LogError(new LogError
                    {
                        Token = "",
                        Ip = ip ?? "",
                        DeviceInfo = agent,
                        Name = nameof(PostClassesList),
                        DateTime = DateTime.Now,
                        Message = error
                    });
                    return Unauthorized(new HttpData
                    {
                        Code = 401,
                        Message = error
                    });
                }
                var classesList = _dbContext.Classes.Where(c => c.UserId == user.UserId).ToList();
                return await Task.FromResult<IActionResult>(Ok(new { code = 200, message = "获取成功", data = classesList }));
            }
            catch (Exception ex)
            {
                _logService.LogError(new MeowMemoirsAPI.Models.Log.LogError { Token = "", Ip = ip ?? "", DeviceInfo = agent, Name = "ClassesController.GetClassesList", DateTime = DateTime.Now, Message = ex.Message.ToString()});
                return await Task.FromResult<IActionResult>(BadRequest(new { code = 500, message = "获取失败" }));
            }
        }
        /// <summary>
        /// 获取用户班级列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("PostClassesListByID")]
        public async Task<IActionResult> PostClassesListByID(int userId)
        {
            var (ip, agent) = GetClientInfo();
            try
            {
                var classesList = _dbContext.Classes.Where(c => c.UserId == userId).ToList();
                return await Task.FromResult<IActionResult>(Ok(new { code = 200, message = "获取成功", data = classesList }));
            }
            catch (Exception ex)
            {
                _logService.LogError(new MeowMemoirsAPI.Models.Log.LogError { Token = "", Ip = ip ?? "", DeviceInfo = agent, Name = "ClassesController.GetClassesList", DateTime = DateTime.Now, Message = ex.Message.ToString(), RequestBody = System.Text.Json.JsonSerializer.Serialize(userId) });
                return await Task.FromResult<IActionResult>(BadRequest(new { code = 500, message = "获取失败" }));
            }
        }
    }
}
