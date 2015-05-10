using System;
using System.Threading.Tasks.Dataflow;

namespace TPLDataflowByExample
{
    static class ActionBlockExample1
    {
        static public void Run() {

            var actionBlock = new ActionBlock<int>(n => Console.WriteLine(n));

            for (int i = 0; i < 10; i++) {
                actionBlock.Post(i);
            }

            Console.WriteLine("Done");
        }
    }
}
