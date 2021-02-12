using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using GroundedBot.Json;

namespace GroundedBot.Commands
{
    class PingRequest
    {
        public static List<ulong> RequiredRoles =
            new List<ulong>(BaseConfig.GetConfig().Roles.Mod);
        public static List<ulong> PtanProRoles =
            new List<ulong>(BaseConfig.GetConfig().Roles.PtanP);

        public static string[] Aliases =
        {
            "pingrequest",
            "ping-request",
            "pr",
            "ping"
        };
        public static string Description = "Request to ping a programming language role to get help faster.";
        public static string[] Usages =
        {
            "pingrequest <language>",
            "pingrequest <id> <approve/deny>"
        };
        public static string Permission = "Anyone can use it, with Ptan+ Pro, pinging is instant, but only Moderators can review.";
        public static string Trello = "https://trello.com/c/AAuBVeZY/2-ping-request";

        public async static void DoCommand()
        {
            var message = Recieved.Message;
            string[] m = message.Content.Split();

            if (m.Length == 1)
                return;

            await Program.Log("command", "");

            if (m.Length >= 3 && Program.HasPerm(RequiredRoles))
            {
                if (m.Length >= 4)
                {
                    await message.Channel.SendMessageAsync("❌ Too many parameters!");
                    return;
                }

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
                        Approve(message, id);
                        break;
                    case "deny":
                        Deny(message, id);
                        break;

                    default:
                        await message.Channel.SendMessageAsync("❌ Invalid action!");
                        return;
                }
            }
            else
            {
                var role = ((SocketGuildChannel)message.Channel).Guild.GetRole(Program.GetRoleId(m[1]));
                if (role != null)
                    Request(message, role);
            }
        }

        async static void Request(SocketMessage message, SocketRole role)
        {
            var requests = PingRequests.PullData();
            Random r = new Random();
            var pingRequestsChannel = (IMessageChannel)Program._client.GetChannel(BaseConfig.GetConfig().Channels.PingRequests);

            if (Program.HasPerm(PtanProRoles))
            {
                ulong[] allowedRoles =
                {
                    643057615542157325,
                    642999690140450816,
                    693928850576506962,
                    718745058328707144,
                    656934086295945222,
                    656934935978049586,
                    656935125610790932,
                    656935401889464320
                };

                if (allowedRoles.Contains(role.Id))
                {
                    await message.Channel.SendMessageAsync(role.Mention);
                    await message.Channel.DeleteMessageAsync(message);
                }
                else
                    await message.Channel.SendMessageAsync("❌ No, I will not ping that!");
                return;
            }

            var responseEmbed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author
                        .WithName("Ping Request Sent")
                        .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/782336504462049300/noun_At_7133.png"); // At by Márcio Duarte from the Noun Project
                })
                .WithDescription($"{role.Mention}\nYour request will be reviewed soon")
                .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name)
                .WithColor(role.Color).Build();
            var response = await message.Channel.SendMessageAsync(null, embed: responseEmbed);

            var embedMessage = await pingRequestsChannel.SendMessageAsync(null, embed: new EmbedBuilder().Build());
            var requestEmbed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author
                        .WithName("Ping Request")
                        .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/782886030638055464/noun_Plus_1809808.png"); // Plus by sumhi_icon from the Noun Project
                })
                .WithDescription($"{message.Author.Mention} requested to ping {role.Mention} in <#{message.Channel.Id}>.\nStatus: **Waiting...**\n\n Request's link:\n https://discord.com/channels/{((SocketGuildChannel)message.Channel).Guild.Id}/{message.Channel.Id}/{response.Id} \n\nID: `{embedMessage.Id}`")
                .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name).Build();
            await embedMessage.ModifyAsync(m => m.Embed = requestEmbed);

            var mention = await pingRequestsChannel.SendMessageAsync(((SocketGuildChannel)message.Channel).Guild.GetRole(782879567873310740).Mention); // PingRequest role pingelése a moderátorok értesítéséért.

            await message.Channel.DeleteMessageAsync(message);
            await pingRequestsChannel.DeleteMessageAsync(mention);

            requests.Add(new PingRequests(embedMessage.Id, role.Id, message.Channel.Id, requestEmbed.Description));
            PingRequests.PushData(requests);
        }

        async static void Approve(SocketMessage message, ulong id)
        {
            var requests = PingRequests.PullData();
            if (requests.Count(x => x.ID == id) == 0)
            {
                await message.Channel.SendMessageAsync("❌ Unkown ID!");
                return;
            }
            var request = requests.Find(x => x.ID == id);
            var role = ((SocketGuildChannel)message.Channel).Guild.GetRole(request.RoleID);

            await ((IMessageChannel)Program._client.GetChannel(request.ChannelID)).SendMessageAsync(role.Mention);

            var approvedEmbed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author
                        .WithName("Ping Request")
                        .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/782586791831666688/noun_checkmark_737739.png"); // checkmark by Vladimir from the Noun Project
                })
                .WithDescription(request.Description.Replace("Waiting...", "Approved"))
                .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name)
                .WithColor(new Color(0x00DD00)).Build();
            var oldMsg = await ((IMessageChannel)Program._client.GetChannel(BaseConfig.GetConfig().Channels.PingRequests)).GetMessageAsync(request.ID);
            await ((IUserMessage)oldMsg).ModifyAsync(m => m.Embed = approvedEmbed);

            requests.Remove(request);
            PingRequests.PushData(requests);

            await message.Channel.SendMessageAsync("Request approved");
        }

        async static void Deny(SocketMessage message, ulong id)
        {
            var requests = PingRequests.PullData();
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
                        .WithName("Ping Request")
                        .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/782619217055842334/noun_Close_1984788.png"); // Close by Bismillah from the Noun Project
                })
                .WithDescription(request.Description.Replace("Waiting...", "Denied"))
                .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name)
                .WithColor(new Color(0xDD0000)).Build();
            var oldMsg = await ((IMessageChannel)Program._client.GetChannel(BaseConfig.GetConfig().Channels.PingRequests)).GetMessageAsync(request.ID);
            await ((IUserMessage)oldMsg).ModifyAsync(m => m.Embed = deniedEmbed);

            requests.Remove(request);
            PingRequests.PushData(requests);

            await message.Channel.SendMessageAsync("Request denied");
        }
    }
}
