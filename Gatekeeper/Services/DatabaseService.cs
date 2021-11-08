using Discord.WebSocket;
using Gatekeeper.Commands;
using Gatekeeper.Models;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gatekeeper.Services
{
    public class DatabaseService
    {
        private readonly string connectionString;
        private readonly ConfigService _config;

        public DatabaseService(IServiceProvider services)
        {
            _config = services.GetRequiredService<ConfigService>();
            connectionString = String.Format("server={0};userid={1};password={2};database={3};port=3306",
                _config.BotConfig.DatabaseIp,
                _config.BotConfig.DatabaseUsername,
                _config.BotConfig.Password,
                _config.BotConfig.Database);
        }

        public bool PlayerExists(SocketGuildUser user)
        {
            EMIPlayer player = new EMIPlayer();
            using var con = new MySqlConnection(connectionString);
            con.Open();

            var stm = String.Format("SELECT * FROM players WHERE discord_id = {0}", user.Id);
            var cmd = new MySqlCommand(stm, con);

            using (MySqlDataReader rdr = cmd.ExecuteReader())
            {
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        player.Id = rdr.GetInt32("player_id");
                        player.Name = rdr.GetString("player_name");
                        player.UUID = rdr.GetString("player_uuid");
                        player.DiscordId = rdr.GetInt64("discord_id");
                    }
                }
                rdr.Close();
            }

            return !(player.DiscordId == 0 || player == null);
        }

        public bool RemoveSync(SocketGuildUser user)
        {
            using var con = new MySqlConnection(connectionString);
            con.Open();

            var stm = String.Format("UPDATE players SET discord_id = NULL WHERE discord_id = {0}", user.Id);
            var cmd = new MySqlCommand(stm, con);

            int rowsAffected = cmd.ExecuteNonQuery();

            return rowsAffected > 0;
        }
    }
}
