using CPP_EP.Execute;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CPP_EP.Lab
{
    class Lab4 : Lab3
    {

        public Lab4(GDB gdb) : base(gdb) { }

        public override void Draw()
        {
            throw new NotImplementedException();
        }

        public static readonly Regex AddressToAddressToAddress = new Regex(@"(0x[0-9a-f]+)=>(0x[0-9a-f]+)=>(0x[0-9a-f]+)");
        public static Dictionary<string, SelectSet> SelectSetHash = new Dictionary<string, SelectSet>();
        public class ParsingTable
        {
            public class Row
            {
                public Rule Rule;
                public List<Select> Selects;
            }
            public List<string> TableHead;
            public List<Row> TableRows;
            public ParsingTable(string s)
            {
                string[] structs = s.Split(new string[] { "~\"|\"" }, StringSplitOptions.RemoveEmptyEntries);
                TableHead = new List<string>();
                foreach (Match m in Text.Matches(structs[0]))
                {
                    TableHead.Add(m.Groups[1].Value);
                }
                TableRows = new List<Row>();
                for (int i = 1; i < structs.Length; i++)
                {
                    var ms = Text.Matches(structs[i]);
                    List<Select> selects = new List<Select>();
                    for (int j = 1; j < ms.Count; j++)
                    {
                        selects.Add(GetSelect(ms[j].Groups[1].Value));
                    }
                    TableRows.Add(new Row() { Rule = GetRule(ms[0].Groups[1].Value), Selects = selects });
                }
            }

        }
        public ParsingTable GetParsingTable(string address)
        {
            return new ParsingTable(gdb.SendScript("getparsing " + address));
        }
        public List<SelectSet> GetSelectSetList(string address)
        {
            List<SelectSet> selectsetList = new List<SelectSet>();
            string[] ruleStrings = gdb.SendScript("getselectsetlist " + address).Split(new string[] { "~\"|\"" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in ruleStrings)
            {
                selectsetList.Add(new SelectSet(s));
            }
            return selectsetList;
        }
    }
    public class SelectSet
    {
        public Rule Rule;
        public Select Select;
        public string Address;
        public List<string> Terminal;
        public SelectSet(string s)
        {
            Match m = Lab4.AddressToAddressToAddress.Match(s);
            if (m.Success)
            {
                Address = m.Groups[1].Value;
                Rule = AbstractLab.RuleHash[m.Groups[2].Value];
                Select = AbstractLab.SelectHash[m.Groups[3].Value];
                Terminal = new List<string>();
                MatchCollection ms = AbstractLab.Text.Matches(s);
                for (int i = 1; i < ms.Count; i++)
                {
                    Terminal.Add(ms[i].Groups[1].Value);
                }
                Lab4.SelectSetHash[Address] = this;
            }
            else
            {
                throw new Exception("Parsing SelectSet Error: " + s);
            }
        }
    }
}
