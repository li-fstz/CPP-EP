using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CPP_EP.Lab.Data {

    public abstract class GDBData {
        private readonly int HashCode;
        public readonly string Address;
        private static readonly Dictionary<string, GDBData> MemeryHash = new();

        //protected static readonly Regex Int = new Regex(@"value=""(-?\d+)""");
        //protected static readonly Regex Address = new Regex(@"value=""(0x[0-9a-f]+)""");
        public static readonly Regex Text = new(@"~""(.*?)""");

        //protected static readonly Regex Int = new Regex(@"~""(-?\d*)""");
        public static readonly Regex AddressToSymbolInQuot = new(@"""(0x[0-9a-f]+)=>(.+?)""");

        public static readonly Regex AddressToSymbol = new(@"(0x[0-9a-f]+)=>(.+)");

        protected GDBData (string address, string baseString) {
            Address = address;
            HashCode = baseString.GetHashCode ();
            MemeryHash[address] = this;
        }

        public override int GetHashCode () {
            return HashCode;
        }

        public override bool Equals (object obj) {
            return obj is GDBData d && d.HashCode == HashCode;
        }

        public static T Get<T> (string address) where T : GDBData {
            if (address is string && MemeryHash.ContainsKey (address)) {
                return MemeryHash[address] as T;
            } else {
                return null;
            }
        }
    }
}