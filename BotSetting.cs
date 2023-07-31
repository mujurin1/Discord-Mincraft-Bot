using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MinecraftBot;

static class BotSetting
{
    public static string Path { get; set; } = null!;
    public static readonly string FileName = "noticeApp.yml";
    public static SettingData Data { get; private set; } = new();

    private static FileSystemWatcher? _watcher = null;

    public static void Save()
    {
        _watcher?.Dispose();

        var filePath = System.IO.Path.Combine(Path, FileName);
        using var writer = File.CreateText(filePath);

        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        serializer.Serialize(writer, Data);

        StartWatchFile(Path);
    }

    public static void Load(bool isCreate = false)
    {
        var filePath = System.IO.Path.Combine(Path, FileName);

        if (!File.Exists(filePath))
        {
            if (isCreate)
                Save();

            return;
        }

        using var stream = File.OpenText(filePath);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        if (deserializer.Deserialize<SettingData>(stream) is not SettingData data)
        {
            throw new FileLoadException($"{FileName} の形式が不正です");
        }

        Data = data;
    }

    public static void StartWatchFile(string path)
    {
        Path = path;
        _watcher?.Dispose();

        _watcher = new()
        {
            Path = Path,
            NotifyFilter = NotifyFilters.LastWrite,
            Filter = FileName
        };
        _watcher.Changed += FileChanged;

        //監視を開始する
        _watcher.EnableRaisingEvents = true;
    }

    private static void FileChanged(object source, FileSystemEventArgs e)
    {
        DiscordNotifire.ChangeWebhookUrl(Data.WebhookUrl);

        try
        {
            Load();
            Program.ConsoleWriteLine($"{FileName} の変更を反映しました");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Program.ConsoleWriteLine("INTERNAL ERROR:");
            Console.WriteLine(ex.ToString());
            Console.WriteLine();
        }
    }
}

class SettingData
{
    public string StartupArg { get; set; } = "-Xmx2048M -jar paper.jar nogui";
    public string WebhookUrl { get; set; } = "";
    public string BotName { get; set; } = "Minecraft Notifier";
    public string OpendServer { get; set; } = ":green_circle: サーバーを開きました";
    public string ClosedServer { get; set; } = ":red_circle: サーバーを閉じました";
    public string Join { get; set; } = ":laughing: $1 さんが参加しました！";
    public string Exit { get; set; } = ":wave: $1 さんが退出しました";
}
