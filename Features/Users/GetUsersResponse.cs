using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Entities;


namespace TaskManager.Features.Users
{
    // Response class for listing all users
    public class GetUsersResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }

    internal sealed class GetAllUsers : EndpointWithoutRequest<List<GetUsersResponse>, UsersResponseMapper>
    {
        private readonly DatabaseContext _context;

        public GetAllUsers(DatabaseContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/users");  // Define the GET route for retrieving all users
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var users = await _context.Users.ToListAsync(ct);  // Fetch all users

            var response = Map.FromEntity(users);  // Map users to response DTO
            await SendAsync(response, 200, ct);  // Send response
        }
    }

    // Mapper class for the user response
    internal sealed class UsersResponseMapper : ResponseMapper<List<GetUsersResponse>, List<User>>
    {
        public override List<GetUsersResponse> FromEntity(List<User> userEntities)
        {
            return userEntities.Select(userEntity => new GetUsersResponse
            {
                Id = userEntity.Id,
                Username = userEntity.Username,
                Email = userEntity.Email
            }).ToList();
        }
    }
}