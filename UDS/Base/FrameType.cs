namespace WpfApp1.UDS
{
    public enum FrameType
    {
        SingleFrame = 0,
        FirstFrame = 0x10,
        ContinueFrame = 0x20,
        FlowControlFrame = 0x30
    }
}