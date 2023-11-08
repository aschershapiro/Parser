using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    [Serializable]
    public class DataLabels
    {
        public List<String> FloatLabels = new List<string>();
        public List<String> UInt16Labels = new List<string>();
        public int FloatSize;
        public int UInt16Size;
        public int[] Header = new int[3];
        public DataLabels() {

        }
    }
    public static class DataLabelsSerializer
    {

    }
}
