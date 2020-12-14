using CPP_EP.Execute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.AvalonDock.Layout;

namespace CPP_EP.Lab {
    class Lab8: Lab4 {
        public Lab8 (GDB gdb) : base (gdb) { }

        public override void Draw (LayoutAnchorablePane layoutAnchorablePane) {
            throw new NotImplementedException ();
        }
        public List<Symbol> GetParsingStack(string address) {
            List<Symbol> stack = new List<Symbol>();
            return stack;
        }
    }
}
