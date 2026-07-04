# [BIM_ENERGY_PROJECTOR]

The energy-model raise: `EnergyProjector : IElementProjection`, the second Bim projector beside the `Projection/semantic#SEMANTIC_PROJECTOR` (the `Exchange/reconstruct#RECONSTRUCTION` precedent) — five decode arms (HBJSON/DFJSON managed, OSM/gbXML/IDF SWIG) converge on ONE projection landing the EXACT graph shape the `csharp:Rasm.Compute/Analysis/energy` runner already reads: `IfcSpace`-classified `Object` nodes, bounding-surface Objects joined by `IfcRelSpaceBoundary`-named neutral `Generic` edges carrying the `BoundaryLevel` payload, `Host`-attributed opening boundaries for every window and door, analytical footprints content-keyed into `Representations.FootPrint`, and seam `MaterialComposition.LayerSet` material evidence — opaque layers as `MaterialPropertySet.Thermal`, glazing layers as the `Optical` case the Compute `StandardGlazing` build consumes — so a document raised by this page is simulable by `Rasm.Compute` with zero adapter, the wire-name/Qto/Pset contract being the load-bearing alignment. The `Energy/exchange#ENERGY_EXCHANGE` `EnergyExchange.Apply` `Raise` arm drives it under the seam `ProjectionAssembly.Assemble` fold with `IfcLegality` composed; the `Energy/derive#MODEL_DERIVE` lower reads the same class table, boundary statics, and evidence bags back, so the two directions can never drift.

Wire posture: HOST-LOCAL, foreign types decode-confined. The LBT-Newtonsoft `Model.FromJson` gates hard by construction (parse throw + in-parse DataAnnotations), no `HoneybeeSchema.*`/`DragonflySchema.*` DTO outlives `Project`, and every `OpenStudio.*` SWIG wrapper — the model, translators, `Optional*`, `*Vector`, per-element handles — is `using`-bracketed with the index-loop + per-element disposal as the named marshaling exemption. Faults route the `Model/faults#FAULT_BAND` arms: `ModelRejected` (`energy-decode`), `UnmappedClass` (`energy-face-miss`/`energy-class-miss`), `DanglingReference` (`energy-construction-absent`).

## [01]-[INDEX]

- [01]-[ENERGY_PROJECTOR]: `EnergyProjector : IElementProjection` — five format arms onto one raise fold landing the Compute-readable seam shape, the `EnergyClassRows` one-table FaceType↔`IfcClass` correspondence (plus the OSM surface-type and opening-type derivations), and the seam material/footprint/evidence minting.

## [02]-[ENERGY_PROJECTOR]

- Owner: `EnergyProjector : IElementProjection` the energy-model raise (the raw `EnergyDoc` captured internally, the seam contract carrying only `Node`/`Relationship`/`GraphDelta`); `EnergyClassRows` the ONE FaceType↔`IfcClass` correspondence table both directions derive from — the raise map, the OSM `Surface.surfaceType()` string leg, the OSM `SubSurface.subSurfaceType()` opening leg, and the lower-side `ToFace` inverse; the frozen `Arms` format→arm index the `Serves` capability predicate reads.
- Entry: `EnergyProjector.Project(ProjectionContext ctx)` → `Fin<GraphDelta>` — the frozen arm index dispatches the captured document's format row: `Hbjson` decodes through `HoneybeeSchema.Model.FromJson` inside the `Try.lift` funnel (the LBT-Newtonsoft parse throw and the DataAnnotations reject both land `BimFault.ModelRejected` `energy-decode`, the raw message preserved — `FromJson` itself gates DataAnnotations through `IsValid(throwException: true)`, throwing `ArgumentException` on a structurally-invalid document; a `"type"`-mismatch null-parse faults explicitly, never a null flowing on), `Dfjson` through `DragonflySchema.Model.FromJson` (the identical funnel and in-parse gate), and `Osm`/`GbXml`/`Idf` through the bracketed SWIG decode trio (`VersionTranslator.loadModelFromString` — the version-upgrading in-string OSM read; `GbXMLReverseTranslator.loadModel(Path)`/`EnergyPlusReverseTranslator.loadModel(Path)` — path-bound readers crossed via a bracketed temp path, the dotbim/usd-stage precedent) converging on ONE `RaiseOsm` fold.
- Auto: every arm lands the Compute-readable seam shape — a room/space mints an `IfcSpace`-classified rooted `Object` (`ExternalId` = the schema identifier, so a re-raise correlates), each bounding face a surface `Object` through the `EnergyClassRows` row (`Wall`→`IfcWall`, `Floor`→`IfcSlab`+`FLOOR`, `RoofCeiling`→`IfcSlab`+`ROOF`, `AirBoundary`→`IfcVirtualElement`), joined by the `IfcRelKind.SpaceBoundary`-named neutral `Generic` edge carrying `BoundaryLevel: "2nd"` (a honeybee/OSM face is a per-space bounded surface — the second-level shape `Rasm.Compute` `EnergyGraphReads.BoundingSurfacesOf` prefers) plus the `BoundaryCondition` text payload; the face ring content-keys into `Representations.FootPrint` through the seam footprint byte projection (recorded on the projector's `Footprints` side-channel for the caller's write-blob-first landing); openings land on EVERY arm — honeybee `Aperture`/`Door` rows and OSM `Surface.subSurfaces()` alike mint `IfcWindow`/`IfcDoor` Objects joined to the space by the SAME boundary edge shape with a `Host` correlation attribute naming their face (an opening IS a space boundary in energy modeling; the typed `Void`/fill lowering demands the `IfcOpeningElement` intermediary no energy schema carries), and each opening's own abridged window construction resolves through the ONE `Composition` fold so raised fenestration carries the `Optical` layer evidence the Compute `SubSurface` glazing build reads; an abridged construction resolves through the model-level canonical lists (the `[ABRIDGED_REFERENCE_MODEL]` law — construction ids into `ModelEnergyProperties.Constructions`, material ids into `.Materials`) discriminating opaque versus window by which list carries the id — an `EnergyMaterial` mints a content-keyed seam `Node.Material` carrying `MaterialPropertySet.OfThermal(conductivity, specificHeat, conductivity/thickness, 1.0, key)` (the per-layer U derived, the vapour factor floored at the vapour-open 1.0 the schema does not carry), an `EnergyWindowMaterialGlazing` mints the `OfOptical` nine-fraction case (the `AnyOf<Autocalculate,double>` back-reflectances resolving to the front value — the honeybee autocalculate semantic, never a zero read) — and the surface or opening takes an `Associate` edge over `MaterialComposition.OfLayerSet`; the OSM arm reads `Model.getSpaces()`/`Space.surfaces`/`PlanarSurface.vertices()`/`Surface.surfaceType()`/`outsideBoundaryCondition()` and resolves typed layer materials through `Construction.layers()` + the handle re-read pair `Model.getStandardOpaqueMaterial(handle)`/`getStandardGlazing(handle)` (the SWIG vector element is statically `Material` — the handle re-read is the ONLY typed downcast the binding owns, and ONE `TypedLayer` read serves the opaque and glazing rows); the dragonfly massing arm additionally lands each `Room2D.FloorToCeilingHeight` as the `Qto_SpaceBaseQuantities` `Height` quantity the derive's massing lower reads back (a DFJSON round trip falling to the 3.0 m policy default was the deleted round-trip hole) and stamps `Story.Multiplier > 1` as `Pset_EnergyModel`/`StoryMultiplier` Import-source bag evidence (a 40-storey tower ships unique stories × multiplier — dropped, the re-emitted model under-counts by the repeat factor).
- Receipt: the projector tallies `Spaces`/`Surfaces`/`Openings`/`Constructions` and the degraded-material warnings as owner-local fold state projected once into the `Energy/exchange#ENERGY_EXCHANGE` `EnergyReceipt`; the managed decodes reject inside `FromJson` (the funnel owns the evidence), so the raise tallies degrade warnings only — `Validate()` counting belongs to the lower legs authoring models locally.
- Packages: HoneybeeSchema, DragonflySchema, NREL.OpenStudio.macOS-arm64, Rasm.Element, Rasm, LanguageExt.Core, Thinktecture.Runtime.Extensions
- Growth: a new face/class correspondence is one `EnergyClassRows` row (both directions derive); a new OSM opening token is one `ByOpeningType` row; a new energy form is one `Arms` row; a dragonfly parameter (window ratios, shading, skylights) deepens the massing arm as row folds over the `Room2D` `AnyOf` unions; a NoMass/Vegetation material arm is one typed-layer row the moment the seam carries an R-value-only thermal case; the FULL (non-abridged) `OpaqueConstruction`/`WindowConstruction` store rows — inline material OBJECTS, not id references, so a different resolve shape — are one `Library` projection widening plus one inline-material arm the moment full-form documents ship, and until then a full-form construction id resolves in neither abridged list and faults `DanglingReference` (the declared abridged-only restraint, never a silent partial read); honeybee `Shade`/`ShadeMesh` context geometry raises as one arm row the moment an `IfcShadingDevice` roster row is exercised by a consumer read.
- Boundary: the projector decodes INSIDE `Project`, so no LBT DTO or SWIG handle outlives the projection (every OpenStudio wrapper — the model, translators, `Optional*`, `*Vector`, per-element handles — is `using`-bracketed; the SWIG index-loop with per-element disposal is the named marshaling exemption); the `EnergyMaterial` density has NO seam thermal column and a fabricated `OfMechanical` stiffness is the rejected form — the density is DROPPED at the raise (systematic, so never a per-material warning), and the OSM rebuild's documented 1000 kg/m³ fallback carries the consequence; a glazing layer mints ONLY the `Optical` case — an `OfThermal` with a fabricated specific heat is the rejected fabricated-physics form, and the Compute glazing build already tolerates the absent Thermal (`setThermalConductivity` rides an optional read); an OSM layer whose typed re-read misses BOTH rows (a massless/airgap/gas-mixture layer) degrades the WHOLE set to the assembly `ConstructionBase.uFactor()` landed as `Pset_EnergyModel` bag evidence plus a warning, never a fabricated layer set; the structural-graph legality (endpoints, ids) is the seam's `ElementFault`, the IFC-semantic legality is the composed `IfcLegality` → `BimFault.ModelRejected`, and this projector re-checks neither; the rooted `NodeId` is LOCAL per raise (Guid-v7), the schema identifier riding `ExternalId` for correlation — the wire law verbatim.

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
using Rasm.Domain;
using Rasm.Element;
using static LanguageExt.Prelude;
using Df = DragonflySchema;      // boundary-only aliases: the DTO namespaces never escape this file
using Hb = HoneybeeSchema;
using Os = OpenStudio;
// The per-document canonical-list projection the abridged-reference resolve reads — opaque and window lists in
// one dense alias, not a shape.
using EnergyLibrary = (
    LanguageExt.Seq<HoneybeeSchema.OpaqueConstructionAbridged> Constructions,
    LanguageExt.Seq<HoneybeeSchema.EnergyMaterial> Materials,
    LanguageExt.Seq<HoneybeeSchema.WindowConstructionAbridged> WindowConstructions,
    LanguageExt.Seq<HoneybeeSchema.EnergyWindowMaterialGlazing> Glazings);

namespace Rasm.Bim;

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

    // The OSM read's string leg — Surface.surfaceType() tokens ARE the FaceType names — derived from the one table.
    // OrdinalIgnoreCase like ByOpeningType: IDF fields are case-insensitive, so a reverse-translated "WALL" is legal.
    internal static readonly FrozenDictionary<string, (IfcClass Class, string Predefined)> BySurfaceType =
        Rows.ToFrozenDictionary(static r => r.Face.ToString(), static r => (r.Class, r.Predefined), StringComparer.OrdinalIgnoreCase);

    // The OSM SubSurface.subSurfaceType() tokens onto the opening classes (validSubSurfaceTypeValues roster):
    // glass-bearing tokens fold to IfcWindow, door tokens to IfcDoor — the Compute FixedWindow/Door build inverts it.
    internal static readonly FrozenDictionary<string, IfcClass> ByOpeningType = new KeyValuePair<string, IfcClass>[] {
        new("FixedWindow", IfcClass.Window), new("OperableWindow", IfcClass.Window), new("Skylight", IfcClass.Window),
        new("TubularDaylightDome", IfcClass.Window), new("TubularDaylightDiffuser", IfcClass.Window),
        new("GlassDoor", IfcClass.Door), new("Door", IfcClass.Door), new("OverheadDoor", IfcClass.Door),
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    internal static readonly FrozenDictionary<(string Code, PredefinedType Token), Hb.FaceType> ToFace =
        Rows.Select(static r => ((r.Class.Key, PredefinedType.Create(r.Predefined)), r.Face))
            .Concat(LowerOverlay.Select(static r => ((r.Class.Key, PredefinedType.Create("")), r.Face)))
            .ToFrozenDictionary(static p => p.Item1, static p => p.Item2);
}

// --- [SERVICES] ---------------------------------------------------------------------------
// The energy-model PRIMARY projector: the raw document captured internally, five arms converging on the
// seam shape the Compute energy runner reads. Footprints is the write-blob-first side-channel; the counters
// are owner-local fold state the receipt projects once.
public sealed class EnergyProjector(EnergyDoc doc) : IElementProjection {
    readonly List<(UInt128 Key, FootprintPolygon Ring)> footprints = [];
    int spaces, surfaces, openings, constructions, warnings;

    public Seq<(UInt128 Key, FootprintPolygon Ring)> Footprints => toSeq(footprints);

    public EnergyReceipt Receipt(Instant at) =>
        new(EnergyLeg.Raised, doc.Format, None, spaces, surfaces, openings, constructions, warnings, doc.SourceKey, at);

    static readonly FrozenDictionary<InterchangeFormat, Func<EnergyProjector, ProjectionContext, Fin<GraphDelta>>> Arms =
        new KeyValuePair<InterchangeFormat, Func<EnergyProjector, ProjectionContext, Fin<GraphDelta>>>[] {
            new(InterchangeFormat.Hbjson, static (p, ctx) => p.Honeybee(ctx)),
            new(InterchangeFormat.Dfjson, static (p, ctx) => p.Dragonfly(ctx)),
            new(InterchangeFormat.Osm,    static (p, ctx) => p.OsmFamily(ctx)),
            new(InterchangeFormat.GbXml,  static (p, ctx) => p.OsmFamily(ctx)),
            new(InterchangeFormat.Idf,    static (p, ctx) => p.OsmFamily(ctx)),
        }.ToFrozenDictionary();

    internal static bool Serves(InterchangeFormat format) => Arms.ContainsKey(format);

    public Fin<GraphDelta> Project(ProjectionContext ctx) =>
        Arms.TryGetValue(doc.Format, out var arm)
            ? arm(this, ctx)
            : Fin.Fail<GraphDelta>(new BimFault.CodecReject(ctx.Key, $"energy-form-miss:{doc.Format.Key}"));

    // --- [HONEYBEE_ARM]
    // FromJson gates HARD by construction: the LBT-Newtonsoft parse throw AND the DataAnnotations reject (the
    // in-FromJson IsValid(throwException: true) ArgumentException) land in the funnel, a "type"-mismatch
    // null-parse faults explicitly. Post-admission Validate() is structurally empty, so the raise tallies no
    // schema warnings — the Validate() tally belongs to the LOWER legs authoring models locally.
    Fin<GraphDelta> Honeybee(ProjectionContext ctx) =>
        Try.lift(() => Hb.Model.FromJson(doc.Text)).Run()
            .MapFail(error => new BimFault.ModelRejected(ctx.Key, $"energy-decode:{error.Message}"))
            .Bind(model => model is null
                ? Fin.Fail<GraphDelta>(new BimFault.ModelRejected(ctx.Key, "energy-decode:<type-mismatch>"))
                : RaiseRooms(Library(model.Properties?.Energy?.Constructions, model.Properties?.Energy?.Materials), toSeq(model.Rooms ?? []), ctx));

    // The model-level canonical lists projected ONCE per document — the abridged-reference resolve source, opaque
    // AND window rows. ONE body serves both schemas: the dragonfly store is its OWN type but its lists are
    // HoneybeeSchema.AnyOf rows (DragonflySchema ships no AnyOf of its own), so both stores enter covariantly as
    // IEnumerable<Hb.AnyOf> and Building.Room3ds route through the identical room fold with no second resolve path.
    static EnergyLibrary Library(IEnumerable<Hb.AnyOf>? constructions, IEnumerable<Hb.AnyOf>? materials) => (
        toSeq(constructions ?? []).Choose(static any => any.Obj is Hb.OpaqueConstructionAbridged oc ? Some(oc) : None),
        toSeq(materials ?? []).Choose(static any => any.Obj is Hb.EnergyMaterial m ? Some(m) : None),
        toSeq(constructions ?? []).Choose(static any => any.Obj is Hb.WindowConstructionAbridged wc ? Some(wc) : None),
        toSeq(materials ?? []).Choose(static any => any.Obj is Hb.EnergyWindowMaterialGlazing g ? Some(g) : None));

    Fin<GraphDelta> RaiseRooms(EnergyLibrary library, Seq<Hb.Room> rooms, ProjectionContext ctx) =>
        rooms.Fold(
            Fin.Succ(GraphDelta.Empty.Reheader(ctx.Header)),
            (acc, room) => acc.Bind(delta => RaiseRoom(library, room, ctx).Map(delta.Merge)));

    // host carries the dragonfly building a Room3d nests under (Some exactly on the massing arm's 3D rooms).
    Fin<GraphDelta> RaiseRoom(EnergyLibrary library, Hb.Room room, ProjectionContext ctx, Option<NodeId> host = default) {
        NodeId spaceId = NodeId.Rooted();
        spaces++;
        GraphDelta seed = GraphDelta.Empty.Put(Element(spaceId, IfcClass.Space, "", room.Identifier));
        seed = host.Match(Some: building => seed.Link(new Relationship.Compose(building, spaceId, ComposeKind.Contain)), None: () => seed);
        return toSeq(room.Faces ?? []).Fold(
            Fin.Succ(seed),
            (acc, face) => acc.Bind(delta => RaiseFace(library, spaceId, face, ctx).Map(delta.Merge)));
    }

    // A face mints the surface Object + FootPrint representation key + the SpaceBoundary Generic edge the
    // Compute reads consume; the apertures/doors fold as ONE opening-row pass (identifier, ring, abridged
    // construction id, class) so a raised window carries the Optical evidence the Compute SubSurface build
    // reads; the face construction associates the seam layer composition. BoundaryLevel is "2nd" — a honeybee
    // face IS the per-space bounded surface.
    Fin<GraphDelta> RaiseFace(EnergyLibrary library, NodeId spaceId, Hb.Face face, ProjectionContext ctx) {
        if (!EnergyClassRows.ToClass.TryGetValue(face.FaceType, out var row)) {
            return Fin.Fail<GraphDelta>(new BimFault.UnmappedClass(ctx.Key, $"energy-face-miss:{face.FaceType}"));
        }
        NodeId surfaceId = NodeId.Rooted();
        surfaces++;
        UInt128 footprint = Footprint(Ring(face.Geometry.Boundary), ctx.Header.Tolerance);
        GraphDelta seed = GraphDelta.Empty
            .Put(Element(surfaceId, row.Class, row.Predefined, face.Identifier, footprint))
            .Link(Boundary(spaceId, surfaceId,
                face.BoundaryCondition.Obj is Hb.OpenAPIGenBaseModel bc ? bc.Type : "Outdoors"));   // the schema Type discriminator, never a downcast chain
        Seq<(string Identifier, List<List<double>> Ring, string? Construction, IfcClass Class)> rows =
            toSeq(face.Apertures ?? []).Map(a => (a.Identifier, a.Geometry.Boundary, a.Properties?.Energy?.Construction, IfcClass.Window))
            + toSeq(face.Doors ?? []).Map(d => (d.Identifier, d.Geometry.Boundary, d.Properties?.Energy?.Construction, IfcClass.Door));
        return rows
            .Fold(Fin.Succ(seed), (acc, o) => acc.Bind(delta => {
                (GraphDelta opened, NodeId openingId) = Opening(delta, spaceId, face.Identifier, o.Class, o.Identifier, Ring(o.Ring), ctx);
                return Composition(library, o.Construction, ctx).Map(set => Associate(opened, openingId, set));
            }))
            .Bind(delta => Composition(library, face.Properties?.Energy?.Construction, ctx).Map(set => Associate(delta, surfaceId, set)));
    }

    // The abridged-reference resolve, opaque AND window: the construction id resolves in the opaque list
    // (EnergyMaterial rows -> Thermal) or the window list (EnergyWindowMaterialGlazing rows -> Optical) — one
    // resolve serves faces and openings; a dangling id faults, a NoMass/Vegetation/gas layer degrades to a warning.
    Fin<Option<(Node.Material LayerSetNode, Seq<Node.Material> Layers)>> Composition(EnergyLibrary library, string? construction, ProjectionContext ctx) =>
        Optional(construction).Match(
            None: () => Fin.Succ(Option<(Node.Material, Seq<Node.Material>)>.None),
            Some: id => library.Constructions.Find(oc => oc.Identifier == id).Match(
                Some: oc => Resolve(id, toSeq(oc.Materials), mid => SeamMaterial(library, mid, ctx), ctx),
                None: () => library.WindowConstructions.Find(wc => wc.Identifier == id)
                    .ToFin(new BimFault.DanglingReference(ctx.Key, $"energy-construction-absent:{id}"))
                    .Bind(wc => Resolve(id, toSeq(wc.Materials), mid => SeamGlazing(library, mid, ctx), ctx))));

    // Material ids -> per-material Single nodes + ONE LayerSet Material node the surface/opening associates
    // (the seam composition shape graph.CompositionOf resolves) — one fold parameterized by the material arm,
    // never an opaque copy beside a glazing copy.
    Fin<Option<(Node.Material, Seq<Node.Material>)>> Resolve(
        string id, Seq<string> materialIds,
        Func<string, Validation<Error, Option<(Node.Material Node, MaterialLayer Layer)>>> arm, ProjectionContext ctx) =>
        materialIds.Traverse(arm).As().ToFin()
            .Bind(rows => rows.Somes() is { IsEmpty: false } typed
                ? MaterialComposition.OfLayerSet(typed.Map(static r => r.Layer), ctx.Key)
                    .Map(set => Some((Mint(id, set, Seq<MaterialPropertySet>(), ctx.Header.Tolerance), typed.Map(static r => r.Node))))
                : Fin.Succ(Option<(Node.Material, Seq<Node.Material>)>.None));

    Validation<Error, Option<(Node.Material Node, MaterialLayer Layer)>> SeamMaterial(EnergyLibrary library, string materialId, ProjectionContext ctx) =>
        library.Materials
            .Find(m => m.Identifier == materialId)
            .Match(
                None: () => { warnings++; return Success<Error, Option<(Node.Material, MaterialLayer)>>(None); },
                Some: m => MaterialPropertySet
                    .OfThermal(m.Conductivity, m.SpecificHeat, m.Conductivity / m.Thickness, 1.0, ctx.Key)
                    .ToValidation()
                    .Map(thermal => Some((
                        Mint(m.Identifier, MaterialComposition.OfSingle(MaterialId.Create(m.Identifier)), Seq(thermal), ctx.Header.Tolerance),
                        new MaterialLayer(MaterialId.Create(m.Identifier), MeasureValue.OfSi(Dimension.LengthDim, m.Thickness), m.Identifier)))));

    // The glazing counterpart: EnergyWindowMaterialGlazing -> the seam Optical nine-fraction case ONLY — an
    // OfThermal with a fabricated specific heat is the rejected fabricated-physics form (the Compute glazing
    // build tolerates the absent Thermal).
    Validation<Error, Option<(Node.Material Node, MaterialLayer Layer)>> SeamGlazing(EnergyLibrary library, string materialId, ProjectionContext ctx) =>
        library.Glazings
            .Find(g => g.Identifier == materialId)
            .Match(
                None: () => { warnings++; return Success<Error, Option<(Node.Material, MaterialLayer)>>(None); },
                Some: g => MaterialPropertySet
                    .OfOptical(
                        g.VisibleTransmittance, g.VisibleReflectance, Alt(g.VisibleReflectanceBack, g.VisibleReflectance),
                        g.SolarTransmittance, g.SolarReflectance, Alt(g.SolarReflectanceBack, g.SolarReflectance),
                        g.InfraredTransmittance, g.Emissivity, g.EmissivityBack, ctx.Key)
                    .ToValidation()
                    .Map(optical => Some((
                        Mint(g.Identifier, MaterialComposition.OfSingle(MaterialId.Create(g.Identifier)), Seq(optical), ctx.Header.Tolerance),
                        new MaterialLayer(MaterialId.Create(g.Identifier), MeasureValue.OfSi(Dimension.LengthDim, g.Thickness), g.Identifier)))));

    // The AnyOf<Autocalculate,double> back-reflectance sentinel resolves to the front value — the honeybee
    // autocalculate semantic, never a zero read.
    static double Alt(Hb.AnyOf<Hb.Autocalculate, double> value, double front) => value?.Obj is double d ? d : front;

    // Content-keyed Material mint: probe id overwritten by the canonical-bytes content hash so identical
    // materials and layer sets dedup across raises (the composition-page mint law).
    static Node.Material Mint(string identifier, MaterialComposition composition, Seq<MaterialPropertySet> properties, double tolerance) {
        Node.Material probe = new(NodeId.Content([]), MaterialId.Create(identifier), composition, properties);
        return new(NodeId.Content(probe.ToCanonicalBytes(tolerance).Span), MaterialId.Create(identifier), composition, properties);
    }

    // --- [DRAGONFLY_ARM]
    // The massing raise: Building/Story/Room2D lower onto an IfcBuilding/IfcBuildingStorey/IfcSpace Compose
    // tree with the floor plate content-keyed as the space FootPrint, the Room2D floor-to-ceiling height landed
    // as the Qto_SpaceBaseQuantities Height quantity the derive reads back, and Story.Multiplier stamped as
    // Import-source bag evidence; Building.Room3ds (full honeybee rooms) route through the SAME honeybee room
    // fold under the building host — dragonfly composes the honeybee vocabulary, never re-mints it.
    Fin<GraphDelta> Dragonfly(ProjectionContext ctx) =>
        Try.lift(() => Df.Model.FromJson(doc.Text)).Run()
            .MapFail(error => new BimFault.ModelRejected(ctx.Key, $"energy-decode:{error.Message}"))
            .Bind(model => model is null
                ? Fin.Fail<GraphDelta>(new BimFault.ModelRejected(ctx.Key, "energy-decode:<type-mismatch>"))
                : toSeq(model.Buildings ?? []).Fold(
                    Fin.Succ(GraphDelta.Empty.Reheader(ctx.Header)),
                    (acc, building) => acc.Bind(delta => RaiseBuilding(Library(model.Properties?.Energy?.Constructions, model.Properties?.Energy?.Materials), building, ctx).Map(delta.Merge))));

    Fin<GraphDelta> RaiseBuilding(EnergyLibrary library, Df.Building building, ProjectionContext ctx) {
        NodeId buildingId = NodeId.Rooted();
        var seed = GraphDelta.Empty.Put(Element(buildingId, IfcClass.Building, "", building.Identifier));
        Fin<GraphDelta> massing = toSeq(building.UniqueStories ?? []).Fold(Fin.Succ(seed), (acc, story) => acc.Map(delta => {
            NodeId storeyId = NodeId.Rooted();
            var d = delta.Put(Element(storeyId, IfcClass.BuildingStorey, "", story.Identifier))
                .Link(new Relationship.Compose(buildingId, storeyId, ComposeKind.Aggregate));
            d = story.Multiplier > 1 ? Assigned(d, storeyId, MultiplierEvidence(story.Multiplier, ctx.Header.Tolerance)) : d;
            return toSeq(story.Room2ds ?? []).Fold(d, (dd, room) => {
                NodeId spaceId = NodeId.Rooted();
                spaces++;
                UInt128 plate = Footprint(PlateRing(room), ctx.Header.Tolerance);
                return Assigned(
                    dd.Put(Element(spaceId, IfcClass.Space, "", room.Identifier, plate))
                        .Link(new Relationship.Compose(storeyId, spaceId, ComposeKind.Contain)),
                    spaceId, HeightQuantity(room.FloorToCeilingHeight, ctx.Header.Tolerance));
            });
        }));
        return toSeq(building.Room3ds ?? []).Fold(massing,
            (acc, room) => acc.Bind(delta => RaiseRoom(library, room, ctx, Some(buildingId)).Map(delta.Merge)));
    }

    static FootprintPolygon PlateRing(Df.Room2D room) =>
        new(toSeq(room.FloorBoundary).Map(p => new Vector3(p[0], p[1], room.FloorHeight)));

    // The dragonfly space height landed as the SAME Qto_SpaceBaseQuantities Height quantity the derive's massing
    // lower reads back — a DFJSON round trip that fell to the 3.0 m policy default was the deleted round-trip hole.
    static Node.QuantitySet HeightQuantity(double floorToCeiling, double tolerance) {
        QuantityBag bag = new("Qto_SpaceBaseQuantities",
            Map((PropertyName.Create("Height"), MeasureValue.OfSi(Dimension.LengthDim, floorToCeiling))),
            InheritanceMode.OccurrenceWins, PropertySource.Import);
        Node.QuantitySet probe = new(NodeId.Content([]), bag);
        return new(NodeId.Content(probe.ToCanonicalBytes(tolerance).Span), bag);
    }

    // Story.Multiplier is SOURCE data (unique stories x vertical repeat) — dropped, the derive re-emits
    // multiplier-1 stories and the energy model under-counts by the repeat factor; read back onto Story(multiplier:).
    static Node.PropertySet MultiplierEvidence(int multiplier, double tolerance) {
        PropertyBag bag = new("Pset_EnergyModel",
            Map((StoryMultiplier, (PropertyValue)new PropertyValue.Measure(MeasureValue.OfSi(Dimension.Dimensionless, multiplier)))),
            InheritanceMode.OccurrenceWins, PropertySource.Import);
        Node.PropertySet probe = new(NodeId.Content([]), bag);
        return new(NodeId.Content(probe.ToCanonicalBytes(tolerance).Span), bag);
    }

    // --- [OSM_ARM]
    // Three decode arms, one raise fold. loadModelFromString upgrades any older .osm in-string; the gbXML/IDF
    // readers are Path-bound, crossed via a bracketed temp path (Exemption: SWIG + filesystem boundary). The
    // catch spans decode AND raise: every SWIG member on the fold path can throw natively, and a raise escaping
    // the Fin signature is the exception-control-flow defect the funnel closes.
    Fin<GraphDelta> OsmFamily(ProjectionContext ctx) {
        try {
            return Decode(ctx).Bind(model => { using (model) { return RaiseOsm(model, ctx); } });
        }
        catch (Exception ex) when (ex is SystemException or ApplicationException) {
            return Fin.Fail<GraphDelta>(new BimFault.ModelRejected(ctx.Key, $"energy-decode:{ex.Message}"));
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
            : Fin.Fail<Os.Model>(new BimFault.ModelRejected(ctx.Key, $"energy-decode:<{arm}-unreadable>"));

    // The OSM raise: spaces/surfaces/boundary edges land the same seam shape as the honeybee arm, and each
    // surface's SubSurfaces land as Host-attributed openings; a typed layer set resolves via the handle re-read
    // (the only SWIG downcast), else the assembly uFactor lands as Pset_EnergyModel bag evidence + a warning.
    // Index-loop + per-element using = the SWIG marshaling exemption.
    Fin<GraphDelta> RaiseOsm(Os.Model model, ProjectionContext ctx) {
        var delta = GraphDelta.Empty.Reheader(ctx.Header);
        using Os.SpaceVector osSpaces = model.getSpaces();
        for (int i = 0; i < osSpaces.Count; i++) {
            using Os.Space osSpace = osSpaces[i];
            NodeId spaceId = NodeId.Rooted();
            spaces++;
            delta = delta.Put(Element(spaceId, IfcClass.Space, "", osSpace.nameString()));
            using Os.SurfaceVector surfs = osSpace.surfaces;
            for (int j = 0; j < surfs.Count; j++) {
                using Os.Surface surf = surfs[j];
                string kind = surf.surfaceType();
                if (!EnergyClassRows.BySurfaceType.TryGetValue(kind, out var row)) {
                    return Fin.Fail<GraphDelta>(new BimFault.UnmappedClass(ctx.Key, $"energy-class-miss:{kind}"));
                }
                NodeId surfaceId = NodeId.Rooted();
                surfaces++;
                using Os.Point3dVector vertices = surf.vertices();
                UInt128 footprint = Footprint(OsmRing(vertices), ctx.Header.Tolerance);
                delta = delta
                    .Put(Element(surfaceId, row.Class, row.Predefined, surf.nameString(), footprint))
                    .Link(Boundary(spaceId, surfaceId, surf.outsideBoundaryCondition()));
                delta = OsmComposition(model, surf, surfaceId, delta, ctx);
                delta = OsmSubSurfaces(model, surf, spaceId, delta, ctx);
            }
        }
        return Fin.Succ(delta);
    }

    // The OSM opening fold: Surface.subSurfaces() land as IfcWindow/IfcDoor Objects joined to the SPACE by the
    // same Host-attributed boundary edge the honeybee arm mints — an OSM raised without its fenestration
    // simulates an unglazed building, the deleted coverage hole; each opening's own construction resolves
    // through the ONE PlanarSurface composition fold. An out-of-roster subSurfaceType degrades warning-counted
    // where a base-surface class miss FAULTS — deliberate: an opening is envelope refinement, a base surface
    // envelope structure (all eight validSubSurfaceTypeValues are mapped, so a miss is an invalid file token).
    GraphDelta OsmSubSurfaces(Os.Model model, Os.Surface surf, NodeId spaceId, GraphDelta delta, ProjectionContext ctx) {
        using Os.SubSurfaceVector subs = surf.subSurfaces();
        string host = surf.nameString();
        for (int i = 0; i < subs.Count; i++) {
            using Os.SubSurface sub = subs[i];
            if (!EnergyClassRows.ByOpeningType.TryGetValue(sub.subSurfaceType(), out IfcClass @class)) { warnings++; continue; }
            using Os.Point3dVector vertices = sub.vertices();
            (delta, NodeId openingId) = Opening(delta, spaceId, host, @class, sub.nameString(), OsmRing(vertices), ctx);
            delta = OsmComposition(model, sub, openingId, delta, ctx);
        }
        return delta;
    }

    static FootprintPolygon OsmRing(Os.Point3dVector vertices) {
        var ring = Seq<Vector3>();
        for (int i = 0; i < vertices.Count; i++) {
            using Os.Point3d p = vertices[i];
            ring = ring.Add(new Vector3(p.x(), p.y(), p.z()));
        }
        return new FootprintPolygon(ring);
    }

    // ONE composition fold over the PlanarSurface construction — a base Surface's opaque wall and a SubSurface's
    // glazing resolve through the SAME layer read; a layer neither typed re-read resolves degrades the WHOLE set
    // to the assembly uFactor evidence + a warning, never a fabricated layer set.
    GraphDelta OsmComposition(Os.Model model, Os.PlanarSurface surf, NodeId nodeId, GraphDelta delta, ProjectionContext ctx) {
        using Os.OptionalConstructionBase cb = surf.construction();
        if (!cb.is_initialized()) { return delta; }
        using Os.ConstructionBase constructionBase = cb.get();
        using Os.OptionalConstruction layered = model.getConstruction(constructionBase.handle());
        if (!layered.is_initialized()) { warnings++; return delta; }
        using Os.Construction construction = layered.get();
        using Os.MaterialVector layers = construction.layers();
        var rows = Seq<(Node.Material Node, MaterialLayer Layer)>();
        for (int i = 0; i < layers.Count; i++) {
            using Os.Material element = layers[i];
            if (TypedLayer(model, element, ctx).Case is not (Node.Material node, MaterialLayer layer)) {
                warnings++;
                return UFactorEvidence(delta, nodeId, constructionBase, ctx);
            }
            rows = rows.Add((node, layer));
        }
        return MaterialComposition.OfLayerSet(rows.Map(static r => r.Layer), ctx.Key).Match(
            Succ: set => Associate(delta, nodeId,
                Some((Mint(constructionBase.nameString(), set, Seq<MaterialPropertySet>(), ctx.Header.Tolerance), rows.Map(static r => r.Node)))),
            Fail: _ => { warnings++; return UFactorEvidence(delta, nodeId, constructionBase, ctx); });
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
                .OfThermal(m.conductivity(), m.specificHeat(), m.conductivity() / m.thickness(), 1.0, ctx.Key)
                .Map(thermal => (
                    Mint(m.nameString(), MaterialComposition.OfSingle(MaterialId.Create(m.nameString())), Seq(thermal), ctx.Header.Tolerance),
                    new MaterialLayer(MaterialId.Create(m.nameString()), MeasureValue.OfSi(Dimension.LengthDim, m.thickness()), m.nameString())))
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
            .Map(optical => (
                Mint(g.nameString(), MaterialComposition.OfSingle(MaterialId.Create(g.nameString())), Seq(optical), ctx.Header.Tolerance),
                new MaterialLayer(MaterialId.Create(g.nameString()), MeasureValue.OfSi(Dimension.LengthDim, g.thickness()), g.nameString())));
    }

    // Read a SWIG OptionalDouble onto the K-KINDED Option slot and DISPOSE the native handle (the getter's optional
    // is itself disposable) — the one lowering a missing OSM field takes, never a faulting get(). K<Option, double>,
    // NOT Option<double>: the shipped tuple Apply binds only on (K<F,A>, …) receivers (decompile-verified 5.0.0-beta-77,
    // no concrete-carrier tuple overload exists), and a concrete Option tuple neither infers nor converts at the receiver.
    static K<Option, double> Opt(Os.OptionalDouble optional) { using (optional) { return optional.is_initialized() ? Some(optional.get()) : None; } }

    // --- [SHARED_MINTS]
    // The energy boundary payload keys: BoundaryLevel is the SemanticProjector-owned attr (ONE symbol across both
    // projectors — a re-spelled literal is the seam-drift defect), BoundaryCondition/Host/StoryMultiplier are
    // energy-raise-owned and the lower reads the same statics back.
    internal static readonly PropertyName BoundaryCondition = PropertyName.Create("BoundaryCondition");
    internal static readonly PropertyName Host = PropertyName.Create("Host");
    internal static readonly PropertyName StoryMultiplier = PropertyName.Create("StoryMultiplier");

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

    static Relationship Boundary(NodeId space, NodeId surface, string condition) =>
        new Relationship.Generic(IfcRelKind.SpaceBoundary.Key, space, surface, Map(
            (SemanticProjector.BoundaryLevel, (PropertyValue)new PropertyValue.Text("2nd")),
            (BoundaryCondition,               (PropertyValue)new PropertyValue.Text(condition))));

    // An aperture/door IS a space boundary in energy modeling (the IfcRelSpaceBoundary related element may be a
    // window/door), so the opening joins the SPACE by the same edge shape with a Host correlation attribute — the
    // typed Void/fill lowering demands an IfcOpeningElement intermediary no energy schema carries, the rejected form.
    // Returns the minted id so the caller associates the opening's own glazing composition.
    (GraphDelta Delta, NodeId Id) Opening(GraphDelta delta, NodeId space, string hostIdentifier, IfcClass @class, string identifier, FootprintPolygon ring, ProjectionContext ctx) {
        NodeId openingId = NodeId.Rooted();
        openings++;
        return (delta
            .Put(Element(openingId, @class, "", identifier, Footprint(ring, ctx.Header.Tolerance)))
            .Link(new Relationship.Generic(IfcRelKind.SpaceBoundary.Key, space, openingId, Map(
                (SemanticProjector.BoundaryLevel, (PropertyValue)new PropertyValue.Text("2nd")),
                (Host,                            (PropertyValue)new PropertyValue.Text(hostIdentifier))))), openingId);
    }

    // The one composition landing: layer nodes + the LayerSet node + the Associate edge, the constructions tally
    // riding the Some arm — the per-arm Match copy is the deleted repeated form.
    GraphDelta Associate(GraphDelta delta, NodeId subject, Option<(Node.Material LayerSetNode, Seq<Node.Material> Layers)> set) =>
        set.Match(
            Some: s => {
                constructions++;
                return s.Layers.Fold(delta.Put(s.LayerSetNode), static (d, n) => d.Put(n))
                    .Link(new Relationship.Associate(subject, s.LayerSetNode.Id, new MaterialUsage.None()));
            },
            None: () => delta);

    // Content-keyed bag node + Assign(PropertyDefinition) — the one evidence-landing shape the uFactor degrade,
    // the dragonfly height quantity, and the storey-multiplier row share.
    static GraphDelta Assigned(GraphDelta delta, NodeId subject, Node node) =>
        delta.Put(node).Link(new Relationship.Assign(subject, node.Id, AssignKind.PropertyDefinition));

    static FootprintPolygon Ring(List<List<double>> boundary) =>
        new(toSeq(boundary).Map(static p => new Vector3(p[0], p[1], p.Count > 2 ? p[2] : 0.0)));

    // The footprint blob key rides the seam analytical byte projection (the ONE CanonicalWriter layout the
    // SemanticProjector [M2] mint owns — a page-local byte order is the cross-projector divergence defect);
    // the ring is recorded for the caller's write-blob-first landing.
    UInt128 Footprint(FootprintPolygon ring, double tolerance) {
        var writer = new CanonicalWriter(tolerance);
        foreach (Vector3 p in ring.Ring) { writer = writer.Double(p.X).Double(p.Y).Double(p.Z); }
        UInt128 key = ContentHash.Of(writer.ToBytes().Span);
        footprints.Add((key, ring));
        return key;
    }

    // The degrade row: no typed layer read -> the assembly uFactor lands as bag evidence the lower/review
    // reads, never a fabricated layer set (PropertySource.Derived — computed evidence, not authored data).
    GraphDelta UFactorEvidence(GraphDelta delta, NodeId surfaceId, Os.ConstructionBase construction, ProjectionContext ctx) {
        using Os.OptionalDouble u = construction.uFactor();
        if (!u.is_initialized()) { return delta; }
        return MeasureValue.Of(u.get(), UnitsNet.Units.HeatTransferCoefficientUnit.WattPerSquareMeterKelvin, ctx.Key).Match(
            Fail: _ => { warnings++; return delta; },
            Succ: uValue => {
                PropertyBag bag = new("Pset_EnergyModel", Map(
                    (PropertyName.Create("UFactor"),          (PropertyValue)new PropertyValue.Measure(uValue)),
                    (PropertyName.Create("ConstructionName"), (PropertyValue)new PropertyValue.Text(construction.nameString()))),
                    InheritanceMode.OccurrenceWins, PropertySource.Derived);
                Node.PropertySet probe = new(NodeId.Content([]), bag);
                return Assigned(delta, surfaceId, new Node.PropertySet(NodeId.Content(probe.ToCanonicalBytes(ctx.Header.Tolerance).Span), bag));
            });
    }
}
```

## [03]-[RESEARCH]

- [SCHEMA_STACK]: the managed decode grounds against the folder catalogs and the decompile tier — `HoneybeeSchema 2.102.0` (`.api/api-honeybee-schema`): `Model.FromJson(string)`, `Room(identifier, List<Face>, RoomPropertiesAbridged, …)`, `Face(identifier, Face3D, FaceType, AnyOf<Ground,Outdoors,Adiabatic,Surface,OtherSideTemperature>, FacePropertiesAbridged, …)`, `Aperture`/`Door` with their `AperturePropertiesAbridged(energy:)`/`DoorEnergyPropertiesAbridged.Construction` abridged window-construction references, `EnergyMaterial(identifier, thickness, conductivity, density, specificHeat, …)`, `EnergyWindowMaterialGlazing(identifier, …, thickness, solarTransmittance, solarReflectance, AnyOf<Autocalculate,double> solarReflectanceBack, visibleTransmittance, visibleReflectance, AnyOf<Autocalculate,double> visibleReflectanceBack, infraredTransmittance, emissivity, emissivityBack, conductivity, …)`, `OpaqueConstructionAbridged(identifier, List<string> materials, …)`/`WindowConstructionAbridged(identifier, List<string> materials, …)`, `IsValid(bool)` (`FromJson` gates through `IsValid(throwException: true)` — null on `"type"` mismatch, `ArgumentException` on a DataAnnotations reject), and the `AnyOf.Obj` boxed read — all decompile-verified (`assay api query --key openstudio`/`honeybee-schema`); `DragonflySchema 2.201.0` (`.api/api-dragonfly-schema`): `Model(identifier, ModelProperties, …, List<Building>, …)`, `Building(identifier, BuildingPropertiesAbridged, …, List<Story> uniqueStories, List<Room> room3ds, …)`, `Story(identifier, List<Room2D>, StoryPropertiesAbridged, …, int multiplier = 1, …)`, `Room2D(identifier, List<List<double>> floorBoundary, double floorHeight, double floorToCeilingHeight, Room2DPropertiesAbridged, …)` — dragonfly composes the honeybee vocabulary by identifier (`[HONEYBEE_STACK_LAW]`), so the massing arm re-declares nothing; `FloorToCeilingHeight` and `Multiplier` are the two source columns the massing raise preserves as seam evidence.
- [OSM_SURFACE]: the OSM raise grounds against the OpenStudio 3.11.0 decompile — `VersionTranslator.loadModelFromString(string) → OptionalModel` (the version-upgrading in-string read), `GbXMLReverseTranslator.loadModel(Path)`/`EnergyPlusReverseTranslator.loadModel(Path)` the path-bound reverse readers; the model read is `Model.getSpaces() → SpaceVector`, the `Space.surfaces` PROPERTY (not a method), `PlanarSurface.vertices() → Point3dVector`/`.construction() → OptionalConstructionBase` (shared by `Surface` AND `SubSurface`, which is why one composition fold serves both), `Surface.surfaceType()`/`outsideBoundaryCondition()` strings, `Surface.subSurfaces() → SubSurfaceVector` and `SubSurface.subSurfaceType()` over the `validSubSurfaceTypeValues` roster (`FixedWindow`/`OperableWindow`/`Door`/`GlassDoor`/`OverheadDoor`/`Skylight`/`TubularDaylightDome`/`TubularDaylightDiffuser`), `Construction.layers() → MaterialVector`, and the typed-downcast pair `Model.getStandardOpaqueMaterial(UUID)`/`Model.getStandardGlazing(UUID)` over `IdfObject.handle()` — the SWIG binding constructs a vector element at its STATIC type with no `to_*` cast surface, so the handle re-read is the ONLY typed layer read and a C# `as StandardOpaqueMaterial` downcast is the verified-absent phantom; `StandardOpaqueMaterial.conductivity()/specificHeat()/thickness()` and the defaulted-IDD `StandardGlazing` IR trio (`infraredTransmittanceatNormalIncidence()`/`front|backSideInfraredHemisphericalEmissivity()`) are plain doubles, while the SIX normal-incidence transmittance/reflectance getters (`solar|visible` × `Transmittance|frontSideReflectance|backSideReflectance` `atNormalIncidence`) return `OptionalDouble` — the plain `solarTransmittance()` sibling reads the SAME optional IDD field and THROWS natively when a spectral-data glazing leaves it unset, so the raise reads only the optional forms and an unset optional degrades the layer, never a fabricated zero fraction and never a native throw.
- [SEAM_ALIGNMENT]: the raise lands the exact contract `csharp:Rasm.Compute/Analysis/energy` `EnergyGraphReads` consumes — `IfcSpace` classification codes, `IfcRelSpaceBoundary`-named `Generic` edges with the three-valued `BoundaryLevel` payload (this page stamps `"2nd"`: a honeybee/OSM face is a per-space bounded surface, the second-level shape the Compute read prefers when both levels exist), the `Host` opening-correlation attribute (`BoundingSurfacesOf` excludes Host-bearing edges, `OpeningsOf` joins them onto typed OSM `SubSurface`s — a window never simulates as an opaque base surface), `Representations.FootPrint` content keys the seam `GeometrySource` resolves, and the `Qto_SpaceBaseQuantities` bags the Compute floor-area and the derive height reads consume — so raised-then-simulated needs zero adapter and the alignment is the seam row `Energy/projector ⇄ csharp:Rasm.Compute/Analysis`, never a reference; the footprint blob layout rides the seam `Rasm.Element/Projection/address#CANONICAL_WRITER` projection the `Projection/semantic` [M2] mint owns, and the raise returns the `(key, ring)` pairs so the caller lands blobs WRITE-BLOB-FIRST before the delta applies; the opaque layer evidence (`Thermal` conductivity/specific-heat, no density column, 1000 kg/m³ Compute fallback) and the glazing evidence (`Optical` nine fractions, no fabricated Thermal) mirror exactly what the Compute `BuildConstruction`/`Layer` fold reads back, so the two derivations of one graph agree on the layer physics by construction.
