using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LibGit2Sharp;
using Newtonsoft.Json;

namespace Quantom
{
    public class MainViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool Installed = Repository.IsValid("quantaxis");
        private Settings settings = new Settings();
        public string MongoIP { get { return settings.MongoIP; } set { settings.MongoIP = value; } }
        public string MongoPort { get { return settings.MongoPort; } }
        public string MongoDBName { get { return settings.MongoDBName; } }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public MainViewModel()
        {
            try
            {
                settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
            }
            catch (FileNotFoundException)
            {
                File.WriteAllText("settings.json", JsonConvert.SerializeObject(settings));
            }
            _OpenSettingWindow = new RelayCommand(__OpenSettingWindow, CanSetSettings);
            _DownloadOrUpdate = new RelayCommand(__DownloadOrUpdate, CanSetSettings);
            _StartQuantaxis = new RelayCommand(__StartQuantaxis, CanSetSettings);
        }

        public ICommand OpenSettingWindow
        {
            get { return _OpenSettingWindow; }
        }
        public ICommand DownloadOrUpdate
        {
            get { return _DownloadOrUpdate; }
        }
        public ICommand StartQuantaxis
        {
            get { return _StartQuantaxis; }
        }
        private readonly ICommand _OpenSettingWindow;
        private readonly ICommand _DownloadOrUpdate;
        private readonly ICommand _StartQuantaxis;
        private void __OpenSettingWindow(object obj)
        {
            Window SettingWindow = new SettingWindow();
            SettingWindow.Closed += FreshSetting;
            SettingWindow.Show();
        }
        public bool CanSetSettings(object obj)
        {
            return true;
        }

        private void __DownloadOrUpdate(object obj)
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

        private void __StartQuantaxis(object obj)
        {
            MessageBox.Show("Start Quantaxis");
            ProcessStartInfo info = new ProcessStartInfo("npm.exe");
            info.WorkingDirectory = "quantaxis/QUANTAXIS/QUANTAXIS_Webkit";
            info.Arguments = "run all";
            Process.Start(info);
        }
        public void FreshSetting(object sender, System.EventArgs e)
        {
            settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
            OnPropertyChanged("MongoIP");
            OnPropertyChanged("MongoPort");
            OnPropertyChanged("MongoDBName");
        }
    }    
}
