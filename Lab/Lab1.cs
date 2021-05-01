using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using CPP_EP.Execute;
using CPP_EP.Lab.Data;

namespace CPP_EP.Lab {

    internal class Lab1: AbstractLab {
        private readonly List<string> _LabFiles = new List<string> () {
            "lab1.c",
            "src\\rule.c",
            "src\\voidtable.c",
            "inc\\voidtable.h",
            "inc\\rule.h"
        };

        public override List<string> LabFiles => _LabFiles;

        public override int LabNo => 1;

        public override void Build () {
            Util.ThreadRun (() => {
                new GCC ()
                .Compile ("src\\rule.c", "build\\obj\\rule.o")
                .Compile ("src\\voidtable.c", "build\\obj\\voidtable.o")
                .Compile ("lab1.c", "build\\obj\\lab1.o")
                .Link ("build\\lab1.exe");
            });
        }

        protected void DrawVoidTable (int i, string label) {
            GetVoidTable (label, voidTable => {
                if (voidTable != null && voidTable.TableHead.Count > 0) {
                    if (DataHash.ContainsKey (label) && voidTable.Equals (DataHash[label] as VoidTable)
                        && !CheckWatchedValueChange ("DrawVoidTable_", "symbol", "rule")) {
                        return;
                    }
                    WatchedValue.TryGetValue ("symbol", out string sAddress);
                    WatchedValue.TryGetValue ("rule", out string rAddress);
                    DataHash[label] = voidTable;
                    UpdateUI (i, tb => {
                        tb.Inlines.Clear ();
                        tb.Inlines.Add (label + ":");
                        Border rb = new Border () {
                            Background = Brushes.PaleGreen,
                            Child = new TextBlock (new Run ("rule")),
                            Visibility = Visibility.Collapsed
                        };
                        Border sb = new Border () {
                            Background = Brushes.SandyBrown,
                            Child = new TextBlock (new Run ("symbol")),
                            Visibility = Visibility.Collapsed
                        };
                        tb.Inlines.Add (rb);
                        tb.Inlines.Add (sb);
                        tb.Inlines.Add (new LineBreak ());
                        foreach (string h in voidTable.TableHead) {
                            Symbol symbol = GDBData.Get<Symbol> (sAddress);
                            Rule rule = GDBData.Get<Rule> (rAddress);
                            Brush b = null;
                            if (symbol != null && symbol.Name == h) {
                                b = Brushes.SandyBrown;
                                sb.Visibility = Visibility.Visible;
                            } else if (rule != null && rule.Name == h) {
                                b = Brushes.PaleGreen;
                                rb.Visibility = Visibility.Visible;
                            }
                            if (h == voidTable.TableHead[0]) {
                                tb.Inlines.Add (NewBorder (new TextBlock (new Run (h)), 1, 1, 1, 1, b));
                            } else {
                                tb.Inlines.Add (NewBorder (new TextBlock (new Run (h)), 0, 1, 1, 1, b));
                            }
                        }
                        tb.Inlines.Add (new LineBreak ());
                        for (int i = 0; i < voidTable.HasVoid.Count; i++) {
                            int f = i == 0 ? 1 : 0;
                            if (voidTable.HasVoid[i].HasValue) {
                                tb.Inlines.Add (NewBorder (new TextBlock (new Run (voidTable.HasVoid[i].Value ? "1" : "0")), f, 0, 1, 1));
                            } else {
                                tb.Inlines.Add (NewBorder (new TextBlock (), f, 0, 1, 1));
                            }
                        }
                        tb.Inlines.Add (new LineBreak ());
                    });
                }
            });
        }

        protected static Border NewBorder (UIElement u, int a, int b, int c, int d, Brush brush = null) {
            return new Border () {
                BorderThickness = new Thickness (a, b, c, d),
                BorderBrush = Brushes.Gray,
                Padding = new Thickness (1, 1, 1, 1),
                Width = 52,
                Child = u,
                Background = brush is Brush ? brush : Brushes.White
            };
        }

        public override void Draw () {
            WatchValues (() => {
                DrawRules (1, "ruleHead");
                DrawRules (2, "copiedRule", "copiedRule");
                DrawRules (2, "*copiedRulePrePtr", "copiedRule");
                DrawVoidTable (3, "voidTable");
            }, "rule", "production", "symbol");
        }

        public void GetVoidTable (string address, Action<VoidTable> AfterGetVoidTable) {
            gdb.SendScript ("getvoidtable " + address, r => AfterGetVoidTable (VoidTable.Gen (r)));
        }
    }
}