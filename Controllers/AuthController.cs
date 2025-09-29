
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeowMemoirsAPI.Interfaces;
using MeowMemoirsAPI.Middleware.auth;
using MeowMemoirsAPI.Models.DataBaseContext;
using MeowMemoirsAPI.Models.Http;
using MeowMemoirsAPI.Models.JWT;
using MeowMemoirsAPI.Models.Log;
using System.Text.Json;

namespace MeowMemoirsAPI.Controllers
{
    /// <summary>
    /// 用户认证控制器
    /// </summary>
    /// <param name="DbContext"></param>
    /// <param name="logService"></param>
    /// <param name="authService"></param>
    /// <param name="httpContextAccessor"></param>
    /// <param name="jwtService"></param>
    [Route("MeowMemoirs/[controller]")]
    [ApiController]
    public class AuthController(ILogService logService, IAuthService authService, IHttpContextAccessor httpContextAccessor, IJwtService jwtService, MyRainbowContext DbContext) : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogService _logService = logService;
        private readonly IAuthService _authService = authService;
        private readonly IJwtService _jwtService = jwtService;
        private readonly MyRainbowContext _dbContext = DbContext;

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

        /// <summary>
        /// 获取全部用户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            var (ip, _) = GetClientInfo();
            var users = _dbContext.Users.AsNoTracking().ToList();
            return Ok(new HttpData
            {
                Code = 200,
                Data = new { users, ip },
                Message = "获取成功"
            });
        }

        /// <summary>
        /// 新用户注册接口
        /// </summary>
        /// <param name="addUser"></param>
        /// <returns></returns>
        [HttpPost("UserRegistration")]
        public async Task<IActionResult> NewUserRegistration([FromBody] AddUser addUser)
        {
            var (ip, agent) = GetClientInfo();
            try
            {
                // 验证输入模型
                if (addUser == null ||
                    string.IsNullOrEmpty(addUser.UserPhone) ||
                    string.IsNullOrEmpty(addUser.UserPwd))
                {
                    return BadRequest(new HttpData { Code = 400, Message = "无效的用户数据" });
                }

                // 设置默认值
                addUser.RainbowId ??= $"Rainbow_{addUser.UserPhone}";
                addUser.UserName ??= $"{addUser.RainbowId}_{DateTime.Now:yyyyMMddHHmmss}";

                // 一次性检查所有唯一性约束
                var existingUser = await _dbContext.Users
                    .Where(u => u.RainbowId == addUser.RainbowId ||
                                u.UserName == addUser.UserName ||
                                u.UserPhone == addUser.UserPhone ||
                                u.UserEmail == addUser.UserEmail)
                    .Select(u => new { u.RainbowId, u.UserName, u.UserPhone, u.UserEmail })
                    .FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    return existingUser.RainbowId == addUser.RainbowId
                        ? BadRequest(new HttpData { Code = 409, Message = "RainbowId已注册" })
                        : existingUser.UserName == addUser.UserName
                            ? BadRequest(new HttpData { Code = 409, Message = "用户名已注册" })
                            : existingUser.UserPhone == addUser.UserPhone
                                ? BadRequest(new HttpData { Code = 409, Message = "用户电话已注册" })
                                : BadRequest(new HttpData { Code = 409, Message = "用户邮箱已注册" });
                }

                var newUser = new User
                {
                    RainbowId = addUser.RainbowId,
                    UserName = addUser.UserName,
                    UserPwd = addUser.UserPwd,
                    UserPhone = addUser.UserPhone,
                    UserImg = addUser.UserImg,
                    Question = addUser.Question,
                    SecPwd = addUser.SecPwd,
                    UserEmail = addUser.UserEmail // 直接赋值，EF会忽略null值
                };

                _dbContext.Users.Add(newUser);
                await _dbContext.SaveChangesAsync();

                _logService.LogLogin(new LogLogIn
                {
                    Token = JsonSerializer.Serialize(new { newUser.UserName, newUser.RainbowId }),
                    Ip = ip ?? "",
                    DateTime = DateTime.Now,
                    Message = "用户注册成功",
                    RequestBody = JsonSerializer.Serialize(addUser)
                });

                return Ok(new HttpData
                {
                    Code = 201,
                    Data = newUser,
                    Message = "用户注册成功"
                });
            }
            catch (DbUpdateException ex)
            {
                LogError(nameof(NewUserRegistration), "", JsonSerializer.Serialize(addUser), ex);
                return StatusCode(500, new HttpData
                {
                    Code = 500,
                    Message = "数据库更新失败"
                });
            }
            catch (Exception ex)
            {
                LogError(nameof(NewUserRegistration), "", JsonSerializer.Serialize(addUser), ex);
                return StatusCode(500, new HttpData
                {
                    Code = 500,
                    Message = "服务器内部错误"
                });
            }
        }

        /// <summary>
        /// 用户登录接口
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        [HttpPost("UserLogin")]
        public async Task<IActionResult> UserLogin([FromBody] LoginDto userLogin)
        {
            var (ip, agent) = GetClientInfo();
            try
            {
                if (userLogin == null ||
                    string.IsNullOrEmpty(userLogin.Identifier) ||
                    string.IsNullOrEmpty(userLogin.Password) ||
                    string.IsNullOrEmpty(userLogin.Type))
                {
                    return Ok(new HttpData { Code = 400, Message = "无效的登录数据" });
                }

                var user = await _authService.FindUserByLoginTypeAsync(
                    userLogin.Type, userLogin.Identifier, userLogin.Password);

                if (user == null)
                {
                    return Ok(new HttpData
                    {
                        Code = 403,
                        Message = "用户名或密码错误"
                    });
                }

                var (isBlacklisted, expireTime) = await _authService.IsBlacklistedAsync(user.UserId.ToString());
                if (isBlacklisted)
                {
                    var message = expireTime.HasValue
                        ? $"账号被封禁至{expireTime.Value:yyyy-MM-dd HH:mm:ss}"
                        : "账号被永久封禁";
                    return Ok(new HttpData
                    {
                        Code = 403,
                        Message = message
                    });
                }

                var jwtToken = _jwtService.GenerateEncodedTokenAsync(user.RainbowId, user);
                var sessionSaved = await _authService.SaveLoginSessionAsync(
                    user, jwtToken, agent, ip ?? "");

                if (sessionSaved == 0)
                {
                    return StatusCode(500, new HttpData
                    {
                        Code = 500,
                        Message = "登录会话保存失败"
                    });
                }

                _logService.LogLogin(new LogLogIn
                {
                    Token = JsonSerializer.Serialize(new { user.UserName, user.RainbowId }),
                    Ip = ip ?? "",
                    DateTime = DateTime.Now,
                    Message = "用户登录成功",
                    RequestBody = JsonSerializer.Serialize(userLogin)
                });
                return Ok(new HttpData
                {
                    Code = 200,
                    Data = new
                    {
                        jwtTokenResult = new
                        {
                            jwtToken.Access_token,
                            jwtToken.Token_type,
                            jwtToken.Refresh_token,
                            jwtToken.Expires_in
                        }
                    },
                    Message = "登录成功"
                });
            }
            catch (Exception ex)
            {
                LogError(nameof(UserLogin), "", JsonSerializer.Serialize(userLogin), ex);
                return StatusCode(500, new HttpData
                {
                    Code = 500,
                    Message = "登录处理失败"
                });
            }
        }

        // TokenToMenuList 和 TokenToInfo 方法类似优化
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

        /// <summary>
        /// 获取用户菜单列表
        /// </summary>
        /// <returns></returns>
        [HttpPost("TokenToMenuList")]
        public async Task<IActionResult> TokenToMenuList()
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
                        Name = nameof(TokenToMenuList),
                        DateTime = DateTime.Now,
                        Message = error
                    });
                    return Ok(new HttpData { Code = 401, Message = error });
                }

                var menuList = _authService.GetMenulist(user.Permissions);
                return Ok(new HttpData
                {
                    Code = 200,
                    Data = new { menuList },
                    Message = "菜单获取成功"
                });
            }
            catch (Exception ex)
            {
                LogError(nameof(TokenToMenuList), "", "", ex);
                return StatusCode(500, new HttpData
                {
                    Code = 500,
                    Message = "菜单获取失败"
                });
            }
        }
        #region 获取用户信息接口
        /// <summary>
        /// 获取用户信息接口
        /// </summary>
        /// <returns></returns>
        [HttpPost("TokenToInfo")]
        public async Task<IActionResult> TokenToInfo()
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
                        Name = nameof(TokenToInfo),
                        DateTime = DateTime.Now,
                        Message = error
                    });
                    return Unauthorized(new HttpData
                    {
                        Code = 401,
                        Message = error
                    });
                }

                var buttonList = await _authService.GetButtonPermissionsAsync(user.RainbowId);
                var roleList = new[] { user.Permissions };

                return Ok(new HttpData
                {
                    Code = 200,
                    Data = new
                    {
                        userInfo = new
                        {
                            user.RainbowId,
                            user.UserName,
                            user.UserImg
                        },
                        buttonList,
                        roleList
                    },
                    Message = "用户信息获取成功"
                });
            }
            catch (Exception ex)
            {
                LogError(nameof(TokenToInfo), "", "", ex);
                return StatusCode(500, new HttpData
                {
                    Code = 500,
                    Message = "用户信息获取失败"
                });
            }
        }
        #endregion

        #region 长token刷新接口
        /// <summary>
        /// 刷新令牌接口
        /// </summary>
        /// <returns></returns>
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var (ip, agent) = GetClientInfo();
            try
            {
                var (login, type) = HttpContext.GetRequestUser();
                if (type != "refresh" || login == null)
                {
                    _logService.LogError(new LogError
                    {
                        Token = JsonSerializer.Serialize(login),
                        Ip = ip ?? "",
                        DeviceInfo = agent,
                        Name = nameof(RefreshToken),
                        DateTime = DateTime.Now,
                        Message = "非法token类型"
                    });
                    return Ok(new HttpData
                    {
                        Code = 403,
                        Message = "无效的刷新令牌"
                    });
                }

                var tokenHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
                var user = await _dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u =>
                        u.RainbowId == login.RainbowId &&
                        u.Permissions == login.Permissions &&
                        u.UserName == login.UserName);

                if (user == null)
                {
                    return Ok(new HttpData
                    {
                        Code = 403,
                        Message = "用户不存在或登录过期"
                    });
                }

                // 检查账号封禁状态
                var (isBlacklisted, expireTime) = await _authService.IsBlacklistedAsync(user.UserId.ToString());
                if (isBlacklisted && (!expireTime.HasValue || expireTime.Value >= DateTime.Now))
                {
                    var message = expireTime.HasValue
                        ? $"账号被封禁至{expireTime.Value:yyyy-MM-dd HH:mm:ss}"
                        : "账号被永久封禁";
                    return BadRequest(new HttpData
                    {
                        Code = 403,
                        Message = message
                    });
                }

                // 验证刷新令牌
                var latestSession = await _dbContext.Loginsessions
                    .Where(ls => ls.UserId == user.UserId)
                    .OrderByDescending(ls => ls.LastActivity)
                    .FirstOrDefaultAsync();

                if (latestSession == null ||
                    !_authService.SecureCompare($"Bearer {latestSession.RefreshToken}", tokenHeader ?? ""))
                {
                    _logService.LogError(new LogError
                    {
                        Token = tokenHeader ?? "",
                        Ip = ip ?? "",
                        DeviceInfo = agent,
                        Name = nameof(RefreshToken),
                        DateTime = DateTime.Now,
                        Message = "刷新令牌不匹配"
                    });
                    return Ok(new HttpData
                    {
                        Code = 403,
                        Message = "无效的刷新令牌"
                    });
                }

                // 生成新令牌
                var jwtToken = _jwtService.GenerateEncodedTokenAsync(user.RainbowId, user);

                // 更新会话
                latestSession.LastActivity = DateTime.Now;
                var sessionSaved = await _authService.SaveLoginSessionAsync(user, jwtToken, agent, ip ?? "");

                if (sessionSaved == 0)
                {
                    return StatusCode(500, new HttpData
                    {
                        Code = 500,
                        Message = "会话更新失败"
                    });
                }

                await _dbContext.SaveChangesAsync();

                _logService.LogLogin(new LogLogIn
                {
                    Token = JsonSerializer.Serialize(new { user.UserName, user.RainbowId }),
                    Ip = ip ?? "",
                    DateTime = DateTime.Now,
                    Message = "令牌刷新成功",
                    RequestBody = JsonSerializer.Serialize(jwtToken)
                });

                return Ok(new HttpData
                {
                    Code = 200,
                    Data = new
                    {
                        jwtTokenResult = new
                        {
                            jwtToken.Access_token,
                            jwtToken.Token_type,
                            jwtToken.Refresh_token,
                            jwtToken.Expires_in
                        }
                    },
                    Message = "令牌刷新成功"
                });
            }
            catch (Exception ex)
            {
                LogError(nameof(RefreshToken), "", "", ex);
                return StatusCode(500, new HttpData
                {
                    Code = 500,
                    Message = "令牌刷新失败"
                });
            }
        }
        #endregion
        ///// <summary>
        ///// 获取全部用户信息
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public IActionResult Get()
        //{
        //    // 获取全部用户信息
        //    var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        //    var users = _dbContext.Users.ToList();
        //    return Ok(new HttpData { Code = 200, Data = new { users, ip }, Message = "获取成功" });
        //}

        //#region 用户注册接口
        ///// <summary>
        ///// 新用户注册接口
        ///// </summary>
        ///// <param name="addUser">用户基础信息</param>
        ///// <returns></returns>
        //[HttpPost("UserRegistration")]
        //public async Task<IActionResult> NewUserRegistration([FromBody] AddUser addUser)
        //{
        //    var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        //    var agent = Request.Headers.UserAgent.ToString();
        //    try
        //    {
        //        // 判断数据是否为空
        //        if (addUser == null || string.IsNullOrEmpty(addUser.UserPhone) || string.IsNullOrEmpty(addUser.UserPwd))
        //        {
        //            return BadRequest(new HttpData { Code = 403, Message = "无效的用户数据" });
        //        }
        //        // 判断RainbowId是否为空 为空则使用Rainbow_+UserPhone作为RainbowId
        //        if (string.IsNullOrEmpty(addUser.RainbowId))
        //        {
        //            addUser.RainbowId = $"Rainbow_{addUser.UserPhone}";
        //        }
        //        // 判断UserName是否为空 为空则使用RainbowId+注册时间作为用户名
        //        if (string.IsNullOrEmpty(addUser.UserName))
        //        {
        //            addUser.UserName = $"{addUser.RainbowId}_{DateTime.Now:yyyyMMddHHmmss}";
        //        }
        //        // 判断RainbowId是否已存在
        //        if (_dbContext.Users.Any(u => u.RainbowId == addUser.RainbowId))
        //        {
        //            return BadRequest(new HttpData { Code = 403, Message = "RainbowId已注册" });
        //        }
        //        // 判断UserName是否已存在
        //        if (_dbContext.Users.Any(u => u.UserName == addUser.UserName))
        //        {
        //            return BadRequest(new HttpData { Code = 403, Message = "用户名已注册" });
        //        }
        //        // 判断UserPhone是否已存在
        //        if (_dbContext.Users.Any(u => u.UserPhone == addUser.UserPhone))
        //        {
        //            return BadRequest(new HttpData { Code = 403, Message = "用户电话已注册" });
        //        }
        //        // 创建新用户对象
        //        var newUser = new User
        //        {
        //            RainbowId = addUser.RainbowId,
        //            UserName = addUser.UserName,
        //            UserPwd = addUser.UserPwd,
        //            UserPhone = addUser.UserPhone,
        //            UserImg = addUser.UserImg,
        //            Question = addUser.Question,
        //            SecPwd = addUser.SecPwd
        //        };
        //        // 判断UserEmail是否不为空 不为空就添加到newUser中 
        //        if (!string.IsNullOrEmpty(addUser.UserEmail))
        //        {
        //            newUser.UserEmail = addUser.UserEmail;
        //        }
        //        // 判断UserEmail是否已存在
        //        if (_dbContext.Users.Any(u => u.UserEmail == addUser.UserEmail))
        //        {
        //            return BadRequest(new HttpData { Code = 403, Message = "用户邮箱已注册" });
        //        }
        //        _dbContext.Users.Add(newUser);
        //        var userID = await _dbContext.SaveChangesAsync();
        //        var user = _dbContext.Users.FirstOrDefault(u => u.UserId == userID);
        //        _logService.LogLogin(new LogLogIn
        //        {
        //            Token = System.Text.Json.JsonSerializer.Serialize(user),
        //            Ip = ip ?? "",
        //            DateTime = DateTime.Now,
        //            Message = "用户登录成功",
        //            RequestBody = System.Text.Json.JsonSerializer.Serialize(addUser)
        //        });

        //        return Ok(new HttpData { Code = 200, Data = user, Message = "用户添加成功" });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logService.LogError(new LogError { Token = "", Ip = ip ?? "", DeviceInfo = agent, Name = "AuthController.NewUserRegistration", DateTime = DateTime.Now, Message = ex.Message.ToString(), RequestBody = System.Text.Json.JsonSerializer.Serialize(addUser) });
        //        return BadRequest(new HttpData { Code = 500, Message = $"服务器出错，稍后重试" });
        //    }
        //}
        //#endregion

        //#region 用户登录接口
        ///// <summary>
        ///// 用户登录接口
        ///// </summary>
        ///// <param name="userLogin">登录信息</param>
        ///// <returns></returns>
        //[HttpPost("UserLogin")]
        //public async Task<IActionResult> UserLogin([FromBody] LoginDto userLogin)
        //{
        //    var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        //    var agent = Request.Headers.UserAgent.ToString();
        //    try
        //    {
        //        // 判断数据是否为空
        //        if (userLogin == null || string.IsNullOrEmpty(userLogin.Identifier) || string.IsNullOrEmpty(userLogin.Password) || string.IsNullOrEmpty(userLogin.Type))
        //        {
        //            return BadRequest(new HttpData { Code = 403, Message = "无效的用户数据" });
        //        }
        //        // 查询用户信息
        //        var user = await _authService.FindUserByLoginTypeAsync(loginType: userLogin.Type, identifier: userLogin.Identifier, password: userLogin.Password);
        //        if (user == null)
        //        {
        //            return BadRequest(new HttpData { Code = 403, Message = "用户名或密码错误" });
        //        }
        //        var (isBlacklisted, expireTime) = await _authService.IsBlacklistedAsync(user.UserId.ToString());
        //        if (expireTime.HasValue && expireTime.Value < DateTime.Now) isBlacklisted = false; // 如果封禁时间已过，则不再视为封禁状态 当expireTime为null时，表示用户被永久封禁
        //        if (isBlacklisted) return Ok(new HttpData { Code = 401, Message = expireTime.HasValue ? $"账号被封禁至{expireTime.Value:yyyy-MM-dd HH:mm:ss}" : "账号被永久封禁" });

        //        // 生成JWT令牌
        //        var jwtToken = _jwtService.GenerateEncodedTokenAsync(user.RainbowId, user);
        //        var count = await _authService.SaveLoginSessionAsync(user, jwtToken, agent, ip ?? "");
        //        if (count == 0)
        //        {
        //            return BadRequest(new HttpData { Code = 500, Message = "服务器出错，稍后重试" });
        //        }
        //        _logService.LogLogin(new LogLogIn
        //        {
        //            Token = System.Text.Json.JsonSerializer.Serialize(new { user.UserName, user.RainbowId }),
        //            Ip = ip ?? "",
        //            DateTime = DateTime.Now,
        //            Message = "用户登录成功",
        //            RequestBody = System.Text.Json.JsonSerializer.Serialize(userLogin)
        //        });
        //        return Ok(new HttpData { Code = 200, Data = new { jwtTokenResult = new { jwtToken.Access_token, jwtToken.Token_type, jwtToken.Refresh_token, jwtToken.Expires_in } }, Message = "登录成功" });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logService.LogError(new LogError { Token = "", Ip = ip ?? "", DeviceInfo = agent, Name = "AuthController.UserLogin", DateTime = DateTime.Now, Message = ex.Message.ToString(), RequestBody = System.Text.Json.JsonSerializer.Serialize(userLogin) });
        //        return BadRequest(new HttpData { Code = 500, Message = $"服务器出错，稍后重试" });
        //    }
        //}
        //#endregion

        //#region 获取用户菜单接口
        ///// <summary>
        ///// 获取用户菜单列表
        ///// </summary>
        ///// <returns>菜单列表</returns>
        //[HttpPost("TokenToMenuList")]
        //public ActionResult TokenToMenuList()
        //{
        //    var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        //    var agent = Request.Headers.UserAgent.ToString();
        //    try
        //    {
        //        var (login, type) = HttpContext.GetRequestUser();
        //        if (type == "access")
        //        {
        //            if (login == null) return Ok(new HttpData { Code = 401, Message = "登录过期,请重新登陆" });
        //            var user = _dbContext.Users.FirstOrDefault(u => u.RainbowId == login.RainbowId && u.Permissions == login.Permissions && u.UserName == login.UserName);
        //            if (user == null) return Ok(new HttpData { Code = 401, Message = "登录过期,请重新登陆" });
        //            var MenuList = _authService.GetMenulist(user.Permissions);
        //            return Ok(new HttpData { Code = 200, Message = "获取成功!", Data = new { MenuList } });
        //        }
        //        else
        //        {
        //            _logService.LogError(new LogError { Token = System.Text.Json.JsonSerializer.Serialize(login), Ip = ip ?? "", DeviceInfo = agent, Name = "AuthController.TokenToMenuList", DateTime = DateTime.Now, Message = "非法token" });
        //            return Ok(new HttpData { Code = 401, Message = "非法token" });
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        _logService.LogError(new LogError { Token = "", Ip = ip ?? "", DeviceInfo = agent, Name = "AuthController.TokenToMenuList", DateTime = DateTime.Now, Message = ex.Message.ToString() });
        //        return BadRequest(new HttpData
        //        {
        //            Code = 500,
        //            Message = "获取失败,请稍后重试"
        //        });
        //    }
        //}
        //#endregion

        //#region 获取用户信息接口
        ///// <summary>
        ///// 获取用户信息
        ///// </summary>
        ///// <returns>用户信息</returns>
        //[HttpPost("TokenToInfo")]
        //public async Task<ActionResult> TokenToInfo()
        //{
        //    var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        //    var agent = Request.Headers.UserAgent.ToString();
        //    try
        //    {
        //        var (login, type) = HttpContext.GetRequestUser();
        //        if (type == "access")
        //        {
        //            if (login == null) return Ok(new HttpData { Code = 401, Message = "登录过期,请重新登陆" });
        //            var user = _dbContext.Users.FirstOrDefault(u => u.RainbowId == login.RainbowId && u.Permissions == login.Permissions && u.UserName == login.UserName);
        //            if (user == null) return Ok(new HttpData { Code = 401, Message = "登录过期,请重新登陆" });
        //            string[] buttonList = await _authService.GetButtonPermissionsAsync(user.RainbowId);
        //            string[] roleList = [user.Permissions];
        //            return Ok(new HttpData { Code = 200, Message = "获取成功!", Data = new { userInfo = new { user.RainbowId, user.UserName, user.UserImg }, buttonList, roleList } });
        //        }
        //        else
        //        {
        //            _logService.LogError(new LogError { Token = System.Text.Json.JsonSerializer.Serialize(login), Ip = ip ?? "", DeviceInfo = agent, Name = "AuthController.TokenToInfo", DateTime = DateTime.Now, Message = "非法token" });
        //            return Ok(new HttpData { Code = 401, Message = "非法token" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logService.LogError(new LogError { Token = "", Ip = ip ?? "", DeviceInfo = agent, Name = "AuthController.TokenToInfo", DateTime = DateTime.Now, Message = ex.Message.ToString() });
        //        return BadRequest(new HttpData
        //        {
        //            Code = 500,
        //            Message = ex.ToString()
        //        });
        //    }

        //}
        //#endregion

        //#region 长token刷新接口
        ///// <summary>
        ///// 长token刷新接口
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost("RefreshToken")]
        //public async Task<IActionResult> RefreshToken()
        //{
        //    var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        //    var agent = Request.Headers.UserAgent.ToString();
        //    try
        //    {
        //        var (login, type) = HttpContext.GetRequestUser();
        //        if (type == "refresh")
        //        {
        //            var token = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        //            if (login == null) return Ok(new HttpData { Code = 403, Message = "登录异常,请重新登陆" });
        //            var user = _dbContext.Users.FirstOrDefault(u => u.RainbowId == login.RainbowId && u.Permissions == login.Permissions && u.UserName == login.UserName);
        //            if (user == null) return Ok(new HttpData { Code = 403, Message = "登录异常,请重新登陆" });
        //            var latestSession = await _dbContext.Loginsessions.Where(ls => ls.UserId == user.UserId).OrderByDescending(ls => ls.LastActivity).FirstOrDefaultAsync();

        //            // 检查记录是否存在且 RefreshToken 匹配
        //            if (latestSession != null)
        //            {
        //                // 安全比较 RefreshToken（避免时序攻击）
        //                if (!_authService.SecureCompare("Bearer " + latestSession.RefreshToken, token ?? ""))
        //                {
        //                    _logService.LogError(new LogError { Token = token ?? "", Ip = ip ?? "", DeviceInfo = agent, Name = "AuthController.RefreshToken", DateTime = DateTime.Now, Message = "Bearer " + latestSession.RefreshToken });
        //                    return Ok(new HttpData { Code = 403, Message = "非法token" });
        //                }
        //            }
        //            var (isBlacklisted, expireTime) = await _authService.IsBlacklistedAsync(user.UserId.ToString());
        //            if (expireTime.HasValue && expireTime.Value < DateTime.Now) isBlacklisted = false; // 如果封禁时间已过，则不再视为封禁状态 当expireTime为null时，表示用户被永久封禁
        //            if (isBlacklisted) return Ok(new HttpData { Code = 401, Message = expireTime.HasValue ? $"账号被封禁至{expireTime.Value:yyyy-MM-dd HH:mm:ss}" : "账号被永久封禁" });
        //            // 生成新的JWT令牌
        //            var jwtToken = _jwtService.GenerateEncodedTokenAsync(user.RainbowId, user);
        //            // 查询用户上一次的登录会话并更改最后会话时间为当前时间
        //            // 直接更新而不先查询（适用于简单更新）//最新一条
        //            var affectedRows = await _dbContext.Loginsessions.Where(ls => ls.UserId == user.UserId).OrderByDescending(ls => ls.LastActivity).Take(1).ExecuteUpdateAsync(setters =>
        //                    setters.SetProperty(ls => ls.LastActivity, DateTime.Now));

        //            // affectedRows 返回更新的行数（0或1）
        //            var count = await _authService.SaveLoginSessionAsync(user, jwtToken, agent, ip ?? "");
        //            if (count == 0)
        //            {
        //                return BadRequest(new HttpData { Code = 500, Message = "服务器出错，稍后重试" });
        //            }
        //            _logService.LogLogin(new LogLogIn
        //            {
        //                Token = System.Text.Json.JsonSerializer.Serialize(new { user.UserName, user.RainbowId }),
        //                Ip = ip ?? "",
        //                DateTime = DateTime.Now,
        //                Message = "用户刷新Token成功",
        //                RequestBody = System.Text.Json.JsonSerializer.Serialize(jwtToken),
        //            });
        //            return Ok(new HttpData { Code = 200, Data = new { jwtTokenResult = new { jwtToken.Access_token, jwtToken.Token_type, jwtToken.Refresh_token, jwtToken.Expires_in } }, Message = "登录成功" });
        //        }
        //        else
        //        {
        //            _logService.LogError(new LogError { Token = System.Text.Json.JsonSerializer.Serialize(login), Ip = ip ?? "", DeviceInfo = agent, Name = "AuthController.RefreshToken", DateTime = DateTime.Now, Message = "非法token" });
        //            return Ok(new HttpData { Code = 403, Message = "非法token" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logService.LogError(new LogError { Token = "", Ip = ip ?? "", DeviceInfo = agent, Name = "AuthController.RefreshToken", DateTime = DateTime.Now, Message = ex.Message.ToString() });
        //        return BadRequest(new HttpData { Code = 500, Message = $"服务器出错，稍后重试" });
        //    }
        //}
        //#endregion
    }
}
