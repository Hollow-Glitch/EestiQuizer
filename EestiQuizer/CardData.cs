using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EestiQuizer; 


internal interface ICardDataFragment {
    [Obsolete(
        $"Part of initial design, but now that we are trying to supply data " +
        $"for only 1 universal anki note type, replaced with: {nameof(Form1)}, {nameof(Form2)}, {nameof(Form3)}.")
    ]
    internal string ToAnkiRowFragment(string interFieldSeparator);

    internal string Form1 { get; }
    internal string Form2 { get; }
    internal string Form3 { get; }
}


internal class NoomenCardData : ICardDataFragment{
    internal string sgN;
    internal string sgG;
    internal string sgP;

    internal NoomenCardData(SonapiResponse? response, string intraFieldSeparator) {
        sgN = response?.WordFormValues(WordFormCode.noomen_SgN)?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
        sgG = response?.WordFormValues(WordFormCode.noomen_SgG)?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
        sgP = response?.WordFormValues(WordFormCode.noomen_SgP)?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
    }

    string ICardDataFragment.Form1 => sgN;
    string ICardDataFragment.Form2 => sgG;
    string ICardDataFragment.Form3 => sgP;

    string ICardDataFragment.ToAnkiRowFragment(string interFieldSeparator) {
        var s = interFieldSeparator;
        return $"{sgN}{s}{sgG}{s}{sgP}";
    }
}


internal class VerbCardData : ICardDataFragment{
    internal string ma  ;
    internal string da  ;
    internal string sg1p;

    internal VerbCardData(SonapiResponse? response, string intraFieldSeparator) {
        ma   = response?.WordFormValues(WordFormCode.verb_Ma  )?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
        da   = response?.WordFormValues(WordFormCode.verb_Da  )?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
        sg1p = response?.WordFormValues(WordFormCode.verb_Sg1P)?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
    }

    string ICardDataFragment.Form1 => ma;
    string ICardDataFragment.Form2 => da;
    string ICardDataFragment.Form3 => sg1p;

    string ICardDataFragment.ToAnkiRowFragment(string interFieldSeparator) {
        var s = interFieldSeparator;
        return $"{ma}{s}{da}{s}{sg1p}";
    }
}


internal class MuutumatuCardData : ICardDataFragment{
    internal string form;

    internal MuutumatuCardData(SonapiResponse? response, string intraFieldSeparator) {
        form = response?.WordFormValues(WordFormCode.muutumatu_ID)?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";
    }

    string ICardDataFragment.ToAnkiRowFragment(string interFieldSeparator) {
        return ToAnkiRowFragment_v1(interFieldSeparator);
    }

    string ICardDataFragment.Form1 => form;
    string ICardDataFragment.Form2 => "";
    string ICardDataFragment.Form3 => "";

    string ToAnkiRowFragment_v0(string interFieldSeparator) {
        var s = interFieldSeparator;
        return $"{form}";
    }

    string ToAnkiRowFragment_v1(string interFieldSeparator) {
        var s = interFieldSeparator;
        return $"{form}{s}{s}";
    }
}


internal class CardData {
    internal WordClass? myWordClass { get; set; }
    internal ICardDataFragment? VariableCardData { get; set; }

    public string TextWeAskedFor { get; }
    internal string? RequestedWord { get; set; }

    internal string? PartOfSpeech { get; set; }
    internal string Translations { get; set; }
    internal string Examples { get; set; }
    internal string Tags { get; set; }

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

        var wordClasses = response
            ?.SearchResults?.FirstOrDefault() //TODO: such first or default which is not null???
            ?.WordClasses?.Where(wc => wc is not null)
            ??[];
        myWordClass = wordClasses?.FirstOrDefault();
        VariableCardData = myWordClass switch {
            WordClass.noomen => new NoomenCardData(response, intraFieldSeparator),
            WordClass.verb => new VerbCardData(response, intraFieldSeparator),
            WordClass.muutumatu => new MuutumatuCardData(response, intraFieldSeparator),
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
        Examples = response?.ExamplesBalancedGroupedPerMeanings(maxExampleCount, meaningsToConsider)?.Distinct()?.StringJoin("<br>") ?? "";

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
    public IEnumerable<string>? MergedEngTranslations(List<string> outers, List<string> inners, int countPerDef) {
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
