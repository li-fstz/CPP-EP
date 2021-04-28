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

    internal abstract class AbstractLab {
        public abstract List<string> LabFiles { get; }
        public abstract int LabNo { get; }
        protected GDB gdb;
        public static Action<int, Action<TextBlock>> UpdateUI { protected get; set; }

        public abstract void Draw ();

        public abstract void Build ();

        public static Dictionary<string, object> DataHash = new Dictionary<string, object> ();
        public static Dictionary<string, string> WatchedValue;

        protected void WatchValues (Action AfterGetValues, params string[] names) {
            gdb.GetValues (names, AfterGetValues);
        }

        protected bool CheckWatchedValueChanged (string name, string prefix) {
            string a = null, b = null;
            if (WatchedValue.ContainsKey (name)) {
                a = WatchedValue[name];
            }
            if (DataHash.ContainsKey (prefix + name)) {
                b = DataHash[prefix + name] as string;
            }
            if (a == b) {
                return false;
            } else {
                DataHash[prefix + name] = a;
                return true;
            }
        }

        protected static UIElement Border (UIElement u, bool b, Brush c) => b ? new Border () {
            BorderThickness = new Thickness (0),
            Child = u,
            Background = c
        } : u;

        protected void DrawRules (int i, string label, string key = null) {
            GetRules (label, rules => {
                if (rules == null || rules.Count == 0) {
                    return;
                }
                if (key == null) key = label;
                if (DataHash.ContainsKey (key) && rules.SequenceEqual (DataHash[key] as List<Rule>)
                    && !CheckWatchedValueChanged ("rule", "DrawRules_")
                    && !CheckWatchedValueChanged ("production", "DrawRules_")
                    && !CheckWatchedValueChanged ("symbol", "DrawRules_")
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
                    var rb = new Border () {
                        Background = Brushes.PaleGreen,
                        Child = new TextBlock (new Run ("rule")),
                        Visibility = Visibility.Collapsed
                    };
                    var pb = new Border () {
                        Background = Brushes.Khaki,
                        Child = new TextBlock (new Run ("production")),
                        Visibility = Visibility.Collapsed
                    };
                    var sb = new Border () {
                        Background = Brushes.SandyBrown,
                        Child = new TextBlock (new Run ("symbol")),
                        Visibility = Visibility.Collapsed
                    };
                    tb.Inlines.Add (rb);
                    tb.Inlines.Add (pb);
                    tb.Inlines.Add (sb);
                    tb.Inlines.Add (new LineBreak ());
                    bool rbv = false, pbv = false, sbv = false;
                    foreach (var rule in rules) {
                        var rt = new TextBlock ();
                        tb.Inlines.Add (Border (rt, rule.Address == rAddress, Brushes.PaleGreen));
                        rbv |= rule.Address == rAddress;
                        rt.Inlines.Add (rule.Name);
                        rt.Inlines.Add (new Run (" -> ") { Foreground = Brushes.Gray });
                        foreach (var production in rule.Productions) {
                            var pt = new TextBlock ();
                            pbv |= production.Address == pAddress;
                            if (production != rule.Productions[0]) {
                                rt.Inlines.Add (new Run (" | ") { Foreground = Brushes.Gray });
                            }
                            foreach (var symbol in production.Symbols) {
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

        public void GetRules (string address, Action<List<Rule>> AfterGetRules) {
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