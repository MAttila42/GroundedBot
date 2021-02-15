using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using GroundedBot.Json;

namespace GroundedBot.Commands
{
    class AnswerRequest
    {
        public static List<ulong> RequiredRoles =
            new List<ulong>(BaseConfig.GetConfig().Roles.Mod);

        public static string[] Aliases =
        {
            "answerrequest",
            "answer-request",
            "ar",
            "answer"
        };
        public static string Description = "Request to review an answer. If someone wants to get paid for their help they need to run this command.";
        public static string[] Usages =
        {
            "answerrequest",
            "answerrequest <id> approve <rating(1-5)>",
            "answerrequest <id> deny"
        };
        public static string Permission = "Anyone can request, but only Moderators can review.";
        public static string Trello = "https://trello.com/c/PYYyovJK/3-answer-request";

        public async static void DoCommand()
        {
            await Program.Log();

            var message = Recieved.Message;
            string[] m = message.Content.Split();

            if (m.Length == 1)
                Request(message);
            if (m.Length >= 3 && Program.HasPerm(RequiredRoles))
            {
                ulong id;
                try { id = ulong.Parse(m[1]); }
                catch (Exception)
                {
                    await message.Channel.SendMessageAsync("❌ Invalid ID!");
                    return;
                }

                switch (m[2])
                {
                    case "approve":
                        if (m.Length == 3)
                        {
                            await message.Channel.SendMessageAsync("❌ Not enough parameters!");
                            return;
                        }
                        if (m.Length >= 5)
                        {
                            await message.Channel.SendMessageAsync("❌ Too many parameters!");
                            return;
                        }

                        byte score;
                        try { score = byte.Parse(m[3]); }
                        catch (Exception)
                        {
                            await message.Channel.SendMessageAsync("❌ Invalid score!");
                            return;
                        }
                        if (score < 1 || score > 5)
                        {
                            await message.Channel.SendMessageAsync("❌ The score can only be between 1 and 5!");
                            return;
                        }

                        Approve(message, id, score);
                        break;
                    case "deny":
                        if (m.Length == 2)
                        {
                            await message.Channel.SendMessageAsync("❌ Not enough parameters!");
                            return;
                        }
                        if (m.Length >= 4)
                        {
                            await message.Channel.SendMessageAsync("❌ Too many parameters!");
                            return;
                        }
                        Deny(message, id);
                        break;

                    default:
                        await message.Channel.SendMessageAsync("❌ Invalid action!");
                        break;
                }
            }
        }

        public async static void Request(SocketMessage message)
        {
            var requests = AnswerRequests.PullData();
            var answerRequestsChannel = (IMessageChannel)Program._client.GetChannel(BaseConfig.GetConfig().Channels.AnswerRequests);

            var responseEmbed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author
                        .WithName("Answer Request Sent")
                        .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/782963424191447061/noun_comment_3266585.png"); // comment by Larea from the Noun Project
                })
                .WithDescription($"{message.Author.Mention}\nYour request will be reviewed soon")
                .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name)
                .WithColor(new Color(0x7289DA)).Build();
            var response = await message.Channel.SendMessageAsync(null, embed: responseEmbed);

            var embedMessage = await answerRequestsChannel.SendMessageAsync(null, embed: new EmbedBuilder().Build());
            var requestEmbed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author
                        .WithName("Answer Request")
                        .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/782886030638055464/noun_Plus_1809808.png"); // Plus by sumhi_icon from the Noun Project
                })
                .WithDescription($"{message.Author.Mention} requested to review their answer in <#{message.Channel.Id}>.\nStatus: **Waiting...**\n\n Request's link:\n https://discord.com/channels/{((SocketGuildChannel)message.Channel).Guild.Id}/{message.Channel.Id}/{response.Id} \n\nID: `{embedMessage.Id}`")
                .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name).Build();
            await embedMessage.ModifyAsync(m => m.Embed = requestEmbed);

            var mention = await answerRequestsChannel.SendMessageAsync(((SocketGuildChannel)message.Channel).Guild.GetRole(782965011047514162).Mention); // AnswerRequest role pingelése a moderátorok értesítéséért.

            await message.Channel.DeleteMessageAsync(message);
            await answerRequestsChannel.DeleteMessageAsync(mention);

            requests.Add(new AnswerRequests(embedMessage.Id, message.Author.Id, requestEmbed.Description));
            AnswerRequests.PushData(requests);
        }

        public async static void Approve(SocketMessage message, ulong id, byte score)
        {
            var requests = AnswerRequests.PullData();
            if (requests.Count(x => x.ID == id) == 0)
            {
                await message.Channel.SendMessageAsync("❌ Unkown ID!");
                return;
            }
            var request = requests.Find(x => x.ID == id);

            var approvedEmbed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author
                        .WithName("Answer Request")
                        .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/782586791831666688/noun_checkmark_737739.png"); // checkmark by Vladimir from the Noun Project
                })
                .WithDescription(request.Description.Replace("Waiting...", $"Approved ({score})"))
                .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name)
                .WithColor(new Color(0x00DD00)).Build();
            var oldMsg = await ((IMessageChannel)Program._client.GetChannel(BaseConfig.GetConfig().Channels.AnswerRequests)).GetMessageAsync(request.ID);
            await ((IUserMessage)oldMsg).ModifyAsync(m => m.Embed = approvedEmbed);

            requests.Remove(request);
            AnswerRequests.PushData(requests);

            var userId = request.UserID;
            var members = Members.PullData();
            if (members.Count(x => x.ID == userId) == 0)
                members.Add(new Members(userId));
            members[members.IndexOf(members.Find(x => x.ID == userId))].Help += score;
            Members.PushData(members);

            await message.Channel.SendMessageAsync("Request approved");
        }

        public async static void Deny(SocketMessage message, ulong id)
        {
            var requests = AnswerRequests.PullData();
            if (requests.Count(x => x.ID == id) == 0)
            {
                await message.Channel.SendMessageAsync("❌ Unkown ID!");
                return;
            }
            var request = requests.Find(x => x.ID == id);

            var deniedEmbed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author
                        .WithName("Answer Request")
                        .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/782619217055842334/noun_Close_1984788.png"); // Close by Bismillah from the Noun Project
                })
                .WithDescription(request.Description.Replace("Waiting...", "Denied"))
                .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name)
                .WithColor(new Color(0xDD0000)).Build();
            var oldMsg = await ((IMessageChannel)Program._client.GetChannel(BaseConfig.GetConfig().Channels.AnswerRequests)).GetMessageAsync(request.ID);
            await ((IUserMessage)oldMsg).ModifyAsync(m => m.Embed = deniedEmbed);

            requests.Remove(request);
            AnswerRequests.PushData(requests);

            await message.Channel.SendMessageAsync("Request denied");
        }
    }
}
