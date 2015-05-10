using System;
using System.Threading.Tasks.Dataflow;

namespace TPLDataflowByExample
{
    class LinkExample2
    {
        static public void Run() {
            var printBlock1 = MakePrintBlock("printBlock1");
            var printBlock2 = MakePrintBlock("printBlock2");

            var bufferBlock = new BufferBlock<int>();

            bufferBlock.LinkTo(printBlock1, n => n % 2 == 0);
            bufferBlock.LinkTo(printBlock2);

            for (int i = 0; i < 10; i++) {
                bufferBlock.Post(i);
            }

            Console.WriteLine("Done");
        }

        static ActionBlock<int> MakePrintBlock(String prefix) {
            return new ActionBlock<int>(
                n => Console.WriteLine(prefix + " Accepted " + n)
            );
        }
    }
}
