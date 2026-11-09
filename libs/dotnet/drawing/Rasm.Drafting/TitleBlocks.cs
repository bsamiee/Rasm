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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record TitleValue {
    public sealed record Text(string Value) : TitleValue;

    public sealed record Revision(RevisionIndex Index) : TitleValue;

    public sealed record Date(LocalDate Day) : TitleValue;
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class TitleFormat {
    public static readonly TitleFormat Text = new(static (_, text) => new TitleValue.Text(text));
    public static readonly TitleFormat Revision = new(static (alphabet, text) => RevisionIndex.FromLetters(alphabet, text).Map(static TitleValue (index) => new TitleValue.Revision(index)));
    public static readonly TitleFormat Date = new(static (_, text) => LocalDatePattern.Iso.Parse(text) is { Success: true } parsed ? new TitleValue.Date(parsed.Value) : new DraftingFault(text, DraftingReason.IssueDate));

    [UseDelegateFromConstructor]
    public partial Fin<TitleValue> Parse(RevisionAlphabet alphabet, string text);
}

public sealed record TitleField {
    private TitleField(string name, bool mandatory, TitleFormat format) => (Name, Mandatory, Format) = (name, mandatory, format);
    public string Name { get; }
    public bool Mandatory { get; }
    public TitleFormat Format { get; }

    public static readonly TitleField LegalOwner = new("Legal owner", mandatory: true, TitleFormat.Text);
    public static readonly TitleField IdentificationNumber = new("Identification number", mandatory: true, TitleFormat.Text);
    public static readonly TitleField RevisionIndex = new("Revision index", mandatory: false, TitleFormat.Revision);
    public static readonly TitleField DateOfIssue = new("Date of issue", mandatory: true, TitleFormat.Date);
    public static readonly TitleField SegmentNumber = new("Segment number", mandatory: false, TitleFormat.Text);
    public static readonly TitleField SegmentCount = new("Number of segments", mandatory: false, TitleFormat.Text);
    public static readonly TitleField LanguageCode = new("Language code", mandatory: false, TitleFormat.Text);
    public static readonly TitleField Title = new("Title", mandatory: true, TitleFormat.Text);
    public static readonly TitleField SupplementaryTitle = new("Supplementary title", mandatory: false, TitleFormat.Text);
    public static readonly TitleField ResponsibleDepartment = new("Responsible department", mandatory: false, TitleFormat.Text);
    public static readonly TitleField TechnicalReference = new("Technical reference", mandatory: false, TitleFormat.Text);
    public static readonly TitleField ApprovalPerson = new("Approval person", mandatory: true, TitleFormat.Text);
    public static readonly TitleField Creator = new("Creator", mandatory: true, TitleFormat.Text);
    public static readonly TitleField DocumentType = new("Document type", mandatory: true, TitleFormat.Text);
    public static readonly TitleField Classification = new("Classification", mandatory: false, TitleFormat.Text);
    public static readonly TitleField DocumentStatus = new("Document status", mandatory: false, TitleFormat.Text);
    public static readonly TitleField PaperSize = new("Paper size", mandatory: false, TitleFormat.Text);
    public static readonly TitleField ProjectName = new("Project name", mandatory: false, TitleFormat.Text);
    public static readonly TitleField Client = new("Client", mandatory: false, TitleFormat.Text);
    public static readonly TitleField Checker = new("Checker", mandatory: false, TitleFormat.Text);

    public static readonly Seq<TitleField> Iso7200 = [
        LegalOwner, IdentificationNumber, RevisionIndex, DateOfIssue, SegmentNumber, SegmentCount, LanguageCode, Title, SupplementaryTitle,
        ResponsibleDepartment, TechnicalReference, ApprovalPerson, Creator, DocumentType, Classification, DocumentStatus, PaperSize,
    ];

    public static Fin<TitleField> Validated(string name, bool mandatory, TitleFormat format) =>
        DraftingFault.Unless(!string.IsNullOrWhiteSpace(name), DraftingReason.Blank, name).Map(_ => new TitleField(name.Trim(), mandatory, format));
}

public sealed record TitleBlock {
    private TitleBlock(HashMap<TitleField, TitleValue> values) => Values = values;
    public HashMap<TitleField, TitleValue> Values { get; }

    public static Fin<TitleBlock> Validated(Seq<TitleField> fields, RevisionAlphabet alphabet, HashMap<TitleField, string> entries) =>
        (fields.Traverse(field => entries.Find(field).Map(static text => text.Trim()).Filter(static text => text.Length > 0).Match(
                Some: text => field.Format.Parse(alphabet, text).Map(value => Some((Field: field, Value: value))).MapFail(error => new DraftingFault(field.Name, DraftingReason.TitleInvalid, Inner: error)),
                None: () => DraftingFault.Unless(!field.Mandatory, DraftingReason.TitleMissing, field.Name).Map(static _ => Option<(TitleField Field, TitleValue Value)>.None))).As(),
         toSeq(entries.Keys).Filter(field => !fields.Exists(known => known == field)).Map(static field => field.Name) switch { var unknown => DraftingFault.Unless(unknown.IsEmpty, DraftingReason.TitleUnknown, unknown) })
            .Apply(static (values, _) => new TitleBlock(toHashMap(values.Somes()))).As();
}
