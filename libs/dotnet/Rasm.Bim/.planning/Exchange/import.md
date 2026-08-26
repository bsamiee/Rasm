# [BIM_IMPORT_PIPELINE]

`BimIo` owns foreign-bytes-and-graph ingest: one import fold lowers every `format#FORMAT_AXIS` `InterchangeFormat` row to a canonical carrier — managed mesh and scene sources to the pooled `ImportedGeometry`, IFC/IFC5 and the SAF structural workbook to the live `DatabaseIfc`, STEP to `StepSemanticModel`, the Speckle `Base` display tree to `ImportedGeometry`, and the already-tessellated IFC face-set family to `ExplicitTessellation`. Decode is the import path's only concern: the entity walk off a live graph is `Projection/semantic#SEMANTIC_PROJECTOR`'s and the foreign OBJECT-graph shared arms are `Projection/foreign#FOREIGN_PROJECTION`'s. No BRep or NURBS evaluates in-process — a non-mesh geometry request routes to `tessellation#TESSELLATION_BRIDGE`.

Mesh accumulation is the kernel's: every arm appends into one `Rasm/Meshing/mesh#MESH_SOURCE` `MeshDraft` and closes it into the shared `Rasm.Element/Projection/projection#INTERCHANGE_CARRIER` `ImportedGeometry` pool, and every provider scene graph rides ONE `SceneWalk<TNode>` instantiation supplying provider accessors alone. Attribute lanes are the kernel `Rasm.Drawing` `EncodingChannel` vocabulary the pool fold strides on; an arm whose source declares no lane declares no descriptor, because a zero-filled column is a forged fact no consumer can tell from an authored one. Posture stays HOST-LOCAL: the Speckle arm composes `Speckle.Sdk`/`Speckle.Objects` only in the host-neutral exchange assembly, never inside the in-Rhino plugin ALC.

## [01]-[INDEX]

- [02]-[IMPORT_PIPELINE]: foreign bytes and object graphs to the pooled carrier, the live `DatabaseIfc`, and `StepSemanticModel` — one codec-keyed arm per decode over one draft accumulator and one scene fold.
- [03]-[EXPLICIT_TESSELLATION]: `BimIo.ImportIfcTessellation` decoding the `IfcTessellatedFaceSet` family IN PROCESS onto the same carrier and routing the evaluator residue to the companion as a narrowed `TessellationScope.Elements`.

## [02]-[IMPORT_PIPELINE]

- Owner: `BimIo` — the import fold over `InterchangeFormat`, one `InterchangeCodec`-keyed arm per decode. Three canonical carriers: the shared `ImportedGeometry` mesh POOL this import path produces, the live `DatabaseIfc` the `Projection/semantic#SEMANTIC_PROJECTOR` captures and lowers, and `StepSemanticModel` the ISO 10303 product-structure projection. `UsdScope` is the stage-population vocabulary the USD arm opens under and `UsdNode` its closed walk vocabulary; `OrdinalCompactor` the shared sparse-id compaction; `PlyLane`, `MeshoptMode`, and `MeshoptFilter` the three foreign spec rosters this import path reads by row rather than by string literal.
- Entry: `ImportGeometry` (mesh-and-scene bytes → `ImportedGeometry`), `ImportSpeckle` (a received `Base` display tree → `ImportedGeometry`), `ImportIfc` (IFC/IFC5 and the SAF workbook → live `DatabaseIfc`), and `ImportStep` (Part-21 → `StepSemanticModel`), each dispatching by `InterchangeCodec` so a path lands one decode without a call-site type branch. `Fin<T>` aborts on `Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Codec` or the companion-required `BimFault.Refused` with `BimReason.Capability`, each `Op`-keyed case lifting BARE (band 2600 owns the generated `Code`, no `.ToError()` hop). `ImportGeometry` takes the optional `Model/observability#HOOKS` hook carrier and hands it to EVERY managed arm, and the optional `UsdScope` the USD arm opens its stage under — an absent scope is the whole stage, so the unscoped call is unchanged.
- Auto: glTF decode routes binary GLB and text `.gltf` by format with zero intermediate file, a `Decompress` pre-decode branch reading each primitive's `KHR_draco_mesh_compression` and each bufferView's `EXT_meshopt_compression` extension before the `LogicalMeshes.Decode()` fold. IFC decode constructs the live `DatabaseIfc` by the row's own wire form at the schema `IfcWireForm.Sniff` reads off the bytes — the container unwrapped before the probe, so a zipped payload sniffs its true schema where a text probe over archive bytes reported every one as a header miss — never a hardcoded default. Every managed arm beats the shared `DecodeStage` ladder onto `rasm.bim.exchange.progress` at its own phase boundaries; the ladder declares the phase fractions ONCE and the ACadSharp arm folds its package-published `ReadStage` onto those same rows, so one lane's foreign progress source never becomes a second fraction vocabulary.
- Law: the three provider scene graphs run ONE fold. `SceneWalk<TNode>` takes the four provider accessors (`Flatten`, `Excluded`, `Placements`, `Blocks`) and owns the placement law itself — parent-frame threading, the per-node placement fan, and the identity sweep of every block no node referenced — so glTF's `EXT_mesh_gpu_instancing` fan and USD's point-instancer scatter are ONE `Placements` arm rather than two bodies. NAMED LOSS: the per-provider referenced mask is gone, and with it each arm's inline statement of its own sweep; the draft's own block roster IS the arity, so the sweep is structural and a provider that places every appended block (USD) needs no arm to skip it. Second named loss: the placement currency crosses the accumulator as the kernel `Transform` and returns as the contract's `Matrix4x4`, so the row-vector/column-vector convention flip lands in ONE declared `Placed`/`Numeric` correspondence instead of being re-derived per arm.
- Law: an absent source lane declares no descriptor. Sources carrying no authored normals contribute no `Normal` lane, and every consumer reads the block's own `Declared` set — the up-normal fill six arms wrote was a FORGED measurement indistinguishable from an authored one, and the shared carrier's own absence law forbids it.
- Exemption: the per-vertex lane fills, the corner gathers, the Part-21 recursive-descent tokenizer, and the sparse-id compactor are boundary decode KERNELS under `EXPRESSION_SPINE`, named once here rather than at each site. One fact charters them: the accessor contracts (`IMeshPrimitiveDecoder`, `DMesh3`, `Ply.Net`, Assimp `Scene`, the USD typed-array bridge, GeometryGym's coordinate lists) admit no zero-copy span into package buffers, so the one boundary materialization IS the allocation point and the import path resumes at the draft append.
- Output: `BimFact.Imported` fires once at the decode seal with the format key, codec key, source byte count, and elapsed, an instanced source also reading the carrier's `Blocks.Count`/`Instances.Count` sharing evidence; every other decode fact stays on its carrier — the schema version and model view off the live `DatabaseIfc` (the entity count rides the `SemanticProjector` delta, not the import path), the `StepProtocol`, `FILE_SCHEMA` name, and product/definition/assembly/geometry-ref counts off the `StepSemanticModel`.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Runtime, GeometryGymIFC_Core, StructuralAnalysisFormat, Openize.Drako, Alimer.Bindings.MeshOptimizer, geometry3Sharp, Ply.Net, AssimpNetter, UniversalSceneDescription, ACadSharp, dotbim, Speckle.Sdk, Speckle.Objects, Riok.Mapperly, NodaTime, LanguageExt.Core, Rasm
- Growth: a new managed import is one codec arm keyed by the `InterchangeFormat.Codec` row, taking the hook carrier and beating the shared `DecodeStage` rows — a new phase is one row on that ladder, never a per-arm fraction table; a new SCENE source is one `SceneWalk<TNode>` instantiation and zero new folds; a new populated-source scope is one `UsdScope` case the `Staged` open reads, never a filter-mode flag beside the value; a new per-vertex attribute is one `EncodingChannel` row and one lane entry in whichever arms read it — the draft's pool fold strides on the channel's own declared arity, so the accumulator, the mint, and every other arm stay untouched; a new glTF compression codec is one `KhrEncoder`-keyed arm on the `Decompress` branch; a new meshopt mode or filter is one roster row carrying its own decoder delegate; a new PLY column is one `PlyLane` row; a new extracted IFC entity family is one `Extract<T>` arm on the `SemanticProjector`, never on the import path; a new STEP application protocol is one `InterchangeFormat` row carrying its `StepProtocol` discriminant, the entity-instance grammar being protocol-agnostic so AP203/AP214/AP242 share one reader.
- Boundary: `BimIo` is the page boundary capsule — leaked package types (`Ply.Net.*`, `Assimp.*`, `pxr.*`, the `SWIGTYPE_p_*`/`*PINVOKE` USD interop, `Speckle.*`, `dotbim.*`) never cross past `Exchange/import`. Decoded attributes land on the unit-valued domain their shared channel stores: a PLY colour column divides by the full scale its DECLARED width names, never by a scale inferred from the values, because a dark scan and a float writer's output are indistinguishable by inspection and guessing there blackens every such delivery. `.bim` byte admission reads through a source-generated `JsonSerializerContext` declaring `dotbim.File` as an EXTERNAL serializable root, so no reflection-mode `Deserialize<T>` survives a trimmed or AOT publish. Foreign type vocabularies are OPEN, so a type this import path does not evaluate reports ONCE per type name on the degrade channel — a DWG of a hundred thousand 2D lines and a USD stage of curves each state their skipped families exactly once, where a per-entity fact drowns the channel and the retired silent tail hid the drop entirely. `Mesh` AND `PointInstancer` prims both admit on the USD arm, because USD expresses repetition natively and a Mesh-only filter imports a point-instanced site delivery EMPTY; the instancer's own `ComputeInstanceTransformsAtTime` composes each instance matrix and the fan groups BY PROTOTYPE, so each prototype's blocks place at the instances that wear it rather than at every instance the node carries. USD carries a multi-material mesh as material-bound `UsdGeomSubset` children over face ordinals, so the decode partitions on the AUTHORED subsets: one block per subset stamping the shared `MeshBlock.Material` key off that subset's own direct binding, with one further block over the remainder `GetUnassignedIndices` names, and each partition compacts to the points its own faces reference. Stage population is decided AT the open through `UsdScope` and `UsdStage.OpenMasked` — a post-open traversal filter is the deleted form, because it pays the whole layer stack's composition and prim indexing before discarding it, which is the entire cost a scoped read of a federated site delivery exists to avoid. IFC decodes ONLY the live `DatabaseIfc`; the lossy `IfcSemanticModel` flat-row re-projection is the deleted form, and GeometryGym carries no tessellation kernel so an IFC geometry request routes to `tessellation#TESSELLATION_BRIDGE`. STEP splits two legs: the managed semantic-graph leg in-process through the BCL-only `StepReader`, the B-rep geometry leg companion-routed — no managed Part-21 reader admits, and GeometryGym is IFC-schema-bound so it grounds no STEP semantic leg. SAF admits on the IFC entrypoint because its carrier IS the live `DatabaseIfc`: the XLSX bytes decode through the ONE `Exchange/saf#SAF_EXCHANGE` `SafCodec.Run` import leg and the validated `ExcelModel` AUTHORS GeometryGym structural entities onto a fresh SI-declared database the `SemanticProjector` then ingests — a SAF-side projector minting shared member nodes is the deleted standalone form — the authoring residue firing the `saf-residue` degrade row once per uncarried payload so no drop is silent.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ACadSharp.IO;
using Assimp;
using CommunityToolkit.HighPerformance;
using GeometryGym.Ifc;
using g3;
using LanguageExt;
using NodaTime;
using Ply.Net;
using pxr;
using Riok.Mapperly.Abstractions;
using Speckle.Sdk.Models;
using Rasm;
using Rasm.Drawing;
using Rasm.Meshing;
using Rasm.Bim.Model;
using Rasm.Bim.Projection;
using Rasm.Domain;
using Rasm.Element.Projection;
using Thinktecture;
using static LanguageExt.Prelude;
using Cad = ACadSharp.Entities;
using SAF.DataAccess.Contracts;
using SharpGLTF.Schema2;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Transform = Rhino.Geometry.Transform;
using Vector3 = System.Numerics.Vector3;
using BimHooks = Rasm.Domain.HookSet<Rasm.Bim.Model.BimPoint, Rasm.Bim.Model.BimFact, Rasm.Domain.TelemetrySource>;
using GGRelease = GeometryGym.Ifc.ReleaseVersion;

namespace Rasm.Bim;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UsdScope {
    private UsdScope() { }

    public sealed record WholeStage : UsdScope;
    public sealed record Populated(Seq<string> Paths) : UsdScope;

    public static readonly UsdScope Whole = new WholeStage();

    public static Fin<UsdScope> Of(Seq<string> paths, Op key) =>
        paths.IsEmpty
            ? Fin.Succ(Whole)
            : paths.Traverse(candidate => Populates(candidate)
                    ? Validation<Error, string>.Success(candidate)
                    : Validation<Error, string>.Fail(new BimFault.Refused(key, BimScope.Import, BimReason.Rejected, string.Join(':', new object?[] { "usd-scope-path", candidate }))))
                .As()
                .Map(static admitted => (UsdScope)new Populated(admitted.Distinct()))
                .ToFin();

    static bool Populates(string candidate) {
        if (!SdfPath.IsValidPathString(candidate, out string _)) { return false; }
        using var path = new SdfPath(candidate);
        return path.IsAbsolutePath() && path.IsPrimPath();
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UsdNode {
    private UsdNode() { }

    public sealed record Meshed(UsdPrim Prim, Transform World) : UsdNode;
    public sealed record Scattered(UsdPrim Prototype, Seq<Transform> Worlds) : UsdNode;
}

// --- [MODELS] --------------------------------------------------------------------------
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

public sealed record SafServices(
    IExcelImportService Imports, IExcelExportService Exports, IExcelValidator Validator,
    Version Target, GGRelease Schema);

// --- [BOUNDARIES] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class PoolMap {
    [MapProperty(nameof(MeshBlockRange.VertexOffset), nameof(MeshBlock.VertexOffset), Use = nameof(Narrow))]
    [MapProperty(nameof(MeshBlockRange.VertexCount), nameof(MeshBlock.VertexCount), Use = nameof(Narrow))]
    [MapProperty(nameof(MeshBlockRange.CornerOffset), nameof(MeshBlock.IndexOffset), Use = nameof(Narrow))]
    [MapProperty(nameof(MeshBlockRange.CornerCount), nameof(MeshBlock.IndexCount), Use = nameof(Narrow))]
    public static partial MeshBlock ToBlock(MeshBlockRange range);

    [UserMapping] private static int Narrow(long extent) => (int)extent;
}

// --- [SERVICES] ------------------------------------------------------------------------
public static partial class BimIo {
    [SmartEnum]
    public sealed partial class DecodeStage {
        public static readonly DecodeStage Opened = new(done: 0.00, witness: "opened", read: ReadStage.Read);
        public static readonly DecodeStage Decoded = new(done: 0.45, witness: "decoded", read: ReadStage.Build);
        public static readonly DecodeStage Placed = new(done: 0.80, witness: "placed", read: null);
        public static readonly DecodeStage Assembled = new(done: 1.00, witness: "assembled", read: null);

        public double Done { get; }
        public string Witness { get; }

        public ReadStage? Read { get; }

        public StageMark Mark => new(Done, Witness);

        public Unit Beat(Option<BimHooks> hooks, Op key) =>
            hooks.IfSome(live => ignore(live.Fire(BimPoint.ExchangeProgress, new BimFact.Progress(key, ProgressLane.Exchange, Mark), key)));

        public static readonly FrozenDictionary<ReadStage, DecodeStage> ByReadStage =
            Items.Where(static row => row.Read is not null)
                .ToFrozenDictionary(static row => row.Read!.Value, static row => row);
    }

    [SmartEnum<string>]
    [KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
    [KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
    public sealed partial class DecodeReason {
        public static readonly DecodeReason SolidUnevaluated = new("solid-unevaluated");
        public static readonly DecodeReason SafResidue = new("saf-residue");
        public static readonly DecodeReason TypeUnevaluated = new("type-unevaluated");
        public static readonly DecodeReason PresentationDropped = new("presentation-dropped");

        public Unit Degrade(Option<BimHooks> hooks, Op key, string subject) =>
            hooks.IfSome(live => ignore(live.Fire(BimPoint.ExchangeDegrade, new BimFact.Degraded(key, "exchange", Key, subject), key)));
    }

    sealed class Unseen {
        readonly HashSet<string> seen = new(StringComparer.Ordinal);

        public Unit Once(string type, Option<BimHooks> hooks, Op key) =>
            seen.Add(type) ? DecodeReason.TypeUnevaluated.Degrade(hooks, key, type) : unit;
    }

    public static Fin<ImportedGeometry> ImportGeometry(
        InterchangeFormat format, ReadOnlyMemory<byte> bytes, IClock clock, Op key,
        Option<BimHooks> hooks = default, Option<UsdScope> scope = default) =>
        InterchangeFormat.Admitted(format, InterchangeCapability.Import, key).Bind(row => row.Codec.Switch(
            sharpGltf:        () => Boundary(key, () => Gltf(format, bytes, clock.GetCurrentInstant(), hooks, key)).Bind(g => Framed(format, g, key)),
            meshText:         () => MeshTextGeometry(format, bytes, clock.GetCurrentInstant(), hooks, key),
            ply:              () => Boundary(key, () => Ply(format, bytes, clock.GetCurrentInstant(), hooks, key)).Bind(g => Framed(format, g, key)),
            sceneExchange:    () => Boundary(key, () => Scene(format, bytes, clock.GetCurrentInstant(), hooks, key)).Bind(g => Framed(format, g, key)),
            usdStage:         () => Boundary(key, () => Usd(format, bytes, clock.GetCurrentInstant(), hooks, scope, key)),
            acadSharp:        () => Boundary(key, () => AcadReader.Read(format, bytes, clock.GetCurrentInstant(), hooks, key)).Bind(g => Framed(format, g, key)),
            dotBim:           () => Boundary(key, () => DotBim(format, bytes, clock.GetCurrentInstant(), hooks, key)).Bind(g => Framed(format, g, key)),
            geometryGym:      () => Fin.Fail<ImportedGeometry>(new BimFault.Refused(key, BimScope.Import, BimReason.Codec, string.Join(':', new object?[] { "import-ifc-route", "use-ImportIfc", format.Key }))),
            stepIso10303:     () => Fin.Fail<ImportedGeometry>(new BimFault.Refused(key, BimScope.Import, BimReason.Codec, string.Join(':', new object?[] { "import-step-route", "use-ImportStep", format.Key }))),
            geospatialVector: () => Fin.Fail<ImportedGeometry>(new BimFault.Refused(key, BimScope.Import, BimReason.Codec, string.Join(':', new object?[] { "import-geospatial-route", format.Key }))),
            geospatialRaster: () => Fin.Fail<ImportedGeometry>(new BimFault.Refused(key, BimScope.Import, BimReason.Codec, string.Join(':', new object?[] { "import-geospatial-route", format.Key }))),
            pointCloud:       () => Fin.Fail<ImportedGeometry>(new BimFault.Refused(key, BimScope.Import, BimReason.Codec, string.Join(':', new object?[] { "import-point-cloud-route", format.Key }))),
            nativeCompanion:  () => Fin.Fail<ImportedGeometry>(new BimFault.Refused(key, BimScope.Import, BimReason.Capability, string.Join(':', new object?[] { "import-needs-companion", format.Key }))),
            igesAnsi:         () => Fin.Fail<ImportedGeometry>(new BimFault.Refused(key, BimScope.Import, BimReason.Capability, string.Join(':', new object?[] { "import-needs-companion", format.Key }))),
            saf:              () => Fin.Fail<ImportedGeometry>(new BimFault.Refused(key, BimScope.Import, BimReason.Codec, string.Join(':', new object?[] { "import-ifc-route", "use-ImportIfc", format.Key }))),
            cobieXlsx:        () => Fin.Fail<ImportedGeometry>(new BimFault.Refused(key, BimScope.Format, BimReason.Codec, string.Join(':', new object?[] { "direction-unsupported", InterchangeCapability.Import.Key, format.Key }))),
            energyModel:      () => Fin.Fail<ImportedGeometry>(new BimFault.Refused(key, BimScope.Import, BimReason.Codec, string.Join(':', new object?[] { "import-energy-route", "EnergyExchange.Apply", format.Key }))),
            ifc5Pending:      () => Fin.Fail<ImportedGeometry>(new BimFault.Refused(key, BimScope.Import, BimReason.Codec, string.Join(':', new object?[] { "import-catalogue-pending", format.Key })))));

    public static Fin<ImportedGeometry> ImportSpeckle(Base root, IClock clock, Op key, Option<BimHooks> hooks = default) =>
        Boundary(key, () => DisplayScene(root, clock.GetCurrentInstant(), hooks, key))
            .Bind(scene => scene.TriangleCount > 0
                ? Fin.Succ(scene)
                : Fin.Fail<ImportedGeometry>(new BimFault.Refused(key, BimScope.Import, BimReason.Rejected, string.Join(':', new object?[] { "speckle-no-display", root.speckle_type }))));

    public static Fin<DatabaseIfc> ImportIfc(
        InterchangeFormat format, ReadOnlyMemory<byte> bytes, Op key,
        Option<SafServices> saf = default, Option<BimHooks> hooks = default) =>
        InterchangeFormat.Admitted(format, InterchangeCapability.Import, key).Bind(row =>
            row.Codec == InterchangeCodec.GeometryGym
                ? row.Serialization.ToFin(new BimFault.Refused(key, BimScope.Import, BimReason.Codec, string.Join(':', new object?[] { "ifc-codec-miss", row.Key, "serialization-absent" })))
                    .Bind(form => form.Sniff(bytes, key)
                        .Bind(schema => Boundary(key, () => Database(row, bytes, schema, key))))
            : row.Codec == InterchangeCodec.Saf
                ? saf.Match(
                    Some: services => SafDatabase(bytes, services, hooks, key),
                    None: () => Fin.Fail<DatabaseIfc>(new BimFault.Refused(key, BimScope.Import, BimReason.Codec, string.Join(':', new object?[] { "ifc-codec-miss", row.Key, "saf-services-absent" }))))
                : Fin.Fail<DatabaseIfc>(new BimFault.Refused(key, BimScope.Import, BimReason.Codec, string.Join(':', new object?[] { "ifc-codec-miss", row.Key }))));

    public static Fin<StepSemanticModel> ImportStep(InterchangeFormat format, ReadOnlyMemory<byte> bytes, IClock clock, Op key) =>
        InterchangeFormat.Admitted(format, InterchangeCapability.Import, key)
            .Bind(row => row.Codec == InterchangeCodec.StepIso10303
                ? Boundary(key, () => Fin.Succ(StepReader.Read(row, bytes.Span, clock.GetCurrentInstant())))
                : Fin.Fail<StepSemanticModel>(new BimFault.Refused(key, BimScope.Import, BimReason.Codec, string.Join(':', new object?[] { "step-codec-miss", row.Key }))));

    // --- [OPERATIONS]
    static Fin<T> Boundary<T>(Op key, Func<Fin<T>> decode) =>
        key.Catch(decode);

    static Fin<ImportedGeometry> Sealed(MeshDraft draft, InterchangeFormat format, Instant at, Option<BimHooks> hooks, Op key) =>
        draft.Close(key)
            .Bind(closed => ImportedGeometry.Of(
                formatKey:     format.Key,
                lanes:         closed.Lanes,
                indices:       closed.Corners,
                vertexCount:   (int)draft.VertexCount,
                triangleCount: closed.Corners.Length / 3,
                blocks:        closed.Blocks.Map(PoolMap.ToBlock),
                instances:     draft.Instances.Map(Instanced),
                at:            at,
                key:           key))
            .Map(geometry => { DecodeStage.Assembled.Beat(hooks, key); return geometry; });

    static Fin<int> Baked(
        MeshDraft draft, long count, Seq<(EncodingChannel Channel, float[] Values)> lanes,
        ReadOnlySpan<long> corners, Op key, Option<string> material = default) =>
        draft.Append(count, lanes, corners, key, material).Bind(block => draft.Place(block, Transform.Identity).Map(_ => block));

    static Seq<(int A, int B, int C)> Fan(int arity) =>
        toSeq(Enumerable.Range(1, Math.Max(0, arity - 2))).Map(static k => (A: 0, B: k, C: k + 1));

    static Transform Placed(Matrix4x4 m) {
        Transform t = Transform.Identity;
        (t.M00, t.M01, t.M02, t.M03) = (m.M11, m.M21, m.M31, m.M41);
        (t.M10, t.M11, t.M12, t.M13) = (m.M12, m.M22, m.M32, m.M42);
        (t.M20, t.M21, t.M22, t.M23) = (m.M13, m.M23, m.M33, m.M43);
        (t.M30, t.M31, t.M32, t.M33) = (m.M14, m.M24, m.M34, m.M44);
        return t;
    }

    static Transform Placed(Assimp.Matrix4x4 m) {
        Transform t = Transform.Identity;
        (t.M00, t.M01, t.M02, t.M03) = (m.A1, m.A2, m.A3, m.A4);
        (t.M10, t.M11, t.M12, t.M13) = (m.B1, m.B2, m.B3, m.B4);
        (t.M20, t.M21, t.M22, t.M23) = (m.C1, m.C2, m.C3, m.C4);
        (t.M30, t.M31, t.M32, t.M33) = (m.D1, m.D2, m.D3, m.D4);
        return t;
    }

    static Transform Placed(GfMatrix4d m) {
        var (a, b, c, d) = (m.GetRow(0), m.GetRow(1), m.GetRow(2), m.GetRow(3));
        Transform t = Transform.Identity;
        (t.M00, t.M01, t.M02, t.M03) = (a[0], b[0], c[0], d[0]);
        (t.M10, t.M11, t.M12, t.M13) = (a[1], b[1], c[1], d[1]);
        (t.M20, t.M21, t.M22, t.M23) = (a[2], b[2], c[2], d[2]);
        (t.M30, t.M31, t.M32, t.M33) = (a[3], b[3], c[3], d[3]);
        return t;
    }

    static Matrix4x4 Numeric(Transform t) => new(
        (float)t.M00, (float)t.M10, (float)t.M20, (float)t.M30,
        (float)t.M01, (float)t.M11, (float)t.M21, (float)t.M31,
        (float)t.M02, (float)t.M12, (float)t.M22, (float)t.M32,
        (float)t.M03, (float)t.M13, (float)t.M23, (float)t.M33);

    static MeshInstance Instanced((int Block, Transform Placement) instance) =>
        new(instance.Block, Numeric(instance.Placement));

    // --- [ORDINAL_COMPACTION]
    ref struct OrdinalCompactor {
        readonly Dictionary<int, int> slots;
        readonly List<int> sources;

        public OrdinalCompactor(int capacityHint) {
            slots = new Dictionary<int, int>(capacityHint);
            sources = new List<int>(capacityHint);
        }

        public int Count => sources.Count;
        public IReadOnlyList<int> Sources => sources;

        public int Slot(int foreignId) {
            if (slots.TryGetValue(foreignId, out int held)) { return held; }
            slots[foreignId] = sources.Count;
            sources.Add(foreignId);
            return sources.Count - 1;
        }
    }

    // --- [MESH_TEXT]
    static Fin<ImportedGeometry> MeshTextGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at, Option<BimHooks> hooks, Op key) {
        string extension = format.Extensions.Head.Map(static ext => ext.TrimStart('.')).IfNone("");
        var builder = new DMesh3Builder();
        var reader = new StandardMeshReader { MeshBuilder = builder };
        return reader.SupportsFormat(extension)
            ? Boundary(key, () => MeshText(reader, builder, format, extension, bytes, at, hooks, key)).Bind(g => Framed(format, g, key))
            : Fin.Fail<ImportedGeometry>(new BimFault.Refused(key, BimScope.Import, BimReason.Codec, string.Join(':', new object?[] { "mesh-text-unsupported", format.Key, extension })));
    }

    static Fin<ImportedGeometry> MeshText(
        StandardMeshReader reader, DMesh3Builder builder, InterchangeFormat format, string extension,
        ReadOnlyMemory<byte> bytes, Instant at, Option<BimHooks> hooks, Op key) {
        var read = reader.Read(new MemoryStream(bytes.ToArray()), extension, ReadOptions.Defaults);
        if (read.code != IOCode.Ok) {
            return Fin.Fail<ImportedGeometry>(new BimFault.Refused(key, BimScope.Import, BimReason.Rejected, string.Join(':', new object?[] { "import-decode", "mesh-text-read", read.code.ToString(), read.message })));
        }
        DecodeStage.Opened.Beat(hooks, key);
        using var draft = MeshDraft.Of();
        return toSeq(builder.Meshes)
            .Traverse(mesh => TextBlock(draft, mesh, key)).As()
            .Map(_ => DecodeStage.Decoded.Beat(hooks, key))
            .Bind(_ => Sealed(draft, format, at, hooks, key));
    }

    static Fin<int> TextBlock(MeshDraft draft, DMesh3 mesh, Op key) {
        var compact = new OrdinalCompactor(mesh.VertexCount);
        foreach (int vid in mesh.VertexIndices()) { ignore(compact.Slot(vid)); }
        var positions = new float[compact.Count * 3];
        var normals = mesh.HasVertexNormals ? new float[compact.Count * 3] : [];
        for (int slot = 0; slot < compact.Count; slot++) {
            var p = mesh.GetVertex(compact.Sources[slot]);
            int v = slot * 3;
            (positions[v], positions[v + 1], positions[v + 2]) = ((float)p.x, (float)p.y, (float)p.z);
            if (normals.Length > 0) {
                var n = mesh.GetVertexNormal(compact.Sources[slot]);
                (normals[v], normals[v + 1], normals[v + 2]) = (n.x, n.y, n.z);
            }
        }
        long[] corners = mesh.TriangleIndices()
            .SelectMany(tid => mesh.GetTriangle(tid) is var tri
                ? new long[] { compact.Slot(tri.a), compact.Slot(tri.b), compact.Slot(tri.c) }
                : [])
            .ToArray();
        return Baked(draft, compact.Count, Lanes(positions, normals), corners, key);
    }

    static Seq<(EncodingChannel Channel, float[] Values)> Lanes(float[] positions, float[] normals) =>
        Seq((EncodingChannel.Position, positions))
        + (normals.Length > 0 ? Seq((EncodingChannel.Normal, normals)) : Seq<(EncodingChannel, float[])>());

    // --- [SAF]
    static Fin<DatabaseIfc> SafDatabase(ReadOnlyMemory<byte> bytes, SafServices services, Option<BimHooks> hooks, Op key) =>
        SafCodec.Run(
                new SafOp.Import(new MemoryStream(bytes.ToArray()), services.Target),
                services.Imports, services.Exports, services.Validator, key)
            .Bind(model => Boundary(key, () => {
                DatabaseIfc db = new(services.Schema);
                IfcSite host = new(db, "SAF");
                _ = new IfcProject(host, "SAF", IfcUnitAssignment.Length.Metre);
                return Fin.Succ((Db: db, Host: host, Model: model));
            }))
            .Bind(authored => Fidelity.Run(SafCodec.Author(authored.Db, authored.Host, authored.Model, key))
                .Map(run => {
                    run.Log.Facts.Iter(fact => DecodeReason.SafResidue.Degrade(hooks, key, fact.Anchor));
                    return authored.Db;
                }));

    // --- [IFC]
    static Fin<DatabaseIfc> Database(InterchangeFormat format, ReadOnlyMemory<byte> bytes, GGRelease schema, Op key) =>
        format.Serialization
            .ToFin(new BimFault.Refused(key, BimScope.Import, BimReason.Codec, string.Join(':', new object?[] { "ifc-codec-miss", format.Key, "serialization-absent" })))
            .Map(form => form.Admit(bytes, schema));

    // --- [GLTF]
    static Fin<ImportedGeometry> Gltf(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at, Option<BimHooks> hooks, Op key) {
        string json = Compression.JsonChunk(bytes);
        bool compressed = Compression.IsPresent(json);
        var validation = compressed ? ValidationMode.Skip : ValidationMode.Strict;
        ModelRoot model;
        if (format == InterchangeFormat.Glb) {
            using Stream source = bytes.AsStream();
            model = ModelRoot.ReadGLB(source, new ReadSettings { Validation = validation });
        } else {
            model = TextContext(bytes, validation).ReadTextSchema2(new MemoryStream(bytes.ToArray()));
        }
        DecodeStage.Opened.Beat(hooks, key);
        return Decoded(format, compressed ? Compression.Decompress(model, json) : model, at, hooks, key);
    }

    static ReadContext TextContext(ReadOnlyMemory<byte> bytes, ValidationMode validation) {
        var context = ReadContext.CreateFromDictionary(
            new Dictionary<string, ArraySegment<byte>> { ["model.gltf"] = new ArraySegment<byte>(bytes.ToArray()) },
            checkExtensions: true);
        context.Validation = validation;
        return context;
    }

    static Fin<ImportedGeometry> Decoded(InterchangeFormat format, ModelRoot model, Instant at, Option<BimHooks> hooks, Op key) {
        var meshes = model.LogicalMeshes.Decode();
        using var draft = MeshDraft.Of();
        return toSeq(Enumerable.Range(0, meshes.Count))
            .Traverse(m => GltfBlock(draft, meshes[m], Declared(model.LogicalMeshes[m]), key)).As()
            .Map(blocks => { DecodeStage.Decoded.Beat(hooks, key); return blocks; })
            .Bind(blocks => Walk(model, blocks).Accrue(Option<SharpGLTF.Schema2.Node>.None, draft, key))
            .Map(_ => DecodeStage.Placed.Beat(hooks, key))
            .Bind(_ => Sealed(draft, format, at, hooks, key));
    }

    static SceneWalk<Option<SharpGLTF.Schema2.Node>> Walk(ModelRoot model, Seq<int> blocks) => new(
        Flatten: node => node.IsNone
            ? toSeq(Optional(model.DefaultScene).Map(static s => SharpGLTF.Schema2.Node.Flatten(s)).IfNone([])).Map(Some)
            : Seq<Option<SharpGLTF.Schema2.Node>>(),
        Excluded: static node => node.Map(static n => n.Mesh is null).IfNone(false),
        Placements: static (node, _) => node.Match(
            Some: n => n.GetGpuInstancing() is { Count: > 0 } gpu
                ? toSeq(Enumerable.Range(0, gpu.Count)).Map(i => Placed(gpu.GetWorldMatrix(i)))
                : Seq(Placed(n.WorldMatrix)),
            None: static () => Seq<Transform>()),
        Blocks: (node, _) => Fin.Succ(node.Bind(static n => Optional(n.Mesh)).Map(mesh => blocks[mesh.LogicalIndex]).ToSeq()));

    static Seq<EncodingChannel> Declared(SharpGLTF.Schema2.Mesh mesh) =>
        mesh.Primitives.Any(static prim => prim.GetVertexAccessor("TEXCOORD_0") is not null)
            ? Seq(EncodingChannel.Uv)
            : Seq<EncodingChannel>();

    static Fin<int> GltfBlock(MeshDraft draft, IMeshDecoder<Material> mesh, Seq<EncodingChannel> declared, Op key) {
        var triangles = toSeq(mesh.Primitives.SelectMany(static prim => prim.TriangleIndices.Select(tri => (prim, tri))));
        int vertexCount = triangles.Count * 3;
        var positions = new float[vertexCount * 3];
        var normals = new float[vertexCount * 3];
        bool mapped = declared.Contains(EncodingChannel.Uv);
        var uvs = mapped ? new float[vertexCount * 2] : [];
        var corners = new long[vertexCount];
        int slot = 0;
        Span<int> fan = stackalloc int[3];
        foreach (var (prim, (a, b, c)) in triangles) {
            (fan[0], fan[1], fan[2]) = (a, b, c);
            foreach (int corner in fan) {
                var p = prim.GetPosition(corner);
                var n = prim.GetNormal(corner);
                int v = slot * 3;
                (positions[v], positions[v + 1], positions[v + 2]) = (p.X, p.Y, p.Z);
                (normals[v], normals[v + 1], normals[v + 2]) = (n.X, n.Y, n.Z);
                if (mapped) {
                    var uv = prim.GetTextureCoord(corner, 0);
                    (uvs[slot * 2], uvs[(slot * 2) + 1]) = (uv.X, uv.Y);
                }
                corners[slot] = slot;
                slot++;
            }
        }
        return draft.Append(vertexCount,
            Lanes(positions, normals) + (mapped ? Seq((EncodingChannel.Uv, uvs)) : Seq<(EncodingChannel, float[])>()),
            corners, key);
    }

    static Fin<ImportedGeometry> Framed(InterchangeFormat format, ImportedGeometry geometry, Op key) {
        if (format.IsCanonicalFrame) {
            return Fin.Succ(geometry);
        }
        Matrix4x4 basis = Basis(format);
        Matrix4x4 inverse = Matrix4x4.Transpose(basis);
        Seq<(EncodingChannel Channel, float[] Raw)> lanes = geometry.Lanes.Descriptors.Map(descriptor => {
            var raw = new float[descriptor.Count * descriptor.Channel.Arity];
            descriptor.Dtype.Unpack(geometry.Lanes.Channel(descriptor.Channel).Span, raw);
            if (descriptor.Channel == EncodingChannel.Position || descriptor.Channel == EncodingChannel.Normal) {
                FrameNormalization.Canonicalize(format, raw.AsSpan(), stride: descriptor.Channel.Arity);
            }
            return (descriptor.Channel, raw);
        });
        return Encode.Of(geometry.VertexCount, lanes).Map(arena => geometry with {
            Lanes = arena,
            Instances = geometry.Instances.Map(i => i with { Transform = inverse * i.Transform * basis }),
        });
    }

    static Matrix4x4 Basis(InterchangeFormat format) {
        var (xx, xy, xz) = format.Frame.Apply(1f, 0f, 0f);
        var (yx, yy, yz) = format.Frame.Apply(0f, 1f, 0f);
        var (zx, zy, zz) = format.Frame.Apply(0f, 0f, 1f);
        return new Matrix4x4(xx, xy, xz, 0f, yx, yy, yz, 0f, zx, zy, zz, 0f, 0f, 0f, 0f, 1f);
    }

    // --- [GLB_COMPRESSION]
    static class Compression {
        public unsafe delegate int MeshoptDecode(byte* destination, nuint count, nuint stride, byte* source, nuint length);
        public unsafe delegate void MeshoptUnfilter(void* buffer, nuint count, nuint stride);

        [SmartEnum<string>]
        [UseDelegateFromConstructor]
        public sealed partial class MeshoptMode {
            public static readonly MeshoptMode Attributes = new("ATTRIBUTES", Meshopt.DecodeVertexBuffer);
            public static readonly MeshoptMode Triangles = new("TRIANGLES", Meshopt.DecodeIndexBuffer);
            public static readonly MeshoptMode Indices = new("INDICES", Meshopt.DecodeIndexBuffer);

            public static readonly MeshoptMode Default = Attributes;

            public partial int Decode(byte* destination, nuint count, nuint stride, byte* source, nuint length);

            public static Option<MeshoptMode> Route(string token) => TryGet(token, out var row) ? Some(row) : None;
        }

        [SmartEnum<string>]
        [UseDelegateFromConstructor]
        public sealed partial class MeshoptFilter {
            public static readonly MeshoptFilter None = new("NONE", static (void* _, nuint _, nuint _) => { });
            public static readonly MeshoptFilter Octahedral = new("OCTAHEDRAL", Meshopt.DecodeFilterOct);
            public static readonly MeshoptFilter Quaternion = new("QUATERNION", Meshopt.DecodeFilterQuat);
            public static readonly MeshoptFilter Exponential = new("EXPONENTIAL", Meshopt.DecodeFilterExp);
            public static readonly MeshoptFilter Colour = new("COLOR", Meshopt.DecodeFilterColor);

            public static readonly MeshoptFilter Default = None;

            public partial void Unfilter(void* buffer, nuint count, nuint stride);

            public static Option<MeshoptFilter> Route(string token) => TryGet(token, out var row) ? Some(row) : None;
        }

        public static bool IsPresent(string json) =>
            KhrExtension.MeshoptCompression.Key is var meshopt
            && KhrExtension.DracoMeshCompression.Key is var draco
            && json.Length > 0
            && (json.Contains(draco, StringComparison.Ordinal) || json.Contains(meshopt, StringComparison.Ordinal));

        public static ModelRoot Decompress(ModelRoot model, string json) {
            var root = JsonNode.Parse(json)!.AsObject();
            Rows(root, "meshes").Iter((m, mesh) => mesh.Iter(entry =>
                toSeq(model.LogicalMeshes[m].Primitives).Iter((p, primitive) =>
                    Reach(entry, "primitives", p, KhrExtension.DracoMeshCompression.Key)
                        .Iter(extension => DracoPrimitive(primitive, extension)))));
            Rows(root, "bufferViews").Iter((v, view) => view
                .Bind(entry => Extension(entry, KhrExtension.MeshoptCompression.Key))
                .Iter(extension => MeshoptView(model, model.LogicalBufferViews[v], extension)));
            return model;
        }

        static Seq<Option<JsonObject>> Rows(JsonObject root, string member) =>
            Optional(root[member]).Bind(static node => Optional(node.AsArray()))
                .Map(static array => toSeq(array).Map(static entry => Optional(entry).Bind(static e => Optional(e.AsObject()))))
                .IfNone(Seq<Option<JsonObject>>());

        static Option<JsonObject> Reach(JsonObject entry, string member, int ordinal, string extension) =>
            Optional(entry[member]).Bind(node => Optional(node.AsArray()))
                .Bind(array => ordinal < array.Count ? Optional(array[ordinal]) : None)
                .Bind(static node => Optional(node.AsObject()))
                .Bind(row => Extension(row, extension));

        static Option<JsonObject> Extension(JsonObject row, string extension) =>
            Optional(row["extensions"]).Bind(node => Optional(node[extension])).Bind(static node => Optional(node.AsObject()));

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

        static unsafe void MeshoptView(ModelRoot model, BufferView view, JsonObject extension) {
            int count = (int)extension["count"]!;
            int stride = (int)extension["byteStride"]!;
            MeshoptMode mode = Token(extension, "mode", MeshoptMode.Route, MeshoptMode.Default);
            MeshoptFilter filter = Token(extension, "filter", MeshoptFilter.Route, MeshoptFilter.Default);
            var compressed = model.LogicalBuffers[(int)extension["buffer"]!].Content;
            int offset = Optional((int?)extension["byteOffset"]).IfNone(0);
            int length = (int)extension["byteLength"]!;
            var destination = new byte[count * stride];
            fixed (byte* dst = destination)
            fixed (byte* src = compressed) {
                int status = mode.Decode(dst, (nuint)count, (nuint)stride, src + offset, (nuint)length);
                if (status != 0) { throw new InvalidDataException(string.Join(':', new object?[] { "import-decode", "meshopt-decode-status", status.ToString(CultureInfo.InvariantCulture) })); }
                filter.Unfilter(dst, (nuint)count, (nuint)stride);
            }
            destination.CopyTo(view.Content.AsSpan(0, destination.Length));
        }

        static TRow Token<TRow>(JsonObject extension, string member, Func<string, Option<TRow>> route, TRow fallback)
            where TRow : class =>
            Optional((string?)extension[member]).Match(
                Some: token => route(token).IfNone(
                    () => throw new InvalidDataException(string.Join(':', new object?[] { "import-decode", $"meshopt-{member}", token }))),
                None: () => fallback);

        public static string JsonChunk(ReadOnlyMemory<byte> glb) {
            using Stream source = glb.AsStream();
            if (!ReadContext.IdentifyBinaryContainer(source)) { return Encoding.UTF8.GetString(glb.Span); }
            source.Position = 0L;
            return ReadContext.ReadJson(source);
        }
    }

    // --- [PLY]
    [SmartEnum<string>]
    [KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
    public sealed partial class PlyLane {
        public static readonly PlyLane X = new("x", EncodingChannel.Position, 0);
        public static readonly PlyLane Y = new("y", EncodingChannel.Position, 1);
        public static readonly PlyLane Z = new("z", EncodingChannel.Position, 2);
        public static readonly PlyLane Nx = new("nx", EncodingChannel.Normal, 0);
        public static readonly PlyLane Ny = new("ny", EncodingChannel.Normal, 1);
        public static readonly PlyLane Nz = new("nz", EncodingChannel.Normal, 2);
        public static readonly PlyLane S = new("s", EncodingChannel.Uv, 0, aliases: Seq("texture_u"));
        public static readonly PlyLane T = new("t", EncodingChannel.Uv, 1, aliases: Seq("texture_v"));
        public static readonly PlyLane Red = new("red", EncodingChannel.ColorRgba, 0);
        public static readonly PlyLane Green = new("green", EncodingChannel.ColorRgba, 1);
        public static readonly PlyLane Blue = new("blue", EncodingChannel.ColorRgba, 2);
        public static readonly PlyLane Alpha = new("alpha", EncodingChannel.ColorRgba, 3, absent: 1f);

        public EncodingChannel Channel { get; }
        public int Ordinate { get; }
        public Seq<string> Aliases { get; }
        public Option<float> Absent { get; }

        public Seq<string> Names => Key.Cons(Aliases);

        public static readonly FrozenDictionary<EncodingChannel, Seq<PlyLane>> ByChannel =
            Items.GroupBy(static row => row.Channel)
                .ToFrozenDictionary(static group => group.Key, static group => toSeq(group).OrderBy(static row => row.Ordinate).ToSeq());
    }

    static Fin<ImportedGeometry> Ply(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at, Option<BimHooks> hooks, Op key) {
        using var stream = new MemoryStream(bytes.ToArray());
        using var draft = MeshDraft.Of();
        var elements = PlyParser.Parse(stream, maxChunkSize: 1 << 20).Data.ToList();
        DecodeStage.Opened.Beat(hooks, key);
        var vertex = elements.First(static d => d.Element.Type == ElementType.Vertex);
        var face = elements.FirstOrDefault(static d => d.Element.Type == ElementType.Face);
        int vertexCount = Resolve(vertex, PlyLane.X).Map(static column => column.Length).IfNone(0);
        Seq<(EncodingChannel Channel, float[] Values)> lanes = toSeq(PlyLane.ByChannel)
            .Choose(pair => Lane(vertex, pair.Key, pair.Value, vertexCount));
        long[] corners = face is null
            ? []
            : ((int[][])face["vertex_indices"].Data)
                .SelectMany(static polygon => Fan(polygon.Length).Map(tri => (polygon, tri)))
                .SelectMany(static entry => new long[] {
                    entry.polygon[entry.tri.A], entry.polygon[entry.tri.B], entry.polygon[entry.tri.C] })
                .ToArray();
        DecodeStage.Decoded.Beat(hooks, key);
        return Baked(draft, vertexCount, lanes, corners, key)
            .Bind(_ => Sealed(draft, format, at, hooks, key));
    }

    static Option<(EncodingChannel Channel, float[] Values)> Lane(
        ElementData vertex, EncodingChannel channel, Seq<PlyLane> rows, int vertexCount) =>
        rows.Traverse(row => Resolve(vertex, row).Match(
                Some: column => Some((Row: row, Read: (int v) => column[v])),
                None: () => row.Absent.Map(fill => (Row: row, Read: (int _) => fill))))
            .Map(resolved => {
                var values = new float[vertexCount * channel.Arity];
                resolved.Iter(lane => {
                    for (int v = 0; v < vertexCount; v++) {
                        values[(v * channel.Arity) + lane.Row.Ordinate] = lane.Read(v);
                    }
                });
                return (channel, values);
            });

    static Option<float[]> Resolve(ElementData vertex, PlyLane row) =>
        row.Names.Find(name => vertex.Element.Properties.Exists(p => p.Name == name))
            .Map(name => row.Channel == EncodingChannel.ColorRgba
                ? ColourScale.TryGetValue(vertex[name].Property.DataType, out float scale)
                    ? Array.ConvertAll(Column(vertex, name), value => value / scale)
                    : Column(vertex, name)
                : Column(vertex, name));

    static float[] Column(ElementData element, string name) => element[name].Data switch {
        float[] f  => f,
        double[] d => Array.ConvertAll(d, static x => (float)x),
        int[] i    => Array.ConvertAll(i, static x => (float)x),
        Array a    => Enumerable.Range(0, a.Length).Select(i => Convert.ToSingle(a.GetValue(i), CultureInfo.InvariantCulture)).ToArray(),
        _          => [],
    };

    static readonly FrozenDictionary<PlyParser.DataType, float> ColourScale = new Dictionary<PlyParser.DataType, float> {
        [PlyParser.DataType.Int8] = 127f, [PlyParser.DataType.UInt8] = 255f,
        [PlyParser.DataType.Int16] = 32767f, [PlyParser.DataType.UInt16] = 65535f,
        [PlyParser.DataType.Int32] = 2147483647f, [PlyParser.DataType.UInt32] = 4294967295f,
        [PlyParser.DataType.Int64] = 9223372036854775807f, [PlyParser.DataType.UInt64] = 18446744073709551615f,
    }.ToFrozenDictionary();

    // --- [SCENE_EXCHANGE]
    static Fin<ImportedGeometry> Scene(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at, Option<BimHooks> hooks, Op key) {
        using var context = new AssimpContext();
        using var stream = new MemoryStream(bytes.ToArray());
        var scene = context.ImportFileFromStream(stream,
            PostProcessSteps.Triangulate | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.GenerateSmoothNormals
                | PostProcessSteps.CalculateTangentSpace | PostProcessSteps.GenerateUVCoords,
            format.Extensions.Head.Map(static ext => ext.TrimStart('.')).IfNone(format.Key));
        DecodeStage.Opened.Beat(hooks, key);
        using var draft = MeshDraft.Of();
        return toSeq(Enumerable.Range(0, scene.MeshCount))
            .Traverse(m => AssimpBlock(draft, scene.Meshes[m], key)).As()
            .Map(blocks => { DecodeStage.Decoded.Beat(hooks, key); return blocks; })
            .Bind(blocks => AssimpWalk(blocks).Accrue(scene.RootNode, draft, key))
            .Map(_ => DecodeStage.Placed.Beat(hooks, key))
            .Bind(_ => Sealed(draft, format, at, hooks, key));
    }

    static SceneWalk<Assimp.Node> AssimpWalk(Seq<int> blocks) => new(
        Flatten: static node => toSeq(node.Children),
        Excluded: static _ => false,
        Placements: static (node, parent) => Seq(parent * Placed(node.Transform)),
        Blocks: (node, _) => Fin.Succ(toSeq(node.MeshIndices).Map(m => blocks[m])));

    static Fin<int> AssimpBlock(MeshDraft draft, Assimp.Mesh mesh, Op key) {
        var positions = new float[mesh.VertexCount * 3];
        var normals = mesh.HasNormals ? new float[mesh.VertexCount * 3] : [];
        bool mapped = mesh.TextureCoordinateChannelCount > 0 && mesh.UVComponentCount[0] >= 2;
        bool painted = mesh.VertexColorChannelCount > 0 && mesh.HasVertexColors(0);
        var uvs = mapped ? new float[mesh.VertexCount * 2] : [];
        var colours = painted ? new float[mesh.VertexCount * 4] : [];
        for (int i = 0; i < mesh.VertexCount; i++) {
            var p = mesh.Vertices[i];
            int v = i * 3;
            (positions[v], positions[v + 1], positions[v + 2]) = (p.X, p.Y, p.Z);
            if (normals.Length > 0) {
                (normals[v], normals[v + 1], normals[v + 2]) = (mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z);
            }
            if (mapped) {
                var uv = mesh.TextureCoordinateChannels[0][i];
                (uvs[i * 2], uvs[(i * 2) + 1]) = (uv.X, uv.Y);
            }
            if (painted) {
                var colour = mesh.VertexColorChannels[0][i];
                (colours[i * 4], colours[(i * 4) + 1], colours[(i * 4) + 2], colours[(i * 4) + 3]) =
                    (colour.X, colour.Y, colour.Z, colour.W);
            }
        }
        long[] corners = mesh.Faces
            .SelectMany(static face => Fan(face.IndexCount).Map(tri => (face, tri)))
            .SelectMany(static entry => new long[] {
                entry.face.Indices[entry.tri.A], entry.face.Indices[entry.tri.B], entry.face.Indices[entry.tri.C] })
            .ToArray();
        return draft.Append(mesh.VertexCount,
            Lanes(positions, normals)
            + (mapped ? Seq((EncodingChannel.Uv, uvs)) : Seq<(EncodingChannel, float[])>())
            + (painted ? Seq((EncodingChannel.ColorRgba, colours)) : Seq<(EncodingChannel, float[])>()),
            corners, key);
    }

    // --- [USD]
    static Fin<ImportedGeometry> Usd(
        InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at, Option<BimHooks> hooks,
        Option<UsdScope> scope, Op key) =>
        use(() => Spooled(bytes, format), path => IO.lift(() => key.Catch(() => {
            using var stage = Staged(path.Value, scope);
            DecodeStage.Opened.Beat(hooks, key);
            bool zUp = UsdGeom.UsdGeomGetStageUpAxis(stage).ToString() == "Z";
            using var draft = MeshDraft.Of();
            var xform = new UsdGeomXformCache();
            var pool = new Dictionary<string, Seq<int>>(StringComparer.Ordinal);
            return UsdWalk(Roster(stage, xform, hooks, key), pool, key).Accrue(Option<UsdNode>.None, draft, key)
                .Map(_ => { DecodeStage.Decoded.Beat(hooks, key); DecodeStage.Placed.Beat(hooks, key); return unit; })
                .Bind(_ => Sealed(draft, format, at, hooks, key))
                .Bind(geometry => zUp ? Fin.Succ(geometry) : Framed(format, geometry, key));
        }))).Run().As();

    static SceneWalk<Option<UsdNode>> UsdWalk(Seq<UsdNode> roster, Dictionary<string, Seq<int>> pool, Op key) => new(
        Flatten: node => node.IsNone ? roster.Map(Some) : Seq<Option<UsdNode>>(),
        Excluded: static _ => false,
        Placements: static (node, _) => node.Map(static held => held.Switch(
            meshed: static mesh => Seq(mesh.World),
            scattered: static scatter => scatter.Worlds)).IfNone(Seq<Transform>()),
        Blocks: (node, draft) => node.Match(
            Some: held => held.Switch(
                meshed: mesh => Pooled(mesh.Prim, draft, pool, key),
                scattered: scatter => Pooled(scatter.Prototype, draft, pool, key)),
            None: () => Fin.Succ(Seq<int>())));

    static Fin<Seq<int>> Pooled(UsdPrim prim, MeshDraft draft, Dictionary<string, Seq<int>> pool, Op key) =>
        prim.GetPath().GetAsString() is var path && pool.TryGetValue(path, out Seq<int> held)
            ? Fin.Succ(held)
            : UsdMesh(new UsdGeomMesh(prim), draft, key).Map(minted => pool[path] = minted);

    static Seq<UsdNode> Roster(UsdStage stage, UsdGeomXformCache xform, Option<BimHooks> hooks, Op key) {
        Seq<string> prototypes = stage.Traverse().AsIterable()
            .Filter(static prim => prim.GetTypeName().ToString() == PointInstancerType)
            .Bind(prim => new UsdGeomPointInstancer(prim).GetPrototypesRel().GetTargets().AsIterable())
            .Map(static target => target.GetAsString())
            .ToSeq().Distinct();
        var unseen = new Unseen();
        return stage.Traverse().AsIterable()
            .Filter(prim => !Prototyped(prim, prototypes))
            .Bind(prim => prim.GetTypeName().ToString() switch {
                MeshType => Seq<UsdNode>(new UsdNode.Meshed(prim, Placed(xform.GetLocalToWorldTransform(prim)))),
                PointInstancerType => Scatter(stage, prim, Placed(xform.GetLocalToWorldTransform(prim))),
                var other => Skipped(other, unseen, hooks, key),
            })
            .ToSeq().Strict();
    }

    static Seq<UsdNode> Skipped(string type, Unseen unseen, Option<BimHooks> hooks, Op key) {
        ignore(unseen.Once(type, hooks, key));
        return Seq<UsdNode>();
    }

    static Seq<UsdNode> Scatter(UsdStage stage, UsdPrim prim, Transform instancerWorld) {
        var instancer = new UsdGeomPointInstancer(prim);
        SdfPathVector protoPaths = instancer.GetPrototypesRel().GetTargets();
        var indexValue = new VtValue();
        instancer.GetProtoIndicesAttr().Get(indexValue, UsdTimeCode.Default());
        var protoIndices = (VtIntArray)indexValue;
        var transforms = new VtMatrix4dArray(protoIndices.size());
        if (!instancer.ComputeInstanceTransformsAtTime(transforms, UsdTimeCode.Default(), UsdTimeCode.Default())) {
            return Seq<UsdNode>();
        }
        return toSeq(Enumerable.Range(0, (int)protoIndices.size()))
            .Filter(i => protoIndices[i] >= 0 && protoIndices[i] < protoPaths.Count)
            .Map(i => (Slot: protoIndices[i], World: instancerWorld * Placed(transforms[i])))
            .GroupBy(static instance => instance.Slot)
            .Choose(group => stage.GetPrimAtPath(protoPaths[group.Key]) is var proto
                && proto.GetTypeName().ToString() == MeshType
                    ? Some((UsdNode)new UsdNode.Scattered(proto, toSeq(group).Map(static instance => instance.World)))
                    : None)
            .ToSeq();
    }

    static UsdStage Staged(string path, Option<UsdScope> scope) =>
        scope.IfNone(UsdScope.Whole).Switch(
            state: path,
            wholeStage: static (root, _) => UsdStage.Open(root, UsdStage.InitialLoadSet.LoadAll),
            populated:  static (root, populated) => Masked(root, populated.Paths));

    static UsdStage Masked(string path, Seq<string> paths) {
        using var addresses = new SdfPathVector(paths.Map(static prim => new SdfPath(prim)));
        using var mask = new UsdStagePopulationMask(addresses);
        return UsdStage.OpenMasked(path, mask, UsdStage.InitialLoadSet.LoadAll);
    }

    static Spool Spooled(ReadOnlyMemory<byte> bytes, InterchangeFormat format) {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{format.Extensions.Head.IfNone(".usd")}");
        File.WriteAllBytes(path, bytes.ToArray());
        return new Spool(path);
    }

    readonly record struct Spool(string Value) : IDisposable {
        public void Dispose() => File.Delete(Value);
    }

    const string MeshType = "Mesh";
    const string PointInstancerType = "PointInstancer";

    static bool Prototyped(UsdPrim prim, Seq<string> prototypes) =>
        prim.GetPath().GetAsString() is var path
        && prototypes.Exists(root => path == root || path.StartsWith($"{root}/", StringComparison.Ordinal));

    static Fin<Seq<int>> UsdMesh(UsdGeomMesh mesh, MeshDraft draft, Op key) {
        var (points, authored, counts, corners) = (new VtValue(), new VtValue(), new VtValue(), new VtValue());
        mesh.GetPointsAttr().Get(points, UsdTimeCode.Default());
        bool hasNormals = mesh.GetNormalsAttr().Get(authored, UsdTimeCode.Default());
        mesh.GetFaceVertexCountsAttr().Get(counts, UsdTimeCode.Default());
        mesh.GetFaceVertexIndicesAttr().Get(corners, UsdTimeCode.Default());
        var (xyz, faceCounts, faceIndices) = ((VtVec3fArray)points, (VtIntArray)counts, (VtIntArray)corners);
        var perVertex = hasNormals && (VtVec3fArray)authored is { } nrm && (int)nrm.size() == (int)xyz.size() ? nrm : null;
        var stValue = new VtValue();
        var st = new UsdGeomPrimvarsAPI(mesh.GetPrim()).GetPrimvar(new TfToken("st"));
        bool mapped = st.HasAuthoredValue() && st.ComputeFlattened(stValue)
            && st.GetInterpolation().ToString() == "vertex"
            && (VtVec2fArray)stValue is { } uvArray && (int)uvArray.size() == (int)xyz.size();
        int faces = (int)faceCounts.size();
        var cursors = new int[faces];
        for (int f = 1; f < faces; f++) { cursors[f] = cursors[f - 1] + faceCounts[f - 1]; }
        UsdGeomSubsetVector subsets = UsdGeomSubset.GetGeomSubsets(mesh, UsdGeomTokens.face, UsdShadeTokens.materialBind);
        Option<string> own = BoundMaterial(mesh.GetPrim());
        Seq<(Seq<int> Faces, Option<string> Material)> subsetGroups =
            toSeq(subsets).Map(static subset => (FaceOrdinals(subset), BoundMaterial(subset.GetPrim())));
        Seq<int> remainder = subsets.Count == 0
            ? toSeq(Enumerable.Range(0, faces))
            : Ordinals(UsdGeomSubset.GetUnassignedIndices(subsets, (uint)faces));
        return subsetGroups.Add((remainder, own))
            .Filter(static group => !group.Faces.IsEmpty)
            .Traverse(group => Partition(draft, group.Faces, group.Material, key)).As();

        Fin<int> Partition(MeshDraft target, Seq<int> group, Option<string> material, Op op) {
            var compact = new OrdinalCompactor(0);
            var tris = new long[group.Fold(0, (sum, f) => sum + Math.Max(0, faceCounts[f] - 2)) * 3];
            int slot = 0;
            foreach (int f in group) {
                int cursor = cursors[f];
                foreach (var (a, b, c) in Fan(faceCounts[f])) {
                    (tris[slot], tris[slot + 1], tris[slot + 2]) = (
                        compact.Slot(faceIndices[cursor + a]),
                        compact.Slot(faceIndices[cursor + b]),
                        compact.Slot(faceIndices[cursor + c]));
                    slot += 3;
                }
            }
            var verts = new float[compact.Count * 3];
            var normals = perVertex is null ? [] : new float[compact.Count * 3];
            var uvs = mapped ? new float[compact.Count * 2] : [];
            for (int local = 0; local < compact.Count; local++) {
                int point = compact.Sources[local];
                var p = xyz[point];
                (verts[local * 3], verts[(local * 3) + 1], verts[(local * 3) + 2]) = (p[0], p[1], p[2]);
                if (normals.Length > 0) {
                    var n = perVertex![point];
                    (normals[local * 3], normals[(local * 3) + 1], normals[(local * 3) + 2]) = (n[0], n[1], n[2]);
                }
                if (mapped) {
                    var uv = ((VtVec2fArray)stValue)[point];
                    (uvs[local * 2], uvs[(local * 2) + 1]) = (uv[0], uv[1]);
                }
            }
            return target.Append(compact.Count,
                Lanes(verts, normals) + (mapped ? Seq((EncodingChannel.Uv, uvs)) : Seq<(EncodingChannel, float[])>()),
                tris, op, material);
        }
    }

    static Seq<int> FaceOrdinals(UsdGeomSubset subset) {
        var indices = new VtValue();
        subset.GetIndicesAttr().Get(indices, UsdTimeCode.Default());
        return Ordinals((VtIntArray)indices);
    }

    static Seq<int> Ordinals(VtIntArray array) =>
        toSeq(Enumerable.Range(0, (int)array.size()).Select(i => array[i]));

    static Option<string> BoundMaterial(UsdPrim prim) =>
        new UsdShadeMaterialBindingAPI(prim).GetDirectBinding().GetMaterialPath() is { } path && !path.IsEmpty()
            ? Some(path.GetAsString())
            : None;

    // --- [DOTBIM]
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
    [JsonSerializable(typeof(dotbim.File), TypeInfoPropertyName = "DotBimFile")]
    public sealed partial class DotBimContext : JsonSerializerContext;

    static Fin<ImportedGeometry> DotBim(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at, Option<BimHooks> hooks, Op key) {
        using var draft = MeshDraft.Of();
        return Optional(JsonSerializer.Deserialize(bytes.Span, DotBimContext.Default.DotBimFile))
            .ToFin(new BimFault.Refused(key, BimScope.Import, BimReason.Rejected, string.Join(':', new object?[] { "import-decode", "dotbim-empty-document" })))
            .Map(file => { DecodeStage.Opened.Beat(hooks, key); return file; })
            .Bind(file => toSeq(file.Meshes)
                .Traverse(mesh => draft.Append(
                        mesh.Coordinates.Count / 3,
                        Lanes(mesh.Coordinates.Select(static c => (float)c).ToArray(), []),
                        mesh.Indices.Select(static i => (long)i).ToArray(), key)
                    .Map(block => (mesh.MeshId, Block: block))).As()
                .Map(pool => { DecodeStage.Decoded.Beat(hooks, key); return (File: file, Pool: toMap(pool)); }))
            .Bind(decoded => toSeq(decoded.File.Elements)
                .Traverse(element => decoded.Pool.Find(element.MeshId)
                    .ToFin(new BimFault.Refused(key, BimScope.Import, BimReason.Rejected, string.Join(':', new object?[] { "import-decode", "dotbim-mesh-miss", element.MeshId.ToString(CultureInfo.InvariantCulture) })))
                    .Bind(block => draft.Place(block, Placed(
                        Matrix4x4.CreateFromQuaternion(new System.Numerics.Quaternion(
                            (float)element.Rotation.Qx, (float)element.Rotation.Qy,
                            (float)element.Rotation.Qz, (float)element.Rotation.Qw))
                        * Matrix4x4.CreateTranslation(
                            (float)element.Vector.X, (float)element.Vector.Y, (float)element.Vector.Z))))).As())
            .Map(_ => DecodeStage.Placed.Beat(hooks, key))
            .Bind(_ => Sealed(draft, format, at, hooks, key));
    }

    // --- [SPECKLE]
    static Fin<ImportedGeometry> DisplayScene(Base root, Instant at, Option<BimHooks> hooks, Op key) {
        using var draft = MeshDraft.Of();
        DecodeStage.Opened.Beat(hooks, key);
        return toSeq(root.Flatten()
                .SelectMany(static node => Optional(node.TryGetDisplayValue()).Map(static d => d.OfType<Mesh>()).IfNone([])))
            .Traverse(mesh => SpeckleBlock(draft, mesh, key)).As()
            .Map(_ => DecodeStage.Decoded.Beat(hooks, key))
            .Bind(_ => Sealed(draft, InterchangeFormat.Glb, at, hooks, key));
    }

    static Fin<int> SpeckleBlock(MeshDraft draft, Mesh mesh, Op key) =>
        Fans(mesh.faces, key).Bind(fans => {
            double scale = Units.GetConversionFactor(mesh.units, Units.Meters);
            bool authored = mesh.vertexNormals.Count == mesh.vertices.Count;
            var positions = new float[fans.Count * 3];
            var normals = authored ? new float[fans.Count * 3] : [];
            var corners = new long[fans.Count];
            for (int i = 0; i < fans.Count; i++) {
                int vertex = fans[i], slot = i * 3;
                (positions[slot], positions[slot + 1], positions[slot + 2]) = (
                    (float)(mesh.vertices[vertex * 3] * scale),
                    (float)(mesh.vertices[(vertex * 3) + 1] * scale),
                    (float)(mesh.vertices[(vertex * 3) + 2] * scale));
                if (authored) {
                    (normals[slot], normals[slot + 1], normals[slot + 2]) = (
                        (float)mesh.vertexNormals[vertex * 3],
                        (float)mesh.vertexNormals[(vertex * 3) + 1],
                        (float)mesh.vertexNormals[(vertex * 3) + 2]);
                }
                corners[i] = i;
            }
            return Baked(draft, fans.Count, Lanes(positions, normals), corners, key);
        });

    static Fin<Seq<int>> Fans(List<int> faces, Op key) {
        var run = Seq<int>();
        for (int cursor = 0; cursor < faces.Count;) {
            int span = faces[cursor] switch { 0 => 3, 1 => 4, var n => n };
            if (span < 3) {
                return Fin.Fail<Seq<int>>(new BimFault.Refused(key, BimScope.Import, BimReason.Rejected, string.Join(':', new object?[] { "import-decode", "speckle-degenerate-face", span.ToString(CultureInfo.InvariantCulture) })));
            }
            foreach (var (a, b, c) in Fan(span)) {
                run = run.Add(faces[cursor + 1 + a]).Add(faces[cursor + 1 + b]).Add(faces[cursor + 1 + c]);
            }
            cursor += span + 1;
        }
        return Fin.Succ(run);
    }

    // --- [ACAD]
    static class AcadReader {
        public static Fin<ImportedGeometry> Read(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at, Option<BimHooks> hooks, Op key) {
            using var stream = new MemoryStream(bytes.ToArray());
            using ICadReader reader = IsDxf(bytes) ? new DxfReader(stream) : new DwgReader(stream);
            reader.OnProgress += (_, args) => {
                if (DecodeStage.ByReadStage.TryGetValue(args.Stage, out DecodeStage? stage)) { stage.Beat(hooks, key); }
            };
            using var draft = MeshDraft.Of();
            var unseen = new Unseen();
            Seq<Cad.Entity> entities = toSeq(reader.Read().Entities);
            return entities
                .Traverse(entity => Accumulate(draft, entity, unseen, hooks, key)).As()
                .Map(_ => {
                    entities.Choose(static entity => Optional(entity.Layer?.Name).Filter(static name => name.Length > 0))
                        .Distinct().Iter(layer => DecodeReason.PresentationDropped.Degrade(hooks, key, layer));
                    return DecodeStage.Placed.Beat(hooks, key);
                })
                .Bind(_ => Sealed(draft, format, at, hooks, key));
        }

        static Fin<Unit> Accumulate(MeshDraft draft, Cad.Entity entity, Unseen unseen, Option<BimHooks> hooks, Op key) =>
            entity switch {
                Cad.Mesh mesh => Block(draft, Faces(mesh.Vertices, mesh.Faces), key),
                Cad.Face3D face => Block(draft, Quad(face.FirstCorner, face.SecondCorner, face.ThirdCorner, face.FourthCorner), key),
                Cad.PolyfaceMesh poly => Block(draft, Polyface(poly), key),
                Cad.Insert insert => toSeq(insert.Explode())
                    .Traverse(placed => Accumulate(draft, placed, unseen, hooks, key)).As().Map(static _ => unit),
                Cad.ModelerGeometry acis => Fin.Succ(DecodeReason.SolidUnevaluated.Degrade(
                    hooks, key, acis.Handle.ToString(CultureInfo.InvariantCulture))),
                var other => Fin.Succ(unseen.Once(other.GetType().Name, hooks, key)),
            };

        static Fin<Unit> Block(MeshDraft draft, (float[] Positions, long[] Corners) block, Op key) =>
            Baked(draft, block.Positions.Length / 3, Lanes(block.Positions, []), block.Corners, key).Map(static _ => unit);

        static bool IsDxf(ReadOnlyMemory<byte> bytes) =>
            bytes.Length >= 4 && !(bytes.Span[0] == (byte)'A' && bytes.Span[1] == (byte)'C' && char.IsDigit((char)bytes.Span[2]));

        static (float[] Positions, long[] Corners) Polyface(Cad.PolyfaceMesh poly) => (
            Triples(poly.Vertices.Select(static v => v.Location).ToList()),
            poly.Faces.SelectMany(static f => {
                long a = Math.Abs(f.Index1) - 1, b = Math.Abs(f.Index2) - 1, c = Math.Abs(f.Index3) - 1;
                return f.Index4 == 0 ? new[] { a, b, c } : new[] { a, b, c, a, c, (long)Math.Abs(f.Index4) - 1 };
            }).ToArray());

        static (float[] Positions, long[] Corners) Faces(
            IReadOnlyList<XYZ> vertices, IReadOnlyList<int[]> faces) => (
            Triples(vertices),
            faces.SelectMany(static face => Fan(face.Length).Map(tri => (face, tri)))
                .SelectMany(static entry => new long[] {
                    entry.face[entry.tri.A], entry.face[entry.tri.B], entry.face[entry.tri.C] })
                .ToArray());

        static (float[] Positions, long[] Corners) Quad(XYZ a, XYZ b, XYZ c, XYZ d) {
            var pool = d.Equals(c) ? new[] { a, b, c } : new[] { a, b, c, d };
            return (Triples(pool), Fan(pool.Length)
                .Bind(static tri => Seq<long>(tri.A, tri.B, tri.C)).ToArray());
        }

        static float[] Triples(IReadOnlyList<XYZ> pool) {
            var positions = new float[pool.Count * 3];
            for (int i = 0; i < pool.Count; i++) {
                var p = pool[i];
                (positions[i * 3], positions[(i * 3) + 1], positions[(i * 3) + 2]) = ((float)p.X, (float)p.Y, (float)p.Z);
            }
            return positions;
        }
    }

    // --- [STEP]
    static partial class StepReader {
        static readonly FrozenSet<string> GeometryTypes = new[] {
            "ADVANCED_BREP_SHAPE_REPRESENTATION", "MANIFOLD_SOLID_BREP", "FACETED_BREP", "SHELL_BASED_SURFACE_MODEL",
            "B_SPLINE_SURFACE", "B_SPLINE_CURVE", "GEOMETRIC_CURVE_SET", "SHAPE_REPRESENTATION", "TESSELLATED_SHAPE_REPRESENTATION",
        }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        static readonly FrozenSet<string> PmiTypes = new[] {
            "DIMENSIONAL_CHARACTERISTIC_REPRESENTATION", "DRAUGHTING_CALLOUT", "ANNOTATION_OCCURRENCE",
            "DATUM", "DATUM_FEATURE", "GEOMETRIC_TOLERANCE", "DIMENSIONAL_SIZE", "DIMENSIONAL_LOCATION",
        }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        // --- [VALUE]
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
            var text = Strip(Encoding.UTF8.GetString(bytes));
            var (header, data) = Sections(text);
            var instances = data.Map(Parse).ToSeq();
            var graph = instances.ToDictionary(static i => i.Id, static i => i);
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

        // --- [TOKENIZE]
        static string Strip(string source) {
            var sink = new StringBuilder(source.Length);
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

        static IEnumerable<int> Bare(string text, int from) {
            bool inString = false;
            for (int i = from; i < text.Length; i++) {
                if (inString) { inString = text[i] != '\''; }
                else if (text[i] == '\'') { inString = true; }
                else { yield return i; }
            }
        }

        static int BareIndexOf(string text, string token, int from) =>
            Bare(text, from).FirstOrDefault(
                i => string.Compare(text, i, token, 0, token.Length, StringComparison.OrdinalIgnoreCase) == 0, -1);

        static (string Header, Seq<string> Data) Sections(string text) {
            int header = BareIndexOf(text, "HEADER", 0);
            int headerEnd = header >= 0 ? BareIndexOf(text, "ENDSEC", header) : -1;
            int data = BareIndexOf(text, "DATA", headerEnd >= 0 ? headerEnd : 0);
            int endsec = data >= 0 ? BareIndexOf(text, "ENDSEC", data) : -1;
            int open = data >= 0 ? BareIndexOf(text, ";", data) : -1;
            string headerBody = header >= 0 && headerEnd > header ? text[header..headerEnd] : "";
            string dataBody = open >= 0 ? text[(open + 1)..(endsec < 0 ? text.Length : endsec)] : "";
            return (headerBody, Statements(dataBody));
        }

        static Seq<string> Statements(string body) {
            var (rows, depth, start) = (Seq<string>(), 0, 0);
            foreach (int i in Bare(body, 0)) {
                char ch = body[i];
                if (ch == '(') { depth++; }
                else if (ch == ')') { depth--; }
                else if (ch == ';' && depth == 0) {
                    var statement = body[start..i].Trim();
                    if (statement.StartsWith('#')) { rows = rows.Add(statement); }
                    start = i + 1;
                }
            }
            return rows;
        }

        // --- [PARSE]
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

        // --- [EXTRACT]
        static StepSemanticModel.DefinitionRow Definition(Instance definition, Dictionary<long, Instance> graph) {
            var formation = Resolve(graph, RefAt(definition.Args, 2));
            return new StepSemanticModel.DefinitionRow(
                definition.Id,
                formation.Map(f => RefAt(f.Args, 2)).IfNone(0L),
                formation.Map(f => Str(f.Args, 0)).IfNone(""),
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

        static string HeaderText(string header, string keyword, int ordinal) =>
            BareIndexOf(header, keyword, 0) is var head and >= 0
            && BareIndexOf(header, "(", head) is var open and >= 0
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
```

## [03]-[EXPLICIT_TESSELLATION]

- Owner: `BimIo.ImportIfcTessellation` the in-process decode of the ALREADY-TESSELLATED IFC representation family — `IfcTriangulatedFaceSet` and `IfcPolygonalFaceSet` over their shared `IfcCartesianPointList3D` coordinate store — onto the same shared `ImportedGeometry` every other arm produces, contributing an `EncodingChannel.Uv` lane from the face set's OWN `HasTextures` texture map and an `EncodingChannel.ColorRgba` lane from its OWN `HasColours` map exactly when it declares one; the colour read composes the `Semantics/appearance#APPEARANCE_PROJECTION` `IndexedColour` value that owns both directions of the per-face radiometry, so this walk declares no colour shape and mints no accessor. `ExplicitTessellation` is the split product pairing that geometry with the `GlobalId` residue the companion still owns, and `Gather` the closed emit discriminant every corner-addressed payload forces.
- Cases: `Gather.Welded` carries the coordinate COUNT and materializes no index array — the packed per-coordinate emit every plain face set wants; `Gather.Unwelded` carries the corner run, one vertex per corner. Cases carry the decision, so the two mirrored ternaries and the per-vertex re-test the retired `bool unweld` needed collapse into one value both the vertex fill and the UV sampler read.
- Entry: `BimIo.ImportIfcTessellation(DatabaseIfc db, IClock clock, Op key)` returns `Fin<ExplicitTessellation>`, walking the live graph once and partitioning every product's representation items into the explicitly-tessellated set this page decodes and the evaluated set the `tessellation#TESSELLATION_BRIDGE` crosses; the caller hands `ExplicitTessellation.Deferred` straight to `TessellationScope.Elements` so `Plan` narrows the companion cross to exactly the products that need an evaluator. Malformed index runs — a corner past the coordinate count, a texture-coordinate index past the vertex list, a colour ordinal past the palette — return `Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Rejected` off `key`, lifted BARE, so every bound belongs to the one typed `Op.Catch` envelope and no read carries a guard of its own.
- Auto: `IfcTessellatedFaceSet` IS explicit mesh data, so evaluating it needs no solid kernel and crossing it to the companion is a round trip that COSTS a whole transport hop and DESTROYS both the IFC-native UV set and the radiometry, neither of which any glTF the companion returns carries. `HasTextures` is a SET of `IfcIndexedTextureMap`, each pairing a `TexCoords` vertex list with the `Maps` list naming WHICH `IfcSurfaceTexture` rows that parameterization serves, so the decode joins the UV set to the appearance roster by texture identity rather than by position. `HasColours` is a SINGLE `IfcIndexedColourMap` binding a palette, a one-based index run with one entry per FACE, and one `Opacity` the schema applies to every face alike; the `IndexedColour` value owns that read whole — unit-valued triples already lowered to scene-linear — so this walk applies no transfer of its own. Both index forms address CORNERS where the coordinate store addresses VERTICES, so ONE gather decision owns them: a face set declaring either emits one vertex per corner, a face set declaring neither keeps the packed emit, and the per-coordinate texture-vertex form lands through the same gather at either length.
- Output: `ExplicitTessellation` carries the decoded `ImportedGeometry`, the decoded product count, the deferred `GlobalId` set, and the bound texture identities — the split evidence a composition reads to know how much of a model needed an evaluator at all, and the reason a texture-bearing or colour-bearing IFC now round-trips its parameterization and its radiometry when the companion path cannot.
- Packages: GeometryGymIFC_Core (`IfcTessellatedFaceSet`/`IfcTextureVertexList`/`IfcIndexedTriangleTextureMap` — the triangulated UV-index payload reached through the `Semantics/appearance#APPEARANCE_PROJECTION` `IfcInternals` capsule under that catalog's `[INTERNAL_ACCESS_LAW]`; the polygonal `IfcTextureCoordinateIndices` row is PUBLIC and needs no capsule, as does the `IfcTextureCoordinate.Maps` bound-texture list; the colour payload crosses through that page's `IndexedColour`), Rasm.Element, Rasm, NodaTime, LanguageExt.Core
- Growth: a new tessellated subtype is one arm on the representation-item dispatch and one row on the corner run; a new attribute lane is one lane entry the SAME walk fills; a corner-indexed presentation payload beyond these joins the existing `Gather` discriminant rather than adding a second emit path, and an n-gon's arity is absorbed by the ONE `Fan` owner before `Slot`; never a second IFC mesh decoder and never an in-process evaluator for a swept, BREP, or voided-face item.
- Boundary: `ImportIfcTessellation` decodes explicit indexed meshes and nothing else. Solids requiring an evaluator route to `tessellation#TESSELLATION_BRIDGE`, whose `TessellationRequest.Plan` door admits only the IFC source the real peer implements. Unsupported face-set subtypes refuse typed rather than yielding an empty product. UV coordinates and colour palettes are carried through the existing `IfcInternals`/`IndexedColour` owners; this page mints neither. Decoded geometry lands the same kernel accumulator as every managed arm, and IFC coordinates remain in the declared model frame. Texture payload remains the appearance roster's; this owner carries only coordinates and the texture identity they bind to.

```csharp
public sealed record ExplicitTessellation(
    ImportedGeometry Geometry, int DecodedProducts, Seq<string> Deferred, Seq<string> Textures);

public static partial class BimIo {
    [Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
    public abstract partial record Gather {
        private Gather() { }

        public sealed record Welded(int Count) : Gather;
        public sealed record Unwelded(long[] Corners) : Gather;

        public int Length => Switch(welded: static w => w.Count, unwelded: static u => u.Corners.Length);
        public bool Indexed => Switch(welded: static _ => false, unwelded: static _ => true);
        public int Source(int slot) => Switch(welded: _ => slot, unwelded: u => (int)u.Corners[slot]);
    }

    public static Fin<ExplicitTessellation> ImportIfcTessellation(DatabaseIfc db, IClock clock, Op key) =>
        Boundary(key, () => Partition(db, clock, key));

    static Fin<ExplicitTessellation> Partition(DatabaseIfc db, IClock clock, Op key) {
        using var draft = MeshDraft.Of();
        return toSeq(db.Project.Extract<IfcProduct>())
            .Fold(Fin.Succ((Deferred: Seq<string>(), Textures: Seq<string>(), Decoded: 0)), (acc, product) =>
                acc.Bind(split => Items(product) is var items && items.IsEmpty
                    ? Fin.Succ(split)
                    : !items.ForAll(static item => item is IfcTessellatedFaceSet set && Fannable(set))
                        ? Fin.Succ(split with { Deferred = split.Deferred.Add(product.GlobalId) })
                        : items.Traverse(item => Decode(draft, (IfcTessellatedFaceSet)item, key)).As()
                            .Map(bound => split with {
                                Textures = split.Textures + bound.Choose(identity),
                                Decoded = split.Decoded + 1,
                            })))
            .Bind(split => Sealed(draft, InterchangeFormat.Ifc, clock.GetCurrentInstant(), None, key)
                .Map(geometry => new ExplicitTessellation(
                    geometry, split.Decoded, split.Deferred, split.Textures.Distinct())));
    }

    static Seq<IfcRepresentationItem> Items(IfcProduct product) =>
        Optional(product.Representation).Match(
            None: () => Seq<IfcRepresentationItem>(),
            Some: static shape => shape.Representations.AsIterable().Bind(static rep => rep.Items.AsIterable()).ToSeq());

    static bool Fannable(IfcTessellatedFaceSet faceSet) =>
        faceSet is not IfcPolygonalFaceSet poly || poly.Faces.All(static face => face is not IfcIndexedPolygonalFaceWithVoids);

    static Fin<Option<string>> Decode(MeshDraft draft, IfcTessellatedFaceSet faceSet, Op key) =>
        Corners(faceSet, key).Bind(mesh => {
            var points = Coordinates(faceSet);
            var authored = Normals(faceSet);
            Option<Seq<(int A, int B, int C)>> normalRun = NormalIndex(faceSet);
            Option<IndexedColour> colour = IndexedColour.Of(faceSet);
            Option<UvPlan> uv = Uv(faceSet, mesh.Corner.Length / 3, points.Count);
            Gather gather = colour.IsSome || normalRun.IsSome || uv.Exists(static plan => plan.Indexed)
                ? new Gather.Unwelded(mesh.Corner)
                : new Gather.Welded(points.Count);
            var positions = new float[gather.Length * 3];
            var normals = authored.IsSome ? new float[gather.Length * 3] : [];
            for (int v = 0; v < gather.Length; v++) {
                int source = gather.Source(v);
                (positions[v * 3], positions[(v * 3) + 1], positions[(v * 3) + 2]) = points[source];
                if (normals.Length > 0) {
                    var n = normalRun.Match(
                        Some: run => authored.Map(store => store[Slot(run[v / 3], v) - 1]),
                        None: () => authored.Map(store => store[source])).IfNone((0f, 0f, 1f));
                    (normals[v * 3], normals[(v * 3) + 1], normals[(v * 3) + 2]) = n;
                }
            }
            float[] uvs = uv.Map(plan => Sampled(plan, gather)).IfNone([]);
            float[] paint = colour.Map(read => Painted(read, mesh.Face, gather.Length)).IfNone([]);
            long[] corners = gather.Indexed ? Identity(gather.Length) : mesh.Corner;
            return Baked(draft, gather.Length,
                    Lanes(positions, normals)
                    + (uvs.Length > 0 ? Seq((EncodingChannel.Uv, uvs)) : Seq<(EncodingChannel, float[])>())
                    + (paint.Length > 0 ? Seq((EncodingChannel.ColorRgba, paint)) : Seq<(EncodingChannel, float[])>()),
                    corners, key)
                .Map(_ => uv.Bind(static plan => plan.Texture));
        });

    readonly record struct UvPlan(Seq<(int A, int B, int C)> Index, Seq<(double U, double V)> List, Option<string> Texture) {
        public bool Indexed => !Index.IsEmpty;
    }

    static long[] Identity(int count) {
        long[] run = GC.AllocateUninitializedArray<long>(count);
        for (int i = 0; i < count; i++) { run[i] = i; }
        return run;
    }

    static float[] Painted(IndexedColour colour, int[] faces, int vertexCount) {
        var lane = new float[vertexCount * 4];
        for (int v = 0; v < vertexCount; v++) {
            (double r, double g, double b, double a) = colour.Rgba(faces[v / 3]);
            (lane[v * 4], lane[(v * 4) + 1], lane[(v * 4) + 2], lane[(v * 4) + 3]) = ((float)r, (float)g, (float)b, (float)a);
        }
        return lane;
    }

    static Option<UvPlan> Uv(IfcTessellatedFaceSet faceSet, int triangles, int coordinateCount) =>
        faceSet.HasTextures.AsIterable()
            .Choose(map => Optional(map.TexCoords)
                .Map(list => new UvPlan(
                    Index(faceSet, map),
                    toSeq(list.TexCoordsList).Map(static uv => (U: uv.Item1, V: uv.Item2)),
                    map.Maps.AsIterable().Head.Map(static texture =>
                        texture.StepId.ToString(CultureInfo.InvariantCulture)))))
            .Filter(plan => plan.Indexed ? plan.Index.Count == triangles : plan.List.Count == coordinateCount)
            .Head;

    static Seq<(int A, int B, int C)> Index(IfcTessellatedFaceSet faceSet, IfcIndexedTextureMap map) => (faceSet, map) switch {
        (IfcTriangulatedFaceSet, IfcIndexedTriangleTextureMap triangle) => IfcInternals.TexCoordRun(triangle),
        (IfcPolygonalFaceSet poly, _) => Fan(poly)
            .Traverse(static tri => Optional(tri.Face.HasTexCoords)
                .Map(row => (A: row.TexCoordIndex[tri.I0], B: row.TexCoordIndex[tri.I1], C: row.TexCoordIndex[tri.I2])))
            .As().IfNone(Seq<(int, int, int)>()),
        _ => Seq<(int A, int B, int C)>(),
    };

    static float[] Sampled(UvPlan plan, Gather gather) {
        var uvs = new float[gather.Length * 2];
        for (int v = 0; v < gather.Length; v++) {
            (double s, double t) = plan.List[plan.Indexed ? Slot(plan.Index[v / 3], v) - 1 : gather.Source(v)];
            (uvs[v * 2], uvs[(v * 2) + 1]) = ((float)s, (float)t);
        }
        return uvs;
    }

    static int Slot((int A, int B, int C) triple, int vertex) =>
        (vertex % 3) switch { 0 => triple.A, 1 => triple.B, _ => triple.C };

    static IReadOnlyList<(float X, float Y, float Z)> Coordinates(IfcTessellatedFaceSet faceSet) =>
        faceSet.Coordinates is IfcCartesianPointList3D list
            ? list.CoordList.ConvertAll(static p => ((float)p.Item1, (float)p.Item2, (float)p.Item3))
            : [];

    static Option<IReadOnlyList<(float X, float Y, float Z)>> Normals(IfcTessellatedFaceSet faceSet) =>
        faceSet is IfcTriangulatedFaceSet { Normals.Count: > 0 } tri
            ? Some<IReadOnlyList<(float X, float Y, float Z)>>(
                tri.Normals.ConvertAll(static n => ((float)n.Item1, (float)n.Item2, (float)n.Item3)))
            : None;

    static Option<Seq<(int A, int B, int C)>> NormalIndex(IfcTessellatedFaceSet faceSet) =>
        faceSet is IfcTriangulatedFaceSet { NormalIndex.Count: > 0 } tri
            ? Some(toSeq(tri.NormalIndex).Map(static t => (A: t.Item1, B: t.Item2, C: t.Item3)))
            : None;

    static Fin<(long[] Corner, int[] Face)> Corners(IfcTessellatedFaceSet faceSet, Op key) => faceSet switch {
        IfcTriangulatedFaceSet tri => Fin.Succ((
            (long[])[.. tri.CoordIndex.SelectMany(t => new long[] {
                Point(tri.PnIndex, t.Item1), Point(tri.PnIndex, t.Item2), Point(tri.PnIndex, t.Item3) })],
            (int[])[.. Enumerable.Range(0, tri.CoordIndex.Count)])),
        IfcPolygonalFaceSet poly => Fin.Succ(Fanned(Fan(poly), poly.PnIndex)),
        var other => Fin.Fail<(long[], int[])>(
            new BimFault.Refused(key, BimScope.Tessellation, BimReason.Rejected, string.Join(':', new object?[] { "ifc-tessellation", "face-set-subtype", other.GetType().Name }))),
    };

    static (long[] Corner, int[] Face) Fanned(
        Seq<(IfcIndexedPolygonalFace Face, int Ordinal, int I0, int I1, int I2)> fan,
        IReadOnlyList<int> pnIndex) => (
        [.. fan.Bind(tri => Seq(
            Point(pnIndex, tri.Face.CoordIndex[tri.I0]),
            Point(pnIndex, tri.Face.CoordIndex[tri.I1]),
            Point(pnIndex, tri.Face.CoordIndex[tri.I2])))],
        [.. fan.Map(static tri => tri.Ordinal)]);

    static long Point(IReadOnlyList<int> pnIndex, int index) =>
        pnIndex is { Count: > 0 } ? pnIndex[index - 1] - 1 : index - 1;

    static Seq<(IfcIndexedPolygonalFace Face, int Ordinal, int I0, int I1, int I2)> Fan(IfcPolygonalFaceSet poly) =>
        toSeq(poly.Faces).Map(static (face, ordinal) => (Face: face, Ordinal: ordinal))
            .Bind(static entry => Fan(entry.Face.CoordIndex.Count)
                .Map(tri => (entry.Face, entry.Ordinal, I0: tri.A, I1: tri.B, I2: tri.C)));
}
```

## [04]-[RESEARCH]

(none)
