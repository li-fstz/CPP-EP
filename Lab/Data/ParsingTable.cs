using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CPP_EP.Lab.Data {

    public class ParsingTable: GDBData {

        public class Row {
            public Rule Rule;
            public List<(string, Production)> Productions;
        }

        public List<string> TableHead;
        public List<Row> TableRows;

        public static readonly Regex AddressToAddress = new Regex (@"(0x[0-9a-f]+)=>(0x[0-9a-f]+)");

        private ParsingTable (string a, string s) : base (a, s) {
        }

        public static ParsingTable Gen (string s) {
            ParsingTable p = null;
            string[] structs = s.Split (new string[] { "~\"|parsingtable|\"" }, StringSplitOptions.RemoveEmptyEntries);
            if (structs.Length > 0) {
                MatchCollection ms = Text.Matches (structs[0]);
                if (ms.Count > 0 && ms.First ().Success) {
                    string address = ms.First ().Groups[1].Value;
                    ParsingTable h = Get<ParsingTable> (address);
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
                            List<(string, Production)> Productions = new List<(string, Production)> ();
                            Match m = Text.Match (structs[i]);
                            if (m.Success) {
                                p.TableRows.Add (new Row () { Rule = Get<Rule> (m.Groups[1].Value), Productions = Productions });
                                ms = AddressToAddress.Matches (structs[i]);
                                for (int j = 0; j < ms.Count; j++) {
                                    if (ms[j].Success) {
                                        Productions.Add ((ms[j].Groups[1].Value, Get<Production> (ms[j].Groups[2].Value)));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return p;
        }
    }
}