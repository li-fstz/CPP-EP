using CPP_EP.Execute;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CPP_EP.Lab {
    class Lab4: Lab3 {
                
        public override List<string> LabFiles => new List<string> () { "lab4.c", "src\\rule.c", "src\\voidtable.c", "src\\first.c", "src\\follow.c", "src\\parsingtable.c" };

        public override int LabNo => 4;
        public override void Build () {
            Util.ThreadRun (() => {
                new GCC ("C:\\MinGW\\bin\\gcc.exe")
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
            throw new NotImplementedException ();
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
            public ParsingTable (string s) {
                string[] structs = s.Split (new string[] { "~\"|\"" }, StringSplitOptions.RemoveEmptyEntries);
                TableHead = new List<string> ();
                foreach (Match m in Text.Matches (structs[0])) {
                    TableHead.Add (m.Groups[1].Value);
                }
                TableRows = new List<Row> ();
                for (int i = 1; i < structs.Length; i++) {
                    var ms = Text.Matches (structs[i]);
                    List<Production> Productions = new List<Production> ();
                    for (int j = 1; j < ms.Count; j++) {
                        Productions.Add (GetProduction (ms[j].Groups[1].Value));
                    }
                    TableRows.Add (new Row () { Rule = GetRule (ms[0].Groups[1].Value), Productions = Productions });
                }
            }

        }
        public void GetParsingTable (string address, Action<ParsingTable> AfterGetParsingTable) {
            gdb.SendScript ("getparsing " + address, r => AfterGetParsingTable (new ParsingTable (r)));
        }
        public void GetSelectSetList (string address, Action<List<SelectSet>> AfterGetSelectSetList) {
            gdb.SendScript ("getselectsetlist " + address, r => {
                List<SelectSet> selectsetList = new List<SelectSet> ();
                string[] ruleStrings = r.Split (new string[] { "~\"|\"" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in ruleStrings) {
                    selectsetList.Add (new SelectSet (s));
                }
                AfterGetSelectSetList (selectsetList);
            });
        }
    }
    public class SelectSet {
        public Rule Rule;
        public Production Production;
        public string Address;
        public List<string> Terminal;
        public SelectSet (string s) {
            Match m = Lab4.AddressToAddressToAddress.Match (s);
            if (m.Success) {
                Address = m.Groups[1].Value;
                Rule = AbstractLab.RuleHash[m.Groups[2].Value];
                Production = AbstractLab.ProductionHash[m.Groups[3].Value];
                Terminal = new List<string> ();
                MatchCollection ms = AbstractLab.Text.Matches (s);
                for (int i = 1; i < ms.Count; i++) {
                    Terminal.Add (ms[i].Groups[1].Value);
                }
                Lab4.SelectSetHash[Address] = this;
            } else {
                throw new Exception ("Parsing SelectSet Error: " + s);
            }
        }
    }
}
