using CPP_EP.Execute;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace CPP_EP.Lab {
    class Lab1: AbstractLab {

        public override List<string> LabFiles => new List<string> () { "lab1.c", "src\\rule.c", "src\\voidtable.c" };

        public override int LabNo => 1;

        public override void Build () {
            Util.ThreadRun (() => {
                new GCC ("C:\\MinGW\\bin\\gcc.exe")
                .Compile ("src\\rule.c", "build\\obj\\rule.o")
                .Compile ("src\\voidtable.c", "build\\obj\\voidtable.o")
                .Compile ("lab1.c", "build\\obj\\lab1.o")
                .Link ("build\\lab1.exe", "build\\obj\\rule.o", "build\\obj\\voidtable.o", "build\\obj\\lab1.o");
            });
        }

        public override void Draw () {
        }

        public void GetVoidTable (string address, Action<VoidTable> AfterGetVoidTable) {
            gdb.SendScript ("getvoidtable " + address, r => AfterGetVoidTable (new VoidTable (r)));
        }

    }
    public class VoidTable {
        public List<string> TableHead;
        public List<bool?> HasVoid;
        public VoidTable (string s) {
            if (s == null) {
                throw new Exception ("Parsing VoidTable Error: " + s);
            }
            string[] structs = s.Split (new string[] { "~\"|\"" }, StringSplitOptions.RemoveEmptyEntries);
            TableHead = new List<string> ();
            foreach (Match m in AbstractLab.Text.Matches (structs[0])) {
                TableHead.Add (m.Groups[1].Value);
            }
            HasVoid = new List<bool?> ();
            foreach (Match m in AbstractLab.Text.Matches (structs[1])) {
                if (m.Groups[1].Value == "1") {
                    HasVoid.Add (true);
                } else if (m.Groups[1].Value == "0") {
                    HasVoid.Add (false);
                } else {
                    HasVoid.Add (null);
                }
            }
        }
    }
}
