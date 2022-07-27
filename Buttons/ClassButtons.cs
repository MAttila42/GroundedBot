using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MongoDB.Driver;
using GroundedBot.Modals;
using GroundedBot.Services;

namespace GroundedBot.Buttons
{
    public class ClassButtons : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
    {
        public DiscordSocketClient _client { get; set; }
        public MongoService _mongo { get; set; }

        [ComponentInteraction("classbutton-create")]
        public async Task Create() => await RespondWithModalAsync<ClassModal>("classmodal-create");

        [ComponentInteraction("classbutton-approve:*")]
        public async Task Approve(string classId)
        {
            if (((IGuildUser)Context.User).RoleIds.Contains(_mongo.GetGuildSettings(Context.Guild.Id).Role.Moderator))
            {
                await DeferAsync();
                return;
            }
            TanClass cClass = _mongo.Classes.AsQueryable().First(c => c.ID == int.Parse(classId));

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
            if (((IGuildUser)Context.User).RoleIds.Contains(_mongo.GetGuildSettings(Context.Guild.Id).Role.Moderator))
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
            GuildSettings gs = _mongo.GetGuildSettings(Context.Guild.Id);
            TanClass cClass = _mongo.Classes.AsQueryable().First(c => c.ID == int.Parse(classId));
            if (cClass.Students.Contains(Context.User.Id) || cClass.Teacher == Context.User.Id)
                return;

            try
            {
                await ((IGuildUser)Context.User).AddRoleAsync(gs.Role.Student);
            }
            catch (Exception e)
            {
                await FollowupAsync(embed: EmbedService.Error("Nem sikerült csatlakozni az osztályhoz", $"Szólj egy adminisztrátornak, hogy nem találok `Student` role-t, vagy nem tudom ráadni a felhasználóra!\n```{e.Message}```"), ephemeral: true);
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
                await FollowupAsync(embed: EmbedService.Error("Nem sikerült csatlakozni az osztályhoz", $"Szólj egy adminisztáronak, hogy nincs elég jogom módosítani a csatornákat!\n```{e.Message}```"), ephemeral: true);
                await ((IGuildUser)Context.User).RemoveRoleAsync(gs.Role.Student);
                return;
            }

            await FollowupAsync(embed: EmbedService.Success("Csatlakoztál az osztályhoz", $"Informálódni, tanulni és beszélgetni a(z) <#{cClass.TextChannel}> szobában, illetve a(z) <#{cClass.VoiceChannel}> hangcsatornában tudsz. Jó tanulást kíván a **{Context.Guild.Name}**!"), ephemeral: true);
            cClass.Students.Add(Context.User.Id);
            await _mongo.Classes.ReplaceOneAsync(c => c.ID == int.Parse(classId), cClass);
        }

        [ComponentInteraction("classbutton-leave:*")]
        public async Task Leave(string classId)
        {
            await DeferAsync();
            GuildSettings gs = _mongo.GetGuildSettings(Context.Guild.Id);
            TanClass cClass = _mongo.Classes.AsQueryable().First(c => c.ID == int.Parse(classId));
            if (!cClass.Students.Contains(Context.User.Id))
                return;

            try
            {
                await ((IGuildUser)Context.User).AddRoleAsync(gs.Role.Student);
                await ((IGuildUser)Context.User).RemoveRoleAsync(gs.Role.Student);
            }
            catch (Exception e)
            {
                await FollowupAsync(embed: EmbedService.Error("Nem sikerült kiléptetni az osztályból", $"Szólj egy adminisztrátornak, hogy nem találok `Student` role-t, vagy tudom elvenni!\n```{e.Message}```"));
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
                await FollowupAsync(embed: EmbedService.Error("Nem sikerült kiléptetni az osztályból", $"Szólj egy adminisztáronak, hogy nincs elég jogom módosítani a csatornákat!\n```{e.Message}```"), ephemeral: true);
                await ((IGuildUser)Context.User).AddRoleAsync(gs.Role.Student);
                return;
            }

            cClass.Students.Remove(Context.User.Id);
            await _mongo.Classes.ReplaceOneAsync(c => c.ID == int.Parse(classId), cClass);
        }

        [ComponentInteraction("classbutton-delete:*")]
        public async Task Delete(string classIdStr)
        {
            await DeferAsync();
            int classId = int.Parse(classIdStr);
            GuildSettings gs = _mongo.GetGuildSettings(Context.Guild.Id);
            TanClass cClass = _mongo.Classes.AsQueryable().First(c => c.ID == classId);
            IReadOnlyCollection<ulong> roles = ((IGuildUser)Context.User).RoleIds;
            if (Context.User.Id != cClass.Teacher && !roles.Contains(gs.Role.Moderator))
                return;

            try
            {
                foreach (ulong id in cClass.Students)
                    if (_mongo.Classes.AsQueryable().Count(c => c.Students.Contains(id)) == 1)
                        await Context.Guild.GetUser(id).RemoveRoleAsync(gs.Role.Student);
            }
            catch (Exception e)
            {
                await FollowupAsync(embed: EmbedService.Error("Nem sikerült törölni az osztályt", $"Szólj egy adminisztrátornak, hogy nem találok `Student` role-t, vagy nincs jogom elvenni!\n```{e.Message}```"), ephemeral: true);
                return;
            }
            try
            {
                if (_mongo.Classes.AsQueryable().Count(c => c.Teacher == cClass.Teacher) == 1)
                    await Context.Guild.GetUser(cClass.Teacher).RemoveRoleAsync(gs.Role.Teacher);
            }
            catch (Exception e)
            {
                await FollowupAsync(embed: EmbedService.Error("Nem sikerült törölni az osztályt", $"Szólj egy adminisztrátornak, hogy nem találok `Teacher` role-t, vagy nincs jogom elvenni!\n```{e.Message}```"), ephemeral: true);
                await RevertStudentRoles(cClass.Students);
                return;
            }
            try
            {
                await Context.Guild.GetChannel(cClass.TextChannel).DeleteAsync();
                await Context.Guild.GetChannel(cClass.VoiceChannel).DeleteAsync();
            }
            catch (Exception e)
            {
                await FollowupAsync(embed: EmbedService.Error("Nem sikerült törölni az osztályt", $"Szólj egy adminisztrátornak, hogy nincs jogom törölni a csatornákat!\n```{e.Message}```"), ephemeral: true);
                await RevertStudentRoles(cClass.Students);
                await Context.Guild.GetUser(cClass.Teacher).AddRoleAsync(gs.Role.Teacher);
                return;
            }

            await Context.Interaction.Message.DeleteAsync();
            await _mongo.Classes.DeleteOneAsync(c => c.ID == classId);

            if (Context.User.Id != cClass.Teacher)
                await Context.Guild.GetUser(cClass.Teacher).SendMessageAsync(embed: EmbedService.Error("Egy osztályod törölve lett", $"Osztály: **{cClass.Theme}**\nSzerver: **{Context.Guild.Name}**"));
        }
        private async Task RevertStudentRoles(List<ulong> students)
        {
            GuildSettings gs = _mongo.GetGuildSettings(Context.Guild.Id);
            foreach (ulong id in students)
                await Context.Guild.GetUser(id).AddRoleAsync(gs.Role.Student);
        }
    }
}
