using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Gatekeeper.Services
{
    public class CommandHandlerService
    {
        private readonly DiscordSocketClient _client;
        private readonly ConfigService _config;
        private readonly InteractionService _commands;
        private IServiceProvider _services;

        public CommandHandlerService(IServiceProvider services, DiscordSocketClient client, 
            ConfigService config, InteractionService commands)
        {
            _client = client;
            _config = config;
            _services = services;
            _commands = commands;

            _client.Ready += RegisterCommands;

            _client.InteractionCreated += InteractionCreated;
            _commands.SlashCommandExecuted += SlashCommandExecutedAsync;
        }

        public async Task InstallCommandsAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
        }

        private async Task RegisterCommands()
        {
            try
            {
                await _commands.RegisterCommandsToGuildAsync(_config.BotConfig.GuildId);
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }

            _client.Ready -= RegisterCommands;
        }

        private async Task InteractionCreated(SocketInteraction interaction)
        {
            // I am not sure if bot's can trigger interactions/slash commands, but if they can we want to ignore them
            if (interaction.User.IsBot) return;

            var context = new SocketInteractionContext(_client, interaction);
            await _commands.ExecuteCommandAsync(context, _services);
        }

        private async Task SlashCommandExecutedAsync(SlashCommandInfo command, IInteractionContext context, Discord.Interactions.IResult result)
        {
            // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
            if (result.IsSuccess) return;

            await context.Interaction.RespondAsync(result.ErrorReason, ephemeral: true);
        }
    }
}
