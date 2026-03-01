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

    /// <summary>
    /// Represents the baseline example/usage sentence length we are looking for,
    /// if not found, then we are looking for shorter and longer sentences
    /// </summary>
    const int sentenceLengthOrigin = 2;

    /// <summary>
    /// Number of usage/example sentences we will add to the CardData.
    /// </summary>
    const int usageSentencesToTake = 10;


    internal Processor(RequestClient client) {
        this.client = client;
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
            .OrderBy(usage => Math.Abs(usage.Split(" ").Length - sentenceLengthOrigin) )
            .Select(usage => usage) // `select` trick so that I can use `?? []` otherwise the usage of `OrderBy` prevents it.
            ?? [];
    }


    internal CardData? LoadWord(WordToLoad wordToLoad, int wordId) {
        var potWordDetail = client.WordDetails(wordId);
        if (potWordDetail is null) {
            throw new ArgumentNullException();
        }
        var wordDetail = potWordDetail!;
        //>> Let's ignore prefixes and sufixes
        if (wordDetail.word?.prefixoid ?? false) return null;
        if (wordDetail.word?.suffixoid ?? false) return null;

        //>> I must choose a lexeme and work with only one otherwise I could mix examples of one with the translation of another incompatible one...
        //   Big assumption, the first lexeme is what soneveeb chooses which shall be good enough for me
        WordDetailsEndpoint.Lexeme lexeme; {
            WordDetailsEndpoint.Lexeme? _lexeme = wordDetail
                ?.lexemes
                ?.Where(l => l.@public ?? false)
                .OrderBy(l => l.datasetCode == "eki" ? 0 : 1) //<< prioritize eki as it is the most authoritative.
                .FirstOrDefault();
            if (_lexeme is null) return null;
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
                if (_synonyms.Count == 0) return null; //<< in case we have zero then we can't translate, then it is meaningless to continue.
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
            if (translations.Count == 0) return null; //<< if we don't have any translations then there is nothing to learn hence meaningless to continue.
        }

        string form1 = ""; string form2 = ""; string form3 = ""; string wordClass; {
            var paradigms = wordDetail?.word?.paradigms
                ?.Where(paradigm => paradigm.wordClass is not null).ToList()
                ?? [];
            if (paradigms.Count == 0) return null;
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

        var proficiencyLevel = lexeme.lexemeProficiencyLevelCode ?? "";
        //var usages = lexeme.usages?.Select(usage => usage.value) ?? [];
        var usages = CollectUsages(lexeme.usages);
        //string pos = lexeme.pos?.FirstOrDefault()?.code ?? throw new NotImplementedException();
        string pos = lexeme.pos?.FirstOrDefault()?.code ?? ""; //TODO check this

        string tags; {
            // We are prepending everything here so that in Anki all these tags are cleanly subtags of the generated tag.
            // So even if I make a mistake while creating a file, or adding a tag which could complicate operations in Anki,
            // it won't be a problem.
            const string generatedTag = "generated";
            IEnumerable<string> tempTagList =
                wordToLoad.Tags
                .Append(proficiencyLevel);
            tags = generatedTag + " " + tempTagList.Select(tag => $"{generatedTag}::{tag}").StringJoin(" ");
        }

        List<string> imageNames; { 
            imageNames = lexeme.meaning?.images
                ?.Where(i => i.url is not null)
                .Select(i => 
                    i.url.Split("/").Last() //TODO: probably we need handling of words with spaces which in url have the funny characters.
                )
                .ToList()
                ?? [];

            foreach(var url in imageNames) {
                client.DownloadImage(url); 
            }
        }

        return new CardData() {
            RequestedWord = wordToLoad.Word,

            Id = form1,
            Form1 = form1,
            Form2 = form2,
            Form3 = form3,
            WordClass = wordClass,
            PartOfSpeech = pos,
            Translations = translations.StringJoin(", "),
            Examples = usages.Take(usageSentencesToTake).StringJoin("<br>"),
            Tags = tags,

            ProficiencyLevel = proficiencyLevel,
            ImageNamesInCache = imageNames,
        };
    }
}
