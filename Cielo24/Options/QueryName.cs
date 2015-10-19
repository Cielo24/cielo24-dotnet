using System;

namespace Cielo24.Options
{
    [AttributeUsage(AttributeTargets.Property)]
    public class QueryName : Attribute
    {
        public string Name;

        public QueryName(string name)
        {
            this.Name = name;
        }
    }
}
