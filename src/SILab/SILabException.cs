using System.Runtime.Serialization;

namespace SILab
{
    /// <summary>
    /// Base exception type for those are thrown by system for specific exceptions.
    /// </summary>
    [Serializable]
    public class SILabException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="SILabException"/> object.
        /// </summary>
        public SILabException()
        {

        }

        /// <summary>
        /// Creates a new <see cref="SILabException"/> object.
        /// </summary>
        public SILabException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {

        }

        /// <summary>
        /// Creates a new <see cref="SILabException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        public SILabException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates a new <see cref="SILabException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public SILabException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
