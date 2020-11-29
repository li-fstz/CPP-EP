using CPP_EP.Execute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xceed.Wpf.AvalonDock.Layout;


namespace CPP_EP.View {
    class Lab1: Lab {
        public Lab1 (GDB gdb) : base (gdb) { }

        public override void Draw (LayoutAnchorable layout) {
            throw new NotImplementedException ();
        }
        public VoidTable GetVoidTable(string address) {
            VoidTable voidTable = null;
            try {
                int colCount = GetInt("(" + address + ")->ColCount");
                voidTable = new VoidTable () {
                    TableHead = GetTableHead ("(" + address + ")->pTableHead", colCount),
                    HasVoid = GetHasVoid ("(" + address + ")->TableRows[0].hasVoid", colCount)
                };
            } catch (Exception) { }

            return voidTable;
        }
        public class VoidTable {
            public List<string> TableHead;
            public List<bool?> HasVoid;
        }
        private List<string> GetTableHead(string address, int n) {
            List<string> tableHead = new List<string>();
            for (int i = 0; i < n; i ++) {
                tableHead.Add (GetText(address + "[" + i + "]"));
            }
            return tableHead;
        }
        private List<bool?> GetHasVoid(string address, int n) {
            List<bool?> hasVoid = new List<bool?>();
            for (int i = 0; i < n; i ++) {
                int has = GetInt(address + "[" + i + "]");
                if (has == 0) {
                    hasVoid.Add (false);
                } else if (has == -1) {
                    hasVoid.Add (null);
                } else {
                    hasVoid.Add (true);
                }
            }
            return hasVoid;
        }
    }
}
