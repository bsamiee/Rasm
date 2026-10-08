using System.Runtime.CompilerServices;
using Rhino;
using Rhino.Collections;
using Rhino.FileIO;
using Rhino.PlugIns;

namespace Rasm.Rhino.Document.Files;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct FileTypeRow(Guid PlugInId, string Description, Seq<string> Extensions) {
    public bool Accepts(string path) =>
        Path.GetExtension(path) is var named && Extensions.Exists(extension => string.Equals(extension, named, StringComparison.OrdinalIgnoreCase));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Exchange {
    // --- [FORMATS]
    public static Seq<FileTypeRow> Rows(PlugInInfo info) =>
        toSeq(info.FileTypeDescriptions)
            .Zip(toSeq(info.FileTypeExtensions), (description, patterns) => new FileTypeRow(
                info.Id,
                description,
                toSeq(patterns.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).Map(static pattern => pattern.TrimStart('*')).Strict()))
            .Strict();

    public static bool LoadSilently(PlugInInfo info) => info.IsLoadProtected(out bool loadSilently) && loadSilently;

    public static IO<Seq<FileTypeRow>> FileTypes(PlugInType direction) =>
        IO.lift(() => toSeq(PlugIn.GetInstalledPlugIns().Keys)
            .Choose(static id => Optional(PlugIn.GetPlugInInfo(id)))
            .Filter(info => info.PlugInType.HasFlag(direction) && LoadSilently(info))
            .Bind(Rows)
            .Strict());

    // --- [PATHS]
    public static Fin<string> QualifiedPath(string path) => Path.IsPathFullyQualified(path) ? path : new UnqualifiedPath(path);

    public static Fin<string> ExistingPath(string path) => QualifiedPath(path).Bind(static qualified => File.Exists(qualified) ? Fin.Succ(qualified) : new FileMissing(qualified));

    public static Fin<string> ExistingFolder(string path) => QualifiedPath(path).Bind(static qualified => Directory.Exists(qualified) ? Fin.Succ(qualified) : new FolderMissing(qualified));

    // --- [READS]
    public static IO<Committed<Unit>> Import(RhinoDoc doc, string path, RedrawPolicy redraw, Option<ArchivableDictionary> options) =>
        Recorded(doc, path, redraw, existing => IO.lift(() => Refused.Unless(doc.Import(existing, options.ValueUnsafe()), nameof(RhinoDoc.Import))));

    public static IO<Committed<Unit>> Import<TOptions>(
        RhinoDoc doc, string path, RedrawPolicy redraw, Func<string, RhinoDoc, TOptions, bool> reader, Func<FileReadOptions, TOptions> options,
        [CallerArgumentExpression(nameof(reader))] string member = "") =>
        Recorded(doc, path, redraw, existing => Scoped(
            static () => new FileReadOptions { ImportMode = true, BatchMode = true },
            carrier => Refused.Unless(reader(existing, doc, options(carrier)), member)));

    public static IO<Committed<Unit>> ReadFile(string path, RedrawPolicy redraw, Action<FileReadOptions> mode) =>
        Documents.WithDocument(new DocumentSource.Active(), active => Recorded(active, path, redraw, existing => Scoped(
            static () => new FileReadOptions { BatchMode = true },
            carrier => {
                mode(carrier);
                return Refused.Unless(RhinoDoc.ReadFile(existing, carrier), nameof(RhinoDoc.ReadFile));
            })));

    // --- [WRITES]
    public static IO<Unit> Export<TOptions>(
        RhinoDoc doc, string path, Func<string, RhinoDoc, TOptions, bool> writer, Func<FileWriteOptions, TOptions> options,
        [CallerArgumentExpression(nameof(writer))] string member = "") =>
        Written(doc, path, (target, carrier) => Refused.Unless(writer(target, doc, options(carrier)), member));

    public static IO<Unit> Export<TOptions>(
        RhinoDoc doc, string path, Func<string, RhinoDoc, TOptions, WriteFileResult> writer, Func<FileWriteOptions, TOptions> options,
        [CallerArgumentExpression(nameof(writer))] string member = "") =>
        Written(doc, path, (target, carrier) => FromWriteFileResult(writer(target, doc, options(carrier)), member));

    public static IO<Unit> WriteFile(RhinoDoc doc, string path, Action<FileWriteOptions> channels) =>
        Written(doc, path, (target, carrier) => {
            channels(carrier);
            return Refused.Unless(doc.WriteFile(target, carrier), nameof(RhinoDoc.WriteFile));
        });

    public static IO<Unit> WriteMultipleObjects(string path, Seq<GeometryBase> geometry) =>
        IO.lift(() => QualifiedPath(path).Bind(target => Refused.Unless(File3dm.WriteMultipleObjects(target, geometry), nameof(File3dm.WriteMultipleObjects))));

    public static IO<Option<string>> Save(RhinoDoc doc) =>
        IO.lift(() => doc.Modified || doc.IsHeadless
                ? Writable(doc, doc.Path).Bind(path => Refused.Unless(doc.Save(), Some(path), nameof(RhinoDoc.Save)))
                : Fin.Succ(Option<string>.None))
            .Catch(static error => error.HasException<InvalidOperationException>(), static _ => IO.fail<Option<string>>(new DocumentUnsaved()));

    // --- [SCOPES]
    private static IO<Committed<Unit>> Recorded(RhinoDoc doc, string path, RedrawPolicy redraw, Func<string, IO<Unit>> read) =>
        IO.lift(() => ExistingPath(path)).Bind(existing => Commits.Commit(doc, Path.GetFileName(existing), redraw, read(existing)));

    private static IO<Unit> Written(RhinoDoc doc, string path, Func<string, FileWriteOptions, Fin<Unit>> write) =>
        IO.lift(() => QualifiedPath(path).Bind(target => Writable(doc, target)))
            .Bind(target => Scoped(static () => new FileWriteOptions { SuppressAllInput = true, SuppressDialogBoxes = true }, carrier => write(target, carrier)));

    private static IO<Unit> Scoped<TCarrier>(Func<TCarrier> acquire, Func<TCarrier, Fin<Unit>> call) where TCarrier : IDisposable =>
        use(acquire).Bind(carrier => IO.lift(() => call(carrier))).Bracket();

    // --- [ANSWERS]
    private static Fin<string> Writable(RhinoDoc doc, string target) =>
        doc.IsReadOnly && string.Equals(target, doc.Path, StringComparison.OrdinalIgnoreCase) ? new DocumentReadOnly(target) : target;

    private static Fin<Unit> FromWriteFileResult(WriteFileResult result, string member) =>
        result switch {
            WriteFileResult.Success => unit,
            WriteFileResult.Cancel => Errors.Cancelled,
            WriteFileResult.Failure => new Refused(member),
        };
}
