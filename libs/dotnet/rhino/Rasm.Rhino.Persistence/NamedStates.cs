using Rasm.Rhino.Document;
using Rhino;
using Rhino.Collections;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.DocObjects.SnapShots;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;
using Rhino.PlugIns;
using Rhino.Runtime.InteropWrappers;

namespace Rasm.Rhino.Persistence;

// --- [TYPES] ---------------------------------------------------------------------------
public enum SnapshotOption { Save = 0, Restore = 1, Delete = 2 }

// --- [MODELS] --------------------------------------------------------------------------
public sealed record NamedPosition(Guid Id, string Name, Seq<(Guid ObjectId, Transform Xform)> Objects);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PositionOp {
    public sealed record Restore() : PositionOp;

    public sealed record Update() : PositionOp;

    public sealed record Delete() : PositionOp;

    public sealed record Rename(string Name) : PositionOp;

    public sealed record Append(Seq<Guid> ObjectIds) : PositionOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerStateOp {
    public sealed record Restore(RestoreLayerProperties Properties, Option<Guid> Viewport) : LayerStateOp;

    public sealed record Rename(string NewName) : LayerStateOp;

    public sealed record Delete() : LayerStateOp;
}

public sealed record SnapshotAnimationCallbacks(
    Func<RhinoDoc, int, IO<Unit>> AnimationStart,
    Func<RhinoDoc, BinaryArchiveReader, BinaryArchiveReader, IO<Unit>> PrepareForDocumentAnimation,
    Func<RhinoDoc, BinaryArchiveReader, BinaryArchiveReader, BoundingBox, IO<BoundingBox>> ExtendBoundingBoxForDocumentAnimation,
    Func<RhinoDoc, double, BinaryArchiveReader, BinaryArchiveReader, IO<Unit>> AnimateDocument,
    Func<RhinoDoc, RhinoObject, Transform, BinaryArchiveReader, BinaryArchiveReader, IO<Transform>> PrepareForObjectAnimation,
    Func<RhinoDoc, RhinoObject, Transform, BinaryArchiveReader, BinaryArchiveReader, BoundingBox, IO<BoundingBox>> ExtendBoundingBoxForObjectAnimation,
    Func<RhinoDoc, RhinoObject, Transform, double, BinaryArchiveReader, BinaryArchiveReader, IO<Transform>> AnimateObject,
    Func<RhinoDoc, IO<Unit>> AnimationStop);

public sealed record SnapshotDocumentCallbacks(Func<RhinoDoc, IO<ArchivableDictionary>> Save, Func<RhinoDoc, ChunkFrame, ArchivableDictionary, IO<Unit>> Restore);

public sealed record SnapshotObjectCallbacks(
    Func<RhinoObject, bool> Supports,
    Func<RhinoDoc, RhinoObject, Transform, IO<ArchivableDictionary>> Save,
    Func<RhinoDoc, RhinoObject, Transform, ChunkFrame, ArchivableDictionary, IO<Transform>> Restore);

public sealed record SnapshotCallbacks(Action<Error> Reject, Guid ClientId, string Category, string Name, ChunkFrame Frame, Func<int, int, bool> SupportsVersion) {
    public Option<SnapshotDocumentCallbacks> Document { get; init; }

    public Option<SnapshotObjectCallbacks> Objects { get; init; }

    public Option<Func<RhinoDoc, IO<Unit>>> SnapshotRestored { get; init; }

    public Option<Func<RhinoDoc, RhinoObject, Transform, BinaryArchiveReader, IO<Transform>>> ObjectTransformNotification { get; init; }

    public Option<Func<RhinoDoc, Option<RhinoObject>, BinaryArchiveReader, SimpleArrayBinaryArchiveReader, Option<TextLog>, IO<bool>>> IsCurrentModelStateInAnySnapshot { get; init; }

    public Option<SnapshotAnimationCallbacks> Animation { get; init; }
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class CallbackSnapshotClient : SnapShotsClient {
    protected abstract SnapshotCallbacks Callbacks { get; }

    public sealed override Guid PlugInId() => Optional(PlugIn.Find(GetType().Assembly)).Map(static plugIn => plugIn.Id).IfNone(Guid.Empty);

    public sealed override Guid ClientId() => Callbacks.ClientId;

    public sealed override string Category() => Callbacks.Category;

    public sealed override string Name() => Callbacks.Name;

    public sealed override bool SupportsDocument() => Callbacks.Document.IsSome;

    public sealed override bool SaveDocument(RhinoDoc doc, BinaryArchiveWriter archive) =>
        Succeeded(Callbacks.Document.Map(document => document.Save(doc).Bind(dictionary => AttachedData.WriteChunk(archive, Callbacks.Frame, dictionary))));

    public sealed override bool RestoreDocument(RhinoDoc doc, BinaryArchiveReader archive) =>
        Succeeded(Callbacks.Document.Map(document => Chunk(archive).Bind(read => document.Restore(doc, read.Frame, read.Dictionary))));

    public sealed override bool SupportsObjects() => Callbacks.Objects.IsSome;

    public sealed override bool SupportsObject(RhinoObject doc_object) => Callbacks.Objects.Exists(objects => objects.Supports(doc_object));

    public sealed override bool SaveObject(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveWriter archive) {
        Transform current = transform;
        return Succeeded(Callbacks.Objects.Map(objects => objects.Save(doc, doc_object, current).Bind(dictionary => AttachedData.WriteChunk(archive, Callbacks.Frame, dictionary))));
    }

    public sealed override bool RestoreObject(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveReader archive) =>
        Transformed(ref transform, Callbacks.Objects.Map(objects => fun((Transform current) => Chunk(archive).Bind(read => objects.Restore(doc, doc_object, current, read.Frame, read.Dictionary)))));

    public sealed override void SnapshotRestored(RhinoDoc doc) => _ = Succeeded(Callbacks.SnapshotRestored.Map(restored => restored(doc)));

    public sealed override bool SupportsAnimation() => Callbacks.Animation.IsSome;

    public sealed override void AnimationStart(RhinoDoc doc, int iFrames) => _ = Succeeded(Callbacks.Animation.Map(animation => animation.AnimationStart(doc, iFrames)));

    public sealed override bool PrepareForDocumentAnimation(RhinoDoc doc, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop) =>
        Succeeded(Callbacks.Animation.Map(animation => animation.PrepareForDocumentAnimation(doc, archive_start, archive_stop)));

    public sealed override void ExtendBoundingBoxForDocumentAnimation(RhinoDoc doc, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop, ref BoundingBox bbox) =>
        bbox = Extended(bbox, Callbacks.Animation.Map(animation => fun((BoundingBox current) => animation.ExtendBoundingBoxForDocumentAnimation(doc, archive_start, archive_stop, current))));

    public sealed override bool AnimateDocument(RhinoDoc doc, double dPos, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop) =>
        Succeeded(Callbacks.Animation.Map(animation => animation.AnimateDocument(doc, dPos, archive_start, archive_stop)));

    public sealed override bool PrepareForObjectAnimation(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop) =>
        Transformed(ref transform, Callbacks.Animation.Map(animation => fun((Transform current) => animation.PrepareForObjectAnimation(doc, doc_object, current, archive_start, archive_stop))));

    public sealed override void ExtendBoundingBoxForObjectAnimation(
        RhinoDoc doc,
        RhinoObject doc_object,
        ref Transform transform,
        BinaryArchiveReader archive_start,
        BinaryArchiveReader archive_stop,
        ref BoundingBox bbox) {
        Transform current = transform;
        bbox = Extended(bbox, Callbacks.Animation.Map(animation => fun((BoundingBox box) => animation.ExtendBoundingBoxForObjectAnimation(doc, doc_object, current, archive_start, archive_stop, box))));
    }

    public sealed override bool AnimateObject(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, double dPos, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop) =>
        Transformed(ref transform, Callbacks.Animation.Map(animation => fun((Transform current) => animation.AnimateObject(doc, doc_object, current, dPos, archive_start, archive_stop))));

    public sealed override bool AnimationStop(RhinoDoc doc) => Succeeded(Callbacks.Animation.Map(animation => animation.AnimationStop(doc)));

    public sealed override bool ObjectTransformNotification(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveReader archive) =>
        Transformed(ref transform, Callbacks.ObjectTransformNotification.Map(notify => fun((Transform current) => notify(doc, doc_object, current, archive))));

    public sealed override bool IsCurrentModelStateInAnySnapshot(RhinoDoc doc, BinaryArchiveReader archive, SimpleArrayBinaryArchiveReader archive_array, TextLog? text_log = null) =>
        InAnySnapshot(doc, Option<RhinoObject>.None, archive, archive_array, text_log);

    public sealed override bool IsCurrentModelStateInAnySnapshot(RhinoDoc doc, RhinoObject doc_object, BinaryArchiveReader archive, SimpleArrayBinaryArchiveReader archive_array, TextLog? text_log = null) =>
        InAnySnapshot(doc, Some(doc_object), archive, archive_array, text_log);

    private bool InAnySnapshot(RhinoDoc doc, Option<RhinoObject> subject, BinaryArchiveReader archive, SimpleArrayBinaryArchiveReader array, TextLog? log) =>
        Answers.Answer(Callbacks.IsCurrentModelStateInAnySnapshot.Map(check => check(doc, subject, archive, array, Optional(log))), Callbacks.Reject, static () => false);

    private IO<(ChunkFrame Frame, ArchivableDictionary Dictionary)> Chunk(BinaryArchiveReader archive) =>
        AttachedData.ReadChunk(archive, Callbacks.Frame.TypeCode, Callbacks.SupportsVersion);

    private bool Succeeded(Option<IO<Unit>> effect) =>
        Answers.Succeeded(effect, Callbacks.Reject, static () => false);

    private bool Transformed(ref Transform transform, Option<Func<Transform, IO<Transform>>> step) {
        Transform current = transform;
        Option<Transform> next = Answers.Answer(step.Map(run => run(current).Map(Some)), Callbacks.Reject, static () => Option<Transform>.None);
        transform = next.IfNone(current);
        return next.IsSome;
    }

    private BoundingBox Extended(BoundingBox box, Option<Func<BoundingBox, IO<BoundingBox>>> step) =>
        Answers.Answer(step.Map(run => run(box)), Callbacks.Reject, () => box);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NamedStates {
    // --- [CONSTRUCTION_PLANES]
    public static IO<Option<ConstructionPlane>> FindConstructionPlane(RhinoDoc doc, ComponentRef address) =>
        from found in PlaneIndex(doc, address)
        from plane in found.Traverse(index => IO.lift(() => doc.NamedConstructionPlanes[index])).As()
        select plane;

    public static IO<int> Add(RhinoDoc doc, ConstructionPlane plane) =>
        from valid in IO.lift(() => Invalid.Unless(plane.Plane.IsValid, nameof(Plane.IsValid)))
        from index in IO.lift(() => Answers.NonNegative(doc.NamedConstructionPlanes.Add(plane), nameof(NamedConstructionPlaneTable.Add)))
        select index;

    public static IO<Unit> DeleteConstructionPlane(RhinoDoc doc, ComponentRef address) =>
        from found in PlaneIndex(doc, address)
        from index in IO.lift(found.ToFin(new Missing(nameof(NamedConstructionPlaneTable.Find))))
        from deleted in IO.lift(() => Refused.Unless(doc.NamedConstructionPlanes.Delete(index), nameof(NamedConstructionPlaneTable.Delete)))
        select deleted;

    private static IO<Option<int>> PlaneIndex(RhinoDoc doc, ComponentRef address) =>
        TableOps.Find<int>(
            address,
            static _ => Option<int>.None,
            index => Some(index).Filter(found => found < doc.NamedConstructionPlanes.Count),
            name => Answers.Present(doc.NamedConstructionPlanes.Find(name)));

    // --- [POSITIONS]
    public static IO<Seq<NamedPosition>> Positions(RhinoDoc doc) =>
        from ids in IO.lift(() => toSeq(doc.NamedPositions.Ids))
        from positions in ids.TraverseM(id =>
            from name in IO.lift(() => Missing.Unless(doc.NamedPositions.Name(id), nameof(NamedPositionTable.Name)))
            from objectIds in IO.lift(() => toSeq(doc.NamedPositions.ObjectIds(id)))
            from objects in objectIds.TraverseM(objectId => IO.lift(() => {
                Transform xform = Transform.Identity;
                return from stored in Refused.Unless(doc.NamedPositions.ObjectXform(id, objectId, ref xform), nameof(NamedPositionTable.ObjectXform))
                       select (ObjectId: objectId, Xform: xform);
            })).As()
            select new NamedPosition(id, name, objects)).As()
        select positions;

    public static IO<Guid> SavePosition(RhinoDoc doc, string name, Seq<Guid> objectIds) =>
        from named in IO.lift(() => Invalid.Unless(name.Length > 0, nameof(NamedPositionTable.Save)))
        from populated in IO.lift(() => Invalid.Unless(!objectIds.IsEmpty, nameof(NamedPositionTable.Save)))
        from objects in Queries.Evaluate(doc, new ObjectTarget.Ids(objectIds))
        from id in IO.lift(() => Answers.NonEmpty(doc.NamedPositions.Save(name, objects), nameof(NamedPositionTable.Save)))
        select id;

    public static IO<Unit> ApplyPositionOp(RhinoDoc doc, ComponentRef address, PositionOp op) =>
        from found in TableOps.Find<Guid>(
            address,
            id => Some(id).Filter(doc.NamedPositions.Ids.Contains),
            static _ => Option<Guid>.None,
            name => Answers.Present(doc.NamedPositions.Id(name)))
        from id in IO.lift(found.ToFin(new Missing(nameof(NamedPositionTable.Id))))
        from applied in op.Switch(
            (Doc: doc, Id: id),
            restore: static (state, _) => IO.lift(() => Refused.Unless(state.Doc.NamedPositions.Restore(state.Id), nameof(NamedPositionTable.Restore))),
            update: static (state, _) => IO.lift(() => Refused.Unless(state.Doc.NamedPositions.Update(state.Id), nameof(NamedPositionTable.Update))),
            delete: static (state, _) => IO.lift(() => Refused.Unless(state.Doc.NamedPositions.Delete(state.Id), nameof(NamedPositionTable.Delete))),
            rename: static (state, rename) =>
                from named in IO.lift(() => Invalid.Unless(rename.Name.Length > 0, nameof(PositionOp.Rename)))
                from current in IO.lift(() => state.Doc.NamedPositions.Name(state.Id))
                from renamed in IO.lift(() => string.Equals(current, rename.Name, StringComparison.Ordinal)
                    ? Fin.Succ(unit)
                    : Refused.Unless(state.Doc.NamedPositions.Rename(state.Id, rename.Name), nameof(NamedPositionTable.Rename)))
                select renamed,
            append: static (state, append) =>
                from populated in IO.lift(() => Invalid.Unless(!append.ObjectIds.IsEmpty, nameof(PositionOp.Append)))
                from objects in Queries.Evaluate(state.Doc, new ObjectTarget.Ids(append.ObjectIds))
                from appended in IO.lift(() => Refused.Unless(state.Doc.NamedPositions.Append(state.Id, objects), nameof(NamedPositionTable.Append)))
                select appended)
        select applied;

    // --- [LAYER_STATES]
    public static IO<int> SaveLayerState(RhinoDoc doc, string name, Option<Guid> viewport) =>
        from named in IO.lift(() => Invalid.Unless(name.Length > 0, nameof(NamedLayerStateTable.Save)))
        from viewportId in IO.lift(() => ViewportId(viewport, nameof(NamedLayerStateTable.Save)))
        from index in IO.lift(() => Answers.NonNegative(
            viewportId.Match(Some: id => doc.NamedLayerStates.Save(name, id), None: () => doc.NamedLayerStates.Save(name)),
            nameof(NamedLayerStateTable.Save)))
        from found in IO.lift(() => doc.NamedLayerStates.FindName(name))
        from same in IO.lift(Mismatch.Unless(found == index, nameof(NamedLayerStateTable.FindName)))
        select index;

    public static IO<Unit> ApplyLayerStateOp(RhinoDoc doc, ComponentRef address, LayerStateOp op) =>
        from found in TableOps.Find<string>(
            address,
            static _ => Option<string>.None,
            index => toSeq(doc.NamedLayerStates.Names).At(index),
            name => Some(name).Filter(named => doc.NamedLayerStates.FindName(named) >= 0))
        from name in IO.lift(found.ToFin(new Missing(nameof(NamedLayerStateTable.FindName))))
        from applied in op.Switch(
            (Doc: doc, Name: name),
            restore: static (state, restore) =>
                from viewportId in IO.lift(() => ViewportId(restore.Viewport, nameof(LayerStateOp.Restore)))
                from restored in IO.lift(() => Refused.Unless(
                    viewportId.Match(
                        Some: id => state.Doc.NamedLayerStates.Restore(state.Name, restore.Properties, id),
                        None: () => state.Doc.NamedLayerStates.Restore(state.Name, restore.Properties)),
                    nameof(NamedLayerStateTable.Restore)))
                select restored,
            rename: static (state, rename) =>
                from named in IO.lift(() => Invalid.Unless(rename.NewName.Length > 0, nameof(LayerStateOp.Rename)))
                from renamed in IO.lift(() => Refused.Unless(state.Doc.NamedLayerStates.Rename(state.Name, rename.NewName), nameof(NamedLayerStateTable.Rename)))
                select renamed,
            delete: static (state, _) => IO.lift(() => Refused.Unless(state.Doc.NamedLayerStates.Delete(state.Name), nameof(NamedLayerStateTable.Delete))))
        select applied;

    public static IO<int> ImportLayerStates(RhinoDoc doc, string path) =>
        Answers.ExistingPath(path).Bind(existing => IO.lift(() => doc.NamedLayerStates.Import(existing)));

    private static Fin<Option<Guid>> ViewportId(Option<Guid> viewport, string member) =>
        viewport.Traverse(id => Answers.NonEmpty(id, member)).As();

    // --- [SNAPSHOTS]
    public static IO<Unit> RunSnapshot(RhinoDoc doc, SnapshotOption option, string name) =>
        from named in IO.lift(() => Invalid.Unless((name.Length > 0) && (name.IndexOfAny(['"', '\r', '\n']) < 0), nameof(RhinoApp.RunScript)))
        from runner in IO.lift(static () => Refused.Unless(Command.InScriptRunnerCommand(), nameof(Command.InScriptRunnerCommand)))
        from before in Stored(doc, name)
        from existing in IO.lift(option == SnapshotOption.Save ? Invalid.Unless(!before, nameof(SnapshotOption.Save)) : Missing.Unless(before, nameof(SnapshotTable.Names)))
        from ran in IO.lift(() => Refused.Unless(RhinoApp.RunScript(doc.RuntimeSerialNumber, Script(option, name), echo: false), nameof(RhinoApp.RunScript)))
        from after in Stored(doc, name)
        from same in IO.lift(Mismatch.Unless(after == (option != SnapshotOption.Delete), nameof(SnapshotTable.Names)))
        select same;

    public static IO<TValue> WithSnapshot<TValue>(RhinoDoc doc, string name, IO<TValue> body) =>
        Disposal.Bracketed(
            RunSnapshot(doc, SnapshotOption.Save, name),
            _ => Disposal.Bracketed(IO.pure(unit), _ => RunSnapshot(doc, SnapshotOption.Delete, name), _ => RunSnapshot(doc, SnapshotOption.Restore, name)),
            _ => body);

    public static IO<Unit> RegisterSnapShotClient(SnapShotsClient client) =>
        from plugged in IO.lift(() => Missing.Unless(PlugIn.Find(client.GetType().Assembly) is not null, nameof(PlugIn.Find)))
        from registered in IO.lift(() => Refused.Unless(SnapShotsClient.RegisterSnapShotClient(client), nameof(SnapShotsClient.RegisterSnapShotClient)))
        select registered;

    private static string Script(SnapshotOption option, string name) =>
        option switch {
            SnapshotOption.Save => $"_-Snapshots _Save _Parameters=_All \"{name}\" _Enter",
            SnapshotOption.Restore => $"_-Snapshots _Restore \"{name}\" _Enter _Enter",
            SnapshotOption.Delete => $"_-Snapshots _Delete \"{name}\" _Enter",
        };

    private static IO<bool> Stored(RhinoDoc doc, string name) =>
        IO.lift(() => doc.Snapshots.Names.Contains(name, StringComparer.OrdinalIgnoreCase));
}
