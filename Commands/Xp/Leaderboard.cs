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
        public async static void DoCommand()
        {
            await Program.Log("command");

            var message = Recieved.Message;
            var members = Members.PullData();
            string output = "";
            byte counter = 1;

            foreach (var i in members.OrderByDescending(x => x.XP).Take(5))
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

                output +=
                    $"#**{counter}** {Program._client.GetUser(id).Mention}\n" +
                    $"­ ­ ­ ­ ­ ­ ­ XP: **{xp}** /{totalXpNeeded}\n" +
                    $"­ ­ ­ ­ ­ ­ ­ Rank: **{rank}**\n" +
                    $"\n"; // Figyelem! Az üres helyek tele vannak "láthatatlan" karakterekkel.

                counter++;
            }

            var embed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author
                        .WithName("Leaderboard")
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
