using Microsoft.Data.SqlClient;
using System.Data;

namespace Directo.Wari.TarifaEngine.Infrastructure.Persistence.Helpers
{
    public static class SqlParameterHelper
    {
        public static void AddParameter(SqlCommand command, string name, SqlDbType type, object? value)
        {
            command.Parameters.Add(name, type).Value = value ?? DBNull.Value;
        }

        public static bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
