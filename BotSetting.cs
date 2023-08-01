using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MinecraftBot;

static class BotSetting
{
    public static string Path { get; set; } = Directory.GetCurrentDirectory();
    public static readonly string FileName = "noticeApp.yml";

    private static SettingData _data = null!;
    public static SettingData Data
    {
        get => _data;
        set
        {
            _data = value;
            ChangedData();
        }
    }

    private static FileSystemWatcher? _watcher = null;

    public static void Save(SettingData? data = null)
    {
        data ??= Data;

        if (_watcher is not null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }

        try
        {
            var filePath = System.IO.Path.Combine(Path, FileName);
            using var writer = File.CreateText(filePath);

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            serializer.Serialize(writer, data);
        }
        catch (Exception e) when (e is UnauthorizedAccessException or DirectoryNotFoundException)
        {
            throw new Exception($"{FileName} の保存に失敗しました. 以下のメッセージを確認して下さい\n{e}");
        }

        if (data != Data)
        {
            Data = data;
        }

        StartWatchFile();
    }

    public static bool Load()
    {
        if (LoadFromFile() is not SettingData data)
            return false;

        var errors = data.CheckErrors();
        if (errors is { Count: > 0 })
        {
            throw new Exception(
                $"""
            {FileName} の内容が不正です
            以下の情報を確認して {FileName} を修正して下さい
            {errors.Join("\n")}
            """
            );
        }

        Data = data;

        return true;
    }

    private static void ChangedData()
    {
        DiscordNotifire.ChangeWebhookUrl(Data.WebhookUrl);
    }

    public static void StartWatchFile()
    {
        _watcher?.Dispose();

        _watcher = new FileSystemWatcher()
        {
            Path = Path,
            NotifyFilter = NotifyFilters.LastWrite,
            Filter = FileName
        };
        _watcher.Changed += FileChanged;

        _watcher.EnableRaisingEvents = true; // 監視を開始する
    }

    private static void FileChanged(object source, FileSystemEventArgs e)
    {
        if (LoadFromFile() is not SettingData data)
        {
            Program.ConsoleWriteLine($"Minecraft Bot ERROR !");
            Program.ConsoleWriteLine($"{FileName} のロードに失敗しました");
            return;
        }

        var errors = data.CheckErrors();

        if (errors.Count > 0)
        {
            Program.ConsoleWriteLine($"{FileName} の内容が不正です");
            Program.ConsoleWriteLine($"以下の情報を確認して {FileName} を修正して下さい");
            Console.WriteLine($"\n{errors.Join("\n")}\n");
            return;
        }

        Data = data;
        Program.ConsoleWriteLine($"{FileName} の変更を反映しました");
    }

    public static SettingData? LoadFromFile()
    {
        var filePath = System.IO.Path.Combine(Path, FileName);

        try
        {
            if (!File.Exists(filePath))
                return null;

            using var stream = File.OpenText(filePath);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return deserializer.Deserialize<SettingData>(stream);
        }
        catch
        {
            return null;
        }
    }
}

class SettingData
{
    public string Java { get; set; } = "java";
    public string StartupArg { get; set; } = "-Xmx2048M -jar paper.jar nogui";
    public string WebhookUrl { get; set; } = "";
    public string BotName { get; set; } = "Minecraft Notifier";
    public MessageText Message { get; set; } = new();

    public class MessageText
    {
        public string OpendServer { get; set; } = ":green_circle: サーバーを開きました";
        public string ClosedServer { get; set; } = ":red_circle: サーバーを閉じました";
        public string Join { get; set; } = ":laughing: $1 さんが参加しました！";
        public string Exit { get; set; } = ":wave: $1 さんが退出しました";
    }

    public List<string> CheckErrors()
    {
        var errors = Utils
            .FilterNullOrWhiteSpace(
                (nameof(Java), Java),
                (nameof(StartupArg), StartupArg),
                (nameof(WebhookUrl), WebhookUrl),
                (nameof(BotName), BotName),
                (nameof(Message.OpendServer), Message.OpendServer),
                (nameof(Message.ClosedServer), Message.ClosedServer),
                (nameof(Message.Join), Message.Join),
                (nameof(Message.Exit), Message.Exit)
            )
            .Select(name => $"> {name} が空文字です")
            .ToList();

        return errors;
    }
}
