using System.IO;

namespace MeowMemoirsAPI.Models.Folder
{
    /// <summary>
    /// 文件夹模型
    /// </summary>
    public class Folder
    {
        /// <summary>
        /// 文件或目录名称
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// 文件或目录大小（字节）
        /// </summary>
        public long Size { get; set; }
        /// <summary>
        /// 文件或目录创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }
        /// <summary>
        /// 文件或目录最后修改时间
        /// </summary>
        public DateTime LastWriteTime { get; set; }
        /// <summary>
        /// 文件或目录属性
        /// </summary>
        public required string Attributes { get; set; }
        /// <summary>
        /// 文件或目录类型（例如：文件、目录）
        /// </summary>
        public required string Type { get; set; }

    }
}
