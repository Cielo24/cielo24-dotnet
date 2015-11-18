using System;

namespace Cielo24
{
    static internal class Assert
    {
        public static void NotNull<T>(T value, string argName)
            where T : class 
        {
            if (value == null)
                throw new ArgumentNullException(argName);
        }

        public static void NotEmpty<T>(T value, string argName)
        {
            if (object.Equals(default(T), value))
                throw new ArgumentException("Value cannot be an empty", argName);
        }

        public static void StringRequired(string value, string argName)
        {
            NotNull(value, argName);
            NotEmpty(value, argName);
        }
    }
}