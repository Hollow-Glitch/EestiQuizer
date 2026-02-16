using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EestiQuizer.Ekilex.Endpoints;


public static class WordDetailsEndpoint {
    extension(RequestClient client) {
        internal Root? WordDetails(int wordId) {
            var url = $"https://ekilex.ee/api/word/details/{wordId}";
            return client.RequestSynch<Root>(url);
        }
    }


    public class Root
    {
        public Word word { get; set; }
        public List<Lexeme> lexemes { get; set; }
        public WordRelationDetails wordRelationDetails { get; set; }
        public object firstDefinitionValue { get; set; }
        public bool? activeTagComplete { get; set; }
    }



    public class Collocation
    {
        public int? lexemeId { get; set; }
        public int? wordId { get; set; }
        public string wordValue { get; set; }
        public List<Usage> usages { get; set; }
        public List<Member> members { get; set; }
        public int? groupOrder { get; set; }
        public int? headwordCollocMemberId { get; set; }
    }

    public class CollocationMemberMeaning
    {
        public int? wordId { get; set; }
        public int? lexemeId { get; set; }
        public int? meaningId { get; set; }
        public List<string> definitionValues { get; set; }
        public bool? selected { get; set; }
    }

    public class Definition
    {
        public int? id { get; set; }
        public string value { get; set; }
        public string valuePrese { get; set; }
        public string lang { get; set; }
        public int? orderBy { get; set; }
        public string typeCode { get; set; }
        public string typeValue { get; set; }
        public List<string> datasetCodes { get; set; }
        public object notes { get; set; }
        public object sourceLinks { get; set; }
        public bool? editDisabled { get; set; }
        public bool? @public { get; set; }
        public bool? wwUnif { get; set; }
        public bool? wwLite { get; set; }
        public bool? wwOs { get; set; }
    }

    public class DefinitionLangGroup
    {
        public string lang { get; set; }
        public bool? selected { get; set; }
        public List<Definition> definitions { get; set; }
    }

    public class Etymology
    {
        public int? wordEtymId { get; set; }
        public object etymologyTypeCode { get; set; }
        public object etymologyYear { get; set; }
        public string comment { get; set; }
        public bool? questionable { get; set; }
        public List<object> wordEtymSourceLinks { get; set; }
        public List<object> wordEtymRelations { get; set; }
    }

    public class Form
    {
        public int? id { get; set; }
        public string value { get; set; }
        public string valuePrese { get; set; }
        public object components { get; set; }
        public string displayForm { get; set; }
        public string morphCode { get; set; }
        public string morphValue { get; set; }
        public object morphFrequency { get; set; }
        public object formFrequency { get; set; }
    }

    public class Freeform
    {
        public object createdBy { get; set; }
        public object createdOn { get; set; }
        public object modifiedBy { get; set; }
        public object modifiedOn { get; set; }
        public int? id { get; set; }
        public object parentId { get; set; }
        public string freeformTypeCode { get; set; }
        public string freeformTypeValue { get; set; }
        public string value { get; set; }
        public string valuePrese { get; set; }
        public object lang { get; set; }
        public int? orderBy { get; set; }
        public object sourceLinks { get; set; }
        public bool? @public { get; set; }
        public bool? wwUnif { get; set; }
        public bool? wwLite { get; set; }
        public bool? wwOs { get; set; }
    }

    public class Image
    {
        public object createdBy { get; set; }
        public object createdOn { get; set; }
        public object modifiedBy { get; set; }
        public object modifiedOn { get; set; }
        public int? id { get; set; }
        public object meaningId { get; set; }
        public object title { get; set; }
        public string url { get; set; }
        public object orderBy { get; set; }
        public object sourceLinks { get; set; }
        public bool? @public { get; set; }
        public bool? wwUnif { get; set; }
        public bool? wwLite { get; set; }
        public bool? wwOs { get; set; }
    }

    public class Lexeme
    {
        public LexemeWord lexemeWord { get; set; }
        public Meaning meaning { get; set; }
        public int? lexemeId { get; set; }
        public int? wordId { get; set; }
        public int? meaningId { get; set; }
        public string datasetCode { get; set; }
        public string datasetName { get; set; }
        public int? level1 { get; set; }
        public int? level2 { get; set; }
        public string levels { get; set; }
        public object lexemeValueStateCode { get; set; }
        public object lexemeValueState { get; set; }
        public string lexemeProficiencyLevelCode { get; set; }
        public LexemeProficiencyLevel lexemeProficiencyLevel { get; set; }
        public object reliability { get; set; }
        public double? weight { get; set; }
        public int? orderBy { get; set; }
        public List<string> tags { get; set; }
        public List<Po> pos { get; set; }
        public object derivs { get; set; }
        public object registers { get; set; }
        public object regions { get; set; }
        public List<object> governments { get; set; }
        public List<object> grammars { get; set; }
        public List<Usage> usages { get; set; }
        public List<object> freeforms { get; set; }
        public List<object> notes { get; set; }
        public List<object> noteLangGroups { get; set; }
        public List<object> lexemeRelations { get; set; }
        public List<PrimaryCollocation> primaryCollocations { get; set; }
        public List<SecondaryCollocation> secondaryCollocations { get; set; }
        public List<object> collocationMembers { get; set; }
        public List<CollocationMemberMeaning> collocationMemberMeanings { get; set; }
        public List<object> sourceLinks { get; set; }
        public object meaningWords { get; set; }
        public List<SynonymLangGroup> synonymLangGroups { get; set; }
        public bool? collocationsExist { get; set; }
        public bool? collocationMemberMeaningCandidacyExist { get; set; }
        public bool? lexemeOrMeaningClassifiersExist { get; set; }
        public bool? classifiersExist { get; set; }
        public bool? word { get; set; }
        public bool? collocation { get; set; }
        public bool? @public { get; set; }
        public bool? wwUnif { get; set; }
        public bool? wwLite { get; set; }
        public bool? wwOs { get; set; }
    }

    public class LexemeProficiencyLevel
    {
        public string name { get; set; }
        public string code { get; set; }
        public string value { get; set; }
    }

    public class LexemeWord
    {
        public int? wordId { get; set; }
        public string wordValue { get; set; }
        public string wordValuePrese { get; set; }
        public int? homonymNr { get; set; }
        public string lang { get; set; }
        public string morphophonoForm { get; set; }
        public bool? prefixoid { get; set; }
        public bool? suffixoid { get; set; }
        public bool? foreign { get; set; }
        public List<string> lexemesTagNames { get; set; }
        public List<string> datasetCodes { get; set; }
        public List<Etymology> etymology { get; set; }
        public List<Paradigm> paradigms { get; set; }
        public WordOsMorph wordOsMorph { get; set; }
        public DateTime? lastActivityEventOn { get; set; }
        public DateTime? manualEventOn { get; set; }
        public bool? wordPublic { get; set; }
        public bool? @public { get; set; }
        public bool? wwUnif { get; set; }
        public bool? wwLite { get; set; }
        public bool? wwOs { get; set; }
    }

    public class Meaning
    {
        public int? meaningId { get; set; }
        public object firstWordValue { get; set; }
        public object lexemeIds { get; set; }
        public List<Definition> definitions { get; set; }
        public List<DefinitionLangGroup> definitionLangGroups { get; set; }
        public object lexemes { get; set; }
        public object lexemeLangGroups { get; set; }
        public object lexemeDatasetCodes { get; set; }
        public List<object> domains { get; set; }
        public List<SemanticType> semanticTypes { get; set; }
        public List<Freeform> freeforms { get; set; }
        public List<object> learnerComments { get; set; }
        public List<Image> images { get; set; }
        public List<object> medias { get; set; }
        public List<object> forums { get; set; }
        public List<object> noteLangGroups { get; set; }
        public List<object> relations { get; set; }
        public List<object> viewRelations { get; set; }
        public object synonymLangGroups { get; set; }
        public object tags { get; set; }
        public bool? activeTagComplete { get; set; }
        public object lastActivityEventOn { get; set; }
        public object lastApproveEventOn { get; set; }
        public object manualEventOn { get; set; }
        public bool? @public { get; set; }
        public bool? wwUnif { get; set; }
        public bool? wwLite { get; set; }
        public bool? wwOs { get; set; }
    }

    public class Member
    {
        public int? id { get; set; }
        public string datasetCode { get; set; }
        public int? collocLexemeId { get; set; }
        public int? memberLexemeId { get; set; }
        public int? memberMeaningId { get; set; }
        public int? memberWordId { get; set; }
        public string memberWordValue { get; set; }
        public int? homonymNr { get; set; }
        public string lang { get; set; }
        public int? memberFormId { get; set; }
        public string memberFormValue { get; set; }
        public string morphCode { get; set; }
        public string morphValue { get; set; }
        public int? conjunctLexemeId { get; set; }
        public string conjunctValue { get; set; }
        public string posGroupCode { get; set; }
        public string posGroupValue { get; set; }
        public string relGroupCode { get; set; }
        public string relGroupValue { get; set; }
        public double? weight { get; set; }
        public object weightLevel { get; set; }
        public int? memberOrder { get; set; }
        public int? groupOrder { get; set; }
        public object definitionValues { get; set; }
        public object lexemeId { get; set; }
        public object meaningId { get; set; }
        public int? wordId { get; set; }
        public string wordValue { get; set; }
        public string wordValuePrese { get; set; }
        public string wordLang { get; set; }
        public object wordHomonymNr { get; set; }
        public object wordAspectCode { get; set; }
        public List<string> wordTypeCodes { get; set; }
        public bool? prefixoid { get; set; }
        public bool? suffixoid { get; set; }
        public bool? foreign { get; set; }
        public bool? homonymsExist { get; set; }
        public string relTypeCode { get; set; }
        public string relTypeLabel { get; set; }
        public int? orderBy { get; set; }
        public object groupId { get; set; }
        public object groupWordRelTypeCode { get; set; }
        public bool? @public { get; set; }
        public bool? wwUnif { get; set; }
        public bool? wwLite { get; set; }
        public bool? wwOs { get; set; }
    }

    public class Paradigm
    {
        public int? paradigmId { get; set; }
        public object comment { get; set; }
        public string inflectionType { get; set; }
        public string inflectionTypeNr { get; set; }
        public string wordClass { get; set; }
        public List<Form> forms { get; set; }
        public bool? formsExist { get; set; }
    }

    public class Po
    {
        public string name { get; set; }
        public string code { get; set; }
        public string value { get; set; }
    }

    public class PrimaryCollocation
    {
        public string posGroupCode { get; set; }
        public string posGroupValue { get; set; }
        public List<RelGroup> relGroups { get; set; }
    }

    public class PrimaryWordRelationGroup
    {
        public object id { get; set; }
        public string groupTypeCode { get; set; }
        public string groupTypeLabel { get; set; }
        public List<Member> members { get; set; }
    }

    public class RelGroup
    {
        public string relGroupCode { get; set; }
        public string relGroupValue { get; set; }
        public List<Collocation> collocations { get; set; }
    }

    public class SecondaryCollocation
    {
        public int? lexemeId { get; set; }
        public int? wordId { get; set; }
        public string wordValue { get; set; }
        public List<Usage> usages { get; set; }
        public List<Member> members { get; set; }
        public object groupOrder { get; set; }
        public int? headwordCollocMemberId { get; set; }
    }

    public class SecondaryWordRelationGroup
    {
        public object id { get; set; }
        public string groupTypeCode { get; set; }
        public string groupTypeLabel { get; set; }
        public List<Member> members { get; set; }
    }

    public class SemanticType
    {
        public string name { get; set; }
        public string code { get; set; }
        public string value { get; set; }
    }

    public class Synonym
    {
        public string type { get; set; }
        public int? meaningId { get; set; }
        public int? relationId { get; set; }
        public List<Word> words { get; set; }
        public string wordLang { get; set; }
        public double? weight { get; set; }
        public int? orderBy { get; set; }
    }

    public class SynonymLangGroup
    {
        public string lang { get; set; }
        public bool? selected { get; set; }
        public List<Synonym> synonyms { get; set; }
        public List<object> inexactSynonyms { get; set; }
    }

    public class Usage
    {
        public string createdBy { get; set; }
        public DateTime? createdOn { get; set; }
        public string modifiedBy { get; set; }
        public DateTime? modifiedOn { get; set; }
        public int? id { get; set; }
        public string value { get; set; }
        public string valuePrese { get; set; }
        public string lang { get; set; }
        public int? orderBy { get; set; }
        public object translations { get; set; }
        public object sourceLinks { get; set; }
        public bool? @public { get; set; }
        public bool? wwUnif { get; set; }
        public bool? wwLite { get; set; }
        public bool? wwOs { get; set; }
    }

    public class Word
    {
        public int? wordId { get; set; }
        public string wordValue { get; set; }
        public string wordValuePrese { get; set; }
        public int? homonymNr { get; set; }
        public string lang { get; set; }
        public string morphophonoForm { get; set; }
        public bool? prefixoid { get; set; }
        public bool? suffixoid { get; set; }
        public bool? foreign { get; set; }
        public List<string> lexemesTagNames { get; set; }
        public List<string> datasetCodes { get; set; }
        public List<Etymology> etymology { get; set; }
        public List<Paradigm> paradigms { get; set; }
        public WordOsMorph wordOsMorph { get; set; }
        public DateTime? lastActivityEventOn { get; set; }
        public DateTime? manualEventOn { get; set; }
        public bool? wordPublic { get; set; }
        public bool? @public { get; set; }
        public bool? wwUnif { get; set; }
        public bool? wwLite { get; set; }
        public bool? wwOs { get; set; }
    }

    public class Word2
    {
        public int? wordId { get; set; }
        public string wordValue { get; set; }
        public string wordValuePrese { get; set; }
        public int? homonymNr { get; set; }
        public bool? homonymsExist { get; set; }
        public string lang { get; set; }
        public List<string> wordTypeCodes { get; set; }
        public bool? prefixoid { get; set; }
        public bool? suffixoid { get; set; }
        public bool? foreign { get; set; }
        public int? lexemeId { get; set; }
        public string lexemeLevels { get; set; }
        public object lexemeValueStateCode { get; set; }
        public List<string> lexemeRegisterCodes { get; set; }
        public bool? lexemePublic { get; set; }
    }

    public class WordOsMorph
    {
        public string createdBy { get; set; }
        public DateTime? createdOn { get; set; }
        public string modifiedBy { get; set; }
        public DateTime? modifiedOn { get; set; }
        public int? id { get; set; }
        public int? wordId { get; set; }
        public string value { get; set; }
        public string valuePrese { get; set; }
        public bool? @public { get; set; }
        public bool? wwUnif { get; set; }
        public bool? wwLite { get; set; }
        public bool? wwOs { get; set; }
    }

    public class WordRelationDetails
    {
        public object wordSynRelations { get; set; }
        public List<PrimaryWordRelationGroup> primaryWordRelationGroups { get; set; }
        public List<SecondaryWordRelationGroup> secondaryWordRelationGroups { get; set; }
        public List<object> wordGroups { get; set; }
        public bool? groupRelationExists { get; set; }
    }
}
