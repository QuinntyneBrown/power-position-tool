using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace TPLDataflowByExample
{
    class BatchedJoinBlockExample2
    {
        static public void Run() {
            var bjBlock = new BatchedJoinBlock<int, int>(2);

            var delayBlock1 = MakeDelayBlock(1000);
            var delayBlock2 = MakeDelayBlock(1000);
            
            for (int i = 0; i < 10; i++) {
                delayBlock1.SendAsync(i);
                delayBlock2.SendAsync(i - 2 * i); // same number just negated
            }

            delayBlock1.LinkTo(bjBlock.Target1);
            delayBlock2.LinkTo(bjBlock.Target2);

            for (int i = 0; i < 10; i++) {
                Console.WriteLine(Util.TupleListToString(bjBlock.Receive()));
            }

            Console.WriteLine("Done");
        }

        static TransformBlock<int, int> MakeDelayBlock(int maxdelay) {
            var generator = new Random();
            return new TransformBlock<int, int>(n => {
                Thread.Sleep(generator.Next(maxdelay));
                return n;
            });
        }
    }
}
