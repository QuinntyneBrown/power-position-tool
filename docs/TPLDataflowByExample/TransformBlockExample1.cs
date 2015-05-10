using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace TPLDataflowByExample
{
    class TransformBlockExample1
    {
        static public void Run() {
            Func<int, int> fn = n => {
                Thread.Sleep(1000);
                return n * n;
            };

            var tfBlock = new TransformBlock<int, int>(fn);

            for (int i = 0; i < 10; i++) {
                tfBlock.Post(i);
            }

            for (int i = 0; i < 10; i++) {
                int result = tfBlock.Receive();
                Console.WriteLine(result);
            }

            Console.WriteLine("Done");
        }
    }
}
