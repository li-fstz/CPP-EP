using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace CPP_EP {

    public static class Util {
        private static List<(string, string)> GDBCodeMap = new() {
            (@"\\241\\260", "“"),
            (@"\\241\\261", "”"),
            (@"\\306\\245", "匹"),
            (@"\\305\\344", "配"),
            (@"\\262\\273", "不"),
            (@"\\275\\323", "接"),
            (@"\\312\\334", "受"),
        };

        public class RegexGroupOneException: Exception { };

        public static string RegexGroupOne (Regex r, string str) {
            Match m = r.Match (str);
            if (m.Success) {
                return m.Groups[1].Value;
            } else {
                throw new RegexGroupOneException ();
            }
        }

        public static void ThreadRun (ThreadStart a) {
            //a.Invoke ();
            new Thread (a).Start ();
        }

        public static string DecodeGDBsGBK (string s) {
            foreach(var (key, value) in GDBCodeMap) {
                s = s.Replace(key, value);
            }
            return s;
        }
    }
}