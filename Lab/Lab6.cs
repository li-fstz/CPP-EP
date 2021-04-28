using System.Collections.Generic;

using CPP_EP.Execute;

namespace CPP_EP.Lab {

    internal class Lab6: AbstractLab {
        private readonly List<string> _LabFiles = new List<string> () { "lab6.c", "src\\rule.c", "src\\removeleftrecursion1.c" };

        public override List<string> LabFiles => _LabFiles;

        public override int LabNo => 6;

        public override void Build () {
            Util.ThreadRun (() => {
                new GCC ()
                .Compile ("src\\rule.c", "build\\obj\\rule.o")
                .Compile ("src\\removeleftrecursion1.c", "build\\obj\\removeleftrecursion1.o")
                .Compile ("lab6.c", "build\\obj\\lab6.o")
                .Link ("build\\lab6.exe");
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