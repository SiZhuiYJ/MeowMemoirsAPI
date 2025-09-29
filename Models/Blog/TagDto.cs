namespace MeowMemoirsAPI.Models.Blog
{
    /// <summary>
    /// 标签数据传输对象
    /// </summary>
    public class TagDto
    {
        /// <summary>
        /// 标签名称
        /// </summary>
        public string TagName { get; set; } = null!;

        /// <summary>
        /// 标签颜色
        /// </summary>
        public string TagColor { get; set; } = null!;

        /// <summary>
        /// 标签图标
        /// </summary>
        public string TagIcon { get; set; } = null!;

        /// <summary>
        /// 标签描述
        /// </summary>
        public string TagDescription { get; set; } = null!;
    }
}
