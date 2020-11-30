using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;

namespace GroundedBot.Json
{
    /// <summary>
    /// Ping requesteknek létrehozott adatbázis.
    /// <para>ID, Ping, ChannelID</para>
    /// </summary>
    class PingRequests
    {
        public ulong ID { get; set; }
        public ulong RoleID { get; set; }
        public ulong ChannelID { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Ping requestek lekérése a Json-ből.
        /// </summary>
        /// <returns></returns>
        public static List<PingRequests> PullData()
        {
            try { return JsonSerializer.Deserialize<List<PingRequests>>(File.ReadAllText("PingRequests.json")); }
            catch (Exception) { File.WriteAllText("PingRequests.json", "[]"); }
            return JsonSerializer.Deserialize<List<PingRequests>>(File.ReadAllText("PingRequests.json"));
        }
        /// <summary>
        /// Ping requestek feltöltése a Json-be.
        /// </summary>
        /// <param name="list"></param>
        public static void PushData(List<PingRequests> list)
        {
            File.WriteAllText("PingRequests.json", JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
        }

        /// <summary>
        /// Ez a sztupid Json deserialize miatt kell. Használd a másik konstruktort!
        /// </summary>
        public PingRequests() { }

        /// <summary>
        /// Alap konstruktor az összes, kötelező adattal.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ping"></param>
        public PingRequests(ulong id, ulong roleId, ulong channelId, string description)
        {
            ID = id;
            RoleID = roleId;
            ChannelID = channelId;
            Description = description;
        }
    }
}
