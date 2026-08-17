using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Interfaces
{
    public interface IEmailService
    {
        string GenerateActivationToken();
        Task SendEmailAsync(string recipient, string subject, string body);
        Task SendActivationMail(string name,string recipient, string activationToken);
    }
}
