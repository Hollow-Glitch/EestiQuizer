using System.Windows.Controls;

namespace EestiQuizer.Common; 

internal static class IEnumerableExtensions {
    extension(IEnumerable<string?> strings)
    {
        public string StringJoin(string separator) => 
            string.Join(separator, strings.Where(s => s is not null) );

        public string StringJoin(char separator) => 
            string.Join(separator, strings.Where(s => s is not null) );
    }

    extension<T>(IEnumerable<T> e) {
        public IEnumerable<(T item, int idx)> WithIndexBase0() => 
            e.Select( (e, idx) => (e, idx) );

        public IEnumerable<(T item, int idx)> WithIndexBase1() => 
            e.Select( (e, idx) => (e, idx+1) );
    }
}
