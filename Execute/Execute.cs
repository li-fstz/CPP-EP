using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace CPP_EP.Execute {
    using ReciveData = List<List<string>>;
    class Execute {
        class ExecuteNotResponseException: Exception { }
        private Process ExecuteProcess;
        private Thread ReadThread;
        private string Filename, Arguments;
        private readonly Queue<List<string>> reciveData = new Queue<List<string>>();
        protected Execute (string filename, string arguments) {
            Filename = filename;
            Arguments = arguments;
            Start ();
        }
        public void Start () {
            ExecuteProcess = new Process ();
            ExecuteProcess.StartInfo.FileName = Filename;
            ExecuteProcess.StartInfo.Arguments = Arguments;
            ExecuteProcess.StartInfo.UseShellExecute = false;
            ExecuteProcess.StartInfo.RedirectStandardOutput = true;
            ExecuteProcess.StartInfo.RedirectStandardInput = true;
            ExecuteProcess.StartInfo.RedirectStandardError = true;
            ExecuteProcess.Start ();
            ExecuteProcess.StandardInput.AutoFlush = true;
            ReadThread = new Thread (new ThreadStart (Receive));
            ReadThread.Start ();
            GetReciveData ();//.ForEach( l => ParseResponcs (l));
        }
        protected void Send (string cmd) {
            ExecuteProcess.StandardInput.WriteLine (cmd);
        }
        protected ReciveData GetReciveData () {
            Thread.Sleep (100);
            ReciveData data = new List<List<string>>();
            lock (reciveData) {
                while (reciveData.Count != 0) {
                    data.Add (reciveData.Dequeue ());
                }
            }
            return data;
        }
        public void Stop () {
            ReadThread.Abort ();
            ExecuteProcess.Kill ();
            ReadThread = null;
            ExecuteProcess = null;
        }
        private void Receive () {
            string s;
            List<string> tmp = null;
            try {
                while ((s = ExecuteProcess.StandardOutput.ReadLine ()) != null) {
                    if (tmp == null) {
                        Monitor.Enter (reciveData);
                        tmp = new List<string> ();
                    }
                    if (s == "(gdb) ") {
                        reciveData.Enqueue (tmp);
                        Monitor.Exit (reciveData);
                        tmp = null;
                    } else {
                        tmp.Add (s);
                    }
                }
            } catch (Exception ignore) { }
            if (tmp != null) {
                reciveData.Enqueue (tmp);
                Monitor.Exit (reciveData);
            }
        }
    }
}
