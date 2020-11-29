using CPP_EP.Execute;
using CPP_EP.View;
using ScintillaNET;
using ScintillaNET.WPF;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CPP_EP {
    using RunResult = ValueTuple<string, string>;
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow: Window {
        private readonly MenuItem[] itemLabs;

        private const int NUMBER_MARGIN = 2;

        private const int BOOKMARK_MARGIN = 1;

        private const int BOOKMARK_MARKER = 1;

        private const int LINE_NUMBERS_MARGIN_WIDTH = 30; // TODO - don't hardcode this

        private bool run = false;

        private GDB gdb = null;
        public MainWindow () {
            InitializeComponent ();
            itemLabs = new MenuItem[] { itemLab1, itemLab2, itemLab3, itemLab4, itemLab5, itemLab6, itemLab7, itemLab8 };
        }
        public DocumentForm ActiveDocument {
            get { return documentsRoot.Children.FirstOrDefault (c => c.Content == dockPanel.ActiveContent) as DocumentForm; }
        }
        private void Window_Loaded (object sender, RoutedEventArgs e) {

        }
        private DocumentForm OpenFile (string filePath) {
            DocumentForm doc = new DocumentForm();
            SetScintillaToCurrentOptions (doc);
            doc.Scintilla.Text = File.ReadAllText (filePath);
            //doc.Scintilla.UndoRedo.EmptyUndoBuffer();
            //doc.Scintilla.Modified = false;
            doc.Title = Path.GetFileName (filePath);
            doc.FilePath = filePath;
            documentsRoot.Children.Add (doc);
            doc.DockAsDocument ();
            //doc.IsActive = true;
            doc.CanClose = false;
            doc.CanTogglePin = false;
            //incrementalSearcher.Scintilla = doc.Scintilla;

            return doc;
        }
        private void ItemLab_Click (object sender, RoutedEventArgs e) {
            foreach (var itemLab in itemLabs) {
                if (itemLab != sender) {
                    itemLab.IsChecked = false;
                } else {
                    itemLab.IsChecked = true;
                    Clean ();
                    while (documentsRoot.Children.Count () != 0) {
                        documentsRoot.Children[0].Close ();
                    }
                    if (sender == itemLab1) {
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\lab1.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\rule.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\voidtable.c");
                    } else if (sender == itemLab2) {
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\lab2.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\rule.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\voidtable.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\first.c");
                    } else if (sender == itemLab3) {
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\lab3.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\rule.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\voidtable.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\first.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\follow.c");
                    } else if (sender == itemLab4) {
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\lab4.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\rule.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\voidtable.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\first.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\follow.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\parsingtable.c");
                    } else if (sender == itemLab5) {
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\lab5.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\rule.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\pickupleftfactor.c");
                    } else if (sender == itemLab6) {
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\lab6.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\rule.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\removeleftrecursion1.c");
                    } else if (sender == itemLab7) {
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\lab7.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\rule.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\removeleftrecursion2.c");
                    } else if (sender == itemLab8) {
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\lab8.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\rule.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\voidtable.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\first.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\follow.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\parsingtable.c");
                        OpenFile ("C:\\Users\\li-fs\\Documents\\labs\\src\\inc\\parser.c");
                    }
                }
            }
            
        }
        private void SetScintillaToCurrentOptions (DocumentForm doc) {
            ScintillaWPF ScintillaNet = doc.Scintilla;
            //ScintillaNet.KeyDown += ScintillaNet_KeyDown;

            // INITIAL VIEW CONFIG
            ScintillaNet.WrapMode = WrapMode.None;
            ScintillaNet.IndentationGuides = IndentView.LookBoth;

            // STYLING
            InitSyntaxColoring (ScintillaNet);

            // NUMBER MARGIN
            InitNumberMargin (ScintillaNet);

            // BOOKMARK MARGIN
            InitBookmarkMargin (ScintillaNet);

            doc.Scintilla.WrapMode = WrapMode.Word;


            ScintillaNet.CaretLineVisible = true;
            ScintillaNet.CaretLineBackColor = System.Windows.Media.Color.FromRgb (0, 0, 100);
            ScintillaNet.CaretLineBackColorAlpha = 100;

            
            // Set the zoom
            //doc.Scintilla.Zoom = _zoomLevel;
        }
        private void InitSyntaxColoring (ScintillaWPF ScintillaNet) {
            // Configure the default style
            ScintillaNet.StyleResetDefault ();
            ScintillaNet.Styles[ScintillaNET.Style.Default].Font = "Consolas";
            ScintillaNet.Styles[ScintillaNET.Style.Default].Size = 10;
            ScintillaNet.StyleClearAll ();

            // Configure the CPP (C#) lexer styles
            ScintillaNet.Styles[ScintillaNET.Style.Cpp.Default].ForeColor = Color.Silver;
            ScintillaNet.Styles[ScintillaNET.Style.Cpp.Comment].ForeColor = Color.FromArgb (0, 128, 0); // Green
            ScintillaNet.Styles[ScintillaNET.Style.Cpp.CommentLine].ForeColor = Color.FromArgb (0, 128, 0); // Green
            ScintillaNet.Styles[ScintillaNET.Style.Cpp.CommentLineDoc].ForeColor = Color.FromArgb (128, 128, 128); // Gray
            ScintillaNet.Styles[ScintillaNET.Style.Cpp.Number].ForeColor = Color.Olive;
            ScintillaNet.Styles[ScintillaNET.Style.Cpp.Word].ForeColor = Color.Blue;
            ScintillaNet.Styles[ScintillaNET.Style.Cpp.Word2].ForeColor = Color.DarkCyan;
            ScintillaNet.Styles[ScintillaNET.Style.Cpp.String].ForeColor = Color.FromArgb (163, 21, 21); // Red
            ScintillaNet.Styles[ScintillaNET.Style.Cpp.Character].ForeColor = Color.FromArgb (163, 21, 21); // Red
            ScintillaNet.Styles[ScintillaNET.Style.Cpp.Verbatim].ForeColor = Color.FromArgb (163, 21, 21); // Red
            ScintillaNet.Styles[ScintillaNET.Style.Cpp.StringEol].BackColor = Color.Pink;
            ScintillaNet.Styles[ScintillaNET.Style.Cpp.Operator].ForeColor = Color.Purple;
            ScintillaNet.Styles[ScintillaNET.Style.Cpp.Preprocessor].ForeColor = Color.Maroon;

            ScintillaNet.Lexer = Lexer.Cpp;

            ScintillaNet.SetKeywords (0, "NULL break case const continue default do else enum extern for goto if long return signed sizeof static struct switch typedef union unsigned while char double float int long short void");
            ScintillaNet.SetKeywords (1, "Rule RuleSymbol Set SetList ParsingStack VoidTable pHead pRule pSelect pSymbol");
        }
        private void InitNumberMargin (ScintillaWPF ScintillaNet) {
            var nums = ScintillaNet.Margins[NUMBER_MARGIN];
            nums.Width = LINE_NUMBERS_MARGIN_WIDTH;
            nums.Type = MarginType.Number;
            nums.Sensitive = true;
            nums.Mask = 0;

        }
        private void InitBookmarkMargin (ScintillaWPF ScintillaNet) {
            var margin = ScintillaNet.Margins[BOOKMARK_MARGIN];
            margin.Width = 16;
            margin.Sensitive = true;
            margin.Type = MarginType.Symbol;
            margin.Mask = (1 << BOOKMARK_MARKER);

            var marker = ScintillaNet.Markers[BOOKMARK_MARKER];
            marker.Symbol = MarkerSymbol.Circle;
            marker.SetBackColor (IntToColor (0xFF003B));
            marker.SetForeColor (IntToColor (0x000000));
            marker.SetAlpha (100);
            ScintillaNet.MarginClick += TextArea_MarginClick;
        }
        private void TextArea_MarginClick (object sender, MarginClickEventArgs e) {
            ScintillaWPF TextArea = ActiveDocument.Scintilla;
            if (e.Margin == BOOKMARK_MARGIN) {
                const uint mask = (1 << BOOKMARK_MARKER);
                Line line = null;
                if (gdb != null) {
                    var b = gdb.SetBreakpoint (ActiveDocument.Title + ":" + (TextArea.LineFromPosition(e.Position) + 1));
                    if (b.HasValue) {
                        line = TextArea.Lines[b.Value.Line - 1];
                    }
                } else {
                    line = TextArea.Lines[TextArea.LineFromPosition (e.Position)];
                }
                // Do we have a marker for this line?
                if (line != null) {
                    if ((line.MarkerGet () & mask) > 0) {
                        // Remove existing bookmark
                        line.MarkerDelete (BOOKMARK_MARKER);
                    } else {
                        // Add bookmark
                        line.MarkerAdd (BOOKMARK_MARKER);
                    }
                }
            }
        }
        public static Color IntToColor (int rgb) {
            return Color.FromArgb (255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        private void startButton_Click (object sender, RoutedEventArgs e) {
            startButton.IsEnabled = false;
            if (run) {
                AfterRun (gdb.Continue ());
            } else {
                Clean ();
                for (int i = 0; i < itemLabs.Count (); i++) {
                    if (itemLabs[i].IsChecked) {
                        new Xmake ("C:\\Users\\li-fs\\Documents\\labs", "D:\\xmake\\xmake.exe", i + 1).Build (buildText);
                        gdb = new GDB ("D:\\MinGW\\bin\\gdb.exe", "C:\\Users\\li-fs\\Documents\\labs\\build\\lab" + (i + 1) + ".exe");
                        run = true;
                        SetBreakpoint ();
                        AfterRun (gdb.Run ());
                        break;
                    }
                }
            }
            startButton.IsEnabled = true;
        }

        private void SetBreakpoint() {
            foreach (DocumentForm doc in documentsRoot.Children) {
                int line = 0;
                while (true) {
                    line = doc.Scintilla.Lines[line + 1].MarkerNext (1 << BOOKMARK_MARKER);
                    if (line == -1) {
                        break;
                    } else {
                        var r = gdb.SetBreakpoint (doc.Title, line + 1);
                        if (r.HasValue) {
                            if (r.Value.Line != line + 1) {
                                Console.WriteLine (line);
                                doc.Scintilla.Lines[line].MarkerDelete (BOOKMARK_MARKER);
                                doc.Scintilla.Lines[r.Value.Line - 1].MarkerAdd (BOOKMARK_MARKER);
                            }
                        }
                    }
                }
            }
        }
        private void AfterRun(RunResult r) {
            stepButton.IsEnabled = false;
            nextButton.IsEnabled = false;
            finishButton.IsEnabled = false;
            stopButton.IsEnabled = true;
            if (r.Item2 == null) {

            } else {
                if (r.Item2.IndexOf ("breakpoint-hit") != -1
                    || r.Item2.IndexOf ("end-stepping-range") != -1
                    || r.Item2.IndexOf ("function-finished") != -1) {
                    var lab4 = new Lab4(gdb);
                    List<Lab.Rule> rules = lab4.GetRules("pHead");
                    Lab1.VoidTable voidTable = lab4.GetVoidTable("&VoidTable");
                    List<Lab2.Set> firstSet = lab4.GetSetList ("&FirstSetList");
                    List<Lab2.Set> followSet = lab4.GetSetList ("&FollowSetList");
                    List<Lab4.SelectSet> selectSet = lab4.GetSelectSetList ("&SelectSetList");
                    Lab4.ParsingTable parsingTable = lab4.GetParsingTable ("&ParsingTable");
                    foreach (DocumentForm doc in documentsRoot.Children) {
                        var b = BreakPoint.Parse(r.Item2);
                        if (b.HasValue) {
                            if (doc.Title == Path.GetFileName (b.Value.Filename)) {
                                doc.IsActive = true;
                                doc.Scintilla.Lines[b.Value.Line - 1].Goto ();
                                doc.Scintilla.Scintilla.Focus ();

                                stepButton.IsEnabled = true;
                                nextButton.IsEnabled = true;
                                finishButton.IsEnabled = true;
                                break;
                            }
                        }
                    }
                } else {
                    run = false;
                    Console.WriteLine (r.Item2);
                }
            }
        }

        private void stepButton_Click (object sender, RoutedEventArgs e) {
            AfterRun (gdb.Step ());
        }

        private void nextButton_Click (object sender, RoutedEventArgs e) {
            AfterRun (gdb.Next());
        }

        private void finishButton_Click (object sender, RoutedEventArgs e) {
            AfterRun (gdb.Finish());
        }

        private void Clean() {
            if (gdb != null) {
                gdb.Stop ();
                gdb = null;
            }
            run = false;
            startButton.IsEnabled = true;
            refButton.IsEnabled = false;
            stopButton.IsEnabled = false;
            stepButton.IsEnabled = false;
            nextButton.IsEnabled = false;
            finishButton.IsEnabled = false;
        }

        private void stopButton_Click (object sender, RoutedEventArgs e) {
            Clean ();
        }
    }
}
