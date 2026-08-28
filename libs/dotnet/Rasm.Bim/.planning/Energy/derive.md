# [BIM_ENERGY_DERIVE]

`EnergyDerive` lowers the shared `ElementGraph` to two authoring schemas, and `EnergyTranslate` runs the OpenStudio translator matrix as one frozen `(source, target)` row table. `IfcSpace` nodes — landed by the `Projection/semantic#SEMANTIC_PROJECTOR` IFC ingest or any projector — fold to honeybee `Room`/`Face`/`Aperture`/`Door` building envelopes with their layered shared composition lowered onto the energy library under the abridged-reference law, or to dragonfly `Story`/`Room2D` massing plates. `Energy/exchange#ENERGY_EXCHANGE`'s `EnergyExchange.Apply` drives both arms and emits content-keyed `EnergyArtifact`s; class correspondence, the energy-owned boundary payload statics, and evidence bags read back from the `Energy/projector#ENERGY_PROJECTOR` owner — the space-boundary `Host`/`Level` rows composing the contract-declared `BoundaryRows` statics at both ends — so the raise and the lower cannot drift.

Wire posture is HOST-LOCAL, foreign types emit-confined: each lowered `Hb.Model`/`Df.Model` is authored, `Validate()`-noted, serialized through `ToJson()`, and released inside the arm; every `OpenStudio.*` SWIG wrapper is `using`-bracketed, and the translate temp-path crossings (`Workspace.save`/`Model.save` over a bracketed scratch file) are the platform-forced statement boundary. Terminal faults are `Refused` over the closed Energy scope/reason axes, OpenStudio captures are `BoundaryFailed` retaining the original `Error`, and abandonment stays the LanguageExt `Errors.Cancelled`.

## [01]-[INDEX]

- [02]-[MODEL_DERIVE]: `EnergyDerive` lowers the graph onto the honeybee building envelope + energy library (opaque AND glazing constructions, opening sub-faces with their own constructions) over the shared `GeometrySource` port, and onto dragonfly massing with the height/multiplier evidence read back.
- [03]-[TRANSLATE_MATRIX]: the `EnergyTranslate` OSM-centric translator matrix (osm↔gbxml, osm↔idf, osm version-upgrade) as frozen row data over the OpenStudio translators.

## [02]-[MODEL_DERIVE]

- Owner: `EnergyDerive` the BIM-to-BEM lower fold (graph → honeybee HBJSON building envelope + energy library, graph → dragonfly DFJSON massing); `BoundaryRow` the closed boundary-condition vocabulary carrying one honeybee closure, its derived dragonfly projection, and the ONE `Admit` both the raise and the lower resolve a condition token through; `EnvelopeFace` the segment-aligned building-envelope row both arms read; `MaterialArm` the per-construction-family policy value one set lower is parameterized by; `LowerLog` the immutable accumulation threaded through every fold.
- Entry: `EnergyDerive.Lower(ElementGraph graph, InterchangeFormat target, EnergyScope scope, GeometrySource geometry, Instant at)` → `Fin<EnergyOutcome.Emitted>` — dispatches the frozen `Lowers` target table: the `hbjson` arm lowers each scoped `IfcSpace` and its opening sub-faces onto the honeybee building envelope + energy library, the `dfjson` arm folds the `Compose` tree onto dragonfly massing plates whose per-segment boundary conditions, window ratios, ground contact, and sky exposure read that same building envelope, with the site's un-massed neighbours lowered onto `ContextShade` and the shared georeference onto `ReferenceVector`; each surface and opening composition lowers through ONE property-case fold.
- Auto: lowered models carry the SEMANTIC building envelope and library only; simulation context — parameters, run period, conditioning, weather — is Compute's or the python recipe plane's, never authored on the lower. `Envelope` derives each space's bounding surfaces ONCE — face type, boundary row, footprint ring, attributed openings — so the two arms cannot drift about which wall carries which condition or which window, and the dragonfly arm joins those surfaces to floor-boundary SEGMENTS in plan within half a segment length so the parameter lists index the walls they describe.
- Output: `LowerLog` threads the spaces/surfaces/openings/constructions tallies and the typed `Energy/exchange#ENERGY_EXCHANGE` `EnergyNote` degrade rows through every fold, and one `EnergyCensus` per emit projects it — the model's `Validate()` DataAnnotations results fold onto the SAME rows so the warning tally is one fold over one row family, never an exception and never a second counter. Every degrade names the node it dropped against, so a reader reaches the surface rather than a number.
- Packages: HoneybeeSchema, DragonflySchema, Rasm.Element, Rasm, LanguageExt.Core, NodaTime
- Growth: a new lower target is one row on the frozen `Lowers` target table (the `EnergyProjector.Arms`/`EnergyTranslate.Matrix` row law); a new boundary condition is one `BoundaryRow` row both schemas project from; a richer glazing posture is one `PlateWindow` case swapped at the `Glazing` return with the same measured quotient behind it; per-space program/loads lower as `ProgramTypeAbridged` rows once the contract carries occupancy evidence; a NoMass R-value lower is one `MaterialArm` row the moment the contract carries an R-value-only thermal case; the space-adjacency `Surface` boundary condition needs no new row — `BoundaryRow.Surface` already projects into both schemas and waits only on the raise stamping the counterpart-face payload.
- Boundary: `EnergyDerive` reads the graph through contract-owned surfaces and the `Model/query#ELEMENT_SET` scope algebra; Compute is a peer stratum, never a dependency, so its discipline reads (`SpacesOf`/`BoundingSurfacesOf`) are never referenced. Building-envelope derivation is SHARED and single — a per-arm boundary walk is the deleted form that let two emitted documents disagree about one building — and the ring area both arms need is the contract owner's `FootprintPolygon.Area` — the Newell fold, shell minus holes, seated where the rings live — because a vertical aperture ring projects to near-zero area under the planar NTS algebra the geospatial owner holds and a page-local ring-area helper re-derives arithmetic the carrier owns. Every missing- or ambiguous-evidence path degrades onto a typed `EnergyReason` row naming its subject — a footprint-less space, a material lacking both the `Thermal` and `Optical` case, a wall segment no bounding surface matched — never a zero-area fabrication or a fabricated physics row; a zero-area glazing sum emits dragonfly's ABSENT window slot and never a `SimpleWindowRatio` of `0`, which a solver reads as a real zero-area window rather than as no window. `graph→OSM`/`gbXML`/`IDF` DIRECT egress is a PROVED negative, not a pending arm: the binding's translator matrix admits OSM-family sources alone — its reverse roster is gbXML/IDF/SDD/three.js/floorplan and no HBJSON ingest exists on it, so no matrix column feeds from the lowered honeybee document — HBJSON→OSM is the python peer's `honeybee-openstudio` wire leg, and a second graph→OSM builder beside Compute's simulation-scoped `BuildModel` is the duplicate-fold defect, so the request returns `BimFault.Refused` with `BimReason.Capability` and the egress composes as `Lower` HBJSON → peer wire → `Translate`. Glazing lowering consumes the same shared `Optical` case (`Discipline.Energy`) Compute's `StandardGlazing` build reads, so the lowered honeybee document and Compute's OSM model agree on layer physics by construction.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
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
using Thinktecture;
using static LanguageExt.Prelude;
using Df = DragonflySchema;
using Hb = HoneybeeSchema;
using Os = OpenStudio;
using FaceBoundary = HoneybeeSchema.AnyOf<HoneybeeSchema.Ground, HoneybeeSchema.Outdoors, HoneybeeSchema.Adiabatic, HoneybeeSchema.Surface, HoneybeeSchema.OtherSideTemperature>;
using PlateBoundary = HoneybeeSchema.AnyOf<HoneybeeSchema.Ground, HoneybeeSchema.Outdoors, HoneybeeSchema.Surface, HoneybeeSchema.Adiabatic, HoneybeeSchema.OtherSideTemperature>;
using PlateWindow = HoneybeeSchema.AnyOf<DragonflySchema.SingleWindow, DragonflySchema.SimpleWindowArea, DragonflySchema.SimpleWindowRatio, DragonflySchema.RepeatingWindowRatio, DragonflySchema.RectangularWindows, DragonflySchema.DetailedWindows>;
using ShadeGeometry = HoneybeeSchema.AnyOf<HoneybeeSchema.Face3D, HoneybeeSchema.Mesh3D>;
using BimHooks = Rasm.Domain.HookSet<Rasm.Bim.Model.BimPoint, Rasm.Bim.Model.BimFact, Rasm.Domain.TelemetrySource>;

namespace Rasm.Bim;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
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

    public static BoundaryRow Admit(string? token) =>
        token is not null && TryGet(token, out BoundaryRow? row) && row is { } found ? found : Outdoors;
}

sealed record MaterialArm(
    string Prefix,
    Func<Hb.ModelEnergyProperties, MaterialLayer, Node.Material, Option<string>> Layer,
    Func<Hb.ModelEnergyProperties, string, Seq<string>, Unit> Construction) {

    const double GlazingConductivity = 0.9;

    public static readonly MaterialArm Opaque = new("con",
        static (store, layer, node) => (node.Properties.Thermal, node.Properties.Mechanical)
            .Apply((thermal, mechanical) => new Hb.EnergyMaterial(
                layer.Material.ToString(), layer.Thickness.Si, thermal.Conductivity.Si,
                mechanical.Density.Si, thermal.SpecificHeat.Si)).As()
            .Map(material => { store.AddMaterial(material); return material.Identifier; }),
        static (store, id, layers) => {
            store.AddConstruction(new Hb.OpaqueConstructionAbridged(id, [.. layers]));
            return unit;
        });

    public static readonly MaterialArm Glazing = new("win",
        static (store, layer, node) => node.Properties.Optical
            .Map(o => new Hb.EnergyWindowMaterialGlazing(
                layer.Material.ToString(), thickness: layer.Thickness.Si,
                solarTransmittance: o.SolarTransmittance, solarReflectance: o.SolarReflectanceFront, solarReflectanceBack: o.SolarReflectanceBack,
                visibleTransmittance: o.VisibleTransmittance, visibleReflectance: o.VisibleReflectanceFront, visibleReflectanceBack: o.VisibleReflectanceBack,
                infraredTransmittance: o.ThermalIrTransmittance, emissivity: o.ThermalIrEmissivityFront, emissivityBack: o.ThermalIrEmissivityBack,
                conductivity: node.Properties.Thermal.Map(static t => t.Conductivity.Si).IfNone(GlazingConductivity)))
            .Map(glazing => { store.AddMaterial(glazing); return glazing.Identifier; }),
        static (store, id, layers) => {
            store.AddConstruction(new Hb.WindowConstructionAbridged(id, [.. layers]));
            return unit;
        });
}

public sealed record LowerLog(int Spaces, int Surfaces, int Openings, int Constructions, Seq<EnergyNote> Notes) {
    public static readonly LowerLog Empty = new(0, 0, 0, 0, Seq<EnergyNote>());

    public LowerLog Land(EnergySlot slot) => slot.Switch(
        space:        () => this with { Spaces = Spaces + 1 },
        surface:      () => this with { Surfaces = Surfaces + 1 },
        opening:      () => this with { Openings = Openings + 1 },
        construction: () => this with { Constructions = Constructions + 1 });

    public LowerLog Noted(Seq<EnergyNote> notes) => this with { Notes = Notes + notes };
    public LowerLog Note(EnergyReason reason, string subject) => Noted(Seq(new EnergyNote(reason, subject, 1)));

    public LowerLog Annotated(IEnumerable<ValidationResult> results) =>
        Noted(toSeq(results).Map(static r => new EnergyNote(EnergyReason.SchemaAnnotation, r.ErrorMessage ?? "", 1)));

    public EnergyCensus Census(InterchangeFormat form, Instant at) =>
        new(EnergyLeg.Lowered, form, None, Spaces, Surfaces, Openings, Constructions, Notes, default, at);
}

readonly record struct EnvelopeFace(
    Node.Object Surface, Hb.FaceType Face, BoundaryRow Condition, FootprintPolygon Ring, Seq<Node.Object> Openings);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EnergyDerive {
    static readonly FrozenDictionary<InterchangeFormat, Func<ElementGraph, EnergyScope, GeometrySource, Instant, Fin<EnergyOutcome.Emitted>>> Lowers =
        new KeyValuePair<InterchangeFormat, Func<ElementGraph, EnergyScope, GeometrySource, Instant, Fin<EnergyOutcome.Emitted>>>[] {
            new(InterchangeFormat.Hbjson, Honeybee),
            new(InterchangeFormat.Dfjson, Dragonfly),
        }.ToFrozenDictionary();

    internal static Fin<EnergyOutcome.Emitted> Lower(
        ElementGraph graph, InterchangeFormat target, EnergyScope scope, GeometrySource geometry, Instant at) =>
        Lowers.TryGetValue(target, out var arm)
            ? arm(graph, scope, geometry, at)
            : EnergyProjector.Serves(target)
                ? Fin.Fail<EnergyOutcome.Emitted>(new BimFault.Refused(BimScope.Energy, BimReason.Capability, string.Join(':', new object?[] { "energy-graph-egress-pending", target.Key })))
                : Fin.Fail<EnergyOutcome.Emitted>(new BimFault.Refused(BimScope.Energy, BimReason.Codec, string.Join(':', new object?[] { "energy-lower-unsupported", target.Key })));

    static Fin<EnergyOutcome.Emitted> Honeybee(ElementGraph graph, EnergyScope scope, GeometrySource geometry, Instant at) {
        ContentAddress pedigree = ContentAddress.OfGraph(graph);
        var store = new Hb.ModelEnergyProperties();
        (Seq<Hb.Room> Rooms, LowerLog Log) built = SpacesUnder(graph, scope).Fold(
            (Rooms: Seq<Hb.Room>(), Log: LowerLog.Empty),
            (state, space) => {
                (Hb.Room room, LowerLog log) = Room(graph, space, geometry, store, state.Log);
                return (state.Rooms.Add(room), log);
            });
        if (built.Rooms.IsEmpty) {
            return Fin.Fail<EnergyOutcome.Emitted>(new BimFault.Refused(BimScope.Energy, BimReason.Capability, string.Join(':', new object?[] { "energy-lower-empty", InterchangeFormat.Hbjson.Key, Scoped(scope) })));
        }
        var model = new Hb.Model($"rasm-energy-{pedigree.Value:x32}", new Hb.ModelProperties(energy: store),
            rooms: [.. built.Rooms], units: Hb.Units.Meters, tolerance: graph.Header.Tolerance);
        return Fin.Succ(Emit(InterchangeFormat.Hbjson, Encoding.UTF8.GetBytes(model.ToJson()), pedigree, at,
            built.Log.Annotated(model.Validate()).Census(InterchangeFormat.Hbjson, at)));
    }

    static (Hb.Room Room, LowerLog Log) Room(
        ElementGraph graph, Node.Object space, GeometrySource geometry, Hb.ModelEnergyProperties store, LowerLog log) {
        (Seq<EnvelopeFace> bounds, Seq<EnergyNote> notes) = Envelope(graph, space, geometry);
        (Seq<Hb.Face> Faces, LowerLog Log) folded = bounds.Fold(
            (Faces: Seq<Hb.Face>(), Log: log.Noted(notes)),
            (state, bound) => {
                (Option<string> construction, LowerLog composed) = Composed(graph, bound.Surface.Id, store, state.Log);
                (Seq<Hb.Aperture> apertures, Seq<Hb.Door> doors, LowerLog opened) =
                    Openings(graph, bound, geometry, store, composed);
                return (state.Faces.Add(new Hb.Face(
                        Identifier(bound.Surface), Face3D(bound.Ring), bound.Face,
                        bound.Condition.Face(), new Hb.FacePropertiesAbridged(
                            energy: HostEdge.Slot(construction.Map(
                                static id => new Hb.FaceEnergyPropertiesAbridged(construction: id)))),
                        apertures: [.. apertures], doors: [.. doors])),
                    opened.Land(EnergySlot.Surface));
            });
        return (new Hb.Room(Identifier(space), [.. folded.Faces], new Hb.RoomPropertiesAbridged()),
                folded.Log.Land(EnergySlot.Space));
    }

    static (Option<string> Id, LowerLog Log) Composed(
        ElementGraph graph, NodeId node, Hb.ModelEnergyProperties store, LowerLog log) {
        (Option<string> id, Seq<EnergyNote> notes) = LowerComposition(graph, node, store);
        LowerLog noted = log.Noted(notes);
        return (id, id.IsSome ? noted.Land(EnergySlot.Construction) : noted);
    }

    static (Option<string> Id, Seq<EnergyNote> Notes) LowerComposition(
        ElementGraph graph, NodeId node, Hb.ModelEnergyProperties store) {
        if (graph.CompositionOf(node).Bind(static c => c is MaterialComposition.LayerSet set ? Some(set) : None).Case
            is not MaterialComposition.LayerSet set) { return (None, Seq<EnergyNote>()); }
        if (set.Layers.Traverse(layer => graph.Material(layer.Material).Map(m => (Layer: layer, Node: m))).As().Case
            is not Seq<(MaterialLayer Layer, Node.Material Node)> rows) {
            return (None, Seq(new EnergyNote(EnergyReason.LayerUnresolved, node.Value, 1)));
        }
        bool anyOptical = rows.Exists(static r => r.Node.Properties.Optical.IsSome);
        if (anyOptical && !rows.ForAll(static r => r.Node.Properties.Optical.IsSome)) {
            return (None, Seq(new EnergyNote(EnergyReason.CompositionMixed, node.Value, 1)));
        }
        return LowerSet(rows, set, store, anyOptical ? MaterialArm.Glazing : MaterialArm.Opaque, node);
    }

    static (Option<string> Id, Seq<EnergyNote> Notes) LowerSet(
        Seq<(MaterialLayer Layer, Node.Material Node)> rows, MaterialComposition.LayerSet set,
        Hb.ModelEnergyProperties store, MaterialArm arm, NodeId node) {
        if (rows.Traverse(row => arm.Layer(store, row.Layer, row.Node)).As().Case is not Seq<string> layers) {
            return (None, Seq(new EnergyNote(EnergyReason.PropertyIncomplete, node.Value, 1)));
        }
        string id = ContentId(arm.Prefix, set);
        ignore(arm.Construction(store, id, layers));
        return (Some(id), Seq<EnergyNote>());
    }

    static string ContentId(string prefix, MaterialComposition.LayerSet set) =>
        $"{prefix}-{ContentAddress.Of(set, 0.0, static (buildup, writer) => buildup.Layers
            .Fold(writer, static (w, layer) => w.String(layer.Material.ToString()).Double(layer.Thickness.Si))).Value:x32}";

    static Hb.Face3D Face3D(FootprintPolygon ring) =>
        new([.. ring.Ring.Map(static p => (List<double>)[p.X, p.Y, p.Z])],
            holes: ring.Holes.IsEmpty ? null : [.. ring.Holes.Map(static hole => (List<List<double>>)[.. hole.Map(static p => (List<double>)[p.X, p.Y, p.Z])])]);

    static BoundaryRow Condition(Relationship.Generic edge) =>
        BoundaryRow.Admit(edge.Attributes.Find(EnergyProjector.BoundaryCondition)
            .Bind(static v => v is PropertyValue.Text t ? Some(t.Value) : None)
            .IfNone((string?)null));

    static string Identifier(Node.Object node) => node.ExternalId.IfNone(node.Name);

    static (Seq<EnvelopeFace> Faces, Seq<EnergyNote> Notes) Envelope(
        ElementGraph graph, Node.Object space, GeometrySource geometry) =>
        Boundaries(graph, space.Id).Fold(
            (Faces: Seq<EnvelopeFace>(), Notes: Seq<EnergyNote>()),
            (state, bound) =>
                geometry.Footprint(bound.Surface.Representations).Case is not FootprintPolygon ring
                    ? (state.Faces, state.Notes.Add(new EnergyNote(EnergyReason.FootprintMissing, Identifier(bound.Surface), 1)))
                    : EnergyClassRows.FaceOf(bound.Surface.Classification.Code, bound.Surface.PredefinedType).Case is Hb.FaceType faceType
                        ? (state.Faces.Add(new EnvelopeFace(bound.Surface, faceType, Condition(bound.Edge), ring,
                               OpeningsOf(graph, space.Id, Identifier(bound.Surface)))), state.Notes)
                        : (state.Faces, state.Notes.Add(new EnergyNote(EnergyReason.ClassUnmapped, Identifier(bound.Surface), 1))));

    static Seq<Node.Object> OpeningsOf(ElementGraph graph, NodeId space, string hostIdentifier) =>
        toSeq(graph.EdgesAt(space)).Choose(e =>
            e is Relationship.Generic g && g.WireName == IfcRelKind.SpaceBoundary.Key && g.Relating == space
                && g.Attributes.Find(BoundaryRows.Host).Exists(v => v is PropertyValue.Text t && t.Value == hostIdentifier)
                ? graph.Find<Node.Object>(g.Related) : None)
            .Filter(static o => o.Classification.Code == IfcClass.Window.Key || o.Classification.Code == IfcClass.Door.Key);

    static Seq<Node.Object> SpacesUnder(ElementGraph graph, EnergyScope scope) =>
        graph.ObjectNodes.Filter(o => o.Classification.Code == IfcClass.Space.Key)
            .Filter(o => scope.Switch(
                wholeModel: static _ => true,
                spaces:     s => o.ExternalId.Exists(s.GlobalIds.Contains)));

    static Seq<(Relationship.Generic Edge, Node.Object Surface)> Boundaries(ElementGraph graph, NodeId space) =>
        toSeq(graph.EdgesAt(space)).Choose(e =>
            e is Relationship.Generic g && g.WireName == IfcRelKind.SpaceBoundary.Key && g.Relating == space
                && g.Attributes.Find(BoundaryRows.Host).IsNone
                ? graph.Find<Node.Object>(g.Related).Map(s => (g, s))
                : None);

    static (Seq<Hb.Aperture> Apertures, Seq<Hb.Door> Doors, LowerLog Log) Openings(
        ElementGraph graph, EnvelopeFace bound, GeometrySource geometry, Hb.ModelEnergyProperties store, LowerLog log) =>
        bound.Openings.Fold(
            (Apertures: Seq<Hb.Aperture>(), Doors: Seq<Hb.Door>(), Log: log),
            (state, opening) => {
                if (geometry.Footprint(opening.Representations).Case is not FootprintPolygon ring) {
                    return (state.Apertures, state.Doors,
                            state.Log.Note(EnergyReason.FootprintMissing, Identifier(opening)));
                }
                (Option<string> construction, LowerLog composed) = Composed(graph, opening.Id, store, state.Log);
                LowerLog opened = composed.Land(EnergySlot.Opening);
                return opening.Classification.Code == IfcClass.Window.Key
                    ? (state.Apertures.Add(new Hb.Aperture(Identifier(opening), Face3D(ring),
                           new Hb.Outdoors(), new Hb.AperturePropertiesAbridged(
                               energy: HostEdge.Slot(construction.Map(
                                   static id => new Hb.ApertureEnergyPropertiesAbridged(construction: id))))),
                       state.Doors, opened)
                    : (state.Apertures,
                       state.Doors.Add(new Hb.Door(Identifier(opening), Face3D(ring),
                           new Hb.Outdoors(), new Hb.DoorPropertiesAbridged(
                               energy: HostEdge.Slot(construction.Map(
                                   static id => new Hb.DoorEnergyPropertiesAbridged(construction: id))))),
                       opened);
            });

    static Fin<EnergyOutcome.Emitted> Dragonfly(ElementGraph graph, EnergyScope scope, GeometrySource geometry, Instant at) {
        ContentAddress pedigree = ContentAddress.OfGraph(graph);
        (Seq<Df.Building> Buildings, Seq<NodeId> Massed, LowerLog Log) built = graph.ObjectNodes
            .Filter(o => o.Classification.Code == IfcClass.Building.Key)
            .Fold((Buildings: Seq<Df.Building>(), Massed: Seq<NodeId>(), Log: LowerLog.Empty),
                (state, building) => {
                    (Seq<Df.Story> stories, LowerLog log) = Stories(graph, building, scope, geometry, state.Log);
                    return stories.IsEmpty
                        ? (state.Buildings, state.Massed, log)
                        : (state.Buildings.Add(new Df.Building(Identifier(building),
                               new Df.BuildingPropertiesAbridged(), uniqueStories: [.. stories])),
                           state.Massed.Add(building.Id), log);
                });
        if (built.Buildings.IsEmpty) {
            return Fin.Fail<EnergyOutcome.Emitted>(new BimFault.Refused(BimScope.Energy, BimReason.Capability, string.Join(':', new object?[] { "energy-lower-empty", InterchangeFormat.Dfjson.Key, Scoped(scope) })));
        }
        var model = new Df.Model($"rasm-massing-{pedigree.Value:x32}", new Df.ModelProperties(),
            buildings: [.. built.Buildings], units: Df.Units.Meters, tolerance: graph.Header.Tolerance,
            contextShades: [.. Context(graph, geometry, built.Massed)],
            referenceVector: [graph.Header.Reference.Eastings, graph.Header.Reference.Northings, graph.Header.Reference.OrthogonalHeight]);
        return Fin.Succ(Emit(InterchangeFormat.Dfjson, Encoding.UTF8.GetBytes(model.ToJson()), pedigree, at,
            built.Log.Annotated(model.Validate()).Census(InterchangeFormat.Dfjson, at)));
    }

    static (Seq<Df.Story> Stories, LowerLog Log) Stories(
        ElementGraph graph, Node.Object building, EnergyScope scope, GeometrySource geometry, LowerLog log) =>
        Parts(graph, building.Id, IfcClass.BuildingStorey).Fold(
            (Stories: Seq<Df.Story>(), Log: log),
            (state, storey) => {
                (Seq<Df.Room2D> plates, LowerLog folded) = Plates(graph, storey, scope, geometry, state.Log);
                return plates.IsEmpty
                    ? (state.Stories, folded)
                    : (state.Stories.Add(new Df.Story(Identifier(storey), [.. plates],
                           new Df.StoryPropertiesAbridged(), multiplier: Multiplier(graph, storey.Id))), folded);
            });

    static (Seq<Df.Room2D> Plates, LowerLog Log) Plates(
        ElementGraph graph, Node.Object storey, EnergyScope scope, GeometrySource geometry, LowerLog log) =>
        Parts(graph, storey.Id, IfcClass.Space).Filter(s => InScope(s, scope)).Fold(
            (Plates: Seq<Df.Room2D>(), Log: log),
            (state, space) => {
                if (geometry.Footprint(space.Representations).Case is not FootprintPolygon ring) {
                    return (state.Plates, state.Log.Note(EnergyReason.FootprintMissing, Identifier(space)));
                }
                (Seq<EnvelopeFace> envelope, Seq<EnergyNote> notes) = Envelope(graph, space, geometry);
                Seq<Vector3> plate = Open(ring.Ring, graph.Header.Tolerance);
                double height = Height(graph, space.Id).IfNone(DefaultFloorToCeiling);
                Seq<((Vector3 From, Vector3 To) Wall, Option<EnvelopeFace> Face)> joined =
                    Segments(plate).Map(wall => (Wall: wall, Face: Aligned(envelope, wall)));
                Map<string, double> spans = joined
                    .Choose(row => row.Face.Map(face =>
                        (Id: Identifier(face.Surface), Length: Vector3.Distance(row.Wall.From, row.Wall.To))))
                    .Fold(Map<string, double>(), static (held, row) =>
                        held.AddOrUpdate(row.Id, existing => existing + row.Length, row.Length));
                return (state.Plates.Add(new Df.Room2D(
                        Identifier(space),
                        [.. plate.Map(static p => (List<double>)[p.X, p.Y])],
                        plate.Head.Map(static p => p.Z).IfNone(0.0),
                        height,
                        new Df.Room2DPropertiesAbridged(),
                        isGroundContact: envelope.Exists(static f => f.Face == Hb.FaceType.Floor && f.Condition == BoundaryRow.Ground),
                        isTopExposed: envelope.Exists(static f => f.Face == Hb.FaceType.RoofCeiling && f.Condition == BoundaryRow.Outdoors),
                        boundaryConditions: [.. joined.Map(static row =>
                            row.Face.Map(static face => face.Condition).IfNone(BoundaryRow.Outdoors).Plate())],
                        windowParameters: [.. joined.Map(row => Glazing(row.Face, row.Wall, height, spans, geometry))],
                        floorHoles: ring.Holes.IsEmpty ? null : [.. ring.Holes.Map(hole =>
                            (List<List<double>>)[.. Open(hole, graph.Header.Tolerance).Map(static p => (List<double>)[p.X, p.Y])])])),
                    state.Log
                        .Noted(notes)
                        .Noted(joined.Filter(static row => row.Face.IsNone)
                            .Map(_ => new EnergyNote(EnergyReason.SegmentUnmatched, Identifier(space), 1)))
                        .Land(EnergySlot.Space));
            });

    static Seq<Df.ContextShade> Context(ElementGraph graph, GeometrySource geometry, Seq<NodeId> massed) =>
        graph.ObjectNodes
            .Filter(o => o.Classification.Code == IfcClass.GeographicElement.Key
                || (o.Classification.Code == IfcClass.Building.Key && !massed.Contains(o.Id)))
            .Choose(o => geometry.Footprint(o.Representations).Map(ring =>
                new Df.ContextShade(Identifier(o), [(ShadeGeometry)Face3D(ring)], new Df.ContextShadePropertiesAbridged())));

    static Seq<Vector3> Open(Seq<Vector3> ring, double tolerance) =>
        ring.Count > 1 && ring.Head.Exists(head => Vector3.Distance(head, ring.Last) <= tolerance)
            ? ring.Take(ring.Count - 1)
            : ring;

    static Seq<(Vector3 From, Vector3 To)> Segments(Seq<Vector3> ring) =>
        ring.Count < 3
            ? Seq<(Vector3, Vector3)>()
            : ring.Map((point, index) => (From: point, To: ring[(index + 1) % ring.Count]));

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

    static PlateWindow? Glazing(
        Option<EnvelopeFace> matched, (Vector3 From, Vector3 To) wall, double height,
        Map<string, double> spans, GeometrySource geometry) {
        double length = Vector3.Distance(wall.From, wall.To);
        double area = length * height;
        return matched.Match(
            Some: face => {
                double span = spans.Find(Identifier(face.Surface)).IfNone(length);
                double glazed = span > 0.0
                    ? face.Openings
                        .Filter(static o => o.Classification.Code == IfcClass.Window.Key)
                        .Fold(0.0, (sum, window) => sum + geometry.Footprint(window.Representations)
                            .Map(static ring => ring.Area).IfNone(0.0)) * (length / span)
                    : 0.0;
                return area > 0.0 && glazed > 0.0
                    ? (PlateWindow)new Df.SimpleWindowRatio(Math.Min(glazed / area, 1.0))
                    : null;
            },
            None: static () => null);
    }

    static Vector3 Centre(Seq<Vector3> ring) =>
        ring.IsEmpty ? Vector3.Zero : ring.Fold(Vector3.Zero, static (sum, point) => sum + point) * (1.0 / ring.Count);

    static double PlanDistance(Vector3 a, Vector3 b) =>
        Math.Sqrt(((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y)));

    const double DefaultFloorToCeiling = 3.0;

    static Option<double> Height(ElementGraph graph, NodeId space) =>
        toSeq(graph.EdgesAt(space)).Choose(e =>
            e is Relationship.Assign { SubKind: var k } a && k == AssignKind.PropertyDefinition && a.Subject == space
                ? graph.Find<Node.QuantitySet>(a.Definition) : None)
            .Filter(static qs => qs.Bag.SetName == QuantityRows.SpaceBaseQuantities).Head
            .Bind(static qs => qs.Bag.Values.Find(QuantityRows.Height))
            .Bind(static m => m.Length);

    static int Multiplier(ElementGraph graph, NodeId storey) =>
        toSeq(graph.EdgesAt(storey)).Choose(e =>
            e is Relationship.Assign { SubKind: var k } a && k == AssignKind.PropertyDefinition && a.Subject == storey
                ? graph.Find<Node.PropertySet>(a.Definition) : None)
            .Filter(static ps => ps.Bag.SetName == EnergyProjector.EnergyModelSet).Head
            .Bind(static ps => ps.Bag.Values.Find(EnergyProjector.StoryMultiplier))
            .Bind(static v => v is PropertyValue.Measure m ? Some((int)m.Value.Si) : None)
            .IfNone(1);

    static Seq<Node.Object> Parts(ElementGraph graph, NodeId whole, IfcClass @class) =>
        toSeq(graph.EdgesAt(whole)).Choose(e =>
            e is Relationship.Compose c && c.Whole == whole && c.SubKind != ComposeKind.Reference
                ? graph.Find<Node.Object>(c.Part).Filter(o => o.Classification.Code == @class.Key)
                : None);

    static string Scoped(EnergyScope scope) => scope.Switch(
        wholeModel: static _ => "whole-model",
        spaces:     static s => string.Join(',', s.GlobalIds));

    static bool InScope(Node.Object space, EnergyScope scope) => scope.Switch(
        wholeModel: static _ => true,
        spaces:     s => space.ExternalId.Exists(s.GlobalIds.Contains));

    static EnergyOutcome.Emitted Emit(InterchangeFormat format, byte[] bytes, ContentAddress graph, Instant at, EnergyCensus census) {
        EnergyArtifact artifact = EnergyArtifact.Of(format, bytes, Some(graph), at);
        return new EnergyOutcome.Emitted(artifact, census with { Key = artifact.ContentKey });
    }
}
```

## [03]-[TRANSLATE_MATRIX]

- Owner: `EnergyTranslate` the OSM-centric translator matrix — one frozen `(source, target)` row table over the OpenStudio translators, never a per-pair method family; `TranslateLane` the one governance value each row carries (the observe hooks beside the managed abort token); `TranslateStage` the declared stage-fraction ladder every governed leg opens on; `TranslateProgress` the single SWIG director that is both the lane's progress adapter and its finest in-flight token read.
- Entry: `EnergyTranslate.Run(EnergyDoc source, InterchangeFormat target, Instant at, TranslateLane lane)` → `Fin<EnergyOutcome.Emitted>` resolves the `(source, target)` matrix row — `osm→gbxml` (`GbXMLForwardTranslator.modelToGbXMLString`), `osm→idf` (`EnergyPlusForwardTranslator.translateModel` + `Workspace.save`), `gbxml→osm`/`idf→osm` (the reverse readers + `Model.save`), `osm→osm` (the `VersionTranslator` version-upgrade row) — and emits the translated bytes as an `EnergyArtifact` (no graph pedigree — a translation never touched the graph) with the translator `warnings()`/`errors()` tallied into the `Translated` census; every run threads ONE `TranslateProgress` director onto the verified translator overloads (`loadModelFromString(string, ProgressBar)`, `translateModel(Model, ProgressBar)`, `modelToGbXMLString(Model, ProgressBar)`, `loadModel(Path, ProgressBar)`) unconditionally, its `onPercentageUpdated(double)` override publishing through `TranslateStage.Native` — the roster's own projection mapping a translator percentage into the declared native span under that row's witness — onto the `Model/observability#HOOKS` `rasm.bim.energy.progress` observe point, and latching the lane token, so a long translation surfaces monotone stage positions and answers an abort with zero translator coupling and no per-call-site absence fork.
- Packages: NREL.OpenStudio.macOS-arm64, Rasm, LanguageExt.Core, NodaTime
- Growth: a new governance checkpoint is one `TranslateStage` row whose declared fraction the observe point reads with no arithmetic elsewhere; a new translation is one `Matrix` row over a verified translator member (SDD via `SddForwardTranslator`/`SddReverseTranslator` is the named next row); the matrix's SOURCE axis is closed at what the binding ships — no HBJSON reader exists, so a graph→OSM/gbXML/IDF egress is never a matrix row and stays the `energy-graph-egress-pending` `Refused/BimReason.Capability` refusal, the egress riding `Lower` HBJSON → the python peer's `honeybee-openstudio` wire → this matrix.
- Boundary: OpenStudio publishes NO native interrupt — the `ProgressBar` director surface is percentage, range, and visibility alone — so the abort GRAIN is the `TranslateStage` boundary and the director callback, an observed abandonment discards the translated result at the next boundary rather than interrupting the running translator, and claiming a mid-translator abort is the overclaim this Law forecloses; abandonment lowers the LanguageExt `Errors.Cancelled`, the branch's one cancellation spelling, never a third `BimFault` case; an exception thrown across the SWIG director frame is the deleted form because it unwinds native frames holding live handles, so the director LATCHES and the managed boundary decides; every native leg crosses ONE kernel `Try.lift` funnel, exceptional captures gain `BimBoundary.OpenStudioRaise` while retaining the original `Error`, and returned typed errors pass through unchanged; the translate temp-path crossings ride ONE `Scratch` bracket and the SWIG handle brackets are the named platform-forced statement boundary; `Workspace.save`/`Model.save` path-bound emits cross a bracketed scratch file exactly as the decode arms do; a matrix miss returns `Refused` with `BimReason.Codec`, an unreadable source `Refused` with `BimReason.Rejected`.

```csharp
public sealed record TranslateLane(Option<BimHooks> Hooks, CancellationToken Cancel) {
    public static readonly TranslateLane Ungoverned = new(None, CancellationToken.None);
}

[SmartEnum]
public sealed partial class TranslateStage {
    public static readonly TranslateStage Decoded = new(done: 0.00, witness: "decode");
    public static readonly TranslateStage Translated = new(done: 0.10, witness: "translate");
    public static readonly TranslateStage Emitted = new(done: 0.90, witness: "emit");
    public static readonly TranslateStage Sealed = new(done: 1.00, witness: "seal");

    public double Done { get; }
    public string Witness { get; }

    public StageMark Mark => new(Done, Witness);

    public static StageMark Native(double percentage) =>
        new(Translated.Done + ((Emitted.Done - Translated.Done) * Math.Clamp(percentage / 100.0, 0.0, 1.0)),
            Translated.Witness);
}

public static class EnergyTranslate {
    static readonly FrozenDictionary<(InterchangeFormat Source, InterchangeFormat Target), Func<EnergyDoc, TranslateProgress, Fin<(byte[] Bytes, int Warnings)>>> Matrix =
        new KeyValuePair<(InterchangeFormat, InterchangeFormat), Func<EnergyDoc, TranslateProgress, Fin<(byte[], int)>>>[] {
            new((InterchangeFormat.Osm,   InterchangeFormat.GbXml), static (doc, key, bar) => OsmTo(doc, bar, static (model, tally, progress, _) => {
                using Os.GbXMLForwardTranslator gb = new();
                byte[] emitted = Encoding.UTF8.GetBytes(gb.modelToGbXMLString(model, progress));
                return Fin.Succ((emitted, tally + Tally(gb.warnings(), gb.errors())));
            })),
            new((InterchangeFormat.Osm,   InterchangeFormat.Idf),   static (doc, key, bar) => OsmTo(doc, bar, static (model, tally, progress, op) => {
                using Os.EnergyPlusForwardTranslator ep = new();
                using Os.Workspace idf = ep.translateModel(model, progress);
                return Saved(op, w => idf.save(w, true))
                    .Map(bytes => (bytes, tally + Tally(ep.warnings(), ep.errors())));
            })),
            new((InterchangeFormat.Osm,   InterchangeFormat.Osm),   static (doc, key, bar) => OsmTo(doc, bar, static (model, tally, _, op) =>
                Saved(op, w => model.save(w, true)).Map(bytes => (bytes, tally)))),
            new((InterchangeFormat.GbXml, InterchangeFormat.Osm),   static (doc, key, bar) => ReverseTo(doc, bar)),
            new((InterchangeFormat.Idf,   InterchangeFormat.Osm),   static (doc, key, bar) => ReverseTo(doc, bar)),
        }.ToFrozenDictionary();

    internal static Fin<EnergyOutcome.Emitted> Run(EnergyDoc source, InterchangeFormat target, Instant at, TranslateLane lane) {
        if (!Matrix.TryGetValue((source.Format, target), out var row)) {
            return Fin.Fail<EnergyOutcome.Emitted>(new BimFault.Refused(BimScope.Energy, BimReason.Codec, string.Join(':', new object?[] { "energy-translate-miss", source.Format.Key, target.Key })));
        }
        using TranslateProgress progress = new(lane);
        return Opened(TranslateStage.Decoded, progress, lane)
            .Bind(_ => row(source, progress))
            .Bind(result => Opened(TranslateStage.Sealed, progress, lane).Map(_ => result))
            .Map(result => {
                EnergyArtifact artifact = EnergyArtifact.Of(target, result.Bytes, None, at);
                return new EnergyOutcome.Emitted(artifact, new EnergyCensus(
                    EnergyLeg.Translated, source.Format, Some(target), 0, 0, 0, 0,
                    Seq(new EnergyNote(EnergyReason.TranslatorLog, $"{source.Format.Key}->{target.Key}", result.Warnings)),
                    artifact.ContentKey, at));
            });
    }

    static Fin<Unit> Opened(TranslateStage stage, TranslateProgress progress, TranslateLane lane) {
        ignore(progress.Open(stage));
        return lane.Cancel.IsCancellationRequested || progress.Abandoned
            ? Fin.Fail<Unit>(Errors.Cancelled)
            : Fin.Succ(unit);
    }

    sealed class TranslateProgress(TranslateLane lane) : Os.ProgressBar {
        public bool Abandoned { get; private set; }

        public Unit Open(TranslateStage stage) =>
            lane.Hooks.IfSome(live => ignore(live.Fire(BimPoint.EnergyProgress, new BimFact.Progress(ProgressLane.Energy, stage.Mark))));

        public override void onPercentageUpdated(double percentage) {
            Abandoned = Abandoned || lane.Cancel.IsCancellationRequested;
            lane.Hooks.IfSome(live => ignore(live.Fire(BimPoint.EnergyProgress,
                new BimFact.Progress(ProgressLane.Energy, TranslateStage.Native(percentage)))));
        }
    }

    static Option<BimFault.BoundaryFailed> NativeFailure(Error cause) =>
        cause.Exception.Case is ApplicationException
            ? Some(new BimFault.BoundaryFailed(BimBoundary.OpenStudioRaise, cause))
            : None;

    static Option<BimFault.BoundaryFailed> ScratchFailure(Error cause) =>
        cause.Exception.Case is IOException or UnauthorizedAccessException
            ? Some(new BimFault.BoundaryFailed(BimBoundary.HostScratchWrite, cause))
            : None;

    static Fin<T> Native<T>(Func<Fin<T>> leg) =>
        Try.lift(leg).Run().Bind(static inner => inner);

    static Fin<T> Scratch<T>(Option<ReadOnlyMemory<byte>> seed, Func<Os.Path, Func<Fin<byte[]>>, Fin<T>> cross) =>
        IO.lift(() => Try.lift(() => {
                string temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                return Fin.Succ(temp);
            }).Run().Bind(static inner => inner))
            .Bracket(
                Use: temp => IO.lift(() => seed.Match(
                        Some: bytes => Try.lift(() => {
                            File.WriteAllBytes(temp, bytes.ToArray());
                            return Fin.Succ(unit);
                        }).Run().Bind(static inner => inner),
                        None: static () => Fin.Succ(unit))
                    .Bind(_ => Native(() => {
                        using Os.Path path = Os.OpenStudioUtilitiesCore.toPath(temp);
                        return cross(path, () => Try.lift(() => File.ReadAllBytes(temp)).Run());
                    }))),
                Fin: temp => IO.lift(() => Try.lift(() => {
                    File.Delete(temp);
                    return Fin.Succ(unit);
                }).Run().Bind(static inner => inner)))
            .Try().runFin.As().Run();

    static Fin<(byte[], int)> OsmTo(EnergyDoc doc, TranslateProgress bar, Func<Os.Model, int, Os.ProgressBar, Fin<(byte[], int)>> emit) =>
        Native(() => {
            using Os.VersionTranslator vt = new();
            using Os.OptionalModel optional = vt.loadModelFromString(doc.Text, bar);
            if (!optional.is_initialized()) {
                return Fin.Fail<(byte[], int)>(new BimFault.Refused(BimScope.Energy, BimReason.Rejected, "energy-decode:osm:unreadable"));
            }
            bar.Open(TranslateStage.Translated);
            Os.Model model = optional.get();
            return emit(model, Tally(vt.warnings(), vt.errors()), bar);
        });

    static Fin<(byte[], int)> ReverseTo(EnergyDoc doc, TranslateProgress bar) =>
        Scratch(Some(doc.Bytes), (path, _) => Native(() => {
            if (doc.Format == InterchangeFormat.GbXml) {
                using Os.GbXMLReverseTranslator gb = new();
                using Os.OptionalModel fromGb = gb.loadModel(path, bar);
                if (!fromGb.is_initialized()) {
                    return Fin.Fail<(byte[], int)>(new BimFault.Refused(BimScope.Energy, BimReason.Rejected, "energy-decode:gbxml:unreadable"));
                }
                bar.Open(TranslateStage.Emitted);
                return Save(fromGb.get(), Tally(gb.warnings(), gb.errors()));
            }
            using Os.EnergyPlusReverseTranslator ep = new();
            using Os.OptionalModel fromIdf = ep.loadModel(path, bar);
            if (!fromIdf.is_initialized()) {
                return Fin.Fail<(byte[], int)>(new BimFault.Refused(BimScope.Energy, BimReason.Rejected, "energy-decode:idf:unreadable"));
            }
            bar.Open(TranslateStage.Emitted);
            return Save(fromIdf.get(), Tally(ep.warnings(), ep.errors()));
        }));

    static Fin<(byte[], int)> Save(Os.Model model, int warnings) =>
        Saved(w => model.save(w, true)).Map(bytes => (bytes, warnings));

    static Fin<byte[]> Saved(Action<Os.Path> save) =>
        Scratch(None, (path, read) => { save(path); return read(); });

    static int Tally(Os.LogMessageVector warnings, Os.LogMessageVector errors) {
        using (warnings) using (errors) { return warnings.Count + errors.Count; }
    }
}
```

## [04]-[RESEARCH]

(none)
