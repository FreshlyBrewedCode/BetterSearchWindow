using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterSearchWindow
{
    /// <summary>
    /// Static class that provides utility methods related to the BetterSearchWindow
    /// </summary>
    public static class BetterSearchWindowUtility
    {
        public struct TypeWithAttributes<T> where T : Attribute
        {
            public Type type;
            public IEnumerable<T> attributes;
        }
        
        /// <summary>
        /// Get a list of types that have a attribute of type <see cref="T"/> attached.
        /// </summary>
        /// <typeparam name="T">The attribute type to search for</typeparam>
        /// <returns>A list of types along with the attribute.</returns>
        public static List<TypeWithAttributes<T>> GetTypesByAttribute<T>() where T : System.Attribute
        {
            // Just a fancy LINQ query...
            var typesWithAttribute =
                from a in AppDomain.CurrentDomain.GetAssemblies().AsParallel()
                from t in a.GetTypes()
                let attributes = t.GetCustomAttributes(typeof(T), true)
                where attributes != null && attributes.Length > 0
                select new TypeWithAttributes<T>() { type = t, attributes = attributes.Cast<T>() };
            
            return typesWithAttribute.ToList();
        } 
    }
}