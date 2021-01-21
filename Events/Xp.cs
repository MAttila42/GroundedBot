using System;
using System.Linq;
using Discord;
using Discord.WebSocket;
using GroundedBot.Json;

namespace GroundedBot.Events
{
    public class Xp
    {
        public async static void DoEvent()
        {
            var message = Recieved.Message;
            bool isSpam = false;
            try
            {
                var lastMessage = (await message.Channel.GetMessagesAsync(10).FlattenAsync()).Where(x => x.Author.Id == message.Author.Id).ElementAt(1);
                isSpam = lastMessage.Content == message.Content || (message.CreatedAt.DateTime - lastMessage.CreatedAt.DateTime).Seconds < 5;
            }
            catch (Exception) { }

            if (message.Content.Length == 1 ||
                isSpam ||
                BaseConfig.GetConfig().Channels.BotChannel.Contains(message.Channel.Id))
                return;

            var members = Members.PullData();
            int memberIndex = Members.GetMemberIndex(members, message.Author.Id.ToString());
            if (memberIndex == -1)
            {
                memberIndex = members.Count();
                members.Add(new Members(message.Author.Id));
            }
            members[memberIndex].XP++;

            int xp = members[memberIndex].XP;
            int rankup = 30;
            byte rank = 0;
            while (xp >= rankup)
            {
                rank++;
                xp -= rankup;
                rankup += rankup / 5;
            }

            if (rank > members[memberIndex].Rank)
            {
                var embed = new EmbedBuilder()
                    .WithAuthor(author =>
                    {
                        author
                            .WithName("Rank Up")
                            .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/786676132850302996/noun_Graph_2500310.png"); // Graph by Alice Design from the Noun Project
                    })
                    .WithDescription($"Congratulations **{message.Author.Mention}**! You ranked up.\nNew rank: **{rank}**")
                    .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name)
                    .WithThumbnailUrl(message.Author.GetAvatarUrl())
                    .WithColor(new Color(0xFFCC00)).Build();

                var channel = BaseConfig.GetConfig().Channels.LevelUp;
                if (channel != 0)
                    await ((IMessageChannel)Program._client.GetChannel(channel)).SendMessageAsync(
                        null,
                        embed: embed)
                        .ConfigureAwait(false);
            }

            if (members[memberIndex].Rank < rank)
            {
                members[memberIndex].Rank = rank;
                members[memberIndex].Floppy++;
            }

            Members.PushData(members);
        }
    }
}
