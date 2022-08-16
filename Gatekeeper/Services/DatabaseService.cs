using Discord.WebSocket;
using Gatekeeper.Models;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

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
                        apps.Add(new WhitelistApp(rdr.GetString("mc_ign"),
                            rdr.GetString("location"),
                            rdr.GetInt32("age"),
                            rdr.GetString("friend"),
                            rdr.GetString("looking_for"),
                            rdr.GetString("has_been_banned"),
                            rdr.GetString("love_hate"),
                            rdr.GetString("intro"),
                            rdr.GetString("secret_word"),
                            rdr.GetUInt64("applicant_discord_id"),
                            rdr.GetGuid("mc_uuid")));
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
    }
}
