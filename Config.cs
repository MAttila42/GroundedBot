using System.Xml.Linq;

namespace GroundedBot
{
    public class Config
    {
        public string Token { get; private set; }
        public ulong DebugGuild { get; private set; }
        public List<ulong> EmojiGuilds { get; private set; }

        public Config(string path)
        {
            using (StreamReader stream = File.OpenText(path))
            {
                XDocument config = XDocument.Load(stream);
                XElement configElement = config.Element("config");
                this.Token = configElement.Element("token").Value;
                this.DebugGuild = ulong.Parse(configElement.Element("debugguild").Value);
                this.EmojiGuilds = new List<ulong>();

                configElement.Element("emojiguilds").Elements("guild").ToList().ForEach(s => this.EmojiGuilds.Add(ulong.Parse(s.Value)));
            }
        }
        public Config() { }
    }
}
