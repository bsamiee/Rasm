# [MATERIALS_CAPACITY]

THE SECTION-CAPACITY OWNER and THE ONE UTILISATION RAIL. One `SectionCapacity` is the closed structural-capacity surface a `Profiles` cross-section carries beyond its elastic `ComputedSection` — the reinforced-concrete elastic transformed-section properties (`VividOrange.Sections.SectionProperties` `ConcreteSectionProperties`) and the ultimate biaxial Force-Moment-Moment capacity hull (`VividOrange.InteractionDiagram`) over the `Connection/reinforcement#RC_SECTION` `IConcreteSection`, plus the rolled-steel LRFD `DesignCapacity` the `steel#STEEL_FAMILY` already derives — and one `Demand` folded against it is the typed `Utilisation` verdict. A capacity is NEVER a per-section-type calculation: an RC column's N-M-M envelope, an RC beam's transformed neutral-axis depth, and a steel column's `φPn`/`φMn` are `SectionCapacity` `[Union]` cases over one `Demand`/`Utilisation` rail, so a member is checked through one fold differing only in the capacity case — never an `RcColumnCheck`/`SteelBeamCheck` surface. This owner is the ULTIMATE complement to `profile#PROFILE_OWNER` `ParametricSection`: that solver gives the elastic `Area`/`MomentOfInertia` every family computes from its perimeter, THIS owner gives the reinforced-section transformed properties and the ultimate capacity hull the elastic solver does not compute (`.api/api-vividorange-sections-sectionproperties.md` `[property seam]` / `.api/api-vividorange-interactiondiagram.md` `[property seam]`). The capacity surface grows by case — a new capacity kind is one `SectionCapacity` case (admitted only when no existing case's column set carries it), a new demand axis one `Demand` column, a new utilisation metric one `Utilisation` projection. The `InteractionDiagram` constructor RUNS the full eager fibre-integration solve at construction (the `Triangle` section mesh, the `Parallel.For` strain-plane sweep, the `MIConvexHull` hull weld are encapsulated `internal` — this owner composes the welded `IForceMomentMesh`, never the meshing primitive), so a design page constructs the capacity ONCE per section/settings and reads `diagram.Mesh` cached, never re-solving per query. The page composes `Connection/reinforcement#RC_SECTION` `RcSection`/`IConcreteSection` for the RC input, `VividOrange.InteractionDiagram` (`InteractionDiagram`/`DiagramSettings`/`IForceMomentMesh`) for the N-M-M hull, `VividOrange.Sections.SectionProperties` `ConcreteSectionProperties` for the elastic transformed-section properties, `VividOrange.Materials` `AnalysisMaterialFactory` for the `σ(ε)` strengths the fibre integral reads, the in-folder `UnitsNet` `Force`/`Torque`/`Area`/`Length` quantity coercion at the edge, and the `MaterialFault` band-2450 rail for a non-finite or infeasible solve; the capacity surface and the utilisation verdict feed the forward `cs:AEC_SIMULATION_BRIDGE` analysis consumer by `MaterialId`/section key, host-neutral here, the `IForceMomentMesh` round-tripping through `VividOrange.Serialization` for the C#-internal cache.

## [01]-[INDEX]

- [01]-[SECTION_CAPACITY]: the `SectionCapacity` `[Union]` (`RcInteraction` N-M-M hull · `RcElastic` transformed-section · `SteelLrfd` rolled-steel) over the `CapacityFault`-railed admission, the `DiagramResolution` `[SmartEnum]` mesh/sweep-refinement policy folding to a `DiagramSettings`, the `Demand` applied-load shape, the `Utilisation` verdict, and the `SectionCapacity.Resolve` eager-solve boundary over a `Connection/reinforcement#RC_SECTION` `RcSection`.

## [02]-[SECTION_CAPACITY]

- Owner: `SectionCapacity` `[Union]` the closed capacity-surface family; `Demand` the applied (N, My, Mz) load shape; `Utilisation` the typed demand-vs-capacity verdict; `DiagramResolution` the `[SmartEnum]` sweep/mesh-refinement policy folding to a `VividOrange.InteractionDiagram` `DiagramSettings`; `CapacityFault` folds onto the `MaterialFault` band-2450 rail; `SectionCapacity.Resolve` the eager-solve boundary.
- Cases: `RcInteraction` (the ultimate biaxial N-M-M capacity hull as the `IForceMomentMesh` over an `IConcreteSection`, `VividOrange.InteractionDiagram`) · `RcElastic` (the elastic transformed-section reinforcement properties — `TotalReinforcementArea`/`ConcreteArea`/`GeometricReinforcementRatio`/`ReinforcementSecondMomentOfAreaYy`/`Zz`/`EffectiveDepth(SectionFace)`, `VividOrange.Sections.SectionProperties` `ConcreteSectionProperties`) · `SteelLrfd` (the rolled-steel `steel#STEEL_FAMILY` `DesignCapacity` `φMn`/`φPn`/`φVn` lifted into the one rail) — the closed structural-capacity family; a capacity is a `SectionCapacity` case over a section, never a per-section-type check.
- Entry: `public static Fin<SectionCapacity> Resolve(RcSection rc, CapacityKind kind, DiagramResolution resolution, Op key)` — the ONE capacity boundary: it discriminates the `kind` onto the matching solver over the `RcSection.Section` `IConcreteSection`, the `RcInteraction` case constructing `new InteractionDiagram(rc.Section, resolution.ToSettings())` (the eager fibre-integration solve, trapping a degenerate/non-EN-grade solve onto `CapacityFault`) and reading `diagram.Mesh` as the cached `IForceMomentMesh`, the `RcElastic` case constructing `new ConcreteSectionProperties(rc.Section)` and reading the transformed properties, each `UnitsNet` quantity coerced to its SI base once at the edge; `public static SectionCapacity SteelLrfd(DesignCapacity capacity)` lifts the `steel#STEEL_FAMILY` rolled-steel receipt into the rail; `public Utilisation Check(Demand demand)` folds the applied `(N, My, Mz)` demand against the capacity — for `RcInteraction` the ray-cast of the demand vector against the `IForceMomentMesh` hull faces (the demand-magnitude / capacity-boundary-magnitude ratio along the load ray), for `SteelLrfd` the `max(N/φPn, My/φMn)` interaction — one polymorphic verdict, never a `CheckRcColumn`/`CheckSteelBeam` family.
- Packages: VividOrange.InteractionDiagram (`InteractionDiagram`/`DiagramSettings`, the eager-solve ctor + `Mesh`; `.api/api-vividorange-interactiondiagram.md`), VividOrange.IForceMomentInteraction (`IForceMomentMesh`/`IForceMomentVertex`/`IForceMomentTriFace` the hull read through, the `Force`/`Torque` axes; `.api/api-vividorange-iforcemomentinteraction.md`), VividOrange.Sections.SectionProperties (`ConcreteSectionProperties` the transformed-section solver; `.api/api-vividorange-sections-sectionproperties.md`), VividOrange.Sections (`IConcreteSection`/`SectionFace` from the `Connection/reinforcement#RC_SECTION` `RcSection`; `.api/api-vividorange-sections.md`), VividOrange.Materials (`AnalysisMaterialFactory.CreateLinearElastic` the fibre `fcd`/`fyd` the engine reads internally; `.api/api-vividorange-materials.md`), UnitsNet (`Force`/`Torque`/`Area`/`Length`/`Ratio` coerced at the edge; `.api/api-unitsnet.md`), Rasm (project — `PositiveMagnitude`), LanguageExt.Core (`Fin`/`Seq`/`Fold`), Thinktecture.Runtime.Extensions (`[Union]` for `SectionCapacity`, `[SmartEnum]` for `DiagramResolution`/`CapacityKind`). Triangle + MIConvexHull ride transitively INSIDE the `InteractionDiagram` engine (encapsulated `internal`, `.api/api-triangle.md` / `.api/api-miconvexhull.md`) — this owner mints NO direct mesher/hull call, composing only the welded `IForceMomentMesh`.
- Growth: a new capacity kind is one `SectionCapacity` `[Union]` case binding its solver and one `Resolve` arm (a moment-curvature analysis, a shear-capacity envelope, a fire-reduced residual capacity), a new demand axis one `Demand` column (a torsion `Mt`, a biaxial shear), a new utilisation metric one `Utilisation` projection — never a per-section-type capacity surface, never a re-derived elastic property where `ConcreteSectionProperties` computes it, never a direct `Triangle`/`MIConvexHull` call where the `InteractionDiagram` engine welds the hull; the rolled-steel `DesignCapacity` stays the `steel#STEEL_FAMILY` derivation lifted here, never re-computed.
- Boundary: `SectionCapacity.Resolve` is the BOUNDARY_ADMISSION point where the `VividOrange.InteractionDiagram`/`Sections.SectionProperties` surface is admitted EXACTLY ONCE — the `InteractionDiagram` ctor runs the expensive eager solve (`.api/api-vividorange-interactiondiagram.md` `[construction law]`) and a non-EN material whose `IEnConcreteMaterial`/`IEnRebarMaterial` cast the engine cannot read, an under-reinforced degenerate section, or a hull-weld failure rails `CapacityFault` (folded onto `MaterialFault` band 2450) rather than throwing, so no `VividOrange` throw and no infeasible hull reaches an interior signature; the `IForceMomentMesh` is read THROUGH its interface floor (`.api/api-vividorange-iforcemomentinteraction.md` `[LOCAL_ADMISSION]`), never the `ForceMomentMesh` concrete, and the `Force`/`Torque` hull coordinates carry as `UnitsNet` quantities coerced to SI base (`ForceUnit.Kilonewton`/`TorqueUnit.KilonewtonMeter`) once at the edge so no interior signature carries the hull as raw `double`; the `Triangle` section mesher and the `MIConvexHull` hull builder are encapsulated `internal` inside the engine (`.api/api-triangle.md` `[STACKING_LAW]` / `.api/api-miconvexhull.md` `[STACKING_LAW]`) — this AEC-DOMAIN owner mints NO direct mesher/hull call, composing the welded hull through the constructor, the strata-correct seam (the computational-geometry primitives are `Rasm`-kernel-owned, consumed transitively here); the eager solve is cached on the `SectionCapacity` `RcInteraction` carrier (`.api/api-vividorange-interactiondiagram.md` `[LOCAL_ADMISSION]` — construct once per section/settings, never re-solve per query), so a `Check(demand)` reads the cached hull; the `RcInteraction` utilisation is the exact Möller–Trumbore intersection of the origin-cast demand ray against the hull faces (the `IForceMomentTriFace.A`/`B`/`C` the demand vector pierces, the positive front-face pierce `t` the capacity boundary along the load direction), NEVER the facet `Area` `Ratio` read as a physical quantity (`.api/api-vividorange-iforcemomentinteraction.md` `[AXIS_SEMANTICS]`); the capacity surface is host-neutral — the `IForceMomentMesh` round-trips through `VividOrange.Serialization` for the C#-internal cache (`.api/api-vividorange-serialization.md`, distinct from the canonical Thinktecture wire) and the utilisation verdict crosses to `cs:AEC_SIMULATION_BRIDGE` as portable scalar data keyed by section, never a `VividOrange` assembly type crossing the boundary.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using VividOrange.ForceMomentInteraction;            // IForceMomentMesh, IForceMomentVertex, IForceMomentTriFace
using ForceMomentEngine = VividOrange.ForceMomentInteraction.InteractionDiagram;  // the eager-solve engine
using VividOrange.Sections.SectionProperties;        // ConcreteSectionProperties (the RC transformed-section solver)
using VividOrange.Sections;                          // IConcreteSection, SectionFace
using VividOrange.Materials.StandardMaterials.En;    // EnConcreteFactory (the fck the EC2 fctm cracking reference derives from)
using UnitsNet;                                      // Force, Torque, Area, Length, Ratio (coerced at the edge)
using UnitsNet.Units;

// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class CapacityKind {
    public static readonly CapacityKind RcInteraction = new("rc-interaction");   // ultimate N-M-M hull
    public static readonly CapacityKind RcElastic     = new("rc-elastic");       // elastic transformed-section
}

// The InteractionDiagram mesh/sweep-refinement policy folded to a VividOrange.InteractionDiagram DiagramSettings:
// the Steps knob drives a Steps² strain-plane sweep (quadratic cost), so the band trades hull fidelity for solve cost
// rather than scattering a DiagramSettings ctor at the call site (.api/api-vividorange-interactiondiagram.md [default law]).
[SmartEnum<string>]
public sealed partial class DiagramResolution {
    public static readonly DiagramResolution Draft    = new("draft",    steps: 16, concreteMaxAreaMm2: 500.0, rebarDivisions: 12);
    public static readonly DiagramResolution Standard = new("standard", steps: 30, concreteMaxAreaMm2: 250.0, rebarDivisions: 16);
    public static readonly DiagramResolution Fine     = new("fine",     steps: 48, concreteMaxAreaMm2: 120.0, rebarDivisions: 24);
    public int Steps { get; }
    public double ConcreteMaxAreaMm2 { get; }
    public int RebarDivisions { get; }

    public DiagramSettings ToSettings() =>
        new(Area.FromSquareMillimeters(ConcreteMaxAreaMm2), Angle.FromDegrees(25.0),
            Area.FromSquareMillimeters(ConcreteMaxAreaMm2 * 0.8), Angle.FromDegrees(25.0), RebarDivisions, Steps);
}

// --- [MODELS] ------------------------------------------------------------------------------
// The applied design action checked against the capacity surface — axial N plus biaxial moments, SI scalars admitted
// at the edge; the demand vector the RcInteraction hull-ray cast pierces and the SteelLrfd interaction reads.
public readonly record struct Demand(double AxialKn, double MomentYKnm, double MomentZKnm) {
    public double Magnitude => Math.Sqrt(AxialKn * AxialKn + MomentYKnm * MomentYKnm + MomentZKnm * MomentZKnm);
}

// The typed utilisation verdict — the demand/capacity ratio plus the governing axis and the pass flag, never a bare
// double; Ratio > 1 is over-capacity, the Governing naming which load component drives the check.
public readonly record struct Utilisation(double Ratio, string Governing, bool Adequate) {
    public static Utilisation Of(double ratio, string governing) => new(ratio, governing, ratio <= 1.0);
}

// One SectionCapacity [Union] closes the structural-capacity family — the ultimate N-M-M hull, the elastic transformed
// section, and the rolled-steel LRFD receipt — so a member is checked through one Check fold, never a per-type surface.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionCapacity {
    private SectionCapacity() { }

    // The cached ultimate biaxial capacity hull — the IForceMomentMesh held once from the eager InteractionDiagram solve.
    public sealed record RcInteraction(IForceMomentMesh Hull) : SectionCapacity;
    // The elastic transformed-section reinforcement properties (SI scalars from ConcreteSectionProperties) plus the gross
    // section depth + the EC2 concrete flexural tensile limit fctm so the elastic SERVICE bending-stress check is total.
    public sealed record RcElastic(double TotalReinforcementAreaMm2, double ConcreteAreaMm2, double ReinforcementRatio, double InertiaYyMm4, double InertiaZzMm4, double DepthMm, double FctmMpa) : SectionCapacity;
    // The rolled-steel LRFD receipt lifted from steel#STEEL_FAMILY DesignCapacity — never re-derived here.
    public sealed record SteelLrfd(double FlexuralKnm, double CompressionKn, double ShearKn) : SectionCapacity;

    // The demand-vs-capacity verdict: an RcInteraction ray-casts the demand against the hull faces, an RcElastic the EC2
    // SLS extreme-fibre transformed bending-stress ratio σ = M·(h/2)/I against fctm, a SteelLrfd the AISC interaction.
    // One polymorphic Check, never per-type — the elastic arm a genuine service-stress check, not an adequate-by-default 0.
    public Utilisation Check(Demand demand) => Switch(
        rcInteraction: h => CapacityRay.Cast(h.Hull, demand),
        rcElastic:     e => Utilisation.Of(
            Math.Abs(demand.MomentYKnm) * 1e6 * (e.DepthMm * 0.5) / Math.Max(e.InertiaYyMm4, double.Epsilon) / Math.Max(e.FctmMpa, double.Epsilon),
            "elastic-service-stress"),   // σ/fctm > 1 => the transformed section cracks under the service moment
        steelLrfd:     s => Utilisation.Of(
            Math.Max(Math.Abs(demand.AxialKn) / Math.Max(s.CompressionKn, double.Epsilon),
                     Math.Abs(demand.MomentYKnm) / Math.Max(s.FlexuralKnm, double.Epsilon)),
            Math.Abs(demand.AxialKn) / Math.Max(s.CompressionKn, double.Epsilon) >= Math.Abs(demand.MomentYKnm) / Math.Max(s.FlexuralKnm, double.Epsilon) ? "axial" : "flexure"));
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class SectionCapacityResolver {
    // The ONE capacity boundary: discriminate the kind onto its VividOrange solver over the RcSection's IConcreteSection,
    // admit the eager solve ONCE, coerce the UnitsNet outputs to SI scalars at the edge, trap every VividOrange throw onto
    // CapacityFault (MaterialFault band 2450). The InteractionDiagram ctor IS the expensive solve — cached on the carrier.
    public static Fin<SectionCapacity> Resolve(RcSection rc, CapacityKind kind, DiagramResolution resolution, Op key) =>
        kind.Switch(
            rcInteraction: _ => Try(() => new ForceMomentEngine(rc.Section, resolution.ToSettings()).Mesh).ToFin()
                .MapFail(e => CapacityFault(key, $"<rc-interaction-solve:{e.Message}>"))
                .Map(mesh => (SectionCapacity)new SectionCapacity.RcInteraction(mesh)),
            rcElastic: _ => Try(() => new ConcreteSectionProperties(rc.Section)).ToFin()
                .MapFail(e => CapacityFault(key, $"<rc-elastic-solve:{e.Message}>"))
                .Map(p => (SectionCapacity)new SectionCapacity.RcElastic(
                    p.TotalReinforcementArea.SquareMillimeters,
                    p.ConcreteArea.SquareMillimeters,
                    p.GeometricReinforcementRatio.DecimalFractions,
                    p.ReinforcementSecondMomentOfAreaYy.MillimetersToTheFourth,
                    p.ReinforcementSecondMomentOfAreaZz.MillimetersToTheFourth,
                    rc.ConcreteProfile.Unit.HeightMm.Value,
                    Fctm(EnConcreteFactory.CreateLinearElastic(rc.Concrete.Grade).Strength.Megapascals))));

    public static SectionCapacity SteelLrfd(DesignCapacity capacity) =>
        new SectionCapacity.SteelLrfd(capacity.FlexuralNmm * 1e-6, capacity.CompressionN * 1e-3, capacity.ShearN * 1e-3);

    // EC2 mean flexural tensile strength from fck: fctm = 0.30·fck^(2/3) for ≤C50, 2.12·ln(1+(fck+8)/10) above —
    // the cracking-stress reference the RcElastic service check compares the transformed extreme-fibre stress against.
    static double Fctm(double fckMpa) =>
        fckMpa <= 50.0 ? 0.30 * Math.Pow(fckMpa, 2.0 / 3.0) : 2.12 * Math.Log(1.0 + (fckMpa + 8.0) / 10.0);

    static MaterialFault CapacityFault(Op key, string detail) => MaterialFault.Parameter(key, detail);
}

// The biaxial utilisation: cast the demand (N, My, Mz) load ray from the origin against the closed IForceMomentMesh
// capacity onion and read the ratio of the demand magnitude to the ray's pierce magnitude on the hull surface — the
// exact Möller–Trumbore ray-triangle intersection over the IForceMomentTriFace.A/B/C facets, not a vertex surrogate.
// The hull is convex and origin-interior (the safe zero-load state is enclosed), so a single positive pierce t exists.
public static class CapacityRay {
    public static Utilisation Cast(IForceMomentMesh hull, Demand demand) {
        double demandMag = demand.Magnitude;
        (double dirN, double dirMy, double dirMz) = demandMag > double.Epsilon
            ? (demand.AxialKn / demandMag, demand.MomentYKnm / demandMag, demand.MomentZKnm / demandMag)
            : (0.0, 0.0, 0.0);
        // The pierce parameter t of the origin-cast unit demand ray through each tri-face; over a closed convex hull
        // exactly one face yields a positive front-facing t — the capacity-boundary magnitude along the load direction.
        double boundaryMag = toSeq(toSeq(hull.Faces)
            .Map(f => Pierce(f, dirN, dirMy, dirMz))
            .Somes()
            .Filter(static t => t > 0.0)
            .OrderBy(static t => t))
            .Head
            .IfNone(double.Epsilon);
        double ratio = demandMag > 0.0 && boundaryMag > double.Epsilon ? demandMag / boundaryMag : double.PositiveInfinity;
        string governing = Math.Abs(demand.AxialKn) >= Math.Max(Math.Abs(demand.MomentYKnm), Math.Abs(demand.MomentZKnm)) ? "axial" : "biaxial-moment";
        return Utilisation.Of(ratio, governing);
    }

    // Möller–Trumbore: the parametric distance t at which the origin ray (direction d) pierces triangle A-B-C, None
    // when the ray is parallel to the facet or the barycentric hit lands outside the triangle.
    static Option<double> Pierce(IForceMomentTriFace face, double dN, double dMy, double dMz) {
        (double ax, double ay, double az) = Coord(face.A);
        (double e1x, double e1y, double e1z) = Sub(Coord(face.B), (ax, ay, az));
        (double e2x, double e2y, double e2z) = Sub(Coord(face.C), (ax, ay, az));
        (double px, double py, double pz) = Cross((dN, dMy, dMz), (e2x, e2y, e2z));
        double det = e1x * px + e1y * py + e1z * pz;
        if (Math.Abs(det) < 1e-12) return None;
        double inv = 1.0 / det;
        double u = -(ax * px + ay * py + az * pz) * inv;          // origin - A = -A
        if (u < 0.0 || u > 1.0) return None;
        (double qx, double qy, double qz) = Cross((-ax, -ay, -az), (e1x, e1y, e1z));
        double v = (dN * qx + dMy * qy + dMz * qz) * inv;
        if (v < 0.0 || u + v > 1.0) return None;
        return (e2x * qx + e2y * qy + e2z * qz) * inv;            // t along the unit ray
    }

    static (double, double, double) Coord(IForceMomentVertex v) => (v.X.Kilonewtons, v.Y.KilonewtonMeters, v.Z.KilonewtonMeters);
    static (double, double, double) Sub((double x, double y, double z) a, (double x, double y, double z) b) => (a.x - b.x, a.y - b.y, a.z - b.z);
    static (double, double, double) Cross((double x, double y, double z) a, (double x, double y, double z) b) =>
        (a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
}
```

## [03]-[RESEARCH]

- [RC_INTERACTION_HULL]: REALIZED — the `RcInteraction` capacity case is the ultimate biaxial Force-Moment-Moment capacity hull `VividOrange.InteractionDiagram` computes over the `Connection/reinforcement#RC_SECTION` `IConcreteSection`: `SectionCapacityResolver.Resolve` constructs `new InteractionDiagram(rc.Section, resolution.ToSettings())` (the eager solve that `Triangle`-meshes the concrete+rebar section into `AnalyticalFace` fibres, runs the `Parallel.For` strain-plane sweep integrating fibre stress, and `MIConvexHull`-welds the (N, My, Mz) cloud into the closed onion, `.api/api-vividorange-interactiondiagram.md` `[FIBRE_INTEGRATION_CONTRACT]`) and reads `diagram.Mesh` as the cached `IForceMomentMesh`. The `Triangle` mesher and `MIConvexHull` hull builder are encapsulated `internal` inside the engine — this AEC-DOMAIN owner composes the welded hull through the constructor, mints NO direct `Triangle`/`MIConvexHull` call (the strata-correct seam: those primitives are `Rasm`-kernel-owned, consumed transitively, `.api/api-triangle.md` / `.api/api-miconvexhull.md` `[STACKING_LAW]`). The hull is read through the `IForceMomentMesh` interface floor, the `Force`/`Torque` coordinates as `UnitsNet` quantities, never the `ForceMomentMesh` concrete and never the facet `Area` `Ratio` as a physical quantity. The eager solve caches on the `RcInteraction` carrier so a `Check(demand)` reads the cached hull, never re-solving. Ripple counterpart: `Connection/reinforcement` `[RC_SECTION]` (the `RcSection`/`IConcreteSection` input this owner consumes).
- [RC_ELASTIC_TRANSFORMED]: REALIZED — the `RcElastic` capacity case is the elastic transformed-section reinforcement properties `VividOrange.Sections.SectionProperties` `ConcreteSectionProperties` computes over the same `IConcreteSection`: `new ConcreteSectionProperties(rc.Section)` reads `TotalReinforcementArea`/`ConcreteArea`/`GeometricReinforcementRatio`/`ReinforcementSecondMomentOfAreaYy`/`Zz` as `UnitsNet` quantities coerced to SI scalars at the edge (`.api/api-vividorange-sections-sectionproperties.md` `[RC seam]`). One `IConcreteSection` minted at `Connection/reinforcement#RC_SECTION` drives BOTH the elastic transformed-section properties here and the ultimate N-M-M hull (`.api/api-vividorange-sections-sectionproperties.md` `[01]-[RC_COMPOSITION_PATH]`), so the cracked/uncracked elastic check and the ultimate capacity envelope share one section input — the elastic `RcElastic` is the transformed-section complement the bare `profile#PROFILE_OWNER` `ParametricSection` (gross elastic over any `IProfile`) does not compute, this owner the reinforced-section transformed properties.
- [STEEL_LRFD_LIFT]: REALIZED — the `SteelLrfd` capacity case lifts the `steel#STEEL_FAMILY` `DesignCapacity` (`φMn`/`φPn`/`φVn`, the AISC 360 Chapters F/E/G rolled-steel LRFD already derived over the computed `SteelSection` columns) into the one `SectionCapacity` rail, so a steel column and an RC column are checked through the SAME `Check(demand)` fold — never a parallel `SteelBeamCheck` surface. The lift is a projection of the realized steel receipt (`FlexuralNmm`/`CompressionN`/`ShearN` → SI kN/kNm), the steel capacity computation staying the `steel#STEEL_FAMILY` owner, this page only the unified utilisation rail. The `SteelLrfd` interaction is the AISC `max(N/φPn, My/φMn)`; the full AISC H1.1 combined-axial-bending interaction (the `8/9` factor split) is one `Check` arm refinement, the realized check the governing-axis ratio.
- [UTILISATION_RAY_CAST]: REALIZED — the `RcInteraction` utilisation is the `CapacityRay.Cast` of the applied demand `(N, My, Mz)` vector against the `IForceMomentMesh` hull: the demand-magnitude / pierce-magnitude ratio along the load ray, the boundary the exact Möller–Trumbore intersection of the origin-cast unit demand ray through the `IForceMomentTriFace.A`/`B`/`C` facets (the `IForceMomentVertex.X`/`Y`/`Z` `Force`/`Torque` coordinates read through the floor), over the closed convex origin-interior onion the engine welds. Each face yields an `Option<double>` pierce parameter `t` (`None` for a parallel or barycentrically-missed facet); the positive front face's `t` is the capacity-boundary magnitude, so a ratio ≤ 1 is adequate — a finer `DiagramResolution.Fine` (48 steps) refines the discretised hull the same fold casts against, never a second algorithm. The `Utilisation` verdict crosses to `cs:AEC_SIMULATION_BRIDGE` as portable scalar data keyed by section, never a `VividOrange` type crossing the seam.
