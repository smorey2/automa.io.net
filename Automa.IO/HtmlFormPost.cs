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
        /// <param name="s">The s.</param>
        /// <param name="formOptions">The options.</param>
        /// <exception cref="System.InvalidOperationException">unable to find marker</exception>
        /// <exception cref="InvalidOperationException">unable to find marker</exception>
        public HtmlFormPost(string s = null, HtmlFormSettings formOptions = null)
        {
            if (s == null)
                return;
            if (formOptions == null)
                formOptions = HtmlFormSettings.Default;

            // marker
            var markerIdx = 0;
            if (formOptions.Marker != null)
            {
                markerIdx = s.IndexOf(formOptions.Marker);
                if (markerIdx < 0)
                    throw new InvalidOperationException("unable to find marker");
            }

            // strip <form.../form>
            var endIdx = s.IndexOfSkip("</form>", markerIdx);
            s = endIdx == -1 ? s.Substring(markerIdx) : s.Substring(markerIdx, endIdx - markerIdx);
            s = s.Replace("\"", "'");

            // parse action
            var start_formIdx = s.IndexOfSkip("<form");
            if (start_formIdx != -1)
            {
                endIdx = s.IndexOfSkip(">", start_formIdx);
                Action = s.ExtractSpanInner(" action='", "'", start_formIdx, endIdx) ?? s.ExtractSpanInner(" action=", " ", start_formIdx, endIdx);
            }

            // parse fields
            var parseIdx = 0;
            var start_inputIdx = s.IndexOfSkip("<input", parseIdx);
            var start_textareaIdx = s.IndexOfSkip("<textarea", parseIdx);
            var start_selectIdx = s.IndexOfSkip("<select", parseIdx);
            var start_optionIdx = formOptions.ParseOptions ? s.IndexOfSkip("<option", parseIdx) : -1;
            var start_buttonIdx = formOptions.ParseButtons ? s.IndexOfSkip("<button", parseIdx) : -1;
            while (true)
            {
                // minimum
                var startIdx = int.MaxValue;
                if (start_inputIdx != -1 && startIdx > start_inputIdx) startIdx = start_inputIdx;
                if (start_textareaIdx != -1 && startIdx > start_textareaIdx) startIdx = start_textareaIdx;
                if (start_selectIdx != -1 && startIdx > start_selectIdx) startIdx = start_selectIdx;
                if (start_optionIdx != -1 && startIdx > start_optionIdx) startIdx = start_optionIdx;
                if (start_buttonIdx != -1 && startIdx > start_buttonIdx) startIdx = start_buttonIdx;
                if (startIdx == int.MaxValue)
                    break;

                // tag
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
                    // advance
                    start_inputIdx = s.IndexOfSkip("<input", start_inputIdx);
                }
                // textarea element
                else if (startIdx == start_textareaIdx)
                {
                    type = "text";
                    // advance
                    start_textareaIdx = s.IndexOfSkip("<textarea", start_textareaIdx);
                }
                // select element
                else if (startIdx == start_selectIdx)
                {
                    var multipleIdx = s.IndexOf(" multiple", startIdx, StringComparison.OrdinalIgnoreCase); var multiple = multipleIdx != -1 && multipleIdx < endIdx;
                    type = !multiple ? "select" : "selectMultiple";
                    // advance
                    start_selectIdx = s.IndexOfSkip("<select", start_selectIdx);
                }
                // option element
                else if (startIdx == start_optionIdx)
                {
                    type = "option";
                    var checkedIdx = s.IndexOf(" selected", startIdx, StringComparison.OrdinalIgnoreCase); @checked = checkedIdx != -1 && checkedIdx < endIdx;
                    // advance
                    start_optionIdx = formOptions.ParseOptions ? s.IndexOfSkip("<option", start_optionIdx) : -1;
                }
                // button element
                else if (startIdx == start_buttonIdx)
                {
                    type = "button";
                    // advance
                    start_buttonIdx = formOptions.ParseButtons ? s.IndexOfSkip("<button", start_buttonIdx) : -1;
                }
                // unknown element
                else throw new InvalidOperationException("should not reach");

                // add   
                Add(name, type, value, @checked, text);

                // apply disabled
                if (type != "option")
                {
                    var disabledIdx = s.IndexOf(" disabled", startIdx, StringComparison.OrdinalIgnoreCase); var disabled = disabledIdx != -1 && disabledIdx < endIdx;
                    if (disabled)
                        Types[name] = "disabled";
                }
            }
            return;
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
            // set-value
            SetValue(name, value, type, @checked);
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
        /// Sets the value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        /// <param name="checked">if set to <c>true</c> [checked].</param>
        public void SetValue(string name, string value, string type = "text", bool @checked = false)
        {
            Values[name] = value;
            Types[name] = type;
            if (type.StartsWith("checkbox", StringComparison.OrdinalIgnoreCase))
                Checked[name] = @checked;
            if (type.Equals("file", StringComparison.OrdinalIgnoreCase))
                Files[name] = null;
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
        public readonly Dictionary<string, string> Values = new Dictionary<string, string>();

        /// <summary>
        /// Gets the types.
        /// </summary>
        /// <value>The types.</value>
        public readonly Dictionary<string, string> Types = new Dictionary<string, string>();

        /// <summary>
        /// Gets the checkboxs.
        /// </summary>
        /// <value>The checkboxs.</value>
        public readonly Dictionary<string, bool> Checked = new Dictionary<string, bool>();

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        public readonly Dictionary<string, Dictionary<string, string>> Selects = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Gets the buttons.
        /// </summary>
        /// <value>The buttons.</value>
        public readonly Dictionary<string, bool> Buttons = new Dictionary<string, bool>();

        /// <summary>
        /// Gets the files.
        /// </summary>
        /// <value>The files.</value>
        public readonly Dictionary<string, Stream> Files = new Dictionary<string, Stream>();

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
        /// <param name="comparisonType">Type of the comparison.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool FromSelect(string name, string value, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase) =>
            FromSelectByPredicate(name, value, x => x.Value.Equals(value ?? string.Empty, comparisonType));
        /// <summary>
        /// Froms the select.
        /// </summary>
        /// <param name="name">Name of the select.</param>
        /// <param name="value">The value.</param>
        /// <param name="comparisonType">Type of the comparison.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool FromSelectStartsWith(string name, string value, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase) =>
            FromSelectByPredicate(name, value, x => x.Value.StartsWith(value ?? string.Empty, comparisonType));

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