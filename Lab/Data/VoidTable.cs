using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CPP_EP.Lab.Data {
    public class VoidTable: GDBData {
        public List<string> TableHead;
        public List<bool?> HasVoid;
        private VoidTable (string a, string s) : base (a, s) { }
        public static VoidTable GenVoidTable (string s) {
            if (s == null) return null;
            VoidTable v = null;
            string[] structs = s.Split (new string[] { "~\"|voidtable|\"" }, StringSplitOptions.RemoveEmptyEntries);
            if (structs.Length > 0) {
                var ms = Text.Matches (structs[0]);
                if (ms.Count > 0 && ms.First ().Success) {
                    var address = ms.First ().Groups[1].Value;
                    var h = Get<VoidTable> (address);
                    if (h != null && h.GetHashCode () == s.GetHashCode ()) {
                        v = h;
                    } else {
                        v = new VoidTable (address, s) {
                            TableHead = new List<string> (),
                            HasVoid = new List<bool?> ()
                        };
                        foreach (Match m in ms) {
                            if (m != ms.First () && m.Success) {
                                v.TableHead.Add (m.Groups[1].Value);
                            }
                        }
                        if (structs.Length > 1) {
                            foreach (Match m in Text.Matches (structs[1])) {
                                if (m.Success) {
                                    if (m.Groups[1].Value == "1") {
                                        v.HasVoid.Add (true);
                                    } else if (m.Groups[1].Value == "0") {
                                        v.HasVoid.Add (false);
                                    } else {
                                        v.HasVoid.Add (null);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return v;
        }
    }
}
