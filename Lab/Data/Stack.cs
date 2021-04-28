using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CPP_EP.Lab.Data {

    internal class Stack: GDBData {
        public List<string> Symbols;

        private Stack (string a, string s) : base (a, s) {
        }

        public static Stack Gen (string s) {
            Stack stack = null;
            var ms = Text.Matches (s);
            if (ms.Count > 0 && ms.First ().Success) {
                var address = ms.First ().Groups[1].Value;
                var h = Get<Stack> (address);
                if (h != null && h.GetHashCode () == s.GetHashCode ()) {
                    stack = h;
                } else {
                    stack = new Stack (address, s) {
                        Symbols = new List<string> ()
                    };
                    foreach (Match m in ms) {
                        if (m != ms.First () && m.Success) {
                            stack.Symbols.Add (m.Groups[1].Value);
                        }
                    }
                }
            }
            return stack;
        }
    }
}