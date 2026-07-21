# [TS_CORE_TASKLOG]

Open and closed work distilled from `IDEAS.md`. `[1]-[OPEN]` carries task cards whose leader holds a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` — and three to four scoped bullets: the capability or file to build, the external packages to integrate, the integration points and boundaries or wires, and the key considerations. `[2]-[CLOSED]` carries `[COMPLETE]` and `[DROPPED]` items. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[VOCABULARY_TABLE_OWNER]-[QUEUED]: One generative vocabulary-table owner for the value floor.
- Capability: a declaration-time generator deriving the kinds tuple, literal schema, guard pair, positional `Order`, and assembled `Shape` from one row-table declaration.
- Shape: lands in `libs/typescript/core/.planning/value/schema.md` as the owner the six re-spelled assembly grammars become declarations of; drives from IDEAS `[VALUE_VOCABULARY_TABLE]`.
- Unlocks: IDEAS.md [VALUE_VOCABULARY_TABLE] — the next value-floor vocabulary is one row-table declaration, the position-to-`Order` projection carrying one spelling branch-wide.
- Anchors: `fault.md#CLASS_VOCABULARY`; the derivation vocabulary-table owner form.
- Tension: the fault-module collapse ruling keeps the three row families distinct, and the stated-annotation export gate constrains a generic assembled-owner annotation.

[BIM_CENSUS_RECONCILE]-[BLOCKED]: Reconcile the interchange BIM/geo census to the post-rebuild C# wire owners.
- Capability: `codec.md#WIRE_CENSUS` rows and landings for the Bim families re-anchored to the wire owners the C# rebuild settled — the IFC interchange wire, the kind-discriminated model diff, the BCF wire — with each row's arm matching its owner's dialect.
- Shape: census rows, `_landingRows`, and the landing classes in `interchange/codec.md`; the seam labels in `ARCHITECTURE.md` follow.
- Unlocks: Bim interchange census decodes each C#-settled wire family — IFC interchange wire, kind-discriminated model diff, BCF wire — exactly once, each arm matching its owner's dialect.
- Anchors: `interchange/codec.md#WIRE_CENSUS` rows and `_landingRows`; `ARCHITECTURE.md` seam labels; `csharp:Rasm.Bim` seam registry, `Exchange`, and `Review` pages holding the frozen wire spellings.
- Tension: the frozen spellings are C#-owned and `csharp:Rasm.Bim`'s seam registry disagrees with its own Exchange and Review pages; core cannot pick unilaterally.
- Ripple: `csharp:Rasm.Bim` seam-registry self-reconciliation.

[COMMAND_GATE_LANDING]-[QUEUED]: `CommandGateWire` closes its guard pair — census, format suite, and landing agree.
- Capability: the codec-homed command-gate family gains its landing line and byte-schema row, or leaves the census and format suite whole — either way the landing guard pair proves the table closed at the declaration.
- Shape: one `_landingRows` line and one `_schemas` row in `libs/typescript/core/.planning/interchange/codec.md`; the `_census` row and the `interchange/format.md` `_names`/`_suite` entries stay or leave with it.
- Unlocks: codec.md's own guard law holds — a codec-homed census row without a landing line is the compile error the page declares, live for this family.
- Anchors: `codec.md` `_census` `CommandGateWire` row (`home: "codec"`, source `Rasm.AppUi/Shell`); `format.md` `_suite` `pb.CommandGateWireSchema`; the `_Landed`/`_LandingKeys` guard pair.
- Atomic: one landing line and one schemas row, or two entry deletions.

[EVIDENCE_TIMELINE_KIND]-[QUEUED]: `EvidenceTimelineWire` reconciles to one kind — census family or process-local composition, spelled identically at every seam.
- Capability: the AppUi evidence-timeline crossing carries one truth: a census-plus-landing pair making it a decoded wire family, or every seam registry re-kinds it to the `ReceiptEnvelopeWire` reuse the feed page states — no `[WIRE]`-labeled family sits outside the closed census.
- Shape: census and landing rows in `libs/typescript/core/.planning/interchange/codec.md`, or seam re-labeling on the AppUi edge in `libs/typescript/core/ARCHITECTURE.md` `[03]-[SEAMS]` and the branch companion-contract list.
- Unlocks: decode-once reads consistently — every `[WIRE]`-kinded seam edge resolves to a census row.
- Anchors: `state/feed.md` `Feed.Timeline` (`ReceiptEnvelope` wire-twin reuse); `codec.md` `_families` (no row); the core and branch seam registries naming the family as `[WIRE]`.
- Atomic: one kind reconciliation across three surfaces.

[CARRIER_OWNED_SPELLINGS]-[QUEUED]: Carrier reads its owned names — the tenancy baggage key and `-bin` family names stop re-spelling their owners.
- Capability: the carrier's tenancy literal reads the observe convention owner and its typed-metadata family names tie to the codec census spellings — or a must-match co-ownership law is stated at both spelling sites, so a rename at either owner cannot silently orphan the twin.
- Shape: import-or-law rows in `libs/typescript/core/.planning/interchange/carrier.md` (`_TENANT`, `_BIN`) against `libs/typescript/core/.planning/observe/convention.md`'s baggage-key row and `codec.md` `_families`.
- Unlocks: one spelling site per owned name across the interchange and observe planes.
- Anchors: `carrier.md` `_TENANT = "rasm.tenant"`; `convention.md` `tenant: "rasm.tenant"` row; `carrier.md` `_BIN` naming `TenantContextWire`/`HlcStampWire` beside the codec census rows; `invoke.md`'s convention import proving the edge is legal.
- Atomic: two spelling-site repairs.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[CONVENTION_WIRE_ROWS]-[COMPLETE]: `observe/convention.md` gained the estate wire law as rows — `Convention.scope`/`Convention.wire`, the work-plane/lane/batch/security metric+instrument rows with descriptions, the dotted-name guard, and the log-signal ruling; `observe/board.md` aligned panel and model fields to the Foundation-SDK builder members.
[BOARD_PACK_CONSUMERS]-[COMPLETE]: `observe/board.md` gained the `work` and `security` packs with the crash fingerprint table, so every declared instrument has a board consumer; `value/fault.md` gained the stable `code.*` frame quartet on `FaultCapture.Forensic` with the `Evidence.frame` admission; the semconv catalog's consumer census reconciled to the `Convention` hub and the `otel/emit`/`otel/vital`/`otel/crash` owners.
[DIALECT_EXPORT_PAIR]-[COMPLETE]: `observe/board.md` gained the `_DIALECT` export-label pair with the invoke fault-reason and vital grade panels — the last instrument rows without board consumers; `ARCHITECTURE.md` seam labels reconciled to the page-owned wire spellings (`EvidenceTimelineWire`, `GeoFeatureWire`) and `Alert.Spec` re-sourced to its `slo` owner; `README.md` registered `@effect/experimental` and the interchange charter gained the capability dial.
[OBJECT_CONVENTION_ROWS]-[COMPLETE]: object rows landed in `observe/convention.md` (`objectWritten`/`objectBytes`/`objectReclaimed`/`streamBytes`, the `bjectOutcome` axis) with `observe/board.md`'s `object` pack as their standing consumer.
[TAP_PAGE]-[COMPLETE]: `observe/tap.md` landed with the point brand, modality table, subscription contract, and breach isolation; `README.md` routes OBSERVE to the hook rail and `ARCHITECTURE.md` carries the `tap.ts` codemap row and the `Tap.Registry` runtime seam.
[TAP_POINT_ANCHORS]-[COMPLETE]: three standing tap points ride `tap.md`'s `_points` rows with publisher anchors at `state/machine.md#ACTOR`, `interchange/invoke.md#COMMAND_GATEWAY`, and `interchange/codec.md#FAULT_RAIL`.
[CARRIER_PAGE]-[COMPLETE]: `interchange/carrier.md` landed — typed triple, total folds, `rasm.tenant` promotion, and the closed connect/nats/kafka/mqtt/cloudevents dialect table — with the codemap row and the `Carrier.Context` runtime seam in `ARCHITECTURE.md`.
[BINARY_HEADER_LANE]-[COMPLETE]: `Carrier.bin` rows land the `-bin` lane at `carrier.md#DIALECT_TABLE` over `encodeBinaryHeader`/`decodeBinaryHeader` (`rasm-tenant-bin`/`rasm-stamp-bin`), consumed by `invoke#DIAL_AXIS`'s interceptor law.
[PROFILE_CONVENTION_ROWS]-[COMPLETE]: `Convention.profile`/`Convention.profiled` landed in `observe/convention.md`; `observe/board.md` gained the `Flamegraph` row with the per-tag datasource arm and the `profile` pack.
[SPAN_LIFETIME_ANCHORS]-[COMPLETE]: `Effect.makeSpanScoped` lifetime anchors landed — `machine/<name>` on the actor boot/restore path (`state/machine.md#ACTOR` `risen`) and `gateway/duplex` on the duplex acquisition (`interchange/invoke.md#COMMAND_GATEWAY`).
[CLAIM_BAND_LANDING]-[COMPLETE]: `Claim.Band` landed on the `interchange/codec.md#LANDING_WIRE` claim landing mirroring the mitata `stats` record; `observe/board.md#BENCH` carries `Bench.graded` with the host-print refusal and the `bench` pack trends the bridged series.
