using System.Collections.Generic;

using CPP_EP.Execute;

namespace CPP_EP.Lab {

    internal class Lab7: AbstractLab {
        private readonly List<string> _LabFiles = new List<string> () {
            "lab7.c",
            "src\\rule.c",
            "src\\removeleftrecursion2.c",
            "inc\\removeleftrecursion2.h",
            "inc\\rule.h"
        };
        public override List<string> LabFiles => _LabFiles;
        public override int LabNo => 7;

        public override void Build () {
            Util.ThreadRun (() => {
                new GCC ()
                .Compile ("src\\rule.c", "build\\obj\\rule.o")
                .Compile ("src\\removeleftrecursion2.c", "build\\obj\\removeleftrecursion2.o")
                .Compile ("lab7.c", "build\\obj\\lab7.o")
                .Link ("build\\lab7.exe");
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