using ScintillaNET;
using ScintillaNET.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace CPP_EP
{
    using CodePosition = ValueTuple<string, int>;
    public partial class FileTab : TabItem
    {
        private const int LINE_NUMBERS_MARGIN_WIDTH = 30;

        private const int BACK_COLOR = 0x2A211C;

        private const int FORE_COLOR = 0xB7B7B7;

        private const int NUMBER_MARGIN = 1;

        private const int BOOKMARK_MARGIN = 2;

        private const int BOOKMARK_MARKER = 2;

        private const int FOLDING_MARGIN = 3;

        private const bool CODEFOLDING_CIRCULAR = false;

        public delegate CodePosition MethodCorrectBreakPoint(CodePosition cp);
        public MethodCorrectBreakPoint CorrectBreakPoint;
        public delegate void MethodDeleteBreakPoint(CodePosition cp);
        public MethodDeleteBreakPoint DeleteBreakPoint;
        public FileTab(string filepath)
        {
            InitializeComponent();

            scintilla.WrapMode = ScintillaNET.WrapMode.None;
            scintilla.IndentationGuides = ScintillaNET.IndentView.LookBoth;

            scintilla.CaretForeColor = Colors.White;
            scintilla.SetSelectionBackColor(true, IntToMediaColor(0x114D9C));

            // Configure the default style
            scintilla.StyleResetDefault();
            scintilla.Styles[ScintillaNET.Style.Default].Font = "Consolas";
            scintilla.Styles[ScintillaNET.Style.Default].Size = 11;
            scintilla.Styles[ScintillaNET.Style.Default].BackColor = IntToColor(0x212121);
            scintilla.Styles[ScintillaNET.Style.Default].ForeColor = IntToColor(0xFFFFFF);
            scintilla.StyleClearAll();

            // Configure the CPP (C#) lexer styles
            scintilla.Styles[ScintillaNET.Style.Cpp.Identifier].ForeColor = IntToColor(0xD0DAE2);
            scintilla.Styles[ScintillaNET.Style.Cpp.Comment].ForeColor = IntToColor(0xBD758B);
            scintilla.Styles[ScintillaNET.Style.Cpp.CommentLine].ForeColor = IntToColor(0x40BF57);
            scintilla.Styles[ScintillaNET.Style.Cpp.CommentDoc].ForeColor = IntToColor(0x2FAE35);
            scintilla.Styles[ScintillaNET.Style.Cpp.Number].ForeColor = IntToColor(0xFFFF00);
            scintilla.Styles[ScintillaNET.Style.Cpp.String].ForeColor = IntToColor(0xFFFF00);
            scintilla.Styles[ScintillaNET.Style.Cpp.Character].ForeColor = IntToColor(0xE95454);
            scintilla.Styles[ScintillaNET.Style.Cpp.Preprocessor].ForeColor = IntToColor(0x8AAFEE);
            scintilla.Styles[ScintillaNET.Style.Cpp.Operator].ForeColor = IntToColor(0xE0E0E0);
            scintilla.Styles[ScintillaNET.Style.Cpp.Regex].ForeColor = IntToColor(0xff00ff);
            scintilla.Styles[ScintillaNET.Style.Cpp.CommentLineDoc].ForeColor = IntToColor(0x77A7DB);
            scintilla.Styles[ScintillaNET.Style.Cpp.Word].ForeColor = IntToColor(0x48A8EE);
            scintilla.Styles[ScintillaNET.Style.Cpp.Word2].ForeColor = IntToColor(0xF98906);
            scintilla.Styles[ScintillaNET.Style.Cpp.CommentDocKeyword].ForeColor = IntToColor(0xB3D991);
            scintilla.Styles[ScintillaNET.Style.Cpp.CommentDocKeywordError].ForeColor = IntToColor(0xFF0000);
            scintilla.Styles[ScintillaNET.Style.Cpp.GlobalClass].ForeColor = IntToColor(0x48A8EE);

            scintilla.Lexer = ScintillaNET.Lexer.Cpp;

            scintilla.SetKeywords(0, "break case const continue default do else for goto if return sizeof static struct switch typedef union void while");
            scintilla.SetKeywords(1, "char int Rule VoidTable SetList SelectSetList ParsingTable Symbol Production");

            scintilla.Styles[ScintillaNET.Style.LineNumber].BackColor = IntToColor(BACK_COLOR);
            scintilla.Styles[ScintillaNET.Style.LineNumber].ForeColor = IntToColor(FORE_COLOR);
            scintilla.Styles[ScintillaNET.Style.IndentGuide].ForeColor = IntToColor(FORE_COLOR);
            scintilla.Styles[ScintillaNET.Style.IndentGuide].BackColor = IntToColor(BACK_COLOR);

            var nums = scintilla.Margins[NUMBER_MARGIN];
            nums.Width = LINE_NUMBERS_MARGIN_WIDTH;
            nums.Type = MarginType.Number;
            nums.Sensitive = true;
            nums.Mask = 0;

            var margin = scintilla.Margins[BOOKMARK_MARGIN];
            margin.Width = 20;
            margin.Sensitive = true;
            margin.Type = MarginType.Symbol;
            margin.Mask = (1 << BOOKMARK_MARKER);

            var marker = scintilla.Markers[BOOKMARK_MARKER];
            marker.Symbol = MarkerSymbol.Circle;
            marker.SetBackColor(IntToColor(0xFF003B));
            marker.SetForeColor(IntToColor(0x000000));
            marker.SetAlpha(100);

            scintilla.Text = File.ReadAllText(filepath);

            Header = Path.GetFileName(filepath);

            scintilla.MarginClick += Scintilla_MarginClick;
        }

        private void Scintilla_MarginClick(object sender, MarginClickEventArgs e)
        {
            if (e.Margin == BOOKMARK_MARGIN)
            {
                const uint mask = (1 << BOOKMARK_MARKER);
                Line line = scintilla.Lines[scintilla.LineFromPosition(e.Position)];
                if ((line.MarkerGet() & mask) > 0)
                {
                    line.MarkerDelete(BOOKMARK_MARKER);
                    DeleteBreakPoint(new CodePosition((string)Header, line.Index + 1));
                }
                else
                {
                    SetBreakPoint(scintilla.LineFromPosition(e.Position));
                }
            }
        }

        public ScintillaWPF Scintilla
        {
            get { return scintilla; }
        }
        public static Color IntToMediaColor(int rgb)
        {
            return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }
        public static System.Drawing.Color IntToColor(int rgb)
        {
            return System.Drawing.Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }
        public List<int> GetAllBreakPointLine()
        {
            List<int> lines = new List<int>();
            int line = 0;
            while (true)
            {
                line = scintilla.Lines[line + 1].MarkerNext(1 << BOOKMARK_MARKER);
                if (line == -1)
                {
                    break;
                }
                else
                {
                    lines.Add(line);
                }
            }
            return lines;
        }
        public void SetBreakPoint(int line)
        {
            MarkLine(CorrectBreakPoint(new CodePosition((string)Header, line + 1)).Item2);
        }
        public void MarkLine(int line)
        {
            scintilla.Lines[line - 1].MarkerAdd(BOOKMARK_MARKER);
        }
        public void UnMarkLine(int line)
        {
            scintilla.Lines[line - 1].MarkerDelete(BOOKMARK_MARKER);
        }
    }
}
