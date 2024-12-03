using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Entities;

namespace TaskManager.Features.Tasks;


public class GetTaskResponse
{
    public int TaskId { get; set; }
}

internal sealed class GetTasks: EndpointWithoutRequest<List<GetTaskResponse>, TaskResponseMapper>
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
    }


    public override async Task HandleAsync(CancellationToken ct)
    {
        var tasks = await _context.Tasks.ToListAsync(ct);

        //tasks.Where(x => x.TaskId > 5).ToList();

        var response = Map.FromEntity(tasks);
        
        await SendAsync(response, 200, ct);
    }
}

//sukurti request mapper!!!!!!!!!!!!!!
//prideti filtra???

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

