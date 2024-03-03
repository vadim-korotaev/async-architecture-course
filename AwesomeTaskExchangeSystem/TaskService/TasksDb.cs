using Common;
using LinqToDB;
using LinqToDB.Mapping;

namespace TaskService;

public class TasksDb : LinqToDB.Data.DataConnection
{
    public TasksDb() : base(ProviderName.PostgreSQL15, "") { }

    public ITable<User> Users => this.GetTable<User>();
    
    public ITable<AtesTask> Tasks => this.GetTable<AtesTask>();
}