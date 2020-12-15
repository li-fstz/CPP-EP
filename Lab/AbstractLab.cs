using CPP_EP.Execute;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CPP_EP.Lab
{
    abstract class AbstractLab
    {
        protected readonly GDB gdb;
        //protected static readonly Regex Int = new Regex(@"value=""(-?\d+)""");
        //protected static readonly Regex Address = new Regex(@"value=""(0x[0-9a-f]+)""");
        public static readonly Regex Text = new Regex(@"~""(.*?)""");
        //protected static readonly Regex Int = new Regex(@"~""(-?\d*)""");
        public static readonly Regex AddressToSymbolInQuot = new Regex(@"""(0x[0-9a-f]+)=>(.+?)""");
        public static readonly Regex AddressToSymbol = new Regex(@"(0x[0-9a-f]+)=>(.+?)");
        public AbstractLab(GDB gdb)
        {
            this.gdb = gdb;
        }
        public abstract void Draw();


        public static Dictionary<string, Rule> RuleHash = new Dictionary<string, Rule>();
        public static Dictionary<string, Select> SelectHash = new Dictionary<string, Select>();
        public static Dictionary<string, Symbol> SymbolHash = new Dictionary<string, Symbol>();
        public static Rule GetRule(string address)
        {
            if (RuleHash.ContainsKey(address))
            {
                return RuleHash[address];
            }
            else
            {
                return null;
            }
        }
        public static Select GetSelect(string address)
        {
            if (SelectHash.ContainsKey(address))
            {
                return SelectHash[address];
            }
            else
            {
                return null;
            }
        }
        public static Symbol GetSymbol(string address)
        {
            if (SymbolHash.ContainsKey(address))
            {
                return SymbolHash[address];
            }
            else
            {
                return null;
            }
        }
        public List<Rule> GetRules(string address)
        {
            try
            {
                List<Rule> rules = new List<Rule>();
                string[] ruleStrings = gdb.SendScript("getrule " + address).Split(new string[] { "~\"||\"" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in ruleStrings)
                {
                    rules.Add(new Rule(s));
                }
                return rules;
            }
            catch
            {
                throw new Exception("Parsing Rules Error: " + address);
            }

        }

    }
    public class Symbol
    {
        public string Name;
        public string Address;
        public Symbol(string s)
        {
            Match m = AbstractLab.AddressToSymbol.Match(s);
            if (m.Success)
            {
                Address = m.Groups[1].Value;
                Name = m.Groups[2].Value;
                AbstractLab.SymbolHash[Address] = this;
            }
            else
            {
                throw new Exception("Parsing Rule Error: " + s);
            }
        }
    }
    public class Select
    {
        public List<Symbol> Symbols;
        public string Address;
        public Select(string s)
        {
            try
            {
                Symbols = new List<Symbol>();
                foreach (Match m in AbstractLab.Text.Matches(s))
                {
                    Symbols.Add(new Symbol(m.Groups[1].Value));
                }
                Address = Symbols[0].Address;
                AbstractLab.SelectHash[Address] = this;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + "\n" + "Parsing Select Error: " + s);
            }

        }
    }
    public class Rule
    {
        public string Name;
        public List<Select> Selects;
        public string Address;
        public Rule(string s)
        {
            try
            {
                string[] selectStrings = s.Split(new string[] { "~\"|\"" }, StringSplitOptions.RemoveEmptyEntries);
                Match m = AbstractLab.AddressToSymbolInQuot.Match(selectStrings[0]);
                Address = m.Groups[1].Value;
                Name = m.Groups[2].Value;
                Selects = new List<Select>();
                for (int i = 1; i < selectStrings.Length; i++)
                {
                    Selects.Add(new Select(selectStrings[i]));
                }
                AbstractLab.RuleHash[Address] = this;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + "\n" + "Parsing Rule Error: " + s);
            }
        }
    }
}
