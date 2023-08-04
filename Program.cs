using System.Diagnostics;

namespace MinecraftBot;

class Program
{
    public static HttpClient HttpClient { get; } = new();

    public static async Task Main()
    {
        try
        {
            await Start();
        }
        catch (Exception ex)
        {
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
            async (sender, e) =>
            {
                e.Cancel = true;

                if (!process.HasExited)
                {
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

        if (string.IsNullOrWhiteSpace(msg))
            return;

        if (NoticeMessage.FromMinecraftLog(msg) is NoticeMessage message)
        {
            DiscordNotifire.Notice(message.Content);
        }
    }

    public static void ConsoleWriteLine(string? message)
    {
        Console.WriteLine($"NoticeBOT > {message}");
    }
}

class NoticeMessage
{
    public MessageType Type { get; set; }
    public string Content { get; set; } = null!;


    public static NoticeMessage? FromMinecraftLog(string log)
    {
        log = log[(log.LastIndexOf("INFO]") + 7)..];
        if (string.IsNullOrWhiteSpace(log)) return null;

        if (log.IndexOf("joined the game") != -1)
        {
            return new NoticeMessage
            {
                Content = BotSetting.Data.Message.Join.Replace("{name}", log[..log.IndexOf(' ')]),
                Type = MessageType.Join,
            };
        }
        else if (log.IndexOf("left the game") != -1)
        {
            return new NoticeMessage
            {
                Content = BotSetting.Data.Message.Left.Replace("{name}", log[..log.IndexOf(' ')]),
                Type = MessageType.Join,
            };
        }
        else if (log.StartsWith("Done"))
        {
            return new NoticeMessage
            {
                Content = BotSetting.Data.Message.OpendServer,
                Type = MessageType.Join,
            };
        }
        else if (log.StartsWith("Stopping server"))
        {
            return new NoticeMessage
            {
                Content = BotSetting.Data.Message.ClosedServer,
                Type = MessageType.Join,
            };
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
