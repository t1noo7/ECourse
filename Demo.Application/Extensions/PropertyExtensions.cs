using Demo.Core.Models;
using System.ComponentModel;
using System.Reflection;

namespace Demo.Application.Extensions
{
    public static class PropertyExtensions
    {
        public static IEnumerable<SystemParamData> GetDataTypes<T>()
        {
            var properties = typeof(T).IsInterface ? (new Type[] { typeof(T) })
                   .Concat(typeof(T).GetInterfaces())
                   .SelectMany(i => i.GetProperties()) :
            typeof(T).GetProperties();

            foreach (PropertyInfo p in properties)
            {
                var descriptionAttribute = p.GetCustomAttribute(typeof(DescriptionAttribute), inherit: false) as DescriptionAttribute;
                yield return new SystemParamData { DataName = p.Name, Type = p.PropertyType.Name, Description = descriptionAttribute?.Description };
            }
        }
    }
}
