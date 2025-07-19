using Discord;
using Discord.Webhook;

namespace MinecraftBot;

static class DiscordNotifire
{
    public static DiscordWebhookClient Webhook { get; private set; } = null!;

    public static void ChangeWebhookUrl(string url)
    {
        Webhook?.Dispose();
        Webhook = new DiscordWebhookClient(url);
    }

    public static async ValueTask<ulong> Notice(
        string message,
        string? username = null,
        MessageFlags flags = MessageFlags.None)
    {
        if (message.StartsWith("@silent")) {
            message = message[7..].TrimStart();
            flags |= MessageFlags.SuppressNotification;
        }

        try {
            return await Webhook.SendMessageAsync(
                message,
                username: username,
                flags: flags
            );
        } catch (Exception e) {
            Program.ConsoleWriteLine("ERROR!\n" + e.ToString() + "\n");
        }

        return 0;
    }

    public static async Task<bool> CheckWebhookUrl(string? url)
    {
        if (
            string.IsNullOrWhiteSpace(url) ||
            !Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) ||
            uriResult.Scheme != Uri.UriSchemeHttps
        )
            return false;

        using var response = await Program.HttpClient.GetAsync(uriResult);
        return response.IsSuccessStatusCode;
    }

    private static Embed CreateEmbed()
    {
        var iconUrl = "https://pbs.twimg.com/profile_images/1014513302775271424/IvvOe1Il_400x400.jpg";

        return new EmbedBuilder()
            .WithTitle("Embed title")
            .WithDescription("Embed description")
            .WithColor(Discord.Color.Red)
            .WithUrl("https://twitter.com/mujurin_2525")
            .WithTimestamp(DateTime.Now)
            .WithFooter("Footer Text", iconUrl)
            .WithImageUrl(iconUrl)
            // //valid for thumb and video
            // Provider = new EmbedProvider()
            // {
            //     Name = "Provider Name",
            //     Url = new Uri("Provider Url")
            // },
            .WithAuthor("Author Name", iconUrl, url: "https://twitter.com/mujurin_2525")
            .WithFields(new EmbedFieldBuilder[] {
                new EmbedFieldBuilder()
                    .WithName("Field Name")
                    .WithValue("Field Value")
                    .WithIsInline(true),
                new EmbedFieldBuilder()
                    .WithName("Field Name 2")
                    .WithValue("Field Value 2")
                    .WithIsInline(true)
            })
            //.WithFields([
            //    new EmbedFieldBuilder()
            //        .WithName("Field Name")
            //        .WithValue("Field Value")
            //        .WithIsInline(true),
            //    new EmbedFieldBuilder()
            //        .WithName("Field Name 2")
            //        .WithValue("Field Value 2")
            //        .WithIsInline(true)
            //])
            .Build();



        //var embed = new DiscordEmbed {
        //    Title = "Embed title",
        //    Description = "Embed description",
        //    Url = new Uri("https://twitter.com/mujurin_2525"),
        //    Timestamp = new DiscordTimestamp(DateTime.Now),
        //    Color = new DiscordColor(Color.Red), // alpha will be ignored, you can use any RGB color
        //    Footer = new EmbedFooter() { Text = "Footer Text", IconUrl = new Uri(iconUrl) },
        //    Image = new EmbedMedia() {
        //        Url = new Uri(iconUrl),
        //        Width = 150,
        //        Height = 150
        //    },
        //    // //valid for thumb and video
        //    // Provider = new EmbedProvider()
        //    // {
        //    //     Name = "Provider Name",
        //    //     Url = new Uri("Provider Url")
        //    // },
        //    Author = new EmbedAuthor() {
        //        Name = "Author Name",
        //        Url = new Uri("https://twitter.com/mujurin_2525"),
        //        IconUrl = new Uri(iconUrl)
        //    },
        //    //fields
        //    Fields = new List<EmbedField>()
        //    {
        //        new EmbedField()
        //        {
        //            Name = "Field Name",
        //            Value = "Field Value",
        //            Inline = true
        //        },
        //        new EmbedField()
        //        {
        //            Name = "Field Name 2",
        //            Value = "Field Value 2",
        //            Inline = true
        //        }
        //    }
        //};
    }
}


