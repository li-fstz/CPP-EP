using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace CPP_EP.Execute
{
    using RunResult = ValueTuple<string, string>;
    using CodePosition = Nullable<ValueTuple<string, int>>;
    class GDB
    {
        class ExecuteNotResponseException : Exception { }
        private object ReadLock = new object();
        private Process ExecuteProcess;
        private readonly Queue<string> ExecResult = new Queue<string>();
        private readonly Dictionary<int, string> CmdResult = new Dictionary<int, string>();
        private int no = 0;
        public GDB(string gdbpath, string filepath)
        {

            ExecuteProcess = new Process();
            ExecuteProcess.StartInfo.FileName = gdbpath;
            ExecuteProcess.StartInfo.Arguments = "-x C:\\Users\\li-fs\\Documents\\哈理工\\毕业设计\\CPP-EP\\script\\struct.gdb -silent --interpreter mi " + filepath;
            ExecuteProcess.StartInfo.UseShellExecute = false;
            ExecuteProcess.StartInfo.RedirectStandardOutput = true;
            ExecuteProcess.StartInfo.RedirectStandardInput = true;
            ExecuteProcess.StartInfo.RedirectStandardError = true;
            ExecuteProcess.StartInfo.CreateNoWindow = true;
            ExecuteProcess.Start();
            ExecuteProcess.StandardInput.AutoFlush = true;
            Send("set args > out.txt");
            Send("set print null-stop on");
        }
        private RunResult SomeWayExecute(string cmd)
        {
            return (Send(cmd), GetExecResult());
        }
        public RunResult Run()
        {
            return SomeWayExecute("-exec-run");
        }
        public RunResult Continue()
        {
            return SomeWayExecute("-exec-continue");
        }
        public RunResult Next()
        {
            return SomeWayExecute("-exec-next");
        }
        public RunResult Step()
        {
            return SomeWayExecute("-exec-step");
        }
        public RunResult Finish()
        {
            return SomeWayExecute("-exec-finish");
        }
        public CodePosition SetBreakpoint(string filename, int line)
        {
            return SetBreakpoint(string.Format("{0}:{1}", filename, line));
        }
        public CodePosition SetBreakpoint(string parameter)
        {
            string r = Send("-break-insert " + parameter);
            if (r.IndexOf("done") != -1)
            {
                return BreakPoint.Parse(r);
            }
            else
            {
                return null;
            }
        }
        public bool ClearBreakpoint(string parameter)
        {
            return Send("clear " + parameter).IndexOf("done") != -1;
        }
        public string Print(string parameter)
        {
            var r = Send("-data-evaluate-expression \"" + parameter + "\"");
            if (r.IndexOf("done") != -1)
            {
                return r;
            }
            else
            {
                return null;
            }
        }
        public bool ClearBreakpoint(string filename, int line)
        {
            return ClearBreakpoint(string.Format("{0}:{1}", filename, line));
        }
        protected string Send(string cmd, bool r = true)
        {
            cmd = no + cmd;
            Console.WriteLine("gdb <- " + cmd);
            ExecuteProcess.StandardInput.WriteLine(cmd);
            if (r) return GetCmdResult(no++);
            else return null;
        }
        private string ReadLine()
        {
            string s;
            lock (ReadLock)
            {
                s = ExecuteProcess.StandardOutput.ReadLine();
            }
            return s;
        }
        protected string GetCmdResult(int no)
        {
            string s;
            if (CmdResult.ContainsKey(no))
            {
                s = CmdResult[no];
                CmdResult.Remove(no);
                return s;
            }
            while ((s = ReadLine()) != null)
            {
                if (char.IsDigit(s[0]))
                {
                    Console.WriteLine("gdb -> " + s);
                    int inno = int.Parse(s.Substring(0, s.IndexOf("^")));
                    if (inno == no)
                    {
                        return s;
                    }
                    CmdResult[inno] = s;
                }
                else if (s[0] == '*')
                {
                    Console.WriteLine("gdb -> " + s);
                    ExecResult.Enqueue(s);
                }
            }
            return null;
        }
        public string SendScript(string script)
        {
            string r = "";
            string s;
            Console.WriteLine("gdb <- " + script);
            ExecuteProcess.StandardInput.WriteLine(script);
            while ((s = ReadLine()) != null)
            {
                Console.WriteLine("gdb -> " + s);
                r += s;
                if (s[0] == '^')
                {
                    if (s != "^done")
                    {
                        r = null;
                    }
                    break;
                }
            }
            return r;
        }
        protected string GetExecResult(bool first = true)
        {
            string r = null;
            if (ExecResult.Count != 0)
            {
                r = ExecResult.Dequeue();
            }
            else
            {
                var task = Task.Run(() => ReadLine());
                if (Task.WhenAny(task, Task.Delay(1000)).Result == task)
                {
                    r = task.Result;
                }
            }
            if (first)
            {
                if (r == null)
                {
                    throw new ExecuteNotResponseException();
                } 
                else if (r.IndexOf("*stop") == -1)
                {
                    r = GetExecResult(false);
                    if (r == null)
                    {
                        throw new ExecuteNotResponseException();
                    } 
                    else
                    {
                        return r;
                    }
                }
                else
                {
                    return r;
                }
            }
            else
            {
                return r;
            }
        }
        public void Stop()
        {
            ExecuteProcess.Kill();
            ExecuteProcess = null;
        }
    }
    static class BreakPoint
    {
        private static readonly Regex BreakpointFile = new Regex(@"fullname=""(.+?)""");
        private static readonly Regex BreakpointLine = new Regex(@"line=""(\d+)""");
        public static CodePosition Parse(string str)
        {
            try
            {
                return (Util.RegexGroupOne(BreakpointFile, str), int.Parse(Util.RegexGroupOne(BreakpointLine, str)));
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
