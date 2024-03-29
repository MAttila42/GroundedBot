using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MongoDB.Driver;
using GroundedBot.Services;
using GroundedBot.Helpers;

namespace GroundedBot.Modals;

public class ClassModal : IModal
{
	public string Title => "Osztálykészítés";

	[InputLabel("Téma")]
	[ModalTextInput("tb-theme", placeholder: "Osztályod témája/címe", minLength: 1, maxLength: 50)]
	public string Theme { get; set; }

	[InputLabel("Leírás")]
	[ModalTextInput("tb-description", TextInputStyle.Paragraph, placeholder: "Rövid ismertető magadról és a tananyagodról.", minLength: 50, maxLength: 1000)]
	public string Description { get; set; }

	[InputLabel("Tananyag")]
	[ModalTextInput("tb-url", placeholder: "Link a részletes tananyagodhoz")]
	[RequiredInput(false)]
	public string CourseURL { get; set; }
}
public class RejectClassModal : IModal
{
	public string Title => "Osztály elutasítása";

	[InputLabel("Indok")]
	[ModalTextInput("tb-reason", TextInputStyle.Paragraph, "Elutasítás indoklása")]
	public string Reason { get; set; }
}

public class ClassModals : InteractionModuleBase
{
	public DiscordSocketClient Client { get; set; }
	public EmoteService Emote { get; set; }
	public MongoService Mongo { get; set; }

	[ModalInteraction("classmodal-create")]
	public async Task Create(ClassModal modal)
	{
		int id;
		try { id = Mongo.Classes.AsQueryable().Select(c => c.ID).Max() + 1; }
		catch (Exception) { id = 0; }
		TanClass newClass = new()
		{
			ID = id,
			Guild = Context.Guild.Id,
			Teacher = Context.User.Id,
			Theme = modal.Theme,
			Description = modal.Description,
			CourseURL = modal.CourseURL == "" ? "-" : modal.CourseURL,
			Approved = false,
			TextChannel = 0,
			VoiceChannel = 0,
			Students = new List<ulong>()
		};

		EmbedBuilder embed = new EmbedBuilder()
			.WithAuthor("Új osztály lett létrehozva", Icons.Plus)
			.AddField("Tanár", Context.User.Mention)
			.AddField("Téma", newClass.Theme)
			.AddField("Leírás", newClass.Description)
			.AddField("Tananyag", newClass.CourseURL)
			.WithFooter("Moderátor jóváhagyására vár...")
			.WithColor(Colors.Orange);
		ComponentBuilder components = new ComponentBuilder()
			.WithButton(customId: $"classbutton-approve:{newClass.ID}", style: ButtonStyle.Success, emote: Emote.GetEmote("util_checkmark"))
			.WithButton(customId: $"classbutton-reject:{newClass.ID}", style: ButtonStyle.Danger, emote: Emote.GetEmote("util_cross"));

		try
		{
			await ((IMessageChannel)await Context.Guild.GetChannelAsync(Mongo.GetSettings(Context.Guild.Id).Channel.ClassRequest)).SendMessageAsync(Context.Guild.GetRole(Mongo.GetSettings(Context.Guild.Id).Role.ClassRequest).Mention, embed: embed.Build(), components: components.Build());
		}
		catch (Exception e)
		{
			await RespondAsync(embed: Embeds.Error("Nem sikerült létrehozni az osztályt", $"Szólj egy adminisztrátornak, hogy nem találok `ClassRequest` csatornát, nem tudok oda üzenetet írni, vagy nincs `WaitingClass` role!\n```{e.Message}```"), ephemeral: true);
			return;
		}

		await Mongo.Classes.InsertOneAsync(newClass);
		await RespondAsync(embed: Embeds.Success("Létrehozási kérelem elküldve", $"A jóváhagyásról vagy elutasításról privát üzenetben kapsz értesítést. (Győződj meg róla, hogy {Client.CurrentUser.Mention} tud üzenetet küldeni neked!)"), ephemeral: true);
	}

	[ModalInteraction("classmodal-approve:*,*")]
	public async Task Approve(string classId, string messageIdStr, ClassModal modal)
	{
		GuildSettings gs = Mongo.GetSettings(Context.Guild.Id);
		ulong messageId = ulong.Parse(messageIdStr);

		TanClass cClass = Mongo.Classes.AsQueryable().First(c => c.ID == int.Parse(classId));
		cClass.Theme = modal.Theme;
		cClass.Description = modal.Description;
		cClass.CourseURL = modal.CourseURL;
		cClass.Approved = true;

		EmbedBuilder embed = new EmbedBuilder()
			.WithAuthor(cClass.Theme, Icons.Book)
			.AddField("Tanár", (await Context.Guild.GetUserAsync(cClass.Teacher)).Mention)
			.AddField("Leírás", cClass.Description)
			.AddField("Tananyag", cClass.CourseURL)
			.WithFooter(Context.Guild.Name)
			.WithColor(Colors.Blurple);
		ComponentBuilder components = new ComponentBuilder()
			.WithButton("Csatlakozás", $"classbutton-join:{cClass.ID}", ButtonStyle.Success)
			.WithButton("Módosítás", $"classbutton-modify:{cClass.ID}", ButtonStyle.Primary)
			.WithButton("Törlés", $"classbutton-delete:{cClass.ID}", ButtonStyle.Danger);

		await ((IMessageChannel)await Context.Guild.GetChannelAsync(Context.Interaction.ChannelId ?? 0)).ModifyMessageAsync(messageId, m => m.Content = $"{Context.Guild.GetRole(gs.Role.ClassRequest).Mention} {Emote.GetEmote("util_loading")}");
		await DeferAsync();

		IGuildUser teacher;
		try
		{
			teacher = await Context.Guild.GetUserAsync(cClass.Teacher);
			await teacher.AddRoleAsync(gs.Role.Teacher);
		}
		catch (Exception e)
		{
			await FollowupAsync(embed: Embeds.Error("Nem sikerült eldogadni az osztályt", $"Szólj egy adminisztrátornak, hogy nem találok `Teacher` role-t, vagy nem tudom ráadni a felhasználóra!\n```{e.Message}```"), ephemeral: true);
			await Revert(messageId);
			return;
		}

		ITextChannel text;
		IVoiceChannel voice;
		try
		{
			List<Overwrite> perms = new List<Overwrite>
				{
					new Overwrite(Context.Guild.EveryoneRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Deny)),
					new Overwrite(gs.Role.Moderator, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Allow, connect: PermValue.Allow)),
					new Overwrite(Client.CurrentUser.Id, PermissionTarget.User, new OverwritePermissions(manageChannel: PermValue.Allow, viewChannel: PermValue.Allow)),
					new Overwrite(cClass.Teacher, PermissionTarget.User, new OverwritePermissions
					(
						manageChannel: PermValue.Allow,
						viewChannel: PermValue.Allow,
						sendMessages: PermValue.Allow,
						manageMessages: PermValue.Allow,
						mentionEveryone: PermValue.Allow,
						connect: PermValue.Allow,
						muteMembers: PermValue.Allow,
						deafenMembers: PermValue.Allow
					))
				};
			text = await Context.Guild.CreateTextChannelAsync(cClass.Theme, c =>
			{
				c.CategoryId = gs.Category.Teaching;
				c.PermissionOverwrites = perms;
			});
			voice = await Context.Guild.CreateVoiceChannelAsync(cClass.Theme, c =>
			{
				c.CategoryId = gs.Category.Teaching;
				c.PermissionOverwrites = perms;
			});
			cClass.TextChannel = text.Id;
			cClass.VoiceChannel = voice.Id;
		}
		catch (Exception e)
		{
			await FollowupAsync(embed: Embeds.Error("Nem Sikerült elfogadni az osztályt", $"Szólj egy adminisztáronak, hogy nem találok `Teaching` kategóriát, vagy nincs elég jogom létrehozni a csatornákat!\n```{e.Message}```"), ephemeral: true);
			await Revert(messageId, teacher);
			return;
		}
		try
		{
			IMessageChannel channel = await Context.Guild.GetChannelAsync(gs.Channel.Classes) as IMessageChannel;
			IUserMessage msg = await channel.SendMessageAsync(Context.Guild.GetRole(gs.Role.NewClass).Mention, embed: embed.Build(), components: components.Build());
			await msg.ModifyAsync(m => m.Content = "");
		}
		catch (Exception e)
		{
			await FollowupAsync(embed: Embeds.Error("Nem sikerült elfogadni az osztályt", $"Szólj egy adminisztrátornak, hogy nem találok `Classes` csatornát, nem tudok oda üzenetet írni, vagy nincs `NewClass` role!\n```{e.Message}```"), ephemeral: true);
			await Revert(messageId, teacher, text, voice);
			return;
		}

		ComponentBuilder leaveButton = new ComponentBuilder()
			.WithButton("Kilépés", $"classbutton-leave:{cClass.ID}", ButtonStyle.Danger);
		await (await text.SendMessageAsync(embed: Embeds.Info(cClass.Theme, "Az osztályból való kilépéshez kattints a `Kilépés` gombra"), components: leaveButton.Build())).PinAsync();
		await Mongo.Classes.ReplaceOneAsync(c => c.ID == int.Parse(classId), cClass);
		try
		{
			await Context.Channel.DeleteMessageAsync(messageId);
		}
		catch (Exception e)
		{
			await FollowupAsync(embed: Embeds.Error("Nem sikerült elfogadni az osztályt", $"Valószínűleg valaki nálad hamarabb kezelte a kérést.\n```{e.Message}```"), ephemeral: true);
			return;
		}

		try
		{
			await Client.GetUser(cClass.Teacher).SendMessageAsync(embed: Embeds.Success($"Egy osztályod létrehozása el lett fogadva", $"Osztály: **{cClass.Theme}**\nSzerver: **{Context.Guild}**"));
		}
		catch (Exception) { /*ignore*/ }
	}
	private async Task Revert(ulong messageId, IGuildUser teacher = null, ITextChannel text = null, IVoiceChannel voice = null)
	{
		GuildSettings gs = Mongo.GetSettings(Context.Guild.Id);
		await ((IMessageChannel)await Context.Guild.GetChannelAsync(Context.Interaction.ChannelId ?? 0)).ModifyMessageAsync(messageId, m => m.Content = Context.Guild.GetRole(gs.Role.ClassRequest).Mention);
		if (teacher != null)
			await teacher.RemoveRoleAsync(gs.Role.Teacher);
		if (text != null && voice != null)
		{
			await text.DeleteAsync();
			await voice.DeleteAsync();
		}
	}

	[ModalInteraction("classmodal-reject:*,*")]
	public async Task Reject(string classId, string messageId, RejectClassModal modal)
	{
		await DeferAsync();
		TanClass cClass = Mongo.Classes.AsQueryable().First(c => c.ID == int.Parse(classId));
		await Mongo.Classes.DeleteOneAsync(c => c.Teacher == cClass.Teacher && c.Theme == cClass.Theme);
		await Context.Channel.DeleteMessageAsync(ulong.Parse(messageId));
		try
		{
			await Client.GetUser(cClass.Teacher).SendMessageAsync(embed: Embeds.Error($"Egy osztályod létrehozása el lett utasítva", $"Osztály: **{cClass.Theme}**\nSzerver: **{Context.Guild}**\nIndoklás:\n```{modal.Reason}```"));
		}
		catch (Exception) { /*ignore*/ }
	}

	[ModalInteraction("classmodal-modify:*,*")]
	public async Task Modify(string classId, string messageId, ClassModal modal)
	{
		await DeferAsync();

		TanClass cClass = Mongo.Classes.AsQueryable().First(c => c.ID == int.Parse(classId));
		cClass.Theme = modal.Theme;
		cClass.Description = modal.Description;
		cClass.CourseURL = modal.CourseURL;

		EmbedBuilder embed = new EmbedBuilder()
			.WithAuthor(cClass.Theme, Icons.Book)
			.AddField("Tanár", (await Context.Guild.GetUserAsync(cClass.Teacher)).Mention)
			.AddField("Leírás", cClass.Description)
			.AddField("Tananyag", cClass.CourseURL)
			.WithFooter(Context.Guild.Name)
			.WithColor(Colors.Blurple);

		await Context.Channel.ModifyMessageAsync(ulong.Parse(messageId), m => m.Embed = embed.Build());
		Mongo.Classes.ReplaceOne(c => c.ID == cClass.ID, cClass);
	}

	[ModalInteraction("classmodal-delete:*,*")]
	public async Task Delete(
		string classIdStr, string messageId, RejectClassModal modal)
	{
		await DeferAsync();

		int classId = int.Parse(classIdStr);
		GuildSettings gs = Mongo.GetSettings(Context.Guild.Id);
		TanClass cClass = Mongo.Classes.AsQueryable()
		.First(c => c.ID == classId);

		if (modal.Reason != cClass.Theme)
		{
			await FollowupAsync(
				embed: Embeds.Error(
					"Hiba",
					"A megerősítő szöveg nem egyezik az osztály nevével."),
					ephemeral: true);
			return;
		}

		try
		{
			foreach (ulong id in cClass.Students)
				if (Mongo.Classes.AsQueryable()
					.Count(c => c.Students.Contains(id)) == 1
				)
					await (await Context.Guild
						.GetUserAsync(id))
						.RemoveRoleAsync(gs.Role.Student);
		}
		catch (Exception e)
		{
			await FollowupAsync(
				embed: Embeds.Error(
					"Nem sikerült törölni az osztályt",
					"Szólj egy adminisztrátornak, hogy nem találok `Student` role-t, vagy nincs jogom elvenni!\n" +
					$"```{e.Message}```"),
					ephemeral: true);
			return;
		}
		try
		{
			if (Mongo.Classes.AsQueryable()
				.Count(c => c.Teacher == cClass.Teacher) == 1
			)
				await (await Context.Guild
					.GetUserAsync(cClass.Teacher))
					.RemoveRoleAsync(gs.Role.Teacher);
		}
		catch (Exception e)
		{
			await FollowupAsync(
				embed: Embeds.Error(
					"Nem sikerült törölni az osztályt",
					"Szólj egy adminisztrátornak, hogy nem találok `Teacher` role-t, vagy nincs jogom elvenni!\n" +
					$"```{e.Message}```"),
				components: new ComponentBuilder()
					.WithButton("Force Delete", $"classbutton-forcedelete:{classIdStr},{messageId}")
					.Build(),
				ephemeral: true);
			await RevertStudentRoles(cClass.Students);
			return;
		}
		try
		{
			await (await Context.Guild
				.GetChannelAsync(cClass.TextChannel))
				.DeleteAsync();
			await (await Context.Guild
				.GetChannelAsync(cClass.VoiceChannel))
				.DeleteAsync();
		}
		catch (Exception e)
		{
			await FollowupAsync(
				embed: Embeds.Error(
					"Nem sikerült törölni az osztályt",
					"Szólj egy adminisztrátornak, hogy nincs jogom törölni a csatornákat!\n" +
					$"```{e.Message}```"),
					ephemeral: true);
			await RevertStudentRoles(cClass.Students);
			await (await Context.Guild
				.GetUserAsync(cClass.Teacher))
				.AddRoleAsync(gs.Role.Teacher);
			return;
		}

		await Context.Channel
			.DeleteMessageAsync(ulong.Parse(messageId));
		await Mongo.Classes
			.DeleteOneAsync(c => c.ID == classId);

		if (Context.User.Id != cClass.Teacher)
			await (await Context.Guild
				.GetUserAsync(cClass.Teacher))
				.SendMessageAsync(
					embed: Embeds.Error(
						"Egy osztályod törölve lett",
						$"Osztály: **{cClass.Theme}**\n" +
						$"Szerver: **{Context.Guild.Name}**"));
	}
	private async Task RevertStudentRoles(List<ulong> students)
	{
		GuildSettings gs = Mongo.GetSettings(Context.Guild.Id);
		foreach (ulong id in students)
			await (await Context.Guild
				.GetUserAsync(id))
				.AddRoleAsync(gs.Role.Student);
	}
}
