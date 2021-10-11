using Discord;
using Discord.Commands;
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
		private DiscordSocketClient _client;
		private CommandService _commands;
		private RankingService _ranking;
		private ConfigService _config;
		private UserJoinEvent _joinEvent;
		private DataService _data;
		private AuditService _auditer;
		private RoleService _manager;
		private BotConfig _token;
		private DatabaseService _database;

		public static void Main()
		=> new Program().MainAsync().GetAwaiter().GetResult();	

		public async Task MainAsync()
		{
			using (var services = ConfigureServices())
			{
				_client = services.GetRequiredService<DiscordSocketClient>();
				_commands = services.GetRequiredService<CommandService>();
				_ranking = services.GetRequiredService<RankingService>();
				_config = services.GetRequiredService<ConfigService>();
				_joinEvent = services.GetRequiredService<UserJoinEvent>();
				_data = services.GetRequiredService<DataService>();
				_auditer = services.GetRequiredService<AuditService>();
				_manager = services.GetRequiredService<RoleService>();
				_database = services.GetRequiredService<DatabaseService>();

				_client.Log += Log;

				await _client.SetGameAsync(_config.BotConfig.CommandPrefix + "help", null, ActivityType.Listening);

				await _client.LoginAsync(TokenType.Bot, _config.BotConfig.Token);
				await _client.StartAsync();

				await services.GetRequiredService<CommandHandlerService>().InstallCommandsAsync();

				// Block this task until the program is closed.
				await Task.Delay(-1);
			}
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
						MessageCacheSize = 3000
					}))
				.AddSingleton<CommandService>()
				.AddSingleton<CommandHandlerService>()
				.AddSingleton<RankingService>()
				.AddSingleton<ConfigService>()
				.AddSingleton<DataService>()
				.AddSingleton<UserJoinEvent>()
				.AddSingleton<AuditService>()
				.AddSingleton<RoleService>()
				.AddSingleton<DatabaseService>()
				.BuildServiceProvider();
		}
	}
}
