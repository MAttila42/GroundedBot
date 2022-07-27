using Discord;
using Discord.WebSocket;

namespace GroundedBot.Services
{
    public class EmojiService
    {
        private List<GuildEmote> emojis;

        public GuildEmote GetEmoji(string name) => this.emojis.Find(e => e.Name == name);

        public void LoadEmojis(DiscordSocketClient client, List<ulong> emojiServers)
        {
            this.emojis = new List<GuildEmote>();
            foreach (ulong s in emojiServers)
                foreach (GuildEmote e in client.GetGuild(s).Emotes)
                    this.emojis.Add(e);
        }
    }
}
