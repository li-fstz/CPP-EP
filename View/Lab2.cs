using CPP_EP.Execute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.AvalonDock.Layout;
using System.Text.RegularExpressions;

namespace CPP_EP.View {
    class Lab2: Lab1 {

        public Lab2 (GDB gdb) : base (gdb) { }

        public override void Draw (LayoutAnchorable layout) {
            throw new NotImplementedException ();
        }

        public class Set {
            public string Name;
            public List<string> Terminal;
        }

        public List<Set> GetSetList(string address) {
            List<Set> setList = new List<Set>();
            try {
                int setCount = GetInt("(" + address + ")->nSetCount");
                for (int i = 0; i < setCount; i++) {
                    int terminalCount = GetInt("(" + address + ")->Sets[" + i + "].nTerminalCount");
                    List<string> terminals = new List<string>();
                    for (int j = 0; j < terminalCount; j++) {
                        terminals.Add (GetText("(" + address + ")->Sets[" + i + "].Terminal[" + j + "]"));
                    }
                    setList.Add (new Set () {
                        Name = GetText ("(" + address + ")->Sets[" + i + "].Name"),
                        Terminal = terminals
                    });
                }
            } catch (Exception) { };
            return setList;
        }
    }
}
