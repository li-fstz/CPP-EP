using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

using CPP_EP.Execute;
using CPP_EP.Lab;

namespace CPP_EP {

    using CodePosition = ValueTuple<string, int>;

    public partial class MainWindow: Window {
        private AbstractLab lab;
        private GDB gdb;
        private bool run;
        private FileTab lastStopTab;
        private int lastStopLine;
        private readonly List<TextBlock> textBlocks = new List<TextBlock> ();

        public MainWindow () {
            InitializeComponent ();
            System.Text.Encoding.RegisterProvider (System.Text.CodePagesEncodingProvider.Instance);
            GDB.PrintLog = s => Dispatcher.BeginInvoke ((Action<string>)PrintGDBLog, s);
            GDB.PrintLog = s => { };
            GDB.AfterRun = (s1, s2) => Dispatcher.BeginInvoke ((Action<string, string>)AfterRun, s1, s2);
            GCC.AfterBuild = b => Dispatcher.BeginInvoke ((Action<bool>)AfterBuild, b);
            GCC.PrintLog = s => Dispatcher.BeginInvoke ((Action<string>)PrintMakeLog, s);
            AbstractLab.UpdateUI = UpdateUI;
            FileTab.SetBreakPoint = SetBreakPoint;
            FileTab.DeleteBreakPoint = DeleteBreakPoint;
        }

        private void UpdateUI (int i, Action<TextBlock> a) {
            Dispatcher.BeginInvoke ((Action)(() => {
                var c = textBlocks.Count;
                if (c < i) {
                    for (; c <= i; c++) {
                        var tb = new TextBlock ();
                        var cb = new TextBlock ();
                        textBlocks.Add (tb);
                        dataStructureView.Inlines.Add (tb);
                        dataStructureView.Inlines.Add (cb);
                        dataStructureView.Inlines.Add (new LineBreak ());
                    }
                }
                a (textBlocks[i - 1]);
            }));
        }

        private void SetBreakPoint (CodePosition cp, Action<CodePosition?> AfterSetBreakPoint) {
            if (gdb == null) {
                AfterSetBreakPoint (cp);
            } else {
                gdb.SetBreakpoint (cp.Item1, cp.Item2, r => Dispatcher.BeginInvoke (AfterSetBreakPoint, r));
            }
        }

        private void DeleteBreakPoint (CodePosition cp) {
            if (gdb != null) {
                gdb.ClearBreakpoint (cp.Item1, cp.Item2, r => { });
            }
        }

        private void StartButton_Click (object sender, RoutedEventArgs e) {
            if (run) {
                SetRunButton (false);
                gdb.Continue ();
            } else {
                textBlocks.Clear ();
                dataStructureView.Inlines.Clear ();
                AbstractLab.DataHash.Clear ();
                startButton.IsEnabled = false;
                labSelect.IsEnabled = false;
                lab.Build ();
            }
        }

        private void AfterBuild (bool buildOk) {
            if (buildOk) {
                gdb = lab.GetGDB ();
                gdb.Start ();
                CorrectBreakPoint (() => {
                    Dispatcher.BeginInvoke ((Action)(() => {
                        run = true;
                        startButton.IsEnabled = true;
                        startButton.Content = "继续";
                        stopButton.IsEnabled = true;
                    }));
                    gdb.Run ();
                });
            } else {
                startButton.IsEnabled = true;
                labSelect.IsEnabled = true;
                logControl.SelectedIndex = 0;
            }
        }

        private void AfterRun (string state, string res) {
            SetRunButton (true);
            if (state != null && state.IndexOf ("^error") != -1) {
                return;
            }
            if (lastStopTab != null) {
                lastStopTab.UnRunMarkLine ();
            }
            if (res == null) {
                restartButton.IsEnabled = false;
                stepButton.IsEnabled = false;
                nextButton.IsEnabled = false;
                finishButton.IsEnabled = false;
                startButton.Content = "启动";
                run = false;
                gdb.Stop ();
                gdb = null;
                PrintGDBLog (res);
                PrintOutput (File.ReadAllText (Properties.Settings.Default.LabsPath + "out.txt", System.Text.Encoding.GetEncoding ("GB2312")));
            } else {
                if (res.IndexOf ("breakpoint-hit") != -1
                    || res.IndexOf ("end-stepping-range") != -1
                    || res.IndexOf ("function-finished") != -1) {
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
                    var b = BreakPoint.Parse (res);
                    if (b.HasValue) {
                        lastStopTab = FileTab.GetInstance (Path.GetFileName (b.Value.Item1));
                        tabControl.SelectedItem = lastStopTab;
                        lastStopLine = b.Value.Item2;
                        lastStopTab.GotoLine (lastStopLine);
                        lastStopTab.RunMarkLine (lastStopLine);
                    }
                    lab.Draw ();
                } else {
                    restartButton.IsEnabled = false;
                    stepButton.IsEnabled = false;
                    nextButton.IsEnabled = false;
                    finishButton.IsEnabled = false;
                    stopButton.IsEnabled = false;
                    labSelect.IsEnabled = true;
                    startButton.Content = "启动";
                    run = false;
                    gdb.Stop ();
                    gdb = null;
                    PrintGDBLog (res);
                    PrintOutput (File.ReadAllText (Properties.Settings.Default.LabsPath + "out.txt", System.Text.Encoding.GetEncoding ("GB2312")));
                }
            }
        }

        private void StopButton_Click (object sender, RoutedEventArgs e) {
            if (gdb != null) {
                gdb.Stop ();
                gdb = null;
            }
            run = false;
            startButton.Content = "启动";
            stopButton.IsEnabled = false;
            labSelect.IsEnabled = true;
            SetRunButton (false);
            startButton.IsEnabled = true;
            restartButton.IsEnabled = false;
            if (lastStopTab != null) {
                lastStopTab.UnRunMarkLine ();
                lastStopTab = null;
            }
        }

        private void RestartButton_Click (object sender, RoutedEventArgs e) {
        }

        private void SettingButton_Click (object sender, RoutedEventArgs e) {
        }

        private void StepButton_Click (object sender, RoutedEventArgs e) {
            SetRunButton (false);
            gdb.Step ();
        }

        private void NextButton_Click (object sender, RoutedEventArgs e) {
            SetRunButton (false);
            gdb.Next ();
        }

        private void FinishButton_Click (object sender, RoutedEventArgs e) {
            SetRunButton (false);
            gdb.Finish ();
        }

        private void LabSelect_SelectionChanged (object sender, SelectionChangedEventArgs e) {
            lab = AbstractLab.GetLab (labSelect.SelectedIndex);
            if (labSelect.SelectedIndex != 0) {
                tabControl.Items.Clear ();
                foreach (var file in lab.LabFiles) {
                    tabControl.Items.Add (FileTab.GetInstance (Properties.Settings.Default.LabsPath + file));
                }
                tabControl.SelectedIndex = 0;
                tabControl.Focus ();
                startButton.IsEnabled = true;
            } else {
                startButton.IsEnabled = false;
            }
        }

        private void PrintMakeLog (string s) {
            logControl.SelectedIndex = 0;
            buildText.AppendText (s);
            buildText.AppendText ("\n");
            buildText.ScrollToEnd ();
        }

        private void PrintOutput (string s) {
            logControl.SelectedIndex = 1;
            outputText.Text = s;
            outputText.ScrollToEnd ();
        }

        private void PrintGDBLog (string s) {
            if (s == null || s.IndexOf ("~") != -1) return;
            logControl.SelectedIndex = 2;
            gdbText.AppendText (s);
            gdbText.AppendText ("\n");
            gdbText.ScrollToEnd ();
        }

        private void SetRunButton (bool enabled) {
            startButton.IsEnabled = enabled;
            stepButton.IsEnabled = enabled;
            nextButton.IsEnabled = enabled;
            finishButton.IsEnabled = enabled;
        }

        private void CorrectBreakPoint (Action AfterCorrectBreakPoint) {
            List<(FileTab, string, int)> lines = new List<(FileTab, string, int)> ();
            foreach (FileTab tab in tabControl.Items) {
                foreach (int line in tab.breakPoints.Keys) {
                    lines.Add ((tab, tab.Header as string, line));
                }
            }

            gdb.SetBreakpoints (
                lines,
                (r, tab, line) => Dispatcher.BeginInvoke ((Action)(
                    () => {
                        if (r.HasValue) {
                            if (line != r.Value.Item2) {
                                tab.UnBreakPointLine (line);
                                tab.BreakPointLine (r.Value.Item2);
                            }
                        } else {
                            tab.UnBreakPointLine (line);
                        }
                    }
                )),
                AfterCorrectBreakPoint
            );
        }

        private void Window_Closed (object sender, EventArgs e) {
            StopButton_Click (sender, null);
        }
    }
}