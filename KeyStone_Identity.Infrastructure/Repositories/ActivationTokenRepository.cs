using KeyStone_Identity.Core.Interfaces;
using KeyStone_Identity.Core.Models;
using KeyStone_Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Infrastructure.Repositories
{
    public class ActivationTokenRepository : IActivationTokenRepository
    {
        private KeyStone_Identity_DbContext _DbContext;
        public ActivationTokenRepository(KeyStone_Identity_DbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public async Task<ActivationToken> Get(string token)
        {
            return await _DbContext.ActivationTokens.FirstOrDefaultAsync(x => x.Token == token);
        }

        public async Task<ActivationToken> Upsert(ActivationToken token)
        {
            if (await Get(token.Token) != null)
            {
                _DbContext.ActivationTokens.Update(token);
            }
            else
            {
                await _DbContext.ActivationTokens.AddAsync(token);
            }
            await _DbContext.SaveChangesAsync();
            return token;
        }
    }
}
