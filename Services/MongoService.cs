﻿using Discord.WebSocket;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace GroundedBot.Services
{
    public class MongoService
    {
        private MongoClient dbClient;

        private IMongoDatabase db => this.dbClient.GetDatabase("groundedbot");
        public IMongoCollection<GuildSettings> Guilds => this.db.GetCollection<GuildSettings>("guilds");
        public IMongoCollection<TanClass> Classes => this.db.GetCollection<TanClass>("classes");

        public GuildSettings GetGuildSettings(ulong guildId) => this.Guilds.AsQueryable().First(s => s.Guild == guildId);

        public async Task InsertAllGuilds(IReadOnlyCollection<SocketGuild> guilds)
        {
            List<ulong> knownGuilds = this.Guilds.AsQueryable().Select(s => s.Guild).ToList();
            foreach (ulong g in guilds.Select(g => g.Id))
                if (!knownGuilds.Contains(g))
                    await this.Guilds.InsertOneAsync(new GuildSettings(g));
        }

        public MongoService(string uri)
        {
            this.dbClient = new(uri);
        }
        public MongoService() { }
    }

    public class GuildSettings
    {
        [BsonId]
        public ulong Guild { get; set; }
        public GuildCategories Category { get; set; }
        public GuildChannels Channel { get; set; }
        public GuildRoles Role { get; set; }

        public GuildSettings(ulong id)
        {
            this.Guild = id;
            this.Category = new();
            this.Channel = new();
            this.Role = new();
        }
    }
    public class GuildCategories
    {
        public ulong Teaching { get; set; }
    }
    public class GuildChannels
    {
        public ulong Classes { get; set; }
        public ulong ClassRequest { get; set; }
        public ulong PingRequest { get; set; }
    }
    public class GuildRoles
    {
        public ulong ClassRequest { get; set; }
        public ulong Moderator { get; set; }
        public ulong NewClass { get; set; }
        public ulong PingRequest { get; set; }
        public ulong Student { get; set; }
        public ulong Teacher { get; set; }
        public HashSet<ulong> PingRequests { get; set; }

        public GuildRoles()
        {
            this.PingRequests = new();
        }
    }

    public class TanClass
    {
        [BsonId]
        public int ID { get; set; }
        public ulong Teacher { get; set; }
        public string Theme { get; set; }
        public string Description { get; set; }
        public string CourseURL { get; set; }
        public bool Approved { get; set; }
        public ulong TextChannel { get; set; }
        public ulong VoiceChannel { get; set; }
        public List<ulong> Students { get; set; }
    }
}