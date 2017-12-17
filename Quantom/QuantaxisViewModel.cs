using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantom
{
    public class QuantaxisViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string MongoIP { get; set; }
        public string MongoPort { get; set; }
        public string MongoDBName { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public QuantaxisViewModel()
        {
            this.MongoIP = "127.0.0.1";
            this.MongoPort = "27017";
            this.MongoDBName = "quantaxis";
        }

    }
}
