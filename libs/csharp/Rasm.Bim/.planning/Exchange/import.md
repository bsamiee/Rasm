# [BIM_IMPORT_RAIL]

The foreign-bytes ingest rail: one `BimIo` import fold over the `format#FORMAT_AXIS` `InterchangeFormat` rows, dispatching the managed glTF/GLB mesh-and-scene decode through SharpGLTF, the STL/OBJ/OFF mesh-text arm through `geometry3Sharp`, the dedicated PLY decode through `Ply.Net` (retiring the hand-rolled BCL `PlyReader`), the FBX/Collada/3MF scene decode through `AssimpNetter` (retiring the hand-rolled BCL `ThreeMfReader`), the OpenUSD scene decode through `UniversalSceneDescription` `UsdStage`, the in-process IFC/IFC5 graph decode through GeometryGym to the live `DatabaseIfc` the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector` lowers to a seam `GraphDelta`, the in-process ISO 10303-21 Part-21 product-structure semantic ingest through the BCL-only `StepReader`, the AP242/native-companion two-hop geometry route, and the Speckle `Base` object-graph seam folding a deserialized `Speckle.Sdk.Models.Base` tree onto the `ImportedGeometry` display-mesh carrier and a seam `GraphDelta` through the `SpeckleProjector : IElementProjection` host-object projection — never tessellated BRep, never a lossy `IfcSemanticModel` flat-row re-projection. The import rail OWNS the foreign byte->carrier decode; the entity walk, the full `IfcRel*` relationship roster, the typed property/quantity projection, `OwnerHistory`, and `StepHeader` are the `Rasm.Element` seam projector's, read off the live graph. The page composes the kernel `Rasm` geometry and consumes the `format#FORMAT_AXIS` codec/frame rows as settled vocabulary; an IFC/native/Speckle-non-mesh geometry request routes to `tessellation#TESSELLATION_BRIDGE`. The page is HOST-LOCAL in posture; the Speckle seam composes `Speckle.Sdk`/`Speckle.Objects` and runs only in the host-neutral exchange assembly, never inside the in-Rhino plugin ALC.

## [01]-[INDEX]

- [01]-[IMPORT_RAIL]: foreign-bytes ingest — managed mesh decode to `ImportedGeometry`, the in-process IFC/IFC5 decode to the live `DatabaseIfc` the seam `SemanticProjector` lowers, and the in-process STEP product-structure `StepSemanticModel`.
- [02]-[SPECKLE_SEAM]: Speckle `Base` object-graph — the display-mesh decode to `ImportedGeometry` and the `SpeckleProjector : IElementProjection` host-object projection to a seam `GraphDelta`.
- [03]-[REIMPORT]: projector-polymorphic incremental re-ingest — re-project a revised source, reconcile to the prior `ElementGraph` by `ExternalId`, emit the delta-cost `GraphDelta`.

## [02]-[IMPORT_RAIL]

- Owner: `BimIo` — the import fold over `InterchangeFormat`, dispatching the managed glTF/GLB mesh-and-scene decode through SharpGLTF, the OBJ/STL/OFF mesh-text arm through the `geometry3Sharp` `StandardMeshReader`, the dedicated PLY decode through the `Ply.Net` `PlyParser` (the `ply-net` codec retiring the BCL `PlyReader`), the FBX/Collada/3MF scene decode through the `AssimpNetter` `AssimpContext` (the `scene-exchange` codec retiring the BCL `ThreeMfReader`), the OpenUSD scene decode through the `UniversalSceneDescription` `UsdStage` (the `usd-stage` codec), the in-process IFC/IFC5 decode through GeometryGym to the live `DatabaseIfc`, the managed BCL-only `StepReader` ISO 10303-21 Part-21 entity-instance-graph semantic ingest over the `StepIso10303` codec, and the AP242/native-companion two-hop geometry route; `ImportedGeometry` the decoded mesh-scene carrier, the live `DatabaseIfc` the IFC graph the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector` captures and lowers to a seam `GraphDelta`, `StepSemanticModel` the ISO 10303 product-structure projection.
- Entry: `BimIo.ImportGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks, Op key)` for the managed mesh-and-scene path (dispatching by `InterchangeCodec` to SharpGLTF, the `geometry3Sharp` mesh-text arm, `Ply.Net`, `AssimpNetter`, `UsdStage`, or ACadSharp); `BimIo.ImportIfc(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Op key)` for the in-process IFC/IFC5 decode to the live `DatabaseIfc` the seam `SemanticProjector` captures; `BimIo.ImportStep(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks, Op key)` for the in-process ISO 10303-21 Part-21 product-structure semantic graph — `Fin<T>` aborts on a codec reject (`Model/faults#FAULT_BAND` `BimFault.CodecReject`) or a companion-required geometry request (`BimFault.CapabilityMiss`), each `Op`-keyed case lifting BARE onto the `Fin<T>` rail (band 2600 IS the `Expected` `Code` — no `.ToError()` hop), the foreign decode arity discriminating on the row's `InterchangeCodec` so a path lands one decode without a call-site type branch, projecting the package or parse exception onto `BimFault.ModelRejected(key, error.Message)` at the boundary so domain code never sees the SharpGLTF `ModelException`, the GeometryGym parse fault, or a malformed-Part-21 `InvalidDataException`.
- Auto: binary GLB decode lands through `ModelRoot.ParseGLB(ArraySegment<byte>)` and text `.gltf` decode through `ReadContext.ReadTextSchema2(Stream)` then a `Decompress` pre-decode branch reading the parsed model's `KHR_draco_mesh_compression` primitive extension and `EXT_meshopt_compression` bufferView extension and routing the compressed payload through the package-owned `Draco.Decode(byte[])` and `Meshopt.DecodeVertexBuffer`/`DecodeIndexBuffer`/`DecodeFilterOct`/`DecodeFilterQuat`/`DecodeFilterExp`/`DecodeFilterColor` decoders before `model.LogicalMeshes.Decode()` projecting `IMeshDecoder<Material>` primitives to `ImportedGeometry` vertex and index spans with zero intermediate file; the IFC semantic path constructs the live `DatabaseIfc` over the bytes through `DatabaseIfc.ParseString`/`ReadXMLDoc`/`ReadJSON` by the row's format — the ifcJSON/ifcXML construction reading its `ReleaseVersion` from `SemanticProjector.Sniff(bytes, format)` BEFORE construction [H8] rather than a hardcoded `IFC4X3_ADD2`, the in-process IFC graph the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector` captures internally and lowers to a seam `GraphDelta`; the entity walk (`db.Project.Extract<T>()` over the spatial hierarchy, products with the `ParserIfc.IdentifyIfcClass` predefined split, property/quantity sets, materials, classifications, type objects with the `IfcTypeProduct.RepresentationMaps` instanced-geometry content key, the grouping/zone overlays, the `IfcMapConversion`/`IfcProjectedCRS` georeference, and the FULL `IfcRel*` relationship roster including the eight families the retired flat rows stranded), the per-bag `InheritanceMode` stamp, the `OwnerHistory`, and the `StepHeader` are the projector's — read off this live graph, never a lossy `IfcSemanticModel` flat-row re-projection here and never tessellated BRep.
- Receipt: the `ModelLoad` receipt case carries the format key, codec key, source byte count, and elapsed for a managed mesh import; an IFC decode stamps the schema version (`db.Release`) and the model-view (`db.ModelView`) read off the live `DatabaseIfc` (the entity-count receipt rides the `SemanticProjector`'s delta, not the import rail); a STEP semantic ingest stamps the `StepProtocol`, the `FILE_SCHEMA` schema name, and the product/definition/assembly/geometry-ref counts; emission rides the sink port at the composition edge.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Runtime, GeometryGymIFC_Core, Openize.Drako, Alimer.Bindings.MeshOptimizer, geometry3Sharp, Ply.Net, AssimpNetter, UniversalSceneDescription, ACadSharp, NodaTime, LanguageExt.Core, Rasm
- Growth: a new managed import is one codec arm on the import fold keyed by the `InterchangeFormat.Codec` row; a new extracted IFC entity family is one `Extract<T>` arm on the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector`, never on the import rail (which owns only the byte->`DatabaseIfc` decode); a new extracted STEP entity family is one `Keyword`-filtered projection on `StepSemanticModel` over the resolved instance graph; a new STEP application protocol is one `InterchangeFormat` row carrying its `StepProtocol` discriminant — the single `StepReader` reads the protocol off `format.StepProtocol` and the entity-instance grammar is protocol-agnostic, so AP203/AP214/AP242 share one reader and one codec without a per-protocol reader; a new glTF compression codec is one `KhrEncoder`-keyed arm on the `Decompress` pre-decode branch symmetric to the `export#EXPORT_RAIL` `GlbBytes` compression switch, never a second importer.
- Boundary: `BimIo` is the page boundary capsule and its codec arms carry the language-owned statement forms the foreign package decode requires; glTF mesh decode rides the `MeshDecoder.Decode` runtime contract reading `IMeshPrimitiveDecoder.GetPosition`/`GetNormal`/`TriangleIndices` (an accessor-based contract returning per-vertex `Vector3`/index-tuple values, so the decode materializes one contiguous `ImportedGeometry` vertex/normal/index triple at the boundary — the accessor contract admits no zero-copy span into SharpGLTF's internal buffers, so the one boundary materialization, not a per-primitive `float[]` proliferation, is the allocation point); the `mesh-text` decode arm reads the OBJ/STL/OFF mesh-text containers through the `geometry3Sharp` `StandardMeshReader.Read(Stream, extension, ReadOptions)` extension-dispatched reader into a `DMesh3Builder`, projecting the resulting `DMesh3` (`VertexIndices()`/`TriangleIndices()` over the refcounted sparse pools, `GetVertex(int)` `Vector3d` position, `GetVertexNormal(int)` `Vector3f` normal, `GetTriangle(int)` `Index3i`) onto the same contiguous `ImportedGeometry` vertex/normal/index triple the glTF arm materializes — one boundary allocation, the `DMesh3` sparse index space iterated through its live-id enumerators rather than a dense `0..VertexCount` loop because the refcounted pool leaves holes; the `mesh-text` arm is now geometry3Sharp ONLY (OBJ/STL/OFF) because PLY and 3MF left the codec — PLY is the dedicated `ply-net` codec composing `Ply.Net` `PlyParser.Parse(stream, maxChunkSize)` (the `Ply` arm: the `Dataset.Data` `ElementData` rows read the `Vertex` element's typed `x`/`y`/`z`/`nx`/`ny`/`nz` columns as a `System.Array` typed per `DataType` and the `Face` element's `vertex_indices` `int[][]` list column fan-triangulated, retiring the hand-rolled BCL `PlyReader` endian/ascii fork), and FBX/Collada/3MF are the `scene-exchange` codec composing `AssimpNetter` (the `Scene` arm: one disposable `AssimpContext.ImportFileFromStream(stream, PostProcessSteps.Triangulate | JoinIdenticalVertices | GenerateSmoothNormals, format.Key)` folding the `Scene.Meshes` `Vertices`/`Normals`/`Faces` graph onto the triangle-soup, retiring the hand-rolled BCL `ThreeMfReader` OPC/ZIP parse); the OpenUSD `usd-stage` codec composes `UniversalSceneDescription` (the `Usd` arm: `UsdStage.Open` over the temp-path layer stack, `Traverse` filtering each `UsdGeomMesh` prim by `GetTypeName()`, reading the points `VtVec3fArray`/`GfVec3f` and the `GetFaceVertexCountsAttr`/`GetFaceVertexIndicesAttr` `VtIntArray` topology through the typed-array mesh-bridge seam, fan-triangulating each face — USD a scene-graph peer, the BIM semantics staying the GeometryGym IFC graph's, never re-derived from USD prim type names); each arm materializes one contiguous `ImportedGeometry` boundary allocation and the leaked package types (`Ply.Net.*`, `Assimp.*`, `pxr.*`) never cross past `Exchange/import` — internal code holds the canonical carriers per the boundary-mapping law, and the `SWIGTYPE_p_*`/`*PINVOKE` USD interop types never enter the fold; the rejected reader picks stand (`lib3mf` native C++, `Aspose.3D` closed/commercial — `AssimpNetter` ships its own osx-arm64 `libassimp.dylib` admitted as the one scene-exchange owner), and `geometry3Sharp.OBJReader`/`STLReader` stay consumed through the `StandardMeshReader` dispatch, never a second hand-rolled tokenizer; the IFC arm decodes ONLY the live `DatabaseIfc` — the entity walk and the seam-node/edge projection are the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector`'s (it captures the `DatabaseIfc` internally so GeometryGym never crosses the seam `IElementProjection.Project` signature), and a lossy `IfcSemanticModel` flat-row re-projection here that drops the eight stranded `IfcRel*` families, `OwnerHistory`, and `StepHeader` is the deleted form; GeometryGym carries no tessellation kernel, so a geometry request on an IFC row routes to the `tessellation#TESSELLATION_BRIDGE` rail and never evaluates a BRep in-process; the `step-iso10303` STEP route splits two legs: the managed semantic-graph leg is now in-process through the BCL-only `StepReader`, while the B-rep/NURBS geometry leg stays companion-routed because no managed STEP solid evaluator is admitted — `StepReader` strips the Part-21 comment and string spans, splits the HEADER and DATA sections, slices each `#N = ENTITY(...);` statement at depth-zero semicolons, parses each statement into an `Instance(Id, Keyword, Args)` where the recursive-descent `ParseArg` discriminates the Part-21 token grammar into a closed `Arg` `[Union]` — reference `#N` (`Arg.Ref`), string `'...'` with the `''` escape (`Arg.Text`), typed-enum `.X.` (`Arg.Enum`), number (`Arg.Number`), nested list `(...)` (`Arg.List`), typed-constructor `KEYWORD(...)` (`Arg.Typed`), and `$`/`*`/identifier (`Arg.Untyped`) — builds the forward-reference instance graph as a `Dictionary<long, Instance>` resolved in a second pass through `Resolve`, and projects the product structure (`PRODUCT` → `ProductRow`, `PRODUCT_DEFINITION` → `DefinitionRow` walking the `PRODUCT_DEFINITION_FORMATION` reference to its `PRODUCT`, `NEXT_ASSEMBLY_USAGE_OCCURRENCE` → `AssemblyEdge` resolving both relating and related definitions to their product ids) and the AP242 PMI/semantic metadata (`DIMENSIONAL_*`/`DATUM`/`GEOMETRIC_TOLERANCE`/`ANNOTATION_OCCURRENCE` filtered through a frozen `PmiTypes` set into `PmiRow`); the geometry entities (`ADVANCED_BREP_SHAPE_REPRESENTATION`/`MANIFOLD_SOLID_BREP`/`B_SPLINE_SURFACE`/`SHAPE_REPRESENTATION` filtered through a frozen `GeometryTypes` set) are NOT evaluated in-process — `StepReader` carries only their `GeometryRef(Id, EntityType, ShapeDefinitionId)` and routes the actual B-rep/NURBS solid evaluation to the `tessellation#TESSELLATION_BRIDGE` companion rail (OpenCascade serving the STEP solid read), so `TessellationRequiresCompanion` stays `true` on the STEP rows; the rejected managed readers stand — `IxMilia.Step` and `StepFileParser` do not exist on NuGet, `STPLoader` is net35/2015/`AForge.Math`-coupled (RID-unsafe, abandoned), and `DevelApp.StepParser` is a GrammarForge regex/PCRE2 grammar engine that does not model the Part-21 entity-instance graph and pulls a prerelease `ICU4N` alpha transitive — so `StepReader` is the in-process BCL-only entity-instance graph the codec needed and a managed STEP B-rep evaluator beside the companion is the deleted form; GeometryGym is IFC-schema-bound and does not parse generic ISO-10303 AP242 product structure, so it grounds no STEP semantic leg; `DatabaseIfc.Tolerance`/`ToleranceAngleRadians`/`ScaleSI` read the model precision the content-key folds; the `Decompress` pre-decode branch is the decompress-on-import arm symmetric to the `export#EXPORT_RAIL` `GlbBytes` compression switch — `SharpGLTF.Core` ships no compression decoder (the catalogued assembly carries no Draco/meshopt type, decompile-verified absence), so a GLB whose primitive carries a `KHR_draco_mesh_compression` extension or whose bufferView carries an `EXT_meshopt_compression` extension reaches the `LogicalMeshes.Decode()` fold with its accessor data still compressed and unreadable, and `Detect` cannot distinguish a compressed GLB from a plain one because the compression rides a per-primitive/per-bufferView extension, not the row; `SharpGLTF.Core` retains no typed handle to an unrecognized extension (`KHR_draco_mesh_compression`/`EXT_meshopt_compression` have no in-box `JsonSerializable` extension class, so the `ExtraProperties.Extensions` collection never holds them and `ExtraProperties.Extras` is a free-form `JsonNode`, not an extension accessor), so the branch reparses the GLB/glTF JSON chunk the `ReadContext.ReadJson`/`IdentifyBinaryContainer` pair already extracts into a `System.Text.Json.Nodes.JsonNode` tree and reads each `meshes[].primitives[].extensions.KHR_draco_mesh_compression` and `bufferViews[].extensions.EXT_meshopt_compression` object directly out of that tree for its `bufferView`/`count`/`byteStride`/`mode`/`filter` parameters; it routes the `KHR_draco_mesh_compression` primitive payload through `Draco.Decode(byte[])` (downcasting the returned `DracoPointCloud` to `DracoMesh`, reading each `PointAttribute` through `DracoPointCloud.GetNamedAttribute(AttributeType.Position)`/`Normal` and `PointAttribute.GetValueAsVector3(PointAttribute.MappedIndex(point))` per point — the point index mapped to its deduplicated attribute-value index, since Draco shares attribute values across points and `GetValueAsVector3` indexes by value, not point — and the faces through `DracoMesh.NumFaces`/`ReadFace(id, int[3])` yielding point indices aligned with that per-point vertex order, then `Fill`-ing the `MeshPrimitive.GetVertexAccessor("POSITION")`/`"NORMAL"` `Accessor.AsVector3Array()` and the `GetIndexAccessor().AsIndicesArray()` cast to their concrete `Vector3Array`/`IntegerArray` write surface — `Fill(IEnumerable<T>)` is a member of the concrete accessor-array structs, not the `IAccessorArray<T>` interface the factory statically returns, so the decode downcasts to the runtime `Vector3Array`/`IntegerArray` before filling) and the `EXT_meshopt_compression` bufferView payload through `Meshopt.DecodeVertexBuffer`/`DecodeIndexBuffer` then the filter inverse `Meshopt.DecodeFilterOct` (octahedral-encoded normals), `DecodeFilterQuat` (quantized rotations), `DecodeFilterExp` (shared-exponent floats), and `DecodeFilterColor` (quantized vertex color) keyed on the bufferView extension's `mode`/`filter` discriminant, then writes the decompressed buffer back through `ModelRoot.UseBufferView(ArraySegment<byte>, byteStride, BufferMode?)` (the byte buffer wrapped in an `ArraySegment<byte>` so the call binds the strided `(data, byteStride, target)` overload rather than the `(byte[], byteOffset, byteLength, …)` overload, the target read from `BufferView.IsIndexBuffer`) before the `IMeshDecoder` fold so a web-compressed artifact round-trips back through import without a companion — the decode reuses the same dormant `Openize.Drako`/`Alimer.Bindings.MeshOptimizer` surface the export encode switch drives, closing the export-can-compress / import-cannot-decompress asymmetry with zero new package and zero new `InterchangeFormat` row; the SharpGLTF `ReadSettings.Validation` rides `ValidationMode.Strict` on an uncompressed asset so a malformed glTF faults at parse, and a compressed asset parses under `ValidationMode.Skip` past the unrecognized-extension and missing-bufferView-data validation errors the compression extension provokes, then re-validates the reconstructed geometry at the materialization boundary; the `fbx`/`dae` rows are now live `scene-exchange` (`AssimpNetter`) and the USD rows live `usd-stage` (`UniversalSceneDescription`), so the former `import-catalogue-pending` fault no longer fires for them; every codec admit reaching this fold is one `InterchangeCodec`-keyed arm on the existing `ImportGeometry`/`ImportIfc`/`ImportStep` dispatch — the row-promotion discipline `format#FORMAT_AXIS` owns, never a new `BimIo` entrypoint or a parallel importer family — and the companion-versus-managed geometry route is read from the row's `TessellationRequiresCompanion` column rather than a call-site `if (ifc)`/`if (step)` branch; an `IfcImporter`/`GltfImporter`/`PlyImporter`/`SceneImporter`/`UsdImporter`/`StepImporter` service family, a per-extension `DracoImporter`/`MeshoptImporter`, a per-protocol AP203/AP214/AP242 STEP reader, and a managed IFC or STEP B-rep tessellator are the deleted forms.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Xml;
using Assimp;
using GeometryGym.Ifc;
using g3;
using LanguageExt;
using NodaTime;
using Ply.Net;
using pxr;
using Rasm;
using Rasm.Domain;
using Rasm.Element;
using SharpGLTF.Schema2;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record ImportedGeometry(
    InterchangeFormat Format,
    ReadOnlyMemory<float> Vertices,
    ReadOnlyMemory<float> Normals,
    ReadOnlyMemory<long> Indices,
    int VertexCount,
    int TriangleCount,
    Instant At);

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
    // Capability gates first — CataloguePending BEFORE CanImport (pending ⟹ CanImport=false, so the unsupported gate
    // otherwise shadows the richer package-naming message) — then the TOTAL generated InterchangeCodec Switch
    // dispatches every codec: the six managed-mesh codecs decode inline, the IFC/STEP codecs name their own
    // entrypoint, the geospatial/point-cloud codecs name their owning page, the companion codecs route to the bridge.
    // The Switch has NO silent fallthrough, so a new InterchangeCodec row breaks this call site at compile time — a new
    // managed-mesh import lands as one arm and a non-mesh codec is forced to declare its route, never misrouting to a
    // stale "needs-companion" fault the prior == ladder produced for GeometryGym/StepIso10303/geospatial.
    public static Fin<ImportedGeometry> ImportGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks, Op key) =>
        format.CataloguePending ? Fin.Fail<ImportedGeometry>(new BimFault.CodecReject(key, $"import-catalogue-pending:{format.Key}:{format.Codec.CataloguePackage.IfNone("unknown")}"))
        : !format.CanImport ? Fin.Fail<ImportedGeometry>(new BimFault.CodecReject(key, $"import-unsupported:{format.Key}"))
        : format.Codec.Switch(
            sharpGltf:        () => Boundary(key, () => Framed(format, Gltf(format, bytes, clocks.Now))),
            meshText:         () => MeshTextGeometry(format, bytes, clocks.Now, key),
            ply:              () => Boundary(key, () => Framed(format, Ply(format, bytes, clocks.Now))),
            sceneExchange:    () => Boundary(key, () => Framed(format, Scene(format, bytes, clocks.Now))),
            usdStage:         () => Boundary(key, () => Framed(format, Usd(format, bytes, clocks.Now))),
            acadSharp:        () => Boundary(key, () => Framed(format, AcadReader.Read(format, bytes, clocks.Now))),
            geometryGym:      () => Fin.Fail<ImportedGeometry>(new BimFault.CodecReject(key, $"import-ifc-route:use-ImportIfc:{format.Key}")),
            stepIso10303:     () => Fin.Fail<ImportedGeometry>(new BimFault.CodecReject(key, $"import-step-route:use-ImportStep:{format.Key}")),
            geospatialVector: () => Fin.Fail<ImportedGeometry>(new BimFault.CodecReject(key, $"import-geospatial-route:{format.Key}")),
            geospatialRaster: () => Fin.Fail<ImportedGeometry>(new BimFault.CodecReject(key, $"import-geospatial-route:{format.Key}")),
            pointCloud:       () => Fin.Fail<ImportedGeometry>(new BimFault.CodecReject(key, $"import-point-cloud-route:{format.Key}")),
            nativeCompanion:  () => Fin.Fail<ImportedGeometry>(new BimFault.CapabilityMiss(key, $"import-needs-companion:{format.Key}")),
            igesAnsi:         () => Fin.Fail<ImportedGeometry>(new BimFault.CapabilityMiss(key, $"import-needs-companion:{format.Key}")),
            ifc5Pending:      () => Fin.Fail<ImportedGeometry>(new BimFault.CodecReject(key, $"import-catalogue-pending:{format.Key}")));

    // OBJ/STL/OFF only — PLY now routes to the dedicated `ply-net` codec (the `Ply` arm) and 3MF/FBX/Collada
    // to the `scene-exchange` codec (the `Scene` arm), so the mesh-text sub-dispatch is one geometry3Sharp leg.
    static Fin<ImportedGeometry> MeshTextGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at, Op key) {
        string extension = format.Extensions.Head.Map(static ext => ext.TrimStart('.')).IfNone("");
        return new StandardMeshReader().SupportsFormat(extension)
            ? Boundary(key, () => Framed(format, MeshText(format, extension, bytes, at)))
            : Fin.Fail<ImportedGeometry>(new BimFault.CodecReject(key, $"import-catalogue-pending:{format.Key}:mesh-text"));
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

    // The IFC arm decodes foreign bytes to the LIVE GeometryGym DatabaseIfc — the in-process IFC graph the
    // Projection/semantic#SEMANTIC_PROJECTOR SemanticProjector captures and lowers to a seam GraphDelta. The import
    // rail OWNS the byte->graph decode; the entity walk, the full IfcRel* roster, the typed property/quantity
    // projection, OwnerHistory, and StepHeader are the projector's (read off this live graph), NEVER a lossy
    // flat-row re-projection here — the retired IfcSemanticModel rows that dropped the eight stranded relationship
    // families are the deleted form. GeometryGym is captured by the projector internally, so DatabaseIfc never
    // crosses the seam IElementProjection.Project signature.
    public static Fin<DatabaseIfc> ImportIfc(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Op key) =>
        format.Codec == InterchangeCodec.GeometryGym
            ? Boundary(key, () => Database(format, bytes))
            : Fin.Fail<DatabaseIfc>(new BimFault.CodecReject(key, $"ifc-codec-miss:{format.Key}"));

    public static Fin<StepSemanticModel> ImportStep(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks, Op key) =>
        format.Codec == InterchangeCodec.StepIso10303
            ? Boundary(key, () => StepReader.Read(format, bytes.Span, clocks.Now))
            : Fin.Fail<StepSemanticModel>(new BimFault.CodecReject(key, $"step-codec-miss:{format.Key}"));

    // The captured-fault funnel: Try.lift runs the foreign decode, and MapFail closes over the Op key to lift the
    // raw error.Message into BimFault.ModelRejected BARE (the Expected-derived case IS the Error — no .ToError()
    // hop). The lambda is NOT static because it captures key; Try.lift preserves the raw message a kernel Op.Catch
    // would re-wrap in Fault.InvalidResult boilerplate, so the SharpGLTF ModelException, the GeometryGym parse
    // fault, and the malformed-Part-21 InvalidDataException never cross a domain signature.
    static Fin<T> Boundary<T>(Op key, Func<T> decode) =>
        Try.lift(decode).Run().MapFail(error => new BimFault.ModelRejected(key, error.Message));

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
        Span<int> corners = stackalloc int[3];
        foreach (var (prim, (a, b, c)) in triangles) {
            (corners[0], corners[1], corners[2]) = (a, b, c);
            foreach (int corner in corners) {
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

    // The IFC byte->graph decode the import rail OWNS: STEP/XML/JSON by the row's format into the live DatabaseIfc.
    // The entity/relationship/property projection onto seam nodes+edges is Projection/semantic#SEMANTIC_PROJECTOR's
    // (it captures this DatabaseIfc internally), so the import rail mints no IfcSemanticModel, no AssemblyRel, and no
    // MapConversionRow — those flat rows dropped the eight stranded IfcRel* families, OwnerHistory, and StepHeader the
    // projector preserves off the live graph, and a re-projection here would be the deleted lossy form.
    static DatabaseIfc Database(InterchangeFormat format, ReadOnlyMemory<byte> bytes) {
        // The schema is sniffed off the bytes BEFORE the database is constructed [H8] — the STEP FILE_SCHEMA header
        // line auto-resolves through ParseString, and ifcJSON/ifcXML construct at the SemanticProjector.Sniff schema
        // (the schema_identifier member / FILE_SCHEMA token) rather than a hardcoded IFC4X3_ADD2, so a 2x3 file admits
        // as 2x3. SemanticProjector.Sniff is the ONE schema-sniff owner this decode and the wire#WIRE_PROJECTION
        // Admit decode both compose, never a second hardcoded construction.
        ReleaseVersion schema = SemanticProjector.Sniff(bytes, format);
        return format == InterchangeFormat.IfcJson ? JsonDatabase(bytes, schema)
            : format == InterchangeFormat.IfcXml ? XmlDatabase(bytes, schema)
            : DatabaseIfc.ParseString(Encoding.UTF8.GetString(bytes.Span));
    }

    static DatabaseIfc JsonDatabase(ReadOnlyMemory<byte> bytes, ReleaseVersion schema) {
        var db = new DatabaseIfc(false, schema);
        db.ReadJSON((JsonObject)JsonNode.Parse(bytes.Span)!);
        return db;
    }

    static DatabaseIfc XmlDatabase(ReadOnlyMemory<byte> bytes, ReleaseVersion schema) {
        var doc = new XmlDocument();
        doc.LoadXml(Encoding.UTF8.GetString(bytes.Span));
        var db = new DatabaseIfc(false, schema);
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
        // Dataset.Data is a lazy/streamed sequence over the parse stream (api-ply-net), so it materializes ONCE before
        // the vertex+face lookups — a second enumeration re-reads the already-advanced stream and strands the columns.
        var elements = dataset.Data.ToList();
        var vertex = elements.First(static d => d.Element.Type == ElementType.Vertex);
        var face = elements.FirstOrDefault(static d => d.Element.Type == ElementType.Face);
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
        var soup = scene.Meshes.Fold(MeshSoup.Empty, static (acc, mesh) => acc.Append(
            mesh.Vertices.SelectMany(static p => new[] { p.X, p.Y, p.Z }).ToSeq(),
            Enumerable.Range(0, mesh.VertexCount)
                .SelectMany(i => mesh.HasNormals ? new[] { mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z } : new[] { 0f, 0f, 1f }).ToSeq(),
            mesh.Faces.AsIterable()
                .SelectMany(face => Enumerable.Range(1, face.IndexCount - 2)
                    .SelectMany(k => new long[] { face.Indices[0], face.Indices[k], face.Indices[k + 1] })).ToSeq(),
            mesh.VertexCount));
        return new ImportedGeometry(format, soup.Vertices.ToArray(), soup.Normals.ToArray(), soup.Indices.ToArray(),
            soup.VertexCount, soup.TriangleCount, at);
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
                .Fold(MeshSoup.Empty, static (acc, prim) => {
                    var (points, indices) = UsdMesh(new UsdGeomMesh(prim));
                    return acc.Append(points,
                        Enumerable.Range(0, points.Count / 3).SelectMany(static _ => new[] { 0f, 0f, 1f }).ToSeq(),
                        indices, points.Count / 3);
                });
            return new ImportedGeometry(format, soup.Vertices.ToArray(), soup.Normals.ToArray(), soup.Indices.ToArray(),
                soup.VertexCount, soup.TriangleCount, at);
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

    // The shared triangle-soup the AssimpNetter, USD, ACadSharp, and Speckle decode arms fold onto the
    // contiguous ImportedGeometry triple — ONE accumulator carrier, never a per-source soup struct. Each arm
    // projects its foreign mesh into a (vertices, normals, 0-based corner indices, vertexCount) block; Append
    // offsets the corners by the running VertexCount so the block joins the soup (the welded Assimp/USD/ACad
    // blocks keep their source vertex sharing; the Speckle arm pre-expands its n-gon fans into an unwelded block).
    readonly record struct MeshSoup(Seq<float> Vertices, Seq<float> Normals, Seq<long> Indices, int VertexCount) {
        public static readonly MeshSoup Empty = new(Seq<float>(), Seq<float>(), Seq<long>(), 0);

        public int TriangleCount => Indices.Count / 3;

        public MeshSoup Append(Seq<float> vertices, Seq<float> normals, Seq<long> corners, int addedVertices) =>
            new(Vertices + vertices, Normals + normals, Indices + corners.Map(corner => VertexCount + corner), VertexCount + addedVertices);

        public MeshSoup Append((Seq<float> Vertices, Seq<float> Normals, Seq<long> Corners, int Added) block) =>
            Append(block.Vertices, block.Normals, block.Corners, block.Added);
    }

    // Managed in-process DWG/DXF decode through ACadSharp — the `acad-sharp` codec the format#FORMAT_AXIS
    // Dwg row carries. The DXF/CadDocument is the same decompile-verified reader Fabrication consumes for
    // 2D profiles; here the Bim arm folds the mesh-bearing entities onto the canonical triangle-soup.
    static class AcadReader {
        public static ImportedGeometry Read(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at) {
            using var stream = new MemoryStream(bytes.ToArray());
            var document = IsDxf(bytes) ? DxfReader.Read(stream) : DwgReader.Read(stream);
            var soup = document.Entities.AsIterable().Fold(MeshSoup.Empty, Accumulate);
            return new ImportedGeometry(format, soup.Vertices.ToArray(), soup.Normals.ToArray(), soup.Indices.ToArray(),
                soup.VertexCount, soup.TriangleCount, at);

            // An Insert flattens through the package-owned Explode() — the OCS->WCS placement, Rotation, per-axis
            // scale, OCS Normal, AND the MINSERT array replication ACadSharp owns — each placed entity folded back
            // through the same classifier so a block-nested Insert recurses. The deleted form hand-rolled an
            // InsertPoint/XScale matrix the api-acadsharp RAIL_LAW rejects: it dropped Rotation, the OCS Normal, every
            // MINSERT instance, and every block-nested Mesh/PolyfaceMesh (it walked the block's Face3D entities only).
            static MeshSoup Accumulate(MeshSoup soup, Entity entity) => entity switch {
                Mesh mesh         => soup.Append(Faces(mesh.Vertices, mesh.Faces)),
                Face3D face       => soup.Append(Quad(face.FirstCorner, face.SecondCorner, face.ThirdCorner, face.FourthCorner)),
                PolyfaceMesh poly => soup.Append(Polyface(poly)),
                Insert insert     => insert.Explode().AsIterable().Fold(soup, Accumulate),
                _                 => soup,
            };
        }

        // DXF (ascii/binary) opens with "0\nSECTION" / "AutoCAD Binary DXF"; DWG with "AC10xx" — the one sniff the
        // package leaves to the caller (CadReaderFactory.GetFileFormat is filename-only and the shared Dwg row carries
        // both extensions over a byte stream), so the reader pick is a boundary kernel, never a hand-rolled DXF parse.
        static bool IsDxf(ReadOnlyMemory<byte> bytes) =>
            bytes.Length >= 4 && !(bytes.Span[0] == (byte)'A' && bytes.Span[1] == (byte)'C' && char.IsDigit((char)bytes.Span[2]));

        // A POLYLINE/AcDbPolyFaceMesh: the VertexFaceMesh vertex pool plus the 1-based signed VertexFaceRecord index
        // records (a negative index marks a hidden edge -> abs, a zero Index4 marks a triangle), fan-triangulated to a
        // 0-based block the shared MeshSoup.Append offsets by the running VertexCount.
        static (Seq<float> Vertices, Seq<float> Normals, Seq<long> Corners, int Added) Polyface(PolyfaceMesh poly) {
            var pool = poly.Vertices.Select(static v => v.Location).ToList();
            var verts = pool.SelectMany(static p => new[] { (float)p.X, (float)p.Y, (float)p.Z }).ToSeq();
            var normals = pool.SelectMany(static _ => new[] { 0f, 0f, 1f }).ToSeq();
            var corners = poly.Faces.SelectMany(static f => {
                long a = Math.Abs(f.Index1) - 1, b = Math.Abs(f.Index2) - 1, c = Math.Abs(f.Index3) - 1;
                return f.Index4 == 0 ? new[] { a, b, c } : new[] { a, b, c, a, c, (long)Math.Abs(f.Index4) - 1 };
            }).ToSeq();
            return (verts, normals, corners, pool.Count);
        }

        // A SubDMesh: the vertex list plus the n-gon face index list (each face fan-triangulated), as a 0-based
        // triangle-soup block the shared MeshSoup.Append offsets by the running VertexCount.
        static (Seq<float> Vertices, Seq<float> Normals, Seq<long> Corners, int Added) Faces(
            System.Collections.Generic.IReadOnlyList<XYZ> vertices, System.Collections.Generic.IReadOnlyList<int[]> faces) {
            var verts = vertices.SelectMany(static p => new[] { (float)p.X, (float)p.Y, (float)p.Z }).ToSeq();
            var corners = faces.SelectMany(face => Enumerable.Range(1, face.Length - 2)
                .SelectMany(k => new long[] { face[0], face[k], face[k + 1] })).ToSeq();
            var normals = Enumerable.Range(0, vertices.Count).SelectMany(static _ => new[] { 0f, 0f, 1f }).ToSeq();
            return (verts, normals, corners, vertices.Count);
        }

        // A 3DFACE quad (the fourth corner equals the third for a triangle) fan-triangulated to a 0-based block.
        static (Seq<float> Vertices, Seq<float> Normals, Seq<long> Corners, int Added) Quad(XYZ a, XYZ b, XYZ c, XYZ d) {
            bool tri = d.Equals(c);
            var corners = tri ? new[] { a, b, c } : new[] { a, b, c, d };
            var verts = corners.SelectMany(static p => new[] { (float)p.X, (float)p.Y, (float)p.Z }).ToSeq();
            var indices = (tri ? new[] { 0, 1, 2 } : new[] { 0, 1, 2, 0, 2, 3 }).Select(static i => (long)i).ToSeq();
            var normals = corners.SelectMany(static _ => new[] { 0f, 0f, 1f }).ToSeq();
            return (verts, normals, indices, corners.Length);
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

- Owner: `BimIo.ImportSpeckle` the Speckle display-mesh arm of the import fold (a deserialized `Speckle.Sdk.Models.Base` tree → `ImportedGeometry`), and `SpeckleProjector : IElementProjection` the Speckle host-object arm of the SEAM (the same `Base` tree → a seam `GraphDelta`, the peer of the IFC `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector`); there is no `SpeckleImporter`/`SpeckleConverter` type and no parallel decode family — the geometry arm is a third entrypoint on the existing `BimIo` capsule symmetric to `ImportGeometry`/`ImportIfc`, the projector an `IElementProjection` the app registers in its `Seq<IElementProjection>`, both consuming the receive-side `Base` the Persistence `csharp:Persistence/Version/ledger#SYNC_TRANSPORTS` `IOperations.Receive` returns.
- Entry: `BimIo.ImportSpeckle(Base root, ClockPolicy clocks, Op key)` projecting the display-mesh geometry to `ImportedGeometry`, and `new SpeckleProjector(root).Project(ProjectionContext ctx)` lowering the host-object graph to a seam `GraphDelta` — the geometry `Fin<T>` aborts on a graph with no displayable geometry or a malformed display mesh, projecting the Speckle exception onto `BimFault.ModelRejected(key, error.Message)` BARE at the boundary (band 2600 IS the `Expected` `Code` — no `.ToError()` hop) so domain code never sees a `Speckle.Sdk.SpeckleException`, while the projector's foreign fault funnels to `ElementFault.ProjectionFailed` at the caller's capture boundary — the `ProjectionAssembly.Assemble` `Try.lift` funnel (the seam idiom; a kernel `Op.Catch` would erase the typed arm into `Fault.InvalidResult`) or the Bim-internal `BimIo.Reimport` `key.Catch`; the `Base` arrives already deserialized, so the seam mints no transport, no `IOperations` reference, and no second graph walk beyond the package-owned traversal.
- Auto: the geometry fold runs the package-owned `BaseExtensions.Flatten(Base, BaseExtensions.BaseRecursionBreaker?)` deduplicating graph walk, projects each node's `BaseExtensions.TryGetDisplayValue(Base)` display list to its `Mesh` members, and decodes each `Mesh` — the flat `vertices`/`vertexNormals` (`List<double>`, flat `x,y,z`) and length-prefixed `faces` (`List<int>`, each face `[n, i0, … i(n-1)]`) triangulate through a fan over the n-gon, scaled onto the canonical metre frame by `Units.GetConversionFactor(mesh.units, Units.Meters)` so a millimetre or foot Speckle model lands in kernel units; a node that `IsDisplayableObject` is false yet carries non-mesh geometry (`Brep`/`Surface`/`Curve` with no `displayValue`) routes its content to `tessellation#TESSELLATION_BRIDGE` over the GLB rail rather than evaluating a BRep in-process; the `SpeckleProjector` fold lowers every `DataObject` (and its `RhinoObject`/`RevitObject`/`ArchicadObject`/`TeklaObject`/`Civil3dObject`/`AutocadObject` host-object subtypes) onto a rooted seam `Node.Object` carrying the generic `Classification("speckle", speckle_type)` and the host `applicationId` as the 1:1 `ExternalId`, its `DataObject.properties` (`Dictionary<string, object?>`) into one content-keyed `PropertySet` bag node attached by an `Assign.PropertyDefinition` edge, and the `BaseExtensions.TraverseWithPath` path prefixes reconstructing the namespace containment as `Compose.Contain` edges — the containment the retired flat-row projection claimed in prose but produced empty.
- Receipt: the `ModelLoad` receipt case carries the format key `InterchangeFormat.Glb.Key` proxy for the decoded scene, the codec key `speckle-base`, the `Base.GetTotalChildrenCount()` source object count, and elapsed; the `SpeckleProjector` contributes the host-object `GraphDelta` (its `NodeCount`/`EdgeCount` the change magnitude, the distinct `speckle_type` discriminants the seam `Classification` codes); emission rides the sink port at the composition edge.
- Packages: Speckle.Sdk, Speckle.Objects, SharpGLTF.Core, Rasm.Element, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, Rasm
- Growth: a new Speckle geometry leaf is one arm on the `DisplayMeshes` projection keyed on the `IDisplayValue<T>` payload type; a new host-object discriminant is one `Classification("speckle", speckle_type)` code on the `SpeckleProjector`'s `Node.Object`, never a parallel row family; a non-mesh evaluation never grows a managed Speckle tessellator — it widens the `tessellation#TESSELLATION_BRIDGE` request, never this fold.
- Boundary: `BimIo` is the page boundary capsule and the Speckle arm carries the language-owned statement forms the foreign graph walk requires; the `Base` graph is admitted exactly once — `Flatten` is the single package-owned deduplicating traversal (it caches on `Base.id`), so the seam never re-walks the tree or hand-rolls a `DynamicBase.GetMembers` recursion, and `TryGetDisplayValue`/`IsDisplayableObject` own the displayable-node vocabulary rather than a per-type `is Mesh`/`is Brep` ladder; the Speckle `Mesh.faces` length-prefixed n-gon encoding fans into the canonical triangle-soup `ImportedGeometry` at the boundary (one contiguous vertex/normal/index triple, the allocation point, never a per-face `double[]` proliferation), and a degenerate face (`n < 3`) faults the decode; non-mesh geometry never evaluates in-process — GeometryGym carries no Speckle BRep kernel and the managed branch owns no NURBS evaluator, so a `Brep`/`Surface`/`Curve` with no `displayValue` rides the companion GLB rail exactly as the IFC geometry request does, joining the same content-keyed artifact; `Speckle.Sdk`/`Speckle.Objects` are the OUTSIDE-RHINO concern (`Speckle.Sdk.Dependencies` repacks the SDK's Polly/channel/serialisation-V2 closure), so this arm composes them only in the host-neutral `Rasm.Bim` exchange assembly and the in-Rhino plugin assembly never loads them; the host-object semantic projection is the `SpeckleProjector : IElementProjection` lowering to a seam `GraphDelta` (the generic `Classification("speckle", speckle_type)`, never an IFC class), so a `SpeckleImporter`/`SpeckleConverter` service family, a hand-rolled `Base`-graph recursion, a lossy `IfcSemanticModel` host-object re-projection, and a managed Speckle tessellator are the deleted forms.

```csharp signature
public static partial class BimIo {
    public static Fin<ImportedGeometry> ImportSpeckle(Base root, ClockPolicy clocks, Op key) =>
        Boundary(key, () => DisplayScene(root, clocks.Now))
            .Bind(scene => scene.TriangleCount > 0
                ? Fin.Succ(scene)
                : Fin.Fail<ImportedGeometry>(new BimFault.ModelRejected(key, $"speckle-no-display:{root.speckle_type}")));

    static ImportedGeometry DisplayScene(Base root, Instant at) {
        var soup = root.Flatten()
            .SelectMany(static node => node.TryGetDisplayValue()?.OfType<Mesh>() ?? Enumerable.Empty<Mesh>())
            .ToSeq()
            .Fold(MeshSoup.Empty, static (soup, mesh) => soup.Append(SpeckleBlock(mesh)));
        return new ImportedGeometry(
            InterchangeFormat.Glb, soup.Vertices.ToArray(), soup.Normals.ToArray(), soup.Indices.ToArray(),
            soup.VertexCount, soup.TriangleCount, at);
    }

    // The Speckle Mesh -> UNWELDED triangle-soup block the shared MeshSoup folds: each length-prefixed n-gon fans
    // to triangles, each fan corner expands to its own vertex (Speckle faces index the shared vertex list, the seam
    // unwelds), the vertexNormals sampled when present else an up-normal, scaled onto the canonical metre frame by
    // the source unit; the 0-based corners MeshSoup.Append offsets by the running VertexCount.
    static (Seq<float> Vertices, Seq<float> Normals, Seq<long> Corners, int Added) SpeckleBlock(Mesh mesh) {
        double scale = Units.GetConversionFactor(mesh.units, Units.Meters);
        var fans = Triangulate(mesh.faces).ToSeq();
        var vertices = fans.Bind(corner => Sample(mesh.vertices, corner, scale));
        var normals = fans.Bind(corner => mesh.vertexNormals.Count == mesh.vertices.Count
            ? Sample(mesh.vertexNormals, corner, 1.0)
            : Seq<float>(0f, 0f, 1f));
        return (vertices, normals, fans.Map(static (_, ordinal) => (long)ordinal), fans.Count);
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

// The Speckle arm of the seam — the IElementProjection peer of the IFC Projection/semantic#SEMANTIC_PROJECTOR
// SemanticProjector. It captures one already-deserialized Speckle Base graph internally and lowers the host-object
// tree onto a seam GraphDelta: each DataObject becomes a rooted Object.Occurrence node carrying the generic
// Classification("speckle", speckle_type) and the host applicationId as the 1:1 ExternalId, its parameter dictionary
// becomes one content-keyed PropertySet bag node attached by an Assign.PropertyDefinition edge, and the namespace
// nesting becomes Compose.Contain edges reconstructed from the TraverseWithPath path prefixes — the containment the
// retired flat-row SpeckleSemantic CLAIMED in prose but produced empty. Speckle is a PRIMARY source of element
// identity, so each object mints the kernel static NodeId.Rooted() (ProjectionContext exposes only For/Owns, never a
// mint pass-through); the display geometry rides the separate ImportSpeckle ImportedGeometry path, so the semantic node
// references no IFC representation. A SpeckleConverter service family, a hand-rolled Base
// recursion, and an IfcSemanticModel re-projection are the deleted forms; a thrown Speckle fault is funnelled to
// ElementFault.ProjectionFailed at the caller's capture boundary (ProjectionAssembly.Assemble's Try.lift funnel, or BimIo.Reimport's key.Catch), never here.
public sealed class SpeckleProjector(Base root) : IElementProjection {
    static readonly BaseRecursionBreaker Descend = static _ => false;

    public Fin<GraphDelta> Project(ProjectionContext ctx) => Fin.Succ(Lower(ctx));

    GraphDelta Lower(ProjectionContext ctx) {
        // One path-carrying deduplicating walk (the package-owned TraverseWithPath, never a DynamicBase recursion):
        // every DataObject gets a neutral rooted id, the path retained so containment is the nearest-ancestor DataObject.
        var hosts = root.TraverseWithPath(Descend)
            .Where(static step => step.Item2 is DataObject)
            .Select(static step => (Path: step.Item1, Data: (DataObject)step.Item2, Id: NodeId.Rooted()))
            .ToSeq();
        var span = SchemaSpan.From(ctx.Header.Schema);
        double tolerance = ctx.Header.Tolerance;
        var withNodes = hosts.Fold(GraphDelta.Empty.Reheader(ctx.Header), (delta, host) => {
            var bag = BagNode(host.Data, tolerance);
            return delta
                .Put(ObjectNode(host.Data, host.Id, span))
                .Put(bag)
                .Link(new Relationship.Assign(host.Id, bag.Id, AssignKind.PropertyDefinition));
        });
        return Containment(hosts).Fold(withNodes, static (delta, edge) => delta.Link(edge));
    }

    // The DataObject -> seam Object.Occurrence node: neutral Classification("speckle", speckle_type), the host
    // applicationId the 1:1 ExternalId (the re-ingest reconcile key), no IFC representation (the display mesh rides the
    // ImportSpeckle path), NotDefined predefined (a Speckle host object carries no IFC predefined token).
    static Node ObjectNode(DataObject data, NodeId id, SchemaSpan span) => new Node.Object(
        Id:              id,
        Kind:            ObjectKind.Occurrence,
        ExternalId:      Optional(data.applicationId),
        Classification:  Classification.Create("speckle", data.speckle_type, "", "", None, None),
        PredefinedType:  PredefinedType.NotDefined,
        Name:            data.name ?? "",
        Tag:             "",
        Representations: RepresentationContentHash.Empty,
        History:         Option<OwnerHistory>.None,
        Span:            span);

    // The host parameter dictionary -> one CONTENT-KEYED PropertySet bag node, so two identical parameter sets dedup
    // to one node; the id mint excludes the bag's own id (ToCanonicalBytes drops it) by minting off a temp seed.
    // OccurrenceWins because a Speckle host object carries no type-driven inheritance.
    static Node.PropertySet BagNode(DataObject data, double tolerance) {
        var values = toMap(data.properties.Select(static pair =>
            (PropertyName.Create(pair.Key), (PropertyValue)new PropertyValue.Text(pair.Value?.ToString() ?? ""))));
        var seed = new Node.PropertySet(NodeId.Rooted(), new PropertyBag(data.speckle_type, values, InheritanceMode.OccurrenceWins));
        return seed with { Id = NodeId.Content(seed.ToCanonicalBytes(tolerance).Span) };
    }

    // Compose.Contain edges from the namespace nesting: a host's parent is the host with the longest path that is a
    // strict prefix of its own (the nearest enclosing DataObject), so the Speckle containment tree the flat-row
    // projection dropped rides the neutral Compose edge a Bake fold descends; a root host (no DataObject ancestor) adds none.
    static Seq<Relationship> Containment(Seq<(string[] Path, DataObject Data, NodeId Id)> hosts) =>
        hosts.Choose(child => hosts
            .Filter(parent => parent.Id != child.Id && IsPrefix(parent.Path, child.Path))
            .OrderByDescending(static parent => parent.Path.Length)
            .ToSeq().Head
            .Map(parent => (Relationship)new Relationship.Compose(parent.Id, child.Id, ComposeKind.Contain)));

    static bool IsPrefix(string[] prefix, string[] path) =>
        prefix.Length < path.Length && prefix.AsSpan().SequenceEqual(path.AsSpan(0, prefix.Length));
}
```

## [04]-[REIMPORT]

- Owner: `BimIo.Reimport` the projector-polymorphic incremental re-ingest — re-projecting a revised source through ANY `IElementProjection` and reconciling it to a prior `ElementGraph` snapshot by `ExternalId`, so a large model's minor revision costs the delta, not the whole graph; `ReimportResult` the receipt carrying the patched `ElementGraph` plus the delta-cost `GraphDelta` the reconcile produces in one fold; `Reconcile` the `ExternalId`-keyed structural diff and `Remap` the node/edge id-reidentification.
- Entry: `BimIo.Reimport(IElementProjection projector, ElementGraph prior, ProjectionContext ctx, Op key)` re-projects a revised source (the caller decodes the revised bytes once into the projector — `ImportIfc` → `new SemanticProjector(db)`, or a `Base` → `new SpeckleProjector(root)`) and reconciles the fresh graph to `prior` by `ExternalId` (the IFC `GlobalId` / Speckle `applicationId`), emitting only the added/revised/removed nodes and edges — `Fin<T>` funnels a foreign projector fault to `Projection/fault#FAULT_BAND` `ElementFault.ProjectionFailed` through `key.Catch` and rails `ElementFault.NodeAbsent` at `Graph/element#ELEMENT_GRAPH` `Apply` on a corrupt delta; the heavy display geometry is NEVER re-tessellated because an unchanged representation content-keys identically on `RepresentationContentHash`, so the incrementality is wholly in the reconcile, the whole-file re-projection notwithstanding.
- Auto: `Reimport` runs the projector once onto a `Genesis(ctx.Header)` seed to a fresh revised `ElementGraph`, then `Reconcile` remaps each revised rooted `Object` to its prior identity by `ExternalId` (a re-projection mints FRESH neutral Guid-v7 ids, so identity is matched on the stable external id, never the node id) and rewrites every revised node and edge through that id map; the structural diff then partitions: a remapped node absent from `prior` is `AddedNodes`, present with DIFFERING `ToCanonicalBytes` is `RevisedNodes`, a prior node absent from the revised set is `RemovedNodes`, and edges diff by structural equality — a non-rooted content-keyed node (Material/PropertySet/...) needs no remap because identical content already shares its `NodeId`, so only rooted Objects with a stable remapped id and changed canonical bytes are revisions; the resulting `GraphDelta` applied to `prior` yields the reconciled `Patched`, the delta IS the change set, never a second diff pass.
- Receipt: the `ReimportResult` carries the patched `ElementGraph` (the prior snapshot advanced by the incremental delta) and the forward `GraphDelta` the `Rasm.Persistence` event log stores — a delta-cost minor revision, the `csharp:Rasm.Persistence/Version/ledger#GRAPH_DELTA` stream appending only the changed nodes/edges, the `GraphDelta.ToCanonicalBytes` content key deduping a re-applied delta; the `Review/diff#MODEL_DIFF` `ElementChange` federation change-set is the SEPARATE review surface, not minted here.
- Packages: Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new re-ingestable source is one more `IElementProjection` the caller hands `Reimport` (the IFC `SemanticProjector`, the `SpeckleProjector`, a future Materials/Fabrication projector) — the reconcile is projector-agnostic, keyed only on `ExternalId`, so no second reimport entrypoint; a finer change granularity is the `ToCanonicalBytes` content comparison the diff already uses; never a parallel delta store and never a re-tessellation of a content-key-matched representation.
- Boundary: the reconcile keys on the seam `Object.ExternalId` (the IFC `GlobalId` / Speckle `applicationId`) — a re-projection mints fresh neutral Guid-v7 ids, so matching on the node id would treat every element as new; a second identity scheme or a field-by-field element comparison is the deleted form; the `GraphDelta` is the FORWARD event delta the Persistence stream stores, distinct from the `Review/diff#MODEL_DIFF` `ElementChange` review change-set — minting a `ModelDiff` here is the deleted form; reimport is ONE polymorphic owner over `IElementProjection` and a per-format `ReimportIfc`/`ReimportSpeckle` family or the retired `BimModel`/`BimModel.Project` patch is the deleted form; a content-key-matched representation is never re-tessellated (the `RepresentationContentHash` is identical) and a re-tessellation is the named seam violation; the patched value is the one `Graph/element#ELEMENT_GRAPH` `ElementGraph` snapshot, never a parallel delta-model; a corrupt reconcile delta rails `Projection/fault#FAULT_BAND` `ElementFault.NodeAbsent` at `Apply`.

```csharp signature
public sealed record ReimportResult(ElementGraph Patched, GraphDelta Delta);

public static partial class BimIo {
    // Incremental re-ingest, projector-polymorphic over ANY IElementProjection (the IFC SemanticProjector, the Speckle
    // SpeckleProjector, a future Materials/Fabrication projector): re-project a revised source to a fresh ElementGraph,
    // reconcile it to the prior snapshot by ExternalId, and emit the delta-cost GraphDelta the Persistence event log
    // stores. The caller decodes the revised bytes once (ImportIfc -> new SemanticProjector(db), or a Base ->
    // new SpeckleProjector(root)) and hands the projector, so reimport never re-decodes a format and stays ONE
    // polymorphic owner — the retired BimModel/ModelDiff patch over GlobalId-keyed BimElement rows is the deleted form
    // (BimModel is retired; the GraphDelta IS the forward event delta, distinct from the Review/diff#MODEL_DIFF review
    // change-set). A thrown foreign fault funnels to ElementFault.ProjectionFailed through key.Catch; a corrupt
    // reconcile delta naming an absent endpoint rails ElementFault.NodeAbsent at Graph/element#ELEMENT_GRAPH Apply.
    public static Fin<ReimportResult> Reimport(IElementProjection projector, ElementGraph prior, ProjectionContext ctx, Op key) =>
        key.Catch(() => projector.Project(ctx))
            .Map(fresh => fresh.ReplayOnto(ElementGraph.Genesis(ctx.Header)))
            .Map(revised => Reconcile(prior, revised))
            .Bind(delta => prior.Apply(delta, key).Map(patched => new ReimportResult(patched, delta)));

    // The ExternalId reconcile: a re-projection mints FRESH rooted ids (neutral Guid v7), so a rooted Object is matched
    // to its prior identity by ExternalId (the IFC GlobalId / Speckle applicationId) and the revised ids are remapped to
    // the prior ids — an unchanged element keeps its identity and contributes no change. A non-rooted node (Material/
    // PropertySet/...) is content-keyed, so an unchanged one already shares its NodeId and a changed one is a fresh id
    // (add + remove); only a rooted Object with the SAME remapped id and DIFFERING canonical bytes is a RevisedNode.
    // The heavy display geometry is NEVER re-tessellated because RepresentationContentHash content-keys an unchanged
    // representation identically; the forward GraphDelta applied to prior yields the reconciled revised — the delta-cost
    // minor revision, the Review/diff#MODEL_DIFF ElementChange federation surface a SEPARATE concern, not minted here.
    static GraphDelta Reconcile(ElementGraph prior, ElementGraph revised) {
        double tolerance = revised.Header.Tolerance;
        var priorByExternal = prior.ObjectNodes
            .Choose(static o => o.ExternalId.Map(x => (External: x, o.Id)))
            .ToMap(static p => p.External, static p => p.Id);
        var remap = revised.ObjectNodes
            .Choose(o => o.ExternalId.Bind(x => priorByExternal.Find(x)).Map(priorId => (o.Id, Prior: priorId)))
            .ToMap(static p => p.Id, static p => p.Prior);
        NodeId Reidentify(NodeId id) => remap.Find(id).IfNone(id);
        var revisedNodes = revised.Nodes.Values.Select(n => Remap(n, Reidentify)).ToSeq();
        var revisedEdges = revised.Edges.Select(e => Remap(e, Reidentify)).ToSeq();
        var revisedById = revisedNodes.ToMap(static n => n.Id);
        var added = revisedNodes.Filter(n => !prior.Nodes.ContainsKey(n.Id));
        var removed = prior.Nodes.Keys.Where(id => !revisedById.ContainsKey(id)).ToSeq();
        var revisedPairs = revisedNodes.Choose(n => prior.Nodes.TryGetValue(n.Id, out Node? p)
            && !p.ToCanonicalBytes(tolerance).Span.SequenceEqual(n.ToCanonicalBytes(tolerance).Span)
                ? Some((Before: p, After: n))
                : None);
        var addedEdges = revisedEdges.Filter(e => !prior.Edges.Contains(e));
        var removedEdges = prior.Edges.Where(e => !revisedEdges.Contains(e)).ToSeq();
        return new GraphDelta(added, removed, revisedPairs, addedEdges, removedEdges, Some(revised.Header));
    }

    // Remap a node's identity to its prior identity (Object only — a content-keyed node is not in the remap, so the
    // lookup is identity for it), so an ExternalId-matched element keeps its prior NodeId across the re-projection.
    static Node Remap(Node node, Func<NodeId, NodeId> reidentify) =>
        node is Node.Object o ? o with { Id = reidentify(o.Id) } : node;

    // Remap an edge's endpoints through the same id lookup so an edge between matched elements reconnects the prior ids.
    static Relationship Remap(Relationship edge, Func<NodeId, NodeId> reidentify) => edge.Switch<Relationship>(
        compose:   c => new Relationship.Compose(reidentify(c.Whole), reidentify(c.Part), c.SubKind),
        assign:    a => new Relationship.Assign(reidentify(a.Subject), reidentify(a.Definition), a.SubKind),
        associate: r => new Relationship.Associate(reidentify(r.Subject), reidentify(r.Resource), r.Usage),
        connect:   n => new Relationship.Connect(reidentify(n.From), reidentify(n.To), n.SubKind, n.Realizing.Map(reidentify)),
        @void:     v => new Relationship.Void(reidentify(v.Host), reidentify(v.Feature), v.SubKind),
        generic:   g => new Relationship.Generic(g.WireName, reidentify(g.Relating), reidentify(g.Related), g.Attributes));
}
```

## [05]-[RESEARCH]

- [ACADSHARP_DWG_DECODE]: the `acad-sharp` codec the `Exchange/format#FORMAT_AXIS` `Dwg` row carries reads DWG/DXF in-process through the decompile-verified `ACadSharp` surface (catalogued folder-local at `.api/api-acadsharp`) — `DxfReader.Read(Stream)`/`DwgReader.Read(Stream)` return a `CadDocument` whose `Entities` (an alias for `ModelSpace.Entities`) carry the drawing geometry, and the Bim arm folds the mesh-bearing entities onto the canonical `ImportedGeometry` triangle-soup: `Mesh` (the `Vertices` `List<XYZ>` plus the `Faces` `List<int[]>` n-gon index list, each face fan-triangulated), `Face3D` (the `FirstCorner`..`FourthCorner` `XYZ` quad, the fourth corner equal to the third for a triangle), `PolyfaceMesh` (the `VertexFaceMesh` vertex pool indexed by the 1-based signed `VertexFaceRecord` `Index1`..`Index4` records, a negative index a hidden edge and a zero `Index4` a triangle, fan-triangulated), and the `Insert`-referenced geometry flattened through the package-owned `Insert.Explode()` (the OCS→WCS placement, `Rotation`, per-axis scale, OCS `Normal`, and `MINSERT` array replication `ACadSharp` owns, each placed entity folded back through the same classifier so a block-nested `Insert` recurses) rather than a hand-rolled `InsertPoint`/`XScale` matrix — the 2D profile entities (`LwPolyline`/`Polyline2D`/`Arc`/`Circle`/`Spline`) the `csharp:Rasm.Fabrication` `Polygon/import` boundary owns are skipped, the one `CadDocument` read by two folders each projecting its owned entity families; `ACadSharp` is pure-managed AnyCPU IL, osx-arm64-safe, already consumed by Fabrication and AppUi, so the DWG/DXF round-trip lands managed without crossing the Python companion, the `netDxf` (DXF-only) reader NOT admitted because `ACadSharp` supersedes it (managed DWG AND DXF), and the reader exception lowers to `BimFault.ModelRejected` through the `BimIo.Boundary` funnel.
- [NATIVE_FORMAT_BRIDGES]: the Revit `.rvt` and Navisworks `.nwc`/`.nwd` native readers ride the `native-companion` codec through the Compute companion process (the managed C# branch has no native loader for the proprietary application formats); DWG/DXF is now managed in-process through the `acad-sharp` codec (the `[ACADSHARP_DWG_DECODE]` note above), no longer a `native-companion` two-hop; the `mesh-text` OBJ/STL/OFF decode is managed in-process through the `geometry3Sharp` `StandardMeshReader`/`OBJFormatReader`/`STLFormatReader`/`OFFFormatReader` surface decompile-verified in `.api/api-geometry3sharp`, projecting the `DMesh3` (`VertexIndices`/`TriangleIndices`/`GetVertex`/`GetVertexNormal`/`GetTriangle`) onto the canonical `ImportedGeometry` triangle-soup; PLY and 3MF have LEFT the `mesh-text` codec because `geometry3Sharp` ships no PLY and no 3MF handler — PLY is the dedicated `ply-net` codec composing `Ply.Net` `PlyParser.Parse(stream, maxChunkSize)` over the immutable `Dataset`/`ElementData` record graph (the `Vertex` element's typed `x`/`y`/`z`/`nx`/`ny`/`nz` columns read as a `System.Array` per `DataType` and the `Face` element's `vertex_indices` list column fan-triangulated, ascii/`binary_little_endian`/`binary_big_endian` off `Header.Format`), retiring the hand-rolled BCL `PlyReader`, and 3MF is the `scene-exchange` codec composing the `AssimpNetter` `AssimpContext.ImportFileFromStream` `Scene`→`Mesh` fold under the canonical `Triangulate | JoinIdenticalVertices | GenerateSmoothNormals` post-process, retiring the hand-rolled BCL `ThreeMfReader` OPC/ZIP parse — `AssimpNetter` ships its own osx-arm64 `libassimp.dylib` admitted as the one scene-exchange owner (FBX/Collada/3MF), so the former native-coupling rejection no longer holds and the rejected reader picks that DO stand are `lib3mf` (native C++) and `Aspose.3D` (closed/commercial); each codec materializes one contiguous `ImportedGeometry` boundary allocation per the boundary-mapping law, the leaked `Ply.Net.*`/`Assimp.*` package types never crossing past `Exchange/import`.
- [SPECKLE_CATALOGUE]: the `Speckle.Sdk`/`Speckle.Objects` member spellings the `ImportSpeckle` display-mesh fold and the `SpeckleProjector` host-object fold compose — `Speckle.Sdk.Models.Base` (`id`/`applicationId`/`speckle_type`/`GetTotalChildrenCount`), `Speckle.Sdk.Models.Extensions.BaseExtensions.Flatten`/`Traverse`/`TraverseWithPath`/`TryGetDisplayValue`/`IsDisplayableObject` with the `BaseRecursionBreaker` delegate (`TraverseWithPath → IEnumerable<(string[], Base)>` the path-carrying walk the `SpeckleProjector` reconstructs `Compose.Contain` containment from by nearest-ancestor path prefix), `Speckle.Sdk.Models.GraphTraversal.TraversalContext.Parent`/`PropName`/`Current`, `Speckle.Sdk.Common.Units.GetConversionFactor`/`Meters`, `Speckle.Objects.Geometry.Mesh` (`vertices`/`faces`/`vertexNormals` `List<double>`/`List<int>`, length-prefixed n-gon `faces`, `units`, `VerticesCount`), `Speckle.Objects.Geometry.Brep` (`displayValue` `List<Mesh>`), `Speckle.Objects.IDisplayValue<out T>`, and `Speckle.Objects.Data.DataObject` (`name`/`displayValue` `List<Base>`/`properties` `Dictionary<string, object?>`) and its host-object subtypes (`RevitObject`/`ArchicadObject`/`TeklaObject`/`Civil3dObject`/`AutocadObject`/`RhinoObject`, each extending `DataObject`) — are decompile-verified against the `Speckle.Sdk`/`Speckle.Objects` 3.21.1 assemblies and catalogued folder-local at `.api/api-speckle`, which records the `Mesh.vertexNormals` flat-normal member, the `DataObject.properties` `Dictionary<string, object?>` shape, and the host-object subtype roster the cross-folder `csharp:Persistence/.api/api-speckle` sync catalogue elides; the host-object semantic projection is the `SpeckleProjector : IElementProjection` lowering the `DataObject` tree onto a seam `GraphDelta` (each host object a rooted `Node.Object` carrying the generic `Classification("speckle", speckle_type)` and the `applicationId` `ExternalId`), never the retired `IfcSemanticModel` flat rows.
- [IFC5_ECS]: the IFC5 ECS-JSON (`.ifcx`) component-graph parse rides the GeometryGym IFC5 surface for the live `DatabaseIfc` the `Projection/semantic#SEMANTIC_PROJECTOR` lowers, and the Compute companion for native-grade tessellation; the `ifc5` row mirrors the IFC4x3 decode (bytes → `DatabaseIfc`), the ECS-component projection riding the projector's seam graph. IFC4.3 ADD2 is the production baseline (`ReleaseVersion.IFC4X3_ADD2`); IFC5 is the componentized/granular `.ifcx` architecture in active public development, so the `ifc5` row is a forward-looking GeometryGym IFC5-surface row grounding against the GeometryGym IFC5 member surface at alignment.
- [SEAM_DECODE]: the import-rail IFC arm collapse to a `DatabaseIfc`-only decode grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT C5 and the `Projection/semantic#SEMANTIC_PROJECTOR` owner — the `SemanticProjector : IElementProjection` reads the LIVE `DatabaseIfc` (not lossy import-rail rows), so the FULL `IfcRel*` roster, the eight stranded families, `OwnerHistory`, `StepHeader`, and the per-bag `InheritanceMode` survive on the neutral seam edge algebra; the retired `IfcSemanticModel` flat-row projection that dropped them is the deleted form §2/§4B name, and `DatabaseIfc` is captured by the projector internally so GeometryGym never crosses the seam `IElementProjection.Project` signature (the §4A IoC inversion). The ifcJSON/ifcXML `DatabaseIfc` construction reads its `ReleaseVersion` from `SemanticProjector.Sniff(bytes, format)` (the `schema_identifier`/`FILE_SCHEMA` read [H8]) rather than a hardcoded `IFC4X3_ADD2`, so a 2x3 file admits as 2x3 — the ONE schema-sniff owner this `Database` decode and the `wire#WIRE_PROJECTION` `Admit` decode both compose (the cross-file dedup `wire` flags). `DatabaseIfc.ParseString`/`ReadJSON`/`ReadXMLDoc`/`Release`/`ModelView`/`Tolerance` and `SemanticProjector.Sniff` confirm against `.api/api-geometrygym-ifc` and `Projection/egress#IFC_EGRESS`.
- [REIMPORT_RECONCILE]: the projector-polymorphic reimport grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT H6 (a rooted `NodeId` is a NEUTRAL kernel-minted Guid-v7, the IFC `GlobalId` a Bim-stored `ExternalId` projection) — a re-projection mints fresh ids, so `Reconcile` matches a rooted `Object` to its prior identity by `ExternalId`, never the node id; the forward `GraphDelta` is §4-RT C1/H11's event body the `Rasm.Persistence` Marten stream appends (distinct from the §4F `Review/diff#MODEL_DIFF` `ElementChange` review change-set per `Graph/element#ELEMENT_GRAPH`'s two-change-surface law), and the §4-RT H7 `Node.ToCanonicalBytes` content comparison drives the revised-node detection. `ElementGraph.Genesis`/`Apply`/`ObjectNodes`, `GraphDelta.ReplayOnto`/`Reheader`, and `Node.Object.ExternalId` confirm against the seam `Graph/element`/`Graph/delta` owners.
