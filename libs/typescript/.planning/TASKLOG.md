# [TYPESCRIPT_BRANCH_TASKLOG]

Branch-level cross-package work — the wiring, guards, and seams no single TS folder owns, distilled from the branch concert; per-folder work stays in the owning folder ledger. `[1]-[OPEN]` holds live tasks and `[2]-[CLOSED]` compacts a finished or dropped task to a one-line disposition.

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

[SIGNAL_PLANE_CLOSURE]-[QUEUED]: Branch signal plane proves end to end — core vocabulary, runtime bridge, and iac compile leg spell one row set.
- Capability: conformance audit over the three legs `[BRANCH_SIGNAL_PLANE]` binds — every folder emit site resolves through the `core/observe/convention.md` wire rows, `runtime/otel` stays the sole `@opentelemetry/*` importer branch-wide, and the `iac/operate/observe.md` compile leg consumes the `BoardPack` census rows under the exact spellings the emitters mint.
- Shape: row-spelling reconciliation across `core/observe/convention.md`, `core/observe/board.md`, `runtime/otel/emit.md`, `runtime/otel/meter.md`, and `iac/operate/observe.md` — drift repairs at the owning leg, with the `dataflow-system.md` `AppIdentity` resource law held at each.
- Anchors: `[BRANCH_SIGNAL_PLANE]`; the carded meter-census and pack-ingest rows it composes.
- Atomic: spelling reconciliation across five settled pages.

[TAP_GRAMMAR_CONFORMANCE]-[QUEUED]: Folder point sets conform to the core tap brand — one grammar across every registry.
- Capability: every folder hook card's point names re-verify against core `observe/tap.md`'s brand grammar once it lands, and folder `ARCHITECTURE.md` seam registries gain the tap seam edge where a registry mounts the runtime engine.
- Shape: conformance verification across the folder hook-card decompositions; seam edges mirrored in the owning `ARCHITECTURE.md` registries with identical `[KIND]` labels.
- Anchors: `[BRANCH_HOOK_RAIL]`; core `observe/tap.md` point brand (carded); runtime dispatch engine (carded).
- Atomic: grammar verification and seam-edge mirrors.

[EXTERNAL_SPAN_CONTINUATION]-[QUEUED]: Foreign trace context continues as external spans at every ingress decode.
- Capability: `Tracer.externalSpan` continuation rows land where a foreign `traceparent` decodes — carrier ingress, webhook intake, EventLog sync — so a foreign parent continues instead of orphaning a new root span.
- Shape: continuation rows named beside the core carrier dialect table and the runtime ingress decodes.
- Anchors: `libs/typescript/.api/effect.md` `Tracer.externalSpan`; core `interchange/carrier.md` dialect table (carded); `[BRANCH_CONTEXT_CARRIAGE]`.
- Atomic: continuation rows at named ingress points.

[VARIANT_FAMILY_CENSUS]-[QUEUED]: Parallel-spelling census pins the first `VariantSchema` adopters.
- Capability: a census of families spelling wire/domain/patch variants by hand, each pinned to its owning page by repo-relative path with the variant keys it derives; the first adoption rows land on the census verdict.
- Shape: census rows riding `[BRANCH_SCHEMA_VARIANTS]`'s adoption law; owners pinned before any derivation lands.
- Anchors: `VariantSchema.*` (`libs/typescript/.api/effect-experimental.md`); data `read/query.md` relation models; core `interchange/codec.md` wire families.
- Atomic: census and first adoption pins.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[OTEL_SUBSTRATE_HOMING]-[COMPLETE]: `@effect/opentelemetry` demoted out of the branch substrate tier as a single-consumer package — branch registry row and branch catalogue removed; the runtime folder registry and `runtime/.api/` own the package and its catalogue.
[PLATFORM_BINDING_ROWS]-[COMPLETE]: branch substrate registry enumerates the `-node`/`-bun`/`-browser` binding rows, matching the branch catalogues and the folder registries.
[SEAM_REGISTRY_MERMAID]-[COMPLETE]: branch `[03]-[SEAMS]` renders as the kinded Mermaid seam registry, folder mirrors spelling the C# endpoint contracts verbatim.
