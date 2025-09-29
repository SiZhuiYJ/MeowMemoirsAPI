using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using MeowMemoirsAPI.Interfaces;
using MeowMemoirsAPI.Models.Folder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using System.IO.Compression;

namespace MeowMemoirsAPI.Services
{
    /// <summary>
    /// 文件服务
    /// </summary>
    public class FileService : IFileService
    {
        readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogService _logService;
        private readonly string _originalServer;
        private readonly string _fileServer;
        private readonly string _errorImagePath;

        /// <summary>
        /// 文件服务构造函数
        /// </summary>
        /// <param name="logService"></param>
        /// <param name="configuration"></param>
        /// <param name="httpContextAccessor"></param>
        public FileService(ILogService logService, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _logService = logService;
            _originalServer = configuration["FileStorage:OriginalPath"] ?? "/www/wwwroot/www.meowmemoirs.cn.api/UserFiles/Original/";
            _fileServer = configuration["FileStorage:BasePath"] ?? "/www/wwwroot/www.meowmemoirs.cn.api/UserFiles/";
            _errorImagePath = Path.Combine("MapStorage", "indigenous", "Error.webp");

            // 确保目录存在
            Directory.CreateDirectory(_originalServer);
            Directory.CreateDirectory(_fileServer);
        }

        #region 文件操作

        /// <summary>
        /// 上传文件并转换为WebP格式
        /// </summary>
        public async Task<(int count, long size)> UploadFile(List<IFormFile> files, string subDirectory)
        {
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            if (files == null || files.Count == 0)
                return (0, 0);

            try
            {
                subDirectory = SanitizePath(subDirectory);
                var targetPath = Path.Combine(_originalServer, subDirectory);
                Directory.CreateDirectory(targetPath);

                long totalSize = 0;
                int successCount = 0;

                foreach (var file in files)
                {
                    if (file.Length <= 0) continue;

                    // 保存原始文件
                    var safeFileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(targetPath, safeFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                        totalSize += file.Length;
                    }

                    // 仅处理图片文件
                    if (IsImageFile(filePath))
                    {
                        await ConvertToWebp(filePath, targetPath);
                    }
                    successCount++;
                }

                return (successCount, totalSize);
            }
            catch (Exception ex)
            {
                _logService.LogError(new Models.Log.LogError
                {
                    Token = "",
                    Ip = ip ?? "",
                    Name = "FileService.UploadFile",
                    DateTime = DateTime.Now,
                    Message = ex.Message,
                });
                return (0, 0);
            }
        }

        /// <summary>
        /// 下载文件或目录
        /// </summary>
        public async Task<(string fileType, byte[] data, string fileName)> DownloadFiles(string path)
        {
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            try
            {
                var fullPath = Path.Combine(_originalServer, SanitizePath(path));

                // 处理单个文件下载
                if (File.Exists(fullPath))
                {
                    var fileData = await File.ReadAllBytesAsync(fullPath);
                    var mimeType = GetMimeType(fullPath);
                    return (mimeType, fileData, Path.GetFileName(fullPath));
                }

                // 处理目录下载
                if (Directory.Exists(fullPath))
                {
                    using var memoryStream = new MemoryStream();
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var file in Directory.GetFiles(fullPath))
                        {
                            var entry = archive.CreateEntry(Path.GetFileName(file));
                            using var entryStream = entry.Open();
                            var fileData = await File.ReadAllBytesAsync(file);
                            await entryStream.WriteAsync(fileData);
                        }
                    }
                    return ("application/zip", memoryStream.ToArray(), $"{Path.GetFileName(fullPath)}.zip");
                }

                // 文件不存在时返回错误图片
                var errorPath = Path.Combine(_originalServer, _errorImagePath);
                if (!File.Exists(errorPath)) throw new FileNotFoundException("文件未找到");

                return ("image/webp", await File.ReadAllBytesAsync(errorPath), "Error.webp");
            }
            catch (Exception ex)
            {
                _logService.LogError(new Models.Log.LogError
                {
                    Token = "",
                    Ip = ip ?? "",
                    Name = "FileService.UploadFile",
                    DateTime = DateTime.Now,
                    Message = ex.Message,
                });
                throw;
            }
        }

        /// <summary>
        /// 删除文件或目录
        /// </summary>
        public bool DeleteFile(string path)
        {
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            try
            {
                var fullPath = Path.Combine(_originalServer, SanitizePath(path));

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }

                if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath, true);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logService.LogError(new Models.Log.LogError
                {
                    Token = "",
                    Ip = ip ?? "",
                    Name = "FileService.UploadFile",
                    DateTime = DateTime.Now,
                    Message = ex.Message,
                });
                return false;
            }
        }

        #endregion

        #region 目录操作

        /// <summary>
        /// 创建目录
        /// </summary>
        public async Task<bool> CreateDirectory(string path)
        {
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            return await Task.Run(() =>
            {
                try
                {
                    var fullPath = Path.Combine(_originalServer, SanitizePath(path));
                    if (Directory.Exists(fullPath)) return false;

                    Directory.CreateDirectory(fullPath);
                    return true;
                }
                catch (Exception ex)
                {
                    _logService.LogError(new Models.Log.LogError
                    {
                        Token = "",
                        Ip = ip ?? "",
                        Name = "FileService.UploadFile",
                        DateTime = DateTime.Now,
                        Message = ex.Message,
                    });
                    return false;
                }
            });
        }

        /// <summary>
        /// 获取目录内容
        /// </summary>
        public async Task<List<Folder>> ListDirectory(string path)
        {
            return await Task.Run(() =>
            {
                var fullPath = Path.Combine(_originalServer, SanitizePath(path));
                var contents = new List<Folder>();

                if (!Directory.Exists(fullPath))
                    return contents;

                // 添加子目录
                foreach (var dir in Directory.GetDirectories(fullPath))
                {
                    var dirInfo = new DirectoryInfo(dir);
                    contents.Add(new Folder
                    {
                        Name = dirInfo.Name,
                        Size = 0,
                        CreationTime = dirInfo.CreationTime,
                        LastWriteTime = dirInfo.LastWriteTime,
                        Attributes = dirInfo.Attributes.ToString(),
                        Type = ""
                    });
                }

                // 添加文件
                foreach (var file in Directory.GetFiles(fullPath))
                {
                    var fileInfo = new FileInfo(file);
                    contents.Add(new Folder
                    {
                        Name = fileInfo.Name,
                        Size = fileInfo.Length,
                        CreationTime = fileInfo.CreationTime,
                        LastWriteTime = fileInfo.LastWriteTime,
                        Attributes = fileInfo.Attributes.ToString(),
                        Type = GetMimeType(file)
                    });
                }

                return contents;
            });
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 将图片转换为WebP格式
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <returns></returns>
        private async Task ConvertToWebp(string sourcePath, string targetPath)
        {
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            try
            {
                var webpPath = Path.Combine(
                    targetPath,
                    $"{Path.GetFileNameWithoutExtension(sourcePath)}.webp");

                using var image = await Image.LoadAsync(sourcePath);
                await image.SaveAsync(webpPath, new WebpEncoder { Quality = 100 });
            }
            catch (Exception ex)
            {
                _logService.LogError(new Models.Log.LogError
                {
                    Token = "",
                    Ip = ip ?? "",
                    Name = "FileService.UploadFile",
                    DateTime = DateTime.Now,
                    Message = ex.Message,
                });
            }
        }

        /// <summary>
        /// 检查文件是否为图片类型
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool IsImageFile(string path)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
            return imageExtensions.Contains(Path.GetExtension(path).ToLower());
        }

        /// <summary>
        /// 获取文件的MIME类型
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string GetMimeType(string fileName)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string? contentType);
            return contentType ?? "application/octet-stream";
        }

        /// <summary>
        /// 将路径转换为相对路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string SanitizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            return Path.GetRelativePath(".", path);
        }

        #endregion
    }
}
