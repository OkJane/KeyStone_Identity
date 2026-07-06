using KeyStone_Identity.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.DTOs.Response
{
    public class JWTAuthResult
    {
        public ResponseCodes Code { get; set; }
        public string Message { get; set; }
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpirationDate { get; set; }
        public AuthUser AuthUser { get; set; }

        public JWTAuthResult() { Code = ResponseCodes.Successful; Message = "Successful"; }
    }
}
