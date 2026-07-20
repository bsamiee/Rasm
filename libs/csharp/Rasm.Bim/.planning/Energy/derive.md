# [BIM_ENERGY_DERIVE]

`EnergyDerive` lowers the seam `ElementGraph` to two authoring schemas, and `EnergyTranslate` runs the OpenStudio translator matrix as one frozen `(source, target)` row table. `IfcSpace` nodes — landed by the `Projection/semantic#SEMANTIC_PROJECTOR` IFC ingest or any projector — fold to honeybee `Room`/`Face`/`Aperture`/`Door` envelopes with their layered seam composition lowered onto the energy library under the abridged-reference law, or to dragonfly `Story`/`Room2D` massing plates. `Energy/exchange#ENERGY_EXCHANGE`'s `EnergyExchange.Apply` drives both arms and emits content-keyed `EnergyArtifact`s; class correspondence, boundary payload statics, and evidence bags read back from the `Energy/projector#ENERGY_PROJECTOR` owner, so the raise and the lower cannot drift.

Wire posture is HOST-LOCAL, foreign types emit-confined: each lowered `Hb.Model`/`Df.Model` is authored, `Validate()`-tallied, serialized through `ToJson()`, and released inside the arm; every `OpenStudio.*` SWIG wrapper is `using`-bracketed, and the translate temp-path crossings (`Workspace.save`/`Model.save` over a bracketed scratch file) are the platform-forced statement seam. Faults route the `Model/faults#FAULT_BAND` arms: `CodecReject` (`energy-lower-unsupported`/`energy-translate-miss`), `CapabilityMiss` (`energy-graph-egress-pending`), `ModelRejected` (`energy-decode` on a translate source).

## [01]-[INDEX]

- [01]-[MODEL_DERIVE]: the `EnergyDerive` lower fold — graph → honeybee envelope + energy library (opaque AND glazing constructions, opening sub-faces with their own constructions) over the seam `GeometrySource` port, graph → dragonfly massing with the height/multiplier evidence read back.
- [02]-[TRANSLATE_MATRIX]: the `EnergyTranslate` OSM-centric translator matrix (osm↔gbxml, osm↔idf, osm version-upgrade) as frozen row data over the OpenStudio translators.

## [02]-[MODEL_DERIVE]

- Owner: `EnergyDerive` the BIM-to-BEM lower fold (graph → honeybee HBJSON envelope + energy library, graph → dragonfly DFJSON massing).
- Entry: `EnergyDerive.Lower(ElementGraph graph, InterchangeFormat target, EnergyScope scope, GeometrySource geometry, Instant at, Op key)` → `Fin<EnergyOutcome.Emitted>` — dispatches the frozen `Lowers` target table: the `hbjson` arm lowers each scoped `IfcSpace` and its opening sub-faces onto the honeybee envelope + energy library, the `dfjson` arm folds the `Compose` tree onto dragonfly massing plates; each surface and opening composition lowers through ONE property-case fold.
- Auto: lowered models carry the SEMANTIC envelope and library only; simulation context — parameters, run period, conditioning, weather — is Compute's or the python recipe plane's, never authored on the lower.
- Receipt: one `EnergyReceipt` per emit tallies the folded spaces, surfaces, openings, and constructions; the model's `Validate()` DataAnnotations fold into `Warnings` beside the degrade tallies, never an exception.
- Packages: HoneybeeSchema, DragonflySchema, Rasm.Element, Rasm, LanguageExt.Core, NodaTime
- Growth: a new lower target is one row on the frozen `Lowers` target table (the `EnergyProjector.Arms`/`EnergyTranslate.Matrix` row law); per-space program/loads lower as `ProgramTypeAbridged` rows once the seam carries occupancy evidence; a NoMass R-value lower is one arm row the moment the seam carries an R-value-only thermal case; the space-adjacency `Surface` boundary condition is one arm row the moment the seam carries a second-level adjacency payload naming the counterpart face.
- Boundary: `EnergyDerive` reads the graph through seam-owned surfaces and the `Model/query#ELEMENT_SET` scope algebra; Compute is a peer stratum, never a dependency, so its discipline reads (`SpacesOf`/`BoundingSurfacesOf`) are never referenced. Every missing- or ambiguous-evidence path degrades warning-counted — a footprint-less space, a material lacking both the `Thermal` and `Optical` case — never a zero-area fabrication or a fabricated physics row. `graph→OSM`/`gbXML`/`IDF` DIRECT egress is deliberately absent: no in-process HBJSON→OSM translation is admitted (the python peer's `honeybee-openstudio` leg owns it), and a second graph→OSM builder beside Compute's simulation-scoped `BuildModel` is the duplicate-fold defect, so the request rails `BimFault.CapabilityMiss`. Glazing lowering consumes the same seam `Optical` case (`Discipline.Energy`) Compute's `StandardGlazing` build reads, so the lowered honeybee document and Compute's OSM model agree on layer physics by construction.

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

namespace Rasm.Bim;

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

    // Openings of one face: the space's boundary edges whose Host attribute names the face identifier — a raise
    // correlation idiom read back, never a NodeId join (rooted ids are raise-local). One read yields BOTH sub-face
    // lists (IfcWindow -> Aperture, IfcDoor -> Door), and each opening's own composition lowers through the SAME
    // discriminated fold so a raised window construction round-trips onto the aperture's abridged reference.
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
    // flattens onto a Room2D floor boundary — massing altitude only, no energy library, the honeybee shape
    // inverted; storey multiplier evidence reads back onto Story(multiplier:), so a unique-stories-x-repeat tower
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

- [SEAM_ALIGNMENT]-[OPEN]: do the Compute-side member spellings `BuildConstruction`, `StandardGlazing`, and the density-1000 fallback row still match the lower's seam reads; verify against `csharp:Rasm.Compute` `Runtime` energy-build pages when the Compute OSM build page next rebuilds.
