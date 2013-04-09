using System;

namespace Amido.Azure.Resources
{
    /// <summary>
    /// The exception class for Azure table storage resource provider exceptions.
    /// </summary>
    [Serializable]
    public class TableStorageResourceProviderException : Exception 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageResourceProviderException"/> class.
        /// </summary>
        public TableStorageResourceProviderException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageResourceProviderException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public TableStorageResourceProviderException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageResourceProviderException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public TableStorageResourceProviderException(string message, Exception innerException) : base(message, innerException) { }
    }
}