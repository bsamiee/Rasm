# [BIM_ENERGY_DERIVE]

`EnergyDerive` lowers the seam `ElementGraph` to two authoring schemas, and `EnergyTranslate` runs the OpenStudio translator matrix as one frozen `(source, target)` row table. `IfcSpace` nodes — landed by the `Projection/semantic#SEMANTIC_PROJECTOR` IFC ingest or any projector — fold to honeybee `Room`/`Face`/`Aperture`/`Door` envelopes with their layered seam composition lowered onto the energy library under the abridged-reference law, or to dragonfly `Story`/`Room2D` massing plates. `Energy/exchange#ENERGY_EXCHANGE`'s `EnergyExchange.Apply` drives both arms and emits content-keyed `EnergyArtifact`s; class correspondence, boundary payload statics, and evidence bags read back from the `Energy/projector#ENERGY_PROJECTOR` owner, so the raise and the lower cannot drift.

Wire posture is HOST-LOCAL, foreign types emit-confined: each lowered `Hb.Model`/`Df.Model` is authored, `Validate()`-tallied, serialized through `ToJson()`, and released inside the arm; every `OpenStudio.*` SWIG wrapper is `using`-bracketed, and the translate temp-path crossings (`Workspace.save`/`Model.save` over a bracketed scratch file) are the platform-forced statement seam. Faults route the `Model/faults#FAULT_BAND` arms: `CodecReject` (`energy-lower-unsupported`/`energy-translate-miss`), `CapabilityMiss` (`energy-graph-egress-pending`), `ModelRejected` (`energy-decode` on a translate source).

## [01]-[INDEX]

- [02]-[MODEL_DERIVE]: the `EnergyDerive` lower fold — graph → honeybee envelope + energy library (opaque AND glazing constructions, opening sub-faces with their own constructions) over the seam `GeometrySource` port, graph → dragonfly massing with the height/multiplier evidence read back.
- [03]-[TRANSLATE_MATRIX]: the `EnergyTranslate` OSM-centric translator matrix (osm↔gbxml, osm↔idf, osm version-upgrade) as frozen row data over the OpenStudio translators.

## [02]-[MODEL_DERIVE]

- Owner: `EnergyDerive` the BIM-to-BEM lower fold (graph → honeybee HBJSON envelope + energy library, graph → dragonfly DFJSON massing); `BoundaryRow` the closed boundary-condition vocabulary carrying one honeybee closure and its derived dragonfly projection; `EnvelopeFace` the segment-aligned envelope row both arms read.
- Entry: `EnergyDerive.Lower(ElementGraph graph, InterchangeFormat target, EnergyScope scope, GeometrySource geometry, Instant at, Op key)` → `Fin<EnergyOutcome.Emitted>` — dispatches the frozen `Lowers` target table: the `hbjson` arm lowers each scoped `IfcSpace` and its opening sub-faces onto the honeybee envelope + energy library, the `dfjson` arm folds the `Compose` tree onto dragonfly massing plates whose per-segment boundary conditions, window ratios, ground contact, and sky exposure read that same envelope, with the site's un-massed neighbours lowered onto `ContextShade` and the seam georeference onto `ReferenceVector`; each surface and opening composition lowers through ONE property-case fold.
- Auto: lowered models carry the SEMANTIC envelope and library only; simulation context — parameters, run period, conditioning, weather — is Compute's or the python recipe plane's, never authored on the lower. `Envelope` derives each space's bounding surfaces ONCE — face type, boundary row, footprint ring, attributed openings — so the two arms cannot drift about which wall carries which condition or which window, and the dragonfly arm joins those surfaces to floor-boundary SEGMENTS in plan within half a segment length so the parameter lists index the walls they describe.
- Receipt: one `EnergyReceipt` per emit tallies the folded spaces, surfaces, openings, and constructions; the model's `Validate()` DataAnnotations fold into `Warnings` beside the degrade tallies, never an exception.
- Packages: HoneybeeSchema, DragonflySchema, Rasm.Element, Rasm, LanguageExt.Core, NodaTime
- Growth: a new lower target is one row on the frozen `Lowers` target table (the `EnergyProjector.Arms`/`EnergyTranslate.Matrix` row law); a new boundary condition is one `BoundaryRow` row both schemas project from; a richer glazing posture is one `PlateWindow` case swapped at the `Glazing` return with the same measured quotient behind it; per-space program/loads lower as `ProgramTypeAbridged` rows once the seam carries occupancy evidence; a NoMass R-value lower is one arm row the moment the seam carries an R-value-only thermal case; the space-adjacency `Surface` boundary condition needs no new row — `BoundaryRow.Surface` already projects into both schemas and waits only on the raise stamping the counterpart-face payload.
- Boundary: `EnergyDerive` reads the graph through seam-owned surfaces and the `Model/query#ELEMENT_SET` scope algebra; Compute is a peer stratum, never a dependency, so its discipline reads (`SpacesOf`/`BoundingSurfacesOf`) are never referenced. Envelope derivation is SHARED and single — a per-arm boundary walk is the deleted form that let two emitted documents disagree about one building — and the ring area both arms need is the seam owner's `FootprintPolygon.Area` — the Newell fold, shell minus holes, seated where the rings live — because a vertical aperture ring projects to near-zero area under the planar NTS algebra the geospatial owner holds and a page-local ring-area helper re-derives arithmetic the carrier owns. Every missing- or ambiguous-evidence path degrades warning-counted — a footprint-less space, a material lacking both the `Thermal` and `Optical` case, a wall segment no bounding surface matched — never a zero-area fabrication or a fabricated physics row; a zero-area glazing sum emits dragonfly's ABSENT window slot and never a `SimpleWindowRatio` of `0`, which a solver reads as a real zero-area window rather than as no window. `graph→OSM`/`gbXML`/`IDF` DIRECT egress is deliberately absent: no in-process HBJSON→OSM translation is admitted (the python peer's `honeybee-openstudio` leg owns it), and a second graph→OSM builder beside Compute's simulation-scoped `BuildModel` is the duplicate-fold defect, so the request rails `BimFault.CapabilityMiss`. Glazing lowering consumes the same seam `Optical` case (`Discipline.Energy`) Compute's `StandardGlazing` build reads, so the lowered honeybee document and Compute's OSM model agree on layer physics by construction.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LanguageExt;
using NodaTime;
using Rasm;
using Rasm.Bim.Model;
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Composition;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using static LanguageExt.Prelude;
using Df = DragonflySchema;
using Hb = HoneybeeSchema;
using Os = OpenStudio;
// Both schemas close the SAME five boundary cases in DIFFERENT type-argument orders — honeybee's Face slot puts
// Adiabatic third, dragonfly's Room2D slot puts Surface third — so those closures are distinct constructed types
// one shared row projects into; window and shade closures alias for signature density alone.
using FaceBoundary = HoneybeeSchema.AnyOf<HoneybeeSchema.Ground, HoneybeeSchema.Outdoors, HoneybeeSchema.Adiabatic, HoneybeeSchema.Surface, HoneybeeSchema.OtherSideTemperature>;
using PlateBoundary = HoneybeeSchema.AnyOf<HoneybeeSchema.Ground, HoneybeeSchema.Outdoors, HoneybeeSchema.Surface, HoneybeeSchema.Adiabatic, HoneybeeSchema.OtherSideTemperature>;
using PlateWindow = HoneybeeSchema.AnyOf<DragonflySchema.SingleWindow, DragonflySchema.SimpleWindowArea, DragonflySchema.SimpleWindowRatio, DragonflySchema.RepeatingWindowRatio, DragonflySchema.RectangularWindows, DragonflySchema.DetailedWindows>;
using ShadeGeometry = HoneybeeSchema.AnyOf<HoneybeeSchema.Face3D, HoneybeeSchema.Mesh3D>;

namespace Rasm.Bim;

// --- [TYPES] ---------------------------------------------------------------------------------
// BoundaryRow closes the boundary-condition vocabulary both schemas read. Ingest stamps a text token on the seam
// edge and the prior form dispatched it through a `_`-armed text switch per arm — two switches over one string,
// each defaulting an unrecognized token to Outdoors, so a Surface adjacency silently became an exterior wall on
// BOTH sides and neither arm could tell. The row is the closed vocabulary, Face is its honeybee closure, and Plate
// DERIVES from Face rather than declaring a second delegate column: the two closures differ only in the ORDER of
// their five type arguments, so one concrete case re-converts into the other and a per-row second delegate would be
// five more chances for the two documents to disagree about one boundary. The open-object `_` is the exhaustiveness
// floor over AnyOf's untyped payload, never a swallowed case of a closed owner.
[SmartEnum<string>]
public sealed partial class BoundaryRow {
    public static readonly BoundaryRow Ground = new("Ground", static () => (FaceBoundary)new Hb.Ground());
    public static readonly BoundaryRow Outdoors = new("Outdoors", static () => (FaceBoundary)new Hb.Outdoors());
    public static readonly BoundaryRow Adiabatic = new("Adiabatic", static () => (FaceBoundary)new Hb.Adiabatic());
    public static readonly BoundaryRow Surface = new("Surface", static () => (FaceBoundary)new Hb.Surface());
    public static readonly BoundaryRow OtherSideTemperature = new("OtherSideTemperature", static () => (FaceBoundary)new Hb.OtherSideTemperature());

    [UseDelegateFromConstructor]
    public partial FaceBoundary Face();

    public PlateBoundary Plate() => Face().Obj switch {
        Hb.Ground ground => ground,
        Hb.Adiabatic adiabatic => adiabatic,
        Hb.Surface surface => surface,
        Hb.OtherSideTemperature other => other,
        _ => new Hb.Outdoors(),
    };
}

// EnvelopeFace is the ONE segment-aligned envelope row both arms read, so a honeybee envelope and a dragonfly
// massing plate can never disagree about which wall carries which boundary condition or which window: a Room2D
// floor-boundary SEGMENT and a space's vertical bounding SURFACE are one fact seen from two schemas, and deriving
// them twice is exactly how two emitted documents drift. Openings carries the occurrence NODES the raise's Host
// correlation attributed to this face — the honeybee arm lowers each one's construction, the dragonfly arm
// quotients their ring areas against the extruded wall.
readonly record struct EnvelopeFace(
    Node.Object Surface, Hb.FaceType Face, BoundaryRow Condition, FootprintPolygon Ring, Seq<Node.Object> Openings);

// --- [OPERATIONS] --------------------------------------------------------------------------
// BIM-to-BEM lower: graph -> honeybee envelope + library (hbjson) or dragonfly massing (dfjson). Model store
// populates through the ModelEnergyProperties Add* mutators (the canonical lists); faces and openings
// reference by abridged id.
public static class EnergyDerive {
    // Frozen target→arm table (the EnergyTranslate.Matrix and EnergyProjector.Arms row law): a new lower target
    // is ONE row here, never a widened ternary chain; a miss splits CapabilityMiss (a raise-served form whose
    // graph egress is pending) from CodecReject (a form no energy arm serves).
    static readonly FrozenDictionary<InterchangeFormat, Func<ElementGraph, EnergyScope, GeometrySource, Instant, Op, Fin<EnergyOutcome.Emitted>>> Lowers =
        new KeyValuePair<InterchangeFormat, Func<ElementGraph, EnergyScope, GeometrySource, Instant, Op, Fin<EnergyOutcome.Emitted>>>[] {
            new(InterchangeFormat.Hbjson, Honeybee),
            new(InterchangeFormat.Dfjson, Dragonfly),
        }.ToFrozenDictionary();

    internal static Fin<EnergyOutcome.Emitted> Lower(
        ElementGraph graph, InterchangeFormat target, EnergyScope scope, GeometrySource geometry, Instant at, Op key) =>
        Lowers.TryGetValue(target, out var arm)
            ? arm(graph, scope, geometry, at, key)
            : EnergyProjector.Serves(target)
                ? Fin.Fail<EnergyOutcome.Emitted>(new BimFault.CapabilityMiss(key, $"energy-graph-egress-pending:{target.Key}"))
                : Fin.Fail<EnergyOutcome.Emitted>(new BimFault.CodecReject(key, $"energy-lower-unsupported:{target.Key}"));

    static Fin<EnergyOutcome.Emitted> Honeybee(ElementGraph graph, EnergyScope scope, GeometrySource geometry, Instant at, Op key) {
        // Model identifier IS the graph pedigree: a timestamp identifier forks the emitted BYTES per second, so a
        // re-lowered identical graph never byte-matches and the object-plane 412-noop dedup never fires; a
        // content-stable identifier restores the reuse join the dual-key law states.
        ContentAddress pedigree = ContentAddress.OfGraph(graph);
        var store = new Hb.ModelEnergyProperties();
        var state = (Rooms: Seq<Hb.Room>(), Surfaces: 0, Openings: 0, Constructions: 0, Warnings: 0);
        foreach (Node.Object space in SpacesUnder(graph, scope)) {
            var faces = Seq<Hb.Face>();
            foreach (EnvelopeFace bound in Envelope(graph, space, geometry, ref state.Warnings)) {
                state.Surfaces++;
                Option<string> construction = LowerComposition(graph, bound.Surface.Id, store, ref state.Warnings)
                    .Do(_ => state.Constructions++);
                var (apertures, doors) = Openings(graph, bound, geometry, store, ref state.Openings, ref state.Warnings, ref state.Constructions);
                faces = faces.Add(new Hb.Face(
                    bound.Surface.ExternalId.IfNone(bound.Surface.Name), Face3D(bound.Ring), bound.Face,
                    bound.Condition.Face(), new Hb.FacePropertiesAbridged(
                        energy: construction.Match(
                            Some: static id => new Hb.FaceEnergyPropertiesAbridged(construction: id),
                            None: static () => (Hb.FaceEnergyPropertiesAbridged?)null)),
                    apertures: apertures, doors: doors));
            }
            state.Rooms = state.Rooms.Add(new Hb.Room(
                space.ExternalId.IfNone(space.Name), [.. faces], new Hb.RoomPropertiesAbridged()));
        }
        var model = new Hb.Model($"rasm-energy-{pedigree.Value:x32}", new Hb.ModelProperties(energy: store),
            rooms: [.. state.Rooms], units: Hb.Units.Meters, tolerance: graph.Header.Tolerance);
        int warnings = state.Warnings + model.Validate().Count();
        return Fin.Succ(Emit(InterchangeFormat.Hbjson, Encoding.UTF8.GetBytes(model.ToJson()), pedigree, at,
            new EnergyReceipt(EnergyLeg.Lowered, InterchangeFormat.Hbjson, None,
                state.Rooms.Count, state.Surfaces, state.Openings, state.Constructions, warnings,
                default, at)));
    }

    // ONE property-case-discriminated composition lower, dedup-appended through the identifier-keyed
    // Extension-backed Add* mutators; the construction id IS the material-key join (the layer ids + SI
    // thicknesses content-hashed) so identical buildups across N surfaces dedup to ONE library entry. An
    // Optical-free set lowers EnergyMaterial rows (density reads the seam Mechanical case, the 1000 kg/m3
    // Compute fallback mirrored — Thermal carries no density); an all-Optical set lowers
    // EnergyWindowMaterialGlazing rows (the seam nine fractions, conductivity when Thermal rides beside);
    // a MIXED set has no legal EnergyPlus construction — a warning-counted degrade, the Compute
    // BuildConstruction mixed rejection mirrored, never a silently-wrong opaque lowering.
    static Option<string> LowerComposition(ElementGraph graph, NodeId node, Hb.ModelEnergyProperties store, ref int warnings) {
        if (graph.CompositionOf(node).Bind(static c => c is MaterialComposition.LayerSet set ? Some(set) : None).Case
            is not MaterialComposition.LayerSet set) { return None; }
        if (set.Layers.Traverse(layer => graph.Material(layer.Material).Map(m => (Layer: layer, Node: m))).As().Case
            is not Seq<(MaterialLayer Layer, Node.Material Node)> rows) { warnings++; return None; }
        bool anyOptical = rows.Exists(static r => r.Node.Properties.Optical.IsSome);
        if (anyOptical && !rows.ForAll(static r => r.Node.Properties.Optical.IsSome)) { warnings++; return None; }
        return anyOptical ? LowerGlazing(rows, set, store, ref warnings) : LowerOpaque(rows, set, store, ref warnings);
    }

    static Option<string> LowerOpaque(Seq<(MaterialLayer Layer, Node.Material Node)> rows, MaterialComposition.LayerSet set, Hb.ModelEnergyProperties store, ref int warnings) {
        if (rows.Traverse(r => r.Node.Properties.Thermal.Map(thermal => new Hb.EnergyMaterial(
                r.Layer.Material.ToString(), r.Layer.Thickness.Si, thermal.Conductivity.Si,
                r.Node.Properties.Mechanical.Map(static m => m.Density.Si).IfNone(1000.0), thermal.SpecificHeat.Si))).As().Case
            is not Seq<Hb.EnergyMaterial> materials) { warnings++; return None; }
        string id = ContentId("con", set);
        foreach (var m in materials) { store.AddMaterial(m); }
        store.AddConstruction(new Hb.OpaqueConstructionAbridged(id, [.. materials.Map(static m => m.Identifier)]));
        return Some(id);
    }

    // Glazing lower over the seam Optical case (Discipline.Energy): the nine [0,1] fractions map onto the
    // EnergyWindowMaterialGlazing columns 1:1; Thermal conductivity rides when carried (the 0.9 schema default
    // otherwise — the one authoring default the schema itself declares, never fabricated physics).
    static Option<string> LowerGlazing(Seq<(MaterialLayer Layer, Node.Material Node)> rows, MaterialComposition.LayerSet set, Hb.ModelEnergyProperties store, ref int warnings) {
        if (rows.Traverse(r => r.Node.Properties.Optical.Map(o => new Hb.EnergyWindowMaterialGlazing(
                r.Layer.Material.ToString(), thickness: r.Layer.Thickness.Si,
                solarTransmittance: o.SolarTransmittance, solarReflectance: o.SolarReflectanceFront, solarReflectanceBack: o.SolarReflectanceBack,
                visibleTransmittance: o.VisibleTransmittance, visibleReflectance: o.VisibleReflectanceFront, visibleReflectanceBack: o.VisibleReflectanceBack,
                infraredTransmittance: o.ThermalIrTransmittance, emissivity: o.ThermalIrEmissivityFront, emissivityBack: o.ThermalIrEmissivityBack,
                conductivity: r.Node.Properties.Thermal.Map(static t => t.Conductivity.Si).IfNone(0.9)))).As().Case
            is not Seq<Hb.EnergyWindowMaterialGlazing> glazings) { warnings++; return None; }
        string id = ContentId("win", set);
        foreach (var g in glazings) { store.AddMaterial(g); }
        store.AddConstruction(new Hb.WindowConstructionAbridged(id, [.. glazings.Map(static g => g.Identifier)]));
        return Some(id);
    }

    static string ContentId(string prefix, MaterialComposition.LayerSet set) =>
        $"{prefix}-{ContentHash.Of(set.Layers.Fold(new CanonicalWriter(0.0),
            static (w, layer) => w.String(layer.Material.ToString()).Double(layer.Thickness.Si)).ToBytes().Span):x32}";

    // Holes ride the seam carrier whole: a courtyard slab lowers as boundary plus hole loops, so honeybee's own
    // face algebra subtracts the court instead of conditioning it.
    static Hb.Face3D Face3D(FootprintPolygon ring) =>
        new([.. ring.Ring.Map(static p => (List<double>)[p.X, p.Y, p.Z])],
            holes: ring.Holes.IsEmpty ? null : [.. ring.Holes.Map(static hole => (List<List<double>>)[.. hole.Map(static p => (List<double>)[p.X, p.Y, p.Z])])]);

    // Boundary-condition derivation reads the edge payload the raise stamped through the closed BoundaryRow
    // vocabulary's own generated keyed lookup, so an unrecognized token resolves to the NAMED Outdoors default
    // rather than falling out of a `_` arm. Surface adjacency stays reachable the moment the seam payload carries
    // its counterpart-face ids — that row already exists, only the raise's stamp is pending.
    static BoundaryRow Condition(Relationship.Generic edge) =>
        edge.Attributes.Find(EnergyProjector.BoundaryCondition)
            .Bind(static v => v is PropertyValue.Text t ? Some(t.Value) : None)
            .Bind(static text => BoundaryRow.TryGet(text, out BoundaryRow? row) && row is { } found ? Some(found) : None)
            .IfNone(BoundaryRow.Outdoors);

    // ONE envelope derivation both arms read: each bounding surface's face type, boundary-condition row, footprint
    // ring, and attributed opening occurrences, gathered once per space. A footprint-less or class-unmapped surface
    // is warning-counted and dropped exactly as before — never a zero-area fabrication — and both arms count one
    // set of drops because they read one fold.
    static Seq<EnvelopeFace> Envelope(ElementGraph graph, Node.Object space, GeometrySource geometry, ref int warnings) {
        var faces = Seq<EnvelopeFace>();
        foreach ((Relationship.Generic edge, Node.Object surface) in Boundaries(graph, space.Id)) {
            if (geometry.Footprint(surface.Representations).Case is not FootprintPolygon ring) { warnings++; continue; }
            if (!EnergyClassRows.ToFace.TryGetValue((surface.Classification.Code, surface.PredefinedType), out var faceType)) {
                warnings++; continue;
            }
            faces = faces.Add(new EnvelopeFace(surface, faceType, Condition(edge), ring,
                OpeningsOf(graph, space.Id, surface.ExternalId.IfNone(surface.Name))));
        }
        return faces;
    }

    // Openings of one face: the space's boundary edges whose Host attribute names the face identifier — a raise
    // correlation idiom read back, never a NodeId join (rooted ids are raise-local). Window and door occurrences
    // ride ONE set because both arms need the same nodes: the honeybee arm splits them by class into sub-faces, the
    // dragonfly arm sums the window rings alone into a glazing ratio.
    static Seq<Node.Object> OpeningsOf(ElementGraph graph, NodeId space, string hostIdentifier) =>
        graph.EdgesAt(space).Choose(e =>
            e is Relationship.Generic g && g.WireName == IfcRelKind.SpaceBoundary.Key && g.Relating == space
                && g.Attributes.Find(EnergyProjector.Host).Exists(v => v is PropertyValue.Text t && t.Value == hostIdentifier)
                ? graph.Find<Node.Object>(g.Related) : None)
            .Filter(static o => o.Classification.Code == IfcClass.Window.Key || o.Classification.Code == IfcClass.Door.Key)
            .ToSeq();

    static Seq<Node.Object> SpacesUnder(ElementGraph graph, EnergyScope scope) =>
        graph.ObjectNodes.Filter(o => o.Classification.Code == IfcClass.Space.Key)
            .Filter(o => scope.Switch(
                wholeModel: static _ => true,
                spaces:     s => o.ExternalId.Exists(s.GlobalIds.Contains)))
            .ToSeq();

    // Host-attributed boundary edges are OPENING boundaries (the raise's correlation idiom) — excluded here so
    // a window never folds as an opaque face; the Openings read consumes them.
    static Seq<(Relationship.Generic Edge, Node.Object Surface)> Boundaries(ElementGraph graph, NodeId space) =>
        graph.EdgesAt(space).Choose(e =>
            e is Relationship.Generic g && g.WireName == IfcRelKind.SpaceBoundary.Key && g.Relating == space
                && g.Attributes.Find(EnergyProjector.Host).IsNone
                ? graph.Find<Node.Object>(g.Related).Map(s => (g, s))
                : None).ToSeq();

    // Honeybee sub-face lowering over the SHARED opening set: one pass yields both lists (IfcWindow -> Aperture,
    // IfcDoor -> Door) and each opening's own composition lowers through the SAME discriminated fold so a raised
    // window construction round-trips onto the aperture's abridged reference.
    static (List<Hb.Aperture> Apertures, List<Hb.Door> Doors) Openings(
        ElementGraph graph, EnvelopeFace bound, GeometrySource geometry, Hb.ModelEnergyProperties store,
        ref int openings, ref int warnings, ref int constructions) {
        var apertures = new List<Hb.Aperture>();
        var doors = new List<Hb.Door>();
        foreach (Node.Object opening in bound.Openings) {
            // A footprint-less opening is warning-counted exactly as a footprint-less surface — a silent skip
            // under-glazed the emitted model with zero receipt evidence, the deleted asymmetry.
            if (geometry.Footprint(opening.Representations).Case is not FootprintPolygon ring) { warnings++; continue; }
            Option<string> construction = LowerComposition(graph, opening.Id, store, ref warnings);
            if (construction.IsSome) { constructions++; }
            if (opening.Classification.Code == IfcClass.Window.Key) {
                apertures.Add(new Hb.Aperture(opening.ExternalId.IfNone(opening.Name), Face3D(ring),
                    new Hb.Outdoors(), new Hb.AperturePropertiesAbridged(
                        energy: construction.Match(
                            Some: static id => new Hb.ApertureEnergyPropertiesAbridged(construction: id),
                            None: static () => (Hb.ApertureEnergyPropertiesAbridged?)null))));
            }
            else if (opening.Classification.Code == IfcClass.Door.Key) {
                doors.Add(new Hb.Door(opening.ExternalId.IfNone(opening.Name), Face3D(ring),
                    new Hb.Outdoors(), new Hb.DoorPropertiesAbridged(
                        energy: construction.Match(
                            Some: static id => new Hb.DoorEnergyPropertiesAbridged(construction: id),
                            None: static () => (Hb.DoorEnergyPropertiesAbridged?)null))));
            }
            openings++;
        }
        return (apertures, doors);
    }

    // Dragonfly massing: the OWNING Compose tree lowers Building/Story shells and each space's footprint plate
    // flattens onto a Room2D floor boundary — massing altitude only, no energy library, the honeybee shape
    // inverted; storey multiplier evidence reads back onto Story(multiplier:), so a unique-stories-x-repeat tower
    // round-trips its repeat factor.
    // Every plate reads the SHARED envelope: boundaryConditions and windowParameters index the floor-boundary
    // SEGMENTS one for one, isGroundContact reads a Ground-conditioned floor face and isTopExposed an
    // Outdoors-conditioned roof face. Filling five of nineteen columns emitted a sealed opaque box — no glazing, no
    // soil contact, no sky — whose every solar gain, daylight autonomy, and glazing-ratio result was wrong by the
    // whole envelope, and the omission was invisible because each widened column is schema-OPTIONAL and Validate
    // passes. The graph the honeybee arm reads to full envelope depth is the same graph this arm reads.
    static Fin<EnergyOutcome.Emitted> Dragonfly(ElementGraph graph, EnergyScope scope, GeometrySource geometry, Instant at, Op key) {
        ContentAddress pedigree = ContentAddress.OfGraph(graph);   // the content-stable identifier + Graph pedigree, one derivation
        var buildings = Seq<Df.Building>();
        var massed = Seq<NodeId>();
        int spaces = 0, warnings = 0;
        foreach (Node.Object building in graph.ObjectNodes.Filter(o => o.Classification.Code == IfcClass.Building.Key)) {
            var stories = Seq<Df.Story>();
            foreach (Node.Object storey in Parts(graph, building.Id, IfcClass.BuildingStorey)) {
                var plates = Seq<Df.Room2D>();
                foreach (Node.Object space in Parts(graph, storey.Id, IfcClass.Space).Filter(s => InScope(s, scope))) {
                    if (geometry.Footprint(space.Representations).Case is not FootprintPolygon ring) { warnings++; continue; }
                    Seq<Vector3> plate = Open(ring.Ring);
                    Seq<EnvelopeFace> envelope = Envelope(graph, space, geometry, ref warnings);
                    Seq<(Vector3 From, Vector3 To)> walls = Segments(plate);
                    double height = Height(graph, space.Id).IfNone(DefaultFloorToCeiling);
                    spaces++;
                    plates = plates.Add(new Df.Room2D(
                        space.ExternalId.IfNone(space.Name),
                        [.. plate.Map(static p => (List<double>)[p.X, p.Y])],
                        plate.Head.Map(static p => p.Z).IfNone(0.0),
                        height,
                        new Df.Room2DPropertiesAbridged(),
                        isGroundContact: envelope.Exists(static f => f.Face == Hb.FaceType.Floor && f.Condition == BoundaryRow.Ground),
                        isTopExposed: envelope.Exists(static f => f.Face == Hb.FaceType.RoofCeiling && f.Condition == BoundaryRow.Outdoors),
                        boundaryConditions: [.. walls.Map(wall => Aligned(envelope, wall)
                            .Map(static face => face.Condition).IfNone(BoundaryRow.Outdoors).Plate())],
                        windowParameters: [.. walls.Map(wall => Glazing(envelope, wall, height, geometry))],
                        // Hole plates lower each interior ring as one further coordinate list, so a courtyard,
                        // atrium, or lightwell subtracts from conditioned floor area instead of massing solid.
                        floorHoles: ring.Holes.IsEmpty ? null : [.. ring.Holes.Map(hole =>
                            (List<List<double>>)[.. Open(hole).Map(static p => (List<double>)[p.X, p.Y])])]));
                }
                if (!plates.IsEmpty) {
                    stories = stories.Add(new Df.Story(storey.ExternalId.IfNone(storey.Name), [.. plates],
                        new Df.StoryPropertiesAbridged(), multiplier: Multiplier(graph, storey.Id)));
                }
            }
            if (!stories.IsEmpty) {
                massed = massed.Add(building.Id);
                buildings = buildings.Add(new Df.Building(building.ExternalId.IfNone(building.Name),
                    new Df.BuildingPropertiesAbridged(), uniqueStories: [.. stories]));
            }
        }
        var model = new Df.Model($"rasm-massing-{pedigree.Value:x32}", new Df.ModelProperties(),
            buildings: [.. buildings], units: Df.Units.Meters, tolerance: graph.Header.Tolerance,
            // Site context is the DOMINANT shading term in any urban setting, and the geospatial owner already
            // landed the neighbours on the SAME graph this fold walks — so an emitted massing model without them
            // computed solar gain as if the site were empty desert. ReferenceVector is how dragonfly relocates a
            // local model onto the earth for solar position; without it every sun path is computed at the model
            // origin, so a georeferenced graph emitted a model that could not know where it stood.
            contextShades: [.. Context(graph, geometry, massed)],
            referenceVector: [graph.Header.Reference.Eastings, graph.Header.Reference.Northings, graph.Header.Reference.OrthogonalHeight]);
        warnings += model.Validate().Count();
        return Fin.Succ(Emit(InterchangeFormat.Dfjson, Encoding.UTF8.GetBytes(model.ToJson()), pedigree, at,
            new EnergyReceipt(EnergyLeg.Lowered, InterchangeFormat.Dfjson, None, spaces, 0, 0, 0, warnings, default, at)));
    }

    // Site context is every footprint-bearing geographic element the geospatial projector landed plus every
    // IfcBuilding this fold did NOT mass — a neighbour block ingested as a site-context occurrence carries no
    // storey decomposition, so "contributed no stories" is exactly the discriminant that separates the model's own
    // buildings from the ones surrounding it, read off the massed set rather than re-walked.
    static Seq<Df.ContextShade> Context(ElementGraph graph, GeometrySource geometry, Seq<NodeId> massed) =>
        graph.ObjectNodes
            .Filter(o => o.Classification.Code == GeographicElementClass
                || (o.Classification.Code == IfcClass.Building.Key && !massed.Contains(o.Id)))
            .Choose(o => geometry.Footprint(o.Representations).Map(ring =>
                new Df.ContextShade(o.ExternalId.IfNone(o.Name),
                    [(ShadeGeometry)Face3D(ring)], new Df.ContextShadePropertiesAbridged())))
            .ToSeq();

    // Geospatial projection carries the TRUE IFC4.3 entity-type string on the seam Classification rather than an
    // IfcClass row, so this site-context read matches that same code — resolving it through IfcClass.TryGet
    // collapses it to the Proxy fallback and misses every neighbour.
    const string GeographicElementClass = "IfcGeographicElement";

    // Dragonfly's FloorBoundary is an OPEN ring — the schema closes it implicitly — so a source ring carrying its
    // repeated first vertex emits a zero-length final wall and shifts every parameter list by one against the
    // segments it indexes. Open drops that duplicate once; a ring that never carried one passes through.
    static Seq<Vector3> Open(Seq<Vector3> ring) =>
        ring.Count > 1 && ring.Head.Exists(head => head == ring.Last) ? ring.Take(ring.Count - 1).ToSeq() : ring;

    // Each floor-boundary SEGMENT is one Room2D wall, so every per-wall parameter list indexes by segment and the
    // ring closes back to its first vertex. A ring under three vertices bounds no plate and yields no segments, so
    // every parameter list stays empty rather than misaligned against a boundary that is not there.
    static Seq<(Vector3 From, Vector3 To)> Segments(Seq<Vector3> ring) =>
        ring.Count < 3
            ? Seq<(Vector3, Vector3)>()
            : ring.Select((point, index) => (From: point, To: ring[(index + 1) % ring.Count])).ToSeq();

    // Aligned joins a Room2D wall segment to the space's vertical bounding SURFACE that is the same fact in the
    // other schema: the wall face whose ring centre lies nearest the segment midpoint IN PLAN, accepted only within
    // half the segment length so a surface across the room never claims a segment it does not bound. Floor and
    // ceiling faces are excluded because they bound the plate itself, not any of its walls.
    static Option<EnvelopeFace> Aligned(Seq<EnvelopeFace> envelope, (Vector3 From, Vector3 To) wall) {
        Vector3 mid = (wall.From + wall.To) * 0.5;
        double reach = Vector3.Distance(wall.From, wall.To) * 0.5;
        return envelope
            .Filter(static f => f.Face == Hb.FaceType.Wall)
            .Fold(Option<(EnvelopeFace Face, double Gap)>.None, (best, face) =>
                PlanDistance(mid, Centre(face.Ring.Ring)) is var gap && gap <= reach
                && best.Map(held => gap < held.Gap).IfNone(true)
                    ? Some((face, gap))
                    : best)
            .Map(static row => row.Face);
    }

    // Glazing answers one wall segment's window parameter: aperture area over wall area, where the wall is that
    // segment EXTRUDED to the floor-to-ceiling height — Room2D's own massing model, so the denominator needs no
    // area owner at all — and the numerator is the seam owner's FootprintPolygon.Area Newell fold over the window
    // rings the raise attributed to the matched face (exact for a VERTICAL ring the planar NTS algebra reads as
    // near-zero). Doors are excluded: a door is not glazing and folding it into the ratio over-states solar gain
    // on every entrance wall. Segments with no matched face, a degenerate wall, or zero aperture area yield NULL,
    // because dragonfly's own ABSENT slot is how "no glazing" is spelled — a SimpleWindowRatio of 0 reads
    // downstream as a real zero-area window the solver still meshes and reports. Clamping the quotient below one
    // keeps a mis-measured ring from emitting a wall more than fully glazed.
    static PlateWindow? Glazing(Seq<EnvelopeFace> envelope, (Vector3 From, Vector3 To) wall, double height, GeometrySource geometry) {
        double area = Vector3.Distance(wall.From, wall.To) * height;
        return Aligned(envelope, wall).Match(
            Some: face => area > 0.0
                && face.Openings
                    .Filter(static o => o.Classification.Code == IfcClass.Window.Key)
                    .Fold(0.0, (sum, window) => sum + geometry.Footprint(window.Representations)
                        .Map(static ring => ring.Area).IfNone(0.0)) is var glazed && glazed > 0.0
                ? (PlateWindow)new Df.SimpleWindowRatio(Math.Min(glazed / area, 1.0))
                : null,
            None: () => null);
    }

    static Vector3 Centre(Seq<Vector3> ring) =>
        ring.IsEmpty ? Vector3.Zero : ring.Fold(Vector3.Zero, static (sum, point) => sum + point) * (1.0 / ring.Count);

    // Plan distance alone: a wall surface's ring centre sits at mid-height while its floor-boundary
    // segment lies on the slab, so a 3D distance rejects every real match by half the storey height.
    static double PlanDistance(Vector3 a, Vector3 b) =>
        Math.Sqrt(((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y)));

    // Massing fallback when a space carries no Qto height — a named policy default (the Compute density-1000
    // precedent), never a silent zero; a real height reads the Qto_SpaceBaseQuantities bag the IFC ingest OR the
    // dragonfly raise landed.
    const double DefaultFloorToCeiling = 3.0;

    static Option<double> Height(ElementGraph graph, NodeId space) =>
        graph.EdgesAt(space).Choose(e =>
            e is Relationship.Assign { SubKind: var k } a && k == AssignKind.PropertyDefinition && a.Subject == space
                ? graph.Find<Node.QuantitySet>(a.Definition) : None)
            .Filter(static qs => qs.Bag.SetName == "Qto_SpaceBaseQuantities").Head
            .Bind(static qs => qs.Bag.Values.Find(PropertyName.Create("Height")))
            .Bind(static m => m.Length);

    // Raise StoryMultiplier evidence read back through the projector-owned symbol (never a re-spelled literal);
    // absent evidence is multiplier 1 — the dragonfly schema default.
    static int Multiplier(ElementGraph graph, NodeId storey) =>
        graph.EdgesAt(storey).Choose(e =>
            e is Relationship.Assign { SubKind: var k } a && k == AssignKind.PropertyDefinition && a.Subject == storey
                ? graph.Find<Node.PropertySet>(a.Definition) : None)
            .Filter(static ps => ps.Bag.SetName == "Pset_EnergyModel").Head
            .Bind(static ps => ps.Bag.Values.Find(EnergyProjector.StoryMultiplier))
            .Bind(static v => v is PropertyValue.Measure m ? Some((int)m.Value.Si) : None)
            .IfNone(1);

    // Transitive OWNING decomposition step (aggregate/nest/contain, never Reference), class-filtered — the
    // same descent law the Compute spatial reads state.
    static Seq<Node.Object> Parts(ElementGraph graph, NodeId whole, IfcClass @class) =>
        graph.EdgesAt(whole).Choose(e =>
            e is Relationship.Compose c && c.Whole == whole && c.SubKind != ComposeKind.Reference
                ? graph.Find<Node.Object>(c.Part).Filter(o => o.Classification.Code == @class.Key)
                : None).ToSeq();

    static bool InScope(Node.Object space, EnergyScope scope) => scope.Switch(
        wholeModel: static _ => true,
        spaces:     s => space.ExternalId.Exists(s.GlobalIds.Contains));

    static EnergyOutcome.Emitted Emit(InterchangeFormat format, byte[] bytes, ContentAddress graph, Instant at, EnergyReceipt receipt) {
        EnergyArtifact artifact = EnergyArtifact.Of(format, bytes, Some(graph), at);
        return new EnergyOutcome.Emitted(artifact, receipt with { Key = artifact.ContentKey });
    }
}
```

## [03]-[TRANSLATE_MATRIX]

- Owner: `EnergyTranslate` the OSM-centric translator matrix — one frozen `(source, target)` row table over the OpenStudio translators, never a per-pair method family.
- Entry: `EnergyTranslate.Run(EnergyDoc source, InterchangeFormat target, Instant at, Op key, Option<BimHooks> hooks = default)` → `Fin<EnergyOutcome.Emitted>` resolves the `(source, target)` matrix row — `osm→gbxml` (`GbXMLForwardTranslator.modelToGbXMLString`), `osm→idf` (`EnergyPlusForwardTranslator.translateModel` + `Workspace.save`), `gbxml→osm`/`idf→osm` (the reverse readers + `Model.save`), `osm→osm` (the `VersionTranslator` version-upgrade row) — and emits the translated bytes as an `EnergyArtifact` (no graph pedigree — a translation never touched the graph) with the translator `warnings()`/`errors()` tallied into the `Translated` receipt; a hook-bearing composition threads one `ProgressBar` director subclass per run onto the verified translator overloads (`loadModelFromString(string, ProgressBar)`, `translateModel(Model, ProgressBar)`, `modelToGbXMLString(Model, ProgressBar)`, `loadModel(Path, ProgressBar)`), its `onPercentageUpdated(double)` override firing the `Model/observability#HOOK_RAIL` `rasm.bim.energy.progress` observe point, so a long translation surfaces percentage facts with zero translator coupling.
- Packages: NREL.OpenStudio.macOS-arm64, Rasm, LanguageExt.Core, NodaTime
- Growth: a new translation is one `Matrix` row over a verified translator member (SDD via `SddForwardTranslator`/`SddReverseTranslator` is the named next row); the graph→OSM/gbXML egress lands as ONE matrix column fed by the lowered honeybee leg the moment an in-process HBJSON→OSM translation is admitted.
- Boundary: the translate temp-path crossings and the SWIG handle brackets are the named platform-forced statement seam; `Workspace.save`/`Model.save` path-bound emits cross a bracketed scratch file exactly as the decode arms do; a matrix miss rails `CodecReject` (`energy-translate-miss`), an unreadable source `ModelRejected` (`energy-decode`).

```csharp signature
// Shares the [02] RUNTIME_PRELUDE (the Os alias and the seam usings — one compilation unit per page).
// OSM-centric translator matrix: (source, target) rows over verified OpenStudio members; a translation is one
// row, never a per-pair method family. Path-bound emits cross a bracketed scratch file.
public static class EnergyTranslate {
    static readonly FrozenDictionary<(InterchangeFormat Source, InterchangeFormat Target), Func<EnergyDoc, Op, Os.ProgressBar?, Fin<(byte[] Bytes, int Warnings)>>> Matrix =
        new KeyValuePair<(InterchangeFormat, InterchangeFormat), Func<EnergyDoc, Op, Os.ProgressBar?, Fin<(byte[], int)>>>[] {
            new((InterchangeFormat.Osm,   InterchangeFormat.GbXml), static (doc, key, progress) => OsmTo(doc, key, progress, static (model, tally, bar) => {
                using Os.GbXMLForwardTranslator gb = new();
                string xml = bar is null ? gb.modelToGbXMLString(model) : gb.modelToGbXMLString(model, bar);
                return (Encoding.UTF8.GetBytes(xml), tally + Tally(gb.warnings(), gb.errors()));
            })),
            new((InterchangeFormat.Osm,   InterchangeFormat.Idf),   static (doc, key, progress) => OsmTo(doc, key, progress, static (model, tally, bar) => {
                using Os.EnergyPlusForwardTranslator ep = new();
                using Os.Workspace idf = bar is null ? ep.translateModel(model) : ep.translateModel(model, bar);
                return (Saved(w => idf.save(w, true)), tally + Tally(ep.warnings(), ep.errors()));
            })),
            new((InterchangeFormat.Osm,   InterchangeFormat.Osm),   static (doc, key, progress) => OsmTo(doc, key, progress, static (model, tally, _) =>
                (Saved(w => model.save(w, true)), tally))),   // the VersionTranslator upgrade row: decode already upgraded
            new((InterchangeFormat.GbXml, InterchangeFormat.Osm),   static (doc, key, progress) => ReverseTo(doc, key, progress)),
            new((InterchangeFormat.Idf,   InterchangeFormat.Osm),   static (doc, key, progress) => ReverseTo(doc, key, progress)),
        }.ToFrozenDictionary();

    internal static Fin<EnergyOutcome.Emitted> Run(EnergyDoc source, InterchangeFormat target, Instant at, Op key, Option<BimHooks> hooks = default) {
        if (!Matrix.TryGetValue((source.Format, target), out var row)) {
            return Fin.Fail<EnergyOutcome.Emitted>(new BimFault.CodecReject(key, $"energy-translate-miss:{source.Format.Key}->{target.Key}"));
        }
        // One director bar per run, bracketed with the SWIG seam; a hook-less run passes null and every
        // translator call takes its ProgressBar-free overload.
        using TranslateProgress? progress = hooks.Case is BimHooks h ? new TranslateProgress(h, key) : null;
        return row(source, key, progress).Map(result => {
            EnergyArtifact artifact = EnergyArtifact.Of(target, result.Bytes, None, at);
            return new EnergyOutcome.Emitted(artifact, new EnergyReceipt(
                EnergyLeg.Translated, source.Format, Some(target), 0, 0, 0, 0, result.Warnings, artifact.ContentKey, at));
        });
    }

    // SWIG director subclass: OpenStudio calls the virtual onPercentageUpdated across the native boundary, and the
    // override fires the Model/observability#HOOK_RAIL rasm.bim.energy.progress observe point — a 0..100
    // percentage normalized onto the [0,1] Fraction the Progress fact carries.
    sealed class TranslateProgress(BimHooks hooks, Op key) : Os.ProgressBar {
        public override void onPercentageUpdated(double percentage) =>
            ignore(hooks.EnergyProgress.Fire(new BimFact.Progress(key, "energy", "translate", Some(percentage / 100.0))));
    }

    // Decode-then-emit over the version-upgrading in-string read; the emit lambda owns its translator brackets
    // and threads the optional director bar onto the verified ProgressBar overloads.
    static Fin<(byte[], int)> OsmTo(EnergyDoc doc, Op key, Os.ProgressBar? progress, Func<Os.Model, int, Os.ProgressBar?, (byte[], int)> emit) {
        try {
            using Os.VersionTranslator vt = new();
            using Os.OptionalModel optional = progress is null ? vt.loadModelFromString(doc.Text) : vt.loadModelFromString(doc.Text, progress);
            if (!optional.is_initialized()) {
                return Fin.Fail<(byte[], int)>(new BimFault.ModelRejected(key, "energy-decode:<osm-unreadable>"));
            }
            using Os.Model model = optional.get();
            return Fin.Succ(emit(model, Tally(vt.warnings(), vt.errors()), progress));
        }
        catch (Exception ex) when (ex is SystemException or ApplicationException) {
            return Fin.Fail<(byte[], int)>(new BimFault.ModelRejected(key, $"energy-translate:{ex.Message}"));
        }
    }

    // gbXML/IDF -> OSM: the Path-bound reverse readers over a bracketed temp file, saved back as .osm bytes.
    static Fin<(byte[], int)> ReverseTo(EnergyDoc doc, Op key, Os.ProgressBar? progress) {
        string temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try {
            File.WriteAllBytes(temp, doc.Bytes.ToArray());
            using Os.Path path = Os.OpenStudioUtilitiesCore.toPath(temp);
            if (doc.Format == InterchangeFormat.GbXml) {
                using Os.GbXMLReverseTranslator gb = new();
                using Os.OptionalModel fromGb = progress is null ? gb.loadModel(path) : gb.loadModel(path, progress);
                return fromGb.is_initialized()
                    ? Save(fromGb.get(), Tally(gb.warnings(), gb.errors()))
                    : Fin.Fail<(byte[], int)>(new BimFault.ModelRejected(key, "energy-decode:<gbxml-unreadable>"));
            }
            using Os.EnergyPlusReverseTranslator ep = new();
            using Os.OptionalModel fromIdf = progress is null ? ep.loadModel(path) : ep.loadModel(path, progress);
            return fromIdf.is_initialized()
                ? Save(fromIdf.get(), Tally(ep.warnings(), ep.errors()))
                : Fin.Fail<(byte[], int)>(new BimFault.ModelRejected(key, "energy-decode:<idf-unreadable>"));
        }
        catch (Exception ex) when (ex is SystemException or ApplicationException) {
            return Fin.Fail<(byte[], int)>(new BimFault.ModelRejected(key, $"energy-translate:{ex.Message}"));
        }
        finally { File.Delete(temp); }
    }

    static Fin<(byte[], int)> Save(Os.Model model, int warnings) { using (model) { return Fin.Succ((Saved(w => model.save(w, true)), warnings)); } }

    // Path-bound emit crossed via a bracketed scratch path (Exemption: SWIG + filesystem boundary).
    static byte[] Saved(Action<Os.Path> save) {
        string temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try { using Os.Path path = Os.OpenStudioUtilitiesCore.toPath(temp); save(path); return File.ReadAllBytes(temp); }
        finally { File.Delete(temp); }
    }

    static int Tally(Os.LogMessageVector warnings, Os.LogMessageVector errors) {
        using (warnings) using (errors) { return warnings.Count + errors.Count; }
    }
}
```

## [04]-[RESEARCH]

- [HBJSON_WIRE_PARITY]-[OPEN]: does the `HoneybeeSchema` `Model.ToJson()` render this lower content-keys feed the same octets the python `libs/python/geometry` energy model keys — `honeybee.Model.to_dict(included_prop=("energy",))` under `msgspec` deterministic encoding — or do the two serializers diverge in key order, float spelling, or omitted-default handling; serialize ONE document through both ends and byte-compare the two renders, then pin one byte source on both pages or seat a canonical re-encode at the crossing.
- [SEAM_ALIGNMENT]-[OPEN]: do the Compute-side member spellings `BuildConstruction`, `StandardGlazing`, and the density-1000 fallback row still match the lower's seam reads; verify against `csharp:Rasm.Compute` `Runtime` energy-build pages when the Compute OSM build page next rebuilds.
