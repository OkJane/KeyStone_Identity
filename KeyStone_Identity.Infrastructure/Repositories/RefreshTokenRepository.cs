using KeyStone_Identity.Core.DTOs;
using KeyStone_Identity.Core.Interfaces;
using KeyStone_Identity.Core.Models;
using KeyStone_Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly KeyStone_Identity_DbContext _dbContext;
        public RefreshTokenRepository(KeyStone_Identity_DbContext DbContext)
        {
            this._dbContext = DbContext;
        }

        public async Task DeleteExpired(string refreshTokenString)
        {
            throw new NotImplementedException();
        }

        public async Task<RefreshToken> GetToken(string refreshTokenString)
        {
            try
            {
               var refreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshTokenString);
                return refreshToken;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message} \n Exception: {ex.ToString()} \n Inner Exception: {ex?.InnerException}");
            }
        }

        public async Task Revoke(string? refreshTokenString)
        {
            try
            {
                var refreshToken = await GetToken(refreshTokenString);
                if (refreshToken != null)
                {
                    refreshToken.RevokedAt = DateTime.UtcNow;
                    _dbContext.RefreshTokens.Update(refreshToken);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message} \n Exception: {ex.ToString()} \n Inner Exception: {ex?.InnerException}");
            }
        }

        public async Task RevokeTokens(long userID)
        {
            await _dbContext.RefreshTokens.Where(r => r.RevokedAt == null && r.UserId == userID).ExecuteUpdateAsync(x => x.SetProperty(r => r.RevokedAt, DateTime.UtcNow));
        }

        public async Task<RefreshToken> Save(RefreshToken refreshToken)
        {
            try
            {
                await _dbContext.RefreshTokens.AddAsync(refreshToken);
                await _dbContext.SaveChangesAsync();
                return refreshToken;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message} \n Exception: {ex.ToString()} \n Inner Exception: {ex?.InnerException}");
            }
        }
    }
}
