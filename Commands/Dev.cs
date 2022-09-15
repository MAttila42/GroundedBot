using System.Diagnostics;
using MongoDB.Bson;
using MongoDB.Driver;
using Discord;
using Discord.Interactions;
using GroundedBot.Services;

namespace GroundedBot.Commands
{
    [Group("dev", "[DEV] Fejlesztő parancsok")]
    [RequireOwner]
    public class Dev : InteractionModuleBase
    {
        public MongoService _mongo { get; set; }

        [SlashCommand("export", "[DEV] Adatbázisok exportálása")]
        public async Task Export()
        {
            List<FileAttachment> temp = new();
            string classes = _mongo.Classes.AsQueryable().ToArray().ToJson();
            File.WriteAllText("classes.json", classes);
            temp.Add(new FileAttachment("classes.json"));
            string guilds = _mongo.Guilds.AsQueryable().ToArray().ToJson();
            File.WriteAllText("guilds.json", guilds);
            temp.Add(new FileAttachment("guilds.json"));
            await RespondWithFilesAsync(temp);
            await RespondAsync();
        }

        [SlashCommand("import", "[DEV] Adatbázisok importálása")]
        public async Task Import()
        {

        }

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
                await RespondAsync(embed: EmbedService.Info("Újraindítás...", "Ez eltarthat egy darabig."));
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
            await RespondAsync(embed: EmbedService.Info("Kikapcsolás..."));
            Environment.Exit(0);
        }

        [SlashCommand("Test", "[DEV] Tesztelés")]
        public async Task Test()
        {
            await RespondAsync(embed: EmbedService.Info("Kikapcsolás..."));
            Environment.Exit(0);
        }
    }
}
