using System;
using System.Collections.Generic;
using System.Linq;
using GroundedBot.Json;

namespace GroundedBot.Events
{
    class RemoveWhoLeft
    {
        public static async void DoEvent(ulong id)
        {
            var members = Members.PullData();
            members.RemoveAt(members.IndexOf(members.Find(x => x.ID == id)));
            Members.PushData(members);
        }
    }
}
