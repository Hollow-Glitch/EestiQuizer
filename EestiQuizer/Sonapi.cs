using System.Text.Json.Serialization;

namespace EestiQuizer; 

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WordClass {
    /// <summary>
    ///     <list type="bullet">
    ///         <item> nimisõna   : kolleeg  </item>
    ///         <item> omadussõna : roheline </item>
    ///         <item> arvsõna    : kaks </item>
    ///         <item> asesõna    : see, kõik </item>
    ///     </list>
    /// </summary>
    noomen,

    /// <summary>
    ///     <list type="bullet">
    ///         <item> tegusõna : vabandama </item>
    ///     </list>
    /// </summary>
    verb,

    /// <summary>
    ///     <list type="bullet">
    ///         <item> sidesõna : kuid, ja </item>
    ///         <item> määrsõna : koos, ka </item>
    ///         <item> tagasõna : pärast </item>
    ///         <item> eessõna  : ilma </item>
    ///         <item> hüüdsõna : pst </item>
    ///     </list>
    /// </summary>
    muutumatu,
}


public class SonapiResponse {
    [JsonPropertyName("requestedWord")]
    public string? RequestedWord { get; set; }

    [JsonPropertyName("estonianWord")]
    public string? EstonianWord { get; set; }

    [JsonPropertyName("searchResult")]
    // The sg. vs pl. mismatch is intended - API has sg. form but is actually an array hence why in my internal API I want plural.
    public SearchResult[]? SearchResults { get; set; }

    [JsonPropertyName("translations")]
    public TranslationMain[]? Translations { get; set; }


    public string? FirstWordFormValue(WordFormCode code) {
        var formSgN = SearchResults
            ?.FirstOrDefault()
            ?.WordForms
            ?.FirstOrDefault(form => form.Code?.Equals(code.Value) ?? false)
            ?.Value;

        return formSgN;
    }

    public IEnumerable<string>? WordFormValues(WordFormCode code) {
        IEnumerable<string>? formSgN = SearchResults
            ?.FirstOrDefault()
            ?.WordForms
            ?.Where(form => string.Equals(form.Code, code.Value, StringComparison.Ordinal))
            .Select(form => form.Value)
            //>> next instructions are just so that compiler can reason about type safety.
            .Where(value => ! string.IsNullOrWhiteSpace(value) ) //<< we filter out only those which are not null (or empty)
            .Select(value => value!); //<< since we have filtered out null values (empty is not important here)
                                      //   with `!` we are explaining that thus no null value is ensured.
        return formSgN;
    }


    /// <summary>
    /// <para>
    /// Instead of digging the translations withing the meanings(aka "definitions" - but that is a field actually)
    /// Here we are just pulling the immediate translations list.
    /// Still the purpose here is that if in the future changes something this serves as a layer.
    /// </para>
    /// Postcondition: distinctness ensured.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string>? OuterEngTranslations(int countPerDef) {
        const string from = "et";
        const string to = "en";
        var outerTranslations =  Translations
            ?.FirstOrDefault(translation
                => translation.To is to
                && translation.From is from
            )
            ?.Translations
            ?.Distinct();
        return countPerDef switch {
            < 0 => outerTranslations,
            _ => outerTranslations?.Take( countPerDef)
        };
    }
}


public class SearchResult {
    //[JsonPropertyName("wordClasses")]
    //public string[]? WordClasses { get; set; }
    [JsonPropertyName("wordClasses")]
    public WordClass?[]? WordClasses { get; set; }

    [JsonPropertyName("wordForms")]
    public WordForm[]? WordForms { get; set; }

    [JsonPropertyName("meanings")]
    public Meaning[]? Meanings { get; set; }

    [JsonPropertyName("similarWords")]
    public string[]? SimilarWords { get; set; }


    /// <summary>
    /// Postcondition: distinctness ensured.
    /// </summary>
    /// <param name="countPerDef"></param>
    /// <returns></returns>
    public IEnumerable<string>? InnerEngTranslations(int countPerDef) {
        var innerTranslations =
            Meanings
            ?.Select(meaning => meaning.Translations)
            //>> Filter & tell compiler that: translation is not null
            .Where(translation => translation is not null).Select(t => t!)
            //>> Filter & tell compiler that: translation.EnglishTranslations is not null
            .Where(translation => translation.EnglishTranslations is not null)
            .SelectMany(translation => translation.EnglishTranslations! )
            //>> End of type theory trickery.
            .OrderByDescending(engTrans => engTrans.Weight) // .Take( (int) countPerDef) //<< this doesn't work if `countPerDef` is `-1`
            .SelectMany(engTrans => engTrans.WordsSeparated() )
            .Distinct();

        return countPerDef switch {
            < 0 => innerTranslations,
            _ => innerTranslations?.Take( countPerDef)
        };
    }


    /// <returns>
    /// Returns <paramref name="examplesPerMeaning"/> number of example sentences 
    /// per <paramref name="meaningsToConsider"/> meanings. Thus if params are 2 & 2, but first has 3 while second has 1
    /// then we get just 3 examples.
    /// </returns>
    public IEnumerable<string>? ExamplesPerConsideredMeanings(
        uint examplesPerMeaning, 
        uint meaningsToConsider, 
        string defPrefix = "D", 
        string separator = ": "
    ) {
        IEnumerable<string>? examples =
            Meanings
            ?.Take( (int) meaningsToConsider)
            ?.SelectMany( (meaning, defId) => 
                meaning
                    ?.Examples
                    ?.OrderBy(ex => ex, StringComparer.OrdinalIgnoreCase)
                    ?.Take( (int) examplesPerMeaning ) // meaning == definition ... more or less (technicaly definition is a meaning field).
                    ?.Select(example => $"{defPrefix}{defId+1}{separator}{example}") // `+1` to convert from index to "ordinal" (i.e. base 1).
                    ?? Enumerable.Empty<string>()
            )
            .Where(example => ! string.IsNullOrEmpty(example) );
        return examples;
    }


    /// <summary>
    /// Collects <paramref name="examplesPerMeaning"/> number of example sentences per meanings
    /// into a single sequence and from this sequence returns <paramref name="maxExampleCount"/>.
    /// </summary>
    public IEnumerable<string>? ExamplesCappedPerMeanings(
        uint maxExampleCount, 
        uint examplesPerMeaning, 
        string defPrefix = "D", 
        string separator = ": "
    ) {
        IEnumerable<string>? examples =
            Meanings
            ?.SelectMany( (meaning, defId) => 
                meaning
                    ?.Examples
                    ?.OrderBy(ex => ex, StringComparer.OrdinalIgnoreCase)
                    ?.Take( (int) examplesPerMeaning ) // meaning == definition ... more or less (technicaly definition is a meaning field).
                    ?.Select(example => $"{defPrefix}{defId+1}{separator}{example}") // `+1` to convert from index to "ordinal" (i.e. base 1).
                    ?? Enumerable.Empty<string>()
            )
            .Where(example => ! string.IsNullOrEmpty(example) )
            ?.Take( (int) maxExampleCount);
        return examples;
    }


    /// <summary>
    /// TODO
    /// </summary>
    //public List<string>? ExamplesBalancedPerMeanings(
    public IEnumerable<string> ExamplesBalancedPerMeanings(
        uint maxExampleCount, 
        int firstNMeaningsToConsider = -1, // if `-1` then consider all
        string defPrefix = "D", 
        string separator = ": "
    ) {
        var exampleEnumeratorsPerMeaning =
            Meanings
            ?.Select( (meaning, defId) => 
                meaning
                    ?.Examples
                    ?.OrderBy(ex => ex, StringComparer.OrdinalIgnoreCase)
                    ?.Select(example => $"{defPrefix}{defId+1}{separator}{example}") // `+1` to convert from index to "ordinal" (i.e. base 1).
                    ?? Enumerable.Empty<string>()
            )
            .Where(examples => examples.Count() != 0)
            .Select(examples => examples.GetEnumerator() )
            .Take(firstNMeaningsToConsider < 0 ? int.MaxValue : firstNMeaningsToConsider)
            .ToList(); // we need to materialize here otherwise the enumerators will be recalc and seem as if always reset below in foreach.

        // list vs yield
        //if (exampleEnumeratorsPerMeaning is null)  return null;
        // vs
        if (exampleEnumeratorsPerMeaning is null) yield break;

        // We want to give higher relevance to the first definitions.
        // If it would happen for example that we want 2 examples preferably per meaning, but we have:
        // - def1 has 5 examples
        // - def2 has 1 example
        // then we definitely don't want 3 examples: 2 for def1 & 1 for def2.
        // What we want is 4 examples: 3 for def1 & 1 for def2 - so that we reach the max.
        //
        // We need to do a round-robin algo here.
        //
        // ?? Do I also want to determine the number of relevant definitions to consider ??

        //>> Maybe the logic with the while condition and goto and break is a bit redundant but better be safe than eternaly-looping :D
        //List<string> collectedExamples = []; //not doing the list approach.
        int collectedExamples = 0;
        //while ( collectedExamples < maxExampleCount ) {
        while (true) { // simplifying
            foreach(var enumerator in exampleEnumeratorsPerMeaning) {
                //>> list vs yield
                // if (enumerator.MoveNext() ) collectedExamples.Add(enumerator.Current);
                // vs
                if (enumerator.MoveNext() ) {
                    collectedExamples++;
                    yield return enumerator.Current;
                }

                //>> list vs yield
                //if (collectedExamples.Count >= maxExampleCount) goto AdditionFinished;
                // vs
                if (collectedExamples >= maxExampleCount) yield break;
            }
            if (exampleEnumeratorsPerMeaning.All(e => e.MoveNext() is false) ) yield break;
        }
    }


    /// <summary>
    /// Goes through each meaning and collect all of their examples. 
    /// With round-robin try to fulfill constraints (`maxExampleCount`, `firstNMeaningsToConsider`).
    /// Ensures that we can't get a result like: M1, M1, M2, M1;
    /// by actually collecting into an internal structure per meaning and the in the end collecting all examples per meaning.
    /// </summary>
    /// <returns>
    /// Returns examples per "meaning" while trying to fulfill given constraints such that it is prioritizing meanings in their order.
    /// </returns>
    //public List<string>? ExamplesBalancedPerMeanings(
    public IEnumerable<string>? ExamplesBalancedGroupedPerMeanings(
        uint maxExampleCount, 
        int firstNMeaningsToConsider = -1, // if `-1` then consider all
        string idPrefix = "M", 
        string separator = ": "
    ) {
        var exampleEnumeratorsPerMeaning =
            Meanings
            ?.Select( (meaning, id) => 
                meaning
                    ?.Examples
                    ?.OrderBy(ex => ex.Length)
                    ?.Select(example => $"{idPrefix}{id+1}{separator}{example}") // `+1` to convert from index to "ordinal" (i.e. base 1).
                    ?? Enumerable.Empty<string>()
            )
            .Where(examples => examples.Count() != 0)
            .Select(examples => examples.GetEnumerator() )
            .Take(firstNMeaningsToConsider < 0 ? int.MaxValue : firstNMeaningsToConsider)
            .Select(enumerator => (chosens: new List<string>(), enumerator) )
            .ToList(); // we need to materialize here otherwise the enumerators will be recalc and seem as if always reset below in foreach.

        if (exampleEnumeratorsPerMeaning is null) return null;

        // We want to give higher relevance to the first meanings.
        // If it would happen for example that we want 2 examples preferably per meaning, but we have:
        // - M1 has 5 examples
        // - M2 has 1 example
        // then we definitely don't want 3 examples: 2 for M1 & 1 for M2.
        // What we want is 4 examples: 3 for M1 & 1 for M2 - so that we reach the max.
        //
        //>> We need to do a round-robin algo here.

        int collectedExamples = 0;
        while (true) {
            foreach(var pair in exampleEnumeratorsPerMeaning) {
                if (pair.enumerator.MoveNext() ) {
                    collectedExamples++;
                    pair.chosens.Add(pair.enumerator.Current);
                }

                if (collectedExamples >= maxExampleCount) goto AdditionFinished;
            }
            if (exampleEnumeratorsPerMeaning.All(pair => pair.enumerator.MoveNext() is false) ) break;
        }
        AdditionFinished:
        return exampleEnumeratorsPerMeaning.SelectMany(pair => pair.chosens);
    }
}


public class Meaning {
    [JsonPropertyName("definition")]
    public string? Definition { get; set; }

    [JsonPropertyName("partOfSpeech")]
    // e.g., noun, verb, etc.
    public PartOfSpeechDetail[]? LexicalCategories { get; set; }
    public class PartOfSpeechDetail {
        /// <summary>
        ///     Takes the begginning of the english part of the `Value`, ex.: s  OR  v  OR  konj  OR  adj
        /// </summary>
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        /// <summary>
        ///     e.g., noun, verb, etc.
        ///     <list type="bullet">
        ///         <item>"nimisõna, substantiiv"</item>
        ///         <item>"tegusõna, verb"</item>
        ///         <item>"sidesõna, konjunktsioon"</item>
        ///         <item>"omadussõna, adjektiiv"</item>
        ///     </list>
        /// </summary>
        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }

    // Don't know what rection is, but it's in the API.
    [JsonPropertyName("rection")]
    public string? Rection { get; set; }

    [JsonPropertyName("examples")]
    public string[]? Examples { get; set; }

    [JsonPropertyName("synonyms")]
    public string[]? Synonyms { get; set; }

    [JsonPropertyName("translations")]
    public TranslationMeaning? Translations { get; set; }
}


public class TranslationMeaning {
    public class WeightedTranslation {
        [JsonPropertyName("words")]
        public string? Words { get; set; }

        [JsonPropertyName("weight")]
        public float? Weight { get; set; }

        public IEnumerable<string> WordsSeparated() {
            return Words
                ?.Split(',')
                .Select(word => word.Trim() ) ?? [];
        }
    }

    [JsonPropertyName("eng")]
    public WeightedTranslation[]? EnglishTranslations { get; set; }

    [JsonPropertyName("fra")]
    public WeightedTranslation[]? FrenchTranslations { get; set; }

    [JsonPropertyName("ukr")]
    public WeightedTranslation[]? UkrainianTranslations { get; set; }

    [JsonPropertyName("rus")]
    public WeightedTranslation[]? RussianTranslations { get; set; }
}


public class WordForm {
    [JsonPropertyName("inflectionType")]
    public string? InflectionType { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("morphValue")]
    public string? MorphValue { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}


public class TranslationMain {
    [JsonPropertyName("from")]
    public string? From { get; set; }

    [JsonPropertyName("to")]
    public string? To { get; set; }

    [JsonPropertyName("input")]
    public string? Input { get; set; }

    [JsonPropertyName("translations")]
    public string[]? Translations { get; set; }
}


public class WordFormCode
{
    private WordFormCode(string value) { Value = value; }

    public string Value { get; private set; }

    public static WordFormCode verb_Ma   => new WordFormCode("Sup");      // sup - supine     : ma-form
    public static WordFormCode verb_Da   => new WordFormCode("Inf");      // inf - infinitive : da-form
    public static WordFormCode verb_Sg1P => new WordFormCode("IndPrSg1"); // 1. person singular

    public static WordFormCode noomen_SgN => new WordFormCode("SgN");
    public static WordFormCode noomen_SgG => new WordFormCode("SgG");
    public static WordFormCode noomen_SgP => new WordFormCode("SgP");

    public static WordFormCode muutumatu_ID => new WordFormCode("ID");

    public override string ToString() => Value;
}
