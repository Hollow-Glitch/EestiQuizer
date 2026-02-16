// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0037:Use inferred member name",
    Justification = "If I want to specify explicitly why warn me even if equals the same, probably I am doing it for formatting reasons or something, nonsense that it annoys me.",
    Scope = "member",
    Target = "~M:EestiQuizer.CardData.Load(System.String,System.Collections.Generic.IEnumerable{System.String},EestiQuizer.SonapiResponse,System.String,System.Int32,System.Int32,System.UInt32)~EestiQuizer.CardData[]"
)]
