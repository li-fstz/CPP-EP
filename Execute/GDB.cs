using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

using CPP_EP.Lab;

namespace CPP_EP.Execute {

    using CodePosition = Nullable<ValueTuple<string, int>>;

    internal class GDB {
        public static readonly Regex StringValue = new Regex (@"\\""(.+)\\""");
        public static readonly Regex AddressValue = new Regex (@"(0x[0-9a-f]+)");
        private Process ExecuteProcess;
        private Thread readLineThread;
        private readonly Queue<string> ExecResult = new Queue<string> ();
        public static Action<string> PrintLog { private get; set; }
        public static Action<string, string> AfterRun { private get; set; }
        private readonly Queue<string> ReadLines = new Queue<string> ();
        private readonly object gdbLock = new object ();

        public GDB (string filepath) {
            ExecuteProcess = new Process ();
            ExecuteProcess.StartInfo.FileName = Properties.Settings.Default.GDBPath;
            ExecuteProcess.StartInfo.WorkingDirectory = Properties.Settings.Default.LabsPath;
            ExecuteProcess.StartInfo.Arguments = "-x " + Properties.Settings.Default.ScriptPath + " -silent --interpreter mi " + filepath;
            ExecuteProcess.StartInfo.UseShellExecute = false;
            ExecuteProcess.StartInfo.RedirectStandardOutput = true;
            ExecuteProcess.StartInfo.RedirectStandardInput = true;
            ExecuteProcess.StartInfo.RedirectStandardError = true;
            ExecuteProcess.StartInfo.CreateNoWindow = true;
            readLineThread = new Thread (ReadLineThreadFunc);
        }

        public void Start () {
            PrintLog (ExecuteProcess.StartInfo.FileName + " " + ExecuteProcess.StartInfo.Arguments);
            ExecuteProcess.Start ();
            ExecuteProcess.StandardInput.AutoFlush = true;
            readLineThread.Start ();
            GetExecResult (false);
            Send ("-exec-arguments > out.txt");
            Send ("-gdb-set print null-stop on");
        }

        private void ReadLineThreadFunc () {
            try {
                while (true) {
                    string s = ExecuteProcess.StandardOutput.ReadLine ();
                    lock (ReadLines) {
                        ReadLines.Enqueue (s);
                    }
                }
            } catch { };
        }

        private void SomeWayExecute (string cmd) {
            Util.ThreadRun (() => {
                lock (gdbLock) {
                    string r = Send (cmd);
                    AfterRun (r, r.IndexOf ("^error") != -1 ? null : GetExecResult ());
                }
            });
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

        private string ReadLine () {
            string r = null;
            int times = 10;
            while (times != 0) {
                Monitor.Enter (ReadLines);
                if (ReadLines.Count > 0) {
                    r = ReadLines.Dequeue ();
                    Monitor.Exit (ReadLines);
                    break;
                } else {
                    Monitor.Exit (ReadLines);
                    Thread.Sleep (100);
                    times -= 1;
                }
            }
            return r;
        }

        private string GetCmdResult () {
            string s, r = null;
            while ((s = ReadLine ()) != null && s.Length >= 0) {
                if (s[0] == '^') {
                    PrintLog ("gdb -> " + s);
                    //Console.WriteLine ("gdb -> " + s);
                    r = s;
                } else if (s[0] == '*') {
                    PrintLog ("gdb -> " + s);
                    //Console.WriteLine ("gdb -> " + s);
                    ExecResult.Enqueue (s);
                } else if (s[0] == '(') {
                    return r;
                }
            }
            return null;
        }

        public void SendScript (string script, Action<string> AfterSendScript) {
            Util.ThreadRun (() => {
                lock (gdbLock) {
                    string r = "";
                    string s;
                    PrintLog ("gdb <- " + script);
                    //Console.WriteLine ("gdb <- " + script);
                    try {
                        ExecuteProcess.StandardInput.WriteLine (script);
                        while ((s = ReadLine ()) != null) {
                            PrintLog ("gdb -> " + s);
                            //Console.WriteLine ("gdb -> " + s);
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
                    } catch (Exception e) {
                        PrintLog (e.Message);
                    }
                };
            });
        }

        public void GetValues (string[] names, Action AfterGetValues) {
            Util.ThreadRun (() => {
                lock (gdbLock) {
                    Dictionary<string, string> kvs = new Dictionary<string, string> ();
                    foreach (var name in names) {
                        string r = "";
                        string s;
                        PrintLog ("gdb <- " + name);
                        //Console.WriteLine ("gdb <- " + script);
                        try {
                            ExecuteProcess.StandardInput.WriteLine ("-data-evaluate-expression \"" + name + "\"");
                        } catch {
                            break;
                        }
                        while ((s = ReadLine ()) != null) {
                            PrintLog ("gdb -> " + s);
                            //Console.WriteLine ("gdb -> " + s);
                            r += s;
                            if (s[0] == '^') {
                                if (s.IndexOf ("^done") == -1) {
                                    r = null;
                                }
                            } else if (s[0] == '(') {
                                break;
                            }
                        }
                        if (r != null) {
                            var m = StringValue.Match (r);
                            if (m.Success) {
                                kvs[name] = m.Groups[1].Value;
                            } else {
                                m = AddressValue.Match (r);
                                if (m.Success) {
                                    kvs[name] = m.Groups[1].Value;
                                }
                            }
                        }
                    }
                    AbstractLab.WatchedValue = kvs;
                };
                AfterGetValues ();
            });
        }

        private string GetExecResult (bool first = true) {
            string r = null, s;
            if (ExecResult.Count != 0) {
                r = ExecResult.Dequeue ();
            } else {
                while ((s = ReadLine ()) != null) {
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
            if (readLineThread != null) {
                readLineThread = null;
            }
            if (ExecuteProcess != null) {
                ExecuteProcess.Kill ();
                ExecuteProcess = null;
            }
        }
    }

    internal static class BreakPoint {
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