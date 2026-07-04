# [DOSSIER] api-manifests lane — RASM-CS-GEOMETRY

Lane scope: BOTH `.api` tiers COMPLETE (`libs/csharp/.api/` shared + `libs/csharp/Rasm/.api/` kernel), `Directory.Packages.props`, `libs/csharp/Rasm/Rasm.csproj`. Register rows re-verified on disk: E13, E14, every `[04]-[PACKAGE_PRESSURE]` stub/feed anchor. Feed facts verified via the `nuget` MCP against nuget.org (2026-07-03). Stance: hostile; the register is a pointer, disk + feed are truth.

Disk state note that recolors the whole lane: the shared tier holds **31** files, not the brief's **30**; the kernel tier holds **12** (exact). Six `.api` files carry a `2 hours` mtime (`api-languageext.md`, both `api-mathnet-numerics.md`, both `api-mathnet-providers.md`, `api-rhino.md`, `api-tensors.md` kernel) — a roster-reconciliation edit wave already LANDED between the brief's authoring and this survey. Several E13/`[04]` catalog obligations are therefore already discharged on disk; the register describes a pre-edit corpus. Verdicts below reflect current disk, not the register's snapshot.

---

## [A] REGISTER VERDICTS — E13 / E14

### E13 — Catalog debt (four sub-claims, verdict SPLIT)

| Sub-claim | Verdict | Disk evidence |
|---|---|---|
| shared `api-hashing.md:82` `XxHash3`/`ulong` falsehood vs kernel-overlay truth | **HOLD** | Shared `libs/csharp/.api/api-hashing.md:82` `[STACK]` bullet: "The identity rail composes `XxHash3.HashToUInt64` as the canonical `ContentHash` seed … the `ulong` value flows into the `SnapshotId`/`ContentHash`". Kernel `libs/csharp/Rasm/.api/api-hashing.md:82`: "composes `XxHash128.HashToUInt128` at SEED ZERO as the kernel's one `ContentHash.Of(ReadOnlySpan<byte>) → UInt128` entry". Anchor exact in both files; the shared catalog is a live falsehood against the landed `Domain/identity.md` federation entry (`ARCHITECTURE.md:76-81`). Compounded: the shared file's own `[RAIL_LAW]` rejects "a base-level `HashToUIntNN` phantom" while `:82` commits the wrong-width/wrong-call variant. |
| `api-csparse.md` byte-identical cross-tier dupe | **HOLD** | `shasum` identical across tiers (`a69ef8e6…`, 16 132 B each). CSparse consumed by TWO strata (`Rasm.csproj:8` + `Rasm.Compute.csproj:16`) → genuine multi-stratum; dedupe to the SHARED tier, delete the kernel copy. |
| `api-tensors.md` byte-identical cross-tier dupe | **REFUTED** | NOT byte-identical: shared `091f9b5…` 23 931 B vs kernel `84835ef…` 24 415 B (+484 B). `diff` shows the kernel tier is a clean ADDITIVE SUPERSET — enriched generic `Norm` sig (`:151`), enriched `SumOfMagnitudes` sig (`:200`), and a NEW `[11] IsFiniteAll`/`IsFiniteAny` row (`:209`, "the validity gate `Numerics/matrix`/`Domain/rails` compose"). Tensors is 4-stratum (`Rasm:13`, `AppHost:91`, `Compute:23`, `Persistence:52`). E13 mis-groups it with csparse: it is an OVERLAY PAIR, not a dupe — deduping DESTROYS the kernel validity-gate enrichment. Treat like the mathnet overlays (verify superset, never collapse). |
| `api-mathnet-numerics.md`/`api-mathnet-providers.md` deliberate kernel overlays — verify subset not fork | **HOLD** | Both are proper trimmed subsets, not forks. `numerics`: shared 25 FFT/fourier/transform refs → kernel 2 (FFT-trimmed as declared). `providers`: shared 23 MKL refs → kernel 0 (OpenBLAS-only as declared). No divergent/contradictory content; additive kernel seam notes only. |
| NO LanguageExt catalog in either tier | **REFUTED** | `libs/csharp/.api/api-languageext.md` EXISTS (170 LOC, first-class: `[01]-[PACKAGE_SURFACE]`→`[04]-[IMPLEMENTATION_LAW]` with full `Fin`/`Validation`/`Option`/`Eff`/`IO`/`Error`/`Seq`/trait surface, version `5.0.0-beta-77`, license MIT, `lib/net10.0`). Correctly SHARED-tier only (universal substrate); kernel tier correctly has none. The `[ROSTER_RECONCILIATION]` "AUTHOR the absent LanguageExt catalog" + `[04]` SHARED-TIER-LAW authoring obligation are DISCHARGED. This is the +1 that makes the shared tier 31 not 30. |

E13 net: the api-hashing `:82` falsehood is the one LIVE catalog defect. csparse dedupe stands. tensors is mis-classed (overlay, not dupe). mathnet overlays verified clean. LanguageExt already landed.

### E14 — Version/platform facts (feed-verified 2026-07-03) — ALL HOLD

| Package (register claim) | Feed truth | Verdict |
|---|---|---|
| MathNet `6.0.0-beta2` newest, no stable 6.0 | stable latest **5.0.0** (2022-04-03); prerelease latest **6.0.0-beta2** (2025-03-02) | HOLD |
| LanguageExt `5.0.0-beta-77` newest, active 2025-12 | **5.0.0-beta-77** published **2025-12-30** | HOLD |
| GShark `2.3.1` dormant since 2023-08 | latest (incl. prerelease) **2.3.1** published **2023-08-12** | HOLD — genuinely dormant, load-bearing for the `[V2]`/GShark hinge |
| ManifoldNET stalled `1.0.7-alpha` 2024-08 | latest (incl. prerelease) **1.0.7-alpha** published **2024-08-25** | HOLD |
| CavalierContours `1.0.0` 2026-06 | **1.0.0** published **2026-06-03** | HOLD |
| geometry4Sharp `1.0.0` on NuGet since 2022-11 (WATCH trigger stale) | **1.0.0** published **2022-11-14** | HOLD — "on first NuGet release" trigger is dead; re-key mandate valid |
| CSparse.Extensions unpublished | "not found on any package source" | HOLD |
| TVGL 2019 | latest **1.0.19.213** published **2019-04-28** | HOLD |
| Kemsekov.GraphSharp `3.1.2` 2024-02 | **3.1.2** published **2024-02-13** | HOLD |
| MIConvexHull `1.1.19.1019` 2019 | latest **1.1.19.1019** published **2019-10-19** | HOLD |
| QuikGraph `2.5.0` 2022 | latest **2.5.0** published **2022-07-04** | HOLD |
| Supercluster.KDTree.Net `1.0.22` 2025-08 | latest **1.0.22** published **2025-08-20** | HOLD |
| Clipper2 `2.0.0` 2025-12 (`[04]`) | latest **2.0.0** published **2025-12-17** | HOLD |

Every E14-cited pin on disk equals newest-available EXCEPT the deliberate MathNet beta (ruled to downgrade). No pin is behind feed.

**E14 completeness gap (DRIFT, additive):** the register's MathNet anchor `PROPS:62-64` names three beta pins; disk carries a **fourth** — `MathNet.Numerics.FSharp` `6.0.0-beta2` at `PROPS:467` (Transitive Floors). The widened survival sweep must include it, not just `:62-64`.

---

## [B] REGISTER VERDICTS — [04] STUB/FEED ANCHORS

### SHARED-TIER LAW anchors

| Anchor | Verdict | Disk |
|---|---|---|
| `CommunityToolkit.HighPerformance` `Span2D`/`MemoryOwner<T>`/`ParallelHelper` (`api-highperformance.md:26,35,54`) | **HOLD** | `Span2D<T>` at `:26`, `MemoryOwner<T>` at `:35` (`[10]`), `ParallelHelper` at `:54` (`[07]`) — all three present at/near the cited lines. Pin `PROPS:38` `8.4.2`, kernel-blind today (admit-on-`[V6]`-composition per brief). |
| `System.Numerics.Tensors` span-feed law `api-tensors.md:248` | **HOLD** | Shared `:248` `[INTEGRATION_STACKING]`: "Span-feed, not re-pack: the geometry kernel's struct-of-arrays coordinate buffers … feed `TensorPrimitives` operators DIRECTLY as `ReadOnlySpan<T>` … a second tensor-shaped re-pack … is the rejected double-layout." Anchor exact. Bonus `[V13]` backing verified: `ReadOnlyTensorSpan<T>` element-type-generic (`:14,:29`), `ConvertToHalf`/`ConvertToSingle` (`:175-176`) — the descriptor-dispatched `<float>`/`<Half>` typed-view claim is real. |
| `System.IO.Hashing` `XxHash128` seed-zero through `Domain/identity.md` ONLY | **HOLD** (kernel), defect in shared | Kernel `api-hashing.md:82` states the seed-zero federation entry correctly; shared `:82` is the falsehood (E13). |

### Per-package anchors

| Anchor | Verdict | Disk |
|---|---|---|
| GShark station/frame spine `Divide`/`ParameterAtLength`/`PointAtLength`/`PerpendicularFrames` (`api-gshark.md:81-88`) | **HOLD** (trivial line drift) | Kernel `api-gshark.md` curve entrypoint table rows: `PointAtLength` `[02]`, rotation-minimizing `PerpendicularFrames` `[05]`, `ParameterAtLength` `[06]`, `Divide(maxSegmentLength,equalSegmentLengths)`+`DivideByChordLength` `[08]`, `DecomposeIntoBeziers` `[10]`, `Offset(distance,Plane)` `[11]`. Rows span ~`:80-91` (brief `:81-88` off by a row or two — conversion-era drift, allowed). Every `[V2]`/`[04]` GShark member VERIFIED PRESENT. |
| `NurbsSurface.ClosestParameter` UV fallback (`api-gshark.md:111`) | **HOLD** | Surface table `[06] ClosestParameter(Point3) → (double U,double V)` at ~`:113`. Exact-intent match (foreign-point UV pullback). |
| GShark normalized-domain convention (`api-gshark.md:165`) | **HOLD** | `[VALUE_PROFILE]` "parameter convention: curve parameters are the NORMALIZED domain `[0,1]` … surface `(u,v)∈[0,1]²`" at `:165`. |
| GShark SSI absence (`[V2]` "no surface-surface intersection") | **HOLD** | `[INTERSECTION_SCOPE]` `Intersect` family (`:142-146`): PlanePlane/LinePlane/LineLine/LineCircle/PlaneCircle/CurveCurve/CurveLine/CurveSelf/CurvePlane/PolylinePlane — NO surface-surface. The host-deferred SSI charter row is warranted. License MIT `:31`, version `2.3.1` `:30`. |
| Supercluster.KDTree.Net landed `neighbors.md` lane | **HOLD** | Catalog `api-kdtree.md` present (kernel tier); pin `PROPS:76` `1.0.22`, kernel ref `Rasm.csproj:19`. Feed newest. |
| PeterO directed-rounding interval filter (`api-peteronumbers.md:153`) | **HOLD** | `[LOCAL_ADMISSION]` bullet ~`:152-153`: "`ERounding.Floor`/`Ceiling` with `RoundToExponentExact`/`RoundToPrecision` bracket a value into a directed-rounded interval … (no current consumer composes it)". The one genuinely unmined ladder enhancement, present and flagged unmined — `[V14]` evaluation target confirmed real. |
| QuikGraph downstream-view claims (`api-quikgraph.md:11`) re-verify vs landed admission | **HOLD** | `:9-14` bullet: `Rasm` folds `Spatial/neighbors` kNN → `UndirectedGraph<int,SEdge<int>>` → `MinimumSpanningTreePrim` under Hoppe-DeRose `1−|nᵢ·nⱼ|` — exactly the landed bounded lane (`README:121`). Catalog frames QuikGraph as cross-stratum shared substrate (correct shared-tier placement). Pin `PROPS:41` `2.5.0`, kernel ref `Rasm.csproj:31`. |
| Manifold ADD — ManifoldNET stalled, thin in-house P/Invoke, `.api` authored with admission | **HOLD** (+ TRAP flagged) | ManifoldNET README confirms it is the elalish/manifold C# P/Invoke binding, stalled `1.0.7-alpha` (2024-08), default pkg "cannot export glb". No `api-manifold.md` in either tier (correct — authored with admission). **TRAP:** a NuGet package named `Manifold` `1.0.0` (2026-03-29, MIT) EXISTS but is a **homonym** — a CLI/MCP source-generator framework (`GeneratedOperationRegistry`/`GeneratedMcpCatalog`), NOT the geometry engine. A future agent must NOT pin `Manifold`; the `[V5]` in-house `manifoldc` P/Invoke mandate stands unchallenged. |
| Fabrication-lane pins `CavalierContours`/`Clipper2`/`SharpVoronoiLib` in "float-lane group" `PROPS:70-76`, zero kernel refs | **DRIFT** | Line anchors right (CavalierContours `:70`, Clipper2 `:71`, SharpVoronoiLib `:75`) and "zero kernel references" TRUE (none in `Rasm.csproj`). But there is NO "float-lane group" — those pins live in the group **labeled `Kernel Geometry`** (`PROPS:69`), CO-MINGLED with real kernel pins (GShark `:73`, MIConvexHull `:74`, Supercluster `:76`) and the dropped `geometry3Sharp` `:72`. The re-group to a Fabrication label is the pending wave-1 motion the brief itself mandates; the descriptive "float-lane group" is not on disk. |
| Catalog hygiene — `api-rhino.md:134` "repair composes the Rhino heal surface" corrects | **REFUTED** | `sed -n '134p'` → BLANK line. Whole-file grep (`heal\|repair\|RebuildNormals\|Weld\|hole`) returns only NEUTRAL member listings (`:60` "the booleans/repair/reduce surface", `:145` `RebuildNormals`/`UnifyNormals`/`Weld`/`Unweld`/`Compact`/`FillHoles`/`HealNakedEdges`/`MergeAllCoplanarFaces`). No "repair composes" overclaim exists on disk (file mtime 2h — correction already applied). The mandated hygiene target is gone; `:145` lists the Rhino Mesh heal members accurately without asserting composition. |
| Catalog hygiene — `api-mathnet-providers` keeps osx-arm64 managed-fallback + OpenBLAS-only headline | **HOLD** | Kernel `api-mathnet-providers.md:7` "OpenBLAS as the sole opt-in native accelerator and a managed-path parallelism governor"; `:12` "OpenBLAS native assets are x64-only (no osx-arm64)"; `:29` "no osx-arm64 asset"; `:166-167` "on osx-arm64 (no native asset) … falls back to `UseManaged`". Headline present and honest. |

### MathNet.Numerics `[04]`/`[ROSTER_RECONCILIATION]` — the beta ruling + OpenBLAS keep (verdict SPLIT)

- **Downgrade feasibility — HOLD, and CLEANER than the register knew.** `MathNet.Numerics` stable latest is `5.0.0` (2022-04-03). CRITICAL: the split provider packages ALSO have coherent stable `5.0.0` releases — `MathNet.Numerics.Providers.OpenBLAS` `5.0.0` and `MathNet.Numerics.Providers.MKL` `5.0.0`, both published `2022-04-03` (same day as core). The "downgrade to 5.0.0 stable" ruled default is version-feasible for the ENTIRE kernel-consumed MathNet stack, not just the core — no provider is 6.0-only. The survival sweep's only open question is a fence composing a genuine 6.0-only member.
- **OpenBLAS keep — DRIFT toward the drop branch, on independent evidence.** The keep is DECORATIVE on the stated osx-arm64 target at ANY version: (1) nuget cache `mathnet.numerics.providers.openblas/6.0.0-beta2/` ships `lib/{netstandard2.0,net6.0,net8.0,net48}` ONLY — no `runtimes/`, and a global-cache find for any `openblas` `.dylib`/`.so`/`arm64` returns EMPTY; (2) the catalog self-admits `:12,:167` the native is x64-only and osx-arm64 falls to `UseManaged`. The brief's "verify the arm64 asset or the drop re-opens with evidence" resolves to NO ASSET → the drop branch is evidence-backed. Recommendation for the DECISION: DROP the `MathNet.Numerics.Providers.OpenBLAS` pin (`PROPS:64`) + kernel reference (`Rasm.csproj:11`) and rely on managed MathNet, OR keep it EXPLICITLY as a documented x64-deployment hedge (decorative-on-arm64) — but "opt-in native accel" is a non-fact on the target RID today.

### Reserve/no-op anchors (recorded so silence is not oversight)

| Anchor | Verdict | Disk |
|---|---|---|
| `TYoshimura.DoubleDouble` reserve; QEM/LM `ddouble` rows land per README:86 | HOLD | `api-doubledouble.md` present (kernel), pin `PROPS:66` `5.0.8`, ref `Rasm.csproj:14`. |
| `ExtendedNumerics.BigRational` `Fraction` oracle; `Mediant` reserve | HOLD | `api-bigrational.md` present (kernel), pin `PROPS:61` `3000.0.2.132`, ref `Rasm.csproj:9`. |
| `CSparse` reached through landed `matrix.md` only; LGPL-2.1 recorded | HOLD | `api-csparse.md` present both tiers (dedupe → shared), pin `PROPS:60` `4.4.0`, ref `Rasm.csproj:8` + `Compute:16`. |
| `MIConvexHull` consumer-backed by landed `cloud.md`; removal dead | HOLD | `api-miconvexhull.md` present (kernel), pin `PROPS:74` `1.1.19.1019`, ref `Rasm.csproj:23`. |
| Judged shared-tier stubs NOT mined (Generator.Equals / Riok.Mapperly / UnitsNet) | HOLD | Catalogs present shared-tier (`api-generator-equals.md`, `api-mapperly.md`, `api-unitsnet.md`); no kernel geometry-plane obligation — correct silence. |
| Stratum-split records (`Alimer.Bindings.MeshOptimizer`=Compute, `PicoGK`=Fabrication, `OcctNet.Wrapper`=Fabrication) | HOLD | Pins live in `Compute`/`Fabrication` groups (`PROPS:89,83,82`), zero kernel refs; no kernel catalogs. |
| Named-and-rejected never re-proposed (SISL/TetGen/CGAL/…/MeshLib/OCCT) | HOLD | No pins, no refs, no catalogs for any rejected engine in either tier or manifest. |

---

## [C] MANIFEST VERDICTS

### `libs/csharp/Rasm/Rasm.csproj` — "12 references" — HOLD (exact)

12 `PackageReference` entries, no more: Numerics 7 (`CSparse:8`, `ExtendedNumerics.BigRational:9`, `MathNet.Numerics:10`, `MathNet.Numerics.Providers.OpenBLAS:11`, `PeterO.Numbers:12`, `System.Numerics.Tensors:13`, `TYoshimura.DoubleDouble:14`); Geometry 2 (`GShark:18`, `Supercluster.KDTree.Net:19`); Computational Geometry 1 (`MIConvexHull:23`); Content Hash 1 (`System.IO.Hashing:27`); Graph Algorithms 1 (`QuikGraph:31`). Conversion drops confirmed on disk: NO Triangle, NO geometry3Sharp, NO MathNet.Numerics.Providers.MKL, NO Clipper2/CavalierContours/SharpVoronoiLib/LibTessDotNet reference. Kernel Usings (`:34-44`) inject `Rasm.Domain` + Rhino namespaces — the RhinoCommon-aware end-to-end compile surface (`[PLACEMENT_LAW]`). One reference to watch under `[V8]`: no upward `Rasm.Compute` reference exists here (good — the `index.md:27` `using Rasm.Compute.Solver` inversion is a FENCE-level import, not a manifest reference).

### `Directory.Packages.props` — label-grouped, hand-edited — HOLD with two motions pending

- MathNet trio `6.0.0-beta2` at `:62-64` (+ `MathNet.Numerics.FSharp:467`) — the downgrade motion (§B).
- "Kernel Geometry" group `:69-77` co-mingles kernel + Fabrication-lane + dropped pins — the wave-1 re-group motion (§B Fabrication-lane DRIFT). Post-motion the group should hold only GShark/MIConvexHull/Supercluster; CavalierContours/Clipper2/SharpVoronoiLib move to a `Fabrication` label; `geometry3Sharp` disposition routes to its consuming campaigns.
- LanguageExt.Core `:29` `5.0.0-beta-77` (newest), CommunityToolkit.HighPerformance `:38` `8.4.2` (kernel-blind pending `[V6]`), QuikGraph `:41` `2.5.0`, System.IO.Hashing `:31`, System.Numerics.Tensors `:42` — all newest/consistent.

---

## [D] CROSS-CUTTING FINDINGS (beyond the register)

1. **Cross-tier `.api` file census (5 name-overlaps, only 1 a true dupe).** Files present in BOTH tiers: `api-csparse.md` (byte-identical → dedupe to shared), `api-hashing.md` (overlay pair; fix shared `:82`), `api-mathnet-numerics.md` (kernel = FFT-trimmed subset, OK), `api-mathnet-providers.md` (kernel = OpenBLAS-only subset, OK), `api-tensors.md` (kernel = additive superset, OK — do NOT dedupe). The `[ROSTER_RECONCILIATION]` dedupe ruling must be re-scoped: dedupe csparse ONLY; the other four are intentional overlay pairs whose kernel/shared divergence is verified coherent. Charter-as-it-should-be: one dedupe row (csparse→shared) + four "verify-overlay-stays-subset/superset" rows.

2. **No orphan catalog, no orphan reference.** All 12 kernel references resolve to a catalog (kernel-tier for the 10 kernel-scoped packages; shared-tier `api-quikgraph.md` for the multi-stratum QuikGraph reference; shared-tier for System.IO.Hashing which also carries the kernel federation overlay). No kernel `.api` file lacks a corresponding pin+reference. The absence of `api-manifold.md` is intended (authored with the `[V5]` admission).

3. **Naivety axis — decorative capability (structural).** The OpenBLAS provider is the corpus's clearest "advertised-but-inert" surface at the manifest tier: a kernel `PackageReference` + full `.api` catalog for a native accelerator that does not exist on the target RID. It mirrors the fence-tier dead-engine class (`view.md` BSP, `offset.md` discarded medial) one stratum down. The honest resolution is drop-or-document-as-x64-hedge.

4. **Naivety axis — homonym pin trap (anticipatory).** The live `Manifold` `1.0.0` NuGet ID is a same-name, wrong-domain package. Because `[V5]` names "Manifold" as the tier-3 engine, an under-careful realization agent could pin the framework by ID. The DECISION should spell the engine as `elalish/manifold` via `manifoldc` P/Invoke and explicitly name-and-reject the `Manifold` NuGet ID, exactly as the brief name-and-rejects SISL/TetGen/etc.

5. **Phantom-member sweep — clean at the catalog tier.** Every `[V2]`/`[04]` GShark member the parametric tier will compose is fenced in `api-gshark.md` (station/frame/offset/UV-pullback family). No `[04]` anchor cites a member absent from its catalog. The GShark envelope probe's likely verdict from catalog evidence is FEASIBLE_WITH_GAPS: the curve/surface instance algebra + fitting + intersection are present; the SSI gap is real and its closure (host-deferred) is named; the dormancy (2023-08, feed-confirmed) is the standing risk that makes MIT vendor-and-own the hedge.

6. **Count/mtime reconciliation.** shared 31 = brief's 30 + the just-authored `api-languageext.md`. The 2h edit wave already discharged: LanguageExt authoring (E13), api-rhino heal-overclaim correction (`[04]`), and the mathnet FFT-trim/OpenBLAS-only overlays (E13/`[ROSTER_RECONCILIATION]`). The still-open catalog defect is the shared `api-hashing.md:82` falsehood; the still-open manifest motions are the MathNet downgrade+OpenBLAS disposition and the Fabrication-lane re-group.

---

## [E] CHARTER-AS-IT-SHOULD-BE (roster/catalog obligations the DECISION should carry)

- **api-hashing (shared) `:82`** → rewrite the `[STACK]` seed bullet to the federation truth: `ContentHash.Of` composes `XxHash128.HashToUInt128` at seed zero → `UInt128`, deferring to the landed `Domain/identity.md` entry; the shared catalog's generic surface may name `XxHash3` as the fast-root example but must NOT assert it is the `ContentHash` seed.
- **api-csparse** → dedupe to `libs/csharp/.api/api-csparse.md` (shared, multi-stratum kernel+Compute); delete `libs/csharp/Rasm/.api/api-csparse.md`.
- **api-tensors** → KEEP both; reclassify from "dedupe" to "overlay pair"; the kernel superset (`IsFiniteAll`, validity-gate notes) is load-bearing for the `Domain/rails`/`Numerics/matrix` finiteness gate and must survive.
- **api-mathnet-numerics / api-mathnet-providers** → keep both tiers; the kernel FFT-trim / OpenBLAS-only subsets are verified proper; re-run the subset check after the MathNet 5.0.0 downgrade lands (the 5.0.0 surface differs from 6.0.0-beta2; both catalogs must re-verify against the downgraded assembly).
- **MathNet stack** → downgrade `MathNet.Numerics` + `.Providers.OpenBLAS` (+ `.FSharp`, + kernel-dropped `.MKL` if kept for other campaigns) `PROPS:62-64,467` to `5.0.0` stable; the beta survives ONLY on a recorded 6.0-only fence member (bump blocker). Resolve OpenBLAS: DROP `PROPS:64` + `Rasm.csproj:11` (evidence-backed) or keep-as-x64-hedge documented.
- **Fabrication-lane re-group** → move `CavalierContours:70`/`Clipper2:71`/`SharpVoronoiLib:75` out of the `Kernel Geometry` group into a `Fabrication` label; the folder-tier catalogs are the standing `Rasm.Fabrication/.api` copies.
- **api-manifold** → author WITH the `[V5]` admission (kernel tier), spelling the engine `manifoldc` (elalish/manifold) and recording the `Manifold` NuGet-ID homonym as rejected.
- **WATCH re-key** → geometry4Sharp trigger "on first NuGet release" is dead (on NuGet since 2022-11-14); re-key to a post-2022 release signal per `[ROSTER_RECONCILIATION]`.
