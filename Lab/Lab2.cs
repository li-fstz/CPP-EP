using CPP_EP.Execute;
using CPP_EP.Lab.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;

namespace CPP_EP.Lab {
    class Lab2: Lab1 {
                
        public override List<string> LabFiles => new List<string> () { "lab2.c", "src\\rule.c", "src\\voidtable.c", "src\\first.c" };

        public override int LabNo => 2;
        public override void Draw () {
            DrawRules (1, "ruleHead");
            DrawVoidTable (2, "voidTable");
            DrawSetList (3, "firstSetList", "FIRST");
        }
        protected void DrawSetList (int i, string label, string type) {
            GetSetList (label, setList => {
                if (setList != null && setList.Count > 0) {
                    if (DataHash.ContainsKey (label) && setList.SequenceEqual (DataHash[label] as List<Set>)) {
                        return;
                    }
                    DataHash[label] = setList;
                    UpdateUI (i, (tb) => {
                        tb.Inlines.Clear ();
                        tb.Inlines.Add (label + ":");
                        tb.Inlines.Add (new LineBreak ());
                        foreach(var set in setList) {
                            tb.Inlines.Add (new Run (type + "( ") { Foreground = Brushes.Gray });
                            tb.Inlines.Add (set.Name);
                            tb.Inlines.Add (new Run (" ) = { ") { Foreground = Brushes.Gray });
                            
                            for (int i = 0; i < set.Terminal.Count; i ++) {
                                if (i == 0) {
                                    tb.Inlines.Add (set.Terminal[i]);
                                } else {
                                    tb.Inlines.Add (new Run (" , ") { Foreground = Brushes.Gray });
                                    tb.Inlines.Add (set.Terminal[i]);
                                }
                            }
                            tb.Inlines.Add (new Run (" }") { Foreground = Brushes.Gray });
                            tb.Inlines.Add (new LineBreak ());
                        }
                    });
                }
            });
        }
        public override void Build () {
            Util.ThreadRun (() => {
                new GCC ()
                .Compile ("src\\rule.c", "build\\obj\\rule.o")
                .Compile ("src\\voidtable.c", "build\\obj\\voidtable.o")
                .Compile ("src\\first.c", "build\\obj\\first.o")
                .Compile ("lab2.c", "build\\obj\\lab2.o")
                .Link ("build\\lab2.exe");
            });
        }
        public void GetSetList (string address, Action<List<Set>> AfterGetSetList) {
            gdb.SendScript ("getsetlist " + address, r => {
                List<Set> setList = new List<Set> ();
                string[] ruleStrings = r.Split (new string[] { "~\"|setlist|\"" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in ruleStrings) {
                    var set = Set.GenSet (s);
                    if (set != null) {
                        setList.Add (Set.GenSet (s));
                    }
                }
                AfterGetSetList (setList);
            });
        }

        
    }
}
