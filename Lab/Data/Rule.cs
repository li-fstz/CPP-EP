using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CPP_EP.Lab.Data {

    public class Rule: GDBData {
        public string Name;
        public List<Production> Productions;

        private Rule (string a, string s) : base (a, s) {
        }

        public static Rule Gen (string s) {
            if (s == null) return null;
            Rule r = null;
            string[] ProductionStrings = s.Split (new string[] { "~\"|production|\"" }, StringSplitOptions.RemoveEmptyEntries);
            if (ProductionStrings.Length > 0) {
                Match m = AddressToSymbolInQuot.Match (ProductionStrings[0]);
                if (m.Success) {
                    var address = m.Groups[1].Value;
                    var h = Get<Rule> (address);
                    if (h != null && h.GetHashCode () == s.GetHashCode ()) {
                        r = h;
                    } else {
                        r = new Rule (address, s) {
                            Name = m.Groups[2].Value,
                            Productions = new List<Production> ()
                        };
                        for (int i = 1; i < ProductionStrings.Length; i++) {
                            var p = Production.Gen (ProductionStrings[i]);
                            if (p != null) {
                                r.Productions.Add (p);
                            }
                        }
                    }
                }
            }
            return r;
        }

        public static List<Rule> GenRules (string s) {
            List<Rule> rules = new List<Rule> ();
            string[] ruleStrings = s.Split (new string[] { "~\"|rule|\"" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var ruleString in ruleStrings) {
                var rule = Gen (ruleString);
                if (rule != null) rules.Add (rule);
            }
            return rules;
        }
    }
}