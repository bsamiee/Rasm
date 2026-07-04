# [DOSSIER] — Fabrication api-manifests lane

Scope: the 14 folder-tier `.api` catalogs under `libs/csharp/Rasm.Fabrication/.api/` (COMPLETE), the shared substrate tier `libs/csharp/.api/` where cited, `Directory.Packages.props` Fabrication-relevant groups, `Rasm.Fabrication.csproj`, cross-csproj ownership, and live nuget-feed verification of the roster. Register rows re-verified on disk: E1 + every `[ROSTER_RECONCILIATION]`/`[04]` disposition. Method: full-read all 14 catalogs, word-boundary consumer grep over the 17 pages, fence-namespace census, comprehensive stale-governance sweep across the `.api` tier, and `nuget` MCP feed calls for every pin + both ADD candidates + the dominated alternative.

Anchor convention as in the brief `[02]`: `.planning/`-relative for pages; `CSPROJ:` = `Rasm.Fabrication.csproj`; `PROPS:` = `Directory.Packages.props`; `README:`/`ARCH:` = package root; `.api/*` = folder-tier catalog.

---

## [01] — REGISTER VERDICTS (assigned rows)

### E1 — Dead admissions — **HOLD** (one minor anchor-pairing DRIFT)

CONFIRMED on disk, decisively. Zero fence consumers for all four across the 17 pages: a word-boundary `using`/token grep returns **no** `using` for `PicoGK`, `OcctNet`, `DSTV`, or `SharpVoronoiLib`/`Voronoi` in any of the 17 pages. The only PicoGK-adjacent hits are `Toolpath/slicing.md:19,222` prose calling gyroid a 2D-hatch `InfillPattern` row — the exact false-collapse `[V6]` flags (`api-picogk.md:152` states the planar quartet cannot express TPMS), NOT a consumer.

- Pins present: `CSPROJ:13-26` carries all 14. The four dead ones sit at `CSPROJ:16` (`DSTV.Net`), `:19` (`OcctNet.Wrapper`), `:20` (`PicoGK`), `:24` (`SharpVoronoiLib` `Aliases="Voronoi"`). **DRIFT (pairing only):** E1's prose pairs `CSPROJ:16,19,20,24 → PicoGK/OcctNet.Wrapper/DSTV.Net/SharpVoronoiLib`, but `PicoGK` is at `:20` and `DSTV.Net` at `:16` (swapped). The line-SET `{16,19,20,24}` is exact; the ordered pairing is wrong. Corrected mapping above.
- README charters exist at **package root** `README.md`: `:35 [STEEL_FABRICATION_EXCHANGE]` (DSTV, `:36`), `:51 [VORONOI_TESSELLATION]` (SharpVoronoiLib, `:52`), `:54 [IMPLICIT_VOXEL]` (PicoGK, `:55`), `:60 [SOLID_INGRESS]` (OcctNet, `:61`) — all "fence-realized nowhere." HOLD (E1's `README` anchor is the package root per brief `[02]`, not `.planning/README.md`).
- `.api` catalogs "121-161 LOC each": CONFIRMED against `wc -l` — `api-picogk.md`=161, `api-occtnet-wrapper.md`=143, `api-dstv-net.md`=143, `api-sharpvoronoilib.md`=121. (The `loc` CODE metric gives 99-131; the brief's "LOC" is raw line count.)
- Feed truth: all four are **current, actively maintained** — PicoGK 2.2.0 (published 2026-06-05), OcctNet.Wrapper 0.1.1 (2026-04-23), DSTV.Net 1.3.0, SharpVoronoiLib 1.2.0 (2026-02-24). "Dead" = zero-consumer, NOT abandoned. The INTEGRATION-FIRST mandate is well-founded: these are live, capable packages with real production scope, not roster rot.

### [ROSTER_RECONCILIATION] / [04] dispositions

| # | Disposition | Status | Anchor / evidence |
|---|---|---|---|
| R1 | `netDxf` VERIFY-REMOVE (`PROPS:357`) | **HOLD** | `PROPS:357` `netDxf 2023.11.10` present, under a non-Fabrication rendering cluster (~`:340-361`). Feed latest = 2023.11.10 (2025-01-02) — pin current. Owner attribution AppUi `[V7]`. |
| R2 | `netDxf.netstandard` VERIFY-REMOVE (`PROPS:472`) | **DRIFT** | Actual line is **`PROPS:468`** (not 472; `:472` is `VividOrange.ICases`), under the "Compute, interchange, and BIM closure" cluster. Pin `3.0.1`; feed latest = 3.0.1 (2024-05-20) — current. Compute `[V8]` FEALiTE2D.Plotting transitive floor. |
| R3 | `RectpackSharp` REMOVE on redundancy (`PROPS:84`) | **HOLD** | `PROPS:84` under `Label="Fabrication"`. Sole consumer = `nfp.md` (`using RectpackSharp`, `:34`; `RectanglePacker.Pack(..., PackingHints.FindBest, ...)`, `:282`). Feed 1.2.0 current. Redundancy real — see R7. |
| R4 | `RectangleBinPack.CSharp` re-group from vacated Materials label (`PROPS:165`) | **HOLD** | `PROPS:165` under `Label="Materials"` (line 164) — confirms "vacated Materials label" framing. Consumer = `stock.md` (`using RectangleBinPacking`, `:41`; `MaxRectsBinPack`/`GuillotineBinPack`/`SingleBinPack`). Feed 1.0.4 published **2026-01-27** (current, actively maintained). Re-group to Fabrication label PENDING. |
| R5 | Float-lane re-group `Clipper2`/`CavalierContours`/`SharpVoronoiLib` → Fabrication label (`PROPS:70-75`) | **HOLD (unexecuted)** | All three still under `Label="Kernel Geometry"` (`:69-77`): `CavalierContours`=`:70`, `Clipper2`=`:71`, `SharpVoronoiLib`=`:75` (`geometry3Sharp`=`:72` also in range). **Re-group NOT landed on disk** — matches the brief's fallback ("execute the motion where the geometry pass has not run"). Well-founded: each is sole-consumed by `Rasm.Fabrication.csproj` (kernel `Rasm.csproj` references none). This campaign owns the motion. |
| R6 | `SharpVoronoiLib` KEEP | **HOLD**; catalog dual-ownership **REFUTED** | Sole consumer = Fabrication (`CSPROJ:24`, `Aliases="Voronoi"`). `[04]` members verified: `Relax(iterations, strength, reTessellate)` = `api-sharpvoronoilib.md:85`; `VoronoiSite.Centroid` = `:99`; point-site-only = `:5,114,121`. **REFUTED sub-claim:** `api-sharpvoronoilib.md:3,18` asserts "OWNED by BOTH `Rasm.Fabrication` AND the `Rasm` geometry kernel (`Rasm/Rasm.csproj`)" — but `rg -l SharpVoronoiLib '*.csproj'` returns ONLY `Rasm.Fabrication.csproj`. The kernel supersession removed kernel ownership; the catalog's dual-owner prose is stale. |
| R7 | `RectpackSharp` ⊂ `RectangleBinPack.CSharp` (E12 redundancy proof) | **HOLD** | RectpackSharp = maxrects/guillotine over `Span<PackingRectangle>` (`api-rectpacksharp.md:24`), `Pack` defaults to `PackingHints.FindBest` (`:61,72`). RBP strictly richer: 4 packers + `SingleBinPack` + `FreeRectangles` remnants (`api-rectanglebinpack-csharp.md:27-32,57`). `README:65` self-contradiction: rejects MaxRect/BinPack.NET "superseded by RectpackSharp" while RBP subsumes RectpackSharp. |
| R8 | `CavalierContours` sited at ONE `[V10]a` Fabrication stratum | **HOLD**; `lnContours` divergence **REFUTED**; new `.Positive` phantom | Sole consumer = Fabrication. `[04]` members verified in catalog: `PlineOffset.ParallelOffset<O,T>` (`api-cavaliercontours.md:38,115`), `Shape<T>.FromPlines(...).ParallelOffset(offset, ShapeOffsetOptions<T>)` (`:70`), `FindPointAtPathLength` (`:103`), `StaticAABB2DIndex<T>` (`:56`). **REFUTED (E3 sub-claim):** the `lnContours`/`PlineOffset.ln<>` divergence at `clipper.md:169-227` does NOT exist on disk — the fence already declares the real `CavalierContours.Core/.Polyline/.Shape/.Spatial` namespaces (`clipper.md:169-171`) and `PlineOffset.ParallelOffset<Polyline<double>, double>` (`:213`). **New live phantom:** `clipper.md:221` calls `result.Positive`, but `api-cavaliercontours.md:47` states the accessor is `PosPlines`, "never a `Positive` accessor" — the divergence-resolution disposition still has this target. |
| R9 | ADD `OpenCAMLib` (non-distribution) | **HOLD** | Feed: `OpenCAMLib` NOT FOUND on nuget.org — confirms "NO C# binding or NuGet package exists." No `.api` catalog, absent from `PROPS` (correct — ADD pending Phase-1 probe). |
| R10 | ADD `lib3mf` (non-distribution) | **HOLD** | Feed: `lib3mf` NOT FOUND on nuget.org — confirms "NOT on NuGet, vendored-binding." Dominated alternative `IxMilia.ThreeMf` ALSO NOT FOUND — confirms "unpublished on any feed." No `.api` catalog, absent from `PROPS` (correct — ADD pending). |
| R11 | REPLACE-candidate `geometry3Sharp` (abandonment) | **HOLD** | Two consumers CONFIRMED exactly: `Rasm.Bim.csproj:22` + `Rasm.Fabrication.csproj:17`. Feed: latest = 1.0.324 published **2019-03-07** (7 years stale) — abandonment feed-corroborated. `api-geometry3sharp.md:106` "dropped CavalierContours / one-Clipper2 law" self-contradiction present. |
| R12 | `Clipper2` line-space owner + re-group | **HOLD (re-group pending)** | Consumer = `clipper.md` (`using Clipper2Lib`). Assembly/namespace = `Clipper2Lib` (package id `Clipper2`). `ReuseableDataContainer64` (`:60,180,201-209,245`), `DeltaCallback64` (`:221,225,244`), Minkowski (`:59,116-119,130-133`) all documented. Re-group per R5. |
| R13 | `Robots` cell owner held | **HOLD** | Consumer = `kinematics.md` (`using Robots`). `Program` look-ahead = `api-robots.md:72`. band-2500 stale at `:5,94` (see E10 sweep). |
| R14 | `MTConnect.NET-Common` model-only held | **HOLD** | Consumer = `magazine.md` (`using MTConnect.Assets.CuttingTools[.Measurements]`, `:28-29`; `GenerateHash(includeTimestamp:false)`, `:84,119`). `GenerateHash`↔`ContentHash.Of` reconciliation debt live at `api-mtconnect-net-common.md:3,116`. Persistence/Schema stale ref at `:119` (E10 sweep). |
| R15 | `ACadSharp` read-only held | **HOLD** | Consumer = `import.md` (`using ACadSharp[.Entities/.IO]`). `Failsafe`/`NotificationType.Error`/`Insert.Explode`/bulge+NURBS samplers all catalogued. Stale write-leg at `api-acadsharp.md:120` (E10 sweep). |

---

## [02] — PER-CATALOG VERDICTS (14 folder-tier catalogs)

Each catalog verified for: license-gate compliance, version-vs-`PROPS`, namespace truth, `[04]` member claims, stale-governance content. All version pins match the feed (see `[05]`). Catalogs are craft-strong and member-accurate; the defects are governance-ripple and one phantom.

- **api-picogk.md** (161 LOC) — Apache-2.0; 2.2.0 = `PROPS:83`; namespace `PicoGK`. Three-lane `[04]` claim fully present: implicit/TPMS (`IImplicit`→`Voxels`, `:29,152`), lattice/support + SLA/DLP `.cli` (`oVectorize`/`CliIo`/`Vdb2Cli`, `:60-64,155`), voxel NC-verify (`BoolSubtract`, `:100`). ALC-firebreak posture `:146,151-155` CONFIRMED. `[V5]` capsule-beam claim (`Lattice.AddBeam`=`:49`, `Voxels(Lattice)`=`:90-92`) CONFIRMED. **Defect:** `:5,113,154` frame `geometry3Sharp DMesh3` as "the SOLE biarc/curve-fit + mesh owner" and the wire meeting point — a permanent-owner coupling that contradicts the `[04]` geometry3Sharp REPLACE disposition; should re-frame to the owned biarc fold + kernel mesh vocabulary.
- **api-cavaliercontours.md** (138) — ISC; 1.0.0 = `PROPS:70`, feed current (2026-06-03); namespaces `.Polyline/.Shape/.Spatial/.Core` (match `clipper.md:169-171`). All `[04]`/`[V2]d` members accurate. **Load-bearing note:** `:47` explicitly documents `BooleanResult.PosPlines`/`NegPlines`, "never a `Positive` accessor" — this is the authority that flags `clipper.md:221`'s `result.Positive` phantom. Catalog also correctly records the `g3.BiArcFit2` retirement-for-arc-sourced-paths (`:153-155`).
- **api-occtnet-wrapper.md** (143→118loc) — wrapper MIT + native OCCT 7.9.3 `LGPL-2.1-with-OCCT-exception-1.0` dynamic-link (`:10`, admissible under the license gate); 0.1.1 = `PROPS:82`, feed current (2026-04-23). `[04]` ingress chain `ImportStep/Iges/Stl → OcctShape → Triangulate → OcctMesh` = `:59-61`; SCOPE_LIMIT (`libTKHLR`/`libTKXCAF` managed-unbound, single-shape) = `:120-124`. **Defect (new, unlisted):** `:137` `persistence: ... flow to Rasm.Persistence/Schema` — the deleted folder; must re-point to the content-keyed artifact index (`PBRIEF [V12]`).
- **api-dstv-net.md** (143→116loc) — Apache-2.0; 1.3.0 = `PROPS:80`; namespaces `DSTV.Net.*`. `KA` bend rows present (`:39,65,113`); emission-routed-to-posting = `:135`. **Defects (E10, all CONFIRMED):** band-2500 at `:3,126`; netDxf at `:132,143`.
- **api-sharpvoronoilib.md** (121) — MIT (`:12`, "NOT ISC"); 1.2.0 = `PROPS:75`, feed current. `Relax`=`:85`, `Centroid`=`:99`, point-site-only=`:5,114,121`. **Defect (REFUTED dual-ownership):** `:3,18` claims kernel co-ownership — kernel `Rasm.csproj` has no SharpVoronoiLib reference; slim to Fabrication-sole.
- **api-rectpacksharp.md** (75) — MIT; 1.2.0 = `PROPS:84`. `FindBest`=`:61`, `Pack` default=`:72`. Whole catalog is the REMOVE target; content re-homes to the nfp MaxRects fast-path over RBP. RAIL_LAW `:91` still carries the stale "dropped MaxRect/BinPack.NET" rejection (README:65 twin).
- **api-rectanglebinpack-csharp.md** (112) — MIT; 1.0.4 = `PROPS:165`, feed current (2026-01-27). **Namespace census fact:** assembly + single namespace = `RectangleBinPacking`, diverging from package id `RectangleBinPack.CSharp` (`:13-14`) — matches `stock.md:41 using RectangleBinPacking`. Subset-superset proof for E12 at `:27-32,57`.
- **api-hashing.md** (77) — MIT; `System.IO.Hashing 10.0.9` = `PROPS:31`. **Defect (E12/V10, CONFIRMED):** `:3` and `:88` assert "the no-shared-C#-tier law" / "C# has no shared tier" — FALSE: `libs/csharp/.api/api-hashing.md` exists (shared tier, 9001 bytes). The whole catalog frames raw `XxHash128.HashToUInt128` as the direct `Remnant`/`Stock` mint site (`:25,46,69,75`) — the second-hasher defect; must slim to the `api-unitsnet.md` thin-overlay pattern and name the kernel `Domain/identity.md ContentHash.Of` as the one mint site. `ContentHash` currently appears in ZERO pages and ZERO catalogs.
- **api-geometry3sharp.md** (88) — Boost-1.0; 1.0.324 = `PROPS:72`, feed 2019 (abandoned). `BiArcFit2`/`Arc2d`/`Segment2d`/`Vector2d` firewalled surface accurate. **Defect (E10, CONFIRMED):** `:106` RAIL_LAW-Reject asserts "the dropped CavalierContours arc-offset engine ... the one-Clipper2 law forbids" — a self-contradiction (CavalierContours IS admitted, `CSPROJ:14`); this line dies per `[V10]`.
- **api-robots.md** (85) — MIT; 2.1.2 = `PROPS:85`; geometry substrate `Rhino3dm` (`extern alias R3`). `internal` solver-class discipline correct. **Defects (E10):** band-2500 at `:5,94` (no literal netDxf at those lines — E10's "netDxf" tag in that batch belongs to the dstv anchors).
- **api-clipper2.md** (214) — BSL-1.0; 2.0.0 = `PROPS:71`; assembly/namespace `Clipper2Lib`. Clean — no stale-governance content. `Triangulate` correctly flagged buggy/excluded (`:122-123,247`).
- **api-acadsharp.md** (99) — MIT; 3.6.35 = `PROPS:120` (under `Label="BIM"`). READ surface, `Failsafe`, `Insert.Explode`, `Vertex2D.Location` (no `Pt` overload) all accurate. **Defect (E10, CONFIRMED):** `:120` RAIL_LAW-Reject attributes CAD write to "Rhino owns the host-bound native write" — stale per `[V7]` (write is AppUi's ACadSharp `DxfWriter`+`DwgWriter` leg). No literal `netDxf` token in this catalog.
- **api-mtconnect-net-common.md** (105) — MIT; 6.9.0.2 = `PROPS:81`; namespaces `MTConnect.Assets.CuttingTools[.Measurements]`. ISO-13399 measurement family + `GenerateHash`/`IsValid` accurate. **Defects:** `:3,116` frame `GenerateHash` meeting `XxHash128` (the second-identity-mint to reconcile to `ContentHash.Of`); **`:119` `Rasm.Persistence/Schema`** deleted-folder ref (new, unlisted).
- **api-unitsnet.md** (66) — MIT; 5.75.0 = `PROPS:43`; namespace `UnitsNet`. THE thin-overlay exemplar the hashing overlay must mirror. Covers Speed/Length/RotationalSpeed/Pressure only; `[V8]` extension (Force/Power/Temperature/Angle/Torque) PENDING.

---

## [03] — MANIFEST VERDICTS

- `Rasm.Fabrication.csproj`: 14 `PackageReference`s under `Label="Fabrication"` (`:12-27`), `ProjectReference` to `Rasm` + `Rasm.Element` only (`:7-10`, strata-correct: AEC-DOMAIN over `{Rasm, Rasm.Element}`, no AEC peer, no app-platform). `System.IO.Hashing` (`:25`) + `UnitsNet` (`:26`) referenced directly though centrally grouped elsewhere. `SharpVoronoiLib Aliases="Voronoi"` (`:24`) — an `extern alias` the catalog/brief do not mention.
- `Directory.Packages.props` label topology (Fabrication-relevant): `Label="Fabrication"` (`:79-86`) holds ONLY `DSTV.Net`/`MTConnect.NET-Common`/`OcctNet.Wrapper`/`PicoGK`/`RectpackSharp`/`Robots`. The float lane (`CavalierContours`/`Clipper2`/`geometry3Sharp`/`SharpVoronoiLib`) sits under `Label="Kernel Geometry"` (`:69-77`); `RectangleBinPack.CSharp` under `Label="Materials"` (`:164-165`); `ACadSharp` under `Label="BIM"` (`:120`); shared-tier mine rows (`Riok.Mapperly` `:30`, `System.Numerics.Tensors` `:42`, `CommunityToolkit.HighPerformance` `:38`, `NodaTime` `:39`, `QuikGraph` `:41`) under `Foundational Core`/`Shared Domain Primitives`. `MathNet.Numerics 6.0.0-beta2` (`:62`) — the `[04]` SHARED-TIER LAW expects the Geometry `[ROSTER_RECONCILIATION]` 5.0.0 downgrade; NOT landed (still beta2). `netDxf` `:357`, `netDxf.netstandard` `:468` (brief's `:472` is DRIFT). Neither `OpenCAMLib` nor `lib3mf` present (correct — ADD pending).
- Central-grouping vs consumer truth: the "Kernel Geometry" label is a manifest grouping artifact — `CavalierContours`/`Clipper2`/`SharpVoronoiLib` are each sole-consumed by `Rasm.Fabrication.csproj`, so the float-lane re-group to the Fabrication label is consumer-justified, not merely cosmetic.

---

## [04] — CROSS-CUTTING FINDINGS

### NAMESPACE census (fence-declared vs folder) — confirms `[V1]` "12 namespaces, 5 folders" EXACTLY

12 distinct declared namespaces across the 5 folders; 8 of 17 pages declare a namespace that is NOT its folder (the schism the DECISION re-homes):

| Declared namespace | Pages | Folder-true? |
|---|---|---|
| `…Process` | owner | YES |
| `…` (root) | faults | NO → `Process/` |
| `…ProcessModel` | family, magazine | NO → `Process/` (family), `Tooling/` (magazine) |
| `…ProcessPhysics` | physics | NO → `Process/` |
| `…Geometry2D` | clipper, import | NO → `Geometry2D/` (clipper), `Ingress/` (import) |
| `…Toolpath` | guard, motion, skeleton | YES |
| `…Kinematics` | kinematics | NO → `Kinematics/` |
| `…Additive` | slicing | NO → `Additive/` |
| `…Nesting` | nfp, stock | YES |
| `…Fixturing` | workholding | NO → `Fixturing/` |
| `…Posting` | program | YES |
| `…Projection` | projection | NO → `Documentation/` |

The realized namespaces already NAME the DECISION's target folders — the fence namespace rewrite is a MOVE-with-rename, not a new decomposition.

### CONSUMER census (admitted-package `using` per page)

10 of 14 packages have real fence consumers; the 4 dead admissions have none (E1):

| Page | Admitted packages composed (direct `using`) |
|---|---|
| `nfp.md` | `RectpackSharp`, `System.IO.Hashing` (+ `Clipper2` Minkowski via `clipper#POLYGON_ALGEBRA` seam) |
| `stock.md` | `RectangleBinPacking` (RectangleBinPack.CSharp) |
| `clipper.md` | `CavalierContours.Core/.Polyline/.Shape/.Spatial`, `Clipper2Lib` |
| `import.md` | `ACadSharp[.Entities/.IO]` |
| `program.md` | `g3` (geometry3Sharp) |
| `magazine.md` | `MTConnect.Assets.CuttingTools[.Measurements]`, `UnitsNet` |
| `physics.md` | `UnitsNet` |
| `kinematics.md` | `Robots` (→ `Rhino3dm` `extern alias R3`) |
| owner, family, faults, guard, motion, skeleton, slicing, workholding, projection | NONE (kernel/author-kernel composers) |

### SEAM census (cross-package edges, .api/manifest lens)

Admitted cross-package edges: `magazine→MTConnect(CuttingToolAsset)`; `clipper→Clipper2+CavalierContours`; `import→ACadSharp`; `program→geometry3Sharp`; `kinematics→Robots→Rhino3dm(R3)`; `nfp→RectpackSharp+System.IO.Hashing`; `stock→RectangleBinPacking + Rasm.Element/MaterialId (peer, wired-undeclared per E11)`; `physics→UnitsNet`. Persistence edges are currently mis-targeted to the deleted `Rasm.Persistence/Schema` in two catalogs (see stale sweep). The identity federation edge to the kernel `ContentHash.Of` is ABSENT everywhere (zero hits) — every current mint is a raw `XxHash128`/`GenerateHash` second-hasher.

### Stale-governance sweep across the 14 catalogs (comprehensive)

- band-2500 (should be 2700): `api-dstv-net.md:3,126`; `api-robots.md:5,94`. No `band-2700`/`2701` anywhere in the `.api` tier — the renumber has NOT rippled to the catalogs.
- netDxf: `api-dstv-net.md:132,143` only. (`api-acadsharp.md:120` is a Rhino-write-leg attribution, NOT a literal netDxf token.)
- **`Rasm.Persistence/Schema` (deleted folder) — TWO anchors BEYOND the register:** `api-mtconnect-net-common.md:119` and `api-occtnet-wrapper.md:137`. E10 lists only `ARCH:53,54`; these `.api` instances are new and must re-point to the `PBRIEF [V12]` blobstore/cache artifact index.
- `api-geometry3sharp.md:106` "dropped CavalierContours / one-Clipper2 law" self-contradiction.
- `api-hashing.md:3,88` false "no shared tier" law.

### Phantom / member-accuracy findings

- `clipper.md:221` `result.Positive` — phantom; the real CavalierContours accessor is `BooleanResult.PosPlines` (`api-cavaliercontours.md:47`). The only live member phantom found; the broader `lnContours`/`ln<>` divergence (E3) is already REFUTED on disk.
- No phantom members detected in the 14 catalogs themselves against their `[04]` member claims (assay unavailable; verified against the catalog surface + feed currency + fence usage). `assay api` re-verification of the CavalierContours `PosPlines`/`Positive` split is the one leg-1 member check to run against the restored assembly.

### Naivety axes (api-manifest lens)

- COVERAGE: the roster is provisioned for the full production telos (voxel/implicit, B-rep ingress, steel exchange, Voronoi CAM) but 4 of 14 packages deliver zero fence coverage — the thin-slice defect made manifest at the manifest layer. Two ADD lanes (OpenCAMLib surface engine, lib3mf 3MF egress) are entirely un-provisioned (correct — pending Phase-1).
- APPROACH: the manifest already models the roster as data (label-grouped `PackageVersion` rows), so no enumerated-instance defect at the manifest layer. The `.api` catalogs are per-package (correct), but two carry stale central-truth claims that fight the generator law: `api-hashing.md`'s "no shared tier" duplicates the shared surface instead of deferring to it (the `api-unitsnet.md` thin-overlay is the correct pattern), and `api-sharpvoronoilib.md`'s dual-ownership prose encodes a supersession the csproj already executed.

### nuget feed table (live, task-mandated)

| Package | PROPS pin | Feed latest | Published | Verdict |
|---|---|---|---|---|
| netDxf | 2023.11.10 | 2023.11.10 | 2025-01-02 | current |
| netDxf.netstandard | 3.0.1 | 3.0.1 | 2024-05-20 | current |
| RectpackSharp | 1.2.0 | 1.2.0 | 2024-01-09 | current (REMOVE on redundancy) |
| RectangleBinPack.CSharp | 1.0.4 | 1.0.4 | 2026-01-27 | current (KEEP) |
| geometry3Sharp | 1.0.324 | 1.0.324 | **2019-03-07** | abandoned (7y) |
| OcctNet.Wrapper | 0.1.1 | 0.1.1 | 2026-04-23 | current, young single-maintainer |
| CavalierContours | 1.0.0 | 1.0.0 | 2026-06-03 | current |
| PicoGK | 2.2.0 | 2.2.0 | 2026-06-05 | current |
| SharpVoronoiLib | 1.2.0 | 1.2.0 | 2026-02-24 | current |
| OpenCAMLib | — | NOT FOUND | — | non-distribution (ADD via C-shim/P-Invoke) |
| lib3mf | — | NOT FOUND | — | non-distribution (ADD via vendored binding) |
| IxMilia.ThreeMf | — | NOT FOUND | — | unpublished (dominated lib3mf alternative) |

Every pin matches its feed latest — the manifest is version-current; the roster motions are structural (consumer wiring, label re-group, REMOVE/ADD), not version bumps.
