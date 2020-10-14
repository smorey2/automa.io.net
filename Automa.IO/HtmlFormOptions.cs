namespace Automa.IO
{
    /// <summary>
    /// HtmlFormSettings
    /// </summary>
    public class HtmlFormOptions
    {
        public readonly static HtmlFormOptions Default = new HtmlFormOptions();

        /// <summary>
        /// Gets or sets the marker.
        /// </summary>
        /// <value>The marker.</value>
        public string Marker { get; set; }

        /// <summary>
        /// Gets or sets the form template.
        /// </summary>
        /// <value>
        /// The form template.
        /// </value>
        public HtmlFormTemplate FormTemplate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [parse buttons].
        /// </summary>
        /// <value><c>true</c> if [parse buttons]; otherwise, <c>false</c>.</value>
        public bool ParseButtons { get; set; } = true;

        /// <summary>
        /// Gets or sets the parse selects.
        /// </summary>
        /// <value>The parse selects.</value>
        public bool ParseOptions { get; set; } = true;
    }
}