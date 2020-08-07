using System;
using System.Runtime.Serialization;

namespace Automa.IO
{
    /// <summary>
    /// SecondAttemptFailedException
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class SecondAttemptFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecondAttemptFailedException" /> class.
        /// </summary>
        public SecondAttemptFailedException() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="SecondAttemptFailedException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SecondAttemptFailedException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="SecondAttemptFailedException" /> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected SecondAttemptFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="SecondAttemptFailedException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public SecondAttemptFailedException(string message, Exception inner) : base(message, inner) { }
    }
}

