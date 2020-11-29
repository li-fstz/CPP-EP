using CPP_EP.Execute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.AvalonDock.Layout;
using System.Text.RegularExpressions;

namespace CPP_EP.View {
    class ParsingTableView: RuleView {

        public ParsingTableView (GDB gdb) : base (gdb) { }

        public override void Draw (LayoutAnchorable layout) {
            throw new NotImplementedException ();
        }

        public class Set {
            public Rule Rule;
            public Select Select;
            public List<string> Terminal;
        }
        public class ParsingTable {
            public class Row {
                public Rule Rule;
                public List<Select> Selects;
            }
            public List<string> TableHead;
            public List<Row> TableRows;
        }
        public ParsingTable GetParsingTable(string address) {
            ParsingTable parsingTable = null;
            try {
                int colCount = GetInt("(" + address + ")->ColCount");
                List<string> tableHead = GetTableHead("(" + address + ")->pTableHead", colCount);
                List<ParsingTable.Row> rows = new List<ParsingTable.Row>();
                try {
                    for (int i = 0; i < 32; i ++) {
                        Rule rule = GetRule(GetAddress("(" + address + ")->TableRows[" + i + "].pRule"));
                        if (rule == null) {
                            break;
                        }
                        List<Select> selects = new List<Select>();
                        for (int j = 0; j < colCount; j ++) {
                            selects.Add (GetSelect (GetAddress ("(" + address + ")->TableRows[" + i + "].Select[" + j + "]")));
                        }
                        ParsingTable.Row row = new ParsingTable.Row() { 
                            Rule = rule,
                            Selects = selects
                        };
                        rows.Add (row);
                    }
                } catch (Exception) { }
                parsingTable = new ParsingTable () {
                    TableHead = tableHead,
                    TableRows = rows
                };
            } catch (Exception) { }
            return parsingTable;
        }

        private List<string> GetTableHead (string address, int n) {
            List<string> tableHead = new List<string>();
            for (int i = 0; i < n; i++) {
                tableHead.Add (GetText (address + "[" + i + "]"));
            }
            return tableHead;
        }
        public List<Set> GetSetList(string address) {
            List<Set> setList = new List<Set>();
            try {
                int setCount = GetInt("(" + address + ")->nSetCount");
                for (int i = 0; i < setCount; i++) {
                    int terminalCount = GetInt("(" + address + ")->Sets[" + i + "].nTerminalCount");
                    List<string> terminals = new List<string>();
                    for (int j = 0; j < terminalCount; j++) {
                        terminals.Add (GetText ("(" + address + ")->Sets[" + i + "].Terminal[" + j + "]"));
                    }
                    setList.Add (new Set () {
                        Rule = GetRule ("(" + address + ")->Sets[" + i + "].pRule"),
                        Select = GetSelect ("(" + address + ")->Sets[" + i + "].pSelect"),
                        Terminal = terminals
                    });
                }
            } catch (Exception) { };
            return setList;
        }
    }
}
