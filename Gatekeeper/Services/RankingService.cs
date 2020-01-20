using Discord.WebSocket;
using Gatekeeper.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

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

        public RankingService(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
            _applicants = new List<Applicant>();
        }
        public void Process(SocketMessage message)
        {
            // Does this user exist in the applicant list?
            if (!_applicants.Exists(a => a.DiscordId == message.Author.Id))
            {
                _applicants.Add(new Applicant(message.Author.Id, message.Author.Username, message.Author.DiscriminatorValue, 0));
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
            if(content.Length >= 20 && wordCount >= 5 && hasActualWords(content))
            {
                Score(content);
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

        private void Score(string message)
        {

        }

        private void Promote(SocketGuildUser user)
        {
            
        }
    }
}
