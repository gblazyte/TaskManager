using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Entities;

namespace TaskManager.Features.Users
{
    // Request class for updating user information
    public class UpdateUserRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
    }

    // Response class for the updated user
    public class UpdateUserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }

    internal sealed class UpdateUser : Endpoint<UpdateUserRequest, UpdateUserResponse, UserUpdateMapper>
    {
        private readonly DatabaseContext _context;

        public UpdateUser(DatabaseContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Put("/users/{id:int}");  // Define the PUT route for updating user details
            AllowAnonymous();
        }

        public override async Task HandleAsync(UpdateUserRequest req, CancellationToken ct)
        {
            int userId = Route<int>("id");  // Get the user id from the route

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);  // Find user by id

            if (user is null)
            {
                await SendNotFoundAsync(ct);  // Send 404 if user not found
                return;
            }

            // Update user's data
            user.Username = req.Username;
            user.Email = req.Email;

            _context.Users.Update(user);  // Mark user entity as modified
            await _context.SaveChangesAsync(ct);  // Save changes

            var response = Map.FromEntity(user);  // Map entity to response DTO
            await SendAsync(response, 200, ct);  // Send response
        }
    }

    // Mapper class for the update user response
    internal sealed class UserUpdateMapper : ResponseMapper<UpdateUserResponse, User>
    {
        public override UpdateUserResponse FromEntity(User userEntity)
        {
            return new UpdateUserResponse
            {
                Id = userEntity.Id,
                Username = userEntity.Username,
                Email = userEntity.Email
            };
        }
    }
}
