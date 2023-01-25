namespace GroundedBot;

public class Program
{
	public static void Main() =>
		new GroundedBot("config.json")
			.MainAsync().GetAwaiter().GetResult();

	private Program() { }
}
