using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace shortenerTools.Infrastructure
{
    public class EntityJsonPropertyConverter
    {
        public static void Serialize<TEntity>(TEntity entity, IDictionary<string, EntityProperty> results)
        {
            entity.GetType().GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(EntityJsonPropertyConverterAttribute), false).Any())
                .ToList()
                .ForEach(x => results.Add(x.Name, new EntityProperty(JsonConvert.SerializeObject(x.GetValue(entity)))));
        }

        public static void Deserialize<TEntity>(TEntity entity, IDictionary<string, EntityProperty> properties)
        {
            entity.GetType().GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(EntityJsonPropertyConverterAttribute), false).Any())
                .ToList()
                .ForEach(x =>
                {
                    if (properties.ContainsKey(x.Name))
                    {
                        x.SetValue(entity,
                            JsonConvert.DeserializeObject(properties[x.Name]?.StringValue, x.PropertyType));
                    }
                });
        }
    }
}