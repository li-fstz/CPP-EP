using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using CPP_EP.Execute;
using Xceed.Wpf.AvalonDock.Layout;
namespace CPP_EP.View {
    class RuleView: View {
        protected Dictionary<string, Rule> RuleHash = new Dictionary<string, Rule>();
        protected Dictionary<string, Select> SelectHash = new Dictionary<string, Select>();
        protected Dictionary<string, Symbol> SymbolHash = new Dictionary<string, Symbol>();
        public RuleView (GDB gdb) : base (gdb) { }
        public override void Draw (LayoutAnchorable layout) {
            var rules = GetRule("pHead");
        }
        public StructRuleSymbol GetStructRuleSymbol (string address) {
            if (address == "0x0") {
                return null;
            }
            StructRuleSymbol symbol = null;
            try {
                symbol = new StructRuleSymbol () {
                    Address = address.IndexOf ("0x") == 0 ? address : GetAddress ("&*(RuleSymbol *)" + address),
                    pNextSymbol = GetAddress ("((RuleSymbol *)" + address + ")->pNextSymbol"),
                    pOther = GetAddress ("((RuleSymbol *)" + address + ")->pOther"),
                    SymbolName = GetText ("((RuleSymbol *)" + address + ")->SymbolName"),
                };
            } catch (Exception) { };
            return symbol;
        }
        public StructRule GetStructRule (string address) {
            if (address == "0x0") {
                return null;
            }
            StructRule rule = null;
            try {
                rule = new StructRule () {
                    Address = address.IndexOf("0x") == 0? address: GetAddress ("&*(Rule *)" + address),
                    RuleName = GetText("((Rule*)" + address + ")->RuleName"),
                    pFirstSymbol = GetAddress ("((Rule *)" + address + ")->pFirstSymbol"),
                    pNextRule = GetAddress ("((Rule *)" + address + ")->pNextRule"),
                };
            } catch (Exception) { };
            return rule;
        }
        public Symbol GetSymbol (string address) {
            if (address == "0x0") {
                return null;
            }
            bool zx = address.IndexOf("0x") == 0;
            if (zx && SymbolHash.ContainsKey(address)) {
                return SymbolHash[address];
            }
            StructRuleSymbol pSymbol = GetStructRuleSymbol (address);
            if (!zx && SymbolHash.ContainsKey (pSymbol.Address)) {
                return SymbolHash[pSymbol.Address];
            }
            Symbol symbol = new Symbol (pSymbol);
            SymbolHash.Add (symbol.Address, symbol);
            return symbol;
        }
        public Select GetSelect (string address) {
            if (address == "0x0") {
                return null;
            }
            bool zx = address.IndexOf("0x") == 0;
            if (zx && SelectHash.ContainsKey (address)) {
                return SelectHash[address];
            }
            StructRuleSymbol pSelect = GetStructRuleSymbol(address);
            if (!zx && SelectHash.ContainsKey (pSelect.Address)) {
                return SelectHash[pSelect.Address];
            }
            Select select = new Select(pSelect);
            StructRuleSymbol pSymbol = pSelect;
            while (pSymbol != null) {
                select.Symbols.Add (new Symbol (pSymbol));
                pSymbol = GetStructRuleSymbol (pSymbol.pNextSymbol);
            }
            return select;
        }
        public Rule GetRule(string address) {
            if (address == "0x0") {
                return null;
            }
            StructRule pRule = GetStructRule(address);
            if (RuleHash.ContainsKey (pRule.Address)) {
                return RuleHash[pRule.Address];
            } else {
                Rule rule = new Rule(pRule);
                StructRuleSymbol pSelect = GetStructRuleSymbol(pRule.pFirstSymbol);
                while (pSelect != null) {
                    rule.Selects.Add (GetSelect (pSelect.Address));
                    pSelect = GetStructRuleSymbol (pSelect.pOther);
                }
                return rule;
            }
        }
        public List<Rule> GetRules (string address) {
            StructRule structRule = GetStructRule(address);
            if (structRule == null) {
                return null;
            } else {
                List<Rule> rules = new List<Rule>();
                StructRule pRule = structRule;
                while (pRule != null) {
                    rules.Add (GetRule (pRule.Address));
                    pRule = GetStructRule (pRule.pNextRule);
                }
                return rules;
            }
        }
        public class StructRuleSymbol {
            public string Address;
            public string SymbolName;
            public string pNextSymbol;
            public string pOther;
        }
        public class StructRule {
            public string Address;
            public string RuleName;
            public string pFirstSymbol;
            public string pNextRule;
        }
        public class Symbol {
            public string Name;
            public string Address;
            public Symbol (StructRuleSymbol s) {
                Name = s.SymbolName;
                Address = s.Address;
            }
        }
        public class Select {
            public List<Symbol> Symbols = new List<Symbol>();
            public string Address;
            public Select (StructRuleSymbol s) {
                Address = s.Address;
            }
        }
        public class Rule {
            public string Name;
            public List<Select> Selects = new List<Select>();
            public string Address;
            public Rule (StructRule s) {
                Name = s.RuleName;
                Address = s.Address;
            }
        }
    }
}
