using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EestiQuizer.Common; 


// Since I am collecting the words from each chapter, I am already saving them into files with the chapter name in it.
// The chapter "name", ex. `chapt_03`, or that it is a blue box word, I know I want as a tag.
// So the point of this class is so that when I am reading the words from the file I immediately group them with these tags.
internal class WordToLoad(string word, IEnumerable<string> tags) {
    internal string Word { get; set; } = word;
    internal IEnumerable<string> Tags { get; set; } = tags;
}
