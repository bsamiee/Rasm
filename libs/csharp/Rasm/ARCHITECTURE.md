# [RASM_ARCHITECTURE]

The domain map of `Rasm` — the KERNEL RhinoCommon-aware geometry/numeric kernel, an ordinary planning-scoped package whose whole design corpus lives under one `.planning/` root in nine sub-domain folders. Namespace mirrors folder path (`[03]`). The kernel remains RhinoCommon-aware end to end per the Tier-0 universal-vs-capture law; the pure-numeric floor is host-neutral-shaped without minting a host-free assembly.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm/
├── Domain/                  # The kernel substrate floor every sibling composes
│   ├── Rails.cs             # Op operation key, Expected/Fault union, Catch/Side rail, Lease<T>, IValidityEvidence fold, generator contracts
│   ├── Context.cs           # Tolerance/units value objects + the immutable Context bundle; Of(RhinoDoc) boundary adapter
│   ├── Identity.cs          # ContentHash.Of seed-zero XxHash128 federation key + the one splitmix64 derivation owner
│   ├── Validation.cs        # Requirement/Check readiness algebra + the one OpAcceptance validity oracle + admission vocabulary
│   ├── Normalization.cs     # Topology/Kind taxonomy + capability web, CurveForm, the coercion lattice, one projection carrier
│   ├── Evaluation.cs        # ClosestHit receipt + ClosestOf polymorphic evaluation, frames, sampling, signed distance
│   └── Stats.cs             # ScalarMetric vocabulary, Welford Stat, banded Extrema, Distribution, SampleMoment covariance
├── Numerics/                # Exact-predicate floor + host-neutral-shaped numerics
│   ├── Predicates.cs        # PrecisionTier ladder (double→ddouble→Expansion→Fraction) exact + implicit-point predicates
│   ├── Faults.cs            # Consolidated band-2400 GeometryFault family
│   ├── Atoms.cs             # Vector-algebra primitive floor + the AtomProjection/ProjectionRow projection dispatch
│   ├── Matrix.cs            # Dense/sparse/complex algebra over MathNet+CSparse; CholeskySparse, GaugePolicy, LOBPCG MatrixKernel
│   ├── Integrate.cs         # IntegratorKind 9-tableau RK floor, ButcherTableau moment validation, dense output, FieldIntegrator
│   ├── Spectral.cs          # DiscreteCalculus DEC bundle, SpectralBasis/SpectralFilter algebra, SpectralDescriptor
│   └── Calculus.cs          # Six-axis central-difference stencil, FieldNoise lattices, weight-kernel/falloff math
├── Spatial/                 # Proximity, clouds, neighborhoods, transport, fields, acceleration, naming
│   ├── Index.cs             # SAH-BVH/Morton-octree SpatialIndex over NodeStore with query/refit fold
│   ├── Naming.cs            # TopoName lineage/NameTable/Track re-anchor
│   ├── Reconciliation.cs    # Reconciliation.Apply(ReconcileOp) fence: EncodeForm {Mesh·Cloud·Parametric} → GeometryHash + NamingHash reconcile
│   ├── Support.cs           # SupportProjection 14-case gated projection over the SupportSpace boundary adapter
│   ├── Cloud.cs             # VectorCloud union, 30-case VectorCloudMetric, CloudKernel PCA, realized hull rail
│   ├── Neighbors.cs         # The one kNN/radius/graph substrate: RTree + KDTree, MST normals, curvature, the one RMF owner
│   ├── Transport.cs         # Log-domain Sinkhorn OT (balanced/unbalanced/debiased) + SinkhornPlan projections
│   └── Fields.cs            # ScalarField/VectorField/TensorField unions, typed SdfKind primitives, BlendKind, SampleDetailed seam
├── Parametric/              # The vendored NURBS engine + host-neutral op tier, evaluation + location
│   ├── Nurbs.cs             # THE vendored NURBS engine (MIT set owned in-kernel): Nurbs.Of(NurbsWire) one four-shape admission, KnotVector normalized-clamped algebra, De Boor + fixed-convention RationalDerivatives, Bezier-decomposed arc-length, Wang-2008 RMF, Piegl-Tiller curve/surface fits, fundamental forms, ToEncodeForm identity projection
│   ├── Curve.cs             # ParametricOp eight-case op rail (Evaluate·Measure·Divide·Stations·Split·Reconstruct·Offset·Intersect2D) over NurbsForm.Curve; StationField SoA producer, promoted deviation-refined Offset, Fill overlay delegation
│   ├── Surface.cs           # SurfaceOp six-case op rail (Tessellate·Isolines·Geodesics·NormalOffset·CurvatureSample·Pullback); UvTessellation the tier's UV-provenance carrier, real NURBS normal offset, kd-tree-seeded batch pullback
│   ├── Subdivide.cs         # Stencil-row subdivision: Catmull-Clark/Loop as SubdivisionScheme data rows over ONE sparse-operator refinement fold, semi-sharp creases + region closure, Stam eigenbasis limit evaluation
│   ├── Develop.cs           # Guaranteed-isometric developable strips: MMP-exact rails, torsal ruling solve, exact rigid unroll, ddouble isometry witness, ChartAtlas emission with DevelopmentReceipt beside
│   ├── Panelize.cs          # Cross-field-guided panelization: Lattice (remesh QuadProvenance substrate) + Seeded (sample suite, geodesic-Voronoi cells) families, per-panel placement frames, planarity acceptance, PanelField SoA wire
│   ├── Patternmap.cs        # Wallpaper-group pattern-to-surface instancing: 17 theorem-closed Seitz rows over ONE orbit fold, PL log-map inversion with flip/clip censuses, vector-heat-transported instance frames
│   ├── Projections.cs       # CurveProjection/SurfaceProjection selectors, the one shape-operator + pose-slerp owners, SurfaceSpace
│   └── Locate.cs            # Locator/LocationValue/Division location algebra with curvature extrema
├── Meshing/                 # Mesh substrate + construction lattice
│   ├── Delaunay.cs          # Constrained Bowyer-Watson Delaunay/tetrahedralization on InCircle/InSphere
│   ├── Arrangement.cs       # Managed exact boolean/overlay cell-complex retiring the native CSG gate
│   ├── Intersect.cs         # Predicate-exact IntersectOp crossing lattice
│   ├── Slice.cs             # Slicing.Apply slice-stack fold: LayerPlan height-law rows over ONE March integrator, oriented contours + typed open chains, exact-parity nesting forest, SliceStack five-channel SoA wire
│   ├── Offset.cs            # Aichholzer-Aurenhammer skeleton/medial/minkowski OffsetOp
│   ├── Skeleton.cs          # Au-2008 MCF 3D curve-skeleton: implicit contraction over the MeshEdit arena, cost-ordered collapse to 1D, Kruskal tree extraction, CurveSkeleton SoA wire composing offset's clearance family
│   ├── Mesh.cs              # MeshSpace snapshot handle, LaplacianCache, IntrinsicMesh + MeshAdjointSnapshot, one cotangent owner, power diagram
│   ├── Edit.cs              # MeshEdit single-writer SoA build arena: one polymorphic Of (space|soup), weld kernel + knob
│   ├── Dec.cs               # AssembleDecOperators, CR connection heat, CDS holonomy, harmonic basis + Hodge decomposition
│   └── Reconstruct.cs       # RBF/MLS/Levin/APSS/Poisson kernels, the unified signed-heat spine, mesh-SDF, iso-extraction
├── Processing/              # Algorithm pipelines over the floors
│   ├── Repair.cs            # HealOp repair algebra + Heal.Repair session fold
│   ├── Receipts.cs          # Typed RebuildReceipt chain + ManifoldStatus + HealSession/RebuildLog
│   ├── Decimate.cs          # Garland-Heckbert QEM SimplifyOp decimation
│   ├── Remesh.cs            # Remeshing.Apply two-row rewrite: Botsch-Kobbelt isotropic + cross-field-guided quad extraction; exact projected-convexity flip gate, RemeshTrace receipt, QuadProvenance the panelize substrate
│   ├── Flatten.cs           # LSCM/ARAP/BFF ParamOp UV-flattening over the DEC substrate
│   ├── Intent.cs            # VectorIntent consumer rail: Project<TOut>(Context, Op?) dispatch composing every owner
│   ├── Sample.cs            # SampleKind union (Bridson…BNOT power-CCVT) + SampleKernel domain dispatch
│   ├── Extract.cs           # ExtractionDomain ingress, ContourPolicy native-first sectioning, Extraction union
│   ├── Flow.cs              # Termination 6-stop union + FlowKernel.Trace<TOut> dense-output tracing
│   ├── Register.cs          # AlignKind 6-variant ICP dispatcher behind one AlignmentPolicy
│   ├── Geodesics.cs         # Heat-method + MMP geodesics, log/exp maps, vector-heat transport, MCF
│   └── Segment.cs           # HKS/WKS descriptors, feature edges, MeshSegmentation union, cross-fields/stripes, host remesh capture
├── Solving/                 # Nonlinear least-squares owners over the matrix floor
│   ├── Solver.cs            # Lm.Minimize ILmModel λ-ladder (the ONE damped Gauss-Newton) + Constraint solver, island decomposition, DofReport/DofAnalysis
│   └── Fit.cs               # MLESAC FitOp primitive-fit (PROSAC/NAPSAC draws over the kd-tree lane) + FitModel : ILmModel orthogonal-distance refine
├── Drawing/                 # Kernel-quality 2D drawing-geometry producers
│   ├── View.cs              # Predicate-exact hidden-line/silhouette ViewOp returning DrawingProjection
│   └── Pack.cs              # Canonical PackOp geometry-encoding lattice returning EncodedGeometry
└── Analysis/                # The measured-query public entry
    ├── Query.cs             # AnalysisQuery request algebra, Operation<TGeometry,TOut>, Env reader, Analyze facade
    ├── Measure.cs           # Measure mass-property union, Bounds enclosing fits, ConformanceMetric residuals
    ├── Inspect.cs           # Topologies genus/Euler folds + Meshes defect/quality capture
    ├── Select.cs            # Curves/Faces/Points selection unions over the edge taxonomy + PCA spread
    └── Relations.cs         # 25-row Rhino intersection lattice, IntersectionHit/RayQuery, deviation, classification
```

## [02]-[SEAMS]

```text seams
Domain/Identity.cs        →  csharp:Rasm.Element/Projection/address       # [CONTENT_KEY]: the kernel seed-zero XxHash128 ContentHash.Of entry the Rasm.Element seam composes for every NodeId/ContentAddress — ONE hasher, no second hasher
Domain/Identity.cs        →  csharp:Rasm.Persistence/Element/codec        # [CONTENT_KEY]: ContentAddress composes the kernel seed-zero XxHash128 entry — no second hasher at the codec
Domain/Identity.cs        →  csharp:Rasm.Compute/Model/identity           # [CONTENT_KEY]: ModelIdentity.Checksum composes ContentHash.Of — the ONE federation hasher, never a per-call-site XxHash128
Spatial/Reconciliation.cs →  csharp:Rasm.Persistence/Query/topology       # [CONTENT_KEY]: adjacency-derived GeometryHash canonical-byte content-identity hashed through the kernel Domain/Identity seed-zero entry; geometry crosses the seam by content-hash ONLY, read never re-minted
Spatial/Reconciliation.cs ⇄  python:runtime/evidence/identity             # [CONTENT_KEY]: canonical-byte content-identity reproducing the one Domain/Identity seed (XxHash128 seed-zero)
Spatial/Reconciliation.cs ⇄  typescript:core/value/contentKey             # [CONTENT_KEY]: content-hashing wasm reproducing the one Domain/Identity seed (XxHash128 seed-zero)
Numerics/Spectral.cs      ⇄  csharp:Rasm.Compute                          # [SHAPE]: DiscreteCalculus DEC operator bundle — the frozen adjoint-carrier shape
Meshing/Mesh.cs           →  csharp:Rasm.Compute                          # [SHAPE]: MeshAdjointSnapshot adjoint handle over the cached DiscreteCalculus
Spatial/Index.cs          ⇄  csharp:Rasm.Fabrication/Toolpath/guard       # [SHAPE]: SpatialIndex BVH broad-phase keep-out prune
Spatial/Index.cs          ⇄  csharp:Rasm.Fabrication/Posting/projection   # [SHAPE]: SpatialIndex BVH broad-phase
Spatial/Index.cs          →  csharp:Rasm.Compute                          # [WIRE]: Spatial.Apply Wire case emits; Compute decodes
Meshing/Intersect.cs      →  csharp:Rasm.Fabrication/Posting              # [WIRE]: IntersectResult / PlaneMesh section curve
Meshing/Slice.cs          →  csharp:Rasm.Fabrication/Toolpath             # [WIRE]: SliceStack five-channel forest wire (layers · contours · nesting parent/child · open chains · elevations) — the FAB:48 re-route realized; the in-folder planar section dies
Meshing/Slice.cs          →  csharp:Rasm.Compute                          # [WIRE]: AtElevations story-elevation contours through the SliceStack wire — the circulation decoder (RASM-CS-COMPUTE [V12]a)
Meshing/Offset.cs         ⇄  csharp:Rasm.Fabrication/Toolpath             # [SHAPE]: the ONE 2D/3D clearance family — Medial + Clearance(probe) radius payload; Toolpath/Skeleton.cs dies for Offsetting.Apply
Meshing/Skeleton.cs       →  csharp:Rasm.Fabrication/Toolpath             # [WIRE]: CurveSkeleton node/arc/radius SoA + Clearance(probe) — the 3D half of the same clearance family, composed from offset.md's rows
Processing/Remesh.cs      →  csharp:Rasm.Compute                          # [SHAPE]: Isotropic the named volumetric boundary-conditioning pre-step, decoded through the wire, never a Compute-side remesher (RASM-CS-COMPUTE [V7] recorded growth)
Meshing/Arrangement.cs    →  csharp:Rasm.Fabrication/Posting/projection   # [WIRE]: Arrangement Apply/ToMesh kept-cell boundary watertight outline
Numerics/Predicates.cs    ←  csharp:Rasm.Fabrication/Posting              # [WIRE]: Predicate.Orient2D/Orient3D exact verdict
Numerics/Predicates.cs    ←  csharp:Rasm.Compute/Solver/discretization    # [SHAPE]: CDTet exact gates — the public Predicate.Orient3D/InSphere verdicts satisfy by shape, never a Compute-side predicate mint
Drawing/View.cs           →  csharp:Rasm.Fabrication/Posting              # [PROJECTION]: DrawingProjection / HLR visible/hidden segments
Drawing/View.cs           →  csharp:Rasm.AppUi/Render                     # [PROJECTION]: DrawingProjection / drafting-sheet layout
Drawing/Pack.cs           →  csharp:Rasm.AppHost/Runtime                  # [WIRE]: EncodedGeometry / PackOp.Apply channel discriminant
Drawing/Pack.cs           →  csharp:Rasm.Compute/Tensor/residency         # [WIRE]: EncodedGeometry wrapped as EncodedTensor — residency view, never a re-pack
Processing/Flatten.cs     →  csharp:Rasm.Fabrication/Nesting/nfp          # [PROJECTION]: ChartAtlas / UV island layout + DistortionReceipt
Processing/Flatten.cs     →  csharp:Rasm.AppUi/Render                     # [PROJECTION]: ChartAtlas / texture UV channel
Parametric/Develop.cs     →  csharp:Rasm.Fabrication/Nesting/nfp          # [PROJECTION]: ChartAtlas unrolled isometric strips (same seam type as flatten) + DevelopmentReceipt isometry witness — the sheet/plywood/fabric acceptance evidence
Parametric/Nurbs.cs       →  csharp:Rasm.Generation                       # [WIRE]: Nurbs.Of(NurbsWire) public arbitrary-knot ingress — SpineRef surface resolution (G1); spec-decided consumer, folder not stood up (RASM-GENERATION-SPEC)
Parametric/Curve.cs       →  csharp:Rasm.Generation                       # [WIRE]: Stations (station, frame) StationField SoA over the SpineRef [T0,T1] window — the PathRow/Placement producer
Parametric/Surface.cs     →  csharp:Rasm.Generation                       # [WIRE]: UvTessellation / Isolines / Geodesics / NormalOffset gate items; the UV-provenance carrier the whole Parametric tier's consumers demand
Parametric/Subdivide.cs   →  csharp:Rasm.Generation                       # [WIRE]: region subdivision via SubdividePolicy.Region with sealed T-junction closure; quad-preserving publish
Parametric/Develop.cs     →  csharp:Rasm.Generation                       # [WIRE]: developable gate item — isometry-witnessed strips read off DevelopmentReceipt
Parametric/Panelize.cs    →  csharp:Rasm.Generation                       # [WIRE]: PanelField panel graph + per-panel placement frames — the panelization gate item
Parametric/Patternmap.cs  →  csharp:Rasm.Generation                       # [WIRE]: InstanceStream with vector-heat-transported per-instance frames — the PATTERN/TILING plane's exact input
Processing/Intent.cs      →  csharp:Rasm.Rhino/Camera                     # [BOUNDARY]: VectorIntent (here) + VectorFrame (Numerics/Atoms.cs) + MotionInterpolation (Parametric/Projections.cs) frozen-name contract; dormant host edge until host-boundary re-entry
Analysis/Query.cs         →  csharp:Rasm.Rhino/Commands                   # [BOUNDARY]: Analyze/AnalysisQuery/Env frozen-name contract (Commands + Overlay bind the same entry); dormant host edge until host-boundary re-entry
*                         ←  csharp:Rasm.Fabrication                      # [SHAPE]: Matrix / Point3d / Vector3d
```

## [03]-[NAMESPACES]

Namespace mirrors folder path — `.editorconfig` `dotnet_style_namespace_match_folder = true:error`: every fence under `Rasm/<Folder>/` declares `namespace Rasm.<Folder>;`, giving the nine roots `Rasm.Analysis`, `Rasm.Domain`, `Rasm.Drawing`, `Rasm.Meshing`, `Rasm.Numerics`, `Rasm.Parametric`, `Rasm.Processing`, `Rasm.Solving`, `Rasm.Spatial`.

`Rasm.Domain.Fault` and the band-2400 `GeometryFault` family (`Numerics/Faults.cs`) are two families by explicit decision — kernel-substrate faults and robust-core geometry faults; `Numerics/Faults.cs` and `Domain/Rails.cs` each state the seam, and neither absorbs the other.
