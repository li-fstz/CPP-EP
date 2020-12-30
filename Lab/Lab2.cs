using CPP_EP.Execute;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CPP_EP.Lab {
    class Lab2: Lab1 {

        public Lab2 (GDB gdb) : base (gdb) { }

        public override void Draw () {
            throw new NotImplementedException ();
        }

        public static Dictionary<string, Set> SetHash = new Dictionary<string, Set> ();


        public void GetSetList (string address, Action<List<Set>> AfterGetSetList) {
            gdb.SendScript ("getsetlist " + address, r => {
                List<Set> setList = new List<Set> ();
                string[] ruleStrings = r.Split (new string[] { "~\"|\"" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in ruleStrings) {
                    setList.Add (new Set (s));
                }
                AfterGetSetList (setList);
            });
        }
    }
    public class Set {
        public string Name;
        public string Address;
        public List<string> Terminal;
        public Set (string s) {
            Match m = Lab2.AddressToSymbolInQuot.Match (s);
            if (m.Success) {
                Address = m.Groups[1].Value;
                Name = m.Groups[2].Value;
                Terminal = new List<string> ();
                MatchCollection ms = AbstractLab.Text.Matches (s);
                for (int i = 1; i < ms.Count; i++) {
                    Terminal.Add (ms[i].Groups[1].Value);
                }
                Lab2.SetHash[Address] = this;
            } else {
                throw new Exception ("Parsing Set Error: " + s);
            }
        }
    }
}
