using Common;
using LinqToDB;
using LinqToDB.Mapping;

namespace AuthService;

public class DataConnection : LinqToDB.Data.DataConnection
{
    public DataConnection() : base(ProviderName.PostgreSQL15, "") { }

    public ITable<User> Users => this.GetTable<User>();
}