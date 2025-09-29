namespace MeowMemoirsAPI.Models.JWT
{

    /// <summary>
    /// 登录成功返回model
    /// </summary>
    public class JwtTokenResult
    {
        /// <summary>
        /// jwt令牌
        /// </summary>
        public required string Access_token { get; set; }
        /// <summary>
        /// 刷新令牌
        /// </summary>
        public required string Refresh_token { get; set; }
        /// <summary>
        /// 过期时间(单位秒)
        /// </summary>
        public int Expires_in { get; set; }
        /// <summary>
        /// 令牌类型
        /// </summary>
        public required string Token_type { get; set; }
    }
}
