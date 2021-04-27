using CPP_EP.Execute;
using CPP_EP.Lab.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace CPP_EP.Lab {
    class Lab8: Lab4 {
               
        public override List<string> LabFiles => new List<string> () { "lab8.c", "src\\rule.c", "src\\voidtable.c", "src\\first.c", "src\\follow.c", "src\\parsingtable.c", "src\\parser.c" };

        public static readonly Regex StringValue = new Regex (@"\\""(.+)\\""");
        public override int LabNo => 8;
        public override void Build () {
            Util.ThreadRun (() => {
                new GCC ()
                .Compile ("src\\rule.c", "build\\obj\\rule.o")
                .Compile ("src\\voidtable.c", "build\\obj\\voidtable.o")
                .Compile ("src\\first.c", "build\\obj\\first.o")
                .Compile ("src\\follow.c", "build\\obj\\follow.o")
                .Compile ("src\\parsingtable.c", "build\\obj\\parsingtable.o")
                .Compile ("src\\parser.c", "build\\obj\\parser.o")
                .Compile ("lab8.c", "build\\obj\\lab8.o")
                .Link ("build\\lab8.exe");
            });
        }
        public override void Draw () {
            DrawRules (1, "ruleHead");
            DrawParsingTable (2, "parsingTable");
            DrawValue (3, "string");
            DrawValue (4, "topSymbol", "((struct Symbol *)topSymbol->value)->symbolName");
            DrawParsingStack (5, "stack");
        }
        public void GetParsingStack (string address, Action<List<string>> AfterParsingStack) {
            gdb.SendScript ("getparsingstack " + address, r => {
                List<string> stack = new List<string> ();
                foreach (Match m in GDBData.Text.Matches (r)) {
                    if (m.Success) {
                        stack.Add (m.Groups[1].Value);
                    }
                }
                AfterParsingStack (stack);
            });
        }
        protected void DrawValue (int i, string label, string path = null) {
            if (path == null) {
                path = label;
            }
            gdb.GetValue (path, (value) => {
                if (value == null) {
                    return;
                }
                Match m = StringValue.Match (value);
                if (!m.Success) {
                    return;
                }
                if (DataHash.ContainsKey (label) && m.Groups[1].Value == DataHash[label] as string) {
                    return;
                }
                DataHash[label] = m.Groups[1].Value;
                UpdateUI (i, (tb) => {
                    tb.Inlines.Clear ();
                    tb.Inlines.Add (label);
                    tb.Inlines.Add (new Run (" = ") { Foreground = Brushes.Gray });
                    tb.Inlines.Add (m.Groups[1].Value);
                    tb.Inlines.Add (new LineBreak ());
                });
            });
        }
        protected void DrawParsingStack (int i, string label) {
            GetParsingStack (label, stack => {
                if (stack == null || stack.Count == 0) {
                    return;
                }
                if (DataHash.ContainsKey (label) && stack.SequenceEqual (DataHash[label] as List<string>)) {
                    return;
                }
                DataHash[label] = stack;
                UpdateUI (i, (tb) => {
                    tb.Inlines.Clear ();
                    tb.Inlines.Add (label + ":");
                    tb.Inlines.Add (new LineBreak ());
                    for (int i = stack.Count () - 1; i >= 0; i --) {
                        tb.Inlines.Add (NewBorder (new TextBlock (new Run(stack[i])), 1, 0, 1, 1));
                        tb.Inlines.Add (new LineBreak ());
                    }
                });
            });
        }
    }
}
