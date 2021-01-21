using System;
using System.Collections.Generic;

namespace GroundedBot.Commands
{
    class Command
    {
        public string Name { get; set; }
        public string[] Aliases { get; set; }
        public string Description { get; set; }
        public string Usage { get; set; }
        public string Perm { get; set; }
        public string Category { get; set; }
        public string Trello { get; set; }

        public Command(string name, string[] aliases, string desc, string usage, string perm, string category, string trello)
        {
            Name = name;
            Aliases = aliases;
            Description = desc;
            Usage = usage;
            Perm = perm;
            Category = category;
            Trello = trello;
        }
    }

    class Commands
    {
        public static string[] Aliases =
        {
            "commands",
            "command",
            "parancsok",
            "parancs",
            "help",
            "segítség",
            "segitseg"
        };
        public static string Description = "Shows the list or the specified command.";
        public static string usage = ".commands [command]";


        public static async void DoCommand()
        {
            await Program.Log("command");

            var message = Recieved.Message;
            string[] m = message.Content.Split();

            switch (m.Length)
            {
                case 1:
                    break;
                case 2:
                    break;

                default:
                    await message.Channel.SendMessageAsync("❌ Too many parameters!");
                    return;
            }
        }
    }
}
