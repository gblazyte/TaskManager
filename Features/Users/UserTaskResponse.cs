using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Entities;


namespace TaskManager.Features.Users
{
    // Response class for the user's tasks
    public class UserTaskResponse
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    internal sealed class GetUserTasks : EndpointWithoutRequest<List<UserTaskResponse>, UserTaskResponseMapper>
    {
        private readonly DatabaseContext _context;

        public GetUserTasks(DatabaseContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/users/{id:int}/tasks");  // Define the GET route for user tasks by user id
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            int userId = Route<int>("id");  // Get the user id from the route

            var user = await _context.Users
                                     .Include(u => u.Tasks)
                                     .FirstOrDefaultAsync(u => u.Id == userId, ct);  // Find user with their tasks

            if (user is null)
            {
                await SendNotFoundAsync(ct);  // Send 404 if user not found
                return;
            }

            var response = Map.FromEntity(user.Tasks.ToList());  // Map the user's tasks to response
            await SendAsync(response, 200, ct);  // Send the response with 200 OK status
        }
    }

    // Mapper class for the user's tasks
    internal sealed class UserTaskResponseMapper : ResponseMapper<List<UserTaskResponse>, List<TaskEntity>>
    {
        public override List<UserTaskResponse> FromEntity(List<TaskEntity> taskEntities)
        {
            return taskEntities.Select(taskEntity => new UserTaskResponse
            {
                TaskId = taskEntity.TaskId,
                Title = taskEntity.Title,
                Description = taskEntity.Description
            }).ToList();
        }
    }
}
