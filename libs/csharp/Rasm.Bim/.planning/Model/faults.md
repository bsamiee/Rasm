# [BIM_FAULTS]

`BimFault` is the closed `[Union]` fault band (band 2600) every `Rasm.Bim` entrypoint returns on failure, arming `ModelRejected` (a rejected foreign payload, an IFC-legality violation, or a captured native exception), `UnmappedClass` (a closed-vocabulary miss), `DanglingReference` (an undeclared `GlobalId`/`NodeId`), `CodecReject` (an unavailable codec or degraded service), and `CapabilityMiss` (an in-process kernel the managed branch lacks). `GeometryFault` owns kernel geometry failures, `ElementFault` owns structural-graph failures, and `BimFault` owns BIM semantic-and-exchange failures; no band re-cases another.

Band 2600 is `Expected`-derived, so every case lifts BARE onto the `Fin<T>`/`Validation<Error,T>` rail with no `.ToError()` hop, and recovery reads `error.IsType<BimFault.DanglingReference>()` or `error.HasCode(2600)`, never a message substring. Per-case codes (`2601`–`2605`) with manual `Code`/`Message`/`ToError()` are the retired shape this owner closes: the typed case IS the `Error`.

Wire posture is HOST-LOCAL: `BimFault` rides the rail every Bim entrypoint returns and never sits between wire and rail. Accumulating surfaces — `IfcLegality.Validate` and the `Model/elements#IFC_CLASS` `IfcClass.AuditTarget` schema-retarget preflight — return `Validation<Error,Unit>`, folding independent `BimFault.ModelRejected` arms through `Error.Combine`; every other entrypoint rails `Fin<T>`. `ElementSet.Query` is total and carries no fault rail — only its `ValueMatch.Pattern.Of` admission and the graph-identity-gated `ElementSet.Combine` rail `ModelRejected`. `BimFault` dispatches through its generated total `Switch`, keys no `FrozenDictionary`, and a `[KeyMemberComparer]` on the fault is the deleted form. `Detail` is the ONE detail vocabulary beside the band: a row carries the leg that owns it and the arm it qualifies, so a raising site anywhere in the folder spells one row and never a literal.

## [01]-[INDEX]

- [02]-[FAULT_BAND]: `BimFault`, the `Expected`-derived `[Union]` band 2600 (`ModelRejected`/`UnmappedClass`/`DanglingReference`/`CodecReject`/`CapabilityMiss`) every Bim `Fin.Fail`/`Validation` failure lifts BARE, keyed by the kernel `Op` with a `Category` projection and a `Code => FaultBand.Bim` registry read.
- [03]-[DETAIL_ROSTER]: `Detail`, the `[SmartEnum<string>]` fault-detail roster beside the band — one row per diagnostic token, its `DetailLeg` owning column, the `BimFault` arm delegate it raises through, and the `Of`/`At` composers that append subjects after the stem.

## [02]-[FAULT_BAND]

- Owner: `BimFault` the closed `[Union]` fault band (band 2600) for BIM-and-exchange failures, `Expected`-derived (`IValidationError<BimFault>`) so the band IS the `Expected` `Code` — the `Code => FaultBand.Bim` read of the seam `Rasm.Element/Projection/fault#FAULT_BAND` `FaultBand` registry, the band shape the seam `ElementFault` realizes over the same registry — one `Op`-keyed case per failure carrying its `Detail`, a `Category` telemetry projection, and the LanguageExt `Error` lift the `Fin<T>`/`Validation<Error,T>` failure channel carries with no `.ToError()` hop.
- Cases: five arms partitioned by what failed — `ModelRejected` (a rejected foreign payload, an IFC-legality violation, a derived-view invariant failure, or a captured native exception) · `UnmappedClass` (a closed-vocabulary miss at any roster gate) · `DanglingReference` (an undeclared `GlobalId`/`NodeId` a fold resolves against) · `CodecReject` (an unavailable codec, an unresolved format, a catalogue-pending row, or a degraded external service) · `CapabilityMiss` (an in-process evaluation the managed branch owns no kernel for) (5). This band fixes the arms; `[03]-[DETAIL_ROSTER]` fixes the detail tokens and pairs each to its arm.
- Entry: `BimFault.ModelRejected(key, detail)` and its four siblings construct directly and lift BARE onto `Fin.Fail`, the implicit `Error → Fin`, and a `Validation`-accumulating `Fail<Error,Unit>` with no `.ToError()` — one idiom for the whole folder, so a `Project` ingress, an `Emit` egress, an exchange codec, and an `IfcLegality.Validate` accumulation compose on one rail without a second fault family. Composed kernel `Rasm` `GeometryFault` lowers through its OWN plain-union `.ToError()` member (deriving from no `Error`, so not `Expected`-derived), meeting a `BimFault` as two `Error` values on the single `Fin<ImportedGeometry>` rail.
- Auto: each Bim owner routes the most specific case — vocabulary and target-schema misses use `UnmappedClass`; absent graph identities use `DanglingReference`; malformed foreign payloads, IFC-legality failures, and derived-view invariant failures use `ModelRejected`; unavailable codecs and degraded external services use `CodecReject`; absent in-process kernels use `CapabilityMiss`. `SpatialStructure.Of` accumulates root, parent, rank, and connectivity failures as `ModelRejected`, `IfcLegality.Validate` accumulates independent semantic violations on the same `Validation<Error,Unit>` rail, and native exceptions enter only through `Try.lift(...).Run().MapFail(...)` at their owning boundaries.
- Receipt: `BimFault` is the typed fault evidence on the `Fin<T>`/`Validation<Error,T>` failure rail; no generic `IFault`/error-code abstraction, the cases stay typed per BIM concern, and a recovery reads `error.IsType<BimFault.DanglingReference>()` for the dangling arm, `error.HasCode(2600)` for band membership, or `error.Category` (`"Codec"`, `"Capability"`, …) for telemetry banding, never a message substring.
- Packages: `Rasm` (the kernel `Op` operation key + the `Expected` base each case derives through, and the composed `GeometryFault` band for the shared degenerate re-imported-tessellation failure), `Rasm.Element` (the seam `GraphDelta`/`ElementGraph` the projector rails carry + `IGraphConstraint` the `IfcLegality` accumulation implements + the `FaultBand` `[SmartEnum<int>]` band-allocation registry the `Code` override reads; the neighbor `ElementFault` band the seam owns), Thinktecture.Runtime.Extensions (`[Union]`/`IValidationError`), LanguageExt.Core (`Error`/`Fin`/`Validation`/`Try`).
- Growth: a new BIM-and-exchange failure routes onto one of the five existing arms — an IDS facet miss and a wire admission reject are `ModelRejected`, an unknown classification system or property template is `UnmappedClass`, a BCF viewpoint naming an absent element is `DanglingReference`, a bSDD service-unreachable degradation is `CodecReject`, a CRS the transform algebra cannot reconcile is `CapabilityMiss` — never a sixth arm per sub-domain, zero new band; a geometry or structural-graph failure routes its own neighbor band.
- Boundary: `BimFault` mints the closed BIM-and-exchange band and derives from `Expected`. Cases lift directly onto `Fin`/`Validation`/`Eff`; native exceptions enter through the owning `Try.lift(...).Run().MapFail(...)` boundary; structural graph faults remain the seam `ElementFault`, geometry faults remain the kernel `GeometryFault`, lifecycle abandonment of a long native lane remains the kernel `Fault.Cancelled` (the energy translate lane lowers it — never a sixth band-2600 arm), and IFC-semantic legality accumulates `BimFault.ModelRejected`.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using Rasm.Element.Projection;                       // the FaultBand band-allocation registry the Code override reads
using Thinktecture;
using Expected = Rasm.Domain.Expected;               // aliases the kernel base, dodging the LanguageExt.Common.Expected collision
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Model;

// --- [ERRORS] -----------------------------------------------------------------------------
// BIM-and-exchange fault band: Expected-derived so band 2600 IS the Expected Code and the typed case
// lifts bare onto Fin<T>/Validation<Error,T> with no .ToError() hop — Code reads the seam FaultBand registry
// row (disjointness type-enforced at the registry, a duplicate integer failing its generated key lookup at
// type initialization). No [GenerateUnionOps] (the kernel union-ops generator is strictly opt-in): the band wants
// no generated per-case SelfOp because every case carries an explicit kernel Op operation context;
// arm-specificity is error.IsType<Case>() (no Error.Is<E>()), telemetry error.Category.
// Construction is the nested-record ctor (new BimFault.Case(key, detail)) — [Union] generates the Switch/Map
// projection, not a per-case factory — and the Expected derivation makes the case an Error directly, so it lifts
// bare; IfcLegality.Validate accumulates ModelRejected over Validation via Error.Combine, which a .ToError() hop
// would erase. The kernel Rasm.Domain.Expected base ctor is PARAMETERLESS (Code a virtual Error member, Message
// abstract), so band 2600 IS the Code — the FaultBand.Bim row via the generated implicit SmartEnum-to-int
// conversion, one line, never a .Value spelling — and Message projects Detail.
[Union]
public abstract partial record BimFault : Expected, IValidationError<BimFault> {
    private BimFault(Op key, string detail) { Key = key; Detail = detail; }

    public Op Key { get; }
    public string Detail { get; }
    public override int Code => FaultBand.Bim;
    public override string Message => Detail;

    // IValidationError<BimFault>.Create — string-only admission the generated converter bridge calls on a
    // deserialization reject; routes the unspecific case under a boundary-admission Op so the bridged case still
    // carries an operation context (never a default Op) and a raw message never escapes the typed family.
    private static readonly Op Admission = Op.Of(name: nameof(Admission));
    public static BimFault Create(string message) => new ModelRejected(Admission, message);

    public sealed record ModelRejected(Op Key, string Detail)     : BimFault(Key, Detail) { public override string Category => "ModelRejected"; }
    public sealed record UnmappedClass(Op Key, string Detail)     : BimFault(Key, Detail) { public override string Category => "UnmappedClass"; }
    public sealed record DanglingReference(Op Key, string Detail) : BimFault(Key, Detail) { public override string Category => "Reference"; }
    public sealed record CodecReject(Op Key, string Detail)       : BimFault(Key, Detail) { public override string Category => "Codec"; }
    public sealed record CapabilityMiss(Op Key, string Detail)    : BimFault(Key, Detail) { public override string Category => "Capability"; }
}
```

## [03]-[DETAIL_ROSTER]

- Owner: `Detail` the `[SmartEnum<string>]` fault-detail vocabulary for the whole folder — one row per diagnostic token an operator greps and a consumer routes on, carrying the `DetailLeg` that owns it and the `BimFault` arm it raises through; `DetailLeg` the owning-leg column; `Detail.Of` the subject composer and `Detail.At` the one raising member every fence spells.
- Law: a detail is a ROW, never a literal and never a per-page const class — the retired `ExchangeDetail`/`EnergyDetail` pair split one grep contract across two owners while the interchange codecs, the wire, the events, the tessellation bridge, the reconstruction front, and the energy legs all raise into it. Subjects append AFTER the stem, so every row is a fixed searchable prefix: the slot- and codec-parameterized families that infixed their variable (`event-<slot>-malformed`, `<codec>-decode`) become `event-slot-malformed:<slot>` and `codec-decode:<codec>:<message>`, one row each.
- Entry: `Detail.<Row>.At(key, subjects…)` mints the typed `BimFault` whole — the row's own arm delegate closes the arm choice, so a site cannot pair a codec token with a `CapabilityMiss` or an unmapped-class token with a `CodecReject`. `Detail.Of(row, subjects…)` yields the composed string alone for a site already holding its arm.
- Growth: a new diagnostic is one row carrying its leg and its arm; a new leg is one `DetailLeg` value. A row minted for a token no fence raises is the deleted phantom, and a second detail owner beside this roster re-forks the contract this cluster exists to hold.
- Boundary: `Detail` fixes the TOKEN space; `BimFault` fixes the ARM space, and the arm column is the join. Degrade evidence is NOT a detail — a warning-counted drop rides the owning receipt's own typed reason row (`Energy/exchange#ENERGY_EXCHANGE` `EnergyReason`, `Exchange/import#IMPORT_RAIL` `DecodeDegrade`), never this roster, because a degrade never reaches a fault rail.

```csharp signature
// Shares the [02] RUNTIME_PRELUDE (one compilation unit per page).

// --- [TYPES] ------------------------------------------------------------------------------
// The leg that OWNS a row. Grouping rides this column rather than a per-leg class, so the roster
// stays one grep contract while a reader still reaches the fence that raises a token.
public enum DetailLeg : byte {
    Format = 0, Import = 1, Export = 2, Wire = 3, Events = 4,
    Tessellation = 5, Reconstruct = 6, Energy = 7,
}

// --- [ERRORS] -----------------------------------------------------------------------------
// Detail closes the folder's fault-detail vocabulary. Each row is a STEM: At appends its subjects
// after the stem separated by ':', so every token is a fixed prefix an operator greps and a
// consumer routes on, and the slot- and codec-parameterized families that used to infix their
// variable carry it as a subject instead. The arm column is a static lambda per row — closure-free,
// and the whole reason a token cannot drift onto the wrong BimFault case.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Detail {
    // --- [FORMAT_LEG]
    public static readonly Detail InterchangeFormatMiss = new("interchange-format-miss", DetailLeg.Format, Codec);
    public static readonly Detail DirectionUnsupported = new("direction-unsupported", DetailLeg.Format, Codec);

    // --- [IMPORT_LEG]
    public static readonly Detail ImportCataloguePending = new("import-catalogue-pending", DetailLeg.Import, Codec);
    public static readonly Detail ImportGeospatialRoute = new("import-geospatial-route", DetailLeg.Import, Codec);
    public static readonly Detail ImportIfcRoute = new("import-ifc-route", DetailLeg.Import, Codec);
    public static readonly Detail ImportNeedsCompanion = new("import-needs-companion", DetailLeg.Import, Capability);
    public static readonly Detail ImportPointCloudRoute = new("import-point-cloud-route", DetailLeg.Import, Codec);
    public static readonly Detail ImportStepRoute = new("import-step-route", DetailLeg.Import, Codec);
    public static readonly Detail IfcCodecMiss = new("ifc-codec-miss", DetailLeg.Import, Codec);
    public static readonly Detail StepCodecMiss = new("step-codec-miss", DetailLeg.Import, Codec);
    public static readonly Detail MeshTextUnsupported = new("mesh-text-unsupported", DetailLeg.Import, Codec);
    public static readonly Detail SpeckleNoDisplay = new("speckle-no-display", DetailLeg.Import, Rejected);
    public static readonly Detail TypeCandidateIdentityMissing = new("type-candidate-identity-missing", DetailLeg.Import, Rejected);
    public static readonly Detail UsdScopePath = new("usd-scope-path", DetailLeg.Import, Rejected);

    // --- [EXPORT_LEG]
    public static readonly Detail BimExport = new("bim-export", DetailLeg.Export, Codec);
    public static readonly Detail CobieEmit = new("cobie-emit", DetailLeg.Export, Codec);
    public static readonly Detail CobieExportGraphRoute = new("cobie-export-graph-route", DetailLeg.Export, Codec);
    public static readonly Detail ElementSceneEmpty = new("element-scene-empty", DetailLeg.Export, Codec);
    public static readonly Detail ElementSceneMeshMiss = new("element-scene-mesh-miss", DetailLeg.Export, Dangling);
    public static readonly Detail ExportCataloguePending = new("export-catalogue-pending", DetailLeg.Export, Codec);
    public static readonly Detail ExportNeedsHost = new("export-needs-host", DetailLeg.Export, Capability);
    public static readonly Detail GeoExportRoute = new("geo-export-route", DetailLeg.Export, Codec);
    public static readonly Detail GltfExport = new("gltf-export", DetailLeg.Export, Codec);
    public static readonly Detail IfcExportCodecMiss = new("ifc-export-codec-miss", DetailLeg.Export, Codec);
    public static readonly Detail IfcExportRoute = new("ifc-export-route", DetailLeg.Export, Codec);
    public static readonly Detail KhrEncoderUnrouted = new("khr-encoder-unrouted", DetailLeg.Export, Codec);
    public static readonly Detail LodDecimate = new("lod-decimate", DetailLeg.Export, Rejected);
    public static readonly Detail MeshletBuild = new("meshlet-build", DetailLeg.Export, Rejected);
    public static readonly Detail SceneAuthor = new("scene-author", DetailLeg.Export, Codec);
    public static readonly Detail SceneExport = new("scene-export", DetailLeg.Export, Codec);
    public static readonly Detail ScheduleAnimation = new("schedule-animation", DetailLeg.Export, Rejected);
    public static readonly Detail SubtreeAuthor = new("subtree-author", DetailLeg.Export, Rejected);
    public static readonly Detail SubtreeAuthorMany = new("subtree-author-many", DetailLeg.Export, Rejected);
    public static readonly Detail SubtreeAvailabilityMismatch = new("subtree-availability-mismatch", DetailLeg.Export, Rejected);
    public static readonly Detail SubtreeReread = new("subtree-reread", DetailLeg.Export, Rejected);
    public static readonly Detail TileMetadata = new("tile-metadata", DetailLeg.Export, Rejected);
    public static readonly Detail UsdExport = new("usd-export", DetailLeg.Export, Codec);

    // --- [WIRE_LEG]
    public static readonly Detail WireDecode = new("wire-decode", DetailLeg.Wire, Rejected);
    public static readonly Detail WireEncode = new("wire-encode", DetailLeg.Wire, Rejected);
    public static readonly Detail WireNoMutual = new("wire-no-mutual", DetailLeg.Wire, Codec);

    // --- [EVENTS_LEG]
    // The four slot-parameterized rows carry their wire-slot name as a SUBJECT: the retired
    // `event-<slot>-malformed` grammar infixed it, so the family had no prefix a row could own.
    public static readonly Detail EventArtifactKeyMalformed = new("event-artifact-key-malformed", DetailLeg.Events, Codec);
    public static readonly Detail EventBodyMiss = new("event-body-miss", DetailLeg.Events, Codec);
    public static readonly Detail EventEnvelopeMalformed = new("event-envelope-malformed", DetailLeg.Events, Codec);
    public static readonly Detail EventKeyMalformed = new("event-key-malformed", DetailLeg.Events, Codec);
    public static readonly Detail EventMutationMiss = new("event-mutation-miss", DetailLeg.Events, Codec);
    public static readonly Detail EventPayloadDecode = new("event-payload-decode", DetailLeg.Events, Codec);
    public static readonly Detail EventSetMalformed = new("event-set-malformed", DetailLeg.Events, Codec);
    public static readonly Detail EventSlotMalformed = new("event-slot-malformed", DetailLeg.Events, Codec);
    public static readonly Detail EventSlotNegative = new("event-slot-negative", DetailLeg.Events, Codec);
    public static readonly Detail EventSubjectMismatch = new("event-subject-mismatch", DetailLeg.Events, Codec);
    public static readonly Detail EventTypeMiss = new("event-type-miss", DetailLeg.Events, Codec);

    // --- [TESSELLATION_LEG]
    public static readonly Detail CompanionUnreachable = new("companion-unreachable", DetailLeg.Tessellation, Capability);
    public static readonly Detail GlbStoreReject = new("glb-store-reject", DetailLeg.Tessellation, Codec);
    public static readonly Detail GlbStoreUnreachable = new("glb-store-unreachable", DetailLeg.Tessellation, Codec);
    public static readonly Detail IfcTessellation = new("ifc-tessellation", DetailLeg.Tessellation, Rejected);
    public static readonly Detail TessellationDegenerate = new("tessellation-degenerate", DetailLeg.Tessellation, Rejected);
    public static readonly Detail TessellationNotRequired = new("tessellation-not-required", DetailLeg.Tessellation, Capability);

    // --- [RECONSTRUCT_LEG]
    // CodecDecode carries the LasCompression row key as its first subject, so the two engine legs
    // share ONE row and neither spells a runtime-built prefix.
    public static readonly Detail CloudDecimate = new("cloud-decimate", DetailLeg.Reconstruct, Codec);
    public static readonly Detail CloudExtent = new("cloud-extent", DetailLeg.Reconstruct, Capability);
    public static readonly Detail CodecDecode = new("codec-decode", DetailLeg.Reconstruct, Codec);
    public static readonly Detail ReconBelowBand = new("recon-below-band", DetailLeg.Reconstruct, Unmapped);
    public static readonly Detail ReconShapeMiss = new("recon-shape-miss", DetailLeg.Reconstruct, Unmapped);
    public static readonly Detail ReconUnregistered = new("recon-unregistered", DetailLeg.Reconstruct, Capability);

    // --- [ENERGY_LEG]
    public static readonly Detail EnergyClassMiss = new("energy-class-miss", DetailLeg.Energy, Unmapped);
    public static readonly Detail EnergyConstructionAbsent = new("energy-construction-absent", DetailLeg.Energy, Dangling);
    public static readonly Detail EnergyDecode = new("energy-decode", DetailLeg.Energy, Rejected);
    public static readonly Detail EnergyEgressPending = new("energy-graph-egress-pending", DetailLeg.Energy, Capability);
    public static readonly Detail EnergyEnvelopeEmpty = new("energy-envelope-empty", DetailLeg.Energy, Rejected);
    public static readonly Detail EnergyFaceMiss = new("energy-face-miss", DetailLeg.Energy, Unmapped);
    public static readonly Detail EnergyFormMiss = new("energy-form-miss", DetailLeg.Energy, Codec);
    public static readonly Detail EnergyLowerEmpty = new("energy-lower-empty", DetailLeg.Energy, Capability);
    public static readonly Detail EnergyLowerUnsupported = new("energy-lower-unsupported", DetailLeg.Energy, Codec);
    public static readonly Detail EnergyResultDuplicate = new("energy-result-duplicate", DetailLeg.Energy, Rejected);
    public static readonly Detail EnergyResultTargetMiss = new("energy-result-target-miss", DetailLeg.Energy, Dangling);
    public static readonly Detail EnergyTranslate = new("energy-translate", DetailLeg.Energy, Rejected);
    public static readonly Detail EnergyTranslateMiss = new("energy-translate-miss", DetailLeg.Energy, Codec);

    // Arm constructors named ONCE so a row spells one token, and the five names read as the band's
    // own vocabulary rather than five repeated `new BimFault.Case` expressions down the roster.
    static readonly Func<Op, string, BimFault> Rejected = static (key, detail) => new BimFault.ModelRejected(key, detail);
    static readonly Func<Op, string, BimFault> Unmapped = static (key, detail) => new BimFault.UnmappedClass(key, detail);
    static readonly Func<Op, string, BimFault> Dangling = static (key, detail) => new BimFault.DanglingReference(key, detail);
    static readonly Func<Op, string, BimFault> Codec = static (key, detail) => new BimFault.CodecReject(key, detail);
    static readonly Func<Op, string, BimFault> Capability = static (key, detail) => new BimFault.CapabilityMiss(key, detail);

    public DetailLeg Leg { get; }

    // The arm this row raises through. A row and its arm travel together, so a site can neither
    // pair a codec token with CapabilityMiss nor re-decide the arm at the raise.
    Func<Op, string, BimFault> Arm { get; }

    private Detail(string key, DetailLeg leg, Func<Op, string, BimFault> arm) : this(key) => (Leg, Arm) = (leg, arm);

    // At is the ONE raising member: the row's stem, its subjects, and its arm in one expression, so
    // no fence spells `new BimFault.<Case>(key, $"…")` and no interpolation re-forks a token.
    public BimFault At(Op key, params ReadOnlySpan<string> subjects) => Arm(key, Of(this, subjects));

    // Of composes the operator-facing string: the stem, then each subject after a ':' separator.
    // An empty subject span yields the bare stem, which is exactly what a row carrying no variable
    // wants — so no row needs a second no-subject spelling.
    public static string Of(Detail row, params ReadOnlySpan<string> subjects) =>
        subjects.IsEmpty ? row.Key : string.Concat(row.Key, ":", string.Join(':', subjects));
}
```

## [04]-[RESEARCH]

(none)
