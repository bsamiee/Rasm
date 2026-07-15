# [BIM_ENERGY_DERIVE]

The BIM-to-BEM lower and the OSM-centric translation: `EnergyDerive` folds the seam `ElementGraph` down to the two managed authoring schemas — `IfcSpace` nodes (landed by the `Projection/semantic#SEMANTIC_PROJECTOR` IFC ingest or any other projector) fold to honeybee `Room`/`Face`/`Aperture`/`Door` envelopes with the layered seam composition lowered onto the energy library under the abridged-reference law (opaque `MaterialPropertySet.Thermal` layers onto `EnergyMaterial`/`OpaqueConstructionAbridged`, glazing `Optical` layers onto `EnergyWindowMaterialGlazing`/`WindowConstructionAbridged`), or to dragonfly `Story`/`Room2D` massing plates — and `EnergyTranslate` runs the OpenStudio translator matrix as one frozen `(source, target)` row table. Both arms are driven by the `Energy/exchange#ENERGY_EXCHANGE` `EnergyExchange.Apply` entry and emit content-keyed `EnergyArtifact`s; the class correspondence, the boundary payload statics, and the evidence bags are read back from the `Energy/projector#ENERGY_PROJECTOR` owner (`EnergyClassRows.ToFace`, `EnergyProjector.BoundaryCondition`/`Host`/`StoryMultiplier`), so the raise and the lower can never drift.

Wire posture: HOST-LOCAL, foreign types emit-confined. The lowered `Hb.Model`/`Df.Model` is authored, `Validate()`-tallied, serialized through `ToJson()`, and released inside the arm; every `OpenStudio.*` SWIG wrapper is `using`-bracketed, and the translate temp-path crossings (`Workspace.save`/`Model.save` path-bound emits over a bracketed scratch file) are the named platform-forced statement seam. Faults route the `Model/faults#FAULT_BAND` arms: `CodecReject` (`energy-lower-unsupported`/`energy-translate-miss`), `CapabilityMiss` (`energy-graph-egress-pending`), `ModelRejected` (`energy-decode` on a translate source).

## [01]-[INDEX]

- [01]-[MODEL_DERIVE]: the `EnergyDerive` lower fold — graph → honeybee envelope + energy library (opaque AND glazing constructions, opening sub-faces with their own constructions) over the seam `GeometrySource` port, graph → dragonfly massing with the height/multiplier evidence read back.
- [02]-[TRANSLATE_MATRIX]: the `EnergyTranslate` OSM-centric translator matrix (osm↔gbxml, osm↔idf, osm version-upgrade) as frozen row data over the OpenStudio translators.

## [02]-[MODEL_DERIVE]

- Owner: `EnergyDerive` the BIM-to-BEM lower fold (graph → honeybee HBJSON envelope + energy library, graph → dragonfly DFJSON massing).
- Entry: `EnergyDerive.Lower(ElementGraph graph, InterchangeFormat target, EnergyScope scope, GeometrySource geometry, Instant at, Op key)` → `Fin<EnergyOutcome.Emitted>` — the `hbjson` arm selects the `IfcSpace`-classified `Object` nodes under the scope, folds each space's `IfcRelSpaceBoundary` `Generic`-edged bounding surfaces into honeybee `Face`s (`Face3D` boundary from the seam `GeometrySource`-resolved `FootprintPolygon` — the same one-hop content-key resolve the Compute build takes; `FaceType` from the derived `EnergyClassRows.ToFace` index; the `Host`-attributed opening boundaries into `Aperture`/`Door` sub-faces carrying their OWN lowered constructions), lowers each surface's and opening's `MaterialComposition.LayerSet` through ONE property-case-discriminated fold — an `Optical`-free set onto `EnergyMaterial` + `OpaqueConstructionAbridged`, an all-`Optical` set onto `EnergyWindowMaterialGlazing` + `WindowConstructionAbridged`, a mixed set a warning-counted degrade (no legal EnergyPlus construction exists, the Compute `BuildConstruction` mixed rejection mirrored) — populated into the model store through the identifier-dedup `ModelEnergyProperties` extension `AddMaterial`/`AddConstruction` mutators, the construction id the content-hashed material-key join (the abridged form referenced from `FaceEnergyPropertiesAbridged`/`ApertureEnergyPropertiesAbridged`/`DoorEnergyPropertiesAbridged` — the abridged-reference law, never expand-then-reinline), assembles `new Hb.Model(identifier, properties, rooms:, units: Meters, tolerance: Header.Tolerance)` and emits `model.ToJson()`; the `dfjson` arm folds the `IfcBuilding`/`IfcBuildingStorey` `Compose` tree into `Building`/`Story` with each space's footprint ring flattened to a `Room2D` floor plate, the `Qto_SpaceBaseQuantities` `Height` quantity read back as the floor-to-ceiling height, and the `Pset_EnergyModel`/`StoryMultiplier` evidence read back onto `Story(multiplier:)`.
- Auto: the lowered model carries the SEMANTIC envelope and library only — no `SimulationParameter`, no run period, no conditioning, no weather (simulation context is Compute's or the python recipe plane's); boundary conditions derive from the boundary edge's `BoundaryCondition` payload where the raise stamped one (`Ground`/`Adiabatic`/`Outdoors` text → the `IBoundarycondition` case) and default `Outdoors` otherwise; the artifact content-key is the emitted-bytes derivation with `Graph = Some(ContentAddress.OfGraph(graph))` stamping the semantic pedigree, and the model IDENTIFIER is that same pedigree key (`rasm-energy-{key:x32}`) so a re-lowered identical graph emits byte-identical documents and the object-plane 412-noop dedup fires — a timestamp identifier forked the bytes per second, the deleted form.
- Receipt: one `EnergyReceipt` per emit — `Lowered` legs count the folded spaces/surfaces/openings/constructions; the lowered model's `Validate()` DataAnnotations results fold into `Warnings` beside the degrade tallies, never an exception.
- Packages: HoneybeeSchema, DragonflySchema, Rasm.Element, Rasm, LanguageExt.Core, NodaTime
- Growth: a new lower target is one row on the frozen `Lowers` target table (the `EnergyProjector.Arms`/`EnergyTranslate.Matrix` row law); per-space program/loads lower as `ProgramTypeAbridged` rows once the seam carries occupancy evidence; a NoMass R-value lower is one arm row the moment the seam carries an R-value-only thermal case; the space-adjacency `Surface` boundary condition is one arm row the moment the seam carries a second-level adjacency payload naming the counterpart face — until then the `Surface` text degrades `Outdoors` by the documented default.
- Boundary: the lower reads the graph through seam-owned surfaces (`ObjectNodes`, `EdgesAt`, `CompositionOf`, `Material`, the `GeometrySource` port) and the `Model/query#ELEMENT_SET` algebra for scope selection — a Compute-owned discipline read (`SpacesOf`/`BoundingSurfacesOf`) is never referenced (Compute is a peer stratum, not a dependency); a space whose footprint blob is absent lowers as a logged warning and a skipped room, never a zero-area fabrication; a layer-set material lacking BOTH the `Thermal` and `Optical` case degrades warning-counted, never a fabricated physics row; the density reads the seam `Mechanical` case when carried, the 1000 kg/m³ fallback mirroring the Compute OSM-build row (the `Thermal` case itself carries no density); the graph→OSM/gbXML/IDF DIRECT egress is deliberately absent — no in-process HBJSON→OSM translation is admitted (`honeybee-openstudio` is the python peer's leg) and a second graph→OSM builder beside Compute's simulation-scoped `BuildModel` would be the duplicate-fold defect, so the request rails `BimFault.CapabilityMiss` (`energy-graph-egress-pending`) and the capability lands as one matrix column when its translation is admitted.

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

namespace Rasm.Bim;

// --- [OPERATIONS] --------------------------------------------------------------------------
// The BIM-to-BEM lower: graph -> honeybee envelope + library (hbjson) or dragonfly massing (dfjson). The
// model store populates through the ModelEnergyProperties Add* mutators (the canonical lists), faces and
// openings reference by abridged id.
public static class EnergyDerive {
    // The frozen target→arm table (the EnergyTranslate.Matrix and EnergyProjector.Arms row law): a new lower
    // target is ONE row here, never a widened ternary chain — the miss splits CapabilityMiss (a raise-served form
    // whose graph egress is pending) from CodecReject (a form no energy arm serves).
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
        // The model identifier IS the graph pedigree: a timestamp identifier forked the emitted BYTES per second,
        // so a re-lowered identical graph never byte-matched and the object-plane 412-noop dedup never fired —
        // the content-stable identifier restores the reuse join the dual-key law states.
        ContentAddress pedigree = ContentAddress.OfGraph(graph);
        var store = new Hb.ModelEnergyProperties();
        var state = (Rooms: Seq<Hb.Room>(), Surfaces: 0, Openings: 0, Constructions: 0, Warnings: 0);
        foreach (Node.Object space in SpacesUnder(graph, scope)) {
            var faces = Seq<Hb.Face>();
            foreach ((Relationship.Generic edge, Node.Object surface) in Boundaries(graph, space.Id)) {
                if (geometry.Footprint(surface.Representations).Case is not FootprintPolygon ring) { state.Warnings++; continue; }
                if (!EnergyClassRows.ToFace.TryGetValue((surface.Classification.Code, surface.PredefinedType), out var faceType)) {
                    state.Warnings++; continue;
                }
                state.Surfaces++;
                Option<string> construction = LowerComposition(graph, surface.Id, store, ref state.Warnings)
                    .Do(_ => state.Constructions++);
                var (apertures, doors) = Openings(graph, space.Id, surface.ExternalId.IfNone(surface.Name), geometry, store, ref state.Openings, ref state.Warnings, ref state.Constructions);
                faces = faces.Add(new Hb.Face(
                    surface.ExternalId.IfNone(surface.Name), Face3D(ring), faceType,
                    Condition(edge), new Hb.FacePropertiesAbridged(
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

    // The glazing lower over the seam Optical case (Discipline.Energy): the nine [0,1] fractions map onto the
    // EnergyWindowMaterialGlazing columns 1:1, the Thermal conductivity rides when carried (the 0.9 schema
    // default otherwise — the one authoring default the schema itself declares, never fabricated physics).
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

    static Hb.Face3D Face3D(FootprintPolygon ring) =>
        new([.. ring.Ring.Map(static p => (List<double>)[p.X, p.Y, p.Z])]);

    // Boundary-condition derivation: the edge payload the raise stamped, defaulting Outdoors — the closed
    // IBoundarycondition set dispatched by text, never a downcast chain. The Surface adjacency case needs the
    // counterpart-face ids no seam payload carries yet — the documented Outdoors degrade, a growth arm row.
    static Hb.AnyOf<Hb.Ground, Hb.Outdoors, Hb.Adiabatic, Hb.Surface, Hb.OtherSideTemperature> Condition(Relationship.Generic edge) =>
        edge.Attributes.Find(EnergyProjector.BoundaryCondition)
            .Bind(static v => v is PropertyValue.Text t ? Some(t.Value) : None)
            .Match<Hb.AnyOf<Hb.Ground, Hb.Outdoors, Hb.Adiabatic, Hb.Surface, Hb.OtherSideTemperature>>(
                Some: static text => text switch {
                    "Ground"    => new Hb.Ground(),
                    "Adiabatic" => new Hb.Adiabatic(),
                    _           => new Hb.Outdoors(),
                },
                None: static () => new Hb.Outdoors());

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

    // The openings of one face: the space's boundary edges whose Host attribute names the face identifier —
    // the raise's correlation idiom read back, never a NodeId join (rooted ids are raise-local). One read
    // yields BOTH sub-face lists (IfcWindow -> Aperture, IfcDoor -> Door), and each opening's own composition
    // lowers through the SAME discriminated fold so a raised window construction round-trips onto the
    // aperture's abridged reference.
    static (List<Hb.Aperture> Apertures, List<Hb.Door> Doors) Openings(
        ElementGraph graph, NodeId space, string hostIdentifier, GeometrySource geometry, Hb.ModelEnergyProperties store,
        ref int openings, ref int warnings, ref int constructions) {
        var apertures = new List<Hb.Aperture>();
        var doors = new List<Hb.Door>();
        foreach (var opening in graph.EdgesAt(space).Choose(e =>
            e is Relationship.Generic g && g.WireName == IfcRelKind.SpaceBoundary.Key && g.Relating == space
                && g.Attributes.Find(EnergyProjector.Host).Exists(v => v is PropertyValue.Text t && t.Value == hostIdentifier)
                ? graph.Find<Node.Object>(g.Related) : None)) {
            if (opening.Classification.Code != IfcClass.Window.Key && opening.Classification.Code != IfcClass.Door.Key) { continue; }
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
    // flattens onto a Room2D floor boundary — massing altitude only, no energy library, the honeybee shape inverted;
    // the storey multiplier evidence reads back onto Story(multiplier:), so a unique-stories-x-repeat tower
    // round-trips its repeat factor.
    static Fin<EnergyOutcome.Emitted> Dragonfly(ElementGraph graph, EnergyScope scope, GeometrySource geometry, Instant at, Op key) {
        ContentAddress pedigree = ContentAddress.OfGraph(graph);   // the content-stable identifier + Graph pedigree, one derivation
        var buildings = Seq<Df.Building>();
        int spaces = 0, warnings = 0;
        foreach (Node.Object building in graph.ObjectNodes.Filter(o => o.Classification.Code == IfcClass.Building.Key)) {
            var stories = Seq<Df.Story>();
            foreach (Node.Object storey in Parts(graph, building.Id, IfcClass.BuildingStorey)) {
                var plates = Seq<Df.Room2D>();
                foreach (Node.Object space in Parts(graph, storey.Id, IfcClass.Space).Filter(s => InScope(s, scope))) {
                    if (geometry.Footprint(space.Representations).Case is not FootprintPolygon ring) { warnings++; continue; }
                    Seq<Vector3> plate = ring.Ring;
                    spaces++;
                    plates = plates.Add(new Df.Room2D(
                        space.ExternalId.IfNone(space.Name),
                        [.. plate.Map(static p => (List<double>)[p.X, p.Y])],
                        plate.Head.Map(static p => p.Z).IfNone(0.0),
                        Height(graph, space.Id).IfNone(DefaultFloorToCeiling),
                        new Df.Room2DPropertiesAbridged()));
                }
                if (!plates.IsEmpty) {
                    stories = stories.Add(new Df.Story(storey.ExternalId.IfNone(storey.Name), [.. plates],
                        new Df.StoryPropertiesAbridged(), multiplier: Multiplier(graph, storey.Id)));
                }
            }
            if (!stories.IsEmpty) {
                buildings = buildings.Add(new Df.Building(building.ExternalId.IfNone(building.Name),
                    new Df.BuildingPropertiesAbridged(), uniqueStories: [.. stories]));
            }
        }
        var model = new Df.Model($"rasm-massing-{pedigree.Value:x32}", new Df.ModelProperties(),
            buildings: [.. buildings], units: Df.Units.Meters, tolerance: graph.Header.Tolerance);
        warnings += model.Validate().Count();
        return Fin.Succ(Emit(InterchangeFormat.Dfjson, Encoding.UTF8.GetBytes(model.ToJson()), pedigree, at,
            new EnergyReceipt(EnergyLeg.Lowered, InterchangeFormat.Dfjson, None, spaces, 0, 0, 0, warnings, default, at)));
    }

    // The documented massing fallback when a space carries no Qto height — a named policy default (the Compute
    // density-1000 precedent), never a silent zero; a real height reads the Qto_SpaceBaseQuantities bag the
    // IFC ingest OR the dragonfly raise landed.
    const double DefaultFloorToCeiling = 3.0;

    static Option<double> Height(ElementGraph graph, NodeId space) =>
        graph.EdgesAt(space).Choose(e =>
            e is Relationship.Assign { SubKind: var k } a && k == AssignKind.PropertyDefinition && a.Subject == space
                ? graph.Find<Node.QuantitySet>(a.Definition) : None)
            .Filter(static qs => qs.Bag.SetName == "Qto_SpaceBaseQuantities").Head
            .Bind(static qs => qs.Bag.Values.Find(PropertyName.Create("Height")))
            .Bind(static m => m.Length);

    // The raise's StoryMultiplier evidence read back through the projector-owned symbol (never a re-spelled
    // literal); absent evidence is multiplier 1 — the dragonfly schema default.
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
- Entry: `EnergyTranslate.Run(EnergyDoc source, InterchangeFormat target, Instant at, Op key)` → `Fin<EnergyOutcome.Emitted>` resolves the `(source, target)` matrix row — `osm→gbxml` (`GbXMLForwardTranslator.modelToGbXMLString`), `osm→idf` (`EnergyPlusForwardTranslator.translateModel` + `Workspace.save`), `gbxml→osm`/`idf→osm` (the reverse readers + `Model.save`), `osm→osm` (the `VersionTranslator` version-upgrade row) — and emits the translated bytes as an `EnergyArtifact` (no graph pedigree — a translation never touched the graph) with the translator `warnings()`/`errors()` tallied into the `Translated` receipt.
- Packages: NREL.OpenStudio.macOS-arm64, Rasm, LanguageExt.Core, NodaTime
- Growth: a new translation is one `Matrix` row over a verified translator member (SDD via `SddForwardTranslator`/`SddReverseTranslator` is the named next row); the graph→OSM/gbXML egress lands as ONE matrix column fed by the lowered honeybee leg the moment an in-process HBJSON→OSM translation is admitted.
- Boundary: the translate temp-path crossings and the SWIG handle brackets are the named platform-forced statement seam; `Workspace.save`/`Model.save` path-bound emits cross a bracketed scratch file exactly as the decode arms do; a matrix miss rails `CodecReject` (`energy-translate-miss`), an unreadable source `ModelRejected` (`energy-decode`).

```csharp signature
// Shares the [02] RUNTIME_PRELUDE (the Os alias and the seam usings — one compilation unit per page).
// The OSM-centric translator matrix: (source, target) rows over verified OpenStudio members; a translation
// is one row, never a per-pair method family. Path-bound emits cross a bracketed scratch file.
public static class EnergyTranslate {
    static readonly FrozenDictionary<(InterchangeFormat Source, InterchangeFormat Target), Func<EnergyDoc, Op, Fin<(byte[] Bytes, int Warnings)>>> Matrix =
        new KeyValuePair<(InterchangeFormat, InterchangeFormat), Func<EnergyDoc, Op, Fin<(byte[], int)>>>[] {
            new((InterchangeFormat.Osm,   InterchangeFormat.GbXml), static (doc, key) => OsmTo(doc, key, static (model, tally) => {
                using Os.GbXMLForwardTranslator gb = new();
                string xml = gb.modelToGbXMLString(model);
                return (Encoding.UTF8.GetBytes(xml), tally + Tally(gb.warnings(), gb.errors()));
            })),
            new((InterchangeFormat.Osm,   InterchangeFormat.Idf),   static (doc, key) => OsmTo(doc, key, static (model, tally) => {
                using Os.EnergyPlusForwardTranslator ep = new();
                using Os.Workspace idf = ep.translateModel(model);
                return (Saved(w => idf.save(w, true)), tally + Tally(ep.warnings(), ep.errors()));
            })),
            new((InterchangeFormat.Osm,   InterchangeFormat.Osm),   static (doc, key) => OsmTo(doc, key, static (model, tally) =>
                (Saved(w => model.save(w, true)), tally))),   // the VersionTranslator upgrade row: decode already upgraded
            new((InterchangeFormat.GbXml, InterchangeFormat.Osm),   static (doc, key) => ReverseTo(doc, key)),
            new((InterchangeFormat.Idf,   InterchangeFormat.Osm),   static (doc, key) => ReverseTo(doc, key)),
        }.ToFrozenDictionary();

    internal static Fin<EnergyOutcome.Emitted> Run(EnergyDoc source, InterchangeFormat target, Instant at, Op key) =>
        Matrix.TryGetValue((source.Format, target), out var row)
            ? row(source, key).Map(result => {
                EnergyArtifact artifact = EnergyArtifact.Of(target, result.Bytes, None, at);
                return new EnergyOutcome.Emitted(artifact, new EnergyReceipt(
                    EnergyLeg.Translated, source.Format, Some(target), 0, 0, 0, 0, result.Warnings, artifact.ContentKey, at));
            })
            : Fin.Fail<EnergyOutcome.Emitted>(new BimFault.CodecReject(key, $"energy-translate-miss:{source.Format.Key}->{target.Key}"));

    // Decode-then-emit over the version-upgrading in-string read; the emit lambda owns its translator brackets.
    static Fin<(byte[], int)> OsmTo(EnergyDoc doc, Op key, Func<Os.Model, int, (byte[], int)> emit) {
        try {
            using Os.VersionTranslator vt = new();
            using Os.OptionalModel optional = vt.loadModelFromString(doc.Text);
            if (!optional.is_initialized()) {
                return Fin.Fail<(byte[], int)>(new BimFault.ModelRejected(key, "energy-decode:<osm-unreadable>"));
            }
            using Os.Model model = optional.get();
            return Fin.Succ(emit(model, Tally(vt.warnings(), vt.errors())));
        }
        catch (Exception ex) when (ex is SystemException or ApplicationException) {
            return Fin.Fail<(byte[], int)>(new BimFault.ModelRejected(key, $"energy-translate:{ex.Message}"));
        }
    }

    // gbXML/IDF -> OSM: the Path-bound reverse readers over a bracketed temp file, saved back as .osm bytes.
    static Fin<(byte[], int)> ReverseTo(EnergyDoc doc, Op key) {
        string temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try {
            File.WriteAllBytes(temp, doc.Bytes.ToArray());
            using Os.Path path = Os.OpenStudioUtilitiesCore.toPath(temp);
            if (doc.Format == InterchangeFormat.GbXml) {
                using Os.GbXMLReverseTranslator gb = new();
                using Os.OptionalModel fromGb = gb.loadModel(path);
                return fromGb.is_initialized()
                    ? Save(fromGb.get(), Tally(gb.warnings(), gb.errors()))
                    : Fin.Fail<(byte[], int)>(new BimFault.ModelRejected(key, "energy-decode:<gbxml-unreadable>"));
            }
            using Os.EnergyPlusReverseTranslator ep = new();
            using Os.OptionalModel fromIdf = ep.loadModel(path);
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

- [AUTHORING_STACK]: the lower authors against the decompile-verified honeybee/dragonfly surface — the required-`properties` `Model` ctor (`identifier, ModelProperties, …, Units units = Meters, double tolerance = 0.01`), `Room(identifier, List<Face>, RoomPropertiesAbridged, …)`, `Face(identifier, Face3D, FaceType, AnyOf<Ground,Outdoors,Adiabatic,Surface,OtherSideTemperature>, FacePropertiesAbridged, …)`, `Aperture(identifier, Face3D, AnyOf<Outdoors,Surface>, AperturePropertiesAbridged, …)`/`Door(…, DoorPropertiesAbridged, …)` with `ApertureEnergyPropertiesAbridged(string construction = null, …)`/`DoorEnergyPropertiesAbridged(string construction = null, …)`, `Face3D(List<List<double>> boundary, …)`, `EnergyMaterial(identifier, thickness, conductivity, density, specificHeat, …)`, `EnergyWindowMaterialGlazing(identifier, …, thickness, solarTransmittance, solarReflectance, solarReflectanceBack, visibleTransmittance, visibleReflectance, visibleReflectanceBack, infraredTransmittance, emissivity, emissivityBack, conductivity, …)` (the `AnyOf<Autocalculate,double>` back-reflectance slots take the double through the generated implicit conversion), `OpaqueConstructionAbridged(identifier, List<string>)`/`WindowConstructionAbridged(identifier, List<string>)` (both `IConstruction`, the glazing an `IMaterial`, so the identifier-dedup `ModelEnergyProperties`-extension `AddMaterial`/`AddConstruction` mutators admit both families through one call), `Validate()` the authoring-side DataAnnotations tally, and `Story(identifier, List<Room2D>, StoryPropertiesAbridged, …, int multiplier = 1, …)`/`Room2D(identifier, floorBoundary, floorHeight, floorToCeilingHeight, Room2DPropertiesAbridged, …)` on the dragonfly leg.
- [TRANSLATOR_MATRIX]: the OSM-family members ground against the OpenStudio 3.11.0 decompile — `VersionTranslator.loadModelFromString(string) → OptionalModel` (the version-upgrading in-string read; `loadModel(Path)`/`originalVersion()` beside it), `GbXMLForwardTranslator.modelToGbXML(Model, Path) → bool` / `modelToGbXMLString(Model) → string`, `EnergyPlusForwardTranslator.translateModel(Model) → Workspace`, `GbXMLReverseTranslator.loadModel(Path) → OptionalModel`, `EnergyPlusReverseTranslator.loadModel(Path) → OptionalModel` / `translateWorkspace(Workspace) → Model`, every translator carrying `warnings()`/`errors()` `LogMessageVector`, and the `Workspace.save`/`Model.save(Path, bool overwrite)` path-bound emits — the SWIG `Path` is built through `OpenStudioUtilitiesCore.toPath` (no `Path(string)` ctor exists).
- [SEAM_ALIGNMENT]: the lower reads `CompositionOf`/`Material`/`MaterialPropertySet.Thermal` plus the `Mechanical` density (1000 kg/m³ fallback) exactly as the Compute OSM build does, and the glazing lower reads the SAME seam `Optical` case (`Discipline.Energy`) the Compute `StandardGlazing` build consumes — so the honeybee document a graph lowers and the OSM model Compute builds from that graph agree on the layer physics by construction, and a mixed opaque+glazing set degrades identically on both sides (Compute rails, the lower warns) rather than authoring a construction EnergyPlus rejects; the `Qto_SpaceBaseQuantities` `Height` and `Pset_EnergyModel`/`StoryMultiplier` reads consume the evidence the `Energy/projector` dragonfly arm lands, closing the DFJSON round trip; the graph→OSM/gbXML/IDF DIRECT egress stays deliberately absent — `honeybee-openstudio` is the python peer's leg, and a second graph→OSM builder beside Compute's simulation-scoped `BuildModel` would be the duplicate-fold defect.
