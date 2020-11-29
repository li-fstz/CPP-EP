using CPP_EP.Execute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xceed.Wpf.AvalonDock.Layout;

namespace CPP_EP.View {
    abstract class View {
        protected readonly GDB gdb;
        private static readonly Regex Int = new Regex(@"value=""(\d+)""");
        private static readonly Regex Address = new Regex(@"value=""(0x[0-9a-f]+)""");
        private static readonly Regex Text = new Regex(@"\\""(.+?)\\""");
        public View(GDB gdb) {
            this.gdb = gdb;
        }   
        public abstract void Draw (LayoutAnchorable layout);

        public int GetInt(string value) {
            return int.Parse(Util.RegexGroupOne (Int, gdb.Print (value)));
        }

        public string GetAddress(string value) {
            if (value.IndexOf("0x") == 0) {
                return value;
            } else {
                return Util.RegexGroupOne (Address, gdb.Print (value));
            }
        }

        public string GetText(string address) {
            return Util.RegexGroupOne (Text, gdb.Print (address));
        }
    }
}
