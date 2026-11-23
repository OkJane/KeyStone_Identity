using KeyStone_Identity.Core.Enums;
using KeyStone_Identity.Core.Interfaces;
using KeyStone_Identity.Core.Models;
using KeyStone_Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Infrastructure.Repositories
{
    public class TokenRepository : ITokenRepository
    {
        private readonly KeyStone_Identity_DbContext _DbContext;
        public TokenRepository(KeyStone_Identity_DbContext dbContext)
        {
            _DbContext = dbContext;
        }
        public async Task<Token> GetToken(string hashedToken)
        {
            return await _DbContext.Tokens.FirstOrDefaultAsync(x => x.TokenHash == hashedToken);
        }

        public async Task<Token> Upsert(Token token)
        {
            var existingToken = await _DbContext.Tokens.Where(x => x.UserId == token.UserId && x.TokenHash == token.TokenHash).AnyAsync();
            if (!existingToken)
            {
                await _DbContext.Tokens.AddAsync(token);
            }
            else
            {
                _DbContext.Tokens.Update(token);
            }
            await _DbContext.SaveChangesAsync();
            return token;
        }

        public async Task InvalidateTokens(long userId, TokenTypes type)
        {
            await _DbContext.Tokens
                .Where(x =>
                    x.UserId == userId &&
                    x.Type == type &&
                    x.UsedAt == null)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(
                        x => x.UsedAt,
                        DateTime.UtcNow));
        }

        public async Task<bool> ConsumeToken(
            string hashedToken,
            TokenTypes type)
        {
            var affectedRows = await _DbContext.Tokens
                .Where(x =>
                    x.TokenHash == hashedToken &&
                    x.Type == type &&
                    x.UsedAt == null &&
                    x.ExpiresAt > DateTime.UtcNow)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(
                        x => x.UsedAt,
                        DateTime.UtcNow));

            return affectedRows == 1;
        }
    }
}
