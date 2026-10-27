using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rasm.RhinoCommon.Document.Files;
using Rasm.RhinoCommon.Document.Tables;
using Rasm.RhinoCommon.UI;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.SnapShots;
using Rhino.FileIO;
using Rhino.Runtime.InteropWrappers;

namespace Rasm.RhinoCommon.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(SkipIParsable = true)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<RhinoFault>]
public sealed partial class SnapshotText {
    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref string value) =>
        validationError = Quotable(value) ? null : new RhinoFault(value, RhinoReason.Unqualified);

    internal static bool Quotable(string value) => value.Length > 0 && !value.AsSpan().ContainsAny('"', '\r', '\n');
}

[ValueObject<string>(SkipIParsable = true, EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads, ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[ValidationError<RhinoFault>]
public sealed partial class SnapshotName {
    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref string value) =>
        validationError = SnapshotText.Quotable(value) ? null : new RhinoFault(value, RhinoReason.Unqualified);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SnapshotChange {
    public sealed record Save(SnapshotName Name) : SnapshotChange;

    public sealed record Restore(SnapshotName Name) : SnapshotChange;

    public sealed record Delete(SnapshotName Name) : SnapshotChange;

    public sealed record Rename(SnapshotName Name, SnapshotName NewName) : SnapshotChange;

    public sealed record Import(InputPath Path) : SnapshotChange;

    public sealed record UserText(SnapshotName Name, SnapshotText Key, Option<SnapshotText> Value) : SnapshotChange;

    // --- [RUN]
    public IO<Unit> Run(RhinoDoc doc) =>
        from names in IO.lift(() => Conversions.Rows(doc.Snapshots.Names))
        from script in IO.lift(() => Script(names))
        from ran in Documents.RunScript(doc, script, echo: false, display: None)
        select ran;

    // --- [ROWS]
    internal static IO<Seq<NamedEntry>> Entries(RhinoDoc doc) =>
        IO.lift(() => Conversions.Rows(doc.Snapshots.Names).Map(static name => new NamedEntry(NamedTable.Snapshots, name, None)).Strict());

    internal static IO<NamedWrite> Saved(RhinoDoc doc, SnapshotName name) =>
        new Save(name).Landed(doc);

    internal static IO<NamedWrite> Restored(RhinoDoc doc, SnapshotName name) =>
        new Restore(name).Landed(doc);

    internal static IO<NamedWrite> Renamed(RhinoDoc doc, NamedEntry entry, StateName name) =>
        IO.lift(() => (SnapshotName.TryCreate(entry.Name, out SnapshotName? held, out RhinoFault? heldFault) ? Fin.Succ(held) : heldFault,
                  SnapshotName.TryCreate(name, out SnapshotName? next, out RhinoFault? nextFault) ? Fin.Succ(next) : nextFault).Apply(static (current, renamed) => new Rename(current, renamed)).As())
            .Bind(rename => rename.Landed(doc));

    internal static IO<NamedWrite> Deleted(RhinoDoc doc, NamedEntry entry) =>
        IO.lift(() => SnapshotName.TryCreate(entry.Name, out SnapshotName? name, out RhinoFault? fault) ? Fin.Succ(name) : fault).Bind(name => new Delete(name).Landed(doc));

    internal static IO<NamedWrite> Held(RhinoDoc doc) =>
        from name in IO.lift(static () => SnapshotName.TryCreate(Guid.NewGuid().ToString(), out SnapshotName? held, out RhinoFault? fault) ? Fin.Succ(held) : fault)
        from saved in new Save(name).Run(doc)
        select (NamedWrite)new NamedWrite.Compensated(new Restore(name).Run(doc), new Delete(name).Run(doc));

    internal static IO<NamedWrite> Imported(RhinoDoc doc, ArchiveSource source, Option<Seq<StateName>> names) =>
        from whole in when(names.IsSome, IO.fail<Unit>(Error.New($"{nameof(Import)} reads every snapshot of a file"))).As()
        from path in source.Existing()
        from imported in new Import(path).Landed(doc)
        select imported;

    private IO<NamedWrite> Landed(RhinoDoc doc) =>
        Run(doc).Map(static _ => (NamedWrite)new NamedWrite.Recorded());

    // --- [SCRIPT]
    private Fin<string> Script(Seq<string> names) => Switch(names,
        save: static (held, change) => $"""_-Snapshots _Save _Parameters=_All "{change.Name}" {(Holds(held, change.Name) ? "_Yes " : "")}_Enter""",
        restore: static (held, change) => Expecting(held, [(change.Name, true)], $"""_-Snapshots _Restore "{change.Name}" _Enter _Enter"""),
        delete: static (held, change) => Expecting(held, [(change.Name, true)], $"""_-Snapshots _Delete "{change.Name}" _Enter"""),
        rename: static (held, change) => Expecting(held, [(change.Name, true), (change.NewName, false)], $"""_-Snapshots _Rename "{change.Name}" "{change.NewName}" _Enter"""),
        import: static (_, change) => SnapshotText.Quotable(change.Path)
            ? $"""_-Snapshots _Import "{change.Path}" _Enter"""
            : new RhinoFault(change.Path, RhinoReason.Unreadable),
        userText: static (held, change) => Expecting(held, [(change.Name, true)], change.Value.Match(
            Some: value => $"""_-Snapshots _UserText "{change.Name}" _Set "{change.Key}" "{value}" _Enter _Enter""",
            None: () => $"""_-Snapshots _UserText "{change.Name}" _Set "{change.Key}" _Enter _Enter _Enter""")));

    private static Fin<string> Expecting(Seq<string> names, Seq<(SnapshotName Name, bool Present)> rows, string script) =>
        rows.Traverse(row => Holds(names, row.Name) == row.Present ? Fin.Succ(unit)
            : new RhinoFault(row.Name, row.Present ? RhinoReason.Missing : RhinoReason.Taken)).As().Map(_ => script);

    private static bool Holds(Seq<string> names, SnapshotName name) =>
        names.Exists(stored => name == stored);
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

    public sealed record Of<TState>(
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
            from held in stored.TraverseM(Stored).As()
            select held.Exists(state => state.Exists(value => EqualityComparer<TState>.Default.Equals(value, now)));

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
            Stored(archive).Bind(found => IO.lift(found.ToFin(Error.New($"snapshot holds no readable {Codec.Name} record"))));

        private IO<Option<TState>> Stored(BinaryArchiveReader archive) =>
            from positioned in IO.lift(() => RhinoFault.Unless(archive.SeekFromStart(0UL)))
            from stored in AttachedData.Read(archive, TypeCode, Codec)
            select stored.Present;
    }
}

public sealed record SnapshotDefinition(
    Guid Client,
    string Name,
    Func<string> Category,
    Option<SnapshotState<RhinoDoc>> Document = default,
    Option<(Func<RhinoObject, bool> Supports, SnapshotState<SnapshotObject> State)> Objects = default,
    Option<Func<RhinoDoc, IO<Unit>>> Restored = default);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DefinedSnapshotClient(SnapshotDefinition definition, IPlugInViews views) : SnapShotsClient {
    // --- [REGISTRATION]
    public static Func<IPlugInViews, IO<Unit>> Register(SnapshotDefinition definition) =>
        views => IO.lift(() => RhinoFault.Unless(RegisterSnapShotClient(new DefinedSnapshotClient(definition, views))));

    // --- [IDENTITY]
    public override Guid PlugInId() => views.PlugIn.Id;

    public override Guid ClientId() => definition.Client;

    public override string Category() => definition.Category();

    public override string Name() => definition.Name;

    // --- [DOCUMENT]
    public override bool SupportsDocument() => definition.Document.IsSome;

    public override bool SaveDocument(RhinoDoc doc, BinaryArchiveWriter archive) =>
        Succeeded(definition.Document.Map(state => state.Save(doc, archive)));

    public override bool RestoreDocument(RhinoDoc doc, BinaryArchiveReader archive) =>
        Succeeded(definition.Document.Map(state => state.Restore(doc, archive)));

    public override void SnapshotRestored(RhinoDoc doc) =>
        _ = views.Run(definition.Restored.Map(restored => IO.lift(() => restored(doc)).Flatten()).IfNone(IO.pure(unit)));

    public override bool IsCurrentModelStateInAnySnapshot(
        RhinoDoc doc, BinaryArchiveReader archive, SimpleArrayBinaryArchiveReader archive_array, TextLog? text_log = null) =>
        views.Answer(definition.Document.Map(state => state.Held(archive, Stored(archive_array))).IfNone(IO.pure(value: true)), failed: false);

    // --- [OBJECTS]
    public override bool SupportsObjects() => definition.Objects.IsSome;

    public override bool SupportsObject(RhinoObject doc_object) =>
        views.Answer(definition.Objects.Map(objects => IO.lift(() => objects.Supports(doc_object))).IfNone(IO.pure(value: false)), failed: false);

    public override bool SaveObject(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveWriter archive) =>
        new SnapshotObject(doc, doc_object, transform) switch {
            var subject => Succeeded(definition.Objects.Map(objects => objects.State.Save(subject, archive))),
        };

    public override bool RestoreObject(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveReader archive) =>
        new SnapshotObject(doc, doc_object, transform) switch {
            var subject => Succeeded(definition.Objects.Map(objects => objects.State.Restore(subject, archive))),
        };

    public override bool ObjectTransformNotification(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveReader archive) => true;

    public override bool IsCurrentModelStateInAnySnapshot(
        RhinoDoc doc, RhinoObject doc_object, BinaryArchiveReader archive, SimpleArrayBinaryArchiveReader archive_array, TextLog? text_log = null) =>
        views.Answer(definition.Objects.Map(objects => objects.State.Held(archive, Stored(archive_array))).IfNone(IO.pure(value: true)), failed: false);

    // --- [ANIMATION]
    private readonly Atom<Option<SnapshotFrames<RhinoDoc>>> documentFrames = Atom(Option<SnapshotFrames<RhinoDoc>>.None);

    private readonly AtomHashMap<Guid, SnapshotFrames<SnapshotObject>> objectFrames = AtomHashMap<Guid, SnapshotFrames<SnapshotObject>>();

    public override bool SupportsAnimation() =>
        definition.Document.Exists(static state => state.Animates) || definition.Objects.Exists(static objects => objects.State.Animates);

    public override void AnimationStart(RhinoDoc doc, int iFrames) { }

    public override bool PrepareForDocumentAnimation(RhinoDoc doc, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop) =>
        Succeeded(
            from state in definition.Document
            select state.Prepare(doc, archive_start, archive_stop).Bind(frames => writeIO(documentFrames, frames)));

    public override void ExtendBoundingBoxForDocumentAnimation(RhinoDoc doc, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop, ref BoundingBox bbox) =>
        bbox = Extended(bbox, documentFrames.Value.Bind(static frames => frames.Extent));

    public override bool AnimateDocument(RhinoDoc doc, double dPos, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop) =>
        Succeeded(documentFrames.Value.Map(frames => frames.Animate(doc, dPos)));

    public override bool PrepareForObjectAnimation(
        RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop) =>
        new SnapshotObject(doc, doc_object, transform) switch {
            var subject => Succeeded(
                from objects in definition.Objects
                select objects.State.Prepare(subject, archive_start, archive_stop).Bind(frames => IO.lift(() => objectFrames.SwapKey(doc_object.Id, _ => frames)))),
        };

    public override void ExtendBoundingBoxForObjectAnimation(
        RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop, ref BoundingBox bbox) =>
        bbox = Extended(bbox, objectFrames.Find(doc_object.Id).Bind(static frames => frames.Extent));

    public override bool AnimateObject(
        RhinoDoc doc, RhinoObject doc_object, ref Transform transform, double dPos, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop) =>
        new SnapshotObject(doc, doc_object, transform) switch {
            var subject => Succeeded(objectFrames.Find(doc_object.Id).Map(frames => frames.Animate(subject, dPos))),
        };

    public override bool AnimationStop(RhinoDoc doc) =>
        views.Answer(
            from document in writeIO(documentFrames, None)
            from objects in IO.lift(objectFrames.Clear)
            select true,
            failed: false);

    private bool Succeeded<T>(Option<IO<T>> effect, [CallerMemberName] string member = "") =>
        views.Answer(effect.Map(static held => held.Map(static _ => true)).IfNone(IO.pure(value: false)), failed: false, member);

    private static BoundingBox Extended(BoundingBox box, Option<BoundingBox> extent) =>
        extent.Map(added => BoundingBox.Union(box, added)).IfNone(box);

    private static Seq<BinaryArchiveReader> Stored(SimpleArrayBinaryArchiveReader stored) =>
        toSeq(Range(0, stored.Count)).Map(stored.Get).Strict();
}
