using System.IO;

namespace EestiQuizer.Sonapi; 


// Since I am collecting the words from each chapter, I am already saving them into files with the chapter name in it.
// The chapter "name", ex. `chapt_03`, or that it is a blue box word, I know I want as a tag.
// So the point of this class is so that when I am reading the words from the file I immediately group them with these tags.
// `filepath` ... meta info, so that if fails to load we can report where is it coming from
internal class WordToLoad(string word, IEnumerable<string> tags) {
    internal string Word { get; } = word;
    internal IEnumerable<string> Tags { get; } = tags;
}
