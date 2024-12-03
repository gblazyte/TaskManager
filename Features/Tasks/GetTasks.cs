using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Entities;

namespace TaskManager.Features.Tasks;

// Response model for individual task
public class GetTaskResponse
{
    public int TaskId { get; set; }
}

// Filter class for task queries
public class TaskFilter
{
    public string? State { get; set; } // optional
    public DateTime? DueDateTime { get; set; } // optional
    public string? SortBy { get; set; }  // Optional sorting
}

internal sealed class GetTasks : Endpoint<TaskFilter, List<GetTaskResponse>, TaskResponseMapper>
{
    private readonly DatabaseContext _context;

    public GetTasks(DatabaseContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        Get("/tasks");
        AllowAnonymous();
        // Enable Swagger to recognize query parameters
        Description(x => x.WithSummary("Get a list of tasks").WithTags("Tasks"));
    }

    public override async Task HandleAsync(TaskFilter req, CancellationToken ct)
    {
        var tasksQuery = _context.Tasks.AsQueryable();

        // Use the FilterTasks method to apply filters
        tasksQuery = TaskFilterHandler.FilterTasks(tasksQuery, req);
        
        // Apply sorting based on the SortBy filter
        tasksQuery = TaskFilterHandler.SortTasks(tasksQuery, req);

        var tasks = await tasksQuery.ToListAsync(ct);
        var response = Map.FromEntity(tasks);
        
        await SendAsync(response, 200, ct);
    }
}

// Class to handle filtering logic
internal static class TaskFilterHandler
{
    public static IQueryable<TaskEntity> FilterTasks(IQueryable<TaskEntity> tasksQuery, TaskFilter filter)
    {
        // Apply state filter
        if (!string.IsNullOrEmpty(filter.State))
        {
            tasksQuery = tasksQuery.Where(x => x.State == filter.State);
        }
        
        // Apply dueDate filter
        if (filter.DueDateTime.HasValue)
        {
            tasksQuery = tasksQuery.Where(x => x.DueDateTime.Date == filter.DueDateTime.Value.Date);
        }

        return tasksQuery;
    }
    
    public static IQueryable<TaskEntity> SortTasks(IQueryable<TaskEntity> tasksQuery, TaskFilter filter)
    {
        // Apply sorting based on the SortBy parameter
        if (!string.IsNullOrEmpty(filter.SortBy))
        {
            if (filter.SortBy.Equals("DueDateTime", StringComparison.OrdinalIgnoreCase)) // write DueDateTime
            {
                tasksQuery = tasksQuery.OrderBy(x => x.DueDateTime); // Sort by dueDate ascending
            }
        }

        return tasksQuery;
    }
}

// Mapper class to convert task entities to response models
internal sealed class TaskResponseMapper : ResponseMapper<List<GetTaskResponse>, List<TaskEntity>>
{
    public override List<GetTaskResponse> FromEntity(List<TaskEntity> e)
    {
        return e.Select(taskEntity => new GetTaskResponse
        {
            TaskId = taskEntity.TaskId,
        }).ToList();
    }
}


