using System.Globalization;
using NodaTime.Text;

namespace Rasm.Drafting;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class RevisionAlphabet {
    public static readonly RevisionAlphabet Asme = new(symbols: "ABCDEFGHJKLMNPRTUVWY", maxLetters: 3);
    public static readonly RevisionAlphabet Iso = new(symbols: "ABCDEFGHJKLMNPQRSTUVWXYZ", maxLetters: 3);

    public string Symbols { get; }
    public int MaxLetters { get; }
    public int Count => Range(1, MaxLetters).Fold(0, (sum, _) => (sum + 1) * Symbols.Length);
}

[ComplexValueObject(SkipToString = true)]
[ValidationError<DraftingFault>]
public sealed partial class RevisionIndex {
    public RevisionAlphabet Alphabet { get; }
    public int Ordinal { get; }

    public static Fin<RevisionIndex> Validated(RevisionAlphabet alphabet, int ordinal) => TryCreate(alphabet, ordinal, out RevisionIndex? index, out DraftingFault? fault) ? index : fault;

    public static Fin<RevisionIndex> FromLetters(RevisionAlphabet alphabet, string text) =>
        text.ToUpperInvariant() switch {
            var letters when letters.Length < 1 || letters.Length > alphabet.MaxLetters || letters.AsSpan().ContainsAnyExcept(alphabet.Symbols) => new DraftingFault(letters, DraftingReason.RevisionLetters),
            var letters => new RevisionIndex(alphabet, toSeq(letters).Fold(0, (ordinal, letter) => (ordinal * alphabet.Symbols.Length) + alphabet.Symbols.IndexOf(letter, StringComparison.Ordinal) + 1)),
        };

    public Fin<RevisionIndex> Next() => Validated(Alphabet, Ordinal + 1);

    public override string ToString() =>
        string.Concat(LanguageExt.List.unfold(Ordinal, rest => rest is 0 ? None : Some((Alphabet.Symbols[(rest - 1) % Alphabet.Symbols.Length], (rest - 1) / Alphabet.Symbols.Length))).Reverse());

    static partial void ValidateFactoryArguments(ref DraftingFault? validationError, ref RevisionAlphabet alphabet, ref int ordinal) =>
        validationError = ordinal >= 1 && ordinal <= alphabet.Count ? null : new DraftingFault(string.Create(CultureInfo.InvariantCulture, $"revision {ordinal}"), DraftingReason.RevisionOrdinal);
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class TitleRule {
    public static readonly TitleRule Required = new(mandatory: true, static (_, text) => text);
    public static readonly TitleRule Optional = new(mandatory: false, static (_, text) => text);
    public static readonly TitleRule Revision = new(mandatory: false, static (alphabet, text) => RevisionIndex.FromLetters(alphabet, text).Map(static index => index.ToString()));
    public static readonly TitleRule IssueDate = new(mandatory: true, static (_, text) =>
        LocalDatePattern.Iso.Parse(text) is { Success: true } parsed ? LocalDatePattern.Iso.Format(parsed.Value) : new DraftingFault(text, DraftingReason.IssueDate));

    public bool Mandatory { get; }

    public Fin<Option<string>> Accept(TitleField field, RevisionAlphabet alphabet, Option<string> entry) =>
        entry.IsNone && Mandatory ? new DraftingFault(field.Key, DraftingReason.TitleMissing)
            : entry.Traverse(text => Normalize(alphabet, text).MapFail(error => new DraftingFault(field.Key, DraftingReason.TitleInvalid, Inner: error))).As();

    [UseDelegateFromConstructor]
    private partial Fin<string> Normalize(RevisionAlphabet alphabet, string text);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TitleField {
    public static readonly TitleField LegalOwner = new ProjectEntry("legal-owner", TitleRule.Required);
    public static readonly TitleField IdentificationNumber = new SheetName("identification-number");
    public static readonly TitleField RevisionIndex = new SheetEntry("revision-index", TitleRule.Revision);
    public static readonly TitleField DateOfIssue = new SheetEntry("date-of-issue", TitleRule.IssueDate);
    public static readonly TitleField SegmentNumber = new SheetEntry("segment-number", None);
    public static readonly TitleField SegmentCount = new SheetEntry("segment-count", None);
    public static readonly TitleField LanguageCode = new ProjectEntry("language-code", TitleRule.Optional);
    public static readonly TitleField Title = new SheetEntry("title", TitleRule.Required);
    public static readonly TitleField SupplementaryTitle = new SheetEntry("supplementary-title", TitleRule.Optional);
    public static readonly TitleField ResponsibleDepartment = new ProjectEntry("responsible-department", TitleRule.Optional);
    public static readonly TitleField TechnicalReference = new ProjectEntry("technical-reference", TitleRule.Optional);
    public static readonly TitleField ApprovalPerson = new SheetEntry("approval-person", TitleRule.Required);
    public static readonly TitleField Creator = new SheetEntry("creator", TitleRule.Required);
    public static readonly TitleField DocumentType = new ProjectEntry("document-type", TitleRule.Required);
    public static readonly TitleField Classification = new ProjectEntry("classification", TitleRule.Optional);
    public static readonly TitleField DocumentStatus = new SheetEntry("document-status", TitleRule.Optional);
    public static readonly TitleField PaperSize = new SheetFact("paper-size", static sheet => sheet.Size.Key);
    public static readonly TitleField ProjectName = new ProjectEntry("project-name", TitleRule.Optional);
    public static readonly TitleField Client = new ProjectEntry("client", TitleRule.Optional);
    public static readonly TitleField Checker = new SheetEntry("checker", TitleRule.Optional);

    private TitleField(Option<TitleRule> rule) => Rule = rule;
    public abstract string Key { get; init; }
    public Option<TitleRule> Rule { get; }

    public sealed record ProjectEntry(string Key, Option<TitleRule> Rule) : TitleField(Rule);
    public sealed record SheetEntry(string Key, Option<TitleRule> Rule) : TitleField(Rule);
    public sealed record SheetName(string Key) : TitleField(None);
    public sealed record SheetFact(string Key, Func<Sheet, string> Read) : TitleField(None);

    public static Fin<Seq<(TitleField Field, Option<string> Text)>> Accept(DraftingConvention convention, HashMap<TitleField, string> entries) =>
        toSeq(entries.AsIterable())
            .Choose(static entry => entry.Key.Rule.Map(rule => (Field: entry.Key, Rule: rule, Text: entry.Value.Trim())))
            .Traverse(entry => entry.Rule.Accept(entry.Field, convention.Revisions, Some(entry.Text).Filter(static text => text.Length > 0)).Map(text => (entry.Field, text))).As();
}
