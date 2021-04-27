using CPP_EP.Execute;
using CPP_EP.Lab.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace CPP_EP.Lab {
    class Lab1: AbstractLab {

        public override List<string> LabFiles => new List<string> () { "lab1.c", "src\\rule.c", "src\\voidtable.c" };

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
                    if (DataHash.ContainsKey (label) && voidTable.Equals (DataHash[label] as VoidTable)) {
                        return;
                    }
                    DataHash[label] = voidTable;
                    UpdateUI (i, (tb) => {
                        tb.Inlines.Clear ();
                        tb.Inlines.Add (label + ":");
                        tb.Inlines.Add (new LineBreak ());
                        foreach (var h in voidTable.TableHead) {
                            if (h == voidTable.TableHead[0]) {
                                tb.Inlines.Add (NewBorder (new TextBlock (new Run (h)), 1, 1, 1, 1));
                            } else {
                                tb.Inlines.Add (NewBorder (new TextBlock (new Run (h)), 0, 1, 1, 1));
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
        protected static Border NewBorder (UIElement u, int a, int b, int c, int d) => new Border () {
            BorderThickness = new Thickness (a, b, c, d),
            BorderBrush = Brushes.Gray,
            Child = u,
            Padding = new Thickness (3, 3, 0, 3),
            Width = 54
        };
        public override void Draw () {
            DrawRules (1, "ruleHead");
            DrawRules (2, "copiedRule", "copiedRule");
            DrawRules (2, "*copiedRulePrePtr", "copiedRule");
            DrawVoidTable (3, "voidTable");
        }
        public void GetVoidTable (string address, Action<VoidTable> AfterGetVoidTable) {
            gdb.SendScript ("getvoidtable " + address, r => AfterGetVoidTable (VoidTable.GenVoidTable (r)));
        }

        
    }
}
