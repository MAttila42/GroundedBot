using Discord;

namespace GroundedBot.Helpers;

public static class Embeds
{
	public static Embed Info(in string msg) =>
		Info(msg, null);
	public static Embed Info(in string title, in string body) =>
		new EmbedBuilder()
			.WithAuthor(title, Icons.Info)
			.WithDescription(body)
			.WithColor(Colors.Blurple)
			.Build();

	public static Embed Success(in string msg) =>
		Success(msg, null);
	public static Embed Success(in string title, in string body) =>
		new EmbedBuilder()
			.WithAuthor(title, Icons.Checkmark)
			.WithDescription(body)
			.WithColor(Colors.Green)
			.Build();

	public static Embed Error(in string msg) =>
		Error(msg, null);
	public static Embed Error(in string title, in string body) =>
		new EmbedBuilder()
			.WithAuthor(title, Icons.Close)
			.WithDescription(body)
			.WithColor(Colors.Red)
			.Build();
}
