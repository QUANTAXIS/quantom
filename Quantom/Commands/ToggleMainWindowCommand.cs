using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Quantom.Commands
{
    class ToggleMainWindowCommand : ICommand
    {        
        public void Execute(object parameter)
        {
            if (Application.Current.MainWindow.IsVisible)
                Application.Current.MainWindow.Hide();
            else
                Application.Current.MainWindow.Show();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
