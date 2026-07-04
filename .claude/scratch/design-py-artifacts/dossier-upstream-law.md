# DOSSIER — lane: upstream-law

Scope: the four upstream py briefs read as LANDED LAW (`RASM-PY-RUNTIME/DATA/GEOMETRY/COMPUTE-BRIEF.md`), the artifacts `ARCHITECTURE.md` seam ledger, and the `[V16]`/`[06]` contact surface. Method: every assigned register row and every upstream-anchored claim re-proven against disk (artifacts `.planning` + upstream `.planning` where the anchor is cross-corpus). Verdicts: HOLD (anchor exact) / DRIFT (real defect, anchor moved — corrected) / REFUTED (disk falsifies — line cited).

Headline: the upstream contact surface is sound and bidirectionally coherent — every gated obligation and every `[V16]`/`[04]` law anchors to explicit upstream verdict text that names artifacts as the demanding consumer. Three findings the DECISION must act on: (1) the limiter+stamina concurrency-collapse census is undercounted ~10x by BOTH the artifacts brief AND runtime `[V5]`; (2) the outward-figure gated obligation is mis-named "model-asset case" against compute `[V2]`'s "artifacts-origin axis case" authority; (3) two of the three `[06]` gates are ALREADY UNBLOCKED because their upstream campaigns land before artifacts — they must be REALIZED, not kept gated.

---

## 1. E13 TRACK-LAW DEBT RE-COUNT (mechanical, on disk)

Every `[V16]`/E13 count HOLDS EXACTLY. `rg` over `libs/python/artifacts/.planning/`:

| Claim (brief) | Disk | Verdict |
|---|---|---|
| 69 `content_identity` spellings | 69 occurrences | HOLD |
| 45 full `rasm.runtime.content_identity` paths | 45 occurrences | HOLD |
| on 44 pages | 44 pages (any `content_identity`), 44 pages (full path) | HOLD |
| 59 `from builtins import frozendict` sites | 59 occurrences | HOLD |
| on 54 pages | 54 pages | HOLD |
| 54 pages carry `[RESEARCH]` tails | 54 pages (65 header occurrences) | HOLD |

`[V16]` mechanical acceptance (`rg` returns zero) is currently UNMET: 45 / 59 / 65 — the debt stands unstarted, as expected pre-campaign.

Two non-defect data points worth carrying to the DECISION:
- **55 pages mention `frozendict`, only 54 import it.** The 55th is `core/plan.md` — prose-only mentions at `:16` (Packages), `:18` (Boundary), `:465` (RESOLVED note), no `from builtins import frozendict` and no `frozendict[...]` type. `core/plan.md` is already in the `[V16](b)` target deleted-form state (names the anti-pattern without importing it) — the one page needing no import rewrite, only prose confirmation it stays contrast-framed.
- **1143 total `frozendict` occurrences** corpus-wide — the `Final[frozendict[...]]` table declarations the rebind re-types `Final[Map[...]]`. The 59-import figure is the raise-on-import subset; the type-annotation footprint is two orders larger and is the real `[V16](b)` edit surface (every `_SIZES`/`_HATCH`/etc. table).

---

## 2. E13 LIMITER + STAMINA CENSUS — DRIFT (census undercounts ~10x; shared by runtime [V5])

E13 anchors "folder-minted limiters + bare stamina against the runtime lane/retry exports" to FOUR sites: `composition/compose.md:153,159`, `imposition.md:150`, `sheet.md:151`. `[04]` SHARED-TIER LAW repeats "the three folder-minted `CapacityLimiter(4)`s" at three of them. Runtime `[V5]` — the LANDED upstream law — names the IDENTICAL three (`compose.md:153`, `imposition.md:150`, `sheet.md:151`) plus `compose.md:159`.

The four named anchors HOLD exactly:
- `compose.md:153` `_GATE: CapacityLimiter = CapacityLimiter(4)` ✓
- `compose.md:159` `_TRANSIENT = stamina.AsyncRetryingCaller(attempts=3, timeout=30.0).on(BrokenWorkerProcess)` ✓
- `imposition.md:150` / `sheet.md:151` `_GATE: CapacityLimiter = CapacityLimiter(4)` ✓

But the DEBT is corpus-wide — the census is a ~10x undercount:

**CapacityLimiter mint sites: ~38 across ~35 pages** (10 literal `CapacityLimiter(4)`, plus `(8)`, `(os.process_cpu_count() or N)`, `(1)`, `(_SHAPE_SLOTS)`, `(_RENDER_SLOTS)`). Every offloading page mints its own limiter, spanning ALL THREE legs:
- FOUNDATIONS: `graphic/color/derive.md:319`, `managed.md:171,172`, `graphic/marks/encode.md:297`, `graphic/raster/io.md`, `typography/font.md:64`, `typography/layout.md:62`, `typography/shape.md:127`, `core/plan.md:84`.
- MID: `visualization/table.md:149`, `visualization/chart/export.md:66`, `visualization/diagram/draw.md:55`, `layout.md:193`, `scene/render.md:78`, `document/emit.md:219`, `egress.md:170`, `lens.md:158`, `report.md:414`, `tagged.md:310`, `composition/{compose,imposition,sheet}` (the 3 named).
- AEC/EGRESS: `drawing/{annotate:69,detail:62,dimension:74,schedule:86,symbol:58}`, `exchange/{conformance:807,credential:617,detect:586,metadata:674}`, `export/{dxf:108,indesign:405,layered:577,580}`, `delivery/{register:170,transmittal:341}`, `specification/section:454`, `package/codec:49`.

Notably `delivery/register.md:170` is a FOURTH literal `CapacityLimiter(4)` the census does not name — same defect class as the three it does.

**Bare stamina retry: ~12 pages** (15 pages touch `stamina`). Two forms, both the deleted form per `[04]`:
- Worker-death `.on(BrokenWorkerProcess)` → must ride `offload(retry=RetryClass.OCCT)`: `compose.md:159` (named), `detect.md:202`, `encode.md:300`, `raster/io.md:75`, `media/container.md:65`, `layout.md:66`, `shape.md:147`, `chart/export.md:69`.
- Transient `@stamina.retry(...)` → must route through runtime `guarded` over the POLICY table: `conformance.md:601`, `credential.md:529`, `metadata.md:418,442`, `dxf.md:1056`.

**Implication for the DECISION.** `[05]` names "entry realization corpus-wide" as leg 1's reconcile obligation; the limiter+stamina collapse is the SAME class of corpus-wide obligation and must be scoped identically — a ~38-site limiter collapse + ~12-page stamina collapse onto `lanes.offload`/`guarded`, spanning all three legs, closed in leg-1's reconcile with the later legs verifying (mirroring the `ArtifactWork` rewire's leg-1-reconcile / leg-2-3-verify shape). The upstream `resilience.md:254` OCCT target is verified landed on runtime disk: `("occt", Policy(attempts=3, timeout=120.0, target=(anyio.BrokenWorkerInterpreter, anyio.BrokenWorkerProcess), wait_initial=0.5))` — so `offload(retry=RetryClass.OCCT)` is a ruled upstream surface, and the `conformance`/`metadata`/`credential`/`dxf` transient-`@stamina.retry` arms collapse onto the runtime `ORACLE`/`WIRE`/POLICY rows, not a folder retry. The census in a draft that lists only 3-4 sites is a COVERAGE thin slice — a disqualifying naivety per the campaign's own axis.

---

## 3. THE THREE [06] GATED OBLIGATIONS — verified against upstream verdict text

### Gate #1 — measured-signals contribution → runtime `[V5]` — HOLD, and NOW UNBLOCKED

Artifacts `[06]`: production duration / byte-volume / compression-ratio facts enter the one runtime metric stream through `ArtifactReceipt.contribute`, "unblocks on runtime `[V5]`'s table-keyed domain-histogram recorder — artifacts is its named demanding consumer."

Runtime `[V5]` (line 87, verbatim): "`metrics`' artifacts-domain surface (`record_artifact` + the two `artifact.*` histogram rows …) generalizes to a table-keyed domain-histogram recorder over `INSTRUMENTS` rows … `artifacts` is the named demanding consumer: its measured-signals contribution (`RASM-PY-ARTIFACTS-BRIEF.md` carries the concern, gated on exactly this recorder) admits through the generalized rows, never a re-minted stream."

PERFECT bidirectional match. Runtime disk confirms the substrate: `runtime/.planning/observability/metrics.md:9` — "the one `INSTRUMENTS` `Block[InstrumentSpec]` table `install` folds." Runtime is track 1/5, lands BEFORE artifacts (5/5), so the recorder is LANDED LAW. **RULING FOR THE DECISION: REALIZE, not keep gated** — artifacts adds its domain-histogram rows and wires production-duration/byte-volume/compression-ratio facts through `ArtifactReceipt.contribute` over the generalized `INSTRUMENTS` rows. `[EXECUTION]` step 1 already dates this: "the measured-signals contribution re-ruled against the landed runtime `[V5]` recorder."

### Gate #2 — outward figure hand-off → compute `[V1]`/`[V2]` — HOLD on gate, DRIFT on case identity

Artifacts `[06]` + `ARCHITECTURE.md:94`: figures cross "ONLY as the `compute/graduation` `HandoffAxis` **model-asset case** keyed by `ContentIdentity` … unblocks on compute `[V1]`/`[V2]`'s hub — the artifacts-origin axis case ships WITH this folder's producer per the compute `[V2]` extension law."

Compute `[V1]` (line 73) realizes `graduation/handoff.md` as the multi-domain HUB. Compute `[V2]` (line 77, verbatim): "The axis roster is additionally extensible by sibling campaign … `RASM-PY-ARTIFACTS-BRIEF.md` holds an outward figure-handoff crossing gated on the very hub this verdict realizes — standing consumer pressure for an **artifacts-origin axis case**, admitted only when the artifacts campaign ships the case AND its self-wired producer in one motion (the geometry-ripple discipline at axis scale); until then the roster closes at the ruled set, and the named blocker condition is the artifacts producer landing."

The GATE match is clean and bidirectional. But there is a genuine CROSS-BRIEF DRIFT on the case identity:
- Artifacts `[06]`/`ARCH:94` say figures ride the **`model_asset`** case.
- Compute `[V2]` (the upstream authority) requires a NEW **artifacts-origin** case shipped WITH the artifacts producer.

Compute disk falsifies the "model-asset" framing: `compute/.planning/graduation/handoff.md:20` — "Cases: `HandoffAxis` cases `solver`, `symbolic`, `model_asset`, `array_layout`, `unit_law`, `uncertainty_law`, `geometry`, and `convex_program`." `model_asset` is a distinct compute-OWN case (produced by compute's `model` page); there is NO `artifacts-origin` case on disk. `handoff.md:30` states the extension law: "a new handoff kind is one `HandoffAxis` case plus one `_subject` match arm." Compute `[V2]`'s "one self-wired producer per case" law means artifacts figures CANNOT ride `model_asset` (whose producer is compute), or the discipline breaks.

**RULING FOR THE DECISION: REALIZE via a NEW artifacts-origin `HandoffAxis` case** (not `model_asset`), shipped WITH its self-wired artifacts producer in one motion per compute `[V2]`'s geometry-ripple-at-axis-scale discipline — and CORRECT `[06]`/`ARCHITECTURE.md:94`'s "model-asset case" wording. Compute is track 4/5, lands before artifacts, so the hub is landed and the case is admissible now. The geometry precedent is the exact pattern: geometry MINTS `GeometrySubject` + the `GeometryHandoff` carrier (geometry `[V2]`), compute DECODES; artifacts MINTS its origin case + self-wired producer, compute admits it as decoded wire data.

The companion claim — "artifacts sources re-mint no canonical concept so the runtime `evidence` `Structural.drift` query stays clean" (`ARCH:94`) — is consistent with runtime's single-mint law (runtime `[V10]` evidence charter) and holds: artifacts keys via `ContentIdentity.of`, re-minting nothing.

### Gate #3 — content-keyed output elision → C# Persistence — HOLD (correctly stays gated)

Artifacts `[06]`: each producer threads its `(ContentKey, Work)` pair into runtime lane admission so outputs short-circuit on a cache hit — "stays gated on the C# Persistence reuse fabric."

This gates on C# Persistence (`RASM-CS-PERSISTENCE`), NOT a py-track upstream, so it correctly STAYS gated. The runtime-side seam is real and landed: `ARCH:216` `core/plan ← python:runtime/execution [KEYED]: Keyed (ContentKey, Work) session-lane elision` and `ARCH:109` `core/receipt ← runtime/execution [RECEIPT]: reuse-fabric elision ContentKey hit/miss`; runtime `[V5]` names "the keyed session-lane elision seam" with artifacts' `core/plan` the consumer, and the runtime tail (leg law, line 28) confirms "the lanes leg confirms the keyed session-lane elision seam to artifacts' pipeline." The DURABLE cache hit/miss is C#-owned. **RULING FOR THE DECISION: KEEP GATED, blocker named (C# Persistence reuse fabric).** The DECISION rules this explicitly per `[06]`'s "never silently."

**Net gate ruling:** two of three are now UNBLOCKED (runtime + compute land before artifacts) → REALIZE #1 and #2; keep #3 gated. A DECISION that leaves #1 or #2 gated, or that realizes #2 via `model_asset`, is defective.

---

## 4. [V16] TRACK-REBIND LAWS — anchored to upstream verdicts (all HOLD)

- **(a) `content_identity` → `identity`** ← runtime `[V4]` (line 83, verbatim): "ruled default `rasm.runtime.identity` … the fences' `content_identity` spelling is the drift." Artifacts inherits verbatim; counts (69/45/44) HOLD. HOLD.
- **(b) frozendict-kill → `expression.Map` (`Map.of_seq`)** ← the "runtime/data/compute campaigns already forced" claim. Precedent is data-E4 / data `[V2]` (line 78, verbatim acceptance): "`rg 'from builtins import frozendict' … returns zero … deleted-form prose names the anti-pattern by the `Map`-vs-frozendict contrast, never the importable statement." Compute `[V5]` (line 89) and runtime `[GENERATOR_LAW]`/`[V8]` (plain-dict tables → `Map` rail) corroborate. Artifacts `[V16](b)` reproduces the exact deleted-form-prose rule. E13's "the data-E4 defect at artifacts scale" (59/54 vs data's 13/10) is precise. HOLD.
- **(c) `[RESEARCH]` purge (purge-and-fold)** ← runtime `[V11]` + `[SEAM_AND_PROSE_LAW]` (line 55, "the `[RESEARCH]` appendix is BANNED"), data `[V11]` (line 114, "purge-and-fold, never delete-blind"), geometry `[V10]`, compute `[V11]`. Artifacts `[V16](c)` inherits the purge-and-fold ruling verbatim (load-bearing member confirmations fold to `Packages`/`.api`; version/freshness narration deletes; unsettled → in-body gated obligations). HOLD.

---

## 5. [04] SHARED-TIER LAW UPSTREAM ANCHORS (all HOLD)

- **`lanes.offload` isolation-modality axis + `offload(retry=RetryClass.OCCT)`** ← runtime `[V5]` (names the identical three CapacityLimiter sites + `compose.md:159` stamina + OCCT + `resilience.md:254`). Verified on runtime disk (§2). Cross-cited identically by compute `[V4]` and geometry `[V3]`. HOLD (with the §2 census-scope DRIFT).
- **retry via runtime `guarded` over one POLICY table; the `ORACLE` row** ← runtime `[V5]` (verbatim): "the POLICY table gains an `ORACLE` row (import-free predicate over flaky external-oracle faults — veraPDF/JHOVE subprocess verdicts) with artifacts `exchange/conformance` the named demanding consumer." Disk shows the unconverted form at `conformance.md:601` (`@stamina.retry`). HOLD.
- **`ContentIdentity.of` over canonical bytes under `CANONICAL_POLICY`** ← runtime `[V7]` (line 95, verbatim): "the identity surface exports `CANONICAL_POLICY` — the one default the `of` `policy` parameter binds — and key equality is bytes-law." Data `[V3]`/compute `[V7]` bind the same default. HOLD.
- **receipts via runtime `ReceiptContributor` structural conformance (the geometry-ruled no-subclass discipline)** ← geometry `[V5]` (line 91, verbatim): "ruled default STRUCTURAL-ONLY: the Protocol is runtime-checkable, the scan trio already conforms without importing it, and a subclass adds nothing." Data `[SEAM_AND_RAIL_LAW]` corroborates (contribute through `ReceiptContributor`, no second rail). Disk: `ReceiptContributor` on 13 artifacts pages, 36 occurrences — consistent with `[04]`'s "the 32 standing consume sites hold." HOLD.

---

## 6. E7 / E10 ARCH-SIDE ROWS (on disk)

E7 (unwired seams):
- **ARCH:122 dimension←vector** — HOLD. Line 122 is `drawing/dimension ← graphic/vector [VECTOR]: VectorOp.Outline / outline() … the landed outline the refinement`. No live fence composes it: `dimension.md:746` `[TERMINATOR_SELF_CONTAINED] [RESOLVED]` records "the prior `_terminator_layer` called `VectorOp.Outline(...)` and `Vector.over(ops)._worked(ops)` — BOTH phantoms. `graphic/vector#VECTOR` exposes NO `VectorOp.Outline` case." The "landed outline the refinement" parenthetical is doubly stale (phantom case + comment-only intent at `dimension.md:531,650`). HOLD.
- **ARCH:137 annotate←vector** — HOLD (line 137 `[VECTOR]: skia-pathops stroke-to-outline for the revision-cloud filled band`).
- **ARCH:153/156 emit←shape/layout** — HOLD (line 153 `typography/shape → …/document [DOCUMENT]: PositionedGlyphRun text placement`; line 156 `typography/layout → …/document [LAYOUT]: line-broken paragraph runs`; V3 confirms composed in zero emit arms).
- **ARCH:103-105 derive [DERIVE] edges to visualization/scene/managed** — HOLD (lines 103,104,105 exact; match zero importers per the E2 color-orphan).
- **`annotate.md:49` import, no ledger edge** — HOLD. Line 49 `from artifacts.visualization.chart.spec import Palette, hex_ramp` — the E2 spec-alias palette, unledgered (annotate's ARCH inbound edges 133-140 carry no visualization/chart/spec row).
- **`transmittal.md:58` imports `package.codec`, seam list omits it** — HOLD. Line 58 `from artifacts.package.codec import Bundle, CodecProfile, CompressionAlgo`; ARCH transmittal rows (210-215) list `→ package/archive` but no codec edge.
- **`filtergraph.md:14` derive coupling unledgered** — REFUTED. `filtergraph.md:14` is `av.filter.filters_available` routing (`media_filters()`/`_NATIVE`/`_ROUTE`), NOT a `graphic/color/derive` coupling. `filtergraph.md` has ZERO `from artifacts.graphic.color.derive` / `Colorimetry` imports anywhere; the only "derive" tokens are unrelated (`:194` pixel-format re-derived, `:252` derived substitute, `:316` `COLOR_SUBSTITUTE` av color-grade filter). Disk falsifies the claim. Stale/erroneous register anchor — the DECISION drops it from the unwired-seam census.

E10 (ledger drift):
- **ARCH:122 vs `dimension.md:746`** — HOLD (§ above; ARCH claims outline landed, page records it phantom).
- **ARCH:103-105 vs zero derive importers** — HOLD.
- **`model.md:15` ten-variant vs `:7` eleven-variant + FormulaNode mint** — HOLD. `model.md:7` "the recursive **eleven-variant** `msgspec` tagged-union tree (the `FormulaNode` math carrier …)", `model.md:12` "**eleven** `msgspec.Struct` variants", but `model.md:15` "a `type` alias over the **ten-variant** `Union`" — stale by one (FormulaNode is the eleventh). (Register cites the FormulaNode mint at `:319`; the load-bearing ten-vs-eleven drift at `:15` is exact.)
- **package `codec⇄archive⇄delta` trio unledgered and cyclic** — HOLD (ARCH-side). The full ARCH seams carry only `codec ← runtime` (219) and `delta ← runtime` (220); NO intra-package `codec↔archive↔delta` edges exist — the trio is unledgered as claimed. (The page-level cycle anchors `codec.md:37-38`/`archive.md:92`/`delta.md:30` are page-lane, not re-checked here; the ARCH-side "unledgered" holds.)

---

## 7. ENERGY DATA FLOW + CROSS-PLANE CONSUMER PRESSURE (bidirectionally pinned — all HOLD)

The task's "energy data flow is real consumer pressure" is grounded in explicit reciprocal upstream text:
- **geometry `[TELOS]` (line 28, verbatim):** "construction-verification and energy evidence feed artifacts reports (the result frames crossing the data seam self-describing per `[V1]`), and the artifacts scene plane's typed mesh ingress decodes this folder's mesh interchange (`RASM-PY-ARTIFACTS-BRIEF.md` `[V9]`/`[V15]` — the massing suite's named upstream)." Names artifacts `[V9]` (typed mesh ingress) AND `[V15]` (massing suite) as the geometry mesh-interchange consumers.
- **geometry `[V1]` (line 75):** `energy/simulate` "SQLite result decode … into typed result frames crossing the data seam — fence-pinned SELF-DESCRIBING … output name, unit, analysis period, room/zone identifier, and content key ride the frame columns, content-keyed Arrow bytes composed at the consumer edge." The self-describing column floor artifacts `[V10]`/`[V13]`/`[V15]` consumes.
- **V15 Sunpath boundary — PERFECT reciprocal pin.** geometry `[V1]` (verbatim): "`Sunpath` solar geometry — the energy plane's solar owner only: artifacts' diagram sun-path furniture stays its own closed-form kernel per `RASM-PY-ARTIFACTS-BRIEF.md` `[V15]`, so `Sunpath` gains no diagram consumer." Exact reciprocal of artifacts `[V15]` ("geometry `energy/climate`'s `Sunpath` … stays OUT of the diagram path"). Ladybug AGPL posture verified: geometry `[TELOS]`/`E1`/`[PYPROJECT_RECONCILIATION]` — "the out-of-process AGPL Ladybug Tools … band," provisioned through the companion-lane owner behind the process boundary. HOLD.
- **compute `[V10]` render seam — PERFECT reciprocal pin** (line 109, verbatim): "the study/posterior evidence artifacts renders (`RASM-PY-ARTIFACTS-BRIEF.md` `[V10]`, the named demanding consumer) crosses as a self-describing tabular frame … columns carry source, unit, identifier, and content key … through the data `columnar` public fold (the geometry `[V1]` precedent)." HOLD.
- **data `[V4]`/`[V5]` column discipline** — data `[V4]` fixes the self-describing column floor (`source`/`unit`/`declared_unit`/`content_key`); data `[V5]` (line 90, verbatim) names artifacts `[V15]` plan-geometry ingress AND artifacts `visualization/table` QTO/schedule as downstream consumers of the geospatial frames — "consumer exports, never internal conveniences." HOLD.

Artifacts `[V10]`'s "the data `[V4]`/geometry `[V1]`/compute `[V10]` column discipline — source, unit, identifier, content key ride the columns" is exact against all three upstreams. The DECISION must treat visualization/table + chart + diagram + scene as DEMANDING NAMED consumers of these self-describing frames (attribution/dedupe by column, never re-derived) — this is the standing law of the energy/geospatial/study/QTO ingress bands.

---

## 8. INTEGRATION HOMES FOR ORPHAN-LOOKING ADMISSIONS IN THIS LANE

Integration TARGETS, never removals:
- **`pvlib`** (new feed-verified admission, `[04]`) — home: the V15 solar-ephemeris owner (the sun-position/sun-path furniture generator wherever the DECISION sites V15's solar machine). It DISCHARGES the recorded proof burden (NREL SPA azimuth/altitude, sunrise/transit/solstice solvers, numpy-vectorized date sampling — capability the owned closed-form kernel re-derives). `.api/pvlib.md` authored with the verified `solarposition` member set at admission. The owned closed-form kernel STANDS as fallback if the DECISION declines the admission — never a removal. NOTE the boundary: pvlib backs the artifacts DIAGRAM sun-path (closed-form/pvlib), distinct from geometry's ladybug `Sunpath` (energy plane, AGPL companion-lane) — the two solar owners are mutually pinned to stay separate (geometry `[V1]`).
- **`zlib-ng` re-entry** — the data campaign struck it as a dead DATA admission and named the artifacts compression band its natural home (data `[00]`/`[PYPROJECT_RECONCILIATION]`, verbatim: "a future artifacts-side `zlib-ng` admission re-enters through the artifacts campaign"). Home: a `package/`-plane consuming fence beside the live `lz4`/`brotli`/`zstandard` band (`package/codec.md`, `ARCH:87`). The DECISION rules the re-entry: admit beside the codec band with its `.api` stub, OR the strike stands track-wide. Not a removal candidate for artifacts — an admission-pressure decision the DECISION owns.
- The `[PYPROJECT_RECONCILIATION]` standing removal defaults (`iptcinfo3`, `python-xmp-toolkit`, `pyexiv2`, `grandalf`-on-parity) and the dead-marker census rows (`scikit-image`/`vtk`/`usd-core`/`lets-plot`/`pyexiv2`/`PhotoshopAPI`/`PyICU` + `pyvista` via vtk) are OUT of the upstream-law lane (folder-internal roster, not upstream-anchored) — flagged only as the sanctioned removal set the DECISION rules by survival condition, never zero-consumer counting.

---

## 9. CHARTER-AS-IT-SHOULD-BE (the [V16]/[06] contact surface, for the DECISION)

- **`[V16]` rebind:** three mechanical corpus-wide sweeps landing in leg-1's reconcile so later legs inherit clean spellings: (a) `content_identity`→`identity` at 45 full-path + 24 short-form sites / 44 pages; (b) `from builtins import frozendict`→`expression.Map` at 59 import sites / 54 pages PLUS every `Final[frozendict[...]]` table re-typed `Final[Map[...]]` (the ~1143-occurrence annotation footprint), deleted-form prose contrast-framed (`core/plan.md` already exemplifies the target state); (c) `[RESEARCH]` purge-and-fold on 54 pages (65 headers). Acceptance is mechanical (`rg` zero on all three) — currently 45/59/65.
- **Concurrency collapse (leg-1 reconcile, corpus-wide):** ~38 folder-minted `CapacityLimiter` sites collapse onto `lanes.offload` (isolation-modality axis, runtime-owned bounds, THREAD/process band), zero artifacts-minted limiters; ~12-page bare-stamina collapse — worker-death `.on(BrokenWorkerProcess)`→`offload(retry=RetryClass.OCCT)`, transient `@stamina.retry`→runtime `guarded` over POLICY (`ORACLE`/`WIRE` rows). Scope this as the entry-realization's sibling obligation, NOT a 3-4-site patch.
- **`[06]` gated obligations, re-ruled:** (1) measured-signals — REALIZE through runtime `[V5]`'s generalized `INSTRUMENTS` domain-histogram recorder via `ArtifactReceipt.contribute`; (2) outward figure hand-off — REALIZE via a NEW `artifacts-origin` `HandoffAxis` case + self-wired artifacts producer per compute `[V2]` (correct the "model-asset case" mis-naming in `[06]` and `ARCHITECTURE.md:94`); (3) content-keyed output elision — KEEP GATED on the C# Persistence reuse fabric, blocker named. Two blockers landed (runtime, compute both precede artifacts in track order), so #1 and #2 are realize-now, not defer.

pages_read note: 4 upstream briefs (full), artifacts ARCHITECTURE.md (full seams), targeted disk verification across ~25 artifacts pages + runtime `resilience.md`/`metrics.md` + compute `handoff.md`.
