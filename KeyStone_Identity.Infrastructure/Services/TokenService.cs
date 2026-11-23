using KeyStone_Identity.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace KeyStone_Identity.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        public string GenerateToken()
        {
            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return string.Create(8, characters, (span, chars) =>
            {
                for (int i = 0; i < span.Length; i++)
                {
                    span[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
                }
            });
        }

        public string HashToken(string token)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);

            var normalizedToken = token.Trim().ToUpperInvariant();

            var hash = SHA256.HashData(
                Encoding.UTF8.GetBytes(normalizedToken));

            return Convert.ToHexString(hash);
        }
    }
}
