using Discord;
using Discord.WebSocket;

namespace GroundedBot.Services;

public class EmoteService
{
	private List<GuildEmote> emotes;

	public GuildEmote GetEmote(string name) =>
		this.emotes.Find(e => e.Name == name);

	public void LoadEmotes(DiscordSocketClient client, List<ulong> emoteGuilds)
	{
		this.emotes = new();
		foreach (ulong guildId in emoteGuilds)
		{
			SocketGuild guild = client.GetGuild(guildId);
			this.emotes.AddRange(guild.Emotes);
		}
	}
}
