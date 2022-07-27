namespace GroundedBot
{
    public class Program
    {
        public static void Main()
        {
            GroundedBot bot = new GroundedBot(new Config("config.xml"));
            bot.MainAsync().GetAwaiter().GetResult();
        }
        protected Program() { }
    }
}
