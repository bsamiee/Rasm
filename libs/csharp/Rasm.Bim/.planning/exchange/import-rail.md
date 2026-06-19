# [BIM_IMPORT_RAIL]

The foreign-bytes ingest rail: one `BimIo` import fold over the `format-axis#FORMAT_AXIS` `InterchangeFormat` rows, dispatching the managed glTF/GLB mesh-and-scene decode through SharpGLTF, the STL/3MF/OBJ/PLY mesh-text arm, the in-process semantic IFC/IFC5/STEP graph ingest through GeometryGym, the AP242/native-companion two-hop route, and the Speckle `Base` object-graph seam folding a deserialized `Speckle.Sdk.Models.Base` tree onto the same `ImportedGeometry`/`IfcSemanticModel` carriers — never tessellated BRep. The page composes the kernel `Rasm` geometry and consumes the `format-axis#FORMAT_AXIS` codec/frame rows as settled vocabulary; an IFC/native/Speckle-non-mesh geometry request routes to `tessellation-bridge#TESSELLATION_BRIDGE`. The page is HOST-LOCAL in posture; the Speckle seam composes `Speckle.Sdk`/`Speckle.Objects` and runs only in the host-neutral exchange assembly, never inside the in-Rhino plugin ALC.

## [1]-[INDEX]

- [2]-[IMPORT_RAIL]: foreign-bytes ingest — managed mesh decode and in-process semantic IFC/IFC5/STEP graph.
- [3]-[SPECKLE_SEAM]: Speckle `Base` object-graph fold — display-mesh decode and host-object semantic projection onto the canonical carriers.

## [2]-[IMPORT_RAIL]

- Owner: `BimIo` — the import fold over `InterchangeFormat`, dispatching the managed glTF/GLB mesh-and-scene decode through SharpGLTF, the STL/3MF/OBJ/PLY mesh-text arm that faults until its reader package catalogues, the in-process semantic IFC/IFC5 ingest through GeometryGym over `DatabaseIfc`/`Extract<T>`, and the AP242/native-companion two-hop route; `ImportedGeometry` the decoded mesh-scene carrier, `IfcSemanticModel` the IFC model-graph projection.
- Entry: `BimIo.ImportGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks)` for the managed glTF and mesh-text path; `BimIo.ImportIfc(...)` for the in-process IFC/IFC5 semantic graph — `Fin<T>` aborts on a codec reject (`faults#FAULT_BAND` `BimFault.CodecReject`) or a companion-required geometry request (`BimFault.CapabilityMiss`), each lowered with `.ToError()`, the foreign decode arity discriminating on the row's `InterchangeCodec` so a path lands one decode without a call-site type branch, projecting the package exception onto `BimFault.ModelRejected` at the boundary so domain code never sees the SharpGLTF `ModelException` or the GeometryGym parse fault.
- Auto: binary GLB decode lands through `ModelRoot.ParseGLB(ArraySegment<byte>)` and text `.gltf` decode through `ReadContext.ReadTextSchema2(Stream)` then `model.LogicalMeshes.Decode()` projecting `IMeshDecoder<Material>` primitives to `ImportedGeometry` vertex and index spans with zero intermediate file; the IFC semantic path constructs a `DatabaseIfc` over the bytes through `DatabaseIfc.ParseString`/`ReadXMLDoc`/`ReadJSON` by the row's format, narrows `db.Project`, and folds `db.Project.Extract<T>()` collecting spatial hierarchy (`IfcSpatialStructureElement`), products (`IfcProduct`, splitting the predefined-type token through `ParserIfc.IdentifyIfcClass(name, out predefinedConstant)` and carrying the `IfcObject.ObjectType` user-defined fallback), property sets (`IfcPropertySet`), quantities (`IfcElementQuantity`), materials (`IfcRelAssociatesMaterial`), classification associations (`IfcRelAssociatesClassification`), type objects (`IfcTypeObject`, widened to carry the type-bound `HasPropertySets` property family, the type materials, and the `IfcTypeProduct.RepresentationMaps` instanced-geometry content key the `model/elements#BIM_TYPE` `BimType` reads), grouping/zone overlays (`IfcGroup`/`IfcZone`/`IfcSpatialZone` with their `IsGroupedBy` `IfcRelAssignsToGroup` member sets the `zoning/grouping#ZONE_GRAPH` overlay folds), the map-conversion projection (`IfcGeometricRepresentationContext.HasCoordinateOperation` → `IfcMapConversion`/`IfcProjectedCRS` the `georeferencing/coordinate-reference#GEO_REFERENCE` `Project` reads), and decomposition relationships (`IfcRelDecomposes`) into the `IfcSemanticModel` graph — never tessellated BRep.
- Receipt: the `ModelLoad` receipt case carries the format key, codec key, source byte count, and elapsed for a managed mesh import; an IFC semantic ingest stamps the schema version (`db.Release`), the model-view (`db.ModelView`), and the extracted-entity counts; emission rides the sink port at the composition edge.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Runtime, GeometryGymIFC_Core, NodaTime, LanguageExt.Core, Rasm
- Growth: a new managed import is one codec arm on the import fold keyed by the `InterchangeFormat.Codec` row; a new extracted IFC entity family is one `Extract<T>` projection on `IfcSemanticModel`; a new STEP application protocol is one `InterchangeFormat` row carrying its `StepProtocol` discriminant — the `step-iso10303` codec reads the protocol column to select the entity-instance vocabulary version so a single STEP reader spans all three without a per-protocol codec.
- Boundary: `BimIo` is the page boundary capsule and its codec arms carry the language-owned statement forms the foreign package decode requires; glTF mesh decode rides the `MeshDecoder.Decode` runtime contract reading `IMeshPrimitiveDecoder.GetPosition`/`GetNormal`/`TriangleIndices` (an accessor-based contract returning per-vertex `Vector3`/index-tuple values, so the decode materializes one contiguous `ImportedGeometry` vertex/normal/index triple at the boundary — the accessor contract admits no zero-copy span into SharpGLTF's internal buffers, so the one boundary materialization, not a per-primitive `float[]` proliferation, is the allocation point); the IFC semantic graph is a model-data projection only — `BaseClassIfc.Extract<T>` collects reachable entities and GeometryGym carries no tessellation kernel, so a geometry request on an IFC row routes to the `tessellation-bridge#TESSELLATION_BRIDGE` rail and never evaluates a BRep in-process; the `step-iso10303` STEP solid-model path reads the entity-instance graph at the protocol the `StepProtocol` column names (the AP203/AP214/AP242 EXPRESS vocabularies share the STEP physical-file token grammar) and routes its B-rep/NURBS evaluation through the same companion rail because managed STEP solid evaluation has no in-process kernel; `DatabaseIfc.Tolerance`/`ToleranceAngleRadians`/`ScaleSI` read the model precision the content-key folds; the SharpGLTF `ReadSettings.Validation` rides `ValidationMode.Strict` on import so a malformed asset faults at parse; a candidate `fbx`/`dae` import faults at the boundary with the `import-catalogue-pending` rail naming the admitting package because the format is enumerable and detectable but its codec body is unwritten until the catalogue lands; an `IfcImporter`/`GltfImporter` service family and a managed IFC tessellator are the deleted forms.

```csharp signature
public sealed record ImportedGeometry(
    InterchangeFormat Format,
    ReadOnlyMemory<float> Vertices,
    ReadOnlyMemory<float> Normals,
    ReadOnlyMemory<long> Indices,
    int VertexCount,
    int TriangleCount,
    Instant At);

public sealed record IfcSemanticModel(
    ReleaseVersion Schema,
    ModelView View,
    Seq<IfcSemanticModel.SpatialNode> Spatial,
    Seq<IfcSemanticModel.ProductRow> Products,
    Seq<IfcSemanticModel.PropertyRow> Properties,
    Seq<IfcSemanticModel.QuantityRow> Quantities,
    Seq<IfcSemanticModel.MaterialRow> Materials,
    Seq<IfcSemanticModel.ClassificationRow> Classifications,
    Seq<IfcSemanticModel.TypeRow> Types,
    Seq<IfcSemanticModel.ZoneRow> Zones,
    Seq<AssemblyRel> Decomposition,
    Option<IfcSemanticModel.MapConversionRow> MapConversion,
    double Tolerance,
    Instant At) {
    public sealed record SpatialNode(string GlobalId, string EntityType, string Name, string LongName, Seq<string> ContainedGlobalIds);
    public sealed record ProductRow(string GlobalId, string EntityType, string Name, string Tag, string PredefinedType, string ObjectType, Option<string> TypeGlobalId);
    public sealed record PropertyRow(string OwnerGlobalId, string SetName, string PropertyName, string Value);
    public sealed record QuantityRow(string OwnerGlobalId, string SetName, string QuantityName, double Value, string Unit);
    public sealed record MaterialRow(string OwnerGlobalId, string MaterialName);
    public sealed record ClassificationRow(string OwnerGlobalId, string System, string Code, string DictionaryClassUri);
    public sealed record TypeRow(
        string GlobalId, string EntityType, string Name, string PredefinedType,
        Seq<IfcSemanticModel.PropertyRow> Properties, Seq<IfcSemanticModel.MaterialRow> Materials, Option<UInt128> RepresentationMapKey);
    public sealed record ZoneRow(string GlobalId, string EntityType, string Name, string PredefinedType, Seq<string> AssignedGlobalIds);
    public sealed record MapConversionRow(
        double Eastings, double Northings, double OrthogonalHeight,
        double XAxisAbscissa, double XAxisOrdinate, double Scale,
        string SourceCrsName, string TargetCrsName, string GeodeticDatum, string MapProjection, string MapZone);
}

public static partial class BimIo {
    public static Fin<ImportedGeometry> ImportGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks) =>
        !format.CanImport ? Fin.Fail<ImportedGeometry>(new BimFault.CodecReject($"import-unsupported:{format.Key}").ToError())
        : format.CataloguePending ? Fin.Fail<ImportedGeometry>(new BimFault.CodecReject($"import-catalogue-pending:{format.Key}:{format.Codec.CataloguePackage.IfNone("unknown")}").ToError())
        : format.Codec == InterchangeCodec.SharpGltf ? Boundary(() => Framed(format, Gltf(format, bytes, clocks.Now)))
        : Fin.Fail<ImportedGeometry>(new BimFault.CapabilityMiss($"import-needs-companion:{format.Key}").ToError());

    public static Fin<IfcSemanticModel> ImportIfc(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks) =>
        format.Codec == InterchangeCodec.GeometryGym
            ? Boundary(() => Semantic(Database(format, bytes), clocks.Now))
            : Fin.Fail<IfcSemanticModel>(new BimFault.CodecReject($"ifc-codec-miss:{format.Key}").ToError());

    static Fin<T> Boundary<T>(Func<T> decode) =>
        Try.lift(decode).Run().MapFail(static error => new BimFault.ModelRejected(error.Message).ToError());

    static ImportedGeometry Gltf(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at) {
        var settings = new ReadSettings { Validation = ValidationMode.Strict };
        var model = format == InterchangeFormat.Glb
            ? ModelRoot.ParseGLB(new ArraySegment<byte>(bytes.ToArray()), settings)
            : TextContext(bytes).ReadTextSchema2(new MemoryStream(bytes.ToArray()));
        return Decoded(format, model.LogicalMeshes.Decode(), at);
    }

    static ReadContext TextContext(ReadOnlyMemory<byte> bytes) {
        var context = ReadContext.CreateFromDictionary(
            new Dictionary<string, ArraySegment<byte>> { ["model.gltf"] = new ArraySegment<byte>(bytes.ToArray()) },
            checkExtensions: true);
        context.Validation = ValidationMode.Strict;
        return context;
    }

    static ImportedGeometry Decoded(InterchangeFormat format, IReadOnlyList<IMeshDecoder<Material>> meshes, Instant at) {
        var triangles = meshes
            .SelectMany(static mesh => mesh.Primitives)
            .SelectMany(static prim => prim.TriangleIndices.Map(tri => (prim, tri)))
            .ToSeq();
        int vertexCount = triangles.Count * 3;
        var vertices = new float[vertexCount * 3];
        var normals = new float[vertexCount * 3];
        var indices = new long[vertexCount];
        int slot = 0;
        foreach (var (prim, (a, b, c)) in triangles) {
            foreach (int corner in stackalloc[] { a, b, c }) {
                var p = prim.GetPosition(corner);
                var n = prim.GetNormal(corner);
                int v = slot * 3;
                (vertices[v], vertices[v + 1], vertices[v + 2]) = (p.X, p.Y, p.Z);
                (normals[v], normals[v + 1], normals[v + 2]) = (n.X, n.Y, n.Z);
                indices[slot] = slot;
                slot++;
            }
        }
        return new ImportedGeometry(format, vertices, normals, indices, vertexCount, triangles.Count, at);
    }

    static ImportedGeometry Framed(InterchangeFormat format, ImportedGeometry geometry) {
        if (format.IsCanonicalFrame) {
            return geometry;
        }
        var vertices = MemoryMarshal.AsMemory(geometry.Vertices);
        FrameNormalization.Canonicalize(format, vertices.Span, stride: 3);
        return geometry with { Vertices = vertices };
    }

    static IfcSemanticModel Semantic(DatabaseIfc db, Instant at) {
        var project = db.Project;
        var properties = project.Extract<IfcPropertySet>().AsIterable()
            .SelectMany(static ps => ps.HasProperties.Values.OfType<IfcPropertySingleValue>()
                .Select(pv => new IfcSemanticModel.PropertyRow(ps.GlobalId, ps.Name ?? "", pv.Name ?? "", pv.NominalValue?.ValueString ?? ""))).ToSeq();
        var materials = project.Extract<IfcRelAssociatesMaterial>().AsIterable()
            .Map(static r => new IfcSemanticModel.MaterialRow(r.GlobalId, (r.RelatingMaterial as IfcMaterial)?.Name ?? "")).ToSeq();
        return new IfcSemanticModel(
            db.Release, db.ModelView,
            project.Extract<IfcSpatialStructureElement>().AsIterable()
                .Map(static s => new IfcSemanticModel.SpatialNode(s.GlobalId, s.GetType().Name, s.Name ?? "", s.LongName ?? "",
                    s.Extract<IfcProduct>().AsIterable().Map(static p => p.GlobalId).ToSeq())).ToSeq(),
            project.Extract<IfcProduct>().AsIterable()
                .Map(static p => new IfcSemanticModel.ProductRow(
                    p.GlobalId, ParserIfc.IdentifyIfcClass(p.GetType().Name, out var predefined), p.Name ?? "", (p as IfcElement)?.Tag ?? "",
                    predefined ?? "", (p as IfcObject)?.ObjectType ?? "",
                    Optional((p as IfcObject)?.IsTypedBy?.RelatingType?.GlobalId))).ToSeq(),
            properties,
            project.Extract<IfcElementQuantity>().AsIterable()
                .SelectMany(static eq => eq.Quantities.Values.OfType<IfcPhysicalSimpleQuantity>()
                    .Select(q => new IfcSemanticModel.QuantityRow(eq.GlobalId, eq.Name ?? "", q.Name ?? "", q.MeasureValue?.Measure ?? 0d, q.Unit?.ToString() ?? ""))).ToSeq(),
            materials,
            project.Extract<IfcRelAssociatesClassification>().AsIterable()
                .SelectMany(static r => r.RelatedObjects.Select(o => (o.GlobalId, reference: r.RelatingClassification as IfcClassificationReference)))
                .Where(static pair => pair.reference is not null)
                .Select(static pair => new IfcSemanticModel.ClassificationRow(
                    pair.GlobalId,
                    (pair.reference!.ReferencedSource as IfcClassification)?.Name ?? "",
                    pair.reference.Identification ?? "",
                    pair.reference.Location ?? "")).ToSeq(),
            project.Extract<IfcTypeObject>().AsIterable()
                .Map(t => new IfcSemanticModel.TypeRow(
                    t.GlobalId, ParserIfc.IdentifyIfcClass(t.GetType().Name, out var typePredefined), t.Name ?? "", typePredefined ?? "",
                    properties.Filter(p => t.HasPropertySets.Any(ps => ps.GlobalId == p.OwnerGlobalId)),
                    materials.Filter(m => m.OwnerGlobalId == t.GlobalId),
                    Optional((t as IfcTypeProduct)?.RepresentationMaps?.FirstOrDefault())
                        .Map(map => InterchangeIdentity.Key("ifc-repmap", Encoding.UTF8.GetBytes(map.StringSTEP()), db.Tolerance, db.Tolerance, db.ToleranceAngleRadians)))).ToSeq(),
            project.Extract<IfcGroup>().AsIterable()
                .Map(static g => new IfcSemanticModel.ZoneRow(
                    g.GlobalId, g.GetType().Name, g.Name ?? "",
                    g is IfcSpatialZone spatialZone ? spatialZone.PredefinedType.ToString() : "",
                    g.IsGroupedBy.SelectMany(static rel => rel.RelatedObjects.Select(static o => o.GlobalId)).ToSeq())).ToSeq(),
            project.Extract<IfcRelAggregates>().AsIterable()
                .Map(static r => (AssemblyRel)new AssemblyRel.Aggregates(r.RelatingObject.GlobalId, r.RelatedObjects.Select(static o => o.GlobalId).ToSeq())).ToSeq(),
            MapConversion(project),
            db.Tolerance, at);
    }

    static Option<IfcSemanticModel.MapConversionRow> MapConversion(IfcProject project) =>
        Optional(project.RepresentationContexts
                .OfType<IfcGeometricRepresentationContext>()
                .Select(static ctx => ctx.HasCoordinateOperation as IfcMapConversion)
                .FirstOrDefault(static conversion => conversion is not null))
            .Map(static conversion => new IfcSemanticModel.MapConversionRow(
                conversion.Eastings, conversion.Northings, conversion.OrthogonalHeight,
                conversion.XAxisAbscissa, conversion.XAxisOrdinate, conversion.Scale == 0.0 ? 1.0 : conversion.Scale,
                (conversion.SourceCRS as IfcCoordinateReferenceSystem)?.Name ?? "",
                (conversion.TargetCRS as IfcProjectedCRS)?.Name ?? "",
                (conversion.TargetCRS as IfcProjectedCRS)?.GeodeticDatum ?? "",
                (conversion.TargetCRS as IfcProjectedCRS)?.MapProjection ?? "",
                (conversion.TargetCRS as IfcProjectedCRS)?.MapZone ?? ""));

    static DatabaseIfc Database(InterchangeFormat format, ReadOnlyMemory<byte> bytes) =>
        format == InterchangeFormat.IfcJson ? JsonDatabase(bytes)
        : format == InterchangeFormat.IfcXml ? XmlDatabase(bytes)
        : DatabaseIfc.ParseString(Encoding.UTF8.GetString(bytes.Span));

    static DatabaseIfc JsonDatabase(ReadOnlyMemory<byte> bytes) {
        var db = new DatabaseIfc(false, ReleaseVersion.IFC4X3_ADD2);
        db.ReadJSON((JsonObject)JsonNode.Parse(bytes.Span)!);
        return db;
    }

    static DatabaseIfc XmlDatabase(ReadOnlyMemory<byte> bytes) {
        var doc = new XmlDocument();
        doc.LoadXml(Encoding.UTF8.GetString(bytes.Span));
        var db = new DatabaseIfc(false, ReleaseVersion.IFC4X3_ADD2);
        db.ReadXMLDoc(doc);
        return db;
    }
}
```

## [3]-[SPECKLE_SEAM]

- Owner: `BimIo.ImportSpeckle` — the Speckle object-graph arm of the import fold, folding a deserialized `Speckle.Sdk.Models.Base` tree onto the canonical `ImportedGeometry` mesh-scene carrier and the `IfcSemanticModel` graph; there is no `SpeckleImporter` type and no parallel decode family — the seam is a third entrypoint on the existing `BimIo` capsule, symmetric to `ImportGeometry`/`ImportIfc`, consuming the receive-side `Base` the Persistence `csharp:Persistence/sync/collaboration#SPECKLE_SYNC` `IOperations.Receive` returns.
- Entry: `BimIo.ImportSpeckle(Base root, ClockPolicy clocks)` projecting the display-mesh geometry, and `BimIo.ImportSpeckleSemantic(Base root, ClockPolicy clocks)` projecting the host-object semantic graph — `Fin<T>` aborts on a graph with no displayable geometry or a malformed display mesh, projecting the Speckle exception onto `BimFault.ModelRejected` at the boundary so domain code never sees a `Speckle.Sdk.SpeckleException`; the `Base` arrives already deserialized, so the seam mints no transport, no `IOperations` reference, and no second graph walk beyond the package-owned traversal.
- Auto: the geometry fold runs the package-owned `BaseExtensions.Flatten(Base, BaseExtensions.BaseRecursionBreaker?)` deduplicating graph walk, projects each node's `BaseExtensions.TryGetDisplayValue(Base)` display list to its `Mesh` members, and decodes each `Mesh` — the flat `vertices`/`vertexNormals` (`List<double>`, flat `x,y,z`) and length-prefixed `faces` (`List<int>`, each face `[n, i0, … i(n-1)]`) triangulate through a fan over the n-gon, scaled onto the canonical metre frame by `Units.GetConversionFactor(mesh.units, Units.Meters)` so a millimetre or foot Speckle model lands in kernel units; a node that `IsDisplayableObject` is false yet carries non-mesh geometry (`Brep`/`Surface`/`Curve` with no `displayValue`) routes its content to `tessellation-bridge#TESSELLATION_BRIDGE` over the GLB rail rather than evaluating a BRep in-process; the semantic fold projects every `DataObject` (and its `RhinoObject`/`RevitObject`/`ArchicadObject`/`TeklaObject`/`Civil3dObject`/`AutocadObject` host-object subtypes) onto an `IfcSemanticModel.ProductRow` keyed on `Base.applicationId`, its `DataObject.properties` (`Dictionary<string, object?>`) flattened to `PropertyRow`, and the `TraversalContext.Parent` chain reconstructing the spatial `SpatialNode` containment.
- Receipt: the `ModelLoad` receipt case carries the format key `InterchangeFormat.Glb.Key` proxy for the decoded scene, the codec key `speckle-base`, the `Base.GetTotalChildrenCount()` source object count, and elapsed; a semantic ingest stamps the host-object count and the distinct `speckle_type` discriminants extracted; emission rides the sink port at the composition edge.
- Packages: Speckle.Sdk, Speckle.Objects, SharpGLTF.Core, NodaTime, LanguageExt.Core, Rasm
- Growth: a new Speckle geometry leaf is one arm on the `DisplayMeshes` projection keyed on the `IDisplayValue<T>` payload type; a new host-object discriminant is one `speckle_type`-keyed row on the semantic projection; a non-mesh evaluation never grows a managed Speckle tessellator — it widens the `tessellation-bridge#TESSELLATION_BRIDGE` request, never this fold.
- Boundary: `BimIo` is the page boundary capsule and the Speckle arm carries the language-owned statement forms the foreign graph walk requires; the `Base` graph is admitted exactly once — `Flatten` is the single package-owned deduplicating traversal (it caches on `Base.id`), so the seam never re-walks the tree or hand-rolls a `DynamicBase.GetMembers` recursion, and `TryGetDisplayValue`/`IsDisplayableObject` own the displayable-node vocabulary rather than a per-type `is Mesh`/`is Brep` ladder; the Speckle `Mesh.faces` length-prefixed n-gon encoding fans into the canonical triangle-soup `ImportedGeometry` at the boundary (one contiguous vertex/normal/index triple, the allocation point, never a per-face `double[]` proliferation), and a degenerate face (`n < 3`) faults the decode; non-mesh geometry never evaluates in-process — GeometryGym carries no Speckle BRep kernel and the managed branch owns no NURBS evaluator, so a `Brep`/`Surface`/`Curve` with no `displayValue` rides the companion GLB rail exactly as the IFC geometry request does, joining the same content-keyed artifact; `Speckle.Sdk`/`Speckle.Objects` are the OUTSIDE-RHINO concern (`Speckle.Sdk.Dependencies` repacks the SDK's Polly/channel/serialisation-V2 closure), so this arm composes them only in the host-neutral `Rasm.Bim` exchange assembly and the in-Rhino plugin assembly never loads them; a `SpeckleImporter`/`SpeckleConverter` service family, a hand-rolled `Base`-graph recursion, and a managed Speckle tessellator are the deleted forms.

```csharp signature
public static partial class BimIo {
    public static Fin<ImportedGeometry> ImportSpeckle(Base root, ClockPolicy clocks) =>
        Boundary(() => DisplayScene(root, clocks.Now))
            .Bind(static scene => scene.TriangleCount > 0
                ? Fin.Succ(scene)
                : Fin.Fail<ImportedGeometry>(new BimFault.ModelRejected($"speckle-no-display:{root.speckle_type}").ToError()));

    public static Fin<IfcSemanticModel> ImportSpeckleSemantic(Base root, ClockPolicy clocks) =>
        Boundary(() => SpeckleSemantic(root, clocks.Now));

    static ImportedGeometry DisplayScene(Base root, Instant at) {
        var soup = root.Flatten()
            .SelectMany(static node => node.TryGetDisplayValue()?.OfType<Mesh>() ?? Enumerable.Empty<Mesh>())
            .ToSeq()
            .Fold(MeshSoup.Empty, static (soup, mesh) => soup.Append(mesh));
        return new ImportedGeometry(
            InterchangeFormat.Glb, soup.Vertices.ToArray(), soup.Normals.ToArray(), soup.Indices.ToArray(),
            soup.VertexCount, soup.TriangleCount, at);
    }

    readonly record struct MeshSoup(Seq<float> Vertices, Seq<float> Normals, Seq<long> Indices, int VertexCount, int TriangleCount) {
        public static readonly MeshSoup Empty = new(Seq<float>(), Seq<float>(), Seq<long>(), 0, 0);

        public MeshSoup Append(Mesh mesh) {
            double scale = Units.GetConversionFactor(mesh.units, Units.Meters);
            var fans = Triangulate(mesh.faces).ToSeq();
            var vertices = fans.Bind(corner => Sample(mesh.vertices, corner, scale));
            var normals = fans.Bind(corner => mesh.vertexNormals.Count == mesh.vertices.Count
                ? Sample(mesh.vertexNormals, corner, 1.0)
                : Seq<float>(0f, 0f, 1f));
            var indices = fans.Map((_, ordinal) => (long)(VertexCount + ordinal));
            return new MeshSoup(
                Vertices + vertices, Normals + normals, Indices + indices,
                VertexCount + fans.Count, TriangleCount + fans.Count / 3);
        }

        static IEnumerable<int> Triangulate(List<int> faces) {
            for (int cursor = 0; cursor < faces.Count;) {
                int span = faces[cursor];
                if (span < 3) { throw new InvalidDataException($"<speckle-degenerate-face:{span}>"); }
                for (int corner = 1; corner + 1 < span; corner++) {
                    yield return faces[cursor + 1];
                    yield return faces[cursor + 1 + corner];
                    yield return faces[cursor + 2 + corner];
                }
                cursor += span + 1;
            }
        }

        static Seq<float> Sample(List<double> flat, int vertex, double scale) =>
            Seq((float)(flat[vertex * 3] * scale), (float)(flat[vertex * 3 + 1] * scale), (float)(flat[vertex * 3 + 2] * scale));
    }

    static IfcSemanticModel SpeckleSemantic(Base root, Instant at) {
        var objects = root.Flatten().OfType<DataObject>().ToSeq();
        return new IfcSemanticModel(
            ReleaseVersion.IFC4X3_ADD2, ModelView.Ifc4Reference,
            Seq<IfcSemanticModel.SpatialNode>(),
            objects.Map(static data => new IfcSemanticModel.ProductRow(
                data.applicationId ?? data.id ?? "", data.speckle_type, data.name, "", "", "", Option<string>.None)),
            objects.Bind(static data => data.properties
                .Select(pair => new IfcSemanticModel.PropertyRow(data.applicationId ?? data.id ?? "", data.speckle_type, pair.Key, pair.Value?.ToString() ?? ""))
                .ToSeq()),
            Seq<IfcSemanticModel.QuantityRow>(), Seq<IfcSemanticModel.MaterialRow>(),
            Seq<IfcSemanticModel.ClassificationRow>(), Seq<IfcSemanticModel.TypeRow>(),
            Seq<IfcSemanticModel.ZoneRow>(), Seq<AssemblyRel>(), Option<IfcSemanticModel.MapConversionRow>.None, 1e-6, at);
    }
}
```

## [4]-[RESEARCH]

- [NATIVE_FORMAT_BRIDGES]: the Revit `.rvt`, Navisworks `.nwc`/`.nwd`, and DWG/DXF native readers ride the `native-companion` codec through the Compute companion process (the managed C# branch has no native loader); the STL/3MF/OBJ/PLY mesh-text decode is managed-in-intent but the reader packages are uncatalogued, so the `mesh-text` import arm faults until its decode member spellings ground against the admitted mesh-text libraries.
- [SPECKLE_CATALOGUE]: the `Speckle.Sdk`/`Speckle.Objects` member spellings the `ImportSpeckle` fold composes — `Speckle.Sdk.Models.Base` (`id`/`applicationId`/`speckle_type`/`GetTotalChildrenCount`), `Speckle.Sdk.Models.Extensions.BaseExtensions.Flatten`/`Traverse`/`TryGetDisplayValue`/`IsDisplayableObject` with the `BaseRecursionBreaker` delegate, `Speckle.Sdk.Models.GraphTraversal.TraversalContext.Parent`/`PropName`/`Current`, `Speckle.Sdk.Common.Units.GetConversionFactor`/`Meters`, `Speckle.Objects.Geometry.Mesh` (`vertices`/`faces`/`vertexNormals` `List<double>`/`List<int>`, length-prefixed n-gon `faces`, `units`, `VerticesCount`), `Speckle.Objects.Geometry.Brep` (`displayValue` `List<Mesh>`), `Speckle.Objects.IDisplayValue<out T>`, and `Speckle.Objects.Data.DataObject` (`name`/`displayValue` `List<Base>`/`properties` `Dictionary<string, object?>`) and its host-object subtypes — are decompile-verified against the `Speckle.Sdk`/`Speckle.Objects` 3.21.1 assemblies and catalogued at `csharp:Persistence/.api/api-speckle` (the cross-folder sync owner), and land as a `Rasm.Bim/.api/api-speckle` folder catalogue at alignment so the seam's external members are folder-local verified; the catalogue records the `Mesh.vertexNormals` flat-normal member and the `DataObject.properties` `Dictionary<string, object?>` shape the cross-folder catalogue elides.
- [IFC5_ECS]: the IFC5 ECS-JSON (`.ifcx`) component-graph parse rides the GeometryGym IFC5 surface for the semantic graph and the Compute companion for native-grade tessellation; the `ifc5` row mirrors the IFC4x3 ingest with the ECS-component projection. IFC4.3 ADD2 is the production baseline (`ReleaseVersion.IFC4X3_ADD2`); IFC5 is the componentized/granular `.ifcx` architecture in active public development, so the `ifc5` row is a forward-looking GeometryGym IFC5-surface row grounding against the GeometryGym IFC5 member surface at alignment.
