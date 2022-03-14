using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Verint_Chat_API_Capture
{
    
    public partial class Text_Capture_Service : ServiceBase
    {
        private Timer servicetimer = null;
        public Text_Capture_Service()
        {
            InitializeComponent();

        }
        
        protected override void OnStart(string[] args)
        {
            servicetimer = new Timer();
            this.servicetimer.Interval = 30000;
            this.servicetimer.Elapsed += new System.Timers.ElapsedEventHandler(this.servicetimer_event);

        }

        public void servicetimer_event(object sender, ElapsedEventArgs e)
        {
            Library.logerror("Started the service");

        }

        protected override void OnStop()
        {
            servicetimer.Enabled = false;
            Library.logerror("Ended/Stopped the service");
        }
    }
}
