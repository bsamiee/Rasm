using System.Collections.Generic;
using Rasm.Rhino.Document.Files;
using Rhino;
using Rhino.Render;

namespace Rasm.Rhino.Render.Content;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SerializerRow(FileExtension FileExtension, RenderContentKind ContentKind, string EnglishDescription) {
    public Option<Func<string, RenderContent?>> Read { get; init; }
    public Option<Func<string, RenderContent, Option<CreatePreviewEventArgs>, IO<Unit>>> Write { get; init; }
    public Option<Func<string, RenderContentKind, Option<RhinoDoc>, Seq<Func<RenderContent?>>>> Load { get; init; }

    internal string Extension => ((string)FileExtension).TrimStart(FileExtension.Mark);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DefinedContentSerializer(SerializerRow row)
    : RenderContentSerializer(row.Extension, row.ContentKind, row.Read.IsSome, row.Write.IsSome) {
    // --- [REGISTRATION]
    public static IO<Unit> Register(SerializerRow row) =>
        from serializer in IO.lift(() => new DefinedContentSerializer(row))
        from registered in Products.Accepted(() => serializer.RegisterSerializer(Session.PlugIn.Id))
            .Catch(error => IO.lift(() => Internals.Destroy(serializer)) >> IO.fail<Unit>(error))
        select registered;

    // --- [DESCRIPTIONS]
    public override string EnglishDescription => row.EnglishDescription;

    public override string LocalDescription => LocalText.Of(row.EnglishDescription).Local;

    // --- [CALLBACKS]
    public override RenderContent? Read(string pathToFile) =>
        Session.Run(
            row.Read.Map(read => OnFile(pathToFile, RhinoReason.Unreadable, Products.Transfer(() => read(pathToFile), static created =>
                IO.lift(() => RhinoFault.Unless(Internals.RuntimeSerialNumber(created) > 0, RhinoReason.Invalid, created.Id).Map(_ => (RenderContent?)Adopted(created))))))
                .IfNone(IO.pure<RenderContent?>(value: null)),
            failed: null);

    public override bool Write(string pathToFile, RenderContent renderContent, CreatePreviewEventArgs? previewArgs) =>
        Session.Run(
            row.Write.Map(write => OnFile(pathToFile, RhinoReason.Unwritable, write(pathToFile, renderContent, Optional(previewArgs))).Map(static _ => true)).IfNone(IO.pure(value: false)),
            failed: false);

    public override bool CanLoadMultiple() => row.Load.IsSome;

    public override bool LoadMultiple(RhinoDoc? doc, IEnumerable<string> fileNames, RenderContentKind contentKind, LoadMultipleFlags flags) =>
        Session.Run(
            row.Load.Map(load => Products.Each(toSeq(fileNames).Map(path => OnFile(path, RhinoReason.Unreadable, Products.Each(load(path, contentKind, Optional(doc)).Map(make =>
                    Products.Transfer(make, created => IO.lift(() => {
                        if (flags is LoadMultipleFlags.Preload) ReportDeferredContentAndFile(Adopted(created), path, 0);
                        else ReportContentAndFile(Adopted(created), path, 0);
                    }))))))).Map(static _ => true))
                .IfNone(IO.pure(value: false)),
            failed: false);

    // --- [STEPS]
    private static RenderContent Adopted(RenderContent content) {
        Internals.AutoDelete(content, autoDelete: false);
        return content;
    }

    private static IO<T> OnFile<T>(string path, RhinoReason reason, IO<T> run) => run.MapFail(error => new RhinoFault(path, reason, Inner: error));
}
