using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace CPP_EP {

    public static class Util {

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
            new Thread (a).Start ();
        }
    }
}