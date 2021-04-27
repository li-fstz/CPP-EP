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
    abstract class AbstractLab {
        public abstract List<string> LabFiles { get; }
        public abstract int LabNo { get; }
        protected GDB gdb;
        public static Action<int, Action<TextBlock>> UpdateUI { protected get; set; }
        public abstract void Draw ();
        public abstract void Build ();

        public static Dictionary<string, object> DataHash = new Dictionary<string, object> ();
        protected void DrawRules (int i, string label, string key = null) {
            GetRules (label, rules => {
                if (rules == null || rules.Count == 0) {
                    return;
                }
                if (key == null) key = label;
                if (DataHash.ContainsKey (key) && rules.SequenceEqual (DataHash[key] as List<Rule>)) {
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
                
            });
        }
        public GDB GetGDB () {
            gdb = new GDB (
                "build\\lab" + LabNo + ".exe"
            );
            return gdb;
        }
        public void GetRules (string address, Action<List<Rule>> AfterGetRules) {
            gdb.SendScript ("getrule " + address, r => {
                List<Rule> rules = new List<Rule> ();
                string[] ruleStrings = r.Split (new string[] { "~\"|rule|\"" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in ruleStrings) {
                    var rule = Rule.GenRule (s);
                    if (rule != null) rules.Add (rule);
                }
                AfterGetRules (rules);
            });
        }
        public static AbstractLab GetLab(int index) {
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
