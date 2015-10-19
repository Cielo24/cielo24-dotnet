using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Cielo24.Options
{
    /* The base class. All of the other option classes inherit from it. */
    public abstract class BaseOptions
    {
        /* Returns a dictionary that contains key-value pairs of options, where key is the Name property
         * of the QueryName attribute assigned to every option and value is the value of the property.
         * Options with null value are not included in the dictionary. */
        public virtual Dictionary<string, string> GetDictionary()
        {
            var queryDictionary = new Dictionary<string, string>();
            var properties = GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(this, null);
                if (value == null) // If property is null, don't include the key-value pair in the dictioanary
                    continue;
                var key = (QueryName)property.GetCustomAttributes(typeof(QueryName), true).First();
                queryDictionary.Add(key.Name, GetStringValue(value));
            }
            return queryDictionary;
        }

        /* Returns a query string representation of options */
        public virtual string ToQuery()
        {
            var queryDictionary = GetDictionary();
            return Utils.ToQuery(queryDictionary);
        }

        /* Sets the property whose QueryName attribute matches the key */
        public virtual void PopulateFromKeyValuePair(KeyValuePair<string, string> pair)
        {
            var properties = GetType().GetProperties();
            foreach (var property in properties)
            {
                var key = (QueryName)property.GetCustomAttributes(typeof(QueryName), true).First();
                var type = property.PropertyType;
                if (!key.Name.Equals(pair.Key))
                    continue;
                property.SetValue(this, GetValueFromString(pair.Value, type), null);
                return;
            }
            throw new ArgumentException("Invalid Option: " + pair.Key);
        }

        // Array of strings in the key=value form 
        public void PopulateFromArray(string[] array)
        {
            foreach (var dictionary in from s in array ?? new string[0]
                                       select Regex.Matches(s, "([^?=&]+)(=([^&]*))?").Cast<Match>().ToDictionary(x => x.Groups[1].Value, x => x.Groups[3].Value))
            {
                PopulateFromKeyValuePair(dictionary.First());
            }
        }

        /* Converts string into an object */
        protected object GetValueFromString(string str, Type type)
        {
            var result = JsonConvert.DeserializeObject("\"" + str + "\"", type); // Quotes are necessary in json
            return result;
        }

        /* Converts 'value' into string based on its type. Precondition: value != null */
        protected string GetStringValue(object value)
        {
            if (value is List<string>)
            {
                return Utils.JoinQuoteList((List<string>)value, ", ");
            }
            if (value is List<Tag>)
            {
                return Utils.JoinQuoteList((List<Tag>)value, ", ");
            }
            if (value is List<Fidelity>)
            {
                return Utils.JoinQuoteList((List<Fidelity>)value, ", ");
            }
            if (value is char[])       // char[] (returned as (a, b))
            {
                return "(" + string.Join(", ", ((char[])value)) + ")";
            }
            if (value is DateTime)     // DateTime (in ISO 8601 format)
            {
                return Utils.DateToIsoFormat((DateTime)value);
            }
            if (value is Enum)
            {
                return ((Enum)value).GetDescription();
            }
            // Takes care of the rest: int, bool, string, Uri
            return value.ToString();
        }
    }
}
