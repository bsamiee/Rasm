# [AUDIT_CORE] — line-by-line doctrine audit of libs/typescript/core

Scope: all 22 design pages under `libs/typescript/core/.planning/` graded against the full `docs/stacks/typescript` doctrine (README + 11 concept pages) and the planning grammar. Findings only — no edits applied.

## [0]-[DOCTRINE_LAW_STATEMENT]

The sixteen laws in five groups (README `[02]-[DOCTRINE]`), each with its bearing on core:

FLOW
- `EXPRESSION_SPINE` — domain logic is expression-shaped on `Effect<A,E,R>`; dependence binds (`flatMap`/`gen`), independence accumulates (`all`/`zipWith`); statements only in measured/platform kernels. Core: every fold/step is expression-shaped; the d2 engine drains and the marked byte kernels are the only statement seams.
- `BOUNDARY_ADMISSION` — decode exactly once through a Schema owner, `ParseError` lifts at the seam, interior never sees `null`/provider shapes. Core: `interchange` is the one decode boundary; `value`/`state` never re-validate.
- `CAPABILITY_CHANNEL` — dependency is a Tag satisfied only by Layer. Core: `FaultEnricher`, `Quarantine`, `Dial`, `Gateway`, `DescriptorGate`, `WkbParser`, `AvailabilityGate`, `SupportIntake` all ride Tags/Services.

SHAPE
- `SCHEMA_AUTHORITY` — the Schema owner is the single shape authority; a hand DTO beside it is the defect. Core: honored corpus-wide; wire twins derive via `Proto.family`/`Pack.schema`/`FromWire`.
- `SHAPE_BUDGET` — one deep owner replaces ~5 loose shapes; variants are tags in one closed family; brands are field refinements. Core: strongly held (`Digest`, `Merge`, `Fold`, `Wire`); the erased `Record<string,unknown>` in `fold.grouped` is the one breach (F3).
- `DEEP_SURFACES` — one/two exports, `_`-interior, no barrels. Core: mostly held; `codec.ts` surfaces ~30 loose landing classes — the largest strain (F4).
- `MODAL_ARITY` — one entrypoint owns every modality via input-shape discrimination; one `Function.dual`. Core: `Fold.run`, `Digest.mint`, `Replay.view`, `Quarantine.divert`, `gated`/`salvaged` duals all correct.
- `ANTICIPATORY_COLLAPSE` — shape for the family, next feature is one row. Core: exemplary — every page's Growth line proves the one-row diff.

DERIVATION
- `VOCABULARY_VALUES` — one contract-checked `as const` table drives every secondary surface. Core: the assembled-owner + guard-pair form is used pervasively (`FaultClass`, `Budget`, `Degrade`, `Wire`, `Hops`, `Availability`, `_BURN`).
- `DERIVED_TYPES` — types compute from one anchor; owner pre-solves inference. Core: `keyof typeof`, indexed access, `Types.Simplify` everywhere; `Capability.Sdk<T>` mapped derivation.
- `SEMANTIC_NAMING` — canonical bounded-context terms. Core: consistent.

MATERIAL
- `LIBRARY_DEPTH` — the Effect ecosystem is stdlib; use the deepest primitive. Core: deep use of `HashMap`/`SortedMap`/`STM`/`Machine`/`ExecutionPlan`/`Schedule`/typeclass instances; d2 engines mined at operator depth.
- `INLINE_COMPOSITION` — wrapping/policy attaches at the owner declaration. Core: `Budget` compiles schedules at module init; `Dial` builds transports at the Layer; telemetry rows ride the owner.

INTEGRATION
- `ROOT_REBUILD` — weave capability into the owner, no shims. Core: adopted-verbatim wire names land into owners, no migration surfaces.
- `ONE_HOP_RESOLUTION` — a name resolves in one hop. Core: held; `Wire.diverted → _diverted` is the sanctioned single alias.
- `COMPOSED_IMPLEMENTATION` — features compose the page owners; a need with no spelling extends a law first. Core: the `state` folds compose `merge`+`fold`+`clock`+`causal`; the one place a need lacks a spelling and forces a hand implementation is the missing `Merge.hashMap` instance (F2).

Verdict on grounding: all high-risk external members spot-checked (`cbor-x` `decodeMultiple`/`setSizeLimits`, `@msgpack/msgpack` `encodeSharedRef`/`decodeMultiStream`, d2mini `orderByWithFractionalIndexBTree`/`loadBTree`/`groupByOperators`, d2ts `Index.addValue`/`reconstructAt`/`compact`/`Antichain`/`iterate`, rfc6902 `VoidableDiff`/`createTests`/`isDestructive`) are present in the folder `.api/` catalogs — no phantoms found in the sampled set.

Overall: this folder is already near the 13/10 bar. The findings below are surgical collapse/consistency/erasure repairs, not evidence of naive code.

---

## [1]-[PER_PAGE]

### value/schema.md — clean
- `_deeper` (l.86) marked-kernel frontier; `_canonical` (l.32) marked-kernel `Intl` throw-fold; both card-declared exemptions. `Ingress.bounded` returns `Schema.Schema<A, unknown, R>` (l.112) — encoded erased to `unknown`, correct at an untrusted depth gate.
- No actionable finding.

### value/identity.md — clean
- `_scope = Schema.decodeSync(_Scope)` (l.41) trusted-construction mint over pattern-proven parts; matches the clock.md idiom. Two owners (`AppIdentity`, `TenantContext`) — distinct decode-boundary concepts, justified.
- No actionable finding.

### value/contentKey.md — clean
- `Digest` is a deep polymorphic owner (width rows + session + keyed mac + binary twin) — model owner form. `_walk` (l.110) marked kernel. `_compiled` (l.107) global-value memo per row. Strong.
- No actionable finding.

### value/clock.md — clean
- `Hlc`/`Uncertainty` share one branded physical axis. `_unpacked`/`_packed` (l.53/58) marked kernels. `Uncertainty.around` bound is modality-polymorphic via `_isGrade` guard (l.153) — correct discriminant-on-value.
- No actionable finding.

### value/quantity.md — clean
- `Dimension` algebra via `_folded`/`_mapped` kernels (l.32/43); `Quantity` on an `Either<QuantityFault>` rail; total `Order<Quantity>` correctly rejected as forging cross-dimension comparability. `Number.isFinite` (l.123) is the JS global re-checking computed magnitudes — legitimate.
- No actionable finding.

### value/fault.md — clean
- `FaultClass`/`Budget`/`Degrade` are three assembled vocabulary owners with guard pairs; `Budget` compiles class-gated `Schedule` values at module init (l.210); `FaultEnricher` is a Tag port with a shipped `identity` Layer. Textbook `VOCABULARY_VALUES` + `INLINE_COMPOSITION`.
- No actionable finding.

### state/merge.md — F2 root (roster gap), minor cast cluster
- **F2 (root cause here):** the `INSTANCE_ROSTER` provides `union` for `Record.ReadonlyRecord` (l.298-303) but has **no keyed-merge instance for `HashMap.HashMap`**, even though the whole branch's keyed-state law (`values.md`) is HashMap-first. Consequence lands in `evidence.md` (see F2). Add a `hashMap: <V>(row) => Instance<HashMap<K,V>>` roster row (structural-key present-in-one-keep / present-in-both-combine, `empty: some(HashMap.empty())`), parallel to `union`.
- Minor: `_mapped`/`_struct` (l.89-127) carry three `as` casts because `Record.map` erases per-key correlation — card-declared exemption, and the reverse-map limitation is a real Effect constraint; defensible, but note it is the same erasure the `derivation.md` REVERSE_MAPPED_SITE avoids with a typed mapped parameter. Low.

### state/fold.md — F1, F3 (the two deepest findings in the folder)
- **F1 — engine-lane scaffold duplication (highest leverage).** Six lanes hand-roll the identical D2 scaffold: `gate = makeSemaphore(1)` + `new Mini.D2()` / `new Diff.D2(...)` + `newInput` + `pending: Array<...> = []` + `input.pipe(...output(delta => for(...) pending.push))` + `graph.finalize()` + `SubscriptionRef.make` + `push = gate.withPermits(1)(Effect.sync{ sendData; run; splice } → Ref.update(state, reduce _patch))`. Locations: `_memory` l.234-265, `_ordered` l.267-302, `_joined` l.342-385, `_grouped` l.387-417, `_closure` l.419-470, `_versioned` l.486-565. `_memory`/`_joined`/`_grouped` are ~90% identical. Doctrine: COLLAPSE_SCAN [25] (recurring combinator cluster → one parameterized transformer) and DEEP_SURFACES. Collapse: one interior `_engine({ build:(input)=>pipeline, drain, publish })` owner owning the graph construction, the permit, the `pending` splice, and the SubscriptionRef publish; each lane supplies only its distinct pipeline stages, its patch/reduce, and (versioned/closure) the AsOf/frontier handling. Removes ~120-150 LOC of scaffold and makes "a new dataflow verb is a handle row" literally one pipeline function.
- **F3 — erased `Record<string, unknown>` in the grouped verb (deepest type erasure in the folder).** `Replay.Grouped` state is `Fold.Table<string, Readonly<Record<string, unknown>>>` (l.161-164); `grouped` takes `by: (op)=>Readonly<Record<string,unknown>>` and `aggs: Readonly<Record<string, Agg<Op>>>` and returns the same erased record (l.192-196, impl l.387-417). A consumer gets zero type info on group-key shape or aggregate result. Doctrine `[05]-[IMPLEMENTATION_CONSTRAINTS]` (no erased types where the language can express the domain) + `SHAPE_BUDGET`. Repair: parameterize `grouped<By extends Record<string,unknown>, Aggs extends Record<string,Agg<Op>>>` and derive the result row as a mapped type `{ [K in keyof Aggs]: number }` (every `Agg` resolves to a numeric column), yielding `Fold.Table<By, {readonly [K in keyof Aggs]: number}>`. The engine call stays cast-bounded but the public surface types the rollup.
- Minor: `Fold.run` discriminates with `Array.isArray(ops)` (l.88); surfaces-and-dispatch warns raw `Array.isArray` narrows to mutable `Array`. Functionally correct here (Stream is not an array) and no readonly tuple is at stake — this is the only guard available for `ReadonlyArray | Stream`, so acceptable, but note the streams.md `digest` twin uses `Chunk.isChunk` for the analogous split. Low.
- Note: `_reduced` (l.209) returns mutable `Array<[S,number]>` — required by the d2 `reduce` ABI (engine interop), pure construction, acceptable.

### state/causal.md — clean
- `Vector` owner with four-way `compare`, `join`/`meet` as `Merge` lattice instances, STM `Tracker`. `_pointwise` (l.60) double-`HashMap.reduce` key-union-then-combine is dense but correct. `_drain` (l.163) fixpoint to data depth (card-bounded by held census).
- No actionable finding.

### state/commit.md — clean
- `Commit` owner + Merkle summary; `_diverges` (l.55) is a card-declared measured tier-descent kernel (`!` at l.56, `let candidates`, nested loops). `_encoded` (l.33) marked `TextEncoder` kernel. Both legitimate exemptions.
- Minor: `_encoded` allocates `new TextEncoder()` per call (l.34) — a hoistable module const inside the kernel; trivial. Low.

### state/machine.md — F6
- **F6 — per-boot recompile of the machine and request classes.** `_compiled` (l.100-129) constructs the `Feed`/`Poll` `TaggedRequest` classes and calls `Machine.makeSerializable` **inside a function invoked once per `_boot` (l.140) and once per `_restore` (l.150)**. Booting-then-restoring one spec compiles the machine twice with fresh request-class identities; each call allocates the full machine definition. Doctrine `INLINE_COMPOSITION`/one-owner: the compilation is a definition fact of the spec, not of each boot. Repair: compile once at `Transition.spec(...)` (or memoize `_compiled` per spec value) and carry the compiled machine + request classes on the returned spec, so `boot`/`restore` only run `Machine.boot`/`Machine.restore`. Also `_surfaced` (l.131-134) types its parameter with `ReturnType<typeof _compiled<P,S,V>>` — a `ReturnType` over an interior function (derivation.md CALL_SITE_RESIDUE smell); disappears once `_compiled` returns a named owner shape. Medium-low.

### state/presence.md — clean (F2 reference model)
- `Presence.state` composes `Merge.struct` (l.91) — the correct merge-table form. `_lifted` via `Match.tagsExhaustive` (l.98). This is the consistency baseline `evidence.Availability` breaks from.
- No actionable finding.

### state/evidence.md — F2 (consequence site)
- **F2 (surfaces here):** `Availability.worst` hand-rolls `Semigroup.make((self,that) => new Availability({...}))` (l.334-353) with an inline `HashMap.reduce` worst-wins over `commands`, while its siblings `Progress._state` (l.173) and `presence._state` compose `Merge.struct`. The only reason it cannot use `Merge.struct` is the missing `Merge.hashMap` instance (F2 root in merge.md). With that instance: `Availability.worst = Merge.instance/struct({ level: Merge.lattice(bounded).join, commands: Merge.hashMap(_worstVerdict), since: Merge.max(Hlc.Order), tenant: Merge.first(TenantContext-eq) })` — collapses the hand-rolled combine, restores sibling consistency, and the convergence proof rides `Converge` like every other instance instead of an ad-hoc `posture`/`alike` literal. Medium.
- `Progress._weights` (l.205) is native recursion over a parent hierarchy — card-declared bounded snapshot fold (live rollup routed to `fold` DATAFLOW closure); acceptable, though a cyclic parent chain would not terminate (the hierarchy is contractually a tree). Low.

### state/feed.md — minor
- `Feed.merge.alike` (l.199-207) compares two feeds by `Chunk.zip` of sorted-map values under `Equal.equals` positionally — pairs by rank, not by key; correct only because each entry's key derives from the entry (equal values ⇒ equal keys). Fragile if the invariant ever loosens; a key-inclusive comparison (like `causal`/`merge` `_tables`) would be safer. Low.
- `Data.tuple` composite keys and single-head eviction fold — correct.

### interchange/format.md — minor
- `_lifted` (l.27) is the shared defect→`ParseError` fold; decode-only arms fail `ParseResult.Forbidden` on encode — good. `declare module "cbor-x"` (l.175) co-located quirk capture — boundaries.md sanctioned.
- Minor: `Proto.stream` returns `Stream<Message, unknown>` and `Pack.stream` returns `Stream<unknown, unknown>` (error channels erased to `unknown`); this is the raw engine boundary re-spelled into `WireFault` by `Wire.diverted`/`_diverted`, so the erasure never reaches domain flow. Acceptable, noted for completeness. Low.

### interchange/frame.md — clean
- `_gathered` (l.51) is an 8-branch keyed Mealy step via nested ternaries — the branches are numeric guards (ordinal/byte-ceiling), not tag dispatch, so ternary is the correct form (Match owns tag/structural, not numeric threshold). `_joined` (l.106) marked kernel. `reassembled` composes `Stream.take(budget) → mapAccum → filterMap → mapEffect(concurrency:1)` correctly.
- `GeometryFrame.view` (l.203) constructs typed-array windows off `_views[dtype]` vocabulary — dtype-driven, card-declared platform seam. Note: `view` sizes by `shape` product and ignores `byteStride` for interleaved buffers (domain concern, not doctrine). Low/none.

### interchange/contract.md — minor
- `_leaf` (l.125) uses `Match.discriminatorsExhaustive("fieldKind")` (good) but the `list`/`map` arms hand-write nested `arm.listKind === ... ? ... :` ternary ladders (l.131-143) over closed sub-discriminants. A nested `Match.discriminatorsExhaustive("listKind")`/`("mapKind")` would break loudly on a new sub-kind; 3-arm ternary is doctrine-tolerable for structural sub-dispatch, so low. `ContractDrift`/`DescriptorGate` are strong (verdict receipt is Schema-declared, gate is a boot Service folding one verdict per census family).
- Low.

### interchange/codec.md — F4, minor tag-attach note
- **F4 — 30 loose landing-class exports (largest DEEP_SURFACES strain in the folder).** The final export block (l.1128-1133) surfaces ~30 landing classes (`RenderReceipt`, `BcfTopic`, `BcfViewpoint`, `BimModel`, `BimDiff`, `IdsAudit`, `Material`, `PbrGroups`, `AppearanceSummary`, `Credential`, `Claim`, `HostFingerprint`, `SnapshotHeader`, `ElementGraph`, `ControlIntent`, `LayoutProgram`, `FlagVerdict`, `BindingStatus`, `CoercedValue`, `WriteReceipt`, `GeoFeature`, `FaultDetail`, `Hops`, `CrdtOp`, `OpLog`, `Gap`, `Parity`, `Quarantine`, `WireFault`, `Wire`, `WkbParser`, `feed`). Doctrine README `DEEP_SURFACES`/language.md `DEEP_MODULE_SITE` prescribe one owner plus at most one operation family. These are genuine independent decode owners consumed directly by later waves, so this is a real tension rather than a clean defect — but it is the folder's single largest deviation from the two-export ideal. Decision to record: either (a) accept the landing classes as a deliberate co-located owner family (the registry census demands them all in one module), or (b) attach them as decoded-owner statics on `Wire` (`Wire.BcfTopic`, `Wire.Credential`, …) so the module surfaces `Wire` + `WireFault` + `Quarantine` + `Parity` + the mechanics, matching the deep-module law. Recommend (a) with an explicit charter note, since the classes are legitimately registry-independent; flag if the branch wants strict two-export conformance. Medium-low.
- Minor: `_stamp` (l.428) hand-transforms an untagged wire by spreading `{ ...raw, _tag: tag }`, used for the class landings (`FaultDetail.FromWire`, `BindingStatus.FromWire`, …), while `ControlIntent`'s members use the doctrine-sanctioned `Schema.attachPropertySignature("_tag", …)` (l.528-533). Two spellings for one concept (discriminant-attach), justified because `attachPropertySignature` targets `Struct` and cannot prepend to a `Schema.Class`/`TaggedClass` input — but worth a one-line card note so a future agent does not read it as drift. Low.
- Strong elsewhere: `Wire` registry (one landing anchor `_landingRows` → derived `_Landing` → guard-tied `_schemas`), `Quarantine` budgeted poison replay, `Parity` verify family, `Gap` sequence Mealy, `feed` transition combinator — all model owners. The `_schemas` table (l.862-899) is 36 arm-dispatchable rows that could in principle derive from `_landingRows` + `_census.arm`, but the explicit governed-value table is derivation.md-sanctioned (GENERATION_FORMS governed value under a stated annotation) and a derivation would reintroduce the reverse-map cast — leave as is.

### interchange/invoke.md — F5
- **F5 — `methodKind` ternary chain instead of exhaustive Match.** `Capability.bind` (l.225-258) dispatches the closed protobuf-es `methodKind` union with `method.methodKind === "unary" ? … : method.methodKind === "server_streaming" ? … : Effect.fail(…)`. The catch-all silently absorbs both `client_streaming` and `bidi_streaming`; a future 5th kind falls through to the fail arm. surfaces-and-dispatch.md `[07]` (`Match.discriminatorsExhaustive("<field>")` for a foreign-field discriminant) is the sanctioned form and would break loudly on an unhandled kind. The fail arm is safe, so this is a robustness/doctrine-conformance finding, not a correctness bug. Repair: `Match.value(method).pipe(Match.discriminatorsExhaustive("methodKind")({ unary, server_streaming, client_streaming, bidi_streaming }))` with the two unsupported kinds emitting the `drift` `WireFault`. Medium-low.
- `_slots`/`Object.fromEntries` cast cluster (l.213, l.259) is card-declared exemption. `Sdk<T>` mapped derivation, `Dial` ExecutionPlan ladder, `Transport.fault` total fold — all strong.

### observe/convention.md — clean
- Flat `as const` semconv/incubating/rasm/metric/event anchors with literal-preserving types; `Convention.identity` conditional-spread tenant (l.163-171) matches language.md exact-optional construction; guard pair on the hub. Model vocabulary owner.
- No actionable finding.

### observe/slo.md — clean
- `Sli` process-local `Data.taggedEnum`; `_BURN`/`_severity` closed tables with guard pairs; `_evaluate` via `Record.map` + `Option.zipWith`; `Alert.of` via `Struct.keys(Slo.rows)`. Exemplary algebra-as-data.
- No actionable finding.

### observe/board.md — clean
- `Query` closed recursive family + one `render` fold (the sanctioned single dialect fold — string-building IS the codegen output). `DashboardModel` identity-derived owner, `_PACKS` mapped handler record with generic indexed dispatch, `laid` `mapAccum` grid fold. `_render`/`_selector` PromQL templating is the closed-family renderer, not fragile stringy logic.
- No actionable finding.

---

## [2]-[FOLDER_VERDICT] — deepest structural weaknesses, ranked by leverage

1. **F1 — fold.md engine-lane scaffold duplication (highest LOC-through-collapse leverage).** Six D2 lanes repeat the graph+input+`pending`+permit+`splice`+SubscriptionRef-publish scaffold; `_memory`/`_joined`/`_grouped` are near-identical. One parameterized `_engine` owner collapses ~120-150 LOC and makes a new dataflow verb one pipeline function. COLLAPSE_SCAN [25] + DEEP_SURFACES.

2. **F2 — missing `Merge.hashMap` roster instance → Availability breaks sibling consistency.** The merge roster covers `Record` but not `HashMap`, contradicting the branch's HashMap-first keyed-state law; `evidence.Availability.worst` is the one merge in `state` forced to hand-roll `Semigroup.make` instead of `Merge.struct`. Adding the instance closes a roster gap, fixes the inconsistency, and hands every future HashMap-CRDT a lawful, `Converge`-provable merge. `COMPOSED_IMPLEMENTATION` (extend the law, then the feature).

3. **F3 — fold.grouped erased `Record<string, unknown>` (deepest type erasure).** The one surface in the folder that hands a consumer an untyped result; parameterize the by-shape and derive the aggregate row as a mapped type. `SHAPE_BUDGET` / no-erased-types.

4. **F4 — codec.ts 30 loose landing-class exports.** The largest deviation from the deep-module one-to-two-export ideal; a co-location decision to ratify (charter note) or collapse (`Wire.*` statics).

5. **F5 — invoke `methodKind` ternary vs exhaustive Match.** Loses the loud-break guarantee on a new method kind; swap to `Match.discriminatorsExhaustive`.

6. **F6 — machine `_compiled` per-boot recompile.** Compile the machine + request classes once per spec, not per boot/restore.

No missing machine (machine.md is research-grade `@effect/experimental` Machine), no naive SQL (core owns no SQL; board renders PromQL via a closed dialect fold), no missing resilience where egress exists (`Budget` schedules, `Dial` ExecutionPlan+retry+timeout, `Quarantine` budgeted replay all present), no phantoms in the spot-checked external surface. Coverage of `values.md` collection owners, `derivation.md` vocabulary/guard forms, `shapes.md` owner forms, `rails-and-effects.md` fault architecture, `concurrency.md` STM cells, and `streams.md` Mealy/window/dedup forms is thorough. The folder is materially at the bar; the six findings are the remaining collapse, consistency, and erasure repairs.
