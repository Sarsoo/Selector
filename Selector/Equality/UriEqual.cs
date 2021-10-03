using System;

namespace Selector
{
    public class UriEqual : IEqual
    {
        public bool IsEqual<T>(T item, T other)
        {
            var uri = typeof(T).GetProperty("Uri");
            
            if(uri is not null)
                return (string) uri.GetValue(item) == (string) uri.GetValue(other);
            else 
            {
                var id = typeof(T).GetProperty("Id");

                if(id is not null)
                    return (string) id.GetValue(item) == (string) id.GetValue(other);
                else 
                    throw new ArgumentException($"{typeof(T)} does not contain a uri or id");
            }
        }
    }
}
