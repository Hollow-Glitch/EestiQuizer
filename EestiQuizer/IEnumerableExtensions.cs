using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EestiQuizer; 

internal static class IEnumerableExtensions {
    extension(IEnumerable<string?> strings)
    {
        public string StringJoin(string separator) => 
            string.Join(separator, strings.Where(s => s is not null) );
    }
}
