using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

using CPP_EP.Lab;

namespace CPP_EP.Execute {

    using CodePosition = Nullable<ValueTuple<string, int>>;

    internal class GDB {
        public static readonly Regex StringValue = new Regex (@"\\""(.+)\\""");
        public static readonly Regex AddressValue = new Regex (@"(0x[0-9a-f]+)");
        private Process ExecuteProcess;
        public static Action<string> AfterRun { private get; set; }
        public static Action<string> PrintLog { private get; set; }
        private readonly object gdbLock = new object ();
        private readonly Queue<(ActionType, Action<string>)> GDBActions = new Queue<(ActionType, Action<string>)> ();

        private readonly StringBuilder GDBResult = new StringBuilder ();
        private bool stopMark, gdbMark;

        private enum ActionType {
            Run,
            Send,
            Value,
        }

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
            ExecuteProcess.OutputDataReceived += ExecuteProcess_OutputDataReceived;
            //readLineThread = new Thread (ReadLineThreadFunc);
        }

        private void ExecuteProcess_OutputDataReceived (object sender, DataReceivedEventArgs e) {
            if (!String.IsNullOrEmpty (e.Data)) {
                PrintLog ("gdb -> " + e.Data);
                if (GDBActions.Count > 0) {
                    switch (GDBActions.Peek ().Item1) {
                        case ActionType.Run:
                            switch (e.Data[0]) {
                                case '^':
                                    if (e.Data.IndexOf ("^error") == 0) {
                                        GDBResult.Clear ();
                                    } else {
                                        GDBResult.Append (e.Data);
                                    }
                                    break;

                                case '(':
                                    if (gdbMark && stopMark) {
                                        gdbMark = stopMark = false;
                                        GDBActions.Dequeue ().Item2 (GDBResult.ToString ());
                                        GDBResult.Clear ();
                                    } else {
                                        gdbMark = true;
                                    }
                                    break;

                                case '*':
                                    if (e.Data.IndexOf ("*stop") == 0) {
                                        GDBResult.Append (e.Data);
                                        stopMark = true;
                                    }
                                    break;

                                default:
                                    GDBResult.Append (e.Data);
                                    break;
                            }
                            break;

                        case ActionType.Send:
                            switch (e.Data[0]) {
                                case '^':
                                    if (e.Data.IndexOf ("^error") == 0) {
                                        GDBResult.Clear ();
                                    } else {
                                        GDBResult.Append (e.Data);
                                    }
                                    break;

                                case '(':
                                    GDBActions.Dequeue ().Item2 (GDBResult.ToString ());
                                    GDBResult.Clear ();
                                    break;

                                default:
                                    GDBResult.Append (e.Data);
                                    break;
                            }
                            break;

                        case ActionType.Value:
                            switch (e.Data[0]) {
                                case '^':
                                    if (e.Data.IndexOf ("^error") == 0) {
                                        GDBResult.Clear ();
                                    } else {
                                        GDBResult.Append (e.Data);
                                    }
                                    break;

                                case '(':
                                    var m = StringValue.Match (GDBResult.ToString ());
                                    if (m.Success) {
                                        GDBActions.Dequeue ().Item2 (m.Groups[1].Value);
                                    } else {
                                        m = AddressValue.Match (GDBResult.ToString ());
                                        if (m.Success) {
                                            GDBActions.Dequeue ().Item2 (m.Groups[1].Value);
                                        } else {
                                            GDBActions.Dequeue ().Item2 (null);
                                        }
                                    }
                                    GDBResult.Clear ();
                                    break;
                            }
                            break;
                    }
                }
            }
        }

        public void Start () {
            PrintLog (ExecuteProcess.StartInfo.FileName + " " + ExecuteProcess.StartInfo.Arguments);
            ExecuteProcess.Start ();
            ExecuteProcess.StandardInput.AutoFlush = true;
            ExecuteProcess.BeginOutputReadLine ();
            GDBActions.Enqueue ((ActionType.Send, s => { }));
            //readLineThread.Start ();
            //GetExecResult (false);
            Send ("-exec-arguments > out.txt", ActionType.Send, (s) => { });
            Send ("-gdb-set print null-stop on", ActionType.Send, (s) => { });
        }

        private void SomeWayExecute (string cmd) {
            Util.ThreadRun (() => {
                Send (cmd, ActionType.Run, AfterRun);
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
            Util.ThreadRun (() => {
                Send (
                    string.Format ("-break-insert {0}:{1}", filename, line),
                    ActionType.Send,
                    r => {
                        AfterSetBreakPoint (BreakPoint.Parse (r));
                    }
                );
            });
        }

        public void SetBreakpoints (List<(FileTab tab, string filename, int line)> lines, Action<CodePosition, FileTab, int> AfterSetBreakPoint, Action AfterSetBreakPoints) {
            Util.ThreadRun (() => {
                foreach (var line in lines) {
                    Send (
                        string.Format ("-break-insert {0}:{1}", line.filename, line.line),
                        ActionType.Send,
                        r => {
                            AfterSetBreakPoint (BreakPoint.Parse (r), line.tab, line.line);
                        }
                    );
                }
                AfterSetBreakPoints ();
            });
        }

        public void ClearBreakpoint (string filename, int line) {
            Util.ThreadRun (() => Send (string.Format ("clear {0}:{1}", filename, line), ActionType.Send, (r) => { }));
        }

        private void Send (string cmd, ActionType t, Action<string> AfterSend) {
            PrintLog ("gdb <- " + cmd);
            lock (gdbLock) {
                ExecuteProcess.StandardInput.WriteLine (cmd);
                GDBActions.Enqueue ((t, AfterSend));
            }
        }

        public void SendScript (string script, Action<string> AfterSendScript) {
            Util.ThreadRun (() => {
                Send (script, ActionType.Send, AfterSendScript);
            });
        }

        public void GetValues (string[] names, Action AfterGetValues) {
            Util.ThreadRun (() => {
                Dictionary<string, string> kvs = new Dictionary<string, string> ();
                foreach (var name in names) {
                    Send ("-data-evaluate-expression \"" + name + "\"", ActionType.Value, v => kvs[name] = v);
                }
                AbstractLab.WatchedValue = kvs;
                AfterGetValues ();
            });
        }

        public void Stop () {
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