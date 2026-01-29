using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EestiQuizer; 


internal interface ICardDataFragment {
    internal string ToAnkiRowFragment(string interFieldSeparator);
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
        var s = interFieldSeparator;
        return $"{form}";
    }
}


internal class CardData {
    internal WordClass? myWordClass { get; set; }
    internal ICardDataFragment? VariableCardData { get; set; }

    //public string WordIAskedFor { get; }
    internal string? RequestedWord { get; set; }

    internal string Translations { get; set; }
    internal string Examples { get; set; }
    internal string Tags { get; set; }

    internal CardData(
        //string wordIAskedFor,
        SonapiResponse? response,
        string intraFieldSeparator,
        uint translationCount,
        int meaningsToConsider,
        uint maxExampleCount
    ) {
        // RequestedWord = response
        //     ?.RequestedWord ?? throw new ArgumentNullException("RequestedWord was null in response.");
        // //<< argument is that since we are sending something it may never happen that it would be null.
        //
        //<< was causing a fucking bug due to null or whatever...

        // WordIAskedFor = wordIAskedFor;
        //
        //<< can't even do this now because in the caller it is overcomplicated so can't just trivially pass arround the word, would have to restructure.
        //   Let's try again the other way
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

        Translations = response?.SipmleEngTranslations(translationCount)?.Distinct()?.StringJoin(intraFieldSeparator) ?? "";

        //>> examples
        //const int meaningsToConsider = 3;
        //const int maxExampleCount = 4;
        Examples = response?.ExamplesBalancedGroupedPerMeanings(maxExampleCount, meaningsToConsider)?.Distinct()?.StringJoin("<br>") ?? "";

        Tags = "generated";
    }

    internal string ToAnkiRow(string interFieldSeparator) {
        var s = interFieldSeparator;
        return myWordClass switch {
            not null => $"{VariableCardData?.ToAnkiRowFragment(interFieldSeparator)}{s}{Translations}{s}{Examples}{s}{Tags}",
            null => $"ERROR FOR: {RequestedWord}",
        };
    }
}
