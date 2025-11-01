using MeowMemoirsAPI.Interfaces;
using MeowMemoirsAPI.Middleware.auth;
using MeowMemoirsAPI.Models.DataBaseContext;
using MeowMemoirsAPI.Models.Log;
using MeowMemoirsAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MeowMemoirsAPI.Controllers
{
    [Route("MeowMemoirs/[controller]")]
    [ApiController]
    public class ScheduleController(ILogService logService, IHttpContextAccessor httpContextAccessor, MyRainbowContext DbContext) : ControllerBase
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
                    u.PermissionLevel == login.PermissionLevel &&
                    u.UserName == login.UserName);

            return user == null
                ? (null, "用户不存在或登录过期")
                : (user, "");
        }
        #endregion

        [HttpPost("PostScheduleList")]
        public async Task<IActionResult> PostScheduleList()
        {
            var (user, error) = await ValidateAccessToken();
            if (user == null)
            {
                return Unauthorized(new { message = error });
            }
            var scheduleList = await _dbContext.Schedules
                .AsNoTracking()
                .Where(s => s.UserId == user.Id && s.IsDeleted == 0)
                .Select(s => new
                {
                    s.Id,
                    s.ScheduleName,
                    s.StartTime,
                    s.WeekCount,
                    s.Timetable,
                    s.Remark,
                    s.CreateTime,
                    s.UpdateTime
                })
                .ToListAsync();
            return Ok(new { code = 200, message = "课程表获取成功", data = new { schedule = scheduleList } });
        }

        [HttpPost("PostCourseListByID")]
        public async Task<IActionResult> PostCourseList(ulong id)
        {
            var (user, error) = await ValidateAccessToken();
            if (user == null)
            {
                return Unauthorized(new { message = error });
            }
            var schedule = await _dbContext.Schedules
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.Id && s.IsDeleted == 0);
            if (schedule == null)
                return NotFound(new { code = 400, message = "获取失败" });
            var courseList = await _dbContext.Courses
                .AsNoTracking()
                .Where(c => c.ScheduleId == id && c.IsDeleted == 0)
                .Select(c => new
                {
                    c.Id,
                    c.ScheduleId,
                    c.CourseName,
                    c.Color,
                    c.TimeSlots,
                    c.Remark,
                    c.CreateTime,
                    c.UpdateTime
                })
                .ToListAsync();
            return Ok(new { code = 200, message = "课程列表获取成功", data = new { course = courseList } });
        }
    }
}
