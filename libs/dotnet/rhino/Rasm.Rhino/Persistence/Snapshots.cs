using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.SnapShots;
using Rhino.FileIO;
using Rhino.Runtime.InteropWrappers;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------

[ValueObject<string>(SkipIParsable = true)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<RhinoFault>]
public sealed partial class SnapshotText {
    public static Fin<SnapshotText> Validated(string value) => TryCreate(value, out SnapshotText? text, out RhinoFault? fault) ? text : fault;

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref string value) =>
        validationError = Quotable(value) ? null : new RhinoFault(value, RhinoReason.Unqualified);

    internal static bool Quotable(string value) => value.Length > 0 && !value.AsSpan().ContainsAny('"', '\r', '\n');
}

[ValueObject<string>(SkipIParsable = true, EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads, ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[ValidationError<RhinoFault>]
public sealed partial class SnapshotName {
    public static Fin<SnapshotName> Validated(string value) => TryCreate(value, out SnapshotName? name, out RhinoFault? fault) ? name : fault;

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref string value) =>
        validationError = SnapshotText.Quotable(value) ? null : new RhinoFault(value, RhinoReason.Unqualified);
}

[SmartEnum]
public sealed partial class SnapshotOperation {
    public static readonly SnapshotOperation Save = new(static (names, name) =>
        $"""_-Snapshots _Save _Parameters=_All "{name}" {(SnapshotChange.Holds(names, name) ? "_Yes " : "")}_Enter""");

    public static readonly SnapshotOperation Restore = new(static (names, name) =>
        SnapshotChange.Expecting(names, [(name, true)], $"""_-Snapshots _Restore "{name}" _Enter _Enter"""));

    public static readonly SnapshotOperation Delete = new(static (names, name) =>
        SnapshotChange.Expecting(names, [(name, true)], $"""_-Snapshots _Delete "{name}" _Enter"""));

    [UseDelegateFromConstructor]
    internal partial Fin<string> Script(Seq<string> names, SnapshotName name);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SnapshotChange {

    // --- [CASES]

    public sealed record Named(SnapshotOperation Operation, SnapshotName Name) : SnapshotChange;

    public sealed record Rename(SnapshotName Name, SnapshotName NewName) : SnapshotChange;

    public sealed record Import(InputPath Path) : SnapshotChange;

    public sealed record UserText(SnapshotName Name, SnapshotText Key, Option<SnapshotText> Value) : SnapshotChange;

    // --- [EXECUTION]

    public IO<Unit> Run(RhinoDoc doc) =>
        from names in IO.lift(() => Conversions.Rows(doc.Snapshots.Names))
        from script in IO.lift(Script(names))
        from ran in Documents.RunScript(doc, script, echo: false, display: None)
        select ran;

    // --- [SCRIPTS]

    private Fin<string> Script(Seq<string> names) => Switch(names,
        named: static (held, change) => change.Operation.Script(held, change.Name),
        rename: static (held, change) => Expecting(held, [(change.Name, true), (change.NewName, false)], $"""_-Snapshots _Rename "{change.Name}" "{change.NewName}" _Enter"""),
        import: static (_, change) => SnapshotText.Quotable(change.Path)
            ? $"""_-Snapshots _Import "{change.Path}" _Enter"""
            : new RhinoFault(change.Path, RhinoReason.Unreadable),
        userText: static (held, change) => Expecting(held, [(change.Name, true)], change.Value.Match(
            Some: value => $"""_-Snapshots _UserText "{change.Name}" _Set "{change.Key}" "{value}" _Enter _Enter""",
            None: () => $"""_-Snapshots _UserText "{change.Name}" _Set "{change.Key}" _Enter _Enter _Enter""")));

    internal static Fin<string> Expecting(Seq<string> names, Seq<(SnapshotName Name, bool Present)> rows, string script) =>
        rows.Traverse(row => Holds(names, row.Name) == row.Present ? Fin.Succ(unit)
            : new RhinoFault(row.Name, row.Present ? RhinoReason.Missing : RhinoReason.Taken)).As().Map(_ => script);

    internal static bool Holds(Seq<string> names, SnapshotName name) =>
        names.Exists(stored => name == stored);
}

public sealed record SnapshotState(StateName Name) : NamedState(Snapshots.Table, Name) {
    internal override IO<NamedWrite> Save(RhinoDoc doc) => Snapshots.Ran(doc, SnapshotOperation.Save, Name);
}

public sealed record SnapshotRestore(StateName Name) : NamedRestore(Snapshots.Table, Name) {
    internal override IO<NamedWrite> Restore(RhinoDoc doc) => Snapshots.Ran(doc, SnapshotOperation.Restore, Name);
}

public sealed record SnapshotObject(RhinoDoc Doc, RhinoObject Object, Transform Transform);

public sealed record SnapshotTween<TSubject, TState>(Func<TState, TState, double, TState> Between, Option<Func<TSubject, TState, TState, BoundingBox>> Bounds = default);

internal sealed record SnapshotFrames<TSubject>(Func<TSubject, double, IO<Unit>> Animate, Option<BoundingBox> Extent);

public sealed record SnapshotRecord<TSubject, TState>(uint TypeCode, DictionaryCodec<TState> Codec, Func<TSubject, IO<TState>> Read, Func<TSubject, TState, IO<Unit>> Write) where TState : notnull {
    public Option<SnapshotTween<TSubject, TState>> Tween { get; init; }

    public Option<Func<TSubject, TState, TState>> Transformed { get; init; }

    internal IO<Unit> Save(TSubject subject, BinaryArchiveWriter archive) =>
        from state in IO.lift(() => Read(subject)).Flatten()
        from written in AttachedData.Write(archive, TypeCode, Codec, state)
        select written;

    internal IO<Unit> Restore(TSubject subject, BinaryArchiveReader archive) =>
        from state in Decoded(archive)
        from written in IO.lift(() => Write(subject, Placed(subject, state))).Flatten()
        select written;

    internal IO<bool> InAny(BinaryArchiveReader current, Seq<BinaryArchiveReader> stored) =>
        from now in Decoded(current)
        from held in stored.TraverseM(Stored).As()
        select held.Exists(state => state.Exists(value => EqualityComparer<TState>.Default.Equals(value, now)));

    internal IO<Option<SnapshotFrames<TSubject>>> Prepare(TSubject subject, BinaryArchiveReader start, BinaryArchiveReader stop) =>
        Tween.Match(
            Some: tween =>
                from first in Decoded(start)
                from last in Decoded(stop)
                select Some(new SnapshotFrames<TSubject>(
                    (frame, position) => IO.lift(() => Write(frame, Placed(frame, tween.Between(first, last, position)))).Flatten(),
                    tween.Bounds.Map(bounds => bounds(subject, first, last)))),
            None: static () => IO.pure(Option<SnapshotFrames<TSubject>>.None));

    private TState Placed(TSubject subject, TState state) =>
        Transformed.Map(place => place(subject, state)).IfNone(state);

    private IO<TState> Decoded(BinaryArchiveReader archive) =>
        Stored(archive).Bind(found => IO.lift(found.ToFin(new RhinoFault(Codec.Name, RhinoReason.Missing))));

    private IO<Option<TState>> Stored(BinaryArchiveReader archive) =>
        from positioned in Products.Accepted(() => archive.SeekFromStart(0UL))
        from stored in AttachedData.Read(archive, TypeCode, Codec)
        select stored.Present;
}

public sealed record SnapshotDefinition<TDocument, TObject>(Guid Client, string Name, Func<string> Category) where TDocument : notnull where TObject : notnull {
    public Option<SnapshotRecord<RhinoDoc, TDocument>> Document { get; init; }

    public Option<(Func<RhinoObject, bool> Supports, SnapshotRecord<SnapshotObject, TObject> Record)> Objects { get; init; }

    public Option<Func<RhinoDoc, IO<Unit>>> Restored { get; init; }

    public IO<Unit> Register => Products.Accepted(() => SnapShotsClient.RegisterSnapShotClient(new DefinedSnapshotClient<TDocument, TObject>(this)));
}

// --- [SERVICES] ------------------------------------------------------------------------

internal sealed class DefinedSnapshotClient<TDocument, TObject>(SnapshotDefinition<TDocument, TObject> definition) : SnapShotsClient where TDocument : notnull where TObject : notnull {

    // --- [STATE]

    private readonly Atom<Option<SnapshotFrames<RhinoDoc>>> documentFrames = Atom(Option<SnapshotFrames<RhinoDoc>>.None);
    private readonly AtomHashMap<Guid, SnapshotFrames<SnapshotObject>> objectFrames = AtomHashMap<Guid, SnapshotFrames<SnapshotObject>>();

    // --- [IDENTITY]

    public override Guid PlugInId() => Session.PlugIn.Id;

    public override Guid ClientId() => definition.Client;

    public override string Category() => definition.Category();

    public override string Name() => definition.Name;

    // --- [DOCUMENTS]

    public override bool SupportsDocument() => definition.Document.IsSome;

    public override bool SaveDocument(RhinoDoc doc, BinaryArchiveWriter archive) => Succeeded(definition.Document.Map(state => state.Save(doc, archive)));

    public override bool RestoreDocument(RhinoDoc doc, BinaryArchiveReader archive) => Succeeded(definition.Document.Map(state => state.Restore(doc, archive)));

    public override void SnapshotRestored(RhinoDoc doc) => _ = Session.Run(definition.Restored.Map(restored => IO.lift(() => restored(doc)).Flatten()).IfNone(IO.pure(unit)));

    public override bool IsCurrentModelStateInAnySnapshot(RhinoDoc doc, BinaryArchiveReader archive, SimpleArrayBinaryArchiveReader archive_array, TextLog? text_log = null) =>
        Session.Run(definition.Document.Map(state => state.InAny(archive, Stored(archive_array))).IfNone(IO.pure(value: true)), failed: false);

    // --- [OBJECTS]

    public override bool SupportsObjects() => definition.Objects.IsSome;

    public override bool SupportsObject(RhinoObject doc_object) =>
        Session.Run(definition.Objects.Map(objects => IO.lift(() => objects.Supports(doc_object))).IfNone(IO.pure(value: false)), failed: false);

    public override bool SaveObject(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveWriter archive) {
        SnapshotObject subject = new(doc, doc_object, transform);
        return Succeeded(definition.Objects.Map(objects => objects.Record.Save(subject, archive)));
    }

    public override bool RestoreObject(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveReader archive) {
        SnapshotObject subject = new(doc, doc_object, transform);
        return Succeeded(definition.Objects.Map(objects => objects.Record.Restore(subject, archive)));
    }

    public override bool ObjectTransformNotification(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveReader archive) => SupportsObject(doc_object);

    public override bool IsCurrentModelStateInAnySnapshot(RhinoDoc doc, RhinoObject doc_object, BinaryArchiveReader archive, SimpleArrayBinaryArchiveReader archive_array, TextLog? text_log = null) =>
        Session.Run(definition.Objects.Map(objects => objects.Record.InAny(archive, Stored(archive_array))).IfNone(IO.pure(value: true)), failed: false);

    // --- [ANIMATION]

    public override bool SupportsAnimation() =>
        definition.Document.Exists(static state => state.Tween.IsSome) || definition.Objects.Exists(static objects => objects.Record.Tween.IsSome);

    public override void AnimationStart(RhinoDoc doc, int iFrames) { }

    public override bool PrepareForDocumentAnimation(RhinoDoc doc, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop) =>
        Succeeded(definition.Document.Map(state => state.Prepare(doc, archive_start, archive_stop).Bind(frames => writeIO(documentFrames, frames))));

    public override void ExtendBoundingBoxForDocumentAnimation(RhinoDoc doc, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop, ref BoundingBox bbox) =>
        bbox = Extended(bbox, documentFrames.Value.Bind(static frames => frames.Extent));

    public override bool AnimateDocument(RhinoDoc doc, double dPos, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop) =>
        Succeeded(documentFrames.Value.Map(frames => frames.Animate(doc, dPos)));

    public override bool PrepareForObjectAnimation(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop) {
        SnapshotObject subject = new(doc, doc_object, transform);
        return Succeeded(definition.Objects.Map(objects => objects.Record.Prepare(subject, archive_start, archive_stop)
            .Bind(frames => IO.lift(() => objectFrames.SwapKey(doc_object.Id, _ => frames)))));
    }

    public override void ExtendBoundingBoxForObjectAnimation(
        RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop, ref BoundingBox bbox) =>
        bbox = Extended(bbox, objectFrames.Find(doc_object.Id).Bind(static frames => frames.Extent));

    public override bool AnimateObject(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, double dPos, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop) {
        SnapshotObject subject = new(doc, doc_object, transform);
        return Succeeded(objectFrames.Find(doc_object.Id).Map(frames => frames.Animate(subject, dPos)));
    }

    public override bool AnimationStop(RhinoDoc doc) =>
        Session.Run(
            from document in writeIO(documentFrames, None)
            from objects in IO.lift(objectFrames.Clear)
            select true,
            failed: false);

    private static bool Succeeded<T>(Option<IO<T>> effect, [CallerMemberName] string member = "") =>
        Session.Run(effect.Map(static held => held.Map(static _ => true)).IfNone(IO.pure(value: false)), failed: false, member);

    private static BoundingBox Extended(BoundingBox box, Option<BoundingBox> extent) =>
        extent.Map(added => BoundingBox.Union(box, added)).IfNone(box);

    private static Seq<BinaryArchiveReader> Stored(SimpleArrayBinaryArchiveReader stored) =>
        toSeq(Range(0, stored.Count)).Map(stored.Get).Strict();
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Snapshots {
    public static NamedTable Table { get; } = new(nameof(Snapshots), StringComparer.OrdinalIgnoreCase) {
        Batches = false,
        Entries = Listed,
        Rename = static (doc, entry, name) =>
            IO.lift(() => (SnapshotName.Validated(entry.Name), SnapshotName.Validated(name)).Apply(static (current, renamed) => new SnapshotChange.Rename(current, renamed)).As())
                .Bind(rename => rename.Run(doc)).Map(static _ => NamedWrite.Undoable),
        Delete = static (doc, entry) => Ran(doc, SnapshotOperation.Delete, entry.Name),
        Hold = static (doc, _) =>
            from name in IO.lift(static () => SnapshotName.Validated(Guid.NewGuid().ToString("D")))
            from saved in new SnapshotChange.Named(SnapshotOperation.Save, name).Run(doc)
            select new NamedWrite(new SnapshotChange.Named(SnapshotOperation.Restore, name).Run(doc), new SnapshotChange.Named(SnapshotOperation.Delete, name).Run(doc)),
        Import = Some<Func<RhinoDoc, InputPath, Option<Seq<StateName>>, IO<NamedWrite>>>(static (doc, path, names) =>
            from whole in IO.lift(() => RhinoFault.Unless(names.IsNone, RhinoReason.Unsupported, names))
            from imported in new SnapshotChange.Import(path).Run(doc)
            select NamedWrite.Undoable),
    };

    private static IO<Seq<NamedEntry>> Listed(RhinoDoc doc) =>
        IO.lift(() => Conversions.Rows(doc.Snapshots.Names).Map(static name => new NamedEntry(Table, name, None)).Strict());

    internal static IO<NamedWrite> Ran(RhinoDoc doc, SnapshotOperation operation, string name) =>
        IO.lift(() => SnapshotName.Validated(name)).Bind(held => new SnapshotChange.Named(operation, held).Run(doc)).Map(static _ => NamedWrite.Undoable);
}
