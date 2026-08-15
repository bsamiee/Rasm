# [MATERIALS_CAPACITY]

THE SECTION-CAPACITY OWNER and THE ONE UTILISATION RAIL. One `SectionCapacity` `[Union]` is the closed structural-capacity surface a `Component` cross-section carries beyond its elastic `ComputedSection`, and one `Demand` folded against it through `Check` is the typed `Utilisation` verdict — so EVERY family's design check is one polymorphic fold differing only in the capacity case, never a per-family `RcColumnCheck`/`SteelBeamCheck`/`MasonryWallCheck` surface. The closed case set spans the realized `ComponentFamily` structural rails: `RcInteraction` (the ultimate biaxial Force-Moment-Moment capacity hull `VividOrange.InteractionDiagram` welds over the `reinforcement#RC_SECTION` `IConcreteSection`), `RcElastic` (the elastic transformed-section reinforcement properties `VividOrange.Sections.SectionProperties` `ConcreteSectionProperties` computes over the same section, AND the EC2 §6.2 section-level shear screen over the bottom-face tension steel and the two-leg link area `CrossSectionalShearReinforcementArea` carries), `SteelMember` (the `steel#STEEL_FAMILY` `DesignCapacity` design-resistance receipt lifted whole under the basis it names — the AISC 360 `φMn`/`φMny`/`φPn`/`φVn` or the EN 1993-1-1 `χ`/`χLT` γM-divided resistances — with `CompactnessClass`/slenderness; the AISI deck receipt and the EN 1993-1-2 fire state land the same case), `TimberMember` (the EN 1995-1-1 `timber#TIMBER_CAPACITY` `TimberCapacity` design-resistance receipt lifted whole — `M_Rd,y`/`M_Rd,z` per axis with the §`6.1.6`(2) `k_m` weight), and `MasonryUnreinforced` (the axial-flexural unity check AND the flexural-tension screen — the TMS 402 §`9.2.2` Table `9.1.9.2` `fr` or the EN 1996-1-1 §`6.3` Table `3.4` `f_xk` the basis selects — over the `cmu#CMU_FAMILY` `CmuStrength` `f'm` + grouted `ComputedSection` + the `masonry#MASONRY_FAMILY` mortar-keyed row feed). Every case binds a `DesignBasis`: the JURISDICTION axis carrying the authority body, the `SafetyFormat`, the γM0/γM1 partial factors, the `NationalAnnex`-threaded typed `IStandard` citation, and the interaction kernel `Check` dispatches — so a second design code for an already-cased family is a BASIS ROW, never a sibling case forking the closed `GoverningAction`/`Utilisation` verdict vocabulary the `Rasm.Compute/Analysis/structural#DESIGN_CHECK` consumer keys on, and a case name spelling one code is the deleted form. A capacity is admitted to the family ONLY when no existing case's column set carries it: each sibling family page that hand-rolls its design rules (`steel#STEEL_FAMILY`, `timber#TIMBER_CAPACITY`, `cmu#CMU_FAMILY`) lifts its already-computed receipt into ONE case here, and the RC cases are the two `Resolve` builds over the section input — the design-code COMPUTATION stays the family owner's, the unified VERDICT this owner's. The rail is TOTAL over the load path: `MasonryReinforced` carries the TMS 402 §9.3 steel-couple arm over the cmu lattice facts, `GlassPane` the EN 16612 pane resistance the glazing family lifts, `Connection` the weld/adhesive/stud/connector/anchor receipts, `AluminumMember` the EN 1999-1-1 elastic-floor resistances over the banded (fo, fu) pair the aluminum family proves at seed time with the §6.3.1 buckling-curve columns published for the forward stability check, `Fatigue` the ONE detail-category S-N law spanning the EN 1993-1-9 fourteen-rung ladder and the AISC 360 Appendix 3 A–E′ constants, and `BasePlate` the AISC DG1 bearing/plate-thickness pair — one `Check` from cross-section to weld to hanger to anchor — while `SectionSelection.Lightest` and `SectionSelection.Fabricated` are the rail's INVERSE queries, the least-MASS section-passing scan over the frozen catalogue maps the full-database steel seed supplies and over a caller-parameterized `SectionProfile.BuiltUp` composition sweep. This owner is the ULTIMATE complement to `component#COMPONENT_OWNER` `SectionSolver`: that solver gives the elastic `ComputedSection` every family solves from its `SectionProfile` arm, THIS owner gives the reinforced-section transformed properties, the EC2 section-level shear screen, the ultimate capacity hull, and the unified utilisation fold the elastic solver does not. The `InteractionDiagram` constructor RUNS the full eager fibre-integration solve at construction (the `Triangle` section mesh, the `Parallel.For` strain-plane sweep, the `MIConvexHull` hull weld are encapsulated `internal` — this owner composes the welded `IForceMomentMesh`, never the meshing primitive), so a design page constructs the capacity ONCE per section/settings and reads `diagram.Mesh` cached, never re-solving per query. The page composes `reinforcement#RC_SECTION` `RcSection`/`IConcreteSection` for the RC input, `VividOrange.InteractionDiagram` (`InteractionDiagram`/`DiagramSettings`/`IForceMomentMesh`) for the N-M-M hull, `VividOrange.Sections.SectionProperties` `ConcreteSectionProperties` for the elastic transformed-section properties, `VividOrange.Materials` `EnConcreteFactory` for the EC2 `fck` the cracking reference reads, the `steel#STEEL_FAMILY` `DesignCapacity` / `timber#TIMBER_CAPACITY` `TimberCapacity` / `cmu#CMU_FAMILY` `CmuStrength` sibling receipts, the in-folder `UnitsNet` `Force`/`Torque`/`Area`/`Length` quantity coercion at the edge, and the `component#COMPONENT_OWNER` `ComponentFault` band-2300 rail (the SAME component-sub-domain fault every sibling Component family page rails — NOT a borrowed appearance band) for a non-finite, degenerate, or infeasible solve; the capacity surface and the utilisation verdict feed the forward `Rasm.Compute/Analysis/structural#DESIGN_CHECK` structural-Assessment route by `MaterialId`/section key, host-neutral here, the `IForceMomentMesh` round-tripping through the realized `SectionCapacity.Freeze`/`Thaw` `VividOrange.Serialization` pair into a `Rasm.Persistence` artifact row content-keyed on `(ComponentId, DiagramResolution.Key)` — the eager `Steps²` solve is paid once, persisted, and rehydrated across processes, never re-run.

## [01]-[INDEX]

- [02]-[SECTION_CAPACITY]: the `SectionCapacity` `[Union]` (`RcInteraction` N-M-M hull · `RcElastic` transformed-section · `SteelMember` rolled/cold-formed/deck/fire/stainless steel · `TimberMember` EC5 receipt · `AluminumMember` EN 1999 extrusion · `MasonryUnreinforced` axial-flexural + flexural-tension · `MasonryReinforced` reinforced steel-couple · `GlassPane` EN 16612 pane · `Connection` weld/adhesive/stud/connector/anchor load path · `LateralPanel` sheathed in-plane unit shear · `Fatigue` detail-category S-N · `BasePlate` DG1 bearing/plate-bending) over the `component#COMPONENT_OWNER` `ComponentFault` band-2300 rail, the `SafetyFormat` resistance-factor/partial-factor/allowable axis and the `DesignBasis` `[SmartEnum]` jurisdiction policy row with its `NationalAnnex`-threaded `Standard` citation and its `Interact` kernel over the `InteractionOperands` normalized ratios, the `FatigueAssessment` γMf grid with the `EnFatigueCategory`/`AiscFatigueCategory` ladders folded by the `FatigueLaw` `[Union]`, the `CapacityBuild` RC-build request `[Union]` (hull · elastic — the hull arm alone carrying its `DiagramResolution`), the `DiagramResolution` `[SmartEnum]` mesh/sweep-refinement policy folding to a `DiagramSettings`, the `Demand` applied-action shape (axial · biaxial moment · biaxial shear · torsion · bearing · in-plane unit shear · fatigue range/count), the `GoverningAction` `[SmartEnum]` verdict axis, the `LateralHazard` SDPWS §4.1.4 reduction policy, the `Utilisation` typed verdict, the `MemberCheckRequirement` section-undecidable deferral vocabulary, the `AnchorBed`/`PlateBed` placement-declaration records, the `CapacityReceipt` sibling-receipt request `[Union]` (one case per already-computed family receipt, spanning the member, fire, connection, and fastener modalities — each carrying its full lift context, so the roster grows with the families and is read off the union rather than restated here), and the `SectionCapacity.Resolve` eager-solve boundary with the ONE TOTAL `Lift(CapacityReceipt)` entry and the `Freeze`/`Thaw` content-keyed hull-artifact round-trip — every boundary static on the union owner, no satellite resolver class — and the `SectionSelection.Lightest`/`Fabricated` inverse sizing folds over the frozen catalogue maps and the fabricated composition sweep.

## [02]-[SECTION_CAPACITY]

- Owner: `SectionCapacity` is the closed capacity family spanning the member rails and the connection load path; `DesignBasis` is the jurisdiction row every case binds — authority body, `SafetyFormat`, γM0/γM1, the annex-threaded `IStandard` citation, and the `Interact(InteractionOperands)` combined-action kernel — so a second code over an already-cased family is one ROW; `Demand` admits the signed action vector; `Utilisation` distinguishes a bounded verdict, a section pass owing a named member check, and an UNBOUNDED verdict (the capacity surface does not bound the demand), projecting the strict `Adequate` acceptance bit, the section-altitude `SectionPasses` bit the sizing folds select on, and the optional `Ratio` every downstream reader charts against; `MemberCheckRequirement` closes the section-undecidable deferral vocabulary; `MasonryReduction` OWNS the TMS 402 stability bracket as a derivation over `(height, radius of gyration)`; `FatigueLaw` OWNS the two-ladder S-N algebra (the `EnFatigueCategory` fourteen-rung EN 1993-1-9 set under the `FatigueAssessment` γMf grid, the `AiscFatigueCategory` A–E′ constants) as one closed family; `AnchorBed`/`PlateBed` carry the placement declarations the anchor and base-plate lifts consume; `CapacityBuild` and `CapacityReceipt` carry solve and lift modality, and `CapacityReceipt.Kind` owns the case-name projection every signal dimension and analytics column keys on, so a reflected runtime type name at a consumer has no reason to exist.
- Cases: `RcInteraction` (the ultimate biaxial N-M-M capacity hull as the `IForceMomentMesh` over an `IConcreteSection`, `VividOrange.InteractionDiagram`) · `RcElastic` (the elastic section state read off the ONE `ConcreteSectionProperties` carrier the `RcSection` receipt holds — `TotalReinforcementArea`/`ConcreteArea`/`GeometricReinforcementRatio`, the GROSS `MomentOfInertiaYy`/`Zz` (the inherited base polygon integral — the SLS fibre divisors) AND the `ReinforcementSecondMomentOfAreaYy`/`Zz` `Σ(As·d²)` steel moments (the cracked-`Icr` readout), + the bottom-face `EffectiveDepth(SectionFace)` ULS lever + the bottom-face `ReinforcementArea(SectionFace)` tension steel and the two-leg `CrossSectionalShearReinforcementArea` link area + the gross depth AND width (the major/minor-axis SLS extreme-fibre levers) + the parsed `fck` and its EC2 `fctm` cracking limit, the combined `N/A ± My·cy/Iyy ± Mz·cz/Izz` SLS check AND the EC2 §6.2 shear screen) · `SteelMember` (the rolled/composite/cold-formed `steel#STEEL_FAMILY` `DesignCapacity` resistance columns + `CompactnessClass` + slenderness + the §6.3.1/§6.3.2 `χ`/`χLT` reductions lifted WHOLE under the receipt's own basis — the minor column the per-axis combined fold divides against) · `TimberMember` (the EN 1995-1-1 `timber#TIMBER_CAPACITY` `TimberCapacity` `M_Rd,y`/`M_Rd,z`/`N_Rd`/`V_Rd`/`R_90,Rd` + `λ_rel` + `k_m` + `k_mod` lifted WHOLE — the member minor column `k_h(w)`-scaled with no `k_crit`, the panel minor the net-section in-plane arm, zero where its form declares no edgewise strength) · `MasonryUnreinforced` (the axial-flexural check + the flexural-tension screen the `cmu#CMU_FAMILY` `CmuStrength` `f'm` + the grouted `ComputedSection` net area AND both net moduli `SxMm3`/`SyMm3` + slenderness reduction + the basis-selected `masonry#MASONRY_FAMILY` tension source — TMS 402 Table `9.1.9.2` `fr` or EN 1996-1-1 Table `3.4` `f_xk` — feed) — and `MasonryReinforced` (the reinforced steel-couple arm over the cmu lattice's `ReinforcedCells`/`RebarBarMm`/grouted-net facts and the bar grade's yield), `GlassPane` (the EN 16612 governing-pane per-metre resistance the `glazing#GLAZING_FAMILY` `GlassCapacity` receipt lifts WHOLE), `Connection` (the `joint#JOINT_FAMILY` weld/adhesive/stud design values, the `connector#CONNECTOR_FAMILY` duration-governed capacity, and the EN 1992-4 cast-in anchor's cone/steel minimum as one shear/tension/bearing column triple with its forward-modes deferral), `AluminumMember` (the EN 1999-1-1 class-3 elastic-floor resistances computed at lift over the `aluminum#ALUMINUM_FAMILY` banded (fo, fu) pair — the ONE family whose design algebra lives here because no aluminium producer exists among admitted packages — with the §6.3.1.2 Table 6.6 α/λ̄0 curve columns per `BucklingClass` letter), `Fatigue` (the `FatigueLaw` detail-category S-N surface — the EN 1993-1-9 ladder under its `FatigueAssessment` γMf or the AISC Appendix 3 Cf/FTH constants — checked against the `Demand` range/count pair), and `BasePlate` (the AISC DG1 §J8 bearing and cantilever plate-bending pair precomputed from the `PlateBed`) — the closed structural-capacity family across steel/stainless/RC/timber/aluminium/masonry/glass, the connection-and-anchorage load path, and the cyclic and base-connection checks; a capacity is a `SectionCapacity` case over a section or connection receipt, never a per-section-type check.
- Entry: `SectionCapacity.Resolve(RcSection, CapacityBuild, Op)` dispatches the RC solve request; the TOTAL `SectionCapacity.Lift(CapacityReceipt)` dispatches every already-computed sibling receipt — steel (carbon and stainless alike, the basis column telling them apart), timber, steel deck, masonry, reinforced masonry, glass, the two fire modalities, the aluminium die, the fatigue law, the cast-in anchor, the base plate, and the connection kinds; internal `Freeze`/`Thaw` persist the content-keyed hull artifact; and `Check(Demand)` returns the closed `Utilisation` verdict. The masonry receipts carry the member HEIGHT as a kernel-admitted `PositiveMagnitude` beside their section, so `Lift` mints the stability reduction from the section's own governing radius — no caller-supplied stability scalar and no re-derived code bracket exists. `SectionSelection.Lightest` and `SectionSelection.Fabricated` are the inverse queries over the frozen catalogue and a caller-parameterized composition sweep. The `RcInteraction` arm casts the raw `(N, My, Mz)` demand vector against the hull and interprets the smallest positive intersection parameter as the capacity multiplier; utilization is its reciprocal. Force and moment axes are never Euclidean-normalized together.
- Packages: VividOrange.InteractionDiagram (`InteractionDiagram`/`DiagramSettings`, the eager-solve ctor + `Mesh`; `.api/api-vividorange-interactiondiagram.md`), VividOrange.IForceMomentInteraction (`IForceMomentMesh`/`IForceMomentVertex`/`IForceMomentTriFace` the hull read through, the `Faces`/`A`/`B`/`C`/`X`/`Y`/`Z` `Force`/`Torque` members; `.api/api-vividorange-iforcemomentinteraction.md`), VividOrange.Sections.SectionProperties (`ConcreteSectionProperties` the transformed-section carrier RIDING the `RcSection` receipt — the `EffectiveDepth(SectionFace)`/`ReinforcementArea(SectionFace)` face queries, the `CrossSectionalShearReinforcementArea` two-leg link area, and the inherited base `MomentOfInertiaYy`/`Zz` gross polygon integral the SLS fibre divisors read; `.api/api-vividorange-sections-sectionproperties.md`), VividOrange.Sections (`IConcreteSection`/`SectionFace` from the `reinforcement#RC_SECTION` `RcSection`; `.api/api-vividorange-sections.md`), VividOrange.Materials (`EnConcreteFactory.CreateLinearElastic` whose `LinearElasticMaterial.Strength` IS the parsed `fck` — decompile-verified: the factory parses the first `Cxx` token of the grade, so `Strength.Megapascals` is the characteristic cylinder strength the EC2 `fctm` AND the §6.2 shear screen read; `.api/api-vividorange-materials.md`), VividOrange.Serialization (`JsonSerializationExtensions.ToJson`/`FromJson<T>` `where T : ITaxonomySerializable` — the `Freeze`/`Thaw` content-keyed hull artifact over the marker `IForceMomentMesh` itself extends, `$type`-tagged Newtonsoft wire + `UnitsNet` SI-scalar+unit quantities, producer=consumer only; `.api/api-vividorange-serialization.md`), UnitsNet (`Force.Kilonewtons`/`Torque.KilonewtonMeters`/`Area`/`Length`/`Ratio`/`Angle` coerced at the edge; `libs/csharp/.api/api-unitsnet.md`), Rasm.Element (project — `MaterialId`/`ProfileRef` the seam-carried identity, seam-canonical), Rasm (project — `PositiveMagnitude` from `Rasm.Numerics`, `Op`/`Context` from `Rasm.Domain`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Fold`), VividOrange.Standards (`IStandard` the typed citation every `DesignBasis` row projects, `En1992`/`En1993`/`En1994`/`En1995`/`En1996`/`En1999` with their `En19xxPart` partitions — `Part1_4`/`Part1_9` decompile-verified `En1993Part` members, `En1992Part` shipping NO `Part4` — and the `NationalAnnex` axis the row threads; `.api/api-vividorange-standards.md`), Thinktecture.Runtime.Extensions (`[Union]` for `SectionCapacity`/`CapacityBuild`/`CapacityReceipt`, `[SmartEnum]` for `DiagramResolution`/`GoverningAction`/`MemberCheckRequirement`/`SafetyFormat`/`DesignBasis`, `[UseDelegateFromConstructor]` for the `DesignBasis` `Standard`/`Interact` kernel columns). Triangle + MIConvexHull ride transitively INSIDE the `InteractionDiagram` engine (encapsulated `internal`, `.api/api-triangle.md` `[LOCAL_ADMISSION]` / `.api/api-vividorange-forcemomentinteraction.md`) — this owner mints NO direct mesher/hull call, composing only the welded `IForceMomentMesh`. The `steel#STEEL_FAMILY` `DesignCapacity`, `timber#TIMBER_CAPACITY` `TimberCapacity`, and `cmu#CMU_FAMILY` `CmuStrength` are sibling-page receipts lifted, never re-computed.
- Growth: a SECOND design code over an already-cased family is one `DesignBasis` row — its body, format, partial factors, annex-threaded citation, and `Interact` kernel — and the owning family page's per-basis resistance arm, NEVER a sibling `SectionCapacity` case (a per-code case forks the closed `GoverningAction`/`Utilisation` vocabulary the forward design-check consumer keys on, and the case set is cut by structural FAMILY and MODALITY alone); a new structural family's capacity is one `SectionCapacity` `[Union]` case binding either a `Resolve` build (a section-input solve) or a lift factory (an already-computed sibling receipt) and one `Check` arm — a moment-curvature `RcInteraction` refinement, a panel diaphragm unit-shear check — admitted only when no existing case's column set carries it; a new demand axis is one `Demand` column (a warping bimoment, a second-order P-Δ amplifier); a new utilisation metric one `Utilisation`/`GoverningAction` projection — never a per-section-type capacity surface, never a re-derived elastic property where `ConcreteSectionProperties` computes it, never a direct `Triangle`/`MIConvexHull` call where the `InteractionDiagram` engine welds the hull; a persisted-capacity need is the one `Freeze`/`Thaw` pair over the `ITaxonomySerializable` marker, never a second serializer; the `steel`/`timber`/`cmu`/`panel` design receipts stay the family-owner derivation lifted here, never re-computed — the fire modality is that law EXECUTED, two `CapacityReceipt` cases lifting the landed `SteelDesign` `FireRetention` retention pair and the timber `ResidualStack` charred receipt onto the existing verdict cases, the stainless modality the BASIS-ROW law executed (an `en1993-1-4` receipt lands the shared `SteelMember` case, never a sibling), and the aluminium/fatigue/anchorage/base-plate landings the new-case and new-demand-column arms executed in one pass.
- Boundary: `SectionCapacity.Resolve` and `Check` are the `Projection/observability#SIGNAL_FACTS` `MaterialsFact.CapacityCheck(Key, Receipt, Verdict, Elapsed)` tap SUBJECTS and `Check` is the `Projection/benchmarks#BENCH_CORPUS` `BenchKernel.InteractionSweep` measured kernel; the tap is a composition-root decorator over `MaterialsHooks.CapacityCheck`, so this owner emits nothing, carries no `Duration`, and references no signal type — the seam is declared at both ends and instrumented at neither, and `CapacityReceipt.Kind` is the one dimension spelling both the fact stream and the analytics column key on.
- Boundary: the frozen hull RESIDES in `Rasm.Persistence` as an artifact row content-keyed on `(ComponentId, DiagramResolution.Key)` — the `Rasm.Materials/ARCHITECTURE.md` `[CONTENT_KEY]: ArtifactIndexRow` edge the raster estate already crosses, reused verbatim rather than minted a second time. `Freeze` writes that row and `Thaw` is fed EXCLUSIVELY from that store, so the eager `Steps²` solve is paid once per `(section, resolution)` pair and rehydrated across processes; a process-local memo re-pays the sweep on every load and carries none of the claim this page states. The store is the ONLY `Thaw` ingress: the `TypeNameHandling.Objects` `$type` wire is a deserialization-gadget surface, so no peer document reaches it.
- Boundary: `SectionCapacity.Resolve` is the BOUNDARY_ADMISSION point where the `VividOrange.InteractionDiagram` engine is admitted EXACTLY ONCE and the `ConcreteSectionProperties` carrier — admitted once at `RcSectionBuilder.Of`, riding the `RcSection` receipt — is READ, never re-constructed — the `InteractionDiagram` ctor runs the expensive eager solve (`.api/api-vividorange-interactiondiagram.md` `[TOPOLOGY]`) and a non-EN material whose `IEnConcreteMaterial`/`IEnRebarMaterial` cast the engine cannot read, an under-reinforced degenerate section, or a hull-weld failure rails `ComponentFault.Capacity` (the component-sub-domain band 2300 — `FaultBand.Component` on the registry — the dedicated capacity-solve slot distinct from the `Section` elastic-integral slot `component#COMPONENT_OWNER` `SectionSolver.Admit` rails, both band 2300 with their Component siblings, NOT the `Appearance/bsdf#SHADING_FRAME` `MaterialFault` band 2450) rather than throwing, so no `VividOrange` throw and no infeasible hull reaches an interior signature; the `IForceMomentMesh` is read THROUGH its interface floor (`.api/api-vividorange-iforcemomentinteraction.md` `[LOCAL_ADMISSION]`), never the `ForceMomentMesh` concrete, and the `Force`/`Torque` hull coordinates carry as `UnitsNet` quantities coerced to SI base (`Force.Kilonewtons`/`Torque.KilonewtonMeters`) once at the edge so no interior signature carries the hull as raw `double`; the `Triangle` section mesher and the `MIConvexHull` hull builder are encapsulated `internal` inside the engine (`.api/api-triangle.md` `[LOCAL_ADMISSION]` / `.api/api-vividorange-forcemomentinteraction.md` `[STACKING]`) — this AEC-DOMAIN owner mints NO direct mesher/hull call, composing the welded hull through the constructor, the strata-correct seam (the computational-geometry primitives are `Rasm`-kernel-owned, consumed transitively here); the eager solve is cached on the `SectionCapacity` `RcInteraction` carrier (`.api/api-vividorange-interactiondiagram.md` `[LOCAL_ADMISSION]` — construct once per section/settings, never re-solve per query), so a `Check(demand)` reads the cached hull; the `RcInteraction` utilisation is the exact Möller–Trumbore intersection of the origin-cast demand ray against the hull faces (the `IForceMomentTriFace.A`/`B`/`C` the demand vector pierces, the positive front-face pierce `t` the capacity boundary along the load direction), the no-pierce case (an eccentric hull that does not enclose the origin) yielding the typed `Utilisation.Unbounded` verdict rather than a silent `+∞`, NEVER the facet `Area` `Ratio` read as a physical quantity (`.api/api-vividorange-iforcemomentinteraction.md` `[TOPOLOGY]`); the `Utilisation.Governing` is the typed `GoverningAction` `[SmartEnum]` (axial · flexure · biaxial-moment · combined · shear · torsion · bearing — ONE canonical term per action; a `bending` synonym row beside `flexure` is the deleted form, and every axial-and-flexure interaction reports `combined` rather than whichever component was larger), NEVER a stringly-typed verdict; the capacity surface is host-neutral — the `IForceMomentMesh` round-trips through the realized `Freeze`/`Thaw` pair (`ToJson`/`FromJson<IForceMomentMesh>` over the marker the interface itself extends, `.api/api-vividorange-serialization.md`) into its content-keyed `Rasm.Persistence` artifact row, producer=consumer ONLY: the `TypeNameHandling.Objects` `$type` wire is a deserialization-gadget surface, so `Thaw` is fed exclusively JSON a trusted `Freeze` minted, never an external document, and the `$type` shape NEVER crosses to a peer (distinct from the canonical Thinktecture wire) — the utilisation verdict crosses to `Rasm.Compute/Analysis/structural#DESIGN_CHECK` as portable scalar data keyed by section, never a `VividOrange` assembly type crossing the boundary, and the `DesignBasis.Key` is that crossing's JURISDICTION column — every MEMBER-check key IS the consumer's `DesignCode` roster spelled identically, one vocabulary carried by two typed rows because the branch strata forbid a reference in either direction, so a key minted on either side without its counterpart is the defect and the correspondence is read off the two rosters rather than restated as a list here; the section-and-load-path-only keys (`en16612` glazing, `en1993-1-8`/`aws-d1-1`/`astm-d1002`/`icc-es` connection, `en1992-4` anchorage, `en1993-1-9`/`aisc-app3` fatigue) are the declared carve with no member-check counterpart and never cross, while `en1993-1-4` and `en1999` are MEMBER bases whose `DesignCode` counterpart rows the consumer roster carries under the same two-roster law; the annex a basis projects its `IStandard` under is `CapacityPlacement.Annex`, so one placement fixes the jurisdiction and the annex together and no arm re-reads a second annex. Checks stand REFUSED at this altitude as standing law, never faked as arms: the SLS DEFLECTION verdict needs the span, the load distribution, and the modulus — none a `SectionCapacity` carries — so it stays the forward `Rasm.Compute` member check's, reading E off the `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` substance row and I off the `ComputedSection` the seam already publishes; RC PUNCHING SHEAR is a slab-column JUNCTION check over a control perimeter no cross-section carries and stays Compute's beside the §6.2 beam-shear screen this page owns; and the SEISMIC system coefficients (ASCE 7 R/Ω0/Cd, the EN 1998-1 q ladder — `concrete#SEISMIC_SYSTEMS` owns the rows) are DEMAND-side scalars the load derivation consumes before a `Demand` ever reaches `Check`, so no capacity case carries them and no check row reads them.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;                     // FrozenDictionary (the SectionSelection.Lightest catalogue maps)
using System.Linq;                                   // Enumerable.Range, OrderBy (the selection sweep and mass ranking)
using LanguageExt;
using LanguageExt.Common;                            // Error (the MapFail carrier every VividOrange trap lands on)
using Rasm.Numerics;                                  // PositiveMagnitude (the >0 finite magnitude the ComputedSection AreaMm2/SxMm3 + ComponentUnit dimension columns carry — the kernel value-object atoms live in Rasm.Numerics, NOT Rasm.Domain)
using Rasm.Domain;                                   // Op (the boundary-admission key SectionCapacity.Resolve rails the ComponentFault on)
using Rasm.Element.Composition;      // MaterialId, ProfileRef (the seam-carried identity — STAYS seam-canonical, the rename stops at the Materials boundary)
using Rasm.Element.Properties;       // MeasureValue, QuantityType
using Thinktecture;
using VividOrange.ForceMomentInteraction;            // IForceMomentMesh, IForceMomentVertex, IForceMomentTriFace, the Faces/A/B/C/X/Y/Z floor
using ForceMomentEngine = VividOrange.ForceMomentInteraction.InteractionDiagram;  // the eager-solve engine (alias frees the bare name for the SectionCapacity owner)
using VividOrange.Sections;                          // IConcreteSection, SectionFace (the RcSection input + the effective-depth face)
using VividOrange.Materials.StandardMaterials.En;    // EnConcreteFactory (the LinearElasticMaterial.Strength == parsed fck the EC2 fctm + §6.2 shear screen read)
using VividOrange.Serialization;                     // JsonSerializationExtensions ToJson/FromJson (the Freeze/Thaw content-keyed hull artifact)
using VividOrange.Standards;                         // IStandard (the governing-code column every SectionCapacity case names)
using VividOrange.Standards.Eurocode;                // En1992/En1993/En1994/En1995/En1996/En1999 + NationalAnnex (the typed citations and the placement annex)
using UnitsNet;                                      // Force, Torque, Area, Length, Ratio, Angle (coerced at the edge)
using Dimension = Rasm.Element.Properties.Dimension; // the SI-dimension axis — disambiguated from the Rasm.Numerics discrete count
using static LanguageExt.Prelude;                    // toSeq, Some, None, Optional

// The capacity owner declares in the ONE flat Rasm.Materials.Component namespace (the codemap maps Component/Capacity.cs
// flat, and dotnet_style_namespace_match_folder = true:error forces the folder path), so it composes every family owner
// it lifts receipts from — ComputedSection/ComponentFault, DesignCapacity/CompactnessClass (steel), TimberCapacity
// (timber), CmuStrength/CmuRow (cmu), RuptureModulus/MortarSystem/MortarType (masonry), RcSection/RebarGradeRow
// (reinforcement), GlassCapacity (glazing), JointRow (joint), ConnectorCapacity (connector), AluminumGrade/
// ExtrusionForm/BucklingClass (aluminum), FastenerAssembly/Fastening (fastener) — by bare name.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The InteractionDiagram mesh/sweep-refinement policy folded to a VividOrange.InteractionDiagram DiagramSettings:
// the Steps knob drives a Steps² strain-plane sweep (quadratic cost), so the band trades hull fidelity for solve cost
// rather than scattering a DiagramSettings ctor at the call site (.api/api-vividorange-interactiondiagram.md [03]-[ENTRYPOINTS]).
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
    // DiagramSettings default ratio (250 mm² concrete / 200 mm² rebar) the engine ships (.api/api-vividorange-interactiondiagram.md [03]-[ENTRYPOINTS]).
    public DiagramSettings ToSettings() =>
        new(Area.FromSquareMillimeters(ConcreteMaxAreaMm2), Angle.FromDegrees(25.0),
            Area.FromSquareMillimeters(ConcreteMaxAreaMm2 * 0.8), Angle.FromDegrees(25.0), RebarDivisions, Steps);
}

// The SAFETY-FORMAT axis every design basis declares — how a code turns a nominal strength into a design resistance:
// a resistance-factor code multiplies by φ, a partial-factor code divides a characteristic value by γM, an allowable
// code divides by Ω. The keys ARE the Rasm.Compute/Analysis/structural#DESIGN_CHECK SafetyFormat vocabulary: one
// spelling across the [WIRE]: SectionCapacity seam, two typed carriers because the branch strata forbid a reference.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SafetyFormat {
    public static readonly SafetyFormat Asd        = new("asd");
    public static readonly SafetyFormat Lrfd       = new("lrfd");
    public static readonly SafetyFormat LimitState = new("limit-state");
}

// The LATERAL HAZARD a unit-shear demand represents, carrying the SDPWS §4.1.4 reduction from the ONE tabulated
// nominal to a design value. The 2021 edition publishes a SINGLE nominal per configuration and expresses the
// wind-versus-seismic distinction HERE, as the factor applied to it; earlier editions tabulated two nominal columns,
// so a second seeded column would fork the table it transcribes and re-import a distinction the standard removed.
// The reduction is a function of BOTH axes — hazard and the project's safety format — which is why it is a policy row
// beside the format rather than a column on the basis: a basis row states its format, a placement states its hazard,
// and neither alone determines the factor.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LateralHazard {
    public static readonly LateralHazard Wind    = new("wind",    asdDivisor: 2.0, lrfdFactor: 0.80);
    public static readonly LateralHazard Seismic = new("seismic", asdDivisor: 2.8, lrfdFactor: 0.50);
    public double AsdDivisor { get; }
    public double LrfdFactor { get; }

    // RAILED because the correspondence is partial: SDPWS defines the reduction for the two US formats and says
    // nothing about a limit-state basis, so an unserved (format, hazard) pair reports not-applicable instead of
    // silently borrowing whichever factor happened to sit in the other branch.
    public Fin<double> Design(double nominalKnPerM, SafetyFormat format, Op key) =>
        format == SafetyFormat.Asd ? Fin.Succ(nominalKnPerM / AsdDivisor)
        : format == SafetyFormat.Lrfd ? Fin.Succ(nominalKnPerM * LrfdFactor)
        : ComponentFault.Capacity(key, $"<lateral-format-unserved:{format.Key}:{Key}>");
}

// The three ALREADY-NORMALIZED ratios a combined-action kernel folds, plus the two shape facts the EC5 form reads:
// the arms divide demand by capacity per axis (each arm knows its own resistance columns) and hand the basis a pure
// dimensionless triple, so a basis row owns the INTERACTION ALGEBRA alone and never re-reads a capacity column.
// MinorWeight is the EN 1995 §6.1.6(2) k_m stress-redistribution factor (1.0 where the code publishes none) and
// Slender the λ_rel > 0.3 buckling-governs bit selecting the EC5 linear-versus-quadratic axial term.
public readonly record struct InteractionOperands(double Axial, double Major, double Minor, double MinorWeight, bool Slender);

// The DESIGN BASIS — the JURISDICTION axis every SectionCapacity case binds in place of a hardcoded code. A second
// design code over an already-cased family is one ROW here: the body, the safety format, the γM0 cross-section and
// γM1 member-stability partial factors, the NationalAnnex-threaded typed IStandard citation, and the combined-action
// kernel Check dispatches. A case name spelling ONE code forks the closed GoverningAction/Utilisation vocabulary the
// forward design-check consumer keys on, so the code lives on this column and never in a case name. Keys mirror the
// consumer's DesignCode roster exactly for the member bases — the en1993-1-4 and en1999 counterpart rows live at
// Compute's DesignCode roster; the glazing, connection, anchorage, and fatigue keys are section-and-load-path altitude
// only and cross to no member check.
// γM values are the codes' own recommended values (EN 1993 §6.1, EN 1992 §2.4.2.4, EN 1995 §2.4.1 Table 2.3,
// EN 1994 §2.4.1.2, EN 1996 §2.4.3 Table 2.3 whose 1.5-3.0 band this row reads at the class-3 category-I value);
// a φ-format row carries unity because its factors are per-action and live on the arm applying them.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DesignBasis {
    public static readonly DesignBasis Aisc360      = new("aisc360",    ComponentAuthority.Aisc, SafetyFormat.Lrfd,       1.00, 1.00, NoCitation,   Aisc);
    public static readonly DesignBasis AisiS100     = new("aisi-s100",  ComponentAuthority.Aisi, SafetyFormat.Lrfd,       1.00, 1.00, NoCitation,   Aisc);
    public static readonly DesignBasis En1992       = new("en1992",     ComponentAuthority.En,   SafetyFormat.LimitState, 1.50, 1.50, Ec2,          Linear, gammaS: 1.15);
    public static readonly DesignBasis En1993       = new("en1993",     ComponentAuthority.En,   SafetyFormat.LimitState, 1.00, 1.00, Ec3,          Linear, gammaM2: 1.25);
    // EN 1993-1-4 STAINLESS: the code declares its OWN recommended set — γM0 = γM1 = 1.10 (cross-section resistance
    // AND member instability both divide by 1.10, unlike the carbon-steel unity pair) and γM2 = 1.25 — so a stainless
    // DesignCapacity the steel#STEEL_FAMILY en1993-1-4 jurisdiction computes lands the SAME SteelMember case under
    // this row, never a sibling case: the reduced ε = √(235/fy · E/210000) and the 200 GPa design modulus stay the
    // steel owner's jurisdiction columns, the γ set and the interaction algebra this row's.
    public static readonly DesignBasis En1993Stainless = new("en1993-1-4", ComponentAuthority.En, SafetyFormat.LimitState, 1.10, 1.10, Ec3Stainless, Linear, gammaM2: 1.25);
    // EN 1993-1-9 FATIGUE: the jurisdiction row the EN detail-category law binds. γMf is NOT a γM column — it keys on
    // the (assessment method, consequence) pair a project declares, so it rides the FatigueAssessment row inside the
    // FatigueLaw the receipt carries, and this row's γM pair stays unity; γFf = 1.0.
    public static readonly DesignBasis En1993Fatigue = new("en1993-1-9", ComponentAuthority.En,   SafetyFormat.LimitState, 1.00, 1.00, Ec3Fatigue,   Linear);
    // AISC 360 Appendix 3: allowable-stress-range form FSR = (Cf/nSR)^0.333 ≥ FTH — no φ and no γ, the category
    // constants carry the whole margin, so the row is bare jurisdiction identity for the AISC fatigue law.
    public static readonly DesignBasis AiscFatigue  = new("aisc-app3",  ComponentAuthority.Aisc, SafetyFormat.Lrfd,       1.00, 1.00, NoCitation,   Aisc);
    public static readonly DesignBasis En1994       = new("en1994",     ComponentAuthority.En,   SafetyFormat.LimitState, 1.00, 1.00, Ec4,          Linear, gammaS: 1.15);
    public static readonly DesignBasis En1995       = new("en1995",     ComponentAuthority.En,   SafetyFormat.LimitState, 1.25, 1.25, Ec5,          Timber);
    public static readonly DesignBasis En1996       = new("en1996",     ComponentAuthority.En,   SafetyFormat.LimitState, 2.00, 2.00, Ec6,          Linear, gammaS: 1.15);
    // EN 1999-1-1 ALUMINIUM: the code declares NO γM0 — γM1 = 1.10 covers cross-section resistance and member
    // instability alike, so BOTH slots carry 1.10 and the invariant is mirrored at aluminum#ALUMINUM_FAMILY
    // AluminumPartialFactor, stating at both owners and moving as one; γM2 = 1.25 rides the fracture rail.
    public static readonly DesignBasis En1999       = new("en1999",     ComponentAuthority.En,   SafetyFormat.LimitState, 1.10, 1.10, Ec9,          Linear, gammaM2: 1.25);
    // EN 1992-4 ANCHORAGE: the concrete failure modes (cone, pullout, pryout, edge) divide by the γc family the
    // En1992 row already cites (§2.4.2.4 recommended 1.5); VividOrange.Standards partitions En1992 as 1-1/1-2/2/3
    // and ships NO Part4, so the citation answers None honestly and the clause rides the arm comments. The anchor
    // lift is CAST-IN only today — a post-installed product's ETA-set installation factor has no proven cell.
    public static readonly DesignBasis En1992Anchors = new("en1992-4", ComponentAuthority.En,    SafetyFormat.LimitState, 1.50, 1.50, NoCitation,   Linear, gammaS: 1.15);
    // TMS 402 strength design: §9.1.4 φ = 0.60 for unreinforced flexure and axial, φv = 0.80 for shear, the 0.80
    // stress-block cap on the masonry compressive fibre, and the §9.2.6.1 300 psi ceiling on the GOVERNING shear.
    // A φ-format jurisdiction by this axis's own definition (strength design multiplies by φ — the Aci318 twin), so
    // the format row reads lrfd and the γM pair stays unity; limit-state is the partial-factor family alone.
    public static readonly DesignBasis Tms402       = new("tms402",     ComponentAuthority.Astm, SafetyFormat.Lrfd,       1.00, 1.00, NoCitation,   Linear,
        phiFlexure: 0.60, phiShear: 0.80, stressBlock: 0.80, shearCeilingMpa: Some(2.07));
    public static readonly DesignBasis En16612      = new("en16612",    ComponentAuthority.En,   SafetyFormat.LimitState, 1.00, 1.00, NoCitation,   Linear);   // a European Norm outside the Eurocode set — no VividOrange body
    // The joint row divides JOINT resistance by γM2 and leaves the cross-section and stability factors at the EN 1993
    // recommended unity — the number moved to the slot the code puts it in.
    public static readonly DesignBasis En1993Joints = new("en1993-1-8", ComponentAuthority.En,   SafetyFormat.LimitState, 1.00, 1.00, Ec3Joints,    Linear, gammaM2: 1.25);
    public static readonly DesignBasis AwsD11       = new("aws-d1-1",   ComponentAuthority.Aws,  SafetyFormat.Lrfd,       1.00, 1.00, NoCitation,   Linear);
    public static readonly DesignBasis AstmD1002    = new("astm-d1002", ComponentAuthority.Astm, SafetyFormat.Lrfd,       1.00, 1.00, NoCitation,   Linear);
    public static readonly DesignBasis IccEs        = new("icc-es",     ComponentAuthority.IccEs, SafetyFormat.Asd,       1.00, 1.00, NoCitation,   Linear);   // the evaluation report itself is the issuing body — an allowable published by ICC-ES is not an AISI value
    // SDPWS publishes nominal unit shears and reduces them by the §4.1.4 factor pair, so the row carries ASD as the
    // format its own tables are written to be read under and unity γM — the reduction is LateralHazard's, not a
    // partial factor. The reduction reads the PROJECT's format off the placement's declared basis (an LRFD project
    // declares an lrfd-format placement basis and the ×0.80/×0.50 factors fire), so this row's own format column
    // states the tables' native reading, never the reduction input.
    public static readonly DesignBasis Sdpws        = new("sdpws",      ComponentAuthority.Awc,  SafetyFormat.Asd,        1.00, 1.00, NoCitation,   Linear);
    // The NDS is an ALLOWABLE-stress jurisdiction: its reference design values are divided by no partial factor and
    // increased by the adjustment-factor product instead, the duration term of which is the connector-owned Cd row.
    // The row STANDS as the [WIRE] counterpart of the consumer DesignCode `nds` key (the NDS member cells — CP/CL
    // adjusted reference values — live at Compute's capacity table) — deleting it would strand that key against the
    // two-roster correspondence law; no Materials-side producer mints under it, the timber family being EN-bodied.
    public static readonly DesignBasis Nds          = new("nds",        ComponentAuthority.Awc,  SafetyFormat.Asd,        1.00, 1.00, NoCitation,   Timber);
    // ACI 318 strength design: §21.2 φ = 0.90 tension-controlled flexure, φv = 0.75 shear, and the §22.2.2.4.1
    // equivalent-rectangular-stress-block coefficient 0.85 on f'c. The row STANDS as the [WIRE] counterpart of the
    // consumer DesignCode `aci318` key — deleting it would strand that key against the two-roster correspondence law
    // below — while its Materials-side producer waits on a US-bodied RC arm: the concrete family runs EN-bodied
    // (every non-EN VividOrange concrete factory arm throws, probe-confirmed), so no verdict mints under it yet.
    public static readonly DesignBasis Aci318       = new("aci318",     ComponentAuthority.Astm, SafetyFormat.Lrfd,       1.00, 1.00, NoCitation,   Linear,
        phiFlexure: 0.90, phiShear: 0.75, stressBlock: 0.85);

    public ComponentAuthority Body { get; }
    public SafetyFormat Format { get; }
    public double GammaM0 { get; }
    public double GammaM1 { get; }

    // The THIRD EN partial factor. EN 1993 distinguishes γM0 cross-section resistance, γM1 member-stability
    // resistance, and γM2 the resistance of a section in TENSION-TO-FRACTURE and of every joint component — the
    // recommended set is 1.00 / 1.00 / 1.25, and the joints row previously spelled its 1.25 in the γM0 and γM1 slots,
    // which is the wrong factor under the right number: a reader comparing member rows saw a jurisdiction that
    // divides cross-section resistance by 1.25, which EN 1993 does not.
    public double GammaM2 { get; }
    // The REINFORCEMENT partial factor, distinct from the concrete γM0 the same row carries: EN 1992 §2.4.2.4 pairs
    // γc with γs = 1.15, and a steel-couple arm dividing by the concrete factor over-prices every reinforced section.
    public double GammaS { get; }
    // The φ RESISTANCE FACTORS and the stress-block cap a strength-design jurisdiction publishes per action. A
    // partial-factor row carries unity across all three because its reduction rides γM instead, so one arm reads the
    // same columns on either basis and no kernel spells a code constant the other jurisdiction never published.
    public double PhiFlexure { get; }
    public double PhiShear { get; }
    public double StressBlock { get; }
    // The GOVERNING shear ceiling, present only where a code caps the resolved value — absence is the honest state
    // for every jurisdiction that publishes no cap, and a sentinel ceiling would clamp arms nothing clamps.
    public Option<double> ShearCeilingMpa { get; }

    // The citation is a FUNCTION of the placement annex, never a frozen field: an EN row cites its own part under the
    // project's annex, and a body shipping no VividOrange type answers None honestly rather than carrying a fabricated
    // Eurocode identity — a design report reads Body for those and the arm comment names the clause.
    [UseDelegateFromConstructor]
    public partial Option<IStandard> Standard(NationalAnnex annex);

    // The per-basis COMBINED-ACTION kernel — the one place a jurisdiction's interaction algebra lives, so a steel arm
    // folding AISC §H1.1 and the same arm folding EN 1993-1-1 §6.3.3 are ONE code path over two rows.
    [UseDelegateFromConstructor]
    public partial double Interact(InteractionOperands operands);

    // The EN citations, partitioned: Part 1-1 the general member rules every family reads, Part 1-4 the stainless
    // supplement, Part 1-8 the joint rules the bolted-connection lifts cite, Part 1-9 the fatigue part — all four
    // decompile-verified En1993Part members — and En1999 Part 1-1 the aluminium member rules
    // (.api/api-vividorange-standards.md).
    static Option<IStandard> Ec2(NationalAnnex a)       => Some<IStandard>(new En1992(En1992Part.Part1_1, a));
    static Option<IStandard> Ec3(NationalAnnex a)       => Some<IStandard>(new En1993(En1993Part.Part1_1, a));
    static Option<IStandard> Ec3Stainless(NationalAnnex a) => Some<IStandard>(new En1993(En1993Part.Part1_4, a));
    static Option<IStandard> Ec3Joints(NationalAnnex a) => Some<IStandard>(new En1993(En1993Part.Part1_8, a));
    static Option<IStandard> Ec3Fatigue(NationalAnnex a) => Some<IStandard>(new En1993(En1993Part.Part1_9, a));
    static Option<IStandard> Ec4(NationalAnnex a)       => Some<IStandard>(new En1994(En1994Part.Part1_1, a));
    static Option<IStandard> Ec5(NationalAnnex a)       => Some<IStandard>(new En1995(En1995Part.Part1_1, a));
    static Option<IStandard> Ec6(NationalAnnex a)       => Some<IStandard>(new En1996(En1996Part.Part1_1, a));
    static Option<IStandard> Ec9(NationalAnnex a)       => Some<IStandard>(new En1999(En1999Part.Part1_1, a));
    static Option<IStandard> NoCitation(NationalAnnex _) => Option<IStandard>.None;

    // AISC 360 §H1.1 / AISI S100 §C5: the two-branch combined form a max-of-independents under-predicts (p = m = 0.9
    // passes a max fold yet fails H1.1 at 1.7); the biaxial bending is the PER-AXIS two-term sum, never a resultant
    // against the major resistance alone.
    static double Aisc(InteractionOperands o) =>
        o.Axial >= 0.2 ? o.Axial + 8.0 / 9.0 * (o.Major + o.Minor) : o.Axial / 2.0 + o.Major + o.Minor;

    // EN 1993-1-1 §6.3.3 eq 6.61/6.62, EN 1994-1-1 §6.7.3.6, EN 1992 §6.1, EN 1996 §6.1, TMS 402 §9.2/§9.3, EN 16612:
    // the LINEAR unity sum — the interaction factors kyy/kzz an Annex-A/B evaluation refines ride at 1.0, the
    // conservative bound the estate states rather than a per-annex table it has not transcribed.
    static double Linear(InteractionOperands o) => o.Axial + o.Major + o.Minor;

    // EN 1995-1-1 §6.3.2 eq 6.23/6.24 (buckling governs — the LINEAR axial term over the already k_c-reduced N_Rd)
    // and §6.2.4 eq 6.19/6.20 (the stocky member's QUADRATIC n²), the pair MAX-swapped on the k_m weight so the
    // redistribution credit lands on one bending axis at a time.
    static double Timber(InteractionOperands o) =>
        (o.Slender ? o.Axial : o.Axial * o.Axial)
            + Math.Max(o.Major + o.MinorWeight * o.Minor, o.MinorWeight * o.Major + o.Minor);
}

// --- [FATIGUE]
// The EN 1993-1-9 §3(7) γMf grid as ONE four-row axis over (assessment method × consequence of failure): the pair a
// PROJECT declares together, so it rides the EN fatigue law as one row and no arm re-pairs the two halves.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FatigueAssessment {
    public static readonly FatigueAssessment DamageTolerantLow  = new("damage-tolerant-low",  gammaMf: 1.00);
    public static readonly FatigueAssessment DamageTolerantHigh = new("damage-tolerant-high", gammaMf: 1.15);
    public static readonly FatigueAssessment SafeLifeLow        = new("safe-life-low",        gammaMf: 1.15);
    public static readonly FatigueAssessment SafeLifeHigh       = new("safe-life-high",       gammaMf: 1.35);
    public double GammaMf { get; }
}

// The EN 1993-1-9 direct-stress detail-category ladder — the standard's CLOSED fourteen-rung set, ΔσC the printed
// category number at 2×10⁶ cycles and the ONE stored column. ΔσD/ΔσL are the standard's own §7.1 generators
// (ΔσD = (2/5)^(1/3)·ΔσC at 5×10⁶, ΔσL = (5/100)^(1/5)·ΔσD at 10⁸) — DERIVED, because the independently tabulated
// integer columns reproduce cell-for-cell off these two lines and a stored copy could only agree or drift. The
// per-detail assignment (WHICH constructional detail takes which rung, Tables 8.1–8.10) is the caller's declaration;
// the shear ΔτC rungs (100/80, single slope m = 5) are typed-absent this pass and land as a sibling column pair.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EnFatigueCategory {
    public static readonly EnFatigueCategory C160 = new("160", refMpa: 160.0);
    public static readonly EnFatigueCategory C140 = new("140", refMpa: 140.0);
    public static readonly EnFatigueCategory C125 = new("125", refMpa: 125.0);
    public static readonly EnFatigueCategory C112 = new("112", refMpa: 112.0);
    public static readonly EnFatigueCategory C100 = new("100", refMpa: 100.0);
    public static readonly EnFatigueCategory C90  = new("90",  refMpa: 90.0);
    public static readonly EnFatigueCategory C80  = new("80",  refMpa: 80.0);
    public static readonly EnFatigueCategory C71  = new("71",  refMpa: 71.0);
    public static readonly EnFatigueCategory C63  = new("63",  refMpa: 63.0);
    public static readonly EnFatigueCategory C56  = new("56",  refMpa: 56.0);
    public static readonly EnFatigueCategory C50  = new("50",  refMpa: 50.0);
    public static readonly EnFatigueCategory C45  = new("45",  refMpa: 45.0);
    public static readonly EnFatigueCategory C40  = new("40",  refMpa: 40.0);
    public static readonly EnFatigueCategory C36  = new("36",  refMpa: 36.0);
    public double RefMpa { get; }
    public double CaflMpa => Math.Pow(2.0 / 5.0, 1.0 / 3.0) * RefMpa;      // ΔσD, the constant-amplitude limit at 5×10⁶
    public double CutoffMpa => Math.Pow(5.0 / 100.0, 1.0 / 5.0) * CaflMpa; // ΔσL, the cut-off at 10⁸
}

// AISC 360 Appendix 3 Table A-3.1 — the TWO-SOURCED categories A–E′ alone under the uniform 0.333 exponent: Cf the
// ksi-form constant (the SI evaluation carries the standard's own ×329 factor) and FTH the printed MPa threshold.
// Categories F (its own Eq. A-3-2, 0.167 exponent) and G are single-sourced and typed-absent — the closed set is
// exactly the rungs whose constants two independent captures agree on, and F′ does not exist in the standard.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AiscFatigueCategory {
    public static readonly AiscFatigueCategory A      = new("a",       cf: 250e8, fthMpa: 165.0);
    public static readonly AiscFatigueCategory B      = new("b",       cf: 120e8, fthMpa: 110.0);
    public static readonly AiscFatigueCategory BPrime = new("b-prime", cf: 61e8,  fthMpa: 83.0);
    public static readonly AiscFatigueCategory C      = new("c",       cf: 44e8,  fthMpa: 69.0);
    public static readonly AiscFatigueCategory CPrime = new("c-prime", cf: 44e8,  fthMpa: 83.0);
    public static readonly AiscFatigueCategory D      = new("d",       cf: 22e8,  fthMpa: 48.0);
    public static readonly AiscFatigueCategory E      = new("e",       cf: 11e8,  fthMpa: 31.0);
    public static readonly AiscFatigueCategory EPrime = new("e-prime", cf: 3.9e8, fthMpa: 18.0);
    public double Cf { get; }
    public double FthMpa { get; }
}

// The ONE detail-category S-N family: the two codes publish the SAME anatomy — a permissible direct-stress range as
// a function of cycle count with a threshold floor — under two non-convertible ladders (EN fixes its knee at 5×10⁶
// for every rung; the AISC thresholds knee anywhere from ~1.8 to 22×10⁶, so no cell-for-cell map exists and the
// ladder is the case discriminant, never a conversion). DesignMpa is the design resistance at the demanded count:
// the EN arm walks the m = 3 / m = 5 two-slope law to the ΔσL floor and divides by its assessment's γMf (γFf = 1.0);
// the AISC arm evaluates (Cf·329/n)^(1/3) MPa floored at FTH, the allowable form carrying no further factor.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FatigueLaw {
    private FatigueLaw() { }
    public sealed record En(EnFatigueCategory Category, FatigueAssessment Assessment) : FatigueLaw;
    public sealed record Aisc(AiscFatigueCategory Category) : FatigueLaw;

    public DesignBasis Basis => Switch(
        en: static _ => DesignBasis.En1993Fatigue,
        aisc: static _ => DesignBasis.AiscFatigue);

    public double DesignMpa(double cycles) => Switch(
        en: e => EnRange(e.Category, cycles) / e.Assessment.GammaMf,
        aisc: a => Math.Max(Math.Pow(a.Category.Cf * 329.0 / cycles, 1.0 / 3.0), a.Category.FthMpa));

    // §7.1: m = 3 to the 5×10⁶ knee, m = 5 to the 10⁸ cut-off, ΔσL the constant-amplitude floor beyond — a range
    // below ΔσL contributes no damage, so the floor is the honest terminal resistance, never a zero.
    static double EnRange(EnFatigueCategory category, double cycles) =>
        cycles <= 5e6 ? category.RefMpa * Math.Pow(2e6 / cycles, 1.0 / 3.0)
        : cycles <= 1e8 ? category.CaflMpa * Math.Pow(5e6 / cycles, 1.0 / 5.0)
        : category.CutoffMpa;
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

// The CAST-IN anchor's concrete-side facts — the placement declaration the fastener row cannot carry: cylinder
// strength, effective embedment, the nearest edge where one exists, and the cracked state the EN 1992-4 k1 pair
// selects on. The bed carries NO basis column: en1992-4 is the one realized anchorage jurisdiction (the ACI arm —
// kc = 24, the 8·Abrg·f'c pullout, the kcp pryout, all two-source-proven — lands with its anchor φ roster, and the
// bed regains its Abrg column then), so the lift pins the row by construction and a mis-jurisdictioned anchor is
// unrepresentable rather than mis-priced.
public readonly record struct AnchorBed(
    double FckMpa,
    PositiveMagnitude HefMm,
    Option<double> EdgeMm,
    bool Cracked);

// The DG1 base-plate declaration: plate B × N × t and its yield, the wide-flange column footprint the m/n cantilever
// pair reads (the HSS/pipe variants are single-sourced and typed-absent — a non-wide-flange plate waits on them),
// the bearing concrete, and the caller's √(A2/A1) confinement ratio the arm clamps at the J8 ceiling of 2.
public readonly record struct PlateBed(
    PositiveMagnitude WidthMm,
    PositiveMagnitude LengthMm,
    PositiveMagnitude ThicknessMm,
    double FyMpa,
    PositiveMagnitude ColumnDepthMm,
    PositiveMagnitude ColumnFlangeMm,
    double FcMpa,
    double ConfinementRatio);

// The sibling-receipt request [Union] the ONE Lift dispatches (FORM_CHOOSER row 1: a receipt family collapses onto a
// request union + total Switch, never an overload roster) — each case CARRIES its full lift context so the modality
// is recoverable from the request value alone: the steel/timber cases the already-computed family receipt, the
// masonry case its typed CmuStrength row + the (grouted) net ComputedSection + the member slenderness reduction + the
// Table 9.1.9.2 RuptureModulus row with its MortarSystem/MortarType keys (the prior five-parameter overload tail is
// the deleted form). A new family receipt is one case + one Switch arm, never another overload.
// Every case carries its SUBJECT — the catalogued component the check is about. The analytics per-check dataset and
// the MaterialsFact stream both key on (op, kind, governing), which collides for two members of one kind under one
// op, so the identity rides the BASE where a new case cannot forget it: a receipt is always about something, and a
// case that could omit it would drop the only column separating two members of one family in one report.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CapacityReceipt {
    private CapacityReceipt(ComponentId subject) => Subject = subject;
    public ComponentId Subject { get; }
    public sealed record Steel(ComponentId Subject, DesignCapacity Capacity) : CapacityReceipt(Subject);
    public sealed record Timber(ComponentId Subject, TimberCapacity Capacity) : CapacityReceipt(Subject);
    // The steel deck's AISI S100 receipt beside its gauge and rib rows: the panel#PANEL_FAMILY deck seeds Sectioned
    // and solves a full ComputedSection through the corrugated arm, and SteelDesign's AISI overload prices it at the
    // GaugeRow's own SS Grade 33/50 yield — so the deck datum GaugeRow.AxialSectionCapacityKnPerMm declared for the
    // seam finally has a check behind it. Gauge and Rib ride the receipt because the report and the analytics kind
    // dimension name WHICH deck, never a bare steel row.
    public sealed record DeckSheet(ComponentId Subject, GaugeRow Gauge, DeckProfileRow Rib, DesignCapacity Capacity) : CapacityReceipt(Subject);
    // The masonry receipt carries BOTH tension tables because they key on different axes — the TMS 402 row on span
    // direction × grout form, the EN 1996 row on unit group — so neither collapses into the other; the receipt's own
    // Basis picks which one Lift reads, and RuptureModulus.SpanParallelToBed is the ONE direction source both use.
    public sealed record Masonry(ComponentId Subject, CmuStrength Strength, ComputedSection Section, PositiveMagnitude HeightMm, DesignBasis Basis, RuptureModulus Rupture, FlexuralStrengthEn Flexural, MortarSystem System, MortarType Mortar) : CapacityReceipt(Subject);
    // The reinforced case reads the cmu lattice facts the unreinforced case never consumed: the seed row's
    // ReinforcedCells/RebarBarMm steel, the bar grade's yield, and the grouted net section — the TMS 402 §9.3 /
    // EN 1996-1-1 §6.6 steel-couple inputs.
    public sealed record ReinforcedMasonry(ComponentId Subject, CmuStrength Strength, ComputedSection Section, PositiveMagnitude HeightMm, DesignBasis Basis, CmuRow Unit, RebarGradeRow Bar) : CapacityReceipt(Subject);
    // The glazing pane resistance lifted WHOLE from glazing#GLAZING_FAMILY GlazingStructural — never re-derived here.
    public sealed record Glass(ComponentId Subject, GlassCapacity Capacity) : CapacityReceipt(Subject);
    // The ACCIDENTAL fire design situation as two more lift cases over the SAME law: the steel owner's already-read
    // EN 1993-1-2 Table 3.1 retention pair (steel#STEEL_FAMILY FireRetention.At over the section's SectionFactorPerM
    // and CriticalTemperatureC) rides beside the ambient receipt, and the timber owner's ResidualSection/ResidualStack
    // charred receipt arrives already priced at kmod = γM = 1.0 — the EN 1995-1-2 accidental combination. Neither arm
    // derives fire physics here: the family owner computes, this owner lifts, and Check folds the fire state through
    // the identical ambient interaction so a fire verdict and an ambient verdict are one rail.
    public sealed record SteelFire(ComponentId Subject, DesignCapacity Ambient, double Ky, double Ke, double SteelTemperatureC) : CapacityReceipt(Subject);
    public sealed record TimberFire(ComponentId Subject, TimberCapacity Residual) : CapacityReceipt(Subject);
    // The connection receipts — the joint#JOINT_FAMILY line/area/stud design values and the connector#CONNECTOR_FAMILY
    // duration-governed capacity — each case carrying its full lift context (the weld its load angle, the stud its
    // group count), so the load-path verdict rides the SAME Check fold as the member cases.
    public sealed record Weld(ComponentId Subject, JointRow.Weld Row, double LoadAngleDeg) : CapacityReceipt(Subject);
    public sealed record Adhesive(ComponentId Subject, JointRow.Adhesive Row) : CapacityReceipt(Subject);
    // The stud group is a PLACEMENT fact — the deck relation, the studs-per-rib count, and the rib position — and
    // AISC Eq I8-1 reads it directly, so it rides the receipt rather than the row: one stud class welded into three
    // different deck conditions is three different capacities.
    public sealed record Stud(ComponentId Subject, JointRow.Stud Row, StudGroup Group, int Count) : CapacityReceipt(Subject);
    public sealed record Connector(ComponentId Subject, ConnectorCapacity Capacity) : CapacityReceipt(Subject);
    // The sheathed-assembly receipt: the design unit shear the panel family already reduced, and the hazard that
    // reduced it.
    public sealed record LateralPanel(ComponentId Subject, double DesignKnPerM, LateralHazard Hazard) : CapacityReceipt(Subject);
    // The BEARING-type bolted connection: the fastener#BOLT_ASSEMBLY FastenerAssembly carries the (thread, grade,
    // category, faying, planes) state its own Of already admitted, BearingDesign the ply the shank bears against, and
    // ShearPlane the ONE free axis a placement chooses — threads in or out of the shear plane. BoltCategory is NOT a
    // case column: the assembly already holds it, and a second spelling is the redundant parallel lift parameter this
    // owner bans. Lift folds the assembly's own ShearResistanceKn (plane-counted), TensionResistanceKn, and
    // BearingResistanceKn projections onto the ONE Connection triple, so a bolt group reports through the same
    // governing-action fold as a weld; the two EN-railed members collapse an untabulated grade to the unprovided
    // column (0/None), never a fabricated resistance.
    public sealed record Bolt(ComponentId Subject, FastenerAssembly Assembly, BearingDesign Bearing, ShearPlane Plane) : CapacityReceipt(Subject);
    // The SLIP-CRITICAL (EN 1993-1-8 category B/C/E) state of the SAME assembly: the shear column is the §3.9 slip
    // resistance rather than the shank shear, and a non-preloaded assembly answers None — lifted as a 0 shear column
    // the GuardedRatio fold makes govern loud, never a bearing value silently substituted for a slip value.
    public sealed record SlipCritical(ComponentId Subject, FastenerAssembly Assembly, FastenerInstallation Install) : CapacityReceipt(Subject);
    // The EC5 §8 dowel-type TIMBER connection: fastener#FASTENER_FAMILY Fastening.TimberDowelShearKn is the family
    // owner's railed six-mode Johansen minimum, so its ALREADY-COMPUTED per-shear-plane design value arrives as a
    // column here and Lift stays total — the design-code computation is the sibling page's, the unified verdict this
    // owner's, exactly as the steel/timber/masonry receipts already ride.
    public sealed record TimberDowel(ComponentId Subject, double PerPlaneShearKn, int Planes) : CapacityReceipt(Subject);
    // The aluminium die: the banded (fo, fu) pair the aluminum#ALUMINUM_FAMILY grade registry proved at seed time,
    // the grade (its BucklingClass letter selects the §6.3.1 curve columns), the product form for the report, the
    // solved section, and the basis — en1999 the one jurisdiction with landed aluminium bands, which the family
    // producer PROVES: AluminumSeed.Capacity refuses any other placement basis typed, so a receipt reaching this
    // case carries en1999 by construction and no die is priced through a foreign kernel.
    public sealed record Aluminum(ComponentId Subject, AluminumGrade Grade, ExtrusionForm Form, double FoMpa, double FuMpa, ComputedSection Section, DesignBasis Basis) : CapacityReceipt(Subject);
    // The detail-category fatigue declaration: the LAW carries the ladder rung and (EN) the γMf assessment row —
    // the per-detail category assignment (EN Tables 8.1–8.10 / the AISC descriptive rows) is the caller's stated
    // detail fact, exactly as a weld states its load angle and a stud its group.
    public sealed record Fatigue(ComponentId Subject, FatigueLaw Law) : CapacityReceipt(Subject);
    // The cast-in anchor: the fastener#BOLT_ASSEMBLY assembly (an F1554 rod under the Anchor kind), the threads-in/
    // out shear plane, and the concrete bed — the EN 1992-4 cone/steel minimum lowers onto the ONE Connection triple.
    public sealed record Anchor(ComponentId Subject, FastenerAssembly Assembly, ShearPlane Plane, AnchorBed Bed) : CapacityReceipt(Subject);
    // The DG1 base plate: bearing and cantilever plate bending resolved from the bed alone — both are axial-demand
    // checks, so the case precomputes the two axial capacities and the verdict rides the standard Check fold.
    public sealed record BasePlate(ComponentId Subject, PlateBed Plate) : CapacityReceipt(Subject);

    // Case identity IS the kind dimension every downstream reader keys on — signal roster tag and analytics column
    // alike — so this total projection holds the one spelling, a further case breaks it at compile time, and no
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
        connector: static _ => nameof(Connector),
        bolt: static _ => nameof(Bolt),
        slipCritical: static _ => nameof(SlipCritical),
        timberDowel: static _ => nameof(TimberDowel),
        lateralPanel: static _ => nameof(LateralPanel),
        aluminum: static _ => nameof(Aluminum),
        fatigue: static _ => nameof(Fatigue),
        anchor: static _ => nameof(Anchor),
        basePlate: static _ => nameof(BasePlate));
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
    // The DIAPHRAGM and SHEAR-WALL action: a per-unit-length shear carried in the plane of a sheathed assembly. One row
    // serves both, because a shear wall and a diaphragm differ in orientation and in which table publishes them, never
    // in the action — a `diaphragm` row beside a `shear-wall` row would be two names for one verdict axis.
    public static readonly GoverningAction InPlaneShear  = new("in-plane-shear");
    // The CYCLIC direct-stress-range action a detail-category check governs on — one row for both S-N ladders,
    // because EN and AISC differ in the ladder, never in the action a fatigue verdict names.
    public static readonly GoverningAction Fatigue       = new("fatigue");
}

// --- [MODELS] ------------------------------------------------------------------------------
// The applied design action checked against the capacity surface — the full member-action vector in SI engineering
// units (kN, kNm), SIGNED (axial − compression / + tension, moments ± for direction), so the columns are signed
// doubles NOT PositiveMagnitude — yet ADMITTED ONCE (BOUNDARY_ADMISSION): a signed value never licenses NaN/∞, so the
// generated validation owns the all-finite guard and the railed Of lifts a rejected action onto
// ComponentFault.Dimension with typed evidence — no non-finite component reaches Check, and no per-case arm
// re-checks one. N/My/Mz are the RcInteraction hull-ray vector, the RcElastic combined-stress demand, and
// the flexure/axial demands; Vy/Vz the shear demands the SteelMember, TimberMember V_Rd, RcElastic §6.2, and
// MasonryUnreinforced shear arms fold; Mt the torsion the SteelMember §H3.1/§6.2.7 and TimberMember §6.1.8 torsion
// arms fold against the lifted torsional resistance; Rb the perpendicular support reaction the TimberMember R_90,Rd
// bearing arm folds; q the IN-PLANE UNIT SHEAR the LateralPanel arm folds against a sheathed assembly's design unit
// shear — a per-LENGTH action, which is why it is its own column rather than a reading of Vy/Vz: a diaphragm carries
// shear per metre of chord and a column carries shear, and one column serving both would compare unlike quantities.
// StressRangeMpa/CycleCount are the FATIGUE action pair the detail-category arm folds — Δσ the direct-stress range at
// the declared detail and the demanded cycle count beside it, a per-cycle action no static column can express, which
// is why they are their own columns rather than a reading of the moments. They are MODALITY columns under the same
// law the panel's unit shear rides: a member's static verdict and its detail's fatigue verdict are TWO Check
// invocations over two capacity surfaces, the static arms leave the pair unread exactly as they leave q unread, and
// the fatigue arm folds every static column loud in return. The biaxial moment magnitude and the shear resultant are
// derived projections, never re-passed columns.
[ComplexValueObject]
public readonly partial struct Demand {
    public double AxialKn { get; }
    public double MomentYKnm { get; }
    public double MomentZKnm { get; }
    public double ShearYKn { get; }
    public double ShearZKn { get; }
    public double TorsionKnm { get; }
    public double BearingKn { get; }
    public double UnitShearKnPerM { get; }
    public double StressRangeMpa { get; }
    public double CycleCount { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double axialKn, ref double momentYKnm, ref double momentZKnm,
        ref double shearYKn, ref double shearZKn, ref double torsionKnm, ref double bearingKn,
        ref double unitShearKnPerM, ref double stressRangeMpa, ref double cycleCount) =>
        validationError = double.IsFinite(axialKn) && double.IsFinite(momentYKnm) && double.IsFinite(momentZKnm)
            && double.IsFinite(shearYKn) && double.IsFinite(shearZKn) && double.IsFinite(torsionKnm) && double.IsFinite(bearingKn)
            && double.IsFinite(unitShearKnPerM) && double.IsFinite(stressRangeMpa) && stressRangeMpa >= 0.0
            && double.IsFinite(cycleCount) && cycleCount >= 0.0
            ? null
            : new ValidationError($"<demand-nonfinite:n={axialKn:R}:my={momentYKnm:R}:mz={momentZKnm:R}:vy={shearYKn:R}:vz={shearZKn:R}:mt={torsionKnm:R}:rb={bearingKn:R}:q={unitShearKnPerM:R}:dsig={stressRangeMpa:R}:ncyc={cycleCount:R}>");

    public static Fin<Demand> Of(double axialKn, double momentYKnm, double momentZKnm, Op key,
        double shearYKn = 0.0, double shearZKn = 0.0, double torsionKnm = 0.0, double bearingKn = 0.0,
        double unitShearKnPerM = 0.0, double stressRangeMpa = 0.0, double cycleCount = 0.0) =>
        Validate(axialKn, momentYKnm, momentZKnm, shearYKn, shearZKn, torsionKnm, bearingKn, unitShearKnPerM, stressRangeMpa, cycleCount, out Demand demand) is { } error
            ? Fin.Fail<Demand>(ComponentFault.Dimension(key, error.Message))
            : Fin.Succ(demand);

    public double MomentResultantKnm => Math.Sqrt(MomentYKnm * MomentYKnm + MomentZKnm * MomentZKnm);
    public double ShearResultantKn => Math.Sqrt(ShearYKn * ShearYKn + ShearZKn * ShearZKn);
}

// The PLACEMENT facts a capacity needs and a catalogue row cannot carry — the lengths a bracing scheme fixes, the
// DESIGN BASIS and annex a project selects together (one jurisdiction choice fixes both, so no arm reads a second
// annex and no family owner accepts a loose code flag), the EC5 service×duration pair, the NDS duration row a wood connector scales by, the member
// height the masonry stability bracket divides, the weld load angle, the stud-group count, the LATERAL HAZARD a
// sheathed assembly's unit shear is reduced under together with the assembly form, framing width, and load case its
// published table is cut by, and the glass load-duration and fire-rating pair. ONE currency threads component#COMPONENT_OWNER ComponentFamily.Capacity, so a family producer
// takes exactly the same third argument and a new placement input is one column here rather than a per-family
// parameter tail. Defaults are the unbraced/ten-year/recommended-annex reference state every arm reads when its
// family ignores the column — an arm consuming a column its family does not use is unrepresentable, because the
// producer names the columns it reads.
public readonly record struct CapacityPlacement(
    double EffectiveLengthMm,
    double UnbracedLengthMm,
    DesignBasis Basis,
    NationalAnnex Annex,
    ServiceClass Service,
    LoadDuration Duration,
    DurationRow ConnectorDuration,
    PositiveMagnitude HeightMm,
    double LoadAngleDeg,
    int StudCount,
    double GlassLoadDurationS,
    LateralHazard Hazard,
    LateralAssembly Assembly,
    double FramingWidthMm,
    int DiaphragmCase,
    Option<FastenerPlacement> Fastener,
    StudGroup StudGroup,
    GlassBasis GlassBasis,
    double GlassEdgeFactor,
    int FireEiMinutes,
    RuptureModulus Rupture,
    FlexuralStrengthEn Flexural,
    MortarSystem System,
    MortarType Mortar,
    RebarGradeRow BarGrade);

// The member-stability reduction as a DERIVED value object, PER BASIS: the formula IS the owner, so no caller
// re-derives a code bracket and a transposed branch is unrepresentable. The height arrives as the kernel-admitted
// PositiveMagnitude and the radius as the always-positive ComputedSection.GoverningRadiusMm, so BOTH derivations are
// TOTAL over h/r ∈ (0, ∞) with range (0, 1] — and the throwing Create is the sanctioned re-admission of a value the
// algebra already proves. Every producer is the Lift arm that holds the section AND its basis; a raw-scalar Of with a
// caller-supplied branch is the deleted form.
[ValueObject<double>]
public readonly partial struct MasonryReduction {
    const double SlendernessBreak = 99.0;   // TMS 402: h/r <= 99 takes the parabolic bracket, above it the Euler-form ratio
    // EN 1996-1-1 §5.5.1.1 initial eccentricity e_init = h_ef/450 folded into the §6.1.2.2(1) Φ = 1 − 2·e/t: for the
    // solid rectangle whose r = t/√12, Φ = 1 − (2/(450·√12))·(h/r), so the EN reduction reads the SAME two inputs the
    // TMS bracket does and needs no second placement column. A slenderness driving Φ to zero is the §5.5.1.4 h/t = 27
    // ceiling the clamp expresses as a floor rather than as a throw.
    const double EnInitialEccentricity = 2.0 / (450.0 * 3.4641016151377544);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value is > 0.0 and <= 1.0
            ? null
            : new ValidationError($"<masonry-reduction-invalid:{value:R}>");

    public static MasonryReduction Of(DesignBasis basis, PositiveMagnitude heightMm, double radiusOfGyrationMm) =>
        heightMm.Value / radiusOfGyrationMm is var ratio && basis == DesignBasis.En1996
            ? Create(Math.Clamp(1.0 - EnInitialEccentricity * ratio, double.Epsilon, 1.0))
            : ratio <= SlendernessBreak
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
    // `Overcapacity` said the opposite to every design-report reader charting the verdict. The fold REACHES this case
    // by a candidate that publishes no ratio, so the verdict is structural: no sentinel magnitude is minted, compared,
    // or recovered from anywhere on the rail, and the case carries no Value because there is none to carry.
    public sealed record Unbounded(GoverningAction Action) : Utilisation(Action);
}

// The section-UNDECIDABLE deferrals: a check whose remaining input is member-level DETAILING the cross-section does
// not carry, so the section verdict passes with the named obligation attached instead of failing on a zero column.
// Each row is a real code clause whose missing input is spelled: stirrup/link/bar SPACING, an open-shape warping
// torsion that is not one resistance, or an in-plane panel whose form declares no edgewise bending strength.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MemberCheckRequirement {
    public static readonly MemberCheckRequirement RcShearReinforcement          = new("rc-shear-reinforcement");            // EC2 §6.2.3(3) V_Rd,s needs the stirrup spacing — the ONE linked-section deferral
    public static readonly MemberCheckRequirement SteelWarpingTorsion           = new("steel-warping-torsion");             // AISC §H3.3 open-shape warping torsion is not a single resistance
    public static readonly MemberCheckRequirement CltInPlaneBending             = new("clt-in-plane-bending");              // the net-section in-plane arm priced nothing: the form declares no edgewise bending strength
    public static readonly MemberCheckRequirement ReinforcedMasonryShearSpacing = new("reinforced-masonry-shear-spacing");  // TMS 402 §9.3.4.1.2 V_ns needs the bar spacing
    public static readonly MemberCheckRequirement TimberBearingLength           = new("timber-bearing-length");              // EN 1995-1-1 §6.1.5 R_90,Rd needs the support bearing length
    public static readonly MemberCheckRequirement AnchorForwardModes            = new("anchor-forward-modes");               // EN 1992-4 group areas/spacing, the shear edge mode, the ETA-owned pullout, and a non-EN rod-steel band — the anchor modes only the forward fixture-level check can finish
    public static readonly MemberCheckRequirement AluminumMemberBuckling        = new("aluminum-member-buckling");           // EN 1999-1-1 §6.3.1/§6.3.2 χ over the published α/λ̄0 curve needs the effective length
}

// One SectionCapacity [Union] closes the structural-capacity family across the realized structural rails AND the
// connection load path — the ultimate N-M-M hull, the elastic transformed RC section, the rolled/composite/cold-formed
// (and, basis-told, stainless) steel receipt, the EC5 timber design receipt, the EN 1999 aluminium member, the TMS 402
// URM and §9.3 reinforced masonry checks, the EN 16612 glass pane, the weld/adhesive/stud/connector/anchor Connection
// triple, the detail-category Fatigue law, and the DG1 BasePlate pair — so a member AND its connection are checked
// through one Check fold, never a per-type surface. The non-RC cases lift their family-owner receipts WHOLE (the
// design-code computation stays the sibling page's, the unified verdict this owner's); the RC cases are the Resolve
// builds; the aluminium, anchorage, and base-plate arms COMPUTE at lift because their families own data, not algebra.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionCapacity {
    // The DESIGN BASIS as the ONE base-constructor column: a verdict names the jurisdiction it was computed under, so
    // a design report, the analytics governing column, and the forward member check all read one row rather than a
    // (body, code) pair a case had to spell twice. The row carries the authority body, the safety format, the partial
    // factors, the annex-threaded citation, and the interaction kernel — so the prior per-case
    // `ComponentAuthority.X, Option<IStandard>.None` tail and the frozen recommended-annex Ec2/Ec5 statics are the
    // deleted form: an EN citation now reads the PROJECT's annex through the placement rather than a hardcoded one.
    private SectionCapacity(DesignBasis basis) => Basis = basis;
    public DesignBasis Basis { get; }
    public ComponentAuthority Body => Basis.Body;

    // The cached ultimate biaxial capacity hull — the IForceMomentMesh held once from the eager InteractionDiagram solve.
    // EC2 §6.1 ultimate axial-flexural resistance over the rigid-plastic stress-block fibre integral.
    public sealed record RcInteraction(IForceMomentMesh Hull) : SectionCapacity(DesignBasis.En1992);
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
        // The link DESIGN yield f_ywd = f_yk / γs, γs the basis row's own column — Option because a link grade
        // without a published characteristic yield declares absence, and the seam triple then stays unpublished.
        Option<double> FywdMpa,
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
        double FctmMpa) : SectionCapacity(DesignBasis.En1992) {

        // The EC2 §6.2.3 web-crushing ceiling V_Rd,max at the policy cotθ = 2.5 — DERIVED off the record's own
        // columns (a stored copy would double-store what bw/d/fck already decide), the guarded floors matching the
        // shear screen's own admission.
        public double VrdMaxKn =>
            Math.Max(WidthMm, 1.0) * 0.9 * Math.Max(EffectiveDepthMm, 1.0) * 0.6
                * (1.0 - FckMpa / 250.0) * (FckMpa / 1.5) / (2.5 + 0.4) * 1e-3;

        // The seam publication of the shear-link triple — the three Element StructuralRows-keyed measured rows the
        // Compute member check reads for its V_Rd,s truss arm; SI conversion happens at THIS one site (mm²→m²,
        // MPa→Pa, kN→N) through the typed MeasureValue mints, and the triple publishes ONLY whole: a link-less
        // section or an absent design yield publishes the EMPTY set, so the reader's all-three-present gate is the
        // same absence the producer declared.
        public Fin<Seq<(PropertyName Row, PropertyValue Value)>> ShearLinkRows(Op key) =>
            ShearLinkAreaMm2 > 0.0
                ? FywdMpa.Match(
                    Some: fywd =>
                        from area in MeasureValue.Of(ShearLinkAreaMm2 * 1e-6, UnitsNet.Units.AreaUnit.SquareMeter, key)
                        from fywdPa in MeasureValue.Of(fywd * 1e6, UnitsNet.Units.PressureUnit.Pascal, key)
                        from ceiling in MeasureValue.Of(VrdMaxKn * 1e3, UnitsNet.Units.ForceUnit.Newton, key)
                        select Seq(
                            (StructuralRows.ShearLinkArea, (PropertyValue)new PropertyValue.Measure(area)),
                            (StructuralRows.ShearLinkYield, (PropertyValue)new PropertyValue.Measure(fywdPa)),
                            (StructuralRows.ShearLinkCeiling, (PropertyValue)new PropertyValue.Measure(ceiling))),
                    None: () => Fin.Succ(Seq<(PropertyName, PropertyValue)>()))
                : Fin.Succ(Seq<(PropertyName, PropertyValue)>());
    }
    // The steel design-resistance receipt lifted WHOLE from steel#STEEL_FAMILY DesignCapacity (the SI N·mm/N
    // resistances carried as kN·m/kN here, plus the cross-section CompactnessClass and the column slenderness λ) —
    // never re-derived here, and BASIS-TAGGED because the same case serves the AISC 360 φ-format and the EN 1993-1-1
    // γM-format receipts: the columns hold φMn/φMny/φPn/φVn under aisc360 and Mb,Rd/Mz,Rd/Nb,Rd/Vpl,Rd under en1993,
    // one shape the Check fold reads through the basis's own interaction kernel rather than two parallel cases.
    // FlexuralMinorKnm is the §F6 weak-axis φMny = φb·min(Fy·Zy, 1.6·Fy·Sy) (1.5 cap on the F10 single-angle regime,
    // F6.2-bounded for the F2 flange classes, Seff-scaled on the cold-formed arm) or the EN §6.2.5 Wpl,z·fy/γM0 — the
    // per-axis divisor beside FlexuralKnm on either basis. TorsionalKnm is the AISC §H3.1 φTn = φT·Fcr·C or the EN
    // §6.2.7 Wt·fy/(√3·γM0) — positive for a CLOSED HSS/pipe, 0 for an OPEN thin-walled shape whose warping torsion is
    // not a single resistance, so an open-shape torsion demand governs as the structural Unbounded verdict (the
    // consumed-action discipline), never a silently-ignored 0 column.
    // Chi/ChiLt are the EN 1993-1-1 §6.3.1 flexural-buckling and §6.3.2 lateral-torsional reduction factors the design
    // report and the forward stability check read; a φ-format receipt publishes 1.0 on both because AISC folds
    // buckling INTO Fcr and Mn rather than beside them, so the columns state the format's own premise.
    // StiffnessRetention is the EN 1993-1-2 kE,θ Young's-modulus retention the FIRE lift carries onto the SAME case
    // (1.0 at ambient): stiffness never enters the strength interaction, so it rides as the forward member-stability
    // input a Rasm.Compute fire buckling check reads off the receipt rather than re-deriving from a temperature the
    // verdict no longer carries.
    public sealed record SteelMember(
        DesignBasis Basis,
        double FlexuralKnm,
        double FlexuralMinorKnm,
        double CompressionKn,
        double ShearKn,
        double TorsionalKnm,
        CompactnessClass Classification,
        double Slenderness,
        double Chi,
        double ChiLt,
        double StiffnessRetention) : SectionCapacity(Basis);   // aisc360 · aisi-s100 · en1993 · en1994, the EN 1993-1-2 fire state riding either EN row
    // The EN 1995-1-1 timber design receipt lifted WHOLE from timber#TIMBER_CAPACITY TimberCapacity (the M_Rd/N_Rd/V_Rd/
    // R_90,Rd design resistances + the relative slenderness λ_rel + the k_mod service×duration factor) — never re-derived.
    // BendingMinorKnm is the member M_Rd,z = k_h(w)·k_mod·f_m,k·S_y/γ_M (no k_crit — no LTB about the minor axis) and
    // 0.0 on a panel arm whose form declares no edgewise bending strength (the GuardedRatio fold makes a
    // panel Mz demand govern loud, never pass silent); Km the §6.1.6(2) per-form stress-redistribution weight the
    // biaxial fold swaps. TorsionalKnm is the EN 1995-1-1 §6.1.8 torsional resistance T_Rd = k_shape·f_v,d·W_tor the
    // timber owner derives over the rectangular section (the TimberCapacity.TorsionalNmm column) — positive for every
    // realized timber section, so a torsion-loaded glulam member folds demand.TorsionKnm against a real resistance,
    // never an inert 0.
    public sealed record TimberMember(
        double BendingKnm,
        double BendingMinorKnm,
        double CompressionKn,
        double ShearKn,
        double BearingPerpKnPerMm,
        double TorsionalKnm,
        double RelativeSlenderness,
        double Km,
        double Kmod) : SectionCapacity(DesignBasis.En1995);
    // The EN 1999-1-1 aluminium member — the ONE family whose design algebra lives HERE rather than on its seed page,
    // because no aluminium producer exists among admitted packages and the family owns only DATA (the banded fo/fu,
    // the class letter, the die section): the lift computes the CROSS-SECTION design resistances on the class-3
    // elastic floor (Wel·fo/γM1 both axes — no EC9 cross-section classification lands here, so the plastic credit a
    // class-1/2 section earns stays unclaimed rather than unproven; A·fo/γM1 compression; Av·fo/(√3·γM1) shear — EN
    // 1999 declares no γM0, the row's 1.10 covering cross-section and instability alike). Curve/Alpha/LambdaZero are
    // the §6.3.1.2 Table 6.6 buckling-curve columns per class letter (A: α = 0.20, λ̄0 = 0.10; B: α = 0.32,
    // λ̄0 = 0.00) the forward member-stability check reduces by over its own effective length — the section altitude
    // cannot price χ, so the combined verdict DEFERS member buckling by name rather than passing a slender column
    // silent, and BucklingRows crosses the pair to that forward check as member rows. No torsional modulus crosses
    // the die receipt, so TorsionalKnm does not exist as a column: a torsion demand folds GuardedRatio-against-0
    // and governs loud until a Wt column lands on the section receipt.
    public sealed record AluminumMember(
        DesignBasis Basis,
        double FlexuralKnm,
        double FlexuralMinorKnm,
        double CompressionKn,
        double ShearKn,
        BucklingClass Curve,
        double Alpha,
        double LambdaZero,
        double FoMpa,
        double FuMpa) : SectionCapacity(Basis) {

        // The seam publication of the member-stability pair — the two Element StructuralRows-keyed dimensionless
        // Number rows the Compute en1999 axial-compression cell reduces by (the ShearLinkRows precedent, one wire
        // shape): the columns are per-class Table 6.6 code constants, so the pair publishes WHOLE and infallibly —
        // no railed mint and no partial set, the reader's both-present gate matching this producer's completeness.
        public Seq<(PropertyName Row, PropertyValue Value)> BucklingRows() =>
            Seq((StructuralRows.BucklingAlpha, (PropertyValue)new PropertyValue.Number(Alpha)),
                (StructuralRows.BucklingPlateau, (PropertyValue)new PropertyValue.Number(LambdaZero)));
    }
    // The UNREINFORCED masonry case: the cmu#CMU_FAMILY CmuStrength strength (the TMS 402 specified f'm, read as the
    // EN 1996 characteristic f_k under that basis — one column, the basis naming its symbol) + the (grouted) net
    // ComputedSection facts the shared SectionSolver.Solve computes over the cmu SectionProfile.CellularRectangle (the
    // as-built net, VoidCell.Grouted cells filled) — net area AND BOTH net elastic moduli (SxMm3/SyMm3, so a pier bent
    // about both axes folds each moment against ITS modulus, never a resultant against the major alone) — + the
    // basis-minted slenderness reduction (the TMS 402 bracket or the EN 1996 §6.1.2.2 Φ, MasonryReduction owning both)
    // the unity check scales. FlexuralTensionMpa is the CHARACTERISTIC tension-fibre limit
    // the Lift resolves off the basis's own table — the TMS 402 Table 9.1.9.2 modulus of rupture fr, or the EN 1996-1-1
    // Table 3.4 f_xk — the complement to the compression fibre (~0.05-2.3 MPa against ~11 MPa design stress) and the
    // governing axis of every low-axial unreinforced wall on either basis; the arm applies the basis's own φ or γM, so
    // both columns stay pre-factor and one reader compares them. ShearBondMpa is the zero-compression shear bond BOTH
    // codes publish — the TMS §9.2.6.1 running-bond 0.386 MPa (56 psi) constant and the EN Table 3.5 f_vk0 — hoisted
    // onto one column so the shear arm reads DATA rather than a literal buried per basis.
    public sealed record MasonryUnreinforced(
        DesignBasis Basis,
        double FmMpa,
        double NetAreaMm2,
        double SectionModulusXMm3,
        double SectionModulusYMm3,
        double SlendernessReduction,
        double FlexuralTensionMpa,
        double ShearBondMpa) : SectionCapacity(Basis);   // TMS 402 §9.1/§9.2 · EN 1996-1-1 §6.1/§6.2/§6.3
    // The REINFORCED masonry case over the cmu lattice facts: f'm, the bar-grade yield, the reinforced-cell steel area,
    // the grouted net area, the out-of-plane lever d (mid-wall bars: W/2), the per-unit bed length b, and the member
    // slenderness reduction — the steel-couple flexural arm plus the reinforced axial the unreinforced case's
    // no-steel-term admission law reserved for exactly this case.
    public sealed record MasonryReinforced(
        DesignBasis Basis,
        double FmMpa,
        double FyMpa,
        double SteelAreaMm2,
        double NetAreaMm2,
        double EffectiveDepthMm,
        double BedLengthMm,
        double SlendernessReduction) : SectionCapacity(Basis);   // TMS 402 §9.3 · EN 1996-1-1 §6.6
    // The EN 16612 glazing pane resistance lifted WHOLE from glazing#GLAZING_FAMILY GlassCapacity: the governing pane's
    // per-metre-strip design moment, its design bending strength, and the effective laminate thickness the report reads.
    // LoadShareFraction is the EN 16612 insulating-unit share the governing pane draws — the pressure a Demand states
    // is the WHOLE unit's, so the fold applies the share before dividing. Carrying it here rather than pre-multiplying
    // the resistance keeps both numbers readable in a design report.
    public sealed record GlassPane(
        double BendingKnmPerM,
        double ResistanceMpa,
        double EffectiveThicknessMm,
        double LoadShareFraction) : SectionCapacity(DesignBasis.En16612);
    // The connection load-path case: the lifted line/area/group shear, the tension (uplift) column, and the seat-bearing
    // (download) column — a 0 column is an unresisted axis the GuardedRatio fold makes govern loud, so one case carries
    // the weld, adhesive, stud-group, and connector receipts without per-kind capacity surfaces.
    // TensionKn is OPTIONAL because a weld line, a stud group, and an epoxy lap publish NO tension band at all while a
    // hold-down publishes an uplift value that may be zero on an unresisted direction — a distinction a design report
    // reads off the column and a lift can never flatten. The CHECK folds both as an unprovided capacity, so an uplift
    // demand against either is the structural Unbounded verdict and only a positive published band divides.
    // The basis is PER LIFT ARM here rather than per case: a weld line, an epoxy lap, a stud group, an evaluated
    // connector, a bolted joint, and a dowelled timber joint are one capacity SHAPE under six publishing bodies, so
    // the load path's jurisdiction rides the column the lift fills and no arm loses its citation to a shared default.
    // LateralF2Kn and Combines carry the connector report's SECOND lateral direction and its own interaction rule.
    // A report either publishes the two directions as an interacting pair — a resultant must fit inside the combined
    // envelope, so the ratios SUM — or as independent checks each verified on its own axis, where the WORST governs.
    // The report states which, so the rule rides the receipt; one fold guessing a single envelope for both
    // conventions either over-rates an interacting connector or refuses an independent one.
    // Defer is the load path's section-undecidable obligation, riding the shear and tension candidates: the cast-in
    // anchor lift carries the EN 1992-4 forward-modes deferral here — its single-anchor cone (and EN steel where
    // tabulated) is fully priced, the group/edge/pullout/steel-band modes the forward fixture-level check finishes —
    // and every other lift passes None.
    public sealed record Connection(
        DesignBasis Basis,
        double ShearKn,
        Option<double> TensionKn,
        double BearingKn,
        Option<double> LateralF2Kn = default,
        bool Combines = false,
        Option<MemberCheckRequirement> Defer = default) : SectionCapacity(Basis);   // aws-d1-1 · astm-d1002 · aisc360 · icc-es · en1993-1-8 · en1995 · en1992-4
    // The IN-PLANE sheathed-assembly case: a shear wall or a diaphragm priced by its own tabulated unit shear. The
    // column is the FINISHED design unit shear — the SDPWS §4.1.4 reduction ran exactly once at the family producer
    // where the rail and the placement's hazard both exist, the same posture the connector receipts take — so this
    // arm applies nothing further and a report reads the hazard beside the value it already governs.
    public sealed record LateralPanel(
        DesignBasis Basis,
        double DesignKnPerM,
        LateralHazard Hazard) : SectionCapacity(Basis);   // sdpws
    // The detail-category fatigue surface: the law IS the capacity — a permissible direct-stress range as a
    // function of the demanded cycle count — so the case carries the FatigueLaw whole and its basis derives from
    // the law's own ladder. Every static Demand column is unresisted here and governs loud: a fatigue surface
    // checks cycles, and a static action stated against one is a modelling error the verdict must surface.
    public sealed record Fatigue(FatigueLaw Law) : SectionCapacity(Law.Basis);   // en1993-1-9 · aisc-app3
    // The DG1 base plate as TWO precomputed axial capacities: BearingKn the φ·0.85·f'c·A1·√(A2/A1) concrete bearing
    // (φ = 0.65, √(A2/A1) ≤ 2 — AISC 360 §J8), PlateBendingKn the axial load at which the required cantilever
    // thickness equals the plate's own (t_min = l·√(2·Pu/(0.9·Fy·B·N)) inverted at t, l = max(m, n, n′) with the
    // two-sourced λ = 1 conservative bound) — both demand-linear in the download, so the verdict rides the standard
    // fold with no bespoke check shape. A moment-transferring or uplift base rides its anchor receipts and the
    // moment/uplift columns govern loud here. The φ pair are per-action constants on this arm, the aisc360 row's
    // own posture for a φ-format jurisdiction.
    public sealed record BasePlate(
        double BearingKn,
        double PlateBendingKn) : SectionCapacity(DesignBasis.Aisc360);   // AISC DG1 / §J8, wide-flange cantilever method

    // The demand-vs-capacity verdict, one polymorphic Check over the closed family — never per-type and never
    // per-code. Each arm divides demand by ITS OWN resistance columns and hands the normalized triple to the case's
    // DesignBasis.Interact kernel, so the jurisdiction's combined-action algebra lives on the basis row while the
    // resistance reading stays with the family that owns it: the RcInteraction arm ray-casts the demand against the
    // hull; the RcElastic arm the WORST of the EC2 SLS combined extreme-CONCRETE-fibre cracking stress and the EC2
    // §6.2 shear screen; the SteelMember arm the per-axis biaxial ratios folded through §H1.1 or §6.3.3 and
    // worst-folded with shear and torsion; the TimberMember arm the EN 1995-1-1 §6.3.2/§6.2.4 km-swapped pair
    // worst-folded with shear, §6.1.8 torsion, and §6.1.5 bearing; the MasonryUnreinforced arm the basis-selected
    // strength-design or partial-factor resistances folded through the unity sum with its flexural-tension and shear
    // screens; the MasonryReinforced arm the steel-couple unity sum with the reinforcement-spacing shear screen; the
    // GlassPane arm the EN 16612 per-metre plate-bending fold; the Connection arm the shear/tension/bearing load-path
    // triple; the AluminumMember arm the EN 1999 elastic-floor unity sum with its member-buckling deferral; the
    // Fatigue arm the detail-category range against the law's design resistance at the demanded count; the BasePlate
    // arm the download against the bearing/plate-bending pair. Every arm is TOTAL over the member-action columns
    // (axial, both moments, both shears, torsion, bearing): an action the case's capacity surface does not resist
    // folds through GuardedRatio against 0 and governs loud — a hull shear, an RC torsion, a steel bearing, a
    // masonry torsion, a static action on a fatigue surface can never pass silent (the consumed-action discipline).
    // The MODALITY columns bind to their own cases — the unit shear to LateralPanel, the range/count pair to
    // Fatigue — a member arm neither resists nor reads them, and the check that consumes them is its own invocation.
    public Utilisation Check(Demand demand) => Switch(
        rcInteraction: h => Cast(h.Hull, demand),
        rcElastic: e => RcElasticUtilisation(e, demand),
        steelMember: s => SteelUtilisation(s, demand),
        timberMember: t => TimberUtilisation(t, demand),
        aluminumMember: a => AluminumUtilisation(a, demand),
        masonryUnreinforced: m => MasonryUtilisation(m, demand),
        masonryReinforced: m => MasonryReinforcedUtilisation(m, demand),
        glassPane: g => GlassUtilisation(g, demand),
        connection: c => ConnectionUtilisation(c, demand),
        lateralPanel: p => LateralUtilisation(p, demand),
        fatigue: f => FatigueUtilisation(f, demand),
        basePlate: p => BasePlateUtilisation(p, demand));

    // One RC elastic arm, two limit-state ratios: the SLS cracking fibre stress and the ULS shear screen fold through
    // the same Worst governing-axis law every other arm drives — never a second RC surface for the shear check.
    // EXPRESSION_SPINE measured-kernel exemption: the intermediate candidate bindings feed one closed Worst fold.
    static Utilisation RcElasticUtilisation(RcElastic e, Demand demand) {
        (Option<double> cracking, GoverningAction axis) = Cracking(e, demand);
        Option<double> shear = GuardedRatio(demand.ShearResultantKn, ShearResistanceKn(e));
        // A LINKED section defers stirrup detailing whichever action governs — the §6.2.3(3) V_Rd,s spacing is the ONE
        // obligation, so it rides the shear candidate AND the whole-verdict wrap through the SAME row, never a second
        // spelling of one clause.
        Option<MemberCheckRequirement> linked = e.ShearLinkAreaMm2 > 0.0 ? Some(MemberCheckRequirement.RcShearReinforcement) : None;
        // The stirrup obligation rides the two candidates the §6.2.3(3) spacing actually finishes — the cracking fold
        // and the shear screen. Torsion and bearing are UNRESISTED at this altitude, not deferred: the section
        // publishes no torsional or bearing resistance at all, and labelling those with a stirrup-spacing obligation
        // told a design report that detailing would complete a check no amount of detailing completes.
        return Worst(
            (cracking, axis, linked),
            (shear, GoverningAction.Shear, linked),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, None));
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
    // The candidate rides FibreRatio, not GuardedRatio: the stress is SIGNED, so a compressive service state stays a
    // negative ratio that loses the fold instead of a magnitude reporting a cracking governance it does not have.
    static (Option<double> Ratio, GoverningAction Governing) Cracking(RcElastic e, Demand demand) {
        double axialStress = demand.AxialKn * 1e3 / Math.Max(e.ConcreteAreaMm2, double.Epsilon);                  // signed N/A (MPa)
        double bendingYStress = Math.Abs(demand.MomentYKnm) * 1e6 * (e.DepthMm * 0.5) / Math.Max(e.GrossInertiaYyMm4, double.Epsilon);
        double bendingZStress = Math.Abs(demand.MomentZKnm) * 1e6 * (e.WidthMm * 0.5) / Math.Max(e.GrossInertiaZzMm4, double.Epsilon);
        double tensileStress = axialStress + bendingYStress + bendingZStress;                                     // max tensile fibre (MPa)
        GoverningAction governing = Math.Max(bendingYStress, bendingZStress) >= Math.Abs(axialStress)
            ? GoverningAction.Flexure : GoverningAction.Axial;   // either bending axis dominating is a FLEXURE verdict — biaxial-moment names only the hull ray
        return (FibreRatio(tensileStress, e.FctmMpa), governing);
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
        return e.ShearLinkAreaMm2 > 0.0 ? e.VrdMaxKn : vrdc;
    }

    // Combined axial-flexure through the receipt's OWN basis kernel: AISC 360 §H1.1's two-branch form (p + 8/9·m at
    // p >= 0.2, p/2 + m below — the COMBINED interaction a max-of-independents under-predicts, p = m = 0.9 passing a
    // max fold yet failing H1.1 at 1.7) or EN 1993-1-1 §6.3.3's linear eq 6.61/6.62 sum, selected by the row and never
    // by a branch here. Both bases read the PER-AXIS ratios Mry/Mcx and Mrz/Mcy — the moment resultant folded against
    // the major-axis resistance alone is the DELETED unconservative spelling (it credited a weak-axis moment the full
    // major/minor ratio, 3-10x on an I-shape). The combined ratio worst-folds with the shear and torsion ratios; the
    // CompactnessClass and the χ/χLT reductions ride the carrier for the design report. Torsion folds demand.TorsionKnm
    // against the lifted torsional resistance (0.0 ⇒ a zero-torsion demand stays 0, a nonzero torsion demand against an
    // unpublished resistance is the absent candidate that governs the whole verdict).
    // The three interaction operands are each a candidate ratio, so the combined candidate is their SEQUENCE: a
    // jurisdiction's kernel is an algebra over three dimensionless numbers, and an operand the section cannot state
    // gives the kernel nothing to fold — the absence propagates to the combined candidate rather than the kernel
    // summing a manufactured magnitude.
    static Utilisation SteelUtilisation(SteelMember s, Demand demand) {
        Option<double> combined =
            from axial in GuardedRatio(demand.AxialKn, s.CompressionKn)
            from major in GuardedRatio(demand.MomentYKnm, s.FlexuralKnm)
            from minor in GuardedRatio(demand.MomentZKnm, s.FlexuralMinorKnm)
            select s.Basis.Interact(new InteractionOperands(axial, major, minor, MinorWeight: 1.0, Slender: true));
        return Worst(
            (combined, GoverningAction.Combined, None),
            (GuardedRatio(demand.ShearResultantKn, s.ShearKn), GoverningAction.Shear, None),
            // An OPEN shape's φTn is engineering-zero because §H3.3 warping torsion is not one resistance, so a
            // torsion demand on it takes the absent candidate and governs the verdict; the clause rides the candidate
            // for the ratio-present state, where detailing genuinely finishes the check.
            (GuardedRatio(demand.TorsionKnm, s.TorsionalKnm), GoverningAction.Torsion,
                s.TorsionalKnm > 0.0 ? None : Some(MemberCheckRequirement.SteelWarpingTorsion)),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, None));
    }

    // The EN 1995 basis kernel folds the km-swapped two-equation MAX pair — §6.3.2 eq 6.23/6.24 with the LINEAR axial
    // term when buckling governs (λ_rel > 0.3, N_Rd already k_c-reduced), §6.2.4 eq 6.19/6.20 with the QUADRATIC n²
    // for the stocky member — over the operands this arm normalizes; km is the lifted §6.1.6(2) per-form weight the
    // row swaps between the axes. my/mz ride GuardedRatio so a panel BendingMinorKnm of 0 makes an
    // in-plane Mz demand govern loud (never a silent pass) while a zero Mz stays inert; the moment resultant folded
    // against the major M_Rd alone is the DELETED unconservative spelling. Worst-folded with the shear, §6.1.8
    // torsion, and §6.1.5 bearing ratios (BearingKn folds against the lifted R_90,Rd — a consumed action, never a
    // carried-but-ignored capacity column).
    static Utilisation TimberUtilisation(TimberMember t, Demand demand) {
        Option<double> combined =
            from axial in GuardedRatio(demand.AxialKn, t.CompressionKn)
            from major in GuardedRatio(demand.MomentYKnm, t.BendingKnm)
            from minor in GuardedRatio(demand.MomentZKnm, t.BendingMinorKnm)
            select t.Basis.Interact(new InteractionOperands(axial, major, minor, t.Km, t.RelativeSlenderness > 0.3));
        Option<double> shear = GuardedRatio(demand.ShearResultantKn, t.ShearKn);
        // §6.1.5 bearing is section-UNDECIDABLE: the receipt carries R_90,Rd PER MM of bearing length, and the length
        // is the support DETAILING a cross-section does not hold, so a bearing demand attaches its obligation to the
        // WHOLE verdict — the RcElastic linked-stirrup posture — rather than dividing against a fabricated w×d area.
        Option<MemberCheckRequirement> bearing =
            Math.Abs(demand.BearingKn) > double.Epsilon ? Some(MemberCheckRequirement.TimberBearingLength) : None;
        // BendingMinorKnm is 0.0 only where a PANEL form declares no edgewise strength (a member always prices M_Rd,z),
        // so an in-plane panel Mz demand takes the absent minor operand and governs the verdict, the edgewise clause
        // riding the candidate for the state that still publishes a ratio.
        return Worst(
            (combined, GoverningAction.Combined,
                t.BendingMinorKnm > 0.0 || Math.Abs(demand.MomentZKnm) <= double.Epsilon
                    ? bearing : Some(MemberCheckRequirement.CltInPlaneBending)),
            (shear, GoverningAction.Shear, bearing),
            (GuardedRatio(demand.TorsionKnm, t.TorsionalKnm), GoverningAction.Torsion, bearing));
    }

    // EN 1999 §6.2 cross-section unity through the row's Linear kernel over the per-axis elastic ratios — the same
    // consumed-action discipline as every member arm: shear folds against the computed Av·fo/(√3·γM1), torsion and
    // bearing against 0 (no torsional modulus and no bearing band cross the die receipt — both govern loud). The
    // combined candidate carries the §6.3 member-buckling deferral by NAME: the case publishes the α/λ̄0 curve
    // columns, and the χ reduction over the effective length is the forward member check's — a slender extrusion
    // therefore reports SectionPasses-with-obligation, never a silent full pass.
    static Utilisation AluminumUtilisation(AluminumMember a, Demand demand) {
        Option<double> combined =
            from axial in GuardedRatio(demand.AxialKn, a.CompressionKn)
            from major in GuardedRatio(demand.MomentYKnm, a.FlexuralKnm)
            from minor in GuardedRatio(demand.MomentZKnm, a.FlexuralMinorKnm)
            select a.Basis.Interact(new InteractionOperands(axial, major, minor, MinorWeight: 1.0, Slender: true));
        return Worst(
            (combined, GoverningAction.Combined, Some(MemberCheckRequirement.AluminumMemberBuckling)),
            (GuardedRatio(demand.ShearResultantKn, a.ShearKn), GoverningAction.Shear, None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, None));
    }

    // The detail-category verdict: Δσ_Ed against the law's design range at the demanded count. A zero range is the
    // trivially satisfied candidate; a real range with no lawful count (below one cycle) is the absent candidate —
    // no S-N law prices it, so the verdict is structural Unbounded, never a fabricated finite resistance. Every
    // static action is unresisted at fatigue altitude and governs loud.
    static Utilisation FatigueUtilisation(Fatigue f, Demand demand) {
        Option<double> range = demand.StressRangeMpa <= double.Epsilon
            ? Some(0.0)
            : demand.CycleCount >= 1.0
                ? GuardedRatio(demand.StressRangeMpa, f.Law.DesignMpa(demand.CycleCount))
                : None;
        return Worst(
            (range, GoverningAction.Fatigue, None),
            (GuardedRatio(demand.AxialKn, 0.0), GoverningAction.Axial, None),
            (GuardedRatio(demand.MomentResultantKnm, 0.0), GoverningAction.Flexure, None),
            (GuardedRatio(demand.ShearResultantKn, 0.0), GoverningAction.Shear, None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, None));
    }

    // The base-plate verdict: the DOWNLOAD (compressive axial) against the two precomputed axial capacities — the
    // §J8 bearing and the DG1 cantilever plate bending — worst-folded so the report names WHICH of the two governs.
    // Uplift rides the anchor receipts (a positive axial here meets no tension band and governs loud), and a
    // transferred moment is the DG1 moment-plate method this arm does not price.
    static Utilisation BasePlateUtilisation(BasePlate p, Demand demand) {
        double download = Math.Max(0.0, -demand.AxialKn);
        return Worst(
            (GuardedRatio(download, p.BearingKn), GoverningAction.Bearing, None),
            (GuardedRatio(download, p.PlateBendingKn), GoverningAction.Flexure, None),
            (GuardedRatio(Math.Max(demand.AxialKn, 0.0), 0.0), GoverningAction.Axial, None),
            (GuardedRatio(demand.MomentResultantKnm, 0.0), GoverningAction.Flexure, None),
            (GuardedRatio(demand.ShearResultantKn, 0.0), GoverningAction.Shear, None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None));
    }

    // The ONE candidate-ratio constructor every arm's demand/capacity pair rides, and the ONLY site that decides
    // whether a candidate EXISTS: a zero demand is trivially satisfied (so an unpriced capacity column never
    // spuriously governs an unloaded member), a real demand over a positive finite capacity divides, and a real demand
    // against a capacity the case does not publish — the open-shape φTn = 0, an edgewise-strengthless CLT panel's
    // BendingMinorKnm = 0, the hull's absent shear axis, a non-finite lifted column — is ABSENT, the structural
    // failure the fold reads as Unbounded. Absence rather than a magnitude is what makes every Demand column a
    // consumed action on every case: no arithmetic can manufacture a passing ratio out of a resistance nobody stated.
    // Both sides carry the totality guard because both can arrive unstatable: a degenerate section geometry divides a
    // stress into a non-finite demand term exactly as a receipt can carry a non-finite column, and neither is a ratio.
    static Option<double> GuardedRatio(double demand, double capacity) =>
        (Math.Abs(demand) <= double.Epsilon, double.IsFinite(demand) && capacity > 0.0 && double.IsFinite(capacity)) switch {
            (true, _)      => Some(0.0),
            (false, true)  => Some(Math.Abs(demand) / capacity),
            (false, false) => None,
        };

    // The SIGNED counterpart both fibre screens ride — the EC2 cracking stress against fctm and the unreinforced
    // tension stress against fr/f_xk. It is a sibling of GuardedRatio on ONE discriminant: a fibre stress carries its
    // sign as physics, because compression RELIEVES the fibre and must lose the fold, where a magnitude would report a
    // cracking or rupture governance the state does not have. A compressed fibre demands nothing of the limit; a
    // tensile fibre against a limit the basis does not publish (fr = 0 under a stack-bond Type O mortar), or a stress
    // a degenerate section geometry drove non-finite, is the absent candidate.
    static Option<double> FibreRatio(double stress, double limit) =>
        (double.IsFinite(stress) && limit > 0.0, stress <= 0.0) switch {
            (true, _)      => Some(stress / limit),
            (false, true)  => Some(0.0),
            (false, false) => None,
        };

    // The unreinforced verdict over ONE ratio structure and TWO code kernels: the axial-flexural unity sum through the
    // basis's own Interact, the flexural-tension screen, and the shear screen, each resistance minted by
    // MasonryResistances per basis. A net-TENSION axial governs outright on either code (TMS §9.2.5 neglects URM axial
    // tensile strength; EN 1996 §6.1 admits none), and the tension screen σt = |My|/Sx + |Mz|/Sy + N/A (MPa, the
    // SIGNED Demand axial — compression RELIEVES tension per Mu/S − Pu/A) rides the pre-factor limit the resistance
    // fold already factored. Torsion and bearing are unresisted at wall altitude and govern loud through GuardedRatio.
    static Utilisation MasonryUtilisation(MasonryUnreinforced m, Demand demand) {
        (double pn, double mnx, double mny, double tensionLimit, double vnKn) = MasonryResistances(m, demand);
        // The axial branch is on the CAPACITY column, never on two ratio constructions: a net-tension demand meets the
        // axial tensile resistance neither code grants URM (0), a compression demand the slenderness-reduced Pn.
        Option<double> combined =
            from axial in GuardedRatio(demand.AxialKn, demand.AxialKn > 0.0 ? 0.0 : pn)
            from major in GuardedRatio(demand.MomentYKnm, mnx)
            from minor in GuardedRatio(demand.MomentZKnm, mny)
            select m.Basis.Interact(new InteractionOperands(axial, major, minor, MinorWeight: 1.0, Slender: true));
        double sigmaT = Math.Abs(demand.MomentYKnm) * 1e6 / Math.Max(m.SectionModulusXMm3, double.Epsilon)
            + Math.Abs(demand.MomentZKnm) * 1e6 / Math.Max(m.SectionModulusYMm3, double.Epsilon)
            + demand.AxialKn * 1e3 / Math.Max(m.NetAreaMm2, double.Epsilon);
        return Worst(
            (combined, GoverningAction.Combined, None),
            (FibreRatio(sigmaT, tensionLimit), GoverningAction.Flexure, None),
            (GuardedRatio(demand.ShearResultantKn, vnKn), GoverningAction.Shear, None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, None));
    }

    // The PER-BASIS unreinforced-masonry kernel — the one site where the two jurisdictions' resistance algebra
    // differs, so the ratio fold above stays single and no arm reads a code constant the other basis never publishes.
    // TMS 402 strength design (§9.1.4 UNREINFORCED φ = 0.60 flexure+axial — the reinforced 0.90 on a steel-less Pn was
    // the deleted unconservative form — φv = 0.80 shear): the §9.2 slenderness-reduced compression φPn =
    // 0.80·φ·0.80·f'm·An·R and the per-axis φMn = φ·0.80·f'm·S, the 0.80 stress-block cap being the maximum masonry
    // compressive stress reinforced and unreinforced alike (a full-f'm fibre over-prices flexure 25%); the §9.2.2
    // tension limit φ·fr, where fr = 0 (StackOther, Type O/K mortar) makes any net tension govern outright,
    // code-faithful; the §9.2.6.1 shear minimum over the material arm 0.315·√f'm·Anv (the 3.8·√f'm psi arm) and the
    // running-bond arm ShearBond·Anv + 0.45·Nu (the factored-compression benefit — the conservative floor for the
    // solidly-grouted 0.621 arm; a stack-bond pier's 0.158 arm is the bond-axis growth case), with the 2.07 MPa
    // (300 psi) ceiling clamping the RESOLVED value at the outlet because the code caps the GOVERNING shear — folding
    // the ceiling into one arm let the other exceed it.
    // EN 1996-1-1 partial-factor design: f_d = f_k/γM0 with §6.1.2.1 N_Rd = Φ·A·f_d over the §6.1.2.2 Φ the
    // MasonryReduction owner mints, §6.3 out-of-plane bending resisted by f_xd = f_xk/γM0 over the section modulus
    // (the code prices unreinforced flexure on the FLEXURAL strength alone, so the bending and tension screens read
    // one limit and coincide — the compressive-fibre flexural arm TMS carries has no EN counterpart and inventing one
    // would credit resistance the code does not grant), and §6.2 shear f_vd = (f_vk0 + 0.4·σ_d)/γM0 over the
    // compressed area, σ_d the design compressive stress the demand's own axial supplies.
    // EXPRESSION_SPINE measured-kernel exemption: the code constants and arm scalars bind once, one tuple exits.
    static (double Pn, double Mnx, double Mny, double Tension, double Vn) MasonryResistances(MasonryUnreinforced m, Demand demand) {
        double compression = Math.Max(0.0, -demand.AxialKn);
        if (m.Basis == DesignBasis.En1996) {
            double fd = m.FmMpa / m.Basis.GammaM0, fxd = m.FlexuralTensionMpa / m.Basis.GammaM0;
            double sigmaD = compression * 1e3 / Math.Max(m.NetAreaMm2, double.Epsilon);
            return (m.SlendernessReduction * m.NetAreaMm2 * fd * 1e-3,
                fxd * m.SectionModulusXMm3 * 1e-6,
                fxd * m.SectionModulusYMm3 * 1e-6,
                fxd,
                (m.ShearBondMpa + 0.4 * sigmaD) / m.Basis.GammaM0 * m.NetAreaMm2 * 1e-3);
        }
        // Every code constant is a BASIS column: φ, φv, the stress-block cap, and the governing-shear ceiling read off
        // the row, so the §9.2 slenderness coefficient below is the only literal this arm still spells and a second
        // strength-design jurisdiction runs the same body off its own columns.
        double phi = m.Basis.PhiFlexure, phiV = m.Basis.PhiShear, block = m.Basis.StressBlock;
        double material = Math.Min(0.315 * Math.Sqrt(m.FmMpa) * m.NetAreaMm2 * 1e-3, m.ShearBondMpa * m.NetAreaMm2 * 1e-3 + 0.45 * compression);
        return (SlendernessCoefficient * phi * block * m.FmMpa * m.NetAreaMm2 * m.SlendernessReduction * 1e-3,
            phi * block * m.FmMpa * m.SectionModulusXMm3 * 1e-6,
            phi * block * m.FmMpa * m.SectionModulusYMm3 * 1e-6,
            phi * m.FlexuralTensionMpa,
            phiV * m.Basis.ShearCeilingMpa.Match(
                Some: ceiling => Math.Min(material, ceiling * m.NetAreaMm2 * 1e-3),
                None: () => material));
    }

    // The reinforced verdict is ONE steel-couple algebra both codes publish, the basis supplying only the STRESS
    // SCALARS: the §9.3.4.1.1 / §6.6.2 reinforced axial Pn = 0.80·[fm·(An − As) + fy·As]·R, the §9.3.5 / §6.6.1
    // steel-couple flexure Mn = As·fy·(d − a/2) over the a = As·fy/(fm·b) stress block about the out-of-plane bed
    // axis, a NET-TENSION axial resisted by the steel alone (the unreinforced tension-governs-outright arm retires for
    // the reinforced state), and the §9.3.4.1.2 masonry shear screen Vnm = 0.083·(4 − 1.75·min(M/(V·dv), 1))·Anv·√f'm
    // pinned at the M/(V·dv) = 1 conservative bound — the ONE masonry-shear form both bases run, because the EN
    // §6.7.2 reinforced arm and the TMS one alike complete only with the bar spacing, so neither can finish here.
    // The reinforcement shear term Vns needs that SPACING the section does not carry, so shear detailing stays the
    // forward member check's; an in-plane Mz demand folds
    // GuardedRatio-against-0 loud, because bar STATIONS along the bed length are lattice member facts, never section
    // columns.
    // EXPRESSION_SPINE measured-kernel exemption: the code constants and arm scalars bind once, one Worst fold exits.
    static Utilisation MasonryReinforcedUtilisation(MasonryReinforced m, Demand demand) {
        (double fm, double fy, double phi, double phiV) = ReinforcedStresses(m);
        double pn = 0.80 * (fm * Math.Max(m.NetAreaMm2 - m.SteelAreaMm2, 0.0) + fy * m.SteelAreaMm2) * m.SlendernessReduction * 1e-3;
        double block = m.SteelAreaMm2 * fy / Math.Max(fm * m.BedLengthMm, double.Epsilon);
        double mn = m.SteelAreaMm2 * fy * Math.Max(m.EffectiveDepthMm - block / 2.0, 0.0) * 1e-6;
        // A net-TENSION axial is carried by the steel alone and a compression by the reinforced Pn — one ratio
        // construction, the direction selecting the CAPACITY column the reinforced state actually publishes.
        Option<double> combined =
            from axial in GuardedRatio(demand.AxialKn, phi * (demand.AxialKn > 0.0 ? m.SteelAreaMm2 * fy * 1e-3 : pn))
            from major in GuardedRatio(demand.MomentYKnm, phi * mn)
            select m.Basis.Interact(new InteractionOperands(axial, major, Minor: 0.0, MinorWeight: 1.0, Slender: true));
        double vnm = 0.083 * 2.25 * m.NetAreaMm2 * Math.Sqrt(m.FmMpa) * 1e-3;
        // Vnm alone is the section-decidable shear: Vns needs the bar SPACING, so a shear-governed reinforced verdict
        // DEFERS to the member check rather than reporting a resistance the section cannot complete.
        return Worst(
            (combined, GoverningAction.Combined, None),
            (GuardedRatio(demand.MomentZKnm, 0.0), GoverningAction.Flexure, None),
            (GuardedRatio(demand.ShearResultantKn, phiV * vnm), GoverningAction.Shear,
                Some(MemberCheckRequirement.ReinforcedMasonryShearSpacing)),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, None));
    }

    // The reinforced stress pair per basis, the WHOLE per-code difference in the steel-couple algebra: TMS 402 §9.3
    // works on the nominal 0.80·f'm block and the bar yield with φ = 0.90 flexure/axial and φv = 0.80 shear applied to
    // the resistances; EN 1996-1-1 §6.6 works on already-factored design strengths — f_d = f_k/γM and
    // f_yd = f_yk/γs with the §2.4.3 reinforcing-steel γs = 1.15 — so its resistance factors are unity and no φ is
    // applied twice.
    static (double Fm, double Fy, double Phi, double PhiV) ReinforcedStresses(MasonryReinforced m) =>
        m.Basis == DesignBasis.En1996
            ? (m.FmMpa / m.Basis.GammaM0, m.FyMpa / m.Basis.GammaS, 1.0, 1.0)
            : (0.80 * m.FmMpa, m.FyMpa, 0.90, 0.80);

    // EN 16612 pane check per metre strip: BOTH plate bending directions fold against the SAME isotropic per-metre
    // resistance, their SUM the conservative combined-stress bound, scaled by the governing pane's own INSULATING-UNIT
    // load share — a Demand states the pressure on the unit, and the sealed cavity partitions it by pane stiffness, so
    // dividing the whole-unit moment by one pane's resistance over-rated every asymmetric build. In-plane axial, shear,
    // torsion, and bearing are unresisted at pane altitude and govern loud through GuardedRatio.
    static Utilisation GlassUtilisation(GlassPane g, Demand demand) =>
        Worst(
            (GuardedRatio((Math.Abs(demand.MomentYKnm) + Math.Abs(demand.MomentZKnm)) * g.LoadShareFraction, g.BendingKnmPerM),
                GoverningAction.Flexure, None),
            (GuardedRatio(demand.AxialKn, 0.0), GoverningAction.Axial, None),
            (GuardedRatio(demand.ShearResultantKn, 0.0), GoverningAction.Shear, None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, None));

    // The connection verdict over the load path's three resisted axes: the shear resultant against the lifted line/
    // area/group/bolt shear, a POSITIVE axial (tension, uplift) against the tension column, and the seat reaction (a
    // hanger's download) against the bearing column; a compressive axial rides the member, and moments/torsion are
    // unresisted at connection altitude and govern loud — the connector's private demand-ratio mini-rail is DELETED
    // in favour of this one fold, its direction vocabulary living on in the lift columns. A receipt that published no
    // tension band states an unprovided capacity, so the uplift candidate reads it as the zero column it is and the
    // ONE guarded construction covers both an absent band and a published zero — a tension demand against either is
    // the structural verdict, and the report reads which of the two the receipt held off the column itself.
    static Utilisation ConnectionUtilisation(Connection c, Demand demand) =>
        Worst(
            (LateralRatio(c, demand), GoverningAction.Shear, c.Defer),
            (GuardedRatio(Math.Max(demand.AxialKn, 0.0), c.TensionKn.IfNone(0.0)), GoverningAction.Axial, c.Defer),
            (GuardedRatio(demand.BearingKn, c.BearingKn), GoverningAction.Bearing, None),
            (GuardedRatio(demand.MomentResultantKnm, 0.0), GoverningAction.Flexure, None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None));

    // The in-plane arm: the unit-shear demand against the assembly's own design unit shear, every member action
    // unresisted at sheathing altitude and governing loud through GuardedRatio. A sheathed panel carries shear in its
    // plane and nothing else — a moment or a bearing reaction stated against one is a modelling error the verdict
    // must surface rather than a column this case quietly ignores.
    static Utilisation LateralUtilisation(LateralPanel p, Demand demand) =>
        Worst(
            (GuardedRatio(demand.UnitShearKnPerM, p.DesignKnPerM), GoverningAction.InPlaneShear, None),
            (GuardedRatio(demand.AxialKn, 0.0), GoverningAction.Axial, None),
            (GuardedRatio(demand.MomentResultantKnm, 0.0), GoverningAction.Flexure, None),
            (GuardedRatio(demand.ShearResultantKn, 0.0), GoverningAction.Shear, None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, None));

    // The connector report's own lateral rule: a SINGLE published direction reads the demand resultant against it,
    // an INTERACTING pair sums the two per-axis ratios, and an INDEPENDENT pair takes the worse of them — the rule
    // choosing the COMBINATION over one sequenced pair, so either axis lacking its resistance leaves the whole lateral
    // candidate absent under both conventions.
    static Option<double> LateralRatio(Connection c, Demand demand) =>
        c.LateralF2Kn.Match(
            Some: f2 =>
                from primary in GuardedRatio(demand.ShearYKn, c.ShearKn)
                from secondary in GuardedRatio(demand.ShearZKn, f2)
                select c.Combines ? primary + secondary : Math.Max(primary, secondary),
            None: () => GuardedRatio(demand.ShearResultantKn, c.ShearKn));

    // The worst candidate over the span — the unified governing-axis fold every arm drives, so a steel/timber/masonry
    // check reports WHICH action governs, not just a ratio; the span-params buffer stack-allocates per Check, one
    // verdict is minted at the exit, and the strict-greater fold keeps the earliest-maximal tie-break without a
    // per-call array. ABSENCE DOMINATES BY STRUCTURE: a candidate with no ratio demanded a capacity its case does not
    // provide, so it outranks every present ratio and every later absence, and the verdict is Unbounded because the
    // fold reached a candidate that cannot publish a number — never because a magnitude compared against a sentinel.
    // A present candidate carries its own DEFERRAL, so a governing section-undecidable clause (the linked section's
    // §6.2.3(3) stirrup spacing, the reinforced-masonry V_ns) folds to RequiresMemberCheck WITH its ratio and the
    // distinction between a failed check and a check the section cannot finish survives to the design report; an
    // ABSENT candidate carries its clause no further, because a deferring verdict is defined by the ratio it publishes
    // and there is none — no amount of member detailing rescues a resistance the case never stated.
    static Utilisation Worst(params ReadOnlySpan<(Option<double> Ratio, GoverningAction Action, Option<MemberCheckRequirement> Defer)> candidates) {
        (Option<double> Ratio, GoverningAction Action, Option<MemberCheckRequirement> Defer) won =
            Iterable<(Option<double> Ratio, GoverningAction Action, Option<MemberCheckRequirement> Defer)>.FromSpan(candidates[1..])
                .Fold(candidates[0], static (best, next) => (best.Ratio, next.Ratio) switch {
                    ({ IsSome: true, Case: double held }, { IsSome: true, Case: double rival }) => rival > held ? next : best,
                    ({ IsSome: true }, _) => next,
                    _ => best,
                });
        return (won.Ratio, won.Defer) switch {
            ({ IsSome: true, Case: double ratio }, { IsSome: true, Case: MemberCheckRequirement owed }) =>
                (Utilisation)new Utilisation.RequiresMemberCheck(ratio, won.Action, owed),
            ({ IsSome: true, Case: double ratio }, _) => new Utilisation.Bounded(ratio, won.Action),
            _ => new Utilisation.Unbounded(won.Action),
        };
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
                        rc.LinkYieldMpa.Map(fyk => fyk / DesignBasis.En1992.GammaS),  // f_ywd = f_yk/γs off the basis row
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
    // discriminant (never a per-family or per-code factory roster and never an overload set). Each case carries an
    // already-computed family-owner receipt WHOLE into the rail as kN·m/kN with no re-derivation: the steel
    // DesignCapacity (N·mm/N major + minor flexure + CompactnessClass + slenderness + χ/χLT + the torsional column,
    // positive for a CLOSED HSS and 0 for an OPEN warping-torsion shape) under the BASIS it was computed on, so the
    // AISC and EC3 receipts of one section land one case; the timber TimberCapacity (major + minor design resistances
    // + λ_rel + the §6.1.6(2) Km + k_mod + the §6.1.8 TorsionalNmm); the masonry case f'm read off the TYPED
    // CmuStrength row and its tension limit minted on the typed masonry#MASONRY_FAMILY table the basis selects —
    // never bare caller doubles. Direction is a lift-time key: a vertical
    // member's bed-plane section stresses normal-to-bed on BOTH moment axes (a normal row); a horizontally-spanning
    // strip lifts a parallel row over its vertical-cut section; a stack-bond pier its stack row; the
    // partially-grouted normal-direction wall composes RuptureModulus.PartialGrout(CmuPhysics.GroutedCellFraction,
    // system, mortar) with direct case construction — the TMS footnote's one sanctioned bypass. The reinforced-masonry
    // case computes As off the lattice facts and takes the mid-wall d = W/2 / bed-length b levers; the glass case reads
    // the GlazingStructural receipt whole; the connection cases collapse the weld line (its AISC J2-5 directional
    // factor applied at lift), the adhesive lap, the stud group, the connector's duration-governed columns, and the
    // cast-in anchor's cone/steel minimum onto the ONE Connection triple; the aluminium case computes its elastic-
    // floor resistances at lift over the proven band; the fatigue and base-plate cases seat their law and their
    // precomputed pair. Every capacity column reads DIRECTLY off its receipt or typed row — ONE source, no
    // redundant parallel lift parameter.
    public static SectionCapacity Lift(CapacityReceipt receipt) => receipt.Switch(
        steel: static r => (SectionCapacity)new SteelMember(
            r.Capacity.Basis,
            r.Capacity.FlexuralNmm * 1e-6, r.Capacity.FlexuralMinorNmm * 1e-6, r.Capacity.CompressionN * 1e-3,
            r.Capacity.ShearN * 1e-3, r.Capacity.TorsionalNmm * 1e-6, r.Capacity.Classification, r.Capacity.Slenderness,
            r.Capacity.Chi, r.Capacity.ChiLt, StiffnessRetention: 1.0),
        timber: static r => new TimberMember(
            r.Capacity.BendingNmm * 1e-6, r.Capacity.BendingMinorNmm * 1e-6, r.Capacity.CompressionN * 1e-3,
            r.Capacity.ShearN * 1e-3, r.Capacity.BearingPerpNPerMm * 1e-3, r.Capacity.TorsionalNmm * 1e-6,
            r.Capacity.RelativeSlenderness, r.Capacity.Km, r.Capacity.Kmod),
        // The deck's AISI receipt lands the SAME SteelMember case — one cold-formed verdict shape for a stud and a
        // sheet; only the receipt KIND distinguishes them for the report and the analytics dimension.
        deckSheet: static r => new SteelMember(
            r.Capacity.Basis,
            r.Capacity.FlexuralNmm * 1e-6, r.Capacity.FlexuralMinorNmm * 1e-6, r.Capacity.CompressionN * 1e-3,
            r.Capacity.ShearN * 1e-3, r.Capacity.TorsionalNmm * 1e-6, r.Capacity.Classification, r.Capacity.Slenderness,
            r.Capacity.Chi, r.Capacity.ChiLt, StiffnessRetention: 1.0),
        // The slenderness reduction MINTS here off the receipt's basis, the carried height, and the section's own
        // governing radius of gyration — the TMS bracket and the EN 1996 Φ are both the MasonryReduction owner's
        // derivations, so no caller re-derives either and a transposed branch is unrepresentable. The tension limit
        // and the shear bond read the basis's OWN table: the TMS Table 9.1.9.2 row keyed by mortar, or the EN Table
        // 3.4/3.5 row keyed by unit group at the mortar's declared EN class, both under the ONE span direction the
        // rupture row states.
        masonry: static r => new MasonryUnreinforced(
            r.Basis, r.Strength.FmMpa, r.Section.AreaMm2.Value, r.Section.SxMm3.Value, r.Section.SyMm3.Value,
            MasonryReduction.Of(r.Basis, r.HeightMm, r.Section.GoverningRadiusMm).Value,
            r.Basis == DesignBasis.En1996
                ? r.Flexural.FxkMpa(r.Mortar, r.Rupture.SpanParallelToBed)
                : r.Rupture.FrMpa(r.System, r.Mortar),
            r.Basis == DesignBasis.En1996 ? r.Flexural.Fvk0Mpa(r.Mortar) : TmsRunningBondShearMpa),
        reinforcedMasonry: static r => new MasonryReinforced(
            r.Basis, r.Strength.FmMpa, r.Bar.MinimumYieldMpa,
            r.Unit.ReinforcedCells * Math.PI / 4.0 * r.Unit.RebarBarMm * r.Unit.RebarBarMm,   // As off the lattice facts
            r.Section.AreaMm2.Value, r.Unit.WMm / 2.0, r.Unit.LMm,                            // d = W/2 mid-wall bars, b the bed length
            MasonryReduction.Of(r.Basis, r.HeightMm, r.Section.GoverningRadiusMm).Value),
        glass: static r => new GlassPane(r.Capacity.StripBendingKnmPerM, r.Capacity.ResistanceMpa, r.Capacity.EffectiveThicknessMm, r.Capacity.LoadShareFraction),
        // The EN 1993-1-2 accidental situation: ky,θ scales every STRENGTH column (flexure both axes, compression,
        // shear, torsion), kE,θ rides the StiffnessRetention column for the forward member-stability check, and the
        // ambient classification/slenderness/χ carry unchanged — the section's geometry does not char, and the fire
        // state keeps the AMBIENT basis because EN 1993-1-2 modifies the resistance rather than the jurisdiction.
        steelFire: static r => new SteelMember(
            r.Ambient.Basis,
            r.Ambient.FlexuralNmm * r.Ky * 1e-6, r.Ambient.FlexuralMinorNmm * r.Ky * 1e-6, r.Ambient.CompressionN * r.Ky * 1e-3,
            r.Ambient.ShearN * r.Ky * 1e-3, r.Ambient.TorsionalNmm * r.Ky * 1e-6, r.Ambient.Classification, r.Ambient.Slenderness,
            r.Ambient.Chi, r.Ambient.ChiLt, StiffnessRetention: r.Ke),
        // The EN 1995-1-2 residual section is already priced at kmod = γM = 1.0 by the timber owner, so the fire arm
        // lifts it verbatim — the charring is geometry, never a factor applied here.
        timberFire: static r => new TimberMember(
            r.Residual.BendingNmm * 1e-6, r.Residual.BendingMinorNmm * 1e-6, r.Residual.CompressionN * 1e-3,
            r.Residual.ShearN * 1e-3, r.Residual.BearingPerpNPerMm * 1e-3, r.Residual.TorsionalNmm * 1e-6,
            r.Residual.RelativeSlenderness, r.Residual.Km, r.Residual.Kmod),
        // AWS D1.1 publishes one shear band for a weld line and no separate tension allowable, so the tension column
        // is ABSENT rather than 0 — a tension demand on a weld reads Unbounded, never a silent pass against zero.
        // DirectionalShearKn is the joint row's OWN Option (a weld row whose electrode publishes no shear area
        // answers None), collapsed onto the unprovided-0 shear column so a shear demand on it governs loud — the
        // same posture the EN-railed bolt members take.
        weld: static r => new Connection(DesignBasis.AwsD11, r.Row.DirectionalShearKn(Angle.FromDegrees(r.LoadAngleDeg)).IfNone(0.0), None, 0.0),
        // The ASTM C1401 structural-bite tension is the adhesive row's OWN Option: a silicone SSG row publishes it, an
        // epoxy/MMA/PU row does not, and that ABSENCE is distinct from zero resistance — the row already carries the
        // distinction, so the lift threads it rather than flattening it to a fabricated 0.
        adhesive: static r => new Connection(DesignBasis.AstmD1002, r.Row.DesignShearKn, r.Row.DesignTensionKn, 0.0),
        stud: static r => new Connection(DesignBasis.Aisc360, Math.Max(r.Count, 0) * r.Row.DesignShearKn(r.Group), None, 0.0),
        // Duration scaling is applied at `ConnectorRow.GovernedCapacity`, where each published cell meets its OWN
        // basis — the four report formats scale differently — so this lift re-scales nothing and carries the admitted
        // columns straight across. An absent direction stays ABSENT: a report that publishes no uplift is a distinct
        // verdict from one publishing zero, and the tension column is already `Option` to say so.
        connector: static r => new Connection(DesignBasis.IccEs,
            r.Capacity.LateralF1Kn.IfNone(0.0), r.Capacity.UpliftKn, r.Capacity.DownloadKn.IfNone(0.0),
            r.Capacity.LateralF2Kn, r.Capacity.Combines),
        // EN 1993-1-8 §3.6 bearing-type bolt: the assembly's OWN plane-counted shear, head-factored tension, and ply
        // bearing projections — every value the sibling family's Fastening algebra already owns, read through the
        // assembly rather than re-derived. The shear/tension members are EN-RAILED (a grade the EN tables do not
        // tabulate — an ASTM band — publishes no αv/k2), so the lift collapses that refusal onto the unprovided
        // column: shear 0, tension None — and the demand that meets it governs loud, never a borrowed factor.
        bolt: static r => {
            Op key = Op.Of(name: r.Subject.Value);
            return new Connection(DesignBasis.En1993Joints,
                r.Assembly.ShearResistanceKn(r.Plane, key).IfFail(0.0),
                r.Assembly.TensionResistanceKn(key).ToOption(),
                r.Assembly.BearingResistanceKn(r.Bearing));
        },
        // EN 1993-1-8 §3.9 slip resistance: a non-preloaded assembly answers None, lifted as a 0 shear column so a
        // slip-critical demand on a snug-tight joint governs loud rather than reading the bearing value by accident.
        slipCritical: static r => new Connection(DesignBasis.En1993Joints, r.Assembly.SlipResistanceKn(r.Install).IfNone(0.0), None, 0.0),
        // EC5 §8: the family owner's railed six-mode Johansen minimum per shear plane, summed over the planes.
        timberDowel: static r => new Connection(DesignBasis.En1995, Math.Max(r.Planes, 0) * r.PerPlaneShearKn, None, 0.0),
        // The panel family already applied the §4.1.4 reduction on its own rail, so the lift is a straight seat.
        lateralPanel: static r => new LateralPanel(DesignBasis.Sdpws, r.DesignKnPerM, r.Hazard),
        // EN 1999: the banded (fo, fu) pair arrives PROVEN (the aluminum seed refused any die outside its printed
        // window), the resistances compute on the class-3 elastic floor under the row's γM1-covers-everything set,
        // and the §6.3.1.2 Table 6.6 curve constants seat per class letter — A precipitation-hardened (0.20, 0.10),
        // B work-hardened (0.32, 0.00) — for the forward stability check to reduce by.
        aluminum: static r => new AluminumMember(
            r.Basis,
            r.Section.SxMm3.Value * r.FoMpa / r.Basis.GammaM1 * 1e-6,
            r.Section.SyMm3.Value * r.FoMpa / r.Basis.GammaM1 * 1e-6,
            r.Section.AreaMm2.Value * r.FoMpa / r.Basis.GammaM1 * 1e-3,
            r.Section.AvyMm2.Value * r.FoMpa / (Math.Sqrt(3.0) * r.Basis.GammaM1) * 1e-3,
            r.Grade.Class,
            r.Grade.Class == BucklingClass.A ? 0.20 : 0.32,
            r.Grade.Class == BucklingClass.A ? 0.10 : 0.00,
            r.FoMpa, r.FuMpa),
        // The S-N law seats whole — the ladder rung, and for the EN case its γMf assessment row, are the receipt's
        // own declaration and the capacity IS the law.
        fatigue: static r => new Fatigue(r.Law),
        // EN 1992-4 cast-in anchor onto the ONE Connection triple, EN-PURE: tension = min(steel where the grade's EN
        // band prices it, edge-factored cone/γc) — the k1 pair 8.9 cracked / 12.7 uncracked (N, mm, MPa basis) with
        // ψs,N = min(0.7 + 0.3·c/(1.5·hef), 1) at the declared edge and 1.0 away from every edge; shear = the
        // assembly's own plane-counted EN steel shear. The modes the section cannot finish — group areas, the shear
        // edge mode, the ETA-owned EN pullout, EN pryout (k8 unproven), a non-EN rod band — ride the ONE
        // forward-modes deferral; the post-installed k1 cells (7.7/11.0, proven) wait on their ETA installation
        // factor, and the ACI mode set (kc = 24, 8·Abrg·f'c, kcp) waits whole on its φ roster rather than minning
        // an ACI nominal into an EN verdict.
        anchor: static r => Anchoring(r),
        // DG1: both axial capacities precompute from the bed alone, so the case is two columns and the check is the
        // standard fold.
        basePlate: static r => Baseplating(r.Plate));

    // The cast-in anchoring kernel — the one site the EN 1992-4 single-anchor coefficients live. The steel tension
    // rides the assembly's EN-railed projection: an EN-tabulated grade mins its k2·fub·As/γM2 in beside the cone,
    // an untabulated (ASTM) band prices no EN steel mode and the cone stands alone WITH the forward-modes deferral
    // carrying the steel completion — never a zero that voids the tension column the cone honestly bounds.
    // EXPRESSION_SPINE measured-kernel exemption: the code constants and mode scalars bind once, one Connection exits.
    static SectionCapacity Anchoring(CapacityReceipt.Anchor r) {
        Op key = Op.Of(name: r.Subject.Value);
        double k1 = r.Bed.Cracked ? 8.9 : 12.7;                                     // EN 1992-4 cast-in headed kcr,N/kucr,N
        double edge = r.Bed.EdgeMm.Map(ca => Math.Min(0.7 + 0.3 * ca / (1.5 * r.Bed.HefMm.Value), 1.0)).IfNone(1.0);
        double coneKn = k1 * Math.Sqrt(r.Bed.FckMpa) * Math.Pow(r.Bed.HefMm.Value, 1.5) * edge / DesignBasis.En1992Anchors.GammaM0 * 1e-3;
        return new Connection(
            DesignBasis.En1992Anchors,
            r.Assembly.ShearResistanceKn(r.Plane, key).IfFail(0.0),
            Some(r.Assembly.TensionResistanceKn(key).ToOption().Map(steel => Math.Min(steel, coneKn)).IfNone(coneKn)),
            0.0,
            Defer: Some(MemberCheckRequirement.AnchorForwardModes));
    }

    // The DG1 kernel: §J8 bearing at φ = 0.65 with the √(A2/A1) confinement clamped at 2, and the cantilever
    // plate-bending capacity Pu* = 0.9·Fy·B·N·t²/(2·l²) — the t_min equation inverted at the plate's own thickness,
    // l = max(m, n, n′) under the two-sourced λ = 1 conservative bound (m = (N − 0.95·d)/2, n = (B − 0.8·bf)/2,
    // n′ = √(d·bf)/4, wide-flange columns — the HSS/pipe m–n variants are single-sourced and typed-absent).
    // EXPRESSION_SPINE measured-kernel exemption: the geometry scalars bind once, one BasePlate exits.
    static SectionCapacity Baseplating(PlateBed plate) {
        double b = plate.WidthMm.Value, n = plate.LengthMm.Value, t = plate.ThicknessMm.Value;
        double bearingKn = 0.65 * 0.85 * plate.FcMpa * b * n * Math.Clamp(plate.ConfinementRatio, 1.0, 2.0) * 1e-3;
        double mArm = (n - 0.95 * plate.ColumnDepthMm.Value) / 2.0;
        double nArm = (b - 0.8 * plate.ColumnFlangeMm.Value) / 2.0;
        double nPrime = Math.Sqrt(plate.ColumnDepthMm.Value * plate.ColumnFlangeMm.Value) / 4.0;
        double l = Math.Max(Math.Max(mArm, nArm), nPrime);
        double bendingKn = 0.9 * plate.FyMpa * b * n * t * t / (2.0 * Math.Max(l, double.Epsilon) * Math.Max(l, double.Epsilon)) * 1e-3;
        return new BasePlate(bearingKn, bendingKn);
    }

    // The TMS 402 §9.2.6.1 running-bond shear constant (56 psi) lifted out of the kernel onto the ShearBondMpa column
    // so the unreinforced shear arm reads one DATA source on either basis — the EN side fills it from Table 3.5.
    const double TmsRunningBondShearMpa = 0.386;

    // TMS 402 §9.2: the 0.80 multiplier on the slenderness-reduced axial resistance is the code's own accidental
    // -eccentricity coefficient, NOT the stress-block cap that shares its value — two distinct clauses reading the
    // same number, so folding them into one column would make an edit to either silently move the other.
    const double SlendernessCoefficient = 0.80;

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
        // A hull enclosing the demand direction answers the smallest positive pierce and the utilisation is its
        // reciprocal; a hull the ray never pierces bounds nothing along that direction, and THAT is the absent
        // candidate — the eccentric-hull verdict is reached by the pierce set being empty, never by a magnitude.
        Option<double> ray = Math.Abs(demand.AxialKn) <= double.Epsilon && demand.MomentResultantKnm <= double.Epsilon
            ? Some(0.0)
            : toSeq(hull.Faces)
                .Map(face => Pierce(face, demand.AxialKn, demand.MomentYKnm, demand.MomentZKnm))
                .Somes()
                .Filter(static multiplier => multiplier > 0.0)
                .Fold(Option<double>.None, static (best, multiplier) => Some(best.Map(won => Math.Min(won, multiplier)).IfNone(multiplier)))
                .Map(static multiplier => 1.0 / multiplier);
        return Worst(
            (ray, governing, None),
            (GuardedRatio(demand.ShearResultantKn, 0.0), GoverningAction.Shear, None),
            (GuardedRatio(demand.TorsionKnm, 0.0), GoverningAction.Torsion, None),
            (GuardedRatio(demand.BearingKn, 0.0), GoverningAction.Bearing, None));
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

    // The GENERATIVE counterpart: `SectionProfile.BuiltUp` plus the `component#SECTION_SOLVER` `Compose` fold already
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
