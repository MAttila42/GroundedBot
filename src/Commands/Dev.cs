using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GroundedBot.Helpers;
using GroundedBot.Services;
using MongoDB.Driver;

namespace GroundedBot.Commands;

[Group("dev", "[DEV] Fejlesztő parancsok")]
[RequireOwner]
public class Dev : InteractionModuleBase
{
	public DiscordSocketClient Client { get; set; }
	public MongoService Mongo { get; set; }

	[SlashCommand("guildcount", "[DEV] Szerverek száma")]
	public async Task ServerCount() =>
		await RespondAsync(embed: Embeds.Info($"A bot {Client.Guilds.Count} db szerveren van bent."));

	[SlashCommand("shutdown", "[DEV] Bot leállítása")]
	public async Task Shutdown()
	{
		await RespondAsync(embed: Embeds.Info("Kikapcsolás..."));
		Environment.Exit(0);
	}

	[SlashCommand("restoreclasses", "[DEV] Megkeresi az osztályokat és eltárolja az adatbázisban")]
	public async Task RestoreClasses()
	{
		await DeferAsync();

		var guilds = Client.Guilds;
		foreach (var guild in guilds)
		{
			var gs = Mongo.GuildSettings.AsQueryable().FirstOrDefault(s => s.Guild == guild.Id);
			if (gs.Channel.Classes == 0 || gs.Category.Teaching == 0)
				continue;

			var channel = guild.GetTextChannel(gs.Channel.Classes);
			var messages = await channel.GetMessagesAsync().FlattenAsync();

			foreach (var message in messages)
			{
				var embed = message.Embeds.FirstOrDefault();
				var components = message.Components;
				if (components.Count == 0)
					continue;
				string customId = ((ActionRowComponent)components.First()).Components.First().CustomId;
				TanClass classData;
				if (customId.StartsWith("classbutton-join"))
				{
					classData = new()
					{
						ID = int.Parse(new Regex(@"\d+").Match(customId).Value),
						Guild = guild.Id,
						Teacher = ulong.Parse(new Regex(@"\d+").Match(embed.Fields[0].Value).Value),
						Theme = embed.Author.Value.Name,
						Description = embed.Fields[1].Value,
						CourseURL = embed.Fields[2].Value,
						Approved = true,
					};

					var temp = await Client.GetGuild(712287958274801695).CreateTextChannelAsync(classData.Theme);
					classData.TextChannel = guild
						.GetCategoryChannel(gs.Category.Teaching)
						.Channels
						.FirstOrDefault(c =>
							c.Name == temp.Name
						).Id;
					await temp.DeleteAsync();

					classData.VoiceChannel = guild.GetCategoryChannel(gs.Category.Teaching).Channels.FirstOrDefault(c => c.Name == classData.Theme).Id;

					classData.Students = guild
						.GetTextChannel(classData.TextChannel)
						.PermissionOverwrites
						.Where(o => o.TargetType == PermissionTarget.User)
						.Where(o => o.Permissions.ViewChannel == PermValue.Allow)
						.Select(o => o.TargetId)
						.Where(id =>
							id != classData.Teacher &&
							id != Client.CurrentUser.Id
						).ToList();
				}
				else if (customId.StartsWith("classbutton-approve"))
				{
					classData = new()
					{
						ID = int.Parse(new Regex(@"\d+").Match(customId).Value),
						Guild = guild.Id,
						Teacher = ulong.Parse(new Regex(@"\d+").Match(embed.Fields[0].Value).Value),
						Theme = embed.Fields[1].Value,
						Description = embed.Fields[2].Value,
						CourseURL = embed.Fields[3].Value,
						Approved = false,
						TextChannel = 0,
						VoiceChannel = 0,
						Students = new()
					};
				}
				else
					continue;
				Mongo.Classes.InsertOne(classData);
			}
		}
		await FollowupAsync(embed: Embeds.Success("Az osztályok sikeresen hozzá lettek adva az adatbázishoz."));
	}

	[SlashCommand("test", "[DEV] Tesztelés")]
	public async Task Test()
	{
		await RespondAsync(Context.User.Mention, allowedMentions: AllowedMentions.None);
		if (await Mongo.Classes.CountDocumentsAsync(x => true) > 0)
			await FollowupAsync(string.Join(", ", Mongo.Classes.AsQueryable().Select(c => c.Theme).ToList()));
	}
}
