using MinecraftConnection;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static string address = "127.0.0.1";
    static ushort port = 25575;
    static string pass = "rcon_pass";
    static MinecraftCommands command = new MinecraftCommands(address, port, pass);

    public static async Task Main()
    {
        var res = command.SendCommand("list");
        Console.WriteLine(res);
    }
}
