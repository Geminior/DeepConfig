namespace DeepConfig
{
    using System.Collections.Generic;

    public static class TestExtensions
    {
        public static void Add<T>(this ICollection<T> list, params T[] items)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }
        }
    }
}
