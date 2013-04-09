using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Diagnostics;
using System.Globalization;
using Amido.Azure.Storage.TableStorage;
using Amido.Azure.Storage.TableStorage.Account;

namespace Amido.Azure.Resources
{
    public class ResourceRepository : TableStorageRepository<Resource>, IResourceRepository
    {
        private readonly Dictionary<string, Dictionary<string, object>> resourceCache;
        
        public ResourceRepository()
            : base(AccountConfiguration())
        {
            resourceCache = new Dictionary<string, Dictionary<string, object>>();
        }

        public ResourceRepository(string tableName)
            : base(AccountConfiguration(tableName))
        {
            resourceCache = new Dictionary<string, Dictionary<string, object>>();
        }

        public static AccountConfiguration<Resource> AccountConfiguration()
        {
            return new AccountConfiguration<Resource>("TableStorage.ConnectionString"); 
        }

        public static AccountConfiguration<Resource> AccountConfiguration(string tableName)
        {
            return new AccountConfiguration<Resource>("TableStorage.ConnectionString", tableName);
        }

        public IDictionary<string, object> GetResources(string resourceSetName, CultureInfo culture) 
        {
            var resourceDictionary = new Dictionary<string, object>();
            var partitionKey = resourceSetName;

            try {
                if(resourceCache.ContainsKey(partitionKey)) 
                {
                    return resourceCache[partitionKey];
                }

                var resourceRows = ListByPartitionKey(partitionKey).All();

                foreach(var resourceRow in resourceRows) 
                {
                    if(!resourceDictionary.ContainsKey(resourceRow.RowKey.ToLower())) 
                    {
                        resourceDictionary.Add(resourceRow.RowKey.ToLower(), resourceRow.Data);
                    }
                    else 
                    {
                        Trace.TraceError("Key already added: " + resourceRow.RowKey.ToLower());
                    }
                }

                if(!resourceCache.ContainsKey(partitionKey)) 
                {
                    resourceCache.Add(partitionKey, resourceDictionary);
                }
                else 
                {
                    Trace.TraceError("Key already added: " + partitionKey);
                }

                return resourceDictionary;
            }
            catch(DataServiceQueryException) {
                // this exception is caught when the resource does not exist in table storage (the method FirstOfDefault throws an exception.
                return null;
            }
            catch(Exception ex) 
            {
                Trace.TraceError(ex.ToString());
                throw;
            }
        }
    }
}