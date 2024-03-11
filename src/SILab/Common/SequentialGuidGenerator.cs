using SILab.Threading;
using System.Security.Cryptography;


namespace SILab.Common
{

    /// <summary>
    /// Implements <see cref="IGuidGenerator"/> by creating sequential Guids.
    /// </summary>
    public class SequentialGuidGenerator : IGuidGenerator
    {
        /// <summary>
        /// Gets the singleton <see cref="SequentialGuidGenerator"/> instance.
        /// </summary>
        public static SequentialGuidGenerator Instance { get; } = new SequentialGuidGenerator();

        private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

        public SequentialGuidDatabaseType DatabaseType { get; set; }

        private SequentialGuidGenerator()
        {
            DatabaseType = SequentialGuidDatabaseType.SqlServer;
        }

        public Guid Create()
        {
            return Create(DatabaseType);
        }

        public Guid Create(SequentialGuidDatabaseType databaseType)
        {
            switch (databaseType)
            {
                case SequentialGuidDatabaseType.SqlServer:
                    return Create(SequentialGuidType.SequentialAtEnd);
                case SequentialGuidDatabaseType.Oracle:
                    return Create(SequentialGuidType.SequentialAsBinary);
                case SequentialGuidDatabaseType.MySql:
                    return Create(SequentialGuidType.SequentialAsString);
                case SequentialGuidDatabaseType.PostgreSql:
                    return Create(SequentialGuidType.SequentialAsString);
                default:
                    throw new InvalidOperationException();
            }
        }

        public Guid Create(SequentialGuidType guidType)
        {
            // We start with 16 bytes of cryptographically strong random data.
            var randomBytes = new byte[10];
            Rng.Locking(r => r.GetBytes(randomBytes));
 
            long timestamp = DateTime.UtcNow.Ticks / 10000L;

            // Then get the bytes
            byte[] timestampBytes = BitConverter.GetBytes(timestamp);

            // Since we're converting from an Int64, we have to reverse on
            // little-endian systems.
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(timestampBytes);
            }

            byte[] guidBytes = new byte[16];

            switch (guidType)
            {
                case SequentialGuidType.SequentialAsString:
                case SequentialGuidType.SequentialAsBinary:

                    // For string and byte-array version, we copy the timestamp first, followed
                    // by the random data.
                    Buffer.BlockCopy(timestampBytes, 2, guidBytes, 0, 6);
                    Buffer.BlockCopy(randomBytes, 0, guidBytes, 6, 10);
                    
                    if (guidType == SequentialGuidType.SequentialAsString && BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(guidBytes, 0, 4);
                        Array.Reverse(guidBytes, 4, 2);
                    }

                    break;

                case SequentialGuidType.SequentialAtEnd:

                    // For sequential-at-the-end versions, we copy the random data first,
                    // followed by the timestamp.
                    Buffer.BlockCopy(randomBytes, 0, guidBytes, 0, 10);
                    Buffer.BlockCopy(timestampBytes, 2, guidBytes, 10, 6);
                    break;
            }

            return new Guid(guidBytes);
        }


        public enum SequentialGuidDatabaseType
        {
            SqlServer,

            Oracle,

            MySql,

            PostgreSql,
        }

        /// <summary>
        /// Describes the type of a sequential GUID value.
        /// </summary>
        public enum SequentialGuidType
        {
            /// <summary>
            /// The GUID should be sequential when formatted using the
            /// <see cref="Guid.ToString()" /> method.
            /// </summary>
            SequentialAsString,

            /// <summary>
            /// The GUID should be sequential when formatted using the
            /// <see cref="Guid.ToByteArray" /> method.
            /// </summary>
            SequentialAsBinary,

            /// <summary>
            /// The sequential portion of the GUID should be located at the end
            /// of the Data4 block.
            /// </summary>
            SequentialAtEnd
        }
    }
}
