using System;

namespace POSSystems.Helpers
{
    public static class StringExtensions
    {
        public static bool Contains(this string str, string substring,
                                    StringComparison comp)
        {
            if (substring == null)
                throw new ArgumentNullException(nameof(substring), "substring cannot be null.");
            else if (!Enum.IsDefined(typeof(StringComparison), comp))
                throw new ArgumentException($"{nameof(comp)} is not a member of StringComparison");

            return str.IndexOf(substring, comp) >= 0;
        }

        public static double? ReturnNullableDouble(string svalue)
        {
            double value = 0;
            double? nValue = null;
            if (double.TryParse(svalue, out value))
            {
                nValue = value;
            }

            return nValue;
        }

        public static int? ReturnNullableInt(string svalue)
        {
            int value = 0;
            int? nValue = null;
            if (int.TryParse(svalue, out value))
            {
                nValue = value;
            }

            return nValue;
        }
    }
}