using System.Diagnostics;

namespace MinecraftBot;

static class MinecraftProcess
{
    public static Process Process { get; private set; } = null!;

    public static void Run()
    {
        if (Process != null)
            return;

        Program.ConsoleWriteLine("マインクラフトサーバーを起動します\n");

        Process = Process.Start(CreateProcessStartInfo())!;

        Process.BeginOutputReadLine();
    }

    private static ProcessStartInfo CreateProcessStartInfo()
    {
        return new ProcessStartInfo()
        {
            WorkingDirectory = BotSetting.Path,
            FileName = BotSetting.Data.Java,
            Arguments = BotSetting.Data.StartupArg,
            RedirectStandardOutput = true,
            // RedirectStandardInput = true,
            UseShellExecute = false,
        };
    }
}
