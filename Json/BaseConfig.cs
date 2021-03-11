using System.IO;
using System.Text.Json;

namespace GroundedBot.Json
{
    public class Role
    {
        /// <summary>
        /// Azoknak a role-oknak az ID-jük, amikkel rendelkezőek teljes hozzáférész kapnak a bothoz.
        /// </summary>
        public ulong[] Admin { get; set; }
        /// <summary>
        /// Moderátor role-ok ID-jük.
        /// </summary>
        public ulong[] Mod { get; set; }
        /// <summary>
        /// Ptan+ Budget role-ok ID-jük
        /// </summary>
        public ulong[] PtanB { get; set; }
        /// <summary>
        /// Ptan+ Standard role-ok ID-jük
        /// </summary>
        public ulong[] PtanS { get; set; }
        /// <summary>
        /// Ptan+ Pro role-ok ID-jük
        /// </summary>
        public ulong[] PtanP { get; set; }
    }
    public class Channel
    {
        /// <summary>
        /// Azoknak a szobáknak az ID-rük, ahova a bot elküldi minden logját, amit a konzolablakba is.
        /// </summary>
        public ulong[] BotTerminal { get; set; }
        /// <summary>
        /// Azoknak a szobáknak az ID-jük, ahova a bot óránként menti az adatbázisokat.
        /// </summary>
        public ulong[] Backups { get; set; }
        /// <summary>
        /// Azoknak a szobáknak az ID-jük, ahova korlátozva vannak egyes funkciók működései. (Nincs XP adás, egyes parancsok csak itt működnek)
        /// </summary>
        public ulong[] BotChannel { get; set; }
        /// <summary>
        /// Azoknak a szobáknak az ID-jük, ahova mennek a Ping Request értesítések.
        /// </summary>
        public ulong PingRequests { get; set; }
        /// <summary>
        /// Azoknak a szobáknak az ID-jük, ahova ahova mennek az Answer Request értesítések.
        /// </summary>
        public ulong AnswerRequests { get; set; }
        /// <summary>
        /// Azoknak a szobáknak az ID-jük, ahol gratulál a Bot a szintlépésért.
        /// </summary>
        public ulong LevelUp { get; set; }
    }
    /// <summary>
    /// A Bot működéséhez elengedhetetlen configok.
    /// <para>Token, Prefix, Roles, Channels</para>
    /// </summary>
    class BaseConfig
    {
        /// <summary>
        /// Bot tokenje. Enélkül el se indul a bot.
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// Egykarakteres prefix a parancsokhoz.
        /// </summary>
        public char Prefix { get; set; }
        /// <summary>
        /// A Bot saját ID-je.
        /// </summary>
        public ulong ServerID { get; set; }
        /// <summary>
        /// Szükséges role-ok listája.
        /// </summary>
        public Role Roles { get; set; }
        /// <summary>
        /// Szükséges szobák listája.
        /// </summary>
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
