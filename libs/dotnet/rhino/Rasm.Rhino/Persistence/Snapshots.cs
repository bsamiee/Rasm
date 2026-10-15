using Rasm.Rhino.Document.Files;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.SnapShots;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;
using Rhino.PlugIns;
using Rhino.Runtime.InteropWrappers;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(SkipIParsable = true, EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads, ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads)]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class SnapshotName {
    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref string value) =>
        validationError = value.Length > 0 && !value.AsSpan().ContainsAny('"', '\r', '\n') ? null : new InvalidRhinoValue();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SnapshotChange {
    public sealed record Save(SnapshotName Name) : SnapshotChange;

    public sealed record Restore(SnapshotName Name) : SnapshotChange;

    public sealed record Delete(SnapshotName Name) : SnapshotChange;

    public sealed record Rename(SnapshotName Name, SnapshotName NewName) : SnapshotChange;

    internal (string Script, Seq<(SnapshotName Name, bool Present)> Before) Request => Switch(
        save: static change => ($"""_-Snapshots _Save _Parameters=_All "{change.Name}" _Enter""", Seq((change.Name, false))),
        restore: static change => ($"""_-Snapshots _Restore "{change.Name}" _Enter _Enter""", Seq((change.Name, true))),
        delete: static change => ($"""_-Snapshots _Delete "{change.Name}" _Enter""", Seq((change.Name, true))),
        rename: static change => ($"""_-Snapshots _Rename "{change.Name}" "{change.NewName}" _Enter""", Seq((change.Name, true), (change.NewName, false))));
}

public sealed record SnapshotObject(RhinoDoc Doc, RhinoObject Object, Transform Transform);

public sealed record SnapshotTween<TSubject, TState>(Func<TState, TState, double, TState> Between, Option<Func<TSubject, TState, TState, BoundingBox>> Bounds = default);

internal sealed record SnapshotFrames<TSubject>(Func<TSubject, double, IO<Unit>> Animate, Option<BoundingBox> Extent);

public abstract record SnapshotState<TSubject> {
    internal abstract bool Animates { get; }

    internal abstract IO<Unit> Save(TSubject subject, BinaryArchiveWriter archive);

    internal abstract IO<Unit> Restore(TSubject subject, BinaryArchiveReader archive);

    internal abstract IO<bool> Held(BinaryArchiveReader current, Seq<BinaryArchiveReader> stored);

    internal abstract IO<Option<SnapshotFrames<TSubject>>> Prepare(TSubject subject, BinaryArchiveReader start, BinaryArchiveReader stop);
}

public sealed record SnapshotState<TSubject, TState>(
    uint TypeCode,
    DictionaryCodec<TState> Codec,
    Func<TSubject, IO<TState>> Read,
    Func<TSubject, TState, IO<Unit>> Write) : SnapshotState<TSubject> where TState : notnull {
    public Option<SnapshotTween<TSubject, TState>> Tween { get; init; }

    internal override bool Animates => Tween.IsSome;

    internal override IO<Unit> Save(TSubject subject, BinaryArchiveWriter archive) =>
        from state in IO.lift(() => Read(subject)).Flatten()
        from written in AttachedData.Write(archive, TypeCode, Codec, state)
        select written;

    internal override IO<Unit> Restore(TSubject subject, BinaryArchiveReader archive) =>
        from state in Decoded(archive)
        from written in Write(subject, state)
        select written;

    internal override IO<bool> Held(BinaryArchiveReader current, Seq<BinaryArchiveReader> stored) =>
        from now in Decoded(current)
        from held in stored.TraverseM(Decoded).As()
        select held.Exists(state => EqualityComparer<TState>.Default.Equals(state, now));

    internal override IO<Option<SnapshotFrames<TSubject>>> Prepare(TSubject subject, BinaryArchiveReader start, BinaryArchiveReader stop) =>
        Tween.Match(
            Some: tween =>
                from first in Decoded(start)
                from last in Decoded(stop)
                select Some(new SnapshotFrames<TSubject>(
                    (frameSubject, position) => IO.lift(() => Write(frameSubject, tween.Between(first, last, position))).Flatten(),
                    tween.Bounds.Map(bounds => bounds(subject, first, last)))),
            None: static () => IO.pure(Option<SnapshotFrames<TSubject>>.None));

    private IO<TState> Decoded(BinaryArchiveReader archive) =>
        from positioned in IO.lift(() => Refused.Unless(archive.SeekFromStart(0UL), nameof(BinaryArchiveReader.SeekFromStart)))
        from state in AttachedData.Read(archive, TypeCode, Codec)
        select state;
}

public sealed record SnapshotDefinition(
    string Name,
    Func<string> Category,
    Option<SnapshotState<RhinoDoc>> Document = default,
    Option<(Func<RhinoObject, bool> Supports, SnapshotState<SnapshotObject> State)> Objects = default,
    Option<Func<RhinoDoc, IO<Unit>>> Restored = default);

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedSnapshotClient(SnapshotDefinition definition) : SnapShotsClient {
    // --- [IDENTITY]
    public sealed override Guid PlugInId() => PlugIn.Find(GetType().Assembly).Id;

    public sealed override Guid ClientId() => GetType().GUID;

    public sealed override string Category() => definition.Category();

    public sealed override string Name() => definition.Name;

    // --- [DOCUMENT]
    public sealed override bool SupportsDocument() => definition.Document.IsSome;

    public sealed override bool SaveDocument(RhinoDoc doc, BinaryArchiveWriter archive) =>
        Callbacks.Succeeded(definition.Document.Map(state => state.Save(doc, archive)), static () => false, CallbackSite.Of(this));

    public sealed override bool RestoreDocument(RhinoDoc doc, BinaryArchiveReader archive) =>
        Callbacks.Succeeded(definition.Document.Map(state => state.Restore(doc, archive)), static () => false, CallbackSite.Of(this));

    public sealed override void SnapshotRestored(RhinoDoc doc) =>
        Callbacks.Run(definition.Restored.Map(restored => IO.lift(() => restored(doc)).Flatten()), CallbackSite.Of(this));

    public sealed override bool IsCurrentModelStateInAnySnapshot(
        RhinoDoc doc, BinaryArchiveReader archive, SimpleArrayBinaryArchiveReader archive_array, TextLog? text_log = null) =>
        Callbacks.Answer(definition.Document.Map(state => state.Held(archive, Stored(archive_array))), static () => true, static () => false, CallbackSite.Of(this));

    // --- [OBJECTS]
    public sealed override bool SupportsObjects() => definition.Objects.IsSome;

    public sealed override bool SupportsObject(RhinoObject doc_object) =>
        Callbacks.Answer(definition.Objects.Map(objects => IO.lift(() => objects.Supports(doc_object))), static () => false, static () => false, CallbackSite.Of(this));

    public sealed override bool SaveObject(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveWriter archive) =>
        new SnapshotObject(doc, doc_object, transform) switch {
            var subject => Callbacks.Succeeded(definition.Objects.Map(objects => objects.State.Save(subject, archive)), static () => false, CallbackSite.Of(this)),
        };

    public sealed override bool RestoreObject(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveReader archive) =>
        new SnapshotObject(doc, doc_object, transform) switch {
            var subject => Callbacks.Succeeded(definition.Objects.Map(objects => objects.State.Restore(subject, archive)), static () => false, CallbackSite.Of(this)),
        };

    public sealed override bool ObjectTransformNotification(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveReader archive) => true;

    public sealed override bool IsCurrentModelStateInAnySnapshot(
        RhinoDoc doc, RhinoObject doc_object, BinaryArchiveReader archive, SimpleArrayBinaryArchiveReader archive_array, TextLog? text_log = null) =>
        Callbacks.Answer(definition.Objects.Map(objects => objects.State.Held(archive, Stored(archive_array))), static () => true, static () => false, CallbackSite.Of(this));

    // --- [ANIMATION]
    private readonly Atom<Option<SnapshotFrames<RhinoDoc>>> documentFrames = Atom(Option<SnapshotFrames<RhinoDoc>>.None);

    private readonly AtomHashMap<Guid, SnapshotFrames<SnapshotObject>> objectFrames = AtomHashMap<Guid, SnapshotFrames<SnapshotObject>>();

    public sealed override bool SupportsAnimation() =>
        definition.Document.Exists(static state => state.Animates) || definition.Objects.Exists(static objects => objects.State.Animates);

    public sealed override void AnimationStart(RhinoDoc doc, int iFrames) { }

    public sealed override bool PrepareForDocumentAnimation(RhinoDoc doc, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop) =>
        Callbacks.Succeeded(
            from state in definition.Document
            select state.Prepare(doc, archive_start, archive_stop).Bind(frames => writeIO(documentFrames, frames)).Map(unit),
            static () => false, CallbackSite.Of(this));

    public sealed override void ExtendBoundingBoxForDocumentAnimation(RhinoDoc doc, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop, ref BoundingBox bbox) =>
        bbox = Extended(bbox, documentFrames.Value.Bind(static frames => frames.Extent));

    public sealed override bool AnimateDocument(RhinoDoc doc, double dPos, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop) =>
        Callbacks.Succeeded(documentFrames.Value.Map(frames => frames.Animate(doc, dPos)), static () => false, CallbackSite.Of(this));

    public sealed override bool PrepareForObjectAnimation(
        RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop) =>
        new SnapshotObject(doc, doc_object, transform) switch {
            var subject => Callbacks.Succeeded(
                from objects in definition.Objects
                select from frames in objects.State.Prepare(subject, archive_start, archive_stop)
                       from stored in IO.lift(() => objectFrames.SwapKey(doc_object.Id, _ => frames))
                       select unit,
                static () => false, CallbackSite.Of(this)),
        };

    public sealed override void ExtendBoundingBoxForObjectAnimation(
        RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop, ref BoundingBox bbox) =>
        bbox = Extended(bbox, objectFrames.Find(doc_object.Id).Bind(static frames => frames.Extent));

    public sealed override bool AnimateObject(
        RhinoDoc doc, RhinoObject doc_object, ref Transform transform, double dPos, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop) =>
        new SnapshotObject(doc, doc_object, transform) switch {
            var subject => Callbacks.Succeeded(objectFrames.Find(doc_object.Id).Map(frames => frames.Animate(subject, dPos)), static () => false, CallbackSite.Of(this)),
        };

    public sealed override bool AnimationStop(RhinoDoc doc) =>
        Callbacks.Succeeded(
            from document in writeIO(documentFrames, None)
            from objects in IO.lift(objectFrames.Clear)
            select unit,
            CallbackSite.Of(this));

    private static BoundingBox Extended(BoundingBox box, Option<BoundingBox> extent) =>
        extent.Map(added => BoundingBox.Union(box, added)).IfNone(box);

    private static Seq<BinaryArchiveReader> Stored(SimpleArrayBinaryArchiveReader stored) =>
        toSeq(Range(0, stored.Count)).Map(stored.Get).Strict();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NamedSnapshots {
    // --- [NAMES]
    public static IO<Seq<string>> Names(RhinoDoc doc) =>
        IO.lift(() => Conversions.Rows(doc.Snapshots.Names));

    private static Validation<Error, Unit> Expect(Seq<string> names, (SnapshotName Name, bool Present) row) =>
        Holds(names, row) ? unit
        : row.Present ? new UnknownSnapshot(row.Name)
        : new Taken(nameof(SnapshotTable.Names), row.Name);

    private static bool Holds(Seq<string> names, (SnapshotName Name, bool Present) row) =>
        names.Exists(stored => row.Name == stored) == row.Present;

    // --- [TRANSITIONS]
    public static IO<Unit> Save(RhinoDoc doc, SnapshotName name) =>
        from held in Names(doc)
        from saved in RunSnapshot(doc, Holds(held, (name, true))
            ? [new SnapshotChange.Delete(name), new SnapshotChange.Save(name)]
            : [new SnapshotChange.Save(name)])
        select saved;

    public static IO<Unit> RunSnapshot(RhinoDoc doc, SnapshotChange change) =>
        from before in Names(doc)
        let request = change.Request
        from expected in IO.lift(() => request.Before.Traverse(row => Expect(before, row)).As().ToFin())
        from ran in Documents.RunScript(doc, request.Script, echo: false, display: None)
        select ran;

    public static IO<Unit> RunSnapshot(RhinoDoc doc, Seq<SnapshotChange> changes) =>
        changes.TraverseM(change => RunSnapshot(doc, change)).As().Map(unit);

    // --- [SCOPES]
    public static IO<T> WithSnapshot<T>(RhinoDoc doc, SnapshotName held, Option<SnapshotName> target, IO<T> body) =>
        from saved in RunSnapshot(doc, new SnapshotChange.Save(held))
        from result in (from restored in target.Traverse(name => RunSnapshot(doc, new SnapshotChange.Restore(name))).As()
                        from value in body
                        select value).Finally(RunSnapshot(doc, [new SnapshotChange.Restore(held), new SnapshotChange.Delete(held)]))
        select result;
}
