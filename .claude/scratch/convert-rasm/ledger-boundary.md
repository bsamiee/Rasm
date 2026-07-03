# LEDGER — BOUNDARY LANE (convert-rasm-kernel)

Read-only census of the Vectors<->Geometry boundary, the RhinoCommon compile entry, the compile-consumer disposition, and the RASM-CS-GEOMETRY-BRIEF re-anchoring surface. Every member below is disk-verified (file:line). `assay` was NOT invoked (tooling ban); external members route through `.api` catalogues + the source files themselves.

Scope read fully: 18 Geometry `.planning` pages; `libs/csharp/Rasm/{ARCHITECTURE,README}.md` + `Vectors/_ARCHITECTURE.md`; `Rasm.csproj`; root `Directory.Build.{props,targets}`; `Workspace.slnx`; both `.api` tiers (`api-rhino.md` read fully, roster cross-checked); `RASM-CS-GEOMETRY-BRIEF.md`; the compile consumers (Rasm.Rhino Camera/Commands/UI, Rasm.Grasshopper UI, the three test csprojs); `tools/cs-analyzer` docID/generator strings.

---

## [00]-[HEADLINE FACTS]

- **Only THREE csharp packages carry source**: `Rasm/` (31 .cs — Vectors 13 + Domain 4 + Analysis 14), `Rasm.Rhino/` (32 .cs), `Rasm.Grasshopper/` (16 .cs). The other 8 (`AppHost/AppUi/Bim/Compute/Element/Fabrication/Materials/Persistence`) are already planning-scoped (0 .cs). The conversion moves the kernel into that planning-scoped majority; **Rasm.Rhino + Rasm.Grasshopper become the ONLY remaining source in all of `libs/csharp`, orphaned against an empty kernel.**
- **Rasm.Bim is the exact target shape** (`Rasm.Bim.csproj` read): real `.csproj` with `ProjectReference ../Rasm/Rasm.csproj` + dozens of `PackageReference` + `.api/` + `.planning/`, ZERO `.cs`. So the converted kernel KEEPS `Rasm.csproj` (empty-source assembly, its geometry PackageReferences + RhinoCommon reference machinery intact) — retire only the `.cs`.
- **Archive verified complete**: `.archive/Rasm/` holds identical counts (Vectors 13/13, Domain 4/4, Analysis 14/14; git-ignored `-I`). Delete is safe against it.
- **The 18 Geometry pages RE-HOME path-only** (per task): `libs/csharp/Rasm/Geometry/.planning/{subdomain}/` → the new `libs/csharp/Rasm/.planning/` root. The V1 folder/namespace restructure in the brief is the LATER geometry-campaign's job, NOT this conversion — content is untouched.
- **Analysis deletion is Geometry-safe**: NO Geometry page imports `Rasm.Analysis`; the greenfield robust-core is disjoint from the mature measure/query layer (intersect.md:388 is "the robust counterpart Analysis/Intersect.cs never owns"). Analysis has only TWO source consumers (Rasm.Rhino Commands/Context.cs + UI/Overlay.cs) + 2 cs-analyzer vocabulary strings.

---

## [01]-[HOW RHINOCOMMON ENTERS THE COMPILE] (rhino ruling evidence)

RhinoCommon is **not a NuGet pin** — the in-process `RhinoCommon.dll` from the RhinoWIP bundle is the resolved asset (`api-rhino.md:8`). The machinery, all in root `Directory.Build.{props,targets}`:

| Fact | Anchor |
|---|---|
| `RhinoCommonReferencePath = $(RhinoWipResourcesPath)/RhinoCommon.dll` | `Directory.Build.props:159` |
| `RhinoWipResourcesPath = $(RhinoWipAppPath)/Contents/Frameworks/RhCore.framework/Versions/Current/Resources` | `props:158` |
| `RhinoWipAppPath` default `/Applications/RhinoWIP.app`, override `$(RHINO_WIP_APP_PATH)` | `props:156-157` |
| Kernel `Rasm` is `IsRhinoCommonAwareProject` (dir under `$(CSharpLibsRoot)Rasm/`) | `props:46` |
| → gets `<Reference Include="RhinoCommon"><HintPath>$(RhinoCommonReferencePath)</HintPath><Private>$(HostReferencePrivate)</Private>` (=`false` for non-test) | `props:346-349, 37, 173` |
| Kernel is NOT `IsRhinoUiAwareProject` / NOT `NeedsRhinoHostUiSurface` → **RhinoCommon.dll ONLY**, no Rhino.UI/Eto/System.Drawing.Common/Grasshopper2/Microsoft.macOS | `props:47-48, 174-181` |
| `VerifyRhinoHostBundle` target errors loudly if `RhinoCommon.dll` absent when RhinoCommon-aware | `Directory.Build.targets:20-26` |
| `Rasm.csproj` global usings inject `Rhino`, `Rhino.FileIO`, `Rhino.Geometry`, `Rhino.Geometry.Intersect` (+ `Rasm.Domain`) → RhinoCommon value vocab is pervasive across kernel source | `Rasm.csproj:38-42` |
| `InternalsVisibleTo` Rasm→{Rasm.Grasshopper, Rasm.Rhino}; and Rasm→{$(name).Tests, Rasm.TestKit} | `props:290-297` |

**BOUNDARY_LAW** (`api-rhino.md:168-172`): the kernel reads `Rhino.Geometry` VALUE structs only (`Point3d/Point3f/Vector3d/Vector3f/Plane/Line/Polyline/BoundingBox/Ray3d/Transform/Interval/Quaternion/Circle/Sphere/Arc/Box/Cone/Cylinder/Torus` + reference `Mesh/MeshFace/Curve/NurbsCurve/PolylineCurve/Brep/GeometryBase` + `Rhino.Geometry.Intersect.*` + `Rhino.Geometry.Collections` topology lists), composed **through `Rasm.Vectors`**, re-emitted at the seam. It NEVER reaches `RhinoDoc/RhinoApp/RhinoView/DisplayConduit/ObjectTable` — that document/view/command/display surface is the Rasm.Rhino host-boundary stratum's concern (`api-rhino.md:172`). Host `RTree` broad-phase is the DELETED form (kernel authors its own SAH-BVH/Morton — `api-rhino.md:76`); host `Intersection.Curve*/Brep*/Ray*` parametric is the `Analysis` layer's concern (`api-rhino.md:162-166`); host `Mesh.CreateBoolean*` is the boundary the predicate-exact arrangement does NOT use (`api-rhino.md:138`).

**Rhino coupling of the Geometry pages**: 17 of 18 import `using Rhino.Geometry;` — **only `faults.md` is host-neutral** (imports LanguageExt + Thinktecture only; a pure `GeometryFault` union with no geometry). `Point3d` is the dominant coupled type (predicates.md 27×). Per-page Rhino value surface in [03] below.

**Rhino ruling for the conversion**: KEEP `Rasm.csproj` as a planning-scoped package (Rasm.Bim shape). The RhinoCommon reference machinery (`props:155-182`, `targets:20-26`) stays valid — with ZERO `.cs`, the empty assembly still resolves `RhinoCommonReferencePath` fine (or the `_RhinoRequiredHostFile` gate simply has nothing to compile against, but the `<Reference>` is harmless on an empty compile). The `Rhino.Geometry` global usings and geometry `PackageReference`s in `Rasm.csproj` remain the documented implementation surface the design pages compose. `IsRhinoCommonAwareProject`'s `$(CSharpLibsRoot)Rasm/` clause (`props:46`) still matches the (now source-less) kernel dir — no MSBuild edit needed for the kernel itself.

---

## [02]-[COMPILE-CONSUMER CENSUS — EXACT MEMBERS] (consumer disposition evidence)

The retiring kernel namespaces (`Rasm.Vectors`, `Rasm.Domain`, `Rasm.Analysis`) are consumed by exactly two source packages + the analyzer. Rasm.Rhino/Rasm.Grasshopper SOURCE is NOT edited by this run — their build-surface disposition is the blueprint ruling. Exact usage:

### Rasm.Rhino (32 .cs, CspScope=Boundary, `ProjectReference ../Rasm/Rasm.csproj`)

| Consumer file:line | Kernel member consumed | Owner (retiring) |
|---|---|---|
| `Camera/Operations.cs:1` | `using Rasm.Vectors;` | Vectors |
| `Camera/Operations.cs:475` | `VectorIntent.Direction(value).Project<Vector3d>(context, key)` | `Vectors/Intent.cs:46` (`Direction`), `Intent.cs` (`Project`) |
| `Camera/Operations.cs:578` | `VectorIntent.Pose(start.Frame, to, t, mode: MotionInterpolation.Slerp, key)` | `Vectors/Intent.cs:145` (`Pose`), `Vectors/Modes.cs:135` (`MotionInterpolation.Slerp`) |
| `Camera/State.cs:2` | `using Rasm.Vectors;` | Vectors |
| `Camera/State.cs:193` | `VectorFrame.Of(origin, normal, xHint, context, key)` | `Vectors/Atoms.cs` (`VectorFrame`) |
| `Camera/State.cs:612` | `VectorFrame.Chain(...)` | `Vectors/Atoms.cs` |
| `Commands/Context.cs:1` | `using Rasm.Analysis;` | Analysis |
| `UI/Overlay.cs:3` | `using Rasm.Analysis;` | Analysis |
| `UI/Overlay.cs:191` | `Analyze.Run<object,BoundingBox>(query: AnalysisQuery.Bounds(Analysis.Bounds.AxisAligned), input)` | `Analysis/Analyze.cs:100` (`Run`), `Analysis/Query.cs:8` (`AnalysisQuery.BoundsCase`), `Analysis/Bounds.cs` (`Bounds`) |
| `Rasm.Rhino.csproj:12` | `<Using Include="Rasm.Domain" />` (global) | Domain (unqualified Domain types across source) |
| props global usings | `Rhino.*` host usings (csproj:11-18) — unaffected | host |

Plus **20+ `[Union]` declarations** in `Rasm.Rhino/Blocks/*.cs` + `Camera/*.cs` (grep: Blocks.cs, Operations.cs ×5, State.cs ×11, Camera.cs, Operations.cs ×2). Each expands via the cs-analyzer generator to `global::Rasm.Domain.Op.Of(...)` — see [05].

### Rasm.Grasshopper (16 .cs, CspScope=Boundary, `ProjectReference ../Rasm/Rasm.csproj`)

| Consumer | Kernel member consumed | Owner (retiring) |
|---|---|---|
| **11 UI/*.cs** (Canvas, Document, Editor, Events, Input, Interaction, Layout, Motion, Paint, Ui, Wire) | `using Op = Rasm.Domain.Op;` — **explicit type alias, HARD compile edge** (line 4-21 of each) | `Domain/Validation.cs:20` (`readonly partial struct Op`, `Op.Of([CallerMemberName])`) |
| UI/*.cs (e.g. Paint.cs:521,627; Layout.cs:190) | `Geometry.Bounds`, `Geometry.Path`, `Geometry.IsSome` (Domain `Geometry` static) | `Domain/Geometry.cs` |
| UI/Ui.cs:11 | `Domain.Expected` | `Domain/Validation.cs:182` (`abstract record Expected : Error`) |
| props (IsGrasshopperAwareProject) | global usings `Rasm.Analysis` + `Rasm.Domain` injected | `Directory.Build.props:276-277` |

Grasshopper.csproj itself lists only `Grasshopper2.*` host usings; the `Rasm.Domain`/`Rasm.Analysis` bindings arrive via `props:272-280` (Grasshopper-Aware Usings) + the 11 explicit `using Op = Rasm.Domain.Op;` aliases. Plus `[Union]` declarations (Components + UI) → same `Rasm.Domain.Op` generator edge.

### Test projects (3, all `AssayTestShell=true` — trivial disposition)

All three (`tests/csharp/libs/Rasm/Rasm.Tests.csproj`, `.../Rasm.Rhino/Rasm.Rhino.Tests.csproj`, `.../Rasm.Grasshopper/Rasm.Grasshopper.Tests.csproj`) are **spec-free shells** ("Spec-free shell during the planning phase; assay routes SHELL-lane projects out of test runs"): a single `ProjectReference` to their target package, ZERO `.cs`. **They break only if their target csproj is removed** — since Rasm.csproj + Rasm.Rhino.csproj + Rasm.Grasshopper.csproj all survive, the test shells need no edit. Disposition: leave as-is.

### Workspace.slnx rows (which stay)

`/libs/csharp/Rasm/` → `Rasm.csproj` (row 27-29) **stays** (empty-source, Rasm.Bim shape). `/libs/csharp/Rasm.Rhino/` (39-41), `/libs/csharp/Rasm.Grasshopper/` (30-32) **stay** (source untouched). `/tests/csharp/libs/{Rasm,Rasm.Rhino,Rasm.Grasshopper}/` (81-92) **stay** (shells reference surviving csprojs). No slnx row is removed by the conversion. (Slnx also carries `Rasm.Element` at 21-23 — planning-scoped, and its `.csproj` is referenced by Rasm.Bim; unrelated to this run.)

### tools/cs-analyzer coupling (docID STRINGS + one GENERATOR edge)

| Anchor | Content | Kind |
|---|---|---|
| `Generators/UnionOpsGenerator.cs:31` | `const string GenerateUnionOpsMetadataName = "Rasm.Domain.GenerateUnionOpsAttribute";` | marker-name string |
| `Generators/UnionOpsGenerator.cs:95` | emits `internal static readonly global::Rasm.Domain.Op SelfOp = global::Rasm.Domain.Op.Of(name: nameof(<case>));` | **GENERATED-CODE dependency on `Rasm.Domain.Op`** |
| `Kernel/Vocabulary.cs:42,63` | `"T:Rasm.Domain.OpAcceptance"`, `"T:Rasm.Domain.ClosestHit"` | docID vocabulary string |
| `Kernel/Vocabulary.cs:64,65` | `"T:Rasm.Analysis.IntersectionHit"`, `"T:Rasm.Analysis.RayQuery"` | docID vocabulary string |

The 4 `Vocabulary.cs` strings are pure data (no compile edge). But **UnionOpsGenerator.cs:95 is a live semantic edge**: every `[Union]` in every project that runs this generator expands to code referencing `global::Rasm.Domain.Op`. Rasm.Rhino + Rasm.Grasshopper both declare many `[Union]`s → their generated partials require `Rasm.Domain.Op` and `Rasm.Domain.GenerateUnionOpsAttribute` to exist at compile.

---

## [03]-[PER-PAGE CAPABILITY LEDGER] (the 18 Geometry pages)

`fence-ns` = namespace the page's code fence declares. `Vectors composed` = the settled Vectors vocabulary the page reads (co-assembly, incl. `internal` Vectors types). `Domain composed` = Domain vocab. `Rhino` = dominant `Rhino.Geometry` value types (all via Vectors substrate). Quality verdict synthesizes the brief's disk-verified VERDICT/V1-V14/E1-E15 anchors with my read. Target folder = brief V1 ratification (FUTURE geometry-campaign state, NOT this conversion).

### Numerics/ (2 pages, 590 LOC)

| Unit | Anchor | What it owns | Vectors / Domain / Rhino | Verdict | Bloat / dup | Target |
|---|---|---|---|---|---|---|
| `Predicate` (Orient2D/3D, InCircle, InSphere + 4 implicit-point LPI/TPI), `Sign`, `PrecisionTier`, `Expansion`, `ErrorBound`, `RationalOracle`, `Lpi/Tpi`, `NumericsPolicy` | `predicates.md` (502) | Adaptive-precision exact-predicate floor (double→ddouble→Expansion→Fraction ladder) | **Vectors: `Point3d`/`Direction` only** (reads `.X/.Y/.Z` at seam). Rhino: `Point3d` (27×), `Plane`. Domain: none. Ext: DoubleDouble, BigRational, BCL | **9/10 — crown jewel, award-grade, every composed member `.api`-verified** | Clean. GAP: **fence declares NO namespace** (should be `Rasm.Geometry.Numerics`); V14 wants a strict-IEEE-754/RID invariant + FMA-free `TwoProduct` fallback + a package-boundary-consumable export for Rasm.Compute CDTet | Numerics/ |
| `GeometryFault` `[Union]` (band 2400-2447) | `faults.md` (88) | The one fault union every geometry rail routes through, `.ToError()` lowering | **HOST-NEUTRAL: no Rhino, no Vectors, no MeshSpace.** Domain: `Validation`/`Kind` (via Thinktecture). Ext: LanguageExt, Thinktecture | 7/10 — sound single-union band shape | **10 of 17 cases carry bare `string Detail`** vs the page's own typed-payload law (E15, V12); 5 rich cases prove the shape | Numerics/ (root `Rasm.Geometry` ns) |

### Spatial/ (3 pages, 887 LOC)

| Unit | Anchor | What it owns | Vectors / Domain / Rhino | Verdict | Bloat / dup | Target |
|---|---|---|---|---|---|---|
| `SpatialIndex` `[Union]` (Bvh/LinearOctree), `NodeStore` SoA, `SpatialQuery` (Nearest/Range/Ray/Overlap/Winding), `Refit`, `ToAcceleration` | `index.md` (636) | SAH-BVH + Morton-octree broad-phase over one flat node store | Vectors: none (composes RhinoCommon `BoundingBox/Point3d/Vector3d/Ray3d/Sphere/Line` "via Vectors substrate" but no MeshSpace). **Ext: `SuperClusterKDTree`** (kd-tree) + **`Rasm.Compute.Solver` (UPWARD STRATA VIOLATION, `index.md:27`)**. Rhino: `BoundingBox`, `Point3d` | **5/10 — BVH/octree/PLOC/GWN breadth sound, but 4 breaches** | `using Rasm.Compute.Solver` + `ToAcceleration` returns Compute's `AccelerationStructure` (E6, V8 — the exact upward edge `libs/.planning/architecture.md:35` forbids); throws `InvalidOperationException` (`:130,574,579`); silent-empty (`:358`); round-to-nearest bounds → false-negative prune (`:79`) | Spatial/ |
| `TopoName` `[ValueObject<UInt128>]`, `NameTable`, `Track`, `EntityKind`, `TopoSignature`, `TrackOutcome`, `Generation` | `naming.md` (131) | Persistent topological naming surviving rebuilds | **Vectors: `MeshSpace`, native `Mesh`. Domain: `Rasm.Domain.ContentHash.Of` (explicit, :59,77).** No Rhino import (via MeshSpace). Ext: Thinktecture, LanguageExt | 5/10 — sound lineage algebra, two confirmed bugs | `Track` keys `VertexNames` by `entity.IncidentVertices[0]` (star-min, not self — E4c, `:113`); `Subsumes` subset direction wrong for splits (`:62-66`); Survive×Migrate injectivity residual OPEN (`:161`). Fence declares NO namespace | Spatial/ |
| `CanonicalTopology`, `NamingHash`, `Encode`, `Reconcile`, `NameAddress`, `ONE_WIRE_FIXTURE_CORPUS` | `reconciliation.md` (120) | Naming↔hash fence → Persistence `GeometryHash` content-address | **Vectors: `MeshSpace.DuplicateNative()`→native `Mesh` (`TopologyVertices`, `TopologyEdges.GetTopologyVertices`, `IndicesFromFace`, `IndexPair.I/.J`). Domain: `Rasm.Domain.ContentHash.Of` (seed-zero XxHash128).** Rhino: `Mesh`, `IndexPair` | **8/10 — the federation content-hash seam, frozen-fixture spine sound** | Fence declares NO namespace. Origin of the naming mis-key (star-build `:55-62`). FROZEN: 52-byte stream + `0x9462A71A5DD13DCFA3B1D6D225FCBE70` | Spatial/ |

### Meshing/ (4 pages, 1380 LOC)

| Unit | Anchor | What it owns | Vectors / Domain / Rhino | Verdict | Bloat / dup | Target |
|---|---|---|---|---|---|---|
| `Tessellation`, `SimplexStore`, Bowyer-Watson CDT/CDTet | `delaunay.md` (211), ns `Rasm.Geometry.Tessellation` | Constrained Delaunay/tetrahedralization on exact InCircle/InSphere | Vectors: `MeshSpace`. Domain: `Context`. Rhino: `Point3d`, `Vector3d`. Composes Numerics | 6/10 — exact floor sound | **7 signature-only `[STORE_OPS]` stubs** (Seed/Retriangulate/RecoverOne/StripSuper/LastLive/TouchesSuper/AddSimplexFaces, `:192-198`, E15); exposes NO Voronoi-dual accessor the medial needs; false "Triangle GPL" rejection (`:3`) | Meshing/ |
| `IntersectOp` `[Union]`, `Crossing` (Lpi/Tpi), exact-sign crossing lattice | `intersect.md` (332), ns `Rasm.Geometry.Intersection` | Predicate-exact seg/tri/mesh crossing — the declared single crossing owner | Vectors: `MeshSpace`, `MeshEdit`. Rhino: `Point3d` (6×). Ext: `ExtendedNumerics` (BigRational). Composes Healing/Numerics/Spatial | 6/10 — owner-declared collapse target | Declares itself the sole crossing owner (`:388`) but **3 inline copies still live** (arrangement/offset/repair — E7); silent Z=0 projection (`:293`) | Meshing/ |
| `Arrangement` `[Union]`, `ArrangementStore`, `BooleanOp`, `BooleanReceipt`, cell-complex | `arrangement.md` (419), ns `Rasm.Geometry.Arrangement` | Managed exact mesh-boolean/planar-overlay retiring native CSG | Vectors: `MeshSpace`, `MeshEdit`. **Domain: `Op` (`:124,125,476`), `Context`.** Rhino: `Point3d`, `Vector3d`. Ext: `ExtendedNumerics`. Composes Healing/Numerics/Spatial/Tessellation | 7/10 — composition depth sound | Implicit-point exactness OVERCLAIMED (`:467`) vs rounded `PlaneCrossPoint` (`:258-262`, E7); `BooleanOp` upward from Healing + repair⇄arrangement **page cycle** (V5); `BooleanReceipt` name collides w/ receipts.md (`:115` vs receipts.md:44) | Meshing/ |
| `OffsetOp` `[Union]`, `WavefrontStore`, skeleton/medial/minkowski, `MedialAxis` | `offset.md` (418), ns `Rasm.Geometry.Offsetting` | Aichholzer-Aurenhammer wavefront offset owner | Vectors: `MeshSpace`. **Domain: `Context`.** Rhino: `Point3d`, `Polyline`, `Mesh`. Composes Arrangement/Numerics/Tessellation | 5/10 — miter-only, gate-incomplete | Medial "reconciles against Voronoi dual" but `MedialFrom` builds+**DISCARDS** the tessellation (`:325-326`, E7); `WavefrontStore` `2n` under-alloc vs unbounded Spawn (`:195`); `SegmentsCross` inline crossing copy (`:440-444`); stale 4-of-5 owner roster (`:13`) | Meshing/ (+`slice`/`skeleton` new per V10) |

### Processing/ (6 pages, 2488 LOC)

| Unit | Anchor | What it owns | Vectors / Domain / Rhino | Verdict | Bloat / dup | Target |
|---|---|---|---|---|---|---|
| `HealOp` closed `[Union]`, `Heal.Repair`, **`MeshEdit`** (de-facto shared mesh working set) | `repair.md` (386), ns `Rasm.Geometry.Healing` | Repair algebra (6 author-kernels + 1 native-gate boolean) | Vectors: `MeshSpace`, **`MeshEdit` (OWNER)**, `VectorIntent`, `TopologyReceipt`, `MeshKernel`. **Domain: `Op`, `Context`, `Topology`.** Rhino: `Point3d` | 5/10 — donor of MeshEdit substrate | `SelfIntersectResolve` naive one-point fan (`:244-250`) vs its own delaunay dep; `((QueryResult.Hits)hits)` hard cast (`:243`); 2N topology recompute (`:382-385`); `TriangleCrossPoint`/`EdgesCrossTriangle` inline crossing (`:346-372`, E7); `BooleanOp` mis-homed (V5) | Meshing/ substrate extract (V6) + Processing/ |
| `RebuildReceipt` `[Union]` (7 cases), `ManifoldStatus`, `HealSession`, `RebuildLog` | `receipts.md` (102) | Typed per-op heal evidence the naming Track consumes | **Vectors: `TopologyReceipt` via PUBLIC `VectorIntent.Topology(space).Project<(int Euler,int Genus,int BoundaryComponents)>` seam** (`IsManifold`/`NonManifoldEdges` NON-projectable; `MeshKernel.TopologyDetailed` internal). `MeshEdit`, `MeshKernel`. Domain: `Op`, `Topology`. Rhino: `Mesh` | 6/10 — typed union sound | `Converged` requires `b.AssetGated` (`:99`) but every `BooleanReceipt` mints `AssetGated:false` (`:65`) → every managed boolean reports non-converged (E4d); `BooleanReceipt` name collision (V5). **Fence declares NO namespace** | Processing/ |
| `SimplifyOp` `[Union]`, `QuadricStore`, Garland-Heckbert QEM, `DecimationReceipt` | `decimate.md` (499), ns `Rasm.Geometry.Simplification` | Predicate-guarded decimation/LOD | Vectors: `MeshSpace`, `MeshEdit`, `ScalarField`, `SignedDistanceFromMesh`, `VectorCloudMetric`, `VectorCloud`, `VectorIntent`. **Domain: `Context`.** Rhino: `Vector3d`, `Mesh`. Ext: DoubleDouble, MathNet | **4/10 — two structural bugs** | Hardcoded interior link `shared==2` (`:312`) → boundary edges never collapse, `TargetFraction` unreachable on open meshes (E4e); `FanFaces`/`CollapsedFaces` linear-scan full face array per collapse = O(F²) vs "O(1) fans" prose (`:16` vs `:318-322,342-360`) | Processing/ |
| `ParamOp` `[Union]` (harmonic/LSCM/ARAP/BFF), `ChartAtlas`, `ChartStore`, `LaplacianCache` compose | `flatten.md` (519), ns `Rasm.Geometry.Parameterization` | Robust UV-flattening over the Vectors DEC substrate | **Vectors: `DiscreteCalculus`, `MeshAdjointSnapshot`, `LaplacianCache`, `IntrinsicMesh`, `MeshSpace`, `VectorIntent` — the DEEPEST Vectors coupling (the DEC operator substrate).** Rhino: `Point3d`. Ext: MathNet (via Vectors CholeskySparse) | **4/10 — three math bugs, but DEC composition shape sound** | Inverted Dirichlet: `MassShift`(1e-9) added to every diagonal + rhs `value/MassShift`=value·1e9, no penalty on pinned rows (`:359,364`, E4a); LSCM claims sparse but allocates dense `(2n)²`+`DecomposeSvd` O(n³) (`:4` vs `:377,173`, E4b); ARAP `IndexOf` O(F) scan (`:482-485`); `FactorNonZeros` reports `D1.NonZeros` not factor fill (`:330`) | Processing/ |
| `FitOp` `[Union]` (plane/sphere/cyl/cone/torus/line), `FitStore`, MLESAC + LM refine, `FitReceipt` | `fit.md` (648), ns `Rasm.Geometry.Fitting` | Robust primitive-fit (scan-to-BIM segmentation) | Vectors: `VectorCloudMetric`, `VectorCloud`, `CloudKernel`, `VectorIntent`. **Domain: `Context`.** Rhino: `Sphere`, `Plane`, `Line`, `Circle`. Ext: DoubleDouble, MathNet. Composes Spatial | 5/10 — MLESAC sound, LM double-owner | Claims "the SAME LM the sketch solver composes" (`:16`) but re-implements the ladder (`:568-593` vs solver.md:295-347) with hardcoded `1e-3`/`×10`/`1e12`, no λ carry (E7, V7); `FitStore` documented but NEVER constructed (`:3,13,248`, E11); `Line` chart degenerate (Z-fixed anchor) | Solving/ (V1 lifts fit+solver into one folder) |
| `Constraint` residual/Jacobian algebra, `DofAnalysis`, LM `Solve`/`Iterate`/`SolveCholesky` | `solver.md` (334), ns `Rasm.Geometry.Constraints` | Author-kernel geometric constraint solver | **Vectors: NONE.** Domain: `Kind`, `Topology`. Rhino: `Circle`, `Vector3d`, `Point3d`. **Ext: direct `MathNet.Numerics.LinearAlgebra`** (the V7 target — should be the one SolveCholesky rail) | 7/10 — analytic-Jacobian LM + witness DOF sound | `Iterate`/`Step`/`SolveCholesky` private + `Constraint`-coupled via `Linearize` (`:295-347`); direct MathNet scatter vs flatten's wrapper discipline (two laws, one concern — V7) | Solving/ |

### Drawing/ (2 pages, 776 LOC)

| Unit | Anchor | What it owns | Vectors / Domain / Rhino | Verdict | Bloat / dup | Target |
|---|---|---|---|---|---|---|
| `ViewOp` `[Union]` (silhouette/hidden-line/section/outline), `BspNode`/`Partition`/`Split`, `DrawingProjection` | `view.md` (378), ns `Rasm.Geometry.Projection` | Predicate-exact HLR/silhouette projection | Vectors: `MeshSpace`, `MeshEdit`, `VectorIntent`. **Domain: `Context`.** Rhino: `Point3d` (6×), `Polyline`. Composes Healing/Intersection/Numerics/Spatial | **3/10 — the advertised engine does NOT exist** | Thesis is Newell-Newell-Sancha BSP + Appel QI; **`Paint` never reads `bsp`** (`:302`), builds+discards `Partition/Split/BspNode` (`:263-300`), visibility is uniform `SampleStep` ray-sampling (`:320-347`), `ToPolylines` concats by kind-key not connectivity (`:130-135`) → garbage (VERDICT proof 1, V3 full rebuild) | Drawing/ |
| `PackOp` `[Union]` (point-cloud/mesh-patch/voxel/brep-patch), `EncodingChannel` (8 rows), `EncodedGeometry`, `EncodedStore` | `pack.md` (398), ns `Rasm.Geometry.Encoding` | Canonical geometry-encoding/residency lattice | **Vectors: WIDEST — `MeshSpace`, `VectorCloud`, `ScalarField`, `MeanCurvatureFlow`, `SampleDetailed`, `SignedDistanceFromMesh`, `VectorCloudMetric`, `VectorIntent`, `MeshSegmentation`** (the live mesh/cloud/SDF/curvature/geodesic readers). **Domain: `ContentHash`, `Context`, `Topology`.** Rhino: `Mesh`, `Vector3d`. Ext: `System.Numerics.Tensors` | 5/10 — widest composition, residency claims false | **Imports phantom `Rasm.Geometry.Topology` (`:33`, E5)**; false Half-residency (`EncodedStore.Payload` always `float[]`, `:126-129` vs `:13,463`, E13); `OrientedNormals` mis-attributed to Processing/fit (`:3`); **`ScalarField.SampleDetailed` seam admitted UNVERIFIED/unexposed (`:459`)** | Drawing/ |

### Parametric/ (1 page, 156 LOC) — THE UNROUTED 18th PAGE

| Unit | Anchor | What it owns | Vectors / Domain / Rhino | Verdict | Bloat / dup | Target |
|---|---|---|---|---|---|---|
| `Parametric` `[Union]` (Curve/Surface), `ParametricKind`, `ParametricOp`, `SurfaceFactory`, `EvalResult`, `ReconstructKind`, `Parametrics` | `curve.md` (156), ns `Rasm.Geometry.Parametric` | Host-neutral NURBS eval over GShark (the non-Rhino runtime contract) | **Ext: `GShark.Geometry`** (aliases `GPoint3/GVector3/GPlane`; `NurbsBase`/`NurbsSurface` instance algebra, `Fitting.Curve`, `Intersection.Intersect`). Vectors: `Point3d`/`Vector3d`/`Plane`/`Polyline`/`MeshSpace` (marshalled to GShark at boundary). Domain: `Context`. Rhino: `Point3d`/`Vector3d`/`Plane`/`Ray3d`. Composes Numerics (Predicate escalation) | **3/10 — signature-only stubs** | `CurveFrom`/`SurfaceFrom`/`CurveApply`/`SurfaceApply`/`ToPolyline`/`ToMesh` are BODY-LESS (`:116-138`); curve `Offset` demoted to Growth (`:19`); **one-file folder (V1 standing violation)**; **UNROUTED by README router + ARCHITECTURE codemap** (see [06]). Runtime-split law (GShark host-neutral vs Analysis/Intersect.cs Rhino, meeting at the wire) stated correctly (`:191`) | Parametric/ (V2 grows +5: surface/subdivide/develop/panelize/patternmap) |

---

## [04]-[THE VECTORS<->GEOMETRY BOUNDARY MAP]

**Namespace law** (`ARCHITECTURE.md:62-64`): `Rasm.Domain`/`Rasm.Vectors`/`Rasm.Analysis` (mature) and `Rasm.Geometry.*` (greenfield) are DISJOINT roots — the `Geometry` token is a path coincidence, not a namespace collision (`Domain/Geometry.cs` declares `Rasm.Domain` and owns `Topology`/`Kind`/`CurveForm`, NOT a type named `Geometry`).

**ONE ASSEMBLY**: Vectors + Domain + Analysis + Geometry are ALL the single `Rasm.csproj` assembly. So Geometry pages compose **`internal` Vectors types co-assembly** — the decisive proof: `flatten.md` composes `IntrinsicMesh` which is `internal sealed class IntrinsicMesh` (`Vectors/Mesh.cs:1153`). The public `MeshAdjointSnapshot` handle (`Vectors/_ARCHITECTURE.md:132,151`) exists for CROSS-package (Rasm.Compute), but INSIDE the assembly Geometry reaches `DiscreteCalculus`/`LaplacianCache`/`IntrinsicMesh` directly. **Conversion implication**: keep everything under ONE `Rasm.csproj` (Rasm.Bim shape) and the internal-visibility composition seams survive as design-page vocabulary; splitting the kernel into multiple assemblies would break them.

**The settled Vectors vocabulary Geometry composes** (read, never re-mint — `ARCHITECTURE.md:39`, `README.md:3`):
- `MeshSpace` — the canonical mesh handle (composed by 12 of 18 pages; the dominant seam type).
- `MeshEdit` — the working mutable mesh arena (owned by repair.md `Rasm.Geometry.Healing`, consumed co-namespace by view/arrangement/intersect/decimate/receipts — the V6 "MeshEdit substrate" fan).
- `TopologyReceipt` via `VectorIntent.Topology(space).Project<(Euler,Genus,BoundaryComponents)>` — the PUBLIC topology projection seam (receipts.md, repair.md). `IsManifold`/`NonManifoldEdges` are non-projectable; `MeshKernel.TopologyDetailed` is Vectors-internal.
- `DiscreteCalculus`/`LaplacianCache`/`MeshAdjointSnapshot`/`IntrinsicMesh` — the DEC operator substrate (flatten.md, the deepest coupling).
- `ScalarField`/`VectorField` — the SDF/curvature/geodesic field readers (`SignedDistanceFromMesh`, `MeanCurvatureFlow`, `Geodesic` — decimate, pack).
- `VectorCloud`/`VectorCloudMetric`/`CloudKernel` — PCA/normals/curvature + Sinkhorn (fit, decimate, pack).
- `SampleDetailed` — a pack.md seam ADMITTED UNVERIFIED/unexposed (`pack.md:459`; the V-law "verify or die" member).
- `VectorIntent`, `MeshSegmentation` — the intent rail + segmentation.

**The Domain vocabulary Geometry composes**:
- `Rasm.Domain.ContentHash.Of` (seed-zero XxHash128) — the ONE federation hasher (naming, reconciliation, pack). No second hasher anywhere.
- `Rasm.Domain.Op` — the operation-key primitive (arrangement, repair, receipts) — SAME `Op` the `[Union]` generator bakes.
- `Rasm.Domain.Context` — the tolerance context (10 of 18 pages).
- `Rasm.Domain.Topology`/`Kind`/`CurveForm` — the object-kind discriminant (repair, receipts, solver, pack).

**Boundary pressure Vectors→Geometry**: (1) Vectors owns the SUBSTRATE (mesh/DEC/field/cloud/spectral); Geometry owns the EXACT/ROBUST tier (predicates/CDT/arrangement/crossing/skeleton/heal). The `README.md:118-123` invariant is explicit: "Domain owns Rhino normalization + ClosestHit; Vectors owns intent/field/cloud/mesh/sampling/alignment/spectral; RhinoCommon provides native geometry." (2) The floor-first law (`ARCHITECTURE.md:39`, brief `[FOUNDATIONS_FIRST]`): a Geometry page may not hand-roll what Vectors owns; a Vectors floor too thin to serve its consumers is Vectors' defect (the `SampleDetailed` exposure gap, the Voronoi-dual accessor). (3) The GWN double-owner (`index.md` Barnes-Hut vs Vectors exact per-triangle winding — V8 rules index owns the BVH query, Vectors' SDF composes it).

---

## [05]-[BRIEF RE-ANCHORING SURFACE] (RASM-CS-GEOMETRY-BRIEF.md)

The brief is `cs track 1/6, run FIRST` and already anticipates the kernel conversion ("Phase 1 ... before or after the kernel conversion, never mid"; "no own `.csproj`; `libs/csharp/Rasm/Rasm.csproj` owns the manifest"). But its anchoring assumes the CURRENT topology. The conversion invalidates two anchor classes:

1. **Path anchors** (`brief:125` "Anchors are `libs/csharp/Rasm/Geometry/.planning/`-relative"): every page anchor (`view.md:302`, `flatten.md:359`, `curve.md:117-138`, etc. — the entire E1-E15 register + V1-V14) is relative to `Geometry/.planning/`. The RE-HOME shifts these to the new `libs/csharp/Rasm/.planning/` root. **All E-register and V-ruling path anchors need a prefix re-map.**

2. **Mature-source anchors + the verification spine** (CRITICAL): the brief repeatedly anchors to the LIVE mature source — `Vectors/Field.cs:386-394,1864-1872` (ScalarField.Geodesic/MeanCurvatureFlow/SignedDistanceFromMesh, CrossField/VectorHeat/GeodesicTangent/TangentLogMap), `Vectors/Extraction.cs`, `Vectors/Flow.cs`, `Analysis/Intersect.cs`. Its verification methodology is **"`assay api` over the restored `Rasm.Vectors` assembly is truth"** (`brief:48,59`; also `[SEAM_AND_RAIL_LAW]`, V14, E4). **Post-conversion there is NO compiled `Rasm.Vectors` assembly to `assay api` against** — Vectors/Domain retire to design pages. The brief's entire member-verification rail (`ScalarField.SampleDetailed`, `VectorIntent.Topology` projection, `DiscreteCalculus` CSR shape, `MeshAdjointSnapshot` seam) must re-anchor from "assay over restored assembly" to ".api catalogue + the newly-authored Vectors/Domain design-page fences." This is the deepest brief re-anchoring obligation and should be surfaced to whoever sequences the geometry campaign relative to the conversion.

3. **Cross-corpus blast radius**: the retiring kernel namespaces are referenced in **162 design-page lines across sibling PLANNING corpora** (Persistence/Materials/Fabrication/Element/Bim) as `using Rasm.Vectors;` / `Rasm.Domain.X` / `Rasm.Analysis.X` (e.g. Materials/bsdf.md composes `Rasm.Vectors.Direction`/`VectorFrame`; Fabrication/stock.md `using Rasm.Vectors; // PositiveMagnitude`; Element/element.md the `Rasm.Vectors` coordinate seam). These are design-page anchors (not compile edges), but every one is a re-anchoring obligation when the Vectors/Domain namespaces reshape. Owned by other lanes/campaigns — flagged here as boundary fact.

---

## [06]-[CROSS-CUTTING FINDINGS]

**F1 — The two source consumers become orphans; their disposition is the ruling.** After the kernel `.cs` retires, Rasm.Rhino (32) + Rasm.Grasshopper (16) are the ONLY source in `libs/csharp` and reference retired namespaces via HARD edges: 11 Grasshopper `using Op = Rasm.Domain.Op;` aliases + props global usings `Rasm.Domain`/`Rasm.Analysis`; Rasm.Rhino `using Rasm.Vectors;`×2 + `using Rasm.Analysis;`×2 + `<Using Include="Rasm.Domain"/>`; PLUS every `[Union]` in both expands to `global::Rasm.Domain.Op`. Since the task freezes their source, the architect must rule: they join the planning-phase non-compiling set (consistent with the 8 already-source-less packages), OR a minimal compiled kernel primitive survives. This run does not edit them; the ruling is the blueprint's.

**F2 — `Rasm.Domain.Op` is the single hardest coupling in the tree.** It is (a) the operation-key primitive Geometry pages compose (arrangement/repair/receipts), (b) referenced pervasively by the mature Vectors/Analysis source, (c) baked into the cs-analyzer `UnionOpsGenerator.cs:95` emit `global::Rasm.Domain.Op.Of(...)` that EVERY `[Union]` in EVERY project expands against, (d) explicitly aliased by 11 Grasshopper files, and (e) `Rasm.Domain.GenerateUnionOpsAttribute` (`UnionOpsGenerator.cs:31`) is the marker. Retiring `Domain` to planning removes `Op` as a compiled type → the `[Union]` machinery breaks across all remaining source. The architect must rule Op's fate explicitly (it is NOT just a namespace import).

**F3 — Entry-point rot (naive/dead engines behind live claims).** view.md ships a dead BSP behind an HLR thesis (the worst page, 3/10); curve.md is signature-only stubs (3/10); delaunay.md has 7 body-less `[STORE_OPS]`; fit.md's `FitStore` is documented-never-constructed. These are the brief's VERDICT proofs, disk-confirmed. None is a boundary edit for THIS conversion (RE-HOME is path-only), but they define the geometry campaign's true starting quality.

**F4 — Parallel rails / duplicate owners.** One crossing concern with 3 live inline copies (arrangement/offset/repair) despite intersect.md declaring itself sole owner; LM×2 (solver + fit) + Cholesky-wrapper×3; GWN×2 (index Barnes-Hut vs Vectors exact); soup-adapter×3; `BooleanReceipt` name×2 (arrangement vs receipts). All intra-kernel, resolved by the brief's V4/V5/V7 — not this conversion.

**F5 — Hardcoding.** decimate `shared==2` link-condition + hardcoded `1e-3`/`×10`/`1e12` in fit's LM; flatten `MassShift` 1e-9; the RhinoWIP path `/Applications/RhinoWIP.app` is the one sanctioned hardcode (env-overridable, `props:157`). The predicate constants (ErrorBound coefficients) are legitimate numeric-analysis permanence bounds, not rot.

**F6 — Strata violations.** `index.md:27` `using Rasm.Compute.Solver` + `ToAcceleration` returns Compute's `AccelerationStructure` — the kernel referencing an UPWARD stratum (`libs/.planning/architecture.md:35` forbids). Since Rasm.Compute is source-less, this is a design-intent edge that would not even compile today. V8 kills it (kernel emits its own node-link wire, Compute decodes). Also `pack.md:33` imports the phantom `Rasm.Geometry.Topology` (no such namespace exists).

**F7 — The unrouted 18th page.** README `[01]-[ROUTER]` lists 17 pages; the ARCHITECTURE `[01]` codemap and `[03]` namespace law omit `Parametric/curve.md` entirely (E5, brief V1: "Parametric absent from ARCH codemap — an undeclared sixth sub-domain"). When the conversion rebuilds the kernel index docs (ARCHITECTURE/README) for the new `.planning/` root, Parametric MUST enter the codemap + router + namespace law — or the page is orphaned in the new home too.

**F8 — Namespace-law schism carries into the re-home.** 14 pages declare a `Rasm.Geometry.*` namespace, 4 declare NONE (predicates/faults[root]/naming/reconciliation — actually faults declares root `Rasm.Geometry`; predicates/naming/reconciliation/receipts declare nothing). The fence namespaces (14: Encoding/Projection/Arrangement/Tessellation/Intersection/Offsetting/Parametric/Simplification/Fitting/Parameterization/Healing/Constraints/Spatial + root) contradict the ARCHITECTURE.md:64 5-namespace law. The RE-HOME does not fix this (content untouched) — it is the geometry campaign's V1. Flag: the new-home ARCHITECTURE.md must not re-assert the stale 5-namespace law.

---

## [07]-[THE STRUCTURAL RULINGS THE ARCHITECT MUST MAKE]

1. **KEEP `Rasm.csproj` (Rasm.Bim shape).** Retire the 31 `.cs` only; preserve the `.csproj` with its geometry `PackageReference`s + RhinoCommon reference machinery + `Rhino.Geometry` global usings as the documented implementation surface. Consumers (Rasm.Bim, Rasm.Rhino, Rasm.Grasshopper) that `ProjectReference ../Rasm/Rasm.csproj` keep referencing the (empty) assembly. No slnx row removed.

2. **Rule the orphaned-consumer disposition (F1) explicitly.** Rasm.Rhino + Rasm.Grasshopper source is frozen this run; decide whether they (a) join the planning-phase non-compiling majority (the likely intent, matching the 8 source-less packages + the AssayTestShell test posture), or (b) a minimal compiled kernel primitive survives to keep them building. The ledger evidence (every csharp package but these 3 is already source-less; test shells are spec-free) points to (a).

3. **Rule `Rasm.Domain.Op` + `GenerateUnionOpsAttribute` fate (F2).** This is the single cross-cutting hard edge (Geometry compose + Vectors/Analysis source + the `[Union]` generator emit + 11 Grasshopper aliases). If Domain retires wholesale to planning, the `[Union]` machinery has no compiled `Op` — decide whether the generator is re-pointed, `Op` survives minimally, or the consumers stop compiling.

4. **Rule the RE-HOME partition path.** `libs/csharp/Rasm/Geometry/.planning/{sub}/` → `libs/csharp/Rasm/.planning/`: does the `Geometry/` grouping survive as one sub-domain alongside newly-authored `Vectors/`/`Domain/` groupings, or do the 6 sub-domains attach directly to `.planning/` (risking collision with retired-source page groupings)? Rasm.Bim precedent = domain-group folders directly under `.planning/`.

5. **Rule the brief re-anchoring (§05).** The RASM-CS-GEOMETRY-BRIEF's `assay api over restored Rasm.Vectors` verification spine dies with the mature assembly. Decide the geometry-campaign sequencing relative to the conversion, and re-anchor the brief's mature-source member anchors to the `.api` catalogue + the newly-authored Vectors/Domain design-page fences. Path anchors (Geometry/.planning/-relative) all shift.

6. **Preserve the ONE-ASSEMBLY internal-visibility seam (§04).** flatten↔`IntrinsicMesh`, receipts↔`TopologyReceipt`/`MeshKernel.TopologyDetailed`, the whole `MeshEdit`/DEC substrate composition depend on Vectors + Geometry being co-assembly. Keep the converted kernel a SINGLE `Rasm.csproj`; do not fragment into per-sub-domain assemblies.

7. **Rebuild the kernel index docs for the new root (F7/F8).** ARCHITECTURE.md `[01]` codemap + `[02]` seams + `[03]` namespace law and README `[01]` router must (a) add the unrouted `Parametric/curve.md`, (b) update every `Geometry/...` seam anchor to the new path, (c) NOT re-assert the stale 5-namespace law, (d) carry the retired Vectors/Domain capability as new design-page groupings in the codemap. The `[02]-[SEAMS]` block (12 outbound Geometry→sibling seams: pack→AppHost, intersect/view/flatten/arrangement→Fabrication, reconciliation→Persistence + python/ts, index→Compute) re-anchors from `Geometry/...` to the new path.

8. **Confirm Analysis-delete is clean (headline).** No Geometry page imports Analysis; only Rasm.Rhino (Commands/Context.cs, UI/Overlay.cs) + 2 cs-analyzer vocabulary strings consume it. Deleting Analysis touches zero Geometry content and only the (frozen) Rasm.Rhino source + the analyzer's data strings.
