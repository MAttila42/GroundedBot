using System;
using System.Collections.Generic;
using System.Diagnostics;
using GroundedBot.Json;

namespace GroundedBot.Commands.Dev
{
    class Restart
    {
        public static List<ulong> AllowedRoles =
            new List<ulong>(BaseConfig.GetConfig().Roles.Admin);

        public static string[] Aliases = { "restart" };

        public async static void DoCommand()
        {
            var message = Recieved.Message;
            try
            {
                if (message.Content.Split()[1] == "exit")
                {
                    await message.Channel.SendMessageAsync("Shutting down...");
                    Environment.Exit(0);
                }
            }
            catch (Exception) { }
            try
            {
                await message.Channel.SendMessageAsync("Restarting bot... (This may take a few moments)");
                string commands =
                    "cd ..\n" +
                    "git pull\n" +
                    "dotnet build -o build\n" +
                    "cd build\n" +
                    "dotnet GroundedBot.dll";
                var process = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{commands}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                Process.Start(process);
                Environment.Exit(0);
            }
            catch (Exception) { await message.Channel.SendMessageAsync("❌ Can't find bash!"); }
        }
    }
}
