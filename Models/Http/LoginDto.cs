namespace MeowMemoirsAPI.Models.Http
{
    /// <summary>
    /// 登录数据传输对象
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// 登录类型
        /// </summary>
        public required string Type { get; set; }
        /// <summary>
        /// 登录标识
        /// </summary>
        public required string Identifier { get; set; }
        /// <summary>
        /// 登录密码
        /// </summary>
        public required string Password { get; set; }
    }

}
