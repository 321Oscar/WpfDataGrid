using System;
using System.Collections.Generic;

namespace WpfApp1.UDS
{
    [Serializable]
    public class UDSExtendedData
    {
        public byte RecordNum { get; set; }

        public List<SnapDIDInfo> SnapDIDInfos { get; set; }
    }
}
