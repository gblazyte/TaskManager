using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Entities;

namespace TaskManager.Features.Tasks;

// Request class for updating the task's status
public class UpdateTaskStateRequest
{
    public string State { get; set; } // The new state of the task
}

// Response class for the updated task's status
public class UpdateTaskStateResponse
{
    public int TaskId { get; set; }
    public string State { get; set; }
}

internal sealed class
    UpdateTaskState : Endpoint<UpdateTaskStateRequest, UpdateTaskStateResponse, TaskStateResponseMapper>
{
    private readonly DatabaseContext _context;

    public UpdateTaskState(DatabaseContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        Patch("/tasks/{id:int}/state"); // Define the PATCH route for updating the task state
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateTaskStateRequest req, CancellationToken ct)
    {
        int taskId = Route<int>("id"); // Get the task id from the route

        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.TaskId == taskId, ct); // Find task by id

        if (task is null)
        {
            await SendNotFoundAsync(ct); // Send 404 if task not found
            return;
        }

        // Update only the status field
        task.State = req.State;

        _context.Tasks.Update(task); // Mark the task entity as modified
        await _context.SaveChangesAsync(ct); // Save changes to the database

        var response = Map.FromEntity(task); // Map the updated entity to the response DTO
        await SendAsync(response, 200, ct); // Send the response with 200 status
    }
}

// Mapper class for the response
internal sealed class TaskStateResponseMapper : ResponseMapper<UpdateTaskStateResponse, TaskEntity>
{
    public override UpdateTaskStateResponse FromEntity(TaskEntity taskEntity)
    {
        return new UpdateTaskStateResponse
        {
            TaskId = taskEntity.TaskId,
            State = taskEntity.State
        };
    }
}