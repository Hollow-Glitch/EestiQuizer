using System;
using System.Collections.Generic;
using System.Text;

namespace EestiQuizer.Ekilex;


internal class NormalizedWord(string baseForm, int id) {
    public string BaseForm { get; } = baseForm;
    public int Id { get; } = id;
}
