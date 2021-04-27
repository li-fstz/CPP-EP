using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CPP_EP.Lab.Data {
    public class ParsingTable: GDBData {
        public class Row {
            public Rule Rule;
            public List<Production> Productions;
        }
        public List<string> TableHead;
        public List<Row> TableRows;
        private ParsingTable (string a, string s) : base (a, s) { }
        public static ParsingTable GenParsingTabele (string s) {
            ParsingTable p = null;
            string[] structs = s.Split (new string[] { "~\"|parsingtable|\"" }, StringSplitOptions.RemoveEmptyEntries);
            if (structs.Length > 0) {
                var ms = Text.Matches (structs[0]);
                if (ms.Count > 0 && ms.First ().Success) {
                    var address = ms.First ().Groups[1].Value;
                    var h = Get<ParsingTable> (address);
                    if (h != null && h.GetHashCode () == s.GetHashCode ()) {
                        p = h;
                    } else {
                        p = new ParsingTable (address, s) {
                            TableHead = new List<string> (),
                            TableRows = new List<Row> ()
                        };
                        foreach (Match m in ms) {
                            if (m != ms.First () && m.Success) {
                                p.TableHead.Add (m.Groups[1].Value);
                            }
                        }
                        for (int i = 1; i < structs.Length; i++) {
                            ms = Text.Matches (structs[i]);
                            List<Production> Productions = new List<Production> ();
                            if (ms.Count > 0 && ms.First ().Success) {
                                p.TableRows.Add (new Row () { Rule = Get<Rule> (ms.First ().Groups[1].Value), Productions = Productions });
                            }
                            for (int j = 1; j < ms.Count; j++) {
                                Productions.Add (Get<Production> (ms[j].Groups[1].Value));
                            }
                        }
                    }
                }
            }
            return p;
        }
    }
}
