using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

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
        private readonly MainWindowDataContext dataContext;

        public MainWindow () {
            dataContext = new MainWindowDataContext () {
                StartButtonEnable = true,
                StartButtonContent = "启动",
                LabSelectEnable = true,
            };
            DataContext = dataContext;
            InitializeComponent ();
            System.Text.Encoding.RegisterProvider (System.Text.CodePagesEncodingProvider.Instance);
            GDB.PrintLog = s => Dispatcher.BeginInvoke ((Action<string>)PrintGDBLog, s);
            GDB.PrintLog = s => Debug.WriteLine (s);
            GDB.PrintLog = s => { };
            GDB.AfterRun = (s) => Dispatcher.BeginInvoke ((Action<string>)AfterRun, s);
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
                        textBlocks.Add (tb);
                        dataStructureView.Inlines.Add (tb);
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
                gdb.ClearBreakpoint (cp.Item1, cp.Item2);
            }
        }

        private void AfterBuild (bool buildOk) {
            if (buildOk) {
                gdb = lab.GetGDB ();
                gdb.Start ();
                CorrectBreakPoint (() => {
                    Dispatcher.BeginInvoke ((Action)(() => {
                        run = true;
                        dataContext.BeforeRun ();
                    }));
                    gdb.Run ();
                });
            } else {
                dataContext.BuildFail ();
                logControl.SelectedIndex = 0;
            }
        }

        private void AfterRun (string result) {
            if (result != null && result.IndexOf ("^error") != -1) {
                dataContext.BreakPoint ();
                return;
            }
            if (lastStopTab != null) {
                lastStopTab.UnRunMarkLine ();
            }
            if (result == null) {
                dataContext.Finish ();
                run = false;
                gdb.Stop ();
                gdb = null;
                PrintGDBLog (result);
                PrintOutput (File.ReadAllText (Properties.Settings.Default.LabsPath + "out.txt", System.Text.Encoding.GetEncoding ("GB2312")));
            } else {
                if (result.IndexOf ("breakpoint-hit") != -1
                    || result.IndexOf ("end-stepping-range") != -1
                    || result.IndexOf ("function-finished") != -1) {
                    /*
                        List<Lab.Lab.Rule> rules = lab4.GetRules("pHead");
                        Lab1.VoidTable voidTable = lab4.GetVoidTable("&VoidTable");
                        List<Lab2.Set> firstSet = lab4.GetSetList ("&FirstSetList");
                        List<Lab2.Set> followSet = lab4.GetSetList ("&FollowSetList");
                        List<Lab4.SelectSet> selectSet = lab4.GetSelectSetList ("&SelectSetList");
                        Lab4.ParsingTable parsingTable = lab4.GetParsingTable ("&ParsingTable");
                        List<Lab.Lab.Symbol> stack = lab8.GetParsingStack("&Stack");
                    */
                    dataContext.BreakPoint ();
                    var b = BreakPoint.Parse (result);
                    if (b.HasValue) {
                        lastStopTab = FileTab.GetInstance (Path.GetFileName (b.Value.Item1));
                        tabControl.SelectedItem = lastStopTab;
                        lastStopLine = b.Value.Item2;
                        lastStopTab.GotoLine (lastStopLine);
                        lastStopTab.RunMarkLine (lastStopLine);
                    }
                    lab.Draw ();
                } else {
                    dataContext.Finish ();
                    run = false;
                    gdb.Stop ();
                    gdb = null;
                    PrintGDBLog (result);
                    PrintOutput (File.ReadAllText (Properties.Settings.Default.LabsPath + "out.txt", System.Text.Encoding.GetEncoding ("GB2312")));
                }
            }
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
                dataContext.StartButtonEnable = true;
            } else {
                dataContext.StartButtonEnable = false;
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
            Stop_Executed (null, null);
        }

        private void StartOrContinue_CanExecute (object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = dataContext.StartButtonEnable;
        }

        private void StartOrContinue_Executed (object sender, ExecutedRoutedEventArgs e) {
            if (run) {
                dataContext.SetRunButton (false);
                gdb.Continue ();
            } else {
                textBlocks.Clear ();
                dataStructureView.Inlines.Clear ();
                AbstractLab.DataHash.Clear ();
                dataContext.Start ();
                lab.Build ();
            }
        }

        private void Stop_CanExecute (object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = dataContext.StopButtonEnable;
        }

        private void Stop_Executed (object sender, ExecutedRoutedEventArgs e) {
            if (gdb != null) {
                gdb.Stop ();
                gdb = null;
            }
            run = false;
            dataContext.Finish ();
            if (lastStopTab != null) {
                lastStopTab.UnRunMarkLine ();
                lastStopTab = null;
            }
        }

        private void Step_CanExecute (object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = dataContext.StepButtonEnable;
        }

        private void Step_Executed (object sender, ExecutedRoutedEventArgs e) {
            dataContext.SetRunButton (false);
            gdb.Step ();
        }

        private void Next_CanExecute (object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = dataContext.NextButtonEnable;
        }

        private void Next_Executed (object sender, ExecutedRoutedEventArgs e) {
            dataContext.SetRunButton (false);
            gdb.Next ();
        }

        private void Finish_CanExecute (object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = dataContext.FinishButtonEnable;
        }

        private void Finish_Executed (object sender, ExecutedRoutedEventArgs e) {
            dataContext.SetRunButton (false);
            gdb.Finish ();
        }

        private void StartButton_Click (object sender, RoutedEventArgs e) {
            StartOrContinue_Executed (null, null);
        }

        private void StopButton_Click (object sender, RoutedEventArgs e) {
            Stop_Executed (null, null);
        }

        private void StepButton_Click (object sender, RoutedEventArgs e) {
            Step_Executed (null, null);
        }

        private void NextButton_Click (object sender, RoutedEventArgs e) {
            Next_Executed (null, null);
        }

        private void FinishButton_Click (object sender, RoutedEventArgs e) {
            Finish_Executed (null, null);
        }

        private void Save_CanExecute (object sender, CanExecuteRoutedEventArgs e) {
        }

        private void Save_Executed (object sender, ExecutedRoutedEventArgs e) {
        }

        private void SaveAll_CanExecute (object sender, CanExecuteRoutedEventArgs e) {
        }

        private void SaveAll_Executed (object sender, ExecutedRoutedEventArgs e) {
        }

        private void Option_CanExecute (object sender, CanExecuteRoutedEventArgs e) {
        }

        private void Option_Executed (object sender, ExecutedRoutedEventArgs e) {
        }

        private void Exit_CanExecute (object sender, CanExecuteRoutedEventArgs e) {
        }

        private void Exit_Executed (object sender, ExecutedRoutedEventArgs e) {
        }

        private void About_CanExecute (object sender, CanExecuteRoutedEventArgs e) {
        }

        private void About_Executed (object sender, ExecutedRoutedEventArgs e) {
        }
    }

    internal class MainWindowDataContext: INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _startButtonEnable;
        private bool _stopButtonEnable;
        private bool _stepButtonEnable;
        private bool _nextButtonEnable;
        private bool _finishButtonEnable;
        private string _startButtonContent;
        private bool _labSelectEnable;

        public bool StartButtonEnable {
            get => _startButtonEnable;
            set {
                if (SetProperty (ref _startButtonEnable, value)) {
                    PropertyChanged?.Invoke (this, new PropertyChangedEventArgs ("StartButtonImage"));
                }
            }
        }

        public string StartButtonImage {
            get {
                if (_startButtonEnable) {
                    return "image/debug-continue.png";
                } else {
                    return "image/debug-continue-disabled.png";
                }
            }
        }

        public bool StopButtonEnable {
            get => _stopButtonEnable;
            set {
                if (SetProperty (ref _stopButtonEnable, value)) {
                    PropertyChanged?.Invoke (this, new PropertyChangedEventArgs ("StopButtonImage"));
                }
            }
        }

        public string StopButtonImage {
            get {
                if (_stopButtonEnable) {
                    return "image/debug-stop.png";
                } else {
                    return "image/debug-stop-disabled.png";
                }
            }
        }

        public bool StepButtonEnable {
            get => _stepButtonEnable;
            set {
                if (SetProperty (ref _stepButtonEnable, value)) {
                    PropertyChanged?.Invoke (this, new PropertyChangedEventArgs ("StepButtonImage"));
                }
            }
        }

        public string StepButtonImage {
            get {
                if (_stepButtonEnable) {
                    return "image/debug-step-into.png";
                } else {
                    return "image/debug-step-into-disabled.png";
                }
            }
        }

        public bool NextButtonEnable {
            get => _nextButtonEnable;
            set {
                if (SetProperty (ref _nextButtonEnable, value)) {
                    PropertyChanged?.Invoke (this, new PropertyChangedEventArgs ("NextButtonImage"));
                }
            }
        }

        public string NextButtonImage {
            get {
                if (_nextButtonEnable) {
                    return "image/debug-step-over.png";
                } else {
                    return "image/debug-step-over-disabled.png";
                }
            }
        }

        public bool FinishButtonEnable {
            get => _finishButtonEnable;
            set {
                if (SetProperty (ref _finishButtonEnable, value)) {
                    PropertyChanged?.Invoke (this, new PropertyChangedEventArgs ("FinishButtonImage"));
                }
            }
        }

        public string FinishButtonImage {
            get {
                if (_finishButtonEnable) {
                    return "image/debug-step-out.png";
                } else {
                    return "image/debug-step-out-disabled.png";
                }
            }
        }

        public string StartButtonContent {
            get => _startButtonContent;
            set {
                if (SetProperty (ref _startButtonContent, value)) {
                    PropertyChanged?.Invoke (this, new PropertyChangedEventArgs ("StartMenuContent"));
                }
            }
        }

        public string StartMenuContent { get => _startButtonContent == "启动" ? "启动(_S)" : "继续(_C)"; }
        public bool LabSelectEnable { get => _labSelectEnable; set => SetProperty (ref _labSelectEnable, value); }

        private bool SetProperty<T> (ref T field, T newValue, [CallerMemberName] string propertyName = null) {
            if (!Equals (field, newValue)) {
                field = newValue;
                PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
                return true;
            }

            return false;
        }

        public void SetRunButton (bool enabled) {
            StartButtonEnable = enabled;
            StepButtonEnable = enabled;
            NextButtonEnable = enabled;
            FinishButtonEnable = enabled;
        }

        public void Start () {
            StartButtonEnable = false;
            LabSelectEnable = false;
        }

        public void BeforeRun () {
            StartButtonEnable = true;
            StartButtonContent = "继续";
            StopButtonEnable = true;
        }

        public void BuildFail () {
            StartButtonEnable = true;
            LabSelectEnable = true;
        }

        public void BreakPoint () {
            StartButtonEnable = true;
            StepButtonEnable = true;
            NextButtonEnable = true;
            FinishButtonEnable = true;
        }

        public void Finish () {
            StartButtonEnable = true;
            StepButtonEnable = false;
            NextButtonEnable = false;
            FinishButtonEnable = false;
            StopButtonEnable = false;
            LabSelectEnable = true;
            StartButtonContent = "启动";
        }
    }
}