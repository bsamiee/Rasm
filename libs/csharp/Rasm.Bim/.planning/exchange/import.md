# [BIM_IMPORT_RAIL]

The foreign-bytes ingest rail: one `BimIo` import fold over the `format#FORMAT_AXIS` `InterchangeFormat` rows, dispatching the managed glTF/GLB mesh-and-scene decode through SharpGLTF, the STL/OBJ/OFF mesh-text arm through `geometry3Sharp`, the dedicated PLY decode through `Ply.Net` (retiring the hand-rolled BCL `PlyReader`), the FBX/Collada/3MF scene decode through `AssimpNetter` (retiring the hand-rolled BCL `ThreeMfReader`), the OpenUSD scene decode through `UniversalSceneDescription` `UsdStage`, the in-process semantic IFC/IFC5 graph ingest through GeometryGym, the in-process ISO 10303-21 Part-21 product-structure semantic ingest through the BCL-only `StepReader`, the AP242/native-companion two-hop geometry route, and the Speckle `Base` object-graph seam folding a deserialized `Speckle.Sdk.Models.Base` tree onto the same `ImportedGeometry`/`IfcSemanticModel` carriers — never tessellated BRep. The page composes the kernel `Rasm` geometry and consumes the `format#FORMAT_AXIS` codec/frame rows as settled vocabulary; an IFC/native/Speckle-non-mesh geometry request routes to `tessellation#TESSELLATION_BRIDGE`. The page is HOST-LOCAL in posture; the Speckle seam composes `Speckle.Sdk`/`Speckle.Objects` and runs only in the host-neutral exchange assembly, never inside the in-Rhino plugin ALC.

## [01]-[INDEX]

- [01]-[IMPORT_RAIL]: foreign-bytes ingest — managed mesh decode and in-process semantic IFC/IFC5/STEP graph.
- [02]-[SPECKLE_SEAM]: Speckle `Base` object-graph fold — display-mesh decode and host-object semantic projection onto the canonical carriers.

## [02]-[IMPORT_RAIL]

- Owner: `BimIo` — the import fold over `InterchangeFormat`, dispatching the managed glTF/GLB mesh-and-scene decode through SharpGLTF, the OBJ/STL/OFF mesh-text arm through the `geometry3Sharp` `StandardMeshReader`, the dedicated PLY decode through the `Ply.Net` `PlyParser` (the `ply-net` codec retiring the BCL `PlyReader`), the FBX/Collada/3MF scene decode through the `AssimpNetter` `AssimpContext` (the `scene-exchange` codec retiring the BCL `ThreeMfReader`), the OpenUSD scene decode through the `UniversalSceneDescription` `UsdStage` (the `usd-stage` codec), the in-process semantic IFC/IFC5 ingest through GeometryGym over `DatabaseIfc`/`Extract<T>`, the managed BCL-only `StepReader` ISO 10303-21 Part-21 entity-instance-graph semantic ingest over the `StepIso10303` codec, and the AP242/native-companion two-hop geometry route; `ImportedGeometry` the decoded mesh-scene carrier, `IfcSemanticModel` the IFC model-graph projection, `StepSemanticModel` the ISO 10303 product-structure projection.
- Entry: `BimIo.ImportGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks)` for the managed mesh-and-scene path (dispatching by `InterchangeCodec` to SharpGLTF, the `geometry3Sharp` mesh-text arm, `Ply.Net`, `AssimpNetter`, `UsdStage`, or ACadSharp); `BimIo.ImportIfc(...)` for the in-process IFC/IFC5 semantic graph; `BimIo.ImportStep(...)` for the in-process ISO 10303-21 Part-21 product-structure semantic graph — `Fin<T>` aborts on a codec reject (`Model/faults#FAULT_BAND` `BimFault.CodecReject`) or a companion-required geometry request (`BimFault.CapabilityMiss`), each lowered with `.ToError()`, the foreign decode arity discriminating on the row's `InterchangeCodec` so a path lands one decode without a call-site type branch, projecting the package or parse exception onto `BimFault.ModelRejected` at the boundary so domain code never sees the SharpGLTF `ModelException`, the GeometryGym parse fault, or a malformed-Part-21 `InvalidDataException`.
- Auto: binary GLB decode lands through `ModelRoot.ParseGLB(ArraySegment<byte>)` and text `.gltf` decode through `ReadContext.ReadTextSchema2(Stream)` then a `Decompress` pre-decode branch reading the parsed model's `KHR_draco_mesh_compression` primitive extension and `EXT_meshopt_compression` bufferView extension and routing the compressed payload through the package-owned `Draco.Decode(byte[])` and `Meshopt.DecodeVertexBuffer`/`DecodeIndexBuffer`/`DecodeFilterOct`/`DecodeFilterQuat`/`DecodeFilterExp`/`DecodeFilterColor` decoders before `model.LogicalMeshes.Decode()` projecting `IMeshDecoder<Material>` primitives to `ImportedGeometry` vertex and index spans with zero intermediate file; the IFC semantic path constructs a `DatabaseIfc` over the bytes through `DatabaseIfc.ParseString`/`ReadXMLDoc`/`ReadJSON` by the row's format, narrows `db.Project`, and folds `db.Project.Extract<T>()` collecting spatial hierarchy (`IfcSpatialStructureElement`), products (`IfcProduct`, splitting the predefined-type token through `ParserIfc.IdentifyIfcClass(name, out predefinedConstant)` and carrying the `IfcObject.ObjectType` user-defined fallback), property sets (`IfcPropertySet`), quantities (`IfcElementQuantity`), materials (`IfcRelAssociatesMaterial`), classification associations (`IfcRelAssociatesClassification`), type objects (`IfcTypeObject`, widened to carry the type-bound `HasPropertySets` property family, the type materials, and the `IfcTypeProduct.RepresentationMaps` instanced-geometry content key the `Model/elements#BIM_TYPE` `BimType` reads), grouping/zone overlays (`IfcGroup`/`IfcZone`/`IfcSpatialZone` with their `IsGroupedBy` `IfcRelAssignsToGroup` member sets the `Model/zones#ZONE_GRAPH` overlay folds), the map-conversion projection (`IfcGeometricRepresentationContext.HasCoordinateOperation` → `IfcMapConversion`/`IfcProjectedCRS` the `Semantics/georeference#GEO_REFERENCE` `Project` reads), and decomposition relationships (`IfcRelDecomposes`) into the `IfcSemanticModel` graph — never tessellated BRep.
- Receipt: the `ModelLoad` receipt case carries the format key, codec key, source byte count, and elapsed for a managed mesh import; an IFC semantic ingest stamps the schema version (`db.Release`), the model-view (`db.ModelView`), and the extracted-entity counts; a STEP semantic ingest stamps the `StepProtocol`, the `FILE_SCHEMA` schema name, and the product/definition/assembly/geometry-ref counts; emission rides the sink port at the composition edge.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Runtime, GeometryGymIFC_Core, Openize.Drako, Alimer.Bindings.MeshOptimizer, geometry3Sharp, Ply.Net, AssimpNetter, UniversalSceneDescription, ACadSharp, NodaTime, LanguageExt.Core, Rasm
- Growth: a new managed import is one codec arm on the import fold keyed by the `InterchangeFormat.Codec` row; a new extracted IFC entity family is one `Extract<T>` projection on `IfcSemanticModel`; a new extracted STEP entity family is one `Keyword`-filtered projection on `StepSemanticModel` over the resolved instance graph; a new STEP application protocol is one `InterchangeFormat` row carrying its `StepProtocol` discriminant — the single `StepReader` reads the protocol off `format.StepProtocol` and the entity-instance grammar is protocol-agnostic, so AP203/AP214/AP242 share one reader and one codec without a per-protocol reader; a new glTF compression codec is one `KhrEncoder`-keyed arm on the `Decompress` pre-decode branch symmetric to the `export#EXPORT_RAIL` `GlbBytes` compression switch, never a second importer.
- Boundary: `BimIo` is the page boundary capsule and its codec arms carry the language-owned statement forms the foreign package decode requires; glTF mesh decode rides the `MeshDecoder.Decode` runtime contract reading `IMeshPrimitiveDecoder.GetPosition`/`GetNormal`/`TriangleIndices` (an accessor-based contract returning per-vertex `Vector3`/index-tuple values, so the decode materializes one contiguous `ImportedGeometry` vertex/normal/index triple at the boundary — the accessor contract admits no zero-copy span into SharpGLTF's internal buffers, so the one boundary materialization, not a per-primitive `float[]` proliferation, is the allocation point); the `mesh-text` decode arm reads the OBJ/STL/OFF mesh-text containers through the `geometry3Sharp` `StandardMeshReader.Read(Stream, extension, ReadOptions)` extension-dispatched reader into a `DMesh3Builder`, projecting the resulting `DMesh3` (`VertexIndices()`/`TriangleIndices()` over the refcounted sparse pools, `GetVertex(int)` `Vector3d` position, `GetVertexNormal(int)` `Vector3f` normal, `GetTriangle(int)` `Index3i`) onto the same contiguous `ImportedGeometry` vertex/normal/index triple the glTF arm materializes — one boundary allocation, the `DMesh3` sparse index space iterated through its live-id enumerators rather than a dense `0..VertexCount` loop because the refcounted pool leaves holes; the `mesh-text` arm is now geometry3Sharp ONLY (OBJ/STL/OFF) because PLY and 3MF left the codec — PLY is the dedicated `ply-net` codec composing `Ply.Net` `PlyParser.Parse(stream, maxChunkSize)` (the `Ply` arm: the `Dataset.Data` `ElementData` rows read the `Vertex` element's typed `x`/`y`/`z`/`nx`/`ny`/`nz` columns as a `System.Array` typed per `DataType` and the `Face` element's `vertex_indices` `int[][]` list column fan-triangulated, retiring the hand-rolled BCL `PlyReader` endian/ascii fork), and FBX/Collada/3MF are the `scene-exchange` codec composing `AssimpNetter` (the `Scene` arm: one disposable `AssimpContext.ImportFileFromStream(stream, PostProcessSteps.Triangulate | JoinIdenticalVertices | GenerateSmoothNormals, format.Key)` folding the `Scene.Meshes` `Vertices`/`Normals`/`Faces` graph onto the triangle-soup, retiring the hand-rolled BCL `ThreeMfReader` OPC/ZIP parse); the OpenUSD `usd-stage` codec composes `UniversalSceneDescription` (the `Usd` arm: `UsdStage.Open` over the temp-path layer stack, `Traverse` filtering each `UsdGeomMesh` prim by `GetTypeName()`, reading the points `VtVec3fArray`/`GfVec3f` and the `GetFaceVertexCountsAttr`/`GetFaceVertexIndicesAttr` `VtIntArray` topology through the typed-array mesh-bridge seam, fan-triangulating each face — USD a scene-graph peer, the BIM semantics staying the GeometryGym IFC graph's, never re-derived from USD prim type names); each arm materializes one contiguous `ImportedGeometry` boundary allocation and the leaked package types (`Ply.Net.*`, `Assimp.*`, `pxr.*`) never cross past `Exchange/import` — internal code holds the canonical carriers per the boundary-mapping law, and the `SWIGTYPE_p_*`/`*PINVOKE` USD interop types never enter the fold; the rejected reader picks stand (`lib3mf` native C++, `Aspose.3D` closed/commercial — `AssimpNetter` ships its own osx-arm64 `libassimp.dylib` admitted as the one scene-exchange owner), and `geometry3Sharp.OBJReader`/`STLReader` stay consumed through the `StandardMeshReader` dispatch, never a second hand-rolled tokenizer; the IFC semantic graph is a model-data projection only — `BaseClassIfc.Extract<T>` collects reachable entities and GeometryGym carries no tessellation kernel, so a geometry request on an IFC row routes to the `tessellation#TESSELLATION_BRIDGE` rail and never evaluates a BRep in-process; the `step-iso10303` STEP route splits two legs: the managed semantic-graph leg is now in-process through the BCL-only `StepReader`, while the B-rep/NURBS geometry leg stays companion-routed because no managed STEP solid evaluator is admitted — `StepReader` strips the Part-21 comment and string spans, splits the HEADER and DATA sections, slices each `#N = ENTITY(...);` statement at depth-zero semicolons, parses each statement into an `Instance(Id, Keyword, Args)` where the recursive-descent `ParseArg` discriminates the Part-21 token grammar into a closed `Arg` `[Union]` — reference `#N` (`Arg.Ref`), string `'...'` with the `''` escape (`Arg.Text`), typed-enum `.X.` (`Arg.Enum`), number (`Arg.Number`), nested list `(...)` (`Arg.List`), typed-constructor `KEYWORD(...)` (`Arg.Typed`), and `$`/`*`/identifier (`Arg.Untyped`) — builds the forward-reference instance graph as a `Dictionary<long, Instance>` resolved in a second pass through `Resolve`, and projects the product structure (`PRODUCT` → `ProductRow`, `PRODUCT_DEFINITION` → `DefinitionRow` walking the `PRODUCT_DEFINITION_FORMATION` reference to its `PRODUCT`, `NEXT_ASSEMBLY_USAGE_OCCURRENCE` → `AssemblyEdge` resolving both relating and related definitions to their product ids) and the AP242 PMI/semantic metadata (`DIMENSIONAL_*`/`DATUM`/`GEOMETRIC_TOLERANCE`/`ANNOTATION_OCCURRENCE` filtered through a frozen `PmiTypes` set into `PmiRow`); the geometry entities (`ADVANCED_BREP_SHAPE_REPRESENTATION`/`MANIFOLD_SOLID_BREP`/`B_SPLINE_SURFACE`/`SHAPE_REPRESENTATION` filtered through a frozen `GeometryTypes` set) are NOT evaluated in-process — `StepReader` carries only their `GeometryRef(Id, EntityType, ShapeDefinitionId)` and routes the actual B-rep/NURBS solid evaluation to the `tessellation#TESSELLATION_BRIDGE` companion rail (OpenCascade serving the STEP solid read), so `TessellationRequiresCompanion` stays `true` on the STEP rows; the rejected managed readers stand — `IxMilia.Step` and `StepFileParser` do not exist on NuGet, `STPLoader` is net35/2015/`AForge.Math`-coupled (RID-unsafe, abandoned), and `DevelApp.StepParser` is a GrammarForge regex/PCRE2 grammar engine that does not model the Part-21 entity-instance graph and pulls a prerelease `ICU4N` alpha transitive — so `StepReader` is the in-process BCL-only entity-instance graph the codec needed and a managed STEP B-rep evaluator beside the companion is the deleted form; GeometryGym is IFC-schema-bound and does not parse generic ISO-10303 AP242 product structure, so it grounds no STEP semantic leg; `DatabaseIfc.Tolerance`/`ToleranceAngleRadians`/`ScaleSI` read the model precision the content-key folds; the `Decompress` pre-decode branch is the decompress-on-import arm symmetric to the `export#EXPORT_RAIL` `GlbBytes` compression switch — `SharpGLTF.Core` ships no compression decoder (the catalogued assembly carries no Draco/meshopt type, decompile-verified absence), so a GLB whose primitive carries a `KHR_draco_mesh_compression` extension or whose bufferView carries an `EXT_meshopt_compression` extension reaches the `LogicalMeshes.Decode()` fold with its accessor data still compressed and unreadable, and `Detect` cannot distinguish a compressed GLB from a plain one because the compression rides a per-primitive/per-bufferView extension, not the row; `SharpGLTF.Core` retains no typed handle to an unrecognized extension (`KHR_draco_mesh_compression`/`EXT_meshopt_compression` have no in-box `JsonSerializable` extension class, so the `ExtraProperties.Extensions` collection never holds them and `ExtraProperties.Extras` is a free-form `JsonNode`, not an extension accessor), so the branch reparses the GLB/glTF JSON chunk the `ReadContext.ReadJson`/`IdentifyBinaryContainer` pair already extracts into a `System.Text.Json.Nodes.JsonNode` tree and reads each `meshes[].primitives[].extensions.KHR_draco_mesh_compression` and `bufferViews[].extensions.EXT_meshopt_compression` object directly out of that tree for its `bufferView`/`count`/`byteStride`/`mode`/`filter` parameters; it routes the `KHR_draco_mesh_compression` primitive payload through `Draco.Decode(byte[])` (downcasting the returned `DracoPointCloud` to `DracoMesh`, reading each `PointAttribute` through `DracoPointCloud.GetNamedAttribute(AttributeType.Position)`/`Normal` and `PointAttribute.GetValueAsVector3(PointAttribute.MappedIndex(point))` per point — the point index mapped to its deduplicated attribute-value index, since Draco shares attribute values across points and `GetValueAsVector3` indexes by value, not point — and the faces through `DracoMesh.NumFaces`/`ReadFace(id, int[3])` yielding point indices aligned with that per-point vertex order, then `Fill`-ing the `MeshPrimitive.GetVertexAccessor("POSITION")`/`"NORMAL"` `Accessor.AsVector3Array()` and the `GetIndexAccessor().AsIndicesArray()` cast to their concrete `Vector3Array`/`IntegerArray` write surface — `Fill(IEnumerable<T>)` is a member of the concrete accessor-array structs, not the `IAccessorArray<T>` interface the factory statically returns, so the decode downcasts to the runtime `Vector3Array`/`IntegerArray` before filling) and the `EXT_meshopt_compression` bufferView payload through `Meshopt.DecodeVertexBuffer`/`DecodeIndexBuffer` then the filter inverse `Meshopt.DecodeFilterOct` (octahedral-encoded normals), `DecodeFilterQuat` (quantized rotations), `DecodeFilterExp` (shared-exponent floats), and `DecodeFilterColor` (quantized vertex color) keyed on the bufferView extension's `mode`/`filter` discriminant, then writes the decompressed buffer back through `ModelRoot.UseBufferView(ArraySegment<byte>, byteStride, BufferMode?)` (the byte buffer wrapped in an `ArraySegment<byte>` so the call binds the strided `(data, byteStride, target)` overload rather than the `(byte[], byteOffset, byteLength, …)` overload, the target read from `BufferView.IsIndexBuffer`) before the `IMeshDecoder` fold so a web-compressed artifact round-trips back through import without a companion — the decode reuses the same dormant `Openize.Drako`/`Alimer.Bindings.MeshOptimizer` surface the export encode switch drives, closing the export-can-compress / import-cannot-decompress asymmetry with zero new package and zero new `InterchangeFormat` row; the SharpGLTF `ReadSettings.Validation` rides `ValidationMode.Strict` on an uncompressed asset so a malformed glTF faults at parse, and a compressed asset parses under `ValidationMode.Skip` past the unrecognized-extension and missing-bufferView-data validation errors the compression extension provokes, then re-validates the reconstructed geometry at the materialization boundary; the `fbx`/`dae` rows are now live `scene-exchange` (`AssimpNetter`) and the USD rows live `usd-stage` (`UniversalSceneDescription`), so the former `import-catalogue-pending` fault no longer fires for them; every codec admit reaching this fold is one `InterchangeCodec`-keyed arm on the existing `ImportGeometry`/`ImportIfc`/`ImportStep` dispatch — the row-promotion discipline `format#FORMAT_AXIS` owns, never a new `BimIo` entrypoint or a parallel importer family — and the companion-versus-managed geometry route is read from the row's `TessellationRequiresCompanion` column rather than a call-site `if (ifc)`/`if (step)` branch; an `IfcImporter`/`GltfImporter`/`PlyImporter`/`SceneImporter`/`UsdImporter`/`StepImporter` service family, a per-extension `DracoImporter`/`MeshoptImporter`, a per-protocol AP203/AP214/AP242 STEP reader, and a managed IFC or STEP B-rep tessellator are the deleted forms.

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

public sealed record StepSemanticModel(
    StepProtocol Protocol,
    string SchemaName,
    string Originating,
    Seq<StepSemanticModel.ProductRow> Products,
    Seq<StepSemanticModel.DefinitionRow> Definitions,
    Seq<StepSemanticModel.AssemblyEdge> Assembly,
    Seq<StepSemanticModel.GeometryRef> Geometry,
    Seq<StepSemanticModel.PmiRow> Pmi,
    Instant At) {
    public sealed record ProductRow(long Id, string ProductId, string Name, string Description);
    public sealed record DefinitionRow(long Id, long ProductId, string Formation, string LifeCycle, string FrameOfReference);
    public sealed record AssemblyEdge(string RelatingProductId, string RelatedProductId, string ReferenceDesignator);
    public sealed record GeometryRef(long Id, string EntityType, long ShapeDefinitionId);
    public sealed record PmiRow(long Id, string EntityType, string Name, string Description);
}

public static partial class BimIo {
    public static Fin<ImportedGeometry> ImportGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks) =>
        !format.CanImport ? Fin.Fail<ImportedGeometry>(new BimFault.CodecReject($"import-unsupported:{format.Key}").ToError())
        : format.CataloguePending ? Fin.Fail<ImportedGeometry>(new BimFault.CodecReject($"import-catalogue-pending:{format.Key}:{format.Codec.CataloguePackage.IfNone("unknown")}").ToError())
        : format.Codec == InterchangeCodec.SharpGltf ? Boundary(() => Framed(format, Gltf(format, bytes, clocks.Now)))
        : format.Codec == InterchangeCodec.MeshText ? MeshTextGeometry(format, bytes, clocks.Now)
        : format.Codec == InterchangeCodec.Ply ? Boundary(() => Framed(format, Ply(format, bytes, clocks.Now)))
        : format.Codec == InterchangeCodec.SceneExchange ? Boundary(() => Framed(format, Scene(format, bytes, clocks.Now)))
        : format.Codec == InterchangeCodec.UsdStage ? Boundary(() => Framed(format, Usd(format, bytes, clocks.Now)))
        : format.Codec == InterchangeCodec.AcadSharp ? Boundary(() => Framed(format, AcadGeometry(format, bytes, clocks.Now)))
        : Fin.Fail<ImportedGeometry>(new BimFault.CapabilityMiss($"import-needs-companion:{format.Key}").ToError());

    // OBJ/STL/OFF only — PLY now routes to the dedicated `ply-net` codec (the `Ply` arm) and 3MF/FBX/Collada
    // to the `scene-exchange` codec (the `Scene` arm), so the mesh-text sub-dispatch is one geometry3Sharp leg.
    static Fin<ImportedGeometry> MeshTextGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at) {
        string extension = format.Extensions.Head.Map(static ext => ext.TrimStart('.')).IfNone("");
        return new StandardMeshReader().SupportsFormat(extension)
            ? Boundary(() => Framed(format, MeshText(format, extension, bytes, at)))
            : Fin.Fail<ImportedGeometry>(new BimFault.CodecReject($"import-catalogue-pending:{format.Key}:mesh-text").ToError());
    }

    static ImportedGeometry MeshText(InterchangeFormat format, string extension, ReadOnlyMemory<byte> bytes, Instant at) {
        var builder = new DMesh3Builder();
        var read = new StandardMeshReader { MeshBuilder = builder }
            .Read(new MemoryStream(bytes.ToArray()), extension, ReadOptions.Defaults);
        if (read.code != IOCode.Ok) { throw new InvalidDataException($"<mesh-text-read:{read.code}:{read.message}>"); }
        var mesh = builder.Meshes[0];
        var ordinal = mesh.VertexIndices().AsIterable().Fold((slot: 0, map: Map<int, int>()),
            static (state, vid) => (state.slot + 1, state.map.Add(vid, state.slot)));
        int vertexCount = ordinal.slot;
        var vertices = new float[vertexCount * 3];
        var normals = new float[vertexCount * 3];
        bool hasNormals = mesh.HasVertexNormals;
        ordinal.map.Iter((vid, slot) => {
            var p = mesh.GetVertex(vid);
            var n = hasNormals ? mesh.GetVertexNormal(vid) : Vector3f.AxisZ;
            int v = slot * 3;
            (vertices[v], vertices[v + 1], vertices[v + 2]) = ((float)p.x, (float)p.y, (float)p.z);
            (normals[v], normals[v + 1], normals[v + 2]) = (n.x, n.y, n.z);
        });
        var indices = mesh.TriangleIndices()
            .SelectMany(tid => mesh.GetTriangle(tid) is var tri ? new[] { tri.a, tri.b, tri.c } : [])
            .Select(vid => (long)ordinal.map[vid])
            .ToArray();
        return new ImportedGeometry(format, vertices, normals, indices, vertexCount, indices.Length / 3, at);
    }

    public static Fin<IfcSemanticModel> ImportIfc(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks) =>
        format.Codec == InterchangeCodec.GeometryGym
            ? Boundary(() => Semantic(Database(format, bytes), clocks.Now))
            : Fin.Fail<IfcSemanticModel>(new BimFault.CodecReject($"ifc-codec-miss:{format.Key}").ToError());

    public static Fin<StepSemanticModel> ImportStep(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks) =>
        format.Codec == InterchangeCodec.StepIso10303
            ? Boundary(() => StepReader.Read(format, bytes.Span, clocks.Now))
            : Fin.Fail<StepSemanticModel>(new BimFault.CodecReject($"step-codec-miss:{format.Key}").ToError());

    static Fin<T> Boundary<T>(Func<T> decode) =>
        Try.lift(decode).Run().MapFail(static error => new BimFault.ModelRejected(error.Message).ToError());

    static ImportedGeometry Gltf(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at) {
        bool compressed = Compression.IsPresent(bytes);
        var validation = compressed ? ValidationMode.Skip : ValidationMode.Strict;
        var model = format == InterchangeFormat.Glb
            ? ModelRoot.ParseGLB(new ArraySegment<byte>(bytes.ToArray()), new ReadSettings { Validation = validation })
            : TextContext(bytes, validation).ReadTextSchema2(new MemoryStream(bytes.ToArray()));
        var meshes = compressed ? Compression.Decompress(model, bytes).LogicalMeshes.Decode() : model.LogicalMeshes.Decode();
        return Decoded(format, meshes, at);
    }

    static ReadContext TextContext(ReadOnlyMemory<byte> bytes, ValidationMode validation) {
        var context = ReadContext.CreateFromDictionary(
            new Dictionary<string, ArraySegment<byte>> { ["model.gltf"] = new ArraySegment<byte>(bytes.ToArray()) },
            checkExtensions: true);
        context.Validation = validation;
        return context;
    }

    static ImportedGeometry Decoded(InterchangeFormat format, IReadOnlyList<IMeshDecoder<Material>> meshes, Instant at) {
        var triangles = meshes
            .SelectMany(static mesh => mesh.Primitives)
            .SelectMany(static prim => prim.TriangleIndices.Select(tri => (prim, tri)))
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

    static class Compression {
        public static bool IsPresent(ReadOnlyMemory<byte> glb) =>
            KhrExtension.MeshoptCompression.Key is var meshopt
            && KhrExtension.DracoMeshCompression.Key is var draco
            && JsonChunk(glb) is { Length: > 0 } json
            && (json.Contains(draco, StringComparison.Ordinal) || json.Contains(meshopt, StringComparison.Ordinal));

        // SharpGLTF.Core drops unrecognized extension JSON (Draco/meshopt have no in-box JsonSerializable
        // extension class), so the extension parameters are read from the raw glTF/GLB JSON tree the parse
        // discards — not from a typed ExtraProperties accessor — and the decode writes back through the
        // public Accessor / ModelRoot.UseBufferView surface.
        public static ModelRoot Decompress(ModelRoot model, ReadOnlyMemory<byte> bytes) {
            var root = JsonNode.Parse(JsonChunk(bytes))!.AsObject();
            var meshes = root["meshes"]?.AsArray() ?? new JsonArray();
            for (int m = 0; m < model.LogicalMeshes.Count; m++) {
                var primitives = model.LogicalMeshes[m].Primitives;
                for (int p = 0; p < primitives.Count; p++) {
                    Optional(meshes[m]?["primitives"]?[p]?["extensions"]?[KhrExtension.DracoMeshCompression.Key]?.AsObject())
                        .Iter(extension => DracoPrimitive(primitives[p], extension));
                }
            }
            var views = root["bufferViews"]?.AsArray() ?? new JsonArray();
            for (int v = 0; v < model.LogicalBufferViews.Count; v++) {
                Optional(views[v]?["extensions"]?[KhrExtension.MeshoptCompression.Key]?.AsObject())
                    .Iter(extension => MeshoptView(model, model.LogicalBufferViews[v], extension));
            }
            return model;
        }

        static void DracoPrimitive(MeshPrimitive primitive, JsonObject extension) {
            int bufferView = (int)extension["bufferView"]!;
            var decoded = (DracoMesh)Draco.Decode(primitive.LogicalParent.LogicalParent.LogicalBufferViews[bufferView].Content.ToArray());
            ((Vector3Array)primitive.GetVertexAccessor("POSITION").AsVector3Array()).Fill(Vectors(decoded, AttributeType.Position));
            Optional(primitive.GetVertexAccessor("NORMAL"))
                .Filter(_ => decoded.GetNamedAttributeId(AttributeType.Normal) >= 0)
                .Iter(accessor => ((Vector3Array)accessor.AsVector3Array()).Fill(Vectors(decoded, AttributeType.Normal)));
            ((IntegerArray)primitive.GetIndexAccessor().AsIndicesArray()).Fill(Corners(decoded));
        }

        static IEnumerable<Vector3> Vectors(DracoPointCloud cloud, AttributeType type) =>
            cloud.GetNamedAttribute(type) is { } attribute
                ? Enumerable.Range(0, cloud.NumPoints).Select(point => attribute.GetValueAsVector3(attribute.MappedIndex(point)))
                : [];

        static IEnumerable<uint> Corners(DracoMesh mesh) {
            var face = new int[3];
            for (int id = 0; id < mesh.NumFaces; id++) {
                mesh.ReadFace(id, face);
                yield return (uint)face[0];
                yield return (uint)face[1];
                yield return (uint)face[2];
            }
        }

        static unsafe void MeshoptView(ModelRoot model, BufferView view, JsonObject extension) {
            int count = (int)extension["count"]!;
            int stride = (int)extension["byteStride"]!;
            string mode = (string?)extension["mode"] ?? "ATTRIBUTES";
            string filter = (string?)extension["filter"] ?? "NONE";
            var source = view.Content;
            var destination = new byte[count * stride];
            fixed (byte* dst = destination)
            fixed (byte* src = source.Array) {
                byte* origin = src + source.Offset;
                int status = mode is "TRIANGLES" or "INDICES"
                    ? Meshopt.DecodeIndexBuffer(dst, (nuint)count, (nuint)stride, origin, (nuint)source.Count)
                    : Meshopt.DecodeVertexBuffer(dst, (nuint)count, (nuint)stride, origin, (nuint)source.Count);
                if (status != 0) { throw new InvalidDataException($"<meshopt-decode-status:{status}>"); }
                Filter(filter)(dst, (nuint)count, (nuint)stride);
            }
            model.UseBufferView(
                new ArraySegment<byte>(destination), stride,
                view.IsIndexBuffer ? BufferMode.ELEMENT_ARRAY_BUFFER : BufferMode.ARRAY_BUFFER);
        }

        static unsafe delegate*<void*, nuint, nuint, void> Filter(string filter) => filter switch {
            "OCTAHEDRAL" => &Meshopt.DecodeFilterOct,
            "QUATERNION" => &Meshopt.DecodeFilterQuat,
            "EXPONENTIAL" => &Meshopt.DecodeFilterExp,
            "COLOR" => &Meshopt.DecodeFilterColor,
            _ => &Identity,
        };

        static unsafe void Identity(void* buffer, nuint count, nuint stride) { }

        static string JsonChunk(ReadOnlyMemory<byte> glb) =>
            ReadContext.IdentifyBinaryContainer(new MemoryStream(glb.ToArray()))
                ? ReadContext.ReadJson(new MemoryStream(glb.ToArray()))
                : Encoding.UTF8.GetString(glb.Span);
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
            Decomposition(project),
            MapConversion(project),
            db.Tolerance, at);
    }

    // Every closed AssemblyRel arm extracted — not Aggregates alone — so the Model/structure#ASSEMBLY_TREE
    // BimAssembly.Traverse fold reads a complete decomposition graph. Each IfcRel* family lands its arm on
    // the verified GeometryGym member spellings; the four formerly-unextracted arms join the one Seq.
    static Seq<AssemblyRel> Decomposition(IfcProject project) =>
        project.Extract<IfcRelAggregates>().AsIterable()
            .Map(static r => (AssemblyRel)new AssemblyRel.Aggregates(r.RelatingObject.GlobalId, r.RelatedObjects.Select(static o => o.GlobalId).ToSeq()))
        .Concat(project.Extract<IfcRelNests>().AsIterable()
            .Map(static r => (AssemblyRel)new AssemblyRel.Nests(r.RelatingObject.GlobalId, r.RelatedObjects.Select(static o => o.GlobalId).ToSeq())))
        .Concat(project.Extract<IfcRelContainedInSpatialStructure>().AsIterable()
            .Map(static r => (AssemblyRel)new AssemblyRel.ContainedIn(r.RelatingStructure?.GlobalId ?? "", r.RelatedElements.Select(static o => o.GlobalId).ToSeq())))
        .Concat(project.Extract<IfcRelVoidsElement>().AsIterable()
            .Map(static r => (AssemblyRel)new AssemblyRel.Voids(r.RelatingBuildingElement?.GlobalId ?? "", r.RelatedOpeningElement?.GlobalId ?? "")))
        .Concat(project.Extract<IfcRelConnectsElements>().AsIterable()
            .Where(static r => r is not IfcRelConnectsWithRealizingElements)
            .Map(static r => (AssemblyRel)new AssemblyRel.Connects(r.RelatingElement?.GlobalId ?? "", r.RelatedElement?.GlobalId ?? "")))
        .Where(static rel => rel.Edge.Parent.Length > 0)
        .ToSeq();

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

    // PLY decode through Ply.Net — the dedicated `ply-net` codec retiring the hand-rolled PlyReader.
    // PlyParser.Parse decodes the header-plus-chunked body into the immutable Dataset record graph; the
    // Vertex element's typed x/y/z columns and the Face element's vertex_indices list column materialize
    // once as a typed System.Array (no parser DTO), fan-triangulated onto the canonical triangle-soup.
    static ImportedGeometry Ply(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at) {
        using var stream = new MemoryStream(bytes.ToArray());
        var dataset = PlyParser.Parse(stream, maxChunkSize: 1 << 20);
        var vertex = dataset.Data.First(static d => d.Element.Type == ElementType.Vertex);
        var face = dataset.Data.FirstOrDefault(static d => d.Element.Type == ElementType.Face);
        float[] xs = Column(vertex, "x"), ys = Column(vertex, "y"), zs = Column(vertex, "z");
        var (nx, ny, nz) = (OptionalColumn(vertex, "nx"), OptionalColumn(vertex, "ny"), OptionalColumn(vertex, "nz"));
        int vertexCount = xs.Length;
        var vertices = new float[vertexCount * 3];
        var normals = new float[vertexCount * 3];
        for (int v = 0; v < vertexCount; v++) {
            (vertices[v * 3], vertices[v * 3 + 1], vertices[v * 3 + 2]) = (xs[v], ys[v], zs[v]);
            (normals[v * 3], normals[v * 3 + 1], normals[v * 3 + 2]) = (nx?[v] ?? 0f, ny?[v] ?? 0f, nz?[v] ?? 1f);
        }
        var indices = face is null
            ? Array.Empty<long>()
            : ((int[][])face["vertex_indices"].Data)
                .SelectMany(static corners => Enumerable.Range(1, corners.Length - 2)
                    .SelectMany(k => new long[] { corners[0], corners[k], corners[k + 1] }))
                .ToArray();
        return new ImportedGeometry(format, vertices, normals, indices, vertexCount, indices.Length / 3, at);
    }

    // A PLY column materialized as float[] regardless of on-disk scalar width — Ply.Net types each column as
    // the matching System.Array (float[] for Float32, double[] for Float64, int[] for the integer widths).
    static float[] Column(ElementData element, string name) => element[name].Data switch {
        float[] f  => f,
        double[] d => Array.ConvertAll(d, static x => (float)x),
        int[] i    => Array.ConvertAll(i, static x => (float)x),
        Array a    => Enumerable.Range(0, a.Length).Select(i => Convert.ToSingle(a.GetValue(i))).ToArray(),
        _          => [],
    };

    static float[]? OptionalColumn(ElementData element, string name) =>
        element.Element.Properties.Exists(p => p.Name == name) ? Column(element, name) : null;

    // FBX/Collada/3MF decode through AssimpNetter — the `scene-exchange` codec retiring the hand-rolled
    // ThreeMfReader. One disposable AssimpContext imports with the canonical Bim post-process (Triangulate |
    // JoinIdenticalVertices | GenerateSmoothNormals), then the Scene→Mesh graph folds onto the triangle-soup;
    // handedness rides the per-importer FrameNormalization the row carries, not a blanket MakeLeftHanded.
    static ImportedGeometry Scene(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at) {
        using var context = new AssimpContext();
        using var stream = new MemoryStream(bytes.ToArray());
        var scene = context.ImportFileFromStream(stream,
            PostProcessSteps.Triangulate | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.GenerateSmoothNormals,
            format.Key);
        var soup = scene.Meshes.Fold(SceneSoup.Empty, static (acc, mesh) => acc.Append(mesh));
        return new ImportedGeometry(format, soup.Vertices.ToArray(), soup.Normals.ToArray(), soup.Indices.ToArray(),
            soup.VertexCount, soup.Indices.Count / 3, at);
    }

    // USD decode through UniversalSceneDescription — the `usd-stage` codec. One UsdStage opens the layer
    // stack (the native plugin tree reads the temp path), Traverse walks the namespace, and each UsdGeomMesh
    // prim's points (VtVec3fArray of GfVec3f) and face-vertex counts/indices (VtIntArray) cross the typed-array
    // mesh-bridge seam onto the triangle-soup, fan-triangulating each face. USD is a scene-graph peer — the BIM
    // semantics stay the GeometryGym IFC graph's, never re-derived from USD prim type names.
    static ImportedGeometry Usd(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at) {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{format.Extensions.Head.IfNone(".usd")}");
        File.WriteAllBytes(path, bytes.ToArray());
        try {
            using var stage = UsdStage.Open(path, UsdStage.InitialLoadSet.LoadAll);
            var soup = stage.Traverse().AsIterable()
                .Filter(static prim => prim.GetTypeName().ToString() == "Mesh")
                .Fold(SceneSoup.Empty, static (acc, prim) => acc.AppendUsd(UsdMesh(new UsdGeomMesh(prim))));
            return new ImportedGeometry(format, soup.Vertices.ToArray(), soup.Normals.ToArray(), soup.Indices.ToArray(),
                soup.VertexCount, soup.Indices.Count / 3, at);
        } finally { File.Delete(path); }
    }

    // The typed-array mesh-bridge: GetPointsAttr/GetFaceVertexCountsAttr/GetFaceVertexIndicesAttr each fill a
    // VtValue the typed Vt*Array reads (size()/indexer), per the api-usd mesh-bridge seam; faces fan-triangulate.
    static (Seq<float> Points, Seq<long> Indices) UsdMesh(UsdGeomMesh mesh) {
        var (points, counts, corners) = (new VtValue(), new VtValue(), new VtValue());
        mesh.GetPointsAttr().Get(points, UsdTimeCode.Default());
        mesh.GetFaceVertexCountsAttr().Get(counts, UsdTimeCode.Default());
        mesh.GetFaceVertexIndicesAttr().Get(corners, UsdTimeCode.Default());
        var (xyz, faceCounts, faceIndices) = ((VtVec3fArray)points, (VtIntArray)counts, (VtIntArray)corners);
        var verts = Seq<float>();
        for (uint i = 0; i < xyz.size(); i++) { var p = xyz[(int)i]; verts = verts.Add(p[0]).Add(p[1]).Add(p[2]); }
        var (tris, cursor) = (Seq<long>(), 0);
        for (uint f = 0; f < faceCounts.size(); f++) {
            int n = faceCounts[(int)f];
            for (int k = 1; k < n - 1; k++) {
                tris = tris.Add(faceIndices[cursor]).Add(faceIndices[cursor + k]).Add(faceIndices[cursor + k + 1]);
            }
            cursor += n;
        }
        return (verts, tris);
    }

    // The shared scene-mesh soup the AssimpNetter and USD arms fold onto the contiguous ImportedGeometry triple.
    readonly record struct SceneSoup(Seq<float> Vertices, Seq<float> Normals, Seq<long> Indices, int VertexCount) {
        public static readonly SceneSoup Empty = new(Seq<float>(), Seq<float>(), Seq<long>(), 0);

        public SceneSoup Append(Assimp.Mesh mesh) {
            bool hasNormals = mesh.HasNormals;
            var verts = mesh.Vertices.SelectMany(static p => new[] { p.X, p.Y, p.Z }).ToSeq();
            var norms = Enumerable.Range(0, mesh.VertexCount)
                .SelectMany(i => hasNormals ? new[] { mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z } : new[] { 0f, 0f, 1f }).ToSeq();
            var indices = mesh.Faces.AsIterable()
                .SelectMany(face => Enumerable.Range(1, face.IndexCount - 2)
                    .SelectMany(k => new long[] { VertexCount + face.Indices[0], VertexCount + face.Indices[k], VertexCount + face.Indices[k + 1] }))
                .ToSeq();
            return new SceneSoup(Vertices + verts, Normals + norms, Indices + indices, VertexCount + mesh.VertexCount);
        }

        public SceneSoup AppendUsd((Seq<float> Points, Seq<long> Indices) mesh) =>
            new(Vertices + mesh.Points,
                Normals + Enumerable.Range(0, mesh.Points.Count / 3).SelectMany(static _ => new[] { 0f, 0f, 1f }).ToSeq(),
                Indices + mesh.Indices.Map(i => VertexCount + i),
                VertexCount + mesh.Points.Count / 3);
    }

    // Managed in-process DWG/DXF decode through ACadSharp — the `acad-sharp` codec the format#FORMAT_AXIS
    // Dwg row carries. The DXF/CadDocument is the same decompile-verified reader Fabrication consumes for
    // 2D profiles; here the Bim arm folds the mesh-bearing entities onto the canonical triangle-soup.
    static ImportedGeometry AcadGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at) =>
        AcadReader.Read(format, bytes, at);

    static class AcadReader {
        public static ImportedGeometry Read(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at) {
            using var stream = new MemoryStream(bytes.ToArray());
            var document = IsDxf(bytes) ? DxfReader.Read(stream) : DwgReader.Read(stream);
            var soup = document.Entities.AsIterable().Fold(MeshSoup.Empty, static (soup, entity) => entity switch {
                Mesh mesh         => soup.Faces(mesh.Vertices, mesh.Faces),
                Face3D face       => soup.Quad(face.FirstCorner, face.SecondCorner, face.ThirdCorner, face.FourthCorner),
                Insert insert     => insert.Block is { } block
                    ? block.Entities.AsIterable().OfType<Face3D>().Fold(soup, (acc, f) => acc.Quad(
                        Place(insert, f.FirstCorner), Place(insert, f.SecondCorner), Place(insert, f.ThirdCorner), Place(insert, f.FourthCorner)))
                    : soup,
                _                 => soup,
            });
            return new ImportedGeometry(format, soup.Vertices.ToArray(), soup.Normals.ToArray(), soup.Indices.ToArray(),
                soup.VertexCount, soup.Indices.Count / 3, at);
        }

        // DXF (ascii/binary) opens with the "0\nSECTION" / "AutoCAD Binary DXF" sentinel; DWG with "AC10xx".
        static bool IsDxf(ReadOnlyMemory<byte> bytes) =>
            bytes.Length >= 4 && !(bytes.Span[0] == (byte)'A' && bytes.Span[1] == (byte)'C' && char.IsDigit((char)bytes.Span[2]));

        static XYZ Place(Insert insert, XYZ corner) =>
            new(insert.InsertPoint.X + corner.X * insert.XScale,
                insert.InsertPoint.Y + corner.Y * insert.YScale,
                insert.InsertPoint.Z + corner.Z * insert.ZScale);

        readonly record struct MeshSoup(Seq<float> Vertices, Seq<float> Normals, Seq<long> Indices, int VertexCount) {
            public static readonly MeshSoup Empty = new(Seq<float>(), Seq<float>(), Seq<long>(), 0);

            // A SubDMesh: the vertex list plus the n-gon face index list (each face fan-triangulated).
            public MeshSoup Faces(System.Collections.Generic.IReadOnlyList<XYZ> vertices, System.Collections.Generic.IReadOnlyList<int[]> faces) {
                var verts = vertices.SelectMany(static p => new[] { (float)p.X, (float)p.Y, (float)p.Z }).ToSeq();
                var indices = faces.SelectMany(face => Enumerable.Range(1, face.Length - 2)
                    .SelectMany(k => new[] { (long)(VertexCount + face[0]), VertexCount + face[k], VertexCount + face[k + 1] })).ToSeq();
                return new MeshSoup(Vertices + verts, Normals + Enumerable.Range(0, vertices.Count).SelectMany(static _ => new[] { 0f, 0f, 1f }).ToSeq(),
                    Indices + indices, VertexCount + vertices.Count);
            }

            // A 3DFACE quad (the fourth corner equals the third for a triangle) fan-triangulated.
            public MeshSoup Quad(XYZ a, XYZ b, XYZ c, XYZ d) {
                bool tri = d.Equals(c);
                var corners = tri ? new[] { a, b, c } : new[] { a, b, c, d };
                var verts = corners.SelectMany(static p => new[] { (float)p.X, (float)p.Y, (float)p.Z }).ToSeq();
                var indices = (tri ? new[] { 0, 1, 2 } : new[] { 0, 1, 2, 0, 2, 3 }).Select(i => (long)(VertexCount + i)).ToSeq();
                return new MeshSoup(Vertices + verts, Normals + corners.SelectMany(static _ => new[] { 0f, 0f, 1f }).ToSeq(),
                    Indices + indices, VertexCount + corners.Length);
            }
        }
    }

    static partial class StepReader {
        static readonly FrozenSet<string> GeometryTypes = new[] {
            "ADVANCED_BREP_SHAPE_REPRESENTATION", "MANIFOLD_SOLID_BREP", "FACETED_BREP", "SHELL_BASED_SURFACE_MODEL",
            "B_SPLINE_SURFACE", "B_SPLINE_CURVE", "GEOMETRIC_CURVE_SET", "SHAPE_REPRESENTATION", "TESSELLATED_SHAPE_REPRESENTATION",
        }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        static readonly FrozenSet<string> PmiTypes = new[] {
            "DIMENSIONAL_CHARACTERISTIC_REPRESENTATION", "DRAUGHTING_CALLOUT", "ANNOTATION_OCCURRENCE",
            "DATUM", "DATUM_FEATURE", "GEOMETRIC_TOLERANCE", "DIMENSIONAL_SIZE", "DIMENSIONAL_LOCATION",
        }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        // --- [VALUE] ----------------------------------------------------------------------
        [Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
        abstract partial record Arg {
            private Arg() { }
            public sealed record Ref(long Id) : Arg;
            public sealed record Text(string Value) : Arg;
            public sealed record Enum(string Value) : Arg;
            public sealed record Number(double Value) : Arg;
            public sealed record List(Seq<Arg> Items) : Arg;
            public sealed record Untyped(string Token) : Arg;
            public sealed record Typed(string Keyword, Seq<Arg> Args) : Arg;
        }

        sealed record Instance(long Id, string Keyword, Seq<Arg> Args);

        public static StepSemanticModel Read(InterchangeFormat format, ReadOnlySpan<byte> bytes, Instant at) {
            var text = Strip(System.Text.Encoding.UTF8.GetString(bytes));
            var (header, data) = Sections(text);
            var instances = data.Map(Parse).ToSeq();
            var graph = instances.ToDictionary(static i => i.Id, static i => i);
            var schema = HeaderField(header, "FILE_SCHEMA");
            return new StepSemanticModel(
                format.StepProtocol, schema, HeaderField(header, "FILE_NAME"),
                instances.Filter(static i => i.Keyword is "PRODUCT")
                    .Map(static i => new StepSemanticModel.ProductRow(i.Id, Str(i.Args, 0), Str(i.Args, 1), Str(i.Args, 2))).ToSeq(),
                instances.Filter(static i => i.Keyword is "PRODUCT_DEFINITION")
                    .Map(i => Definition(i, graph)).ToSeq(),
                instances.Filter(static i => i.Keyword is "NEXT_ASSEMBLY_USAGE_OCCURRENCE")
                    .Map(i => Assembly(i, graph)).ToSeq(),
                instances.Filter(static i => GeometryTypes.Contains(i.Keyword))
                    .Map(static i => new StepSemanticModel.GeometryRef(i.Id, i.Keyword, RefAt(i.Args, 0))).ToSeq(),
                instances.Filter(static i => PmiTypes.Contains(i.Keyword))
                    .Map(static i => new StepSemanticModel.PmiRow(i.Id, i.Keyword, Str(i.Args, 0), Str(i.Args, 1))).ToSeq(),
                at);
        }

        // --- [TOKENIZE] -------------------------------------------------------------------
        static string Strip(string source) {
            var sink = new System.Text.StringBuilder(source.Length);
            bool inString = false, inComment = false;
            for (int i = 0; i < source.Length; i++) {
                char ch = source[i];
                if (inComment) { if (ch == '*' && i + 1 < source.Length && source[i + 1] == '/') { inComment = false; i++; } }
                else if (inString) { sink.Append(ch); if (ch == '\'') { inString = false; } }
                else if (ch == '\'') { sink.Append(ch); inString = true; }
                else if (ch == '/' && i + 1 < source.Length && source[i + 1] == '*') { inComment = true; i++; }
                else { sink.Append(ch); }
            }
            return sink.ToString();
        }

        static (string Header, Seq<string> Data) Sections(string text) {
            int header = text.IndexOf("HEADER", StringComparison.Ordinal);
            int headerEnd = header >= 0 ? text.IndexOf("ENDSEC", header, StringComparison.Ordinal) : -1;
            int data = headerEnd >= 0 ? text.IndexOf("DATA", headerEnd, StringComparison.Ordinal) : text.IndexOf("DATA", StringComparison.Ordinal);
            int endsec = data >= 0 ? text.IndexOf("ENDSEC", data, StringComparison.Ordinal) : -1;
            string headerBody = header >= 0 && headerEnd > header ? text[header..headerEnd] : "";
            string dataBody = data >= 0 ? text[(text.IndexOf(';', data) + 1)..(endsec < 0 ? text.Length : endsec)] : "";
            return (headerBody, Statements(dataBody));
        }

        static Seq<string> Statements(string body) {
            var (rows, depth, inString, start) = (Seq<string>(), 0, false, 0);
            for (int i = 0; i < body.Length; i++) {
                char ch = body[i];
                if (inString) { if (ch == '\'') { inString = false; } }
                else if (ch == '\'') { inString = true; }
                else if (ch == '(') { depth++; }
                else if (ch == ')') { depth--; }
                else if (ch == ';' && depth == 0) {
                    var statement = body[start..i].Trim();
                    if (statement.StartsWith('#')) { rows = rows.Add(statement); }
                    start = i + 1;
                }
            }
            return rows;
        }

        // --- [PARSE] ----------------------------------------------------------------------
        static Instance Parse(string statement) {
            int eq = statement.IndexOf('=');
            long id = long.Parse(statement[1..eq].Trim(), CultureInfo.InvariantCulture);
            string rhs = statement[(eq + 1)..].Trim();
            int paren = rhs.IndexOf('(');
            return rhs.StartsWith('(')
                ? new Instance(id, "", ParseList(rhs, out _).Items)
                : new Instance(id, rhs[..paren].Trim().ToUpperInvariant(), ParseList(rhs[paren..], out _).Items);
        }

        static Arg.List ParseList(string source, out int consumed) {
            var (items, cursor) = (Seq<Arg>(), 1);
            while (cursor < source.Length && source[cursor] != ')') {
                if (source[cursor] is ' ' or ',' or '\t' or '\n' or '\r') { cursor++; continue; }
                items = items.Add(ParseArg(source, ref cursor));
            }
            consumed = cursor + 1;
            return new Arg.List(items);
        }

        static Arg ParseArg(string source, ref int cursor) {
            char ch = source[cursor];
            return ch switch {
                '#' => Reference(source, ref cursor),
                '\'' => Quoted(source, ref cursor),
                '.' => Enumerated(source, ref cursor),
                '(' => SubList(source, ref cursor),
                '$' or '*' => Sentinel(source, ref cursor),
                _ when char.IsLetter(ch) => TypedOrToken(source, ref cursor),
                _ => Scalar(source, ref cursor),
            };
        }

        static Arg Reference(string source, ref int cursor) {
            int start = ++cursor;
            while (cursor < source.Length && char.IsDigit(source[cursor])) { cursor++; }
            return new Arg.Ref(long.Parse(source[start..cursor], CultureInfo.InvariantCulture));
        }

        static Arg Quoted(string source, ref int cursor) {
            int start = ++cursor;
            while (cursor < source.Length) {
                if (source[cursor] == '\'' && (cursor + 1 >= source.Length || source[cursor + 1] != '\'')) { break; }
                cursor += source[cursor] == '\'' ? 2 : 1;
            }
            string value = source[start..cursor].Replace("''", "'");
            cursor++;
            return new Arg.Text(value);
        }

        static Arg Enumerated(string source, ref int cursor) {
            int start = ++cursor;
            while (cursor < source.Length && source[cursor] != '.') { cursor++; }
            string value = source[start..cursor];
            cursor++;
            return new Arg.Enum(value);
        }

        static Arg SubList(string source, ref int cursor) {
            var list = ParseList(source[cursor..], out int consumed);
            cursor += consumed;
            return list;
        }

        static Arg Sentinel(string source, ref int cursor) {
            string token = source[cursor].ToString();
            cursor++;
            return new Arg.Untyped(token);
        }

        static Arg TypedOrToken(string source, ref int cursor) {
            int start = cursor;
            while (cursor < source.Length && (char.IsLetterOrDigit(source[cursor]) || source[cursor] == '_')) { cursor++; }
            string keyword = source[start..cursor];
            if (cursor < source.Length && source[cursor] == '(') {
                var list = ParseList(source[cursor..], out int consumed);
                cursor += consumed;
                return new Arg.Typed(keyword.ToUpperInvariant(), list.Items);
            }
            return new Arg.Untyped(keyword);
        }

        static Arg Scalar(string source, ref int cursor) {
            int start = cursor;
            while (cursor < source.Length && source[cursor] is not (',' or ')')) { cursor++; }
            string token = source[start..cursor].Trim();
            return double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double number)
                ? new Arg.Number(number)
                : new Arg.Untyped(token);
        }

        // --- [EXTRACT] --------------------------------------------------------------------
        static StepSemanticModel.DefinitionRow Definition(Instance definition, Dictionary<long, Instance> graph) {
            long formation = RefAt(definition.Args, 2);
            var formationInstance = Resolve(graph, formation);
            long product = formationInstance.Map(f => RefAt(f.Args, 2)).IfNone(0L);
            return new StepSemanticModel.DefinitionRow(
                definition.Id, product,
                formationInstance.Map(f => Str(f.Args, 0)).IfNone(""),
                Str(definition.Args, 1),
                Str(definition.Args, 3));
        }

        static StepSemanticModel.AssemblyEdge Assembly(Instance usage, Dictionary<long, Instance> graph) =>
            new(ProductId(graph, RefAt(usage.Args, 3)), ProductId(graph, RefAt(usage.Args, 4)), Str(usage.Args, 5));

        static string ProductId(Dictionary<long, Instance> graph, long definitionId) =>
            Resolve(graph, definitionId)
                .Bind(def => Resolve(graph, RefAt(def.Args, 2)))
                .Bind(formation => Resolve(graph, RefAt(formation.Args, 2)))
                .Map(product => Str(product.Args, 0))
                .IfNone("");

        static Option<Instance> Resolve(Dictionary<long, Instance> graph, long id) =>
            graph.TryGetValue(id, out var instance) ? Optional(instance) : None;

        static Option<Arg> ArgAt(Seq<Arg> args, int index) =>
            index >= 0 && index < args.Count ? Optional(args[index]) : None;

        static long RefAt(Seq<Arg> args, int index) =>
            ArgAt(args, index).Bind(static a => a is Arg.Ref r ? Optional(r.Id) : None).IfNone(0L);

        static string Str(Seq<Arg> args, int index) =>
            ArgAt(args, index).Match(
                Some: static a => a switch { Arg.Text t => t.Value, Arg.Enum e => e.Value, Arg.Untyped u => u.Token, _ => "" },
                None: static () => "");

        static string HeaderField(string header, string keyword) =>
            header.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) is var head and >= 0
            && header.IndexOf('(', head) is var open and >= 0
                ? ParseList(header[open..], out _).Items
                    .Choose(static a => a switch {
                        Arg.Text t => Optional(t.Value),
                        Arg.List { Items: var nested } => nested.Choose(static n => n is Arg.Text nt ? Optional(nt.Value) : None).Head,
                        _ => None,
                    })
                    .Head.IfNone("")
                : "";
    }
}
```

## [03]-[SPECKLE_SEAM]

- Owner: `BimIo.ImportSpeckle` — the Speckle object-graph arm of the import fold, folding a deserialized `Speckle.Sdk.Models.Base` tree onto the canonical `ImportedGeometry` mesh-scene carrier and the `IfcSemanticModel` graph; there is no `SpeckleImporter` type and no parallel decode family — the seam is a third entrypoint on the existing `BimIo` capsule, symmetric to `ImportGeometry`/`ImportIfc`, consuming the receive-side `Base` the Persistence `csharp:Persistence/Sync/collaboration#SPECKLE_SYNC` `IOperations.Receive` returns.
- Entry: `BimIo.ImportSpeckle(Base root, ClockPolicy clocks)` projecting the display-mesh geometry, and `BimIo.ImportSpeckleSemantic(Base root, ClockPolicy clocks)` projecting the host-object semantic graph — `Fin<T>` aborts on a graph with no displayable geometry or a malformed display mesh, projecting the Speckle exception onto `BimFault.ModelRejected` at the boundary so domain code never sees a `Speckle.Sdk.SpeckleException`; the `Base` arrives already deserialized, so the seam mints no transport, no `IOperations` reference, and no second graph walk beyond the package-owned traversal.
- Auto: the geometry fold runs the package-owned `BaseExtensions.Flatten(Base, BaseExtensions.BaseRecursionBreaker?)` deduplicating graph walk, projects each node's `BaseExtensions.TryGetDisplayValue(Base)` display list to its `Mesh` members, and decodes each `Mesh` — the flat `vertices`/`vertexNormals` (`List<double>`, flat `x,y,z`) and length-prefixed `faces` (`List<int>`, each face `[n, i0, … i(n-1)]`) triangulate through a fan over the n-gon, scaled onto the canonical metre frame by `Units.GetConversionFactor(mesh.units, Units.Meters)` so a millimetre or foot Speckle model lands in kernel units; a node that `IsDisplayableObject` is false yet carries non-mesh geometry (`Brep`/`Surface`/`Curve` with no `displayValue`) routes its content to `tessellation#TESSELLATION_BRIDGE` over the GLB rail rather than evaluating a BRep in-process; the semantic fold projects every `DataObject` (and its `RhinoObject`/`RevitObject`/`ArchicadObject`/`TeklaObject`/`Civil3dObject`/`AutocadObject` host-object subtypes) onto an `IfcSemanticModel.ProductRow` keyed on `Base.applicationId`, its `DataObject.properties` (`Dictionary<string, object?>`) flattened to `PropertyRow`, and the `TraversalContext.Parent` chain reconstructing the spatial `SpatialNode` containment.
- Receipt: the `ModelLoad` receipt case carries the format key `InterchangeFormat.Glb.Key` proxy for the decoded scene, the codec key `speckle-base`, the `Base.GetTotalChildrenCount()` source object count, and elapsed; a semantic ingest stamps the host-object count and the distinct `speckle_type` discriminants extracted; emission rides the sink port at the composition edge.
- Packages: Speckle.Sdk, Speckle.Objects, SharpGLTF.Core, NodaTime, LanguageExt.Core, Rasm
- Growth: a new Speckle geometry leaf is one arm on the `DisplayMeshes` projection keyed on the `IDisplayValue<T>` payload type; a new host-object discriminant is one `speckle_type`-keyed row on the semantic projection; a non-mesh evaluation never grows a managed Speckle tessellator — it widens the `tessellation#TESSELLATION_BRIDGE` request, never this fold.
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

## [04]-[REIMPORT]

- Owner: `BimIo.Reimport` the incremental delta-reimport fold re-projecting only the changed elements against a prior `BimModel` snapshot through the `Review/diff#MODEL_DIFF` content-key, so a large model's minor revision costs the delta, not the whole graph; `ReimportResult` the receipt carrying the patched `BimModel` plus the `ModelDiff` change-set the reimport produces in one fold.
- Entry: `BimIo.Reimport(InterchangeFormat format, ReadOnlyMemory<byte> revisedBytes, BimModel prior, ClockPolicy clocks)` ingests a revised source and, joining against `prior` by `GlobalId` plus the `Review/diff#MODEL_DIFF` `ElementFingerprint` content-key, re-projects only the added/modified/moved elements while reusing the unchanged element rows verbatim — `Fin<T>` aborts on a codec reject or a malformed revised graph (`Model/faults#FAULT_BAND` `BimFault.CodecReject`/`BimFault.ModelRejected`) lowered with `.ToError()`; the IFC semantic graph still parses fully at ingest (the GeometryGym parse is whole-file), so the incrementality is in the projection-and-fingerprint stage — a revised element whose content-key matches its prior row reuses the prior `BimElement` verbatim (preserving its kernel `GeometryHandle` by reference, no re-tessellation), and only the changed set re-projects through the `Model/elements#ELEMENT_MODEL` `BimModel.Project` fold.
- Auto: `Reimport` runs the full `ImportIfc` semantic ingest on the revised bytes, fingerprints each revised product row through the `Review/diff#MODEL_DIFF` `ElementFingerprint` content-key, builds the prior fingerprint index by `GlobalId`, and partitions the revised set: a revised `GlobalId` whose content-key matches the prior row's content-key reuses the prior `BimElement` (the `GeometryHandle` and every binding preserved by reference), a revised `GlobalId` absent from the prior index or whose content-key diverged re-projects through `BimModel.Project` for that element alone, and a prior `GlobalId` absent from the revised set drops — so the patched model is `(reusedPriorRows ++ reprojectedChangedRows)`, and the `ModelDiff.Between(prior, patched)` falls out of the same fingerprint join for free; the reuse fold reads the prior `BimModel.Elements` index once and the re-projection fold runs only over the changed `IfcSemanticModel.ProductRow` subset, the `Map<string, ElementFingerprint>` content-key comparison driving the partition rather than a field-by-field diff.
- Receipt: the `ReimportResult` carries the patched `BimModel` (the prior unchanged rows reused by reference plus the re-projected changed rows) and the `ModelDiff` change-set the federation cache reads — a delta-cost minor revision, the federation patch storage reading only the changed rows, and the diff falling out of the reimport rather than a second diff pass; the `csharp:Rasm.Persistence/Query#CONTENT_KEY` reimport prior-`BimModel` content-key delta join reads the patched model by the same content-key the diff carries.
- Packages: SharpGLTF.Core, GeometryGymIFC_Core, NodaTime, System.IO.Hashing, LanguageExt.Core, Rasm
- Growth: a new reuse dimension (a property-only delta reusing the geometry but re-projecting the property bag) is one finer content-key partition on the same fold; a new source format rides the existing `ImportIfc`/`ImportGeometry` dispatch before the fingerprint partition; the diff falls out of the same fingerprint join; never a second reimport entrypoint, never a parallel delta store, and never a re-tessellation of an unchanged element.
- Boundary: the reimport reuses the `Review/diff#MODEL_DIFF` `ElementFingerprint` content-key as the change-detection key rather than a second identity scheme — a field-by-field element comparison or a `Guid`-keyed delta join is the deleted form; the unchanged-element reuse preserves the kernel `GeometryHandle` by reference and a re-tessellation of a content-key-matched element is the named seam violation; the IFC semantic graph parses fully at ingest (GeometryGym is whole-file), so the incrementality is in the projection-and-fingerprint stage, not the byte parse — claiming a partial byte parse is the deleted form; the `ModelDiff` falls out of the fingerprint join and a second diff pass over the patched model is the deleted form; the patched `BimModel` is the one `Model/elements#ELEMENT_MODEL` model shape, never a parallel delta-model; a reimport rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` through `.ToError()`.

```csharp signature
public sealed record ReimportResult(BimModel Patched, ModelDiff Diff);

public static partial class BimIo {
    public static Fin<ReimportResult> Reimport(InterchangeFormat format, ReadOnlyMemory<byte> revisedBytes, BimModel prior, ClockPolicy clocks) =>
        ImportIfc(format, revisedBytes, clocks)
            .Bind(semantic => BimModel.Project(semantic, clocks))
            .Map(revised => Patch(prior, revised));

    static ReimportResult Patch(BimModel prior, BimModel revised) {
        var priorByKey = prior.Elements.ToMap(static e => e.GlobalId, static e => ModelDiff.Fingerprint(e).ContentKey);
        var priorRows = prior.Elements.ToMap(static e => e.GlobalId);
        var patchedElements = revised.Elements.Map(element =>
            priorByKey.Find(element.GlobalId).Match(
                Some: priorKey => priorKey == ModelDiff.Fingerprint(element).ContentKey ? priorRows[element.GlobalId] : element,
                None: () => element));
        var patched = revised with { Elements = patchedElements };
        return new ReimportResult(patched, ModelDiff.Between(prior, patched));
    }
}
```

## [05]-[RESEARCH]

- [ACADSHARP_DWG_DECODE]: the `acad-sharp` codec the `Exchange/format#FORMAT_AXIS` `Dwg` row carries reads DWG/DXF in-process through the decompile-verified `ACadSharp` surface (catalogued folder-local at `.api/api-acadsharp`) — `DxfReader.Read(Stream)`/`DwgReader.Read(Stream)` return a `CadDocument` whose `Entities` (an alias for `ModelSpace.Entities`) carry the drawing geometry, and the Bim arm folds the mesh-bearing entities onto the canonical `ImportedGeometry` triangle-soup: `Mesh` (the `Vertices` `List<XYZ>` plus the `Faces` `List<int[]>` n-gon index list, each face fan-triangulated), `Face3D` (the `FirstCorner`..`FourthCorner` `XYZ` quad, the fourth corner equal to the third for a triangle), and the `Insert`-referenced `Face3D` set placed through `InsertPoint`/`XScale`/`YScale`/`ZScale` — the 2D profile entities (`LwPolyline`/`Polyline2D`/`Arc`/`Circle`/`Spline`) the `csharp:Rasm.Fabrication` `Polygon/import` boundary owns are skipped, the one `CadDocument` read by two folders each projecting its owned entity families; `ACadSharp` is pure-managed AnyCPU IL, osx-arm64-safe, already consumed by Fabrication and AppUi, so the DWG/DXF round-trip lands managed without crossing the Python companion, the `netDxf` (DXF-only) reader NOT admitted because `ACadSharp` supersedes it (managed DWG AND DXF), and the reader exception lowers to `BimFault.ModelRejected` through the `BimIo.Boundary` funnel.
- [NATIVE_FORMAT_BRIDGES]: the Revit `.rvt` and Navisworks `.nwc`/`.nwd` native readers ride the `native-companion` codec through the Compute companion process (the managed C# branch has no native loader for the proprietary application formats); DWG/DXF is now managed in-process through the `acad-sharp` codec (the `[ACADSHARP_DWG_DECODE]` note above), no longer a `native-companion` two-hop; the `mesh-text` OBJ/STL/OFF decode is managed in-process through the `geometry3Sharp` `StandardMeshReader`/`OBJFormatReader`/`STLFormatReader`/`OFFFormatReader` surface decompile-verified in `.api/api-geometry3sharp`, projecting the `DMesh3` (`VertexIndices`/`TriangleIndices`/`GetVertex`/`GetVertexNormal`/`GetTriangle`) onto the canonical `ImportedGeometry` triangle-soup; PLY and 3MF have LEFT the `mesh-text` codec because `geometry3Sharp` ships no PLY and no 3MF handler — PLY is the dedicated `ply-net` codec composing `Ply.Net` `PlyParser.Parse(stream, maxChunkSize)` over the immutable `Dataset`/`ElementData` record graph (the `Vertex` element's typed `x`/`y`/`z`/`nx`/`ny`/`nz` columns read as a `System.Array` per `DataType` and the `Face` element's `vertex_indices` list column fan-triangulated, ascii/`binary_little_endian`/`binary_big_endian` off `Header.Format`), retiring the hand-rolled BCL `PlyReader`, and 3MF is the `scene-exchange` codec composing the `AssimpNetter` `AssimpContext.ImportFileFromStream` `Scene`→`Mesh` fold under the canonical `Triangulate | JoinIdenticalVertices | GenerateSmoothNormals` post-process, retiring the hand-rolled BCL `ThreeMfReader` OPC/ZIP parse — `AssimpNetter` ships its own osx-arm64 `libassimp.dylib` admitted as the one scene-exchange owner (FBX/Collada/3MF), so the former native-coupling rejection no longer holds and the rejected reader picks that DO stand are `lib3mf` (native C++) and `Aspose.3D` (closed/commercial); each codec materializes one contiguous `ImportedGeometry` boundary allocation per the boundary-mapping law, the leaked `Ply.Net.*`/`Assimp.*` package types never crossing past `Exchange/import`.
- [SPECKLE_CATALOGUE]: the `Speckle.Sdk`/`Speckle.Objects` member spellings the `ImportSpeckle` fold composes — `Speckle.Sdk.Models.Base` (`id`/`applicationId`/`speckle_type`/`GetTotalChildrenCount`), `Speckle.Sdk.Models.Extensions.BaseExtensions.Flatten`/`Traverse`/`TryGetDisplayValue`/`IsDisplayableObject` with the `BaseRecursionBreaker` delegate, `Speckle.Sdk.Models.GraphTraversal.TraversalContext.Parent`/`PropName`/`Current`, `Speckle.Sdk.Common.Units.GetConversionFactor`/`Meters`, `Speckle.Objects.Geometry.Mesh` (`vertices`/`faces`/`vertexNormals` `List<double>`/`List<int>`, length-prefixed n-gon `faces`, `units`, `VerticesCount`), `Speckle.Objects.Geometry.Brep` (`displayValue` `List<Mesh>`), `Speckle.Objects.IDisplayValue<out T>`, and `Speckle.Objects.Data.DataObject` (`name`/`displayValue` `List<Base>`/`properties` `Dictionary<string, object?>`) and its host-object subtypes (`RevitObject`/`ArchicadObject`/`TeklaObject`/`Civil3dObject`/`AutocadObject`/`RhinoObject`, each extending `DataObject`) — are decompile-verified against the `Speckle.Sdk`/`Speckle.Objects` 3.21.1 assemblies and catalogued folder-local at `.api/api-speckle`, which records the `Mesh.vertexNormals` flat-normal member, the `DataObject.properties` `Dictionary<string, object?>` shape, and the host-object subtype roster the cross-folder `csharp:Persistence/.api/api-speckle` sync catalogue elides.
- [IFC5_ECS]: the IFC5 ECS-JSON (`.ifcx`) component-graph parse rides the GeometryGym IFC5 surface for the semantic graph and the Compute companion for native-grade tessellation; the `ifc5` row mirrors the IFC4x3 ingest with the ECS-component projection. IFC4.3 ADD2 is the production baseline (`ReleaseVersion.IFC4X3_ADD2`); IFC5 is the componentized/granular `.ifcx` architecture in active public development, so the `ifc5` row is a forward-looking GeometryGym IFC5-surface row grounding against the GeometryGym IFC5 member surface at alignment.
