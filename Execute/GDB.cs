using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CPP_EP.Execute {
    using CodePosition = Nullable<ValueTuple<string, int>>;

    class GDB {
        private Process ExecuteProcess;
        private readonly Queue<string> ExecResult = new Queue<string> ();
        private Action<string> PrintLog;
        private Action<string, string> AfterRun;
        public GDB (string gdbpath, string filepath, Action<string> printLog, Action<string, string> afterRun) {
            PrintLog = printLog;
            AfterRun = afterRun;

            ExecuteProcess = new Process ();
            ExecuteProcess.StartInfo.FileName = gdbpath;
            ExecuteProcess.StartInfo.Arguments = "-x C:\\Users\\User\\source\\repos\\li-fstz\\CPP-EP\\script\\struct.gdb -silent --interpreter mi " + filepath;
            ExecuteProcess.StartInfo.UseShellExecute = false;
            ExecuteProcess.StartInfo.RedirectStandardOutput = true;
            ExecuteProcess.StartInfo.RedirectStandardInput = true;
            ExecuteProcess.StartInfo.RedirectStandardError = true;
            ExecuteProcess.StartInfo.CreateNoWindow = true;
            ExecuteProcess.Start ();
            ExecuteProcess.StandardInput.AutoFlush = true;

            GetExecResult (false);

            Send ("set args > out.txt");
            Send ("set print null-stop on");
        }
        private void SomeWayExecute (string cmd) {
            Util.ThreadRun (() => AfterRun (Send (cmd), GetExecResult ()));
        }
        public void Run () {
            SomeWayExecute ("-exec-run");
        }
        public void Continue () {
            SomeWayExecute ("-exec-continue");
        }
        public void Next () {
            SomeWayExecute ("-exec-next");
        }
        public void Step () {
            SomeWayExecute ("-exec-step");
        }
        public void Finish () {
            SomeWayExecute ("-exec-finish");
        }
        public void SetBreakpoint (string filename, int line, Action<CodePosition> AfterSetBreakPoint) {
            Util.ThreadRun (() => AfterSetBreakPoint (SetBreakpoint (string.Format ("{0}:{1}", filename, line))));
        }
        public void SetBreakpoints (List<(FileTab tab, string filename, int line)> lines, Action<CodePosition, FileTab, int> AfterSetBreakPoint, Action AfterSetBreakPoints) {
            Util.ThreadRun (() => {
                foreach (var line in lines) {
                    AfterSetBreakPoint (SetBreakpoint (string.Format ("{0}:{1}", line.filename, line.line)), line.tab, line.line);
                }
                AfterSetBreakPoints ();
            });
        }
        private CodePosition SetBreakpoint (string parameter) {
            string r = Send ("-break-insert " + parameter);
            if (r.IndexOf ("done") != -1) {
                return BreakPoint.Parse (r);
            } else {
                return null;
            }
        }
        private bool ClearBreakpoint (string parameter) {
            return Send ("clear " + parameter).IndexOf ("done") != -1;
        }
        public string Print (string parameter) {
            var r = Send ("-data-evaluate-expression \"" + parameter + "\"");
            if (r.IndexOf ("done") != -1) {
                return r;
            } else {
                return null;
            }
        }
        public void ClearBreakpoint (string filename, int line, Action<bool> AfterClearBreakPoint) {
            Util.ThreadRun (() => AfterClearBreakPoint (ClearBreakpoint (string.Format ("{0}:{1}", filename, line))));
        }
        private string Send (string cmd) {
            PrintLog ("gdb <- " + cmd);
            ExecuteProcess.StandardInput.WriteLine (cmd);
            return GetCmdResult ();
        }
        private string ReadLine (bool timelimit = false) {
            string r = null;
            if (timelimit) {
                var task = Task.Run (() => ReadLine ());
                if (Task.WhenAny (task, Task.Delay (1000)).Result == task) {
                    r = task.Result;
                }
            } else {
                r = ExecuteProcess.StandardOutput.ReadLine ();
            }
            return r;
        }
        private string GetCmdResult () {
            string s, r = null;
            while ((s = ReadLine ()) != null) {
                if (s[0] == '^') {
                    PrintLog ("gdb -> " + s);
                    Console.WriteLine ("gdb -> " + s);
                    r = s;

                } else if (s[0] == '*') {
                    PrintLog ("gdb -> " + s);
                    Console.WriteLine ("gdb -> " + s);
                    ExecResult.Enqueue (s);
                } else if (s[0] == '(') {
                    return r;
                }
            }
            return null;
        }
        public void SendScript (string script, Action<string> AfterSendScript) {
            Util.ThreadRun (() => {
                string r = "";
                string s;
                PrintLog ("gdb <- " + script);
                Console.WriteLine ("gdb <- " + script);
                ExecuteProcess.StandardInput.WriteLine (script);
                while ((s = ReadLine ()) != null) {
                    PrintLog ("gdb -> " + s);
                    Console.WriteLine ("gdb -> " + s);
                    r += s;
                    if (s[0] == '^') {
                        if (s != "^done") {
                            r = null;
                        }
                    } else if (s[0] == '(') {
                        break;
                    }
                }
                AfterSendScript (r);
            });
        }

        private string GetExecResult (bool first = true) {
            string r = null, s;
            if (ExecResult.Count != 0) {
                r = ExecResult.Dequeue ();
            } else {
                while ((s = ReadLine (true)) != null) {
                    if (s[0] == '*') {
                        r = s;
                    } else if (s[0] == '(') {
                        break;
                    }
                }
            }
            if (first) {
                if (r == null || r.IndexOf ("*stop") != -1) {
                    return r;
                } else {
                    return GetExecResult (false);
                }
            } else {
                return r;
            }
        }
        public void Stop () {
            ExecuteProcess.Kill ();
            ExecuteProcess = null;
        }
    }
    static class BreakPoint {
        private static readonly Regex BreakpointFile = new Regex (@"fullname=""(.+?)""");
        private static readonly Regex BreakpointLine = new Regex (@"line=""(\d+)""");
        public static CodePosition Parse (string str) {
            try {
                return (Util.RegexGroupOne (BreakpointFile, str), int.Parse (Util.RegexGroupOne (BreakpointLine, str)));
            } catch (Exception) {
                return null;
            }
        }
    }
}
