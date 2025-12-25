using Microsoft.EntityFrameworkCore;
using UsersService.Models;

namespace UsersService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserChange> UserChanges => Set<UserChange>();

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<UserChange>()
            .HasIndex(c => c.UserId);

        modelBuilder.Entity<Order>()
            .HasIndex(c => c.UserId);

        base.OnModelCreating(modelBuilder);
    }
}
