using System.Drawing;
using CSharpDiscordWebhook.NET.Discord;

namespace MinecraftBot;

static class DiscordNotifire
{
    public static DiscordWebhook Webhook { get; private set; } = new();

    public static void ChangeWebhookUrl(string url)
    {
        Webhook.WebhookUrl = new(url);
    }

    public static Task<DiscordResponse> Notice(string message)
    {
        return Webhook.SendAsync(DefaultMessage(message));
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

    public static DiscordMessage DefaultMessage(
        string content,
        string? userName = null,
        DiscordMessageFlag flag = DiscordMessageFlag.None
    )
    {
        if (content.IndexOf("@silent") == 0)
        {
            content = content[7..].TrimStart();
            flag |= DiscordMessageFlag.SUPPRESS_NOTIFICATIONS;
        }

        return new DiscordMessage()
        {
            Content = content,
            // TTS = true, //read message to everyone on the channel
            Username = userName ?? BotSetting.Data.BotName,
            Flags = flag
        };
    }

    private static DiscordEmbed CreateEmbed()
    {
        var iconUrl =
            "https://pbs.twimg.com/profile_images/1014513302775271424/IvvOe1Il_400x400.jpg";

        var embed = new DiscordEmbed
        {
            Title = "Embed title",
            Description = "Embed description",
            Url = new Uri("https://twitter.com/mujurin_2525"),
            Timestamp = new DiscordTimestamp(DateTime.Now),
            Color = new DiscordColor(Color.Red), // alpha will be ignored, you can use any RGB color
            Footer = new EmbedFooter() { Text = "Footer Text", IconUrl = new Uri(iconUrl) },
            Image = new EmbedMedia()
            {
                Url = new Uri(iconUrl),
                Width = 150,
                Height = 150
            },
            // //valid for thumb and video
            // Provider = new EmbedProvider()
            // {
            //     Name = "Provider Name",
            //     Url = new Uri("Provider Url")
            // },
            Author = new EmbedAuthor()
            {
                Name = "Author Name",
                Url = new Uri("https://twitter.com/mujurin_2525"),
                IconUrl = new Uri(iconUrl)
            },
            //fields
            Fields = new List<EmbedField>()
            {
                new EmbedField()
                {
                    Name = "Field Name",
                    Value = "Field Value",
                    Inline = true
                },
                new EmbedField()
                {
                    Name = "Field Name 2",
                    Value = "Field Value 2",
                    Inline = true
                }
            }
        };

        return embed;
    }
}


