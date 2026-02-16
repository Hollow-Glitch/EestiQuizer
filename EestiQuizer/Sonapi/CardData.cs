using EestiQuizer.Common;

namespace EestiQuizer.Sonapi; 


public interface ICardDataFragment {
    public string Form1 { get; }
    public string Form2 { get; }
    public string Form3 { get; }
}


internal class NoomenCardData : ICardDataFragment{
    internal string sgN;
    internal string sgG;
    internal string sgP;

    internal NoomenCardData(SearchResult? response, string intraFieldSeparator) {
        sgN = response?.WordFormValues(WordFormCode.noomen_SgN)?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
        sgG = response?.WordFormValues(WordFormCode.noomen_SgG)?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
        sgP = response?.WordFormValues(WordFormCode.noomen_SgP)?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
    }

    public string Form1 => sgN;
    public string Form2 => sgG;
    public string Form3 => sgP;
}


internal class VerbCardData : ICardDataFragment{
    internal string ma  ;
    internal string da  ;
    internal string sg1p;

    internal VerbCardData(SearchResult? response, string intraFieldSeparator) {
        ma   = response?.WordFormValues(WordFormCode.verb_Ma  )?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
        da   = response?.WordFormValues(WordFormCode.verb_Da  )?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
        sg1p = response?.WordFormValues(WordFormCode.verb_Sg1P)?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
    }

    public string Form1 => ma;
    public string Form2 => da;
    public string Form3 => sg1p;
}


internal class MuutumatuCardData : ICardDataFragment{
    internal string form;

    internal MuutumatuCardData(SearchResult? response, string intraFieldSeparator) {
        form = response?.WordFormValues(WordFormCode.muutumatu_ID)?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
    }

    public string Form1 => form;
    public string Form2 => "";
    public string Form3 => "";
}


public class CardData {
    required public WordClass? myWordClass { get; init; }
    required public ICardDataFragment? VariableCardData { get; init; }

    required public string TextWeAskedFor { get; init; }
    required public string? RequestedWord { get; init; }

    required public string? PartOfSpeech { get; init; }
    required public string Translations { get; init; }
    required public string Examples { get; init; }
    required public string Tags { get; init; }

    const string asesõna = nameof(asesõna);
    const string määrsõna = nameof(määrsõna);
    const string nimisõna = nameof(nimisõna);
    const string omadussõna = nameof(omadussõna);
    const string sidesõna = nameof(sidesõna);
    const string tegusõna = nameof(tegusõna);
    const string arvsõna = nameof(arvsõna);
    const string tagasõna = nameof(tagasõna);
    const string eessõna  = nameof(eessõna);
    const string hüüdsõna = nameof(hüüdsõna);
    const string pärisnimi = nameof(pärisnimi);


    public CardData() { }


    internal static IEnumerable<CardData> Load(
        string textWeAskedFor,
        IEnumerable<string> tagsWeWant,
        SonapiResponse? response,
        string intraFieldSeparator,
        int translationCount, //<< <0 means all
        int meaningsToConsider,
        uint maxExampleCount
    ) {

        const string generatedTag = "generated";

        const int takeAll = -1; //<< -1 because we want all and then we limit when merging.
        var common = (
            textWeAskedFor : textWeAskedFor,
            requestedWord  : response?.RequestedWord,
            outers         : response?.OuterEngTranslations(takeAll)?.ToList() ?? [],
            tags           : generatedTag + " " + tagsWeWant.Select(tag => $"{generatedTag}::{tag}").StringJoin(" ")
        );

        if (response?.SearchResults is null) {
            throw new NotImplementedException();
        }
        SearchResult[] searchResults = (response?.SearchResults) !;

        foreach(SearchResult searchResult in searchResults) {
            var examples = searchResult
                .ExamplesBalancedGroupedPerMeanings(maxExampleCount, meaningsToConsider)
                ?.Distinct()?.StringJoin("<br>")
                ?? "";

            string translations; {
                var inners = searchResult.InnerEngTranslations(takeAll)?.ToList() ?? [];
                //if (inners.Count == 0) {
                //    continue;
                //}
                translations = MergedEngTranslations(common.outers, inners, translationCount)?.StringJoin(intraFieldSeparator)
                    ?? "";
            }

            ICardDataFragment? variableCardData; WordClass? wordClass; {
                wordClass = searchResult.WordClasses
                    ?.Where(wc => wc is not null).Select(wc => wc!)
                    .FirstOrDefault();
                variableCardData = wordClass switch {
                    WordClass.noomen => new NoomenCardData(searchResult, intraFieldSeparator),
                    WordClass.verb => new VerbCardData(searchResult, intraFieldSeparator),
                    WordClass.muutumatu => new MuutumatuCardData(searchResult, intraFieldSeparator),
                    null => null,
                    _ => throw new NotImplementedException(),
                };
            }

            string? partOfSpeech; {
                var partOfSpeechValue = searchResult
                    ?.Meanings?.FirstOrDefault()
                    ?.LexicalCategories?.FirstOrDefault()
                    ?.Value;
                if      (partOfSpeechValue?.Contains(asesõna   ) ?? false)  partOfSpeech =   asesõna;
                else if (partOfSpeechValue?.Contains(määrsõna  ) ?? false)  partOfSpeech =  määrsõna;
                else if (partOfSpeechValue?.Contains(nimisõna  ) ?? false)  partOfSpeech =  nimisõna;
                else if (partOfSpeechValue?.Contains(omadussõna) ?? false)  partOfSpeech = omadussõna;
                else if (partOfSpeechValue?.Contains(sidesõna  ) ?? false)  partOfSpeech =  sidesõna;
                else if (partOfSpeechValue?.Contains(tegusõna  ) ?? false)  partOfSpeech =  tegusõna;
                else if (partOfSpeechValue?.Contains(arvsõna   ) ?? false)  partOfSpeech =   arvsõna;
                else if (partOfSpeechValue?.Contains(tagasõna  ) ?? false)  partOfSpeech =  tagasõna;
                else if (partOfSpeechValue?.Contains(eessõna   ) ?? false)  partOfSpeech =   eessõna;
                else if (partOfSpeechValue?.Contains(hüüdsõna  ) ?? false)  partOfSpeech =  hüüdsõna;
                else if (partOfSpeechValue?.Contains(pärisnimi  ) ?? false)  partOfSpeech =  pärisnimi;
                else if (partOfSpeechValue is null)  partOfSpeech =  null;
                else throw new NotImplementedException();
            }

            var cardData = new CardData{
                TextWeAskedFor = common.textWeAskedFor,
                RequestedWord = common.requestedWord,
                Tags = common.tags,
                Examples = examples,
                Translations = translations,
                myWordClass = wordClass,
                VariableCardData = variableCardData,
                PartOfSpeech = partOfSpeech
            };

            yield return cardData;
        }
    }


    internal CardData(
        string textWeAskedFor,
        IEnumerable<string> tagsWeWant,
        SonapiResponse? response,
        string intraFieldSeparator,
        int translationCount, //<< <0 means all
        int meaningsToConsider,
        uint maxExampleCount
    ) {
        TextWeAskedFor = textWeAskedFor;
        RequestedWord = response?.RequestedWord;
        var searchResult = response?.SearchResults?.FirstOrDefault();

        var wordClasses = response
            ?.SearchResults?.FirstOrDefault() //TODO: such first or default which is not null???
            ?.WordClasses?.Where(wc => wc is not null)
            ??[];
        myWordClass = wordClasses?.FirstOrDefault();
        VariableCardData = myWordClass switch {
            WordClass.noomen => new NoomenCardData(searchResult, intraFieldSeparator),
            WordClass.verb => new VerbCardData(searchResult, intraFieldSeparator),
            WordClass.muutumatu => new MuutumatuCardData(searchResult, intraFieldSeparator),
            null => null,
            _ => throw new NotImplementedException(),
        };

        { //== translations
            const int takeAll = -1; //<< -1 because we want all and then we limit when merging.
            var outers = response?.OuterEngTranslations(takeAll)?.ToList() ?? []; 
            var inners = response?.SearchResults?.FirstOrDefault()?.InnerEngTranslations(takeAll)?.ToList() ?? [];
            Translations = MergedEngTranslations(outers, inners, translationCount)?.StringJoin(intraFieldSeparator) ?? "";
        }

        //>> examples
        //const int meaningsToConsider = 3;
        //const int maxExampleCount = 4;
        Examples = response
            ?.SearchResults
            ?.FirstOrDefault()
            ?.ExamplesBalancedGroupedPerMeanings(maxExampleCount, meaningsToConsider)?.Distinct()?.StringJoin("<br>") ?? "";

        const string generatedTag = "generated";
        Tags = generatedTag + " " + tagsWeWant.Select(tag => $"{generatedTag}::{tag}").StringJoin(" ");

        var partOfSpeechValue = response
            ?.SearchResults?.FirstOrDefault()
            ?.Meanings?.FirstOrDefault()
            ?.LexicalCategories?.FirstOrDefault()
            ?.Value;

        if      (partOfSpeechValue?.Contains(asesõna   ) ?? false)  PartOfSpeech =   asesõna;
        else if (partOfSpeechValue?.Contains(määrsõna  ) ?? false)  PartOfSpeech =  määrsõna;
        else if (partOfSpeechValue?.Contains(nimisõna  ) ?? false)  PartOfSpeech =  nimisõna;
        else if (partOfSpeechValue?.Contains(omadussõna) ?? false)  PartOfSpeech = omadussõna;
        else if (partOfSpeechValue?.Contains(sidesõna  ) ?? false)  PartOfSpeech =  sidesõna;
        else if (partOfSpeechValue?.Contains(tegusõna  ) ?? false)  PartOfSpeech =  tegusõna;
        else if (partOfSpeechValue?.Contains(arvsõna   ) ?? false)  PartOfSpeech =   arvsõna;
        else if (partOfSpeechValue?.Contains(tagasõna  ) ?? false)  PartOfSpeech =  tagasõna;
        else if (partOfSpeechValue?.Contains(eessõna   ) ?? false)  PartOfSpeech =   eessõna;
        else if (partOfSpeechValue?.Contains(hüüdsõna  ) ?? false)  PartOfSpeech =  hüüdsõna;
        else if (partOfSpeechValue is null)  PartOfSpeech =  null;
        else throw new NotImplementedException();
    }


    /// <summary>
    /// Postcondition: distinctness ensured.
    /// to be used in conjunction with <see cref="SearchResult.InnerEngTranslations(int)"/> and <see cref="SonapiResponse.OuterEngTranslations(int)"/>
    /// </summary>
    /// <param name="countPerDef"></param>
    /// <returns></returns>
    public static IEnumerable<string>? MergedEngTranslations(List<string> outers, List<string> inners, int countPerDef) {
        // Example of how to construct inputs:
        //List<string> outers = OuterEngTranslations(/*countPerDef*/ (-1)/*i.e. all*/)?.ToList() ?? [];
        //List<string> inners = InnerEngTranslations(/*countPerDef*/ (-1)/*i.e. all*/)?.ToList() ?? [];

        if (outers.Count == 0) return inners.Take(countPerDef);
        if (inners.Count == 0) return outers.Take(countPerDef);
        if (outers.Count == 0 && inners.Count == 0) return inners; //<< Irrelevant which, but let's avoid creating a new empty instance.

        Dictionary<string, int> wordToWeight = new();

        // Assuming that it is ensured that values of `outers` and `inners` are distinct.
        foreach(var outer in outers) {
            wordToWeight.Add(outer, 0);
        }

        foreach(var inner in inners) {
            if ( wordToWeight.ContainsKey(inner) ) {
                wordToWeight[inner]++;
            } else {
                wordToWeight.Add(inner, 0);
            }
        }

        var translations = wordToWeight
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => kvp.Key);

        return countPerDef switch {
            < 0 => translations,
            _ => translations?.Take( countPerDef)
        };
    }


    internal string ToAnkiRow(string interFieldSeparator) {
        var v = VariableCardData;
        var id = v?.Form1;
        //var partOfSpeech = 

        //>> these are intentionally left blank for now
        var image     = "";
        var audio     = "";
        var frontHint = "";
        var backHint  = "";

        return myWordClass switch {
            not null => 
                new string?[] {
                    id,
                    v?.Form1,
                    v?.Form2,
                    v?.Form3,
                    //partOfSpeech,
                    PartOfSpeech,
                    Translations,
                    Examples,
                    image,
                    audio,
                    frontHint,
                    backHint,
                    Tags,
                }.StringJoin(interFieldSeparator),
            null => $"ERROR FOR: requestedWord = {RequestedWord}; TextWeAskedFor = {TextWeAskedFor}",
        };
    }
}
