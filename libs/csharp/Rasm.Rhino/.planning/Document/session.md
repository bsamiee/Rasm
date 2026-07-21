# [RASM_RHINO_SESSION]

`Rasm.Rhino.Document` owns document identity, lifecycle admission, capability-scoped access, document lifetime, and the live model/page regime. `DocumentSession` retains an owned headless lease only after every admission gate succeeds, serializes each consuming host call around handle re-resolution and fresh evidence, and re-reads the kernel `Context` so a regime change cannot leave a stale domain bundle.

## [01]-[INDEX]

- [02]-[IDENTITY_AND_STATE]: `DocKey`, `DocumentSet`, `SessionSnapshot`, the detached `WorksessionSnapshot` topology with serial-keyed model resolution, and the scripted `WorksessionOp` attach/detach/reload rail.
- [03]-[CAPABILITY]: `SessionMode` and the behavior-bearing `SessionNeed` capability vocabulary.
- [04]-[SOURCE_AND_SESSION]: `DocumentPath`, the flattened `SessionSource` admission family, and the lease-retaining `DocumentSession` owner.
- [05]-[REGIME]: `DocumentSpace`, the kernel-composed tolerance regime, and the one `RegimeChange` mutation rail.
- [06]-[REGIME_TEXT]: the locale-aware `UnitText` correspondence over one live regime.
- [07]-[SURFACE_LEDGER]: the page-owned surface registry.

## [02]-[IDENTITY_AND_STATE]

- Owner: `DocKey` `[ValueObject<uint>]` admits the positive `RuntimeSerialNumber`; `DocumentSet` carries live, headless, and combined census predicates as behavior rows. `SessionPhase` folds the host lifecycle product into one position, and `SessionSnapshot` `[ComplexValueObject]` admits identity, optional path/name provenance, and every capability-relevant host fact. `WorksessionSnapshot` projects the active/reference file topology, optional runtime identity, and serial-keyed model resolution without retaining the live `Worksession` handle; `WorksessionOp` carries one model plus a closed verb program, and each `WorksessionVerb` row owns script, membership, and inverse policy.
- Entry: `DocKey.Census` returns detached keys from the host iterator. Internal `DocKey.Resolve` re-enters the handle only inside the session owner, and internal `SessionSnapshot.Of` is the single read site for availability, transition, access, undo enablement, active undo recording, headless, dirty, active-command, and public point-acquisition state. One `session.Worksession` name carries both modalities — a serial spread reads, a `WorksessionOp` value transitions — and `WorksessionSnapshot.FileOf` resolves a worksession runtime serial to its file with no document at all.
- Law: lifecycle precedence is closing, opening, initializing, creating, ready, unavailable; the tuple switch covers the complete flag product and names no default arm.
- Law: `SessionSnapshot` is immutable evidence from one read. Every capability use re-resolves the retained key and obtains a new snapshot inside `DocumentSession.Demand` immediately before invoking its host body.
- Law: the host `Worksession` is a read-only roster — every transition rides the serial-pinned script rail: per verb one fresh membership precondition, one scripted host run, one membership postcondition, and one declared inverse; reload composes detach then attach inside one demand window, and a failed suffix restores the completed prefix before the rail returns. A receipt carries the before and after topology, never a bare success flag.
- Boundary: `InGetPoint` proves only a point acquisition; the broader acquisition reentrancy token belongs to the command acquisition algebra.
- Boundary: `Worksession.ModelCount` may exceed `ModelPaths.Length` by one for an unsaved active model; `UnsavedActive` preserves that state. Serial resolution admits unique requests, exact key coverage, distinct resolved paths, and membership in the model-path roster before map construction. Attach/detach observation stays on the events page's `WorksessionFile` family; this owner carries the transactional receipt.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Globalization;
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
            .Bind(candidate => op.AcceptValidated<DocKey>(candidate: candidate.RuntimeSerialNumber)));
    }

    public static Fin<Seq<DocKey>> Census(DocumentSet scope, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in Optional(scope).ToFin(Fail: op.InvalidInput())
               from documents in op.Catch(() => Fin.Succ(
                   value: toSeq(RhinoDoc.OpenDocuments(includeHeadless: admitted.IncludeHeadless))
                       .Filter(document => admitted.Admits(document: document))
                       .Strict()))
               from keys in documents
                   .Traverse(document => Of(document: document, key: op).ToValidation())
                   .As()
                   .ToFin()
               select keys.Strict();
    }
}

[SmartEnum<int>]
public sealed partial class DocumentSet {
    public static readonly DocumentSet Live = new(
        key: 0,
        includeHeadless: false,
        admits: static document => !document.IsHeadless);
    public static readonly DocumentSet Headless = new(
        key: 1,
        includeHeadless: true,
        admits: static document => document.IsHeadless);
    public static readonly DocumentSet All = new(
        key: 2,
        includeHeadless: true,
        admits: static _ => true);

    public bool IncludeHeadless { get; }

    [UseDelegateFromConstructor]
    internal partial bool Admits(RhinoDoc document);
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
[ComplexValueObject]
public sealed partial class SessionSnapshot : IDetachedDocumentResult {
    public DocKey Key { get; }
    public Option<DocumentPath> Path { get; }
    public Option<string> Name { get; }
    public SessionPhase Phase { get; }
    public bool ReadOnly { get; }
    public bool Locked { get; }
    public bool UndoEnabled { get; }
    public bool UndoRecordActive { get; }
    public bool Undoing { get; }
    public bool Redoing { get; }
    public bool Headless { get; }
    public bool Modified { get; }
    public bool Pointing { get; }
    public int CommandDepth { get; }
    public Option<Guid> ActiveCommand { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref DocKey key,
        ref Option<DocumentPath> path,
        ref Option<string> name,
        ref SessionPhase phase,
        ref bool readOnly,
        ref bool locked,
        ref bool undoEnabled,
        ref bool undoRecordActive,
        ref bool undoing,
        ref bool redoing,
        ref bool headless,
        ref bool modified,
        ref bool pointing,
        ref int commandDepth,
        ref Option<Guid> activeCommand) =>
        validationError = key == default
            || path.Exists(static value => value == default)
            || name.Exists(static value => string.IsNullOrWhiteSpace(value: value))
            || phase is null
            || undoRecordActive && !undoEnabled
            || commandDepth < 0
            ? new ValidationError(message: "Document snapshot evidence is invalid.")
            : null;

    internal static Fin<SessionSnapshot> Of(RhinoDoc document, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(document).ToFin(Fail: op.MissingContext())
               from identity in DocKey.Of(document: active, key: op)
               from path in Optional(active.Path)
                   .Filter(static value => !string.IsNullOrWhiteSpace(value: value))
                   .Traverse(value => DocumentPath.Of(value: value, key: op))
                   .As()
               from snapshot in op.Catch(() => Admission.Admitted(
                   fault: Validate(
                       identity,
                       path,
                       Optional(active.Name).Filter(static value => !string.IsNullOrWhiteSpace(value: value)),
                       SessionPhase.Of(document: active),
                       active.IsReadOnly,
                       active.IsLocked,
                       active.UndoRecordingEnabled,
                       active.UndoRecordingIsActive,
                       active.UndoActive,
                       active.RedoActive,
                       active.IsHeadless,
                       active.Modified,
                       active.InGetPoint,
                       active.InCommand(bIgnoreScriptRunnerCommands: false),
                       Optional(active.ActiveCommandId).Filter(static id => id != Guid.Empty),
                       out SessionSnapshot? admitted),
                   value: admitted,
                   refusal: op.InvalidResult()))
               select snapshot;
    }
}

[SmartEnum<int>]
public sealed partial class WorksessionCustody {
    public static readonly WorksessionCustody Active = new(key: 0);
    public static readonly WorksessionCustody Reference = new(key: 1);
}

[ComplexValueObject]
public sealed partial class WorksessionModel {
    public DocumentPath Path { get; }
    public WorksessionCustody Custody { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref DocumentPath path,
        ref WorksessionCustody custody) =>
        validationError = string.IsNullOrWhiteSpace(value: path.Value) || custody is null
            ? new ValidationError(message: "Worksession model identity is incomplete.")
            : null;

    internal static Fin<WorksessionModel> Of(DocumentPath path, WorksessionCustody custody, Op key) =>
        Admission.Admitted(fault: Validate(path, custody, out WorksessionModel? admitted), value: admitted, refusal: key.InvalidInput());
}

[ComplexValueObject]
public sealed partial class WorksessionSnapshot : IDetachedDocumentResult {
    public DocKey Document { get; }
    public Option<uint> Serial { get; }
    public Option<DocumentPath> Definition { get; }
    public Option<string> Name { get; }
    public Seq<WorksessionModel> Models { get; }
    public int ReportedCount { get; }
    public bool UnsavedActive { get; }
    public HashMap<uint, DocumentPath> Resolved { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref DocKey document,
        ref Option<uint> serial,
        ref Option<DocumentPath> definition,
        ref Option<string> name,
        ref Seq<WorksessionModel> models,
        ref int reportedCount,
        ref bool unsavedActive,
        ref HashMap<uint, DocumentPath> resolved) {
        Seq<WorksessionModel> admittedModels = models.Choose(static model => Optional(model)).Strict();
        int difference = reportedCount - admittedModels.Count;
        int activeCount = admittedModels.Filter(static model => model.Custody == WorksessionCustody.Active).Count;
        HashSet<DocumentPath> roster = admittedModels.Map(static model => model.Path).ToHashSet();
        validationError = document == default
            || serial.Exists(static value => value is 0u)
            || definition.Exists(static value => value == default)
            || name.Exists(static value => string.IsNullOrWhiteSpace(value: value))
            || admittedModels.Count != models.Count
            || admittedModels.DistinctBy(static model => model.Path).Count != admittedModels.Count
            || activeCount != (unsavedActive ? 0 : 1)
            || reportedCount <= 0
            || difference is < 0 or > 1
            || unsavedActive != (difference is 1)
            || resolved.Keys.Exists(static value => value is 0u)
            || resolved.Values.Distinct().Count != resolved.Count
            || resolved.Values.Exists(path => !roster.Contains(key: path))
            ? new ValidationError(message: "Worksession model census is inconsistent.")
            : null;
    }

    public bool Member(DocumentPath path) => Models.Exists(model => model.Path == path);

    public static Fin<Option<DocumentPath>> FileOf(uint runtimeSerial, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in guard(runtimeSerial > 0u, op.InvalidInput()).ToFin()
               from resolved in op.Catch(() => Optional(global::Rhino.DocObjects.Worksession.FileNameFromRuntimeSerialNumber(
                       runtimeSerialNumber: runtimeSerial))
                   .Filter(static value => !string.IsNullOrWhiteSpace(value: value))
                   .Traverse(value => DocumentPath.Of(value: value, key: op))
                   .As())
               select resolved;
    }

    internal static Fin<WorksessionSnapshot> Of(RhinoDoc document, Op key, Seq<uint> modelSerials) =>
        from owner in Optional(document).ToFin(Fail: key.MissingContext())
        from identity in DocKey.Of(document: owner, key: key)
        from active in Optional(owner.Path)
            .Filter(static value => !string.IsNullOrWhiteSpace(value: value))
            .Traverse(value => DocumentPath.Of(value: value, key: key))
            .As()
        from worksession in Optional(owner.Worksession).ToFin(Fail: key.InvalidResult())
        from modelPaths in Optional(worksession.ModelPaths).ToFin(Fail: key.InvalidResult())
        from paths in toSeq(modelPaths)
            .Traverse(value => DocumentPath.Of(value: value, key: key).ToValidation())
            .As()
            .ToFin()
        from definition in Optional(worksession.FileName)
            .Filter(static value => !string.IsNullOrWhiteSpace(value: value))
            .Traverse(value => DocumentPath.Of(value: value, key: key))
            .As()
        from models in paths
            .Traverse(path => WorksessionModel.Of(
                path: path,
                custody: active.Exists(candidate => candidate == path)
                    ? WorksessionCustody.Active
                    : WorksessionCustody.Reference,
                key: key).ToValidation())
            .As()
            .ToFin()
        from requested in modelSerials
            .Traverse(serial => (guard(serial > 0u, key.InvalidInput()).ToFin().Map(_ => serial)).ToValidation())
            .As()
            .ToFin()
        from resolved in Resolve(
            worksession: worksession,
            requested: requested,
            roster: models.Map(static model => model.Path).ToHashSet(),
            key: key)
        from snapshot in Admission.Admitted(
            fault: Validate(
                identity,
                Optional(worksession.RuntimeSerialNumber).Filter(static serial => serial > 0u),
                definition,
                Optional(worksession.Name).Filter(static value => !string.IsNullOrWhiteSpace(value: value)),
                models,
                worksession.ModelCount,
                active.IsNone,
                resolved,
                out WorksessionSnapshot? admitted),
            value: admitted,
            refusal: key.InvalidResult())
        select snapshot;

    private static Fin<HashMap<uint, DocumentPath>> Resolve(
        global::Rhino.DocObjects.Worksession worksession,
        Seq<uint> requested,
        HashSet<DocumentPath> roster,
        Op key) =>
        from unique in guard(requested.Distinct().Count == requested.Count, key.InvalidInput())
            .ToFin()
            .Map(_ => requested)
        from rows in unique
            .Traverse(serial => key.Catch(() => Optional(worksession.ModelPathFromSerialNumber(modelSerialNumber: serial))
                    .Filter(static value => !string.IsNullOrWhiteSpace(value: value))
                    .ToFin(Fail: key.MissingContext())
                    .Bind(value => DocumentPath.Of(value: value, key: key))
                    .Map(path => (Serial: serial, Path: path)))
                .ToValidation())
            .As()
            .ToFin()
        from _ in (
                guard(rows.Count == unique.Count, key.InvalidResult()).ToFin().ToValidation(),
                guard(rows.Map(static row => row.Serial).Distinct().Count == unique.Count, key.InvalidResult())
                    .ToFin()
                    .ToValidation(),
                guard(unique.ForAll(serial => rows.Exists(row => row.Serial == serial)), key.InvalidResult())
                    .ToFin()
                    .ToValidation(),
                guard(rows.ForAll(row => roster.Contains(key: row.Path)), key.InvalidResult())
                    .ToFin()
                    .ToValidation())
            .Apply(static (_, _, _, _) => unit)
            .As()
            .ToFin()
        select rows.ToHashMap();
}

[SmartEnum<string>]
public sealed partial class WorksessionVerb {
    public static readonly WorksessionVerb Attach = new(
        "attach",
        scriptToken: "_Attach",
        requiresMember: false,
        leavesMember: true,
        inverse: static () => Detach);
    public static readonly WorksessionVerb Detach = new(
        "detach",
        scriptToken: "_Detach",
        requiresMember: true,
        leavesMember: false,
        inverse: static () => Attach);

    internal string ScriptToken { get; }
    internal bool RequiresMember { get; }
    internal bool LeavesMember { get; }

    [UseDelegateFromConstructor]
    internal partial WorksessionVerb Inverse();

    internal string Script(DocumentPath model) =>
        string.Create(CultureInfo.InvariantCulture, $"_-Worksession {ScriptToken} \"{model.Value}\" _Enter");
}

public sealed record WorksessionOp {
    private WorksessionOp(DocumentPath model, Seq<WorksessionVerb> verbs) => (Model, Verbs) = (model, verbs);

    public DocumentPath Model { get; }
    internal Seq<WorksessionVerb> Verbs { get; }

    public static Fin<WorksessionOp> Attach(DocumentPath model) => Scriptable(model: model, verbs: Seq(WorksessionVerb.Attach));

    public static Fin<WorksessionOp> Detach(DocumentPath model) => Scriptable(model: model, verbs: Seq(WorksessionVerb.Detach));

    public static Fin<WorksessionOp> Reload(DocumentPath model) => Scriptable(
        model: model,
        verbs: Seq(WorksessionVerb.Detach, WorksessionVerb.Attach));

    private static Fin<WorksessionOp> Scriptable(DocumentPath model, Seq<WorksessionVerb> verbs) {
        Op op = Op.Of();
        return from admitted in guard(model != default, op.InvalidInput()).ToFin()
               from safe in guard(model.Value.IndexOfAny(['\r', '\n', '"']) < 0, op.InvalidInput()).ToFin()
               from program in verbs
                   .Traverse(verb => Optional(verb).ToFin(Fail: op.InvalidInput()).ToValidation())
                   .As()
                   .ToFin()
               from nonempty in guard(!program.IsEmpty, op.InvalidInput()).ToFin()
               select new WorksessionOp(model: model, verbs: program.Strict());
    }
}

public sealed record WorksessionReceipt(
    WorksessionOp Operation,
    WorksessionSnapshot Before,
    WorksessionSnapshot After) : IDetachedDocumentResult;

public static class SessionWorksession {
    extension(DocumentSession session) {
        public Fin<WorksessionSnapshot> Worksession(Op? key = null, params ReadOnlySpan<uint> modelSerials) {
            Op op = key.OrDefault();
            Seq<uint> serials = toSeq(modelSerials.ToArray());
            return Optional(session).ToFin(Fail: op.InvalidInput()).Bind(scope => scope.Demand(
                use: document => WorksessionSnapshot.Of(document: document, key: op, modelSerials: serials),
                key: op,
                needs: [SessionNeed.Read]));
        }

        public Fin<WorksessionReceipt> Worksession(WorksessionOp change, Op? key = null) {
            Op op = key.OrDefault();
            return from scope in Optional(session).ToFin(Fail: op.InvalidInput())
                   from request in Optional(change).ToFin(Fail: op.InvalidInput())
                   from receipt in scope.Demand(
                       use: document =>
                           from before in WorksessionSnapshot.Of(document: document, key: op, modelSerials: Seq<uint>())
                           from completed in request.Verbs.Fold(
                               Fin.Succ(value: Seq<WorksessionVerb>()),
                               (rail, verb) => rail.Bind(done => Apply(
                                       document: document,
                                       model: request.Model,
                                       verb: verb,
                                       op: op)
                                   .Map(_ => done.Add(value: verb))
                                   .MapFail(primary => Restore(
                                       document: document,
                                       model: request.Model,
                                       completed: done,
                                       op: op).Match(
                                           Succ: _ => primary,
                                           Fail: cleanup => primary + cleanup))))
                           from after in WorksessionSnapshot.Of(document: document, key: op, modelSerials: Seq<uint>())
                               .MapFail(primary => Restore(
                                   document: document,
                                   model: request.Model,
                                   completed: completed,
                                   op: op).Match(
                                       Succ: _ => primary,
                                       Fail: cleanup => primary + cleanup))
                           select new WorksessionReceipt(Operation: request, Before: before, After: after),
                       key: op,
                       needs: [SessionNeed.Read, SessionNeed.Mutate, SessionNeed.Acquire, SessionNeed.Interrupt])
                   select receipt;
        }
    }

    private static Fin<Unit> Apply(RhinoDoc document, DocumentPath model, WorksessionVerb verb, Op op) =>
        from current in WorksessionSnapshot.Of(document: document, key: op, modelSerials: Seq<uint>())
        from admitted in guard(current.Member(path: model) == verb.RequiresMember, op.InvalidInput()).ToFin()
        from landed in Run(document: document, model: model, verb: verb, op: op).BiBind(
            Succ: _ => WorksessionSnapshot.Of(document: document, key: op, modelSerials: Seq<uint>())
                .Bind(proof => guard(proof.Member(path: model) == verb.LeavesMember, op.InvalidResult()).ToFin())
                .BindFail(primary => Compensate(
                    document: document,
                    model: model,
                    verb: verb,
                    primary: primary,
                    op: op)),
            Fail: primary => Compensate(
                document: document,
                model: model,
                verb: verb,
                primary: primary,
                op: op))
        select unit;

    private static Fin<Unit> Compensate(
        RhinoDoc document,
        DocumentPath model,
        WorksessionVerb verb,
        Error primary,
        Op op) => Restore(document: document, model: model, verb: verb, op: op).BiBind(
            Succ: _ => Fin.Fail<Unit>(error: primary),
            Fail: cleanup => Fin.Fail<Unit>(error: primary + cleanup));

    private static Fin<Unit> Restore(
        RhinoDoc document,
        DocumentPath model,
        Seq<WorksessionVerb> completed,
        Op op) => completed.Rev()
        .Traverse(verb => Restore(document: document, model: model, verb: verb, op: op).ToValidation())
        .As()
        .ToFin()
        .Map(static _ => unit);

    private static Fin<Unit> Restore(RhinoDoc document, DocumentPath model, WorksessionVerb verb, Op op) {
        WorksessionVerb inverse = verb.Inverse();
        return from current in WorksessionSnapshot.Of(document: document, key: op, modelSerials: Seq<uint>())
               from restored in current.Member(path: model) == inverse.LeavesMember
                   ? Fin.Succ(value: unit)
                   : from admitted in guard(current.Member(path: model) == inverse.RequiresMember, op.InvalidResult()).ToFin()
                     from run in Run(document: document, model: model, verb: inverse, op: op)
                     from proof in WorksessionSnapshot.Of(document: document, key: op, modelSerials: Seq<uint>())
                     from landed in guard(proof.Member(path: model) == inverse.LeavesMember, op.InvalidResult()).ToFin()
                     select unit
               select restored;
    }

    private static Fin<Unit> Run(RhinoDoc document, DocumentPath model, WorksessionVerb verb, Op op) =>
        op.Catch(() => op.Confirm(success: RhinoApp.RunScript(
            documentSerialNumber: document.RuntimeSerialNumber,
            script: verb.Script(model: model),
            echo: false)));
}
```

## [03]-[CAPABILITY]

- Owner: `SessionMode` re-closes the foreign `RunMode` enum, adds the package-owned `Headless` lane, and carries independent live-host and native-dialog columns. `SessionNeed` is a keyless behavior vocabulary: each row separates mode admission from snapshot admission, so deterministic refusals run before acquisition and host state remains recoverable from the declaration.
- Entry: `SessionMode.OfRunMode` returns `Fin<SessionMode>` and rejects an unknown foreign ordinal. `DocumentSession.Of` rejects an empty or mode-incompatible capability set before acquisition; `DocumentSession.Demand` proves one or more granted rows against one fresh snapshot around the consuming host body.
- Law: `Observe` admits transition snapshots, while operation capabilities require `Ready`. Mutation excludes read-only documents and undo/redo replay; undo-bearing mutation additionally requires enabled undo recording. Redraw requires a live view pipeline; acquisition admits interactive and scripted command lanes; native dialogs admit only the interactive lane; interrupt remains admissible while a point acquisition is active.
- Law: `IsLocked` is detached file-write ownership evidence, not a mutation refusal; Rhino reports it when the current document owns the file lock.
- Law: `SessionNeed.Mutation(undo, redraw)` is the one mutation-need derivation — `Mutate` plus conditional `Undo` plus conditional `Redraw` — every commit rail passes to `Demand`; a per-rail inline `Seq(Mutate) + ...` re-derivation is the deleted form.
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
            && snapshot.UndoEnabled
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

    internal static Seq<SessionNeed> Mutation(bool undo, RedrawPolicy redraw) =>
        Seq(Mutate)
        + (undo ? Seq(Undo) : Seq<SessionNeed>())
        + (redraw.Enabled ? Seq(Redraw) : Seq<SessionNeed>());
}
```

## [04]-[SOURCE_AND_SESSION]

- Owner: `DocumentPath` admits absolute nonblank path text once, while `DocumentFile` carries existing-file versus existing-`.3dm` resolution as behavior rows. `SessionSource` is one flat `[Union]` over borrowed live/active/keyed/opened documents and owned empty/template/archive/configured headless documents; source depth is case data, so admission performs one generated dispatch, and the ambient `RhinoDoc.ActiveDoc` static crosses the boundary only through the `Active` case.
- Entry: `SessionSource.Acquire` returns `Fin<Lease<RhinoDoc>>`; every deterministic source/mode refusal and file requirement completes before its host call, and the `Configured` case carries a typed `ArchiveMap` (Persistence/dictionary.md) minted into the native option payload inside the acquire arm, so the host receives a fresh dictionary no caller can mutate. `DocumentSession.Of` rejects empty, duplicate, or mode-incompatible capabilities, acquires, snapshots, checks lane/document agreement, validates the kernel context, and only then adopts the lease.
- Law: a failed admission releases an owned lease before returning its original fault. A successful admission never calls `Lease.Use`, because `Use` closes `Owned` when its projection returns; the session retains the lease and releases it exactly once through `Dispose`.
- Law: `Demand<TResult>` is the sole capability surface. It validates a nonempty unique capability set and the consumer, re-resolves `Key`, proves every granted row against one fresh snapshot, and executes the host body through `Op.Catch`; its `IDetachedDocumentResult` bound rejects a raw `RhinoDoc` result at compile time on every demanding path, and the kernel `Context` crosses through the private detached `DetachedContext` capsule rather than an unconstrained core.
- Law: `Demand` is re-entrant on the demanding thread — the reentrant `Lock` plus the demand-depth counter admit a nested demand inside a running host body, and each nesting proves its own grants against its own fresh snapshot. `Dispose` issued during any demand defers to the outermost demand's exit, and a pending disposal refuses every new demand, nested included.
- Law: `Context()` re-enters `Context.Of(RhinoDoc)` on every call, so model-unit and tolerance changes cannot stale the context consumed by later geometry work.
- Law: `Admission.All` and `Admission.Pair` own reference-argument admission — one span fold and one applicative pair composed by every Document spine, so the Optional-traverse spelling appears once.
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

    public static Fin<DocumentPath> Of(string value, Op? key = null) =>
        key.OrDefault().AcceptValidated<DocumentPath>(candidate: value);

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
public sealed class DocumentSession : IDisposable, IDetachedDocumentResult {
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
        return from admission in Admission.Pair(first: source, second: mode, key: op)
               from demanded in AdmitNeeds(needs: needs, mode: admission.Second, op: op)
               from session in OnHost(
                   mode: admission.Second,
                   use: () =>
                       from acquired in admission.First.Acquire(mode: admission.Second, key: op)
                       from adopted in Adopt(
                           acquired: acquired,
                           lane: admission.Second,
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
        where TResult : IDetachedDocumentResult {
        Op op = key.OrDefault();
        return from admission in (
                   Optional(use).ToFin(Fail: op.InvalidInput()).ToValidation(),
                   AdmitNeeds(needs: needs, mode: Mode, op: op).ToValidation())
                   .Apply(static (body, requested) => (Body: body, Requested: requested))
                   .As()
                   .ToFin()
               from result in OnHost(
                   mode: Mode,
                   use: () => DemandLocked(
                       use: admission.Body,
                       requested: admission.Requested,
                       op: op),
                   op: op)
               select result;
    }

    public Fin<Context> Context(Op? key = null) {
        Op op = key.OrDefault();
        return Demand(
            use: document => Rasm.Domain.Context.Of(doc: document)
                .ToFin()
                .Map(static value => new DetachedContext(Value: value)),
            key: op,
            needs: [SessionNeed.Read])
            .Map(static detached => detached.Value);
    }

    public Fin<SessionSnapshot> Interrupt(Op? key = null) {
        Op op = key.OrDefault();
        return Demand(
            use: document => {
                document.TimeoutActiveGet();
                return SessionSnapshot.Of(document: document, key: op);
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

    private Fin<TResult> DemandLocked<TResult>(
        Func<RhinoDoc, Fin<TResult>> use,
        LanguageExt.HashSet<SessionNeed> requested,
        Op op)
        where TResult : IDetachedDocumentResult {
        lock (gate) {
            if (released || disposePending) {
                return Fin.Fail<TResult>(error: op.MissingContext());
            }

            demandDepth += 1;
            Fin<TResult> result;
            try {
                result = op.Catch(() =>
                    from grants in Proven(needs: requested, admits: granted.Contains, refusal: op.MissingContext())
                    from document in Key.Resolve(key: op)
                    from snapshot in SessionSnapshot.Of(document: document, key: op)
                    from capabilities in Proven(
                        needs: requested,
                        admits: need => need.Admits(snapshot: snapshot, mode: Mode),
                        refusal: op.MissingContext())
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
            .Traverse(need => Optional(need).ToFin(Fail: op.InvalidInput()).ToValidation())
            .As()
            .ToFin()
        from nonempty in guard(flag: !demanded.IsEmpty, False: op.InvalidInput()).ToFin()
        let distinct = toHashSet(demanded)
        from unique in guard(flag: distinct.Count == demanded.Count, False: op.InvalidInput()).ToFin()
        from modeAdmitted in Proven(needs: distinct, admits: need => need.AdmitsMode(mode: mode), refusal: op.InvalidInput())
        select distinct;

    private static Fin<Unit> Proven(LanguageExt.HashSet<SessionNeed> needs, Func<SessionNeed, bool> admits, Error refusal) =>
        needs.AsIterable()
            .Traverse(need => (admits(need) ? Fin.Succ(value: need) : Fin.Fail<SessionNeed>(error: refusal)).ToValidation())
            .As()
            .ToFin()
            .Map(static _ => unit);

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
            from capabilities in Proven(
                needs: granted,
                admits: need => need.Admits(snapshot: snapshot, mode: lane),
                refusal: op.MissingContext())
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

    private sealed record DetachedContext(Rasm.Domain.Context Value) : IDetachedDocumentResult;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class Admission {
    internal static Fin<T> Admitted<T>(ValidationError? fault, T? value, Error refusal) where T : class =>
        fault is null && value is not null ? Fin.Succ(value: value) : Fin.Fail<T>(error: refusal);

    internal static Fin<Seq<T>> All<T>(ReadOnlySpan<T> values, Op key) =>
        toSeq(values.ToArray())
            .Traverse(value => Optional(value).ToFin(Fail: key.InvalidInput()).ToValidation())
            .As()
            .ToFin();

    internal static Fin<(T1 First, T2 Second)> Pair<T1, T2>(T1 first, T2 second, Op key)
        where T1 : class where T2 : class =>
        (
            Optional(first).ToFin(Fail: key.InvalidInput()).ToValidation(),
            Optional(second).ToFin(Fail: key.InvalidInput()).ToValidation())
        .Apply(static (one, two) => (First: one, Second: two))
        .As()
        .ToFin();
}
```

## [05]-[REGIME]

- Owner: `DocumentSpace` carries the model/page host-member axis as read, tolerance-write, and precision-write behavior columns. Kernel `ModelUnit` owns unit admission and equality; the native `LengthUnit` survives only beside the host call that reads or writes it. Closed `RegimeChange` cases expose one overloaded `Of` admission family and one mutation dispatch.
- Entry: `session.Regime(space)` reads a validated `UnitRegime`. `session.Adjust(space, change)` captures the before regime, seals one undo record around the change, proves the exact requested postcondition, and returns `RegimeReceipt` with the sealed serial.
- Law: `UnitScaling` replaces the host `bool scale` knob with `PreserveCoordinates` and `PreservePhysicalSize`. Known, custom, and native `LengthUnit` inputs all produce the private-constructed `RegimeChange.Units` case, so `AdjustLengthUnits` is the sole unit-system mutation path.
- Law: tolerance values compose the kernel `AbsoluteTolerance`, `RelativeTolerance`, and `AngleTolerance` owners. This page carries no independent numeric admission or radians/degrees duplication.
- Law: unit postconditions compare canonical `ModelUnit` evidence, including custom name and meters-per-unit scale. `UnitRegime` retains the native `LengthUnit` only for compensation.
- Boundary: the row delegates contain the property-set statement seam required by the host API. A failed write restores every scalar without assuming a failed unit call changed geometry; a proven unit write followed by a failed postcondition reverses the unit scaling and restores every scalar. Compensation faults accumulate and join the original fault, and the shared `DocumentCommit` envelope seals or rolls back the enclosing record under `RedrawPolicy.None`.

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
        internal Units(LengthUnit native, ModelUnit unit, UnitScaling scaling) =>
            (Native, Unit, Scaling) = (native, unit, scaling);

        internal LengthUnit Native { get; }
        public ModelUnit Unit { get; }
        public UnitScaling Scaling { get; }
    }

    public sealed record Tolerances(ToleranceRegime Value) : RegimeChange;

    public sealed record Precision(DisplayPrecision Value) : RegimeChange;

    public static Fin<RegimeChange> Of(
        UnitSystem system,
        UnitScaling scaling,
        Op? key = null) {
        Op op = key.OrDefault();
        return from admission in (
                   Optional(scaling).ToFin(Fail: op.InvalidInput()).ToValidation(),
                   ModelUnit.Of(value: system, key: op).ToValidation())
                   .Apply(static (policy, unit) => (Policy: policy, Unit: unit))
                   .As()
                   .ToFin()
               from native in op.Catch(() => Fin.Succ(value: LengthUnit.FromKnownUnitSystem(
                   knownUnitSystem: admission.Unit.System)))
               select (RegimeChange)new Units(
                   native: native,
                   unit: admission.Unit,
                   scaling: admission.Policy);
    }

    public static Fin<RegimeChange> Of(
        string name,
        double metersPerUnit,
        UnitScaling scaling,
        Op? key = null) {
        Op op = key.OrDefault();
        return from admission in (
                   op.AcceptText(value: name).ToValidation(),
                   op.Positive(value: metersPerUnit).ToValidation())
                   .Apply(static (label, scale) => (Label: label, Scale: scale))
                   .As()
                   .ToFin()
               from unitValue in op.Catch(() => Fin.Succ(value: LengthUnit.FromCustomUnitSystem(
                   name: admission.Label,
                   customUnitSize: admission.Scale,
                   knownUnitSystem: UnitSystem.Meters)))
               from change in Of(units: unitValue, scaling: scaling, key: op)
               select change;
    }

    public static Fin<RegimeChange> Of(
        LengthUnit units,
        UnitScaling scaling,
        Op? key = null) {
        Op op = key.OrDefault();
        return (
                Optional(scaling).ToFin(Fail: op.InvalidInput()).ToValidation(),
                ModelUnit.Of(value: units, key: op).ToValidation())
            .Apply((policy, admitted) => (RegimeChange)new Units(
                native: units,
                unit: admitted,
                scaling: policy))
            .As()
            .ToFin();
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
        return mutation.BindFail(error => Restore<Unit>(
            document: document,
            space: space,
            before: before,
            scaling: None,
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
        Restore<TResult>(
            document: document,
            space: space,
            before: before,
            scaling: Switch(
                units: static change => Some(change.Scaling),
                tolerances: static _ => None,
                precision: static _ => None),
            failure: failure,
            op: op);

    private static Fin<TResult> Restore<TResult>(
        RhinoDoc document,
        DocumentSpace space,
        UnitRegime before,
        Option<UnitScaling> scaling,
        Error failure,
        Op op) {
        K<Validation<Error>, Unit> units = scaling.Match(
            Some: policy => op.Catch(() => op.Confirm(
                success: document.AdjustLengthUnits(
                    modelUnits: space.ModelUnits,
                    units: before.Native,
                    scale: policy.HostScale))),
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
            .ToFin()
            .BiBind(
                Succ: _ => Fin.Fail<TResult>(error: failure),
                Fail: cleanup => Fin.Fail<TResult>(error: failure + cleanup));
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
        return (
                op.AcceptValidated<AbsoluteTolerance>(candidate: absolute).ToValidation(),
                op.AcceptValidated<RelativeTolerance>(candidate: relative).ToValidation(),
                op.AcceptValidated<AngleTolerance>(candidate: angle).ToValidation())
            .Apply(static (admittedAbsolute, admittedRelative, admittedAngle) => new ToleranceRegime(
                Absolute: admittedAbsolute,
                Relative: admittedRelative,
                Angle: admittedAngle))
            .As()
            .ToFin();
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
        (
            ModelUnit.Of(value: units, key: op).ToValidation(),
            ToleranceRegime.Of(
                absolute: absolute,
                relative: relative,
                angle: angle,
                key: op).ToValidation(),
            op.AcceptValidated<DisplayPrecision>(candidate: precision).ToValidation())
        .Apply((admittedUnit, admittedTolerances, admittedPrecision) => new UnitRegime(
            native: units,
            unit: admittedUnit,
            tolerances: admittedTolerances,
            precision: admittedPrecision))
        .As()
        .ToFin();
}

public sealed record RegimeReceipt : IDetachedDocumentResult {
    private RegimeReceipt(
        DocumentSpace space,
        RegimeChange change,
        UnitRegime before,
        UnitRegime after,
        Option<uint> undoRecord) =>
        (Space, Change, Before, After, UndoRecord) = (space, change, before, after, undoRecord);

    public DocumentSpace Space { get; }
    public RegimeChange Change { get; }
    public UnitRegime Before { get; }
    public UnitRegime After { get; }
    public Option<uint> UndoRecord { get; private init; }

    internal static RegimeReceipt Pending(
        DocumentSpace space,
        RegimeChange change,
        UnitRegime before,
        UnitRegime after) =>
        new(space: space, change: change, before: before, after: after, undoRecord: None);

    internal RegimeReceipt Seal(uint serial) => this with { UndoRecord = Some(serial) };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class SessionRegimes {
    extension(DocumentSession session) {
        public Fin<UnitRegime> Regime(DocumentSpace space, Op? key = null) {
            Op op = key.OrDefault();
            return from admission in Admission.Pair(first: session, second: space, key: op)
                   from regime in admission.First.Demand(
                       use: document => admission.Second.Read(document: document, op: op),
                       key: op,
                       needs: [SessionNeed.Read])
                   select regime;
        }

        public Fin<RegimeReceipt> Adjust(
            DocumentSpace space,
            RegimeChange change,
            Op? key = null) {
            Op op = key.OrDefault();
            return from admission in (
                       Optional(session).ToFin(Fail: op.InvalidInput()).ToValidation(),
                       Optional(space).ToFin(Fail: op.InvalidInput()).ToValidation(),
                       Optional(change).ToFin(Fail: op.InvalidInput()).ToValidation())
                       .Apply(static (scope, axis, request) => (
                           Scope: scope,
                           Axis: axis,
                           Request: request))
                       .As()
                       .ToFin()
                   from receipt in admission.Scope.Demand(
                       use: document => DocumentCommit.Sealed(
                           document: document,
                           name: nameof(Adjust),
                           recordsUndo: true,
                           redraw: RedrawPolicy.None,
                           run: () =>
                               from before in admission.Axis.Read(document: document, op: op)
                               from applied in admission.Request.Apply(
                                   document: document,
                                   space: admission.Axis,
                                   before: before,
                                   op: op)
                               from after in (
                                   from observed in admission.Axis.Read(document: document, op: op)
                                   from matches in op.Catch(() => Fin.Succ(value: admission.Request.Matches(actual: observed)))
                                   from exact in guard(flag: matches, False: op.InvalidResult()).ToFin()
                                   select observed).BindFail(error => admission.Request.Rollback<UnitRegime>(
                                   document: document,
                                   space: admission.Axis,
                                   before: before,
                                   failure: error,
                                   op: op))
                               select RegimeReceipt.Pending(
                                   space: admission.Axis,
                                   change: admission.Request,
                                   before: before,
                                   after: after),
                           stamp: static (result, serial) => result.Seal(serial: serial),
                           op: op),
                       key: op,
                       needs: [SessionNeed.Mutate, SessionNeed.Undo])
                   select receipt;
        }
    }
}
```

## [06]-[REGIME_TEXT]

- Owner: `UnitDialect` carries every catalogued host parser preset as a behavior row; `AngleGrammar` rows own bare-number angle interpretation and land canonical radians. `UnitForm` and `UnitNotation` re-close foreign formatting ordinals. `UnitText` `[Union]` carries encoded length/scale/angle text, detached semantic evidence, display/name projections, and rendered text on one correspondence owner. `UnitLabel` generates every case, number, and width combination from three independent policy axes.
- Entry: `session.Text(space, text)` resolves the live regime and crosses one `UnitText` case. Length text crosses to semantic evidence and the same returned carrier crosses back to exact text; scale and angle values return typed unsupported faults where the catalog exposes no verified inverse formatter.
- Law: dialect rows return the host preset statics — process-shared constants whose `Dispose` is inert — and never mutate one: a preset setter writes through to the shared native object and poisons every later parse, and `new StringParserSettings()` seeds a different ambiguous grammar, so neither is a dialect base.
- Law: a length parse admits only a whole-string parse of a set value — `parsedAll` and `!IsUnset()` gate together — and converts on egress through `Length(LengthUnit)` into regime units. Reverse crossing scales the detached value from its captured `ModelUnit` into the current regime before formatting and preserves its admitted dialect. A scale parse gates the scale and both terms before detaching each magnitude and `LengthUnit`, so unitless, custom, mixed-unit, and same-unit ratios remain distinguishable.
- Law: an angle phrase parses through `StringParser.ParseAngleExpressionRadians` or `ParseAngleExpressionDegrees` under its `AngleGrammar` row; the degrees row converts through `RhinoMath.ToRadians` at the seam, so `UnitText.AngleValueCase` always carries canonical radians and no consumer re-derives the angular unit.
- Law: every parsed `LengthValue`/`ScaleValue` is a native disposable bracketed inside its arm; `UnitText` leaves as detached evidence or text, never as a live parse handle. Exact length crossing round-trips through `LengthValue.LengthString`; display suffixes use `UnitSuffix`, and `UnitSystemName` receives the generated `UnitLabel` policy rather than boolean knobs.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class UnitDialect {
    public static readonly UnitDialect Standard = new(key: 0, settings: static () => StringParserSettings.DefaultParseSettings);
    public static readonly UnitDialect Integers = new(key: 1, settings: static () => StringParserSettings.ParseSettingsIntegerNumber);
    public static readonly UnitDialect Rationals = new(key: 2, settings: static () => StringParserSettings.ParseSettingsRationalNumber);
    public static readonly UnitDialect Doubles = new(key: 3, settings: static () => StringParserSettings.ParseSettingsDoubleNumber);
    public static readonly UnitDialect Reals = new(key: 4, settings: static () => StringParserSettings.ParseSettingsRealNumber);
    public static readonly UnitDialect Radians = new(key: 5, settings: static () => StringParserSettings.ParseSettingsRadians);
    public static readonly UnitDialect Degrees = new(key: 6, settings: static () => StringParserSettings.ParseSettingsDegrees);
    public static readonly UnitDialect Empty = new(key: 7, settings: static () => StringParserSettings.ParseSettingsEmpty);

    [UseDelegateFromConstructor]
    internal partial StringParserSettings Settings();
}

[SmartEnum<int>]
public sealed partial class AngleGrammar {
    public static readonly AngleGrammar Radians = new(key: 0, parse: static (text, op) =>
        StringParser.ParseAngleExpressionRadians(text, out double value)
            && double.IsFinite(value)
            ? Fin.Succ(value: value)
            : Fin.Fail<double>(error: op.InvalidInput()));
    public static readonly AngleGrammar Degrees = new(key: 1, parse: static (text, op) =>
        StringParser.ParseAngleExpressionDegrees(text, out double value)
            && RhinoMath.ToRadians(value) is var radians
            && double.IsFinite(radians)
            ? Fin.Succ(value: radians)
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

[SmartEnum<int>]
public sealed partial class UnitSuffix {
    public static readonly UnitSuffix Omit = new(key: 0, append: false);
    public static readonly UnitSuffix Append = new(key: 1, append: true);

    internal bool Append { get; }
}

[SmartEnum<int>]
public sealed partial class UnitLetterCase {
    public static readonly UnitLetterCase Lower = new(key: 0, capitalize: false);
    public static readonly UnitLetterCase Capitalized = new(key: 1, capitalize: true);

    internal bool Capitalize { get; }
}

[SmartEnum<int>]
public sealed partial class UnitNumber {
    public static readonly UnitNumber Plural = new(key: 0, singular: false);
    public static readonly UnitNumber Singular = new(key: 1, singular: true);

    internal bool Singular { get; }
}

[SmartEnum<int>]
public sealed partial class UnitWidth {
    public static readonly UnitWidth Full = new(key: 0, abbreviate: false);
    public static readonly UnitWidth Abbreviated = new(key: 1, abbreviate: true);

    internal bool Abbreviate { get; }
}

[ComplexValueObject]
public sealed partial class UnitLabel {
    public UnitLetterCase LetterCase { get; }
    public UnitNumber Number { get; }
    public UnitWidth Width { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref UnitLetterCase letterCase,
        ref UnitNumber number,
        ref UnitWidth width) =>
        validationError = letterCase is null || number is null || width is null
            ? new ValidationError(message: "Unit label policy is incomplete.")
            : null;

    public static Fin<UnitLabel> Of(
        UnitLetterCase letterCase,
        UnitNumber number,
        UnitWidth width,
        Op? key = null) =>
        Admission.Admitted(
            fault: Validate(letterCase, number, width, out UnitLabel? admitted),
            value: admitted,
            refusal: key.OrDefault().InvalidInput());
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UnitText : IDetachedDocumentResult {
    private UnitText() { }

    public sealed record LengthTextCase : UnitText {
        internal LengthTextCase(string text, UnitDialect dialect, UnitForm form) =>
            (Text, Dialect, Form) = (text, dialect, form);
        public string Text { get; }
        public UnitDialect Dialect { get; }
        public UnitForm Form { get; }
    }

    public sealed record LengthValueCase : UnitText {
        internal LengthValueCase(
            double value,
            ModelUnit unit,
            LengthUnit source,
            UnitDialect dialect,
            UnitForm form) =>
            (Value, Unit, Source, Dialect, Form) = (value, unit, source, dialect, form);
        public double Value { get; }
        public ModelUnit Unit { get; }
        public LengthUnit Source { get; }
        public UnitDialect Dialect { get; }
        public UnitForm Form { get; }
    }

    public sealed record ScaleTextCase : UnitText {
        internal ScaleTextCase(string text, UnitDialect dialect) => (Text, Dialect) = (text, dialect);
        public string Text { get; }
        public UnitDialect Dialect { get; }
    }

    public sealed record ScaleValueCase : UnitText {
        internal ScaleValueCase(
            double left,
            double right,
            LengthUnit leftUnit,
            LengthUnit rightUnit,
            double leftToRight,
            double rightToLeft) =>
            (Left, Right, LeftUnit, RightUnit, LeftToRight, RightToLeft) =
                (left, right, leftUnit, rightUnit, leftToRight, rightToLeft);
        public double Left { get; }
        public double Right { get; }
        public LengthUnit LeftUnit { get; }
        public LengthUnit RightUnit { get; }
        public double LeftToRight { get; }
        public double RightToLeft { get; }
    }

    public sealed record AngleTextCase : UnitText {
        internal AngleTextCase(string text, AngleGrammar grammar) => (Text, Grammar) = (text, grammar);
        public string Text { get; }
        public AngleGrammar Grammar { get; }
    }

    public sealed record AngleValueCase : UnitText {
        internal AngleValueCase(double radians) => Radians = radians;
        public double Radians { get; }
    }

    private sealed record DisplayCase(double Value, UnitNotation Notation, UnitSuffix Suffix) : UnitText;
    private sealed record NameCase(UnitLabel Label) : UnitText;

    public sealed record RenderedCase : UnitText {
        internal RenderedCase(string text) => Text = text;
        public string Text { get; }
    }

    public static Fin<UnitText> Length(
        string text,
        Option<UnitDialect> dialect = default,
        Option<UnitForm> form = default,
        Op? key = null) =>
        key.OrDefault().AcceptText(value: text).Map(admitted => (UnitText)new LengthTextCase(
            text: admitted,
            dialect: dialect.IfNone(UnitDialect.Standard),
            form: form.IfNone(UnitForm.CleanDecimal)));

    public static Fin<UnitText> Scale(string text, Option<UnitDialect> dialect = default, Op? key = null) =>
        key.OrDefault().AcceptText(value: text)
            .Map(admitted => (UnitText)new ScaleTextCase(
                text: admitted,
                dialect: dialect.IfNone(UnitDialect.Standard)));

    public static Fin<UnitText> Angle(string text, Option<AngleGrammar> grammar = default, Op? key = null) =>
        key.OrDefault().AcceptText(value: text)
            .Map(admitted => (UnitText)new AngleTextCase(text: admitted, grammar: grammar.IfNone(AngleGrammar.Radians)));

    public static Fin<UnitText> Display(
        double value,
        UnitSuffix suffix,
        Option<UnitNotation> notation = default,
        Op? key = null) {
        Op op = key.OrDefault();
        return (
                guard(flag: double.IsFinite(value), False: op.InvalidInput()).ToFin().ToValidation(),
                Optional(suffix).ToFin(Fail: op.InvalidInput()).ToValidation())
            .Apply((_, admittedSuffix) => (UnitText)new DisplayCase(
                Value: value,
                Notation: notation.IfNone(UnitNotation.Decimal),
                Suffix: admittedSuffix))
            .As()
            .ToFin();
    }

    public static Fin<UnitText> Name(UnitLabel label, Op? key = null) =>
        Optional(label).ToFin(Fail: key.OrDefault().InvalidInput())
            .Map(static admitted => (UnitText)new NameCase(Label: admitted));

    internal Fin<UnitText> Cross(UnitRegime regime, Op key) => Switch(
        state: (Regime: regime, Op: key),
        lengthTextCase: static (context, text) => context.Op.Catch(() => {
            using LengthValue parsed = LengthValue.Create(
                s: text.Text,
                ps: text.Dialect.Settings(),
                parsedAll: out bool parsedAll);
            return from _whole in guard(
                       flag: parsedAll && !parsed.IsUnset(),
                       False: context.Op.InvalidInput()).ToFin()
                   from value in context.Op.Catch(() => Fin.Succ(value: parsed.Length(units: context.Regime.Native)))
                   from finite in guard(flag: double.IsFinite(value), False: context.Op.InvalidResult()).ToFin()
                   select (UnitText)new LengthValueCase(
                       value: value,
                       unit: context.Regime.Unit,
                       source: parsed.Units,
                       dialect: text.Dialect,
                       form: text.Form);
        }),
        lengthValueCase: static (context, value) =>
            from scale in value.Unit.ScaleTo(target: context.Regime.Unit, key: context.Op)
            let converted = value.Value * scale
            from finite in guard(flag: double.IsFinite(converted), False: context.Op.InvalidResult()).ToFin()
            from rendered in context.Op.Catch(() => {
                using LengthValue formatted = LengthValue.Create(
                    length: converted,
                    units: context.Regime.Native,
                    format: value.Form.Native);
                return guard(flag: !formatted.IsUnset(), False: context.Op.InvalidResult()).ToFin()
                    .Bind(_ => context.Op.AcceptText(value: formatted.LengthString))
                    .Map(text => (UnitText)new LengthTextCase(
                        text: text,
                        dialect: value.Dialect,
                        form: value.Form));
            })
            select rendered,
        scaleTextCase: static (context, text) => context.Op.Catch(() => {
            using ScaleValue parsed = ScaleValue.Create(s: text.Text, ps: text.Dialect.Settings());
            return guard(flag: !parsed.IsUnset(), False: context.Op.InvalidInput()).ToFin().Bind(_ => {
                using LengthValue left = parsed.LeftLengthValue();
                using LengthValue right = parsed.RightLengthValue();
                double leftValue = left.Length();
                double rightValue = right.Length();
                double leftToRight = parsed.LeftToRightScale;
                double rightToLeft = parsed.RightToLeftScale;
                return guard(
                        flag: !left.IsUnset()
                            && !right.IsUnset()
                            && double.IsFinite(leftValue)
                            && double.IsFinite(rightValue)
                            && double.IsFinite(leftToRight)
                            && double.IsFinite(rightToLeft)
                            && leftValue > 0.0
                            && rightValue > 0.0
                            && leftToRight > 0.0
                            && rightToLeft > 0.0,
                        False: context.Op.InvalidResult())
                    .ToFin()
                    .Map(_ => (UnitText)new ScaleValueCase(
                        left: leftValue,
                        right: rightValue,
                        leftUnit: left.Units,
                        rightUnit: right.Units,
                        leftToRight: leftToRight,
                        rightToLeft: rightToLeft));
            });
        }),
        scaleValueCase: static (context, _) => Fin.Fail<UnitText>(error: context.Op.Unsupported(
            geometryType: typeof(ScaleValueCase),
            outputType: typeof(ScaleTextCase))),
        angleTextCase: static (context, text) => text.Grammar.Parse(text: text.Text, op: context.Op)
            .Map(static radians => (UnitText)new AngleValueCase(radians: radians)),
        angleValueCase: static (context, _) => Fin.Fail<UnitText>(error: context.Op.Unsupported(
            geometryType: typeof(AngleValueCase),
            outputType: typeof(AngleTextCase))),
        displayCase: static (context, value) => context.Op.Catch(() => Fin.Succ(
            value: (UnitText)new RenderedCase(text: global::Rhino.UI.Localization.FormatNumber(
                x: value.Value,
                units: context.Regime.Native,
                mode: value.Notation.Native,
                precision: context.Regime.Precision.Value,
                appendUnitSystemName: value.Suffix.Append)))),
        nameCase: static (context, value) => context.Op.Catch(() => Fin.Succ(
            value: (UnitText)new RenderedCase(text: global::Rhino.UI.Localization.UnitSystemName(
                units: context.Regime.Unit.System,
                capitalize: value.Label.LetterCase.Capitalize,
                singular: value.Label.Number.Singular,
                abbreviate: value.Label.Width.Abbreviate)))),
        renderedCase: static (context, _) => Fin.Fail<UnitText>(error: context.Op.Unsupported(
            geometryType: typeof(RenderedCase),
            outputType: typeof(UnitText))));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class RegimeText {
    extension(DocumentSession session) {
        public Fin<UnitText> Text(DocumentSpace space, UnitText text, Op? key = null) {
            Op op = key.OrDefault();
            return from request in Optional(text).ToFin(Fail: op.InvalidInput())
                   from regime in session.Regime(space: space, key: op)
                   from crossed in request.Cross(regime: regime, key: op)
                   select crossed;
        }
    }
}
```

## [07]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]            | [OWNER]               | [FORM]                            | [ENTRY]                      |
| :-----: | :------------------- | :-------------------- | :-------------------------------- | :--------------------------- |
|  [01]   | document identity    | `DocKey`              | positive generated value          | `Of` / `Census`              |
|  [02]   | lifecycle evidence   | `SessionSnapshot`     | detached host-state product       | `DocumentSession.Snapshot`   |
|  [03]   | capability policy    | `SessionNeed`         | keyless behavior rows             | `DocumentSession.Demand`     |
|  [04]   | source admission     | `SessionSource`       | flat closed source family         | `DocumentSession.Of`         |
|  [05]   | scoped lifetime      | `DocumentSession`     | retained kernel lease             | `DocumentSession.Of`         |
|  [06]   | space regime         | `DocumentSpace`       | model/page behavior rows          | `Regime` / `Adjust`          |
|  [07]   | regime mutation      | `RegimeChange`        | units/tolerances/precision union  | `RegimeChange.Of` / `Adjust` |
|  [08]   | unit correspondence  | `UnitText`            | encoded/semantic/projection union | `UnitText.Length` / `Text`   |
|  [09]   | worksession topology | `WorksessionSnapshot` | detached active/reference rows    | `Worksession` / `FileOf`     |
|  [10]   | worksession custody  | `WorksessionOp`       | scripted attach/detach/reload     | `Worksession` / receipt      |

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
