using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render;
using Rasm.Rhino.Render.Queue;
using Rhino;
using Rhino.FileIO;

namespace Rasm.Rhino.Plugin;

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
    internal override IO<bool> Filled(RhinoDoc doc, FileWriteOptions options) =>
        Held(doc, options).Map(held => !held.Equals(Empty));

    internal override IO<Unit> Write(RhinoDoc doc, BinaryArchiveWriter writer, FileWriteOptions options) =>
        Held(doc, options).Bind(held => AttachedData.Write(writer, TypeCode, Codec, held));

    internal override IO<Unit> Read(RhinoDoc doc, BinaryArchiveReader reader) =>
        AttachedData.Read(reader, TypeCode, Codec).Bind(read => Restore(doc, read));
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

    private static DocumentFrame Locks { get; } = new DocumentFrame<HashMap<Guid, LanguageExt.HashSet<EntryKey>>>(
        LockSets.TypeCode,
        LockSets.Codec,
        HashMap<Guid, LanguageExt.HashSet<EntryKey>>(),
        static (doc, _) => LockSets.Registry(doc).Map(static registry => registry.ToHashMap()),
        static (doc, locks) => LockSets.Registry(doc).Bind(registry => IO.lift(() => registry.AddOrUpdateRange(locks.AsIterable()))));

    private static DocumentFrame Sets { get; } = new DocumentFrame<SetBook>(
        SetBook.TypeCode,
        SetBook.Codec,
        SetBook.Empty,
        static (doc, _) => RenderSets.Book(doc),
        static (doc, book) => RenderSets.Held(doc).Bind(held => held.SwapIO(_ => book)).Map(static _ => unit));

    private static DocumentFrame Queue { get; } = new DocumentFrame<QueueBook>(
        QueueBook.TypeCode,
        QueueBook.Codec,
        QueueBook.Empty,
        static (doc, _) => Queues.Book(doc),
        static (doc, book) => Queues.Held(doc).Bind(held => held.SwapIO(_ => book)).Map(static _ => unit));

    private static Seq<DocumentFrame> Chunk(Seq<DocumentFrame> frames) => Seq(Histories, Locks, Sets, Queue).Concat(frames);

    // --- [CALLBACKS]
    internal static bool ShouldWrite(Seq<DocumentFrame> frames, FileWriteOptions options, CallbackSite site) =>
        Callbacks.Answer(
            IO.lift(() => Optional(options.RhinoDoc).Filter(_ => !options.WriteGeometryOnly && options.WriteUserData && !options.WriteSelectedObjectsOnly))
                .Bind(saved => saved.Match(
                    Some: doc => Chunk(frames).TraverseM(frame => frame.Filled(doc, options)).As().Map(static filled => filled.Exists(static held => held)),
                    None: static () => IO.pure(value: false))),
            static () => false,
            site);

    internal static Unit Write(Seq<DocumentFrame> frames, RhinoDoc doc, BinaryArchiveWriter writer, FileWriteOptions options, CallbackSite site) =>
        Callbacks.Answer(Chunk(frames).TraverseM(frame => frame.Write(doc, writer, options)).As().Map(static _ => unit), static () => unit, site);

    internal static Unit Read(Seq<DocumentFrame> frames, RhinoDoc doc, BinaryArchiveReader reader, FileReadOptions options, CallbackSite site) =>
        Callbacks.Answer(
            IO.lift(() => options.OpenMode || options.NewMode)
                .Bind(opened => when(opened, Chunk(frames).TraverseM(frame => frame.Read(doc, reader)).As().Map(static _ => unit)).As()),
            static () => unit,
            site);
}
