using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using TaskManager.Data;
using TaskManager.Entities;

namespace TaskManager.Features.Tasks;


public class TicketResponseDto
{
    public int TaskId { get; set; }
}

public class CreateTask: Endpoint<TaskEntity>
{
    
    private readonly DatabaseContext _context;

    public CreateTask(DatabaseContext context)
    {
        _context = context;
    }

    public override void Configure()
    {

        Post("tasks/create");
        AllowAnonymous();
        Validator<CreateTaskValidator>();
        // AuthSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
        // Roles("Admin");
    }


    public override async Task HandleAsync(TaskEntity req, CancellationToken ct)
    {
        await _context.Tasks.AddAsync(req, ct);
        await _context.SaveChangesAsync(ct);

        await SendOkAsync(ct);

    }
    
    internal class CreateTaskValidator : Validator<TaskEntity>
    {
        public CreateTaskValidator()
        {
            RuleFor(x=>x.TaskId).GreaterThan(0).WithMessage("Task ID must be greater than 0");
        }
    }
    
    
}