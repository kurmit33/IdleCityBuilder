using System;

namespace IdleBuilder.Core
{
    public static class NumberFormatter
    {
        private static readonly string[] Suffixes = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };

        public static string Format(double value)
        {
            if (value < 0) return "-" + Format(-value);
            if (value < 1000) return value.ToString("F0");

            int suffixIndex = 0;
            while (value >= 1000 && suffixIndex < Suffixes.Length - 1)
            {
                value /= 1000;
                suffixIndex++;
            }

            return $"{value:F2}{Suffixes[suffixIndex]}";
        }
    }
}