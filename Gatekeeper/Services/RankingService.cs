using Discord.WebSocket;
using Gatekeeper.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

namespace Gatekeeper.Services
{
    public class RankingService
    {
        private DiscordSocketClient _client;
        private IServiceProvider _services;
        private List<Applicant> _applicants;
        private ConfigService _config;
        private DataService _data;

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
        }
        public void Process(SocketMessage message)
        {
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
            if(content.Length >= _config.Config.BaseCharReq && wordCount >= _config.Config.RequiredWords && hasActualWords(content))
            {
                Score(content, message.Author);
            }
        }

        private bool hasActualWords(string content)
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

            applicant.Score += ((message.Length / _config.Config.BaseCharReq) * _config.Config.AdditionalCharsScore) + _config.Config.BaseScore;

            if (applicant.Score >= _config.Config.PromoThreshold)
                Promote(user as SocketGuildUser);
            else
                _data.Save("applicants", _applicants);
        }

        private void Promote(SocketGuildUser user)
        {
            var role = user.Guild.Roles.SingleOrDefault(r => r.Name == "Pending");
            user.AddRoleAsync(role);
            user.RemoveRoleAsync(user.Guild.Roles.SingleOrDefault(r => r.Name == "Applicant"));

            // Once added to pending group, remove from ranking service
            // No more tracking needed
            if(Remove(user))
                _data.Save("applicants", _applicants);
        }
        // debug command logic
        public bool Remove(SocketGuildUser user)
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
    }
}
