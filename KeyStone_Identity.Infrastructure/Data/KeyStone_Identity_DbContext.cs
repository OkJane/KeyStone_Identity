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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>().HasData(
                new Role { ID = 1, Name = "Admin" },
                new Role { ID = 2, Name = "User" },
                new Role { ID = 3, Name = "Support" }
            );

            modelBuilder.Entity<Permissions>().HasData(
                new Permissions { ID = 1, Name = "users.read", Description = "View user records", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permissions { ID = 2, Name = "users.update", Description = "Update user details or account status", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permissions { ID = 3, Name = "users.lock", Description = "Lock or disable a user account", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permissions { ID = 4, Name = "users.assign_role", Description = "Assign roles to users", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permissions { ID = 5, Name = "roles.read", Description = "View roles", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permissions { ID = 6, Name = "roles.create", Description = "Create roles", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permissions { ID = 7, Name = "roles.update", Description = "Update role details", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permissions { ID = 8, Name = "roles.delete", Description = "Delete roles", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permissions { ID = 9, Name = "permissions.read", Description = "View available permissions", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permissions { ID = 10, Name = "roles.assign_permission", Description = "Grant or revoke permissions for roles", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permissions { ID = 11, Name = "profile.read", Description = "View your own profile", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permissions { ID = 12, Name = "profile.update", Description = "Update your own profile", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permissions { ID = 13, Name = "audit.read_all", Description = "View all audit logs", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
                );

            modelBuilder.Entity<RolePermissions>().HasData(
                new RolePermissions { ID = 1, RoleID = 1, PermissionID = 1 },
                new RolePermissions { ID = 2, RoleID = 1, PermissionID = 2 },
                new RolePermissions { ID = 3, RoleID = 1, PermissionID = 3 },
                new RolePermissions { ID = 4, RoleID = 1, PermissionID = 4 },
                new RolePermissions { ID = 5, RoleID = 1, PermissionID = 5 },
                new RolePermissions { ID = 6, RoleID = 1, PermissionID = 6 },
                new RolePermissions { ID = 7, RoleID = 1, PermissionID = 7 },
                new RolePermissions { ID = 8, RoleID = 1, PermissionID = 8 },
                new RolePermissions { ID = 9, RoleID = 1, PermissionID = 9 },
                new RolePermissions { ID = 10, RoleID = 1, PermissionID = 10 },
                new RolePermissions { ID = 11, RoleID = 1, PermissionID = 11 },
                new RolePermissions { ID = 12, RoleID = 1, PermissionID = 12 },
                new RolePermissions { ID = 13, RoleID = 2, PermissionID = 11 },
                new RolePermissions { ID = 14, RoleID = 2, PermissionID = 12 },
                new RolePermissions { ID = 15, RoleID = 3, PermissionID = 1 },
                new RolePermissions { ID = 16, RoleID = 3, PermissionID = 3 },
                new RolePermissions { ID = 17, RoleID = 3, PermissionID = 5 },
                new RolePermissions { ID = 18, RoleID = 3, PermissionID = 9 },
                new RolePermissions { ID = 19, RoleID = 3, PermissionID = 11 },
                new RolePermissions { ID = 20, RoleID = 3, PermissionID = 12 },
                new RolePermissions { ID = 21, RoleID = 1, PermissionID = 13 },
                new RolePermissions { ID = 22, RoleID = 3, PermissionID = 13 }
                );

            modelBuilder.Entity<UserRole>()
    .HasOne(ur => ur.User)
    .WithMany(u => u.UserRoles)
    .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<RolePermissions>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleID);

            modelBuilder.Entity<RolePermissions>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionID);
        }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<ActivationToken> ActivationTokens { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Permissions> Permissions { get; set; }
        public DbSet<RolePermissions> RolePermissions { get; set; }
    }
}
