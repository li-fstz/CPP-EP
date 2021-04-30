using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using CPP_EP.Execute;
using CPP_EP.Lab.Data;

namespace CPP_EP.Lab {

    internal class Lab8: Lab4 {
        private readonly List<string> _LabFiles = new List<string> () { "lab8.c", "src\\rule.c", "src\\voidtable.c", "src\\first.c", "src\\follow.c", "src\\parsingtable.c", "src\\parser.c" };
        public override List<string> LabFiles => _LabFiles;
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
            WatchValues (() => {
                DrawRules (1, "ruleHead");
                DrawParsingTable (2, "parsingTable");
                //DrawValue (3, "string");
                //DrawValue (4, "topSymbol", "((struct Symbol *)topSymbol->value)->symbolName");
                DrawParsingStack (3, "stack");
            }, "rule", "production", "symbol", "foundProduction", "((struct Symbol *)topSymbol->value)->symbolName", "string");
        }

        public void GetParsingStack (string address, Action<Stack> AfterParsingStack) {
            gdb.SendScript ("getparsingstack " + address, r => AfterParsingStack (Stack.Gen (r)));
        }

        protected void DrawParsingStack (int i, string label) {
            GetParsingStack (label, stack => {
                if (stack == null || stack.Symbols.Count == 0) {
                    return;
                }
                if (DataHash.ContainsKey (label) && stack.Equals (DataHash[label] as List<string>)
                    && !CheckWatchedValueChange ("DrawParsingStack_", "((struct Symbol *)topSymbol->value)->symbolName", "string")) {
                    return;
                }
                WatchedValue.TryGetValue ("((struct Symbol *)topSymbol->value)->symbolName", out string topSymbol);
                WatchedValue.TryGetValue ("string", out string str);
                DataHash[label] = stack;
                UpdateUI (i, tb => {
                    tb.Inlines.Clear ();
                    tb.Inlines.Add ("string: " + str);
                    tb.Inlines.Add (new LineBreak ());
                    tb.Inlines.Add (label + ":");
                    var tsb = new Border () {
                        Background = Brushes.PaleGreen,
                        Child = new TextBlock (new Run ("topSymbol")),
                        Visibility = Visibility.Collapsed
                    };
                    tb.Inlines.Add (tsb);
                    tb.Inlines.Add (new LineBreak ());
                    if (topSymbol != null && topSymbol.IndexOf ("0x") == -1) {
                        tsb.Visibility = Visibility.Visible;
                        tb.Inlines.Add (NewBorder (new TextBlock (new Run (topSymbol)), 1, 1, 1, 1, Brushes.PaleGreen));
                        tb.Inlines.Add (new LineBreak ());
                        tb.Inlines.Add (new LineBreak ());
                    }
                    for (int i = stack.Symbols.Count () - 1; i >= 0; i--) {
                        tb.Inlines.Add (NewBorder (new TextBlock (new Run (stack.Symbols[i])), 1, 0, 1, 1));
                        tb.Inlines.Add (new LineBreak ());
                    }
                });
            });
        }
    }
}