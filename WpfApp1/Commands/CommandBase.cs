using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ERad5TestGUI.Commands
{
    public abstract class CommandBase : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public virtual bool CanExecute(object parameter)
        {
            return true;
        }

        public abstract void Execute(object parameter);

        protected void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }

    public delegate ICommand CreateCommand<TViewModel>(TViewModel viewModel) where TViewModel : ObservableObject;

    public static class ApplicationCommands
    {
        // 定义一个新的 RoutedUICommand
        public static readonly RoutedUICommand CopyNameCommand = new RoutedUICommand(
            "Copy Name", // 命令文本
            "CopyNameCommand",  // 命令名称
            typeof(ApplicationCommands), // 类型
            new InputGestureCollection() // 可选：输入手势集合
            {
            new KeyGesture(Key.C, ModifierKeys.Control) // Ctrl+C
            });
    }

}
