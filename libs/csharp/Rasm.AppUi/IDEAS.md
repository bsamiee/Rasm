# [APPUI_IDEAS]

Forward concept pool for the product UI engine: each open idea is a higher-order folder concept grounded in the AEC/Rhino purpose and current rendering, reality-capture, and coordination research, and each drives one or more `TASKLOG.md` cards.

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

(none)

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[DOCUMENT_SEARCH]-[COMPLETE]: landed as `libs/csharp/Rasm.AppUi/.planning/Document/search.md` — the closed query shape, source-coverage rows over the landed owners (notebook cells, media prose, issue comments, evidence payloads), ranked source-attributed results through the `VirtualWindow` fabric, highlight navigation on the `SearchStrategyFactory.Create` mint, and `SearchFault` on `AppUiFaultBand.Search` (6430) — WITH the Persistence counterpart landed the same pass: `Query/retrieval.md` `[06]-[DOCUMENT_CORPUS]` bm25 index lane and the `DocumentQuery`/`DocumentHit` wire named at both ends.
[SUPPORT_BUNDLE]-[COMPLETE]: blocker fell — AppHost `SupportManifest.Entry` gained the post-redaction, post-cap `ContentKey` (`Option<string>` hex, `None` on every no-byte arm including the bundle-cap drop) across every construction, and the AppUi golden leg pins content identity as the third fact of the `ProofLaw.BundleShape` pair; the pre-redaction `BundleMember.ContentKey` and the archive key agree exactly where nothing was masked or truncated, an inequality naming redaction or a cap rather than corruption.
[RAY_CONE_CURVATURE]-[COMPLETE]: the producer counterpart landed (`Rasm.Compute` `ResidencyMeshlet.Curvature`, measured normal-variation bound on `ResidencyPayload.Clusters`) and the whole consumer chain realized — `ResidencyMeshletView` projects the column, `SurfaceAttributes.Curvature` fills at hit resolve, `RayCone.Advanced(distance, curvature)` widens spread by `2 * curvature * distance` beside the distance term, and `MeshletWire`/`MeshletWireOf` carry the column under the bumped `Schema = 3`; a planar hit reads a measured 0 and widens by distance alone.
[HOST_SPRING_PARITY]-[COMPLETE]: the parity-map table landed in `libs/csharp/Rasm.AppUi/.planning/Theme/motion.md` `[02]-[MOTION_AXIS]` — `SpringValue` members (`Response`, `DampingFraction`, `Shape`) beside the host surface classes mirroring them, a host preset and a shell token evaluating the SAME kernel `SpringShape.OfResponse` closed form; the host preset table itself seats at the composition root per the branch spring-parity ruling, AppUi the parity source with zero surface change.
[COLLAB_SESSION_GOVERNANCE]-[COMPLETE]: landed as `libs/csharp/Rasm.AppUi/.planning/Collab/session.md` — `SessionRole`/`SessionCapability` `[SmartEnum<string>]` vocabulary with delegate-column grants, `MembershipState` total lifecycle rows, `MembershipOp` landing durably as `EditIntent.Membership`, and `SessionGate.Required`/`Admit` the total admission fold the `sync.md` merge authority composes ahead of `LedgerAppend`; rejections derive through the new `AppUiFaultBand.Session` (6530) registry row and presence stays a projection over the landed awareness channel.
[COLLAB_WIRE_CONTEXT]-[COMPLETE]: blocker fell — AppHost landed `Wire/topics` `Topic.Collab`/`Topic.Presence` and the generic `TraceContext` `Inject`/`Extract`/`Continue` spine whose boundary law seats a domain carrier's concrete bodies beside the consuming egress leg; `Collab/sync.md` `CollabCarrier` now binds the frame's getter/setter pair with `TenantAdoption.Adopted`, and the named `[COLLAB_DELTA_FEED]` cluster never existed — the topic rows are the reciprocal.
[MATH_TYPESET_OWNER]-[COMPLETE]: landed as `MathStyle`/`MathBox`/`MathTypeset`/`MathRun`/`MathInlineVisual` on `libs/csharp/Rasm.AppUi/.planning/Document/media.md` `[02]-[MARKDOWN_INLINES]` — one painter serves the measure and the draw, the block and inline arms both materialize through `MathStyle`, math composites into the leased canvas through the settled `ICustomDrawOperation` vehicle, and the typography projection stays typeset-free as ruled.
[PROFILE_FLAME_JOIN]-[COMPLETE]: the AppHost `ProfileCapture.Bind` producer landed at `Observability/benchmarks.md` `[04]-[PROFILE_CORRELATION]`, so `FlameNode.Of` now folds published `ProfileSample` values; the row widened with `ThreadId`, `Kind`, and the `ProfileFrameForm` posture, and `FromSamples` lane-groups by thread with the posture on the lane name.
[TELEMETRY_BOARD_PAGE]-[COMPLETE]: store-profile tiles read the landed `SeriesKind.Telemetry` projection through `StoreProfileTrack`'s `(domain, slot, measure)` facets and one injected read arrow, so every board tile binds a producer.
[MULTI_MEASUREMENT_LEVELS]-[COMPLETE]: keyed level families ride the kernel `InstrumentSpec.Levels` declaration over `LevelCells.Reader`, declared beside each producer and swapped by the `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `[03]-[TELEMETRY_SPINE]` fan arms.
[COLLAB_FORENSICS]-[COMPLETE]: landed as `LiveWire.TapPreCommit`/`ExportJson` with the devloop `CollabPreCommit`/`CollabJson` verbs sealing `PreCommitFact` evidence in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/devloop.md`.
[FRAME_BENCH_LANE]-[COMPLETE]: landed as `BenchLane`/`ProofEngine.Bench` with `ProofLaw.FrameBench`/`Divergence` gating through the AppHost `BenchmarkGate` in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/proof.md`.
[TENANT_USAGE_FOLD]-[COMPLETE]: landed as `TenantUsage`/`TenantUsageFold.Fold` beside `EvidenceJoin` with the `TenantUsageWire` crossing in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `[04]-[CORRELATION_JOIN]`.
[FRAME_SLO_FEED]-[COMPLETE]: landed as the `ViewportObjectives` latency indicators in `libs/csharp/Rasm.AppUi/.planning/Diagnostics/evidence.md` `[03]-[TELEMETRY_SPINE]`, consumed by `SloTiles`/`BurnFeed` over the kernel burn table.
