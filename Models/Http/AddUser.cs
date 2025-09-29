namespace MeowMemoirsAPI.Models.Http
{
    /// <summary>
    /// 添加用户请求模型
    /// </summary>
    public class AddUser
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string? RainbowId { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string? UserName { get; set; }
        /// <summary>
        /// 用户密码
        /// </summary>
        public required string UserPwd { get; set; }
        /// <summary>
        /// 用户电话
        /// </summary>
        public required string UserPhone { get; set; }
        /// <summary>
        /// 用户邮箱
        /// </summary>
        public string? UserEmail { get; set; }
        /// <summary>
        /// 用户头像
        /// </summary>
        public required string UserImg { get; set; }
        /// <summary>
        /// 安全问题
        /// </summary>
        public string? Question { get; set; }
        /// <summary>
        /// 安全问题答案
        /// </summary>
        public string? SecPwd { get; set; }
    }
}
