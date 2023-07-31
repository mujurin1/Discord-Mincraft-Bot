using System.Diagnostics;
using System.Drawing;
using CSharpDiscordWebhook.NET.Discord;

namespace MinecraftBot;

class Program
{
    private static SettingData _setting => BotSetting.Data;

    public static void Main()
    {
        try
        {
            Start();
        }
        catch (Exception ex)
        {
            ConsoleWriteLine("予期せぬエラーが発生しました");
            ConsoleWriteLine("マインクラフトの動作には影響ありませんが、通知が行われなくなります");
            ConsoleWriteLine("コマンドプロンプト (この画面) の文字が更新されなくなります");
            Console.WriteLine();
            ConsoleWriteLine("再度通知を有効化するには、サーバーを再起動して下さい");
            Console.WriteLine();
            ConsoleWriteLine("また、以下のエラーメッセージをコピーして報告して下さい");
            Console.WriteLine(ex.Message);
            Console.WriteLine();
        }

        Console.WriteLine();
        Console.WriteLine();
        ConsoleWriteLine(" -- エンターキーを押して終了します --");
        Console.ReadLine();
    }

    private static void Start()
    {
        BotSetting.StartWatchFile(Directory.GetCurrentDirectory());
        BotSetting.Load(true);
        DiscordNotifire.ChangeWebhookUrl(BotSetting.Data.WebhookUrl);

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

        if (MinecraftLogToNoticeMessage(msg) is string _message)
        {
            DiscordNotifire.Notice(_message);
        }
    }

    private static string? MinecraftLogToNoticeMessage(string msg)
    {
        // msg: [06:02:37 INFO]: mujurin joined the game
        msg = msg[(msg.LastIndexOf("INFO]") + 7)..];
        // msg: mujurin joined the game

        if (msg.IndexOf("joined the game") != -1)
        {
            var name = msg[..msg.IndexOf(' ')];
            return _setting.Join.Replace("$1", name);
        }
        else if (msg.IndexOf("left the game") != -1)
        {
            var name = msg[..msg.IndexOf(' ')];
            return _setting.Exit.Replace("$1", name);
        }
        else if (msg.StartsWith("Done"))
        {
            return _setting.OpendServer;
        }
        else if (msg.StartsWith("Stopping server"))
        {
            return _setting.ClosedServer;
        }

        return null;
    }

    public static void ConsoleWriteLine(string? message)
    {
        Console.WriteLine($"NoticeBOT > {message}");
    }
}
