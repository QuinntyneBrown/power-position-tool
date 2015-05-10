using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace TPLDataflowByExample
{
    class DataflowBlockOptionsExample2
    {
        static public void Run() {

            Action<int> fn = n => {
                Thread.Sleep(1000);
                Console.WriteLine(
                    n + " ThreadId:" + Thread.CurrentThread.ManagedThreadId
                    );
            };
            var opts = new ExecutionDataflowBlockOptions { 
                MaxMessagesPerTask = 1 
            };
            // Each Task will only process one message
            // A new task will be created for every new message

            var actionBlock = new ActionBlock<int>(fn, opts);

            for (int i = 0; i < 10; i++) {
                actionBlock.Post(i);
            }

            Console.WriteLine("Done");
        }
    }
}
