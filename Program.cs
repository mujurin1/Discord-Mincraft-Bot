using System.Net.Mime;
using System.Globalization;
using System.Diagnostics;
using System.Drawing;
using CSharpDiscordWebhook.NET.Discord;

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
            ConsoleWriteLine("以下のメッセージを開発者へ送信して下さい");
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
