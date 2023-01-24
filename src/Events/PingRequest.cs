using Discord;
using GroundedBot.Helpers;
using GroundedBot.Services;

namespace GroundedBot.Events;

public class PingRequest : MessageEvent
{
	public async Task Execute()
	{
		string[] m = Context.Message.Content.Split('@');
		if (m.Length != 2 || m[0].Length > 1 || ((IGuildUser)Context.User).GuildPermissions.Administrator)
			return;

		IRole role;
		try
		{
			role = Context.Guild.GetRole(ulong.Parse(m[1][1..^1]));
			if (role == null)
				throw new Exception();
		}
		catch (Exception)
		{
			try { role = Context.Guild.Roles.First(r => r.Name.ToLowerInvariant() == m[1].ToLowerInvariant()); }
			catch (Exception) { return; }
		}

		GuildSettings gs = Mongo.GetSettings(Context.Guild.Id);
		if (!gs.Role.PingRequests.Contains(role.Id))
			return;

		await Context.Message.DeleteAsync();

		if (((IGuildUser)Context.User).RoleIds.Contains(gs.Role.Moderator))
		{
			await Context.Channel.SendMessageAsync(role.Mention);
			return;
		}

		EmbedBuilder pingEmbed = new EmbedBuilder()
			.WithAuthor("Jóváhagyásra vár...")
			.WithDescription($"{role.Mention}")
			.WithColor(role.Color);
		IUserMessage pingMsg = await Context.Channel.SendMessageAsync(embed: pingEmbed.Build());

		EmbedBuilder requestEmbed = new EmbedBuilder()
			.WithAuthor("Új ping kérelem", Icons.Plus)
			.WithDescription($"**Felhasználó:** {Context.User.Mention} *({Context.User})* `{Context.User.Id}`\n**Role:** {role.Mention}\n[Üzenet]({pingMsg.GetJumpUrl()})")
			.WithFooter("Moderátor jóváhagyására vár...")
			.WithColor(Colors.Orange);
		ComponentBuilder components = new ComponentBuilder()
			.WithButton(customId: $"prbutton-approve:{Context.Channel.Id},{pingMsg.Id},{role.Id}", style: ButtonStyle.Success, emote: this.Emote.GetEmote("util_checkmark"))
			.WithButton(customId: $"prbutton-reject:{Context.Channel.Id},{pingMsg.Id}", style: ButtonStyle.Danger, emote: Emote.GetEmote("util_cross"));

		try { await ((IMessageChannel)Context.Guild.GetChannel(gs.Channel.PingRequest)).SendMessageAsync(Context.Guild.GetRole(gs.Role.PingRequest).Mention, embed: requestEmbed.Build(), components: components.Build()); }
		catch (Exception e) { await pingMsg.ModifyAsync(m => m.Embed = Embeds.Error("Nem sikerült elküldeni a ping kérést", $"Szólj egy adminisztrátornak, hogy nem találok `PingRequest` role-t, `PingRequest` csatornát, vagy nincs jogom üzenetet küldeni oda!\n```{e.Message}```")); }
	}
}
