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

        public async static void DoCommand()
        {
            await Program.Log("command", "");

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
            commands.Add(new Command("EmojiList", EmojiList.Aliases, EmojiList.Description, EmojiList.Usages, EmojiList.Permission, "Info", EmojiList.Trello));
            commands.Add(new Command("Leaderboard", Leaderboard.Aliases, Leaderboard.Description, Leaderboard.Usages, Leaderboard.Permission, "Info", Leaderboard.Trello));
            commands.Add(new Command("UserInfo", UserInfo.Aliases, UserInfo.Description, UserInfo.Usages, UserInfo.Permission, "Info", UserInfo.Trello));
            // Util
            commands.Add(new Command("AnswerRequest", AnswerRequest.Aliases, AnswerRequest.Description, AnswerRequest.Usages, AnswerRequest.Permission, "Util", AnswerRequest.Trello));
            commands.Add(new Command("PingRequest", PingRequest.Aliases, PingRequest.Description, PingRequest.Usages, PingRequest.Permission, "Util", PingRequest.Trello));
            commands.Add(new Command("Store", Store.Aliases, Store.Description, Store.Usages, Store.Permission, "Util", Store.Trello));

            commands.Add(new Command("Placeholder", null, null, null, null, "Placeholder", null));
            commands.Add(new Command("Placeholder", null, null, null, null, "Placeholder2", null));

            var embed = new EmbedBuilder()
                .WithAuthor(author => { author.WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/801852346350174208/noun_Information_405516.png"); }) // Information by Viktor Ostrovsky from the Noun Project
                .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name)
                .WithColor(new Color(0x7289DA));

            switch (m.Length)
            {
                case 1:
                    embed.WithAuthor(author => { author.WithName("Commands"); });
                    embed.WithDescription("For more information use `.commands [command]`.\n" +
                        "Official Documentation on [Trello](https://trello.com/b/Ns1WcpEB/groundedbot).");

                    var categories = new HashSet<string>();
                    foreach (var i in commands)
                        categories.Add(i.Category);

                    foreach (var category in categories)
                    {
                        string content = "```\n";
                        var currentCategory = commands.Where(x => x.Category == category).ToList();
                        for (int j = 0; j < currentCategory.Count(); j++)
                            content += $"{currentCategory[j].Name}\n";
                        for (int j = 0; j < commands.GroupBy(x => x.Category).OrderBy(x => x.Count()).Last().Count() - currentCategory.Count(); j++)
                            content += " \n";
                        content += "```";
                        embed.AddField(category, content, true);
                    }
                    await message.Channel.SendMessageAsync(null, embed: embed.Build());
                    break;
                case 2:
                    List<Command> foundCommands;
                    try { foundCommands = commands.Where(x => x.Aliases.Contains(m[1].ToLower())).ToList(); }
                    catch (Exception) { return; }

                    if (foundCommands.Count < 1)
                        return;

                    foreach (var command in foundCommands)
                    {
                        embed.WithAuthor(author => { author.WithName(command.Name); });
                        string content = "";
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
                        embed.WithDescription(content);
                        await message.Channel.SendMessageAsync(null, embed: embed.Build());
                    }
                    break;

                default:
                    await message.Channel.SendMessageAsync("❌ Too many parameters!");
                    return;
            }
        }
    }
}
