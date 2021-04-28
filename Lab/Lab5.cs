using System.Collections.Generic;

using CPP_EP.Execute;

namespace CPP_EP.Lab {

    internal class Lab5: AbstractLab {
        private readonly List<string> _LabFiles = new List<string> () { "lab5.c", "src\\rule.c", "src\\pickupleftfactor.c" };
        public override List<string> LabFiles => _LabFiles;
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
            WatchValues (() => {
                DrawRules (1, "ruleHead");
                DrawRules (2, "newRule");
            }, "rule", "production", "symbol");
        }
    }
}