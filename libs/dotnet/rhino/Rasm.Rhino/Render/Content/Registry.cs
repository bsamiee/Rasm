using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rasm.RhinoCommon.Document.Files;
using Rasm.RhinoCommon.UI;
using Rhino;
using Rhino.Render;

namespace Rasm.RhinoCommon.Render.Content;

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
    private readonly IPlugInViews owner;

    private DefinedContentSerializer(SerializerRow row, IPlugInViews owner)
        : base(((string)row.FileExtension)[1..], row.ContentKind, row.Read.IsSome, row.Write.IsSome) =>
        (this.row, this.owner) = (row, owner);

    public static Func<IPlugInViews, IO<Unit>> Register(SerializerRow row) =>
        owner =>
            from serializer in IO.lift(() => new DefinedContentSerializer(row, owner))
            from registered in IO.lift(() => RhinoFault.Unless(serializer.RegisterSerializer(owner.PlugIn.Id)))
                .Catch(error => IO.lift(() => Destroy(serializer)) >> IO.fail<Unit>(error))
            select registered;

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Destroy")]
    private static extern void Destroy(RenderContentSerializer serializer);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "RuntimeSerialNumber")]
    private static extern ref int RuntimeSerialNumber(RenderContent content);

    // --- [DESCRIPTIONS]
    public override string EnglishDescription => row.EnglishDescription;

    public override string LocalDescription => RowText.Localize(row.EnglishDescription, owner).Local;

    // --- [CALLBACKS]
    public override RenderContent? Read(string pathToFile) =>
        owner.Answer(
            row.Read.Map(read => OnFile(pathToFile, RhinoReason.Unreadable, read(pathToFile).Bind(static source => Contents.Handed(None, source, static created =>
                IO.lift(() => RhinoFault.Unless(RuntimeSerialNumber(created) > 0).Map(_ => (RenderContent?)Adopted(created)))))))
                .IfNone(IO.pure<RenderContent?>(value: null)),
            failed: null);

    public override bool Write(string pathToFile, RenderContent renderContent, CreatePreviewEventArgs? previewArgs) =>
        owner.Answer(
            row.Write.Map(write => OnFile(pathToFile, RhinoReason.Unwritable, write(pathToFile, renderContent, Optional(previewArgs))).Map(value: true)).IfNone(IO.pure(value: false)),
            failed: false);

    public override bool CanLoadMultiple() => row.LoadMultiple.IsSome;

    public override bool LoadMultiple(RhinoDoc? doc, IEnumerable<string> fileNames, RenderContentKind contentKind, LoadMultipleFlags flags) =>
        owner.Answer(
            row.LoadMultiple.Map(load => Callbacks.Each(toSeq(fileNames).Map(path => OnFile(path, RhinoReason.Unreadable, Loaded(load, Optional(doc), contentKind, flags, path)))).Map(value: true))
                .IfNone(IO.pure(value: false)),
            failed: false);

    // --- [STEPS]
    private IO<Unit> Loaded(ContentLoad load, Option<RhinoDoc> document, RenderContentKind kind, LoadMultipleFlags flags, string path) =>
        flags switch {
            LoadMultipleFlags.Normal =>
                from sources in load.Normal(document, kind, path)
                from reported in Callbacks.Each(sources.Map(source => Reported(document, source, content => ReportContentAndFile(content, path, 0))))
                select unit,
            LoadMultipleFlags.Preload =>
                from source in load.Preload(path)
                from reported in Reported(document, source, content => ReportDeferredContentAndFile(content, path, 0))
                select unit,
        };

    private static IO<Unit> Reported(Option<RhinoDoc> document, ContentSource source, Action<RenderContent> report) =>
        Contents.Handed(document, source, created => IO.lift(() => {
            report(created);
            _ = Adopted(created);
        }));

    private static RenderContent Adopted(RenderContent content) {
        Contents.AutoDelete(content, autoDelete: false);
        return content;
    }

    private static IO<T> OnFile<T>(string path, RhinoReason reason, IO<T> run) =>
        run.MapFail(error => new RhinoFault(path, reason, Inner: error));
}
