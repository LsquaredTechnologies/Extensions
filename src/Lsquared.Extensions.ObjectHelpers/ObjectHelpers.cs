using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lsquared.Internal
{
    internal static class ObjectHelpers
    {
        public static IDictionary<string, object> ObjectToDictionary(object obj)
        {
            var dict = new Dictionary<string, object>(20);
            var type = obj.GetType();

            PropertyInfo[] properties;
            if (!_propertiesCache.TryGetValue(type, out properties))
            {
                properties = type.GetTypeInfo().DeclaredProperties.ToArray();
                _propertiesCache.Add(type, properties);
            }

            foreach (var property in properties)
            {
                dict[property.Name] = property.GetValue(obj);
            }

            return dict;
        }

        #region Fields

        private static readonly Dictionary<Type, PropertyInfo[]> _propertiesCache = new Dictionary<Type, PropertyInfo[]>();
        
        #endregion
    }
}
