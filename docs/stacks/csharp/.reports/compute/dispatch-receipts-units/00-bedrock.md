# dispatch-receipts-units — bedrock

## intent admission

- A typed compute intent is the single solve-path entrypoint value: payload reference (tensor view, plane, or
  stream handle by staging class), op identity (the kernel symbol), dtype and length class, unit-bearing
  parameters already admitted to canonical units, and the budget reference.
- Admission validates once — shape, finiteness (composing the settled finite gate under the dtype-row law), unit
  conversion — and the interior is total over admitted intents; any check appearing twice marks the second site
  as a deletion target.
- The discriminant is recoverable from the intent value itself — payload class, length class, dtype — never from
  a parallel mode flag beside it; a boolean fast-flag or execution-hint parameter re-describing what the value
  already encodes is the rejected arity form.
- Intent families collapse by union, not by entrypoint multiplication: singular, batch, and streamed solve
  requests are cases of one closed intent family dispatched by one total fold — the case is the modality, and a
  new modality is one case plus its fold arm.
- Vocabulary segregation is a named law: the solve path and the lifecycle-drain queue carry disjoint bounded
  vocabularies.
- Solve-path names — admit, route, execute, receipt — never appear in lifecycle signatures; lifecycle names —
  quiesce, drain, evict, dispose, fence — never appear in solve signatures.
- The segregation is what keeps eviction from ever blocking inference: the two queues cannot share a method, so
  they cannot accidentally share a lock.
- A surface mixing the vocabularies — a run-or-evict-shaped member — is the structural defect the law
  forecloses; the review check is purely lexical, which is what makes it enforceable.

## substrate-selection fold

- One total fold over a closed substrate-row table routes every admitted intent: scalar reference fold,
  vectorized span kernel, parallel partition, plane kernel, model session, remote.
- The remote row composes the transport substrate — wire law is owned elsewhere; compute sees an opaque executor
  arrow and never a wire type.
- Columns: veto (capability predicate — dtype or constraint mismatch, plane non-contiguity, provider absence,
  claim absence), cost (measured-claim lookup), cap (budget-derived ceiling), fallback (next row).
- The terminal row is the scalar reference and is unvetoable by construction — all-rows-vetoed is unreachable,
  so the fold is total without a failure arm.
- The fold is deterministic data: same intent plus same claims table ⇒ same route, reproducible offline from the
  two values alone.
- Route provenance is evidence, not logging: the chosen row and every vetoed row with its veto reason enter the
  receipt, so any "why did this run slow or there" question resolves from the receipt without reproduction.
- Veto ordering is a cost gradient: cheap static vetoes (dtype, shape class) precede probe vetoes (contiguity
  probe, provider probe) precede claim lookups — the fold rejects at the cheapest distinguishing column, making
  routing cost proportional to how early a row disqualifies, not to table size.
- The CPU budget composes, never duplicates: every cap column derives arithmetically from the one settled
  parallelism budget record.
- This page declares only the derivation — lane share × payload-class weight — and the same derivation feeds
  parallel-partition degree floors and session thread counts, so one budget edit moves every axis coherently.
- A second budget-like record anywhere in compute is the foreclosed defect; the derivation is the only budget
  arithmetic this page owns.
- Budget exhaustion is a queue event, not a drop: an intent over the cap queues on the solve-path lane under the
  settled backpressure law — unreceipted loss is a rail rejection — and the queue-entry receipt carries the cap
  that deferred it.
- The deadline is a cost-column input: rows whose claimed cost exceeds the intent's remaining deadline veto on
  cost, so deadline-aware routing falls through expensive rows to cheaper ones without any deadline-specific
  branch — the deadline simply tightens the fold's admission arithmetic.
- Intent timing rides the settled clock seam — admission stamps, queue-entry stamps, and receipt durations come
  from the one clock the process declares; an ad-hoc stopwatch beside the fold is the foreclosed second clock.
- The remote row's cost column includes serialization plus hop; its veto column includes the payload-size cap;
  degradation from remote follows the same fallback walk as any row — distributed and local compute are one
  table, which is the entire point of the substrate abstraction.
- The decision-tree alternative is the foreclosed shape: N capabilities as nested branches yield 2^N paths each
  needing tests; N rows with four columns yield N testable declarations, and adding a capability is one row plus
  its claims.

## benchmark-gated claims

- The law: no fast path without a matching measured receipt.
- A non-reference substrate row is admissible for an intent only when the claims table holds a measured claim
  whose identity matches (kernel symbol, dtype, length class, substrate row) and whose environment fingerprint
  matches the live environment.
- Absent or mismatched claims demote the row to vetoed-with-reason — performance degrades to
  correct-but-reference, never to exception; the demotion receipt is the re-measurement signal.
- Claim identity composes the equivalence-row identity the tensor lane owns: a route needs both gates —
  correctness proof and speed claim — and the equivalence gate dominates: a fast wrong kernel is poisoned
  regardless of ratio.
- The environment fingerprint is a closed column set: ISA and vector-width class, core-topology class,
  budget-record version, session fingerprint where the row is a model row, native asset hashes where the row
  carries native code.
- Fingerprints are values compared structurally; an environment change flips claims to stale en masse by
  construction, with zero per-claim invalidation code.
- Staleness is epoch algebra, not deletion: the claims table carries an epoch; a fingerprint change bumps the
  live epoch; prior-epoch claims demote but persist — re-measurement confirms or replaces them.
- The demotion-to-confirmation interval is itself a health signal an operational fold renders; a fleet that
  never confirms is permanently slow and permanently silent without it.
- Route flapping is foreclosed by a ratio floor: a non-reference row displaces the incumbent only when its
  measured ratio clears a declared margin — a policy value on the order of 1.2×, never a literal at a call site;
  within the margin the incumbent holds. Hysteresis is a declared column, not tuned behavior.
- Pipeline claims are measured end-to-end, never composed: stage ratios multiply only under independence, and
  cache effects between stages break independence as a rule — a pipeline is its own claim row with its own
  fingerprint, and deriving it from stage claims is the forbidden arithmetic.
- Claims are frozen data shipped with the process; the measurement lane that produces them is proof tooling
  owned by the testing rail and never production code — this page owns only the claim-table shape and the gate
  that consumes it.
- Literal fast-path branches (`if (length > 1024) UseSimd()`) are the foreclosed spelling: every threshold is a
  claim row, so functions differing by a literal collapse into the claim column.

## typed receipts

- Every dispatch exits with a typed receipt: route taken, vetoed rows with reasons, timing, budget consumption,
  cache hit class, payload class, and unit-bearing results with dual evidence.
- Receipt combination — identity element, merge, N-into-one collapse — is settled rail law; this lane owns only
  the compute receipt's field vocabulary and its projections.
- Receipt-as-data-source: a receipt family registers one project-to-row delegate, and receipts enter the same
  change-set engine as any other source.
- Change-set mechanics are owned elsewhere; compute's whole obligation is the delegate and the row schema — one
  declaration makes every receipt family a live data source.
- Operational views — route mix, claim staleness inventory, budget pressure, demotion intervals — are folds over
  receipt rows; consumers render totals and never compute them, so the route-mix number on a panel and the one
  in an export can never disagree.
- Typed algorithm receipts never flatten into a generic ledger: fields carrying route, status, sampling, or
  solver evidence keep their algorithm-specific record shape; the generic-receipt abstraction loses exactly the
  evidence that makes a receipt worth keeping.
- Genericity lives in the projection, not the receipt: the fold that renders operational views consumes typed
  rows through the projection delegate.
- Dual evidence is the receipt's admission mirror: for every normalized input — unit conversion, dtype
  narrowing, downsampling — the receipt carries both the canonical value used and the original evidence pair
  received.
- Egress can answer "what did you receive" exactly; disputes resolve from receipts rather than re-parse or
  reproduction.

## units admission

- Unit families convert exactly once at admission: boundary numerics with physical meaning enter as quantity
  values, convert to the family's canonical unit via `As(unit)` / `ToUnit(unit)`, and the interior computes on
  canonical scalars.
- The quantity structs implement the full generic-math operator surface — `IAdditionOperators`,
  `ISubtractionOperators`, `IMultiplyOperators<Q, double, Q>`, `IDivisionOperators<Q, double, Q>`,
  `IUnaryNegationOperators`, `IComparisonOperators`, `IParsable`, additive identity — so quantity-typed code
  composes with operator-generic folds without adapters.
- Display conversion happens only at egress through the declared display unit; the receipt carries the (original
  value, original unit) pair beside the canonical — dual evidence at the unit boundary.
- Cross-family algebra is operator-derived, never hand-scaled: length × length = area, length × force = torque,
  length ÷ duration = speed exist as typed operators on the quantity structs; time interop is built in (length ÷
  `TimeSpan` yields speed).
- A literal conversion factor in interior code restates what the type owns and is the defect — the
  symbolic-reference law instantiated for physics.
- Dynamic flows ride metadata algebra: `BaseDimensions.Multiply` / `Divide` / `Dimensionless` give
  dimension-vector arithmetic; `QuantityInfo` / `UnitInfo` (per-family static `Info`, the info catalog,
  dimension-indexed lookup) feed diagnostics and receipt projection without reflection.
- Boundary rows are the non-throwing family projected to option and rail shapes: `Quantity.TryFrom(value,
  unit-enum)`, `Quantity.TryParse(type, text)`, `Quantity.TryFromUnitAbbreviation(value, abbreviation)`,
  `UnitConverter.TryConvert(value, fromUnit, toUnit, out converted)`; the throwing forms are vetoed at
  boundaries.
- The value carrier admits both floating and decimal sources losslessly — the to-quantity-value projections
  exist from both `double` and `decimal` — so financial-grade decimal inputs enter the same admission rail as
  measurement doubles without a precision-losing pre-cast.
- String-routed dynamic rows exist for fully foreign descriptors: `Quantity.From(value, quantityName, unitName)`
  and the abbreviation factories resolve quantities when even the family is data; they are boundary-only rows,
  and the interior never dispatches on quantity names.
- Custom conversions are registry rows at boot: `SetConversionFunction<TQuantity>(from, to, fn)` extends the
  converter for domain-specific mappings — capability extension is registration, never a wrapper type over the
  converter.
- Every quantity family exposes `Zero` as its additive identity — quantity folds seed from the family's own
  monoid identity, and a literal zero-construction at fold sites restates what the type declares.
- Abbreviation parsing is culture-sensitive — the format-provider overloads are the deliberate column, not an
  afterthought; a boundary that parses abbreviations without pinning the provider drifts per host locale.
- Quantity aggregation is owned vocabulary: `UnitMath` `Sum` / `Min` / `Max` / `Average` over `(source,
  unitType)` pin the result unit for collection folds; `Clamp` bounds; the operator-generic `Sum` / `Average`
  extensions serve carrier-generic folds.
- These are the analytical fold rows for quantity-bearing series — consumers receive folded quantities, never
  element loops.
- Unit-system projection is a declared row, not arithmetic: `As(UnitSystem)` projects a quantity into a
  base-unit system in one verb where a system-level convention (SI base units) governs an interface.
- Declarative unit policy attaches at definition time: the default-unit, convert-to-unit, and display-as-unit
  attributes mark contract members so unit posture travels with the type — definition-time aspects for physics.
- Process-level unit configuration is boot material: the default setup root (abbreviation cache, converter
  registry, `SetConversionFunction` for custom mappings) is configured once at composition; per-call converter
  construction or registration is the foreclosed form.
- The admitted family table — which quantities, which canonical unit, which display unit — is one frozen
  declaration per bounded context, closed and total.
- Tensor-payload composition: a quantity-bearing tensor carries canonical-unit scalars with the unit row on the
  intent, not per element — per-element quantity structs would multiply payload size for information constant
  across the tensor; egress reattaches the unit by projecting through the intent's unit row.

## perceptual-to-physical projection

- The projection law: a tuning row stores perceptual parameters and derives physical parameters as computed
  members at the type.
- The canonical second-order case: a row stores response time and damping fraction and derives angular frequency
  (2π / response), stiffness ((2π / response)²), and damping coefficient (2 × dampingFraction × 2π / response);
  consumers never see the derived constants as stored state.
- Editing feel is editing two numbers in one row; the four-raw-constants spelling forecloses nothing and invites
  the de-sync class of bugs where stiffness and damping drift out of their derivation.
- The general form: any N-parameter physical system controlled through M < N perceptual knobs gets one row type
  with M stored members and N−M derived members, the derivation being the executable specification.
- Storing a derived member is the rejected form; the moment two stored members must co-vary, the type has hidden
  an invariant it should compute.
- Quantity-typing the row keeps the projection dimension-checked: response as a duration quantity, frequency
  deriving as its inverse — a unit mistake in the derivation becomes a type error, not a runtime oscillation
  artifact.

## divergent — substrate-selection-fold

- The fold absorbs every "should we" decision compute will ever face — parallelize, vectorize, cache, offload,
  batch — as rows with the same four columns; maximal unification means no compute decision lives outside the
  table.
- The proof of shape correctness is the diff of the next capability: one row, its claims, zero consumer edits.
- Advanced interaction — fold × budget × queue: the cap column, the queue admission, and the parallel-degree
  derivation all read one budget record version, and the version is a fingerprint column on claims.
- A budget edit therefore simultaneously re-caps routing, re-floors parallelism, and stales speed claims — one
  edit, system-wide coherence, zero coordination code.
- Failure taxonomy is closed: all-vetoed — unreachable, terminal row; cap exhaustion — queue with receipt; claim
  table empty — cold start, reference routes only, the system is correct on day zero and becomes fast through
  measurement, never the reverse; veto-reason drift — a veto firing for a reason outside its declared column set
  is a table-integrity defect surfaced by the receipt fold, not a silent misroute.
- Rejected forms and what each forecloses: decision-tree routing — forecloses receipt provenance and 2^N-path
  testability; per-call-site routing helpers — fragment the table, two sites disagree about the same intent;
  capability flags on the intent — smuggle the discriminant out of the value; a "fast mode" configuration knob —
  routes are earned by claims, not configured.
- Batching is a fold output, not an intent property: the fold may coalesce queued intents sharing (kernel
  symbol, dtype, substrate row) into one batch execution whose receipt fans back out per intent — admission
  stays singular, amortization stays routed, and the per-intent receipt keeps individual provenance through the
  shared run.

## divergent — benchmark-gated-claims

- The claims table is a contract between three parties stated once: the tensor lane proves correctness per
  route, the measurement lane produces ratios, this fold consumes both — and the kernel symbol is the join key
  across all three, so a renamed kernel breaks the join at compile time rather than serving stale claims.
- Demotion telemetry is a first-class fold: count of routes running demoted × duration of demotion = the
  performance-debt gauge.
- A release that flips many fingerprints shows as a debt spike that decays as re-measurement lands — the gauge
  distinguishes "slow because regressed" from "slow because unproven," which no profiler distinguishes.
- The margin floor interacts with measurement variance: a margin below the measurement's own noise band admits
  flapping through the gate it was meant to stop.
- The floor is therefore declared ≥ the measurement lane's published variance class for that row family — a
  policy-value relation between two tables, checkable as data.
- Length classes are derived, not invented: class boundaries sit at the structural breakpoints the route's own
  mechanics dictate — vector-width multiples, the parallel floor, cache-level footprints — so the class table
  derives from the environment fingerprint rather than accumulating ad-hoc thresholds per claim.
- Claim provenance composes dual evidence: a claim row records the reference it was measured against (reference
  route timing and its payload), so a later dispute about a ratio re-derives from the row itself — the same
  evidence posture receipts carry, applied to the claims table.

## divergent — units-admission

- The dual-evidence law generalizes beyond units to every lossy or normalizing admission compute performs —
  dtype narrowing, downsampling, and unit conversion all record (canonical, original-evidence) pairs in the
  receipt.
- The unified statement: normalization without retained evidence is information destruction at the boundary, and
  the receipt is where evidence lives because the receipt already travels with the result.
- Unit-aware claims are unit-invariant by construction: admission canonicalizes before dispatch, so measured
  ratios never depend on input units — the convert-inside-the-kernel spelling would make claims unit-dependent
  and multiply the claim table by the unit enum, which is the quantitative argument for admission-time
  conversion.
- The perceptual-projection row composes policy-as-values at its richest: the row is simultaneously
  configuration (two stored knobs), specification (the derivation), and admission evidence (quantity-typed
  members) — one declaration serving three concerns that ordinarily fragment into a config file, a comment, and
  a validator.
- Frozen family-table corollary: because admitted unit families are closed per bounded context, the boundary
  parser's accepted-abbreviation set is enumerable at boot — the admission surface publishes its complete
  accepted-input grammar as data, and an unparseable input's rejection names the admitted set rather than
  echoing a parser error.
- Dimension-indexed admission: dimension-vector lookup over the quantity catalog admits dynamically-described
  quantities (a dimension vector from a foreign descriptor resolves to the owning family or refuses) — foreign
  systems integrate through dimensions, not through name matching.
