using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
        string AppDir = AppDomain.CurrentDomain.BaseDirectory;
        string PATH = 
            Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) + ";" + 
            Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
        private string QuantaxisDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "quantaxis"); 
        private string FrontendRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "frontend");
        private string BackendFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "quantaxisbackend.exe");
        private Models.HTTPServer frontendServer = new Models.HTTPServer(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "frontend"), IPAddress.Loopback.ToString(), 8080);
        private bool Loading = false;
        private Task backendTask = Task.CompletedTask;
        private Process backendProcess;
        private string _output;
        private Settings settings = new Settings();
        private readonly ICommand _OpenSettingWindow;
        private readonly ICommand _DownloadOrUpdate;
        private readonly ICommand _StartQuantaxis;
        private readonly ICommand _StopQuantaxis;
        private readonly ICommand _StartFrontend;
        private readonly ICommand _StopFrontend;

        public bool NotLoading
        {
            get { return !Loading; }
            set { Loading = !value; }
        }
        public string MongoIP {
            get { return settings.MongoIP; }
            set { settings.MongoIP = value; }
        }
        public string MongoPort { get { return settings.MongoPort; } }
        public string MongoDBName { get { return settings.MongoDBName; } }
        public string ToggleLabel
        {
            get
            {
                if (backendTask.IsCompleted)
                    return "启动";
                else
                    return "关闭";
            }
        }
        public string FrontendLabel
        {
            get
            {
                if (frontendServer.thread == null)
                    return "启动网页服务"; 
                else
                    return "关闭网页服务";
            }
        }

        public string Output
        {
            get { return _output; } set { _output = value; }
        }

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
            _StopQuantaxis = new RelayCommand(__StopQuantaxis, CanSetSettings);
            _StartFrontend = new RelayCommand(__StartFrontend, CanSetSettings);
            _StopFrontend = new RelayCommand(__StopFrontend, CanSetSettings);
        }

        public ICommand OpenSettingWindow
        {
            get { return _OpenSettingWindow; }
        }
        public ICommand ToggleWorker
        {
            get { return _DownloadOrUpdate; }
        }
        public ICommand ToggleQuantaxis
        {
            get
            {
                if (backendTask.IsCompleted)
                    return _StartQuantaxis;
                else
                    return _StopQuantaxis;
            }
        }
        public ICommand ToggleFrontend
        {
            get
            {
                if (frontendServer.thread == null)
                    return _StartFrontend;
                else
                    return _StopFrontend;
            }
        }
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
            _output = "";
            OnPropertyChanged("Output");
            await Task.Run(() =>
            {
                Loading = true;
                OnPropertyChanged("NotLoading");
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
            start.Environment.Add("PATH", PATH);
            using (Process pro = Process.Start(start))
            {
                using (StreamReader reader = pro.StandardError)
                {
                    string result = reader.ReadToEnd();
                    _output += result;
                    OnPropertyChanged("Output");
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
       
        private void __StartQuantaxis(object obj)
        {
            if (!File.Exists(BackendFile))
            {
                MessageBox.Show("Backend 执行文件丢失");
                return;
            }
            _output = "";
            OnPropertyChanged("Output");
            ProcessStartInfo info = new ProcessStartInfo(BackendFile)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            backendProcess = new Process
            {
                StartInfo = info
            };
            backendTask = Task.Run(() =>
            {
                backendProcess.Start();
                backendProcess.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                backendProcess.BeginOutputReadLine();
                backendProcess.WaitForExit();
            });
            OnPropertyChanged("ToggleLabel");
            OnPropertyChanged("ToggleQuantaxis");
        }
        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            // Collect the sort command output.
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                _output += Environment.NewLine + outLine.Data;
                OnPropertyChanged("Output");
            }
        }
        public void __StopQuantaxis(object obj)
        {
            backendProcess.Kill();
            backendProcess.Dispose();
            backendTask.Wait();
            OnPropertyChanged("ToggleLabel");
            OnPropertyChanged("ToggleQuantaxis");
        }
        public void FreshSetting(object sender, System.EventArgs e)
        {
            settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
            OnPropertyChanged("MongoIP");
            OnPropertyChanged("MongoPort");
            OnPropertyChanged("MongoDBName");
        }
        private void __StartFrontend(object obj)
        {
            if (!Directory.Exists(FrontendRoot))
            {
                MessageBox.Show("frontend folder is missing.");
                return;
            }           
            frontendServer.Start();
            OnPropertyChanged("FrontendLabel");
            OnPropertyChanged("ToggleFrontend");
        }
        public void __StopFrontend(object obj)
        {
            frontendServer.Stop();
            OnPropertyChanged("FrontendLabel");
            OnPropertyChanged("ToggleFrontend");
        }
    }        
}
