using Microsoft.Data.SqlClient;

namespace Directo.Wari.TarifaEngine.Infrastructure.Persistence.Helpers
{
    public static class SqlDataReaderExtensions
    {
        public static bool HasColumn(this SqlDataReader reader, string column)
        {
            for (int i = 0; i < reader.FieldCount; i++)
                if (reader.GetName(i).Equals(column, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        public static string? GetNullableString(this SqlDataReader reader, string column)
        {
            if (!reader.HasColumn(column)) return null;

            var ord = reader.GetOrdinal(column);
            if (reader.IsDBNull(ord)) return null;

            return reader.GetValue(ord)?.ToString();
        }

        public static int? GetNullableInt(this SqlDataReader reader, string column)
        {
            if (!reader.HasColumn(column)) return null;

            var ord = reader.GetOrdinal(column);
            if (reader.IsDBNull(ord)) return null;

            return Convert.ToInt32(reader.GetValue(ord));
        }

        public static decimal? GetNullableDecimal(this SqlDataReader reader, string column)
        {
            if (!reader.HasColumn(column)) return null;

            var ord = reader.GetOrdinal(column);
            if (reader.IsDBNull(ord)) return null;

            return Convert.ToDecimal(reader.GetValue(ord));
        }

        public static bool? GetNullableBool(this SqlDataReader reader, string column)
        {
            if (!reader.HasColumn(column)) return null;

            var ord = reader.GetOrdinal(column);
            if (reader.IsDBNull(ord)) return null;

            return Convert.ToBoolean(reader.GetValue(ord));
        }

        public static DateTime? GetNullableDateTime(this SqlDataReader reader, string column)
        {
            if (!reader.HasColumn(column)) return null;

            var ord = reader.GetOrdinal(column);
            if (reader.IsDBNull(ord)) return null;

            return Convert.ToDateTime(reader.GetValue(ord));
        }

        public static double? GetNullableDouble(this SqlDataReader reader, string column)
        {
            if (!reader.HasColumn(column)) return null;

            var ord = reader.GetOrdinal(column);
            if (reader.IsDBNull(ord)) return null;

            return Convert.ToDouble(reader.GetValue(ord));
        }

        public static bool GetBoolOrFalse(this SqlDataReader reader, string column)
            => reader.GetNullableBool(column) ?? false;

        public static int GetIntOrDefault(this SqlDataReader reader, string column, int defaultValue = 0)
            => reader.GetNullableInt(column) ?? defaultValue;

        public static decimal GetDecimalOrDefault(this SqlDataReader reader, string column, decimal defaultValue = 0)
            => reader.GetNullableDecimal(column) ?? defaultValue;
    }
}
