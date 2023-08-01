using System.Reflection.Metadata.Ecma335;

namespace MinecraftBot;

static class Utils
{
    public static string Join(this IEnumerable<string> _this, string separator) =>
        string.Join(separator, _this);

    public static string JoinFormat(this IEnumerable<string> _this, string format) =>
        string.Join("\n", _this.Select(s => string.Format(format, s)));

    public static IEnumerable<string> FilterNullOrWhiteSpace(params (string, string)[] strs) =>
        strs.Where(str => string.IsNullOrWhiteSpace(str.Item2)) //
            .Select(str => str.Item1);
}
