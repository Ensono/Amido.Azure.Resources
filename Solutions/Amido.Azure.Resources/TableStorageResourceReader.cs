using System;
using System.Collections;
using System.Resources;

namespace Amido.Azure.Resources
{
    public class TableStorageResourceReader : IResourceReader
    {
        private readonly IDictionary resources;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageResourceReader"/> class.
        /// </summary>
        /// <param name="resources">The resources.</param>
        public TableStorageResourceReader(IDictionary resources) 
        {
            this.resources = resources;
        }

        /// <summary>
        /// Returns an <see cref="T:System.Collections.IDictionaryEnumerator"/> of the resources for this reader.
        /// </summary>
        /// <returns>
        /// A dictionary enumerator for the resources for this reader.
        /// </returns>
        IDictionaryEnumerator IResourceReader.GetEnumerator() 
        {
            return resources.GetEnumerator();
        }

        /// <summary>
        /// Closes the resource reader after releasing any resources associated with it.
        /// </summary>
        void IResourceReader.Close() 
        {
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() 
        {
            return resources.GetEnumerator();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        void IDisposable.Dispose() 
        {
        }
    }
}