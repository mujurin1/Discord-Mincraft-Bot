using System.Net.Mime;
using System.Globalization;
using System.Diagnostics;
using System.Drawing;
using CSharpDiscordWebhook.NET.Discord;

namespace MinecraftBot;

class Program
{
    public static void Main()
    {
        try
        {
            Start();
        }
        catch (Exception ex)
        {
            ConsoleWriteLine("予期せぬエラーが発生しました. アプリケーションを終了します");
            Console.WriteLine();
            Console.WriteLine(ex.Message);
        }

        Console.WriteLine();
        Console.WriteLine();
        ConsoleWriteLine(" -- エンターキーを押して終了します --");
        Console.ReadLine();
    }

    private static void Start()
    {
        // BotSetting.Path = Directory.GetCurrentDirectory();

        if (!BotSetting.Load())
        {
            ConsoleWriteLine($"{BotSetting.FileName} が存在しないため、新規作成します");

            var data = new SettingData();
            while (string.IsNullOrWhiteSpace(data.WebhookUrl))
            {
                Console.Write("ディスコードの WebHookUrl を入力して下さい > ");
                data.WebhookUrl = Console.ReadLine()!;
            }

            BotSetting.Save(data);

            ConsoleWriteLine($"{BotSetting.FileName} を新規作成しました");
        }

        BotSetting.StartWatchFile();

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

        var messages = BotSetting.Data.Message;

        if (msg.IndexOf("joined the game") != -1)
        {
            var name = msg[..msg.IndexOf(' ')];
            return messages.Join.Replace("$1", name);
        }
        else if (msg.IndexOf("left the game") != -1)
        {
            var name = msg[..msg.IndexOf(' ')];
            return messages.Exit.Replace("$1", name);
        }
        else if (msg.StartsWith("Done"))
        {
            return messages.OpendServer;
        }
        else if (msg.StartsWith("Stopping server"))
        {
            return messages.ClosedServer;
        }

        return null;
    }

    public static void ConsoleWriteLine(string? message)
    {
        Console.WriteLine($"NoticeBOT > {message}");
    }
}
