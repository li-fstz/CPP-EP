using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CPP_EP.Lab.Data {

    public class SelectSet: GDBData {
        public Rule Rule;
        public Production Production;
        public List<string> Terminal;
        public static readonly Regex AddressToAddressToAddress = new Regex (@"(0x[0-9a-f]+)=>(0x[0-9a-f]+)=>(0x[0-9a-f]+)");

        private SelectSet (string a, string s) : base (a, s) {
        }

        public static SelectSet Gen (string s) {
            SelectSet set = null;
            Match m = AddressToAddressToAddress.Match (s);
            if (m.Success) {
                var address = m.Groups[1].Value;
                var h = Get<SelectSet> (address);
                if (h != null && h.GetHashCode () == s.GetHashCode ()) {
                    set = h;
                } else {
                    set = new SelectSet (address, s) {
                        Rule = Get<Rule> (m.Groups[2].Value),
                        Production = Get<Production> (m.Groups[3].Value),
                        Terminal = new List<string> ()
                    };
                    MatchCollection ms = Text.Matches (s);
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