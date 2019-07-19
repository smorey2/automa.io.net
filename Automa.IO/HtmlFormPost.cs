using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;

namespace Automa.IO
{
    /// <summary>
    /// HtmlFormPost
    /// </summary>
    public class HtmlFormPost
    {
        /// <summary>
        /// Enum Mode
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// The form
            /// </summary>
            Form,
            /// <summary>
            /// The json
            /// </summary>
            Json,
        }

        /// <summary>
        /// Enum Merge
        /// </summary>
        public enum Merge
        {
            /// <summary>
            /// Replace
            /// </summary>
            Replace,
            /// <summary>
            /// Include
            /// </summary>
            Include,
            /// <summary>
            /// Exclude
            /// </summary>
            Exclude,
        }

        string _selectName;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlFormPost" /> class.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <param name="s">The s.</param>
        /// <param name="marker">The marker.</param>
        /// <exception cref="InvalidOperationException">unable to find marker</exception>
        public HtmlFormPost(Mode mode, string s, string marker = null)
        {
            Values = new Dictionary<string, string>();
            Types = new Dictionary<string, string>();
            Checked = new Dictionary<string, bool>();
            Selects = new Dictionary<string, Dictionary<string, string>>();
            Files = new Dictionary<string, Stream>();
            // advance
            var markerIdx = 0;
            if (marker != null)
            {
                markerIdx = s.IndexOf(marker);
                if (markerIdx < 0)
                    throw new InvalidOperationException("unable to find marker");
            }
            //
            switch (mode)
            {
                case Mode.Json:
                    {
                        var endIdx = s.IndexOfSkip("}", markerIdx);
                        s = s.Substring(markerIdx, endIdx - markerIdx);
                        var r = JObject.Parse(s);
                        foreach (var v in r)
                            Values.Add(v.Key, (string)v.Value);
                        return;
                    }
                case Mode.Form:
                    {
                        var endIdx = s.IndexOfSkip("</form>", markerIdx);
                        s = endIdx == -1 ? s.Substring(markerIdx) : s.Substring(markerIdx, endIdx - markerIdx);
                        s = s.Replace("\"", "'");
                        // parse form element
                        var start_formIdx = s.IndexOfSkip("<form");
                        if (start_formIdx != -1)
                        {
                            endIdx = s.IndexOfSkip(">", start_formIdx);
                            Action = s.ExtractSpanInner(" action='", "'", start_formIdx, endIdx) ?? s.ExtractSpanInner(" action=", " ", start_formIdx, endIdx);
                        }
                        // parse
                        while (true)
                        {
                            var start_inputIdx = s.IndexOfSkip("<input");
                            var start_textareaIdx = s.IndexOfSkip("<textarea");
                            var start_selectIdx = s.IndexOfSkip("<select");
                            var start_optionIdx = s.IndexOfSkip("<option");
                            var start_buttonIdx = s.IndexOfSkip("<button");
                            var startIdx = new[] {
                                start_inputIdx != -1 ? start_inputIdx : int.MaxValue,
                                start_textareaIdx != -1 ? start_textareaIdx : int.MaxValue,
                                start_selectIdx != -1 ? start_selectIdx : int.MaxValue,
                                start_optionIdx != -1 ? start_optionIdx : int.MaxValue,
                                start_buttonIdx != -1 ? start_buttonIdx : int.MaxValue }.Min();
                            if (startIdx == int.MaxValue)
                                break;
                            endIdx = s.IndexOfSkip(">", startIdx);
                            var endIdx2 = s.IndexOfSkip("<", endIdx);

                            // common attributes
                            string type;
                            var @checked = false;
                            var name = HttpUtility.HtmlDecode(s.ExtractSpanInner(" name='", "'", startIdx, endIdx) ?? s.ExtractSpanInner(" name=", " ", startIdx, endIdx));
                            var value = HttpUtility.HtmlDecode(s.ExtractSpanInner(" value='", "'", startIdx, endIdx) ?? s.ExtractSpanInner(" value=", " ", startIdx, endIdx));
                            var text = endIdx2 != -1 ? HttpUtility.HtmlDecode(s.Substring(endIdx, endIdx2 - endIdx - 1).Trim()) : null;

                            // input element
                            if (startIdx == start_inputIdx)
                            {
                                type = HttpUtility.HtmlDecode(s.ExtractSpanInner(" type='", "'", startIdx, endIdx) ?? s.ExtractSpanInner(" type=", " ", startIdx, endIdx));
                                var checkedIdx = s.IndexOf(" checked", startIdx, StringComparison.OrdinalIgnoreCase); @checked = checkedIdx != -1 && checkedIdx < endIdx;
                                var multipleIdx = s.IndexOf(" multiple", startIdx, StringComparison.OrdinalIgnoreCase); var multiple = multipleIdx != -1 && multipleIdx < endIdx;
                                type = !multiple ? type : type + "Multiple";
                            }
                            // input element
                            else if (startIdx == start_textareaIdx)
                                type = "text";
                            // select element
                            else if (startIdx == start_selectIdx)
                            {
                                var multipleIdx = s.IndexOf(" multiple", startIdx, StringComparison.OrdinalIgnoreCase); var multiple = multipleIdx != -1 && multipleIdx < endIdx;
                                type = !multiple ? "select" : "selectMultiple";
                            }
                            // option element
                            else if (startIdx == start_optionIdx)
                            {
                                type = "option";
                                var checkedIdx = s.IndexOf(" selected", startIdx, StringComparison.OrdinalIgnoreCase); @checked = checkedIdx != -1 && checkedIdx < endIdx;
                            }
                            // button element
                            else if (startIdx == start_buttonIdx)
                                type = "button";
                            // unknown element
                            else
                                type = "unknown";

                            // add   
                            Add(name, type, value, @checked, text);
                            // apply disabled
                            if (type != "option")
                            {
                                var disabledIdx = s.IndexOf(" disabled", startIdx, StringComparison.OrdinalIgnoreCase); var disabled = disabledIdx != -1 && disabledIdx < endIdx;
                                if (disabled)
                                    Types[name] = "disabled";
                            }
                            // advance
                            s = s.Substring(endIdx);
                        }
                        return;
                    }
            }
        }

        /// <summary>
        /// Adds the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <param name="checked">if set to <c>true</c> [checked].</param>
        /// <param name="text">The text.</param>
        public void Add(string name, string type, string value, bool @checked = false, string text = null)
        {
            // selectName
            if (type == "radio" || type.StartsWith("select"))
                _selectName = name;
            // process option
            if (type == "radio" || type == "option")
            {
                if (!Selects.TryGetValue(_selectName, out var select))
                {
                    Selects[_selectName] = select = new Dictionary<string, string>();
                    if (value == null)
                        @checked = true;
                }
                select[value ?? text ?? string.Empty] = text;
                if (@checked)
                {
                    if (!Types.ContainsKey(_selectName))
                        Types.Add(_selectName, "radio");
                    Values[_selectName] = value ?? text;
                }
                if (type == "option")
                    return;
            }
            // skip if button
            else if (type == "button")
                return;
            // skip if name blank
            else if (name == null)
                return;
            // multi-key
            if (Values.ContainsKey(name))
            {
                if (type == "radio")
                    return;
                for (var i = 1; i < int.MaxValue; i++)
                {
                    var newName = $"{name}>{i}";
                    if (!Values.ContainsKey(newName))
                    {
                        name = newName;
                        break;
                    }
                }
            }
            // add
            Values.Add(name, value);
            Types.Add(name, type);
            if (type.StartsWith("checkbox", StringComparison.OrdinalIgnoreCase))
                Checked.Add(name, @checked);
            if (type.Equals("file", StringComparison.OrdinalIgnoreCase))
                Files.Add(name, null);
        }

        /// <summary>
        /// Removes the specified name.
        /// </summary>
        /// <param name="names">The names.</param>
        public void Remove(params string[] names)
        {
            foreach (var name in names)
            {
                if (name == null || !Values.ContainsKey(name))
                    continue;
                Values.Remove(name);
                Types.Remove(name);
                if (Checked.ContainsKey(name))
                    Checked.Remove(name);
            }
        }

        /// <summary>
        /// Gets the action.
        /// </summary>
        /// <value>The action.</value>
        public string Action { get; set; }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public Dictionary<string, string> Values { get; private set; }

        /// <summary>
        /// Gets the types.
        /// </summary>
        /// <value>The types.</value>
        public Dictionary<string, string> Types { get; private set; }

        /// <summary>
        /// Gets the checkboxs.
        /// </summary>
        /// <value>The checkboxs.</value>
        public Dictionary<string, bool> Checked { get; private set; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        public Dictionary<string, Dictionary<string, string>> Selects { get; private set; }

        /// <summary>
        /// Gets the buttons.
        /// </summary>
        /// <value>The buttons.</value>
        public Dictionary<string, bool> Buttons { get; private set; }

        /// <summary>
        /// Gets the files.
        /// </summary>
        /// <value>The files.</value>
        public Dictionary<string, Stream> Files { get; private set; }

        /// <summary>
        /// Froms the select by predicate.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="pred">The pred.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool FromSelectByPredicate(string name, string value, Func<KeyValuePair<string, string>, bool> pred)
        {
            if (!Selects.TryGetValue(name, out var select))
                return false;
            Values[name] = select.FirstOrDefault(pred).Key;
            return true;
        }
        /// <summary>
        /// Froms the select by key.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="key">The key.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool FromSelectByKey(string name, string key, bool ignoreCase = true) =>
            FromSelectByPredicate(name, key, x => string.Compare(x.Key, key, ignoreCase) == 0);
        /// <summary>
        /// Froms the select.
        /// </summary>
        /// <param name="name">Name of the select.</param>
        /// <param name="value">The value.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool FromSelect(string name, string value, bool ignoreCase = true) =>
            FromSelectByPredicate(name, value, x => string.Compare(x.Value, value, ignoreCase) == 0);

        /// <summary>
        /// Froms the multi checkbox.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="values">The values.</param>
        /// <param name="ignoreCase">The ignore case.</param>
        /// <param name="merge">The merge.</param>
        /// <returns>System.Boolean.</returns>
        public bool FromMultiCheckbox(string name, string values, bool ignoreCase = true, Merge merge = Merge.Replace) => FromMultiCheckbox(name, values?.Split(','), ignoreCase, merge);
        /// <summary>
        /// Froms the multi checkbox.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="values">The values.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <param name="merge">The merge.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool FromMultiCheckbox(string name, IEnumerable<string> values, bool ignoreCase = true, Merge merge = Merge.Replace)
        {
            var set = values != null ? new HashSet<string>(values) : new HashSet<string>();
            foreach (var v in Checked.ToList())
            {
                // multi-key
                var key = v.Key;
                var keyIdx = key.IndexOf(">");
                if (keyIdx > 0)
                    key = key.Substring(0, keyIdx);
                // check
                if (string.Compare(key, name, ignoreCase) == 0)
                    switch (merge)
                    {
                        case Merge.Replace: Checked[v.Key] = set.Contains(Values[v.Key]); break;
                        case Merge.Include: if (set.Contains(Values[v.Key])) Checked[v.Key] = true; break;
                        case Merge.Exclude: if (set.Contains(Values[v.Key])) Checked[v.Key] = false; break;
                    }
            }
            return true;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            var b = new StringBuilder();
            foreach (var v in Values)
            {
                var hasValue = !string.IsNullOrEmpty(v.Value);
                var skipValue = !Types.TryGetValue(v.Key, out var type) || type == "unknown" || type == "disabled" ? true :
                    (type.StartsWith("checkbox") && !Checked[v.Key]) ||
                    (type == "selectMultiple" && !hasValue);
                // multi-key
                var key = v.Key;
                var keyIdx = key.IndexOf(">");
                if (keyIdx > 0)
                    key = key.Substring(0, keyIdx);
                // render
                if (skipValue) continue;
                else if (hasValue) b.Append($"{key}={HttpUtility.UrlEncode(v.Value)}&");
                else b.Append($"{key}=&");
            }
            if (b.Length > 0)
                b.Length--;
            return b.ToString();
        }

        /// <summary>
        /// To the key value pair.
        /// </summary>
        /// <returns>HttpContent.</returns>
        public IEnumerable<KeyValuePair<string, string>> ToKeyValuePairs()
        {
            var b = new List<KeyValuePair<string, string>>();
            foreach (var v in Values)
            {
                var hasValue = !string.IsNullOrEmpty(v.Value);
                var skipValue = !Types.TryGetValue(v.Key, out var type) || type == "unknown" || type == "disabled" ? true :
                    (type.StartsWith("checkbox") && !Checked[v.Key]) ||
                    (type == "selectMultiple" && !hasValue);
                // multi-key
                var key = v.Key;
                var keyIdx = key.IndexOf(">");
                if (keyIdx > 0)
                    key = key.Substring(0, keyIdx);
                // render
                if (skipValue) continue;
                else if (hasValue) b.Add(new KeyValuePair<string, string>(key, v.Value));
                else b.Add(new KeyValuePair<string, string>(key, null));
            }
            return b;
        }

        /// <summary>
        /// To the content.
        /// </summary>
        /// <returns>HttpContent.</returns>
        public HttpContent ToContent()
        {
            if (Files.Values.All(x => x == null))
                return new FormUrlEncodedContent(ToKeyValuePairs());
            var multi = new MultipartFormDataContent();
            foreach (var keyValuePair in ToKeyValuePairs())
                multi.Add(new StringContent(keyValuePair.Value), QuoteValue(keyValuePair.Key));
            foreach (var file in Files)
                multi.Add(new StreamContent(file.Value), QuoteValue(file.Key), Values.TryGetValue(file.Key, out var filename) && !string.IsNullOrEmpty(filename) ? QuoteValue(Path.GetFileName(filename)) : null);
            return multi;
        }

        static string QuoteValue(string value) => $"\"{value}\"";
    }
}