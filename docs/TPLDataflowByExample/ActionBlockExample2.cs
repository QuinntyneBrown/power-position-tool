using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace TPLDataflowByExample
{
    class ActionBlockExample2
    {
        static public void Run() {

            var actionBlock = new ActionBlock<int>(n => {
                Thread.Sleep(1000);
                Console.WriteLine(n);
            });
            for (int i = 0; i < 10; i++) {
                actionBlock.Post(i);
            }

            Console.WriteLine("Done");
        }
    }
}
