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
    class Lab4: Lab3 {
                
        public override List<string> LabFiles => new List<string> () { "lab4.c", "src\\rule.c", "src\\voidtable.c", "src\\first.c", "src\\follow.c", "src\\parsingtable.c" };

        public override int LabNo => 4;
        public override void Build () {
            Util.ThreadRun (() => {
                new GCC ()
                .Compile ("src\\rule.c", "build\\obj\\rule.o")
                .Compile ("src\\voidtable.c", "build\\obj\\voidtable.o")
                .Compile ("src\\first.c", "build\\obj\\first.o")
                .Compile ("src\\follow.c", "build\\obj\\follow.o")
                .Compile ("src\\parsingtable.c", "build\\obj\\parsingtable.o")
                .Compile ("lab4.c", "build\\obj\\lab4.o")
                .Link ("build\\lab4.exe");
            });
        }
        public override void Draw () {
            DrawRules (1, "ruleHead");
            DrawVoidTable (2, "voidTable");
            DrawSetList (3, "firstSetList", "FIRST");
            DrawSetList (4, "followSetList", "FOLLOW");
            DrawSelectSetList (5, "selectSetList");
            DrawParsingTable (6, "parsingTable");
        }
        protected void DrawSelectSetList (int i, string label) {
            GetSelectSetList (label, setList => {
                if (setList != null && setList.Count > 0) {
                    if (DataHash.ContainsKey (label) && setList.SequenceEqual (DataHash[label] as List<SelectSet>)) {
                        return;
                    }
                    DataHash[label] = setList;
                    UpdateUI (i, (tb) => {
                        tb.Inlines.Clear ();
                        tb.Inlines.Add (label + ":");
                        tb.Inlines.Add (new LineBreak ());
                        foreach (var set in setList) {
                            tb.Inlines.Add (new Run ("SELECT( ") { Foreground = Brushes.Gray });
                            tb.Inlines.Add (set.Rule.Name);
                            tb.Inlines.Add (new Run (" -> ") { Foreground = Brushes.Gray });
                            if (set.Production != null) {
                                foreach (var s in set.Production.Symbols) {
                                    tb.Inlines.Add (s.Name);
                                }
                            }
                            tb.Inlines.Add (new Run (" ) = { ") { Foreground = Brushes.Gray });
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
                        }
                    });
                }
            });
        }
        protected void DrawParsingTable (int i, string label) {
            GetParsingTable (label, parsingTable => {
                if (parsingTable != null && parsingTable.TableHead.Count > 0) {
                    if (DataHash.ContainsKey (label) && parsingTable.Equals (DataHash[label] as ParsingTable)) {
                        return;
                    }
                    DataHash[label] = parsingTable;
                    UpdateUI (i, (tb) => {
                        tb.Inlines.Clear ();
                        tb.Inlines.Add (label + ":");
                        tb.Inlines.Add (new LineBreak ());
                        tb.Inlines.Add (NewBorder (new TextBlock(), 1, 1, 1, 1));
                        foreach (var h in parsingTable.TableHead) {
                            tb.Inlines.Add (NewBorder (new TextBlock (new Run (h)), 0, 1, 1, 1));
                        }
                        tb.Inlines.Add (new LineBreak ());
                        foreach (var r in parsingTable.TableRows) {
                            tb.Inlines.Add (NewBorder (new TextBlock (new Run (r.Rule.Name)), 1, 0, 1, 1));
                            foreach (var c in r.Productions) {
                                var t = new TextBlock ();
                                if (c != null) {
                                    foreach (var s in c.Symbols) {
                                        t.Inlines.Add (s.Name);
                                    }
                                }
                                tb.Inlines.Add (NewBorder (t, 0, 0, 1, 1));
                            }
                            tb.Inlines.Add (new LineBreak ());
                        }
                    });
                }
            });
        }
        
        public void GetParsingTable (string address, Action<ParsingTable> AfterGetParsingTable) {
            gdb.SendScript ("getparsingtable " + address, r => AfterGetParsingTable (ParsingTable.GenParsingTabele (r)));
        }
        public void GetSelectSetList (string address, Action<List<SelectSet>> AfterGetSelectSetList) {
            gdb.SendScript ("getselectsetlist " + address, r => {
                List<SelectSet> selectsetList = new List<SelectSet> ();
                string[] ruleStrings = r.Split (new string[] { "~\"|selectsetlist|\"" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in ruleStrings) {
                    var set = SelectSet.GenSelectSet (s);
                    if (set != null) selectsetList.Add (set);
                }
                AfterGetSelectSetList (selectsetList);
            });
        }

        
    }
}
