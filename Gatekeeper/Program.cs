using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Gatekeeper.Events;
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

		public static void Main(string[] args)
		=> new Program().MainAsync().GetAwaiter().GetResult();	

		public async Task MainAsync()
		{
			using (var services = ConfigureServices())
			{
				_client = services.GetRequiredService<DiscordSocketClient>();
				_commands = services.GetRequiredService<CommandService>();
				_ranking = services.GetRequiredService<RankingService>();
				_config = services.GetRequiredService<ConfigService>();
				//_joinEvent = services.GetRequiredService<UserJoinEvent>();

				_client.Log += Log;

				await _client.SetGameAsync("everyone", null, ActivityType.Watching);

				await _client.LoginAsync(TokenType.Bot,
					Environment.GetEnvironmentVariable("DiscordToken"));
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
				.AddSingleton<DiscordSocketClient>()
				.AddSingleton<CommandService>()
				.AddSingleton<CommandHandlerService>()
				.AddSingleton<RankingService>()
				.AddSingleton<ConfigService>()
				.AddSingleton<UserJoinEvent>()
				.BuildServiceProvider();
		}
	}
}
