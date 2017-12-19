using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Quantom
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class Application : System.Windows.Application
    {
        private TaskbarIcon notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
            notifyIcon = (TaskbarIcon)Resources["TrayIcon"];
            base.OnStartup(e);
        }
    }
}
