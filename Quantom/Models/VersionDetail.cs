using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Quantom.Models
{
    class VersionDetail
    {
        public string version;
        public int MajorVersion
        {
            get
            {
                try { return Int32.Parse(version.Split('.')[0]); }
                catch
                {
                    MessageBox.Show("invalid remote update repository version format.");
                    return 0;
                }
            }
        }
        public int MinorVersion
        {
            get
            {
                try { return Int32.Parse(version.Split('.')[1]); }
                catch
                {
                    MessageBox.Show("invalid remote update repository version format.");
                    return 0;
                }
            }
        }
        public int PatchVersion
        {
            get
            {
                try { return Int32.Parse(version.Split('.')[2]); }
                catch
                {
                    MessageBox.Show("invalid remote update repository version format.");
                    return 0;
                }
            }
        }
    }
}
