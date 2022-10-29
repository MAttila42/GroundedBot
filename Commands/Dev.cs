using System.Diagnostics;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MongoDB.Driver;
using GroundedBot.Services;

namespace GroundedBot.Commands
{
	[Group("dev", "[DEV] Fejlesztő parancsok")]
	[RequireOwner]
	public class Dev : InteractionModuleBase
	{
		public DiscordSocketClient _client { get; set; }
		public MongoService _mongo { get; set; }

		[SlashCommand("restart", "[DEV] Bot újraindítása")]
		public async Task Restart()
		{
			try
			{
				var process = new ProcessStartInfo
				{
					FileName = "/bin/bash",
					Arguments = $"-c \"" +
						"cd ..\n" +
						"sudo git pull\n" +
						"sudo dotnet build -c Release -o build\n" +
						"cd build\n" +
						"sudo dotnet GroundedBot.dll" +
						"\"",
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true
				};
				Process.Start(process);
				await RespondAsync(embed: EmbedService.Info(
					"Újraindítás...",
					"Ez eltarthat egy darabig."));
				Environment.Exit(0);
			}
			catch (Exception e)
			{
				await RespondAsync(
					embed: EmbedService.Error(
						"Hiba",
						$"Nem található bash a `/bin/bash` helyen.\n" +
						$"```{e.Message}```"),
					ephemeral: true);
			}
		}

		[SlashCommand("shutdown", "[DEV] Bot leállítása")]
		public async Task Shutdown()
		{
			await RespondAsync(embed: EmbedService.Info(
				"Kikapcsolás..."));
			Environment.Exit(0);
		}

		[SlashCommand("test", "[DEV] Tesztelés")]
		public async Task Test() =>
			await RespondAsync(
				Context.User.Mention,
				allowedMentions: AllowedMentions.None);

		[SlashCommand(
			"classmodify",
			"[DEV] Osztály módosítása gomb hozzáadása az osztályokhoz")]
		public async Task ClassModify()
		{
			foreach (TanClass tanClass in _mongo.Classes.AsQueryable())
			{
				var guild = _client.GetGuild(tanClass.Guild);
				var channel = guild.GetTextChannel(
						_mongo.Guilds.AsQueryable()
						.SingleOrDefault(g => g.Guild == tanClass.Guild)
						.Channel.Classes);
				var messages = await channel.GetMessagesAsync(5).FlattenAsync();
				var message = messages.First(
						m => m.Embeds.First()
							.Author.Value.Name == tanClass.Theme);
				await ((IUserMessage)message).ModifyAsync(m => m.Components = new ComponentBuilder()
						.WithButton(
							"Csatlakozás",
							$"classbutton-join:{tanClass.ID}",
							ButtonStyle.Success)
						.WithButton(
							"Módosítás",
							$"classbutton-modify:{tanClass.ID}",
							ButtonStyle.Primary)
						.WithButton(
							"Törlés",
							$"classbutton-delete:{tanClass.ID}",
							ButtonStyle.Danger)
						.Build());
			}
			await RespondAsync(
				embed: EmbedService.Info(
					"Módosítás gomb hozzáadva az osztályokhoz"),
				ephemeral: true);
		}
	}
}
