namespace MinecraftBot;

static class Utils
{
    public static string Join(this IEnumerable<string> _this, string separator)
        => string.Join(separator, _this);

    public static string JoinFormat(this IEnumerable<string> _this, string format)
        => string.Join("\n", _this.Select(s => string.Format(format, s)));

    public static IEnumerable<string> FilterNullOrWhiteSpace(params (string, string)[] strs)
        => strs
            .Where(str => string.IsNullOrWhiteSpace(str.Item2))
            .Select(str => str.Item1);

    public static string SearchUniqueFileName(string dir, string fileName, string fileExtend)
    {
        if (!File.Exists(Path.Combine(dir, $"{fileName}.{fileExtend}"))) return $"{fileName}.{fileExtend}";

        var serial = 1;

        while (true)
        {
            var newFileName = $"{fileName} ({serial}).{fileExtend}";
            if (!File.Exists(Path.Combine(dir, newFileName)))
                return newFileName;
        }
    }
}
