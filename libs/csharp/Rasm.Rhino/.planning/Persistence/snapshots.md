# [SNAPSHOTS]

`SnapshotOperation` closes roster reads and scripted mutations behind `Snapshots.Commit`. `SnapshotParticipant` is the sole `SnapShotsClient` adapter; one capability registry supplies load-bearing document, object, and animation lanes, while `SnapshotCodec` composes `ArchiveIo.Cross` for every archive crossing.

## [01]-[SCRIPTED_TABLE]

Rhino exposes only `SnapshotTable.Names`; capture, restore, and delete therefore route through serial-pinned `RhinoApp.RunScript`. `SnapshotVerb` owns script grammar and roster predicates as one policy row, and generated operation routing makes every scripted case exhaustive.

```csharp signature
namespace Rasm.Rhino.Persistence;

using System.Globalization;
using LanguageExt;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Thinktecture;
using static LanguageExt.Prelude;

[ValueObject<string>]
public readonly partial struct SnapshotName
{
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            validationError = new ValidationError("Snapshot name is empty or unsafe for Rhino command scripting.");
            return;
        }

        value = value.Trim();
        validationError = value.IndexOfAny(['\r', '\n', '"']) >= 0
            ? new ValidationError("Snapshot name is empty or unsafe for Rhino command scripting.")
            : null;
    }
}

[Union]
public abstract partial record SnapshotOperation
{
    public sealed record RosterCase : SnapshotOperation;
    public sealed record CaptureCase(SnapshotName Name) : SnapshotOperation;
    public sealed record RestoreCase(SnapshotName Name) : SnapshotOperation;
    public sealed record DeleteCase(SnapshotName Name) : SnapshotOperation;
}

[SmartEnum<string>]
public sealed partial class SnapshotVerb
{
    public static readonly SnapshotVerb Capture = Of("capture", "_Save", requiresPresent: false, leavesPresent: true);
    public static readonly SnapshotVerb Restore = Of("restore", "_Restore", requiresPresent: true, leavesPresent: true);
    public static readonly SnapshotVerb Delete = Of("delete", "_Delete", requiresPresent: true, leavesPresent: false);

    internal bool RequiresPresent { get; }
    internal bool LeavesPresent { get; }

    [UseDelegateFromConstructor]
    internal partial string Script(SnapshotName name);

    private static SnapshotVerb Of(string key, string token, bool requiresPresent, bool leavesPresent) =>
        new(
            key,
            requiresPresent,
            leavesPresent,
            script: name => string.Create(CultureInfo.InvariantCulture, $"_-Snapshot {token} _Name \"{name.Value}\" _Enter"));
}

public sealed record SnapshotRoster(Seq<SnapshotName> Names);

public sealed record SnapshotMutationReceipt(
    SnapshotOperation Operation,
    SnapshotRoster Before,
    SnapshotRoster After,
    uint DocumentSerial);

[Union]
public abstract partial record SnapshotAnswer
{
    public sealed record RosterCase(SnapshotRoster Roster) : SnapshotAnswer;
    public sealed record MutationCase(SnapshotMutationReceipt Receipt) : SnapshotAnswer;
}

public static class Snapshots
{
    public static Fin<SnapshotAnswer> Commit(
        DocumentSession session,
        SnapshotOperation operation,
        Op? key = null)
    {
        Op op = key.OrDefault();
        return from owner in Optional(session).ToFin(Fail: op.MissingContext())
               from request in Optional(operation).ToFin(Fail: op.InvalidInput())
               from routed in Admit(request, op)
               from answer in owner.Demand(
                   use: document => routed.Command.Match(
                       Some: command => Run(document, routed.Operation, command.Name, command.Verb, op)
                           .Map<SnapshotAnswer>(static receipt => new SnapshotAnswer.MutationCase(receipt)),
                       None: () => Roster(document, op).Map<SnapshotAnswer>(static roster => new SnapshotAnswer.RosterCase(roster))),
                   key: op,
                   needs: routed.Command.IsSome
                       ? [SessionNeed.Read, SessionNeed.Mutate, SessionNeed.Interrupt]
                       : [SessionNeed.Read])
               select answer;
    }

    public static Fin<T> Within<T>(
        DocumentSession session,
        SnapshotName target,
        Func<Fin<T>> use,
        Op? key = null)
    {
        Op op = key.OrDefault();
        return from body in Optional(use).ToFin(Fail: op.InvalidInput())
               from sentinel in op.AcceptValidated<SnapshotName>($"rasm-{Guid.NewGuid():N}")
               from _capture in Commit(session, new SnapshotOperation.CaptureCase(sentinel), op)
               from outcome in Sealed(
                   op.Catch(() =>
                       from _restore in Commit(session, new SnapshotOperation.RestoreCase(target), op)
                       from value in body()
                       select value),
                   () => Commit(session, new SnapshotOperation.RestoreCase(sentinel), op).Map(static _ => unit),
                   () => Commit(session, new SnapshotOperation.DeleteCase(sentinel), op).Map(static _ => unit))
               select outcome;
    }

    private static Fin<T> Sealed<T>(Fin<T> body, params Func<Fin<Unit>>[] finals) =>
        toSeq(finals).Fold(body, static (state, final) => state.Match(
            Succ: value => final().Map(_ => value),
            Fail: primary => final().Match(
                Succ: _ => Fail<T>(primary),
                Fail: secondary => Fail<T>(primary + secondary))));

    private static Fin<SnapshotMutationReceipt> Run(
        RhinoDoc document,
        SnapshotOperation operation,
        SnapshotName name,
        SnapshotVerb verb,
        Op op) =>
        from before in Roster(document, op)
        from _precondition in guard(before.Names.Contains(name) == verb.RequiresPresent, op.InvalidInput()).ToFin()
        from _run in op.Catch(() => op.Confirm(RhinoApp.RunScript(
            document.RuntimeSerialNumber,
            verb.Script(name),
            echo: false)))
        from after in Roster(document, op)
        from _postcondition in guard(after.Names.Contains(name) == verb.LeavesPresent, op.InvalidResult()).ToFin()
        select new SnapshotMutationReceipt(operation, before, after, document.RuntimeSerialNumber);

    private static Fin<SnapshotRoster> Roster(RhinoDoc document, Op op) =>
        op.Catch(() => document.Snapshots.Names
            .Map(name => op.AcceptValidated<SnapshotName>(name))
            .Traverse(static value => value)
            .Map(values => new SnapshotRoster(values.OrderBy(static value => value.Value, StringComparer.Ordinal).ToSeq())));

    private static Fin<(SnapshotOperation Operation, Option<(SnapshotName Name, SnapshotVerb Verb)> Command)> Admit(
        SnapshotOperation operation,
        Op op) => operation.Switch<Op, Fin<(SnapshotOperation, Option<(SnapshotName, SnapshotVerb)>)>>(
        state: op,
        rosterCase: static (_, _) => Fin.Succ(value: (
            (SnapshotOperation)new SnapshotOperation.RosterCase(),
            Option<(SnapshotName, SnapshotVerb)>.None)),
        captureCase: static (op, capture) => op.AcceptValidated<SnapshotName>(capture.Name.Value)
            .Map(static name => (
                (SnapshotOperation)new SnapshotOperation.CaptureCase(name),
                Some((name, SnapshotVerb.Capture)))),
        restoreCase: static (op, restore) => op.AcceptValidated<SnapshotName>(restore.Name.Value)
            .Map(static name => (
                (SnapshotOperation)new SnapshotOperation.RestoreCase(name),
                Some((name, SnapshotVerb.Restore)))),
        deleteCase: static (op, delete) => op.AcceptValidated<SnapshotName>(delete.Name.Value)
            .Map(static name => (
                (SnapshotOperation)new SnapshotOperation.DeleteCase(name),
                Some((name, SnapshotVerb.Delete))));

}
```

## [02]-[PARTICIPANT_SEAMS]

`ParticipantSpec` admits a non-empty, unique capability registry. Each capability row owns its lane contract, generic lane lookup derives the row from `TLane`, and the host adapter remains the only Rhino override implementation.

```csharp signature
namespace Rasm.Rhino.Persistence;

using LanguageExt;
using Rasm.Domain;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.SnapShots;
using Rhino.FileIO;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

[SmartEnum<string>]
public sealed partial class SnapshotCategory
{
    public static readonly SnapshotCategory Application = new("application", SnapShotsClient.ApplicationCategory);
    public static readonly SnapshotCategory Document = new("document", SnapShotsClient.DocumentCategory);
    public static readonly SnapshotCategory Rendering = new("rendering", SnapShotsClient.RenderingCategory);
    public static readonly SnapshotCategory Views = new("views", SnapShotsClient.ViewsCategory);
    public static readonly SnapshotCategory Objects = new("objects", SnapShotsClient.ObjectsCategory);
    public static readonly SnapshotCategory Layers = new("layers", SnapShotsClient.LayersCategory);
    public static readonly SnapshotCategory Lights = new("lights", SnapShotsClient.LightsCategory);
    internal Func<string> Native { get; }
}

[SmartEnum<string>]
public sealed partial class SnapshotCapability
{
    public static readonly SnapshotCapability Document = new("document", typeof(IDocumentSnapshotLane));
    public static readonly SnapshotCapability Objects = new("objects", typeof(IObjectSnapshotLane));
    public static readonly SnapshotCapability Animation = new("animation", typeof(IAnimationSnapshotLane));
    internal Type Contract { get; }
}

public sealed record SnapshotObjectState(Transform Transform, ArchiveMap Payload);

public interface ISnapshotLane
{
    SnapshotCapability Capability { get; }
}

public interface IDocumentSnapshotLane : ISnapshotLane
{
    Fin<ArchiveMap> Save(RhinoDoc document);
    Fin<Unit> Restore(RhinoDoc document, ArchiveMap payload);
    Fin<Unit> Restored(RhinoDoc document);
    Fin<bool> IsCurrent(RhinoDoc document, ArchiveMap current, Seq<ArchiveMap> snapshots, TextLog? log);
}

public interface IObjectSnapshotLane : ISnapshotLane
{
    Fin<bool> Supports(RhinoObject value);
    Fin<SnapshotObjectState> Save(RhinoDoc document, RhinoObject value, Transform transform);
    Fin<SnapshotObjectState> Restore(RhinoDoc document, RhinoObject value, Transform transform, ArchiveMap payload);
    Fin<SnapshotObjectState> TransformChanged(RhinoDoc document, RhinoObject value, Transform transform, ArchiveMap payload);
    Fin<bool> IsCurrent(RhinoDoc document, RhinoObject value, ArchiveMap current, Seq<ArchiveMap> snapshots, TextLog? log);
}

public interface IAnimationSnapshotLane : ISnapshotLane
{
    Fin<Unit> Start(RhinoDoc document, int frames);
    Fin<Unit> PrepareDocument(RhinoDoc document, ArchiveMap start, ArchiveMap stop);
    Fin<Unit> AnimateDocument(RhinoDoc document, double position, ArchiveMap start, ArchiveMap stop);
    Fin<Transform> PrepareObject(RhinoDoc document, RhinoObject value, Transform transform, ArchiveMap start, ArchiveMap stop);
    Fin<Transform> AnimateObject(RhinoDoc document, RhinoObject value, Transform transform, double position, ArchiveMap start, ArchiveMap stop);
    Fin<BoundingBox> ExtendDocument(RhinoDoc document, ArchiveMap start, ArchiveMap stop, BoundingBox bounds);
    Fin<BoundingBox> ExtendObject(
        RhinoDoc document,
        RhinoObject value,
        Transform transform,
        ArchiveMap start,
        ArchiveMap stop,
        BoundingBox bounds);
    Fin<Unit> Stop(RhinoDoc document);
}

public abstract class SnapshotCodec
{
    protected SnapshotCodec(ArchiveSchema schema) => Schema = schema;
    public ArchiveSchema Schema { get; }
    protected abstract Fin<ArchiveMap> Upgrade(ArchiveEnvelope envelope);

    internal Fin<Unit> Write(BinaryArchiveWriter archive, ArchiveMap payload, Op op) =>
        ArchiveIo.Cross(new ArchiveExchange.WriteCase(archive, Schema, payload), op)
            .Map(static _ => unit);

    internal Fin<ArchiveMap> Read(BinaryArchiveReader archive, Op op) =>
        ArchiveIo.Cross(new ArchiveExchange.ReadCase(archive, Schema), op)
            .Bind(result => result.Switch<Fin<ArchiveMap>>(
                writtenCase: _ => Fail<ArchiveMap>(op.InvalidResult(detail: "Snapshot archive read returned a write receipt.")),
                readCase: read => op.Catch(() => Upgrade(read.Envelope))));
}

public sealed class ParticipantSpec
{
    private ParticipantSpec(
        Guid plugInId,
        Guid clientId,
        SnapshotCategory category,
        string name,
        SnapshotCodec codec,
        HashMap<SnapshotCapability, ISnapshotLane> lanes,
        Action<Error> report) =>
        (PlugInId, ClientId, Category, Name, Codec, Lanes, Report) =
        (plugInId, clientId, category, name, codec, lanes, report);

    public Guid PlugInId { get; }
    public Guid ClientId { get; }
    public SnapshotCategory Category { get; }
    public string Name { get; }
    public SnapshotCodec Codec { get; }
    internal HashMap<SnapshotCapability, ISnapshotLane> Lanes { get; }
    internal Action<Error> Report { get; }

    public static Fin<ParticipantSpec> Of(
        Guid plugInId,
        Guid clientId,
        SnapshotCategory category,
        string name,
        SnapshotCodec codec,
        Action<Error> report,
        params ReadOnlySpan<ISnapshotLane> lanes)
    {
        Op op = Op.Of();
        ISnapshotLane[] roster = lanes.ToArray();
        return from _ids in guard(plugInId != Guid.Empty && clientId != Guid.Empty, op.InvalidInput()).ToFin()
               from label in op.AcceptText(value: name)
               from group in Optional(category).ToFin(Fail: op.InvalidInput())
               from format in Optional(codec).ToFin(Fail: op.InvalidInput())
               from reject in Optional(report).ToFin(Fail: op.InvalidInput())
               from admitted in roster
                   .Map(lane => ValidateLane(lane, op))
                   .Traverse(static value => value)
               from _nonempty in guard(!admitted.IsEmpty, op.InvalidInput()).ToFin()
               from indexed in admitted.Fold(
                   Succ(HashMap<SnapshotCapability, ISnapshotLane>()),
                   (Fin<HashMap<SnapshotCapability, ISnapshotLane>> state, ISnapshotLane lane) => state.Bind(map => map.ContainsKey(lane.Capability)
                       ? Fail<HashMap<SnapshotCapability, ISnapshotLane>>(op.InvalidResult(detail: $"Duplicate snapshot lane '{lane.Capability.Key}'."))
                       : Succ(map.Add(lane.Capability, lane))))
               select new ParticipantSpec(plugInId, clientId, group, label, format, indexed, reject);
    }

    internal Fin<TLane> Lane<TLane>(Op op)
        where TLane : class, ISnapshotLane =>
        SnapshotCapability.Items
            .Find(static capability => capability.Contract == typeof(TLane))
            .Bind(Lanes.Find)
            .Bind(value => Optional(value as TLane))
            .ToFin(Fail: op.MissingContext());

    internal bool Carries<TLane>()
        where TLane : class, ISnapshotLane =>
        SnapshotCapability.Items
            .Find(static capability => capability.Contract == typeof(TLane))
            .Bind(Lanes.Find)
            .IsSome;

    private static Fin<ISnapshotLane> ValidateLane(ISnapshotLane lane, Op op) =>
        Optional(lane).ToFin(Fail: op.InvalidInput())
            .Bind(value => Optional(value.Capability).ToFin(Fail: op.InvalidInput())
                .Bind(capability => capability.Contract.IsInstanceOfType(value)
                    ? Succ(value)
                    : Fail<ISnapshotLane>(op.InvalidResult(detail: "Snapshot lane interface and capability disagree."))));
}
```

## [03]-[HOST_ADAPTER]

Every override mints its own `Op.Of()`, invokes its lane through the kernel `Op.Catch` funnel, and sends every fault to `ParticipantSpec.Report` before collapsing to the host `bool`. `Supports*` probes read lane presence through `Carries<TLane>`; a lane absent at invocation time is a typed `MissingContext` fault, reported and collapsed to `false`. Successful registration consumes a client id permanently because Rhino exposes no removal member.

Rhino's `ref`, `bool`, and `void` override contracts form the platform-forced statement seam. Mutable out evidence remains local to the override capsule and crosses into lane code only as an admitted value.

```csharp signature
namespace Rasm.Rhino.Persistence;

using LanguageExt;
using Rasm.Domain;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.SnapShots;
using Rhino.FileIO;
using Rhino.Geometry;
using static LanguageExt.Prelude;

public sealed class SnapshotParticipant : SnapShotsClient
{
    private static readonly Atom<HashMap<Guid, Guid>> Registered = Atom(HashMap<Guid, Guid>());
    private readonly ParticipantSpec _spec;

    private SnapshotParticipant(ParticipantSpec spec) => _spec = spec;

    public static Fin<Unit> Enlist(ParticipantSpec spec, Op? key = null)
    {
        Op op = key.OrDefault();
        return Optional(spec).ToFin(Fail: op.InvalidInput()).Bind(admitted =>
        {
            Guid token = Guid.NewGuid();
            Registered.Swap(state => state.Find(admitted.ClientId).IsSome
                ? state
                : state.Add(admitted.ClientId, token));
            return Registered.Value.Find(admitted.ClientId).Exists(value => value == token)
                ? op.Catch(() => op.Confirm(success: RegisterSnapShotClient(new SnapshotParticipant(admitted))))
                    .BindFail(error =>
                    {
                        Registered.Swap(state => state.Find(admitted.ClientId).Exists(value => value == token)
                            ? state.Remove(admitted.ClientId)
                            : state);
                        return Fin.Fail<Unit>(error: error);
                    })
                : Fin.Fail<Unit>(error: op.InvalidResult(
                    detail: $"Snapshot participant '{admitted.ClientId}' is already resident."));
        });
    }

    public override Guid PlugInId() => _spec.PlugInId;
    public override Guid ClientId() => _spec.ClientId;
    public override string Category() => _spec.Category.Native();
    public override string Name() => _spec.Name;

    public override bool SupportsDocument() => _spec.Carries<IDocumentSnapshotLane>();
    public override bool SupportsObjects() => _spec.Carries<IObjectSnapshotLane>();
    public override bool SupportsAnimation() => _spec.Carries<IAnimationSnapshotLane>();

    public override bool SaveDocument(RhinoDoc doc, BinaryArchiveWriter archive)
    {
        Op op = Op.Of();
        return Landed(op.Catch(() => _spec.Lane<IDocumentSnapshotLane>(op)
            .Bind(lane => lane.Save(doc))
            .Bind(payload => _spec.Codec.Write(archive, payload, op))));
    }

    public override bool RestoreDocument(RhinoDoc doc, BinaryArchiveReader archive)
    {
        Op op = Op.Of();
        return Landed(op.Catch(() => _spec.Codec.Read(archive, op)
            .Bind(payload => _spec.Lane<IDocumentSnapshotLane>(op).Bind(lane => lane.Restore(doc, payload)))));
    }

    public override void SnapshotRestored(RhinoDoc doc)
    {
        Op op = Op.Of();
        _ = Landed(op.Catch(() => _spec.Lane<IDocumentSnapshotLane>(op).Bind(lane => lane.Restored(doc))));
    }

    public override bool SupportsObject(RhinoObject rhObject)
    {
        Op op = Op.Of();
        return Landed(op.Catch(() => _spec.Lane<IObjectSnapshotLane>(op).Bind(lane => lane.Supports(rhObject))));
    }

    public override bool SaveObject(
        RhinoDoc doc,
        RhinoObject rhObject,
        ref Transform transform,
        BinaryArchiveWriter archive)
    {
        Op op = Op.Of();
        Transform incoming = transform;
        return ObjectState(
            op,
            () => _spec.Lane<IObjectSnapshotLane>(op).Bind(lane => lane.Save(doc, rhObject, incoming)),
            archive,
            ref transform);
    }

    public override bool RestoreObject(
        RhinoDoc doc,
        RhinoObject rhObject,
        ref Transform transform,
        BinaryArchiveReader archive)
    {
        Op op = Op.Of();
        Transform incoming = transform;
        return ObjectState(
            op,
            () => from lane in _spec.Lane<IObjectSnapshotLane>(op)
                  from payload in _spec.Codec.Read(archive, op)
                  from state in lane.Restore(doc, rhObject, incoming, payload)
                  select state,
            writer: null,
            ref transform);
    }

    public override bool ObjectTransformNotification(
        RhinoDoc doc,
        RhinoObject rhObject,
        ref Transform transform,
        BinaryArchiveReader archive)
    {
        Op op = Op.Of();
        Transform incoming = transform;
        return ObjectState(
            op,
            () => from lane in _spec.Lane<IObjectSnapshotLane>(op)
                  from payload in _spec.Codec.Read(archive, op)
                  from state in lane.TransformChanged(doc, rhObject, incoming, payload)
                  select state,
            writer: null,
            ref transform);
    }

    public override void AnimationStart(RhinoDoc doc, int frames)
    {
        Op op = Op.Of();
        _ = Landed(op.Catch(() => _spec.Lane<IAnimationSnapshotLane>(op).Bind(lane => lane.Start(doc, frames))));
    }

    public override bool PrepareForDocumentAnimation(
        RhinoDoc doc,
        BinaryArchiveReader start,
        BinaryArchiveReader stop)
    {
        Op op = Op.Of();
        return Landed(op.Catch(() => AnimationMaps(start, stop, op).Bind(maps =>
            _spec.Lane<IAnimationSnapshotLane>(op).Bind(lane => lane.PrepareDocument(doc, maps.Start, maps.Stop)))));
    }

    public override bool AnimateDocument(
        RhinoDoc doc,
        double pos,
        BinaryArchiveReader start,
        BinaryArchiveReader stop)
    {
        Op op = Op.Of();
        return Landed(op.Catch(() => AnimationMaps(start, stop, op).Bind(maps =>
            _spec.Lane<IAnimationSnapshotLane>(op).Bind(lane => lane.AnimateDocument(doc, pos, maps.Start, maps.Stop)))));
    }

    public override bool PrepareForObjectAnimation(
        RhinoDoc doc,
        RhinoObject rhObject,
        ref Transform transform,
        BinaryArchiveReader start,
        BinaryArchiveReader stop) =>
        AnimationTransform(Op.Of(), doc, rhObject, transform, position: None, start, stop, ref transform);

    public override bool AnimateObject(
        RhinoDoc doc,
        RhinoObject rhObject,
        ref Transform transform,
        double pos,
        BinaryArchiveReader start,
        BinaryArchiveReader stop) =>
        AnimationTransform(Op.Of(), doc, rhObject, transform, Some(pos), start, stop, ref transform);

    public override bool AnimationStop(RhinoDoc doc)
    {
        Op op = Op.Of();
        return Landed(op.Catch(() => _spec.Lane<IAnimationSnapshotLane>(op).Bind(lane => lane.Stop(doc))));
    }

    public override void ExtendBoundingBoxForDocumentAnimation(
        RhinoDoc doc,
        BinaryArchiveReader start,
        BinaryArchiveReader stop,
        ref BoundingBox bbox)
    {
        Op op = Op.Of();
        BoundingBox incoming = bbox;
        bbox = Bound(
            op.Catch(() => AnimationMaps(start, stop, op).Bind(maps =>
                _spec.Lane<IAnimationSnapshotLane>(op).Bind(lane => lane.ExtendDocument(doc, maps.Start, maps.Stop, incoming)))),
            incoming,
            op);
    }

    public override void ExtendBoundingBoxForObjectAnimation(
        RhinoDoc doc,
        RhinoObject rhObject,
        ref Transform transform,
        BinaryArchiveReader start,
        BinaryArchiveReader stop,
        ref BoundingBox bbox)
    {
        Op op = Op.Of();
        Transform incomingTransform = transform;
        BoundingBox incomingBounds = bbox;
        bbox = Bound(
            op.Catch(() => AnimationMaps(start, stop, op).Bind(maps =>
                _spec.Lane<IAnimationSnapshotLane>(op).Bind(lane =>
                    lane.ExtendObject(doc, rhObject, incomingTransform, maps.Start, maps.Stop, incomingBounds)))),
            incomingBounds,
            op);
    }

    public override bool IsCurrentModelStateInAnySnapshot(
        RhinoDoc doc,
        BinaryArchiveReader archive,
        SimpleArrayBinaryArchiveReader archiveArray,
        TextLog? textLog = null)
    {
        Op op = Op.Of();
        return Landed(op.Catch(() => ProbeSet(archive, archiveArray, op).Bind(probe =>
            _spec.Lane<IDocumentSnapshotLane>(op).Bind(lane => lane.IsCurrent(doc, probe.Current, probe.Snapshots, textLog)))));
    }

    public override bool IsCurrentModelStateInAnySnapshot(
        RhinoDoc doc,
        RhinoObject rhObject,
        BinaryArchiveReader archive,
        SimpleArrayBinaryArchiveReader archiveArray,
        TextLog? textLog = null)
    {
        Op op = Op.Of();
        return Landed(op.Catch(() => ProbeSet(archive, archiveArray, op).Bind(probe =>
            _spec.Lane<IObjectSnapshotLane>(op).Bind(lane => lane.IsCurrent(doc, rhObject, probe.Current, probe.Snapshots, textLog)))));
    }

    private bool AnimationTransform(
        Op op,
        RhinoDoc document,
        RhinoObject value,
        Transform incoming,
        Option<double> position,
        BinaryArchiveReader start,
        BinaryArchiveReader stop,
        ref Transform transform)
    {
        Fin<Transform> outcome = op.Catch(() =>
            from maps in AnimationMaps(start, stop, op)
            from lane in _spec.Lane<IAnimationSnapshotLane>(op)
            from current in position.Match(
                Some: location => lane.AnimateObject(document, value, incoming, location, maps.Start, maps.Stop),
                None: () => lane.PrepareObject(document, value, incoming, maps.Start, maps.Stop))
            from admitted in op.AcceptValue(current)
            select admitted);
        Transform? accepted = null;
        bool succeeded = outcome.Match(
            Succ: current =>
            {
                accepted = current;
                return true;
            },
            Fail: Fault);
        if (accepted is Transform current)
        {
            transform = current;
        }

        return succeeded;
    }

    private bool ObjectState(
        Op op,
        Func<Fin<SnapshotObjectState>> use,
        BinaryArchiveWriter? writer,
        ref Transform transform)
    {
        SnapshotObjectState? accepted = null;
        bool succeeded = op.Catch(() => use()
                .Bind(state => op.AcceptValue(state.Transform).Map(_ => state))
                .Bind(state => writer is null
                    ? Succ(state)
                    : _spec.Codec.Write(writer, state.Payload, op).Map(_ => state)))
            .Match(
                Succ: state =>
                {
                    accepted = state;
                    return true;
                },
                Fail: Fault);
        if (accepted is not null)
        {
            transform = accepted.Transform;
        }

        return succeeded;
    }

    private Fin<(ArchiveMap Start, ArchiveMap Stop)> AnimationMaps(
        BinaryArchiveReader start,
        BinaryArchiveReader stop,
        Op op) =>
        from first in _spec.Codec.Read(start, op)
        from last in _spec.Codec.Read(stop, op)
        select (first, last);

    private Fin<(ArchiveMap Current, Seq<ArchiveMap> Snapshots)> ProbeSet(
        BinaryArchiveReader archive,
        SimpleArrayBinaryArchiveReader archiveArray,
        Op op) =>
        from current in _spec.Codec.Read(archive, op)
        from snapshots in Enumerable.Range(0, archiveArray.Count)
            .Map(index => _spec.Codec.Read(archiveArray.Get(index), op))
            .Traverse(static value => value)
        select (current, snapshots);

    private bool Landed(Fin<Unit> outcome) => outcome.Match(Succ: static _ => true, Fail: Fault);
    private bool Landed(Fin<bool> outcome) => outcome.Match(Succ: static value => value, Fail: Fault);

    private BoundingBox Bound(Fin<BoundingBox> outcome, BoundingBox fallback, Op op) =>
        outcome.Bind(value => op.AcceptValue(value)).Match(Succ: static value => value, Fail: error =>
        {
            _spec.Report(error);
            return fallback;
        });

    private bool Fault(Error error)
    {
        _spec.Report(error);
        return false;
    }
}
```

## [04]-[LIFECYCLE]

Scripted operations follow roster capture → precondition → serial-pinned script → roster postcondition. Scoped restore uses a GUID sentinel; one catch frame admits the body before `Sealed` runs restore and delete finalizers on every outcome, appending each finalizer failure onto the primary fault.

Participant archive flow follows `ArchiveIo.Cross` → schema admission → codec upgrade → lane. Lane-produced transforms and bounds cross the shared result-admission oracle before any host `ref` assignment. Registration releases a rejected local reservation and retains every accepted client id for the process lifetime.

## [05]-[SEAMS]

`SnapshotCodec` and `TypedUserData<TSelf>` consume the same `ArchiveExchange`/`ArchiveEnvelope` contract. `DocumentStream` owns worksession change observation, and the Document session owner carries every worksession read and transition through `WorksessionSnapshot` and `WorksessionOp`.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
