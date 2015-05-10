using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TPLDataflowByExample
{
    class TransformBlockExample2
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

            // RecieveAsynch returns a Task
            for (int i = 0; i < 10; i++) {
                Task<int> resultTask = tfBlock.ReceiveAsync();
                int result = resultTask.Result; 
                    // Calling Result will wait until it has a value ready
                Console.WriteLine(result);
            }

            Console.WriteLine("Done");
        }
    }
}
