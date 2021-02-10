using System;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using System.Linq;

namespace GroundedBot.Commands
{
    class EmojiList
    {
        public static string[] Aliases =
        {
            "emojilist",
            "emoji-list",
            "emoji",
            "emojis",
            "emotelist",
            "emote-list",
            "emote",
            "emotes"
        };
        public static string Description = "Shows a categorized list of avaliable emojis.";
        public static string[] Usages = { ".emojilist [category]" };
        public static string Permission = "Anyone can us it.";
        public static string Trello = "https://trello.com/c/eQ5k04TI/30-emoji";

        public async static void DoCommand()
        {
            await Program.Log("command", "");
            var message = Recieved.Message;
            string[] m = message.Content.Split();
            var r = new Random();

            var embed = new EmbedBuilder()
                .WithAuthor(author => { author.WithName($"Avaliable emojis"); })
                .WithColor(new Color(0xFFDD00));

            if (m.Length > 2)
                await message.Channel.SendMessageAsync("❌ Too many parameters!");
            else if (m.Length == 1)
                foreach (var i in Program._client.Guilds.Where(x => x.Name.Contains("Emojis")))
                {
                    string emojis = "";
                    for (int j = 0; j < 3; j++)
                    {
                        for (int k = 0; k < 4; k++)
                            emojis += $"{i.Emotes.ElementAt(r.Next(0, i.Emotes.Count()))} ";
                        emojis += "\n";
                    }
                    emojis += $"[Server Invite]({i.GetInvitesAsync().Result.FirstOrDefault().Url})";
                    embed.AddField(i.Name, emojis, true);
                }
            else
            {
                await message.Channel.SendMessageAsync("WIP");
            }

            await message.Channel.SendMessageAsync(null, embed: embed.Build());
        }
    }
}
