using Discord;

namespace GroundedBot.Services
{
    public static class EmbedService
    {
        public readonly static string CloseIcon = "https://media.discordapp.net/attachments/782305154342322226/984550039357628456/noun_Close_1984788.png"; // Close by Bismillah from NounProject.com
        public readonly static string InfoIcon = "https://media.discordapp.net/attachments/782305154342322226/984550064112427209/noun-info-1228105.png"; // Info by David Khai from NounProject.com
        public readonly static string CheckmarkIcon = "https://media.discordapp.net/attachments/782305154342322226/987032768477274112/noun-checkmark-2193570.png"; // Checkmark by Asmuh from NounProject.com
        public readonly static string PlusIcon = "https://media.discordapp.net/attachments/782305154342322226/987315169094926377/noun-plus-1809808.png"; // Plus by sumhi_icon from NounProject.com
        public readonly static string BookIcon = "https://media.discordapp.net/attachments/782305154342322226/993596041318367444/noun-book-3908258.png?width=670&height=670"; // Book by HideMaru from NounProject.com

        public readonly static Color Blurple = new(0x5864f2);
        public readonly static Color Green = new(0x00e200);
        public readonly static Color Orange = new(0xfaa81a);
        public readonly static Color Red = new(0xff1821);

        public static Embed Info(in string msg) => Info(msg, null);
        public static Embed Info(in string title, in string body) => new EmbedBuilder()
            .WithAuthor(title, InfoIcon)
            .WithDescription(body)
            .WithColor(Blurple)
            .Build();

        public static Embed Success(in string msg) => Success(msg, null);
        public static Embed Success(in string title, in string body) => new EmbedBuilder()
            .WithAuthor(title, CheckmarkIcon)
            .WithDescription(body)
            .WithColor(Green)
            .Build();

        public static Embed Error(in string msg) => Error(msg, null);
        public static Embed Error(in string title, in string body) => new EmbedBuilder()
            .WithAuthor(title, CloseIcon)
            .WithDescription(body)
            .WithColor(Red)
            .Build();
    }
}
