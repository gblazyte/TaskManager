using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;

namespace TaskManager.Features.Users
{
    internal sealed class DeleteUser : EndpointWithoutRequest
    {
        private readonly DatabaseContext _context;

        public DeleteUser(DatabaseContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Delete("/users/{id:int}");  // Define the DELETE route for deleting a user
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            int userId = Route<int>("id");  // Get the user id from the route

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);  // Find user by id

            if (user is null)
            {
                await SendNotFoundAsync(ct);  // Send 404 if user not found
                return;
            }
            
            // Delete associated tasks explicitly
            var tasks = await _context.Tasks.Where(t => t.UserId == userId).ToListAsync(ct);
            _context.Tasks.RemoveRange(tasks);

            _context.Users.Remove(user);  // Mark user entity for deletion
            await _context.SaveChangesAsync(ct);  // Save changes

            await SendOkAsync(ct);  // Send 200 OK after successful deletion
        }
    }
}