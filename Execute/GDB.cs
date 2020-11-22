using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CPP_EP.Execute {
    using ReciveData = List<List<string>>;
    class GDB: Execute {
        public GDB (string gdbpath, string filepath) : base (gdbpath, "-silent --interpreter mi " + filepath) {
            Send ("set args > out.txt");
            Send ("set print null-stop on");
            GetReciveData ().ForEach (l => ParseResponcs (l));
        }
        private RunResult SomeWayExecute (string cmd) {
            Send (cmd);
            return RunResult.Parse (GetReciveData ());
        }
        public RunResult Run () {
            return SomeWayExecute ("-exec-run");
        }
        public RunResult Continue () {
            return SomeWayExecute ("-exec-continue");
        }
        public RunResult Next () {
            return SomeWayExecute ("-exec-next");
        }
        public RunResult Step () {
            return SomeWayExecute ("-exec-step");
        }
        public RunResult Finish () {
            return SomeWayExecute ("-exec-finish");
        }
        public BreakPoint SetBreakpoint (string filename, int line) {
            return SetBreakpoint (String.Format ("{0}:{1}", filename, line));
        }
        public BreakPoint SetBreakpoint (string parameter) {
            Send ("-break-insert " + parameter);
            GDBOutput output = ParseResponcs(GetReciveData ()[0]);
            if (output.Result.Class == "done") {
                var b = BreakPoint.Parsing(output.Result.Result);
                return BreakPoint.Insert (b.Item1, b.Item2);
            } else {
                return null;
            }
        }
        public bool ClearBreakpoint (string parameter) {
            Send ("clear " + parameter);
            return ParseResponcs (GetReciveData ()[0]).Result.Class == "done";
        }
        public string Print (string parameter) {
            Send ("print " + parameter);
            return ParseResponcs (GetReciveData ()[0]).StreamConsole;
        }
        public bool ClearBreakpoint (string filename, int line) {
            BreakPoint.Delete (filename, line);
            return ClearBreakpoint (String.Format ("{0}:{1}", filename, line));
        }
        public static GDBOutput ParseResponcs (List<string> Responcs) {
            GDBOutput output = new GDBOutput();
            Responcs.ForEach (s => {
                Console.WriteLine ("  *  " + s);
                int offset;
                switch (s.ToCharArray ()[0]) {
                    case '*':
                        offset = s.IndexOf (',');
                        output.AsyncExec.Add (new GDBOutput.Record (s.Substring (1, offset - 1), s.Substring (offset + 1)));
                        break;
                    case '+':
                        break;
                    case '=':
                        offset = s.IndexOf (',');
                        output.AsyncNotify.Add (new GDBOutput.Record (s.Substring (1, offset - 1), s.Substring (offset + 1)));
                        break;
                    case '^':
                        offset = s.IndexOf (',');
                        if (offset == -1) {
                            output.Result.Class = s.Substring (1);
                            output.Result.Result = "";
                        } else {
                            output.Result.Class = s.Substring (1, offset - 1);
                            output.Result.Result = s.Substring (offset + 1);
                        }
                        break;
                    case '~':
                        output.StreamConsole += s.Substring (2, s.Length - 3) + '\n';
                        break;
                    case '@':
                        break;
                    case '&':
                        break;
                    default:
                        break;
                }
            });
            return output;
        }
    }
    class GDBOutput {
        public class Record {
            public string Class;
            public string Result;
            public Record (string Class, string Result) {
                this.Class = Class;
                this.Result = Result;
            }
            public Record () { }

        };
        public Record Result = new Record();
        public List<Record> AsyncExec = new List<Record>();
        //public List<Record> AsyncStatus = new List<Record>();
        public List<Record> AsyncNotify = new List<Record>();
        public string StreamConsole = "";
        //public List<Record> StreamTarget = new List<Record>();
        //public List<Record> StreamLog = new List<Record>();
    }
    class BreakPoint {
        private static readonly Regex BreakpointFile = new Regex(@"fullname=""(.+?)""");
        private static readonly Regex BreakpointLine = new Regex(@"line=""(\d+)""");
        public static List<BreakPoint> BreakPoints = new List<BreakPoint>();
        public string Filename;
        public int Line;
        private BreakPoint (string filename, int line) {
            Filename = filename;
            Line = line;
        }
        public static BreakPoint Insert (string filename, int line) {
            BreakPoint b = Get(filename, line);
            if (b == null) {
                b = new BreakPoint (filename, line);
            }
            return b;
        }
        public static BreakPoint Get (string filename, int line) {
            BreakPoint breakPoint = null;
            foreach (var b in BreakPoints) {
                if (b.Filename == filename && b.Line == line) {
                    breakPoint = b;
                    break;
                }
            }
            return breakPoint;
        }
        public static void Delete (string filename, int line) {
            BreakPoint breakPoint = Get(filename, line);
            if (breakPoint != null) {
                BreakPoints.Remove (breakPoint);
            }
        }
        public static (string, int) Parsing (string str) {
            return (BreakpointFile.Match (str).Groups[1].Value, int.Parse (BreakpointLine.Match (str).Groups[1].Value));
        }
    }
    class RunResult {
        private static readonly Regex StopReason = new Regex(@"reason=""(.+?)""");
        public bool Running = true;
        public string Reason = null;
        public BreakPoint Breakpoint = null;
        private RunResult () { }
        public static RunResult Parse (ReciveData reciveData) {
            bool running = true;
            string result = null;
            foreach (var l in reciveData) {
                foreach (var ae in GDB.ParseResponcs (l).AsyncExec) {
                    if (ae.Class == "stopped") {
                        running = false;
                        result = ae.Result;
                        break;
                    }
                }
                if (!running) {
                    break;
                }
            }
            RunResult r;
            if (running) {
                r = new RunResult ();
            } else {
                string reason = StopReason.Match (result).Groups[1].Value;
                BreakPoint breakPoint = null;
                if (reason == "breakpoint-hit") {
                    (var filename, var line) = BreakPoint.Parsing (result);
                    breakPoint = BreakPoint.Get (filename, line);
                }
                r = new RunResult () {
                    Running = running,
                    Reason = reason,
                    Breakpoint = breakPoint
                };
            }
            return r;
        }

    }
}
