using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Entities;

namespace TaskManager.Features.Tasks
{
    internal sealed class DeleteTask : EndpointWithoutRequest
    {
        private readonly DatabaseContext _context;

        public DeleteTask(DatabaseContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Delete("/tasks/{id:int}"); // Define the DELETE route with task id
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            int taskId = Route<int>("id"); // Get the task id from the route

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.TaskId == taskId, ct); // Find the task by id

            if (task is null)
            {
                await SendNotFoundAsync(ct); // Send 404 if task not found
                return;
            }

            _context.Tasks.Remove(task); // Mark the task entity for deletion
            await _context.SaveChangesAsync(ct); // Save changes to the database

            await SendOkAsync(ct); // Send 200 OK response after successful deletion
        }
    }
}