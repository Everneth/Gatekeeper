using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Gatekeeper.Events;
using Gatekeeper.Models;
using Gatekeeper.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Gatekeeper
{
    class Program
    {
		public const string VERSION = "2.0.0";

		private DiscordSocketClient _client;
		private CommandService _commands;
		private RankingService _ranking;
		private ConfigService _config;
		private UserJoinEvent _joinEvent;
		private UserLeaveEvent _leaveEvent;
		private DataService _data;
		private AuditService _auditer;
		private RoleService _manager;
		private BotConfig _token;
		private DatabaseService _database;
		private WhitelistAppService _whitelist;
		private GuildMemberUpdated _memberUpdatedEvent;

		public static void Main()
		=> new Program().MainAsync().GetAwaiter().GetResult();	

		public async Task MainAsync()
		{
            using var services = ConfigureServices();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _ranking = services.GetRequiredService<RankingService>();
            _config = services.GetRequiredService<ConfigService>();
            _joinEvent = services.GetRequiredService<UserJoinEvent>();
			_leaveEvent = services.GetRequiredService<UserLeaveEvent>();
            _data = services.GetRequiredService<DataService>();
            _auditer = services.GetRequiredService<AuditService>();
            _manager = services.GetRequiredService<RoleService>();
            _database = services.GetRequiredService<DatabaseService>();
			_whitelist = services.GetRequiredService<WhitelistAppService>();
			_memberUpdatedEvent = services.GetRequiredService<GuildMemberUpdated>();

            _client.Log += Log;
            _client.Ready += OnReady;
            await _client.SetGameAsync("the door", type: ActivityType.Watching);

            await _client.LoginAsync(TokenType.Bot, _config.BotConfig.Token);
            await _client.StartAsync();

			await services.GetRequiredService<InteractionHandlerService>().InitializeAsync();

			// Block this task until the program is closed.
			await Task.Delay(-1);
        }

        private async Task OnReady()
        {
			// cache all members of the Everneth discord
			await _client.GetGuild(_config.BotConfig.GuildId).DownloadUsersAsync();

			// This is called here because loading the apps requires members already be cached
			_whitelist.Load();
		}

        private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		private ServiceProvider ConfigureServices()
		{
			return new ServiceCollection()
				.AddSingleton(new DiscordSocketClient(
					new DiscordSocketConfig
					{
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 3000,
                        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.MessageContent
                    }))
				.AddSingleton<InteractionService>()
				.AddSingleton<InteractionHandlerService>()
				.AddSingleton<RankingService>()
				.AddSingleton<ConfigService>()
				.AddSingleton<DataService>()
				.AddSingleton<UserJoinEvent>()
				.AddSingleton<UserLeaveEvent>()
				.AddSingleton<AuditService>()
				.AddSingleton<RoleService>()
				.AddSingleton<DatabaseService>()
				.AddSingleton<WhitelistAppService>()
				.AddSingleton<GuildMemberUpdated>()
				.BuildServiceProvider();
		}
	}
}
