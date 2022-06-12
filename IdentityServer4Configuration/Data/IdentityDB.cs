using IdentityServer4Configuration.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace IdentityServer4Configuration.Data
{
    public class IdentityDB : IdentityDbContext<SysUsers,SysRole,Guid,SysUserClaim,SysUserRole,SysUserLogin,SysRoleClaim,SysUserToken>
    {
        public IdentityDB(DbContextOptions<IdentityDB> options) :base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SysUsers>(b =>
            {
                b.HasMany(e => e.Claims)
                .WithOne(l => l.User)
                .HasForeignKey(p => p.UserId)
                .IsRequired();

                b.HasMany(e => e.Logins)
                .WithOne(j => j.User)
                .HasForeignKey(k => k.UserId)
                .IsRequired();

                b.HasMany(e => e.Tokens)
                .WithOne(k => k.User)
                .HasForeignKey(o => o.UserId)
                .IsRequired();
            });

            builder.Entity<SysRole>(b =>
            {
                b.HasMany(e => e.UserRoles)
                .WithOne(e => e.Role)
                .HasForeignKey(p => p.RoleId)
                .IsRequired();

                b.HasMany(e => e.RoleClaims)
                .WithOne(e => e.Role)
                .HasForeignKey(p => p.RoleId)
                .IsRequired();
            });

            base.OnModelCreating(builder);
        }

        public DbSet<SysUsers> SysUsers { get; set; }
        public DbSet<SysRole> SysRoles { get; set; }
        public DbSet<SysUserClaim> SysUserClaims { get; set; }
        public DbSet<SysUserLogin> SysUserLogins { get; set; }
        public DbSet<SysUserToken> SysUserTokens { get; set; }
        public DbSet<SysRoleClaim> SysRoleClaims { get; set; }
        public DbSet<SysUserRole> SysUserRoles { get; set; }
        public DbSet<SysClientEntity> SysClients { get; set; }
        public DbSet<SysApiResourceEntity> SysApiResources { get; set; }
        public DbSet<SysIdentityResourceEntity> SysIdentityResources { get; set; }
        public DbSet<SysApiScopeEntity> SysApiScopes { get; set; }
    }
}
