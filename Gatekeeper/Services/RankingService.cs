using Discord.WebSocket;
using Gatekeeper.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gatekeeper.Services
{
    public class RankingService
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        private readonly List<Applicant> _applicants;
        private readonly ConfigService _config;
        private readonly DataService _data;

        public List<Applicant> Applicants
        {
            get { return _applicants; }
            set { value = _applicants; }
        }

        public RankingService(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _config = services.GetRequiredService<ConfigService>();
            _data = services.GetRequiredService<DataService>();
            _services = services;
            _applicants = _data.Load("applicants", _applicants);

            _client.MessageReceived += Process;
        }
        public async Task Process(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            SocketUserMessage message = messageParam as SocketUserMessage;
            if (message == null || message.Author == null || message.Author.IsBot) return;

            var user = message.Author as SocketGuildUser;

            // If the user is not an applicant we can ignore them
            if (!user.Roles.Any(role => role.Name == "Applicant")) return;

            // Does this user exist in the applicant list?
            if (!_applicants.Exists(a => a.DiscordId == message.Author.Id))
            {
                _applicants.Add(new Applicant(
                    message.Author.Id,
                    message.Author.Username,
                    message.Author.DiscriminatorValue,
                    0
                    ));
                _data.Save("applicants", _applicants);
            }

            // validate the message
            string content = message.Content;

            // get rid of leading and ending whitespace
            content = content.Trim();

            // trim extra spaces in between words
            while (content.Contains("  "))
                content = content.Replace("  ", " ");

            int wordCount = 0;

            // count "words"
            foreach ( string word in content.Split(' '))
            {
                ++wordCount;
            }

            // count 

            // validation
            if(content.Length >= _config.RankingConfig.BaseCharReq && wordCount >= _config.RankingConfig.RequiredWords && HasActualWords(content))
            {
                Score(content, message.Author);
            }
        }

        private bool HasActualWords(string content)
        {
            int numWords = 0;
            foreach (string word in content.Split(' '))
            {
                int numChars = 0;
                foreach (char value in word)
                {
                    numChars++;
                }
                if (numChars > 3)
                    numWords++;
            }
            if (numWords >= 3)
                return true;
            else
                return false;
        }

        private void Score(string message, SocketUser user)
        {
            int score = 0;
            var applicant = _applicants.Find(u => u.DiscordId == user.Id);

            applicant.Score += ((message.Length / _config.RankingConfig.BaseCharReq) * _config.RankingConfig.AdditionalCharsScore) + _config.RankingConfig.BaseScore;

            if (applicant.Score >= _config.RankingConfig.PromoThreshold)
            {
                // Our applicant has hit the promotion threshold, give them the pending rank
                var role = (user as SocketGuildUser).Guild.Roles.SingleOrDefault(r => r.Name == "Pending");
                (user as SocketGuildUser).AddRoleAsync(role);
            }
            else
                _data.Save("applicants", _applicants);
        }

        // debug command logic
        public bool Remove(SocketUser user)
        {
            var appToRemove = Applicants.SingleOrDefault(a => a.DiscordId == user.Id);
            if (appToRemove.Equals(null))
                return false;
            else { Applicants.Remove(appToRemove); return true; }
        }

        public bool Remove(Applicant user)
        {
            if (user.Equals(null))
                return false;
            else { Applicants.Remove(user); return true; }
        }

        public string Clean(SocketGuild guild)
        {
            int numErrors = 0, numSuccesses = 0;
            var role = guild.Roles.SingleOrDefault(r => r.Name == "Applicant");
            foreach (var user in _applicants.ToList())
            {
                var socketUser = _client.GetUser(user.DiscordId);

                // Deleted user?
                if(socketUser == null)
                {
                    if (_applicants.Remove(user))
                        ++numSuccesses;
                    else
                        ++numErrors;
                }
                // Is the user in the guild?
                else if(guild.GetUser(user.DiscordId) == null)
                {
                    if (_applicants.Remove(user))
                        ++numSuccesses;
                    else
                        ++numErrors;
                }
                // Does the user exist, but is not an applicant anymore?
                else if (!guild.GetUser(user.DiscordId).Roles.Contains(role))
                {
                    if (_applicants.Remove(user))
                        ++numSuccesses;
                    else
                        ++numErrors;
                }
            }
            _data.Save("applicants", _applicants);
            return "Clean completed. There were " + numSuccesses +" orphaned data. " +
                "Error(s): " + numErrors;
        }

        public void Save()
        {
            _data.Save("applicants", _applicants);
        }
    }
}
