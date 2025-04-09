using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERad5TestGUI.Services
{
    public interface IDialogService
    {
        void ShowDialog(string name, Action<string> callback);
        void ShowDialog<ViewModel>(Action<string> callback);
    }

    public class DialogService : IDialogService
    {
        public void ShowDialog(string name, Action<string> callback)
        {
            CreateView(name, callback);
        }

        private static void CreateView(string name, Action<string> callback)
        {
            var dialog = new Views.DialogView();

            EventHandler closeEventHandler = null;
            closeEventHandler = (s, e) =>
            {
                callback(dialog.DialogResult.ToString());
                dialog.Closed -= closeEventHandler;
            };
            dialog.Closed += closeEventHandler;

            var type = Type.GetType($"ERad5TestGUI.Views.{name}");

            dialog.Content = Activator.CreateInstance(type);

            dialog.ShowDialog();
        }

        public void ShowDialog<ViewModel>(Action<string> callback)
        {
            //throw new NotImplementedException();
        }
    }
}
