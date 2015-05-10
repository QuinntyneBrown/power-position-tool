using System;
using System.Threading.Tasks.Dataflow;

namespace TPLDataflowByExample
{
    class LinkExample1
    {
        static public void Run() {
            var bufferBlock = new BufferBlock<int>();
            var printBlock = new ActionBlock<int>(
                    n => Console.WriteLine(n)
                );

            for (int i = 0; i < 10; i++) {
                bufferBlock.Post(i);
            }

            bufferBlock.LinkTo(printBlock);
            
            Console.WriteLine("Done");
        }
    }
}
