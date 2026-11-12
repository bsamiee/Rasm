using System.Reflection;
using System.Runtime.CompilerServices;
using Rhino;
using Rhino.Collections;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.PlugIns;

namespace Rasm.Rhino.Document.Files;

// --- [MODELS] --------------------------------------------------------------------------

[ValueObject<string>(SkipFactoryMethods = true)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class InputPath {
    public static Fin<InputPath> Of(string path) => Existing(path, File.Exists).Map(static held => new InputPath(held));

    internal static Fin<string> Existing(string path, Func<string, bool> exists) =>
        path switch {
            _ when !Path.IsPathFullyQualified(path) => new RhinoFault(path, RhinoReason.Unqualified),
            _ when !exists(path) => new RhinoFault(path, RhinoReason.Missing),
            _ => path,
        };
}

[ValueObject<string>(SkipFactoryMethods = true)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FolderPath {
    public static Fin<FolderPath> Of(string path) => InputPath.Existing(path, Directory.Exists).Map(static held => new FolderPath(held));

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

public sealed record FileTypeRow(Guid PlugInId, string Description, Seq<string> Extensions) {
    public bool Accepts(string path) =>
        Path.GetExtension(path) is var named && Extensions.Exists(extension => string.Equals(extension, named, StringComparison.OrdinalIgnoreCase));

    public static Seq<FileTypeRow> Of(PlugInInfo info) =>
        toSeq(info.FileTypeDescriptions)
            .Zip(toSeq(info.FileTypeExtensions), (description, patterns) => new FileTypeRow(
                info.Id,
                description,
                toSeq(patterns.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).Map(static pattern => pattern.TrimStart('*')).Strict()))
            .Strict();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StepOutcome {
    public sealed record Imported(Seq<Guid> Objects) : StepOutcome;

    public sealed record Written(string Path, ContentHash Content) : StepOutcome;

    public sealed record Edited : StepOutcome;

    public sealed record Unchanged : StepOutcome;

    public static IO<StepOutcome> Import<T>(RhinoDoc document, IO<T> import) =>
        from next in IO.lift(static () => RhinoObject.NextRuntimeSerialNumber)
        from _ in import
        from added in IO.lift(() => Conversions.Rows(document.Objects.AllObjectsSince(next - 1u)).Map(static item => item.Id).Strict())
        select (StepOutcome)new Imported(added);

    public static IO<StepOutcome> Write(string path, Func<string, IO<Unit>> write) => Save(write(path).Map(_ => Some(path)));

    public static IO<StepOutcome> Save(IO<Option<string>> save) =>
        save.Bind(static written => IO.lift(() => written.Match<StepOutcome>(Some: static path => new Written(path, ContentHash.CreateFromFile(path)), None: static () => new Unchanged())));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Exchange {

    // --- [FORMATS]

    public static IO<Seq<FileTypeRow>> FileTypes(PlugInType direction) =>
        IO.lift(() => toSeq(PlugIn.GetInstalledPlugIns().Keys)
            .Choose(static id => Optional(PlugIn.GetPlugInInfo(id)))
            .Filter(info => {
                _ = info.IsLoadProtected(out bool loadSilently);
                return info.PlugInType.HasFlag(direction) && loadSilently;
            })
            .Bind(FileTypeRow.Of)
            .Strict());

    // --- [READS]

    public static IO<Committed<Unit>> Import(RhinoDoc doc, InputPath path, RedrawPolicy redraw, Option<ArchivableDictionary> options) =>
        Recorded(doc, path, redraw, Products.Accepted(() => doc.Import(path, options.ValueUnsafe())));

    public static IO<Committed<Unit>> Import<TOptions>(
        RhinoDoc doc, InputPath path, RedrawPolicy redraw, Func<string, RhinoDoc, TOptions, bool> reader, Func<FileReadOptions, TOptions> options,
        [CallerArgumentExpression(nameof(reader))] string member = "") =>
        Recorded(doc, path, redraw, Using(static () => new FileReadOptions { ImportMode = true, BatchMode = true }, fileOptions => Products.Accepted(() => reader(path, doc, options(fileOptions)), member)));

    public static IO<Committed<Unit>> ReadFile(InputPath path, RedrawPolicy redraw, Action<FileReadOptions> mode) =>
        Documents.With(new DocumentSource.Active(), active => Recorded(active, path, redraw,
            Using(static () => new FileReadOptions { BatchMode = true }, fileOptions => IO.lift(() => mode(fileOptions)) >> Products.Accepted(() => RhinoDoc.ReadFile(path, fileOptions)))));

    // --- [WRITES]

    public static IO<Unit> Export<TOptions>(
        RhinoDoc doc, OutputPath path, Func<string, RhinoDoc, TOptions, WriteFileResult> writer, Func<FileWriteOptions, TOptions> options,
        [CallerArgumentExpression(nameof(writer))] string member = "") =>
        Written(doc, path, fileOptions => IO.lift(() => writer(path, doc, options(fileOptions)) switch {
            WriteFileResult.Success => Fin.Succ(unit),
            WriteFileResult.Cancel => Errors.Cancelled,
            WriteFileResult.Failure => new RhinoFault(member, RhinoReason.Declined),
        }));

    public static IO<Unit> WriteFile(RhinoDoc doc, OutputPath path, Action<FileWriteOptions> options) =>
        Written(doc, path, fileOptions => IO.lift(() => options(fileOptions)) >> Products.Accepted(() => doc.WriteFile(path, fileOptions)));

    public static IO<Option<string>> Save(RhinoDoc doc) =>
        IO.lift(() => doc.Modified || doc.IsHeadless).Bind(pending => pending
            ? from titled in IO.lift(() => Conversions.Required(Conversions.Present(doc.Path)))
              from writable in IO.lift(Writable(doc, titled))
              from saved in Products.Accepted(doc.Save)
              select Some(titled)
            : IO.pure(Option<string>.None));

    // --- [OPTIONS]

    private static IO<Committed<Unit>> Recorded(RhinoDoc doc, InputPath path, RedrawPolicy redraw, IO<Unit> read) =>
        Commits.Commit(doc, Path.GetFileName(path), redraw, read);

    private static IO<Unit> Written(RhinoDoc doc, OutputPath path, Func<FileWriteOptions, IO<Unit>> write) =>
        IO.lift(() => Writable(doc, path)) >> Using(static () => new FileWriteOptions { SuppressAllInput = true, SuppressDialogBoxes = true }, write);

    private static IO<Unit> Using<TOptions>(Func<TOptions> acquire, Func<TOptions, IO<Unit>> call) where TOptions : IDisposable =>
        use(acquire).Bind(call).Bracket();

    private static Fin<Unit> Writable(RhinoDoc doc, string target) =>
        RhinoFault.Unless(!doc.IsReadOnly || !string.Equals(target, doc.Path, StringComparison.OrdinalIgnoreCase), RhinoReason.Unwritable, target);
}
