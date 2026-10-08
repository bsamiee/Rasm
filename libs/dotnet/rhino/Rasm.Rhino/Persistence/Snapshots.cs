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
[ValueObject<string>(EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads, ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads)]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class SnapshotName {
    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref string value) =>
        validationError = value.Length > 0 && !value.AsSpan().ContainsAny('"', '\r', '\n') ? null : new InvalidRhinoValue();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SnapshotChange {
    private SnapshotChange(SnapshotName name) => Name = name;

    public SnapshotName Name { get; }

    public abstract string Script { get; }

    public abstract Seq<(SnapshotName Name, bool Present)> Before { get; }

    public abstract Seq<(SnapshotName Name, bool Present)> After { get; }

    public sealed record Save(SnapshotName Name) : SnapshotChange(Name) {
        public override string Script => $"_-Snapshots _Save _Parameters=_All \"{Name}\" _Enter";

        public override Seq<(SnapshotName Name, bool Present)> Before => [(Name, false)];

        public override Seq<(SnapshotName Name, bool Present)> After => [(Name, true)];
    }

    public sealed record Restore(SnapshotName Name) : SnapshotChange(Name) {
        public override string Script => $"_-Snapshots _Restore \"{Name}\" _Enter _Enter";

        public override Seq<(SnapshotName Name, bool Present)> Before => [(Name, true)];

        public override Seq<(SnapshotName Name, bool Present)> After => [];
    }

    public sealed record Delete(SnapshotName Name) : SnapshotChange(Name) {
        public override string Script => $"_-Snapshots _Delete \"{Name}\" _Enter";

        public override Seq<(SnapshotName Name, bool Present)> Before => [(Name, true)];

        public override Seq<(SnapshotName Name, bool Present)> After => [(Name, false)];
    }

    public sealed record Rename(SnapshotName Name, SnapshotName NewName) : SnapshotChange(Name) {
        public override string Script => $"_-Snapshots _Rename \"{Name}\" \"{NewName}\" _Enter";

        public override Seq<(SnapshotName Name, bool Present)> Before => [(Name, true), (NewName, false)];

        public override Seq<(SnapshotName Name, bool Present)> After => [(Name, false), (NewName, true)];
    }
}

public sealed record SnapshotObject(RhinoDoc Doc, RhinoObject Object, Transform Transform);

public sealed record SnapshotTween<TState>(Func<TState, TState, double, TState> Between) {
    public Option<Func<TState, BoundingBox>> Bounds { get; init; }
}

internal sealed record SnapshotFrames<TSubject>(Func<TSubject, double, IO<Unit>> Animate, Option<BoundingBox> Extent);

public abstract record SnapshotState<TSubject> {
    internal abstract bool Animates { get; }

    internal abstract IO<Unit> Save(TSubject subject, BinaryArchiveWriter archive);

    internal abstract IO<Unit> Restore(TSubject subject, BinaryArchiveReader archive);

    internal abstract IO<bool> Held(BinaryArchiveReader current, Seq<BinaryArchiveReader> stored);

    internal abstract IO<Option<SnapshotFrames<TSubject>>> Prepare(BinaryArchiveReader start, BinaryArchiveReader stop);
}

public sealed record SnapshotState<TSubject, TState>(
    uint TypeCode,
    DictionaryCodec<TState> Codec,
    Func<TSubject, IO<TState>> Read,
    Func<TSubject, TState, IO<Unit>> Write) : SnapshotState<TSubject> where TState : notnull {
    public Option<SnapshotTween<TState>> Tween { get; init; }

    internal override bool Animates => Tween.IsSome;

    internal override IO<Unit> Save(TSubject subject, BinaryArchiveWriter archive) =>
        Read(subject).Bind(state => AttachedData.Write(archive, TypeCode, Codec, state));

    internal override IO<Unit> Restore(TSubject subject, BinaryArchiveReader archive) =>
        Decoded(archive).Bind(state => Write(subject, state));

    internal override IO<bool> Held(BinaryArchiveReader current, Seq<BinaryArchiveReader> stored) =>
        from now in Decoded(current)
        from held in stored.TraverseM(Decoded).As()
        select held.Exists(state => EqualityComparer<TState>.Default.Equals(state, now));

    internal override IO<Option<SnapshotFrames<TSubject>>> Prepare(BinaryArchiveReader start, BinaryArchiveReader stop) =>
        Tween.Match(
            Some: tween =>
                from first in Decoded(start)
                from last in Decoded(stop)
                select EqualityComparer<TState>.Default.Equals(first, last)
                    ? Option<SnapshotFrames<TSubject>>.None
                    : Some(new SnapshotFrames<TSubject>(
                        (subject, position) => Write(subject, tween.Between(first, last, double.Clamp(position, 0d, 1d))),
                        tween.Bounds.Map(bounds => BoundingBox.Union(bounds(first), bounds(last))))),
            None: static () => IO.pure(Option<SnapshotFrames<TSubject>>.None));

    private IO<TState> Decoded(BinaryArchiveReader archive) =>
        IO.lift(() => Refused.Unless(archive.SeekFromStart(0UL), nameof(BinaryArchiveReader.SeekFromStart)))
            .Bind(_ => AttachedData.Read(archive, TypeCode, Codec));
}

public sealed record SnapshotDefinition {
    public Option<SnapshotState<RhinoDoc>> Document { get; init; }

    public Option<(Func<RhinoObject, bool> Supports, SnapshotState<SnapshotObject> State)> Objects { get; init; }

    public Option<Func<RhinoDoc, IO<Unit>>> Restored { get; init; }
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedSnapshotClient(SnapshotDefinition definition) : SnapShotsClient {
    // --- [IDENTITY]
    public sealed override Guid PlugInId() => PlugIn.Find(GetType().Assembly).Id;

    public sealed override Guid ClientId() => GetType().GUID;

    // --- [DOCUMENT]
    public sealed override bool SupportsDocument() => definition.Document.IsSome;

    public sealed override bool SaveDocument(RhinoDoc doc, BinaryArchiveWriter archive) =>
        Callbacks.Succeeded(definition.Document.Map(state => state.Save(doc, archive)), static () => false, CallbackSite.Of(this));

    public sealed override bool RestoreDocument(RhinoDoc doc, BinaryArchiveReader archive) =>
        Callbacks.Succeeded(definition.Document.Map(state => state.Restore(doc, archive)), static () => false, CallbackSite.Of(this));

    public sealed override void SnapshotRestored(RhinoDoc doc) =>
        _ = Callbacks.Answer(definition.Restored.Map(restored => restored(doc)), static () => unit, static () => unit, CallbackSite.Of(this));

    public sealed override bool IsCurrentModelStateInAnySnapshot(
        RhinoDoc doc, BinaryArchiveReader archive, SimpleArrayBinaryArchiveReader archive_array, TextLog? text_log = null) =>
        Callbacks.Answer(definition.Document.Map(state => state.Held(archive, Stored(archive_array))), static () => true, static () => false, CallbackSite.Of(this));

    // --- [OBJECTS]
    public sealed override bool SupportsObjects() => definition.Objects.IsSome;

    public sealed override bool SupportsObject(RhinoObject doc_object) =>
        Callbacks.Answer(definition.Objects.Map(objects => IO.lift(() => objects.Supports(doc_object))), static () => false, static () => false, CallbackSite.Of(this));

    public sealed override bool SaveObject(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveWriter archive) {
        SnapshotObject subject = new(doc, doc_object, transform);
        return Callbacks.Succeeded(definition.Objects.Map(objects => objects.State.Save(subject, archive)), static () => false, CallbackSite.Of(this));
    }

    public sealed override bool RestoreObject(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveReader archive) {
        SnapshotObject subject = new(doc, doc_object, transform);
        return Callbacks.Succeeded(definition.Objects.Map(objects => objects.State.Restore(subject, archive)), static () => false, CallbackSite.Of(this));
    }

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
        Callbacks.Answer(
            definition.Document.Map(state => state.Prepare(archive_start, archive_stop).Bind(frames => IO.lift(() => documentFrames.Swap(_ => frames).IsSome))),
            static () => false,
            static () => false,
            CallbackSite.Of(this));

    public sealed override void ExtendBoundingBoxForDocumentAnimation(RhinoDoc doc, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop, ref BoundingBox bbox) =>
        bbox = Extended(bbox, documentFrames.Value.Bind(static frames => frames.Extent));

    public sealed override bool AnimateDocument(RhinoDoc doc, double dPos, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop) =>
        Callbacks.Succeeded(documentFrames.Value.Map(frames => frames.Animate(doc, dPos)), static () => false, CallbackSite.Of(this));

    public sealed override bool PrepareForObjectAnimation(
        RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop) =>
        Callbacks.Answer(
            definition.Objects.Map(objects =>
                from frames in objects.State.Prepare(archive_start, archive_stop)
                from swapped in IO.lift(() => objectFrames.SwapKey(doc_object.Id, _ => frames))
                select frames.IsSome),
            static () => false,
            static () => false,
            CallbackSite.Of(this));

    public sealed override void ExtendBoundingBoxForObjectAnimation(
        RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop, ref BoundingBox bbox) =>
        bbox = Extended(bbox, objectFrames.Find(doc_object.Id).Bind(static frames => frames.Extent));

    public sealed override bool AnimateObject(
        RhinoDoc doc, RhinoObject doc_object, ref Transform transform, double dPos, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop) {
        SnapshotObject subject = new(doc, doc_object, transform);
        return Callbacks.Succeeded(objectFrames.Find(doc_object.Id).Map(frames => frames.Animate(subject, dPos)), static () => false, CallbackSite.Of(this));
    }

    public sealed override bool AnimationStop(RhinoDoc doc) =>
        Callbacks.Succeeded(
            from document in IO.lift(() => documentFrames.Swap(static _ => Option<SnapshotFrames<RhinoDoc>>.None))
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

    // --- [TRANSITIONS]
    public static IO<Unit> Save(RhinoDoc doc, SnapshotName name) =>
        Names(doc).Bind(held => RunSnapshot(doc, Holds(held, (name, true))
            ? Seq<SnapshotChange>(new SnapshotChange.Delete(name), new SnapshotChange.Save(name))
            : Seq<SnapshotChange>(new SnapshotChange.Save(name))));

    public static IO<Unit> Delete(RhinoDoc doc, SnapshotName name) =>
        RunSnapshot(doc, new SnapshotChange.Delete(name));

    public static IO<Unit> RunSnapshot(RhinoDoc doc, SnapshotChange change) =>
        from before in Names(doc)
        from expected in IO.lift(() => change.Before.Traverse(row => Expect(before, row)).As().ToFin())
        from ran in Documents.RunScript(doc, change.Script, echo: false, display: None)
        from after in Names(doc)
        from applied in IO.lift(() => SnapshotNotApplied.Unless(change.After.ForAll(row => Holds(after, row)), change))
        select applied;

    public static IO<Unit> RunSnapshot(RhinoDoc doc, Seq<SnapshotChange> changes) =>
        changes.TraverseM(change => RunSnapshot(doc, change)).As().Map(static _ => unit);

    // --- [SCOPES]
    public static IO<T> WithSnapshot<T>(RhinoDoc doc, SnapshotName held, Option<SnapshotName> target, IO<T> body) =>
        RunSnapshot(doc, new SnapshotChange.Save(held)).Bracket(
            Use: _ => target.Traverse(name => RunSnapshot(doc, new SnapshotChange.Restore(name))).As().Bind(_ => body),
            Fin: _ => RunSnapshot(doc, new SnapshotChange.Restore(held)).Bind(_ => Delete(doc, held)));

    private static Validation<Error, Unit> Expect(Seq<string> names, (SnapshotName Name, bool Present) row) =>
        Holds(names, row) ? unit
        : row.Present ? new UnknownSnapshot(row.Name)
        : new Taken(nameof(SnapshotTable.Names), row.Name);

    private static bool Holds(Seq<string> names, (SnapshotName Name, bool Present) row) =>
        names.Exists(stored => row.Name == stored) == row.Present;
}
