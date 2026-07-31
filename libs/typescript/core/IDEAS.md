# [TS_CORE_IDEAS]

Forward pool of higher-order folder concepts grounded in the branch-law domain and the monorepo purpose. `[1]-[OPEN]` carries the active ideas as cards; each card names the capability, what it unlocks, and the gap or technique it draws on. `[2]-[CLOSED]` carries the finished or dropped ideas with a one-line disposition so the same idea is never re-litigated. Ideas drive one or more `TASKLOG.md` tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[VALUE_VOCABULARY_TABLE]-[QUEUED]: One generative vocabulary-table owner for the value floor.
- Capability: a declaration-time generator deriving the kinds tuple, literal schema, guard pair, positional `Order`, and assembled `Shape` from one row-table declaration.
- Shape: lands in `libs/typescript/core/.planning/value/schema.md` as the declaration-time owner the six re-spelled assembly grammars (`FaultClass`, `Budget`, `Degrade`, `Uncertainty.grades`, `Availability._ROWS`, `WireFault._policy`) become declarations of.
- Unlocks: the next vocabulary is one row-table declaration; the position-to-`Order` projection has one spelling branch-wide.
- Anchors: `value/fault#CLASS_VOCABULARY` assembly grammar; `clock.md` `Uncertainty`; `evidence.md` `Availability`; `codec.md` `WireFault._policy`; the derivation vocabulary-table owner form.
- Tension: the fault-module collapse ruling keeps the three row families distinct — the generator shares machinery, never merges; the stated-annotation export gate constrains a generic assembled-owner annotation.

[PARITY_CELL_CONSUMER]-[QUEUED]: The field-level parity walk earns its declaration — a content-keyed landing proves its key cells.
- Capability: field-level content-key parity becomes a reachable proof rather than a minted surface, so a landing whose key columns disagree with its own bytes refuses with the field-mask coordinate naming which column moved.
- Shape: a first `Parity.cells` composition at a byte-key-bearing landing in `libs/typescript/core/.planning/interchange/codec.md` `[07]-[KEYED_REGISTRY]`, beside the `verifiedSnapshot` and `admittedGraph` entry compositions the registry already carries.
- Unlocks: the `paths` output and the accumulated `drift` coordinate stop being dead outputs, and a peer that renames a key column surfaces as an addressed refusal instead of a silent mismatch.
- Anchors: `Parity.cells` with its accumulating `Array.partitionMap` roster walk and `_addressed` field-mask spelling; the byte-key columns the landings already carry through `Digest.FromBytes` (`ElementGraph`/`Node`/`Relation`, `BimModel`/`BimDiff`, `RenderReceipt`, `SnapshotHeader`); `@bufbuild/protobuf`'s reflect surface.
- Tension: the walk reads proto `bytes` cells alone, so a landing whose identity columns are hex strings is outside its reach and the first consumer must be a genuinely byte-carried family rather than the widest roster.

[PBR_GROUPS_MAP_FIELDS]-[BLOCKED]: Decoded parameter-group landings bind their baked planes — `PbrGroups` gains the map-address block.
- Capability: the OpenPBR parameter-group landing carries per-map texture addresses — digest, egress leaf, color space, uv transform — beside its scalar blocks, so a viewer material binds baked planes off the one decoded truth instead of scalars alone.
- Shape: map-address fields on `PbrGroups` in `libs/typescript/core/.planning/interchange/codec.md` `[06]-[LANDING_WIRE]`, mirroring the C# projection field-for-field.
- Unlocks: the ui viewer's PBR bind reaches baked planes through the census landing, and the served-asset directory join gains its material-bind consumer.
- Anchors: the `TextureSetWire`/`AssetSetManifest` census landings already carrying `maps[{role, digest, file, colorSpace}]` rows; `libs/.planning/ARCHITECTURE.md` `[07]-[CROSS_LANGUAGE_WIRE]` domain single-producer law and the `csharp:Rasm.Materials/Appearance/interchange` C#-sole-producer law — the TS landing mirrors the projection and never widens it ahead of its producer.
- Arms: the C# producer card `csharp:Rasm.Materials` `[OPENPBR_GROUPS_MAP_COLUMNS]` lands its map columns on `OpenPbrGroupsWire` at `csharp:Rasm.Materials/Appearance/interchange#MATERIAL_WIRE`; the TS landing then mirrors them on the `PbrGroups` msgpack roster (the appearance families carry no proto schema — the census's `Pack` arm is the wire) — an ordering constraint behind an owned card, never an unowned widening.
- Tension: the ui set-bind reaches baked planes through the `TextureSetWire` census landing today and never waits on this card — this card adds only the scalar-group-to-plane join on `PbrGroups` itself.
- Ripple: mirrors `csharp:Rasm.Materials` `[OPENPBR_GROUPS_MAP_COLUMNS]`; `ui` viewer material-bind counterpart follows.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

- [CONVENTION_METER_FACTORY]-[COMPLETE]: instrument rows gained their materialization half — `bounds` rides the distribution rows as a generated ladder, `Convention.mount` derives the live handle from kind, unit, width, and ladder, and `Mounted<N>` types each carrier off the row's own columns, so declaration-to-materialization drift is unrepresentable and the branch's boundary vectors and constructor picks collapsed into the owner.
- [WIRE_TRANSLATION_SELECTOR_AGREEMENT]-[COMPLETE]: resolution landed as a render property rather than a name property — `Convention.translated` projects the store series name from the mint name through the `_translation` strategy roster, `_promUnit` and `_tail` carry the unit word and type tail per code and kind, and `board#QUERY`'s PromQL fold renders every selector through it, so the Tier-0 pin stands unnarrowed and a store row translating differently renders its own names off one query value.
- [OBJECT_PLANE_ROWS]-[COMPLETE]: object-plane rows landed in `observe/convention.md` (`objectWritten`/`objectSize`/`objectReclaimed`/`streamSize` with the `objectOutcome` axis) with the `object` pack as their `observe/board.md` consumer; the data-side `[OBJECT_PLANE_INSTRUMENT_PROJECTION]` counterpart was already closed against these rows.
- [OBSERVE_TAP_OWNER]-[COMPLETE]: `observe/tap.md` landed as the fourth observe owner — `TapPoint` brand, veto/observe/replay modality table, subscription contract, `FaultClass` breach isolation, `Tap.Registry` — with the codemap and runtime seam registered.
- [INTERCHANGE_CARRIER]-[COMPLETE]: `interchange/carrier.md` landed — the typed `traceparent`/`tracestate`/`baggage` value with total folds, `rasm.tenant` promotion, the closed connect/nats/kafka/mqtt/cloudevents dialect table, and the `-bin` typed-metadata lane — composed by `interchange/invoke#DIAL_AXIS`'s `_stamped` fold.
- [PROFILE_SIGNAL_VOCABULARY]-[COMPLETE]: profile signal landed — `Convention.profile`/`Convention.profiled` correlation vocabulary in `observe/convention.md`, the `Flamegraph` panel row with the per-tag datasource arm, and the `profile` pack in `observe/board.md`.
- [CLAIM_MEASUREMENT_BAND]-[COMPLETE]: claim measurement band landed on `interchange/codec#LANDING_WIRE` (`Claim.Band` carrying the sample count beside its measured-rung map) with `observe/board#BENCH`'s `Bench.graded` per-host-print regression fold and the `bench` pack.
