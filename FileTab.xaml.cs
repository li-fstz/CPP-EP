

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;

namespace CPP_EP {
    using CodePosition = ValueTuple<string, int>;
    public partial class FileTab: TabItem {
        private static readonly Dictionary<string, FileTab> fileTabHash = new Dictionary<string, FileTab> ();

        private readonly string FilePath;
        public Dictionary<int, CodePosition> breakPoints = new Dictionary<int, CodePosition>();
        public static Action<CodePosition, Action<CodePosition?>> SetBreakPoint { private get; set; }
        public static Action<CodePosition> DeleteBreakPoint { private get; set; }
        public double breakPointAreaWidth;
        private int runMarkLine = -1;
        private HighlightCurrentLineBackgroundRenderer backgroundRenderer;
        public static FileTab GetInstance (string filepath) {
            if (fileTabHash.ContainsKey (filepath)) {
                return fileTabHash[filepath];
            } else {
                foreach (var key in fileTabHash.Keys) {
                    if (key.EndsWith (filepath)) {
                        return fileTabHash[key];
                    }
                }
                var tab = new FileTab (filepath);
                fileTabHash[filepath] = tab;
                return tab;
            }
        }
        private FileTab (string filepath) {
            InitializeComponent ();
            FilePath = filepath;
            Header = System.IO.Path.GetFileName (filepath);
            ReloadFile ();
            textEditor.TextArea.TextView.VisualLinesChanged += TextView_VisualLinesChanged;
            breakPointAreaWidth = textEditor.TextArea.TextView.DefaultLineHeight;
            breakPointGrid.Width = new GridLength (breakPointAreaWidth);
            backgroundRenderer = new HighlightCurrentLineBackgroundRenderer (textEditor);
            textEditor.TextArea.TextView.BackgroundRenderers.Add (backgroundRenderer);
            
        }

        private void TextView_VisualLinesChanged (object sender, EventArgs e) {
            DrawAllBreakPoint ();
        }

        private void ReloadFile () {
            textEditor.Text = File.ReadAllText (FilePath);
        }

        private void BreakPointArea_MouseUp (object sender, MouseButtonEventArgs e) {
            //textEditor.Text += e.GetPosition (breakPointArea).Y;
            var y = e.GetPosition (breakPointArea).Y;
            var visualLine = textEditor.TextArea.TextView.VisualLines.FirstOrDefault (vl => vl.GetTextLineVisualYPosition(vl.TextLines[0], VisualYPosition.LineBottom) - textEditor.TextArea.TextView.VerticalOffset > y);
            var lineNumber = visualLine.FirstDocumentLine.LineNumber;
            if (breakPoints.ContainsKey(lineNumber)) {
                breakPoints.Remove (lineNumber);
                DeleteBreakPoint (new CodePosition ((string)Header, lineNumber));
                DrawAllBreakPoint ();
            } else {
                SetBreakPoint (new CodePosition ((string)Header, lineNumber), r => {
                    if (r.HasValue && !breakPoints.ContainsKey (r.Value.Item2)) {
                        breakPoints.Add (r.Value.Item2, r.Value);
                        DrawAllBreakPoint ();
                    }
                });
            }
        }

        private void DrawAllBreakPoint () {
            if (textEditor.TextArea.TextView.VisualLinesValid && textEditor.TextArea.TextView.VisualLines.Count > 0) {
                breakPointArea.Children.Clear ();
                foreach (var vl in textEditor.TextArea.TextView.VisualLines) {
                    if (breakPoints.ContainsKey(vl.FirstDocumentLine.LineNumber)) {
                        breakPointArea.Children.Add (GenCircle (vl));
                        if (vl.FirstDocumentLine.LineNumber == runMarkLine) {
                            breakPointArea.Children.Add (GenRectangle (vl));
                        }
                    } 
                    if (vl.FirstDocumentLine.LineNumber == runMarkLine) {
                        breakPointArea.Children.Add (GenArrow (vl));
                    }
                }
            }
        }


        public void BreakPointLine (int line) {
            if (!breakPoints.ContainsKey(line)) {
                breakPoints.Add (line, new CodePosition ((string)Header, line));
                DrawAllBreakPoint ();
            }
        }
        public void UnBreakPointLine (int line) {
            if (breakPoints.Remove (line)) {
                DrawAllBreakPoint ();
            }
        }
        
        public void GotoLine (int line) {
            textEditor.ScrollToLine (line);
        }
        private Ellipse GenCircle (VisualLine visualLine) {
            Ellipse el = new Ellipse {
                Fill = new SolidColorBrush (Color.FromRgb(255, 80, 65)),
                Width = breakPointAreaWidth - 2,
                Height = breakPointAreaWidth - 2
            };
            el.SetValue (Canvas.LeftProperty, 1.0);
            el.SetValue (Canvas.TopProperty, visualLine.GetTextLineVisualYPosition (visualLine.TextLines[0], VisualYPosition.LineTop) - textEditor.TextArea.TextView.VerticalOffset + 1);
            el.SetValue (Canvas.ZIndexProperty, 2);
            return el;
        }
        private Rectangle GenRectangle (VisualLine visualLine) {
            Rectangle r = new Rectangle {
                Fill = new SolidColorBrush (Colors.Green),
                Width = breakPointAreaWidth,
                Height = breakPointAreaWidth
            };
            r.SetValue (Canvas.ZIndexProperty, 1);
            r.SetValue (Canvas.TopProperty, visualLine.GetTextLineVisualYPosition (visualLine.TextLines[0], VisualYPosition.LineTop) - textEditor.TextArea.TextView.VerticalOffset);
            return r;
        }
        private Polygon GenArrow (VisualLine visualLine) {
            Polygon p = new Polygon {
                Fill = new SolidColorBrush (Color.FromRgb(0, 176, 80)),
                Points = new PointCollection (new Point[] { new Point(3.5, 0), new Point (breakPointAreaWidth, breakPointAreaWidth / 2), new Point (3.5, breakPointAreaWidth) })
            };
            p.SetValue (Canvas.ZIndexProperty, 3);
            p.SetValue (Canvas.TopProperty, visualLine.GetTextLineVisualYPosition (visualLine.TextLines[0], VisualYPosition.LineTop) - textEditor.TextArea.TextView.VerticalOffset);
            return p;
        }
        public void RunMarkLine (int line) {
            runMarkLine = line;
            backgroundRenderer.LineNumber = line;
            DrawAllBreakPoint ();
            textEditor.TextArea.TextView.Redraw ();
        }
        public void UnRunMarkLine () {
            runMarkLine = 0;
            backgroundRenderer.LineNumber = 0;
            DrawAllBreakPoint ();
            textEditor.TextArea.TextView.Redraw ();
        }
        public class HighlightCurrentLineBackgroundRenderer: IBackgroundRenderer {
            private TextEditor _editor;
            public int LineNumber { get; set; } /* DiffPlex model's lines */

            public HighlightCurrentLineBackgroundRenderer (TextEditor editor) {
                _editor = editor;
            }

            public KnownLayer Layer {
                get { return KnownLayer.Background; }
            }

            public void Draw (TextView textView, DrawingContext drawingContext) {
                if (_editor.Document == null || LineNumber == 0)
                    return;

                textView.EnsureVisualLines ();
                var highlight = new SolidColorBrush(Color.FromArgb (100, 0, 176, 80));
                var currentLine = _editor.Document.GetLineByNumber (LineNumber);
                
                foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment (textView, currentLine)) {
                    drawingContext.DrawRectangle (highlight, null, new Rect (rect.Location, new Size (9999, rect.Height)));
                    break;
                }
            }
        }
    }
}

