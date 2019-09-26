using System;

namespace Automa.IO.Unanet
{
    public static class UnanetExtensions
    {
        public static string DecodeString(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.Contains("&quote;") || s.Contains("&amp;"))
            {
                Console.WriteLine($"{s}");
                throw new InvalidOperationException("Should not get here");
            }
            s = s.Trim();
            if (s.StartsWith("\"") && s.EndsWith("\""))
            {
                s = s.Substring(1, s.Length - 2);
                if (s.Contains("\"\""))
                    s = s.Replace("\"\"", "\"");
            }
            return s;
        }

        public static DateTime? ToDateTime(this string s, string nullif = null)
        {
            if (s == nullif && nullif != null) return null;
            if (s == "BOT") return DateTime.MinValue;
            else if (s == "EOT") return DateTime.MaxValue;
            return DateTime.TryParse(s, out var value) ? (DateTime?)value : null;
        }
        public static string FromDateTime(this DateTime? s, string ifnull = null)
        {
            if (s == null && ifnull != null) return ifnull;
            else if (s == DateTime.MinValue) return "BOT";
            else if (s == DateTime.MaxValue) return "EOT";
            return s?.ToString("M/d/yyyy");
        }

        public static int? ToInt(this string d, string nullif = null)
        {
            if (d == nullif && nullif != null) return null;
            return int.TryParse(d, out var value) ? (int?)value : null;
        }

        public static decimal? ToDecimal(this string s, string nullif = null)
        {
            if (s == nullif && nullif != null) return null;
            return decimal.TryParse(s, out var value) ? (decimal?)value : null;
        }

        public static string ToProjectToOrg(this string s, string nullif = null)
        {
            if (s == nullif && nullif != null) return null;
            return s.Split(new[] { '-' }, 2)[0];
        }
    }
}
