using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Gatekeeper.Models;
using Gatekeeper.Models.Modals;
using Gatekeeper.Services;
using System;
using System.Linq;
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
            app.SecretWord = selectedWord[0];
            await ModifyOriginalResponseAsync(message =>
            {
                message.Embeds = new Embed[] { app.BuildApplicationEmbed() };
                message.Components = BuildApplicationComponents();
            });
        }

        [ComponentInteraction("app1")]
        public async Task OnInfoModalRequest()
        {
            WhitelistApp app = _whitelist.GetApp(Context.User.Id);
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
            WhitelistApp app = _whitelist.GetApp(Context.User.Id);
            SocketGuild guild = _client.GetGuild(_config.BotConfig.GuildId);
            SocketTextChannel channel = guild.GetTextChannel(_config.BotConfig.AppsChannelId);
            await channel.SendMessageAsync(embed: app.BuildApplicationEmbed());

            SocketRole applicant = guild.Roles.FirstOrDefault(role => role.Name == "Applicant");
            await guild.GetUser(Context.User.Id).AddRoleAsync(applicant);
        }

        [ModalInteraction("info")]
        public async Task AppInfoSubmission(GenericApplicationInputModal modal)
        {
            await DeferAsync();
            WhitelistApp app = _whitelist.GetApp(Context.User.Id);
            app.FillInfoFromModal(modal.First, modal.Second, modal.Third, modal.Fourth);
            await ModifyOriginalResponseAsync(message =>
            {
                message.Embeds = new Embed[] { app.BuildApplicationEmbed() };
                message.Components = BuildApplicationComponents();
            });
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
    }
}
