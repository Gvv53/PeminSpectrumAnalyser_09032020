using System;
using System.Globalization;

namespace PeminSpectrumAnalyser
{
    class Converters
    {
        public static long ValueFromUI(string input, int mul)
        {
            try
            {
                if ((input.Length == 0) || (input == " "))
                    return 0;

                string result = input;
                char sep = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);
                result = result.Replace('.', sep);
                result = result.Replace(',', sep);

                return (long)(double.Parse(result) * mul);
            }
            catch
            {
                return 0;
            }
        }


        public static double DoubleValueFromUI(string input)
        {
            try
            {
                if ((input.Length == 0) || (input == " "))
                    return 0;

                string result = input;
                char sep = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);
                result = result.Replace('.', sep);
                result = result.Replace(',', sep);

                return double.Parse(result);
            }
            catch
            {
                return 0;
            }
        }




        public static string ValueToUI(long value, long mul)
        {
            try
            {
                return ((double)value / mul).ToString();
            }
            catch
            {
                return "0";
            }
        }
        public static string ValueToUI(double value, long mul)
        {
            try
            {
                return (value / mul).ToString();
            }
            catch
            {
                return "0";
            }
        }
    }
}
