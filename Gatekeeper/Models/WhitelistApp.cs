using Discord;
using Discord.WebSocket;
using Gatekeeper.Models.Modals;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Gatekeeper.Models
{
    public class WhitelistApp
    {
        public int AppId { get; private set; }
        public string InGameName { get; private set; }
        public string Location { get; private set; }
        public int Age { get; private set; }
        public string Friend { get; private set; }
        public string BannedElsewhere { get; private set; }
        public string LookingFor { get; private set; }
        public string LoveHate { get; private set; }
        public string Intro { get; private set; }
        public string SecretWord { get; set; }
        public ulong ApplicantDiscordId { get; set; }
        public SocketUser User { get; set; }
        public Guid? MinecraftUuid { get; private set; }
        
        private string emptyChar = "\u200b";

        public WhitelistApp(SocketUser user) 
        {
            User = user;
            ApplicantDiscordId = user.Id;
        }

        public WhitelistApp(int appId, string inGameName, string location, int age, string friend, string bannedElsewhere,
            string lookingFor, string loveHate, string intro, string secretWord, ulong applicantDiscordId, Guid? minecraftUuid)
        {
            AppId = appId;
            InGameName = inGameName;
            Location = location;
            Age = age;
            Friend = friend;
            BannedElsewhere = bannedElsewhere;
            LookingFor = lookingFor;
            LoveHate = loveHate;
            Intro = intro;
            SecretWord = secretWord;
            ApplicantDiscordId = applicantDiscordId;
            MinecraftUuid = minecraftUuid;
        }

        public void FillInfoFromModal(string ign, string location, string ageString, string friend)
        {
            MinecraftUser user = GetMinecraftUser(ign).Result;
            if (user != null)
            {
                InGameName = user.Name;
                MinecraftUuid = Guid.Parse(user.Uuid);
            }
            else
            {
                // it's possible they had a correct name before and changed it to an invalid one, empty the properties
                InGameName = null;
                MinecraftUuid = null;
            }

            Location = location;
            Friend = friend;

            // There is no input validation for numerical values on Discord's modals so if the input value is not numeric we use TryParse to set age to 0
            int.TryParse(ageString, out int age);
            Age = age;
    
        }

        public void FillEssayFromModal(string bannedElsewhere, string lookingFor, string loveHate, string intro)
        {
            BannedElsewhere = bannedElsewhere;
            LookingFor = lookingFor;
            LoveHate = loveHate;
            Intro = intro;
        }

        public Embed BuildApplicationEmbed()
        {
            Color color = Color.Red;
            if (IsCompleted())
                color = Color.Green;
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle($"Whitelist Application for {InGameName ?? "[IGN]"}");
            builder.WithThumbnailUrl(User.GetAvatarUrl());
            builder.WithColor(color);
            builder.WithDescription($"**Discord Name**: `{User.Username}#{User.Discriminator}`\n" +
                $"**Discord ID**: `{User.Id}`");
            builder.AddField("Minecraft IGN", InGameName ?? emptyChar);
            builder.AddField("Where are you from?", Location ?? emptyChar);
            builder.AddField("How old are you?", Age > 0 ? Age.ToString() : emptyChar);
            builder.AddField("Do you know someone in our community?", Friend ?? emptyChar);
            builder.AddField("Have you been banned elsewhere before?", BannedElsewhere ?? emptyChar);
            builder.AddField("What are you looking for in a Minecraft community?", LookingFor ?? emptyChar);
            builder.AddField("What do you love and/or hate about Minecraft?", LoveHate ?? emptyChar);
            builder.AddField("Tell us something about yourself.", Intro ?? emptyChar);
            builder.AddField("What is the secret word?", SecretWord ?? emptyChar);
            builder.WithFooter($"UUID: {(MinecraftUuid == Guid.Empty ? emptyChar : MinecraftUuid.ToString())}");
            return builder.Build();
        }

        private async Task<MinecraftUser> GetMinecraftUser(string username)
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    var json = await client.DownloadStringTaskAsync(new Uri($"https://api.mojang.com/users/profiles/minecraft/{username}"));
                    if (json == "") 
                        return null;
                    return JsonSerializer.Deserialize<MinecraftUser>(json);
                }
                catch (WebException)
                {
                    return null;
                }
            }

        }

        public bool InfoQuestionsCompleted()
        {
            return InGameName != null
                && MinecraftUuid != Guid.Empty
                && Location != null
                && Age > 0
                && Friend != null;
        }

        public bool EssayQuestionsCompleted()
        {
            return BannedElsewhere != null
                && LookingFor != null
                && LoveHate != null
                && Intro != null;
        }

        public bool IsCompleted()
        {
            return InfoQuestionsCompleted() && EssayQuestionsCompleted() && SecretWord != null;
        }

        private class MinecraftUser
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("id")]
            public string Uuid { get; set; }
        }
    }
}
