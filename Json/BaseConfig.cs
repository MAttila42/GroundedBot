using System.IO;
using System.Text.Json;

namespace GroundedBot.Json
{
    public class Role
    {
        public ulong[] Admin { get; set; }
        public ulong[] Mod { get; set; }
        public ulong[] PtanP { get; set; }
    }
    public class Channel
    {
        public ulong[] BotChannel { get; set; }
        public ulong[] BotTerminal { get; set; }
    }
    /// <summary>
    /// A Bot működéséhez elengedhetetlen configok.
    /// <para>Token, Prefix, Roles, Channels</para>
    /// </summary>
    public class BaseConfig
    {
        public string Token { get; set; }
        public char Prefix { get; set; }
        public Role Roles { get; set; }
        public Channel Channels { get; set; }

        /// <summary>
        /// BaseConfig adatainak a lekérése.
        /// </summary>
        /// <returns></returns>
        public static BaseConfig GetConfig()
        {
            return JsonSerializer.Deserialize<BaseConfig>(File.ReadAllText("BaseConfig.json"));
        }
    }
}
