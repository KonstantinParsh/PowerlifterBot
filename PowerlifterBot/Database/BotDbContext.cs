using Microsoft.EntityFrameworkCore;
using PowerlifterBot.Record;

namespace PowerlifterBot.Database;

public class BotDbContext : DbContext
{
    public DbSet<UserProfile> Users { get; set; }
    public DbSet<WorkoutRecords> WorkoutRecords { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=powerlifter.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserProfile>()
            .Property(u => u.WeightUnit)
            .HasConversion<string>();
    }
}