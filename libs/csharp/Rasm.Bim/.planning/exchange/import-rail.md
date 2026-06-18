# [BIM_IMPORT_RAIL]

The foreign-bytes ingest rail: one `BimIo` import fold over the `format-axis#FORMAT_AXIS` `InterchangeFormat` rows, dispatching the managed glTF/GLB mesh-and-scene decode through SharpGLTF, the STL/3MF/OBJ/PLY mesh-text arm, the in-process semantic IFC/IFC5/STEP graph ingest through GeometryGym, and the AP242/native-companion two-hop route — never tessellated BRep. The page composes the kernel `Rasm` geometry and consumes the `format-axis#FORMAT_AXIS` codec/frame rows as settled vocabulary; an IFC/native geometry request routes to `tessellation-bridge#TESSELLATION_BRIDGE`. The page is HOST-LOCAL.

## [1]-[INDEX]

- [2]-[IMPORT_RAIL]: foreign-bytes ingest — managed mesh decode and in-process semantic IFC/IFC5/STEP graph.

## [2]-[IMPORT_RAIL]

- Owner: `BimIo` — the import fold over `InterchangeFormat`, dispatching the managed glTF/GLB mesh-and-scene decode through SharpGLTF, the STL/3MF/OBJ/PLY mesh-text arm that faults until its reader package catalogues, the in-process semantic IFC/IFC5 ingest through GeometryGym over `DatabaseIfc`/`Extract<T>`, and the AP242/native-companion two-hop route; `ImportedGeometry` the decoded mesh-scene carrier, `IfcSemanticModel` the IFC model-graph projection.
- Entry: `BimIo.ImportGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks)` for the managed glTF and mesh-text path; `BimIo.ImportIfc(...)` for the in-process IFC/IFC5 semantic graph — `Fin<T>` aborts on a codec reject or a capability miss, the foreign decode arity discriminating on the row's `InterchangeCodec` so a path lands one decode without a call-site type branch, projecting the package exception onto `BimFault.ModelRejected` at the boundary so domain code never sees the SharpGLTF `ModelException` or the GeometryGym parse fault.
- Auto: binary GLB decode lands through `ModelRoot.ParseGLB(ArraySegment<byte>)` and text `.gltf` decode through `ReadContext.ReadTextSchema2(Stream)` then `model.LogicalMeshes.Decode()` projecting `IMeshDecoder<Material>` primitives to `ImportedGeometry` vertex and index spans with zero intermediate file; the IFC semantic path constructs a `DatabaseIfc` over the bytes through `DatabaseIfc.ParseString`/`ReadXMLDoc`/`ReadJSON` by the row's format, narrows `db.Project`, and folds `db.Project.Extract<T>()` collecting spatial hierarchy (`IfcSpatialStructureElement`), products (`IfcProduct`), property sets (`IfcPropertySet`), quantities (`IfcElementQuantity`), materials (`IfcRelAssociatesMaterial`), classification associations (`IfcRelAssociatesClassification`), type objects (`IfcTypeObject`), and decomposition relationships (`IfcRelDecomposes`) into the `IfcSemanticModel` graph — never tessellated BRep.
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
    Seq<AssemblyRel> Decomposition,
    double Tolerance,
    Instant At) {
    public sealed record SpatialNode(string GlobalId, string EntityType, string Name, string LongName, Seq<string> ContainedGlobalIds);
    public sealed record ProductRow(string GlobalId, string EntityType, string Name, string Tag, Option<string> TypeGlobalId);
    public sealed record PropertyRow(string OwnerGlobalId, string SetName, string PropertyName, string Value);
    public sealed record QuantityRow(string OwnerGlobalId, string SetName, string QuantityName, double Value, string Unit);
    public sealed record MaterialRow(string OwnerGlobalId, string MaterialName);
    public sealed record ClassificationRow(string OwnerGlobalId, string System, string Code, string DictionaryClassUri);
    public sealed record TypeRow(string GlobalId, string EntityType, string Name);
}

public static class BimIo {
    public static Fin<ImportedGeometry> ImportGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks) =>
        !format.CanImport ? Fin.Fail<ImportedGeometry>(new BimFault.ModelRejected($"<import-unsupported:{format.Key}>"))
        : format.CataloguePending ? Fin.Fail<ImportedGeometry>(new BimFault.ModelRejected($"<import-catalogue-pending:{format.Key}:{format.Codec.CataloguePackage.IfNone("<unknown>")}>"))
        : format.Codec == InterchangeCodec.SharpGltf ? Boundary(() => Framed(format, Gltf(format, bytes, clocks.Now)))
        : Fin.Fail<ImportedGeometry>(new BimFault.ModelRejected($"<import-needs-companion:{format.Key}>"));

    public static Fin<IfcSemanticModel> ImportIfc(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks) =>
        format.Codec == InterchangeCodec.GeometryGym
            ? Boundary(() => Semantic(Database(format, bytes), clocks.Now))
            : Fin.Fail<IfcSemanticModel>(new BimFault.ModelRejected($"<ifc-codec-miss:{format.Key}>"));

    static Fin<T> Boundary<T>(Func<T> decode) =>
        Try.lift(decode).Run().MapFail(static error => (Error)new BimFault.ModelRejected(error.Message));

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
        return new IfcSemanticModel(
            db.Release, db.ModelView,
            project.Extract<IfcSpatialStructureElement>().AsIterable()
                .Map(static s => new IfcSemanticModel.SpatialNode(s.GlobalId, s.GetType().Name, s.Name ?? "", s.LongName ?? "",
                    s.Extract<IfcProduct>().AsIterable().Map(static p => p.GlobalId).ToSeq())).ToSeq(),
            project.Extract<IfcProduct>().AsIterable()
                .Map(static p => new IfcSemanticModel.ProductRow(p.GlobalId, p.GetType().Name, p.Name ?? "", (p as IfcElement)?.Tag ?? "",
                    Optional((p as IfcObject)?.IsTypedBy.FirstOrDefault()?.RelatingType?.GlobalId))).ToSeq(),
            project.Extract<IfcPropertySet>().AsIterable()
                .SelectMany(static ps => ps.HasProperties.Values.OfType<IfcPropertySingleValue>()
                    .Select(pv => new IfcSemanticModel.PropertyRow(ps.GlobalId, ps.Name ?? "", pv.Name ?? "", pv.NominalValue?.ValueString ?? ""))).ToSeq(),
            project.Extract<IfcElementQuantity>().AsIterable()
                .SelectMany(static eq => eq.Quantities.Values.OfType<IfcPhysicalSimpleQuantity>()
                    .Select(q => new IfcSemanticModel.QuantityRow(eq.GlobalId, eq.Name ?? "", q.Name ?? "", q.SimpleValue, q.Unit?.ToString() ?? ""))).ToSeq(),
            project.Extract<IfcRelAssociatesMaterial>().AsIterable()
                .Map(static r => new IfcSemanticModel.MaterialRow(r.GlobalId, (r.RelatingMaterial as IfcMaterial)?.Name ?? "")).ToSeq(),
            project.Extract<IfcRelAssociatesClassification>().AsIterable()
                .SelectMany(static r => r.RelatedObjects.Select(o => (o.GlobalId, reference: r.RelatingClassification as IfcClassificationReference)))
                .Where(static pair => pair.reference is not null)
                .Select(static pair => new IfcSemanticModel.ClassificationRow(
                    pair.GlobalId,
                    (pair.reference!.ReferencedSource as IfcClassification)?.Name ?? "",
                    pair.reference.Identification ?? "",
                    pair.reference.Location ?? "")).ToSeq(),
            project.Extract<IfcTypeObject>().AsIterable()
                .Map(static t => new IfcSemanticModel.TypeRow(t.GlobalId, t.GetType().Name, t.Name ?? "")).ToSeq(),
            project.Extract<IfcRelAggregates>().AsIterable()
                .Map(static r => (AssemblyRel)new AssemblyRel.Aggregates(r.RelatingObject.GlobalId, r.RelatedObjects.Select(static o => o.GlobalId).ToSeq())).ToSeq(),
            db.Tolerance, at);
    }

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

## [3]-[RESEARCH]

- [NATIVE_FORMAT_BRIDGES]: the Revit `.rvt`, Navisworks `.nwc`/`.nwd`, and DWG/DXF native readers ride the `native-companion` codec through the Compute companion process (the managed C# branch has no native loader); the STL/3MF/OBJ/PLY mesh-text decode is managed-in-intent but the reader packages are uncatalogued, so the `mesh-text` import arm faults until its decode member spellings ground against the admitted mesh-text libraries.
- [IFC5_ECS]: the IFC5 ECS-JSON (`.ifcx`) component-graph parse rides the GeometryGym IFC5 surface for the semantic graph and the Compute companion for native-grade tessellation; the `ifc5` row mirrors the IFC4x3 ingest with the ECS-component projection. IFC4.3 ADD2 is the production baseline (`ReleaseVersion.IFC4X3_ADD2`); IFC5 is the componentized/granular `.ifcx` architecture in active public development, so the `ifc5` row is a forward-looking GeometryGym IFC5-surface row grounding against the GeometryGym IFC5 member surface at alignment.
