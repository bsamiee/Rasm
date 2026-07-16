# [RASM_RHINO_MODELING_MESHING]

`Rasm.Rhino.Modeling` owns mesh generation and value-semantic editing. One `MeshOp` union carries parameter-driven meshing, primitive tessellation, quad remeshing, shrink wrapping, curve pipes and extrusions, isosurfacing, network and constrained tessellation, convex hulls, subdivision, mesh booleans, splitting, editing, and component extrusion through `Meshes.Build`. `MeshFidelity`, `QuadLaw`, `WrapLaw`, and `ReduceLaw` carry their complete host policy surfaces while `Context` supplies tolerance slots. Kernel remeshing, decimation, intersections, contours, thickness, and analysis remain kernel-owned.

## [01]-[INDEX]

- [02]-[FIDELITY_POLICY]: `MeshPreset`, `MeshLaw`, `MeshFidelity` — the mesher parameter surface as one policy family.
- [03]-[ENGINE_POLICY]: `QuadLaw`, `WrapLaw`, `ReduceLaw`, `SubdivideLaw`, `ExtrudeLaw`, `MeshSeed`, `CollapseLaw`, `MeshSplitter` — the engine carriers and modality vocabularies.
- [04]-[OPERATION_RAIL]: `MeshSlot`, `MeshEdit`, `MeshOp`, and the `Meshes.Build` entry.
- [05]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[FIDELITY_POLICY]

- Owner: `MeshPreset` `[SmartEnum<int>]` — the host preset rows (`Minimal`, `Standard`, `FastRender`, `QualityRender`, `Analysis`) each carrying its factory column; `MeshLaw` — the full custom `MeshingParameters` property surface as one value; `MeshFidelity` `[Union]` — preset row, density scalar, or custom law as the one fidelity discriminant every mesher case consumes.
- Law: `Rig` is the one site naming `MeshingParameters` — a preset row invokes its factory column, a density case runs the density constructor, and the custom case writes every property once; the rigged carrier is disposable and dies in the consuming arm.
- Law: the obsolete `Coarse`/`Smooth` presets never enter the vocabulary — `FastRenderMesh` and `QualityRenderMesh` are the live rows.
- Growth: a new host mesher knob is one `MeshLaw` field written in `Rig`; a new preset is one row with its factory column.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class MeshPreset {
    public static readonly MeshPreset Minimal = new(key: 0, static () => MeshingParameters.Minimal);
    public static readonly MeshPreset Standard = new(key: 1, static () => MeshingParameters.Default);
    public static readonly MeshPreset FastRender = new(key: 2, static () => MeshingParameters.FastRenderMesh);
    public static readonly MeshPreset QualityRender = new(key: 3, static () => MeshingParameters.QualityRenderMesh);
    public static readonly MeshPreset Analysis = new(key: 4, static () => MeshingParameters.DefaultAnalysisMesh);

    [UseDelegateFromConstructor]
    internal partial MeshingParameters Mint();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeshFidelity {
    private MeshFidelity() { }
    public sealed record Preset(MeshPreset Row) : MeshFidelity;
    public sealed record Density(double Value, Option<double> MinimumEdgeLength = default) : MeshFidelity;
    public sealed record Custom(MeshLaw Law) : MeshFidelity;

    internal Fin<MeshingParameters> Rig(Context domain, Op key) =>
        key.Catch(() => Fin.Succ(value: Switch(
            state: domain,
            preset: static (_, fidelity) => fidelity.Row.Mint(),
            density: static (_, fidelity) => fidelity.MinimumEdgeLength.Case switch {
                double minimum => new MeshingParameters(density: fidelity.Value, minimumEdgeLength: minimum),
                _ => new MeshingParameters(density: fidelity.Value),
            },
            custom: static (model, fidelity) => fidelity.Law.Mint(domain: model))));
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record MeshLaw(
    MeshingParameterTextureRange TextureRange = MeshingParameterTextureRange.PackedScaledNormalized,
    bool JaggedSeams = false,
    bool RefineGrid = true,
    bool DoublePrecision = false,
    bool SimplePlanes = false,
    bool ComputeCurvature = false,
    bool ClosedObjectPostProcess = false,
    int GridMinCount = 0,
    int GridMaxCount = 0,
    double GridAspectRatio = 0.0,
    double GridAmplification = 1.0,
    double MinimumEdgeLength = 0.0001,
    double MaximumEdgeLength = 0.0) {
    internal MeshingParameters Mint(Context domain) => new() {
        TextureRange = TextureRange,
        JaggedSeams = JaggedSeams,
        RefineGrid = RefineGrid,
        DoublePrecision = DoublePrecision,
        SimplePlanes = SimplePlanes,
        ComputeCurvature = ComputeCurvature,
        ClosedObjectPostProcess = ClosedObjectPostProcess,
        GridMinCount = GridMinCount,
        GridMaxCount = GridMaxCount,
        GridAngle = domain.Angle.Value,
        GridAspectRatio = GridAspectRatio,
        GridAmplification = GridAmplification,
        Tolerance = domain.Absolute.Value,
        MinimumTolerance = domain.Absolute.Value,
        RelativeTolerance = domain.Fractional.Value,
        MinimumEdgeLength = MinimumEdgeLength,
        MaximumEdgeLength = MaximumEdgeLength,
        RefineAngle = domain.Angle.Value,
    };
}
```

## [03]-[ENGINE_POLICY]

- Owner: `QuadLaw` — the whole `QuadRemeshParameters` surface; `WrapLaw` — the whole `ShrinkWrapParameters` surface; `ReduceLaw` — the whole `ReduceMeshParameters` surface with locked components and face tags; `SubdivideLaw` `[Union]` — refined Loop, refined Catmull-Clark, and scoped mid-edge subdivision; `ExtrudeLaw` — the `MeshExtruder` column set; `MeshSeed` `[Union]` — the primitive tessellations; `CollapseLaw` `[Union]` — the three face-collapse criteria; `MeshSplitter` `[Union]` — the split-source modalities.
- Law: each carrier rigs once — `QuadLaw.Rig`, `WrapLaw.Rig`, and `ReduceLaw.Rig` are the only sites naming their native types, so guide influence codes, adaptive grants, wrap offsets, and locked-component rosters never scatter to call sites.
- Law: refined subdivision names its nested namespace once — `MeshRefinements.RefinementSettings`, `LoopFormula`, and `CreaseEdges` appear only inside `SubdivideLaw`'s arm, with the level and naked-edge mode as case payload.
- Law: async quad-remeshing is the same case — the remesh case carries `CancellationToken` and `IProgress<int>` as boundary payload, so cancellable long remeshes and blocking calls answer one vocabulary.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SubdivideLaw {
    private SubdivideLaw() { }
    public sealed record Loop(MeshRefinements.LoopFormula Formula = MeshRefinements.LoopFormula.WarrenWeimer, int Level = 1, MeshRefinements.CreaseEdges NakedEdges = MeshRefinements.CreaseEdges.NakedSmooth) : SubdivideLaw;
    public sealed record CatmullClark(int Level = 1, MeshRefinements.CreaseEdges NakedEdges = MeshRefinements.CreaseEdges.NakedSmooth) : SubdivideLaw;
    public sealed record MidEdge(Seq<int> Faces) : SubdivideLaw;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CollapseLaw {
    private CollapseLaw() { }
    public sealed record ByEdgeLength(bool GreaterThan, double EdgeLength) : CollapseLaw;
    public sealed record ByArea(double LessThanArea, double GreaterThanArea) : CollapseLaw;
    public sealed record ByAspectRatio(double Value) : CollapseLaw;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeshSplitter {
    private MeshSplitter() { }
    public sealed record ByPlane(Plane Value) : MeshSplitter;
    public sealed record ByMeshes(Seq<GeometryHandle> Cutters, bool SplitAtCoplanar = false, bool CreateNgons = false) : MeshSplitter;
    public sealed record Disjoint : MeshSplitter;
    public sealed record NonManifold : MeshSplitter;
    public sealed record ByProjectedPolylines(Seq<GeometryHandle> Curves) : MeshSplitter;
    public sealed record AtUnweldedEdges : MeshSplitter;
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeshSeed {
    private MeshSeed() { }
    public sealed record OfPlane(Plane Frame, Interval X, Interval Y, int XCount, int YCount) : MeshSeed;
    public sealed record OfBox(Box Value, int XCount, int YCount, int ZCount) : MeshSeed;
    public sealed record OfSphere(Sphere Value, int XCount, int YCount) : MeshSeed;
    public sealed record IcoSphere(Sphere Value, int Subdivisions) : MeshSeed;
    public sealed record QuadSphere(Sphere Value, int Subdivisions) : MeshSeed;
    public sealed record OfCylinder(Cylinder Value, int Vertical, int Around, bool CapBottom = true, bool CapTop = true, bool Circumscribe = false, bool QuadCaps = false) : MeshSeed;
    public sealed record OfCone(Cone Value, int Vertical, int Around, bool Solid = true, bool QuadCaps = false) : MeshSeed;
    public sealed record OfTorus(Torus Value, int Vertical, int Around) : MeshSeed;
    public sealed record OfClosedPolyline(Seq<Point3d> Points) : MeshSeed;

    internal Fin<GeometryHandle> Build(Op key) =>
        Switch(
            context: key,
            ofPlane: static (op, seed) => op.Catch(() => ModelGate.Own(built: Mesh.CreateFromPlane(
                plane: seed.Frame, xInterval: seed.X, yInterval: seed.Y, xCount: seed.XCount, yCount: seed.YCount), key: op)),
            ofBox: static (op, seed) => op.Catch(() => ModelGate.Own(built: Mesh.CreateFromBox(
                box: seed.Value, xCount: seed.XCount, yCount: seed.YCount, zCount: seed.ZCount), key: op)),
            ofSphere: static (op, seed) => op.Catch(() => ModelGate.Own(built: Mesh.CreateFromSphere(
                sphere: seed.Value, xCount: seed.XCount, yCount: seed.YCount), key: op)),
            icoSphere: static (op, seed) => op.Catch(() => ModelGate.Own(built: Mesh.CreateIcoSphere(
                sphere: seed.Value, subdivisions: seed.Subdivisions), key: op)),
            quadSphere: static (op, seed) => op.Catch(() => ModelGate.Own(built: Mesh.CreateQuadSphere(
                sphere: seed.Value, subdivisions: seed.Subdivisions), key: op)),
            ofCylinder: static (op, seed) => op.Catch(() => ModelGate.Own(built: Mesh.CreateFromCylinder(
                cylinder: seed.Value, vertical: seed.Vertical, around: seed.Around,
                capBottom: seed.CapBottom, capTop: seed.CapTop, circumscribe: seed.Circumscribe, quadCaps: seed.QuadCaps), key: op)),
            ofCone: static (op, seed) => op.Catch(() => ModelGate.Own(built: Mesh.CreateFromCone(
                cone: seed.Value, vertical: seed.Vertical, around: seed.Around, solid: seed.Solid, quadCaps: seed.QuadCaps), key: op)),
            ofTorus: static (op, seed) => op.Catch(() => ModelGate.Own(built: Mesh.CreateFromTorus(
                torus: seed.Value, vertical: seed.Vertical, around: seed.Around), key: op)),
            ofClosedPolyline: static (op, seed) => op.Catch(() => ModelGate.Own(built: Mesh.CreateFromClosedPolyline(
                polyline: new Polyline(collection: seed.Points.AsIterable())), key: op)));
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record QuadLaw(
    int TargetQuadCount = 2000,
    double TargetEdgeLength = 0.0,
    double AdaptiveSize = 50.0,
    bool AdaptiveQuadCount = true,
    bool DetectHardEdges = true,
    int GuideCurveInfluence = 0,
    int PreserveMeshArrayEdgesMode = 0,
    QuadRemeshSymmetryAxis Symmetry = QuadRemeshSymmetryAxis.None) {
    internal Fin<QuadRemeshParameters> Rig(Op key) =>
        key.Catch(() => Fin.Succ(value: new QuadRemeshParameters {
            TargetQuadCount = TargetQuadCount,
            TargetEdgeLength = TargetEdgeLength,
            AdaptiveSize = AdaptiveSize,
            AdaptiveQuadCount = AdaptiveQuadCount,
            DetectHardEdges = DetectHardEdges,
            GuideCurveInfluence = GuideCurveInfluence,
            PreserveMeshArrayEdgesMode = PreserveMeshArrayEdgesMode,
            SymmetryAxis = Symmetry,
        }));
}

public sealed record WrapLaw(
    double TargetEdgeLength = 1.0,
    double Offset = 0.0,
    int SmoothingIterations = 0,
    bool FillHolesInInputObjects = false,
    int PolygonOptimization = 10,
    bool InflateVerticesAndPoints = false,
    bool PreserveColors = false) {
    internal Fin<ShrinkWrapParameters> Rig(Op key) =>
        key.Catch(() => Fin.Succ(value: new ShrinkWrapParameters {
            TargetEdgeLength = TargetEdgeLength,
            Offset = Offset,
            SmoothingIterations = SmoothingIterations,
            FillHolesInInputObjects = FillHolesInInputObjects,
            PolygonOptimization = PolygonOptimization,
            InflateVerticesAndPoints = InflateVerticesAndPoints,
            PreserveColors = PreserveColors,
        }));
}

public sealed record ReduceLaw(
    int DesiredPolygonCount,
    bool AllowDistortion = false,
    int Accuracy = 10,
    bool NormalizeMeshSize = false,
    Seq<int> FaceTags = default,
    Seq<ComponentIndex> LockedComponents = default,
    CancellationToken Cancel = default,
    Option<IProgress<double>> Progress = default) {
    internal Fin<ReduceMeshParameters> Rig(Op key) =>
        key.Catch(() => Fin.Succ(value: new ReduceMeshParameters {
            DesiredPolygonCount = DesiredPolygonCount,
            AllowDistortion = AllowDistortion,
            Accuracy = Accuracy,
            NormalizeMeshSize = NormalizeMeshSize,
            FaceTags = FaceTags.ToArray(),
            LockedComponents = LockedComponents.ToArray(),
            CancelToken = Cancel,
            ProgressReporter = Progress.IfNoneUnsafe((IProgress<double>?)null),
        }));
}

public sealed record ExtrudeLaw(
    Transform Motion,
    bool UVN = false,
    bool EdgeBasedUVN = false,
    bool KeepOriginalFaces = false,
    MeshExtruderParameterMode TextureCoordinates = MeshExtruderParameterMode.CoverWalls,
    MeshExtruderParameterMode SurfaceParameters = MeshExtruderParameterMode.CoverWalls,
    MeshExtruderFaceDirectionMode FaceDirection = MeshExtruderFaceDirectionMode.Keep);
```

## [04]-[OPERATION_RAIL]

- Owner: `MeshSlot` `[SmartEnum<int>]` — the consequence vocabulary; `MeshEdit` `[Union]` — the value-semantic edit verbs applied to one working duplicate; `MeshOp` `[Union]` — generation, remeshing, wrapping, booleans, splitting, editing, and extruding as one verb family; `Meshes` — the one entry folding any operation spread into one `Built<MeshSlot>`.
- Law: every boolean carries its verdict — the options form runs unconditionally, the terminal `Result` lands as a `Code` fact and the `int[][]` input map as `SourceGroups`, so a boolean that silently produced nothing is distinguishable from one that failed.
- Law: edits are value-semantic — `Edit` duplicates the borrowed mesh once, dispatches the verb on the working copy, and owns the copy or the pieces the verb returned; wall-face lists, collapse counts, unified-normal counts, and reduce diagnostics land as facts beside the products.
- Law: the extruder is rigged and dropped — `MeshExtruder` is constructed over the borrowed mesh and component set, its columns written from `ExtrudeLaw`, and the extruded mesh, created component indices, and wall faces cross as product plus `Components` facts.
- Law: hull facets and cleanup demands are evidence — `CreateConvexHull3D` folds its facet index rows as `SourceGroups`, and `RequireIterativeCleanup` gates the cleanup case with a `Flag` fact so a no-op cleanup is visible.
- Boundary: `Mesh.CreateContourCurves` and `ComputeThickness` are kernel-analysis altitude (`Rasm` extraction and measurement own sectioning and metrology); this rail never re-owns them.
- Growth: a new mesher, engine, or edit verb is one case with its arm; the spine and every consumer read it with zero new surface.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class MeshSlot {
    public static readonly MeshSlot Meshed = new(key: 0);
    public static readonly MeshSlot Seeded = new(key: 1);
    public static readonly MeshSlot Remeshed = new(key: 2);
    public static readonly MeshSlot Wrapped = new(key: 3);
    public static readonly MeshSlot Piped = new(key: 4);
    public static readonly MeshSlot Extruded = new(key: 5);
    public static readonly MeshSlot Isosurfaced = new(key: 6);
    public static readonly MeshSlot Networked = new(key: 7);
    public static readonly MeshSlot Hulled = new(key: 8);
    public static readonly MeshSlot Patched = new(key: 9);
    public static readonly MeshSlot Rebuilt = new(key: 10);
    public static readonly MeshSlot Cleaned = new(key: 11);
    public static readonly MeshSlot Subdivided = new(key: 12);
    public static readonly MeshSlot Booled = new(key: 13);
    public static readonly MeshSlot SplitApart = new(key: 14);
    public static readonly MeshSlot EdgeMatched = new(key: 15);
    public static readonly MeshSlot Edited = new(key: 16);
    public static readonly MeshSlot WallFaces = new(key: 17);
    public static readonly MeshSlot Appended = new(key: 18);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeshEdit {
    private MeshEdit() { }
    public sealed record Reduce(ReduceLaw Law) : MeshEdit;
    public sealed record Weld(bool PreserveSurfaceParameters = false) : MeshEdit;
    public sealed record Unweld(bool ModifyNormals = true) : MeshEdit;
    public sealed record UnweldEdges(Seq<int> Edges, bool ModifyNormals = true) : MeshEdit;
    public sealed record UnweldVertices(Seq<int> TopologyVertices, bool ModifyNormals = true) : MeshEdit;
    public sealed record Offset(double Distance, bool Solidify, Option<Vector3d> Direction = default) : MeshEdit;
    public sealed record Heal(double Distance) : MeshEdit;
    public sealed record FillHoles(Option<int> TopologyEdge = default) : MeshEdit;
    public sealed record MatchNaked(double Distance, bool Ratchet) : MeshEdit;
    public sealed record MergeCoplanar : MeshEdit;
    public sealed record Smooth(double Factor, int Steps, bool X, bool Y, bool Z, bool FixBoundaries, SmoothingCoordinateSystem System, Plane Frame, Seq<int> Vertices = default) : MeshEdit;
    public sealed record Collapse(CollapseLaw Law) : MeshEdit;
    public sealed record RebuildNormals : MeshEdit;
    public sealed record UnifyNormals : MeshEdit;
    public sealed record Orient(bool VertexNormals, bool FaceNormals, bool FaceOrientation, bool NgonBoundaries) : MeshEdit;
    public sealed record Compact : MeshEdit;
    public sealed record ExtractNonManifold(bool Selective) : MeshEdit;
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeshOp {
    private MeshOp() { }
    public sealed record FromGeometry(GeometryHandle Source, MeshFidelity Fidelity) : MeshOp;
    public sealed record FromSubD(GeometryHandle Source, SubDDisplayParameters.Density Level) : MeshOp;
    public sealed record Cage(GeometryHandle Source, bool TextureCoordinates = false) : MeshOp;
    public sealed record FromBoundary(GeometryHandle Boundary, MeshFidelity Fidelity) : MeshOp;
    public sealed record Seed(MeshSeed Value) : MeshOp;
    public sealed record QuadRemesh(GeometryHandle Source, QuadLaw Law, Seq<GeometryHandle> Guides, Seq<int> FaceBlocks = default, CancellationToken Cancel = default, Option<IProgress<int>> Progress = default) : MeshOp;
    public sealed record Wrap(Seq<GeometryHandle> Sources, WrapLaw Law, Option<MeshFidelity> Fidelity = default, CancellationToken Cancel = default) : MeshOp;
    public sealed record CurvePipe(GeometryHandle Curve, double Radius, int Segments, int Accuracy, MeshPipeCapStyle Cap, bool Faceted, Seq<Interval> Intervals = default) : MeshOp;
    public sealed record CurveExtrude(GeometryHandle Curve, Vector3d Direction, Option<MeshFidelity> Fidelity = default, Option<BoundingBox> Bounds = default) : MeshOp;
    public sealed record Isosurface(Func<Point3d, double> Field, BoundingBox Box, int Resolution, int RootFindingMaxSteps) : MeshOp;
    public sealed record FromLines(Seq<GeometryHandle> Lines, int MaxFaceValence) : MeshOp;
    public sealed record Tessellate(Seq<Point3d> Points, Seq<Seq<Point3d>> Edges, Plane Frame, bool AllowNewVertices) : MeshOp;
    public sealed record ConvexHull(Seq<Point3d> Points) : MeshOp;
    public sealed record Patch(Seq<Point3d> OuterBoundary, Option<GeometryHandle> PullbackSurface, Seq<GeometryHandle> InnerBoundaries, Seq<GeometryHandle> BothSideCurves, Seq<Point3d> InnerPoints, bool TrimBack, int Divisions) : MeshOp;
    public sealed record Rebuild(GeometryHandle Source, bool PreserveTextureCoordinates = true, bool PreserveVertexColors = true) : MeshOp;
    public sealed record Cleanup(Seq<GeometryHandle> Sources) : MeshOp;
    public sealed record Refine(GeometryHandle Source, SubdivideLaw Law) : MeshOp;
    public sealed record Boolean(BooleanVerb Verb, Seq<GeometryHandle> First, Seq<GeometryHandle> Second, CancellationToken Cancel = default, Option<IProgress<double>> Progress = default) : MeshOp;
    public sealed record Split(GeometryHandle Target, MeshSplitter Splitter) : MeshOp;
    public sealed record SplitCount(GeometryHandle Target, int MaxCount, bool CountSum, bool CountTriangles) : MeshOp;
    public sealed record Partition(GeometryHandle Target, int MaxVertexCount, int MaxFaceCount) : MeshOp;
    public sealed record MatchEdges(Seq<GeometryHandle> Targets, double Distance, bool SimpleSplits, bool Ratchet, bool Average, bool JoinResult) : MeshOp;
    public sealed record Append(Seq<GeometryHandle> Sources) : MeshOp;
    public sealed record Edit(GeometryHandle Target, MeshEdit Verb) : MeshOp;
    public sealed record Extrude(GeometryHandle Target, Seq<ComponentIndex> Components, ExtrudeLaw Law) : MeshOp;

    internal Fin<Built<MeshSlot>> Apply(Context domain) =>
        Switch(
            context: domain,
            fromGeometry: static (model, edit) => {
                Op op = Op.Of(name: nameof(FromGeometry));
                return ModelGate.Borrow<GeometryBase, Built<MeshSlot>>(handle: edit.Source, key: op, body: source =>
                    from parameters in edit.Fidelity.Rig(domain: model, key: op)
                    from built in op.Catch(() => {
                        using MeshingParameters live = parameters;
                        return source switch {
                            Brep brep => Many(op, MeshSlot.Meshed, () => Mesh.CreateFromBrep(brep: brep, meshingParameters: live)),
                            Surface surface => Single(op, MeshSlot.Meshed, () => Mesh.CreateFromSurface(surface: surface, meshingParameters: live)),
                            Extrusion extrusion => Single(op, MeshSlot.Meshed, () => Mesh.CreateFromExtrusion(extrusion: extrusion, meshingParameters: live)),
                            _ => Fin.Fail<Built<MeshSlot>>(error: op.Unsupported(geometryType: source.GetType(), outputType: typeof(Mesh))),
                        };
                    })
                    select built);
            },
            fromSubD: static (_, edit) => {
                Op op = Op.Of(name: nameof(FromSubD));
                return ModelGate.Borrow<SubD, Built<MeshSlot>>(handle: edit.Source, key: op, body: subd =>
                    Single(op, MeshSlot.Meshed, () => Mesh.CreateFromSubD(subd: subd, displayDensity: edit.Level)));
            },
            cage: static (_, edit) => {
                Op op = Op.Of(name: nameof(Cage));
                return ModelGate.Borrow<GeometryBase, Built<MeshSlot>>(handle: edit.Source, key: op, body: source =>
                    source switch {
                        SubD subd => Single(op, MeshSlot.Meshed, () => edit.TextureCoordinates
                            ? Mesh.CreateFromSubDControlNetWithTextureCoordinates(subd: subd)
                            : Mesh.CreateFromSubDControlNet(subd: subd)),
                        Surface surface => Single(op, MeshSlot.Meshed, () => Mesh.CreateFromSurfaceControlNet(surface: surface)),
                        _ => Fin.Fail<Built<MeshSlot>>(error: op.Unsupported(geometryType: source.GetType(), outputType: typeof(Mesh))),
                    });
            },
            fromBoundary: static (model, edit) => {
                Op op = Op.Of(name: nameof(FromBoundary));
                return ModelGate.Borrow<Curve, Built<MeshSlot>>(handle: edit.Boundary, key: op, body: boundary =>
                    from parameters in edit.Fidelity.Rig(domain: model, key: op)
                    from built in op.Catch(() => {
                        using MeshingParameters live = parameters;
                        return Single(op, MeshSlot.Meshed, () => Mesh.CreateFromPlanarBoundary(
                            boundary: boundary, parameters: live, tolerance: model.Absolute.Value));
                    })
                    select built);
            },
            seed: static (_, edit) => {
                Op op = Op.Of(name: nameof(Seed));
                return edit.Value.Build(key: op).Map(product => new Built<MeshSlot>(
                    Products: Seq(product),
                    Evidence: BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Seeded, body: new BuildBody.Tally(Count: 1))));
            },
            quadRemesh: static (_, edit) => {
                Op op = Op.Of(name: nameof(QuadRemesh));
                return ModelGate.Borrow<GeometryBase, Built<MeshSlot>>(handle: edit.Source, key: op, body: source =>
                    ModelGate.BorrowMany<Curve, Built<MeshSlot>>(handles: edit.Guides, key: op, allowEmpty: true, body: guides =>
                        from parameters in edit.Law.Rig(key: op)
                        from built in (source switch {
                            Brep brep => Single(op, MeshSlot.Remeshed, () => Mesh.QuadRemeshBrep(
                                brep: brep, parameters: parameters, guideCurves: guides.AsIterable(),
                                progress: edit.Progress.IfNoneUnsafe((IProgress<int>?)null), cancelToken: edit.Cancel)),
                            Mesh mesh when !edit.FaceBlocks.IsEmpty => Single(op, MeshSlot.Remeshed, () => mesh.QuadRemesh(
                                faceBlocks: edit.FaceBlocks.AsIterable(), parameters: parameters, guideCurves: guides.AsIterable(),
                                progress: edit.Progress.IfNoneUnsafe((IProgress<int>?)null), cancelToken: edit.Cancel)),
                            Mesh mesh => Single(op, MeshSlot.Remeshed, () => mesh.QuadRemesh(
                                parameters: parameters, guideCurves: guides.AsIterable())),
                            _ => Fin.Fail<Built<MeshSlot>>(error: op.Unsupported(geometryType: source.GetType(), outputType: typeof(Mesh))),
                        })
                        select built));
            },
            wrap: static (model, edit) => {
                Op op = Op.Of(name: nameof(Wrap));
                return ModelGate.BorrowMany<GeometryBase, Built<MeshSlot>>(handles: edit.Sources, key: op, body: sources =>
                    from parameters in edit.Law.Rig(key: op)
                    from built in op.Catch(() => sources.ForAll(static value => value is Mesh)
                        ? Single(op, MeshSlot.Wrapped, () => Mesh.ShrinkWrap(
                            meshes: sources.Map(static value => (Mesh)value).AsIterable(), parameters: parameters, token: edit.Cancel))
                        : sources.Count == 1 && sources[0] is PointCloud cloud
                            ? Single(op, MeshSlot.Wrapped, () => Mesh.ShrinkWrap(pointCloud: cloud, parameters: parameters, token: edit.Cancel))
                            : edit.Fidelity.Case switch {
                                MeshFidelity fidelity => fidelity.Rig(domain: model, key: op).Bind(meshing => {
                                    using MeshingParameters live = meshing;
                                    return Single(op, MeshSlot.Wrapped, () => Mesh.ShrinkWrap(
                                        geometryBases: sources.AsIterable(), parameters: parameters, meshingParameters: live, token: edit.Cancel));
                                }),
                                _ => Fin.Fail<Built<MeshSlot>>(error: op.MissingContext()),
                            })
                    select built);
            },
            curvePipe: static (_, edit) => {
                Op op = Op.Of(name: nameof(CurvePipe));
                return ModelGate.Borrow<Curve, Built<MeshSlot>>(handle: edit.Curve, key: op, body: curve =>
                    Single(op, MeshSlot.Piped, () => Mesh.CreateFromCurvePipe(
                        curve: curve, radius: edit.Radius, segments: edit.Segments, accuracy: edit.Accuracy,
                        capType: edit.Cap, faceted: edit.Faceted,
                        intervals: edit.Intervals.IsEmpty ? null : edit.Intervals.AsIterable())));
            },
            curveExtrude: static (model, edit) => {
                Op op = Op.Of(name: nameof(CurveExtrude));
                return ModelGate.Borrow<Curve, Built<MeshSlot>>(handle: edit.Curve, key: op, body: curve =>
                    edit.Fidelity.Case switch {
                        MeshFidelity fidelity => fidelity.Rig(domain: model, key: op).Bind(parameters => op.Catch(() => {
                            using MeshingParameters live = parameters;
                            return Single(op, MeshSlot.Extruded, () => edit.Bounds.Case switch {
                                BoundingBox bounds => Mesh.CreateFromCurveExtrusion(curve: curve, direction: edit.Direction, parameters: live, boundingBox: bounds),
                                _ => Mesh.CreateExtrusion(profile: curve, direction: edit.Direction, parameters: live),
                            });
                        })),
                        _ => Single(op, MeshSlot.Extruded, () => Mesh.CreateExtrusion(profile: curve, direction: edit.Direction)),
                    });
            },
            isosurface: static (_, edit) => {
                Op op = Op.Of(name: nameof(Isosurface));
                return
                    from field in Optional(edit.Field).ToFin(Fail: op.InvalidInput())
                    from built in Single(op, MeshSlot.Isosurfaced, () => Mesh.CreateFromIsosurface(
                        scalarFieldEvaluator: field, box: edit.Box, resolution: edit.Resolution, RootFindingMaxSteps: edit.RootFindingMaxSteps))
                    select built;
            },
            fromLines: static (model, edit) => {
                Op op = Op.Of(name: nameof(FromLines));
                return ModelGate.BorrowMany<Curve, Built<MeshSlot>>(handles: edit.Lines, key: op, body: lines =>
                    Single(op, MeshSlot.Networked, () => Mesh.CreateFromLines(
                        lines: lines.ToArray(), maxFaceValence: edit.MaxFaceValence, tolerance: model.Absolute.Value)));
            },
            tessellate: static (_, edit) => {
                Op op = Op.Of(name: nameof(Tessellate));
                return Single(op, MeshSlot.Networked, () => Mesh.CreateFromTessellation(
                    points: edit.Points.AsIterable(),
                    edges: edit.Edges.Map(static loop => loop.AsIterable()).AsIterable(),
                    plane: edit.Frame, allowNewVertices: edit.AllowNewVertices));
            },
            convexHull: static (model, edit) => {
                Op op = Op.Of(name: nameof(ConvexHull));
                return op.Catch(() => {
                    Mesh hull = Mesh.CreateConvexHull3D(
                        points: edit.Points.AsIterable(), hullFacets: out int[][] facets,
                        tolerance: model.Absolute.Value, angleTolerance: model.Angle.Value);
                    return ModelGate.Own(built: hull, key: op).Map(owned => new Built<MeshSlot>(
                        Products: Seq(owned),
                        Evidence: BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Hulled, body: new BuildBody.Tally(Count: 1))
                            + BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Hulled, body: new BuildBody.SourceGroups(
                                Groups: toSeq(facets ?? []).Map(static rows => toSeq(rows))))));
                });
            },
            patch: static (model, edit) => {
                Op op = Op.Of(name: nameof(Patch));
                return ModelGate.BorrowMany<Curve, Built<MeshSlot>>(handles: edit.InnerBoundaries, key: op, allowEmpty: true, body: inner =>
                    ModelGate.BorrowMany<Curve, Built<MeshSlot>>(handles: edit.BothSideCurves, key: op, allowEmpty: true, body: bothSides => {
                        Fin<Built<MeshSlot>> Run(Option<Surface> pullback) => Single(op, MeshSlot.Patched, () => Mesh.CreatePatch(
                            outerBoundary: new Polyline(collection: edit.OuterBoundary.AsIterable()),
                            angleToleranceRadians: model.Angle.Value,
                            pullbackSurface: pullback.IfNoneUnsafe((Surface?)null),
                            innerBoundaryCurves: inner.AsIterable(), innerBothSideCurves: bothSides.AsIterable(),
                            innerPoints: edit.InnerPoints.AsIterable(), trimback: edit.TrimBack, divisions: edit.Divisions));
                        return edit.PullbackSurface.Case switch {
                            GeometryHandle surface => ModelGate.Borrow<Surface, Built<MeshSlot>>(handle: surface, key: op, body: live => Run(pullback: Some(live))),
                            _ => Run(pullback: Option<Surface>.None),
                        };
                    }));
            },
            rebuild: static (_, edit) => {
                Op op = Op.Of(name: nameof(Rebuild));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Source, key: op, body: mesh =>
                    Single(op, MeshSlot.Rebuilt, () => Mesh.RebuildMesh(
                        mesh: mesh, preserveTextureCoordinates: edit.PreserveTextureCoordinates, preserveVertexColors: edit.PreserveVertexColors)));
            },
            cleanup: static (model, edit) => {
                Op op = Op.Of(name: nameof(Cleanup));
                return ModelGate.BorrowMany<Mesh, Built<MeshSlot>>(handles: edit.Sources, key: op, body: sources =>
                    op.Catch(() => {
                        bool required = Mesh.RequireIterativeCleanup(meshes: sources.AsIterable(), tolerance: model.Absolute.Value);
                        return ModelGate.OwnMany(built: Mesh.CreateFromIterativeCleanup(
                                meshes: sources.AsIterable(), tolerance: model.Absolute.Value), key: op)
                            .Map(owned => new Built<MeshSlot>(
                                Products: owned,
                                Evidence: BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Cleaned, body: new BuildBody.Tally(Count: owned.Count))
                                    + BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Cleaned, body: new BuildBody.Flag(Value: required))));
                    }));
            },
            refine: static (_, edit) => {
                Op op = Op.Of(name: nameof(Refine));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Source, key: op, body: mesh =>
                    edit.Law switch {
                        SubdivideLaw.Loop law => op.Catch(() => Single(op, MeshSlot.Subdivided, () => Mesh.CreateRefinedLoopMesh(
                            mesh: mesh, formula: law.Formula,
                            settings: new MeshRefinements.RefinementSettings { Level = law.Level, NakedEdgeMode = law.NakedEdges }))),
                        SubdivideLaw.CatmullClark law => op.Catch(() => Single(op, MeshSlot.Subdivided, () => Mesh.CreateRefinedCatmullClarkMesh(
                            mesh: mesh, settings: new MeshRefinements.RefinementSettings { Level = law.Level, NakedEdgeMode = law.NakedEdges }))),
                        SubdivideLaw.MidEdge law => op.Catch(() => {
                            Mesh working = (Mesh)mesh.Duplicate();
                            Fin<bool> divided = op.Catch(() => Fin.Succ(value: law.Faces.IsEmpty
                                ? working.Subdivide()
                                : working.Subdivide(faceIndices: law.Faces.AsIterable())));
                            return divided.Bind(done => {
                                if (done) {
                                    return ModelGate.Own(built: working, key: op).Map(owned => new Built<MeshSlot>(
                                        Products: Seq(owned),
                                        Evidence: BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Subdivided, body: new BuildBody.Tally(Count: 1))));
                                }
                                working.Dispose();
                                return Fin.Fail<Built<MeshSlot>>(error: op.InvalidResult());
                            });
                        }),
                        _ => Fin.Fail<Built<MeshSlot>>(error: op.InvalidInput()),
                    });
            },
            boolean: static (model, edit) => {
                Op op = Op.Of(name: nameof(Boolean));
                return ModelGate.BorrowMany<Mesh, Built<MeshSlot>>(handles: edit.First, key: op, body: first =>
                    ModelGate.BorrowMany<Mesh, Built<MeshSlot>>(handles: edit.Second, key: op, allowEmpty: !edit.Verb.RequiresSecond, body: second =>
                        op.Catch(() => {
                            MeshBooleanOptions options = new() {
                                Tolerance = model.MeshIntersectionTolerance,
                                CancellationToken = edit.Cancel,
                                ProgressReporter = edit.Progress.IfNoneUnsafe((IProgress<double>?)null),
                            };
                            Rhino.Commands.Result verdict = Rhino.Commands.Result.Failure;
                            int[][] map = [];
                            Mesh[] products = edit.Verb.Switch(
                                union: () => Mesh.CreateBooleanUnion(meshes: first.AsIterable(), options: options, commandResult: out verdict, inputMap: out map),
                                intersection: () => Mesh.CreateBooleanIntersection(firstSet: first.AsIterable(), secondSet: second.AsIterable(), options: options, result: out verdict, inputMap: out map),
                                difference: () => Mesh.CreateBooleanDifference(firstSet: first.AsIterable(), secondSet: second.AsIterable(), options: options, result: out verdict, inputMap: out map),
                                split: () => Mesh.CreateBooleanSplit(meshesToSplit: first.AsIterable(), meshSplitters: second.AsIterable(), options: options, result: out verdict, inputMap: out map));
                            return ModelGate.OwnMany(built: products, key: op, allowEmpty: true).Map(owned => new Built<MeshSlot>(
                                Products: owned,
                                Evidence: BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Booled, body: new BuildBody.Tally(Count: owned.Count))
                                    + BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Booled, body: new BuildBody.Code(Value: (int)verdict))
                                    + BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Booled, body: new BuildBody.SourceGroups(
                                        Groups: toSeq(map ?? []).Map(static rows => toSeq(rows))))));
                        })));
            },
            split: static (model, edit) => {
                Op op = Op.Of(name: nameof(Split));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Target, key: op, body: mesh =>
                    edit.Splitter switch {
                        MeshSplitter.ByPlane law => Many(op, MeshSlot.SplitApart, () => mesh.Split(plane: law.Value)),
                        MeshSplitter.Disjoint => Many(op, MeshSlot.SplitApart, () => mesh.SplitDisjointPieces()),
                        MeshSplitter.NonManifold => Many(op, MeshSlot.SplitApart, () => mesh.SplitNon2Manifolds()),
                        MeshSplitter.AtUnweldedEdges => Many(op, MeshSlot.SplitApart, () => mesh.ExplodeAtUnweldedEdges()),
                        MeshSplitter.ByMeshes law => ModelGate.BorrowMany<Mesh, Built<MeshSlot>>(handles: law.Cutters, key: op, body: cutters =>
                            Many(op, MeshSlot.SplitApart, () => mesh.Split(
                                meshes: cutters.AsIterable(), tolerance: model.MeshIntersectionTolerance,
                                splitAtCoplanar: law.SplitAtCoplanar, createNgons: law.CreateNgons,
                                textLog: null, cancel: CancellationToken.None, progress: null))),
                        MeshSplitter.ByProjectedPolylines law => ModelGate.BorrowMany<PolylineCurve, Built<MeshSlot>>(handles: law.Curves, key: op, body: curves =>
                            Many(op, MeshSlot.SplitApart, () => mesh.SplitWithProjectedPolylines(
                                curves: curves.AsIterable(), tolerance: model.Absolute.Value))),
                        _ => Fin.Fail<Built<MeshSlot>>(error: op.InvalidInput()),
                    });
            },
            splitCount: static (_, edit) => {
                Op op = Op.Of(name: nameof(SplitCount));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Target, key: op, body: mesh =>
                    Many(op, MeshSlot.SplitApart, () => Mesh.SplitMesh(
                        mesh: mesh, maxCount: edit.MaxCount, countSum: edit.CountSum, countTriangles: edit.CountTriangles)));
            },
            partition: static (_, edit) => {
                Op op = Op.Of(name: nameof(Partition));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Target, key: op, body: mesh =>
                    Many(op, MeshSlot.SplitApart, () => Mesh.PartitionMesh(
                        mesh: mesh, maxVertexCount: edit.MaxVertexCount, maxFaceCount: edit.MaxFaceCount)));
            },
            matchEdges: static (_, edit) => {
                Op op = Op.Of(name: nameof(MatchEdges));
                return ModelGate.BorrowMany<Mesh, Built<MeshSlot>>(handles: edit.Targets, key: op, body: meshes =>
                    Many(op, MeshSlot.EdgeMatched, () => Mesh.MatchEdges(
                        inputMeshes: meshes.AsIterable(), distance: edit.Distance, simpleSplits: edit.SimpleSplits,
                        rachet: edit.Ratchet, average: edit.Average, join: edit.JoinResult)));
            },
            append: static (_, edit) => {
                Op op = Op.Of(name: nameof(Append));
                return ModelGate.BorrowMany<Mesh, Built<MeshSlot>>(handles: edit.Sources, key: op, body: sources =>
                    op.Catch(() => {
                        Mesh working = new();
                        working.Append(meshes: sources.AsIterable());
                        return ModelGate.Own(built: working, key: op).Map(owned => new Built<MeshSlot>(
                            Products: Seq(owned),
                            Evidence: BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Appended, body: new BuildBody.Tally(Count: sources.Count))));
                    }));
            },
            edit: static (model, request) => {
                Op op = Op.Of(name: nameof(Edit));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: request.Target, key: op, body: source =>
                    op.Catch(() => {
                        Mesh working = (Mesh)source.Duplicate();
                        return Edited(working: working, verb: request.Verb, domain: model, op: op).MapFail(error => {
                            working.Dispose();
                            return error;
                        });
                    }));
            },
            extrude: static (_, edit) => {
                Op op = Op.Of(name: nameof(Extrude));
                return ModelGate.Borrow<Mesh, Built<MeshSlot>>(handle: edit.Target, key: op, body: mesh =>
                    op.Catch(() => {
                        using MeshExtruder engine = new(inputMesh: mesh, componentIndices: edit.Components.AsIterable()) {
                            Transform = edit.Law.Motion,
                            UVN = edit.Law.UVN,
                            EdgeBasedUVN = edit.Law.EdgeBasedUVN,
                            KeepOriginalFaces = edit.Law.KeepOriginalFaces,
                            TextureCoordinateMode = edit.Law.TextureCoordinates,
                            SurfaceParameterMode = edit.Law.SurfaceParameters,
                            FaceDirectionMode = edit.Law.FaceDirection,
                        };
                        Seq<Line> preview = toSeq(engine.PreviewLines ?? []);
                        return op.Confirm(success: engine.ExtrudedMesh(
                                extrudedMeshOut: out Mesh extruded, componentIndicesOut: out System.Collections.Generic.List<ComponentIndex> created))
                            .Bind(_ => ModelGate.Own(built: extruded, key: op).Map(owned => new Built<MeshSlot>(
                                Products: Seq(owned),
                                Evidence: BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Extruded, body: new BuildBody.Tally(Count: 1))
                                    + BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Extruded, body: new BuildBody.ComponentRows(Indices: toSeq(created ?? [])))
                                    + BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Extruded, body: new BuildBody.Segments(Lines: preview))
                                    + BuildReceipt<MeshSlot>.Of(slot: MeshSlot.WallFaces, body: new BuildBody.Components(Indices: toSeq(engine.GetWallFaces()))))));
                    }));
            });

    private static Fin<Built<MeshSlot>> Edited(Mesh working, MeshEdit verb, Context domain, Op op) =>
        verb.Switch(
            state: (Working: working, Domain: domain, Op: op),
            reduce: static (ctx, edit) =>
                from parameters in edit.Law.Rig(key: ctx.Op)
                from _ in ctx.Op.Confirm(success: ctx.Working.Reduce(parameters: parameters, threaded: true))
                from built in Kept(ctx.Op, ctx.Working, extra: BuildReceipt<MeshSlot>.Of(
                    slot: MeshSlot.Edited, body: new BuildBody.Text(Value: parameters.Error ?? string.Empty)))
                select built,
            weld: static (ctx, edit) => ctx.Op.Catch(() => {
                ctx.Working.Weld(angleToleranceRadians: ctx.Domain.Angle.Value, preserveSurfaceParameters: edit.PreserveSurfaceParameters);
                return Kept(ctx.Op, ctx.Working);
            }),
            unweld: static (ctx, edit) => ctx.Op.Catch(() => {
                ctx.Working.Unweld(angleToleranceRadians: ctx.Domain.Angle.Value, modifyNormals: edit.ModifyNormals);
                return Kept(ctx.Op, ctx.Working);
            }),
            unweldEdges: static (ctx, edit) => ctx.Op
                .Confirm(success: ctx.Working.UnweldEdge(edgeIndices: edit.Edges.AsIterable(), modifyNormals: edit.ModifyNormals))
                .Bind(_ => Kept(ctx.Op, ctx.Working)),
            unweldVertices: static (ctx, edit) => ctx.Op
                .Confirm(success: ctx.Working.UnweldVertices(topologyVertexIndices: edit.TopologyVertices.AsIterable(), modifyNormals: edit.ModifyNormals))
                .Bind(_ => Kept(ctx.Op, ctx.Working)),
            offset: static (ctx, edit) => ctx.Op.Catch(() => {
                Seq<int> wallRows = Seq<int>();
                Mesh shelled;
                if (edit.Direction.Case is Vector3d direction) {
                    shelled = ctx.Working.Offset(
                        distance: edit.Distance, solidify: edit.Solidify, direction: direction,
                        wallFacesOut: out System.Collections.Generic.List<int> walls);
                    wallRows = toSeq(walls ?? []);
                } else {
                    shelled = ctx.Working.Offset(distance: edit.Distance, solidify: edit.Solidify);
                }
                return ModelGate.Own(built: shelled, key: ctx.Op).Map(owned => {
                    ctx.Working.Dispose();
                    return new Built<MeshSlot>(
                        Products: Seq(owned),
                        Evidence: BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Edited, body: new BuildBody.Tally(Count: 1))
                            + BuildReceipt<MeshSlot>.Of(slot: MeshSlot.WallFaces, body: new BuildBody.Components(Indices: wallRows)));
                });
            }),
            heal: static (ctx, edit) => ctx.Op.Confirm(success: ctx.Working.HealNakedEdges(distance: edit.Distance)).Bind(_ => Kept(ctx.Op, ctx.Working)),
            fillHoles: static (ctx, edit) => ctx.Op.Confirm(success: edit.TopologyEdge.Case switch {
                    int edge => ctx.Working.FillHole(topologyEdgeIndex: edge),
                    _ => ctx.Working.FillHoles(),
                })
                .Bind(_ => Kept(ctx.Op, ctx.Working)),
            matchNaked: static (ctx, edit) => ctx.Op.Confirm(success: ctx.Working.MatchEdges(distance: edit.Distance, rachet: edit.Ratchet)).Bind(_ => Kept(ctx.Op, ctx.Working)),
            mergeCoplanar: static ctx => ctx.Op
                .Confirm(success: ctx.Working.MergeAllCoplanarFaces(tolerance: ctx.Domain.Absolute.Value, angleTolerance: ctx.Domain.Angle.Value))
                .Bind(_ => Kept(ctx.Op, ctx.Working)),
            smooth: static (ctx, edit) => ctx.Op.Confirm(success: edit.Vertices.IsEmpty
                    ? ctx.Working.Smooth(
                        smoothFactor: edit.Factor, numSteps: edit.Steps, bXSmooth: edit.X, bYSmooth: edit.Y, bZSmooth: edit.Z,
                        bFixBoundaries: edit.FixBoundaries, coordinateSystem: edit.System, plane: edit.Frame)
                    : ctx.Working.Smooth(
                        vertexIndices: edit.Vertices.AsIterable(), smoothFactor: edit.Factor, numSteps: edit.Steps,
                        bXSmooth: edit.X, bYSmooth: edit.Y, bZSmooth: edit.Z,
                        bFixBoundaries: edit.FixBoundaries, coordinateSystem: edit.System, plane: edit.Frame))
                .Bind(_ => Kept(ctx.Op, ctx.Working)),
            collapse: static (ctx, edit) => ctx.Op.Catch(() => {
                int collapsed = edit.Law switch {
                    CollapseLaw.ByEdgeLength law => ctx.Working.CollapseFacesByEdgeLength(bGreaterThan: law.GreaterThan, edgeLength: law.EdgeLength),
                    CollapseLaw.ByArea law => ctx.Working.CollapseFacesByArea(lessThanArea: law.LessThanArea, greaterThanArea: law.GreaterThanArea),
                    CollapseLaw.ByAspectRatio law => ctx.Working.CollapseFacesByByAspectRatio(aspectRatio: law.Value),
                    _ => -1,
                };
                return collapsed >= 0
                    ? Kept(ctx.Op, ctx.Working, extra: BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Edited, body: new BuildBody.Tally(Count: collapsed)))
                    : Fin.Fail<Built<MeshSlot>>(error: ctx.Op.InvalidInput());
            }),
            rebuildNormals: static ctx => ctx.Op.Catch(() => {
                ctx.Working.RebuildNormals();
                return Kept(ctx.Op, ctx.Working);
            }),
            unifyNormals: static ctx => ctx.Op.Catch(() => {
                int modified = ctx.Working.UnifyNormals();
                return Kept(ctx.Op, ctx.Working, extra: BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Edited, body: new BuildBody.Tally(Count: modified)));
            }),
            orient: static (ctx, edit) => ctx.Op.Catch(() => {
                ctx.Working.Flip(
                    vertexNormals: edit.VertexNormals, faceNormals: edit.FaceNormals,
                    faceOrientation: edit.FaceOrientation, ngonsBoundaryDirection: edit.NgonBoundaries);
                return Kept(ctx.Op, ctx.Working);
            }),
            compact: static ctx => ctx.Op.Confirm(success: ctx.Working.Compact()).Bind(_ => Kept(ctx.Op, ctx.Working)),
            extractNonManifold: static (ctx, edit) => ctx.Op.Catch(() =>
                from extracted in ModelGate.Own(built: ctx.Working.ExtractNonManifoldEdges(selective: edit.Selective), key: ctx.Op)
                from remainder in ModelGate.Own(built: ctx.Working, key: ctx.Op).MapFail(error => { extracted.Dispose(); return error; })
                select new Built<MeshSlot>(
                    Products: Seq(extracted, remainder),
                    Evidence: BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Edited, body: new BuildBody.Tally(Count: 2)))));

    private static Fin<Built<MeshSlot>> Kept(Op op, Mesh working) =>
        ModelGate.Own(built: working, key: op).Map(owned => new Built<MeshSlot>(
            Products: Seq(owned),
            Evidence: BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Edited, body: new BuildBody.Tally(Count: 1))));

    private static Fin<Built<MeshSlot>> Kept(Op op, Mesh working, BuildReceipt<MeshSlot> extra) =>
        ModelGate.Own(built: working, key: op).Map(owned => new Built<MeshSlot>(
            Products: Seq(owned),
            Evidence: BuildReceipt<MeshSlot>.Of(slot: MeshSlot.Edited, body: new BuildBody.Tally(Count: 1)) + extra));

    private static Fin<Built<MeshSlot>> Single(Op op, MeshSlot slot, Func<Mesh?> run) =>
        op.Catch(() => ModelGate.Own(built: run(), key: op).Map(owned => new Built<MeshSlot>(
            Products: Seq(owned),
            Evidence: BuildReceipt<MeshSlot>.Of(slot: slot, body: new BuildBody.Tally(Count: 1)))));

    private static Fin<Built<MeshSlot>> Many(Op op, MeshSlot slot, Func<System.Collections.Generic.IEnumerable<Mesh>> run) =>
        op.Catch(() => ModelGate.OwnMany(built: run(), key: op).Map(owned => new Built<MeshSlot>(
            Products: owned,
            Evidence: BuildReceipt<MeshSlot>.Of(slot: slot, body: new BuildBody.Tally(Count: owned.Count)))));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Meshes {
    public static Fin<Built<MeshSlot>> Build(Context context, params ReadOnlySpan<MeshOp> operations) {
        Op op = Op.Of();
        return from domain in Optional(context).ToFin(Fail: op.MissingContext())
               from _ in guard(operations.Length > 0, op.InvalidInput())
               from built in ModelGate.Folded(
                   context: domain,
                   operations: toSeq(operations.ToArray()),
                   apply: static (operation, model) => operation.Apply(domain: model))
               select built;
    }
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]           | [OWNER]         | [FORM]                                               | [ENTRY]                     |
| :-----: | :------------------ | :-------------- | :---------------------------------------------------- | :-------------------------- |
|  [01]   | mesher fidelity     | `MeshFidelity`  | preset rows, density scalar, or full custom law      | `Rig(domain, key)`          |
|  [02]   | mesher parameters   | `MeshLaw`       | whole `MeshingParameters` surface as one value       | `MeshFidelity.Custom`       |
|  [03]   | quad remeshing      | `QuadLaw`       | whole `QuadRemeshParameters` surface as one value    | `MeshOp.QuadRemesh` / `Rig` |
|  [04]   | shrink wrapping     | `WrapLaw`       | whole `ShrinkWrapParameters` surface as one value    | `MeshOp.Wrap` / `Rig`       |
|  [05]   | decimation          | `ReduceLaw`     | whole `ReduceMeshParameters` with locked components  | `MeshEdit.Reduce` / `Rig`   |
|  [06]   | refined subdivision | `SubdivideLaw`  | Loop, Catmull-Clark, and mid-edge as one union       | `MeshOp.Refine`             |
|  [07]   | primitive seeding   | `MeshSeed`      | nine tessellation constructors as one union          | `MeshOp.Seed`               |
|  [08]   | boolean verdicts    | `MeshOp`        | terminal `Result` code + `int[][]` map as facts      | `MeshSlot.Booled` facts     |
|  [09]   | split modality      | `MeshSplitter`  | plane, meshes, disjoint, non-manifold, projected     | `MeshOp.Split`              |
|  [10]   | value-semantic edit | `MeshEdit`      | duplicate-edit-own verbs with count evidence         | `MeshOp.Edit`               |
|  [11]   | component extrusion | `ExtrudeLaw`    | `MeshExtruder` column set as one value               | `MeshOp.Extrude`            |
|  [12]   | mesh verbs          | `MeshOp`        | one flat `[Union]`, total generated dispatch         | `Meshes.Build`              |
