using Common;
using DataModels;
using LinqToDB;
using LinqToDB.Mapping;

namespace AuthService;

public class AuthDb : LinqToDB.Data.DataConnection
{
    public AuthDb(string connectionString) : base(ProviderName.PostgreSQL15, connectionString) { }

    public ITable<User> Users => this.GetTable<User>();
}