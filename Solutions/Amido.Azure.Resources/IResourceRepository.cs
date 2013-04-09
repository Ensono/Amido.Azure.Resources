using System.Collections.Generic;
using System.Globalization;

namespace Amido.Azure.Resources
{
    public interface IResourceRepository
    {
        IDictionary<string, object> GetResources(string resourceSetName, CultureInfo culture);
    }
}