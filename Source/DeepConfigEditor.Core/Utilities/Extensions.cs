namespace DeepConfigEditor.Extensions
{
    using System.Collections.Generic;
    using Caliburn.Micro;

    internal static class Extensions
    {
        internal static BindableCollection<T> AddRange<T>(this BindableCollection<T> col, params T[] children)
        {
            return AddRange(col, (IEnumerable<T>)children);
        }

        internal static BindableCollection<T> AddRange<T>(this BindableCollection<T> col, IEnumerable<T> children)
        {
            foreach (var c in children)
            {
                col.Add(c);
            }

            return col;
        }

        internal static string Elipsify(this string value, int maxLength, bool fromLeft = false)
        {
            if (value == null || value.Length <= maxLength)
            {
                return value;
            }

            if (fromLeft)
            {
                return string.Concat("...", value.Substring(value.Length - maxLength + 3));
            }

            return string.Concat(value.Substring(0, maxLength - 3), "...");
        }
    }
}
