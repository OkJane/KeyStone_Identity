using KeyStone_Identity.Core.Interfaces;
using KeyStone_Identity.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace KeyStone_Identity.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smptsettings;
        private readonly IConfiguration _configmanager;
        public EmailService(IOptions<SmtpSettings> smtpSettings, IConfiguration configManager)
        {
            _smptsettings = smtpSettings.Value;
            _configmanager = configManager;
        }
        public string GenerateActivationToken()
        {
            byte[] randomBytes = RandomNumberGenerator.GetBytes(32);
            string base64TokenString = Convert.ToBase64String(randomBytes);
            return base64TokenString;
        }

        public async Task SendActivationMail(string name,string recipient, string activationToken)
        {
            string subject = "Please Activate Your Account";
            string serviceBaseUrl = _configmanager["KeystoneAuthBaseUrl"];
            string body = $"Hello {name}, please activate your account by clicking this link {serviceBaseUrl}/api/Auth/Verify-Email?token={activationToken}";
            await SendEmailAsync(recipient, subject, body);
        }

        public async Task SendEmailAsync(string recipient, string subject, string body)
        {
            using var client = new SmtpClient(_smptsettings.Host, 2525)
            {
                Credentials = new NetworkCredential(_smptsettings.Username, _smptsettings.Password),
                EnableSsl = true
            };

            client.Send(_smptsettings.From, recipient, subject, body);
        }
    }
}
