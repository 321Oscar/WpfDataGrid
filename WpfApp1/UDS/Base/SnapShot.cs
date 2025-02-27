using System;
using System.Collections.Generic;

namespace ERad5TestGUI.UDS
{
    [Serializable]
    public class SnapShot
    {
        public byte Index { get; set; }
        public List<SnapDIDInfo> SnapDIDInfos { get;set;}
    }
}
