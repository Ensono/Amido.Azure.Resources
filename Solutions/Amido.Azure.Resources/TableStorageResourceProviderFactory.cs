using System.Web.Compilation;

namespace Amido.Azure.Resources
{
    /// <summary>
    /// The resource provider factory implementation for Azure table storage.
    /// </summary>
    [DesignTimeResourceProviderFactory(typeof(TableStorageResourceProviderFactory))]
    public class TableStorageResourceProviderFactory : ResourceProviderFactory 
    {
        /// <summary>
        /// Gets a global resource provider used to access shared resources.
        /// </summary>
        /// <param name="classname">The class key used for the name of the resource to load.</param>
        /// <returns>An <see cref="System.Web.Compilation.IResourceProvider" /> able to read the requested resource.</returns>
        public override IResourceProvider CreateGlobalResourceProvider(string classname) 
        {
            return new TableStorageResourceProvider(null, classname);
        }

        /// <summary>
        /// Gets a local resource provider used to access private resources.
        /// </summary>
        /// <param name="virtualPath">The virtual path for the page whose resources are being requested.</param>
        /// <returns>An <see cref="System.Web.Compilation.IResourceProvider" /> able to read the requested resource.</returns>
        public override IResourceProvider CreateLocalResourceProvider(string virtualPath) 
        {
            return new TableStorageResourceProvider(virtualPath, null);
        }
    }
}