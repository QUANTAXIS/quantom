using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Quantom.Commands
{
    class CheckUpdate : ICommand
    {
        public void Execute(object parameter)
        {
            MainWindow main = (MainWindow) Application.Current.MainWindow;
            main.CheckUpdate();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
