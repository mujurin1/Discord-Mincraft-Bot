using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MinecraftBot;

partial class Program
{
    public static HttpClient HttpClient { get; } = new();

    public static async Task Main()
    {
        try {
            await Start();
        } catch (Exception ex) {
            ConsoleWriteLine("予期せぬエラーが発生しました. アプリケーションを終了します");
            ConsoleWriteLine("以下のメッセージをみて対策を試みて下さい");
            Console.WriteLine();
            ConsoleWriteLine("(内容が不明なエラーの場合は開発者に報告して下さい)");
            Console.WriteLine();
            Console.WriteLine(ex.ToString());
        }

        Console.WriteLine();
        Console.WriteLine();
        ConsoleWriteLine(" -- エンターキーを押して終了します --");
        Console.ReadLine();
    }

    private static async Task Start()
    {
        await BotSetting.Load();

        MinecraftProcess.Run();
        var process = MinecraftProcess.Process;
        process.OutputDataReceived += DataReceiveHandler;

        Console.CancelKeyPress += new ConsoleCancelEventHandler(
            async (sender, e) => {
                e.Cancel = true;

                if (!process.HasExited) {
                    ConsoleWriteLine("cancel");
                    await process.StandardInput.WriteLineAsync("stop");
                }
            }
        );

        process.WaitForExit();
    }

    private static void DataReceiveHandler(object sender, DataReceivedEventArgs e)
    {
        var msg = e.Data;
        Console.WriteLine(msg);

        ConsoleWriteLine(msg);
        if (string.IsNullOrWhiteSpace(msg))
            return;

        msg = MyRegex().Replace(msg, string.Empty);

        if (NoticeMessage.FromMinecraftLog(msg) is NoticeMessage message) {
            ConsoleWriteLine(message.Content);    // デバッグ用
            _ = DiscordNotifire.Notice(message.Content);
        }
    }

    public static void ConsoleWriteLine(string? message)
    {
        Console.WriteLine($"NoticeBOT > {message}");
    }

    /// <summary>
    /// コンソールの色などの文字を取り除く
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"\x1b\[[0-9;]*m")]
    private static partial Regex MyRegex();
}

class NoticeMessage
{
    public MessageType Type { get; set; }
    public string Content { get; set; } = null!;


    public static NoticeMessage? FromMinecraftLog(string log)
    {
        log = log[(log.LastIndexOf("INFO]") + 7)..];
        if (string.IsNullOrWhiteSpace(log)) return null;

        if (log.Contains("joined the game", StringComparison.CurrentCulture)) {
            return new NoticeMessage {
                Content = BotSetting.Data.Message.Join.Replace("{name}", log[..log.IndexOf(' ')]),
                Type = MessageType.Join,
            };
        } else if (log.Contains("left the game", StringComparison.CurrentCulture)) {
            return new NoticeMessage {
                Content = BotSetting.Data.Message.Left.Replace("{name}", log[..log.IndexOf(' ')]),
                Type = MessageType.Join,
            };
        } else if (log.StartsWith("Running delayed init tasks")) {
            return new NoticeMessage {
                Content = BotSetting.Data.Message.OpendServer,
                Type = MessageType.Join,
            };
        } else if (log.StartsWith("Stopping the server")) {
            return new NoticeMessage {
                Content = BotSetting.Data.Message.ClosedServer,
                Type = MessageType.Join,
            };
        }

        return null;
    }

    public static string AAAA(string log = "[02:00:51 INFO]: mujurin joined the game")
    {
        var OpendServer = ":green_circle: サーバーを開きました\nどうぞご参加下さい！";
        var ClosedServer = ":red_circle: サーバーを閉じました";
        var Join = "@silent :laughing: {name} さんが参加しました！";
        var Left = "@silent :wave: {name} さんが退出しました";

        log = log[(log.LastIndexOf("INFO]") + 7)..];
        if (string.IsNullOrWhiteSpace(log)) return null;

        if (log.Contains("joined the game", StringComparison.CurrentCulture)) {
            return Join.Replace("{name}", log[..log.IndexOf(' ')]);
        } else if (log.Contains("left the game", StringComparison.CurrentCulture)) {
            return Left.Replace("{name}", log[..log.IndexOf(' ')]);
        } else if (log.StartsWith("Done preparing level")) {
            return OpendServer;
        } else if (log.StartsWith("Stopping server")) {
            return ClosedServer;
        }

        return null;
    }
}

enum MessageType
{
    Open = 0,
    Close = 1,
    Join = 2,
    Exit = 3,
}
