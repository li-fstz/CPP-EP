using CPP_EP.Execute;

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
                .Link ("build\\lab1.exe", "build\\obj\\rule.o", "build\\obj\\voidtable.o", "build\\obj\\lab1.o");
            });
        }
        protected void DrawRules (int i, string label, string key = null) {
            GetRules (label, rules => {
                if (rules != null && rules.Count > 0) {
                    if (key == null) key = label;
                    if (DataHash.ContainsKey(key) && rules.SequenceEqual(DataHash[key] as List<Rule>)) {
                        return;
                    }
                    DataHash[key] = rules;
                    UpdateUI (i, (tb) => {
                        tb.Inlines.Clear ();
                        tb.Inlines.Add (label + ":");
                        tb.Inlines.Add (new LineBreak ());
                        foreach (var rule in rules) {
                            tb.Inlines.Add (rule.Name);
                            tb.Inlines.Add (new Run (" -> ") { Foreground = Brushes.Gray });
                            foreach (var production in rule.Productions) {
                                if (production != rule.Productions[0]) {
                                    tb.Inlines.Add (new Run (" | ") { Foreground = Brushes.Gray });
                                }
                                foreach (var synbol in production.Symbols) {
                                    tb.Inlines.Add (synbol.Name);
                                }
                            }
                            tb.Inlines.Add (new LineBreak ());
                        }
                    });
                }
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

        public class VoidTable {
            public List<string> TableHead;
            public List<bool?> HasVoid;
            private VoidTable () { }
            public override bool Equals (object obj) {
                return obj is VoidTable v && v.TableHead.SequenceEqual (TableHead) && v.HasVoid.SequenceEqual (HasVoid);
            }
            public static VoidTable GenVoidTable (string s) {
                VoidTable v = null;
                try {
                    if (s == null) {
                        throw new Exception ("Parsing VoidTable Error: " + s);
                    }
                    string[] structs = s.Split (new string[] { "~\"|voidtable|\"" }, StringSplitOptions.RemoveEmptyEntries);
                    v = new VoidTable () {
                        TableHead = new List<string> (),
                        HasVoid = new List<bool?> ()
                    };
                    foreach (Match m in Text.Matches (structs[0])) {
                        v.TableHead.Add (m.Groups[1].Value);
                    }
                    foreach (Match m in Text.Matches (structs[1])) {
                        if (m.Groups[1].Value == "1") {
                            v.HasVoid.Add (true);
                        } else if (m.Groups[1].Value == "0") {
                            v.HasVoid.Add (false);
                        } else {
                            v.HasVoid.Add (null);
                        }
                    }
                    if (v.HasVoid.Count == 0 || v.TableHead.Count == 0) {
                        v = null;
                    }
                } catch { }
                return v;
            }
        }
    }
}
