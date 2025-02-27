using System;
using System.Collections.Generic;

namespace ERad5TestGUI.UDS
{
    [Serializable]
    public class UDSExtendedData
    {
        public byte RecordNum { get; set; }

        public List<SnapDIDInfo> SnapDIDInfos { get; set; }
    }
}
