﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CPP_EP.Lab.Data {

    public class VoidTable: GDBData {
        public List<string> TableHead;
        public List<bool?> HasVoid;

        private VoidTable (string a, string s) : base (a, s) {
        }

        public static VoidTable Gen (string s) {
            if (s == null) {
                return null;
            }

            VoidTable v = null;
            string[] structs = s.Split (new string[] { "~\"|voidtable|\"" }, StringSplitOptions.RemoveEmptyEntries);
            if (structs.Length > 0) {
                MatchCollection ms = Text.Matches (structs[0]);
                if (ms.Count > 1 && ms.First ().Success) {
                    string address = ms.First ().Groups[1].Value;
                    VoidTable h = Get<VoidTable> (address, s);
                    if (h is VoidTable) {
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