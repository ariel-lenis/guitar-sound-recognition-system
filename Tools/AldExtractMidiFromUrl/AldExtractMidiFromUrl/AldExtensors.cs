using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;


namespace AldExtractMidiFromUrl
{
    public static class AldExtensors
    {
        public static string AldJump(this string where, string what)
        {
            int pos = where.IndexOf(what);
            if (pos < 0) return null;
            return where.Substring(pos + what.Length);
        }
        public static string AldTake(this string where, string before)
        {
            int pos = where.IndexOf(before);
            if (pos < 0) return null;
            return where.Substring(0,pos);            
        }
        public static string AldNoAcent(this string where)
        {
            return where.Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u").
                         Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U").
                         Replace("Ñ", "NH").Replace("ñ", "nh").Replace("�","Ñ");
        }

    }
}
