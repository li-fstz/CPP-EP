using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CPP_EP.Lab.Data {

    public class Stack: GDBData {
        public List<string> Symbols;

        private Stack (string a, string s) : base (a, s) {
        }

        public static Stack Gen (string s) {
            Stack stack = null;
            MatchCollection ms = Text.Matches (s);
            if (ms.Count > 0 && ms.First ().Success) {
                string address = ms.First ().Groups[1].Value;
                Stack h = Get<Stack> (address, s);
                if (h is Stack) {
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