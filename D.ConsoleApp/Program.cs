// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using D.ConsoleApp;
using D.Core;

public class Program
{
    static void Main(string[] args)
    {
       BenchmarkRunner.Run<GameStateBenchmark>();
    }
}

