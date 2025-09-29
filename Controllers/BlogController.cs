using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeowMemoirsAPI.Interfaces;
using MeowMemoirsAPI.Middleware.auth;
using MeowMemoirsAPI.Models.Blog;
using MeowMemoirsAPI.Models.DataBaseContext;
using MeowMemoirsAPI.Models.Http;
using MeowMemoirsAPI.Models.Log;
using System.Text.Json;

namespace MeowMemoirsAPI.Controllers
{
    /// <summary>
    /// 博客控制器
    /// </summary>
    /// <param name="logService"></param>
    /// <param name="authService"></param>
    /// <param name="httpContextAccessor"></param>
    /// <param name="DbContext"></param>
    [Route("MeowMemoirs/[controller]")]
    [ApiController]
    public class BlogController(ILogService logService, IAuthService authService, IHttpContextAccessor httpContextAccessor, MyRainbowContext DbContext) : ControllerBase
    {
        private readonly ILogService _logService = logService;
        private readonly IAuthService _authService = authService;
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

        #region 获取所有博客列表
        /// <summary>
        /// 获取所有博客
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAllBlogs")]
        public async Task<IActionResult> GetAllBlogs()
        {
            var (ip, agent) = GetClientInfo();
            try
            {
                var blogs = await _dbContext.Blogs.Select(b => new { b.Id, b.Title, b.CoverContent, b.CreatedAt, b.UpdatedAt, b.Tags }).ToListAsync();
                return Ok(new HttpData() { Code = 200, Data = new { blogs }, Message = "获取成功" });
            }
            catch (Exception ex)
            {
                _logService.LogError(new LogError { Token = "", Ip = ip ?? "", DeviceInfo = agent ?? "", Name = "BlogController.GetAllBlogs", DateTime = DateTime.Now, Message = ex.Message, RequestBody = "" });
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion

        #region 获取所有博客标签
        /// <summary>
        /// 获取所有博客标签
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetBlogTags")]
        public async Task<IActionResult> GetBlogTags()
        {
            var (ip, agent) = GetClientInfo();
            try
            {
                var tags = await _dbContext.Blogtags.Where(b => b.TagStatus == 1).Select(b => new { b.TagId, b.TagName, b.TagColor }).Distinct().ToListAsync();
                return Ok(new HttpData { Code = 200, Data = new { tags }, Message = "获取成功" });
            }
            catch (Exception ex)
            {
                _logService.LogError(new LogError { Token = "", Ip = ip ?? "", DeviceInfo = agent ?? "", Name = "BlogController.GetBlogTags", DateTime = DateTime.Now, Message = ex.Message, RequestBody = "" });
                return StatusCode(500, "Internal server error");
            }
        }

        #endregion

        #region 获取指定博客
        /// <summary>
        /// 获取指定ID的博客
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetBlogById/{id}")]
        public async Task<IActionResult> GetBlogById(int id)
        {
            var (ip, agent) = GetClientInfo();
            try
            {
                var blog = await _dbContext.Blogs.FindAsync(id);
                if (blog == null)
                {
                    return NotFound(new HttpData { Code = 404, Message = "博客未找到" });
                }
                return Ok(new HttpData { Code = 200, Data = new { blog }, Message = "获取成功" });
            }
            catch (Exception ex)
            {
                _logService.LogError(new LogError { Token = "", Ip = ip ?? "", DeviceInfo = agent ?? "", Name = "BlogController.GetBlogById", DateTime = DateTime.Now, Message = ex.Message, RequestBody = id.ToString() });
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion

        #region 操作博客
        /// <summary>
        /// 上传博客
        /// </summary>
        /// <param name="blog"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        [HttpPost("UploadBlog/{operation}")]
        public async Task<IActionResult> UploadBlog([FromBody] BlogDto blog, string operation)
        {
            var (ip, agent) = GetClientInfo();
            try
            {
                var newBlog = new Blog { };
                if (blog == null || string.IsNullOrEmpty(blog.Title) || string.IsNullOrEmpty(blog.Content))
                {
                    return BadRequest(new HttpData { Code = 400, Message = "博客内容不能为空" });
                }
                var (user, error) = await ValidateAccessToken();
                if (user == null)
                {
                    _logService.LogError(new LogError { Token = "", Ip = ip ?? "", DeviceInfo = agent ?? "", Name = "BlogController.UploadBlog", DateTime = DateTime.Now, Message = error, RequestBody = JsonSerializer.Serialize(blog) });
                    return Unauthorized(new HttpData { Code = 401, Message = error });
                }
                if (operation != "add" && operation != "update")
                {
                    return BadRequest(new HttpData { Code = 400, Message = "非法操作" });
                }
                else if (operation == "add")
                {
                    var addBlog = new Blog
                    {
                        Title = blog.Title,
                        Content = blog.Content,
                        CoverContent = blog.CoverContent,
                        UserId = user.UserId,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _dbContext.Blogs.Add(addBlog);
                    var context = await _dbContext.SaveChangesAsync();
                    if (context <= 0)
                    {
                        return BadRequest(new HttpData { Code = 400, Message = "博客上传失败" });
                    }
                    else
                    {
                        newBlog = addBlog;
                    }
                }
                else if (operation == "update")
                {
                    var existingBlog = await _dbContext.Blogs.FindAsync(blog.Id);
                    if (existingBlog == null)
                    {
                        return NotFound(new HttpData { Code = 404, Message = "博客未找到" });
                    }
                    existingBlog.Title = blog.Title;
                    existingBlog.Content = blog.Content;
                    existingBlog.UpdatedAt = DateTime.Now;
                    _dbContext.Blogs.Update(existingBlog);
                    var context = await _dbContext.SaveChangesAsync();
                    if (context <= 0)
                    {
                        return BadRequest(new HttpData { Code = 400, Message = "博客更新失败" });
                    }
                    else
                    {
                        newBlog = await _dbContext.Blogs.FindAsync(existingBlog.Id);
                    }
                }

                return Ok(new HttpData { Code = 200, Message = "博客上传成功", Data = new { blog = newBlog } });
            }
            catch (Exception ex)
            {
                _logService.LogError(new LogError { Token = "", Ip = ip ?? "", DeviceInfo = agent ?? "", Name = "BlogController.UploadBlog", DateTime = DateTime.Now, Message = ex.Message, RequestBody = JsonSerializer.Serialize(blog) });
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion

        #region 添加博客标签
        /// <summary>
        /// 添加博客标签
        /// </summary>
        /// <param name="blogTag"></param>
        /// <param name="tagColor"></param>
        /// <returns></returns>
        [HttpPost("AddBlogTag")]
        public async Task<IActionResult> AddBlogTag([FromBody] TagDto blogTag)
        {
            var (ip, agent) = GetClientInfo();
            try
            {
                if (blogTag == null || string.IsNullOrEmpty(blogTag.TagName))
                {
                    return BadRequest(new HttpData { Code = 400, Message = "标签内容不能为空" });
                }
                var (user, error) = await ValidateAccessToken();
                if (user == null)
                {
                    _logService.LogError(new LogError { Token = "", Ip = ip ?? "", DeviceInfo = agent ?? "", Name = "BlogController.AddBlogTag", DateTime = DateTime.Now, Message = error, RequestBody = JsonSerializer.Serialize(blogTag) });
                    return Unauthorized(new HttpData { Code = 401, Message = error });
                }
                var newTag = new Blogtag
                {
                    TagName = blogTag.TagName,
                    TagColor = blogTag.TagColor,
                    TagIcon = blogTag.TagIcon,
                    TagDescription = blogTag.TagDescription,
                    UserId = user.UserId,
                    TagStatus = 1,
                };
                _dbContext.Blogtags.Add(newTag);
                await _dbContext.SaveChangesAsync();
                return Ok(new HttpData { Code = 200, Message = "标签添加成功", Data = new { tagId = newTag.TagId } });
            }
            catch (Exception ex)
            {
                _logService.LogError(new LogError { Token = "", Ip = ip ?? "", DeviceInfo = agent ?? "", Name = "BlogController.AddBlogTag", DateTime = DateTime.Now, Message = ex.Message, RequestBody = JsonSerializer.Serialize(blogTag) });
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion
    }
}
