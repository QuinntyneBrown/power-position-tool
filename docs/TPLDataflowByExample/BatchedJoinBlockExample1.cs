using System;
using System.Threading.Tasks.Dataflow;

namespace TPLDataflowByExample
{
    class BatchedJoinBlockExample1
    {
        static public void Run() {
            var bjBlock = new BatchedJoinBlock<int, int>(2);

            for (int i = 0; i < 10; i++) {
                bjBlock.Target1.Post(i);
            }

            for (int i = 0; i < 10; i++) {
                bjBlock.Target2.Post(i);
            }

            for (int i = 0; i < 10; i++) {
                Console.WriteLine(Util.TupleListToString(bjBlock.Receive()));
            }

            Console.WriteLine("Done");
        }
    }
}
