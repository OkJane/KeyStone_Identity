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
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
    }
}
