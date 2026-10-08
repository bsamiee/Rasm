using System.Buffers;
using System.Globalization;

namespace Rasm.Drafting;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[ValidationError<InvalidDrafting>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Discipline {
    public static readonly Discipline General = new("G");
    public static readonly Discipline HazardousMaterials = new("H");
    public static readonly Discipline Survey = new("V");
    public static readonly Discipline Geotechnical = new("B");
    public static readonly Discipline Civil = new("C");
    public static readonly Discipline Landscape = new("L");
    public static readonly Discipline Structural = new("S");
    public static readonly Discipline Architectural = new("A");
    public static readonly Discipline Interiors = new("I");
    public static readonly Discipline Equipment = new("Q");
    public static readonly Discipline FireProtection = new("F");
    public static readonly Discipline Plumbing = new("P");
    public static readonly Discipline Process = new("D");
    public static readonly Discipline Mechanical = new("M");
    public static readonly Discipline Electrical = new("E");
    public static readonly Discipline DistributedEnergy = new("W");
    public static readonly Discipline Telecommunications = new("T");
    public static readonly Discipline Resource = new("R");
    public static readonly Discipline OtherDisciplines = new("X");
    public static readonly Discipline Contractor = new("Z");
    public static readonly Discipline Operations = new("O");

    internal static Option<int> Rank(string designator) =>
        Some(toSeq(Items).TakeWhile(item => !designator.StartsWith(item.Key, StringComparison.Ordinal)).Count).Filter(static rank => rank < Items.Count);
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
internal sealed partial class FieldRole {
    public static readonly FieldRole Code = new(static _ => Some(0));
    public static readonly FieldRole Designator = new(Discipline.Rank);
    public static readonly FieldRole Sequence = new(static value => int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int sequence) ? Some(sequence) : None);

    [UseDelegateFromConstructor]
    public partial Option<int> Rank(string value);

    public int Compare(string left, string right) => Rank(left).CompareTo(Rank(right)) is var order and not 0 ? order : string.CompareOrdinal(left, right);
}

[Union(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record NameLayout {
    internal abstract string Compose(Seq<NameField> fields, HashMap<NameField, string> entries);
    internal abstract Option<HashMap<NameField, string>> Split(Seq<NameField> fields, string text);
    internal abstract bool Allows(Seq<NameField> fields, HashMap<NameField, string> entries);

    public sealed record Delimited(char Separator) : NameLayout {
        internal override string Compose(Seq<NameField> fields, HashMap<NameField, string> entries) =>
            string.Join(Separator, fields.Map(entries.Find).Somes());
        internal override Option<HashMap<NameField, string>> Split(Seq<NameField> fields, string text) =>
            Some(fields.Fold(
                    (Tokens: toSeq(text.Split(Separator)), Entries: HashMap<NameField, string>()),
                    static (state, field) => state.Tokens.Head.Filter(token => field.Fits(token.Length)).Match(
                        Some: token => (state.Tokens.Tail, state.Entries.Add(field, token)),
                        None: () => state)))
                .Filter(static state => state.Tokens.IsEmpty)
                .Map(static state => state.Entries);
        internal override bool Allows(Seq<NameField> fields, HashMap<NameField, string> entries) =>
            fields.Zip(fields.Inits).ForAll(pair => entries.Find(pair.First).ForAll(value =>
                pair.Second.Rev().TakeWhile(earlier => !entries.ContainsKey(earlier)).ForAll(earlier => !earlier.Fits(value.Length))));
    }

    public sealed record Positional(char Pad) : NameLayout {
        internal override string Compose(Seq<NameField> fields, HashMap<NameField, string> entries) =>
            fields.Init.FoldBack(
                fields.Last.Bind(entries.Find).IfNone(""),
                (text, field) => entries.Find(field) switch {
                    { IsNone: true } when text.Length == 0 => text,
                    var value => value.IfNone("").PadRight(field.MaxWidth.IfNone(0), Pad) + text,
                });
        internal override Option<HashMap<NameField, string>> Split(Seq<NameField> fields, string text) =>
            from sliced in fields.Init.Fold(
                    Some((Remaining: text, Slices: Seq<(NameField Field, string Value)>())),
                    (progress, field) => progress.Bind(state => (state.Remaining, field.MaxWidth.IfNone(state.Remaining.Length)) switch {
                        ("", _) => Some(state),
                        var (rest, width) when rest.Length < width => None,
                        var (rest, width) => Some((rest[width..], state.Slices.Add((field, rest[..width].TrimEnd(Pad))))),
                    }))
            from last in fields.Last
            select toHashMap(sliced.Slices.Add((last, sliced.Remaining)).Filter(static slice => slice.Value.Length > 0));
        internal override bool Allows(Seq<NameField> fields, HashMap<NameField, string> entries) => true;
    }
}

[SmartEnum<string>]
[ValidationError<InvalidDrafting>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NamingStandard {
    public static readonly NamingStandard Uds = new("uds", new NameLayout.Positional('-'));
    public static readonly NamingStandard Iso19650 = new("iso-19650", new NameLayout.Delimited('-'));
    public static readonly NamingStandard Ncs = new("ncs", new NameLayout.Delimited('-'));
    public static readonly NamingStandard Iso13567 = new("iso-13567", new NameLayout.Positional('-'));
    public static readonly NamingStandard AecUk = new("aec-uk", new NameLayout.Delimited('-'));

    public NameLayout Layout { get; }
    public Seq<NameField> Fields => toSeq(NameField.Items).Filter(nameField => nameField.Standard == this);
    internal Option<NameField> SequenceField => Fields.Find(static nameField => nameField.Role == FieldRole.Sequence);
}

[SmartEnum<string>]
[ValidationError<InvalidDrafting>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NameField {
    private const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
    private const string DecimalDigits = "0123456789";
    private static readonly SearchValues<char> Upper = SearchValues.Create(UppercaseLetters);
    private static readonly SearchValues<char> Digit = SearchValues.Create(DecimalDigits);
    private static readonly SearchValues<char> NoCharacters = SearchValues.Create("");
    private static readonly SearchValues<char> UpperDigit = SearchValues.Create(UppercaseLetters + DecimalDigits);
    private static readonly SearchValues<char> UpperDigitTilde = SearchValues.Create(UppercaseLetters + DecimalDigits + "~");
    private static readonly SearchValues<char> StatusLetterDigit = SearchValues.Create("ADEFMNTX123456789");
    private static readonly SearchValues<char> UpperDigitUnderscore = SearchValues.Create(UppercaseLetters + DecimalDigits + "_");
    private static readonly SearchValues<char> LetterDigitUnderscore = SearchValues.Create(UppercaseLetters + LowercaseLetters + DecimalDigits + "_");
    private static readonly SearchValues<char> PresentationLetter = SearchValues.Create("DHMPTX");
    private static readonly SearchValues<char> LetterDigit = SearchValues.Create(UppercaseLetters + LowercaseLetters + DecimalDigits);
    private static readonly SearchValues<char> Letter = SearchValues.Create(UppercaseLetters + LowercaseLetters);

    public static readonly NameField UdsDiscipline = new("uds-discipline", NamingStandard.Uds, FieldRole.Designator, minWidth: 1, maxWidth: 2, required: true, characters: Upper);
    public static readonly NameField UdsSheetType = new("uds-sheet-type", NamingStandard.Uds, FieldRole.Code, minWidth: 1, maxWidth: 1, required: true, characters: Digit);
    public static readonly NameField UdsSequence = new("uds-sequence", NamingStandard.Uds, FieldRole.Sequence, minWidth: 2, maxWidth: 2, required: true, characters: Digit);
    public static readonly NameField UdsSuffixDash = new("uds-suffix-dash", NamingStandard.Uds, FieldRole.Code, minWidth: 1, maxWidth: 1, required: false, characters: NoCharacters);
    public static readonly NameField UdsSuffix = new("uds-suffix", NamingStandard.Uds, FieldRole.Code, minWidth: 1, maxWidth: 3, required: false, characters: UpperDigit);
    public static readonly NameField ContainerProject = new("container-project", NamingStandard.Iso19650, FieldRole.Code, minWidth: 1, maxWidth: None, required: true, characters: UpperDigit);
    public static readonly NameField ContainerOriginator = new("container-originator", NamingStandard.Iso19650, FieldRole.Code, minWidth: 1, maxWidth: None, required: true, characters: UpperDigit);
    public static readonly NameField ContainerFunctional = new("container-functional", NamingStandard.Iso19650, FieldRole.Code, minWidth: 1, maxWidth: None, required: true, characters: UpperDigit);
    public static readonly NameField ContainerSpatial = new("container-spatial", NamingStandard.Iso19650, FieldRole.Code, minWidth: 1, maxWidth: None, required: true, characters: UpperDigit);
    public static readonly NameField ContainerForm = new("container-form", NamingStandard.Iso19650, FieldRole.Code, minWidth: 1, maxWidth: None, required: true, characters: UpperDigit);
    public static readonly NameField ContainerDiscipline = new("container-discipline", NamingStandard.Iso19650, FieldRole.Code, minWidth: 1, maxWidth: None, required: true, characters: UpperDigit);
    public static readonly NameField ContainerNumber = new("container-number", NamingStandard.Iso19650, FieldRole.Sequence, minWidth: 1, maxWidth: 9, required: true, characters: Digit);
    public static readonly NameField NcsDiscipline = new("ncs-discipline", NamingStandard.Ncs, FieldRole.Designator, minWidth: 1, maxWidth: 2, required: true, characters: Upper);
    public static readonly NameField NcsMajorGroup = new("ncs-major-group", NamingStandard.Ncs, FieldRole.Code, minWidth: 4, maxWidth: 4, required: true, characters: UpperDigitTilde);
    public static readonly NameField NcsMinorGroup = new("ncs-minor-group", NamingStandard.Ncs, FieldRole.Code, minWidth: 4, maxWidth: 4, required: false, characters: UpperDigitTilde);
    public static readonly NameField NcsSecondMinorGroup = new("ncs-second-minor-group", NamingStandard.Ncs, FieldRole.Code, minWidth: 4, maxWidth: 4, required: false, characters: UpperDigitTilde);
    public static readonly NameField NcsStatus = new("ncs-status", NamingStandard.Ncs, FieldRole.Code, minWidth: 1, maxWidth: 1, required: false, characters: StatusLetterDigit);
    public static readonly NameField IsoAgent = new("iso-agent", NamingStandard.Iso13567, FieldRole.Code, minWidth: 1, maxWidth: 2, required: true, characters: UpperDigitUnderscore);
    public static readonly NameField IsoElement = new("iso-element", NamingStandard.Iso13567, FieldRole.Code, minWidth: 1, maxWidth: 6, required: true, characters: UpperDigitUnderscore);
    public static readonly NameField IsoPresentation = new("iso-presentation", NamingStandard.Iso13567, FieldRole.Code, minWidth: 1, maxWidth: 2, required: true, characters: UpperDigitUnderscore);
    public static readonly NameField IsoStatus = new("iso-status", NamingStandard.Iso13567, FieldRole.Code, minWidth: 1, maxWidth: 1, required: false, characters: UpperDigitUnderscore);
    public static readonly NameField IsoSector = new("iso-sector", NamingStandard.Iso13567, FieldRole.Code, minWidth: 1, maxWidth: 4, required: false, characters: UpperDigitUnderscore);
    public static readonly NameField IsoPhase = new("iso-phase", NamingStandard.Iso13567, FieldRole.Code, minWidth: 1, maxWidth: 1, required: false, characters: UpperDigitUnderscore);
    public static readonly NameField IsoProjection = new("iso-projection", NamingStandard.Iso13567, FieldRole.Code, minWidth: 1, maxWidth: 1, required: false, characters: UpperDigitUnderscore);
    public static readonly NameField IsoScale = new("iso-scale", NamingStandard.Iso13567, FieldRole.Code, minWidth: 1, maxWidth: 1, required: false, characters: UpperDigitUnderscore);
    public static readonly NameField IsoWorkPackage = new("iso-work-package", NamingStandard.Iso13567, FieldRole.Code, minWidth: 1, maxWidth: 2, required: false, characters: UpperDigitUnderscore);
    public static readonly NameField IsoUserDefined = new("iso-user-defined", NamingStandard.Iso13567, FieldRole.Code, minWidth: 1, maxWidth: None, required: false, characters: LetterDigitUnderscore);
    public static readonly NameField AecRole = new("aec-role", NamingStandard.AecUk, FieldRole.Code, minWidth: 1, maxWidth: 2, required: true, characters: UpperDigit);
    public static readonly NameField AecClassification = new("aec-classification", NamingStandard.AecUk, FieldRole.Code, minWidth: 5, maxWidth: None, required: true, characters: LetterDigitUnderscore);
    public static readonly NameField AecPresentation = new("aec-presentation", NamingStandard.AecUk, FieldRole.Code, minWidth: 1, maxWidth: 1, required: true, characters: PresentationLetter);
    public static readonly NameField AecDescription = new("aec-description", NamingStandard.AecUk, FieldRole.Code, minWidth: 1, maxWidth: None, required: true, characters: LetterDigit);
    public static readonly NameField AecView = new("aec-view", NamingStandard.AecUk, FieldRole.Code, minWidth: 3, maxWidth: 3, required: false, characters: Letter);

    public NamingStandard Standard { get; }
    internal FieldRole Role { get; }
    public int MinWidth { get; }
    public Option<int> MaxWidth { get; }
    public bool Required { get; }
    public SearchValues<char> Characters { get; }

    public bool Fits(int length) => length >= MinWidth && MaxWidth.ForAll(max => length <= max);
    public bool Allows(string value) => Fits(value.Length) && !value.AsSpan().ContainsAnyExcept(Characters) && Role.Rank(value).IsSome;
}

[ComplexValueObject(SkipToString = true)]
[ValidationError<InvalidDrafting>]
public sealed partial class StandardName : IComparable<StandardName> {
    public NamingStandard Standard { get; }
    public HashMap<NameField, string> Fields { get; }

    public string Text => Standard.Layout.Compose(Standard.Fields, Fields);

    public Seq<StandardName> Path =>
        Standard.Fields.Filter(static nameField => !nameField.Required)
            .Map(nameField => Fields.Find(nameField).Map(value => (Field: nameField, Value: value))).Somes()
            .Scan(Fields.Filter(static (nameField, _) => nameField.Required), static (prefix, entry) => prefix.Add(entry.Field, entry.Value))
            .Map(fields => new StandardName(Standard, fields));

    public static Fin<StandardName> From(NamingStandard standard, string text) =>
        standard.Layout.Split(standard.Fields, text)
            .Bind(fields => Validate(standard, fields, out StandardName? name) is null ? Some(name!) : None)
            .ToFin(new MalformedName(standard, text));

    public static Validation<Error, Seq<(TSheet Sheet, StandardName Number)>> Renumber<TSheet>(Seq<(TSheet Sheet, StandardName Number)> ordered, Seq<StandardName> others) =>
        ordered.Map(entry => (entry.Sheet, Number: entry.Number.Resequenced(old =>
                ordered.Map(static other => other.Number).Filter(other => other.Series == entry.Number.Series)
                    .Map(static other => other.Sequence.Map(static sequence => sequence.Value)).Somes().Distinct() switch {
                        var sequences => sequences.Fold(old, Math.Min) + sequences.TakeWhile(sequence => sequence != old).Count,
                    })))
            switch {
                var renumbered => renumbered
                    .Map(static (entry, index) => (entry.Sheet, entry.Number, Index: index))
                    .Traverse(entry => renumbered.Filter(other => other.Number == entry.Number).Count > 1 || others.Exists(other => other == entry.Number)
                        ? Fail<Error, (TSheet Sheet, StandardName Number)>(new SheetNumberCollision(entry.Index, entry.Number))
                        : Success<Error, (TSheet Sheet, StandardName Number)>((entry.Sheet, entry.Number)))
                    .As(),
            };

    public int CompareTo(StandardName? other) =>
        other is null ? 1
        : Standard.CompareTo(other.Standard) is var standard and not 0 ? standard
        : Standard.Fields.Map(field => Fields.Find(field).Match(
                Some: left => other.Fields.Find(field).Match(Some: right => field.Role.Compare(left, right), None: static () => 1),
                None: () => other.Fields.Find(field).IsSome ? -1 : 0))
            .Find(static order => order != 0).IfNone(0);

    public static bool operator <(StandardName? left, StandardName? right) => Comparer<StandardName>.Default.Compare(left, right) < 0;
    public static bool operator <=(StandardName? left, StandardName? right) => Comparer<StandardName>.Default.Compare(left, right) <= 0;
    public static bool operator >(StandardName? left, StandardName? right) => Comparer<StandardName>.Default.Compare(left, right) > 0;
    public static bool operator >=(StandardName? left, StandardName? right) => Comparer<StandardName>.Default.Compare(left, right) >= 0;

    public override string ToString() => Text;

    private (NamingStandard Standard, HashMap<NameField, string> Fields) Series =>
        (Standard, toHashMap(Standard.Fields.TakeWhile(static nameField => nameField.Role != FieldRole.Sequence)
            .Map(nameField => Fields.Find(nameField).Map(value => (nameField, value))).Somes()));

    private Option<(NameField Field, string Text, int Value)> Sequence =>
        from nameField in Standard.SequenceField
        from text in Fields.Find(nameField)
        from value in nameField.Role.Rank(text)
        select (nameField, text, value);

    private StandardName Resequenced(Func<int, int> next) =>
        Sequence.Map(sequence => new StandardName(Standard, Fields.SetItem(sequence.Field, next(sequence.Value).ToString(CultureInfo.InvariantCulture).PadLeft(sequence.Text.Length, '0'))))
            .IfNone(this);

    static partial void ValidateFactoryArguments(ref InvalidDrafting? validationError, ref NamingStandard standard, ref HashMap<NameField, string> fields) =>
        validationError = fun(static (NamingStandard named, HashMap<NameField, string> entries) =>
            entries.Keys.ForAll(field => field.Standard == named)
                && named.Fields.ForAll(field => entries.Find(field).Match(Some: field.Allows, None: () => !field.Required))
                && named.Layout.Allows(named.Fields, entries))(standard, fields) ? null : new InvalidDrafting();
}
