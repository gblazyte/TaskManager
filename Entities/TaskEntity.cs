using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.Entities;

public class TaskEntity
{
    [Key]
    public int TaskId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DueDateTime { get; set; }
    public string State { get; set; }
    
    public int? UserId { get; set; }
    public User? User { get; set; }
    
}

public class TaskEntityModelBuilder
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskEntity>().Property(y => y.Title).HasMaxLength(180).IsRequired();
        
        modelBuilder.Entity<TaskEntity>().HasOne(t => t.User).WithMany(u => u.Tasks).OnDelete(DeleteBehavior.Restrict);
    }
}