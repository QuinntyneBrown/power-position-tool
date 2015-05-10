using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace TPLDataflowByExample
{
    class ExecutionDataflowBlockOptionsExample1
    {
        static public void Run() {

            var generator = new Random();
            Action<int> fn = n => {
                Thread.Sleep(generator.Next(1000));
                Console.WriteLine(n);
            };
            var opts = new ExecutionDataflowBlockOptions { 
                MaxDegreeOfParallelism = 2 
            };

            var actionBlock = new ActionBlock<int>(fn, opts);

            for (int i = 0; i < 10; i++) {
                actionBlock.Post(i);
            }

            Console.WriteLine("Done");
        }
    }
}
