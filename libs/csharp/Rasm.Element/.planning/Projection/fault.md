# [ELEMENT_FAULTS]

`ElementFault` is the seam's one closed `[Union]` fault band (band 2500) over the structural-graph, value-admission, projection, and content-verification failures every `Rasm.Element` entrypoint produces. Six cases share one `(Op Key, string Detail)` base and derive `Expected` (`IValidationError<ElementFault>`), so band 2500 IS the `Expected` `Code`, a typed case lifts BARE onto the `Fin<T>`/`Validation<Error,T>` rail through the LanguageExt implicit `Error` conversion with no `.ToError()` hop, and each case projects a `Category` for telemetry banding.

`FaultBand` hosts the band-allocation registry HERE because `Rasm.Element` is the lowest stratum every rebinding peer shares: one `[SmartEnum<int>]` row per federation band names its declaring `Owner` and its `Mirror` allocation-versus-reservation discriminant, a duplicate integer fails the generated key lookup at type initialization, and a cross-folder `Fin<T>` reads its origin from the band code.

Wire posture holds HOST-NEUTRAL: `ElementFault` rides the `Fin<T>` rail every seam entrypoint returns and never sits between wire and rail, a total surface mints no case at all, the seam's own `Switch` enforces STRUCTURAL edge legality alone while IFC-semantic legality rides the consumer's `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint.Validate` [M3], and the band keys no `FrozenDictionary` — dispatch is the generated total `Switch` and a `[KeyMemberComparer]` on the fault is the deleted form.

## [01]-[INDEX]

- [02]-[FAULT_BAND]: `FaultBand` band-allocation registry with its `Owner` and `Mirror` columns, and the `Expected`-derived `ElementFault` band-2500 union every seam failure lifts bare.

## [02]-[FAULT_BAND]

- Owner: `FaultBand` the `[SmartEnum<int>]` band-allocation registry (one row per federation band, the `Owner` string column naming the declaring surface for telemetry docs, the `Mirror` bool separating an allocated row a peer's `Code` rebinds to from the kernel's pinned reservation — hosted on this page because the seam is the lowest stratum every rebinding peer shares) and `ElementFault` the closed `[Union]` fault band (band 2500) for the structural-graph, value-admission, projection, and content-verification failures, `Expected`-derived (`IValidationError<ElementFault>`) so band 2500 IS the `Expected` `Code` (`Code => FaultBand.Element` reading the registry row). Six cases share one base carrying the kernel `Op Key` and the `Detail` string, so `Message => Detail` is one line and the band projects `Category` per case, the typed case lifting bare onto the `Fin<T>`/`Validation<Error,T>` failure channel.
- Cases: `NodeAbsent` (an edge endpoint, a `Bake` root, or a replayed-`GraphDelta` reference naming an undeclared `NodeId`) · `RelationshipInvalid` (an edge whose endpoint node-kinds violate the structural edge law over the `Relations/relation#EDGE_ALGEBRA` `Endpoints`, or a cyclic `Compose` ancestry) · `DeltaConflict` (a put-existing / drop-absent / duplicate-link conflicting with the working-graph state) · `ValueRejected` (the band-wide value-admission rail every typed-value smart-constructor across the seam value vocabulary fails onto — `Properties/quantity#MEASURE_VALUE` `MeasureValue.Of`/`OfCount` on a non-finite magnitude, unresolvable unit, or SI-unavailable quantity and `Sum` on a cross-quantity type mismatch, `Properties/property#PROPERTY_VALUE` `PropertyValue.Of` on a malformed composite value, `Composition/acoustic#ACOUSTIC_FOLDS` `Acoustic.Of` on a band-arity mismatch / out-of-unit absorption / non-finite sound-reduction and `RatingContour.Fit` on a short or non-finite contour window, `Composition/material#MATERIAL_COMPOSITION` `OfLayerSet`/`OfConstituentSet` on an empty set / non-positive thickness / out-of-unit or unnormalized fraction, `Composition/material#MATERIAL_PROPERTY` `OfMechanical`/`OfThermal`/`OfEnvironmental`/`OfCost` on an out-of-range datum and the discipline vocabulary `Parse` factories (`FireRating`/`Currency`/`MeasurementBasis`/`ImpactCategory`) on an unknown token, `Geospatial/reference#GEO_REFERENCE` `GeoReference.Admit` on a present-but-unresolvable CRS name, `Geospatial/coverage#COVERAGE_NODE` `CoverageGrid.Of` on a degenerate grid / empty band set / duplicate band index and `RasterSampleType.Parse` on an unknown pixel type, and `Relations/relation#EDGE_ALGEBRA` `CardinalPoint.Of` on an out-of-grid profile reference) · `ProjectionFailed` (an `IElementProjection.Project`/`Assemble` delta the structural law rejects, or a captured projector exception lowered with its raw foreign message — a hook-tap capture parks as `IsolatedFault` on the observe rail's evidence cell, never a case here) · `AddressUnstable` (a content-verification mismatch — a persisted node whose recomputed content id no longer equals its stored `NodeId`, the H7 tamper/corruption gate a `Rasm.Persistence` rehydrate and the cross-runtime parity corpus run) (6); the IFC-semantic legality (containment-relating-must-be-spatial, `Void` element→opening, `Type`-may-not-aggregate-`Occurrence`) routes the consumer's `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint.Validate` → `BimFault.ModelRejected` over `Validation<Error,Unit>` [M3], never re-cased here, and the total MINT path (`ContentAddress.Of`/`OfGraph`/`ToCanonicalBytes`) mints no fault — `AddressUnstable` rides the `Verify` re-hash dual alone.
- Entry: the per-case static factories are the fault constructors the `Fin<T>`/`Validation<Error,T>` rail carries — `ElementFault.NodeAbsent(key, detail)` and the five siblings return the `Expected`-derived base over the nested sealed `…Case` record (the production `UiFault.InvalidInput`/`MutationRejected` pattern — the union carries no `[GenerateUnionOps]` because every case already carries an explicit `Op Key` and the kernel union-ops generator is strictly opt-in, while the generated `Switch` survives, so a case constructs through `ElementFault.Case(key, detail)`, never `new …Case(…)` at a call site and never a runtime string code), each an `Error` whose 2500 band IS the `Code`, so `Fin.Fail<ElementGraph>(ElementFault.DeltaConflict(key, detail))` and the implicit `Error → Fin` lift both carry the typed case directly (the `Expected` derivation makes `.ToError()` unnecessary — the case IS the `Error`), and a `Validation`-accumulating gate passes the bare case into `Fail<Error,Unit>(ElementFault.RelationshipInvalid(key, detail))`; one construction idiom serves the whole seam, and a model route, a value admission, and a projection compose on one `Fin<T>` rail without a second fault family.
- Auto: `FaultBand.Allocations` and `Reservations` derive the operational registry partitions from the `Mirror` policy column, so telemetry and collision checks consume the same generated row family instead of re-filtering owners or literals; each seam owner routes the most specific case under an angle-bracketed `<kind:value>` discriminant `Detail` (the exact literal owned by the producing page, never restated here) — `Bake` routes `NodeAbsent` on an absent root and `RelationshipInvalid` on a cyclic `Compose` ancestry, `ElementGraph.Apply` routes `NodeAbsent` on a replayed delta whose added edge names an absent endpoint; `WorkingGraph.Apply` routes `NodeAbsent` on a `Link` to an absent endpoint, `RelationshipInvalid` when the `LegalLink`/`LegalAssign` structural law rejects the endpoint kinds, and `DeltaConflict` on a put-existing / drop-absent / duplicate-link state conflict; the typed-value smart-constructors across the seam value vocabulary (the Cases roster) all route `ValueRejected` on their admission miss; `Assemble` lowers a captured projector exception into `ProjectionFailed(key, error.Message)` through the `Try.lift(() => projector.Project(ctx)).Run().MapFail(...)` boundary funnel (preserving the RAW foreign message — the kernel `Op.Catch` re-wrap into `Fault.InvalidResult` boilerplate that erases the typed arm is NOT the seam idiom) and routes `ProjectionFailed` when a folded `GraphDelta` fails the structural law; `ElementHookRail.Of` routes `ValueRejected` when the kernel capsule refuses a gate on an observe-only point, lowering that capsule verdict onto the band under the mint's own `Op` so a composition failure reads its origin from band 2500 like every other entrypoint, while the same `Projection/observe` composition parks a captured subscriber failure as a point-attributed `IsolatedFault` row in its `TapFaults` evidence cell — the kernel capsule's shield, the emitter's rail untouched; `ContentAddress.Verify` routes `AddressUnstable` when a node's re-derived id no longer equals its stored `NodeId` — a non-rooted node's re-projected content id or a TYPE `Object`'s re-minted `NodeId.RootedType` (over its `Representations`-excluded `ToTypeSeedBytes` seed) drifting, an OCCURRENCE `Object`'s random Guid-v7 verifying vacuously, the snapshot sweep accumulating every drift over `Validation<Error,Unit>`.
- Receipt: `ElementFault` is the typed fault evidence on the `Fin<T>`/`Validation<Error,T>` failure rail; no generic `IFault`/error-code abstraction, the cases stay typed per seam concern, and a recovery reads `error.IsType<ElementFault.ValueRejectedCase>()` for the arm, `error.HasCode(2500)` for band membership, or `error.Category` (`"Value"`, `"Projection"`, `"Address"`, …) through the kernel `FaultExtensions` extension property for telemetry banding, never a message substring.
- Packages: `Rasm` (the kernel `Op` operation key + the `Domain.Expected` base each case derives through + the `FaultExtensions.Category` projection), Thinktecture.Runtime.Extensions (`[Union]`/`IValidationError<T>` + the `[SmartEnum<int>]` registry with the generated key lookup and implicit key conversion), LanguageExt.Core (`Error`/`Fin`/`Validation`/`Try`).
- Growth: a new structural-graph or value-admission failure routes onto one of the six existing arms — a new edge legality is `RelationshipInvalid`, a new typed-value admission is `ValueRejected`, a new graph-state conflict is `DeltaConflict`, a content-verification mismatch is `AddressUnstable` — never a new arm; a projector's foreign rejection routes `ProjectionFailed` and an IFC-semantic legality rejection routes the consumer's `IGraphConstraint` `Validation<Error,Unit>`; a new sub-domain (`assessment`, `coverage`, `reference`) routes its rejection onto an existing arm — a missing assessment input is `NodeAbsent`, an out-of-range coverage band or an unresolvable CRS is `ValueRejected`; a new FEDERATION band is ONE `FaultBand` row, its owning package consuming the row in its own `Code` override; zero new `ElementFault` arm, zero new `Category` arm without a case.
- Boundary: `ElementFault` mints ONLY the six seam cases and is `Expected`-derived — a parallel band that re-cases a value-admission miss per sub-domain, a seventh arm, or a re-cased kernel `GeometryFault`/`Rasm.Bim` `BimFault` case is the deleted form; the typed case lifts BARE onto the rail (`Fin.Fail<T>(ElementFault.X(key, detail))` / `Fail<Error,Unit>(ElementFault.RelationshipInvalid(key, detail))`) and a `.ToError()` hop OR a hand-built `Error.New(2500, message)` bypassing the typed factory is the named defect; the six homogeneous `(Op Key, string Detail)` cases share one base so `Message => Detail` is ONE line — a central `Message => Switch(…)` over six identical `c.Detail` arms is the repeated-arm collapse defect, the central form belonging to the production `UiFault` band whose heterogeneous cases genuinely require it; exception-style control flow in domain logic is the named defect (EXPRESSION_SPINE) — a foreign `Exception` (a GeometryGym parse fault, a VividOrange miss) enters ONLY through the `Try.lift(...).Run().MapFail(...)` boundary funnel and never crosses a seam signature; the IFC-schema legality rejection is the consumer's `IGraphConstraint` concern, never an `ElementFault` arm, because the band carries no IFC vocabulary; and the content-address MINT path is total — routing a non-finite value or guarding the mint through `AddressUnstable` is the deleted form, the arm existing for the `Verify` mismatch alone.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using Thinktecture;
using Expected = Rasm.Domain.Expected;   // the kernel Expected (parameterless ctor + virtual Category), NOT LanguageExt.Common.Expected
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

namespace Rasm.Element.Projection;

// --- [TABLES] -----------------------------------------------------------------------------
// Rasm.Element hosts the band-allocation registry because it is the LOWEST SHARED STRATUM every rebinding peer
// already depends on (Materials/Bim/Fabrication reference the seam; their `Code` overrides consume their row
// through the generated implicit SmartEnum-to-int conversion), matching the registry form AppHost, Persistence,
// and AppUi mirror at their own strata. A new band is ONE row; a duplicate integer fails the generated key lookup
// at type initialization, so disjointness is type-enforced, never prose. Mirror discriminates ALLOCATION from
// RESERVATION: an allocated row (Mirror: false) carries the integer its named owner rebinds `Code` to, while
// kernel 2400 is the ONE pinned MIRROR (Mirror: true) — sitting BELOW this stratum, the kernel keeps its literal
// and this row only reserves the integer against every other claimant, so the seam allocates nothing above an
// owner it cannot see. Owner names the declaring surface for telemetry docs.
[SmartEnum<int>]
public sealed partial class FaultBand {
    public static readonly FaultBand Component   = new(2300, owner: "Rasm.Materials/Component", mirror: false);
    public static readonly FaultBand Generation  = new(2350, owner: "Rasm.Generation", mirror: false);
    public static readonly FaultBand Geometry    = new(2400, owner: "Rasm", mirror: true);
    public static readonly FaultBand Material    = new(2450, owner: "Rasm.Materials/Appearance", mirror: false);
    public static readonly FaultBand Projection  = new(2470, owner: "Rasm.Materials/Projection", mirror: false);
    public static readonly FaultBand Element     = new(2500, owner: "Rasm.Element", mirror: false);
    public static readonly FaultBand Bim         = new(2600, owner: "Rasm.Bim", mirror: false);
    public static readonly FaultBand Fabrication = new(2700, owner: "Rasm.Fabrication", mirror: false);
    public string Owner { get; }
    public bool Mirror { get; }
    public static Seq<FaultBand> Allocations => toSeq(Items).Filter(static band => !band.Mirror);
    public static Seq<FaultBand> Reservations => toSeq(Items).Filter(static band => band.Mirror);
}

// --- [ERRORS] -----------------------------------------------------------------------------
// Expected-derived so band 2500 IS the Code (the FaultBand.Element row via the generated implicit SmartEnum-to-int
// conversion) and the typed case lifts bare onto Fin<T>/Validation<Error,T> — no .ToError() hop, which erases the
// ManyErrors IsType/HasCode/Filter recursion an IGraphConstraint accumulation relies on. Kernel Expected declares a
// PARAMETERLESS ctor (base(detail, 2500, None) targets the OTHER LanguageExt.Common.Expected — the named defect).
// Homogeneous cases: the base carries Key/Detail, Message => Detail once; per-case Category feeds
// FaultExtensions extension property error.Category. No [GenerateUnionOps] — the kernel union-ops generator is strictly opt-in, and
// every case already carries an explicit Op Key, wanting no generated SelfOp; the generated Switch survives.
// Arm probe: error.IsType<ElementFault.XCase>() — there is no Error.Is<E>(); Error.Is takes an Error argument.
[Union]
public abstract partial record ElementFault : Expected, IValidationError<ElementFault> {
    private ElementFault(Op key, string detail) { Key = key; Detail = detail; }

    public Op Key { get; }
    public string Detail { get; }
    public override int Code => FaultBand.Element;
    public override string Message => Detail;

    public sealed record NodeAbsentCase(Op Key, string Detail)          : ElementFault(Key, Detail) { public override string Category => "NodeAbsent"; }
    public sealed record RelationshipInvalidCase(Op Key, string Detail) : ElementFault(Key, Detail) { public override string Category => "Relationship"; }
    public sealed record DeltaConflictCase(Op Key, string Detail)       : ElementFault(Key, Detail) { public override string Category => "Delta"; }
    public sealed record ValueRejectedCase(Op Key, string Detail)       : ElementFault(Key, Detail) { public override string Category => "Value"; }
    public sealed record ProjectionFailedCase(Op Key, string Detail)    : ElementFault(Key, Detail) { public override string Category => "Projection"; }
    // ContentAddress.Verify re-hash dual (H7) raises this alone; the mint path stays total.
    public sealed record AddressUnstableCase(Op Key, string Detail)     : ElementFault(Key, Detail) { public override string Category => "Address"; }

    // Per-case static factories are the ONE construction idiom the whole seam calls (ElementFault.ValueRejected(key,
    // detail)), returning the Expected-derived base so the case lifts bare onto Fin<T>/Validation<Error,T>.
    public static ElementFault NodeAbsent(Op key, string detail)          => new NodeAbsentCase(key, detail);
    public static ElementFault RelationshipInvalid(Op key, string detail) => new RelationshipInvalidCase(key, detail);
    public static ElementFault DeltaConflict(Op key, string detail)       => new DeltaConflictCase(key, detail);
    public static ElementFault ValueRejected(Op key, string detail)       => new ValueRejectedCase(key, detail);
    public static ElementFault ProjectionFailed(Op key, string detail)    => new ProjectionFailedCase(key, detail);
    public static ElementFault AddressUnstable(Op key, string detail)     => new AddressUnstableCase(key, detail);

    // IValidationError<ElementFault>.Create — the string-only admission the generated converter bridge calls on a
    // deserialization reject; routes the unspecific case under a boundary-admission Op (never a default Op) so a raw
    // message never escapes the typed family.
    private static readonly Op Admission = Op.Of(name: nameof(Admission));
    public static ElementFault Create(string message) => ValueRejected(Admission, message);
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
