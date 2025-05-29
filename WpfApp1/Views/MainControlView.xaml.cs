using System;
using System.Collections.Generic;
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

namespace ERad5TestGUI.Views
{
    /// <summary>
    /// MainControlView.xaml 的交互逻辑
    /// </summary>
    public partial class MainControlView : UserControl
    {
        public MainControlView()
        {
            InitializeComponent();

            this.DataContextChanged += MainControlView_DataContextChanged;
        }

        private void MainControlView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is ViewModels.MainViewModel viewModel)
            {
                viewModel.LogService.UiLogAppender.LogControl = this.logListBox;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            
        }
        //定义ComboBox选项的类，存放Name和Value
        public class CategoryInfo
        {
            public string Name
            {
                get;
                set;
            }
            public string Value
            {
                get;
                set;
            }

        }
        private void ChangeLang_Click(object sender, RoutedEventArgs e)
        {
            object selectedName = cbbLang.SelectedValue;
            if (selectedName != null)
            {
                string langName = selectedName.ToString();
                //英语的语言文件名为:DefaultLanguage,所有这里要转换一下
                if (langName == "en_US")
                    langName = "DefaultLanguage";
                //根据本地语言来进行本地化,不过这里上不到
                //CultureInfo currentCultureInfo = CultureInfo.CurrentCulture;

                ResourceDictionary langRd = null;
                try
                {
                    //根据名字载入语言文件
                    langRd = Application.LoadComponent(new Uri(@"lang\" + langName + ".xaml", UriKind.Relative)) as ResourceDictionary;
                }
                catch (Exception e2)
                {
                    MessageBox.Show(e2.Message);
                }

                if (langRd != null)
                {
                    //如果已使用其他语言,先清空
                    if (Application.Current.Resources.MergedDictionaries.Count > 0)
                    {
                        //Application.Current..Resources.MergedDictionaries.RemoveAt(0);
                        Application.Current.Resources.MergedDictionaries.RemoveAt(0);
                    }
                    Application.Current.Resources.MergedDictionaries.Insert(0, langRd);
                }
            }
            else
                MessageBox.Show("Please selected one Language first.");
        }

        private void cbbLang_Loaded(object sender, RoutedEventArgs e)
        {
            List<CategoryInfo> categoryList = new List<CategoryInfo>
            {
                new CategoryInfo() { Name = "English", Value = "en_US" },
                new CategoryInfo() { Name = "中文", Value = "zh_CN" }
            };

            cbbLang.ItemsSource = categoryList;//绑定数据，真正的赋值
            cbbLang.DisplayMemberPath = "Name";//指定显示的内容
            cbbLang.SelectedValuePath = "Value";//指定选中后的能够获取到的内容

            cbbLang.SelectedIndex = 0;
        }
    }
}
