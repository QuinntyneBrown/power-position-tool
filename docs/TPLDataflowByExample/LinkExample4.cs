using System;
using System.Threading.Tasks.Dataflow;

namespace TPLDataflowByExample
{
    class LinkExample4
    {
        static public void Run() {
            var sourceBlock = new BroadcastBlock<int>(n => n);
                // we use a BroadcastBlock so it will
                // discard unused messages
            var printBlock = new ActionBlock<int>(
                    n => Console.WriteLine(n)
                );

            // Send only even numbers to printBlock
            sourceBlock.LinkTo(printBlock, n => n % 2 == 0);

            for (int i = 0; i < 10; i++) {
                sourceBlock.SendAsync(i);
            }

            Console.WriteLine("Done");
        }
    }
}
