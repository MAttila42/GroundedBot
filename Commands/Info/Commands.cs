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
        public string[] Usages { get; set; }
        public string Permission { get; set; }
        public string Category { get; set; }
        public string Trello { get; set; }

        public Command(string name, string[] aliases, string desc, string[] usages, string perm, string category, string trello)
        {
            Name = name;
            Aliases = aliases;
            Description = desc;
            Usages = usages;
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
        public static string[] Usages = { ".commands [command]" };
        public static string Permission = "Anyone can use it.";
        public static string Trello = "https://trello.com/c/VUQlIot5/27-commands";

        public static async void DoCommand()
        {
            await Program.Log("command");

            var message = Recieved.Message;
            string[] m = message.Content.Split();

            var commands = new List<Command>();
            // Dev
            commands.Add(new Command("Evaluate", Evaluate.Aliases, Evaluate.Description, Evaluate.Usages, Evaluate.Permission, "Dev", Evaluate.Trello));
            commands.Add(new Command("Ping", Ping.Aliases, Ping.Description, Ping.Usages, Ping.Permission, "Dev", Ping.Trello));
            commands.Add(new Command("Restart", Restart.Aliases, Restart.Description, Restart.Usages, Restart.Permission, "Dev", Restart.Trello));
            commands.Add(new Command("Test", Test.Aliases, Test.Description, Test.Usages, Test.Permission, "Dev", Test.Trello));
            // Fun
            commands.Add(new Command("Minesweeper", Minesweeper.Aliases, Minesweeper.Description, Minesweeper.Usages, Minesweeper.Permission, "Fun", Minesweeper.Trello));
            // Info
            commands.Add(new Command("Commands", Commands.Aliases, Commands.Description, Commands.Usages, Commands.Permission, "Info", Commands.Trello));
            commands.Add(new Command("Leaderboard", Leaderboard.Aliases, Leaderboard.Description, Leaderboard.Usages, Leaderboard.Permission, "Info", Leaderboard.Trello));
            commands.Add(new Command("UserInfo", UserInfo.Aliases, UserInfo.Description, UserInfo.Usages, UserInfo.Permission, "Info", UserInfo.Trello));
            // Util
            commands.Add(new Command("AnswerRequest", AnswerRequest.Aliases, AnswerRequest.Description, AnswerRequest.Usages, AnswerRequest.Permission, "Util", AnswerRequest.Trello));
            commands.Add(new Command("PingRequest", PingRequest.Aliases, PingRequest.Description, PingRequest.Usages, PingRequest.Permission, "Util", PingRequest.Trello));

            string content = "";

            switch (m.Length)
            {
                case 1:
                    content += "For more information use `.commands [command]`.\n\n";

                    var categories = new HashSet<string>();
                    foreach (var i in commands)
                        categories.Add(i.Category);

                    foreach (var category in categories)
                    {
                        content += $"**{category}**\n";
                        var currentCategory = commands.Where(x => x.Category == category).ToList();
                        for (int j = 0; j < currentCategory.Count(); j++)
                            content += $"`{currentCategory[j].Name}`{(j < currentCategory.Count() - 1 ? ", " : "\n\n")}";
                    }
                    content += "Official Documentation on [Trello](https://trello.com/b/Ns1WcpEB/groundedbot).";

                    var embed = new EmbedBuilder()
                        .WithAuthor(author =>
                        {
                            author
                                .WithName("Commands")
                                .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/801852346350174208/noun_Information_405516.png"); // Information by Viktor Ostrovsky from the Noun Project
                        })
                        .WithDescription(content)
                        .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name)
                        .WithColor(new Color(0x7289DA)).Build();
                    await message.Channel.SendMessageAsync(null, embed: embed);
                    break;
                case 2:
                    var foundCommands = commands.Where(x => x.Aliases.Contains(m[1])).ToList();

                    foreach (var command in foundCommands)
                    {
                        content = "";
                        content += $"Category: **{command.Category}**\n" +
                            $"{command.Description}\n" +
                            $"{command.Permission}\n\n" +
                            $"Usage{(command.Usages.Length > 1 ? "s:\n" : ": ")}";
                        foreach (var usage in command.Usages)
                            content += $"`{usage}`\n";
                        content += $"\nAliases: ";
                        for (int j = 0; j < command.Aliases.Count(); j++)
                            content += $"`{command.Aliases[j]}`{(j < command.Aliases.Count() - 1 ? ", " : "\n\n")}";
                        content += $"Official Documentation on [Trello]({command.Trello}).";

                        var embed2 = new EmbedBuilder()
                            .WithAuthor(author =>
                            {
                                author
                                    .WithName(command.Name)
                                    .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/801852346350174208/noun_Information_405516.png"); // Information by Viktor Ostrovsky from the Noun Project
                            })
                            .WithDescription(content)
                            .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name)
                            .WithColor(new Color(0x7289DA)).Build();
                        await message.Channel.SendMessageAsync(null, embed: embed2);
                    }
                    break;

                default:
                    await message.Channel.SendMessageAsync("❌ Too many parameters!");
                    return;
            }
        }
    }
}
