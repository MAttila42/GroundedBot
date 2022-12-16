using Discord.Interactions;

namespace GroundedBot.Commands
{
	public class Ping : InteractionModuleBase
	{
		[SlashCommand("ping", "Kiszámolja a bot pingjét")]
		public async Task Run()
		{
			await RespondAsync("Pinging...");
			var message = await Context.Interaction.GetOriginalResponseAsync();
			TimeSpan time = DateTime.Now - message.Timestamp;
			await message.ModifyAsync(m => m.Content = $"`{time.Milliseconds} ms`");
		}
	}
}
