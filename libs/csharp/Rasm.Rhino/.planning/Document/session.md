# [RASM_RHINO_SESSION]

`Rasm.Rhino.Document` owns document identity, lifecycle admission, capability-scoped access, document lifetime, and the live model/page regime. `DocumentSession` retains an owned headless lease only after every admission gate succeeds, serializes each consuming host call around handle re-resolution and fresh evidence, and re-reads the kernel `Context` so a regime change cannot leave a stale domain bundle.

## [01]-[INDEX]

- [02]-[IDENTITY_AND_STATE]: `DocKey`, `DocumentSet`, `SessionPhase`, and the detached `SessionSnapshot` lifecycle fact.
- [03]-[CAPABILITY]: `SessionMode` and the behavior-bearing `SessionNeed` capability vocabulary.
- [04]-[SOURCE_AND_SESSION]: `DocumentPath`, the flattened `SessionSource` admission family, and the lease-retaining `DocumentSession` owner.
- [05]-[REGIME]: `DocumentSpace`, the kernel-composed tolerance regime, and the one `RegimeChange` mutation rail.
- [06]-[REGIME_TEXT]: the locale-aware unit-text correspondence — `UnitPhrase` parse and `UnitScript` render over one regime.
- [07]-[SURFACE_LEDGER]: the page-owned surface registry.

## [02]-[IDENTITY_AND_STATE]

- Owner: `DocKey` `[ValueObject<uint>]` admits the positive `RuntimeSerialNumber`; `DocumentSet` carries the live-only versus live-and-headless census policy as a row column. `SessionPhase` folds the host lifecycle product into one position, and `SessionSnapshot` detaches every capability-relevant host fact before the handle leaves the serialized read window.
- Entry: `DocKey.Census` returns detached keys from the host iterator. Internal `DocKey.Resolve` re-enters the handle only inside the session owner, and internal `SessionSnapshot.Of` is the single read site for availability, transition, access, undo, headless, dirty, active-command, and public point-acquisition state.
- Law: lifecycle precedence is closing, opening, initializing, creating, ready, unavailable; the tuple switch covers the complete flag product and names no default arm.
- Law: `SessionSnapshot` is immutable evidence from one read. Every capability use re-resolves the retained key and obtains a new snapshot inside `DocumentSession.Demand` immediately before invoking its host body.
- Boundary: `InGetPoint` proves only a point acquisition; the broader acquisition reentrancy token belongs to the command acquisition algebra.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.IO;
using System.Threading;
using Rasm.Domain;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input;

namespace Rasm.Rhino.Document;

// --- [TYPES] ------------------------------------------------------------------------------
public interface IDetachedDocumentResult { }

[ValueObject<uint>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
public readonly partial struct DocKey : IDetachedDocumentResult {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref uint value) =>
        validationError = value is 0u ? new ValidationError(message: "Document serial is zero.") : null;

    internal Fin<RhinoDoc> Resolve(Op? key = null) {
        Op op = key.OrDefault();
        return op.Catch(() => Optional(RhinoDoc.FromRuntimeSerialNumber(serialNumber: this))
            .ToFin(Fail: op.MissingContext()));
    }

    public static Fin<DocKey> Of(RhinoDoc document, Op? key = null) {
        Op op = key.OrDefault();
        return op.Catch(() => Optional(document)
            .ToFin(Fail: op.MissingContext())
            .Bind(candidate => OpExtensions.AcceptValidated<uint, DocKey>(op: op, candidate: candidate.RuntimeSerialNumber)));
    }

    public static Fin<Seq<DocKey>> Census(DocumentSet scope, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in Optional(scope).ToFin(Fail: op.InvalidInput())
               from documents in op.Catch(() => Fin.Succ(
                   value: toSeq(RhinoDoc.OpenDocuments(includeHeadless: admitted.IncludeHeadless)).Strict()))
               from keys in documents.TraverseM(document => Of(document: document, key: op)).As()
               select keys.Strict();
    }
}

[SmartEnum<int>]
public sealed partial class DocumentSet {
    public static readonly DocumentSet Live = new(key: 0, includeHeadless: false);
    public static readonly DocumentSet All = new(key: 1, includeHeadless: true);

    public bool IncludeHeadless { get; }
}

[SmartEnum<int>]
public sealed partial class SessionPhase {
    public static readonly SessionPhase Ready = new(key: 0, open: true, transitional: false);
    public static readonly SessionPhase Opening = new(key: 1, open: false, transitional: true);
    public static readonly SessionPhase Closing = new(key: 2, open: false, transitional: true);
    public static readonly SessionPhase Initializing = new(key: 3, open: false, transitional: true);
    public static readonly SessionPhase Creating = new(key: 4, open: false, transitional: true);
    public static readonly SessionPhase Unavailable = new(key: 5, open: false, transitional: false);

    public bool Open { get; }
    public bool Transitional { get; }

    internal static SessionPhase Of(RhinoDoc document) =>
        (document.IsAvailable, document.IsClosing, document.IsOpening, document.IsInitializing, document.IsCreating) switch {
            (_, true, _, _, _) => Closing,
            (_, false, true, _, _) => Opening,
            (_, false, false, true, _) => Initializing,
            (_, false, false, false, true) => Creating,
            (true, false, false, false, false) => Ready,
            (false, false, false, false, false) => Unavailable,
        };
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record SessionSnapshot(
    SessionPhase Phase,
    bool ReadOnly,
    bool Locked,
    bool UndoRecording,
    bool Undoing,
    bool Redoing,
    bool Headless,
    bool Modified,
    bool Pointing,
    int CommandDepth,
    Option<Guid> ActiveCommand) : IDetachedDocumentResult {
    internal static Fin<SessionSnapshot> Of(RhinoDoc document, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(document).ToFin(Fail: op.MissingContext())
               from snapshot in op.Catch(() => Fin.Succ(value: new SessionSnapshot(
                   Phase: SessionPhase.Of(document: active),
                   ReadOnly: active.IsReadOnly,
                   Locked: active.IsLocked,
                   UndoRecording: active.UndoRecordingEnabled,
                   Undoing: active.UndoActive,
                   Redoing: active.RedoActive,
                   Headless: active.IsHeadless,
                   Modified: active.Modified,
                   Pointing: active.InGetPoint,
                   CommandDepth: active.InCommand(bIgnoreScriptRunnerCommands: false),
                   ActiveCommand: Optional(active.ActiveCommandId).Filter(static id => id != Guid.Empty))))
               select snapshot;
    }
}
```

## [03]-[CAPABILITY]

- Owner: `SessionMode` re-closes the foreign `RunMode` enum, adds the package-owned `Headless` lane, and carries independent live-host and native-dialog columns. `SessionNeed` is a keyless behavior vocabulary: each row separates mode admission from snapshot admission, so deterministic refusals run before acquisition and host state remains recoverable from the declaration.
- Entry: `SessionMode.OfRunMode` returns `Fin<SessionMode>` and rejects an unknown foreign ordinal. `DocumentSession.Of` rejects an empty or mode-incompatible capability set before acquisition; `DocumentSession.Demand` proves one or more granted rows against one fresh snapshot around the consuming host body.
- Law: `Observe` admits transition snapshots, while operation capabilities require `Ready`. Mutation excludes read-only documents and undo/redo replay; undo-bearing mutation additionally requires enabled undo recording. Redraw requires a live view pipeline; acquisition admits interactive and scripted command lanes; native dialogs admit only the interactive lane; interrupt remains admissible while a point acquisition is active.
- Law: `IsLocked` is detached file-write ownership evidence, not a mutation refusal; Rhino reports it when the current document owns the file lock.
- Growth: a capability lands as one `SessionNeed` row. Admission and demand folds consume `Items` through the passed values and require no parallel flag or subsystem field.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SessionMode {
    public static readonly SessionMode Interactive = new(key: 0, live: true, dialogs: true);
    public static readonly SessionMode Scripted = new(key: 1, live: true, dialogs: false);
    public static readonly SessionMode Headless = new(key: 2, live: false, dialogs: false);

    public bool Live { get; }
    public bool Dialogs { get; }

    public static Fin<SessionMode> OfRunMode(RunMode mode, Op? key = null) {
        Op op = key.OrDefault();
        return mode switch {
            RunMode.Interactive => Fin.Succ(value: Interactive),
            RunMode.Scripted => Fin.Succ(value: Scripted),
            var unknown => Fin.Fail<SessionMode>(error: op.InvalidResult(detail: unknown.ToString())),
        };
    }
}

[SmartEnum]
public sealed partial class SessionNeed {
    public static readonly SessionNeed Observe = new(
        static _ => true,
        static snapshot => snapshot.Phase.Open || snapshot.Phase.Transitional);
    public static readonly SessionNeed Read = new(static _ => true, static snapshot => snapshot.Phase.Open);
    public static readonly SessionNeed Mutate = new(
        static _ => true,
        static snapshot => snapshot.Phase.Open && !snapshot.ReadOnly && !snapshot.Undoing && !snapshot.Redoing);
    public static readonly SessionNeed Undo = new(
        static _ => true,
        static snapshot =>
            snapshot.Phase.Open
            && !snapshot.ReadOnly
            && snapshot.UndoRecording
            && !snapshot.Undoing
            && !snapshot.Redoing);
    public static readonly SessionNeed Redraw = new(
        static mode => mode.Live,
        static snapshot => snapshot.Phase.Open && !snapshot.Headless);
    public static readonly SessionNeed Acquire = new(
        static mode => mode.Live,
        static snapshot => snapshot.Phase.Open && !snapshot.Headless && !snapshot.Pointing);
    public static readonly SessionNeed Dialog = new(
        static mode => mode.Dialogs,
        static snapshot => snapshot.Phase.Open && !snapshot.Headless && !snapshot.Pointing);
    public static readonly SessionNeed Interrupt = new(
        static mode => mode.Live,
        static snapshot => snapshot.Phase.Open && !snapshot.Headless);
    public static readonly SessionNeed Export = new(static _ => true, static snapshot => snapshot.Phase.Open);

    [UseDelegateFromConstructor]
    internal partial bool AdmitsMode(SessionMode mode);

    [UseDelegateFromConstructor]
    internal partial bool AdmitsSnapshot(SessionSnapshot snapshot);

    internal bool Admits(SessionSnapshot snapshot, SessionMode mode) =>
        AdmitsMode(mode: mode) && AdmitsSnapshot(snapshot: snapshot);
}
```

## [04]-[SOURCE_AND_SESSION]

- Owner: `DocumentPath` admits absolute nonblank path text once, while `DocumentFile` carries existing-file versus existing-`.3dm` resolution as behavior rows. `SessionSource` is one flat `[Union]` over borrowed live/active/keyed/opened documents and owned empty/template/archive/configured headless documents; source depth is case data, so admission performs one generated dispatch, and the ambient `RhinoDoc.ActiveDoc` static crosses the boundary only through the `Active` case.
- Entry: `SessionSource.Acquire` returns `Fin<Lease<RhinoDoc>>`; every deterministic source/mode refusal and file requirement completes before its host call, and the `Configured` case carries a typed `ArchiveMap` (Persistence/dictionary.md) minted into the native option payload inside the acquire arm, so the host receives a fresh dictionary no caller can mutate. `DocumentSession.Of` rejects empty, duplicate, or mode-incompatible capabilities, acquires, snapshots, checks lane/document agreement, validates the kernel context, and only then adopts the lease.
- Law: a failed admission releases an owned lease before returning its original fault. A successful admission never calls `Lease.Use`, because `Use` closes `Owned` when its projection returns; the session retains the lease and releases it exactly once through `Dispose`.
- Law: `Demand<TResult>` is the capability surface. It validates a nonempty unique capability set and the consumer, re-resolves `Key`, proves every granted row against one fresh snapshot, and executes the host body through `Op.Catch`; its result constraint excludes a raw `RhinoDoc` from the result rail.
- Law: `Demand` is re-entrant on the demanding thread — the reentrant `Lock` plus the demand-depth counter admit a nested demand inside a running host body, and each nesting proves its own grants against its own fresh snapshot. `Dispose` issued during any demand defers to the outermost demand's exit, and a pending disposal refuses every new demand, nested included.
- Law: `Context()` re-enters `Context.Of(RhinoDoc)` on every call, so model-unit and tolerance changes cannot stale the context consumed by later geometry work.
- Boundary: `Lock` plus deferred reentrant disposal serializes handle resolution, evidence, callbacks, and owned cleanup. Live acquisition and demand discriminate on `RhinoApp.IsOnMainThread` and marshal through `InvokeAndWait`; headless work remains on the caller thread.
- Boundary: `IDetachedDocumentResult` marks the admitted result census: detached facts and explicit lifetime capsules. `Demand` forbids a raw `RhinoDoc`, and each capsule owns every live handle it carries beyond the callback. `DocumentPath` conforms — admitted path text is detached by construction — so a path resolution returns through `Demand` directly.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct DocumentPath : IDetachedDocumentResult {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value switch {
            "" => new ValidationError(message: "Document path is blank."),
            var path when !Path.IsPathFullyQualified(path: path) =>
                new ValidationError(message: "Document path is not fully qualified."),
            _ => null,
        };
    }

    internal Fin<string> Resolve(DocumentFile file, Op key) =>
        from policy in Optional(file).ToFin(Fail: key.InvalidInput())
        from pathAdmitted in guard(flag: policy.Admits(path: Value), False: key.InvalidInput()).ToFin()
        select Value;
}

[SmartEnum]
internal sealed partial class DocumentFile {
    public static readonly DocumentFile Existing = new(admits: static path => File.Exists(path: path));
    public static readonly DocumentFile ThreeDm = new(admits: static path =>
        File.Exists(path: path)
        && string.Equals(
            a: Path.GetExtension(path: path),
            b: ".3dm",
            comparisonType: StringComparison.OrdinalIgnoreCase));

    [UseDelegateFromConstructor]
    internal partial bool Admits(string path);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SessionSource {
    private SessionSource() { }

    public sealed record Live(RhinoDoc Document) : SessionSource;
    public sealed record Active : SessionSource;
    public sealed record Keyed(DocKey Key) : SessionSource;
    public sealed record Opened(DocumentPath Path) : SessionSource;
    public sealed record Empty : SessionSource;
    public sealed record Template(DocumentPath Path) : SessionSource;
    public sealed record Archive(DocumentPath Path) : SessionSource;
    public sealed record Configured(DocumentPath Path, ArchiveMap Options) : SessionSource;

    internal Fin<Lease<RhinoDoc>> Acquire(SessionMode mode, Op key) =>
        from modeAdmitted in Admits(mode: mode, key: key)
        from lease in Switch(
            state: key,
            live: static (op, source) => Borrowed(document: Optional(source.Document).ToFin(Fail: op.MissingContext())),
            active: static (op, _) => op.Catch(() =>
                Borrowed(document: Optional(RhinoDoc.ActiveDoc).ToFin(Fail: op.MissingContext()))),
            keyed: static (op, source) => Borrowed(document: source.Key.Resolve(key: op)),
            opened: static (op, source) =>
                from path in source.Path.Resolve(file: DocumentFile.ThreeDm, key: op)
                from acquired in op.Catch(() => Borrowed(document: Optional(RhinoDoc.Open(
                        filePath: path,
                        wasAlreadyOpen: out _))
                    .ToFin(Fail: op.InvalidResult(detail: path))))
                select acquired,
            empty: static (op, _) => op.Catch(() => Minted(
                document: RhinoDoc.CreateHeadless(file3dmTemplatePath: string.Empty),
                key: op)),
            template: static (op, source) => Headless(
                path: source.Path,
                open: static resolved => RhinoDoc.CreateHeadless(file3dmTemplatePath: resolved),
                key: op),
            archive: static (op, source) => Headless(
                path: source.Path,
                open: static resolved => RhinoDoc.OpenHeadless(file3dmPath: resolved),
                key: op),
            configured: static (op, source) =>
                from path in source.Path.Resolve(file: DocumentFile.Existing, key: op)
                from options in Optional(source.Options).ToFin(Fail: op.InvalidInput())
                from minted in options.Mint(key: op)
                from lease in op.Catch(() => Minted(
                    document: RhinoDoc.OpenHeadless(filePath: path, options: minted),
                    key: op))
                select lease)
        select lease;

    private Fin<Unit> Admits(SessionMode mode, Op key) =>
        guard(
            flag: Switch(
                state: mode,
                live: static (lane, _) => lane.Live,
                active: static (lane, _) => lane.Live,
                keyed: static (_, _) => true,
                opened: static (lane, _) => lane.Live,
                empty: static (lane, _) => !lane.Live,
                template: static (lane, _) => !lane.Live,
                archive: static (lane, _) => !lane.Live,
                configured: static (lane, _) => !lane.Live),
            False: key.InvalidInput()).ToFin();

    private static Fin<Lease<RhinoDoc>> Borrowed(Fin<RhinoDoc> document) =>
        document.Map(static value => (Lease<RhinoDoc>)new Lease<RhinoDoc>.Borrowed(Value: value));

    private static Fin<Lease<RhinoDoc>> Headless(DocumentPath path, Func<string, RhinoDoc?> open, Op key) =>
        from resolved in path.Resolve(file: DocumentFile.ThreeDm, key: key)
        from lease in key.Catch(() => Minted(document: open(arg: resolved), key: key))
        select lease;

    private static Fin<Lease<RhinoDoc>> Minted(RhinoDoc? document, Op key) =>
        Optional(document)
            .ToFin(Fail: key.InvalidResult())
            .Map(static value => (Lease<RhinoDoc>)new Lease<RhinoDoc>.Owned(Value: value));
}

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed class DocumentSession : IDisposable {
    private readonly Lock gate = new();
    private readonly Lease<RhinoDoc> lease;
    private readonly LanguageExt.HashSet<SessionNeed> granted;
    private bool disposePending;
    private bool released;
    private int demandDepth;

    private DocumentSession(
        DocKey key,
        SessionMode mode,
        Lease<RhinoDoc> lease,
        LanguageExt.HashSet<SessionNeed> granted) {
        Key = key;
        Mode = mode;
        this.granted = granted;
        this.lease = lease;
    }

    public DocKey Key { get; }
    public SessionMode Mode { get; }

    public static Fin<DocumentSession> Of(
        SessionSource source,
        SessionMode mode,
        params ReadOnlySpan<SessionNeed> needs) {
        Op op = Op.Of();
        return from request in Optional(source).ToFin(Fail: op.InvalidInput())
               from lane in Optional(mode).ToFin(Fail: op.InvalidInput())
               from demanded in AdmitNeeds(needs: needs, mode: lane, op: op)
               from session in OnHost(
                   mode: lane,
                   use: () =>
                       from acquired in request.Acquire(mode: lane, key: op)
                       from adopted in Adopt(
                           acquired: acquired,
                           lane: lane,
                           granted: demanded,
                           op: op)
                       select adopted,
                   op: op)
               select session;
    }

    public Fin<SessionSnapshot> Snapshot(Op? key = null) {
        Op op = key.OrDefault();
        return Demand(
            use: document => SessionSnapshot.Of(document: document, key: op),
            key: op,
            needs: [SessionNeed.Observe]);
    }

    internal Fin<TResult> Demand<TResult>(
        Func<RhinoDoc, Fin<TResult>> use,
        Op? key = null,
        params ReadOnlySpan<SessionNeed> needs)
        where TResult : IDetachedDocumentResult =>
        DemandCore(use: use, key: key, needs: needs);

    public Fin<Context> Context(Op? key = null) {
        Op op = key.OrDefault();
        return DemandCore(
            use: document => Rasm.Domain.Context.Of(doc: document).ToFin(),
            key: op,
            needs: [SessionNeed.Read]);
    }

    public Fin<Unit> Interrupt(Op? key = null) {
        Op op = key.OrDefault();
        return DemandCore(
            use: document => {
                document.TimeoutActiveGet();
                return Fin.Succ(value: unit);
            },
            key: op,
            needs: [SessionNeed.Interrupt]);
    }

    public void Dispose() {
        lock (gate) {
            if (released || disposePending) {
                return;
            }

            if (demandDepth > 0) {
                disposePending = true;
                return;
            }

            released = true;
            lease.Dispose();
        }
    }

    private Fin<TResult> DemandCore<TResult>(
        Func<RhinoDoc, Fin<TResult>> use,
        Op? key,
        ReadOnlySpan<SessionNeed> needs) {
        Op op = key.OrDefault();
        return from body in Optional(use).ToFin(Fail: op.InvalidInput())
               from requested in AdmitNeeds(needs: needs, mode: Mode, op: op)
               from result in OnHost(
                   mode: Mode,
                   use: () => DemandLocked(use: body, requested: requested, op: op),
                   op: op)
               select result;
    }

    private Fin<TResult> DemandLocked<TResult>(
        Func<RhinoDoc, Fin<TResult>> use,
        LanguageExt.HashSet<SessionNeed> requested,
        Op op) {
        lock (gate) {
            if (released || disposePending) {
                return Fin.Fail<TResult>(error: op.MissingContext());
            }

            demandDepth += 1;
            Fin<TResult> result;
            try {
                result = op.Catch(() =>
                    from grants in requested.AsIterable()
                        .TraverseM(need => granted.Contains(need)
                            ? Fin.Succ(value: need)
                            : Fin.Fail<SessionNeed>(error: op.MissingContext()))
                        .As()
                    from document in Key.Resolve(key: op)
                    from snapshot in SessionSnapshot.Of(document: document, key: op)
                    from capabilities in requested.AsIterable()
                        .TraverseM(need => need.Admits(snapshot: snapshot, mode: Mode)
                            ? Fin.Succ(value: need)
                            : Fin.Fail<SessionNeed>(error: op.MissingContext()))
                        .As()
                    from value in Optional(use(arg: document))
                        .ToFin(Fail: op.InvalidResult())
                        .Bind(identity)
                    select value);
            } finally {
                demandDepth -= 1;
            }

            return demandDepth is 0 && disposePending
                ? CompleteDisposal(result: result, op: op)
                : result;
        }
    }

    private Fin<TResult> CompleteDisposal<TResult>(Fin<TResult> result, Op op) {
        disposePending = false;
        return Release(op: op).BiBind(
            Succ: _ => result,
            Fail: cleanup => result.BiBind(
                Succ: _ => Fin.Fail<TResult>(error: cleanup),
                Fail: failure => Fin.Fail<TResult>(error: failure + cleanup)));
    }

    private Fin<Unit> Release(Op op) {
        if (released) {
            return Fin.Succ(value: unit);
        }

        released = true;
        return op.Catch(() => {
            lease.Dispose();
            return Fin.Succ(value: unit);
        });
    }

    private static Fin<LanguageExt.HashSet<SessionNeed>> AdmitNeeds(
        ReadOnlySpan<SessionNeed> needs,
        SessionMode mode,
        Op op) =>
        from demanded in toSeq(needs.ToArray())
            .TraverseM(need => Optional(need).ToFin(Fail: op.InvalidInput()))
            .As()
        from nonempty in guard(flag: !demanded.IsEmpty, False: op.InvalidInput()).ToFin()
        let distinct = toHashSet(demanded)
        from unique in guard(flag: distinct.Count == demanded.Count, False: op.InvalidInput()).ToFin()
        from modeAdmitted in distinct.AsIterable()
            .TraverseM(need => need.AdmitsMode(mode: mode)
                ? Fin.Succ(value: need)
                : Fin.Fail<SessionNeed>(error: op.InvalidInput()))
            .As()
        select distinct;

    private static Fin<TResult> OnHost<TResult>(SessionMode mode, Func<Fin<TResult>> use, Op op) =>
        from lane in Optional(mode).ToFin(Fail: op.InvalidInput())
        from body in Optional(use).ToFin(Fail: op.InvalidInput())
        from result in op.Catch(() => {
            if (!lane.Live || RhinoApp.IsOnMainThread) {
                return body();
            }

            Fin<TResult> dispatched = Fin.Fail<TResult>(error: op.InvalidResult());
            RhinoApp.InvokeAndWait(() => dispatched = op.Catch(body));
            return dispatched;
        })
        select result;

    private static Fin<DocumentSession> Adopt(
        Lease<RhinoDoc> acquired,
        SessionMode lane,
        LanguageExt.HashSet<SessionNeed> granted,
        Op op) {
        RhinoDoc document = acquired.Resource;
        Fin<DocumentSession> admitted = op.Catch(() =>
            from snapshot in SessionSnapshot.Of(document: document, key: op)
            from laneAdmitted in guard(
                flag: snapshot.Headless != lane.Live,
                False: op.InvalidInput()).ToFin()
            from capabilities in granted.AsIterable()
                .TraverseM(need => need.Admits(snapshot: snapshot, mode: lane)
                    ? Fin.Succ(value: need)
                    : Fin.Fail<SessionNeed>(error: op.MissingContext()))
                .As()
            from key in DocKey.Of(document: document, key: op)
            from context in Rasm.Domain.Context.Of(doc: document).ToFin()
            select new DocumentSession(
                key: key,
                mode: lane,
                lease: acquired,
                granted: granted));
        return admitted.BindFail(error => op.Catch(() => {
            acquired.Dispose();
            return Fin.Succ(value: unit);
        }).BiBind(
            Succ: _ => Fin.Fail<DocumentSession>(error: error),
            Fail: cleanup => Fin.Fail<DocumentSession>(error: error + cleanup)));
    }
}
```

## [05]-[REGIME]

- Owner: `DocumentSpace` carries the model/page host-member axis as read, tolerance-write, and precision-write behavior columns. Kernel `ModelUnit` owns unit admission and equality; the native `LengthUnit` survives only beside the host call that reads or writes it. Closed `RegimeChange` cases expose one overloaded `Of` admission family and one mutation dispatch.
- Entry: `session.Regime(space)` reads a validated `UnitRegime`. `session.Adjust(space, change)` captures the before regime, seals one undo record around the change, proves the exact requested postcondition, and returns `RegimeReceipt` with the sealed serial.
- Law: `UnitScaling` replaces the host `bool scale` knob with `PreserveCoordinates` and `PreservePhysicalSize`. Known, custom, and native `LengthUnit` inputs all produce the private-constructed `RegimeChange.Units` case, so `AdjustLengthUnits` is the sole unit-system mutation path.
- Law: tolerance values compose the kernel `AbsoluteTolerance`, `RelativeTolerance`, and `AngleTolerance` owners. This page carries no independent numeric admission or radians/degrees duplication.
- Law: unit postconditions compare canonical `ModelUnit` evidence, including custom name and meters-per-unit scale. `UnitRegime` retains the native `LengthUnit` only for compensation.
- Boundary: the row delegates contain the property-set statement seam required by the host API. A failed write restores every scalar without assuming a failed unit call changed geometry; a proven unit write followed by a failed postcondition reverses the unit scaling and restores every scalar. Compensation faults accumulate and join the original fault, and the shared `UndoBracket` seals or rolls back the enclosing record.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class UnitScaling {
    public static readonly UnitScaling PreserveCoordinates = new(key: 0, hostScale: false);
    public static readonly UnitScaling PreservePhysicalSize = new(key: 1, hostScale: true);

    internal bool HostScale { get; }
}

[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct DisplayPrecision {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value < 0
            ? new ValidationError(message: "Display precision is negative.")
            : null;
}

[SmartEnum<int>]
public sealed partial class DocumentSpace {
    public static readonly DocumentSpace Model = new(
        key: 0,
        modelUnits: true,
        read: static (document, op) => UnitRegime.Of(
            units: document.ModelUnits,
            absolute: document.ModelAbsoluteTolerance,
            relative: document.ModelRelativeTolerance,
            angle: document.ModelAngleToleranceRadians,
            precision: document.ModelDistanceDisplayPrecision,
            op: op),
        setTolerances: static (document, tolerances) => {
            document.ModelAbsoluteTolerance = tolerances.Absolute.Value;
            document.ModelRelativeTolerance = tolerances.Relative.Value;
            document.ModelAngleToleranceRadians = tolerances.Angle.Value;
            return unit;
        },
        setPrecision: static (document, precision) => {
            document.ModelDistanceDisplayPrecision = precision.Value;
            return unit;
        });

    public static readonly DocumentSpace Page = new(
        key: 1,
        modelUnits: false,
        read: static (document, op) => UnitRegime.Of(
            units: document.PageUnits,
            absolute: document.PageAbsoluteTolerance,
            relative: document.PageRelativeTolerance,
            angle: document.PageAngleToleranceRadians,
            precision: document.PageDistanceDisplayPrecision,
            op: op),
        setTolerances: static (document, tolerances) => {
            document.PageAbsoluteTolerance = tolerances.Absolute.Value;
            document.PageRelativeTolerance = tolerances.Relative.Value;
            document.PageAngleToleranceRadians = tolerances.Angle.Value;
            return unit;
        },
        setPrecision: static (document, precision) => {
            document.PageDistanceDisplayPrecision = precision.Value;
            return unit;
        });

    internal bool ModelUnits { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<UnitRegime> Read(RhinoDoc document, Op op);

    [UseDelegateFromConstructor]
    internal partial Unit SetTolerances(RhinoDoc document, ToleranceRegime tolerances);

    [UseDelegateFromConstructor]
    internal partial Unit SetPrecision(RhinoDoc document, DisplayPrecision precision);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RegimeChange {
    private RegimeChange() { }

    public sealed record Units : RegimeChange {
        private Units(LengthUnit native, ModelUnit unit, UnitScaling scaling) =>
            (Native, Unit, Scaling) = (native, unit, scaling);

        internal LengthUnit Native { get; }
        public ModelUnit Unit { get; }
        public UnitScaling Scaling { get; }

        internal static Units Of(LengthUnit native, ModelUnit unit, UnitScaling scaling) =>
            new(native: native, unit: unit, scaling: scaling);
    }

    public sealed record Tolerances(ToleranceRegime Value) : RegimeChange;

    public sealed record Precision(DisplayPrecision Value) : RegimeChange;

    public static Fin<RegimeChange> Of(
        UnitSystem system,
        UnitScaling scaling,
        Op? key = null) {
        Op op = key.OrDefault();
        return from policy in Optional(scaling).ToFin(Fail: op.InvalidInput())
               from admitted in ModelUnit.Of(value: system, key: op)
               from native in op.Catch(() => Fin.Succ(value: LengthUnit.FromKnownUnitSystem(knownUnitSystem: admitted.System)))
               select (RegimeChange)Units.Of(native: native, unit: admitted, scaling: policy);
    }

    public static Fin<RegimeChange> Of(
        string name,
        double metersPerUnit,
        UnitScaling scaling,
        Op? key = null) {
        Op op = key.OrDefault();
        return from policy in Optional(scaling).ToFin(Fail: op.InvalidInput())
               from label in op.AcceptText(value: name)
               from scale in op.Positive(value: metersPerUnit)
               from unitValue in op.Catch(() => Fin.Succ(value: LengthUnit.FromCustomUnitSystem(
                   name: label,
                   customUnitSize: scale,
                   knownUnitSystem: UnitSystem.Meters)))
               from admitted in ModelUnit.Of(value: unitValue, key: op)
               select (RegimeChange)Units.Of(native: unitValue, unit: admitted, scaling: policy);
    }

    public static Fin<RegimeChange> Of(
        LengthUnit units,
        UnitScaling scaling,
        Op? key = null) {
        Op op = key.OrDefault();
        return from policy in Optional(scaling).ToFin(Fail: op.InvalidInput())
               from admitted in ModelUnit.Of(value: units, key: op)
               select (RegimeChange)Units.Of(native: units, unit: admitted, scaling: policy);
    }

    public static Fin<RegimeChange> Of(
        double absolute,
        double relative,
        double angle,
        Op? key = null) {
        Op op = key.OrDefault();
        return ToleranceRegime.Of(
                absolute: absolute,
                relative: relative,
                angle: angle,
                key: op)
            .Map(static value => (RegimeChange)new Tolerances(Value: value));
    }

    public static Fin<RegimeChange> Of(int precision, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<DisplayPrecision>(candidate: precision)
            .Map(static value => (RegimeChange)new Precision(Value: value));
    }

    internal Fin<Unit> Apply(
        RhinoDoc document,
        DocumentSpace space,
        UnitRegime before,
        Op op) {
        Fin<Unit> mutation = Switch(
            state: (Document: document, Space: space, Op: op),
            units: static (context, change) => context.Op.Catch(() => context.Op.Confirm(
                success: context.Document.AdjustLengthUnits(
                    modelUnits: context.Space.ModelUnits,
                    units: change.Native,
                    scale: change.Scaling.HostScale))),
            tolerances: static (context, change) => context.Op.Catch(() => Fin.Succ(
                value: context.Space.SetTolerances(
                    document: context.Document,
                    tolerances: change.Value))),
            precision: static (context, change) => context.Op.Catch(() => Fin.Succ(
                value: context.Space.SetPrecision(
                    document: context.Document,
                    precision: change.Value))));
        return mutation.BindFail(error => CompensateFailed<Unit>(
            document: document,
            space: space,
            before: before,
            failure: error,
            op: op));
    }

    internal bool Matches(UnitRegime actual) =>
        Switch(
            state: actual,
            units: static (regime, change) => regime.Unit == change.Unit,
            tolerances: static (regime, change) => regime.Tolerances == change.Value,
            precision: static (regime, change) => regime.Precision == change.Value);

    internal Fin<TResult> Rollback<TResult>(
        RhinoDoc document,
        DocumentSpace space,
        UnitRegime before,
        Error failure,
        Op op) =>
        Compensate(
            document: document,
            space: space,
            before: before,
            reversal: Reversal(),
            failure: failure,
            op: op);

    private Fin<TResult> CompensateFailed<TResult>(
        RhinoDoc document,
        DocumentSpace space,
        UnitRegime before,
        Error failure,
        Op op) =>
        Compensate(
            document: document,
            space: space,
            before: before,
            reversal: None,
            failure: failure,
            op: op);

    private static Fin<TResult> Compensate<TResult>(
        RhinoDoc document,
        DocumentSpace space,
        UnitRegime before,
        Option<UnitScaling> reversal,
        Error failure,
        Op op) =>
        Restore(
            document: document,
            space: space,
            before: before,
            reversal: reversal,
            op: op).BiBind(
            Succ: _ => Fin.Fail<TResult>(error: failure),
            Fail: cleanup => Fin.Fail<TResult>(error: failure + cleanup));

    private Option<UnitScaling> Reversal() =>
        Switch(
            units: static change => Some(change.Scaling),
            tolerances: static _ => None,
            precision: static _ => None);

    private static Fin<Unit> Restore(
        RhinoDoc document,
        DocumentSpace space,
        UnitRegime before,
        Option<UnitScaling> reversal,
        Op op) {
        K<Validation<Error>, Unit> units = reversal.Match(
            Some: scaling => op.Catch(() => op.Confirm(
                success: document.AdjustLengthUnits(
                    modelUnits: space.ModelUnits,
                    units: before.Native,
                    scale: scaling.HostScale))),
            None: () => Fin.Succ(value: unit)).ToValidation();
        K<Validation<Error>, Unit> tolerances = op.Catch(() => Fin.Succ(value: space.SetTolerances(
            document: document,
            tolerances: before.Tolerances))).ToValidation();
        K<Validation<Error>, Unit> precision = op.Catch(() => Fin.Succ(value: space.SetPrecision(
            document: document,
            precision: before.Precision))).ToValidation();
        return (units, tolerances, precision)
            .Apply(static (_, _, _) => unit)
            .As()
            .ToFin();
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record ToleranceRegime(
    AbsoluteTolerance Absolute,
    RelativeTolerance Relative,
    AngleTolerance Angle) : IDetachedDocumentResult {
    public static Fin<ToleranceRegime> Of(
        double absolute,
        double relative,
        double angle,
        Op? key = null) {
        Op op = key.OrDefault();
        return from admittedAbsolute in op.AcceptValidated<AbsoluteTolerance>(candidate: absolute)
               from admittedRelative in op.AcceptValidated<RelativeTolerance>(candidate: relative)
               from admittedAngle in op.AcceptValidated<AngleTolerance>(candidate: angle)
               select new ToleranceRegime(
                   Absolute: admittedAbsolute,
                   Relative: admittedRelative,
                   Angle: admittedAngle);
    }
}

public sealed record UnitRegime : IDetachedDocumentResult {
    private UnitRegime(
        LengthUnit native,
        ModelUnit unit,
        ToleranceRegime tolerances,
        DisplayPrecision precision) =>
        (Native, Unit, Tolerances, Precision) = (native, unit, tolerances, precision);

    internal LengthUnit Native { get; }
    public ModelUnit Unit { get; }
    public ToleranceRegime Tolerances { get; }
    public DisplayPrecision Precision { get; }

    internal static Fin<UnitRegime> Of(
        LengthUnit units,
        double absolute,
        double relative,
        double angle,
        int precision,
        Op op) =>
        from admittedUnit in ModelUnit.Of(value: units, key: op)
        from admittedTolerances in ToleranceRegime.Of(
            absolute: absolute,
            relative: relative,
            angle: angle,
            key: op)
        from admittedPrecision in op.AcceptValidated<DisplayPrecision>(candidate: precision)
        select new UnitRegime(
            native: units,
            unit: admittedUnit,
            tolerances: admittedTolerances,
            precision: admittedPrecision);
}

public sealed record RegimeReceipt(
    DocumentSpace Space,
    RegimeChange Change,
    UnitRegime Before,
    UnitRegime After,
    uint UndoRecord = 0u) : IDetachedDocumentResult;

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class SessionRegimes {
    extension(DocumentSession session) {
        public Fin<UnitRegime> Regime(DocumentSpace space, Op? key = null) {
            Op op = key.OrDefault();
            return from axis in Optional(space).ToFin(Fail: op.InvalidInput())
                   from regime in session.Demand(
                       use: document => axis.Read(document: document, op: op),
                       key: op,
                       needs: [SessionNeed.Read])
                   select regime;
        }

        public Fin<RegimeReceipt> Adjust(
            DocumentSpace space,
            RegimeChange change,
            Op? key = null) {
            Op op = key.OrDefault();
            return from axis in Optional(space).ToFin(Fail: op.InvalidInput())
                   from request in Optional(change).ToFin(Fail: op.InvalidInput())
                   from receipt in session.Demand(
                       use: document => {
                           using UndoBracket undo = UndoBracket.Begin(
                               document: document,
                               name: nameof(Adjust),
                               recordsUndo: true);
                           Fin<RegimeReceipt> executed = guard(undo.Admitted, op.InvalidResult()).ToFin().Bind(_ =>
                               from before in axis.Read(document: document, op: op)
                               from applied in request.Apply(
                                   document: document,
                                   space: axis,
                                   before: before,
                                   op: op)
                               from after in (
                                   from observed in axis.Read(document: document, op: op)
                                   from matches in op.Catch(() => Fin.Succ(value: request.Matches(actual: observed)))
                                   from exact in guard(flag: matches, False: op.InvalidResult()).ToFin()
                                   select observed).BindFail(error => request.Rollback<UnitRegime>(
                                   document: document,
                                   space: axis,
                                   before: before,
                                   failure: error,
                                   op: op))
                               select new RegimeReceipt(
                                   Space: axis,
                                   Change: request,
                                   Before: before,
                                   After: after));
                           return undo.Seal(
                               outcome: executed,
                               stamp: static (result, serial) => result with { UndoRecord = serial },
                               key: op);
                       },
                       key: op,
                       needs: [SessionNeed.Mutate, SessionNeed.Undo])
                   select receipt;
        }
    }
}
```

## [06]-[REGIME_TEXT]

- Owner: `UnitDialect` — behavior rows handing back the host parse-settings presets, one per admitted numeric grammar; `AngleGrammar` rows own the bare-number angle interpretation, each parsing through its host expression member and landing canonical radians. `UnitForm` and `UnitNotation` re-close the foreign `LengthValue.StringFormat` and `DistanceDisplayMode` ordinals. `UnitPhrase` `[Union]` admits length, scale, and angle text; `UnitReading` `[Union]` is the detached parse evidence; `UnitScript` `[Union]` carries the render requests — exact round-trip text, locale display formatting, and the unit-system name. Parse and render are the two directions of one regime-text correspondence on one owner pair, never direction-named siblings.
- Entry: `session.Parse(space, phrase)` resolves the regime through the capability rail, then parses against it; `session.Render(space, script)` reads the same regime and projects text. Both compose `Regime`, so every text operation prices the live regime, never a cached unit.
- Law: dialect rows return the host preset statics — process-shared constants whose `Dispose` is inert — and never mutate one: a preset setter writes through to the shared native object and poisons every later parse, and `new StringParserSettings()` seeds a different ambiguous grammar, so neither is a dialect base.
- Law: a length parse admits only a whole-string parse of a set value — `parsedAll` and `!IsUnset()` gate together — and converts on egress through `Length(LengthUnit)` into regime units; a unitless phrase resolves under the dialect's `DefaultLengthUnitSystem` before that conversion. A unitless scale ("1:100") parses to `LengthUnit.None` on both sides, so `UnitReading.ScaleCase` carries `Unitless` as evidence and the consumer substitutes the document unit where a host write rejects `None`.
- Law: an angle phrase parses through `StringParser.ParseAngleExpressionRadians` or `ParseAngleExpressionDegrees` under its `AngleGrammar` row; the degrees row converts through `RhinoMath.ToRadians` at the seam, so `UnitReading.AngleCase` always carries canonical radians and no consumer re-derives the angular unit.
- Law: every parsed `LengthValue`/`ScaleValue` is a native disposable bracketed inside its arm; text leaves as detached values and rendered strings, never as a live parse handle. Exact rendering round-trips through `LengthValue.LengthString` under the ambient host locale; display rendering and unit naming ride `global::Rhino.UI.Localization.FormatNumber`/`UnitSystemName` with precision drawn from the regime.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class UnitDialect {
    public static readonly UnitDialect Standard = new(key: 0, settings: static () => StringParserSettings.DefaultParseSettings);
    public static readonly UnitDialect Integers = new(key: 1, settings: static () => StringParserSettings.ParseSettingsIntegerNumber);
    public static readonly UnitDialect Rationals = new(key: 2, settings: static () => StringParserSettings.ParseSettingsRationalNumber);
    public static readonly UnitDialect Reals = new(key: 3, settings: static () => StringParserSettings.ParseSettingsRealNumber);

    [UseDelegateFromConstructor]
    internal partial StringParserSettings Settings();
}

[SmartEnum<int>]
public sealed partial class AngleGrammar {
    public static readonly AngleGrammar Radians = new(key: 0, parse: static (text, op) =>
        StringParser.ParseAngleExpressionRadians(text, out double value)
            ? Fin.Succ(value: value)
            : Fin.Fail<double>(error: op.InvalidInput()));
    public static readonly AngleGrammar Degrees = new(key: 1, parse: static (text, op) =>
        StringParser.ParseAngleExpressionDegrees(text, out double value)
            ? Fin.Succ(value: RhinoMath.ToRadians(value))
            : Fin.Fail<double>(error: op.InvalidInput()));

    [UseDelegateFromConstructor]
    internal partial Fin<double> Parse(string text, Op op);
}

[SmartEnum<int>]
public sealed partial class UnitForm {
    public static readonly UnitForm ExactDecimal = new(key: 0, native: LengthValue.StringFormat.ExactDecimal);
    public static readonly UnitForm ExactProperFraction = new(key: 1, native: LengthValue.StringFormat.ExactProperFraction);
    public static readonly UnitForm ExactImproperFraction = new(key: 2, native: LengthValue.StringFormat.ExactImproperFraction);
    public static readonly UnitForm CleanDecimal = new(key: 3, native: LengthValue.StringFormat.CleanDecimal);
    public static readonly UnitForm CleanProperFraction = new(key: 4, native: LengthValue.StringFormat.CleanProperFraction);
    public static readonly UnitForm CleanImproperFraction = new(key: 5, native: LengthValue.StringFormat.CleanImproperFraction);

    internal LengthValue.StringFormat Native { get; }
}

[SmartEnum<int>]
public sealed partial class UnitNotation {
    public static readonly UnitNotation Decimal = new(key: 0, native: global::Rhino.UI.DistanceDisplayMode.Decimal);
    public static readonly UnitNotation Fractional = new(key: 1, native: global::Rhino.UI.DistanceDisplayMode.Fractional);
    public static readonly UnitNotation FeetInches = new(key: 2, native: global::Rhino.UI.DistanceDisplayMode.FeetInches);

    internal global::Rhino.UI.DistanceDisplayMode Native { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UnitReading : IDetachedDocumentResult {
    private UnitReading() { }

    public sealed record LengthCase(double Value, ModelUnit Unit, UnitSystem Source) : UnitReading;
    public sealed record ScaleCase(double LeftToRight, double RightToLeft, bool Unitless) : UnitReading;
    public sealed record AngleCase(double Radians) : UnitReading;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UnitPhrase {
    private UnitPhrase() { }

    internal sealed record LengthCase(string Text, UnitDialect Dialect) : UnitPhrase;
    internal sealed record ScaleCase(string Text) : UnitPhrase;
    internal sealed record AngleCase(string Text, AngleGrammar Grammar) : UnitPhrase;

    public static Fin<UnitPhrase> Length(string text, Option<UnitDialect> dialect = default, Op? key = null) =>
        key.OrDefault().AcceptText(value: text)
            .Map(admitted => (UnitPhrase)new LengthCase(Text: admitted, Dialect: dialect.IfNone(UnitDialect.Standard)));

    public static Fin<UnitPhrase> Scale(string text, Op? key = null) =>
        key.OrDefault().AcceptText(value: text)
            .Map(static admitted => (UnitPhrase)new ScaleCase(Text: admitted));

    public static Fin<UnitPhrase> Angle(string text, Option<AngleGrammar> grammar = default, Op? key = null) =>
        key.OrDefault().AcceptText(value: text)
            .Map(admitted => (UnitPhrase)new AngleCase(Text: admitted, Grammar: grammar.IfNone(AngleGrammar.Radians)));

    internal Fin<UnitReading> Parse(UnitRegime regime, Op key) => Switch(
        state: (Regime: regime, Op: key),
        lengthCase: static (ctx, phrase) => ctx.Op.Catch(() => {
            using LengthValue parsed = LengthValue.Create(s: phrase.Text, ps: phrase.Dialect.Settings(), parsedAll: out bool parsedAll);
            return from _whole in guard(flag: parsedAll && !parsed.IsUnset(), False: ctx.Op.InvalidInput()).ToFin()
                   from value in ctx.Op.Catch(() => Fin.Succ(value: parsed.Length(units: ctx.Regime.Native)))
                   select (UnitReading)new UnitReading.LengthCase(Value: value, Unit: ctx.Regime.Unit, Source: parsed.UnitSystem);
        }),
        scaleCase: static (ctx, phrase) => ctx.Op.Catch(() => {
            using ScaleValue parsed = ScaleValue.Create(s: phrase.Text, ps: StringParserSettings.DefaultParseSettings);
            using LengthValue left = parsed.LeftLengthValue();
            using LengthValue right = parsed.RightLengthValue();
            return from _set in guard(flag: !parsed.IsUnset(), False: ctx.Op.InvalidInput()).ToFin()
                   select (UnitReading)new UnitReading.ScaleCase(
                       LeftToRight: parsed.LeftToRightScale,
                       RightToLeft: parsed.RightToLeftScale,
                       Unitless: LengthUnit.IsNone(left.Units) && LengthUnit.IsNone(right.Units));
        }),
        angleCase: static (ctx, phrase) => phrase.Grammar.Parse(text: phrase.Text, op: ctx.Op)
            .Map(static radians => (UnitReading)new UnitReading.AngleCase(Radians: radians)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UnitScript {
    private UnitScript() { }

    internal sealed record ExactCase(double Value, UnitForm Form) : UnitScript;
    internal sealed record DisplayCase(double Value, UnitNotation Notation, bool AppendUnitName) : UnitScript;
    internal sealed record NameCase(bool Capitalize, bool Singular, bool Abbreviate) : UnitScript;

    public static Fin<UnitScript> Exact(double value, Option<UnitForm> form = default, Op? key = null) =>
        guard(flag: double.IsFinite(value), False: key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => (UnitScript)new ExactCase(Value: value, Form: form.IfNone(UnitForm.CleanDecimal)));

    public static Fin<UnitScript> Display(double value, Option<UnitNotation> notation = default, bool appendUnitName = false, Op? key = null) =>
        guard(flag: double.IsFinite(value), False: key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => (UnitScript)new DisplayCase(Value: value, Notation: notation.IfNone(UnitNotation.Decimal), AppendUnitName: appendUnitName));

    public static UnitScript Name(bool capitalize = true, bool singular = true, bool abbreviate = false) =>
        new NameCase(Capitalize: capitalize, Singular: singular, Abbreviate: abbreviate);

    internal Fin<string> Render(UnitRegime regime, Op key) => Switch(
        state: (Regime: regime, Op: key),
        exactCase: static (ctx, script) => ctx.Op.Catch(() => {
            using LengthValue value = LengthValue.Create(length: script.Value, units: ctx.Regime.Native, format: script.Form.Native);
            return guard(flag: !value.IsUnset(), False: ctx.Op.InvalidResult()).ToFin().Map(_ => value.LengthString);
        }),
        displayCase: static (ctx, script) => ctx.Op.Catch(() => Fin.Succ(value: global::Rhino.UI.Localization.FormatNumber(
            x: script.Value,
            units: ctx.Regime.Native,
            mode: script.Notation.Native,
            precision: ctx.Regime.Precision.Value,
            appendUnitSystemName: script.AppendUnitName))),
        nameCase: static (ctx, script) => ctx.Op.Catch(() => Fin.Succ(value: global::Rhino.UI.Localization.UnitSystemName(
            units: ctx.Regime.Unit.System,
            capitalize: script.Capitalize,
            singular: script.Singular,
            abbreviate: script.Abbreviate))));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class RegimeText {
    extension(DocumentSession session) {
        public Fin<UnitReading> Parse(DocumentSpace space, UnitPhrase phrase, Op? key = null) {
            Op op = key.OrDefault();
            return from request in Optional(phrase).ToFin(Fail: op.InvalidInput())
                   from regime in session.Regime(space: space, key: op)
                   from reading in request.Parse(regime: regime, key: op)
                   select reading;
        }

        public Fin<string> Render(DocumentSpace space, UnitScript script, Op? key = null) {
            Op op = key.OrDefault();
            return from request in Optional(script).ToFin(Fail: op.InvalidInput())
                   from regime in session.Regime(space: space, key: op)
                   from text in request.Render(regime: regime, key: op)
                   select text;
        }
    }
}
```

## [07]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]          | [OWNER]           | [FORM]                           | [ENTRY]                       |
| :-----: | :----------------- | :---------------- | :------------------------------- | :---------------------------- |
|  [01]   | document identity  | `DocKey`          | positive generated value         | `Of` / `Census`               |
|  [02]   | lifecycle evidence | `SessionSnapshot` | detached host-state product      | `DocumentSession.Snapshot`    |
|  [03]   | capability policy  | `SessionNeed`     | keyless behavior rows            | `DocumentSession.Demand`      |
|  [04]   | source admission   | `SessionSource`   | flat closed source family        | `DocumentSession.Of`          |
|  [05]   | scoped lifetime    | `DocumentSession` | retained kernel lease            | `DocumentSession.Of`          |
|  [06]   | space regime       | `DocumentSpace`   | model/page behavior rows         | `Regime` / `Adjust`           |
|  [07]   | regime mutation    | `RegimeChange`    | units/tolerances/precision union | `RegimeChange.Of` / `Adjust`  |
|  [08]   | unit-text parse    | `UnitPhrase`      | length/scale/angle phrase union  | `UnitPhrase.Length` / `Parse` |
|  [09]   | unit-text render   | `UnitScript`      | exact/display/name render union  | `UnitScript.Exact` / `Render` |
