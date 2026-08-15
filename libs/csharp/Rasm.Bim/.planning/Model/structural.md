# [STRUCTURAL_PROJECTION]

The structural-analysis-domain reader the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector` composes: `StructuralProjection` lowers the GeometryGym structural-analysis entity surface onto NEUTRAL seam payloads — the `Map<PropertyName, PropertyValue>` attribute bag a `Relations/relation#EDGE_ALGEBRA` `Relationship.Generic` edge or a structural `Properties/property#PROPERTY_BAG` `PropertySet` node carries — so a `Rasm.Compute` frame solve reads the idealization off the ONE `Graph/element#ELEMENT_GRAPH` `ElementGraph` it already holds, never a second store. The idealized analytical line is NOT a payload this reader produces: it is content-keyed into the member `Object` node's `Graph/element#NODE_MODEL` `RepresentationContentHash` map under the `Axis` key at `Projection/semantic#SEMANTIC_PROJECTOR` `ObjectNode` time (the ONE `IfcRepresentation.Keys` representation content-keyer, every geometry — display `Body` and analytical `Axis`/`FootPrint` — hashed alike), and `Rasm.Compute` resolves the coordinate line one-hop BY CONTENT KEY from the blob store; an inline `AxisCurve`/`Vector3` coordinate field on the seam `Object` node is the named seam violation, the deleted form. This RETIRES the migration source's parallel `AnalysisModel`/`AnalysisMember`/`LoadGroup`/`Support`/`MemberConnection`/`SupportRestraint` record family keyed by `BimModel`/`GlobalId` (the "second stored record off the element" the rebuild forbids): an idealized curve member is the seam `Object` node it already is (an `IfcStructuralItem` is an `IfcProduct`, so the general projector mints it), its analytical line the content-keyed `Axis` representation, the member↔connection 6-DOF restraint and member↔activity applied load the `Generic` edge payloads `Projection/relations#RELATION_ALGEBRA` `EdgeProjection.Structural` carries, and the analysis-model / load-group grouping the `Assign.Group` edge the general `IfcRelAssignsToGroup` fold authors — never a re-modeled analysis mesh and never a parallel selection surface.

The owner is the deep STRUCTURAL half of the projection `SemanticProjector` keeps out of the general fold: the complete `IfcBoundaryCondition` restraint algebra, relationship-level member-end release, applied-load family, grouping definitions, analytical topology discriminants, the physical↔analytical correspondence roster with its graph fold, inverse authoring residue, and the SAF workbook exchange whose import authors GeometryGym entities the one projector ingests and whose export lowers the seam graph onto `ExcelModel`. `Projection/semantic#SEMANTIC_PROJECTOR` supplies both regimes — the ONE per-projection `UnitScale` and the elected `Option<EurocodePolicy>` — so no member re-derives one off its own entity's database. The reader is HOST-NEUTRAL: absent optional source detail remains `Option`/empty enrichment, while every present SI magnitude crosses `MeasureValue.OfSi` on the enclosing `Fin` rail and re-keys rejection through `ElementFault.ValueRejected`; no host geometry type, non-finite measure, or partial inverse crosses the boundary.

## [01]-[INDEX]

- [02]-[STRUCTURAL_PROJECTION]: `StructuralProjection` reads the GeometryGym structural-analysis domain — polymorphic `Attrs` entity→bag lowering, `RestraintOf` over the `RestraintFamily` release/support split with its three-way DOF `Verdict` and capsule-read warping arm, `Frame` orientation, `LoadOf` with the typed `Carrier` family, `ActionRow` case derivation, EN 1990 `Factors` and the `Combination` roster with its seismic/EQU/accidental `Elect` arms, `AtStart`/`Station` discriminants, `Author` re-stamp, the `StructuralCorrespondence` physical↔analytical roster with its `Correspondence` graph fold, and the SAF exchange — `Saf` XLSX I/O, `Workbook` graph→model lowering, the model→GeometryGym `Author` overload.

## [02]-[STRUCTURAL_PROJECTION]

- Owner: `StructuralProjection` the static structural-analysis-domain reader `SemanticProjector` composes, lowering the GeometryGym structural-analysis surface onto neutral seam payloads — never a stored record. It owns the polymorphic `Attrs` attribute-bag reader (one entry discriminating the structural entity — relationship, connection, activity, load case/group, result group, analysis model, curve/surface member — onto its restraint / load / definition bag), the `AtStart`/`Station` transient-topology discriminants, the `ActionRow` two-tier load-action derivation, the `EurocodePolicy`/`EurocodeAction` pair under which the EN 1990 combination and partial factors resolve — the policy carrying the `CombinationSet` ULS-set axis (the `DesignSituationClass` ladder names no EQU or GEO member) and the seismic `Importance` gammaI, the action roster carrying the hand-minted seismic `VariableCase` beside the four live `ENLoadCaseFactory` verbs — and the closed `StructuralCorrespondence` roster binding the physical member classes (`IfcBeam`/`IfcColumn`/`IfcMember`/`IfcPile` and `IfcSlab`/`IfcWall`/`IfcPlate`/`IfcFooting`) to their idealized `IfcStructuralCurveMember`/`IfcStructuralSurfaceMember` counterparts with the schema-derived variety sets, and the point/curve/surface connection rows to the boundary-condition families this page lowers; every row name it stamps is a `Rasm.Element` `StructuralRows` static or a `PropertyCategory.Seam.Row` mint, never a call-site spelling; the typed analysis structures the migration source minted (`AnalysisModel`, the `AnalysisMember` `[Union]`, `LoadGroup`, `Support`, `MemberConnection`, `SupportRestraint`, `StructuralLoadKind`, `StructuralCurveMemberKind`) are all GONE — the member is the seam `Object` node, the joint kind its `PredefinedType` token, the topology its neutral `Connect`/`Generic` edges, the restraint/load the typed `PropertyValue` edge payloads, and the analytical line the `Axis`-keyed content hash in the member's `Representations` map (content-keyed at `ObjectNode`, resolved one-hop by `Rasm.Compute`, never read or baked here).
- Entry: `Attrs(BaseClassIfc? entity, UnitScale scale, Option<EurocodePolicy> eurocode, IIfcProfileStore profiles, Op key)` lowers every supported structural entity through one `Fin<Map<PropertyName, PropertyValue>>` dispatch; `profiles` is the ONE content-addressed fragment lane the mandatory `IfcRelConnectsWithEccentricity.ConnectionConstraint` geometry preserves through, so the eccentricity row carries a content key and never inlined coordinates. Both regime arguments are REQUIRED and caller-supplied: `scale` is the one per-projection unit regime the `Projection/semantic#SEMANTIC_PROJECTOR` fold head already holds, and `eurocode` the annex-plus-`IDesignSituation` policy VALUE under which the load arm resolves the EN 1990 combination and partial factors — `None` emits the IFC-declared attributes alone and never a `RecommendedValues` set nobody selected, and the elected situation belongs to the composition that knows the project's annex, never to this reader. Every magnitude leaves its reader model-NATIVE beside the IFC measure type its source attribute declares, then crosses ONE `Admit` entry that resolves the frozen `MeasureDimensions` row, coerces on it, and traverses `MeasureValue.OfSi` re-keying `ElementFault.ValueRejected`; a non-finite spring stiffness faults through that same gate rather than reading as a free DOF. `AtStart` and `Station` return `Option` discriminants, so unresolved topology emits no assertion. `Author(DatabaseIfc, IfcObjectDefinition, Map<PropertyName, PropertyValue>, Op key)` re-stamps verified restraint and single-force constructors and returns `Fin<Seq<PropertyName>>` — the unconsumed row names as typed fidelity residue the egress end MUST fold, the GeometryGym ctor throw crossing as `BimFault.CodecReject`. `Saf(SafOp operation, IExcelImportService imports, IExcelExportService exports, IExcelValidator validator, Op key)` validates and executes both XLSX directions over `ExcelModel.Objects`; the source version derives from `ExcelModel.OriginalVersion` — the import service alone assigns it, so a GRAPH-authored export model coalesces onto the target version rather than handing the validator a null `Version` — while the operation carries only the caller-selected target version. `Correspondence(ElementGraph graph, Op key)` yields the typed `CorrespondenceRow` set off the seam Generic edges — the physical↔analytical member pairs, roles, varieties, and joints with their `AtStart`/`Eccentricity` reads. `Workbook(ElementGraph graph, ResolveAxis resolve, Op key)` lowers the graph onto the SAF `ExcelModel` the `Saf` export leg writes, geometry crossing only through the content-key `ResolveAxis` hop; `Author(DatabaseIfc db, IfcSpatialElement host, ExcelModel model, Op key)` realizes the import by authoring the GeometryGym structural-analysis entities the ONE `SemanticProjector` then ingests, returning the SAF residue rows as `Fin<Seq<string>>` on the same fidelity idiom the entity-keyed `Author` holds.
- Auto: `Projection/semantic#SEMANTIC_PROJECTOR` owns the analytical line, not this reader — it content-keys every `RepresentationIdentifier` alike, so the `Axis` line and the heavy display body both ride `RepresentationContentHash` and `Rasm.Compute` resolves the line's coordinates one-hop by content key from the blob store. `RestraintOf` takes a `RestraintFamily` POLICY VALUE carrying its own row families, so the rel-level member END RELEASE and the connection-level joint SUPPORT land as two families on one bag and neither reads as the other; the `IfcBoundaryNodeConditionWarping` arm precedes its own base and reads the sealed seventh DOF through the ONE `IfcInternals` `[UnsafeAccessor]` capsule [SEALED_PAYLOAD_RULING]. Each DOF row is a THREE-WAY verdict — zero is the fixity `Boolean`, positive is the SI spring `Measure`, negative-finite or non-finite is the typed `ElementFault.ValueRejected` — because a row that asserts a support cannot be lowered from a magnitude the reading never established. The 1D load families mint the typed `VividOrange.Loads` carrier beside their rows, and a `LOAD_COMBINATION_GROUP` under an elected policy lowers the combination roster its design situation calls for — the live `CreateStrGeoSetB`/`SetC` verbs on the policy's `CombinationSet`, the live `CreateSeismic` Eq 6.12a/b roster with the policy's gammaI leading the seismic-row cases, and the hand-assembled `EquilibriumCombination`/`AccidentalCombination` sweeps where the factory's own EQU and accidental verbs throw — each `ILoadCombination.Definition` expression beside the `GetFactoredLoads()` design actions, sharing one combination order. `RestraintOf` reduces every DOF through one type switch over GeometryGym's SPLIT select hierarchy — `IfcTranslationalStiffnessSelect` and the two subgrade-reaction selects derive `StiffnessSelect<TMeasure>` while `IfcRotationalStiffnessSelect` stands alone, so no common base admits a single property pattern, yet all four independently expose a `Rigid` Boolean beside a `Stiffness` whose `.Measure` rides `IfcDerivedMeasureValue` — onto ONE row per degree of freedom whose `PropertyValue` CASE carries the verdict: a rigid or free DOF a `Boolean`, a positive stiffness the SI spring `Measure` typed by the select's OWN declared measure — the node pair's linear and rotational stiffnesses against the edge pair's two subgrade reactions, three reaction types one exponent apart — a non-finite magnitude the typed `ElementFault.ValueRejected` the `MeasureValue.OfSi` gate raises. The orientation frame is ONE `StructuralRows.Frame` positional list, so a skewed support's restraint axes and a 2D analysis model's loading plane land on the same row rather than a prefix-built name family. The 1D load families SHARE the consumer-neutral `ForceX..Z`/`MomentX..Z` wire names the `Rasm.Compute` `Vec(g, "Force")`/`Vec(g, "Moment")` probes read for point AND uniform actions, the family discriminated by the `LoadType` token and each component's own IFC measure type, never by the row name; `IfcStructuralLoadSingleDisplacement` carries frame attrs alone because its components cross no public accessor. The `ActionRow` derivation is TWO-TIER — the specific `CaseSources` row over `IfcActionSourceTypeEnum`, else the group's `ActionType` nature with `PERMANENT_G` the dead permanent action and every other nature the imposed variable one — so a prestress, shrinkage, or settlement group factors permanent instead of silently mis-casing variable, and the elected `EurocodeAction` mint supplies `Psi0`/`Psi1`/`Psi2` beside the elected `IDesignSituation`'s whole partial-factor set and its design-situation class. The `LoadedBy`/`HasResults` model→group joins ride GlobalId `PropertyValue.List` payloads because no `IfcRel*` edge carries these direct set attributes and a count erases the wiring a multi-model file needs.
- Receipt: the readers' payloads land on the ONE seam `ElementGraph` — the six-DOF restraint, frame, supported length, and `AtStart` on the `IfcRelConnectsStructuralMember` `Generic` edge, the applied load and `Station` on the `IfcRelConnectsStructuralActivity` `Generic` edge, and the load-group / load-case / result-group / analysis-model / member definitions on structural `PropertySet` nodes, the idealized analytical line riding the member `Object`'s `Axis`-keyed content hash in `Representations` — so the `Rasm.Compute` structural runner resolves the analytical line one-hop by content key, reads the support fixity-or-stiffness and the load components off the member's incident edges through the SAME `Rasm.Element` `StructuralRows` statics this reader stamps (`AtStart`, `Station`, `SupportedLength`, `Frame`, `LoadKind`, `Case`, and the `Translation`/`Rotation`/`Force`/`Moment` axis families) rather than a duplicated literal at either end, one DOF row carrying either the boolean restraint or the spring measure, and joins the section properties the `Graph/element#ELEMENT_GRAPH` `SectionOf` accessor bakes off the member's `ProfileSet` composition — resolved through the member's `Component` Type by the seam's one-hop type-resolved fallback (an occurrence with no own `ProfileSet` reads its `Element.Type` `Component`'s `SectionProperties`, the `Assign.TypeDefinition` inheritance the `Bake` fold applies), so an analytical member sharing a standardized cross-section reads it once off the deduped Type rather than per occurrence, the frozen Op-free `SectionOf(member)` signature untouched — the analysis owner producing the idealized graph, the solve and the typed `FrameModel` living wholly in `Rasm.Compute`, never re-projected here. The `Correspondence` fold serves that same consumer the member/joint spine as typed rows — the `Rasm.Compute` analytical-model assembly reads physical↔analytical pairs, roles, varieties, and joint discriminants in one call instead of re-walking Generic edges — and the `Workbook` SAF lowering consumes the identical rows as its member spine, so the analytical exchange and the analytical solve read one correspondence owner. A beam's analytical line, a slab's idealized thickness, a column-base node's six-DOF skewed support, a quarter-span point load, and a self-weight-vectored gravity case each ride the one graph the consumer already holds.
- Packages: GeometryGymIFC_Core, StructuralAnalysisFormat, VividOrange.Cases, VividOrange.Loads, UnitsNet, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new boundary-condition kind is one arm on the `RestraintOf` switch reading the next `IfcBoundaryCondition` subtype's stiffness selects through the SAME `object`-pattern reducer, and a new restraint SEMANTICS (a third fact beside release and support) is one `RestraintFamily` value carrying its own row families with no reducer edit; a new typed load carrier is one `Carrier` arm and its `Components` counterpart; a new combination regime is one `Elect` arm over the class-then-set switch; a new applied-load family is one arm on the `Vectors` switch reading the next `IfcStructuralLoad` subtype's components; a new structural entity or relationship with a definition bag is one arm on the polymorphic `Attrs` switch; a new action-source classification is one `CaseSources` row carrying its token, `ActionClass`, category, and mint (the `ActionType` nature tier already totalizing the residue); a new Eurocode action is one `EurocodeAction` row carrying its `ENLoadCaseFactory` mint; a new national deviation is a `NationalAnnex` value on the policy, never a per-country branch; a new degree of freedom or load axis is one `StructuralRows.Axes` entry at the seam owner every family absorbs; a new measured payload is one row naming its GeometryGym measure type, and a measure type this reader stamps whose row the `Projection/semantic#SEMANTIC_PROJECTOR` `MeasureDimensions` table lacks lands as one row THERE and none here; a new analytical-geometry kind is one more `RepresentationIdentifier` the `Projection/semantic#SEMANTIC_PROJECTOR` `IfcRepresentation.Keys` content-keyer maps into the member's `Representations`, resolved by content key downstream; a new physical member family is one entry on the owning `StructuralCorrespondence` row's physical map (its SAF role riding the same entry) and a new analytical family one roster row, never a second classifier beside the roster; a new SAF worksheet is one arm on the `Workbook` and `Author` folds beside the roster row that classifies it, and a new SAF↔IFC vocabulary axis is one correspondence map (`Behaviours`/`CaseTypes`/`SourceOf`) both legs share; never a per-member-type analysis record, never a `RestraintAttrs`/`LoadAttrs` sibling family, never a second analysis store, never a re-modeled analytical mesh, and never an inline analytical coordinate field on the node.
- Boundary: `StructuralProjection` produces ONLY neutral seam payloads — the migration `AnalysisModel`/`AnalysisMember`/`LoadGroup`/`Support`/`MemberConnection`/`SupportRestraint`/`StructuralLoadKind`/`StructuralCurveMemberKind` typed store is the deleted form, the idealized member being the seam `Object` node (an `IfcStructuralItem` IS an `IfcProduct` the general fold mints), its joint kind the `Object.PredefinedType` token, its topology the neutral `Connect`/`Generic` edges, and its restraint/load the typed `PropertyValue` edge payloads; the entity→bag reading is ONE polymorphic `Attrs` discriminating by input value and a `RestraintAttrs`/`LoadAttrs`/`GroupAttrs`/`ModelAttrs` sibling-method family is the deleted form; the `IfcRelConnectsStructuralMember`/`IfcRelConnectsStructuralActivity` edge bag builds from ONE `Attrs(rel, scale, eurocode, profiles, key)` read and a caller-side bag-plus-manual-`Add` assembly is the deleted two-step; the 1D load components ride the consumer-neutral `ForceX..Z`/`MomentX..Z` wire names the `Rasm.Compute` `StructuralReads` accessors probe, and a per-family `LinearForceX`-style namespace that forks the uniform-load read onto silent zeros is the deleted form (the family discriminant is the `LoadType` token + the component `Dimension`, never the name); the structural reader is the DEEP half `SemanticProjector` composes and re-introducing it as a standalone `IElementProjection` (a second projector minting the member nodes the general fold already mints) is the deleted form; the analytical line rides the member `Object`'s `Axis`-keyed content hash in `RepresentationContentHash` (content-keyed at `ObjectNode` by `IfcRepresentation.Keys`, resolved one-hop by content key in `Rasm.Compute`), and an inline `AxisCurve`/`Vector3` analytical-coordinate field on the seam node — like a RhinoCommon `Curve`/`Brep` field or an in-process BRep tessellation — is the named seam violation, the deleted form (the `AtStart`/`Station`/`Frame` topology reads are TRANSIENT, emitting only Boolean/scalar attributes); every row name this reader stamps resolves to an OWNER-declared static — the cross-package structural vocabulary through `Rasm.Element` `StructuralRows` and every remaining name through the owner-blessed empty-prefix `PropertyCategory.Seam.Row` — so a call-site `PropertyName.Create` anywhere in this reader is the deleted form that forks the key space between the Bim writer and its non-referencing `Rasm.Compute` reader, and a name a second package begins keying on is PROMOTED to `StructuralRows` at the Element owner rather than re-declared here; the restraint preserves the SI spring stiffness as a `MeasureValue` on the DOF's OWN row, the `PropertyValue` case carrying restraint-versus-spring, and a parallel `<dof>Stiffness` roster beside the fixity row — the shape that strands the magnitude on every reader keying only the boolean — is the deleted form, as is a boolean-only fixity that drops the magnitude outright; a negative-finite or non-finite stiffness FAULTS on the DOF's own three-way verdict and a `Boolean(false)` free-DOF verdict fabricated from a malformed magnitude is the deleted form — a positive claim no reading establishes, and one no egress filter retracts because a `Boolean` row is never dropped; the rel-level END RELEASE and the connection-level SUPPORT are TWO row families reduced through one `RestraintFamily` policy value, and the `rel.AppliedCondition ?? connection.AppliedCondition` fallback that fused them is the deleted form that let a released beam end read as a support and a supported joint read as a free one; the sealed warping stiffness reads through the ONE `IfcInternals` capsule and is UNAUTHORABLE (every `IfcWarpingStiffnessSelect` constructor is internal, so the public warping-condition ctor takes an argument no caller can construct), and its SUPPORT row is the Element roster's own `StructuralRows.Warping["Axial"]` — the seventh-DOF family the seam already promoted, so a page-local `Seam.Row("Warping")` re-mint is the key-space fork the custody law deletes — while the `Author` CONSUME set is the six `Translation`/`Rotation` support rows alone, deliberately NOT `StructuralRows.Dofs` (the Element `Dofs` roster INCLUDES the warping family, so consuming it would erase the un-re-authorable row from the residue without authoring anything), which is what carries the warping row to the egress receipt as `FidelityDrop.StructuralResidue` rather than re-authoring a stiffness the file never declared; the magnitude roster is MIXED and the split is a ROW property, never a call-site ladder — a row READ off a GeometryGym attribute admits SI-NATIVE through the typed `MeasureValue.OfSi(QuantityType, Dimension, double)` mint carrying the IFC measure type that attribute declares, which is the `Properties/property#DETAIL_SCHEMA` round-trip law's own condition (the producer names the identity truthfully, so the payload re-exports as itself and an authored and a re-imported payload content-key on the same name), while a row this reader DERIVES (the normalized `Station`, every EN 1990 combination and partial factor) or reads off an attribute the schema declares a bare `IfcReal` (the direction ratios) names no IFC measure at all and takes the DIMENSION-ONLY `MeasureValue.OfSi(Dimension, double)` mint, staying dimension-anonymous because a stamped name is fabricated identity where nothing was ingested — those, and only those, are the `Projection/egress#IFC_EGRESS` `RaiseMeasure` tail's COUNTED `MeasureFlattened` drops, each a dimensionless coefficient no IFC physical quantity spells; the measure name is spelled as the GeometryGym `IfcValue` TYPE, so the seam `Dimension`, the coercion axis, and the egress raise row all resolve from ONE symbol through the frozen `Projection/semantic#SEMANTIC_PROJECTOR` `MeasureDimensions` table, a page-local exponent static beside that table is the deleted second dimension source, and a named type the table does not carry rails rather than coercing on a guessed vector; the typed `VividOrange.Loads` carrier is the load payload the EN 1990 algebra folds `ILoad.Factor(Ratio)` across and a hand-multiplied partial or combination factor beside it is the deleted form, its components minted FROM already-coerced SI magnitudes and read back as SI doubles so the carrier lane reaches neither `ToUnit(UnitSystem.SI)` nor `QuantityTypeConverter`; ROUTING a measure ADMISSION through a UnitsNet quantity struct is the deleted form on two independent counts — the registry ingress coerces through `ToUnit(UnitSystem.SI)`, which throws `No units were found for the given UnitSystem` for every quantity whose SI unit-info walk is empty (`LinearDensity`, `ThermalResistance`, `Mass`, `Density`, `Torque`, `HeatTransferCoefficient` among the majority of the registry), so the admission rails `ValueRejected` rather than landing a measure; and the `QuantityTypeConverter` wire is a culture-formatted abbreviation string (`1 kg/m`, `1 m²K/kW`) while the seam wire is the `Projection/address#CONTENT_ADDRESS` `CanonicalWriter.Measure` byte run — the length-prefixed type token, the IEEE-754 SI magnitude, and the seven ordinal exponents — so the two currencies are incommensurable and neither reproduces the other. The INGESTED measure-type NAME is the round-trip identity `RaiseMeasure` derives from, so re-tokening it to a registry quantity name forks every content key AND strands the raise table; the orientation frame is ONE `StructuralRows.Frame` positional list and a prefix-built `RestraintAxisX`/`PlaneRefZ` name family is the deleted form; the Eurocode regime is the `EurocodePolicy` VALUE (annex, elected `IDesignSituation`, ULS combination set, seismic importance, imposed category, snow altitude) and a per-country branch, a hand-tabulated psi set beside `ENLoadCaseFactory`, or a `MissingNationalAnnexException`/`NotImplementedException` propagating past the one `BimFault.CapabilityMiss` seam is the deleted form; the partial factors reach this reader through `IDesignSituation` — the package's OWN partial-factor policy contract — because every `EN.ITableA1_2` implementation (`ENTableA1_2A`/`B`/`C`) is INTERNAL to `VividOrange.Cases` and no public member yields one, so an `ITableA1_2` column on the policy is a value no caller can construct and the whole factor composition it fronts is unreachable; the elected situation carries the FULL set (`GammaSup`/`GammaInf`/`GammaQ1`/`GammaQi`/`Xi` beside the situation class) and a two-factor slice stranding the leading-action and 6.10a-b reduction factors on every combination consumer is the deleted form; an absent policy emits no factor row at all rather than a fabricated `NationalAnnex.RecommendedValues` factor, and the policy is REQUIRED on the entry so every caller states its regime — a defaulted parameter that let every landed caller elect nothing left the whole `VividOrange.Loads`/`Cases` composition unreachable; the unit regime is likewise ONE caller-supplied `UnitScale` threaded from the projector's fold head, and a per-entity `UnitScale.Of(entity.Database)` rebuild re-reading one project regime per row — falling to `UnitScale.Si` on a null database and silently pricing a mm-declared model's stiffnesses and forces as SI — is the deleted form; the re-author residue is a TYPED `Fin<Seq<PropertyName>>` the egress consumer folds into its fidelity receipt, and a bare `Map` return an egress arm can drop on the floor is the deleted form that made the residue promise unobservable; the load family is read over the full `IfcStructuralLoad` subtype set and a single-force-only reader is the deleted form; the `Case` derivation is total over `IfcActionSourceTypeEnum` through the two-tier source-row-then-`ActionType`-nature fold and a partial source map folding every permanent action to `live` is the deleted mis-casing; the member↔connection topology is the seam `Generic` edge (wire-name `IfcRelConnectsStructuralMember`) the `EdgeProjection.Structural` fold authors and a typed `MemberConnection` record is the deleted form; the content-key identity is the seam `ElementGraph` content address (the kernel seed-zero `XxHash128` over `Node.ToCanonicalBytes`) the consumer reads the graph by, and minting a second `(GeometryKey, PropertyKey)` scheme or reaching the up-stratum `Rasm.Compute` `InterchangeIdentity` is the named cross-folder drift defect; the GeometryGym structural-analysis surface is consumed as settled vocabulary (`.api/api-geometrygym-ifc` structural-analysis-domain rows) and a hand-rolled structural-member reader is the deleted form; the reader is TOTAL and routing a structural enrichment onto `Model/faults#FAULT_BAND` `BimFault` is the deleted form (the class/reference rails are the general fold's `Fin<GraphDelta>`); the physical↔analytical correspondence is the ONE `StructuralCorrespondence` roster read through the ONE `Correspondence` fold — a second Generic-edge walker in `Rasm.Compute` or a per-consumer class map is the deleted form, and every roster column is consumed (the physical map keys classification and elects the SAF role, the variety set grounds the behaviour maps, the dimension routes the support arms); the ULS set election is the policy's `CombinationSet` VALUE and switching on a `DesignSituationClass` member the package does not declare is the uncompilable form the rebuilt `Elect` replaced; `ENCombinationFactory.CreateEquSetA` (both overloads) carries an unconditional `item2[1]`/`list[2]` tail — it throws `ArgumentOutOfRangeException` below two variable cases and silently OVERWRITES the third combination above, so no input shape survives it intact — and `CreateAccidental` is live but REQUIRES the leading AEd `IVariableCase` the IFC action vocabulary never classifies (it dereferences `accidentalCase.Name` unconditionally, so the caseless call NREs), so the EQU and accidental rosters HAND-ASSEMBLE the package's own combination records under the elected `IDesignSituation` — construction only, the factoring staying inside `GetFactoredLoads` — and a fence re-electing either verb on the inputs this fold holds is the refuted form; the SAF import AUTHORS GeometryGym entities and re-enters through the ONE `SemanticProjector`, so a SAF-side projector minting seam member nodes is the same deleted standalone-projector form named above; the SAF legs are residue-HONEST with their negatives stated on the arm that owns each — export: the eccentricity STEP fragment, the thermal gradients, and the EN combination roster name no SAF cell and stay off the workbook; import: the surface-connection subsoil and support-deformation rows are UNAUTHORABLE (sealed internal GG payloads), the rigid-link/member/cross relations carry no IFC counterpart, and every directional or non-linear constraint linearizes COUNTED into the residue — so the `Exchange/format#FORMAT_AXIS` `saf` row's `CanImport`/`CanExport` flags stand against these named arms and no aspirational capability.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Bim.Projection;   // UnitScale — the ONE per-projection regime the SEMANTIC_PROJECTOR fold head threads in;
                             // PropertyLowering.MeasureDimensions — the package-internal measure roster every row signs on;
                             // IIfcProfileStore — the ONE content-addressed fragment lane the eccentricity geometry preserves through;
                             // IfcRelKind — the wire-name rows the Correspondence fold keys the seam Generic edges on
using Rasm.Bim.Semantics;    // IfcInternals — the ONE [UnsafeAccessor] capsule the sealed warping stiffness reads through
using Rasm.Domain;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;             // Relationship.Generic — the seam edges the Correspondence and Workbook folds read
using SAF.DataAccess.Contracts;
using SAF.DataAccess.Models;
using SAF.DataAccess.Models.Enums;
using SAF.DataAccess.Models.Loads;
using SAF.DataAccess.Models.StructuralElements;
using SAF.DataAccess.Models.Subtypes;     // ExcelFlexibleEnum/ExcelLoadDirectionVector/ExcelMemberThickness — the typed SAF cells
using Thinktecture;
using UnitsNet;                           // Force/ForcePerLength/Pressure/Torque/Ratio — the ILoad component quantities — plus the
                                          // SAF field quantities (Length/Angle/RotationalStiffness/Temperature), every one minted
                                          // FROM already-coerced SI magnitudes and read back as SI doubles: neither
                                          // ToUnit(UnitSystem.SI) nor QuantityTypeConverter is reached, so neither count of the
                                          // no-UnitsNet-at-the-measure-boundary law fires on the carrier or SAF lanes
using VividOrange.Loads;                  // ILoad + Factor(Ratio), the PointForce/PointMoment/LineForce/AreaForce/Gravity carrier
                                          // family, IDesignSituation, DesignSituationClass
using VividOrange.Loads.Cases;            // ActionClass, ImposedLoadCategory, IVariableCase, PermanentCase, ENLoadCaseFactory
using VividOrange.Loads.Combinations;     // ENCombinationFactory, ILoadCombination — the EN 1990 combination algebra
using VividOrange.Standards.Eurocode;     // NationalAnnex — the key every psi table dispatches on
using static LanguageExt.Prelude;

namespace Rasm.Bim.Model;

// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public abstract partial record SafOp {
    private SafOp() { }

    public sealed record Import(Stream Workbook, Version TargetVersion) : SafOp;
    public sealed record Export(Stream Workbook, ExcelModel Model, Version TargetVersion) : SafOp;
}

// The ONE content-key→coordinates hop the SAF lowering takes: the composition binds it to the blob store the member
// Axis and connection Vertex representations were content-keyed into — the same one-hop Rasm.Compute reads — so no
// coordinate ever rides a seam node and no second geometry lane opens here. None means the key resolves no stored
// polyline, which Workbook treats as the named degradation: a member or node with no resolvable geometry emits no
// SAF coordinate cell rather than a fabricated span.
public delegate Option<Seq<Vector3>> ResolveAxis(UInt128 contentKey);

// The EN 1990 Annex A1 ULS combination-set election — EQU Set A, STR Set B, STR/GEO Set C — the persistent and
// transient design situations resolve their combination arm on. The situation CLASS cannot carry it: the package's
// DesignSituationClass ladder is persistent/transient/accidental/seismic and names no equilibrium or geotechnical
// member, so the set is its own policy axis — the same split SAF's ExcelLoadCaseCombinationStandard wire makes
// between EnUlsSetB and EnUlsSetC.
public enum CombinationSet : byte { SetB = 0, SetC = 1, SetA = 2 }

// The physical↔analytical correspondence the seam graph carries but nothing owned: ONE closed roster keyed on the
// analytical entity name. A member row carries the physical IfcClass→SAF-role map its family admits (the map's keys
// classify the physical counterpart, its values elect the SAF member Type token — IfcPile and other roles the SAF
// enum lacks ride ExcelFlexibleEnum's own other-text lane), the schema-derived variety allowed-set its
// PredefinedType tokens draw from, and the analytical topology dimension; a connection row (empty physical map — a
// connection idealizes a joint, and IfcRelConnectsStructuralElement never binds one) carries the
// IfcBoundaryCondition family its restraint lowers through — the node selects at dimension 0, the two subgrade
// selects at dimension 1, the SEALED face condition at dimension 2 (internal fields, the Attrs empty-bag arm) — and
// every row names the SAF worksheet classes it exchanges as. Produced into CorrespondenceRow values by the
// Correspondence fold (the IfcRelConnectsStructuralElement/IfcRelConnectsStructuralMember Generic-edge reader);
// consumed by the Rasm.Compute analytical-model assembly and by the Workbook/Author SAF arms, whose member,
// support, and role elections all key on these rows — never a second reader beside this owner.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class StructuralCorrespondence {
    public static readonly StructuralCorrespondence CurveMember = new("IfcStructuralCurveMember",
        physical: toMap(Seq((nameof(IfcBeam), nameof(ExcelMember1DType.Beam)), (nameof(IfcColumn), nameof(ExcelMember1DType.Column)),
            (nameof(IfcMember), nameof(ExcelMember1DType.General)), (nameof(IfcPile), "Pile"))),
        varieties: toSeq(Enum.GetNames<IfcStructuralCurveMemberTypeEnum>()), dimension: 1,
        condition: Option<string>.None, saf: Seq(nameof(ExcelStructuralCurveMember)));
    public static readonly StructuralCorrespondence SurfaceMember = new("IfcStructuralSurfaceMember",
        physical: toMap(Seq((nameof(IfcSlab), nameof(ExcelMember2DType.Plate)), (nameof(IfcWall), nameof(ExcelMember2DType.Wall)),
            (nameof(IfcPlate), nameof(ExcelMember2DType.Plate)), (nameof(IfcFooting), nameof(ExcelMember2DType.Plate)))),
        varieties: toSeq(Enum.GetNames<IfcStructuralSurfaceMemberTypeEnum>()), dimension: 2,
        condition: Option<string>.None, saf: Seq(nameof(ExcelStructuralSurfaceMember)));
    public static readonly StructuralCorrespondence PointConnection = new("IfcStructuralPointConnection",
        physical: Map<string, string>(), varieties: Seq<string>(), dimension: 0,
        condition: Some(nameof(IfcBoundaryNodeCondition)),
        saf: Seq(nameof(ExcelStructuralPointConnection), nameof(ExcelStructuralPointSupport), nameof(ExcelRelConnectsStructuralMember)));
    public static readonly StructuralCorrespondence CurveConnection = new("IfcStructuralCurveConnection",
        physical: Map<string, string>(), varieties: Seq<string>(), dimension: 1,
        condition: Some(nameof(IfcBoundaryEdgeCondition)),
        saf: Seq(nameof(ExcelStructuralCurveConnection), nameof(ExcelStructuralEdgeConnection)));
    public static readonly StructuralCorrespondence SurfaceConnection = new("IfcStructuralSurfaceConnection",
        physical: Map<string, string>(), varieties: Seq<string>(), dimension: 2,
        condition: Some(nameof(IfcBoundaryFaceCondition)),
        saf: Seq(nameof(ExcelStructuralSurfaceConnection)));

    public Map<string, string> Physical { get; }
    public Seq<string> Varieties { get; }
    public int Dimension { get; }
    public Option<string> Condition { get; }
    public Seq<string> Saf { get; }

    public bool IsMember => !Physical.IsEmpty;

    static readonly FrozenDictionary<string, StructuralCorrespondence> ByAnalytical =
        Items.ToFrozenDictionary(static row => row.Key, static row => row, StringComparer.OrdinalIgnoreCase);

    static readonly FrozenDictionary<string, StructuralCorrespondence> ByPhysical =
        Items.SelectMany(static row => row.Physical.Keys.Select(cls => (Class: cls, Row: row)))
            .ToFrozenDictionary(static pair => pair.Class, static pair => pair.Row, StringComparer.OrdinalIgnoreCase);

    public static Option<StructuralCorrespondence> OfAnalytical(string ifcClass) =>
        ByAnalytical.TryGetValue(ifcClass, out StructuralCorrespondence? row) && row is { } hit ? Some(hit) : None;

    public static Option<StructuralCorrespondence> OfPhysical(string ifcClass) =>
        ByPhysical.TryGetValue(ifcClass, out StructuralCorrespondence? row) && row is { } hit ? Some(hit) : None;
}

// Which EN 1990 Annex A1.1 action a source classifies as, and the ONE case mint that answers both its psi factors
// and its combination payload. Four rows mint through the live ENLoadCaseFactory verbs — each Create* takes the
// case's own load roster, so ONE mint serves the factor read (an empty roster) and the combination fold (the
// activities' typed carriers): the case arrives with its psi set off the package's own Table A1.1 singletons AND
// its loads in one call, never a construction followed by a mutation pass, and this reader never tabulates a psi
// beside the package that owns the table — while the seismic row hand-mints the package's own VariableCase, the
// factory shipping no seismic verb. An action with no mint carries no case and therefore no psi, the honest
// reading for a source the code does not classify. The imposed mint is category-keyed and yields None when the
// project declares no category, so a psi row is absent rather than defaulted onto whichever Category A-H the
// reader picked.
[SmartEnum<string>]
public sealed partial class EurocodeAction {
    public static readonly EurocodeAction Imposed = new("imposed", static (policy, loads) =>
        policy.Imposed.Map(category => (IVariableCase)ENLoadCaseFactory.CreateImposed(loads, category, policy.Annex)));
    // ENLoadCaseFactory ships NO seismic verb (and no fatigue surface anywhere in the train), so the seismic CASE
    // hand-mints the package's own VariableCase: the zero psi triple is the Annex A1.1 statement that an AEd never
    // rides as an accompanying action — the table carries no psi column for it to tabulate — while the COMBINATION
    // side is the LIVE ENCombinationFactory.CreateSeismic Eq 6.12a/b roster the Elect seismic arm composes with the
    // policy's gammaI onto SeismicCombination.LeadingSeismicPartialFactor. EN 1998 spectra and behaviour factors
    // have no producer in the package train and stay authored upstream of the load roster this mint receives.
    public static readonly EurocodeAction Seismic = new("seismic", static (policy, loads) =>
        Some((IVariableCase)new VariableCase {
            Name = "seismic", Loads = loads,
            CombinationFactor = Ratio.FromDecimalFractions(0),
            FrequentFactor = Ratio.FromDecimalFractions(0),
            QuasiPermanentFactor = Ratio.FromDecimalFractions(0),
        }));
    public static readonly EurocodeAction Snow = new("snow", static (policy, loads) =>
        Some((IVariableCase)ENLoadCaseFactory.CreateSnow(loads, policy.Annex, policy.AltitudeAbove1000m)));
    public static readonly EurocodeAction Thermal = new("thermal", static (policy, loads) =>
        Some((IVariableCase)ENLoadCaseFactory.CreateThermal(loads, policy.Annex)));
    public static readonly EurocodeAction Wind = new("wind", static (policy, loads) =>
        Some((IVariableCase)ENLoadCaseFactory.CreateWind(loads, policy.Annex)));

    [UseDelegateFromConstructor]
    public partial Option<IVariableCase> Mint(EurocodePolicy policy, IList<ILoad> loads);
}

// --- [MODELS] ----------------------------------------------------------------------------- The
// Eurocode regime as ONE policy value: the national annex every psi lookup keys on, the IDesignSituation the
// composition elected, the project's imposed-load category, and the snow-altitude discriminant the snow factory
// takes. IDesignSituation IS the package's own partial-factor policy contract and the ONLY public reach to the EN
// 1990 Table A1.2 gammas — the ENTableA1_2A/B/C singletons are INTERNAL to VividOrange.Cases and no public member
// hands one out, so an ITableA1_2 column on this policy is a value no caller can supply, and the composition
// instead states its situation directly or lifts the one ENCombinationFactory.CreateEquSetA/CreateStrGeoSetB/
// CreateStrGeoSetC minted onto its combinations. The contract carries the WHOLE partial-factor set — gammaG,sup /
// gammaG,inf / gammaQ,1 / gammaQ,i / the 6.10a-b reduction factor xi — beside the design-situation class, so the
// reader stamps every factor the combination algebra consumes rather than the two permanent ones alone. Absent
// policy means absent factors: the reader emits the IFC-declared attributes alone rather than stamping
// RecommendedValues nobody selected.
// Set is the ULS combination-set axis the persistent/transient Elect arm reads — the DesignSituationClass ladder
// names no EQU or GEO member, so the set cannot derive from the situation — and Importance is the seismic gammaI
// the Elect seismic arm threads onto SeismicCombination.LeadingSeismicPartialFactor; only the Seismic class reads
// it, and the composition that knows the building's EN 1998 importance class states it (class II reads 1.0).
public readonly record struct EurocodePolicy(
    NationalAnnex Annex, IDesignSituation Situation, CombinationSet Set, Ratio Importance,
    Option<ImposedLoadCategory> Imposed, bool AltitudeAbove1000m);

// One resolved action row: the consumer-neutral case token, the EN 1990 action nature the combination algebra factors
// under, the imposed category a Category A-H action carries, and the psi-factor mint. It replaces the bare token map —
// the token alone stranded every consumer re-deriving the nature it already knew and left the code factors unreachable.
internal readonly record struct ActionRow(
    string Case, ActionClass Class, Option<ImposedLoadCategory> Imposed, Option<EurocodeAction> Action);

// One physical↔analytical correspondence fact off the seam graph: the analytical member node, its optional
// physical counterpart (an analytical-only model binds none), the roster row that classified it, the member's own
// variety token, the SAF role the physical class elects (None when unbound — the SAF Type column stays unset
// rather than fabricating a role), and the member's joints. Produced by StructuralProjection.Correspondence;
// consumed by the Rasm.Compute analytical-model assembly and the Workbook SAF lowering.
public readonly record struct CorrespondenceRow(
    NodeId Analytical, Option<NodeId> Physical, StructuralCorrespondence Kind, string Variety,
    Option<string> SafRole, Seq<CorrespondenceJoint> Joints);

// One member joint: the connection node, its roster row, the AtStart discriminant, and the eccentricity content
// key — each read back through the SAME owner-declared row this reader stamped on the
// IfcRelConnectsStructuralMember Generic edge, so producer and consumer share one spelling.
public readonly record struct CorrespondenceJoint(
    NodeId Connection, StructuralCorrespondence Kind, Option<bool> AtStart, Option<UInt128> Eccentricity);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class StructuralProjection {
    public static Fin<ExcelModel> Saf(
        SafOp operation,
        IExcelImportService imports,
        IExcelExportService exports,
        IExcelValidator validator,
        Op key) =>
        operation.Switch<Fin<ExcelModel>>(
            import: request => Try.lift(() => imports.Import(request.Workbook, request.TargetVersion)).Run()
                .MapFail(error => new BimFault.CodecReject(key, $"saf-import:{error.Message}"))
                .Bind(model => AdmitSaf(validator.ValidateForImport(model, request.TargetVersion, model.OriginalVersion), key)),
            // A GRAPH-authored ExcelModel carries no source workbook, so its OriginalVersion is unset (the ctor
            // never assigns it — only the import service does); the target IS the source currency for a model born
            // at that version, so the coalesce states it rather than handing the validator a null Version.
            export: request => AdmitSaf(validator.ValidateForExport(request.Model, request.TargetVersion, request.Model.OriginalVersion ?? request.TargetVersion), key)
                .Bind(model => Try.lift(() => exports.Export(request.Workbook, model, request.TargetVersion, model.OriginalVersion ?? request.TargetVersion)).Run()
                    .MapFail(error => new BimFault.CodecReject(key, $"saf-export:{error.Message}")))
                .Bind(result => result.IsSuccess
                    ? Fin.Succ(result.Model)
                    : Fin.Fail<ExcelModel>(new BimFault.ModelRejected(key, $"saf-export:{ExcelValidationResult.Format(result.ValidationResults)}"))));

    private static Fin<ExcelModel> AdmitSaf(ExcelModel model, Op key) =>
        model.ValidationErrors.Any(static error => error.Severity == ExcelValidationMessageSeverity.Error)
            ? Fin.Fail<ExcelModel>(new BimFault.ModelRejected(key, $"saf-validation:{ExcelValidationResult.Format(model.ValidationErrors)}"))
            : Fin.Succ(model);

    private static readonly Seq<string> LoadKinds = Seq(
        "IfcStructuralLoadSingleForce", "IfcStructuralLoadLinearForce", "IfcStructuralLoadPlanarForce",
        "IfcStructuralLoadTemperature", "IfcStructuralLoadSingleDisplacement", "IfcStructuralLoadConfiguration");

    // The Enumerated allowed-sets are DERIVED from the GeometryGym enums (IfcLoadGroupTypeEnum carries
    // LOAD_COMBINATION_GROUP beyond the obvious four; IfcAnalysisTheoryTypeEnum the first/second/third-order +
    // full-nonlinear ladder; IfcAnalysisModelTypeEnum the in-plane/out-plane/3D loading split) so no roster
    // comment or hand-listed subset ever drifts from the schema.
    private static readonly Seq<string> LoadGroupKinds = toSeq(Enum.GetNames<IfcLoadGroupTypeEnum>());
    private static readonly Seq<string> TheoryKinds = toSeq(Enum.GetNames<IfcAnalysisTheoryTypeEnum>());
    private static readonly Seq<string> ModelKinds = toSeq(Enum.GetNames<IfcAnalysisModelTypeEnum>());

    // The EN 1990 design-situation vocabulary derives from the package's own [Flags] enum, so the persistent /
    // transient / accidental / seismic ladder and its PersistentAndTransient pairing stay the package's to widen.
    private static readonly Seq<string> SituationKinds = toSeq(Enum.GetNames<DesignSituationClass>());

    // The SPECIFIC tier of the two-tier load-CASE derivation Rasm.Compute factors under its LoadCombinationSpec:
    // the named permanent/climatic/seismic IfcActionSourceTypeEnum sources resolve to a ROW carrying the consumer's
    // closed dead/live/snow/wind/seismic token, the EN 1990 ActionClass nature the combination algebra factors under,
    // the imposed category where the action is category-keyed, and the psi-factor mint. The permanent-nature sources
    // (completion, prestress, settlement, shrinkage, creep, imperfection, lack-of-fit) land dead + Permanent so they
    // factor as permanent actions; the token alone was the thin slice that made every consumer re-derive the nature.
    // A source with no row falls to the NATURE tier through Nature, so the residue (temperature, fire, impact, wave,
    // brakes, ...) is the imposed variable action rather than a silently mis-cased permanent, and USERDEFINED/
    // NOTDEFINED sources classify identically.
    private static readonly Map<IfcActionSourceTypeEnum, ActionRow> CaseSources = toMap(Seq(
        (IfcActionSourceTypeEnum.DEAD_LOAD_G,        new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.COMPLETION_G1,      new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.PRESTRESSING_P,     new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.SETTLEMENT_U,       new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.SHRINKAGE,          new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.CREEP,              new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.SYSTEM_IMPERFECTION, new ActionRow("dead",   ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.LACK_OF_FIT,        new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.SNOW_S,             new ActionRow("snow",    ActionClass.Variable,   None, Some(EurocodeAction.Snow))),
        (IfcActionSourceTypeEnum.WIND_W,             new ActionRow("wind",    ActionClass.Variable,   None, Some(EurocodeAction.Wind))),
        (IfcActionSourceTypeEnum.EARTHQUAKE_E,       new ActionRow("seismic", ActionClass.Accidental, None, Some(EurocodeAction.Seismic)))));

    // --- [ROW_NAMES] ---------------------------------------------------------------------------
    // Every row name the reader stamps resolves to an OWNER-declared static: the cross-package structural vocabulary
    // is the Rasm.Element StructuralRows roster (the Bim writer and the Rasm.Compute analysis reader are
    // non-referencing peers, so a literal at either end forks on the first rename), and every remaining row mints
    // through PropertyCategory.Seam.Row — the owner-blessed EMPTY-prefix category, which is what keeps a round-tripped
    // name bare while still routing custody through the seam declarer. A call-site PropertyName.Create anywhere in
    // this reader is the deleted form. A name a second package begins keying on is PROMOTED to StructuralRows at the
    // Element owner rather than re-declared here.
    private static readonly PropertyName LoadType = PropertyCategory.Seam.Row("LoadType");
    private static readonly PropertyName GlobalOrLocal = PropertyCategory.Seam.Row("GlobalOrLocal");
    private static readonly PropertyName Source = PropertyCategory.Seam.Row("Source");
    private static readonly PropertyName ActionClassRow = PropertyCategory.Seam.Row("ActionClass");
    private static readonly PropertyName ImposedCategory = PropertyCategory.Seam.Row("ImposedCategory");
    private static readonly PropertyName LoadGroupType = PropertyCategory.Seam.Row("LoadGroupType");
    private static readonly PropertyName ActionType = PropertyCategory.Seam.Row("ActionType");
    private static readonly PropertyName ActionSource = PropertyCategory.Seam.Row("ActionSource");
    private static readonly PropertyName Coefficient = PropertyCategory.Seam.Row("Coefficient");
    private static readonly PropertyName Purpose = PropertyCategory.Seam.Row("Purpose");
    private static readonly PropertyName AnalysisTheory = PropertyCategory.Seam.Row("AnalysisTheory");
    private static readonly PropertyName IsLinear = PropertyCategory.Seam.Row("IsLinear");
    private static readonly PropertyName ResultFor = PropertyCategory.Seam.Row("ResultFor");
    private static readonly PropertyName ModelType = PropertyCategory.Seam.Row("ModelType");
    private static readonly PropertyName LoadedBy = PropertyCategory.Seam.Row("LoadedBy");
    private static readonly PropertyName HasResults = PropertyCategory.Seam.Row("HasResults");
    private static readonly PropertyName Thickness = PropertyCategory.Seam.Row("Thickness");

    // The RELEASE row family: a rel-level IfcRelConnectsStructuralMember.AppliedCondition is the member END RELEASE
    // and a connection's own AppliedCondition is the joint SUPPORT — two physically different facts a `??` fallback
    // fused onto one row family, so a released beam end read as a support and a supported joint read as a release.
    // The support family is the Element roster's Translation/Rotation; the release family mints beside it off the SAME
    // StructuralRows.Axes roster and both land on ONE bag, so Rasm.Compute reads a member end and its joint separately.
    private static readonly Map<string, PropertyName> ReleaseTranslation = Family("ReleaseTranslation");
    private static readonly Map<string, PropertyName> ReleaseRotation = Family("ReleaseRotation");

    // The seventh degree of freedom an IfcBoundaryNodeConditionWarping carries. Its select is SEALED at both ends —
    // mWarpingStiffness on the condition and mFixed/mStiffness on IfcWarpingStiffnessSelect are internal fields with
    // no public accessor — so the READ binds through the ONE Semantics/appearance#... IfcInternals [UnsafeAccessor]
    // capsule [SEALED_PAYLOAD_RULING]. The SUPPORT row is the Element roster's OWN StructuralRows.Warping["Axial"] —
    // the promoted cross-package seventh-DOF family, so no page-local "Warping" re-mint forks the spelling the
    // Rasm.Compute reader keys on — and only the RELEASE counterpart mints here, off the SAME WarpingKeys roster the
    // Element family closes over. The row stays OUT of the Author CONSUME set below (NOT out of StructuralRows.Dofs,
    // which carries WarpingAxial), which is what carries it to the egress fidelity receipt as StructuralResidue:
    // the select's every constructor is internal, so no public authoring path exists and a fabricated re-author
    // would emit a warping stiffness the file never declared.
    private static readonly Map<string, PropertyName> ReleaseWarping = Family("ReleaseWarping", Some(StructuralRows.WarpingKeys));

    // The whole release roster — the Workbook releases filter probes it, so a joint carrying ANY release family row
    // (the reducer stamps a family whole, but the probe states the full domain rather than one axis's membership)
    // lowers onto the SAF hinge row.
    private static readonly Seq<PropertyName> ReleaseDofs =
        ReleaseTranslation.Values.ToSeq() + ReleaseRotation.Values.ToSeq() + ReleaseWarping.Values.ToSeq();

    // The eccentricity the IfcRelConnectsWithEccentricity subtype adds to its base connection: the content key the
    // IIfcProfileStore preserved the ConnectionConstraint geometry fragment under, never inlined coordinates [M2].
    // ASSEMBLY-INTERNAL rather than private because the Projection/egress#IFC_EGRESS re-author reads this row back to
    // restore the constraint geometry — a mirrored literal at that end forks the key space on the first rename, which
    // is the same custody law the cross-package StructuralRows roster holds one tier out.
    internal static readonly PropertyName Eccentricity = PropertyCategory.Seam.Row("Eccentricity");

    // The EN 1990 combination roster a LOAD_COMBINATION_GROUP lowers: the package-generated combination expressions
    // beside the factored design-action stream they produce, in one combination order both rows share.
    private static readonly PropertyName Combinations = PropertyCategory.Seam.Row("Combinations");
    private static readonly PropertyName FactoredActions = PropertyCategory.Seam.Row("FactoredActions");

    // The span a varying line action occupies, read off its own Locations roster — the distance-along of the kept
    // first and last ramp values. A trapezoid whose extent no row carries reads as a full-length action.
    private static readonly PropertyName SpanStart = PropertyCategory.Seam.Row("SpanStart");
    private static readonly PropertyName SpanEnd = PropertyCategory.Seam.Row("SpanEnd");

    // The two axis families no cross-package reader keys on — the self-weight vector a load case declares and the
    // member's own local axis ratios — minted off the SAME StructuralRows.Axes roster so a seventh axis is unreachable
    // by typo. Every family a Rasm.Compute reader probes (Force/Moment/PlanarForce/Start/End/DeltaT) is the Element
    // roster's own static, never re-minted here.
    private static readonly Map<string, PropertyName> SelfWeight = Family("SelfWeight");
    private static readonly Map<string, PropertyName> LocalAxis = Family("LocalAxis");

    // The EN 1990 factor rows: the three combination factors off the action's own minted case and the elected design
    // situation's whole partial-factor set, each a dimensionless Measure beside Coefficient so one consumer read
    // covers every factor on the bag. GammaQ1 is Option-valued at the contract (the leading-action sweep leaves it
    // unstated), so its row is absent rather than defaulted.
    private static readonly Seq<PropertyName> PsiRows = Seq(
        PropertyCategory.Seam.Row("Psi0"), PropertyCategory.Seam.Row("Psi1"), PropertyCategory.Seam.Row("Psi2"));
    private static readonly PropertyName GammaSup = PropertyCategory.Seam.Row("GammaSup");
    private static readonly PropertyName GammaInf = PropertyCategory.Seam.Row("GammaInf");
    private static readonly PropertyName GammaQ1 = PropertyCategory.Seam.Row("GammaQ1");
    private static readonly PropertyName GammaQi = PropertyCategory.Seam.Row("GammaQi");
    private static readonly PropertyName Xi = PropertyCategory.Seam.Row("Xi");
    private static readonly PropertyName Situation = PropertyCategory.Seam.Row("DesignSituation");

    // Mirrors the Element roster's own Family mint: the key roster is the whole discriminant, Axes the canonical
    // default, and a family keyed on its own shape (the release warping's WarpingKeys) hands its roster in.
    private static Map<string, PropertyName> Family(string stem, Option<Seq<string>> keys = default) =>
        keys.IfNone(StructuralRows.Axes).Fold(Map<string, PropertyName>(), (map, axis) => map.Add(axis, PropertyCategory.Seam.Row($"{stem}{axis}")));

    // --- [ATTRIBUTES] -------------------------------------------------------------------------
    // ONE polymorphic structural attribute-bag reader discriminating on the entity shape — never a RestraintAttrs/
    // LoadAttrs/GroupAttrs sibling family. The two IfcRelConnects* arms build the WHOLE Generic edge payload in one
    // call (restraint + frame + supported length + AtStart; load + Station) so EdgeProjection.Structural reads
    // Attrs(rel, scale, eurocode, profiles, key) once; a connection's/activity's own bag serves the entity-level enrichment; a load-group /
    // load-case / result-group / analysis-model / member definition rides a structural PropertySet node. A
    // non-structural or null entity yields the empty bag (a graceful skip). Both regimes arrive as ARGUMENTS —
    // the projector's one UnitScale and the composition's elected EurocodePolicy — so no arm rebuilds a regime off
    // its own entity's Database and none falls back to UnitScale.Si. The Measures gate Filters every non-finite
    // magnitude, so the surfaces whose public getter exposes the unset NaN sentinel never emit: DeltaT_* (raw
    // auto-property), Coefficient, Thickness, SupportedLength, and a 2D direction's DirectionRatioZ all read NaN
    // unset and drop there. The IfcStructuralLoad force families (Single/Linear/Planar) are NOT in that set — the
    // NaN backing field's public getter COERCES unset NaN -> 0.0, so an unset force component reads a deliberate 0
    // (a zero force, harmless to the FE consumer) no Filter can distinguish from a real 0. The restraint DOF rows
    // ride NEITHER path: a DOF carries a verdict, so a non-finite stiffness FAULTS at SixDof rather than dropping
    // a row nobody keys on or asserting a free DOF the reading never established.
    public static Fin<Map<PropertyName, PropertyValue>> Attrs(
        BaseClassIfc? entity, UnitScale scale, Option<EurocodePolicy> eurocode, IIfcProfileStore profiles, Op key) =>
        entity switch {
            // The rel bag carries BOTH restraint facts as two row families: the rel's own AppliedCondition is the
            // member END RELEASE and the related connection's is the joint SUPPORT. The eccentricity rides the
            // subtype: IfcRelConnectsWithEccentricity.ConnectionConstraint is a mandatory IfcConnectionGeometry, so it
            // content-keys through the ONE IIfcProfileStore fragment lane [M2] and the row carries that key — never
            // inlined coordinates, and never the silent degrade to the base binding the egress counts as
            // EccentricityDegraded when the store misses.
            IfcRelConnectsStructuralMember relation =>
                from release in RestraintOf(relation.AppliedCondition, Release, scale, key)
                from support in RestraintOf(relation.RelatedStructuralConnection?.AppliedCondition, Support, scale, key)
                from frame in Frame(relation.ConditionCoordinateSystem, key)
                from length in Measures(Seq((StructuralRows.SupportedLength, Named<IfcLengthMeasure>(), relation.SupportedLength)), scale, key)
                select release.AddRange(support).AddRange(frame).AddRange(length)
                    .AddRange(Optional((relation as IfcRelConnectsWithEccentricity)?.ConnectionConstraint)
                        .Map(constraint => (Eccentricity, (PropertyValue)new PropertyValue.Text(profiles.Preserve(constraint, key).ToString("X32"))))
                        .ToSeq())
                    .AddRange(AtStart(relation.RelatingStructuralMember as IfcStructuralCurveMember, relation.RelatedStructuralConnection)
                        .Map(static atStart => (StructuralRows.AtStart, (PropertyValue)new PropertyValue.Boolean(atStart))).ToSeq()),
            IfcRelConnectsStructuralActivity relation =>
                from load in LoadOf(relation.RelatedStructuralActivity, scale, eurocode, key)
                from station in Measures(Station(relation.RelatingElement as IfcStructuralCurveMember, relation.RelatedStructuralActivity)
                    .Map(static value => (StructuralRows.Station, Anonymous, value)).ToSeq(), scale, key)
                select load.AddRange(station),
            IfcStructuralConnection connection =>
                from restraint in RestraintOf(connection.AppliedCondition, Support, scale, key)
                from frame in Frame((connection as IfcStructuralPointConnection)?.ConditionCoordinateSystem, key)
                select restraint.AddRange(frame),
            IfcStructuralActivity activity => LoadOf(activity, scale, eurocode, key),
            IfcStructuralLoadCase loadCase =>
                from group in GroupOf(loadCase, scale, eurocode, key)
                from weight in Measures(Optional(loadCase.SelfWeightCoefficients).ToSeq().Bind(static vector => Seq(
                    (SelfWeight["X"], Named<IfcRatioMeasure>(), vector.Item1),
                    (SelfWeight["Y"], Named<IfcRatioMeasure>(), vector.Item2),
                    (SelfWeight["Z"], Named<IfcRatioMeasure>(), vector.Item3))), scale, key)
                select group.AddRange(weight),
            IfcStructuralLoadGroup group => GroupOf(group, scale, eurocode, key),
            IfcStructuralResultGroup result => Fin.Succ(Map(
                (AnalysisTheory, Enumerated(result.TheoryType.ToString(), TheoryKinds)),
                (IsLinear, (PropertyValue)new PropertyValue.Boolean(result.IsLinear)))
                .AddRange(Optional(result.ResultForLoadGroup)
                    .Map(static loadGroup => (ResultFor, (PropertyValue)new PropertyValue.Text(loadGroup.GlobalId))).ToSeq())),
            IfcStructuralAnalysisModel model =>
                from frame in Frame(model.OrientationOf2DPlane, key)
                select Seq(
                        (LoadedBy, toSeq(model.LoadedBy).Map(static group => group.GlobalId)),
                        (HasResults, toSeq(model.HasResults).Map(static result => result.GlobalId)))
                    .Filter(static join => !join.Item2.IsEmpty)
                    .Fold(Map((ModelType, Enumerated(model.PredefinedType.ToString(), ModelKinds))),
                        static (map, join) => map.Add(join.Item1,
                            new PropertyValue.List(join.Item2.Map(static id => (PropertyValue)new PropertyValue.Text(id)))))
                    .AddRange(frame),
            IfcStructuralCurveMember member => Measures(Optional(member.Axis).ToSeq().Bind(static axis => Seq(
                (LocalAxis["X"], Anonymous, axis.DirectionRatioX),
                (LocalAxis["Y"], Anonymous, axis.DirectionRatioY),
                (LocalAxis["Z"], Anonymous, axis.DirectionRatioZ))), scale, key),
            IfcStructuralSurfaceMember surface => Measures(
                Seq((Thickness, Named<IfcPositiveLengthMeasure>(), surface.Thickness)), scale, key),
            _ => Fin.Succ(Map<PropertyName, PropertyValue>()),
        };

    // --- [RESTRAINT] -------------------------------------------------------------------------- The
    // six-DOF support condition: a fixity Boolean PLUS the SI spring-stiffness magnitude per DOF, so
    // Rasm.Compute reads BOTH a pinned/fixed support and a finite spring off the edge (the prior boolean-only
    // reader dropped the stiffness). A node condition reads its 6 stiffness selects, an edge condition its 6
    // by-length selects; the four select types each expose a Rigid/Stiffness shape (no shared base — see Dof) so
    // the per-DOF type switch reduces every DOF. Takes the CONDITION (not the connection) so the rel-level
    // member-end release and the connection's own support reduce through one reader. An absent condition — or a
    // face condition, whose area-stiffness GeometryGym exposes ONLY as internal fields (no public properties) —
    // yields the empty (free) bag.
    // The measure type of each arm is the select's OWN declaration, never a signature this reader picks: a node
    // condition reads StiffnessSelect<IfcLinearStiffnessMeasure> and the standalone IfcRotationalStiffnessSelect whose
    // Stiffness property IS an IfcRotationalStiffnessMeasure, an edge condition the two subgrade-reaction selects
    // closing over IfcModulusOfLinear-/IfcModulusOfRotationalSubgradeReactionMeasure — three reaction types one
    // exponent apart, so the edge pair reading the node pair's names would price every edge spring by a length factor.
    // Which physical fact the condition states, as a POLICY VALUE carrying its own row families: a rel-level
    // AppliedCondition is the member END RELEASE and a connection's own AppliedCondition is the joint SUPPORT. The
    // retired `rel.AppliedCondition ?? connection.AppliedCondition` fallback FUSED the two onto one family, so a
    // released beam end read downstream as a support and a supported joint with no declared release read as a fully
    // free end — both silent. One reducer serves both families and one bag carries both without collision.
    private readonly record struct RestraintFamily(
        Map<string, PropertyName> Translation, Map<string, PropertyName> Rotation, PropertyName Warping);

    private static readonly RestraintFamily Support = new(StructuralRows.Translation, StructuralRows.Rotation, StructuralRows.Warping["Axial"]);
    private static readonly RestraintFamily Release = new(ReleaseTranslation, ReleaseRotation, ReleaseWarping["Axial"]);

    private static Fin<Map<PropertyName, PropertyValue>> RestraintOf(
        IfcBoundaryCondition? condition, RestraintFamily family, UnitScale scale, Op key) => condition switch {
        // The warping subtype precedes its own base: IfcBoundaryNodeConditionWarping IS an IfcBoundaryNodeCondition,
        // so the base arm ordered first would swallow every seventh degree of freedom.
        IfcBoundaryNodeConditionWarping w => SixDof(
                (w.TranslationalStiffnessX, w.TranslationalStiffnessY, w.TranslationalStiffnessZ),
                (w.RotationalStiffnessX, w.RotationalStiffnessY, w.RotationalStiffnessZ),
                Named<IfcLinearStiffnessMeasure>(), Named<IfcRotationalStiffnessMeasure>(), family, scale, key)
            .Bind(rows => WarpingOf(w, family, scale, key).Map(rows.AddRange)),
        IfcBoundaryNodeCondition n => SixDof(
            (n.TranslationalStiffnessX, n.TranslationalStiffnessY, n.TranslationalStiffnessZ),
            (n.RotationalStiffnessX, n.RotationalStiffnessY, n.RotationalStiffnessZ),
            Named<IfcLinearStiffnessMeasure>(), Named<IfcRotationalStiffnessMeasure>(), family, scale, key),
        IfcBoundaryEdgeCondition e => SixDof(
            (e.LinearStiffnessByLengthX, e.LinearStiffnessByLengthY, e.LinearStiffnessByLengthZ),
            (e.RotationalStiffnessByLengthX, e.RotationalStiffnessByLengthY, e.RotationalStiffnessByLengthZ),
            Named<IfcModulusOfLinearSubgradeReactionMeasure>(), Named<IfcModulusOfRotationalSubgradeReactionMeasure>(), family, scale, key),
        _ => Fin.Succ(Map<PropertyName, PropertyValue>()),
    };

    // The SEVENTH degree of freedom, read through the ONE IfcInternals [UnsafeAccessor] capsule
    // [SEALED_PAYLOAD_RULING]: IfcBoundaryNodeConditionWarping.mWarpingStiffness and IfcWarpingStiffnessSelect's own
    // mFixed/mStiffness are all internal fields behind no public accessor, so the capsule projects the DETACHED
    // (rigid, native) pair and a second accessor spelled here would fork its version pin. The reading takes the SAME
    // three-way verdict every other DOF takes onto the family's own warping row — the Element roster's
    // Warping["Axial"] on the support side. The row rides OUTSIDE the Author CONSUME set deliberately: every
    // IfcWarpingStiffnessSelect constructor is internal too, so no public authoring path exists and Author leaves the
    // row unconsumed, which is what carries it to the egress fidelity receipt as StructuralResidue instead of
    // re-authoring a warping stiffness the file never declared.
    private static Fin<Map<PropertyName, PropertyValue>> WarpingOf(
        IfcBoundaryNodeConditionWarping condition, RestraintFamily family, UnitScale scale, Op key) =>
        IfcInternals.Warping(condition).Match(
            None: static () => Fin.Succ(Map<PropertyName, PropertyValue>()),
            Some: reading => Verdict(family.Warping, reading, Named<IfcWarpingMomentMeasure>(), scale, key)
                .Map(static row => Map((row.Name, row.Value))));

    // The local orientation frame as ONE StructuralRows.Frame row carrying the six Axis/RefDirection direction ratios
    // in declared order (AxisX, AxisY, AxisZ, RefX, RefY, RefZ) — the owner-declared frame row both the connection's
    // skewed ConditionCoordinateSystem (an inclined roller's DOF axes) and the analysis model's OrientationOf2DPlane
    // land on, so the retired prefix-parameterized RestraintAxisX/PlaneRefZ name family is gone with the string-built
    // spellings it forced. The two frames never co-occupy one bag (a restraint frame rides a connection or rel bag, a
    // plane frame the analysis-model bag), so one row carries either. A global-axes placement (absent, or partial)
    // emits nothing rather than a fabricated frame, a non-finite ratio drops the WHOLE row rather than poisoning a
    // positional list, and the ratios are attribute data, never the content-keyed analytical geometry.
    private static Fin<Map<PropertyName, PropertyValue>> Frame(IfcAxis2Placement3D? system, Op key) =>
        system is { Axis: { } axis, RefDirection: { } reference }
        && Seq(axis.DirectionRatioX, axis.DirectionRatioY, axis.DirectionRatioZ,
               reference.DirectionRatioX, reference.DirectionRatioY, reference.DirectionRatioZ) is var ratios
        && ratios.ForAll(double.IsFinite)
            ? ratios.TraverseM(ratio => PropertyValue.Of(new PropertyValue.Number(ratio), key)).As()
                .Map(values => Map((StructuralRows.Frame, (PropertyValue)new PropertyValue.List(values))))
            : Fin.Succ(Map<PropertyName, PropertyValue>());

    // ONE row per degree of freedom, its PropertyValue CASE carrying whether the support is a rigid restraint or a
    // finite spring — the seam's own custody law, so a reader keying the DOF never has to know a parallel Kx roster
    // exists and can never read the boolean while stranding the magnitude. A rigid or free DOF stamps Boolean, a DOF
    // carrying a positive stiffness stamps the SI Measure under the select's OWN stiffness measure type, so an
    // exported spring re-authors as the reaction it was read as; the retired TranslationX + TranslationKx pair was the
    // twin that split one fact across two rows. The magnitude arrives NATIVE and coerces inside the one Admit entry,
    // which is also where a NON-FINITE spring meets the MeasureValue.OfSi finite gate and re-keys
    // ElementFault.ValueRejected — a DOF row is a VERDICT, so a malformed stiffness cannot
    // lower onto Boolean(false), which asserts a free DOF the reading never established and which no downstream
    // Filter retracts, because a Boolean row is never dropped.
    private static Fin<Map<PropertyName, PropertyValue>> SixDof(
        (object? X, object? Y, object? Z) translation, (object? X, object? Y, object? Z) rotation,
        Option<string> translationMeasure, Option<string> rotationMeasure, RestraintFamily family, UnitScale scale, Op key) =>
        Seq((family.Translation["X"], translation.X, translationMeasure),
            (family.Translation["Y"], translation.Y, translationMeasure),
            (family.Translation["Z"], translation.Z, translationMeasure),
            (family.Rotation["X"],    rotation.X,    rotationMeasure),
            (family.Rotation["Y"],    rotation.Y,    rotationMeasure),
            (family.Rotation["Z"],    rotation.Z,    rotationMeasure))
            .TraverseM(degree => Verdict(degree.Item1, Dof(degree.Item2), degree.Item3, scale, key))
            .As()
            .Map(static rows => rows.Fold(Map<PropertyName, PropertyValue>(), static (map, row) => map.Add(row.Name, row.Value)));

    // ONE degree of freedom's verdict, THREE-WAY over the magnitude the select yielded, because the row is an
    // ASSERTION about the support and every branch states something the reading established: a ZERO magnitude is the
    // fixity Boolean (rigid where the select declared rigid, free otherwise), a POSITIVE magnitude is the SI spring
    // Measure under the select's own stiffness measure type, and a NEGATIVE-finite or non-finite magnitude is the
    // typed ValueRejected fault. The retired two-arm shape routed a negative-finite stiffness to the Boolean branch,
    // where `rigid || native > 0` reads false — fabricating a FREE degree of freedom out of a malformed magnitude,
    // a positive claim no reading establishes and one no downstream filter retracts, because a Boolean row is never
    // dropped. NaN falls to the same fault arm: a non-finite spring is malformed, never free.
    private static Fin<(PropertyName Name, PropertyValue Value)> Verdict(
        PropertyName name, (bool Fixity, double Native) reading, Option<string> measure, UnitScale scale, Op key) =>
        reading.Native switch {
            0d   => Fin.Succ((Name: name, Value: (PropertyValue)new PropertyValue.Boolean(reading.Fixity))),
            > 0d => Admit(name, measure, reading.Native, scale, key)
                        .Map(value => (Name: name, Value: (PropertyValue)new PropertyValue.Measure(value))),
            _    => ElementFault.ValueRejected(key, $"<structural-stiffness:{name}:{reading.Native:R}>"),
        };

    // ONE reading per DOF select: the fixity Boolean AND the SI spring magnitude from one four-arm type switch over
    // GeometryGym's SPLIT select hierarchy (IfcTranslationalStiffnessSelect + the two subgrade-reaction selects derive
    // StiffnessSelect<TMeasure>; IfcRotationalStiffnessSelect is standalone) — no common base unifies them, so a single
    // property pattern is impossible, but all four independently expose a Rigid Boolean + a Stiffness measure whose
    // .Measure rides IfcDerivedMeasureValue, so the prior Fixity/SpringOf split that pattern-matched every DOF twice
    // collapses to one reader. A DOF is fixed when rigid OR carrying a finite positive spring, and the magnitude is 0
    // for a rigid or free DOF and the model-NATIVE spring otherwise — the reading stays NATIVE the whole way, since
    // the coercion factor is positive and cannot move a sign or a zero, so the fixity verdict is the same on either
    // side of it and the ONE Admit entry owns the coercion. A malformed select carries its non-finite magnitude
    // forward UNTOUCHED — the caller's finite gate owns the verdict, so this reading never launders a NaN into a
    // fixity Boolean.
    private static (bool Fixity, double Native) Dof(object? select) {
        (bool Rigid, double Native) reading = select switch {
            IfcTranslationalStiffnessSelect s                 => (s.Rigid, s.Stiffness?.Measure ?? 0d),
            IfcRotationalStiffnessSelect s                    => (s.Rigid, s.Stiffness?.Measure ?? 0d),
            IfcModulusOfTranslationalSubgradeReactionSelect s => (s.Rigid, s.Stiffness?.Measure ?? 0d),
            IfcModulusOfRotationalSubgradeReactionSelect s    => (s.Rigid, s.Stiffness?.Measure ?? 0d),
            _                                                 => (false, 0d),
        };
        return (reading.Rigid || reading.Native > 0d, reading.Rigid ? 0d : reading.Native);
    }

    // The measure identity ONE row admits under, spelled as the GG IfcValue TYPE its source attribute declares. The
    // generic bound makes a non-value type uncompilable and typeof(T).Name IS the key
    // Projection/semantic#SEMANTIC_PROJECTOR PropertyLowering.MeasureDimensions stores (the property lane keys on
    // value.GetType().Name), so the seam Dimension, the coercion axis, the stamped QuantityType, and the
    // Projection/egress#IFC_EGRESS MeasureMints raiser all resolve from ONE symbol and a literal spelling is
    // unreachable. Anonymous is its counterpart — the row NO IFC measure type names: a coefficient this reader
    // computes (the normalized Station, every EN 1990 factor) or a source attribute the schema declares a bare
    // IfcReal (the direction ratios). Naming one anyway forges a round-trip identity the file never carried.
    private static Option<string> Named<TMeasure>() where TMeasure : IfcValue => Some(typeof(TMeasure).Name);

    private static readonly Option<string> Anonymous = None;

    // The whole admission triple a structural magnitude resolves to — signature, identity, SI magnitude — in ONE
    // read of the name, so the mint below cannot elect a different identity than the one that signed the row. A NAMED
    // measure resolves the frozen MeasureDimensions row, the ONE table that also signs the property lane and derives
    // the egress raiser, so ingress dimension, unit coercion, and egress spelling are one decision; an anonymous row
    // is dimensionless by construction and the coercion factor its empty exponent vector gives is the identity, so
    // the native magnitude IS the SI one. A named type the table does not carry signs NO dimension here and rails
    // rather than coercing on a guessed exponent vector, wrong by a power of the model's own length factor.
    private static Fin<(Dimension Signature, Option<QuantityType> Type, double Si)> Resolve(
        Option<string> measure, double native, UnitScale scale, Op key) =>
        measure.Match(
            Some: type => PropertyLowering.MeasureDimensions.TryGetValue(type, out MeasureRow row)
                ? Fin.Succ((Signature: row.Dimension, Type: Some(QuantityType.Create(type)), Si: scale.Coerce(native, row, null)))
                : ElementFault.ValueRejected(key, $"<structural-measure-unrostered:{type}>"),
            None: static () => Fin.Succ((Signature: Dimension.Dimensionless, Type: Option<QuantityType>.None, Si: native)));

    // The ONE structural magnitude admission both the DOF springs and every attribute row cross — the MIXED mint the
    // seam's round-trip law rules: a resolved identity stamps its QuantityType, so an IfcLinearForceMeasure re-exports
    // as ITSELF through the raiser keyed on the very same name, while an unresolved one takes the dimension-only mint
    // and stays dimension-anonymous. Both cross the same MeasureValue.OfSi finite gate and re-key one
    // ElementFault.ValueRejected spelling, so a DOF verdict and a dropped attribute fault read alike.
    private static Fin<MeasureValue> Admit(PropertyName name, Option<string> measure, double native, UnitScale scale, Op key) =>
        Resolve(measure, native, scale, key).Bind(resolved => resolved.Type
            .Match(
                Some: type => MeasureValue.OfSi(type, resolved.Signature, resolved.Si),
                None: () => MeasureValue.OfSi(resolved.Signature, resolved.Si))
            .MapFail(_ => ElementFault.ValueRejected(key, $"<structural-measure:{name}:{native:R}>")));

    private static Fin<Map<PropertyName, PropertyValue>> Measures(
        Seq<(PropertyName Name, Option<string> Measure, double Native)> rows,
        UnitScale scale, Op key) =>
        rows.Filter(static row => double.IsFinite(row.Native))
            .TraverseM(row => Admit(row.Name, row.Measure, row.Native, scale, key)
                .Map(value => (Name: row.Name, Value: (PropertyValue)new PropertyValue.Measure(value))))
            .As()
            .Map(static admitted => admitted.Fold(
                Map<PropertyName, PropertyValue>(),
                static (map, row) => map.Add(row.Name, row.Value)));

    private static PropertyValue Enumerated(string selected, Seq<string> allowed) =>
        new PropertyValue.Enumerated(
            Seq<PropertyValue>(new PropertyValue.Text(selected)),
            allowed.Map(static value => (PropertyValue)new PropertyValue.Text(value)));

    // The reader's inverse the Projection/egress#IFC_EGRESS Emit composes over the authored structural entities:
    // the node-level AppliedCondition (6-DOF fixity + SI springs) and the AppliedLoad single-force components
    // re-stamp off the StructuralDefinition bag the ingest Attrs lowered — the [RELATIONSHIP_REEMIT] named drop
    // this closes. The family discriminant is the ingest's own LoadType token — the ForceX..Z wire names are
    // family-SHARED across the 1D loads, so a token-blind ForceX gate would re-author every uniform line action
    // as a fabricated point force. The egress target database is SI by construction, so the bag's SI magnitudes
    // land verbatim — no inverse UnitScale fold exists to get wrong.
    // TYPE-CLOSED at both ends and by two independent routes: a consumed row re-enters its GG entity slot, whose own
    // declaration re-mints the ingest-stamped measure (IfcTranslationalStiffnessSelect's double ctor builds an
    // IfcLinearStiffnessMeasure, IfcStructuralLoadSingleForce's slots are the schema's IfcForceMeasure/IfcTorqueMeasure),
    // while an UNCONSUMED row reaches the Projection/egress#IFC_EGRESS bag lane, where RaiseMeasure keys its typed mint
    // on the very QuantityType this reader stamped. The two routes agree because both resolve the same measure name.
    // TOTAL and residue-HONEST: the RETURN IS the residue — the row names the re-stamp did NOT consume, on the same
    // Fin rail the ingest rides — so a payload with no verified re-author ctor (a line/planar/temperature action, a
    // trapezoid configuration, a displacement) reaches Emit as a value it must FOLD into the exchange fidelity
    // receipt, never a bag an egress arm can drop on the floor behind a total void surface. The GG ctor is the one
    // throwing seam, and it crosses as BimFault.CodecReject rather than escaping a re-author fold.
    public static Fin<Seq<PropertyName>> Author(
        DatabaseIfc db, IfcObjectDefinition entity, Map<PropertyName, PropertyValue> attrs, Op key) =>
        entity switch {
            IfcStructuralConnection connection when attrs.ContainsKey(StructuralRows.Translation["X"]) =>
                Consume(attrs, SupportNames, key, () => connection.AppliedCondition = new IfcBoundaryNodeCondition(db, "",
                    Translational(attrs, StructuralRows.Translation["X"]),
                    Translational(attrs, StructuralRows.Translation["Y"]),
                    Translational(attrs, StructuralRows.Translation["Z"]),
                    Rotational(attrs, StructuralRows.Rotation["X"]),
                    Rotational(attrs, StructuralRows.Rotation["Y"]),
                    Rotational(attrs, StructuralRows.Rotation["Z"]))),
            IfcStructuralActivity activity when LoadTypeOf(attrs) == nameof(IfcStructuralLoadSingleForce) =>
                Consume(attrs, ForceNames, key, () => activity.AppliedLoad = new IfcStructuralLoadSingleForce(db,
                        Si(attrs, StructuralRows.Force["X"]), Si(attrs, StructuralRows.Force["Y"]), Si(attrs, StructuralRows.Force["Z"])) {
                    MomentX = Si(attrs, StructuralRows.Moment["X"]),
                    MomentY = Si(attrs, StructuralRows.Moment["Y"]),
                    MomentZ = Si(attrs, StructuralRows.Moment["Z"]),
                }),
            _ => Fin.Succ(attrs.Keys.ToSeq()),
        };

    // Consumed names = the stamped components plus the family discriminant; the frame tokens (LoadKind/Case/
    // ActionClass/GlobalOrLocal/Source) re-derive at the next ingest and never count as drops, and the Eurocode factors
    // re-resolve from the annex policy rather than round-tripping. SupportNames is the SIX Translation/Rotation
    // support rows the IfcBoundaryNodeCondition ctor re-authors — deliberately NOT StructuralRows.Dofs, whose
    // Element declaration INCLUDES the Warping family: consuming Dofs would strike the un-re-authorable
    // WarpingAxial row from the residue while authoring nothing for it, the exact silent drop the receipt exists
    // to prevent — and the release family, the warping row, the eccentricity key, and the combination roster are
    // all deliberately OUTSIDE it: each re-authors on a relationship or through a sealed constructor this
    // entity-keyed entry cannot reach, so each survives as residue the egress folds into its fidelity receipt
    // rather than as a silent drop. The stamp Action is the GG-authoring mutation seam, confined here.
    private static readonly Seq<PropertyName> SupportNames =
        StructuralRows.Translation.Values.ToSeq() + StructuralRows.Rotation.Values.ToSeq();

    private static readonly Seq<PropertyName> ForceNames =
        StructuralRows.Force.Values.ToSeq() + StructuralRows.Moment.Values.ToSeq() + Seq(LoadType);

    private static Fin<Seq<PropertyName>> Consume(
        Map<PropertyName, PropertyValue> attrs, Seq<PropertyName> names, Op key, Action stamp) =>
        Try.lift(() => {
                stamp();
                return names.Fold(attrs, static (residue, name) => residue.Remove(name)).Keys.ToSeq();
            }).Run()
            .MapFail(error => new BimFault.CodecReject(key, $"structural-reauthor:{error.Message}"));

    private static string LoadTypeOf(Map<PropertyName, PropertyValue> attrs) =>
        attrs.Find(LoadType)
            .Bind(static value => value is PropertyValue.Enumerated enumerated ? enumerated.Selected.Head : None)
            .Bind(static selected => selected is PropertyValue.Text text ? Some(text.Value) : None)
            .IfNone("");

    // A DOF select off the ONE bag row — the Dof reading's inverse over the collapsed shape: a Measure row re-stamps
    // its SI stiffness through the double ctor, a true Boolean row re-stamps rigid, and an absent or false row is free.
    private static IfcTranslationalStiffnessSelect Translational(Map<PropertyName, PropertyValue> attrs, PropertyName dof) =>
        Si(attrs, dof) is > 0d and var k ? new IfcTranslationalStiffnessSelect(k) : new IfcTranslationalStiffnessSelect(Fixity(attrs, dof));

    private static IfcRotationalStiffnessSelect Rotational(Map<PropertyName, PropertyValue> attrs, PropertyName dof) =>
        Si(attrs, dof) is > 0d and var k ? new IfcRotationalStiffnessSelect(k) : new IfcRotationalStiffnessSelect(Fixity(attrs, dof));

    private static double Si(Map<PropertyName, PropertyValue> attrs, PropertyName name) =>
        attrs.Find(name).Bind(static v => v is PropertyValue.Measure m ? Some(m.Value.Si) : None).IfNone(0d);

    private static bool Fixity(Map<PropertyName, PropertyValue> attrs, PropertyName name) =>
        attrs.Find(name).Exists(static v => v is PropertyValue.Boolean { Value: true });

    // --- [LOAD] ------------------------------------------------------------------------------- The
    // applied load the IfcRelConnectsStructuralActivity Generic edge carries: typed force/moment/pressure/
    // temperature MeasureValue components over the IfcStructuralLoad family PLUS the load-type token, the
    // global/local frame, and the source name (the prior single-force-only reader dropped the line/planar/
    // temperature families). IfcStructuralLoadSingleDisplacement, whose components are internal-field-only, yields
    // the frame attrs only, a graceful passthrough; a Temperature DeltaT_* unset = NaN drops at the Attrs egress,
    // while a force/moment component the public getter coerced to 0.0 emits a 0 (the getter masks the unset
    // sentinel, so the Filter NaN-guards the raw-sentinel surfaces and never suppresses a coerced 0). Every
    // component leaves Vectors model-NATIVE beside its own measure type and coerces inside the one Admit entry, so
    // this arm spells no factor and no dimension of its own.
    private static Fin<Map<PropertyName, PropertyValue>> LoadOf(IfcStructuralActivity? activity, UnitScale scale, Option<EurocodePolicy> eurocode, Op key) =>
        Optional(activity).Bind(static candidate => Optional(candidate.AppliedLoad).Map(load => (Activity: candidate, Load: load))).Match(
            Some: pair => {
                ActionRow row = RowOf(pair.Activity, eurocode);
                return from measures in Measures(Vectors(pair.Load), scale, key)
                       from factors in Factors(row, eurocode, scale, key)
                       select Map(
                           (LoadType, Enumerated(pair.Load.GetType().Name, LoadKinds)),
                           (StructuralRows.LoadKind, (PropertyValue)new PropertyValue.Text(KindOf(pair.Load))),
                           (StructuralRows.Case, new PropertyValue.Text(row.Case)),
                           (ActionClassRow, new PropertyValue.Text(row.Class.ToString())),
                           (GlobalOrLocal, new PropertyValue.Text(pair.Activity.GlobalOrLocal.ToString())),
                           (Source, new PropertyValue.Text(pair.Activity.Name ?? "")))
                           .AddRange(row.Imposed.Map(static category => (ImposedCategory, (PropertyValue)new PropertyValue.Text(category.ToString()))).ToSeq())
                           .AddRange(measures)
                           .AddRange(factors);
            },
            None: static () => Fin.Succ(Map<PropertyName, PropertyValue>()));

    // The Rasm.Compute FE idealization kind (point/uniform/trapezoid) the IfcStructuralLoad class lowers onto, stamped
    // ALONGSIDE the faithful IFC LoadType: a single force is a point action, a linear force a uniform line action, and
    // the IFC varying line action — IfcStructuralLoadConfiguration over its public `Values`/`Locations` —
    // is the trapezoid the Analysis/structural ToLoad Vec(g, "Start")/Vec(g, "End") probes read.
    private static string KindOf(IfcStructuralLoad load) => load switch {
        IfcStructuralLoadConfiguration => "trapezoid",
        IfcStructuralLoadLinearForce   => "uniform",
        _                              => "point",
    };

    // The two-tier neutral load-ACTION row walked off the activity's IfcStructuralLoadGroup assignment
    // (HasAssignments -> IfcRelAssignsToGroup.RelatingGroup -> IfcStructuralLoadGroup): the SPECIFIC CaseSources row
    // over ActionSource first, else the NATURE tier off the group's ActionType — so a prestress or shrinkage group
    // factors permanent and a temperature or impact group factors variable under the consumer's LoadCombinationSpec.
    // An ungrouped activity takes the same variable nature row, the unfactored case the consumer defaults.
    private static ActionRow RowOf(IfcStructuralActivity activity, Option<EurocodePolicy> eurocode) =>
        activity.HasAssignments.AsIterable()
            .Choose(static a => a is IfcRelAssignsToGroup { RelatingGroup: IfcStructuralLoadGroup g } ? Some(g) : None)
            .ToSeq().Head
            .Map(g => CaseSources.Find(g.ActionSource).IfNone(() => Nature(g.ActionType, eurocode)))
            .IfNone(() => Nature(IfcActionTypeEnum.VARIABLE_Q, eurocode));

    // The NATURE tier: a permanent group is the dead permanent action carrying no psi, and every other nature is the
    // IMPOSED variable action — which is where the project's own ImposedLoadCategory lands, because EN 1990 keys the
    // imposed psi set by category and the IFC source vocabulary carries no category of its own. Absent a policy the
    // row keeps its token and nature and carries no category and no mint, so the reader never invents one.
    private static ActionRow Nature(IfcActionTypeEnum nature, Option<EurocodePolicy> eurocode) =>
        nature == IfcActionTypeEnum.PERMANENT_G
            ? new ActionRow("dead", ActionClass.Permanent, None, None)
            : new ActionRow("live", ActionClass.Variable, eurocode.Bind(static policy => policy.Imposed),
                eurocode.Bind(static policy => policy.Imposed).Map(static _ => EurocodeAction.Imposed));

    // The EN 1990 Annex A1 factor rows: psi0/psi1/psi2 read off the case the action's own ENLoadCaseFactory mint
    // pre-loads (never a hand-tabulated psi set beside the package that owns the tables) and the whole partial-factor
    // set off the composition's elected design situation, all as dimensionless Measures beside Coefficient so one
    // consumer read covers every factor on the bag, with the situation CLASS riding an Enumerated row over the
    // package's own vocabulary. Only the psi mint reaches a table: the Annex A1.1 singletons throw for an uncovered
    // annex and the standards kernel throws MissingNationalAnnexException, so both cross into the Fin rail as
    // BimFault.CapabilityMiss at this one seam and never propagate into the fold; the partial factors are settled
    // property reads on an admitted policy value and cannot fault. An absent policy yields no rows at all.
    private static Fin<Map<PropertyName, PropertyValue>> Factors(ActionRow row, Option<EurocodePolicy> eurocode, UnitScale scale, Op key) =>
        eurocode.Match(
            None: static () => Fin.Succ(Map<PropertyName, PropertyValue>()),
            // The factor read mints the case over an EMPTY roster: the psi set is the package's own Table A1.1
            // column and carries no dependence on a payload, so one mint serves both this read and the combination
            // fold rather than a second factor-only entry beside it.
            Some: policy => Try.lift(() => row.Action.Bind(action => action.Mint(policy, [])).Match(
                    Some: variable => PsiRows.Zip(Seq(
                        variable.CombinationFactor.DecimalFractions,
                        variable.FrequentFactor.DecimalFractions,
                        variable.QuasiPermanentFactor.DecimalFractions)),
                    None: static () => Seq<(PropertyName, double)>())
                + Partials(policy)).Run()
                .MapFail(error => new BimFault.CapabilityMiss(key, $"eurocode-factors:{policy.Annex}:{error.Message}"))
                .Bind(rows => Measures(rows.Map(static factor => (factor.Item1, Anonymous, factor.Item2)), scale, key))
                .Map(factors => factors.Add(Situation, Enumerated(policy.Situation.Class.ToString(), SituationKinds))));

    // The elected situation's own gamma columns, read as declared doubles — the contract publishes them in decimal
    // fractions, so no Ratio unwrap sits between the policy and the row. LeadingActionPartialFactor is nullable
    // because the EN leading-action sweep varies it per combination, so it lands as a row only where stated.
    private static Seq<(PropertyName, double)> Partials(EurocodePolicy policy) =>
        Seq((GammaSup, policy.Situation.UnfavourablePermanentActionsPartialFactor),
            (GammaInf, policy.Situation.FavourablePermanentActionsPartialFactor),
            (GammaQi, policy.Situation.OtherAccompanyingVariableActionsPartialFactor),
            (Xi, policy.Situation.ReductionFactor))
        + Optional(policy.Situation.LeadingActionPartialFactor).Map(static gamma => (GammaQ1, gamma)).ToSeq();

    // The load-group definition bag the LoadCase arm extends: the combination/case/group discriminant, the action
    // nature and source, the partial-safety Coefficient (NaN unset, dropped at egress), and the purpose label. A
    // LOAD_COMBINATION_GROUP under an elected policy extends it once more with the EN 1990 combination roster.
    private static Fin<Map<PropertyName, PropertyValue>> GroupOf(
        IfcStructuralLoadGroup group, UnitScale scale, Option<EurocodePolicy> eurocode, Op key) =>
        from measures in Measures(Seq((Coefficient, Named<IfcRatioMeasure>(), group.Coefficient)), scale, key)
        from combination in group.PredefinedType == IfcLoadGroupTypeEnum.LOAD_COMBINATION_GROUP
            ? eurocode.Match(
                Some: policy => Combination(group, policy, scale, key),
                None: static () => Fin.Succ(Map<PropertyName, PropertyValue>()))
            : Fin.Succ(Map<PropertyName, PropertyValue>())
        select Map(
            (LoadGroupType, Enumerated(group.PredefinedType.ToString(), LoadGroupKinds)),
            (ActionType, (PropertyValue)new PropertyValue.Text(group.ActionType.ToString())),
            (ActionSource, new PropertyValue.Text(group.ActionSource.ToString())),
            (Purpose, new PropertyValue.Text(group.Purpose ?? "")))
            .AddRange(measures)
            .AddRange(combination);

    // --- [COMBINATION] --------------------------------------------------------------------------- The
    // EN 1990 combination roster a LOAD_COMBINATION_GROUP lowers: the grouped load cases are minted as the package's
    // OWN typed cases carrying their activities' typed carriers, the combination set is elected off the policy's
    // design-situation class through ENCombinationFactory, and each combination publishes its generated Definition —
    // the 6.10 / 6.10a-b expression with its gammas and psis already resolved — beside the factored design actions
    // GetFactoredLoads yields. The two rows share ONE combination order, so a consumer reads the k-th definition
    // against the actions it produced. Deriving the clause logic here instead is the deleted form: the package owns
    // Annex A1, and a second combination algebra beside it diverges on the first national deviation.
    // Combination minting is the ONE throwing seam (the Annex A1 tables throw for an uncovered annex and the
    // standards kernel throws MissingNationalAnnexException), so it crosses into the rail as BimFault.CapabilityMiss
    // at this one point, exactly as the psi mint does.
    private static Fin<Map<PropertyName, PropertyValue>> Combination(
        IfcStructuralLoadGroup group, EurocodePolicy policy, UnitScale scale, Op key) =>
        toSeq(group.IsGroupedBy)
            .Bind(static rel => toSeq(rel.RelatedObjects).Choose(static o => o is IfcStructuralLoadGroup g ? Some(g) : None))
            .TraverseM(member => CaseOf(member, policy, scale, key))
            .As()
            .Bind(cases => Try.lift(() => Elect(cases.Somes().ToList(), policy)).Run()
                .MapFail(error => new BimFault.CapabilityMiss(key, $"eurocode-combination:{policy.Annex}:{error.Message}")))
            // The carrier's magnitudes are ALREADY SI (the ingest admission coerced them before the carrier was
            // minted), so the factored action re-admits under the IDENTITY regime — threading the model's UnitScale
            // here would price every design action by the model's length factor a second time.
            .Bind(combinations => toSeq(combinations)
                .Bind(static c => toSeq(c.GetFactoredLoads()).Bind(Components))
                .TraverseM(row => Admit(FactoredActions, row.Measure, row.Si, UnitScale.Si, key))
                .As()
                .Map(values => Map(
                    (Combinations, (PropertyValue)new PropertyValue.List(
                        toSeq(combinations).Map(static c => (PropertyValue)new PropertyValue.Text(c.Definition)))),
                    (FactoredActions, (PropertyValue)new PropertyValue.List(
                        values.Map(static value => (PropertyValue)new PropertyValue.Measure(value)))))));

    // The combination roster the elected design situation calls for, composed at the ONE factory seam. The seismic
    // arm is the LIVE CreateSeismic Eq 6.12a/b roster: the seismic-row cases lead under the policy's gammaI
    // (LeadingSeismicPartialFactor), every other case accompanies under its own psi2. The persistent/transient arm
    // reads the policy's CombinationSet — the DesignSituationClass ladder names no EQU or GEO member, so the set is
    // the policy's own axis — and both live StrGeo verbs spell the full (cases, annex, ..., prefix, firstCaseId)
    // arity. STANDING HAND-ASSEMBLY LAW, each verb on its own decompiled ground: CreateEquSetA (both overloads)
    // carries an unconditional item2[1]/list[2] tail after its leading-variable loop — ArgumentOutOfRangeException
    // below two variable cases, a silent overwrite of the third combination above — so no input shape survives it
    // intact; CreateAccidental is index-sound but takes the leading AEd IVariableCase as a REQUIRED argument this
    // fold never holds (the IFC action vocabulary classifies no accidental design action, and the verb dereferences
    // accidentalCase.Name unconditionally, so the caseless call NREs). The EQU and accidental rosters therefore
    // construct the package's own EquilibriumCombination/AccidentalCombination directly through Sweep under the
    // elected DesignSituation — construction only; every gamma and psi still factors inside the package's
    // GetFactoredLoads.
    private const string ComboPrefix = "CO";

    private static IList<ILoadCombination> Elect(IList<(ActionRow Row, ILoadCase Case)> cases, EurocodePolicy policy) {
        List<ILoadCase> all = cases.Select(static pair => pair.Case).ToList();
        return policy.Situation.Class switch {
            DesignSituationClass.Seismic => ENCombinationFactory.CreateSeismic(
                cases.Where(static pair => pair.Row.Case == "seismic").Select(static pair => pair.Case).OfType<IVariableCase>().ToList(),
                policy.Importance,
                cases.Where(static pair => pair.Row.Case != "seismic").Select(static pair => pair.Case).ToList(),
                policy.Annex, ComboPrefix, 1).Cast<ILoadCombination>().ToList(),
            DesignSituationClass.Accidental => Sweep(cases, policy.Situation, accidental: true),
            _ => policy.Set switch {
                CombinationSet.SetA => Sweep(cases, policy.Situation, accidental: false),
                CombinationSet.SetC => ENCombinationFactory.CreateStrGeoSetC(all, policy.Annex, ComboPrefix, 1).Cast<ILoadCombination>().ToList(),
                _ => ENCombinationFactory.CreateStrGeoSetB(all, policy.Annex, use6_10aAnd6_10b: true, ComboPrefix, 1).Cast<ILoadCombination>().ToList(),
            },
        };
    }

    // The hand-assembled EQU/accidental roster: one combination per leading variable action (one bare combination
    // when no variable exists), every permanent case seated through the package's own SetPermanentCases, the
    // remaining variables accompanying. The accidental sweep takes the frequent factor on the main accompanying
    // variable — the EN 1990 6.11b recommended election — and carries no AEd term, because the IFC source
    // vocabulary classifies no accidental design action into a case this fold could lead with; an authored
    // accidental case joins the roster upstream as its own group.
    private static IList<ILoadCombination> Sweep(IList<(ActionRow Row, ILoadCase Case)> cases, IDesignSituation situation, bool accidental) {
        List<IPermanentCase> permanents = cases.Select(static pair => pair.Case).OfType<IPermanentCase>().ToList();
        List<bool> favours = permanents.ConvertAll(static _ => false);
        List<IVariableCase> variables = cases.Select(static pair => pair.Case).OfType<IVariableCase>().ToList();
        IEnumerable<(IList<IVariableCase> Main, IList<IVariableCase> Rest)> sweeps = variables.Count == 0
            ? [(new List<IVariableCase>(), new List<IVariableCase>())]
            : variables.Select(leader => (
                (IList<IVariableCase>)new List<IVariableCase> { leader },
                (IList<IVariableCase>)variables.Where(other => !ReferenceEquals(other, leader)).ToList()));
        return sweeps.Select(split => {
            LoadCombination combination = accidental
                ? new AccidentalCombination {
                    DesignSituation = situation,
                    MainAccompanyingVariableCases = split.Main,
                    OtherAccompanyingVariableCases = split.Rest,
                    UseFrequentCombinationFactorForMainAccompanying = true,
                }
                : new EquilibriumCombination {
                    DesignSituation = situation,
                    LeadingVariableCases = split.Main,
                    AccompanyingVariableCases = split.Rest,
                };
            combination.SetPermanentCases(permanents, favours);
            return (ILoadCombination)combination;
        }).ToList();
    }

    // One grouped load group lowered to the package's own typed case BESIDE its resolved ActionRow — the pair is
    // what lets Elect partition the seismic-row cases from their accompaniment without a name probe. The row
    // resolves through the SAME two-tier CaseSources-then-nature fold RowOf takes (a nature-only read here left an
    // EARTHQUAKE_E group unclassified and the whole seismic chain unreachable); a permanent nature mints the bare
    // PermanentCase, a variable one mints through the ActionRow's OWN mint — the loads-taking call, so the case
    // arrives with its psi set AND its payload in one call rather than a mutation pass afterwards. A variable group
    // whose action the code does not classify carries no mint and therefore no case, so it never enters the
    // combination under a fabricated psi set.
    private static Fin<Option<(ActionRow Row, ILoadCase Case)>> CaseOf(
        IfcStructuralLoadGroup group, EurocodePolicy policy, UnitScale scale, Op key) =>
        toSeq(group.IsGroupedBy)
            .Bind(static rel => toSeq(rel.RelatedObjects).Choose(static o => o is IfcStructuralActivity a ? Some(a) : None))
            .TraverseM(activity => Optional(activity.AppliedLoad).Match(
                Some: load => Carrier(load, Application(activity.GlobalOrLocal), scale, key),
                None: static () => Fin.Succ(Seq<ILoad>())))
            .As()
            .Map(carried => {
                List<ILoad> loads = carried.Bind(static seq => seq).ToList();
                ActionRow row = CaseSources.Find(group.ActionSource).IfNone(() => Nature(group.ActionType, Some(policy)));
                return group.ActionType == IfcActionTypeEnum.PERMANENT_G
                    ? Some((Row: row, Case: (ILoadCase)new PermanentCase { Name = group.Name ?? row.Case, Loads = loads }))
                    : row.Action.Bind(action => action.Mint(policy, loads)).Map(variable => (Row: row, Case: (ILoadCase)variable));
            });

    // The measure identity each ILoad component re-admits under: the carrier holds UnitsNet quantities, so the SI
    // magnitude reads off the quantity's own SI accessor and the row signs the IFC measure type that quantity
    // corresponds to — the factored action re-enters the seam on the SAME frozen MeasureDimensions rows the ingest
    // used, never as a bare double and never under a registry quantity name the raise table cannot resolve.
    private static Seq<(Option<string> Measure, double Si)> Components(ILoad load) => load switch {
        IPointForce f => Seq((Named<IfcForceMeasure>(), f.X.Newtons), (Named<IfcForceMeasure>(), f.Y.Newtons), (Named<IfcForceMeasure>(), f.Z.Newtons)),
        IPointMoment m => Seq((Named<IfcTorqueMeasure>(), m.Xx.NewtonMeters), (Named<IfcTorqueMeasure>(), m.Yy.NewtonMeters), (Named<IfcTorqueMeasure>(), m.Zz.NewtonMeters)),
        ILineForce l => Seq((Named<IfcLinearForceMeasure>(), l.X.NewtonsPerMeter), (Named<IfcLinearForceMeasure>(), l.Y.NewtonsPerMeter), (Named<IfcLinearForceMeasure>(), l.Z.NewtonsPerMeter)),
        IAreaForce a => Seq((Named<IfcPlanarForceMeasure>(), a.X.Pascals), (Named<IfcPlanarForceMeasure>(), a.Y.Pascals), (Named<IfcPlanarForceMeasure>(), a.Z.Pascals)),
        IGravity g => Seq((Anonymous, g.X.DecimalFractions), (Anonymous, g.Y.DecimalFractions), (Anonymous, g.Z.DecimalFractions)),
        _ => Seq<(Option<string>, double)>(),
    };

    // The 1D families share the consumer-neutral ForceX..Z / MomentX..Z names — the exact wire the Rasm.Compute
    // StructuralReads Vec(g, "Force")/Vec(g, "Moment") probes take for point AND uniform actions — the family
    // discriminated by the LoadType token and each component's own MEASURE TYPE (a point force N against a line force
    // N/m, a torque N.m against a line moment N.m/m = N, the last pair sharing a signature the NAME alone separates);
    // a per-family LinearForceX-style namespace forked the uniform read onto silent zeros.
    private static Seq<(PropertyName Name, Option<string> Measure, double Native)> Vectors(IfcStructuralLoad load) => load switch {
        IfcStructuralLoadSingleForce f => Seq(
            (StructuralRows.Force["X"], Named<IfcForceMeasure>(), f.ForceX), (StructuralRows.Force["Y"], Named<IfcForceMeasure>(), f.ForceY), (StructuralRows.Force["Z"], Named<IfcForceMeasure>(), f.ForceZ),
            (StructuralRows.Moment["X"], Named<IfcTorqueMeasure>(), f.MomentX), (StructuralRows.Moment["Y"], Named<IfcTorqueMeasure>(), f.MomentY), (StructuralRows.Moment["Z"], Named<IfcTorqueMeasure>(), f.MomentZ)),
        IfcStructuralLoadLinearForce l => Seq(
            (StructuralRows.Force["X"], Named<IfcLinearForceMeasure>(), l.LinearForceX), (StructuralRows.Force["Y"], Named<IfcLinearForceMeasure>(), l.LinearForceY), (StructuralRows.Force["Z"], Named<IfcLinearForceMeasure>(), l.LinearForceZ),
            (StructuralRows.Moment["X"], Named<IfcLinearMomentMeasure>(), l.LinearMomentX), (StructuralRows.Moment["Y"], Named<IfcLinearMomentMeasure>(), l.LinearMomentY), (StructuralRows.Moment["Z"], Named<IfcLinearMomentMeasure>(), l.LinearMomentZ)),
        IfcStructuralLoadPlanarForce p => Seq(
            (StructuralRows.PlanarForce["X"], Named<IfcPlanarForceMeasure>(), p.PlanarForceX), (StructuralRows.PlanarForce["Y"], Named<IfcPlanarForceMeasure>(), p.PlanarForceY), (StructuralRows.PlanarForce["Z"], Named<IfcPlanarForceMeasure>(), p.PlanarForceZ)),
        IfcStructuralLoadTemperature t => Seq(
            (StructuralRows.DeltaT["Constant"], Named<IfcThermodynamicTemperatureMeasure>(), t.DeltaT_Constant), (StructuralRows.DeltaT["Y"], Named<IfcThermodynamicTemperatureMeasure>(), t.DeltaT_Y), (StructuralRows.DeltaT["Z"], Named<IfcThermodynamicTemperatureMeasure>(), t.DeltaT_Z)),
        // The IFC varying line action: IfcStructuralLoadConfiguration over its public Values/Locations, whose kept
        // first/last rows lower onto the trapezoid wire (StartX..Z/EndX..Z) the Rasm.Compute Vec(g, "Start")/
        // Vec(g, "End") probes read BESIDE the span those two positions bound. A single-row or non-linear-force
        // configuration falls through to the graceful passthrough, never a fabricated ramp.
        IfcStructuralLoadConfiguration cfg when Ramp(cfg) is { Count: >= 2 } ramp => Seq(
            (StructuralRows.Start["X"], Named<IfcLinearForceMeasure>(), ramp[0].Force.LinearForceX), (StructuralRows.Start["Y"], Named<IfcLinearForceMeasure>(), ramp[0].Force.LinearForceY), (StructuralRows.Start["Z"], Named<IfcLinearForceMeasure>(), ramp[0].Force.LinearForceZ),
            (StructuralRows.End["X"], Named<IfcLinearForceMeasure>(), ramp[ramp.Count - 1].Force.LinearForceX), (StructuralRows.End["Y"], Named<IfcLinearForceMeasure>(), ramp[ramp.Count - 1].Force.LinearForceY), (StructuralRows.End["Z"], Named<IfcLinearForceMeasure>(), ramp[ramp.Count - 1].Force.LinearForceZ),
            (SpanStart, Named<IfcLengthMeasure>(), ramp[0].At), (SpanEnd, Named<IfcLengthMeasure>(), ramp[ramp.Count - 1].At)),
        // IfcStructuralLoadSingleDisplacement holds its DisplacementX/Y/Z + RotationalDisplacementRX/RY/RZ as INTERNAL
        // fields — NO public accessor crosses the assembly boundary — so a prescribed-displacement
        // (support-settlement) load reads the frame attrs only (LoadType/LoadKind/Case/GlobalOrLocal/Source via LoadOf),
        // the documented surface boundary rather than a phantom `d.DisplacementX` read or a silently-invented 0-settlement;
        // the `_` graceful passthrough owns it alongside any unenumerated load family, never a fabricated component.
        _ => Seq<(PropertyName, Option<string>, double)>(),
    };

    // The IFC varying line action pairs Values with Locations POSITIONALLY (LIST [1:?] OF LIST [1:2] OF
    // IfcLengthMeasure — the distance-along each value sits at), so the ramp zips the two by ORDINAL and keeps the
    // linear-force rows only afterwards: filtering Values first re-indexes the survivors and silently re-reads
    // another value's position. An ordinal whose Locations entry is short or absent drops WHOLE, so a partial-span
    // action carries both its magnitudes and its own span or does not lower at all — the action assumed to run the
    // member's full length is the deleted fabrication, and it is unobservable downstream because a trapezoid with no
    // span reads as a full-length ramp.
    private static Seq<(IfcStructuralLoadLinearForce Force, double At)> Ramp(IfcStructuralLoadConfiguration cfg) =>
        toSeq(cfg.Values).Zip(toSeq(cfg.Locations))
            .Choose(static pair => pair.Item1 is IfcStructuralLoadLinearForce force && pair.Item2 is { Count: > 0 } at
                ? Some((Force: force, At: at[0]))
                : None);

    // --- [CARRIER] ------------------------------------------------------------------------------ The
    // typed VividOrange carrier one GG load lowers to. Components enter as UnitsNet quantities minted FROM the
    // already-coerced SI magnitudes the Measures admission produced — Force.FromNewtons, ForcePerLength.
    // FromNewtonsPerMeter, Pressure.FromPascals, Torque.FromNewtonMeters — so the carrier lane reaches neither
    // ToUnit(UnitSystem.SI) (which throws for every quantity with an empty SI unit-info walk) nor
    // QuantityTypeConverter (whose wire is a culture-formatted abbreviation), the two counts the
    // no-UnitsNet-at-the-measure-boundary law rests on. The carrier is the payload the EN 1990 algebra folds
    // ILoad.Factor(Ratio) across, so every combination and partial factor is applied by the package owning the
    // tables and never by a scalar multiply here; the per-activity ROWS stay the faithful unfactored IFC reading,
    // because the re-author re-stamps what the file declared. A single force yields TWO carriers — the point force
    // and its point moment — because the two are distinct ILoad shapes the combination algebra factors separately.
    private static Fin<Seq<ILoad>> Carrier(IfcStructuralLoad load, LoadApplication application, UnitScale scale, Op key) =>
        Measures(Vectors(load), scale, key).Map(si => load switch {
            IfcStructuralLoadSingleForce => Seq<ILoad>(
                new PointForce(Force.FromNewtons(Si(si, StructuralRows.Force["X"])), Force.FromNewtons(Si(si, StructuralRows.Force["Y"])), Force.FromNewtons(Si(si, StructuralRows.Force["Z"]))),
                new PointMoment(Torque.FromNewtonMeters(Si(si, StructuralRows.Moment["X"])), Torque.FromNewtonMeters(Si(si, StructuralRows.Moment["Y"])), Torque.FromNewtonMeters(Si(si, StructuralRows.Moment["Z"])))),
            IfcStructuralLoadLinearForce => Seq<ILoad>(new LineForce(
                ForcePerLength.FromNewtonsPerMeter(Si(si, StructuralRows.Force["X"])), ForcePerLength.FromNewtonsPerMeter(Si(si, StructuralRows.Force["Y"])),
                ForcePerLength.FromNewtonsPerMeter(Si(si, StructuralRows.Force["Z"])), application)),
            IfcStructuralLoadPlanarForce => Seq<ILoad>(new AreaForce(
                Pressure.FromPascals(Si(si, StructuralRows.PlanarForce["X"])),
                Pressure.FromPascals(Si(si, StructuralRows.PlanarForce["Y"])),
                Pressure.FromPascals(Si(si, StructuralRows.PlanarForce["Z"])), application)),
            // A temperature action, a prescribed displacement, and a trapezoid configuration have no ILoad shape in
            // the taxonomy: they carry their rows and take no part in the combination fold rather than lowering onto
            // a nearest-fit carrier that would factor as a force the model never declared.
            _ => Seq<ILoad>(),
        });

    // The projection frame an ILineForce/IAreaForce resolves against, off the activity's own IFC declaration — the
    // same GlobalOrLocal fact the LoadOf bag stamps as a row, read once rather than defaulted per carrier.
    private static LoadApplication Application(IfcGlobalOrLocalEnum declared) =>
        declared == IfcGlobalOrLocalEnum.LOCAL_COORDS ? LoadApplication.Local : LoadApplication.Global;

    // --- [TOPOLOGY_DISCRIMINANTS] ---------------------------------------------------------------
    // The start/end discriminant the IfcRelConnectsStructuralMember Generic edge carries so Rasm.Compute resolves a
    // support to the correct member joint: the point connection's vertex compared to the member's analytical-edge
    // endpoints (nearer Start -> true). The endpoint coordinates are read TRANSIENTLY off GeometryGym topology to
    // compute the boolean — never stored on the seam node (the analytical line itself rides the Axis-keyed content
    // hash in Representations). A member with no analytical edge, a connection with no vertex, or a malformed
    // vertex point folds to the end joint (false — the consumer's WireAtStart default), so an unresolved endpoint
    // never silently claims the start and never compares against a fabricated origin.
    public static Option<bool> AtStart(IfcStructuralCurveMember? member, IfcStructuralConnection? connection) =>
        from m in Optional(member)
        from edge in EdgeOf(m.Representation)
        from vertex in PointOf((connection as IfcStructuralPointConnection)?.Vertex)
        from s in PointOf(edge.EdgeStart)
        from e in PointOf(edge.EdgeEnd)
        select Vector3.Distance(vertex, s) <= Vector3.Distance(vertex, e);

    // The normalized point-action position along the member's analytical edge (0 start .. 1 end): the activity's
    // topology vertex projected onto the start-end chord, the SAME transient read discipline AtStart holds. None
    // when any topology is absent/malformed or the chord degenerate — a surface action or an unpositioned load
    // never fabricates a station, and the consumer's absent-default (midspan 0.5) stays the honest fallback.
    public static Option<double> Station(IfcStructuralCurveMember? member, IfcStructuralActivity? activity) =>
        from m in Optional(member)
        from edge in EdgeOf(m.Representation)
        from v in VertexOf(activity?.Representation)
        from s in PointOf(edge.EdgeStart)
        from e in PointOf(edge.EdgeEnd)
        let chord = Vector3.Dot(e - s, e - s)
        where chord > 0d
        select Math.Clamp(Vector3.Dot(v - s, e - s) / chord, 0d, 1d);

    // The member's analytical topology edge / the activity's position vertex, read transiently off the inherited
    // IfcProduct.Representation for the AtStart/Station compares ONLY — the coordinates produce the Boolean/scalar
    // discriminant and are never carried onto a seam node.
    private static Option<IfcEdge> EdgeOf(IfcProductDefinitionShape? shape) =>
        Optional(shape).Bind(static s => s.Representations.AsIterable()
            .SelectMany(static rep => rep.Items.AsIterable())
            .Choose(static item => item is IfcEdge e ? Some(e) : None)
            .ToSeq().Head);

    private static Option<Vector3> VertexOf(IfcProductDefinitionShape? shape) =>
        Optional(shape).Bind(static s => s.Representations.AsIterable()
            .SelectMany(static rep => rep.Items.AsIterable())
            .Choose(static item => item is IfcVertexPoint vp ? PointOf(vp) : None)
            .ToSeq().Head);

    // 2D-honest: an IN_PLANE analytical model's IfcCartesianPoint legally carries TWO coordinates, so a 2-coordinate
    // vertex reads (x, y, 0) rather than collapsing every plane-frame joint onto a fabricated origin; a point with
    // fewer coordinates is malformed and yields None — the callers' honest fallbacks own it.
    private static Option<Vector3> PointOf(IfcVertex? vertex) =>
        vertex is IfcVertexPoint { VertexGeometry: IfcCartesianPoint { Coordinates: { Count: >= 2 } c } }
            ? Some(new Vector3(c[0], c[1], c.Count >= 3 ? c[2] : 0d))
            : None;

    // --- [CORRESPONDENCE] -----------------------------------------------------------------------
    // The physical↔analytical read path over the ONE seam graph: every analytical member Object node classifies
    // through the StructuralCorrespondence roster, its optional physical counterpart resolves off the
    // IfcRelConnectsStructuralElement Generic edge (physical element → idealized member, the 2x3 binding the
    // relations fold lands), and its joints off the IfcRelConnectsStructuralMember Generic edges — the AtStart
    // Boolean and the Eccentricity content key read back through the SAME owner-declared rows this reader stamped,
    // so producer and consumer share one spelling. Producer: this fold. Consumers: the Rasm.Compute
    // analytical-model assembly (member/joint topology in one read beside the edge-borne restraint and load reads)
    // and the Workbook SAF lowering below, whose member spine these rows ARE. An analytical member class outside
    // the roster faults ModelRejected — an unrostered family cannot silently skip idealization — and a malformed
    // eccentricity key faults the same way: this reader stamped it, so a non-hex payload is corruption, never
    // vocabulary.
    public static Fin<Seq<CorrespondenceRow>> Correspondence(ElementGraph graph, Op key) {
        Seq<Relationship.Generic> generics = toSeq(graph.Edges).Choose(static edge => edge is Relationship.Generic g ? Some(g) : None);
        // AddOrUpdate, never toMap: the edge set is FILE-controlled, and a source binding one idealized member from
        // two physical elements (or repeating the relation) is malformed data a throwing duplicate-key Add would
        // escalate into an unhandled exception across the Fin rail — the same census law the taxonomy Audit's
        // by-entity index states. Last-wins is the deterministic election over the seam's ordered edge fold.
        Map<NodeId, NodeId> physicals = generics
            .Filter(static edge => edge.WireName == IfcRelKind.ConnectsStructElement.Key)
            .Fold(Map<NodeId, NodeId>(), static (map, edge) => map.AddOrUpdate(edge.Target, edge.Source));
        Seq<Relationship.Generic> joints = generics.Filter(static edge => edge.WireName == IfcRelKind.ConnectsStructMember.Key);
        return graph.ObjectNodes
            .Choose(node => StructuralCorrespondence.OfAnalytical(node.Classification.Code)
                .Filter(static row => row.IsMember)
                .Map(row => (Node: node, Kind: row)))
            .TraverseM(member => joints.Filter(joint => joint.Source == member.Node.Id)
                .TraverseM(joint => JointOf(graph, joint, key)).As()
                .Map(resolved => new CorrespondenceRow(
                    member.Node.Id, physicals.Find(member.Node.Id), member.Kind, member.Node.PredefinedType.Token,
                    physicals.Find(member.Node.Id)
                        .Bind(graph.Find)
                        .Bind(node => node is Node.Object o ? member.Kind.Physical.Find(o.Classification.Code) : None),
                    resolved)))
            .As();
    }

    // One joint read: the related connection classifies through the roster's connection rows, the AtStart
    // discriminant reads its Boolean row, and the eccentricity content key parses back from the X32 hex the
    // Preserve stamp wrote.
    private static Fin<CorrespondenceJoint> JointOf(ElementGraph graph, Relationship.Generic joint, Op key) =>
        from kind in graph.Find(joint.Target)
            .Bind(static node => node is Node.Object o ? StructuralCorrespondence.OfAnalytical(o.Classification.Code) : None)
            .Filter(static row => !row.IsMember)
            .ToFin(new BimFault.ModelRejected(key, $"correspondence-connection-unrostered:{joint.Target}"))
        from eccentricity in joint.Attributes.Find(Eccentricity).Match(
            Some: value => value is PropertyValue.Text text && UInt128.TryParse(text.Value, NumberStyles.HexNumber, null, out UInt128 parsed)
                ? Fin.Succ(Some(parsed))
                : Fin.Fail<Option<UInt128>>(new BimFault.ModelRejected(key, $"correspondence-eccentricity-malformed:{joint.Target}")),
            None: static () => Fin.Succ(Option<UInt128>.None))
        select new CorrespondenceJoint(joint.Target, kind,
            joint.Attributes.Find(StructuralRows.AtStart).Bind(static value => value is PropertyValue.Boolean b ? Some(b.Value) : None),
            eccentricity);

    // --- [SAF_EXCHANGE] -------------------------------------------------------------------------
    // The ExcelModel↔seam arms the saf format row's capability flags stand on — Saf above owns the XLSX bytes and
    // validation, Workbook builds the export model FROM the graph, and the Author overload realizes the import by
    // AUTHORING the GeometryGym structural-analysis entities the ONE SemanticProjector then ingests, so no second
    // projector ever mints member nodes beside the general fold. SAF is an ANALYTICAL exchange: the Correspondence
    // rows are the member spine on both legs, the DOF verdicts cross as the SAF constraint pair, and every
    // quantity mints FROM already-SI seam magnitudes through the UnitsNet From* factories and reads back through
    // the typed SI accessors — the carrier-lane idiom, so neither ToUnit(UnitSystem.SI) nor QuantityTypeConverter
    // is reached on the SAF lane either.
    // The variety↔behaviour correspondence, stated at BOTH owners as one invariant: analytical variety tokens lower
    // onto the SAF FE-behaviour cell here, and the import elects the variety back through VarietyElect below —
    // CABLE and TENSION_MEMBER share TensionOnly on the wire, so an imported tension-only member reads
    // TENSION_MEMBER (the wire keeps no cable distinction); RIGID_JOINED_MEMBER and the USERDEFINED/NOTDEFINED
    // residue read Standard.
    private static readonly Map<string, ExcelCurveBehaviour> Behaviours = toMap(Seq(
        (nameof(IfcStructuralCurveMemberTypeEnum.RIGID_JOINED_MEMBER), ExcelCurveBehaviour.Standard),
        (nameof(IfcStructuralCurveMemberTypeEnum.PIN_JOINED_MEMBER), ExcelCurveBehaviour.AxialForceOnly),
        (nameof(IfcStructuralCurveMemberTypeEnum.CABLE), ExcelCurveBehaviour.TensionOnly),
        (nameof(IfcStructuralCurveMemberTypeEnum.TENSION_MEMBER), ExcelCurveBehaviour.TensionOnly),
        (nameof(IfcStructuralCurveMemberTypeEnum.COMPRESSION_MEMBER), ExcelCurveBehaviour.CompressionOnly)));

    private static readonly Map<ExcelCurveBehaviour, IfcStructuralCurveMemberTypeEnum> VarietyElect = toMap(Seq(
        (ExcelCurveBehaviour.Standard, IfcStructuralCurveMemberTypeEnum.RIGID_JOINED_MEMBER),
        (ExcelCurveBehaviour.AxialForceOnly, IfcStructuralCurveMemberTypeEnum.PIN_JOINED_MEMBER),
        (ExcelCurveBehaviour.TensionOnly, IfcStructuralCurveMemberTypeEnum.TENSION_MEMBER),
        (ExcelCurveBehaviour.CompressionOnly, IfcStructuralCurveMemberTypeEnum.COMPRESSION_MEMBER)));

    // The case-token↔SAF-load-type correspondence both legs share: the seam Case vocabulary is the consumer's
    // closed dead/live/snow/wind/seismic set, so dead and live read Others (SAF's SelfWeight names the generated
    // self-weight case specifically, which the seam token does not assert) and the climatic/seismic tokens map
    // one-to-one. The import inverse rides SourceOf below off the SAF LoadType directly, the richer axis.
    private static readonly Map<string, ExcelLoadCaseType> CaseTypes = toMap(Seq(
        ("dead", ExcelLoadCaseType.Others), ("live", ExcelLoadCaseType.Others), ("snow", ExcelLoadCaseType.Snow),
        ("wind", ExcelLoadCaseType.Wind), ("seismic", ExcelLoadCaseType.Seismic)));

    // SAF load-case nature → the IFC action source the ingest CaseSources tier re-classifies on the next read, so a
    // SAF round-trip lands the same ActionRow the IFC wire would. Moving rides TRANSPORT and Maintenance the
    // imposed LIVE_LOAD_Q; Dynamic and Static carry no IFC source of their own and stay NOTDEFINED, which the
    // nature tier absorbs.
    private static readonly Map<ExcelLoadCaseType, IfcActionSourceTypeEnum> SourceOf = toMap(Seq(
        (ExcelLoadCaseType.SelfWeight, IfcActionSourceTypeEnum.DEAD_LOAD_G),
        (ExcelLoadCaseType.Prestress, IfcActionSourceTypeEnum.PRESTRESSING_P),
        (ExcelLoadCaseType.Temperature, IfcActionSourceTypeEnum.TEMPERATURE_T),
        (ExcelLoadCaseType.Wind, IfcActionSourceTypeEnum.WIND_W),
        (ExcelLoadCaseType.Snow, IfcActionSourceTypeEnum.SNOW_S),
        (ExcelLoadCaseType.Maintenance, IfcActionSourceTypeEnum.LIVE_LOAD_Q),
        (ExcelLoadCaseType.Fire, IfcActionSourceTypeEnum.FIRE),
        (ExcelLoadCaseType.Moving, IfcActionSourceTypeEnum.TRANSPORT),
        (ExcelLoadCaseType.Seismic, IfcActionSourceTypeEnum.EARTHQUAKE_E)));

    // The graph→ExcelModel lowering the export leg realizes. Geometry crosses ONLY by content key through the one
    // ResolveAxis hop — a member or node whose key resolves nothing emits its row without coordinate cells, the
    // named degradation, never a fabricated span. Named negatives this leg states: the eccentricity content key is
    // a preserved STEP fragment, not a Y/Z scalar pair, so the SAF AnalysisEccentricity columns stay unset (the
    // eccentricity survives the IFC wire, not the SAF wire); the thermal gradient rows (DeltaT_Y/DeltaT_Z) name no
    // SAF cell — TempL/R/T/B are fiber temperatures needing a section height no row carries — so only the constant
    // DeltaT crosses; and the EN combination roster stays off the workbook, because SAF's combination table wants
    // per-case factor arrays where the seam stores the package-generated Definition expressions and factored SI
    // actions — a consumer re-elects combinations from its own national standard, the SAF-idiomatic read, and a
    // hand-parsed factor array off the Definition text is the deleted form.
    public static Fin<ExcelModel> Workbook(ElementGraph graph, ResolveAxis resolve, Op key) =>
        Correspondence(graph, key).Map(rows => {
            Map<NodeId, Node.Object> objects = toMap(graph.ObjectNodes.Map(static node => (node.Id, node)));
            Seq<Relationship.Generic> generics = toSeq(graph.Edges).Choose(static edge => edge is Relationship.Generic g ? Some(g) : None);
            Seq<Relationship.Generic> joints = generics.Filter(static edge => edge.WireName == IfcRelKind.ConnectsStructMember.Key);
            Seq<Relationship.Generic> activities = generics.Filter(static edge => edge.WireName == IfcRelKind.ConnectsStructActivity.Key);

            Seq<IExcelModuleObject> points = graph.ObjectNodes
                .Filter(static node => StructuralCorrespondence.OfAnalytical(node.Classification.Code)
                    .Exists(static row => row == StructuralCorrespondence.PointConnection))
                .Map(node => (IExcelModuleObject)(VertexOf(node, resolve).Match(
                    Some: at => new ExcelStructuralPointConnection {
                        Id = GuidOf(node), Name = SafName(node),
                        X = Length.FromMeters(at.X), Y = Length.FromMeters(at.Y), Z = Length.FromMeters(at.Z),
                    },
                    None: () => new ExcelStructuralPointConnection { Id = GuidOf(node), Name = SafName(node) })));

            Seq<IExcelModuleObject> members = rows
                .Choose(row => objects.Find(row.Analytical).Map(node => (Row: row, Node: node)))
                .Map(member => member.Row.Kind.Dimension == 1
                    ? (IExcelModuleObject)Curve(objects, member.Row, member.Node, resolve)
                    : Surface(graph, member.Row, member.Node));

            Seq<IExcelModuleObject> supports = graph.ObjectNodes
                .Choose(node => StructuralCorrespondence.OfAnalytical(node.Classification.Code)
                    .Filter(static row => !row.IsMember)
                    .Map(row => (Node: node, Kind: row)))
                .Bind(connection => SupportsOf(graph, joints, objects, connection.Node, connection.Kind));

            Seq<IExcelModuleObject> releases = joints
                .Filter(static joint => ReleaseDofs.Exists(joint.Attributes.ContainsKey))
                .Map(joint => (IExcelModuleObject)new ExcelRelConnectsStructuralMember {
                    Name = $"{Host(objects, joint.Source)}-{Host(objects, joint.Target)}",
                    Member = Host(objects, joint.Source),
                    Position = joint.Attributes.Find(StructuralRows.AtStart)
                        .Bind(static value => value is PropertyValue.Boolean b ? Some(b.Value) : None)
                        .Map(static atStart => atStart ? ExcelPosition.Begin : ExcelPosition.End)
                        .Match(Some: static position => (ExcelPosition?)position, None: static () => null),
                    TranslationXType = Constraint(joint.Attributes, ReleaseTranslation["X"]).Type,
                    TranslationXStiffness = Spring(joint.Attributes, ReleaseTranslation["X"], ForcePerLength.FromNewtonsPerMeter),
                    TranslationYType = Constraint(joint.Attributes, ReleaseTranslation["Y"]).Type,
                    TranslationYStiffness = Spring(joint.Attributes, ReleaseTranslation["Y"], ForcePerLength.FromNewtonsPerMeter),
                    TranslationZType = Constraint(joint.Attributes, ReleaseTranslation["Z"]).Type,
                    TranslationZStiffness = Spring(joint.Attributes, ReleaseTranslation["Z"], ForcePerLength.FromNewtonsPerMeter),
                    RotationXType = Constraint(joint.Attributes, ReleaseRotation["X"]).Type,
                    RotationXStiffness = Spring(joint.Attributes, ReleaseRotation["X"], RotationalStiffness.FromNewtonMetersPerRadian),
                    RotationYType = Constraint(joint.Attributes, ReleaseRotation["Y"]).Type,
                    RotationYStiffness = Spring(joint.Attributes, ReleaseRotation["Y"], RotationalStiffness.FromNewtonMetersPerRadian),
                    RotationZType = Constraint(joint.Attributes, ReleaseRotation["Z"]).Type,
                    RotationZStiffness = Spring(joint.Attributes, ReleaseRotation["Z"], RotationalStiffness.FromNewtonMetersPerRadian),
                });

            Seq<string> tokens = activities.Choose(static edge => edge.Attributes.Find(StructuralRows.Case).Bind(Text)).Distinct();
            Seq<IExcelModuleObject> cases = tokens.Map(token => (IExcelModuleObject)new ExcelStructuralLoadCase {
                Name = token,
                ActionType = activities
                    .Filter(edge => edge.Attributes.Find(StructuralRows.Case).Bind(Text) == Some(token))
                    .Choose(static edge => edge.Attributes.Find(ActionClassRow).Bind(Text)).Head
                    .Map(static nature => nature switch {
                        nameof(ActionClass.Permanent) => ExcelActionType.Permanent,
                        nameof(ActionClass.Accidental) => ExcelActionType.Accidental,
                        _ => ExcelActionType.Variable,
                    })
                    .Match(Some: static nature => (ExcelActionType?)nature, None: static () => null),
                LoadType = CaseTypes.Find(token).Match(Some: static type => (ExcelLoadCaseType?)type, None: static () => null),
            });

            Seq<IExcelModuleObject> loads = activities.Bind(edge => Actions(objects, edge));

            return new ExcelModel(
                (points + members + supports + releases + cases + loads).ToList(),
                new List<ExcelValidationResult>(), ExcelSystemOfUnits.Metric);
        });

    // One 1D member row off its correspondence: joints order AtStart-first onto the SAF begin-to-end node list, the
    // role cell fills only off a BOUND physical counterpart (an unbound member's Type stays unset rather than
    // fabricated), a role outside the SAF enum rides the flexible enum's own other-text lane, and the axis chord
    // fills Length when the content key resolves.
    private static ExcelStructuralCurveMember Curve(
        Map<NodeId, Node.Object> objects, CorrespondenceRow row, Node.Object node, ResolveAxis resolve) {
        Seq<CorrespondenceJoint> ordered =
            row.Joints.Filter(static joint => joint.AtStart == Some(true))
            + row.Joints.Filter(static joint => joint.AtStart != Some(true));
        ExcelStructuralCurveMember member = new() {
            Id = GuidOf(node), Name = SafName(node),
            Type = row.SafRole.Map(static role => Enum.TryParse(role, out ExcelMember1DType known)
                    ? new ExcelFlexibleEnum<ExcelMember1DType>(known)
                    : new ExcelFlexibleEnum<ExcelMember1DType>(role))
                .Match(Some: static type => type, None: static () => (ExcelFlexibleEnum<ExcelMember1DType>?)null),
            Behaviour = Behaviours.Find(row.Variety).Match(Some: static b => (ExcelCurveBehaviour?)b, None: static () => null),
            Nodes = ordered.Map(joint => Host(objects, joint.Connection)).ToArray(),
        };
        node.Representations.Find("Axis").Bind(contentKey => resolve(contentKey))
            .Filter(static polyline => polyline.Count >= 2)
            .IfSome(polyline => member.Length = Length.FromMeters(Vector3.Distance(polyline[0], polyline[polyline.Count - 1])));
        return member;
    }

    // One 2D member row: role off the bound physical class, the constant thickness off the member's own entity bag
    // (the Attrs surface arm stamped it there), the outline nodes off the joints — a varying thickness has no seam
    // row and stays a SAF-side authoring concern.
    private static ExcelStructuralSurfaceMember Surface(ElementGraph graph, CorrespondenceRow row, Node.Object node) {
        ExcelStructuralSurfaceMember member = new() {
            Id = GuidOf(node), Name = SafName(node),
            Type = row.SafRole.Map(static role => Enum.TryParse(role, out ExcelMember2DType known)
                    ? new ExcelFlexibleEnum<ExcelMember2DType>(known)
                    : new ExcelFlexibleEnum<ExcelMember2DType>(role))
                .Match(Some: static type => type, None: static () => (ExcelFlexibleEnum<ExcelMember2DType>?)null),
        };
        BagOf(graph, node.Id).Find(Thickness)
            .Bind(static value => value is PropertyValue.Measure m ? Some(m.Value.Si) : None)
            .IfSome(si => member.Thickness = new ExcelMemberThickness { ThicknessFirst = Length.FromMeters(si) });
        return member;
    }

    // The support rows one connection lowers: dimension 0 emits the SAF point support off the node DOF verdicts
    // (the connection's own bag first, else the first incident joint edge's Support family — one custody, two
    // stamp sites), dimension 1 the curve connection off the subgrade verdicts (Pressure and
    // RotationalStiffnessPerLength — the two reaction quantities one exponent below the node pair, exactly the
    // measure split the ingest read), and dimension 2 the bare surface-connection row: the face condition's
    // stiffness is SEALED at the source (internal fields, the Attrs empty-bag arm), so no subsoil cell is ever
    // fabricated for it.
    private static Seq<IExcelModuleObject> SupportsOf(
        ElementGraph graph, Seq<Relationship.Generic> joints, Map<NodeId, Node.Object> objects,
        Node.Object connection, StructuralCorrespondence kind) {
        Map<PropertyName, PropertyValue> bag = BagOf(graph, connection.Id);
        Map<PropertyName, PropertyValue> attrs = StructuralRows.Axes.Exists(axis => bag.ContainsKey(StructuralRows.Translation[axis]))
            ? bag
            : joints.Filter(joint => joint.Target == connection.Id).Map(static joint => joint.Attributes)
                .Filter(static payload => StructuralRows.Axes.Exists(axis => payload.ContainsKey(StructuralRows.Translation[axis])))
                .Head.IfNone(bag);
        string member = joints.Filter(joint => joint.Target == connection.Id).Head
            .Map(joint => Host(objects, joint.Source)).IfNone("");
        return kind.Dimension switch {
            0 => Seq((IExcelModuleObject)new ExcelStructuralPointSupport {
                Id = GuidOf(connection), Name = SafName(connection), Node = SafName(connection),
                Type = Predefined(attrs), BoundaryCondition = ExcelStructuralPointSupportType.InNode,
                TranslationXType = Constraint(attrs, StructuralRows.Translation["X"]).Type,
                TranslationXStiffness = Spring(attrs, StructuralRows.Translation["X"], ForcePerLength.FromNewtonsPerMeter),
                TranslationYType = Constraint(attrs, StructuralRows.Translation["Y"]).Type,
                TranslationYStiffness = Spring(attrs, StructuralRows.Translation["Y"], ForcePerLength.FromNewtonsPerMeter),
                TranslationZType = Constraint(attrs, StructuralRows.Translation["Z"]).Type,
                TranslationZStiffness = Spring(attrs, StructuralRows.Translation["Z"], ForcePerLength.FromNewtonsPerMeter),
                RotationXType = Constraint(attrs, StructuralRows.Rotation["X"]).Type,
                RotationXStiffness = Spring(attrs, StructuralRows.Rotation["X"], RotationalStiffness.FromNewtonMetersPerRadian),
                RotationYType = Constraint(attrs, StructuralRows.Rotation["Y"]).Type,
                RotationYStiffness = Spring(attrs, StructuralRows.Rotation["Y"], RotationalStiffness.FromNewtonMetersPerRadian),
                RotationZType = Constraint(attrs, StructuralRows.Rotation["Z"]).Type,
                RotationZStiffness = Spring(attrs, StructuralRows.Rotation["Z"], RotationalStiffness.FromNewtonMetersPerRadian),
            }),
            1 => Seq((IExcelModuleObject)new ExcelStructuralCurveConnection {
                Id = GuidOf(connection), Name = SafName(connection), Member = member,
                TranslationXType = Constraint(attrs, StructuralRows.Translation["X"]).Type,
                TranslationXStiffness = Spring(attrs, StructuralRows.Translation["X"], Pressure.FromPascals),
                TranslationYType = Constraint(attrs, StructuralRows.Translation["Y"]).Type,
                TranslationYStiffness = Spring(attrs, StructuralRows.Translation["Y"], Pressure.FromPascals),
                TranslationZType = Constraint(attrs, StructuralRows.Translation["Z"]).Type,
                TranslationZStiffness = Spring(attrs, StructuralRows.Translation["Z"], Pressure.FromPascals),
                RotationXType = Constraint(attrs, StructuralRows.Rotation["X"]).Type,
                RotationXStiffness = Spring(attrs, StructuralRows.Rotation["X"], RotationalStiffnessPerLength.FromNewtonMetersPerRadianPerMeter),
                RotationYType = Constraint(attrs, StructuralRows.Rotation["Y"]).Type,
                RotationYStiffness = Spring(attrs, StructuralRows.Rotation["Y"], RotationalStiffnessPerLength.FromNewtonMetersPerRadianPerMeter),
                RotationZType = Constraint(attrs, StructuralRows.Rotation["Z"]).Type,
                RotationZStiffness = Spring(attrs, StructuralRows.Rotation["Z"], RotationalStiffnessPerLength.FromNewtonMetersPerRadianPerMeter),
            }),
            _ => Seq((IExcelModuleObject)new ExcelStructuralSurfaceConnection {
                Id = GuidOf(connection), Name = SafName(connection), Member2D = member,
            }),
        };
    }

    // The SAF actions one activity edge lowers, dispatched on the faithful LoadType token exactly as the Author
    // re-stamp is: point force/moment split into the SAF point action and one point-moment row per stamped axis,
    // uniform line force into the vector curve action beside one curve-moment row per stamped axis, the trapezoid
    // into the two-vector Trapezoidal row bounded by its own span, the planar force into one surface action per
    // stamped axis (SAF's surface cell is single-valued per direction), and the temperature action into the
    // constant-DeltaT thermal row. A displacement edge carries no component rows and emits nothing.
    private static Seq<IExcelModuleObject> Actions(Map<NodeId, Node.Object> objects, Relationship.Generic edge) {
        Map<PropertyName, PropertyValue> attrs = edge.Attributes;
        string host = Host(objects, edge.Source);
        bool onNode = objects.Find(edge.Source)
            .Bind(static node => StructuralCorrespondence.OfAnalytical(node.Classification.Code))
            .Exists(static row => !row.IsMember);
        string caseName = attrs.Find(StructuralRows.Case).Bind(Text).IfNone("live");
        ExcelCoordinateSystem system = attrs.Find(GlobalOrLocal).Bind(Text)
            .Exists(static frame => frame == nameof(IfcGlobalOrLocalEnum.LOCAL_COORDS))
            ? ExcelCoordinateSystem.Local : ExcelCoordinateSystem.Global;
        Option<double> station = attrs.Find(StructuralRows.Station)
            .Bind(static value => value is PropertyValue.Measure m ? Some(m.Value.Si) : None);
        return LoadTypeOf(attrs) switch {
            nameof(IfcStructuralLoadSingleForce) =>
                Seq((IExcelModuleObject)new ExcelStructuralPointAction {
                    Name = caseName, LoadCase = caseName, CoordinateSystem = system,
                    Direction = ExcelActionDirection.Vector,
                    DirectionVector = VectorOf(attrs, StructuralRows.Force, Force.FromNewtons),
                    ReferenceNode = onNode ? host : null, ReferenceMember = onNode ? null : host,
                    CoordinateDefinition = onNode ? null : ExcelCoordinateDefinition.Relative,
                    PositionX = onNode ? null : station.Match(Some: static at => (object)at, None: static () => null),
                    Origin = onNode ? null : ExcelOrigin.FromStart,
                })
                + Moments(attrs, host, onNode, caseName, system, station),
            nameof(IfcStructuralLoadLinearForce) =>
                Seq((IExcelModuleObject)new ExcelStructuralCurveAction {
                    Name = caseName, Member = host, LoadCase = caseName, CoordinateSystem = system,
                    Distribution = ExcelCurveDistribution.Uniform, Direction = ExcelActionDirection.Vector,
                    DirectionVector = VectorOf(attrs, StructuralRows.Force, ForcePerLength.FromNewtonsPerMeter),
                })
                + StructuralRows.Axes.Filter(axis => attrs.ContainsKey(StructuralRows.Moment[axis]))
                    .Map(axis => (IExcelModuleObject)new ExcelStructuralCurveMoment {
                        Name = caseName, Member = host, LoadCase = caseName, CoordinateSystem = system,
                        Distribution = ExcelCurveDistribution.Uniform,
                        Direction = axis switch { "X" => ExcelMomentDirection.Mx, "Y" => ExcelMomentDirection.My, _ => ExcelMomentDirection.Mz },
                        Value1 = TorquePerLength.FromNewtonMetersPerMeter(Si(attrs, StructuralRows.Moment[axis])),
                    }),
            nameof(IfcStructuralLoadConfiguration) =>
                Seq((IExcelModuleObject)new ExcelStructuralCurveAction {
                    Name = caseName, Member = host, LoadCase = caseName, CoordinateSystem = system,
                    Distribution = ExcelCurveDistribution.Trapezoidal, Direction = ExcelActionDirection.Vector,
                    DirectionVector = VectorOf(attrs, StructuralRows.Start, ForcePerLength.FromNewtonsPerMeter),
                    DirectionVector2 = VectorOf(attrs, StructuralRows.End, ForcePerLength.FromNewtonsPerMeter),
                    CoordinateDefinition = ExcelCoordinateDefinition.Absolute, Origin = ExcelOrigin.FromStart,
                    StartPoint = Si(attrs, SpanStart), EndPoint = Si(attrs, SpanEnd),
                }),
            nameof(IfcStructuralLoadPlanarForce) =>
                StructuralRows.Axes.Filter(axis => attrs.ContainsKey(StructuralRows.PlanarForce[axis]))
                    .Map(axis => (IExcelModuleObject)new ExcelStructuralSurfaceAction {
                        Name = caseName, Member2DReference = host, LoadCase = caseName, CoordinateSystem = system,
                        Direction = axis switch { "X" => ExcelActionDirection.X, "Y" => ExcelActionDirection.Y, _ => ExcelActionDirection.Z },
                        Value = Pressure.FromPascals(Si(attrs, StructuralRows.PlanarForce[axis])),
                    }),
            nameof(IfcStructuralLoadTemperature) =>
                Seq((IExcelModuleObject)new ExcelStructuralCurveActionThermal {
                    Name = caseName, Member = host, LoadCase = caseName,
                    DeltaT = Temperature.FromKelvins(Si(attrs, StructuralRows.DeltaT["Constant"])),
                }),
            _ => Seq<IExcelModuleObject>(),
        };
    }

    private static Seq<IExcelModuleObject> Moments(
        Map<PropertyName, PropertyValue> attrs, string host, bool onNode, string caseName,
        ExcelCoordinateSystem system, Option<double> station) =>
        StructuralRows.Axes.Filter(axis => attrs.ContainsKey(StructuralRows.Moment[axis]))
            .Map(axis => (IExcelModuleObject)new ExcelStructuralPointMoment {
                Name = caseName, LoadCase = caseName, CoordinateSystem = system,
                Direction = axis switch { "X" => ExcelMomentDirection.Mx, "Y" => ExcelMomentDirection.My, _ => ExcelMomentDirection.Mz },
                Value = Torque.FromNewtonMeters(Si(attrs, StructuralRows.Moment[axis])),
                ReferenceNode = onNode ? host : null, ReferenceMember = onNode ? null : host,
                CoordinateDefinition = onNode ? null : ExcelCoordinateDefinition.Relative,
                PositionX = onNode ? null : station.Match(Some: static at => (object)at, None: static () => null),
                Origin = onNode ? null : ExcelOrigin.FromStart,
            });

    // The SAF predefined-support cell derived from the six verdicts it summarizes — Fixed, Hinged, and Sliding
    // exactly on the triples the SAF vocabulary defines, Custom for every other shape — so the cell never asserts
    // a named condition the DOF rows contradict.
    private static ExcelBoundaryNodeCondition Predefined(Map<PropertyName, PropertyValue> attrs) {
        Seq<ExcelConstraintType?> translations = StructuralRows.Axes.Map(axis => Constraint(attrs, StructuralRows.Translation[axis]).Type);
        Seq<ExcelConstraintType?> rotations = StructuralRows.Axes.Map(axis => Constraint(attrs, StructuralRows.Rotation[axis]).Type);
        return (translations, rotations) switch {
            _ when translations.ForAll(static t => t == ExcelConstraintType.Rigid)
                && rotations.ForAll(static r => r == ExcelConstraintType.Rigid) => ExcelBoundaryNodeCondition.Fixed,
            _ when translations.ForAll(static t => t == ExcelConstraintType.Rigid)
                && rotations.ForAll(static r => r == ExcelConstraintType.Free) => ExcelBoundaryNodeCondition.Hinged,
            _ when translations[0] == ExcelConstraintType.Free && translations[1] == ExcelConstraintType.Free
                && translations[2] == ExcelConstraintType.Rigid
                && rotations.ForAll(static r => r == ExcelConstraintType.Free) => ExcelBoundaryNodeCondition.Sliding,
            _ => ExcelBoundaryNodeCondition.Custom,
        };
    }

    // ONE DOF row → the SAF constraint pair: the Boolean verdict lowers Rigid/Free, the Measure verdict Flexible
    // beside its SI spring re-minted through the quantity's own From* factory — the seam's single-row custody
    // arriving intact on SAF's split Type/Stiffness columns, an absent row an unset cell.
    private static (ExcelConstraintType? Type, double Si) Constraint(Map<PropertyName, PropertyValue> attrs, PropertyName dof) =>
        attrs.Find(dof).Match(
            Some: static value => value switch {
                PropertyValue.Boolean b => (b.Value ? ExcelConstraintType.Rigid : ExcelConstraintType.Free, 0d),
                PropertyValue.Measure m => ((ExcelConstraintType?)ExcelConstraintType.Flexible, m.Value.Si),
                _ => ((ExcelConstraintType?)null, 0d),
            },
            None: static () => ((ExcelConstraintType?)null, 0d));

    private static TQuantity? Spring<TQuantity>(
        Map<PropertyName, PropertyValue> attrs, PropertyName dof, Func<double, TQuantity> mint) where TQuantity : struct, IQuantity =>
        Constraint(attrs, dof) is { Type: ExcelConstraintType.Flexible, Si: var si } ? mint(si) : null;

    private static ExcelLoadDirectionVector<TQuantity> VectorOf<TQuantity>(
        Map<PropertyName, PropertyValue> attrs, Map<string, PropertyName> family, Func<double, TQuantity> mint)
        where TQuantity : struct, IQuantity =>
        new() { X = mint(Si(attrs, family["X"])), Y = mint(Si(attrs, family["Y"])), Z = mint(Si(attrs, family["Z"])) };

    // The entity-bag read: every PropertySet bag assigned to the owner folds into one map — the same
    // Assign.PropertyDefinition walk the seam Bake takes, so this leg reads the bags the Attrs entity arms landed.
    private static Map<PropertyName, PropertyValue> BagOf(ElementGraph graph, NodeId owner) =>
        toSeq(graph.EdgesAt(owner))
            .Choose(edge => edge is Relationship.Assign assign
                && assign.Subject == owner && assign.SubKind == AssignKind.PropertyDefinition
                ? Some(assign.Definition) : None)
            .Choose(definition => graph.Find(definition).Bind(static node => node is Node.PropertySet set ? Some(set.Bag.Values) : None))
            .Fold(Map<PropertyName, PropertyValue>(), static (folded, values) => folded.AddRange(values.ToSeq()));

    private static Option<Vector3> VertexOf(Node.Object node, ResolveAxis resolve) =>
        (node.Representations.Find("Vertex") | node.Representations.Find("Reference"))
            .Bind(contentKey => resolve(contentKey))
            .Bind(static polyline => polyline.Head);

    private static string Host(Map<NodeId, Node.Object> objects, NodeId id) =>
        objects.Find(id).Map(SafName).IfNone(id.Value);

    private static string SafName(Node.Object node) => node.Name.Length > 0 ? node.Name : node.Id.Value;

    // SAF's Id is a Guid: the 32-hex NodeId re-keys verbatim through the exact "N" parse; a non-hex identity keeps
    // Guid.Empty and the NAME stays the join key SAF resolves references by — SAF references are name-strung, so no
    // cross-reference rides the Guid.
    private static Guid GuidOf(Node.Object node) =>
        Guid.TryParseExact(node.Id.Value, "N", out Guid id) ? id : Guid.Empty;

    private static Option<string> Text(PropertyValue value) => value is PropertyValue.Text text ? Some(text.Value) : None;

    // The import leg: the ExcelModel AUTHORS the GeometryGym structural-analysis entities on the target database —
    // nodes, members, supports, releases, cases, combinations, actions — and the ONE SemanticProjector then ingests
    // that database, so the SAF wire re-enters through the exact fold the IFC wire takes and no second projector
    // mints member nodes. TOTAL and residue-HONEST on the Author idiom: the return is the SAF object types and rows
    // this authoring did NOT carry, folded into the codec's fidelity receipt — the surface-connection SUBSOIL and
    // point-support-deformation rows (GeometryGym's face condition and displacement components are sealed internal
    // fields with no public authoring path), the rigid-link/rigid-member/rigid-cross relations (no IFC counterpart
    // entity), and every directional or non-linear constraint DEGRADED to its linear base (named per row) — never a
    // silent drop. The GG ctor is the one throwing seam and crosses as BimFault.CodecReject.
    public static Fin<Seq<string>> Author(DatabaseIfc db, IfcSpatialElement host, ExcelModel model, Op key) =>
        Try.lift(() => {
            IfcStructuralAnalysisModel analysis = new(host, "SAF", IfcAnalysisModelTypeEnum.LOADING_3D);
            Seq<string> residue = Seq<string>();
            Map<string, IfcStructuralPointConnection> nodes = toMap(model.Objects.OfType<ExcelStructuralPointConnection>()
                .Select(point => (point.Name, new IfcStructuralPointConnection(analysis, new IfcVertexPoint(new IfcCartesianPoint(
                    db, point.X?.Meters ?? 0d, point.Y?.Meters ?? 0d, point.Z?.Meters ?? 0d))) { Name = point.Name })));
            Map<string, IfcStructuralCurveMember> members = toMap(model.Objects.OfType<ExcelStructuralCurveMember>()
                .Select(static (row, ordinal) => (Row: row, Ordinal: ordinal))
                .Choose(pair => (
                    from a in Optional(pair.Row.NodeStartName).Bind(name => nodes.Find(name))
                    from b in Optional(pair.Row.NodeEndName).Bind(name => nodes.Find(name))
                    select (pair.Row.Name, new IfcStructuralCurveMember(analysis, a, b, new IfcDirection(db, 0, 0, 1), pair.Ordinal + 1) {
                        Name = pair.Row.Name,
                        PredefinedType = Optional(pair.Row.Behaviour)
                            .Bind(VarietyElect.Find)
                            .IfNone(IfcStructuralCurveMemberTypeEnum.RIGID_JOINED_MEMBER),
                        ObjectType = Optional(pair.Row.Type).Bind(static type => type.IsOther ? Some(type.ToString()) : None).IfNone(""),
                    }))));
            // Surface members: the SAF outline nodes close an IfcPolyLoop on a plane through the loop — the
            // analytical face, not a display body — with the constant thickness; a varying thickness row degrades
            // to its first value, the named residue below.
            Map<string, IfcStructuralSurfaceMember> surfaces = toMap(model.Objects.OfType<ExcelStructuralSurfaceMember>()
                .Select(static (row, ordinal) => (Row: row, Ordinal: ordinal))
                .Choose(pair => toSeq(pair.Row.Nodes ?? []).TraverseM(name => nodes.Find(name)).As()
                    .Filter(static corners => corners.Count >= 3)
                    .Map(corners => {
                        Seq<IfcCartesianPoint> outline = corners.Map(static corner => (IfcCartesianPoint)((IfcVertexPoint)corner.Vertex).VertexGeometry);
                        return (pair.Row.Name, new IfcStructuralSurfaceMember(
                            analysis,
                            new IfcFaceSurface(
                                new IfcFaceOuterBound(new IfcPolyLoop(outline), true),
                                new IfcPlane(new IfcAxis2Placement3D(outline[0])), true),
                            new IfcMaterial(db, pair.Row.Material ?? ""), pair.Ordinal + 1,
                            pair.Row.Thickness?.ThicknessFirst?.Meters ?? 0d) { Name = pair.Row.Name });
                    })));
            foreach (ExcelStructuralPointSupport support in model.Objects.OfType<ExcelStructuralPointSupport>()) {
                nodes.Find(support.Node ?? "").IfSome(connection => {
                    connection.AppliedCondition = new IfcBoundaryNodeCondition(db, support.Name ?? "",
                        Translational(support.TranslationXType, support.TranslationXStiffness?.NewtonsPerMeter),
                        Translational(support.TranslationYType, support.TranslationYStiffness?.NewtonsPerMeter),
                        Translational(support.TranslationZType, support.TranslationZStiffness?.NewtonsPerMeter),
                        Rotational(support.RotationXType, support.RotationXStiffness?.NewtonMetersPerRadian),
                        Rotational(support.RotationYType, support.RotationYStiffness?.NewtonMetersPerRadian),
                        Rotational(support.RotationZType, support.RotationZStiffness?.NewtonMetersPerRadian));
                    residue += Degraded(support.Name ?? "", Seq(
                        support.TranslationXType, support.TranslationYType, support.TranslationZType,
                        support.RotationXType, support.RotationYType, support.RotationZType));
                });
            }
            foreach (ExcelRelConnectsStructuralMember hinge in model.Objects.OfType<ExcelRelConnectsStructuralMember>()) {
                Option<ExcelStructuralCurveMember> row = toSeq(model.Objects.OfType<ExcelStructuralCurveMember>())
                    .Filter(candidate => candidate.Name == hinge.Member).Head;
                Seq<string> ends = hinge.Position switch {
                    ExcelPosition.Both => row.ToSeq().Bind(static r => Seq(r.NodeStartName ?? "", r.NodeEndName ?? "")),
                    ExcelPosition.End => row.ToSeq().Map(static r => r.NodeEndName ?? ""),
                    _ => row.ToSeq().Map(static r => r.NodeStartName ?? ""),
                };
                foreach (string end in ends) {
                    (members.Find(hinge.Member ?? ""), nodes.Find(end)).Apply((member, connection) => {
                        _ = new IfcRelConnectsStructuralMember(member, connection) {
                            AppliedCondition = new IfcBoundaryNodeCondition(db, hinge.Name ?? "",
                                Translational(hinge.TranslationXType, hinge.TranslationXStiffness?.NewtonsPerMeter),
                                Translational(hinge.TranslationYType, hinge.TranslationYStiffness?.NewtonsPerMeter),
                                Translational(hinge.TranslationZType, hinge.TranslationZStiffness?.NewtonsPerMeter),
                                Rotational(hinge.RotationXType, hinge.RotationXStiffness?.NewtonMetersPerRadian),
                                Rotational(hinge.RotationYType, hinge.RotationYStiffness?.NewtonMetersPerRadian),
                                Rotational(hinge.RotationZType, hinge.RotationZStiffness?.NewtonMetersPerRadian)),
                        };
                        return unit;
                    });
                }
                residue += Degraded(hinge.Name ?? "", Seq(
                    hinge.TranslationXType, hinge.TranslationYType, hinge.TranslationZType,
                    hinge.RotationXType, hinge.RotationYType, hinge.RotationZType));
            }
            Map<string, IfcStructuralLoadCase> cases = toMap(model.Objects.OfType<ExcelStructuralLoadCase>()
                .Select(row => (row.Name, new IfcStructuralLoadCase(analysis, row.Name) {
                    ActionType = row.ActionType switch {
                        ExcelActionType.Permanent => IfcActionTypeEnum.PERMANENT_G,
                        ExcelActionType.Accidental => IfcActionTypeEnum.EXTRAORDINARY_A,
                        ExcelActionType.Variable => IfcActionTypeEnum.VARIABLE_Q,
                        _ => IfcActionTypeEnum.NOTDEFINED,
                    },
                    ActionSource = Optional(row.LoadType).Bind(SourceOf.Find).IfNone(IfcActionSourceTypeEnum.NOTDEFINED),
                })));
            foreach (ExcelStructuralLoadCombination combination in model.Objects.OfType<ExcelStructuralLoadCombination>()) {
                _ = new IfcStructuralLoadGroup(analysis, combination.Name,
                    toSeq(combination.LoadFactors ?? []).Map(static factor => factor ?? 1d).ToList(),
                    toSeq(combination.LoadCases ?? []).Choose(cases.Find).Map(static loadCase => (IfcStructuralLoadGroup)loadCase).ToList(),
                    ULS: combination.Category == ExcelLoadCaseCombinationCategory.UltimateLimitState);
            }
            foreach (ExcelStructuralPointAction action in model.Objects.OfType<ExcelStructuralPointAction>()) {
                (cases.Find(action.LoadCase ?? ""), nodes.Find(action.ReferenceNode ?? "")).Apply((loadCase, at) => {
                    _ = new IfcStructuralPointAction(loadCase, at,
                        new IfcStructuralLoadSingleForce(db,
                            action.DirectionVector?.X?.Newtons ?? 0d, action.DirectionVector?.Y?.Newtons ?? 0d, action.DirectionVector?.Z?.Newtons ?? 0d),
                        action.CoordinateSystem != ExcelCoordinateSystem.Local);
                    return unit;
                });
            }
            foreach (ExcelStructuralCurveAction action in model.Objects.OfType<ExcelStructuralCurveAction>()) {
                (cases.Find(action.LoadCase ?? ""), members.Find(action.Member ?? "")).Apply((loadCase, member) => {
                    IfcStructuralLoadLinearForce Force(ExcelLoadDirectionVector<ForcePerLength>? vector) => new(db,
                        vector?.X?.NewtonsPerMeter ?? 0d, vector?.Y?.NewtonsPerMeter ?? 0d, vector?.Z?.NewtonsPerMeter ?? 0d, 0d, 0d, 0d);
                    _ = action.Distribution == ExcelCurveDistribution.Trapezoidal
                        ? new IfcStructuralCurveAction(loadCase, member,
                            new IfcStructuralLoadConfiguration(
                                Force(action.DirectionVector), action.StartPoint as double? ?? 0d,
                                Force(action.DirectionVector2), action.EndPoint as double? ?? 1d),
                            action.CoordinateSystem != ExcelCoordinateSystem.Local, action.Location == ExcelLocation.Projection,
                            IfcStructuralCurveActivityTypeEnum.LINEAR)
                        : new IfcStructuralCurveAction(loadCase, member, Force(action.DirectionVector),
                            action.CoordinateSystem != ExcelCoordinateSystem.Local, action.Location == ExcelLocation.Projection,
                            IfcStructuralCurveActivityTypeEnum.CONST);
                    return unit;
                });
            }
            foreach (ExcelStructuralSurfaceAction action in model.Objects.OfType<ExcelStructuralSurfaceAction>()) {
                (cases.Find(action.LoadCase ?? ""), surfaces.Find(action.Member2DReference ?? "")).Apply((loadCase, surface) => {
                    double magnitude = action.Value?.Pascals ?? 0d;
                    IfcStructuralLoadPlanarForce load = new(db) {
                        PlanarForceX = action.Direction == ExcelActionDirection.X ? magnitude : 0d,
                        PlanarForceY = action.Direction == ExcelActionDirection.Y ? magnitude : 0d,
                        PlanarForceZ = action.Direction is ExcelActionDirection.Z or null ? magnitude : 0d,
                    };
                    _ = new IfcStructuralSurfaceAction(loadCase, surface, load,
                        action.CoordinateSystem != ExcelCoordinateSystem.Local, projected: false,
                        IfcStructuralSurfaceActivityTypeEnum.CONST);
                    return unit;
                });
            }
            foreach (ExcelStructuralCurveActionThermal action in model.Objects.OfType<ExcelStructuralCurveActionThermal>()) {
                (cases.Find(action.LoadCase ?? ""), members.Find(action.Member ?? "")).Apply((loadCase, member) => {
                    _ = new IfcStructuralCurveAction(loadCase, member,
                        new IfcStructuralLoadTemperature(db, action.DeltaT?.Kelvins ?? 0d, 0d, 0d),
                        global: true, projected: false, IfcStructuralCurveActivityTypeEnum.CONST);
                    return unit;
                });
            }
            return residue + Unmapped(model);
        }).Run().MapFail(error => new BimFault.CodecReject(key, $"saf-author:{error.Message}"));

    // The named authoring negatives: sealed-at-the-source payloads (face-condition subsoil, displacement
    // components), IFC-counterpartless relations, and the SAF result tables an authoring leg never carries — one
    // residue row per PRESENT object type, so an absent table names nothing.
    private static Seq<string> Unmapped(ExcelModel model) =>
        Seq(nameof(ExcelStructuralSurfaceConnection), nameof(ExcelStructuralPointSupportDeformation),
            nameof(ExcelRelConnectsRigidLink), nameof(ExcelRelConnectsRigidMember), nameof(ExcelRelConnectsRigidCross),
            nameof(ExcelResultInternalForce1D), nameof(ExcelResultInternalForce2D))
            .Filter(type => model.Objects.Exists(row => row.GetType().Name == type));

    private static Seq<string> Degraded(string name, Seq<ExcelConstraintType?> constraints) =>
        constraints.Exists(static constraint => constraint is ExcelConstraintType.CompressionOnly or ExcelConstraintType.TensionOnly
            or ExcelConstraintType.NonLinear or ExcelConstraintType.FlexibleCompressionOnly or ExcelConstraintType.FlexibleTensionOnly)
            ? Seq($"constraint-linearized:{name}")
            : Seq<string>();

    // The SAF constraint pair → the GG DOF select, the exact inverse of the export Constraint read; a directional
    // or non-linear constraint authors its LINEAR base — rigid for the rigid-acting kinds, its spring for the
    // flexible-acting kinds — and the Degraded scan above names every such row in the residue, so the
    // linearization is counted, never silent.
    private static IfcTranslationalStiffnessSelect Translational(ExcelConstraintType? constraint, double? si) => constraint switch {
        ExcelConstraintType.Flexible or ExcelConstraintType.FlexibleCompressionOnly or ExcelConstraintType.FlexibleTensionOnly
            when si is > 0d => new IfcTranslationalStiffnessSelect(si.Value),
        ExcelConstraintType.Rigid or ExcelConstraintType.CompressionOnly or ExcelConstraintType.TensionOnly
            or ExcelConstraintType.NonLinear => new IfcTranslationalStiffnessSelect(true),
        _ => new IfcTranslationalStiffnessSelect(false),
    };

    private static IfcRotationalStiffnessSelect Rotational(ExcelConstraintType? constraint, double? si) => constraint switch {
        ExcelConstraintType.Flexible or ExcelConstraintType.FlexibleCompressionOnly or ExcelConstraintType.FlexibleTensionOnly
            when si is > 0d => new IfcRotationalStiffnessSelect(si.Value),
        ExcelConstraintType.Rigid or ExcelConstraintType.CompressionOnly or ExcelConstraintType.TensionOnly
            or ExcelConstraintType.NonLinear => new IfcRotationalStiffnessSelect(true),
        _ => new IfcRotationalStiffnessSelect(false),
    };
}
```

## [03]-[RESEARCH]

(none)
