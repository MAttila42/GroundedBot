using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using MongoDB.Driver;
using GroundedBot.Services;

namespace GroundedBot
{
	class GroundedBot
	{
		private readonly Config _config;
		private readonly DiscordSocketClient _client;
		private readonly InteractionService _interaction;
		private readonly EventService _event;
		private readonly EmojiService _emoji;
		private readonly MongoService _mongo;

		private readonly IServiceProvider _services;

		public GroundedBot(Config config)
		{
			this._config = config;
			this._client = new DiscordSocketClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = true,
				GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages,
				UseInteractionSnowflakeDate = false
			});
			this._interaction = new InteractionService(_client.Rest);
			this._event = new();
			this._emoji = new();
			this._mongo = new MongoService("mongodb://localhost:27017");

			this._services = new ServiceCollection()
				.AddSingleton(_config)
				.AddSingleton(_client)
				.AddSingleton(_emoji)
				.AddSingleton(_mongo)
				.BuildServiceProvider();
		}

		public async Task MainAsync()
		{
			_client.Log += Log;
			_interaction.Log += Log;

			await _client.LoginAsync(TokenType.Bot, this._config.Token);
			await _client.StartAsync();

			_client.SlashCommandExecuted += ExecuteInteractionAsync;
			_client.ButtonExecuted += ExecuteInteractionAsync;
			_client.ModalSubmitted += ExecuteInteractionAsync;

			_client.MessageReceived += async m =>
			{
				if (m.Author.IsBot) return;
				SocketCommandContext ctx = new(_client, m as SocketUserMessage);
				await _event.ExecuteMessageEventsAsync(ctx, _services);
			};

			_client.Ready += async () =>
			{
				_emoji.LoadEmojis(_client, _config.EmojiGuilds);
				await _mongo.UpdateGuilds(_client.Guilds);

				foreach (SocketGuild guild in _client.Guilds)
					foreach (ulong userId in _mongo.Classes.AsQueryable().SelectMany(c => c.Students))
						if (!guild.Users.Any(u => u.Id == userId))
							await _mongo.Classes.UpdateManyAsync(c => c.Students.Contains(userId) && c.Guild == guild.Id, Builders<TanClass>.Update.Pull(c => c.Students, userId));

				await _interaction.AddModulesAsync(typeof(GroundedBot).Assembly, _services);
#if DEBUG
				await _interaction.RegisterCommandsToGuildAsync(_config.DebugGuild, true);
#else
                await _interaction.RegisterCommandsGloballyAsync(true);
#endif
			};

			_client.JoinedGuild += async g => await _mongo.UpdateGuilds(_client.Guilds);
			_client.UserLeft += async (g, u) => await _mongo.Classes.UpdateManyAsync(c => c.Students.Contains(u.Id) && c.Guild == g.Id, Builders<TanClass>.Update.Pull(c => c.Students, u.Id));

			await Task.Delay(-1).ConfigureAwait(false);
		}

		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		private async Task ExecuteInteractionAsync<T>(T i)
			where T : SocketInteraction
		{
			SocketInteractionContext<T> ctx = new SocketInteractionContext<T>(_client, i);
			await _interaction.ExecuteCommandAsync(ctx, _services);
		}
	}
}
