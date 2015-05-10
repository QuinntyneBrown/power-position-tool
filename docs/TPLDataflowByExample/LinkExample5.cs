using System;
using System.Threading.Tasks.Dataflow;

namespace TPLDataflowByExample
{
    class LinkExample5
    {
        static public void Run() {
            var sourceBlock = new BufferBlock<int>();
            var printBlock = new ActionBlock<int>(
                    n => Console.WriteLine(n)
                );

            // Send only even numbers to printBlock
            sourceBlock.LinkTo(printBlock, n => n % 2 == 0);
            // A NullTarget discards all received messages
            sourceBlock.LinkTo(DataflowBlock.NullTarget<int>());

            for (int i = 0; i < 10; i++) {
                sourceBlock.SendAsync(i);
            }

            Console.WriteLine("Done");
        }
    }
}
