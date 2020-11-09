using System;

namespace shortenerTools.Infrastructure
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EntityJsonPropertyConverterAttribute : Attribute
    {
        public EntityJsonPropertyConverterAttribute()
        {
        }
    }
}