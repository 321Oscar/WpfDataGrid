using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERad5TestGUI.Models
{
    public class LinConfigSignal : TransFormSignalBase
    {
        public LinConfigSignal()
        {

        }

        public LinConfigSignal(Stores.Signal signal, string viewName) : base(signal, viewName)
        {

        }
    }

    public class LinData
    {
        public LinConfigSignal[] Data { get; } = new LinConfigSignal[8];

        public override string ToString()
        {
            return string.Join(" ", Data.Select(x => $"{x.OriginValue:X2}"));
        }
    }
}
