﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Automa.IO.Unanet.Records
{
    public class ChangedFields : Dictionary<string, (Type t, object v)>
    {
        readonly ManageFlags _changeFlag;
        public ChangedFields(ManageFlags changeFlag) => _changeFlag = changeFlag;
        public ChangedFields Changed() { Flags = _changeFlag; return this; }
        public ChangedFields Changed(object r) { if (r != null) Flags = _changeFlag; return this; }
        public ManageFlags Flags { get; set; } = ManageFlags.None;
        public T _<T>(T value, string name) { this[name] = (typeof(T), value); return value; }
    }

    public class ModelBase
    {
        protected readonly static DateTime BeginDate = new DateTime(2019, 01, 01);
        protected readonly static DateTime BeginTimeWindowDate = new DateTime(2020, 01, 01); // new DateTime(2019, 09, 30);
        protected readonly static DateTime BeginInvoiceWindowDate = new DateTime(2020, 01, 01); // new DateTime(2019, 01, 01);

        //protected static string XEscape(string value)
        //{
        //    for (var x = 0; x < 5; x++)
        //    {
        //        var last = value;
        //        value = HttpUtility.UrlDecode(value);
        //        if (value == last)
        //            return value;
        //    }
        //    throw new InvalidOperationException("XEscape");
        //}
        protected static XAttribute XAttribute(string name, string value) => !string.IsNullOrEmpty(value) ? new XAttribute(name, value) : null;
        protected static XAttribute XAttribute<T>(string name, T? value) where T : struct => value != null ? new XAttribute(name, value.Value) : null;

        protected static bool ManageRecordBase(string key, string xcf, int mode, out HashSet<string> cf, out bool add, out string last, bool canDelete = false)
        {
            cf = new HashSet<string>((xcf ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            add = mode == 1 ? cf.Contains("insert") : key == "-1";
            if (!add && cf.Count == 0) { last = "no-changes"; return true; }
            else if (cf.Contains("INFO")) { last = $"INFO: {xcf}"; return true; }
            else if (!add && cf.Contains("insert")) { last = $"manual: {xcf}"; return true; }
            else if (!canDelete && cf.Contains("delete")) { last = $"not implemented: {xcf}"; return true; }
            last = null; return false;
        }

        protected static void GetWindowDates(string entity, int window, out DateTime? beginDate, out DateTime? endDate)
        {
            DateTime beginWindowDate;
            switch (entity)
            {
                case "Begin": beginWindowDate = BeginDate; break;
                case nameof(TimeModel): beginWindowDate = BeginTimeWindowDate; break;
                case "Assist": beginWindowDate = new DateTime(2020, 01, 01); break;
                case nameof(InvoiceModel): beginWindowDate = BeginInvoiceWindowDate; break;
                default: beginWindowDate = BeginDate; break;
            }
            switch (window)
            {
                case -1:
                    beginDate = beginWindowDate;
                    endDate = null;
                    return;
                case 0:
                    beginDate = new DateTime(Math.Max(DateTime.Today.Ticks, beginWindowDate.Ticks));
                    endDate = DateTime.Today;
                    return;
                case 3: // 3 full months (3 months + this month)
                    var startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-3);
                    beginDate = new DateTime(Math.Max(startDate.Ticks, beginWindowDate.Ticks));
                    endDate = DateTime.Today;
                    return;
                default: throw new ArgumentOutOfRangeException(nameof(window), window.ToString());
            }
        }

        protected static string GetLookupValue(Dictionary<string, string> source, string value, bool missingThrows = false)
        {
            if (missingThrows && value != null && !source.ContainsKey(value))
                throw new ArgumentOutOfRangeException(nameof(value), value);
            return value != null && source.TryGetValue(value, out var key) ? key : "-1";
        }
    }
}