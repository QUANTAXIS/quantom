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
using Newtonsoft.Json;
using Quantom.Commands;

namespace Quantom
{
    public class MainViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        string AppDir = System.AppDomain.CurrentDomain.BaseDirectory;
        string QuantaxisDir;
        private bool Installed;
        private bool Loading = false;
        public bool NotLoading
        {
            get { return !Loading; }
            set { Loading = !value; }
        }
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
            Installed = Directory.Exists(QuantaxisDir);
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

        private async void __DownloadOrUpdate(object obj)
        {
            await Task.Run(() =>
            {
                if (!CheckPythonVersion()) return;
                if (!CheckNodeVersion()) return;
                if (!CheckGitVersion()) return;
                Loading = true;
                OnPropertyChanged("NotLoading");
                if (Installed)
                {
                    PullQuantaxis();
                }
                else
                {
                    CloneQuantaxis();
                }
                InstallQuantaxis();
                Loading = false;
                OnPropertyChanged("NotLoading");
            });
        }
        private bool CheckPythonVersion()
        {
            ProcessStartInfo start = new ProcessStartInfo()
            {
                FileName = @"cmd.exe", // Specify exe name.
                Arguments = "/c python --version",
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (Process pro = Process.Start(start))
            {
                using (StreamReader reader = pro.StandardError)
                {
                    string result = reader.ReadToEnd();
                    if (result.Length > 10 && result.Substring(0, 8) == "'python'")
                    {
                        MessageBox.Show("Python 3 is not installed. Please install Python Anaconda");
                        Process.Start("https://www.anaconda.com/download");
                        return false;
                    }
                    return true;
                }
            }
        }
        private bool CheckNodeVersion()
        {
            ProcessStartInfo start = new ProcessStartInfo()
            {
                FileName = @"cmd.exe", // Specify exe name.
                Arguments = "/c node --version",
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (Process pro = Process.Start(start))
            {
                using (StreamReader reader = pro.StandardError)
                {
                    string result = reader.ReadToEnd();
                    if (result.Length > 10 && result.Substring(0, 6) == "'node'")
                    {
                        MessageBox.Show("Node.js is not installed. Please install Node.js");
                        Process.Start("https://nodejs.org/");
                        return false;
                    }
                    return true;
                }
            }
        }
        private bool CheckGitVersion()
        {
            ProcessStartInfo start = new ProcessStartInfo()
            {
                FileName = @"cmd.exe", // Specify exe name.
                Arguments = "/c git --version",
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (Process pro = Process.Start(start))
            {
                using (StreamReader reader = pro.StandardError)
                {
                    string result = reader.ReadToEnd();
                    if (result.Length > 10 && result.Substring(0, 5) == "'git'")
                    {
                        MessageBox.Show("Git is not installed. Please install Git");
                        Process.Start("https://git-scm.com/download");
                        return false;
                    }
                    return true;
                }
            }
        }
        private bool CloneQuantaxis()
        {
            ProcessStartInfo start = new ProcessStartInfo()
            {
                FileName = @"cmd.exe", // Specify exe name.
                Arguments = "/c git clone https://github.com/yutiansut/QUANTAXIS.git quantaxis --depth 1",
                WorkingDirectory = AppDir,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (Process pro = Process.Start(start))
            {
                using (StreamReader reader = pro.StandardError)
                {
                    string result = reader.ReadToEnd();
                    if (result.Substring(0, 7) != "Cloning")
                    {
                        MessageBox.Show("Git clone failed");
                        return false;
                    }
                    return true;
                }
            }
        }
        private void PullQuantaxis()
        {
            ProcessStartInfo start = new ProcessStartInfo()
            {
                FileName = @"cmd.exe", // Specify exe name.
                Arguments = "/c git pull",
                WorkingDirectory = QuantaxisDir,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (Process pro = Process.Start(start))
            {
                using (StreamReader reader = pro.StandardError)
                {
                    string result = reader.ReadToEnd();
                    if (result.Length > 1) MessageBox.Show("Git pull update failed");                       
                }
            }
        }

        private void InstallQuantaxis()
        {
            ProcessStartInfo start = new ProcessStartInfo()
            {
                FileName = @"cmd.exe", // Specify exe name.
                Arguments = "/c pip install -r requirements.txt -i https://pypi.doubanio.com/simple && pip install tushare==0.8.7 && pip install -e .",
                WorkingDirectory = QuantaxisDir,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (Process pro = Process.Start(start))
            {
                using (StreamReader reader = pro.StandardError)
                {
                    string result = reader.ReadToEnd();
                    if (result.Length < 0) MessageBox.Show("Pip install Quantaxis failed");
                }
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
