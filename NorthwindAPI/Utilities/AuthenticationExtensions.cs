using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PracticalApp.NorthwindAPI.Utilities
{
    public class AuthenticationExtensions
    {
        private static string _secret="this is a secret key for signing";
        public static SymmetricSecurityKey Key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        public static string SignJWTToken(string username) {
            var claims=new Claim[] {
                new Claim(ClaimTypes.Name,username),
                new Claim(JwtRegisteredClaimNames.Nbf,new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp,new DateTimeOffset(DateTime.UtcNow.AddDays(1)).ToUnixTimeSeconds().ToString())
            };
            var signingCredentials=new SigningCredentials(
                   key:Key,
                   algorithm:SecurityAlgorithms.HmacSha256);
            var jwtHeader=new JwtHeader(
                   signingCredentials);
            var jwtPayload=new JwtPayload(claims);
            var token=new JwtSecurityToken(
               header:jwtHeader,payload:jwtPayload
            );
            Console.WriteLine($"Token using ToString: {token.ToString()}");
            Console.WriteLine(@"Token Using JWT Handler: {0}",new JwtSecurityTokenHandler().WriteToken(token));
            var jwt=new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
    }
}