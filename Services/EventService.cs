using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;

namespace GroundedBot.Services
{
    public class EventService
    {
        public async Task ExecuteMessageEventsAsync(SocketCommandContext ctx, IServiceProvider services) => await ExecuteEventsAsync(services, messageContext: ctx);
        public async Task ExecuteEventsAsync(IServiceProvider services, SocketCommandContext messageContext = null)
        {
            List<Event> events = new List<Event>();
            foreach (Type t in typeof(Event).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Event)) && !t.IsAbstract))
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
        virtual public Config _config { get; set; } = new();
        virtual public DiscordSocketClient _client { get; set; } = new();
        virtual public EmojiService _emoji { get; set; } = new();
        virtual public MongoService _mongo { get; set; } = new();
    }
    public abstract class MessageEvent : Event
    {
        virtual public SocketCommandContext Context { get; set; }
    }
}
