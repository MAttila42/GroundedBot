using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using MongoDB.Driver;
using GroundedBot.Services;

namespace GroundedBot;

public class GroundedBot
{
	private readonly Config config;
	private readonly IServiceProvider services;

	public DiscordSocketClient Client { get; }
	public InteractionService Interaction { get; }
	public EmoteService Emote { get; }
	public MongoService Mongo { get; }

	public GroundedBot(string configPath)
	{
		Config config = JsonSerializer.Deserialize<Config>(
			File.ReadAllText(configPath, Encoding.UTF8));
		this.config = config;

		this.Client = new(new()
		{
			GatewayIntents =
				GatewayIntents.Guilds |
				GatewayIntents.GuildMembers |
				GatewayIntents.GuildMessages |
				GatewayIntents.MessageContent,
			HandlerTimeout = null,
			UseInteractionSnowflakeDate = false
		});
		this.Interaction = new(this.Client);
		this.Emote = new();
#if DEBUG
		this.Mongo = new("mongodb://localhost:27017");
#else
		this.Mongo = new("mongodb://db");
#endif

		this.services = new ServiceCollection()
			.AddSingleton(this.Client)
			.AddSingleton(this.Emote)
			.AddSingleton(this.Mongo)
			.BuildServiceProvider();
	}

	private GroundedBot() { }

	public async Task MainAsync()
	{
		Client.Log += Log;
		Interaction.Log += Log;

		await Client.LoginAsync(TokenType.Bot, this.config.Token);
		await Client.StartAsync();

		Client.SlashCommandExecuted += ExecuteInteractionAsync;
		Client.ButtonExecuted += ExecuteInteractionAsync;
		Client.SelectMenuExecuted += ExecuteInteractionAsync;
		Client.ModalSubmitted += ExecuteInteractionAsync;

		Client.MessageReceived += MessageHandler;

		Client.Ready += ReadyHandler;

		Client.JoinedGuild += async guild =>
			await Mongo.UpdateGuilds(Client.Guilds);

		Client.UserLeft += async (g, u) =>
			await Mongo.Classes.UpdateManyAsync(
				c =>
					c.Students.Contains(u.Id) &&
					c.Guild == g.Id,
				Builders<TanClass>.Update.Pull(
					c => c.Students, u.Id
				)
			);

		await Task.Delay(-1);
	}

	private Task Log(LogMessage message)
	{
		Console.WriteLine(message.ToString());
		return Task.CompletedTask;
	}

	private async Task ExecuteInteractionAsync<T>(T interaction)
		where T : SocketInteraction
	{
		SocketInteractionContext<T> context = new(Client, interaction);
		await Interaction.ExecuteCommandAsync(context, this.services);
	}

	private async Task MessageHandler(SocketMessage message)
	{
		if (message.Author.IsBot)
			return;
		SocketCommandContext context = new(
			Client,
			message as SocketUserMessage);
		await EventService.ExecuteMessageEventsAsync(context, this.services);
	}

	private async Task ReadyHandler()
	{
		Emote.LoadEmotes(Client, this.config.EmoteGuilds);
		await Mongo.UpdateGuilds(Client.Guilds);

		foreach (SocketGuild guild in Client.Guilds)
		{
			await guild.DownloadUsersAsync();
			var users = Mongo.Classes
				.AsQueryable()
				.SelectMany(c => c.Students);
			foreach (ulong userId in users)
				if (guild.GetUser(userId) is null)
					await Mongo.Classes
						.UpdateManyAsync(
							c =>
								c.Students.Contains(userId) &&
								c.Guild == guild.Id,
							Builders<TanClass>.Update.Pull(
								c => c.Students, userId
							)
						);
		}

		await Interaction.AddModulesAsync(
			typeof(GroundedBot).Assembly, this.services);
#if DEBUG
		await Interaction.RegisterCommandsToGuildAsync(
			this.config.DebugGuild, true);
#else
		await Interaction.RegisterCommandsGloballyAsync(true);
#endif
	}
}
