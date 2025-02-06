using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Services;

namespace WpfApp1.ViewModels
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
