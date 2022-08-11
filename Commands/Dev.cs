﻿using Discord;
using System.Diagnostics;
using Discord.Interactions;
using GroundedBot.Services;

namespace GroundedBot.Commands
{
    public class Dev : InteractionModuleBase
    {
        public MongoService _mongo { get; set; }

        public enum Command
        {
            Test,
            ShutDown,
            Restart
        }

        [SlashCommand("dev", "[DEV] Developer commands")]
        [RequireOwner]
        public async Task Run(Command command)
        {
            try
            {
                switch (command)
                {
                    case Command.Test:
                        await RespondAsync(Context.User.Mention, allowedMentions: AllowedMentions.None);
                        break;
                    case Command.ShutDown:
                        await RespondAsync(embed: EmbedService.Info("Kikapcsolás..."));
                        Environment.Exit(0);
                        break;
                    case Command.Restart:
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
                        await RespondAsync(embed: EmbedService.Info("Újraindítás...", "Ez eltarthat egy darabig"));
                        Environment.Exit(0);
                        break;
                    default:
                        await RespondAsync(embed: EmbedService.Error("Nem sikerült futtatni a parancsot", $"Nincs {command} parancs."), ephemeral: true);
                        break;
                }
            }
            catch (Exception e) { await RespondAsync(embed: EmbedService.Error("Hiba", $"Nem található bash a `/bin/bash` helyen.\n```{e.Message}```"), ephemeral: true); }
        }
    }
}
