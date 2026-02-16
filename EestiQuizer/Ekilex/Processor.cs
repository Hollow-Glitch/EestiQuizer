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

        // 6) Find translations
        // 	/api/meaning/search/{word}
        // 		$.results[*].meaningWords[*].wordValue
        // 		$.results[*].meaningWords[*].lang
        /*
        var translations = client.MeaningSearch(wordToLoad.Word)
            !.results.SelectMany(result => result.meaningWords)
            .Where(meaningWord => meaningWord.lang.Equals("eng") )
            .Select(meaningWord => meaningWord.wordValue)
            .Distinct();
        *///<< bullshit, can use the details info directly tied to the word's wordId
        //  /api/word/details/{wordId}
        //      $.lexemes[*].synonymLangGroups[*].synonyms[*].words[*].wordValue
        //
        /*
        var translations = wordDetail.lexemes
            .SelectMany(lexeme => lexeme.synonymLangGroups)
            .SelectMany(langGroup => langGroup.synonyms)
            .Where(synonym => synonym.type.Equals("MEANING_WORD", InvariantCultureIgnoreCase) )
            .OrderBy(synonym => synonym.weight)
            .SelectMany(synonym => synonym.words)
            .Where(word => word.lang.Equals("eng", InvariantCultureIgnoreCase) )
            .Select(word => word.wordValue)
            .Distinct();
        */

        // I must choose a lexeme and work with only one otherwise I could mix examples of one with the translation of another incompatible one...
        //>> big assumption, the first lexeme is what soneveeb chooses which shall be good enough for me
        var lexeme = wordDetail.lexemes[0];
        var weightLimit = 0.8;

        //>> works but MEANING_REL gives me a headache but sometimes they are needed...
        /* 
        var translations = lexeme.synonymLangGroups
            //== group
                .Where(group => group.lang.Equals("eng", InvariantCultureIgnoreCase) )
                .SelectMany(langGroup => langGroup.synonyms)
            //== synonym
                //>> simplifying, I don't want to deal with relations... since they are not really explained anywhere.
                //.Where(synonym => synonym.type.Equals("MEANING_WORD", InvariantCultureIgnoreCase) )
                .Where(synonym => synonym.weight > weightLimit) //<< let's not take shitty words.
                .OrderBy(synonym => synonym.weight)
                .SelectMany(synonym => synonym.words)
            //== word
                //>> doesn't hurt but I think this is redundant since above we are already filtering "eng".
                .Where(word => word.lang.Equals("eng", InvariantCultureIgnoreCase) )
                .Select(word => word.wordValue)
            .Distinct();
        */
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
        var translations = synonyms
            //== synonym
                //>> simplifying, I don't want to deal with relations... since they are not really explained anywhere.
                //.Where(synonym => synonym.type.Equals("MEANING_WORD", InvariantCultureIgnoreCase) )
                .Where(synonym => synonym.weight > weightLimit) //<< let's not take shitty words.
                .OrderBy(synonym => synonym.weight)
                .SelectMany(synonym => synonym.words)
            //== word
                //>> doesn't hurt but I think this is redundant since above we are already filtering "eng".
                .Where(word => word.lang.Equals("eng", InvariantCultureIgnoreCase) )
                .Select(word => word.wordValue)
            .Distinct();

        // 3) Example
        //     /api/word/details/{wordId}
        //         $lexemes[*].usages[*].value

        // 5) Find paradigms ... Paradigm = inflectional form.
        // 	/api/word/details/{wordId}
        // 		$word.paradigms[*].forms[*].value

        // 9) Level of proficiency
        // 	api/word/details/{wordId}
        // 		$.lexemes[*].lexemeProficiencyLevelCode

        // word classes
        // 	/api/word/details/{wordId}
        // 		$.word.paradigms[*].wordClass

        // find part of speech  ... pos
        //  /api/word/details/{wordId}
        //      $.lexemes[*].pos[*].value
        return new CardData() {
            Translations = translations.StringJoin("| "),
        };
    }
}
