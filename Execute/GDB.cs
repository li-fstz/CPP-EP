﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace CPP_EP.Execute {
    using RunResult = ValueTuple<string, string>;
    class GDB {
        class ExecuteNotResponseException: Exception { }
        private object ReadLock = new object();
        private Process ExecuteProcess;
        private Thread ReadThread;
        private readonly string Filename;
        private readonly string Arguments;
        private readonly Queue<string> ExecResult = new Queue<string>();
        private readonly Dictionary<int, string> CmdResult = new Dictionary<int, string>();
        private int no = 0;
        public GDB (string gdbpath, string filepath){

            Filename = gdbpath;
            Arguments = "-silent --interpreter mi " + filepath;

            ExecuteProcess = new Process ();
            ExecuteProcess.StartInfo.FileName = Filename;
            ExecuteProcess.StartInfo.Arguments = Arguments;
            ExecuteProcess.StartInfo.UseShellExecute = false;
            ExecuteProcess.StartInfo.RedirectStandardOutput = true;
            ExecuteProcess.StartInfo.RedirectStandardInput = true;
            ExecuteProcess.StartInfo.RedirectStandardError = true;
            ExecuteProcess.StartInfo.CreateNoWindow = true;
            ExecuteProcess.Start ();
            ExecuteProcess.StandardInput.AutoFlush = true;
            //ReadThread = new Thread (new ThreadStart (Receive));
            //ReadThread.Start ();
            Send ("set args > out.txt");
            //Thread.Sleep (100);
            Send ("set print null-stop on");
            //GetReciveData (200).ForEach (l => ParseResponcs (l));
        }
        private RunResult SomeWayExecute (string cmd) {
            return (Send (cmd), GetExecResult2());
        }
        public (string cmdResult, string execResult) Run () {
            return SomeWayExecute ("-exec-run");
        }
        public (string cmdResult, string execResult) Continue () {
            return SomeWayExecute ("-exec-continue");
        }
        public (string cmdResult, string execResult) Next () {
            return SomeWayExecute ("-exec-next");
        }
        public (string cmdResult, string execResult) Step () {
            return SomeWayExecute ("-exec-step");
        }
        public (string cmdResult, string execResult) Finish () {
            return SomeWayExecute ("-exec-finish");
        }
        public (string Filename, int Line)? SetBreakpoint (string filename, int line) {
            return SetBreakpoint (string.Format ("{0}:{1}", filename, line));
        }
        public (string Filename, int Line)? SetBreakpoint (string parameter) {
            string r = Send ("-break-insert " + parameter);
            if (r.IndexOf ("done") != -1) {
                return BreakPoint.Parse (r);
            } else {
                return null;
            }
        }
        public bool ClearBreakpoint (string parameter) {
            return Send ("clear " + parameter).IndexOf ("done") != -1;
        }
        public string Print (string parameter) {
            var r = Send ("-data-evaluate-expression \"" + parameter + "\"");
            if (r.IndexOf("done") != -1) {
                return r;
            } else {
                return null;
            }
        }
        public bool ClearBreakpoint (string filename, int line) {
            return ClearBreakpoint (string.Format ("{0}:{1}", filename, line));
        }
        protected string Send (string cmd, bool r = true) {
            cmd = no + cmd;
            Console.WriteLine ("gdb <- " + cmd);
            //GDBText.AppendText ("gdb <- " + cmd + "\n");
            //GDBText.ScrollToEnd (); ;
            ExecuteProcess.StandardInput.WriteLine (cmd);
            if (r) return GetCmdResult2 (no++);
            else return null;
        }
        private string ReadLine() {
            string s;
            lock(ReadLock) {
                s = ExecuteProcess.StandardOutput.ReadLine ();
            }
            return s;
        }
        protected string GetCmdResult(int no) {
            Thread.Sleep (50);
            string r = null;
            lock (CmdResult) {
                if (CmdResult.ContainsKey (no)) {
                    r = CmdResult[no];
                    CmdResult.Remove (no);
                }
            }
            return r ?? GetCmdResult (no);
        }
        protected string GetCmdResult2 (int no) {
            string s = null;
            if (CmdResult.ContainsKey (no)) {
                s = CmdResult[no];
                CmdResult.Remove (no);
            }
            if (s != null) {
                return s;
            }
            while ((s = ReadLine()) != null) {
                if (char.IsDigit (s[0])) {
                    Console.WriteLine ("gdb -> " + s);
                    int inno = int.Parse (s.Substring (0, s.IndexOf ("^")));
                    if (inno == no) {
                        return s;
                    }
                    CmdResult[inno] = s;
                } else if (s[0] == '*') {
                    Console.WriteLine ("gdb -> " + s);
                    ExecResult.Enqueue (s);
                }
            }
            return null;
        }
        protected string GetExecResult(int time = 0) {
            if (time != 0) Thread.Sleep (100);
            string r = null;
            lock (ExecResult) {
                if (ExecResult.Count != 0) {
                    r = ExecResult.Dequeue();
                }
            }
            if (r.IndexOf ("*running") == 0) {
                r = null;
            }
            return r ?? (time == 10? null: GetExecResult (time + 1));
        }
        protected string GetExecResult2 (int time = 0) {
            string r = null;
            if (ExecResult.Count != 0) {
                r = ExecResult.Dequeue ();
            }
            var task = Task.Run(() => ReadLine());
            if (Task.WhenAny (task, Task.Delay (100)).Result != task) {
                return Send ("", false);
            }
            r = task.Result;
            if (r.IndexOf ("*stop") == -1) {
                r = null;
            } else {
                Console.WriteLine ("gdb -> " + r);
            }
            return r ?? (time == 10 ? null : GetExecResult2 (time + 1));
        }
        public void Stop () {
            //ReadThread.Abort ();
            ExecuteProcess.Kill ();
            //ReadThread = null;
            ExecuteProcess = null;
        }
        private void Receive () {
            string s;
            try {
                while ((s = ReadLine()) != null) {
                    //GDBText.AppendText ("gdb -> " + s + "\n");
                    if (char.IsDigit (s[0])) {
                        Console.WriteLine ("gdb -> " + s);
                        lock (CmdResult) {
                            CmdResult[int.Parse (s.Substring (0, s.IndexOf ("^")))] = s;
                        }
                    } else if (s[0] == '*') {
                        Console.WriteLine ("gdb -> " + s);
                        lock (ExecResult) {
                            ExecResult.Enqueue (s);
                        }
                    }
                }
            } catch (Exception) { }
        }
    }
    static class BreakPoint {
        private static readonly Regex BreakpointFile = new Regex(@"fullname=""(.+?)""");
        private static readonly Regex BreakpointLine = new Regex(@"line=""(\d+)""");
        public static (string Filename, int Line)? Parse (string str) {
            try {
                return (Util.RegexGroupOne (BreakpointFile, str), int.Parse (Util.RegexGroupOne (BreakpointLine, str)));
            } catch (Exception) {
                return null;
            }
        }
    }
}
