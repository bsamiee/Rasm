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
[ValidationError<InvalidDrafting>]
public sealed partial class RevisionIndex {
    public RevisionAlphabet Alphabet { get; }
    public int Ordinal { get; }

    public string Letters => Spell(Alphabet.Symbols, Ordinal);

    public static Fin<RevisionIndex> FromLetters(RevisionAlphabet alphabet, string text) =>
        text.ToUpperInvariant() switch {
            var letters when letters.Length > alphabet.MaxLetters || letters.AsSpan().ContainsAnyExcept(alphabet.Symbols) => new InvalidDrafting(),
            var letters => Validate(alphabet,
                toSeq(letters).Fold(0, (ordinal, letter) => (ordinal * alphabet.Symbols.Length) + alphabet.Symbols.IndexOf(letter, StringComparison.Ordinal) + 1),
                out RevisionIndex? index) is { } error ? error : index!,
        };

    public Fin<RevisionIndex> Next() => Validate(Alphabet, Ordinal + 1, out RevisionIndex? next) is { } error ? error : next!;

    public override string ToString() => Letters;

    private static string Spell(string symbols, int ordinal) =>
        ordinal == 0 ? "" : Spell(symbols, (ordinal - 1) / symbols.Length) + symbols[(ordinal - 1) % symbols.Length];

    static partial void ValidateFactoryArguments(ref InvalidDrafting? validationError, ref RevisionAlphabet alphabet, ref int ordinal) =>
        validationError = ordinal >= 1 && ordinal <= alphabet.Count ? null : new InvalidDrafting();
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class TitleRule {
    public static readonly TitleRule Required = new(mandatory: true, static (_, text) => text);
    public static readonly TitleRule Optional = new(mandatory: false, static (_, text) => text);
    public static readonly TitleRule Revision = new(mandatory: false, static (alphabet, text) => RevisionIndex.FromLetters(alphabet, text).Map(static index => index.Letters));
    public static readonly TitleRule IssueDate = new(mandatory: true, static (_, text) =>
        LocalDatePattern.Iso.Parse(text) is { Success: true } parsed ? LocalDatePattern.Iso.Format(parsed.Value) : new InvalidDrafting());

    public bool Mandatory { get; }

    public Validation<Error, Option<string>> Accept(TitleField field, RevisionAlphabet alphabet, Option<string> entry) =>
        entry.Match(
            Some: text => Normalize(alphabet, text).Map(static accepted => Some(accepted)).MapFail(error => new InvalidTitleField(field, error)).ToValidation(),
            None: () => Mandatory ? new MissingTitleField(field) : Success<Error, Option<string>>(None));

    [UseDelegateFromConstructor]
    private partial Fin<string> Normalize(RevisionAlphabet alphabet, string text);
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record TitleSource {
    public sealed record ProjectText(TitleRule Rule) : TitleSource;
    public sealed record SheetText(TitleRule Rule) : TitleSource;
    public sealed record Identifier : TitleSource;
    public sealed record Ordinal : TitleSource;
    public sealed record Count : TitleSource;
    public sealed record Designation : TitleSource;
}

[SmartEnum<string>]
[ValidationError<InvalidDrafting>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TitleField {
    public static readonly TitleField LegalOwner = new("legal-owner", new TitleSource.ProjectText(TitleRule.Required));
    public static readonly TitleField IdentificationNumber = new("identification-number", new TitleSource.Identifier());
    public static readonly TitleField RevisionIndex = new("revision-index", new TitleSource.SheetText(TitleRule.Revision));
    public static readonly TitleField DateOfIssue = new("date-of-issue", new TitleSource.SheetText(TitleRule.IssueDate));
    public static readonly TitleField SegmentNumber = new("segment-number", new TitleSource.Ordinal());
    public static readonly TitleField SegmentCount = new("segment-count", new TitleSource.Count());
    public static readonly TitleField LanguageCode = new("language-code", new TitleSource.ProjectText(TitleRule.Optional));
    public static readonly TitleField Title = new("title", new TitleSource.SheetText(TitleRule.Required));
    public static readonly TitleField SupplementaryTitle = new("supplementary-title", new TitleSource.SheetText(TitleRule.Optional));
    public static readonly TitleField ResponsibleDepartment = new("responsible-department", new TitleSource.ProjectText(TitleRule.Optional));
    public static readonly TitleField TechnicalReference = new("technical-reference", new TitleSource.ProjectText(TitleRule.Optional));
    public static readonly TitleField ApprovalPerson = new("approval-person", new TitleSource.SheetText(TitleRule.Required));
    public static readonly TitleField Creator = new("creator", new TitleSource.SheetText(TitleRule.Required));
    public static readonly TitleField DocumentType = new("document-type", new TitleSource.ProjectText(TitleRule.Required));
    public static readonly TitleField Classification = new("classification", new TitleSource.ProjectText(TitleRule.Optional));
    public static readonly TitleField DocumentStatus = new("document-status", new TitleSource.SheetText(TitleRule.Optional));
    public static readonly TitleField PaperSize = new("paper-size", new TitleSource.Designation());
    public static readonly TitleField ProjectName = new("project-name", new TitleSource.ProjectText(TitleRule.Optional));
    public static readonly TitleField Client = new("client", new TitleSource.ProjectText(TitleRule.Optional));
    public static readonly TitleField Checker = new("checker", new TitleSource.SheetText(TitleRule.Optional));

    public TitleSource Source { get; }

    public static Validation<Error, HashMap<TitleField, string>> Accept(RevisionAlphabet alphabet, HashMap<TitleField, string> entries) =>
        toSeq(Items)
            .Traverse(field => field.Source.Switch(
                    (Field: field, Alphabet: alphabet, Entry: entries.Find(field).Map(static text => text.Trim()).Filter(static text => text.Length > 0)),
                    projectText: static (input, source) => source.Rule.Accept(input.Field, input.Alphabet, input.Entry),
                    sheetText: static (input, source) => source.Rule.Accept(input.Field, input.Alphabet, input.Entry),
                    identifier: static (_, _) => Success<Error, Option<string>>(None),
                    ordinal: static (_, _) => Success<Error, Option<string>>(None),
                    count: static (_, _) => Success<Error, Option<string>>(None),
                    designation: static (_, _) => Success<Error, Option<string>>(None))
                .Map(accepted => accepted.Map(text => (field, text))))
            .As()
            .Map(static accepted => toHashMap(accepted.Somes()));
}
