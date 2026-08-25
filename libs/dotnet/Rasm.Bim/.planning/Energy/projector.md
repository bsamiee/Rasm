# [BIM_ENERGY_PROJECTOR]

`EnergyProjector : IElementProjection` mints the energy-model raise, second Bim projector beside `Projection/semantic#SEMANTIC_PROJECTOR` on the `Exchange/reconstruct#RECONSTRUCTION` precedent: five decode arms — HBJSON/DFJSON managed, OSM/gbXML/IDF SWIG — converge on ONE projection the `Rasm.Compute/Analysis/energy` runner simulates with zero adapter.

`Energy/exchange#ENERGY_EXCHANGE` `EnergyExchange.Apply` `Raise` drives it under `ProjectionAssembly.Assemble` with `IfcLegality` composed; `Energy/derive#MODEL_DERIVE` lowers the same class table, boundary statics, and evidence bags back, so raise and lower never drift.

Every arm lands one Compute-readable shape: `IfcSpace`-classified `Object` nodes, bounding-surface Objects joined by `IfcRelSpaceBoundary`-named neutral `Generic` edges carrying the `BoundaryLevel` `"2nd"` payload, `Host`-attributed opening boundaries for every window and door, footprints content-keyed into `Representations.FootPrint`, and `MaterialComposition.LayerSet` evidence — opaque layers `MaterialPropertySet.Thermal`, glazing the `Optical` case the Compute `StandardGlazing` build reads; wire names, `Qto`, and `Pset` are the load-bearing alignment.

Wire posture is HOST-LOCAL, foreign types decode-confined: `Model.FromJson` gates hard by construction (parse throw, in-parse DataAnnotations), no `HoneybeeSchema.*`/`DragonflySchema.*` DTO outlives `Project`, and every `OpenStudio.*` SWIG wrapper — model, translators, `Optional*`, `*Vector`, per-element handles — is `using`-bracketed, index-loop with per-element disposal the marshaling exemption. Faults route the `Model/faults#FAULT_BAND` arms: `Refused/BimReason.Rejected` (`energy-decode`), `Refused/BimReason.Unmapped` (`energy-face-miss`/`energy-class-miss`), `Refused/BimReason.DanglingReference` (`energy-construction-absent`).

## [01]-[INDEX]

- [02]-[ENERGY_PROJECTOR]: `EnergyProjector : IElementProjection` — five format arms onto one raise fold landing the Compute-readable seam shape from the `EnergyClassRows` correspondence table.

## [02]-[ENERGY_PROJECTOR]

- Owner: `EnergyProjector : IElementProjection` the energy-model raise (the raw `EnergyDoc` captured internally, the seam contract carrying only `Node`/`Relationship`/`GraphDelta`); `EnergyClassRows` the ONE FaceType↔`IfcClass` correspondence table both directions derive from — the raise map, the OSM `Surface.surfaceType()` string leg, the OSM `SubSurface.subSurfaceType()` opening leg, and the lower-side `ToFace` inverse; the frozen `Arms` format→raise index the `Serves` capability predicate and dispatch share; `RaiseState` the whole threaded accumulation (delta, footprint blobs, landing tallies, degrade rows) with `EnergySlot` its landing vocabulary and `Resolved` the composition answer that carries its own degrade rows.
- Entry: `EnergyProjector.Project(ProjectionContext ctx)` → `Fin<GraphDelta>` — the frozen `Arms` index dispatches the captured document's format onto its arm, `Hbjson`/`Dfjson` decoding through `Op.Catch` and the named JSON boundaries and `Osm`/`GbXml`/`Idf` through the bracketed SWIG decode trio converging on ONE `RaiseOsm` fold; an unmapped format faults `energy-form-miss`.
- Auto: openings mint `IfcWindow`/`IfcDoor` on EVERY arm, honeybee `Aperture`/`Door` and OSM `Surface.subSurfaces()` alike, never only the managed formats; the dragonfly massing arm lands each `Room2D` floor-to-ceiling height as a `Qto_SpaceBaseQuantities` `Height` quantity, stamps `Story.Multiplier > 1` as `Pset_EnergyModel` evidence, and routes `Building.Room3ds` through the same honeybee room fold.
- Output: `RaiseState` threads `Spaces`/`Surfaces`/`Openings`/`Constructions` and the typed `Energy/exchange#ENERGY_EXCHANGE` `EnergyNote` degrade rows through the fold, and the run edge commits it to the boundary cell in ONE swap the `Energy/exchange#ENERGY_EXCHANGE` `EnergyCensus` and the footprint side-channel both read — instance counters a `Fin`-returning arm bumped on the side survived a failed fold and named no subject. Managed decodes reject inside `FromJson`, so the raise notes degrade rows only — `Validate()` annotations belong to the lower legs authoring models locally.
- Packages: HoneybeeSchema, DragonflySchema, NREL.OpenStudio.macOS-arm64, Rasm.Element, Rasm (the kernel `Op.Catch` funnel and cause-preserving `BimBoundary` fault posture), LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions
- Growth: a new face/class correspondence is one `EnergyClassRows` row (both directions derive); a new OSM opening token is one `ByOpeningType` row; a new energy form is one `Arms` row carrying its raise arm; a dragonfly parameter (window ratios, shading, skylights) deepens the massing arm as row folds over the `Room2D` `AnyOf` unions; a NoMass/Vegetation material arm is one typed-layer row the moment the seam carries an R-value-only thermal case; the FULL (non-abridged) `OpaqueConstruction`/`WindowConstruction` store rows — inline material OBJECTS, not id references, so a different resolve shape — are one `Library` projection widening with one inline-material arm the moment full-form documents ship, a full-form construction id resolving in neither abridged list faulting `Refused/BimReason.DanglingReference` before that (the declared abridged-only restraint, never a silent partial read); honeybee `Shade`/`ShadeMesh` context geometry raises as one arm row the moment an `IfcShadingDevice` roster row is exercised by a consumer read.
- Boundary: projector dispatch publishes no recovery policy. Every native region crosses ONE kernel `Op.Catch` funnel; the documented energy boundaries become `BoundaryFailed` with their original `Error` and immutable posture, while returned typed errors and unknown foreign errors pass through unchanged. `EnergyMaterial` density has NO seam thermal column and a fabricated `OfMechanical` stiffness is the rejected form — density is DROPPED at the raise (systematic, never a per-material warning), the OSM rebuild's 1000 kg/m³ fallback carrying the consequence. Every physics literal is a NAMED policy value on this owner: `VapourOpen` is μ = 1 still air (the vapour-open end of the seam's own `>= 1` admission) because no energy schema declares the factor, and `LayerConductance` is the EN ISO 6946 λ/d unit conductance the seam `Thermal` case stores per `MaterialId` — film-free by construction, since surface resistances belong to the ASSEMBLY U-value its own owner computes and folding them into a ply attributes an assembly property to one layer. Structural-graph legality (endpoints, ids) is the seam's `ElementFault`, IFC-semantic legality the composed `IfcLegality` → `BimFault.Refused` with `BimReason.Rejected`, and this projector re-checks neither; the rooted `NodeId` is LOCAL per raise (Guid-v7), the schema identifier riding `ExternalId` for correlation.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LanguageExt;
using LanguageExt.Traits;
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
using EnergyLibrary = (
    LanguageExt.Seq<HoneybeeSchema.OpaqueConstructionAbridged> Constructions,
    LanguageExt.Seq<HoneybeeSchema.EnergyMaterial> Materials,
    LanguageExt.Seq<HoneybeeSchema.WindowConstructionAbridged> WindowConstructions,
    LanguageExt.Seq<HoneybeeSchema.EnergyWindowMaterialGlazing> Glazings);

namespace Rasm.Bim;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EnergySlot {
    public static readonly EnergySlot Space = new("space");
    public static readonly EnergySlot Surface = new("surface");
    public static readonly EnergySlot Opening = new("opening");
    public static readonly EnergySlot Construction = new("construction");
}

// --- [TABLES] --------------------------------------------------------------------------
static class EnergyClassRows {
    internal static readonly (Hb.FaceType Face, IfcClass Class, string Predefined)[] Rows = [
        (Hb.FaceType.Wall,        IfcClass.Wall,           ""),
        (Hb.FaceType.Floor,       IfcClass.Slab,           "FLOOR"),
        (Hb.FaceType.RoofCeiling, IfcClass.Slab,           "ROOF"),
        (Hb.FaceType.AirBoundary, IfcClass.VirtualElement, ""),
    ];

    internal static readonly (IfcClass Class, Hb.FaceType Face)[] LowerOverlay = [
        (IfcClass.WallStandardCase, Hb.FaceType.Wall),
        (IfcClass.CurtainWall,      Hb.FaceType.Wall),
        (IfcClass.Roof,             Hb.FaceType.RoofCeiling),
    ];

    internal static readonly FrozenDictionary<Hb.FaceType, (IfcClass Class, string Predefined)> ToClass =
        Rows.ToFrozenDictionary(static r => r.Face, static r => (r.Class, r.Predefined));

    internal static readonly FrozenDictionary<string, (IfcClass Class, string Predefined)> BySurfaceType =
        Rows.ToFrozenDictionary(static r => r.Face.ToString(), static r => (r.Class, r.Predefined), StringComparer.OrdinalIgnoreCase);

    internal static readonly FrozenDictionary<string, IfcClass> ByOpeningType = new KeyValuePair<string, IfcClass>[] {
        new("FixedWindow", IfcClass.Window), new("OperableWindow", IfcClass.Window), new("Skylight", IfcClass.Window),
        new("TubularDaylightDome", IfcClass.Window), new("TubularDaylightDiffuser", IfcClass.Window),
        new("GlassDoor", IfcClass.Door), new("Door", IfcClass.Door), new("OverheadDoor", IfcClass.Door),
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    internal static readonly FrozenDictionary<(string Code, Option<PredefinedType> Token), Hb.FaceType> ToFace =
        Rows.Select(static r => (
                (r.Class.Key, r.Predefined.Length > 0 ? Some(PredefinedType.Create(r.Predefined)) : Option<PredefinedType>.None),
                r.Face))
            .Concat(LowerOverlay.Select(static r => ((r.Class.Key, Option<PredefinedType>.None), r.Face)))
            .ToFrozenDictionary(static p => p.Item1, static p => p.Item2);

    internal static Option<Hb.FaceType> FaceOf(string code, PredefinedType token) =>
        ToFace.TryGetValue((code, Some(token)), out Hb.FaceType refined) ? Some(refined)
        : ToFace.TryGetValue((code, Option<PredefinedType>.None), out Hb.FaceType classLevel) ? Some(classLevel)
        : None;
}

readonly record struct SpatialSpine(NodeId Project, NodeId Site, NodeId Building, Map<string, NodeId> Storeys) {
    public SpatialSpine Under(NodeId building) => this with { Building = building, Storeys = Map<string, NodeId>() };
}

readonly record struct RaiseScope(RaiseState State, SpatialSpine Spine, Map<string, NodeId> Zones);

// --- [MODELS] --------------------------------------------------------------------------
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

    public EnergyCensus Census(InterchangeFormat form, UInt128 source, Instant at) =>
        new(EnergyLeg.Raised, form, None, Spaces, Surfaces, Openings, Constructions, Notes, source, at);
}

public readonly record struct Resolved(
    Option<(Node.Material LayerSetNode, Seq<Node.Material> Layers)> Composition, Seq<EnergyNote> Notes) {
    public static readonly Resolved Absent = new(Option<(Node.Material, Seq<Node.Material>)>.None, Seq<EnergyNote>());
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class EnergyProjector(EnergyDoc doc) : IElementProjection {
    readonly Atom<RaiseState> run = Atom(RaiseState.Empty);

    public Seq<(UInt128 Key, FootprintPolygon Ring)> Footprints => run.Value.Footprints;

    public EnergyCensus Census(Instant at) => run.Value.Census(doc.Format, doc.SourceKey, at);

    static readonly FrozenDictionary<InterchangeFormat, Func<EnergyProjector, ProjectionContext, Fin<RaiseState>>> Arms =
        new KeyValuePair<InterchangeFormat, Func<EnergyProjector, ProjectionContext, Fin<RaiseState>>>[] {
            new(InterchangeFormat.Hbjson, static (p, ctx) => p.Honeybee(ctx)),
            new(InterchangeFormat.Dfjson, static (p, ctx) => p.Dragonfly(ctx)),
            new(InterchangeFormat.Osm,    static (p, ctx) => p.OsmFamily(ctx)),
            new(InterchangeFormat.GbXml,  static (p, ctx) => p.OsmFamily(ctx)),
            new(InterchangeFormat.Idf,    static (p, ctx) => p.OsmFamily(ctx)),
        }.ToFrozenDictionary();

    internal static bool Serves(InterchangeFormat format) => Arms.ContainsKey(format);

    public static Fin<EnergyProjector> Of(EnergyDoc doc, Op key) =>
        Serves(doc.Format)
            ? Fin.Succ(new EnergyProjector(doc))
            : Fin.Fail<EnergyProjector>(new BimFault.Refused(key, BimScope.Energy, BimReason.Codec, string.Join(':', new object?[] { "energy-form-miss", doc.Format.Key })));

    public Fin<GraphDelta> Project(ProjectionContext ctx) =>
        Arms.TryGetValue(doc.Format, out var arm)
            ? arm(this, ctx).Bind(state => Envelope(state, ctx)).Map(state => run.Swap(_ => state).Delta)
            : Fin.Fail<GraphDelta>(new BimFault.Refused(ctx.Key, BimScope.Energy, BimReason.Codec, string.Join(':', new object?[] { "energy-form-miss", doc.Format.Key })));

    static Fin<RaiseState> Envelope(RaiseState state, ProjectionContext ctx) =>
        state.Surfaces > 0
            ? Fin.Succ(state)
            : Fin.Fail<RaiseState>(new BimFault.Refused(ctx.Key, BimScope.Energy, BimReason.Rejected, string.Join(':', new object?[] { "energy-envelope-empty", state.Spaces.ToString(CultureInfo.InvariantCulture) })));

    // --- [HONEYBEE_ARM]
    Fin<RaiseState> Honeybee(ProjectionContext ctx) =>
        ctx.Key.Catch(
                () => Fin.Succ(Hb.Model.FromJson(doc.Text)),
                cause => JsonFailure(BimBoundary.HoneybeeJson, cause))
            .Bind(model => model is null
                ? Fin.Fail<RaiseState>(new BimFault.Refused(ctx.Key, BimScope.Energy, BimReason.Rejected, "energy-decode:type-mismatch"))
                : RaiseRooms(Seeded(ctx, model.Identifier), Library(model), Rows(model.Rooms), ctx)
                    .Map(static scope => scope.State));

    static RaiseScope Seeded(ProjectionContext ctx, string identifier) {
        (RaiseState state, SpatialSpine spine) = Root(RaiseState.Of(GraphDelta.Empty.Reheader(ctx.Header)), identifier);
        return new RaiseScope(state, spine, Map<string, NodeId>());
    }

    static EnergyLibrary Library(Hb.Model model) =>
        Library(model.Properties?.Energy?.Constructions, model.Properties?.Energy?.Materials);

    static EnergyLibrary Library(Df.Model model) =>
        Library(model.Properties?.Energy?.Constructions, model.Properties?.Energy?.Materials);

    static EnergyLibrary Library(IEnumerable<Hb.AnyOf>? constructions, IEnumerable<Hb.AnyOf>? materials) {
        Seq<Hb.AnyOf> cons = Rows(constructions), mats = Rows(materials);
        return (
            cons.Choose(static any => any.Obj is Hb.OpaqueConstructionAbridged oc ? Some(oc) : None),
            mats.Choose(static any => any.Obj is Hb.EnergyMaterial m ? Some(m) : None),
            cons.Choose(static any => any.Obj is Hb.WindowConstructionAbridged wc ? Some(wc) : None),
            mats.Choose(static any => any.Obj is Hb.EnergyWindowMaterialGlazing g ? Some(g) : None));
    }

    static Seq<T> Rows<T>(IEnumerable<T>? values) => values is null ? Seq<T>() : toSeq(values);

    Fin<RaiseScope> RaiseRooms(RaiseScope scope, EnergyLibrary library, Seq<Hb.Room> rooms, ProjectionContext ctx) =>
        rooms.Fold(Fin.Succ(scope), (acc, room) => acc.Bind(s => RaiseRoom(s, library, room, ctx)));

    Fin<RaiseScope> RaiseRoom(RaiseScope scope, EnergyLibrary library, Hb.Room room, ProjectionContext ctx) {
        NodeId spaceId = NodeId.Of(new NodeSeed.Placement());
        (RaiseState levelled, SpatialSpine spine, NodeId storey) =
            Storey(scope.State, scope.Spine, room.Story is { Length: > 0 } named ? named : ImplicitStorey);
        RaiseState landed = levelled
            .Land(EnergySlot.Space, Element(spaceId, IfcClass.Space, "", room.Identifier))
            .Link(new Relationship.Compose(storey, spaceId, ComposeKind.Contain));
        (RaiseState grouped, Map<string, NodeId> zones) = Zoned(landed, scope.Zones, spaceId, Optional(room.Zone));
        Fin<RaiseState> seeded = room.Multiplier > 1
            ? MultiplierEvidence(room.Multiplier, ctx.Header.Tolerance, ctx.Key).Map(evidence => Assigned(grouped, spaceId, evidence))
            : Fin.Succ(grouped);
        return seeded
            .Bind(state => Rows(room.Faces).Fold(
                Fin.Succ(state), (acc, face) => acc.Bind(s => RaiseFace(s, library, spaceId, face, ctx))))
            .Map(state => scope with { State = state, Spine = spine, Zones = zones });
    }

    const string ImplicitStorey = "level";

    Fin<RaiseState> RaiseFace(RaiseState state, EnergyLibrary library, NodeId spaceId, Hb.Face face, ProjectionContext ctx) {
        if (!EnergyClassRows.ToClass.TryGetValue(face.FaceType, out var row)) {
            return Fin.Succ(state.Note(EnergyReason.ClassUnmapped, face.Identifier));
        }
        NodeId surfaceId = NodeId.Of(new NodeSeed.Placement());
        (RaiseState blobbed, UInt128 footprint) = Footprint(state,
            Ring(face.Geometry.Boundary.Count, index => Point(face.Geometry.Boundary[index], 0.0)), ctx.Header.Tolerance);
        RaiseState seed = blobbed
            .Land(EnergySlot.Surface, Element(surfaceId, row.Class, row.Predefined, face.Identifier, footprint))
            .Link(Boundary(spaceId, surfaceId,
                BoundaryRow.Admit(face.BoundaryCondition.Obj is Hb.OpenAPIGenBaseModel bc ? bc.Type : null)));
        Seq<(string Identifier, List<List<double>> Ring, string? Construction, IfcClass Class)> rows =
            Rows(face.Apertures).Map(a => (a.Identifier, a.Geometry.Boundary, a.Properties?.Energy?.Construction, IfcClass.Window))
            + Rows(face.Doors).Map(d => (d.Identifier, d.Geometry.Boundary, d.Properties?.Energy?.Construction, IfcClass.Door));
        return rows
            .Fold(Fin.Succ(seed), (acc, o) => acc.Bind(s => {
                (RaiseState opened, NodeId openingId) = Opening(s, spaceId, face.Identifier, o.Class, o.Identifier,
                    Ring(o.Ring.Count, index => Point(o.Ring[index], 0.0)), ctx);
                return Composition(library, o.Construction, ctx).Map(set => Associate(opened, openingId, set));
            }))
            .Bind(s => Composition(library, face.Properties?.Energy?.Construction, ctx).Map(set => Associate(s, surfaceId, set)));
    }

    Fin<Resolved> Composition(EnergyLibrary library, string? construction, ProjectionContext ctx) =>
        Optional(construction).Match(
            None: () => Fin.Succ(Resolved.Absent),
            Some: id => library.Constructions.Find(oc => oc.Identifier == id).Match(
                Some: oc => Resolve(id, toSeq(oc.Materials), mid => SeamMaterial(library, mid, ctx), ctx),
                None: () => library.WindowConstructions.Find(wc => wc.Identifier == id)
                    .ToFin(new BimFault.Refused(ctx.Key, BimScope.Energy, BimReason.DanglingReference, string.Join(':', new object?[] { "energy-construction-absent", id })))
                    .Bind(wc => Resolve(id, toSeq(wc.Materials), mid => SeamGlazing(library, mid, ctx), ctx))));

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
                    .Bind(thermal => MeasureValue.OfSi(Dimension.LengthDim, m.Thickness, ctx.Key).Map(thickness => (thermal, thickness)))
                    .ToValidation()
                    .Map(pair => Some((
                        Mint(m.Identifier, MaterialComposition.OfSingle(MaterialId.Create(m.Identifier)), Seq(pair.thermal), ctx.Header.Tolerance),
                        new MaterialLayer(MaterialId.Create(m.Identifier), pair.thickness, m.Identifier)))));

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
                    .Bind(optical => MeasureValue.OfSi(Dimension.LengthDim, g.Thickness, ctx.Key).Map(thickness => (optical, thickness)))
                    .ToValidation()
                    .Map(pair => Some((
                        Mint(g.Identifier, MaterialComposition.OfSingle(MaterialId.Create(g.Identifier)), Seq(pair.optical), ctx.Header.Tolerance),
                        new MaterialLayer(MaterialId.Create(g.Identifier), pair.thickness, g.Identifier)))));

    static double Alt(Hb.AnyOf<Hb.Autocalculate, double> value, double front) => value?.Obj is double d ? d : front;

    static Node.Material Mint(string identifier, MaterialComposition composition, Seq<MaterialPropertySet> properties, double tolerance) {
        Node.Material probe = new(NodeId.Of(new NodeSeed.Placement()), MaterialId.Create(identifier), composition, properties);
        return new(NodeId.Of(new NodeSeed.Content(probe, tolerance)), MaterialId.Create(identifier), composition, properties);
    }

    // --- [DRAGONFLY_ARM]
    Fin<RaiseState> Dragonfly(ProjectionContext ctx) =>
        ctx.Key.Catch(
                () => Fin.Succ(Df.Model.FromJson(doc.Text)),
                cause => JsonFailure(BimBoundary.DragonflyJson, cause))
            .Bind(model => model is null
                ? Fin.Fail<RaiseState>(new BimFault.Refused(ctx.Key, BimScope.Energy, BimReason.Rejected, "energy-decode:type-mismatch"))
                : Library(model) is var library
                    ? Rows(model.Buildings).Fold(
                            Fin.Succ(Seeded(ctx, model.Identifier)),
                            (acc, building) => acc.Bind(s => RaiseBuilding(s, library, building, ctx)))
                        .Map(static scope => scope.State)
                    : Fin.Fail<RaiseState>(new BimFault.Refused(ctx.Key, BimScope.Energy, BimReason.Rejected, "energy-decode:library")));

    Fin<RaiseScope> RaiseBuilding(RaiseScope scope, EnergyLibrary library, Df.Building building, ProjectionContext ctx) {
        NodeId buildingId = NodeId.Of(new NodeSeed.Placement());
        SpatialSpine spine = scope.Spine.Under(buildingId);
        RaiseState seed = scope.State
            .Put(Element(buildingId, IfcClass.Building, "", building.Identifier))
            .Link(new Relationship.Compose(scope.Spine.Site, buildingId, ComposeKind.Aggregate));
        Fin<RaiseScope> massing = Rows(building.UniqueStories).Fold(
            Fin.Succ(scope with { State = seed, Spine = spine }),
            (acc, story) => acc.Bind(held => RaiseStory(held, story, ctx)));
        return Rows(building.Room3ds).Fold(massing,
            (acc, room) => acc.Bind(held => RaiseRoom(held, library, room, ctx)));
    }

    Fin<RaiseScope> RaiseStory(RaiseScope scope, Df.Story story, ProjectionContext ctx) {
        (RaiseState levelled, SpatialSpine spine, NodeId storeyId) = Storey(scope.State, scope.Spine, story.Identifier);
        Fin<RaiseState> seeded = story.Multiplier > 1
            ? MultiplierEvidence(story.Multiplier, ctx.Header.Tolerance, ctx.Key).Map(evidence => Assigned(levelled, storeyId, evidence))
            : Fin.Succ(levelled);
        return Rows(story.Room2ds).Fold(
            seeded.Map(state => scope with { State = state, Spine = spine }),
            (acc, room) => acc.Bind(held => RaisePlate(held, storeyId, room, ctx)));
    }

    Fin<RaiseScope> RaisePlate(RaiseScope scope, NodeId storeyId, Df.Room2D room, ProjectionContext ctx) {
        NodeId spaceId = NodeId.Of(new NodeSeed.Placement());
        (RaiseState blobbed, UInt128 plate) = Footprint(scope.State,
            Ring(room.FloorBoundary.Count, index => Point(room.FloorBoundary[index], room.FloorHeight)),
            ctx.Header.Tolerance);
        RaiseState landed = blobbed
            .Land(EnergySlot.Space, Element(spaceId, IfcClass.Space, "", room.Identifier, plate))
            .Link(new Relationship.Compose(storeyId, spaceId, ComposeKind.Contain));
        (RaiseState grouped, Map<string, NodeId> zones) = Zoned(landed, scope.Zones, spaceId, Optional(room.Zone));
        return HeightQuantity(room.FloorToCeilingHeight, ctx.Header.Tolerance, ctx.Key)
            .Map(height => scope with { State = Assigned(grouped, spaceId, height), Zones = zones });
    }

    static Fin<Node.QuantitySet> HeightQuantity(double floorToCeiling, double tolerance, Op key) =>
        MeasureValue.OfSi(Dimension.LengthDim, floorToCeiling, key).Map(height => {
            QuantityBag bag = new(QuantityRows.SpaceBaseQuantities,
                Map((QuantityRows.Height, height)),
                InheritanceMode.OccurrenceWins, EvidenceGrade.Import);
            Node.QuantitySet probe = new(NodeId.Of(new NodeSeed.Placement()), bag);
            return new Node.QuantitySet(NodeId.Of(new NodeSeed.Content(probe, tolerance)), bag);
        });

    static Fin<Node.PropertySet> MultiplierEvidence(int multiplier, double tolerance, Op key) =>
        MeasureValue.OfSi(Dimension.Dimensionless, multiplier, key).Map(value => {
            PropertyBag bag = new(EnergyModelSet,
                Map((StoryMultiplier, (PropertyValue)new PropertyValue.Measure(value))),
                InheritanceMode.OccurrenceWins, EvidenceGrade.Import);
            Node.PropertySet probe = new(NodeId.Of(new NodeSeed.Placement()), bag);
            return new Node.PropertySet(NodeId.Of(new NodeSeed.Content(probe, tolerance)), bag);
        });

    // --- [OSM_ARM]
    static Option<BimFault.BoundaryFailed> JsonFailure(BimBoundary boundary, Error cause) =>
        cause.Exception.Case is Newtonsoft.Json.JsonException or ArgumentException
            ? Some(new BimFault.BoundaryFailed(boundary, cause))
            : None;

    static Option<BimFault.BoundaryFailed> NativeFailure(BimBoundary boundary, Error cause) =>
        cause.Exception.Case switch {
            IOException _ when boundary == BimBoundary.HostScratchWrite => Some(new BimFault.BoundaryFailed(boundary, cause)),
            UnauthorizedAccessException _ when boundary == BimBoundary.HostScratchWrite => Some(new BimFault.BoundaryFailed(boundary, cause)),
            ApplicationException _ when boundary != BimBoundary.HostScratchWrite => Some(new BimFault.BoundaryFailed(boundary, cause)),
            _ => None,
        };

    static Fin<T> Native<T>(Op key, BimBoundary boundary, Func<Fin<T>> leg) =>
        key.Catch(leg, cause => NativeFailure(boundary, cause));

    Fin<RaiseState> OsmFamily(ProjectionContext ctx) =>
        Decode(ctx, model => Native(ctx.Key, BimBoundary.OpenStudioRaise, () => RaiseOsm(model, ctx)));

    Fin<T> Decode<T>(ProjectionContext ctx, Func<Os.Model, Fin<T>> use) =>
        doc.Format == InterchangeFormat.Osm
            ? Native(ctx.Key, BimBoundary.OpenStudioInMemoryDecode, () => {
                using Os.VersionTranslator vt = new();
                using Os.OptionalModel osm = vt.loadModelFromString(doc.Text);
                return Lowered(osm, ctx, "osm", use);
            })
            : Scratched(ctx, use);

    Fin<T> Scratched<T>(ProjectionContext ctx, Func<Os.Model, Fin<T>> use) =>
        IO.lift(() => Native(ctx.Key, BimBoundary.HostScratchWrite, () => {
                string temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                return Fin.Succ(temp);
            }))
            .Bracket(
                Use: temp => IO.lift(() => Native(ctx.Key, BimBoundary.HostScratchWrite, () => {
                        File.WriteAllBytes(temp, doc.Bytes.ToArray());
                        return Fin.Succ(unit);
                    })
                    .Bind(_ => Native(ctx.Key, BimBoundary.OpenStudioPathDecode, () => {
                        using Os.Path path = Os.OpenStudioUtilitiesCore.toPath(temp);
                        if (doc.Format == InterchangeFormat.GbXml) {
                            using Os.GbXMLReverseTranslator gb = new();
                            using Os.OptionalModel fromGb = gb.loadModel(path);
                            return Lowered(fromGb, ctx, "gbxml", use);
                        }
                        using Os.EnergyPlusReverseTranslator ep = new();
                        using Os.OptionalModel fromIdf = ep.loadModel(path);
                        return Lowered(fromIdf, ctx, "idf", use);
                    }))),
                Fin: temp => IO.lift(() => Native(ctx.Key, BimBoundary.HostScratchWrite, () => {
                    File.Delete(temp);
                    return Fin.Succ(unit);
                })))
            .Try().runFin.As().Run();

    static Fin<T> Lowered<T>(Os.OptionalModel optional, ProjectionContext ctx, string arm, Func<Os.Model, Fin<T>> use) =>
        optional.is_initialized()
            ? use(optional.get())
            : Fin.Fail<T>(new BimFault.Refused(ctx.Key, BimScope.Energy, BimReason.Rejected, string.Join(':', new object?[] { "energy-decode", arm, "unreadable" })));

    Fin<RaiseState> RaiseOsm(Os.Model model, ProjectionContext ctx) {
        RaiseScope scope = Seeded(ctx, model.nameString());
        RaiseState state = scope.State;
        SpatialSpine spine = scope.Spine;
        Map<string, NodeId> zones = scope.Zones;
        using Os.SpaceVector osSpaces = model.getSpaces();
        for (int i = 0; i < osSpaces.Count; i++) {
            using Os.Space osSpace = osSpaces[i];
            NodeId spaceId = NodeId.Of(new NodeSeed.Placement());
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
                if (!EnergyClassRows.BySurfaceType.TryGetValue(kind, out var row)) {
                    state = state.Note(EnergyReason.ClassUnmapped, surf.nameString());
                    continue;
                }
                NodeId surfaceId = NodeId.Of(new NodeSeed.Placement());
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

    static string Named(Os.OptionalBuildingStory optional) { using Os.BuildingStory storey = optional.get(); return storey.nameString(); }

    static string Named(Os.OptionalThermalZone optional) { using Os.ThermalZone zone = optional.get(); return zone.nameString(); }

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

    static FootprintPolygon OsmRing(Os.Point3dVector vertices) =>
        Ring(vertices.Count, index => { using Os.Point3d p = vertices[index]; return new Vector3(p.x(), p.y(), p.z()); });

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

    Option<(Node.Material Node, MaterialLayer Layer)> TypedLayer(Os.Model model, Os.Material element, ProjectionContext ctx) {
        using Os.OptionalStandardOpaqueMaterial opaque = model.getStandardOpaqueMaterial(element.handle());
        if (opaque.is_initialized()) {
            using Os.StandardOpaqueMaterial m = opaque.get();
            return MaterialPropertySet
                .OfThermal(m.conductivity(), m.specificHeat(), LayerConductance(m.conductivity(), m.thickness()), VapourOpen, ctx.Key)
                .Bind(thermal => MeasureValue.OfSi(Dimension.LengthDim, m.thickness(), ctx.Key).Map(thickness => (
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
            .Bind(optical => MeasureValue.OfSi(Dimension.LengthDim, g.thickness(), ctx.Key).ToOption().Map(thickness => (
                Mint(g.nameString(), MaterialComposition.OfSingle(MaterialId.Create(g.nameString())), Seq(optical), ctx.Header.Tolerance),
                new MaterialLayer(MaterialId.Create(g.nameString()), thickness, g.nameString()))));
    }

    static K<Option, double> Opt(Os.OptionalDouble optional) { using (optional) { return optional.is_initialized() ? Some(optional.get()) : None; } }

    // --- [SPATIAL_SPINE]
    static (RaiseState State, SpatialSpine Spine) Root(RaiseState state, string identifier) {
        NodeId project = NodeId.Of(new NodeSeed.Placement()), site = NodeId.Of(new NodeSeed.Placement()), building = NodeId.Of(new NodeSeed.Placement());
        RaiseState seeded = state
            .Put(Element(project, IfcClass.Project, "", identifier))
            .Put(Element(site, IfcClass.Site, "", identifier))
            .Put(Element(building, IfcClass.Building, "", identifier))
            .Link(new Relationship.Compose(project, site, ComposeKind.Aggregate))
            .Link(new Relationship.Compose(site, building, ComposeKind.Aggregate));
        return (seeded, new SpatialSpine(project, site, building, Map<string, NodeId>()));
    }

    static (RaiseState State, SpatialSpine Spine, NodeId Storey) Storey(RaiseState state, SpatialSpine spine, string name) =>
        spine.Storeys.Find(name).Match(
            Some: held => (state, spine, held),
            None: () => {
                NodeId storey = NodeId.Of(new NodeSeed.Placement());
                RaiseState landed = state
                    .Put(Element(storey, IfcClass.BuildingStorey, "", name))
                    .Link(new Relationship.Compose(spine.Building, storey, ComposeKind.Aggregate));
                return (landed, spine with { Storeys = spine.Storeys.Add(name, storey) }, storey);
            });

    static (RaiseState State, Map<string, NodeId> Zones) Zoned(
        RaiseState state, Map<string, NodeId> zones, NodeId space, Option<string> name) =>
        name.Filter(static z => z.Length > 0).Match(
            None: () => (state, zones),
            Some: zone => zones.Find(zone).Match(
                Some: held => (state.Link(new Relationship.Assign(space, held, AssignKind.Group)), zones),
                None: () => {
                    NodeId minted = NodeId.Of(new NodeSeed.Placement());
                    return (state
                        .Put(Element(minted, IfcClass.Zone, "", zone))
                        .Link(new Relationship.Assign(space, minted, AssignKind.Group)),
                        zones.Add(zone, minted));
                }));

    // --- [SHARED_MINTS]
    internal static readonly PropertyName BoundaryCondition = PropertyCategory.Seam.Row("BoundaryCondition");
    internal static readonly PropertyName StoryMultiplier = PropertyCategory.Seam.Row("StoryMultiplier");
    internal static readonly PropertyName UFactor = PropertyCategory.Seam.Row("UFactor");
    internal static readonly PropertyName ConstructionName = PropertyCategory.Seam.Row("ConstructionName");

    internal const string EnergyModelSet = "Pset_EnergyModel";

    internal const double VapourOpen = 1.0;

    static double LayerConductance(double conductivity, double thickness) => conductivity / thickness;

    static Node.Object Element(NodeId id, IfcClass @class, string predefined, string identifier, UInt128 footprint = default) =>
        new(Id: id, Kind: ObjectKind.Occurrence,
            ExternalId: Optional(identifier).Filter(static s => s.Length > 0),
            Classification: @class.EntityClass,
            PredefinedType: PredefinedType.Create(predefined),
            Name: identifier, Tag: "",
            Representations: footprint == default
                ? RepresentationContentHash.Empty
                : RepresentationContentHash.Empty.With("FootPrint", footprint),
            History: None, Span: @class.Span);

    static readonly WireName SpaceBoundary = WireName.Create(IfcRelKind.SpaceBoundary.Key);

    static Relationship Boundary(NodeId space, NodeId surface, BoundaryRow condition) =>
        new Relationship.Generic(SpaceBoundary, space, surface, Map(
            (BoundaryRows.BoundaryLevel,  (PropertyValue)new PropertyValue.Text("2nd")),
            (BoundaryCondition,   (PropertyValue)new PropertyValue.Text(condition.Key))));

    static (RaiseState State, NodeId Id) Opening(RaiseState state, NodeId space, string hostIdentifier, IfcClass @class, string identifier, FootprintPolygon ring, ProjectionContext ctx) {
        NodeId openingId = NodeId.Of(new NodeSeed.Placement());
        (RaiseState blobbed, UInt128 footprint) = Footprint(state, ring, ctx.Header.Tolerance);
        return (blobbed
            .Land(EnergySlot.Opening, Element(openingId, @class, "", identifier, footprint))
            .Link(new Relationship.Generic(SpaceBoundary, space, openingId, Map(
                (BoundaryRows.BoundaryLevel, (PropertyValue)new PropertyValue.Text("2nd")),
                (BoundaryRows.Host,  (PropertyValue)new PropertyValue.Text(hostIdentifier))))), openingId);
    }

    static RaiseState Associate(RaiseState state, NodeId subject, Resolved resolved) =>
        resolved.Composition.Match(
            Some: s => s.Layers
                .Fold(state.Noted(resolved.Notes).Land(EnergySlot.Construction, s.LayerSetNode), static (acc, n) => acc.Put(n))
                .Link(new Relationship.Associate(subject, s.LayerSetNode.Id, new MaterialUsage.Unbound())),
            None: () => state.Noted(resolved.Notes));

    static RaiseState Assigned(RaiseState state, NodeId subject, Node node) =>
        state.Put(node).Link(new Relationship.Assign(subject, node.Id, AssignKind.PropertyDefinition));

    static FootprintPolygon Ring(int count, Func<int, Vector3> at) =>
        new(toSeq(Enumerable.Range(0, count)).Map(at));

    static Vector3 Point(List<double> coordinate, double plane) =>
        new(coordinate[0], coordinate[1], coordinate.Count > 2 ? coordinate[2] : plane);

    static (RaiseState State, UInt128 Key) Footprint(RaiseState state, FootprintPolygon ring, double tolerance) {
        UInt128 key = ContentAddress.Of(ring, tolerance, static (polygon, writer) =>
            polygon.Ring.Fold(writer, static (w, p) => w.Double(p.X).Double(p.Y).Double(p.Z))).Value;
        return (state.Blob(key, ring), key);
    }

    static RaiseState UFactorEvidence(RaiseState state, NodeId surfaceId, Os.ConstructionBase construction, ProjectionContext ctx) {
        using Os.OptionalDouble u = construction.uFactor();
        if (!u.is_initialized()) { return state; }
        return MeasureValue.Of(u.get(), UnitsNet.Units.HeatTransferCoefficientUnit.WattPerSquareMeterKelvin, ctx.Key).Match(
            Fail: _ => state.Note(EnergyReason.MeasureRejected, construction.nameString()),
            Succ: uValue => {
                PropertyBag bag = new(EnergyModelSet, Map(
                    (UFactor,          (PropertyValue)new PropertyValue.Measure(uValue)),
                    (ConstructionName, (PropertyValue)new PropertyValue.Text(construction.nameString()))),
                    InheritanceMode.OccurrenceWins, EvidenceGrade.Derived);
                Node.PropertySet probe = new(NodeId.Of(new NodeSeed.Placement()), bag);
                return Assigned(state, surfaceId, new Node.PropertySet(NodeId.Of(new NodeSeed.Content(probe, ctx.Header.Tolerance)), bag));
            });
    }
}
```

## [03]-[RESEARCH]

(none)
