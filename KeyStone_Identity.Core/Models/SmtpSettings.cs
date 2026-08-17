using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Models
{
    public class SmtpSettings
    {
        public string Host {  get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
    }
}
