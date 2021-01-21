using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;

namespace GroundedBot.Commands
{
    class Command
    {
        public string Name { get; set; }
        public string[] Aliases { get; set; }
        public string Description { get; set; }
        public string Usage { get; set; }
        public string Permission { get; set; }
        public string Category { get; set; }
        public string Trello { get; set; }

        public Command(string name, string[] aliases, string desc, string usage, string perm, string category, string trello)
        {
            Name = name;
            Aliases = aliases;
            Description = desc;
            Usage = usage;
            Permission = perm;
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
        public static string Description = "Shows the list of commands or information about the one asked.";
        public static string Usage = ".commands [command]";
        public static string Permission = "Anyone can use it.";
        public static string Trello = "https://trello.com/c/VUQlIot5/27-commands";

        public static async void DoCommand()
        {
            await Program.Log("command");

            var message = Recieved.Message;
            string[] m = message.Content.Split();

            var commands = new List<Command>();
            // Dev
            commands.Add(new Command("Evaluate", Evaluate.Aliases, Evaluate.Description, Evaluate.Usage, Evaluate.Permission, "Dev", Evaluate.Trello));
            commands.Add(new Command("Ping", Ping.Aliases, Ping.Description, Ping.Usage, Ping.Permission, "Dev", Ping.Trello));
            commands.Add(new Command("Restart", Restart.Aliases, Restart.Description, Restart.Usage, Restart.Permission, "Dev", Restart.Trello));
            commands.Add(new Command("Test", Test.Aliases, Test.Description, Test.Usage, Test.Permission, "Dev", Test.Trello));
            // Fun
            commands.Add(new Command("Minesweeper", Minesweeper.Aliases, Minesweeper.Description, Minesweeper.Usage, Minesweeper.Permission, "Fun", Minesweeper.Trello));
            // Info
            commands.Add(new Command("Commands", Commands.Aliases, Commands.Description, Commands.Usage, Commands.Permission, "Info", Commands.Trello));
            commands.Add(new Command("Leaderboard", Leaderboard.Aliases, Leaderboard.Description, Leaderboard.Usage, Leaderboard.Permission, "Info", Leaderboard.Trello));
            commands.Add(new Command("UserInfo", UserInfo.Aliases, UserInfo.Description, UserInfo.Usage, UserInfo.Permission, "Info", UserInfo.Trello));
            // Util
            commands.Add(new Command("AnswerRequest", AnswerRequest.Aliases, AnswerRequest.Description, AnswerRequest.Usage, AnswerRequest.Permission, "Util", AnswerRequest.Trello));
            commands.Add(new Command("PingRequest", PingRequest.Aliases, PingRequest.Description, PingRequest.Usage, PingRequest.Permission, "Util", PingRequest.Trello));

            string title = "Commands";
            string content = "";

            switch (m.Length)
            {
                case 1:
                    content += "For more information use `.commands [command]`.\n" +
                        "\n" +
                        "**Dev**\n";
                    for (int i = 0; i < commands.Where(x => x.Category == "Dev").Count(); i++)
                        content += $"`{commands[i].Name}`{(i < commands.Where(x => x.Category == "Dev").Count() - 1 ? ", " : "\n\n")}";
                    content += "**Fun**\n";
                    for (int i = 0; i < commands.Where(x => x.Category == "Fun").Count(); i++)
                        content += $"`{commands[i].Name}`{(i < commands.Where(x => x.Category == "Fun").Count() - 1 ? ", " : "\n\n")}";
                    content += "**Info**\n";
                    for (int i = 0; i < commands.Where(x => x.Category == "Info").Count(); i++)
                        content += $"`{commands[i].Name}`{(i < commands.Where(x => x.Category == "Info").Count() - 1 ? ", " : "\n\n")}";
                    content += "**Util**\n";
                    for (int i = 0; i < commands.Where(x => x.Category == "Util").Count(); i++)
                        content += $"`{commands[i].Name}`{(i < commands.Where(x => x.Category == "Util").Count() - 1 ? ", " : "\n\n")}";
                    content += "Official Documentation on [Trello](https://trello.com/b/Ns1WcpEB/groundedbot)";
                    break;
                case 2:
                    break;

                default:
                    await message.Channel.SendMessageAsync("❌ Too many parameters!");
                    return;
            }

            var embed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author
                        .WithName(title)
                        .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/801852346350174208/noun_Information_405516.png"); // Information by Viktor Ostrovsky from the Noun Project
                })
                .WithDescription(content)
                .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name)
                .WithColor(new Color(0x7289DA)).Build();
            await message.Channel.SendMessageAsync(null, embed: embed);
        }
    }
}
