using CPP_EP.Execute;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CPP_EP.Lab {
    abstract class AbstractLab {
        public abstract List<string> LabFiles { get; }
        public abstract int LabNo { get; }
        protected GDB gdb;
        //protected static readonly Regex Int = new Regex(@"value=""(-?\d+)""");
        //protected static readonly Regex Address = new Regex(@"value=""(0x[0-9a-f]+)""");
        public static readonly Regex Text = new Regex (@"~""(.*?)""");
        //protected static readonly Regex Int = new Regex(@"~""(-?\d*)""");
        public static readonly Regex AddressToSymbolInQuot = new Regex (@"""(0x[0-9a-f]+)=>(.+?)""");
        public static readonly Regex AddressToSymbol = new Regex (@"(0x[0-9a-f]+)=>(.+?)");
        public abstract void Draw ();
        public abstract void Build ();

        public static Dictionary<string, Rule> RuleHash = new Dictionary<string, Rule> ();
        public static Dictionary<string, Production> ProductionHash = new Dictionary<string, Production> ();
        public static Dictionary<string, Symbol> SymbolHash = new Dictionary<string, Symbol> ();

        public GDB GetGDB () {
            gdb = new GDB (
                "C:\\MinGW\\bin\\gdb.exe",
                "build\\lab" + LabNo + ".exe"
            );
            return gdb;
        }
        public static Rule GetRule (string address) {
            if (RuleHash.ContainsKey (address)) {
                return RuleHash[address];
            } else {
                return null;
            }
        }
        public static Production GetProduction (string address) {
            if (ProductionHash.ContainsKey (address)) {
                return ProductionHash[address];
            } else {
                return null;
            }
        }
        public static Symbol GetSymbol (string address) {
            if (SymbolHash.ContainsKey (address)) {
                return SymbolHash[address];
            } else {
                return null;
            }
        }
        public void GetRules (string address, Action<List<Rule>> AfterGetRules) {
            try {
                gdb.SendScript ("getrule " + address, r => {
                    List<Rule> rules = new List<Rule> ();
                    string[] ruleStrings = r.Split (new string[] { "~\"||\"" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var s in ruleStrings) {
                        rules.Add (new Rule (s));
                    }
                    AfterGetRules (rules);
                });
            } catch {
                throw new Exception ("Parsing Rules Error: " + address);
            }
        }
        public static AbstractLab GetLab(int index) {
            switch (index) {
                case 1:
                    return new Lab1 ();
                case 2:
                    return new Lab2 ();
                case 3:
                    return new Lab3 ();
                case 4:
                    return new Lab4 ();
                case 5:
                    return new Lab5 ();
                case 6:
                    return new Lab6 ();
                case 7:
                    return new Lab7 ();
                case 8:
                    return new Lab8 ();
                default:
                    return null;
            }
        }

    }
    public class Symbol {
        public string Name;
        public string Address;
        public Symbol (string s) {
            Match m = AbstractLab.AddressToSymbol.Match (s);
            if (m.Success) {
                Address = m.Groups[1].Value;
                Name = m.Groups[2].Value;
                AbstractLab.SymbolHash[Address] = this;
            } else {
                throw new Exception ("Parsing Rule Error: " + s);
            }
        }
    }
    public class Production {
        public List<Symbol> Symbols;
        public string Address;
        public Production (string s) {
            try {
                Symbols = new List<Symbol> ();
                foreach (Match m in AbstractLab.Text.Matches (s)) {
                    Symbols.Add (new Symbol (m.Groups[1].Value));
                }
                Address = Symbols[0].Address;
                AbstractLab.ProductionHash[Address] = this;
            } catch (Exception e) {
                throw new Exception (e.Message + "\n" + "Parsing Production Error: " + s);
            }

        }
    }
    public class Rule {
        public string Name;
        public List<Production> Productions;
        public string Address;
        public Rule (string s) {
            try {
                string[] ProductionStrings = s.Split (new string[] { "~\"|\"" }, StringSplitOptions.RemoveEmptyEntries);
                Match m = AbstractLab.AddressToSymbolInQuot.Match (ProductionStrings[0]);
                Address = m.Groups[1].Value;
                Name = m.Groups[2].Value;
                Productions = new List<Production> ();
                for (int i = 1; i < ProductionStrings.Length; i++) {
                    Productions.Add (new Production (ProductionStrings[i]));
                }
                AbstractLab.RuleHash[Address] = this;
            } catch (Exception e) {
                throw new Exception (e.Message + "\n" + "Parsing Rule Error: " + s);
            }
        }
    }
}
