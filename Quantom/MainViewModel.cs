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
        string PATH = 
            Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) + ";" + 
            Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
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
        private readonly ICommand _StartBackend;
        private readonly ICommand _StopBackend;
        private readonly ICommand _StartFrontend;
        private readonly ICommand _StopFrontend;

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
            _OpenSettingWindow = new RelayCommand(__OpenSettingWindow, Can);
            _DownloadOrUpdate = new RelayCommand(__DownloadOrUpdate, Can);
            _StartBackend = new RelayCommand(__StartBackend, Can);
            _StopBackend = new RelayCommand(__StopBackend, Can);
            _StartFrontend = new RelayCommand(__StartFrontend, Can);
            _StopFrontend = new RelayCommand(__StopFrontend, Can);
        }        
        public string MongoIP {
            get { return settings.MongoIP; }
            set { settings.MongoIP = value; }
        }
        public string MongoPort { get { return settings.MongoPort; } }
        public string MongoDBName { get { return settings.MongoDBName; } }
        public string WorkerLabel { get { return "启动QA Worker"; } }
        public string BackendLabel
        {
            get
            {
                if (backendTask.IsCompleted)
                    return "启动后端服务";
                else
                    return "关闭后端服务";
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
        
        public ICommand OpenSettingWindow
        {
            get { return _OpenSettingWindow; }
        }
        public ICommand ToggleWorker
        {
            get { return _DownloadOrUpdate; }
        }
        public ICommand ToggleBackend
        {
            get
            {
                if (backendTask.IsCompleted)
                    return _StartBackend;
                else
                    return _StopBackend;
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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            // Collect the sort command output.
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                _output += outLine.Data + Environment.NewLine;
                OnPropertyChanged("Output");
            }
        }

        public bool Can(object obj) { return true; }

        private void __OpenSettingWindow(object obj)
        {
            Window SettingWindow = new SettingWindow();
            SettingWindow.Closed += FreshSetting;
            SettingWindow.Show();
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
       
        private void __StartBackend(object obj)
        {
            if (!File.Exists(BackendFile))
            {
                MessageBox.Show("Backend 执行文件丢失");
                return;
            }
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
                backendProcess.OutputDataReceived += 
                    new DataReceivedEventHandler(OutputHandler);
                backendProcess.BeginOutputReadLine();
                backendProcess.WaitForExit();
            });
            OnPropertyChanged("BackendLabel");
            OnPropertyChanged("ToggleBackend");
        }
        
        public void __StopBackend(object obj)
        {
            backendProcess.Kill();
            backendProcess.Dispose();
            backendTask.Wait();
            OnPropertyChanged("BackendLabel");
            OnPropertyChanged("ToggleBackend");
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
