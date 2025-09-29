using MeowMemoirsAPI.Models.Folder;

namespace MeowMemoirsAPI.Interfaces
{
    /// <summary>
    /// 文件服务接口
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<(string fileType, byte[] data, string fileName)> DownloadFiles(string path);

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="files"></param>
        /// <param name="subDirectory"></param>
        /// <returns></returns>
        Task<(int count, long size)> UploadFile(List<IFormFile> files, string subDirectory);

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool DeleteFile(string path);

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<bool> CreateDirectory(string path);

        /// <summary>
        /// 列出目录下的文件和子目录
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<List<Folder>> ListDirectory(string path);
    }
}
