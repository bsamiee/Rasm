# [MATERIALS_CAPACITY]

THE SECTION-CAPACITY OWNER and THE ONE UTILISATION RAIL. One `SectionCapacity` `[Union]` is the closed structural-capacity surface a `Profiles` cross-section carries beyond its elastic `ComputedSection`, and one `Demand` folded against it through `Check` is the typed `Utilisation` verdict — so EVERY family's design check is one polymorphic fold differing only in the capacity case, never a per-family `RcColumnCheck`/`SteelBeamCheck`/`MasonryWallCheck` surface. The closed case set spans the four realized `ProfileFamily` structural rails: `RcInteraction` (the ultimate biaxial Force-Moment-Moment capacity hull `VividOrange.InteractionDiagram` welds over the `Connection/reinforcement#RC_SECTION` `IConcreteSection`), `RcElastic` (the elastic transformed-section reinforcement properties `VividOrange.Sections.SectionProperties` `ConcreteSectionProperties` computes over the same section), `SteelLrfd` (the AISC 360 `steel#STEEL_FAMILY` `DesignCapacity` `φMn`/`φPn`/`φVn` + `CompactnessClass`/slenderness lifted whole), `TimberEc5` (the EN 1995-1-1 `timber#TIMBER_FAMILY` `TimberCapacity` design-resistance receipt lifted whole), and `MasonryCompression` (the TMS 402 axial-flexural unity check the `cmu#CMU_FAMILY` `CmuStrength` `f'm` + grouted `ComputedSection` feed). A capacity is admitted to the family ONLY when no existing case's column set carries it: each sibling family page that hand-rolls its design rules (`steel#STEEL_FAMILY`, `timber#TIMBER_FAMILY`, `cmu#CMU_FAMILY`) lifts its already-computed receipt into ONE case here, and the RC cases are the two `Resolve` builds over the section input — the design-code COMPUTATION stays the family owner's, the unified VERDICT this owner's. This owner is the ULTIMATE complement to `profile#PROFILE_OWNER` `ParametricSection`: that solver gives the elastic `Area`/`MomentOfInertia` every family computes from its perimeter, THIS owner gives the reinforced-section transformed properties, the ultimate capacity hull, and the unified utilisation fold the elastic solver does not. The `InteractionDiagram` constructor RUNS the full eager fibre-integration solve at construction (the `Triangle` section mesh, the `Parallel.For` strain-plane sweep, the `MIConvexHull` hull weld are encapsulated `internal` — this owner composes the welded `IForceMomentMesh`, never the meshing primitive), so a design page constructs the capacity ONCE per section/settings and reads `diagram.Mesh` cached, never re-solving per query. The page composes `Connection/reinforcement#RC_SECTION` `RcSection`/`IConcreteSection` for the RC input, `VividOrange.InteractionDiagram` (`InteractionDiagram`/`DiagramSettings`/`IForceMomentMesh`) for the N-M-M hull, `VividOrange.Sections.SectionProperties` `ConcreteSectionProperties` for the elastic transformed-section properties, `VividOrange.Materials` `EnConcreteFactory` for the EC2 `fck` the cracking reference reads, the `steel#STEEL_FAMILY` `DesignCapacity` / `timber#TIMBER_FAMILY` `TimberCapacity` / `cmu#CMU_FAMILY` `CmuStrength` sibling receipts, the in-folder `UnitsNet` `Force`/`Torque`/`Area`/`Length` quantity coercion at the edge, and the `profile#PROFILE_OWNER` `ProfileFault` band-2300 rail (the SAME profile-sub-domain fault every sibling Profiles page rails — NOT a borrowed appearance band) for a non-finite, degenerate, or infeasible solve; the capacity surface and the utilisation verdict feed the forward `cs:AEC_SIMULATION_BRIDGE` analysis consumer by `MaterialId`/section key, host-neutral here, the `IForceMomentMesh` round-tripping through `VividOrange.Serialization` for the C#-internal cache.

## [01]-[INDEX]

- [01]-[SECTION_CAPACITY]: the `SectionCapacity` `[Union]` (`RcInteraction` N-M-M hull · `RcElastic` transformed-section · `SteelLrfd` rolled-steel · `TimberEc5` EC5 receipt · `MasonryCompression` TMS 402) over the `profile#PROFILE_OWNER` `ProfileFault` band-2300 rail, the `DiagramResolution` `[SmartEnum]` mesh/sweep-refinement policy folding to a `DiagramSettings`, the `Demand` applied-action shape (axial · biaxial moment · biaxial shear · torsion), the `GoverningAction` `[SmartEnum]` verdict axis, the `Utilisation` typed verdict, and the `SectionCapacity.Resolve` eager-solve boundary plus the `SteelLrfd`/`TimberEc5`/`MasonryCompression` sibling-receipt lifts.

## [02]-[SECTION_CAPACITY]

- Owner: `SectionCapacity` `[Union]` the closed five-case capacity-surface family; `Demand` the applied (N, My, Mz, Vy, Vz, Mt) action shape; `Utilisation` the typed demand-vs-capacity verdict over the `GoverningAction` `[SmartEnum]` axis; `DiagramResolution` the `[SmartEnum]` sweep/mesh-refinement policy folding to a `VividOrange.InteractionDiagram` `DiagramSettings`; the `profile#PROFILE_OWNER` `ProfileFault` band-2300 rail (composed, not re-declared); `SectionCapacity.Resolve` the eager-solve boundary plus the `SteelLrfd`/`TimberEc5`/`MasonryCompression` lift factories.
- Cases: `RcInteraction` (the ultimate biaxial N-M-M capacity hull as the `IForceMomentMesh` over an `IConcreteSection`, `VividOrange.InteractionDiagram`) · `RcElastic` (the elastic transformed-section reinforcement properties — `TotalReinforcementArea`/`ConcreteArea`/`GeometricReinforcementRatio`/`ReinforcementSecondMomentOfAreaYy`/`Zz` + the bottom-face `EffectiveDepth(SectionFace)` ULS lever + the gross depth AND width (the major/minor-axis SLS extreme-fibre levers) + the EC2 `fctm` cracking limit, the combined `N/A ± My·cy/Iyy ± Mz·cz/Izz` SLS check, `VividOrange.Sections.SectionProperties` `ConcreteSectionProperties`) · `SteelLrfd` (the rolled/composite/cold-formed `steel#STEEL_FAMILY` `DesignCapacity` `φMn`/`φPn`/`φVn` + `CompactnessClass` + slenderness lifted WHOLE) · `TimberEc5` (the EN 1995-1-1 `timber#TIMBER_FAMILY` `TimberCapacity` `M_Rd`/`N_Rd`/`V_Rd`/`R_90,Rd` + `λ_rel` + `k_mod` lifted WHOLE) · `MasonryCompression` (the TMS 402 axial-flexural check the `cmu#CMU_FAMILY` `CmuStrength` `f'm` + grouted `ComputedSection` net area/modulus + slenderness reduction feed) — the closed structural-capacity family across steel/RC/timber/masonry; a capacity is a `SectionCapacity` case over a section, never a per-section-type check.
- Entry: `public static Fin<SectionCapacity> Resolve(RcSection rc, CapacityKind kind, DiagramResolution resolution, Op key)` — the ONE RC capacity boundary: it discriminates the RC-build `kind` onto the matching solver over the `RcSection.Section` `IConcreteSection`, the `RcInteraction` case constructing `new InteractionDiagram(rc.Section, resolution.ToSettings())` (the eager fibre-integration solve, trapping a degenerate/non-EN-grade solve onto `ProfileFault.Capacity`) and reading `diagram.Mesh` as the cached `IForceMomentMesh`, the `RcElastic` case constructing `new ConcreteSectionProperties(rc.Section)` and reading the transformed properties + the `EffectiveDepth(SectionFace.Bottom)` ULS flexural lever (a carried concept member, distinct from the SLS extreme-fibre distance the check uses) + the gross depth/width (the major/minor-axis fibre levers) + the `EnConcreteFactory`-parsed `fck`, each `UnitsNet` quantity coerced to its SI base once at the edge; the three sibling-receipt lifts `public static SectionCapacity SteelLrfd(DesignCapacity capacity)` / `TimberEc5(TimberCapacity capacity)` / `MasonryCompression(double FmMpa, ComputedSection section, double slendernessReduction)` carry the already-computed `steel`/`timber`/`cmu` receipts into the rail with no re-derivation (the AISC §H3.1 `DesignCapacity.TorsionalNmm` / EN 1995-1-1 §6.1.8 `TimberCapacity.TorsionalNmm` design torsional resistance read DIRECTLY off the receipt onto `TorsionalKnm` — one source, no redundant parallel lift parameter); `public Utilisation Check(Demand demand)` folds the applied action against the capacity — for `RcInteraction` the Möller–Trumbore ray-cast of the demand vector against the `IForceMomentMesh` hull faces (the demand-magnitude / capacity-boundary-magnitude ratio along the load ray), for `RcElastic` the EC2 SLS COMBINED extreme-fibre transformed stress `σ = N/A ± My·cy/Iyy ± Mz·cz/Izz` against `fctm` (signed axial, BOTH bending axes — never a major-axis-only slice), for `SteelLrfd` the AISC `max(N/φPn, M/φMn, V/φVn, T/φTn)` interaction reporting the governing `CompactnessClass`, for `TimberEc5` the EN 1995 `max(M_Ed/M_Rd, N_Ed/N_Rd, V_Ed/V_Rd, T_Ed/T_Rd)`, for `MasonryCompression` the TMS 402 `max(P/φPn, M/φMn)` with the slenderness reduction — one polymorphic `Check`, never a `CheckRcColumn`/`CheckSteelBeam` family.
- Packages: VividOrange.InteractionDiagram (`InteractionDiagram`/`DiagramSettings`, the eager-solve ctor + `Mesh`; `.api/api-vividorange-interactiondiagram.md`), VividOrange.IForceMomentInteraction (`IForceMomentMesh`/`IForceMomentVertex`/`IForceMomentTriFace` the hull read through, the `Faces`/`A`/`B`/`C`/`X`/`Y`/`Z` `Force`/`Torque` members; `.api/api-vividorange-iforcemomentinteraction.md`), VividOrange.Sections.SectionProperties (`ConcreteSectionProperties` the transformed-section solver + the `EffectiveDepth(SectionFace)`/`ReinforcementArea(SectionFace)` queries; `.api/api-vividorange-sections-sectionproperties.md`), VividOrange.Sections (`IConcreteSection`/`SectionFace` from the `Connection/reinforcement#RC_SECTION` `RcSection`; `.api/api-vividorange-sections.md`), VividOrange.Materials (`EnConcreteFactory.CreateLinearElastic` whose `LinearElasticMaterial.Strength` IS the parsed `fck` the EC2 `fctm` cracking reference reads; `.api/api-vividorange-materials.md`), UnitsNet (`Force.Kilonewtons`/`Torque.KilonewtonMeters`/`Area`/`Length`/`Ratio` coerced at the edge; `.api/api-unitsnet.md`), Rasm.Element (project — `MaterialId`/`ProfileRef` the seam-carried identity), Rasm (project — `PositiveMagnitude`, `Op`/`Context`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Fold`), Thinktecture.Runtime.Extensions (`[Union]` for `SectionCapacity`, `[SmartEnum]` for `DiagramResolution`/`CapacityKind`/`GoverningAction`). Triangle + MIConvexHull ride transitively INSIDE the `InteractionDiagram` engine (encapsulated `internal`, `.api/api-triangle.md` / `.api/api-miconvexhull.md`) — this owner mints NO direct mesher/hull call, composing only the welded `IForceMomentMesh`. The `steel#STEEL_FAMILY` `DesignCapacity`, `timber#TIMBER_FAMILY` `TimberCapacity`, and `cmu#CMU_FAMILY` `CmuStrength` are sibling-page receipts lifted, never re-computed.
- Growth: a new structural family's capacity is one `SectionCapacity` `[Union]` case binding either a `Resolve` build (a section-input solve) or a lift factory (an already-computed sibling receipt) plus one `Check` arm — a moment-curvature `RcInteraction` refinement, a glazing structural-glass check, a connection-design check — admitted only when no existing case's column set carries it; a new demand axis is one `Demand` column (a warping bimoment, a second-order P-Δ amplifier); a new utilisation metric one `Utilisation`/`GoverningAction` projection — never a per-section-type capacity surface, never a re-derived elastic property where `ConcreteSectionProperties` computes it, never a direct `Triangle`/`MIConvexHull` call where the `InteractionDiagram` engine welds the hull; the `steel`/`timber`/`cmu` design receipts stay the family-owner derivation lifted here, never re-computed.
- Boundary: `SectionCapacity.Resolve` is the BOUNDARY_ADMISSION point where the `VividOrange.InteractionDiagram`/`Sections.SectionProperties` surface is admitted EXACTLY ONCE — the `InteractionDiagram` ctor runs the expensive eager solve (`.api/api-vividorange-interactiondiagram.md` `[construction law]`) and a non-EN material whose `IEnConcreteMaterial`/`IEnRebarMaterial` cast the engine cannot read, an under-reinforced degenerate section, or a hull-weld failure rails `ProfileFault.Capacity` (the profile-sub-domain band 2300, the dedicated capacity-solve slot distinct from the `Section` elastic-integral slot `steel#STEEL_FAMILY` `SectionReader.Read` rails — both band 2300 with their Profiles siblings, NOT the `Appearance/bsdf#SHADING_FRAME` appearance band 2450) rather than throwing, so no `VividOrange` throw and no infeasible hull reaches an interior signature; the `IForceMomentMesh` is read THROUGH its interface floor (`.api/api-vividorange-iforcemomentinteraction.md` `[LOCAL_ADMISSION]`), never the `ForceMomentMesh` concrete, and the `Force`/`Torque` hull coordinates carry as `UnitsNet` quantities coerced to SI base (`Force.Kilonewtons`/`Torque.KilonewtonMeters`) once at the edge so no interior signature carries the hull as raw `double`; the `Triangle` section mesher and the `MIConvexHull` hull builder are encapsulated `internal` inside the engine (`.api/api-triangle.md` `[STACKING_LAW]` / `.api/api-miconvexhull.md` `[STACKING_LAW]`) — this AEC-DOMAIN owner mints NO direct mesher/hull call, composing the welded hull through the constructor, the strata-correct seam (the computational-geometry primitives are `Rasm`-kernel-owned, consumed transitively here); the eager solve is cached on the `SectionCapacity` `RcInteraction` carrier (`.api/api-vividorange-interactiondiagram.md` `[LOCAL_ADMISSION]` — construct once per section/settings, never re-solve per query), so a `Check(demand)` reads the cached hull; the `RcInteraction` utilisation is the exact Möller–Trumbore intersection of the origin-cast demand ray against the hull faces (the `IForceMomentTriFace.A`/`B`/`C` the demand vector pierces, the positive front-face pierce `t` the capacity boundary along the load direction), the no-pierce case (an eccentric hull that does not enclose the origin) yielding a typed over-capacity verdict rather than a silent `+∞`, NEVER the facet `Area` `Ratio` read as a physical quantity (`.api/api-vividorange-iforcemomentinteraction.md` `[AXIS_SEMANTICS]`); the `Utilisation.Governing` is the typed `GoverningAction` `[SmartEnum]` (axial · flexure · biaxial-moment · shear · bending · torsion), NEVER a stringly-typed verdict; the capacity surface is host-neutral — the `IForceMomentMesh` round-trips through `VividOrange.Serialization` for the C#-internal cache (`.api/api-vividorange-serialization.md`, distinct from the canonical Thinktecture wire) and the utilisation verdict crosses to `cs:AEC_SIMULATION_BRIDGE` as portable scalar data keyed by section, never a `VividOrange` assembly type crossing the boundary.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;                                   // Op, PositiveMagnitude (the kernel admission key + magnitude)
using Rasm.Element;                                  // MaterialId, ProfileRef (the seam-carried identity)
using Rasm.Materials.Profiles;                       // ComputedSection, ProfileFault (the parent PROFILE_OWNER section receipt + band-2300 rail)
using Rasm.Materials.Profiles.Steel;                 // DesignCapacity, SteelClass, CompactnessClass (the steel#STEEL_FAMILY LRFD receipt lifted WHOLE as SteelLrfd)
using Rasm.Materials.Profiles.Timber;                // TimberCapacity (the timber#TIMBER_FAMILY EC5 receipt lifted WHOLE as TimberEc5)
using Rasm.Materials.Profiles.Cmu;                   // CmuStrength (the cmu#CMU_FAMILY f'm the MasonryCompression case feeds)
using Rasm.Materials.Connection.Reinforcement;       // RcSection (the Connection/reinforcement#RC_SECTION IConcreteSection input the RC cases solve over)
using Thinktecture;
using VividOrange.ForceMomentInteraction;            // IForceMomentMesh, IForceMomentVertex, IForceMomentTriFace, the Faces/A/B/C/X/Y/Z floor
using ForceMomentEngine = VividOrange.ForceMomentInteraction.InteractionDiagram;  // the eager-solve engine (alias frees the bare name for the SectionCapacity owner)
using VividOrange.Sections.SectionProperties;        // ConcreteSectionProperties (the RC transformed-section solver)
using VividOrange.Sections;                          // IConcreteSection, SectionFace (the RcSection input + the effective-depth face)
using VividOrange.Materials.StandardMaterials.En;    // EnConcreteFactory (the LinearElasticMaterial.Strength == parsed fck the EC2 fctm reads)
using UnitsNet;                                      // Force, Torque, Area, Length, Ratio (coerced at the edge)
using static LanguageExt.Prelude;                    // toSeq, Some, None, Optional

// The capacity owner is its OWN Rasm.Materials.Profiles.Capacity sub-namespace (the SAME per-page sub-namespace law the
// family pages follow); it composes the parent PROFILE_OWNER (ComputedSection/ProfileFault), the three structural family
// sub-namespaces it lifts receipts from (Steel/Timber/Cmu), and the Connection.Reinforcement RC section — every
// cross-page type imported by its sub-namespace-qualified using above, never the prior ambient flat namespace.
namespace Rasm.Materials.Profiles.Capacity;

// --- [TYPES] -------------------------------------------------------------------------------
// The RC-build dispatch for SectionCapacity.Resolve — the TWO capacity surfaces built FROM an RcSection input (the
// N-M-M hull and the elastic transformed section). The steel/timber/masonry cases are LIFTS of already-computed
// sibling receipts (SteelLrfd/TimberEc5/MasonryCompression factories), not Resolve builds, so this kind is RC-scoped
// by design — it does NOT mirror the full SectionCapacity case set (that would be a redundant parallel discriminant).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CapacityKind {
    public static readonly CapacityKind RcInteraction = new("rc-interaction");   // ultimate N-M-M hull
    public static readonly CapacityKind RcElastic     = new("rc-elastic");       // elastic transformed-section SLS
}

// The InteractionDiagram mesh/sweep-refinement policy folded to a VividOrange.InteractionDiagram DiagramSettings:
// the Steps knob drives a Steps² strain-plane sweep (quadratic cost), so the band trades hull fidelity for solve cost
// rather than scattering a DiagramSettings ctor at the call site (.api/api-vividorange-interactiondiagram.md [default law]).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DiagramResolution {
    public static readonly DiagramResolution Draft    = new("draft",    steps: 16, concreteMaxAreaMm2: 500.0, rebarDivisions: 12);
    public static readonly DiagramResolution Standard = new("standard", steps: 30, concreteMaxAreaMm2: 250.0, rebarDivisions: 16);
    public static readonly DiagramResolution Fine     = new("fine",     steps: 48, concreteMaxAreaMm2: 120.0, rebarDivisions: 24);
    public int Steps { get; }
    public double ConcreteMaxAreaMm2 { get; }
    public int RebarDivisions { get; }

    // The rebar mesh uses 0.8× the concrete max face area + the same 25° minimum-angle quality constraint, matching the
    // DiagramSettings default ratio (250 mm² concrete / 200 mm² rebar) the engine ships (.api [default law]).
    public DiagramSettings ToSettings() =>
        new(Area.FromSquareMillimeters(ConcreteMaxAreaMm2), Angle.FromDegrees(25.0),
            Area.FromSquareMillimeters(ConcreteMaxAreaMm2 * 0.8), Angle.FromDegrees(25.0), RebarDivisions, Steps);
}

// The verdict axis — which applied action governs the check, a typed bounded vocabulary NEVER a stringly-typed label.
// One closed set across every family: axial/flexure for a uniaxial governing component, biaxial-moment for the RC hull
// ray, shear for a shear-governed check, bending for a flexure-governed RcElastic/TimberEc5 arm, torsion for the
// St-Venant torsional-shear demand the steel/timber arms fold against their torsional resistance.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GoverningAction {
    public static readonly GoverningAction Axial         = new("axial");
    public static readonly GoverningAction Flexure       = new("flexure");
    public static readonly GoverningAction BiaxialMoment = new("biaxial-moment");
    public static readonly GoverningAction Shear         = new("shear");
    public static readonly GoverningAction Bending       = new("bending");
    public static readonly GoverningAction Torsion       = new("torsion");
}

// --- [MODELS] ------------------------------------------------------------------------------
// The applied design action checked against the capacity surface — the full member-action vector in SI engineering
// units (kN, kNm), signed (axial − compression / + tension, moments ± for direction), so it is raw double NOT
// PositiveMagnitude (a value that is genuinely signed cannot be a positive-magnitude). N/My/Mz are the RcInteraction
// hull-ray vector, the RcElastic combined-stress demand, and the flexure/axial demands; Vy/Vz the shear demands the
// SteelLrfd φVn and TimberEc5 V_Rd check fold; Mt the torsion the SteelLrfd §H3.1 / TimberEc5 §6.1.8 torsion arm folds
// against the lifted torsional resistance. The biaxial moment magnitude and the shear resultant are derived
// projections, never re-passed columns.
public readonly record struct Demand(
    double AxialKn,
    double MomentYKnm,
    double MomentZKnm,
    double ShearYKn = 0.0,
    double ShearZKn = 0.0,
    double TorsionKnm = 0.0) {
    public double NmmMagnitude => Math.Sqrt(AxialKn * AxialKn + MomentYKnm * MomentYKnm + MomentZKnm * MomentZKnm);
    public double MomentResultantKnm => Math.Sqrt(MomentYKnm * MomentYKnm + MomentZKnm * MomentZKnm);
    public double ShearResultantKn => Math.Sqrt(ShearYKn * ShearYKn + ShearZKn * ShearZKn);
}

// The typed utilisation verdict — the demand/capacity ratio plus the typed governing action and the pass flag, never a
// bare double and never a stringly-typed axis; Ratio > 1 is over-capacity, Adequate the Ratio <= 1 gate. Over a hull
// that does not enclose the origin (the demand ray never pierces a front face) the verdict is Overcapacity — a typed
// "outside the capacity surface" rather than a silent +∞ a divide-by-epsilon would produce.
public readonly record struct Utilisation(double Ratio, GoverningAction Governing, bool Adequate) {
    public static Utilisation Of(double ratio, GoverningAction governing) =>
        new(ratio, governing, double.IsFinite(ratio) && ratio <= 1.0);
    public static Utilisation Overcapacity(GoverningAction governing) =>
        new(double.PositiveInfinity, governing, Adequate: false);
}

// One SectionCapacity [Union] closes the structural-capacity family across the four realized structural ProfileFamily
// rails — the ultimate N-M-M hull, the elastic transformed RC section, the rolled/composite/cold-formed steel LRFD
// receipt, the EC5 timber design receipt, and the TMS 402 masonry compression check — so a member is checked through
// one Check fold, never a per-type surface. The non-RC cases lift their family-owner receipts WHOLE (the design-code
// computation stays the sibling page's, the unified verdict this owner's); the RC cases are the Resolve builds.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionCapacity {
    private SectionCapacity() { }

    // The cached ultimate biaxial capacity hull — the IForceMomentMesh held once from the eager InteractionDiagram solve.
    public sealed record RcInteraction(IForceMomentMesh Hull) : SectionCapacity;
    // The elastic transformed-section reinforcement properties (SI scalars from ConcreteSectionProperties) plus the
    // bottom-face EFFECTIVE DEPTH d (the ULS flexural lever to the tension steel — the ConcreteSectionProperties concept
    // member the bridge consumer reads for a moment-capacity readout, distinct from the SLS extreme-fibre distance), the
    // gross depth (the SLS extreme-CONCRETE-fibre lever c ≈ h/2), and the EC2 concrete flexural tensile limit fctm so the
    // elastic SLS cracking check is total.
    public sealed record RcElastic(
        double TotalReinforcementAreaMm2,
        double ConcreteAreaMm2,
        double ReinforcementRatio,
        double InertiaYyMm4,
        double InertiaZzMm4,
        double EffectiveDepthMm,
        double DepthMm,
        double WidthMm,
        double FctmMpa) : SectionCapacity;
    // The steel LRFD receipt lifted WHOLE from steel#STEEL_FAMILY DesignCapacity (the SI N·mm/N capacities carried as
    // kN·m/kN here, plus the AISC Table B4.1 CompactnessClass and the column slenderness λ) — never re-derived here.
    // TorsionalKnm is the AISC 360 §H3.1 design torsional resistance φTn = φT·Fcr·C (C the HSS torsional constant J/c the
    // steel owner derives) the steel DesignCapacity.TorsionalNmm column carries — positive for a CLOSED HSS/pipe, 0 for an
    // OPEN thin-walled shape whose §H3.3 warping torsion is not a single resistance, so an open-shape torsion demand
    // surfaces as the governing over-ratio (the consumed-action discipline), never a silently-ignored 0 column.
    public sealed record SteelLrfd(
        double FlexuralKnm,
        double CompressionKn,
        double ShearKn,
        double TorsionalKnm,
        CompactnessClass Classification,
        double Slenderness) : SectionCapacity;
    // The EN 1995-1-1 timber design receipt lifted WHOLE from timber#TIMBER_FAMILY TimberCapacity (the M_Rd/N_Rd/V_Rd/
    // R_90,Rd design resistances + the relative slenderness λ_rel + the k_mod service×duration factor) — never re-derived.
    // TorsionalKnm is the EN 1995-1-1 §6.1.8 torsional resistance T_Rd = k_shape·f_v,d·W_tor the timber owner derives over
    // the rectangular section (the TimberCapacity.TorsionalNmm column) — positive for every realized timber section, so a
    // torsion-loaded glulam member folds demand.TorsionKnm against a real resistance, never an inert 0.
    public sealed record TimberEc5(
        double BendingKnm,
        double CompressionKn,
        double ShearKn,
        double BearingPerpKn,
        double TorsionalKnm,
        double RelativeSlenderness,
        double Kmod) : SectionCapacity;
    // The TMS 402 masonry compression case: the cmu#CMU_FAMILY CmuStrength specified strength f'm + the (grouted) net
    // ComputedSection (net area / section modulus from ParametricSection.Hollow) + the slenderness reduction (the
    // TMS 402 §9.3.4.1.1 [1 - (h/140r)²] member-stability factor) the axial-flexural unity check scales.
    public sealed record MasonryCompression(
        double FmMpa,
        double NetAreaMm2,
        double SectionModulusMm3,
        double SlendernessReduction) : SectionCapacity;

    // The demand-vs-capacity verdict, one polymorphic Check over the closed family — never per-type. The RcInteraction
    // arm ray-casts the demand against the hull; the RcElastic arm the EC2 SLS combined extreme-CONCRETE-fibre stress
    // σ = N/A ± My·cy/Iyy ± Mz·cz/Izz against fctm; the SteelLrfd arm the AISC max(N/φPn, M/φMn, V/φVn, T/φTn)
    // interaction; the TimberEc5 arm the EN 1995 max over the design resistances; the MasonryCompression arm TMS 402.
    public Utilisation Check(Demand demand) => Switch(
        rcInteraction: h => CapacityRay.Cast(h.Hull, demand),
        rcElastic: e => RcCrackingUtilisation(e, demand),
        steelLrfd: s => SteelUtilisation(s, demand),
        timberEc5: t => TimberUtilisation(t, demand),
        masonryCompression: m => MasonryUtilisation(m, demand));

    // EC2 SLS cracking: the MAXIMUM-tensile extreme-CONCRETE-fibre transformed stress against fctm — the FULL combined
    // action σ = N/A ± My·cy/Iyy ± Mz·cz/Izz, never the major-axis-bending-only slice. A SIGNED axial N/A (the Demand
    // axial convention: − compression, + tension) so a compressive service axial DELAYS cracking (the physically-correct
    // SLS behaviour, not a |N| that would over-predict cracking under compression); BOTH bending axes add their
    // tension-side fibre stress (|My|·cy/Iyy + |Mz|·cz/Izz with cy = h/2, cz = b/2 the gross half-depths of the
    // symmetric uncracked transformed section — NOT the effective depth d, the ULS lever to the tension STEEL the record
    // carries for the bridge readout). σ_max/fctm > 1 ⇒ the section cracks; the governing axis the larger bending
    // contribution (or Axial when N/A dominates a near-zero-moment service state). This consumes ConcreteAreaMm2 (N/A)
    // and InertiaZzMm4 (the minor-axis fibre stress) the prior major-axis-only slice carried dead.
    static Utilisation RcCrackingUtilisation(RcElastic e, Demand demand) {
        double axialStress = demand.AxialKn * 1e3 / Math.Max(e.ConcreteAreaMm2, double.Epsilon);                  // signed N/A (MPa)
        double bendingYStress = Math.Abs(demand.MomentYKnm) * 1e6 * (e.DepthMm * 0.5) / Math.Max(e.InertiaYyMm4, double.Epsilon);
        double bendingZStress = Math.Abs(demand.MomentZKnm) * 1e6 * (e.WidthMm * 0.5) / Math.Max(e.InertiaZzMm4, double.Epsilon);
        double tensileStress = axialStress + bendingYStress + bendingZStress;                                     // max tensile fibre (MPa)
        GoverningAction governing = bendingYStress >= bendingZStress && bendingYStress >= Math.Abs(axialStress)
            ? GoverningAction.Bending
            : bendingZStress >= Math.Abs(axialStress) ? GoverningAction.BiaxialMoment : GoverningAction.Axial;
        return Utilisation.Of(tensileStress / Math.Max(e.FctmMpa, double.Epsilon), governing);
    }

    // AISC 360 combined-action: the worst of axial, flexural-resultant, shear-resultant, AND §H3.1 torsional ratios; the
    // governing axis the typed GoverningAction the larger ratio names. The CompactnessClass rides the carrier for the
    // design report. The torsion ratio folds demand.TorsionKnm against the §H3.1 φTn the lift carries (0.0 ⇒ a
    // zero-torsion demand stays 0, a nonzero torsion demand against an unbounded φTn surfaces as the governing over-ratio).
    static Utilisation SteelUtilisation(SteelLrfd s, Demand demand) {
        double axial = Math.Abs(demand.AxialKn) / Math.Max(s.CompressionKn, double.Epsilon);
        double flexure = demand.MomentResultantKnm / Math.Max(s.FlexuralKnm, double.Epsilon);
        double shear = demand.ShearResultantKn / Math.Max(s.ShearKn, double.Epsilon);
        double torsion = TorsionRatio(demand.TorsionKnm, s.TorsionalKnm);
        return Worst((axial, GoverningAction.Axial), (flexure, GoverningAction.Flexure), (shear, GoverningAction.Shear), (torsion, GoverningAction.Torsion));
    }

    static Utilisation TimberUtilisation(TimberEc5 t, Demand demand) {
        double bending = demand.MomentResultantKnm / Math.Max(t.BendingKnm, double.Epsilon);
        double axial = Math.Abs(demand.AxialKn) / Math.Max(t.CompressionKn, double.Epsilon);
        double shear = demand.ShearResultantKn / Math.Max(t.ShearKn, double.Epsilon);
        double torsion = TorsionRatio(demand.TorsionKnm, t.TorsionalKnm);
        return Worst((bending, GoverningAction.Bending), (axial, GoverningAction.Axial), (shear, GoverningAction.Shear), (torsion, GoverningAction.Torsion));
    }

    // The torsional-resistance ratio: a zero torsion demand is trivially 0 (so an unbounded φTn = 0 never spuriously
    // governs a member with no applied torsion), a nonzero torsion demand divides by the φTn the lift carries — so a
    // torsion-loaded member whose owner has not yet bounded its torsional resistance surfaces as the governing over-ratio
    // rather than silently passing, making demand.TorsionKnm a consumed action rather than a carried-but-ignored column.
    static double TorsionRatio(double torsionDemandKnm, double torsionalCapacityKnm) =>
        Math.Abs(torsionDemandKnm) <= double.Epsilon ? 0.0 : Math.Abs(torsionDemandKnm) / Math.Max(torsionalCapacityKnm, double.Epsilon);

    // TMS 402 axial-flexural: the slenderness-reduced compression capacity φPn = 0.80·φ·0.80·f'm·An·R (R the member
    // stability factor) and the flexural capacity φMn = φ·f'm·S; the unity check the worse of the two ratios.
    static Utilisation MasonryUtilisation(MasonryCompression m, Demand demand) {
        const double phi = 0.90;
        double pn = 0.80 * phi * 0.80 * m.FmMpa * m.NetAreaMm2 * Math.Clamp(m.SlendernessReduction, 0.0, 1.0) * 1e-3;
        double mn = phi * m.FmMpa * m.SectionModulusMm3 * 1e-6;
        double axial = Math.Abs(demand.AxialKn) / Math.Max(pn, double.Epsilon);
        double flexure = demand.MomentResultantKnm / Math.Max(mn, double.Epsilon);
        return Worst((axial, GoverningAction.Axial), (flexure, GoverningAction.Flexure));
    }

    // The worst (largest) ratio over a set of (ratio, governing-action) candidates — the unified governing-axis fold
    // every uniaxial-component arm drives, so a steel/timber/masonry check reports WHICH action governs, not just a ratio.
    static Utilisation Worst(params (double Ratio, GoverningAction Action)[] candidates) {
        (double ratio, GoverningAction action) = candidates.MaxBy(c => c.Ratio);
        return Utilisation.Of(ratio, action);
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class SectionCapacityResolver {
    // The ONE RC capacity boundary: discriminate the RC-build kind onto its VividOrange solver over the RcSection's
    // IConcreteSection, admit the eager solve ONCE, coerce the UnitsNet outputs to SI scalars at the edge, trap every
    // VividOrange throw onto ProfileFault.Capacity (the profile-sub-domain band 2300 capacity-solve slot, distinct from
    // the Section elastic-integral slot — a capacity telemetry reader bands a design-solve fault apart from a section fault).
    // The InteractionDiagram ctor IS the expensive solve — cached on the RcInteraction carrier, never re-solved.
    public static Fin<SectionCapacity> Resolve(RcSection rc, CapacityKind kind, DiagramResolution resolution, Op key) =>
        kind.Switch(
            rcInteraction: _ => Try(() => new ForceMomentEngine(rc.Section, resolution.ToSettings()).Mesh).ToFin()
                .MapFail(e => ProfileFault.Capacity(key, $"<rc-interaction-solve:{e.Message}>"))
                .Map(mesh => (SectionCapacity)new SectionCapacity.RcInteraction(mesh)),
            // The WHOLE ConcreteSectionProperties read is trapped in ONE Try: the ctor, the transformed-property reads,
            // AND EffectiveDepth(SectionFace.Bottom) — the engine's CalculateEffectiveDepth divides a bottom-layer
            // centroid by its area and throws/NaNs for a section with no bottom-face bars, so reading it OUTSIDE the
            // trap would leak a throw past the Fin; here every VividOrange throw rails ProfileFault.Capacity at the edge.
            rcElastic: _ => Try(() => {
                ConcreteSectionProperties p = new(rc.Section);
                return (SectionCapacity)new SectionCapacity.RcElastic(
                    p.TotalReinforcementArea.SquareMillimeters,
                    p.ConcreteArea.SquareMillimeters,
                    p.GeometricReinforcementRatio.DecimalFractions,
                    p.ReinforcementSecondMomentOfAreaYy.MillimetersToTheFourth,
                    p.ReinforcementSecondMomentOfAreaZz.MillimetersToTheFourth,
                    p.EffectiveDepth(SectionFace.Bottom).Millimeters,   // the true transformed extreme-fibre lever d
                    rc.ConcreteProfile.Unit.HeightMm.Value,             // gross depth h — the major-axis fibre lever cy = h/2
                    rc.ConcreteProfile.Unit.WidthMm.Value,              // gross width b — the minor-axis fibre lever cz = b/2
                    Fctm(EnConcreteFactory.CreateLinearElastic(rc.Concrete.Grade).Strength.Megapascals));
            }).ToFin().MapFail(e => ProfileFault.Capacity(key, $"<rc-elastic-solve:{e.Message}>")));

    // The steel LRFD lift — the steel#STEEL_FAMILY DesignCapacity (N·mm/N + CompactnessClass + slenderness + the AISC
    // §H3.1 TorsionalNmm) carried WHOLE into the rail as kN·m/kN; the steel capacity computation stays the steel owner's,
    // this only the unified rail. The §H3.1 design torsional resistance φTn = φT·Fcr·C is the steel owner's
    // DesignCapacity.TorsionalNmm column (positive for a CLOSED HSS the J/c modulus yields, 0 for an OPEN warping-torsion
    // shape) read DIRECTLY off the receipt — ONE source, no redundant parallel lift parameter beside the carried column,
    // so a torsion-loaded HSS folds against a real resistance and an open-shape torsion demand surfaces as the governing
    // over-ratio (the consumed-action discipline).
    public static SectionCapacity SteelLrfd(DesignCapacity capacity) =>
        new SectionCapacity.SteelLrfd(
            capacity.FlexuralNmm * 1e-6, capacity.CompressionN * 1e-3, capacity.ShearN * 1e-3, capacity.TorsionalNmm * 1e-6,
            capacity.Classification, capacity.Slenderness);

    // The timber EC5 lift — the timber#TIMBER_FAMILY TimberCapacity (N·mm/N design resistances + λ_rel + k_mod + the
    // §6.1.8 TorsionalNmm) carried WHOLE into the rail as kN·m/kN; the EN 1995 design-resistance computation stays the
    // timber owner's. The EN 1995-1-1 §6.1.8 torsional resistance T_Rd = k_shape·f_v,d·W_tor is the timber owner's
    // TimberCapacity.TorsionalNmm column read DIRECTLY off the receipt — ONE source, no redundant lift parameter — so the
    // T_Ed/T_Rd arm checks against a real resistance rather than an inert 0.
    public static SectionCapacity TimberEc5(TimberCapacity capacity) =>
        new SectionCapacity.TimberEc5(
            capacity.BendingNmm * 1e-6, capacity.CompressionN * 1e-3, capacity.ShearN * 1e-3,
            capacity.BearingPerpN * 1e-3, capacity.TorsionalNmm * 1e-6, capacity.RelativeSlenderness, capacity.Kmod);

    // The masonry TMS 402 lift — the cmu#CMU_FAMILY CmuStrength.FmMpa + the (grouted) net ComputedSection area/modulus
    // + the member slenderness reduction; the TMS 402 unity check is the MasonryCompression Check arm.
    public static SectionCapacity MasonryCompression(double fmMpa, ComputedSection section, double slendernessReduction) =>
        new SectionCapacity.MasonryCompression(fmMpa, section.AreaMm2.Value, section.SxMm3.Value, slendernessReduction);

    // EC2 mean flexural tensile strength from fck: fctm = 0.30·fck^(2/3) for ≤C50, 2.12·ln(1+(fck+8)/10) above —
    // the cracking-stress reference the RcElastic service check compares the transformed extreme-fibre stress against.
    // The fck source is EnConcreteFactory.CreateLinearElastic(grade).Strength — verified: Strength IS the parsed
    // characteristic cylinder strength fck (the first Cxx token), not the design fcd (.api/api-vividorange-materials.md).
    static double Fctm(double fckMpa) =>
        fckMpa <= 50.0 ? 0.30 * Math.Pow(fckMpa, 2.0 / 3.0) : 2.12 * Math.Log(1.0 + (fckMpa + 8.0) / 10.0);
}

// The biaxial utilisation: cast the demand (N, My, Mz) load ray from the origin against the closed IForceMomentMesh
// capacity onion and read the ratio of the demand magnitude to the ray's pierce magnitude on the hull surface — the
// exact Möller–Trumbore ray-triangle intersection over the IForceMomentTriFace.A/B/C facets, not a vertex surrogate.
// A balanced section's hull encloses the origin (the safe zero-load state interior), so a single positive front-face
// pierce gives the capacity boundary; a heavily eccentric or pure-tension section whose hull does NOT enclose the
// origin yields no positive pierce, and the cast returns a typed Overcapacity verdict rather than a silent +∞.
public static class CapacityRay {
    public static Utilisation Cast(IForceMomentMesh hull, Demand demand) {
        double demandMag = demand.NmmMagnitude;
        GoverningAction governing = Math.Abs(demand.AxialKn) >= demand.MomentResultantKnm
            ? GoverningAction.Axial : GoverningAction.BiaxialMoment;
        if (demandMag <= double.Epsilon) { return Utilisation.Of(0.0, governing); }   // zero load is trivially adequate
        (double dirN, double dirMy, double dirMz) = (demand.AxialKn / demandMag, demand.MomentYKnm / demandMag, demand.MomentZKnm / demandMag);
        // The pierce parameter t of the origin-cast unit demand ray through each tri-face; over a closed convex hull
        // enclosing the origin exactly one face yields a positive front-facing t — the capacity-boundary magnitude
        // along the load direction. None over EVERY face means the origin is outside the hull (an eccentric section).
        Option<double> boundary = toSeq(hull.Faces)
            .Map(f => Pierce(f, dirN, dirMy, dirMz))
            .Somes()
            .Filter(static t => t > 0.0)
            .OrderBy(static t => t)
            .HeadOrNone();
        return boundary.Match(
            Some: b => Utilisation.Of(demandMag / b, governing),
            None: () => Utilisation.Overcapacity(governing));
    }

    // Möller–Trumbore: the parametric distance t at which the origin ray (direction d) pierces triangle A-B-C, None
    // when the ray is parallel to the facet or the barycentric hit lands outside the triangle.
    static Option<double> Pierce(IForceMomentTriFace face, double dN, double dMy, double dMz) {
        (double ax, double ay, double az) = Coord(face.A);
        (double e1x, double e1y, double e1z) = Sub(Coord(face.B), (ax, ay, az));
        (double e2x, double e2y, double e2z) = Sub(Coord(face.C), (ax, ay, az));
        (double px, double py, double pz) = Cross((dN, dMy, dMz), (e2x, e2y, e2z));
        double det = e1x * px + e1y * py + e1z * pz;
        if (Math.Abs(det) < 1e-12) { return None; }
        double inv = 1.0 / det;
        double u = -(ax * px + ay * py + az * pz) * inv;          // origin - A = -A
        if (u is < 0.0 or > 1.0) { return None; }
        (double qx, double qy, double qz) = Cross((-ax, -ay, -az), (e1x, e1y, e1z));
        double v = (dN * qx + dMy * qy + dMz * qz) * inv;
        if (v < 0.0 || u + v > 1.0) { return None; }
        return (e2x * qx + e2y * qy + e2z * qz) * inv;            // t along the unit ray
    }

    static (double, double, double) Coord(IForceMomentVertex v) => (v.X.Kilonewtons, v.Y.KilonewtonMeters, v.Z.KilonewtonMeters);
    static (double, double, double) Sub((double x, double y, double z) a, (double x, double y, double z) b) => (a.x - b.x, a.y - b.y, a.z - b.z);
    static (double, double, double) Cross((double x, double y, double z) a, (double x, double y, double z) b) =>
        (a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
}
```

## [03]-[RESEARCH]

- [RC_INTERACTION_HULL]: REALIZED — the `RcInteraction` capacity case is the ultimate biaxial Force-Moment-Moment capacity hull `VividOrange.InteractionDiagram` computes over the `Connection/reinforcement#RC_SECTION` `IConcreteSection`: `SectionCapacityResolver.Resolve` constructs `new InteractionDiagram(rc.Section, resolution.ToSettings())` (the eager solve that `Triangle`-meshes the concrete+rebar section into `AnalyticalFace` fibres, runs the `Parallel.For` strain-plane sweep integrating fibre stress, and `MIConvexHull`-welds the (N, My, Mz) cloud into the closed onion, `.api/api-vividorange-interactiondiagram.md` `[FIBRE_INTEGRATION_CONTRACT]`) and reads `diagram.Mesh` as the cached `IForceMomentMesh`. VERIFIED via `assay api query InteractionDiagram --key VividOrange.InteractionDiagram`: the ctor pair `(IConcreteSection)` / `(IConcreteSection, DiagramSettings)` and the `public IForceMomentMesh Mesh { get; set; }` property, the sweep emitting `ForceMomentVertex(N, My, Mz, ForceUnit.Kilonewton, TorqueUnit.KilonewtonMeter)` so the hull coordinates are kN/kNm. The `Triangle` mesher and `MIConvexHull` hull builder are encapsulated `internal` inside the engine — this AEC-DOMAIN owner composes the welded hull through the constructor, mints NO direct `Triangle`/`MIConvexHull` call (the strata-correct seam: those primitives are `Rasm`-kernel-owned, consumed transitively, `.api/api-triangle.md` / `.api/api-miconvexhull.md` `[STACKING_LAW]`). The hull is read through the `IForceMomentMesh` interface floor (`hull.Faces` → `IForceMomentTriFace.A`/`B`/`C` → `IForceMomentVertex.X`/`Y`/`Z`, all VERIFIED on the `VividOrange.ICartesianBase` generic floor `ICartesianMesh.Faces`/`ICartesianTriFace.A,B,C`/`ICartesian3d.X`+`ILocalCartesian2d.Y,Z`), the `Force`/`Torque` coordinates as `UnitsNet` quantities through `Force.Kilonewtons`/`Torque.KilonewtonMeters`, never the `ForceMomentMesh` concrete and never the facet `Area` `Ratio` as a physical quantity. The eager solve caches on the `RcInteraction` carrier so a `Check(demand)` reads the cached hull, never re-solving. Ripple counterpart: `Connection/reinforcement` `[RC_SECTION]` (the `RcSection`/`IConcreteSection` input this owner consumes).
- [RC_ELASTIC_TRANSFORMED]: REALIZED — the `RcElastic` capacity case is the elastic transformed-section reinforcement properties `VividOrange.Sections.SectionProperties` `ConcreteSectionProperties` computes over the same `IConcreteSection`: `new ConcreteSectionProperties(rc.Section)` reads `TotalReinforcementArea`/`ConcreteArea`/`GeometricReinforcementRatio`/`ReinforcementSecondMomentOfAreaYy`/`Zz` AND the bottom-face `EffectiveDepth(SectionFace.Bottom)` as `UnitsNet` quantities coerced to SI scalars at the edge (VERIFIED on the `IConcreteSectionProperties` floor: `Area TotalReinforcementArea`/`ConcreteArea`, `Ratio GeometricReinforcementRatio`, `AreaMomentOfInertia ReinforcementSecondMomentOfAreaYy`/`Zz`, `Length EffectiveDepth(SectionFace)`, `.api/api-vividorange-sections-sectionproperties.md` `[RC seam]`). The EC2 SLS cracking check compares the maximum-tensile extreme-CONCRETE-fibre transformed stress against `fctm` as the FULL combined action `σ = N/A ± My·cy/Iyy ± Mz·cz/Izz` — a SIGNED axial `N/A` (the `Demand` convention − compression / + tension, so a compressive service axial DELAYS cracking, the physically-correct SLS behaviour a `|N|` would over-predict) plus BOTH bending axes' tension-side fibre stress (`cy = h/2`, `cz = b/2` the gross half-depths of the symmetric uncracked transformed section), so the arm consumes `ConcreteAreaMm2` (the `N/A` divisor) and `InertiaZzMm4`+`WidthMm` (the minor-axis fibre stress) the prior major-axis-bending-only slice carried DEAD — never the effective depth `d`. The bottom-face `EffectiveDepth(SectionFace.Bottom)` is the distinct ULS flexural lever to the tension steel; it is a first-class `ConcreteSectionProperties` concept member (the `.api` `[RC seam]` lists it) carried on the `RcElastic` record for the `cs:AEC_SIMULATION_BRIDGE` consumer's moment-capacity readout, never conflated with the SLS extreme-fibre distance. One `IConcreteSection` minted at `Connection/reinforcement#RC_SECTION` drives BOTH the elastic transformed-section properties here and the ultimate N-M-M hull (`.api/api-vividorange-sections-sectionproperties.md` `[01]-[RC_COMPOSITION_PATH]`), so the cracked/uncracked elastic check and the ultimate capacity envelope share one section input — the elastic `RcElastic` is the transformed-section complement the bare `profile#PROFILE_OWNER` `ParametricSection` (gross elastic over any `IProfile`) does not compute, this owner the reinforced-section transformed properties.
- [STEEL_LRFD_LIFT]: REALIZED — the `SteelLrfd` capacity case lifts the `steel#STEEL_FAMILY` `DesignCapacity` WHOLE into the one `SectionCapacity` rail: the AISC 360 Chapters F/E/G rolled-steel LRFD (`φMn`/`φPn`/`φVn`, plus the AISC 360 Chapter I composite and the AISI S100 cold-formed arms) already derived over the computed `SteelSection` columns, carried as the SI N·mm/N → kN·m/kN projection PLUS the `CompactnessClass` Table B4.1 verdict and the column slenderness `λ` the earlier draft dropped (so the `Check` reports the governing compactness in a design report, not a bare ratio). A steel column and an RC column are checked through the SAME `Check(demand)` fold — never a parallel `SteelBeamCheck` surface. The `SteelLrfd` interaction is the AISC `max(N/φPn, M/φMn, V/φVn, T/φTn)` over the moment/shear/torsion demands (the biaxial moment and the shear resultant the `Demand` projects, the §H3.1 torsion `demand.TorsionKnm` folded against the lifted `TorsionalKnm`), the governing axis the typed `GoverningAction` the larger ratio names; the full AISC H1.1 `8/9`-factor combined-axial-bending split is one `SteelUtilisation` refinement, the realized check the governing-axis ratio. A zero torsion demand contributes a 0 ratio (so a `φTn = 0` open-shape column never spuriously governs), a nonzero torsion against a `φTn = 0` open shape surfaces as the governing over-ratio rather than silently passing — `demand.TorsionKnm` is a CONSUMED action, not a carried-but-ignored column. The §H3.1 `φTn = φT·Fcr·C` resistance is REALIZED on the steel `DesignCapacity.TorsionalNmm` column (the `SteelDesign.TorsionalResistance` projection over the canonical `JMm4` St-Venant constant, positive for the CLOSED HSS/pipe topology and engineering-zero for an OPEN warping-torsion shape), which the `SectionCapacityResolver.SteelLrfd(DesignCapacity)` lift reads DIRECTLY onto `TorsionalKnm` — one source, no redundant parallel lift parameter, so a torsion-loaded HSS checks against a real resistance. The steel capacity computation stays the `steel#STEEL_FAMILY` owner, this page only the unified utilisation rail. Ripple counterpart: `steel#STEEL_FAMILY` `[STEEL_FAMILY]` (the `DesignCapacity` receipt lifted here, the REALIZED AISC §H3.1 `DesignCapacity.TorsionalNmm` column the `SteelLrfd` lift folds against `demand.TorsionKnm`).
- [TIMBER_EC5_LIFT]: REALIZED — the `TimberEc5` capacity case lifts the `timber#TIMBER_FAMILY` `TimberCapacity` WHOLE into the rail, closing the cross-file ripple `timber#EC5_DESIGN_CAPACITY` explicitly declared ("the `TimberCapacity` lifts into the `Profiles/capacity#SECTION_CAPACITY` unified utilisation rail as a `SectionCapacity.TimberEc5` case the SAME way the steel `DesignCapacity` lifts as `SteelLrfd`"): the EN 1995-1-1 design resistances (`M_Rd` = `k_h·k_mod·f_m,k·W / γ_M`, `N_Rd` = `k_c·k_mod·f_c0,k·A / γ_M` column-buckling-reduced, `V_Rd` = `k_mod·f_v,k·k_cr·A_shear / γ_M` rolling-shear for a CLT panel, `R_90,Rd` = `k_mod·f_c90,k·A_bearing / γ_M`) carried as kN·m/kN with the relative slenderness `λ_rel` and the `k_mod` service×duration factor preserved. The `TimberEc5` `Check` is the EN 1995 `max(M_Ed/M_Rd, N_Ed/N_Rd, V_Ed/V_Rd, T_Ed/T_Rd)` over the moment/shear/torsion demands (the §6.1.8 torsion `demand.TorsionKnm` folded against the lifted `TorsionalKnm`, the same zero-demand-inert / unbounded-resistance-governs discipline the steel arm holds), the governing axis the typed `GoverningAction`; the perpendicular-bearing `R_90,Rd` rides the carrier for a bearing check a future `Demand` reaction column drives. A timber column and a steel column are checked through ONE `Check(demand)` fold differing only in the capacity case — the EN 1995 design rules HAND-ROLLED in `timber#TIMBER_FAMILY` (no .NET EC5 package), the unified verdict this owner's. The §6.1.8 `T_Rd = k_shape·f_v,d·W_tor` resistance is REALIZED on the timber `TimberCapacity.TorsionalNmm` column (`TimberDesign.Capacity` over the rectangular `W_tor` Roark torsion modulus and the design longitudinal shear `f_v,d`), which the `SectionCapacityResolver.TimberEc5(TimberCapacity)` lift reads DIRECTLY onto `TorsionalKnm` — one source, no redundant lift parameter, so a torsion-loaded glulam member checks against a real resistance. Ripple counterpart: `timber#TIMBER_FAMILY` `[TIMBER_CAPACITY]` (the `TimberCapacity` receipt lifted here, the REALIZED EN 1995-1-1 §6.1.8 `TimberCapacity.TorsionalNmm` column the `TimberEc5` lift folds against `demand.TorsionKnm`).
- [MASONRY_COMPRESSION_CHECK]: REALIZED — the `MasonryCompression` capacity case closes the cross-file ripple `cmu#CMU_STRENGTH_GRADE` / `cmu#CMU_GROUT_REINFORCEMENT` explicitly name ("the `CmuSection.PrismStrength` receipt feeds the `Profiles/capacity#SECTION_CAPACITY` masonry compressive utilisation … the masonry `MasonryCompression` capacity case the `f'm` + grouted net section feed"): the case carries the `cmu#CMU_FAMILY` `CmuStrength.FmMpa` specified masonry strength `f'm`, the (grouted) net `ComputedSection` area + section modulus (the `CmuSection.NetSection`/`GroutedSection` output through the shared `ParametricSection.Hollow` solver), and the TMS 402 §9.3.4.1.1 member-stability slenderness reduction `R = [1 - (h/140r)²]`. The `MasonryUtilisation` `Check` arm is the TMS 402 axial-flexural unity check `max(P/φPn, M/φMn)` with `φPn = 0.80·φ·0.80·f'm·An·R` (the slenderness-reduced compression capacity) and `φMn = φ·f'm·S`, the governing axis the typed `GoverningAction`. A masonry wall and a steel column are checked through the SAME `Check(demand)` fold — the TMS 402 strength tables HAND-ROLLED in `cmu#CMU_FAMILY` (the `CmuStrength.Resolve` unit-strength method), the unified verdict this owner's. Ripple counterpart: `cmu#CMU_FAMILY` `[CMU_FAMILY]` (the `CmuStrength` `f'm` + `GroutedSection` the case feeds).
- [UTILISATION_RAY_CAST]: REALIZED — the `RcInteraction` utilisation is the `CapacityRay.Cast` of the applied demand `(N, My, Mz)` vector against the `IForceMomentMesh` hull: the demand-magnitude / pierce-magnitude ratio along the load ray, the boundary the exact Möller–Trumbore intersection of the origin-cast unit demand ray through the `IForceMomentTriFace.A`/`B`/`C` facets (the `IForceMomentVertex.X`/`Y`/`Z` `Force`/`Torque` coordinates read through the floor), over the closed convex onion the engine welds. Each face yields an `Option<double>` pierce parameter `t` (`None` for a parallel or barycentrically-missed facet); the smallest positive front-face `t` is the capacity-boundary magnitude, so a ratio ≤ 1 is adequate. The `Cast` is TOTAL over the origin-interiority assumption — a hull that does NOT enclose the origin (a heavily eccentric or pure-tension section whose safe state is not interior) yields no positive pierce and returns a typed `Utilisation.Overcapacity` verdict (the section is outside its capacity envelope) rather than the silent `ratio = +∞` a divide-by-epsilon boundary fallback would produce, and a zero-magnitude demand returns a trivially-adequate ratio 0. The governing axis is the typed `GoverningAction.Axial`/`BiaxialMoment` (the axial-vs-moment-resultant comparison), never a stringly-typed label. A finer `DiagramResolution.Fine` (48 steps) refines the discretised hull the same fold casts against, never a second algorithm. The `Utilisation` verdict crosses to `cs:AEC_SIMULATION_BRIDGE` as portable scalar data keyed by section, never a `VividOrange` type crossing the seam.
- [PROFILE_FAULT_RAIL]: REALIZED — the capacity owner rails the `profile#PROFILE_OWNER` `ProfileFault` band-2300 fault, the SAME profile-sub-domain rail every sibling Profiles page uses (`steel#STEEL_FAMILY` `SectionReader.Read` → `ProfileFault.Family`, `cmu#CMU_FAMILY`/`timber#TIMBER_FAMILY` → `ProfileFault.Family`, `Connection/reinforcement#RC_SECTION` → `ConnectionFault`). A section-capacity solve is a profile-sub-domain concern, so its fault bands 2300 with its Profiles siblings — NOT the `Appearance/bsdf#SHADING_FRAME` `MaterialFault` band 2450, the APPEARANCE band that would conflate a structural fault with a shading fault for any telemetry reader banding by `Expected.Code`, and NOT a thin private `CapacityFault` rename wrapper that adds zero domain value. `SectionCapacityResolver.Resolve` rails the REALIZED `ProfileFault.Capacity(key, $"<rc-interaction-solve:{e.Message}>")` / `ProfileFault.Capacity(key, $"<rc-elastic-solve:{e.Message}>")` (a non-EN material the `IEnConcreteMaterial`/`IEnRebarMaterial` cast cannot read, an under-reinforced degenerate section, a no-bottom-bar `EffectiveDepth` divide, or a hull-weld failure), so no `VividOrange` throw reaches an interior signature. The dedicated `ProfileFault.Capacity` case (the seventh `ProfileFault` `[Union]` arm, `Category => "Capacity"`, distinct from `Dimension`/`Coring`/`Family`/`Bond`/`Mortar`/`Section`) is now REALIZED on the `profile#PROFILE_OWNER` owner so a section-CAPACITY-SOLVE fault carries a capacity-specific `Category` a telemetry reader bands apart from a section-PROPERTY (`Section`) or registration (`Family`) fault — the capacity solve no longer flattens onto `ProfileFault.Family`. Ripple counterpart: `profile#PROFILE_OWNER` `[PROFILE_OWNER]` (the realized `ProfileFault.Capacity` case the capacity solves rail).
