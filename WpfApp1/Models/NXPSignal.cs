using ERad5TestGUI.Stores;
using System.Xml.Serialization;

namespace ERad5TestGUI.Models
{
    public class NXPSignal : SignalBase
    {
        public NXPSignal()
        {

        }

        public NXPSignal(Stores.Signal signal, string viewName) : base(signal, viewName)
        {

        }
    }

    public class NXPInputSignal : TransFormSignalBase, ITransform2
    {
        private string value2 = "NAN";

        public NXPInputSignal()
        {

        }

        public NXPInputSignal(Stores.Signal signal, string viewName) : base(signal, viewName)
        {
           
        }
        [XmlIgnore]
        public string Value2 { get => value2; set => SetProperty(ref value2, value); }
        public int Transform2Type { get; set; }

        public string TableName { get; set; }

        public double TransForm2Factor { get; set; }

        public double TransForm2Offset { get; set; }

        public double TransForm2(double oldVal)
        {
            if (Transform2Type == 0)
            {
                return oldVal * TransForm2Factor + TransForm2Offset;
            }
            else
            {
                //get table from tableName
                return Stores.ValueTable.ConvertByTable(TableName, oldVal);
            }
        }

        public override void UpdateFormDBC(Signal signal)
        {
            base.UpdateFormDBC(signal);
            Transform2Type = (int)signal.Comment.GetCommenDoubleByKey("Conversion_mode", 0);
            if (Transform2Type == 0)
            {
                TransForm2Factor = signal.Comment.GetCommenDoubleByKey("Factor", 1);
                TransForm2Offset = signal.Comment.GetCommenDoubleByKey("Offset", 0);
            }
            else
            {
                TableName = signal.Comment.GetCommentByKey("Table");
            }
        }

        protected override double TransForm(double oldVal)
        {
            if (TransForm2Factor == 1)
                return oldVal;

            return oldVal;// * 5 / 4096;
        }

        protected override void OnOriginValueChaned(double originValue, bool changed)
        {
            base.OnOriginValueChaned(originValue, changed);
            if (changed)
            {
                var realValue = TransForm(originValue);
                Value2 = TransForm2(realValue).ToString(Format);
            }
        }
    }
}
