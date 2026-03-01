using EestiQuizer.Common;

namespace EestiQuizer.Ekilex;


public class CardData {
    required public string RequestedWord { get; init; } //<< For debug and bug hunting.

    required public string Id { get; init; }
    required public string Form1 { get; init; }
    required public string Form2 { get; init; }
    required public string Form3 { get; init; }
    required public string WordClass { get; init; }
    required public string PartOfSpeech { get; init; }
    required public string Translations { get; init; }
    required public string Examples { get; init; }
    required public string Tags { get; init; }

    required public string ProficiencyLevel { get; set; }
    required public List<string> ImageNamesInCache { get; set; }


    internal static string Header() =>
        """
        #separator:Pipe
        #columns:id|form1|form2|form3|partOfSpeech|meaning|examples|image|audio|frontHint|backHint|tags
        """;


    internal string ToAnkiRow(string interFieldSeparator) {
        //>> these are intentionally left blank for now
        var images    = ImageNamesInCache.StringJoin(" ");
        var audio     = "";
        var frontHint = "";
        var backHint  = "";

        // !!! Keep this in synch with the above `Header` method.
        var asdf = new string?[] {
            Id,
            Form1,
            Form2,
            Form3,
            // WordClass //<< Let's try not including this, can be derived from `PartOfSpeach` I guess.
            PartOfSpeech,
            Translations,
            Examples,
            images,
            audio,
            frontHint,
            backHint,
            Tags,
        }.StringJoin(interFieldSeparator);

        return asdf;
    }
}
