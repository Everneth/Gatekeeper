using Discord.WebSocket;
using Gatekeeper.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace Gatekeeper.Services
{
    public class DatabaseService
    {
        private readonly string connectionString;
        private DiscordSocketClient _client;
        private readonly ConfigService _config;

        public DatabaseService(IServiceProvider services, ConfigService config, DiscordSocketClient client)
        {
            _client = client;
            _config = config;
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
                        player.Uuid = Guid.Parse(rdr.GetString("player_uuid"));
                        player.DiscordId = rdr.GetUInt64("discord_id");
                    }
                }
                rdr.Close();
            }

            return player.DiscordId != 0;
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

        public List<WhitelistApp> GetActiveWhitelistApps()
        {
            List<WhitelistApp> apps = new List<WhitelistApp>();
            using var con = new MySqlConnection(connectionString);
            con.Open();

            var stm = "SELECT * FROM applications WHERE app_active = 1";
            var cmd = new MySqlCommand(stm, con);

            using (MySqlDataReader rdr = cmd.ExecuteReader())
            {
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        // Populate the app and get the related SocketUser
                        WhitelistApp app = new WhitelistApp(rdr.GetString("mc_ign"),
                            rdr.GetString("location"),
                            rdr.GetInt32("age"),
                            rdr.GetString("friend"),
                            rdr.GetString("looking_for"),
                            rdr.GetString("has_been_banned"),
                            rdr.GetString("love_hate"),
                            rdr.GetString("intro"),
                            rdr.GetString("secret_word"),
                            rdr.GetUInt64("applicant_discord_id"),
                            rdr.GetGuid("mc_uuid"));
                        app.User = _client.GetUser(app.ApplicantDiscordId);
                        apps.Add(app);
                    }
                }
            }

            return apps;
        }

        public bool InsertApplication(WhitelistApp app)
        {
            using var con = new MySqlConnection(connectionString);
            con.Open();

            // We go ahead and insert the application row before it's been filled out in case the bot goes down before the application has been finished
            var stm = $"INSERT INTO applications (applicant_discord_id) VALUES ({app.ApplicantDiscordId})";
            var cmd = new MySqlCommand(stm, con);

            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        public bool UpdateApplication(WhitelistApp app)
        {
            using var con = new MySqlConnection(connectionString);
            con.Open();

            var stm = string.Format("UPDATE applications SET" +
                $" mc_ign = {app.InGameName}," +
                $" location = {app.Location}," +
                $" age = {app.Age}," +
                $" friend = {app.Friend}," +
                $" looking_for = {app.LookingFor}," +
                $" has_been_banned = {app.BannedElsewhere}," +
                $" love_hate = {app.LoveHate}," +
                $" intro = {app.Intro}," +
                $" secret_word = {app.SecretWord}," +
                $" mc_uuid = {app.MinecraftUuid}," +
                $" WHERE applicant_discord_id = {app.ApplicantDiscordId}");
            var cmd = new MySqlCommand(stm, con);

            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        public bool SetApplicationInactive(WhitelistApp app)
        {
            using var con = new MySqlConnection(connectionString);
            con.Open();

            var stm = $"UPDATE applications SET app_active = 0 WHERE applicant_discord_id = {app.ApplicantDiscordId}";
            var cmd = new MySqlCommand(stm, con);

            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        public EMIPlayer GetEMIPlayer(ulong discordId)
        {
            using var con = new MySqlConnection(connectionString);
            con.Open();

            var stm = $"SELECT * FROM players WHERE discord_id = {discordId}";
            var cmd = new MySqlCommand(stm, con);

            MySqlDataReader reader = cmd.ExecuteReader();
            return new EMIPlayer(reader.GetInt32("player_id"),
                reader.GetString("player_name"),
                Guid.Parse(reader.GetString("player_uuid")),
                reader.GetString("alt_name"),
                Guid.Parse(reader.GetString("alt_uuid")),
                reader.GetDateTime("date_alt_added"),
                reader.GetUInt64("discord_id"),
                reader.GetDateTime("date_can_next_refer"));
        }

        public bool UpdateEMIPlayer(EMIPlayer player)
        {
            using var con = new MySqlConnection(connectionString);
            con.Open();

            string dateFormat = "yyyy-MM-dd HH:mm:ss";
            var stm = $"UPDATE players SET" +
                $" player_name = {player.Name}," +
                $" player_uuid = {player.Uuid}," +
                $" alt_name = {player.AltName}," +
                $" alt_uuid = {player.AltUuid}," +
                $" date_alt_added = {player.AltAdded.ToString(dateFormat)}," +
                $" discord_id = {player.DiscordId}," +
                $" date_can_next_refer = {player.CanNextReferFriend.ToString(dateFormat)}" +
                $" WHERE player_id = {player.Id}";
            var cmd = new MySqlCommand(stm, con);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
