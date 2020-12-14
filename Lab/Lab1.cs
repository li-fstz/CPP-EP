using CPP_EP.Execute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xceed.Wpf.AvalonDock.Layout;


namespace CPP_EP.Lab {
    class Lab1: Lab {
        public Lab1 (GDB gdb) : base (gdb) { }

        private RuleLayout layout1 = null;
        private RuleLayout layout2 = null;

        public override void Draw (LayoutAnchorablePane layoutAnchorablePane) {
            
            VoidTable voidTable;

            
            try {
                RuleLayout ruleLayout = new RuleLayout ();
                List<Rule> rules = GetRules ("pHead");
                ruleLayout.Draw (rules);
                if (layout1 != null) {
                    layoutAnchorablePane.Children.Remove (layout1);
                }
                layout1 = ruleLayout;
                layoutAnchorablePane.Children.Add (layout1);
            } catch {}
            try {
                voidTable = GetVoidTable ("&VoidTable");
            } catch { }
        }
        
        public VoidTable GetVoidTable(string address) {
            return new VoidTable (gdb.SendScript ("getvoidtable " + address));
        }

    }
    public class VoidTable {
        public List<string> TableHead;
        public List<bool?> HasVoid;
        public VoidTable (string s) {
            if (s == null) {
                throw new Exception ("Parsing VoidTable Error: " + s);
            }
            string[] structs = s.Split (new string[] { "~\"|\"" }, StringSplitOptions.RemoveEmptyEntries);
            TableHead = new List<string> ();
            foreach (Match m in Lab.Text.Matches (structs[0])) {
                TableHead.Add (m.Groups[1].Value);
            }
            HasVoid = new List<bool?> ();
            foreach (Match m in Lab.Text.Matches (structs[1])) {
                if (m.Groups[1].Value == "1") {
                    HasVoid.Add (true);
                } else if (m.Groups[1].Value == "0") {
                    HasVoid.Add (false);
                } else {
                    HasVoid.Add (null);
                }
            }
        }
    }
}
