using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MeowMemoirsAPI.Models.JWT
{
    /// <summary>
    /// jwt配置
    /// </summary>
    public class JwtConfig : IOptions<JwtConfig>
    {
        /// <summary>
        /// jwt配置
        /// </summary>
        public JwtConfig Value => this;
        /// <summary>
        /// jwt密钥
        /// </summary>
        public required string SecretKey { get; set; }
        /// <summary>
        /// jwt颁发者
        /// </summary>
        public required string Issuer { get; set; }
        /// <summary>
        /// jwt接收者
        /// </summary>
        public required string Audience { get; set; }
        /// <summary>
        /// jwt过期时间(分钟)
        /// </summary>
        public int Expired { get; set; }

        /// <summary>
        /// jwt是否验证过期时间
        /// </summary>
        public DateTime NotBefore => DateTime.UtcNow;
        /// <summary>
        /// jwt签发时间
        /// </summary>
        public DateTime IssuedAt => DateTime.UtcNow;
        /// <summary>
        /// jwt过期时间
        /// </summary>
        public DateTime Expiration => IssuedAt.AddMinutes(Expired);
        /// <summary>
        /// 长jwt长时间过期(分钟)
        /// </summary>
        public int LongExpired { get; set; }
        /// <summary>
        /// 长jwt是否验证过期时间
        /// </summary>
        public DateTime LongNotBefore => DateTime.UtcNow;
        /// <summary>
        /// 长jwt签发时间
        /// </summary>
        public DateTime LongIssuedAt => DateTime.UtcNow;
        /// <summary>
        /// 长jwt过期时间
        /// </summary>
        public DateTime LongExpiration => IssuedAt.AddMinutes(LongExpired);
        /// <summary>
        /// jwt签名密钥
        /// </summary>
        private SecurityKey SigningKey => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        /// <summary>
        /// jwt签名凭据
        /// </summary>
        public SigningCredentials SigningCredentials => new(SigningKey, SecurityAlgorithms.HmacSha256);
    }
}