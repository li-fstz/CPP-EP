using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using CPP_EP.Execute;
using CPP_EP.Lab.Data;

namespace CPP_EP.Lab {

    internal class Lab4: Lab3 {
        private readonly List<string> _LabFiles = new() {
            "lab4.c",
            "src\\rule.c",
            "src\\voidtable.c",
            "src\\first.c",
            "src\\follow.c",
            "src\\parsingtable.c",
            "inc\\parsingtable.h",
            "inc\\follow.h",
            "inc\\first.h",
            "inc\\voidtable.h",
            "inc\\rule.h"
        };

        public override List<string> LabFiles => _LabFiles;

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
            WatchValues (() => {
                DrawRules (1, "ruleHead");
                DrawVoidTable (2, "voidTable");
                DrawSetList (3, "firstSetList", "FIRST");
                DrawSetList (4, "followSetList", "FOLLOW");
                DrawSelectSetList (5, "selectSetList");
                DrawParsingTable (6, "parsingTable");
            }, "rule", "production", "symbol", "srcSet", "selectSet", "foundProduction", "isLL1Rule");
        }

        protected void DrawSelectSetList (int i, string label) {
            GetSelectSetList (label, setList => {
                if (setList != null && setList.Count > 0) {
                    if (DataHash.ContainsKey (label) && setList.SequenceEqual (DataHash[label] as List<SelectSet>)
                        && !CheckWatchedValueChange ("DrawSelectSetList_", "selectSet")) {
                        return;
                    }
                    WatchedValue.TryGetValue ("selectSet", out string selectSet);
                    DataHash[label] = setList;
                    UpdateUI (i, tb => {
                        tb.Inlines.Clear ();
                        tb.Inlines.Add (label + ":");
                        Border selectb = new() {
                            Background = Brushes.PaleGreen,
                            Child = new TextBlock (new Run ("selectSet")),
                            Visibility = Visibility.Collapsed
                        };
                        bool selectv = false;
                        tb.Inlines.Add(selectb);
                        tb.Inlines.Add (new LineBreak ());
                        foreach (SelectSet set in setList) {
                            TextBlock sb = new();
                            sb.Inlines.Add (new Run ("SELECT( ") { Foreground = Brushes.Gray });
                            sb.Inlines.Add (set.Rule.Name);
                            sb.Inlines.Add (new Run (" -> ") { Foreground = Brushes.Gray });
                            if (set.Production != null) {
                                foreach (Symbol s in set.Production.Symbols) {
                                    sb.Inlines.Add (s.Name);
                                }
                            }
                            sb.Inlines.Add (new Run (" ) = { ") { Foreground = Brushes.Gray });
                            for (int i = 0; i < set.Terminal.Count; i++) {
                                if (i == 0) {
                                    sb.Inlines.Add (set.Terminal[i]);
                                } else {
                                    sb.Inlines.Add (new Run (" , ") { Foreground = Brushes.Gray });
                                    sb.Inlines.Add (set.Terminal[i]);
                                }
                            }
                            sb.Inlines.Add (new Run (" }") { Foreground = Brushes.Gray });
                            tb.Inlines.Add (Border (sb, set.Address == selectSet, Brushes.PaleGreen));
                            selectv |= set.Address == selectSet;
                            tb.Inlines.Add (new LineBreak ());
                        }
                        if (selectv) {
                            selectb.Visibility = Visibility.Visible;
                        }
                    });
                }
            });
        }

        protected void DrawParsingTable (int i, string label) {
            GetParsingTable (label, parsingTable => {
                if (parsingTable != null && parsingTable.TableHead.Count > 0) {
                    if (DataHash.ContainsKey (label) && parsingTable.Equals (DataHash[label] as ParsingTable)
                        && !CheckWatchedValueChange ("DrawParsingTable_", "foundProduction", "isLL1Rule")) {
                        return;
                    }
                    WatchedValue.TryGetValue ("foundProduction", out string pAddress);
                    WatchedValue.TryGetValue("isLL1Rule", out string isLL1Rule);
                    DataHash[label] = parsingTable;
                    UpdateUI (i, tb => {
                        tb.Inlines.Clear ();
                        tb.Inlines.Add (label + ":");
                        tb.Inlines.Add (new LineBreak ());
                        tb.Inlines.Add (NewBorder (new TextBlock (), 1, 1, 1, 1));
                        foreach (string h in parsingTable.TableHead) {
                            tb.Inlines.Add (NewBorder (new TextBlock (new Run (h)), 0, 1, 1, 1));
                        }
                        tb.Inlines.Add (new LineBreak ());
                        foreach (ParsingTable.Row r in parsingTable.TableRows) {
                            tb.Inlines.Add (NewBorder (new TextBlock (new Run (r.Rule == null ? "" : r.Rule.Name)), 1, 0, 1, 1));
                            foreach ((string, Production) c in r.Productions) {
                                TextBlock t = new();
                                if (c.Item2 != null) {
                                    foreach (Symbol s in c.Item2.Symbols) {
                                        t.Inlines.Add (s.Name);
                                    }
                                }
                                tb.Inlines.Add (NewBorder (t, 0, 0, 1, 1, 
                                    c.Item1 == pAddress ? 
                                        ((isLL1Rule is null || "1".Equals(isLL1Rule)) ? Brushes.PaleGreen : Brushes.SandyBrown) : 
                                        Brushes.White));
                            }
                            tb.Inlines.Add (new LineBreak ());
                        }
                    });
                }
            });
        }

        public void GetParsingTable (string address, Action<ParsingTable> AfterGetParsingTable) {
            gdb.SendScript ("getparsingtable " + address, r => AfterGetParsingTable (ParsingTable.Gen (r)));
        }

        public void GetSelectSetList (string address, Action<List<SelectSet>> AfterGetSelectSetList) {
            gdb.SendScript ("getselectsetlist " + address, r => {
                List<SelectSet> selectsetList = new();
                string[] ruleStrings = r.Split (new string[] { "~\"|selectsetlist|\"" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in ruleStrings) {
                    SelectSet set = SelectSet.Gen (s);
                    if (set != null) {
                        selectsetList.Add (set);
                    }
                }
                AfterGetSelectSetList (selectsetList);
            });
        }
    }
}