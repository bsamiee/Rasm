using System.Runtime.CompilerServices;
using Rasm.Rhino.Document.Files;
using Rhino;
using Rasm.Rhino.UI;
using Rhino.Render;

namespace Rasm.Rhino.Render.Content;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ContentLoad(
    Func<Option<RhinoDoc>, RenderContentKind, string, IO<Seq<ContentSource>>> Normal,
    Func<string, IO<ContentSource>> Preload);

public sealed record SerializerRow(
    FileExtension FileExtension,
    RenderContentKind ContentKind,
    string EnglishDescription,
    Option<Func<string, IO<ContentSource>>> Read,
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

    public static Func<IPlugInViews, Action<IDisposable>, IO<Unit>> Register(SerializerRow row) =>
        (sink, _) =>
            DisposalOps.Within(
                () => {
                    DefinedContentSerializer serializer = new(row, sink);
                    return (Serializer: serializer, Ownership: new Disposal<DefinedContentSerializer>(serializer, static held => Destroy(held)));
                },
                held => IO.lift(() => (held.Serializer.RegisterSerializer(sink.PlugIn.Id) ? Fin.Succ(unit) : Error.New($"{nameof(RegisterSerializer)} already holds {held.Serializer.FileExtension}"))
                    .Map(_ => { held.Ownership.Relinquish(); return unit; })).Uninterruptible(),
                static held => IO.lift(held.Ownership.Dispose));

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Destroy")]
    private static extern void Destroy(RenderContentSerializer serializer);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "RuntimeSerialNumber")]
    private static extern ref int RuntimeSerialNumber(RenderContent content);

    // --- [DESCRIPTIONS]
    public override string EnglishDescription => row.EnglishDescription;

    public override string LocalDescription => RowText.Localize(row.EnglishDescription, sink).Local;

    // --- [CALLBACKS]
    public override RenderContent? Read(string pathToFile) =>
        Callbacks.Answer(
            row.Read.Map(read => OnFile(pathToFile, FileReason.Unreadable, DisposalOps.Within<IDisposable, RenderContent?>(retain =>
                read(pathToFile).Bind(source => Contents.FreeFloating(None, source, retain)).Bind(static created => IO.lift(() =>
                    RuntimeSerialNumber(created.Content) > 0
                        ? Fin.Succ<RenderContent?>(Adopted(created))
                        : Error.New($"render content {created.Content.Id} has no runtime serial number")).Uninterruptible()), DisposalOps.Release))),
            static () => null,
            static () => null,
            new CallbackSite(sink, typeof(DefinedContentSerializer), nameof(Read)));

    public override bool Write(string pathToFile, RenderContent renderContent, CreatePreviewEventArgs? previewArgs) =>
        Callbacks.Succeeded(
            row.Write.Map(write => OnFile(pathToFile, FileReason.Unwritable, write(pathToFile, renderContent, Optional(previewArgs)))),
            static () => false,
            new CallbackSite(sink, typeof(DefinedContentSerializer), nameof(Write)));

    public override bool CanLoadMultiple() => row.LoadMultiple.IsSome;

    public override bool LoadMultiple(RhinoDoc? doc, IEnumerable<string> fileNames, RenderContentKind contentKind, LoadMultipleFlags flags) =>
        Callbacks.Succeeded(
            from load in row.LoadMultiple
            select Callbacks.Each(toSeq(fileNames).Map(path => OnFile(path, FileReason.Unreadable, Loaded(load, Optional(doc), contentKind, flags, path)))).Map(unit),
            static () => false,
            new CallbackSite(sink, typeof(DefinedContentSerializer), nameof(LoadMultiple)));

    // --- [STEPS]
    private IO<Unit> Loaded(ContentLoad load, Option<RhinoDoc> document, RenderContentKind kind, LoadMultipleFlags flags, string path) =>
        flags switch {
            LoadMultipleFlags.Normal =>
                from sources in load.Normal(document, kind, path)
                from _ in Callbacks.Each(sources.Map(source => Reported(document, source, content => ReportContentAndFile(content, path, 0))))
                select unit,
            LoadMultipleFlags.Preload =>
                from source in load.Preload(path)
                from _ in Reported(document, source, content => ReportDeferredContentAndFile(content, path, 0))
                select unit,
        };

    private static IO<Unit> Reported(Option<RhinoDoc> document, ContentSource source, Action<RenderContent> report) =>
        DisposalOps.Within<IDisposable, Unit>(retain => Contents.FreeFloating(document, source, retain).Bind(created =>
            IO.lift(() => { report(created.Content); _ = Adopted(created); }).Uninterruptible()), DisposalOps.Release);

    private static RenderContent Adopted((RenderContent Content, Disposal<RenderContent> Ownership) created) {
        Contents.AutoDelete(created.Content, autoDelete: false);
        created.Ownership.Relinquish();
        return created.Content;
    }

    private static IO<T> OnFile<T>(string path, FileReason reason, IO<T> run) =>
        run.MapFail(error => new FileRefused(path, reason, error));
}
