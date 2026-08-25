# [RASM_RHINO_SESSION]

`Rasm.Rhino.Document` owns document identity, lifecycle admission, capability-scoped access, document lifetime, and the live model/page regime. `DocumentSession` retains an owned headless lease only after every admission gate succeeds, serializes each consuming host call around handle re-resolution and fresh evidence, and re-reads the kernel `Context` so a regime change cannot leave a stale domain bundle. The page also mints the folder's admission fault family: `DraftFault` codes on the kernel `FaultBand.HostDraft` row, every generated owner across the Document and Annotation spines stamps `[ValidationError]`, and the message a `ValidateFactoryArguments` hook authors reaches the rail typed instead of dying inside a boolean funnel.

## [01]-[INDEX]

- [02]-[FAULT]: `DraftFault` — the folder's one admission-refusal family on the kernel band registry, and the folder law that seats it.
- [03]-[IDENTITY_AND_STATE]: `DocKey`, `DocumentReach`, `DocumentSet`, `PhaseStance`, `SessionPhase`, `SessionCondition`, `SessionSnapshot`, the detached `WorksessionSnapshot` topology with serial-keyed model resolution, and the scripted `WorksessionOp` attach/detach/reload rail under `MembershipShift`.
- [04]-[CAPABILITY]: `LaneCapability`, `SessionMode`, `UndoCustody`, and the fully data-driven `SessionNeed` capability table.
- [05]-[SOURCE_AND_SESSION]: `DocumentPath`, the flattened `SessionSource` admission family, `SessionGate`, and the lease-retaining `DocumentSession` owner.
- [06]-[REGIME]: `DocumentSpace`, the kernel-`Context` regime, `DrawingPrecision` composition, and the one `RegimeChange` mutation rail.
- [07]-[REGIME_TEXT]: the host parse/render correspondence over one live regime.
- [08]-[SURFACE_LEDGER]: the page-owned surface registry.

## [02]-[FAULT]

- Owner: `DraftFault` is the direct draft-host family on `FaultBand.HostDraft`; generated-value refusals cross the kernel validation bridge.
- Cases: `HostRefused` preserves the host member and native detail; generated admission and foreign failures retain their exact `Error`.
- Law: generated owners stamp `[ValidationError]`; independent clauses use `FactoryValidation`, and public accumulation rides `Validation<Error, T>`.
- Law: the generated fault-case identity supplies the numeric code, while this root's total `Message` switch supplies presentation.
- Boundary: `DraftFault` never represents generated validation, aggregates, categories, or wire envelopes.
- Packages: `Domain/rails`, Thinktecture.Runtime.Extensions, and LanguageExt.Core.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;
using Thinktecture;

namespace Rasm.Rhino.Document;

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DraftFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.HostDraft;
    private DraftFault() { }

    [FaultCase(0)] public sealed partial record HostRefused(Op Key, string Member, string Detail) : DraftFault;

    public sealed override string Message => Switch(
        hostRefused: static fault => $"Draft host member '{fault.Member}' refused '{fault.Key}': {fault.Detail}");
}
```

## [03]-[IDENTITY_AND_STATE]

- Owner: `DocKey` `[ValueObject<uint>]` admits the positive `RuntimeSerialNumber`; `DocumentReach` is the census vocabulary and `DocumentSet` rows carry ONE `CapabilitySet<DocumentReach>` column from which both the host iterator flag and the admission predicate DERIVE. `SessionPhase` folds the host lifecycle product into one position and carries its `PhaseStance`; `SessionCondition` is the snapshot's condition vocabulary and `SessionSnapshot` `[ComplexValueObject]` admits identity, optional path/name provenance, and every capability-relevant host fact as ONE `CapabilitySet<SessionCondition>` beside the phase. `SessionMap` is the generated host projection both snapshot mints read. `WorksessionSnapshot` projects the active/reference file topology, optional runtime identity, and serial-keyed model resolution without retaining the live `Worksession` handle; `WorksessionOp` carries one model plus a closed verb program, and each `WorksessionVerb` row owns its script token and its `MembershipShift`.
- Entry: `DocKey.Census` returns detached keys from the host iterator. Internal `DocKey.Resolve` re-enters the handle only inside the session owner, and internal `SessionSnapshot.Of` is the single read site: `SessionMap.Facts` projects the host getters once, the condition set folds from the projected facts, and the constraint clauses admit accumulating. One `session.Worksession` name carries both modalities — a serial spread reads, a `WorksessionOp` value transitions — and `WorksessionSnapshot.FileOf` resolves a worksession runtime serial to its file with no document at all.
- Auto: `DocumentSet.IncludeHeadless` and `Admits` both derive from the row's `Reach` set — the iterator flag is `Reach.Admits(Headless)` and the predicate reads the document's own headless bit against the set — so the census discriminant has one authority and a fourth census row is one set literal.
- Auto: the snapshot's nine former bool columns ride `CapabilitySet<SessionCondition>`; the three pairwise clauses admit at the ONE mint through `FactoryValidation` — `UndoRecording` demands `UndoEnabled`, `Undoing` excludes `Redoing`, `Headless` excludes `Pointing` — so an illegal condition product is refused with every violated clause named. The kernel `CapabilityLaw` states exact-corner rosters and no per-row predicate arm, so the implication clause lives at this mint rather than as a law row; the set is constructible nowhere else.
- Law: lifecycle precedence is closing, opening, initializing, creating, ready, unavailable; the tuple switch covers the complete flag product and names no default arm. `PhaseStance` is the phase's one openness column — `Open`, `Transitional`, `Closed` — so a capability row states the stances it admits as a set instead of two bools every reader re-conjoins.
- Law: `SessionSnapshot` is immutable evidence from one read. Every capability use re-resolves the retained key and obtains a new snapshot inside `DocumentSession.Demand` immediately before invoking its host body.
- Law: the host `Worksession` is a read-only roster — every transition rides the serial-pinned script rail: per verb one fresh membership precondition, one scripted host run, one membership postcondition, and one declared inverse; reload composes detach then attach inside one demand window, and the verb fold rides `DocumentCommit.Compensated`, the slice's one compensation algebra, so a failed suffix unwinds the completed prefix through the same landed-then-rollback shape every other rail uses. `WorksessionOutcome` carries the before and after topology, never a bare success flag. `MembershipShift` is the verb's one membership column — the precondition is `Shift.Before`, the postcondition `Shift.After`, and the inverse verb carries the opposite row — so the two former bool columns and the drift they admitted between them are unrepresentable.
- Boundary: the worksession transition rail is STRINGLY by host truth, and the carve is named rather than tolerated. `Rhino.DocObjects.Worksession` exposes reads alone — `Document`, `RuntimeSerialNumber`, `FileName`, `Name`, `ModelCount`, `ModelPaths`, plus the two serial resolvers — and declares no attach, detach, add, or remove member at any access level, so `RhinoApp.RunScript` against the `_-Worksession` command is the only managed route a model transition has. Everything the carve costs is paid back on the same rail: `WorksessionVerb` owns the script token so no call site composes command text, `DocumentPath` refuses anything but a fully qualified path, `Scriptable` refuses a path carrying a quote or a newline so the composed line cannot be broken out of, and each verb proves membership before and after its run so a silently-failed script is a typed refusal rather than an unnoticed no-op. A hand-composed `_-Worksession` string anywhere else in the boundary is the deleted form.
- Boundary: the worksession rail is the boundary's single non-undoable host mutation and is exempt from `DocumentCommit.Sealed` by host truth — attach and detach reshape which files the session references, which Rhino's undo stack does not record, so a bracket here would seal an empty record and advertise a rollback the host cannot perform. Its inverse is the declared per-verb script, not an undo serial, which is why it carries no `SessionNeed.Undo` and no `RedrawPolicy`.
- Boundary: `InGetPoint` proves only a point acquisition; the broader acquisition reentrancy token belongs to the command acquisition algebra. `Worksession.ModelCount` may exceed `ModelPaths.Length` by one for an unsaved active model; `UnsavedActive` preserves that state. Serial resolution admits unique requests, exact key coverage, distinct resolved paths, and membership in the model-path roster before map construction. Attach/detach observation stays on the events page's `WorksessionFile` family; this owner carries the transactional outcome.
- Packages: RhinoCommon `RhinoDoc`/`Worksession` (`.api/api-rhinocommon-document.md`); Riok.Mapperly for `SessionMap` (per-project, `PrivateAssets` — generator only); Thinktecture.Runtime.Extensions; LanguageExt.Core.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Globalization;
using System.IO;
using Rasm.Domain;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Document;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IDetachedDocumentResult { }

[ValueObject<uint>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
[ValidationError]
public readonly partial struct DocKey : IDetachedDocumentResult {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref uint value) =>
        validationError = value is 0u
            ? new ValidationError(string.Join(" | ", new object?[] { Op.Of(), nameof(DocKey), 0d, "a positive runtime serial" }))
            : null;

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
        return from admitted in op.Need(scope)
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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DocumentReach : ICapability<DocumentReach> {
    public static readonly DocumentReach Live = new(key: "live");
    public static readonly DocumentReach Headless = new(key: "headless");
}

[SmartEnum<int>]
public sealed partial class DocumentSet {
    public static readonly DocumentSet Live = new(key: 0, reach: static () => CapabilitySet<DocumentReach>.Of(DocumentReach.Live));
    public static readonly DocumentSet Headless = new(key: 1, reach: static () => CapabilitySet<DocumentReach>.Of(DocumentReach.Headless));
    public static readonly DocumentSet All = new(key: 2, reach: static () => CapabilitySet<DocumentReach>.All);

    [UseDelegateFromConstructor]
    internal partial CapabilitySet<DocumentReach> Reach();

    public bool IncludeHeadless => Reach().Admits(capability: DocumentReach.Headless);

    internal bool Admits(RhinoDoc document) =>
        Reach().Admits(capability: document.IsHeadless ? DocumentReach.Headless : DocumentReach.Live);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PhaseStance : ICapability<PhaseStance> {
    public static readonly PhaseStance Open = new(key: "open");
    public static readonly PhaseStance Transitional = new(key: "transitional");
    public static readonly PhaseStance Closed = new(key: "closed");
}

[SmartEnum<int>]
public sealed partial class SessionPhase {
    public static readonly SessionPhase Ready = new(key: 0, stance: PhaseStance.Open);
    public static readonly SessionPhase Opening = new(key: 1, stance: PhaseStance.Transitional);
    public static readonly SessionPhase Closing = new(key: 2, stance: PhaseStance.Transitional);
    public static readonly SessionPhase Initializing = new(key: 3, stance: PhaseStance.Transitional);
    public static readonly SessionPhase Creating = new(key: 4, stance: PhaseStance.Transitional);
    public static readonly SessionPhase Unavailable = new(key: 5, stance: PhaseStance.Closed);

    public PhaseStance Stance { get; }

    internal static SessionPhase Of(SessionFacts facts) =>
        (facts.Available, facts.Closing, facts.Opening, facts.Initializing, facts.Creating) switch {
            (_, true, _, _, _) => Closing,
            (_, false, true, _, _) => Opening,
            (_, false, false, true, _) => Initializing,
            (_, false, false, false, true) => Creating,
            (true, false, false, false, false) => Ready,
            (false, false, false, false, false) => Unavailable,
        };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SessionCondition : ICapability<SessionCondition> {
    public static readonly SessionCondition ReadOnly = new(key: "read-only");
    public static readonly SessionCondition Locked = new(key: "locked");
    public static readonly SessionCondition UndoEnabled = new(key: "undo-enabled");
    public static readonly SessionCondition UndoRecording = new(key: "undo-recording");
    public static readonly SessionCondition Undoing = new(key: "undoing");
    public static readonly SessionCondition Redoing = new(key: "redoing");
    public static readonly SessionCondition Headless = new(key: "headless");
    public static readonly SessionCondition Modified = new(key: "modified");
    public static readonly SessionCondition Pointing = new(key: "pointing");
}

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record SessionFacts(
    uint Serial,
    string? Path,
    string? Name,
    bool Available,
    bool Closing,
    bool Opening,
    bool Initializing,
    bool Creating,
    bool ReadOnly,
    bool Locked,
    bool UndoEnabled,
    bool UndoRecording,
    bool Undoing,
    bool Redoing,
    bool Headless,
    bool Modified,
    bool Pointing,
    bool InCommand,
    Guid ActiveCommand);

internal sealed record WorksessionFacts(
    uint Serial,
    string? FileName,
    string? Name,
    int ModelCount,
    string[] ModelPaths);

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target,
        EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]
internal static partial class SessionMap {
    [MapProperty(nameof(RhinoDoc.RuntimeSerialNumber), nameof(SessionFacts.Serial))]
    [MapProperty(nameof(RhinoDoc.IsAvailable), nameof(SessionFacts.Available))]
    [MapProperty(nameof(RhinoDoc.IsClosing), nameof(SessionFacts.Closing))]
    [MapProperty(nameof(RhinoDoc.IsOpening), nameof(SessionFacts.Opening))]
    [MapProperty(nameof(RhinoDoc.IsInitializing), nameof(SessionFacts.Initializing))]
    [MapProperty(nameof(RhinoDoc.IsCreating), nameof(SessionFacts.Creating))]
    [MapProperty(nameof(RhinoDoc.IsReadOnly), nameof(SessionFacts.ReadOnly))]
    [MapProperty(nameof(RhinoDoc.IsLocked), nameof(SessionFacts.Locked))]
    [MapProperty(nameof(RhinoDoc.UndoRecordingEnabled), nameof(SessionFacts.UndoEnabled))]
    [MapProperty(nameof(RhinoDoc.UndoRecordingIsActive), nameof(SessionFacts.UndoRecording))]
    [MapProperty(nameof(RhinoDoc.UndoActive), nameof(SessionFacts.Undoing))]
    [MapProperty(nameof(RhinoDoc.RedoActive), nameof(SessionFacts.Redoing))]
    [MapProperty(nameof(RhinoDoc.IsHeadless), nameof(SessionFacts.Headless))]
    [MapProperty(nameof(RhinoDoc.InGetPoint), nameof(SessionFacts.Pointing))]
    [MapProperty(nameof(RhinoDoc.ActiveCommandId), nameof(SessionFacts.ActiveCommand))]
    [MapValue(nameof(SessionFacts.InCommand), Use = nameof(CommandDepth))]
    internal static partial SessionFacts Facts(RhinoDoc document);

    [MapProperty(nameof(Worksession.RuntimeSerialNumber), nameof(WorksessionFacts.Serial))]
    internal static partial WorksessionFacts Worksession(Worksession worksession);

    private static bool CommandDepth(RhinoDoc document) => document.InCommand(bIgnoreScriptRunnerCommands: false);
}

[ComplexValueObject]
[ValidationError]
public sealed partial class SessionSnapshot : IDetachedDocumentResult {
    public DocKey Key { get; }
    public Option<DocumentPath> Path { get; }
    public Option<string> Name { get; }
    public SessionPhase Phase { get; }
    public CapabilitySet<SessionCondition> Conditions { get; }
    public Option<Guid> ActiveCommand { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref DocKey key,
        ref Option<DocumentPath> path,
        ref Option<string> name,
        ref SessionPhase phase,
        ref CapabilitySet<SessionCondition> conditions,
        ref Option<Guid> activeCommand) {
        Op op = Op.Of();
        CapabilitySet<SessionCondition> held = conditions;
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
                (key == default, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Key) }))),
                (name.Exists(static value => string.IsNullOrWhiteSpace(value: value)),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Name) }))),
                (phase is null, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Phase) }))),
                (held.Admits(SessionCondition.UndoRecording) && !held.Admits(SessionCondition.UndoEnabled),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Conditions), "undo recording only under undo enabled" }))),
                (held.Admits(SessionCondition.Undoing) && held.Admits(SessionCondition.Redoing),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Conditions), "undoing excludes redoing" }))),
                (held.Admits(SessionCondition.Headless) && held.Admits(SessionCondition.Pointing),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Conditions), "a headless document acquires no point" }))));
    }

    internal static Fin<SessionSnapshot> Of(RhinoDoc document, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(document).ToFin(Fail: op.MissingContext())
               from facts in op.Catch(() => Fin.Succ(value: SessionMap.Facts(document: active)))
               from identity in op.AcceptValidated<DocKey>(candidate: facts.Serial)
               from path in Optional(facts.Path)
                   .Filter(static value => !string.IsNullOrWhiteSpace(value: value))
                   .Traverse(value => DocumentPath.Of(value: value, key: op))
                   .As()
               from snapshot in op.AcceptValidated<SessionSnapshot>(
                   Validate(
                       identity,
                       path,
                       Optional(facts.Name).Filter(static value => !string.IsNullOrWhiteSpace(value: value)),
                       SessionPhase.Of(facts: facts),
                       Conditions(facts: facts),
                       Optional(facts.ActiveCommand).Filter(static id => id != Guid.Empty),
                       out SessionSnapshot? admitted),
                   admitted)
               select snapshot;
    }

    private static CapabilitySet<SessionCondition> Conditions(SessionFacts facts) =>
        CapabilitySet<SessionCondition>.Of(
            Seq<(bool Held, SessionCondition Row)>(
                (facts.ReadOnly, SessionCondition.ReadOnly),
                (facts.Locked, SessionCondition.Locked),
                (facts.UndoEnabled, SessionCondition.UndoEnabled),
                (facts.UndoRecording, SessionCondition.UndoRecording),
                (facts.Undoing, SessionCondition.Undoing),
                (facts.Redoing, SessionCondition.Redoing),
                (facts.Headless, SessionCondition.Headless),
                (facts.Modified, SessionCondition.Modified),
                (facts.Pointing || facts.InCommand && facts.Headless is false && facts.Pointing, SessionCondition.Pointing))
            .Choose(static row => row.Held ? Some(row.Row) : None)
            .ToArray()
            .AsSpan());
}

[SmartEnum<int>]
public sealed partial class WorksessionCustody {
    public static readonly WorksessionCustody Active = new(key: 0);
    public static readonly WorksessionCustody Reference = new(key: 1);
}

[ComplexValueObject]
[ValidationError]
public sealed partial class WorksessionModel {
    public DocumentPath Path { get; }
    public WorksessionCustody Custody { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref DocumentPath path,
        ref WorksessionCustody custody) =>
        validationError = string.IsNullOrWhiteSpace(value: path.Value) || custody is null
            ? new ValidationError(string.Join(" | ", new object?[] { Op.Of(), nameof(WorksessionModel) }))
            : null;

    internal static Fin<WorksessionModel> Of(DocumentPath path, WorksessionCustody custody, Op key) =>
        key.AcceptValidated<WorksessionModel>(Validate(path, custody, out WorksessionModel? admitted), admitted);
}

[ComplexValueObject]
[ValidationError]
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
        Op op = Op.Of();
        Seq<WorksessionModel> admittedModels = models.Choose(static model => Optional(model)).Strict();
        int difference = reportedCount - admittedModels.Count;
        int activeCount = admittedModels.Filter(static model => model.Custody == WorksessionCustody.Active).Count;
        HashSet<DocumentPath> roster = admittedModels.Map(static model => model.Path).ToHashSet();
        bool unsaved = unsavedActive;
        HashMap<uint, DocumentPath> map = resolved;
        validationError = FactoryValidation.Of(
            FactoryValidation.Violated(
                (document == default, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Document) }))),
                (serial.Exists(static value => value is 0u),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Serial), 0d, "a positive worksession serial" }))),
                (name.Exists(static value => string.IsNullOrWhiteSpace(value: value)),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Name) }))),
                (admittedModels.Count != models.Count, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Models) }))),
                (admittedModels.DistinctBy(static model => model.Path).Count != admittedModels.Count,
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Models), "distinct model paths" }))),
                (activeCount != (unsaved ? 0 : 1),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Models), "exactly one active model unless the active model is unsaved" }))),
                (reportedCount <= 0 || difference is < 0 or > 1 || unsaved != (difference is 1),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(ReportedCount), "a count matching the roster with at most one unsaved active" }))),
                (map.Keys.Exists(static value => value is 0u) || map.Values.Distinct().Count != map.Count
                    || map.Values.Exists(path => !roster.Contains(key: path)),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Resolved), "distinct rostered resolutions under positive serials" }))));
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
        from worksession in key.Catch(() => Optional(owner.Worksession).ToFin(Fail: new DraftFault.HostRefused(
            Key: key, Member: nameof(RhinoDoc.Worksession), Detail: "returned no worksession")))
        from facts in key.Catch(() => Fin.Succ(value: SessionMap.Worksession(worksession: worksession)))
        from paths in toSeq(facts.ModelPaths ?? [])
            .Traverse(value => DocumentPath.Of(value: value, key: key).ToValidation())
            .As()
            .ToFin()
        from definition in Optional(facts.FileName)
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
        from snapshot in key.AcceptValidated<WorksessionSnapshot>(
            Validate(
                identity,
                Optional(facts.Serial).Filter(static serial => serial > 0u),
                definition,
                Optional(facts.Name).Filter(static value => !string.IsNullOrWhiteSpace(value: value)),
                models,
                facts.ModelCount,
                active.IsNone,
                resolved,
                out WorksessionSnapshot? admitted),
            admitted)
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

[SmartEnum<int>]
public sealed partial class MembershipShift {
    public static readonly MembershipShift Joins = new(key: 0, before: false, after: true);
    public static readonly MembershipShift Leaves = new(key: 1, before: true, after: false);

    public bool Before { get; }
    public bool After { get; }
}

[SmartEnum<string>]
public sealed partial class WorksessionVerb {
    public static readonly WorksessionVerb Attach = new(
        "attach",
        scriptToken: "_Attach",
        shift: MembershipShift.Joins,
        inverse: static () => Detach);
    public static readonly WorksessionVerb Detach = new(
        "detach",
        scriptToken: "_Detach",
        shift: MembershipShift.Leaves,
        inverse: static () => Attach);

    internal string ScriptToken { get; }
    internal MembershipShift Shift { get; }

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
                   .Traverse(verb => op.Need(verb).ToValidation())
                   .As()
                   .ToFin()
               from nonempty in guard(!program.IsEmpty, op.InvalidInput()).ToFin()
               select new WorksessionOp(model: model, verbs: program.Strict());
    }
}

public sealed record WorksessionOutcome(
    WorksessionOp Operation,
    WorksessionSnapshot Before,
    WorksessionSnapshot After) : IDetachedDocumentResult;

public static class SessionWorksession {
    extension(DocumentSession session) {
        public Fin<WorksessionSnapshot> Worksession(params ReadOnlySpan<uint> modelSerials) {
            Op op = Op.Of();
            Seq<uint> serials = toSeq(modelSerials.ToArray());
            return op.Need(session).Bind(scope => scope.Demand(
                use: document => WorksessionSnapshot.Of(document: document, key: op, modelSerials: serials),
                key: op,
                needs: [SessionNeed.Read]));
        }

        public Fin<WorksessionOutcome> Worksession(WorksessionOp change, Op? key = null) {
            Op op = key.OrDefault();
            return from scope in op.Need(session)
                   from request in op.Need(change)
                   from outcome in scope.Demand(
                       use: document =>
                           from before in WorksessionSnapshot.Of(document: document, key: op, modelSerials: Seq<uint>())
                           from completed in DocumentCommit.Compensated(
                               source: request.Verbs,
                               land: verb => Apply(document: document, model: request.Model, verb: verb, op: op).Map(_ => verb),
                               rollback: landed => Restore(
                                   document: document,
                                   model: request.Model,
                                   completed: landed,
                                   op: op))
                           from after in WorksessionSnapshot.Of(document: document, key: op, modelSerials: Seq<uint>())
                               .Rollback(() => Restore(
                                   document: document,
                                   model: request.Model,
                                   completed: completed,
                                   op: op))
                           select new WorksessionOutcome(Operation: request, Before: before, After: after),
                       key: op,
                       needs: [SessionNeed.Read, SessionNeed.Mutate, SessionNeed.Acquire, SessionNeed.Interrupt])
                   select outcome;
        }
    }

    private static Fin<Unit> Apply(RhinoDoc document, DocumentPath model, WorksessionVerb verb, Op op) =>
        from current in WorksessionSnapshot.Of(document: document, key: op, modelSerials: Seq<uint>())
        from admitted in guard(current.Member(path: model) == verb.Shift.Before, op.InvalidInput()).ToFin()
        from landed in (
            from run in Run(document: document, model: model, verb: verb, op: op)
            from proof in WorksessionSnapshot.Of(document: document, key: op, modelSerials: Seq<uint>())
            from exact in guard(proof.Member(path: model) == verb.Shift.After, op.InvalidResult()).ToFin()
            select unit)
            .Rollback(() => Restore(document: document, model: model, completed: Seq(verb), op: op))
        select unit;

    private static Fin<Unit> Restore(
        RhinoDoc document,
        DocumentPath model,
        Seq<WorksessionVerb> completed,
        Op op) => completed.Rev()
        .Traverse(verb => {
            WorksessionVerb inverse = verb.Inverse();
            return (from current in WorksessionSnapshot.Of(document: document, key: op, modelSerials: Seq<uint>())
                    from restored in current.Member(path: model) == inverse.Shift.After
                        ? Fin.Succ(value: unit)
                        : from admitted in guard(current.Member(path: model) == inverse.Shift.Before, op.InvalidResult()).ToFin()
                          from run in Run(document: document, model: model, verb: inverse, op: op)
                          from proof in WorksessionSnapshot.Of(document: document, key: op, modelSerials: Seq<uint>())
                          from landed in guard(proof.Member(path: model) == inverse.Shift.After, op.InvalidResult()).ToFin()
                          select unit
                    select restored).ToValidation();
        })
        .As()
        .ToFin()
        .Map(static _ => unit);

    private static Fin<Unit> Run(RhinoDoc document, DocumentPath model, WorksessionVerb verb, Op op) =>
        op.Catch(() => RhinoApp.RunScript(
            documentSerialNumber: document.RuntimeSerialNumber,
            script: verb.Script(model: model),
            echo: false)
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(new DraftFault.HostRefused(
                    Key: op, Member: nameof(RhinoApp.RunScript), Detail: $"{verb.Key}:{model.Value}")));
}
```

## [04]-[CAPABILITY]

- Owner: `LaneCapability` is the mode vocabulary and `SessionMode` rows carry ONE `CapabilitySet<LaneCapability>` from which every lane question reads. `SessionNeed` is a fully DATA-DRIVEN capability table: each row states the lane capabilities it requires, the phase stances it admits, the session conditions it demands, and the conditions that bar it — four set columns, no predicate delegate anywhere. `UndoCustody` closes the undo axis of a mutation demand as a row carrying its own grant set.
- Entry: `SessionMode.OfRunMode` returns `Fin<SessionMode>` and rejects an unknown foreign ordinal. `SessionNeed.Admit(snapshot, mode)` is the ONE probe — it accumulates a typed refusal per violated axis, each naming the need and the axis, so a demand refused on three axes reports three. `DocumentSession.Of` rejects an empty or mode-incompatible capability set before acquisition; `DocumentSession.Demand` proves one or more granted rows against one fresh snapshot around the consuming host body.
- Auto: every admission is set algebra over the three vocabularies this page already owns — the lane and demand gates refuse through `Require`, which hands each refusal the MISSING rows, the stance gate is `Stances.Admits(snapshot.Phase.Stance)`, and the bar gate is per-row exclusion — so a new capability row is four set literals and the fold never changes.
- Law: `IsLocked` is detached file-write ownership evidence, not a mutation refusal; Rhino reports it when the current document owns the file lock, which is why `Locked` bars no row.
- Law: `SessionNeed.Mutation(custody, redraw)` is the one mutation-need derivation — the custody ROW carries its own grant set (`Recorded` grants `Mutate`+`Undo`, `Unrecorded` grants `Mutate` alone) and the redraw policy appends its row — every commit rail passes it to `Demand`; a per-rail inline `Seq(Mutate) + ...` re-derivation is the deleted form. NAMED LOSS: the `bool undo` argument; witness `SessionNeed.Mutation(undo: true, redraw: policy)` rebuilt as `SessionNeed.Mutation(UndoCustody.Recorded, policy)` at all nineteen composing sites.
- Growth: a capability lands as one `SessionNeed` row of four set literals; a new lane capability, stance, or condition is one vocabulary row and no fold edit.
- Boundary: HOST-SPECIFIC-STAYS — `Dialog`, `Acquire`, and `Interrupt` encode `RhinoDoc.InGetPoint`/`InCommand`/`RunMode` host truths; the rows survive because the table is the host contract spelled as data.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LaneCapability : ICapability<LaneCapability> {
    public static readonly LaneCapability Live = new(key: "live");
    public static readonly LaneCapability Dialogs = new(key: "dialogs");
}

[SmartEnum<int>]
public sealed partial class SessionMode {
    public static readonly SessionMode Interactive = new(key: 0,
        capabilities: static () => CapabilitySet<LaneCapability>.All);
    public static readonly SessionMode Scripted = new(key: 1,
        capabilities: static () => CapabilitySet<LaneCapability>.Of(LaneCapability.Live));
    public static readonly SessionMode Headless = new(key: 2,
        capabilities: static () => CapabilitySet<LaneCapability>.None);

    [UseDelegateFromConstructor]
    public partial CapabilitySet<LaneCapability> Capabilities();

    public static Fin<SessionMode> OfRunMode(RunMode mode, Op? key = null) {
        Op op = key.OrDefault();
        return mode switch {
            RunMode.Interactive => Fin.Succ(value: Interactive),
            RunMode.Scripted => Fin.Succ(value: Scripted),
            var unknown => Fin.Fail<SessionMode>(error: op.InvalidResult(detail: unknown.ToString())),
        };
    }
}

[SmartEnum<int>]
public sealed partial class UndoCustody {
    public static readonly UndoCustody Recorded = new(key: 0, records: true, grants: static () => Seq(SessionNeed.Mutate, SessionNeed.Undo));
    public static readonly UndoCustody Unrecorded = new(key: 1, records: false, grants: static () => Seq(SessionNeed.Mutate));

    internal bool Records { get; }

    [UseDelegateFromConstructor]
    internal partial Seq<SessionNeed> Grants();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SessionNeed {
    public static readonly SessionNeed Observe = new(key: "observe",
        lane: static () => CapabilitySet<LaneCapability>.None,
        stances: static () => CapabilitySet<PhaseStance>.Of(PhaseStance.Open, PhaseStance.Transitional),
        demands: static () => CapabilitySet<SessionCondition>.None,
        barred: static () => CapabilitySet<SessionCondition>.None);
    public static readonly SessionNeed Read = new(key: "read",
        lane: static () => CapabilitySet<LaneCapability>.None,
        stances: static () => CapabilitySet<PhaseStance>.Of(PhaseStance.Open),
        demands: static () => CapabilitySet<SessionCondition>.None,
        barred: static () => CapabilitySet<SessionCondition>.None);
    public static readonly SessionNeed Mutate = new(key: "mutate",
        lane: static () => CapabilitySet<LaneCapability>.None,
        stances: static () => CapabilitySet<PhaseStance>.Of(PhaseStance.Open),
        demands: static () => CapabilitySet<SessionCondition>.None,
        barred: static () => CapabilitySet<SessionCondition>.Of(SessionCondition.ReadOnly, SessionCondition.Undoing, SessionCondition.Redoing));
    public static readonly SessionNeed Undo = new(key: "undo",
        lane: static () => CapabilitySet<LaneCapability>.None,
        stances: static () => CapabilitySet<PhaseStance>.Of(PhaseStance.Open),
        demands: static () => CapabilitySet<SessionCondition>.Of(SessionCondition.UndoEnabled),
        barred: static () => CapabilitySet<SessionCondition>.Of(SessionCondition.ReadOnly, SessionCondition.Undoing, SessionCondition.Redoing));
    public static readonly SessionNeed Redraw = new(key: "redraw",
        lane: static () => CapabilitySet<LaneCapability>.Of(LaneCapability.Live),
        stances: static () => CapabilitySet<PhaseStance>.Of(PhaseStance.Open),
        demands: static () => CapabilitySet<SessionCondition>.None,
        barred: static () => CapabilitySet<SessionCondition>.Of(SessionCondition.Headless));
    public static readonly SessionNeed Acquire = new(key: "acquire",
        lane: static () => CapabilitySet<LaneCapability>.Of(LaneCapability.Live),
        stances: static () => CapabilitySet<PhaseStance>.Of(PhaseStance.Open),
        demands: static () => CapabilitySet<SessionCondition>.None,
        barred: static () => CapabilitySet<SessionCondition>.Of(SessionCondition.Headless, SessionCondition.Pointing));
    public static readonly SessionNeed Dialog = new(key: "dialog",
        lane: static () => CapabilitySet<LaneCapability>.Of(LaneCapability.Live, LaneCapability.Dialogs),
        stances: static () => CapabilitySet<PhaseStance>.Of(PhaseStance.Open),
        demands: static () => CapabilitySet<SessionCondition>.None,
        barred: static () => CapabilitySet<SessionCondition>.Of(SessionCondition.Headless, SessionCondition.Pointing));
    public static readonly SessionNeed Interrupt = new(key: "interrupt",
        lane: static () => CapabilitySet<LaneCapability>.Of(LaneCapability.Live),
        stances: static () => CapabilitySet<PhaseStance>.Of(PhaseStance.Open),
        demands: static () => CapabilitySet<SessionCondition>.None,
        barred: static () => CapabilitySet<SessionCondition>.Of(SessionCondition.Headless));
    public static readonly SessionNeed Export = new(key: "export",
        lane: static () => CapabilitySet<LaneCapability>.None,
        stances: static () => CapabilitySet<PhaseStance>.Of(PhaseStance.Open),
        demands: static () => CapabilitySet<SessionCondition>.None,
        barred: static () => CapabilitySet<SessionCondition>.None);

    [UseDelegateFromConstructor] internal partial CapabilitySet<LaneCapability> Lane();
    [UseDelegateFromConstructor] internal partial CapabilitySet<PhaseStance> Stances();
    [UseDelegateFromConstructor] internal partial CapabilitySet<SessionCondition> Demands();
    [UseDelegateFromConstructor] internal partial CapabilitySet<SessionCondition> Barred();

    internal Validation<Error, SessionNeed> Admit(SessionSnapshot snapshot, SessionMode mode, Op op) {
        SessionNeed need = this;
        return FactoryValidation.Admit(
                FactoryValidation.Violated(
                    (!mode.Capabilities().AdmitsAll(Lane()),
                        () => need.Ground(op: op, axis: "lane", shortfall: mode.Capabilities().Missing(Lane()).Wire)),
                    (!Stances().Admits(capability: snapshot.Phase.Stance), () => need.Ground(op: op, axis: "stance")),
                    (!snapshot.Conditions.AdmitsAll(Demands()),
                        () => need.Ground(op: op, axis: "demand", shortfall: snapshot.Conditions.Missing(Demands()).Wire)),
                    (toSeq(Barred().Held).Exists(row => snapshot.Conditions.Admits(capability: row)),
                        () => need.Ground(op: op, axis: "barred"))))
            .ToValidation()
            .Map(_ => need);
    }

    private ValidationClause Ground(Op op, string axis, Option<string> shortfall = default) =>
        new(string.Join(" | ", new object?[] { op, Key, $"the '{Key}' need admitted on the '{axis}' axis"
                + shortfall.Match(Some: static wire => $"; missing <{wire}>", None: static () => string.Empty) }));

    internal bool AdmitsMode(SessionMode mode) => mode.Capabilities().AdmitsAll(required: Lane());

    internal static Seq<SessionNeed> Mutation(UndoCustody custody, RedrawPolicy redraw) =>
        custody.Grants() + (redraw.Traits.Admits(capability: RedrawAxis.Enabled) ? Seq(Redraw) : Seq<SessionNeed>());
}
```

## [05]-[SOURCE_AND_SESSION]

- Owner: `DocumentPath` admits absolute nonblank path text once, while `DocumentFile` carries existing-file versus existing-`.3dm` resolution as behavior rows. `SessionSource` is one flat `[Union]` over borrowed live/active/keyed/opened documents and owned empty/template/archive/configured headless documents; source depth is case data, so admission performs one generated dispatch, and the ambient `RhinoDoc.ActiveDoc` static crosses the boundary only through the `Active` case. `SessionGate` is the session's lifecycle state machine and `DocumentSession` the lease-retaining owner over it.
- Entry: `SessionSource.Acquire` returns `Fin<Lease<RhinoDoc>>`; every deterministic source/mode refusal and file requirement completes before its host call, and the `Configured` case carries a typed `ArchiveMap` (Persistence/dictionary.md) minted into the native option payload inside the acquire arm, so the host receives a fresh dictionary no caller can mutate. `DocumentSession.Of` rejects empty, duplicate, or mode-incompatible capabilities, acquires, snapshots, checks lane/document agreement, validates the kernel context, and only then adopts the lease.
- Auto: the lifecycle rides ONE `Atom<SessionGate>` stepped through `Cell.Step` — `Enter` declines on a disposing or released gate, `Exit` decrements and answers whether this exit owes the deferred disposal, and `Close` either releases immediately or parks the disposal on the open depth — so every transition answers a `Transition<SessionGate>` verdict and the three raw fields plus their lock this replaces cannot report a race they lost. NAMED LOSS: the lock's cross-thread serialization of headless demand bodies; the live lane serializes on the marshal it crosses, and a headless session is single-caller by its acquisition contract, stated here.
- Law: a failed admission releases an owned lease before returning its original fault, the cleanup fault AGGREGATING into the primary through the `Error` monoid — never discarded. A successful admission never calls `Lease.Use`, because `Use` closes `Owned` when its projection returns; the session retains the lease and releases it exactly once through the gate's one terminal transition.
- Law: `Demand` is the sole capability surface. Its command overload admits `Fin<Unit>` directly, while the generic overload retains the `IDetachedDocumentResult` bound that rejects a raw `RhinoDoc`; both enter the same private capability window, and the kernel `Context` crosses through the private detached `DetachedContext` capsule.
- Law: the demand window is a BRACKET — `Enter` acquires, the body runs, `Exit` settles in the `Fin` arm of the same expression — so a body that raises still exits, a nested demand enters its own bracket, and `Dispose` issued mid-demand defers to the outermost exit, which runs the release and aggregates its fault into the body's outcome.
- Law: the marshal is the kernel dispatch — a live-lane demand crosses `UiThread.Run(new UiDispatch<TResult>.Blocking(body), DispatchLane.Immediate, op)`, in-frame when the caller already holds the marshal and invoked otherwise, while a headless session runs the body on the caller thread because a headless process has no marshal to cross and the kernel entry would refuse `Headless` typed. The former hand marshal and its S2-ledger carve both delete; the gauged span is the kernel dispatch's own pulse.
- Law: `Context()` re-enters `Context.Of(RhinoDoc)` on every call, so model-unit and tolerance changes cannot stale the context consumed by later geometry work.
- Law: `Admission.All` and `Admission.Pair` own reference-argument admission — one span fold and one applicative pair composed by every Document spine, so the Optional-traverse spelling appears once. The former `Admission.Admitted` funnel is DELETED: `op.AcceptValidated` surfaces the generated `DraftFault` directly, and a member that re-stamped it with a bare refusal was the message-stranding defect `[02]` names.
- Boundary: `IDetachedDocumentResult` marks the admitted result census: detached facts and explicit lifetime capsules. `Demand` forbids a raw `RhinoDoc`, and each capsule owns every live handle it carries beyond the callback. `DocumentPath` conforms — admitted path text is detached by construction — so a path resolution returns through `Demand` directly.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public readonly partial struct DocumentPath : IDetachedDocumentResult {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value switch {
            "" => new ValidationError(string.Join(" | ", new object?[] { Op.Of(), nameof(DocumentPath) })),
            var path when !Path.IsPathFullyQualified(path: path) =>
                new ValidationError(string.Join(" | ", new object?[] { Op.Of(), nameof(DocumentPath), "a fully qualified path" })),
            _ => null,
        };
    }

    public static Fin<DocumentPath> Of(string value, Op? key = null) =>
        key.OrDefault().AcceptValidated<DocumentPath>(candidate: value);

    internal Fin<string> Resolve(DocumentFile file, Op key) =>
        from policy in key.Need(file)
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
                    .ToFin(Fail: new DraftFault.HostRefused(
                        Key: op, Member: nameof(RhinoDoc.Open), Detail: path.Value))))
                select acquired,
            empty: static (op, _) => op.Catch(() => Minted(
                document: RhinoDoc.CreateHeadless(file3dmTemplatePath: string.Empty),
                member: nameof(RhinoDoc.CreateHeadless),
                key: op)),
            template: static (op, source) => Headless(
                path: source.Path,
                open: static resolved => RhinoDoc.CreateHeadless(file3dmTemplatePath: resolved),
                member: nameof(RhinoDoc.CreateHeadless),
                key: op),
            archive: static (op, source) => Headless(
                path: source.Path,
                open: static resolved => RhinoDoc.OpenHeadless(file3dmPath: resolved),
                member: nameof(RhinoDoc.OpenHeadless),
                key: op),
            configured: static (op, source) =>
                from path in source.Path.Resolve(file: DocumentFile.Existing, key: op)
                from options in op.Need(source.Options)
                from minted in options.Mint(key: op)
                from lease in op.Catch(() => Minted(
                    document: RhinoDoc.OpenHeadless(filePath: path, options: minted),
                    member: nameof(RhinoDoc.OpenHeadless),
                    key: op))
                select lease)
        select lease;

    private Fin<Unit> Admits(SessionMode mode, Op key) =>
        guard(
            flag: Switch(
                state: mode.Capabilities().Admits(capability: LaneCapability.Live),
                live: static (live, _) => live,
                active: static (live, _) => live,
                keyed: static (_, _) => true,
                opened: static (live, _) => live,
                empty: static (live, _) => !live,
                template: static (live, _) => !live,
                archive: static (live, _) => !live,
                configured: static (live, _) => !live),
            False: key.InvalidInput()).ToFin();

    private static Fin<Lease<RhinoDoc>> Borrowed(Fin<RhinoDoc> document) =>
        document.Map(static value => (Lease<RhinoDoc>)new Lease<RhinoDoc>.Borrowed(Value: value));

    private static Fin<Lease<RhinoDoc>> Headless(DocumentPath path, Func<string, RhinoDoc?> open, string member, Op key) =>
        from resolved in path.Resolve(file: DocumentFile.ThreeDm, key: key)
        from lease in key.Catch(() => Minted(document: open(arg: resolved), member: member, key: key))
        select lease;

    private static Fin<Lease<RhinoDoc>> Minted(RhinoDoc? document, string member, Op key) =>
        Optional(document)
            .ToFin(Fail: new DraftFault.HostRefused(Key: key, Member: member, Detail: "returned no document"))
            .Map(static value => (Lease<RhinoDoc>)new Lease<RhinoDoc>.Owned(Value: value));
}

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record SessionGate(int Depth, bool Closing, bool Released) {
    internal static readonly SessionGate Open = new(Depth: 0, Closing: false, Released: false);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DocumentSession : IDisposable, IDetachedDocumentResult {
    private readonly Atom<SessionGate> gate = Atom(SessionGate.Open);
    private readonly Lease<RhinoDoc> lease;
    private readonly LanguageExt.HashSet<SessionNeed> granted;

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
               from session in Marshalled(
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
        where TResult : IDetachedDocumentResult =>
        Within(use: use, key: key, needs: needs);

    internal Fin<Unit> Demand(
        Func<RhinoDoc, Fin<Unit>> use,
        Op? key = null,
        params ReadOnlySpan<SessionNeed> needs) =>
        Within(use: use, key: key, needs: needs);

    private Fin<TResult> Within<TResult>(
        Func<RhinoDoc, Fin<TResult>> use,
        Op? key,
        ReadOnlySpan<SessionNeed> needs) {
        Op op = key.OrDefault();
        return from admission in (
                   op.Need(use).ToValidation(),
                   AdmitNeeds(needs: needs, mode: Mode, op: op).ToValidation())
                   .Apply(static (body, requested) => (Body: body, Requested: requested))
                   .As()
                   .ToFin()
               from result in Marshalled(
                   mode: Mode,
                   use: () => Bracketed(
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

    public void Dispose() =>
        ignore(Cell.Step(
                gate,
                static held => held.Released || held.Closing
                    ? Option<SessionGate>.None
                    : Some(held.Depth is 0
                        ? held with { Released = true }
                        : held with { Closing = true }),
                Op.Of().InvalidResult())
            .Switch(
                state: this,
                committed: static (session, row) => row.State.Released ? ignore(session.Release(op: Op.Of())) : unit,
                ceded: static (_, _) => unit,
                refused: static (_, _) => unit,
                contended: static (_, _) => unit));

    private Fin<TResult> Bracketed<TResult>(
        Func<RhinoDoc, Fin<TResult>> use,
        LanguageExt.HashSet<SessionNeed> requested,
        Op op) =>
        IO.lift(() => Enter(op: op))
            .Bracket(
                Use: _ => IO.lift(() => Proven(use: use, requested: requested, op: op)),
                Fin: _ => IO.lift(() => Exit(op: op)))
            .Run()
            .Flatten();

    private Fin<TResult> Proven<TResult>(
        Func<RhinoDoc, Fin<TResult>> use,
        LanguageExt.HashSet<SessionNeed> requested,
        Op op) =>
        op.Catch(() =>
            from grants in guard(requested.ForAll(granted.Contains), op.MissingContext()).ToFin()
            from document in Key.Resolve(key: op)
            from snapshot in SessionSnapshot.Of(document: document, key: op)
            from capabilities in requested.AsIterable()
                .Traverse(need => need.Admit(snapshot: snapshot, mode: Mode, op: op))
                .As()
                .ToFin()
            from value in Optional(use(arg: document))
                .ToFin(Fail: op.InvalidResult())
                .Bind(identity)
            select value);

    private Fin<Unit> Enter(Op op) =>
        Cell.Step(
            gate,
            static held => held.Released || held.Closing ? Option<SessionGate>.None : Some(held with { Depth = held.Depth + 1 }),
            op.MissingContext())
        .Switch(
            state: op,
            committed: static (_, _) => Fin.Succ(unit),
            ceded: static (key, _) => Fin.Fail<Unit>(key.MissingContext()),
            refused: static (_, row) => Fin.Fail<Unit>(row.Cause),
            contended: static (key, _) => Fin.Fail<Unit>(key.InvalidResult()));

    private Fin<Unit> Exit(Op op) =>
        Cell.Step(
            gate,
            static held => held.Depth > 0
                ? Some(held.Depth is 1 && held.Closing
                    ? held with { Depth = 0, Closing = false, Released = true }
                    : held with { Depth = held.Depth - 1 })
                : Option<SessionGate>.None,
            op.InvalidResult())
        .Switch(
            state: (Session: this, Key: op),
            committed: static (held, row) => row.State.Released ? held.Session.Release(op: held.Key) : Fin.Succ(unit),
            ceded: static (held, _) => Fin.Fail<Unit>(held.Key.InvalidResult()),
            refused: static (_, row) => Fin.Fail<Unit>(row.Cause),
            contended: static (held, _) => Fin.Fail<Unit>(held.Key.InvalidResult()));

    private Fin<Unit> Release(Op op) => op.Catch(() => {
        lease.Dispose();
        return Fin.Succ(value: unit);
    });

    private static Fin<LanguageExt.HashSet<SessionNeed>> AdmitNeeds(
        ReadOnlySpan<SessionNeed> needs,
        SessionMode mode,
        Op op) =>
        from demanded in Admission.All(values: needs, key: op)
        from nonempty in guard(flag: !demanded.IsEmpty, False: op.InvalidInput()).ToFin()
        let distinct = toHashSet(demanded)
        from unique in guard(flag: distinct.Count == demanded.Count, False: op.InvalidInput()).ToFin()
        from modeAdmitted in distinct.AsIterable()
            .Traverse(need => need.AdmitsMode(mode: mode)
                ? Validation<Error, SessionNeed>.Success(need)
                : Validation<Error, SessionNeed>.Fail(new KernelFault.InvalidValue(nameof(SessionNeed), string.Join(" | ", new object?[] { op, need.Key, "a need the session mode's lane capabilities admit" }))))
            .As()
            .ToFin()
        select distinct;

    private static Fin<TResult> Marshalled<TResult>(SessionMode mode, Func<Fin<TResult>> use, Op op) =>
        from lane in op.Need(mode)
        from body in op.Need(use)
        from result in lane.Capabilities().Admits(capability: LaneCapability.Live)
            ? UiThread.Run(new UiDispatch<TResult>.Blocking(Body: body), DispatchLane.Immediate, op)
            : body()
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
                flag: snapshot.Conditions.Admits(capability: SessionCondition.Headless)
                    != lane.Capabilities().Admits(capability: LaneCapability.Live),
                False: op.InvalidInput()).ToFin()
            from capabilities in granted.AsIterable()
                .Traverse(need => need.Admit(snapshot: snapshot, mode: lane, op: op))
                .As()
                .ToFin()
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

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Admission {
    internal static Fin<Seq<T>> All<T>(ReadOnlySpan<T> values, Op key) =>
        toSeq(values.ToArray())
            .Traverse(value => key.Need(value).ToValidation())
            .As()
            .ToFin();

    internal static Fin<(T1 First, T2 Second)> Pair<T1, T2>(T1 first, T2 second, Op key)
        where T1 : class where T2 : class =>
        (
            key.Need(first).ToValidation(),
            key.Need(second).ToValidation())
        .Apply(static (one, two) => (First: one, Second: two))
        .As()
        .ToFin();
}
```

## [06]-[REGIME]

- Owner: `DocumentSpace` carries the model/page host-member axis as read, tolerance-write, and precision-write behavior columns. Kernel `ModelUnit` owns unit admission and equality; the native `LengthUnit` survives only beside the host call that reads or writes it. Closed `RegimeChange` cases expose one overloaded `Of` admission family and one mutation dispatch. The tolerance regime IS the kernel `Context` — model and page space are two `Context` values read through their own `DocumentSpace` rows — and display precision IS the kernel `DrawingPrecision`, derived from the drawing scale through the ISO 128-24 width ladder rather than authored as a digit count.
- Entry: `session.Regime(space)` reads a validated `UnitRegime`. `session.Adjust(space, change)` captures the before regime, seals one undo record around the change, proves the exact requested postcondition, and returns `RegimeOutcome` with the sealed serial.
- Law: `UnitScaling` replaces the host `bool scale` knob with `PreserveCoordinates` and `PreservePhysicalSize`. Known, custom, and native `LengthUnit` inputs all produce the private-constructed `RegimeChange.Units` case, so `AdjustLengthUnits` is the sole unit-system mutation path.
- Law: tolerance values ride the kernel `Context` whole — `Context.Of(absolute:, relative:, angle:, units: native)` is the one admission and `DocumentSpace.SetTolerances` reads `.Absolute.Value`/`.Relative.Value`/`.Angle.Value` at the host write — so this page carries no tolerance value object, no numeric admission, and no radians/degrees duplication. The three former `[ValueObject<double>]` carriers (`AbsoluteTolerance`, `RelativeTolerance`, `AngleTolerance`) are DELETED kernel-wide; every composing site reads a `Tolerance` off a `Context` lane.
- Law: display precision has ONE derivation — `DrawingPrecision.Of(scale, units).Form(key)` answers the digit count the smallest plottable feature implies through the scale, and the host `DistanceDisplayPrecision` int is the residue: a host member publishing decimal places alone, so a `Fraction` form refuses typed naming that host limit rather than flattening a denominator into digits. NAMED LOSS: the bare `Of(int digits)` entry; a caller states its scale and units and the count derives. Witness: the D72 drain — `DisplayPrecision` `[ValueObject<int>]` deletes whole and `RegimeChange.Of(DrawingScale, DrawingUnits)` is the one precision ingress.
- Law: unit postconditions compare canonical `ModelUnit` evidence, including custom name and meters-per-unit scale; tolerance postconditions compare the three `Tolerance` values off the two `Context`s. `UnitRegime` retains the native `LengthUnit` only for compensation.
- Boundary: row delegates contain the property-set statement seam required by the host API. Failed writes restore every scalar without assuming a failed unit call changed geometry; a proven unit write followed by a failed postcondition reverses the unit scaling and restores every scalar. Compensation rides the kernel `Custody.Rollback` delegate arm — its faults accumulate and join the original fault — and the shared `DocumentCommit.Sealed` decides the enclosing record's seal or rollback under `RedrawPolicy.None`.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;
using Rasm.Drawing;

namespace Rasm.Rhino.Document;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class UnitScaling {
    public static readonly UnitScaling PreserveCoordinates = new(key: 0, hostScale: false);
    public static readonly UnitScaling PreservePhysicalSize = new(key: 1, hostScale: true);

    internal bool HostScale { get; }
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
        setTolerances: static (document, context) => {
            document.ModelAbsoluteTolerance = context.Absolute.Value;
            document.ModelRelativeTolerance = context.Relative.Value;
            document.ModelAngleToleranceRadians = context.Angle.Value;
            return unit;
        },
        setPrecision: static (document, digits) => {
            document.ModelDistanceDisplayPrecision = digits;
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
        setTolerances: static (document, context) => {
            document.PageAbsoluteTolerance = context.Absolute.Value;
            document.PageRelativeTolerance = context.Relative.Value;
            document.PageAngleToleranceRadians = context.Angle.Value;
            return unit;
        },
        setPrecision: static (document, digits) => {
            document.PageDistanceDisplayPrecision = digits;
            return unit;
        });

    internal bool ModelUnits { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<UnitRegime> Read(RhinoDoc document, Op op);

    [UseDelegateFromConstructor]
    internal partial Unit SetTolerances(RhinoDoc document, Context context);

    [UseDelegateFromConstructor]
    internal partial Unit SetPrecision(RhinoDoc document, int digits);
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

    public sealed record Tolerances(Context Value) : RegimeChange;

    public sealed record Precision(DrawingPrecision Value) : RegimeChange;

    public static Fin<RegimeChange> Of(
        UnitSystem system,
        UnitScaling scaling,
        Op? key = null) {
        Op op = key.OrDefault();
        return from admission in (
                   op.Need(scaling).ToValidation(),
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
                op.Need(scaling).ToValidation(),
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
        LengthUnit units,
        Op? key = null) =>
        Context.Of(absolute: absolute, relative: relative, angle: angle, units: units)
            .ToFin()
            .Map(static value => (RegimeChange)new Tolerances(Value: value));

    public static Fin<RegimeChange> Of(DrawingScale scale, DrawingUnits units, Op? key = null) {
        Op op = key.OrDefault();
        return from admission in Admission.Pair(first: scale, second: units, key: op)
               select (RegimeChange)new Precision(Value: DrawingPrecision.Of(scale: admission.First, units: admission.Second));
    }

    internal Fin<Unit> Apply(
        RhinoDoc document,
        DocumentSpace space,
        UnitRegime before,
        Op op) {
        Fin<Unit> mutation = Switch(
            state: (Document: document, Space: space, Op: op),
            units: static (context, change) => context.Op.Catch(() => context.Document.AdjustLengthUnits(
                    modelUnits: context.Space.ModelUnits,
                    units: change.Native,
                    scale: change.Scaling.HostScale)
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(new DraftFault.HostRefused(
                    Key: context.Op, Member: nameof(RhinoDoc.AdjustLengthUnits), Detail: "answered false"))),
            tolerances: static (context, change) => context.Op.Catch(() => Fin.Succ(
                value: context.Space.SetTolerances(
                    document: context.Document,
                    context: change.Value))),
            precision: static (context, change) =>
                from digits in change.Value.Digits(key: context.Op)
                from written in context.Op.Catch(() => Fin.Succ(
                    value: context.Space.SetPrecision(document: context.Document, digits: digits)))
                select written);
        return mutation.Rollback(() => Restore(
            document: document,
            space: space,
            before: before,
            scaling: None,
            op: op));
    }

    internal Fin<bool> Matches(UnitRegime actual, Op op) =>
        Switch(
            state: (Regime: actual, Op: op),
            units: static (held, change) => Fin.Succ(held.Regime.Unit == change.Unit),
            tolerances: static (held, change) => Fin.Succ(
                held.Regime.Space.Absolute.Value == change.Value.Absolute.Value
                && held.Regime.Space.Relative.Value == change.Value.Relative.Value
                && held.Regime.Space.Angle.Value == change.Value.Angle.Value),
            precision: static (held, change) => change.Value.Digits(key: held.Op)
                .Map(digits => held.Regime.Digits == digits));

    internal Fin<Unit> Restore(
        RhinoDoc document,
        DocumentSpace space,
        UnitRegime before,
        Op op) =>
        Restore(
            document: document,
            space: space,
            before: before,
            scaling: Switch(
                units: static change => Some(change.Scaling),
                tolerances: static _ => None,
                precision: static _ => None),
            op: op);

    private static Fin<Unit> Restore(
        RhinoDoc document,
        DocumentSpace space,
        UnitRegime before,
        Option<UnitScaling> scaling,
        Op op) {
        K<Validation<Error>, Unit> units = scaling.Match(
            Some: policy => op.Catch(() => document.AdjustLengthUnits(
                    modelUnits: space.ModelUnits,
                    units: before.Native,
                    scale: policy.HostScale)
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(new DraftFault.HostRefused(
                    Key: op, Member: nameof(RhinoDoc.AdjustLengthUnits), Detail: "restore answered false"))),
            None: () => Fin.Succ(value: unit)).ToValidation();
        K<Validation<Error>, Unit> tolerances = op.Catch(() => Fin.Succ(value: space.SetTolerances(
            document: document,
            context: before.Space))).ToValidation();
        K<Validation<Error>, Unit> precision = op.Catch(() => Fin.Succ(value: space.SetPrecision(
            document: document,
            digits: before.Digits))).ToValidation();
        return (units, tolerances, precision)
            .Apply(static (_, _, _) => unit)
            .As()
            .ToFin();
    }
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record UnitRegime : IDetachedDocumentResult {
    private UnitRegime(LengthUnit native, ModelUnit unit, Context space, int digits) =>
        (Native, Unit, Space, Digits) = (native, unit, space, digits);

    internal LengthUnit Native { get; }
    public ModelUnit Unit { get; }
    public Context Space { get; }
    public int Digits { get; }

    internal static Fin<UnitRegime> Of(
        LengthUnit units,
        double absolute,
        double relative,
        double angle,
        int precision,
        Op op) =>
        (
            ModelUnit.Of(value: units, key: op).ToValidation(),
            Context.Of(absolute: absolute, relative: relative, angle: angle, units: units),
            guard(precision >= 0, (Error)new KernelFault.OutOfRange(Label: nameof(Digits), Scalar: precision, Requirement: "a non-negative digit count", Key: Some(op))).ToFin().ToValidation())
        .Apply((admittedUnit, context, _) => new UnitRegime(
            native: units,
            unit: admittedUnit,
            space: context,
            digits: precision))
        .As()
        .ToFin();
}

public sealed record RegimeOutcome : IDetachedDocumentResult {
    private RegimeOutcome(
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

    internal static RegimeOutcome Pending(
        DocumentSpace space,
        RegimeChange change,
        UnitRegime before,
        UnitRegime after) =>
        new(space: space, change: change, before: before, after: after, undoRecord: None);

    internal RegimeOutcome Seal(uint serial) => this with { UndoRecord = Some(serial) };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
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

        public Fin<RegimeOutcome> Adjust(
            DocumentSpace space,
            RegimeChange change,
            Op? key = null) {
            Op op = key.OrDefault();
            return from admission in (
                       op.Need(session).ToValidation(),
                       op.Need(space).ToValidation(),
                       op.Need(change).ToValidation())
                       .Apply(static (scope, axis, request) => (
                           Scope: scope,
                           Axis: axis,
                           Request: request))
                       .As()
                       .ToFin()
                   from outcome in admission.Scope.Demand(
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
                                   from matches in admission.Request.Matches(actual: observed, op: op)
                                   from exact in guard(flag: matches, False: op.InvalidResult()).ToFin()
                                   select observed).Rollback(() => admission.Request.Restore(
                                   document: document,
                                   space: admission.Axis,
                                   before: before,
                                   op: op))
                               select RegimeOutcome.Pending(
                                   space: admission.Axis,
                                   change: admission.Request,
                                   before: before,
                                   after: after),
                           project: Fin.Succ,
                           stamp: static (result, serial) => result.Seal(serial: serial),
                           op: op),
                       key: op,
                       needs: SessionNeed.Mutation(custody: UndoCustody.Recorded, redraw: RedrawPolicy.None).ToArray())
                   select outcome;
        }
    }
}
```

## [07]-[REGIME_TEXT]

- Owner: `UnitDialect` carries every catalogued host parser preset as a behavior row; `AngleGrammar` rows own bare-number angle interpretation and land canonical radians. `UnitForm` re-closes the foreign formatting ordinal. `UnitText` `[Union]` carries encoded length/scale/angle text and detached semantic evidence on one correspondence owner.
- Entry: `session.Text(space, text)` resolves the live regime and crosses one `UnitText` case. Length text crosses to semantic evidence and the same returned carrier crosses back to exact text; scale and angle values return typed unsupported faults where the catalog exposes no verified inverse formatter.
- Law: dialect rows return the host preset statics — process-shared constants whose `Dispose` is inert — and never mutate one: a preset setter writes through to the shared native object and poisons every later parse, and `new StringParserSettings()` seeds a different ambiguous grammar, so neither is a dialect base.
- Law: a length parse admits only a whole-string parse of a set value — `parsedAll` and `!IsUnset()` gate together — and converts on egress through `Length(LengthUnit)` into regime units. Reverse crossing scales the detached value from its captured `ModelUnit` into the current regime before formatting and preserves its admitted dialect. A scale parse gates the scale and both terms before detaching each magnitude and `LengthUnit`, so unitless, custom, mixed-unit, and same-unit ratios remain distinguishable.
- Law: an angle phrase parses through `StringParser.ParseAngleExpressionRadians` or `ParseAngleExpressionDegrees` under its `AngleGrammar` row; the degrees row converts through `RhinoMath.ToRadians` at the seam, so `UnitText.AngleValueCase` always carries canonical radians and no consumer re-derives the angular unit.
- Law: every parsed `LengthValue`/`ScaleValue` is a native disposable bracketed inside its arm — the `using` statement blocks are the `[CAPSULE_OWNER]` exemption, stated once here; `UnitText` leaves as detached evidence or text, never as a live parse handle. Exact length crossing round-trips through `LengthValue.LengthString`.
- Law: the locale display and unit-name legs are DELETED — `UnitText.Display`, `UnitText.Name`, their two private cases, the rendered projection, and the four label vocabularies proved zero consumers corpus-wide. NAMED LOSS: a locale-rendered readout correspondence; a future consumer lands it as one case plus one `LabelTrait` capability row, and the host members (`Localization.FormatNumber`/`UnitSystemName`) re-enter through that admission rather than surviving as reachable-by-nobody arms.
- Boundary: the sheet-standard unit declaration is `Rasm.Drawing`'s `DrawingUnits` and the model regime is this page's — a title block reads the sheet's row and never this correspondence; the host parse/render estate stays because its grammar is the host's own locale surface.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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
            inputType: typeof(ScaleValueCase),
            outputType: typeof(ScaleTextCase))),
        angleTextCase: static (context, text) => text.Grammar.Parse(text: text.Text, op: context.Op)
            .Map(static radians => (UnitText)new AngleValueCase(radians: radians)),
        angleValueCase: static (context, _) => Fin.Fail<UnitText>(error: context.Op.Unsupported(
            inputType: typeof(AngleValueCase),
            outputType: typeof(AngleTextCase))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RegimeText {
    extension(DocumentSession session) {
        public Fin<UnitText> Text(DocumentSpace space, UnitText text, Op? key = null) {
            Op op = key.OrDefault();
            return from request in op.Need(text)
                   from regime in session.Regime(space: space, key: op)
                   from crossed in request.Cross(regime: regime, key: op)
                   select crossed;
        }
    }
}
```

## [08]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]            | [OWNER]               | [FORM]                            | [ENTRY]                      |
| :-----: | :------------------- | :-------------------- | :-------------------------------- | :--------------------------- |
|  [01]   | admission fault      | `DraftFault`          | banded closed family              | hooks / `AcceptValidated`    |
|  [02]   | document identity    | `DocKey`              | positive generated value          | `Of` / `Census`              |
|  [03]   | lifecycle evidence   | `SessionSnapshot`     | phase + condition set             | `DocumentSession.Snapshot`   |
|  [04]   | capability policy    | `SessionNeed`         | four-set data rows                | `DocumentSession.Demand`     |
|  [05]   | source admission     | `SessionSource`       | flat closed source family         | `DocumentSession.Of`         |
|  [06]   | scoped lifetime      | `DocumentSession`     | retained lease over `SessionGate` | `DocumentSession.Of`         |
|  [07]   | space regime         | `DocumentSpace`       | model/page behavior rows          | `Regime` / `Adjust`          |
|  [08]   | regime mutation      | `RegimeChange`        | units/context/precision union     | `RegimeChange.Of` / `Adjust` |
|  [09]   | unit correspondence  | `UnitText`            | encoded/semantic union            | `UnitText.Length` / `Text`   |
|  [10]   | worksession topology | `WorksessionSnapshot` | detached active/reference rows    | `Worksession` / `FileOf`     |
|  [11]   | worksession custody  | `WorksessionOp`       | scripted attach/detach/reload     | `Worksession` / `WorksessionOutcome` |

## [09]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
