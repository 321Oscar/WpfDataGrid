using CommunityToolkit.Mvvm.ComponentModel;
using ERad5TestGUI.Models;
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
    public class ClearSignalDataCommand : CommandBase
    {
        //public override bool CanExecute(object parameter)
        //{
        //    return parameter != null;
        //}

        public override void Execute(object parameter)
        {
            if (parameter is Interfaces.IClearData clearData)
            {
                clearData.ClearData();
            }
            else if (parameter is IEnumerable<SignalBase> signals)
            {
                foreach (var signal in signals)
                {
                    signal.Clear();
                }
            }
            else
            {
                var values = (object[])parameter;

                if (values == null)
                    return;

                //first is SignalStore
                Stores.SignalStore signalStore = (Stores.SignalStore)values[0];
                //ViewName string
                string viewName = values[1].ToString();
                //Type
                if (values.Length == 3)
                {
                    Type t = (Type)values[2];
                    var method = signalStore.GetType().GetMethod("GetSignals", new Type[] { typeof(string), typeof(bool?) }).MakeGenericMethod(t);
                    var test = method.Invoke(signalStore, new object[] { viewName, false });
                    if (test is IEnumerable<SignalBase> enumerable)
                    {
                        foreach (var item in enumerable)
                        {
                            item.Clear();
                        }
                    }
                }
                else
                {
                    foreach (var item in signalStore.GetSignals<SignalBase>(viewName, false))
                    {
                        item.Clear();
                    }
                }

            }
        }
    }
}
