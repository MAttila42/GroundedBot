using System;
using System.Collections.Generic;
using System.Diagnostics;
using GroundedBot.Json;

namespace GroundedBot.Commands
{
    class Restart
    {
        public static List<ulong> AllowedRoles =
            new List<ulong>(BaseConfig.GetConfig().Roles.Admin);

        public static string[] Aliases = { "restart" };
        public static string Description = "Restart or shutdown the bot. Updates from the Github repository.";
        public static string[] Usages = { "restart [option]" };
        public static string Permission = "Only Devs can use it.";
        public static string Trello = "https://trello.com/c/2t1CC8e0/6-restart";

        public async static void DoCommand()
        {
            await Program.Log("command", "");

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
