using Common;
using LinqToDB;
using LinqToDB.Mapping;

namespace TaskService;

public class TasksDb : LinqToDB.Data.DataConnection
{
    public TasksDb() : base(ProviderName.PostgreSQL15, "") { }

    public ITable<UserDto> Users => this.GetTable<UserDto>();
    
    public ITable<AtesTaskDto> Tasks => this.GetTable<AtesTaskDto>();
}