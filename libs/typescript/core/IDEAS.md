# [TS_CORE_IDEAS]

Forward pool of higher-order folder concepts grounded in the branch-law domain and the monorepo purpose. `[1]-[OPEN]` carries the active ideas as cards; each card names the capability, what it unlocks, and the gap or technique it draws on. `[2]-[CLOSED]` carries the finished or dropped ideas with a one-line disposition so the same idea is never re-litigated. An idea drives one or more `TASKLOG.md` tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[C3]-[QUEUED]: Branch hook-rail vocabulary lands as the fourth observe owner.
- Capability: typed hook-point registry vocabulary — the `rasm.<pkg>.<domain>.<point>` point-name brand, the closed modality vocabulary (veto, observe, replay), the typed subscription contract, subscriber-fault isolation onto `FaultClass`, and the telemetry-as-tap law making every signal emitter a subscriber of domain facts instead of an emit call inside domain code.
- Shape: new page `libs/typescript/core/.planning/observe/tap.md` — point brand, registry shape, modality dispatch table, tap law; runtime executes dispatch and core defines every shape it runs.
- Unlocks: any two apps compose hook rails without collision because point names scope through `AppIdentity`; observability subscribes branch-wide with zero scattered emit sites; `observe/` reaches four owners and exits the stub band.
- Anchors: `state/machine.md` macrostep fact stream ("the inspection hook a consumer taps"); `interchange/invoke.md` `SupportIntake` capture; `interchange/codec.md` quarantine divert; `value/fault.md` `FaultClass` dominance; `value/identity.md` `AppIdentity` scope key.
- Tension: veto is the one modality that feeds a verdict back into the emitting fold — the contract must type it as a pure decision so the tap law never re-opens the zero-exporter boundary.

[C4]-[QUEUED]: One W3C propagation-carrier codec spans every transport header dialect.
- Capability: typed `traceparent`/`tracestate`/`baggage` context value with total parse/print folds, the `rasm.tenant` baggage promotion law, and a closed per-transport dialect row table — Connect ASCII metadata, Connect `-bin` protobuf metadata through `encodeBinaryHeader`/`decodeBinaryHeader`, NATS headers, MQTT v5 UserProperties, CloudEvents distributed-tracing extension attributes, Kafka record headers.
- Shape: new page `libs/typescript/core/.planning/interchange/carrier.md`; the `interchange/invoke.md` interceptor row composes it for the Connect lanes.
- Unlocks: every runtime transport client injects and extracts context through one codec — no per-transport propagation fork; tenant baggage rides every hop so cost attribution survives broker boundaries.
- Anchors: `@connectrpc/connect` `encodeBinaryHeader`/`decodeBinaryHeader` and `appendHeaders` (`.api/connectrpc-connect.md`); `interchange/invoke.md` interceptor seam; `value/identity.md` `TenantContext`; the estate wire law's W3C composite `{TraceContext, Baggage}` ruling.
- Tension: dialect rows for brokers the branch has not admitted (`mqtt`, `cloudevents`) state header shape as spec fact and assume the admission lane lands the clients.

[C5]-[QUEUED]: Profile signal vocabulary with board flamegraph consumers.
- Capability: span-profile correlation vocabulary — profile-link attributes keyed by the one identity projection, `Convention` rows for the profile signal, a profile datasource arm on the board dialect, and a `profile` pack whose flamegraph and span-linked panels ride the same pack dispatch every metric pack rides.
- Shape: `_rasm` profile rows in `libs/typescript/core/.planning/observe/convention.md`; dialect arm and `profile` pack in `libs/typescript/core/.planning/observe/board.md`.
- Unlocks: runtime pyroscope wiring correlates profiles to spans through core vocabulary alone; iac compiles flamegraph panels from the same pack law; the estate profiling axis gains its missing vocabulary tier.
- Anchors: `@pyroscope/nodejs` catalog (`libs/typescript/runtime/.api/pyroscope-nodejs.md`); `observe/board.md` `_DIALECT` export-label pair and pack dispatch; `observe/convention.md` identity projection; branch `effect` `Effect.makeSpanScoped` scoped spans as correlation anchors (`libs/typescript/.api/effect.md`).
- Tension: the board query algebra is metric-shaped — the profile arm is a new dialect row beside it, never a `Query` expression kind.

[C6]-[QUEUED]: Benchmark claims gain a comparison algebra and a bench board pack.
- Capability: typed measurement-statistics band on the landed benchmark claim — percentile ladder, GC counts, hardware counters — and a pure baseline-versus-candidate regression fold yielding graded verdicts per host fingerprint; a `bench` pack trends claims on the boards.
- Shape: claim-landing extension in `libs/typescript/core/.planning/interchange/codec.md#LANDING_WIRE`; verdict algebra and `bench` pack rows in `libs/typescript/core/.planning/observe/board.md`.
- Unlocks: performance regressions become graded verdicts a gate reads instead of eyeballed numbers; TS-lane benchmark runs land claims in the same admitted family the C# corpus gate feeds.
- Anchors: `interchange/codec.md` `BenchmarkClaimWire`/`HostFingerprintWire` landings and the foreign-host-claim rejection; `observe/board.md` pack dispatch; `mitata` statistic shape as the measurement-band model, pending admission.
- Tension: cross-host comparison is refused by the fingerprint admission — the regression fold compares only claims sharing one host print, and the verdict says so.

[C2]-[QUEUED]: Object-plane signal rows complete the Convention vocabulary.
- Capability: `rasm.object.*` metric and instrument rows — dedup-write ratio, bytes written, GC reclaim, resumable-upload throughput — with UCUM units and tag axes drawn from the object receipts.
- Shape: `_metric`/`_instrument`/`_rasm` rows in `libs/typescript/core/.planning/observe/convention.md` under the growth law, and consumer rows in `libs/typescript/core/.planning/observe/board.md` keeping every new instrument pack-projected.
- Unlocks: object-plane health on the estate boards; the data object owners emit through Convention rows in the same instrument-row idiom the journal and read pages carry.
- Anchors: `observe/convention.md` growth law; `observe/board.md` `_PACKS` every-instrument-a-consumer discipline; data `object/store.md` and `object/stream.md` receipt families.
- Ripple: `data` `[OBJECT_PLANE_INSTRUMENT_PROJECTION]`.

[C1]-[QUEUED]: One generative vocabulary-table owner for the value floor.
- Capability: a declaration-time generator deriving the kinds tuple, literal schema, guard pair, positional `Order`, and assembled `Shape` from one row-table declaration.
- Shape: lands in `libs/typescript/core/.planning/value/schema.md` as the declaration-time owner the six re-spelled assembly grammars (`FaultClass`, `Budget`, `Degrade`, `Uncertainty.grades`, `Availability._ROWS`, `WireFault._policy`) become declarations of.
- Unlocks: the next vocabulary is one row-table declaration; the position-to-`Order` projection has one spelling branch-wide.
- Anchors: `fault.md#CLASS_VOCABULARY` assembly grammar; `clock.md` `Uncertainty`; `evidence.md` `Availability`; `codec.md` `WireFault._policy`; the derivation vocabulary-table owner form.
- Tension: the three-altitude fault law keeps the tables distinct — the generator shares machinery without merging vocabularies; the stated-annotation export gate constrains a generic assembled-owner annotation.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
