using CPP_EP.Execute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace CPP_EP.Lab
{
    class Lab1 : AbstractLab
    {
        public Lab1(GDB gdb) : base(gdb) { }

        public override void Draw()
        {
        }

        public VoidTable GetVoidTable(string address)
        {
            return new VoidTable(gdb.SendScript("getvoidtable " + address));
        }

    }
    public class VoidTable
    {
        public List<string> TableHead;
        public List<bool?> HasVoid;
        public VoidTable(string s)
        {
            if (s == null)
            {
                throw new Exception("Parsing VoidTable Error: " + s);
            }
            string[] structs = s.Split(new string[] { "~\"|\"" }, StringSplitOptions.RemoveEmptyEntries);
            TableHead = new List<string>();
            foreach (Match m in AbstractLab.Text.Matches(structs[0]))
            {
                TableHead.Add(m.Groups[1].Value);
            }
            HasVoid = new List<bool?>();
            foreach (Match m in AbstractLab.Text.Matches(structs[1]))
            {
                if (m.Groups[1].Value == "1")
                {
                    HasVoid.Add(true);
                }
                else if (m.Groups[1].Value == "0")
                {
                    HasVoid.Add(false);
                }
                else
                {
                    HasVoid.Add(null);
                }
            }
        }
    }
}
