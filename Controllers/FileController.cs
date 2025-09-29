using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MeowMemoirsAPI.Interfaces;
using MeowMemoirsAPI.Models.Http;
using MeowMemoirsAPI.Models.Log;
using System.IO;

namespace MeowMemoirsAPI.Controllers
{
    /// <summary>
    /// 文件控制器
    /// </summary>
    /// <param name="fileService"></param>
    /// <param name="httpContextAccessor"></param>
    /// <param name="logService"></param>
    [Route("MeowMemoirs/[controller]")]
    [ApiController]
    public class FileController(IFileService fileService, IHttpContextAccessor httpContextAccessor, ILogService logService) : ControllerBase
    {
        private readonly IFileService _fileService = fileService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogService _logService = logService;

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [HttpGet("DownloadFile")]
        public async Task<IActionResult> DownloadFile(string path)
        {
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            var agent = Request.Headers.UserAgent.ToString();
            try
            {
                var (fileType, data, fileName) = await _fileService.DownloadFiles(path);
                return File(data, fileType, fileName);
            }
            catch (Exception ex)
            {
                _logService.LogError(new LogError { Token = "", Ip = ip ?? "", DeviceInfo = agent, Name = "FileController.DownloadFile", DateTime = DateTime.Now, Message = ex.Message.ToString(), RequestBody = System.Text.Json.JsonSerializer.Serialize(path) });
                return BadRequest(new HttpData { Code = 500, Message = "文件获取失败" });
            }
        }
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="files"></param>
        /// <param name="subDirectory"></param>
        /// <returns></returns>
        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile(List<IFormFile> files, string subDirectory = "")
        {
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            var agent = Request.Headers.UserAgent.ToString();
            try
            {
                var (count, size) = await _fileService.UploadFile(files, subDirectory);
                return Ok(new HttpData { Code = 200, Message = "文件上传成功", Data = new { count, size } });
            }
            catch (Exception ex)
            {
                _logService.LogError(new LogError { Token = "", Ip = ip ?? "", DeviceInfo = agent, Name = "FileController.UploadFile", DateTime = DateTime.Now, Message = ex.Message.ToString(), RequestBody = System.Text.Json.JsonSerializer.Serialize(subDirectory) });
                return BadRequest(new HttpData { Code = 500, Message = "文件上传失败" });
            }
        }
    }
}
