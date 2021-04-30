using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media;

using CPP_EP.Execute;
using CPP_EP.Lab.Data;

namespace CPP_EP.Lab {

    internal class Lab3: Lab2 {
        private readonly List<string> _LabFiles = new List<string> () { "lab3.c", "src\\rule.c", "src\\voidtable.c", "src\\first.c", "src\\follow.c" };

        public override List<string> LabFiles => _LabFiles;

        public override int LabNo => 3;

        public override void Build () {
            Util.ThreadRun (() => {
                new GCC ()
                .Compile ("src\\rule.c", "build\\obj\\rule.o")
                .Compile ("src\\voidtable.c", "build\\obj\\voidtable.o")
                .Compile ("src\\first.c", "build\\obj\\first.o")
                .Compile ("src\\follow.c", "build\\obj\\follow.o")
                .Compile ("lab3.c", "build\\obj\\lab3.o")
                .Link ("build\\lab3.exe");
            });
        }

        public void GetSet (string address, Action<Set> AfterGetSet) {
            gdb.SendScript ("getset " + address, r => AfterGetSet (Set.Gen (r)));
        }

        public override void Draw () {
            WatchValues (() => {
                DrawRules (1, "ruleHead");
                DrawVoidTable (2, "voidTable");
                DrawSetList (3, "firstSetList", "FIRST");
                DrawSet (4, "&tmpSet");
                DrawSetList (5, "followSetList", "FOLLOW");
            }, "rule", "production", "symbol", "desSet", "srcSet");
        }

        protected void DrawSet (int i, string label) {
            GetSet (label, set => {
                if (set != null && set.Terminal.Count > 0) {
                    if (DataHash.ContainsKey (label) && set.Equals (DataHash[label] as Set)) {
                        return;
                    }
                    DataHash[label] = set;
                    UpdateUI (i, tb => {
                        tb.Inlines.Clear ();
                        tb.Inlines.Add (label + ":");
                        tb.Inlines.Add (new LineBreak ());
                        tb.Inlines.Add (new Run ("{ ") { Foreground = Brushes.Gray });
                        for (int i = 0; i < set.Terminal.Count; i++) {
                            if (i == 0) {
                                tb.Inlines.Add (set.Terminal[i]);
                            } else {
                                tb.Inlines.Add (new Run (" , ") { Foreground = Brushes.Gray });
                                tb.Inlines.Add (set.Terminal[i]);
                            }
                        }
                        tb.Inlines.Add (new Run (" }") { Foreground = Brushes.Gray });
                        tb.Inlines.Add (new LineBreak ());
                    });
                }
            });
        }
    }
}