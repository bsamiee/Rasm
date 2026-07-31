# [MATERIALS_CAPACITY]

THE SECTION-CAPACITY OWNER and THE ONE UTILISATION RAIL. One `SectionCapacity` `[Union]` is the closed structural-capacity surface a `Component` cross-section carries beyond its elastic `ComputedSection`, and one `Demand` folded against it through `Check` is the typed `Utilisation` verdict — so EVERY family's design check is one polymorphic fold differing only in the capacity case, never a per-family `RcColumnCheck`/`SteelBeamCheck`/`MasonryWallCheck` surface. The closed case set spans the realized `ComponentFamily` structural rails: `RcInteraction` (the ultimate biaxial Force-Moment-Moment capacity hull `VividOrange.InteractionDiagram` welds over the `reinforcement#RC_SECTION` `IConcreteSection`), `RcElastic` (the elastic transformed-section reinforcement properties `VividOrange.Sections.SectionProperties` `ConcreteSectionProperties` computes over the same section, PLUS the EC2 §6.2 section-level shear screen over the bottom-face tension steel and the two-leg link area `CrossSectionalShearReinforcementArea` carries), `SteelLrfd` (the AISC 360 `steel#STEEL_FAMILY` `DesignCapacity` `φMn`/`φMny`/`φPn`/`φVn` + `CompactnessClass`/slenderness lifted whole — the AISI deck receipt and the EN 1993-1-2 fire state land the same case), `TimberEc5` (the EN 1995-1-1 `timber#TIMBER_CAPACITY` `TimberCapacity` design-resistance receipt lifted whole — `M_Rd,y`/`M_Rd,z` per axis with the §`6.1.6`(2) `k_m` weight), and `MasonryCompression` (the TMS 402 axial-flexural unity check PLUS the §`9.2.2` flexural-tension screen over the Table `9.1.9.2` `fr` — the `cmu#CMU_FAMILY` `CmuStrength` `f'm` + grouted `ComputedSection` + the `masonry#MASONRY_FAMILY` `RuptureModulus` mortar-keyed row feed). A capacity is admitted to the family ONLY when no existing case's column set carries it: each sibling family page that hand-rolls its design rules (`steel#STEEL_FAMILY`, `timber#TIMBER_CAPACITY`, `cmu#CMU_FAMILY`) lifts its already-computed receipt into ONE case here, and the RC cases are the two `Resolve` builds over the section input — the design-code COMPUTATION stays the family owner's, the unified VERDICT this owner's. The rail is TOTAL over the load path: `MasonryReinforced` carries the TMS 402 §9.3 steel-couple arm over the cmu lattice facts, `GlassPane` the EN 16612 pane resistance the glazing family lifts, and `Connection` the weld/adhesive/stud/connector receipts — one `Check` from cross-section to weld to hanger — while `SectionSelection.Lightest` and `SectionSelection.Fabricated` are the rail's INVERSE queries, the least-MASS section-passing scan over the frozen catalogue maps the full-database steel seed supplies and over a caller-parameterized `SectionProfile.BuiltUp` composition sweep. This owner is the ULTIMATE complement to `component#COMPONENT_OWNER` `SectionSolver`: that solver gives the elastic `ComputedSection` every family solves from its `SectionProfile` arm, THIS owner gives the reinforced-section transformed properties, the EC2 section-level shear screen, the ultimate capacity hull, and the unified utilisation fold the elastic solver does not. The `InteractionDiagram` constructor RUNS the full eager fibre-integration solve at construction (the `Triangle` section mesh, the `Parallel.For` strain-plane sweep, the `MIConvexHull` hull weld are encapsulated `internal` — this owner composes the welded `IForceMomentMesh`, never the meshing primitive), so a design page constructs the capacity ONCE per section/settings and reads `diagram.Mesh` cached, never re-solving per query. The page composes `reinforcement#RC_SECTION` `RcSection`/`IConcreteSection` for the RC input, `VividOrange.InteractionDiagram` (`InteractionDiagram`/`DiagramSettings`/`IForceMomentMesh`) for the N-M-M hull, `VividOrange.Sections.SectionProperties` `ConcreteSectionProperties` for the elastic transformed-section properties, `VividOrange.Materials` `EnConcreteFactory` for the EC2 `fck` the cracking reference reads, the `steel#STEEL_FAMILY` `DesignCapacity` / `timber#TIMBER_CAPACITY` `TimberCapacity` / `cmu#CMU_FAMILY` `CmuStrength` sibling receipts, the in-folder `UnitsNet` `Force`/`Torque`/`Area`/`Length` quantity coercion at the edge, and the `component#COMPONENT_OWNER` `ComponentFault` band-2300 rail (the SAME component-sub-domain fault every sibling Component family page rails — NOT a borrowed appearance band) for a non-finite, degenerate, or infeasible solve; the capacity surface and the utilisation verdict feed the forward `Rasm.Compute/Analysis/structural#DESIGN_CHECK` structural-Assessment route by `MaterialId`/section key, host-neutral here, the `IForceMomentMesh` round-tripping through the realized `SectionCapacity.Freeze`/`Thaw` `VividOrange.Serialization` pair into a `Rasm.Persistence` artifact row content-keyed on `(ComponentId, DiagramResolution.Key)` — the eager `Steps²` solve is paid once, persisted, and rehydrated across processes, never re-run.

## [01]-[INDEX]

- [02]-[SECTION_CAPACITY]: the `SectionCapacity` `[Union]` (`RcInteraction` N-M-M hull · `RcElastic` transformed-section · `SteelLrfd` rolled/cold-formed/deck/fire steel · `TimberEc5` EC5 receipt · `MasonryCompression` TMS 402 compression + §`9.2.2` flexural-tension · `MasonryReinforced` TMS 402 §`9.3` steel-couple · `GlassPane` EN 16612 pane · `Connection` weld/adhesive/stud/connector load path) over the `component#COMPONENT_OWNER` `ComponentFault` band-2300 rail, the `CapacityBuild` RC-build request `[Union]` (hull · elastic — the hull arm alone carrying its `DiagramResolution`), the `DiagramResolution` `[SmartEnum]` mesh/sweep-refinement policy folding to a `DiagramSettings`, the `Demand` applied-action shape (axial · biaxial moment · biaxial shear · torsion · bearing), the `GoverningAction` `[SmartEnum]` verdict axis, the `Utilisation` typed verdict, the `MemberCheckRequirement` section-undecidable deferral vocabulary, the `CapacityReceipt` sibling-receipt request `[Union]` (steel · timber · steel-deck · masonry · reinforced-masonry · glass · steel-fire · timber-fire · weld · adhesive · stud · connector — each case carrying its full lift context), and the `SectionCapacity.Resolve` eager-solve boundary plus the ONE TOTAL `Lift(CapacityReceipt)` entry and the `Freeze`/`Thaw` content-keyed hull-artifact round-trip — every boundary static on the union owner, no satellite resolver class — plus the `SectionSelection.Lightest`/`Fabricated` inverse sizing folds over the frozen catalogue maps and the fabricated composition sweep.

## [02]-[SECTION_CAPACITY]

- Owner: `SectionCapacity` is the closed capacity family spanning the member rails and the connection load path; `Demand` admits the signed action vector; `Utilisation` distinguishes a bounded verdict, a section pass owing a named member check, and an UNBOUNDED verdict (the capacity surface does not bound the demand), projecting the strict `Adequate` acceptance bit, the section-altitude `SectionPasses` bit the sizing folds select on, and the optional `Ratio` every downstream reader charts against; `MemberCheckRequirement` closes the section-undecidable deferral vocabulary; `MasonryReduction` OWNS the TMS 402 stability bracket as a derivation over `(height, radius of gyration)`; `CapacityBuild` and `CapacityReceipt` carry solve and lift modality, and `CapacityReceipt.Kind` owns the case-name projection every signal dimension and analytics column keys on, so a reflected runtime type name at a consumer has no reason to exist.
- Cases: `RcInteraction` (the ultimate biaxial N-M-M capacity hull as the `IForceMomentMesh` over an `IConcreteSection`, `VividOrange.InteractionDiagram`) · `RcElastic` (the elastic section state read off the ONE `ConcreteSectionProperties` carrier the `RcSection` receipt holds — `TotalReinforcementArea`/`ConcreteArea`/`GeometricReinforcementRatio`, the GROSS `MomentOfInertiaYy`/`Zz` (the inherited base polygon integral — the SLS fibre divisors) AND the `ReinforcementSecondMomentOfAreaYy`/`Zz` `Σ(As·d²)` steel moments (the cracked-`Icr` readout), + the bottom-face `EffectiveDepth(SectionFace)` ULS lever + the bottom-face `ReinforcementArea(SectionFace)` tension steel and the two-leg `CrossSectionalShearReinforcementArea` link area + the gross depth AND width (the major/minor-axis SLS extreme-fibre levers) + the parsed `fck` and its EC2 `fctm` cracking limit, the combined `N/A ± My·cy/Iyy ± Mz·cz/Izz` SLS check AND the EC2 §6.2 shear screen) · `SteelLrfd` (the rolled/composite/cold-formed `steel#STEEL_FAMILY` `DesignCapacity` `φMn`/`φMny`/`φPn`/`φVn` + `CompactnessClass` + slenderness lifted WHOLE — the §F6 minor column the per-axis H1.1 fold divides against) · `TimberEc5` (the EN 1995-1-1 `timber#TIMBER_CAPACITY` `TimberCapacity` `M_Rd,y`/`M_Rd,z`/`N_Rd`/`V_Rd`/`R_90,Rd` + `λ_rel` + `k_m` + `k_mod` lifted WHOLE — the member minor column `k_h(w)`-scaled with no `k_crit`, the panel minor research-gated 0) · `MasonryCompression` (the TMS 402 axial-flexural check + the §`9.2.2` flexural-tension screen the `cmu#CMU_FAMILY` `CmuStrength` `f'm` + the grouted `ComputedSection` net area AND both net moduli `SxMm3`/`SyMm3` + slenderness reduction + the `masonry#MASONRY_FAMILY` `RuptureModulus` Table `9.1.9.2` `fr` feed) — plus `MasonryReinforced` (the TMS 402 §`9.3` steel-couple arm over the cmu lattice's `ReinforcedCells`/`RebarBarMm`/grouted-net facts and the bar grade's yield), `GlassPane` (the EN 16612 governing-pane per-metre resistance the `glazing#GLAZING_FAMILY` `GlassCapacity` receipt lifts WHOLE), and `Connection` (the `joint#JOINT_FAMILY` weld/adhesive/stud design values and the `connector#CONNECTOR_FAMILY` duration-governed capacity as one shear/tension/bearing column triple) — the closed structural-capacity family across steel/RC/timber/masonry/glass and the connection load path; a capacity is a `SectionCapacity` case over a section or connection receipt, never a per-section-type check.
- Entry: `SectionCapacity.Resolve(RcSection, CapacityBuild, Op)` dispatches the RC solve request; the TOTAL `SectionCapacity.Lift(CapacityReceipt)` dispatches every already-computed sibling receipt — steel, timber, steel deck, masonry, reinforced masonry, glass, the two fire modalities, and the four connection kinds; internal `Freeze`/`Thaw` persist the content-keyed hull artifact; and `Check(Demand)` returns the closed `Utilisation` verdict. The masonry receipts carry the member HEIGHT as a kernel-admitted `PositiveMagnitude` beside their section, so `Lift` mints the stability reduction from the section's own governing radius — no caller-supplied stability scalar and no re-derived code bracket exists. `SectionSelection.Lightest` and `SectionSelection.Fabricated` are the inverse queries over the frozen catalogue and a caller-parameterized composition sweep. The `RcInteraction` arm casts the raw `(N, My, Mz)` demand vector against the hull and interprets the smallest positive intersection parameter as the capacity multiplier; utilization is its reciprocal. Force and moment axes are never Euclidean-normalized together.
- Packages: VividOrange.InteractionDiagram (`InteractionDiagram`/`DiagramSettings`, the eager-solve ctor + `Mesh`; `.api/api-vividorange-interactiondiagram.md`), VividOrange.IForceMomentInteraction (`IForceMomentMesh`/`IForceMomentVertex`/`IForceMomentTriFace` the hull read through, the `Faces`/`A`/`B`/`C`/`X`/`Y`/`Z` `Force`/`Torque` members; `.api/api-vividorange-iforcemomentinteraction.md`), VividOrange.Sections.SectionProperties (`ConcreteSectionProperties` the transformed-section carrier RIDING the `RcSection` receipt — the `EffectiveDepth(SectionFace)`/`ReinforcementArea(SectionFace)` face queries, the `CrossSectionalShearReinforcementArea` two-leg link area, and the inherited base `MomentOfInertiaYy`/`Zz` gross polygon integral the SLS fibre divisors read; `.api/api-vividorange-sections-sectionproperties.md`), VividOrange.Sections (`IConcreteSection`/`SectionFace` from the `reinforcement#RC_SECTION` `RcSection`; `.api/api-vividorange-sections.md`), VividOrange.Materials (`EnConcreteFactory.CreateLinearElastic` whose `LinearElasticMaterial.Strength` IS the parsed `fck` — decompile-verified: the factory parses the first `Cxx` token of the grade, so `Strength.Megapascals` is the characteristic cylinder strength the EC2 `fctm` AND the §6.2 shear screen read; `.api/api-vividorange-materials.md`), VividOrange.Serialization (`JsonSerializationExtensions.ToJson`/`FromJson<T>` `where T : ITaxonomySerializable` — the `Freeze`/`Thaw` content-keyed hull artifact over the marker `IForceMomentMesh` itself extends, `$type`-tagged Newtonsoft wire + `UnitsNet` SI-scalar+unit quantities, producer=consumer only; `.api/api-vividorange-serialization.md`), UnitsNet (`Force.Kilonewtons`/`Torque.KilonewtonMeters`/`Area`/`Length`/`Ratio`/`Angle` coerced at the edge; `libs/csharp/.api/api-unitsnet.md`), Rasm.Element (project — `MaterialId`/`ProfileRef` the seam-carried identity, seam-canonical), Rasm (project — `PositiveMagnitude` from `Rasm.Numerics`, `Op`/`Context` from `Rasm.Domain`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Fold`), Thinktecture.Runtime.Extensions (`[Union]` for `SectionCapacity`/`CapacityBuild`, `[SmartEnum]` for `DiagramResolution`/`GoverningAction`). Triangle + MIConvexHull ride transitively INSIDE the `InteractionDiagram` engine (encapsulated `internal`, `.api/api-triangle.md` / `.api/api-vividorange-forcemomentinteraction.md [TRANSITIVE_CONVEX_HULL]`) — this owner mints NO direct mesher/hull call, composing only the welded `IForceMomentMesh`. The `steel#STEEL_FAMILY` `DesignCapacity`, `timber#TIMBER_CAPACITY` `TimberCapacity`, and `cmu#CMU_FAMILY` `CmuStrength` are sibling-page receipts lifted, never re-computed.
- Growth: a new structural family's capacity is one `SectionCapacity` `[Union]` case binding either a `Resolve` build (a section-input solve) or a lift factory (an already-computed sibling receipt) plus one `Check` arm — a moment-curvature `RcInteraction` refinement, a panel diaphragm unit-shear check — admitted only when no existing case's column set carries it; a new demand axis is one `Demand` column (a warping bimoment, a second-order P-Δ amplifier); a new utilisation metric one `Utilisation`/`GoverningAction` projection — never a per-section-type capacity surface, never a re-derived elastic property where `ConcreteSectionProperties` computes it, never a direct `Triangle`/`MIConvexHull` call where the `InteractionDiagram` engine welds the hull; a persisted-capacity need is the one `Freeze`/`Thaw` pair over the `ITaxonomySerializable` marker, never a second serializer; the `steel`/`timber`/`cmu`/`panel` design receipts stay the family-owner derivation lifted here, never re-computed — the fire modality is that law EXECUTED, two `CapacityReceipt` cases lifting the landed `SteelDesign` `FireRetention` retention pair and the timber `ResidualStack` charred receipt onto the existing verdict cases.
- Boundary: `SectionCapacity.Resolve` and `Check` are the `Projection/observability#SIGNAL_FACTS` `MaterialsFact.CapacityCheck(Key, Receipt, Verdict, Elapsed)` tap SUBJECTS and `Check` is the `Projection/benchmarks#BENCH_CORPUS` `BenchKernel.InteractionSweep` measured kernel; the tap is a composition-root decorator over `MaterialsHooks.CapacityCheck`, so this owner emits nothing, carries no `Duration`, and references no signal type — the seam is declared at both ends and instrumented at neither, and `CapacityReceipt.Kind` is the one dimension spelling both the fact stream and the analytics column key on.
- Boundary: the frozen hull RESIDES in `Rasm.Persistence` as an artifact row content-keyed on `(ComponentId, DiagramResolution.Key)` — the `Rasm.Materials/ARCHITECTURE.md` `[CONTENT_KEY]: ArtifactIndexRow` edge the raster estate already crosses, reused verbatim rather than minted a second time. `Freeze` writes that row and `Thaw` is fed EXCLUSIVELY from that store, so the eager `Steps²` solve is paid once per `(section, resolution)` pair and rehydrated across processes; a process-local memo re-pays the sweep on every load and carries none of the claim this page states. The store is the ONLY `Thaw` ingress: the `TypeNameHandling.Objects` `$type` wire is a deserialization-gadget surface, so no peer document reaches it.
- Boundary: `SectionCapacity.Resolve` is the BOUNDARY_ADMISSION point where the `VividOrange.InteractionDiagram` engine is admitted EXACTLY ONCE and the `ConcreteSectionProperties` carrier — admitted once at `RcSectionBuilder.Of`, riding the `RcSection` receipt — is READ, never re-constructed — the `InteractionDiagram` ctor runs the expensive eager solve (`.api/api-vividorange-interactiondiagram.md` `[construction law]`) and a non-EN material whose `IEnConcreteMaterial`/`IEnRebarMaterial` cast the engine cannot read, an under-reinforced degenerate section, or a hull-weld failure rails `ComponentFault.Capacity` (the component-sub-domain band 2300 — `FaultBand.Component` on the registry — the dedicated capacity-solve slot distinct from the `Section` elastic-integral slot `component#COMPONENT_OWNER` `SectionSolver.Admit` rails, both band 2300 with their Component siblings, NOT the `Appearance/bsdf#SHADING_FRAME` `MaterialFault` band 2450) rather than throwing, so no `VividOrange` throw and no infeasible hull reaches an interior signature; the `IForceMomentMesh` is read THROUGH its interface floor (`.api/api-vividorange-iforcemomentinteraction.md` `[LOCAL_ADMISSION]`), never the `ForceMomentMesh` concrete, and the `Force`/`Torque` hull coordinates carry as `UnitsNet` quantities coerced to SI base (`Force.Kilonewtons`/`Torque.KilonewtonMeters`) once at the edge so no interior signature carries the hull as raw `double`; the `Triangle` section mesher and the `MIConvexHull` hull builder are encapsulated `internal` inside the engine (`.api/api-triangle.md` `[STACKING_LAW]` / `.api/api-vividorange-forcemomentinteraction.md [TRANSITIVE_CONVEX_HULL]` `[STACKING_LAW]`) — this AEC-DOMAIN owner mints NO direct mesher/hull call, composing the welded hull through the constructor, the strata-correct seam (the computational-geometry primitives are `Rasm`-kernel-owned, consumed transitively here); the eager solve is cached on the `SectionCapacity` `RcInteraction` carrier (`.api/api-vividorange-interactiondiagram.md` `[LOCAL_ADMISSION]` — construct once per section/settings, never re-solve per query), so a `Check(demand)` reads the cached hull; the `RcInteraction` utilisation is the exact Möller–Trumbore intersection of the origin-cast demand ray against the hull faces (the `IForceMomentTriFace.A`/`B`/`C` the demand vector pierces, the positive front-face pierce `t` the capacity boundary along the load direction), the no-pierce case (an eccentric hull that does not enclose the origin) yielding the typed `Utilisation.Unbounded` verdict rather than a silent `+∞`, NEVER the facet `Area` `Ratio` read as a physical quantity (`.api/api-vividorange-iforcemomentinteraction.md` `[AXIS_SEMANTICS]`); the `Utilisation.Governing` is the typed `GoverningAction` `[SmartEnum]` (axial · flexure · biaxial-moment · combined · shear · torsion · bearing — ONE canonical term per action; a `bending` synonym row beside `flexure` is the deleted form, and every axial-plus-flexure interaction reports `combined` rather than whichever component was larger), NEVER a stringly-typed verdict; the capacity surface is host-neutral — the `IForceMomentMesh` round-trips through the realized `Freeze`/`Thaw` pair (`ToJson`/`FromJson<IForceMomentMesh>` over the marker the interface itself extends, `.api/api-vividorange-serialization.md`) into its content-keyed `Rasm.Persistence` artifact row, producer=consumer ONLY: the `TypeNameHandling.Objects` `$type` wire is a deserialization-gadget surface, so `Thaw` is fed exclusively JSON a trusted `Freeze` minted, never an external document, and the `$type` shape NEVER crosses to a peer (distinct from the canonical Thinktecture wire) — the utilisation verdict crosses to `Rasm.Compute/Analysis/structural#DESIGN_CHECK` as portable scalar data keyed by section, never a `VividOrange` assembly type crossing the boundary.

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
using VividOrange.Serialization;                     // JsonSerializationExtensions ToJson/FromJson (the Freeze/Thaw content-keyed hull artifact)
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
    // The steel deck's AISI S100 receipt beside its gauge and rib rows: the panel#PANEL_FAMILY deck seeds Sectioned
    // and solves a full ComputedSection through the corrugated arm, and SteelDesign's AISI overload prices it at the
    // GaugeRow's own SS Grade 33/50 yield — so the deck datum GaugeRow.AxialSectionCapacityKnPerMm declared for the
    // seam finally has a check behind it. Gauge and Rib ride the receipt because the report and the analytics kind
    // dimension name WHICH deck, never a bare steel row.
    public sealed record DeckSheet(GaugeRow Gauge, DeckProfileRow Rib, DesignCapacity Capacity) : CapacityReceipt;
    public sealed record Masonry(CmuStrength Strength, ComputedSection Section, PositiveMagnitude HeightMm, RuptureModulus Rupture, MortarSystem System, MortarType Mortar) : CapacityReceipt;
    // The reinforced case reads the cmu lattice facts the URM case never consumed: the seed row's ReinforcedCells/
    // RebarBarMm steel, the bar grade's yield, and the grouted net section — the TMS 402 §9.3 inputs.
    public sealed record ReinforcedMasonry(CmuStrength Strength, ComputedSection Section, PositiveMagnitude HeightMm, CmuRow Unit, RebarGradeRow Bar) : CapacityReceipt;
    // The glazing pane resistance lifted WHOLE from glazing#GLAZING_FAMILY GlazingStructural — never re-derived here.
    public sealed record Glass(GlassCapacity Capacity) : CapacityReceipt;
    // The ACCIDENTAL fire design situation as two more lift cases over the SAME law: the steel owner's already-read
    // EN 1993-1-2 Table 3.1 retention pair (steel#STEEL_FAMILY FireRetention.At over the section's SectionFactorPerM
    // and CriticalTemperatureC) rides beside the ambient receipt, and the timber owner's ResidualSection/ResidualStack
    // charred receipt arrives already priced at kmod = γM = 1.0 — the EN 1995-1-2 accidental combination. Neither arm
    // derives fire physics here: the family owner computes, this owner lifts, and Check folds the fire state through
    // the identical ambient interaction so a fire verdict and an ambient verdict are one rail.
    public sealed record SteelFire(DesignCapacity Ambient, double Ky, double Ke, double SteelTemperatureC) : CapacityReceipt;
    public sealed record TimberFire(TimberCapacity Residual) : CapacityReceipt;
    // The connection receipts — the joint#JOINT_FAMILY line/area/stud design values and the connector#CONNECTOR_FAMILY
    // duration-governed capacity — each case carrying its full lift context (the weld its load angle, the stud its
    // group count), so the load-path verdict rides the SAME Check fold as the member cases.
    public sealed record Weld(JointRow.Weld Row, double LoadAngleDeg) : CapacityReceipt;
    public sealed record Adhesive(JointRow.Adhesive Row) : CapacityReceipt;
    public sealed record Stud(JointRow.Stud Row, int Count) : CapacityReceipt;
    public sealed record Connector(ConnectorCapacity Capacity) : CapacityReceipt;

    // Case identity IS the kind dimension every downstream reader keys on — signal roster tag and analytics column
    // alike — so this total projection holds the one spelling, a thirteenth case breaks it at compile time, and no
    // reflected runtime type name at a consumer renames that dimension on the next case rename.
    public string Kind => Switch(
        steel: static _ => nameof(Steel),
        timber: static _ => nameof(Timber),
        deckSheet: static _ => nameof(DeckSheet),
        masonry: static _ => nameof(Masonry),
        reinforcedMasonry: static _ => nameof(ReinforcedMasonry),
        glass: static _ => nameof(Glass),
        steelFire: static _ => nameof(SteelFire),
        timberFire: static _ => nameof(TimberFire),
        weld: static _ => nameof(Weld),
        adhesive: static _ => nameof(Adhesive),
        stud: static _ => nameof(Stud),
        connector: static _ => nameof(Connector));
}

// The verdict axis — which applied action governs the check, a typed bounded vocabulary NEVER a stringly-typed label.
// One canonical term per action (flexure owns every bending-governed verdict — a bending synonym row is the deleted
// form): axial/flexure for the verdicts one action really drives (the RC cracking fold, the glass plate-bending fold),
// COMBINED for every unity ratio that is definitionally an axial-plus-flexure INTERACTION — the AISC §H1.1, EN 1995
// §6.3.2/§6.2.4, and TMS 402 §9.2/§9.3 sums no single component reaches, so a design report never reads `Axial` on a
// 1.7 ratio neither p nor m alone attains and the analytics governing column never files an interaction under an axis
// that does not describe it; biaxial-moment for the RC hull ray, shear for a shear-governed check, torsion for the
// St-Venant demand the steel/timber arms fold against their torsional resistance, bearing for the perpendicular
// support reaction the timber R_90,Rd resists.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GoverningAction {
    public static readonly GoverningAction Axial         = new("axial");
    public static readonly GoverningAction Flexure       = new("flexure");
    public static readonly GoverningAction BiaxialMoment = new("biaxial-moment");
    public static readonly GoverningAction Combined      = new("combined");
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

// The TMS 402 member-stability bracket as a DERIVED value object: the formula IS the owner, so no caller re-derives
// the code bracket and a transposed branch is unrepresentable. The height arrives as the kernel-admitted
// PositiveMagnitude and the radius as the always-positive ComputedSection.GoverningRadiusMm, so the derivation is
// TOTAL — its range over h/r ∈ (0, ∞) is exactly (0, 1] — and the throwing Create is the sanctioned re-admission of a
// value the algebra already proves. The prior raw-scalar Of is DELETED with its caller-supplied branch: every producer
// is now the Lift arm that holds the section.
[ValueObject<double>]
public readonly partial struct MasonryReduction {
    const double SlendernessBreak = 99.0;   // TMS 402: h/r <= 99 takes the parabolic bracket, above it the Euler-form ratio

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value is > 0.0 and <= 1.0
            ? null
            : new ValidationError($"<masonry-reduction-invalid:{value:R}>");

    public static MasonryReduction Of(PositiveMagnitude heightMm, double radiusOfGyrationMm) =>
        heightMm.Value / radiusOfGyrationMm is var ratio && ratio <= SlendernessBreak
            ? Create(1.0 - Math.Pow(ratio / 140.0, 2.0))
            : Create(Math.Pow(70.0 / ratio, 2.0));
}

[Union]
public abstract partial record Utilisation {
    private Utilisation(GoverningAction governing) => Governing = governing;
    public GoverningAction Governing { get; }
    // The strict ACCEPTANCE bit: only a bounded ratio at or under unity is a finished verdict.
    public bool Adequate => Switch(
        bounded: static verdict => verdict.Value <= 1.0,
        requiresMemberCheck: static _ => false,
        unbounded: static _ => false);
    // The SECTION-altitude pass: a deferring verdict passed everything the section can decide and owes only the named
    // member-level detailing check, so a sizing query returns it WITH its deferral attached rather than rejecting the
    // exact sections a designer wants — the acceptance bit stays strict for the terminal report.
    public bool SectionPasses => Switch(
        bounded: static verdict => verdict.Value <= 1.0,
        requiresMemberCheck: static verdict => verdict.Value <= 1.0,
        unbounded: static _ => false);
    // Two cases carry ONE demand/capacity number and the unbounded case carries none, so every reader — a design
    // report, a projection, a chart series — takes the verdict's own optional ratio and no consumer re-enumerates
    // which cases happen to hold a Value.
    public Option<double> Ratio => Switch(
        bounded: static verdict => Some(verdict.Value),
        requiresMemberCheck: static verdict => Some(verdict.Value),
        unbounded: static _ => Option<double>.None);

    public sealed record Bounded(double Value, GoverningAction Action) : Utilisation(Action);
    public sealed record RequiresMemberCheck(double Value, GoverningAction Action, MemberCheckRequirement Requirement) : Utilisation(Action);
    // The capacity surface does not BOUND this demand: a demand ray piercing no hull face, or a nonzero demand
    // against a declared-zero resistance column. The name states the surface's relation to the demand — the prior
    // `Overcapacity` said the opposite to every design-report reader charting the verdict — and no +∞ sentinel exists.
    public sealed record Unbounded(GoverningAction Action) : Utilisation(Action);
}

// The section-UNDECIDABLE deferrals: a check whose remaining input is member-level DETAILING the cross-section does
// not carry, so the section verdict passes with the named obligation attached instead of failing on a zero column.
// Each row is a real code clause whose missing input is spelled: stirrup/link/bar SPACING, an open-shape warping
// torsion that is not one resistance, or a research-gated in-plane panel verification.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MemberCheckRequirement {
    public static readonly MemberCheckRequirement RcShearReinforcement          = new("rc-shear-reinforcement");            // EC2 §6.2.3(3) V_Rd,s needs the stirrup spacing — the ONE linked-section deferral
    public static readonly MemberCheckRequirement SteelWarpingTorsion           = new("steel-warping-torsion");             // AISC §H3.3 open-shape warping torsion is not a single resistance
    public static readonly MemberCheckRequirement CltInPlaneBending             = new("clt-in-plane-bending");              // EN 1995-1-1 in-plane CLT bending unsettled pending the 2025 revision
    public static readonly MemberCheckRequirement ReinforcedMasonryShearSpacing = new("reinforced-masonry-shear-spacing");  // TMS 402 §9.3.4.1.2 V_ns needs the bar spacing
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
    // StiffnessRetention is the EN 1993-1-2 kE,θ Young's-modulus retention the FIRE lift carries onto the SAME case
    // (1.0 at ambient): stiffness never enters the strength interaction, so it rides as the forward member-stability
    // input a Rasm.Compute fire buckling check reads off the receipt rather than re-deriving from a temperature the
    // verdict no longer carries.
    public sealed record SteelLrfd(
        double FlexuralKnm,
        double FlexuralMinorKnm,
        double CompressionKn,
        double ShearKn,
        double TorsionalKnm,
        CompactnessClass Classification,
        double Slenderness,
        double StiffnessRetention) : SectionCapacity;
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
        // A LINKED section defers stirrup detailing whichever action governs — the §6.2.3(3) V_Rd,s spacing is the ONE
        // obligation, so it rides the shear candidate AND the whole-verdict wrap through the SAME row, never a second
        // spelling of one clause.
        Option<MemberCheckRequirement> linked = e.ShearLinkAreaMm2 > 0.0 ? Some(MemberCheckRequirement.RcShearReinforcement) : None;
        return Worst(
            (cracking, axis, linked),
            (shear, GoverningAction.Shear, linked),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, linked),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, linked));
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
        return Worst(
            (combined, GoverningAction.Combined, Option<MemberCheckRequirement>.None),
            (shear, GoverningAction.Shear, Option<MemberCheckRequirement>.None),
            // An OPEN shape's φTn is engineering-zero because §H3.3 warping torsion is not one resistance, so a
            // torsion demand on it DEFERS to the member check rather than reading as an infinite over-ratio.
            (GuardedRatio(demand.TorsionKnm, s.TorsionalKnm), GoverningAction.Torsion,
                s.TorsionalKnm > 0.0 ? None : Some(MemberCheckRequirement.SteelWarpingTorsion)),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, Option<MemberCheckRequirement>.None));
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
        // BendingMinorKnm is 0.0 only on the research-gated PANEL arm (a member always prices M_Rd,z), so an in-plane
        // panel Mz demand DEFERS to the member check rather than reading as an unbounded over-ratio.
        return Worst(
            (combined, GoverningAction.Combined,
                t.BendingMinorKnm > 0.0 || Math.Abs(demand.MomentZKnm) <= double.Epsilon ? None : Some(MemberCheckRequirement.CltInPlaneBending)),
            (shear, GoverningAction.Shear, Option<MemberCheckRequirement>.None),
            (GuardedRatio(demand.TorsionKnm, t.TorsionalKnm), GoverningAction.Torsion, Option<MemberCheckRequirement>.None),
            (GuardedRatio(demand.BearingKn, t.BearingPerpKn), GoverningAction.Bearing, Option<MemberCheckRequirement>.None));
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
        return Worst(
            (axial + flexure, GoverningAction.Combined, Option<MemberCheckRequirement>.None),
            (tension, GoverningAction.Flexure, Option<MemberCheckRequirement>.None),
            (shear, GoverningAction.Shear, Option<MemberCheckRequirement>.None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, Option<MemberCheckRequirement>.None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, Option<MemberCheckRequirement>.None));
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
        // Vnm alone is the section-decidable shear: Vns needs the bar SPACING, so a shear-governed reinforced verdict
        // DEFERS to the member check rather than reporting a resistance the section cannot complete.
        return Worst(
            (axial + flexure, GoverningAction.Combined, Option<MemberCheckRequirement>.None),
            (GuardedRatio(demand.MomentZKnm, 0.0), GoverningAction.Flexure, Option<MemberCheckRequirement>.None),
            (demand.ShearResultantKn / Math.Max(phiV * vnm, double.Epsilon), GoverningAction.Shear,
                Some(MemberCheckRequirement.ReinforcedMasonryShearSpacing)),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, Option<MemberCheckRequirement>.None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, Option<MemberCheckRequirement>.None));
    }

    // EN 16612 pane check per metre strip: BOTH plate bending directions fold against the SAME isotropic per-metre
    // resistance, their SUM the conservative combined-stress bound; in-plane axial, shear, torsion, and bearing are
    // unresisted at pane altitude and govern loud through GuardedRatio.
    static Utilisation GlassUtilisation(GlassPane g, Demand demand) =>
        Worst(
            ((Math.Abs(demand.MomentYKnm) + Math.Abs(demand.MomentZKnm)) / Math.Max(g.BendingKnmPerM, double.Epsilon), GoverningAction.Flexure, Option<MemberCheckRequirement>.None),
            (GuardedRatio(demand.AxialKn, 0.0), GoverningAction.Axial, Option<MemberCheckRequirement>.None),
            (GuardedRatio(demand.ShearResultantKn, 0.0), GoverningAction.Shear, Option<MemberCheckRequirement>.None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, Option<MemberCheckRequirement>.None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, Option<MemberCheckRequirement>.None));

    // The connection verdict over the load path's three resisted axes: the shear resultant against the lifted line/
    // area/group shear, a POSITIVE axial (tension, uplift) against the tension column, and the seat reaction (a
    // hanger's download) against the bearing column; a compressive axial rides the member, and moments/torsion are
    // unresisted at connection altitude and govern loud — the connector's private DemandRatio mini-rail collapses
    // onto this one fold, its direction vocabulary living on in the lift columns.
    static Utilisation ConnectionUtilisation(Connection c, Demand demand) =>
        Worst(
            (GuardedRatio(demand.ShearResultantKn, c.ShearKn), GoverningAction.Shear, Option<MemberCheckRequirement>.None),
            (GuardedRatio(Math.Max(demand.AxialKn, 0.0), c.TensionKn), GoverningAction.Axial, Option<MemberCheckRequirement>.None),
            (GuardedRatio(demand.BearingKn, c.BearingKn), GoverningAction.Bearing, Option<MemberCheckRequirement>.None),
            (GuardedRatio(demand.MomentResultantKnm, 0.0), GoverningAction.Flexure, Option<MemberCheckRequirement>.None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, Option<MemberCheckRequirement>.None));

    // The worst (largest) ratio over the candidate span — the unified governing-axis fold every arm drives, so a
    // steel/timber/masonry check reports WHICH action governs, not just a ratio; the span-params buffer stack-allocates
    // per Check, and the strict-greater fold keeps the earliest-maximal tie-break without a per-call array. Each
    // candidate carries its own DEFERRAL: a capacity column that is zero because the code clause is section-undecidable
    // (an open shape's warping torsion, a research-gated CLT in-plane bending, the reinforced-masonry V_ns spacing)
    // names its MemberCheckRequirement, so a governing deferral folds to RequiresMemberCheck WITH its ratio instead of
    // reading as an unbounded verdict — the distinction between "no resistance" and "resistance the section cannot
    // finish computing" survives to the design report.
    static Utilisation Worst(params ReadOnlySpan<(double Ratio, GoverningAction Action, Option<MemberCheckRequirement> Defer)> candidates) {
        (double ratio, GoverningAction action, Option<MemberCheckRequirement> defer) =
            Iterable<(double Ratio, GoverningAction Action, Option<MemberCheckRequirement> Defer)>.FromSpan(candidates[1..])
                .Fold(candidates[0], static (best, next) => next.Ratio > best.Ratio ? next : best);
        return double.IsFinite(ratio)
            ? defer.Match(
                Some: owed => (Utilisation)new Utilisation.RequiresMemberCheck(ratio, action, owed),
                None: () => new Utilisation.Bounded(ratio, action))
            : new Utilisation.Unbounded(action);
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
            r.Capacity.ShearN * 1e-3, r.Capacity.TorsionalNmm * 1e-6, r.Capacity.Classification, r.Capacity.Slenderness,
            StiffnessRetention: 1.0),
        timber: static r => new TimberEc5(
            r.Capacity.BendingNmm * 1e-6, r.Capacity.BendingMinorNmm * 1e-6, r.Capacity.CompressionN * 1e-3,
            r.Capacity.ShearN * 1e-3, r.Capacity.BearingPerpN * 1e-3, r.Capacity.TorsionalNmm * 1e-6,
            r.Capacity.RelativeSlenderness, r.Capacity.Km, r.Capacity.Kmod),
        // The deck's AISI receipt lands the SAME SteelLrfd case — one cold-formed verdict shape for a stud and a
        // sheet; only the receipt KIND distinguishes them for the report and the analytics dimension.
        deckSheet: static r => new SteelLrfd(
            r.Capacity.FlexuralNmm * 1e-6, r.Capacity.FlexuralMinorNmm * 1e-6, r.Capacity.CompressionN * 1e-3,
            r.Capacity.ShearN * 1e-3, r.Capacity.TorsionalNmm * 1e-6, r.Capacity.Classification, r.Capacity.Slenderness,
            StiffnessRetention: 1.0),
        // The slenderness reduction MINTS here off the carried height and the section's own governing radius of
        // gyration — the TMS 402 bracket is the MasonryReduction owner's derivation, so no caller re-derives it and a
        // transposed branch is unrepresentable.
        masonry: static r => new MasonryCompression(
            r.Strength.FmMpa, r.Section.AreaMm2.Value, r.Section.SxMm3.Value, r.Section.SyMm3.Value,
            MasonryReduction.Of(r.HeightMm, r.Section.GoverningRadiusMm).Value, r.Rupture.FrMpa(r.System, r.Mortar)),
        reinforcedMasonry: static r => new MasonryReinforced(
            r.Strength.FmMpa, r.Bar.MinimumYieldMpa,
            r.Unit.ReinforcedCells * Math.PI / 4.0 * r.Unit.RebarBarMm * r.Unit.RebarBarMm,   // As off the lattice facts
            r.Section.AreaMm2.Value, r.Unit.WMm / 2.0, r.Unit.LMm,                            // d = W/2 mid-wall bars, b the bed length
            MasonryReduction.Of(r.HeightMm, r.Section.GoverningRadiusMm).Value),
        glass: static r => new GlassPane(r.Capacity.BendingKnmPerM, r.Capacity.ResistanceMpa, r.Capacity.EffectiveThicknessMm),
        // The EN 1993-1-2 accidental situation: ky,θ scales every STRENGTH column (flexure both axes, compression,
        // shear, torsion), kE,θ rides the StiffnessRetention column for the forward member-stability check, and the
        // ambient classification/slenderness carry unchanged — the section's geometry does not char.
        steelFire: static r => new SteelLrfd(
            r.Ambient.FlexuralNmm * r.Ky * 1e-6, r.Ambient.FlexuralMinorNmm * r.Ky * 1e-6, r.Ambient.CompressionN * r.Ky * 1e-3,
            r.Ambient.ShearN * r.Ky * 1e-3, r.Ambient.TorsionalNmm * r.Ky * 1e-6, r.Ambient.Classification, r.Ambient.Slenderness,
            StiffnessRetention: r.Ke),
        // The EN 1995-1-2 residual section is already priced at kmod = γM = 1.0 by the timber owner, so the fire arm
        // lifts it verbatim — the charring is geometry, never a factor applied here.
        timberFire: static r => new TimberEc5(
            r.Residual.BendingNmm * 1e-6, r.Residual.BendingMinorNmm * 1e-6, r.Residual.CompressionN * 1e-3,
            r.Residual.ShearN * 1e-3, r.Residual.BearingPerpN * 1e-3, r.Residual.TorsionalNmm * 1e-6,
            r.Residual.RelativeSlenderness, r.Residual.Km, r.Residual.Kmod),
        weld: static r => new Connection(r.Row.DirectionalShearKn(Angle.FromDegrees(r.LoadAngleDeg)), 0.0, 0.0),
        adhesive: static r => new Connection(r.Row.DesignShearKn, 0.0, 0.0),
        stud: static r => new Connection(Math.Max(r.Count, 0) * r.Row.DesignShearKn, 0.0, 0.0),
        // The seat-borne download is Cd-exempt (steel-governed bearing); the fastener-transferred uplift/lateral
        // columns scale by the connector's own admitted duration factor.
        connector: static r => new Connection(r.Capacity.LateralKn * r.Capacity.Cd, r.Capacity.UpliftKn * r.Capacity.Cd, r.Capacity.DownloadKn));

    // The persisted hull cache, realized: Freeze writes the eagerly-solved mesh through the ITaxonomySerializable
    // marker IForceMomentMesh itself extends ($type-tagged Newtonsoft wire, UnitsNet SI-scalar+unit quantities) as the
    // BODY of a Rasm.Persistence artifact row content-keyed on (ComponentId, DiagramResolution.Key) — the same
    // content-key edge the raster estate crosses; Thaw rehydrates the exact ForceMomentMesh via the $type tag WITHOUT
    // re-running the Steps² sweep, trapping the FromJson null/throw onto ComponentFault.Capacity. The key pair is the
    // whole preimage: the section identity and the resolution are the only inputs the eager solve reads, so a
    // resolution change is a distinct row rather than a stale hit. Producer=consumer ONLY — TypeNameHandling.Objects
    // is a deserialization-gadget surface, so Thaw is fed exclusively the artifact a trusted Freeze wrote, never a
    // peer document; the composition root owns the store handle, this owner the two projections.
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
            : toSeq(toSeq(hull.Faces)
                    .Map(face => Pierce(face, demand.AxialKn, demand.MomentYKnm, demand.MomentZKnm))
                    .Somes()
                    .Filter(static multiplier => multiplier > 0.0)
                    .OrderBy(static multiplier => multiplier))
                .Head
                .Match(Some: static multiplier => 1.0 / multiplier, None: static () => double.PositiveInfinity);
        return Worst(
            (ray, governing, Option<MemberCheckRequirement>.None),
            (GuardedRatio(demand.ShearResultantKn, 0.0), GoverningAction.Shear, Option<MemberCheckRequirement>.None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, Option<MemberCheckRequirement>.None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, Option<MemberCheckRequirement>.None));
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

// The INVERSE of Check — design as selection over two search spaces under ONE law: `Lightest` scans the frozen
// catalogue (a section someone stocked) and `Fabricated` sweeps a caller-parameterized composition space (a section
// nobody stocked yet), both ranking by REAL linear mass and both accepting on the SECTION-altitude verdict so a
// deferring section returns WITH its member-check obligation rather than being silently rejected.
//
// Mass is `AreaMm2 × ρ(substance)`, never area alone: area ranks linear mass only INSIDE one substance, so a mixed
// steel/timber/masonry catalogue ordered by area returns a 90 mm sawn section ahead of every W-shape. The density
// arrives through the caller's `densityOf` projection (the composing root binds it to
// `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Lookup(id, key)`'s Mechanical density column), so `admit`
// reverts to a genuine POLICY filter — a stocked subset, a depth cap, one `SteelClass` — rather than a correctness
// precondition the type system never enforced. The per-candidate capacity arrives through `capacityOf` because the
// family capacity needs placement facts — unbraced/effective lengths, service class, duration — the catalogue does
// not carry. A candidate whose density or capacity FAULTS aborts the scan loud (a filter admitting a family the
// projections cannot price is a caller defect, never a silently skipped row); an exhausted search faults typed.
public static class SectionSelection {
    public static Fin<(Component Section, Utilisation Verdict)> Lightest(
        FrozenDictionary<ComponentId, Component> rows,
        FrozenDictionary<ComponentId, ComputedSection> sections,
        Demand demand,
        Func<Component, ComputedSection, Fin<SectionCapacity>> capacityOf,
        Func<MaterialId, Fin<double>> densityOf,
        Func<Component, bool> admit,
        Op key) =>
        toSeq(sections)
            .Filter(pair => rows.ContainsKey(pair.Key) && admit(rows[pair.Key]))
            .Traverse(pair => densityOf(rows[pair.Key].SubstanceId)
                .Map(density => (Row: rows[pair.Key], Section: pair.Value, MassPerMm: pair.Value.AreaMm2.Value * density)))
            .As()
            .Map(static ranked => toSeq(ranked.OrderBy(static candidate => candidate.MassPerMm)))
            .Bind(ranked => Least(ranked.Map(static c => (c.Row, c.Section)), demand, capacityOf, key));

    // The GENERATIVE counterpart: `SectionProfile.BuiltUp` plus the `component#SECTION_SOLVER` `Composed` arm already
    // price an arbitrary positioned member set exactly (parallel-axis inertias, the equal-area plastic pair, summed
    // J and shear areas), so a caller-supplied parameterized sweep — a plate-girder web-depth × flange-width lattice,
    // a battened-column spacing sweep — folds through the SAME solve, the SAME Check, and the SAME acceptance as a
    // catalogue row. The generator is indexed so the sweep is a pure function of its own ordinal (replayable, and a
    // caller may cap it without a mutable cursor), and every candidate is solved HERE because a fabricated section
    // has no catalogue entry to have solved it. This turns the utilisation rail from a catalogue query into a
    // fabricated-member design tool at the cost of one fold.
    public static Fin<(Component Section, Utilisation Verdict)> Fabricated(
        Func<int, Seq<(Component Row, SectionProfile.BuiltUp Profile)>> candidates,
        int sweeps,
        Demand demand,
        Func<Component, ComputedSection, Fin<SectionCapacity>> capacityOf,
        Func<MaterialId, Fin<double>> densityOf,
        Op key) =>
        toSeq(Enumerable.Range(0, Math.Max(sweeps, 0))).Bind(candidates)
            .Traverse(candidate => SectionSolver.Solve(candidate.Profile, key)
                .Bind(section => densityOf(candidate.Row.SubstanceId)
                    .Map(density => (candidate.Row, Section: section, MassPerMm: section.AreaMm2.Value * density))))
            .As()
            .Map(static ranked => toSeq(ranked.OrderBy(static candidate => candidate.MassPerMm)))
            .Bind(ranked => Least(ranked.Map(static c => (c.Row, c.Section)), demand, capacityOf, key));

    // The ONE acceptance fold both search spaces drive: the first mass-ordered candidate whose verdict PASSES AT
    // SECTION ALTITUDE wins and carries its verdict verbatim, so a linked RC section that passes and merely owes
    // stirrup detailing returns WITH its deferral for the caller to route forward — the strict `Adequate` bit stays
    // the terminal report's, never the sizing gate's.
    static Fin<(Component Section, Utilisation Verdict)> Least(
        Seq<(Component Row, ComputedSection Section)> ranked,
        Demand demand,
        Func<Component, ComputedSection, Fin<SectionCapacity>> capacityOf,
        Op key) =>
        ranked.Fold(
                Fin.Succ(Option<(Component Section, Utilisation Verdict)>.None),
                (state, candidate) => state.Bind(found => found.IsSome
                    ? Fin.Succ(found)
                    : capacityOf(candidate.Row, candidate.Section)
                        .Map(capacity => capacity.Check(demand))
                        .Map(verdict => verdict.SectionPasses
                            ? Some((candidate.Row, verdict))
                            : Option<(Component, Utilisation)>.None)))
            .Bind(found => found.ToFin(ComponentFault.Capacity(key, "<selection-no-adequate-section>")));
}
```

## [03]-[RESEARCH]

(none)
