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
            if (value != Data)
            {
                _data = value;
                DiscordNotifire.ChangeWebhookUrl(Data.WebhookUrl);
            }
        }
    }

    private static FileSystemWatcher? _watcher;

    public static void Save(SettingData? data = null)
    {
        data ??= Data;

        if (_watcher is not null)
            _watcher.EnableRaisingEvents = false;

        try
        {
            var filePath = System.IO.Path.Combine(Path, FileName);
            using var writer = File.CreateText(filePath);

            var serializer = new Serializer();

            serializer.Serialize(writer, data);
        }
        catch (Exception e) when (e is UnauthorizedAccessException or DirectoryNotFoundException)
        {
            throw new Exception($"{FileName} の保存に失敗しました. 以下のメッセージを確認して下さい\n{e}");
        }

        Data = data;

        StartWatchFile();
    }

    public static async Task Load()
    {
        SettingData? data;
        try
        {
            data = LoadFromFile();
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            throw new Exception($"{FileName} の読み込みに失敗しました. 以下の情報を確認して {FileName} を修正して下さい", ex);
        }

        if (data is null)
        {
            Program.ConsoleWriteLine($"{FileName} が存在しないため、新規作成します");

            Save(new SettingData(await InputWebhookUrl()));

            Program.ConsoleWriteLine($"{FileName} を新規作成しました");
            return;
        }

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

        if (!await DiscordNotifire.CheckWebhookUrl(data.WebhookUrl))
        {
            Program.ConsoleWriteLine($"{FileName} の {nameof(SettingData.WebhookUrl)} が不正です");
            data.WebhookUrl = await InputWebhookUrl();
            Save(data);
            Program.ConsoleWriteLine($"{FileName} を更新しました");
            return;
        }

        Data = data;
        StartWatchFile();
    }

    private static void StartWatchFile()
    {
        if (_watcher is not null)
        {
            _watcher.EnableRaisingEvents = true;
            return;
        }

        _watcher = new FileSystemWatcher(Path, FileName)
        {
            NotifyFilter = NotifyFilters.LastWrite,
        };

        _watcher.Changed += FileChanged;
        _watcher.EnableRaisingEvents = true; // 監視を開始する
    }

    private static async void FileChanged(object source, FileSystemEventArgs e)
    {
        SettingData? data;
        try
        {
            data = LoadFromFile();
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            Program.ConsoleWriteLine($"{FileName} の読み込みに失敗しました");
            Program.ConsoleWriteLine($"以下のメッセージを見て {FileName} を修正して下さい");
            Console.WriteLine();
            Console.WriteLine(ex.ToString());
            return;
        }

        if (data is null)
        {
            Program.ConsoleWriteLine($"{FileName} の読み込みに失敗しました");
            Console.WriteLine($"{FileName} が存在しません");
            return;
        }

        var errors = data.CheckErrors();

        if (errors.Count > 0)
        {
            Program.ConsoleWriteLine($"{FileName} の読み込みに失敗しました");
            Program.ConsoleWriteLine($"{FileName} の内容が不正です");
            Program.ConsoleWriteLine($"以下の情報を確認して {FileName} を修正して下さい");
            Console.WriteLine($"\n{errors.Join("\n")}\n");
            return;
        }

        if (!await DiscordNotifire.CheckWebhookUrl(data.WebhookUrl))
        {
            Program.ConsoleWriteLine($"{FileName} の読み込みに失敗しました");
            Console.WriteLine($"{FileName} の {nameof(SettingData.WebhookUrl)} が無効です");
            return;
        }

        Data = data;
        Program.ConsoleWriteLine($"{FileName} の変更を反映しました");
    }

    public static SettingData? LoadFromFile()
    {
        var filePath = System.IO.Path.Combine(Path, FileName);

        if (!File.Exists(filePath))
            return null;

        var deserializer = new Deserializer();
        try
        {
            using var stream = File.OpenText(filePath);
            return deserializer.Deserialize<SettingData>(stream);
        }
        catch (IOException)
        {
            Task.Delay(3000).Wait();

            using var stream = File.OpenText(filePath);
            return deserializer.Deserialize<SettingData>(stream);
        }
    }

    private static async Task<string> InputWebhookUrl()
    {
        while (true)
        {
            Console.Write("ディスコードの WebHookUrl を入力して下さい > ");
            var url = Console.ReadLine()!;

            if (await DiscordNotifire.CheckWebhookUrl(url))
                return url;

            Console.WriteLine("無効なURLです");
            Console.WriteLine();
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

    public SettingData() { }

    public SettingData(string webHookUrl)
    {
        WebhookUrl = webHookUrl;
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
