using AuthService;
using Common.Extensions;
using Common;
using Common.Events;
using DataModels;
using LinqToDB;
using Microsoft.AspNetCore.Authorization;
using UserRole = Common.UserRole;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var connectionString = configuration.GetConnectionString("AuthDb");

builder.Services.AddRabbitMqConnection(configuration);
builder.Services.AddProducer(configuration);

var app = builder.Build();
app.UseCustomAuthorization();

var logger = app.Logger;

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

await AddDefaultAdminAsync();

app.MapPost("/login", [AllowAnonymous] async (string username, string password) =>
{
    await using var dataConnection = new AuthDb(connectionString);
    var user = dataConnection.Users.FirstOrDefault(x => x.Username == username && x.Password == password);
    
    return user == null ? 
        Results.NotFound() : 
        Results.Ok(new { Token = TokenService.BuildToken(user.Username, user.PublicId, (UserRole)user.RoleId) });
});

app.MapPost("/users", [Authorize(Roles = "Admin")] async (User newUser) =>
{
    await using var db = new AuthDb(connectionString);
    await db.InsertAsync(newUser);
    
    var userDto = new UserDto
    {
        Username = newUser.Username,
        PublicId = newUser.PublicId,
        Role = (UserRole)newUser.RoleId
    };
    await ProduceEventAsync(new StreamingEvent<UserDto>("UserCreated", userDto));

    return Results.Ok();
});

app.MapPut("/users/{userId}", [Authorize(Roles = "Admin")] async (int userId, User updatedUser) =>
{
    await using var db = new AuthDb(connectionString);

    var isUserExist = db.Users.Any(u => u.Id == userId);
    if (!isUserExist)
        return Results.NotFound();

    await db.UpdateAsync(updatedUser);
    
    var userDto = new UserDto
    {
        Username = updatedUser.Username,
        PublicId = updatedUser.PublicId,
        Role = (UserRole)updatedUser.RoleId
    };
    await ProduceEventAsync(new StreamingEvent<UserDto>("UserUpdated", userDto));

    return Results.Ok();
});

app.MapGet("/users", [Authorize(Roles = "Admin")] async () =>
{
    await using var db = new AuthDb(connectionString);

    var users = await db.Users.ToListAsync();
    return Results.Ok(users);
});

async Task ProduceEventAsync<T>(T @event) where T : Event
{
   Console.WriteLine("PRODUCE SOME EVENT!!!");
}

async Task AddDefaultAdminAsync()
{
    var defaultAdminName = "MasterPopug";
    
    await using var dataConnection = new AuthDb(connectionString);
    var isDefaultAdminExits = dataConnection.Users
        .Any(u => u.Username == defaultAdminName && u.RoleId == (int)UserRole.Admin);
    
    if (isDefaultAdminExits)
        return;
    
    var defaultAdmin = new User
    {
        Username = defaultAdminName,
        Password = "fck",
        RoleId = (int)UserRole.Admin,
        PublicId = Guid.NewGuid(),
        IsDeleted = false
    };
    
    await dataConnection.InsertAsync(defaultAdmin);
}

app.Run();