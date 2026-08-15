# [BIM_ENERGY_DERIVE]

`EnergyDerive` lowers the seam `ElementGraph` to two authoring schemas, and `EnergyTranslate` runs the OpenStudio translator matrix as one frozen `(source, target)` row table. `IfcSpace` nodes — landed by the `Projection/semantic#SEMANTIC_PROJECTOR` IFC ingest or any projector — fold to honeybee `Room`/`Face`/`Aperture`/`Door` building envelopes with their layered seam composition lowered onto the energy library under the abridged-reference law, or to dragonfly `Story`/`Room2D` massing plates. `Energy/exchange#ENERGY_EXCHANGE`'s `EnergyExchange.Apply` drives both arms and emits content-keyed `EnergyArtifact`s; class correspondence, the energy-owned boundary payload statics, and evidence bags read back from the `Energy/projector#ENERGY_PROJECTOR` owner — the space-boundary `Host`/`Level` rows composing the seam-declared `BoundaryRows` statics at both ends — so the raise and the lower cannot drift.

Wire posture is HOST-LOCAL, foreign types emit-confined: each lowered `Hb.Model`/`Df.Model` is authored, `Validate()`-noted, serialized through `ToJson()`, and released inside the arm; every `OpenStudio.*` SWIG wrapper is `using`-bracketed, and the translate temp-path crossings (`Workspace.save`/`Model.save` over a bracketed scratch file) are the platform-forced statement seam. Faults route the `Model/faults#FAULT_BAND` arms: `CodecReject` (`energy-lower-unsupported`/`energy-translate-miss`), `CapabilityMiss` (`energy-graph-egress-pending`), `ModelRejected` (`energy-decode` on an unreadable translate source, `energy-translate` on a native throw), and the kernel `Rasm.Domain` `Fault.Cancelled` on abandonment.

## [01]-[INDEX]

- [02]-[MODEL_DERIVE]: `EnergyDerive` lowers the graph onto the honeybee building envelope + energy library (opaque AND glazing constructions, opening sub-faces with their own constructions) over the seam `GeometrySource` port, and onto dragonfly massing with the height/multiplier evidence read back.
- [03]-[TRANSLATE_MATRIX]: the `EnergyTranslate` OSM-centric translator matrix (osm↔gbxml, osm↔idf, osm version-upgrade) as frozen row data over the OpenStudio translators.

## [02]-[MODEL_DERIVE]

- Owner: `EnergyDerive` the BIM-to-BEM lower fold (graph → honeybee HBJSON building envelope + energy library, graph → dragonfly DFJSON massing); `BoundaryRow` the closed boundary-condition vocabulary carrying one honeybee closure, its derived dragonfly projection, and the ONE `Admit` both the raise and the lower resolve a condition token through; `EnvelopeFace` the segment-aligned building-envelope row both arms read; `MaterialArm` the per-construction-family policy value one set lower is parameterized by; `LowerLog` the immutable accumulation threaded through every fold.
- Entry: `EnergyDerive.Lower(ElementGraph graph, InterchangeFormat target, EnergyScope scope, GeometrySource geometry, Instant at, Op key)` → `Fin<EnergyOutcome.Emitted>` — dispatches the frozen `Lowers` target table: the `hbjson` arm lowers each scoped `IfcSpace` and its opening sub-faces onto the honeybee building envelope + energy library, the `dfjson` arm folds the `Compose` tree onto dragonfly massing plates whose per-segment boundary conditions, window ratios, ground contact, and sky exposure read that same building envelope, with the site's un-massed neighbours lowered onto `ContextShade` and the seam georeference onto `ReferenceVector`; each surface and opening composition lowers through ONE property-case fold.
- Auto: lowered models carry the SEMANTIC building envelope and library only; simulation context — parameters, run period, conditioning, weather — is Compute's or the python recipe plane's, never authored on the lower. `Envelope` derives each space's bounding surfaces ONCE — face type, boundary row, footprint ring, attributed openings — so the two arms cannot drift about which wall carries which condition or which window, and the dragonfly arm joins those surfaces to floor-boundary SEGMENTS in plan within half a segment length so the parameter lists index the walls they describe.
- Receipt: `LowerLog` threads the spaces/surfaces/openings/constructions tallies and the typed `Energy/exchange#ENERGY_EXCHANGE` `EnergyNote` degrade rows through every fold, and one `EnergyReceipt` per emit projects it — the model's `Validate()` DataAnnotations results fold onto the SAME rows so the warning tally is one fold over one row family, never an exception and never a second counter. Every degrade names the node it dropped against, so a reader reaches the surface rather than a number.
- Packages: HoneybeeSchema, DragonflySchema, Rasm.Element, Rasm, LanguageExt.Core, NodaTime
- Growth: a new lower target is one row on the frozen `Lowers` target table (the `EnergyProjector.Arms`/`EnergyTranslate.Matrix` row law); a new boundary condition is one `BoundaryRow` row both schemas project from; a richer glazing posture is one `PlateWindow` case swapped at the `Glazing` return with the same measured quotient behind it; per-space program/loads lower as `ProgramTypeAbridged` rows once the seam carries occupancy evidence; a NoMass R-value lower is one `MaterialArm` row the moment the seam carries an R-value-only thermal case; the space-adjacency `Surface` boundary condition needs no new row — `BoundaryRow.Surface` already projects into both schemas and waits only on the raise stamping the counterpart-face payload.
- Boundary: `EnergyDerive` reads the graph through seam-owned surfaces and the `Model/query#ELEMENT_SET` scope algebra; Compute is a peer stratum, never a dependency, so its discipline reads (`SpacesOf`/`BoundingSurfacesOf`) are never referenced. Building-envelope derivation is SHARED and single — a per-arm boundary walk is the deleted form that let two emitted documents disagree about one building — and the ring area both arms need is the seam owner's `FootprintPolygon.Area` — the Newell fold, shell minus holes, seated where the rings live — because a vertical aperture ring projects to near-zero area under the planar NTS algebra the geospatial owner holds and a page-local ring-area helper re-derives arithmetic the carrier owns. Every missing- or ambiguous-evidence path degrades onto a typed `EnergyReason` row naming its subject — a footprint-less space, a material lacking both the `Thermal` and `Optical` case, a wall segment no bounding surface matched — never a zero-area fabrication or a fabricated physics row; a zero-area glazing sum emits dragonfly's ABSENT window slot and never a `SimpleWindowRatio` of `0`, which a solver reads as a real zero-area window rather than as no window. `graph→OSM`/`gbXML`/`IDF` DIRECT egress is a PROVED negative, not a pending arm: the binding's translator matrix admits OSM-family sources alone — its reverse roster is gbXML/IDF/SDD/three.js/floorplan and no HBJSON ingest exists on it, so no matrix column feeds from the lowered honeybee document — HBJSON→OSM is the python peer's `honeybee-openstudio` wire leg, and a second graph→OSM builder beside Compute's simulation-scoped `BuildModel` is the duplicate-fold defect, so the request rails `BimFault.CapabilityMiss` and the egress composes as `Lower` HBJSON → peer wire → `Translate`. Glazing lowering consumes the same seam `Optical` case (`Discipline.Energy`) Compute's `StandardGlazing` build reads, so the lowered honeybee document and Compute's OSM model agree on layer physics by construction.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
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
using Rasm.Bim.Model;                        // BimFault + the Detail roster every energy leg raises through
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
// Keys compare case-INSENSITIVELY because the three source vocabularies disagree on case for one condition — an
// IDF field is case-insensitive by spec and a reverse-translated "OUTDOORS" is legal, exactly as BySurfaceType
// reads its own tokens.
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

    // Both ends compose ONE admission: the raise resolves a honeybee/OSM/IDF condition token here and stamps its
    // row key on the edge, and the lower reads that key back through the same roster. An unrecognized or absent
    // token resolves to the NAMED Outdoors default rather than falling out of an unowned `_` arm.
    public static BoundaryRow Admit(string? token) =>
        token is not null && TryGet(token, out BoundaryRow? row) && row is { } found ? found : Outdoors;
}

// ONE material-arm policy value per construction family: Layer lands a single material row on the store and
// answers its identifier (None where the layer's property case is absent, degrading the whole set), Construction
// lands the family's construction row over those ids, and Prefix seeds the content id. Two parallel lower bodies
// differing only in which seam property case they read and which schema row they mint were two places for the
// dedup key or the layer order to drift, and a third family — a NoMass R-value row — is one arm here.
sealed record MaterialArm(
    string Prefix,
    Func<Hb.ModelEnergyProperties, MaterialLayer, Node.Material, Option<string>> Layer,
    Func<Hb.ModelEnergyProperties, string, Seq<string>, Unit> Construction) {

    // HoneybeeSchema itself declares this glazing-conductivity default — the one authoring default the schema
    // states, never fabricated physics.
    const double GlazingConductivity = 0.9;

    // Optical-free sets lower EnergyMaterial rows (density reads the seam Mechanical case, REQUIRED — the
    // Compute BuildConstruction rails typed on absent density and rejects OpenStudio's fabricated defaults, so a
    // density literal here would fork the physics the two documents must agree on; Thermal carries no density).
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

    // All-Optical sets lower EnergyWindowMaterialGlazing rows over the seam Optical case (Discipline.Energy):
    // nine [0,1] fractions map onto the schema columns 1:1, and Thermal conductivity rides when carried.
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

// LowerLog carries the lower's whole accumulation through every fold: landing tallies each receipt column
// reports, and typed EnergyNote rows behind the warning count. `ref int warnings` threaded a bare counter through six
// signatures and answered how many drops a document took but never which evidence it lost or which surface lost
// it, so a thinner emitted model read identically to a complete one plus a number.
public sealed record LowerLog(int Spaces, int Surfaces, int Openings, int Constructions, Seq<EnergyNote> Notes) {
    public static readonly LowerLog Empty = new(0, 0, 0, 0, Seq<EnergyNote>());

    public LowerLog Land(EnergySlot slot) => slot.Switch(
        space:        () => this with { Spaces = Spaces + 1 },
        surface:      () => this with { Surfaces = Surfaces + 1 },
        opening:      () => this with { Openings = Openings + 1 },
        construction: () => this with { Constructions = Constructions + 1 });

    public LowerLog Noted(Seq<EnergyNote> notes) => this with { Notes = Notes + notes };
    public LowerLog Note(EnergyReason reason, string subject) => Noted(Seq(new EnergyNote(reason, subject, 1)));

    // Schema annotations fold onto the SAME rows the graph degrades take, so the warning tally stays one fold
    // over one row family and a reader sees WHICH member the schema flagged.
    public LowerLog Annotated(IEnumerable<ValidationResult> results) =>
        Noted(toSeq(results).Map(static r => new EnergyNote(EnergyReason.SchemaAnnotation, r.ErrorMessage ?? "", 1)));

    public EnergyReceipt Receipt(InterchangeFormat form, Instant at) =>
        new(EnergyLeg.Lowered, form, None, Spaces, Surfaces, Openings, Constructions, Notes, default, at);
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
    // is ONE row here, never a widened ternary chain; a miss splits CapabilityMiss (a raise-served OSM-family
    // form whose direct graph egress the binding forecloses — no HBJSON source column exists to feed the
    // translator matrix from a lowered model) from CodecReject (a form no energy arm serves).
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
                ? Fin.Fail<EnergyOutcome.Emitted>(Detail.EnergyEgressPending.At(key, target.Key))
                : Fin.Fail<EnergyOutcome.Emitted>(Detail.EnergyLowerUnsupported.At(key, target.Key));

    static Fin<EnergyOutcome.Emitted> Honeybee(ElementGraph graph, EnergyScope scope, GeometrySource geometry, Instant at, Op key) {
        // Model identifier IS the graph pedigree: a timestamp identifier forks the emitted BYTES per second, so a
        // re-lowered identical graph never byte-matches and the object-plane 412-noop dedup never fires; a
        // content-stable identifier restores the reuse join the dual-key law states.
        ContentAddress pedigree = ContentAddress.OfGraph(graph);
        var store = new Hb.ModelEnergyProperties();
        (Seq<Hb.Room> Rooms, LowerLog Log) built = SpacesUnder(graph, scope).Fold(
            (Rooms: Seq<Hb.Room>(), Log: LowerLog.Empty),
            (state, space) => {
                (Hb.Room room, LowerLog log) = Room(graph, space, geometry, store, state.Log);
                return (state.Rooms.Add(room), log);
            });
        // An empty envelope is a CAPABILITY answer, not a thin document: a zero-room HBJSON validates, emits, and
        // content-keys exactly like a real one, so a caller who scoped to spaces the graph does not hold received a
        // legal model of nothing and a simulation of it. The fault names the scope, which is the only thing that
        // distinguishes "this graph carries no spaces" from "your selection matched none".
        if (built.Rooms.IsEmpty) {
            return Fin.Fail<EnergyOutcome.Emitted>(Detail.EnergyLowerEmpty.At(key, InterchangeFormat.Hbjson.Key, Scoped(scope)));
        }
        var model = new Hb.Model($"rasm-energy-{pedigree.Value:x32}", new Hb.ModelProperties(energy: store),
            rooms: [.. built.Rooms], units: Hb.Units.Meters, tolerance: graph.Header.Tolerance);
        return Fin.Succ(Emit(InterchangeFormat.Hbjson, Encoding.UTF8.GetBytes(model.ToJson()), pedigree, at,
            built.Log.Annotated(model.Validate()).Receipt(InterchangeFormat.Hbjson, at)));
    }

    // ONE space lowered: the shared envelope's faces with their constructions and sub-faces, the log threaded
    // through every face and opening so each drop is recorded against the surface that took it.
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
                            energy: construction.Match(
                                Some: static id => new Hb.FaceEnergyPropertiesAbridged(construction: id),
                                None: static () => (Hb.FaceEnergyPropertiesAbridged?)null)),
                        apertures: [.. apertures], doors: [.. doors])),
                    opened.Land(EnergySlot.Surface));
            });
        return (new Hb.Room(Identifier(space), [.. folded.Faces], new Hb.RoomPropertiesAbridged()),
                folded.Log.Land(EnergySlot.Space));
    }

    // Composed pairs the composition lower with its landing: a resolved construction advances the construction
    // column and the degrade rows the fold produced ride the same step, so a face and its openings share one
    // accounting.
    static (Option<string> Id, LowerLog Log) Composed(
        ElementGraph graph, NodeId node, Hb.ModelEnergyProperties store, LowerLog log) {
        (Option<string> id, Seq<EnergyNote> notes) = LowerComposition(graph, node, store);
        LowerLog noted = log.Noted(notes);
        return (id, id.IsSome ? noted.Land(EnergySlot.Construction) : noted);
    }

    // ONE property-case-discriminated composition lower, dedup-appended through the identifier-keyed
    // Extension-backed Add* mutators; the construction id IS the material-key join (the layer ids + SI
    // thicknesses content-hashed) so identical buildups across N surfaces dedup to ONE library entry. The family
    // choice is which MaterialArm the set resolves to; a MIXED or property-incomplete set has no legal EnergyPlus
    // construction — a noted degrade, the Compute BuildConstruction mixed rejection mirrored, never a
    // silently-wrong opaque lowering. Each degrade names the NODE it dropped against, so a reader reaches the
    // surface rather than a tally.
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

    // ONE parameterized set lower over the arm's per-family mints: any layer the arm cannot mint degrades the
    // WHOLE set, because a buildup missing one ply is not a thinner buildup but a different wall.
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
        $"{prefix}-{ContentHash.Of(set.Layers.Fold(new CanonicalWriter(0.0),
            static (w, layer) => w.String(layer.Material.ToString()).Double(layer.Thickness.Si)).ToBytes().Span):x32}";

    // Holes ride the seam carrier whole: a courtyard slab lowers as boundary plus hole loops, so honeybee's own
    // face algebra subtracts the court instead of conditioning it.
    static Hb.Face3D Face3D(FootprintPolygon ring) =>
        new([.. ring.Ring.Map(static p => (List<double>)[p.X, p.Y, p.Z])],
            holes: ring.Holes.IsEmpty ? null : [.. ring.Holes.Map(static hole => (List<List<double>>)[.. hole.Map(static p => (List<double>)[p.X, p.Y, p.Z])])]);

    // Ingest stamps a BoundaryRow key on the edge, so this read is the roster's own admission — one vocabulary,
    // one default, both ends. Surface adjacency stays reachable the moment the seam payload carries its
    // counterpart-face ids — that row already exists, only the raise's stamp is pending.
    static BoundaryRow Condition(Relationship.Generic edge) =>
        BoundaryRow.Admit(edge.Attributes.Find(EnergyProjector.BoundaryCondition)
            .Bind(static v => v is PropertyValue.Text t ? Some(t.Value) : None)
            .IfNone((string?)null));

    // Identifier answers what a lowered object is called: its raise-side ExternalId correlation where that
    // exists, its node name otherwise. One read, so both arms and every sub-face name one surface identically.
    static string Identifier(Node.Object node) => node.ExternalId.IfNone(node.Name);

    // ONE envelope derivation both arms read: each bounding surface's face type, boundary-condition row, footprint
    // ring, and attributed opening occurrences, gathered once per space. A footprint-less or class-unmapped surface
    // notes its own typed reason against its own identifier and drops — never a zero-area fabrication — and both
    // arms read one fold, so they carry one set of drops naming one set of subjects.
    static (Seq<EnvelopeFace> Faces, Seq<EnergyNote> Notes) Envelope(
        ElementGraph graph, Node.Object space, GeometrySource geometry) =>
        Boundaries(graph, space.Id).Fold(
            (Faces: Seq<EnvelopeFace>(), Notes: Seq<EnergyNote>()),
            (state, bound) =>
                geometry.Footprint(bound.Surface.Representations).Case is not FootprintPolygon ring
                    ? (state.Faces, state.Notes.Add(new EnergyNote(EnergyReason.FootprintMissing, Identifier(bound.Surface), 1)))
                    // The face read is the roster's OWN two-rung ladder — the predefined-token row where the source
                    // stamped one, else the class row — so a wall carrying a real STANDARD token resolves where a
                    // mandatory-token key made it miss and drop.
                    : EnergyClassRows.FaceOf(bound.Surface.Classification.Code, bound.Surface.PredefinedType).Case is Hb.FaceType faceType
                        ? (state.Faces.Add(new EnvelopeFace(bound.Surface, faceType, Condition(bound.Edge), ring,
                               OpeningsOf(graph, space.Id, Identifier(bound.Surface)))), state.Notes)
                        : (state.Faces, state.Notes.Add(new EnergyNote(EnergyReason.ClassUnmapped, Identifier(bound.Surface), 1))));

    // Openings of one face: the space's boundary edges whose Host attribute names the face identifier — a raise
    // correlation idiom read back, never a NodeId join (rooted ids are raise-local). Window and door occurrences
    // ride ONE set because both arms need the same nodes: the honeybee arm splits them by class into sub-faces, the
    // dragonfly arm sums the window rings alone into a glazing ratio.
    static Seq<Node.Object> OpeningsOf(ElementGraph graph, NodeId space, string hostIdentifier) =>
        graph.EdgesAt(space).Choose(e =>
            e is Relationship.Generic g && g.WireName == IfcRelKind.SpaceBoundary.Key && g.Relating == space
                && g.Attributes.Find(BoundaryRows.Host).Exists(v => v is PropertyValue.Text t && t.Value == hostIdentifier)
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
                && g.Attributes.Find(BoundaryRows.Host).IsNone
                ? graph.Find<Node.Object>(g.Related).Map(s => (g, s))
                : None).ToSeq();

    // Honeybee sub-face lowering over the SHARED opening set: one pass yields both lists (IfcWindow -> Aperture,
    // IfcDoor -> Door) and each opening's own composition lowers through the SAME discriminated fold so a raised
    // window construction round-trips onto the aperture's abridged reference.
    // OpeningsOf already filters this set to exactly two classes, so the window/door split is TOTAL and the fold
    // needs no third arm and no silent drop.
    static (Seq<Hb.Aperture> Apertures, Seq<Hb.Door> Doors, LowerLog Log) Openings(
        ElementGraph graph, EnvelopeFace bound, GeometrySource geometry, Hb.ModelEnergyProperties store, LowerLog log) =>
        bound.Openings.Fold(
            (Apertures: Seq<Hb.Aperture>(), Doors: Seq<Hb.Door>(), Log: log),
            (state, opening) => {
                // Footprint-less openings note their own drop exactly as a footprint-less surface does — a
                // silent skip under-glazed the emitted model with zero receipt evidence, the deleted asymmetry.
                if (geometry.Footprint(opening.Representations).Case is not FootprintPolygon ring) {
                    return (state.Apertures, state.Doors,
                            state.Log.Note(EnergyReason.FootprintMissing, Identifier(opening)));
                }
                (Option<string> construction, LowerLog composed) = Composed(graph, opening.Id, store, state.Log);
                LowerLog opened = composed.Land(EnergySlot.Opening);
                return opening.Classification.Code == IfcClass.Window.Key
                    ? (state.Apertures.Add(new Hb.Aperture(Identifier(opening), Face3D(ring),
                           new Hb.Outdoors(), new Hb.AperturePropertiesAbridged(
                               energy: construction.Match(
                                   Some: static id => new Hb.ApertureEnergyPropertiesAbridged(construction: id),
                                   None: static () => (Hb.ApertureEnergyPropertiesAbridged?)null)))),
                       state.Doors, opened)
                    : (state.Apertures,
                       state.Doors.Add(new Hb.Door(Identifier(opening), Face3D(ring),
                           new Hb.Outdoors(), new Hb.DoorPropertiesAbridged(
                               energy: construction.Match(
                                   Some: static id => new Hb.DoorEnergyPropertiesAbridged(construction: id),
                                   None: static () => (Hb.DoorEnergyPropertiesAbridged?)null)))),
                       opened);
            });

    // Dragonfly massing: the OWNING Compose tree lowers Building/Story shells and each space's footprint plate
    // flattens onto a Room2D floor boundary — massing altitude only, no energy library, the honeybee shape
    // inverted; storey multiplier evidence reads back onto Story(multiplier:), so a unique-stories-x-repeat tower
    // round-trips its repeat factor. A building contributing no plated storey is site context, not massing.
    static Fin<EnergyOutcome.Emitted> Dragonfly(ElementGraph graph, EnergyScope scope, GeometrySource geometry, Instant at, Op key) {
        ContentAddress pedigree = ContentAddress.OfGraph(graph);   // the content-stable identifier + Graph pedigree, one derivation
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
            return Fin.Fail<EnergyOutcome.Emitted>(Detail.EnergyLowerEmpty.At(key, InterchangeFormat.Dfjson.Key, Scoped(scope)));
        }
        var model = new Df.Model($"rasm-massing-{pedigree.Value:x32}", new Df.ModelProperties(),
            buildings: [.. built.Buildings], units: Df.Units.Meters, tolerance: graph.Header.Tolerance,
            // Site context is the DOMINANT shading term in any urban setting, and the geospatial owner already
            // landed the neighbours on the SAME graph this fold walks — so an emitted massing model without them
            // computed solar gain as if the site were empty desert. ReferenceVector is how dragonfly relocates a
            // local model onto the earth for solar position; without it every sun path is computed at the model
            // origin, so a georeferenced graph emitted a model that could not know where it stood.
            contextShades: [.. Context(graph, geometry, built.Massed)],
            referenceVector: [graph.Header.Reference.Eastings, graph.Header.Reference.Northings, graph.Header.Reference.OrthogonalHeight]);
        return Fin.Succ(Emit(InterchangeFormat.Dfjson, Encoding.UTF8.GetBytes(model.ToJson()), pedigree, at,
            built.Log.Annotated(model.Validate()).Receipt(InterchangeFormat.Dfjson, at)));
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

    // Every plate reads the SHARED envelope: boundaryConditions and windowParameters index the floor-boundary
    // SEGMENTS one for one, isGroundContact reads a Ground-conditioned floor face and isTopExposed an
    // Outdoors-conditioned roof face. Filling five of nineteen columns emitted a sealed opaque box — no glazing, no
    // soil contact, no sky — whose every solar gain, daylight autonomy, and glazing-ratio result was wrong by the
    // whole envelope, and the omission was invisible because each widened column is schema-OPTIONAL and Validate
    // passes. The graph the honeybee arm reads to full envelope depth is the same graph this arm reads.
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
                // ONE alignment join per segment feeds BOTH parameter lists: running it for the boundary condition
                // and again inside the glazing quotient walked every wall face twice per plate AND let a tie
                // between two equidistant faces resolve one way in one list and the other way in the other.
                Seq<((Vector3 From, Vector3 To) Wall, Option<EnvelopeFace> Face)> joined =
                    Segments(plate).Map(wall => (Wall: wall, Face: Aligned(envelope, wall)));
                // The join is MANY segments to ONE face: a plate's boundary is a polyline and a bounding wall
                // surface spans however many of its segments run along that wall, so several segments legitimately
                // match one face. Handing each of them the face's WHOLE aperture area over its own extruded area
                // multiplied the glazing by the match count — a four-segment curtain wall emitted four fully-glazed
                // walls where the source carried one. The face's matched span is folded ONCE here and each segment
                // then takes its own length share of the aperture area, so the emitted ratios sum to the ratio the
                // face actually carries.
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
                        // Hole plates lower each interior ring as one further coordinate list, so a courtyard,
                        // atrium, or lightwell subtracts from conditioned floor area instead of massing solid.
                        floorHoles: ring.Holes.IsEmpty ? null : [.. ring.Holes.Map(hole =>
                            (List<List<double>>)[.. Open(hole, graph.Header.Tolerance).Map(static p => (List<double>)[p.X, p.Y])])])),
                    state.Log
                        .Noted(notes)
                        .Noted(joined.Filter(static row => row.Face.IsNone)
                            .Map(_ => new EnergyNote(EnergyReason.SegmentUnmatched, Identifier(space), 1)))
                        .Land(EnergySlot.Space));
            });

    // Site context is every footprint-bearing geographic element the geospatial projector landed plus every
    // IfcBuilding this fold did NOT mass — a neighbour block ingested as a site-context occurrence carries no
    // storey decomposition, so "contributed no stories" is exactly the discriminant that separates the model's own
    // buildings from the ones surrounding it, read off the massed set rather than re-walked.
    static Seq<Df.ContextShade> Context(ElementGraph graph, GeometrySource geometry, Seq<NodeId> massed) =>
        graph.ObjectNodes
            .Filter(o => o.Classification.Code == IfcClass.GeographicElement.Key
                || (o.Classification.Code == IfcClass.Building.Key && !massed.Contains(o.Id)))
            .Choose(o => geometry.Footprint(o.Representations).Map(ring =>
                new Df.ContextShade(Identifier(o), [(ShadeGeometry)Face3D(ring)], new Df.ContextShadePropertiesAbridged())))
            .ToSeq();

    // Dragonfly's FloorBoundary is an OPEN ring — the schema closes it implicitly — so a source ring carrying its
    // repeated first vertex emits a zero-length final wall and shifts every parameter list by one against the
    // segments it indexes. Closure is a TOLERANCE question in a tolerance-bearing graph: a ring whose ends round
    // to the same point under the model tolerance is closed, and an exact compare left that ring closed by a
    // sub-tolerance gap, re-admitting the degenerate final wall it exists to remove.
    static Seq<Vector3> Open(Seq<Vector3> ring, double tolerance) =>
        ring.Count > 1 && ring.Head.Exists(head => Vector3.Distance(head, ring.Last) <= tolerance)
            ? ring.Take(ring.Count - 1).ToSeq()
            : ring;

    // Each floor-boundary SEGMENT is one Room2D wall, so every per-wall parameter list indexes by segment and the
    // ring closes back to its first vertex. A ring under three vertices bounds no plate and yields no segments, so
    // every parameter list stays empty rather than misaligned against a boundary that is not there.
    static Seq<(Vector3 From, Vector3 To)> Segments(Seq<Vector3> ring) =>
        ring.Count < 3
            ? Seq<(Vector3, Vector3)>()
            : ring.Map((point, index) => (From: point, To: ring[(index + 1) % ring.Count]));

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

    // Glazing answers one ALREADY-MATCHED wall segment's window parameter: aperture area over wall area, where the wall is that
    // segment EXTRUDED to the floor-to-ceiling height — Room2D's own massing model, so the denominator needs no
    // area owner at all — and the numerator is the seam owner's FootprintPolygon.Area Newell fold over the window
    // rings the raise attributed to the matched face (exact for a VERTICAL ring the planar NTS algebra reads as
    // near-zero). Doors are excluded: a door is not glazing and folding it into the ratio over-states solar gain
    // on every entrance wall. Segments with no matched face, a degenerate wall, or zero aperture area yield NULL,
    // because dragonfly's own ABSENT slot is how "no glazing" is spelled — a SimpleWindowRatio of 0 reads
    // downstream as a real zero-area window the solver still meshes and reports. Clamping the quotient below one
    // keeps a mis-measured ring from emitting a wall more than fully glazed.
    static PlateWindow? Glazing(
        Option<EnvelopeFace> matched, (Vector3 From, Vector3 To) wall, double height,
        Map<string, double> spans, GeometrySource geometry) {
        double length = Vector3.Distance(wall.From, wall.To);
        double area = length * height;
        return matched.Match(
            Some: face => {
                // Share is this segment's length over the whole span its face matched, so N segments of one wall
                // partition that wall's aperture area instead of each claiming all of it. A face matched by exactly
                // one segment takes share 1 and the arithmetic collapses to the direct quotient.
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

    // Plan distance alone: a wall surface's ring centre sits at mid-height while its floor-boundary
    // segment lies on the slab, so a 3D distance rejects every real match by half the storey height.
    static double PlanDistance(Vector3 a, Vector3 b) =>
        Math.Sqrt(((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y)));

    // Massing fallback when a space carries no Qto height — a NAMED policy default, never a silent zero; a real
    // height reads the Qto_SpaceBaseQuantities bag the IFC ingest OR the dragonfly raise landed. Geometry defaults
    // stay legal where physics defaults do not: a fabricated storey height distorts massing, a fabricated density
    // distorts the simulation the two documents must agree on.
    const double DefaultFloorToCeiling = 3.0;

    static Option<double> Height(ElementGraph graph, NodeId space) =>
        graph.EdgesAt(space).Choose(e =>
            e is Relationship.Assign { SubKind: var k } a && k == AssignKind.PropertyDefinition && a.Subject == space
                ? graph.Find<Node.QuantitySet>(a.Definition) : None)
            .Filter(static qs => qs.Bag.SetName == QuantityRows.SpaceBaseQuantities).Head
            .Bind(static qs => qs.Bag.Values.Find(QuantityRows.Height))
            .Bind(static m => m.Length);

    // Raise StoryMultiplier evidence read back through the projector-owned symbol (never a re-spelled literal);
    // absent evidence is multiplier 1 — the dragonfly schema default.
    static int Multiplier(ElementGraph graph, NodeId storey) =>
        graph.EdgesAt(storey).Choose(e =>
            e is Relationship.Assign { SubKind: var k } a && k == AssignKind.PropertyDefinition && a.Subject == storey
                ? graph.Find<Node.PropertySet>(a.Definition) : None)
            .Filter(static ps => ps.Bag.SetName == EnergyProjector.EnergyModelSet).Head
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

    // The scope rendered for a fault message — the case IS the modality, so the render is the union's own Switch
    // and no leg re-describes a scope it did not build.
    static string Scoped(EnergyScope scope) => scope.Switch(
        wholeModel: static _ => "whole-model",
        spaces:     static s => string.Join(',', s.GlobalIds));

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

- Owner: `EnergyTranslate` the OSM-centric translator matrix — one frozen `(source, target)` row table over the OpenStudio translators, never a per-pair method family; `TranslateLane` the one governance value each row carries (the observe hooks beside the managed abort token); `TranslateStage` the declared stage-fraction ladder every governed leg opens on; `TranslateProgress` the single SWIG director that is both the lane's progress adapter and its finest in-flight token read.
- Entry: `EnergyTranslate.Run(EnergyDoc source, InterchangeFormat target, Instant at, Op key, TranslateLane lane)` → `Fin<EnergyOutcome.Emitted>` resolves the `(source, target)` matrix row — `osm→gbxml` (`GbXMLForwardTranslator.modelToGbXMLString`), `osm→idf` (`EnergyPlusForwardTranslator.translateModel` + `Workspace.save`), `gbxml→osm`/`idf→osm` (the reverse readers + `Model.save`), `osm→osm` (the `VersionTranslator` version-upgrade row) — and emits the translated bytes as an `EnergyArtifact` (no graph pedigree — a translation never touched the graph) with the translator `warnings()`/`errors()` tallied into the `Translated` receipt; every run threads ONE `TranslateProgress` director onto the verified translator overloads (`loadModelFromString(string, ProgressBar)`, `translateModel(Model, ProgressBar)`, `modelToGbXMLString(Model, ProgressBar)`, `loadModel(Path, ProgressBar)`) unconditionally, its `onPercentageUpdated(double)` override publishing through `TranslateStage.Native` — the roster's own projection mapping a translator percentage into the declared native span under that row's witness — onto the `Model/observability#HOOK_RAIL` `rasm.bim.energy.progress` observe point, and latching the lane token, so a long translation surfaces monotone stage positions and answers an abort with zero translator coupling and no per-call-site absence fork.
- Packages: NREL.OpenStudio.macOS-arm64, Rasm, LanguageExt.Core, NodaTime
- Growth: a new governance checkpoint is one `TranslateStage` row whose declared fraction the observe point reads with no arithmetic elsewhere; a new translation is one `Matrix` row over a verified translator member (SDD via `SddForwardTranslator`/`SddReverseTranslator` is the named next row); the matrix's SOURCE axis is closed at what the binding ships — no HBJSON reader exists, so a graph→OSM/gbXML/IDF egress is never a matrix row and stays the `energy-graph-egress-pending` `CapabilityMiss` rail, the egress riding `Lower` HBJSON → the python peer's `honeybee-openstudio` wire → this matrix.
- Boundary: OpenStudio publishes NO native interrupt — the `ProgressBar` director surface is percentage, range, and visibility alone — so the abort GRAIN is the `TranslateStage` boundary and the director callback, an observed abandonment discards the translated result at the next boundary rather than interrupting the running translator, and claiming a mid-translator abort is the overclaim this Law forecloses; abandonment lowers the kernel `Rasm.Domain` `Fault.Cancelled`, the branch's one cancellation spelling, never a sixth `Model/faults#FAULT_BAND` arm for a lifecycle outcome; an exception thrown across the SWIG director frame is the deleted form because it unwinds native frames holding live handles, so the director LATCHES and the managed boundary decides; the translate temp-path crossings and the SWIG handle brackets are the named platform-forced statement seam; `Workspace.save`/`Model.save` path-bound emits cross a bracketed scratch file exactly as the decode arms do; a matrix miss rails `CodecReject` (`energy-translate-miss`), an unreadable source `ModelRejected` (`energy-decode`).

```csharp signature
// Shares the [02] RUNTIME_PRELUDE (the Os alias and the seam usings — one compilation unit per page).
// OSM-centric translator matrix: (source, target) rows over verified OpenStudio members; a translation is one
// row, never a per-pair method family. Path-bound emits cross a bracketed scratch file.
// The translate governance lane: the observe hooks a composition supplies beside the managed abort token, ONE value
// so a matrix row carries governance in one slot instead of a nullable bar. OpenStudio publishes NO native interrupt
// — its ProgressBar director surface is percentage, range, and visibility, nothing that aborts a running translator
// — so the abort GRAIN is the stage boundary and the director callback: an observed abandonment discards the
// translated result at the next boundary rather than interrupting the native leg, and the observation rides the
// declared TranslateStage fraction. Ungoverned is the real absence a hook-less, token-less caller spells.
public sealed record TranslateLane(Option<BimHooks> Hooks, CancellationToken Cancel) {
    public static readonly TranslateLane Ungoverned = new(None, CancellationToken.None);
}

// The declared stage ladder every governed leg OPENS on — each row its completed fraction and its abandonment
// witness, so a published fraction is a measured stage position rather than a ratio no producer counts. Composing
// the kernel arrangement band's shape keeps one governance grammar across the branch's two long native lanes.
[SmartEnum]
public sealed partial class TranslateStage {
    // Every row's fraction is the work COMPLETED when that row OPENS, so a consumer reads a position rather than a
    // milestone: Decoded opens the decode at zero, Translated opens the native translation with the decode behind
    // it, Emitted opens the byte emit with the translation behind it, and Sealed opens nothing with everything
    // behind it. The retired ladder fired Emitted AFTER the emit had already run, so its 0.90 described work that
    // was finished and a consumer watching the lane saw the run stall at ninety percent and then vanish.
    public static readonly TranslateStage Decoded = new(done: 0.00, witness: "decode");
    public static readonly TranslateStage Translated = new(done: 0.10, witness: "translate");
    public static readonly TranslateStage Emitted = new(done: 0.90, witness: "emit");
    public static readonly TranslateStage Sealed = new(done: 1.00, witness: "seal");

    public double Done { get; }
    public string Witness { get; }

    // Rows PROJECT the one Model/observability#HOOK_RAIL StageMark carrier — the mark is that page's shape, the
    // roster is this lane's membership — so a managed stage boundary and a native measurement publish one fact.
    public StageMark Mark => new(Done, Witness);

    // Translated is the row the NATIVE leg publishes under, and a director percentage maps INTO that row's declared
    // span rather than riding as a raw ratio: the native call sits between Translated and Emitted, so a translator
    // reporting 100 mid-run would publish 1.00 and the emit boundary that follows would then publish 0.90, running
    // a consumer's position backwards. Clamping keeps a translator overshooting its own range inside the span.
    public static StageMark Native(double percentage) =>
        new(Translated.Done + ((Emitted.Done - Translated.Done) * Math.Clamp(percentage / 100.0, 0.0, 1.0)),
            Translated.Witness);
}

public static class EnergyTranslate {
    static readonly FrozenDictionary<(InterchangeFormat Source, InterchangeFormat Target), Func<EnergyDoc, Op, TranslateProgress, Fin<(byte[] Bytes, int Warnings)>>> Matrix =
        new KeyValuePair<(InterchangeFormat, InterchangeFormat), Func<EnergyDoc, Op, TranslateProgress, Fin<(byte[], int)>>>[] {
            new((InterchangeFormat.Osm,   InterchangeFormat.GbXml), static (doc, key, bar) => OsmTo(doc, key, bar, static (model, tally, progress) => {
                using Os.GbXMLForwardTranslator gb = new();
                byte[] emitted = Encoding.UTF8.GetBytes(gb.modelToGbXMLString(model, progress));
                return (emitted, tally + Tally(gb.warnings(), gb.errors()));
            })),
            new((InterchangeFormat.Osm,   InterchangeFormat.Idf),   static (doc, key, bar) => OsmTo(doc, key, bar, static (model, tally, progress) => {
                using Os.EnergyPlusForwardTranslator ep = new();
                using Os.Workspace idf = ep.translateModel(model, progress);
                return (Saved(w => idf.save(w, true)), tally + Tally(ep.warnings(), ep.errors()));
            })),
            new((InterchangeFormat.Osm,   InterchangeFormat.Osm),   static (doc, key, bar) => OsmTo(doc, key, bar, static (model, tally, _) =>
                (Saved(w => model.save(w, true)), tally))),   // the VersionTranslator upgrade row: decode already upgraded
            new((InterchangeFormat.GbXml, InterchangeFormat.Osm),   static (doc, key, bar) => ReverseTo(doc, key, bar)),
            new((InterchangeFormat.Idf,   InterchangeFormat.Osm),   static (doc, key, bar) => ReverseTo(doc, key, bar)),
        }.ToFrozenDictionary();

    internal static Fin<EnergyOutcome.Emitted> Run(EnergyDoc source, InterchangeFormat target, Instant at, Op key, TranslateLane lane) {
        if (!Matrix.TryGetValue((source.Format, target), out var row)) {
            return Fin.Fail<EnergyOutcome.Emitted>(Detail.EnergyTranslateMiss.At(key, source.Format.Key, target.Key));
        }
        // ONE director per run, bracketed with the SWIG seam and ALWAYS present, so every translator call takes its
        // ProgressBar-bearing overload unconditionally: absence is one Option read inside the director, never a fork
        // at each of the five call sites a nullable bar once guarded.
        using TranslateProgress progress = new(lane, key);
        return Opened(TranslateStage.Decoded, progress, lane, key)
            .Bind(_ => row(source, key, progress))
            .Bind(result => Opened(TranslateStage.Sealed, progress, lane, key).Map(_ => result))
            .Map(result => {
                EnergyArtifact artifact = EnergyArtifact.Of(target, result.Bytes, None, at);
                // OpenStudio publishes its diagnostics as a NATIVE vector this branch counts but cannot
                // enumerate as managed rows, so the leg lands ONE note carrying the matrix row as its subject and
                // its native count as the tally — the exact case EnergyNote.Tally exists for.
                return new EnergyOutcome.Emitted(artifact, new EnergyReceipt(
                    EnergyLeg.Translated, source.Format, Some(target), 0, 0, 0, 0,
                    Seq(new EnergyNote(EnergyReason.TranslatorLog, $"{source.Format.Key}->{target.Key}", result.Warnings)),
                    artifact.ContentKey, at));
            });
    }

    // A stage OPENS by publishing its declared fraction and reading BOTH halves of the abandonment evidence — the
    // caller's token and whatever the director latched mid-native-call. Abandonment lowers the branch's ONE
    // cancellation spelling (the kernel Rasm.Domain Fault.Cancelled), so an energy translate and a geometry fold
    // abandon under one vocabulary and no sixth BimFault arm is minted for a lifecycle outcome.
    static Fin<Unit> Opened(TranslateStage stage, TranslateProgress progress, TranslateLane lane, Op key) {
        ignore(progress.Open(stage));
        return lane.Cancel.IsCancellationRequested || progress.Abandoned
            ? Fin.Fail<Unit>(new Fault.Cancelled())
            : Fin.Succ(unit);
    }

    // SWIG director subclass and the lane's ONE native-side adapter: OpenStudio calls the virtual
    // onPercentageUpdated across the native boundary, so this override is both the finest progress the translator
    // publishes and the finest point the managed token can be READ while native code runs. It fires the
    // Model/observability#HOOK_RAIL rasm.bim.energy.progress observe point through the roster's OWN Native
    // projection — a declared row's witness carrying the measured position mapped into that row's span, never a
    // free-text stage slot or a raw ratio — and latches the abandonment the next stage boundary acts on; the latch
    // never throws across the director frame, because an exception crossing a SWIG director unwinds native frames
    // that own live handles.
    sealed class TranslateProgress(TranslateLane lane, Op key) : Os.ProgressBar {
        public bool Abandoned { get; private set; }

        // A managed stage boundary publishes through the SAME director the native callback fires on, so the run
        // holds ONE lane handle and a row inside the matrix opens its stage without the lane and the key being
        // re-threaded to every emit lambda.
        public Unit Open(TranslateStage stage) =>
            lane.Hooks.IfSome(h => ignore(h.EnergyProgress.Fire(new BimFact.Progress(key, "energy", stage.Mark))));

        public override void onPercentageUpdated(double percentage) {
            Abandoned = Abandoned || lane.Cancel.IsCancellationRequested;
            lane.Hooks.IfSome(h => ignore(h.EnergyProgress.Fire(
                new BimFact.Progress(key, "energy", TranslateStage.Native(percentage)))));
        }
    }

    // Decode-then-emit over the version-upgrading in-string read; the emit lambda owns its translator brackets
    // and threads the one director onto the verified ProgressBar overloads.
    static Fin<(byte[], int)> OsmTo(EnergyDoc doc, Op key, TranslateProgress bar, Func<Os.Model, int, Os.ProgressBar, (byte[], int)> emit) {
        try {
            using Os.VersionTranslator vt = new();
            using Os.OptionalModel optional = vt.loadModelFromString(doc.Text, bar);
            if (!optional.is_initialized()) {
                return Fin.Fail<(byte[], int)>(Detail.EnergyDecode.At(key, "osm", "unreadable"));
            }
            // The decode is behind us and the native translation is next, so the Translated row OPENS here — the
            // one point in the run where that fraction is the truth. Its own span is what the director percentage
            // then maps into.
            bar.Open(TranslateStage.Translated);
            Os.Model model = optional.get();
            return Fin.Succ(emit(model, Tally(vt.warnings(), vt.errors()), bar));
        }
        catch (Exception ex) when (ex is SystemException or ApplicationException) {
            return Fin.Fail<(byte[], int)>(Detail.EnergyTranslate.At(key, ex.Message));
        }
    }

    // gbXML/IDF -> OSM: the Path-bound reverse readers over a bracketed temp file, saved back as .osm bytes.
    static Fin<(byte[], int)> ReverseTo(EnergyDoc doc, Op key, TranslateProgress bar) {
        string temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try {
            File.WriteAllBytes(temp, doc.Bytes.ToArray());
            using Os.Path path = Os.OpenStudioUtilitiesCore.toPath(temp);
            if (doc.Format == InterchangeFormat.GbXml) {
                using Os.GbXMLReverseTranslator gb = new();
                using Os.OptionalModel fromGb = gb.loadModel(path, bar);
                if (!fromGb.is_initialized()) {
                    return Fin.Fail<(byte[], int)>(Detail.EnergyDecode.At(key, "gbxml", "unreadable"));
                }
                bar.Open(TranslateStage.Emitted);
                return Save(fromGb.get(), Tally(gb.warnings(), gb.errors()));
            }
            using Os.EnergyPlusReverseTranslator ep = new();
            using Os.OptionalModel fromIdf = ep.loadModel(path, bar);
            if (!fromIdf.is_initialized()) {
                return Fin.Fail<(byte[], int)>(Detail.EnergyDecode.At(key, "idf", "unreadable"));
            }
            bar.Open(TranslateStage.Emitted);
            return Save(fromIdf.get(), Tally(ep.warnings(), ep.errors()));
        }
        catch (Exception ex) when (ex is SystemException or ApplicationException) {
            return Fin.Fail<(byte[], int)>(Detail.EnergyTranslate.At(key, ex.Message));
        }
        finally { File.Delete(temp); }
    }

    // Save BORROWS: the caller's `using Os.OptionalModel` is the sole owner of the native handle `get()` projects,
    // so disposing the model here released a handle the optional's own bracket then released again — a double free
    // across the SWIG boundary that surfaces as a native crash rather than a managed fault.
    static Fin<(byte[], int)> Save(Os.Model model, int warnings) =>
        Fin.Succ((Saved(w => model.save(w, true)), warnings));

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

(none)
