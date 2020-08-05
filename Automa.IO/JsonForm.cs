using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Automa.IO
{
    /// <summary>
    /// JsonForm
    /// </summary>
    public class JsonForm
    {
        public JsonForm(string s = null, string marker = null)
        {
            Values = new Dictionary<string, string>();
            if (s == null)
                return;
            // advance
            var markerIdx = 0;
            if (marker != null)
            {
                markerIdx = s.IndexOf(marker);
                if (markerIdx < 0)
                    throw new InvalidOperationException("unable to find marker");
            }
            // parse
            var endIdx = s.IndexOfSkip("}", markerIdx);
            s = s.Substring(markerIdx, endIdx - markerIdx);
            var r = JObject.Parse(s);
            foreach (var v in r)
                Values.Add(v.Key, (string)v.Value);
            return;
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public readonly Dictionary<string, string> Values = new Dictionary<string, string>();
    }
}