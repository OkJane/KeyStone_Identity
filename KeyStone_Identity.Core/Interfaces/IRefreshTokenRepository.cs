using KeyStone_Identity.Core.DTOs;
using KeyStone_Identity.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> Save(RefreshToken refreshToken);
        Task<RefreshToken> GetToken(string refreshTokenString);
        Task Revoke(string refreshTokenString);
        Task RevokeTokens(long userID);
        Task DeleteExpired(string refreshTokenString);
    }
}
