# [RASM_RHINO_MODELING_SUBD]

`SubDs.Build` owns SubD construction, interpolation, value-semantic editing, crease topology, edge projection, and Brep egress. `SubDOp` admits raw handles once, every policy enters through a generated owner, and every product exits through `Built<SubDSlot>`. `MeshOp.QuadRemesh` feeds `SubDOp.FromMesh`, while `SweepFrameLaw` carries one-rail framing directly on `SubDOp.FromSweepOne`.

## [01]-[INDEX]

- [02]-[ADMISSION]: `CreasePreset`, SubD topology policies, `SubDCreationSpec`, `SubDCreationLaw`, `SubDBrepLaw`, and `SubDEdgeSelection`.
- [03]-[ALGEBRA]: `SubDEditVerb`, `SubDOp`, and `SubDSlot`.
- [04]-[EXECUTION]: `SubDs.Build` and the value-semantic native fold.

## [02]-[ADMISSION]

`SubDCreationSpec` and `SubDBrepLaw` validate policy before native carriers exist, and each consuming seam rejects a zero-initialized smart-enum slot before native projection. Smart-enum rows project closure, corner, crease, symmetry, packing, interpolation, and edge-clamping choices onto native booleans only inside a consuming arm; `MeshShell` owns the shared open-or-solid offset policy. Primitive topologies remain direct `SubDOp` cases. `SubDEdgeSelection` separates edge location from edge character, so contradictory native flag combinations cannot cross admission. Component-addressed edits admit nonnegative indices at the value boundary and bind them to the live vertex, edge, or face roster before mutation.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CreasePreset {
    public static readonly CreasePreset Smooth = new(key: 0, static () => SubDCreationOptions.Smooth);
    public static readonly CreasePreset InteriorCreases = new(key: 1, static () => SubDCreationOptions.InteriorCreases);
    public static readonly CreasePreset ConvexCorners = new(key: 2, static () => SubDCreationOptions.ConvexCornersAndInteriorCreases);
    public static readonly CreasePreset AllCorners = new(key: 3, static () => SubDCreationOptions.ConvexAndConcaveCornersAndInteriorCreases);

    [UseDelegateFromConstructor]
    internal partial SubDCreationOptions Mint();
}

[SmartEnum<int>]
public sealed partial class SubDClosure {
    public static readonly SubDClosure Open = new(key: 0, native: false);
    public static readonly SubDClosure Closed = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class SubDCorners {
    public static readonly SubDCorners Smooth = new(key: 0, native: false);
    public static readonly SubDCorners Cornered = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class SubDLoftFeatures {
    public static readonly SubDLoftFeatures Smooth = new(key: 0, native: (false, false));
    public static readonly SubDLoftFeatures Cornered = new(key: 1, native: (true, false));
    public static readonly SubDLoftFeatures Creased = new(key: 2, native: (false, true));
    public static readonly SubDLoftFeatures CorneredAndCreased = new(key: 3, native: (true, true));

    internal (bool Corners, bool Creases) Native { get; }
}

[SmartEnum<int>]
public sealed partial class SubDJoinPolicy {
    public static readonly SubDJoinPolicy Smooth = new(key: 0, native: (false, true));
    public static readonly SubDJoinPolicy Creased = new(key: 1, native: (true, true));
    public static readonly SubDJoinPolicy SmoothAsymmetric = new(key: 2, native: (false, false));
    public static readonly SubDJoinPolicy CreasedAsymmetric = new(key: 3, native: (true, false));

    internal (bool Creases, bool PreserveSymmetry) Native { get; }
}

[SmartEnum<int>]
public sealed partial class SubDFacePacking {
    public static readonly SubDFacePacking Unpacked = new(key: 0, native: false);
    public static readonly SubDFacePacking Packed = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class SubDVertexInterpolation {
    public static readonly SubDVertexInterpolation ControlNet = new(key: 0, native: false);
    public static readonly SubDVertexInterpolation LimitSurface = new(key: 1, native: true);

    internal bool Native { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SubDCreationLaw {
    private SubDCreationLaw() { }
    public sealed record Preset(CreasePreset Row) : SubDCreationLaw;
    public sealed record Custom(SubDCreationSpec Value) : SubDCreationLaw;

    internal bool Admissible => Switch(
        preset: static law => law.Row is not null,
        custom: static law => law.Value.Admissible);

    internal Fin<SubDCreationOptions> Rig(Op key) =>
        key.Catch(() => Fin.Succ(value: Switch(
            preset: static law => law.Row.Mint(),
            custom: static law => new SubDCreationOptions {
                InteriorCreaseTest = law.Value.InteriorCrease,
                ConvexCornerTest = law.Value.ConvexCorner,
                ConcaveCornerTest = law.Value.ConcaveCorner,
                TextureCoordinateTest = law.Value.TextureCoordinates,
                MaximumConvexCornerEdgeCount = law.Value.MaximumConvexCornerEdgeCount,
                MaximumConvexCornerAngleRadians = law.Value.MaximumConvexCornerAngleRadians,
                MinimumConcaveCornerAngleRadians = law.Value.MinimumConcaveCornerAngleRadians,
                MinimumConcaveCornerEdgeCount = law.Value.MinimumConcaveCornerEdgeCount,
                InterpolateMeshVertices = law.Value.VertexInterpolation.Native,
            })));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SubDCreationSpec {
    public SubDCreationOptions.InteriorCreaseOption InteriorCrease { get; }
    public SubDCreationOptions.ConvexCornerOption ConvexCorner { get; }
    public SubDCreationOptions.ConcaveCornerOption ConcaveCorner { get; }
    public SubDCreationOptions.TextureCoordinateOption TextureCoordinates { get; }
    public uint MaximumConvexCornerEdgeCount { get; }
    public double MaximumConvexCornerAngleRadians { get; }
    public double MinimumConcaveCornerAngleRadians { get; }
    public uint MinimumConcaveCornerEdgeCount { get; }
    public SubDVertexInterpolation VertexInterpolation { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref SubDCreationOptions.InteriorCreaseOption interiorCrease,
        ref SubDCreationOptions.ConvexCornerOption convexCorner,
        ref SubDCreationOptions.ConcaveCornerOption concaveCorner,
        ref SubDCreationOptions.TextureCoordinateOption textureCoordinates,
        ref uint maximumConvexCornerEdgeCount,
        ref double maximumConvexCornerAngleRadians,
        ref double minimumConcaveCornerAngleRadians,
        ref uint minimumConcaveCornerEdgeCount,
        ref SubDVertexInterpolation vertexInterpolation) {
        if (!Admits(
            interiorCrease, convexCorner, concaveCorner, textureCoordinates,
            maximumConvexCornerEdgeCount, maximumConvexCornerAngleRadians,
            minimumConcaveCornerAngleRadians, minimumConcaveCornerEdgeCount, vertexInterpolation)) {
            validationError = new ValidationError("SubD corner thresholds are inconsistent.");
        }
    }

    internal bool Admissible => Admits(
        InteriorCrease, ConvexCorner, ConcaveCorner, TextureCoordinates,
        MaximumConvexCornerEdgeCount, MaximumConvexCornerAngleRadians,
        MinimumConcaveCornerAngleRadians, MinimumConcaveCornerEdgeCount, VertexInterpolation);

    private static bool Admits(
        SubDCreationOptions.InteriorCreaseOption interiorCrease,
        SubDCreationOptions.ConvexCornerOption convexCorner,
        SubDCreationOptions.ConcaveCornerOption concaveCorner,
        SubDCreationOptions.TextureCoordinateOption textureCoordinates,
        uint maximumConvexCornerEdgeCount,
        double maximumConvexCornerAngleRadians,
        double minimumConcaveCornerAngleRadians,
        uint minimumConcaveCornerEdgeCount,
        SubDVertexInterpolation? vertexInterpolation) =>
        Enum.IsDefined(interiorCrease)
        && Enum.IsDefined(convexCorner)
        && Enum.IsDefined(concaveCorner)
        && Enum.IsDefined(textureCoordinates)
        && vertexInterpolation is not null
        && maximumConvexCornerEdgeCount >= 2
        && minimumConcaveCornerEdgeCount >= 3
        && double.IsFinite(maximumConvexCornerAngleRadians)
        && double.IsFinite(minimumConcaveCornerAngleRadians)
        && maximumConvexCornerAngleRadians > 0.0
        && minimumConcaveCornerAngleRadians > maximumConvexCornerAngleRadians;
}

// --- [MODELS] -----------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SubDBrepLaw {
    public SubDFacePacking Packing { get; }
    public SubDToBrepOptions.ExtraordinaryVertexProcessOption VertexProcess { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref SubDFacePacking packing,
        ref SubDToBrepOptions.ExtraordinaryVertexProcessOption vertexProcess) {
        if (!Admits(packing, vertexProcess)) {
            validationError = new ValidationError("SubD Brep policy is invalid.");
        }
    }

    internal bool Admissible => Admits(Packing, VertexProcess);

    internal Fin<SubDToBrepOptions> Rig(Op key) =>
        key.Catch(() => Fin.Succ(value: new SubDToBrepOptions(packFaces: Packing.Native, vertexProcess: VertexProcess)));

    private static bool Admits(
        SubDFacePacking? packing,
        SubDToBrepOptions.ExtraordinaryVertexProcessOption vertexProcess) =>
        packing is not null && Enum.IsDefined(vertexProcess);
}

[SmartEnum<int>]
public sealed partial class SubDEdgeLocation {
    public static readonly SubDEdgeLocation Boundary = new(key: 0, native: (true, false));
    public static readonly SubDEdgeLocation Interior = new(key: 1, native: (false, true));

    internal (bool Boundary, bool Interior) Native { get; }
}

[SmartEnum<int>]
public sealed partial class SubDEdgeCharacter {
    public static readonly SubDEdgeCharacter Smooth = new(key: 0, native: (true, false, false));
    public static readonly SubDEdgeCharacter Sharp = new(key: 1, native: (false, true, false));
    public static readonly SubDEdgeCharacter Crease = new(key: 2, native: (false, false, true));

    internal (bool Smooth, bool Sharp, bool Crease) Native { get; }
}

[SmartEnum<int>]
public sealed partial class SubDEdgeClamp {
    public static readonly SubDEdgeClamp Natural = new(key: 0, native: false);
    public static readonly SubDEdgeClamp Clamped = new(key: 1, native: true);

    internal bool Native { get; }
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SubDEdgeSelection {
    public Option<SubDEdgeLocation> Location { get; }
    public Option<SubDEdgeCharacter> Character { get; }
    public SubDEdgeClamp Ends { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Option<SubDEdgeLocation> location,
        ref Option<SubDEdgeCharacter> character,
        ref SubDEdgeClamp ends) {
        if (!Admits(location, character, ends)) {
            validationError = new ValidationError("SubD edge selection is invalid.");
        }
    }

    internal bool Admissible => Admits(Location, Character, Ends);

    internal (bool Boundary, bool Interior, bool Smooth, bool Sharp, bool Crease, bool Clamp) Native {
        get {
            (bool boundary, bool interior) = Location.Map(static row => row.Native).IfNone((false, false));
            (bool smooth, bool sharp, bool crease) = Character.Map(static row => row.Native).IfNone((false, false, false));
            return (boundary, interior, smooth, sharp, crease, Ends.Native);
        }
    }

    private static bool Admits(
        Option<SubDEdgeLocation> location,
        Option<SubDEdgeCharacter> character,
        SubDEdgeClamp? ends) =>
        ends is not null
        && location.ForAll(static row => row is not null)
        && character.ForAll(static row => row is not null);
}
```

## [03]-[ALGEBRA]

`SubDOp` is the sole construction algebra. `SubDEditVerb` duplicates one borrowed target; whole-surface and indexed interpolation are direct cases because their admission, identity, timing, and consumer coincide. One- and two-rail sweeps are direct `SubDOp` cases for the same reason. Tag, interpolation, subdivision, and transform edits refresh tags, sector coefficients, and the surface cache before ownership crosses `ModelGate`.

- Law: quad-remesh composition feeds the meshing rail's `QuadRemesh` product to `FromMesh`; no second remesh entry exists here.
- Growth: a new subd constructor or edit verb is one case with its arm; the spine and every consumer read it with zero new surface.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SubDSlot {
    public static readonly SubDSlot Converted = new(key: 0);
    public static readonly SubDSlot Fitted = new(key: 1);
    public static readonly SubDSlot Lofted = new(key: 2);
    public static readonly SubDSlot Swept = new(key: 3);
    public static readonly SubDSlot Seeded = new(key: 4);
    public static readonly SubDSlot Joined = new(key: 5);
    public static readonly SubDSlot Edited = new(key: 6);
    public static readonly SubDSlot Brepped = new(key: 7);
    public static readonly SubDSlot EdgeCurves = new(key: 8);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SubDEditVerb {
    private SubDEditVerb() { }
    public sealed record SubdivideAll(int Count = 1) : SubDEditVerb;
    public sealed record SubdivideFaces(Seq<int> Faces) : SubDEditVerb;
    public sealed record InterpolateSurface(Seq<Point3d> Points) : SubDEditVerb;
    public sealed record InterpolateVertices(Seq<uint> Indices, Seq<Point3d> Points) : SubDEditVerb;
    public sealed record SetVertexPoint(uint Vertex, Point3d SurfacePoint) : SubDEditVerb;
    public sealed record Shell(double Distance, MeshShell Kind) : SubDEditVerb;
    public sealed record MergeCoplanar : SubDEditVerb;
    public sealed record Pack : SubDEditVerb;
    public sealed record Flip : SubDEditVerb;
    public sealed record TagVertices(Seq<int> Vertices, SubDVertexTag Tag) : SubDEditVerb;
    public sealed record TagEdges(Seq<int> Edges, SubDEdgeTag Tag) : SubDEditVerb;
    public sealed record MoveComponents(Seq<ComponentIndex> Components, Transform Motion, SubDComponentLocation Location) : SubDEditVerb;
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SubDOp {
    private SubDOp() { }
    public sealed record FromMesh(GeometryHandle Source, SubDCreationLaw Law) : SubDOp;
    public sealed record FromSurface(GeometryHandle Source, SubDFromSurfaceMethods Method, SubDCorners Corners) : SubDOp;
    public sealed record FromLoft(Seq<GeometryHandle> Shapes, SubDClosure Closure, SubDLoftFeatures Features, int Divisions) : SubDOp;
    public sealed record FromSweepOne(GeometryHandle Rail, SweepFrameLaw Frame, Seq<GeometryHandle> Shapes, SubDClosure Closure, SubDCorners Corners) : SubDOp;
    public sealed record FromSweepTwo(GeometryHandle Rail1, GeometryHandle Rail2, Seq<GeometryHandle> Shapes, SubDClosure Closure, SubDCorners Corners) : SubDOp;
    public sealed record SeedQuadSphere(Sphere Value, SubDComponentLocation VertexLocation, uint SubdivisionLevel) : SubDOp;
    public sealed record SeedGlobeSphere(Sphere Value, SubDComponentLocation VertexLocation, uint AxialFaceCount, uint EquatorialFaceCount) : SubDOp;
    public sealed record SeedTriSphere(Sphere Value, SubDComponentLocation VertexLocation, uint SubdivisionLevel) : SubDOp;
    public sealed record SeedIcosahedron(Sphere Value, SubDComponentLocation VertexLocation) : SubDOp;
    public sealed record SeedCylinder(Cylinder Value, uint CircumferenceFaceCount, uint HeightFaceCount, SubDEndCapStyle EndCap, SubDEdgeTag EndCapEdgeTag, SubDComponentLocation RadiusLocation) : SubDOp;
    public sealed record Join(Seq<GeometryHandle> Targets, SubDJoinPolicy Policy) : SubDOp;
    public sealed record Edit(GeometryHandle Target, SubDEditVerb Verb) : SubDOp;
    public sealed record ToBrep(GeometryHandle Target, SubDBrepLaw Law) : SubDOp;
    public sealed record EdgeCurves(GeometryHandle Target, SubDEdgeSelection Selection) : SubDOp;

    internal Fin<SubDOp> Admitted(Op key) =>
        guard(this switch {
            FromMesh edit => edit.Source is not null && edit.Law is { Admissible: true },
            FromSurface edit => edit.Source is not null && edit.Corners is not null,
            FromLoft edit => Handles(edit.Shapes)
                && edit.Closure is not null
                && (!edit.Closure.Native || edit.Shapes.Count >= 3)
                && edit.Features is not null
                && edit.Divisions > 0,
            FromSweepOne edit => edit.Rail is not null && edit.Frame is { Admissible: true } && Handles(edit.Shapes)
                && edit.Closure is not null && edit.Corners is not null,
            FromSweepTwo edit => edit.Rail1 is not null && edit.Rail2 is not null && Handles(edit.Shapes)
                && edit.Closure is not null && edit.Corners is not null,
            SeedQuadSphere edit => edit.Value.IsValid,
            SeedGlobeSphere edit => edit.Value.IsValid && edit.AxialFaceCount > 0 && edit.EquatorialFaceCount >= 3,
            SeedTriSphere edit => edit.Value.IsValid,
            SeedIcosahedron edit => edit.Value.IsValid,
            SeedCylinder edit => edit.Value.IsValid && edit.CircumferenceFaceCount >= 3 && edit.HeightFaceCount > 0,
            Join edit => Handles(edit.Targets) && edit.Policy is not null,
            Edit edit => edit.Target is not null && EditAdmissible(edit.Verb),
            ToBrep edit => edit.Target is not null && edit.Law.Admissible,
            EdgeCurves edit => edit.Target is not null && edit.Selection.Admissible,
            _ => false,
        }, key.InvalidInput()).ToFin().Map(_ => this);

    private static bool EditAdmissible(SubDEditVerb? verb) => verb switch {
        SubDEditVerb.SubdivideAll { Count: > 0 } => true,
        SubDEditVerb.SubdivideFaces edit => !edit.Faces.IsEmpty && edit.Faces.ForAll(static face => face >= 0),
        SubDEditVerb.InterpolateSurface edit => !edit.Points.IsEmpty && edit.Points.ForAll(static point => point.IsValid),
        SubDEditVerb.InterpolateVertices edit => !edit.Indices.IsEmpty && edit.Indices.Count == edit.Points.Count
            && edit.Indices.ForAll(static index => index >= 0)
            && edit.Points.ForAll(static point => point.IsValid),
        SubDEditVerb.SetVertexPoint edit => edit.Vertex >= 0 && edit.SurfacePoint.IsValid,
        SubDEditVerb.Shell edit => double.IsFinite(edit.Distance) && edit.Distance != 0.0 && edit.Kind is not null,
        SubDEditVerb.TagVertices edit => !edit.Vertices.IsEmpty && edit.Vertices.ForAll(static vertex => vertex >= 0),
        SubDEditVerb.TagEdges edit => !edit.Edges.IsEmpty && edit.Edges.ForAll(static edge => edge >= 0),
        SubDEditVerb.MoveComponents edit => !edit.Components.IsEmpty && edit.Motion.IsValid && !edit.Motion.IsZero,
        SubDEditVerb.MergeCoplanar or SubDEditVerb.Pack or SubDEditVerb.Flip => true,
        _ => false,
    };

    private static bool Handles(Seq<GeometryHandle> handles) =>
        !handles.IsEmpty && handles.ForAll(static handle => handle is not null);

    internal Fin<Built<SubDSlot>> Apply(Context domain) =>
        Switch(
            domain,
            fromMesh: static (_, edit) => {
                Op op = Op.Of(name: nameof(FromMesh));
                return ModelGate.Borrow<Mesh, Built<SubDSlot>>(handle: edit.Source, key: op, body: mesh =>
                    from options in edit.Law.Rig(key: op)
                    from built in op.Catch(() => {
                        using SubDCreationOptions live = options;
                        return ModelGate.Single(op, SubDSlot.Converted, () => SubD.CreateFromMesh(mesh: mesh, options: live));
                    })
                    select built);
            },
            fromSurface: static (_, edit) => {
                Op op = Op.Of(name: nameof(FromSurface));
                return ModelGate.Borrow<Surface, Built<SubDSlot>>(handle: edit.Source, key: op, body: surface =>
                    ModelGate.Single(op, SubDSlot.Fitted, () => SubD.CreateFromSurface(
                        surface: surface, method: edit.Method, corners: edit.Corners.Native)));
            },
            fromLoft: static (_, edit) => {
                Op op = Op.Of(name: nameof(FromLoft));
                return ModelGate.BorrowMany<NurbsCurve, Built<SubDSlot>>(handles: edit.Shapes, key: op, body: shapes => {
                    (bool corners, bool creases) = edit.Features.Native;
                    return ModelGate.Single(op, SubDSlot.Lofted, () => SubD.CreateFromLoft(
                        curves: shapes.AsIterable(), closed: edit.Closure.Native, addCorners: corners,
                        addCreases: creases, divisions: edit.Divisions));
                });
            },
            fromSweepOne: static (_, edit) => {
                Op op = Op.Of(name: nameof(FromSweepOne));
                return ModelGate.BorrowMany<NurbsCurve, Built<SubDSlot>>(handles: edit.Shapes, key: op, body: shapes =>
                    ModelGate.Borrow<NurbsCurve, Built<SubDSlot>>(handle: edit.Rail, key: op, body: rail => {
                        (SweepFrame frame, Vector3d normal) = edit.Frame.Native;
                        return ModelGate.Single(op, SubDSlot.Swept, () => SubD.CreateFromSweep(
                            rail1: rail, shapes: shapes.AsIterable(), closed: edit.Closure.Native, addCorners: edit.Corners.Native,
                            roadlikeFrame: frame == SweepFrame.Roadlike, roadlikeNormal: normal));
                    }));
            },
            fromSweepTwo: static (_, edit) => {
                Op op = Op.Of(name: nameof(FromSweepTwo));
                return ModelGate.BorrowMany<NurbsCurve, Built<SubDSlot>>(handles: edit.Shapes, key: op, body: shapes =>
                    ModelGate.Borrow<NurbsCurve, Built<SubDSlot>>(handle: edit.Rail1, key: op, body: rail1 =>
                        ModelGate.Borrow<NurbsCurve, Built<SubDSlot>>(handle: edit.Rail2, key: op, body: rail2 =>
                            ModelGate.Single(op, SubDSlot.Swept, () => SubD.CreateFromSweep(
                                rail1: rail1, rail2: rail2, shapes: shapes.AsIterable(),
                                closed: edit.Closure.Native, addCorners: edit.Corners.Native)))));
            },
            seedQuadSphere: static (_, edit) => {
                Op op = Op.Of(name: nameof(SeedQuadSphere));
                return ModelGate.Single(op, SubDSlot.Seeded, () => SubD.CreateQuadSphere(
                    sphere: edit.Value, vertexLocation: edit.VertexLocation, quadSubdivisionLevel: edit.SubdivisionLevel));
            },
            seedGlobeSphere: static (_, edit) => {
                Op op = Op.Of(name: nameof(SeedGlobeSphere));
                return ModelGate.Single(op, SubDSlot.Seeded, () => SubD.CreateGlobeSphere(
                    sphere: edit.Value, vertexLocation: edit.VertexLocation,
                    axialFaceCount: edit.AxialFaceCount, equatorialFaceCount: edit.EquatorialFaceCount));
            },
            seedTriSphere: static (_, edit) => {
                Op op = Op.Of(name: nameof(SeedTriSphere));
                return ModelGate.Single(op, SubDSlot.Seeded, () => SubD.CreateTriSphere(
                    sphere: edit.Value, vertexLocation: edit.VertexLocation, triSubdivisionLevel: edit.SubdivisionLevel));
            },
            seedIcosahedron: static (_, edit) => {
                Op op = Op.Of(name: nameof(SeedIcosahedron));
                return ModelGate.Single(op, SubDSlot.Seeded, () => SubD.CreateIcosahedron(
                    sphere: edit.Value, vertexLocation: edit.VertexLocation));
            },
            seedCylinder: static (_, edit) => {
                Op op = Op.Of(name: nameof(SeedCylinder));
                return ModelGate.Single(op, SubDSlot.Seeded, () => SubD.CreateFromCylinder(
                    cylinder: edit.Value, circumferenceFaceCount: edit.CircumferenceFaceCount, heightFaceCount: edit.HeightFaceCount,
                    endCapStyle: edit.EndCap, endCapEdgeTag: edit.EndCapEdgeTag, radiusLocation: edit.RadiusLocation));
            },
            join: static (model, edit) => {
                Op op = Op.Of(name: nameof(Join));
                return ModelGate.BorrowMany<SubD, Built<SubDSlot>>(handles: edit.Targets, key: op, body: targets => {
                    (bool creases, bool preserveSymmetry) = edit.Policy.Native;
                    return ModelGate.Many(op, SubDSlot.Joined, () => SubD.JoinSubDs(
                        subdsToJoin: targets.AsIterable(), tolerance: model.Absolute.Value,
                        joinedEdgesAreCreases: creases, preserveSymmetry: preserveSymmetry));
                });
            },
            edit: static (model, request) => {
                Op op = Op.Of(name: nameof(Edit));
                return ModelGate.Borrow<SubD, Built<SubDSlot>>(handle: request.Target, key: op, body: source =>
                    op.Catch(() => {
                        SubD working = (SubD)source.Duplicate();
                        return Edited(working: working, verb: request.Verb, domain: model, op: op).Rollback(working);
                    }));
            },
            toBrep: static (_, edit) => {
                Op op = Op.Of(name: nameof(ToBrep));
                return ModelGate.Borrow<SubD, Built<SubDSlot>>(handle: edit.Target, key: op, body: subd =>
                    from options in edit.Law.Rig(key: op)
                    from built in op.Catch(() => {
                        using SubDToBrepOptions live = options;
                        return ModelGate.Single(op, SubDSlot.Brepped, () => subd.ToBrep(options: live));
                    })
                    select built);
            },
            edgeCurves: static (_, edit) => {
                Op op = Op.Of(name: nameof(EdgeCurves));
                return ModelGate.Borrow<SubD, Built<SubDSlot>>(handle: edit.Target, key: op, body: subd => {
                    (bool boundary, bool interior, bool smooth, bool sharp, bool crease, bool clamp) = edit.Selection.Native;
                    return ModelGate.Many(op, SubDSlot.EdgeCurves,
                        () => subd.DuplicateEdgeCurves(boundary, interior, smooth, sharp, crease, clamp),
                        allowEmpty: true);
                });
            });

    private static Fin<Built<SubDSlot>> Edited(SubD working, SubDEditVerb verb, Context domain, Op op) =>
        verb.Switch(
            (Working: working, Domain: domain, Op: op),
            subdivideAll: static (ctx, edit) => ctx.Op.Confirm(success: ctx.Working.Subdivide(count: edit.Count))
                .Bind(_ => Refreshed(ctx.Op, ctx.Working)),
            subdivideFaces: static (ctx, edit) =>
                from _ in guard(edit.Faces.ForAll(face => face < ctx.Working.Faces.Count), ctx.Op.InvalidInput()).ToFin()
                from __ in ctx.Op.Confirm(success: ctx.Working.Subdivide(faceIndices: edit.Faces.AsIterable()))
                from built in Refreshed(ctx.Op, ctx.Working)
                select built,
            interpolateSurface: static (ctx, edit) =>
                from _ in ctx.Op.Confirm(ctx.Working.InterpolateSurfacePoints(edit.Points.ToArray()))
                from built in Refreshed(ctx.Op, ctx.Working)
                select built,
            interpolateVertices: static (ctx, edit) =>
                from _ in guard(edit.Indices.ForAll(index => index < ctx.Working.Vertices.Count), ctx.Op.InvalidInput()).ToFin()
                from __ in ctx.Op.Confirm(ctx.Working.InterpolateSurfacePoints(edit.Indices.ToArray(), edit.Points.ToArray()))
                from built in Refreshed(ctx.Op, ctx.Working)
                select built,
            setVertexPoint: static (ctx, edit) =>
                from _ in guard(edit.Vertex < ctx.Working.Vertices.Count, ctx.Op.InvalidInput()).ToFin()
                from __ in ctx.Op.Confirm(success: ctx.Working.SetVertexSurfacePoint(
                    vertexIndex: edit.Vertex, surfacePoint: edit.SurfacePoint))
                from built in Refreshed(ctx.Op, ctx.Working)
                select built,
            shell: static (ctx, edit) => ModelGate.Owned(ctx.Op, SubDSlot.Edited, ctx.Working,
                () => ctx.Working.Offset(distance: edit.Distance, solidify: edit.Kind.Native)),
            mergeCoplanar: static ctx => ctx.Op
                .Confirm(success: ctx.Working.MergeAllCoplanarFaces(tolerance: ctx.Domain.Absolute.Value, angleTolerance: ctx.Domain.Angle.Value))
                .Bind(_ => Refreshed(ctx.Op, ctx.Working)),
            pack: static ctx => ctx.Op.Catch(() => {
                uint packed = ctx.Working.PackFaces();
                return ModelGate.Kept(ctx.Op, SubDSlot.Edited, ctx.Working, extra: BuildReceipt<SubDSlot>.Of(slot: SubDSlot.Edited, body: new BuildBody.Tally(Count: (int)packed)));
            }),
            flip: static ctx => ctx.Op.Confirm(success: ctx.Working.Flip()).Bind(_ => Refreshed(ctx.Op, ctx.Working)),
            tagVertices: static (ctx, edit) =>
                from _ in guard(edit.Vertices.ForAll(vertex => vertex < ctx.Working.Vertices.Count), ctx.Op.InvalidInput()).ToFin()
                from built in ctx.Op.Catch(() => {
                    ctx.Working.Vertices.SetVertexTags(vertexIndices: edit.Vertices.AsIterable(), tag: edit.Tag);
                    return Refreshed(ctx.Op, ctx.Working);
                })
                select built,
            tagEdges: static (ctx, edit) =>
                from _ in guard(edit.Edges.ForAll(edge => edge < ctx.Working.Edges.Count), ctx.Op.InvalidInput()).ToFin()
                from built in ctx.Op.Catch(() => {
                    ctx.Working.Edges.SetEdgeTags(edgeIndices: edit.Edges.AsIterable(), tag: edit.Tag);
                    return Refreshed(ctx.Op, ctx.Working);
                })
                select built,
            moveComponents: static (ctx, edit) => ctx.Op.Catch(() => {
                uint moved = ctx.Working.TransformComponents(
                    components: edit.Components.AsIterable(), xform: edit.Motion, componentLocation: edit.Location);
                return moved > 0
                    ? Refreshed(ctx.Op, ctx.Working).Map(built => built with {
                        Evidence = built.Evidence + BuildReceipt<SubDSlot>.Of(slot: SubDSlot.Edited, body: new BuildBody.Tally(Count: (int)moved)),
                    })
                    : Fin.Fail<Built<SubDSlot>>(error: ctx.Op.InvalidResult());
            }));

    private static Fin<Built<SubDSlot>> Refreshed(Op op, SubD working) =>
        op.Catch(() => {
            _ = working.UpdateAllTagsAndSectorCoefficients();
            _ = working.UpdateSurfaceMeshCache(lazyUpdate: false);
            return ModelGate.Kept(op, SubDSlot.Edited, working);
        });

}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class SubDs {
    public static Fin<Built<SubDSlot>> Build(Context context, params ReadOnlySpan<SubDOp> operations) =>
        ModelGate.Entry(
            context: context,
            operations: operations,
            admit: static (operation, key) => operation.Admitted(key: key),
            apply: static (operation, model) => operation.Apply(domain: model));
}
```

## [04]-[EXECUTION]

`SubDs.Build` admits every operation before `ModelGate.Folded`. Construction arms own fresh geometry, edit arms duplicate exactly once, and extraction arms detach edge curves. Native `SubDCreationOptions` and `SubDToBrepOptions` live only inside their consuming arm.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
