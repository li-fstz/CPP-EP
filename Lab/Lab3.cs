using CPP_EP.Execute;

using System;
using System.Collections.Generic;

namespace CPP_EP.Lab {
    class Lab3: Lab2 {
                
        public override List<string> LabFiles => new List<string> () { "lab3.c", "src\\rule.c", "src\\voidtable.c", "src\\first.c", "src\\follow.c" };

        public override int LabNo => 3;
        public override void Build () {
            Util.ThreadRun (() => {
                new GCC ("C:\\MinGW\\bin\\gcc.exe")
                .Compile ("src\\rule.c", "build\\obj\\rule.o")
                .Compile ("src\\voidtable.c", "build\\obj\\voidtable.o")
                .Compile ("src\\first.c", "build\\obj\\first.o")
                .Compile ("src\\follow.c", "build\\obj\\follow.o")
                .Compile ("lab3.c", "build\\obj\\lab3.o")
                .Link ("build\\lab3.exe", "build\\obj\\rule.o", "build\\obj\\voidtable.o", "build\\obj\\first.o", "build\\obj\\follow.o", "build\\obj\\lab3.o");
            });
        }
        public override void Draw () {
            throw new NotImplementedException ();
        }
    }
}
