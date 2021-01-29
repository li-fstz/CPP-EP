using CPP_EP.Execute;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CPP_EP.Lab {
    class Lab2: Lab1 {
                
        public override List<string> LabFiles => new List<string> () { "lab2.c", "src\\rule.c", "src\\voidtable.c", "src\\first.c" };

        public override int LabNo => 2;
        public override void Draw () {
            throw new NotImplementedException ();
        }
        public override void Build () {
            Util.ThreadRun (() => {
                new GCC ("C:\\MinGW\\bin\\gcc.exe")
                .Compile ("src\\rule.c", "build\\obj\\rule.o")
                .Compile ("src\\voidtable.c", "build\\obj\\voidtable.o")
                .Compile ("src\\first.c", "build\\obj\\first.o")
                .Compile ("lab2.c", "build\\obj\\lab2.o")
                .Link ("build\\lab2.exe", "build\\obj\\rule.o", "build\\obj\\voidtable.o", "build\\obj\\first.o", "build\\obj\\lab2.o");
            });
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
