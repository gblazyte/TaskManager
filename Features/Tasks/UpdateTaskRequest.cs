using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Entities;


namespace TaskManager.Features.Tasks
{
    // Request class for updating the task
    public class UpdateTaskRequest
    {
        public string Title { get; set; }  
        public string Description { get; set; }
        public DateTime DueDateTime { get; set; }
        public string State { get; set; }
    }

    // Response class for the updated task
    public class UpdateTaskResponse
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        
        public DateTime DueDateTime { get; set; }
        public string State { get; set; }
    }

    internal sealed class UpdateTask : Endpoint<UpdateTaskRequest, UpdateTaskResponse, TaskUpdateMapper>
    {
        private readonly DatabaseContext _context;

        public UpdateTask(DatabaseContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Put("/tasks/{id:int}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(UpdateTaskRequest req, CancellationToken ct)
        {
            int taskId = Route<int>("id");

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.TaskId == taskId, ct); // Find task by id

            if (task is null)
            {
                await SendNotFoundAsync(ct); 
                return;
            }

            // Update the task properties
            task.Title = req.Title;
            task.Description = req.Description;
            task.DueDateTime = req.DueDateTime;
            task.State = req.State;

            _context.Tasks.Update(task);  // Mark the task entity as modified
            await _context.SaveChangesAsync(ct);  // Save changes to the database

            var response = Map.FromEntity(task);  // Map the updated entity to the response DTO
            await SendAsync(response, 200, ct); 
        }
    }

    // Mapper class for the response
    internal sealed class TaskUpdateMapper : ResponseMapper<UpdateTaskResponse, TaskEntity>
    {
        public override UpdateTaskResponse FromEntity(TaskEntity taskEntity)
        {
            return new UpdateTaskResponse
            {
                TaskId = taskEntity.TaskId,
                Title = taskEntity.Title,
                Description = taskEntity.Description,
                DueDateTime = taskEntity.DueDateTime,
                State = taskEntity.State,
            };
        }
    }
}
