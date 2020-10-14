using System;
using System.Runtime.Serialization;

namespace Automa.IO
{
    /// <summary>
    /// ChromeDriverException
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class WebDriverException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverException" /> class.
        /// </summary>
        public WebDriverException() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public WebDriverException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverException" /> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected WebDriverException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public WebDriverException(string message, Exception inner) : base(message, inner) { }
    }
}

