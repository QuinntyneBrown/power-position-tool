using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPLDataflowByExample
{
    class Util
    {
        static public String TupleListToString(Tuple<IList<int>, IList<int>> tup) {
            StringBuilder result = new StringBuilder();
            result.Append(ListToString(tup.Item1));
            result.Append(" ");
            result.Append(ListToString(tup.Item2));
            return result.ToString();
        }
        static public String ListToString(IList<int> lst) {
            if (lst.Count == 0) return "[]";

            StringBuilder result = new StringBuilder("[");
            foreach (int n in lst) {
                result.AppendFormat("{0},", n.ToString());
            }
            result.Remove(result.Length - 1, 1);
            result.Append("]");
            return result.ToString();
        }
    }
}
