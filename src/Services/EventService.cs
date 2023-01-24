using Discord.Commands;
using Discord.WebSocket;

namespace GroundedBot.Services;

public class EventService
{
	public static async Task ExecuteMessageEventsAsync(
		SocketCommandContext ctx,
		IServiceProvider services) =>
			await ExecuteEventsAsync(
				services,
				messageContext: ctx);

	public static async Task ExecuteEventsAsync(
		IServiceProvider services,
		SocketCommandContext messageContext = null)
	{
		List<Event> events = new List<Event>();
		var types = typeof(Event).Assembly
			.GetTypes()
			.Where(t => t.IsSubclassOf(typeof(Event)) && !t.IsAbstract);

		foreach (Type t in types)
		{
			dynamic e = Convert.ChangeType(Activator.CreateInstance(t), t);
			foreach (var p in t.GetProperties())
			{
				if (p.Name == "Context")
					continue;
				p.SetValue(e, services.GetService(p.GetValue(e).GetType()));
			}
			if (t.IsSubclassOf(typeof(MessageEvent)))
				e.Context = messageContext;
			events.Add(e);
		}
		foreach (dynamic e in events)
			await e.Execute();
	}
}

public abstract class Event
{
	public DiscordSocketClient Client { get; set; } = new();
	public EmoteService Emote { get; set; } = new();
	public MongoService Mongo { get; set; } = new();
}

public abstract class MessageEvent : Event
{
	public SocketCommandContext Context { get; set; }
}
