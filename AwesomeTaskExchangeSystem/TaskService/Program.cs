using System.Security.Claims;
using System.Text;
using Common.Extensions;
using Common;
using Common.Events;
using DataModels;
using LinqToDB;
using Microsoft.AspNetCore.Authorization;
using RabbitMQ.Client;
using TaskService;
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

var connectionString = configuration.GetConnectionString("TasksDb") ?? 
                       throw new ArgumentNullException("configuration.GetConnectionString(\"TasksDb\")");

builder.Services
    .AddRabbitMqConnection(configuration);

var app = builder.Build();
app.UseCustomAuthorization();

var logger = app.Logger;

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/tasks", [Authorize(Roles = "Worker")] async (ClaimsPrincipal user) =>
{
    await using var db = new TasksDb(connectionString);
    
    var userPublicId = user.GetPublicId();
    var tasks = await db.Tasks
        .Where(t => t.AssignedUser == userPublicId)
        .Select(t => new { t.Id, t.PublicId, t.Description, t.Status, t.AssignedUser })
        .ToListAsync();
    
    return Results.Ok(tasks);
});

app.MapPost("/tasks", [Authorize] async (AtesTask newTask) =>
{
    await using var db = new TasksDb(connectionString);

    newTask.StatusId = (int)AtesTaskStatus.InProgress;
    newTask.AssignedUser = GetRandomWorker();
    
    var newTaskDto = new AtesTaskDto
    {
        PublicId = newTask.PublicId,
        Name = newTask.Name, 
        Description = newTask.Description, 
        Status = AtesTaskStatus.InProgress, 
        AssignedUser = GetRandomWorker()
    };
    
    await db.InsertAsync(newTask);
    await ProduceEventAsync(new TaskAssigned(newTask.PublicId, newTask.AssignedUser));
    await ProduceEventAsync(new StreamingEvent<AtesTaskDto>("NewTaskCreated",newTaskDto));

    return Results.Ok();
});

app.MapPost("/tasks/{taskId}/complete", [Authorize(Roles = "Worker")] 
    async (ClaimsPrincipal user, int taskId) =>
{
    await using var db = new TasksDb(connectionString);

    var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
    if (task?.AssignedUser != user.GetPublicId())
        return Results.NotFound(taskId.ToString());
    
    if (task.StatusId == (int)AtesTaskStatus.Completed)
        return Results.BadRequest($"Task {taskId.ToString()} already completed");

    await db.Tasks
        .Where(t => t.Id == taskId)
        .Set(t => t.StatusId, (int)AtesTaskStatus.Completed)
        .UpdateAsync();
    
    await ProduceEventAsync(new TaskCompleted(task.PublicId));

    var atesTaskDto = new AtesTaskDto
    {
        PublicId = task.PublicId,
        Status = AtesTaskStatus.Completed
    };
    
    await ProduceEventAsync(new StreamingEvent<AtesTaskDto>("TaskCompleted", atesTaskDto));

    return Results.Ok();
});

app.MapPost("/tasks/shuffle", [Authorize(Roles = "Manager,Admin")] async () =>
{
    await using var db = new TasksDb(connectionString);

    var tasks = await db.Tasks
        .Where(t => t.StatusId == (int)AtesTaskStatus.InProgress)
        .ToListAsync();
    
    foreach(var task in tasks)
    {
        var assignUserPublicId = GetRandomWorker();
        if (task.AssignedUser == assignUserPublicId)
            continue;

        task.AssignedUser = assignUserPublicId;
        await db.Tasks
            .Where(t => t.Id == task.Id)
            .Set(t => t.AssignedUser, task.AssignedUser)
            .UpdateAsync();
        
        await ProduceEventAsync(new TaskAssigned(task.PublicId, assignUserPublicId));
    }
    
    return Results.Ok();
});

Guid GetRandomWorker()
{
    using var db = new TasksDb(connectionString);
    var workers = db.Users
        .Where(u => u.RoleId == (int)UserRole.Worker)
        .Select(u => u.PublicId)
        .ToList();
    
    if (!workers.Any())
    {
        throw new Exception("Worker was not found");
    }
    var randomWorker = workers[new Random().Next(0, workers.Count)];

    return randomWorker;
}

async Task ProduceEventAsync<T>(T @event) where T : Event
{
    Console.WriteLine("PRODUCE SOME EVENT!");
}

app.Run();


void ConfigurateProducer()
{
    var factory = new ConnectionFactory { HostName = "localhost" };
    using var connection = factory.CreateConnection();
    using var channel = connection.CreateModel();

    channel.ExchangeDeclare(exchange: "direct_tasks", type: ExchangeType.Direct);

    var severity = "tasks";
    var message = string.Empty;
    var body = Encoding.UTF8.GetBytes(message);

    channel.BasicPublish(exchange: "direct_logs",
        routingKey: severity,
        basicProperties: null,
        body: body);
}

// public class UserDataChangedEventHandler
// {
//     public UserDataChangedEventHandler()
//     {
//         
//     }
// }