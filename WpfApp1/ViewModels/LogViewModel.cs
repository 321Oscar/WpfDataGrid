using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERad5TestGUI.Services;

namespace ERad5TestGUI.ViewModels
{
    public class LogViewModel: CommunityToolkit.Mvvm.ComponentModel.ObservableRecipient
    {
        private readonly Services.LogService logService;

        public LogViewModel(LogService logService)
        {
            this.logService = logService;
        }
    }
}
