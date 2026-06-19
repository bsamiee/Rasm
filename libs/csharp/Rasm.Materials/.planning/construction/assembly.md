# [MATERIALS_ASSEMBLY]

THE HOST-NEUTRAL ELEMENT AND THE IFC MATERIAL-ASSIGNMENT OWNER. One `Element` is a single placed unit — a `profile#PROFILE_OWNER` `Profile` at a `Placement` with an orientation and a cut — and one `MaterialAssignment` `[Union]` closes the IFC 4.3 material-assignment trichotomy every architectural element carries: `LayerSet` (material-plus-thickness layers, walls/slabs/IGUs), `ProfileSet` (one material per extruded `Profile`, members), `ConstituentSet` (keyword-tagged components, curtain walls/composite assemblies). An `Element` carries exactly one assignment, the assignment selecting how the element resolves to a placement stream — a wall is a `LayerSet` buildup, a beam a `ProfileSet` extrusion, a curtain wall a `ConstituentSet`. The model is HOST-NEUTRAL: a `Placement` carries a station/elevation/run/rise/path-angle scalar tuple plus the orientation and cut, never a `Rhino.Geometry` curve or transform — the host boundary materializes the placement stream at the app root, this owner produces only the portable data the wire and the appearance engine consume, and the assignment serializes to IFC 4.3 (`IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet`) for `Rasm.Bim`. The page composes `profile#PROFILE_OWNER` for the `Profile` shape, the `Rasm` kernel for the scalar length algebra, and the `appearance/graph#MATERIAL_LIBRARY` `MaterialId` a placed element's `Profile` maps to; the `construction/layout#ASSEMBLY_FOLD` resolves an assignment to a placement stream.

## [1]-[INDEX]

The page's two clusters, each owning one disjoint layer of the host-neutral construction model.

- `[2]-[ELEMENT_MODEL]`: the `Element` placed-unit shape, the `Placement` scalar tuple, the `RunPath` line/arc length algebra, and the `ConstructionFault` band-2350 union.
- `[3]-[MATERIAL_ASSIGNMENT]`: the `MaterialAssignment` `[Union]` layer-set/profile-set/constituent-set trichotomy, the element-to-assignment composition, and the IFC 4.3 alignment.

## [2]-[ELEMENT_MODEL]

- Owner: `Element` placed-unit shape; `Placement` the host-neutral scalar tuple; `RunPath` `[Union]` the path geometry; `ConstructionFault` `[Union]` band 2350.
- Cases: path {line (length), arc (radius, sweep)} — the closed `RunPath` set; an element is a `Profile` placed at a `Placement` carrying one `MaterialAssignment`, never a path subtype.
- Entry: `public static Fin<double> LengthOf(RunPath path, Op key)` — the line/arc arc-length algebra (`Fin<T>` aborts on a non-positive length/radius/sweep, `ConstructionFault.Path`); `RunPathAlgebra.AngleAt` projects a station onto a path angle so a curved run reads its local rotation without a host curve.
- Packages: Rasm (project — scalar geometry), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new path geometry is one `RunPath` case (spline/polyline) carrying its arc-length arm; a new fault is one `ConstructionFault` case; a placed-unit attribute shared by all families is one `Placement`/`Element` column — never a per-path placement method, never a per-family element type.
- Boundary: `Placement` is HOST-NEUTRAL — it carries station/elevation/run/rise/path-angle plus the `NormalOffsetMm` layer-buildup offset along the path normal as raw scalars, plus the orientation and cut, NEVER a `Rhino.Geometry.Plane`/`Transform`/curve; the `NormalOffsetMm` defaults to zero for a single-ply run and carries the cumulative layer offset for a `construction/layout#ASSEMBLY_FOLD` `LayerSet` buildup so the host materializes each ply at its depth without a second placement owner; the host boundary at the app root materializes the placement stream into geometry, this owner produces only portable data the wire and the appearance engine read; `RunPath` is the closed path geometry and `LengthOf` the one arc-length algebra (a line is its length, an arc is `radius · sweep · π/180`), so a curved run never re-derives arc length per call site; `ConstructionFault` is the one fault every `Fin.Fail` reads (path/joint/course/opening slots), an `Expected`-derived `Error` (`IValidationError<ConstructionFault>`) whose 2350 band IS the `Expected` `Code` so a bare typed case lifts directly into the `Fin<T>` rail, so a layout never throws and never returns a sentinel placement; the orientation/cut vocabulary is the `masonry#PROFILE_FAMILY` `Orientation`/`Cut` algebra composed, never re-minted here.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[Union]
public abstract partial record RunPath {
    private RunPath() { }
    public sealed record Line(double LengthMm) : RunPath;
    public sealed record Arc(double RadiusMm, double SweepDegrees) : RunPath;
}

// --- [ERRORS] ------------------------------------------------------------------------------
[Union]
public abstract partial record ConstructionFault : Expected, IValidationError<ConstructionFault> {
    private ConstructionFault(Op key, string detail) : base(detail, 2350, None) => Key = key;
    public Op Key { get; }
    public static ConstructionFault Create(string message) => new Path(default, message);
    public sealed record Path(Op Key, string Detail) : ConstructionFault(Key, Detail) { public override string Category => "Path"; }
    public sealed record Joint(Op Key, string Detail) : ConstructionFault(Key, Detail) { public override string Category => "Joint"; }
    public sealed record Course(Op Key, string Detail) : ConstructionFault(Key, Detail) { public override string Category => "Course"; }
    public sealed record Opening(Op Key, string Detail) : ConstructionFault(Key, Detail) { public override string Category => "Opening"; }
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct Placement(
    int Course,
    int Sequence,
    double StationMm,
    double ElevationMm,
    double RunMm,
    double RiseMm,
    double PathAngleDegrees,
    Orientation Orientation,
    Cut Cut,
    double NormalOffsetMm = 0.0);

public sealed record Element(Profile Profile, Placement Placement, MaterialAssignment Assignment);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class RunPathAlgebra {
    public static Fin<double> LengthOf(RunPath path, Op key) =>
        path.Switch(
            state: key,
            line: static (k, line) => double.IsFinite(line.LengthMm) && line.LengthMm > 0.0
                ? Fin.Succ(line.LengthMm)
                : Fin.Fail<double>(ConstructionFault.Path(k, "<run-path-degenerate>")),
            arc: static (k, arc) => double.IsFinite(arc.RadiusMm) && arc.RadiusMm > 0.0 && Math.Abs(arc.SweepDegrees) > 0.0
                ? Fin.Succ(arc.RadiusMm * (Math.PI / 180.0) * Math.Abs(arc.SweepDegrees))
                : Fin.Fail<double>(ConstructionFault.Path(k, "<run-path-degenerate>")));

    public static double AngleAt(RunPath path, double stationMm) =>
        path.Switch(
            state: stationMm,
            line: static (_, _) => 0.0,
            arc: static (station, arc) => Math.Sign(arc.SweepDegrees) * station / arc.RadiusMm * 180.0 / Math.PI);
}
```

## [3]-[MATERIAL_ASSIGNMENT]

- Owner: `MaterialAssignment` `[Union]` closing the IFC 4.3 material-assignment trichotomy; `MaterialLayer` the layer-set row; `MaterialConstituent` the constituent row; the element-to-assignment composition.
- Cases: `LayerSet` (a `Seq<MaterialLayer>` of material-plus-thickness layers, walls/slabs/IGUs — `IfcMaterialLayerSet`) · `ProfileSet` (one `MaterialId` per extruded `Profile`, members — `IfcMaterialProfileSet`) · `ConstituentSet` (a `Seq<MaterialConstituent>` of keyword-tagged components, curtain walls/composites — `IfcMaterialConstituentSet`).
- Entry: `public static Fin<MaterialAssignment> LayerSet(Seq<MaterialLayer> layers, Op key)` · `ProfileSet(MaterialId material, Profile profile, Op key)` · `ConstituentSet(Seq<MaterialConstituent> constituents, Op key)` — three smart-constructors discriminating the assignment shape, each `Fin<T>` aborting on an empty set or a non-positive layer thickness (`ConstructionFault.Path`); `TotalThickness` reads the layer-set buildup depth, `Materials` projects the assigned `MaterialId` set for the appearance engine.
- Packages: Rasm (project — `Dimension`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: the IFC trichotomy is closed at three cases (`IfcMaterialList` is deprecated and never admitted); a new layer attribute is one `MaterialLayer` column, a new constituent keyword one `MaterialConstituent` column — never a fourth assignment case, never a per-element-type assignment. An element selects its assignment shape by domain: a wall/slab is a `LayerSet`, a beam/column a `ProfileSet`, a curtain wall a `ConstituentSet`.
- Boundary: `MaterialAssignment` is the ONE assignment owner — a per-element-type assignment class is the deleted form; a `LayerSet` layer's `MaterialId` crosses to `appearance/graph#MATERIAL_LIBRARY` for shading and its `Dimension` thickness composes the `Rasm` kernel value-object so a buildup never re-mints a length; a `ProfileSet` carries the `profile#PROFILE_OWNER` `Profile` the `construction/layout#ASSEMBLY_FOLD` extrudes along the `RunPath`; a `ConstituentSet` keyword tags map to IFC `Category` strings on the wire; the assignment serializes to the IFC 4.3 trichotomy for `Rasm.Bim` (`IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet`) and is consumed host-neutral here — no `Rhino.Geometry` type and no `IfcOpenShell` evaluation crosses this owner; the masonry run is layout-resolution of a `LayerSet` or `ProfileSet`, never a fourth special-case.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct MaterialLayer(MaterialId Material, Dimension ThicknessMm, string LayerName);

public readonly record struct MaterialConstituent(MaterialId Material, string Category);

[Union]
public abstract partial record MaterialAssignment {
    private MaterialAssignment() { }

    public sealed record LayerSet(Seq<MaterialLayer> Layers) : MaterialAssignment {
        public double TotalThickness => Layers.Sum(l => l.ThicknessMm.Value);
    }
    public sealed record ProfileSet(MaterialId Material, Profile Profile) : MaterialAssignment;
    public sealed record ConstituentSet(Seq<MaterialConstituent> Constituents) : MaterialAssignment;

    public Seq<MaterialId> Materials => Switch(
        layerSet:      s => s.Layers.Map(l => l.Material),
        profileSet:    s => Seq1(s.Material),
        constituentSet: s => s.Constituents.Map(c => c.Material));

    public static Fin<MaterialAssignment> LayerSet(Seq<MaterialLayer> layers, Op key) =>
        layers.IsEmpty || layers.Exists(l => l.ThicknessMm.Value <= 0.0)
            ? Fin.Fail<MaterialAssignment>(ConstructionFault.Path(key, "<layer-set-empty-or-nonpositive-thickness>"))
            : Fin.Succ<MaterialAssignment>(new LayerSet(layers));

    public static Fin<MaterialAssignment> ProfileSet(MaterialId material, Profile profile, Op key) =>
        Fin.Succ<MaterialAssignment>(new ProfileSet(material, profile));

    public static Fin<MaterialAssignment> ConstituentSet(Seq<MaterialConstituent> constituents, Op key) =>
        constituents.IsEmpty
            ? Fin.Fail<MaterialAssignment>(ConstructionFault.Course(key, "<constituent-set-empty>"))
            : Fin.Succ<MaterialAssignment>(new ConstituentSet(constituents));
}
```

## [4]-[RESEARCH]

- [IFC_MATERIAL_ASSIGNMENT]: the IFC 4.3 material-assignment trichotomy is the canonical host-neutral model — `IfcMaterialLayerSet` (walls and slabs), `IfcMaterialProfileSet` (extruded members), `IfcMaterialConstituentSet` (components); `IfcMaterialList` is deprecated and never admitted. The `MaterialAssignment` union is the single closed owner; the probe is the per-case IFC member-shape mapping for the `Rasm.Bim` wire (`IfcMaterialLayer.LayerThickness`/`IfcMaterialProfile.Profile`/`IfcMaterialConstituent.Category`), authored as portable data here and serialized at the BIM boundary. The masonry run becomes layout-resolution of a `LayerSet` or `ProfileSet`, the largest construction capability the linear-run-only model lacked.
- [LAYER_BUILDUP_GEOMETRY]: a `LayerSet` buildup resolves to a stacked placement stream where each layer offsets by its cumulative thickness along the path normal; the `construction/layout#ASSEMBLY_FOLD` reads `LayerSet.TotalThickness` and the per-layer offset to fold the wall plies into a placement stream the host materializes. The probe is the per-layer offset projection (the cumulative-thickness fold), queued with the layout stages.
