using LibGit2Sharp;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Quantom
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool Installed = Repository.IsValid("quantaxis");
        //private json setting = { };

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            this.Hide();
        }

        private void Download_Or_Update(object sender, RoutedEventArgs e)
        {
            if (Installed)
            {
                Repository repo = new Repository("quantaxis");
                Commands.Pull(repo, new Signature("quantom", "quantom@avaloron.com", new DateTimeOffset(DateTime.Now)), new PullOptions());
            }
            else
            {
                Repository.Clone("https://github.com/yutiansut/quantaxis", "quantaxis");
                Installed = Repository.IsValid("quantaxis");
            }                
        }
    }
}
