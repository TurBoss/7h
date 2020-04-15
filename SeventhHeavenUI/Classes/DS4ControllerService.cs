using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;

namespace SeventhHeaven.Classes
{
    internal class DS4ControllerService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        internal ControlService RootHub { get; set; }

        public DS4ControllerService()
        {
            RootHub = new ControlService();
            RootHub.Debug += RootHub_LogDebug;

            DS4Windows.Global.UseExclusiveMode = true;
            RootHub.Start(showlog: true);
        }

        public void StopService()
        {
            RootHub.Stop(showlog: true);
            RootHub.Debug -= RootHub_LogDebug;
        }


        private void RootHub_LogDebug(object sender, DebugEventArgs e)
        {
            if (e.Warning)
            {
                Logger.Warn(e.Data);
            }
            else
            {
                Logger.Info(e.Data);
            }
        }
    }
}
