using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace TPLDataflowByExample
{
    class DataflowBlockOptionsExample1
    {
        static public void Run() {

            Action<int> fn = n => {
                Thread.Sleep(1000);
                Console.WriteLine(n);
            };
            var opts = new ExecutionDataflowBlockOptions { BoundedCapacity = 1 };
            // Sets the block's buffer size to one message

            var actionBlock = new ActionBlock<int>(fn, opts);

            for (int i = 0; i < 10; i++) {
                //Console.WriteLine(actionBlock.Post(i)); 
                actionBlock.SendAsync(i);
            }

            Console.WriteLine("Done");
        }
    }
}
