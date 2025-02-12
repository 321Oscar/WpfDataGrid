using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataGridDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Stopwatch watch;
        public MainWindow()
        {
            InitializeComponent();
            watch = new Stopwatch();
            watch.Restart();

            view = new MainWindowView();
            view.DgList = new System.Collections.ObjectModel.ObservableCollection<DgModel>();
            this.DataContext = view;
            InitData();
        }

        MainWindowView view;
        private void InitData()
        {
            var list = new List<DgModel>();
            Random rd = new Random();
            for (int i = 0; i < 100; i++)
            {
                var item = new DgModel();
                item.F1 = rd.Next().ToString();
                item.F2 = rd.Next().ToString();
                item.F3 = rd.Next().ToString();
                item.F4 = rd.Next().ToString();
                item.F5 = rd.Next().ToString();
                item.F6 = rd.Next().ToString();
                item.F7 = rd.Next().ToString();
                item.F8 = rd.Next().ToString();
                item.F9 = rd.Next().ToString();
                item.F10 = rd.Next().ToString();
                list.Add(item);
            }
            Stopwatch ms = new Stopwatch();
            ms.Restart();
            view.DgList = new System.Collections.ObjectModel.ObservableCollection<DgModel>(list);
            ms.Stop();
            view.DgMs = ms.ElapsedMilliseconds.ToString();
            ms.Restart();
            view.LvList = new System.Collections.ObjectModel.ObservableCollection<DgModel>(list);
            ms.Stop();
            view.LvMs = ms.ElapsedMilliseconds.ToString();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch ms = new Stopwatch();
            ms.Restart();
            InitData();
            ms.Stop();
            Console.WriteLine(ms.ElapsedMilliseconds.ToString());
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            watch.Stop();
            Console.WriteLine($"{DateTime.Now:HH:mm:ss fff} datagrid loaded take {watch.ElapsedMilliseconds} ms");

            //this.datagrid.Loaded -= datagrid_Loaded;
        }
    }

    public class MainWindowView : System.ComponentModel.INotifyPropertyChanged
    {
        private ObservableCollection<DgModel> dgList;
        private string dgMs;
        private ObservableCollection<DgModel> lvList;
        private string lvMs;

        public System.Collections.ObjectModel.ObservableCollection<DgModel> DgList { get => dgList; 
            set =>SetProperty(ref dgList , value); }
        public string DgMs { get => dgMs; 
            set => SetProperty(ref dgMs , value); }
        public System.Collections.ObjectModel.ObservableCollection<DgModel> LvList { get => lvList; 
            set => SetProperty(ref lvList , value); }
        public string LvMs { get => lvMs; 
            set => SetProperty(ref lvMs , value); }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {

            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            try
            {
                //PropertyChanged?.BeginInvoke(this,args,null,null);
                this.PropertyChanged?.Invoke(this, args);
            }
            catch (System.Exception)
            {
            }
        }
    }

    public class DgModel
    {
        public string F1 { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }
        public string F4 { get; set; }
        public string F5 { get; set; }
        public string F6 { get; set; }
        public string F7 { get; set; }
        public string F8 { get; set; }
        public string F9 { get; set; }
        public string F10 { get; set; }
    }
}
