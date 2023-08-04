using YamlDotNet.Serialization;

namespace MinecraftBot;

static class BotSetting
{
    public static string Path { get; set; } = Directory.GetCurrentDirectory();
    public static readonly string FileName = "noticeApp";
    public static readonly string OldFileName = $"{FileName}.old";
    public static readonly string FileExtend = "yml";
    public static readonly string FileFullName = $"{FileName}.{FileExtend}";

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

    public static void Save(SettingData? data = null, string? fileName = null)
    {
        data ??= Data;
        fileName = FileFullName;

        if (_watcher is not null) _watcher.EnableRaisingEvents = false;

        try
        {
            var filePath = System.IO.Path.Combine(Path, fileName);
            using var writer = File.CreateText(filePath);
            var serializer = new Serializer();
            serializer.Serialize(writer, data);
        }
        catch (Exception e) when (e is UnauthorizedAccessException or DirectoryNotFoundException)
        {
            throw new Exception($"{fileName} の保存に失敗しました. 以下のメッセージを確認して下さい\n{e}");
        }

        Data = data;
        StartWatchFile();
    }

    public static async Task Load()
    {
        // このメソッド特に汚くてつらくなりそう(´・ω・`)

        SettingData? data = null;
        try
        {
            data = LoadFromFile();
        }
        catch (YamlDotNet.Core.YamlException)
        {
            //MEMO: バージョンアップの差で読み取り不可能な場合
            //      あるいは、記述を間違えたせい
            Program.ConsoleWriteLine($"バージョンアップにより {FileFullName} の内容が変わっているか、記述を間違えているため読み取りに失敗しました");
            var saveOldFileName = Utils.SearchUniqueFileName(Path, OldFileName, FileExtend);
            Program.ConsoleWriteLine($"現在の {FileFullName} を {saveOldFileName} に保存して新規作成します\n");

            File.Move(FileFullName, saveOldFileName);
        }

        if (data is null)
        {
            Program.ConsoleWriteLine($"{FileFullName} が存在しないため、新規作成します");
            Save(new SettingData(await InputWebhookUrl()) { DontEditThisValue = SettingData.CURRENT_SETTING_VALUE });
            Program.ConsoleWriteLine($"{FileFullName} を新規作成しました");
            return;
        }

        var errors = data.CheckErrors();
        if (errors.Count == 0) errors = await data.CheckStrictErrors();

        if (errors.Count > 0)
        {
            throw new Exception(
                $"""
                {FileFullName} の読み込みに失敗しました. {FileFullName} の内容が不正です
                以下の情報を確認して {FileFullName} を修正して下さい

                {errors.Join("\n")}
                """
            );
        }

        Data = data;

        if (!Data.CheckLatestVersion())
        {
            // MEMO: バージョンアップしてるけど、ファイルの読み取りは出来た場合
            //       けど、内容が変わっているよってことを伝える & 以前の内容を消さないために

            Data.DontEditThisValue = SettingData.CURRENT_SETTING_VALUE;
            var saveOldFileName = Utils.SearchUniqueFileName(Path, OldFileName, FileExtend);

            Program.ConsoleWriteLine($"バージョンアップにより {FileFullName} の内容が変わっています");
            Program.ConsoleWriteLine($"現在の内容を引き継いで {FileFullName} を新規作成します");
            Program.ConsoleWriteLine($"(現在の {FileFullName} は {saveOldFileName} として保存します)");

            File.Move(FileFullName, saveOldFileName);
            Save();

            Console.WriteLine("\nエンターキーを押して続行する >");
            Console.ReadLine();
        }

        StartWatchFile();
    }

    private static void StartWatchFile()
    {
        if (_watcher is not null)
        {
            _watcher.EnableRaisingEvents = true;
            return;
        }

        _watcher = new FileSystemWatcher(Path, FileFullName)
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
            Program.ConsoleWriteLine($"{FileFullName} の読み込みに失敗しました");
            Program.ConsoleWriteLine($"以下のメッセージを見て {FileFullName} を修正して下さい\n");
            Console.WriteLine(ex.ToString());
            return;
        }

        if (data is null)
        {
            Program.ConsoleWriteLine($"{FileFullName} の読み込みに失敗しました");
            Console.WriteLine($"{FileFullName} が存在しません");
            return;
        }

        var errors = data.CheckErrors();
        if (errors.Count == 0) errors = await data.CheckStrictErrors();

        if (errors.Count > 0)
        {
            Program.ConsoleWriteLine($"{FileFullName} の読み込みに失敗しました");
            Program.ConsoleWriteLine($"{FileFullName} の内容が不正です");
            Program.ConsoleWriteLine($"以下の情報を確認して {FileFullName} を修正して下さい\n");
            Console.WriteLine($"{errors.Join("\n")}");
            return;
        }

        Data = data;
        Program.ConsoleWriteLine($"{FileFullName} の変更を反映しました");
    }

    public static SettingData? LoadFromFile()
    {
        var filePath = System.IO.Path.Combine(Path, FileFullName);

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
            // VisualStudioCode で保存すると即時の読み取りに失敗するため3秒待つ
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
    public const string CURRENT_SETTING_VALUE = "1";

    public string DontEditThisValue { get; set; } = "";
    public string Java { get; set; } = "java";
    public string StartupArg { get; set; } = "-Xmx2048M -jar paper.jar nogui";
    public string WebhookUrl { get; set; } = "";
    public string? BotName { get; set; } = null;
    public MessageText Message { get; set; } = new();

    public class MessageText
    {
        public string OpendServer { get; set; } = ":green_circle: サーバーを開きました\nどうぞご参加下さい！";
        public string ClosedServer { get; set; } = ":red_circle: サーバーを閉じました";
        public string Join { get; set; } = "@silent :laughing: {name} さんが参加しました！";
        public string Left { get; set; } = "@silent :wave: {name} さんが退出しました";
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
                (nameof(WebhookUrl), WebhookUrl)
            )
            .Select(name => $"> {name} が空文字です")
            .ToList();

        return errors;
    }

    /// <summary>
    /// CheckErrors より厳密なエラーチェックを行う
    /// </summary>
    /// <returns></returns>
    public async Task<List<string>> CheckStrictErrors()
    {
        var errors = new List<string>();

        if (!await DiscordNotifire.CheckWebhookUrl(WebhookUrl))
            errors.Add($"""
            {nameof(WebhookUrl)} が古くなっているか、有効なURLではありません
            有効なURLに変更して再度起動して下さい
            """);

        return errors;
    }

    public bool CheckLatestVersion() => DontEditThisValue == CURRENT_SETTING_VALUE;
}
