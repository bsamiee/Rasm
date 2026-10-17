using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Collections;
using Rhino.FileIO;

namespace Rasm.Rhino.PlugIns;

// --- [MODELS] --------------------------------------------------------------------------
public abstract record DocumentFrame {
    internal abstract IO<bool> Filled(RhinoDoc doc, FileWriteOptions options);

    internal abstract IO<Unit> Write(RhinoDoc doc, BinaryArchiveWriter writer, FileWriteOptions options);

    internal abstract IO<Unit> Read(RhinoDoc doc, BinaryArchiveReader reader);
}

public sealed record DocumentFrame<T>(
    uint TypeCode,
    DictionaryCodec<T> Codec,
    T Empty,
    Func<RhinoDoc, FileWriteOptions, IO<T>> Held,
    Func<RhinoDoc, T, IO<Unit>> Restore) : DocumentFrame where T : IEquatable<T> {
    public IO<Unit> Writable(RhinoDoc doc) =>
        Newer(doc).Bind(newer => newer.Match(Some: raw => IO.fail<Unit>(Codec.Refusal(raw)), None: static () => IO.pure(unit)));

    internal override IO<bool> Filled(RhinoDoc doc, FileWriteOptions options) =>
        from newer in Newer(doc)
        from held in Held(doc, options)
        select newer.IsSome || !held.Equals(Empty);

    internal override IO<Unit> Write(RhinoDoc doc, BinaryArchiveWriter writer, FileWriteOptions options) =>
        Newer(doc).Bind(newer => newer.Match(
            Some: raw => AttachedData.Write(writer, TypeCode, raw),
            None: () => Held(doc, options).Bind(held => AttachedData.Write(writer, TypeCode, Codec, held))));

    internal override IO<Unit> Read(RhinoDoc doc, BinaryArchiveReader reader) =>
        from cleared in IO.lift(() => ignore(doc.RuntimeData.Remove(Key)))
        from stored in AttachedData.Read(reader, TypeCode, Codec)
        from restored in stored.Switch(
            (Frame: this, Doc: doc),
            held: static (state, held) => state.Frame.Restore(state.Doc, held.Value),
            absent: static (_, _) => IO.pure(unit),
            newer: static (state, newer) => IO.lift(() => { state.Doc.RuntimeData[state.Frame.Key] = newer.Raw; }))
        select restored;

    private (Type Frame, uint TypeCode) Key => (typeof(DocumentFrame), TypeCode);

    private IO<Option<ArchivableDictionary>> Newer(RhinoDoc doc) =>
        IO.lift(() => Optional(doc.RuntimeData.TryGetValue<ArchivableDictionary>(Key)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class DocumentArchive {
    // --- [FRAMES]
    private static DocumentFrame Histories { get; } = new DocumentFrame<HashMap<Guid, ValueHistory>>(
        ValueHistory.TypeCode,
        ValueHistory.Codec,
        HashMap<Guid, ValueHistory>(),
        static (doc, options) => options.WriteAsTemplate ? IO.pure(HashMap<Guid, ValueHistory>()) : HistoryScope.Of(doc).Map(static registry => registry.ToHashMap()),
        static (doc, histories) => HistoryScope.Of(doc).Bind(registry => IO.lift(() => registry.AddOrUpdateRange(histories.AsIterable()))));

    // --- [CALLBACKS]
    public static bool ShouldWrite(Seq<DocumentFrame> frames, FileWriteOptions options, CallbackSite site) =>
        Callbacks.Answer(
            IO.lift(() => Optional(options.RhinoDoc).Filter(_ => !options.WriteGeometryOnly && options.WriteUserData && !options.WriteSelectedObjectsOnly))
                .Bind(saved => saved.Match(
                    Some: doc => Callbacks.Each(Histories.Cons(frames).Map(frame => frame.Filled(doc, options))).Map(static filled => filled.Exists(static held => held)),
                    None: static () => IO.pure(value: false))),
            static () => false,
            site);

    public static void Write(Seq<DocumentFrame> frames, RhinoDoc doc, BinaryArchiveWriter writer, FileWriteOptions options, CallbackSite site) =>
        Callbacks.Run(Histories.Cons(frames).TraverseM(frame => frame.Write(doc, writer, options)).As(), site);

    public static void Read(Seq<DocumentFrame> frames, RhinoDoc doc, BinaryArchiveReader reader, FileReadOptions options, CallbackSite site) =>
        Callbacks.Run(
            IO.lift(() => options.OpenMode || options.NewMode)
                .Bind(opened => when(opened, Histories.Cons(frames).TraverseM(frame => frame.Read(doc, reader)).As().Map(unit)).As()),
            site);

    // --- [SAVES]
    public static IO<Unit> Rewrites(IPlugInViews plugIn, Action<IDisposable> retain) =>
        EventKind.BeginSaveDocument.Inline(static args => Optional(args.Document).Traverse(ArchivableDictionaries.Rewrite).As().Map(unit), plugIn, retain);
}
