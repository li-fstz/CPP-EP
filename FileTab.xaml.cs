using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Rendering;

namespace CPP_EP {

    using CodePosition = ValueTuple<string, int>;

    public partial class FileTab: TabItem {
        private static readonly Dictionary<string, FileTab> fileTabHash = new Dictionary<string, FileTab> ();

        private readonly string FilePath;
        public Dictionary<int, CodePosition> breakPoints = new Dictionary<int, CodePosition> ();
        public static Action<CodePosition, Action<CodePosition?>> SetBreakPoint { private get; set; }
        public static Action<CodePosition> DeleteBreakPoint { private get; set; }
        public double breakPointAreaWidth;
        private int runMarkLine = -1;
        private readonly HighlightCurrentLineBackgroundRenderer backgroundRenderer;
        private readonly FileTabDataContext dataContext;
        private FoldingManager mFoldingManager;
        private BraceFoldingStrategy mFoldingStrategy = null;

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
            dataContext = new FileTabDataContext ();
            DataContext = dataContext;
            InitializeComponent ();
            FilePath = filepath;
            Header = System.IO.Path.GetFileName (filepath);
            ReloadFile ();
            textEditor.TextArea.TextView.VisualLinesChanged += TextView_VisualLinesChanged;
            textEditor.PreviewMouseWheel += TextEditor_PreviewMouseWheel;
            textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            breakPointAreaWidth = textEditor.TextArea.TextView.DefaultLineHeight;
            breakPointGrid.Width = new GridLength (breakPointAreaWidth);
            backgroundRenderer = new HighlightCurrentLineBackgroundRenderer (textEditor, breakPointArea);
            textEditor.TextArea.TextView.BackgroundRenderers.Add (backgroundRenderer);
            textEditor.Options.AllowToggleOverstrikeMode = true;
            textEditor.Options.ShowSpaces = true;
            textEditor.Options.ShowTabs = true;

            //textEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy ();
        }

        private void Caret_PositionChanged (object sender, EventArgs e) {
            if (textEditor.TextArea != null) {
                dataContext.Col = textEditor.TextArea.Caret.Column.ToString ();
                dataContext.Row = textEditor.TextArea.Caret.Line.ToString ();
            }
        }

        private void TextEditor_PreviewMouseWheel (object sender, MouseWheelEventArgs e) {
            if (Keyboard.Modifiers == ModifierKeys.Control) {
                double fontSize = textEditor.FontSize + e.Delta / 25.0;

                if (fontSize < 6) {
                    textEditor.FontSize = 6;
                } else {
                    if (fontSize > 200) {
                        textEditor.FontSize = 200;
                    } else {
                        textEditor.FontSize = fontSize;
                    }
                }

                e.Handled = true;
            }
        }

        private void TextView_VisualLinesChanged (object sender, EventArgs e) {
            DrawAllBreakPoint ();
        }

        private void ReloadFile () {
            textEditor.Text = File.ReadAllText (FilePath);
            mFoldingManager = FoldingManager.Install (textEditor.TextArea);
            mFoldingStrategy = new BraceFoldingStrategy ();
            mFoldingStrategy.UpdateFoldings (mFoldingManager, textEditor.Document);
        }

        private void BreakPointArea_MouseUp (object sender, MouseButtonEventArgs e) {
            //textEditor.Text += e.GetPosition (breakPointArea).Y;
            var y = e.GetPosition (breakPointArea).Y;
            var visualLine = textEditor.TextArea.TextView.VisualLines.FirstOrDefault (vl => vl.GetTextLineVisualYPosition (vl.TextLines[0], VisualYPosition.LineBottom) - textEditor.TextArea.TextView.VerticalOffset > y);
            if (visualLine == null) {
                return;
            }
            var lineNumber = visualLine.FirstDocumentLine.LineNumber;
            if (breakPoints.ContainsKey (lineNumber)) {
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
                    if (breakPoints.ContainsKey (vl.FirstDocumentLine.LineNumber)) {
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
            if (!breakPoints.ContainsKey (line)) {
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
                Fill = new SolidColorBrush (Color.FromRgb (255, 80, 65)),
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
                Fill = new SolidColorBrush (Color.FromRgb (0, 176, 80)),
                Points = new PointCollection (new Point[] { new Point (3.5, 0), new Point (breakPointAreaWidth, breakPointAreaWidth / 2), new Point (3.5, breakPointAreaWidth) })
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
    }

    public class HighlightCurrentLineBackgroundRenderer: IBackgroundRenderer {
        private readonly TextEditor _editor;
        private readonly Canvas _breakPointArea;
        public int LineNumber { get; set; } /* DiffPlex model's lines */

        public HighlightCurrentLineBackgroundRenderer (TextEditor editor, Canvas breakPointArea) {
            _editor = editor;
            _breakPointArea = breakPointArea;
        }

        public KnownLayer Layer {
            get { return KnownLayer.Background; }
        }

        public void Draw (TextView textView, DrawingContext drawingContext) {
            if (_editor.Document == null || LineNumber == 0)
                return;

            textView.EnsureVisualLines ();
            var highlight = new SolidColorBrush (Color.FromArgb (100, 0, 176, 80));
            var currentLine = _editor.Document.GetLineByNumber (LineNumber);

            foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment (textView, currentLine)) {
                drawingContext.DrawRectangle (highlight, null, new Rect (new Point (0, rect.Location.Y), new Size (9999, rect.Height)));
                //Debug.WriteLine (rect);
                _breakPointArea.Children.Add (GenArrow (rect.Height, rect.Location.Y));
                break;
            }
        }

        private Polygon GenArrow (double h, double y) {
            Polygon p = new Polygon {
                Fill = new SolidColorBrush (Color.FromRgb (0, 176, 80)),
                Points = new PointCollection (new Point[] { new Point (3.5, 0), new Point (h, h / 2), new Point (3.5, h) })
            };
            p.SetValue (Canvas.ZIndexProperty, 3);
            p.SetValue (Canvas.TopProperty, y);
            return p;
        }
    }

    internal class FileTabDataContext: INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        private string _row;
        private string _col;

        public string Row { get => "行: " + _row; set => SetProperty (ref _row, value); }

        public string Col { get => "列: " + _col; set => SetProperty (ref _col, value); }

        private bool SetProperty<T> (ref T field, T newValue, [CallerMemberName] string propertyName = null) {
            if (!Equals (field, newValue)) {
                field = newValue;
                PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
                return true;
            }

            return false;
        }
    }

    [ValueConversion (typeof (bool), typeof (string))]
    public sealed class BoolToStringPropConverter: IValueConverter {

        #region constructor

        /// <summary>
        /// Class constructor
        /// </summary>
        public BoolToStringPropConverter () {
            // set defaults
            TrueValue = "True";
            FalseValue = "False";
        }

        #endregion constructor

        #region properties

        /// <summary>
        /// Gets/sets the <see cref="Visibility"/> value that is associated
        /// (converted into) with the boolean true value.
        /// </summary>
        public string TrueValue { get; set; }

        /// <summary>
        /// Gets/sets the <see cref="Visibility"/> value that is associated
        /// (converted into) with the boolean false value.
        /// </summary>
        public string FalseValue { get; set; }

        #endregion properties

        #region methods

        /// <summary>
        /// Converts a bool value into <see cref="Visibility"/> as configured in the
        /// <see cref="TrueValue"/> and <see cref="FalseValue"/> properties.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(value is bool))
                return null;

            return (bool)value ? TrueValue : FalseValue;
        }

        /// <summary>
        /// Converts a <see cref="Visibility"/> value into bool as configured in the
        /// <see cref="TrueValue"/> and <see cref="FalseValue"/> properties.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) {
            if (Equals (value, TrueValue))
                return true;

            if (Equals (value, FalseValue))
                return false;

            return null;
        }

        #endregion methods
    }

    public class BraceFoldingStrategy {

        /// <summary>
        /// Gets/Sets the opening brace. The default value is '{'.
        /// </summary>
        public char OpeningBrace { get; set; }

        /// <summary>
        /// Gets/Sets the closing brace. The default value is '}'.
        /// </summary>
        public char ClosingBrace { get; set; }

        /// <summary>
        /// Creates a new BraceFoldingStrategy.
        /// </summary>
        public BraceFoldingStrategy () {
            this.OpeningBrace = '{';
            this.ClosingBrace = '}';
        }

        public void UpdateFoldings (FoldingManager manager, TextDocument document) {
            IEnumerable<NewFolding> newFoldings = CreateNewFoldings (document, out int firstErrorOffset);
            manager.UpdateFoldings (newFoldings, firstErrorOffset);
        }

        /// <summary>
        /// Create <see cref="NewFolding"/>s for the specified document.
        /// </summary>
        public IEnumerable<NewFolding> CreateNewFoldings (TextDocument document, out int firstErrorOffset) {
            firstErrorOffset = -1;
            return CreateNewFoldings (document);
        }

        /// <summary>
        /// Create <see cref="NewFolding"/>s for the specified document.
        /// </summary>
        public IEnumerable<NewFolding> CreateNewFoldings (ITextSource document) {
            List<NewFolding> newFoldings = new List<NewFolding> ();

            Stack<int> startOffsets = new Stack<int> ();
            int lastNewLineOffset = 0;
            char openingBrace = this.OpeningBrace;
            char closingBrace = this.ClosingBrace;
            for (int i = 0; i < document.TextLength; i++) {
                char c = document.GetCharAt (i);
                if (c == openingBrace) {
                    startOffsets.Push (i);
                } else if (c == closingBrace && startOffsets.Count > 0) {
                    int startOffset = startOffsets.Pop ();
                    // don't fold if opening and closing brace are on the same line
                    if (startOffset < lastNewLineOffset) {
                        newFoldings.Add (new NewFolding (startOffset, i + 1));
                    }
                } else if (c == '\n' || c == '\r') {
                    lastNewLineOffset = i + 1;
                }
            }
            newFoldings.Sort ((a, b) => a.StartOffset.CompareTo (b.StartOffset));
            return newFoldings;
        }
    }
}