namespace EestiQuizer.Common; 

internal static class IEnumerableExtensions {
    extension(IEnumerable<string?> strings)
    {
        public string StringJoin(string separator) => 
            string.Join(separator, strings.Where(s => s is not null) );
    }
}
