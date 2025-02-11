namespace WpfApp1.Models
{
    public interface ISyncValue
    {
        double? TempValue { get; set; }
        bool Sync { get; set; }

        void UpdateRealValue();
    }
}