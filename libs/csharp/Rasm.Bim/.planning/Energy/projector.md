# [BIM_ENERGY_PROJECTOR]

`EnergyProjector : IElementProjection` mints the energy-model raise, second Bim projector beside `Projection/semantic#SEMANTIC_PROJECTOR` on the `Exchange/reconstruct#RECONSTRUCTION` precedent: five decode arms — HBJSON/DFJSON managed, OSM/gbXML/IDF SWIG — converge on ONE projection the `Rasm.Compute/Analysis/energy` runner simulates with zero adapter.

`Energy/exchange#ENERGY_EXCHANGE` `EnergyExchange.Apply` `Raise` drives it under `ProjectionAssembly.Assemble` with `IfcLegality` composed; `Energy/derive#MODEL_DERIVE` lowers the same class table, boundary statics, and evidence bags back, so raise and lower never drift.

Every arm lands one Compute-readable shape: `IfcSpace`-classified `Object` nodes, bounding-surface Objects joined by `IfcRelSpaceBoundary`-named neutral `Generic` edges carrying the `BoundaryLevel` `"2nd"` payload, `Host`-attributed opening boundaries for every window and door, footprints content-keyed into `Representations.FootPrint`, and `MaterialComposition.LayerSet` evidence — opaque layers `MaterialPropertySet.Thermal`, glazing the `Optical` case the Compute `StandardGlazing` build reads; wire names, `Qto`, and `Pset` are the load-bearing alignment.

Wire posture is HOST-LOCAL, foreign types decode-confined: `Model.FromJson` gates hard by construction (parse throw, in-parse DataAnnotations), no `HoneybeeSchema.*`/`DragonflySchema.*` DTO outlives `Project`, and every `OpenStudio.*` SWIG wrapper — model, translators, `Optional*`, `*Vector`, per-element handles — is `using`-bracketed, index-loop with per-element disposal the marshaling exemption. Faults route the `Model/faults#FAULT_BAND` arms: `ModelRejected` (`energy-decode`), `UnmappedClass` (`energy-face-miss`/`energy-class-miss`), `DanglingReference` (`energy-construction-absent`).

## [01]-[INDEX]

- [02]-[ENERGY_PROJECTOR]: `EnergyProjector : IElementProjection` — five format arms onto one raise fold landing the Compute-readable seam shape from the `EnergyClassRows` correspondence table.

## [02]-[ENERGY_PROJECTOR]

- Owner: `EnergyProjector : IElementProjection` the energy-model raise (the raw `EnergyDoc` captured internally, the seam contract carrying only `Node`/`Relationship`/`GraphDelta`); `EnergyClassRows` the ONE FaceType↔`IfcClass` correspondence table both directions derive from — the raise map, the OSM `Surface.surfaceType()` string leg, the OSM `SubSurface.subSurfaceType()` opening leg, and the lower-side `ToFace` inverse; the frozen `Arms` format→arm index the `Serves` capability predicate reads; `RaiseState` the whole threaded accumulation (delta, footprint blobs, landing tallies, degrade rows) with `EnergySlot` its landing vocabulary and `Resolved` the composition answer that carries its own degrade rows.
- Entry: `EnergyProjector.Project(ProjectionContext ctx)` → `Fin<GraphDelta>` — the frozen `Arms` index dispatches the captured document's format onto its arm, `Hbjson`/`Dfjson` decoding through the managed `Model.FromJson` `Try.lift` funnel and `Osm`/`GbXml`/`Idf` through the bracketed SWIG decode trio converging on ONE `RaiseOsm` fold; an unmapped format faults `energy-form-miss`.
- Auto: openings mint `IfcWindow`/`IfcDoor` on EVERY arm, honeybee `Aperture`/`Door` and OSM `Surface.subSurfaces()` alike, never only the managed formats; the dragonfly massing arm lands each `Room2D` floor-to-ceiling height as a `Qto_SpaceBaseQuantities` `Height` quantity, stamps `Story.Multiplier > 1` as `Pset_EnergyModel` evidence, and routes `Building.Room3ds` through the same honeybee room fold.
- Receipt: `RaiseState` threads `Spaces`/`Surfaces`/`Openings`/`Constructions` and the typed `Energy/exchange#ENERGY_EXCHANGE` `EnergyNote` degrade rows through the fold, and the run edge commits it to the boundary cell in ONE swap the `Energy/exchange#ENERGY_EXCHANGE` `EnergyReceipt` and the footprint side-channel both read — instance counters a `Fin`-returning arm bumped on the side survived a failed fold and named no subject. Managed decodes reject inside `FromJson`, so the raise notes degrade rows only — `Validate()` annotations belong to the lower legs authoring models locally.
- Packages: HoneybeeSchema, DragonflySchema, NREL.OpenStudio.macOS-arm64, Rasm.Element, Rasm, LanguageExt.Core, Thinktecture.Runtime.Extensions
- Growth: a new face/class correspondence is one `EnergyClassRows` row (both directions derive); a new OSM opening token is one `ByOpeningType` row; a new energy form is one `Arms` row; a dragonfly parameter (window ratios, shading, skylights) deepens the massing arm as row folds over the `Room2D` `AnyOf` unions; a NoMass/Vegetation material arm is one typed-layer row the moment the seam carries an R-value-only thermal case; the FULL (non-abridged) `OpaqueConstruction`/`WindowConstruction` store rows — inline material OBJECTS, not id references, so a different resolve shape — are one `Library` projection widening with one inline-material arm the moment full-form documents ship, a full-form construction id resolving in neither abridged list faulting `DanglingReference` before that (the declared abridged-only restraint, never a silent partial read); honeybee `Shade`/`ShadeMesh` context geometry raises as one arm row the moment an `IfcShadingDevice` roster row is exercised by a consumer read.
- Boundary: `EnergyMaterial` density has NO seam thermal column and a fabricated `OfMechanical` stiffness is the rejected form — density is DROPPED at the raise (systematic, never a per-material warning), the OSM rebuild's 1000 kg/m³ fallback carrying the consequence. Every physics literal is a NAMED policy value on this owner: `VapourOpen` is μ = 1 still air (the vapour-open end of the seam's own `>= 1` admission) because no energy schema declares the factor, and `LayerConductance` is the EN ISO 6946 λ/d unit conductance the seam `Thermal` case stores per `MaterialId` — film-free by construction, since surface resistances belong to the ASSEMBLY U-value its own owner computes and folding them into a ply attributes an assembly property to one layer. Structural-graph legality (endpoints, ids) is the seam's `ElementFault`, IFC-semantic legality the composed `IfcLegality` → `BimFault.ModelRejected`, and this projector re-checks neither; the rooted `NodeId` is LOCAL per raise (Guid-v7), the schema identifier riding `ExternalId` for correlation.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LanguageExt;
using LanguageExt.Traits;
using NodaTime;
using Rasm;
using Rasm.Bim.Model;                        // BimFault + the Detail roster the raise raises through
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Composition;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;
using Df = DragonflySchema;      // boundary-only aliases: the DTO namespaces never escape this file
using Hb = HoneybeeSchema;
using Os = OpenStudio;
// Per-document canonical-list projection the abridged-reference resolve reads — opaque and window lists in one
// dense alias, not a shape.
using EnergyLibrary = (
    LanguageExt.Seq<HoneybeeSchema.OpaqueConstructionAbridged> Constructions,
    LanguageExt.Seq<HoneybeeSchema.EnergyMaterial> Materials,
    LanguageExt.Seq<HoneybeeSchema.WindowConstructionAbridged> WindowConstructions,
    LanguageExt.Seq<HoneybeeSchema.EnergyWindowMaterialGlazing> Glazings);

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// Four countable landings close a raise. Each slot rides the landing entry rather than four sibling increment
// members, so an arm cannot put a node and forget to count it, and a fifth countable shape is one row here.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EnergySlot {
    public static readonly EnergySlot Space = new("space");
    public static readonly EnergySlot Surface = new("surface");
    public static readonly EnergySlot Opening = new("opening");
    public static readonly EnergySlot Construction = new("construction");
}

// --- [TABLES] -----------------------------------------------------------------------------
// ONE primary FaceType<->IfcClass correspondence; the raise map, the OSM surface-type string leg, the OSM
// opening-type leg, and the lower map are frozen indexes DERIVED from it (plus the lower-side overlay of IFC
// subtypes folding onto a face kind), so the directions can never drift. Predefined tokens gate at Emit
// through AdmitPredefined, never here.
static class EnergyClassRows {
    internal static readonly (Hb.FaceType Face, IfcClass Class, string Predefined)[] Rows = [
        (Hb.FaceType.Wall,        IfcClass.Wall,           ""),
        (Hb.FaceType.Floor,       IfcClass.Slab,           "FLOOR"),
        (Hb.FaceType.RoofCeiling, IfcClass.Slab,           "ROOF"),
        (Hb.FaceType.AirBoundary, IfcClass.VirtualElement, ""),
    ];

    // Lower-side widening: IFC classes with no primary row still derive a face kind (one row each).
    internal static readonly (IfcClass Class, Hb.FaceType Face)[] LowerOverlay = [
        (IfcClass.WallStandardCase, Hb.FaceType.Wall),
        (IfcClass.CurtainWall,      Hb.FaceType.Wall),
        (IfcClass.Roof,             Hb.FaceType.RoofCeiling),
    ];

    internal static readonly FrozenDictionary<Hb.FaceType, (IfcClass Class, string Predefined)> ToClass =
        Rows.ToFrozenDictionary(static r => r.Face, static r => (r.Class, r.Predefined));

    // OSM read's string leg — Surface.surfaceType() tokens ARE the FaceType names — derived from the one table.
    // OrdinalIgnoreCase like ByOpeningType: IDF fields are case-insensitive, so a reverse-translated "WALL" is legal.
    internal static readonly FrozenDictionary<string, (IfcClass Class, string Predefined)> BySurfaceType =
        Rows.ToFrozenDictionary(static r => r.Face.ToString(), static r => (r.Class, r.Predefined), StringComparer.OrdinalIgnoreCase);

    // OSM SubSurface.subSurfaceType() tokens onto the opening classes (validSubSurfaceTypeValues roster):
    // glass-bearing tokens fold to IfcWindow, door tokens to IfcDoor — the Compute FixedWindow/Door build inverts it.
    internal static readonly FrozenDictionary<string, IfcClass> ByOpeningType = new KeyValuePair<string, IfcClass>[] {
        new("FixedWindow", IfcClass.Window), new("OperableWindow", IfcClass.Window), new("Skylight", IfcClass.Window),
        new("TubularDaylightDome", IfcClass.Window), new("TubularDaylightDiffuser", IfcClass.Window),
        new("GlassDoor", IfcClass.Door), new("Door", IfcClass.Door), new("OverheadDoor", IfcClass.Door),
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    // The predefined token is an OPTIONAL refinement, exactly as the reconstruction classifier's domain axis is: a
    // `Some(token)` row wins where the token genuinely discriminates — IfcSlab is a FLOOR or a ROOF and nothing
    // else decides — and a `None` row binds every other occurrence of its class. Keying the token as MANDATORY
    // forced every class-level row to spell the empty token, so a wall carrying any real predefined value
    // (STANDARD, POLYGONAL, ELEMENTEDWALL) missed the table and degraded, and the lower emitted an envelope with
    // its walls quietly absent.
    internal static readonly FrozenDictionary<(string Code, Option<PredefinedType> Token), Hb.FaceType> ToFace =
        Rows.Select(static r => (
                (r.Class.Key, r.Predefined.Length > 0 ? Some(PredefinedType.Create(r.Predefined)) : Option<PredefinedType>.None),
                r.Face))
            .Concat(LowerOverlay.Select(static r => ((r.Class.Key, Option<PredefinedType>.None), r.Face)))
            .ToFrozenDictionary(static p => p.Item1, static p => p.Item2);

    // The read is the two-rung ladder the optional axis implies: the token row where the source stamped one, else
    // the class row. One member, so the lower never re-spells the ladder and the two rungs cannot drift apart.
    internal static Option<Hb.FaceType> FaceOf(string code, PredefinedType token) =>
        ToFace.TryGetValue((code, Some(token)), out Hb.FaceType refined) ? Some(refined)
        : ToFace.TryGetValue((code, Option<PredefinedType>.None), out Hb.FaceType classLevel) ? Some(classLevel)
        : None;
}

// The rank-0-rooted spatial chain every arm seeds. An energy document carries a building at best and usually only
// rooms, but the seam graph is read by owners that descend from the CONTEXT root — the spatial view, the COBie
// spatial fold, the results scope resolver — so a raise that landed bare IfcSpace nodes produced a graph whose
// spaces belonged to nothing and which every rank-aware consumer read as empty. The chain is minted ONCE per raise
// and its storeys memoize by authored name, so a document naming three storeys lands three and a document naming
// none lands the single implicit one every space hangs from.
readonly record struct SpatialSpine(NodeId Project, NodeId Site, NodeId Building, Map<string, NodeId> Storeys) {
    // Re-rooting under a fresh building resets the storey memo with it, because storey NAMES are building-local:
    // two towers both naming a "Level 3" carry two levels, and a shared memo silently merged them.
    public SpatialSpine Under(NodeId building) => this with { Building = building, Storeys = Map<string, NodeId>() };
}

// The raise threads three facts, not one: the accumulating delta, the spatial chain its rooms hang from, and the
// zone memo its spaces group into. Three parallel parameters through six signatures was the shape that let an arm
// advance the delta and drop the memo, so the second room naming one zone minted a second zone node.
readonly record struct RaiseScope(RaiseState State, SpatialSpine Spine, Map<string, NodeId> Zones);

// --- [MODELS] -----------------------------------------------------------------------------
// RaiseState carries the raise's WHOLE accumulation through one fold: the delta under construction, analytical
// footprint blobs the caller lands write-blob-first, four landing tallies, and typed degrade rows behind the
// warning count. Instance counters a Fin-returning arm incremented on the side survived a failed fold and told a
// reader how many layers degraded but never which, so every receipt column derives from what the fold returned.
public sealed record RaiseState(
    GraphDelta Delta, Seq<(UInt128 Key, FootprintPolygon Ring)> Footprints,
    int Spaces, int Surfaces, int Openings, int Constructions, Seq<EnergyNote> Notes) {
    public static readonly RaiseState Empty =
        new(GraphDelta.Empty, Seq<(UInt128, FootprintPolygon)>(), 0, 0, 0, 0, Seq<EnergyNote>());

    public static RaiseState Of(GraphDelta seed) => Empty with { Delta = seed };

    public RaiseState Land(EnergySlot slot, Node node) => slot.Switch(
        space:        () => this with { Delta = Delta.Put(node), Spaces = Spaces + 1 },
        surface:      () => this with { Delta = Delta.Put(node), Surfaces = Surfaces + 1 },
        opening:      () => this with { Delta = Delta.Put(node), Openings = Openings + 1 },
        construction: () => this with { Delta = Delta.Put(node), Constructions = Constructions + 1 });

    public RaiseState Put(Node node) => this with { Delta = Delta.Put(node) };
    public RaiseState Link(Relationship edge) => this with { Delta = Delta.Link(edge) };
    public RaiseState Blob(UInt128 key, FootprintPolygon ring) => this with { Footprints = Footprints.Add((key, ring)) };
    public RaiseState Noted(Seq<EnergyNote> notes) => this with { Notes = Notes + notes };
    public RaiseState Note(EnergyReason reason, string subject) => Noted(Seq(new EnergyNote(reason, subject, 1)));

    public EnergyReceipt Receipt(InterchangeFormat form, UInt128 source, Instant at) =>
        new(EnergyLeg.Raised, form, None, Spaces, Surfaces, Openings, Constructions, Notes, source, at);
}

// Resolved answers the composition lookup: a landed layer set (None where the document declares none) beside the
// degrade rows its layer reads produced, so a warning never outlives the set it explains or rides a failed rail.
public readonly record struct Resolved(
    Option<(Node.Material LayerSetNode, Seq<Node.Material> Layers)> Composition, Seq<EnergyNote> Notes) {
    public static readonly Resolved Absent = new(Option<(Node.Material, Seq<Node.Material>)>.None, Seq<EnergyNote>());
}

// --- [SERVICES] ---------------------------------------------------------------------------
// Energy-model PRIMARY projector: the raw document captured internally, five arms converging on the seam shape the
// Compute energy runner reads. The fold threads ONE RaiseState and the run edge commits it to the boundary cell
// once, so the footprint write-blob-first side-channel and the receipt both read what the fold returned.
public sealed class EnergyProjector(EnergyDoc doc) : IElementProjection {
    readonly Atom<RaiseState> run = Atom(RaiseState.Empty);

    public Seq<(UInt128 Key, FootprintPolygon Ring)> Footprints => run.Value.Footprints;

    public EnergyReceipt Receipt(Instant at) => run.Value.Receipt(doc.Format, doc.SourceKey, at);

    static readonly FrozenDictionary<InterchangeFormat, Func<EnergyProjector, ProjectionContext, Fin<RaiseState>>> Arms =
        new KeyValuePair<InterchangeFormat, Func<EnergyProjector, ProjectionContext, Fin<RaiseState>>>[] {
            new(InterchangeFormat.Hbjson, static (p, ctx) => p.Honeybee(ctx)),
            new(InterchangeFormat.Dfjson, static (p, ctx) => p.Dragonfly(ctx)),
            new(InterchangeFormat.Osm,    static (p, ctx) => p.OsmFamily(ctx)),
            new(InterchangeFormat.GbXml,  static (p, ctx) => p.OsmFamily(ctx)),
            new(InterchangeFormat.Idf,    static (p, ctx) => p.OsmFamily(ctx)),
        }.ToFrozenDictionary();

    internal static bool Serves(InterchangeFormat format) => Arms.ContainsKey(format);

    // CONSTRUCTION is admission: a projector exists only for a form some arm serves, so no caller holds an
    // instance that claims to project a document it cannot read. The retired shape constructed first and asked
    // Serves after, which put the capability answer beside an object that had already asserted it.
    public static Fin<EnergyProjector> Of(EnergyDoc doc, Op key) =>
        Serves(doc.Format)
            ? Fin.Succ(new EnergyProjector(doc))
            : Fin.Fail<EnergyProjector>(Detail.EnergyFormMiss.At(key, doc.Format.Key));

    // Seam contract returns the delta alone, so the run edge commits the threaded state in ONE swap — the
    // per-run thread carries the fold, the cell carries it across to the reads the exchange makes afterwards.
    // The arm lookup is TOTAL past Of, so its miss arm is the type system's floor rather than a live branch.
    public Fin<GraphDelta> Project(ProjectionContext ctx) =>
        Arms.TryGetValue(doc.Format, out var arm)
            ? arm(this, ctx).Bind(state => Envelope(state, ctx)).Map(state => run.Swap(_ => state).Delta)
            : Fin.Fail<GraphDelta>(Detail.EnergyFormMiss.At(ctx.Key, doc.Format.Key));

    // A class miss DEGRADES per surface, so one unmapped face never costs the building; the raise faults only
    // where the WHOLE envelope failed to survive. That split is the difference between "this model has an odd
    // ceiling" and "this model has no envelope at all", and the retired first-miss fault reported the second for
    // every instance of the first.
    static Fin<RaiseState> Envelope(RaiseState state, ProjectionContext ctx) =>
        state.Surfaces > 0
            ? Fin.Succ(state)
            : Fin.Fail<RaiseState>(Detail.EnergyEnvelopeEmpty.At(ctx.Key, state.Spaces.ToString(CultureInfo.InvariantCulture)));

    // --- [HONEYBEE_ARM]
    // FromJson gates HARD by construction: the LBT-Newtonsoft parse throw AND the DataAnnotations reject (the
    // in-FromJson IsValid(throwException: true) ArgumentException) land in the funnel, a "type"-mismatch
    // null-parse faults explicitly. Post-admission Validate() is structurally empty, so the raise tallies no
    // schema warnings — the Validate() tally belongs to the LOWER legs authoring models locally.
    Fin<RaiseState> Honeybee(ProjectionContext ctx) =>
        Try.lift(() => Hb.Model.FromJson(doc.Text)).Run()
            .MapFail(error => (Error)Detail.EnergyDecode.At(ctx.Key, error.Message))
            .Bind(model => model is null
                ? Fin.Fail<RaiseState>(Detail.EnergyDecode.At(ctx.Key, "type-mismatch"))
                : RaiseRooms(Seeded(ctx, model.Identifier), Library(model), toSeq(model.Rooms ?? []), ctx)
                    .Map(static scope => scope.State));

    // Every arm opens on the same seeded scope, so no arm can forget the chain its rooms need.
    static RaiseScope Seeded(ProjectionContext ctx, string identifier) {
        (RaiseState state, SpatialSpine spine) = Root(RaiseState.Of(GraphDelta.Empty.Reheader(ctx.Header)), identifier);
        return new RaiseScope(state, spine, Map<string, NodeId>());
    }

    // Model-level canonical lists projected ONCE per document — the abridged-reference resolve source, opaque AND
    // window rows. ONE body serves both schemas: the dragonfly store is its OWN type but its lists are
    // HoneybeeSchema.AnyOf rows (DragonflySchema ships no AnyOf of its own), so both stores enter covariantly as
    // IEnumerable<Hb.AnyOf> and Building.Room3ds route through the identical room fold with no second resolve path.
    // The store is a DOCUMENT-level fact, so the projection runs once per document and every building, storey, and
    // room reads the same lists. Folding it inside the per-building loop re-projected the whole library once per
    // building — four Choose passes over every construction and material the model carries, per building.
    static EnergyLibrary Library(Hb.Model model) =>
        Library(model.Properties?.Energy?.Constructions, model.Properties?.Energy?.Materials);

    static EnergyLibrary Library(Df.Model model) =>
        Library(model.Properties?.Energy?.Constructions, model.Properties?.Energy?.Materials);

    static EnergyLibrary Library(IEnumerable<Hb.AnyOf>? constructions, IEnumerable<Hb.AnyOf>? materials) => (
        toSeq(constructions ?? []).Choose(static any => any.Obj is Hb.OpaqueConstructionAbridged oc ? Some(oc) : None),
        toSeq(materials ?? []).Choose(static any => any.Obj is Hb.EnergyMaterial m ? Some(m) : None),
        toSeq(constructions ?? []).Choose(static any => any.Obj is Hb.WindowConstructionAbridged wc ? Some(wc) : None),
        toSeq(materials ?? []).Choose(static any => any.Obj is Hb.EnergyWindowMaterialGlazing g ? Some(g) : None));

    Fin<RaiseScope> RaiseRooms(RaiseScope scope, EnergyLibrary library, Seq<Hb.Room> rooms, ProjectionContext ctx) =>
        rooms.Fold(Fin.Succ(scope), (acc, room) => acc.Bind(s => RaiseRoom(s, library, room, ctx)));

    // ONE room fold serves both managed arms: an HBJSON room and a dragonfly Building.Room3d are the same shape,
    // and both carry the storey, the zone, and the repeat multiplier the schema states. Reading only the faces
    // dropped three facts the dragonfly massing arm was already reading off its own storey — so one document
    // round-tripped its levels and multipliers and the other silently flattened them.
    Fin<RaiseScope> RaiseRoom(RaiseScope scope, EnergyLibrary library, Hb.Room room, ProjectionContext ctx) {
        NodeId spaceId = NodeId.Rooted();
        (RaiseState levelled, SpatialSpine spine, NodeId storey) =
            Storey(scope.State, scope.Spine, room.Story is { Length: > 0 } named ? named : ImplicitStorey);
        RaiseState landed = levelled
            .Land(EnergySlot.Space, Element(spaceId, IfcClass.Space, "", room.Identifier))
            .Link(new Relationship.Compose(storey, spaceId, ComposeKind.Contain));
        (RaiseState grouped, Map<string, NodeId> zones) = Zoned(landed, scope.Zones, spaceId, Optional(room.Zone));
        Fin<RaiseState> seeded = room.Multiplier > 1
            ? MultiplierEvidence(room.Multiplier, ctx.Header.Tolerance).Map(evidence => Assigned(grouped, spaceId, evidence))
            : Fin.Succ(grouped);
        return seeded
            .Bind(state => toSeq(room.Faces ?? []).Fold(
                Fin.Succ(state), (acc, face) => acc.Bind(s => RaiseFace(s, library, spaceId, face, ctx))))
            .Map(state => scope with { State = state, Spine = spine, Zones = zones });
    }

    // The one implicit level every document that names no storey hangs from — a NAMED policy value, so a reader
    // tells an unlevelled source from a source that authored a level called nothing.
    const string ImplicitStorey = "level";

    // A face mints the surface Object + FootPrint representation key + the SpaceBoundary Generic edge the
    // Compute reads consume; the apertures/doors fold as ONE opening-row pass (identifier, ring, abridged
    // construction id, class) so a raised window carries the Optical evidence the Compute SubSurface build
    // reads; the face construction associates the seam layer composition. BoundaryLevel is "2nd" — a honeybee
    // face IS the per-space bounded surface. The schema Type discriminator admits ONCE into the closed BoundaryRow
    // vocabulary here (never a downcast chain, never raw text the lower re-parses), so the payload the graph
    // carries is already the row both schemas project from.
    Fin<RaiseState> RaiseFace(RaiseState state, EnergyLibrary library, NodeId spaceId, Hb.Face face, ProjectionContext ctx) {
        // An unmapped face type is a DEGRADE, not a fault: the surface drops with a typed row naming the token,
        // and the envelope keeps every surface that did map. Faulting the whole raise on the first miss let one
        // unrecognized ceiling kind reject a building whose walls, floors, and windows had all resolved.
        if (!EnergyClassRows.ToClass.TryGetValue(face.FaceType, out var row)) {
            return Fin.Succ(state.Note(EnergyReason.ClassUnmapped, face.Identifier));
        }
        NodeId surfaceId = NodeId.Rooted();
        (RaiseState blobbed, UInt128 footprint) = Footprint(state,
            Ring(face.Geometry.Boundary.Count, index => Point(face.Geometry.Boundary[index], 0.0)), ctx.Header.Tolerance);
        RaiseState seed = blobbed
            .Land(EnergySlot.Surface, Element(surfaceId, row.Class, row.Predefined, face.Identifier, footprint))
            .Link(Boundary(spaceId, surfaceId,
                BoundaryRow.Admit(face.BoundaryCondition.Obj is Hb.OpenAPIGenBaseModel bc ? bc.Type : null)));
        Seq<(string Identifier, List<List<double>> Ring, string? Construction, IfcClass Class)> rows =
            toSeq(face.Apertures ?? []).Map(a => (a.Identifier, a.Geometry.Boundary, a.Properties?.Energy?.Construction, IfcClass.Window))
            + toSeq(face.Doors ?? []).Map(d => (d.Identifier, d.Geometry.Boundary, d.Properties?.Energy?.Construction, IfcClass.Door));
        return rows
            .Fold(Fin.Succ(seed), (acc, o) => acc.Bind(s => {
                (RaiseState opened, NodeId openingId) = Opening(s, spaceId, face.Identifier, o.Class, o.Identifier,
                    Ring(o.Ring.Count, index => Point(o.Ring[index], 0.0)), ctx);
                return Composition(library, o.Construction, ctx).Map(set => Associate(opened, openingId, set));
            }))
            .Bind(s => Composition(library, face.Properties?.Energy?.Construction, ctx).Map(set => Associate(s, surfaceId, set)));
    }

    // Abridged-reference resolve, opaque AND window: the construction id resolves in the opaque list
    // (EnergyMaterial rows -> Thermal) or the window list (EnergyWindowMaterialGlazing rows -> Optical) — one
    // resolve serves faces and openings; a dangling id faults, a NoMass/Vegetation/gas layer degrades to a warning.
    Fin<Resolved> Composition(EnergyLibrary library, string? construction, ProjectionContext ctx) =>
        Optional(construction).Match(
            None: () => Fin.Succ(Resolved.Absent),
            Some: id => library.Constructions.Find(oc => oc.Identifier == id).Match(
                Some: oc => Resolve(id, toSeq(oc.Materials), mid => SeamMaterial(library, mid, ctx), ctx),
                None: () => library.WindowConstructions.Find(wc => wc.Identifier == id)
                    .ToFin(Detail.EnergyConstructionAbsent.At(ctx.Key, id))
                    .Bind(wc => Resolve(id, toSeq(wc.Materials), mid => SeamGlazing(library, mid, ctx), ctx))));

    // Material ids -> per-material Single nodes + ONE LayerSet Material node the surface/opening associates
    // (the seam composition shape graph.CompositionOf resolves) — one fold parameterized by the material arm,
    // never an opaque copy beside a glazing copy. A layer the arm resolves in neither library list notes against
    // its OWN id, so the degrade names the missing material where an anonymous counter named nothing.
    Fin<Resolved> Resolve(
        string id, Seq<string> materialIds,
        Func<string, Validation<Error, Option<(Node.Material Node, MaterialLayer Layer)>>> arm, ProjectionContext ctx) =>
        materialIds.Traverse(arm).As().ToFin()
            .Bind(rows => rows.Somes() is { IsEmpty: false } typed
                ? MaterialComposition.OfLayerSet(typed.Map(static r => r.Layer), ctx.Key)
                    .Map(set => new Resolved(
                        Some((Mint(id, set, Seq<MaterialPropertySet>(), ctx.Header.Tolerance), typed.Map(static r => r.Node))),
                        Unresolved(materialIds, rows)))
                : Fin.Succ(new Resolved(Option<(Node.Material, Seq<Node.Material>)>.None, Unresolved(materialIds, rows))));

    static Seq<EnergyNote> Unresolved(Seq<string> materialIds, Seq<Option<(Node.Material Node, MaterialLayer Layer)>> rows) =>
        materialIds.Zip(rows)
            .Filter(static pair => pair.Item2.IsNone)
            .Map(static pair => new EnergyNote(EnergyReason.LayerUnresolved, pair.Item1, 1));

    Validation<Error, Option<(Node.Material Node, MaterialLayer Layer)>> SeamMaterial(EnergyLibrary library, string materialId, ProjectionContext ctx) =>
        library.Materials
            .Find(m => m.Identifier == materialId)
            .Match(
                None: () => Success<Error, Option<(Node.Material, MaterialLayer)>>(None),
                Some: m => MaterialPropertySet
                    .OfThermal(m.Conductivity, m.SpecificHeat, LayerConductance(m.Conductivity, m.Thickness), VapourOpen, ctx.Key)
                    .Bind(thermal => MeasureValue.OfSi(Dimension.LengthDim, m.Thickness).Map(thickness => (thermal, thickness)))
                    .ToValidation()
                    .Map(pair => Some((
                        Mint(m.Identifier, MaterialComposition.OfSingle(MaterialId.Create(m.Identifier)), Seq(pair.thermal), ctx.Header.Tolerance),
                        new MaterialLayer(MaterialId.Create(m.Identifier), pair.thickness, m.Identifier)))));

    // Glazing counterpart: EnergyWindowMaterialGlazing -> the seam Optical nine-fraction case ONLY — an OfThermal
    // with fabricated specific heat is the rejected fabricated-physics form (Compute's glazing build needs no Thermal).
    Validation<Error, Option<(Node.Material Node, MaterialLayer Layer)>> SeamGlazing(EnergyLibrary library, string materialId, ProjectionContext ctx) =>
        library.Glazings
            .Find(g => g.Identifier == materialId)
            .Match(
                None: () => Success<Error, Option<(Node.Material, MaterialLayer)>>(None),
                Some: g => MaterialPropertySet
                    .OfOptical(
                        g.VisibleTransmittance, g.VisibleReflectance, Alt(g.VisibleReflectanceBack, g.VisibleReflectance),
                        g.SolarTransmittance, g.SolarReflectance, Alt(g.SolarReflectanceBack, g.SolarReflectance),
                        g.InfraredTransmittance, g.Emissivity, g.EmissivityBack, ctx.Key)
                    .Bind(optical => MeasureValue.OfSi(Dimension.LengthDim, g.Thickness).Map(thickness => (optical, thickness)))
                    .ToValidation()
                    .Map(pair => Some((
                        Mint(g.Identifier, MaterialComposition.OfSingle(MaterialId.Create(g.Identifier)), Seq(pair.optical), ctx.Header.Tolerance),
                        new MaterialLayer(MaterialId.Create(g.Identifier), pair.thickness, g.Identifier)))));

    // AnyOf<Autocalculate,double> back-reflectance sentinel resolves to the front value — the honeybee
    // autocalculate semantic, never a zero read.
    static double Alt(Hb.AnyOf<Hb.Autocalculate, double> value, double front) => value?.Obj is double d ? d : front;

    // Content-keyed Material mint: probe id overwritten by the canonical-bytes content hash so identical
    // materials and layer sets dedup across raises (the composition-page mint law).
    static Node.Material Mint(string identifier, MaterialComposition composition, Seq<MaterialPropertySet> properties, double tolerance) {
        Node.Material probe = new(NodeId.Content([]), MaterialId.Create(identifier), composition, properties);
        return new(NodeId.Content(probe.ToCanonicalBytes(tolerance).Span), MaterialId.Create(identifier), composition, properties);
    }

    // --- [DRAGONFLY_ARM]
    // Massing raise: Building/Story/Room2D lower onto an IfcBuilding/IfcBuildingStorey/IfcSpace Compose tree
    // with the floor plate content-keyed as the space FootPrint, the Room2D floor-to-ceiling height landed
    // as the Qto_SpaceBaseQuantities Height quantity the derive reads back, and Story.Multiplier stamped as
    // Import-source bag evidence; Building.Room3ds (full honeybee rooms) route through the SAME honeybee room
    // fold under the building host — dragonfly composes the honeybee vocabulary, never re-mints it.
    Fin<RaiseState> Dragonfly(ProjectionContext ctx) =>
        Try.lift(() => Df.Model.FromJson(doc.Text)).Run()
            .MapFail(error => (Error)Detail.EnergyDecode.At(ctx.Key, error.Message))
            .Bind(model => model is null
                ? Fin.Fail<RaiseState>(Detail.EnergyDecode.At(ctx.Key, "type-mismatch"))
                // The library projects ONCE per document, above the building fold, so N buildings read one store.
                : Library(model) is var library
                    ? toSeq(model.Buildings ?? []).Fold(
                            Fin.Succ(Seeded(ctx, model.Identifier)),
                            (acc, building) => acc.Bind(s => RaiseBuilding(s, library, building, ctx)))
                        .Map(static scope => scope.State)
                    : Fin.Fail<RaiseState>(Detail.EnergyDecode.At(ctx.Key, "library")));

    Fin<RaiseScope> RaiseBuilding(RaiseScope scope, EnergyLibrary library, Df.Building building, ProjectionContext ctx) {
        // Each DFJSON building re-roots the chain under the shared site, and re-rooting resets the storey memo so
        // two towers naming one level carry two levels.
        NodeId buildingId = NodeId.Rooted();
        SpatialSpine spine = scope.Spine.Under(buildingId);
        RaiseState seed = scope.State
            .Put(Element(buildingId, IfcClass.Building, "", building.Identifier))
            .Link(new Relationship.Compose(scope.Spine.Site, buildingId, ComposeKind.Aggregate));
        Fin<RaiseScope> massing = toSeq(building.UniqueStories ?? []).Fold(
            Fin.Succ(scope with { State = seed, Spine = spine }),
            (acc, story) => acc.Bind(held => RaiseStory(held, story, ctx)));
        return toSeq(building.Room3ds ?? []).Fold(massing,
            (acc, room) => acc.Bind(held => RaiseRoom(held, library, room, ctx)));
    }

    // One dragonfly storey and its plates. The storey node routes through the SHARED memo, so a Room3d naming the
    // same storey by name joins the level the massing plates already landed rather than minting a parallel one.
    Fin<RaiseScope> RaiseStory(RaiseScope scope, Df.Story story, ProjectionContext ctx) {
        (RaiseState levelled, SpatialSpine spine, NodeId storeyId) = Storey(scope.State, scope.Spine, story.Identifier);
        Fin<RaiseState> seeded = story.Multiplier > 1
            ? MultiplierEvidence(story.Multiplier, ctx.Header.Tolerance).Map(evidence => Assigned(levelled, storeyId, evidence))
            : Fin.Succ(levelled);
        return toSeq(story.Room2ds ?? []).Fold(
            seeded.Map(state => scope with { State = state, Spine = spine }),
            (acc, room) => acc.Bind(held => RaisePlate(held, storeyId, room, ctx)));
    }

    Fin<RaiseScope> RaisePlate(RaiseScope scope, NodeId storeyId, Df.Room2D room, ProjectionContext ctx) {
        NodeId spaceId = NodeId.Rooted();
        (RaiseState blobbed, UInt128 plate) = Footprint(scope.State,
            Ring(room.FloorBoundary.Count, index => Point(room.FloorBoundary[index], room.FloorHeight)),
            ctx.Header.Tolerance);
        RaiseState landed = blobbed
            .Land(EnergySlot.Space, Element(spaceId, IfcClass.Space, "", room.Identifier, plate))
            .Link(new Relationship.Compose(storeyId, spaceId, ComposeKind.Contain));
        (RaiseState grouped, Map<string, NodeId> zones) = Zoned(landed, scope.Zones, spaceId, Optional(room.Zone));
        return HeightQuantity(room.FloorToCeilingHeight, ctx.Header.Tolerance)
            .Map(height => scope with { State = Assigned(grouped, spaceId, height), Zones = zones });
    }

    // Dragonfly space height landed as the SAME Qto_SpaceBaseQuantities Height quantity the derive's massing
    // lower reads back — a DFJSON round trip that fell to the 3.0 m policy default was the deleted round-trip hole.
    // Fin: a non-finite source height rails the seam OfSi finite gate rather than entering the canonical bytes.
    static Fin<Node.QuantitySet> HeightQuantity(double floorToCeiling, double tolerance) =>
        MeasureValue.OfSi(Dimension.LengthDim, floorToCeiling).Map(height => {
            QuantityBag bag = new(QuantityRows.SpaceBaseQuantities,
                Map((QuantityRows.Height, height)),
                InheritanceMode.OccurrenceWins, PropertySource.Import);
            Node.QuantitySet probe = new(NodeId.Content([]), bag);
            return new Node.QuantitySet(NodeId.Content(probe.ToCanonicalBytes(tolerance).Span), bag);
        });

    // Story.Multiplier is SOURCE data (unique stories x vertical repeat) — dropped, the derive re-emits
    // multiplier-1 stories and the energy model under-counts by the repeat factor; read back onto Story(multiplier:).
    static Fin<Node.PropertySet> MultiplierEvidence(int multiplier, double tolerance) =>
        MeasureValue.OfSi(Dimension.Dimensionless, multiplier).Map(value => {
            PropertyBag bag = new(EnergyModelSet,
                Map((StoryMultiplier, (PropertyValue)new PropertyValue.Measure(value))),
                InheritanceMode.OccurrenceWins, PropertySource.Import);
            Node.PropertySet probe = new(NodeId.Content([]), bag);
            return new Node.PropertySet(NodeId.Content(probe.ToCanonicalBytes(tolerance).Span), bag);
        });

    // --- [OSM_ARM]
    // Three decode arms, one raise fold. loadModelFromString upgrades any older .osm in-string; gbXML/IDF
    // readers are Path-bound, crossed via a bracketed temp path (Exemption: SWIG + filesystem boundary). Catch
    // spans decode AND raise: every SWIG member on the fold path can throw natively, and a raise escaping the Fin
    // signature is the exception-control-flow defect the funnel closes.
    Fin<RaiseState> OsmFamily(ProjectionContext ctx) {
        try {
            return Decode(ctx).Bind(model => { using (model) { return RaiseOsm(model, ctx); } });
        }
        catch (Exception ex) when (ex is SystemException or ApplicationException) {
            return Fin.Fail<RaiseState>(Detail.EnergyDecode.At(ctx.Key, ex.Message));
        }
    }

    Fin<Os.Model> Decode(ProjectionContext ctx) {
        if (doc.Format == InterchangeFormat.Osm) {
            using Os.VersionTranslator vt = new();
            using Os.OptionalModel osm = vt.loadModelFromString(doc.Text);
            return Lowered(osm, ctx, "osm");
        }
        string temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try {
            File.WriteAllBytes(temp, doc.Bytes.ToArray());
            using Os.Path path = Os.OpenStudioUtilitiesCore.toPath(temp);
            if (doc.Format == InterchangeFormat.GbXml) {
                using Os.GbXMLReverseTranslator gb = new();
                using Os.OptionalModel fromGb = gb.loadModel(path);
                return Lowered(fromGb, ctx, "gbxml");
            }
            using Os.EnergyPlusReverseTranslator ep = new();
            using Os.OptionalModel fromIdf = ep.loadModel(path);
            return Lowered(fromIdf, ctx, "idf");
        }
        finally { File.Delete(temp); }
    }

    static Fin<Os.Model> Lowered(Os.OptionalModel optional, ProjectionContext ctx, string arm) =>
        optional.is_initialized()
            ? Fin.Succ(optional.get())
            : Fin.Fail<Os.Model>(Detail.EnergyDecode.At(ctx.Key, arm, "unreadable"));

    // OSM raise: spaces/surfaces/boundary edges land the same seam shape as the honeybee arm, and each
    // surface's SubSurfaces land as Host-attributed openings; a typed layer set resolves via the handle re-read
    // (the only SWIG downcast), else the assembly uFactor lands as Pset_EnergyModel bag evidence + a warning.
    // Index-loop + per-element using = the SWIG marshaling exemption.
    Fin<RaiseState> RaiseOsm(Os.Model model, ProjectionContext ctx) {
        RaiseScope scope = Seeded(ctx, model.nameString());
        RaiseState state = scope.State;
        SpatialSpine spine = scope.Spine;
        Map<string, NodeId> zones = scope.Zones;
        using Os.SpaceVector osSpaces = model.getSpaces();
        for (int i = 0; i < osSpaces.Count; i++) {
            using Os.Space osSpace = osSpaces[i];
            NodeId spaceId = NodeId.Rooted();
            // The OSM storey and thermal zone are BOTH authored on the space, and both are optional handles the
            // SWIG optional gates — so an unlevelled space hangs from the implicit level and an unzoned space
            // joins no grouping, which is what the source stated.
            using Os.OptionalBuildingStory osStorey = osSpace.buildingStory();
            string level = osStorey.is_initialized() ? Named(osStorey) : ImplicitStorey;
            (state, spine, NodeId storeyId) = Storey(state, spine, level);
            state = state
                .Land(EnergySlot.Space, Element(spaceId, IfcClass.Space, "", osSpace.nameString()))
                .Link(new Relationship.Compose(storeyId, spaceId, ComposeKind.Contain));
            using Os.OptionalThermalZone osZone = osSpace.thermalZone();
            (state, zones) = Zoned(state, zones, spaceId, osZone.is_initialized() ? Some(Named(osZone)) : None);
            using Os.SurfaceVector surfs = osSpace.surfaces;
            for (int j = 0; j < surfs.Count; j++) {
                using Os.Surface surf = surfs[j];
                string kind = surf.surfaceType();
                // An out-of-roster surface type DEGRADES with the base surface, exactly as an unmapped honeybee
                // face type does: one odd surface never rejects a building whose envelope otherwise resolved, and
                // the empty-envelope gate at the arm's close is what catches a model that resolved nothing.
                if (!EnergyClassRows.BySurfaceType.TryGetValue(kind, out var row)) {
                    state = state.Note(EnergyReason.ClassUnmapped, surf.nameString());
                    continue;
                }
                NodeId surfaceId = NodeId.Rooted();
                using Os.Point3dVector vertices = surf.vertices();
                (RaiseState blobbed, UInt128 footprint) = Footprint(state, OsmRing(vertices), ctx.Header.Tolerance);
                state = blobbed
                    .Land(EnergySlot.Surface, Element(surfaceId, row.Class, row.Predefined, surf.nameString(), footprint))
                    .Link(Boundary(spaceId, surfaceId, BoundaryRow.Admit(surf.outsideBoundaryCondition())));
                state = OsmComposition(model, surf, surfaceId, state, ctx);
                state = OsmSubSurfaces(model, surf, spaceId, state, ctx);
            }
        }
        return Fin.Succ(state);
    }

    // A named SWIG handle read and released in one expression — the marshaling exemption, spelled once for both
    // the storey and the zone rather than twice at their read sites.
    static string Named(Os.OptionalBuildingStory optional) { using Os.BuildingStory storey = optional.get(); return storey.nameString(); }

    static string Named(Os.OptionalThermalZone optional) { using Os.ThermalZone zone = optional.get(); return zone.nameString(); }

    // OSM opening fold: Surface.subSurfaces() land as IfcWindow/IfcDoor Objects joined to the SPACE by the
    // same Host-attributed boundary edge the honeybee arm mints — an OSM raised without its fenestration
    // simulates an unglazed building, the deleted coverage hole; each opening's own construction resolves
    // through the ONE PlanarSurface composition fold. An out-of-roster subSurfaceType degrades warning-counted
    // where a base-surface class miss FAULTS — deliberate: an opening is envelope refinement, a base surface
    // envelope structure (all eight validSubSurfaceTypeValues are mapped, so a miss is an invalid file token).
    RaiseState OsmSubSurfaces(Os.Model model, Os.Surface surf, NodeId spaceId, RaiseState state, ProjectionContext ctx) {
        using Os.SubSurfaceVector subs = surf.subSurfaces();
        string host = surf.nameString();
        for (int i = 0; i < subs.Count; i++) {
            using Os.SubSurface sub = subs[i];
            if (!EnergyClassRows.ByOpeningType.TryGetValue(sub.subSurfaceType(), out IfcClass @class)) {
                state = state.Note(EnergyReason.OpeningTypeMiss, sub.nameString());
                continue;
            }
            using Os.Point3dVector vertices = sub.vertices();
            (state, NodeId openingId) = Opening(state, spaceId, host, @class, sub.nameString(), OsmRing(vertices), ctx);
            state = OsmComposition(model, sub, openingId, state, ctx);
        }
        return state;
    }

    // Each SWIG vector element owns a native handle, so this lift disposes as it reads (the marshaling
    // exemption) — ring ASSEMBLY itself is the one shared projection every schema's coordinate list crosses.
    static FootprintPolygon OsmRing(Os.Point3dVector vertices) =>
        Ring(vertices.Count, index => { using Os.Point3d p = vertices[index]; return new Vector3(p.x(), p.y(), p.z()); });

    // ONE composition fold over the PlanarSurface construction — a base Surface's opaque wall and a SubSurface's
    // glazing resolve through the SAME layer read; a layer neither typed re-read resolves degrades the WHOLE set
    // to the assembly uFactor evidence + a warning, never a fabricated layer set.
    RaiseState OsmComposition(Os.Model model, Os.PlanarSurface surf, NodeId nodeId, RaiseState state, ProjectionContext ctx) {
        using Os.OptionalConstructionBase cb = surf.construction();
        if (!cb.is_initialized()) { return state; }
        using Os.ConstructionBase constructionBase = cb.get();
        using Os.OptionalConstruction layered = model.getConstruction(constructionBase.handle());
        if (!layered.is_initialized()) { return state.Note(EnergyReason.LayerUnresolved, constructionBase.nameString()); }
        using Os.Construction construction = layered.get();
        using Os.MaterialVector layers = construction.layers();
        var rows = Seq<(Node.Material Node, MaterialLayer Layer)>();
        for (int i = 0; i < layers.Count; i++) {
            using Os.Material element = layers[i];
            if (TypedLayer(model, element, ctx).Case is not (Node.Material node, MaterialLayer layer)) {
                return UFactorEvidence(state.Note(EnergyReason.LayerUnresolved, element.nameString()), nodeId, constructionBase, ctx);
            }
            rows = rows.Add((node, layer));
        }
        return MaterialComposition.OfLayerSet(rows.Map(static r => r.Layer), ctx.Key).Match(
            Succ: set => Associate(state, nodeId, new Resolved(
                Some((Mint(constructionBase.nameString(), set, Seq<MaterialPropertySet>(), ctx.Header.Tolerance), rows.Map(static r => r.Node))),
                Seq<EnergyNote>())),
            Fail: _ => UFactorEvidence(state.Note(EnergyReason.CompositionMixed, constructionBase.nameString()), nodeId, constructionBase, ctx));
    }

    // ONE typed-layer read serving both re-reads: the opaque row (StandardOpaqueMaterial -> Thermal) and the
    // glazing row (StandardGlazing -> Optical) — the SWIG vector element is statically Material, so the handle
    // re-read is the ONLY typed downcast; all SIX normal-incidence fractions read the OptionalDouble getters (the
    // plain solarTransmittance() sibling THROWS over the same unset IDD field), an unset optional degrading the layer.
    Option<(Node.Material Node, MaterialLayer Layer)> TypedLayer(Os.Model model, Os.Material element, ProjectionContext ctx) {
        using Os.OptionalStandardOpaqueMaterial opaque = model.getStandardOpaqueMaterial(element.handle());
        if (opaque.is_initialized()) {
            using Os.StandardOpaqueMaterial m = opaque.get();
            return MaterialPropertySet
                .OfThermal(m.conductivity(), m.specificHeat(), LayerConductance(m.conductivity(), m.thickness()), VapourOpen, ctx.Key)
                .Bind(thermal => MeasureValue.OfSi(Dimension.LengthDim, m.thickness()).Map(thickness => (
                    Mint(m.nameString(), MaterialComposition.OfSingle(MaterialId.Create(m.nameString())), Seq(thermal), ctx.Header.Tolerance),
                    new MaterialLayer(MaterialId.Create(m.nameString()), thickness, m.nameString()))))
                .ToOption();
        }
        using Os.OptionalStandardGlazing glass = model.getStandardGlazing(element.handle());
        if (!glass.is_initialized()) { return None; }
        using Os.StandardGlazing g = glass.get();
        return (Opt(g.visibleTransmittanceatNormalIncidence()),
                Opt(g.frontSideVisibleReflectanceatNormalIncidence()), Opt(g.backSideVisibleReflectanceatNormalIncidence()),
                Opt(g.solarTransmittanceatNormalIncidence()),
                Opt(g.frontSideSolarReflectanceatNormalIncidence()), Opt(g.backSideSolarReflectanceatNormalIncidence()))
            .Apply((vt, rvf, rvb, st, rsf, rsb) => MaterialPropertySet.OfOptical(
                vt, rvf, rvb, st, rsf, rsb,
                g.infraredTransmittanceatNormalIncidence(), g.frontSideInfraredHemisphericalEmissivity(), g.backSideInfraredHemisphericalEmissivity(), ctx.Key))
            .As().Bind(static fin => fin.ToOption())
            .Bind(optical => MeasureValue.OfSi(Dimension.LengthDim, g.thickness()).ToOption().Map(thickness => (
                Mint(g.nameString(), MaterialComposition.OfSingle(MaterialId.Create(g.nameString())), Seq(optical), ctx.Header.Tolerance),
                new MaterialLayer(MaterialId.Create(g.nameString()), thickness, g.nameString()))));
    }

    // Read a SWIG OptionalDouble onto the K-KINDED Option slot and DISPOSE the native handle (the getter's optional
    // is itself disposable) — the one lowering a missing OSM field takes, never a faulting get(). K<Option, double>,
    // NOT Option<double>: the shipped tuple Apply binds only on (K<F,A>, …) receivers — no concrete-carrier tuple
    // overload exists — and a concrete Option tuple neither infers nor converts at the receiver.
    static K<Option, double> Opt(Os.OptionalDouble optional) { using (optional) { return optional.is_initialized() ? Some(optional.get()) : None; } }

    // --- [SPATIAL_SPINE]
    // Root seeds project -> site -> building at rank 0, each an Aggregate step, so every arm's rooms descend from
    // a real context root. The identifiers are the document's own where it names them and derived from the raise
    // otherwise — a synthetic name is honest here because the RANK is the fact consumers read, not the label.
    static (RaiseState State, SpatialSpine Spine) Root(RaiseState state, string identifier) {
        NodeId project = NodeId.Rooted(), site = NodeId.Rooted(), building = NodeId.Rooted();
        RaiseState seeded = state
            .Put(Element(project, IfcClass.Project, "", identifier))
            .Put(Element(site, IfcClass.Site, "", identifier))
            .Put(Element(building, IfcClass.Building, "", identifier))
            .Link(new Relationship.Compose(project, site, ComposeKind.Aggregate))
            .Link(new Relationship.Compose(site, building, ComposeKind.Aggregate));
        return (seeded, new SpatialSpine(project, site, building, Map<string, NodeId>()));
    }

    // Storeys memoize by their AUTHORED name, so N rooms on one storey share one node and a document naming no
    // storey lands exactly one implicit level rather than one per room.
    static (RaiseState State, SpatialSpine Spine, NodeId Storey) Storey(RaiseState state, SpatialSpine spine, string name) =>
        spine.Storeys.Find(name).Match(
            Some: held => (state, spine, held),
            None: () => {
                NodeId storey = NodeId.Rooted();
                RaiseState landed = state
                    .Put(Element(storey, IfcClass.BuildingStorey, "", name))
                    .Link(new Relationship.Compose(spine.Building, storey, ComposeKind.Aggregate));
                return (landed, spine with { Storeys = spine.Storeys.Add(name, storey) }, storey);
            });

    // A THERMAL ZONE is the grouping every energy schema carries and the seam already models: OSM names it on
    // Space.thermalZone(), DFJSON on Room2D.Zone, HBJSON on Room.Zone. Raising the zone as a grouping node with a
    // membership edge per space composes the Model/zones#ZONE_GRAPH vocabulary the results admission already
    // resolves per-zone rows against — so a simulated zone load lands on the node the raise minted rather than on
    // nothing. Zones memoize by name exactly as storeys do; a room naming no zone joins none, because a fabricated
    // one-space-one-zone grouping asserts a conditioning topology the source never stated.
    static (RaiseState State, Map<string, NodeId> Zones) Zoned(
        RaiseState state, Map<string, NodeId> zones, NodeId space, Option<string> name) =>
        name.Filter(static z => z.Length > 0).Match(
            None: () => (state, zones),
            Some: zone => zones.Find(zone).Match(
                Some: held => (state.Link(new Relationship.Assign(space, held, AssignKind.Group)), zones),
                None: () => {
                    NodeId minted = NodeId.Rooted();
                    return (state
                        .Put(Element(minted, IfcClass.Zone, "", zone))
                        .Link(new Relationship.Assign(space, minted, AssignKind.Group)),
                        zones.Add(zone, minted));
                }));

    // --- [SHARED_MINTS]
    // Energy boundary payload keys: BoundaryLevel and Host are the Rasm.Element BoundaryRows statics — the seam
    // declarer's ONE cross-package symbol per space-boundary row, the Rasm.Compute energy build reading Host back
    // — while BoundaryCondition/StoryMultiplier/UFactor/ConstructionName are energy-raise-owned, each minted ONCE
    // through the owner-blessed empty-prefix PropertyCategory.Seam.Row (the Properties/property#DETAIL_SCHEMA
    // custody law, the wire name staying bare) and the lower reads the same statics back.
    internal static readonly PropertyName BoundaryCondition = PropertyCategory.Seam.Row("BoundaryCondition");
    internal static readonly PropertyName StoryMultiplier = PropertyCategory.Seam.Row("StoryMultiplier");
    internal static readonly PropertyName UFactor = PropertyCategory.Seam.Row("UFactor");
    internal static readonly PropertyName ConstructionName = PropertyCategory.Seam.Row("ConstructionName");

    // One evidence BAG name crosses the raise and the lower beside the seam-declared Qto set: the property set
    // carrying the storey multiplier and the uFactor degrade. Re-spelling it at one end drops that round trip
    // silently, because a set-name mismatch reads as an absent bag; the space-height quantity set composes
    // QuantityRows.SpaceBaseQuantities at both ends for the same reason.
    internal const string EnergyModelSet = "Pset_EnergyModel";

    // No energy schema declares a vapour-resistance factor, so every raised layer floors at μ = 1 — still air, the
    // vapour-OPEN end of the seam's own `>= 1` admission. Fabricating a resistance would assert a Glaser
    // condensation result the source document never stated.
    internal const double VapourOpen = 1.0;

    // LayerConductance derives λ/d (EN ISO 6946) — the FILM-FREE per-ply quantity both the honeybee
    // EnergyMaterial and the OSM StandardOpaqueMaterial declare their inputs for, and what the seam Thermal case
    // stores per MaterialId. Surface resistances belong to the ASSEMBLY U-value its own owner computes, and
    // folding them in here attributes an assembly property to one ply. A zero-thickness layer yields a non-finite
    // conductance the seam's finite gate rejects, degrading that layer rather than inventing a number for it.
    static double LayerConductance(double conductivity, double thickness) => conductivity / thickness;

    static Node.Object Element(NodeId id, IfcClass @class, string predefined, string identifier, UInt128 footprint = default) =>
        new(Id: id, Kind: ObjectKind.Occurrence,
            ExternalId: Optional(identifier).Filter(static s => s.Length > 0),
            Classification: Classification.Create("ifc", @class.Key, "", None, None, None),
            PredefinedType: PredefinedType.Create(predefined),
            Name: identifier, Tag: "",
            Representations: footprint == default
                ? RepresentationContentHash.Empty
                : RepresentationContentHash.Empty.With("FootPrint", footprint),
            History: None, Span: @class.Span);   // the roster row's own schema span, the SemanticProjector mint law

    // Each edge carries the ADMITTED row's own key, never the raw schema token: the raise resolves the foreign
    // vocabulary once through BoundaryRow and the lower reads that same roster back, so no string middle sits
    // between two parsers that disagree about what "Adiabatic" means.
    static Relationship Boundary(NodeId space, NodeId surface, BoundaryRow condition) =>
        new Relationship.Generic(IfcRelKind.SpaceBoundary.Key, space, surface, Map(
            (BoundaryRows.Level,  (PropertyValue)new PropertyValue.Text("2nd")),
            (BoundaryCondition,   (PropertyValue)new PropertyValue.Text(condition.Key))));

    // An aperture/door IS a space boundary in energy modeling (the IfcRelSpaceBoundary related element may be a
    // window/door), so the opening joins the SPACE by the same edge shape with a Host correlation attribute — the
    // typed Void/fill lowering demands an IfcOpeningElement intermediary no energy schema carries, the rejected form.
    // Returns the minted id so the caller associates the opening's own glazing composition.
    static (RaiseState State, NodeId Id) Opening(RaiseState state, NodeId space, string hostIdentifier, IfcClass @class, string identifier, FootprintPolygon ring, ProjectionContext ctx) {
        NodeId openingId = NodeId.Rooted();
        (RaiseState blobbed, UInt128 footprint) = Footprint(state, ring, ctx.Header.Tolerance);
        return (blobbed
            .Land(EnergySlot.Opening, Element(openingId, @class, "", identifier, footprint))
            .Link(new Relationship.Generic(IfcRelKind.SpaceBoundary.Key, space, openingId, Map(
                (BoundaryRows.Level, (PropertyValue)new PropertyValue.Text("2nd")),
                (BoundaryRows.Host,  (PropertyValue)new PropertyValue.Text(hostIdentifier))))), openingId);
    }

    // One composition landing: layer nodes, the LayerSet node, the Associate edge, the construction tally on the
    // Some arm, and the resolve's own degrade rows on BOTH arms — the per-arm Match copy is the deleted form, and
    // notes ride the same landing so a dropped layer is recorded whether or not a set survived it.
    static RaiseState Associate(RaiseState state, NodeId subject, Resolved resolved) =>
        resolved.Composition.Match(
            Some: s => s.Layers
                .Fold(state.Noted(resolved.Notes).Land(EnergySlot.Construction, s.LayerSetNode), static (acc, n) => acc.Put(n))
                .Link(new Relationship.Associate(subject, s.LayerSetNode.Id, new MaterialUsage.None())),
            None: () => state.Noted(resolved.Notes));

    // Content-keyed bag node + Assign(PropertyDefinition) — the one evidence-landing shape shared by the uFactor
    // degrade, the dragonfly height quantity, and the storey-multiplier row.
    static RaiseState Assigned(RaiseState state, NodeId subject, Node node) =>
        state.Put(node).Link(new Relationship.Assign(subject, node.Id, AssignKind.PropertyDefinition));

    // ONE foreign-point-list -> FootprintPolygon projection: an index count and a per-index lift are the whole
    // variance between an HBJSON coordinate array, a dragonfly floor boundary at its storey height, and an OSM
    // Point3dVector whose elements own native handles. Three sibling ring builders were three chances to assemble
    // one ring under a different vertex order and content-key the same surface twice.
    static FootprintPolygon Ring(int count, Func<int, Vector3> at) =>
        new(toSeq(Enumerable.Range(0, count)).Map(at));

    // Both managed schemas write a coordinate as [x, y] or [x, y, z]; a 2D pair lifts at the plane height its
    // caller owns (a dragonfly plate rides its storey, a honeybee ring already carries its own z).
    static Vector3 Point(List<double> coordinate, double plane) =>
        new(coordinate[0], coordinate[1], coordinate.Count > 2 ? coordinate[2] : plane);

    // Footprint blob key rides the seam analytical byte projection (the ONE CanonicalWriter layout the
    // SemanticProjector mint owns — a page-local byte order is the cross-projector divergence defect), the ring
    // recorded ON the threaded state for the caller's write-blob-first landing.
    static (RaiseState State, UInt128 Key) Footprint(RaiseState state, FootprintPolygon ring, double tolerance) {
        UInt128 key = ContentHash.Of(ring.Ring
            .Fold(new CanonicalWriter(tolerance), static (w, p) => w.Double(p.X).Double(p.Y).Double(p.Z))
            .ToBytes().Span);
        return (state.Blob(key, ring), key);
    }

    // Degrade row: no typed layer read -> the assembly uFactor lands as bag evidence the lower/review reads,
    // never a fabricated layer set (PropertySource.Derived — computed evidence, not authored data).
    static RaiseState UFactorEvidence(RaiseState state, NodeId surfaceId, Os.ConstructionBase construction, ProjectionContext ctx) {
        using Os.OptionalDouble u = construction.uFactor();
        if (!u.is_initialized()) { return state; }
        return MeasureValue.Of(u.get(), UnitsNet.Units.HeatTransferCoefficientUnit.WattPerSquareMeterKelvin, ctx.Key).Match(
            Fail: _ => state.Note(EnergyReason.MeasureRejected, construction.nameString()),
            Succ: uValue => {
                PropertyBag bag = new(EnergyModelSet, Map(
                    (UFactor,          (PropertyValue)new PropertyValue.Measure(uValue)),
                    (ConstructionName, (PropertyValue)new PropertyValue.Text(construction.nameString()))),
                    InheritanceMode.OccurrenceWins, PropertySource.Derived);
                Node.PropertySet probe = new(NodeId.Content([]), bag);
                return Assigned(state, surfaceId, new Node.PropertySet(NodeId.Content(probe.ToCanonicalBytes(ctx.Header.Tolerance).Span), bag));
            });
    }
}
```

## [03]-[RESEARCH]

(none)
