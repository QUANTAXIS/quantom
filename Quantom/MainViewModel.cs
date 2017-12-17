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
        string AppDir = System.AppDomain.CurrentDomain.BaseDirectory;
        string QuantaxisDir;
        private bool Installed;
        private Settings settings = new Settings();
        public string MongoIP { get { return settings.MongoIP; } set { settings.MongoIP = value; } }
        public string MongoPort { get { return settings.MongoPort; } }
        public string MongoDBName { get { return settings.MongoDBName; } }
        public string ToggleLabel
        {
            get
            {
                if (Running)
                    return "关闭";
                else
                    return "启动";
            }
        }
        public bool Running = false;
        private Process process;

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
            QuantaxisDir = Path.Combine(AppDir, "quantaxis");
            Installed = Repository.IsValid(QuantaxisDir);
            _OpenSettingWindow = new RelayCommand(__OpenSettingWindow, CanSetSettings);
            _DownloadOrUpdate = new RelayCommand(__DownloadOrUpdate, CanSetSettings);
            _StartQuantaxis = new RelayCommand(__StartQuantaxis, CanSetSettings);
            _StopQuantaxis = new RelayCommand(__StopQuantaxis, CanSetSettings);
        }

        public ICommand OpenSettingWindow
        {
            get { return _OpenSettingWindow; }
        }
        public ICommand DownloadOrUpdate
        {
            get { return _DownloadOrUpdate; }
        }
        public ICommand ToggleQuantaxis
        {
            get
            {
                if (Running)
                    return _StopQuantaxis;
                else
                    return _StartQuantaxis;
            }
        }
        private readonly ICommand _OpenSettingWindow;
        private readonly ICommand _DownloadOrUpdate;
        private readonly ICommand _StartQuantaxis;
        private readonly ICommand _StopQuantaxis;
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
                Repository repo = new Repository(QuantaxisDir);
                Commands.Pull(repo, new Signature("quantom", "quantom@avaloron.com", new DateTimeOffset(DateTime.Now)), new PullOptions());
            }
            else
            {
                Repository.Clone("https://github.com/yutiansut/quantaxis", QuantaxisDir);
                Installed = Repository.IsValid(QuantaxisDir);
            }
        }

        private void __StartQuantaxis(object obj)
        {
            ProcessStartInfo info = new ProcessStartInfo(@"cmd.exe");
            info.WorkingDirectory = Path.Combine(QuantaxisDir, "QUANTAXIS_Webkit");
            info.Arguments = "/c npm run all";
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            process = new Process
            {
                StartInfo = info,
                EnableRaisingEvents = true
            };
            process.Exited += (sender, args) =>
            {
                process.Dispose();
                Running = false;
                OnPropertyChanged("ToggleLabel");
                OnPropertyChanged("ToggleQuantaxis");
            };
            Running = true;
            OnPropertyChanged("ToggleLabel");
            OnPropertyChanged("ToggleQuantaxis");
            process.Start();
        }
        private void __StopQuantaxis(object obj)
        {
            process.Kill();
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
