using CPP_EP.Execute;

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
                .Link ("build\\lab4.exe", "build\\obj\\rule.o", "build\\obj\\voidtable.o", "build\\obj\\first.o", "build\\obj\\follow.o", "build\\obj\\parsingtable.o", "build\\obj\\lab4.o");
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
        public static readonly Regex AddressToAddressToAddress = new Regex (@"(0x[0-9a-f]+)=>(0x[0-9a-f]+)=>(0x[0-9a-f]+)");
        public static Dictionary<string, SelectSet> SelectSetHash = new Dictionary<string, SelectSet> ();
        public class ParsingTable {
            public class Row {
                public Rule Rule;
                public List<Production> Productions;
            }
            public List<string> TableHead;
            public List<Row> TableRows;
            private ParsingTable () {}
            public static ParsingTable GenParsingTabele (string s) {
                string[] structs = s.Split (new string[] { "~\"|parsingtable|\"" }, StringSplitOptions.RemoveEmptyEntries);
                ParsingTable p = new ParsingTable () {
                    TableHead = new List<string> (),
                    TableRows = new List<Row> ()
                };
                foreach (Match m in Text.Matches (structs[0])) {
                    p.TableHead.Add (m.Groups[1].Value);
                }
                for (int i = 1; i < structs.Length; i++) {
                    var ms = Text.Matches (structs[i]);
                    List<Production> Productions = new List<Production> ();
                    for (int j = 1; j < ms.Count; j++) {
                        Productions.Add (Get (ProductionHash, ms[j].Groups[1].Value));
                    }
                    if (ms.Count > 0) {
                        p.TableRows.Add (new Row () { Rule = Get (RuleHash, ms[0].Groups[1].Value), Productions = Productions });
                    }
                }
                return p;
            }
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

        public class SelectSet {
            public Rule Rule;
            public Production Production;
            public string Address;
            public List<string> Terminal;
            private SelectSet () { }
            public override bool Equals (object obj) {
                SelectSet s = obj as SelectSet;
                return s == this || (s != null && s.Address == Address && s.Rule.Address == Rule.Address && s.Production.Address == Production.Address && s.Terminal.SequenceEqual (Terminal));
            }
            public static SelectSet GenSelectSet (string s) {
                SelectSet set = null;
                Match m = AddressToAddressToAddress.Match (s);
                if (m.Success) {
                    set = new SelectSet () {
                        Address = m.Groups[1].Value,
                        Rule = Get(RuleHash,m.Groups[2].Value),
                        Production = Get(ProductionHash,m.Groups[3].Value),
                        Terminal = new List<string> ()
                    };
                    MatchCollection ms = Text.Matches (s);
                    for (int i = 1; i < ms.Count; i++) {
                        set.Terminal.Add (ms[i].Groups[1].Value);
                    }
                    if (set.Equals (Get (SelectSetHash, set.Address))) {
                        set = Get (SelectSetHash, set.Address);
                    } else {
                        SelectSetHash[set.Address] = set;
                    }
                } else {
                    //throw new Exception ("Parsing SelectSet Error: " + s);
                }
                return set;
            }
        }
    }
}
