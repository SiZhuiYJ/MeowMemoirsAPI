namespace MeowMemoirsAPI.Models.Blog
{
    /// <summary>
    /// 博客数据传输对象
    /// </summary>
    public class BlogDto
    {
        /// <summary>
        /// 博客ID
        /// </summary>
        public int Id { get; set; } = 0;
        /// <summary>
        /// 博客标题
        /// </summary>
        public required string Title { get; set; } = string.Empty;

        /// <summary>
        /// 博客封面内容（100字以内）
        /// </summary>
        public required string CoverContent { get; set; } = string.Empty;
        /// <summary>
        /// 博客内容
        /// </summary>
        public required string Content { get; set; } = string.Empty;
        /// <summary>
        /// 博客标签（10个最多）
        /// </summary>
        public required string Tags { get; set; } = string.Empty;
    }
}
