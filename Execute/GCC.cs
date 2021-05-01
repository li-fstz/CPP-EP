using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CPP_EP.Execute {

    internal class GCC {
        private readonly Process ExecuteProcess;
        public static Action<string> PrintLog { private get; set; }
        public static Action<bool> AfterBuild { private get; set; }
        private bool buildOk = true, update = false;
        private static readonly Dictionary<string, DateTime> lastTimeHash = new Dictionary<string, DateTime> ();
        private readonly List<string> objs = new List<string> ();
        /*
         * gcc .\src\rule.c -c -I .\inc\ -o build\obj\rule.o
         * gcc .\src\voidtable.c -c -I .\inc\ -o build\obj\voidtable.o
         * gcc .\lab1.c -c -I .\inc\ -o build\obj\lab1.o
         * gcc .\build\obj\lab1.o .\build\obj\rule.o .\build\obj\voidtable.o -o build\lab1.exe
         */

        public GCC () {
            if (!Directory.Exists ("build")) {
                Directory.CreateDirectory ("build");
            }
            if (!Directory.Exists ("build\\obj")) {
                Directory.CreateDirectory ("build\\obj");
            }
            ExecuteProcess = new Process ();
            ExecuteProcess.StartInfo.WorkingDirectory = System.AppDomain.CurrentDomain.BaseDirectory + "labs\\";
            ExecuteProcess.StartInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory + "MinGW\\bin\\gcc.exe";
            ExecuteProcess.StartInfo.UseShellExecute = false;
            ExecuteProcess.StartInfo.RedirectStandardOutput = true;
            ExecuteProcess.StartInfo.RedirectStandardInput = true;
            ExecuteProcess.StartInfo.RedirectStandardError = true;
            ExecuteProcess.StartInfo.CreateNoWindow = true;
        }

        public GCC Compile (string input, string output) {
            FileInfo file = new FileInfo ("labs\\" + input);
            objs.Add (System.AppDomain.CurrentDomain.BaseDirectory + output);
            if (!lastTimeHash.ContainsKey (input) || DateTime.Compare (lastTimeHash[input], file.LastWriteTime) != 0 || !File.Exists (output)) {
                ExecuteProcess.StartInfo.Arguments = string.Format ("-g -fexec-charset=GBK -c -I inc {0} -o \"{1}\"", input, System.AppDomain.CurrentDomain.BaseDirectory + output);
                Run ();
                if (buildOk) {
                    lastTimeHash[input] = file.LastWriteTime;
                }
            }
            return this;
        }

        public void Link (string output) {
            if (update || !File.Exists (output)) {
                ExecuteProcess.StartInfo.Arguments = string.Format ("-g \"{0}\" -o \"{1}\"", string.Join ("\" \"", objs), System.AppDomain.CurrentDomain.BaseDirectory + output);
                Run ();
            }
            AfterBuild (buildOk);
        }

        public void Run () {
            if (buildOk) {
                update = true;
                PrintLog (ExecuteProcess.StartInfo.FileName + " " + ExecuteProcess.StartInfo.Arguments);
                ExecuteProcess.Start ();
                string s;
                while ((s = ExecuteProcess.StandardOutput.ReadLine ()) != null) {
                    PrintLog (s);
                }
                while ((s = ExecuteProcess.StandardError.ReadLine ()) != null) {
                    PrintLog (s);
                }
                buildOk = buildOk && ExecuteProcess.ExitCode == 0;
            }
        }
    }
}