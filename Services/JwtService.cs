using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using MeowMemoirsAPI.Interfaces;
using MeowMemoirsAPI.Models.DataBaseContext;
using MeowMemoirsAPI.Models.JWT;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MeowMemoirsAPI.Services
{
    /// <summary>
    /// 生成jwt
    /// </summary>
    /// <param name="jwtConfig"></param>
    public class JwtService(IOptions<JwtConfig> jwtConfig) : IJwtService
    {
        private readonly JwtConfig _jwtConfig = jwtConfig.Value;

        /// <summary>
        /// 生成token
        /// </summary>
        /// <param name="sub"></param>
        /// <param name="user">携带的用户信息</param>
        /// <returns></returns>
        public JwtTokenResult GenerateEncodedTokenAsync(string sub, User user)
        {
            //创建用户身份标识，可按需要添加更多信息
            var claims = new List<Claim>
            {
                new("rainbowid", user.RainbowId),
                new("username", user.UserName),
                new("permissions",user.Permissions),
                // token验证类型access 或者 refresh
                new("token_type","access"),
                //new("userimg",user.UserImg),
                //new Claim("realname",customClaims.realname),
                //new Claim("roles", string.Join(";",customClaims.roles)),
                //new Claim("permissions", string.Join(";",customClaims.permissions)),
                //new Claim("normalPermissions", string.Join(";",customClaims.normalPermissions)),
                new(JwtRegisteredClaimNames.Sub, sub),
            };
            var longClaims = new List<Claim>
            {
                new("rainbowid", user.RainbowId),
                new("username", user.UserName),
                new("permissions",user.Permissions),
                // token验证类型access 或者 refresh
                new("token_type","refresh"),
                //new("userimg",user.UserImg),
                //new Claim("realname",customClaims.realname),
                //new Claim("roles", string.Join(";",customClaims.roles)),
                //new Claim("permissions", string.Join(";",customClaims.permissions)),
                //new Claim("normalPermissions", string.Join(";",customClaims.normalPermissions)),
                new(JwtRegisteredClaimNames.Sub, sub),
            };
            //创建令牌
            var jwt = new JwtSecurityToken(
                issuer: _jwtConfig.Issuer,
                audience: _jwtConfig.Audience,
                claims: claims,
                notBefore: _jwtConfig.NotBefore,
                expires: _jwtConfig.Expiration,
                signingCredentials: _jwtConfig.SigningCredentials);
            string access_token = new JwtSecurityTokenHandler().WriteToken(jwt);
            var longJwt = new JwtSecurityToken(
                issuer: _jwtConfig.Issuer,
                audience: _jwtConfig.Audience,
                claims: longClaims,
                notBefore: _jwtConfig.LongNotBefore,
                expires: _jwtConfig.LongExpiration,
                signingCredentials: _jwtConfig.SigningCredentials);
            string refresh_token = new JwtSecurityTokenHandler().WriteToken(longJwt);
            return new JwtTokenResult()
            {
                Access_token = access_token,
                Refresh_token = refresh_token,
                Expires_in = _jwtConfig.Expired,
                Token_type = JwtBearerDefaults.AuthenticationScheme,
            };
        }
    }


}
