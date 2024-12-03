using System.ComponentModel.DataAnnotations;

namespace TaskManager.Entities;

public class Task
{
    [Key]
    public int TaskId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DueDateTime { get; set; }
    public string State { get; set; }
    
    
    
}