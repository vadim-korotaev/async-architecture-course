using Common;
using DataModels;
using LinqToDB;

namespace TaskService;

public class TasksDb : LinqToDB.Data.DataConnection
{
    public TasksDb(string connectionString) : base(ProviderName.PostgreSQL15, connectionString) { }

    public ITable<User> Users => this.GetTable<User>();
    
    public ITable<AtesTask> Tasks => this.GetTable<AtesTask>();
}