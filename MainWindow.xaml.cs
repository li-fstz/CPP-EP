using CPP_EP.Execute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CPP_EP
{
    using CodePosition = ValueTuple<string, int>;
    public partial class MainWindow : Window
    {
        
        private List<string>[] labFiles;
        private GDB gdb;
        private Xmake xmake;
        private bool run;
        private FileTab lastStopTab;
        private int lastStopLine;
        

        public MainWindow()
        {
            InitializeComponent();
            labFiles = new List<string>[]
            {
                new List<string>() { "lab1.c", "src\\rule.c", "src\\voidtable.c" },
                new List<string>() { "lab2.c", "src\\rule.c", "src\\voidtable.c", "src\\first.c" },
                new List<string>() { "lab3.c", "src\\rule.c", "src\\voidtable.c", "src\\first.c", "src\\follow.c" },
                new List<string>() { "lab4.c", "src\\rule.c", "src\\voidtable.c", "src\\first.c", "src\\follow.c", "src\\parsingtable.c" },
                new List<string>() { "lab5.c", "src\\rule.c", "src\\pickupleftfactor.c" },
                new List<string>() { "lab6.c", "src\\rule.c", "src\\removeleftrecursion1.c" },
                new List<string>() { "lab7.c", "src\\rule.c", "src\\removeleftrecursion2.c" },
                new List<string>() { "lab8.c", "src\\rule.c", "src\\voidtable.c", "src\\first.c", "src\\follow.c", "src\\parsingtable.c", "src\\parser.c" }
            };

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            xmake = new Xmake("C:\\Users\\User\\CPP-Labs\\", "C:\\xmake\\xmake.exe", PrintXmakeLog);
        }

        CodePosition? SetBreakPoint(CodePosition cp)
        {
            if (gdb == null)
            {
                return cp;
            }
            else
            {
                return gdb.SetBreakpoint(cp.Item1, cp.Item2);
            }
            
        }

        void DeleteBreakPoint(CodePosition cp)
        {
            if (gdb != null)
            {
                gdb.ClearBreakpoint(cp.Item1, cp.Item2);
            }
        }

        private void OpenLabFiles(int index)
        {
            tabControl.Items.Clear();
            foreach (var file in labFiles[index])
            {
                tabControl.Items.Add(FileTab.GetInstance("C:\\Users\\User\\CPP-Labs\\" + file, SetBreakPoint, DeleteBreakPoint));
            }
            tabControl.SelectedIndex = 0;
            tabControl.Focus();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (run)
            {
                gdb.Continue();
            } 
            else
            {
                run = true;
                startButton.Content = "继续";
                stopButton.IsEnabled = true;
                labSelect.IsEnabled = false;
                if (gdb == null)
                {
                    if (xmake.build(labSelect.SelectedIndex))
                    {
                        gdb = new GDB(
                            "C:\\MinGW\\bin\\gdb.exe", 
                            "C:\\Users\\User\\CPP-Labs\\build\\lab" + labSelect.SelectedIndex + ".exe", 
                            PrintGDBLog, 
                            AfterRun
                        );
                    }
                    else
                    {

                    }
                    
                }
                CorrectBreakPoint();
                gdb.Run();
            }
            
        }

        private void AfterRun(string state, string res)
        {
            if (lastStopTab != null)
            {
                lastStopTab.UnMarkLine(lastStopLine, FileTab.HIGHLIGHT_MARKER);
            }
            if (res == null)
            {

            }
            else
            {
                if (res.IndexOf("breakpoint-hit") != -1
                    || res.IndexOf("end-stepping-range") != -1
                    || res.IndexOf("function-finished") != -1)
                {
                    /*
                    List<Lab.Lab.Rule> rules = lab4.GetRules("pHead");
                    Lab1.VoidTable voidTable = lab4.GetVoidTable("&VoidTable");
                    List<Lab2.Set> firstSet = lab4.GetSetList ("&FirstSetList");
                    List<Lab2.Set> followSet = lab4.GetSetList ("&FollowSetList");
                    List<Lab4.SelectSet> selectSet = lab4.GetSelectSetList ("&SelectSetList");
                    Lab4.ParsingTable parsingTable = lab4.GetParsingTable ("&ParsingTable");
                    List<Lab.Lab.Symbol> stack = lab8.GetParsingStack("&Stack");
                    */
                    restartButton.IsEnabled = true;
                    stepButton.IsEnabled = true;
                    nextButton.IsEnabled = true;
                    finishButton.IsEnabled = true;
                    var b = BreakPoint.Parse(res);
                    if (b.HasValue)
                    {
                        lastStopTab = FileTab.GetInstance(Path.GetFileName(b.Value.Item1));
                        tabControl.SelectedItem = lastStopTab;
                        lastStopLine = b.Value.Item2;
                        lastStopTab.scintilla.Lines[lastStopLine - 1].Goto();
                        lastStopTab.MarkLine(lastStopLine, FileTab.HIGHLIGHT_MARKER);
                    }
                }
                else
                {
                    restartButton.IsEnabled = false;
                    stepButton.IsEnabled = false;
                    nextButton.IsEnabled = false;
                    finishButton.IsEnabled = false;
                    startButton.Content = "启动";
                    run = false;
                    PrintGDBLog(res);
                    PrintOutput(File.ReadAllText("out.txt", System.Text.Encoding.GetEncoding("GB2312")));
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            gdb.Stop();
            gdb = null;
            startButton.Content = "启动";
            stopButton.IsEnabled = false;
            labSelect.IsEnabled = true;

            if (lastStopTab != null)
            {
                lastStopTab.UnMarkLine(lastStopLine, FileTab.HIGHLIGHT_MARKER);
                lastStopTab = null;
            }
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            gdb.Step();
        }
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            gdb.Next();
        }
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            gdb.Finish();
        }

        private void LabSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (labSelect.SelectedIndex != 0)
            {
                OpenLabFiles(labSelect.SelectedIndex - 1);
                startButton.IsEnabled = true;
            }
            else
            {
                startButton.IsEnabled = false;
            }
        }
        private void PrintXmakeLog(string s)
        {
            logControl.SelectedIndex = 0;
            buildText.AppendText(s);
            buildText.AppendText("\n");
            buildText.ScrollToEnd();

        }
        private void PrintOutput(string s)
        {
            logControl.SelectedIndex = 1;
            outputText.Text = s;
            outputText.ScrollToEnd();
        }
        private void PrintGDBLog(string s)
        {
            logControl.SelectedIndex = 2;
            gdbText.AppendText(s);
            gdbText.AppendText("\n");
            gdbText.ScrollToEnd();
        }
        private void CorrectBreakPoint()
        {
            foreach (FileTab tab in tabControl.Items)
            {
                foreach (int line in tab.GetAllBreakPointLine())
                {
                    var b = gdb.SetBreakpoint(tab.Header as string, line);
                    if (b.HasValue)
                    {
                        if (line != b.Value.Item2)
                        {
                            tab.UnMarkLine(line, FileTab.BOOKMARK_MARKER);
                            tab.MarkLine(b.Value.Item2, FileTab.BOOKMARK_MARKER);
                        }
                    }
                    else
                    {
                        tab.UnMarkLine(line, FileTab.BOOKMARK_MARKER);
                    }
                }
            }
        }
    }
}
