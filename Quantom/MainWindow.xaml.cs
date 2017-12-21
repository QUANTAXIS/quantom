using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
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
using Newtonsoft.Json;

namespace Quantom
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            Task.Run(async () => 
            {            
                try {
                    bool dated = await IsOutDated();
                    if (dated)
                    {
                        MessageBox.Show("An update is found");
                        Process.Start("https://github.com/hardywu/quantom/releases");
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
             });
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            this.Hide();
        }

        async public Task<bool> IsOutDated()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("https://raw.githubusercontent.com/hardywu/quantom/master/latestVersion.json");
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Reflection.AssemblyName assemblyName = assembly.GetName();
            Version assemblyVersion = assemblyName.Version;
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                Models.VersionDetail version;
                version = JsonConvert.DeserializeObject<Models.VersionDetail>(content);
                return assemblyVersion < new Version(version.version);
            }
            else
            {
                throw new Exception("Cannot connect to the latest repository. Please check your network status.");
            }
        }

        async public void CheckUpdate()
        {
            try
            {
                bool dated = await IsOutDated();
                if (dated)
                {
                    MessageBox.Show("An update is found");
                    Process.Start("https://github.com/hardywu/quantom/releases");
                }
                else { MessageBox.Show("Quantom is up to date"); }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
