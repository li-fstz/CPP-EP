using CPP_EP.Execute;

using System;
using System.Collections.Generic;

namespace CPP_EP.Lab {
    class Lab8: Lab4 {
        public Lab8 (GDB gdb) : base (gdb) { }

        public override void Draw () {
            throw new NotImplementedException ();
        }
        public List<Symbol> GetParsingStack (string address) {
            List<Symbol> stack = new List<Symbol> ();
            return stack;
        }
    }
}
