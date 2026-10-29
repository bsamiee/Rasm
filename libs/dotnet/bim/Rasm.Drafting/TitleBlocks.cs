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
public abstract partial record TitleEntry {
    private TitleEntry(Option<TitleRule> rule) => Rule = rule;

    public Option<TitleRule> Rule { get; }

    public sealed record ProjectEntry(TitleRule Check) : TitleEntry(Check);
    public sealed record SheetEntry(Option<TitleRule> Check) : TitleEntry(Check);
    public sealed record SheetName() : TitleEntry(None);
    public sealed record SheetFact(Func<Sheet, string> Read) : TitleEntry(None);
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<DraftingFault>]
public sealed partial class TitleField {
    public static readonly TitleField LegalOwner = new("legal-owner", new TitleEntry.ProjectEntry(TitleRule.Required));
    public static readonly TitleField IdentificationNumber = new("identification-number", new TitleEntry.SheetName());
    public static readonly TitleField RevisionIndex = new("revision-index", new TitleEntry.SheetEntry(TitleRule.Revision));
    public static readonly TitleField DateOfIssue = new("date-of-issue", new TitleEntry.SheetEntry(TitleRule.IssueDate));
    public static readonly TitleField SegmentNumber = new("segment-number", new TitleEntry.SheetEntry(None));
    public static readonly TitleField SegmentCount = new("segment-count", new TitleEntry.SheetEntry(None));
    public static readonly TitleField LanguageCode = new("language-code", new TitleEntry.ProjectEntry(TitleRule.Optional));
    public static readonly TitleField Title = new("title", new TitleEntry.SheetEntry(TitleRule.Required));
    public static readonly TitleField SupplementaryTitle = new("supplementary-title", new TitleEntry.SheetEntry(TitleRule.Optional));
    public static readonly TitleField ResponsibleDepartment = new("responsible-department", new TitleEntry.ProjectEntry(TitleRule.Optional));
    public static readonly TitleField TechnicalReference = new("technical-reference", new TitleEntry.ProjectEntry(TitleRule.Optional));
    public static readonly TitleField ApprovalPerson = new("approval-person", new TitleEntry.SheetEntry(TitleRule.Required));
    public static readonly TitleField Creator = new("creator", new TitleEntry.SheetEntry(TitleRule.Required));
    public static readonly TitleField DocumentType = new("document-type", new TitleEntry.ProjectEntry(TitleRule.Required));
    public static readonly TitleField Classification = new("classification", new TitleEntry.ProjectEntry(TitleRule.Optional));
    public static readonly TitleField DocumentStatus = new("document-status", new TitleEntry.SheetEntry(TitleRule.Optional));
    public static readonly TitleField PaperSize = new("paper-size", new TitleEntry.SheetFact(static sheet => sheet.Size.Key));
    public static readonly TitleField ProjectName = new("project-name", new TitleEntry.ProjectEntry(TitleRule.Optional));
    public static readonly TitleField Client = new("client", new TitleEntry.ProjectEntry(TitleRule.Optional));
    public static readonly TitleField Checker = new("checker", new TitleEntry.SheetEntry(TitleRule.Optional));

    public TitleEntry Entry { get; }

    public static Fin<Seq<(TitleField Field, Option<string> Text)>> Accept(DraftingConvention convention, HashMap<TitleField, string> entries) =>
        toSeq(entries.AsIterable())
            .Map(static entry => entry.Key.Entry.Rule.Map(rule => (Field: entry.Key, Rule: rule, Text: entry.Value.Trim())))
            .Somes()
            .Traverse(entry => entry.Rule.Accept(entry.Field, convention.Revisions, Some(entry.Text).Filter(static text => text.Length > 0)).Map(text => (entry.Field, text))).As();
}
