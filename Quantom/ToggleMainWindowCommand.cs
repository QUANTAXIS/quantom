using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Quantom
{
    class ToggleMainWindowCommand : ICommand
    {        
        public void Execute(object parameter)
        {
            if (App.Current.MainWindow.IsVisible)
                App.Current.MainWindow.Hide();
            else
                App.Current.MainWindow.Show();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
