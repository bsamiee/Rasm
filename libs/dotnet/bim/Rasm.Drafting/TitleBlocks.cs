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
public sealed partial class RevisionIndex {
    public RevisionAlphabet Alphabet { get; }
    public int Ordinal { get; }

    public static Fin<RevisionIndex> FromLetters(RevisionAlphabet alphabet, string text) =>
        text.ToUpperInvariant() switch {
            var letters when letters.Length < 1 || letters.Length > alphabet.MaxLetters || letters.AsSpan().ContainsAnyExcept(alphabet.Symbols) =>
                Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"revision letters {letters} must be 1 to {alphabet.MaxLetters} of {alphabet.Symbols}")),
            var letters => Validated(alphabet, toSeq(letters).Fold(0, (ordinal, letter) => (ordinal * alphabet.Symbols.Length) + alphabet.Symbols.IndexOf(letter, StringComparison.Ordinal) + 1)),
        };

    public Fin<RevisionIndex> Next() => Validated(Alphabet, Ordinal + 1);

    public override string ToString() =>
        string.Concat(LanguageExt.List.unfold(Ordinal, rest => rest is 0 ? None : Some((Alphabet.Symbols[(rest - 1) % Alphabet.Symbols.Length], (rest - 1) / Alphabet.Symbols.Length))).Reverse());

    private static Fin<RevisionIndex> Validated(RevisionAlphabet alphabet, int ordinal) =>
        ValidationResults.ToFin(Validate(alphabet, ordinal, out RevisionIndex? index), index);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref RevisionAlphabet alphabet, ref int ordinal) =>
        validationError = ValidationResults.OutsideRange(ordinal, 1, alphabet.Count);
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class TitleRule {
    public static readonly TitleRule Required = new(mandatory: true, static (_, text) => text);
    public static readonly TitleRule Optional = new(mandatory: false, static (_, text) => text);
    public static readonly TitleRule Revision = new(mandatory: false, static (alphabet, text) => RevisionIndex.FromLetters(alphabet, text).Map(static index => index.ToString()));
    public static readonly TitleRule IssueDate = new(mandatory: true, static (_, text) =>
        LocalDatePattern.Iso.Parse(text) is { Success: true } parsed ? LocalDatePattern.Iso.Format(parsed.Value) : Error.New(ErrorCodes.ValidationFailed, $"issue date {text} is no ISO 8601 date"));

    public bool Mandatory { get; }

    public Fin<Option<string>> Accept(TitleField field, RevisionAlphabet alphabet, Option<string> entry) =>
        entry.Match(
            Some: text => Normalize(alphabet, text).Map(static accepted => Some(accepted)).MapFail(error => Error.New(ErrorCodes.ValidationFailed, $"title-block field {field.Key} has invalid entry", error)),
            None: () => Mandatory ? Error.New(ErrorCodes.ValidationFailed, $"title-block field {field.Key} has no entry") : Option<string>.None);

    [UseDelegateFromConstructor]
    private partial Fin<string> Normalize(RevisionAlphabet alphabet, string text);
}

[SmartEnum(MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class TitleSource {
    public static readonly TitleSource ProjectText = new();
    public static readonly TitleSource SheetText = new();
    public static readonly TitleSource Identifier = new();
    public static readonly TitleSource Designation = new();
}

public sealed record TitleField(string Key, TitleSource Source, Option<TitleRule> Rule) {
    public static readonly TitleField LegalOwner = new("legal-owner", TitleSource.ProjectText, TitleRule.Required);
    public static readonly TitleField IdentificationNumber = new("identification-number", TitleSource.Identifier, None);
    public static readonly TitleField RevisionIndex = new("revision-index", TitleSource.SheetText, TitleRule.Revision);
    public static readonly TitleField DateOfIssue = new("date-of-issue", TitleSource.SheetText, TitleRule.IssueDate);
    public static readonly TitleField SegmentNumber = new("segment-number", TitleSource.SheetText, None);
    public static readonly TitleField SegmentCount = new("segment-count", TitleSource.SheetText, None);
    public static readonly TitleField LanguageCode = new("language-code", TitleSource.ProjectText, TitleRule.Optional);
    public static readonly TitleField Title = new("title", TitleSource.SheetText, TitleRule.Required);
    public static readonly TitleField SupplementaryTitle = new("supplementary-title", TitleSource.SheetText, TitleRule.Optional);
    public static readonly TitleField ResponsibleDepartment = new("responsible-department", TitleSource.ProjectText, TitleRule.Optional);
    public static readonly TitleField TechnicalReference = new("technical-reference", TitleSource.ProjectText, TitleRule.Optional);
    public static readonly TitleField ApprovalPerson = new("approval-person", TitleSource.SheetText, TitleRule.Required);
    public static readonly TitleField Creator = new("creator", TitleSource.SheetText, TitleRule.Required);
    public static readonly TitleField DocumentType = new("document-type", TitleSource.ProjectText, TitleRule.Required);
    public static readonly TitleField Classification = new("classification", TitleSource.ProjectText, TitleRule.Optional);
    public static readonly TitleField DocumentStatus = new("document-status", TitleSource.SheetText, TitleRule.Optional);
    public static readonly TitleField PaperSize = new("paper-size", TitleSource.Designation, None);
    public static readonly TitleField ProjectName = new("project-name", TitleSource.ProjectText, TitleRule.Optional);
    public static readonly TitleField Client = new("client", TitleSource.ProjectText, TitleRule.Optional);
    public static readonly TitleField Checker = new("checker", TitleSource.SheetText, TitleRule.Optional);

    public static Fin<Seq<(TitleField Field, Option<string> Text)>> Accept(DraftingConvention convention, HashMap<TitleField, string> entries) =>
        toSeq(entries.AsIterable()).Choose(static entry => entry.Key.Rule.Map(rule => (Field: entry.Key, Rule: rule, Entry: Some(entry.Value.Trim()).Filter(static text => text.Length > 0))))
            .Traverse(entry => entry.Rule.Accept(entry.Field, convention.Revisions, entry.Entry).Map(text => (entry.Field, text)))
            .As();
}
