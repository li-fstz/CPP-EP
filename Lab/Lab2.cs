using CPP_EP.Execute;

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
                .Link ("build\\lab2.exe", "build\\obj\\rule.o", "build\\obj\\voidtable.o", "build\\obj\\first.o", "build\\obj\\lab2.o");
            });
        }
        public static Dictionary<string, Set> SetHash = new Dictionary<string, Set> ();
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

        public class Set {
            public string Name;
            public string Address;
            public List<string> Terminal;
            private Set () { }
            public override bool Equals (object obj) {
                Set s = obj as Set;
                return s == this || (s != null && s.Address == Address && s.Name == Name && s.Terminal.SequenceEqual (Terminal));
            }
            public static Set GenSet (string s) {
                Set set = null;
                Match m = AddressToSymbolInQuot.Match (s);
                if (m.Success) {
                    set = new Set () {
                        Address = m.Groups[1].Value,
                        Name = m.Groups[2].Value,
                        Terminal = new List<string> (),
                    };
                    MatchCollection ms = Text.Matches (s);
                    for (int i = 1; i < ms.Count; i++) {
                        set.Terminal.Add (ms[i].Groups[1].Value);
                    }
                    if (set.Equals (Get (SetHash, set.Address))) {
                        set = Get (SetHash, set.Address);
                    } else {
                        SetHash[set.Address] = set;
                    }
                } else {
                    //throw new Exception ("Parsing Set Error: " + s);
                }
                return set;
            }
        }
    }
}
