# [ELEMENT_FAULTS]

`ElementFault` is the seam's one closed `[Union]` fault band (band 2500) over the structural-graph, value-admission, projection, and content-verification failures every `Rasm.Element` entrypoint produces. Seven cases share one `(Op Key, string Detail)` base and derive `Expected` (`IValidationError<ElementFault>`), so band 2500 IS the `Expected` `Code`, a typed case lifts BARE onto the `Fin<T>`/`Validation<Error,T>` rail through the LanguageExt implicit `Error` conversion with no `.ToError()` hop, and each case projects a `Category` for telemetry banding.

`Detail` is IDENTITY, not prose: every case's detail is a frozen `<kind:colon-args>` token the waiver key and the event dedup hash, so the grammar is append-only and a re-wording is a re-key.

`FaultBand` hosts the band-allocation registry HERE because `Rasm.Element` is the lowest stratum every rebinding peer shares: one `[SmartEnum<int>]` row per federation band names its declaring `Owner` and its `Mirror` allocation-versus-reservation discriminant, a duplicate integer fails the generated key lookup at type initialization, and a cross-folder `Fin<T>` reads its origin from the band code.

Wire posture holds HOST-NEUTRAL: `ElementFault` rides the `Fin<T>` rail every seam entrypoint returns and never sits between wire and rail, a total surface mints no case at all, the seam's own `Switch` enforces STRUCTURAL edge legality alone while IFC-semantic legality rides the consumer's `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint.Validate` [M3], and the band keys no `FrozenDictionary` — dispatch is the generated total `Switch` and a `[KeyMemberComparer]` on the fault is the deleted form.

`AdmissionSlots` seats the ONE accumulating admission-slot algebra HERE for the same reason the band registry sits here: `Projection` is the S0 substrate every value, vocabulary, and graph stratum already consumes AND the lowest stratum every rebinding peer shares, so one owner reaches every value-admission gate on the seam and every peer package that rebinds a band above it with no upward edge. The minting arity stamps `ElementFault.ValueRejected`, the band-blind arity lifts a peer's already-minted fault, and one fold serves both.

## [01]-[INDEX]

- [02]-[FAULT_BAND]: `FaultBand` band-allocation registry with its `Owner` and `Mirror` columns, the `Expected`-derived `ElementFault` band-2500 union every seam failure lifts bare, and the frozen `Detail` token grammar its identity consumers hash.
- [03]-[ADMISSION_SLOTS]: the `AdmissionSlots` accumulating slot algebra over the concrete `Validation<Error,_>` carrier — the `Gate` boolean slot in its minting and band-blind arities, the `Accumulate` run fold over a concrete or `K`-typed slot run, the arity-polymorphic `Indexed` scan over a span or a `Seq<A>`, and the `OptionalPositive` scalar slot every seam value-admission composes.

## [02]-[FAULT_BAND]

- Owner: `FaultBand` the `[SmartEnum<int>]` band-allocation registry (one row per federation band, the `Owner` string column naming the declaring surface for telemetry docs, the `Mirror` bool separating an allocated row a peer's `Code` rebinds to from the kernel's pinned reservation — hosted on this page because the seam is the lowest stratum every rebinding peer shares) and `ElementFault` the closed `[Union]` fault band (band 2500) for the structural-graph, value-admission, projection, and content-verification failures, `Expected`-derived (`IValidationError<ElementFault>`) so band 2500 IS the `Expected` `Code` (`Code => FaultBand.Element` reading the registry row). Seven cases share one base carrying the kernel `Op Key` and the `Detail` string, so `Message => Detail` is one line and the band projects `Category` per case, the typed case lifting bare onto the `Fin<T>`/`Validation<Error,T>` failure channel.
- Cases: `NodeAbsent` (an edge endpoint, a `Bake` root, or a replayed-`GraphDelta` reference naming an undeclared `NodeId`) · `RelationshipInvalid` (an edge whose endpoint node-kinds violate the structural edge law over the `Relations/relation#EDGE_ALGEBRA` `Endpoints`, or a cyclic `Compose` ancestry) · `DeltaConflict` (a put-existing / drop-absent / duplicate-link conflicting with the working-graph state) · `ValueRejected` (the band-wide value-admission rail EVERY typed-value smart-constructor across the seam value vocabulary fails onto — the quantity, property, acoustic, material, geospatial, and edge-payload admissions alike, each routing its own `Detail` token under the `[DETAIL_GRAMMAR]` law, the producing page owning the literal) · `ProjectionFailed` (the SEAM-authored structural verdict on a projector's own delta — a folded `GraphDelta` the structural law rejects, its detail the seam's token) · `ProjectorFaulted` (a THROWN foreign projector exception captured at the `Try.lift` boundary funnel and lowered with its raw foreign message — an opaque provider string, never a seam-grammar token, which is exactly why it cases apart from the structural verdict a waiver key hashes; a hook-tap capture parks as `IsolatedFault` on the observe rail's evidence cell, never a case here) · `AddressUnstable` (a content-verification mismatch — a persisted node whose recomputed content id no longer equals its stored `NodeId`, the H7 tamper/corruption gate a `Rasm.Persistence` rehydrate and the cross-runtime parity corpus run) (7); the IFC-semantic legality (containment-relating-must-be-spatial, `Void` element→opening, `Type`-may-not-aggregate-`Occurrence`) routes the consumer's `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint.Validate` → `BimFault.ModelRejected` over `Validation<Error,Unit>` [M3], never re-cased here, and the total MINT path (`ContentAddress.Of`/`OfGraph`/`ToCanonicalBytes`) mints no fault — `AddressUnstable` rides the `Verify` re-hash dual alone.
- Law: [DETAIL_GRAMMAR] — a `Detail` is a FROZEN `<kind:colon-args>` identity token, never prose. The token IS the fault's stable discriminant: `Projection/projection#PROJECTION_CONTRACT` `ConstraintFinding.KeyOf` hashes it into the waiver key a review pins, and the event dedup keys on the same bytes — so a wording edit is a RE-KEY that strands every stored waiver, the grammar grows only by APPEND (a new kind, or a new trailing arg on an existing kind), and a token carrying a culture-formatted scalar forks across locales (`string.Create(CultureInfo.InvariantCulture, …)` is the one interpolation form a detail-minting slot writes). `ProjectorFaulted` is the ONE case outside the grammar and cases apart for it — a foreign exception message is unfrozen provider text, so a key over it was never stable to begin with.
- Entry: the per-case static factories are the fault constructors the `Fin<T>`/`Validation<Error,T>` rail carries — `ElementFault.NodeAbsent(key, detail)` and the six siblings return the `Expected`-derived base over the nested sealed `…Case` record (the production `UiFault.InvalidInput`/`MutationRejected` pattern — the union carries no `[GenerateUnionOps]` because every case already carries an explicit `Op Key` and the kernel union-ops generator is strictly opt-in, while the generated `Switch` survives, so a case constructs through `ElementFault.Case(key, detail)`, never `new …Case(…)` at a call site and never a runtime string code), each an `Error` whose 2500 band IS the `Code`, so `Fin.Fail<ElementGraph>(ElementFault.DeltaConflict(key, detail))` and the implicit `Error → Fin` lift both carry the typed case directly (the `Expected` derivation makes `.ToError()` unnecessary — the case IS the `Error`), and a `Validation`-accumulating admission lifts the bare case through the `[03]-[ADMISSION_SLOTS]` slot algebra rather than a hand-spelled `Fail<Error,Unit>(…)`; one construction idiom serves the whole seam, and a model route, a value admission, and a projection compose on one `Fin<T>` rail without a second fault family.
- Auto: `FaultBand.Allocations` and `Reservations` derive the operational registry partitions from the `Mirror` policy column, so telemetry and collision checks consume the same generated row family instead of re-filtering owners or literals; each seam owner routes the most specific case under an angle-bracketed `<kind:value>` discriminant `Detail` (the exact literal owned by the producing page, never restated here) — `Bake` routes `NodeAbsent` on an absent root and `RelationshipInvalid` on a cyclic `Compose` ancestry, `ElementGraph.Apply` routes `NodeAbsent` on a replayed delta whose added edge names an absent endpoint; `WorkingGraph.Apply` routes `NodeAbsent` on a `Link` to an absent endpoint, `RelationshipInvalid` when the `LegalLink`/`LegalAssign` structural law rejects the endpoint kinds, and `DeltaConflict` on a put-existing / drop-absent / duplicate-link state conflict; the typed-value smart-constructors across the seam value vocabulary all route `ValueRejected` on their admission miss; `Assemble` lowers a captured projector exception into `ProjectorFaulted(key, error.Message)` through the `Try.lift(() => projector.Project(ctx)).Run().MapFail(...)` boundary funnel (preserving the RAW foreign message — the kernel `Op.Catch` re-wrap into `Fault.InvalidResult` boilerplate that erases the typed arm is NOT the seam idiom) and routes `ProjectionFailed` when a folded `GraphDelta` fails the structural law; `ElementHookRail.Of` routes `ValueRejected` when the kernel capsule refuses a gate on an observe-only point, lowering that capsule verdict onto the band under the mint's own `Op` so a composition failure reads its origin from band 2500 like every other entrypoint, while the same `Projection/observe` composition parks a captured subscriber failure as a point-attributed `IsolatedFault` row in its `TapFaults` evidence cell — the kernel capsule's shield, the emitter's rail untouched; `ContentAddress.Verify` routes `AddressUnstable` when a node's re-derived id no longer equals its stored `NodeId` — a non-rooted node's re-projected content id or a TYPE `Object`'s re-minted `NodeId.RootedType` (over its `Representations`-excluded `ToTypeSeedBytes` seed) drifting, an OCCURRENCE `Object`'s random Guid-v7 verifying vacuously, the snapshot sweep accumulating every drift over `Validation<Error,Unit>`.
- Receipt: `ElementFault` is the typed fault evidence on the `Fin<T>`/`Validation<Error,T>` failure rail; no generic `IFault`/error-code abstraction, the cases stay typed per seam concern, and a recovery reads `error.IsType<ElementFault.ValueRejectedCase>()` for the arm, `error.HasCode(2500)` for band membership, or `error.Category` (`"Value"`, `"Projection"`, `"Address"`, …) through the kernel `FaultExtensions` extension property for telemetry banding, never a message substring.
- Packages: `Rasm` (the kernel `Op` operation key + the `Domain.Expected` base each case derives through + the `FaultExtensions.Category` projection), Thinktecture.Runtime.Extensions (`[Union]`/`IValidationError<T>` + the `[SmartEnum<int>]` registry with the generated key lookup and implicit key conversion), LanguageExt.Core (`Error`/`Fin`/`Validation`/`Try`).
- Growth: a new structural-graph or value-admission failure routes onto one of the seven existing arms — a new edge legality is `RelationshipInvalid`, a new typed-value admission is `ValueRejected` plus one appended `Detail` kind, a new graph-state conflict is `DeltaConflict`, a content-verification mismatch is `AddressUnstable` — never a new arm; a projector's structural rejection routes `ProjectionFailed`, its thrown foreign exception `ProjectorFaulted`, and an IFC-semantic legality rejection the consumer's `IGraphConstraint` `Validation<Error,Unit>`; a new sub-domain (`assessment`, `coverage`, `reference`) routes its rejection onto an existing arm — a missing assessment input is `NodeAbsent`, an out-of-range coverage band or an unresolvable CRS is `ValueRejected`; a new FEDERATION band is ONE `FaultBand` row, its owning package consuming the row in its own `Code` override; zero new `ElementFault` arm, zero new `Category` arm without a case.
- Boundary: `ElementFault` mints ONLY the seven seam cases and is `Expected`-derived — a parallel band that re-cases a value-admission miss per sub-domain, an eighth arm, or a re-cased kernel `GeometryFault`/`Rasm.Bim` `BimFault` case is the deleted form; the typed case lifts BARE onto the rail (`Fin.Fail<T>(ElementFault.X(key, detail))` / the `[03]-[ADMISSION_SLOTS]` slot lift) and a `.ToError()` hop OR a hand-built `Error.New(2500, message)` bypassing the typed factory is the named defect; the structural verdict and the captured foreign throw are TWO cases because their details are two grammars — one frozen and key-bearing, one opaque provider text — so the one-case form that carried both is the deleted shape a waiver key could not survive; the seven homogeneous `(Op Key, string Detail)` cases share one base so `Message => Detail` is ONE line — a central `Message => Switch(…)` over seven identical `c.Detail` arms is the repeated-arm collapse defect, the central form belonging to the production `UiFault` band whose heterogeneous cases genuinely require it; exception-style control flow in domain logic is the named defect (EXPRESSION_SPINE) — a foreign `Exception` (a GeometryGym parse fault, a VividOrange miss) enters ONLY through the `Try.lift(...).Run().MapFail(...)` boundary funnel and never crosses a seam signature; the IFC-schema legality rejection is the consumer's `IGraphConstraint` concern, never an `ElementFault` arm, because the band carries no IFC vocabulary; and the content-address MINT path is total — routing a non-finite value or guarding the mint through `AddressUnstable` is the deleted form, the arm existing for the `Verify` mismatch alone.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Globalization;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
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
    public static readonly FaultBand Raster      = new(2460, owner: "Rasm.Materials/Raster", mirror: false);
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
    // The SEAM's own structural verdict on a projector delta — Detail is a frozen [DETAIL_GRAMMAR] token, so a
    // ConstraintFinding waiver keyed on it survives every run.
    public sealed record ProjectionFailedCase(Op Key, string Detail)    : ElementFault(Key, Detail) { public override string Category => "Projection"; }
    // The captured FOREIGN throw — Detail is the provider's raw exception message, deliberately OUTSIDE the frozen
    // grammar, which is why it cannot share the arm a waiver key hashes.
    public sealed record ProjectorFaultedCase(Op Key, string Detail)    : ElementFault(Key, Detail) { public override string Category => "ProjectorFault"; }
    // ContentAddress.Verify re-hash dual (H7) raises this alone; the mint path stays total.
    public sealed record AddressUnstableCase(Op Key, string Detail)     : ElementFault(Key, Detail) { public override string Category => "Address"; }

    // Per-case static factories are the ONE construction idiom the whole seam calls (ElementFault.ValueRejected(key,
    // detail)), returning the Expected-derived base so the case lifts bare onto Fin<T>/Validation<Error,T>.
    public static ElementFault NodeAbsent(Op key, string detail)          => new NodeAbsentCase(key, detail);
    public static ElementFault RelationshipInvalid(Op key, string detail) => new RelationshipInvalidCase(key, detail);
    public static ElementFault DeltaConflict(Op key, string detail)       => new DeltaConflictCase(key, detail);
    public static ElementFault ValueRejected(Op key, string detail)       => new ValueRejectedCase(key, detail);
    public static ElementFault ProjectionFailed(Op key, string detail)    => new ProjectionFailedCase(key, detail);
    public static ElementFault ProjectorFaulted(Op key, string detail)    => new ProjectorFaultedCase(key, detail);
    public static ElementFault AddressUnstable(Op key, string detail)     => new AddressUnstableCase(key, detail);

    // IValidationError<ElementFault>.Create — the string-only admission the generated converter bridge calls on a
    // deserialization reject; routes the unspecific case under a boundary-admission Op (never a default Op) so a raw
    // message never escapes the typed family.
    private static readonly Op Admission = Op.Of(name: nameof(Admission));
    public static ElementFault Create(string message) => ValueRejected(Admission, message);
}
```

## [03]-[ADMISSION_SLOTS]

- Owner: `AdmissionSlots` the accumulating admission-slot algebra over the concrete `Validation<Error,_>` carrier (`Gate` the boolean slot, `Accumulate` the run fold, `Indexed` the arity-polymorphic element scan, `OptionalPositive` the `Option<double>` scalar slot) every value-admission factory across the seam AND every peer package rebinding its own `FaultBand` row above the seam composes. `Gate` discriminates on WHO MINTS THE REFUSAL: the `(holds, key, detail)` arity mints `ElementFault.ValueRejected` under the caller's `Op` and the producing page's own `<kind:value>` `Detail` literal, while the band-blind `(holds, refusal)` arity lifts an `Error` its raise site already minted, so a peer band (`Rasm.Fabrication` `FabricationFault`, `Rasm.Bim` `BimFault`) accumulates on its own vocabulary through this one fold. A seam page takes `using static Rasm.Element.Projection.AdmissionSlots;` and reads `Gate(…)`/`Accumulate(…)` unqualified; a peer package composing its own band qualifies the call.
- Entry: `Gate(holds, key, detail)` lifts one boolean invariant into a minting slot and `Gate(holds, refusal)` the same invariant onto a refusal its raise site owns; `Accumulate(slots)` traverses a slot run into one slot, the applicative join unioning every fault through `Error.Combine`/`ManyErrors`, over a concrete run OR the `K<Validation<Error>, Unit>` run a caller's own wrapper return, generated `Switch` arm, or interface-typed projection already holds; `Indexed(values, holds, key, name)` scans an element run under one predicate and retains every offending element as `<name:index=…:value=…>`, discriminating on the INPUT SHAPE — a `ReadOnlySpan<double>` for the caller holding contiguous storage, a `Seq<A>` for every carrier-shaped caller — so a non-span caller composes the one owner instead of re-rolling the scan or materializing a copy to reach the span arity; `OptionalPositive(value, name, key)` passes an absent `Option<double>` through and rails a present non-finite or non-positive magnitude as `<name-non-positive:value>`. A closed-field product joins its slots through the tuple `.Apply` (`.As()` re-anchoring the `K`-typed join result) and an open run through `Accumulate`, and `.ToFin()` collapses ONCE at the seam return, so the public rail stays `Fin<T>` while a malformed lowering reports EVERY violated invariant in one failure.
- Auto: the slot return is the CONCRETE `Validation<Error,_>` because the lift IS a user-defined implicit conversion (`A → Validation<F,A>` on the success arm, `F → Validation<F,A>` on the fault arm) and C# excludes interface types from user-defined conversion targets — a `K<Validation<Error>,_>` return rejects both ternary arms outright, so the concrete carrier is what makes the bare-case lift compile and the `Success`/`Fail` cast the deleted ceremony; the concrete carrier is itself a `K<Validation<Error>,_>` by implicit reference conversion, so the tuple `.Apply` fan-in and the `Traverse` run fold both bind on the slot exactly as written, both `Gate` arities return it, and the K-run `Accumulate` arity exists for the INPUT side alone — `Seq<A>` is invariant, so a caller holding K-typed slots reaches the join through the second arity rather than a re-rolled fold or a materializing re-cast, and its answer is the same concrete carrier; `Indexed`'s SPAN arity carries its accumulation in a loop because a `ref struct` cannot cross a lambda — the loop is the exemption's whole extent, and the `Seq` arity composes the indexed `Map` and `Accumulate` with no statement at all, so the exemption never leaks to a caller that never held a span.
- Receipt: an accumulated admission failure is one `Fin.Fail` carrying the `ManyErrors` union of every violated invariant, each a typed `ElementFault.ValueRejected` a recovery reads by `IsType`/`HasCode`/`Category` exactly as a single-case failure — the accumulation changes the arity of the evidence, never its taxonomy.
- Packages: LanguageExt.Core (`Validation<Error,_>`/`K<F,A>`/`Seq`/`Option`/`Fin` + the `Apply` tuple join, the indexed `Seq.Map`, the `Traverse` run fold, and the implicit `Error` lift the bare case rides), `Rasm` (the kernel `Op` op-key each slot stamps), BCL inbox (`CultureInfo.InvariantCulture` the detail-token mint pins).
- Growth: a new invariant is one slot at its call site, never a per-constructor guard chain; a new slot SHAPE lands here the moment a SECOND page needs it, and stays on its own page while one page owns it — a slot whose payload is one page's domain (a `MeasureValue` column, a direction cosine, a CRS leg) composes these rather than joining them; a peer package rebinding its own `FaultBand` row composes the band-blind `Gate` and this same fold, and a new carrier arity lands here the moment a peer's own slot shape needs it.
- Boundary: the slot carrier is CONCRETE and uniform — the LIFT cannot reach a `K<Validation<Error>,_>` target, so a helper whose body IS the ternary returns `Validation<Error,_>`, while a helper merely forwarding an already-lifted slot widens to the interface at its own return and re-mints nothing; `AdmissionSlots` is the ONE accumulating-admission owner the seam and every package above it share — a per-page `Gate`/`Accumulate` copy is the named defect (every copy forks the accumulation law the moment one is edited, and the twin two owners on one page mint the moment the second needs it), a PEER-package copy is the same defect twice over (its type name collides with this owner under `CS0104` the moment one page imports both namespaces plainly, and the K-carrier return that copy reaches for never compiled), and so is an early-abort guard chain or a `Bind` fold that reports only the first miss where the applicative fold reports all; the band-blind arity is the PEER's door, never a seam escape hatch — a `Rasm.Element` value-admission takes the minting arity so its `Detail` stays under `[DETAIL_GRAMMAR]`, the case is always `ValueRejected` under the caller's `Op` and the producing page's `Detail` literal, and a slot minting a second arm or a seam call site handing the band-blind arity a bare `Error.New` is the deleted form; a DEPENDENT check binds INSIDE its own slot (the leaf-then-composite `COMPOSITE_ADMISSION` order), never as a sibling slot reading another's result.

```csharp signature
// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class AdmissionSlots {
    // One boolean slot: holds -> unit, else the NAMED ValueRejected. The return is the CONCRETE carrier because both
    // arms reach it through Validation's user-defined implicit conversions, and a user-defined conversion cannot
    // target an interface — a K<Validation<Error>, Unit> return type rejects BOTH arms (CS0029).
    public static Validation<Error, Unit> Gate(bool holds, Op key, string detail) =>
        holds ? unit : ElementFault.ValueRejected(key, detail);

    // The BAND-BLIND arity: a package rebinding its own FaultBand row above this stratum (Rasm.Fabrication's
    // FabricationFault, Rasm.Bim's BimFault) already minted the refusal its raise site owns, so the slot lifts it
    // verbatim and stays blind to which band answers — one accumulation law across every band, where a package-local
    // copy would fork it and collide on the bare type name the moment a page imports both namespaces.
    public static Validation<Error, Unit> Gate(bool holds, Error refusal) =>
        holds ? unit : refusal;

    // Independence rides ACROSS slots: the applicative traversal unions every fault, so one malformed lowering reports
    // every violated invariant at once rather than first-fault-wins (a Bind fold reports only the first).
    public static Validation<Error, Unit> Accumulate(Seq<Validation<Error, Unit>> slots) =>
        slots.Traverse(identity).As().Map(static _ => unit);

    // The K-run arity, discriminating on INPUT SHAPE like Indexed does: Seq<A> is invariant, so a caller whose slots
    // are typed K<Validation<Error>, Unit> — a page-local wrapper's declared return, the common type a generated
    // Switch arm infers, a projection over the interface — folds through the SAME join instead of re-rolling it. The
    // answer is the concrete carrier either way; only the run's element type differs.
    public static Validation<Error, Unit> Accumulate(Seq<K<Validation<Error>, Unit>> slots) =>
        slots.Traverse(identity).As().Map(static _ => unit);

    // One predicate over an element run, every offending element retained with its index and value, so a corrupted
    // spectrum, band set, or matrix yields one complete diagnostic instead of its first bad element. TWO arities over
    // one name, discriminated by INPUT SHAPE: this one reads contiguous storage the caller already holds (Seq.AsSpan,
    // ReadOnlyMemory.Span, ImmutableArray.AsSpan) with no copy, and a span cannot cross a lambda, so the applicative
    // join runs in the loop under the named EXPRESSION_SPINE kernel exemption.
    public static Validation<Error, Unit> Indexed(ReadOnlySpan<double> values, Func<double, bool> holds, Op key, string name) {
        Validation<Error, Unit> scan = Success<Error, Unit>(unit);
        for (int index = 0; index < values.Length; index++) {
            scan = (scan, Gate(holds(values[index]), key, Slot(name, index, values[index])))
                .Apply(static (_, _) => unit).As();
        }
        return scan;
    }

    // The CARRIER arity: a caller holding a Seq of any element type projects each item to its slot through the indexed
    // Map (value-first argument order) and folds through the ONE Accumulate join, so no statement, no materializing
    // copy, and no re-rolled loop appears at a non-span call site. The span arity's kernel exemption stops here.
    public static Validation<Error, Unit> Indexed<A>(Seq<A> values, Func<A, bool> holds, Op key, string name) =>
        Accumulate(values.Map((value, index) => Gate(holds(value), key, Slot(name, index, value))));

    // Absence is legal (never a sentinel a fold reads as real); a present magnitude gates finite-and-strictly-positive
    // and the fault CARRIES the offending value. The Option passes through so a tuple Apply reads the admitted column.
    public static Validation<Error, Option<double>> OptionalPositive(Option<double> value, string name, Op key) =>
        value is { IsSome: true, Case: double scalar } && (!double.IsFinite(scalar) || scalar <= 0.0)
            ? ElementFault.ValueRejected(key, string.Create(CultureInfo.InvariantCulture, $"<{name}-non-positive:{scalar:R}>"))
            : value;

    // The ONE indexed detail-token mint: string.Create pins the INVARIANT culture, because a Detail is identity under
    // [DETAIL_GRAMMAR] and a decimal-comma ambient culture forks the waiver key an interpolated magnitude lands in. The
    // element formats through its own ToString with no specifier — the double surface is shortest-round-trippable by
    // default, so the `:R` a double-only scan would carry buys nothing and costs the generic arity.
    static string Slot<A>(string name, int index, A value) =>
        string.Create(CultureInfo.InvariantCulture, $"<{name}:index={index}:value={value}>");
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
