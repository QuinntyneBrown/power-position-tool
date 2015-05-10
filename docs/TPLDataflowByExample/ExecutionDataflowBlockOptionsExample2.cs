using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace TPLDataflowByExample
{
    // http://blogs.msdn.com/b/pfxteam/archive/2011/09/27/10217461.aspx
    class ExecutionDataflowBlockOptionsExample2
    {
        static public void Benchmark1() {
            var sw = new Stopwatch();
            const int ITERS = 6000000;
            var are = new AutoResetEvent(false);

            var ab = new ActionBlock<int>(i => { if (i == ITERS) are.Set(); });
            while (true) {
                sw.Restart();
                for (int i = 1; i <= ITERS; i++) ab.Post(i);
                are.WaitOne();
                sw.Stop();
                Console.WriteLine("Messages / sec: {0:N0}",
                    (ITERS / sw.Elapsed.TotalSeconds));
            }
        }
        static public void Benchmark2() {
            var sw = new Stopwatch();
            const int ITERS = 6000000;
            var are = new AutoResetEvent(false);

            var ab = new ActionBlock<int>(i => { if (i == ITERS) are.Set(); },
                new ExecutionDataflowBlockOptions {
                    SingleProducerConstrained = true 
                });
            while (true) {
                sw.Restart();
                for (int i = 1; i <= ITERS; i++) ab.Post(i);
                are.WaitOne();
                sw.Stop();
                Console.WriteLine("Messages / sec: {0:N0}",
                    (ITERS / sw.Elapsed.TotalSeconds));
            }
        }
    }
}
