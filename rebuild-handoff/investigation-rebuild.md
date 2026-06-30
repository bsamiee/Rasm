# [WF1_INVESTIGATION_REBUILD]

Record of the 21 REBUILD-phase fixlogs the `element-component` pass wrote in place (Materials `Profiles/`+`Connection/` -> `Component/` collapse and the Element seam alignment). One section per fixlog; `residuals.json` holds the deduped cross-file work the terminal RESOLVE phase must drive to zero. Verdict spread: 18 `hardened`, 2 `refined`, 1 `clean`. Critique/redteam have not yet run.

| # | Page | Verdict | #residuals |
| :-: | --- | --- | :-: |
| 1 | `Rasm.Materials/.planning/Component/capacity.md` | hardened | 2 |
| 2 | `Rasm.Materials/.planning/Component/timber.md` | hardened | 4 |
| 3 | `Rasm.Materials/.planning/Component/cmu.md` | hardened | 4 |
| 4 | `Rasm.Materials/.planning/Component/reinforcement.md` | hardened | 2 |
| 5 | `Rasm.Materials/.planning/Component/component.md` (unified owner) | hardened | 9 |
| 6 | `Rasm.Materials/.planning/Component/fastener.md` | hardened | 3 |
| 7 | `Rasm.Materials/.planning/Component/steel.md` | hardened | 1 |
| 8 | `Rasm.Materials/.planning/Component/glazing.md` | hardened | 5 |
| 9 | `Rasm.Element/.planning/Relations/relation.md` | refined | 2 |
| 10 | `Rasm.Materials/.planning/Component/connector.md` | hardened | 3 |
| 11 | `Rasm.Materials/.planning/Component/masonry.md` | hardened | 3 |
| 12 | `Rasm.Element/.planning/Composition/material.md` | refined | 3 |
| 13 | `Rasm.Element/.planning/Properties/property.md` | hardened | 2 |
| 14 | `Rasm.Element/.planning/Projection/projection.md` | hardened | 3 |
| 15 | `Rasm.Materials/.planning/Projection/component.md` (ComponentProjector) | hardened | 5 |
| 16 | `Rasm.Element/ARCHITECTURE.md` (+ README seams endpoint) | hardened | 3 |
| 17 | `Rasm.Materials/.planning/Component/joint.md` | hardened | 2 |
| 18 | `Rasm.Element/.planning/Graph/element.md` | hardened | 1 |
| 19 | `Rasm.Materials/ARCHITECTURE.md` + README router | clean | 2 |
| 20 | `Rasm.Materials/.planning/Construction/{assembly,layout,nesting}.md` | hardened | 2 |
| 21 | `Rasm.Materials` index docs (README/ARCHITECTURE/IDEAS/TASKLOG) | hardened | 3 |

Total residuals (pre-dedup) 64; deduped union 64 (no exact duplicates across pages).

---

## 1 — `Component/capacity.md` — hardened (2 residuals)

Integrated: full `Profiles+Connection->Component` rename-map collapse page-wide (`ProfileFault->ComponentFault` band 2300 with `Capacity` case preserved, namespace `->Rasm.Materials.Component.Capacity`, `profile#PROFILE_OWNER->component#COMPONENT_OWNER`, `ProfileFamily->ComponentFamily`); the `Rasm.Domain->Rasm.Vectors` import phantom for `PositiveMagnitude` (Op stays `Rasm.Domain`); forward-consumer notation fix `cs:AEC_SIMULATION_BRIDGE` (an IDEAS slug) -> `Rasm.Compute/structural#DESIGN_CHECK`; VividOrange re-verified against folder `.api` (`InteractionDiagram` eager-solve ctor + `.Mesh`, `DiagramSettings` 6-arg, `IForceMomentMesh/TriFace/Vertex`, `ConcreteSectionProperties.EffectiveDepth(SectionFace)`, `EnConcreteFactory.CreateLinearElastic<EnConcreteGrade>`); sibling capacity receipts (`steel#STEEL_FAMILY`/`timber`/`cmu`/`reinforcement`) lifted whole.

Extended: largely-realized page hardened with zero logic regression — `SectionCapacity` `[Union]` held closed at 5 (`RcInteraction`/`RcElastic`/`SteelLrfd`/`TimberEc5`/`MasonryCompression`) over the one `Demand/Utilisation/GoverningAction` rail, `SectionCapacityResolver` eager-solve boundary, `CapacityRay` Moller-Trumbore hull cast all preserved. Cold-grade hardening eliminated 7 `would`, 3 provenance markers, and the retired-band caveat; restated as the forward `ComponentFault-2300` disjoint-from-`GeometryFault-2400` invariant.

Summary: the `Profiles/capacity.md -> Component/capacity.md` move with the full rename collapse and 3 real defect fixes; two adversarial suspicions dismissed after verification. The 2 residuals (Materials `ARCHITECTURE.md` seams+codemap, `README.md` router) are deferred to the terminal Resolve to avoid parallel-write conflict on the shared index docs.

## 2 — `Component/timber.md` — hardened (4 residuals)

Integrated: `VividOrange.Materials LinearElasticOrthotropicMaterial` 7-arg ctor + non-obsolete `MaterialType.Timber` (`OrthotropicLaw [BoundaryAdapter]`, assay-verified); the seam `MaterialPropertySet.OfOrthotropic` 8-arg smart-constructor composed unchanged; `component#COMPONENT_OWNER ParametricSection.Rectangle` + the 17-field `ComputedSection`; `capacity#SECTION_CAPACITY SectionCapacityResolver.TimberEc5` lift incl. §6.1.8 torsion column; per-form EN 1995-1-1 Table 2.3 `gamma_M` onto `TimberForm.GammaM`; `Rasm.Vectors PositiveMagnitude` columns; merged `ComponentFault` band 2300 (`Family`/`Grade`).

Extended: 7 validated EN data fixes landed (`TimberForm.GammaM` per-form {sawn 1.30, glulam 1.25, clt/lvl 1.20, psl 1.25} replacing flat 1.25; `Gl24c/Gl28c E90Mean` 250->300; `C24 ft0k` 14.0->14.5; `C30 ft0k` 18->19 / `fc0k` 23->24; `D40 rhoK` 700->550 / `E0Mean` 11000->13000). Reframed `TimberSection` as the `ComponentSection.Timber` payload (cross-section a Component FIELD). Namespace law: relies on enclosing-scope resolution for parent owner types. One in-file observation (`D40 E005` left conservative at 9400) noted, not logged as residual.

Summary: rebuilt in place from `Profiles/timber.md` (467 lines, hedge-clean), full rename + the orthotropic seam call matching `material.md:596` exactly.

## 3 — `Component/cmu.md` — hardened (4 residuals)

Integrated: rebuilt in place transcription-complete; old `profiles/cmu.md` deleted (file move). `Thinktecture` `[SmartEnum<string>]`+`KeyMemberEqualityComparer` vocabularies, `ParametricSection.Hollow`, the seam 17-field `ComputedSection`, capacity ripple `SectionCapacityResolver.MasonryCompression`; both `.api` tiers cited.

Extended: heavy generative growth. `CmuSection.WebThicknessMm` split into `EndWebMm`+`CrossWebMm` (ASTM C90-14 Table 1); `GroutFraction` replaced by a per-cell `Seq<CmuCell>` lattice (per-cell geometry, `NetSection`/`GroutedSection`/`GroutedCellFraction` derived); new `CmuSpecialUnit`/`CmuFinish` SmartEnums, demold draft, 3D head joint, NCMA TEK 6-2C isothermal-planes `ThermalResistance`. 5 adversarial defects fixed: inverted/mis-grouped `CmuStrength` mortar columns re-based to TMS 602-16/-22 Table 2; linear `FireRatingHours` -> ACI 216.1 power-law clamp; `GroutDensity` 2240->2243; multi-cell hollow IFC egress -> `IfcArbitraryProfileDefWithVoids`; exact per-cell void subtraction.

Summary: `Profiles/cmu.md -> Component/cmu.md` (kind: component, `ComponentClass.Minor`, cluster `#CMU_FAMILY`), validated CMU generative-data mandate captured, cross-section now a Component FIELD.

## 4 — `Component/reinforcement.md` — hardened (2 residuals)

Integrated: `VividOrange.Sections` (`ConcreteSection`/`Rebar`/`Link`/`FaceReinforcementLayer`/`ReinforcementLayoutByCount|BySpacing`/`MinimumReinforcementSpacing` over `BarDiameter D6..D50`); `VividOrange.Materials EnRebarFactory.CreateLinearElastic|CreateBiLinear` on the EN-bodied bar (ductility k=1.05/1.08/1.15 for class A/B/C); `VividOrange.Standards En1992/NationalAnnex`; `Rasm.Vectors PositiveMagnitude`; hand-rolled in-fence ISO 6935-2 rib geometry, ASTM A615 §7.4, ACI 318-19 §25.3 bend factors, EN 1992 §8.3 mandrel, BS 8666:2020 37-code set.

Extended: added the validated generative rebar-deformation capture absent from the naive page (`RebarRibGeometry` over a new `RibPattern` axis); split the conflated `BendFactor` into `HookKind{Development/StirrupTie/Seismic}`; surfaced `MandrelDiameterMm`; widened `ShapeCode` to the full 37-code SmartEnum; expanded `BarSize` to EN-10080 `H6..H50`; added EN B500A ductility branch.

Summary: rebuilt at the new `Component/` path (506 lines, member-verified); full `Connection*->Component*` rename, band 2360 retired to 2300, `ComponentClass.Minor`. Standards data sourced authoritatively.

## 5 — `Component/component.md` (unified owner) — hardened (9 residuals)

Integrated: merged `Profiles/profile.md#PROFILE_OWNER` + `Connection/connection.md#CONNECTION_OWNER` into one `Component` owner (the two `.Of` admissions + the two catalogues collapsed to `ComponentCatalogue.Build` over 9 `BuildXRows`); merged M7 one-hop into `#COMPONENT_RESOLUTION` (`ResolvedComponent`, `ProfileRef` key seam-canonical); relocated `Coring/CoringClass` from masonry; composed the 9 verified family section types as `ComponentSection` arms; verified-local atoms (`PositiveMagnitude/Dimension/UnitInterval` in `Rasm.Vectors`); `VividOrange.Sections.SectionProperties` bridge verbatim; verified IFC tokens; two-tier `.api` cited.

Extended: beyond a mechanical merge — `ComponentClass {Primary,Minor}` is the NEW discriminant with an `IfcSupertype` projection (the `[1].3` `IfcBuiltElement`/`IfcElementComponent` split); `ComponentSection.IfcEntity/PredefinedToken` projects an honest asymmetric egress; `CrossNominalMm` the disambiguated union projection. 3 latent source defects fixed in place (FrozenDictionary comparer type-mismatch, missing child-namespace usings, `Connector` `CS0542` collision). `ParametricSection.ProfileOf` decoupled; `CapacityKey` unified across all 9 families.

Summary: authored fresh (599 LOC, 2 fences) as the unified Component owner — `[02]-[COMPONENT_OWNER]` + `[03]-[COMPONENT_RESOLUTION]`, full rename applied, `AppearanceId:MaterialId` frozen. The 9 residuals are the contract its family pages depend on landing exactly.

## 6 — `Component/fastener.md` — hardened (3 residuals)

Integrated: full rename map (`ConnectionItem->Component`, `ConnectionSection.Fastener->ComponentSection.Fastener`, fault 2360->2300, namespace `->Rasm.Materials.Component.Fastener`), `ComponentClass.Minor`. Verification load-bearing: assay-decompiled `IfcMechanicalFastenerTypeEnum` (full member set, no `NUT`) and `IfcFastenerTypeEnum`; `VividOrange.Materials EnSteelGrade` confirmed structural-only (so `FastenerGrade` bands hand-rolled); ISO 4014/4032/7089 + `b=2d+6/2d+12/2d+25` confirmed against external tables.

Extended: captured the complete validated generative data — ISO 68-1/261/724 thread form (`FlankAngleDeg=60`, derived `d2/D1/H`, 6g/6H tolerance), per-size `HexHardware` envelope (ISO 4014 head, ISO 4032 nut, ISO 7089 washer), shank-vs-thread split. 3 real defects fixed: `FastenerKind.Coupler->COUPLER` (the source's "not in enum" claim was FALSE); inch UNC tensile-stress-area -> ASME B1.1 Unified formula series-dispatched; `ShankLengthMm` misnomer -> `LengthMm`. `HexHardware` rides `ThreadSize.Hardware` as `Option` (None for inch UNC, deferred per the `reinforcement.md` pattern).

Summary: rebuilt at `Component/fastener.md` (387 lines) from `Connection/fastener.md`; `Nut->USERDEFINED` kept (assay-confirmed).

## 7 — `Component/steel.md` — hardened (1 residual)

Integrated: composes `component#COMPONENT_OWNER` via the VERIFIED on-disk direct `Component(...)` ctor (over `.Map`, matching the timber sibling), `ComponentSection.Steel(SteelShape)`, `ComponentFamily.Steel=Primary/IfcBuiltElement`, 17-field `ComputedSection`; `SteelSections` static-property map joined by `ComponentResolution.Build`; `Component/capacity SteelDesign.DesignCapacity` lifted whole by `SteelLrfd`; `Component/joint StudClass.SteelShearKn` composite consumer; `VividOrange.Profiles.Catalogue/.Sections/.Materials(EnSteelFactory)/.Standards(En1993)` verified.

Extended: 4 assay-verified defects fixed — (1) 88 round HSS misclassified as rectangular (both carry `AmericanShape.HSS`); `ClassOf` now splits by `ICircularHollow/IRectangularHollow` geometry; (2) `S420/S460` EN-yield always railed under default AR; `SteelGrade` now carries `EnSteelDeliveryCondition` and sets `Specification.DeliveryCondition=N`; (3) phantom `European.HEM300` -> real `HE300M`; (4) stale `SteelSectionOf` delegate dropped for data-join. Seed densified across every published family; stale `SteelSectionOf` removed.

Summary: rebuilt ground-up under the collapse, superseded `Profiles/steel.md` removed, aligned to the on-disk `component.md` owner contract. The 1 residual is the not-yet-landed `joint.md StudClass` dependency.

## 8 — `Component/glazing.md` — hardened (5 residuals)

Integrated: EN 410/ISO 9050 multi-layer net-g recursion (per-pane directional two-flux Combine + inter-reflection + secondary heat flux) sharing the ONE EN 673 series-resistance chain `Ug` reads; `CavityFill [Union]` {`GasFill`, `VacuumFill`} (Collins pillar conduction + free-molecular + radiative); `Interlayer [SmartEnum]` PVB/SGP/EVA; EN 1279-2 `EdgeSeal` + enriched `SpacerType` + optional `MuntinGrid`; per-substance `GlassType` axis; `Coating Option<double>` emissivity; `GlazingGwp` substance/process split; captured face dims replacing hardcoded 1200; parameterized `FireResistance.Ei`; full rename + `Rasm.Vectors` import fix with a using-alias disambiguating `Rasm.Element.Dimension`.

Extended: 493->705 lines. In-place capability growth on the one `GlazingSection` owner: the previously-deferred EN 410 net-g recursion realized and unified with the thermal chain; VIG vacuum + gas-mixture branches as a `CavityFill [Union]`; GWP/fire/specific-heat/density became per-substance/per-process rows. Every flagged hardcode fixed (specific-heat 840->720, borosilicate λ 1.40->1.14, GWP double-count split, Coating NaN->Option, EI parameterized). `Wacton.Unicolour` deliberately NOT composed (would be cross-seam noise). No new package admitted.

Summary: `Profiles/glazing.md -> Component/glazing.md`, validated complete generative IGU data captured, one real shadowing defect (`Coating.None` vs `Prelude.None`) caught and fixed.

## 9 — `Relations/relation.md` (Element) — refined (2 residuals)

Integrated: affirmed `AssignKind.TypeDefinition` as the occurrence->Type-`Object` bind the Component projection authors and `Bake` resolves into named type->occurrence inheritance — never a parallel `DefinesByType` case (`IfcRelDefinesByType` rides this neutral row); added the named-inheritance prose to `[02]` Auto (distinct from `property#PROPERTY_BAG InheritanceMode` value-bag precedence); corrected the stale PropertyBag-only `TypeDefinition` framing; removed 3 page-wide banned hedges; confirmed the rename touches ZERO anchors this page owns and the neutral 5-kind edge algebra stays closed.

Extended: no capability extension — REUSE-ONLY, fence byte-stable (7 ins / 7 del, prose+comment only). The only semantic deepening is the `Assign.TypeDefinition` edge contract now explicitly carrying the named type->occurrence inheritance, with `InheritanceMode` demoted to the inner value-bag precedence.

Summary: surgical seam edge-algebra refinement realizing the `[1].3/[1].4` mandate. The 2 residuals: `element.md` must realize the named-inheritance `Bake` fold + `MaterialBinding->BakedMaterial` + `TypeBinding`/`TypeId` (HIGH); `property.md` should rename its `DefinesByType` shorthand to `Assign.TypeDefinition` (low).

## 10 — `Component/connector.md` — hardened (3 residuals)

Integrated: `component#COMPONENT_OWNER` (`Component.Of`/`ComponentFamily.Connector`/`ComponentSection.Connector`/`ComponentFault.{Designation,Grade,Dimension,Capacity}`); `Thinktecture` `[SmartEnum<string>]`/`[UseDelegateFromConstructor]`/`[ComplexValueObject]`/`[Union]`/`[KeyMemberEqualityComparer]`; `LanguageExt.Core` Fin/Seq/Choose/Fold rails; `Rasm.Vectors` atoms + `Rasm.Element MaterialId` two-slot independence; GeometryGym `IfcDiscreteAccessoryTypeEnum`/`IfcMechanicalFastenerTypeEnum` assay-verified tokens; `Construction/layout`/`Appearance/graph`/`properties` composed not re-derived.

Extended: `ConnectorPlate` (the page's biggest weakness) rebuilt from a flat single-record smear into a `[Union]` over 4 cold-formed body forms (Saddle/Angle/Strap/AnchorPlate) dispatched by a `ConnectorType.BuildPlate [UseDelegateFromConstructor]` column, every field derived from section columns + cold-forming constants; `FastenerSpec` gained `ShankDiameterMm`. Seed densified 13->23 rows with ICC-ES allowables; the 3 capacity bounds + `GovernedCapacity` rail preserved.

Summary: `hanger->connector` rebuilt in place (380 lines), `ComponentClass.Minor`, VividOrange/hand-roll split stated (no framing-connector range in VividOrange).

## 11 — `Component/masonry.md` — hardened (3 residuals)

Integrated: `component#COMPONENT_OWNER` line-for-line (`Component.Of`, `ComponentSection.Masonry(MasonryUnit)`, relocated `Coring` cases, `ComponentUnit`/`ComponentStandard`/`ComponentAuthority`, band-2300 faults); `Thinktecture v10.4.0` `[SmartEnum]`/`[UseDelegateFromConstructor]`; `LanguageExt.Core`; `Rasm.Vectors`+`Rasm.Domain`; `Rasm.Element MaterialId`; EN 771-1 SizeTolerance/SizeRange (Tavily-validated); ASTM C270 mortar types + tooled-joint recess; ASTM C62/C216/C652 void classes; regional unit dims.

Extended: net-new types — `FrogGeometry`, `Perforation`, `SizeTolerance`/`SizeRange`, `UnitPlacement`, enriched `CourseTemplate(Seq<UnitPlacement>, double CourseOffsetFraction)`. `BondGeometry.Course` now derives the full per-unit transform for 6 bonds; `Cut` gained position+orientation; voussoir taper filled; 3D mortar recess. Phantom fixes (beaded re-graded, extruded+weeping collapsed to one Squeezed case, IS 1077 dims). Pre-existing corpus-wide catalogue comparer observation noted for a uniform sweep.

Summary: `Profiles/masonry.md -> Component/masonry.md`, validated generative-data mandate captured, adversarially aligned to the live owner (real `ToCoring` phantom + `MasonryUnit/Component.Of` shape fixed), hand-rolled (`ComputedSection=None`).

## 12 — `Composition/material.md` (Element) — refined (3 residuals)

Integrated: cited `Properties/quantity#DIMENSION` (the open `QuantityType.Create` mint + `Dimension.Create` `[ComplexValueObject]` factory) as the sanction for the `SectionProperties` engineering-name stamping; wove in the `property#PROPERTY_BAG` neutral detail-schema egress reference (typed `MaterialPropertySet` lowers to the neutral bag at Bim egress, no `Pset_Material*` on the seam); affirmed seam-canonical `ProfileRef`/`MaterialComposition.ProfileSet`/`SectionProperties`/`WithSection` M7 unchanged; neutralized 4 cross-ref prose tokens to host-neutral rename-resilient forms.

Extended: surgical, no structural rebuild — a coherence-flag self-honesty clause (the 4 engineering-section `QuantityType` tokens are consumer-minted domain names), the neutral-detail-schema reference, and rename-coherence neutralization. The `would`/`could` counterfactual idiom is pre-existing corpus-standard, out of scope.

Summary: mostly-affirm seam page; closed the `QuantityType.Create/Dimension.Create` coherence flag on-page, added the point-3 neutral-detail-schema egress reference.

## 13 — `Properties/property.md` (Element) — hardened (2 residuals)

Integrated: added `[04]-[DETAIL_SCHEMA]` — the seam-declared neutral `DetailSchema` (record + `Realization` instance: SetName "Realization", `OccurrenceWins`, JointType allowed-set Bolted/Welded/Bonded/Bearing/Cast) + the 12 realizing-detail `PropertyName` statics + `Bag()`/`Joint()` factories — the SINGLE schema over PropertyBag the Materials Component projection authors and Bim reads. All members compose already-verified seam surfaces.

Extended: NEUTRALIZED the realizing-detail bag — the IFC Pset name `Rasm_ConnectionRealization`, `Pset_*`/bSDD roster, egress mapping, GlobalId, and cross-peer invariants all confined to Bim (never an IFC column on the seam). `PropertyName` stayed a bare open key; `InheritanceMode` stayed PropertyBag-merge-only (no 4th row). Dimensional rows ride `MeasureValue.OfSi`; bag mints via `NodeId.Content` so authored/re-imported bags content-key identically. Fixed 6 `would` hedges.

Summary: rebuilt to realize `[1].5` neutral detail schema. The only residuals are the WF-1 `ARCHITECTURE.md` seam-row landings at both endpoints (Element + Materials); WF-2 Component/Bim body re-binds are out of scope.

## 14 — `Projection/projection.md` (Element) — hardened (3 residuals)

Integrated: the owner-mints-its-identity law `[1].2` woven across lead/INDEX/Entry/Growth/Boundary/`ProjectionContext` doc-comment/RESEARCH (the OWNER of a concept mints its `Object` under the one rooted-identity regime; `ComponentProjector` mints the DETERMINISTIC-rooted Type id via the kernel `Graph/element#NODE_MODEL` Type-seed with Representations EXCLUDED; occurrence authors mint Guid-v7; aspect projectors mint nothing and bind via `ctx.Owns` vouch); rename map (`MaterialProjector->ComponentProjector`, `ConnectionProjector` folded in); page-wide banned-hedge purge (9 `would` + 1 `should`).

Extended: adversarial hardening, not a rewrite — the substantive addition is the owner-mints law refining the old projector-level dichotomy into a concept-level trichotomy; determinism makes "seed Type ids into ElementIds before assembly" coherent. The Type-seed mint lives on the kernel `NodeId` owner (referenced by anchor, no phantom). Signature fences byte-for-byte unchanged; net-new Rasm interfaces stay at 2. Final: 151 lines, zero hedges.

Summary: integrated the owner-mints-its-identity law, applied the rename this page owns, purged hedges — every signature fence structurally unchanged.

## 15 — `Projection/component.md` (Materials, ComponentProjector) — hardened (5 residuals)

Integrated: the unified `ComponentProjector` (one `IElementProjection`, `source.Specs.TraverseM(ProjectSpec)->GraphDelta.Merge` fold, `ComponentProjectionSpec [Union]{Substance(MaterialSpec),Type(ComponentSpec)}` collapsing the dual-projector paradigm); owner-mints-Type-identity (`ProjectType` mints the deterministic-rooted Type via `NodeId.Type` over `ToTypeSeedBytes`, `ObjectKind.Type` reused, `Classification`+`PredefinedType` off the `ComponentSection` egress, occurrences bound via `Assign.TypeDefinition`); M7 `SeamSection` lift to the grown TWENTY-field seam `SectionProperties`; neutral `DetailSchema` realization bag; landed seam-mirror rows at both `ARCHITECTURE [2]-[SEAMS]` endpoints + fixed README router + deleted old `material.md`.

Extended: INVERTS the projection identity model — where the prior `MaterialProjector`+`ConnectionProjector` authored no Object node, the unified Type arm MINTS the deterministic-rooted Type Object; the detail bag re-homed from per-occurrence to TYPE-level via `Assign.PropertyDefinition` that occurrences inherit through the Bake type-bag merge. Absorbs the seam's 17->20 field `SectionProperties` growth, stamping the 3 new asymmetric LTB columns (`ShearCentreY/Z`/`MonosymmetryFactor`) zero — exact for doubly-symmetric/parametric families; the genuine widening (PFC/tee/angle offsets from steel `SteelStiffness`) deferred to the Component `ComputedSection` growth residual. Latent `Mint` fix (`Node.Relabel` over the uncompilable `draft with { Id }`).

Summary: `Projection/material.md -> Projection/component.md` as THE ONE unified `ComponentProjector` (451 lines). 5 cross-file residuals: Element seam body (`NodeId.Type`/`ToTypeSeedBytes`/`BakedMaterial`-rename/`TypeBinding`/`Bake`-rework), the neutral DetailSchema seam owner, the Component `ComputedSection` asymmetry-column growth, the WF-2 Bim re-bind, the folder-wide README/ARCHITECTURE `Profiles+Connection->Component` sweep.

## 16 — `Rasm.Element/ARCHITECTURE.md` (+ README) — hardened (3 residuals)

Integrated: landed the four unified-paradigm seam-mirror rows at the Element `[02]-[SEAMS]` endpoint (the page carried ZERO before) — `[PROJECTION]` (ComponentProjector mints the rooted Type Object), `[SHAPE]` (owner-mints + named Bake inheritance + `TypeId` + `BakedMaterial`/`TypeBinding` rename), the `Properties/property ⇄ Bim/Semantics` neutral `DetailSchema` row, and the retargeted `Composition/material -> Component` M7 row; applied the `Profiles/Connection->Component` + `MaterialProjector->ComponentProjector` rename to every owned anchor; hardened the `[01]-[DOMAIN_MAP]` Element.cs/Property.cs charters, post-codemap prose, `[PROJECTION]` paragraph, `[CONTENT_KEY_IDIOM]`.

Extended: adversarial dual-axis pass beyond the four row edits — a cold read flagged the codemap charters/prose/`[PROJECTION]`/`[CONTENT_KEY_IDIOM]` still describing the old paradigm (internal incoherence); each region hardened to match the rows, truthful to the concurrently-rebuilt `element.md`/`relation.md`/`property.md` bodies. Both new seam KINDs in the allowed glyph set; section numbering kept zero-padded so no anchors break.

Summary: landed the Element seams endpoint backed by the now-rebuilt bodies. The 3 residuals are the Materials + Bim seam-mirror endpoints and one `element.md [1].1` Type-seed gap.

## 17 — `Component/joint.md` — hardened (2 residuals)

Integrated: `WeldType` 14->6 collapse with a 9-member `GrooveGeometry` sub-axis (no-split structural fix); `WeldProfile`/`GroovePrep`/`PlugSlot` generative records (AWS D1.1:2020 + AISC 360 J2); `WeldProcess` PJP-deduction axis (AISC J2.1); `Penetration` Cjp/Pjp + `RootTreatment`; `ElectrodeClass` A5.1 vs A5.5 fix; AISC J2-5 directional strength; ISO 13918:2017 Type SD stud geometry + `StudGrade` (web-verified); `StudClass.SteelShearKn`+`S13..S25` preserved for the steel composite consumer; `AdhesiveClass`+SSG bite; `MinimumFilletLegMm` reading `VividOrange.Profiles II`; typed `WeldRow/StudRow/AdhesiveRow` seed tables; full rename; comparer-free `BuildJointRows`.

Extended: continuous-connection capture extended in-place inside the existing `JointSection [Union]` (no fragmentation): the Weld arm grew to carry `WeldProcess`/`WeldProfile`/`Option<GroovePrep>`/`Option<PlugSlot>` with throat/shear/directional projections as `Switch`-dispatched statics; the Stud arm gained `StudGrade` and exposes L1/L2 (arc-shortening). `NominalMm` disambiguated from `EffectiveThroatMm` (the OLD conflated them).

Summary: rebuilt ground-up (target did not exist; built from retired `Connection/joint.md`, 481 lines); 36KB naive weld vocabulary became a generative-data-complete owner; 3 validated fixes; verified against the realized `component.md`.

## 18 — `Graph/element.md` (Element) — hardened (1 residual)

Integrated: composes `Relations/relation#EDGE_ALGEBRA` (Bake reads the `TypeDefinition` bind), `Graph/delta#GRAPH_DELTA`, `Composition/material#MATERIAL_COMPOSITION`, `Properties/property#PROPERTY_BAG`+`quantity#MEASURE_VALUE`, `Assessment/assessment`, `Classification`, `Geospatial`, `Projection/address#CANONICAL_WRITER`/`#CONTENT_ADDRESS` (the seed-zero `XxHash128` `RootedType` and `Content` share), `Projection/fault#FAULT_BAND` — all members verified-local; LanguageExt confirmed-by-usage (no substrate `.api`, the known gap).

Extended: rebuild executed as targeted Edits (frozen regions byte-for-byte) realizing all 5 DECIDED refinements: (1) `NodeId.RootedType(ReadOnlySpan<byte>)` + `ToTypeSeedBytes` factoring the Object arm into a shared `WriteObject` so the Type seed EXCLUDES Representations (parity unperturbed); (2) named Bake inheritance — `TypeBagsOf` -> one-pass `TypeResolutionOf`, Seq fields union+dedup via new generic `UnionBy` (collision-free ValueTuple keys, a NUL-byte separator caught and replaced); (3) `Option<TypeBinding>` + `Element.TypeId`; (4) one-hop type-resolved fallback in `MaterialsOf`; (5) `MaterialBinding->BakedMaterial` + new `TypeBinding`. Cold checks: balanced fences, no NUL bytes, no stale tokens. Bim/Compute re-binds documented as WF-2/WF-3, not residuals.

Summary: realized all 5 DECIDED refinements; both seam-mirror endpoints already landed by a prior WF-1 pass and exactly matching. No python/ts wire change.

## 19 — `Rasm.Materials/ARCHITECTURE.md` + README router — clean (2 residuals)

Integrated: verified the on-disk Component-paradigm rebuild a concurrent WF-1 writer produced (a write-race lost three times to its superior progressive output; the agent applied no edits to avoid regressing quality). Verified against the full mandate: `[01]-[DOMAIN_MAP]` collapses `Profiles/`+`Connection/` into one `Component/` sub-tree + `Projection/Component.cs`; `[02]-[SEAMS]` carries the four distinct mirror rows with every source-file column renamed and the FROZEN Appearance `CONTENT_KEY` row verbatim; `[03]-[DOMAIN_LAW]` merges `ProfileFault 2300`+`ConnectionFault 2360` into one `ComponentFault 2300` and the dual projector into one `ComponentProjector`; README router collapsed to 26 Component/ entries.

Extended: no edits applied — confirmed the live writer's output complete, correct, and stronger than the draft. No live `Profiles/`/`Connection/` paths or `ProfileFamily`/`ConnectionFamily` types remain (residual mentions are VividOrange package names, merge-history notes, or anti-pattern defect-naming). The Element `ARCHITECTURE.md` mirror endpoint independently landed, satisfying the cross-workflow contract at both endpoints.

Summary: REBUILD target verified clean on a cold dual-axis read; equivalent draft authored but not applied (write-race with the live writer).

## 20 — `Construction/{assembly,layout,nesting}.md` (Materials) — hardened (2 residuals)

Integrated: rebuilt the Construction sub-domain to consume the collapsed Component owner; verified-local against the seam (`MaterialComposition.PrimaryMaterial`/`.LayerSet`), masonry (`BondName.Course`/`MortarJoint`/`Orientation`/`Cut`/`SpecialShape`), and component (`ComponentUnit`/`Component`/`Coring`/`ComponentFamily`); `RectangleBinPack` cutting-stock yield (nesting).

Extended: the pages were ILLUSORY against the freshly-rebuilt Component contract — `layout.md` read phantom `CourseTemplate.Sequence`/`.OffsetFraction`/`.PerUnitRotationDegrees` and `run.Profile.Unit`; `assembly.md` cited `Cut.BevelDegrees`. All closed via full rewrite/edit. `LayoutRun` now carries `Component` + a resolved `ComponentUnit` (caller projects the unit once); `StationStep` rewritten for the per-unit `CourseTemplate.Units`/`UnitPlacement` shape; NEW `Placement.LateralOffsetMm` column (the across-course offset woven/basketweave bonds emit). `nesting.md` rename-clean, hardened by purging REALIZED tags + provenance + 4 hedges.

Summary: rebuilt Construction against the collapsed owner, full rename map applied, all phantom-member reads closed.

## 21 — `Rasm.Materials` index docs (README/ARCHITECTURE/IDEAS/TASKLOG) — hardened (3 residuals)

Integrated: README charter+router+package roster, ARCHITECTURE codemap+domain-law+`[2]-[SEAMS]` fence (the four WF-1 seam-mirror rows mirroring the Element endpoint verbatim), IDEAS/TASKLOG re-derived (renamed closed-card anchors + realized-collapse disposition); the FROZEN `Appearance/interchange -> Element/Graph CONTENT_KEY` row preserved verbatim. Landed the rename: git-removed the 6 orphan `Profiles`/`Connection` pages + empty dirs; applied the 5 in-scope `Appearance` anchor-only fixups (bsdf.md, graph.md).

Extended: README brought into full paradigm consistency (intro -> five-sub-domain Component model + MaterialSpec/ComponentSpec arm + owner-mints law; `[02]-[DOMAIN_PACKAGES]` re-homed); codemap charters deepened; the previously cross-sub-domain `Connection/reinforcement->Profiles` seam row retired into the codemap (an in-package relation is never a seam). Scope decision: externalized the intra-Materials design-page BODY re-binds as residual_high rather than overreaching into M-status pages under active sibling edit. VCS observation: `.planning` subdirs are physically uppercase while git still tracks lowercase paths (a git-mv concern for the commit step, not a content fix).

Summary: rebuilt all four index docs from the stale `Profiles/Connection` topology to the unified Component topology; clean 5-sub-domain tree (Appearance/Component/Construction/Projection/Properties), all 26 router targets resolve, 30 well-formed seam rows. The 3 residuals are the cross-file design-page BODY re-binds (interchange/assembly/layout/properties/sustainability) tagged WF-2.
