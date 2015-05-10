using System;
using System.Threading.Tasks.Dataflow;

namespace TPLDataflowByExample
{
    class WriteOnceBlockExample1
    {
        static public void Run() {

            var woBlock = new WriteOnceBlock<int>(n => n);

            for (int i = 0; i < 10; i++) {
                woBlock.Post(i);
            }

            for (int i = 0; i < 10; i++) {
                Console.WriteLine(woBlock.Receive());
            }

            Console.WriteLine("Done");
        }
    }
}
