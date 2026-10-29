using System.Buffers;
using System.Globalization;
using System.Numerics;
using Rhino;

namespace Rasm.Rhino.Document.Files;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>]
[ValidationError<RhinoFault>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NamePart {
    internal static readonly SearchValues<char> Reserved = SearchValues.Create([.. """*/:<"?>\|""", .. Enumerable.Range(0, 32).Select(static code => (char)code)]);

    public static NamePart Ordinal(int position) => new(position.ToString(CultureInfo.InvariantCulture));

    public static Fin<NamePart> Validated(string value) => TryCreate(value, out NamePart? part, out RhinoFault? fault) ? part : fault;

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref string value) {
        ReadOnlySpan<char> trimmed = value.AsSpan().Trim();
        value = string.Create(trimmed.Length, trimmed, static (target, source) => source.ReplaceAny(target, Reserved, '_'));
        validationError = trimmed.Trim('.').IsEmpty ? new RhinoFault($"value = '{value}'", RhinoReason.Unqualified) : null;
    }
}

[ValueObject<string>]
[ValidationError<RhinoFault>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FileExtension {
    internal const char Mark = '.';

    public static Fin<FileExtension> Validated(string value) => TryCreate(value, out FileExtension? extension, out RhinoFault? fault) ? extension : fault;

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref string value) =>
        validationError = value is [Mark, _, ..] && !value.AsSpan(1).ContainsAny(NamePart.Reserved) && !value.AsSpan(1).Contains(Mark) ? null : new RhinoFault($"value = '{value}'", RhinoReason.Unqualified);
}

[ValueObject<int>(SkipFactoryMethods = true)]
public sealed partial class OutputVersion : IMinMaxValue<OutputVersion> {
    public static OutputVersion MinValue { get; } = new(1);

    public static OutputVersion MaxValue { get; } = new(int.MaxValue);

    public Fin<OutputVersion> Next() => _value < MaxValue._value ? Validated(_value + 1) : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"next of {_value}"), RhinoReason.Outside);

    public static Fin<OutputVersion> Validated(int value) => RhinoFault.Unless(value >= MinValue._value, RhinoReason.Outside, value).Map(_ => new OutputVersion(value));
}

[ValueObject<int>(SkipFactoryMethods = true)]
public sealed partial class SequenceNumber : IMinMaxValue<SequenceNumber> {
    public static SequenceNumber MinValue { get; } = new(0);

    public static SequenceNumber MaxValue { get; } = new(int.MaxValue);

    public static Fin<SequenceNumber> Validated(int value) => RhinoFault.Unless(value >= MinValue._value, RhinoReason.Outside, value).Map(_ => new SequenceNumber(value));
}

[Union]
public abstract partial record OutputVersioning {
    public sealed record Unversioned : OutputVersioning;

    public sealed record Pinned(OutputVersion Version) : OutputVersioning;

    public sealed record Next : OutputVersioning;
}

public sealed record OutputName(Seq<NamePart> Scope, OutputVersioning Version, Seq<NamePart> Parts, Option<SequenceNumber> Number, FileExtension Extension);

public sealed record OutputConvention(char Separator, string VersionMark, int VersionDigits, int SequenceDigits) {
    public static OutputConvention Default { get; } = new('_', "v", 3, 4);

    public string Compose(NamePart stem, OutputName name, Option<OutputVersion> version) =>
        string.Concat([
            string.Join(Separator, [stem, .. name.Scope, .. version.ToSeq().Map(held => VersionMark + ((int)held).ToString(CultureInfo.InvariantCulture).PadLeft(VersionDigits, '0')), .. name.Parts]),
            .. name.Number.ToSeq().Map(held => FileExtension.Mark + ((int)held).ToString(CultureInfo.InvariantCulture).PadLeft(SequenceDigits, '0')),
            name.Extension,
        ]);

    public Option<OutputVersion> VersionOf(NamePart stem, Seq<NamePart> scope, string entry) {
        string lead = string.Join(Separator, [stem, .. scope, VersionMark]);
        ReadOnlySpan<char> rest = entry.StartsWith(lead, StringComparison.OrdinalIgnoreCase) ? entry.AsSpan(lead.Length) : [];
        return int.TryParse(rest.IndexOfAny(Separator, FileExtension.Mark) is >= 0 and var stop ? rest[..stop] : rest, NumberStyles.None, CultureInfo.InvariantCulture, out int number)
            ? OutputVersion.Validated(number).ToOption()
            : None;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
[ValidationError<RhinoFault>]
public abstract partial record OutputFolder : IConvertible<string> {
    private const string DocumentMark = "//";

    public sealed record Absolute(string Folder) : OutputFolder;

    public sealed record BesideDocument(Seq<NamePart> Segments) : OutputFolder;

    public static RhinoFault? Validate(string? value, IFormatProvider? provider, out OutputFolder? item) {
        Fin<Option<OutputFolder>> parsed = Optional(value).Traverse(static text => text.StartsWith(DocumentMark, StringComparison.Ordinal)
            ? toSeq(text[DocumentMark.Length..].Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries)).Traverse(NamePart.Validated).As()
                .Map(static segments => (OutputFolder)new BesideDocument(segments))
            : new Absolute(text)).As();
        item = parsed.ToOption().Bind(static held => held).ValueUnsafe();
        return parsed.Match(Succ: static _ => null, Fail: static error => error.Filter<RhinoFault>().Head as RhinoFault);
    }

    public string ToValue() => Switch(
        absolute: static absolute => absolute.Folder,
        besideDocument: static beside => DocumentMark + string.Join('/', beside.Segments));

    public Fin<string> Of(Option<string> saved) =>
        Switch(saved,
            absolute: static (_, absolute) => Path.IsPathFullyQualified(absolute.Folder) ? absolute.Folder : new RhinoFault(absolute.Folder, RhinoReason.Unqualified),
            besideDocument: static (documentPath, beside) => Conversions.Required(documentPath).Map(path => Path.Join([Path.GetDirectoryName(path), .. beside.Segments])));
}

[Union]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
[ValidationError<RhinoFault>]
public abstract partial record Stem : IConvertible<string> {
    public sealed record DocumentName : Stem;

    public sealed record Literal(NamePart Value) : Stem;

    public static RhinoFault? Validate(string? value, IFormatProvider? provider, out Stem? item) {
        Fin<Option<Stem>> parsed = Optional(value).Traverse(static text => Conversions.Present(text).Match(
            Some: static named => NamePart.Validated(named).Map(static part => (Stem)new Literal(part)),
            None: static () => new DocumentName())).As();
        item = parsed.ToOption().Bind(static held => held).ValueUnsafe();
        return parsed.Match(Succ: static _ => null, Fail: static error => error.Filter<RhinoFault>().Head as RhinoFault);
    }

    public string ToValue() => Switch(
        documentName: static _ => "",
        literal: static literal => (string)literal.Value);

    public Fin<NamePart> Of(Option<string> saved) =>
        Switch(saved,
            documentName: static (documentPath, _) => Conversions.Required(documentPath).Bind(static path => NamePart.Validated(Path.GetFileNameWithoutExtension(path))),
            literal: static (_, literal) => literal.Value);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Destinations {
    // --- [RESOLUTION]
    public static IO<Seq<(OutputName Name, OutputPath Path)>> Preview(RhinoDoc document, OutputFolder folder, Stem stem, bool overwrite, OutputConvention convention, Seq<OutputName> names) =>
        Planned(document, folder, stem, overwrite, convention, names).Map(static planned => planned.Placed);

    public static IO<Seq<(OutputName Name, OutputPath Path)>> Resolve(RhinoDoc document, OutputFolder folder, Stem stem, bool overwrite, OutputConvention convention, Seq<OutputName> names) =>
        from planned in Planned(document, folder, stem, overwrite, convention, names)
        from created in unless(planned.Placed.IsEmpty, FilePublication.Faulted(planned.Folder, RhinoReason.Unwritable, IO.lift(() => ignore(Directory.CreateDirectory(planned.Folder))))).As()
        select planned.Placed;

    // --- [PLANNING]
    private static IO<(string Folder, Seq<(OutputName Name, OutputPath Path)> Placed)> Planned(
        RhinoDoc document, OutputFolder folder, Stem stem, bool overwrite, OutputConvention convention, Seq<OutputName> names) =>
        from saved in IO.lift(() => Conversions.Present(document.Path))
        from location in IO.lift(() => (folder.Of(saved), stem.Of(saved)).Apply(static (path, part) => (Folder: path, Stem: part)).As())
        let fresh = names.Filter(static name => name.Version is OutputVersioning.Next).Map(static name => name.Scope).Distinct()
        from entries in FilePublication.Faulted(location.Folder, RhinoReason.Unreadable, IO.lift(() =>
            !fresh.IsEmpty && Directory.Exists(location.Folder) ? toSeq(Directory.EnumerateFileSystemEntries(location.Folder)).Map(static entry => Path.GetFileName(entry)).Strict() : Seq<string>()))
        from next in IO.lift(() => fresh.Traverse(scope => entries.Choose(entry => convention.VersionOf(location.Stem, scope, entry)).Max<Seq, OutputVersion>()
                .Match(Some: static top => top.Next(), None: static () => OutputVersion.MinValue)
                .Map(version => (scope, version)))
            .As()
            .Map(static versions => toHashMap(versions)))
        from placed in IO.lift(() => names
            .Map(name => (Name: name, Version: name.Version.Switch(
                (Next: next, name.Scope),
                unversioned: static (_, _) => Option<OutputVersion>.None,
                pinned: static (_, pinned) => Some(pinned.Version),
                next: static (state, _) => Some(state.Next[state.Scope]))))
            .Traverse(held => OutputPath.Of(Path.Join(location.Folder, convention.Compose(location.Stem, held.Name, held.Version)))
                .Map(path => (Name: held.Name with { Version = held.Version.Map<OutputVersioning>(static version => new OutputVersioning.Pinned(version)).IfNone(held.Name.Version) }, Path: path)))
            .As()
            .Bind(named => (
                    Conversions.Unique(named, static row => (string)row.Path, StringComparer.OrdinalIgnoreCase),
                    named.Traverse(row => RhinoFault.Unless(overwrite || !Path.Exists(row.Path), RhinoReason.Taken, (string)row.Path)).As())
                .Apply(static (held, _) => held)
                .As()))
        select (location.Folder, placed);
}
