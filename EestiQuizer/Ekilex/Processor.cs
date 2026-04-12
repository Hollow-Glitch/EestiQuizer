using System.IO;
using EestiQuizer.Ekilex.Endpoints;

using static System.StringComparison;
using EestiQuizer.Common;


namespace EestiQuizer.Ekilex; 

/* 
 * 
 * (Endpoints) --[json classes]--> (Processor) --[CardData]--> <CardDatas>
 * ... not a call graph, A -> B doesn't mean that A calls B or B calls A, rather means that the data of A goes into B
 * (x) ... x is a logic class
 * [x] ... x is data  
 * <x> ... x here is a list of the things fed into it
 * 
 * Endpoints ... model the endpoints and convert to class representation
 * Processor ... collects the data and creates a CardData from it
 * CardData  ... representation that implements the to Anki format functionality and allows DataGrid visualization
 *
 */

internal class Processor {
    RequestClient client;
    Settings settings;

    internal Processor(RequestClient client, Settings settings) {
        this.client = client;
        this.settings = settings;
    }


    internal List<int> DetermineWordIds(string word) {
        var wordSearch = client.WordSearch(word);

        List<int> wordIds;
        if (wordSearch is null) {
            var formSearch = client.FormSearch(word);
            wordIds = formSearch! //<< dunno how things will work out, so for now I will assume whatever.
                .Select(res => res.wordId)
                .ToList();
        } else {
            wordIds = wordSearch.words
                ?.Where(res => res.lang?.Equals("est", InvariantCultureIgnoreCase) ?? false)
                ?.Select(res => res.wordId).ToList()
                ?? []; // in case we feed in a phrase and not a word then it can happen that we don't find an id. Thus empty here.
        }

        return wordIds;
    }


    IEnumerable<string> CollectUsages(List<WordDetailsEndpoint.Usage>? usages) =>
        CollectUsagesFancy(usages);


    IEnumerable<string> CollectUsagesSimple(List<WordDetailsEndpoint.Usage>? usages) {
        return usages
            ?.Where(usage => usage.@public is true)
            ?.Select(usage => usage.value)
            ?? [];
    }


    /// <summary>
    /// f(x) = ABS(x - o)
    /// With this the ordering starts from `o` and sentences which are `y` shorter or longer are projected to be `y` bigger and thus taken later.
    /// Of course the `abs` is doing the "merge" here.
    /// </summary>
    /// <param name="usages"></param>
    /// <returns></returns>
    IEnumerable<string> CollectUsagesFancy(List<WordDetailsEndpoint.Usage>? usages) {
        //return (usages?.Select(usage => usage.value) ?? [])
        return usages
            ?.Where(usage => usage.@public is true)
            .Select(usage => usage.value)
            .OrderBy(usage => Math.Abs(usage.Split(" ").Length - settings.SentenceLengthOrigin) )
            .Select(usage => usage) // `select` trick so that I can use `?? []` otherwise the usage of `OrderBy` prevents it.
            ?? [];
    }


    string PosToName(string posCode) {
        return posCode switch {
            "adj"    => "omadussõna", //<< double "s"
            "prep"   => "eessõna",    //<< double "s"
            "postp"  => "tagasõna",
            "s"      => "nimisõna",
            "v"      => "tegusõna",
            "konj"   => "sidesõna",
            "num"    => "marvsõna",
            "adv"    => "määrsõna",
            "prop"   => "pärisnimi",
            "pron"   => "asesõna",
            "interj" => "hüüdsõna",
            ""       => "! MISSING !",
            _ => throw new NotImplementedException()
        };
    }


    /// <summary>
    /// In case `cardData` is null, then `reason` contains the reason why it is null.
    /// This is to ensure that in UI we can communicate this with user.
    /// </summary>
    internal struct LoadWordResult {
        internal LoadWordResult(CardData? cardData, string? reason) {
            this.cardData = cardData;
            this.reason = reason;
        }
        internal CardData? cardData;
        internal string? reason;
    }
    internal LoadWordResult LoadWord(WordToLoad wordToLoad, int wordId) {
        var potWordDetail = client.WordDetails(wordId);
        if (potWordDetail is null) {
            throw new ArgumentNullException();
        }
        var wordDetail = potWordDetail!;
        //>> Let's ignore prefixes and sufixes
        if (wordDetail.word?.prefixoid ?? false) return new LoadWordResult(null, "It is a prefixoid, hence ignoring.");
        if (wordDetail.word?.suffixoid ?? false) return new LoadWordResult(null, "it is a suffixoid, hence ignoring.");

        //>> I must choose a lexeme and work with only one otherwise I could mix examples of one with the translation of another incompatible one...
        //   Big assumption, the first lexeme is what soneveeb chooses which shall be good enough for me
        WordDetailsEndpoint.Lexeme lexeme; {
            WordDetailsEndpoint.Lexeme? _lexeme = wordDetail
                ?.lexemes
                ?.Where(l => l.@public ?? false)
                .OrderBy(l => l.datasetCode == "eki" ? 0 : 1) //<< prioritize eki as it is the most authoritative.
                .FirstOrDefault();
            if (_lexeme is null) return new LoadWordResult(null, "No lexemes sufficing criteria found.");
            lexeme = _lexeme!;
        }

        List<string> translations; {
            const string MEANING_WORD = nameof(MEANING_WORD);
            List<WordDetailsEndpoint.Synonym> synonyms; {
                List<WordDetailsEndpoint.Synonym> _synonyms = lexeme.synonymLangGroups
                    //== group
                        ?.Where(group => group.lang.Equals("eng", InvariantCultureIgnoreCase) )
                        .SelectMany(langGroup => langGroup.synonyms)
                        .ToList()
                        ?? [];

                if (_synonyms.Count == 0) return new LoadWordResult(null, "No synonyms found.");
                //<< in case we have zero then we can't translate, then it is meaningless to continue.

                synonyms = _synonyms.Where(synonym => synonym.type?.Equals(MEANING_WORD, InvariantCultureIgnoreCase) ?? false ).ToList();
                if (synonyms.Count == 0) {
                    synonyms = _synonyms; //<< we reset back in case we now have nothing because then all we had were MEANING_RELs
                }
            }
            translations = synonyms
                //== synonym
                    //.Where(synonym => synonym.weight > weightLimit)
                        //<< let's not take shitty words.
                        //<< problem, because removes sometimes the only viable options...
                    .OrderBy(synonym => synonym.weight)
                    .SelectMany(synonym => synonym.words)
                //== word
                    //>> doesn't hurt but I think this is redundant since above we are already filtering "eng".
                    .Where(word => word.lang.Equals("eng", InvariantCultureIgnoreCase) )
                    .Where(word => word.lexemePublic ?? false) //<< if it is not public we don't want it, sonaveeb doesn't show these it seems.
                    .Select(word => word.wordValue)
                    .Distinct()
                    .Take(4)
                    .ToList();
            if (translations.Count == 0) return new LoadWordResult(null, "No translations found.");
            //<< if we don't have any translations then there is nothing to learn hence meaningless to continue.
        }

        string form1 = ""; string form2 = ""; string form3 = "";
        string form4 = ""; string form5 = ""; string form6 = "";
        string wordClass; {
            var paradigms = wordDetail?.word?.paradigms
                ?.Where(paradigm => paradigm.wordClass is not null).ToList()
                ?? [];
            if (paradigms.Count == 0) return new LoadWordResult(null, "No paradigms found.");
            //if (paradigms.Count is not 1) throw new NotImplementedException(); //TODO: deal with this
            var paradigm = paradigms[0];
            wordClass = paradigm.wordClass; // one of: muutumatu, noomen, verb
            if (wordClass.Equals("noomen", InvariantCultureIgnoreCase) ) {
                const string SgN = nameof(SgN);
                const string SgG = nameof(SgG);
                const string SgP = nameof(SgP);
                form1 = paradigm.forms.Where(form => form.morphCode.Equals(SgN) ).Select(form => form.value).Distinct().StringJoin(", ");
                form2 = paradigm.forms.Where(form => form.morphCode.Equals(SgG) ).Select(form => form.value).Distinct().StringJoin(", ");
                form3 = paradigm.forms.Where(form => form.morphCode.Equals(SgP) ).Select(form => form.value).Distinct().StringJoin(", ");

                const string PlN = nameof(PlN);
                const string PlG = nameof(PlG);
                const string PlP = nameof(PlP);
                form4 = paradigm.forms.Where(form => form.morphCode.Equals(PlN) ).Select(form => form.value).Distinct().StringJoin(", ");
                form5 = paradigm.forms.Where(form => form.morphCode.Equals(PlG) ).Select(form => form.value).Distinct().StringJoin(", ");
                form6 = paradigm.forms.Where(form => form.morphCode.Equals(PlP) ).Select(form => form.value).Distinct().StringJoin(", ");
            } else 
            if (wordClass.Equals("verb", InvariantCultureIgnoreCase) ) {
                const string IndPrSg1 = nameof(IndPrSg1);
                const string Sup = nameof(Sup);
                const string Inf = nameof(Inf);
                form1 = paradigm.forms.Where(form => form.morphCode.Equals(Sup)      ).Select(form => form.value).Distinct().StringJoin(", ");
                form2 = paradigm.forms.Where(form => form.morphCode.Equals(Inf)      ).Select(form => form.value).Distinct().StringJoin(", ");
                form3 = paradigm.forms.Where(form => form.morphCode.Equals(IndPrSg1) ).Select(form => form.value).Distinct().StringJoin(", ");
            } else
            if (wordClass.Equals("muutumatu", InvariantCultureIgnoreCase) ) {
                const string ID = nameof(ID);
                form1 = paradigm.forms.Where(form => form.morphCode.Equals(ID) ).Select(form => form.value).Distinct().StringJoin(", ");
            } else {
                throw new NotImplementedException();
            }
        }

        var proficiencyLevel = lexeme.lexemeProficiencyLevelCode ?? "none";
        //var usages = lexeme.usages?.Select(usage => usage.value) ?? [];
        var usages = CollectUsages(lexeme.usages);

        //== Part of speech
        //string pos = (lexeme.pos?.FirstOrDefault()?.code ?? "") + " " + (lexeme.pos?.FirstOrDefault()?.value ?? "");
        //<< in case something new pops-up comment below and uncomment this to see what it is.
        var posCode = lexeme.pos?.FirstOrDefault()?.code ?? "";
        string pos = PosToName(posCode);
        if (posCode.Equals("v") ) {
            // English translations for verbs are missing the "to"
            // word here we add it back if the estonian word is a verb, hopefully this is a sufficient check.
            // Modal english verbs shouldn't have the "to" but I guess I will fix it by hand when I discover them.
            for (int i = 0; i < translations.Count; i++) {
                translations[i] = "to " + translations[i];
            }
        }

        string tags; {
            // We are prepending everything here so that in Anki all these tags are cleanly subtags of the generated tag.
            // So even if I make a mistake while creating a file, or adding a tag which could complicate operations in Anki,
            // it won't be a problem.
            const string generatedTag = "generated";
            IEnumerable<string> tempTagList =
                wordToLoad.Tags
                .Append("level::" + proficiencyLevel);
            tags = generatedTag + " " + tempTagList.Select(tag => $"{generatedTag}::{tag}").StringJoin(" ");
        }

        List<string> imageNames = []; { 
            List<string> imageUrls = lexeme.meaning?.images
                ?.Where(i => i.url is not null).Select(i => i.url!)
                .ToList()
                ?? [];

            foreach(var url in imageUrls) {
                var fileName = client.DownloadImage(url); 
                imageNames.Add(fileName);
            }
        }

        var cardData = new CardData() {
            RequestedWord = wordToLoad.Word,

            Id = form1 + " " + wordId,
            Form1 = form1,
            Form2 = form2,
            Form3 = form3,
            Form4 = form4,
            Form5 = form5,
            Form6 = form6,
            WordClass = wordClass,
            PartOfSpeech = pos,
            Translations = translations.StringJoin(", "),
            Examples = usages.Take(settings.UsageSentencesToTake).StringJoin("<br>"),
            Tags = tags,

            ProficiencyLevel = proficiencyLevel,
            ImageNamesInCache = imageNames,
        };
        return new LoadWordResult(cardData, null);
    }
}
