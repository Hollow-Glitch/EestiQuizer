using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
}
