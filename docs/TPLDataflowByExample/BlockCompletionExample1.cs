using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace TPLDataflowByExample
{
    class BlockCompletionExample1
    {
        static public void Run() {

            Action<int> fn = n => {
                Thread.Sleep(1000);
                Console.WriteLine(n);
            };

            var actionBlock = new ActionBlock<int>(fn);

            actionBlock.Post(42);
            actionBlock.Complete();
            for (int i = 0; i < 10; i++) {
                actionBlock.Post(i);
            }
            // Even though we send the block all the data it will
            // only execute once because we tell it to "Complete"

            Console.WriteLine("Done");
        }
    }
}
