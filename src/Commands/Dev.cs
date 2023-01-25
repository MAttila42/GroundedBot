// using System.Diagnostics;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GroundedBot.Helpers;
using GroundedBot.Services;
using MongoDB.Driver;

namespace GroundedBot.Commands;

[Group("dev", "[DEV] Fejlesztő parancsok")]
[RequireOwner]
public class Dev : InteractionModuleBase
{
	public DiscordSocketClient Client { get; set; }
	public MongoService Mongo { get; set; }

	[SlashCommand("guildcount", "[DEV] Szerverek száma")]
	public async Task ServerCount() =>
		await RespondAsync(embed: Embeds.Info($"A bot {Client.Guilds.Count} db szerveren van bent."));

	[SlashCommand("shutdown", "[DEV] Bot leállítása")]
	public async Task Shutdown()
	{
		await RespondAsync(embed: Embeds.Info("Kikapcsolás..."));
		Environment.Exit(0);
	}

	[SlashCommand("test", "[DEV] Tesztelés")]
	public async Task Test()
	{
		await RespondAsync(Context.User.Mention, allowedMentions: AllowedMentions.None);
		if (await Mongo.Classes.CountDocumentsAsync(x => true) > 0)
			await FollowupAsync(string.Join(", ", Mongo.Classes.AsQueryable().Select(c => c.Theme).ToList()));
	}
}
