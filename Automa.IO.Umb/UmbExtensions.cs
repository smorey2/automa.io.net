using System;

namespace Automa.IO.Umb
{
    public static class UmbExtensions
    {
        public static DateTime? ToDateTime(this string s, string nullif = null)
        {
            if (s == nullif && nullif != null) return null;
            return DateTime.TryParse(s, out var value) ? (DateTime?)value : null;
        }
        public static string FromDateTime(this DateTime? s, string ifnull = null)
        {
            if (s == null && ifnull != null) return ifnull;
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
            return decimal.TryParse(s.Replace("$", ""), out var value) ? (decimal?)value : null;
        }
    }
}
