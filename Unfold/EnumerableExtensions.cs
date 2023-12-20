static class EnumerableExtensions
{
    public static IEnumerable<T> Each<T>(this IEnumerable<T> items, int n)
    {
        int i = 0;
        foreach (var item in items)
        {
            if (i % n == 0)
            {
                yield return item;
            }
            i += 1;
        }
    }
}