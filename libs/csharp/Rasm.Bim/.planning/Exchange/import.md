# [BIM_IMPORT_RAIL]

The foreign-bytes ingest rail: one `BimIo` import fold over the `format#FORMAT_AXIS` `InterchangeFormat` rows, dispatching the managed glTF/GLB mesh-and-scene decode through SharpGLTF, the STL/OBJ/OFF mesh-text arm through `geometry3Sharp`, the dedicated PLY decode through `Ply.Net` (retiring the hand-rolled BCL `PlyReader`), the FBX/Collada/3MF scene decode through `AssimpNetter` (retiring the hand-rolled BCL `ThreeMfReader`), the OpenUSD scene decode through `UniversalSceneDescription` `UsdStage`, the managed `.bim` decode through `dotbim` (the shared-`Mesh`-pool, placed-`Element` instancing wire, its `Info` semantics riding the `DotbimProjector : IElementProjection`), the in-process IFC/IFC5 graph decode through GeometryGym to the live `DatabaseIfc` the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector` lowers to a seam `GraphDelta`, the in-process ISO 10303-21 Part-21 product-structure semantic ingest through the BCL-only `StepReader`, the AP242/native-companion two-hop geometry route, and the Speckle `Base` object-graph seam folding a deserialized `Speckle.Sdk.Models.Base` tree onto the `ImportedGeometry` display-mesh carrier and a seam `GraphDelta` through the `SpeckleProjector : IElementProjection` host-object projection — never tessellated BRep, never a lossy `IfcSemanticModel` flat-row re-projection. `ImportedGeometry` is a mesh POOL: `Blocks` ranges hold each decoded source mesh once and `Instances` places them by rigid transform, so an instanced source (glTF node reuse + `EXT_mesh_gpu_instancing`, the dotbim `Element` pool, the Assimp node tree, the USD `UsdGeomXformCache` prim placement) round-trips its sharing to `export#EXPORT_RAIL` instead of N baked copies — `Bake()` flattens on demand. The import rail OWNS the foreign byte->carrier decode; the entity walk, the full `IfcRel*` relationship roster, the typed property/quantity projection, `OwnerHistory`, and `StepHeader` are the `Rasm.Element` seam projector's, read off the live graph. The page composes the kernel `Rasm` geometry and consumes the `format#FORMAT_AXIS` codec/frame rows as settled vocabulary; an IFC/native/Speckle-non-mesh geometry request routes to `tessellation#TESSELLATION_BRIDGE`. The page is HOST-LOCAL in posture; the Speckle seam composes `Speckle.Sdk`/`Speckle.Objects` and runs only in the host-neutral exchange assembly, never inside the in-Rhino plugin ALC.

## [01]-[INDEX]

- [01]-[IMPORT_RAIL]: foreign-bytes ingest — managed mesh decode to the pooled/instanced `ImportedGeometry` (`Blocks`/`Instances`/`Bake`), the dotbim `.bim` arm plus its `DotbimProjector` Info-bag seam, the in-process IFC/IFC5 decode to the live `DatabaseIfc` the seam `SemanticProjector` lowers, and the in-process STEP product-structure `StepSemanticModel`.
- [02]-[SPECKLE_SEAM]: Speckle `Base` object-graph — the display-mesh decode to `ImportedGeometry` and the `SpeckleProjector : IElementProjection` host-object projection to a seam `GraphDelta`.
- [03]-[REIMPORT]: projector-polymorphic incremental re-ingest — re-project a revised source, reconcile to the prior `ElementGraph` by `ExternalId`, emit the delta-cost `GraphDelta`.

## [02]-[IMPORT_RAIL]

- Owner: `BimIo` — the import fold over `InterchangeFormat`, dispatching the managed glTF/GLB mesh-and-scene decode through SharpGLTF, the OBJ/STL/OFF mesh-text arm through the `geometry3Sharp` `StandardMeshReader`, the dedicated PLY decode through the `Ply.Net` `PlyParser` (the `ply-net` codec retiring the BCL `PlyReader`), the FBX/Collada/3MF scene decode through the `AssimpNetter` `AssimpContext` (the `scene-exchange` codec retiring the BCL `ThreeMfReader`), the OpenUSD scene decode through the `UniversalSceneDescription` `UsdStage` (the `usd-stage` codec), the in-process IFC/IFC5 decode through GeometryGym to the live `DatabaseIfc`, the managed BCL-only `StepReader` ISO 10303-21 Part-21 entity-instance-graph semantic ingest over the `StepIso10303` codec, the managed `.bim` decode through `dotbim` over the `DotBim` codec (the geometry pool arm plus the `DotbimProjector : IElementProjection` Info-bag seam arm), and the AP242/native-companion two-hop geometry route; `ImportedGeometry` the decoded mesh-POOL carrier (`Vertices`/`Normals`/`Indices` hold each source mesh once as a `MeshBlock` range, `MeshInstance` rows place blocks by rigid transform, `Bake()` flattens on demand), the live `DatabaseIfc` the IFC graph the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector` captures and lowers to a seam `GraphDelta`, `StepSemanticModel` the ISO 10303 product-structure projection.
- Entry: `BimIo.ImportGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, IClock clock, Op key)` for the managed mesh-and-scene path (dispatching by `InterchangeCodec` to SharpGLTF, the `geometry3Sharp` mesh-text arm, `Ply.Net`, `AssimpNetter`, `UsdStage`, ACadSharp, or `dotbim`); `BimIo.ImportIfc(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Op key)` for the in-process IFC/IFC5 decode to the live `DatabaseIfc` the seam `SemanticProjector` captures; `BimIo.ImportStep(InterchangeFormat format, ReadOnlyMemory<byte> bytes, IClock clock, Op key)` for the in-process ISO 10303-21 Part-21 product-structure semantic graph — `Fin<T>` aborts on a codec reject (`Model/faults#FAULT_BAND` `BimFault.CodecReject`) or a companion-required geometry request (`BimFault.CapabilityMiss`), each `Op`-keyed case lifting BARE onto the `Fin<T>` rail (band 2600 IS the `Expected` `Code` — no `.ToError()` hop), the foreign decode arity discriminating on the row's `InterchangeCodec` so a path lands one decode without a call-site type branch, projecting the package or parse exception onto `BimFault.ModelRejected(key, error.Message)` at the boundary so domain code never sees the SharpGLTF `ModelException`, the GeometryGym parse fault, or a malformed-Part-21 `InvalidDataException`.
- Auto: binary GLB decode lands through `ModelRoot.ParseGLB(ArraySegment<byte>)` and text `.gltf` decode through `ReadContext.ReadTextSchema2(Stream)` then a `Decompress` pre-decode branch reading the parsed model's `KHR_draco_mesh_compression` primitive extension and `EXT_meshopt_compression` bufferView extension and routing the compressed payload through the package-owned `Draco.Decode(byte[])` and `Meshopt.DecodeVertexBuffer`/`DecodeIndexBuffer`/`DecodeFilterOct`/`DecodeFilterQuat`/`DecodeFilterExp`/`DecodeFilterColor` decoders before `model.LogicalMeshes.Decode()` projects each logical mesh to ONE `MeshBlock` and the `Node.Flatten(model.DefaultScene)` walk places it per mesh-bearing node — the node `WorldMatrix`, fanned per `EXT_mesh_gpu_instancing` row through `GetGpuInstancing().GetWorldMatrix(i)` — with zero intermediate file; the IFC semantic path constructs the live `DatabaseIfc` over the bytes through `DatabaseIfc.ParseString`/`ReadXMLDoc`/`ReadJSON` by the row's format — the ifcJSON/ifcXML construction reading its `ReleaseVersion` from `SemanticProjector.Sniff(bytes, format)` BEFORE construction [H8] rather than a hardcoded `IFC4X3_ADD2`, the in-process IFC graph the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector` captures internally and lowers to a seam `GraphDelta`; the entity walk (`db.Project.Extract<T>()` over the spatial hierarchy, products with the `ParserIfc.IdentifyIfcClass` predefined split, property/quantity sets, materials, classifications, type objects with the `IfcTypeProduct.RepresentationMaps` instanced-geometry content key, the grouping/zone overlays, the `IfcMapConversion`/`IfcProjectedCRS` georeference, and the FULL `IfcRel*` relationship roster including the eight families the retired flat rows stranded), the per-bag `InheritanceMode` stamp, the `OwnerHistory`, and the `StepHeader` are the projector's — read off this live graph, never a lossy `IfcSemanticModel` flat-row re-projection here and never tessellated BRep.
- Receipt: the `ModelLoad` receipt case carries the format key, codec key, source byte count, and elapsed for a managed mesh import — an instanced source additionally reads the carrier's `Blocks.Count`/`Instances.Count` sharing evidence; an IFC decode stamps the schema version (`db.Release`) and the model-view (`db.ModelView`) read off the live `DatabaseIfc` (the entity-count receipt rides the `SemanticProjector`'s delta, not the import rail); a STEP semantic ingest stamps the `StepProtocol`, the `FILE_SCHEMA` schema name, and the product/definition/assembly/geometry-ref counts; emission rides the sink port at the composition edge.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Runtime, GeometryGymIFC_Core, Openize.Drako, Alimer.Bindings.MeshOptimizer, CommunityToolkit.HighPerformance, geometry3Sharp, Ply.Net, AssimpNetter, UniversalSceneDescription, ACadSharp, dotbim, NodaTime, LanguageExt.Core, Rasm
- Growth: a new managed import is one codec arm on the import fold keyed by the `InterchangeFormat.Codec` row; a new instancing-bearing source is `Append`/`Place` calls inside its one arm — the `Blocks`/`Instances` overlay is format-agnostic, so no carrier edit and no second soup; a new extracted IFC entity family is one `Extract<T>` arm on the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector`, never on the import rail (which owns only the byte->`DatabaseIfc` decode); a new extracted STEP entity family is one `Keyword`-filtered projection on `StepSemanticModel` over the resolved instance graph; a new STEP application protocol is one `InterchangeFormat` row carrying its `StepProtocol` discriminant — the single `StepReader` reads the protocol off `format.StepProtocol` and the entity-instance grammar is protocol-agnostic, so AP203/AP214/AP242 share one reader and one codec without a per-protocol reader; a new glTF compression codec is one `KhrEncoder`-keyed arm on the `Decompress` pre-decode branch symmetric to the `export#EXPORT_RAIL` `GlbBytes` compression switch, never a second importer.
- Boundary: `BimIo` is the page boundary capsule and its codec arms carry the language-owned statement forms the foreign package decode requires; glTF mesh decode rides the `MeshDecoder.Decode` runtime contract reading `IMeshPrimitiveDecoder.GetPosition`/`GetNormal`/`TriangleIndices` (an accessor-based contract returning per-vertex `Vector3`/index-tuple values, so the decode materializes one contiguous `ImportedGeometry` vertex/normal/index triple at the boundary — the accessor contract admits no zero-copy span into SharpGLTF's internal buffers, so the one boundary materialization, not a per-primitive `float[]` proliferation, is the allocation point); the `mesh-text` decode arm reads the OBJ/STL/OFF mesh-text containers through the `geometry3Sharp` `StandardMeshReader.Read(Stream, extension, ReadOptions)` extension-dispatched reader into a `DMesh3Builder`, projecting the resulting `DMesh3` (`VertexIndices()`/`TriangleIndices()` over the refcounted sparse pools, `GetVertex(int)` `Vector3d` position, `GetVertexNormal(int)` `Vector3f` normal, `GetTriangle(int)` `Index3i`) onto the same contiguous `ImportedGeometry` vertex/normal/index triple the glTF arm materializes — one boundary allocation, the `DMesh3` sparse index space iterated through its live-id enumerators rather than a dense `0..VertexCount` loop because the refcounted pool leaves holes; the `mesh-text` arm is now geometry3Sharp ONLY (OBJ/STL/OFF) because PLY and 3MF left the codec — PLY is the dedicated `ply-net` codec composing `Ply.Net` `PlyParser.Parse(stream, maxChunkSize)` (the `Ply` arm: the `Dataset.Data` `ElementData` rows read the `Vertex` element's typed `x`/`y`/`z`/`nx`/`ny`/`nz` columns as a `System.Array` typed per `DataType` and the `Face` element's `vertex_indices` `int[][]` list column fan-triangulated, retiring the hand-rolled BCL `PlyReader` endian/ascii fork), and FBX/Collada/3MF are the `scene-exchange` codec composing `AssimpNetter` (the `Scene` arm: one disposable `AssimpContext.ImportFileFromStream(stream, PostProcessSteps.Triangulate | JoinIdenticalVertices | GenerateSmoothNormals, format.Key)` folding the `Scene.Meshes` `Vertices`/`Normals`/`Faces` graph onto the triangle-soup, retiring the hand-rolled BCL `ThreeMfReader` OPC/ZIP parse); the OpenUSD `usd-stage` codec composes `UniversalSceneDescription` (the `Usd` arm: `UsdStage.Open` over the temp-path layer stack, `Traverse` filtering each `UsdGeomMesh` prim by `GetTypeName()`, reading the points/authored-normals `VtVec3fArray`/`GfVec3f` and the `GetFaceVertexCountsAttr`/`GetFaceVertexIndicesAttr` `VtIntArray` topology through the typed-array mesh-bridge seam, fan-triangulating each face, each prim PLACED by the composed `UsdGeomXformCache.GetLocalToWorldTransform` local-to-world — USD a scene-graph peer, the BIM semantics staying the GeometryGym IFC graph's, never re-derived from USD prim type names); each arm materializes one contiguous `ImportedGeometry` boundary allocation and the leaked package types (`Ply.Net.*`, `Assimp.*`, `pxr.*`) never cross past `Exchange/import` — internal code holds the canonical carriers per the boundary-mapping law, and the `SWIGTYPE_p_*`/`*PINVOKE` USD interop types never enter the fold; the rejected reader picks stand (`lib3mf` native C++, `Aspose.3D` closed/commercial — `AssimpNetter` ships its own osx-arm64 `libassimp.dylib` admitted as the one scene-exchange owner), and `geometry3Sharp.OBJReader`/`STLReader` stay consumed through the `StandardMeshReader` dispatch, never a second hand-rolled tokenizer; the IFC arm decodes ONLY the live `DatabaseIfc` — the entity walk and the seam-node/edge projection are the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector`'s (it captures the `DatabaseIfc` internally so GeometryGym never crosses the seam `IElementProjection.Project` signature), and a lossy `IfcSemanticModel` flat-row re-projection here that drops the eight stranded `IfcRel*` families, `OwnerHistory`, and `StepHeader` is the deleted form; GeometryGym carries no tessellation kernel, so a geometry request on an IFC row routes to the `tessellation#TESSELLATION_BRIDGE` rail and never evaluates a BRep in-process; the `step-iso10303` STEP route splits two legs: the managed semantic-graph leg is now in-process through the BCL-only `StepReader`, while the B-rep/NURBS geometry leg stays companion-routed because no managed STEP solid evaluator is admitted — `StepReader` strips the Part-21 comment and string spans, splits the HEADER and DATA sections, slices each `#N = ENTITY(...);` statement at depth-zero semicolons, parses each statement into an `Instance(Id, Keyword, Args)` where the recursive-descent `ParseArg` discriminates the Part-21 token grammar into a closed `Arg` `[Union]` — reference `#N` (`Arg.Ref`), string `'...'` with the `''` escape (`Arg.Text`), typed-enum `.X.` (`Arg.Enum`), number (`Arg.Number`), nested list `(...)` (`Arg.List`), typed-constructor `KEYWORD(...)` (`Arg.Typed`), and `$`/`*`/identifier (`Arg.Untyped`) — builds the forward-reference instance graph as a `Dictionary<long, Instance>` resolved in a second pass through `Resolve`, and projects the product structure (`PRODUCT` → `ProductRow`, `PRODUCT_DEFINITION` → `DefinitionRow` walking the `PRODUCT_DEFINITION_FORMATION` reference to its `PRODUCT`, `NEXT_ASSEMBLY_USAGE_OCCURRENCE` → `AssemblyEdge` resolving both relating and related definitions to their product ids) and the AP242 PMI/semantic metadata (`DIMENSIONAL_*`/`DATUM`/`GEOMETRIC_TOLERANCE`/`ANNOTATION_OCCURRENCE` filtered through a frozen `PmiTypes` set into `PmiRow`); the geometry entities (`ADVANCED_BREP_SHAPE_REPRESENTATION`/`MANIFOLD_SOLID_BREP`/`B_SPLINE_SURFACE`/`SHAPE_REPRESENTATION` filtered through a frozen `GeometryTypes` set) are NOT evaluated in-process — `StepReader` carries only their `GeometryRef(Id, EntityType, ShapeDefinitionId)` and routes the actual B-rep/NURBS solid evaluation to the `tessellation#TESSELLATION_BRIDGE` companion rail (OpenCascade serving the STEP solid read), so `TessellationRequiresCompanion` stays `true` on the STEP rows; the rejected managed readers stand — `IxMilia.Step` and `StepFileParser` do not exist on NuGet, `STPLoader` is net35/2015/`AForge.Math`-coupled (RID-unsafe, abandoned), and `DevelApp.StepParser` is a GrammarForge regex/PCRE2 grammar engine that does not model the Part-21 entity-instance graph and pulls a prerelease `ICU4N` alpha transitive — so `StepReader` is the in-process BCL-only entity-instance graph the codec needed and a managed STEP B-rep evaluator beside the companion is the deleted form; GeometryGym is IFC-schema-bound and does not parse generic ISO-10303 AP242 product structure, so it grounds no STEP semantic leg; `DatabaseIfc.Tolerance`/`ToleranceAngleRadians`/`ScaleSI` read the model precision the content-key folds; the `Decompress` pre-decode branch is the decompress-on-import arm symmetric to the `export#EXPORT_RAIL` `GlbBytes` compression switch — `SharpGLTF.Core` ships no compression decoder (the catalogued assembly carries no Draco/meshopt type, decompile-verified absence), so a GLB whose primitive carries a `KHR_draco_mesh_compression` extension or whose bufferView carries an `EXT_meshopt_compression` extension reaches the `LogicalMeshes.Decode()` fold with its accessor data still compressed and unreadable, and `Detect` cannot distinguish a compressed GLB from a plain one because the compression rides a per-primitive/per-bufferView extension, not the row; `SharpGLTF.Core` retains no typed handle to an unrecognized extension (`KHR_draco_mesh_compression`/`EXT_meshopt_compression` have no in-box `JsonSerializable` extension class, so the `ExtraProperties.Extensions` collection never holds them and `ExtraProperties.Extras` is a free-form `JsonNode`, not an extension accessor), so the branch reparses the GLB/glTF JSON chunk the `ReadContext.ReadJson`/`IdentifyBinaryContainer` pair already extracts into a `System.Text.Json.Nodes.JsonNode` tree and reads each `meshes[].primitives[].extensions.KHR_draco_mesh_compression` and `bufferViews[].extensions.EXT_meshopt_compression` object directly out of that tree for its `bufferView`/`count`/`byteStride`/`mode`/`filter` parameters; it routes the `KHR_draco_mesh_compression` primitive payload through `Draco.Decode(byte[])` (downcasting the returned `DracoPointCloud` to `DracoMesh`, reading each `PointAttribute` through `DracoPointCloud.GetNamedAttribute(AttributeType.Position)`/`Normal` and `PointAttribute.GetValueAsVector3(PointAttribute.MappedIndex(point))` per point — the point index mapped to its deduplicated attribute-value index, since Draco shares attribute values across points and `GetValueAsVector3` indexes by value, not point — and the faces through `DracoMesh.NumFaces`/`ReadFace(id, int[3])` yielding point indices aligned with that per-point vertex order, then `Fill`-ing the `MeshPrimitive.GetVertexAccessor("POSITION")`/`"NORMAL"` `Accessor.AsVector3Array()` and the `GetIndexAccessor().AsIndicesArray()` cast to their concrete `Vector3Array`/`IntegerArray` write surface — `Fill(IEnumerable<T>)` is a member of the concrete accessor-array structs, not the `IAccessorArray<T>` interface the factory statically returns, so the decode downcasts to the runtime `Vector3Array`/`IntegerArray` before filling) and the `EXT_meshopt_compression` bufferView payload — the compressed slice read from the EXTENSION's `buffer`/`byteOffset`/`byteLength` (per spec the bufferView's own properties describe the UNCOMPRESSED fallback target, so a `view.Content` source read is the deleted inversion) — through `Meshopt.DecodeVertexBuffer`/`DecodeIndexBuffer` then the filter inverse `Meshopt.DecodeFilterOct` (octahedral-encoded normals), `DecodeFilterQuat` (quantized rotations), `DecodeFilterExp` (shared-exponent floats), and `DecodeFilterColor` (quantized vertex color) keyed on the bufferView extension's `mode`/`filter` discriminant, then lands the decoded bytes IN the view's own `Content` region (the bytes every accessor over the view reads; a fallback-less view faults loud) before the `IMeshDecoder` fold so a web-compressed artifact round-trips back through import without a companion — the decode reuses the same dormant `Openize.Drako`/`Alimer.Bindings.MeshOptimizer` surface the export encode switch drives, closing the export-can-compress / import-cannot-decompress asymmetry with zero new package and zero new `InterchangeFormat` row; the SharpGLTF `ReadSettings.Validation` rides `ValidationMode.Strict` on an uncompressed asset so a malformed glTF faults at parse, and a compressed asset parses under `ValidationMode.Skip` past the unrecognized-extension and missing-bufferView-data validation errors the compression extension provokes, then re-validates the reconstructed geometry at the materialization boundary; the `fbx`/`dae` rows are now live `scene-exchange` (`AssimpNetter`) and the USD rows live `usd-stage` (`UniversalSceneDescription`), so the former `import-catalogue-pending` fault no longer fires for them; every codec admit reaching this fold is one `InterchangeCodec`-keyed arm on the existing `ImportGeometry`/`ImportIfc`/`ImportStep` dispatch — the row-promotion discipline `format#FORMAT_AXIS` owns, never a new `BimIo` entrypoint or a parallel importer family — and the companion-versus-managed geometry route is read from the row's `TessellationRequiresCompanion` column rather than a call-site `if (ifc)`/`if (step)` branch; the `dotbim` arm admits `.bim` bytes through one `JsonSerializer.Deserialize<dotbim.File>` (the wire is pure STJ with snake_case `[JsonPropertyName]` members; `File.Read`/`Save` is the path-bound package form) — each pooled `dotbim.Mesh` lands one `MeshBlock`, each `Element` one `MeshInstance` over the quaternion `Rotation` + `Vector` translation, a missing `MeshId` faults the decode, and the `Guid`/`Type`/`Info` semantics ride the `DotbimProjector` seam arm (the `Info` bag one content-keyed `PropertySet`; the 1:1 `ExternalId` prefers the `Info["globalId"]` a Rasm export writes, else the element `Guid`, so `Reimport` reconciles a round-tripped `.bim` against its source graph) so `dotbim.*` never crosses past this capsule; every decode arm folds the ONE `MeshSoup` pool builder (pre-sized/amortized `List<T>` growth, one final contiguous materialization — the per-block `Seq` concatenation that rebuilt three immutable sequences per source mesh is the deleted O(blocks·total) form); `Framed` canonicalizes positions AND normals by their own strided calls and conjugates every instance transform by the row basis (the position-only, transform-blind form is the deleted defect); an `IfcImporter`/`GltfImporter`/`PlyImporter`/`SceneImporter`/`UsdImporter`/`StepImporter`/`DotbimImporter` service family, a per-extension `DracoImporter`/`MeshoptImporter`, a per-protocol AP203/AP214/AP242 STEP reader, and a managed IFC or STEP B-rep tessellator are the deleted forms.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
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
using Matrix4x4 = System.Numerics.Matrix4x4;   // the instance-transform currency — disambiguated from Assimp.Matrix4x4
using Node = Rasm.Element.Node;                // the seam node union owns the bare name; the SharpGLTF scene node is qualified
using Vector3 = System.Numerics.Vector3;       // the numerics coordinate this boundary fold speaks — never the seam Rasm.Element.Vector3

namespace Rasm.Bim;

// --- [MODELS] -----------------------------------------------------------------------------
// The mesh-POOL carrier: Vertices/Normals/Indices hold each decoded source mesh ONCE as a Blocks range, and
// Instances places blocks by rigid transform — an instanced source (glTF node reuse + EXT_mesh_gpu_instancing,
// the dotbim Mesh pool, the Assimp node tree, the USD prim placement) round-trips its sharing to export
// instead of N baked copies. A
// non-instanced decode is one identity instance per block, so its pool IS its world-space scene and Bake()
// returns it unchanged; a consumer needing flat world-space geometry from an instanced carrier calls Bake() once.
public readonly record struct MeshBlock(int VertexOffset, int VertexCount, int IndexOffset, int IndexCount);

public readonly record struct MeshInstance(int Block, Matrix4x4 Transform);

public sealed record ImportedGeometry(
    InterchangeFormat Format,
    ReadOnlyMemory<float> Vertices,
    ReadOnlyMemory<float> Normals,
    ReadOnlyMemory<long> Indices,
    int VertexCount,
    int TriangleCount,
    Seq<MeshBlock> Blocks,
    Seq<MeshInstance> Instances,
    Instant At) {

    public bool IsBaked => Instances.ForAll(static i => i.Transform.IsIdentity);

    // Flatten the pool through the instance placements — positions Transform, normals TransformNormal — into one
    // pre-sized allocation; the baked carrier re-describes itself as one block per placement, identity instances.
    public ImportedGeometry Bake() {
        if (IsBaked) { return this; }
        var (v, n, x) = (Vertices.Span, Normals.Span, Indices.Span);
        int vertexTotal = Instances.Sum(i => Blocks[i.Block].VertexCount);
        int indexTotal = Instances.Sum(i => Blocks[i.Block].IndexCount);
        var (vertices, normals, indices) = (new float[vertexTotal * 3], new float[vertexTotal * 3], new long[indexTotal]);
        var (blocks, placed, vSlot, iSlot) = (Seq<MeshBlock>(), Seq<MeshInstance>(), 0, 0);
        foreach (var instance in Instances) {
            MeshBlock block = Blocks[instance.Block];
            for (int k = 0; k < block.VertexCount; k++) {
                int src = (block.VertexOffset + k) * 3;
                var p = Vector3.Transform(new Vector3(v[src], v[src + 1], v[src + 2]), instance.Transform);
                var m = Vector3.TransformNormal(new Vector3(n[src], n[src + 1], n[src + 2]), instance.Transform);
                int dst = (vSlot + k) * 3;
                (vertices[dst], vertices[dst + 1], vertices[dst + 2]) = (p.X, p.Y, p.Z);
                (normals[dst], normals[dst + 1], normals[dst + 2]) = (m.X, m.Y, m.Z);
            }
            for (int k = 0; k < block.IndexCount; k++) {
                indices[iSlot + k] = vSlot + (x[block.IndexOffset + k] - block.VertexOffset);
            }
            blocks = blocks.Add(new MeshBlock(vSlot, block.VertexCount, iSlot, block.IndexCount));
            placed = placed.Add(new MeshInstance(blocks.Count - 1, Matrix4x4.Identity));
            (vSlot, iSlot) = (vSlot + block.VertexCount, iSlot + block.IndexCount);
        }
        return this with {
            Vertices = vertices, Normals = normals, Indices = indices,
            VertexCount = vertexTotal, TriangleCount = indexTotal / 3, Blocks = blocks, Instances = placed,
        };
    }
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
    // Capability gates first — CataloguePending BEFORE CanImport (pending ⟹ CanImport=false, so the unsupported gate
    // otherwise shadows the richer package-naming message) — then the TOTAL generated InterchangeCodec Switch
    // dispatches every codec: the six managed-mesh codecs decode inline, the IFC/STEP codecs name their own
    // entrypoint, the geospatial/point-cloud codecs name their owning page, the companion codecs route to the bridge.
    // The Switch has NO silent fallthrough, so a new InterchangeCodec row breaks this call site at compile time — a new
    // managed-mesh import lands as one arm and a non-mesh codec is forced to declare its route, never misrouting to a
    // stale "needs-companion" fault the prior == ladder produced for GeometryGym/StepIso10303/geospatial.
    public static Fin<ImportedGeometry> ImportGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, IClock clock, Op key) =>
        format.CataloguePending ? Fin.Fail<ImportedGeometry>(new BimFault.CodecReject(key, $"import-catalogue-pending:{format.Key}:{format.Codec.CataloguePackage.IfNone("unknown")}"))
        : !format.CanImport ? Fin.Fail<ImportedGeometry>(new BimFault.CodecReject(key, $"import-unsupported:{format.Key}"))
        : format.Codec.Switch(
            sharpGltf:        () => Boundary(key, () => Framed(format, Gltf(format, bytes, clock.GetCurrentInstant()))),
            meshText:         () => MeshTextGeometry(format, bytes, clock.GetCurrentInstant(), key),
            ply:              () => Boundary(key, () => Framed(format, Ply(format, bytes, clock.GetCurrentInstant()))),
            sceneExchange:    () => Boundary(key, () => Framed(format, Scene(format, bytes, clock.GetCurrentInstant()))),
            usdStage:         () => Boundary(key, () => Usd(format, bytes, clock.GetCurrentInstant())),   // the arm owns frame selection — upAxis is PER-STAGE metadata
            acadSharp:        () => Boundary(key, () => Framed(format, AcadReader.Read(format, bytes, clock.GetCurrentInstant()))),
            dotBim:           () => Boundary(key, () => Framed(format, DotBim(format, bytes, clock.GetCurrentInstant()))),
            geometryGym:      () => Fin.Fail<ImportedGeometry>(new BimFault.CodecReject(key, $"import-ifc-route:use-ImportIfc:{format.Key}")),
            stepIso10303:     () => Fin.Fail<ImportedGeometry>(new BimFault.CodecReject(key, $"import-step-route:use-ImportStep:{format.Key}")),
            geospatialVector: () => Fin.Fail<ImportedGeometry>(new BimFault.CodecReject(key, $"import-geospatial-route:{format.Key}")),
            geospatialRaster: () => Fin.Fail<ImportedGeometry>(new BimFault.CodecReject(key, $"import-geospatial-route:{format.Key}")),
            pointCloud:       () => Fin.Fail<ImportedGeometry>(new BimFault.CodecReject(key, $"import-point-cloud-route:{format.Key}")),
            nativeCompanion:  () => Fin.Fail<ImportedGeometry>(new BimFault.CapabilityMiss(key, $"import-needs-companion:{format.Key}")),
            igesAnsi:         () => Fin.Fail<ImportedGeometry>(new BimFault.CapabilityMiss(key, $"import-needs-companion:{format.Key}")),
            saf:              () => Fin.Fail<ImportedGeometry>(new BimFault.CodecReject(key, $"import-saf-semantic-route:{format.Key}")),
            cobieXlsx:        () => Fin.Fail<ImportedGeometry>(new BimFault.CodecReject(key, $"import-unsupported:{format.Key}")),
            ifc5Pending:      () => Fin.Fail<ImportedGeometry>(new BimFault.CodecReject(key, $"import-catalogue-pending:{format.Key}")));

    // OBJ/STL/OFF only — PLY now routes to the dedicated `ply-net` codec (the `Ply` arm) and 3MF/FBX/Collada
    // to the `scene-exchange` codec (the `Scene` arm), so the mesh-text sub-dispatch is one geometry3Sharp leg.
    static Fin<ImportedGeometry> MeshTextGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at, Op key) {
        string extension = format.Extensions.Head.Map(static ext => ext.TrimStart('.')).IfNone("");
        return new StandardMeshReader().SupportsFormat(extension)
            ? Boundary(key, () => Framed(format, MeshText(format, extension, bytes, at)))
            : Fin.Fail<ImportedGeometry>(new BimFault.CodecReject(key, $"mesh-text-unsupported:{format.Key}:{extension}"));
    }

    // EVERY DMesh3 the reader yields lands one pool block — an OBJ with N groups/objects builds N meshes, and
    // the first-mesh-only read was the deleted coverage defect; the refcounted sparse id space compacts through
    // a live-id ordinal Dictionary (a pre-sized boundary kernel, never a per-vertex immutable-Map rebuild).
    static ImportedGeometry MeshText(InterchangeFormat format, string extension, ReadOnlyMemory<byte> bytes, Instant at) {
        var builder = new DMesh3Builder();
        var read = new StandardMeshReader { MeshBuilder = builder }
            .Read(new MemoryStream(bytes.ToArray()), extension, ReadOptions.Defaults);
        if (read.code != IOCode.Ok) { throw new InvalidDataException($"<mesh-text-read:{read.code}:{read.message}>"); }
        var soup = new MeshSoup();
        foreach (var mesh in builder.Meshes) {
            var ordinal = new Dictionary<int, int>(mesh.VertexCount);
            foreach (int vid in mesh.VertexIndices()) { ordinal.Add(vid, ordinal.Count); }
            var vertices = new float[ordinal.Count * 3];
            var normals = new float[ordinal.Count * 3];
            bool hasNormals = mesh.HasVertexNormals;
            foreach (var (vid, slot) in ordinal) {
                var p = mesh.GetVertex(vid);
                var n = hasNormals ? mesh.GetVertexNormal(vid) : Vector3f.AxisZ;
                int v = slot * 3;
                (vertices[v], vertices[v + 1], vertices[v + 2]) = ((float)p.x, (float)p.y, (float)p.z);
                (normals[v], normals[v + 1], normals[v + 2]) = (n.x, n.y, n.z);
            }
            var corners = mesh.TriangleIndices()
                .SelectMany(tid => mesh.GetTriangle(tid) is var tri ? new long[] { ordinal[tri.a], ordinal[tri.b], ordinal[tri.c] } : [])
                .ToArray();
            soup.Baked(vertices, normals, corners);
        }
        return soup.ToGeometry(format, at);
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
            ? SemanticProjector.Sniff(bytes, format, key).Bind(schema => Boundary(key, () => Database(format, bytes, schema)))
            : Fin.Fail<DatabaseIfc>(new BimFault.CodecReject(key, $"ifc-codec-miss:{format.Key}"));

    public static Fin<StepSemanticModel> ImportStep(InterchangeFormat format, ReadOnlyMemory<byte> bytes, IClock clock, Op key) =>
        format.Codec == InterchangeCodec.StepIso10303
            ? Boundary(key, () => StepReader.Read(format, bytes.Span, clock.GetCurrentInstant()))
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
        return Decoded(format, compressed ? Compression.Decompress(model, bytes) : model, at);
    }

    static ReadContext TextContext(ReadOnlyMemory<byte> bytes, ValidationMode validation) {
        var context = ReadContext.CreateFromDictionary(
            new Dictionary<string, ArraySegment<byte>> { ["model.gltf"] = new ArraySegment<byte>(bytes.ToArray()) },
            checkExtensions: true);
        context.Validation = validation;
        return context;
    }

    // One block per LOGICAL mesh (the per-primitive corner-expanded triple, mesh-LOCAL space), placed by the
    // scene walk: Node.Flatten over DefaultScene composes each mesh-bearing node's WorldMatrix, and a node carrying
    // EXT_mesh_gpu_instancing fans one instance per GetWorldMatrix(i) — the node-transform/instance-attr loss the
    // flat fold produced is the deleted form. A mesh no scene node references still lands one identity instance.
    static ImportedGeometry Decoded(InterchangeFormat format, ModelRoot model, Instant at) {
        var meshes = model.LogicalMeshes.Decode();
        var soup = new MeshSoup();
        var blocks = new int[meshes.Count];
        for (int m = 0; m < meshes.Count; m++) { blocks[m] = soup.Append(Block(meshes[m])); }
        var referenced = new bool[meshes.Count];
        foreach (var node in Optional(model.DefaultScene).Map(static s => SharpGLTF.Schema2.Node.Flatten(s)).IfNone([])) {
            if (node.Mesh is not { } mesh) { continue; }
            referenced[mesh.LogicalIndex] = true;
            var gpu = node.GetGpuInstancing();
            if (gpu is { Count: > 0 }) {
                for (int i = 0; i < gpu.Count; i++) { soup.Place(blocks[mesh.LogicalIndex], gpu.GetWorldMatrix(i)); }
            } else { soup.Place(blocks[mesh.LogicalIndex], node.WorldMatrix); }
        }
        for (int m = 0; m < meshes.Count; m++) {
            if (!referenced[m]) { soup.Place(blocks[m], Matrix4x4.Identity); }
        }
        return soup.ToGeometry(format, at);
    }

    static (float[] Vertices, float[] Normals, long[] Corners) Block(IMeshDecoder<Material> mesh) {
        var triangles = mesh.Primitives
            .SelectMany(static prim => prim.TriangleIndices.Select(tri => (prim, tri)))
            .ToSeq();
        int vertexCount = triangles.Count * 3;
        var vertices = new float[vertexCount * 3];
        var normals = new float[vertexCount * 3];
        var corners = new long[vertexCount];
        int slot = 0;
        Span<int> fan = stackalloc int[3];
        foreach (var (prim, (a, b, c)) in triangles) {
            (fan[0], fan[1], fan[2]) = (a, b, c);
            foreach (int corner in fan) {
                var p = prim.GetPosition(corner);
                var n = prim.GetNormal(corner);
                int v = slot * 3;
                (vertices[v], vertices[v + 1], vertices[v + 2]) = (p.X, p.Y, p.Z);
                (normals[v], normals[v + 1], normals[v + 2]) = (n.X, n.Y, n.Z);
                corners[slot] = slot;
                slot++;
            }
        }
        return (vertices, normals, corners);
    }

    // Canonicalize the POOL onto the kernel frame: positions AND normals each ride their own strided call (the one
    // orthogonal signed permutation carries both per format#FORMAT_AXIS — the position-only form left normals in the
    // source frame, the deleted defect), and every instance transform conjugates by the basis (row-vector convention:
    // M' = Bᵀ·M·B) so a placed block lands where its baked copy would have.
    static ImportedGeometry Framed(InterchangeFormat format, ImportedGeometry geometry) {
        if (format.IsCanonicalFrame) {
            return geometry;
        }
        var vertices = MemoryMarshal.AsMemory(geometry.Vertices);
        var normals = MemoryMarshal.AsMemory(geometry.Normals);
        FrameNormalization.Canonicalize(format, vertices.Span, stride: 3);
        FrameNormalization.Canonicalize(format, normals.Span, stride: 3);
        Matrix4x4 basis = Basis(format);
        Matrix4x4 inverse = Matrix4x4.Transpose(basis);
        return geometry with {
            Vertices = vertices,
            Normals = normals,
            Instances = geometry.Instances.Map(i => i with { Transform = inverse * i.Transform * basis }),
        };
    }

    // The row's BasisChange as the row-vector numerics matrix: each ROW is the canonical image of a source axis.
    static Matrix4x4 Basis(InterchangeFormat format) {
        var (xx, xy, xz) = format.Frame.Apply(1f, 0f, 0f);
        var (yx, yy, yz) = format.Frame.Apply(0f, 1f, 0f);
        var (zx, zy, zz) = format.Frame.Apply(0f, 0f, 1f);
        return new Matrix4x4(xx, xy, xz, 0f, yx, yy, yz, 0f, zx, zy, zz, 0f, 0f, 0f, 0f, 1f);
    }

    static class Compression {
        public static bool IsPresent(ReadOnlyMemory<byte> glb) =>
            KhrExtension.MeshoptCompression.Key is var meshopt
            && KhrExtension.DracoMeshCompression.Key is var draco
            && JsonChunk(glb) is { Length: > 0 } json
            && (json.Contains(draco, StringComparison.Ordinal) || json.Contains(meshopt, StringComparison.Ordinal));

        // SharpGLTF.Core drops unrecognized extension JSON (Draco/meshopt have no in-box JsonSerializable
        // extension class), so the extension parameters are read from the raw glTF/GLB JSON tree the parse
        // discards — not from a typed ExtraProperties accessor — and the decode writes back by RE-POINTING the
        // Draco accessors at materialized views (UseBufferView + SetData — a KHR_draco accessor has no bufferView
        // for a typed-array Fill to back) and decoding meshopt INTO the view's own Content region.
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

        // A KHR_draco accessor carries NO bufferView (spec) — the typed-array Fill would read a backing region
        // that does not exist — so the write-back MATERIALIZES each decoded stream into a fresh model view and
        // re-points the accessor through the decompile-verified SetData (never a Fill over AsVector3Array there).
        static void DracoPrimitive(MeshPrimitive primitive, JsonObject extension) {
            int bufferView = (int)extension["bufferView"]!;
            ModelRoot model = primitive.LogicalParent.LogicalParent;
            var decoded = (DracoMesh)Draco.Decode(model.LogicalBufferViews[bufferView].Content.ToArray());
            Repoint(model, primitive.GetVertexAccessor("POSITION"), Vectors(decoded, AttributeType.Position));
            Optional(primitive.GetVertexAccessor("NORMAL"))
                .Filter(_ => decoded.GetNamedAttributeId(AttributeType.Normal) >= 0)
                .Iter(accessor => Repoint(model, accessor, Vectors(decoded, AttributeType.Normal)));
            Accessor indices = primitive.GetIndexAccessor();
            uint[] corners = Corners(decoded).ToArray();
            indices.SetData(
                model.UseBufferView(MemoryMarshal.AsBytes(corners.AsSpan()).ToArray()),
                0, corners.Length, DimensionType.SCALAR, EncodingType.UINT, normalized: false);
        }

        static void Repoint(ModelRoot model, Accessor accessor, IEnumerable<Vector3> values) {
            float[] data = values.SelectMany(static v => new[] { v.X, v.Y, v.Z }).ToArray();
            accessor.SetData(
                model.UseBufferView(MemoryMarshal.AsBytes(data.AsSpan()).ToArray()),
                0, data.Length / 3, DimensionType.VEC3, EncodingType.FLOAT, normalized: false);
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

        // The COMPRESSED slice lives at the EXTENSION's buffer/byteOffset/byteLength — per EXT_meshopt_compression
        // the bufferView's own properties describe the UNCOMPRESSED fallback target, so reading view.Content as the
        // source is the deleted spec inversion; the decode lands IN the view's own count*stride region (the bytes
        // every accessor over this view reads), and a fallback-less view (Content shorter than count*stride under
        // the Skip parse) faults loud through the Boundary funnel rather than decoding into a dead side buffer.
        static unsafe void MeshoptView(ModelRoot model, BufferView view, JsonObject extension) {
            int count = (int)extension["count"]!;
            int stride = (int)extension["byteStride"]!;
            string mode = (string?)extension["mode"] ?? "ATTRIBUTES";
            string filter = (string?)extension["filter"] ?? "NONE";
            var compressed = model.LogicalBuffers[(int)extension["buffer"]!].Content;
            int offset = (int?)extension["byteOffset"] ?? 0;
            int length = (int)extension["byteLength"]!;
            var destination = new byte[count * stride];
            fixed (byte* dst = destination)
            fixed (byte* src = compressed) {
                byte* origin = src + offset;
                // mode and filter are CLOSED spec vocabularies — an unrecognized token is a malformed extension
                // faulting LOUD through the Boundary funnel, never a silent wrong-lane decode.
                int status = mode switch {
                    "TRIANGLES" or "INDICES" => Meshopt.DecodeIndexBuffer(dst, (nuint)count, (nuint)stride, origin, (nuint)length),
                    "ATTRIBUTES" => Meshopt.DecodeVertexBuffer(dst, (nuint)count, (nuint)stride, origin, (nuint)length),
                    var unknown => throw new InvalidDataException($"<meshopt-mode:{unknown}>"),
                };
                if (status != 0) { throw new InvalidDataException($"<meshopt-decode-status:{status}>"); }
                Filter(filter)(dst, (nuint)count, (nuint)stride);
            }
            destination.CopyTo(view.Content.AsSpan(0, destination.Length));
        }

        static unsafe delegate*<void*, nuint, nuint, void> Filter(string filter) => filter switch {
            "OCTAHEDRAL" => &Meshopt.DecodeFilterOct,
            "QUATERNION" => &Meshopt.DecodeFilterQuat,
            "EXPONENTIAL" => &Meshopt.DecodeFilterExp,
            "COLOR" => &Meshopt.DecodeFilterColor,
            "NONE" => &Identity,
            var unknown => throw new InvalidDataException($"<meshopt-filter:{unknown}>"),
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
    // The private kernel behind BimIo.ImportIfc — the ONE bytes->DatabaseIfc decode in the package.
    // wire#WIRE_PROJECTION Admit and export#ROUNDTRIP Verify COMPOSE ImportIfc (each re-wrapping its own
    // admission prefix); a second private ParseString/ReadJSON/ReadXMLDoc triple is the deleted form.
    // The schema is sniffed off the bytes BEFORE the database is constructed [H8]: ImportIfc binds the RAILED
    // `Fin<GGRelease> SemanticProjector.Sniff(bytes, format, key)` — the ONE schema-sniff owner (STEP FILE_SCHEMA /
    // ifcJSON schema_identifier / ifcXML xmlns), CodecReject `schema-header-missing`/`schema-header-unmapped` typed
    // OUTSIDE the ModelRejected boundary funnel, the silent IFC4X3_ADD2 default deleted — then this kernel constructs
    // at that schema, so a 2x3 file admits as 2x3. The serialization is the format#FORMAT_AXIS Serialization column
    // (Some on every GeometryGym row the ImportIfc codec gate admits) — the third format== ladder beside the retired
    // export/wire pair was the deleted form.
    static DatabaseIfc Database(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ReleaseVersion schema) =>
        format.Serialization.IfNoneUnsafe(() => throw new InvalidDataException($"<ifc-serialization-miss:{format.Key}>")) switch {
            FormatIfcSerialization.JSON => JsonDatabase(bytes, schema),
            FormatIfcSerialization.XML  => XmlDatabase(bytes, schema),
            _                           => DatabaseIfc.ParseString(Encoding.UTF8.GetString(bytes.Span)),
        };

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
        return new MeshSoup().Baked(vertices, normals, indices).ToGeometry(format, at);
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
    // JoinIdenticalVertices | GenerateSmoothNormals); each scene mesh lands ONE pool block, and the RootNode walk
    // places it per mesh-bearing node with the composed node transform (the world matrix is the product up the
    // Parent chain per api-assimpnetter) — the flat scene.Meshes fold that dropped every node placement is the
    // deleted form. Handedness rides the per-importer FrameNormalization the row carries, not MakeLeftHanded.
    static ImportedGeometry Scene(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at) {
        using var context = new AssimpContext();
        using var stream = new MemoryStream(bytes.ToArray());
        // The READ hint is the row's file EXTENSION (assimp importer selection keys on extension: "dae", not the
        // row key "collada"); the row KEY stays the EXPORT formatId ExportToBlob dispatches on — two foreign
        // contracts, never conflated on one value.
        var scene = context.ImportFileFromStream(stream,
            PostProcessSteps.Triangulate | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.GenerateSmoothNormals,
            format.Extensions.Head.Map(static ext => ext.TrimStart('.')).IfNone(format.Key));
        var soup = new MeshSoup();
        var blocks = new int[scene.MeshCount];
        var referenced = new bool[scene.MeshCount];
        for (int m = 0; m < scene.MeshCount; m++) { blocks[m] = soup.Append(AssimpBlock(scene.Meshes[m])); }
        Walk(scene.RootNode, Matrix4x4.Identity);
        for (int m = 0; m < scene.MeshCount; m++) {
            if (!referenced[m]) { soup.Place(blocks[m], Matrix4x4.Identity); }
        }
        return soup.ToGeometry(format, at);

        // Row-vector composition: world = local × parentWorld (Assimp's column-vector chain, transposed once at Numeric).
        void Walk(Assimp.Node node, Matrix4x4 parent) {
            Matrix4x4 world = Numeric(node.Transform) * parent;
            foreach (int m in node.MeshIndices) { referenced[m] = true; soup.Place(blocks[m], world); }
            foreach (var child in node.Children) { Walk(child, world); }
        }
    }

    static (float[] Vertices, float[] Normals, long[] Corners) AssimpBlock(Assimp.Mesh mesh) {
        var vertices = new float[mesh.VertexCount * 3];
        var normals = new float[mesh.VertexCount * 3];
        for (int i = 0; i < mesh.VertexCount; i++) {
            var p = mesh.Vertices[i];
            int v = i * 3;
            (vertices[v], vertices[v + 1], vertices[v + 2]) = (p.X, p.Y, p.Z);
            (normals[v], normals[v + 1], normals[v + 2]) = mesh.HasNormals
                ? (mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z)
                : (0f, 0f, 1f);
        }
        var corners = mesh.Faces
            .SelectMany(static face => Enumerable.Range(1, face.IndexCount - 2)
                .SelectMany(k => new long[] { face.Indices[0], face.Indices[k], face.Indices[k + 1] }))
            .ToArray();
        return (vertices, normals, corners);
    }

    // Assimp matrices are column-vector convention; the numerics row-vector equivalent is the transpose.
    static Matrix4x4 Numeric(Assimp.Matrix4x4 m) => new(
        m.A1, m.B1, m.C1, m.D1, m.A2, m.B2, m.C2, m.D2, m.A3, m.B3, m.C3, m.D3, m.A4, m.B4, m.C4, m.D4);

    // USD decode through UniversalSceneDescription — the `usd-stage` codec. One UsdStage opens the layer
    // stack (the native plugin tree reads the temp path), Traverse walks the namespace, each UsdGeomMesh
    // prim's points/normals (VtVec3fArray of GfVec3f) and face-vertex counts/indices (VtIntArray) cross the
    // typed-array mesh-bridge seam onto ONE pool block, and the prim PLACES its block by the composed
    // local-to-world transform off ONE UsdGeomXformCache — the identity-placed decode that baked every prim
    // and erased USD's native instancing/placement is the deleted form. USD is a scene-graph peer — the BIM
    // semantics stay the GeometryGym IFC graph's, never re-derived from USD prim type names.
    // The frame is PER-STAGE: upAxis is stage metadata (UsdGeomGetStageUpAxis, decompile-verified — TfToken
    // "Y" the USD default, "Z" the common CAD/BIM export), so a Z-up stage is ALREADY canonical and skips the
    // row's Y-up Frame; the format row keeps the static Y-up default every metadata-less stage falls to.
    static ImportedGeometry Usd(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at) {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{format.Extensions.Head.IfNone(".usd")}");
        File.WriteAllBytes(path, bytes.ToArray());
        try {
            using var stage = UsdStage.Open(path, UsdStage.InitialLoadSet.LoadAll);
            bool zUp = UsdGeom.UsdGeomGetStageUpAxis(stage).ToString() == "Z";
            var soup = new MeshSoup();
            var xform = new UsdGeomXformCache();
            stage.Traverse().AsIterable()
                .Filter(static prim => prim.GetTypeName().ToString() == "Mesh")
                .Iter(prim => {
                    int block = soup.Append(UsdMesh(new UsdGeomMesh(prim)));
                    soup.Place(block, Numeric(xform.GetLocalToWorldTransform(prim)));
                });
            var geometry = soup.ToGeometry(format, at);
            return zUp ? geometry : Framed(format, geometry);
        } finally { File.Delete(path); }
    }

    // GfMatrix4d is row-major double over ROW vectors — the numerics convention — so the narrow is
    // per-component, never a transpose (the Assimp column-vector overload transposes; this one must not).
    static Matrix4x4 Numeric(GfMatrix4d m) {
        var (a, b, c, d) = (m.GetRow(0), m.GetRow(1), m.GetRow(2), m.GetRow(3));
        return new Matrix4x4(
            (float)a[0], (float)a[1], (float)a[2], (float)a[3], (float)b[0], (float)b[1], (float)b[2], (float)b[3],
            (float)c[0], (float)c[1], (float)c[2], (float)c[3], (float)d[0], (float)d[1], (float)d[2], (float)d[3]);
    }

    // The typed-array mesh-bridge: GetPointsAttr/GetNormalsAttr/GetFaceVertexCountsAttr/GetFaceVertexIndicesAttr
    // each fill a VtValue the typed Vt*Array reads (size()/indexer), per the api-usd mesh-bridge seam; authored
    // normals ride when their count matches the points (faceVarying/uniform normals re-index at the admission
    // gate, the up-normal the absent-case fill); faces fan-triangulate into PRE-SIZED buffers (the fan size is
    // Σ(n-2) over the counts array, computed in one first pass).
    static (float[] Points, float[] Normals, long[] Indices) UsdMesh(UsdGeomMesh mesh) {
        var (points, authored, counts, corners) = (new VtValue(), new VtValue(), new VtValue(), new VtValue());
        mesh.GetPointsAttr().Get(points, UsdTimeCode.Default());
        bool hasNormals = mesh.GetNormalsAttr().Get(authored, UsdTimeCode.Default());
        mesh.GetFaceVertexCountsAttr().Get(counts, UsdTimeCode.Default());
        mesh.GetFaceVertexIndicesAttr().Get(corners, UsdTimeCode.Default());
        var (xyz, faceCounts, faceIndices) = ((VtVec3fArray)points, (VtIntArray)counts, (VtIntArray)corners);
        var perVertex = hasNormals && (VtVec3fArray)authored is { } nrm && (int)nrm.size() == (int)xyz.size() ? nrm : null;
        var verts = new float[(int)xyz.size() * 3];
        var normals = new float[verts.Length];
        for (int i = 0; i < (int)xyz.size(); i++) {
            var p = xyz[i];
            (verts[i * 3], verts[i * 3 + 1], verts[i * 3 + 2]) = (p[0], p[1], p[2]);
            (normals[i * 3], normals[i * 3 + 1], normals[i * 3 + 2]) = perVertex is { } n
                ? (n[i][0], n[i][1], n[i][2])
                : (0f, 0f, 1f);
        }
        int fans = 0;
        for (int f = 0; f < (int)faceCounts.size(); f++) { fans += Math.Max(0, faceCounts[f] - 2); }
        var tris = new long[fans * 3];
        var (cursor, slot) = (0, 0);
        for (int f = 0; f < (int)faceCounts.size(); f++) {
            int n = faceCounts[f];
            for (int k = 1; k < n - 1; k++) {
                (tris[slot], tris[slot + 1], tris[slot + 2]) = (faceIndices[cursor], faceIndices[cursor + k], faceIndices[cursor + k + 1]);
                slot += 3;
            }
            cursor += n;
        }
        return (verts, normals, tris);
    }

    // The shared mesh-pool builder every non-glTF decode arm folds into — a SINGLE-USE pooled boundary kernel:
    // the three growth buffers rent through ArrayPoolBufferWriter<T> (BCL IBufferWriter GetSpan/Advance staging,
    // pooled doubling, the admitted CommunityToolkit.HighPerformance owner) replacing both the rejected per-block
    // Seq concatenation (O(blocks·total)) and the List<T> LOH churn. Append lands one block (vertices/normals as
    // flat triples, 0-based corners offset into the pool) and returns its ordinal; Place records one rigid
    // placement; Baked is the identity-placed block the non-instanced arms use. ToGeometry materializes the ONE
    // contiguous ImportedGeometry allocation carrying the Blocks/Instances overlay AND returns the rents — the
    // builder is dead after it.
    sealed class MeshSoup {
        readonly ArrayPoolBufferWriter<float> vertices = new();
        readonly ArrayPoolBufferWriter<float> normals = new();
        readonly ArrayPoolBufferWriter<long> indices = new();
        readonly List<MeshBlock> blocks = [];
        readonly List<MeshInstance> instances = [];

        public int VertexCount { get; private set; }

        public int Append((float[] Vertices, float[] Normals, long[] Corners) block) =>
            Append(block.Vertices, block.Normals, block.Corners);

        public int Append(ReadOnlySpan<float> v, ReadOnlySpan<float> n, ReadOnlySpan<long> corners) {
            blocks.Add(new MeshBlock(VertexCount, v.Length / 3, indices.WrittenCount, corners.Length));
            v.CopyTo(vertices.GetSpan(v.Length));
            vertices.Advance(v.Length);
            n.CopyTo(normals.GetSpan(n.Length));
            normals.Advance(n.Length);
            Span<long> offset = indices.GetSpan(corners.Length);
            for (int c = 0; c < corners.Length; c++) { offset[c] = VertexCount + corners[c]; }
            indices.Advance(corners.Length);
            VertexCount += v.Length / 3;
            return blocks.Count - 1;
        }

        public MeshSoup Place(int block, Matrix4x4 transform) {
            instances.Add(new MeshInstance(block, transform));
            return this;
        }

        public MeshSoup Baked(ReadOnlySpan<float> v, ReadOnlySpan<float> n, ReadOnlySpan<long> corners) =>
            Place(Append(v, n, corners), Matrix4x4.Identity);

        public ImportedGeometry ToGeometry(InterchangeFormat format, Instant at) {
            var geometry = new ImportedGeometry(format,
                vertices.WrittenSpan.ToArray(), normals.WrittenSpan.ToArray(), indices.WrittenSpan.ToArray(),
                VertexCount, indices.WrittenCount / 3, blocks.ToSeq(), instances.ToSeq(), at);
            vertices.Dispose();
            normals.Dispose();
            indices.Dispose();
            return geometry;
        }
    }

    // Managed `.bim` decode through dotbim — the `dotbim` codec: the wire is pure System.Text.Json (every member
    // carries a snake_case [JsonPropertyName]), so the byte admission deserializes dotbim.File directly (File.Read
    // is the path-bound package form). Each pooled dotbim.Mesh lands ONE block (flat XYZ triples + triangle corners,
    // up-normal filled — the format carries none) and each Element places its block by the Vector translation +
    // quaternion Rotation, so an N-element model imports N instances over one shared block, never N baked copies;
    // the Guid/Type/Info semantics ride the DotbimProjector, never this geometry fold.
    static ImportedGeometry DotBim(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at) {
        var file = JsonSerializer.Deserialize<dotbim.File>(bytes.Span)
            ?? throw new InvalidDataException("<dotbim-empty-document>");
        var soup = new MeshSoup();
        var pool = file.Meshes.AsIterable().Fold(Map<int, int>(), (map, mesh) => {
            var verts = mesh.Coordinates.Select(static c => (float)c).ToArray();
            var corners = mesh.Indices.Select(static i => (long)i).ToArray();
            var up = new float[verts.Length];
            for (int i = 2; i < up.Length; i += 3) { up[i] = 1f; }
            return map.Add(mesh.MeshId, soup.Append(verts, up, corners));
        });
        foreach (var element in file.Elements) {
            int block = pool.Find(element.MeshId)
                .IfNone(() => throw new InvalidDataException($"<dotbim-mesh-miss:{element.MeshId}>"));
            var q = new System.Numerics.Quaternion(
                (float)element.Rotation.Qx, (float)element.Rotation.Qy, (float)element.Rotation.Qz, (float)element.Rotation.Qw);
            soup.Place(block, Matrix4x4.CreateFromQuaternion(q)
                * Matrix4x4.CreateTranslation((float)element.Vector.X, (float)element.Vector.Y, (float)element.Vector.Z));
        }
        return soup.ToGeometry(format, at);
    }

    // Managed in-process DWG/DXF decode through ACadSharp — the `acad-sharp` codec the format#FORMAT_AXIS
    // Dwg row carries. The DXF/CadDocument is the same decompile-verified reader Fabrication consumes for
    // 2D profiles; here the Bim arm folds the mesh-bearing entities onto the canonical triangle-soup.
    static class AcadReader {
        public static ImportedGeometry Read(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at) {
            using var stream = new MemoryStream(bytes.ToArray());
            var document = IsDxf(bytes) ? DxfReader.Read(stream) : DwgReader.Read(stream);
            var soup = document.Entities.AsIterable().Fold(new MeshSoup(), Accumulate);
            return soup.ToGeometry(format, at);

            // An Insert flattens through the package-owned Explode() — the OCS->WCS placement, Rotation, per-axis
            // scale, OCS Normal, AND the MINSERT array replication ACadSharp owns — each placed entity folded back
            // through the same classifier so a block-nested Insert recurses (Explode BAKES the placement, so every
            // block lands identity-placed). The deleted form hand-rolled an InsertPoint/XScale matrix the
            // api-acadsharp RAIL_LAW rejects: it dropped Rotation, the OCS Normal, every MINSERT instance, and every
            // block-nested Mesh/PolyfaceMesh (it walked the block's Face3D entities only).
            static MeshSoup Accumulate(MeshSoup soup, Entity entity) => entity switch {
                Mesh mesh         => Baked(soup, Faces(mesh.Vertices, mesh.Faces)),
                Face3D face       => Baked(soup, Quad(face.FirstCorner, face.SecondCorner, face.ThirdCorner, face.FourthCorner)),
                PolyfaceMesh poly => Baked(soup, Polyface(poly)),
                Insert insert     => insert.Explode().AsIterable().Fold(soup, Accumulate),
                _                 => soup,
            };

            static MeshSoup Baked(MeshSoup soup, (float[] Vertices, float[] Normals, long[] Corners) block) =>
                soup.Baked(block.Vertices, block.Normals, block.Corners);
        }

        // DXF (ascii/binary) opens with "0\nSECTION" / "AutoCAD Binary DXF"; DWG with "AC10xx" — the one sniff the
        // package leaves to the caller (CadReaderFactory.GetFileFormat is filename-only and the shared Dwg row carries
        // both extensions over a byte stream), so the reader pick is a boundary kernel, never a hand-rolled DXF parse.
        static bool IsDxf(ReadOnlyMemory<byte> bytes) =>
            bytes.Length >= 4 && !(bytes.Span[0] == (byte)'A' && bytes.Span[1] == (byte)'C' && char.IsDigit((char)bytes.Span[2]));

        // A POLYLINE/AcDbPolyFaceMesh: the VertexFaceMesh vertex pool plus the 1-based signed VertexFaceRecord index
        // records (a negative index marks a hidden edge -> abs, a zero Index4 marks a triangle), fan-triangulated to
        // a 0-based block the shared MeshSoup offsets into the pool.
        static (float[] Vertices, float[] Normals, long[] Corners) Polyface(PolyfaceMesh poly) {
            var pool = poly.Vertices.Select(static v => v.Location).ToList();
            var corners = poly.Faces.SelectMany(static f => {
                long a = Math.Abs(f.Index1) - 1, b = Math.Abs(f.Index2) - 1, c = Math.Abs(f.Index3) - 1;
                return f.Index4 == 0 ? new[] { a, b, c } : new[] { a, b, c, a, c, (long)Math.Abs(f.Index4) - 1 };
            }).ToArray();
            var (verts, normals) = Triples(pool);
            return (verts, normals, corners);
        }

        // A SubDMesh: the vertex list plus the n-gon face index list (each face fan-triangulated), as a 0-based
        // triangle-soup block the shared MeshSoup offsets into the pool.
        static (float[] Vertices, float[] Normals, long[] Corners) Faces(
            System.Collections.Generic.IReadOnlyList<XYZ> vertices, System.Collections.Generic.IReadOnlyList<int[]> faces) {
            var corners = faces.SelectMany(face => Enumerable.Range(1, face.Length - 2)
                .SelectMany(k => new long[] { face[0], face[k], face[k + 1] })).ToArray();
            var (verts, normals) = Triples(vertices);
            return (verts, normals, corners);
        }

        // A 3DFACE quad (the fourth corner equals the third for a triangle) fan-triangulated to a 0-based block.
        static (float[] Vertices, float[] Normals, long[] Corners) Quad(XYZ a, XYZ b, XYZ c, XYZ d) {
            bool tri = d.Equals(c);
            var pool = tri ? new[] { a, b, c } : new[] { a, b, c, d };
            var corners = (tri ? new long[] { 0, 1, 2 } : new long[] { 0, 1, 2, 0, 2, 3 });
            var (verts, normals) = Triples(pool);
            return (verts, normals, corners);
        }

        static (float[] Vertices, float[] Normals) Triples(System.Collections.Generic.IReadOnlyList<XYZ> pool) {
            var verts = new float[pool.Count * 3];
            var normals = new float[pool.Count * 3];
            for (int i = 0; i < pool.Count; i++) {
                var p = pool[i];
                (verts[i * 3], verts[i * 3 + 1], verts[i * 3 + 2]) = ((float)p.X, (float)p.Y, (float)p.Z);
                normals[i * 3 + 2] = 1f;
            }
            return (verts, normals);
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
            // SHAPE_DEFINITION_REPRESENTATION(#definition, #representation) is the definition<->representation
            // join, so a GeometryRef row carries a REAL owning-definition link the companion routes by — arg 0 of
            // a representation entity is its NAME string, so the RefAt(args, 0) read yielded the always-zero
            // decorative column (the deleted illusion).
            var shapeDefinition = instances
                .Filter(static i => i.Keyword is "SHAPE_DEFINITION_REPRESENTATION")
                .Fold(Map<long, long>(), static (held, i) => held.TryAdd(RefAt(i.Args, 1), RefAt(i.Args, 0)));
            return new StepSemanticModel(
                format.StepProtocol, HeaderText(header, "FILE_SCHEMA", 0), HeaderText(header, "FILE_NAME", 5),
                instances.Filter(static i => i.Keyword is "PRODUCT")
                    .Map(static i => new StepSemanticModel.ProductRow(i.Id, Str(i.Args, 0), Str(i.Args, 1), Str(i.Args, 2))).ToSeq(),
                instances.Filter(static i => i.Keyword is "PRODUCT_DEFINITION")
                    .Map(i => Definition(i, graph)).ToSeq(),
                instances.Filter(static i => i.Keyword is "NEXT_ASSEMBLY_USAGE_OCCURRENCE")
                    .Map(i => Assembly(i, graph)).ToSeq(),
                instances.Filter(static i => GeometryTypes.Contains(i.Keyword))
                    .Map(i => new StepSemanticModel.GeometryRef(i.Id, i.Keyword, shapeDefinition.Find(i.Id).IfNone(0L))).ToSeq(),
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

        // The ISO 10303-21 header read at its POSITIONAL grammar: FILE_SCHEMA(('<schema>')) ordinal 0 (a nested
        // list — its first text wins); FILE_NAME(name, stamp, (author), (org), preprocessor, originating_system,
        // authorization) ordinal 5 — the first-text-wins scan returned the file NAME under an "Originating" label,
        // the deleted mislabel.
        static string HeaderText(string header, string keyword, int ordinal) =>
            header.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) is var head and >= 0
            && header.IndexOf('(', head) is var open and >= 0
                ? ArgAt(ParseList(header[open..], out _).Items, ordinal).Match(
                    Some: static a => a switch {
                        Arg.Text t => t.Value,
                        Arg.List { Items: var nested } => nested.Choose(static n => n is Arg.Text nt ? Optional(nt.Value) : None).Head.IfNone(""),
                        _ => "",
                    },
                    None: static () => "")
                : "";
    }
}

// The dotbim arm of the seam — the IElementProjection peer of the IFC SemanticProjector and the SpeckleProjector.
// It captures one already-deserialized dotbim.File internally and lowers each placed Element onto a rooted seam
// Node.Object carrying the neutral Classification("dotbim", Type), the element Guid as the 1:1 ExternalId (the
// re-ingest reconcile key Reimport matches on), and its string->string Info bag as one content-keyed PropertySet
// bound by an Assign.PropertyDefinition edge — the api-dotbim "Info keys re-bind to the canonical element" law.
// The display geometry rides the separate ImportGeometry dotbim arm (the instancing-preserving pool), so the
// semantic node references no representation; dotbim.* never crosses past this capsule.
public sealed class DotbimProjector(dotbim.File file) : IElementProjection {
    public Fin<GraphDelta> Project(ProjectionContext ctx) => Fin.Succ(
        file.Elements.AsIterable().Fold(GraphDelta.Empty.Reheader(ctx.Header), (delta, element) => {
            NodeId id = NodeId.Rooted();
            Node.PropertySet bag = InfoBag(element, ctx.Header.Tolerance);
            return delta
                .Put(new Node.Object(
                    Id:              id,
                    Kind:            ObjectKind.Occurrence,
                    ExternalId:      Some(External(element)),
                    Classification:  Classification.Create("dotbim", element.Type, "", None, None, None),
                    PredefinedType:  PredefinedType.NotDefined,
                    Name:            element.Type,
                    Tag:             element.MeshId.ToString(CultureInfo.InvariantCulture),
                    Representations: RepresentationContentHash.Empty,
                    History:         Option<OwnerHistory>.None,
                    Span:            SchemaSpan.From(ctx.Header.Schema)))
                .Put(bag)
                .Link(new Relationship.Assign(id, bag.Id, AssignKind.PropertyDefinition));
        }));

    // The re-ingest key: a Rasm-exported .bim writes the verbatim seam GlobalId to Info["globalId"] (the element
    // Guid is XxHash128-derived from it — export#EXPORT_RAIL), so the round-trip prefers it and a foreign .bim
    // falls back to the element Guid — either way Reimport reconciles on a stable ExternalId.
    static string External(dotbim.Element element) =>
        element.Info is { } info && info.TryGetValue("globalId", out string? globalId) && globalId.Length > 0
            ? globalId
            : element.Guid;

    // The Info bag -> one CONTENT-KEYED PropertySet node (identical bags dedup); OccurrenceWins because a dotbim
    // element carries no type-driven inheritance, PropertySource.Import because the rows arrive on the wire; the
    // wire's whole-object Color lands as ONE #RRGGBBAA row so the export counterpart re-packs ElementInstance.Rgba
    // and the round-trip preserves it (FaceColors, the per-face override stream, is the one growth row).
    static Node.PropertySet InfoBag(dotbim.Element element, double tolerance) {
        var values = toMap((element.Info ?? new Dictionary<string, string>()).Select(static pair =>
                (PropertyName.Create(pair.Key), (PropertyValue)new PropertyValue.Text(pair.Value))))
            .AddOrUpdate(
                PropertyName.Create("color"),
                new PropertyValue.Text($"#{element.Color.R:X2}{element.Color.G:X2}{element.Color.B:X2}{element.Color.A:X2}"));
        var seed = new Node.PropertySet(NodeId.Content([]),
            new PropertyBag("Pset_Dotbim", values, InheritanceMode.OccurrenceWins, PropertySource.Import));
        return seed with { Id = NodeId.Content(seed.ToCanonicalBytes(tolerance).Span) };
    }
}
```

## [03]-[SPECKLE_SEAM]

- Owner: `BimIo.ImportSpeckle` the Speckle display-mesh arm of the import fold (a deserialized `Speckle.Sdk.Models.Base` tree → `ImportedGeometry`), and `SpeckleProjector : IElementProjection` the Speckle host-object arm of the SEAM (the same `Base` tree → a seam `GraphDelta`, the peer of the IFC `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector`); there is no `SpeckleImporter`/`SpeckleConverter` type and no parallel decode family — the geometry arm is a third entrypoint on the existing `BimIo` capsule symmetric to `ImportGeometry`/`ImportIfc`, the projector an `IElementProjection` the app registers in its `Seq<IElementProjection>`, both consuming the receive-side `Base` the Persistence `csharp:Persistence/Version/ledger#SYNC_TRANSPORTS` `IOperations.Receive` returns.
- Entry: `BimIo.ImportSpeckle(Base root, IClock clock, Op key)` projecting the display-mesh geometry to `ImportedGeometry`, and `new SpeckleProjector(root).Project(ProjectionContext ctx)` lowering the host-object graph to a seam `GraphDelta` — the geometry `Fin<T>` aborts on a graph with no displayable geometry or a malformed display mesh, projecting the Speckle exception onto `BimFault.ModelRejected(key, error.Message)` BARE at the boundary (band 2600 IS the `Expected` `Code` — no `.ToError()` hop) so domain code never sees a `Speckle.Sdk.SpeckleException`, while the projector's foreign fault funnels to `ElementFault.ProjectionFailed` at the caller's capture boundary — the `ProjectionAssembly.Assemble` `Try.lift` funnel (the seam idiom; a kernel `Op.Catch` would erase the typed arm into `Fault.InvalidResult`) or the Bim-internal `BimIo.Reimport` `key.Catch`; the `Base` arrives already deserialized, so the seam mints no transport, no `IOperations` reference, and no second graph walk beyond the package-owned traversal.
- Auto: the geometry fold runs the package-owned `BaseExtensions.Flatten(Base, BaseExtensions.BaseRecursionBreaker?)` deduplicating graph walk, projects each node's `BaseExtensions.TryGetDisplayValue(Base)` display list to its `Mesh` members, and decodes each `Mesh` — the flat `vertices`/`vertexNormals` (`List<double>`, flat `x,y,z`) and length-prefixed `faces` (`List<int>`, each face `[n, i0, … i(n-1)]`) triangulate through a fan over the n-gon, scaled onto the canonical metre frame by `Units.GetConversionFactor(mesh.units, Units.Meters)` so a millimetre or foot Speckle model lands in kernel units; a node that `IsDisplayableObject` is false yet carries non-mesh geometry (`Brep`/`Surface`/`Curve` with no `displayValue`) routes its content to `tessellation#TESSELLATION_BRIDGE` over the GLB rail rather than evaluating a BRep in-process; the `SpeckleProjector` fold lowers every `DataObject` (and its `RhinoObject`/`RevitObject`/`ArchicadObject`/`TeklaObject`/`Civil3dObject`/`AutocadObject` host-object subtypes) onto a rooted seam `Node.Object` carrying the generic `Classification("speckle", speckle_type)` and the host `applicationId` as the 1:1 `ExternalId`, its `DataObject.properties` (`Dictionary<string, object?>`) into one content-keyed `PropertySet` bag node attached by an `Assign.PropertyDefinition` edge, and the `BaseExtensions.TraverseWithPath` path prefixes reconstructing the namespace containment as `Compose.Contain` edges — the containment the retired flat-row projection claimed in prose but produced empty.
- Receipt: the `ModelLoad` receipt case carries the format key `InterchangeFormat.Glb.Key` proxy for the decoded scene, the codec key `speckle-base`, the `Base.GetTotalChildrenCount()` source object count, and elapsed; the `SpeckleProjector` contributes the host-object `GraphDelta` (its `NodeCount`/`EdgeCount` the change magnitude, the distinct `speckle_type` discriminants the seam `Classification` codes); emission rides the sink port at the composition edge.
- Packages: Speckle.Sdk, Speckle.Objects, SharpGLTF.Core, Rasm.Element, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, Rasm
- Growth: a new Speckle geometry leaf is one arm on the `DisplayMeshes` projection keyed on the `IDisplayValue<T>` payload type; a new host-object discriminant is one `Classification("speckle", speckle_type)` code on the `SpeckleProjector`'s `Node.Object`, never a parallel row family; a non-mesh evaluation never grows a managed Speckle tessellator — it widens the `tessellation#TESSELLATION_BRIDGE` request, never this fold.
- Boundary: `BimIo` is the page boundary capsule and the Speckle arm carries the language-owned statement forms the foreign graph walk requires; the `Base` graph is admitted exactly once — `Flatten` is the single package-owned deduplicating traversal (it caches on `Base.id`), so the seam never re-walks the tree or hand-rolls a `DynamicBase.GetMembers` recursion, and `TryGetDisplayValue`/`IsDisplayableObject` own the displayable-node vocabulary rather than a per-type `is Mesh`/`is Brep` ladder; the Speckle `Mesh.faces` length-prefixed n-gon encoding fans into the canonical triangle-soup `ImportedGeometry` at the boundary (one contiguous vertex/normal/index triple, the allocation point, never a per-face `double[]` proliferation), and a degenerate face (`n < 3`) faults the decode; non-mesh geometry never evaluates in-process — GeometryGym carries no Speckle BRep kernel and the managed branch owns no NURBS evaluator, so a `Brep`/`Surface`/`Curve` with no `displayValue` rides the companion GLB rail exactly as the IFC geometry request does, joining the same content-keyed artifact; `Speckle.Sdk`/`Speckle.Objects` are the OUTSIDE-RHINO concern (`Speckle.Sdk.Dependencies` repacks the SDK's Polly/channel/serialisation-V2 closure), so this arm composes them only in the host-neutral `Rasm.Bim` exchange assembly and the in-Rhino plugin assembly never loads them; the host-object semantic projection is the `SpeckleProjector : IElementProjection` lowering to a seam `GraphDelta` (the generic `Classification("speckle", speckle_type)`, never an IFC class), so a `SpeckleImporter`/`SpeckleConverter` service family, a hand-rolled `Base`-graph recursion, a lossy `IfcSemanticModel` host-object re-projection, and a managed Speckle tessellator are the deleted forms.

```csharp signature
public static partial class BimIo {
    public static Fin<ImportedGeometry> ImportSpeckle(Base root, IClock clock, Op key) =>
        Boundary(key, () => DisplayScene(root, clock.GetCurrentInstant()))
            .Bind(scene => scene.TriangleCount > 0
                ? Fin.Succ(scene)
                : Fin.Fail<ImportedGeometry>(new BimFault.ModelRejected(key, $"speckle-no-display:{root.speckle_type}")));

    static ImportedGeometry DisplayScene(Base root, Instant at) {
        var soup = new MeshSoup();
        root.Flatten()
            .SelectMany(static node => node.TryGetDisplayValue()?.OfType<Mesh>() ?? Enumerable.Empty<Mesh>())
            .Iter(mesh => { var (v, n, c) = SpeckleBlock(mesh); soup.Baked(v, n, c); });
        return soup.ToGeometry(InterchangeFormat.Glb, at);
    }

    // The Speckle Mesh -> UNWELDED triangle-soup block the shared MeshSoup folds: each length-prefixed n-gon fans
    // to triangles, each fan corner expands to its own vertex (Speckle faces index the shared vertex list, the seam
    // unwelds), the vertexNormals sampled when present else an up-normal, scaled onto the canonical metre frame by
    // the source unit — PRE-SIZED buffers over the fan count (a Speckle display mesh is world-space, so the block
    // lands identity-placed).
    static (float[] Vertices, float[] Normals, long[] Corners) SpeckleBlock(Mesh mesh) {
        double scale = Units.GetConversionFactor(mesh.units, Units.Meters);
        var fans = Triangulate(mesh.faces).ToArray();
        bool hasNormals = mesh.vertexNormals.Count == mesh.vertices.Count;
        var vertices = new float[fans.Length * 3];
        var normals = new float[fans.Length * 3];
        var corners = new long[fans.Length];
        for (int i = 0; i < fans.Length; i++) {
            int vertex = fans[i], slot = i * 3;
            (vertices[slot], vertices[slot + 1], vertices[slot + 2]) = (
                (float)(mesh.vertices[vertex * 3] * scale),
                (float)(mesh.vertices[vertex * 3 + 1] * scale),
                (float)(mesh.vertices[vertex * 3 + 2] * scale));
            (normals[slot], normals[slot + 1], normals[slot + 2]) = hasNormals
                ? ((float)mesh.vertexNormals[vertex * 3], (float)mesh.vertexNormals[vertex * 3 + 1], (float)mesh.vertexNormals[vertex * 3 + 2])
                : (0f, 0f, 1f);
            corners[i] = i;
        }
        return (vertices, normals, corners);
    }

    static IEnumerable<int> Triangulate(List<int> faces) {
        for (int cursor = 0; cursor < faces.Count;) {
            // Legacy Speckle face heads encode 0 = triangle and 1 = quad; a modern head IS the n-gon vertex count,
            // so the remap widens decode with zero ambiguity (no valid modern face carries n < 3).
            int span = faces[cursor] switch { 0 => 3, 1 => 4, var n => n };
            if (span < 3) { throw new InvalidDataException($"<speckle-degenerate-face:{span}>"); }
            for (int corner = 1; corner + 1 < span; corner++) {
                yield return faces[cursor + 1];
                yield return faces[cursor + 1 + corner];
                yield return faces[cursor + 2 + corner];
            }
            cursor += span + 1;
        }
    }
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
        Classification:  Classification.Create("speckle", data.speckle_type, "", None, None, None),
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
        var seed = new Node.PropertySet(NodeId.Rooted(), new PropertyBag(data.speckle_type, values, InheritanceMode.OccurrenceWins, PropertySource.Import));
        return seed with { Id = NodeId.Content(seed.ToCanonicalBytes(tolerance).Span) };
    }

    // Compose.Contain edges from the namespace nesting: a host's parent is the host with the longest path that is a
    // strict prefix of its own (the nearest enclosing DataObject), so the Speckle containment tree the flat-row
    // projection dropped rides the neutral Compose edge a Bake fold descends; a root host (no DataObject ancestor)
    // adds none. The child probes its OWN strict prefixes longest-first against a path-keyed index — O(hosts·depth);
    // the all-pairs prefix scan was the O(hosts²) form the Reconcile edge diff already rejects. Duplicate paths
    // coalesce last-wins: an ambiguous parent is one parent, never a thrown build.
    static Seq<Relationship> Containment(Seq<(string[] Path, DataObject Data, NodeId Id)> hosts) {
        var byPath = hosts.Fold(Map<string, NodeId>(), static (held, host) => held.AddOrUpdate(string.Join('\u0000', host.Path), host.Id));
        return hosts.Choose(child => Prefixes(child.Path)
            .Choose(prefix => byPath.Find(prefix))
            .Head
            .Map(parent => (Relationship)new Relationship.Compose(parent, child.Id, ComposeKind.Contain)));

        // Strict prefixes longest-first, the empty prefix included (a root DataObject at path [] contains all).
        static Seq<string> Prefixes(string[] path) =>
            toSeq(Enumerable.Range(0, path.Length).Reverse().Select(take => string.Join('\u0000', path.Take(take))));
    }
}
```

## [04]-[REIMPORT]

- Owner: `BimIo.Reimport` the projector-polymorphic incremental re-ingest — re-projecting a revised source through ANY `IElementProjection` and reconciling it to a prior `ElementGraph` snapshot by `ExternalId`, so a large model's minor revision costs the delta, not the whole graph; `ReimportResult` the receipt carrying the patched `ElementGraph` plus the delta-cost `GraphDelta` the reconcile produces in one fold; `Reconcile` the `ExternalId`-keyed structural diff and `Remap` the node/edge id-reidentification.
- Entry: `BimIo.Reimport(IElementProjection projector, ElementGraph prior, ProjectionContext ctx, Op key)` re-projects a revised source (the caller decodes the revised bytes once into the projector — `ImportIfc` → `new SemanticProjector(db, reconciler, profiles)`, or a `Base` → `new SpeckleProjector(root)`) and reconciles the fresh graph to `prior` by `ExternalId` (the IFC `GlobalId` / Speckle `applicationId`), emitting only the added/revised/removed nodes and edges — `Fin<T>` funnels a foreign projector fault to `Projection/fault#FAULT_BAND` `ElementFault.ProjectionFailed` through `key.Catch` and rails `ElementFault.NodeAbsent` at `Graph/element#ELEMENT_GRAPH` `Apply` on a corrupt delta; the heavy display geometry is NEVER re-tessellated because an unchanged representation content-keys identically on `RepresentationContentHash`, so the incrementality is wholly in the reconcile, the whole-file re-projection notwithstanding.
- Auto: `Reimport` runs the projector once onto a `Genesis(ctx.Header)` seed to a fresh revised `ElementGraph`, then `Reconcile` remaps each revised rooted `Object` to its prior identity by `ExternalId` (a re-projection mints FRESH neutral Guid-v7 ids, so identity is matched on the stable external id, never the node id) and rewrites every revised node and edge through that id map; the structural diff then partitions: a remapped node absent from `prior` is `AddedNodes`, present with DIFFERING `ToCanonicalBytes` is `RevisedNodes`, a prior node absent from the revised set is `RemovedNodes`, and edges diff by structural equality — a non-rooted content-keyed node (Material/PropertySet/...) needs no remap because identical content already shares its `NodeId`, so only rooted Objects with a stable remapped id and changed canonical bytes are revisions; the resulting `GraphDelta` applied to `prior` yields the reconciled `Patched`, the delta IS the change set, never a second diff pass.
- Receipt: the `ReimportResult` carries the patched `ElementGraph` (the prior snapshot advanced by the incremental delta) and the forward `GraphDelta` the `Rasm.Persistence` event log stores — a delta-cost minor revision, the `csharp:Rasm.Persistence/Version/ledger#GRAPH_DELTA` stream appending only the changed nodes/edges, the `GraphDelta.ToCanonicalBytes` content key deduping a re-applied delta; the `Review/diff#MODEL_DIFF` `ElementChange` federation change-set is the SEPARATE review surface, not minted here.
- Packages: Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new re-ingestable source is one more `IElementProjection` the caller hands `Reimport` (the IFC `SemanticProjector`, the `SpeckleProjector`, the `DotbimProjector`, a future Materials/Fabrication projector) — the reconcile is projector-agnostic, keyed only on `ExternalId`, so no second reimport entrypoint; a finer change granularity is the `ToCanonicalBytes` content comparison the diff already uses; never a parallel delta store and never a re-tessellation of a content-key-matched representation.
- Boundary: the reconcile keys on the seam `Object.ExternalId` (the IFC `GlobalId` / Speckle `applicationId`) — a re-projection mints fresh neutral Guid-v7 ids, so matching on the node id would treat every element as new; a second identity scheme or a field-by-field element comparison is the deleted form; the `GraphDelta` is the FORWARD event delta the Persistence stream stores, distinct from the `Review/diff#MODEL_DIFF` `ElementChange` review change-set — minting a `ModelDiff` here is the deleted form; reimport is ONE polymorphic owner over `IElementProjection` and a per-format `ReimportIfc`/`ReimportSpeckle` family or the retired `BimModel`/`BimModel.Project` patch is the deleted form; a content-key-matched representation is never re-tessellated (the `RepresentationContentHash` is identical) and a re-tessellation is the named seam violation; the patched value is the one `Graph/element#ELEMENT_GRAPH` `ElementGraph` snapshot, never a parallel delta-model; a corrupt reconcile delta rails `Projection/fault#FAULT_BAND` `ElementFault.NodeAbsent` at `Apply`.

```csharp signature
public sealed record ReimportResult(ElementGraph Patched, GraphDelta Delta);

public static partial class BimIo {
    // Incremental re-ingest, projector-polymorphic over ANY IElementProjection (the IFC SemanticProjector, the Speckle
    // SpeckleProjector, a future Materials/Fabrication projector): re-project a revised source to a fresh ElementGraph,
    // reconcile it to the prior snapshot by ExternalId, and emit the delta-cost GraphDelta the Persistence event log
    // stores. The caller decodes the revised bytes once (ImportIfc -> new SemanticProjector(db, reconciler, profiles), or a Base ->
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
        // Duplicate ExternalIds are a REAL malformed-source long tail (colliding IFC GlobalIds ship in the wild):
        // first-wins TryAdd + a claimed-prior set keep the reconcile TOTAL — the first revised claimant keeps the
        // prior identity, later duplicates keep their fresh ids (an add + a remove, never a wrong merge) — where
        // the throwing ToMap builds escaped the Fin rail as an uncaught duplicate-key exception.
        var priorByExternal = prior.ObjectNodes
            .Choose(static o => o.ExternalId.Map(x => (External: x, o.Id)))
            .Fold(Map<string, NodeId>(), static (held, p) => held.TryAdd(p.External, p.Id));
        var remap = revised.ObjectNodes
            .Choose(o => o.ExternalId.Bind(x => priorByExternal.Find(x)).Map(priorId => (o.Id, Prior: priorId)))
            .Fold((Claimed: HashSet<NodeId>(), Held: Map<NodeId, NodeId>()), static (acc, p) =>
                acc.Claimed.Contains(p.Prior) ? acc : (acc.Claimed.Add(p.Prior), acc.Held.Add(p.Id, p.Prior)))
            .Held;
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
        // Edge sets diff through hashed membership — the Seq.Contains scan was the deleted O(edges²) form.
        var priorEdges = toHashSet(prior.Edges);
        var revisedEdgeSet = toHashSet(revisedEdges);
        var addedEdges = revisedEdges.Filter(e => !priorEdges.Contains(e));
        var removedEdges = prior.Edges.Filter(e => !revisedEdgeSet.Contains(e)).ToSeq();
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

- [ACADSHARP_DWG_DECODE]: the `acad-sharp` codec the `Exchange/format#FORMAT_AXIS` `Dwg` row carries reads DWG/DXF in-process through the decompile-verified `ACadSharp` surface (catalogued folder-local at `.api/api-acadsharp`) — `DxfReader.Read(Stream)`/`DwgReader.Read(Stream)` return a `CadDocument` whose `Entities` (an alias for `ModelSpace.Entities`) carry the drawing geometry, and the Bim arm folds the mesh-bearing entities onto the canonical `ImportedGeometry` triangle-soup: `Mesh` (the `Vertices` `List<XYZ>` plus the `Faces` `List<int[]>` n-gon index list, each face fan-triangulated), `Face3D` (the `FirstCorner`..`FourthCorner` `XYZ` quad, the fourth corner equal to the third for a triangle), `PolyfaceMesh` (the `VertexFaceMesh` vertex pool indexed by the 1-based signed `VertexFaceRecord` `Index1`..`Index4` records, a negative index a hidden edge and a zero `Index4` a triangle, fan-triangulated), and the `Insert`-referenced geometry flattened through the package-owned `Insert.Explode()` (the OCS→WCS placement, `Rotation`, per-axis scale, OCS `Normal`, and `MINSERT` array replication `ACadSharp` owns, each placed entity folded back through the same classifier so a block-nested `Insert` recurses) rather than a hand-rolled `InsertPoint`/`XScale` matrix — the 2D profile entities (`LwPolyline`/`Polyline2D`/`Arc`/`Circle`/`Spline`) the `csharp:Rasm.Fabrication` `Ingress/profile` boundary owns are skipped, the one `CadDocument` read by two folders each projecting its owned entity families; `ACadSharp` is pure-managed AnyCPU IL, osx-arm64-safe, already consumed by Fabrication and AppUi, so the DWG/DXF round-trip lands managed without crossing the Python companion, the `netDxf` (DXF-only) reader NOT admitted because `ACadSharp` supersedes it (managed DWG AND DXF), and the reader exception lowers to `BimFault.ModelRejected` through the `BimIo.Boundary` funnel.
- [NATIVE_FORMAT_BRIDGES]: the Revit `.rvt` and Navisworks `.nwc`/`.nwd` native readers ride the `native-companion` codec through the Compute companion process (the managed C# branch has no native loader for the proprietary application formats); DWG/DXF is now managed in-process through the `acad-sharp` codec (the `[ACADSHARP_DWG_DECODE]` note above), no longer a `native-companion` two-hop; the `mesh-text` OBJ/STL/OFF decode is managed in-process through the `geometry3Sharp` `StandardMeshReader`/`OBJFormatReader`/`STLFormatReader`/`OFFFormatReader` surface decompile-verified in `.api/api-geometry3sharp`, projecting the `DMesh3` (`VertexIndices`/`TriangleIndices`/`GetVertex`/`GetVertexNormal`/`GetTriangle`) onto the canonical `ImportedGeometry` triangle-soup; PLY and 3MF have LEFT the `mesh-text` codec because `geometry3Sharp` ships no PLY and no 3MF handler — PLY is the dedicated `ply-net` codec composing `Ply.Net` `PlyParser.Parse(stream, maxChunkSize)` over the immutable `Dataset`/`ElementData` record graph (the `Vertex` element's typed `x`/`y`/`z`/`nx`/`ny`/`nz` columns read as a `System.Array` per `DataType` and the `Face` element's `vertex_indices` list column fan-triangulated, ascii/`binary_little_endian`/`binary_big_endian` off `Header.Format`), retiring the hand-rolled BCL `PlyReader`, and 3MF is the `scene-exchange` codec composing the `AssimpNetter` `AssimpContext.ImportFileFromStream` `Scene`→`Mesh` fold under the canonical `Triangulate | JoinIdenticalVertices | GenerateSmoothNormals` post-process, retiring the hand-rolled BCL `ThreeMfReader` OPC/ZIP parse — `AssimpNetter` ships its own osx-arm64 `libassimp.dylib` admitted as the one scene-exchange owner (FBX/Collada/3MF), so the former native-coupling rejection no longer holds and the rejected reader picks that DO stand are `lib3mf` (native C++) and `Aspose.3D` (closed/commercial); each codec materializes one contiguous `ImportedGeometry` boundary allocation per the boundary-mapping law, the leaked `Ply.Net.*`/`Assimp.*` package types never crossing past `Exchange/import`.
- [SPECKLE_CATALOGUE]: the `Speckle.Sdk`/`Speckle.Objects` member spellings the `ImportSpeckle` display-mesh fold and the `SpeckleProjector` host-object fold compose — `Speckle.Sdk.Models.Base` (`id`/`applicationId`/`speckle_type`/`GetTotalChildrenCount`), `Speckle.Sdk.Models.Extensions.BaseExtensions.Flatten`/`Traverse`/`TraverseWithPath`/`TryGetDisplayValue`/`IsDisplayableObject` with the `BaseRecursionBreaker` delegate (`TraverseWithPath → IEnumerable<(string[], Base)>` the path-carrying walk the `SpeckleProjector` reconstructs `Compose.Contain` containment from by nearest-ancestor path prefix), `Speckle.Sdk.Models.GraphTraversal.TraversalContext.Parent`/`PropName`/`Current`, `Speckle.Sdk.Common.Units.GetConversionFactor`/`Meters`, `Speckle.Objects.Geometry.Mesh` (`vertices`/`faces`/`vertexNormals` `List<double>`/`List<int>`, length-prefixed n-gon `faces`, `units`, `VerticesCount`), `Speckle.Objects.Geometry.Brep` (`displayValue` `List<Mesh>`), `Speckle.Objects.IDisplayValue<out T>`, and `Speckle.Objects.Data.DataObject` (`name`/`displayValue` `List<Base>`/`properties` `Dictionary<string, object?>`) and its host-object subtypes (`RevitObject`/`ArchicadObject`/`TeklaObject`/`Civil3dObject`/`AutocadObject`/`RhinoObject`, each extending `DataObject`) — are decompile-verified against the `Speckle.Sdk`/`Speckle.Objects` 3.21.1 assemblies and catalogued folder-local at `.api/api-speckle`, which records the `Mesh.vertexNormals` flat-normal member, the `DataObject.properties` `Dictionary<string, object?>` shape, and the host-object subtype roster the cross-folder `csharp:Persistence/.api/api-speckle` sync catalogue elides; the host-object semantic projection is the `SpeckleProjector : IElementProjection` lowering the `DataObject` tree onto a seam `GraphDelta` (each host object a rooted `Node.Object` carrying the generic `Classification("speckle", speckle_type)` and the `applicationId` `ExternalId`), never the retired `IfcSemanticModel` flat rows.
- [IFC5_ECS]: the IFC5 ECS-JSON (`.ifcx`) component-graph parse rides the GeometryGym IFC5 surface for the live `DatabaseIfc` the `Projection/semantic#SEMANTIC_PROJECTOR` lowers, and the Compute companion for native-grade tessellation; the `ifc5` row mirrors the IFC4x3 decode (bytes → `DatabaseIfc`), the ECS-component projection riding the projector's seam graph. IFC4.3 ADD2 is the production baseline (`ReleaseVersion.IFC4X3_ADD2`); IFC5 is the componentized/granular `.ifcx` architecture in active public development, so the `ifc5` row is a forward-looking GeometryGym IFC5-surface row grounding against the GeometryGym IFC5 member surface at alignment.
- [SEAM_DECODE]: the import-rail IFC arm collapse to a `DatabaseIfc`-only decode grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT C5 and the `Projection/semantic#SEMANTIC_PROJECTOR` owner — the `SemanticProjector : IElementProjection` reads the LIVE `DatabaseIfc` (not lossy import-rail rows), so the FULL `IfcRel*` roster, the eight stranded families, `OwnerHistory`, `StepHeader`, and the per-bag `InheritanceMode` survive on the neutral seam edge algebra; the retired `IfcSemanticModel` flat-row projection that dropped them is the deleted form §2/§4B name, and `DatabaseIfc` is captured by the projector internally so GeometryGym never crosses the seam `IElementProjection.Project` signature (the §4A IoC inversion). The ifcJSON/ifcXML `DatabaseIfc` construction reads its `ReleaseVersion` from `SemanticProjector.Sniff(bytes, format)` (the `schema_identifier`/`FILE_SCHEMA` read [H8]) rather than a hardcoded `IFC4X3_ADD2`, so a 2x3 file admits as 2x3 — and `BimIo.ImportIfc` is the ONE public bytes→`DatabaseIfc` decode owner in the package: `wire#WIRE_PROJECTION` `IfcWire.Admit` and `export#ROUNDTRIP` `RoundTrip.Verify` compose it (each re-wrapping its own admission prefix over the `Fin` rail), the duplicated private `Decode`/`JsonDatabase`/`XmlDatabase` triples retired on both consumers. `DatabaseIfc.ParseString`/`ReadJSON`/`ReadXMLDoc`/`Release`/`ModelView`/`Tolerance` and `SemanticProjector.Sniff` confirm against `.api/api-geometrygym-ifc` and `Projection/egress#IFC_EGRESS`.
- [DOTBIM_INGEST]: the `dotbim` codec arm grounds against the decompile-verified `dotbim` 1.2.0 surface (catalogued folder-local at `.api/api-dotbim`) — `dotbim.File` (`SchemaVersion`/`Meshes`/`Elements`/`Info`), the shared `Mesh` pool (`MeshId` the pool key, flat `Coordinates` XYZ triples, `Indices` triangle corners), and the placed `Element` (`MeshId` reference, `Vector` translation, quaternion `Rotation` `Qx..Qw` — never Euler, `Guid` validated identity, free `Type`, `Info` `Dictionary<string,string>`); the wire is pure `System.Text.Json` (every member snake_case `[JsonPropertyName]`), so the byte admission is one `JsonSerializer.Deserialize<dotbim.File>` and the path-bound `File.Read`/`File.Save` (which enforce the `.bim` extension) stay the file-system form; N elements over one `MeshId` decode as N `MeshInstance` rows over ONE `MeshBlock` per the api-dotbim INSTANCING_LAW, and the `Info`-bag re-bind rides `DotbimProjector : IElementProjection` (the `ExternalId` preferring the `Info["globalId"]` the `export#EXPORT_RAIL` `.bim` emit writes — the element `Guid` is `XxHash128`-derived from that GlobalId — else the `Guid`, giving `Reimport` its reconcile key) — a baked-copy decode, a `DotbimImporter` service, and an Euler read of `Rotation` are the rejected forms. The arm dispatches on the `format#FORMAT_AXIS` `InterchangeCodec.DotBim` codec row (`dotbim` central-pinned and referenced in `Rasm.Bim.csproj`), and the `export#EXPORT_RAIL` `.bim` emit is the symmetric instancing-preserving encode.
- [INSTANCE_OVERLAY]: the `Blocks`/`Instances` pool overlay grounds against the decompile-verified SharpGLTF Schema2 surface — `ModelRoot.DefaultScene`/`LogicalMeshes`, `Node.Flatten(IVisualNodeContainer)`, `Node.Mesh`/`Node.WorldMatrix` (`System.Numerics.Matrix4x4`, row-vector), `Node.GetGpuInstancing()`, `MeshGpuInstancing.Count`/`GetLocalMatrix(int)`/`GetWorldMatrix(int)` — and the api-assimpnetter node law (`Scene.RootNode`, `Node.Transform`/`Parent`/`Children`/`MeshIndices`, the world matrix the product up the parent chain; Assimp is column-vector convention so the numerics form is the transpose); the USD arm PLACES each `UsdGeomMesh` block by the composed local-to-world transform off one `UsdGeomXformCache.GetLocalToWorldTransform(UsdPrim)` → `GfMatrix4d` (catalogued in `.api/api-usd`; row-major over ROW vectors, so the numerics narrow is per-component, never the Assimp transpose — the `GfMatrix4d` row-accessor spelling confirms against the SWIG surface at alignment) — the identity-placed decode and the false no-local-to-world-read claim beside it are the deleted forms. `Bake()` is the one flattening operation consumers call for world-space geometry; `Framed` conjugates instance transforms by the row `BasisChange` (rows = `Apply` images of the source axes, `M' = Bᵀ·M·B`) so canonicalization commutes with placement.
- [REIMPORT_RECONCILE]: the projector-polymorphic reimport grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT H6 (a rooted `NodeId` is a NEUTRAL kernel-minted Guid-v7, the IFC `GlobalId` a Bim-stored `ExternalId` projection) — a re-projection mints fresh ids, so `Reconcile` matches a rooted `Object` to its prior identity by `ExternalId`, never the node id; the forward `GraphDelta` is §4-RT C1/H11's event body the `Rasm.Persistence` Marten stream appends (distinct from the §4F `Review/diff#MODEL_DIFF` `ElementChange` review change-set per `Graph/element#ELEMENT_GRAPH`'s two-change-surface law), and the §4-RT H7 `Node.ToCanonicalBytes` content comparison drives the revised-node detection. `ElementGraph.Genesis`/`Apply`/`ObjectNodes`, `GraphDelta.ReplayOnto`/`Reheader`, and `Node.Object.ExternalId` confirm against the seam `Graph/element`/`Graph/delta` owners.
