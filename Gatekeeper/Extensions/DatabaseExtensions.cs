using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

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
    }

}
