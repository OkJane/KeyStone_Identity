using KeyStone_Identity.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Infrastructure.Data
{
    public class KeyStone_Identity_DbContext : DbContext
    {
        public KeyStone_Identity_DbContext(DbContextOptions<KeyStone_Identity_DbContext> options) : base(options)
        {
            
        }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<ActivationToken> ActivationTokens { get; set; }
    }
}
