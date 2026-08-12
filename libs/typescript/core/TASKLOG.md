# [TS_CORE_TASKLOG]

Open and closed work distilled from `IDEAS.md`. `[1]-[OPEN]` carries task cards whose leader holds a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` — and three to four scoped bullets: the capability or file to build, the external packages to integrate, the integration points and boundaries or wires, and the key considerations. `[2]-[CLOSED]` carries `[COMPLETE]` and `[DROPPED]` items. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

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

(none)

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[FLAG_TRACK_PACK]-[COMPLETE]: `Board` gives tracked flag outcomes a standing pack; span-riding flag rows render through wide-event residence by ruling.
[ACTOR_SPAN_KEYS]-[COMPLETE]: `rasm.work.family`/`rasm.work.shard` seat actor identity at the mint's two span seats; the static message seat refutes shard, which rides the lifetime span.
[DIGEST_RAW_BYTE_SLOTS]-[DROPPED]: no live consumer needs content-key bytes, so `Digest.raw` has no owner.
[FAULT_CLASS_CONFORMANCE]-[COMPLETE]: quantity faults derive classification through `Fault.Class.family`.
[VOCABULARY_TABLE_OWNER]-[COMPLETE]: `Shape.vocabulary` drives the value, state, and wire vocabularies.
[BIM_CENSUS_RECONCILE]-[COMPLETE]: `Wire.ModelDiff` is the BIM review landing; phantom BIM and IDS families have no registry seat.
[COMMAND_GATE_LANDING]-[COMPLETE]: `Wire.CommandGate` feeds `Panel`, whose admission covers every command affordance row.
[RENDER_RECEIPT_RECONCILE]-[COMPLETE]: `Wire.EvidenceTimeline` carries pixel identity, and `Probe` compares its canonical hash.
[COMMAND_PAYLOAD_RECONCILE]-[COMPLETE]: `CommandPayloadWire` is the five-arm payload nested by `CommandInvocation`.
[CONTROL_INTENT_RECONCILE]-[COMPLETE]: `Wire.ControlIntent` owns producer widgets; `Panel.Interaction` owns viewer interaction.
[CONTROL_INTENT_WIDENING]-[COMPLETE]: `Wire.ControlIntent` carries the producer's full discriminated widget vocabulary.
[EVIDENCE_TIMELINE_KIND]-[COMPLETE]: `Wire.EvidenceTimeline` decodes AppUi evidence; state receipt folds remain separate.
[CARRIER_OWNED_SPELLINGS]-[COMPLETE]: `Carrier` derives tenant and typed-metadata names from their canonical owners.
[SECURITY_PACK_COMPLETION]-[COMPLETE]: `Board` gives every security instrument a standing dashboard consumer.
[TRACE_RENDER_TARGET]-[COMPLETE]: `Query.Target` stays metric-series only; trace panels query wide-event residence.
[RESIDENCE_KIND_SCALAR]-[COMPLETE]: `Query.Residence` declares value columns per signal kind, and its renderer reads the same declaration.
[MITATA_STATS_DERIVATION]-[COMPLETE]: `Board.Bench` derives its measurement modality from `measure` rather than a phantom export.
[ALTERNATE_REGEX_ESCAPE]-[COMPLETE]: `Board` escapes selector alternation through Effect's `RegExp.escape`.
[LATENCY_TEMPORAL_ADMISSION]-[COMPLETE]: `Reliability.Sli.Latency` admits only temporal distribution instruments.
[METRIC_PLANE_ROSTER]-[COMPLETE]: `Convention` instrument rows declare the dimension fans consumed by metric governors.
[DURATION_SCALE_COLLAPSE]-[COMPLETE]: `Convention` derives temporal scaling from each instrument row's unit.
[INSTRUMENT_MATERIALIZATION]-[COMPLETE]: `Convention.mount` materializes each instrument with no consumer-side constructor choice.
[SELECTOR_SUFFIX_RULE]-[COMPLETE]: `Convention.translated` derives target suffixes, and `Board` renders selectors through it.
[SCOPE_VERSION_COORDINATE]-[COMPLETE]: `Convention.scope` supplies one module, schema URL, and composed version coordinate to every meter.
[VITAL_ATTRIBUTE_VOCABULARY]-[COMPLETE]: `Convention` owns the vital accounting, phase, and subject vocabulary used by runtime evidence.
[VITAL_LEVEL_UNIT_SPLIT]-[COMPLETE]: `Convention` separates vital duration, score, and size levels by UCUM carrier.
[UNIT_CARRIER_ROW]-[COMPLETE]: `Convention` owns the UCUM unit carrier consumed by every metric mint.
[PROFILE_SPAN_JOIN]-[COMPLETE]: `Convention` owns profile correlation; runtime seats trace and span identities on samples.
[CONVENTION_ROW_CULL]-[COMPLETE]: `Convention` retains only attribute and instrument rows with live consumers.
[WIRE_NAMESPACE_PIN]-[COMPLETE]: `Convention.identity(Identity.App)` returns the namespace-pinned `Convention.Resource`.
[CONVENTION_WIRE_ROWS]-[COMPLETE]: `Convention` owns observability scope, wire, instrument, attribute, and log vocabulary.
[BOARD_PACK_CONSUMERS]-[COMPLETE]: `Board` gives every work and security instrument a standing pack consumer.
[DIALECT_EXPORT_PAIR]-[COMPLETE]: `Board` owns export labels and panels; architecture seats `Wire.EvidenceTimeline` and `Reliability.Alert`.
[OBJECT_CONVENTION_ROWS]-[COMPLETE]: `Convention` owns object signals, and `Board` gives them a standing consumer pack.
[TAP_PAGE]-[COMPLETE]: `Tap.Point` and `Tap.Fault` define shared hook grammar for folder-owned registries.
[TAP_POINT_ANCHORS]-[COMPLETE]: state, gateway, and wire-fault publishers use the shared `Tap.Point` grammar.
[CARRIER_PAGE]-[COMPLETE]: `Carrier.Context` owns typed propagation and dialect adapters; runtime owns transport bindings.
[BINARY_HEADER_LANE]-[COMPLETE]: `Carrier` owns typed binary metadata adapters composed by invocation at the Connect boundary.
[PROFILE_CONVENTION_ROWS]-[COMPLETE]: `Convention` owns profile correlation, and `Board` owns profile panels and packs.
[SPAN_LIFETIME_ANCHORS]-[COMPLETE]: actor and gateway acquisitions own their scoped span lifetimes.
[CLAIM_BAND_LANDING]-[COMPLETE]: `Board.Claim` owns measurement bands and grading; `Wire` decodes benchmark claims.
[TEXTURE_SET_CENSUS_LANDING]-[COMPLETE]: `Wire` decodes both texture-set documents through one frozen texture vocabulary.
[ASSET_TRANSFORM_INSTRUMENTS]-[COMPLETE]: `Convention` owns asset-transform signals, and `Board` gives them standing consumers.
[PLANE_LEVELS_CONFORMANCE]-[COMPLETE]: texture landings carry level-ordered references with container-derived lengths.
