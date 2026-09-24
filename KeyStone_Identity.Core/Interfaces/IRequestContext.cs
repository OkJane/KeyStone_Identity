using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Interfaces
{
    public interface IRequestContext
    {
        string? IPAddress { get; }
        string? UserAgent { get; }
    }
}
