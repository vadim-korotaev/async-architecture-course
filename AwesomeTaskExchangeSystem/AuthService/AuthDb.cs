using DataModels;
using LinqToDB;

namespace AuthService;

public class AuthDb : LinqToDB.Data.DataConnection
{
    public AuthDb(string connectionString) : base(ProviderName.PostgreSQL15, connectionString) { }

    public ITable<User> Users => this.GetTable<User>();
}