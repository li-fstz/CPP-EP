using CPP_EP.Execute;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace CPP_EP.Lab {
    abstract class AbstractLab {
        public abstract List<string> LabFiles { get; }
        public abstract int LabNo { get; }
        protected GDB gdb;
        public static Action<int, Action<TextBlock>> UpdateUI { protected get; set; }
        //protected static readonly Regex Int = new Regex(@"value=""(-?\d+)""");
        //protected static readonly Regex Address = new Regex(@"value=""(0x[0-9a-f]+)""");
        public static readonly Regex Text = new Regex (@"~""(.*?)""");
        //protected static readonly Regex Int = new Regex(@"~""(-?\d*)""");
        public static readonly Regex AddressToSymbolInQuot = new Regex (@"""(0x[0-9a-f]+)=>(.+?)""");
        public static readonly Regex AddressToSymbol = new Regex (@"(0x[0-9a-f]+)=>(.+)");
        public abstract void Draw ();
        public abstract void Build ();

        public static Dictionary<string, Rule> RuleHash = new Dictionary<string, Rule> ();
        public static Dictionary<string, Production> ProductionHash = new Dictionary<string, Production> ();
        public static Dictionary<string, Symbol> SymbolHash = new Dictionary<string, Symbol> ();
        public static Dictionary<string, object> DataHash = new Dictionary<string, object> ();
        public static T Get<T> (Dictionary<string, T> Hash, string address) {
            if (Hash.ContainsKey(address)) {
                return Hash[address];
            } else {
                return default;
            }
        }
        public GDB GetGDB () {
            gdb = new GDB (
                "build\\lab" + LabNo + ".exe"
            );
            return gdb;
        }
        public void GetRules (string address, Action<List<Rule>> AfterGetRules) {
            try {
                gdb.SendScript ("getrule " + address, r => {
                    List<Rule> rules = new List<Rule> ();
                    string[] ruleStrings = r.Split (new string[] { "~\"|rule|\"" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var s in ruleStrings) {
                        var rule = Rule.GenRule (s);
                        if (rule != null) rules.Add (rule);
                    }
                    AfterGetRules (rules);
                });
            } catch {
                throw new Exception ("Parsing Rules Error: " + address);
            }
        }
        public static AbstractLab GetLab(int index) {
            return index switch {
                1 => new Lab1 (),
                2 => new Lab2 (),
                3 => new Lab3 (),
                4 => new Lab4 (),
                5 => new Lab5 (),
                6 => new Lab6 (),
                7 => new Lab7 (),
                8 => new Lab8 (),
                _ => null,
            };
        }

        public class Symbol {
            public string Name;
            public string Address;
            private Symbol () {
            }
            public static Symbol GenSymbol (string s) {
                Match m = AddressToSymbol.Match (s);
                Symbol syb;
                if (m.Success) {
                    syb = new Symbol () {
                        Address = m.Groups[1].Value,
                        Name = m.Groups[2].Value,
                    };
                    if (syb.Equals (Get (SymbolHash, syb.Address))) {
                        syb = Get (SymbolHash, syb.Address);
                    } else {
                        SymbolHash[syb.Address] = syb;
                    }
                } else {
                    throw new Exception ("Parsing Rule Error: " + s);
                }
                return syb;
            }
            public override bool Equals (object obj) {
                Symbol s = obj as Symbol;
                return s == this || (s != null && s.Address == Address && s.Name == Name);
            }
        }
        public class Production {
            public List<Symbol> Symbols;
            public string Address;
            private Production () { }
            public static Production GenProduction (string s) {
                Production p;
                try {
                    p = new Production () {
                        Symbols = new List<Symbol> (),
                    };
                    foreach (Match m in Text.Matches (s)) {
                        p.Symbols.Add (Symbol.GenSymbol (m.Groups[1].Value));
                    }
                    p.Address = p.Symbols[0].Address;
                    if (p.Equals (Get (ProductionHash, p.Address))) {
                        p = Get (ProductionHash, p.Address);
                    } else {
                        ProductionHash[p.Address] = p;
                    }
                } catch (Exception e) {
                    throw new Exception (e.Message + "\n" + "Parsing Production Error: " + s);
                }
                return p;
            }
            public override bool Equals (object obj) {
                Production p = obj as Production;
                return p == this || (p != null && p.Address == Address && p.Symbols.SequenceEqual (Symbols));
            }
        }
        public class Rule {
            public string Name;
            public List<Production> Productions;
            public string Address;
            public override bool Equals (object obj) {
                Rule r = obj as Rule;
                return r == this || (r != null && r.Address == Address && r.Name == Name && r.Productions.SequenceEqual (Productions));
            }
            private Rule () { }
            public static Rule GenRule (string s) {
                Rule r = null;
                try {
                    string[] ProductionStrings = s.Split (new string[] { "~\"|production|\"" }, StringSplitOptions.RemoveEmptyEntries);
                    Match m = AddressToSymbolInQuot.Match (ProductionStrings[0]);
                    if (m.Success) {
                        r = new Rule () {
                            Address = m.Groups[1].Value,
                            Name = m.Groups[2].Value,
                            Productions = new List<Production> (),
                        };
                        for (int i = 1; i < ProductionStrings.Length; i++) {
                            r.Productions.Add (Production.GenProduction (ProductionStrings[i]));
                        }
                        if (r.Equals (Get (RuleHash, r.Address))) {
                            r = Get (RuleHash, r.Address);
                        } else {
                            RuleHash[r.Address] = r;
                        }
                    }
                } catch (Exception) {

                }
                return r;
            }
        }
    }
}
