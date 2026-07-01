# [MATERIALS_PANEL]

THE PANEL/SHEET-GOODS COMPONENTFAMILY and THE STEEL-DECK RIB-SECTION OWNER. The panel vocabulary — the `PanelKind` board-type discriminant (gypsum-board · gypsum-sheathing · plywood-sheathing · osb-sheathing · cement-board · steel-deck · rigid-board-insulation) carrying its verified IFC entity/predefined tokens and structural flag, the `EdgeProfile` board-edge axis (square · tapered · beveled · rounded · tongue-groove · shiplap · side-lap-interlock), the `PanelLayerRole` face/core stack axis (paper-face · glass-mat-facer · gypsum-core · veneer-ply · strand-layer · cement-aggregate-core · scrim · foam-core · facer), the `PanelOrientation` strength-axis run direction (strength-axis-perpendicular · strength-axis-parallel-to-span · unidirectional), the `FastenerType` board-fastener axis (drywall-screw · structural-nail · roofing-nail · deck-weld · deck-screw · plate-and-screw · adhesive), the `DeckForm`/`DeckProfile` steel-deck rib geometry axes, the `PanelLayer`/`FastenPattern`/`DeckRib` value-objects, and the `PanelSection` generative board payload — is the realized sheet-goods vocabulary one `component#COMPONENT_OWNER` `Component` carries in the `ComponentFamily.Panel` case, the TENTH family and a `ComponentClass.Panel` standardized type (an `IfcBuiltElement` covering/plate/slab: one standardized board TYPE laid as MANY pieces over a frame, distinct from the primary one-piece members and the minor `IfcElementComponent` parts). A `5/8in` Type-X gypsum board is a `Component` row, never a `DrywallBoard` type: the kind, the board length/width/thickness, the edge profile, the face/core layer stack, the fastening field+edge pattern, the orientation, and the deck rib geometry are panel-`Component` columns, and the `PanelSection` projection feeds the same `component#COMPONENT_OWNER` `Component.Of` admission and the same `ComponentCatalogue.Build` fold the timber (`timber#TIMBER_FAMILY`) and connector (`connector#CONNECTOR_FAMILY`) families drive — a sheathing run lays through the construction layout fold over one `Component`, never a per-family schedule owner. The buildable BOARD is the Component, the board SUBSTANCE a `properties#MATERIAL_PROPERTY_CATALOGUE` row: the gypsum/cement/wood/foam SUBSTANCE resolves to a `Properties` `MaterialId` (`gypsum.board`/`cement.board`/`wood.plywood`/`wood.osb`/`insulation.eps`/`insulation.xps`/`insulation.pir`), while the laid board with its dimensions, layup, edge, and fastening IS the Component — PRESERVE this split exactly, never a panel-substance `Component` and never a board `Properties` row. The panel vocabulary grows by data — a new board is one `PanelRow` catalogue entry, a new edge one `EdgeProfile` row, a new layer role one `PanelLayerRole` row, a new deck profile one `DeckProfile` row — never a per-board type. This owner is ALSO the host-neutral STEEL-DECK SECTION assembler: the structural steel-deck `PanelSection` projects a `DeckRib` trapezoidal corrugation feeding the SAME `VividOrange.Sections.SectionProperties` Green's-theorem polygon integral over a built `VividOrange.Profiles.Perimeter` the `component#COMPONENT_OWNER` `ParametricSection` runs for steel/cmu/timber, filling the canonical `ComputedSection` rib net section — mirroring how `connector#CONNECTOR_FAMILY` is the cold-formed-capacity assembler beside its family axis, NOT a hand-keyed `Sp`/`Sn`/`I` literal. VividOrange owns the structural-MEMBER section catalogues (AISC v16.0 + EN 10365) and EN grade data, NOT the sheet-goods dimension tables (no admitted .NET package owns gypsum/sheathing/deck/insulation board rosters), so the panel catalogue is HAND-ROLLED in-fence exactly as `fastener#FASTENER_FAMILY` hand-keys the ISO 898-1 bolt property classes — the ASTM/EN/APA/SDI board dimensions, edge profiles, span ratings, rib geometry, and R-values are the realized in-fence vocabulary, never a re-tabulated VividOrange surface. The page composes `component#COMPONENT_OWNER` for the `Component`/`ComponentId`/`ComponentSection`/`ComponentFault`/`ComponentUnit`/`ComputedSection`/`ParametricSection` shape, the `Rasm.Vectors` kernel `PositiveMagnitude` value-object for every board/rib/fastening length column (the discrete ply/rib count and the steel gauge ride the `PanelLayer`/`DeckRib` int columns and the connector family's `SteelGauge.GaugeNumber`, never a parallel `Dimension` re-admitting a tautologically-positive constant), the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` board-face appearance column each row carries, and the `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical`/`Thermal` capacity receipt the analysis seam reads by `MaterialId`, never re-derived here. The buildable board crosses the IFC wire as its kind-determined leaf — a space-bounding covering (gypsum/sheathing/cement-board/rigid-board) round-trips as `IfcCovering` with its `IfcCoveringTypeEnum` predefined (CLADDING/CEILING/FLOORING/INSULATION), a wood structural sheet as `IfcPlate`/SHEET, a steel roof/form deck as `IfcPlate`/SHEET and a composite floor deck as `IfcSlab`/FLOOR (assay-confirmed: `GeometryGym.Ifc.IfcDeck` does NOT exist, so the steel deck is never an `IfcDeck`), the `PanelKind.IfcEntity`/`IfcPredefinedType` tokens the `Rasm.Bim` egress gate reads to stamp the IFC entity class, never a free designation string the federation cannot round-trip.

## [01]-[INDEX]

- [02]-[PANEL_FAMILY]: the `PanelKind` board-type discriminant carrying its verified `IfcCovering`/`IfcPlate`/`IfcSlab` entity + `IfcCoveringTypeEnum`/`IfcPlateTypeEnum`/`IfcSlabTypeEnum` predefined tokens and structural flag, the `EdgeProfile`/`PanelLayerRole`/`PanelOrientation`/`DeckForm`/`DeckProfile`/`FastenerType` smart-enum axes, the `CoreType`/`SpanRating`/`BondClass`/`FoamType`/`Facer` board-property axes, the `PanelLayer`/`FastenPattern`/`DeckRib` value-objects, the `PanelSection` board payload with its `Layup`/`CrossNominalMm`/`GrossRectangleMm`/`ToUnit`/`RValue` projections, and the `ComponentCatalogue.BuildPanelRows` ASTM C1396 / APA PRP-108 / ASTM C1325 / SDI / ASTM C578-C1289 row table.
- [03]-[DECK_SECTION]: the steel-deck rib `ComputedSection` — the `PanelSection.DeckSection` projection building the trapezoidal corrugation `VividOrange.Profiles.Perimeter` over one rib pitch, feeding the `component#COMPONENT_OWNER` `ParametricSection.Admit` Green's-theorem polygon integral so a steel-deck row contributes a real net rib section to `ComponentCatalogue.Sections`, the M7 substrate a `Rasm.Compute` deck-diaphragm/composite-floor check reads off the seam without re-running the integral.

## [02]-[PANEL_FAMILY]

- Owner: the panel vocabulary (`PanelKind` the gypsum-board/gypsum-sheathing/plywood-sheathing/osb-sheathing/cement-board/steel-deck/rigid-board-insulation board-type discriminant carrying its `IfcEntity`/`IfcPredefinedType` tokens, structural flag, and `SubstanceId`; `EdgeProfile` the square/tapered/beveled/rounded/tongue-groove/shiplap/side-lap-interlock edge axis; `PanelLayerRole` the face/core stack axis; `PanelOrientation` the strength-axis run-direction axis; `DeckForm`/`DeckProfile` the steel-deck rib-geometry axes; `FastenerType` the board-fastener axis; `CoreType`/`SpanRating`/`BondClass`/`FoamType`/`Facer` the board-property axes; `PanelLayer`/`FastenPattern`/`DeckRib` the value-objects; `PanelSection` the generative board payload); `ComponentCatalogue.BuildPanelRows` the registered-row seed `component#COMPONENT_OWNER` `ComponentCatalogue.Build` folds; the `PanelSection.ToUnit` projection bridging the board to the canonical `ComponentUnit` the layout fold consumes and the `PanelSection.RValuePerInch`/`RValue` projections the rigid-board thermal seam reads.
- Cases: kind {gypsum-board (the interior wall/ceiling lining, an `IfcCovering`/CLADDING, drywall-screw-fastened, a paper-face/gypsum-core/paper-face layup, `gypsum.board` substance) · gypsum-sheathing (the exterior gypsum board, an `IfcCovering`/CLADDING, glass-mat-faced) · plywood-sheathing (the APA Rated wood structural panel, an `IfcPlate`/SHEET, veneer-ply layup, `wood.plywood` substance, span-rated) · osb-sheathing (the oriented strand board, an `IfcPlate`/SHEET, strand-layer layup, `wood.osb` substance, span-rated) · cement-board (the fiber-cement backer board, an `IfcCovering`/CLADDING, glass-scrim/cement-aggregate-core layup, `cement.board` substance) · steel-deck (the SDI roof/form/composite deck, an `IfcPlate`/SHEET for roof/form and `IfcSlab`/FLOOR for composite, a structural ribbed sheet carrying a `DeckRib`) · rigid-board-insulation (the EPS/XPS/polyiso continuous insulation, an `IfcCovering`/INSULATION, facer/foam-core/facer layup, `insulation.eps`/`xps`/`pir` substance, R-value-rated)} · edge {square · tapered · beveled · rounded · tongue-groove · shiplap · side-lap-interlock} · orientation {strength-axis-perpendicular · strength-axis-parallel-to-span · unidirectional} — a panel is a `Component` row over one `PanelKind`, one `EdgeProfile`, one `PanelOrientation`, and (where structural) one `DeckRib`, never a board subtype.
- Entry: `public Fin<ComponentUnit> ToUnit(Context context, Op key)` on `PanelSection` — the section→`ComponentUnit` projection so a sheathing run flows through the SAME `Construction/layout#ASSEMBLY_FOLD` `Resolve` fold the timber/masonry families drive (`WidthMm` the board width, `LengthMm` the board length the run direction reads, `CourseHeightMm` the board width so the sheathing stage steps board-to-board, the `ComponentSection.Panel` arm's `ToUnit` dispatch delegating to this leaf); `public Seq<PanelLayer> Layup` the face/core stack the `Construction/assembly#MATERIAL_COMPOSITION` `CompositionAuthor.LayerSet` coerces into the seam `IfcMaterialLayerSet`; `public double RValuePerInch` / `public double RValue` the rigid-board thermal projections (`ASTM C578`/`C1289` design `R` over the board thickness in inches) the building-envelope seam reads; `ComponentCatalogue.BuildPanelRows(context)` folds the ASTM/APA/SDI/EN `PanelRow` table through `PanelOf` into the registered `Component` rows `ComponentCatalogue.Build` concatenates, and `ComponentCatalogue.PanelSections(context)` folds the steel-deck rows' rib `ComputedSection` into the section map `ComponentCatalogue.Sections` concatenates — one polymorphic catalogue fold, never a `GetBoardByKind`/`GetByThickness` family.
- Packages: Rasm.Vectors (project — `PositiveMagnitude` for the board length/width/thickness, the fastening field/edge spacing and edge distance, and the deck rib depth/pitch columns, never an int-backed `Dimension` that truncates a fractional `12.7 mm` thickness or a `152.4 mm` edge spacing; the discrete ply/rib count rides the `PanelLayer`/`DeckRib` int columns and the steel gauge the `Connector.SteelGauge.GaugeNumber`, never a parallel stored `Dimension` column re-admitting a tautologically-positive constant), Rasm.Domain (project — `Op` the boundary-admission key, the `AcceptValidated` admission extension, `Context`), Rasm.Element (project — `MaterialId` the appearance/capacity/substance rows reference, `ProfileRef` + `SectionProperties` the seam handle + neutral receipt the steel-deck rib section lifts onto), VividOrange.Sections.SectionProperties + VividOrange.Profiles.Perimeter + VividOrange.Geometry (the steel-deck rib-section bridge — `new Perimeter(outerEdge, voidEdges)` over `LocalPolyline2d`/`LocalPoint2d` fed to `new SectionProperties(IProfile)` through the `component#COMPONENT_OWNER` `ParametricSection.Admit`, the SAME Green's-theorem solver steel/cmu/timber run, the `Rasm.Materials/.api` `vividorange-sections-sectionproperties`/`vividorange-profiles-catalogue` catalogues), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` for the kind/edge/orientation/deck-form/deck-profile/fastener/core/span/bond/foam/facer axes with the generated total `Switch`, `[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]` for the catalogue key; the `FastenPattern`/`PanelLayer`/`DeckRib` value-objects are `readonly record struct`s with `Fin`-railed `Of` admission over the kernel `PositiveMagnitude`; the `[Union]` is the parent `ComponentSection.Panel` arm), LanguageExt.Core (`Fin`/`Seq`/`Choose`/`Fold`/`Map`/`toSeq`/`guard` for the admission rail and the catalogue fold), BCL inbox (`FrozenDictionary`). No sheet-goods external package — VividOrange owns the structural-MEMBER section catalogue and EN grade data, NOT the gypsum/sheathing/deck/insulation board rosters, so the board dimension/edge/span/rib/R-value bands are the realized in-fence vocabulary, hand-rolled exactly as `fastener#FASTENER_FAMILY` hand-keys the ISO 898-1 bolt property classes; the IFC entity/predefined enum members are GeometryGym-verified at the `Rasm.Bim` egress (the `Rasm.Bim` folder `.api` `api-geometrygym-ifc`), the token a plain `string` here.
- Growth: the panel vocabulary grows by data — a new board is one `PanelRow` catalogue entry carrying its kind/width/length/thickness/edge/orientation/fastening columns, a new edge one `EdgeProfile` row, a new layer role one `PanelLayerRole` row, a new deck profile one `DeckProfile` row carrying its rib depth/pitch, a new core/span/bond/foam/facer band one row on the matching axis — never a per-board type, never a per-kind `Component` variant. A fiberboard or a magnesium-oxide board is a new `PanelKind` row reusing the `IfcCovering`/CLADDING covering token and an existing layup, so a new sheet good shares the covering wire without a parallel entity axis; a new fastening schedule is one `FastenPattern` value-object growth shared by every kind, never a per-board fastener type; the steel-deck rib geometry grows by one `DeckProfile` row, the rib `ComputedSection` re-derived through the SAME `ParametricSection` integral. The reinforcement/fastener/connector/joint and the profiled steel/timber/glazing/masonry/cmu families carry their own vocabularies on their own pages; the panel family is the deliberate sheet-goods widening (a laid board over a frame, neither a one-piece member nor a discrete part), so the `ComponentFamily` axis closes at TEN, never an eleventh sibling. A panel is laid through the `Construction/layout#ASSEMBLY_FOLD` `Sheathing` stage over the SAME `Resolve` fold (the family axis grows at `component#COMPONENT_OWNER`, so a panel run is the same fold over a panel `Component`/`ComponentUnit`), never a per-family layout method.
- Boundary: the panel vocabulary is the sheet-goods `ComponentFamily` arm and the THIRD `ComponentClass` — a per-board `DrywallBoard`/`Sheathing`/`SteelDeck`/`RigidBoard` class is the deleted form, collapsed into the one `PanelKind` `[SmartEnum]` arm (the 3+-parallel-shape collapse trigger), and the board is a `ComponentClass.Panel` standardized type (an `IfcBuiltElement` covering/plate/slab — assay-confirmed `IfcCovering : IfcBuiltElement`, `IfcPlate : IfcBuiltElement`, `IfcSlab : IfcBuiltElement`), one type laid as MANY pieces, NEVER a primary one-piece member or a minor `IfcElementComponent` part; `PanelSection` composes the `Rasm.Vectors` kernel `PositiveMagnitude` (the double-backed `> 0` finite magnitude) for every length column so a fractional board thickness (`6.4 mm` 1/4in, `9.5 mm` 3/8in, `12.7 mm` 1/2in, `15.9 mm` 5/8in), a fractional fastening spacing (`152.4 mm` 6in edge, `304.8 mm` 12in field, `203.2 mm` 8in), a fractional rib depth (`38.1 mm` 1.5in, `76.2 mm` 3in), and a fractional base-metal thickness (`0.75 mm` 22ga) admit without the truncation an int-backed `Dimension` count forces, the discrete ply/rib count riding the `PanelLayer` stack length and the `DeckRib.RibCountPerCoverage` int, the steel gauge the `connector#CONNECTOR_FAMILY` `SteelGauge.GaugeNumber` (a SmartEnum constant is already a discrete count, so a parallel stored `Dimension` re-admitting it is the dead-column form); the buildable BOARD is the Component and the board SUBSTANCE a `properties#MATERIAL_PROPERTY_CATALOGUE` row — `PanelKind.SubstanceId` is the `MaterialId` whose `Mechanical`/`Thermal` row the analysis seam reads (`gypsum.board`/`cement.board`/`wood.plywood`/`wood.osb`/`insulation.eps`/`xps`/`pir`), the gypsum/insulation rows already in the catalogue and the cement/OSB/plywood rows the deepened catalogue grows beside them, so a board's substance physics are read once from the property library, NEVER re-keyed here and NEVER a panel-substance `Component`; the layup is a TYPED face/core stack — `PanelLayer` carries each layer's role, thickness, and `MaterialId` so a generator builds the real layered board (a paper-face/gypsum-core/paper-face drywall, a glass-mat-facer/gypsum-core/glass-mat-facer sheathing, a veneer-ply plywood, a glass-scrim/cement-core/glass-scrim cement board, a facer/foam-core/facer rigid board), the `CompositionAuthor.LayerSet` coercing the stack into the seam `IfcMaterialLayerSet` exactly as a CLT panel's `PlyLayup` and an IGU's pane stack do, NEVER a single-thickness scalar smear forcing a five-layer board into one column; the fastening is a TYPED schedule — `FastenPattern` carries the field spacing and edge spacing as kernel `PositiveMagnitude` (admitted once through `FastenPattern.Of`), the edge distance, and the `FastenerType` so a generator places the real fastener stations (the field+edge fastener grid the `Construction/layout#ASSEMBLY_FOLD` `SheetCoursing` stage steps as `Placement` rows), NEVER a free nail count; the steel-deck rib is a TYPED corrugation — `DeckRib` carries the form (form/composite/roof), the rib depth, the rib pitch, the profile (wide-rib-B/narrow-rib-A/intermediate-F/deep-N/dovetail), and the coverage width so the rib `ComputedSection` ([03]) is a real net trapezoidal section over a built `Perimeter`, NEVER a hand-keyed `Sp`/`Sn`/`I` literal; the appearance assignment crosses to `Appearance/graph#MATERIAL_LIBRARY` as the `MaterialId` (`gypsum.board` for a paper-faced board, `metal.steel` for galvanized deck, `cement.board`/`wood.plywood`/`wood.osb`/`insulation.eps` for the matching face) the row's `Component.AppearanceId` column carries, never a board-specific shade; the `AppearanceId` and the `CapacityKey`/`SubstanceId` are INDEPENDENT `MaterialId` slots (the `component#COMPONENT_OWNER` two-slot law), a foil-faced polyiso keeping its foam `SubstanceId` while its `AppearanceId` names a metallic-facer finish; `ComponentCatalogue.BuildPanelRows` seeds the `component#COMPONENT_OWNER` `ComponentCatalogue.Rows` table with the ASTM/APA/SDI/EN board rows keyed `panel.<designation>` (`panel.gyp-x-625-4x8`, `panel.ply-rated-1532-4x8-2416`, `panel.deck-b-20ga-roof`, `panel.polyiso-2in-foil`) under the `ComponentId` `family.designation` format, the dimensional columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` so a malformed row drops through `Choose` rather than seeding a degenerate `Component`; the placement of a sheathing run reads `Construction/layout#ASSEMBLY_FOLD` `StationStep` over the SAME realized fold (the new `Sheathing` `LayoutStage` row dispatching the board-laying kernel), never a parallel panel-layout owner; the panel reaches `Rasm.Bim` through the `Projection/component#COMPONENT_PROJECTOR` `ComponentProjector` (the one Component projection, the owner-mints-its-identity law — Materials owns Types, so the projection mints the deterministic-rooted Type `Object` node stamping the kind-determined `IfcCovering`/`IfcPlate`/`IfcSlab` class + the `PanelKind.IfcPredefinedType` token, occurrences binding via `Assign.TypeDefinition`) plus the seam-declared NEUTRAL detail bag `Projection/component#COMPONENT_PROJECTOR` `ComponentProjector.ProjectType` authors on the Type `Object` via one `Assign.PropertyDefinition` edge (the deck rib depth/pitch, the field/edge fastener spacing, and the board thickness/edge-profile a generator round-trips; the `Pset_*` IFC name a `Rasm.Bim` egress mapping, never authored here) — a `ComponentClass.Panel` board is an `IfcBuiltElement`, NOT a realizing element the `Semantics/connection#CONNECTION_DETAIL` reader discriminates, so the bag round-trips through the GENERAL Bim `Object`/property fold (the `Projection/egress#IFC_EGRESS` `ReauthorProperties` `IfcPropertySet` egress, the `Projection/semantic#SEMANTIC_PROJECTOR` `Bags` ingest merged by the seam `Bake` type-bag precedence), never the realizing-element connection reader; the IFC entity-class + `PredefinedType` resolved + validated at the `Rasm.Bim` `Emit` gate ([C6] — the predefined type is a Bim-owned egress concern), so this owner carries only the portable `PanelKind.IfcEntity`/`IfcPredefinedType` strings the projector lands and the detail rows a consumer recovers one-hop off the baked Type bag, NEVER an interior `IfcOpenShell` evaluation.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;     // FrozenDictionary (the registered-row + section tables)
using LanguageExt;
using Rasm.Vectors;                  // PositiveMagnitude (>0 finite — board/rib/fastening length columns) — the kernel value-object atoms live in Rasm.Vectors, NOT Rasm.Domain
using Rasm.Domain;                   // Op (the boundary-admission key), the AcceptValidated admission extension, Context
using Rasm.Element;                  // MaterialId (the appearance/capacity/substance rows), ProfileRef + SectionProperties (the seam handle + neutral receipt the deck rib section lifts onto)
using Rasm.Materials.Component;      // Component/ComponentId/ComponentFamily/ComponentClass/ComponentSection/ComponentFault/Coring/ComponentStandard/ComponentAuthority/ComputedSection/ParametricSection (the parent COMPONENT_OWNER)
using Rasm.Materials.Component.Connector;   // SteelGauge (the cold-formed gauge axis the steel-deck base metal reuses — never a parallel gauge enum)
using Thinktecture;
using static LanguageExt.Prelude;

// Each Component family page is its OWN Rasm.Materials.Component.<Family> sub-namespace so the sibling `ComponentCatalogue`
// static classes are distinct types (a shared Rasm.Materials.Component is a CS0101 collision with component.md's own
// `ComponentCatalogue`); component#COMPONENT_OWNER stays the parent Rasm.Materials.Component and folds
// Panel.ComponentCatalogue.BuildPanelRows / Panel.ComponentCatalogue.PanelSections by the sub-namespace-qualified name;
// parent types via the using above. The steel-deck base-metal gauge reuses the connector family's SteelGauge axis (one
// cold-formed sheet-steel gauge vocabulary under Rasm.Materials.Component, never a parallel deck gauge enum).
namespace Rasm.Materials.Component.Panel;

// --- [TYPES] -------------------------------------------------------------------------------
// PanelKind is the board-type axis grown by data, NOT a per-board class. IfcEntity/IfcPredefinedType are the VERIFIED
// GeometryGym leaf the Projection/component#COMPONENT_PROJECTOR lands on the seam Type Object node's Classification("ifc",
// IfcEntity) + PredefinedType (the THIRD projection mode — a kind-determined leaf, distinct from the four discrete-part
// families projecting a fixed leaf and the five profiled families projecting the ComponentClass supertype). assay-confirmed
// against GeometryGymIFCcore: a space-bounding covering (gypsum/sheathing/cement-board/rigid-board) is an IfcCovering whose
// IfcCoveringTypeEnum carries CEILING/CLADDING/FLOORING/INSULATION/ROOFING (NO SHEATHING member, so a partition lining and an
// exterior sheathing are both CLADDING, a ceiling board CEILING); a wood structural sheet is an IfcPlate whose
// IfcPlateTypeEnum carries SHEET; a steel ROOF/FORM deck is an IfcPlate/SHEET and a COMPOSITE FLOOR deck an IfcSlab whose
// IfcSlabTypeEnum carries FLOOR/ROOF (NO COMPOSITE/DECK member — and GeometryGym.Ifc.IfcDeck does NOT exist, so a deck is
// NEVER an IfcDeck). Structural flags the steel-deck rib (the only kind contributing a ComputedSection); SubstanceId is the
// board's properties#MATERIAL_PROPERTY_CATALOGUE substance row (the board physics, read by key — the buildable board is the
// Component, the substance the Properties row). The tokens are the egress HINT the Rasm.Bim Emit gate resolves + validates
// against the frozen IfcClass valid-set ([C6]), never the authority — this owner carries only the portable strings.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanelKind {
    public static readonly PanelKind GypsumBoard      = new("gypsum-board",      ifcEntity: "IfcCovering", ifcPredefinedType: "CLADDING",   structural: false, substanceId: "gypsum.board");
    public static readonly PanelKind GypsumSheathing  = new("gypsum-sheathing",  ifcEntity: "IfcCovering", ifcPredefinedType: "CLADDING",   structural: false, substanceId: "gypsum.board");
    public static readonly PanelKind PlywoodSheathing = new("plywood-sheathing", ifcEntity: "IfcPlate",    ifcPredefinedType: "SHEET",      structural: true,  substanceId: "wood.plywood");
    public static readonly PanelKind OsbSheathing     = new("osb-sheathing",     ifcEntity: "IfcPlate",    ifcPredefinedType: "SHEET",      structural: true,  substanceId: "wood.osb");
    public static readonly PanelKind CementBoard      = new("cement-board",      ifcEntity: "IfcCovering", ifcPredefinedType: "CLADDING",   structural: false, substanceId: "cement.board");
    public static readonly PanelKind SteelDeckRoof    = new("steel-deck-roof",   ifcEntity: "IfcPlate",    ifcPredefinedType: "SHEET",      structural: true,  substanceId: "metal.steel");
    public static readonly PanelKind SteelDeckFloor   = new("steel-deck-floor",  ifcEntity: "IfcSlab",     ifcPredefinedType: "FLOOR",      structural: true,  substanceId: "metal.steel");
    public static readonly PanelKind RigidBoardEps    = new("rigid-board-eps",   ifcEntity: "IfcCovering", ifcPredefinedType: "INSULATION", structural: false, substanceId: "insulation.eps");
    public static readonly PanelKind RigidBoardXps    = new("rigid-board-xps",   ifcEntity: "IfcCovering", ifcPredefinedType: "INSULATION", structural: false, substanceId: "insulation.xps");
    public static readonly PanelKind RigidBoardPoly   = new("rigid-board-poly",  ifcEntity: "IfcCovering", ifcPredefinedType: "INSULATION", structural: false, substanceId: "insulation.pir");
    public string IfcEntity { get; }
    public string IfcPredefinedType { get; }
    public bool Structural { get; }          // true only for the steel-deck kinds (the rib ComputedSection contributors)
    public string SubstanceId { get; }       // the properties#MATERIAL_PROPERTY_CATALOGUE MaterialId — the board substance, read by key
    public MaterialId Substance => MaterialId.Of(SubstanceId);
}

// EdgeProfile is the board-edge axis the layout coursing reads to butt or lap adjacent boards (the gypsum tapered edge for
// the joint-compound recess, the square cut for a butt joint, the tongue-groove/shiplap interlock for a wood/insulation
// shear transfer, the side-lap-interlock for the steel-deck nestable seam). Lapped flags an interlocking edge the
// Construction/layout#ASSEMBLY_FOLD SheetCoursing stage overlaps rather than butts; GapMm is the board-to-board coursing gap
// the SheetCoursing adds to the board pitch — 0 for an interlocking edge (the boards nest with no gap), a small butt gap for
// a square/tapered/beveled/rounded edge (the ~3 mm control gap gypsum/plywood is laid with). A new edge is one row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EdgeProfile {
    public static readonly EdgeProfile Square           = new("square",             lapped: false, gapMm: 3.0);
    public static readonly EdgeProfile Tapered          = new("tapered",            lapped: false, gapMm: 3.0);   // ASTM C1396 recessed long edge for the joint-compound feather
    public static readonly EdgeProfile Beveled          = new("beveled",            lapped: false, gapMm: 3.0);
    public static readonly EdgeProfile Rounded          = new("rounded",            lapped: false, gapMm: 3.0);
    public static readonly EdgeProfile TongueGroove     = new("tongue-groove",      lapped: true,  gapMm: 0.0);   // APA T&G edge nests with no gap
    public static readonly EdgeProfile Shiplap          = new("shiplap",            lapped: true,  gapMm: 0.0);
    public static readonly EdgeProfile SideLapInterlock = new("side-lap-interlock", lapped: true,  gapMm: 0.0);   // SDI deck nestable side-lap seam
    public bool Lapped { get; }
    public double GapMm { get; }
}

// PanelLayerRole is the face/core stack-layer axis the typed PanelLayer carries so the layup is a real layered board (a
// generator builds the face/core/face solid), never a single-thickness smear. Facing flags an outer layer (the host
// distinguishes a paper/glass-mat/foil face from the core); a new role is one row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanelLayerRole {
    public static readonly PanelLayerRole PaperFace           = new("paper-face",            facing: true);
    public static readonly PanelLayerRole GlassMatFacer       = new("glass-mat-facer",       facing: true);
    public static readonly PanelLayerRole FoilFacer           = new("foil-facer",            facing: true);
    public static readonly PanelLayerRole GlassFiberMatFacer  = new("glass-fiber-mat-facer", facing: true);
    public static readonly PanelLayerRole GlassMeshScrim      = new("glass-mesh-scrim",      facing: true);
    public static readonly PanelLayerRole GypsumCore          = new("gypsum-core",           facing: false);
    public static readonly PanelLayerRole VeneerPly           = new("veneer-ply",            facing: false);
    public static readonly PanelLayerRole StrandLayer         = new("strand-layer",          facing: false);
    public static readonly PanelLayerRole CementAggregateCore = new("cement-aggregate-core", facing: false);
    public static readonly PanelLayerRole FoamCore            = new("foam-core",             facing: false);
    public bool Facing { get; }
}

// PanelOrientation is the strength-axis run-direction the board lays at (the wood/gypsum panel strength axis perpendicular
// to the framing, the steel-deck strength axis parallel to its span, the cement-board unidirectional lay) the
// Construction/layout#ASSEMBLY_FOLD Sheathing stage reads to orient the run. AcrossFrame flags a strength-axis-perpendicular
// lay the staggered-joint coursing offsets along the frame; a new orientation is one row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanelOrientation {
    public static readonly PanelOrientation StrengthAxisPerpendicular = new("strength-axis-perpendicular",    acrossFrame: true);   // the wood/gypsum panel strength axis laid across the framing so a sheet spans the supports
    public static readonly PanelOrientation StrengthAxisParallel      = new("strength-axis-parallel-to-span", acrossFrame: false);  // SDI deck spans parallel to its rib
    public static readonly PanelOrientation Unidirectional            = new("unidirectional",                 acrossFrame: false);
    public bool AcrossFrame { get; }
}

// CoreType is the gypsum-board core formulation (ASTM C1396 regular / Type-X fire / Type-C enhanced-fire / moisture-resistant
// / abuse-resistant / glass-mat / water-resistant), a NON-structural board-property column the substance physics and the
// fire-rating seam read; FireRated flags a Type-X/C core. A non-gypsum kind carries CoreType.None.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CoreType {
    public static readonly CoreType None              = new("none",               fireRated: false);
    public static readonly CoreType Regular           = new("regular",            fireRated: false);
    public static readonly CoreType TypeXFire         = new("type-x-fire",        fireRated: true);    // ASTM C1396 §fire-resistant, 5/8in 1-hr assembly
    public static readonly CoreType TypeCFire         = new("type-c-fire",        fireRated: true);    // enhanced Type-X (vermiculite + glass fiber)
    public static readonly CoreType MoistureResistant = new("moisture-resistant", fireRated: false);
    public static readonly CoreType AbuseResistant    = new("abuse-resistant",    fireRated: false);
    public static readonly CoreType GlassMat          = new("glass-mat",          fireRated: false);   // ASTM C1177 glass-mat exterior sheathing
    public static readonly CoreType WaterResistant    = new("water-resistant",    fireRated: false);
    public bool FireRated { get; }
}

// SpanRating is the APA PRP-108 wood-structural-panel span rating (the roof/floor maximum support spacing the panel develops
// over supports / over edge-supported floor in inches) — a structural board-property column the diaphragm/sheathing-design
// seam reads. The dual rating "24/16" is roof-span/floor-span; RoofSpanIn/FloorSpanIn split it. A non-wood kind carries
// SpanRating.None.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpanRating {
    public static readonly SpanRating None    = new("none",  roofSpanIn: 0,  floorSpanIn: 0);
    public static readonly SpanRating S24_0   = new("24/0",  roofSpanIn: 24, floorSpanIn: 0);
    public static readonly SpanRating S24_16  = new("24/16", roofSpanIn: 24, floorSpanIn: 16);
    public static readonly SpanRating S32_16  = new("32/16", roofSpanIn: 32, floorSpanIn: 16);
    public static readonly SpanRating S40_20  = new("40/20", roofSpanIn: 40, floorSpanIn: 20);
    public static readonly SpanRating S48_24  = new("48/24", roofSpanIn: 48, floorSpanIn: 24);
    public int RoofSpanIn { get; }
    public int FloorSpanIn { get; }
}

// BondClass is the APA / EN 13986 wood-structural-panel exposure / bond durability (Exposure-1 the interim-weather phenolic
// bond, Exterior the fully-weatherproof bond) — a structural board-property column. A non-wood kind carries BondClass.None.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BondClass {
    public static readonly BondClass None      = new("none",       exterior: false);
    public static readonly BondClass Exposure1 = new("exposure-1", exterior: false);   // APA EXP-1 interior with interim weather
    public static readonly BondClass Exterior  = new("exterior",   exterior: true);    // APA / EN 636-3 fully exterior
    public bool Exterior { get; }
}

// FoamType is the rigid-board-insulation foam chemistry (ASTM C578 EPS/XPS, ASTM C1289 polyiso) carrying the design
// R-value-per-inch and the compressive-strength band the building-envelope seam reads. The polyiso LTTR (long-term thermal
// resistance) is the aged design value, lower than the initial; EPS spans the ASTM C578 Type I/VIII/II/IX density bands. A
// non-insulation kind carries FoamType.None.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FoamType {
    public static readonly FoamType None    = new("none",    rValuePerInch: 0.0, compressiveStrengthPsi: 0.0);
    public static readonly FoamType Eps     = new("eps",     rValuePerInch: 3.85, compressiveStrengthPsi: 15.0);  // ASTM C578 Type I-IX, R 3.6-4.2/in, 10-25 psi
    public static readonly FoamType Xps     = new("xps",     rValuePerInch: 5.0,  compressiveStrengthPsi: 25.0);  // ASTM C578, R 5.0/in, 15-100 psi
    public static readonly FoamType Polyiso = new("polyiso", rValuePerInch: 5.7,  compressiveStrengthPsi: 20.0);  // ASTM C1289, LTTR 5.6-6.5/in, 16-25 psi
    public double RValuePerInch { get; }            // ASTM C578/C1289 design R per inch of thickness (hr·ft²·°F/Btu per in)
    public double CompressiveStrengthPsi { get; }
}

// Facer is the rigid-board / sheathing facer the host materializes as the outer face layer (the foil/glass-fiber-mat/coated-
// glass facer on polyiso, the glass-mat on gypsum sheathing) and the appearance projection reads; FacedBoth flags a
// two-sided facer. A bare-core kind carries Facer.None.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Facer {
    public static readonly Facer None         = new("none",            facedBoth: false);
    public static readonly Facer Foil         = new("foil",            facedBoth: true);
    public static readonly Facer GlassFiberMat = new("glass-fiber-mat", facedBoth: true);
    public static readonly Facer CoatedGlass  = new("coated-glass",    facedBoth: true);
    public bool FacedBoth { get; }
}

// FastenerType is the board-fastener axis the FastenPattern carries (the drywall screw, the 8d structural nail, the galvanized
// roofing nail, the deck arc-spot weld / self-drilling deck screw, the insulation plate-and-screw / adhesive bead) the
// Construction/layout#ASSEMBLY_FOLD Sheathing stage places as Placement rows; Welded flags a deck weld the host renders as a
// puddle weld rather than a discrete fastener. AppearanceId is the fastener's Appearance/graph#MATERIAL_LIBRARY render-
// material MaterialId string the SheetCoursing stage tags each fastener Placement with (MaterialId.Of(Fastener.AppearanceId)),
// so the host reads the fastener solid off the placement material distinct from the board substance — a steel finish for the
// screwed/nailed/welded metal fasteners, an adhesive-bead material for the glued rigid board. A new fastener is one row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FastenerType {
    public static readonly FastenerType DrywallScrew  = new("drywall-screw",   welded: false, appearanceId: "metal.steel");     // ASTM C1002 bugle-head
    public static readonly FastenerType StructuralNail = new("structural-nail", welded: false, appearanceId: "metal.steel");    // 8d common, APA edge/field
    public static readonly FastenerType RoofingNail   = new("roofing-nail",    welded: false, appearanceId: "metal.steel");     // galvanized, cement-board
    public static readonly FastenerType DeckWeld      = new("deck-weld",       welded: true,  appearanceId: "metal.steel");     // SDI arc-spot puddle weld
    public static readonly FastenerType DeckScrew     = new("deck-screw",      welded: false, appearanceId: "metal.steel");     // SDI self-drilling support screw
    public static readonly FastenerType PlateAndScrew = new("plate-and-screw", welded: false, appearanceId: "metal.steel");     // rigid-board mechanical fastener + insulation plate
    public static readonly FastenerType Adhesive      = new("adhesive",        welded: false, appearanceId: "adhesive.bead");   // rigid-board ribbon adhesive
    public bool Welded { get; }
    public string AppearanceId { get; }      // the fastener render-material MaterialId string the SheetCoursing placement tags
}

// DeckForm is the SDI steel-deck structural form (FORM deck the stay-in-place concrete form, COMPOSITE deck the embossed-rib
// shear-bond floor deck, ROOF deck the insulation-substrate roof deck) the rib section and the IFC entity choice read
// (a roof/form deck is an IfcPlate/SHEET, a composite floor deck an IfcSlab/FLOOR). FloorDeck flags a deck modeled as a slab.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DeckForm {
    public static readonly DeckForm Form      = new("form",      floorDeck: false);   // ANSI/SDI form deck (stay-in-place)
    public static readonly DeckForm Composite = new("composite", floorDeck: true);    // ANSI/SDI C-2017 embossed-rib floor deck
    public static readonly DeckForm Roof      = new("roof",      floorDeck: false);   // ANSI/SDI RD-2017 roof deck
    public bool FloorDeck { get; }
}

// DeckProfile is the SDI steel-deck rib profile carrying its standard rib geometry (the wide-rib B / narrow-rib A /
// intermediate-F / deep-N roof-deck profiles and the 2VLI/3VLI composite-floor profiles and the dovetail acoustic deck), each
// row carrying the standard rib depth, the rib pitch (centre-to-centre), and the coverage width — the DeckRib value-object
// defaults to these where a row omits an override, so the rib ComputedSection ([03]) is grounded in the SDI standard
// geometry, never an unbacked literal. TopFlangeFraction is the rib top-flange width as a fraction of the pitch (the
// trapezoidal corrugation top width / pitch), so the [03] Perimeter builds the real trapezoid.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DeckProfile {
    public static readonly DeckProfile WideRibB     = new("wide-rib-b",     ribDepthMm: 38.1,  ribPitchMm: 152.4, coverageMm: 914.4, topFlangeFraction: 0.45);   // 1.5in B, 6in pitch, 36in coverage
    public static readonly DeckProfile NarrowRibA   = new("narrow-rib-a",   ribDepthMm: 38.1,  ribPitchMm: 152.4, coverageMm: 914.4, topFlangeFraction: 0.30);   // 1.5in A (narrow top)
    public static readonly DeckProfile IntermediateF = new("intermediate-f", ribDepthMm: 38.1,  ribPitchMm: 152.4, coverageMm: 914.4, topFlangeFraction: 0.38);   // 1.5in F
    public static readonly DeckProfile DeepN        = new("deep-n",         ribDepthMm: 76.2,  ribPitchMm: 203.2, coverageMm: 609.6, topFlangeFraction: 0.40);   // 3in N, 8in pitch, 24in coverage
    public static readonly DeckProfile Composite2VLI = new("composite-2vli", ribDepthMm: 50.8,  ribPitchMm: 152.4, coverageMm: 914.4, topFlangeFraction: 0.42);   // 2in composite
    public static readonly DeckProfile Composite3VLI = new("composite-3vli", ribDepthMm: 76.2,  ribPitchMm: 203.2, coverageMm: 914.4, topFlangeFraction: 0.42);   // 3in composite
    public static readonly DeckProfile Dovetail     = new("dovetail",       ribDepthMm: 50.8,  ribPitchMm: 152.4, coverageMm: 914.4, topFlangeFraction: 0.55);   // acoustic/cellular dovetail
    public double RibDepthMm { get; }
    public double RibPitchMm { get; }
    public double CoverageMm { get; }
    public double TopFlangeFraction { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
// PanelLayer is the typed face/core stack layer the layup carries (a generator builds the real layered board face-by-face,
// the CompositionAuthor.LayerSet coerces it into the seam IfcMaterialLayerSet). The role discriminates a face from the core,
// the thickness a kernel PositiveMagnitude, the material the seam MaterialId the layer is made of — so a five-layer board is
// five PanelLayer rows, never a single-thickness scalar smear.
public readonly record struct PanelLayer(PanelLayerRole Role, PositiveMagnitude ThicknessMm, MaterialId Material);

// FastenPattern is the typed board-fastening schedule the host places (the field+edge fastener grid the
// Construction/layout#ASSEMBLY_FOLD SheetCoursing stage steps as Placement rows). The field and edge SPACINGS are kernel
// PositiveMagnitude (admitted ONCE through Of, so the Projection/component#COMPONENT_PROJECTOR panel Detail arm reads
// FieldSpacingMm.Value/EdgeSpacingMm.Value and the layout reads them typed) — a board is fastened by a REAL grid (field 12in
// / edge 6in for plywood, field 12in / edge 8in for gypsum), never a free nail count. EdgeDistanceMm stays a double because a
// welded steel deck carries a 0 edge inset (no perimeter fastener inset — the weld sits on the support line), which a
// PositiveMagnitude would reject. EdgeStations/FieldStations are the per-board-axis station counts a generator derives from
// the board length over the spacing. The SAME admit-once ComponentUnit.Of idiom, not a [ComplexValueObject] over raw doubles.
public readonly record struct FastenPattern(PositiveMagnitude FieldSpacingMm, PositiveMagnitude EdgeSpacingMm, double EdgeDistanceMm, FastenerType Fastener) {
    public static Fin<FastenPattern> Of(double fieldSpacingMm, double edgeSpacingMm, double edgeDistanceMm, FastenerType fastener, Op key) =>
        from field in key.AcceptValidated<PositiveMagnitude>(candidate: fieldSpacingMm)
        from edge in key.AcceptValidated<PositiveMagnitude>(candidate: edgeSpacingMm)
        from inset in guard(edgeDistanceMm >= 0.0, ComponentFault.Dimension(key, $"<fasten-edge-distance-negative:{edgeDistanceMm:R}>"))
        select new FastenPattern(field, edge, edgeDistanceMm, fastener);

    // The per-edge and per-field fastener-station counts a generator distributes over a board axis of `axisLengthMm`, clamped
    // to >= 1 (a board carries at least one edge and one field station). The SheetCoursing stage reads these to place the
    // fastener grid; a welded deck reads EdgeStations as the side-lap weld count.
    public int EdgeStations(double axisLengthMm) => Math.Max(1, (int)Math.Floor(axisLengthMm / EdgeSpacingMm.Value));
    public int FieldStations(double axisLengthMm) => Math.Max(1, (int)Math.Floor(axisLengthMm / FieldSpacingMm.Value));
}

// DeckRib is the typed steel-deck trapezoidal corrugation the [03] rib ComputedSection builds its Perimeter from — the form
// (form/composite/roof), the profile (carrying the standard rib geometry), the rib depth and pitch (a PositiveMagnitude, the
// DeckProfile default unless a row overrides), the coverage width, and the base-metal SteelGauge (the connector family's
// cold-formed gauge axis, reused — never a parallel deck gauge enum). RibCountPerCoverage is the discrete rib count the
// coverage width admits (coverage / pitch), the int the rib-section coverage reads. A non-steel-deck kind carries no DeckRib
// (the PanelSection.Rib Option is None).
public readonly record struct DeckRib(DeckForm Form, DeckProfile Profile, PositiveMagnitude DepthMm, PositiveMagnitude PitchMm, PositiveMagnitude BaseMetalMm, PositiveMagnitude CoverageMm, SteelGauge Gauge) {
    public int RibCountPerCoverage => Math.Max(1, (int)Math.Round(CoverageMm.Value / PitchMm.Value));
    public double TopFlangeMm => PitchMm.Value * Profile.TopFlangeFraction;            // the trapezoid top-flange width per pitch
    public double WebSlopeRise => DepthMm.Value;                                       // the trapezoid web rise (the rib depth)
}

// PanelSection is the generative board payload — the board kind, the board length/width/thickness as kernel PositiveMagnitude
// columns, the edge profile, the typed face/core PanelLayer stack, the typed FastenPattern schedule, the orientation, the
// board-property bands (CoreType/SpanRating/BondClass/FoamType/Facer), and the optional steel-deck DeckRib. A generator lays
// the real board from these columns; the buildable BOARD is the Component, the board substance the PanelKind.SubstanceId
// Properties row (read by key, never re-keyed here). LengthMm is the full board length the run direction reads, WidthMm the
// board width, ThicknessMm the board thickness (the overall thickness for a layered board, the rib total height for a deck).
public readonly record struct PanelSection(
    PanelKind Kind,
    PositiveMagnitude LengthMm,
    PositiveMagnitude WidthMm,
    PositiveMagnitude ThicknessMm,
    EdgeProfile Edge,
    Seq<PanelLayer> Layup,
    FastenPattern Fastening,
    PanelOrientation Orientation,
    CoreType Core,
    SpanRating Span,
    BondClass Bond,
    FoamType Foam,
    Facer Facer,
    Option<DeckRib> Rib) {

    // The board's IFC leaf the COMPONENT_PROJECTOR stamps — read straight off the kind so the projector never re-derives the
    // entity per board (the THIRD projection mode, a kind-determined leaf). The IfcEntity/IfcPredefinedToken the parent
    // ComponentSection.Panel arm forwards.
    public string IfcEntity => Kind.IfcEntity;
    public string IfcPredefinedToken => Kind.IfcPredefinedType;

    // The board's substance + capacity material the design seam reads by key (the buildable board is the Component, the
    // substance the Properties row) — INDEPENDENT from the AppearanceId finish by the component#COMPONENT_OWNER two-slot law.
    public MaterialId SubstanceId => Kind.Substance;
    public MaterialId CapacityKey => Kind.Substance;

    // The FINISH the appearance projection reads — the board face material (a paper-faced/glass-mat board, a galvanized deck,
    // a foil-faced foam), INDEPENDENT from CapacityKey: a coated/printed board keeps its substance CapacityKey while its
    // AppearanceId names the face finish. The outermost facing PanelLayer's material is the face, falling back to the
    // substance for a bare-core board.
    public MaterialId AppearanceId =>
        Layup.Find(static layer => layer.Role.Facing).Match(Some: static layer => layer.Material, None: () => Kind.Substance);

    // The single defining cross dimension every family projects (the parent ComponentSection.CrossNominalMm reads it): the
    // board thickness (the validated-board invariant guarantees a positive overall thickness). A deck's thickness is its rib
    // total height (the DeckProfile rib depth + the base-metal sheet).
    public PositiveMagnitude CrossNominalMm => ThicknessMm;

    // The gross bounding (width, thickness) pair a family-agnostic concrete-outline consumer reads (the parent
    // ComponentSection.GrossRectangleMm reads it) — a board's in-plane extent is its width × length, but the cross-section
    // bounding pair the section consumers read is the board WIDTH × THICKNESS (the laid-board cross dimension), so the
    // projection is TOTAL across the families. The rib net section ([03]) is the steel-deck refinement over this gross pair.
    public (PositiveMagnitude WidthMm, PositiveMagnitude DepthMm) GrossRectangleMm => (WidthMm, ThicknessMm);

    // The rigid-board design R-value-per-inch (the FoamType band) and the total board R (R-per-inch × board thickness in
    // inches) the building-envelope thermal seam reads — 0 for a non-insulation board (the substance Thermal row carries the
    // conductivity for those). The ASTM C578/C1289 design R the continuous-insulation check reads.
    public double RValuePerInch => Foam.RValuePerInch;
    public double RValue => Foam.RValuePerInch * (ThicknessMm.Value / 25.4);

    // The unit-segment projection a sheathing run flows through the Resolve fold on — WidthMm the board width the course
    // steps, ThicknessMm the board height (the laid-board depth), LengthMm the board length the run direction reads, and the
    // course height the board WIDTH so a sheathing run steps board-to-board across the frame. TOTAL (the SAME shape masonry
    // MasonryUnit.ToUnit() takes): the PanelSection columns are already-admitted PositiveMagnitude, so the direct ComponentUnit
    // ctor re-wraps them with no re-admission and no Op — never a re-validation of a proven-positive value. The
    // Construction/layout#ASSEMBLY_FOLD SheetCoursing stage reads run.Unit off this; the ComponentSection.Panel arm delegates here.
    public ComponentUnit ToUnit() => new(WidthMm, ThicknessMm, LengthMm, WidthMm);
}

// PanelRow is the flat catalogue seed; the optional steel-deck columns (Form/Profile/Gauge) carry the deck rib geometry,
// empty for a non-deck row; the optional board-property columns (Core/Span/Bond/Foam/Facer) default to their None row where
// a kind does not carry the band. WidthMm/LengthMm/ThicknessMm are the board's overall dimensions, the FieldMm/EdgeMm/
// EdgeDistMm the fastening schedule.
public readonly record struct PanelRow(
    string Designation, string Kind, double WidthMm, double LengthMm, double ThicknessMm,
    string Edge, string Orientation, string Fastener, double FieldMm, double EdgeMm, double EdgeDistMm,
    string Core, string Span, string Bond, string Foam, string Facer,
    string Form, string Profile, string Gauge, double BaseMetalMm);

public sealed record PanelShape(ComponentId Id, PanelSection Section, ComponentStandard Standard);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ComponentCatalogue {
    // The non-regional panel standards the registered rows cite (ASTM C1396 gypsum, APA PRP-108 wood structural panels, ASTM
    // C1325 cement board, ANSI/SDI deck, ASTM C578/C1289 rigid board are board-product specifications, not a masonry-style
    // regional joint thickness) — StandardJointThicknessMm 0.0 (a laid board lays no mortar joint), ComponentAuthority.Astm
    // the ASTM/SDI authority, mirroring fastener.md's iso static and connector.md's Aisi static.
    static readonly ComponentStandard Standard = new("us", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.Astm);

    // The ASTM C1396 / APA PRP-108 / ASTM C1325 / ANSI-SDI / ASTM C578-C1289 board roster — the board dimensions transcribe
    // the published standard nominal sizes (the modular 48in/54in widths and 8-16ft lengths, the 1/4-5/8in thicknesses, the
    // 22-16ga deck base metals), the span ratings the APA PRP-108 dual roof/floor spacings, the rib geometry the SDI profile
    // standards, the fastening the field/edge nailing/screwing schedules. A direction/property a board does not carry takes
    // its axis None row. A new board is one row; a new property band one row on the matching axis.
    static readonly Seq<PanelRow> PanelRows = Seq(
        // --- gypsum board (ASTM C1396/C1396M-24; EN 520) — 48in/54in width, 8-16ft length, 1/4-5/8in thickness, tapered/square edge, drywall-screw 12in field / 8in edge
        new PanelRow("panel.gyp-reg-050-4x8",  "gypsum-board", 1219.2, 2438.4, 12.7,  "tapered", "strength-axis-perpendicular", "drywall-screw", 304.8, 203.2, 9.5, "regular",     "none", "none", "none", "none", "", "", "", 0.0),
        new PanelRow("panel.gyp-reg-038-4x8",  "gypsum-board", 1219.2, 2438.4, 9.5,   "tapered", "strength-axis-perpendicular", "drywall-screw", 304.8, 203.2, 9.5, "regular",     "none", "none", "none", "none", "", "", "", 0.0),
        new PanelRow("panel.gyp-x-050-4x8",    "gypsum-board", 1219.2, 2438.4, 12.7,  "tapered", "strength-axis-perpendicular", "drywall-screw", 304.8, 203.2, 9.5, "type-x-fire", "none", "none", "none", "none", "", "", "", 0.0),
        new PanelRow("panel.gyp-x-625-4x8",    "gypsum-board", 1219.2, 2438.4, 15.9,  "tapered", "strength-axis-perpendicular", "drywall-screw", 304.8, 203.2, 9.5, "type-x-fire", "none", "none", "none", "none", "", "", "", 0.0),
        new PanelRow("panel.gyp-x-625-54x12",  "gypsum-board", 1371.6, 3657.6, 15.9,  "tapered", "strength-axis-perpendicular", "drywall-screw", 304.8, 203.2, 9.5, "type-x-fire", "none", "none", "none", "none", "", "", "", 0.0),
        new PanelRow("panel.gyp-c-625-4x8",    "gypsum-board", 1219.2, 2438.4, 15.9,  "tapered", "strength-axis-perpendicular", "drywall-screw", 304.8, 203.2, 9.5, "type-c-fire", "none", "none", "none", "none", "", "", "", 0.0),
        new PanelRow("panel.gyp-mr-050-4x8",   "gypsum-board", 1219.2, 2438.4, 12.7,  "tapered", "strength-axis-perpendicular", "drywall-screw", 304.8, 203.2, 9.5, "moisture-resistant", "none", "none", "none", "none", "", "", "", 0.0),
        new PanelRow("panel.gyp-abuse-625-4x8","gypsum-board", 1219.2, 2438.4, 15.9,  "tapered", "strength-axis-perpendicular", "drywall-screw", 304.8, 203.2, 9.5, "abuse-resistant", "none", "none", "none", "none", "", "", "", 0.0),
        new PanelRow("panel.gyp-025-4x8",      "gypsum-board", 1219.2, 2438.4, 6.4,   "square",  "strength-axis-perpendicular", "drywall-screw", 304.8, 203.2, 9.5, "regular",     "none", "none", "none", "none", "", "", "", 0.0),
        // --- gypsum sheathing (ASTM C1396 §gypsum-sheathing; ASTM C1177 glass-mat) — 48in width, 8-10ft length, 1/2-5/8in, square edge, glass-mat facer, screw 8in
        new PanelRow("panel.gypsheath-x-050-4x8", "gypsum-sheathing", 1219.2, 2438.4, 12.7, "square", "strength-axis-perpendicular", "drywall-screw", 203.2, 203.2, 9.5, "type-x-fire", "none", "none", "none", "glass-fiber-mat", "", "", "", 0.0),
        new PanelRow("panel.gypsheath-x-625-4x8", "gypsum-sheathing", 1219.2, 2438.4, 15.9, "square", "strength-axis-perpendicular", "drywall-screw", 203.2, 203.2, 9.5, "type-x-fire", "none", "none", "none", "glass-fiber-mat", "", "", "", 0.0),
        new PanelRow("panel.gypsheath-gm-625-4x10","gypsum-sheathing", 1219.2, 3048.0, 15.9, "square", "strength-axis-perpendicular", "drywall-screw", 203.2, 203.2, 9.5, "glass-mat", "none", "none", "none", "glass-fiber-mat", "", "", "", 0.0),
        // --- plywood sheathing (APA PRP-108 / US PS 1-19; EN 13986/636) — 48in width, 8ft length, 3/8-3/4in, span-rated, Exposure-1, 8d nail edge 6in / field 12in
        new PanelRow("panel.ply-rated-038-4x8-240",  "plywood-sheathing", 1219.2, 2438.4, 9.5,  "square",        "strength-axis-perpendicular", "structural-nail", 304.8, 152.4, 9.5, "none", "24/0",  "exposure-1", "none", "none", "", "", "", 0.0),
        new PanelRow("panel.ply-rated-1532-4x8-2416","plywood-sheathing", 1219.2, 2438.4, 11.9, "square",        "strength-axis-perpendicular", "structural-nail", 304.8, 152.4, 9.5, "none", "24/16", "exposure-1", "none", "none", "", "", "", 0.0),
        new PanelRow("panel.ply-rated-050-4x8-3216", "plywood-sheathing", 1219.2, 2438.4, 12.7, "square",        "strength-axis-perpendicular", "structural-nail", 304.8, 152.4, 9.5, "none", "32/16", "exposure-1", "none", "none", "", "", "", 0.0),
        new PanelRow("panel.ply-rated-1932-4x8-4020","plywood-sheathing", 1219.2, 2438.4, 15.1, "tongue-groove", "strength-axis-perpendicular", "structural-nail", 304.8, 152.4, 9.5, "none", "40/20", "exposure-1", "none", "none", "", "", "", 0.0),
        new PanelRow("panel.ply-rated-2332-4x8-4824","plywood-sheathing", 1219.2, 2438.4, 18.3, "tongue-groove", "strength-axis-perpendicular", "structural-nail", 304.8, 152.4, 9.5, "none", "48/24", "exterior",   "none", "none", "", "", "", 0.0),
        new PanelRow("panel.ply-str1-1932-4x8",      "plywood-sheathing", 1219.2, 2438.4, 15.1, "square",        "strength-axis-perpendicular", "structural-nail", 304.8, 152.4, 9.5, "none", "40/20", "exterior",   "none", "none", "", "", "", 0.0),
        new PanelRow("panel.ply-rated-075-4x8-4824", "plywood-sheathing", 1219.2, 2438.4, 19.0, "tongue-groove", "strength-axis-perpendicular", "structural-nail", 304.8, 152.4, 9.5, "none", "48/24", "exterior",   "none", "none", "", "", "", 0.0),
        // --- osb sheathing (APA PRP-108 / US PS 2-18; EN 13986/300) — 48in width, 8ft length, 7/16-23/32in, span-rated, Exposure-1, 8d nail edge 6in / field 12in
        new PanelRow("panel.osb-rated-716-4x8-240",  "osb-sheathing", 1219.2, 2438.4, 11.1, "square",        "strength-axis-perpendicular", "structural-nail", 304.8, 152.4, 9.5, "none", "24/0",  "exposure-1", "none", "none", "", "", "", 0.0),
        new PanelRow("panel.osb-rated-1532-4x8-2416","osb-sheathing", 1219.2, 2438.4, 11.9, "square",        "strength-axis-perpendicular", "structural-nail", 304.8, 152.4, 9.5, "none", "24/16", "exposure-1", "none", "none", "", "", "", 0.0),
        new PanelRow("panel.osb-rated-050-4x8-3216", "osb-sheathing", 1219.2, 2438.4, 12.7, "square",        "strength-axis-perpendicular", "structural-nail", 304.8, 152.4, 9.5, "none", "32/16", "exposure-1", "none", "none", "", "", "", 0.0),
        new PanelRow("panel.osb-rated-2332-4x8-4824","osb-sheathing", 1219.2, 2438.4, 18.3, "tongue-groove", "strength-axis-perpendicular", "structural-nail", 304.8, 152.4, 9.5, "none", "48/24", "exposure-1", "none", "none", "", "", "", 0.0),
        new PanelRow("panel.osb-rated-1532-4x24",    "osb-sheathing", 1219.2, 7315.2, 11.9, "tongue-groove", "strength-axis-perpendicular", "structural-nail", 304.8, 152.4, 9.5, "none", "24/16", "exposure-1", "none", "none", "", "", "", 0.0),
        // --- cement board (ASTM C1325; ANSI A118.9) — 32/36/48in width, 5/8/10ft length, 1/4-5/8in, square edge, roofing-nail/screw 8in, edge-dist 3/4in
        new PanelRow("panel.cbu-025-3x5",  "cement-board", 914.4,  1524.0, 6.4,  "square", "unidirectional", "roofing-nail", 203.2, 203.2, 19.0, "none", "none", "none", "none", "glass-mesh-scrim", "", "", "", 0.0),
        new PanelRow("panel.cbu-050-3x5",  "cement-board", 914.4,  1524.0, 12.7, "square", "unidirectional", "roofing-nail", 203.2, 203.2, 19.0, "none", "none", "none", "none", "glass-mesh-scrim", "", "", "", 0.0),
        new PanelRow("panel.cbu-050-4x8",  "cement-board", 1219.2, 2438.4, 12.7, "square", "unidirectional", "roofing-nail", 203.2, 203.2, 19.0, "none", "none", "none", "none", "glass-mesh-scrim", "", "", "", 0.0),
        new PanelRow("panel.cbu-625-4x8",  "cement-board", 1219.2, 2438.4, 15.9, "square", "unidirectional", "roofing-nail", 203.2, 203.2, 19.0, "none", "none", "none", "none", "glass-mesh-scrim", "", "", "", 0.0),
        new PanelRow("panel.cbu-050-32x60","cement-board", 812.8,  1524.0, 12.7, "square", "unidirectional", "roofing-nail", 203.2, 203.2, 19.0, "none", "none", "none", "none", "glass-mesh-scrim", "", "", "", 0.0),
        // --- steel deck (ANSI/SDI RD-2017 roof; C-2017 composite) — coverage 36in B/A/F or 24in N, base-metal 22-16ga, weld/screw 12in support / side-lap screw
        new PanelRow("panel.deck-b-22ga-roof", "steel-deck-roof",  914.4, 6096.0, 38.1, "side-lap-interlock", "strength-axis-parallel-to-span", "deck-weld",  304.8, 304.8, 0.0, "none", "none", "none", "none", "none", "roof",      "wide-rib-b",     "22ga", 0.75),
        new PanelRow("panel.deck-b-20ga-roof", "steel-deck-roof",  914.4, 6096.0, 38.1, "side-lap-interlock", "strength-axis-parallel-to-span", "deck-weld",  304.8, 304.8, 0.0, "none", "none", "none", "none", "none", "roof",      "wide-rib-b",     "20ga", 0.91),
        new PanelRow("panel.deck-a-20ga-roof", "steel-deck-roof",  914.4, 6096.0, 38.1, "side-lap-interlock", "strength-axis-parallel-to-span", "deck-weld",  304.8, 304.8, 0.0, "none", "none", "none", "none", "none", "roof",      "narrow-rib-a",   "20ga", 0.91),
        new PanelRow("panel.deck-f-18ga-roof", "steel-deck-roof",  914.4, 6096.0, 38.1, "side-lap-interlock", "strength-axis-parallel-to-span", "deck-weld",  304.8, 304.8, 0.0, "none", "none", "none", "none", "none", "roof",      "intermediate-f", "18ga", 1.21),
        new PanelRow("panel.deck-n-18ga-roof", "steel-deck-roof",  609.6, 9144.0, 76.2, "side-lap-interlock", "strength-axis-parallel-to-span", "deck-weld",  304.8, 304.8, 0.0, "none", "none", "none", "none", "none", "roof",      "deep-n",         "18ga", 1.21),
        new PanelRow("panel.deck-bform-22ga",  "steel-deck-roof",  914.4, 6096.0, 38.1, "side-lap-interlock", "strength-axis-parallel-to-span", "deck-screw", 304.8, 304.8, 0.0, "none", "none", "none", "none", "none", "form",      "wide-rib-b",     "22ga", 0.75),
        new PanelRow("panel.deck-2vli-18ga",   "steel-deck-floor", 914.4, 9144.0, 50.8, "side-lap-interlock", "strength-axis-parallel-to-span", "deck-weld",  304.8, 304.8, 0.0, "none", "none", "none", "none", "none", "composite", "composite-2vli", "18ga", 1.21),
        new PanelRow("panel.deck-3vli-16ga",   "steel-deck-floor", 914.4, 9144.0, 76.2, "side-lap-interlock", "strength-axis-parallel-to-span", "deck-weld",  304.8, 304.8, 0.0, "none", "none", "none", "none", "none", "composite", "composite-3vli", "16ga", 1.52),
        // --- rigid-board insulation (ASTM C578 EPS/XPS; C1289 polyiso) — 24/48in width, 4/8ft length, 1/2-6in thickness, square/shiplap/T&G edge, plate-and-screw 16in / adhesive
        new PanelRow("panel.eps-1in-4x8",     "rigid-board-eps",  1219.2, 2438.4, 25.4,  "square",        "unidirectional", "plate-and-screw", 406.4, 406.4, 0.0, "none", "none", "none", "eps",     "none",            "", "", "", 0.0),
        new PanelRow("panel.eps-2in-4x8",     "rigid-board-eps",  1219.2, 2438.4, 50.8,  "shiplap",       "unidirectional", "plate-and-screw", 406.4, 406.4, 0.0, "none", "none", "none", "eps",     "none",            "", "", "", 0.0),
        new PanelRow("panel.eps-4in-4x8",     "rigid-board-eps",  1219.2, 2438.4, 101.6, "shiplap",       "unidirectional", "adhesive",        406.4, 406.4, 0.0, "none", "none", "none", "eps",     "none",            "", "", "", 0.0),
        new PanelRow("panel.xps-1in-2x8",     "rigid-board-xps",  609.6,  2438.4, 25.4,  "shiplap",       "unidirectional", "plate-and-screw", 406.4, 406.4, 0.0, "none", "none", "none", "xps",     "none",            "", "", "", 0.0),
        new PanelRow("panel.xps-2in-2x8",     "rigid-board-xps",  609.6,  2438.4, 50.8,  "tongue-groove", "unidirectional", "plate-and-screw", 406.4, 406.4, 0.0, "none", "none", "none", "xps",     "none",            "", "", "", 0.0),
        new PanelRow("panel.polyiso-1in-4x8", "rigid-board-poly", 1219.2, 2438.4, 25.4,  "square",        "unidirectional", "plate-and-screw", 406.4, 406.4, 0.0, "none", "none", "none", "polyiso", "glass-fiber-mat", "", "", "", 0.0),
        new PanelRow("panel.polyiso-2in-foil","rigid-board-poly", 1219.2, 2438.4, 50.8,  "square",        "unidirectional", "plate-and-screw", 406.4, 406.4, 0.0, "none", "none", "none", "polyiso", "foil",            "", "", "", 0.0),
        new PanelRow("panel.polyiso-3in-4x8", "rigid-board-poly", 1219.2, 2438.4, 76.2,  "square",        "unidirectional", "plate-and-screw", 406.4, 406.4, 0.0, "none", "none", "none", "polyiso", "coated-glass",    "", "", "", 0.0));

    // The board row → PanelShape: the kind/edge/orientation/fastener/property axes resolve through their generated TryGet
    // (a miss rails the matching ComponentFault case), the board dimensions admit once through key.AcceptValidated<
    // PositiveMagnitude>(candidate:), the FastenPattern through its Fin-railed Of (the spacings through the kernel rail), the
    // optional steel-deck DeckRib built from the Form/Profile/Gauge columns (None for a non-deck row), and the typed face/core
    // Layup derived from the already-admitted thickness. A malformed row drops through Choose rather than seeding a degenerate Component.
    static Fin<PanelShape> PanelOf(PanelRow r, Context context, Op key) =>
        from kind in PanelKind.TryGet(r.Kind, out PanelKind? k) ? Fin.Succ(k!) : Fin.Fail<PanelKind>(ComponentFault.Family(key, $"<unknown-panel-kind:{r.Kind}>"))
        from edge in EdgeProfile.TryGet(r.Edge, out EdgeProfile? e) ? Fin.Succ(e!) : Fin.Fail<EdgeProfile>(ComponentFault.Designation(key, $"<unknown-edge:{r.Edge}>"))
        from orientation in PanelOrientation.TryGet(r.Orientation, out PanelOrientation? o) ? Fin.Succ(o!) : Fin.Fail<PanelOrientation>(ComponentFault.Designation(key, $"<unknown-orientation:{r.Orientation}>"))
        from fastenerType in FastenerType.TryGet(r.Fastener, out FastenerType? ft) ? Fin.Succ(ft!) : Fin.Fail<FastenerType>(ComponentFault.Designation(key, $"<unknown-fastener:{r.Fastener}>"))
        from core in CoreType.TryGet(r.Core, out CoreType? c) ? Fin.Succ(c!) : Fin.Fail<CoreType>(ComponentFault.Designation(key, $"<unknown-core:{r.Core}>"))
        from span in SpanRating.TryGet(r.Span, out SpanRating? sp) ? Fin.Succ(sp!) : Fin.Fail<SpanRating>(ComponentFault.Grade(key, $"<unknown-span:{r.Span}>"))
        from bond in BondClass.TryGet(r.Bond, out BondClass? b) ? Fin.Succ(b!) : Fin.Fail<BondClass>(ComponentFault.Grade(key, $"<unknown-bond:{r.Bond}>"))
        from foam in FoamType.TryGet(r.Foam, out FoamType? fo) ? Fin.Succ(fo!) : Fin.Fail<FoamType>(ComponentFault.Designation(key, $"<unknown-foam:{r.Foam}>"))
        from facer in Facer.TryGet(r.Facer, out Facer? fa) ? Fin.Succ(fa!) : Fin.Fail<Facer>(ComponentFault.Designation(key, $"<unknown-facer:{r.Facer}>"))
        from width in key.AcceptValidated<PositiveMagnitude>(candidate: r.WidthMm)
        from length in key.AcceptValidated<PositiveMagnitude>(candidate: r.LengthMm)
        from thickness in key.AcceptValidated<PositiveMagnitude>(candidate: r.ThicknessMm)
        from fastening in PatternOf(fastenerType, r, key)
        from rib in RibOf(kind, r, key)
        let layup = LayupOf(kind, facer, thickness)
        let section = new PanelSection(kind, length, width, thickness, edge, layup, fastening, orientation, core, span, bond, foam, facer, rib)
        select new PanelShape(ComponentId.Of(r.Designation), section, Standard);

    // The fastening schedule admits through FastenPattern.Of (the field/edge spacings through the kernel PositiveMagnitude
    // rail, the edge distance guarded >= 0) so a malformed schedule rails ComponentFault.Dimension — a non-positive spacing
    // column is a Dimension fault.
    static Fin<FastenPattern> PatternOf(FastenerType fastener, PanelRow r, Op key) =>
        FastenPattern.Of(r.FieldMm, r.EdgeMm, r.EdgeDistMm, fastener, key);

    // The steel-deck rib: Some(DeckRib) for a structural deck kind (Structural is true ONLY for the two steel-deck kinds —
    // the Form/Profile/Gauge columns resolve and the rib depth/pitch/coverage admit through the kernel rail, the base-metal
    // SteelGauge reused from the connector family), None for a non-deck board (the PanelSection.Rib Option is None and the
    // section contributes no rib ComputedSection).
    static Fin<Option<DeckRib>> RibOf(PanelKind kind, PanelRow r, Op key) =>
        !kind.Structural
            ? Fin.Succ(Option<DeckRib>.None)
            : from form in DeckForm.TryGet(r.Form, out DeckForm? f) ? Fin.Succ(f!) : Fin.Fail<DeckForm>(ComponentFault.Designation(key, $"<unknown-deck-form:{r.Form}>"))
              from profile in DeckProfile.TryGet(r.Profile, out DeckProfile? p) ? Fin.Succ(p!) : Fin.Fail<DeckProfile>(ComponentFault.Designation(key, $"<unknown-deck-profile:{r.Profile}>"))
              from gauge in SteelGauge.TryGet(r.Gauge, out SteelGauge? g) ? Fin.Succ(g!) : Fin.Fail<SteelGauge>(ComponentFault.Grade(key, $"<unknown-deck-gauge:{r.Gauge}>"))
              from depth in key.AcceptValidated<PositiveMagnitude>(candidate: profile.RibDepthMm)
              from pitch in key.AcceptValidated<PositiveMagnitude>(candidate: profile.RibPitchMm)
              from baseMetal in key.AcceptValidated<PositiveMagnitude>(candidate: r.BaseMetalMm)
              from coverage in key.AcceptValidated<PositiveMagnitude>(candidate: profile.CoverageMm)
              select Some(new DeckRib(form, profile, depth, pitch, baseMetal, coverage, gauge));

    // The kind's standard face/core PanelLayer stack, DERIVED from the already-admitted board thickness (the face/core split
    // is a deterministic policy over the admitted PositiveMagnitude, so the derived layer thicknesses mint through the total
    // PositiveMagnitude.Create factory — the SAME derived-from-admitted mint timber#TIMBER_FAMILY ResidualSection and
    // joint#JOINT_FAMILY CrossNominalMm use, NEVER a re-admission of an already-proven-positive value): a paper-face/gypsum-
    // core/paper-face drywall, a glass-mat-facer/gypsum-core/glass-mat-facer sheathing, a glass-scrim/cement-aggregate-core/
    // glass-scrim cement board, a single veneer-ply/strand-layer wood panel (the plies sum to the thickness, one core layer
    // the host subdivides), a facer/foam-core/facer rigid board, a single galvanized sheet for a deck.
    static Seq<PanelLayer> LayupOf(PanelKind kind, Facer facer, PositiveMagnitude thickness) =>
        kind == PanelKind.GypsumBoard
            ? FaceCoreFace(PanelLayerRole.PaperFace, MaterialId.Of("paper.face"), PanelLayerRole.GypsumCore, kind.Substance, thickness)
        : kind == PanelKind.GypsumSheathing
            ? FaceCoreFace(PanelLayerRole.GlassMatFacer, MaterialId.Of("glass.mat"), PanelLayerRole.GypsumCore, kind.Substance, thickness)
        : kind == PanelKind.CementBoard
            ? FaceCoreFace(PanelLayerRole.GlassMeshScrim, MaterialId.Of("glass.scrim"), PanelLayerRole.CementAggregateCore, kind.Substance, thickness)
        : kind == PanelKind.PlywoodSheathing
            ? Seq(new PanelLayer(PanelLayerRole.VeneerPly, thickness, kind.Substance))
        : kind == PanelKind.OsbSheathing
            ? Seq(new PanelLayer(PanelLayerRole.StrandLayer, thickness, kind.Substance))
        : kind == PanelKind.RigidBoardEps || kind == PanelKind.RigidBoardXps || kind == PanelKind.RigidBoardPoly
            ? FacedFoam(facer, kind.Substance, thickness)
        : Seq(new PanelLayer(PanelLayerRole.StrandLayer, thickness, kind.Substance));   // steel deck: one galvanized sheet (the rib geometry rides the DeckRib, not the layup)

    // A face/core/face stack: two thin facing layers (0.5 mm each, the standard paper/glass-mat/scrim face) over a core the
    // remainder — the derived thicknesses mint through the total PositiveMagnitude.Create (Max-clamped to a >= 0.5 mm core so
    // the mint never sees a non-positive value, since the thinnest board admitted is 6.4 mm), the layup summing to the board
    // thickness so a generator builds the real layered board; the host subdivides the wood/cement core into its real plies.
    static Seq<PanelLayer> FaceCoreFace(PanelLayerRole faceRole, MaterialId faceId, PanelLayerRole coreRole, MaterialId coreId, PositiveMagnitude thickness) {
        PositiveMagnitude face = PositiveMagnitude.Create(0.5);
        PositiveMagnitude core = PositiveMagnitude.Create(Math.Max(0.5, thickness.Value - 1.0));
        return Seq(new PanelLayer(faceRole, face, faceId), new PanelLayer(coreRole, core, coreId), new PanelLayer(faceRole, face, faceId));
    }

    // A facer/foam-core/facer rigid board — a thin facer layer (0.2 mm, the foil/glass-fiber-mat/coated-glass facer) over a
    // foam core the remainder (Max-clamped so the mint stays positive), two-sided where the facer faces both sides, else a
    // single facer over the bare core face; a bare-core (Facer.None) board is one foam layer.
    static Seq<PanelLayer> FacedFoam(Facer facer, MaterialId foamId, PositiveMagnitude thickness) {
        PanelLayerRole faceRole = facer == Facer.Foil ? PanelLayerRole.FoilFacer : PanelLayerRole.GlassFiberMatFacer;
        double faceCount = facer.FacedBoth ? 2.0 : 1.0;
        if (facer == Facer.None) { return Seq(new PanelLayer(PanelLayerRole.FoamCore, thickness, foamId)); }
        PositiveMagnitude face = PositiveMagnitude.Create(0.2);
        PositiveMagnitude core = PositiveMagnitude.Create(Math.Max(0.2, thickness.Value - 0.2 * faceCount));
        MaterialId facerId = MaterialId.Of($"facer.{facer.Key}");
        return facer.FacedBoth
            ? Seq(new PanelLayer(faceRole, face, facerId), new PanelLayer(PanelLayerRole.FoamCore, core, foamId), new PanelLayer(faceRole, face, facerId))
            : Seq(new PanelLayer(faceRole, face, facerId), new PanelLayer(PanelLayerRole.FoamCore, core, foamId));
    }

    // Every realized board resolved to its PanelShape ONCE — the seed BuildPanelRows and PanelSections both derive from this
    // one source (the SAME shape timber#TIMBER_FAMILY Shapes takes). A malformed row drops through Choose rather than seeding
    // a partial.
    static readonly Seq<PanelShape> Shapes =
        PanelRows.Choose(row => PanelOf(row, default, default).ToOption());

    // The ComponentFamily.Panel rows folded into the parent component#COMPONENT_OWNER ComponentCatalogue.Build. The cross-
    // section is the ComponentSection.Panel arm carrying the PanelSection (the FIELD, never a peer); the two independent
    // MaterialId slots (CapacityKey the substance Mechanical/Thermal row, AppearanceId the face render row) resolve off the
    // PanelSection projections, distinct columns a coated board keeps apart. ComponentId's generated [KeyMemberEqualityComparer]
    // ordinal value-equality keys the frozen dictionary, so NO explicit comparer is threaded (the component#COMPONENT_OWNER
    // ComponentCatalogue.Build convention the master fold follows).
    public static FrozenDictionary<ComponentId, Component> BuildPanelRows(Context context) =>
        Shapes
            .Choose(shape => Component.Of(ComponentFamily.Panel, shape.Id.Value, ComponentSection.Panel(shape.Section), Coring.None, shape.Standard, shape.Section.CapacityKey, shape.Section.AppearanceId, default)
                .Map(item => (shape.Id, Item: item)).ToOption())
            .ToFrozenDictionary(static r => r.Id, static r => r.Item);

    // The ComponentId → ComputedSection map the parent component#COMPONENT_OWNER ComponentCatalogue.Sections folds and the
    // [M7] ComponentResolution.Build caches by ProfileRef (the realized sibling to steel#STEEL_FAMILY SteelSections /
    // timber#TIMBER_FAMILY TimberSections). ONLY the steel-deck rows contribute — the rib net section runs ONCE here through
    // the [03] DeckSection (the SAME ParametricSection Green's-theorem integral steel/cmu/timber run over a built Perimeter),
    // a section whose integral fails dropping through Choose (a build-time ComponentFault.Section surfaced in THIS page). The
    // non-structural board kinds (gypsum/sheathing/cement-board/rigid-board) are FILTERED out and join Option<ComputedSection>
    // .None at Build (a covering board is an IfcCovering with a LayerSet, never a profiled section). ComponentId's generated
    // [KeyMemberEqualityComparer] keys the frozen dictionary, so NO explicit comparer is threaded.
    public static FrozenDictionary<ComponentId, ComputedSection> PanelSections(Context context) =>
        Shapes
            .Filter(static shape => shape.Section.Rib.IsSome)
            .Choose(shape => shape.Section.Rib.Bind(rib => DeckSection.RibSection(rib, default).ToOption()).Map(section => (shape.Id, Section: section)))
            .ToFrozenDictionary(static r => r.Id, static r => r.Section);
}
```

## [03]-[DECK_SECTION]

- Owner: `DeckSection` the steel-deck rib `ComputedSection` assembler over a `DeckRib` — the `RibSection` projection building the trapezoidal corrugation `VividOrange.Profiles.Perimeter` over ONE rib pitch and feeding it through the `component#COMPONENT_OWNER` `ParametricSection.Admit` Green's-theorem polygon integral, the section per unit pitch scaled to the per-coverage section the deck-diaphragm/composite-floor design seam reads. `DeckSection.RibSection` is the ONE deck-section boundary railing a degenerate rib (a collapsed trapezoid) through `ComponentFault.Section`.
- Cases: a steel-deck rib section is one `DeckRib` (the form/profile/depth/pitch/base-metal/coverage/gauge) feeding the SAME `ParametricSection` integral the profiled families run — the trapezoidal corrugation `Perimeter` is the OUTER deck-cross-section sheet (the top flange, the two sloped webs, the bottom flange repeated per pitch) minus NO void (a thin-walled open sheet, the section the net sheet area × the rib lever arm), the section per pitch then multiplied by the `RibCountPerCoverage` so the `ComputedSection` is the per-coverage-width deck section a `Rasm.Compute` deck check reads. The rib geometry is the `DeckProfile` standard (the top-flange fraction, the rib depth = web rise, the pitch) so the section is grounded in the SDI profile, never a hand-keyed `Sp`/`Sn`/`I`.
- Entry: `public static Fin<ComputedSection> RibSection(DeckRib rib, Op key)` — the deck rib section projection: it builds the per-pitch trapezoidal sheet `Perimeter` (the `LocalPoint2d` corners of the corrugation centre-line offset by the base-metal half-thickness to a thin closed band, exactly as `ParametricSection.RectanglePerimeter` builds a rectangle's corners), feeds it through `ParametricSection.Admit(new SectionProperties((IProfile)perimeter), depthMm, widthMm, plastics, key)` (the SAME admission the rectangle/hollow families run — the elastic columns from the polygon integral, the plastic/torsion/shear columns from the trapezoid closed forms, the asymmetry columns engineering-zero for the doubly-symmetric-per-pitch corrugation), and scales the per-pitch section to the per-coverage section by the `RibCountPerCoverage`, `Fin<T>` railing a degenerate rib through `ComponentFault.Section`; the deck section is captured ONCE at `ComponentCatalogue.PanelSections` build (the M7 substrate the `ComponentResolution.Build` caches by `ProfileRef`), so a `Rasm.Compute` deck-diaphragm shear / composite-floor stiffness check reads `graph.SectionOf(member)` without re-running the integral.
- Packages: Rasm.Element (project — `SectionProperties` the neutral seam receipt the rib section lifts onto, `ProfileRef` the seam handle the M7 resolution dereferences), Rasm.Domain (project — `Op`/`Context`), VividOrange.Sections.SectionProperties + VividOrange.Profiles.Perimeter + VividOrange.Geometry (the `new Perimeter(outerEdge, voidEdges)` over `LocalPolyline2d`/`LocalPoint2d` fed to `new SectionProperties(IProfile)` through `ParametricSection.Admit` — the SAME solver the rectangle/hollow families run, the `Rasm.Materials/.api` `vividorange-sections-sectionproperties`/`vividorange-profiles-catalogue` catalogues), `component#COMPONENT_OWNER` `ParametricSection`/`ComputedSection`/`ComponentFault` (the shared solver + receipt + fault band), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox; no new external package — the deck section composes the realized `[02]` `DeckRib` and the parent `ParametricSection` solver.
- Growth: a new deck rib profile is one `DeckProfile` row carrying its rib depth/pitch/top-flange fraction the `RibSection` `Perimeter` builds, never a per-profile section method; a new deck form (a cellular acoustic deck, a long-span deck) is one `DeckForm`/`DeckProfile` row the rib section re-derives through the SAME integral; the per-coverage scaling is the one `RibCountPerCoverage` multiply, never a per-profile literal. A composite-floor deck's concrete-topping transformed section (the `IfcSlab` floor's full composite stiffness over the deck + the slab) is the `Rasm.Compute` composite-design concern reading the deck rib section + the concrete `Mechanical` row off the seam, never re-derived here — this owner contributes the bare steel-deck rib section, the topping the analysis seam composes.
- Boundary: `DeckSection` is the steel-deck rib-section assembler — a hand-keyed deck `Sp`/`Sn`/`I` literal is the deleted form, the rib section a REAL net trapezoidal section over a built `Perimeter` through the SAME `ParametricSection` Green's-theorem integral the steel/cmu/timber families run, so a deck section and a glulam rectangle resolve the SAME twenty-field `ComputedSection` and the `Projection/component#COMPONENT_PROJECTOR` `SeamSection` lifts the deck rib section onto the seam `SectionProperties` column-for-column without re-resolving or admitting VividOrange downstream; the trapezoidal corrugation is the per-pitch thin-walled sheet (the top flange `RibPitchMm · TopFlangeFraction`, the two sloped webs rising `RibDepthMm`, the bottom flange the pitch remainder) offset by the base-metal half-thickness to a thin closed band, the `Perimeter` the polygon the integral iterates, so the rib `I`/`S`/`r` carry the real corrugation lever arm rather than a flat-plate value; the per-pitch section scales to the per-coverage section by the `RibCountPerCoverage` (the rib count the coverage width admits, a 36in B-deck coverage carrying ~6 ribs at 6in pitch) so the `ComputedSection` is the deck section per coverage width a design check reads, never a per-rib slice; the steel-deck `Component` carries its composition as a seam `ProfileSet` (the `Construction/assembly#MATERIAL_COMPOSITION` `ProfileSet` the M7 `ProfileRef` resolves the section onto) — a deck is genuinely a profiled ribbed sheet, NEVER a `LayerSet` (the layer set is the covering-board path; a deck profiles), so the deck row's seam composition is a `ProfileSet` the `BakeSection` resolves the rib `ComputedSection` onto and the covering-board rows' compositions are `LayerSet`s; the deck base metal reuses the `connector#CONNECTOR_FAMILY` `SteelGauge` cold-formed gauge axis (the 22/20/18/16ga base-metal/design-thickness/yield bands) so the deck and the framing connectors share ONE cold-formed sheet-steel gauge vocabulary, never a parallel deck gauge enum; the rib section is host-neutral scalar DATA the IFC `IfcPlate`/`IfcSlab` wire and the structural-design seam read, NEVER a host brep (the host materializes the corrugated solid from the `DeckRib` columns + the per-pitch `Perimeter` exactly as the connector materializes its `ConnectorPlate`), so this owner stays a leaf below the host boundary; a degenerate rib (a non-positive top flange, a collapsed pitch) rails `ComponentFault.Section` at the build-time integral so a corrupt section never seeds a corrupt deck stiffness on the seam — the `Section` slot, never `Dimension` or `Family`.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Generic;    // List<ILocalPoint2d> (the Perimeter polyline backing)
using LanguageExt;
using Rasm.Domain;                   // Op
using Rasm.Element;                  // SectionProperties, ProfileRef (the seam receipt + handle the rib section lifts onto / the M7 resolution dereferences)
using Rasm.Materials.Component;      // ParametricSection (the shared Green's-theorem solver), ComputedSection (the canonical receipt), ComponentFault (the band)
using VividOrange.Geometry;          // LocalPoint2d, LocalPolyline2d, ILocalPoint2d, ILocalPolyline2d (the Y-Z section-plane corrugation geometry)
using VividOrange.Profiles;          // Perimeter, IProfile (the parametric section input)
using VividOrange.Sections.SectionProperties;   // SectionProperties polygon-integral solver over IProfile
using UnitsNet;                      // Length (the section-plane coordinate quantity)
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component.Panel;

// --- [OPERATIONS] --------------------------------------------------------------------------
// The steel-deck rib-section assembler: the trapezoidal corrugation Perimeter over ONE rib pitch feeds the SAME
// component#COMPONENT_OWNER ParametricSection.Admit Green's-theorem polygon integral the rectangle/hollow families run, so
// a deck rib resolves the canonical ComputedSection without a hand-keyed Sp/Sn/I literal. The per-pitch section scales to
// the per-coverage section by the RibCountPerCoverage; a degenerate rib rails ComponentFault.Section at the integral.
public static class DeckSection {
    public static Fin<ComputedSection> RibSection(DeckRib rib, Op key) =>
        guard(rib.TopFlangeMm > 0.0 && rib.DepthMm.Value > 0.0 && rib.PitchMm.Value > rib.TopFlangeMm,
            ComponentFault.Section(key, $"<deck-rib-degenerate:{rib.Profile.Key}:top={rib.TopFlangeMm:R}/pitch={rib.PitchMm.Value:R}/depth={rib.DepthMm.Value:R}>"))
        .Bind(_ => ParametricSection.Admit(
            new SectionProperties((IProfile)RibPerimeter(rib)),
            depthMm: rib.DepthMm.Value,
            widthMm: rib.PitchMm.Value,
            plastics: RibPlastics(rib),
            key: key))
        .Map(perPitch => Scale(perPitch, rib.RibCountPerCoverage));

    // The trapezoidal corrugation per pitch as a thin closed band: the corrugation CENTRE-LINE (the bottom-left web foot, up
    // the first sloped web to the top flange, across the top flange, down the second sloped web to the bottom-right web foot)
    // offset by the base-metal HALF-thickness to a closed band the Green's-theorem integral iterates — the SAME LocalPoint2d/
    // LocalPolyline2d construction ParametricSection.RectanglePerimeter uses for a rectangle, here tracing the SDI trapezoid
    // (the base spanning the full pitch, the top flange RibPitchMm·TopFlangeFraction wide at the rib-depth rise, the two webs
    // the (pitch - top)/2 slopes between). The band is the real net sheet section, so the rib I/S/r carry the corrugation lever arm.
    static Perimeter RibPerimeter(DeckRib rib) {
        double pitch = rib.PitchMm.Value;
        double depth = rib.DepthMm.Value;
        double top = rib.TopFlangeMm;
        double half = rib.BaseMetalMm.Value * 0.5;
        double webRun = (pitch - top) * 0.5;                                    // the horizontal run of each sloped web: the base spans the full pitch, the top flange `top` centred, so each web runs (pitch - top)/2
        // The centre-line corrugation polyline over ONE pitch (Y the in-plan station, Z the rib elevation): the bottom-left
        // web foot, up the first sloped web to the top-left flange corner, across the top flange, down the second sloped web
        // to the bottom-right web foot — the standard trapezoidal-rib-per-pitch outline (the base is the two web feet meeting
        // the adjacent ribs, so a single-pitch cell carries no separate flat bottom-flange segment). Closed into a thin band
        // by the outer-then-inner offset (+half above the centre-line on the upstroke, −half below on the return), so the
        // closed polygon is the sheet's net cross-section per pitch.
        Seq<(double Y, double Z)> centre = Seq<(double, double)>(
            (0.0, 0.0), (webRun, depth), (webRun + top, depth), (pitch, 0.0));
        return new Perimeter(Band(centre, half), new List<ILocalPolyline2d>());
    }

    // The plastic/torsion/shear columns the elastic polygon integral does not expose, from the thin-walled trapezoid CONSISTENT
    // with RibPerimeter's outline (the sheet segments per pitch are the top flange `top` + the two sloped webs `webLen`, no
    // separate flat bottom-flange segment — the base is the two web feet): the plastic moduli Z ≈ the gross sheet first moment
    // (the thin-wall shape factor ≈1.0 for an open corrugation), the open-section St-Venant torsion J = Σ(b·t³/3) over the
    // flange/web segments (the thin-walled open-section torsion), and the shear area Av the web sheet area (the two sloped webs
    // carrying the diaphragm shear). A corrugation per pitch is doubly-symmetric, so the asymmetry columns are engineering-zero
    // (Admit fills them). The webRun matches RibPerimeter ((pitch - top)/2), so the sheet length is geometrically exact.
    static (double Zx, double Zy, double J, double Avy, double Avz) RibPlastics(DeckRib rib) {
        double t = rib.BaseMetalMm.Value;
        double pitch = rib.PitchMm.Value;
        double depth = rib.DepthMm.Value;
        double top = rib.TopFlangeMm;
        double webRun = (pitch - top) * 0.5;
        double webLen = Math.Sqrt(webRun * webRun + depth * depth);
        double sheetLen = top + 2.0 * webLen;                                   // the developed sheet length per pitch (top flange + two sloped webs)
        double zx = sheetLen * t * depth * 0.5;                                 // thin-wall plastic modulus ≈ sheet area × depth/2 lever
        double zy = sheetLen * t * pitch * 0.25;
        double j = sheetLen * t * t * t / 3.0;                                  // open thin-walled section St-Venant torsion Σ b·t³/3
        double avWeb = 2.0 * webLen * t;                                        // the two sloped webs carry the diaphragm shear
        return (zx, zy, j, avWeb, sheetLen * t);
    }

    // Scale the per-pitch ComputedSection to the per-coverage section: the area/inertia/moduli/shear scale by the rib count
    // (the coverage carries `ribs` corrugations), the radii/depth/width unchanged (a per-pitch geometric property), the
    // perimeter scaled. The scaled columns mint through the total PositiveMagnitude.Create (the SAME derived-from-admitted
    // mint timber#TIMBER_FAMILY ResidualSection uses) — a positive per-pitch section scaled by a >= 1 count stays positive,
    // the per-pitch section already having passed the ParametricSection.Admit rail, so the mint never re-admits nor throws
    // for a valid rib. The scaled section is one fresh ComputedSection over the multiplied values (a struct receipt).
    static ComputedSection Scale(ComputedSection s, int ribs) {
        double n = ribs;
        PositiveMagnitude Mul(PositiveMagnitude v) => PositiveMagnitude.Create(v.Value * n);
        return new ComputedSection(
            Mul(s.AreaMm2), Mul(s.IxMm4), Mul(s.IyMm4), Mul(s.SxMm3), Mul(s.SyMm3), s.RxMm, s.RyMm,
            Mul(s.ZxMm3), Mul(s.ZyMm3), Mul(s.JMm4), IwMm6: s.IwMm6 * n, Mul(s.AvyMm2), Mul(s.AvzMm2),
            s.DepthMm, s.WidthMm, Mul(s.HeatedPerimeterMm), AxisDistanceMm: 0.0, ShearCentreYMm: 0.0, ShearCentreZMm: 0.0, MonosymmetryFactor: 0.0);
    }

    // Close a centre-line corrugation polyline into a thin band by offsetting +half on the forward stroke and −half on the
    // reverse, so the closed polygon is the sheet's net cross-section: the forward corners offset up, the reversed corners
    // offset down, the two halves meeting at the ends. Each corner a LocalPoint2d in the Y-Z section plane (the SAME
    // Length.FromMillimeters construction ParametricSection.Loop uses).
    static ILocalPolyline2d Band(Seq<(double Y, double Z)> centre, double half) {
        Seq<ILocalPoint2d> forward = centre.Map(c => (ILocalPoint2d)new LocalPoint2d(Length.FromMillimeters(c.Y), Length.FromMillimeters(c.Z + half)));
        Seq<ILocalPoint2d> reverse = centre.Rev().Map(c => (ILocalPoint2d)new LocalPoint2d(Length.FromMillimeters(c.Y), Length.FromMillimeters(c.Z - half)));
        return new LocalPolyline2d((forward + reverse).ToList());
    }
}
```

## [04]-[RESEARCH]

- [PANEL_ROW_TRANSCRIPTION]: REALIZED — the ASTM C1396 gypsum / ASTM C1177 glass-mat sheathing / APA PRP-108 wood-structural-panel / ASTM C1325 cement-board / ANSI-SDI steel-deck / ASTM C578-C1289 rigid-board catalogue seeds through `ComponentCatalogue.BuildPanelRows(context)` over the `PanelRow` designation/kind/dimension/edge/orientation/fastening/property table keyed `panel.<designation>`, the board length/width/thickness and the fastening field/edge/edge-distance columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` into the kernel value-object and the kind/edge/orientation/core/span/bond/foam/facer algebra realized as the panel vocabulary; a new board is one `PanelRow` data addition plus, if novel, one axis row. The catalogue is HAND-ROLLED in-fence because no admitted .NET package owns sheet-goods dimension tables — VividOrange owns the structural-MEMBER section catalogues (AISC v16.0 + EN 10365) and EN grade factories, NOT the gypsum/sheathing/deck/insulation board rosters — exactly as `fastener#FASTENER_FAMILY` hand-keys the ISO 898-1 bolt property classes VividOrange's EN-only member-grade data does not own. The gypsum board dimensions transcribe ASTM C1396/C1396M-24 (the modular 48in/54in widths, the 8-16ft lengths, the 1/4in/3/8in/1/2in/5/8in thicknesses, the tapered long edge for the joint-compound recess, the `CoreType` regular/Type-X/Type-C/moisture-resistant/abuse-resistant cores) and EN 520; the gypsum sheathing the ASTM C1396 §gypsum-sheathing / ASTM C1177 glass-mat (the 48in width, 8-10ft length, 1/2in/5/8in thickness, glass-mat facer); the plywood/OSB the APA PRP-108 / US PS 1-19 / PS 2-18 (the 48in×8ft sheet, the 3/8in-3/4in thicknesses, the `SpanRating` 24/0..48/24 dual roof/floor spacings, the Exposure-1/Exterior `BondClass`, the long-length OSB to 24ft) and EN 13986/636/300; the cement board the ASTM C1325 / ANSI A118.9 (the 32in/36in/48in widths, the 5/8/10ft lengths, the 1/4in-5/8in thicknesses, the glass-mesh scrim); the steel deck the ANSI/SDI RD-2017 (roof) / C-2017 (composite) (the 36in B/A/F or 24in N coverage, the 22-16ga base metals, the wide-rib-B/narrow-rib-A/intermediate-F/deep-N roof profiles and the 2VLI/3VLI composite profiles); the rigid board the ASTM C578 (EPS/XPS) / C1289 (polyiso) (the 24in/48in widths, the 4/8ft lengths, the 1/2in-6in thicknesses, the square/shiplap/tongue-groove edges, the foil/glass-fiber-mat/coated-glass facers). A non-positive column rails the `AcceptValidated` `Fin`, a malformed `FastenPattern` rails `ComponentFault.Dimension`, an unknown kind/edge/property rails the matching `ComponentFault` case so a malformed row drops through `Choose` rather than seeding a degenerate `Component`.
- [BOARD_VS_SUBSTANCE_SPLIT]: REALIZED — the buildable BOARD is the `Component` and the board SUBSTANCE a `properties#MATERIAL_PROPERTY_CATALOGUE` `MaterialId` row, the split the unified Material/Component/Element paradigm demands (`RASM-REBUILD-SCOPE.md` [1]). `PanelKind.SubstanceId` is the substance `MaterialId` (`gypsum.board`/`cement.board`/`wood.plywood`/`wood.osb`/`insulation.eps`/`insulation.xps`/`insulation.pir`) whose `Mechanical`/`Thermal` row the analysis seam reads by key — the board physics (the gypsum density/strength, the wood-panel modulus, the foam conductivity) read ONCE from the property library, NEVER re-keyed here. The `gypsum.board`, `insulation.eps`, `insulation.xps`, `insulation.pir` substance rows ALREADY live in the `properties#MATERIAL_PROPERTY_CATALOGUE` roster (EN 520 gypsum, EN 13162-13166 insulation), so a gypsum/rigid board's substance resolves a REQUIRED `Mechanical`/`Thermal` row there; the `cement.board`, `wood.plywood`, `wood.osb` substance rows the deepened catalogue grows beside the existing `gypsum.board` (the cement board's portland-cement-aggregate physics, the plywood/OSB wood-panel orthotropic-surrogate physics) are the one cross-file precondition (a `properties#MATERIAL_PROPERTY_CATALOGUE` row addition). The buildable board with its dimensions, layup, edge, and fastening IS the `Component`, so a `5/8in` Type-X gypsum board and a `1/2in` plywood sheathing are `Component` rows differing in the `PanelSection` columns, never a `gypsum.board`-Component (the substance is the Properties row) and never a board Properties row (the board is the Component).
- [PANEL_LAYUP_GEOMETRY]: REALIZED — the face/core layer stack is the typed `Seq<PanelLayer>` (the per-layer role + thickness + `MaterialId`), the generative datum a generator builds the real layered board from. A gypsum board is a paper-face/gypsum-core/paper-face stack (the two ≈0.5 mm paper faces over the gypsum core the remainder), a gypsum sheathing a glass-mat-facer/gypsum-core/glass-mat-facer stack, a cement board a glass-mesh-scrim/cement-aggregate-core/glass-mesh-scrim stack, a plywood/OSB a single veneer-ply/strand-layer the host subdivides into the real plies, a rigid board a facer/foam-core/facer stack (the foil/glass-fiber-mat/coated-glass facer over the foam core); the layup sums to the board thickness so the `CompositionAuthor.LayerSet` coerces it into the seam `IfcMaterialLayerSet` the SAME way a CLT panel's `PlyLayup` (`timber#TIMBER_LAYUP_GEOMETRY`) and an IGU's pane stack (`glazing#GLAZING_FAMILY`) do — a covering board is genuinely a layered material, never a single-thickness scalar smear forcing a five-layer board into one column. The `PanelLayerRole` discriminates a facing layer from the core so the host renders the paper/glass-mat/foil face distinctly; a steel deck carries no layup (one galvanized sheet — its geometry rides the `DeckRib` corrugation, not the layup). The fastening is the typed `FastenPattern` (the field+edge spacing + edge distance + `FastenerType`, each spacing guarded `> 0` at `Create`), the real fastener grid the `Construction/layout#ASSEMBLY_FOLD` `Sheathing` stage places as `Placement` rows — a plywood's 6in-edge/12in-field 8d-nail schedule, a gypsum's 8in-edge/12in-field drywall-screw schedule, a deck's 12in-support weld + side-lap screw — never a free nail count.
- [STEEL_DECK_RIB_SECTION]: REALIZED — the steel-deck rib `ComputedSection` is a REAL net trapezoidal section over a built `VividOrange.Profiles.Perimeter`, NOT a hand-keyed `Sp`/`Sn`/`I` literal (the `apiUnderutilized` closure — the `ParametricSection` Green's-theorem polygon integral over a built `Perimeter`, the steel-deck rib stacking the SAME solver the steel family runs). `[03]-[DECK_SECTION]` `DeckSection.RibSection` builds the per-pitch trapezoidal corrugation `Perimeter` (the SDI `DeckProfile` top-flange `RibPitchMm·TopFlangeFraction` at the rib-depth rise, the two sloped webs, the bottom flange the pitch remainder, offset by the base-metal half-thickness to a thin closed band the integral iterates) and feeds it through the `component#COMPONENT_OWNER` `ParametricSection.Admit(new SectionProperties((IProfile)perimeter), depthMm, widthMm, plastics, key)` — the SAME admission the rectangle/hollow parametric families run (the elastic `Area`/`I`/`S`/`r`/`Perimeter` from the polygon integral, the plastic/torsion/shear columns from the thin-walled trapezoid closed forms, the asymmetry columns engineering-zero for the doubly-symmetric-per-pitch corrugation), then scales the per-pitch section to the per-coverage section by the `RibCountPerCoverage` (the 36in B-deck coverage carrying ~6 ribs at 6in pitch). A steel-deck row contributes its rib `ComputedSection` to `ComponentCatalogue.PanelSections` (the steel-deck-only filter — the non-structural covering boards join `Option<ComputedSection>.None`), the M7 substrate the `ComponentResolution.Build` caches by `ProfileRef` so a `Rasm.Compute` deck-diaphragm shear / composite-floor stiffness check reads `graph.SectionOf(member)` without re-running the integral, the deck's seam composition a `ProfileSet` (a ribbed sheet profiles, never a `LayerSet`). The deck base metal reuses the `connector#CONNECTOR_FAMILY` `SteelGauge` cold-formed gauge axis (the 22/20/18/16ga bands), one cold-formed sheet-steel gauge vocabulary shared by the deck and the framing connectors. A degenerate rib rails `ComponentFault.Section` at the build-time integral. Ripple counterpart: `Rasm.Compute` (the deck-diaphragm shear + composite-floor transformed-section stiffness over the deck rib section + the concrete topping).
- [IFC_PANEL_WIRE]: REALIZED — a panel round-trips to IFC 4.3 as its kind-determined leaf, the THIRD `ComponentSection` projection mode (a kind-determined leaf, distinct from the four discrete-part families projecting a fixed leaf and the five profiled families projecting the `ComponentClass` supertype). The assay-confirmed `GeometryGym.Ifc` leaves: a space-bounding covering (gypsum-board/gypsum-sheathing/cement-board/rigid-board) is an `IfcCovering` (`IfcCovering : IfcBuiltElement`) carrying `PredefinedType` ∈ `IfcCoveringTypeEnum` {CLADDING (the wall-lining and exterior-sheathing covering — the enum has NO SHEATHING member, so a partition lining and an exterior gypsum/cement sheathing are both CLADDING), INSULATION (the rigid board), CEILING (a ceiling-board variant via a kind-row column), FLOORING (a cement-board underlayment variant)}; a wood structural panel (plywood/OSB) is an `IfcPlate` (`IfcPlate : IfcBuiltElement`) carrying `PredefinedType` ∈ `IfcPlateTypeEnum` {SHEET}; a steel ROOF/FORM deck is an `IfcPlate`/SHEET and a steel COMPOSITE FLOOR deck an `IfcSlab` (`IfcSlab : IfcBuiltElement`) carrying `PredefinedType` ∈ `IfcSlabTypeEnum` {FLOOR, ROOF} (the enum has NO COMPOSITE/DECK member, and `GeometryGym.Ifc.IfcDeck` does NOT exist — assay-confirmed `Could not find type definition GeometryGym.Ifc.IfcDeck` — so a steel deck is NEVER an `IfcDeck`, a roof/form deck taking `IfcPlate`/SHEET and a composite floor deck `IfcSlab`/FLOOR). The `PanelKind.IfcEntity`/`IfcPredefinedType` tokens are the GeometryGym-verified member spellings the `Rasm.Bim` egress gate reads to choose the IFC entity class and validate the predefined token against the frozen `IfcClass` valid-set AND the schema span ([C6]), this owner carrying only the portable strings the `Projection/component#COMPONENT_PROJECTOR` `ComponentProjector` lands on the seam Type `Object` node's `Classification`/`PredefinedType`; a covering/insulation board's composition is the seam `IfcMaterialLayerSet` (the `CompositionAuthor.LayerSet` over the `PanelLayer` stack) and a steel deck's the `IfcMaterialProfileSet` (the rib `ProfileSet` the M7 `ProfileRef` resolves the rib `ComputedSection` onto). The neutral realization detail bag carries the deck rib depth/pitch, the field/edge fastener spacing, and the board thickness/edge-profile a generator round-trips (the seam-declared `DetailSchema` over the canonical `PropertyName` vocabulary; the IFC `Pset_*` name a `Rasm.Bim` egress concern), the panel `ComponentSection.Panel` arm in `Projection/component#COMPONENT_PROJECTOR` carrying a panel detail bag. The wire mapping is the `Rasm.Bim` boundary projection, host-neutral here — this page emits the verified token columns + the scalar board/rib/layup/fastening columns, never an IFC entity. The bag is bound to the Type `Object` via `Assign.PropertyDefinition` and, because a panel is an `IfcBuiltElement` (never a realizing element the `Semantics/connection#CONNECTION_DETAIL` reader discriminates onto its empty-bag `_` arm), it round-trips through the GENERAL Bim `Object`/property fold — `Rasm.Bim` `Projection/egress#IFC_EGRESS` `ReauthorProperties` re-authoring the `IfcPropertySet` at `Emit` and `Projection/semantic#SEMANTIC_PROJECTOR` `Bags` landing it back at ingest — not the realizing-element connection reader. Ripple counterpart: `Projection/component#COMPONENT_PROJECTOR`'s panel arm (the panel detail bag), the `Properties/property#DETAIL_SCHEMA` panel-row `PropertyName` statics + the `Fastened` `JointTypes` token the seam owner adds, and `Rasm.Bim` `Projection/egress#IFC_EGRESS` mapping the neutral `DetailSchema` `SetName` onto the IFC `Pset_*` name at egress.
