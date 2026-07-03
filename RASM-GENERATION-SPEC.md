# [RASM_GENERATION_SPEC]

The durable blueprint for `Rasm.Generation` — the APP-PLATFORM generative engine that turns catalogue rows, grammar rules, and sited occurrences into `ElementGraph` deltas and host-neutral placement streams. The folder is NOT stood up; this spec is the decided design law a future stand-up realizes verbatim. Strata: APP-PLATFORM, depending up on `{Rasm, Rasm.Element, Rasm.Materials, Rasm.Bim, Rasm.Fabrication}`; the live-document bake stays HOST-BOUNDARY (`Rasm.Rhino`/`Rasm.Grasshopper`), so every emission here is host-neutral scalar data and kernel geometry, never a plugin-bound output.

## [01]-[INDEX]

- [02]-[ENGINE]: the typed production grammar — the closed `Production` rule algebra, `GrammarRule`, the `Generated` receipt, and `AssemblageProjection : IElementProjection` evaluation onto the seam; the explicit rejection of the per-course coursing fold.
- [03]-[ROW_PLANES]: the three thing-row currencies — `OpeningRow`, `AssemblageRow`, `PathRow` — with `SlotBinding`, `LiningVector`, `PanelVector`; a new window, wall, roof, or member run is one data row.
- [04]-[SPINE_AND_PLACEMENT]: the emission vocabulary law — forms live in kernel geometry (`SpineRef`), `Placement` is the one host-neutral emission record, and the KERNEL GATE sequencing law.
- [05]-[LAYOUT_PROBLEM]: the honest problem statement — the five layout problem classes (pattern/tiling, member networks, surface development, connections/corners/edges, openings), the three-data-plane law, and the masonry seed instance.
- [06]-[EXPOSURE_CONTRACT]: the Materials/Element/Bim/Fabrication surfaces this engine consumes, all landed and frozen.
- [07]-[FAULT_RESERVATION]: the `GenerationFault` charter under the reserved `FaultBand.Generation` band 2350.
- [08]-[REGISTRATION]: the permanent strata fixtures.

## [02]-[ENGINE]

The engine is an in-fence C# typed production grammar: hierarchical split-grammar refinement for spatial subdivision plus a typed graph-rewrite tier for topology-dominant assemblies, authored as ONE closed rule algebra with a `Generic` passthrough, evaluated onto the `ElementGraph` through the existing seam contract. A new generative behavior is a `GrammarRule` ROW binding existing `Production` cases; a new production case is the only engine edit, at grammar-formalism cadence, never at thing cadence. No new package: the engine is expression-shaped C# over the admitted substrate (Thinktecture unions, LanguageExt rails, the `Rasm` kernel geometry operations).

REJECTED BY NAME: the per-course `ConstructionLayout.Resolve` fold (the retired `Rasm.Materials/Construction/layout.md` owner). A hand-staged election over five hardcoded placement kernels does not scale to thousands of assemblage types; coursing, arches, domes, piers, and sheet layup are GRAMMAR DERIVATIONS over the parameter rows in `[05]`, produced by `Course`/`Layer`/`RepeatSplit` productions, never a second bespoke layout fold beside the grammar.

```csharp
// --- [TYPES] -------------------------------------------------------------------------------
// The closed production algebra. Split productions own spatial refinement (the CGA lineage);
// Course/Layer own the material-placement derivations; Insert/Open/Instance are the terminals
// binding catalogue rows; Rewrite owns the topology tier over the seam's 5-kind edge algebra;
// Generic is the never-drop passthrough mirroring the seam Relationship.Generic precedent.
[Union]
public abstract partial record Production {
    private Production() { }
    public sealed record Extrude(ParamRef Profile, ParamRef Path, ParamRef Material) : Production;          // a SectionProfile swept along a kernel spine
    public sealed record ComponentSplit(FaceSelector Faces, RuleRef Each) : Production;                     // solid -> front/side/roof/soffit faces
    public sealed record RepeatSplit(SplitAxis Axis, ParamRef Module, RuleRef Each) : Production;           // floors, bays, tiles, courses, studs — flexible repeat
    public sealed record Setback(ParamRef Distance, RuleRef Inner) : Production;
    public sealed record Course(ParamRef Unit, ParamRef Path, ParamRef Bond, ParamRef Joints) : Production; // a ComponentId repeated per bond payload -> Placement stream
    public sealed record Layer(ParamRef Buildup, RuleRef PerPly) : Production;                              // seam LayerSet buildup, NormalOffsetMm-stacked
    public sealed record Insert(ComponentId Part, AnchorRule Anchor) : Production;                          // terminal: a Materials catalogue row placed
    public sealed record Open(OpeningId Opening, AnchorRule Anchor) : Production;                           // terminal: an OpeningRow realized into a void + fill
    public sealed record Instance(AssemblageId Child, Seq<ParamBinding> Bindings) : Production;             // sub-assemblage recursion
    public sealed record Rewrite(GraphPattern Match, GraphPattern Replace) : Production;                    // topology tier over the 5-kind edge algebra
    public sealed record Generic(string RuleName, Map<PropertyName, PropertyValue> Attributes) : Production;// passthrough — no rule is ever dropped
}

// The closed placement-anchor vocabulary the Insert/Open terminals dispatch on: Cell places once
// per active repeat/grid cell, Station places at the stations the active RunPath/pattern scope
// derives, UserDefined defers to a named grammar-library resolver at grammar-formalism cadence.
[SmartEnum<string>]
public sealed partial class AnchorRule {
    public static readonly AnchorRule Cell = new("cell");
    public static readonly AnchorRule Station = new("station");
    public static readonly AnchorRule UserDefined = new("userdefined");
}

public sealed record GrammarRule(RuleRef Key, Seq<RuleGuard> Guards, Seq<Production> Body);

// The typed evaluation receipt: the merged delta, the per-node host-neutral placement streams,
// and the assemblage expansion trace a debugger and a cost rollup read.
public sealed record Generated(GraphDelta Delta, Map<NodeId, Seq<Placement>> Placements, Seq<AssemblageId> Expanded);

// --- [SERVICES] ----------------------------------------------------------------------------
// The evaluation contract composes the EXISTING seam floor — Fin<GraphDelta> Project(ProjectionContext)
// — zero new seam interfaces. Evaluate binds the assemblage row's parameter schema against the supplied
// bindings (GenerationFault.Binding on a miss or bound violation), folds the rule Body to per-step deltas
// merged through the seam GraphDelta monoid, mints the assemblage Type via NodeId.RootedType over the
// row's canonical content (representation-excluded, the frozen Type-identity law), binds occurrences via
// Assign.TypeDefinition and children via Compose, recurses Instance under an ancestry set (a cycle rails
// GenerationFault.Program), and emits ONLY the five existing edge kinds plus Generic.
public sealed class AssemblageProjection : IElementProjection {
    public static AssemblageProjection Of(
        FrozenDictionary<AssemblageId, AssemblageRow> assemblages,
        FrozenDictionary<RuleRef, GrammarRule> rules,
        FrozenDictionary<ComponentId, Component> parts,
        AssemblageId root, Seq<ParamBinding> bindings) => new(assemblages, rules, parts, root, bindings);

    public Fin<GraphDelta> Project(ProjectionContext ctx) =>
        Evaluate(root, bindings, ancestry: Seq(root), ctx).Map(static generated => generated.Delta);
}
```

Evaluation law, decision-complete for the stand-up:
- `Evaluate` is one fold: resolve the `AssemblageRow`, validate `Bindings` against the row's `SlotBinding` schema, elect the `GrammarRule` by `Guards`, fold `Body` productions left-to-right, each step contributing a `GraphDelta` merged through the seam `GraphDelta.Merge` monoid.
- Parameter resolution: each `ParamRef` a production carries resolves against the row's `Seq<ParamDef>` schema joined with the supplied `Seq<ParamBinding>` — an absent binding takes the `ParamDef` default; a missing name, an out-of-`[Min,Max]` value, or a `QuantityRow`-kind mismatch rails `GenerationFault.Binding`. The `SlotBinding` validation above is the orthogonal component-slot check on the same rail.
- `RepeatSplit` and `Course` iterations bind the reserved index parameter (`PropertyName` `"index"`, the cell/course ordinal) into the child scope before evaluating `Each`/the per-unit body, so guards and sub-rules are index-parametric (course parity, staggered end joints, indexed station offsets) through an ordinary `ParamRef` — never a bespoke per-ordinal rule.
- `AnchorRule` resolution: `Cell` places the terminal once per active repeat/grid cell; `Station` places at the stations the active `RunPath`/pattern scope derives; `UserDefined` resolves through the named grammar-library resolver.
- The assemblage TYPE node mints exactly as the Materials `ComponentProjector` mints a Component Type: `NodeId.RootedType` over the row's canonical content with `Representations` excluded, so identical assemblage rows dedup to one Type and a geometry attach never re-keys.
- Occurrences bind via `Relationship.Assign` with `AssignKind.TypeDefinition`; part-of structure via `Relationship.Compose`; openings via `Relationship.Void`; material association via `Relationship.Associate` carrying the `CompositionAuthor.UsageOf`-derived `MaterialUsage`; `Rewrite` topology, mating, and adjacency constraints emit `Relationship.Connect` — so the grammar exercises the full five-kind edge algebra plus `Generic`.
- `Instance` recursion threads an ancestry `Seq<AssemblageId>`; re-entering an ancestor rails `GenerationFault.Program`.
- Every Materials-minted IFC stamp on a generated node is validated composition-time by the Bim `IfcLegality` vocabulary arms at `Assemble` — the engine never carries its own IFC validity logic.
- `Placements` streams are host-neutral `Placement` tuples per `[04]`; the HOST-BOUNDARY materializes them (the `Seq<Placement>`, rebar-bend, and dome ring-course materialization seam formerly recorded on the Materials Construction boundary row).

## [03]-[ROW_PLANES]

Three row currencies own the thing space. The type space is the ROW PRODUCT — kind vocabularies times typed parameter vectors times slot bindings — so thousands of opening, path, and assemblage types share one engine and a small rule library. A new thing is one row in one catalogue; zero type edits.

```csharp
// --- [MODELS] ------------------------------------------------------------------------------
// The opening plane: the IfcDoorType/IfcWindowType move. Kind x operation/partitioning vocabulary
// (seed SmartEnums over the full IFC operation token sets with a USERDEFINED tail) x a typed lining
// vector x N panel vectors x an optional catalogue fill. Absence is Option, never a NaN sentinel.
public sealed record OpeningRow(
    OpeningId Id, IfcBinding Ifc, OpeningPartition Partitioning,
    LiningVector Lining, Seq<PanelVector> Panels, Option<ComponentId> Fill);

public sealed record LiningVector(
    PositiveMagnitude LiningDepthMm, PositiveMagnitude LiningThicknessMm,
    Option<PositiveMagnitude> LiningOffsetMm, Option<PositiveMagnitude> MullionThicknessMm,
    Option<PositiveMagnitude> FirstMullionOffsetMm, Option<PositiveMagnitude> TransomThicknessMm,
    Option<PositiveMagnitude> FirstTransomOffsetMm);

public sealed record PanelVector(PanelOperation Operation, PanelPosition Position, PositiveMagnitude FrameDepthMm, PositiveMagnitude FrameThicknessMm);

// The typed-and-bounded parameter bookends closing the ParamDef -> ParamRef -> ParamBinding triad:
// a row DECLARES its numeric surface as QuantityRow-typed, bounded, defaulted ParamDefs; a caller
// SUPPLIES ParamBindings; productions READ via ParamRef. Bounded declarations are what keep
// thousands of types DATA — never free doubles. OpeningRow carries no free schema by design: its
// numeric surface is the fully-typed LiningVector/PanelVector columns.
public readonly record struct ParamDef(PropertyName Name, QuantityRow Kind, double Min, double Max, PropertyValue Default);
public readonly record struct ParamBinding(PropertyName Name, PropertyValue Value);

// The assemblage plane: a rule reference plus typed slot bindings plus the bounded parameter
// schema, so thousands of assemblage types share a small grammar library. SlotBinding is the
// closed slot vocabulary; a new slot kind is a case at framing-practice cadence, never per
// assemblage.
public sealed record AssemblageRow(
    AssemblageId Id, AssemblageKind Kind, IfcBinding Ifc, RuleRef Rule, Seq<ParamDef> Schema,
    Seq<SlotBinding> Bindings, Seq<OpeningRef> Openings, JointPolicy Joints);

[Union]
public abstract partial record SlotBinding {
    private SlotBinding() { }
    public sealed record Frame(ComponentId Member, double SpacingMm) : SlotBinding;
    public sealed record Track(ComponentId Member, int Count) : SlotBinding;
    public sealed record Sheath(ComponentId Board, int Layers, bool BothSides) : SlotBinding;
    public sealed record Fasten(ComponentId Fastener, PatternRef Pattern) : SlotBinding;
    public sealed record Fill(ComponentId Part) : SlotBinding;
}

// The path plane: the RailClone move — modules repeated along a kernel-curve spine ([04] SpineRef).
public sealed record PathRow(PathId Id, SpineRef Spine, Seq<SlotBinding> Modules, ModuleRepeat Repeat, JointPolicy Joints);
```

Diff-of-next-thing, priced (zero type edits each):

| Thing | Row |
|---|---|
| Window | one `OpeningRow`: `new OpeningRow(OpeningId.Of("window.casement-double"), IfcBinding.Of("IfcWindow", "WINDOW"), OpeningPartition.DoublePanelVertical, new LiningVector(Pm(90), Pm(40), Some(Pm(25)), Some(Pm(60)), Some(Pm(600)), None, None), Seq(new PanelVector(PanelOperation.SideHungLeftHand, PanelPosition.Left, Pm(60), Pm(40)), new PanelVector(PanelOperation.SideHungRightHand, PanelPosition.Right, Pm(60), Pm(40))), Fill: Some(ComponentId.Of("glazing.igu-4-16-4")))` |
| Stud wall | one `AssemblageRow`: `new AssemblageRow(AssemblageId.Of("wall.stud-92-gyp2"), AssemblageKind.Assembly, IfcBinding.Of("IfcWall", "PARTITIONING"), RuleRef.Of("grammar.frame-sheath"), Seq<ParamDef>(), Seq<SlotBinding>(new SlotBinding.Frame(ComponentId.Of("timber.38x89-c24"), 406), new SlotBinding.Track(ComponentId.Of("timber.38x89-c24"), 3), new SlotBinding.Sheath(ComponentId.Of("panel.gyp-x-15.9"), 2, true), new SlotBinding.Fasten(ComponentId.Of("fastener.screw-drywall-41"), PatternRef.Of("pattern.gyp-field305-edge203"))), Seq<OpeningRef>(), JointPolicy.Standard)` |
| Roof | one `AssemblageRow`: `new AssemblageRow(AssemblageId.Of("roof.gable-truss-osb"), AssemblageKind.Surface, IfcBinding.Of("IfcRoof", "GABLE_ROOF"), RuleRef.Of("grammar.pitched-plane"), Seq<ParamDef>(), Seq<SlotBinding>(new SlotBinding.Frame(ComponentId.Of("timber.38x140-c24"), 610), new SlotBinding.Sheath(ComponentId.Of("panel.osb-11.9"), 1, false), new SlotBinding.Fasten(ComponentId.Of("fastener.nail-8d"), PatternRef.Of("pattern.sheathing-roof"))), Seq<OpeningRef>(), JointPolicy.Standard)` |
| Member run with hardware | one `PathRow`: `new PathRow(PathId.Of("run.w14-lus26"), SpineRef.OfLine(6100), Seq<SlotBinding>(new SlotBinding.Frame(ComponentId.Of("steel.W14X90"), 0), new SlotBinding.Fasten(ComponentId.Of("connector.lus26"), PatternRef.Of("pattern.station-610"))), ModuleRepeat.At([0, 610, 1220]), JointPolicy.Standard)` |
| Panel (new board type) | one `ComponentRow` on the Materials `PanelSeed` page — the Materials plane, consumed here by `ComponentId` |

`OpeningPartition`, `PanelOperation`, `PanelPosition`, `AssemblageKind`, `FaceSelector`, `SplitAxis`, and `ModuleRepeat` are seed vocabularies authored at stand-up from the IFC 4.3.2 operation/partitioning token sets with `USERDEFINED` tails; `LiningVector`/`PanelVector` column lists finalize against the IFC 4.3.2 door/window lining- and panel-property attribute tables before stand-up (the one carried research item on this plane).

Id-construction law: the new-plane id types (`OpeningId`, `AssemblageId`, `PathId`, `RuleRef`, `PatternRef`) charter their `Of` factories in this spec; a `ComponentId` appearing in an exemplar row is shorthand for an already-admitted Materials catalogue id — admitted through the on-disk `ComponentId.Validate` form at seed time — never a new factory on the Materials owner.

## [04]-[SPINE_AND_PLACEMENT]

The emission vocabulary law: FORMS LIVE IN KERNEL GEOMETRY, never in Generation enums. A spine, a surface region, a grid, or a field the engine lays out over is a `Rasm` kernel geometry reference — a content-keyed curve/polycurve/surface/mesh region with a parameter domain — and the engine consumes the stations, frames, isolines, geodesics, offsets, and subdivisions the kernel computes, enumerating NO form vocabulary of its own. An enumerated form union is REJECTED AS THE GENERAL MECHANISM: a path-kind union (line/arc/dome), a roof-form enum, a per-system corner case set all collapse the moment the domain's real breadth arrives — a dome is a surface-development layout over a rotational surface, not a path kind; a roof is one surface-development rule family over many kernel surfaces; the target space is unbounded and only the kernel's geometry vocabulary scales to it.

```csharp
// --- [TYPES] -------------------------------------------------------------------------------
// The kernel-geometry spine reference: a content-keyed kernel curve + parameter interval. The
// exact shape (curve vs surface-region overload, frame policy) finalizes at stand-up against the
// kernel surface — a RESEARCH row. OfLine mints the degenerate straight spine for seed rows.
public readonly record struct SpineRef(ContentKey Geometry, double T0, double T1) {
    public static SpineRef Of(ContentKey geometry, double t0, double t1) => new(geometry, t0, t1);
    public static SpineRef OfLine(double lengthMm) => /* the chartered degenerate-line mint */ ;
}

// --- [MODELS] ------------------------------------------------------------------------------
// The host-neutral placed unit — the engine's ONE emission record over any spine or surface
// scope: ordinal indices, spine/surface station scalars, the placement frame scalars, and the
// system vocabulary columns. Never a Rhino.Geometry curve or transform.
public readonly record struct Placement(
    int Course, int Sequence, double StationMm, double ElevationMm, double RunMm, double RiseMm,
    double PathAngleDegrees, Orientation Orientation, Cut Cut, MaterialId Material,
    double NormalOffsetMm = 0.0, double LateralOffsetMm = 0.0);
```

KERNEL GATE (sequencing law): stand-up is GATED on the `Rasm` kernel (Vectors + the greenfield Geometry robust-core) carrying the curve/surface/field operations this engine composes — arc-length stations and frames on arbitrary curves, surface isolines/geodesics/offsets, region tessellation and subdivision, developable/panelization operations, and pattern-to-surface mapping. Where those operations are missing, a Vectors/Geometry rebuild campaign PRECEDES Generation stand-up; this spec never re-implements kernel geometry, and no Generation surface may compensate for a kernel gap with a local geometry kernel.

## [05]-[LAYOUT_PROBLEM]

The problem, stated honestly: layout is THOUSANDS of system logics — wall assemblies, corner and edge resolutions, opening treatments, roof and shell families, paving/cladding/shingling patterns — and NO enumeration of them survives contact with the domain. A CMU wall, a brick wall, and a wood-stud wall are three DIFFERENT construction logics over the same wall surface; a bonded masonry corner, a framed corner, and a control-joint corner are three different corner rules; gable/hip/mansard/dome/barrel/freeform roofs are one surface-development family over different kernel surfaces. Hardcoding any of these as engine cases is the REJECTED form. The engine's three data planes over the one closed production algebra (`[02]`): FORMS are kernel geometry (`[04]`), LOGICS are grammar-rule rows plus per-SYSTEM policy rows, PATTERNS are tiling/symmetry rows.

The layout problem classes every rule library targets — the SCOPE a stand-up implementation must own, never an implementation here:

- PATTERN/TILING — bonds, paving, cladding, shingling: a pattern is CONTEXT-FREE 2D data (unit + symmetry group + joint offsets) mapped onto any surface region in any orientation, and the CONTEXT ROW (wall / facade / pavement / pure pattern) decides the placement semantics — one running-bond definition lays a wall course, a facade panel field, or a paver field without a second pattern definition.
- MEMBER NETWORKS — framing systems: studs/tracks/plates, rafters/purlins/joists, grids and bays; per-SYSTEM spacing, alignment, splice, and interruption logics as policy rows — a CMU coursing system, a platform-frame system, and a CFS system are three row-sets over the same network productions, never three engines.
- SURFACE DEVELOPMENT — the roof/shell/vault family: coursing, layup, panelization, and shingling FIELDS over kernel surfaces; a dome is a surface-development instance (a ring-course field over a rotational surface region), never a path or roof-enum case.
- CONNECTIONS/CORNERS/EDGES — per-system corner, jamb, head, sill, and edge resolutions as rule rows a system binds: the masonry closer alternation, the framed king/jack-stud pattern, and the CMU control joint are three rows of ONE rule family.
- OPENINGS — interruption plus head/jamb/sill resolution, composed from the opening plane (`[03]`) against the ACTIVE system's rules — the same `OpeningRow` interrupts a coursing field, a stud network, and a panel field through the system's own interruption row.

Growth law: a new material system is one policy row-set + its rule-library rows; a new form is a kernel geometry; a new pattern is a tiling row; a new context is a context row — the engine gains a production case only at grammar-formalism cadence.

### [MASONRY_SEED_INSTANCE]

The first rule-library instance — the salvaged masonry vocabulary carried intact from the retired Construction pages, DEMOTED from engine vocabulary to ONE system's seed rows, and the density floor every later system row-set meets. The run algebra (line/arc length `L = r·θ·|sweep|`, dome meridian quarter-arc, per-station tilt) re-lands as this system's coursing-field math over kernel spines. The grammar DERIVES the placements these rows parameterize:

- `ArchProfile` — six multi-centre rows {`segmental` 0.25/1, `semicircular` 0.50/1, `three-centre` 0.40/3, `equilateral` 0.87/2, `lancet` 1.20/2, `ogee` 1.00/4/reversed} carrying `RiseRatio`, `CentreCount`, and the `Ogee` reversed-curvature flag. `SweepDegrees` is `Ogee ? 360.0 : 2·atan(2·RiseRatio)·180/π`; `TiltDegreesAt(sweepFraction)` is the per-station meridian tilt — the ogee reverses curvature at the half-sweep inflection, a multi-centre arch turns uniformly across the accumulated sweep. A voussoir derivation places each wedge `Bevel`-cut and `Soldier`-oriented, radially tilted at its arc station, the centre wedge the keystone.
- `OpeningHead` — three delegate rows {`lintel` keeps the coursing cut flush as stretchers, `soldier-course` rotates every head unit to a soldier, `arch-over` alternates `Bevel`/`KingCloser` soldier wedges} with signature `(int course, Cut incomingCut) -> (Cut, Orientation)`.
- `EdgeCut` — three jamb-detail delegate rows {`toothed` whole/three-quarter alternation, `straight` half-bat, `quoined` whole/half alternation} with signature `(int course) -> Cut`.
- `JointPolicy` — the head/bed/profile/mortar resolution: a specified `MortarJoint` or the `Component.Standard.StandardJointThicknessMm` coordinating fallback, with explicit head/bed overrides; the resolved joint carries the ASTM C270 tooled profile and mortar strength downstream shading and thermal seams read.
- `Opening` condition — `(StationMm, WidthMm, SillElevationMm, HeadElevationMm, EdgeCut JambDetail, OpeningHead Head)` with `Interrupts`/`OnJamb`/`OnHead` predicates: interrupted units drop, jamb units read the `EdgeCut`, the head course reads the `OpeningHead` row.
- `Corner` condition — `(StationMm, TurnDegrees, ClosureRule Closure, int Leaves)`: a single-leaf return closes with the closer cut every course; a multi-leaf bonded corner alternates the closer against course parity over its `Leaves` count.
- Dome derivation — a rotational surface region resolves to a stack of horizontal ring courses springing to crown, each ring a full revolution at latitude radius `R·cos(latitude)`, every ring radially tilted to its meridian angle via `ArchProfile.TiltDegreesAt`, the crown ring the compression keystone — the canonical surface-development instance.
- Pier derivation — the alternating stretcher/header course fold over a pier width.
- Sheathing derivation — boards laid edge-to-edge over the frame: pitch is board length plus the panel `EdgeProfile.GapMm`; run direction follows `PanelOrientation.StrengthAxisPerpendicular`; end joints stagger by the half-board course offset; the closing board rip-cuts through the same closing-cut rule the coursing uses; `FastenPattern` field and edge fastener stations emit as `Placement` rows at `FieldSpacingMm`/`EdgeSpacingMm` inset by `EdgeDistanceMm`.
- Buildup derivation — a seam `MaterialComposition.LayerSet` stacks each genuine ply at its cumulative `NormalOffsetMm`, re-tagging each ply's `Placement.Material`; the primary structural ply at offset zero carries the coursing.
- Bond exposure — masonry bond derivations consume the Materials `BondName`/`BondGeometry` delegate rows directly: `BondGeometry.Course` computes the full per-unit course transform stream (`UnitPlacement.RotationDegrees` folds into `Placement.PathAngleDegrees`, `LateralFraction` into `LateralOffsetMm`, `AlongFraction` into the placed station), never re-interpreted by the engine.

## [06]-[EXPOSURE_CONTRACT]

Every surface below is landed and frozen by the Component-Paradigm campaign; the engine composes them and re-learns nothing.

| Surface | Owner | Contract |
|---|---|---|
| `ComponentCatalogue.Of(context)` | `Rasm.Materials` | `Fin<ComponentCatalogue>` carrying the frozen `Rows` and `Sections` maps; `ComponentId` keys the part plane |
| `SectionProfile` | `Rasm.Materials` | the closed named-dimension profile algebra — the descriptor IS the parametric-solid authoring input `Extrude` consumes |
| `Component.Detail` | `Rasm.Materials` | seed-built `Option<PropertyBag>` realization/product bags (fastener patterns, deck ribs, edge profiles) |
| `graph.SectionOf` + `ComponentResolution.Resolve` | `Rasm.Materials` | Op-free one-hop `(Component, Option<ComputedSection>)`; never re-solve a section |
| `CompositionAuthor` + `UsageOf` | `Rasm.Materials.Projection` | the seam `MaterialComposition` author and the C7 occurrence-usage deriver |
| `Bake`/`TypeId` + `Assign.TypeDefinition` | `Rasm.Element` | type-to-occurrence inheritance; the engine mints Types exactly as `ComponentProjector` does |
| `IfcClass` + `IfcLegality` | `Rasm.Bim` | the generated IFC vocabulary validating every Generation-minted `IfcBinding` stamp at `Assemble` |
| `NestPlan` + `StockNest.Pack` | `Rasm.Fabrication` | rectangular cutting-stock realization of generated cut lists |
| `Placement` + `SpineRef` | this spec | the one host-neutral emission record + the kernel-geometry spine reference; HOST-BOUNDARY materializes |
| curve/surface/field operations | `Rasm` (Vectors + Geometry) | stations/frames, isolines/geodesics/offsets, tessellation/subdivision, panelization, pattern-to-surface mapping — the `[04]` KERNEL GATE |

## [07]-[FAULT_RESERVATION]

Band 2350 is reserved for `GenerationFault` by the `FaultBand.Generation` registry row (`Rasm.Element/Projection/fault#FAULT_TABLES`). The charter: an `Expected`-derived `[Union]` in the seam band shape (no `[GenerateUnionOps]` — fault cases are carriers, and the kernel union-ops generator is strictly opt-in) — nested `…Case` records over `(Op Key, string Detail)`, per-case `Category`, static factories, `Code => FaultBand.Generation`.

- `Path` — a degenerate spine or parameter domain (the `SpineRef` resolution rail).
- `Joint` — a non-positive resolved mortar/module joint on a joint-bearing derivation.
- `Course` — a coursing/ring/pier/sheathing derivation the parameters cannot satisfy (an under-counted dome, an over-long unspliceable board run).
- `Opening` — an opening condition that contradicts its run (a head below its sill, a jamb off the run).
- `Binding` — a `SlotBinding`/`ParamBinding` schema miss or bound violation at `Evaluate`.
- `Program` — a rule-program defect: an `Instance` ancestry cycle, an unresolvable `RuleRef`, a guard set electing no rule.

## [08]-[REGISTRATION]

- `libs/.planning/architecture.md` `[01]-[STRATA]` carries the planned `Rasm.Generation` APP-PLATFORM entry with its dependency rule; `[02]-[DEPENDENCY_DIRECTION]` carries its one-directional consumption law.
- `libs/.planning/planning-targets.md` carries the planned-folder row naming this spec as the seed.
- Stand-up realizes this spec as `libs/csharp/Rasm.Generation/.planning/` with the four index docs, converts `[07]` into the realized `GenerationFault`, and deletes this file only after every section lands in the folder corpus.
