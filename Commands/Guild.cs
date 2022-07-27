using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MongoDB.Driver;
using GroundedBot.Services;

namespace GroundedBot.Commands
{
    [Group("guild", "[ADMIN] Szerver beállítások")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class Guild : InteractionModuleBase
    {
        public enum RoleList
        {
            PingRequests
        }
        [Group("add", "[ADMIN]")]
        public class Add : InteractionModuleBase
        {
            public MongoService _mongo { get; set; }

            [SlashCommand("role", "[ADMIN] Role hozzáadása listához")]
            public async Task Role(IRole role, RoleList list)
            {
                GuildSettings gs = _mongo.GetGuildSettings(Context.Guild.Id);
                PropertyInfo p = gs.Role.GetType().GetProperty(list.ToString());
                HashSet<ulong> l = p.GetValue(gs.Role) as HashSet<ulong>;
                l.Add(role.Id);
                p.SetValue(gs.Role, l);
                await _mongo.Guilds.ReplaceOneAsync(g => g.Guild == gs.Guild, gs);
                await RespondAsync(embed: EmbedService.Success("Sikeres beállítás"), ephemeral: true);
            }
        }
        [Group("remove", "[ADMIN]")]
        public class Remove : InteractionModuleBase
        {
            public MongoService _mongo { get; set; }

            [SlashCommand("role", "[ADMIN] Role elvétele listából")]
            public async Task Role(IRole role, RoleList list)
            {
                GuildSettings gs = _mongo.GetGuildSettings(Context.Guild.Id);
                PropertyInfo p = gs.Role.GetType().GetProperty(list.ToString());
                HashSet<ulong> l = p.GetValue(gs.Role) as HashSet<ulong>;
                l.Remove(role.Id);
                p.SetValue(gs.Role, l);
                await _mongo.Guilds.ReplaceOneAsync(g => g.Guild == gs.Guild, gs);
                await RespondAsync(embed: EmbedService.Success("Sikeres beállítás"), ephemeral: true);
            }
        }

        public enum CategoryTask
        {
            Teaching
        }
        public enum ChannelTask
        {
            Classes,
            ClassRequest,
            PingRequest
        }
        public enum RoleTask
        {
            ClassRequest,
            Moderator,
            NewClass,
            PingRequest,
            Student,
            Teacher
        }
        [Group("set", "[ADMIN]")]
        public class Set : InteractionModuleBase
        {
            public MongoService _mongo { get; set; }

            [SlashCommand("category", "[ADMIN] Aktuális kategória konfigurálása")]
            public async Task Category(CategoryTask task)
            {
                try { await SetPropertyValue(task.ToString(), ((SocketGuild)Context.Guild).CategoryChannels.ToList().Find(c => c.Channels.Select(ch => ch.Id).Contains(Context.Channel.Id)).Id); }
                catch (Exception e) { await RespondAsync(embed: EmbedService.Error("Sikertelen beállítás", $"Lehet, hogy a szoba nincs egy kategóriában se.\n```{e.Message}```"), ephemeral: true); }
            }

            [SlashCommand("channel", "[ADMIN] Aktuális csatorna konfigurálása")]
            public async Task Channel(ChannelTask task) => await SetPropertyValue(task.ToString(), Context.Channel.Id);

            [SlashCommand("role", "[ADMIN] Role konfigurálása")]
            public async Task Role(IRole role, RoleTask task) => await SetPropertyValue(task.ToString(), role.Id);

            private async Task SetPropertyValue(string name, ulong value)
            {
                GuildSettings gs = _mongo.GetGuildSettings(Context.Guild.Id);
                string g = ((SocketSlashCommand)Context.Interaction).Data.Options.First().Options.First().Name;
                g = char.ToUpper(g[0]) + g.Substring(1);
                object s = gs.GetType().GetProperty(g).GetValue(gs);
                s.GetType().GetProperty(name).SetValue(s, value);
                await _mongo.Guilds.ReplaceOneAsync(g => g.Guild == gs.Guild, gs);
                await RespondAsync(embed: EmbedService.Success("Sikeres beállítás"), ephemeral: true);
            }
        }
    }
}
