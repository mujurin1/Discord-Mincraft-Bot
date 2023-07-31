namespace MinecraftBot;

static class Utils
{
    public static string Join(this string[] _this, string separator) =>
        string.Join(separator, _this);

    public static string JoinFormat(this string[] _this, string format) =>
        string.Join("\n", _this.Select(s => string.Format(format, s)));
}
