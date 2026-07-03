# DOSSIER — Rasm.Fabrication — lane: ecosystem-core

Scope: real OSS ecosystem deep-dive for the CORE fabrication concerns (CNC/CAM toolpath, post-processing/G-code, FFF/DED/resin/powder slicing, NC verification, machine-tooling intelligence, production specs) + per-page verdict/defect pass through the ecosystem lens. License gate enforced: OSS or free-for-OSS-commercial ADMISSIBLE; pay-tiered/seat/proprietary REJECTED. Member/version verification: nuget MCP was non-functional this session (System.Xml.ReaderWriter v10 assembly-load fault) — versions/licenses corroborated by two web sources each. Corpus read in full: 17 planning pages + README + ARCHITECTURE + .csproj + central manifest rows + libs strata law.

Current roster (14 domain/substrate pins, verified in `Directory.Packages.props`): ACadSharp 3.6.35, CavalierContours 1.0.0, Clipper2 2.0.0, geometry3Sharp 1.0.324, MTConnect.NET-Common 6.9.0.2, OcctNet.Wrapper 0.1.1, PicoGK 2.2.0, RectangleBinPack.CSharp 1.0.4, RectpackSharp 1.2.0, Robots 2.1.2, SharpVoronoiLib 1.2.0, DSTV.Net 1.3.0, UnitsNet 5.75.0, System.IO.Hashing.

---

## [A] CANDIDATE TABLE — ranked categorical-best per CORE concern

Legend: MGD=managed .NET · NAT=native C/C++ (binding surface noted) · reach = in-proc P/Invoke feasibility / CLI-IPC / none.

### CONCERN 1 — G-code INGRESS (parse/round-trip existing NC programs) — corpus does NONE
| rank | package | lic | lang/reach | maturity | binding surface | sources |
|---|---|---|---|---|---|---|
| **1 (best)** | **gsGCode** (gradientspace/rms80) | MIT | MGD C#; GitHub-source (NuGet Packages:0) | active-ish (fork push 2025-01) | `GCodeFile`/parsers/interpreters/builders/writers/assemblers; **depends on geometry3Sharp (Boost) — ALREADY a Fabrication pin** | github.com/gradientspace/gsGCode; gradientspace.com/opensource |
| 2 | GcodeParserSharp (Reitberger) | Apache-2.0 | MGD; NuGet 1.1.4 net6/net8/MAUI | small, 2021 | parse + print-time/filament est. only (thin) | nuget.org/packages/GcodeParserSharp; github |
| 3 | gcodes (Michael-F-Bryan) | MIT | MGD; source | dormant | Lexer/Parser/`Gcode`/`ValueFor()` | github.com/Michael-F-Bryan/gcodes |
| — | GCodeForCSharp (2024), GCodeNet (lic?), ioSender GCode.cs (BSD, app-embedded) | mixed | MGD | immature/embedded | — | github |

**Verdict:** corpus has NO G-code READ path (`Posting/program` is emit-only). gsGCode is the categorical-best managed round-trip owner and its only dependency (geometry3Sharp) is already pinned — but it ships GitHub-source, not NuGet, so admission = source-vendor or build-from-source. Real need: re-post/optimize/verify an EXISTING NC program (shop-traveler round-trip), which the emit-only AST cannot do.

### CONCERN 2 — G-code EMISSION / post-processor dialect engine (controller families, canned cycles, macros)
| rank | package | lic | lang/reach | maturity | binding surface | sources |
|---|---|---|---|---|---|---|
| — MANAGED LIB: **NONE admissible** | — | — | — | — | — | — |
| ref (app, not lib) | E3Studio (YahyaSvm) | MIT | C#+C++ app | early-stage | C# posts GRBL/Fanuc/Haas/Mazak/LinuxCNC/Mach3-4 + Klipper/Heidenhain/Siemens presets; C++ ISO/Fanuc/Haas/Heidenhain | github.com/YahyaSvm/E3Studio |
| ref (python) | rybakov25/PostProcessor | (lic unclear) | Python | maintained | APT/CL→G-code, Siemens/Fanuc/Heidenhain/Haas, 3-5axis, RTCP/CYCLE800, Python macros | github.com/rybakov25/PostProcessor |
| **REJECTED** | STEP-NC `Generate` (StepTools), ModuleWorks PPF, EncyCAM DotnetPostprocessing.SDK | proprietary/commercial | — | mature | Fanuc/Haas/Heidenhain/Okuma/Siemens/APT/DMIS/CRCL | steptools.com; moduleworks.com; encycam.com |

**Verdict:** NO admissible managed post-processor library owns the controller-dialect concern → **VALIDATES the author-kernel `Posting/program` AST**. But the research exposes the dialect-GRAMMAR gap (see verdict candidate V3): the current 8 `PostDialect` rows are all RS-274/ISO-6983 word-address + Marlin; **Heidenhain Klartext is NON-ISO conversational (L/C/CC, BEGIN/END PGM, TOOL CALL, no G-words)** and cannot be expressed by the `GWord` AST; **Siemens Sinumerik** (CYCLE800/TRAORI), **canned cycles** (G81/G83/G76), and **Fanuc macro-B** (`#vars`, parametric) are named scope-mandate expansions the flat-move AST does not carry.

### CONCERN 3 — CAM toolpath: TRUE 3D-surface machining (drop-cutter/waterline/pencil/rest) — corpus has 2.5D ONLY
| rank | package | lic | lang/reach | maturity | binding surface | sources |
|---|---|---|---|---|---|---|
| **1 (best)** | **OpenCAMLib** (aewallin) | **LGPL-2.1** (relicensed 2018-08) | NAT C++; Python/Node/WASM bindings + precompiled libs **win/mac-x64+arm64/linux**; NO .NET binding, no NuGet | active (push 2025-02, 6 releases) | drop-cutter (3D finish), push-cutter (waterline), cutters ball/flat/bull/cone; `libocl` — P/Invoke-wrappable or out-of-proc | github.com/aewallin/opencamlib; anderswallin.net |
| **2 (HEM/adaptive)** | **FreeCAD libarea `Adaptive2d`** (Tolvanen) | LGPL-2+ | NAT C++; py bindings | active (FreeCAD CAM) | constant-engagement adaptive-clearing engine — the reference HEM the corpus author-kernels over the skeleton | github.com/FreeCAD/FreeCAD src/Mod/CAM/libarea/Adaptive.cpp |
| 3 (2.5D profile/pocket) | libarea (Heeks/danielfalck) | BSD-3 | NAT C++; py | dormant-stable | profile/pocket, `FitArcs`, `MakePocketToolpath`, Clipper-backed | github.com/Heeks/libarea; danielfalck/libarea |
| 4 (managed 2.5D) | PathCAM (xenovacivus) | **GPL** | MGD C# | dormant | 2.5D carve STL→GRBL G-code | github.com/xenovacivus/PathCAM |

**Verdict:** corpus CAM (`Toolpath/motion`) is 2.5D only — offset contours (Clipper2) + adaptive-over-straight-skeleton. There is **no 3D-surface machining** (drop-cutter finish, waterline, pencil, rest-machining) which the scope-mandate ("full CNC/CAM toolpath depth, multi-axis strategies, rest machining") demands. OpenCAMLib (LGPL, mac-arm64 precompiled) is the categorical-best and LGPL is admissible (dynamic-link/P-Invoke safe harbor), but it is native/no-.NET-binding → a bound owner (P/Invoke over `libocl` keyed by content, like the OcctNet posture) or companion lane. PathCAM (managed) is GPL → charter risk, superseded.

### CONCERN 4 — NC verification / material-removal simulation (collision/gouge, cut-surface, backplot)
| rank | package | lic | lang/reach | maturity | binding surface | sources |
|---|---|---|---|---|---|---|
| **1 (managed best)** | **MillSimSharp** (nyarurato) | **MIT** | MGD C#; **NuGet 0.2.0-beta**, net8/netstd2.1 | new/beta (0.2.0-beta 2025) | **SDF+voxel material-removal sim, 3-axis AND 5-axis** (`ToolOrientation` A/B/C), stock config, mesh export | github.com/nyarurato/MillSimSharp; libraries.io/nuget/MillSimSharp |
| 2 (native mature) | CAMotics (Cauldron) | **GPLv2+** | NAT C++ app+libs (cbang) | mature | 3-axis material-removal sim, LinuxCNC O-codes, STL export | camotics.org; packages.fedoraproject.org/pkgs/camotics |

**Verdict:** `Toolpath/guard` approximates collision with a **2D swept-envelope only** (`MinkowskiSum` disc-along-segment) — no true 3D material-removal/dexel/voxel verification, no gouge against the actual removed-stock state. MillSimSharp (MIT, managed, 5-axis SDF, on NuGet) is the categorical-best to elevate verification from swept-envelope-approximation to real voxel material-removal — INTEGRATION opportunity (beta maturity is the caveat). CAMotics is mature but GPL+native+app.

### CONCERN 5 — FFF/MSLA slicing engine
| rank | package | lic | lang/reach | maturity | binding surface | sources |
|---|---|---|---|---|---|---|
| **1 (managed best)** | **gsSlicer** (gradientspace) | **MIT** | MGD C#; GitHub-source | functional, dormant | mesh→gcode: shells+infill+**support**, overlapping-shell/open-sheet handling; **composes geometry3Sharp + Clipper (both admitted-concept)** | gradientspace.com/opensource; cotangent.io/open-source |
| 2 (native, FFF+MSLA) | PrusaSlicer / libslic3r | **AGPLv3** | NAT C++; CLI | very active (2.9.x 2025-2026) | FFF + **MSLA/SLA (resin)**, organic supports, variable layer | github.com/prusa3d/PrusaSlicer |
| 3 (native, FFF Arachne) | CuraEngine | **AGPLv3** | NAT C++; CLI + libArcus (protobuf command-socket IPC) | very active | Arachne variable-width walls, tree supports | github.com/Ultimaker/CuraEngine; hackaday.io/page/3745 |

**Verdict:** the `Toolpath/slicing` claim "no managed slicer exists on NuGet" (slicing.md:3) is technically true (gsSlicer is GitHub-source) but **gsSlicer PROVES the exact managed author-kernel approach** (planar section + shells + Clipper infill over a g3 mesh) and carries **support generation + overlapping-shell/open-sheet handling the corpus author-kernel lacks** — reference/enrich, not a NEVER. The mature engines (CuraEngine, PrusaSlicer/libslic3r, Slic3r/SuperSlicer/Orca) are **uniformly AGPLv3 + native C++ + CLI/IPC-only** — this validates the author-kernel posture AND flags that any out-of-process slicer companion carries **AGPL network-copyleft** (charter risk to document). Resin/powder (MSLA) exists only in PrusaSlicer (AGPL) or in the already-pinned **PicoGK** grayscale layer-stack — see Concern 9.

### CONCERN 6 — straight skeleton / medial axis / weighted offset — corpus author-kernels
| rank | package | lic | lang/reach | maturity | binding surface | sources |
|---|---|---|---|---|---|---|
| **1 (best, native)** | **CGAL `Straight_skeleton_2`** | **GPL / commercial** | NAT C++; COMPAS-CGAL=py only, NO .NET | very active (6.2, 2025) | **weighted straight skeletons (5.6+, per-edge speed) + skeleton extrusion** — exactly the corpus "weighted skeleton" growth row | doc.cgal.org/latest/Straight_skeleton_2; cgal.org/2023/05/09 |
| 2 (managed approx) | NetTopologySuite | BSD/LGPL | MGD C#; NuGet | very active | Voronoi/Delaunay/buffer/DE-9IM; medial-axis-via-Voronoi possible but **float-precision model** (inferior to corpus exact-`Orient2D`+Clipper2) | nuget.org NetTopologySuite; gist medial-axis |
| 3 (native) | STALGO (S. Huber) | (research) | NAT C++ | stable | industrial straight-skeleton + mitered offsets | en.wikipedia.org/Straight_skeleton |

**Verdict:** the author-kernel `Toolpath/skeleton` is **CORRECT and validated** — the corpus rationale ("CGAL is C++/GPL, per-RID burden") holds exactly. CGAL now ships the WEIGHTED straight skeleton the corpus lists as growth (skeleton.md:19), but GPL/C++/no-.NET-binding keeps it author-kernel. NTS is the mature managed geometry lib but its float precision loses to the corpus's exact-predicate posture.

### CONCERN 7 — machine connectivity (OPC-UA / MTConnect agent / GRBL / FOCAS) — AppHost/transport, not Fabrication-core
| rank | package | lic | lang/reach | maturity | binding surface | sources |
|---|---|---|---|---|---|---|
| OPC-UA | OPCFoundation.NetStandard.Opc.Ua | **GPL-2 / RCL dual** | MGD; NuGet 1.5.378 net10 | certified, very active | client/server/PubSub/GDS | github.com/OPCFoundation/UA-.NETStandard; opcfoundation.org |
| MTConnect agent/transport | MTConnect.NET (TrakHound) | MIT | MGD | active | HTTP/MQTT/SHDR/Agent — the transport half of the already-pinned model-only slice | (roster) |
| **REJECTED** | Fanuc FOCAS (fwlib32) | **proprietary/distributor-gated**, Windows-only | NAT DLL | — | CNC state/offsets/alarms | inventcom.net/fanuc-focas-library; machinemetrics.com/focas |

**Verdict:** connectivity/telemetry is an **AppHost/transport concern the corpus already defers** (`magazine.md`: "the wire concern an Rasm.AppHost/transport leg"). Record as a forward-obligation to AppHost, not a Fabrication-core admission. FOCAS is license-gated → REJECTED.

### CONCERN 8 — GD&T / metrology / production specs (tolerances, surface finish, Cp/Cpk, setup sheets, travelers) — scope-mandate expansion
| rank | owner | lic | lang | note | sources |
|---|---|---|---|---|---|
| **NO OSS LIBRARY EXISTS** | — | — | — | concern = standards + statistics + open interchange schema | — |
| standard | ASME Y14.5-2018 / ISO GPS-1101 | (standard) | — | tolerance vocabulary (form/orientation/location/profile/runout, MMC/LMC) | asme.org; blackrock-engineering.ca |
| open interchange | **QIF** (Quality Information Framework, ISO 23952) | open XML schema | — | MBD/PMI/measurement/tolerance interchange — the schema to align a typed tolerance/inspection model against | (ISO 23952; colabsoftware.com) |
| statistics | **Math.NET Numerics** (already substrate) | MIT | MGD | Cp/Cpk, Gaussian, Monte-Carlo tolerance-stackup | (libs/csharp/.api/api-mathnet-numerics.md) |

**Verdict:** the scope-mandate "production specs" plane has **NO admissible OSS library** — it is an AUTHOR concern: a typed tolerance/surface-finish/process-capability receipt model + Math.NET statistics + QIF-aligned schema. A NEW author sub-domain, never a package admission.

### CONCERN 9 — additive supports / part-orientation / lattice-TPMS — scope-mandate expansion
| rank | package | lic | lang/reach | note | sources |
|---|---|---|---|---|---|
| **lattice/TPMS = 1 (best)** | **PicoGK** (LEAP71) | **Apache-2.0** | MGD net9 + native picogk.dylib (osx-arm64) | **ALREADY PINNED** — implicit/SDF gyroid/TPMS/lattice, grayscale MSLA/DLP layer-stack (resin/powder), OpenVDB I/O | (roster; api-picogk.md) |
| supports (tree/organic) | inside CuraEngine/PrusaSlicer (AGPL) | AGPL | NAT | no standalone OSS C# support-gen lib | github (slicers) |
| orientation opt | research only (no OSS lib) | — | — | saliency/energy-min support-reduction papers | orca.cardiff.ac.uk/183393 |

**Verdict:** PicoGK is the categorical-best OSS lattice/TPMS/resin owner AND is already pinned — but UNWIRED (see V1). Supports/orientation stay author-kernel (Slicing growth) — no admissible library.

### CONCERN 10 — already-admitted CORE substrates (sanity — keep)
Robots 2.1.2 (visose, robot FK/IK — well-chosen, keep) · geometry3Sharp (biarc — keep) · Clipper2 + CavalierContours (line/arc polygon algebra — keep) · ACadSharp (DXF/DWG read — keep) · MTConnect.NET-Common (ISO-13399 tool-data model — keep; **numeric feeds/speeds dataset remains a real gap — no free-full OSS SFM/chip-load dataset on NuGet**, corroborated: the corpus CuttingData table carries only 6 hand-entered cells). RectangleBinPack.CSharp + RectpackSharp (rect packing — keep; note true-shape NFP nesting OSS is SVGnest/Deepnest = JS+AGPL, so the author-kernel `Nesting/nfp` is correct).

---

## [B] PER-PAGE VERDICTS (ecosystem-core lens; 1-10)

- **Process/owner.md — 8.** Clean polymorphic `Fabrication`/`Run`/`FabricationInput` spine; the settled input-shape collapse is genuinely good. Defect: `FabricationInput` (owner.md:68-78) carries `Model`/`View` (HLR-only) beside CAM/nest fields — a projection concern welded into the CAM input carrier (see V6 folder split). Charter: keep as the dispatch owner; do NOT let production-spec/verification growth widen the union into a god-record.
- **Process/family.md — 9.** The strongest page: two-axis `Process`×`Machine` + `RemovalModality.Admits(CutStrategy)` cross-product is exemplary generator-over-enumeration. Ecosystem bearing: the 9-process/13-machine/8-strategy roster is DATA (correct), extensible to grind/broach/EDM-sink as rows. No defect of note.
- **Process/physics.md — 6.** Modality-discriminated `RemovalBudget` union is good. **Hardcoding-vs-generator defect:** `CuttingData` (physics.md:117-125) is a 6-cell hand-entered `FrozenDictionary`; `Overrides` (physics.md:114-115) is `FrozenDictionary...Empty` — a **dead carrier** (declared table, zero rows). Every material×tool×op cell outside the 6 falls back to the flat per-material `SurfaceSpeed` constant (physics.md:90-97, e.g. `MildSteel = ...Subtractive(90.0)`). No OSS feeds/speeds dataset exists to defeat this (Concern 10) → the gap is real; the fix is a QIF/CSV data-ingress arm, not a formula. Charter: keep the union; make the cutting-data a data-loaded table, delete the empty `Overrides` or fill it.
- **Process/magazine.md — 8.** ISO-13399 `MTConnect.CuttingToolAsset` composition is excellent, correctly model-only. Charter unchanged; note it is the one page that correctly consumes an admitted package to depth.
- **Process/faults.md — 8.** `FaultBand.Fabrication` 2700 + offset codes is clean and the census-closed re-band is honored. No ecosystem bearing.
- **Polygon/clipper.md — 7.** Dense and correct (Clipper2 line + CavalierContours arc, both admitted). **Split-pressure:** ONE 244-LOC page holds TWO distinct owners (`PolygonAlgebra` line-space + `ArcAlgebra` arc-space, two separate code fences, namespaces both `Geometry2D`) — a page doing double duty. Charter: two owners → two pages under a `Geometry2D/` lane.
- **Polygon/import.md — 8.** ACadSharp DXF/DWG ingress is thorough and ratified. **Concern-coverage gap:** import is 2D-profile only; the admitted OcctNet.Wrapper STEP/IGES 3D-solid ingress (README [SOLID_INGRESS]) has NO page (see V1). Charter: `import` is one arm of an ingress lane that must also home the STEP/IGES owner.
- **Toolpath/motion.md — 6.** Good `(RemovalModality,CutStrategy)` dispatch. **Defect (approximation-as-real):** `Turn()` (motion.md:76-79) models a lathe `radial-sweep` as `v.Y - radius - k*stepOver` per-vertex — a naive Y-offset, not a real Z-vs-radius diameter profile; `Plunge`/`Peck` emit a centroid + one move (motion.md:81-84,105-108) — placeholder depth. **Coverage gap:** 2.5D only, no 3D-surface strategy (Concern 3, V2). Charter: the strategy axis is right; the turning/3D generators are stubs needing real profile kinematics or an OpenCAMLib-bound arm.
- **Toolpath/skeleton.md — 9.** Author-kernel validated (Concern 6). Correct posture, correct rationale. Weighted-skeleton growth is real (CGAL has it, GPL). No defect.
- **Toolpath/slicing.md — 6.** Correct planar-section author-kernel. **Overstated seal:** slicing.md:3 "no managed slicer exists on NuGet" — gsSlicer (MIT, managed) exists (GitHub-source) and carries supports/overlapping-shell handling this page lacks (Concern 5, V4). **Unwired:** the additive resin/powder path the scope-mandate wants routes to PicoGK (pinned, unwired — V1); this page only does FFF planar + 4 infill patterns. Charter: FFF planar owner is right; enrich fill/supports from gsSlicer prior art; home the resin/powder path onto PicoGK.
- **Toolpath/kinematics.md — 9.** `Robots` (visose) composition with the RhinoCommon↔Rhino3dm `extern alias` boundary-map is exemplary; the `Collision NotSupportedException` firewall is correctly routed to Guard. No ecosystem defect.
- **Toolpath/guard.md — 6.** Correct swept-envelope + BVH broad-phase + skeleton-reach. **Depth gap:** verification is 2D swept-envelope only — no true material-removal/voxel state, so gouge-against-actual-removed-stock is not modeled (Concern 4). MillSimSharp (MIT, 5-axis SDF) is the categorical-best elevation (V5). Charter: keep the fast envelope pre-pass; add a voxel material-removal verification arm.
- **Toolpath/probing.md — n/a (QUEUED, IDEAS).** Anchors exist; fine.
- **Nesting/nfp.md — 7.** True-shape NFP author-kernel is correct (OSS true-shape nesters = SVGnest/Deepnest = JS+AGPL, inadmissible). Dense but the genetic/bottom-left placement is heuristic-standard. No ecosystem defect; keep.
- **Nesting/stock.md — 8.** `StockNest.Pack` over RectangleBinPack with the 5-packer→1-`NestStrategy` collapse is strong. Keep.
- **Nesting/workholding.md — 7 (mis-homed).** Content is good (keep-out over Clipper2, `HoldingClass`-keyed footprint). **Folder-architecture defect:** it is FIXTURING, not nesting — namespace `Rasm.Fabrication.Fixturing` (workholding.md:34), consumed by `guard.md` as `Rasm.Fabrication.Fixturing`; homing it under `Nesting/` is a mis-partition (V6).
- **Posting/program.md — 7.** Dense, correct dialect-neutral `GWord` AST + `PostDialect` Emit; validated (no admissible managed post library, Concern 2). **Grammar gap (V3):** all 8 dialects RS-274 word-address; no Heidenhain Klartext (non-ISO), no Siemens CYCLE800/TRAORI, no canned cycles (G81/G83/G76), no Fanuc macro-B. Charter: the AST is right for ISO-6983; a conversational-dialect arm needs an emit-grammar generalization, not a 9th word-address row.
- **Posting/projection.md — 7 (mis-homed).** BSP HLR + arrangement-seam silhouette is good. **Folder-architecture defect:** HLR is a DRAFTING/render-feed concern (consumer = AppUi `Viewport2D`, namespace `Rasm.Fabrication.Projection`), NOT NC-code "posting" (consumer = controller) — two unrelated downstreams share `Posting/` (V6).

---

## [C] CROSS-CUTTING FINDINGS

**[C1] FOUR admitted packages are UNWIRED — zero consuming pages (illusory depth).** `rg` over all 17 pages: **PicoGK, OcctNet.Wrapper, SharpVoronoiLib, DSTV.Net** appear in `.csproj` (lines 16,19,20,24) + README + ARCHITECTURE + `.api/` but in NO code fence or `Packages:` line of any page. Per the brief's INTEGRATION-FIRST law these are integration MANDATES (each → a named page/row/arm), NOT removal candidates, and each maps onto a scope-mandate expansion: PicoGK→resin/powder+lattice/TPMS, OcctNet→STEP/IGES production-geometry ingress, SharpVoronoiLib→Voronoi toolpath partitioning/stipple/spiral-seed, DSTV.Net→steel NC1 output beside the G-code AST. This is the single largest structural defect — the README/ARCHITECTURE prose describes capability the design pages never realize. → V1.

**[C2] Prose-vs-fence split — README/ARCHITECTURE over-claim.** README [IMPLICIT_VOXEL]/[SOLID_INGRESS]/[VORONOI_TESSELLATION]/[STEEL_FABRICATION_EXCHANGE] describe rich capability (PicoGK SLA/DLP layer-stack, OcctNet OcctShape B-rep, SharpVoronoiLib Fortune+Lloyd, DSTV NC1) with NO page realizing it. The `.api/` tier catalogs them (14 catalogs incl. api-picogk/occtnet/sharpvoronoilib/dstv-net) — so the evidence exists but is unconsumed. The gap is capability-realization, not roster.

**[C3] Coverage naivety — the CAM plane is 2.5D-only under a production-grade banner.** No 3D-surface machining (drop-cutter/waterline/pencil), no rest-machining, no true multi-axis toolpath (kinematics solves 6-DOF robot poses but `motion` generates only 2.5D+turning-stub geometry), no NC-verification beyond a 2D swept envelope. The scope-mandate ("multi-axis strategies, rest machining, tool-engagement control") is not met by the current fences. OpenCAMLib (LGPL) + MillSimSharp (MIT) are the categorical-best fills.

**[C4] Approach naivety — stub generators masquerading as strategies.** `motion.Turn` (v.Y-offset), `motion.Plunge`/`Peck` (centroid+1 move) are placeholders, not real removal geometry. `physics.Overrides` is an empty dead table; `CuttingData` is 6 hand cells with a flat-constant fallback (no OSS dataset defeats it).

**[C5] Folder-architecture rot.** (a) `Nesting/workholding` is FIXTURING (namespace `.Fixturing`) mis-homed under nesting. (b) `Posting/projection` is DRAFTING/render-feed (consumer AppUi, namespace `.Projection`) mis-homed under NC-posting. (c) `Polygon/clipper` holds two owners in one page. (d) **Namespace-vs-folder drift is already pervasive:** the 5-folder codemap does not match the code namespaces — `slicing`→`.Additive`, `family`/`magazine`→`.ProcessModel`, `physics`→`.ProcessPhysics`, `workholding`→`.Fixturing`, `projection`→`.Projection`, `clipper`/`import`→`.Geometry2D`. The `.planning/` folder tree and the realized namespaces have diverged. → V6.

**[C6] Concern mixing at the owner.** `FabricationInput` fuses HLR inputs (`Model`,`View`,`Watertight`) with CAM/nest/robot/post inputs — one record spanning drafting + machining + nesting. The three `FabricationPolicy` cases (`HiddenLine`/`Cam`/`Nest`) already diverge in consumer (AppUi vs controller vs procurement); the union is correct but the shared input record is a mixing point that production-spec/verification growth will strain.

**[C7] Unmined admitted-substrate capability (catalog anchors).** Math.NET Numerics (`api-mathnet-numerics.md`, shared substrate) is the natural Cp/Cpk/Monte-Carlo owner for the production-specs plane but is not referenced by any Fabrication page. QuikGraph (`api-quikgraph.md`) could own the tool-change/setup sequencing graph the magazine consolidation hand-rolls. Neither is wired.

---

## [D] VERDICT CANDIDATES (strongest, evidence-first)

**V1 — FOUR admitted+catalogued+README'd packages have ZERO consuming design pages; each is an integration MANDATE mapping onto a scope-mandate expansion.** Evidence: `rg` over all 17 `.planning` pages returns no hit for PicoGK/Voxels/IImplicit, occt/ImportStep/OcctShape, voronoi/Fortune, or dstv/nc1; all four are in `.csproj` (16,19,20,24), README [02]-[DOMAIN_PACKAGES], and `.api/`. Ruling: build PicoGK→additive resin/powder+lattice page, OcctNet→STEP/IGES ingress page (beside `Polygon/import`), SharpVoronoiLib→Voronoi-partition toolpath arm (on `motion`), DSTV.Net→steel NC1 emit arm (beside `Posting/program`). This closes the illusory-depth gap and realizes four scope-mandate planes with ALREADY-PINNED packages — highest ROI structural ruling.

**V2 — The CAM plane is 2.5D-only; admit a 3D-surface toolpath owner (OpenCAMLib, LGPL) via P/Invoke and elevate `motion` stubs.** Evidence: `motion.md` cases are boundary-pass/pocket/adaptive (Clipper2+skeleton, all 2.5D) + turning/plunge stubs (motion.md:76-108); no drop-cutter/waterline/pencil/rest. OpenCAMLib is LGPL-2.1 (relicensed 2018-08, repo-confirmed), active (2025-02), precompiled osx-arm64 — admissible and bindable exactly as OcctNet is bound. Ruling: a `Toolpath/surface` 3D-CAM owner (P/Invoke `libocl`, content-keyed, kernel-mapped at the seam) closes the "full CNC/CAM depth / rest machining" mandate the current fences do not meet; FreeCAD `Adaptive2d` (LGPL) is the reference to validate the existing adaptive author-kernel against.

**V3 — The `PostDialect` AST cannot express non-ISO conversational grammars; the scope-mandate's "controller families beyond the current AST + canned cycles + macro programming" needs an emit-grammar generalization, not more word-address rows.** Evidence: all 8 dialects (program.md:40-65, family.md:80-96) are RS-274/ISO-6983 word-address + Marlin; Heidenhain Klartext is non-ISO (L/C/CC, BEGIN/END PGM, TOOL CALL, no G-words — corroborated cam232.com, steptools Generate styles); no canned-cycle (G81/G83/G76) or Fanuc macro-B (`#vars`) rows exist (the AST emits expanded moves). No admissible managed post library exists (E3Studio=app, PostProcessor=python, StepTools/ModuleWorks=proprietary) → the author-kernel is validated but must generalize its Emit grammar (a `DialectGrammar` axis: word-address vs conversational vs canned-cycle) rather than assume RS-274.

**V4 — Retract the `slicing.md:3` "no managed slicer exists" seal; gsSlicer (MIT, managed, composes admitted geometry3Sharp+Clipper) is prior art carrying supports + overlapping-shell handling the author-kernel lacks.** Evidence: gradientspace.com/opensource + cotangent.io confirm gsSlicer (MIT, C#, mesh→gcode, shells+infill+support, open-sheet meshes) built on geometry3Sharp+Clipper. Ruling: the author-kernel posture survives (gsSlicer is GitHub-source not NuGet, and the mature engines CuraEngine/PrusaSlicer/Slic3r are uniformly AGPLv3+native+CLI — a real charter-relevant finding) BUT the seal is a challengeable design pressure: enrich the FFF author-kernel (supports, variable-width/Arachne, adaptive layers) from gsSlicer prior art rather than re-deriving thin.

**V5 — `Toolpath/guard` verification is a 2D swept-envelope approximation; admit MillSimSharp (MIT, 5-axis SDF/voxel) to reach true material-removal verification.** Evidence: guard.md `Sweep` is a `MinkowskiSum` disc-along-segment (guard.md:59-67) tested by polygon `Clip` — no removed-stock state, so a gouge against actual in-process material is unmodeled. MillSimSharp is MIT, NuGet 0.2.0-beta, SDF+voxel, 3+5-axis (`ToolOrientation` A/B/C), mesh export (github.com/nyarurato/MillSimSharp; libraries.io). Ruling: keep the fast swept-envelope pre-pass; add a voxel material-removal verification arm (MillSimSharp) — the "NC-verification" plane the scope-mandate names. Beta maturity is the caveat to weigh vs the GPL native alternative (CAMotics).

**V6 — Folder-architecture re-partition: `workholding` (fixturing) and `projection` (drafting) are mis-homed, `clipper` doubles two owners, and the folder tree has drifted from the realized namespaces.** Evidence: workholding.md:34 namespace `Rasm.Fabrication.Fixturing` under `Nesting/`; projection.md:37 namespace `.Projection` under `Posting/` with consumer AppUi `Viewport2D`; clipper.md carries two owners/fences; namespaces `.Additive/.ProcessModel/.ProcessPhysics/.Fixturing/.Projection/.Geometry2D` do not match the 5-folder codemap. Ruling: re-partition into growth lanes that match namespaces and consumers — e.g. `Process/` (owner+family+faults spine), `ProcessData/` (physics+magazine tool-data), `Geometry2D/` (clipper+arc+import ingress incl. STEP/IGES), `Toolpath/` (2.5D+3D-surface+additive+kinematics), `Verification/` (guard+material-removal sim), `Fixturing/` (workholding), `Nesting/` (nfp+stock), `Posting/` (G-code+DSTV NC1 emit), `Drafting/` (HLR projection), and a NEW `Specs/` (tolerances/surface-finish/Cp-Cpk/setup-sheets/travelers). No loose one-file-one-folder combos; every lane a growth axis.

**V7 — "Production specs" (tolerances, surface finish, process capability, setup sheets, shop travelers) has NO admissible OSS library; it is a NEW author sub-domain over Math.NET statistics + QIF-aligned schema.** Evidence: web sweep found only standards (ASME Y14.5-2018, ISO GPS-1101), open interchange (QIF/ISO 23952), and commercial tools (Verisurf/Hexagon) — zero OSS GD&T/metrology library. Math.NET Numerics (already substrate, `api-mathnet-numerics.md`) owns Cp/Cpk/Monte-Carlo tolerance-stackup. Ruling: author a typed tolerance/surface-finish/process-capability receipt model (QIF-aligned) + Math.NET statistics arm; do NOT search for a package that does not exist.

**V8 — Numeric feeds/speeds data is a real, OSS-unfillable gap; the `CuttingData` table and empty `Overrides` are the honest floor, not a defect to hide.** Evidence: physics.md:114-125 (`Overrides` = Empty dead table; `CuttingData` = 6 hand cells; flat per-material `SurfaceSpeed` fallback). Web sweep confirms no free-full OSS SFM/chip-load dataset (ISO-13399 is geometry, not cutting data; MTConnect models the asset, not recommended parameters). Ruling: keep MTConnect-model tool geometry; make cutting-data a data-INGRESS arm (CSV/QIF loadable), delete or fill the empty `Overrides`, and record the numeric-dataset gap as an accepted external-data dependency — never a formula pretending to be measured.
