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

    public abstract class AbstractLab {
        public abstract List<string> LabFiles { get; }
        public abstract int LabNo { get; }
        protected GDB gdb;
        public static Action<int, Action<TextBlock>> UpdateUI { protected get; set; }

        public abstract void Draw ();

        public abstract void Build ();

        public static Dictionary<string, object> DataHash = new();
        public static Dictionary<string, string> WatchedValue = new();

        protected void WatchValues (Action AfterGetValues, params string[] names) {
            //AfterGetValues ();
            gdb.GetValues (names, AfterGetValues);
        }

        protected static bool CheckWatchedValueChange (string prefix, params string[] names) {
            bool r = false;
            foreach (string name in names) {
                string a = null, b = null;
                if (WatchedValue.ContainsKey (name)) {
                    a = WatchedValue[name];
                }
                if (DataHash.ContainsKey (prefix + name)) {
                    b = DataHash[prefix + name] as string;
                }
                if (a != b) {
                    DataHash[prefix + name] = a;
                    r = true;
                }
            }
            return r;
        }

        protected static UIElement Border (UIElement u, bool b, Brush c) {
            return b ? new Border () {
                BorderThickness = new Thickness (0),
                Child = u,
                Background = c
            } : u;
        }

        protected void DrawRules (int i, string label, string key = null) {
            GetRules (label, rules => {
                if (rules == null || rules.Count == 0) {
                    return;
                }
                if (key == null) {
                    key = label;
                }

                if (DataHash.ContainsKey (key) && rules.SequenceEqual (DataHash[key] as List<Rule>)
                    && !CheckWatchedValueChange ("DrawRules_" + key, "rule", "production", "symbol")
                ) {
                    return;
                }
                WatchedValue.TryGetValue ("rule", out string rAddress);
                WatchedValue.TryGetValue ("production", out string pAddress);
                WatchedValue.TryGetValue ("symbol", out string sAddress);
                DataHash[key] = rules;
                UpdateUI (i, tb => {
                    tb.Inlines.Clear ();
                    tb.Inlines.Add (label + ":");
                    Border rb = new() {
                        Background = Brushes.PaleGreen,
                        Child = new TextBlock (new Run ("rule")),
                        Visibility = Visibility.Collapsed
                    };
                    Border pb = new() {
                        Background = Brushes.Khaki,
                        Child = new TextBlock (new Run ("production")),
                        Visibility = Visibility.Collapsed
                    };
                    Border sb = new() {
                        Background = Brushes.SandyBrown,
                        Child = new TextBlock (new Run ("symbol")),
                        Visibility = Visibility.Collapsed
                    };
                    tb.Inlines.Add (rb);
                    tb.Inlines.Add (pb);
                    tb.Inlines.Add (sb);
                    tb.Inlines.Add (new LineBreak ());
                    bool rbv = false, pbv = false, sbv = false;
                    foreach (Rule rule in rules) {
                        TextBlock rt = new();
                        tb.Inlines.Add (Border (rt, rule.Address == rAddress, Brushes.PaleGreen));
                        rbv |= rule.Address == rAddress;
                        rt.Inlines.Add (rule.Name);
                        rt.Inlines.Add (new Run (" -> ") { Foreground = Brushes.Gray });
                        foreach (Production production in rule.Productions) {
                            TextBlock pt = new();
                            pbv |= production.Address == pAddress;
                            if (production != rule.Productions[0]) {
                                rt.Inlines.Add (new Run (" | ") { Foreground = Brushes.Gray });
                            }
                            foreach (Symbol symbol in production.Symbols) {
                                pt.Inlines.Add (Border (new TextBlock (new Run (symbol.Name)), symbol.Address == sAddress, Brushes.SandyBrown));
                                sbv |= symbol.Address == sAddress;
                            }
                            rt.Inlines.Add (Border (pt, production.Address == pAddress, Brushes.Khaki));
                        }
                        if (rbv) {
                            rb.Visibility = Visibility.Visible;
                        }
                        if (pbv) {
                            pb.Visibility = Visibility.Visible;
                        }
                        if (sbv) {
                            sb.Visibility = Visibility.Visible;
                        }
                        tb.Inlines.Add (new LineBreak ());
                    }
                });
            });
        }

        public GDB GetGDB () {
            gdb = new GDB (
                "build\\lab" + LabNo + ".exe"
            );
            return gdb;
        }

        protected void GetRules (string address, Action<List<Rule>> AfterGetRules) {
            gdb.SendScript ("getrule " + address, r => AfterGetRules (Rule.GenRules (r)));
        }

        public static AbstractLab GetLab (int index) {
            return index switch {
                1 => new Lab1 (),
                2 => new Lab2 (),
                3 => new Lab3 (),
                4 => new Lab4 (),
                5 => new Lab5 (),
                6 => new Lab6 (),
                7 => new Lab7 (),
                8 => new Lab8 (),
                _ => null,
            };
        }
    }
}