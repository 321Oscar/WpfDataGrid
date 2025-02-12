using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Navigation.MVVM.WPF.Commands
{
    public abstract class CommandBase: ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public abstract void Execute(object parameter);

        public void RaiseCanExecuteChanged(object obj, EventArgs e)
        {
            CanExecuteChanged?.Invoke(obj, e);
        }
    }
    
}
