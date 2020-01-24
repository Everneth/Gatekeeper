using Discord.WebSocket;
using Gatekeeper.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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

        public List<Applicant> Applicants
        {
            get { return _applicants; }
            set { value = _applicants; }
        }

        private const int BASE_SCORE = 5;
        private const int ADDITIONAL_CHARS_SCORE = 1;
        private const int PROMOTION_THRESHOLD = 40;
        private const int BASE_CHAR_REQ = 20;
        private const int REQUIRED_WORDS = 5;

        public RankingService(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
            _applicants = Load();
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
                Save();
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
            if(content.Length >= BASE_CHAR_REQ && wordCount >= REQUIRED_WORDS && hasActualWords(content))
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

            applicant.Score += ((message.Length / BASE_CHAR_REQ) * ADDITIONAL_CHARS_SCORE) + BASE_SCORE;

            if (applicant.Score >= PROMOTION_THRESHOLD)
                Promote(user as SocketGuildUser);
            else
                Save();
        }

        private void Promote(SocketGuildUser user)
        {
            var role = user.Guild.Roles.SingleOrDefault(r => r.Name == "Pending");
            user.AddRoleAsync(role);
            user.RemoveRoleAsync(user.Guild.Roles.SingleOrDefault(r => r.Name == "Applicant"));

            // Once added to pending group, remove from ranking service
            // No more tracking needed
            if(Remove(user))
                Save();
        }

        private List<Applicant> Load()
        {
            using (StreamReader file = File.OpenText(@"..\..\..\Data\applicants.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                return Applicants = (List<Applicant>)serializer.Deserialize(file, typeof(List<Applicant>));
            }
        }

        public void Save()
        {
            using (StreamWriter file = File.CreateText(@"..\..\..\Data\applicants.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, Applicants);
            }
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
    }
}
