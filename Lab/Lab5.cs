using CPP_EP.Execute;

using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace CPP_EP.Lab {
    class Lab5: AbstractLab {
        public override List<string> LabFiles => new List<string> () { "lab5.c", "src\\rule.c", "src\\pickupleftfactor.c" };
        public override int LabNo => 5;
        public override void Build () {
            Util.ThreadRun (() => {
                new GCC ()
                .Compile ("src\\rule.c", "build\\obj\\rule.o")
                .Compile ("src\\pickupleftfactor.c", "build\\obj\\pickupleftfactor.o")
                .Compile ("lab5.c", "build\\obj\\lab5.o")
                .Link ("build\\lab5.exe");
            });
        }
        public override void Draw () {
            DrawRules (1, "ruleHead");
            DrawRules (2, "newRule");
        }
    }
}
