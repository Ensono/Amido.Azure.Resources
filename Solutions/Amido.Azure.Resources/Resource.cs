using Microsoft.WindowsAzure.Storage.Table;

namespace Amido.Azure.Resources
{
    public class Resource : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Resource"/> class.
        /// </summary>
        public Resource()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Resource"/> class.
        /// </summary>
        /// <param name="cultureName">Name of the culture.</param>
        /// <param name="resourceKey">The resource key.</param>
        /// <param name="resourceSetName">Name of the resource set.</param>
        /// <param name="data">The data.</param>
        public Resource(string cultureName, string resourceKey, string resourceSetName, string data)
            : base(SecUtility.CombineToKey(cultureName, resourceSetName), resourceKey)
        {
            CultureName = cultureName;
            ResourceSetName = resourceSetName;
            Data = data;
        }
        
        /// <summary>
        /// Gets or sets the name of the culture.
        /// </summary>
        /// <value>The name of the culture.</value>
        public string CultureName { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource set.
        /// </summary>
        /// <value>The name of the resource set.</value>
        public string ResourceSetName { get; set; }


        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        public string Data { get; set; }
    }
}