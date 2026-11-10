using Rasm.Rhino.Persistence;
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

public record ArchiveFrame<T>(
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
        from cleared in IO.lift(() => ignore(doc.RuntimeData.Remove(this)))
        from stored in AttachedData.Read(reader, TypeCode, Codec)
        from restored in stored.Switch(
            (Frame: this, Doc: doc),
            decoded: static (state, decoded) => state.Frame.Restore(state.Doc, decoded.Value),
            absent: static (_, _) => IO.pure(unit),
            newer: static (state, newer) => IO.lift(() => { state.Doc.RuntimeData[state.Frame] = newer.Raw; }))
        select restored;

    private IO<Option<ArchivableDictionary>> Newer(RhinoDoc doc) =>
        IO.lift(() => Optional(doc.RuntimeData.TryGetValue<ArchivableDictionary>(this)));
}
