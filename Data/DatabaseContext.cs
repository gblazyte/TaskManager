using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManager.Entities;

namespace TaskManager.Data;

public class DatabaseContext: IdentityDbContext
{
    public DatabaseContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<TaskEntity> Tasks { get; set; }
    public DbSet<User> Users { get; set; }
    
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        optionsBuilder.UseSqlite("Data Source=TaskManager.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        TaskEntityModelBuilder.Configure(modelBuilder);
    }
}