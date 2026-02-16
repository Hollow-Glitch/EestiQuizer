using EestiQuizer.Ekilex.Endpoints;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.StringComparison;

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
    string apiKey;
    RequestClient client;

    internal Processor(Settings settings) {
        apiKey = settings.EkilexApiKey;
        client = new RequestClient(apiKey);
    }

    List<CardData> LoadWords(IEnumerable<WordToLoad> wordsToLoad, DirectoryInfo? saveFolder = null) {

        try {
            foreach (var wordToLoad in wordsToLoad) {
            }
        } catch (Exception e) {
        }

        throw new NotImplementedException();
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
                !.Select(res => res.wordId).ToList();
        }

        return wordIds;
    }


    internal CardData LoadWord(WordToLoad wordToLoad, int wordId) {
        var potWordDetail = client.WordDetails(wordId);
        if (potWordDetail is null) {
            throw new ArgumentNullException();
        }
        var wordDetail = potWordDetail!;
        var lexeme = wordDetail.lexemes[0];
        //<< I must choose a lexeme and work with only one otherwise I could mix examples of one with the translation of another incompatible one...
        //   Big assumption, the first lexeme is what soneveeb chooses which shall be good enough for me

        IEnumerable<string> translations; {
            const double weightLimit = 0.8;
            const string MEANING_WORD = nameof(MEANING_WORD);
            List<WordDetailsEndpoint.Synonym> synonyms; {
                List<WordDetailsEndpoint.Synonym> _synonyms = lexeme.synonymLangGroups
                    //== group
                        .Where(group => group.lang.Equals("eng", InvariantCultureIgnoreCase) )
                        .SelectMany(langGroup => langGroup.synonyms)
                        .ToList();
                synonyms = _synonyms.Where(synonym => synonym.type.Equals(MEANING_WORD, InvariantCultureIgnoreCase) ).ToList();
                if (synonyms.Count == 0) {
                    synonyms = _synonyms; //<< we reset back in case we now have nothing because then all we had were MEANING_RELs
                }
            }
            translations = synonyms
                //== synonym
                    .Where(synonym => synonym.weight > weightLimit) //<< let's not take shitty words.
                    .OrderBy(synonym => synonym.weight)
                    .SelectMany(synonym => synonym.words)
                //== word
                    //>> doesn't hurt but I think this is redundant since above we are already filtering "eng".
                    .Where(word => word.lang.Equals("eng", InvariantCultureIgnoreCase) )
                    .Select(word => word.wordValue)
                .Distinct();
        }

        string form1 = ""; string form2 = ""; string form3 = ""; string wordClass; {
            var paradigms = lexeme.lexemeWord.paradigms
                .Where(paradigm => paradigm.wordClass is not null).ToList();
            if (paradigms.Count is not 1) throw new NotImplementedException();
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
        var usages = lexeme.usages.Select(usage => usage.value);
        string pos = lexeme.pos.FirstOrDefault()?.code ?? throw new NotImplementedException();
        const string generatedTag = "generated";
        var tags = generatedTag + " " + wordToLoad.Tags.Select(tag => $"{generatedTag}::{tag}").StringJoin(" ");

        return new CardData() {
            RequestedWord = wordToLoad.Word,

            Id = form1,
            Form1 = form1,
            Form2 = form2,
            Form3 = form3,
            WordClass = wordClass,
            PartOfSpeech = pos,
            Translations = translations.StringJoin(", "),
            Examples = usages.StringJoin("<br>"),
            Tags = tags,

            ProficiencyLevel = proficiencyLevel,
        };
    }
}
