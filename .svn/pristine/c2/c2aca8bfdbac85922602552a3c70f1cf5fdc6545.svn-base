using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace NSN.GNOCN.Tools.AWE.NSS.DCM
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new AWE_NSS_DCM() 
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
