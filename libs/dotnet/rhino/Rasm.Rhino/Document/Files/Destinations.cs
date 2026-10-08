using System.Runtime.CompilerServices;
using Rasm.Imaging.Output;
using Rasm.Imaging.Pixels;
using Rhino;

namespace Rasm.Rhino.Document.Files;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
[ObjectFactory<string>]
public abstract partial record OutputFolder : IConvertible<string> {
    private const string DocumentMark = "//";

    public sealed record Absolute(string Folder) : OutputFolder;
    public sealed record BesideDocument(Seq<NamePart> Segments) : OutputFolder;

    public static InvalidRhinoValue? Validate(string? value, IFormatProvider? provider, out OutputFolder? item) {
        item = value switch {
            null => null,
            var text when text.StartsWith(DocumentMark, StringComparison.Ordinal) =>
                toSeq(text[DocumentMark.Length..].Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries))
                    .Traverse(Conversions.Validated<NamePart, string, InvalidRhinoValue>)
                    .As()
                    .Match<OutputFolder?>(Succ: static segments => new BesideDocument(segments), Fail: static _ => null),
            var text => new Absolute(text),
        };
        return value is not null && item is null ? new InvalidRhinoValue() : null;
    }

    public string ToValue() => Switch(
        absolute: static absolute => absolute.Folder,
        besideDocument: static beside => DocumentMark + string.Join('/', beside.Segments));
}

[Union]
[ValidationError<InvalidRhinoValue>]
[ObjectFactory<string>]
public abstract partial record Stem : IConvertible<string> {
    public sealed record DocumentName : Stem;
    public sealed record Literal(NamePart Value) : Stem;

    public static InvalidRhinoValue? Validate(string? value, IFormatProvider? provider, out Stem? item) {
        item = value is null ? null : Conversions.Present(value).Match(
            Some: static text => Conversions.Validated<NamePart, string, InvalidRhinoValue>(text).Match<Stem?>(Succ: static part => new Literal(part), Fail: static _ => null),
            None: static () => new DocumentName());
        return value is not null && item is null ? new InvalidRhinoValue() : null;
    }

    public string ToValue() => Switch(
        documentName: static _ => "",
        literal: static literal => (string)literal.Value);
}

public sealed record Destination(OutputFolder Folder, Stem Stem, bool Overwrite) : IStateRecord<Destination, DestinationParameter, InvalidRhinoValue> {
    public static Destination Default { get; } = new(new OutputFolder.BesideDocument(Seq<NamePart>()), new Stem.DocumentName(), Overwrite: false);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<InvalidRhinoValue>]
public sealed partial class DestinationParameter : IStateParameter<Destination> {
    public static readonly DestinationParameter Folder = new(
        "folder",
        new StateParameter<Destination>.Keyed<OutputFolder, string, InvalidRhinoValue>(
            Lens<Destination, OutputFolder>.New(static destination => destination.Folder, static folder => destination => destination with { Folder = folder })));

    public static readonly DestinationParameter Stem = new(
        "stem",
        new StateParameter<Destination>.Keyed<Stem, string, InvalidRhinoValue>(
            Lens<Destination, Stem>.New(static destination => destination.Stem, static stem => destination => destination with { Stem = stem })));

    public static readonly DestinationParameter Overwrite = new(
        "overwrite",
        new StateParameter<Destination>.Toggle(
            Lens<Destination, bool>.New(static destination => destination.Overwrite, static overwrite => destination => destination with { Overwrite = overwrite })));

    public StateParameter<Destination> Kind { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Destinations {
    // --- [PLANNING]
    public static Fin<string> FolderOf(OutputFolder folder, Option<string> saved) =>
        folder.Switch(saved,
            absolute: static (_, absolute) => Exchange.QualifiedPath(absolute.Folder),
            besideDocument: static (file, beside) => file.ToFin(new DocumentUnsaved()).Map(path => Path.Join([Path.GetDirectoryName(path), .. beside.Segments])));

    public static Fin<NamePart> StemOf(Stem stem, Option<string> saved) =>
        stem.Switch(saved,
            documentName: static (file, _) =>
                file.ToFin(new DocumentUnsaved()).Bind(static path => Conversions.Validated<NamePart, string, InvalidRhinoValue>(Path.GetFileNameWithoutExtension(path))),
            literal: static (_, literal) => Fin.Succ(literal.Value));

    private static Seq<string> Entries(string folder, Seq<Seq<NamePart>> fresh) =>
        !fresh.IsEmpty && Directory.Exists(folder)
            ? toSeq(Directory.EnumerateFileSystemEntries(folder)).Map(static entry => Path.GetFileName(entry)).Strict()
            : Seq<string>();

    private static Fin<HashMap<Seq<NamePart>, OutputVersion>> Versions(NamePart stem, Seq<Seq<NamePart>> fresh, Seq<string> entries) =>
        fresh.Traverse(scope => entries.Choose(entry => Naming.VersionOf(stem, scope, entry))
                .Fold(Option<OutputVersion>.None, static (top, version) => top.Filter(held => held > version) | version)
                .Match(Some: static top => top.Next(), None: static () => OutputVersion.MinValue)
                .ToValidation<Error>(new VersionsExhausted(stem, scope))
                .Map(version => (scope, version)))
            .As()
            .Map(static versions => toHashMap(versions))
            .ToFin();

    private static Validation<Error, Seq<(OutputName Name, OutputPath Path)>> Named(string folder, NamePart stem, HashMap<Seq<NamePart>, OutputVersion> next, Seq<OutputName> names) =>
        names.Map(name => (Name: name, Version: name.Version.Switch(
                (Next: next, name.Scope),
                unversioned: static (_, _) => Option<OutputVersion>.None,
                pinned: static (_, pinned) => Some(pinned.Version),
                nextFree: static (state, _) => Some(state.Next[state.Scope]))))
            .Traverse(held => Conversions.Validated<OutputPath, string, InvalidOutput>(Path.Join(folder, Naming.Compose(stem, held.Name, held.Version)))
                .Map(path => (Name: held.Name with { Version = held.Version.Map<OutputVersioning>(static version => version).IfNone(held.Name.Version) }, Path: path))
                .ToValidation())
            .As();

    private static Validation<Error, Seq<(OutputName Name, OutputPath Path)>> Plan(Seq<(OutputName Name, OutputPath Path)> named, Func<OutputPath, bool> free, string member) =>
        (
            Callbacks.Unique(named, static row => (string)row.Path, member, StringComparer.OrdinalIgnoreCase),
            named.Traverse(row => (free(row.Path) ? Fin.Succ(unit) : new DestinationOccupied(row.Path)).ToValidation()).As()
        )
            .Apply(static (held, _) => held)
            .As();

    private static IO<TValue> OnFolder<TValue>(string folder, Func<TValue> read) =>
        IO.lift(read).Catch(static error => error.HasException<IOException>() || error.HasException<UnauthorizedAccessException>(), error => IO.fail<TValue>(new FolderRefused(folder, error)));

    private static IO<(string Folder, Seq<(OutputName Name, OutputPath Path)> Placed)> Planned(
        RhinoDoc document, Destination destination, Seq<OutputName> names, [CallerMemberName] string member = "") =>
        from saved in IO.lift(() => Conversions.Present(document.Path))
        from folder in IO.lift(() => FolderOf(destination.Folder, saved))
        from stem in IO.lift(() => StemOf(destination.Stem, saved))
        let fresh = names.Filter(static name => name.Version.Map(unversioned: false, pinned: false, nextFree: true)).Map(static name => name.Scope).Distinct()
        from entries in OnFolder(folder, () => Entries(folder, fresh))
        from next in IO.lift(() => Versions(stem, fresh, entries))
        from placed in IO.lift(() => Named(folder, stem, next, names).Bind(named => Plan(named, path => destination.Overwrite || !Path.Exists(path), member)).ToFin())
        select (folder, placed);

    // --- [RESOLUTION]
    public static IO<Seq<(OutputName Name, OutputPath Path)>> Preview(RhinoDoc document, Destination destination, Seq<OutputName> names) =>
        Planned(document, destination, names).Map(static planned => planned.Placed);

    public static IO<Seq<(OutputName Name, OutputPath Path)>> Resolve(RhinoDoc document, Destination destination, Seq<OutputName> names) =>
        from planned in Planned(document, destination, names)
        from _ in unless(planned.Placed.IsEmpty, OnFolder(planned.Folder, () => ignore(Directory.CreateDirectory(planned.Folder))))
        select planned.Placed;
}
