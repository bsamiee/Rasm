# DOSSIER — process-posting lane (Rasm.Fabrication design survey)

Lane: `Process/{owner,family,physics,faults,magazine}` + `Posting/{program,projection}` + governance (README/ARCHITECTURE/TASKLOG/IDEAS/csproj). Read-only. Every anchor re-verified on disk (`sed`/`rg`/word-boundary grep). Verdict keys: HOLD (anchor exact), DRIFT (defect real, anchor moved/expanded), REFUTED (disk falsifies).

Bottom line: the seven interiors are individually competent but the lane is a **subtractive-milling engine wearing 9-process clothing** — the post is a two-column render, the arc rail is unbuilt in the emitter, the magazine→post seam is fiction, the HLR is not CAD-grade and its own robustness floor is a phantom import, physics fragments one material into modality rows, and the folder hides four realized namespaces under `Process/`+`Posting/`. Every EVERY register row assigned VERIFIED; two register **misses** surfaced (E13 undercounts RESEARCH tails; a content-key law breach with no E-row).

---

## [00]-[NAMESPACE_CENSUS] — 5 of 7 pages are folder≠namespace

`rg '^namespace'` per page vs folder basename:

| Page | Folder | Declared namespace | Verdict |
|---|---|---|---|
| owner.md | `Process/` | `Rasm.Fabrication.Process` | leaf-match — **but collides with a TYPE** (below) |
| family.md | `Process/` | `Rasm.Fabrication.ProcessModel` | MISMATCH |
| physics.md | `Process/` | `Rasm.Fabrication.ProcessPhysics` | MISMATCH |
| faults.md | `Process/` | `Rasm.Fabrication` (root) | MISMATCH |
| magazine.md | `Process/` | `Rasm.Fabrication.ProcessModel` | MISMATCH |
| program.md | `Posting/` | `Rasm.Fabrication.Posting` | match |
| projection.md | `Posting/` | `Rasm.Fabrication.Projection` | MISMATCH |

Confirms the brief `[V1]` realized-namespace schism directly on my pages: `Process/` hosts THREE namespaces (`Process`, `ProcessModel`, `ProcessPhysics`, root) and `Posting/` hosts TWO (`Posting`, `Projection`). The brief's six V1 ratifications (`family`/`magazine`→Tooling/Process, `physics`→Process, `faults`→Process, `projection`→Documentation) are all evidenced here.

**HIGH structural hazard the DECISION must rule — namespace/type collision.** `owner.md:39` declares `namespace Rasm.Fabrication.Process`; `family.md:98` declares `public sealed partial class Process` (the `[SmartEnum<string>]`). The recommended V1 floor keeps owner AND moves family both INTO `Process/`, so both would share `namespace Rasm.Fabrication.Process` and the type resolves as `Rasm.Fabrication.Process.Process`. Today the hazard is already live: `program.md:28` does `using Rasm.Fabrication.Process;` (owner atoms) AND `program.md:30` `using Rasm.Fabrication.ProcessModel;` (the `Process` type), then references bare `Process` at `:142,143,150` (`input.Process`, `Resolve(..., Process process)`). From inside a `Rasm.Fabrication.*` namespace the nested namespace `Process` is directly visible as `Process` and competes with the type — a latent `CS0118`-class ambiguity never compile-proven. The DECISION must disambiguate (rename the namespace segment, e.g. `ProcessOwner`/`Fabrication` root for the owner, OR rename the SmartEnum) before folder=namespace ratifies `Process/`.

---

## [01]-[PER_PAGE_VERDICTS]

### Process/owner.md — the entry (brief 9; hold + grow)

Settled polymorphic `Fabrication.Run` over `FabricationPolicy` `[Union]` (`HiddenLine`/`Cam`/`Nest`) → `FabricationResult` (`HiddenLineResult`/`Motion`/`Placement`) via one generated total `Switch`. `FabricationInput` (`:68-78`) is the settled ONE-input carrier. This is the campaign spine and is genuinely sound.

Verified defects (register + beyond):
- **E3 owner.md:42 HOLD** — `public sealed record Loop(Arr<Point3d> Vertices, bool Closed)`; `rg Bulge Process/owner.md` → NONE. No `Bulges` column, no 3-arg ctor, no `BulgeAt`. The arc rail's widened `Loop` was never landed by the atom owner, so `clipper.md:257,271`'s `BulgeAt(i)` calls have no target.
- **E8 owner.md:85 HOLD** — `Cam(CutStrategy Strategy, double StepOver, double ToolRadius, int Passes, CellPolicy Cell, EngagementPolicy Engagement)`; `ToolRadius` is a bare `double`, must re-frame to the `[V5]` `CutterForm`-row reference.
- **SEAM inversion (beyond register).** `FabricationInput.View` is `ProjectionDir` (`owner.md:70`), a type DECLARED downstream in `projection.md:42` (`namespace Projection`). `owner.md:30` imports `Rasm.Fabrication.Projection` to reference it. The upstream atom carrier (`owner#atoms`) depends on a downstream plane's declaration — a backward atom→plane edge. Under the V1 two-node rule `ProjectionDir` should mint on `owner#atoms`.
- **Content-key breach (beyond register).** `owner.md:3` "It computes no hash"; `FabricationResult` cases carry no content key. Per `SEAM_AND_ENTRY_LAW` the machine-consumable egress artifacts must be content-keyed through `ContentHash.Of`. The `[V7]` `ResidualStock`/per-setup-snapshot vocabulary the brief mandates on `owner#atoms` is entirely absent (0 hits).

Charter-as-it-should-be: `owner#atoms` mints `Loop`+`Bulges` (0/tan(θ/4) column, 3-arg ctor, `BulgeAt`), `ProjectionDir`, `ResidualStock`, per-setup `StockSnapshot`, and the content-key seed on egress results; `owner#run` stays the terminal fold, growing `Additive`/`Verify`/`Inspect` policy+result cases. Physically split into two nodes only if the seam ledger fails the two-node discipline.

### Process/family.md — the axes (brief 9; REBUILD: axis widening)

The de-hardcode exemplar: `Process`/`Machine`/`RemovalModality`/`KinematicClass`/`HoldingClass`/`PostDialect`/`CutStrategy` `[SmartEnum]` axes with constructor-bound behavior columns and `Admits` row-relations. Correct SHAPE, thin COVERAGE.

Verified defects:
- **E4 family.md:80-96 HOLD** — `PostDialect` carries three render columns only (`Rs274` `:90`, `Comment` `:91`, `LineNumbers` `:92`). No canned-cycle grammar, macro/subprogram convention, WCS roster, cutter-comp admission, arc-mode, block-cap, or per-dialect code-override column.
- **E4 family.md:94-95 HOLD** — `Admits(RemovalModality) => Rs274 ? modality != Additive : modality == Additive;` a binary render, so a Fanuc post and a Haas post are indistinguishable at the family axis.
- **E8 family.md:100,118 HOLD** — `Process.Mill` (`:100`) binds `KinematicClass.CartesianGantry`; `Machine.Mill5Axis` (`:118`) admits `{Mill, Route}`, so `mill-5axis` binds `CartesianGantry` transitively. `KinematicClass` has 4 flat rows (`:62-65` cartesian-gantry/rotary-spindle/articulated-arm/delta-parallel) — no rotary topology (trunnion/head-head/head-table/nutating), so the 5-axis gap is structural.
- **APPROACH naivety (beyond register).** The eight `CutStrategy` rows are a fixed seed with no dimensionality column (2.5D|3D-surface|multi-axis) and no axis-count admission; production CAM (waterline/scallop/pencil/rest/3+2/swarf/thread-mill/drill-family) is unexpressible as rows.

Charter-as-it-should-be: `PostDialect` grows GRAMMAR-family capability columns (family, canned-cycle grammar, macro/subprogram convention, WCS roster, cutter-comp, arc mode, block cap, decimal/modal policy, code-override); `CutStrategy` gains a dimensionality column; `KinematicClass` deepens to rotary-topology rows; `Process` grows `additive`-modality budget rows. Data-vs-lowering split: the widened axis stays family's, the emission folds are `Posting/dialect`'s.

### Process/physics.md — the physics (brief 6→9; REBUILD: identity map)

`RemovalParameter` projects `(Process, Material, Tool, Operation)` → modality-discriminated `RemovalBudget`. The `[TOOL_CUTTING_DATA_TABLE]` landed (`CuttingData` `:117-125`). The physics-identity model is the weak core.

Verified defects (E6 all HOLD):
- **physics.md:99** — `Material` carries ONE `ModalityPhysics Physics` (single case per row), not a `Map<RemovalModality, ModalityPhysics>`.
- **physics.md:91-97** — `stainless` (`:92` Subtractive 45.0) and `stainless-abrasive` (`:96` Abrasive) are TWO Material rows for one physical material; `mild-steel` (`:91`)/`mild-steel-thermal` (`:95`) likewise. Directly against the page's own anti-flat-record prose (`:3`).
- **physics.md:136** — `erosion: static s => ThermalBudget(s.material, s.tool)` — EDM conflated onto the thermal budget; no `RemovalBudget.Erosion` case exists.
- **physics.md:109** — `RemovalBudget.Additive(ExtrusionWidth, LayerHeight, PrintSpeed, MeltTemp)` is FFF-only; no Resin/Powder cases.
- **physics.md:114-115** — `Overrides = FrozenDictionary<...>.Empty` dead carrier (never populated).
- **physics.md:141** — `... : 90.0, operation.ChipLoad)` the hardcoded surface-speed fallback survives behind the table.
- **E9-adjacent (Kienzle) — beyond my page but rooted here.** `CuttingData` `:117-125` is honest seed data but `program.md:183`'s `0.0012` force coefficient belongs on a `kc` machinability column the brief homes to `Tooling/cuttingdata` (`[V9]`).

Charter-as-it-should-be: ONE `Material` identity carrying `Map<RemovalModality, ModalityPhysics>`; `Budget` modality-dispatches into the map; `RemovalBudget.Erosion` (+ `Resin`/`Powder`) land; `CuttingData` moves to `Tooling/cuttingdata` and deepens to Kienzle `kc`; the dead `Overrides` deletes onto a data-ingress arm; `90.0` demotes behind the table.

### Process/faults.md — the rail (brief 9; hold + payloads)

`FabricationFault` `[Union]` on `FaultBand.Fabrication = 2700`, offsets +1..+10 (Nest at +10, `:52-61`). The band is landed and correct (matches archived DECISION 2701-2710). Composes `Rasm.Element` FaultBand registry (`:26` — legal substrate edge).

Verified defect (beyond register — brief `SEAM_AND_ENTRY_LAW` "typed payload, never bare `string Detail`"):
- **All 10 arms carry bare `string Detail` (`:39-48`).** `Gouge`, `Collision`, `InadmissiblePair`, `NoFit` etc. must carry typed payloads (`Gouge`→point+tool, `Collision`→zone, `InadmissiblePair`→the typed pair, `NoFit`→part+tried rotations). No typed-payload upgrade is present.
- New-concern growth room: `:19` names the next offset `+11` — correct, and the `[V7]`/`[V8]` planes need arms here (probe-overtravel, capability-gate, verify gouge/uncut/overcut/air-cut).

Charter-as-it-should-be: keep the 2700 band + offset discipline; retype every arm's `string Detail` to its typed payload; add the new-plane arms at `+11..` sized by the growth law.

### Process/magazine.md — Tooling (brief 8.5; MOVE+wire)

Genuinely deep `MTConnect.NET-Common` `CuttingToolAsset` composition (`ToolAssembly`, `Schedule` life-split, `HolderEnvelope`). The ONE admitted package on my lane that is actually mined. Two defects:

- **E2 pipeline-island (magazine→program half) — HOLD.** `magazine.md:18` claims "`Posting/program` `Post` emits each `ToolChange` as the `G43`/`M6`/`Tnn` block sequence"; `:22` repeats it; the mermaid `:187` draws it. **Refuted downstream by disk**: `rg 'Magazine|ToolMagazine|Schedule' Posting/program.md` returns ONLY the `GCommand.ToolChange`/`LengthOffset` row declarations (`program.md:61,62`) and the Cases prose (`:14`). NO fold in `program.md:141-164` (or anywhere) consumes `ToolMagazine.Schedule`, and the emitted-GCommand scan proves `GCommand.ToolChange`/`GCommand.LengthOffset` are NEVER referenced in any fold body. The seam is one-directional prose.
- **E12 magazine.md:160-161 HOLD** — `Runout(asset) => ...OfType<ShankDiameterMeasurement>().HeadOrNone().Map(static m => 0.01).IfNone(0.005)` — maps the measurement to a literal `0.01`/`0.005`, ignoring its value.
- **E8 magazine.md:5,161 HOLD** — `ToolAssembly` carries the ISO-13399 measurement set via `Asset` but NO typed `CutterForm` axis (flat/ball/bull/taper/drill/chamfer/thread-mill). The four named consumers (`surface`/`Verify/removal`/`guard`/`cuttingdata`) have no form vocabulary to read.
- **Two-digest tension (beyond register).** `magazine.md:5` "one content-addressing discipline, two digests"; `:84,119` call `Asset.GenerateHash(false)`. Per `[04]` MTConnect row the `GenerateHash` must reconcile with the kernel `ContentHash.Of`, never a second mint.

Charter-as-it-should-be: MOVE to `Tooling/` (namespace `Tooling`); WIRE the schedule into `Post` (the tool-change fold produces real `M6`/`G43`/`Tnn` blocks at interval boundaries); project a typed `CutterForm` axis from the ISO-13399 measurement set; `Runout` reads the real measurement; reconcile `GenerateHash` onto the one `ContentHash.Of` seam.

### Posting/program.md — the emitter (brief 6→9.5; REBUILD: AST growth + content key)

Rich cut-conditioning author-kernel (`Kerf`/`Simplify`/`Compensate`/`Biarc`/`Feedrate`/`Lookahead`/`Sequence`) — but a **two-column render wearing eight dialects**, an unbuilt arc rail, and a phantom AST.

Verified defects:
- **E4 program.md:122-136 HOLD** — `Emit()` varies output by `Dialect.LineNumbers` alone (`:125,133`); the `Dialect.Comment` column is NEVER read (dead). A Fanuc post and a Haas post emit near-identically.
- **E4 program.md:41-65 HOLD** — dialect-invariant `GCommand.Code`: `thread-cycle→"G92"` (`:51`, Fanuc-lathe-specific, wrong as threading on a Haas mill where G92 is coordinate-set); `Dwell`/`Pierce` both `"G4"` (`:46,54`); `Feed`/`Extrude` both `"G1"` (`:43,58`). Zero canned-cycle (G81-G89/G76), macro, subprogram (M98/M99), WCS (G54-G59), cutter-comp (G41/G42), or arc-mode (R-vs-IJK) nodes; `GWord` (`:118-119`) is a flat word-address block.
- **E3 program.md:247-282 HOLD** — `new BiArcFit2(...)` at `:268` still refits chords; reads no `Bulges`. The `clipper.md:5,33` retirement claim is unrealized in the emitter.
- **E3 program.md:215-223 HOLD** — hand-built lead arc: `center = from + 0.5*(to-from)` (`:220`) with manual `I`/`J` math (`:221-222`) — a chord-midpoint "arc", not a tangent lead-in.
- **E6 program.md:183 HOLD** — `double radialForce = 0.0012 * policy.RemovalRate;` magic force coefficient (belongs on a `kc` column, `[V9]`).
- **Content-key breach — HOLD (SEAM law, no E-row).** `program.md:3` "it computes no hash"; `CutProgram(Seq<GWord> Blocks, PostDialect Dialect)` (`:121`) has no content-key field. The `PBRIEF [V12]` `ArtifactKind` content-key law is unmet.
- **STUB-ARM census (beyond register) — 9 of 21 `GCommand` rows declared but NEVER emitted by any fold** (emitted-reference grep): `Dwell`, `Css`(G96), `ThreadCycle`(G92), `TorchHeight`(THC), `HotendTemp`(M104), `Extrude`(E-axis), `BedTemp`(M140), `ToolChange`(M6), `LengthOffset`(G43). The lathe family, the tool-change family, most additive, and thermal-height are phantom vocabulary — the multi-modality AST breadth is illusory.
- **E13 program.md:379 — DRIFT (register MISS).** `program.md` carries `## [03]-[RESEARCH]` (`:379-383`) that the brief E13 does NOT list (it names only kinematics/skeleton/slicing). The V2 acceptance `rg '\[RESEARCH\]' == zero` FAILS here.

Charter-as-it-should-be: `PostDialect`-keyed generated `Emit` `Switch` (grammar families, canned cycles as single blocks where admitted, macro/subprogram lowering, WCS render from `Fixturing/setups`, per-dialect code overrides); `GWord` grows canned-cycle/subprogram/macro nodes with typed R/Q/P slots; the arc rail reads `ArcAlgebra`/kernel arc-native offsets and `Loop.Bulges`, `g3.BiArcFit2` retained only for genuinely line-sourced chains; NC1 emit target on the one `Post` fold; a `Parse` round-trip arm with NIST modal-group state; `CutProgram` gains its content key; the tool-change fold consumes `ToolMagazine.Schedule`; purge the `[03]-[RESEARCH]` tail.

### Posting/projection.md — HLR (brief supersede per V3; MOVE to Documentation)

BSP front-to-back visibility + Clipper2 screen clip. `[CSG_WATERTIGHT_SILHOUETTE]` (`Arrangement.Apply`/`ToMesh` kept-cell arm) landed. But the interior is not CAD-grade AND its own robustness floor is a phantom.

Verified defects (E7 all HOLD):
- **projection.md:74-77** — `SideOf` classifies each facet by its CENTROID's plane side (`Math.Sign((c - plane.A) * plane.Normal)`, `:76`); a straddling facet is mis-assigned whole (no facet splitting) — the proof the BSP is not CAD-grade.
- **projection.md:100** — `Point3d eye = ... Point3d.Origin - 1e6 * input.View.Forward` magic eye distance.
- **projection.md:102,145-146** — `silhouette.Concat(MeshEdges(facets))` (`:102`) draws EVERY facet edge; `MeshEdges` (`:145-146`) returns all edges undeduped, no crease filter — triangulation soup, not silhouette+crease.
- **projection.md:150** — `double edgeDepth = (sa.Z + sb.Z) / 2.0` per-edge average depth; a partially-occluded edge is wholly classified.
- **PHANTOM member (beyond register — strengthens E7).** `projection.md:31` imports `Rasm.Geometry.Numerics` and the prose (`:3,16,18,20`) claims `Predicate.Orient2D` as "the silhouette view-dot sign floor" and names "a `double` cross-product at the call site the named robustness defect". `rg 'Predicate\.Orient2D' Posting/projection.md` → ZERO fence calls. The actual sign tests are raw doubles: `FacesViewer` (`:58` `Normal * forward < 0.0`), `SideOf` (`:76`), `Silhouette` (`:139` `Math.Sign(Normal * forward) != Math.Sign(...)`). The page commits exactly the defect its own prose forbids; the compose is a phantom import.

Charter-as-it-should-be: MOVE to `Documentation/` (namespace `Documentation`); the BSP interior dies for the kernel `DrawingProjection` (`[V3]`); KEEP the `BooleanSolid` watertight arm (`Arrangement.Apply`/`ToMesh` compose-seam); PRESERVE the `HiddenLineResult` receipt shape AppUi is insulated at; drop the phantom `Orient2D` import.

---

## [02]-[REGISTER_VERDICTS] — assigned rows re-verified on disk

| Row / anchor | Status | Evidence |
|---|---|---|
| **E1** dead admissions | HOLD | `CSPROJ:16,19,20,24` = DSTV.Net/OcctNet.Wrapper/PicoGK/SharpVoronoiLib pinned (brief prose lists a different order; set exact). Word-boundary grep over all 17 pages: PicoGK/OcctNet/OcctShape/OcctMesh/DSTV/SharpVoronoiLib/Voronoi/IImplicit/Voxels/TPMS/Lattice/CliIo/ImportStep = **0 consumers** (only "gyroid" appears, in `slicing.md` — the `[V6]` false-collapse, not a PicoGK consumer). README `[IMPLICIT_VOXEL]`/`[SOLID_INGRESS]`/`[STEEL_FABRICATION_EXCHANGE]`/`[VORONOI_TESSELLATION]` charters realized nowhere. |
| **E2** magazine→program (my half) | HOLD | `magazine.md:18,22` claim Post emits `G43`/`M6`/`Tnn`; `program.md:141-164` consumes no `Magazine`/`Schedule`; `GCommand.ToolChange`/`LengthOffset` (`program.md:61,62`) declared but never emitted by any fold. |
| **E2** ARCH:63 queued-as-owned | HOLD | `ARCH:63` (+`:30`,`:65`) present the multi-fixture-setup scheduler as owned `Nesting/workholding` capability; `IDEAS.md` `MULTI_FIXTURE_SCHEDULE` is `[QUEUED]`. |
| **E3** owner.md:42 Loop | HOLD | `Loop(Arr<Point3d> Vertices, bool Closed)`; no `Bulge*` anywhere in owner.md. |
| **E3** program.md:247-282 refit | HOLD | `new BiArcFit2(...)` at `:268`; reads no `Bulges`. |
| **E3** program.md:215-223 lead arc | HOLD | Hand-built midpoint-center arc `:220-222`. |
| **E4** family.md:80-96 / :94-95 | HOLD | `PostDialect` three columns (`Rs274`/`Comment`/`LineNumbers`); binary `Admits` `:94-95`. |
| **E4** program.md:122-136 / :41-65 | HOLD | `Emit` varies by `LineNumbers` only (`Comment` dead); `thread-cycle→G92`; `Dwell`/`Pierce`=G4; `Feed`/`Extrude`=G1; zero canned/macro/subprogram/WCS/cutter-comp/arc-mode nodes. |
| **E6** physics.md 99/91-97/136/109/114-115/141 | HOLD | Single `ModalityPhysics`; stainless+stainless-abrasive & mild-steel+mild-steel-thermal fragmented; erosion→ThermalBudget; FFF-only Additive; `Overrides=Empty`; `90.0` fallback — all lines exact. |
| **E6** program.md:183 | HOLD | `0.0012 * policy.RemovalRate`. |
| **E7** projection.md 74-77/100/102,145/150 | HOLD | Centroid BSP no split; `1e6` eye; undeduped mesh edges no crease filter; per-edge average depth — all exact. |
| **E8** coverage silences | HOLD | `family.md:100,118` mill-5axis→CartesianGantry; `KinematicClass` 4 rows; `owner.md:85` `Cam.ToolRadius` bare double; `magazine.md:5,161` no `CutterForm`; corpus grep: waterline/scallop/rest/NC-verify/spec/G-code-read = 0. |
| **E10** ARCH:16 | HOLD | `# FabricationFault band-2500` (stale; `faults.md`=2700). |
| **E10** ARCH:52 | HOLD | `netDxf in AppUi owns the distinct WRITE leg`. |
| **E10** ARCH:53,54 | HOLD | Both → `Rasm.Persistence/Schema` (deleted folder); `:54` also carries the `XxHash128` second-hasher spelling. |
| **E10** README:33,64 | HOLD | netDxf write-leg reference + `[REJECTED] netDxf` second-reader row. |
| **E10** TASKLOG:44 | HOLD | `[FABRICATION_FAULT_BAND_DEEPENING]` COMPLETE card carries `InadmissiblePair 2505/…/StockOverflow 2509`; superseded by `[FAULT_REBAND_2700]` at `:47`. |
| **E10** IDEAS:27 | HOLD | `[GUARD]-[QUEUED]` realized-but-open (`guard.md` landed; README:21; ARCH codemap `:25`). |
| **E11** physics.md:3 vs ARCH:49 | HOLD (real drift) | `ARCH:49` = `Rasm.Materials/Properties`; `physics.md:3` = `Rasm.Materials/physical-properties#MATERIAL_PROPERTY`. Two disagreeing anchors — reconcile which Materials page name is canonical. |
| **E11** ARCH:51 Compute rollup | HOLD | `Nesting/stock → Rasm.Compute NestYield.WasteAreaMm2` counterpart present (landed `CBRIEF [V12]`). |
| **E11** intra-package web undeclared | HOLD | `ARCH:[02]` carries only `stock→nfp` (`:50`) and `probing→posting` (`:57`); magazine→guard/workholding, magazine→program, program→physics/family/clipper/owner, projection→owner/clipper all absent. |
| **E12** magazine.md:160-161 | HOLD | `Runout` literal `0.01`/`0.005` stub. |
| **E12** README:65,70 | HOLD | `:65` rejects MaxRect/BinPack.NET "superseded by RectpackSharp" (while RBP subsumes RectpackSharp per `[04]`); `:70` confirms shared `libs/csharp/.api/` exists (falsifies `api-hashing.md`'s "no shared tier"). |
| **E13** program.md:379 | **DRIFT** | Register MISS: `program.md:379` `## [03]-[RESEARCH]` not in E13's list. Full census: `rg '\[RESEARCH\]'` → **5 pages** (`program.md:379`, `stock.md:317`, kinematics:122, skeleton:199, slicing:219). V2 acceptance requires purging 5, not 3. |
| **E14** upstream bindings (my-page consumption) | HOLD | Bindings exist upstream; UNMET on my pages — `projection` has not retired the BSP for kernel `DrawingProjection` (`[V3]`), `program` still refits via `g3` not the arc-native seam (`[V2]d`). Consumption is leg-2/leg-4 work. |

No REFUTED rows — every assigned register anchor is factually present on disk. The two DRIFTs are register-completeness gaps, not falsifications.

---

## [03]-[CONSUMER_CENSUS] — admitted packages each page composes (word-boundary)

| Page | Composed packages (real, in-fence) | Dead-admission consumers |
|---|---|---|
| owner | Rasm.Geometry.Numerics (`Predicate.Orient2D`), Rasm.Vectors, Rasm.Geometry, Thinktecture, LanguageExt | none |
| family | Rasm.Geometry (`GeometryFault`), Thinktecture, LanguageExt | none |
| physics | Thinktecture, LanguageExt, **UnitsNet** (`:31`), Rasm.Geometry; Rasm.Materials raw-double peer read | none |
| faults | **Rasm.Element** (`:26` FaultBand — legal substrate), Rasm.Geometry, Thinktecture, LanguageExt | none |
| magazine | **MTConnect.NET-Common** (`:28-29`, genuinely mined), UnitsNet, Clipper2-via-Geometry2D, Thinktecture, LanguageExt | none (MTConnect is not one of the four dead; it IS wired) |
| program | **geometry3Sharp/g3** (`:25`, `BiArcFit2`), Clipper2-via-Geometry2D, Rasm.Geometry.Numerics (`Predicate.Orient2D` REAL at `:334`), Thinktecture, LanguageExt | none |
| projection | Rasm.Geometry.Arrangement/Healing/Spatial, Clipper2-via-Geometry2D, Rhino Mesh, Rasm.Geometry.Numerics (**phantom** — imported `:31`, 0 fence calls) | none |

Confirms the brief's proof #1: the four INTEGRATION-MANDATE packages (PicoGK/OcctNet.Wrapper/DSTV.Net/SharpVoronoiLib) have zero consumers ANYWHERE, and none land on my seven pages. `program.md`'s `geometry3Sharp` is the roster's `REPLACE-candidate` (arc-rail retirement target). `magazine`'s MTConnect is the one deeply-mined admission on the lane.

---

## [04]-[SEAM_CENSUS] — cross-page + cross-package edges

Intra-package (my pages), NONE declared in `ARCH:[02]` except where noted:
- `owner#run → {projection.Hlr.Solve, motion.Cam.Solve, nfp.Nest.Solve}` (`owner.md:103-105`) — terminal fold.
- `owner#atoms → projection` (references `ProjectionDir`, `owner.md:70` — **backward inversion**).
- `family → {owner, physics, motion, program, magazine, workholding, kinematics}` (behavior columns) — undeclared.
- `physics → ProcessModel` (Process/Material/Tool/Operation), `→ Rasm.Materials/Properties` raw-double (ARCH:49, anchor-drifted).
- `faults → Rasm.Element` FaultBand (legal substrate; declared implicitly via import, not an ARCH seam row).
- `magazine → {physics(Tool), owner(Loop), Geometry2D(Offset)}`; read by `{guard, workholding}` (HolderEnvelope) and `program` (ToolChange — **fictional, E2**) — none declared.
- `program → {owner(Move/Loop/Result), family(PostDialect), physics(RemovalBudget via PostPolicy), Geometry2D(Kerf/Simplify), g3(Biarc)}` — none declared.
- `projection → {owner(Loop/Result), Geometry2D(ClipOpenPath), Arrangement, Healing, Spatial}` — cross-package half declared (`ARCH:42-47`), intra-package `→owner`/`→Geometry2D` not.

Cross-package (governance, mine): `ARCH:42-47` (projection ↔ Rasm/Numerics/Meshing/Processing/Spatial/Drawing + →AppUi/Render), `:49` (physics←Materials, drifted), `:51` (stock→Compute), `:53,54` (program/nfp→Persistence/Schema, DELETED target), `:57` (probing→Posting). Stale targets `:16` (2500), `:52` (netDxf), `:53,54` (Schema) all require re-point per `[V10]`.

Two-node acyclicity check (`owner#atoms` vs `owner#run`): the `ProjectionDir`-on-projection inversion + the `program → physics → ProcessModel → owner#atoms` chain are clean IF `ProjectionDir` and the `[V7]` residual field mint on `owner#atoms` (as the brief V1 rules). Today `ProjectionDir` living in `projection.md` forces `owner#atoms → projection`, which with `owner#run → projection` makes owner↔projection bidirectional at the type level — the exact case the two-node split exists to break.

---

## [05]-[CROSS_CUTTING]

1. **Register completeness gaps (2).** (a) E13 undercounts RESEARCH tails: `program.md:379` + `stock.md:317` are uncounted; V2 acceptance must purge 5 pages. (b) No E-row for the CutProgram content-key breach though `SEAM_AND_ENTRY_LAW` mandates it — `program.md:3`/`owner.md:3` "computes no hash", `CutProgram` (`:121`) keyless. The DECISION should add the content-key work explicitly to the Posting leg.

2. **Phantom-member class (beyond E7).** `projection.md` imports and prose-claims `Predicate.Orient2D` as its robustness floor but never calls it (raw-double signs at `:58,76,139`) — a page whose own stated defect is committed in its own fence. Distinct from `program.md` where `Orient2D` is real (`:334`). A leg-1 pass should grep every "settled predicate" claim against fence call sites.

3. **Stub-arm / illusory-breadth class.** `program.md` declares 21 `GCommand` rows; 9 are never emitted (lathe Css/ThreadCycle, tool-change ToolChange/LengthOffset, additive HotendTemp/Extrude/BedTemp, thermal TorchHeight, generic Dwell). The AST's multi-modality vocabulary is a facade over a milling-only emitter — APPROACH naivety (enumerated words, no generator arms). Mirrors the E2 magazine seam (the M6/G43 rows are the unwired half).

4. **APPROACH-naivety twins on my pages.** `physics` fragments one material into modality rows instead of a per-modality map (`[V9]`); `family`'s `PostDialect` is a 3-column render instead of a grammar-family generator (`[V4]`); `faults` uses bare `string Detail` instead of typed payloads. Each is "an enumerated instance where a parameterized generator belongs" — the campaign's canonical defect.

5. **Governance drift beyond the register.** `csproj:3` Description still says "CAM toolpath motion with DH/IK kinematics" — stale (kinematics superseded by the `Robots` cell, README:15,45). ARCH codemap `:16` (Faults band-2500), `:30`/`:63`/`:65` (multi-fixture scheduler as owned), and the `Rasm.Persistence/Schema` seam targets (`:53,54`) all carry pre-reband/pre-supersession facts. The V1 re-partition rewrites `ARCH:[01]/[02]` + the README router wholesale; every stale row above must close in that motion.

6. **What is genuinely sound (preserve, cold-pass).** `owner` entry fold + generated `Switch`; `family` axis machinery and `Admits` relations; `faults` 2700 band + offset discipline; `magazine`'s MTConnect mining and life-split `Schedule`; `program`'s `Lookahead`/`Feedrate`/`Biarc` author-kernel MATH (the dimensional jerk-vs-accel discipline is correct); `projection`'s watertight `Arrangement` arm and `HiddenLineResult` receipt shape. The rot is structural (namespaces, unwired seams, thin axes, phantom composes), not the interiors — architecture-first, per the brief.
