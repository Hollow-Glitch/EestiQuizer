using System;
using System.Collections.Generic;

namespace EestiQuizer.Ekilex;

/*
// Root response object
public class WordDetailsResponse
{
    public Word? word { get; set; }
    public Lexeme[]? lexemes { get; set; }

    //>> ignoring these for now since I don't need them for generating ANKI cards yet.
    //public WordRelationDetail[]? wordRelationDetails { get; set; }
    //public int? firstDefinitionValue { get; set; }
    //public bool activeTagComplete { get; set; }
}

public class Word
{
    public int wordId { get; set; }
    public string? wordValue { get; set; }
    public string? wordValuePrese { get; set; }
    public int homonymNr { get; set; }
    public string? lang { get; set; }
    public string? morphophonoForm { get; set; }
    public bool prefixoid { get; set; }
    public bool suffixoid { get; set; }
    public bool foreign { get; set; }
    public string[]? lexemesTagNames { get; set; }
    public string[]? datasetCodes { get; set; }
    public Etymology[]? etymology { get; set; }
    public Paradigm[]? paradigms { get; set; }
    public WordOsMorph? wordOsMorph { get; set; }
    public DateTime lastActivityEventOn { get; set; }
    public DateTime manualEventOn { get; set; }
    public bool wordPublic { get; set; }
    public bool @public { get; set; }
    public bool wwUnif { get; set; }
    public bool wwLite { get; set; }
    public bool wwOs { get; set; }
}

public class Etymology
{
    public int wordEtymId { get; set; }
    public string? etymologyTypeCode { get; set; }
    public int? etymologyYear { get; set; }
    public string? comment { get; set; }
    public bool questionable { get; set; }
    public object[]? wordEtymSourceLinks { get; set; }
    public object[]? wordEtymRelations { get; set; }
}

public class Paradigm
{
    public int paradigmId { get; set; }
    public string? comment { get; set; }
    public string? inflectionType { get; set; }
    public string? inflectionTypeNr { get; set; }
    public string? wordClass { get; set; }
    public Form[]? forms { get; set; }
    public bool formsExist { get; set; }
}

public class Form
{
    public int id { get; set; }
    public string? value { get; set; }
    public string? valuePrese { get; set; }
    public object? components { get; set; }
    public string? displayForm { get; set; }
    public string? morphCode { get; set; }
    public string? morphValue { get; set; }
    public object? morphFrequency { get; set; }
    public object? formFrequency { get; set; }
}

public class WordOsMorph
{
    public string? createdBy { get; set; }
    public DateTime createdOn { get; set; }
    public string? modifiedBy { get; set; }
    public DateTime modifiedOn { get; set; }
    public int id { get; set; }
    public int wordId { get; set; }
    public string? value { get; set; }
    public string? valuePrese { get; set; }
    public bool @public { get; set; }
    public bool wwUnif { get; set; }
    public bool wwLite { get; set; }
    public bool wwOs { get; set; }
}

// Lexeme and related classes
public class Lexeme
{
    public Word? lexemeWord { get; set; }
    public Meaning? meaning { get; set; }
    public int lexemeId { get; set; }
    public int wordId { get; set; }
    public int meaningId { get; set; }
    public string? datasetCode { get; set; }
    public string? datasetName { get; set; }
    public int level1 { get; set; }
    public int level2 { get; set; }
    public string? levels { get; set; }
    public string? lexemeValueStateCode { get; set; }
    public string? lexemeValueState { get; set; }
    public string? lexemeProficiencyLevelCode { get; set; }
    public CodeValue? lexemeProficiencyLevel { get; set; }
    public object? reliability { get; set; }
    public double weight { get; set; }
    public int orderBy { get; set; }
    public string[]? tags { get; set; }
    public CodeValue[]? pos { get; set; }
    public object? derivs { get; set; }
    public object? registers { get; set; }
    public object? regions { get; set; }
    public object[]? governments { get; set; }
    public object[]? grammars { get; set; }
    public Usage[]? usages { get; set; }
    public object[]? freeforms { get; set; }
    public object[]? notes { get; set; }
    public object[]? noteLangGroups { get; set; }
    public object[]? lexemeRelations { get; set; }
    public PrimaryCollocation[]? primaryCollocations { get; set; }
    public SecondaryCollocation[]? secondaryCollocations { get; set; }
    public object[]? collocationMembers { get; set; }
    public CollocationMemberMeaning[]? collocationMemberMeanings { get; set; }
    public object[]? sourceLinks { get; set; }
    public object? meaningWords { get; set; }
    public SynonymLangGroup[]? synonymLangGroups { get; set; }
    public object? tags2 { get; set; }
    public bool activeTagComplete { get; set; }
}

public class CodeValue
{
    public string? name { get; set; }
    public string? code { get; set; }
    public string? value { get; set; }
}

public class Meaning
{
    public int meaningId { get; set; }
    public object? firstWordValue { get; set; }
    public object? lexemeIds { get; set; }
    public Definition[]? definitions { get; set; }
    public DefinitionLangGroup[]? definitionLangGroups { get; set; }
    public object? lexemes { get; set; }
    public object? lexemeLangGroups { get; set; }
    public object? lexemeDatasetCodes { get; set; }
    public object[]? domains { get; set; }
    public CodeValue[]? semanticTypes { get; set; }
    public Freeform[]? freeforms { get; set; }
    public object[]? learnerComments { get; set; }
    public Image[]? images { get; set; }
    public object[]? medias { get; set; }
    public object[]? forums { get; set; }
    public object[]? noteLangGroups { get; set; }
    public object[]? relations { get; set; }
    public object[]? viewRelations { get; set; }
    public object? synonymLangGroups { get; set; }
    public object? tags { get; set; }
    public bool activeTagComplete { get; set; }
    public object? lastActivityEventOn { get; set; }
    public object? lastApproveEventOn { get; set; }
    public object? manualEventOn { get; set; }
    public bool @public { get; set; }
    public bool wwUnif { get; set; }
    public bool wwLite { get; set; }
    public bool wwOs { get; set; }
}

public class Definition
{
    public int id { get; set; }
    public string? value { get; set; }
    public string? valuePrese { get; set; }
    public string? lang { get; set; }
    public int orderBy { get; set; }
    public string? typeCode { get; set; }
    public string? typeValue { get; set; }
    public string[]? datasetCodes { get; set; }
    public object? notes { get; set; }
    public object? sourceLinks { get; set; }
    public bool editDisabled { get; set; }
    public bool @public { get; set; }
    public bool wwUnif { get; set; }
    public bool wwLite { get; set; }
    public bool wwOs { get; set; }
}

public class DefinitionLangGroup
{
    public string? lang { get; set; }
    public bool selected { get; set; }
    public Definition[]? definitions { get; set; }
}

public class Freeform
{
    public object? createdBy { get; set; }
    public object? createdOn { get; set; }
    public object? modifiedBy { get; set; }
    public object? modifiedOn { get; set; }
    public int id { get; set; }
    public object? parentId { get; set; }
    public string? freeformTypeCode { get; set; }
    public string? freeformTypeValue { get; set; }
    public string? value { get; set; }
    public string? valuePrese { get; set; }
    public object? lang { get; set; }
    public int orderBy { get; set; }
    public object? sourceLinks { get; set; }
    public bool @public { get; set; }
    public bool wwUnif { get; set; }
    public bool wwLite { get; set; }
    public bool wwOs { get; set; }
}

public class Image
{
    public object? createdBy { get; set; }
    public object? createdOn { get; set; }
    public object? modifiedBy { get; set; }
    public object? modifiedOn { get; set; }
    public int id { get; set; }
    public object? meaningId { get; set; }
    public object? title { get; set; }
    public string? url { get; set; }
    public object? orderBy { get; set; }
    public object? sourceLinks { get; set; }
    public bool @public { get; set; }
    public bool wwUnif { get; set; }
    public bool wwLite { get; set; }
    public bool wwOs { get; set; }
}

public class Usage
{
    public string? createdBy { get; set; }
    public DateTime createdOn { get; set; }
    public string? modifiedBy { get; set; }
    public DateTime modifiedOn { get; set; }
    public int id { get; set; }
    public string? value { get; set; }
    public string? valuePrese { get; set; }
    public string? lang { get; set; }
    public int orderBy { get; set; }
    public object? translations { get; set; }
    public object? sourceLinks { get; set; }
    public bool @public { get; set; }
    public bool wwUnif { get; set; }
    public bool wwLite { get; set; }
    public bool wwOs { get; set; }
}

// Collocation classes
public class PrimaryCollocation
{
    public string? posGroupCode { get; set; }
    public string? posGroupValue { get; set; }
    public RelGroup[]? relGroups { get; set; }
}

public class SecondaryCollocation
{
    public int lexemeId { get; set; }
    public int wordId { get; set; }
    public string? wordValue { get; set; }
    public object? usages { get; set; }
    public CollocationMember[]? members { get; set; }
    public object? groupOrder { get; set; }
    public int headwordCollocMemberId { get; set; }
}

public class RelGroup
{
    public string? relGroupCode { get; set; }
    public string? relGroupValue { get; set; }
    public Collocation[]? collocations { get; set; }
}

public class Collocation
{
    public int lexemeId { get; set; }
    public int wordId { get; set; }
    public string? wordValue { get; set; }
    public object? usages { get; set; }
    public CollocationMember[]? members { get; set; }
    public int groupOrder { get; set; }
    public int headwordCollocMemberId { get; set; }
}

public class CollocationMember
{
    public int id { get; set; }
    public string? datasetCode { get; set; }
    public int collocLexemeId { get; set; }
    public int memberLexemeId { get; set; }
    public int memberMeaningId { get; set; }
    public int memberWordId { get; set; }
    public string? memberWordValue { get; set; }
    public int homonymNr { get; set; }
    public string? lang { get; set; }
    public int memberFormId { get; set; }
    public string? memberFormValue { get; set; }
    public string? morphCode { get; set; }
    public string? morphValue { get; set; }
    public object? conjunctLexemeId { get; set; }
    public string? conjunctValue { get; set; }
    public string? posGroupCode { get; set; }
    public string? posGroupValue { get; set; }
    public string? relGroupCode { get; set; }
    public string? relGroupValue { get; set; }
    public double weight { get; set; }
    public object? weightLevel { get; set; }
    public int memberOrder { get; set; }
    public object? groupOrder { get; set; }
    public object? definitionValues { get; set; }
}

public class CollocationMemberMeaning
{
    public int wordId { get; set; }
    public int lexemeId { get; set; }
    public int meaningId { get; set; }
    public string[]? definitionValues { get; set; }
    public bool selected { get; set; }
}

public class SynonymLangGroup
{
    public string? lang { get; set; }
    public bool selected { get; set; }
    public Synonym[]? synonyms { get; set; }
    public object[]? inexactSynonyms { get; set; }
}

public class Synonym
{
    public string? type { get; set; }
    public object? meaningId { get; set; }
    public object? relationId { get; set; }
    public SynonymWord[]? words { get; set; }
    public string? wordLang { get; set; }
    public double weight { get; set; }
    public int orderBy { get; set; }
}

public class SynonymWord
{
    public int wordId { get; set; }
    public string? wordValue { get; set; }
    public string? wordValuePrese { get; set; }
    public int homonymNr { get; set; }
    public bool homonymsExist { get; set; }
    public string? lang { get; set; }
    public object? wordTypeCodes { get; set; }
    public bool prefixoid { get; set; }
    public bool suffixoid { get; set; }
    public bool foreign { get; set; }
    public object? lexemeId { get; set; }
    public object? lexemeLevels { get; set; }
    public object? lexemeValueStateCode { get; set; }
    public object? lexemeRegisterCodes { get; set; }
    public bool lexemePublic { get; set; }
}
*/