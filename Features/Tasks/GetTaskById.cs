using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Entities;


namespace TaskManager.Features.Tasks
{
    public class GetTaskByIdResponse
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        
        public DateTime DueDateTime { get; set; }
        public string State { get; set; }
        
    }

    internal sealed class GetTaskById : EndpointWithoutRequest<GetTaskByIdResponse, TaskByIdResponseMapper>
    {
        private readonly DatabaseContext _context;

        public GetTaskById(DatabaseContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/tasks/{id:int}");  
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            int taskId = Route<int>("id"); // Get the task id from the route

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.TaskId == taskId, ct); // Find task by id

            if (task is null)
            {
                await SendNotFoundAsync(ct); 
                return;
            }

            var response = Map.FromEntity(task); // Map the task entity to response DTO
            await SendAsync(response, 200, ct);  
        }
    }

    internal sealed class TaskByIdResponseMapper : ResponseMapper<GetTaskByIdResponse, TaskEntity>
    {
        public override GetTaskByIdResponse FromEntity(TaskEntity taskEntity)
        {
            return new GetTaskByIdResponse
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