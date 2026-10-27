using System.Reflection;
using System.Runtime.CompilerServices;
using Rhino;
using Rhino.Collections;
using Rhino.FileIO;
using Rhino.PlugIns;

namespace Rasm.RhinoCommon.Document.Files;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(SkipFactoryMethods = true)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class InputPath {
    public static Fin<InputPath> Of(string path) =>
        path switch {
            _ when !Path.IsPathFullyQualified(path) => new RhinoFault(path, RhinoReason.Unqualified),
            _ when !File.Exists(path) => new RhinoFault(path, RhinoReason.Missing),
            _ => new InputPath(path),
        };
}

[ValueObject<string>(SkipFactoryMethods = true)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FolderPath {
    public static Fin<FolderPath> Of(string path) =>
        path switch {
            _ when !Path.IsPathFullyQualified(path) => new RhinoFault(path, RhinoReason.Unqualified),
            _ when !Directory.Exists(path) => new RhinoFault(path, RhinoReason.Missing),
            _ => new FolderPath(path),
        };

    public static Fin<FolderPath> Installed(Assembly assembly) =>
        Optional(Path.GetDirectoryName(assembly.Location)).ToFin(new RhinoFault(assembly.Location, RhinoReason.Missing)).Bind(Of);
}

[ValueObject<string>(SkipFactoryMethods = true)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OutputPath {
    public string Folder => Path.GetDirectoryName(_value.AsSpan()).ToString();

    public static Fin<OutputPath> Of(string path) =>
        Path.IsPathFullyQualified(path) && !Path.GetFileName(path.AsSpan()).IsEmpty ? new OutputPath(path) : new RhinoFault(path, RhinoReason.Unqualified);
}

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

    public static bool LoadSilently(PlugInInfo info) {
        _ = info.IsLoadProtected(out bool loadSilently);
        return loadSilently;
    }

    public static IO<Seq<FileTypeRow>> FileTypes(PlugInType direction) =>
        IO.lift(() => toSeq(PlugIn.GetInstalledPlugIns().Keys)
            .Choose(static id => Optional(PlugIn.GetPlugInInfo(id)))
            .Filter(info => info.PlugInType.HasFlag(direction) && LoadSilently(info))
            .Bind(Rows)
            .Strict());

    // --- [READS]
    public static IO<Committed<Unit>> Import(RhinoDoc doc, InputPath path, RedrawPolicy redraw, Option<ArchivableDictionary> options) =>
        Recorded(doc, path, redraw, IO.lift(() => RhinoFault.Unless(doc.Import(path, options.ValueUnsafe()))));

    public static IO<Committed<Unit>> Import<TOptions>(
        RhinoDoc doc, InputPath path, RedrawPolicy redraw, Func<string, RhinoDoc, TOptions, bool> reader, Func<FileReadOptions, TOptions> options,
        [CallerArgumentExpression(nameof(reader))] string call = "") =>
        Recorded(doc, path, redraw, Scoped(
            static () => new FileReadOptions { ImportMode = true, BatchMode = true },
            carrier => RhinoFault.Unless(reader(path, doc, options(carrier)), call: call)));

    public static IO<Committed<Unit>> ReadFile(InputPath path, RedrawPolicy redraw, Action<FileReadOptions> mode) =>
        Documents.WithDocument(new DocumentSource.Active(), active => Recorded(active, path, redraw, Scoped(
            static () => new FileReadOptions { BatchMode = true },
            carrier => {
                mode(carrier);
                return RhinoFault.Unless(RhinoDoc.ReadFile(path, carrier));
            })));

    // --- [WRITES]
    public static IO<Unit> Export<TOptions>(
        RhinoDoc doc, OutputPath path, Func<string, RhinoDoc, TOptions, bool> writer, Func<FileWriteOptions, TOptions> options,
        [CallerArgumentExpression(nameof(writer))] string call = "") =>
        Written(doc, path, carrier => RhinoFault.Unless(writer(path, doc, options(carrier)), call: call));

    public static IO<Unit> Export<TOptions>(
        RhinoDoc doc, OutputPath path, Func<string, RhinoDoc, TOptions, WriteFileResult> writer, Func<FileWriteOptions, TOptions> options,
        [CallerArgumentExpression(nameof(writer))] string call = "") =>
        Written(doc, path, carrier => writer(path, doc, options(carrier)) switch {
            WriteFileResult.Success => unit,
            WriteFileResult.Cancel => Errors.Cancelled,
            WriteFileResult.Failure => new RhinoFault(call, RhinoReason.Declined),
        });

    public static IO<Unit> WriteFile(RhinoDoc doc, OutputPath path, Action<FileWriteOptions> channels) =>
        Written(doc, path, carrier => {
            channels(carrier);
            return RhinoFault.Unless(doc.WriteFile(path, carrier));
        });

    public static IO<Unit> WriteMultipleObjects(OutputPath path, Seq<GeometryBase> geometry) =>
        IO.lift(() => RhinoFault.Unless(File3dm.WriteMultipleObjects(path, geometry)));

    public static IO<Option<string>> Save(RhinoDoc doc) =>
        IO.lift(() => doc.Modified || doc.IsHeadless
                ? Writable(doc, doc.Path).Bind(_ => RhinoFault.Unless(doc.Save())).Map(_ => Some(doc.Path))
                : Option<string>.None);

    // --- [SCOPES]
    private static IO<Committed<Unit>> Recorded(RhinoDoc doc, InputPath path, RedrawPolicy redraw, IO<Unit> read) =>
        Commits.Commit(doc, Path.GetFileName(path), redraw, read);

    private static IO<Unit> Written(RhinoDoc doc, OutputPath path, Func<FileWriteOptions, Fin<Unit>> write) =>
        IO.lift(() => Writable(doc, path)).Bind(_ => Scoped(static () => new FileWriteOptions { SuppressAllInput = true, SuppressDialogBoxes = true }, write));

    private static IO<Unit> Scoped<TCarrier>(Func<TCarrier> acquire, Func<TCarrier, Fin<Unit>> call) where TCarrier : IDisposable =>
        use(acquire).Bind(carrier => IO.lift(() => call(carrier))).Bracket();

    // --- [ANSWERS]
    private static Fin<Unit> Writable(RhinoDoc doc, string target) =>
        doc.IsReadOnly && string.Equals(target, doc.Path, StringComparison.OrdinalIgnoreCase) ? new RhinoFault(target, RhinoReason.Unwritable) : unit;
}
