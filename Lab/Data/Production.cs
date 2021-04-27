using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CPP_EP.Lab.Data {

    public class Production: GDBData {
        public List<Symbol> Symbols;
        private Production (string a, string s) : base (a, s) { }
        public static Production GenProduction (string s) {
            Production p = null;
            var ms = Text.Matches (s);
            if (ms.Count > 0 && ms.First ().Success) {
                var address = ms.First ().Groups[1].Value;
                var h = Get<Production> (address);
                if (h != null && h.GetHashCode () == s.GetHashCode ()) {
                    p = h;
                } else {
                    p = new Production (address, s) {
                        Symbols = new List<Symbol> ()
                    };
                    foreach (Match m in ms) {
                        if (m != ms.First () && m.Success) {
                            p.Symbols.Add (Symbol.GenSymbol (m.Groups[1].Value));
                        }
                    }
                }
            }
            return p;
        }
    }
}
