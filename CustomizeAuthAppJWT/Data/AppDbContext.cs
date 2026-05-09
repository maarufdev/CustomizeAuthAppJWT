using Microsoft.EntityFrameworkCore;
using CustomizeAuthAppJWT.Models;

namespace CustomizeAuthAppJWT.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // Seed some permissions
        modelBuilder.Entity<Permission>().HasData(
            new Permission { Id = 1, Name = "ReadReports" },
            new Permission { Id = 2, Name = "WriteReports" },
            new Permission { Id = 3, Name = "ManageUsers" }
        );

        // Seed role-permission mapping
        modelBuilder.Entity<RolePermission>().HasData(
            new RolePermission { Id = 1, Role = "Admin", PermissionName = "ReadReports" },
            new RolePermission { Id = 2, Role = "Admin", PermissionName = "WriteReports" },
            new RolePermission { Id = 3, Role = "Admin", PermissionName = "ManageUsers" },
            new RolePermission { Id = 4, Role = "User", PermissionName = "ReadReports" }
        );

        base.OnModelCreating(modelBuilder);
    }
}