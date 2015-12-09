using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Cielo24
{
    internal class Utils
    {
        /* Creates a query string from key-value pairs in the dictionary */
        public static string ToQuery(Dictionary<string, string> dictionary, bool dontEscape = false){
            if (dictionary == null)
                return "";
            var pairs = (from pair in dictionary
                         let value = dontEscape ? pair.Value : Uri.EscapeDataString(pair.Value)
                         select pair.Key + "=" + value).ToList();
            return string.Join("&", pairs);
        }

        /* Deserializes given JSON into an object of type T */
        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static bool TryDeserialize<T>(string json, out T result)
        {
            try
            {
                result = Deserialize<T>(json);
                return true;
            }
            catch (JsonReaderException)
            {
                result = default(T);
                return false;
            }
        }

        /* Joins list with delimeter, adding quotes around every element (result of the form ["item 1", "item2", "item 3"])*/
        public static string JoinQuoteList<T>(List<T> list, string delimeter)
        {
            if (list == null)
                return null;

            return "[" + string.Join(delimeter, list.Select(t => "\"" + t + "\"")) + "]";
        }

        public static string DateToIsoFormat(DateTime? dateTime)
        {
            return dateTime?.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
        }
    }
}