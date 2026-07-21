# [TS_CORE_IDEAS]

Forward pool of higher-order folder concepts grounded in the branch-law domain and the monorepo purpose. `[1]-[OPEN]` carries the active ideas as cards; each card names the capability, what it unlocks, and the gap or technique it draws on. `[2]-[CLOSED]` carries the finished or dropped ideas with a one-line disposition so the same idea is never re-litigated. An idea drives one or more `TASKLOG.md` tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[VALUE_VOCABULARY_TABLE]-[QUEUED]: One generative vocabulary-table owner for the value floor.
- Capability: a declaration-time generator deriving the kinds tuple, literal schema, guard pair, positional `Order`, and assembled `Shape` from one row-table declaration.
- Shape: lands in `libs/typescript/core/.planning/value/schema.md` as the declaration-time owner the six re-spelled assembly grammars (`FaultClass`, `Budget`, `Degrade`, `Uncertainty.grades`, `Availability._ROWS`, `WireFault._policy`) become declarations of.
- Unlocks: the next vocabulary is one row-table declaration; the position-to-`Order` projection has one spelling branch-wide.
- Anchors: `fault.md#CLASS_VOCABULARY` assembly grammar; `clock.md` `Uncertainty`; `evidence.md` `Availability`; `codec.md` `WireFault._policy`; the derivation vocabulary-table owner form.
- Tension: the fault-module collapse ruling keeps the three row families distinct — the generator shares machinery, never merges; the stated-annotation export gate constrains a generic assembled-owner annotation.

[CONVENTION_METER_FACTORY]-[QUEUED]: Instrument rows materialize through one Convention-owned factory — per-site constructor picks collapse.
- Capability: the instrument table gains its materialization half — one factory derives the live metric handle from a row's kind, unit, and boundary spec, so a consumer names the instrument and receives the handle, and a boundary vector has one spelling branch-wide.
- Shape: the factory member beside the instrument rows in `libs/typescript/core/.planning/observe/convention.md`; consumer collapses follow at the data metering and bracket sites and the interchange invoke bracket.
- Unlocks: a new instrument is one row with zero constructor code at any consumer; declaration-to-materialization drift is unrepresentable.
- Anchors: `convention.md` `_instrument` rows (kind/unit/description); the re-spelled `MetricBoundaries.exponential` vectors at data `read/batch.md` and `lane/olap.md`; the materialization-homing row at `libs/typescript/core/RULINGS.md` `[01]-[SHAPE]`.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

- [OBJECT_PLANE_ROWS]-[COMPLETE]: object-plane rows landed in `observe/convention.md` (`objectWritten`/`objectBytes`/`objectReclaimed`/`streamBytes` with the `objectOutcome` axis) with the `object` pack as their `observe/board.md` consumer; the data-side `[OBJECT_PLANE_INSTRUMENT_PROJECTION]` counterpart was already closed against these rows.
- [OBSERVE_TAP_OWNER]-[COMPLETE]: `observe/tap.md` landed as the fourth observe owner — `TapPoint` brand, veto/observe/replay modality table, subscription contract, `FaultClass` breach isolation, `Tap.Registry` — with the codemap and runtime seam registered.
- [INTERCHANGE_CARRIER]-[COMPLETE]: `interchange/carrier.md` landed — the typed `traceparent`/`tracestate`/`baggage` value with total folds, `rasm.tenant` promotion, the closed connect/nats/kafka/mqtt/cloudevents dialect table, and the `-bin` typed-metadata lane — composed by `invoke#DIAL_AXIS`'s `_stamped` fold.
- [PROFILE_SIGNAL_VOCABULARY]-[COMPLETE]: profile signal landed — `Convention.profile`/`Convention.profiled` correlation vocabulary in `observe/convention.md`, the `Flamegraph` panel row with the per-tag datasource arm, and the `profile` pack in `observe/board.md`.
- [CLAIM_MEASUREMENT_BAND]-[COMPLETE]: claim measurement band landed on `interchange/codec.md#LANDING_WIRE` (`Claim.Band` mirroring the mitata `stats` record) with `observe/board.md#BENCH`'s `Bench.graded` per-host-print regression fold and the `bench` pack.
