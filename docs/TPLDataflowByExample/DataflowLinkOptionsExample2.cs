using System;
using System.Threading.Tasks.Dataflow;

namespace TPLDataflowByExample
{
    class DataflowLinkOptionsExample2
    {
        static public void Run() {
            var block1 = MakePrinter("block1");
            var block2 = MakePrinter("block2");

            var source = new BufferBlock<int>();

            source.LinkTo(block1);

            var opt = new DataflowLinkOptions { Append = false };
            source.LinkTo(block2, opt);

            for (int i = 0; i < 10; i++) {
                source.SendAsync(i);
            }
        }

        static ActionBlock<int> MakePrinter(String prefix) {
            return new ActionBlock<int>(
                n => Console.WriteLine(prefix + ": " + n));
        }
    }
}
