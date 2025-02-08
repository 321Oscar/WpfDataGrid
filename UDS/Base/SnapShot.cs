using System;
using System.Collections.Generic;

namespace WpfApp1.UDS
{
    [Serializable]
    public class SnapShot
    {
        public byte Index { get; set; }
        public List<SnapDIDInfo> SnapDIDInfos { get;set;}
    }
}
