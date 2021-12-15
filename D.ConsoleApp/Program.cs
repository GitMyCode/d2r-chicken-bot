// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using D.ConsoleApp;
using D.Core;
using D.Models;
using Microsoft.Extensions.Logging;
using System.Text;

public class Program {
    static void Main(string[] args) {
        // BenchmarkRunner.Run<GameStateBenchmark>();

        var memoryReader = new MemoryReader(new Logger<MemoryReader>(new LoggerFactory()));
        var p = memoryReader.GetProcessInfo();
        var hashTable = memoryReader.GetUnitHashtableAddress();

        var unitHashTable = WindowsHelper.Read<UnitHashTable>(p.Handle, hashTable);

        var dic = new Dictionary<string, List<IntPtr>>();
        var mainPlayer = string.Empty;
        foreach (var unitPtr in unitHashTable.UnitTable) {
            var unitAny = WindowsHelper.Read<UnitAny>(p.Handle, unitPtr);
            if (unitAny.UnityType != 0)
                continue;

            var playerName = Encoding.ASCII.GetString(WindowsHelper.Read<byte>(p.Handle, unitAny.UnitData, 16)).TrimEnd((char)0);
            if (!string.IsNullOrEmpty(playerName)) {
                if (dic.ContainsKey(playerName)) {
                    dic[playerName].Add(unitPtr);
                    mainPlayer = playerName;
                } else {
                    dic.Add(playerName, new List<IntPtr> { unitPtr });
                }
            }
        }

        foreach (var entry in dic) {
            Console.WriteLine($"{entry.Key} count: {entry.Value.Count}");
        }

        Console.WriteLine("finish");
    }
}

