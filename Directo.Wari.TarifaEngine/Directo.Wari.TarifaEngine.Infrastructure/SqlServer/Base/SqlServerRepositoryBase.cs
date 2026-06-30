using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Directo.Wari.TarifaEngine.Infrastructure.SqlServer.Base
{
    public abstract class SqlServerRepositoryBase
    {
        protected readonly string ConnectionString;
        protected const int DefaultCommandTimeout = 120;

        protected SqlServerRepositoryBase(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("LegacyConnection")!;
        }

        protected SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        protected SqlCommand CreateStoredProcedure(SqlConnection connection, string spName)
        {
            var command = connection.CreateCommand();
            command.CommandText = spName;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = DefaultCommandTimeout;

            return command;
        }
    }
}
