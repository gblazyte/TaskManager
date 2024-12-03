using FastEndpoints;
using TaskManager.Data;
using TaskManager.Entities;

namespace TaskManager.Features.Users
{
    // Request class for creating a new user
    public class CreateUserRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }

    // Response class for the created user
    public class CreateUserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }

    internal sealed class CreateUser : Endpoint<CreateUserRequest, CreateUserResponse, UserCreateMapper>
    {
        private readonly DatabaseContext _context;

        public CreateUser(DatabaseContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/users");  // POST route for creating a user
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
        {
            var newUser = new User
            {
                Username = req.Username,
                Password = req.Password,  // 
                Email = req.Email
            };

            _context.Users.Add(newUser);  // add the new user to the database
            await _context.SaveChangesAsync(ct);  // save 

            var response = Map.FromEntity(newUser);  // map the entity to response
            await SendAsync(response, 201, ct);  // send the response with 201 Created status
        }
    }

    // Mapper class for user creation response
    internal sealed class UserCreateMapper : ResponseMapper<CreateUserResponse, User>
    {
        public override CreateUserResponse FromEntity(User userEntity)
        {
            return new CreateUserResponse
            {
                Id = userEntity.Id,
                Username = userEntity.Username,
                Email = userEntity.Email
            };
        }
    }
}
