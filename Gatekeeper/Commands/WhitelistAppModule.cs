using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Gatekeeper.Models;
using Gatekeeper.Models.Modals;
using Gatekeeper.Services;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gatekeeper.Commands
{
    public class WhitelistAppModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly ConfigService _config;
        private readonly WhitelistAppService _whitelist;
        private readonly DatabaseService _database;

        private readonly Emoji invalidEmoji = new Emoji("\U0000274C");
        private readonly Emoji validEmoji = new Emoji("\U00002705");

        public WhitelistAppModule(DiscordSocketClient client, ConfigService config, WhitelistAppService whitelist, DatabaseService database)
        {
            _client = client;
            _config = config;
            _whitelist = whitelist;
            _database = database;
        }

        [ComponentInteraction("apply")]
        public async Task BeginApplication()
        {
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);
            // We do not want users to open a second application before the vote on their current one has concluded
            if (_whitelist.UserHasActiveApplication(user.Id) || user.Roles.Where(role => role.Name == "Pending").Count() > 0)
            {
                await RespondAsync("You already have an active application!", ephemeral: true);
                return;
            }
            else if (user.Roles.Where(role => role.Name == "Citizen").Count() > 0)
            {
                await RespondAsync("You're already whitelisted, you don't need to apply!", ephemeral: true);
                return;
            }

            IDMChannel channel = await Context.User.CreateDMChannelAsync();
            try
            {
                WhitelistApp emptyApp = new WhitelistApp(Context.User);
                await channel.SendMessageAsync("Here is your application preview. Use the buttons to fill out the form and hit the green button when you feel it's ready to send off!",
                    embed: emptyApp.BuildApplicationEmbed(),
                    components: BuildApplicationComponents());
                _whitelist.BeginApplication(user as SocketUser);
                await DeferAsync();
            }
            catch (HttpException)
            {
                await RespondAsync("I need to be able to message you! **Right click server icon \\> Privacy Settings \\> Allow Direct Messages from Server Members** " +
                    "and then try again", ephemeral: true);
            }
        }

        [ComponentInteraction("secret")]
        public async Task OnSecretWordSelected(string[] selectedWord)
        {
            await DeferAsync();
            WhitelistApp app = _whitelist.GetApp(Context.User.Id);
            // If the application is null but the user can interact with components the message must be deleted
            if (app == null)
            {
                await DeleteOriginalResponseAsync();
                return;
            }

            app.SecretWord = selectedWord[0];
            await ModifyOriginalResponseAsync(message =>
            {
                message.Embeds = new Embed[] { app.BuildApplicationEmbed() };
                message.Components = BuildApplicationComponents();
            });
            _database.UpdateApplication(app);
        }

        [ComponentInteraction("app1")]
        public async Task OnInfoModalRequest()
        {
            WhitelistApp app = _whitelist.GetApp(Context.User.Id);
            // If the application is null but the user can interact with components the message must be deleted
            if (app == null)
            {
                await DeleteOriginalResponseAsync();
                return;
            }
            ModalBuilder builder = new ModalBuilder();
            builder.WithCustomId("info")
                .WithTitle("Basic Applicant Information")
                .AddTextInput(new TextInputBuilder()
                {
                    Label = "Minecraft IGN",
                    CustomId = "first",
                    Placeholder = "e.g. Notch",
                    MinLength = 3,
                    MaxLength = 16,
                    Value = app.InGameName
                })
                .AddTextInput(new TextInputBuilder()
                {
                    Label = "Where are you from?",
                    CustomId = "second",
                    Placeholder = "e.g. Florida, USA",
                    MinLength = 2,
                    MaxLength = 50,
                    Value = app.Location
                })
                .AddTextInput(new TextInputBuilder()
                {
                    Label = "How old are you?",
                    CustomId = "third",
                    MinLength = 1,
                    MaxLength = 2,
                    Value = app.Age > 0 ? app.Age.ToString() : null
                })
                .AddTextInput(new TextInputBuilder()
                {
                    Label = "Know someone here? Enter just their username",
                    CustomId = "fourth",
                    Placeholder = "e.g. wumpus",
                    MinLength = 2,
                    MaxLength = 32,
                    Value = app.Friend
                });
            await RespondWithModalAsync(builder.Build());
        }

        [ComponentInteraction("app2")]
        public async Task OnEssayModalRequested()
        {
            WhitelistApp app = _whitelist.GetApp(Context.User.Id);
            // If the application is null but the user can interact with components the message must be deleted
            if (app == null)
            {
                await DeleteOriginalResponseAsync();
                return;
            }
            ModalBuilder builder = new ModalBuilder();
            builder.WithCustomId("essay")
                .WithTitle("Digging a Little Deeper")
                .AddTextInput(new TextInputBuilder()
                {
                    Label = "Have you been banned elsewhere before?",
                    CustomId = "first",
                    MaxLength = 100,
                    Value = app.BannedElsewhere
                })
                .AddTextInput(new TextInputBuilder()
                {
                    Label = "What do you want in a Minecraft community?",
                    CustomId = "second",
                    Style = TextInputStyle.Paragraph,
                    Placeholder = "Traits or qualities you'd like to see",
                    MinLength = 10,
                    Value = app.LookingFor
                })
                .AddTextInput(new TextInputBuilder()
                {
                    Label = "What do you love and/or hate about Minecraft?",
                    CustomId = "third",
                    Style = TextInputStyle.Paragraph,
                    Placeholder = "e.g. Love building, hate mining",
                    MinLength = 10,
                    Value = app.LoveHate
                })
                .AddTextInput(new TextInputBuilder()
                {
                    Label = "Tell us something about yourself.",
                    CustomId = "fourth",
                    Style = TextInputStyle.Paragraph, 
                    MinLength = 10,
                    Value = app.Intro
                });
            await RespondWithModalAsync(builder.Build());
        }

        [ComponentInteraction("sendapp")]
        public async Task SubmitApplication()
        {
            await DeferAsync();
            WhitelistApp app = _whitelist.GetApp(Context.User.Id);
            // If the application is null but the user can interact with components the message must be deleted
            if (app == null)
            {
                await DeleteOriginalResponseAsync();
                return;
            }
            SocketGuild guild = _client.GetGuild(_config.BotConfig.GuildId);
            SocketTextChannel channel = guild.GetTextChannel(_config.BotConfig.AppsChannelId);
            await channel.SendMessageAsync(embed: app.BuildApplicationEmbed());

            SocketRole applicant = guild.Roles.FirstOrDefault(role => role.Name == "Applicant");
            await guild.GetUser(Context.User.Id).AddRoleAsync(applicant);

            await ModifyOriginalResponseAsync(message =>
            {
                // Removing the buttons and select menu so they can no longer interact with them
                message.Components = new ComponentBuilder().Build();
                message.Content = "Application Complete!";
            });
            _database.InsertEMIPlayer(app);
            await Context.Channel.SendMessageAsync("We have received your application! Either get to makin' conversation or have your friend confirm they know you!");
            
            // The user has claimed they have a friend, friend confirmation message needs to be sent
            if (app.Friend.Contains('@'))
            {
                channel = guild.GetTextChannel(_config.BotConfig.GeneralChannelId);
                await channel.SendMessageAsync($"Hey {app.Friend}, {app.User.Mention} has claimed you as a friend. Please press one of the buttons to confirm/deny them.",
                    components: BuildFriendConfirmationComponents());
            }
        }

        [ModalInteraction("info")]
        public async Task AppInfoSubmission(GenericApplicationInputModal modal)
        {
            await DeferAsync();
            WhitelistApp app = _whitelist.GetApp(Context.User.Id);
            string username = modal.Fourth;
            // Get user matching username and discrim, will return null if they do not exist
            var friend = _client.GetGuild(_config.BotConfig.GuildId).Users.FirstOrDefault(
                user => user.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            modal.Fourth = "Nope";
            if (friend != null)
            {
                // Check that the user is a citizen
                if (friend.Roles.Where(role => role.Name.Equals("Citizen")).Count() > 0)
                {
                    modal.Fourth = friend.Mention;
                }
                else
                {
                    await FollowupAsync("The member you specified is not a citizen!", ephemeral: true);
                }

                // Check if user's referral restriction has expired
                EMIPlayer friendPlayer = _database.GetEMIPlayer(friend.Id);
                if (friendPlayer != null && friendPlayer.CanNextReferFriend != null && friendPlayer.CanNextReferFriend > DateTime.Now)
                {
                    long unixEpoch = new DateTimeOffset(friendPlayer.CanNextReferFriend.Value.ToUniversalTime()).ToUnixTimeSeconds();
                    modal.Fourth = "Nope";
                    await FollowupAsync($"{friend.Mention} cannot confirm another friend until <t:{unixEpoch}:f>." +
                        $" If you know someone else you may use them instead.", ephemeral: true);
                }
            }
            else
                await FollowupAsync($"Could not find user {username}", ephemeral: true);

            app.FillInfoFromModal(modal.First, modal.Second, modal.Third, modal.Fourth);
            await ModifyOriginalResponseAsync(message =>
            {
                message.Embeds = new Embed[] { app.BuildApplicationEmbed() };
                message.Components = BuildApplicationComponents();
            });
            _database.UpdateApplication(app);
        }

        [ModalInteraction("essay")]
        public async Task AppEssaySubmission(GenericApplicationInputModal modal)
        {
            await DeferAsync();
            WhitelistApp app = _whitelist.GetApp(Context.User.Id);
            app.FillEssayFromModal(modal.First, modal.Second, modal.Third, modal.Fourth);
            await ModifyOriginalResponseAsync(message =>
            {
                message.Embeds = new Embed[] { app.BuildApplicationEmbed() };
                message.Components = BuildApplicationComponents();
            });
            _database.UpdateApplication(app);
        }

        [ComponentInteraction("confirmation")]
        public async Task FriendConfirmation()
        {
            await DeferAsync();
            // Need to grab discord ids from confirmation message to parse to users <@ulong>, <@ulong>
            ParseUsers(out SocketGuildUser friend, out SocketGuildUser applicant);
            
            // The friend either took too long or one of the users left
            if (friend is null || applicant is null || !_whitelist.UserHasActiveApplication(applicant.Id))
            {
                await DeleteOriginalResponseAsync();
                return;
            }

            if (Context.User.Id == applicant.Id)
            {
                await FollowupAsync("You cannot confirm yourself...", ephemeral: true);
                return;
            }
            else if (Context.User.Id != friend.Id)
            {
                await FollowupAsync("You cannot confirm this applicant.", ephemeral: true);
                return;
            }

            await ModifyOriginalResponseAsync(message =>
            {
                message.Content = $"{friend.Mention} has confirmed {applicant.Mention} as a friend.";
                message.Components = BuildFriendConfirmationComponents(true);
            });

            // Add pending which starts the whitelist vote and insert 
            await applicant.AddRoleAsync(Context.Guild.Roles.Where(role => role.Name == "Pending").First());

            EMIPlayer friendPlayer = _database.GetEMIPlayer(friend.Id);
            friendPlayer.CanNextReferFriend = DateTime.Now.AddDays(3.0f);
            _database.UpdateEMIPlayer(friendPlayer);
        }

        [ComponentInteraction("denial")]
        public async Task FriendDenial()
        {
            await DeferAsync();

            // Need to grab discord ids from confirmation message to parse to users <@ulong>, <@ulong>
            ParseUsers(out SocketGuildUser friend, out SocketGuildUser applicant);

            if (Context.User.Id == applicant.Id)
            {
                await FollowupAsync("Trying to deny yourself, eh?", ephemeral: true);
                return;
            }
            else if (Context.User.Id != friend.Id)
            {
                await FollowupAsync("Stop pushing my buttons.", ephemeral: true);
                return;
            }

            // Just delete the message if they have denied friend accountability, remove accountability, and message the applicant
            await DeleteOriginalResponseAsync();
            EMIPlayer applicantPlayer = _database.GetEMIPlayer(applicant.Id);
            applicantPlayer.ReferredBy = null;
            applicantPlayer.DateReferred = null;
            _database.UpdateEMIPlayer(applicantPlayer);
            await applicant.SendMessageAsync($"{friend.Mention} has denied you as a friend. You will have to speak to our members to trigger a whitelist vote instead.");
        }

        private MessageComponent BuildApplicationComponents()
        {
            WhitelistApp app = _whitelist.GetApp(Context.User.Id);
            bool infoCompleted, essayCompleted, appCompleted;
            infoCompleted = essayCompleted = appCompleted = false;
            if (app != null)
            {
                appCompleted = app.IsCompleted();
                infoCompleted = app.InfoQuestionsCompleted();
                essayCompleted = app.EssayQuestionsCompleted();
            }

            SelectMenuBuilder menuBuilder = new SelectMenuBuilder()
                .WithCustomId($"secret")
                .WithPlaceholder("Choose the Secret Word")
                .WithMinValues(1)
                .WithMaxValues(1)
                .AddOption("Zombie", "Zombie")
                .AddOption("Skeleton", "Skeleton")
                .AddOption("Creeper", "Creeper")
                .AddOption("Phantom", "Phantom")
                .AddOption("Pillager", "Pillager")
                .AddOption("Dolphin", "Dolphin")
                .AddOption("Allay", "Allay")
                .AddOption("Ocelot", "Ocelot");
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithSelectMenu(menuBuilder);
            builder.WithButton("Basic Info", "app1", ButtonStyle.Secondary, emote: infoCompleted ? validEmoji : invalidEmoji);
            builder.WithButton("Digging Deeper", "app2", ButtonStyle.Secondary, emote: essayCompleted ? validEmoji : invalidEmoji);
            builder.WithButton("Submit Application", "sendapp", ButtonStyle.Success, emote: validEmoji, disabled: !appCompleted);
            return builder.Build();
        }

        private MessageComponent BuildFriendConfirmationComponents(bool buttonsDisabled = false)
        {
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Confirm", "confirmation", ButtonStyle.Success, emote: validEmoji, disabled: buttonsDisabled);
            builder.WithButton("Deny", "denial", ButtonStyle.Danger, emote: invalidEmoji, disabled: buttonsDisabled);
            return builder.Build();
        }

        private void ParseUsers(out SocketGuildUser friend, out SocketGuildUser applicant)
        {
            // Only works because we can guarantee this interaction is off a message component
            string message = (Context.Interaction as SocketMessageComponent).Message.Content;

            // Has two capture groups for Discord Ids from the interaction message
            // Mentions will be kept in raw text in the form of <@12345> or <@!12345> if they're nicked
            string pattern = @"\<@!?(\d+)\>.*\<@!?(\d+)\>";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(message);

            friend = Context.Guild.GetUser(ulong.Parse(match.Groups[1].Value));
            applicant = Context.Guild.GetUser(ulong.Parse(match.Groups[2].Value));
        }

        [SlashCommand("sendapplymessage", "Send the application message for the apply channel.")]
        public async Task SendApplyMessage()
        {
            var channel = Context.Channel;
            var components = new ComponentBuilder();
            components.WithButton("Apply now!", "apply");
            components.WithButton("Everneth's Rules", style: ButtonStyle.Link, url: "https://everneth.com/rules");
            await channel.SendMessageAsync("Hey there! \r\nMy name is Jasper. Welcome to the <:everneth:230340926608900096> **Everneth SMP** Discord server!" +
                "\r\nTo get you started I have some information on how to apply using discord! " +
                "Just follow these simple steps to get your application in.\r\n" +
                "\r\n**1)** Please visit our website and at a minimum read Sections 1, 2, 4, and 5 of the rules (linked below). There are more in the rules but the rest of the sections cover rules for staff and how to change our rules." +
                "\r\n**2)** Make note of the secret word (Found in the rules!). You will need this for your application to be accepted." +
                "\r\n**3)** When ready, click the 'Apply now!' button below." +
                "\r\n**4)** Answer the questions in the form provided by the bot and when ready, submit your application!" +
                "\r\n**5)** Once you put your app in, start chatting in our Discord! I will begin keeping tabs on you and will move you to Pending once you have met the requirements! (Shouldn't take too long!)" +
                "\r\n**If you have a friend, they can confirm they know you and you skip to pending!**" +
                "\r\n**6)** Once you meet requirements, staff will vote on your application in Discord. If approved, you will get changed to Citizen automatically and whitelisted." +
                "\r\n\r\n**And thats it!** Good luck!\r\n" +
                "\r\n***Jasper** - Your friendly guild bouncer and welcoming committee*\r\n",
                components: components.Build());
            await RespondAsync("sent", ephemeral: true);
        }
    }
}
