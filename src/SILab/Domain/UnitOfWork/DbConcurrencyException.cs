using System.Runtime.Serialization;

namespace SILab.Domain.UnitOfWork
{
    [Serializable]
    public class DbConcurrencyException : SILabException
    {
        /// <summary>
        /// Creates a new <see cref="DbConcurrencyException"/> object.
        /// </summary>
        public DbConcurrencyException()
        {

        }

        /// <summary>
        /// Creates a new <see cref="SILabException"/> object.
        /// </summary>
        public DbConcurrencyException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {

        }

        /// <summary>
        /// Creates a new <see cref="DbConcurrencyException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        public DbConcurrencyException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates a new <see cref="DbConcurrencyException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public DbConcurrencyException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}