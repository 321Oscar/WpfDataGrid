namespace WpfApp1.Models
{
    public class DiscreteOutputSignal : DiscreteSignal
    {
        public DiscreteOutputSignal()
        {
            //PropertyChanged += DiscreteOutputSignal_PropertyChanged;
        }

        private void DiscreteOutputSignal_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(RealValue))
            {
                OnPropertyChanged(nameof(Pin_High));
                OnPropertyChanged(nameof(Pin_Low));
            }
        }

        public double? TempValue
        {
            get;
            set;
        }
        /// <summary>
        /// 如果是同步的，则把数据存在Temp中
        /// </summary>
        public bool Sync { get; set; }

        public bool Pin_High
        {
            get
            {
                if (Sync && TempValue.HasValue)
                    return TempValue.Value == 1;
                return OriginValue == 1;
            }
            set
            {
                if (Sync)
                {
                    TempValue = value ? 1 : 0;
                    OnPropertyChanged(nameof(Pin_High));
                    OnPropertyChanged(nameof(Pin_Low));
                }
                else
                {
                    OriginValue = value ? 1 : 0;
                }
            }
        }
        public bool Pin_Low
        {
            get 
            {
                if (Sync && TempValue.HasValue)
                    return TempValue.Value == 0; 
                return OriginValue == 0; 
            }
            set
            {
                if (Sync)
                {
                    TempValue = value ? 0 : 1;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Pin_High));
                }
                else
                {
                    OriginValue = value ? 0 : 1;
                }
            }
        }

        public override void OnRealValueChanged()
        {
            base.OnRealValueChanged();
            OnPropertyChanged(nameof(Pin_High));
            OnPropertyChanged(nameof(Pin_Low));
        }

        public void UpdateRealValue()
        {
            if (TempValue.HasValue)
            {
                OriginValue = TempValue.Value;
                TempValue = null;
            }
        }
    }
}
