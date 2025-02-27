namespace ERad5TestGUI.Models
{
    public interface ISyncValue
    {
        double? TempValue { get; set; }
        bool Sync { get; set; }

        void UpdateRealValue();
    }
}