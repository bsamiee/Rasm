# [MATERIALS_CAPACITY]

THE SECTION-CAPACITY OWNER and THE ONE UTILISATION RAIL. One `SectionCapacity` `[Union]` is the closed structural-capacity surface a `Component` cross-section carries beyond its elastic `ComputedSection`, and one `Demand` folded against it through `Check` is the typed `Utilisation` verdict — so EVERY family's design check is one polymorphic fold differing only in the capacity case, never a per-family `RcColumnCheck`/`SteelBeamCheck`/`MasonryWallCheck` surface. The closed case set spans the realized `ComponentFamily` structural rails: `RcInteraction` (the ultimate biaxial Force-Moment-Moment capacity hull `VividOrange.InteractionDiagram` welds over the `reinforcement#RC_SECTION` `IConcreteSection`), `RcElastic` (the elastic transformed-section reinforcement properties `VividOrange.Sections.SectionProperties` `ConcreteSectionProperties` computes over the same section, PLUS the EC2 §6.2 section-level shear screen over the bottom-face tension steel and the two-leg link area `CrossSectionalShearReinforcementArea` carries), `SteelLrfd` (the AISC 360 `steel#STEEL_FAMILY` `DesignCapacity` `φMn`/`φMny`/`φPn`/`φVn` + `CompactnessClass`/slenderness lifted whole), `TimberEc5` (the EN 1995-1-1 `timber#TIMBER_CAPACITY` `TimberCapacity` design-resistance receipt lifted whole — `M_Rd,y`/`M_Rd,z` per axis with the §`6.1.6`(2) `k_m` weight), and `MasonryCompression` (the TMS 402 axial-flexural unity check PLUS the §`9.2.2` flexural-tension screen over the Table `9.1.9.2` `fr` — the `cmu#CMU_FAMILY` `CmuStrength` `f'm` + grouted `ComputedSection` + the `masonry#MASONRY_FAMILY` `RuptureModulus` mortar-keyed row feed). A capacity is admitted to the family ONLY when no existing case's column set carries it: each sibling family page that hand-rolls its design rules (`steel#STEEL_FAMILY`, `timber#TIMBER_CAPACITY`, `cmu#CMU_FAMILY`) lifts its already-computed receipt into ONE case here, and the RC cases are the two `Resolve` builds over the section input — the design-code COMPUTATION stays the family owner's, the unified VERDICT this owner's. The rail is TOTAL over the load path: `MasonryReinforced` carries the TMS 402 §9.3 steel-couple arm over the cmu lattice facts, `GlassPane` the EN 16612 pane resistance the glazing family lifts, and `Connection` the weld/adhesive/stud/connector receipts — one `Check` from cross-section to weld to hanger — while `SectionSelection.Lightest` is the rail's INVERSE query, the lightest-adequate scan over the frozen catalogue maps the full-database steel seed supplies. This owner is the ULTIMATE complement to `component#COMPONENT_OWNER` `SectionSolver`: that solver gives the elastic `ComputedSection` every family solves from its `SectionProfile` arm, THIS owner gives the reinforced-section transformed properties, the EC2 section-level shear screen, the ultimate capacity hull, and the unified utilisation fold the elastic solver does not. The `InteractionDiagram` constructor RUNS the full eager fibre-integration solve at construction (the `Triangle` section mesh, the `Parallel.For` strain-plane sweep, the `MIConvexHull` hull weld are encapsulated `internal` — this owner composes the welded `IForceMomentMesh`, never the meshing primitive), so a design page constructs the capacity ONCE per section/settings and reads `diagram.Mesh` cached, never re-solving per query. The page composes `reinforcement#RC_SECTION` `RcSection`/`IConcreteSection` for the RC input, `VividOrange.InteractionDiagram` (`InteractionDiagram`/`DiagramSettings`/`IForceMomentMesh`) for the N-M-M hull, `VividOrange.Sections.SectionProperties` `ConcreteSectionProperties` for the elastic transformed-section properties, `VividOrange.Materials` `EnConcreteFactory` for the EC2 `fck` the cracking reference reads, the `steel#STEEL_FAMILY` `DesignCapacity` / `timber#TIMBER_CAPACITY` `TimberCapacity` / `cmu#CMU_FAMILY` `CmuStrength` sibling receipts, the in-folder `UnitsNet` `Force`/`Torque`/`Area`/`Length` quantity coercion at the edge, and the `component#COMPONENT_OWNER` `ComponentFault` band-2300 rail (the SAME component-sub-domain fault every sibling Component family page rails — NOT a borrowed appearance band) for a non-finite, degenerate, or infeasible solve; the capacity surface and the utilisation verdict feed the forward `Rasm.Compute/structural#DESIGN_CHECK` structural-Assessment route by `MaterialId`/section key, host-neutral here, the `IForceMomentMesh` round-tripping through the realized `SectionCapacity.Freeze`/`Thaw` `VividOrange.Serialization` pair for the C#-internal cache — the eager `Steps²` solve is paid once, persisted, and rehydrated, never re-run.

## [01]-[INDEX]

- [01]-[SECTION_CAPACITY]: the `SectionCapacity` `[Union]` (`RcInteraction` N-M-M hull · `RcElastic` transformed-section · `SteelLrfd` rolled-steel · `TimberEc5` EC5 receipt · `MasonryCompression` TMS 402 compression + §`9.2.2` flexural-tension · `MasonryReinforced` TMS 402 §`9.3` steel-couple · `GlassPane` EN 16612 pane · `Connection` weld/adhesive/stud/connector load path) over the `component#COMPONENT_OWNER` `ComponentFault` band-2300 rail, the `CapacityBuild` RC-build request `[Union]` (hull · elastic — the hull arm alone carrying its `DiagramResolution`), the `DiagramResolution` `[SmartEnum]` mesh/sweep-refinement policy folding to a `DiagramSettings`, the `Demand` applied-action shape (axial · biaxial moment · biaxial shear · torsion · bearing), the `GoverningAction` `[SmartEnum]` verdict axis, the `Utilisation` typed verdict, the `CapacityReceipt` sibling-receipt request `[Union]` (steel · timber · masonry · reinforced-masonry · glass · weld · adhesive · stud · connector — each case carrying its full lift context), and the `SectionCapacity.Resolve` eager-solve boundary plus the ONE `Lift(CapacityReceipt)` total-`Switch` entry and the `Freeze`/`Thaw` C#-internal hull-cache round-trip — every boundary static on the union owner, no satellite resolver class — plus the `SectionSelection.Lightest` inverse sizing fold over the frozen catalogue maps.

## [02]-[SECTION_CAPACITY]

- Owner: `SectionCapacity` is the closed capacity family spanning the member rails and the connection load path; `Demand` admits the signed action vector; `Utilisation` distinguishes a bounded verdict, a section pass requiring a named member check, and an unbounded overcapacity verdict; scalar `MasonryReduction` admits the `(0,1]` stability factor; `CapacityBuild` and `CapacityReceipt` carry solve and lift modality.
- Cases: `RcInteraction` (the ultimate biaxial N-M-M capacity hull as the `IForceMomentMesh` over an `IConcreteSection`, `VividOrange.InteractionDiagram`) · `RcElastic` (the elastic section state read off the ONE `ConcreteSectionProperties` carrier the `RcSection` receipt holds — `TotalReinforcementArea`/`ConcreteArea`/`GeometricReinforcementRatio`, the GROSS `MomentOfInertiaYy`/`Zz` (the inherited base polygon integral — the SLS fibre divisors) AND the `ReinforcementSecondMomentOfAreaYy`/`Zz` `Σ(As·d²)` steel moments (the cracked-`Icr` readout), + the bottom-face `EffectiveDepth(SectionFace)` ULS lever + the bottom-face `ReinforcementArea(SectionFace)` tension steel and the two-leg `CrossSectionalShearReinforcementArea` link area + the gross depth AND width (the major/minor-axis SLS extreme-fibre levers) + the parsed `fck` and its EC2 `fctm` cracking limit, the combined `N/A ± My·cy/Iyy ± Mz·cz/Izz` SLS check AND the EC2 §6.2 shear screen) · `SteelLrfd` (the rolled/composite/cold-formed `steel#STEEL_FAMILY` `DesignCapacity` `φMn`/`φMny`/`φPn`/`φVn` + `CompactnessClass` + slenderness lifted WHOLE — the §F6 minor column the per-axis H1.1 fold divides against) · `TimberEc5` (the EN 1995-1-1 `timber#TIMBER_CAPACITY` `TimberCapacity` `M_Rd,y`/`M_Rd,z`/`N_Rd`/`V_Rd`/`R_90,Rd` + `λ_rel` + `k_m` + `k_mod` lifted WHOLE — the member minor column `k_h(w)`-scaled with no `k_crit`, the panel minor research-gated 0) · `MasonryCompression` (the TMS 402 axial-flexural check + the §`9.2.2` flexural-tension screen the `cmu#CMU_FAMILY` `CmuStrength` `f'm` + the grouted `ComputedSection` net area AND both net moduli `SxMm3`/`SyMm3` + slenderness reduction + the `masonry#MASONRY_FAMILY` `RuptureModulus` Table `9.1.9.2` `fr` feed) — plus `MasonryReinforced` (the TMS 402 §`9.3` steel-couple arm over the cmu lattice's `ReinforcedCells`/`RebarBarMm`/grouted-net facts and the bar grade's yield), `GlassPane` (the EN 16612 governing-pane per-metre resistance the `glazing#GLAZING_FAMILY` `GlassCapacity` receipt lifts WHOLE), and `Connection` (the `joint#JOINT_FAMILY` weld/adhesive/stud design values and the `connector#CONNECTOR_FAMILY` duration-governed capacity as one shear/tension/bearing column triple) — the closed structural-capacity family across steel/RC/timber/masonry/glass and the connection load path; a capacity is a `SectionCapacity` case over a section or connection receipt, never a per-section-type check.
- Entry: `SectionCapacity.Resolve(RcSection, CapacityBuild, Op)` dispatches the RC solve request; `SectionCapacity.Lift(CapacityReceipt)` dispatches complete steel, timber, and masonry receipts; internal `Freeze`/`Thaw` persist the trusted C# hull cache; and `Check(Demand)` returns the closed `Utilisation` verdict. `CapacityReceipt.Masonry` carries `MasonryReduction`, so no raw stability factor reaches construction. The `RcInteraction` arm casts the raw `(N, My, Mz)` demand vector against the hull and interprets the smallest positive intersection parameter as the capacity multiplier; utilization is its reciprocal. Force and moment axes are never Euclidean-normalized together.
- Packages: VividOrange.InteractionDiagram (`InteractionDiagram`/`DiagramSettings`, the eager-solve ctor + `Mesh`; `.api/api-vividorange-interactiondiagram.md`), VividOrange.IForceMomentInteraction (`IForceMomentMesh`/`IForceMomentVertex`/`IForceMomentTriFace` the hull read through, the `Faces`/`A`/`B`/`C`/`X`/`Y`/`Z` `Force`/`Torque` members; `.api/api-vividorange-iforcemomentinteraction.md`), VividOrange.Sections.SectionProperties (`ConcreteSectionProperties` the transformed-section carrier RIDING the `RcSection` receipt — the `EffectiveDepth(SectionFace)`/`ReinforcementArea(SectionFace)` face queries, the `CrossSectionalShearReinforcementArea` two-leg link area, and the inherited base `MomentOfInertiaYy`/`Zz` gross polygon integral the SLS fibre divisors read; `.api/api-vividorange-sections-sectionproperties.md`), VividOrange.Sections (`IConcreteSection`/`SectionFace` from the `reinforcement#RC_SECTION` `RcSection`; `.api/api-vividorange-sections.md`), VividOrange.Materials (`EnConcreteFactory.CreateLinearElastic` whose `LinearElasticMaterial.Strength` IS the parsed `fck` — decompile-verified: the factory parses the first `Cxx` token of the grade, so `Strength.Megapascals` is the characteristic cylinder strength the EC2 `fctm` AND the §6.2 shear screen read; `.api/api-vividorange-materials.md`), VividOrange.Serialization (`JsonSerializationExtensions.ToJson`/`FromJson<T>` `where T : ITaxonomySerializable` — the `Freeze`/`Thaw` C#-internal hull cache over the marker `IForceMomentMesh` itself extends, `$type`-tagged Newtonsoft wire + `UnitsNet` SI-scalar+unit quantities, producer=consumer only; `.api/api-vividorange-serialization.md`), UnitsNet (`Force.Kilonewtons`/`Torque.KilonewtonMeters`/`Area`/`Length`/`Ratio`/`Angle` coerced at the edge; `libs/csharp/.api/api-unitsnet.md`), Rasm.Element (project — `MaterialId`/`ProfileRef` the seam-carried identity, seam-canonical), Rasm (project — `PositiveMagnitude` from `Rasm.Numerics`, `Op`/`Context` from `Rasm.Domain`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Fold`), Thinktecture.Runtime.Extensions (`[Union]` for `SectionCapacity`/`CapacityBuild`, `[SmartEnum]` for `DiagramResolution`/`GoverningAction`). Triangle + MIConvexHull ride transitively INSIDE the `InteractionDiagram` engine (encapsulated `internal`, `.api/api-triangle.md` / `.api/api-vividorange-forcemomentinteraction.md [TRANSITIVE_CONVEX_HULL]`) — this owner mints NO direct mesher/hull call, composing only the welded `IForceMomentMesh`. The `steel#STEEL_FAMILY` `DesignCapacity`, `timber#TIMBER_CAPACITY` `TimberCapacity`, and `cmu#CMU_FAMILY` `CmuStrength` are sibling-page receipts lifted, never re-computed.
- Growth: a new structural family's capacity is one `SectionCapacity` `[Union]` case binding either a `Resolve` build (a section-input solve) or a lift factory (an already-computed sibling receipt) plus one `Check` arm — a moment-curvature `RcInteraction` refinement, a panel diaphragm unit-shear check — admitted only when no existing case's column set carries it; a new demand axis is one `Demand` column (a warping bimoment, a second-order P-Δ amplifier); a new utilisation metric one `Utilisation`/`GoverningAction` projection — never a per-section-type capacity surface, never a re-derived elastic property where `ConcreteSectionProperties` computes it, never a direct `Triangle`/`MIConvexHull` call where the `InteractionDiagram` engine welds the hull; a persisted-capacity need is the one `Freeze`/`Thaw` pair over the `ITaxonomySerializable` marker, never a second serializer; the `steel`/`timber`/`cmu` design receipts stay the family-owner derivation lifted here, never re-computed; a fire-modality lift case composes the landed family-owner fire facts — `SteelDesign` `FireRetention`/`SectionFactorPerM`/`CriticalTemperatureC` and the timber `ResidualStack` charred section — as one more `CapacityReceipt` case over the same lift law, never a page-local fire derivation.
- Boundary: `SectionCapacity.Resolve` is the BOUNDARY_ADMISSION point where the `VividOrange.InteractionDiagram` engine is admitted EXACTLY ONCE and the `ConcreteSectionProperties` carrier — admitted once at `RcSectionBuilder.Of`, riding the `RcSection` receipt — is READ, never re-constructed — the `InteractionDiagram` ctor runs the expensive eager solve (`.api/api-vividorange-interactiondiagram.md` `[construction law]`) and a non-EN material whose `IEnConcreteMaterial`/`IEnRebarMaterial` cast the engine cannot read, an under-reinforced degenerate section, or a hull-weld failure rails `ComponentFault.Capacity` (the component-sub-domain band 2300 — `FaultBand.Component` on the registry — the dedicated capacity-solve slot distinct from the `Section` elastic-integral slot `component#COMPONENT_OWNER` `SectionSolver.Admit` rails, both band 2300 with their Component siblings, NOT the `Appearance/bsdf#SHADING_FRAME` `MaterialFault` band 2450) rather than throwing, so no `VividOrange` throw and no infeasible hull reaches an interior signature; the `IForceMomentMesh` is read THROUGH its interface floor (`.api/api-vividorange-iforcemomentinteraction.md` `[LOCAL_ADMISSION]`), never the `ForceMomentMesh` concrete, and the `Force`/`Torque` hull coordinates carry as `UnitsNet` quantities coerced to SI base (`Force.Kilonewtons`/`Torque.KilonewtonMeters`) once at the edge so no interior signature carries the hull as raw `double`; the `Triangle` section mesher and the `MIConvexHull` hull builder are encapsulated `internal` inside the engine (`.api/api-triangle.md` `[STACKING_LAW]` / `.api/api-vividorange-forcemomentinteraction.md [TRANSITIVE_CONVEX_HULL]` `[STACKING_LAW]`) — this AEC-DOMAIN owner mints NO direct mesher/hull call, composing the welded hull through the constructor, the strata-correct seam (the computational-geometry primitives are `Rasm`-kernel-owned, consumed transitively here); the eager solve is cached on the `SectionCapacity` `RcInteraction` carrier (`.api/api-vividorange-interactiondiagram.md` `[LOCAL_ADMISSION]` — construct once per section/settings, never re-solve per query), so a `Check(demand)` reads the cached hull; the `RcInteraction` utilisation is the exact Möller–Trumbore intersection of the origin-cast demand ray against the hull faces (the `IForceMomentTriFace.A`/`B`/`C` the demand vector pierces, the positive front-face pierce `t` the capacity boundary along the load direction), the no-pierce case (an eccentric hull that does not enclose the origin) yielding a typed over-capacity verdict rather than a silent `+∞`, NEVER the facet `Area` `Ratio` read as a physical quantity (`.api/api-vividorange-iforcemomentinteraction.md` `[AXIS_SEMANTICS]`); the `Utilisation.Governing` is the typed `GoverningAction` `[SmartEnum]` (axial · flexure · biaxial-moment · shear · torsion · bearing — ONE canonical term per action; a `bending` synonym row beside `flexure` is the deleted form), NEVER a stringly-typed verdict; the capacity surface is host-neutral — the `IForceMomentMesh` round-trips through the realized `Freeze`/`Thaw` pair (`ToJson`/`FromJson<IForceMomentMesh>` over the marker the interface itself extends, `.api/api-vividorange-serialization.md`) for the C#-internal cache, producer=consumer ONLY: the `TypeNameHandling.Objects` `$type` wire is a deserialization-gadget surface, so `Thaw` is fed exclusively JSON a trusted `Freeze` minted, never an external document, and the `$type` shape NEVER crosses to a peer (distinct from the canonical Thinktecture wire) — the utilisation verdict crosses to `Rasm.Compute/structural#DESIGN_CHECK` as portable scalar data keyed by section, never a `VividOrange` assembly type crossing the boundary.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Numerics;                                  // PositiveMagnitude (the >0 finite magnitude the ComputedSection AreaMm2/SxMm3 + ComponentUnit dimension columns carry — the kernel value-object atoms live in Rasm.Numerics, NOT Rasm.Domain)
using Rasm.Domain;                                   // Op (the boundary-admission key SectionCapacity.Resolve rails the ComponentFault on)
using Rasm.Element.Composition;      // MaterialId, ProfileRef (the seam-carried identity — STAYS seam-canonical, the rename stops at the Materials boundary)
using Rasm.Element.Properties;       // MeasureValue, QuantityType, Dimension
using Thinktecture;
using VividOrange.ForceMomentInteraction;            // IForceMomentMesh, IForceMomentVertex, IForceMomentTriFace, the Faces/A/B/C/X/Y/Z floor
using ForceMomentEngine = VividOrange.ForceMomentInteraction.InteractionDiagram;  // the eager-solve engine (alias frees the bare name for the SectionCapacity owner)
using VividOrange.Sections;                          // IConcreteSection, SectionFace (the RcSection input + the effective-depth face)
using VividOrange.Materials.StandardMaterials.En;    // EnConcreteFactory (the LinearElasticMaterial.Strength == parsed fck the EC2 fctm + §6.2 shear screen read)
using VividOrange.Serialization;                     // JsonSerializationExtensions ToJson/FromJson (the Freeze/Thaw C#-internal hull cache)
using UnitsNet;                                      // Force, Torque, Area, Length, Ratio, Angle (coerced at the edge)
using static LanguageExt.Prelude;                    // toSeq, Some, None, Optional

// The capacity owner declares in the ONE flat Rasm.Materials.Component namespace (the codemap maps Component/Capacity.cs
// flat, and dotnet_style_namespace_match_folder = true:error forces the folder path), so it composes every family owner
// it lifts receipts from — ComputedSection/ComponentFault, DesignCapacity/CompactnessClass (steel), TimberCapacity
// (timber), CmuStrength/CmuRow (cmu), RuptureModulus/MortarSystem/MortarType (masonry), RcSection/RebarGradeRow
// (reinforcement), GlassCapacity (glazing), JointRow (joint), ConnectorCapacity (connector) — by bare name.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
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

// The RC-build request for SectionCapacity.Resolve — the TWO capacity surfaces built FROM an RcSection input, each
// arm carrying EXACTLY the knobs its solver consumes: the hull build its DiagramResolution, the elastic build nothing.
// The prior loose (CapacityKind, DiagramResolution) parameter pair is the DELETED form — it forced a half-dead knob
// onto every elastic call. The steel/timber/masonry cases are LIFTS of already-computed sibling receipts (the ONE
// Lift over the CapacityReceipt request union), not Resolve builds, so this request is RC-scoped by design — it does
// NOT mirror the full SectionCapacity case set (a redundant parallel discriminant).
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CapacityBuild {
    private CapacityBuild() { }
    public sealed record Hull(DiagramResolution Resolution) : CapacityBuild;   // ultimate N-M-M hull — the eager Steps² solve, refinement riding the arm
    public sealed record Elastic : CapacityBuild;                              // elastic transformed-section SLS + the §6.2 shear screen
}

// The sibling-receipt request [Union] the ONE Lift dispatches (FORM_CHOOSER row 1: a receipt family collapses onto a
// request union + total Switch, never an overload roster) — each case CARRIES its full lift context so the modality
// is recoverable from the request value alone: the steel/timber cases the already-computed family receipt, the
// masonry case its typed CmuStrength row + the (grouted) net ComputedSection + the member slenderness reduction + the
// Table 9.1.9.2 RuptureModulus row with its MortarSystem/MortarType keys (the prior five-parameter overload tail is
// the deleted form). A new family receipt is one case + one Switch arm, never another overload.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CapacityReceipt {
    private CapacityReceipt() { }
    public sealed record Steel(DesignCapacity Capacity) : CapacityReceipt;
    public sealed record Timber(TimberCapacity Capacity) : CapacityReceipt;
    public sealed record Masonry(CmuStrength Strength, ComputedSection Section, MasonryReduction Slenderness, RuptureModulus Rupture, MortarSystem System, MortarType Mortar) : CapacityReceipt;
    // The reinforced case reads the cmu lattice facts the URM case never consumed: the seed row's ReinforcedCells/
    // RebarBarMm steel, the bar grade's yield, and the grouted net section — the TMS 402 §9.3 inputs.
    public sealed record ReinforcedMasonry(CmuStrength Strength, ComputedSection Section, CmuRow Unit, RebarGradeRow Bar, MasonryReduction Slenderness) : CapacityReceipt;
    // The glazing pane resistance lifted WHOLE from glazing#GLAZING_FAMILY GlazingStructural — never re-derived here.
    public sealed record Glass(GlassCapacity Capacity) : CapacityReceipt;
    // The connection receipts — the joint#JOINT_FAMILY line/area/stud design values and the connector#CONNECTOR_FAMILY
    // duration-governed capacity — each case carrying its full lift context (the weld its load angle, the stud its
    // group count), so the load-path verdict rides the SAME Check fold as the member cases.
    public sealed record Weld(JointRow.Weld Row, double LoadAngleDeg) : CapacityReceipt;
    public sealed record Adhesive(JointRow.Adhesive Row) : CapacityReceipt;
    public sealed record Stud(JointRow.Stud Row, int Count) : CapacityReceipt;
    public sealed record Connector(ConnectorCapacity Capacity) : CapacityReceipt;
}

// The verdict axis — which applied action governs the check, a typed bounded vocabulary NEVER a stringly-typed label.
// One canonical term per action (flexure owns every bending-governed verdict — a bending synonym row is the deleted
// form): axial/flexure for the uniaxial and combined-interaction components, biaxial-moment for the RC hull ray, shear
// for a shear-governed check, torsion for the St-Venant demand the steel/timber arms fold against their torsional
// resistance, bearing for the perpendicular support reaction the timber R_90,Rd resists.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GoverningAction {
    public static readonly GoverningAction Axial         = new("axial");
    public static readonly GoverningAction Flexure       = new("flexure");
    public static readonly GoverningAction BiaxialMoment = new("biaxial-moment");
    public static readonly GoverningAction Shear         = new("shear");
    public static readonly GoverningAction Torsion       = new("torsion");
    public static readonly GoverningAction Bearing       = new("bearing");
}

// --- [MODELS] ------------------------------------------------------------------------------
// The applied design action checked against the capacity surface — the full member-action vector in SI engineering
// units (kN, kNm), SIGNED (axial − compression / + tension, moments ± for direction), so the columns are signed
// doubles NOT PositiveMagnitude — yet ADMITTED ONCE (BOUNDARY_ADMISSION): a signed value never licenses NaN/∞, so the
// generated validation owns the all-finite guard and the railed Of lifts a rejected action onto
// ComponentFault.Dimension with typed evidence — no non-finite component reaches Check, and no late per-case
// IsFinite guard exists. N/My/Mz are the RcInteraction hull-ray vector, the RcElastic combined-stress demand, and
// the flexure/axial demands; Vy/Vz the shear demands the SteelLrfd φVn, TimberEc5 V_Rd, RcElastic §6.2, and
// MasonryCompression §9.2.6.1 shear arms fold; Mt the torsion the SteelLrfd §H3.1 / TimberEc5 §6.1.8 torsion arm
// folds against the lifted torsional resistance; Rb the perpendicular support reaction the TimberEc5 R_90,Rd bearing
// arm folds. The biaxial moment magnitude and the shear resultant are derived projections, never re-passed columns.
[ComplexValueObject]
public readonly partial struct Demand {
    public double AxialKn { get; }
    public double MomentYKnm { get; }
    public double MomentZKnm { get; }
    public double ShearYKn { get; }
    public double ShearZKn { get; }
    public double TorsionKnm { get; }
    public double BearingKn { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double axialKn, ref double momentYKnm, ref double momentZKnm,
        ref double shearYKn, ref double shearZKn, ref double torsionKnm, ref double bearingKn) =>
        validationError = double.IsFinite(axialKn) && double.IsFinite(momentYKnm) && double.IsFinite(momentZKnm)
            && double.IsFinite(shearYKn) && double.IsFinite(shearZKn) && double.IsFinite(torsionKnm) && double.IsFinite(bearingKn)
            ? null
            : new ValidationError($"<demand-nonfinite:n={axialKn:R}:my={momentYKnm:R}:mz={momentZKnm:R}:vy={shearYKn:R}:vz={shearZKn:R}:mt={torsionKnm:R}:rb={bearingKn:R}>");

    public static Fin<Demand> Of(double axialKn, double momentYKnm, double momentZKnm, Op key,
        double shearYKn = 0.0, double shearZKn = 0.0, double torsionKnm = 0.0, double bearingKn = 0.0) =>
        Validate(axialKn, momentYKnm, momentZKnm, shearYKn, shearZKn, torsionKnm, bearingKn, out Demand demand) is { } error
            ? Fin.Fail<Demand>(ComponentFault.Dimension(key, error.Message))
            : Fin.Succ(demand);

    public double MomentResultantKnm => Math.Sqrt(MomentYKnm * MomentYKnm + MomentZKnm * MomentZKnm);
    public double ShearResultantKn => Math.Sqrt(ShearYKn * ShearYKn + ShearZKn * ShearZKn);
}

[ValueObject<double>]
public readonly partial struct MasonryReduction {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value is > 0.0 and <= 1.0
            ? null
            : new ValidationError($"<masonry-reduction-invalid:{value:R}>");

    public static Fin<MasonryReduction> Of(double value, Op key) =>
        Validate(value, out MasonryReduction reduction) is { } error
            ? Fin.Fail<MasonryReduction>(ComponentFault.Dimension(key, error.Message))
            : Fin.Succ(reduction);
}

[Union]
public abstract partial record Utilisation {
    private Utilisation(GoverningAction governing) => Governing = governing;
    public GoverningAction Governing { get; }
    public bool Adequate => Switch(
        bounded: static verdict => verdict.Value <= 1.0,
        requiresMemberCheck: static _ => false,
        overcapacity: static _ => false);

    public sealed record Bounded(double Value, GoverningAction Action) : Utilisation(Action);
    public sealed record RequiresMemberCheck(double Value, GoverningAction Action, MemberCheckRequirement Requirement) : Utilisation(Action);
    public sealed record Overcapacity(GoverningAction Action) : Utilisation(Action);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MemberCheckRequirement {
    public static readonly MemberCheckRequirement RcShearReinforcement = new("rc-shear-reinforcement");
}

// One SectionCapacity [Union] closes the structural-capacity family across the realized structural rails AND the
// connection load path — the ultimate N-M-M hull, the elastic transformed RC section, the rolled/composite/cold-formed
// steel LRFD receipt, the EC5 timber design receipt, the TMS 402 URM and §9.3 reinforced masonry checks, the EN 16612
// glass pane, and the weld/adhesive/stud/connector Connection triple — so a member AND its connection are checked
// through one Check fold, never a per-type surface. The non-RC cases lift their family-owner receipts WHOLE (the
// design-code computation stays the sibling page's, the unified verdict this owner's); the RC cases are the Resolve builds.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionCapacity {
    private SectionCapacity() { }

    // The cached ultimate biaxial capacity hull — the IForceMomentMesh held once from the eager InteractionDiagram solve.
    public sealed record RcInteraction(IForceMomentMesh Hull) : SectionCapacity;
    // The elastic transformed-section reinforcement properties (SI scalars read off the ONE ConcreteSectionProperties
    // carrier the RcSection receipt holds) plus the bottom-face EFFECTIVE DEPTH d (the ULS flexural lever to the tension
    // steel, distinct from the SLS extreme-fibre distance), the bottom-face TENSION steel As (the EC2 ρl input) and the
    // two-leg link area Asw (the engine computes 2·A_link — the §6.2.2-vs-§6.2.3(3) branch discriminant), the gross
    // depth/width (the SLS extreme-CONCRETE-fibre levers cy = h/2 / cz = b/2), and the parsed fck with its EC2 flexural
    // tensile limit fctm. TWO inertia pairs, two limit states: GrossInertia is the base SectionProperties polygon
    // integral over the concrete outline — the EC2 7.1 gross-basis SLS fibre DIVISOR; ReinforcementInertia is the
    // Σ(As·d²) steel-only second moment (Rebars.CalculateInertiaYy/Zz) — ~5% of gross, the cracked-section Icr input
    // the forward Compute member check composes with its modular ratio, NEVER the fibre divisor.
    public sealed record RcElastic(
        double TotalReinforcementAreaMm2,
        double TensionSteelAreaMm2,
        double ShearLinkAreaMm2,
        double ConcreteAreaMm2,
        double ReinforcementRatio,
        double GrossInertiaYyMm4,
        double GrossInertiaZzMm4,
        double ReinforcementInertiaYyMm4,
        double ReinforcementInertiaZzMm4,
        double EffectiveDepthMm,
        double DepthMm,
        double WidthMm,
        double FckMpa,
        double FctmMpa) : SectionCapacity;
    // The steel LRFD receipt lifted WHOLE from steel#STEEL_FAMILY DesignCapacity (the SI N·mm/N capacities carried as
    // kN·m/kN here, plus the AISC Table B4.1 CompactnessClass and the column slenderness λ) — never re-derived here.
    // FlexuralMinorKnm is the §F6 weak-axis φMny = φb·min(Fy·Zy, 1.6·Fy·Sy) (1.5 cap on the F10 single-angle regime),
    // F6.2-bounded for the F2 flange classes and Seff-scaled on the cold-formed arm — the per-axis H1.1 divisor beside
    // FlexuralKnm. TorsionalKnm is the AISC 360 §H3.1 design torsional resistance φTn = φT·Fcr·C (C the HSS torsional
    // constant J/c the steel owner derives) the steel DesignCapacity.TorsionalNmm column carries — positive for a
    // CLOSED HSS/pipe, 0 for an OPEN thin-walled shape whose §H3.3 warping torsion is not a single resistance, so an
    // open-shape torsion demand surfaces as the governing over-ratio (the consumed-action discipline), never a
    // silently-ignored 0 column.
    public sealed record SteelLrfd(
        double FlexuralKnm,
        double FlexuralMinorKnm,
        double CompressionKn,
        double ShearKn,
        double TorsionalKnm,
        CompactnessClass Classification,
        double Slenderness) : SectionCapacity;
    // The EN 1995-1-1 timber design receipt lifted WHOLE from timber#TIMBER_CAPACITY TimberCapacity (the M_Rd/N_Rd/V_Rd/
    // R_90,Rd design resistances + the relative slenderness λ_rel + the k_mod service×duration factor) — never re-derived.
    // BendingMinorKnm is the member M_Rd,z = k_h(w)·k_mod·f_m,k·S_y/γ_M (no k_crit — no LTB about the minor axis) and
    // 0.0 on the research-gated panel arm (in-plane CLT bending verification unsettled — the GuardedRatio fold makes a
    // panel Mz demand govern loud, never pass silent); Km the §6.1.6(2) per-form stress-redistribution weight the
    // biaxial fold swaps. TorsionalKnm is the EN 1995-1-1 §6.1.8 torsional resistance T_Rd = k_shape·f_v,d·W_tor the
    // timber owner derives over the rectangular section (the TimberCapacity.TorsionalNmm column) — positive for every
    // realized timber section, so a torsion-loaded glulam member folds demand.TorsionKnm against a real resistance,
    // never an inert 0.
    public sealed record TimberEc5(
        double BendingKnm,
        double BendingMinorKnm,
        double CompressionKn,
        double ShearKn,
        double BearingPerpKn,
        double TorsionalKnm,
        double RelativeSlenderness,
        double Km,
        double Kmod) : SectionCapacity;
    // The TMS 402 masonry compression case: the cmu#CMU_FAMILY CmuStrength specified strength f'm + the (grouted) net
    // ComputedSection facts the shared SectionSolver.Solve computes over the cmu SectionProfile.CellularRectangle (the
    // as-built net, VoidCell.Grouted cells filled) — net area AND BOTH net elastic moduli (SxMm3/SyMm3, so a pier bent
    // about both axes folds each moment against ITS modulus, never a resultant against the major alone) — + the
    // slenderness reduction (the TMS 402 member-stability bracket: [1 - (h/140r)²] at h/r <= 99, (70r/h)² above — a
    // placement-level caller input, either branch) the unity check scales. FrMpa is the Table 9.1.9.2 modulus of
    // rupture the Lift resolves off the masonry#MASONRY_FAMILY RuptureModulus row for the member's bed-joint tension
    // direction — the tension-fibre complement to the 0.80·f'm compression fibre (~0.2-2.3 MPa vs ~11 MPa design
    // stress), the governing axis of every low-axial URM wall.
    public sealed record MasonryCompression(
        double FmMpa,
        double NetAreaMm2,
        double SectionModulusXMm3,
        double SectionModulusYMm3,
        double SlendernessReduction,
        double FrMpa) : SectionCapacity;
    // The TMS 402 §9.3 REINFORCED masonry case over the cmu lattice facts: f'm, the bar-grade yield, the reinforced-cell
    // steel area, the grouted net area, the out-of-plane lever d (mid-wall bars: W/2), the per-unit bed length b, and the
    // member slenderness reduction — the steel-couple flexural arm plus the reinforced axial the URM case's no-steel-term
    // admission law reserved for exactly this case.
    public sealed record MasonryReinforced(
        double FmMpa,
        double FyMpa,
        double SteelAreaMm2,
        double NetAreaMm2,
        double EffectiveDepthMm,
        double BedLengthMm,
        double SlendernessReduction) : SectionCapacity;
    // The EN 16612 glazing pane resistance lifted WHOLE from glazing#GLAZING_FAMILY GlassCapacity: the governing pane's
    // per-metre-strip design moment, its design bending strength, and the effective laminate thickness the report reads.
    public sealed record GlassPane(
        double BendingKnmPerM,
        double ResistanceMpa,
        double EffectiveThicknessMm) : SectionCapacity;
    // The connection load-path case: the lifted line/area/group shear, the tension (uplift) column, and the seat-bearing
    // (download) column — a 0 column is an unresisted axis the GuardedRatio fold makes govern loud, so one case carries
    // the weld, adhesive, stud-group, and connector receipts without per-kind capacity surfaces.
    public sealed record Connection(
        double ShearKn,
        double TensionKn,
        double BearingKn) : SectionCapacity;

    // The demand-vs-capacity verdict, one polymorphic Check over the closed family — never per-type. The RcInteraction
    // arm ray-casts the demand against the hull; the RcElastic arm the WORST of the EC2 SLS combined
    // extreme-CONCRETE-fibre cracking stress and the EC2 §6.2 shear screen; the SteelLrfd arm the AISC 360 §H1.1
    // per-axis biaxial interaction worst-folded with shear and §H3.1 torsion; the TimberEc5 arm the EN 1995-1-1
    // §6.3.2/§6.2.4 km-swapped biaxial pair worst-folded with shear, §6.1.8 torsion, and §6.1.5 bearing; the
    // MasonryCompression arm the TMS 402 biaxial unity sum worst-folded with the §9.2.2 flexural-tension screen and
    // the §9.2.6.1 URM shear screen; the MasonryReinforced arm the §9.3 steel-couple unity sum with the §9.3.4.1.2
    // masonry shear screen; the GlassPane arm the EN 16612 per-metre plate-bending fold; the Connection arm the
    // shear/tension/bearing load-path triple. Every arm is TOTAL over the seven Demand columns: an action the case's capacity
    // surface does not resist folds through GuardedRatio against 0 and governs loud — a hull shear, an RC torsion, a
    // steel bearing, a masonry torsion demand can never pass silent (the consumed-action discipline).
    public Utilisation Check(Demand demand) => Switch(
        rcInteraction: h => Cast(h.Hull, demand),
        rcElastic: e => RcElasticUtilisation(e, demand),
        steelLrfd: s => SteelUtilisation(s, demand),
        timberEc5: t => TimberUtilisation(t, demand),
        masonryCompression: m => MasonryUtilisation(m, demand),
        masonryReinforced: m => MasonryReinforcedUtilisation(m, demand),
        glassPane: g => GlassUtilisation(g, demand),
        connection: c => ConnectionUtilisation(c, demand));

    // One RC elastic arm, two limit-state ratios: the SLS cracking fibre stress and the ULS shear screen fold through
    // the same Worst governing-axis law every other arm drives — never a second RC surface for the shear check.
    // EXPRESSION_SPINE measured-kernel exemption: the intermediate scalar bindings feed one closed Worst fold.
    static Utilisation RcElasticUtilisation(RcElastic e, Demand demand) {
        (double cracking, GoverningAction axis) = Cracking(e, demand);
        double shear = demand.ShearResultantKn / Math.Max(ShearResistanceKn(e), double.Epsilon);
        Utilisation section = Worst((cracking, axis), (shear, GoverningAction.Shear),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing));
        return e.ShearLinkAreaMm2 > 0.0 && section is Utilisation.Bounded { Value: <= 1.0 } bounded
            ? new Utilisation.RequiresMemberCheck(bounded.Value, bounded.Governing, MemberCheckRequirement.RcShearReinforcement)
            : section;
    }

    // EC2 SLS cracking: the MAXIMUM-tensile extreme-CONCRETE-fibre transformed stress against fctm — the FULL combined
    // action σ = N/A ± My·cy/Iyy ± Mz·cz/Izz, never the major-axis-bending-only slice. A SIGNED axial N/A (the Demand
    // axial convention: − compression, + tension) so a compressive service axial DELAYS cracking (the physically-correct
    // SLS behaviour, not a |N| that over-predicts cracking under compression); BOTH bending axes add their
    // tension-side fibre stress (|My|·cy/Iyy + |Mz|·cz/Izz with cy = h/2, cz = b/2 the gross half-depths — NOT the
    // effective depth d, the ULS lever to the tension STEEL the record carries for the bridge readout). The divisor is
    // the GROSS section inertia (the EC2 7.1 gross-basis SLS) — dividing by the Σ(As·d²) reinforcement-only column
    // inflates the fibre stress ~20× and falsely cracks every service state. σ_max/fctm > 1 ⇒ the section cracks; the
    // governing axis the larger bending contribution (or Axial when N/A dominates a near-zero-moment service state).
    static (double Ratio, GoverningAction Governing) Cracking(RcElastic e, Demand demand) {
        double axialStress = demand.AxialKn * 1e3 / Math.Max(e.ConcreteAreaMm2, double.Epsilon);                  // signed N/A (MPa)
        double bendingYStress = Math.Abs(demand.MomentYKnm) * 1e6 * (e.DepthMm * 0.5) / Math.Max(e.GrossInertiaYyMm4, double.Epsilon);
        double bendingZStress = Math.Abs(demand.MomentZKnm) * 1e6 * (e.WidthMm * 0.5) / Math.Max(e.GrossInertiaZzMm4, double.Epsilon);
        double tensileStress = axialStress + bendingYStress + bendingZStress;                                     // max tensile fibre (MPa)
        GoverningAction governing = Math.Max(bendingYStress, bendingZStress) >= Math.Abs(axialStress)
            ? GoverningAction.Flexure : GoverningAction.Axial;   // either bending axis dominating is a FLEXURE verdict — biaxial-moment names only the hull ray
        return (tensileStress / Math.Max(e.FctmMpa, double.Epsilon), governing);
    }

    // EC2 §6.2 section-level shear: a LINKLESS section resists V_Rd,c (§6.2.2 — C_Rd,c·k·(100·ρl·fck)^(1/3)·bw·d with
    // C_Rd,c = 0.18/1.5, k = 1+√(200/d) ≤ 2, ρl = As,tension/(bw·d) ≤ 0.02, floored at v_min = 0.035·k^1.5·√fck); a
    // LINKED section (Asw > 0) is section-decidable only at the §6.2.3(3) web-crushing ceiling V_Rd,max =
    // bw·0.9d·0.6(1−fck/250)·(fck/1.5)/(cotθ+tanθ) at cotθ = 2.5 — the member V_Rd,s = (Asw/s)·z·f_ywd·cotθ needs the
    // stirrup SPACING the RcSection does not carry, so a linked pass DEFERS detailing to the forward Compute member
    // check reading the carried Asw, and a linked fail refutes the section outright (no spacing can cure crushing).
    static double ShearResistanceKn(RcElastic e) {
        double d = Math.Max(e.EffectiveDepthMm, 1.0), bw = Math.Max(e.WidthMm, 1.0);
        double k = Math.Min(1.0 + Math.Sqrt(200.0 / d), 2.0);
        double rho = Math.Min(e.TensionSteelAreaMm2 / (bw * d), 0.02);
        double vrdc = Math.Max(0.12 * k * Math.Cbrt(100.0 * rho * e.FckMpa), 0.035 * Math.Pow(k, 1.5) * Math.Sqrt(e.FckMpa)) * bw * d * 1e-3;
        double vrdmax = bw * 0.9 * d * 0.6 * (1.0 - e.FckMpa / 250.0) * (e.FckMpa / 1.5) / (2.5 + 0.4) * 1e-3;
        return e.ShearLinkAreaMm2 > 0.0 ? vrdmax : vrdc;
    }

    // AISC 360 §H1.1 combined axial-flexure: p + 8/9·m at p >= 0.2 (H1-1a), p/2 + m below (H1-1b) — the COMBINED
    // interaction a max-of-independents under-predicts (p = m = 0.9 passes a max fold yet fails H1.1 at 1.7). m is the
    // PER-AXIS two-term sum Mry/Mcx + Mrz/Mcy of the H1.1 biaxial form — the moment resultant folded against the
    // major-axis φMn alone is the DELETED unconservative spelling (it credited a weak-axis moment the full φMnx/φMny
    // ratio, 3-10x on an I-shape). The combined ratio worst-folds with the §G shear and §H3.1 torsion ratios; the
    // CompactnessClass rides the carrier for the design report. Torsion folds demand.TorsionKnm against the lifted φTn
    // (0.0 ⇒ a zero-torsion demand stays 0, a nonzero torsion demand against an unbounded φTn surfaces as the
    // governing over-ratio).
    static Utilisation SteelUtilisation(SteelLrfd s, Demand demand) {
        double p = Math.Abs(demand.AxialKn) / Math.Max(s.CompressionKn, double.Epsilon);
        double m = Math.Abs(demand.MomentYKnm) / Math.Max(s.FlexuralKnm, double.Epsilon)
            + Math.Abs(demand.MomentZKnm) / Math.Max(s.FlexuralMinorKnm, double.Epsilon);
        double combined = p >= 0.2 ? p + 8.0 / 9.0 * m : p / 2.0 + m;
        double shear = demand.ShearResultantKn / Math.Max(s.ShearKn, double.Epsilon);
        return Worst((combined, p >= m ? GoverningAction.Axial : GoverningAction.Flexure),
            (shear, GoverningAction.Shear), (GuardedRatio(demand.TorsionKnm, s.TorsionalKnm), GoverningAction.Torsion),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing));
    }

    // EN 1995-1-1 combined axial-bending, the km-swapped two-equation MAX pair: axialTerm + my + km·mz vs
    // axialTerm + km·my + mz — §6.3.2 eq 6.23/6.24 with the LINEAR axial term when buckling governs (λ_rel > 0.3,
    // N_Rd already k_c-reduced), §6.2.4 eq 6.19/6.20 with the QUADRATIC n² for the stocky member; km the lifted
    // §6.1.6(2) per-form weight. my/mz ride GuardedRatio so the research-gated panel BendingMinorKnm = 0 makes an
    // in-plane Mz demand govern loud (never a silent pass) while a zero Mz stays inert; the moment resultant folded
    // against the major M_Rd alone is the DELETED unconservative spelling. Worst-folded with the shear, §6.1.8
    // torsion, and §6.1.5 bearing ratios (BearingKn folds against the lifted R_90,Rd — a consumed action, never a
    // carried-but-ignored capacity column).
    static Utilisation TimberUtilisation(TimberEc5 t, Demand demand) {
        double n = Math.Abs(demand.AxialKn) / Math.Max(t.CompressionKn, double.Epsilon);
        double my = GuardedRatio(demand.MomentYKnm, t.BendingKnm);
        double mz = GuardedRatio(demand.MomentZKnm, t.BendingMinorKnm);
        double axialTerm = t.RelativeSlenderness > 0.3 ? n : n * n;
        double combined = Math.Max(axialTerm + my + t.Km * mz, axialTerm + t.Km * my + mz);
        double shear = demand.ShearResultantKn / Math.Max(t.ShearKn, double.Epsilon);
        return Worst((combined, n >= my + mz ? GoverningAction.Axial : GoverningAction.Flexure),
            (shear, GoverningAction.Shear), (GuardedRatio(demand.TorsionKnm, t.TorsionalKnm), GoverningAction.Torsion),
            (GuardedRatio(demand.BearingKn, t.BearingPerpKn), GoverningAction.Bearing));
    }

    // The zero-demand-inert ratio every arm's unresisted-action candidates share: a zero demand is trivially 0
    // (so an unbounded 0-capacity column never spuriously governs an unloaded member), a nonzero demand divides by the
    // lifted capacity — so a loaded member whose capacity surface has not bounded the resistance (the open-shape
    // φTn = 0, the research-gated CLT panel BendingMinorKnm = 0, the hull's absent shear axis) surfaces as the
    // governing over-ratio rather than silently passing, making every Demand column a consumed action on every case.
    static double GuardedRatio(double demand, double capacity) =>
        Math.Abs(demand) <= double.Epsilon ? 0.0 : Math.Abs(demand) / Math.Max(capacity, double.Epsilon);

    // TMS 402 URM strength design, the uncracked-section pair: the compression fibre holds ≤ 0.80·f'm AND the tension
    // fibre holds ≤ fr (§9.2.2 over the Table 9.1.9.2 modulus of rupture). §9.1.4 UNREINFORCED φ = 0.60 for flexure +
    // axial (the reinforced 0.90 on a steel-less Pn was the deleted unconservative form), the §9.2 slenderness-reduced
    // compression φPn = 0.80·φ·0.80·f'm·An·R, and the per-axis flexural capacities φMnx = φ·0.80·f'm·Sx / φMny =
    // φ·0.80·f'm·Sy — the 0.80 stress-block cap (the maximum masonry compressive stress is 0.80·f'm, reinforced and
    // unreinforced alike; a full-f'm fibre over-prices flexure 25%). The unity SUM P/φPn + |My|/φMnx + |Mz|/φMny <= 1
    // folds the COMBINED biaxial action, never the max of independent ratios and never a moment resultant against the
    // major modulus alone; a net-TENSION axial governs outright (§9.2.5 — URM axial tensile strength is neglected).
    // The §9.2.2 flexural-tension screen σt = |My|/Sx + |Mz|/Sy + N/A (MPa, the SIGNED Demand axial — compression
    // RELIEVES tension per Mu/S − Pu/A) folds against φ·fr: the compression-fibre-only fold was the DELETED form
    // (~19x moment over-prediction on a low-axial ungrouted wall — φ·0.80·f'm·S prices ~6.6 MPa where the tension
    // fibre cracks at φ·fr ~ 0.13-0.67); fr = 0 (StackOther, Type O/K mortar) with net tension governs outright,
    // code-faithful. The shear screen is the FULL §9.2.6.1 three-arm minimum (φv = 0.80): 0.315·√f'm·Anv (the
    // 3.8·√f'm psi arm), the 2.07 MPa (300 psi) ceiling, and the running-bond not-solidly-grouted arm
    // 0.386·Anv + 0.45·Nu (56 psi plus the factored-compression benefit — the conservative floor for the
    // solidly-grouted 0.621 arm; a stack-bond pier's 0.158·Anv arm is the bond-axis growth case) — the low-axial wall
    // the two-arm min over-predicted ~3x now prices its bond arm.
    // EXPRESSION_SPINE measured-kernel exemption: the TMS 402 code constants and arm scalars bind once, one Worst fold exits.
    static Utilisation MasonryUtilisation(MasonryCompression m, Demand demand) {
        const double phi = 0.60, phiV = 0.80;
        double pn = 0.80 * phi * 0.80 * m.FmMpa * m.NetAreaMm2 * m.SlendernessReduction * 1e-3;
        double axial = demand.AxialKn > 0.0
            ? GuardedRatio(demand.AxialKn, 0.0)
            : Math.Abs(demand.AxialKn) / Math.Max(pn, double.Epsilon);
        double flexure = Math.Abs(demand.MomentYKnm) / Math.Max(phi * 0.80 * m.FmMpa * m.SectionModulusXMm3 * 1e-6, double.Epsilon)
            + Math.Abs(demand.MomentZKnm) / Math.Max(phi * 0.80 * m.FmMpa * m.SectionModulusYMm3 * 1e-6, double.Epsilon);
        double sigmaT = Math.Abs(demand.MomentYKnm) * 1e6 / Math.Max(m.SectionModulusXMm3, double.Epsilon)
            + Math.Abs(demand.MomentZKnm) * 1e6 / Math.Max(m.SectionModulusYMm3, double.Epsilon)
            + demand.AxialKn * 1e3 / Math.Max(m.NetAreaMm2, double.Epsilon);
        double tension = sigmaT <= 0.0 ? 0.0 : sigmaT / Math.Max(phi * m.FrMpa, double.Epsilon);
        double vnKn = Math.Min(Math.Min(0.315 * Math.Sqrt(m.FmMpa), 2.07) * m.NetAreaMm2 * 1e-3,
            0.386 * m.NetAreaMm2 * 1e-3 + 0.45 * Math.Max(0.0, -demand.AxialKn));
        double shear = demand.ShearResultantKn / Math.Max(phiV * vnKn, double.Epsilon);
        return Worst((axial + flexure, axial >= flexure ? GoverningAction.Axial : GoverningAction.Flexure),
            (tension, GoverningAction.Flexure),
            (shear, GoverningAction.Shear),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing));
    }

    // TMS 402 §9.3 reinforced masonry (φ = 0.90 flexure/axial, φv = 0.80 shear): the §9.3.4.1.1 reinforced axial
    // Pn = 0.80·[0.80·f'm·(An − As) + fy·As]·R, the §9.3.5 steel-couple flexure Mn = As·fy·(d − a/2) over the
    // a = As·fy/(0.80·f'm·b) stress block about the out-of-plane bed axis, a NET-TENSION axial resisted by the steel
    // alone (φ·As·fy — the URM tension-governs-outright arm retires for the reinforced state), and the §9.3.4.1.2
    // masonry shear screen Vnm = 0.083·(4 − 1.75·min(M/(V·dv), 1))·Anv·√f'm pinned at the M/(V·dv) = 1 conservative
    // bound — the reinforcement shear term Vns needs the bar SPACING the section does not carry, so shear detailing
    // stays the forward member check's. An in-plane Mz demand folds GuardedRatio-against-0 loud: bar STATIONS along
    // the bed length are lattice member facts, never section columns.
    // EXPRESSION_SPINE measured-kernel exemption: the code constants and arm scalars bind once, one Worst fold exits.
    static Utilisation MasonryReinforcedUtilisation(MasonryReinforced m, Demand demand) {
        const double phi = 0.90, phiV = 0.80;
        double pn = 0.80 * (0.80 * m.FmMpa * Math.Max(m.NetAreaMm2 - m.SteelAreaMm2, 0.0) + m.FyMpa * m.SteelAreaMm2) * m.SlendernessReduction * 1e-3;
        double block = m.SteelAreaMm2 * m.FyMpa / Math.Max(0.80 * m.FmMpa * m.BedLengthMm, double.Epsilon);
        double mn = m.SteelAreaMm2 * m.FyMpa * Math.Max(m.EffectiveDepthMm - block / 2.0, 0.0) * 1e-6;
        double axial = demand.AxialKn > 0.0
            ? demand.AxialKn / Math.Max(phi * m.SteelAreaMm2 * m.FyMpa * 1e-3, double.Epsilon)
            : Math.Abs(demand.AxialKn) / Math.Max(phi * pn, double.Epsilon);
        double flexure = Math.Abs(demand.MomentYKnm) / Math.Max(phi * mn, double.Epsilon);
        double vnm = 0.083 * 2.25 * m.NetAreaMm2 * Math.Sqrt(m.FmMpa) * 1e-3;
        return Worst((axial + flexure, axial >= flexure ? GoverningAction.Axial : GoverningAction.Flexure),
            (GuardedRatio(demand.MomentZKnm, 0.0), GoverningAction.Flexure),
            (demand.ShearResultantKn / Math.Max(phiV * vnm, double.Epsilon), GoverningAction.Shear),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing));
    }

    // EN 16612 pane check per metre strip: BOTH plate bending directions fold against the SAME isotropic per-metre
    // resistance, their SUM the conservative combined-stress bound; in-plane axial, shear, torsion, and bearing are
    // unresisted at pane altitude and govern loud through GuardedRatio.
    static Utilisation GlassUtilisation(GlassPane g, Demand demand) =>
        Worst(((Math.Abs(demand.MomentYKnm) + Math.Abs(demand.MomentZKnm)) / Math.Max(g.BendingKnmPerM, double.Epsilon), GoverningAction.Flexure),
            (GuardedRatio(demand.AxialKn, 0.0), GoverningAction.Axial),
            (GuardedRatio(demand.ShearResultantKn, 0.0), GoverningAction.Shear),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing));

    // The connection verdict over the load path's three resisted axes: the shear resultant against the lifted line/
    // area/group shear, a POSITIVE axial (tension, uplift) against the tension column, and the seat reaction (a
    // hanger's download) against the bearing column; a compressive axial rides the member, and moments/torsion are
    // unresisted at connection altitude and govern loud — the connector's private DemandRatio mini-rail collapses
    // onto this one fold, its direction vocabulary living on in the lift columns.
    static Utilisation ConnectionUtilisation(Connection c, Demand demand) =>
        Worst((GuardedRatio(demand.ShearResultantKn, c.ShearKn), GoverningAction.Shear),
            (GuardedRatio(Math.Max(demand.AxialKn, 0.0), c.TensionKn), GoverningAction.Axial),
            (GuardedRatio(demand.BearingKn, c.BearingKn), GoverningAction.Bearing),
            (GuardedRatio(demand.MomentResultantKnm, 0.0), GoverningAction.Flexure),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion));

    // The worst (largest) ratio over the candidate span — the unified governing-axis fold every arm drives, so a
    // steel/timber/masonry check reports WHICH action governs, not just a ratio; the span-params buffer stack-allocates
    // per Check, and the strict-greater fold keeps the earliest-maximal tie-break without a per-call array.
    static Utilisation Worst(params ReadOnlySpan<(double Ratio, GoverningAction Action)> candidates) {
        (double ratio, GoverningAction action) = Iterable<(double Ratio, GoverningAction Action)>.FromSpan(candidates[1..])
            .Fold(candidates[0], static (best, next) => next.Ratio > best.Ratio ? next : best);
        return double.IsFinite(ratio)
            ? new Utilisation.Bounded(ratio, action)
            : new Utilisation.Overcapacity(action);
    }

    // --- [BOUNDARIES]
    // The ONE RC capacity boundary: dispatch the build request onto its VividOrange solver over the RcSection's
    // IConcreteSection, admit the eager solve ONCE, coerce the UnitsNet outputs to SI scalars at the edge, trap every
    // VividOrange throw onto ComponentFault.Capacity (the component-sub-domain band 2300 capacity-solve slot, distinct
    // from the Section elastic-integral slot — a capacity telemetry reader bands a design-solve fault apart from a section
    // fault). The InteractionDiagram ctor IS the expensive solve — cached on the RcInteraction carrier, never re-solved.
    public static Fin<SectionCapacity> Resolve(RcSection rc, CapacityBuild build, Op key) =>
        build.Switch(
            hull: hull => Try.lift(() => new ForceMomentEngine(rc.Section, hull.Resolution.ToSettings()).Mesh).Run()
                .MapFail(e => (Error)ComponentFault.Capacity(key, $"<rc-interaction-solve:{e.Message}>"))
                .Map(mesh => (SectionCapacity)new RcInteraction(mesh)),
            // The ONE ConcreteSectionProperties carrier rides the RcSection receipt (constructed and eager-forced at
            // RcSectionBuilder.Of — a second `new ConcreteSectionProperties(rc.Section)` here is the deleted
            // re-admission). The two face queries are Option-typed AT the RcSection seam (the receipt already traps the
            // engine's no-bottom-bar EffectiveDepth divide) — absence lifts onto the rail as a typed fault, because an
            // elastic build without a bottom tension chord has no ρl and no lever; the remaining lazy gross-integral
            // reads trap in ONE lift so no VividOrange throw escapes the boundary.
            elastic: _ =>
                from d in rc.EffectiveDepthMm(SectionFace.Bottom).ToFin(ComponentFault.Capacity(key, "<rc-elastic-no-bottom-tension-chord>"))
                from asTension in rc.FaceSteelAreaMm2(SectionFace.Bottom).ToFin(ComponentFault.Capacity(key, "<rc-elastic-no-bottom-tension-chord>"))
                from built in Try.lift(() => {
                    double fck = EnConcreteFactory.CreateLinearElastic(rc.Concrete.Grade).Strength.Megapascals;
                    return (SectionCapacity)new RcElastic(
                        rc.GrossSteelAreaMm2,
                        asTension,                                                 // tension steel As — the EC2 ρl input
                        rc.ShearLinkAreaMm2,                                       // two-leg link area Asw (engine: 2·A_link)
                        rc.ConcreteAreaMm2,
                        rc.ReinforcementRatio,
                        rc.Properties.MomentOfInertiaYy.MillimetersToTheFourth,    // GROSS uncracked inertia — the SLS fibre divisor
                        rc.Properties.MomentOfInertiaZz.MillimetersToTheFourth,
                        rc.ReinforcementInertiaYyMm4,                              // Σ(As·d²) steel moments — the cracked-Icr readout
                        rc.ReinforcementInertiaZzMm4,
                        d,                                                         // the ULS flexural lever d to the tension steel
                        rc.ConcreteProfile.GrossRectangleMm.DepthMm.Value,         // gross depth h — the major-axis fibre lever cy = h/2
                        rc.ConcreteProfile.GrossRectangleMm.WidthMm.Value,         // gross width b — the minor-axis fibre lever cz = b/2
                        fck,
                        Fctm(fck));
                }).Run().MapFail(e => (Error)ComponentFault.Capacity(key, $"<rc-elastic-solve:{e.Message}>"))
                select built);

    // The ONE sibling-receipt lift — one canonical name over the CapacityReceipt request union, the case the modality
    // discriminant (never a per-family SteelLrfd/TimberEc5/MasonryCompression factory roster and never an overload
    // set). Each case carries an already-computed family-owner receipt WHOLE into the rail as kN·m/kN with no
    // re-derivation: the steel DesignCapacity (N·mm/N major + §F6 minor flexure + CompactnessClass + slenderness +
    // the AISC §H3.1 TorsionalNmm — positive for a CLOSED HSS, 0 for an OPEN warping-torsion shape); the timber
    // TimberCapacity (major + minor design resistances + λ_rel + the §6.1.6(2) Km + k_mod + the §6.1.8 TorsionalNmm);
    // the masonry case f'm read off the TYPED CmuStrength row and fr minted on the typed masonry#MASONRY_FAMILY
    // RuptureModulus.FrMpa(system, mortar) — never bare caller doubles. Direction is a lift-time key: a vertical
    // member's bed-plane section stresses normal-to-bed on BOTH moment axes (a normal row); a horizontally-spanning
    // strip lifts a parallel row over its vertical-cut section; a stack-bond pier its stack row; the
    // partially-grouted normal-direction wall composes RuptureModulus.PartialGrout(CmuPhysics.GroutedCellFraction,
    // system, mortar) with direct case construction — the TMS footnote's one sanctioned bypass. The reinforced-masonry
    // case computes As off the lattice facts and takes the mid-wall d = W/2 / bed-length b levers; the glass case reads
    // the GlazingStructural receipt whole; the connection cases collapse the weld line (its AISC J2-5 directional
    // factor applied at lift), the adhesive lap, the stud group, and the connector's duration-governed columns onto the
    // ONE Connection triple. Every capacity column reads DIRECTLY off its receipt or typed row — ONE source, no
    // redundant parallel lift parameter.
    public static SectionCapacity Lift(CapacityReceipt receipt) => receipt.Switch(
        steel: static r => (SectionCapacity)new SteelLrfd(
            r.Capacity.FlexuralNmm * 1e-6, r.Capacity.FlexuralMinorNmm * 1e-6, r.Capacity.CompressionN * 1e-3,
            r.Capacity.ShearN * 1e-3, r.Capacity.TorsionalNmm * 1e-6, r.Capacity.Classification, r.Capacity.Slenderness),
        timber: static r => new TimberEc5(
            r.Capacity.BendingNmm * 1e-6, r.Capacity.BendingMinorNmm * 1e-6, r.Capacity.CompressionN * 1e-3,
            r.Capacity.ShearN * 1e-3, r.Capacity.BearingPerpN * 1e-3, r.Capacity.TorsionalNmm * 1e-6,
            r.Capacity.RelativeSlenderness, r.Capacity.Km, r.Capacity.Kmod),
        masonry: static r => new MasonryCompression(
            r.Strength.FmMpa, r.Section.AreaMm2.Value, r.Section.SxMm3.Value, r.Section.SyMm3.Value,
            r.Slenderness.Value, r.Rupture.FrMpa(r.System, r.Mortar)),
        reinforcedMasonry: static r => new MasonryReinforced(
            r.Strength.FmMpa, r.Bar.MinimumYieldMpa,
            r.Unit.ReinforcedCells * Math.PI / 4.0 * r.Unit.RebarBarMm * r.Unit.RebarBarMm,   // As off the lattice facts
            r.Section.AreaMm2.Value, r.Unit.WMm / 2.0, r.Unit.LMm, r.Slenderness.Value),      // d = W/2 mid-wall bars, b the bed length
        glass: static r => new GlassPane(r.Capacity.BendingKnmPerM, r.Capacity.ResistanceMpa, r.Capacity.EffectiveThicknessMm),
        weld: static r => new Connection(r.Row.DirectionalShearKn(Angle.FromDegrees(r.LoadAngleDeg)), 0.0, 0.0),
        adhesive: static r => new Connection(r.Row.DesignShearKn, 0.0, 0.0),
        stud: static r => new Connection(Math.Max(r.Count, 0) * r.Row.DesignShearKn, 0.0, 0.0),
        // The seat-borne download is Cd-exempt (steel-governed bearing); the fastener-transferred uplift/lateral
        // columns scale by the connector's own admitted duration factor.
        connector: static r => new Connection(r.Capacity.LateralKn * r.Capacity.Cd, r.Capacity.UpliftKn * r.Capacity.Cd, r.Capacity.DownloadKn));

    // The C#-internal hull cache, realized: Freeze writes the eagerly-solved mesh through the ITaxonomySerializable
    // marker IForceMomentMesh itself extends ($type-tagged Newtonsoft wire, UnitsNet SI-scalar+unit quantities); Thaw
    // rehydrates the exact ForceMomentMesh via the $type tag WITHOUT re-running the Steps² sweep, trapping the
    // FromJson null/throw onto ComponentFault.Capacity. Producer=consumer ONLY — TypeNameHandling.Objects is a
    // deserialization-gadget surface, so Thaw is fed exclusively JSON a trusted Freeze minted, never a peer document.
    internal static string Freeze(RcInteraction capacity) => capacity.Hull.ToJson();

    internal static Fin<SectionCapacity> Thaw(string json, Op key) =>
        Try.lift(() => json.FromJson<IForceMomentMesh>()).Run()
            .MapFail(e => (Error)ComponentFault.Capacity(key, $"<hull-thaw:{e.Message}>"))
            .Bind(mesh => mesh is null
                ? Fin.Fail<SectionCapacity>(ComponentFault.Capacity(key, "<hull-thaw:null-document>"))
                : Fin.Succ((SectionCapacity)new RcInteraction(mesh)));

    // EC2 mean flexural tensile strength from fck: fctm = 0.30·fck^(2/3) for ≤C50, 2.12·ln(1+(fck+8)/10) above —
    // the cracking-stress reference the RcElastic service check compares the transformed extreme-fibre stress against.
    // The fck source is EnConcreteFactory.CreateLinearElastic(grade).Strength — verified: Strength IS the parsed
    // characteristic cylinder strength fck (the first Cxx token), not the design fcd (.api/api-vividorange-materials.md).
    static double Fctm(double fckMpa) =>
        fckMpa <= 50.0 ? 0.30 * Math.Pow(fckMpa, 2.0 / 3.0) : 2.12 * Math.Log(1.0 + (fckMpa + 8.0) / 10.0);

    // The hull carries N-M-M resistance ONLY, so the ray verdict worst-folds with the shear/torsion/bearing demands
    // against a 0 capacity column (GuardedRatio) — a Demand whose only load is a shear on an RcInteraction capacity
    // governs LOUD as an unresisted action, never a silent Bounded(0) pass (the consumed-action discipline).
    static Utilisation Cast(IForceMomentMesh hull, Demand demand) {
        GoverningAction governing = demand.MomentResultantKnm > double.Epsilon
            ? GoverningAction.BiaxialMoment
            : GoverningAction.Axial;
        double ray = Math.Abs(demand.AxialKn) <= double.Epsilon && demand.MomentResultantKnm <= double.Epsilon
            ? 0.0
            : toSeq(hull.Faces)
                .Map(face => Pierce(face, demand.AxialKn, demand.MomentYKnm, demand.MomentZKnm))
                .Somes()
                .Filter(static multiplier => multiplier > 0.0)
                .OrderBy(static multiplier => multiplier)
                .HeadOrNone()
                .Match(Some: static multiplier => 1.0 / multiplier, None: static () => double.PositiveInfinity);
        return Worst((ray, governing),
            (GuardedRatio(demand.ShearResultantKn, 0.0), GoverningAction.Shear),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing));
    }

    // EXPRESSION_SPINE measured-kernel exemption: Möller-Trumbore ray-triangle scalar kernel — span-free numeric
    // intermediates with early degenerate exits, the bounded kernel role the doctrine names for statement bodies.
    static Option<double> Pierce(IForceMomentTriFace face, double dN, double dMy, double dMz) {
        (double ax, double ay, double az) = Coord(face.A);
        (double e1x, double e1y, double e1z) = Sub(Coord(face.B), (ax, ay, az));
        (double e2x, double e2y, double e2z) = Sub(Coord(face.C), (ax, ay, az));
        (double px, double py, double pz) = Cross((dN, dMy, dMz), (e2x, e2y, e2z));
        double determinant = e1x * px + e1y * py + e1z * pz;
        double edgeNormSquared = e1x * e1x + e1y * e1y + e1z * e1z;
        double crossNormSquared = px * px + py * py + pz * pz;
        double determinantTolerance = 1e-12 * Math.Sqrt(edgeNormSquared * crossNormSquared);
        if (Math.Abs(determinant) <= determinantTolerance) return None;
        double inverse = 1.0 / determinant;
        double u = -(ax * px + ay * py + az * pz) * inverse;
        if (u is < 0.0 or > 1.0) return None;
        (double qx, double qy, double qz) = Cross((-ax, -ay, -az), (e1x, e1y, e1z));
        double v = (dN * qx + dMy * qy + dMz * qz) * inverse;
        if (v < 0.0 || u + v > 1.0) return None;
        return (e2x * qx + e2y * qy + e2z * qz) * inverse;
    }

    static (double, double, double) Coord(IForceMomentVertex vertex) =>
        (vertex.X.Kilonewtons, vertex.Y.KilonewtonMeters, vertex.Z.KilonewtonMeters);

    static (double, double, double) Sub((double x, double y, double z) left, (double x, double y, double z) right) =>
        (left.x - right.x, left.y - right.y, left.z - right.z);

    static (double, double, double) Cross((double x, double y, double z) left, (double x, double y, double z) right) =>
        (left.y * right.z - left.z * right.y, left.z * right.x - left.x * right.z, left.x * right.y - left.y * right.x);
}

// The INVERSE of Check — design as selection: one fold scans the frozen catalogue maps lightest-first (AreaMm2 ranks
// linear mass within a substance) and returns the first candidate whose capacity verdict is Adequate. The per-candidate
// capacity arrives through the caller's capacityOf projection because the family capacity needs placement facts —
// unbraced/effective lengths, service class, duration — the catalogue does not carry; admit is the family/depth policy
// filter (a stocked subset, a depth cap, one SteelClass). A candidate whose capacity FAULTS aborts the scan loud (a
// filter that admits a family the projection cannot price is a caller defect, never a silently skipped row); an
// exhausted scan faults typed. The full-catalogue seed supplies the search space, so the fold IS the lightest-adequate
// sizing query every member-design tool leads with.
public static class SectionSelection {
    public static Fin<(Component Section, Utilisation Verdict)> Lightest(
        FrozenDictionary<ComponentId, Component> rows,
        FrozenDictionary<ComponentId, ComputedSection> sections,
        Demand demand,
        Func<Component, ComputedSection, Fin<SectionCapacity>> capacityOf,
        Func<Component, bool> admit,
        Op key) =>
        toSeq(sections)
            .Filter(pair => rows.ContainsKey(pair.Key) && admit(rows[pair.Key]))
            .OrderBy(static pair => pair.Value.AreaMm2.Value)
            .ToSeq()
            .Fold(
                Fin.Succ(Option<(Component Section, Utilisation Verdict)>.None),
                (state, pair) => state.Bind(found => found.IsSome
                    ? Fin.Succ(found)
                    : capacityOf(rows[pair.Key], pair.Value)
                        .Map(capacity => capacity.Check(demand))
                        .Map(verdict => verdict.Adequate
                            ? Some((rows[pair.Key], verdict))
                            : Option<(Component, Utilisation)>.None)))
            .Bind(found => found.ToFin(ComponentFault.Capacity(key, "<selection-no-adequate-section>")));
}
```

## [03]-[RESEARCH]

- [RC_INTERACTION_HULL]: REALIZED — the `RcInteraction` capacity case is the ultimate biaxial Force-Moment-Moment capacity hull `VividOrange.InteractionDiagram` computes over the `reinforcement#RC_SECTION` `IConcreteSection`: `SectionCapacity.Resolve` dispatches the `CapacityBuild.Hull` request onto `new InteractionDiagram(rc.Section, hull.Resolution.ToSettings())` — the arm's own carried `DiagramResolution`, no loose knob — (the eager solve that `Triangle`-meshes the concrete+rebar section into `AnalyticalFace` fibres, runs the `Parallel.For` strain-plane sweep integrating fibre stress, and `MIConvexHull`-welds the (N, My, Mz) cloud into the closed onion, `.api/api-vividorange-interactiondiagram.md` `[FIBRE_INTEGRATION_CONTRACT]`) and reads `diagram.Mesh` as the cached `IForceMomentMesh`. VERIFIED via `assay api query InteractionDiagram --key VividOrange.InteractionDiagram`: the ctor pair `(IConcreteSection)` / `(IConcreteSection, DiagramSettings)` and the `public IForceMomentMesh Mesh { get; set; }` property, the sweep emitting `ForceMomentVertex(N, My, Mz, ForceUnit.Kilonewton, TorqueUnit.KilonewtonMeter)` so the hull coordinates are kN/kNm. The `Triangle` mesher and `MIConvexHull` hull builder are encapsulated `internal` inside the engine — this AEC-DOMAIN owner composes the welded hull through the constructor, mints NO direct `Triangle`/`MIConvexHull` call (the strata-correct seam: those primitives are `Rasm`-kernel-owned, consumed transitively, `.api/api-triangle.md` / `.api/api-vividorange-forcemomentinteraction.md [TRANSITIVE_CONVEX_HULL]` `[STACKING_LAW]`). The hull is read through the `IForceMomentMesh` interface floor (`hull.Faces` → `IForceMomentTriFace.A`/`B`/`C` → `IForceMomentVertex.X`/`Y`/`Z`, all VERIFIED on the `VividOrange.ICartesianBase` generic floor `ICartesianMesh.Faces`/`ICartesianTriFace.A,B,C`/`ICartesian3d.X`+`ILocalCartesian2d.Y,Z`), the `Force`/`Torque` coordinates as `UnitsNet` quantities through `Force.Kilonewtons`/`Torque.KilonewtonMeters`, never the `ForceMomentMesh` concrete and never the facet `Area` `Ratio` as a physical quantity. The eager solve caches on the `RcInteraction` carrier so a `Check(demand)` reads the cached hull, never re-solving. Ripple counterpart: `reinforcement#RC_SECTION` (the `RcSection`/`IConcreteSection` input this owner consumes).
- [RC_ELASTIC_TRANSFORMED]: REALIZED — the `RcElastic` capacity case reads the ONE `ConcreteSectionProperties` carrier the `RcSection` receipt holds (constructed and eager-forced at `RcSectionBuilder.Of` — a second `new ConcreteSectionProperties(rc.Section)` inside `Resolve` is the DELETED re-admission): the `RcSection` projections supply `TotalReinforcementArea`/`ConcreteArea`/`GeometricReinforcementRatio`/`ReinforcementSecondMomentOfAreaYy`/`Zz`, the bottom-face `EffectiveDepthMm(SectionFace.Bottom)` and `FaceSteelAreaMm2(SectionFace.Bottom)` tension steel — `Option<double>` at the receipt seam (the receipt traps the engine's no-bottom-bar throw), lifted `ToFin` onto the typed `<rc-elastic-no-bottom-tension-chord>` fault here — AND the `CrossSectionalShearReinforcementArea` link area; the GROSS `MomentOfInertiaYy`/`Zz` fibre divisors read the INHERITED base `SectionProperties` polygon integral on the same carrier (VERIFIED: `ConcreteSectionProperties : SectionProperties`, `Area TotalReinforcementArea`, `Area ConcreteArea => base.Area - TotalReinforcementArea`, `AreaMomentOfInertia ReinforcementSecondMomentOfAreaYy => Rebars.CalculateInertiaYy(_section)` — the STEEL-ONLY `Σ(As·d²)` moment — `Length EffectiveDepth(SectionFace)`, `Area ReinforcementArea(SectionFace)`, and `Area CrossSectionalShearReinforcementArea => Rebars.CalculateArea(_section.Link) * 2.0` — the TWO-LEG stirrup area, `.api/api-vividorange-sections-sectionproperties.md` `[RC seam]`). The EC2 SLS cracking check compares the maximum-tensile extreme-CONCRETE-fibre stress against `fctm` as the FULL combined action `σ = N/A ± My·cy/Iyy ± Mz·cz/Izz` — a SIGNED axial `N/A` (the `Demand` convention − compression / + tension, so a compressive service axial DELAYS cracking, the physically-correct SLS behaviour a `|N|` over-predicts) plus BOTH bending axes' tension-side fibre stress (`cy = h/2`, `cz = b/2` the gross half-depths), the divisors the GROSS `GrossInertiaYyMm4`/`GrossInertiaZzMm4` (the EC2 7.1 gross-basis SLS) — dividing by the reinforcement-only `Σ(As·d²)` column (~5% of gross) inflates the fibre stress ~20× and falsely cracks every service state, the ILLUSORY prior spelling now deleted; the `ReinforcementInertiaYyMm4`/`ZzMm4` columns stay carried as the cracked-section `Icr` input the forward `Rasm.Compute` member check composes with its modular ratio — never a major-axis-bending-only slice, never the effective depth `d` as a fibre lever. The bottom-face `EffectiveDepth(SectionFace.Bottom)` is the distinct ULS flexural lever to the tension steel; it is a first-class `ConcreteSectionProperties` concept member (the `.api` `[RC seam]` lists it) carried on the `RcElastic` record for the `Rasm.Compute/structural#DESIGN_CHECK` consumer's moment-capacity readout, never conflated with the SLS extreme-fibre distance. The SHEAR screen closes the coverage gap the `Demand.ShearYKn`/`ShearZKn` columns anticipated: the shear resultant folds against the EC2 §`6.2.2` `V_Rd,c` (`ρl` from the bottom-face tension steel, `k` from the effective depth, both `min`-capped per the code) for a LINKLESS section, and against the §`6.2.3`(3) web-crushing ceiling `V_Rd,max` (`cotθ = 2.5`) for a LINKED one — the ONLY shear facts decidable at section altitude, because the member `V_Rd,s = (Asw/s)·z·f_ywd·cotθ` needs the stirrup SPACING neither `IConcreteSection` (`ILink` carries diameter + mandrel, no spacing) nor `RcSection` models; the carried `ShearLinkAreaMm2` is exactly the `Asw` the forward `Rasm.Compute` member check pairs with its detailing spacing, so a linked section-level pass defers detailing loudly and a linked fail refutes the section outright. One `IConcreteSection` minted at `reinforcement#RC_SECTION` drives BOTH the elastic transformed-section properties here and the ultimate N-M-M hull (`.api/api-vividorange-sections-sectionproperties.md` `[01]-[RC_COMPOSITION_PATH]`), so the cracked/uncracked elastic check and the ultimate capacity envelope share one section input — the elastic `RcElastic` is the transformed-section complement the bare `component#COMPONENT_OWNER` `SectionSolver` (the gross elastic `Admit` lift over any `IProfile`, `SectionSolver.ProfileOf` the kept RC gross-outline entry) does not compute, this owner the reinforced-section transformed properties.
- [STEEL_LRFD_LIFT]: REALIZED — the `SteelLrfd` capacity case lifts the `steel#STEEL_FAMILY` `DesignCapacity` WHOLE into the one `SectionCapacity` rail: the AISC 360 Chapters F/E/G rolled-steel LRFD (`φMn`/`φPn`/`φVn`, plus the AISC 360 Chapter I composite and the AISI S100 cold-formed arms) already derived over the computed `SteelSection` columns, carried as the SI N·mm/N → kN·m/kN projection PLUS the `CompactnessClass` Table B4.1 verdict and the column slenderness `λ` (so the `Check` reports the governing compactness in a design report, not a bare ratio). A steel column and an RC column are checked through the SAME `Check(demand)` fold — never a parallel `SteelBeamCheck` surface. The `SteelLrfd` interaction is the REALIZED AISC 360 §H1.1 combined axial-flexure split — `p + 8/9·m` at `p >= 0.2` (H1-1a), `p/2 + m` below (H1-1b), `m` the PER-AXIS biaxial sum `|My|/φMnx + |Mz|/φMny` over the lifted `FlexuralKnm`/`FlexuralMinorKnm` pair — worst-folded with the §G shear-resultant ratio and the §H3.1 torsion `demand.TorsionKnm` against the lifted `TorsionalKnm`, the governing axis the typed `GoverningAction`; the prior `max(N/φPn, M/φMn, …)` independent-ratio fold AND the moment-resultant-against-`φMnx`-alone fold are the DELETED forms (the first passes `p = m = 0.9` where H1.1 fails at 1.7; the second credited a weak-axis moment the full major capacity, under-predicting weak-axis bending by `φMnx/φMny` — 3-10x on an I-shape — an unconservative naivety, not a refinement to defer). `φMny` is the REALIZED §F6 `DesignCapacity.FlexuralMinorNmm` column (`min(Fy·Zy, 1.6·Fy·Sy)` per F6-1, the 1.5 cap on the F10 single-angle regime, F6.2-bounded for the F2 flange classes over the SAME per-class `SlendernessRow` data, `Seff`-scaled on the cold-formed arm), read DIRECTLY by the lift. A zero torsion demand contributes a 0 ratio (so a `φTn = 0` open-shape column never spuriously governs), a nonzero torsion against a `φTn = 0` open shape surfaces as the governing over-ratio rather than silently passing — `demand.TorsionKnm` is a CONSUMED action, not a carried-but-ignored column. The §H3.1 `φTn = φT·Fcr·C` resistance is REALIZED on the steel `DesignCapacity.TorsionalNmm` column (the `SteelDesign.TorsionalResistance` projection over the canonical `JMm4` St-Venant constant, positive for the CLOSED HSS/pipe topology and engineering-zero for an OPEN warping-torsion shape), which the `SectionCapacity.Lift(CapacityReceipt.Steel)` arm reads DIRECTLY onto `TorsionalKnm` — one source, no redundant parallel lift parameter, so a torsion-loaded HSS checks against a real resistance. The steel capacity computation stays the `steel#STEEL_FAMILY` owner, this page only the unified utilisation rail. Ripple counterpart: `steel#STEEL_FAMILY` (the `DesignCapacity` receipt lifted here, the REALIZED AISC §H3.1 `DesignCapacity.TorsionalNmm` column the `SteelLrfd` lift folds against `demand.TorsionKnm`).
- [TIMBER_EC5_LIFT]: REALIZED — the `TimberEc5` capacity case lifts the `timber#TIMBER_CAPACITY` `TimberCapacity` WHOLE into the rail, closing the cross-file ripple `timber#EC5_DESIGN_CAPACITY` explicitly declared ("the `TimberCapacity` lifts into the `capacity#SECTION_CAPACITY` unified utilisation rail as a `SectionCapacity.TimberEc5` case the SAME way the steel `DesignCapacity` lifts as `SteelLrfd`"): the EN 1995-1-1 design resistances (`M_Rd,y` = `k_crit·k_h·k_mod·f_m,k·W / γ_M`, `M_Rd,z` = `k_h(w)·k_mod·f_m,k·S_y / γ_M` member / research-gated 0 panel, `N_Rd` = `k_c·k_mod·f_c0,k·A / γ_M` column-buckling-reduced, `V_Rd` = `k_mod·f_v,k·k_cr·A_shear / γ_M` rolling-shear for a CLT panel, `R_90,Rd` = `k_mod·f_c90,k·A_bearing / γ_M`) carried as kN·m/kN with the relative slenderness `λ_rel` and the `k_mod` service×duration factor preserved. The `TimberEc5` `Check` is the REALIZED EN 1995-1-1 km-swapped two-equation MAX pair — `axial + my + k_m·mz` vs `axial + k_m·my + mz` with the axial term LINEAR per §`6.3.2` eq 6.23/6.24 when buckling governs (`λ_rel > 0.3`, the carried `RelativeSlenderness` the branch discriminant, `N_Rd` already `k_c`-reduced) and `n²` per §`6.2.4` eq 6.19/6.20 for the stocky member, `k_m` the lifted §`6.1.6`(2) per-form weight, `my`/`mz` the `GuardedRatio` per-axis folds over `BendingKnm`/`BendingMinorKnm` — worst-folded with the shear ratio, the §`6.1.8` torsion `demand.TorsionKnm` against the lifted `TorsionalKnm`, AND the §`6.1.5` bearing `demand.BearingKn` against the lifted `R_90,Rd` (the same zero-demand-inert / unbounded-resistance-governs discipline), the governing axis the typed `GoverningAction`; the perpendicular-bearing column is a CONSUMED capacity — the prior carried-but-unchecked `R_90,Rd`, the independent-ratio `max(...)` fold, AND the moment-resultant-against-`M_Rd,y`-alone fold are the DELETED forms (the resultant fold under-predicted weak-axis bending by `M_Rd,y/M_Rd,z`). `M_Rd,z` is the REALIZED `TimberCapacity.BendingMinorNmm` member column (`k_h(w)·k_mod·f_m,k·S_y/γ_M` — no `k_crit`, no LTB about the minor axis); the PANEL minor is research-GATED at 0.0 (in-plane CLT bending verification unsettled pending EN 1995-1-1:2025), so an in-plane panel `Mz` demand governs loud through `GuardedRatio`, never passing silent. A timber column and a steel column are checked through ONE `Check(demand)` fold differing only in the capacity case — the EN 1995 design rules HAND-ROLLED in `timber#TIMBER_CAPACITY` (no .NET EC5 package), the unified verdict this owner's. The §`6.1.8` `T_Rd = k_shape·f_v,d·W_tor` resistance is REALIZED on the timber `TimberCapacity.TorsionalNmm` column (`TimberDesign.Capacity` over the rectangular `W_tor` Roark torsion modulus and the design longitudinal shear `f_v,d`), which the `SectionCapacity.Lift(CapacityReceipt.Timber)` arm reads DIRECTLY onto `TorsionalKnm` — one source, no redundant lift parameter, so a torsion-loaded glulam member checks against a real resistance. Ripple counterpart: `timber#TIMBER_CAPACITY` (the `TimberCapacity` receipt lifted here, the REALIZED EN 1995-1-1 §`6.1.8` `TimberCapacity.TorsionalNmm` column the `TimberEc5` lift folds against `demand.TorsionKnm`).
- [MASONRY_COMPRESSION_CHECK]: `CapacityReceipt.Masonry` carries `CmuStrength`, `ComputedSection`, `MasonryReduction`, `RuptureModulus`, `MortarSystem`, and `MortarType`. `MasonryReduction.Of` admits the `(0,1]` stability factor once; `Lift` resolves the typed `f'm`, section properties, stability value, and mortar-dependent `fr` into `MasonryCompression`. `Check` combines axial-flexural unity, flexural tension, and URM shear on the common utilization rail.
- [UTILISATION_RAY_CAST]: `SectionCapacity.Cast` intersects the raw applied `(N, My, Mz)` vector with the welded `IForceMomentMesh`. A positive face parameter is a load multiplier, so the bounded utilization is `1/t`; no intersection returns `Utilisation.Overcapacity`, and a fully zero demand returns `Utilisation.Bounded(0, Axial)`. Raw coordinates preserve the independent force and moment scales; a Euclidean unit vector across heterogeneous axes is forbidden. The ray verdict worst-folds with the `GuardedRatio`-against-0 shear/torsion/bearing candidates because the hull carries no resistance on those axes — a shear-only demand on an `RcInteraction` capacity governs loud as an unresisted action, never the prior silent `Bounded(0)` pass; the same total-demand fold closes the `RcElastic` (torsion/bearing), `SteelLrfd` (bearing), and `MasonryCompression` (torsion/bearing) arms.
- [COMPONENT_FAULT_RAIL]: REALIZED — the capacity owner rails the `component#COMPONENT_OWNER` `ComponentFault` band-2300 fault (`Code => FaultBand.Component` through the registry, composed here, never re-declared), the SAME component-sub-domain rail every sibling Component surface uses (`component#COMPONENT_OWNER` `SectionSolver.Solve`/`Admit` → `ComponentFault.Section`, the family seed folds → `ComponentFault.Family`/`Dimension`, `reinforcement#RC_SECTION` `RcSectionBuilder.Of` → `ComponentFault`). A section-capacity solve is a component-sub-domain concern, so its fault bands 2300 with its Component siblings — NOT the `Appearance/bsdf#SHADING_FRAME` `MaterialFault` band 2450, the appearance/material band that conflates a structural fault with a shading fault for any telemetry reader banding by `Expected.Code`, and NOT a thin private `CapacityFault` rename wrapper that adds zero domain value. `SectionCapacity.Resolve` rails the REALIZED `ComponentFault.Capacity(key, $"<rc-interaction-solve:{e.Message}>")` / `ComponentFault.Capacity(key, $"<rc-elastic-solve:{e.Message}>")` (a non-EN material the `IEnConcreteMaterial`/`IEnRebarMaterial` cast cannot read, an under-reinforced degenerate section, or a hull-weld failure), and the Option-typed `RcSection` face queries lift `ToFin` onto the typed `<rc-elastic-no-bottom-tension-chord>` fault (the receipt already traps the engine's no-bottom-bar `EffectiveDepth` divide at ITS seam), so no `VividOrange` throw and no silent absence reaches an interior signature. The dedicated `ComponentFault.Capacity` case (`Category => "Capacity"`, distinct from the `Dimension`/`Coring`/`Family`/`Bond`/`Mortar`/`Section` sibling arms) is REALIZED on the `component#COMPONENT_OWNER` owner so a section-CAPACITY-SOLVE fault carries a capacity-specific `Category` a telemetry reader bands apart from a section-PROPERTY (`Section`) or registration (`Family`) fault — the capacity solve never flattens onto `ComponentFault.Family`. `ComponentFault` 2300 is the one component-sub-domain band, disjoint from the kernel `GeometryFault` band 2400, each case `Expected`-derived with the `…Case`-suffixed factory pattern. Ripple counterpart: `component#COMPONENT_OWNER` (the realized `ComponentFault.Capacity` case the capacity solves rail).
- [MASONRY_REINFORCED_CASE]: REALIZED — `SectionCapacity.MasonryReinforced` is the TMS 402 §`9.3` arm the URM case's own admission law reserved (no existing case carried a steel term): `CapacityReceipt.ReinforcedMasonry(CmuStrength, ComputedSection, CmuRow, RebarGradeRow, MasonryReduction)` lifts the lattice facts the URM lift never consumed — `As = ReinforcedCells·π/4·RebarBarMm²`, the mid-wall lever `d = W/2`, the bed length `b = L`, the grouted net area — and the `Check` arm runs the §`9.3.4.1.1` reinforced `Pn`, the §`9.3.5` steel-couple `Mn = As·fy·(d − a/2)`, the steel-resisted net tension (the URM tension-governs-outright arm retired for the reinforced state), and the §`9.3.4.1.2` `Vnm` screen at the `M/(V·dv) = 1` bound (`Vns` is member detailing — bar spacing is not a section column), φ = 0.90 / φv = 0.80 per §`9.1.4`. The masonry rail is now total over the grouted/reinforced states the cmu lattice distinguishes cell-by-cell; the sanctioned RC/EC2-builder workaround for a grouted unit stays the RC path, never a §`9.3` substitute. Ripple counterpart: `cmu#CMU_FAMILY` (the `ReinforcedCells`/`RebarBarMm`/`GroutedCells` columns this lift consumes).
- [GLASS_PANE_LIFT]: REALIZED — `SectionCapacity.GlassPane` lifts the `glazing#GLAZING_FAMILY` `GlassCapacity` receipt WHOLE through `CapacityReceipt.Glass` (the EN 16612 governing-pane per-metre design moment, resistance, and effective thickness — the derivation stays the glazing owner's `GlazingStructural`), and the `Check` arm folds both plate-bending directions against the isotropic per-metre resistance with every non-plate action governing loud — glazing is the fifth structural rail through the SAME `Check(demand)` fold as steel and timber, per the receipt-lift discipline the steel/timber/cmu cases fixed. Ripple counterpart: `glazing#GLAZING_FAMILY` (`GlassCapacity`/`GlazingStructural`).
- [CONNECTION_RAIL]: REALIZED — the `Utilisation`/`GoverningAction` vocabulary is total over the LOAD PATH: `CapacityReceipt.Weld`/`Adhesive`/`Stud`/`Connector` lift the realized `joint#JOINT_FAMILY` `JointRow.Weld.DirectionalShearKn(Angle)`/`JointRow.Adhesive.DesignShearKn`/`JointRow.Stud.DesignShearKn × count` design values and the `connector#CONNECTOR_FAMILY` `ConnectorCapacity` duration-governed columns (seat-borne download Cd-exempt, fastener-transferred uplift/lateral Cd-scaled) onto ONE `SectionCapacity.Connection(ShearKn, TensionKn, BearingKn)` case — member → connection → member is one `Check` rail, and the connector's private `DemandRatio` unit-check collapses onto the same polymorphic fold (its direction-typed read surviving as the lift columns). The joint page's deferred utilisation case lands here; the composite stud's concrete branch stays the `Rasm.Compute` `min(steel, concrete)` join. Ripple counterparts: `joint#JOINT_FAMILY` (the deferral sentence retired), `connector#CONNECTOR_FAMILY` (the mini-rail collapse).
- [SECTION_SELECTION]: REALIZED — `SectionSelection.Lightest` is the rail's inverse query: a lightest-first fold (AreaMm2 the linear-mass rank) over the frozen `ComponentCatalogue` `Rows`/`Sections` maps under a caller policy filter, each candidate priced through the caller's `capacityOf` projection (placement facts — lengths, service, duration — are not catalogue columns) and checked through the ONE `Check(demand)`; the first `Adequate` verdict returns with its `Component`, a pricing fault aborts loud, an exhausted scan faults typed. The full-database steel seed is the search space, a stocked subset one `admit` filter.
- [ADMITTED_DEMAND_AND_RECEIPT_UNION]: `Demand.Of` admits every finite signed action. `CapacityReceipt` carries the complete sibling receipt per case, including `MasonryReduction`; `Lift(CapacityReceipt)` is the sole lift entry. `Utilisation.Bounded` is final, `Utilisation.RequiresMemberCheck` carries the named unresolved member-level check, and `Utilisation.Overcapacity` carries no infinity sentinel.
- [HULL_CACHE]: REALIZED — the `IForceMomentMesh` C#-internal cache the Boundary prose previously CLAIMED without a fence is the `SectionCapacity.Freeze`/`Thaw` pair: `Freeze` writes the eagerly-solved hull through `VividOrange.Serialization` `JsonSerializationExtensions.ToJson<T>` over the `ITaxonomySerializable` marker the `IForceMomentMesh` INTERFACE itself extends (decompile-verified: `IForceMomentMesh : ICartesianMesh<…>, IGeometryBase, ITaxonomySerializable`, so `capacity.Hull.ToJson()` compiles against the floor, never the `ForceMomentMesh` concrete), emitting the `$type`-tagged Newtonsoft wire whose `Force`/`Torque` vertex fields serialize as `UnitsNet` SI-scalar+unit pairs; `Thaw` rehydrates through `json.FromJson<IForceMomentMesh>()`, the `TypeNameHandling.Objects` `$type` tag reconstructing the exact concrete mesh WITHOUT re-running the eager `Steps²` strain-plane sweep, and the `FromJson` `null` return or Newtonsoft throw traps onto `ComponentFault.Capacity` at the one boundary. The round-trip is producer=consumer ONLY — `TypeNameHandling.Objects` is a known deserialization-gadget surface, so `Thaw` is fed exclusively JSON a trusted `Freeze` minted, and the `$type` shape NEVER crosses to a TS/Python peer (the WIRE_FIREBREAK: cross-language egress stays the portable `Utilisation` scalars over the canonical Thinktecture wire). A second serializer beside this pair, or a re-solve where a frozen hull exists, is the rejected form.
