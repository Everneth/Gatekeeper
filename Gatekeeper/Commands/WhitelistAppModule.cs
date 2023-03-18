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
            if (_whitelist.UserHasActiveApplication(user.Id))
            {
                await RespondAsync("You already have an active application!", ephemeral: true);
                return;
            }
            else if (user.Roles.Where(role => role.Name == "Citizen").Count() > 0 || _database.PlayerExists(user))
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
                    Label = "Do you know someone in our community?",
                    CustomId = "fourth",
                    Placeholder = "Put their username if yes",
                    MinLength = 2,
                    MaxLength = 60,
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
            await Context.Channel.SendMessageAsync("We have received your application! Either get to makin' conversation or have your friend confirm they know you!");
            
            // The user has claimed they have a friend, friend confirmation message needs to be sent
            if (app.Friend.Contains('@'))
            {
                channel = guild.GetTextChannel(_config.BotConfig.GeneralChannelId);
                await channel.SendMessageAsync($"Hey {app.Friend}, {app.User.Mention} has claimed you as a friend. Please press one of the buttons to confirm/deny them as a friend.",
                    components: BuildFriendConfirmationComponents());
            }
        }

        [ModalInteraction("info")]
        public async Task AppInfoSubmission(GenericApplicationInputModal modal)
        {
            await DeferAsync();
            WhitelistApp app = _whitelist.GetApp(Context.User.Id);
            string userWithDiscrim = modal.Fourth;
            modal.Fourth = "Nope";
            // Default friend field to 'Nope' unless we can find a valid discord user by the name and discriminator
            if (userWithDiscrim.Contains('#'))
            {
                string[] username = userWithDiscrim.Split('#');
                // Get user matching username and discrim, will return null if they do not exist
                var user = _client.GetGuild(_config.BotConfig.GuildId).GetUser(_client.GetUser(username[0], username[1]).Id);
                if (user != null)
                {
                    // Check that the user is a citizen
                    if (user.Roles.Where(role => role.Name.Equals("Citizen")).Count() > 0)
                    {
                        modal.Fourth = user.Mention;
                    }

                    // Check if user's referral restriction has expired
                    EMIPlayer player = _database.GetEMIPlayer(user.Id);
                    if (player.CanNextReferFriend > DateTime.Now)
                    {
                        uint unixEpoch = (uint)player.CanNextReferFriend.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                        modal.Fourth = "Nope";
                        await RespondAsync($"{user.Mention} cannot confirm another friend until <t:{unixEpoch}:f>." +
                            $" If you know someone else you may use them instead.", ephemeral: true);
                    }
                }
            }

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
            // Only works because we can guarantee this interaction is off a message component
            string message = (Context.Interaction as SocketMessageComponent).Message.Content;

            // Need to grab discord ids from confirmation message to parse to users <@ulong>, <@ulong>
            ParseUsers(message, out SocketGuildUser friend, out SocketGuildUser applicant);
            
            // The friend either took too long or one of the users left
            if (friend is null || applicant is null || !_whitelist.UserHasActiveApplication(applicant.Id))
            {
                await DeleteOriginalResponseAsync();
                return;
            }

            if (Context.User.Id == applicant.Id)
            {
                await RespondAsync("You cannot confirm yourself...", ephemeral: true);
                return;
            }
            else if (Context.User.Id != friend.Id)
            {
                await RespondAsync("You cannot confirm this applicant.", ephemeral: true);
                return;
            }

            await ModifyOriginalResponseAsync(message =>
            {
                uint unixEpoch = (uint)DateTime.Now.AddDays(30.0f).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                message.Content = $"{friend.Mention} has confirmed {applicant.Mention} as a friend. Friend accountability ends <t:{unixEpoch}:f> your time.";
                message.Components = BuildFriendConfirmationComponents(true);
            });

            // Add pending which starts the whitelist vote
            await applicant.AddRoleAsync(Context.Guild.Roles.Where(role => role.Name == "Pending").First());

            EMIPlayer friendPlayer = _database.GetEMIPlayer(friend.Id);
            friendPlayer.CanNextReferFriend = DateTime.Now.AddDays(3.0f);
            _database.UpdateEMIPlayer(friendPlayer);
        }

        [ComponentInteraction("denial")]
        public async Task FriendDenial()
        {
            // Only works because we can guarantee this interaction is off a message component
            string message = (Context.Interaction as SocketMessageComponent).Message.Content;

            // Need to grab discord ids from confirmation message to parse to users <@ulong>, <@ulong>
            ParseUsers(message, out SocketGuildUser friend, out SocketGuildUser applicant);

            if (Context.User.Id == applicant.Id)
            {
                await RespondAsync("Trying to deny yourself, eh?", ephemeral: true);
                return;
            }
            else if (Context.User.Id != friend.Id)
            {
                await RespondAsync("Stop pushing my buttons.", ephemeral: true);
                return;
            }

            // Just delete the message if they have denied friend accountability and message the applicant
            await DeleteOriginalResponseAsync();
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

        private void ParseUsers(string message, out SocketGuildUser friend, out SocketGuildUser applicant)
        {
            // Has two capture groups for Discord Ids from the interaction message
            string pattern = @"\<@(\d+)\>.*\<@(\d+)\>";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(message);

            friend = Context.Guild.GetUser(ulong.Parse(match.Groups[0].Value));
            applicant = Context.Guild.GetUser(ulong.Parse(match.Groups[1].Value));
        }
    }
}
