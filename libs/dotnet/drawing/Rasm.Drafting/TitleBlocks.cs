using System.Buffers;
using System.Diagnostics.CodeAnalysis;
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

    [IgnoreMember]
    internal SearchValues<char> Letters => field ??= SearchValues.Create(Symbols);
}

[ComplexValueObject(SkipFactoryMethods = true, SkipToString = true)]
public sealed partial class RevisionIndex {
    public RevisionAlphabet Alphabet { get; }
    public int Ordinal { get; }

    public static Fin<RevisionIndex> Validated(RevisionAlphabet alphabet, int ordinal) =>
        DraftingFault.Unless(ordinal >= 1 && ordinal <= alphabet.Count, DraftingReason.RevisionOrdinal, (ordinal, alphabet.Count)).Map(_ => new RevisionIndex(alphabet, ordinal));

    public static Fin<RevisionIndex> FromLetters(RevisionAlphabet alphabet, string text) =>
        text.ToUpperInvariant() switch {
            var letters when letters.Length < 1 || letters.Length > alphabet.MaxLetters || letters.AsSpan().ContainsAnyExcept(alphabet.Letters) => new DraftingFault(letters, DraftingReason.RevisionLetters),
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

[Union(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial class TitleField {
    private TitleField(bool mandatory, TitleFormat format) => (Mandatory, Format) = (mandatory, format);
    public abstract string Name { get; }
    public bool Mandatory { get; }
    public TitleFormat Format { get; }

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is TitleField other && (Name, Mandatory, Format).Equals((other.Name, other.Mandatory, other.Format));
    public override int GetHashCode() => (Name, Mandatory, Format).GetHashCode();

    [SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
    public sealed partial class Iso7200 : TitleField {
        public static readonly Iso7200 LegalOwner = new("Legal owner", mandatory: true, TitleFormat.Text);
        public static readonly Iso7200 IdentificationNumber = new("Identification number", mandatory: true, TitleFormat.Text);
        public static readonly Iso7200 RevisionIndex = new("Revision index", mandatory: false, TitleFormat.Revision);
        public static readonly Iso7200 DateOfIssue = new("Date of issue", mandatory: true, TitleFormat.Date);
        public static readonly Iso7200 SegmentNumber = new("Segment/sheet number", mandatory: true, TitleFormat.Text);
        public static readonly Iso7200 SegmentCount = new("Number of segments/sheets", mandatory: false, TitleFormat.Text);
        public static readonly Iso7200 LanguageCode = new("Language code", mandatory: false, TitleFormat.Text);
        public static readonly Iso7200 Title = new("Title", mandatory: true, TitleFormat.Text);
        public static readonly Iso7200 SupplementaryTitle = new("Supplementary title", mandatory: false, TitleFormat.Text);
        public static readonly Iso7200 ResponsibleDepartment = new("Responsible department", mandatory: false, TitleFormat.Text);
        public static readonly Iso7200 TechnicalReference = new("Technical reference", mandatory: false, TitleFormat.Text);
        public static readonly Iso7200 ApprovalPerson = new("Approval person", mandatory: true, TitleFormat.Text);
        public static readonly Iso7200 Creator = new("Creator", mandatory: true, TitleFormat.Text);
        public static readonly Iso7200 DocumentType = new("Document type", mandatory: true, TitleFormat.Text);
        public static readonly Iso7200 Classification = new("Classification/key words", mandatory: false, TitleFormat.Text);
        public static readonly Iso7200 DocumentStatus = new("Document status", mandatory: false, TitleFormat.Text);
        public static readonly Iso7200 PageNumber = new("Page number", mandatory: false, TitleFormat.Text);
        public static readonly Iso7200 PageCount = new("Number of pages", mandatory: false, TitleFormat.Text);
        public static readonly Iso7200 PaperSize = new("Paper size", mandatory: false, TitleFormat.Text);

        public override string Name => Key;
    }

    public sealed class Custom : TitleField {
        private Custom(string name, bool mandatory, TitleFormat format) : base(mandatory, format) => Name = name;
        public override string Name { get; }

        public static readonly Custom ProjectName = new("Project name", mandatory: false, TitleFormat.Text);
        public static readonly Custom Client = new("Client", mandatory: false, TitleFormat.Text);
        public static readonly Custom Checker = new("Checker", mandatory: false, TitleFormat.Text);

        public static Fin<Custom> Validated(string name, bool mandatory, TitleFormat format) =>
            name.Trim() switch {
                var trimmed => (DraftingFault.Unless(trimmed.Length > 0, DraftingReason.Blank, name), DraftingFault.Unless(!Iso7200.TryGet(trimmed, out _), DraftingReason.Repeated, name))
                    .Apply((_, _) => new Custom(trimmed, mandatory, format)).As(),
            };
    }
}

public sealed record TitleBlock {
    private TitleBlock(HashMap<TitleField, TitleValue> values) => Values = values;
    public HashMap<TitleField, TitleValue> Values { get; }

    public static Fin<TitleBlock> Validated(Seq<TitleField> fields, RevisionAlphabet alphabet, HashMap<TitleField, string> entries) =>
        (fields.Traverse(field => entries.Find(field).Map(static text => text.Trim()).Filter(static text => text.Length > 0).Match(
                Some: text => field.Format.Parse(alphabet, text).Map(value => Some((Field: field, Value: value))).MapFail(error => new DraftingFault(field.Name, DraftingReason.TitleInvalid, Inner: error)),
                None: () => DraftingFault.Unless(!field.Mandatory, DraftingReason.TitleMissing, field.Name).Map(static _ => Option<(TitleField Field, TitleValue Value)>.None))).As(),
         toSeq(entries.Keys.Except(fields)).Map(static field => field.Name) switch { var unknown => DraftingFault.Unless(unknown.IsEmpty, DraftingReason.TitleUnknown, unknown) })
            .Apply(static (values, _) => new TitleBlock(toHashMap(values.Somes()))).As();
}
