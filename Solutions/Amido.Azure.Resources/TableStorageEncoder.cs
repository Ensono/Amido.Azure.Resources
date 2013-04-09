namespace Amido.Azure.Resources
{
    public static class TableStorageEncoder {
        public static string EncodeForTableStorage(this string value) 
        {
            if(!string.IsNullOrEmpty(value)) 
            {
                return value
                    .Replace("/", string.Empty)
                    .Replace(@"\", string.Empty)
                    .Replace("#", string.Empty)
                    .Replace("?", string.Empty)
                    .Replace("~", string.Empty)
                    .Replace(".", string.Empty)
                    .ToLower();
            }

            return string.Empty;
        }
    }
}