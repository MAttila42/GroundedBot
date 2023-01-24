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

	// [SlashCommand("restart", "[DEV] Bot újraindítása")]
	// public async Task Restart()
	// {
	// 	try
	// 	{
	// 		var process = new ProcessStartInfo
	// 		{
	// 			FileName = "/bin/bash",
	// 			Arguments = $"-c \"" +
	// 				"cd ..\n" +
	// 				"git pull\n" +
	// 				"dotnet build -c Release -o build -f net6.0\n" +
	// 				"cd build\n" +
	// 				"dotnet GroundedBot.dll -f net6.0" +
	// 				"\"",
	// 			RedirectStandardOutput = true,
	// 			UseShellExecute = false,
	// 			CreateNoWindow = true
	// 		};
	// 		Process.Start(process);
	// 		await RespondAsync(embed: EmbedService.Info(
	// 			"Újraindítás...",
	// 			"Ez eltarthat egy darabig."));
	// 		Environment.Exit(0);
	// 	}
	// 	catch (Exception e)
	// 	{
	// 		await RespondAsync(
	// 			embed: EmbedService.Error(
	// 				"Hiba",
	// 				$"Nem található bash a `/bin/bash` helyen.\n" +
	// 				$"```{e.Message}```"),
	// 			ephemeral: true);
	// 	}
	// }

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
