using System;
using System.Threading.Tasks.Dataflow;

namespace TPLDataflowByExample
{
    class DataflowLinkOptionsExample1
    {
        static public void Run() {
            var bufferBlock = new BufferBlock<int>();
            var printBlock = new ActionBlock<int>(n => Console.WriteLine(n));

            for (int i = 0; i < 10; i++) {
                bufferBlock.Post(i);
            }

            var opts = new DataflowLinkOptions { MaxMessages = 5 };
            bufferBlock.LinkTo(printBlock, opts);

            Console.WriteLine("Done");
        }
    }
}
