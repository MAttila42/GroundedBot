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

        [SlashCommand("guildcount", "[DEV] Szerverek száma")]
        public async Task ServerCount() =>
            await RespondAsync(embed: EmbedService.Info($"A bot {_client.Guilds.Count} db szerveren van bent."));

        [SlashCommand("shutdown", "[DEV] Bot leállítása")]
        public async Task Shutdown()
        {
            await RespondAsync(embed: EmbedService.Info("Kikapcsolás..."));
            Environment.Exit(0);
        }

        [SlashCommand("test", "[DEV] Tesztelés")]
        public async Task Test()
        {
            await RespondAsync(Context.User.Mention, allowedMentions: AllowedMentions.None);
        }
    }
}
