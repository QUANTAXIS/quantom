using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Quantom
{
    public class SettingViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private Settings settings = new Settings();
        public string MongoIP
        {
            get { return settings.MongoIP; }
            set { settings.MongoIP = value; }
        }
        public string MongoPort
        {
            get { return settings.MongoPort; }
            set { settings.MongoPort = value; }
        }
        public string MongoDBName
        {
            get { return settings.MongoDBName; }
            set { settings.MongoDBName = value; }
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public SettingViewModel()
        {
            try
            {
                settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
            }
            catch (FileNotFoundException)
            {
                File.WriteAllText("settings.json", JsonConvert.SerializeObject(settings));
            }
            _Save = new RelayCommand(__Save, CanSave);
        }
        private readonly ICommand _Save;
        public ICommand Save
        {
            get { return _Save;  }
        }

        public bool CanSave(object obj)
        {
            return true;
        }
        public void __Save(object obj)
        {
            try
            {
                File.WriteAllText("settings.json", JsonConvert.SerializeObject(settings));
                ((Window)obj).Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
