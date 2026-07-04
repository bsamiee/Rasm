# W4 Rasm b2 — grounding dossier

Batch: `Parametric/panelize.md` [new], `Parametric/patternmap.md` [new], `Drawing/view.md` [rebuild], `Drawing/pack.md` [rebuild].
Primary extracts only; every member/claim carries a `file:line` anchor. No doctrine digest, no removal framing.

## [00]-[VERIFICATION_METHOD]

`uv run python -m tools.assay api` is LIVE (RC 0). Members confirmed by direct decompile query:
- `System.Numerics.Tensors.TensorPrimitives` — score 100, `tfm: net10.0`, `source_id: System.Numerics.Tensors`, `version 10.0.9`, asm `~/.nuget/packages/system.numerics.tensors/10.0.9/lib/net10.0/System.Numerics.Tensors.dll` (assay `query --key System.Numerics.Tensors --symbol TensorPrimitives`).
- `System.Numerics.Tensors.ReadOnlyTensorSpan` — decompiled, `tfm: net10.0` (same source).
- `QuikGraph.UndirectedGraph` — decompiled, `tfm: netstandard2.0`, asm `~/.nuget/packages/quikgraph/2.5.0/lib/netstandard2.0/QuikGraph.dll` (assay `query --key quikgraph --symbol UndirectedGraph`).
Exact member SIGNATURES below are quoted from the freshly assay-derived `.api` catalogs (dated Jul 3-4); the DECISION independently cross-verifies the tensor lines (`api-tensors.md:14,29,175-176`, quoted in DECISION Wave-4 row 28).

## [01]-[LS_INVENTORIES]

Shared substrate tier `libs/csharp/.api/` (31 catalogs):
`api-csparse.md api-extensions-ai.md api-generator-equals.md api-grpc-aspnetcore.md api-grpc-client-web.md api-grpc-client.md api-grpc-common.md api-grpc-core-api.md api-grpc-tools.md api-hashing.md api-highperformance.md api-hybrid-cache.md api-jsonpatch.md api-languageext.md api-mapperly.md api-mathnet-numerics.md api-mathnet-providers.md api-messagepack.md api-nodatime-protobuf.md api-nodatime-stj.md api-nodatime.md api-protobuf.md api-quikgraph.md api-redaction.md api-system-configuration.md api-tensors.md api-thinktecture-json.md api-thinktecture-messagepack.md api-thinktecture-runtime-extensions.md api-unicolour.md api-unitsnet.md`

Folder tier `libs/csharp/Rasm/.api/` (11 catalogs):
`api-bigrational.md api-doubledouble.md api-gshark.md api-hashing.md api-kdtree.md api-manifold.md api-mathnet-numerics.md api-miconvexhull.md api-peteronumbers.md api-rhino.md api-tensors.md`

Doctrine root `docs/stacks/csharp/` (8 root pages + `domain/`):
`algorithms.md boundaries.md domain/ language.md rails-and-effects.md README.md shapes.md surfaces-and-dispatch.md system-apis.md`
`docs/stacks/csharp/domain/` (14 shards): `compute.md concurrency.md data-interchange.md diagnostics.md durability.md interaction.md persistence.md postgres.md README.md resilience.md runtime.md transport.md validation.md visuals.md`

Target + sibling LOC (`loc`): `Drawing/view.md` 380, `Drawing/pack.md` 398, `Parametric/curve.md` 157, `Parametric/locate.md` 470, `Parametric/projections.md` 219. Parametric dir on disk today: `curve.md locate.md projections.md` (panelize/patternmap are NET-NEW — E2). Drawing dir: `pack.md view.md`.

## [02]-[API_MEMBER_EXTRACTS] — the underutilized capabilities, quoted with anchors

### `libs/csharp/.api/api-tensors.md` (System.Numerics.Tensors 10.0.9, net10.0)
- `api-tensors.md:14` — "abi: `Tensor<T>`/`TensorSpan<T>`/`ReadOnlyTensorSpan<T>` are `ref struct`-adjacent strided views over `nint`-indexed memory; `TensorPrimitives` operators are `static` generic-math methods … the destination span is caller-owned".
- `api-tensors.md:29` — row `[06]` "`ReadOnlyTensorSpan<T>` | span view | reads tensor data".
- `api-tensors.md:175` — row `[09]` "`ConvertToHalf` | conversion call | `(ReadOnlySpan<float>, Span<Half>)` narrows to `Half`".
- `api-tensors.md:176` — row `[10]` "`ConvertToSingle` | conversion call | `(ReadOnlySpan<Half>, Span<float>)` widens from `Half`".
- `api-tensors.md:152` — row `[02]` "`MaxMagnitude` | reduction call | reduces by absolute extremum".
- `api-tensors.md:100-102` — `TensorPrimitives.Subtract`/`Multiply`/`Divide` "primitive call | computes elementwise op".
- `api-tensors.md:131` — row `[02]` "`Abs` | primitive call | computes elementwise op".
- `api-tensors.md:206` — "`IsNaN` / `IsFinite` | predicate call | `(x, Span<bool> dst)` … full family is `IsNaN`/`IsFinite`/`IsInfinity`/… each with `*All`/`*Any` aggregate forms".
- `api-tensors.md:216` — "`TensorMarshal.CreateReadOnlyTensorSpan` | factory call | wraps raw read memory".
- `api-tensors.md:238-239` — "[ABSENT_OPERATORS]: `TensorPrimitives` exposes no `Normalize` operator; vector normalization composes from `Norm` (or `SumOfSquares` + `Sqrt`) followed by `Divide`".
- `api-tensors.md:248` — "[INTEGRATION_STACKING] Span-feed, not re-pack: the geometry kernel's struct-of-arrays coordinate buffers … feed `TensorPrimitives` operators DIRECTLY as `ReadOnlySpan<T>` — and `TensorMarshal.CreateTensorSpan(ref data, dataLength, lengths, strides, pinned)` wraps a pinned raw buffer as a `TensorSpan<T>` view without a copy. A second tensor-shaped re-pack … is the rejected double-layout."
- `api-tensors.md:251` — "Not the exact-predicate path: `TensorPrimitives` is IEEE-754 floating SIMD … NEVER the substrate of an exact geometric predicate."

### `libs/csharp/.api/api-quikgraph.md` (QuikGraph 2.5.0, netstandard2.0)
- `api-quikgraph.md:71` — "`UndirectedGraph<TVertex, TEdge>` | the undirected adjacency graph; `AdjacentDegree(v)`/`AdjacentEdge(v,i)`, `TryGetEdge(source, target, out edge)` — the symmetric form the MEP port-adjacency closure, the kernel kNN normal-orientation MST, and the connected-components read".
- `api-quikgraph.md:57` — "`SEdge<TVertex>` | the default struct directed edge; `Source`/`Target`, ctor `SEdge(source, target)` — the value-type edge with no allocation".
- `api-quikgraph.md:105` — "`TreeBreadthFirstSearch` | `(this IVertexListGraph<TVertex, TEdge> graph, TVertex root)` → `TryFunc<TVertex, IEnumerable<TEdge>>` | runs a BFS from `root`, returns the path accessor …".
- `api-quikgraph.md:118` — "`StronglyConnectedComponents` / `WeaklyConnectedComponents` … labels each vertex with its component index, returns the count".
- `api-quikgraph.md:132` — "`MinimumSpanningTreeKruskal` / `MinimumSpanningTreePrim` | `(this IUndirectedGraph<TVertex, TEdge> graph, Func<TEdge, double> edgeWeights)` → `IEnumerable<TEdge>`".
- `api-quikgraph.md:148` — kernel precedent: "`NeighborKernel.OrientNormals` folds the `GraphOf` kNN rows into a transient `UndirectedGraph<int, SEdge<int>>` … the graph is transient per `OrientNormals` call, never a stored field".
- `api-quikgraph.md:162-163` — "[LOCAL_ADMISSION] … a hand-rolled topological sort, BFS/DFS reachability fold … is the rejected form — those are exactly the walks this owner collapses".

### `libs/csharp/.api/api-highperformance.md` (CommunityToolkit.HighPerformance 8.4.2)
- `api-highperformance.md:35` — "`MemoryOwner<T>` | pooled owner | `IMemoryOwner<T>` over a rented array; heap-allocatable, async-safe; `.Memory`/`.Span`/`Slice`".
- `api-highperformance.md:37` — "`ArrayPoolBufferWriter<T>` | pooled writer | `IBuffer<T>`+`IBufferWriter<T>`+`IMemoryOwner<T>` over pooled storage; the codec-emit sink".
- `api-highperformance.md:26` — "`Span2D<T>` | span view | addresses dense planes".
- `api-highperformance.md:103` — "`AsBytes<T>(this Span<T>) where T : unmanaged` | reinterprets a blittable span as raw `Span<byte>`".
- `api-highperformance.md:104` — "`Cast<TFrom, TTo>(…) where TFrom, TTo : unmanaged` | reinterprets a blittable `Span`/`Memory` to another width".
- `api-highperformance.md:54` — "`ParallelHelper` | parallel root | partitions batch work".
- `api-highperformance.md:171,193` — content identity routes `System.IO.Hashing` `XxHash128.HashToUInt128`, never a package-local digest (`HashCode<T>.Combine` REJECTED).

### `libs/csharp/README.md` (admission facts)
- `README.md:121-122` — `[NUMERIC_SUBSTRATE]`: `System.Numerics.Tensors`; `CommunityToolkit.HighPerformance — Span2D/MemoryOwner<T>/ParallelHelper on the SoA build arenas, frozen or partition-disjoint spans only`.
- `README.md:124-125` — `[GRAPH_ALGORITHM]`: `QuikGraph` — Prim-MST normal-orientation + constraint-graph decomposition (admitted core substrate).

## [03]-[DECISION_WAVE4_PAGE_ROWS] — `RASM-CS-GEOMETRY-DECISION.md` `[02]-[PAGE_SET]` (sed 91-108)

- Row 25 `Parametric/panelize.md` NEW → new: "Parametric · `Rasm.Geometry.Parametric` · 2449 (Panel stage). Cross-field-guided panelization: ONE mapper composing landed `fields.md` `CrossField` + `segment.md` Knöppel globally-optimal fields/stripes + the remesh substrate (row 19); `int symmetry` owns the quad/diagrid/tri family AS DATA; `sample.md` CCVT/blue-noise seeds panel distributions; panel families are policy rows; OUTPUT CONTRACT — UV-provenanced surfaces in, panel graphs + per-panel PLACEMENT FRAMES out as a kernel-owned SoA wire on `PanelResult`". Entry: `Fin<PanelResult> Panelization.Apply(PanelOp, Op? key = null)`. Seams out: remesh (W4 backward), surface (UV — backward), `fields.md`/`segment.md`/`sample.md` (landed), faults; in: Generation gate.
- Row 26 `Parametric/patternmap.md` NEW → new: "Parametric · `Rasm.Geometry.Parametric` · 2449 (Pattern stage). Pattern-to-surface instancing: landed `geodesics.md` tangent LOG/EXP maps + 2D symmetry-GROUP rows (wallpaper groups as data) → surface-mapped INSTANCE STREAM; every instance carries a frame parallel-transported by the landed vector-heat transport (position without orientation is half an instance); the Generation PATTERN/TILING plane's exact input". Entry: `Fin<InstanceStream> Patterning.Apply(PatternOp, Op? key = null)`. Seams out: surface (UV pullback — backward), `geodesics.md`/transport lanes (landed), faults; in: Generation gate.
- Row 27 `Drawing/view.md` REBUILD (ground-up) → rebuild: "Drawing · `Rasm.Geometry.Projection` · 2436-2439. The visibility engine made REAL — ruled choice: EXACT ANALYTIC APPEL QUANTITATIVE-INVISIBILITY … invisibility counts updated ONLY at silhouette crossings computed through the `[V4]` crossing lattice — no sampling, no `SampleStep`, zero missed occluder edges (the E1 failure class dies by construction); the dead `BspNode`/`Partition`/`Split`/`Kill`/`FreeList` apparatus (`:263-300`) DIES … the phantom `PaintBackToFront` prose (`:16`) dies; `ToPolylines` chains by successor-linked connectivity (never kind-key concat `:130-135`, never visible+hidden merged); `ViewKind` columns RULED: `EmitsHidden` RETURNS as a consulted op-case data column …, `NeedsBsp` DIES …; `OccludedAt`/`Clip`/`OcclusionBias` die with sampling; soup round-trip collapses onto edit.md; SALVAGE verbatim: exact `FacesOppose` silhouette locus (`:235-241`), `Cut` Section delegation to `IntersectOp.PlaneMesh` (`:349-353`); … polygon fill routes arrangement `PlanarOverlay`". Entry: `Fin<DrawingProjection> View.Apply(ViewOp, Op? key = null)` (HiddenLine · Silhouette · Section · Creases). Seams out: intersect (Section + silhouette crossings), index (ray/overlap acceleration for QI seeding), predicates (`Orient3D` signs), edit (soup), arrangement (`PlanarOverlay` fill), faults; in: Fabrication (`FAB:33` Posting BSP HLR DIES), AppUi (`ARCH:91`).
- Row 28 `Drawing/pack.md` REBUILD → rebuild (two-touch; W1 phantom-kill pre-applied): "Drawing · `Rasm.Geometry.Encoding` · 2444-2447. Residency encoding made REAL: `EncodedStore.Payload` becomes dtype-STRIDED `byte[]` with per-channel byte descriptors (the `:126-129` always-`float[]` store dies); the `System.Numerics.Tensors` seam survives as descriptor-dispatched typed views (`ReadOnlyTensorSpan<float>`/`ReadOnlyTensorSpan<Half>` selected on the descriptor `Dtype` row — element-type-generic span verified at `api-tensors.md:14,29,175-176`; `Half` HALVES residency for real; the `:53` widen-back `Pack` dies); … `PackKind` gains `field` · `toolpath` case rows …; the `:3` `OrientedNormals` provenance corrects to cloud→neighbors; PRESERVED VERBATIM: the `SampleDetailed` composition (`pack.md:346,463`…) and the `VectorCloudMetric.OrientedNormals` composition (`:463`)". Entry: `Fin<EncodedGeometry> Encode.Apply(PackOp, Op? key = null)`. Seams out: reconciliation (`Encode` keys), naming (`CanonicalTopology`), `fields.md` (`SampleDetailed`), `identity.md` (cloud keys), faults; in: AppHost (`ARCH:92`), Compute residency (`ARCH:93`).

## [04]-[BRIEF_ANCHORS] — `RASM-CS-GEOMETRY-BRIEF.md`

- `RASM-CS-GEOMETRY-BRIEF.md:150` (E1): "Dead HLR engine | `view.md:302` `Paint` never reads `bsp`; `:263-300` Partition/Split built-discarded; `:320-347` uniform `SampleStep` sampling …".
- `RASM-CS-GEOMETRY-BRIEF.md:151` (E2): "Gate-plane absence | `RASM-GENERATION-SPEC.md:179,226` gate roster; zero subdivision/develop/panelize/pattern-map owners corpus-wide (grep; …" — the net-new scope for panelize/patternmap.
- `RASM-CS-GEOMETRY-BRIEF.md:160` (E11): "Dead/misleading carriers | `BspNode` apparatus (E1); …".
- `RASM-CS-GEOMETRY-BRIEF.md:188` (evidence register, pack row): "Drawing/pack | 7 | 9.5 | real dtype-strided residency or honest re-scope; phantom import dead; the landed `SampleDetailed` (`pack.md:346`) and `Orie…`".
- `RASM-CS-GEOMETRY-BRIEF.md:94` (V2, sed): "The campaign-defining coverage verdict: `Parametric/` grows into the surface-development tier that closes the Generation KERNEL GATE (`RASM-GENERATION…`".
- `RASM-CS-GEOMETRY-BRIEF.md:98` (V3, sed): "`Drawing/view.md`'s visibility engine is rebuilt ground-up — the page cannot be polished because the advertised engine does not exist (`[VERDICT]` pro…".

## [05]-[ARCHITECTURE_ANCHORS] — `libs/csharp/Rasm/ARCHITECTURE.md`

- `ARCHITECTURE.md:36-39` — Parametric folder codemap: `Curve.cs` (GShark NURBS), `Projections.cs` (CurveProjection/SurfaceProjection/SurfaceSpace/shape-operator/pose-slerp), `Locate.cs` (Locator/LocationValue/Division). panelize/patternmap are net-new files.
- `ARCHITECTURE.md:64-66` — Drawing folder codemap: `View.cs` (predicate-exact HLR ViewOp → DrawingProjection), `Pack.cs` (PackOp geometry-encoding → EncodedGeometry).
- `ARCHITECTURE.md:93` — seam `Drawing/View.cs → csharp:Rasm.Fabrication/Posting [PROJECTION]: DrawingProjection / HLR visible/hidden segments`.
- `ARCHITECTURE.md:94` — seam `Drawing/View.cs → csharp:Rasm.AppUi/Render [PROJECTION]: DrawingProjection / drafting-sheet layout`.
- `ARCHITECTURE.md:95` — seam `Drawing/Pack.cs → csharp:Rasm.AppHost/Runtime [WIRE]: EncodedGeometry / PackOp.Apply channel discriminant`.
- `ARCHITECTURE.md:96` — seam `Drawing/Pack.cs → csharp:Rasm.Compute/Tensor/residency [WIRE]: EncodedGeometry wrapped as EncodedTensor — residency view, never a re-pack`.
- `ARCHITECTURE.md:111,113` — namespace map: `Rasm.Vectors` frozen by `Parametric/Projections`; `Rasm.Geometry.*` frozen by `Drawing/*` (settled robust-core; geometry campaign owns namespace reconciliation → `Rasm.Geometry.Projection`/`.Encoding`/`.Parametric` ratified by DECISION V1).

## [06]-[COMPOSED_OWNER_ANCHORS] — internal owners the new pages compose

panelize:
- `Processing/segment.md:744` — "Entry: `SegmentKernel.CrossFieldAt(space, symmetry, constraints, cones, sample, key)` → `Fin<Vector3d>` (the frozen `VectorField.CrossField` delegat…" ; `segment.md:763` `ComputeCrossField(...)`; `segment.md:12` "[05]-[DIRECTION_FIELDS]: Knöppel GODF cross fields (smoothest eigenvector / constrained solve / cone holonomy) + stripe patterns."
- `Spatial/fields.md:398` — "// BiotSavart/Saddle/CrossField/Hodge/VectorHeat/GeodesicTangent/TangentLogMap — admitted payloads throughout." ; `fields.md:425` `crossFieldCase … SegmentKernel.CrossFieldAt(space, symmetry, …)`.
- `Processing/sample.md:14` — "Owner: `SampleKind` `[Union]` — `Explicit(Seq<Point3d>)` · `PoissonDisk(...)` · `Farthest(...)` …" (CCVT/blue-noise seed source); `sample.md:25` `SampleKind.PowerCcvt(...)`.
- Row 19 `Processing/remesh.md` (DECISION): quad remesh "composing landed `segment.md` Knöppel cross-fields/stripes"; "panelize composes it".

patternmap:
- `Processing/geodesics.md:785-788` — `TangentLogMapAlgorithm` `[SmartEnum<int>]`: `VectorHeatApproximate`(0), `ExactStraightestExp`(1), `ExactWindowPropagation`(2).
- `Processing/geodesics.md:825` — "`internal static Fin<Vector3d> VectorHeatAt(MeshSpace space, Seq<(int Vertex, Vector3d Direction)> sources, double time, Point3d sample, Op key)`" (Sharp-Soliman-Crane parallel transport of tangent data — `geodesics.md:777`).
- `geodesics.md:11` — "[04]-[TANGENT_TRANSPORT]: vector-heat parallel transport (connection + magnitude + indicator solves) + the `TangentLogMapAlgorithm` three-arm log-ma…".

Parametric siblings (folder position for both new pages):
- `Parametric/projections.md:26` — `SurfaceSpace` `[BoundaryAdapter]` validated `Surface`+`Context`, `Sample<TOut>(SurfaceProjection, u, v, Op)`. `Rasm.Vectors` namespace.
- `Parametric/curve.md:3` — `Parametric` `[Union]` (Curve/Surface) over GShark; DECISION row 20 vendors GShark into `Parametric/nurbs.md` and DELETES `api-gshark.md` on landing — panelize/patternmap compose the internal `surface.md`/`nurbs.md` owners for UV, NOT `api-gshark` directly.
- Row 22 `Parametric/surface.md` (DECISION): "UV-PROVENANCED tessellation sampler … in: develop, panelize, patternmap (UV-provenanced input law: a world-space polyline with no surface binding cannot feed them)".

## [07]-[CURRENT_BODY_DEFECT_ANCHORS] — the weak fences (buildout targets, never removal)

`Drawing/view.md` (dead/illusory engine — E1):
- `view.md:196-203` `Projection.Apply` fold; `view.md:260-263` `Render` calls `Partition(...).Map(bsp => Paint(soup, edges, camera, bsp, index, policy, emitHidden))`.
- `view.md:304-320` `Paint(...)` signature takes `BspNode bsp` but the body iterates `edges` → `Clip(...)` → `OccludedAt(...)` which reads `index` (SpatialIndex BVH), NEVER `bsp` — the BSP is built (`:265-302` `Partition`/`Split`) and DISCARDED.
- `view.md:322-339` `Clip(...)` — `int samples = Math.Max(1, (int)((screenB - screenA).Length / policy.SampleStep))` uniform SampleStep marching (the sampled visibility, not Appel QI).
- `view.md:341-348` `OccludedAt(...)` — BVH ray per sample (the actual occlusion test).
- `view.md:130-138` `DrawingProjection.ToPolylines()` — `Visible.Append(Hidden).GroupBy(static s => s.Edge.Key).Map(...)` kind-key concat + visible/hidden merged (the DECISION-killed form; rebuild → successor-linked connectivity).
- `view.md:217-234` `Silhouettes(...)` — hand-rolled `Dictionary<(int,int), List<int>>` incidence; `view.md:305-306` `List<ProjectedSegment>` mutable accumulators.
- SALVAGE anchors: `view.md:236-242` `FacesOppose` exact `camera.SideOf`→`Predicate.Orient3D`; `view.md:351-355` `Cut` → `Intersection.Apply(new IntersectOp.PlaneMesh(...))`.

`Drawing/pack.md` (illusory residency — brief score 7):
- `pack.md:50-56` `ChannelDtype.Pack(float) => float` (`float16: (float)(Half)value`) + `pack.md:56` `Unpack(float stored) => stored` — Float16 "quantization" WIDENS back to float32; no residency saving. The `:53` widen-back the DECISION kills.
- `pack.md:120` `EncodedGeometry(... ReadOnlyMemory<float> Payload ...)`; `pack.md:126-131` `EncodedStore(int Count, float[] Payload, ...)` `Reserve(...) => new float[floats]` — always float32 (the store that DIES → dtype-strided `byte[]`).
- `pack.md:211-219` `ChannelError(float[] raw, ReadOnlySpan<float> stored, ...)` scalar `for` max-|Δ| loop; `pack.md:364-371` `Normalize(float[])` scalar max-abs + divide loop; `pack.md:373-428` `PackPoints`/`PackVertices`/`PackNormals`/`PackCells`/`PackVectors` scalar `for` pack loops — none composes `TensorPrimitives`.
- SALVAGE anchors: `pack.md:341-351` `MeshScalarField` → `field.SampleDetailed(...)`; `pack.md:276-283` `ReadNormal` → `VectorCloudMetric.OrientedNormals` via `VectorIntent.Cloud`; `pack.md:221-228` `ContentHash` → `NamingHashOps.Encode(CanonicalTopology.OfMesh(...))` (one hash, preserved).
