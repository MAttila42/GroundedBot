using Discord;
using Discord.Interactions;
using GroundedBot.Services;

namespace GroundedBot.Commands
{
    public class Setup : InteractionModuleBase
    {
        public enum Message
        {
            CreateClass
        }

        [SlashCommand("setup", "[ADMIN] Botkezelő üzenetek küldése")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Run(Message message)
        {
            switch (message)
            {
                case Message.CreateClass:
                    await RespondAsync(embed: EmbedService.Info("Üzenet küldése..."), ephemeral: true);
                    EmbedBuilder embed = new EmbedBuilder()
                        .WithAuthor("Saját osztályod elkészítése")
                        .WithDescription("Ha szeretnél tanítani másokat, akkor a lenti `Létrehozás` gombra kattintva tudsz csinálni magadnak egy osztályt. Ehhez meg kell majd adnod, hogy mit tanítanál; egy rövid ismertetőt magadról és a tananyagodról; illetve egy linket, ahol hosszan részletezed mindazt, amit megtanítanál, személyes órák időpontjait stb. (Ez utóbbi nem kötelező, de hasznos lehet.)\n\n" +
                        "Az érdeklődő diákok rendelkezésére áll majd a kidolgozott tananyagod, amiből okosodhatnak egyedül is, illetve hozzáférés a személyes óráidhoz, ahol szóban, kérdésekre egyből választ kapva tanulhatnak.\n\n" +
                        "A felépítést, tanítási módszereket, használt eszközöket stb. rád bízzuk. Az osztályod létrehozása és annak elfogadása után kapsz saját Discord szobát és hangcsatornát.")
                        .WithFooter(Context.Guild.Name)
                        .WithColor(new Color(0x5864f2));
                    ComponentBuilder components = new ComponentBuilder()
                        .WithButton("Létrehozás", "classbutton-create", style: ButtonStyle.Success);
                    try { await Context.Channel.SendMessageAsync(embed: embed.Build(), components: components.Build()); }
                    catch (Exception e) { await FollowupAsync(embed: EmbedService.Error("Nem sikerült elküldeni az üzenetet", $"```{e.Message}```"), ephemeral: true); }
                    break;
            }
        }
    }
}
