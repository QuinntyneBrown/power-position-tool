using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

namespace TPLDataflowByExample
{
    class DataflowBlockOptionsExample3
    {
        static public void Run() {
            var block1 = new BufferBlock<int>(new DataflowBlockOptions { 
                NameFormat = "Fu"
            });
            var block2 = new BufferBlock<int>(new DataflowBlockOptions { 
                NameFormat = "Bar, Class: {0}, Id: {1}" 
            });
            Debug.Assert(false);
        }
    }
}
