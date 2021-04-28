using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CPP_EP.Lab.Data {

    public class Set: GDBData {
        public string Name;
        public List<string> Terminal;

        private Set (string a, string s) : base (a, s) {
        }

        public static Set Gen (string s) {
            Set set = null;
            Match m = AddressToSymbolInQuot.Match (s);
            if (m.Success) {
                var address = m.Groups[1].Value;
                var h = Get<Set> (address);
                if (h != null && h.GetHashCode () == s.GetHashCode ()) {
                    set = h;
                } else {
                    set = new Set (address, s) {
                        Name = m.Groups[2].Value,
                        Terminal = new List<string> ()
                    };
                    var ms = Text.Matches (s);
                    for (int i = 1; i < ms.Count; i++) {
                        if (ms[i].Success) {
                            set.Terminal.Add (ms[i].Groups[1].Value);
                        }
                    }
                }
            }
            return set;
        }
    }
}