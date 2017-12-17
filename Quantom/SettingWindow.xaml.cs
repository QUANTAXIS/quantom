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
using System.Windows.Shapes;

namespace Quantom
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
        }
        
        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        //private void Save(object sender, RoutedEventArgs e)
        //{
        //    SettingViewModel clickedSettingWindow = ((Button)sender).DataContext as SettingViewModel;            
        //    clickedSettingWindow.Save();
        //    this.Close();
        //}        
    }
}
