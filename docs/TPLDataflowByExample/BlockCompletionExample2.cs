using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace TPLDataflowByExample
{
    class BlockCompletionExample2
    {
        static public void Run() {
            var block = new ActionBlock<bool>(_ => {
                Console.WriteLine("Block started");
                Thread.Sleep(5000);
                Console.WriteLine("Block ended");
            });

            block.Post(true);

            Console.WriteLine("Waiting");
            block.Complete();
            block.Completion.Wait();
            Console.WriteLine("Task done");
        }
    
    }
}
