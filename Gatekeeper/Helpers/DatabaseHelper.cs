using Discord.WebSocket;
using Gatekeeper.Models;
using MySql.Data.MySqlClient;
using System;

namespace Gatekeeper.Helpers
{
    public static class DatabaseHelper
    {
        public static bool PlayerExists(SocketGuildUser user)
        {
            string cs = String.Format("server=localhost;userid=admin_emi;password={0};database=admin_emi", "lWw17l8V0W");
            EMIPlayer player = new EMIPlayer();
            using (var con = new MySqlConnection(cs))
            {
                con.Open();

                var stm = String.Format("SELECT * FROM players WHERE discord_id = {0}", user.Id);
                var cmd = new MySqlCommand(stm, con);

                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.HasRows)
                    {
                        while (rdr.Read())
                        {
                            player.Id = rdr.GetInt32(0);
                            player.Name = rdr.GetString(1);
                            player.UUID = rdr.GetString(2);
                            //player.MemberId = rdr.GetInt32(3);
                            player.DiscordId = rdr.GetInt64(4);
                        }
                    }
                }

                if (player.DiscordId == 0 || player == null)
                    return false;
                else
                    return true;
            }
        }
    }
}
