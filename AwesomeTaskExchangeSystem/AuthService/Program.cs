using AuthService;
using Common.Extensions;
using Common;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCustomAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapPost("/login", [AllowAnonymous] async (string username, string password) =>
{
    await using var dataConnection = new DataConnection();
    var user = dataConnection.Users.FirstOrDefault(x => x.Username == username && x.Password == password);
    if (user == null)
    {
        return Results.NotFound();
    }

    //Produce event UserLogged

    return Results.Ok(new { Token = TokenService.BuildToken(user) });
});

app.Run();