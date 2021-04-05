using CPP_EP.Execute;

using System;
using System.Collections.Generic;

namespace CPP_EP.Lab {
    class Lab8: Lab4 {
               
        public override List<string> LabFiles => new List<string> () { "lab8.c", "src\\rule.c", "src\\voidtable.c", "src\\first.c", "src\\follow.c", "src\\parsingtable.c", "src\\parser.c" };

        public override int LabNo => 8;
        public override void Build () {
            Util.ThreadRun (() => {
                new GCC ()
                .Compile ("src\\rule.c", "build\\obj\\rule.o")
                .Compile ("src\\voidtable.c", "build\\obj\\voidtable.o")
                .Compile ("src\\first.c", "build\\obj\\first.o")
                .Compile ("src\\follow.c", "build\\obj\\follow.o")
                .Compile ("src\\parsingtable.c", "build\\obj\\parsingtable.o")
                .Compile ("src\\parser.c", "build\\obj\\parser.o")
                .Compile ("lab8.c", "build\\obj\\lab8.o")
                .Link ("build\\lab8.exe", "build\\obj\\rule.o", "build\\obj\\voidtable.o", "build\\obj\\first.o", "build\\obj\\follow.o", "build\\obj\\parsingtable.o", "build\\obj\\parser.o", "build\\obj\\lab8.o");
            });
        }
        public override void Draw () {
            throw new NotImplementedException ();
        }
        public List<Symbol> GetParsingStack (string address) {
            List<Symbol> stack = new List<Symbol> ();
            return stack;
        }
    }
}
