using Rasm.Rhino.Document.Files;
using Rhino;
using Rhino.PlugIns;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Render.Content;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ContentTypeState(Guid Id, Guid PlugInId);

public sealed record ContentLoad(
    Func<Option<RhinoDoc>, RenderContentKind, string, IO<Seq<RenderContent>>> Normal,
    Func<string, IO<RenderContent>> Preload);

public sealed record SerializerRow(
    FileExtension FileExtension,
    RenderContentKind ContentKind,
    string EnglishDescription,
    Option<Func<string, IO<RenderContent>>> Read,
    Option<Func<string, RenderContent, Option<CreatePreviewEventArgs>, IO<Unit>>> Write,
    Option<ContentLoad> LoadMultiple);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DefinedContentSerializer : RenderContentSerializer {
    // --- [REGISTRATION]
    private readonly SerializerRow row;
    private readonly IPlugInSink sink;

    private DefinedContentSerializer(SerializerRow row, IPlugInSink sink)
        : base(((string)row.FileExtension)[1..], row.ContentKind, row.Read.IsSome, row.Write.IsSome) =>
        (this.row, this.sink) = (row, sink);

    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register(SerializerRow row) =>
        (plugIn, sink) =>
            from serializer in IO.lift(() => new DefinedContentSerializer(row, sink))
            from _ in IO.lift(() => Taken.Unless(serializer.RegisterSerializer(plugIn.Id), nameof(RegisterSerializer), serializer.FileExtension))
            select Thinktecture.Empty.Disposable();

    // --- [DESCRIPTIONS]
    public override string EnglishDescription => row.EnglishDescription;

    public override string LocalDescription => RowText.Localize(row.EnglishDescription, table: Some<object>(sink)).Local;

    // --- [CALLBACKS]
    public override RenderContent? Read(string pathToFile) =>
        Callbacks.Answer(
            row.Read.Map(read => OnFile(pathToFile, read).Map(static content => (RenderContent?)content)),
            static () => null,
            static () => null,
            new CallbackSite(sink, GetType(), nameof(Read)));

    public override bool Write(string pathToFile, RenderContent renderContent, CreatePreviewEventArgs? previewArgs) =>
        Callbacks.Succeeded(
            row.Write.Map(write => OnFile(pathToFile, path => write(path, renderContent, Optional(previewArgs)))),
            static () => false,
            new CallbackSite(sink, GetType(), nameof(Write)));

    public override bool CanLoadMultiple() => row.LoadMultiple.IsSome;

    public override bool LoadMultiple(RhinoDoc? doc, IEnumerable<string> fileNames, RenderContentKind contentKind, LoadMultipleFlags flags) =>
        Callbacks.Succeeded(
            from load in row.LoadMultiple
            select Callbacks.Each(toSeq(fileNames).Map(path => OnFile(path, file => Loaded(load, Optional(doc), contentKind, flags, file)))).Map(static _ => unit),
            static () => false,
            new CallbackSite(sink, GetType(), nameof(LoadMultiple)));

    // --- [STEPS]
    private IO<Unit> Loaded(ContentLoad load, Option<RhinoDoc> document, RenderContentKind kind, LoadMultipleFlags flags, string path) =>
        flags switch {
            LoadMultipleFlags.Normal =>
                from contents in load.Normal(document, kind, path)
                from _ in IO.lift(() => contents.Iter(content => ReportContentAndFile(content, path, 0)))
                select unit,
            LoadMultipleFlags.Preload =>
                from content in load.Preload(path)
                from _ in IO.lift(() => ReportDeferredContentAndFile(content, path, 0))
                select unit,
        };

    private static IO<T> OnFile<T>(string path, Func<string, IO<T>> run) =>
        IO.pure(path).Bind(run).MapFail(error => new ContentFileFailed(path, error));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class ContentTypeMapper {
    [MapperIgnoreSource(nameof(RenderContentType.InternalName), Justification = "No reader of a registered type's internal name")]
    [MapperIgnoreSource(nameof(RenderContentType.RenderEngineId), Justification = "No reader narrows registered types by engine")]
    internal static partial ContentTypeState ToState(RenderContentType type);
}

public static class ContentRegistry {
    public static IO<Seq<ContentTypeState>> Types { get; } =
        IO.lift(static () => toSeq(RenderContentType.GetAllAvailableTypes()))
            .Bracket(Use: static types => IO.lift(() => types.Map(ContentTypeMapper.ToState).Strict()), Fin: DisposalOps.Release);
}
