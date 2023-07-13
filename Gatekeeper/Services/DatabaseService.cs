using Discord.WebSocket;
using Gatekeeper.Extensions;
using Gatekeeper.Models;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
                        player.Name = rdr.SafeGetString("player_name");
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
                        WhitelistApp app = new WhitelistApp(rdr.GetInt32("app_id"),
                            rdr.SafeGetString("mc_ign"),
                            rdr.SafeGetString("location"),
                            rdr.GetInt32("age"),
                            rdr.SafeGetString("friend"),
                            rdr.SafeGetString("looking_for"),
                            rdr.SafeGetString("has_been_banned"),
                            rdr.SafeGetString("love_hate"),
                            rdr.SafeGetString("intro"),
                            rdr.SafeGetString("secret_word"),
                            rdr.GetBoolean("is_approved"),
                            rdr.GetUInt64("applicant_discord_id"),
                            rdr.SafeGetGuid("mc_uuid"));
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

            var stm = string.Format("UPDATE applications SET" +
                $" mc_ign = @InGameName," +
                $" location = @Location," +
                $" age = {app.Age}," +
                $" friend = @Friend," +
                $" looking_for = @LookingFor," +
                $" has_been_banned = @BannedElsewhere," +
                $" love_hate = @LoveHate," +
                $" intro = @Intro," +
                $" secret_word = @SecretWord," +
                $" is_approved = @Approved," +
                $" mc_uuid = @MinecraftUuid" +
                $" WHERE applicant_discord_id = {app.ApplicantDiscordId} AND app_active = 1");
            using var cmd = new MySqlCommand(stm, con);

            // Parametrizing the queries because we need quotations around strings only when they are not null
            cmd.Parameters.AddWithValue("@InGameName", app.InGameName);
            cmd.Parameters.AddWithValue("@Location", app.Location);
            cmd.Parameters.AddWithValue("@Friend", app.Friend);
            cmd.Parameters.AddWithValue("@LookingFor", app.LookingFor);
            cmd.Parameters.AddWithValue("@BannedElsewhere", app.BannedElsewhere);
            cmd.Parameters.AddWithValue("@LoveHate", app.LoveHate);
            cmd.Parameters.AddWithValue("@Intro", app.Intro);
            cmd.Parameters.AddWithValue("@SecretWord", app.SecretWord);
            cmd.Parameters.AddWithValue("@Approved", app.IsApproved ? 1 : 0);
            cmd.Parameters.AddWithValue("@MinecraftUuid", app.MinecraftUuid);

            con.Open();
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

        public bool InsertEMIPlayer(WhitelistApp app)
        {
            using var con = new MySqlConnection(connectionString);

            string pattern = @"\<@!?(\d+)\>";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(app.Friend);
            EMIPlayer friend = null;
            if (match.Success)
                friend = GetEMIPlayer(ulong.Parse(match.Groups[1].Value));

            con.Open();
            string stm = $"INSERT INTO players (player_name, player_uuid, discord_id, date_referred, referred_by) " +
                $"VALUES (@ign, @uuid, @discordId, @dateReferred, @referredBy)";
            var cmd = new MySqlCommand(stm, con);
            cmd.Parameters.AddWithValue("@ign", app.InGameName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@uuid", app.MinecraftUuid ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@discordId", app.ApplicantDiscordId);
            cmd.Parameters.AddWithValue("@dateReferred", friend != null ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : null);
            cmd.Parameters.AddWithValue("@referredBy", friend != null ? (int?)friend.Id : 0);
            return cmd.ExecuteNonQuery() > 0;
        }

        public EMIPlayer GetEMIPlayer(ulong discordId)
        {
            return RowToEMIPlayer($"SELECT * FROM players WHERE discord_id = {discordId}");
        }

        public EMIPlayer GetEMIPlayer(int playerId)
        {
            return RowToEMIPlayer($"SELECT * FROM players WHERE player_id = {playerId}");
        }

        public EMIPlayer GetEMIPlayer(string playerName)
        {
            return RowToEMIPlayer($"SELECT * FROM players WHERE player_name = {playerName}");
        }

        private EMIPlayer RowToEMIPlayer(string stm)
        {
            using var con = new MySqlConnection(connectionString);
            con.Open();

            var cmd = new MySqlCommand(stm, con);

            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                // Should never cause stack overflow as friends cannot refer each other
                EMIPlayer friend = GetEMIPlayer(reader.SafeGetInt32("referred_by"));
                return new EMIPlayer(reader.GetInt32("player_id"),
                    reader.SafeGetString("player_name"),
                    reader.SafeGetGuid("player_uuid"),
                    reader.SafeGetString("alt_name"),
                    reader.SafeGetGuid("alt_uuid"),
                    reader.SafeGetDateTime("date_alt_added"),
                    reader.SafeGetUInt64("discord_id"),
                    reader.SafeGetDateTime("date_can_next_refer"),
                    friend,
                    reader.SafeGetDateTime("date_referred"));
            }
            return null;
        }

        public bool UpdateEMIPlayer(EMIPlayer player)
        {
            using var con = new MySqlConnection(connectionString);
            con.Open();

            string dateFormat = "yyyy-MM-dd HH:mm:ss";
            var stm = $"UPDATE players SET" +
                $" player_name = @Name," +
                $" player_uuid = @PlayerUuid," +
                $" alt_name = @AltName," +
                $" alt_uuid = @AltUuid," +
                $" date_alt_added = @AltAdded," +
                $" discord_id = {player.DiscordId}," +
                $" date_can_next_refer = @CanNextReferFriend," +
                $" referred_by = @ReferredBy," +
                $" date_referred = @DateReferred" +
                $" WHERE player_id = {player.Id}";
            using var cmd = new MySqlCommand(stm, con);

            // Parametrizing the queries because we need quotations around strings only when they are not null
            cmd.Parameters.AddWithValue("@Name", player.Name ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@PlayerUuid", player.Uuid ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@AltName", player.AltName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@AltUuid", player.AltUuid ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@AltAdded",
                player.AltAdded != null ? player.AltAdded.Value.ToString(dateFormat) : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CanNextReferFriend",
                player.CanNextReferFriend != null ? player.CanNextReferFriend.Value.ToString(dateFormat) : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ReferredBy",
                player.ReferredBy != null ? player.ReferredBy.Id : 0);
            cmd.Parameters.AddWithValue("DateReferred",
                player.DateReferred != null ? player.DateReferred.Value.ToString(dateFormat) : (object)DBNull.Value);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
