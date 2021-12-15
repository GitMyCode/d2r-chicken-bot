using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using D.Core;
using Microsoft.Extensions.Logging;

namespace D.ConsoleApp {
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class GameStateBenchmark {
        public MemoryReader memoryReader;

        [GlobalSetup]
        public void Setup() {
            memoryReader = new MemoryReader(new Logger<MemoryReader>(new LoggerFactory()));
        }

        [Benchmark]
        public void GetState() {
            var state = memoryReader.GetState();
        }

        [Benchmark]
        public void GetTcpTable() {
            var state = InteropTcpHelper.GetAllTcpConnections();
        }
    }
}