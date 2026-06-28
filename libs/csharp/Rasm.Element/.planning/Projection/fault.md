# [ELEMENT_FAULTS]

The seam fault rail: `ElementFault` a closed `[Union]` band (band 2500) for the structural-graph and projection failures every `Rasm.Element` entrypoint produces — `NodeAbsent` (an edge endpoint, `Bake` root, or `GraphDelta` reference naming a `NodeId` the graph never declares), `RelationshipInvalid` (a `Relationship` whose endpoint node-kinds violate the seam's structural edge law — a `Compose` whose part is a non-`Object`, an `Assign` whose target is not a `PropertySet`/`QuantitySet`/`Assessment` bag, a `Void` between two type-level objects), `DeltaConflict` (a `GraphMutation` that puts an already-present `NodeId`, drops an absent one, or links a duplicate edge), `ValueRejected` (a `PropertyValue`/`MeasureValue`/`Dimension` admission failure — a non-finite scalar, an out-of-`[0,1]` ratio, a band-arity mismatch, an unresolvable measure unit), `ProjectionFailed` (an `IElementProjection.Project` or the `Assemble` fold returning a delta the structural law rejects, or a captured projector exception lowered at the boundary), and `AddressUnstable` (a `ToCanonicalBytes`/`ContentAddress` canon failure — a non-finite measure reaching the byte projection, or a node whose canonical ordering cannot be derived). The band is the seam's ONLY fault owner: a projector aspect (`Bim` `SemanticProjector`, `Materials` `MaterialProjector`) lowers ITS foreign rejection onto `ProjectionFailed`, and an IFC-semantic legality rejection lives in the consumer's `IGraphConstraint` (`Projection/projection#GRAPH_CONSTRAINT`) returning `Validation<Error,Unit>`, never a sixth seam arm.

`ElementFault` is `Expected`-derived (`IValidationError<ElementFault>`) so a bare typed case lifts directly onto the `Fin<T>`/`Validation<Error,T>` rail through the LanguageExt implicit `Error` conversion — the band IS the `Expected` `Code`, the `Op` key carries the operation context, and a recovery reads `error.HasCode(2500)` or `error.Is<ElementFault.RelationshipInvalid>` rather than matching the message substring. The seam carries no fault-band string-comparer accessor: `ElementFault` dispatches through its generated total `Switch`, keys no `FrozenDictionary`, and a `[KeyMemberComparer]` on the fault is the deleted form.

Wire posture: HOST-NEUTRAL. `ElementFault` rides the `Fin<T>` rail `ElementGraph.Apply` (`Graph/element#ELEMENT_GRAPH`, the validated delta-replay) and the authoring `WorkingGraph.Apply` (`Graph/delta#GRAPH_DELTA`), `ElementGraph.Bake` (`Graph/element#ELEMENT_GRAPH`), `MeasureValue.Of` (`Properties/quantity#MEASURE_VALUE`), `PropertyValue.Of` (`Properties/property#PROPERTY_VALUE`), `MaterialComposition`/`MaterialPropertySet` admission (`Composition/material#MATERIAL_NODE`), `GeoReference.Admit` (`Geospatial/reference#GEO_REFERENCE`), `ContentAddress.Of` (`Projection/address#CONTENT_ADDRESS`), and `Assemble` (`Projection/projection#PROJECTION_CONTRACT`) return — and never sits between wire and rail.

## [01]-[INDEX]

- [01]-[FAULT_BAND]: the `ElementFault` `[Union]` band-2500 (`NodeAbsent`/`RelationshipInvalid`/`DeltaConflict`/`ValueRejected`/`ProjectionFailed`/`AddressUnstable`), the `Op`-keyed case constructors, and the `Expected`/`IValidationError` lift every seam `Fin.Fail` rides.

## [02]-[FAULT_BAND]

- Owner: `ElementFault` the closed `[Union]` fault band (band 2500) for the structural-graph and projection failures, mirroring the sibling `Rasm.Materials` `ConstructionFault` (band 2350) and `Rasm.Bim` `BimFault` (band 2600) band shapes — one `Op`-keyed case per failure carrying its detail, an `Expected` `Code` of 2500, and a `Category` `Switch` so a recovery reads the typed arm.
- Cases: `NodeAbsent` (an edge endpoint / `Bake` root / `GraphDelta` reference naming an undeclared `NodeId`, produced by `Graph/element#ELEMENT_GRAPH` `Bake`/`Apply` — the latter on a corrupt replayed delta — and `Graph/delta#GRAPH_DELTA` `WorkingGraph.Apply`) · `RelationshipInvalid` (an edge whose endpoint node-kinds violate the structural edge law, produced by `Graph/delta#GRAPH_DELTA` `Apply`·`Relations/relation#EDGE_ALGEBRA` `Endpoints`) · `DeltaConflict` (a put/drop/link conflicting with the graph state, produced by `Graph/delta#GRAPH_DELTA` `Apply`) · `ValueRejected` (a `PropertyValue`/`MeasureValue`/`Dimension` admission miss, produced by `Properties/quantity#MEASURE_VALUE`·`Properties/property#PROPERTY_VALUE`·`Composition/acoustic#ACOUSTIC_FOLDS`·`Composition/material#MATERIAL_NODE`) · `ProjectionFailed` (an `IElementProjection.Project`/`Assemble` delta the structural law rejects or a captured projector fault, produced by `Projection/projection#PROJECTION_CONTRACT`) · `AddressUnstable` (a canonical-byte/`ContentAddress` canon miss, produced by `Projection/address#CONTENT_ADDRESS`·`Graph/element#NODE_MODEL` `ToCanonicalBytes`) (6).
- Entry: the union cases are the fault constructors the `Fin<T>` rail carries — `ElementFault.NodeAbsent(key, detail)` and the five siblings are the Thinktecture-generated case factories, each an `Expected`-derived `Error` whose 2500 band IS the `Code`, so `Fin.Fail<ElementGraph>(ElementFault.DeltaConflict(key, detail))` and the implicit `Error`→`Fin` lift both carry the typed case directly; one lowering idiom serves the whole seam, and a model route, a value admission, and a projection compose on one `Fin<T>` rail without a second fault family.
- Auto: each seam owner routes the most specific case — `Bake` and `Apply` route `NodeAbsent(key, $"<node-absent:{id}>")` on an `Incidence`/`Nodes` miss; `Apply` routes `RelationshipInvalid(key, detail)` when the `Relations/relation#EDGE_ALGEBRA` `Endpoints` law rejects the endpoint kinds and `DeltaConflict(key, detail)` on a put-existing/drop-absent/duplicate-link; `MeasureValue.Of`/`PropertyValue.Of`/`Acoustic.Of`/`Dimension.Validate` route `ValueRejected(key, detail)` on a non-finite, out-of-unit, or band-arity miss; `Assemble` captures a projector fault through `Op.Catch` into `ProjectionFailed(key, error.Message)` and routes `ProjectionFailed` when a folded `GraphDelta` fails the structural law; `ContentAddress.Of`/`ToCanonicalBytes` route `AddressUnstable(key, detail)` when a measure reaching the byte projection is non-finite after tolerance quantization.
- Receipt: `ElementFault` is the typed fault evidence on the `Fin<T>` failure rail; no generic `IFault`/error-code abstraction, the cases stay typed per seam concern, and a recovery reads `error.Is<ElementFault.ValueRejected>()` or `error.HasCode(2500)`, never a message substring.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`), LanguageExt.Core (`Expected`/`Error`/`Fin`/`Validation`), `Rasm` (the kernel `Op` op-key carried per case).
- Growth: a new structural-graph or value-admission failure is one `ElementFault` arm carrying the next ordinal in the 2500 band; a projector's foreign rejection routes `ProjectionFailed` and an IFC-semantic legality rejection routes the consumer's `IGraphConstraint` `Validation<Error,Unit>`, never a sixth seam arm; a new sub-domain (`assessment`, `coverage`, `reference`) routes its rejection onto one of the six existing arms — a missing assessment input is `NodeAbsent`, an out-of-range coverage band is `ValueRejected`, an unresolvable CRS is `ValueRejected`.
- Boundary: `ElementFault` mints ONLY the six seam cases — a parallel band that re-cases a value-admission miss per sub-domain, or a `ProjectionFailed` re-cased per projector, is the deleted form; every seam page lowers through the generated case factory passed directly into `Fin.Fail<T>(...)` (the `Expected` lift makes `.ToError()` unnecessary — the case IS the `Error`), and a hand-built `Error.New(2500, message)` bypassing the typed case is the named defect; faults route through the `Fin`/`Validation`/`Eff` rails and exception-style control flow in domain logic is the named defect — a captured projector exception enters through the kernel `Op.Catch` funnel so a foreign `Exception` never crosses a seam signature; an IFC-schema legality rejection (containment-relating-must-be-spatial, `Void` must be `Object`→opening) is the consumer's `IGraphConstraint` concern returning `Validation<Error,Unit>`, never an `ElementFault` arm, because the seam's total `Switch` enforces ONLY the structural edge law and the band carries no IFC vocabulary.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element;

// --- [ERRORS] -----------------------------------------------------------------------------
[Union]
public abstract partial record ElementFault : Expected, IValidationError<ElementFault> {
 private ElementFault(Op key, string detail) : base(detail, 2500, None) => Key = key;

 public Op Key { get; }

 // Expected.Create is the string-only admission the generated bridge calls; the band routes
 // the unspecific case so a raw message never escapes the typed family.
 public static ElementFault Create(string message) => new NodeAbsent(default, message);

 public sealed record NodeAbsent(Op Key, string Detail) : ElementFault(Key, Detail) { public override string Category => "NodeAbsent"; }
 public sealed record RelationshipInvalid(Op Key, string Detail): ElementFault(Key, Detail) { public override string Category => "Relationship"; }
 public sealed record DeltaConflict(Op Key, string Detail) : ElementFault(Key, Detail) { public override string Category => "Delta"; }
 public sealed record ValueRejected(Op Key, string Detail) : ElementFault(Key, Detail) { public override string Category => "Value"; }
 public sealed record ProjectionFailed(Op Key, string Detail) : ElementFault(Key, Detail) { public override string Category => "Projection"; }
 public sealed record AddressUnstable(Op Key, string Detail) : ElementFault(Key, Detail) { public override string Category => "Address"; }
}
```

## [03]-[RESEARCH]

- [BAND_ALIGNMENT]: the 2500 band sits between the kernel `Rasm` `GeometryFault` (band 2400) and the `Rasm.Bim` `BimFault` (band 2600), distinct from the `Rasm.Materials` `ConstructionFault` (2350) / `MaterialFault` (2450) bands, so a cross-folder `Fin<T>` carrying a seam-routed value rejection and a Bim-routed model rejection reads its origin from the band code; the `Expected`-derived `IValidationError<ElementFault>` lift matches the `ConstructionFault` shape exactly so a seam case passed bare into `Fin.Fail<T>` requires no `.ToError()` hop, the band IS the `Expected` `Code`.
- [CONSTRAINT_SPLIT]: the structural edge law the seam's total `Switch` enforces (`Relations/relation#EDGE_ALGEBRA` endpoint-kind legality) rails `RelationshipInvalid`, while the IFC-semantic legality a consumer layers rides the `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint.Validate` `Validation<Error,Unit>` — the two are disjoint by design so the seam carries no IFC vocabulary and the Bim-implemented constraint carries no structural-graph mechanics; the two-interface split has the seam owning the structural floor and the consumer owning the schema legality.
