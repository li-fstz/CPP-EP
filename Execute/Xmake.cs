using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CPP_EP.Execute {
    class Xmake {
        private string LabsPath, XmakePath;
        private Action<string> PrintXmakeLog;
        public Xmake (string labsPath, string xmakePath, Action<string> printXmakeLog) {
            LabsPath = labsPath;
            XmakePath = xmakePath;
            PrintXmakeLog = printXmakeLog;
        }
        public void build (int lab, Action<bool> AfterBuild) {
            Util.ThreadRun (() => {
                Process ExecuteProcess = new Process ();
                ExecuteProcess.StartInfo.WorkingDirectory = LabsPath;
                ExecuteProcess.StartInfo.FileName = XmakePath;
                ExecuteProcess.StartInfo.Arguments = "build lab" + lab;
                ExecuteProcess.StartInfo.UseShellExecute = false;
                ExecuteProcess.StartInfo.RedirectStandardOutput = true;
                ExecuteProcess.StartInfo.RedirectStandardInput = true;
                ExecuteProcess.StartInfo.RedirectStandardError = true;
                ExecuteProcess.StartInfo.CreateNoWindow = true;
                ExecuteProcess.Start ();
                string s;
                bool r = false;
                while ((s = ExecuteProcess.StandardOutput.ReadLine ()) != null) {
                    //Dispatcher.Invoke(PrintXmakeLog, s);
                    PrintXmakeLog (s);
                    //PrintXmakeLog(s);
                    r = s == "[100%]: build ok!";
                }
                AfterBuild (r);
            });
        }
    }
}
