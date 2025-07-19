using System.Diagnostics;
using System.Text;

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
        var psi = new ProcessStartInfo() {
            WorkingDirectory = BotSetting.Path,
            FileName = BotSetting.Data.Java,
            Arguments = BotSetting.Data.StartupArg,
            //CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            //RedirectStandardInput = true,
            UseShellExecute = false,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        return psi;
    }
}
