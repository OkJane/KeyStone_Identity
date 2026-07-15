using KeyStone_Identity.Core.DTOs;
using KeyStone_Identity.Core.DTOs.Response;
using KeyStone_Identity.Core.Enums;
using KeyStone_Identity.Core.Interfaces;
using KeyStone_Identity.Core.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace KeyStone_Identity.Infrastructure.Services
{
    public class JwtTokenService : ITokenService
    {
        private readonly JwtSettings _settings;
        public JwtTokenService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        public string GenerateRefreshToken()
        {
            byte[] randomBytes = RandomNumberGenerator.GetBytes(32);
            string base64TokenString = Convert.ToBase64String(randomBytes);
            return base64TokenString;
        }

        public async Task<JWTAuthResult> GenerateToken(User user)
        {
            //Create Claim
            var claims = new List<Claim>
            {
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.ID.ToString()),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, user.EmailAddress)
            };

            //Create Signing Key 
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));

            //Create Signing Credentials
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //Create Token Object
            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                signingCredentials: creds,
                expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiration)
                );

            //Convert Token to String
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);

            return new JWTAuthResult
            {
                Code = ResponseCodes.Successful,
                Message = "Successful",
                AccessToken = tokenString,
                RefreshToken = GenerateRefreshToken(),
                AccessTokenExpirationDate = token.ValidTo,
                RefreshTokenExpirationDate = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpiration),
                AuthUser = new Core.DTOs.AuthUser
                {
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    EmailAddress = user.EmailAddress
                }
            };

        }
    }
}
