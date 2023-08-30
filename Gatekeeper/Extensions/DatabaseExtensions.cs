using MySqlConnector;
using System;

namespace Gatekeeper.Extensions
{
    public static class DatabaseExtensions
    {
        public static string SafeGetString(this MySqlDataReader rdr, string column)
        {
            if (!rdr.IsDBNull(rdr.GetOrdinal(column)))
                return rdr.GetString(column);
            return null;
        }
        public static Guid? SafeGetGuid(this MySqlDataReader rdr, string column)
        {
            if (!rdr.IsDBNull(rdr.GetOrdinal(column)))
                return rdr.GetGuid(column);
            return null;
        }

        public static DateTime? SafeGetDateTime(this MySqlDataReader rdr, string column)
        {
            if (!rdr.IsDBNull(rdr.GetOrdinal(column)))
                return rdr.GetDateTime(column);
            return null;
        }

        public static ulong SafeGetUInt64(this MySqlDataReader rdr, string column)
        {
            if (!rdr.IsDBNull(rdr.GetOrdinal(column)))
                return rdr.GetUInt64(column);
            return 0;
        }

        public static int SafeGetInt32(this MySqlDataReader rdr, string column)
        {
            if (!rdr.IsDBNull(rdr.GetOrdinal(column)))
                return rdr.GetInt32(column);
            return 0;
        }
    }

}
