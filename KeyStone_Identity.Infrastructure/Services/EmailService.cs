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
using System.Text.Encodings.Web;

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
            string base64TokenString = Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
            return base64TokenString;
        }

        public async Task SendActivationMail(string name,string recipient,string activationToken)
        {
            const string subject = "Activate Your KeyStone Account";

            string serviceBaseUrl = _configmanager["KeystoneAuthBaseUrl"]!;

            string activationUrl =
                $"{serviceBaseUrl.TrimEnd('/')}/api/Auth/Verify-Email?token={Uri.EscapeDataString(activationToken)}";

            string safeName = HtmlEncoder.Default.Encode(name);

            string body = $"""
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
        </head>
        <body style="margin:0; padding:0; background-color:#f4f6f8;
                     font-family:Arial,sans-serif;">

            <div style="max-width:560px; margin:40px auto; padding:35px;
                        background-color:#ffffff; border-radius:8px;">

                <h2 style="color:#202938; margin-bottom:25px;">
                    Welcome to KeyStone!
                </h2>

                <p style="color:#4b5563; font-size:15px; line-height:1.7;">
                    Hello {safeName},
                </p>

                <p style="color:#4b5563; font-size:15px; line-height:1.7;">
                    Thank you for creating a KeyStone account.
                    We're excited to have you on board.
                </p>

                <p style="color:#4b5563; font-size:15px; line-height:1.7;">
                    To complete your registration and activate your
                    account, please click the button below.
                </p>

                <div style="text-align:center; margin:35px 0;">
                    <a href="{HtmlEncoder.Default.Encode(activationUrl)}"
                       style="display:inline-block; padding:14px 30px;
                              background-color:#2563eb; color:#ffffff;
                              text-decoration:none; border-radius:6px;
                              font-size:15px; font-weight:bold;">
                        Activate My Account
                    </a>
                </div>

                <p style="color:#6b7280; font-size:13px; line-height:1.6;">
                    If the button doesn't work, copy and paste the
                    following link into your browser:
                </p>

                <p style="font-size:13px; word-break:break-all;">
                    <a href="{HtmlEncoder.Default.Encode(activationUrl)}"
                       style="color:#2563eb;">
                        {HtmlEncoder.Default.Encode(activationUrl)}
                    </a>
                </p>

                <p style="color:#6b7280; font-size:14px; line-height:1.6;">
                    If you did not create this account, you can safely
                    ignore this email.
                </p>

                <hr style="border:none; border-top:1px solid #e5e7eb;
                           margin:30px 0;">

                <p style="color:#9ca3af; font-size:12px;">
                    Regards,<br>
                    <strong>KeyStone Security Team</strong>
                </p>

            </div>
        </body>
        </html>
        """;

            await SendEmailAsync(recipient, subject, body);
        }

        public async Task SendEmailAsync(string recipient, string subject, string body)
        {
            using var client = new SmtpClient(_smptsettings.Host, 2525)
            {
                Credentials = new NetworkCredential(_smptsettings.Username, _smptsettings.Password),
                EnableSsl = true
            };
            using var message = new MailMessage(_smptsettings.From,recipient,subject,body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(_smptsettings.From, recipient, subject, body);
        }
    }
}
