using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Globalization;
using System.Resources;
using System.Web.Compilation;
using Amido.Azure.Resources.Properties;
using Microsoft.WindowsAzure;

namespace Amido.Azure.Resources
{
    /// <summary>
    /// The ASP.NET resource provider for Azure table storage
    /// </summary>
    /// <remarks>Implements <see cref="IResourceProvider"/> to provide standard implicit and explicit localization.</remarks>
    public class TableStorageResourceProvider : IResourceProvider 
    {
        private const string InvariantCultureName = "en-gb";
        private TableStorageResourceReader reader;
        private readonly string classname;
        private readonly string virtualPath;
        private readonly IResourceRepository resourceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageResourceProvider"/> class.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <param name="classname">The name of the resource set.</param>
        public TableStorageResourceProvider(string virtualPath, string classname) 
        {
            this.classname = classname.EncodeForTableStorage();
            this.virtualPath = virtualPath.EncodeForTableStorage();
            var resourceTableName = CloudConfigurationManager.GetSetting("TableStorage.ResourceTable");
            resourceRepository = new ResourceRepository(ResourceRepository.AccountConfiguration(resourceTableName));
        }

        /// <summary>
        /// Returns a resource object for the key and culture.
        /// </summary>
        /// <param name="resourceKey">The key identifying a particular resource.</param>
        /// <param name="culture">The culture identifying a localized value for the resource.</param>
        /// <returns>
        /// An <see cref="T:System.Object"/> that contains the resource value for the <paramref name="resourceKey"/> and <paramref name="culture"/>.
        /// </returns>
        public object GetObject(string resourceKey, CultureInfo culture) 
        {
            object value = null;
            var uiCulture = culture ?? CultureInfo.CurrentUICulture;

            var resources = GetResources(uiCulture);

            if(resources.ContainsKey(resourceKey.ToLower())) 
            {
                value = resources[resourceKey.ToLower()];
            }

            if(value != null) 
            {
                return value;
            }

            if(string.IsNullOrEmpty(uiCulture.Name)) 
            {
                throw new TableStorageResourceProviderException(string.Format(Properties.Strings.ResourceNotFoundException, resourceKey, classname, uiCulture.DisplayName));
            }

            return GetObject(resourceKey, uiCulture.Parent);
        }

        /// <summary>
        /// Gets an object to read resource values from a source.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The <see cref="T:System.Resources.IResourceReader"/> associated with the current resource provider.
        /// </returns>
        public IResourceReader ResourceReader {
            get { return reader ?? (reader = InitializeReader()); }
        }

        /// <summary>
        /// Initializes the resource reader with all the keys from the invariant culture resources.
        /// </summary>
        /// <returns></returns>
        /// <remarks>This is used only by the ResourceReader property to get a dictionary of resource keys for a resource set.</remarks>
        private TableStorageResourceReader InitializeReader() 
        {
            var invariantCulture = CultureInfo.InvariantCulture;

            try {
                var resources = (IDictionary)GetResources(invariantCulture);
                return new TableStorageResourceReader(resources);
            }
            catch(DataServiceQueryException) {
                // this exception is caught when the resource does not exist in table storage (the method FirstOfDefault throws an exception.
                return null;
            }
            catch(Exception ex) {
                //RoleManager.WriteToLog(Constants.ResourceLogName, Properties.Resources.ErrorLoadingResources);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a dictionary of data identified by its culture.
        /// </summary>
        /// <param name="culture">The culture for the resources to retrieve..</param>
        /// <returns>A dictionary of resources for the specified cultureName.</returns>
        private IDictionary<string, object> GetResources(CultureInfo culture) 
        {
            if(culture == null) 
            {
                throw new ArgumentNullException("culture", Strings.CultureCannotBeNull);
            }

            string resourceSetName;

            if(!string.IsNullOrEmpty(virtualPath)) 
            {
                resourceSetName = SecUtility.CombineToKey(culture.Name, virtualPath);
            }
            else 
            {
                var cultureKey = !string.IsNullOrEmpty(culture.Name) ? culture.Name : InvariantCultureName;
                resourceSetName = SecUtility.CombineToKey(cultureKey, classname);
            }

            return resourceRepository.GetResources(resourceSetName, culture);
        }
    }
}