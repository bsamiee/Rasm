using System.Numerics;
using NodaTime.Text;

namespace Rasm.Drafting;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class RevisionAlphabet {
    public static readonly RevisionAlphabet Asme = new(symbols: "ABCDEFGHJKLMNPRTUVWY", maxLetters: 3);
    public static readonly RevisionAlphabet Iso = new(symbols: "ABCDEFGHJKLMNPQRSTUVWXYZ", maxLetters: 3);

    public string Symbols { get; }
    public int MaxLetters { get; }
    public int Count => Symbols.Length * (int.CreateChecked(BigInteger.Pow(Symbols.Length, MaxLetters)) - 1) / (Symbols.Length - 1);
}

[ComplexValueObject(SkipFactoryMethods = true, SkipToString = true)]
public sealed partial class RevisionIndex {
    public RevisionAlphabet Alphabet { get; }
    public int Ordinal { get; }

    public static Fin<RevisionIndex> Validated(RevisionAlphabet alphabet, int ordinal) =>
        DraftingFault.Unless(ordinal >= 1 && ordinal <= alphabet.Count, DraftingReason.RevisionOrdinal, (ordinal, alphabet.Count)).Map(_ => new RevisionIndex(alphabet, ordinal));

    public static Fin<RevisionIndex> FromLetters(RevisionAlphabet alphabet, string text) =>
        text.ToUpperInvariant() switch {
            var letters when letters.Length < 1 || letters.Length > alphabet.MaxLetters || letters.AsSpan().ContainsAnyExcept(alphabet.Symbols) => new DraftingFault(letters, DraftingReason.RevisionLetters),
            var letters => new RevisionIndex(alphabet, toSeq(letters).Fold(0, (ordinal, letter) => (ordinal * alphabet.Symbols.Length) + alphabet.Symbols.IndexOf(letter, StringComparison.Ordinal) + 1)),
        };

    public Fin<RevisionIndex> Next() => Validated(Alphabet, Ordinal + 1);

    public override string ToString() =>
        string.Concat(LanguageExt.List.unfold(Ordinal, rest => rest is 0 ? None : Some((Alphabet.Symbols[(rest - 1) % Alphabet.Symbols.Length], (rest - 1) / Alphabet.Symbols.Length))).Reverse());
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class TitleRule {
    public static readonly TitleRule Required = new(mandatory: true, static (_, text) => text);
    public static readonly TitleRule Optional = new(mandatory: false, static (_, text) => text);
    public static readonly TitleRule Revision = new(mandatory: false, static (alphabet, text) => RevisionIndex.FromLetters(alphabet, text).Map(static index => index.ToString()));
    public static readonly TitleRule IssueDate = new(mandatory: true, static (_, text) => LocalDatePattern.Iso.Parse(text) is { Success: true } parsed ? LocalDatePattern.Iso.Format(parsed.Value) : new DraftingFault(text, DraftingReason.IssueDate));

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
    public static readonly TitleField SegmentNumber = new SheetEntry("segment-number");
    public static readonly TitleField SegmentCount = new SheetEntry("segment-count");
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

    public abstract string Key { get; init; }
    private protected virtual Option<TitleRule> Entry => None;

    public sealed record ProjectEntry(string Key, TitleRule Rule) : TitleField { private protected override Option<TitleRule> Entry => Rule; }
    public sealed record SheetEntry(string Key, Option<TitleRule> Rule = default) : TitleField { private protected override Option<TitleRule> Entry => Rule; }
    public sealed record SheetName(string Key) : TitleField;
    public sealed record SheetFact(string Key, Func<Sheet, string> Read) : TitleField;

    public static Fin<Seq<(TitleField Field, Option<string> Text)>> Accept(DraftingConvention convention, HashMap<TitleField, string> entries) =>
        toSeq(entries.AsIterable())
            .Choose(static entry => entry.Key.Entry.Map(rule => (Field: entry.Key, Rule: rule, Text: entry.Value.Trim())))
            .Traverse(entry => entry.Rule.Accept(entry.Field, convention.Revisions, Some(entry.Text).Filter(static text => text.Length > 0)).Map(text => (entry.Field, text))).As();
}
