using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MongoDB.Driver;
using GroundedBot.Services;

namespace GroundedBot.Modals
{
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
		public DiscordSocketClient _client { get; set; }
		public EmojiService _emoji { get; set; }
		public MongoService _mongo { get; set; }

		[ModalInteraction("classmodal-create")]
		public async Task Create(ClassModal modal)
		{
			int id;
			try { id = _mongo.Classes.AsQueryable().Select(c => c.ID).Max() + 1; }
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
				.WithAuthor("Új osztály lett létrehozva", EmbedService.PlusIcon)
				.AddField("Tanár", Context.User.Mention)
				.AddField("Téma", newClass.Theme)
				.AddField("Leírás", newClass.Description)
				.AddField("Tananyag", newClass.CourseURL)
				.WithFooter("Moderátor jóváhagyására vár...")
				.WithColor(EmbedService.Orange);
			ComponentBuilder components = new ComponentBuilder()
				.WithButton(customId: $"classbutton-approve:{newClass.ID}", style: ButtonStyle.Success, emote: _emoji.GetEmoji("util_checkmark"))
				.WithButton(customId: $"classbutton-reject:{newClass.ID}", style: ButtonStyle.Danger, emote: _emoji.GetEmoji("util_cross"));

			try
			{
				await ((IMessageChannel)await Context.Guild.GetChannelAsync(_mongo.GetGuildSettings(Context.Guild.Id).Channel.ClassRequest)).SendMessageAsync(Context.Guild.GetRole(_mongo.GetGuildSettings(Context.Guild.Id).Role.ClassRequest).Mention, embed: embed.Build(), components: components.Build());
			}
			catch (Exception e)
			{
				await RespondAsync(embed: EmbedService.Error("Nem sikerült létrehozni az osztályt", $"Szólj egy adminisztrátornak, hogy nem találok `ClassRequest` csatornát, nem tudok oda üzenetet írni, vagy nincs `WaitingClass` role!\n```{e.Message}```"), ephemeral: true);
				return;
			}

			await _mongo.Classes.InsertOneAsync(newClass);
			await RespondAsync(embed: EmbedService.Success("Létrehozási kérelem elküldve", $"A jóváhagyásról vagy elutasításról privát üzenetben kapsz értesítést. (Győződj meg róla, hogy {_client.CurrentUser.Mention} tud üzenetet küldeni neked!)"), ephemeral: true);
		}

		[ModalInteraction("classmodal-approve:*,*")]
		public async Task Approve(string classId, string messageIdStr, ClassModal modal)
		{
			GuildSettings gs = _mongo.GetGuildSettings(Context.Guild.Id);
			ulong messageId = ulong.Parse(messageIdStr);

			TanClass cClass = _mongo.Classes.AsQueryable().First(c => c.ID == int.Parse(classId));
			cClass.Theme = modal.Theme;
			cClass.Description = modal.Description;
			cClass.CourseURL = modal.CourseURL;
			cClass.Approved = true;

			EmbedBuilder embed = new EmbedBuilder()
				.WithAuthor(cClass.Theme, EmbedService.BookIcon)
				.AddField("Tanár", (await Context.Guild.GetUserAsync(cClass.Teacher)).Mention)
				.AddField("Leírás", cClass.Description)
				.AddField("Tananyag", cClass.CourseURL)
				.WithFooter(Context.Guild.Name)
				.WithColor(EmbedService.Blurple);
			ComponentBuilder components = new ComponentBuilder()
				.WithButton("Csatlakozás", $"classbutton-join:{cClass.ID}", ButtonStyle.Success)
				.WithButton("Törlés", $"classbutton-delete:{cClass.ID}", ButtonStyle.Danger);

			await ((IMessageChannel)await Context.Guild.GetChannelAsync(Context.Interaction.ChannelId ?? 0)).ModifyMessageAsync(messageId, m => m.Content = $"{Context.Guild.GetRole(gs.Role.ClassRequest).Mention} {_emoji.GetEmoji("util_loading")}");
			await DeferAsync();

			IGuildUser teacher;
			try
			{
				teacher = await Context.Guild.GetUserAsync(cClass.Teacher);
				await teacher.AddRoleAsync(gs.Role.Teacher);
			}
			catch (Exception e)
			{
				await FollowupAsync(embed: EmbedService.Error("Nem sikerült eldogadni az osztályt", $"Szólj egy adminisztrátornak, hogy nem találok `Teacher` role-t, vagy nem tudom ráadni a felhasználóra!\n```{e.Message}```"), ephemeral: true);
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
					new Overwrite(_client.CurrentUser.Id, PermissionTarget.User, new OverwritePermissions(manageChannel: PermValue.Allow, viewChannel: PermValue.Allow)),
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
				await FollowupAsync(embed: EmbedService.Error("Nem Sikerült elfogadni az osztályt", $"Szólj egy adminisztáronak, hogy nem találok `Teaching` kategóriát, vagy nincs elég jogom létrehozni a csatornákat!\n```{e.Message}```"), ephemeral: true);
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
				await FollowupAsync(embed: EmbedService.Error("Nem sikerült elfogadni az osztályt", $"Szólj egy adminisztrátornak, hogy nem találok `Classes` csatornát, nem tudok oda üzenetet írni, vagy nincs `NewClass` role!\n```{e.Message}```"), ephemeral: true);
				await Revert(messageId, teacher, text, voice);
				return;
			}

			ComponentBuilder leaveButton = new ComponentBuilder()
				.WithButton("Kilépés", $"classbutton-leave:{cClass.ID}", ButtonStyle.Danger);
			await (await text.SendMessageAsync(embed: EmbedService.Info(cClass.Theme, "Az osztályból való kilépéshez kattints a `Kilépés` gombra"), components: leaveButton.Build())).PinAsync();
			await _mongo.Classes.ReplaceOneAsync(c => c.ID == int.Parse(classId), cClass);
			try
			{
				await Context.Channel.DeleteMessageAsync(messageId);
			}
			catch (Exception e)
			{
				await FollowupAsync(embed: EmbedService.Error("Nem sikerült elfogadni az osztályt", $"Valószínűleg valaki nálad hamarabb kezelte a kérést.\n```{e.Message}```"), ephemeral: true);
				return;
			}

			try
			{
				await _client.GetUser(cClass.Teacher).SendMessageAsync(embed: EmbedService.Success($"Egy osztályod létrehozása el lett fogadva", $"Osztály: **{cClass.Theme}**\nSzerver: **{Context.Guild}**"));
			}
			catch (Exception) { /*ignore*/ }
		}
		private async Task Revert(ulong messageId, IGuildUser teacher = null, ITextChannel text = null, IVoiceChannel voice = null)
		{
			GuildSettings gs = _mongo.GetGuildSettings(Context.Guild.Id);
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
			TanClass cClass = _mongo.Classes.AsQueryable().First(c => c.ID == int.Parse(classId));
			await _mongo.Classes.DeleteOneAsync(c => c.Teacher == cClass.Teacher && c.Theme == cClass.Theme);
			await Context.Channel.DeleteMessageAsync(ulong.Parse(messageId));
			try
			{
				await _client.GetUser(cClass.Teacher).SendMessageAsync(embed: EmbedService.Error($"Egy osztályod létrehozása el lett utasítva", $"Osztály: **{cClass.Theme}**\nSzerver: **{Context.Guild}**\nIndoklás:\n```{modal.Reason}```"));
			}
			catch (Exception) { /*ignore*/ }
		}
	}
}
