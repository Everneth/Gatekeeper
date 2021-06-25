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
		private DiscordToken _token;

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
				_joinEvent = services.GetRequiredService<UserJoinEvent>();
				_data = services.GetRequiredService<DataService>();

				_client.Log += Log;

				await _client.SetGameAsync("everyone", null, ActivityType.Watching);

				await _client.LoginAsync(TokenType.Bot,
					_data.Load("token", _token.Token));
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
						AlwaysDownloadUsers = true
					}))
				.AddSingleton<CommandService>()
				.AddSingleton<CommandHandlerService>()
				.AddSingleton<RankingService>()
				.AddSingleton<ConfigService>()
				.AddSingleton<DataService>()
				.AddSingleton<UserJoinEvent>()
				.BuildServiceProvider();
		}
	}
}
