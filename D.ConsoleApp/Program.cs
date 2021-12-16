// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using D.ConsoleApp;
using D.Core;
using D.Models;
using Microsoft.Extensions.Logging;
using System.Text;

public class Program {

    static string mainPlayer = "";
    static void Main(string[] args) {
        // BenchmarkRunner.Run<GameStateBenchmark>();

        var memoryReader = new MemoryReader(new Logger<MemoryReader>(new LoggerFactory()));
        var p = memoryReader.GetProcessInfo();
        var hashTable = memoryReader.GetUnitHashtableAddress();

        var unitHashTable = WindowsHelper.Read<UnitHashTable>(p.Handle, hashTable);

        var dic = new Dictionary<string, List<IntPtr>>();
        foreach (var unitPtr in unitHashTable.UnitTable) {
            var unitAny = WindowsHelper.Read<UnitAny>(p.Handle, unitPtr);
            ReadAllUnit(unitAny, unitPtr, dic, p.Handle);
            //do {
            //    if (unitAny.UnityType != 0)
            //        continue;

            //    var playerName = Encoding.ASCII.GetString(WindowsHelper.Read<byte>(p.Handle, unitAny.UnitData, 16)).TrimEnd((char)0);
            //    if (!string.IsNullOrEmpty(playerName)) {
            //        if (dic.ContainsKey(playerName)) {
            //            dic[playerName].Add(unitPtr);
            //            mainPlayer = playerName;
            //        } else {
            //            dic.Add(playerName, new List<IntPtr> { unitPtr });
            //        }
            //    }

            //    unitAny = WindowsHelper.Read<UnitAny>(p.Handle, unitAny.pListNext);
            //}while()
            
        }

        foreach (var entry in dic) {
            Console.WriteLine($"{entry.Key} count: {entry.Value.Count}");
        }

        Console.WriteLine("finish");
    }

    public static void ReadAllUnit(UnitAny unitAny, IntPtr curUnitPtr, Dictionary<string, List<IntPtr>> dic, IntPtr hdl) {
        if (curUnitPtr == IntPtr.Zero || unitAny.UnityType != 0)
            return;

        var playerName = Encoding.ASCII.GetString(WindowsHelper.Read<byte>(hdl, unitAny.UnitData, 16)).TrimEnd((char)0);
        if (!string.IsNullOrEmpty(playerName)) {
            if (dic.ContainsKey(playerName)) {
                dic[playerName].Add(curUnitPtr);
                mainPlayer = playerName;
            } else {
                dic.Add(playerName, new List<IntPtr> { curUnitPtr });
            }
        }
        ReadAllUnit(WindowsHelper.Read<UnitAny>(hdl, unitAny.pListNext), unitAny.pListNext, dic, hdl);
    }
}

