using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace Verint_Chat_API_Capture
{
    [RunInstaller(true)]
    public partial class EF_HC_Text_Capture : System.Configuration.Install.Installer
    {
        public EF_HC_Text_Capture()
        {
            InitializeComponent();
        }

        private void serviceProcessInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {

        }
    }
}
