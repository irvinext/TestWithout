using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Utils.Forms.ConfigPicker;
using System.Configuration;

namespace MonkeyCancel
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string configFileName = ConfigPicker.InjectAppSettingsConfig("config");
            ConfigurationManager.AppSettings["configFilename"] = configFileName; //TTPROJECTS-1

            MonkeyCancelFrm guiInstance = new MonkeyCancelFrm();
            Manager manager = new Manager(guiInstance);

            Application.Run(guiInstance);
        }
    }
}
