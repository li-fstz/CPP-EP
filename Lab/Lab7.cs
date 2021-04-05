using CPP_EP.Execute;

using System;
using System.Collections.Generic;

namespace CPP_EP.Lab {
    class Lab7: AbstractLab {
        public override List<string> LabFiles => new List<string> () { "lab7.c", "src\\rule.c", "src\\removeleftrecursion2.c" };

        public override int LabNo => 7;
        public override void Build () {
            Util.ThreadRun (() => {
                new GCC ()
                .Compile ("src\\rule.c", "build\\obj\\rule.o")
                .Compile ("src\\removeleftrecursion2.c", "build\\obj\\removeleftrecursion2.o")
                .Link ("build\\lab7.exe", "build\\obj\\rule.o", "build\\obj\\removeleftrecursion2.o", "build\\obj\\lab7.o");
            });
        }
        public override void Draw () {
            throw new NotImplementedException ();
        }
    }
}
