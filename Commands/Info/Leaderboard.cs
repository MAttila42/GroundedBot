using System.Linq;
using Discord;
using Discord.WebSocket;
using GroundedBot.Json;

namespace GroundedBot.Commands
{
    class Leaderboard
    {
        public static string[] Aliases =
        {
            "leaderboard",
            "lb",
            "top5",
            "top"
        };
        public static string Description = "A command to show different types of leaderboards. By default it shows Floppy and only the TOP 5.";
        public static string[] Usages = { "leaderboard [type]" };
        public static string Permission = "Anyone can use it.";
        public static string Trello = "https://trello.com/c/T1fixUma/9-leaderboard";

        public async static void DoCommand()
        {
            await Program.Log();

            var message = Recieved.Message;
            string[] m = message.Content.Split();
            var members = Members.PullData();
            string output = "";
            byte counter = 1;

            var ordered = members.OrderByDescending(x => x.Floppy);
            string title = "Floppy Leaderboard";
            if (m.Length > 1 && m[1].ToLower() == "xp")
            {
                ordered = members.OrderByDescending(x => x.XP);
                title = "XP Leaderboard";
            }
            if (m.Length > 1 && m[1].ToLower() == "help")
            {
                title = "Help Leaderboard";

                foreach (var i in members.Where(x => x.LastHelp > 0).OrderByDescending(x => x.LastHelp))
                {
                    switch (counter)
                    {
                        case 1:
                            output += ":first_place: ";
                            break;
                        case 2:
                            output += ":second_place: ";
                            break;
                        case 3:
                            output += ":third_place: ";
                            break;
                        default:
                            output += ":small_blue_diamond: ";
                            break;
                    }

                    output += $"#**{counter}** {Program._client.GetUser(i.ID).Mention} - **{i.LastHelp}**\n";
                    counter++;
                }
            }
            else
            {
                foreach (var i in ordered.Take(5))
                {
                    switch (counter)
                    {
                        case 1:
                            output += ":first_place: ";
                            break;
                        case 2:
                            output += ":second_place: ";
                            break;
                        case 3:
                            output += ":third_place: ";
                            break;
                        default:
                            output += ":small_blue_diamond: ";
                            break;
                    }

                    ulong id = i.ID;
                    int xp = i.XP;
                    int bal = i.Floppy;
                    int partXp = xp;
                    int rankup = 30;
                    byte rank = 0;
                    int totalXpNeeded = rankup;

                    while (partXp >= rankup)
                    {
                        rank++;
                        partXp -= rankup;
                        rankup += rankup / 5;
                        totalXpNeeded += rankup;
                    }

                    if (m.Length > 1 && m[1].ToLower() == "xp")
                        output +=
                            $"#**{counter}** {Program._client.GetUser(id).Mention}\n" +
                            $"­ ­ ­ ­ ­ ­ ­ XP: **{xp}** /{totalXpNeeded}\n" +
                            $"­ ­ ­ ­ ­ ­ ­ Rank: **{rank}**\n" +
                            $"\n"; // Figyelem! Az üres helyek tele vannak "láthatatlan" karakterekkel.
                    else
                        output +=
                            $"#**{counter}** {Program._client.GetUser(id).Mention}\n" +
                            $"­ ­ ­ ­ ­ ­ ­ Floppy: **{bal}**\n" +
                            $"\n"; // Figyelem! Az üres helyek tele vannak "láthatatlan" karakterekkel.

                    counter++;
                }
            }

            var embed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author
                        .WithName(title)
                        .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/786668060849209364/noun_Podium_584809.png"); // Podium by Viktor Ostrovsky from the Noun Project
                })
                .WithDescription(output)
                .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name)
                .WithThumbnailUrl(((SocketGuildChannel)message.Channel).Guild.IconUrl)
                .WithColor(new Color(0xFFCC00)).Build();
            await message.Channel.SendMessageAsync(
                null,
                embed: embed)
                .ConfigureAwait(false);
        }
    }
}
