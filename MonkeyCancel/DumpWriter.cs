using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace MonkeyCancel
{
    public class DumpWriter
    {
        string m_filePath;
        object m_synch = new object();

        public DumpWriter()
        {
            try
            {
                var folder = AppDomain.CurrentDomain.BaseDirectory + "Logs\\";
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                m_filePath = folder + DateTime.Now.ToString("yyyyMMdd-hhmmss-ffff") + ".log";
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error create dump folder. " + ex.ToString());
            }
        }


        public void WriteString(string value)
        {
            try
            {
                lock (m_synch)
                {
                    File.AppendAllText(m_filePath, value + "\n\n");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error write dump file '{0}'. {1}", m_filePath, ex.ToString());
            }
        }

    }
}
