using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.Persistence;
using Rasm.RhinoCommon.UI;
using Rhino;
using Rhino.Collections;
using Rhino.FileIO;

namespace Rasm.RhinoCommon.PlugIns;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record FrameKey(uint TypeCode);

public abstract record DocumentFrame {
    internal abstract IO<bool> Filled(RhinoDoc doc, FileWriteOptions options);

    internal abstract IO<Unit> Write(RhinoDoc doc, BinaryArchiveWriter writer, FileWriteOptions options);

    internal abstract IO<Unit> Read(RhinoDoc doc, BinaryArchiveReader reader);

    public sealed record Of<T>(
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

        private FrameKey Key => new(TypeCode);

        private IO<Option<ArchivableDictionary>> Newer(RhinoDoc doc) =>
            IO.lift(() => Optional(doc.RuntimeData.TryGetValue<ArchivableDictionary>(Key)));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class DocumentArchive {
    // --- [CALLBACKS]
    public static IO<bool> ShouldWrite(Seq<DocumentFrame> frames, FileWriteOptions options) =>
        IO.lift(() => Optional(options.RhinoDoc).Filter(_ => !options.WriteGeometryOnly && options.WriteUserData && !options.WriteSelectedObjectsOnly))
            .Bind(saved => saved.Match(
                Some: doc => Callbacks.Each(frames.Map(frame => frame.Filled(doc, options))).Map(static filled => filled.Exists(static held => held)),
                None: static () => IO.pure(value: false)));

    public static IO<Unit> Write(Seq<DocumentFrame> frames, RhinoDoc doc, BinaryArchiveWriter writer, FileWriteOptions options) =>
        frames.TraverseM(frame => frame.Write(doc, writer, options)).As().Map(unit);

    public static IO<Unit> Read(Seq<DocumentFrame> frames, RhinoDoc doc, BinaryArchiveReader reader, FileReadOptions options) =>
        IO.lift(() => options.OpenMode || options.NewMode)
            .Bind(opened => when(opened, frames.TraverseM(frame => frame.Read(doc, reader)).As().Map(unit)).As());

    // --- [SAVES]
    public static IO<Unit> Rewrites(IPlugInViews plugIn) =>
        EventKind.BeginSaveDocument.Inline(static args => Optional(args.Document).Traverse(ArchivableDictionaries.Rewrite).As().Map(unit), plugIn);
}
