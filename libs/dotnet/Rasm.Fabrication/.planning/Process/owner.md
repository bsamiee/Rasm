# [RASM_FABRICATION_OWNER]

`Fabrication` admits one complete production request and runtime, dispatches one `FabricationPolicy`, and returns one `FabricationResult` whose evidence projects content identity and lineage without replaying plane logic. `Process/atoms` holds the acyclic vocabulary floor this spine folds over, and `Run` remains the terminal consumer of plane kernels.

`FabricationInput` carries the columns EVERY policy reads; a column exactly one arm reads rides that arm's own policy case, so the aggregate admits one geometry, identity, and egress contract instead of eighteen slots most planes leave empty. `FabricationInput.Admit` proves process-machine-strategy-dialect compatibility, geometry presence, and requested egress before the `FabricationPolicy.Egress` dispatch, and `FabricationPolicy.Consumed` is the one projection the run spine folds for consumed ancestry.

The `Rasm.Element` `CanonicalWriter` is the one byte codec every preimage composes — MUTABLE-FLUENT, each primitive returning the same writer, so a discarded return is the ordinary spelling and no site copies a result — and `FabricationCanon` is the one facade over it, framing through `Coords`, `Basis`, `Maybe`, `Rows`, and `Discriminant` and closing through `Keyed` and `Ordered`. `ContentKey.Of` length-frames `EgressKind` ahead of those bytes, so equal payloads in different families stay distinct.

`Receipt<TEvidence>` is the settled-receipt carrier a lane output carrying a content key, evidence, band, or stamp seats on — the lane sweep landed, and every surviving `*Receipt` record carries its own evidence or stamp under its RULINGS row. `RunEvidence` is the run spine's own instance of that spine, and `QuantityArrow` the one dimension-text entry a plane outside `Process` reaches, parameterized by the fault its own plane raises.

## [01]-[INDEX]

- [02]-[CONTENT_KEY]: `EgressKind`, `ContentKey`, `DeliveryTarget`, `EgressRequest`, `EgressContract`.
- [03]-[POLICY]: `FabricationPolicy` and `PostSource`.
- [04]-[RECEIPT]: `FabricationResult`, `Receipt<TEvidence>`, `RunProvenance`, `RunEvidence`, `RunLineage`.
- [05]-[RUN_FOLD]: `FabricationInput`, `RunStage`, `FabricationRuntime`.
- [06]-[RUN_DISPATCH]: `FabricationCanon`, `QuantityArrow`, `Fabrication.Run`, `Fabrication.Lineage`, and the provenance fold.

## [02]-[CONTENT_KEY]

- Owner: `EgressKind` owns the artifact family vocabulary; `ContentKey` owns the one mint; `EgressRequest` owns what a caller asked for; `EgressContract` owns what a policy can answer.
- Auto: `ContentKey.Of` length-frames the `EgressKind` key ahead of the payload, so equal bytes under different families stay distinct and no second mint exists.
- Law: an `EgressContract` states its admissible alternatives and its CARDINALITY CEILING alone — a floor is dead under every landed policy because a caller asking for nothing is always admissible, and the produced-versus-requested proof at `FabricationResult.Evidence` is what enforces coverage. `EgressContract.None` is the shared row for a policy producing no artifact.
- Boundary: `EgressKind` federates to the Persistence `ArtifactKind` rows at the content-key boundary by VALUE, never a type reference.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Text;
using System.Threading;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using Microsoft.Extensions.Caching.Hybrid;
using NodaTime;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Observers;
using QuikGraph.Algorithms.Search;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Fabrication.Additive;
using Rasm.Fabrication.Documentation;
using Rasm.Fabrication.Forming;
using Rasm.Fabrication.Ingress;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Nesting;
using Rasm.Fabrication.Posting;
using Rasm.Fabrication.Toolpath;
using Rasm.Fabrication.Verify;
using Rasm.Meshing;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

// Kernel hook rail closed over the `Process/telemetry#HOOK_RAIL` roster/fact/owner triple — the spine reads the
// domain name rather than the three-parameter spelling, and `FabricationHooks.Live` is its one mint.
using FabricationRail = Rasm.Domain.HookRail<Rasm.Fabrication.Process.FabricationPoint, Rasm.Fabrication.Process.FabricationHookFact, Rasm.Domain.TelemetrySource>;

namespace Rasm.Fabrication.Process;

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
// --- [CONTENT_KEY]
[SmartEnum<string>]
public sealed partial class EgressKind {
    public static readonly EgressKind CutProgram = new("cutprogram");
    public static readonly EgressKind Placement = new("placement");
    public static readonly EgressKind Remnant = new("remnant");
    public static readonly EgressKind Cli = new("cli");
    public static readonly EgressKind ThreeMf = new("threemf");
    public static readonly EgressKind Nc1 = new("nc1");
    public static readonly EgressKind StockSnapshot = new("stock-snapshot");
    public static readonly EgressKind Traveler = new("traveler");
    // Two arms address this family: `Verify/audit` `Audit.Preflight` keys its `Receipt<AuditEvidence>` here over the
    // slice stack and policy it read, and `Spec/manufacturability` `Manufacturability.Assess` keys its
    // `Receipt<DfmReport>` here over the DfM request it read — a preflight verdict and a producibility verdict are
    // one egress family read at two planes. Both address their REQUEST, never their findings.
    public static readonly EgressKind QualityRecord = new("quality-record");
    public static readonly EgressKind FlatPattern = new("flat-pattern");
    // Produced by the `FormSource.Tube` and `FormSource.Roll` dispatch arms through `TubeProgram.Apply`; the
    // `FormSource.Sheet` arm mints `FlatPattern` instead, which is why the forming contract reads the source.
    public static readonly EgressKind BendProgram = new("bend-program");
    public static readonly EgressKind WeldPlan = new("weld-plan");
    public static readonly EgressKind ScanVectors = new("scan-vectors");
    public static readonly EgressKind Plan = new("plan");
    public static readonly EgressKind DigitalProductPassport = new("digital-product-passport");
}

public sealed record ContentKey {
    private ContentKey(EgressKind kind, UInt128 digest) => (Kind, Digest) = (kind, digest);

    public EgressKind Kind { get; }
    public UInt128 Digest { get; }

    // Exemption: span framing is a measured byte kernel. Kind is identity-bearing, so it joins the preimage
    // length-framed ahead of the payload; hashing the payload alone collides every egress family over equal bytes.
    public static ContentKey Of(EgressKind kind, ReadOnlySpan<byte> canonicalBytes) {
        int keyLength = Encoding.UTF8.GetByteCount(kind.Key);
        Span<byte> preimage = new byte[(sizeof(int) * 2) + keyLength + canonicalBytes.Length];
        BinaryPrimitives.WriteInt32LittleEndian(preimage, keyLength);
        _ = Encoding.UTF8.GetBytes(kind.Key, preimage[sizeof(int)..]);
        BinaryPrimitives.WriteInt32LittleEndian(preimage[(sizeof(int) + keyLength)..], canonicalBytes.Length);
        canonicalBytes.CopyTo(preimage[((sizeof(int) * 2) + keyLength)..]);
        return new ContentKey(kind, ContentHash.Of(preimage));
    }

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => writer.Discriminant(Kind).U128(Digest);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DeliveryTarget {
    private DeliveryTarget() { }

    public sealed record InProcess : DeliveryTarget;
    public sealed record Artifact(Uri Location) : DeliveryTarget;
    public sealed record Bundle(Uri Location, string Member) : DeliveryTarget;
}

[ComplexValueObject]
public sealed partial class EgressRequest {
    public Set<EgressKind> Kinds { get; }
    public DeliveryTarget Target { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Set<EgressKind> kinds,
        ref DeliveryTarget target) {
        if (!target.Switch(
            state: kinds,
            inProcess: static _ => true,
            artifact: static (requested, value) => !requested.IsEmpty && value.Location is { IsAbsoluteUri: true },
            bundle: static (requested, value) => !requested.IsEmpty && value.Location is { IsAbsoluteUri: true }
                && Witness.Keyed(value.Member)))
            validationError = new ValidationError("egress-request");
    }

    public static Fin<EgressRequest> Admit(Set<EgressKind> kinds, DeliveryTarget target) =>
        Validate(kinds, target, out EgressRequest request).Admitted(request);
}

// The ceiling is the whole contract: a floor is vacuous because asking for no artifact is always admissible, and
// coverage of what WAS asked for is proved against produced keys at `FabricationResult.Evidence`.
public sealed record EgressContract(Set<EgressKind> Alternatives, int Maximum) {
    public static readonly EgressContract None = new(Set<EgressKind>(), 0);

    public bool Admits(EgressRequest request) =>
        (request.Kinds - Alternatives).IsEmpty && request.Kinds.Count <= Maximum;
}
```

## [03]-[POLICY]

- Owner: `FabricationPolicy` owns the closed production-modality family and the payload only its own plane reads; `PostSource` owns the three shapes a posted program lowers from.
- Cases: `Cam` carries its cell, `Nest` its inventory and prior plan, `Verify` its residual and snapshots, `Derive` its capability verdict, `HiddenLine` its view, and `Form` its `FormSource` — the closed sheet/tube/roll family, each arm carrying its own admitted run and its own machine envelope — so an eighteen-column aggregate whose columns most planes leave empty becomes eleven columns every plane reads.
- Auto: `Egress` declares admissible alternatives and cardinality once per case, and `Consumed` projects consumed ancestry once; both are generated total dispatches, so a new case cannot silently inherit a neighbour's contract.
- Law: `Egress` is a per-CASE fact rather than a per-PLANE one — `HiddenLine` and `Document` both own the documentation plane while answering `None` and `{Traveler, DigitalProductPassport}`, and `Inspect` and `Verify` split the verify plane the same way — so the arm is what states the contract and a plane-keyed row cannot. Nine arms answer a constant and the `Form` arm answers a PAYLOAD projection, exactly as `Fits` reads the case payload on `Cam`, `Post`, and `Document`: an unfold and a bend program are two artifact families under one modality, and one union over both would admit a caller asking a press brake for a bend program.
- Growth: a production modality adds one policy case, one `FabricationResult` case, and one dispatch arm — or, where an existing case already closes the family (`Form` over `FormSource`, `TubeFormed` over `TubeResult`), one row on that family and one arm on its own dispatch, with the outer cases untouched; an artifact adds one `EgressKind` row, one entry on the owning `Egress` arm, and its enrollment counterpart.
- Boundary: a payload type is named here and DECLARED at its owning plane, so this union imports names and never plane behaviour.

```csharp signature
// --- [POLICY]
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FabricationPolicy {
    private FabricationPolicy() { }

    public sealed record HiddenLine(ProjectionPolicy Policy, ProjectionDir View) : FabricationPolicy;
    public sealed record Cam(
        CutStrategy Strategy,
        CamPassPolicy Pass,
        CutterForm Cutter,
        CellPolicy Cell,
        EngagementPolicy Engagement,
        Option<RobotCell> Robot) : FabricationPolicy;
    public sealed record Nest(NestPolicy Nesting, Seq<Stock> Inventory, Option<NestPlan> Plan) : FabricationPolicy;
    public sealed record Additive(AdditivePolicy Policy) : FabricationPolicy;
    public sealed record Verify(
        VerifyPolicy Policy,
        Option<ResidualStock> Residual,
        Seq<StockSnapshot> Snapshots) : FabricationPolicy;
    public sealed record Inspect(InspectPolicy Policy) : FabricationPolicy;
    public sealed record Post(PostSource Source, PostDialect Dialect, PostPolicy Policy) : FabricationPolicy;
    public sealed record Document(
        Seq<FabricationResult> Results,
        TravelerReceiptCorpus Corpus,
        Option<PostDialect> Dialect) : FabricationPolicy;
    public sealed record Derive(
        AdmittedComponent Component,
        DerivePolicy Policy,
        Option<CapabilityVerdict> Capability) : FabricationPolicy;
    // The forming plane routes by SOURCE, and the machine envelope rides the source arm rather than this case:
    // a press brake, a tube bender, and a section roll state incompatible capacity axes, so one envelope column
    // here would have routed two of the three modalities through a station that cannot form them.
    public sealed record Form(FormSource Source) : FabricationPolicy;

    // One artifact correspondence distinguishes supported alternatives from request cardinality;
    // `FabricationResult.Evidence` proves every requested kind against actual produced keys.
    public EgressContract Egress => Switch(
        hiddenLine: static _ => EgressContract.None,
        cam: static _ => EgressContract.None,
        nest: static _ => new EgressContract(Set(EgressKind.Placement), 1),
        additive: static _ => new EgressContract(Set(EgressKind.ThreeMf, EgressKind.Cli, EgressKind.ScanVectors), 3),
        verify: static _ => new EgressContract(Set(EgressKind.Remnant, EgressKind.StockSnapshot), 2),
        inspect: static _ => EgressContract.None,
        post: static _ => new EgressContract(Set(EgressKind.CutProgram, EgressKind.Nc1, EgressKind.Cli), 1),
        document: static _ => new EgressContract(Set(EgressKind.Traveler, EgressKind.DigitalProductPassport), 2),
        derive: static _ => new EgressContract(Set(EgressKind.Plan, EgressKind.WeldPlan), 2),
        // The forming contract is a PAYLOAD projection like `Fits`, not a constant: the unfold arm answers
        // `FlatPattern` and the two tube arms answer `BendProgram`, and a union of the two over one case would
        // have admitted a caller asking a press brake for a bend program the arm cannot mint.
        form: static policy => policy.Source.Egress);

    // Consumed ancestry is the POLICY's fact, because only the arm holding a prior artifact knows it consumed one;
    // the run spine folds this beside the input's own parent and source keys and hard-codes no plane's slot.
    public Seq<ContentKey> Consumed => Switch(
        hiddenLine: static _ => Seq<ContentKey>(),
        cam: static _ => Seq<ContentKey>(),
        nest: static policy => policy.Plan.ToSeq().Map(static plan => plan.Key),
        additive: static _ => Seq<ContentKey>(),
        verify: static policy => policy.Residual.ToSeq().Map(static stock => stock.Key)
            + policy.Snapshots.Map(static snapshot => snapshot.Key),
        inspect: static _ => Seq<ContentKey>(),
        post: static _ => Seq<ContentKey>(),
        document: static policy => policy.Corpus.Records.Map(static record => record.Key)
            + policy.Corpus.DigitalProductPassport.ToSeq(),
        derive: static _ => Seq<ContentKey>(),
        form: static _ => Seq<ContentKey>());

    public bool Fits(ProcessKind process) => Switch(
        state: process,
        hiddenLine: static (_, _) => true,
        cam: static (value, policy) => value.Modality.Admits(policy.Strategy),
        nest: static (_, _) => true,
        additive: static (value, _) => value.Modality == ProcessModality.Additive,
        verify: static (_, _) => true,
        inspect: static (_, _) => true,
        post: static (value, policy) => policy.Dialect.Admits(value.Modality),
        document: static (value, policy) => policy.Dialect.Map(dialect => dialect.Admits(value.Modality)).IfNone(true),
        derive: static (_, _) => true,
        form: static (value, _) => value.Modality == ProcessModality.Formed);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PostSource {
    private PostSource() { }

    public sealed record Motion(FabricationResult.Motion Value) : PostSource;
    public sealed record Placement(FabricationResult.Placement Value) : PostSource;
    public sealed record Specialized(SpecializedToolpathEnvelope Value) : PostSource;
}
```

## [04]-[RECEIPT]

- Owner: `FabricationResult` owns plane-specific evidence; `Receipt<TEvidence>` owns the settled-receipt spine every lane output carries; `RunEvidence` owns the run spine's own settled receipt; `RunProvenance` owns the lineage walk's named outputs; `RunLineage` owns the projection a caller reads off a settled run.
- Cases: each `FabricationResult` case names only the evidence its own plane produced, so `Keys` and `Evidence` are one arm per case rather than a re-spelling of every slot.
- Entry: `FabricationResult.Evidence(input, consumed)` seals the receipt and proves the produced keys cover the request; `Receipt<TEvidence>.CanonicalBytes(writer, frame)` is the ONE preimage facade a lane payload frames itself through.
- Law: `Receipt<TEvidence>` makes `Key`, `Concern`, and `Stamped` REQUIRED, so a lane output carrying none of them is not a receipt at all. Lane pages seat content key, evidence band, and stamp on this carrier and keep only their lane evidence as `TEvidence` — the sweep landed, and each surviving `*Receipt` record is individually ratified in the folder RULINGS; `RunEvidence` is the run spine's own instance of the same spine and keeps its name.
- Receipt: `RunEvidence` carries requested and produced artifacts, motion diagnostics, inspection outcomes, verification state, content keys, the ancestral roots its provenance walk reached, and the GENERATION depth that walk measured.
- Boundary: consumers preserve field order while the `Rasm.Element` `CanonicalWriter` owns ordinal, IEEE-754 double with `-0.0` and NaN normalization, `U128`, `I64`, length-prefixed UTF-8, and presence-tag framing; a second byte codec beside it is the deleted form.

```csharp signature
// --- [RECEIPT]
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FabricationResult {
    private FabricationResult() { }

    public sealed record HiddenLineResult(ProjectionEvidence Projection, Seq<ContentKey> Subjects) : FabricationResult;
    public sealed record Motion(Seq<Move> Moves, Seq<MotionDirective> Directives, MotionEvidence Evidence, Seq<ContentKey> Subjects) : FabricationResult {
        public Seq<Arr<double>> Joints => Evidence.Joints;
        public double Duration => Evidence.Cycle.TotalSeconds;
        public Seq<string> CellCode => Evidence.ControllerCode;
    }
    public sealed record Placement(Seq<PartTransform> Parts, double Utilization, int Unplaced, Seq<Remnant> Remnants, ContentKey Key) : FabricationResult;
    public sealed record AdditiveResult(Seq<Move> Moves, int Layers, Seq<ContentKey> Artifacts) : FabricationResult;
    public sealed record VerificationResult(
        ResidualStock Residual,
        Seq<StockSnapshot> Snapshots,
        Seq<GougeWitness> Gouges,
        double UncutVolume,
        double OvercutVolume,
        double AirCutRatio,
        double VolumeTolerance) : FabricationResult {
        // Overcut is an accumulated voxel volume; exact-zero equality never holds, so the verdict gates on the
        // tolerance the verifier admits from its own voxel edge length.
        public bool Clean => Gouges.IsEmpty && OvercutVolume <= VolumeTolerance;
    }
    public sealed record InspectionResult(Seq<InspectionFeature> Features, Seq<ContentKey> Subjects) : FabricationResult;
    public sealed record PostedProgram(Seq<string> Blocks, ContentKey Key) : FabricationResult;
    public sealed record TravelerDocument(TravelerArtifact Artifact) : FabricationResult {
        public ContentKey Key => Artifact.Key;
        public Seq<ContentKey> Consumed => Artifact.Consumed;
        public Seq<ContentKey> Produced => Artifact.Produced;
        public Option<ContentKey> DigitalProductPassport => Artifact.DigitalProductPassport;
    }
    public sealed record FabricationPlan(
        DerivationStage Ceiling,
        Seq<ProcessKind> Routing,
        Seq<MachineMatch> Routes,
        Seq<PlannedStep> Steps,
        OperationTopology Topology,
        Option<CapabilityRequirement> Requirement,
        Option<Receipt<LotEvidence>> LotSchedule,
        Option<CapabilityVerdict> Capability,
        Set<EgressKind> RequestedArtifacts,
        Seq<ContentKey> Artifacts,
        ContentKey Key) : FabricationResult;
    public sealed record FormedResult(Arr<Loop> FlatPattern, Seq<BendStep> Bends, double SpringbackMaxDeg, ContentKey Key) : FabricationResult;
    // Tube forming produces evidence a flat pattern shares NO column with — canonical bend coordinates, tooling
    // rows, deformation witnesses, a pass schedule — so widening `FormedResult` would have made four of its
    // columns `Option` for every sheet run and broken the `Process/derivation` fact table that reads all four.
    // `TubeResult` is already the closed family over the three tube modalities and carries its own key on every
    // arm, so this case takes it WHOLE: the discriminant stays recoverable from the value, and the next tube
    // modality is one row there rather than a new result case here. The run spine routes `Formed` and `Rolled`;
    // `Coped` reaches a caller through `TubeProgram.Apply` directly and lands on the same case when it does.
    public sealed record TubeFormed(TubeResult Outcome) : FabricationResult;

    // The result's own key census — every content key a case produced or carries as its subject face, the set a
    // pricing basis, traveler gather, or provenance fold correlates against; per-case Subjects columns stay the
    // caller-seeded halves this projection composes.
    public Seq<ContentKey> Keys => Map(
        hiddenLineResult: static value => value.Subjects,
        motion: static value => value.Subjects,
        placement: static value => Seq(value.Key),
        additiveResult: static value => value.Artifacts,
        verificationResult: static value => Seq(value.Residual.Key).Concat(value.Snapshots.Map(static row => row.Key)),
        inspectionResult: static value => value.Subjects,
        postedProgram: static value => Seq(value.Key),
        travelerDocument: static value => Seq(value.Key),
        fabricationPlan: static value => Seq(value.Key).Concat(value.Artifacts),
        formedResult: static value => Seq(value.Key),
        tubeFormed: static value => Seq(value.Outcome.Key));

    // Each arm names only the evidence its own case owns; unnamed slots keep the seeded request and consumed ancestry,
    // so a new result case is one arm rather than a re-spelling of every slot.
    public Fin<RunEvidence> Evidence(FabricationInput input, Seq<ContentKey> consumed) {
        RunEvidence evidence = Switch(
            state: RunEvidence.Seed(this, input, consumed),
            hiddenLineResult: static (seed, result) => seed with { Consumed = seed.Consumed + result.Subjects },
            motion: static (seed, result) => seed with {
                Consumed = seed.Consumed + result.Subjects,
                Warnings = result.Evidence.Warnings,
            },
            placement: static (seed, result) => seed with { Produced = Seq(result.Key) },
            additiveResult: static (seed, result) => seed with { Produced = result.Artifacts },
            verificationResult: static (seed, result) => seed with {
                Produced = Seq(result.Residual.Key) + result.Snapshots.Map(static snapshot => snapshot.Key),
                Verified = Some(result.Clean),
            },
            inspectionResult: static (seed, result) => seed with {
                Consumed = seed.Consumed + result.Subjects,
                Inspections = result.Features,
            },
            postedProgram: static (seed, result) => seed with { Produced = Seq(result.Key) },
            travelerDocument: static (seed, result) => seed with {
                Consumed = seed.Consumed + result.Consumed,
                Produced = Seq(result.Key) + result.Produced + result.DigitalProductPassport.ToSeq(),
            },
            fabricationPlan: static (seed, result) => seed with { Produced = Seq(result.Key) + result.Artifacts },
            formedResult: static (seed, result) => seed with { Produced = Seq(result.Key) },
            tubeFormed: static (seed, result) => seed with { Produced = Seq(result.Outcome.Key) });
        Set<EgressKind> missing = input.Egress.Kinds - toSet(evidence.Produced.Map(static key => key.Kind));
        return missing.IsEmpty
            ? Fin.Succ(evidence)
            : Fin.Fail<RunEvidence>(new KernelFault.InvalidValue("owner", $"egress:missing:{string.Join(',', missing.Map(static kind => kind.Key))}"));
    }
}

// The settled-receipt spine, generalized off `RunEvidence`: `TEvidence` is the ONLY varying column, so a lane
// receipt declares its own evidence and inherits plane, key, ancestry, band, and stamp instead of re-spelling them.
public sealed record Receipt<TEvidence>
    where TEvidence : notnull {
    public required TEvidence Evidence { get; init; }
    public required FabConcern Concern { get; init; }
    public required ContentKey Key { get; init; }
    public Seq<ContentKey> Consumed { get; init; } = Seq<ContentKey>();
    public Seq<ContentKey> Produced { get; init; } = Seq<ContentKey>();
    public Seq<RunWarning> Warnings { get; init; } = Seq<RunWarning>();
    public required Instant Stamped { get; init; }
    public Option<bool> Verified { get; init; }

    // The ONE preimage facade: the owning plane frames ahead of the payload and the lane's own frame writes its
    // evidence through the SAME writer, so a receipt never carries a second codec.
    public CanonicalWriter CanonicalBytes(
        CanonicalWriter writer, Func<TEvidence, CanonicalWriter, CanonicalWriter> frame) =>
        frame(Evidence, writer.Discriminant(Concern))
            .Rows(Consumed, static (row, key) => key.CanonicalBytes(row))
            .Rows(Produced, static (row, key) => key.CanonicalBytes(row));
}

// The provenance walk's own outputs, named: the ancestral frontier the child-to-parent walk terminated on and the
// generation depth it measured per key. The graph itself is a transient fold and never leaves the operation.
public sealed record RunProvenance(Seq<ContentKey> Roots, Map<ContentKey, int> Generation) {
    public static readonly RunProvenance Empty = new(Seq<ContentKey>(), Map<ContentKey, int>());

    public int Depth => Generation.Values.Fold(0, static (deepest, row) => Math.Max(deepest, row));
}

public sealed record RunEvidence {
    private RunEvidence(
        FabricationResult result,
        FabricationPolicy policy,
        ProcessKind process,
        Machine machine,
        EgressRequest request,
        Seq<ContentKey> parentRuns,
        Seq<ContentKey> sources,
        Option<ContentKey> materialCertificate,
        Seq<ContentKey> consumed) =>
        (Result, Policy, Process, Machine, Request, ParentRuns, Sources, MaterialCertificate, Consumed) =
        (result, policy, process, machine, request, parentRuns, sources, materialCertificate, consumed);

    public static RunEvidence Seed(FabricationResult result, FabricationInput input, Seq<ContentKey> consumed) =>
        new(result, input.Policy, input.Process, input.Machine, input.Egress,
            input.ParentRuns, input.Sources, input.MaterialCertificate, consumed);

    public FabricationResult Result { get; }
    public FabricationPolicy Policy { get; }
    public ProcessKind Process { get; }
    public Machine Machine { get; }
    public EgressRequest Request { get; }
    public Seq<ContentKey> ParentRuns { get; }
    public Seq<ContentKey> Sources { get; }
    public Option<ContentKey> MaterialCertificate { get; }
    public Seq<ContentKey> Consumed { get; init; }
    public Seq<ContentKey> Produced { get; init; } = Seq<ContentKey>();
    public Seq<RunWarning> Warnings { get; init; } = Seq<RunWarning>();
    public Seq<InspectionFeature> Inspections { get; init; } = Seq<InspectionFeature>();
    public Option<bool> Verified { get; init; }
    public RunProvenance Provenance { get; init; } = RunProvenance.Empty;
}

public sealed record RunLineage(
    FabricationPolicy Policy,
    ProcessKind Process,
    Machine Machine,
    Seq<ContentKey> Parents,
    Seq<ContentKey> Sources,
    Option<ContentKey> MaterialCertificate,
    Seq<ContentKey> Consumed,
    Seq<ContentKey> Produced,
    Seq<ContentKey> Roots,
    Map<ContentKey, int> Generation);
```

## [05]-[RUN_FOLD]

- Owner: `FabricationInput` owns the columns EVERY policy reads — geometry, markings, edge-preparation demand, routing axes, ancestry, and the egress request; `RunStage` owns the declared governance boundaries; `FabricationRuntime` owns clock, cancellation, progress, tap, the kernel hook rail, and the memo tier.
- Entry: `FabricationInput.Admit` accumulates its seven independent gates applicatively, so a caller reads every violated invariant at once rather than the first.
- Auto: run governance is one read per `RunStage` boundary; a requested cancellation lowers `Errors.Cancelled`, while a live run publishes that row's declared fraction.
- Receipt: `Run`'s terminal fold fires `FabricationFact.Run.Of(evidence, elapsed)` through `FabricationRuntime.Telemetry` with elapsed read from `Clock`, projecting duration, artifact kinds, and warnings onto `rasm.fabrication.run.duration`, `rasm.fabrication.run.artifacts`, and `rasm.fabrication.run.warnings` through `Process/telemetry#FACT_PROJECTION` as kind `run`.
- Boundary: `Run` fires the admission veto before dispatch, the per-key egress-mint veto, the stage-advance and verify-verdict points off the settled result, and the delivery hand-off after evidence — all five through the ONE kernel `HookRail` the runtime carries (`Process/telemetry#HOOK_RAIL`), so any app observes, vetoes, or replays the spine without a code edit — and domain kernels stay tap-free: facts fire only where receipts settle on the run spine. `FabricationRuntime.Admit` binds the default rail on the rail rather than collapsing it with `??`, because seating every point is itself a refusable composition.

```csharp signature
// --- [RUN_FOLD]
[ComplexValueObject]
public sealed partial class FabricationInput {
    public FabricationPolicy Policy { get; }
    public Option<MeshSpace> Model { get; }
    public Arr<Loop> Profiles { get; }
    public Arr<Loop> Keepouts { get; }

    // Markings ride the run BESIDE the loops rather than being dropped at admission: the ingress lowers part marks,
    // heat numbers, and shop tags off the drawing, and a traveler or a posted program that cannot see them re-parses
    // an entity sweep it has no access to. Tags is the ingress owner's own keyed fold, so both consumers key by name
    // through one grouping and a marking-free run reads an empty map rather than an absent capability.
    public Arr<ProfileMarking> Markings { get; }

    // Edge preparation is a fact of the ADMITTED GEOMETRY, not a policy choice: DSTV states the groove an edge is cut
    // to at the contour vertex that carries it, and dropping that at admission left a CAM run squaring the joint a
    // downstream weld was designed around. The demand rides here beside the loops for the same reason markings do —
    // the toolpath, posting, documentation, and joining planes all read it — while the `Toolpath/bevel` law that
    // GOVERNS the cut stays the engagement's `Option` column, so the two answer different questions and the folder
    // ruling against a demand flag beside the law is untouched.
    public Arr<EdgePreparation> Preparations { get; }

    public ProcessKind Process { get; }
    public Machine Machine { get; }
    public Seq<ContentKey> ParentRuns { get; }
    public Seq<ContentKey> Sources { get; }
    public Option<ContentKey> MaterialCertificate { get; }
    public EgressRequest Egress { get; }

    public Map<string, Arr<ProfileMarking>> Tags => ProfileImport.TagsOf(Markings);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FabricationPolicy policy,
        ref Option<MeshSpace> model,
        ref Arr<Loop> profiles,
        ref Arr<Loop> keepouts,
        ref Arr<ProfileMarking> markings,
        ref Arr<EdgePreparation> preparations,
        ref ProcessKind process,
        ref Machine machine,
        ref Seq<ContentKey> parentRuns,
        ref Seq<ContentKey> sources,
        ref Option<ContentKey> materialCertificate,
        ref EgressRequest egress) {
        if (model.IsNone && profiles.IsEmpty)
            validationError = new ValidationError("fabrication-input:geometry");
    }

    public static Fin<FabricationInput> Admit(
        FabricationPolicy policy,
        Option<MeshSpace> model,
        Arr<Loop> profiles,
        Arr<Loop> keepouts,
        Arr<ProfileMarking> markings,
        Arr<EdgePreparation> preparations,
        ProcessKind process,
        Machine machine,
        Seq<ContentKey> parentRuns,
        Seq<ContentKey> sources,
        Option<ContentKey> materialCertificate,
        EgressRequest egress) =>
        (AdmissionSlots.Gate(model.IsSome || !profiles.IsEmpty, FabConcern.Process, "fabrication-input:geometry", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(profiles.ForAll(static loop => loop.Closed),
             FabConcern.Process, "fabrication-input:profiles", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(keepouts.ForAll(static loop => loop.Closed),
             FabConcern.Process, "fabrication-input:keepouts", FabricationFault.Inadmissible),
         // A demand naming no admitted profile has no edge to prepare, so it is a request defect rather than a lane
         // the toolpath silently skips.
         AdmissionSlots.Gate(preparations.ForAll(row => row.Profile >= 0 && row.Profile < profiles.Count),
             FabConcern.Process, "fabrication-input:preparations", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(machine.Admits(process), FabConcern.Process, "fabrication-input:process-machine", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(policy.Fits(process), FabConcern.Process, "fabrication-input:policy", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(policy.Egress.Admits(egress), FabConcern.Process, "fabrication-input:egress", FabricationFault.Inadmissible))
            .Apply(static (_, _, _, _, _, _, _) => unit)
            .As()
            .ToFin()
            .Bind(_ => Validate(policy, model, profiles, keepouts, markings, preparations, process, machine,
                parentRuns, sources, materialCertificate, egress, out FabricationInput input).Admitted(input));

}

// Run governance is DECLARED stage rows, never a literal fraction at a report site: the spine crosses exactly
// four measurable boundaries and each row states the fraction complete when that boundary is crossed, its own
// key serving as the abandonment witness so no second column restates it — the kernel `ArrangeStage` band a
// plane kernel reads through `ArrangementPolicy.Governed` is the same shape. A plane kernel publishing finer
// progress reports through the sink it is handed, so the spine never interpolates between its own rows and
// never publishes a fraction it did not measure.
[SmartEnum<string>]
internal sealed partial class RunStage {
    public static readonly RunStage Started    = new("started", done: 0.00);
    public static readonly RunStage Admitted   = new("admitted", done: 0.05);
    public static readonly RunStage Dispatched = new("dispatched", done: 0.90);
    public static readonly RunStage Sealed     = new("sealed", done: 1.00);

    public double Done { get; }
}

[ComplexValueObject]
public sealed partial class FabricationRuntime {
    public IClock Clock { get; }
    public CancellationToken Cancel { get; }
    public FabricationTap Telemetry { get; }

    // Rail, never a wrapper: `Drain`, `Replay`, `Detach`, and `Release` are members a composing app reaches
    // through this column, and a folder record forwarding `Fire` alone hides four of them behind a shell.
    public FabricationRail Hooks { get; }

    // Governance pairs a withdrawal with an observation: the token withdraws the run, the sink watches it. Progress
    // takes the carrier as `Option` for the same reason — no inert reporter exists to default onto, absence IS its
    // second state — and it takes the kernel's own `Option<IProgress<double>>` spelling so `ArrangementPolicy.Governed`
    // and every plane kernel below it seat this column with no adaptation and no sentinel sink.
    public Option<IProgress<double>> Progress { get; }

    // Memo stays app-neutral runtime capability, never process-global state: two runtimes composing the
    // library hold two caches, and a headless kernel run holds none with zero branching.
    public Option<HybridCache> Memo { get; }

    // Tap and rail both default to real values — `Silent` and a subscriber-free `Live` — so their parameters take
    // the nullable that collapses onto them, while the memo has no such value: absence IS its second state, so it
    // enters on the same carrier the property publishes and the nest arm at Nesting/nfp already spells. Minting the
    // default rail is a RAIL operation — `HookRail.Of` seats every point and rolls back a partial subscription —
    // so admission binds it rather than swallowing a composition refusal behind a `??`.
    public static Fin<FabricationRuntime> Admit(
        IClock clock,
        CancellationToken cancel,
        Op key,
        FabricationTap? telemetry = null,
        FabricationRail? hooks = null,
        Option<IProgress<double>> progress = default,
        Option<HybridCache> memo = default) =>
        Optional(hooks).Match(Some: Fin.Succ, None: () => FabricationHooks.Live(key))
            .Bind(rail => Validate(clock, cancel, telemetry ?? FabricationTap.Silent, rail, progress, memo,
                out FabricationRuntime runtime).Admitted(runtime));

    internal Unit Reached(RunStage stage) {
        Progress.Iter(sink => sink.Report(stage.Done));
        return unit;
    }
}
```

## [06]-[RUN_DISPATCH]

- Owner: `FabricationCanon` owns the ONE facade over `CanonicalWriter` every fabrication preimage composes — its framing family and its two closes alike; `QuantityArrow` owns the one dimension-text entry a plane outside `Process` reaches; `Fabrication` owns the run spine, the provenance fold, and the lineage projection.
- Law: `CanonicalWriter` is mutable-fluent — every primitive mutates the bound buffer and returns the SAME writer — so a call site chains or discards the return interchangeably and no fold copies a writer. `Discriminant` writes a generated owner's own key length-framed, so a preimage never carries a provider enum ordinal that a library reorder silently re-keys, and `Rows` writes the count before its rows so the layout stays self-delimiting.
- Law: the writer's constructor is PRIVATE and its two mints answer two different closes, so the facade owns both — `Keyed` opens `Retaining` and closes on the `Fin` rail, `Ordered` opens `Streaming` and closes on the digest. A lane spelling either mint itself carries the close's refusal as its own concern, and a lane spelling `new CanonicalWriter(...)` names no member at all.
- Entry: `QuantityArrow(axis, raised, locus).Admit(text)` routes to `ProcessPhysics.Admit`, the one textual boundary, and re-raises on the CALLER's plane — a `PhysicsQuantity.<axis>.Admit` at a consuming page is a second text boundary answering on a foreign plane and is the deleted form.
- Auto: provenance rails ITS OWN acyclicity before any traversal. A content-addressed key covers its own descendants, so a cycle in child-to-parent lineage is a FORGED key rather than a modelling mistake, and the gate answers `PolicyInadmissible` at the forgery rather than letting a sort throw.
- Receipt: the walk's outputs land as NAMED columns — `RunProvenance.Roots` and `RunProvenance.Generation` — and the graph container never leaves the fold.
- Boundary: only the nest arm is genuinely asynchronous, so `Sync` is the one lift the other nine arms take and no arm hand-spells a completed task. `Fired` dispatches through the generated total `Switch`, so a new result case cannot silently lose its hook point, and each fired result is folded rather than discarded.
- Law: `Raised` is the ONE refusal-only raise and carries four of the five points, proving the admitted fact equals the fired one — an egress-mint gate rewriting a content key forges an identity nothing produced, exactly as a lineage cycle does, and an observe or replay point holds no gate to move it. `Admission` alone takes the kernel's guarded arity, because rewriting the request is what an admission veto exists for; no site spells a point, since `At` is the fact's own column.
- Packages: `QuikGraph` (`BidirectionalGraph`, `SEdge`, `IsDirectedAcyclicGraph`, `Sinks`, `BreadthFirstSearchAlgorithm`, `VertexDistanceRecorderObserver`), `Rasm.Element` `CanonicalWriter`, `System.IO.Hashing` (`XxHash128` at the streaming close), LanguageExt.Core rails.

```csharp signature
// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
// The ONE extension family over the Element codec. Every fabrication preimage composes these and declares nothing
// of its own; a second Point/Option/Transform writer anywhere in the package is the deleted duplicate.
public static class FabricationCanon {
    public static CanonicalWriter Coords(this CanonicalWriter writer, Point3d point) =>
        writer.Double(point.X).Double(point.Y).Double(point.Z);

    public static CanonicalWriter Coords(this CanonicalWriter writer, (double X, double Y, double Z) point) =>
        writer.Double(point.X).Double(point.Y).Double(point.Z);

    public static CanonicalWriter Coords(this CanonicalWriter writer, Vector3d vector) =>
        writer.Double(vector.X).Double(vector.Y).Double(vector.Z);

    // A transform enters through its twelve affine reads, never a serialized basis quadruple: the reads are the
    // matrix, so a basis-point encoding that reconstructs them is a second convention over one fact.
    public static CanonicalWriter Basis(this CanonicalWriter writer, Transform transform) => writer
        .Double(transform.M00).Double(transform.M01).Double(transform.M02).Double(transform.M03)
        .Double(transform.M10).Double(transform.M11).Double(transform.M12).Double(transform.M13)
        .Double(transform.M20).Double(transform.M21).Double(transform.M22).Double(transform.M23);

    // Presence tag then payload: an absent column can never alias a written zero, matching the codec's own
    // `Optional` discipline for scalars over every carrier shape.
    public static CanonicalWriter Maybe<T>(
        this CanonicalWriter writer, Option<T> value, Func<CanonicalWriter, T, CanonicalWriter> write) =>
        value.Match(
            Some: row => write(writer.Bool(true), row),
            None: () => writer.Bool(false));

    public static CanonicalWriter Rows<T>(
        this CanonicalWriter writer, Seq<T> rows, Func<CanonicalWriter, T, CanonicalWriter> write) =>
        rows.Fold(writer.Ordinal(rows.Count), write);

    // The one discriminant framing: a generated owner's own key, length-framed. A provider enum ordinal in a
    // preimage forks every key the day the provider reorders its rows.
    public static CanonicalWriter Discriminant<TRow>(this CanonicalWriter writer, TRow row)
        where TRow : ISmartEnum<string>, IConvertible<string> => writer.String(row.ToValue());

    // --- [CLOSE]
    // The ONE keyed mint every fabrication artifact rides. `CanonicalWriter` publishes no public constructor —
    // `Retaining` holds a buffer and `Streaming` does not — and `ToBytes` is the RETAINING close alone, answering
    // `Fin` because a streaming writer has no preimage to hand back. A lane opening a writer by construction and
    // discarding that rail keyed its artifact off bytes it never held.
    // The bytes-retaining close: a SEALING consumer (an attested record, a signed passport) needs the preimage it
    // signs AND the key that addresses it from ONE close — two closes would let the signed bytes and the address
    // drift. `Keyed` is its key projection, so every keyed mint is a sealed mint that discarded the buffer.
    public static Fin<(ReadOnlyMemory<byte> Preimage, ContentKey Key)> Sealed(
        EgressKind kind, double grid, Func<CanonicalWriter, CanonicalWriter> frame, Op key) =>
        frame(CanonicalWriter.Retaining(grid))
            .ToBytes(key)
            .Map(preimage => (preimage, ContentKey.Of(kind, preimage.Span)));

    public static Fin<ContentKey> Keyed(
        EgressKind kind, double grid, Func<CanonicalWriter, CanonicalWriter> frame, Op key) =>
        Sealed(kind, grid, frame, key).Map(closed => closed.Key);

    // The order-only close: a frontier tie-break, a dominance probe, or any consumer needing a TOTAL ORDER over a
    // preimage rather than its bytes takes the writer's own digest and never materializes a buffer per probe.
    // NAMED LOSS: two preimages colliding at 128 bits tie instead of ordering — the same unforgeability the branch
    // already rests every `ContentKey` on. WITNESS: `ContentKey.Digest` is that width and nothing re-derives it.
    public static UInt128 Ordered(double grid, Func<CanonicalWriter, CanonicalWriter> frame) =>
        frame(CanonicalWriter.Streaming(grid, new XxHash128(seed: 0L))).Digest();

    // The quantum is the parameter and an admitting `Context` is one way to name it, so a lane already holding the
    // context reaches the same close without unpacking it and a lane holding only its own grid — an exact zero, a
    // gauge resolution, a chord error already projected to millimetres — hands that grid directly.
    public static Fin<ContentKey> Keyed(
        EgressKind kind, Context tolerance, Func<CanonicalWriter, CanonicalWriter> frame, Op key) =>
        Keyed(kind, tolerance.Absolute.Value, frame, key);

    public static UInt128 Ordered(Context tolerance, Func<CanonicalWriter, CanonicalWriter> frame) =>
        Ordered(tolerance.Absolute.Value, frame);

}

// The one dimension-text arrow every plane outside `Process` reaches. The axis names WHICH quantity parses and the
// plane names which fault its own refusal answers on, so a second `PhysicsQuantity.<axis>.Admit` entry at a caller
// — a text boundary raising a foreign plane's fault — is the deleted form.
public readonly record struct QuantityArrow(PhysicsQuantity Axis, FabConcern Raised, string Locus) {
    public Fin<double> Admit(string text) => ProcessPhysics
        .Admit(new PhysicsIngress.Quantity(Axis, text))
        .Bind(static admitted => admitted.Canonical);

    public Fin<Seq<double>> Admit(Seq<string> texts) => texts.Traverse(Admit).As();
}

public static class Fabrication {
    public static ValueTask<Fin<RunEvidence>> Run(FabricationInput input, FabricationRuntime runtime) =>
        (from _ in Ready(runtime, RunStage.Started)
         let key = Op.Of()
         let started = runtime.Clock.GetCurrentInstant()
         let asked = new FabricationHookFact.Admission(input)
         // Admission is the ONE transforming veto on the roster, so it takes the kernel's GUARDED arity: the
         // capsule hands the body its admitted fact and the rewritten request threads onward. Its sibling arm is
         // unreachable — `Seats` is 1:1 on `At`, so a fact seating at `Admission` IS an `Admission` case — and it
         // stays a named refusal rather than a cast, because a cast turns a future roster split into a crash.
         from admitted in runtime.Hooks.Fire(asked.At, asked, key, static fact => fact is FabricationHookFact.Admission settled
             ? Fin.Succ(settled.Input)
             : Fin.Fail<FabricationInput>(new KernelFault.InvalidValue("owner", "hook-admission:case")))
         from _dispatch in Ready(runtime, RunStage.Admitted)
         select (Input: admitted, Started: started)).Match(
            Succ: state => Dispatch(state.Input, runtime, state.Started),
            Fail: static error => ValueTask.FromResult(Fin.Fail<RunEvidence>(error)));

    public static Fin<RunLineage> Lineage(RunEvidence run) => Fin.Succ(new RunLineage(
        run.Policy,
        run.Process,
        run.Machine,
        run.ParentRuns,
        run.Sources,
        run.MaterialCertificate,
        run.Consumed,
        run.Produced,
        run.Provenance.Roots,
        run.Provenance.Generation));

    // The exact execution token is the cancellation authority; polling it lowers the kernel singleton directly.
    private static Fin<Unit> Ready(FabricationRuntime runtime, RunStage stage) => runtime.Cancel.IsCancellationRequested
        ? Fin.Fail<Unit>(Errors.Cancelled)
        : Fin.Succ(runtime.Reached(stage));

    private static async ValueTask<Fin<RunEvidence>> Dispatch(
        FabricationInput input,
        FabricationRuntime runtime,
        Instant started) {
        Op key = Op.Of();
        Fin<FabricationResult> dispatched = await input.Policy.Switch(
            state:      (Input: input, Runtime: runtime),
            hiddenLine: static (state, policy) => Sync(Hlr.Solve(
                policy,
                state.Input,
                static projection => new FabricationResult.HiddenLineResult(projection, projection.Sources))),
            cam:        static (state, policy) => Sync(Cam.Solve(policy, state.Input)),
            // Nest is the one genuinely asynchronous plane and its pair-memo leg is a landed abandonment producer,
            // so the runtime travels INTO it WHOLE: tap, memo, token, and settling clock are four columns of one
            // value, the token threading the memo lane so an in-flight cancel surfaces on the kernel cancellation
            // rail, and the clock stamping the settled receipt where it settles. Handing three columns instead left
            // the receipt unstamped and detected a withdrawal only once the search had already run to completion.
            nest:       static (state, policy) => Nest.Solve(policy, state.Input, state.Runtime),
            additive:   static (state, policy) => Sync(Slice.Solve(policy, state.Input)),
            // The verify plane fires its own settled-receipt fact, so it takes the run's tap exactly as the inspect
            // plane does; handing it none left the removal fact firing into `Silent` on the one path that carries a
            // live rail, and the instrument would have reported nothing for every run the spine dispatched.
            verify:     static (state, policy) => Sync(Removal.Verify(policy, state.Input, state.Runtime.Telemetry)),
            inspect:    static (state, policy) => Sync(Probe.Inspect(policy.Policy, state.Input, state.Runtime.Telemetry)),
            post:       static (state, policy) => Sync(Post.Lower(policy.Source, policy.Dialect, state.Input, policy.Policy)),
            document:   static (state, policy) => Sync(Traveler.Assemble(
                policy,
                state.Input,
                state.Runtime.Clock,
                static artifact => new FabricationResult.TravelerDocument(artifact))),
            derive:     static (state, policy) => Sync(Derivation.Plan(policy, state.Input, state.Runtime.Telemetry)),
            // The forming plane routes by SOURCE through the generated total dispatch, so a fourth modality is one
            // arm here and one row there rather than a branch ladder any of them can fall out of.
            form:       static (state, policy) => Sync(policy.Source.Switch(
                state: state,
                sheet: static (run, source) =>
                    from unfold in FlatPattern.Unfold(source.Policy, run.Input)
                    // The bend search is the sheet lane's long leg, so it takes the run's own tap and token: the
                    // engine census fires at its owner and a withdrawal lowers there rather than being detected
                    // only when the spine reads the token again on the far side of dispatch.
                    from bends in BendSequence.Plan(
                        unfold, source.Policy, source.Envelope, run.Runtime.Telemetry, run.Runtime.Cancel)
                    from result in FlatPattern.Formed(unfold, bends.Steps)
                    select result,
                // The tube lanes take the run's own clock because `Receipt<TEvidence>` stamps where it settles;
                // their runs arrive admitted on the case arm, which is why `TubeProgram.Apply` has a caller at all.
                tube: static (run, source) => TubeProgram
                    .Apply(new TubeOp.Form(source.Run, source.Kind, source.Envelope), run.Runtime.Clock)
                    .Map(static outcome => (FabricationResult)new FabricationResult.TubeFormed(outcome)),
                roll: static (run, source) => TubeProgram
                    .Apply(new TubeOp.Roll(source.Run, source.Envelope), run.Runtime.Clock)
                    .Map(static outcome => (FabricationResult)new FabricationResult.TubeFormed(outcome)))));
        // Plane kernels are the run's long leg, so the token is read again on THEIR far side: a withdrawal during
        // dispatch would otherwise seal evidence, mint egress keys, and fire the delivery hand-off for a run the
        // caller already abandoned. The same read publishes the dispatched fraction.
        return from result in dispatched
               from _reached in Ready(runtime, RunStage.Dispatched)
               let consumed = Consumed(input)
               from evidence in result.Evidence(input, consumed)
               from provenance in Provenance(evidence.Produced, consumed)
               let sealedEvidence = evidence with { Provenance = provenance }
               from _mint in sealedEvidence.Produced
                   .TraverseM(produced => Raised(runtime.Hooks, new FabricationHookFact.EgressMint(produced), key)).As().Map(static _ => unit)
               from _points in Fired(runtime.Hooks, result, key)
               from _handoff in Raised(runtime.Hooks, new FabricationHookFact.Delivery(sealedEvidence), key)
               let _fact = runtime.Telemetry.Fire(FabricationFact.Run.Of(sealedEvidence, runtime.Clock.GetCurrentInstant() - started))
               let _sealed = runtime.Reached(RunStage.Sealed)
               select sealedEvidence;
    }

    // Content-addressed lineage CANNOT cycle: a digest covering its own descendant is unforgeable, so a cycle here
    // names a forged key rather than a modelling error and rails before any traversal runs. Edges point child to
    // parent, so the ancestral frontier is the SINK set and generation depth is the child-side distance to it.
    private static Fin<RunProvenance> Provenance(Seq<ContentKey> produced, Seq<ContentKey> consumed) {
        BidirectionalGraph<ContentKey, SEdge<ContentKey>> lineage = new(allowParallelEdges: false);
        lineage.AddVertexRange(produced.Concat(consumed));
        lineage.AddEdgeRange(produced.Bind(child => consumed.Map(parent => new SEdge<ContentKey>(child, parent))));
        if (!lineage.IsDirectedAcyclicGraph())
            return Fin.Fail<RunProvenance>(new KernelFault.InvalidValue("owner", "lineage:forged-key"));

        // The observer's one-argument arity takes the edge weight alone and holds its own `Distances` dictionary; the
        // three-argument arity exists to supply a relaxer and a caller-owned map, neither of which a hop count
        // needs. A unit weight makes every distance the GENERATION depth in edges, keyed by vertex.
        BreadthFirstSearchAlgorithm<ContentKey, SEdge<ContentKey>> walk = new(lineage);
        VertexDistanceRecorderObserver<ContentKey, SEdge<ContentKey>> depths = new(static _ => 1.0);
        using (depths.Attach(walk)) {
            produced.Iter(walk.Compute);
        }
        return Fin.Succ(new RunProvenance(
            toSeq(lineage.Sinks()),
            toMap(toSeq(depths.Distances).Map(static row => (row.Key, (int)row.Value)))));
    }

    // The nine synchronous arms take ONE lift, so no arm hand-spells a completed task and the one genuinely
    // asynchronous plane stands out at the call site.
    private static ValueTask<Fin<FabricationResult>> Sync<T>(Fin<T> settled)
        where T : FabricationResult =>
        ValueTask.FromResult(settled.Map(static value => (FabricationResult)value));

    private static Seq<ContentKey> Consumed(FabricationInput input) =>
        input.Policy.Consumed + input.ParentRuns + input.Sources + input.MaterialCertificate.ToSeq();

    // Result-shaped hook projection through the GENERATED total switch: a new result case cannot silently lose its
    // point, and each fired result folds onto the rail rather than being discarded. Rail and key travel as ONE
    // state tuple, because the dispatch carries a single state slot and a captured key would strip `static`.
    private static Fin<Unit> Fired(FabricationRail rail, FabricationResult result, Op key) => result.Switch(
        state: (Rail: rail, Key: key),
        hiddenLineResult: static (_, _) => Fin.Succ(unit),
        motion: static (_, _) => Fin.Succ(unit),
        placement: static (_, _) => Fin.Succ(unit),
        additiveResult: static (_, _) => Fin.Succ(unit),
        verificationResult: static (spine, verification) =>
            Raised(spine.Rail, new FabricationHookFact.VerifyVerdict(verification), spine.Key),
        inspectionResult: static (_, _) => Fin.Succ(unit),
        postedProgram: static (_, _) => Fin.Succ(unit),
        travelerDocument: static (_, _) => Fin.Succ(unit),
        fabricationPlan: static (spine, plan) => plan.Steps
            .TraverseM(step => Raised(spine.Rail, new FabricationHookFact.StageAdvance(step), spine.Key)).As().Map(static _ => unit),
        formedResult: static (_, _) => Fin.Succ(unit),
        tubeFormed: static (_, _) => Fin.Succ(unit));

    // One refusal-only raise carries every point but `Admission`. Kernel `Fire` returns the ADMITTED fact, so
    // comparing it against the fired one CONSUMES the veto capability rather than discarding what a gate returned:
    // an egress-mint gate rewriting a content key forges an identity nothing produced — the forgery the provenance
    // walk already rails — while an observe or replay point holds no gate to move it, so the equality costs nothing
    // where nothing vetoes and is load-bearing where something does. Point spelling stays off the site: `At` is the
    // fact's own column, so a case and its seat can never be paired wrong here.
    private static Fin<Unit> Raised(FabricationRail rail, FabricationHookFact fact, Op key) =>
        rail.Fire(fact.At, fact, key).Bind(admitted => admitted == fact
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new KernelFault.InvalidValue("owner", $"hook-rewrite:{fact.At.Key}")));
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Fabrication orchestration fold
    accDescr: One generated run dispatch routes ten policy variants to their plane kernels, folds every result back through shared fabrication atoms, and projects content-keyed persistence facts.
    Atoms["atoms#GEOMETRY + MOTION + EQUIPMENT + PLAN Loop · Move · CutterForm · AdmittedComponent · PlannedStep · BendStep · ResidualStock · StockSnapshot · CapabilityVerdict"]
    Run["owner#RUN_DISPATCH 10-arm generated total Switch"]
    Family["family leaf axes ProcessKind · Machine · CutStrategy · PostDialect"]
    Run -->|HiddenLine| Hlr["Documentation/projection Hlr.Solve"]
    Run -->|Cam| Cam["Toolpath/motion Cam.Solve → conditioned Motion"]
    Run -->|Nest| Nest["Nesting/nfp Nest.Solve"]
    Run -->|Derive| Derivation["Process/derivation Derivation.Plan → FabricationPlan"]
    Run -->|"Additive · Verify · Inspect"| Planes["Slice.Solve · Removal.Verify · Probe.Inspect"]
    Run -->|"Post{PostSource, dialect} · Document{results, corpus} · Form{FormSource}"| Egress["Post.Lower · Traveler.Assemble · Unfold+Plan+Formed | TubeProgram.Apply"]
    Hlr --> Atoms
    Cam --> Atoms
    Nest --> Atoms
    Planes --> Atoms
    Egress --> Atoms
    Atoms --> Family
    Atoms -->|"ContentKey.Of → kernel ContentHash.Of"| Persist["Rasm.Persistence ArtifactKind enrollment rows"]
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
