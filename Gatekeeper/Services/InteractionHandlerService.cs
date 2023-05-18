using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Gatekeeper.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace Gatekeeper.Services
{
    public class InteractionHandlerService
    {
        private readonly DiscordSocketClient _client;
        private readonly ConfigService _config;
        private readonly InteractionService _interactions;
        private IServiceProvider _services;

        public InteractionHandlerService(IServiceProvider services, DiscordSocketClient client, 
            ConfigService config, InteractionService commands)
        {
            _client = client;
            _config = config;
            _services = services;
            _interactions = commands;

            _client.Ready += RegisterCommands;
        }

        public async Task InitializeAsync()
        {
            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _client.InteractionCreated += InteractionCreated;
            _interactions.SlashCommandExecuted += SlashCommandExecutedAsync;
            _interactions.InteractionExecuted += InteractionExecutedAsync;
        }

        private async Task RegisterCommands()
        {
            try
            {
                await _interactions.RegisterCommandsToGuildAsync(_config.BotConfig.GuildId);
                await _interactions.AddModulesGloballyAsync(false, modules: _interactions.Modules.Where(module => module.Name == "AboutModule").ToArray());
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
            await _interactions.ExecuteCommandAsync(context, _services);
        }

        private async Task SlashCommandExecutedAsync(SlashCommandInfo command, IInteractionContext context, IResult result)
        {
            // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
            if (result.IsSuccess) return;

            await context.Interaction.RespondAsync(result.ErrorReason, ephemeral: true);
        }

        private async Task InteractionExecutedAsync(ICommandInfo info, IInteractionContext context, IResult result)
        {
            if (result.IsSuccess) return;

            await context.Interaction.RespondAsync(result.ErrorReason, ephemeral: true);
        }
    }
}
