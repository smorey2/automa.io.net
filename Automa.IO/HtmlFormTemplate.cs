using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Automa.IO
{
    /// <summary>
    /// HtmlFormTemplate
    /// </summary>
    public class HtmlFormTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlFormTemplate"/> class.
        /// </summary>
        public HtmlFormTemplate() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlFormTemplate"/> class.
        /// </summary>
        /// <param name="post">The post.</param>
        public HtmlFormTemplate(HtmlFormPost post)
        {
            Action = post.Action;
            Values = post.Values.ToDictionary(x => x.Key, x => x.Value);
            Types = post.Types.ToDictionary(x => x.Key, x => x.Value);
            Checked = post.Checked.ToDictionary(x => x.Key, x => x.Value);
            Selects = post.Selects.ToDictionary(x => x.Key, x => x.Value);
            Buttons = post.Buttons.ToDictionary(x => x.Key, x => x.Value);
            Files = post.Files.ToDictionary(x => x.Key, x => x.Value);
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
    }
}