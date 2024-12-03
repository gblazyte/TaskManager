namespace TaskManager.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    //user can have a lot of tasks
    public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
}