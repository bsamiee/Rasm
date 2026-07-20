# [TS_CORE_TASKLOG]

Open and closed work distilled from `IDEAS.md`. `[1]-[OPEN]` carries task cards whose leader holds a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` — and three to four scoped bullets: the capability or file to build, the external packages to integrate, the integration points and boundaries or wires, and the key considerations. `[2]-[CLOSED]` carries `[COMPLETE]` and `[DROPPED]` items. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[0006]-[QUEUED]: Object-plane Convention rows with their board consumers.
- Capability: `rasm.object.*` metric and instrument rows — dedup-write ratio, bytes written, GC reclaim, upload throughput — with UCUM units and receipt-derived tag axes; drives from IDEAS `[C2]`.
- Shape: `_metric`/`_instrument`/`_rasm` rows in `observe/convention.md` under the growth law; consumer rows in `observe/board.md` so every new instrument keeps a pack projection.
- Anchors: data `object/store.md` and `object/stream.md` receipt families; the `journal/fact.md` instrument-row idiom the emitters copy.
- Ripple: `data` `[OBJECT_PLANE_INSTRUMENT_PROJECTION]`.

[0003]-[QUEUED]: One generative vocabulary-table owner for the value floor.
- Capability: a declaration-time generator deriving the kinds tuple, literal schema, guard pair, positional `Order`, and assembled `Shape` from one row-table declaration.
- Shape: lands in `value` as the owner the six re-spelled assembly grammars become declarations of; drives from IDEAS `[C1]`.
- Anchors: `fault.md#CLASS_VOCABULARY`; the derivation vocabulary-table owner form.
- Tension: the three-altitude fault law keeps vocabularies distinct, and the stated-annotation export gate constrains a generic assembled-owner annotation.

[0004]-[BLOCKED]: Reconcile the interchange BIM/geo census to the post-rebuild C# wire owners.
- Capability: `codec.md#WIRE_CENSUS` rows and landings for the Bim families re-anchored to the wire owners the C# rebuild settled — the IFC interchange wire, the kind-discriminated model diff, the BCF wire — with each row's arm matching its owner's dialect.
- Shape: census rows, `_landingRows`, and the landing classes in `interchange/codec.md`; the seam labels in `ARCHITECTURE.md` follow.
- Tension: the frozen spellings are C#-owned and `csharp:Rasm.Bim`'s seam registry disagrees with its own Exchange and Review pages; core cannot pick unilaterally.
- Ripple: `csharp:Rasm.Bim` seam-registry self-reconciliation.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

- [0001]-[COMPLETE]: `observe/convention.md` gained the estate wire law as rows — `Convention.scope`/`Convention.wire`, the work-plane/lane/batch/security metric+instrument rows with descriptions, the dotted-name guard, and the log-signal ruling; `observe/board.md` aligned panel and model fields to the Foundation-SDK builder members.
- [0002]-[COMPLETE]: `observe/board.md` gained the `work` and `security` packs with the crash fingerprint table, so every declared instrument has a board consumer; `value/fault.md` gained the stable `code.*` frame quartet on `FaultCapture.Forensic` with the `Evidence.frame` admission; the semconv catalog's consumer census reconciled to the `Convention` hub and the `otel/emit`/`otel/vital`/`otel/crash` owners.
- [0005]-[COMPLETE]: `observe/board.md` gained the `_DIALECT` export-label pair with the invoke fault-reason and vital grade panels — the last instrument rows without board consumers; `ARCHITECTURE.md` seam labels reconciled to the page-owned wire spellings (`EvidenceTimelineWire`, `GeoFeatureWire`) and `Alert.Spec` re-sourced to its `slo` owner; `README.md` registered `@effect/experimental` and the interchange charter gained the capability dial.
