﻿using System.Text.RegularExpressions;

namespace CPP_EP.Lab.Data {

    public class Symbol: GDBData {
        public string Name;

        private Symbol (string a, string s) : base (a, s) {
        }

        public static Symbol Gen (string s) {
            Symbol syb = null;
            Match m = AddressToSymbol.Match (s);
            if (m.Success) {
                string address = m.Groups[1].Value;
                Symbol h = Get<Symbol> (address, s);
                if (h is Symbol) {
                    syb = h;
                } else {
                    syb = new Symbol (address, s) {
                        Name = m.Groups[2].Value
                    };
                }
            }
            return syb;
        }
    }
}