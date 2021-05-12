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

    internal class Lab2: Lab1 {
        private readonly List<string> _LabFiles = new() {
            "lab2.c",
            "src\\rule.c",
            "src\\voidtable.c",
            "src\\first.c",
            "inc\\first.h",
            "inc\\voidtable.h",
            "inc\\rule.h"
        };

        public override List<string> LabFiles => _LabFiles;

        public override int LabNo => 2;

        public override void Draw () {
            WatchValues (() => {
                DrawRules (1, "ruleHead");
                DrawVoidTable (2, "voidTable");
                DrawSetList (3, "firstSetList", "FIRST");
            }, "rule", "production", "symbol", "desSet", "srcSet");
        }

        protected void DrawSetList (int i, string label, string type) {
            GetSetList (label, setList => {
                if (setList != null && setList.Count > 0) {
                    if (DataHash.ContainsKey (label) && setList.SequenceEqual (DataHash[label] as List<Set>)
                        && !CheckWatchedValueChange ("DrawSetList_" + label, "desSet", "srcSet")) {
                        return;
                    }
                    WatchedValue.TryGetValue ("desSet", out string desSet);
                    WatchedValue.TryGetValue ("srcSet", out string srcSet);
                    DataHash[label] = setList;
                    UpdateUI (i, tb => {
                        tb.Inlines.Clear ();
                        tb.Inlines.Add (label + ":");
                        Border desb = new() {
                            Background = Brushes.PaleGreen,
                            Child = new TextBlock (new Run ("desSet")),
                            Visibility = Visibility.Collapsed
                        };
                        Border srcb = new() {
                            Background = Brushes.SandyBrown,
                            Child = new TextBlock (new Run ("srcSet")),
                            Visibility = Visibility.Collapsed
                        };
                        bool desv = false, srcv = false;
                        tb.Inlines.Add (desb);
                        tb.Inlines.Add (srcb);
                        tb.Inlines.Add (new LineBreak ());
                        foreach (Set set in setList) {
                            TextBlock sb = new();
                            sb.Inlines.Add (new Run (type + "( ") { Foreground = Brushes.Gray });
                            sb.Inlines.Add (set.Name);
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
                            if (set.Address == srcSet) {
                                tb.Inlines.Add (Border (sb, true, Brushes.SandyBrown));
                                srcv = true;
                            } else if (set.Address == desSet) {
                                tb.Inlines.Add (Border (sb, true, Brushes.PaleGreen));
                                desv = true;
                            } else {
                                tb.Inlines.Add (sb);
                            }
                            if (desv) {
                                desb.Visibility = Visibility.Visible;
                            }
                            if (srcv) {
                                srcb.Visibility = Visibility.Visible;
                            }
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
                List<Set> setList = new();
                string[] ruleStrings = r.Split (new string[] { "~\"|setlist|\"" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in ruleStrings) {
                    Set set = Set.Gen (s);
                    if (set != null) {
                        setList.Add (Set.Gen (s));
                    }
                }
                AfterGetSetList (setList);
            });
        }
    }
}