using Discord;
using Discord.WebSocket;
using Gatekeeper.Models;
using System.Collections.Generic;

namespace Gatekeeper.Services
{
    public class WhitelistAppService
    {
        private readonly DiscordSocketClient _client;
        private readonly DatabaseService _databaseService;
        private readonly RankingService _ranking;
        private readonly Dictionary<ulong, WhitelistApp> appMap;

        public WhitelistAppService(DiscordSocketClient client, DatabaseService database, RankingService ranking)
        {
            _client = client;
            _databaseService = database;
            _ranking = ranking;
            appMap = new Dictionary<ulong, WhitelistApp>();
            Load();
        }

        private void Load()
        { 
            List<WhitelistApp> apps = _databaseService.GetActiveWhitelistApps();
            foreach (WhitelistApp app in apps)
            {
                // Populate the existing fields and grab the SocketUser from the client
                app.User = _client.GetUser(app.ApplicantDiscordId);
                appMap.Add(app.ApplicantDiscordId, app);
            }
        }

        public void BeginApplication(SocketUser user)
        {
            appMap.Add(user.Id, new WhitelistApp(user));
            //_databaseService.InsertApplication(appMap[user.Id]);
        }

        public void CloseApplication(SocketUser user)
        {
            //_databaseService.SetApplicationInactive(appMap[user.Id]);
            appMap.Remove(user.Id);
            if (_ranking.Remove(user))
                _ranking.Save();
        }

        public WhitelistApp GetApp(ulong discordId)
        {
            return appMap.GetValueOrDefault(discordId);
        }

        public bool UserHasActiveApplication(ulong discordId)
        {
            return appMap.ContainsKey(discordId);
        }
    }
}
