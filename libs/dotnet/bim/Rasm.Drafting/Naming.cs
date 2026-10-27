using System.Buffers;
using System.Collections.Generic;
using System.Globalization;

namespace Rasm.Drafting;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
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
public sealed partial class FieldRole {
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

    public sealed record Delimited(char Separator) : NameLayout {
        internal override string Compose(Seq<NameField> fields, HashMap<NameField, string> entries) =>
            string.Join(Separator, fields.Map(entries.Find).Somes());
        internal override Option<HashMap<NameField, string>> Split(Seq<NameField> fields, string text) =>
            Some(fields.Fold(
                    (Tokens: toSeq(text.Split(Separator)), Entries: HashMap<NameField, string>()),
                    static (state, field) => state.Tokens.Head.Filter(token => field.Fits(token.Length)).Match(Some: token => (state.Tokens.Tail, state.Entries.Add(field, token)), None: () => state)))
                .Filter(static state => state.Tokens.IsEmpty)
                .Map(static state => state.Entries);
    }

    public sealed record Positional(char Pad) : NameLayout {
        internal override string Compose(Seq<NameField> fields, HashMap<NameField, string> entries) =>
            fields.Init.FoldBack(fields.Last.Bind(entries.Find).IfNone(""),
                (text, field) => entries.Find(field) switch { { IsNone: true } when text.Length is 0 => text, var value => value.IfNone("").PadRight(field.MaxWidth.IfNone(0), Pad) + text });
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
    }
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class NamingStandard {
    private const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
    private const string DecimalDigits = "0123456789";
    private static readonly SearchValues<char> Upper = SearchValues.Create(UppercaseLetters);
    private static readonly SearchValues<char> Digit = SearchValues.Create(DecimalDigits);
    private static readonly SearchValues<char> UpperDigit = SearchValues.Create(UppercaseLetters + DecimalDigits);
    private static readonly SearchValues<char> UpperDigitTilde = SearchValues.Create(UppercaseLetters + DecimalDigits + "~");
    private static readonly SearchValues<char> UpperDigitUnderscore = SearchValues.Create(UppercaseLetters + DecimalDigits + "_");
    private static readonly SearchValues<char> LetterDigitUnderscore = SearchValues.Create(UppercaseLetters + LowercaseLetters + DecimalDigits + "_");

    public static readonly NamingStandard Uds = new("uds", new NameLayout.Positional('-'), [
        new("discipline", FieldRole.Designator, MinWidth: 1, MaxWidth: 2, Required: true, Upper),
        new("sheet-type", FieldRole.Code, MinWidth: 1, MaxWidth: 1, Required: true, Digit),
        new("sequence", FieldRole.Sequence, MinWidth: 2, MaxWidth: 2, Required: true, Digit),
        new("suffix-dash", FieldRole.Code, MinWidth: 1, MaxWidth: 1, Required: false, SearchValues.Create("")),
        new("suffix", FieldRole.Code, MinWidth: 1, MaxWidth: 3, Required: false, UpperDigit),
    ]);
    public static readonly NamingStandard Iso19650 = new("iso-19650", new NameLayout.Delimited('-'), [
        new("project", FieldRole.Code, MinWidth: 1, MaxWidth: None, Required: true, UpperDigit),
        new("originator", FieldRole.Code, MinWidth: 1, MaxWidth: None, Required: true, UpperDigit),
        new("functional", FieldRole.Code, MinWidth: 1, MaxWidth: None, Required: true, UpperDigit),
        new("spatial", FieldRole.Code, MinWidth: 1, MaxWidth: None, Required: true, UpperDigit),
        new("form", FieldRole.Code, MinWidth: 1, MaxWidth: None, Required: true, UpperDigit),
        new("discipline", FieldRole.Code, MinWidth: 1, MaxWidth: None, Required: true, UpperDigit),
        new("number", FieldRole.Sequence, MinWidth: 1, MaxWidth: 9, Required: true, Digit),
    ]);
    public static readonly NamingStandard Ncs = new("ncs", new NameLayout.Delimited('-'), [
        new("discipline", FieldRole.Designator, MinWidth: 1, MaxWidth: 2, Required: true, Upper),
        new("major-group", FieldRole.Code, MinWidth: 4, MaxWidth: 4, Required: true, UpperDigitTilde),
        new("minor-group", FieldRole.Code, MinWidth: 4, MaxWidth: 4, Required: false, UpperDigitTilde),
        new("second-minor-group", FieldRole.Code, MinWidth: 4, MaxWidth: 4, Required: false, UpperDigitTilde),
        new("status", FieldRole.Code, MinWidth: 1, MaxWidth: 1, Required: false, SearchValues.Create("ADEFMNTX123456789")),
    ]);
    public static readonly NamingStandard Iso13567 = new("iso-13567", new NameLayout.Positional('-'), [
        new("agent", FieldRole.Code, MinWidth: 1, MaxWidth: 2, Required: true, UpperDigitUnderscore),
        new("element", FieldRole.Code, MinWidth: 1, MaxWidth: 6, Required: true, UpperDigitUnderscore),
        new("presentation", FieldRole.Code, MinWidth: 1, MaxWidth: 2, Required: true, UpperDigitUnderscore),
        new("status", FieldRole.Code, MinWidth: 1, MaxWidth: 1, Required: false, UpperDigitUnderscore),
        new("sector", FieldRole.Code, MinWidth: 1, MaxWidth: 4, Required: false, UpperDigitUnderscore),
        new("phase", FieldRole.Code, MinWidth: 1, MaxWidth: 1, Required: false, UpperDigitUnderscore),
        new("projection", FieldRole.Code, MinWidth: 1, MaxWidth: 1, Required: false, UpperDigitUnderscore),
        new("scale", FieldRole.Code, MinWidth: 1, MaxWidth: 1, Required: false, UpperDigitUnderscore),
        new("work-package", FieldRole.Code, MinWidth: 1, MaxWidth: 2, Required: false, UpperDigitUnderscore),
        new("user-defined", FieldRole.Code, MinWidth: 1, MaxWidth: None, Required: false, LetterDigitUnderscore),
    ]);
    public static readonly NamingStandard AecUk = new("aec-uk", new NameLayout.Delimited('-'), [
        new("role", FieldRole.Code, MinWidth: 1, MaxWidth: 2, Required: true, UpperDigit),
        new("classification", FieldRole.Code, MinWidth: 5, MaxWidth: None, Required: true, LetterDigitUnderscore),
        new("presentation", FieldRole.Code, MinWidth: 1, MaxWidth: 1, Required: true, SearchValues.Create("DHMPTX")),
        new("description", FieldRole.Code, MinWidth: 1, MaxWidth: None, Required: true, SearchValues.Create(UppercaseLetters + LowercaseLetters + DecimalDigits)),
        new("view", FieldRole.Code, MinWidth: 3, MaxWidth: 3, Required: false, SearchValues.Create(UppercaseLetters + LowercaseLetters)),
    ]);

    public NameLayout Layout { get; }
    public Seq<NameField> Fields { get; }

    internal Option<NameField> SequenceField => Fields.Find(static nameField => nameField.Role == FieldRole.Sequence);
}

public sealed record NameField(string Key, FieldRole Role, int MinWidth, Option<int> MaxWidth, bool Required, SearchValues<char> Characters) {
    public bool Fits(int length) => length >= MinWidth && MaxWidth.ForAll(max => length <= max);
    public bool Allows(string value) => Fits(value.Length) && !value.AsSpan().ContainsAnyExcept(Characters) && Role.Rank(value).IsSome;
}

[ComplexValueObject(SkipFactoryMethods = true, SkipToString = true)]
public sealed partial class StandardName : IComparable<StandardName> {
    public NamingStandard Standard { get; }
    public HashMap<NameField, string> Fields { get; }

    public string Text => Standard.Layout.Compose(Standard.Fields, Fields);

    public Seq<StandardName> Path =>
        Standard.Fields.Filter(static nameField => !nameField.Required)
            .Map(nameField => Fields.Find(nameField).Map(value => (Field: nameField, Value: value))).Somes()
            .Scan(Fields.Filter(static (nameField, _) => nameField.Required), static (prefix, entry) => prefix.Add(entry.Field, entry.Value))
            .Map(fields => new StandardName(Standard, fields));

    public static Fin<StandardName> Validated(NamingStandard standard, HashMap<NameField, string> fields) =>
        DraftingFault.Unless(Accepts(standard, fields), DraftingReason.NameBroken, fields).Map(_ => new StandardName(standard, fields));

    public static Fin<StandardName> From(NamingStandard standard, string text) =>
        standard.Layout.Split(standard.Fields, text).ToFin(new DraftingFault(text, DraftingReason.NameBroken)).Bind(fields => Validated(standard, fields));

    public static Fin<Seq<(TSheet Sheet, StandardName Number)>> Renumber<TSheet>(Seq<(TSheet Sheet, StandardName Number)> ordered, Seq<StandardName> others) =>
        from sequences in Fin.Succ(ordered.Map(static entry => (entry.Number.Series, Value: entry.Number.Sequence.Map(static sequence => sequence.Value))).Distinct().ToLookup(static entry => entry.Series, static entry => entry.Value))
        let renumbered = ordered.Map(entry => (entry.Sheet, Number: entry.Number.Resequenced(toSeq(sequences[entry.Number.Series]).Somes())))
        let taken = renumbered.Map(static entry => entry.Number).Concat(others).ToLookup(static number => number)
        from numbered in renumbered.Map((entry, index) => taken[entry.Number].Skip(1).Any() ? new DraftingFault(entry.Number.Text, DraftingReason.NumberTaken, index) : Fin.Succ(entry)).Traverse(static held => held).As()
        select numbered;

    public static Seq<(TSheet Sheet, int Ordinal, int Count)> Positions<TSheet>(Seq<(TSheet Sheet, StandardName Number)> sheets) =>
        from members in toSeq(sheets.OrderBy(static entry => entry.Number).GroupBy(static entry => entry.Number.Series)).Map(static series => toSeq(series))
        from position in members.Map((entry, index) => (entry.Sheet, Ordinal: index + 1, members.Count))
        select position;

    public int CompareTo(StandardName? other) =>
        other is null ? 1
        : string.CompareOrdinal(Standard.Key, other.Standard.Key) is var standard and not 0 ? standard
        : Standard.Fields.Map(field => (Fields.Find(field).Case, other.Fields.Find(field).Case) switch { (string left, string right) => field.Role.Compare(left, right), (string, _) => 1, (_, string) => -1, _ => 0 })
            .Find(static order => order is not 0).IfNone(0);

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

    private StandardName Resequenced(Seq<int> sequences) =>
        Sequence.Map(sequence => new StandardName(Standard, Fields.SetItem(sequence.Field,
            (sequences.Fold(sequence.Value, int.Min) + sequences.TakeWhile(other => other != sequence.Value).Count).ToString(CultureInfo.InvariantCulture).PadLeft(sequence.Text.Length, '0')))).IfNone(this);

    private static bool Accepts(NamingStandard standard, HashMap<NameField, string> fields) =>
        standard.Fields.ForAll(field => fields.Find(field).Match(Some: field.Allows, None: () => !field.Required))
        && standard.Layout.Split(standard.Fields, standard.Layout.Compose(standard.Fields, fields)) == Some(fields);
}
