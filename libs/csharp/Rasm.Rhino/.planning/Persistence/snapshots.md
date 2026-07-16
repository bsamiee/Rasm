# [RASM_RHINO_PERSISTENCE_SNAPSHOTS]

Snapshot scripting, plugin participation, and worksession reads (`Rasm.Rhino.Persistence`). `SnapshotAction` rows generate the serial-pinned command family and roster invariants. `SnapshotParticipant` is the sole `SnapShotsClient` subclass; admitted document, object, animation, and current-state probe lanes drive every abstract override. `SnapshotCodec` composes the schema-preserving `ArchiveIo` seam. `WorksessionFacts` detaches the reference-model roster without inventing mutation members.

## [01]-[INDEX]

- [02]-[SCRIPTED_MUTATION]: generated command rows, roster proof, and transient restore bracket.
- [03]-[PARTICIPATION]: admitted lanes and complete host adapter.
- [04]-[WORKSESSION]: detached reference-model identity and roster.
- [05]-[SURFACE_LEDGER]: ownership and entry points.

## [02]-[SCRIPTED_MUTATION]

- Owner: `SnapshotAction` rows carry command verb plus required roster state before and after execution. `SnapshotCommand` combines one admitted name with one action, and every commit returns a `SnapshotFact` receipt.
- Name admission: names reject quotes and line breaks before interpolation into the Rhino command macro.
- Host boundary: every mutation calls `RhinoApp.RunScript(uint, string, bool)` with the current document serial and requires `SessionNeed.Interrupt`, so headless sessions fail before scripting.
- Watcher gate: `RunScript` throws `ApplicationException` from inside an event watcher — the gating member is host-internal — so the caught host call is the gate and observation callbacks land the refusal on the rail.
- Roster proof: capture requires absence then proves presence, restore requires and preserves presence, and delete requires presence then proves absence.
- Transient restore: `Within` validates its body before capture, saves to an absent GUID sentinel, restores the target, runs the body, then restores and deletes the sentinel on every outcome while accumulating cleanup faults.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Globalization;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.SnapShots;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Runtime.InteropWrappers;

namespace Rasm.Rhino.Persistence;

// --- [SCRIPT_POLICY] ------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SnapshotAction {
    public static readonly SnapshotAction Capture = new(key: 0, verb: "_Save", presentBefore: false, presentAfter: true);
    public static readonly SnapshotAction Restore = new(key: 1, verb: "_Restore", presentBefore: true, presentAfter: true);
    public static readonly SnapshotAction Delete = new(key: 2, verb: "_Delete", presentBefore: true, presentAfter: false);

    internal string Verb { get; }
    internal bool PresentBefore { get; }
    internal bool PresentAfter { get; }
}

public sealed record SnapshotCommand {
    private SnapshotCommand(SnapshotAction action, string name) => (Action, Name) = (action, name);

    public SnapshotAction Action { get; }
    public string Name { get; }

    public static Fin<SnapshotCommand> Create(SnapshotAction action, string name, Op? key = null) {
        Op op = key.OrDefault();
        return from verb in Optional(action).ToFin(Fail: op.InvalidInput())
               from label in op.AcceptText(value: name)
               from _ in guard(label.IndexOfAny(['"', '\r', '\n']) < 0, op.InvalidInput()).ToFin()
               select new SnapshotCommand(action: verb, name: label);
    }
}

public sealed record SnapshotFact(SnapshotAction Action, string Name) : IDetachedDocumentResult;

internal sealed record SnapshotRoster(Seq<string> Names) : IDetachedDocumentResult;

// --- [SCRIPT_ENTRY] -------------------------------------------------------------------------
public static class Snapshots {
    public static Fin<Seq<string>> Roster(DocumentSession session) {
        Op op = Op.Of();
        return from context in Optional(session).ToFin(Fail: op.InvalidInput())
               from roster in context.Demand(
                   use: document => op.Catch(() => Fin.Succ(value: new SnapshotRoster(Names: toSeq(document.Snapshots.Names).Strict()))),
                   key: op,
                   needs: [SessionNeed.Read])
               select roster.Names;
    }

    public static Fin<SnapshotFact> Commit(DocumentSession session, SnapshotCommand command) {
        Op op = Op.Of();
        return from context in Optional(session).ToFin(Fail: op.InvalidInput())
               from active in Optional(command).ToFin(Fail: op.InvalidInput())
               from result in context.Demand(
                   use: document => Run(document: document, command: active, op: op),
                   key: op,
                   needs: [SessionNeed.Mutate, SessionNeed.Interrupt])
               select result;
    }

    public static Fin<TResult> Within<TResult>(DocumentSession session, string target, Func<Fin<TResult>> body)
        where TResult : IDetachedDocumentResult {
        Op op = Op.Of();
        string sentinel = string.Create(CultureInfo.InvariantCulture, $"__rasm_{Guid.NewGuid():N}");
        return from context in Optional(session).ToFin(Fail: op.InvalidInput())
               from run in Optional(body).ToFin(Fail: op.InvalidInput())
               from targetName in SnapshotCommand.Create(action: SnapshotAction.Restore, name: target, key: op)
               from sentinelSave in SnapshotCommand.Create(action: SnapshotAction.Capture, name: sentinel, key: op)
               from sentinelRestore in SnapshotCommand.Create(action: SnapshotAction.Restore, name: sentinel, key: op)
               from sentinelDelete in SnapshotCommand.Create(action: SnapshotAction.Delete, name: sentinel, key: op)
               from _ in Commit(session: context, command: sentinelSave)
               from result in Commit(session: context, command: targetName)
                   .Bind(_ => op.Catch(run))
                   .Match(
                       Succ: value => Unwind(context, sentinelRestore, sentinelDelete).Match(
                           Succ: _ => Fin.Succ(value: value),
                           Fail: cleanup => Fin.Fail<TResult>(error: cleanup)),
                       Fail: primary => Unwind(context, sentinelRestore, sentinelDelete).Match(
                           Succ: _ => Fin.Fail<TResult>(error: primary),
                           Fail: cleanup => Fin.Fail<TResult>(error: primary + cleanup)))
               select result;
    }

    private static Fin<Unit> Unwind(
        DocumentSession session,
        SnapshotCommand restore,
        SnapshotCommand delete) =>
        (Commit(session: session, command: restore).ToValidation(),
         Commit(session: session, command: delete).ToValidation())
            .Apply(static (_, _) => unit)
            .As()
            .ToFin();

    private static Fin<SnapshotFact> Run(RhinoDoc document, SnapshotCommand command, Op op) =>
        from before in Contains(document: document, name: command.Name, op: op)
        from _before in guard(before == command.Action.PresentBefore, before ? op.InvalidInput() : op.MissingContext()).ToFin()
        from _run in op.Catch(() => op.Confirm(success: RhinoApp.RunScript(
            documentSerialNumber: document.RuntimeSerialNumber,
            script: string.Create(
                CultureInfo.InvariantCulture,
                $"_-Snapshot {command.Action.Verb} _Name \"{command.Name}\" _Enter"),
            echo: false)))
        from after in Contains(document: document, name: command.Name, op: op)
        from _after in guard(after == command.Action.PresentAfter, op.InvalidResult(detail: command.Name)).ToFin()
        select new SnapshotFact(Action: command.Action, Name: command.Name);

    private static Fin<bool> Contains(RhinoDoc document, string name, Op op) =>
        op.Catch(() => Fin.Succ(value: toSeq(document.Snapshots.Names).Exists(candidate => string.Equals(
            a: candidate,
            b: name,
            comparisonType: StringComparison.Ordinal))));
}
```

## [03]-[PARTICIPATION]

- Identity: `SnapshotCategory` admits only host category methods; `ParticipantSpec.Create` validates plugin id, client id, display name, codec, every supplied lane, and at least one capability lane.
- Codec: `SnapshotCodec` writes its current schema and upgrades every `ArchiveEnvelope` before a restore, animation, transform notification, or current-state probe.
- Lanes: document and object lanes include capture, restore, restored/support behavior, and their respective current-state probe. Animation carries every interpolation and bounding-box operator.
- Adapter: every boolean override converts lane exceptions and `Fin` failures to `false`; every void override executes through the same caught rail. Absent lanes report unsupported and preserve incoming bounds.
- Probe: both `IsCurrentModelStateInAnySnapshot` overrides decode the current archive and every `SimpleArrayBinaryArchiveReader` entry before invoking their lane probe; no hardcoded answer remains.
- Registration: constructor creation and native `RegisterSnapShotClient` run once per client id under a unique-token `Atom` reservation that remains correct across CAS retries. Native registration failure disposes the client and keeps the id consumed because the host constructor already inserted that instance into its process list and exposes no removal member.

```csharp signature
// --- [PARTICIPANT_VOCABULARY] ---------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SnapshotCategory {
    public static readonly SnapshotCategory Application = new(key: SnapShotsClient.ApplicationCategory());
    public static readonly SnapshotCategory Document = new(key: SnapShotsClient.DocumentCategory());
    public static readonly SnapshotCategory Rendering = new(key: SnapShotsClient.RenderingCategory());
    public static readonly SnapshotCategory Views = new(key: SnapShotsClient.ViewsCategory());
    public static readonly SnapshotCategory Objects = new(key: SnapShotsClient.ObjectsCategory());
    public static readonly SnapshotCategory Layers = new(key: SnapShotsClient.LayersCategory());
    public static readonly SnapshotCategory Lights = new(key: SnapShotsClient.LightsCategory());
}

public sealed record SnapshotCodec(
    ArchiveSchema Schema,
    Func<ArchiveEnvelope, Op, Fin<ArchiveMap>> Upgrade) {
    internal Fin<SnapshotCodec> Admit(Op op) =>
        from schema in Optional(Schema).ToFin(Fail: op.InvalidInput())
        from upgrade in Optional(Upgrade).ToFin(Fail: op.InvalidInput())
        select new SnapshotCodec(Schema: schema, Upgrade: upgrade);

    internal Fin<ArchiveMap> Read(BinaryArchiveReader archive, Op op) =>
        ArchiveIo.Read(archive: archive, key: op).Bind(stored => Upgrade(stored, op));
}

public sealed record DocumentLane(
    Func<RhinoDoc, Fin<ArchiveMap>> Save,
    Func<RhinoDoc, ArchiveMap, Fin<Unit>> Restore,
    Func<RhinoDoc, Fin<Unit>> Restored,
    Func<RhinoDoc, ArchiveMap, Seq<ArchiveMap>, TextLog?, Fin<bool>> IsCurrent) {
    internal Fin<DocumentLane> Admit(Op op) =>
        from save in Optional(Save).ToFin(Fail: op.InvalidInput())
        from restore in Optional(Restore).ToFin(Fail: op.InvalidInput())
        from restored in Optional(Restored).ToFin(Fail: op.InvalidInput())
        from probe in Optional(IsCurrent).ToFin(Fail: op.InvalidInput())
        select new DocumentLane(Save: save, Restore: restore, Restored: restored, IsCurrent: probe);
}

public sealed record ObjectLane(
    Func<RhinoObject, Fin<bool>> Supports,
    Func<RhinoDoc, RhinoObject, Transform, Fin<ArchiveMap>> Save,
    Func<RhinoDoc, RhinoObject, Transform, ArchiveMap, Fin<Unit>> Restore,
    Func<RhinoDoc, RhinoObject, Transform, ArchiveMap, Fin<bool>> TransformNotification,
    Func<RhinoDoc, RhinoObject, ArchiveMap, Seq<ArchiveMap>, TextLog?, Fin<bool>> IsCurrent) {
    internal Fin<ObjectLane> Admit(Op op) =>
        from supports in Optional(Supports).ToFin(Fail: op.InvalidInput())
        from save in Optional(Save).ToFin(Fail: op.InvalidInput())
        from restore in Optional(Restore).ToFin(Fail: op.InvalidInput())
        from notification in Optional(TransformNotification).ToFin(Fail: op.InvalidInput())
        from probe in Optional(IsCurrent).ToFin(Fail: op.InvalidInput())
        select new ObjectLane(
            Supports: supports,
            Save: save,
            Restore: restore,
            TransformNotification: notification,
            IsCurrent: probe);
}

public sealed record AnimationLane(
    Func<RhinoDoc, int, Fin<Unit>> Start,
    Func<RhinoDoc, ArchiveMap, ArchiveMap, Fin<bool>> PrepareDocument,
    Func<RhinoDoc, double, ArchiveMap, ArchiveMap, Fin<bool>> AnimateDocument,
    Func<RhinoDoc, RhinoObject, Transform, ArchiveMap, ArchiveMap, Fin<bool>> PrepareObject,
    Func<RhinoDoc, RhinoObject, Transform, double, ArchiveMap, ArchiveMap, Fin<bool>> AnimateObject,
    Func<RhinoDoc, Fin<bool>> Stop,
    Func<RhinoDoc, ArchiveMap, ArchiveMap, BoundingBox, Fin<BoundingBox>> ExtendDocumentBound,
    Func<RhinoDoc, RhinoObject, Transform, ArchiveMap, ArchiveMap, BoundingBox, Fin<BoundingBox>> ExtendObjectBound) {
    internal Fin<AnimationLane> Admit(Op op) =>
        from start in Optional(Start).ToFin(Fail: op.InvalidInput())
        from prepareDocument in Optional(PrepareDocument).ToFin(Fail: op.InvalidInput())
        from animateDocument in Optional(AnimateDocument).ToFin(Fail: op.InvalidInput())
        from prepareObject in Optional(PrepareObject).ToFin(Fail: op.InvalidInput())
        from animateObject in Optional(AnimateObject).ToFin(Fail: op.InvalidInput())
        from stop in Optional(Stop).ToFin(Fail: op.InvalidInput())
        from extendDocument in Optional(ExtendDocumentBound).ToFin(Fail: op.InvalidInput())
        from extendObject in Optional(ExtendObjectBound).ToFin(Fail: op.InvalidInput())
        select new AnimationLane(
            Start: start,
            PrepareDocument: prepareDocument,
            AnimateDocument: animateDocument,
            PrepareObject: prepareObject,
            AnimateObject: animateObject,
            Stop: stop,
            ExtendDocumentBound: extendDocument,
            ExtendObjectBound: extendObject);
}

public sealed record ParticipantSpec {
    private ParticipantSpec(
        Guid plugInId,
        Guid clientId,
        SnapshotCategory category,
        string name,
        SnapshotCodec codec,
        Option<DocumentLane> document,
        Option<ObjectLane> objects,
        Option<AnimationLane> animation) =>
        (PlugInId, ClientId, Category, Name, Codec, Document, Objects, Animation) =
        (plugInId, clientId, category, name, codec, document, objects, animation);

    public Guid PlugInId { get; }
    public Guid ClientId { get; }
    public SnapshotCategory Category { get; }
    public string Name { get; }
    public SnapshotCodec Codec { get; }
    public Option<DocumentLane> Document { get; }
    public Option<ObjectLane> Objects { get; }
    public Option<AnimationLane> Animation { get; }

    public static Fin<ParticipantSpec> Create(
        Guid plugInId,
        Guid clientId,
        SnapshotCategory category,
        string name,
        SnapshotCodec codec,
        Option<DocumentLane> document = default,
        Option<ObjectLane> objects = default,
        Option<AnimationLane> animation = default) {
        Op op = Op.Of();
        return from _plugin in guard(plugInId != Guid.Empty, op.InvalidInput()).ToFin()
               from _client in guard(clientId != Guid.Empty, op.InvalidInput()).ToFin()
               from group in Optional(category).ToFin(Fail: op.InvalidInput())
               from label in op.AcceptText(value: name)
               from format in Optional(codec).ToFin(Fail: op.InvalidInput()).Bind(value => value.Admit(op: op))
               from documentLane in document.Traverse(value => value.Admit(op: op)).As()
               from objectLane in objects.Traverse(value => value.Admit(op: op)).As()
               from animationLane in animation.Traverse(value => value.Admit(op: op)).As()
               from _capability in guard(documentLane.IsSome || objectLane.IsSome || animationLane.IsSome, op.InvalidInput()).ToFin()
               select new ParticipantSpec(
                   plugInId: plugInId,
                   clientId: clientId,
                   category: group,
                   name: label,
                   codec: format,
                   document: documentLane,
                   objects: objectLane,
                   animation: animationLane);
    }
}

// --- [HOST_ADAPTER] -------------------------------------------------------------------------
public sealed class SnapshotParticipant : SnapShotsClient {
    private static readonly Atom<HashMap<Guid, Guid>> Registered = Atom(value: HashMap<Guid, Guid>());
    private readonly ParticipantSpec _spec;

    private SnapshotParticipant(ParticipantSpec spec) => _spec = spec;

    public static Fin<Unit> Enlist(ParticipantSpec spec) {
        Op op = Op.Of(name: nameof(SnapshotParticipant));
        return from declared in Optional(spec).ToFin(Fail: op.InvalidInput())
               from claimed in op.Catch(() => {
                   Guid token = Guid.NewGuid();
                   HashMap<Guid, Guid> held = Registered.Swap(state => state.Find(declared.ClientId).IsSome
                       ? state
                       : state.Add(declared.ClientId, token));
                   return guard(held.Find(declared.ClientId).Exists(candidate => candidate == token), op.InvalidInput()).ToFin();
               })
               from registered in op.Catch(() => {
                   SnapshotParticipant participant = new(spec: declared);
                   if (RegisterSnapShotClient(client: participant)) {
                       return Fin.Succ(value: unit);
                   }

                   participant.Dispose();
                   return Fin.Fail<Unit>(error: op.InvalidResult());
               })
               select registered;
    }

    public override Guid PlugInId() => _spec.PlugInId;
    public override Guid ClientId() => _spec.ClientId;
    public override string Category() => _spec.Category.Key;
    public override string Name() => _spec.Name;

    public override bool SupportsDocument() => _spec.Document.IsSome;

    public override bool SaveDocument(RhinoDoc doc, BinaryArchiveWriter archive) =>
        _spec.Document.Match(
            Some: lane => Result(() => lane.Save(doc)
                .Bind(payload => ArchiveIo.Write(archive: archive, schema: _spec.Codec.Schema, payload: payload))
                .Map(static _ => true)),
            None: static () => false);

    public override bool RestoreDocument(RhinoDoc doc, BinaryArchiveReader archive) =>
        _spec.Document.Match(
            Some: lane => Result(() => _spec.Codec.Read(archive: archive, op: Operation())
                .Bind(payload => lane.Restore(doc, payload))
                .Map(static _ => true)),
            None: static () => false);

    public override void SnapshotRestored(RhinoDoc doc) =>
        _spec.Document.IfSome(lane => Effect(() => lane.Restored(doc)));

    public override bool SupportsObjects() => _spec.Objects.IsSome;

    public override bool SupportsObject(RhinoObject rhObject) =>
        _spec.Objects.Match(
            Some: lane => Result(() => lane.Supports(rhObject)),
            None: static () => false);

    public override bool SaveObject(RhinoDoc doc, RhinoObject rhObject, ref Transform transform, BinaryArchiveWriter archive) {
        Transform accumulated = transform;
        return _spec.Objects.Match(
            Some: lane => Result(() => lane.Save(doc, rhObject, accumulated)
                .Bind(payload => ArchiveIo.Write(archive: archive, schema: _spec.Codec.Schema, payload: payload))
                .Map(static _ => true)),
            None: static () => false);
    }

    public override bool RestoreObject(RhinoDoc doc, RhinoObject rhObject, ref Transform transform, BinaryArchiveReader archive) {
        Transform accumulated = transform;
        return _spec.Objects.Match(
            Some: lane => Result(() => _spec.Codec.Read(archive: archive, op: Operation())
                .Bind(payload => lane.Restore(doc, rhObject, accumulated, payload))
                .Map(static _ => true)),
            None: static () => false);
    }

    public override bool ObjectTransformNotification(
        RhinoDoc doc,
        RhinoObject rhObject,
        ref Transform transform,
        BinaryArchiveReader archive) {
        Transform accumulated = transform;
        return _spec.Objects.Match(
            Some: lane => Result(() => _spec.Codec.Read(archive: archive, op: Operation())
                .Bind(payload => lane.TransformNotification(doc, rhObject, accumulated, payload))),
            None: static () => false);
    }

    public override bool SupportsAnimation() => _spec.Animation.IsSome;

    public override void AnimationStart(RhinoDoc doc, int iFrames) =>
        _spec.Animation.IfSome(lane => Effect(() => lane.Start(doc, iFrames)));

    public override bool PrepareForDocumentAnimation(RhinoDoc doc, BinaryArchiveReader start, BinaryArchiveReader stop) =>
        _spec.Animation.Match(
            Some: lane => Result(() => Pair(start, stop).Bind(pair => lane.PrepareDocument(doc, pair.Start, pair.Stop))),
            None: static () => false);

    public override bool AnimateDocument(RhinoDoc doc, double dPos, BinaryArchiveReader start, BinaryArchiveReader stop) =>
        _spec.Animation.Match(
            Some: lane => Result(() => Pair(start, stop).Bind(pair => lane.AnimateDocument(doc, dPos, pair.Start, pair.Stop))),
            None: static () => false);

    public override bool PrepareForObjectAnimation(
        RhinoDoc doc,
        RhinoObject rhObject,
        ref Transform transform,
        BinaryArchiveReader start,
        BinaryArchiveReader stop) {
        Transform accumulated = transform;
        return _spec.Animation.Match(
            Some: lane => Result(() => Pair(start, stop).Bind(pair => lane.PrepareObject(doc, rhObject, accumulated, pair.Start, pair.Stop))),
            None: static () => false);
    }

    public override bool AnimateObject(
        RhinoDoc doc,
        RhinoObject rhObject,
        ref Transform transform,
        double dPos,
        BinaryArchiveReader start,
        BinaryArchiveReader stop) {
        Transform accumulated = transform;
        return _spec.Animation.Match(
            Some: lane => Result(() => Pair(start, stop).Bind(pair => lane.AnimateObject(doc, rhObject, accumulated, dPos, pair.Start, pair.Stop))),
            None: static () => false);
    }

    public override bool AnimationStop(RhinoDoc doc) =>
        _spec.Animation.Match(
            Some: lane => Result(() => lane.Stop(doc)),
            None: static () => true);

    public override void ExtendBoundingBoxForDocumentAnimation(
        RhinoDoc doc,
        BinaryArchiveReader start,
        BinaryArchiveReader stop,
        ref BoundingBox bbox) {
        BoundingBox held = bbox;
        bbox = _spec.Animation.Match(
            Some: lane => Value(() => Pair(start, stop).Bind(pair => lane.ExtendDocumentBound(doc, pair.Start, pair.Stop, held)), held),
            None: () => held);
    }

    public override void ExtendBoundingBoxForObjectAnimation(
        RhinoDoc doc,
        RhinoObject rhObject,
        ref Transform transform,
        BinaryArchiveReader start,
        BinaryArchiveReader stop,
        ref BoundingBox bbox) {
        Transform accumulated = transform;
        BoundingBox held = bbox;
        bbox = _spec.Animation.Match(
            Some: lane => Value(
                () => Pair(start, stop).Bind(pair => lane.ExtendObjectBound(doc, rhObject, accumulated, pair.Start, pair.Stop, held)),
                held),
            None: () => held);
    }

    public override bool IsCurrentModelStateInAnySnapshot(
        RhinoDoc doc,
        BinaryArchiveReader archive,
        SimpleArrayBinaryArchiveReader archiveArray,
        TextLog? textLog = null) =>
        _spec.Document.Match(
            Some: lane => Result(() => ProbeSet(archive, archiveArray)
                .Bind(probe => lane.IsCurrent(doc, probe.Current, probe.Snapshots, textLog))),
            None: static () => false);

    public override bool IsCurrentModelStateInAnySnapshot(
        RhinoDoc doc,
        RhinoObject rhObject,
        BinaryArchiveReader archive,
        SimpleArrayBinaryArchiveReader archiveArray,
        TextLog? textLog = null) =>
        _spec.Objects.Match(
            Some: lane => Result(() => ProbeSet(archive, archiveArray)
                .Bind(probe => lane.IsCurrent(doc, rhObject, probe.Current, probe.Snapshots, textLog))),
            None: static () => false);

    private Fin<(ArchiveMap Start, ArchiveMap Stop)> Pair(BinaryArchiveReader start, BinaryArchiveReader stop) {
        Op op = Operation();
        return from first in _spec.Codec.Read(archive: start, op: op)
               from second in _spec.Codec.Read(archive: stop, op: op)
               select (first, second);
    }

    private Fin<(ArchiveMap Current, Seq<ArchiveMap> Snapshots)> ProbeSet(
        BinaryArchiveReader archive,
        SimpleArrayBinaryArchiveReader archiveArray) {
        Op op = Operation();
        return from readers in Optional(archiveArray).ToFin(Fail: op.InvalidInput())
               from current in _spec.Codec.Read(archive: archive, op: op)
               from snapshots in toSeq(Enumerable.Range(0, readers.Count))
                   .TraverseM(index => _spec.Codec.Read(archive: readers.Get(index), op: op))
                   .As()
               select (current, snapshots);
    }

    private static Op Operation() => Op.Of(name: nameof(SnapshotParticipant));

    private static bool Result(Func<Fin<bool>> use) {
        Op op = Operation();
        return op.Catch(use).IfFail(false);
    }

    private static Unit Effect(Func<Fin<Unit>> use) {
        Op op = Operation();
        _ = op.Catch(use);
        return unit;
    }

    private static T Value<T>(Func<Fin<T>> use, T fallback) {
        Op op = Operation();
        return op.Catch(use).IfFail(fallback);
    }
}
```

## [04]-[WORKSESSION]

- Owner: `WorksessionFacts` carries file, name, runtime serial, host count, and model paths without forcing the known count/path disagreement into one value.
- Absence: `FileName` is null and `Name` is empty when no saved worksession identity exists, so both project to `None`.
- Roster: `ModelCount` may exceed `ModelPaths.Count` by one when the active model is unsaved; `UnsavedActive` states that host fact.
- Resolution: `ModelPathFromSerialNumber` is an instance member of the live worksession, so model-serial resolution rides `Of` — requested serials reject zero before the read window and land in `ResolvedPaths`; `FileNameFromRuntimeSerialNumber` is the one static host resolver and backs `FileOf`.
- Mutation: worksession changes remain command-owned, and `DocumentStream` owns `EventFamily.WorksessionFile` observation; `EventPayload.Worksession` model serials resolve to paths through `Of`.

```csharp signature
// --- [WORKSESSION_MODEL] --------------------------------------------------------------------
public sealed record WorksessionFacts(
    Option<string> File,
    Option<string> Name,
    uint Serial,
    int ModelCount,
    Seq<string> ModelPaths,
    HashMap<uint, string> ResolvedPaths) : IDetachedDocumentResult {
    public bool UnsavedActive => ModelCount == ModelPaths.Count + 1;

    public static Fin<WorksessionFacts> Of(DocumentSession session, params ReadOnlySpan<uint> modelSerials) {
        Op op = Op.Of();
        return from context in Optional(session).ToFin(Fail: op.InvalidInput())
               from serials in toSeq(modelSerials.ToArray()).TraverseM(serial => serial > 0u
                       ? Fin.Succ(value: serial)
                       : Fin.Fail<uint>(error: op.InvalidInput()))
                   .As()
               from facts in context.Demand(
                   use: document => op.Catch(() => Optional(document.Worksession)
                       .ToFin(Fail: op.MissingContext())
                       .Bind(live => serials.TraverseM(serial => op.Catch(() =>
                               Optional(live.ModelPathFromSerialNumber(modelSerialNumber: serial))
                                   .Filter(static value => value.Length > 0)
                                   .ToFin(Fail: op.MissingContext()))
                               .Map(path => (serial, path)))
                           .As()
                           .Map(resolved => new WorksessionFacts(
                               File: Optional(live.FileName).Filter(static value => value.Length > 0),
                               Name: Optional(live.Name).Filter(static value => value.Length > 0),
                               Serial: live.RuntimeSerialNumber,
                               ModelCount: live.ModelCount,
                               ModelPaths: toSeq(live.ModelPaths ?? []).Strict(),
                               ResolvedPaths: resolved.Fold(
                                   HashMap<uint, string>(),
                                   static (state, pair) => state.AddOrUpdate(pair.serial, pair.path)))))),
                   key: op,
                   needs: [SessionNeed.Read])
               select facts;
    }

    public static Fin<string> FileOf(uint runtimeSerialNumber, Op? key = null) {
        Op op = key.OrDefault();
        return from serial in runtimeSerialNumber > 0u
                   ? Fin.Succ(value: runtimeSerialNumber)
                   : Fin.Fail<uint>(error: op.InvalidInput())
               from path in op.Catch(() => Optional(Worksession.FileNameFromRuntimeSerialNumber(serial))
                   .Filter(static value => value.Length > 0)
                   .ToFin(Fail: op.MissingContext()))
               select path;
    }
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]           | [OWNER]                                         | [FORM]                                    | [ENTRY]                          |
| :-----: | :------------------ | :---------------------------------------------- | :---------------------------------------- | :------------------------------- |
|  [01]   | scripted action     | `SnapshotAction`                                | verb and roster policy row                | `Create`                         |
|  [02]   | snapshot table      | `Snapshots`                                     | roster read and live commit               | `Roster` / `Commit` / `Within`   |
|  [03]   | commit receipt      | `SnapshotFact`                                  | action plus proven name                   | `Snapshots.Commit`               |
|  [04]   | participant codec   | `SnapshotCodec`                                 | schema write and envelope upgrade         | `Read`                           |
|  [05]   | participation lanes | `DocumentLane` / `ObjectLane` / `AnimationLane` | admitted callbacks                        | `Create`                         |
|  [06]   | host adapter        | `SnapshotParticipant`                           | total lane dispatch                       | `Enlist`                         |
|  [07]   | worksession read    | `WorksessionFacts`                              | detached identity, roster, resolved paths | `Of`                             |
|  [08]   | worksession file    | `WorksessionFacts`                              | serial resolver                           | `FileOf`                         |
