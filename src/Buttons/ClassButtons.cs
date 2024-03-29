using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MongoDB.Driver;
using GroundedBot.Modals;
using GroundedBot.Services;
using GroundedBot.Helpers;

namespace GroundedBot.Buttons;

public class ClassButtons : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
	public DiscordSocketClient Client { get; set; }
	public MongoService Mongo { get; set; }

	[ComponentInteraction("classbutton-create")]
	public async Task Create() => await RespondWithModalAsync<ClassModal>("classmodal-create");

	[ComponentInteraction("classbutton-approve:*")]
	public async Task Approve(string classId)
	{
		if (!((IGuildUser)Context.User).RoleIds.Contains(Mongo.GetSettings(Context.Guild.Id).Role.Moderator))
		{
			await DeferAsync();
			return;
		}
		TanClass cClass = Mongo.Classes.AsQueryable().First(c => c.ID == int.Parse(classId));

		Modal modal = new ModalBuilder()
			.WithCustomId($"classmodal-approve:{classId},{Context.Interaction.Message.Id}")
			.WithTitle("Osztály elfogadása")
			.AddTextInput("Téma", "tb-theme", placeholder: "Osztályod témája/címe", minLength: 1, maxLength: 50, value: cClass.Theme)
			.AddTextInput("Leírás", "tb-description", TextInputStyle.Paragraph, "Rövid ismertető magadról és a tananyagodról.", 50, 1000, value: cClass.Description)
			.AddTextInput("Tananyag", "tb-url", placeholder: "Link a részletes tananyagodhoz", required: false, value: cClass.CourseURL)
			.Build();

		await RespondWithModalAsync(modal);
	}

	[ComponentInteraction("classbutton-reject:*")]
	public async Task Reject(string classId)
	{
		if (!((IGuildUser)Context.User).RoleIds.Contains(Mongo.GetSettings(Context.Guild.Id).Role.Moderator))
		{
			await DeferAsync();
			return;
		}
		await RespondWithModalAsync<RejectClassModal>($"classmodal-reject:{classId},{Context.Interaction.Message.Id}");
	}

	[ComponentInteraction("classbutton-join:*")]
	public async Task Join(string classId)
	{
		await DeferAsync();
		GuildSettings gs = Mongo.GetSettings(Context.Guild.Id);
		TanClass cClass = Mongo.Classes.AsQueryable().First(c => c.ID == int.Parse(classId));
		if (cClass.Students.Contains(Context.User.Id) || cClass.Teacher == Context.User.Id)
			return;

		try
		{
			await ((IGuildUser)Context.User).AddRoleAsync(gs.Role.Student);
		}
		catch (Exception e)
		{
			await FollowupAsync(embed: Embeds.Error("Nem sikerült csatlakozni az osztályhoz", $"Szólj egy adminisztrátornak, hogy nem találok `Student` role-t, vagy nem tudom ráadni a felhasználóra!\n```{e.Message}```"), ephemeral: true);
			return;
		}
		try
		{
			OverwritePermissions perms = new OverwritePermissions
			(
				viewChannel: PermValue.Allow,
				sendMessages: PermValue.Allow,
				connect: PermValue.Allow
			);
			await Context.Guild.Channels.First(c => c.Id == cClass.TextChannel).AddPermissionOverwriteAsync(Context.User, perms);
			await Context.Guild.Channels.First(c => c.Id == cClass.VoiceChannel).AddPermissionOverwriteAsync(Context.User, perms);
		}
		catch (Exception e)
		{
			await FollowupAsync(embed: Embeds.Error("Nem sikerült csatlakozni az osztályhoz", $"Szólj egy adminisztáronak, hogy nincs elég jogom módosítani a csatornákat!\n```{e.Message}```"), ephemeral: true);
			await ((IGuildUser)Context.User).RemoveRoleAsync(gs.Role.Student);
			return;
		}

		await FollowupAsync(embed: Embeds.Success("Csatlakoztál az osztályhoz", $"Informálódni, tanulni és beszélgetni a(z) <#{cClass.TextChannel}> szobában, illetve a(z) <#{cClass.VoiceChannel}> hangcsatornában tudsz. Jó tanulást kíván a **{Context.Guild.Name}**!"), ephemeral: true);
		cClass.Students.Add(Context.User.Id);
		await Mongo.Classes.ReplaceOneAsync(c => c.ID == int.Parse(classId), cClass);
	}

	[ComponentInteraction("classbutton-leave:*")]
	public async Task Leave(string classId)
	{
		await DeferAsync();
		GuildSettings gs = Mongo.GetSettings(Context.Guild.Id);
		TanClass cClass = Mongo.Classes.AsQueryable().First(c => c.ID == int.Parse(classId));
		if (!cClass.Students.Contains(Context.User.Id))
			return;

		try
		{
			await ((IGuildUser)Context.User).AddRoleAsync(gs.Role.Student);
			await ((IGuildUser)Context.User).RemoveRoleAsync(gs.Role.Student);
		}
		catch (Exception e)
		{
			await FollowupAsync(embed: Embeds.Error("Nem sikerült kiléptetni az osztályból", $"Szólj egy adminisztrátornak, hogy nem találok `Student` role-t, vagy tudom elvenni!\n```{e.Message}```"));
			return;
		}
		try
		{
			OverwritePermissions perms = new OverwritePermissions
			(
				viewChannel: PermValue.Inherit,
				sendMessages: PermValue.Inherit,
				connect: PermValue.Inherit
			);
			await Context.Guild.Channels.First(c => c.Id == cClass.TextChannel).AddPermissionOverwriteAsync(Context.User, perms);
			await Context.Guild.Channels.First(c => c.Id == cClass.VoiceChannel).AddPermissionOverwriteAsync(Context.User, perms);
		}
		catch (Exception e)
		{
			await FollowupAsync(embed: Embeds.Error("Nem sikerült kiléptetni az osztályból", $"Szólj egy adminisztáronak, hogy nincs elég jogom módosítani a csatornákat!\n```{e.Message}```"), ephemeral: true);
			await ((IGuildUser)Context.User).AddRoleAsync(gs.Role.Student);
			return;
		}

		cClass.Students.Remove(Context.User.Id);
		await Mongo.Classes.ReplaceOneAsync(c => c.ID == int.Parse(classId), cClass);
	}

	[ComponentInteraction("classbutton-modify:*")]
	public async Task Modify(string classIdStr)
	{
		int classId = int.Parse(classIdStr);
		GuildSettings gs = Mongo.GetSettings(Context.Guild.Id);
		TanClass cClass = Mongo.Classes.AsQueryable()
			.First(c => c.ID == classId);
		IReadOnlyCollection<ulong> roles = ((IGuildUser)Context.User)
			.RoleIds;
		if (Context.User.Id != cClass.Teacher &&
			!roles.Contains(gs.Role.Moderator))
		{
			await DeferAsync();
			return;
		}

		Modal modal = new ModalBuilder()
			.WithCustomId(
				"classmodal-modify:" +
				$"{classId}," +
				$"{Context.Interaction.Message.Id}")
			.WithTitle("Osztály módosítása")
			.AddTextInput(
				"Téma",
				"tb-theme",
				placeholder: "Osztályod témája/címe",
				minLength: 1, maxLength: 50,
				value: cClass.Theme)
			.AddTextInput(
				"Leírás",
				"tb-description",
				TextInputStyle.Paragraph,
				"Rövid ismertető magadról és a tananyagodról.",
				50, 1000,
				value: cClass.Description)
			.AddTextInput(
				"Tananyag",
				"tb-url",
				placeholder: "Link a részletes tananyagodhoz",
				value: cClass.CourseURL)
			.Build();

		await RespondWithModalAsync(modal);
	}

	[ComponentInteraction("classbutton-delete:*")]
	public async Task Delete(string classIdStr)
	{
		int classId = int.Parse(classIdStr);
		GuildSettings gs = Mongo.GetSettings(Context.Guild.Id);
		TanClass cClass = Mongo.Classes.AsQueryable()
			.First(c => c.ID == classId);
		IReadOnlyCollection<ulong> roles = ((IGuildUser)Context.User).RoleIds;
		if (Context.User.Id != cClass.Teacher &&
			!roles.Contains(gs.Role.Moderator))
		{
			await DeferAsync();
			return;
		}

		Modal modal = new ModalBuilder()
			.WithCustomId(
				"classmodal-delete:" +
				$"{classId}," +
				$"{Context.Interaction.Message.Id}")
			.WithTitle("Osztály törlése")
			.AddTextInput(
				"Törlés megerősítése",
				"tb-reason",
				placeholder:
					cClass.Theme,
				minLength: 1, maxLength: 50)
			.Build();

		await RespondWithModalAsync(modal);
	}

	[ComponentInteraction("classbutton-forcedelete:*,*")]
	public async Task ForceDelete(string classIdStr, string messageId)
	{
		await DeferAsync();

		int classId = int.Parse(classIdStr);
		GuildSettings gs = Mongo.GetSettings(Context.Guild.Id);
		TanClass cClass = Mongo.Classes.AsQueryable()
		.First(c => c.ID == classId);

		try
		{
			foreach (ulong id in cClass.Students)
				if (Mongo.Classes.AsQueryable()
					.Count(c => c.Students.Contains(id)) == 1
				)
					await Context.Guild
						.GetUser(id)
						.RemoveRoleAsync(gs.Role.Student);
		}
		catch (Exception)
		{
			await FollowupAsync(
				embed: Embeds.Error(
					"Student role nem lett elvéve"),
					ephemeral: true);
		}
		try
		{
			if (Mongo.Classes.AsQueryable()
				.Count(c => c.Teacher == cClass.Teacher) == 1
			)
				await Context.Guild
					.GetUser(cClass.Teacher)
					.RemoveRoleAsync(gs.Role.Teacher);
		}
		catch (Exception)
		{
			await FollowupAsync(
				embed: Embeds.Error(
					"Teacher role nem lett elvéve"),
				ephemeral: true);
		}
		try
		{
			await Context.Guild
				.GetChannel(cClass.TextChannel)
				.DeleteAsync();
			await Context.Guild
				.GetChannel(cClass.VoiceChannel)
				.DeleteAsync();
		}
		catch (Exception)
		{
			await FollowupAsync(
				embed: Embeds.Error(
					"A csatornák nem lettek törölve"),
					ephemeral: true);
		}

		await Context.Channel
			.DeleteMessageAsync(ulong.Parse(messageId));
		await Mongo.Classes
			.DeleteOneAsync(c => c.ID == classId);

		try
		{
			if (Context.User.Id != cClass.Teacher)
				await Context.Guild
					.GetUser(cClass.Teacher)
					.SendMessageAsync(
						embed: Embeds.Error(
							"Egy osztályod törölve lett",
							$"Osztály: **{cClass.Theme}**\n" +
							$"Szerver: **{Context.Guild.Name}**"));
		}
		catch { }
	}
}
