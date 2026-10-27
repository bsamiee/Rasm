using Rhino;

namespace Rasm.RhinoCommon.Document.Files;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
[ValidationError<RhinoFault>]
public abstract partial record OutputFolder : IConvertible<string> {
    private const string DocumentMark = "//";

    public sealed record Absolute(string Folder) : OutputFolder;
    public sealed record BesideDocument(Seq<NamePart> Segments) : OutputFolder;

    public static RhinoFault? Validate(string? value, IFormatProvider? provider, out OutputFolder? item) {
        item = value switch {
            null => null,
            var text when text.StartsWith(DocumentMark, StringComparison.Ordinal) =>
                toSeq(text[DocumentMark.Length..].Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries))
                    .Traverse(static segment => NamePart.TryCreate(segment, out NamePart? part) ? Some(part) : None)
                    .As()
                    .Map(static segments => (OutputFolder)new BesideDocument(segments))
                    .ValueUnsafe(),
            var text => new Absolute(text),
        };
        return value is not null && item is null ? new RhinoFault($"value = '{value}'", RhinoReason.Invalid) : null;
    }

    public string ToValue() => Switch(
        absolute: static absolute => absolute.Folder,
        besideDocument: static beside => DocumentMark + string.Join('/', beside.Segments));
}

[Union]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
[ValidationError<RhinoFault>]
public abstract partial record Stem : IConvertible<string> {
    public sealed record DocumentName : Stem;
    public sealed record Literal(NamePart Value) : Stem;

    public static RhinoFault? Validate(string? value, IFormatProvider? provider, out Stem? item) {
        item = value is null ? null : Conversions.Present(value).Match(
            Some: static text => NamePart.TryCreate(text, out NamePart? part) ? new Literal(part) : default(Stem?),
            None: static () => new DocumentName());
        return value is not null && item is null ? new RhinoFault($"value = '{value}'", RhinoReason.Invalid) : null;
    }

    public string ToValue() => Switch(
        documentName: static _ => "",
        literal: static literal => (string)literal.Value);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Destinations {
    // --- [PLANNING]
    public static Fin<string> FolderOf(OutputFolder folder, Option<string> saved) =>
        folder.Switch(saved,
            absolute: static (_, absolute) => Path.IsPathFullyQualified(absolute.Folder) ? absolute.Folder : new RhinoFault(absolute.Folder, RhinoReason.Unqualified),
            besideDocument: static (documentPath, beside) => Conversions.Found(documentPath).Map(path => Path.Join([Path.GetDirectoryName(path), .. beside.Segments])));

    public static Fin<NamePart> StemOf(Stem stem, Option<string> saved) =>
        stem.Switch(saved,
            documentName: static (documentPath, _) =>
                Conversions.Found(documentPath).Bind(static path => NamePart.TryCreate(Path.GetFileNameWithoutExtension(path), out NamePart? part, out RhinoFault? fault) ? Fin.Succ(part) : fault),
            literal: static (_, literal) => literal.Value);

    public static Fin<(string Folder, NamePart Stem)> Located(OutputFolder folder, Stem stem, Option<string> saved) =>
        (FolderOf(folder, saved), StemOf(stem, saved))
            .Apply(static (folder, stem) => (Folder: folder, Stem: stem))
            .As();

    private static Seq<string> Entries(string folder, Seq<Seq<NamePart>> fresh) =>
        !fresh.IsEmpty && Directory.Exists(folder)
            ? toSeq(Directory.EnumerateFileSystemEntries(folder)).Map(static entry => Path.GetFileName(entry)).Strict()
            : Seq<string>();

    private static Fin<HashMap<Seq<NamePart>, OutputVersion>> Versions(NamePart stem, Seq<Seq<NamePart>> fresh, Seq<string> entries) =>
        fresh.Traverse(scope => entries.Choose(entry => Naming.VersionOf(stem, scope, entry))
                .Fold(Option<OutputVersion>.None, static (top, version) => top.Filter(held => held > version) | version)
                .Match(Some: static top => top.Next(), None: static () => OutputVersion.MinValue)
                .ToFin(new RhinoFault($"next output version of {stem} {scope}", RhinoReason.Outside))
                .Map(version => (scope, version)))
            .As()
            .Map(static versions => toHashMap(versions));

    private static Fin<Seq<(OutputName Name, OutputPath Path)>> Named(string folder, NamePart stem, HashMap<Seq<NamePart>, OutputVersion> next, Seq<OutputName> names) =>
        names.Map(name => (Name: name, Version: name.Version.Switch(
                (Next: next, name.Scope),
                unversioned: static (_, _) => Option<OutputVersion>.None,
                pinned: static (_, pinned) => Some(pinned.Version),
                nextFree: static (state, _) => Some(state.Next[state.Scope]))))
            .Traverse(held => OutputPath.Of(Path.Join(folder, Naming.Compose(stem, held.Name, held.Version)))
                .Map(path => (Name: held.Name with { Version = held.Version.Map<OutputVersioning>(static version => version).IfNone(held.Name.Version) }, Path: path)))
            .As();

    private static Fin<Seq<(OutputName Name, OutputPath Path)>> Plan(Seq<(OutputName Name, OutputPath Path)> named, Func<OutputPath, bool> free) =>
        (
            Conversions.Unique(named, static row => (string)row.Path, StringComparer.OrdinalIgnoreCase),
            named.Traverse(row => free(row.Path) ? Fin.Succ(unit) : new RhinoFault(row.Path, RhinoReason.Taken)).As()
        )
            .Apply(static (held, _) => held)
            .As();

    private static IO<(string Folder, Seq<(OutputName Name, OutputPath Path)> Placed)> Planned(
        RhinoDoc document, OutputFolder folder, Stem stem, bool overwrite, Seq<OutputName> names) =>
        from saved in IO.lift(() => Conversions.Present(document.Path))
        from location in IO.lift(() => Located(folder, stem, saved))
        let fresh = names.Filter(static name => name.Version.Map(unversioned: false, pinned: false, nextFree: true)).Map(static name => name.Scope).Distinct()
        from entries in FilePublication.Refusing(location.Folder, RhinoReason.Unreadable, IO.lift(() => Entries(location.Folder, fresh)))
        from next in IO.lift(() => Versions(location.Stem, fresh, entries))
        from placed in IO.lift(() => Named(location.Folder, location.Stem, next, names).Bind(named => Plan(named, path => overwrite || !Path.Exists(path))))
        select (location.Folder, placed);

    // --- [RESOLUTION]
    public static IO<Seq<(OutputName Name, OutputPath Path)>> Preview(RhinoDoc document, OutputFolder folder, Stem stem, bool overwrite, Seq<OutputName> names) =>
        Planned(document, folder, stem, overwrite, names).Map(static planned => planned.Placed);

    public static IO<Seq<(OutputName Name, OutputPath Path)>> Resolve(RhinoDoc document, OutputFolder folder, Stem stem, bool overwrite, Seq<OutputName> names) =>
        from planned in Planned(document, folder, stem, overwrite, names)
        from _ in unless(planned.Placed.IsEmpty, FilePublication.Refusing(planned.Folder, RhinoReason.Unwritable, IO.lift(() => ignore(Directory.CreateDirectory(planned.Folder)))))
        select planned.Placed;
}
