using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace GeoCodingTest.Class
{
    public class InterfaceLog
    {
        private string path;

        public InterfaceLog(string iPath)
        {
            this.path = iPath;
            if (!Directory.Exists(this.path))
            {
                Directory.CreateDirectory(this.path);
            }
        }

        public void WriteLog(string logMsg, bool newLine = false)
        {
            DateTime now = DateTime.Now;
            using (StreamWriter writer = File.AppendText(string.Format(@"{0}\{1:yyyyMMdd}.txt", this.path, now)))
            {
                writer.WriteLine(string.Format("{0:dd/MM/yyyy HH:mm} : {1}", now, logMsg));
                if (newLine)
                {
                    writer.WriteLine("");
                }
            }
        }

        public void WriteLog(string tableName, int total, int success, int fail, bool newLine = false)
        {
            DateTime now = DateTime.Now;
            using (StreamWriter writer = File.AppendText(string.Format(@"{0}\{1:yyyyMMdd}.txt", this.path, now)))
            {
                writer.WriteLine(string.Format("{0:dd/MM/yyyy HH:mm} : {1}, Total = {2}, Success = {3}, Fail = {4}", new object[] { now, tableName, total, success, fail }));
                if (newLine)
                {
                    writer.WriteLine("");
                }
            }
        }
    }
}