using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Discord.Webhook;
using GroundedBot.Json;

namespace GroundedBot.Events
{
    class EmotePair
    {
        public string Name { get; set; }
        public Emote Emote { get; set; }
        public EmotePair(string name, Emote emote)
        {
            Name = name;
            Emote = emote;
        }
    }
    class Emojify
    {
        public static List<ulong> RequiredRoles =
            new List<ulong>(BaseConfig.GetConfig().Roles.PtanS);
        public static List<ulong> PtanProRoles =
            new List<ulong>(BaseConfig.GetConfig().Roles.PtanP);

        public async static void DoEvent()
        {
            if (!Program.HasPerm(RequiredRoles))
                return;

            var message = Recieved.Message;
            string output = message.Content;
            string[] m = message.Content.Split(':');
            bool containsEmote = false;

            if (Program.HasPerm(PtanProRoles))
            {
                Emote emote;
                ulong emoteId = 0;
                var emotes = new List<EmotePair>();
                foreach (var i in m)
                {
                    try
                    {
                        if (emoteId != 0 && ulong.Parse(i.Substring(0, 18)) != 0)
                            return;
                    }
                    catch (Exception) { }
                    emote = Program._client.Guilds.SelectMany(x => x.Emotes).FirstOrDefault(x => x.Name == i);
                    try { emoteId = emote.Id; }
                    catch (Exception) { emoteId = 0; }
                    if (emote != null)
                    {
                        if (emotes.Count(x => x.Name == i) < 1)
                            emotes.Add(new EmotePair(i, emote));
                        containsEmote = true;
                    }
                }
                if (containsEmote)
                    foreach (var i in emotes)
                        output = output.Replace($":{i.Name}:", i.Emote.ToString());
            }
            else
            {
                try
                {
                    output = $"{Program._client.Guilds.SelectMany(x => x.Emotes).FirstOrDefault(x => x.Name == m[1])}";
                    containsEmote = true;
                }
                catch (Exception) { }
            }

            if (containsEmote)
            {
                var webhooks = await ((ITextChannel)message.Channel).GetWebhooksAsync();
                IWebhook webhook;
                try { webhook = webhooks.First(); }
                catch (Exception) { webhook = await ((ITextChannel)message.Channel).CreateWebhookAsync("emoji"); }
                var webhookClient = new DiscordWebhookClient(webhook);
                await webhookClient.SendMessageAsync(output, username: ((SocketGuildUser)message.Author).Nickname == null ? message.Author.Username : ((SocketGuildUser)message.Author).Nickname, avatarUrl: message.Author.GetAvatarUrl() == null ? message.Author.GetDefaultAvatarUrl() : message.Author.GetAvatarUrl(), allowedMentions: AllowedMentions.None);
                try { await message.Channel.DeleteMessageAsync(message); }
                catch (Exception) { }
            }
        }
    }
}
