namespace Tool
{
    public static class Utility
    { }

    public static class GenericUtility<T>
    {
        public static T[] Copy(T[] source)
        {
            var res = new T[source.Length];
            System.Array.Copy(source, 0, res, 0, source.Length);
            return res;
        }
    }
}