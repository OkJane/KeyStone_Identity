using KeyStone_Identity.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Interfaces
{
    public interface IActivationTokenRepository
    {
        Task<ActivationToken> Upsert(ActivationToken token);
        Task<ActivationToken> Get(string token);
    }
}
