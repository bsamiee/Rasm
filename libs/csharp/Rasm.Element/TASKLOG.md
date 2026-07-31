# [RASM_ELEMENT_TASKLOG]

Open and closed work distilled from `IDEAS.md`. `[01]-[OPEN]` carries task cards with `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` leaders; `[02]-[CLOSED]` carries `[COMPLETE]` or `[DROPPED]` cards. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[QUANTITY_GROUP_COLUMN]-[QUEUED]: Land the `QuantityBag` group axis on the bag shape and the canonical bytes — the seam half of `[QUANTITY_BAG_GROUP_AXIS]`.
- Capability: grouped takeoffs cross the graph identity-lossless per the owning idea.
- Shape: one group column on `ValueBag<V>` in `libs/csharp/Rasm.Element/.planning/Properties/property.md`, presence-delimited count-prefixed write in the `quantitySet` arm of `Node.ToCanonicalBytes` in `libs/csharp/Rasm.Element/.planning/Graph/element.md`; `Merge` precedence untouched, the wire column one append-only numbered field under the `Graph/wire.md` contract-evolution law.
- Unlocks: Bim ingest/egress ends compose the axis the moment the seam carries it.
- Anchors: counted-bag injectivity law (`Projection/address` count-prefix); `Bake` bag merge.
- Atomic: one column and one canonical-bytes arm.

[FEDERATION_HEADER_RULING]-[BLOCKED]: Resolve the federation header-reconciliation ruling that gates `[FEDERATION_AND_PARTIAL_EXCHANGE]`.
- Capability: an answered ruling turns `Federate`/`Extract` from a bet into a design landing in `libs/csharp/Rasm.Element/.planning/Graph/element.md`.
- Shape: question — caller-supplied coordination `Header` with per-source headers demoted to provenance, or a per-source `Header` roster on the federated graph? Route: user interview over the `Header.CanonicalBytes` semantic identity, mixed-`Tolerance` divergence pinned first.
- Unlocks: federated coordination review and `Members`-closed partial exchange.
- Anchors: `Header.CanonicalBytes`; tolerance-quantized measure bytes; `GeoReference` divergence cases.

[REDACTION_IDENTITY_RULING]-[BLOCKED]: Resolve redacted-crossing identity preservation for `[REDACTION_SCOPED_EGRESS]`.
- Capability: the ruling fixes whether redacted crossings preserve or re-derive content keys, and with it the decode-side `AddressUnstable` posture.
- Shape: question and route stated on the owning idea; on resolution, pin the `WireLimits`-sibling redaction policy record and the `Encode` parameterization in `libs/csharp/Rasm.Element/.planning/Graph/wire.md` and the parity-corpus vectors for redacted nodes.
- Unlocks: partner-scoped deliverables off one stored model.
- Anchors: `IRedactorProvider`/`DataClassificationSet` in `libs/csharp/.api/api-redaction.md`; `ContentAddress.Verify` dual.

[AUDIT_FOLD]-[QUEUED]: Pin the coverage ratios, integrity sweeps, and `ModelAudit` receipt for `[MODEL_COMPLETENESS_AUDIT]`.
- Capability: model maturity is one typed receipt a gate or dashboard reads, never a per-consumer query pile.
- Shape: ratio definitions (classified/material-bound/quantified occurrence shares per discipline), integrity sweeps (dangling representation keys, unresolved `ProfileRef`s, orphan nodes, stale assessments), `Verify` drift census, and the graded `ModelAudit` receipt in `libs/csharp/Rasm.Element/.planning/Projection/audit.md` (new page).
- Unlocks: delivery gates and the model-health instrument rows.
- Anchors: `ObjectNodes`/incidence accessors; `AssessmentOutcome` columns; `ConstraintSeverity` grades.

[CORPUS_SPEC]-[BLOCKED]: Seal the graded whole-graph parity pins after the current forge is executable from the tests estate.
- Capability: exact `S`/`M`/`L`/`XL` snapshot addresses turn deterministic witnesses into cross-runtime regression gates.
- Shape: execute every current `GraphForge.Mint` grade from `tests/csharp`, commit the four literal `ContentAddress` values in `Graph/corpus#CORPUS_ROSTER`, and mirror the roster in `libs/python` and `libs/typescript/core`.
- Unlocks: build-breaking whole-graph parity across C#, Python, and TypeScript.
- Anchors: `CorpusGate.Stable`; `CorpusWitness.Snapshot`; `ContentAddress.OfGraph`.
- Tension: arming trigger — the tests-estate corpus harness can execute the settled forge against current source owners; no unset `Option` or self-derived runtime value qualifies as an expected pin, and every forge edit re-derives all four, so the forge settles before a pin lands.

[WIRE_EVOLUTION_PROSE_ALIGN]-[QUEUED]: Growth prose on the graph and material pages aligns to the additive wire law.
- Capability: column-add growth reads as ordinary additive evolution; no page narrates a campaign freeze the wire owner's law forecloses.
- Shape: `libs/csharp/Rasm.Element/.planning/Graph/element.md` and `libs/csharp/Rasm.Element/.planning/Composition/material.md` growth lines — the "frozen this campaign, lands with the wire unfreeze" narration restates as additive column adds under the `Graph/wire.md` contract-evolution law, column content unchanged.
- Unlocks: `RULINGS.md` `[02]` unfreeze-retirement row holds with zero contradicting prose.
- Anchors: `Graph/wire.md` contract-evolution law; the folder `RULINGS.md`.
- Atomic: two growth-line rewrites.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[EQUALITY_POLICY_CENSUS]-[COMPLETE]: the `Generator.Equals` member-policy census landed at `Graph/element.md` — `[StringEquality(StringComparison.Ordinal)]` DECLARED on the `Node.Object` strings the `CanonicalWriter` writes verbatim so equality and content identity cannot fork on a comparer edit, the `[UnorderedEquality]` `Nodes` route named as `DictionaryEqualityComparer<NodeId, Node>` (`FrozenDictionary<K,V>` implements `IDictionary<K,V>`, so the alternative was a `KeyValuePair` multiset falling to reflective `ValueType.Equals`), and `UnionBy` collapsed onto `DistinctBy` with an `IEqualityComparer<K>` policy value that admits a key's own generated comparer where the prior `IEquatable<K>` bound admitted none; `[PrecisionEquality]` REFUSED on both float-bearing surfaces — the generator omits precision members from `GetHashCode` entirely, and every double here is either a `Header.Tolerance`-quantized `MeasureValue` or an `AppearanceSummary` channel preimage to a frozen content key.
[TYPE_QUANTITY_SEAM_ROWS]-[COMPLETE]: `Properties/property#DETAIL_SCHEMA` declares `MassPerLength`, `SurfaceAreaPerLength`, and `VolumePerLength` as owner statics under the `Takeoff` bag at `InheritanceMode.TypeDrivenOverride`, with `Quantities(Option<PropertySource>)` beside `Bag` pinning set name, precedence, and source rank identically; `Composition/material#MATERIAL_PROPERTY` gained `Option<MeasureValue> Density` forwarding over BOTH `Mechanical` and `Orthotropic`, since a directional timber stores density on the orthotropic carrier and one column answers without a case branch.
[OBSERVATION_PAGE]-[COMPLETE]: `Assessment/observation.md` carries the series descriptor and the derived statistics plane, and the ripples landed in one pass — the `Node.Observation` case with its ordinal-7 canonical arm, `Element.Observations` off `BakeObject`, the `AssignKind.Observation` row with its occurrence-only `LegalAssign` arm, the additive `NodeWire` oneof arm 9 with its codec legs, and `ContentAddress.Verify` extended so the default-free node switch stops throwing on the new case.
[FORGE_OBSERVATION_GRADE]-[COMPLETE]: `Graph/corpus#GRAPH_FORGE` mints one `ObservationSeries` per `ObservationStride`-th occurrence through the production `Open`/`Encode`/`From`/`Append` chain and links it by `AssignKind.Observation`, so the ordinal-7 canonical arm, wire oneof arm 9, the occurrence-only assign legality, and the new chunk codec all cross every graded witness; `[CORPUS_SNAPSHOT_PINS]` re-armed to `[BLOCKED]` since the forge edit re-derives all four addresses and no source tree exists to run.
[DETAIL_SCHEMA_READER_PROVISION]-[COMPLETE]: `Properties/property#DETAIL_SCHEMA` declares `StructuralRows` — the scalar topology rows beside the generated restraint and applied-load component families and the `Dofs` projection, each family minted from its own key roster — killing the Bim-writer/Compute-reader literal fork, and `PropertyCategory` blesses each producer's own scope with `Row` the one mint a producer roster composes; `PropertyCategory.Seam` carries the empty prefix, so every landed `DetailSchema` static keeps the bare name an IFC round-trip froze.
[TABLE_TEMPORAL_CATEGORY]-[COMPLETE]: `Graph/table.md` states each family's temporal CATEGORY under the branch analytics ruling rather than a spine preference — `element.assessments` is event-time on `at`, every snapshot family is landing-time because re-tabulating one frozen graph reproduces identical facts and the snapshot address already carries the version identity; `element.observations` and `element.coverages` joined the roster in the same pass, so every `Seq` a baked `Element` carries reaches a dataset. Landing-time batches cannot land today: the custodian appends `landed_at` to the admitted schema yet `ResidenceLanding.Stage` binds only tenant beside the producer's positional cells, so `Conformed` refuses each one on arity — a `Rasm.Persistence` defect returned as a receipt, never a producer-side spine.
[TABLE_ROW_SCHEMA]-[COMPLETE]: the row families and the `Tabulate` fold pin at `Graph/table.md`; the six `TableFamily` rows carry each dataset's columns, key, spine, and measure, and `family.Admission` hands the custodian's gate its whole argument set so a composing root cannot pair one family's columns with another's key.
[HOOK_POINT_ROSTER]-[COMPLETE]: `Projection/observe#HOOK_RAIL` pins the `ElementPoint` roster with the kernel `HookModality` column and the `TraceScope` plane derived off each id, the `ElementFact` payloads with their span `Marks`, and the `ElementHookRail.Of` mint over the kernel point capsule and the admitted `SpanBand` — subscriber faults park as point-attributed `IsolatedFault` rows read through `TapFaults`.
[INSTRUMENT_ROW_TABLE]-[COMPLETE]: `Projection/observe#INSTRUMENT_PROJECTION` pins `ElementInstruments.Rows` as kernel `InstrumentSpec` declarations carrying kind, measurement form, `Buckets` advice, and the tenant slot, beside the dotted slot vocabulary both planes spell and the contributor port the composing root materializes.
[ENVELOPE_VOCAB]-[COMPLETE]: `Graph/wire#EVENT_ENVELOPE` pins the `GraphEventType` token rows and the `GraphEventEnvelope` record with `Attributes`/`Admit` and the W3C trace slots.
[APPEARANCE_PREIMAGE_FREEZE]-[COMPLETE]: `Graph/element#NODE_MODEL` states the `AppearanceSummary` seven-value preimage and the eight-parameter `Of(…, Op key) -> Fin<AppearanceSummary>` arity as law at the factory that owns them, so a richer appearance fact hangs behind the `AppearanceKey` on its producer's own wire and no peer widens the record that keys every stored `Node.Appearance`.
