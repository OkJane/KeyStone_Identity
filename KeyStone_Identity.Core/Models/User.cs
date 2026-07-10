using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KeyStone_Identity.Core.Models
{
    public class User
    {
        public long ID { get; set; }
        [Required]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string EmailAddress { get; set; }
        [Required]
        public DateOnly DateOfBirth { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public DateTime DateCreated { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();


    }
}
