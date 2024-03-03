using System.Security.Claims;
using Common.Extensions;
using Common;
using Common.Events;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Authorization;
using TaskService;

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

app.MapGet("/tasks", [Authorize(Roles = "Worker")] async (ClaimsPrincipal user) =>
{
    await using var db = new TasksDb();
    
    var userId = user.GetId();
    var task = await db.Tasks
        .Where(t => t.Assigned == userId)
        .Select(t => new { t.Id, t.PublicId, t.Description, t.Status, t.Assigned })
        .ToListAsync();
    return Results.Ok(task);
});

app.MapPost("/tasks", [Authorize] async (AtesTask newTask) =>
{
    await using var db = new TasksDb();
    
    newTask = new AtesTask
    {
        Id = newTask.Id, 
        PublicId = new Guid(), 
        Name = newTask.Name, 
        Description = newTask.Description, 
        Status = AtesTaskStatus.InProgress, 
        Assigned = GetRandomWorker()
    };
    
    await db.InsertAsync(newTask);
    await ProduceEventAsync(new TaskAssigned(newTask.PublicId, newTask.Assigned));
    await ProduceEventAsync(new StreamingEvent<AtesTask>(newTask));

    return Results.Ok();
});

app.MapPost("/tasks/{taskId}/complete", [Authorize(Roles = "Worker")] 
    async (ClaimsPrincipal user, int taskId) =>
{
    await using var db = new TasksDb();

    var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
    if (task?.Assigned != user.GetId())
    {
        return Results.NotFound(taskId.ToString());
    }
    if (task.Status == AtesTaskStatus.Completed)
    {
        return Results.BadRequest($"Task {taskId.ToString()} already completed");
    }

    await db.Tasks
        .Where(t => t.Id == taskId)
        .Set(t => t.Status, AtesTaskStatus.Completed)
        .UpdateAsync();
    
    await ProduceEventAsync(new TaskCompleted(task.PublicId));

    task.Status = AtesTaskStatus.Completed;
    await ProduceEventAsync(new StreamingEvent<AtesTask>(task));

    return Results.Ok();
});

app.MapPost("/tasks/shuffle", [Authorize(Roles = "Manager,Admin")] async () =>
{
    await using var db = new TasksDb();

    var tasks = await db.Tasks
        .Where(t => t.Status == AtesTaskStatus.InProgress)
        .ToListAsync();
    
    foreach(var task in tasks)
    {
        var assign = GetRandomWorker();
        if (task.Assigned == assign)
            continue;

        task.Assigned = assign;
        await db.Tasks
            .Where(t => t.Id == task.Id)
            .Set(t => t.Assigned, task.Assigned)
            .UpdateAsync();
        
        await ProduceEventAsync(new TaskAssigned(task.PublicId, assign));
    }
    
    return Results.Ok();
});

Guid GetRandomWorker()
{
    using var db = new TasksDb();
    var workers = db.Users
        .Where(u => u.Role == UserRole.Worker)
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
    logger.LogDebug("PRODUCE SOME EVENT!");
}

app.Run();
