using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CPP_EP.Execute {
    class Xmake {
        private Process ExecuteProcess;
        public Xmake (string labsPath, string xmakePath, int lab) {
            ExecuteProcess = new Process ();
            ExecuteProcess.StartInfo.WorkingDirectory = labsPath;
            ExecuteProcess.StartInfo.FileName = xmakePath;
            ExecuteProcess.StartInfo.Arguments = "build lab" + lab;
            ExecuteProcess.StartInfo.UseShellExecute = false;
            ExecuteProcess.StartInfo.RedirectStandardOutput = true;
            ExecuteProcess.StartInfo.RedirectStandardInput = true;
            ExecuteProcess.StartInfo.RedirectStandardError = true;
            ExecuteProcess.StartInfo.CreateNoWindow = true;
            ExecuteProcess.Start ();
        }
        public void Build(TextBox t) {
            t.Clear ();
            string s;
            while ((s = ExecuteProcess.StandardOutput.ReadLine ()) != null) {
                t.AppendText (s + "\n");
            }
        }
    }
}
