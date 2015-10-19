using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Cielo24
{
    public class Utils
    {
        /* Concatenates baseUri, actionPath and key-value pairs from the dictionary, returning a uri */
        public static Uri BuildUri(string baseUri, string actionPath, Dictionary<string, string> dictionary){
            var uriString = baseUri + actionPath + "?" + ToQuery(dictionary);
            return new Uri(uriString);
        }

        /* Creates a query string from key-value pairs in the dictionary */
        public static string ToQuery(Dictionary<string, string> dictionary){
            if (dictionary == null) { return ""; }
            var pairs = dictionary.Select(pair => pair.Key + "=" + Uri.EscapeDataString(pair.Value));
            return string.Join("&", pairs);
        }

        /* Deserializes given JSON into an object of type T */
        public static T Deserialize<T>(string json)
        {
            var result = JsonConvert.DeserializeObject<T>(json);
            return result;
        }

        /* Encodes the supplied Url into an escaped format */
        public static string EncodeUrl(Uri uri)
        {
            return EncodeString(uri.ToString());
        }

        /* Encodes the supplied string into an escaped format */
        public static string EncodeString(string str)
        {
            return Uri.EscapeUriString(str);
        }

        /* Unescapes a string */
        public static string UnescapeUrl(string uriString)
        {
            return Uri.UnescapeDataString(uriString);
        }

        /* Joins list with delimeter, adding quotes around every element (result of the form ["item 1", "item2", "item 3"])*/
        public static string JoinQuoteList<T>(List<T> list, string delimeter)
        {
            return "[" + string.Join(delimeter, list.Select(t => "\"" + t + "\"")) + "]";
        }

        /* Concatinates two dictionaries together returning one */
        public static Dictionary<string, string> DictConcat(Dictionary<string, string> d1, Dictionary<string, string> d2)
        {
            foreach (var pair in d2)
            {
                d1.Add(pair.Key, pair.Value);
            }
            return d1;
        }

        public static string DateToIsoFormat(DateTime? dateTime)
        {
            return ((DateTime)dateTime).ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
        }
    }
}