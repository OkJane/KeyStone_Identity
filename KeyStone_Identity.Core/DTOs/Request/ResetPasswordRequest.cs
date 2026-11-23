using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KeyStone_Identity.Core.DTOs.Request
{
    public class ResetPasswordRequest
    {
        [Required]
        [MinLength(12)]
        public string NewPassword { get; set; }
        [Required]
        public string Token { get; set; }
    }

    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;
    }
}
