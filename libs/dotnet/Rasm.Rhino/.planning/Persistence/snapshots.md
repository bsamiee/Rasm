# [RASM_RHINO_PERSISTENCE_SNAPSHOTS]

Rhino publishes snapshot NAMES and nothing else, so capture, restore, and delete ride a serial-pinned command script proved on both sides, while a plug-in's own snapshot payload rides the `SnapShotsClient` seam. `Snapshots.Commit` closes the scripted roster rail; `ParticipantSpec` declares which of the three payload lanes a participant serves; `SnapshotParticipant` is the sole host adapter and settles every `ref`-contract override through the kernel's one slot receiver.

## [01]-[INDEX]

- [02]-[SCRIPTED_TABLE]: `SnapshotName`, `SnapshotPresence`, `SnapshotVerb`, `SnapshotOperation`, `SnapshotRoster`, `SnapshotReceipt`, `SnapshotAnswer`, and the `Snapshots` rail with its scoped-restore bracket.
- [03]-[PARTICIPANT_SEAMS]: `SnapshotCategory`, `ParticipantName`, `SnapshotObjectState`, the three lane contracts, and `ParticipantSpec`.
- [04]-[HOST_ADAPTER]: `SnapshotParticipant` — the twenty-four `SnapShotsClient` overrides, the reporting funnel, and the one-time registration claim.
- [05]-[RESEARCH]

## [02]-[SCRIPTED_TABLE]

- Owner: `SnapshotName` admits a name safe to embed in a quoted script token; `SnapshotPresence` is the roster-presence vocabulary and `SnapshotVerb` the three scripted verbs, each carrying its script token and its presence set; `SnapshotOperation` is the request family; `SnapshotRoster` and `SnapshotReceipt` are the detached evidence; `Snapshots` is the rail.
- Entry: `SnapshotOperation.Roster()`, `.Capture(name)`, `.Restore(name)`, `.Delete(name)` mint the request; `Snapshots.Commit(DocumentSession, SnapshotOperation, Op?)` runs it; `Snapshots.Within(DocumentSession, SnapshotName, body, Op?)` restores a target for the length of a body and puts the document back.
- Auto: the three mutation cases are ONE case carrying its verb, because the verb row already discriminates them — the script token, the presence law, and both roster guards read off `SnapshotVerb`, so a fourth scripted verb is one row and the request family does not grow. The receipt is a CENSUS, not a fact stream: this rail opens no `DocumentCommit.Sealed` and stamps no undo serial, so the before/after roster pair is the evidence and a slot vocabulary here names a timing class the page has none of.
- Auto: the presence law is stated as its ILLEGAL corner, not as its legal roster. Three of the four corners are legal — capture leaves the name present, restore requires and leaves it, delete requires it and removes it — and the fourth, requiring the name absent and leaving it absent, is a verb that moves no roster at all. `Forbidden` bars that one corner, `SnapshotOperation.Of` admits every request through it, and the two guards in `Run` read the same set — so a fourth verb declaring the barred corner refuses at its first use instead of scripting a no-op. Enumerating three legal rows to exclude one inverts the author's intent and scales as `2^n - k`.
- Law: the host publishes `SnapshotTable.Names` and no capture, restore, or delete member at any access level, so `RhinoApp.RunScript` against `_-Snapshot` is the only managed route a snapshot transition has. The carve pays for itself on the same rail: `SnapshotVerb` owns the token so no call site composes command text, `SnapshotName` refuses a quote or a newline so the composed line cannot be broken out of, and each run proves roster membership BEFORE and AFTER, so a silently-failed script is a typed refusal rather than an unnoticed no-op.
- Law: the scripted rail records NO undo. Rhino's snapshot commands manage their own document state and the script is run against a pinned runtime serial, so the leg appends `SessionNeed.Interrupt` to the one mutation derivation rather than replacing it, and no `UndoBracket` opens over a transition the host's undo stack does not carry.
- Law: the scoped restore's finalizers ALL run and their faults aggregate onto the primary in order. `Lease<T>` cannot carry this: its constraint is `T : class, IDisposable` and its release is a `void Dispose()`, while both finalizers here are fallible host script runs whose refusals must reach the caller. The fold is therefore local, and it is one expression over the `Error` monoid rather than a `Match` ladder per step.
- Growth: a new scripted verb is one `SnapshotVerb` row with its token and presence set; a new presence axis is one `SnapshotPresence` row and the corner the law bars.
- Boundary: `DocumentStream` owns worksession change observation, and the Document session owner carries every worksession read and transition. This page owns snapshots alone.
- Packages: RhinoCommon (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-document-state.md` — `RhinoDoc.Snapshots`, `SnapshotTable.Names`, `SnapshotTable.Document`; `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-commands.md` — `RhinoApp.RunScript(uint documentSerialNumber, string script, bool echo)`); `Document/session` (`DocumentSession.Demand`, `SessionNeed`, `UndoCustody`, `IDetachedDocumentResult`); `Document/commit` (`RedrawPolicy`); kernel `Domain/validation` (`ICapability`, `CapabilitySet`, `CapabilityLaw`); Thinktecture.Runtime.Extensions; LanguageExt.Core.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Globalization;
using Generator.Equals;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Thinktecture;

namespace Rasm.Rhino.Persistence;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public readonly partial struct SnapshotName : IDisallowDefaultValue {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        Op op = Op.Of();
        value = value?.Trim() ?? string.Empty;
        string candidate = value;
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
                (candidate.Length == 0, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(SnapshotName) }))),
                (candidate.IndexOfAny(['\r', '\n', '"']) >= 0,
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(SnapshotName), "a name carrying no quote or line break" })))));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SnapshotPresence : ICapability<SnapshotPresence> {
    public static readonly SnapshotPresence Before = new(key: "before");
    public static readonly SnapshotPresence After = new(key: "after");

    public static CapabilityLaw<SnapshotPresence> Law =>
        CapabilityLaw<SnapshotPresence>.Forbidden(Seq(CapabilitySet<SnapshotPresence>.None));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SnapshotVerb {
    public static readonly SnapshotVerb Capture = new(
        key: "capture",
        presence: CapabilitySet<SnapshotPresence>.Of(SnapshotPresence.After),
        script: static name => Line(token: "_Save", name: name));
    public static readonly SnapshotVerb Restore = new(
        key: "restore",
        presence: CapabilitySet<SnapshotPresence>.Of(SnapshotPresence.Before, SnapshotPresence.After),
        script: static name => Line(token: "_Restore", name: name));
    public static readonly SnapshotVerb Delete = new(
        key: "delete",
        presence: CapabilitySet<SnapshotPresence>.Of(SnapshotPresence.Before),
        script: static name => Line(token: "_Delete", name: name));

    internal CapabilitySet<SnapshotPresence> Presence { get; }

    [UseDelegateFromConstructor] internal partial string Script(SnapshotName name);

    internal bool Demands(SnapshotPresence axis) => Presence.Admits(capability: axis);

    private static string Line(string token, SnapshotName name) =>
        string.Create(CultureInfo.InvariantCulture, $"_-Snapshot {token} _Name \"{name.Value}\" _Enter");
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SnapshotOperation {
    private SnapshotOperation() { }

    internal sealed record RosterCase : SnapshotOperation;
    internal sealed record MutationCase(SnapshotName Name, SnapshotVerb Verb) : SnapshotOperation;

    public static SnapshotOperation Roster() => new RosterCase();

    public static Fin<SnapshotOperation> Capture(string name, Op? key = null) => Of(name: name, verb: SnapshotVerb.Capture, key: key);

    public static Fin<SnapshotOperation> Restore(string name, Op? key = null) => Of(name: name, verb: SnapshotVerb.Restore, key: key);

    public static Fin<SnapshotOperation> Delete(string name, Op? key = null) => Of(name: name, verb: SnapshotVerb.Delete, key: key);

    internal static Fin<SnapshotOperation> Of(SnapshotName name, SnapshotVerb verb, Op key) =>
        from _presence in SnapshotPresence.Law.Admit(held: verb.Presence)
        select (SnapshotOperation)new MutationCase(Name: name, Verb: verb);

    private static Fin<SnapshotOperation> Of(string name, SnapshotVerb verb, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<SnapshotName>(candidate: name).Bind(admitted => Of(name: admitted, verb: verb, key: op));
    }

    internal Seq<SessionNeed> Needs => Switch<Seq<SessionNeed>>(
        rosterCase:   static _ => Seq(SessionNeed.Read),
        mutationCase: static _ => SessionNeed
            .Mutation(custody: UndoCustody.Unrecorded, redraw: RedrawPolicy.None)
            .Add(SessionNeed.Interrupt));
}

// --- [MODELS] --------------------------------------------------------------------------
[Equatable]
public sealed partial record SnapshotRoster([property: OrderedEquality] Seq<SnapshotName> Names) {
    internal bool Holds(SnapshotName name) => Names.Contains(name);
}

[Equatable]
public sealed partial record SnapshotReceipt(
    SnapshotName Name,
    SnapshotVerb Verb,
    SnapshotRoster Before,
    SnapshotRoster After,
    DocKey Document);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SnapshotAnswer : IDetachedDocumentResult {
    private SnapshotAnswer() { }

    public sealed record RosterCase(SnapshotRoster Roster) : SnapshotAnswer;
    public sealed record MutationCase(SnapshotReceipt Receipt) : SnapshotAnswer;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Snapshots {
    public static Fin<SnapshotAnswer> Commit(DocumentSession session, SnapshotOperation operation, Op? key = null) {
        Op op = key.OrDefault();
        return from owner in op.Need(value: session)
               from request in op.Need(value: operation)
               from answer in owner.Demand(
                   use: document => request.Switch<(RhinoDoc Document, Op Op), Fin<SnapshotAnswer>>(
                       state: (document, op),
                       rosterCase: static (state, _) => Roster(document: state.Document, key: state.Op)
                           .Map<SnapshotAnswer>(static roster => new SnapshotAnswer.RosterCase(Roster: roster)),
                       mutationCase: static (state, mutation) => Run(
                               document: state.Document,
                               name: mutation.Name,
                               verb: mutation.Verb,
                               key: state.Op)
                           .Map<SnapshotAnswer>(static receipt => new SnapshotAnswer.MutationCase(Receipt: receipt))),
                   key: op,
                   needs: request.Needs.ToArray())
               select answer;
    }

    public static Fin<T> Within<T>(DocumentSession session, SnapshotName target, Func<Fin<T>> use, Op? key = null) {
        Op op = key.OrDefault();
        return from body in op.Need(value: use)
               from sentinel in op.AcceptValidated<SnapshotName>(candidate: $"rasm-{Guid.NewGuid():N}")
               from _captured in Scripted(session: session, name: sentinel, verb: SnapshotVerb.Capture, key: op)
               from outcome in Settled(
                   body: op.Catch(() =>
                       from _restored in Scripted(session: session, name: target, verb: SnapshotVerb.Restore, key: op)
                       from value in body()
                       select value),
                   finalizers: Seq(
                       () => Scripted(session: session, name: sentinel, verb: SnapshotVerb.Restore, key: op),
                       () => Scripted(session: session, name: sentinel, verb: SnapshotVerb.Delete, key: op)))
               select outcome;
    }

    private static Fin<Unit> Scripted(DocumentSession session, SnapshotName name, SnapshotVerb verb, Op key) =>
        from operation in SnapshotOperation.Of(name: name, verb: verb, key: key)
        from _answer in Commit(session: session, operation: operation, key: key)
        select unit;

    private static Fin<T> Settled<T>(Fin<T> body, Seq<Func<Fin<Unit>>> finalizers) =>
        finalizers.Fold(body, static (state, final) => state.Match(
            Succ: value => final().Map(_ => value),
            Fail: primary => final().Match(
                Succ: _ => Fin.Fail<T>(error: primary),
                Fail: secondary => Fin.Fail<T>(error: primary + secondary))));

    private static Fin<SnapshotReceipt> Run(RhinoDoc document, SnapshotName name, SnapshotVerb verb, Op key) =>
        from before in Roster(document: document, key: key)
        from _precondition in Proved(
            held: before.Holds(name: name),
            demanded: verb.Demands(axis: SnapshotPresence.Before),
            name: name,
            verb: verb,
            stage: "precondition",
            key: key)
        from _run in key.Catch(() => key.Confirm(success: RhinoApp.RunScript(
            document.RuntimeSerialNumber,
            verb.Script(name: name),
            echo: false)))
        from after in Roster(document: document, key: key)
        from _postcondition in Proved(
            held: after.Holds(name: name),
            demanded: verb.Demands(axis: SnapshotPresence.After),
            name: name,
            verb: verb,
            stage: "postcondition",
            key: key)
        from owner in key.AcceptValidated<DocKey>(candidate: document.RuntimeSerialNumber)
        select new SnapshotReceipt(Name: name, Verb: verb, Before: before, After: after, Document: owner);

    private static Fin<Unit> Proved(bool held, bool demanded, SnapshotName name, SnapshotVerb verb, string stage, Op key) =>
        held == demanded
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: new PersistenceFault.Diverged(
                Key: key,
                Subject: $"{verb.Key}:{stage}:{name.Value}",
                Expected: demanded ? "present" : "absent",
                Observed: held ? "present" : "absent"));

    private static Fin<SnapshotRoster> Roster(RhinoDoc document, Op key) =>
        key.Catch(() => toSeq(document.Snapshots.Names)
            .Traverse(name => key.AcceptValidated<SnapshotName>(candidate: name).ToValidation())
            .As()
            .ToFin())
            .Map(static values => new SnapshotRoster(
                Names: toSeq(values.OrderBy(static value => value.Value, StringComparer.Ordinal))));
}
```

## [03]-[PARTICIPANT_SEAMS]

- Owner: `SnapshotCategory` is the seven-row host category vocabulary; `ParticipantName` admits the displayed participant name; `SnapshotObjectState` is the per-object payload; `IDocumentSnapshotLane`, `IObjectSnapshotLane`, and `IAnimationSnapshotLane` are the three payload contracts; `ParticipantSpec` is the participant's whole declaration.
- Entry: `ParticipantSpec.Of(plugInId, clientId, category, name, codec, report, document, objects, animation, key)` admits identity, category, codec, and at least one lane; the three lane slots ride `Option<T>` on the spec.
- Auto: `SupportsDocument`, `SupportsObjects`, and `SupportsAnimation` DERIVE from the three slots, and every invocation reads the same slot the probe read. The retired form keyed a `HashMap<SnapshotCapability, ISnapshotLane>` by a three-row vocabulary whose only column was a `Type`, then answered both the probe and the invocation by searching that roster for a row whose contract matched `typeof(TLane)` and downcasting the value — a type-test dispatch disagreeing with itself under no compiler check. The slots type the answer.
- Law: the erasure family DELETES whole — `SnapshotCapability`, its `Type Contract` column, the `Lanes` map, the generic `Lane<TLane>`/`Carries<TLane>` lookups, and the `ValidateLane` interface-versus-capability agreement gate. The gate existed only because the map allowed a lane to be filed under a row its own type did not satisfy; a typed slot cannot hold the wrong lane, so the agreement is a compile fact and the roster's three rows had no upstream at all.
- Law: `SnapshotCodec` DELETES onto `Persistence/userdata#ARCHIVE_FRAME`'s `IArchiveCodec` (E-R57) — the retired class declared `Schema`, `Upgrade`, `Write`, and `Read` exactly as `TypedUserData<TSelf>` did, so a page-local abstract adding nothing over the seated one is a forwarding shell. `ParticipantSpec.Codec` is an `IArchiveCodec` and every crossing on this page reads it.
- Law: `ArchiveMap` is the only payload currency across a lane — no live `BinaryArchiveReader`, `BinaryArchiveWriter`, or `ArchivableDictionary` reaches lane code. The codec is the deliberate exception, because the codec IS the crossing: its `Write` and `Read` take the live archive by design and hand back an admitted map.
- Law: `SnapshotCategory.Native` is a DELEGATE column rather than a captured string, because `SnapShotsClient`'s category members are static properties the host initializes; a row capturing their values at type init freezes whatever the host published at assembly load.
- Growth: a new lane is one contract, one `Option` slot, one probe derivation, and its overrides; a new host category is one row.
- Boundary: the composition root is the sole producer of a `ParticipantSpec` and the sole caller of `SnapshotParticipant.Enlist`; lane implementations are foreign, which is why the three contracts are instance interfaces rather than a closed family.
- Packages: RhinoCommon (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-document-state.md` — `SnapShotsClient` and its seven static category members; `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-fileio.md` — `BinaryArchiveWriter`, `BinaryArchiveReader`, `SimpleArrayBinaryArchiveReader`, `TextLog`; `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-geometry.md` — `Transform`, `BoundingBox`; `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-objects.md` — `RhinoObject`); `Persistence/dictionary` (`ArchiveMap`), `Persistence/userdata` (`IArchiveCodec`); Thinktecture.Runtime.Extensions; LanguageExt.Core.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.SnapShots;
using Rhino.FileIO;
using Rhino.Geometry;
using Thinktecture;

namespace Rasm.Rhino.Persistence;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SnapshotCategory {
    public static readonly SnapshotCategory Application = new(key: "application", native: static () => SnapShotsClient.ApplicationCategory);
    public static readonly SnapshotCategory Document = new(key: "document", native: static () => SnapShotsClient.DocumentCategory);
    public static readonly SnapshotCategory Rendering = new(key: "rendering", native: static () => SnapShotsClient.RenderingCategory);
    public static readonly SnapshotCategory Views = new(key: "views", native: static () => SnapShotsClient.ViewsCategory);
    public static readonly SnapshotCategory Objects = new(key: "objects", native: static () => SnapShotsClient.ObjectsCategory);
    public static readonly SnapshotCategory Layers = new(key: "layers", native: static () => SnapShotsClient.LayersCategory);
    public static readonly SnapshotCategory Lights = new(key: "lights", native: static () => SnapShotsClient.LightsCategory);

    [UseDelegateFromConstructor] internal partial string Native();
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public readonly partial struct ParticipantName : IDisallowDefaultValue {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length == 0
            ? new ValidationError(string.Join(" | ", new object?[] { Op.Of(), nameof(ParticipantName) }))
            : null;
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
public interface IDocumentSnapshotLane {
    Fin<ArchiveMap> Save(RhinoDoc document);
    Fin<Unit> Restore(RhinoDoc document, ArchiveMap payload);
    Fin<Unit> Restored(RhinoDoc document);
    Fin<bool> IsCurrent(RhinoDoc document, ArchiveMap current, Seq<ArchiveMap> snapshots, Option<TextLog> log);
}

public interface IObjectSnapshotLane {
    Fin<bool> Supports(RhinoObject value);
    Fin<SnapshotObjectState> Save(RhinoDoc document, RhinoObject value, Transform transform);
    Fin<SnapshotObjectState> Restore(RhinoDoc document, RhinoObject value, Transform transform, ArchiveMap payload);
    Fin<SnapshotObjectState> TransformChanged(RhinoDoc document, RhinoObject value, Transform transform, ArchiveMap payload);
    Fin<bool> IsCurrent(RhinoDoc document, RhinoObject value, ArchiveMap current, Seq<ArchiveMap> snapshots, Option<TextLog> log);
}

public interface IAnimationSnapshotLane {
    Fin<Unit> Start(RhinoDoc document, int frames);
    Fin<Unit> PrepareDocument(RhinoDoc document, ArchiveMap start, ArchiveMap stop);
    Fin<Unit> AnimateDocument(RhinoDoc document, double position, ArchiveMap start, ArchiveMap stop);
    Fin<Transform> PrepareObject(RhinoDoc document, RhinoObject value, Transform transform, ArchiveMap start, ArchiveMap stop);
    Fin<Transform> AnimateObject(RhinoDoc document, RhinoObject value, Transform transform, double position, ArchiveMap start, ArchiveMap stop);
    Fin<BoundingBox> ExtendDocument(RhinoDoc document, ArchiveMap start, ArchiveMap stop, BoundingBox bounds);
    Fin<BoundingBox> ExtendObject(RhinoDoc document, RhinoObject value, Transform transform, ArchiveMap start, ArchiveMap stop, BoundingBox bounds);
    Fin<Unit> Stop(RhinoDoc document);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SnapshotObjectState(Transform Transform, ArchiveMap Payload);

public sealed class ParticipantSpec {
    private ParticipantSpec(
        Guid plugInId,
        Guid clientId,
        SnapshotCategory category,
        ParticipantName name,
        IArchiveCodec codec,
        Action<Error> report,
        Option<IDocumentSnapshotLane> document,
        Option<IObjectSnapshotLane> objects,
        Option<IAnimationSnapshotLane> animation) =>
        (PlugInId, ClientId, Category, Name, Codec, Report, Document, Objects, Animation) =
        (plugInId, clientId, category, name, codec, report, document, objects, animation);

    public Guid PlugInId { get; }

    public Guid ClientId { get; }

    public SnapshotCategory Category { get; }

    public ParticipantName Name { get; }

    public IArchiveCodec Codec { get; }

    internal Action<Error> Report { get; }

    internal Option<IDocumentSnapshotLane> Document { get; }

    internal Option<IObjectSnapshotLane> Objects { get; }

    internal Option<IAnimationSnapshotLane> Animation { get; }

    public static Fin<ParticipantSpec> Of(
        Guid plugInId,
        Guid clientId,
        SnapshotCategory category,
        string name,
        IArchiveCodec codec,
        Action<Error> report,
        Option<IDocumentSnapshotLane> document = default,
        Option<IObjectSnapshotLane> objects = default,
        Option<IAnimationSnapshotLane> animation = default,
        Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in (
                   guard(plugInId != Guid.Empty,
                       (Error)new KernelFault.InvalidValue(nameof(plugInId), string.Join(" | ", new object?[] { op, "a non-empty plug-in identity" }))).ToFin().ToValidation(),
                   guard(clientId != Guid.Empty,
                       (Error)new KernelFault.InvalidValue(nameof(clientId), string.Join(" | ", new object?[] { op, "a non-empty client identity" }))).ToFin().ToValidation(),
                   op.AcceptValidated<ParticipantName>(candidate: name).ToValidation(),
                   op.Need(value: category).ToValidation(),
                   op.Need(value: codec).ToValidation(),
                   op.Need(value: report).ToValidation())
                   .Apply(static (_plugIn, _client, label, group, format, reject) => (
                       Name: label, Category: group, Codec: format, Report: reject))
                   .As()
                   .ToFin()
               from _lanes in guard(
                   document.IsSome || objects.IsSome || animation.IsSome,
                   (Error)new KernelFault.InvalidValue("SnapshotLane", string.Join(" | ", new object?[] { op, "at least one snapshot lane" }))).ToFin()
               select new ParticipantSpec(
                   plugInId: plugInId,
                   clientId: clientId,
                   category: admitted.Category,
                   name: admitted.Name,
                   codec: admitted.Codec,
                   report: admitted.Report,
                   document: document,
                   objects: objects,
                   animation: animation);
    }
}
```

## [04]-[HOST_ADAPTER]

- Owner: `SnapshotParticipant` — the sole `SnapShotsClient` subclass, covering all twenty-four overrides the host declares.
- Entry: `SnapshotParticipant.Enlist(ParticipantSpec, Op?)` claims the client id and registers the native pointer; the class has no other public surface, because every other member is a host callback.
- Auto: every override reads its outcome through ONE reporting funnel and settles it through ONE receiver. `Reported` taps the fault onto `ParticipantSpec.Report` and passes the rail through unchanged; `Landed` collapses a rail to the host `bool`; `Op.Settle(ref slot, outcome)` writes a `ref` contract's slot on success, leaves it untouched on refusal, and answers the same `bool` the override owes. The retired form spelled four collapse members — two `Landed` arities, a `Bound` with an explicit fallback argument, and a `Fault` — beside two nullable local capsules and a `succeeded` flag per `ref`-writing body.
- Auto: the bounding-box fallback DELETES. `Op.Settle` leaves the slot at its incoming value on refusal, which IS the fallback the retired `Bound` passed in beside the rail, so a second authority for "what the box holds when the lane refuses" no longer exists.
- Law: a `ref` host-contract override cannot capture its slot in a lambda (CS1628), so each body copies the incoming value to a local, runs the rail over that local, and settles through the kernel receiver. The copy is the platform-forced statement seam and it appears once per override with no variation.
- Law: `Supports*` reads lane presence off the spec's typed slot, and every invocation resolves that slot; an absent lane becomes a kernel validation refusal before the host scalar collapse.
- Law: registration is a one-shot CLAIM on the client id through the kernel transition owner: `Committed` means this call seated it and `Ceded` means another already had, so no token comparison exists to get wrong and no re-read of the cell is needed to tell an accepted seat from a rejected one. A failed native registration steps the claim back out.
- Law: successful registration consumes a client id for the process lifetime because Rhino exposes no removal member on either list. The base constructor adds the instance to the MANAGED `SnapShotsClientsList` callback roster while `RegisterSnapShotClient` registers the NATIVE pointer with the RDK — two distinct lists, so the explicit call is required and is not a double-add.
- Boundary: Rhino's `ref`, `bool`, and `void` override contracts form the platform-forced statement seam. Mutable evidence stays local to the override capsule and crosses into lane code only as an admitted value.
- Packages: RhinoCommon (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-document-state.md` — `SnapShotsClient` and its twenty-four virtual members, `RegisterSnapShotClient`; `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-fileio.md` — `BinaryArchiveWriter`, `BinaryArchiveReader`, `SimpleArrayBinaryArchiveReader`, `TextLog`); kernel `Domain/rails` (`Op.Catch`, `Op.Settle`, `Op.AcceptValue`, `Transition`, `Cell.Claim`, `Cell.Step`); `Persistence/dictionary` (`ArchiveMap`), `Persistence/userdata` (`IArchiveCodec`); LanguageExt.Core (`Atom`, `HashMap`, `Fin`).

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.SnapShots;
using Rhino.FileIO;
using Rhino.Geometry;

namespace Rasm.Rhino.Persistence;

// --- [BOUNDARIES] ----------------------------------------------------------------------
public sealed class SnapshotParticipant : SnapShotsClient {
    private static readonly Atom<HashMap<Guid, SnapshotCategory>> Registered = Atom(HashMap<Guid, SnapshotCategory>());
    private readonly ParticipantSpec spec;

    private SnapshotParticipant(ParticipantSpec spec) => this.spec = spec;

    public static Fin<Unit> Enlist(ParticipantSpec spec, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in op.Need(value: spec)
               from _claimed in Cell.Claim(cell: Registered, key: admitted.ClientId, mint: () => admitted.Category)
                   is Transition<HashMap<Guid, SnapshotCategory>>.Committed
                   ? Fin.Succ(value: unit)
                   : Fin.Fail<Unit>(error: new PersistenceFault.Resident(Key: op, Subject: admitted.ClientId.ToString()))
               from _resident in op.Catch(() => op.Confirm(success: RegisterSnapShotClient(new SnapshotParticipant(spec: admitted))))
                   .BindFail(primary => {
                       Transition<HashMap<Guid, SnapshotCategory>> released = Cell.Step(
                           cell: Registered,
                           step: state => state.Find(admitted.ClientId).Map(_ => state.Remove(admitted.ClientId)),
                           declined: op.InvalidContext());
                       return released switch {
                           Transition<HashMap<Guid, SnapshotCategory>>.Committed => Fin.Fail<Unit>(error: primary),
                           Transition<HashMap<Guid, SnapshotCategory>>.Refused row => Fin.Fail<Unit>(error: primary + row.Cause),
                           _ => Fin.Fail<Unit>(error: primary + op.InvalidResult()),
                       };
                   })
               select unit;
    }

    public override Guid PlugInId() => spec.PlugInId;

    public override Guid ClientId() => spec.ClientId;

    public override string Category() => spec.Category.Native();

    public override string Name() => spec.Name.Value;

    public override bool SupportsDocument() => spec.Document.IsSome;

    public override bool SupportsObjects() => spec.Objects.IsSome;

    public override bool SupportsAnimation() => spec.Animation.IsSome;

    public override bool SaveDocument(RhinoDoc doc, BinaryArchiveWriter archive) {
        Op op = Op.Of();
        return Landed(op.Catch(() => Document(op).Bind(lane => lane.Save(doc))
            .Bind(payload => spec.Codec.Write(archive, payload, op))));
    }

    public override bool RestoreDocument(RhinoDoc doc, BinaryArchiveReader archive) {
        Op op = Op.Of();
        return Landed(op.Catch(() => spec.Codec.Read(archive, op)
            .Bind(payload => Document(op).Bind(lane => lane.Restore(doc, payload)))));
    }

    public override void SnapshotRestored(RhinoDoc doc) {
        Op op = Op.Of();
        _ = Landed(op.Catch(() => Document(op).Bind(lane => lane.Restored(doc))));
    }

    public override bool SupportsObject(RhinoObject rhObject) {
        Op op = Op.Of();
        return Landed(op.Catch(() => Objects(op).Bind(lane => lane.Supports(rhObject))));
    }

    public override bool SaveObject(RhinoDoc doc, RhinoObject rhObject, ref Transform transform, BinaryArchiveWriter archive) {
        Op op = Op.Of();
        Transform incoming = transform;
        return Op.Settle(ref transform, Reported(op.Catch(() =>
            ObjectState(use: () => Objects(op).Bind(lane => lane.Save(doc, rhObject, incoming)), writer: archive, key: op))));
    }

    public override bool RestoreObject(RhinoDoc doc, RhinoObject rhObject, ref Transform transform, BinaryArchiveReader archive) {
        Op op = Op.Of();
        Transform incoming = transform;
        return Op.Settle(ref transform, Reported(op.Catch(() => ObjectState(
            use: () => from lane in Objects(op)
                       from payload in spec.Codec.Read(archive, op)
                       from state in lane.Restore(doc, rhObject, incoming, payload)
                       select state,
            writer: None,
            key: op))));
    }

    public override bool ObjectTransformNotification(RhinoDoc doc, RhinoObject rhObject, ref Transform transform, BinaryArchiveReader archive) {
        Op op = Op.Of();
        Transform incoming = transform;
        return Op.Settle(ref transform, Reported(op.Catch(() => ObjectState(
            use: () => from lane in Objects(op)
                       from payload in spec.Codec.Read(archive, op)
                       from state in lane.TransformChanged(doc, rhObject, incoming, payload)
                       select state,
            writer: None,
            key: op))));
    }

    public override void AnimationStart(RhinoDoc doc, int frames) {
        Op op = Op.Of();
        _ = Landed(op.Catch(() => Animation(op).Bind(lane => lane.Start(doc, frames))));
    }

    public override bool PrepareForDocumentAnimation(RhinoDoc doc, BinaryArchiveReader start, BinaryArchiveReader stop) {
        Op op = Op.Of();
        return Landed(op.Catch(() => Maps(start, stop, op).Bind(maps =>
            Animation(op).Bind(lane => lane.PrepareDocument(doc, maps.Start, maps.Stop)))));
    }

    public override bool AnimateDocument(RhinoDoc doc, double pos, BinaryArchiveReader start, BinaryArchiveReader stop) {
        Op op = Op.Of();
        return Landed(op.Catch(() => Maps(start, stop, op).Bind(maps =>
            Animation(op).Bind(lane => lane.AnimateDocument(doc, pos, maps.Start, maps.Stop)))));
    }

    public override bool PrepareForObjectAnimation(
        RhinoDoc doc,
        RhinoObject rhObject,
        ref Transform transform,
        BinaryArchiveReader start,
        BinaryArchiveReader stop) {
        Op op = Op.Of();
        Transform incoming = transform;
        return Op.Settle(ref transform, Reported(Motion(
            use: (lane, maps) => lane.PrepareObject(doc, rhObject, incoming, maps.Start, maps.Stop),
            start: start,
            stop: stop,
            key: op)));
    }

    public override bool AnimateObject(
        RhinoDoc doc,
        RhinoObject rhObject,
        ref Transform transform,
        double pos,
        BinaryArchiveReader start,
        BinaryArchiveReader stop) {
        Op op = Op.Of();
        Transform incoming = transform;
        return Op.Settle(ref transform, Reported(Motion(
            use: (lane, maps) => lane.AnimateObject(doc, rhObject, incoming, pos, maps.Start, maps.Stop),
            start: start,
            stop: stop,
            key: op)));
    }

    public override bool AnimationStop(RhinoDoc doc) {
        Op op = Op.Of();
        return Landed(op.Catch(() => Animation(op).Bind(lane => lane.Stop(doc))));
    }

    public override void ExtendBoundingBoxForDocumentAnimation(
        RhinoDoc doc,
        BinaryArchiveReader start,
        BinaryArchiveReader stop,
        ref BoundingBox bbox) {
        Op op = Op.Of();
        BoundingBox incoming = bbox;
        _ = Op.Settle(ref bbox, Reported(op.Catch(() => Maps(start, stop, op).Bind(maps =>
            Animation(op).Bind(lane => lane.ExtendDocument(doc, maps.Start, maps.Stop, incoming))
                .Bind(value => op.AcceptValue(value: value))))));
    }

    public override void ExtendBoundingBoxForObjectAnimation(
        RhinoDoc doc,
        RhinoObject rhObject,
        ref Transform transform,
        BinaryArchiveReader start,
        BinaryArchiveReader stop,
        ref BoundingBox bbox) {
        Op op = Op.Of();
        Transform incomingTransform = transform;
        BoundingBox incomingBounds = bbox;
        _ = Op.Settle(ref bbox, Reported(op.Catch(() => Maps(start, stop, op).Bind(maps =>
            Animation(op).Bind(lane => lane.ExtendObject(doc, rhObject, incomingTransform, maps.Start, maps.Stop, incomingBounds))
                .Bind(value => op.AcceptValue(value: value))))));
    }

    public override bool IsCurrentModelStateInAnySnapshot(
        RhinoDoc doc,
        BinaryArchiveReader archive,
        SimpleArrayBinaryArchiveReader archiveArray,
        TextLog? textLog = null) {
        Op op = Op.Of();
        return Landed(op.Catch(() => Probes(archive, archiveArray, op).Bind(probe =>
            Document(op).Bind(lane => lane.IsCurrent(doc, probe.Current, probe.Snapshots, Optional(textLog))))));
    }

    public override bool IsCurrentModelStateInAnySnapshot(
        RhinoDoc doc,
        RhinoObject rhObject,
        BinaryArchiveReader archive,
        SimpleArrayBinaryArchiveReader archiveArray,
        TextLog? textLog = null) {
        Op op = Op.Of();
        return Landed(op.Catch(() => Probes(archive, archiveArray, op).Bind(probe =>
            Objects(op).Bind(lane => lane.IsCurrent(doc, rhObject, probe.Current, probe.Snapshots, Optional(textLog))))));
    }

    // --- [DISPATCH]
    private Fin<IDocumentSnapshotLane> Document(Op key) => Lane(held: spec.Document, label: nameof(IDocumentSnapshotLane), key: key);

    private Fin<IObjectSnapshotLane> Objects(Op key) => Lane(held: spec.Objects, label: nameof(IObjectSnapshotLane), key: key);

    private Fin<IAnimationSnapshotLane> Animation(Op key) => Lane(held: spec.Animation, label: nameof(IAnimationSnapshotLane), key: key);

    private static Fin<TLane> Lane<TLane>(Option<TLane> held, string label, Op key)
        where TLane : class =>
        held.ToFin(Fail: new KernelFault.InvalidValue(label, string.Join(" | ", new object?[] { key, "a present snapshot value" })));

    private Fin<Transform> Motion(
        Func<IAnimationSnapshotLane, (ArchiveMap Start, ArchiveMap Stop), Fin<Transform>> use,
        BinaryArchiveReader start,
        BinaryArchiveReader stop,
        Op key) =>
        key.Catch(() =>
            from maps in Maps(start: start, stop: stop, key: key)
            from lane in Animation(key)
            from current in use(arg1: lane, arg2: maps)
            from admitted in key.AcceptValue(value: current)
            select admitted);

    private Fin<Transform> ObjectState(Func<Fin<SnapshotObjectState>> use, Option<BinaryArchiveWriter> writer, Op key) =>
        from state in use()
        from admitted in key.AcceptValue(value: state.Transform)
        from _written in writer.Match(
            Some: archive => spec.Codec.Write(archive, state.Payload, key),
            None: static () => Fin.Succ(value: unit))
        select admitted;

    private Fin<(ArchiveMap Start, ArchiveMap Stop)> Maps(BinaryArchiveReader start, BinaryArchiveReader stop, Op key) =>
        from first in spec.Codec.Read(start, key)
        from last in spec.Codec.Read(stop, key)
        select (first, last);

    private Fin<(ArchiveMap Current, Seq<ArchiveMap> Snapshots)> Probes(
        BinaryArchiveReader archive,
        SimpleArrayBinaryArchiveReader archiveArray,
        Op key) =>
        from current in spec.Codec.Read(archive, key)
        from snapshots in toSeq(Range(0, archiveArray.Count))
            .TraverseM(index => spec.Codec.Read(archiveArray.Get(index), key))
            .As()
        select (current, snapshots);

    private Fin<T> Reported<T>(Fin<T> outcome) =>
        outcome.BindFail(error => {
            spec.Report(error);
            return Fin.Fail<T>(error: error);
        });

    private bool Landed(Fin<bool> outcome) => Reported(outcome).IfFail(false);

    private bool Landed(Fin<Unit> outcome) => Landed(outcome.Map(static _ => true));
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
