using AuthService;
using Common.Extensions;
using Common;
using Common.Events;
using LinqToDB;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCustomAuthorization();

var logger = app.Logger;

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/login", [AllowAnonymous] async (string username, string password) =>
{
    await using var dataConnection = new DataConnection();
    var user = dataConnection.Users.FirstOrDefault(x => x.Username == username && x.Password == password);
    if (user == null)
    {
        return Results.NotFound();
    }

    await ProduceEventAsync(new UserLogged(user.PublicId));
    
    return Results.Ok(new { Token = TokenService.BuildToken(user) });
});

app.MapPost("/users", [Authorize(Roles = "Admin")] async (User newUser) =>
{
    await using var dataConnection = new DataConnection();
    await dataConnection.InsertAsync(newUser);
    
    await ProduceEventAsync(new StreamingEvent<User>(newUser));

    return Results.Ok();
});

app.MapPut("/users/{publicId}", [Authorize(Roles = "Admin")] async (Guid publicId, User updatedUser) =>
{
    await using var dataConnection = new DataConnection();
    await dataConnection.UpdateAsync(updatedUser);
    
    await ProduceEventAsync(new StreamingEvent<User>(updatedUser));

    return Results.Ok();
});

async Task ProduceEventAsync<T>(T @event) where T : Event
{
    logger.LogDebug("PRODUCE SOME EVENT!!!");
}

app.Run();