using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GroundedBot.Services;

namespace GroundedBot.Buttons
{
    public class PrButtons : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
    {
        public MongoService _mongo { get; set; }
        public EmojiService _emoji { get; set; }

        [ComponentInteraction("prbutton-approve:*,*,*")]
        public async Task Approve(string channelId, string pingMsgId, string roleId) => await Review(ulong.Parse(channelId), ulong.Parse(pingMsgId), ulong.Parse(roleId));

        [ComponentInteraction("prbutton-reject:*,*")]
        public async Task Reject(string channelId, string pingMsgId) => await Review(ulong.Parse(channelId), ulong.Parse(pingMsgId));

        private async Task Review(ulong channelId, ulong pingMsgId, ulong roleId = 0)
        {
            if (!((IGuildUser)Context.User).RoleIds.Contains(_mongo.GetGuildSettings(Context.Guild.Id).Role.Moderator))
            {
                await DeferAsync();
                return;
            }
            try
            {
                await Context.Interaction.Message.ModifyAsync(m => m.Content = $"{Context.Guild.GetRole(_mongo.GetGuildSettings(Context.Guild.Id).Role.PingRequest).Mention} {_emoji.GetEmoji("util_loading")}");
                await DeferAsync();
                ISocketMessageChannel channel = Context.Guild.GetChannel(channelId) as ISocketMessageChannel;
                if (roleId != 0)
                    await channel.SendMessageAsync(Context.Guild.GetRole(roleId).Mention);
                await (await channel.GetMessageAsync(pingMsgId)).DeleteAsync();
                await Context.Interaction.Message.DeleteAsync();
            }
            catch (Exception) { }
        }
    }
}
