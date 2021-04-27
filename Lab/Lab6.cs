using CPP_EP.Execute;

using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace CPP_EP.Lab {
    class Lab6: AbstractLab {
                
        public override List<string> LabFiles => new List<string> () { "lab6.c", "src\\rule.c", "src\\removeleftrecursion1.c" };

        public override int LabNo => 6;
        public override void Build () {
            Util.ThreadRun (() => {
                new GCC ()
                .Compile ("src\\rule.c", "build\\obj\\rule.o")
                .Compile ("src\\removeleftrecursion1.c", "build\\obj\\removeleftrecursion1.o")
                .Link ("build\\lab6.exe", "build\\obj\\rule.o", "build\\obj\\removeleftrecursion1.o", "build\\obj\\lab6.o");
            });
        }
        public override void Draw () {
            throw new NotImplementedException ();
        }
    }
}
