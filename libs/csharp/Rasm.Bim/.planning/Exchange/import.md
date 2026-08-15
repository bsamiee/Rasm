# [BIM_IMPORT_RAIL]

`BimIo` owns foreign-bytes ingest: one import fold lowers every `format#FORMAT_AXIS` `InterchangeFormat` row to a canonical carrier — managed mesh to the pooled `ImportedGeometry`, IFC/IFC5 — and the SAF structural workbook, by GeometryGym authoring — to the live `DatabaseIfc`, STEP to `StepSemanticModel`, the Speckle `Base` seam to both. Byte->carrier decode is the rail's only concern; the entity walk is the `Rasm.Element` seam projector's off the live graph, never a lossy `IfcSemanticModel` flat-row re-projection. No BRep/NURBS evaluates in-process — a non-mesh geometry request routes to `tessellation#TESSELLATION_BRIDGE`.

`ImportedGeometry` is the seam `Rasm.Element/Projection/projection#INTERCHANGE_CARRIER` mesh POOL this rail produces: `Blocks` ranges hold each decoded source mesh once and `Instances` place them by rigid transform, so an instanced source round-trips its sharing to `export#EXPORT_RAIL` instead of N baked copies and `Bake()` flattens on demand.

Every arm contributes the attribute LANES its own format declares — parameterization and radiometry off the format's own channels — at the ONE `Encode.Of` mint per decode, so the Compute tile partition, the residency meshlet arm, and the `export#EXPORT_RAIL` texture binding read a REAL unwrap and a REAL vertex colour with no re-import edge. Arms whose source declares neither leave the lane ABSENT — a missing descriptor, the arena's own typed absence — because a zero-filled lane is a forged unwrap no consumer can tell from an authored one.

This page composes kernel `Rasm` geometry and consumes the `format#FORMAT_AXIS` codec/frame rows as settled vocabulary. Posture stays HOST-LOCAL: the Speckle seam composes `Speckle.Sdk`/`Speckle.Objects` only in the host-neutral exchange assembly, never inside the in-Rhino plugin ALC.

## [01]-[INDEX]

- [02]-[IMPORT_RAIL]: foreign-bytes ingest — managed mesh to pooled `ImportedGeometry`, IFC and the SAF-authored structural workbook to live `DatabaseIfc`, STEP to `StepSemanticModel`.
- [03]-[SPECKLE_SEAM]: Speckle `Base` object-graph — display-mesh decode to `ImportedGeometry`, host-object projection to a seam `GraphDelta`.
- [04]-[REIMPORT]: projector-polymorphic re-ingest — reconcile a re-projected source to prior `ElementGraph` by `ExternalId`, emit the delta `GraphDelta`; the `TypeCandidate` reverse export off the unreconciled ingested type objects.

## [02]-[IMPORT_RAIL]

- Owner: `BimIo` — the import fold over `InterchangeFormat`, one `InterchangeCodec`-keyed arm per managed decode. Three canonical carriers: the seam `Rasm.Element/Projection/projection#INTERCHANGE_CARRIER` `ImportedGeometry` mesh-POOL this rail produces (one kernel `EncodedGeometry` arena beside `Indices` holds each source mesh once as a `MeshBlock` range, `MeshInstance` rows place blocks by rigid transform, the seam `Bake()` flattens on demand), the live `DatabaseIfc` the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector` captures and lowers to a seam `GraphDelta`, and `StepSemanticModel` the ISO 10303 product-structure projection. `UsdScope` is the stage-population vocabulary the USD arm opens under.
- Entry: `ImportGeometry` (managed mesh-and-scene → `ImportedGeometry`), `ImportIfc` (in-process IFC/IFC5 → live `DatabaseIfc`; its `InterchangeCodec.Saf` arm authors the SAF structural workbook onto a fresh SI-declared database through the `Model/structural#STRUCTURAL_PROJECTION` import leg, the wired `SafServices` triple and the degrade-channel hook carrier riding optional parameters), and `ImportStep` (ISO 10303-21 Part-21 → `StepSemanticModel`), each dispatching by `InterchangeCodec` so a path lands one decode without a call-site type branch. `Fin<T>` aborts on `Model/faults#FAULT_BAND` `BimFault.CodecReject` or the companion-required `BimFault.CapabilityMiss`, each `Op`-keyed case lifting BARE (band 2600 IS the `Expected` `Code`, no `.ToError()` hop). `ImportGeometry` takes the optional `Model/observability#HOOK_RAIL` `BimHooks` carrier and hands it to EVERY managed arm, and the optional `UsdScope` the USD arm opens its stage under — an absent scope is the whole stage, so the unscoped call is unchanged.
- Auto: glTF decode routes binary GLB (`ModelRoot.ParseGLB`) and text `.gltf` (`ReadContext.ReadTextSchema2`) by format with zero intermediate file, a `Decompress` pre-decode branch reading each primitive's `KHR_draco_mesh_compression` and each bufferView's `EXT_meshopt_compression` extension before the `LogicalMeshes.Decode()` fold. IFC decode constructs the live `DatabaseIfc` by the row's STEP/XML/JSON serialization at the schema `SemanticProjector.Sniff` reads off the bytes, never a hardcoded default; the entity walk off that live graph is the projector's, never a lossy `IfcSemanticModel` flat-row re-projection. Every managed arm beats the shared `DecodeStage` ladder onto `rasm.bim.exchange.progress` at its own phase boundaries — the ladder declares the phase fractions ONCE and the ACadSharp arm folds its package-published `ReadStage` onto those same rows, so one lane's foreign progress source never becomes a second fraction vocabulary.
- Receipt: `ModelLoad` carries the format key, codec key, source byte count, and elapsed for a managed mesh import, an instanced source also reading the carrier's `Blocks.Count`/`Instances.Count` sharing evidence; an IFC decode stamps the schema version (`db.Release`) and model-view (`db.ModelView`) off the live `DatabaseIfc` (the entity-count receipt rides the `SemanticProjector` delta, not the import rail); a STEP ingest stamps the `StepProtocol`, `FILE_SCHEMA` name, and product/definition/assembly/geometry-ref counts; emission rides the sink port at the composition edge.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Runtime, GeometryGymIFC_Core, StructuralAnalysisFormat, Openize.Drako, Alimer.Bindings.MeshOptimizer, CommunityToolkit.HighPerformance, geometry3Sharp, Ply.Net, AssimpNetter, UniversalSceneDescription, ACadSharp, dotbim, NodaTime, LanguageExt.Core, Rasm
- Growth: a new managed import is one codec arm on the import fold keyed by the `InterchangeFormat.Codec` row, taking the hook carrier and beating the shared `DecodeStage` rows at its own phase boundaries — a new phase is one row on that ladder, never a per-arm fraction table; a new populated-source scope is one `UsdScope` case (an exclude-by-path polarity, a variant selection) the `Staged` open reads, never a filter-mode flag beside the value and never a post-open prim filter; a new instancing-bearing source is `Append`/`Place` calls inside its one arm and a new material-splitting source one `MeshChunk` per partition carrying its own `Material` key — the `Blocks`/`Instances` overlay is format-agnostic, so no carrier edit and no second soup; a new per-vertex attribute is one `EncodingChannel` row and one `MeshChunk.Attributes` entry in whichever arms read it — the pool fold strides on the channel's own arity, so the builder, the mint, and every other arm stay untouched; a new extracted IFC entity family is one `Extract<T>` arm on the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector`, never on the import rail (which owns only the byte->`DatabaseIfc` decode); a new extracted STEP entity family is one `Keyword`-filtered projection on `StepSemanticModel` over the resolved instance graph; a new STEP application protocol is one `InterchangeFormat` row carrying its `StepProtocol` discriminant — the single `StepReader` reads the protocol off `format.StepProtocol` and the entity-instance grammar is protocol-agnostic, so AP203/AP214/AP242 share one reader and one codec without a per-protocol reader; a new glTF compression codec is one `KhrEncoder`-keyed arm on the `Decompress` pre-decode branch symmetric to the `export#EXPORT_RAIL` `GlbBytes` compression switch, never a second importer.
- Boundary: `BimIo` is the page boundary capsule — leaked package types (`Ply.Net.*`, `Assimp.*`, `pxr.*`, the `SWIGTYPE_p_*`/`*PINVOKE` USD interop) never cross past `Exchange/import`, internal code holding the canonical carriers per the boundary-mapping law. Each decode arm materializes ONE contiguous `ImportedGeometry` allocation — the accessor contracts (`IMeshPrimitiveDecoder`, `DMesh3`, `Ply.Net`, Assimp `Scene`, the USD typed-array bridge) admit no zero-copy span into package buffers, so the one boundary materialization is the allocation point, never a per-primitive proliferation. Decoded attributes land on the unit-valued domain their seam channel stores — a PLY colour column divides by the full scale its DECLARED width names, never by a scale inferred from the values, because a dark scan and a float writer's output are indistinguishable by inspection and guessing there blackens every such delivery. `DotbimProjector` lands each element's display-referred `Color` on the seam appearance path the `export#EXPORT_RAIL` counterpart writes from, decoding it through the `Semantics/appearance#APPEARANCE_PROJECTION` transfer pair into an `AppearanceSummary` and binding a content-keyed `Node.Appearance` by an `Associate` edge, so the round trip closes on ONE curve and one carrier; re-reading the colour into a `PropertyValue.Text` hex row beside a summary-sourced export is the deleted asymmetry, and it lost a Rasm-authored `.bim`'s own colour on re-ingest. `.bim` byte admission reads through a source-generated `JsonSerializerContext` declaring `dotbim.File` as an EXTERNAL serializable root, so no reflection-mode `Deserialize<T>` survives a trimmed or AOT publish. Codec ownership is fixed: `mesh-text` is `geometry3Sharp` ONLY (OBJ/STL/OFF), PLY the dedicated `ply-net`, FBX/Collada/3MF the `scene-exchange` `AssimpNetter` (the one owner, shipping its own osx-arm64 `libassimp.dylib`), USD the `usd-stage`. `Mesh` AND `PointInstancer` prims both admit on the USD arm, because USD expresses repetition natively and a Mesh-only filter imports a point-instanced site or facade delivery EMPTY; the instancer's own `ComputeInstanceTransformsAtTime` composes each instance matrix (positions, orientations, scales, the prototype's own xform, the `invisibleIds` mask), so a hand-multiplied transform triple is the deleted re-derivation, and prototype subtrees are excluded from the mesh pass so a stage authoring them as ordinary prims does not bake the scene twice. USD carries a multi-material mesh as material-bound `UsdGeomSubset` children over face ordinals, so the mesh decode partitions on the AUTHORED subsets: one block per subset stamping the seam `MeshBlock.Material` key off that subset's own direct binding, with one further block over the remainder `GetUnassignedIndices` names — reading the subsets alone drops every uncovered face and reading the whole mesh strands the split, and each partition compacts to the points its own faces reference so two subsets of one mesh land as disjoint blocks rather than two copies of the point array. Stage population is decided AT the open through `UsdScope` and `UsdStage.OpenMasked` over a `UsdStagePopulationMask` built from the scope's admitted prim paths — a post-open traversal filter is the deleted form, because it pays the whole layer stack's composition and prim indexing before discarding it, which is the entire cost a scoped read of a federated site delivery exists to avoid; the scope admits its paths ONCE (absolute prim paths under the package's own `SdfPath` grammar, an empty run meaning the whole stage) so the mask build is total, and everything below the open — the prototype exclusion, the subset partition, the pooled `Declared` lane evidence — reads whatever prims the stage holds and carries no scope arm. IFC decodes ONLY the live `DatabaseIfc`; the entity walk and seam projection are the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector`'s (it captures `DatabaseIfc` internally, so GeometryGym never crosses `IElementProjection.Project`), the lossy `IfcSemanticModel` flat-row re-projection is the deleted form, and GeometryGym carries no tessellation kernel so an IFC geometry request routes to `tessellation#TESSELLATION_BRIDGE`, never a BRep evaluated in-process. STEP splits two legs: the managed semantic-graph leg in-process through the BCL-only `StepReader`, the B-rep/NURBS geometry leg companion-routed so `TessellationRequiresCompanion` stays `true` — no managed Part-21 reader admits, and GeometryGym is IFC-schema-bound so it grounds no STEP semantic leg. The SAF row admits on the IFC entrypoint because its carrier IS the live `DatabaseIfc`: the XLSX bytes decode and validate through the ONE `Model/structural#STRUCTURAL_PROJECTION` `StructuralProjection.Saf` import leg, the validated `ExcelModel` AUTHORS GeometryGym structural entities onto a fresh SI-declared database through the one `Author(db, host, model, key)` overload the `SemanticProjector` then ingests — a SAF-side projector minting seam member nodes is the deleted standalone-projector form — the authoring residue fires the `saf-residue` degrade row once per uncarried payload so no drop is silent, and the SAF service CONTRACTS cross only as the wired `SafServices` dependency surface while the `ExcelModel` rows stay behind the arm.

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
using ACadSharp.IO;                            // ICadReader/DxfReader/DwgReader + the ProgressEventArgs/ReadStage progress pair
using Assimp;
using GeometryGym.Ifc;
using g3;
using LanguageExt;
using NodaTime;
using Ply.Net;
using pxr;
using Rasm;
using Rasm.Drawing;                            // EncodingChannel — the kernel lane vocabulary the pool fold strides on
using Rasm.Bim.Model;
using Rasm.Bim.Projection;                     // SemanticProjector — the Sniff schema owner and the TypeSignatureSet bag symbol
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Cad = ACadSharp.Entities;                // the DWG/DXF entity family QUALIFIES: its Mesh collides with the imported Assimp.Mesh
using SAF.DataAccess.Contracts;                // IExcelImportService/IExcelExportService/IExcelValidator — the SAF codec's
                                               // service triple the SafServices carrier bundles for the ImportIfc Saf arm
using SharpGLTF.Schema2;
using Thinktecture;
using static LanguageExt.Prelude;
using Matrix4x4 = System.Numerics.Matrix4x4;   // the instance-transform currency — disambiguated from Assimp.Matrix4x4
using Node = Rasm.Element.Graph.Node;          // the seam node union owns the bare name; the SharpGLTF scene node is qualified
using Vector3 = System.Numerics.Vector3;       // the numerics coordinate this boundary fold speaks — never the seam Rasm.Element.Graph.Vector3
using GGRelease = GeometryGym.Ifc.ReleaseVersion;   // IFC-text codec schema token the DatabaseIfc ctor takes; the
                                                   // seam Rasm.Element.Graph.ReleaseVersion owns the bare name

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// WHICH prim subtrees a USD stage populates, on the `tessellation#TESSELLATION_BRIDGE` `TessellationScope`
// precedent: the case IS the modality, so no populate-mode flag rides beside the value and no second scope
// vocabulary grows for the next masked source. An unscoped stage composes and prim-indexes the WHOLE layer
// stack, so a site delivery whose consumer wants one storey pays for every building first — the mask moves that
// cost to the open. Paths admit ONCE through `Of` against the package's own path grammar, so the arm's mask
// build is total; the admitted value carries plain strings because an `SdfPath` is a native SWIG handle whose
// lifetime belongs inside the decode's own `using` window, never on a carrier a caller holds.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UsdScope {
    private UsdScope() { }

    public sealed record WholeStage : UsdScope;
    public sealed record Populated(Seq<string> Paths) : UsdScope;

    public static readonly UsdScope Whole = new WholeStage();

    // Empty runs ARE the whole stage: a mask over nothing populates nothing, which reaches a caller as a
    // silently empty import rather than as a refusal. Every other run gates each path absolute-and-prim through the
    // package's own grammar, so a property path, a relative path, or a malformed string is refused at admission
    // instead of composing to an empty stage a consumer reads as a source with no geometry.
    public static Fin<UsdScope> Of(Seq<string> paths, Op key) =>
        paths.IsEmpty
            ? Fin.Succ(Whole)
            : paths.Find(static candidate => !Populates(candidate)).Match(
                Some: refused => Fin.Fail<UsdScope>(Detail.UsdScopePath.At(key, refused)),
                None: () => Fin.Succ<UsdScope>(new Populated(paths.Distinct())));

    // Native path handle — lifetime is a `using` statement, the named boundary exemption.
    static bool Populates(string candidate) {
        if (!SdfPath.IsValidPathString(candidate, out string _)) { return false; }
        using var path = new SdfPath(candidate);
        return path.IsAbsolutePath() && path.IsPrimPath();
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
// ImportedGeometry — the seam `Rasm.Element/Projection/projection#INTERCHANGE_CARRIER` mesh-POOL carrier
// (with `MeshBlock`/`MeshInstance` and the one `Bake` flatten) — is PRODUCED by this rail, and the Compute tile
// partition reads the SAME shape, so no package-local twin exists; every decode arm constructs it through
// `MeshSoup.ToGeometry`, `FormatKey` carrying the `format#FORMAT_AXIS` row key this rail re-hydrates on egress.

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

// The SAF codec's dependency surface, wired ONCE by the composition: the package resolves its service impls
// behind SAF.Infrastructure's own bootstrapper (the ref surface ships contracts alone, so no rail constructs an
// implementation), Target is the caller-selected SAF schema version every SafOp carries, and Schema the IFC
// release the import authoring targets — both stated by the composition, never defaulted here. ONE carrier
// serves BOTH directions: the ImportIfc Saf arm and the export#SAF_EMIT lowering read the same triple, so the
// wiring exists exactly once and the two legs can never disagree on codec, version, or validator.
public sealed record SafServices(
    IExcelImportService Imports, IExcelExportService Exports, IExcelValidator Validator,
    Version Target, GGRelease Schema);

public static partial class BimIo {
    // ONE declared decode-stage ladder every managed arm beats against, so the `rasm.bim.exchange.progress` observe
    // point carries a MEASURED position from every long decode rather than from the one lane whose package publishes
    // its own phases. Each row is a phase a byte->carrier decode genuinely passes, so an arm publishes existing rows
    // and a new phase is one row HERE — a per-arm fraction table is the form that let five arms discard the hook
    // carrier silently. The roster is a [SmartEnum] rather than a record struct so its rows carry the foreign-phase
    // correspondence column below and read identically to the energy lane's own TranslateStage ladder.
    [SmartEnum]
    public sealed partial class DecodeStage {
        public static readonly DecodeStage Opened = new(done: 0.00, witness: "opened", read: ReadStage.Read);          // container parsed, geometry not yet read
        public static readonly DecodeStage Decoded = new(done: 0.45, witness: "decoded", read: ReadStage.Build);       // every source mesh read onto its own block
        public static readonly DecodeStage Placed = new(done: 0.80, witness: "placed", read: null);                    // every instance transform recorded
        public static readonly DecodeStage Assembled = new(done: 1.00, witness: "assembled", read: null);              // the one kernel arena minted

        public double Done { get; }
        public string Witness { get; }

        // Read names the ACadSharp phase this row COMPLETES, null where no foreign phase maps onto it; the
        // correspondence is a COLUMN on the roster rather than a side dictionary beside one arm: a lane whose package publishes
        // its own phases states the mapping where the fractions live, and a foreign phase no row claims publishes
        // NOTHING, because a StageMark carries a measured position by construction and a zero standing in for an
        // unmeasured phase is the deleted form.
        public ReadStage? Read { get; }

        // Rows PROJECT the shared Model/observability#HOOK_RAIL carrier through one member, exactly as the energy
        // translate lane's own roster does, so the two native lanes read identically at their fire sites.
        public StageMark Mark => new(Done, Witness);

        // Beat is a no-op on a hook-less composition, so an arm threads the carrier with no per-arm hook branch.
        public Unit Beat(Option<BimHooks> hooks, Op key) =>
            hooks.IfSome(h => ignore(h.ExchangeProgress.Fire(new BimFact.Progress(key, "exchange", Mark))));

        // Foreign-phase index DERIVED from the column, so the roster is the one place the correspondence lives.
        public static readonly FrozenDictionary<ReadStage, DecodeStage> ByReadStage =
            Items.Where(static row => row.Read is not null)
                .ToFrozenDictionary(static row => row.Read!.Value, static row => row);
    }

    // Decode-degrade vocabulary: a payload an arm READ and deliberately did not decode. A degrade is neither a
    // fault (the import succeeds) nor progress (it carries no position), so it fires the Model/observability
    // #HOOK_RAIL `rasm.bim.exchange.degrade` observe point the SAME hook carrier every arm already threads — one
    // evidence channel, best-effort, shielding a subscriber fault into the registry cell exactly as the progress
    // point does. A silent drop is the deleted form: a DWG whose whole content is ACIS solids imports EMPTY, and
    // without a row the receipt reads identical to a file that carried no geometry at all.
    [SmartEnum<string>]
    [KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
    [KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
    public sealed partial class DecodeReason {
        public static readonly DecodeReason SolidUnevaluated = new("solid-unevaluated");
        // One row per SAF payload the workbook authoring could not carry — sealed subsoil/deformation payloads,
        // IFC-counterpartless rigid relations, linearized constraints — the structural Author residue surfaced on
        // the rail's own evidence channel instead of dropping on the floor.
        public static readonly DecodeReason SafResidue = new("saf-residue");

        public Unit Degrade(Option<BimHooks> hooks, Op key, string subject) =>
            hooks.IfSome(h => ignore(h.ExchangeDegrade.Fire(new BimFact.Degraded(key, "exchange", Key, subject))));
    }

    // Capability is the ONE format#FORMAT_AXIS gate — InterchangeFormat.Admitted reads the catalogue-pending state, the
    // companion binding, and the direction column off the row, so this entrypoint carries no pending-then-
    // capability ladder of its own and cannot re-order it away from its sibling entrypoints. Past the gate the
    // TOTAL generated InterchangeCodec Switch dispatches every codec: the managed-mesh codecs decode inline, the
    // IFC/STEP codecs — and the SAF row, whose carrier is the live DatabaseIfc — name their own entrypoint, the
    // geospatial/point-cloud/energy codecs name their owning page, the companion codecs route to the bridge. This Switch has NO silent fallthrough, so a new InterchangeCodec row
    // breaks this call site at compile time — a new managed-mesh import lands as one arm and a non-mesh codec is
    // forced to declare its route, never misrouting to a stale "needs-companion" fault the prior == ladder produced
    // for GeometryGym/StepIso10303/geospatial. The arms whose every row is write-only or pending are unreachable
    // past the gate and spell the gate's own row, so no second refusal vocabulary grows beside it.
    // EVERY managed arm takes the hook carrier and the Op key: an arm that accepted the parameter and dropped it
    // reported a decode as instantaneous, which reads to a caller exactly like a decode that never ran.
    public static Fin<ImportedGeometry> ImportGeometry(
        InterchangeFormat format, ReadOnlyMemory<byte> bytes, IClock clock, Op key,
        Option<BimHooks> hooks = default, Option<UsdScope> scope = default) =>
        InterchangeFormat.Admitted(format, InterchangeDirection.Import, key).Bind(row => row.Codec.Switch(
            sharpGltf:        () => Boundary(key, () => Framed(format, Gltf(format, bytes, clock.GetCurrentInstant(), hooks, key))),
            meshText:         () => MeshTextGeometry(format, bytes, clock.GetCurrentInstant(), hooks, key),
            ply:              () => Boundary(key, () => Framed(format, Ply(format, bytes, clock.GetCurrentInstant(), hooks, key))),
            sceneExchange:    () => Boundary(key, () => Framed(format, Scene(format, bytes, clock.GetCurrentInstant(), hooks, key))),
            usdStage:         () => Boundary(key, () => Usd(format, bytes, clock.GetCurrentInstant(), hooks, scope, key)),   // the arm owns frame selection and stage population — upAxis is PER-STAGE metadata
            acadSharp:        () => Boundary(key, () => Framed(format, AcadReader.Read(format, bytes, clock.GetCurrentInstant(), hooks, key))),
            dotBim:           () => Boundary(key, () => Framed(format, DotBim(format, bytes, clock.GetCurrentInstant(), hooks, key))),
            geometryGym:      () => Fin.Fail<ImportedGeometry>(Detail.ImportIfcRoute.At(key, "use-ImportIfc", format.Key)),
            stepIso10303:     () => Fin.Fail<ImportedGeometry>(Detail.ImportStepRoute.At(key, "use-ImportStep", format.Key)),
            geospatialVector: () => Fin.Fail<ImportedGeometry>(Detail.ImportGeospatialRoute.At(key, format.Key)),
            geospatialRaster: () => Fin.Fail<ImportedGeometry>(Detail.ImportGeospatialRoute.At(key, format.Key)),
            pointCloud:       () => Fin.Fail<ImportedGeometry>(Detail.ImportPointCloudRoute.At(key, format.Key)),
            nativeCompanion:  () => Fin.Fail<ImportedGeometry>(Detail.ImportNeedsCompanion.At(key, format.Key)),
            igesAnsi:         () => Fin.Fail<ImportedGeometry>(Detail.ImportNeedsCompanion.At(key, format.Key)),
            saf:              () => Fin.Fail<ImportedGeometry>(Detail.ImportIfcRoute.At(key, "use-ImportIfc", format.Key)),   // the SAF carrier IS a live DatabaseIfc
            cobieXlsx:        () => Fin.Fail<ImportedGeometry>(Detail.DirectionUnsupported.At(key, nameof(InterchangeDirection.Import), format.Key)),
            energyModel:      () => Fin.Fail<ImportedGeometry>(Detail.ImportEnergyRoute.At(key, "EnergyExchange.Apply", format.Key)),   // the energy-model raise is Energy/exchange#ENERGY_EXCHANGE's, never a mesh decode
            ifc5Pending:      () => Fin.Fail<ImportedGeometry>(Detail.ImportCataloguePending.At(key, format.Key))));

    // OBJ/STL/OFF only — PLY now routes to the dedicated `ply-net` codec (the `Ply` arm) and 3MF/FBX/Collada
    // to the `scene-exchange` codec (the `Scene` arm), so the mesh-text sub-dispatch is one geometry3Sharp leg.
    // ONE StandardMeshReader is configured once and answers BOTH the support probe and the read: the second
    // instance the probe used carried no MeshBuilder, so the format set it admitted was never the set that read.
    static Fin<ImportedGeometry> MeshTextGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at, Option<BimHooks> hooks, Op key) {
        string extension = format.Extensions.Head.Map(static ext => ext.TrimStart('.')).IfNone("");
        var builder = new DMesh3Builder();
        var reader = new StandardMeshReader { MeshBuilder = builder };
        return reader.SupportsFormat(extension)
            ? Boundary(key, () => Framed(format, MeshText(reader, builder, format, extension, bytes, at, hooks, key)))
            : Fin.Fail<ImportedGeometry>(Detail.MeshTextUnsupported.At(key, format.Key, extension));
    }

    // EVERY DMesh3 the reader yields lands one pool block — an OBJ with N groups/objects builds N meshes, and a
    // first-mesh-only read was the deleted coverage defect. Refcounted sparse id space compacts through a live-id
    // ordinal Dictionary (a pre-sized boundary kernel, never a per-vertex immutable-Map rebuild).
    static ImportedGeometry MeshText(
        StandardMeshReader reader, DMesh3Builder builder, InterchangeFormat format, string extension,
        ReadOnlyMemory<byte> bytes, Instant at, Option<BimHooks> hooks, Op key) {
        var read = reader.Read(new MemoryStream(bytes.ToArray()), extension, ReadOptions.Defaults);
        if (read.code != IOCode.Ok) { throw new InvalidDataException($"<mesh-text-read:{read.code}:{read.message}>"); }
        DecodeStage.Opened.Beat(hooks, key);
        using var soup = new MeshSoup();
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
        DecodeStage.Decoded.Beat(hooks, key);
        return soup.ToGeometry(format, at, hooks, key);
    }

    // IFC arm decodes foreign bytes to the LIVE GeometryGym DatabaseIfc — the in-process IFC graph the
    // Projection/semantic#SEMANTIC_PROJECTOR SemanticProjector captures and lowers to a seam GraphDelta. Byte->graph
    // decode is the rail's; the entity walk, the full IfcRel* roster, the typed property/quantity projection,
    // OwnerHistory, and StepHeader are the projector's off this live graph, never a lossy IfcSemanticModel flat-row
    // re-projection here. GeometryGym is captured by the projector internally, so DatabaseIfc never crosses the seam
    // IElementProjection.Project signature. The InterchangeCodec.Saf arm shares THIS entrypoint because the SAF
    // import's carrier IS a live DatabaseIfc: the workbook authors GeometryGym structural entities the same
    // projector ingests, so the SAF wire re-enters through the exact fold the IFC wire takes and no sibling
    // entrypoint grows per format. The service triple rides the optional SafServices carrier — a saf-row call
    // without it refuses typed rather than defaulting a codec nothing wired — and the hook carrier serves the
    // Saf arm's residue degrade channel alone (this entrypoint beats no decode stages).
    public static Fin<DatabaseIfc> ImportIfc(
        InterchangeFormat format, ReadOnlyMemory<byte> bytes, Op key,
        Option<SafServices> saf = default, Option<BimHooks> hooks = default) =>
        InterchangeFormat.Admitted(format, InterchangeDirection.Import, key).Bind(row =>
            row.Codec == InterchangeCodec.GeometryGym
                ? SemanticProjector.Sniff(bytes, row, key)
                    .Bind(schema => Boundary(key, () => Database(row, bytes, schema)))
            : row.Codec == InterchangeCodec.Saf
                ? saf.Match(
                    Some: services => SafDatabase(bytes, services, hooks, key),
                    None: () => Fin.Fail<DatabaseIfc>(Detail.IfcCodecMiss.At(key, row.Key, "saf-services-absent")))
                : Fin.Fail<DatabaseIfc>(Detail.IfcCodecMiss.At(key, row.Key)));

    // SAF structural-workbook admission — the InterchangeCodec.Saf-keyed arm of the DatabaseIfc-carrier
    // entrypoint. The XLSX bytes decode and validate through the ONE Model/structural#STRUCTURAL_PROJECTION
    // StructuralProjection.Saf import leg (its AdmitSaf severity gate refusing an Error-carrying model typed),
    // and the validated ExcelModel AUTHORS GeometryGym structural entities onto a FRESH database — site host,
    // project context, Metre-declared units matching the SI magnitudes the authoring writes — through the one
    // Author(db, host, model, key) overload, so the SAF wire re-enters the ONE SemanticProjector off the
    // returned database and no second projector mints seam member nodes. The residue rows the authoring could
    // not carry fire DecodeReason.SafResidue one row each on the same deliberate-non-decode evidence lane every
    // arm on this rail rides, so the import succeeds while the uncarried payload stays counted, never silent.
    static Fin<DatabaseIfc> SafDatabase(ReadOnlyMemory<byte> bytes, SafServices services, Option<BimHooks> hooks, Op key) =>
        StructuralProjection.Saf(
                new SafOp.Import(new MemoryStream(bytes.ToArray()), services.Target),
                services.Imports, services.Exports, services.Validator, key)
            .Bind(model => Boundary(key, () => {
                DatabaseIfc db = new(services.Schema);
                IfcSite host = new(db, "SAF");
                _ = new IfcProject(host, "SAF", IfcUnitAssignment.Length.Metre);
                return (Db: db, Host: host, Model: model);
            }))
            .Bind(authored => StructuralProjection.Author(authored.Db, authored.Host, authored.Model, key)
                .Map(residue => {
                    residue.Iter(row => DecodeReason.SafResidue.Degrade(hooks, key, row));
                    return authored.Db;
                }));

    public static Fin<StepSemanticModel> ImportStep(InterchangeFormat format, ReadOnlyMemory<byte> bytes, IClock clock, Op key) =>
        InterchangeFormat.Admitted(format, InterchangeDirection.Import, key)
            .Bind(row => row.Codec == InterchangeCodec.StepIso10303
                ? Boundary(key, () => StepReader.Read(row, bytes.Span, clock.GetCurrentInstant()))
                : Fin.Fail<StepSemanticModel>(Detail.StepCodecMiss.At(key, row.Key)));

    // Captured-fault funnel: Try.lift runs the foreign decode, and MapFail closes over the Op key to lift the raw
    // error.Message into BimFault.ModelRejected BARE (the Expected-derived case IS the Error, no .ToError() hop).
    // This lambda is NOT static because it captures key; Try.lift preserves the raw message a kernel Op.Catch would
    // re-wrap in Fault.InvalidResult boilerplate, so the SharpGLTF ModelException, GeometryGym parse fault, and
    // malformed-Part-21 InvalidDataException never cross a domain signature.
    static Fin<T> Boundary<T>(Op key, Func<T> decode) =>
        Try.lift(decode).Run().MapFail(error => new BimFault.ModelRejected(key, error.Message));

    static ImportedGeometry Gltf(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at, Option<BimHooks> hooks, Op key) {
        bool compressed = Compression.IsPresent(bytes);
        var validation = compressed ? ValidationMode.Skip : ValidationMode.Strict;
        var model = format == InterchangeFormat.Glb
            ? ModelRoot.ParseGLB(new ArraySegment<byte>(bytes.ToArray()), new ReadSettings { Validation = validation })
            : TextContext(bytes, validation).ReadTextSchema2(new MemoryStream(bytes.ToArray()));
        DecodeStage.Opened.Beat(hooks, key);
        return Decoded(format, compressed ? Compression.Decompress(model, bytes) : model, at, hooks, key);
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
    static ImportedGeometry Decoded(InterchangeFormat format, ModelRoot model, Instant at, Option<BimHooks> hooks, Op key) {
        var meshes = model.LogicalMeshes.Decode();
        using var soup = new MeshSoup();
        var blocks = new int[meshes.Count];
        for (int m = 0; m < meshes.Count; m++) {
            // TEXCOORD_0 availability probes the SCHEMA accessor (GetVertexAccessor null = unmapped), so an
            // unmapped mesh contributes an EMPTY uv block and a zero-filled decoder read never fabricates a mapping.
            bool mapped = model.LogicalMeshes[m].Primitives.Any(static prim => prim.GetVertexAccessor("TEXCOORD_0") is not null);
            blocks[m] = soup.Append(Block(meshes[m], mapped));
        }
        DecodeStage.Decoded.Beat(hooks, key);
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
        DecodeStage.Placed.Beat(hooks, key);
        return soup.ToGeometry(format, at, hooks, key);
    }

    static MeshChunk Block(IMeshDecoder<Material> mesh, bool mapped) {
        var triangles = toSeq(mesh.Primitives
            .SelectMany(static prim => prim.TriangleIndices.Select(tri => (prim, tri))));
        int vertexCount = triangles.Count * 3;
        var vertices = new float[vertexCount * 3];
        var normals = new float[vertexCount * 3];
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
                (vertices[v], vertices[v + 1], vertices[v + 2]) = (p.X, p.Y, p.Z);
                (normals[v], normals[v + 1], normals[v + 2]) = (n.X, n.Y, n.Z);
                if (mapped) {
                    var uv = prim.GetTextureCoord(corner, 0);
                    (uvs[slot * 2], uvs[(slot * 2) + 1]) = (uv.X, uv.Y);
                }
                corners[slot] = slot;
                slot++;
            }
        }
        return new MeshChunk(vertices, normals, corners,
            mapped ? Seq((EncodingChannel.Uv, uvs)) : Seq<(EncodingChannel, float[])>());
    }

    // Canonicalize the POOL onto the kernel frame: positions AND normals each ride their own strided call (the one
    // orthogonal signed permutation carries both per format#FORMAT_AXIS — the position-only form left normals in the
    // source frame, the deleted defect), and every instance transform conjugates by the basis (row-vector convention:
    // M' = Bᵀ·M·B) so a placed block lands where its baked copy would have. Canonicalize takes each lane's own arity
    // as its stride, so a two-ordinate lane can never be walked at a three-ordinate step.
    static ImportedGeometry Framed(InterchangeFormat format, ImportedGeometry geometry) {
        if (format.IsCanonicalFrame) {
            return geometry;
        }
        Matrix4x4 basis = Basis(format);
        Matrix4x4 inverse = Matrix4x4.Transpose(basis);
        // Arenas are immutable, so a frame change RE-MINTS one rather than writing back into a column. Each lane
        // lifts to floats through its OWN dtype and returns through the one Encode.Of mint, which re-measures the
        // witness instead of carrying a stale one. Only Position and Normal move: the frame change is a rigid
        // signed permutation, so every other lane — parameterization, vertex colour, and whatever the roster grows
        // next — is rigid-invariant and copies unchanged. Dispatching on the DESCRIPTOR is what keeps a new
        // EncodingChannel row free here; the per-column rewrite the parallel-buffer form used grew a branch per lane.
        Seq<(EncodingChannel Channel, float[] Raw)> lanes = geometry.Lanes.Descriptors.Map(descriptor => {
            var raw = new float[descriptor.Count * descriptor.Channel.Arity];
            descriptor.Dtype.Unpack(geometry.Lanes.Channel(descriptor.Channel).Span, raw);
            if (descriptor.Channel == EncodingChannel.Position || descriptor.Channel == EncodingChannel.Normal) {
                FrameNormalization.Canonicalize(format, raw.AsSpan(), stride: descriptor.Channel.Arity);
            }
            return (descriptor.Channel, raw);
        });
        return geometry with {
            Lanes = Encode.Of(geometry.VertexCount, lanes).ThrowIfFail(),
            Instances = geometry.Instances.Map(i => i with { Transform = inverse * i.Transform * basis }),
        };
    }

    // Row's BasisChange as the row-vector numerics matrix: each ROW is the canonical image of a source axis.
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

        // KHR_draco accessors carry NO bufferView (spec) — the typed-array Fill would read a backing region
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

        // COMPRESSED slice lives at the EXTENSION's buffer/byteOffset/byteLength — per EXT_meshopt_compression a
        // bufferView's own properties describe the UNCOMPRESSED fallback target, so reading view.Content as the source
        // is the deleted spec inversion. Decode lands IN the view's own count*stride region (the bytes every accessor
        // over this view reads), and a fallback-less view (Content shorter than count*stride under a Skip parse) faults
        // loud through the Boundary funnel rather than decoding into a dead side buffer.
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

    // IFC byte->graph decode the import rail OWNS: the ONE bytes->DatabaseIfc admission in the package. Entity/
    // relationship/property projection onto seam nodes+edges is Projection/semantic#SEMANTIC_PROJECTOR's (it
    // captures this DatabaseIfc internally), so the import rail mints no IfcSemanticModel, AssemblyRel, or
    // MapConversionRow — those flat rows dropped the stranded IfcRel* families, OwnerHistory, and StepHeader the
    // projector preserves off the live graph. wire#WIRE_PROJECTION Admit and export#ROUNDTRIP Verify COMPOSE
    // ImportIfc, each re-wrapping its own admission prefix. Schema is sniffed off the bytes BEFORE construction:
    // ImportIfc binds the RAILED `Fin<GGRelease> SemanticProjector.Sniff(bytes, format, key)` — the ONE schema-sniff
    // owner (STEP FILE_SCHEMA / ifcJSON schemaIdentifier / ifcXML xmlns), CodecReject `schema-header-missing`/
    // `schema-header-unmapped` typed OUTSIDE the ModelRejected funnel with no silent schema default, so the
    // construction lands at that schema and a 2x3 file admits as 2x3.
    // Serialization dispatch is the ROW ITSELF: the format#FORMAT_AXIS Serialization column carries the
    // Projection/egress#IFC_EGRESS IfcWireForm, and that row owns BOTH directions as delegates — Seal writes its
    // container, Admit reads it — so this body hands the bytes and the sniffed schema to the row and holds no
    // serialization ladder at all.
    static DatabaseIfc Database(InterchangeFormat format, ReadOnlyMemory<byte> bytes, GGRelease schema) =>
        format.Serialization
            .IfNone(() => throw new InvalidDataException($"<ifc-serialization-miss:{format.Key}>"))
            .Admit(bytes, schema);

    // PLY decode through Ply.Net — the dedicated `ply-net` codec retiring the hand-rolled PlyReader.
    // PlyParser.Parse decodes the header-plus-chunked body into the immutable Dataset record graph; the
    // Vertex element's typed x/y/z columns and the Face element's vertex_indices list column materialize
    // once as a typed System.Array (no parser DTO), fan-triangulated onto the canonical triangle-soup.
    static ImportedGeometry Ply(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at, Option<BimHooks> hooks, Op key) {
        using var stream = new MemoryStream(bytes.ToArray());
        using var soup = new MeshSoup();
        var dataset = PlyParser.Parse(stream, maxChunkSize: 1 << 20);
        // Dataset.Data is a lazy/streamed sequence over the parse stream (api-ply-net), materializing ONCE before the
        // vertex+face lookups — a second enumeration re-reads the already-advanced stream and strands the columns.
        var elements = dataset.Data.ToList();
        DecodeStage.Opened.Beat(hooks, key);
        var vertex = elements.First(static d => d.Element.Type == ElementType.Vertex);
        var face = elements.FirstOrDefault(static d => d.Element.Type == ElementType.Face);
        float[] xs = Column(vertex, "x"), ys = Column(vertex, "y"), zs = Column(vertex, "z");
        var (nx, ny, nz) = (OptionalColumn(vertex, "nx"), OptionalColumn(vertex, "ny"), OptionalColumn(vertex, "nz"));
        // Scan-derived and photogrammetry PLY — the format's dominant real-world source — carries its
        // parameterization as s/t and its radiometry as red/green/blue vertex columns, so an as-built delivery
        // that reads geometry alone discards exactly the two facts it exists to preserve. The pair name is not
        // canonical across writers: s/t is the PLY convention and texture_u/texture_v the widespread alias, so the
        // lane resolves through the alias fallback the name-keyed indexer already makes free.
        var (s, t) = (OptionalColumn(vertex, "s") ?? OptionalColumn(vertex, "texture_u"),
                      OptionalColumn(vertex, "t") ?? OptionalColumn(vertex, "texture_v"));
        // Radiometry rides the red/green/blue triple with an OPTIONAL alpha, each column normalized against ITS OWN
        // declared width: the seam colour lane is Unorm8, whose stored value is a unit fraction, while PLY writes a
        // uchar 0..255 far more often than a float 0..1 — reading the declared DataType is what keeps a class-7
        // scan colour from landing as 7.0 and a float writer's 0.5 from landing as 127.5. An absent alpha fills
        // opaque, because a colour lane's arity is fixed at four and a source that declared RGB declared opacity 1.
        var (red, green, blue) = (UnitColumn(vertex, "red"), UnitColumn(vertex, "green"), UnitColumn(vertex, "blue"));
        float[]? alpha = UnitColumn(vertex, "alpha");
        int vertexCount = xs.Length;
        var vertices = new float[vertexCount * 3];
        var normals = new float[vertexCount * 3];
        // UV lanes materialize only when BOTH ordinates are present: a half-declared pair is a malformed
        // header, and a zero-filled partner would forge a parameterization the file never carried. The colour lane
        // takes the same law over its three mandatory channels.
        float[] uvs = s is not null && t is not null ? new float[vertexCount * 2] : [];
        float[] colours = red is not null && green is not null && blue is not null ? new float[vertexCount * 4] : [];
        for (int v = 0; v < vertexCount; v++) {
            (vertices[v * 3], vertices[v * 3 + 1], vertices[v * 3 + 2]) = (xs[v], ys[v], zs[v]);
            (normals[v * 3], normals[v * 3 + 1], normals[v * 3 + 2]) = (nx?[v] ?? 0f, ny?[v] ?? 0f, nz?[v] ?? 1f);
            if (uvs.Length > 0) { (uvs[v * 2], uvs[(v * 2) + 1]) = (s![v], t![v]); }
            if (colours.Length > 0) {
                (colours[v * 4], colours[(v * 4) + 1], colours[(v * 4) + 2], colours[(v * 4) + 3]) =
                    (red![v], green![v], blue![v], alpha?[v] ?? 1f);
            }
        }
        var indices = face is null
            ? Array.Empty<long>()
            : ((int[][])face["vertex_indices"].Data)
                .SelectMany(static corners => Enumerable.Range(1, corners.Length - 2)
                    .SelectMany(k => new long[] { corners[0], corners[k], corners[k + 1] }))
                .ToArray();
        Seq<(EncodingChannel Channel, float[] Raw)> attributes =
            (uvs.Length > 0 ? Seq((EncodingChannel.Uv, uvs)) : Seq<(EncodingChannel, float[])>())
            + (colours.Length > 0 ? Seq((EncodingChannel.ColorRgba, colours)) : Seq<(EncodingChannel, float[])>());
        // PLY clouds arrive world-space and identity-placed, so this arm passes through the Placed row rather than
        // beating a position no placement reached — rows state PHASES, never a per-arm padding schedule.
        DecodeStage.Decoded.Beat(hooks, key);
        return soup.Baked(vertices, normals, indices, attributes).ToGeometry(format, at, hooks, key);
    }

    // PLY column materialized as float[] regardless of on-disk scalar width — Ply.Net types each column as its
    // matching System.Array (float[] for Float32, double[] for Float64, int[] for the integer widths).
    static float[] Column(ElementData element, string name) => element[name].Data switch {
        float[] f  => f,
        double[] d => Array.ConvertAll(d, static x => (float)x),
        int[] i    => Array.ConvertAll(i, static x => (float)x),
        Array a    => Enumerable.Range(0, a.Length).Select(i => Convert.ToSingle(a.GetValue(i))).ToArray(),
        _          => [],
    };

    static float[]? OptionalColumn(ElementData element, string name) =>
        element.Element.Properties.Exists(p => p.Name == name) ? Column(element, name) : null;

    // UnitColumn reads a colour column onto the UNIT interval its seam lane stores: an integer-width column divides
    // by its own full scale and a float-width column is already unit. That divisor comes from the column's DECLARED
    // DataType, never from probing the values — a dark scan whose channels all sit under 1.0 is indistinguishable
    // from a float writer's output by inspection, and guessing there silently blackens every such delivery.
    static float[]? UnitColumn(ElementData element, string name) =>
        OptionalColumn(element, name) is not { } column
            ? null
            : ColourScale.Find(element[name].Property.DataType).Match(
                Some: scale => Array.ConvertAll(column, value => value / scale),
                None: () => column);

    // Full scale per integer PLY width; a float or double column carries no row because it is already unit-valued.
    static readonly Map<PlyParser.DataType, float> ColourScale = Map(
        (PlyParser.DataType.Int8, 127f), (PlyParser.DataType.UInt8, 255f),
        (PlyParser.DataType.Int16, 32767f), (PlyParser.DataType.UInt16, 65535f),
        (PlyParser.DataType.Int32, 2147483647f), (PlyParser.DataType.UInt32, 4294967295f),
        (PlyParser.DataType.Int64, 9223372036854775807f), (PlyParser.DataType.UInt64, 18446744073709551615f));

    // FBX/Collada/3MF decode through AssimpNetter — the `scene-exchange` codec retiring the hand-rolled
    // ThreeMfReader. One disposable AssimpContext imports with the canonical Bim post-process (Triangulate |
    // JoinIdenticalVertices | GenerateSmoothNormals); each scene mesh lands ONE pool block, and the RootNode walk
    // places it per mesh-bearing node with the composed node transform (the world matrix is the product up the
    // Parent chain per api-assimpnetter) — the flat scene.Meshes fold that dropped every node placement is the
    // deleted form. Handedness rides the per-importer FrameNormalization the row carries, not MakeLeftHanded.
    static ImportedGeometry Scene(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at, Option<BimHooks> hooks, Op key) {
        using var context = new AssimpContext();
        using var stream = new MemoryStream(bytes.ToArray());
        // READ hint is the row's file EXTENSION (assimp importer selection keys on extension: "dae", not the
        // row key "collada"); the row KEY stays the EXPORT formatId ExportToBlob dispatches on — two foreign
        // contracts, never conflated on one value.
        // Post-process is the catalogue's own declared normalization, complete: Triangulate |
        // JoinIdenticalVertices | GenerateSmoothNormals | CalculateTangentSpace | GenerateUVCoords. The last two
        // are the texture half — GenerateUVCoords projects the parametric mappings an FBX/Collada authoring tool
        // stores as generators into real per-vertex coordinates, and CalculateTangentSpace derives the basis a
        // normal map samples in; without either, an FBX carrying a full unwrap and a normal-map material lands
        // with its parameterization thrown away and the export side has nothing to bind.
        var scene = context.ImportFileFromStream(stream,
            PostProcessSteps.Triangulate | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.GenerateSmoothNormals
                | PostProcessSteps.CalculateTangentSpace | PostProcessSteps.GenerateUVCoords,
            format.Extensions.Head.Map(static ext => ext.TrimStart('.')).IfNone(format.Key));
        DecodeStage.Opened.Beat(hooks, key);
        using var soup = new MeshSoup();
        var blocks = new int[scene.MeshCount];
        var referenced = new bool[scene.MeshCount];
        for (int m = 0; m < scene.MeshCount; m++) { blocks[m] = soup.Append(AssimpBlock(scene.Meshes[m])); }
        DecodeStage.Decoded.Beat(hooks, key);
        Walk(scene.RootNode, Matrix4x4.Identity);
        for (int m = 0; m < scene.MeshCount; m++) {
            if (!referenced[m]) { soup.Place(blocks[m], Matrix4x4.Identity); }
        }
        DecodeStage.Placed.Beat(hooks, key);
        return soup.ToGeometry(format, at, hooks, key);

        // Row-vector composition: world = local × parentWorld (Assimp's column-vector chain, transposed once at Numeric).
        void Walk(Assimp.Node node, Matrix4x4 parent) {
            Matrix4x4 world = Numeric(node.Transform) * parent;
            foreach (int m in node.MeshIndices) { referenced[m] = true; soup.Place(blocks[m], world); }
            foreach (var child in node.Children) { Walk(child, world); }
        }
    }

    // TextureCoordinateChannels is a per-SET array behind TextureCoordinateChannelCount, and each set declares
    // its own component width in UVComponentCount — assimp stores every set as Vector3 regardless, so a 2-component
    // set carries its third ordinate as a zero the seam lane must not transcribe. Set 0 is TEXCOORD_0, the one set the
    // seam carrier declares; a further set is one more carrier lane, never a second decode.
    // VertexColorChannels mirrors that shape exactly — a per-SET array behind VertexColorChannelCount whose entries
    // are System.Numerics.Vector4 already on the unit interval, so set 0 lands the ColorRgba lane with no rescale.
    // FBX and Collada carry per-vertex colour as their standard channel for baked lighting, mesh-paint authoring,
    // and analysis-result surfaces, and reading geometry alone discarded exactly the signal those files exist for.
    static MeshChunk AssimpBlock(Assimp.Mesh mesh) {
        var vertices = new float[mesh.VertexCount * 3];
        var normals = new float[mesh.VertexCount * 3];
        bool mapped = mesh.TextureCoordinateChannelCount > 0 && mesh.UVComponentCount[0] >= 2;
        bool painted = mesh.VertexColorChannelCount > 0 && mesh.HasVertexColors(0);
        var uvs = mapped ? new float[mesh.VertexCount * 2] : [];
        var colours = painted ? new float[mesh.VertexCount * 4] : [];
        for (int i = 0; i < mesh.VertexCount; i++) {
            var p = mesh.Vertices[i];
            int v = i * 3;
            (vertices[v], vertices[v + 1], vertices[v + 2]) = (p.X, p.Y, p.Z);
            (normals[v], normals[v + 1], normals[v + 2]) = mesh.HasNormals
                ? (mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z)
                : (0f, 0f, 1f);
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
        var corners = mesh.Faces
            .SelectMany(static face => Enumerable.Range(1, face.IndexCount - 2)
                .SelectMany(k => new long[] { face.Indices[0], face.Indices[k], face.Indices[k + 1] }))
            .ToArray();
        return new MeshChunk(vertices, normals, corners,
            (mapped ? Seq((EncodingChannel.Uv, uvs)) : Seq<(EncodingChannel, float[])>())
            + (painted ? Seq((EncodingChannel.ColorRgba, colours)) : Seq<(EncodingChannel, float[])>()));
    }

    // Assimp matrices are column-vector convention; the numerics row-vector equivalent is the transpose.
    static Matrix4x4 Numeric(Assimp.Matrix4x4 m) => new(
        m.A1, m.B1, m.C1, m.D1, m.A2, m.B2, m.C2, m.D2, m.A3, m.B3, m.C3, m.D3, m.A4, m.B4, m.C4, m.D4);

    // USD decode through UniversalSceneDescription — the `usd-stage` codec. One UsdStage opens the layer
    // stack (the native plugin tree reads the temp path), Traverse walks the namespace, each UsdGeomMesh
    // prim's points/normals (VtVec3fArray of GfVec3f) and face-vertex counts/indices (VtIntArray) cross the
    // typed-array mesh-bridge seam onto its pool blocks, and the prim PLACES every one of them by the composed
    // local-to-world transform off ONE UsdGeomXformCache — the identity-placed decode that baked every prim
    // and erased USD's native instancing/placement is the deleted form. A UsdGeomPointInstancer decodes on the
    // SAME overlay: its prototypes are pool blocks and its instances are placements, so a site or facade-panel
    // scene authored as one instancer — the shape a large USD delivery actually takes — lands its geometry
    // instead of importing EMPTY off a Mesh-only filter. A mesh carrying material-bound UsdGeomSubset children
    // lands ONE block per subset, each stamping the seam MeshBlock.Material shading key off that subset's own
    // bound material path, so a multi-material USD mesh reaches a consumer as the partition its author wrote.
    // USD is a scene-graph peer — the BIM semantics stay the GeometryGym IFC graph's, never re-derived from USD
    // prim type names.
    // Frame is PER-STAGE: upAxis is stage metadata (UsdGeomGetStageUpAxis, decompile-verified — TfToken
    // "Y" the USD default, "Z" the common CAD/BIM export), so a Z-up stage is ALREADY canonical and skips the
    // row's Y-up Frame; the format row keeps the static Y-up default every metadata-less stage falls to.
    static ImportedGeometry Usd(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at, Option<BimHooks> hooks, Option<UsdScope> scope, Op key) {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{format.Extensions.Head.IfNone(".usd")}");
        File.WriteAllBytes(path, bytes.ToArray());
        try {
            using var stage = Staged(path, scope);
            DecodeStage.Opened.Beat(hooks, key);
            bool zUp = UsdGeom.UsdGeomGetStageUpAxis(stage).ToString() == "Z";
            using var soup = new MeshSoup();
            var xform = new UsdGeomXformCache();
            // Prototype subtrees hold the instancer's OWN geometry, placed by its per-instance transforms alone.
            // Stages authoring them as ordinary defined prims rather than under a class or deactivated scope would
            // otherwise bake every prototype a SECOND time at its authoring place, doubling the scene.
            Seq<string> prototypes = stage.Traverse().AsIterable()
                .Filter(static prim => prim.GetTypeName().ToString() == PointInstancerType)
                .Bind(prim => new UsdGeomPointInstancer(prim).GetPrototypesRel().GetTargets().AsIterable())
                .Map(static target => target.GetAsString())
                .ToSeq().Distinct();
            // Blocks key on prim PATH and hold that prim's whole PARTITION SET: a prototype referenced by ten
            // thousand instances appends its blocks once and places them ten thousand times, which IS the
            // carrier's Blocks/Instances overlay, and a multi-material prototype shares every one of its splits.
            var blocks = new Dictionary<string, Seq<int>>(StringComparer.Ordinal);
            stage.Traverse().AsIterable()
                .Filter(prim => !Prototyped(prim, prototypes))
                .Iter(prim => ignore(prim.GetTypeName().ToString() switch {
                    MeshType           => Place(soup, Blocks(soup, prim, blocks), Numeric(xform.GetLocalToWorldTransform(prim))),
                    PointInstancerType => Scatter(stage, prim, Numeric(xform.GetLocalToWorldTransform(prim)), soup, blocks),
                    _                  => soup,
                }));
            // USD traversal decodes and places on ONE pass — a prototype's blocks append at its first reference and
            // every instance places against the same pool — so both rows beat at that traversal's own close.
            DecodeStage.Decoded.Beat(hooks, key);
            DecodeStage.Placed.Beat(hooks, key);
            var geometry = soup.ToGeometry(format, at, hooks, key);
            return zUp ? geometry : Framed(format, geometry);
        } finally { File.Delete(path); }
    }

    // Population is a STAGE-OPEN decision, so it lands at the open and nowhere after it: a masked stage never
    // composes the prims outside the mask, where a post-open traversal filter pays the whole layer stack's
    // composition and prim indexing first and then discards it. Downstream the two opens are indistinguishable — the
    // traversal, the prototype exclusion, the `UsdGeomSubset` partition, and the pooled/`Declared` lane
    // evidence all read whatever prims the stage holds — so the scope adds no arm anywhere below this line.
    static UsdStage Staged(string path, Option<UsdScope> scope) =>
        scope.IfNone(UsdScope.Whole).Switch(
            state: path,
            wholeStage: static (root, _) => UsdStage.Open(root, UsdStage.InitialLoadSet.LoadAll),
            populated:  static (root, populated) => Masked(root, populated.Paths));

    // Masks CONSTRUCT from the admitted run in one shot through the path-vector ctor: the mutating
    // `Add(SdfPath)` returns a fresh managed wrapper over the same native pointer, so an accumulating build
    // hands ownership of one mask to several finalizers.
    static UsdStage Masked(string path, Seq<string> paths) {
        using var addresses = new SdfPathVector(paths.Map(static prim => new SdfPath(prim)));
        using var mask = new UsdStagePopulationMask(addresses);
        return UsdStage.OpenMasked(path, mask, UsdStage.InitialLoadSet.LoadAll);
    }

    // Decode admits these two USD schema type names, spelled once — foreign open vocabulary the traversal
    // discriminates on, never a closed owner and never re-spelled per call site. Face-subset element type and
    // material-binding family name ride the package's OWN interned tokens (UsdGeomTokens/UsdShadeTokens), so
    // neither is spelled as a literal here.
    const string MeshType = "Mesh";
    const string PointInstancerType = "PointInstancer";

    // Prims belong to an instancer when their path IS a prototype target or descends from one; a trailing
    // separator keeps `/World/ProtoHouse` from swallowing its sibling `/World/ProtoHouseAnnex`.
    static bool Prototyped(UsdPrim prim, Seq<string> prototypes) =>
        prim.GetPath().GetAsString() is var path
        && prototypes.Exists(root => path == root || path.StartsWith($"{root}/", StringComparison.Ordinal));

    // One prim's whole partition set, pooled by path. Append is an EFFECT, so the mapped Seq FORCES through
    // Strict before it is pooled — a deferred Seq re-enumerated at the second placement would append the same
    // partitions again and fan duplicate blocks through the rest of the traversal.
    static Seq<int> Blocks(MeshSoup soup, UsdPrim prim, Dictionary<string, Seq<int>> pool) =>
        prim.GetPath().GetAsString() is var path && pool.TryGetValue(path, out Seq<int> held)
            ? held
            : pool[path] = UsdMesh(new UsdGeomMesh(prim)).Map(chunk => soup.Append(chunk)).Strict();

    // Place lands EVERY block of a partition set at the ONE world transform its prim carries — placement stays the
    // prim's fact and partition the material's, so both overlays compose without either re-deriving the other.
    static MeshSoup Place(MeshSoup soup, Seq<int> blocks, Matrix4x4 world) =>
        blocks.Fold(soup, (pool, block) => pool.Place(block, world));

    // Point instancers ARE the carrier's block-and-instance overlay authored in USD: a prototypes relationship
    // names the geometry, protoIndices says which prototype each instance wears, and ComputeInstanceTransformsAtTime
    // composes positions, orientations, scales AND each prototype's own xform into one per-instance matrix while
    // applying the invisibleIds mask — the package's own composition, so a hand-multiplied position/orientation/
    // scale triple beside it is the deleted re-derivation dropping mask and prototype xform together. Each
    // instancer's own local-to-world then carries its whole scatter into the stage frame (row-vector convention, so
    // each instance matrix precedes it). Prototypes that are not Meshes contribute no block, so a curve, light, or
    // nested-instancer prototype scatters nothing rather than a fabricated triangle.
    static MeshSoup Scatter(UsdStage stage, UsdPrim prim, Matrix4x4 instancerWorld, MeshSoup soup, Dictionary<string, Seq<int>> blocks) {
        var instancer = new UsdGeomPointInstancer(prim);
        SdfPathVector protoPaths = instancer.GetPrototypesRel().GetTargets();
        var indexValue = new VtValue();
        instancer.GetProtoIndicesAttr().Get(indexValue, UsdTimeCode.Default());
        var protoIndices = (VtIntArray)indexValue;
        var transforms = new VtMatrix4dArray(protoIndices.size());
        if (!instancer.ComputeInstanceTransformsAtTime(transforms, UsdTimeCode.Default(), UsdTimeCode.Default())) { return soup; }
        for (int i = 0; i < (int)protoIndices.size(); i++) {
            int slot = protoIndices[i];
            if (slot < 0 || slot >= protoPaths.Count) { continue; }
            UsdPrim proto = stage.GetPrimAtPath(protoPaths[slot]);
            if (proto.GetTypeName().ToString() != MeshType) { continue; }
            Place(soup, Blocks(soup, proto, blocks), Matrix4x4.Multiply(Numeric(transforms[i]), instancerWorld));
        }
        return soup;
    }

    // GfMatrix4d is row-major double over ROW vectors — the numerics convention — so the narrow is
    // per-component, never a transpose (the Assimp column-vector overload transposes; this one must not).
    static Matrix4x4 Numeric(GfMatrix4d m) {
        var (a, b, c, d) = (m.GetRow(0), m.GetRow(1), m.GetRow(2), m.GetRow(3));
        return new Matrix4x4(
            (float)a[0], (float)a[1], (float)a[2], (float)a[3], (float)b[0], (float)b[1], (float)b[2], (float)b[3],
            (float)c[0], (float)c[1], (float)c[2], (float)c[3], (float)d[0], (float)d[1], (float)d[2], (float)d[3]);
    }

    // Typed-array mesh-bridge: GetPointsAttr/GetNormalsAttr/GetFaceVertexCountsAttr/GetFaceVertexIndicesAttr
    // each fill a VtValue the typed Vt*Array reads (size()/indexer), per the api-usd mesh-bridge seam; authored
    // normals ride when their count matches the points (faceVarying/uniform normals re-index at the admission
    // gate, the up-normal the absent-case fill); faces fan-triangulate into PRE-SIZED buffers, each group folding
    // its own Σ(n-2) fan size before its buffer allocates.
    // One prim yields one chunk per SHADING PARTITION: USD carries a multi-material mesh as material-bound
    // UsdGeomSubset children over face ordinals, so the partition is the authored fact and each chunk stamps the
    // seam MeshBlock.Material key off its subset's own bound material path. GetUnassignedIndices names the
    // remainder an `unrestricted` family leaves, which is what keeps every face landing exactly once — a
    // subsets-only read silently drops the uncovered run and a whole-mesh read strands the split.
    static Seq<MeshChunk> UsdMesh(UsdGeomMesh mesh) {
        var (points, authored, counts, corners) = (new VtValue(), new VtValue(), new VtValue(), new VtValue());
        mesh.GetPointsAttr().Get(points, UsdTimeCode.Default());
        bool hasNormals = mesh.GetNormalsAttr().Get(authored, UsdTimeCode.Default());
        mesh.GetFaceVertexCountsAttr().Get(counts, UsdTimeCode.Default());
        mesh.GetFaceVertexIndicesAttr().Get(corners, UsdTimeCode.Default());
        var (xyz, faceCounts, faceIndices) = ((VtVec3fArray)points, (VtIntArray)counts, (VtIntArray)corners);
        var perVertex = hasNormals && (VtVec3fArray)authored is { } nrm && (int)nrm.size() == (int)xyz.size() ? nrm : null;
        // `st` is USD's canonical UV primvar and it reaches the prim through the primvars API, never the typed
        // schema — GetPrimvar resolves it and ComputeFlattened expands the INDEXED form (a primvar may store one
        // value per unique corner plus an index array, and reading the raw values there mislabels every vertex).
        // Only the per-vertex interpolation lands on the seam lane: a faceVarying or uniform `st` re-indexes on the
        // same admission gate the authored-normals branch takes, and until that gate runs the honest lane is the
        // seam's typed absence rather than a mis-indexed unwrap.
        var stValue = new VtValue();
        var st = new UsdGeomPrimvarsAPI(mesh.GetPrim()).GetPrimvar(new TfToken("st"));
        bool mapped = st.HasAuthoredValue() && st.ComputeFlattened(stValue)
            && st.GetInterpolation().ToString() == "vertex"
            && (VtVec2fArray)stValue is { } uvArray && (int)uvArray.size() == (int)xyz.size();
        // Corner cursor per face, so a partition walks its own faces without re-scanning the counts prefix.
        int faces = (int)faceCounts.size();
        var cursors = new int[faces];
        for (int f = 1; f < faces; f++) { cursors[f] = cursors[f - 1] + faceCounts[f - 1]; }
        UsdGeomSubsetVector subsets = UsdGeomSubset.GetGeomSubsets(mesh, UsdGeomTokens.face, UsdShadeTokens.materialBind);
        Option<string> own = BoundMaterial(mesh.GetPrim());
        Seq<(Seq<int> Faces, Option<string> Material)> subsetGroups =
            toSeq(subsets).Map(static subset => (FaceOrdinals(subset), BoundMaterial(subset.GetPrim())));
        // Remainder group closes the partition — GetUnassignedIndices names the faces no subset claims, and an
        // unpartitioned mesh takes every face under its own direct binding, so ONE Chunk fold serves both readings
        // and neither is a special case. Enumerating the face run for the subset-free mesh rather than trusting the
        // remainder call over an empty vector is what keeps an ordinary single-material mesh from importing empty.
        Seq<int> remainder = subsets.Count == 0
            ? toSeq(Enumerable.Range(0, faces))
            : Ordinals(UsdGeomSubset.GetUnassignedIndices(subsets, (uint)faces));
        // Strict FORCES every chunk inside the decode: each lane reads live pxr typed arrays, so a deferred Seq
        // would materialize them after this frame let the native handles go and hand a consumer a torn read.
        return subsetGroups.Add((remainder, own))
            .Filter(static group => !group.Faces.IsEmpty)
            .Map(group => Chunk(group.Faces, group.Material))
            .Strict();

        // Compacted per-partition chunk: only the points this face group references cross, remapped 0-based, so two
        // subsets of one mesh land as DISJOINT pool blocks instead of two copies of the whole point array.
        // Exemption: the corner walk and the lane fill are the boundary decode kernel; the rail resumes at the mint.
        MeshChunk Chunk(Seq<int> group, Option<string> material) {
            var ordinal = new Dictionary<int, int>();
            var tris = new long[group.Fold(0, (sum, f) => sum + Math.Max(0, faceCounts[f] - 2)) * 3];
            int slot = 0;
            foreach (int f in group) {
                int n = faceCounts[f], cursor = cursors[f];
                for (int k = 1; k < n - 1; k++) {
                    (tris[slot], tris[slot + 1], tris[slot + 2]) =
                        (Local(faceIndices[cursor]), Local(faceIndices[cursor + k]), Local(faceIndices[cursor + k + 1]));
                    slot += 3;
                }
            }
            var verts = new float[ordinal.Count * 3];
            var normals = new float[verts.Length];
            var uvs = mapped ? new float[ordinal.Count * 2] : [];
            foreach (var (point, local) in ordinal) {
                var p = xyz[point];
                (verts[local * 3], verts[local * 3 + 1], verts[local * 3 + 2]) = (p[0], p[1], p[2]);
                (normals[local * 3], normals[local * 3 + 1], normals[local * 3 + 2]) = perVertex is { } n
                    ? (n[point][0], n[point][1], n[point][2])
                    : (0f, 0f, 1f);
                if (mapped) {
                    var uv = ((VtVec2fArray)stValue)[point];
                    (uvs[local * 2], uvs[(local * 2) + 1]) = (uv[0], uv[1]);
                }
            }
            return new MeshChunk(verts, normals, tris,
                mapped ? Seq((EncodingChannel.Uv, uvs)) : Seq<(EncodingChannel, float[])>(), material);

            // First reference of a point mints its 0-based local slot; the indexer reads Count BEFORE the insert.
            long Local(int point) => ordinal.TryGetValue(point, out int held) ? held : ordinal[point] = ordinal.Count;
        }
    }

    // FaceOrdinals reads a subset's own authored face set — its one authored surface, never a reverse lookup off
    // whichever material it binds.
    static Seq<int> FaceOrdinals(UsdGeomSubset subset) {
        var indices = new VtValue();
        subset.GetIndicesAttr().Get(indices, UsdTimeCode.Default());
        return Ordinals((VtIntArray)indices);
    }

    // Both ordinal reads share one narrowing: a subset's authored indices and the package's own remainder
    // computation are the same VtIntArray shape.
    static Seq<int> Ordinals(VtIntArray array) =>
        toSeq(Enumerable.Range(0, (int)array.size()).Select(i => array[i]));

    // BoundMaterial narrows a prim's DIRECT binding to its own scene path — a subset's binding for a partitioned
    // face range, its mesh's own otherwise. An unbound prim yields None, so a block carries a shading key only
    // where its source authored one and the appearance projection never re-hydrates a fabricated path.
    static Option<string> BoundMaterial(UsdPrim prim) =>
        new UsdShadeMaterialBindingAPI(prim).GetDirectBinding().GetMaterialPath() is { } path && !path.IsEmpty()
            ? Some(path.GetAsString())
            : None;

    // Shared mesh-pool builder every non-glTF decode arm folds into — a SINGLE-USE pooled boundary kernel: three
    // growth buffers rent through ArrayPoolBufferWriter<T> (BCL IBufferWriter GetSpan/Advance staging, pooled doubling,
    // admitted CommunityToolkit.HighPerformance owner) replacing both the rejected per-block Seq concatenation
    // (O(blocks·total)) and the List<T> LOH churn. Append lands one block (vertices/normals as flat triples, 0-based
    // corners offset into the pool, the OPTIONAL attribute lanes that block declared, and the OPTIONAL shading key
    // its face range binds) and returns its ordinal; Place records one rigid placement; Baked is the identity-placed
    // block the non-instanced arms use. ToGeometry
    // mints the seam carrier's ONE kernel EncodedGeometry arena through the raw-lane Encode.Of entry over the lanes
    // this decode ACTUALLY declared, carries the Blocks/Instances overlay, AND returns the rents — the builder is
    // dead after it. Parameterization contributes a lane only when some block carried one, so an unmapped source
    // yields a MISSING DESCRIPTOR rather than an empty column every reader would length-probe.
    sealed class MeshSoup : IDisposable {
        readonly ArrayPoolBufferWriter<float> vertices = new();
        readonly ArrayPoolBufferWriter<float> normals = new();
        readonly ArrayPoolBufferWriter<long> indices = new();
        // Per-block OPTIONAL lanes keyed by their kernel channel: parameterization, vertex colour, and whatever the
        // EncodingChannel roster grows next all ride ONE list of lane sets rather than a field, a buffer, and a
        // per-lane pool fold each. The prior form carried a `blockUvs` field beside a `PoolUvs` member, so the
        // second attribute the roster opened would have doubled both — the exact per-channel proliferation the
        // carrier's own arena deleted one stratum down, re-grown here.
        readonly List<Seq<(EncodingChannel Channel, float[] Raw)>> blockLanes = [];
        readonly List<MeshBlock> blocks = [];
        readonly List<MeshInstance> instances = [];

        public int VertexCount { get; private set; }

        public int Append(MeshChunk block) =>
            Append(block.Vertices, block.Normals, block.Corners, block.Attributes, block.Material);

        public int Append(ReadOnlySpan<float> v, ReadOnlySpan<float> n, ReadOnlySpan<long> corners,
            Seq<(EncodingChannel Channel, float[] Raw)> attributes = default, Option<string> material = default) {
            blocks.Add(new MeshBlock(VertexCount, v.Length / 3, indices.WrittenCount, corners.Length,
                attributes.Map(static lane => lane.Channel), material));
            blockLanes.Add(attributes);
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

        public MeshSoup Baked(MeshChunk block) => Place(Append(block), Matrix4x4.Identity);

        public MeshSoup Baked(ReadOnlySpan<float> v, ReadOnlySpan<float> n, ReadOnlySpan<long> corners,
            Seq<(EncodingChannel Channel, float[] Raw)> attributes = default, Option<string> material = default) =>
            Place(Append(v, n, corners, attributes, material), Matrix4x4.Identity);

        // Materialization mints the seam carrier's ONE kernel arena through the raw-lane entry: Position and Normal
        // every source declares, plus one lane per attribute channel some block carried. Absence is a MISSING
        // DESCRIPTOR, never a zero-length column a consumer length-probes — so the per-lane `if (!uvs.IsEmpty)`
        // guard the parallel-column form needed at every reader deletes with the columns themselves, and a new
        // EncodingChannel row costs this builder NOTHING: the pool fold is channel-generic and strides on the
        // channel's own declared arity.
        // Refusal from the mint (a lane whose length disagrees with the vertex count, a payload past Array.MaxLength,
        // a round-trip error past the channel dtype's tolerance) throws INSIDE this boundary capsule, so the arm's
        // own funnel lifts it as a typed BimFault rather than a half-built arena leaving the decode.
        // Minting IS every arm's terminal decode phase, so the Assembled row beats HERE — one owner rather than the
        // same closing line copied into each arm's tail.
        public ImportedGeometry ToGeometry(InterchangeFormat format, Instant at, Option<BimHooks> hooks, Op key) {
            Seq<(EncodingChannel Channel, float[] Raw)> lanes =
                Seq((EncodingChannel.Position, vertices.WrittenSpan.ToArray()), (EncodingChannel.Normal, normals.WrittenSpan.ToArray()))
                + Declared().Map(Pooled);
            var geometry = new ImportedGeometry(format.Key,
                Encode.Of(VertexCount, lanes).ThrowIfFail(), indices.WrittenSpan.ToArray(),
                VertexCount, indices.WrittenCount / 3, toSeq(blocks), toSeq(instances), at);
            DecodeStage.Assembled.Beat(hooks, key);
            return geometry;
        }

        // Release brackets ACQUISITION: three pooled writers rent at construction, so the builder is IDisposable and
        // every arm `using`-brackets it. The retired form returned the rents inside ToGeometry alone, so an arm that
        // refused before the mint — a malformed index run, a lane arity the mint rejects, a foreign reader throwing
        // mid-walk — leaked all three back to the caller's Try.lift funnel with nothing to return them.
        public void Dispose() {
            vertices.Dispose();
            normals.Dispose();
            indices.Dispose();
        }

        // Every attribute channel at least one block declared, in the roster's own declaration order so two decodes
        // of one file mint byte-identical arenas and their content keys agree.
        Seq<EncodingChannel> Declared() {
            var seen = toSeq(blockLanes).Bind(static lanes => lanes.Map(static lane => lane.Channel)).ToSeq();
            return EncodingChannel.Items.AsIterable().Filter(seen.Contains).ToSeq();
        }

        // One pooled lane, channel-generic: a block that declared the channel copies its own range, and a block that
        // did not leaves its range untouched so per-vertex lockstep holds across the whole lane and the arena's
        // descriptor stride stays exact. Those untouched ordinates are NEVER read as values — every consumer gates on the
        // block's own MeshBlock.Declared set, which is why the pooled lane stays dense while a partially-declaring
        // pool stays honest.
        (EncodingChannel Channel, float[] Raw) Pooled(EncodingChannel channel) {
            float[] raw = new float[VertexCount * channel.Arity];
            for (int b = 0; b < blocks.Count; b++) {
                blockLanes[b].Find(lane => lane.Channel == channel)
                    .Iter(lane => lane.Raw.CopyTo(raw.AsSpan(blocks[b].VertexOffset * channel.Arity)));
            }
            return (channel, raw);
        }
    }

    // One decoded block: the two lanes every source carries as flat triples, its 0-based corner run, the OPTIONAL
    // attribute lanes that source actually declared, and the OPTIONAL shading key its face range binds. Every
    // per-format block helper returns this ONE shape, so a format that starts carrying vertex colour, a second UV
    // set, an intensity channel, or a per-material split grows one entry in its own arm and touches no builder, no
    // pool fold, and no other arm. Material rides straight through onto the seam MeshBlock, so an arm whose source
    // declares no partition leaves it absent and no consumer reads a fabricated key.
    readonly record struct MeshChunk(
        float[] Vertices, float[] Normals, long[] Corners, Seq<(EncodingChannel Channel, float[] Raw)> Attributes,
        Option<string> Material = default);

    // dotbim.File is a FOREIGN root, so this context declares it as an external serializable root and the arm reads
    // through the generated JsonTypeInfo: reflection-mode Deserialize<T>, which a trimmed or AOT publish cannot
    // keep, is the deleted form. Wire members carry their own snake_case [JsonPropertyName], so no naming policy
    // joins this posture; TypeInfoPropertyName renames the emitted contract off `File`, which reads here as this
    // page's own System.IO.File calls.
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
    [JsonSerializable(typeof(dotbim.File), TypeInfoPropertyName = "DotBimFile")]
    public sealed partial class DotBimContext : JsonSerializerContext;

    // Managed `.bim` decode through dotbim — the `dotbim` codec: the wire is pure System.Text.Json, so the byte
    // admission deserializes dotbim.File directly (File.Read is the path-bound package form). Each pooled
    // dotbim.Mesh lands ONE block (flat XYZ triples + triangle corners, up-normal filled — the format carries none)
    // and each Element places its block by the Vector translation + quaternion Rotation, so an N-element model
    // imports N instances over one shared block, never N baked copies; Guid/Type/Info/Color semantics ride the
    // DotbimProjector, never this geometry fold.
    static ImportedGeometry DotBim(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at, Option<BimHooks> hooks, Op key) {
        var file = JsonSerializer.Deserialize(bytes.Span, DotBimContext.Default.DotBimFile)
            ?? throw new InvalidDataException("<dotbim-empty-document>");
        DecodeStage.Opened.Beat(hooks, key);
        using var soup = new MeshSoup();
        var pool = file.Meshes.AsIterable().Fold(Map<int, int>(), (map, mesh) => {
            var verts = mesh.Coordinates.Select(static c => (float)c).ToArray();
            var corners = mesh.Indices.Select(static i => (long)i).ToArray();
            var up = new float[verts.Length];
            for (int i = 2; i < up.Length; i += 3) { up[i] = 1f; }
            return map.Add(mesh.MeshId, soup.Append(verts, up, corners));
        });
        DecodeStage.Decoded.Beat(hooks, key);
        foreach (var element in file.Elements) {
            int block = pool.Find(element.MeshId)
                .IfNone(() => throw new InvalidDataException($"<dotbim-mesh-miss:{element.MeshId}>"));
            var q = new System.Numerics.Quaternion(
                (float)element.Rotation.Qx, (float)element.Rotation.Qy, (float)element.Rotation.Qz, (float)element.Rotation.Qw);
            soup.Place(block, Matrix4x4.CreateFromQuaternion(q)
                * Matrix4x4.CreateTranslation((float)element.Vector.X, (float)element.Vector.Y, (float)element.Vector.Z));
        }
        DecodeStage.Placed.Beat(hooks, key);
        return soup.ToGeometry(format, at, hooks, key);
    }

    // Managed in-process DWG/DXF decode through ACadSharp — the `acad-sharp` codec the format#FORMAT_AXIS
    // Dwg row carries. The DXF/CadDocument is the same decompile-verified reader Fabrication consumes for
    // 2D profiles; here the Bim arm folds the mesh-bearing entities onto the canonical triangle-soup.
    static class AcadReader {
        // Stream-path INSTANCE readers (new DxfReader(Stream)/new DwgReader(Stream)) carry the ICadReader event
        // surface the static Read facade hides: OnProgress (ProgressEventArgs — ReadStage + current object) registers
        // onto the Model/observability#HOOK_RAIL rasm.bim.exchange.progress observe point when a composition passes
        // hooks, so a long DWG decode surfaces stage facts with zero codec coupling; the subscription dies with the
        // using-scoped reader. Hook-less calls read identically.
        // ProgressEventArgs carries a ReadStage and the current CadObjectData — no count and no total, so no COUNTED
        // fraction exists to publish. The foreign-phase correspondence is the DecodeStage roster's OWN nullable Read
        // column, so the fraction stays the ladder's and never a per-arm schedule, and a ReadStage no row claims
        // publishes NOTHING because a StageMark carries a measured position by construction.
        public static ImportedGeometry Read(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at, Option<BimHooks> hooks, Op key) {
            using var stream = new MemoryStream(bytes.ToArray());
            using ICadReader reader = IsDxf(bytes) ? new DxfReader(stream) : new DwgReader(stream);
            reader.OnProgress += (_, args) => {
                if (DecodeStage.ByReadStage.TryGetValue(args.Stage, out DecodeStage? stage)) { stage.Beat(hooks, key); }
            };
            using var soup = new MeshSoup();
            var document = reader.Read();
            document.Entities.AsIterable().Iter(entity => Accumulate(soup, entity, hooks, key));
            DecodeStage.Placed.Beat(hooks, key);
            return soup.ToGeometry(format, at, hooks, key);

            // Inserts flatten through the package-owned Explode() — the OCS->WCS placement, Rotation, per-axis
            // scale, OCS Normal, AND the MINSERT array replication ACadSharp owns — each placed entity folded back
            // through the same classifier so a block-nested Insert recurses (Explode BAKES the placement, so every
            // block lands identity-placed). The deleted form hand-rolled an InsertPoint/XScale matrix the
            // api-acadsharp RAIL_LAW rejects: it dropped Rotation, the OCS Normal, every MINSERT instance, and every
            // block-nested Mesh/PolyfaceMesh (it walked the block's Face3D entities only).
            // This walk PARTITIONS rather than filtering: the mesh-bearing entities decode, the solid-modelling
            // family (Solid3D/Region/Body/Surface — ACIS payloads no managed evaluator in this branch tessellates)
            // fires a typed DecodeReason row on the degrade point naming its handle, and everything else is 2D profile
            // geometry Rasm.Fabrication's Loop concern owns. The retired `_ => soup` tail collapsed all three into
            // one silent drop, so a DWG whose whole content was ACIS solids imported EMPTY with a clean receipt.
            static void Accumulate(MeshSoup soup, Cad.Entity entity, Option<BimHooks> hooks, Op key) {
                switch (entity) {
                    case Cad.Mesh mesh:         Baked(soup, Faces(mesh.Vertices, mesh.Faces)); break;
                    case Cad.Face3D face:       Baked(soup, Quad(face.FirstCorner, face.SecondCorner, face.ThirdCorner, face.FourthCorner)); break;
                    case Cad.PolyfaceMesh poly: Baked(soup, Polyface(poly)); break;
                    case Cad.Insert insert:     insert.Explode().AsIterable().Iter(placed => Accumulate(soup, placed, hooks, key)); break;
                    // ModelerGeometry is the ACIS base ACadSharp seats Solid3D, Region, and CadBody under, so ONE
                    // arm covers the whole solid-modelling family and a sibling the package adds later degrades
                    // rather than falling into the profile tail — a four-name list would have missed it silently.
                    case Cad.ModelerGeometry acis:
                        DecodeReason.SolidUnevaluated.Degrade(hooks, key, acis.Handle.ToString(CultureInfo.InvariantCulture));
                        break;
                    default: break;
                }
            }

            static void Baked(MeshSoup soup, (float[] Vertices, float[] Normals, long[] Corners) block) =>
                ignore(soup.Baked(block.Vertices, block.Normals, block.Corners));
        }

        // DXF (ascii/binary) opens with "0\nSECTION" / "AutoCAD Binary DXF"; DWG with "AC10xx" — the one sniff the
        // package leaves to the caller (CadReaderFactory.GetFileFormat is filename-only and the shared Dwg row carries
        // both extensions over a byte stream), so the reader pick is a boundary kernel, never a hand-rolled DXF parse.
        static bool IsDxf(ReadOnlyMemory<byte> bytes) =>
            bytes.Length >= 4 && !(bytes.Span[0] == (byte)'A' && bytes.Span[1] == (byte)'C' && char.IsDigit((char)bytes.Span[2]));

        // POLYLINE/AcDbPolyFaceMesh: the VertexFaceMesh vertex pool with the 1-based signed VertexFaceRecord index
        // records (a negative index marks a hidden edge -> abs, a zero Index4 marks a triangle), fan-triangulated to
        // a 0-based block the shared MeshSoup offsets into the pool.
        static (float[] Vertices, float[] Normals, long[] Corners) Polyface(Cad.PolyfaceMesh poly) {
            var pool = poly.Vertices.Select(static v => v.Location).ToList();
            var corners = poly.Faces.SelectMany(static f => {
                long a = Math.Abs(f.Index1) - 1, b = Math.Abs(f.Index2) - 1, c = Math.Abs(f.Index3) - 1;
                return f.Index4 == 0 ? new[] { a, b, c } : new[] { a, b, c, a, c, (long)Math.Abs(f.Index4) - 1 };
            }).ToArray();
            var (verts, normals) = Triples(pool);
            return (verts, normals, corners);
        }

        // SubDMesh: the vertex list with the n-gon face index list (each face fan-triangulated), as a 0-based
        // triangle-soup block the shared MeshSoup offsets into the pool.
        static (float[] Vertices, float[] Normals, long[] Corners) Faces(
            System.Collections.Generic.IReadOnlyList<XYZ> vertices, System.Collections.Generic.IReadOnlyList<int[]> faces) {
            var corners = faces.SelectMany(face => Enumerable.Range(1, face.Length - 2)
                .SelectMany(k => new long[] { face[0], face[k], face[k + 1] })).ToArray();
            var (verts, normals) = Triples(vertices);
            return (verts, normals, corners);
        }

        // 3DFACE quad (fourth corner equals the third for a triangle), fan-triangulated to a 0-based block.
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

        // ONE literal-aware scan serves the section locator AND the statement split. Strip PRESERVES string literals
        // (they carry entity data), so every downstream position read must skip them: a description reading
        // 'DATASHEET-01' carries the DATA token and a part name carrying ENDSEC would relocate a section boundary
        // into the middle of an entity, silently truncating the instance graph. Bare yields each index whose
        // character sits outside a Part-21 literal (the doubled '' escape resolving as a close then a reopen, which
        // leaves the enclosed run bare for exactly one index and can never match a multi-character keyword), so a
        // token or a statement terminator inside a literal is unreachable to both consumers by construction.
        // Two independent scans were the split that let the locator and the splitter disagree about where a literal was.
        static IEnumerable<int> Bare(string text, int from) {
            bool inString = false;
            for (int i = from; i < text.Length; i++) {
                if (inString) { inString = text[i] != '\''; }
                else if (text[i] == '\'') { inString = true; }
                else { yield return i; }
            }
        }

        // First bare index at or after `from` carrying `token`; -1 when the token appears only inside literals.
        // Case-insensitive because the keyword register is the writer's, where literal exclusion is the grammar's.
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

        // ISO 10303-21 header read at its POSITIONAL grammar: FILE_SCHEMA(('<schema>')) ordinal 0 (a nested list —
        // its first text wins); FILE_NAME(name, stamp, (author), (org), preprocessor, originating_system,
        // authorization) ordinal 5 — the first-text-wins scan returned the file NAME under an "Originating" label, the
        // deleted mislabel. Both locators ride the same bare scan the sections take, so a FILE_NAME literal naming
        // FILE_SCHEMA cannot relocate the read.
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

// dotbim arm of the seam — the IElementProjection peer of the IFC SemanticProjector and the SpeckleProjector.
// It captures one already-deserialized dotbim.File internally and lowers each placed Element onto a rooted seam
// Node.Object carrying the neutral Classification("dotbim", Type), the element Guid as the 1:1 ExternalId (the
// re-ingest reconcile key Reimport matches on), and its string->string Info bag as one content-keyed PropertySet
// bound by an Assign.PropertyDefinition edge — the api-dotbim "Info keys re-bind to the canonical element" law —
// and its whole-object Color onto a content-keyed seam Node.Appearance bound by an Associate edge.
// Display geometry rides the separate ImportGeometry dotbim arm (the instancing-preserving pool), so the
// semantic node references no representation; dotbim.* never crosses past this capsule.
// This fold rides Fin because the seam AppearanceSummary.Of ADMITS its channels: an out-of-unit colour rails the
// seam's own ElementFault.ValueRejected rather than seating a summary the content key cannot reproduce.
public sealed class DotbimProjector(dotbim.File file) : IElementProjection {
    public Fin<GraphDelta> Project(ProjectionContext ctx) =>
        file.Elements.AsIterable().Fold(
            Fin.Succ(GraphDelta.Empty.Reheader(ctx.Header)),
            (acc, element) => acc.Bind(delta => Appearance(element, ctx.Header.Tolerance, ctx.Key).Map(appearance => {
                NodeId id = NodeId.Rooted();
                Node.PropertySet bag = InfoBag(element, ctx.Header.Tolerance);
                return delta
                    .Put(new Node.Object(
                        Id:              id,
                        Kind:            ObjectKind.Occurrence,
                        ExternalId:      Some(External(element)),
                        Classification:  Classification.Create("dotbim", element.Type, "", None, None, None),
                        PredefinedType:  PredefinedType.NotDefined,
                        ObjectType:      Option<string>.None,   // dotBIM carries no predefined discriminant, so no label
                        Name:            element.Type,
                        Tag:             element.MeshId.ToString(CultureInfo.InvariantCulture),
                        Representations: RepresentationContentHash.Empty,
                        History:         Option<OwnerHistory>.None,
                        Span:            SchemaSpan.From(ctx.Header.Schema)))
                    .Put(bag)
                    .Put(appearance)
                    .Link(new Relationship.Assign(id, bag.Id, AssignKind.PropertyDefinition))
                    .Link(new Relationship.Associate(id, appearance.Id, new MaterialUsage.None()));
            })));

    // dotbim's whole-object Color is DISPLAY-REFERRED 0-255 RGBA, so the ingest lands it on the SAME seam
    // appearance path export#EXPORT_RAIL sources its bytes from: Semantics/appearance#APPEARANCE_PROJECTION owns
    // this estate's one sRGB transfer pair, and Linearize IS the declared inverse of the Encode that egress runs, so
    // one curve closes the round trip. Re-reading the colour into a PropertyValue.Text hex row was the asymmetry —
    // it stranded that fact in a property bag no appearance or material consumer reads while the export half wrote
    // from the summary, so a Rasm-authored .bim lost its own colour on re-ingest. ALPHA never takes the curve:
    // coverage is linear by definition.
    // dotbim declares colour ALONE, so the remaining preimage values take the matte-dielectric reading the format's
    // own vocabulary implies (metalness 0, roughness 1, non-transmissive); inventing a metalness or a specular
    // guess from a byte tint is the deleted form. Of takes the seven frozen preimage values positionally with the
    // Op key eighth — the seam factory owns the whole AppearanceKey derivation and this capsule assembles no bytes.
    // Each node content-keys through the seam's own mint: NodeId.Content over ToCanonicalBytes with the id excluded,
    // re-stamped through Node.Relabel because a class-root [Union] Node case generates no `with`, so two identically
    // coloured elements dedup to one appearance node.
    static Fin<Node.Appearance> Appearance(dotbim.Element element, double tolerance, Op key) =>
        AppearanceSummary.Of(
            AppearanceProjection.Linearize(element.Color.R / 255.0),
            AppearanceProjection.Linearize(element.Color.G / 255.0),
            AppearanceProjection.Linearize(element.Color.B / 255.0),
            0.0, 1.0, element.Color.A / 255.0, false, key)
        .Map(summary => {
            Node.Appearance draft = new(NodeId.Content(ReadOnlySpan<byte>.Empty), summary);
            return (Node.Appearance)draft.Relabel(NodeId.Content(draft.ToCanonicalBytes(tolerance).Span));
        });

    // Re-ingest key: a Rasm-exported .bim writes the verbatim seam GlobalId to Info["globalId"] (the element
    // Guid is XxHash128-derived from it — export#EXPORT_RAIL), so the round-trip prefers it and a foreign .bim
    // falls back to the element Guid — either way Reimport reconciles on a stable ExternalId.
    static string External(dotbim.Element element) =>
        element.Info is { } info && info.TryGetValue("globalId", out string? globalId) && globalId.Length > 0
            ? globalId
            : element.Guid;

    // Info bag -> one CONTENT-KEYED PropertySet node (identical bags dedup); OccurrenceWins because a dotbim
    // element carries no type-driven inheritance, PropertySource.Import because the rows arrive on the wire. The
    // bag carries the wire's Info keys ALONE — colour is the appearance node's fact, and duplicating it here as a
    // hex text row would fork the round-trip between two carriers with different consumers. FaceColors, the
    // per-face override stream, is the one growth row and it lands on the seam colour lane, never in this bag.
    static Node.PropertySet InfoBag(dotbim.Element element, double tolerance) {
        var values = toMap((element.Info ?? new Dictionary<string, string>()).Select(static pair =>
            (PropertyName.Create(pair.Key), (PropertyValue)new PropertyValue.Text(pair.Value))));
        var seed = new Node.PropertySet(NodeId.Content([]),
            new PropertyBag("Pset_Dotbim", values, InheritanceMode.OccurrenceWins, PropertySource.Import));
        return seed with { Id = NodeId.Content(seed.ToCanonicalBytes(tolerance).Span) };
    }
}
```

## [03]-[SPECKLE_SEAM]

- Owner: `BimIo.ImportSpeckle` the Speckle display-mesh arm of the import fold (a deserialized `Speckle.Sdk.Models.Base` tree → `ImportedGeometry`), and `SpeckleProjector : IElementProjection` the Speckle host-object arm of the SEAM (the same `Base` tree → a seam `GraphDelta`, the peer of the IFC `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector`); the geometry arm is a third entrypoint on the existing `BimIo` capsule symmetric to `ImportGeometry`/`ImportIfc`, the projector an `IElementProjection` the app registers in its `Seq<IElementProjection>`, both consuming the receive-side `Base` the Persistence `Rasm.Persistence/Version/ledger#SYNC_TRANSPORTS` `IOperations.Receive` returns.
- Entry: `BimIo.ImportSpeckle(Base root, IClock clock, Op key)` projecting the display-mesh geometry to `ImportedGeometry`, and `new SpeckleProjector(root).Project(ProjectionContext ctx)` lowering the host-object graph to a seam `GraphDelta` — the geometry `Fin<T>` aborts on a graph with no displayable geometry or a malformed display mesh, projecting the Speckle exception onto `BimFault.ModelRejected(key, error.Message)` BARE at the boundary (band 2600 IS the `Expected` `Code` — no `.ToError()` hop) so domain code never sees a `Speckle.Sdk.SpeckleException`, while the projector's thrown foreign fault funnels to `ElementFault.ProjectorFaulted` at the caller's capture boundary — the `ProjectionAssembly.Assemble` `Try.lift` funnel (the seam idiom; a kernel `Op.Catch` erases the typed arm into `Fault.InvalidResult`) or the Bim-internal `BimIo.Reimport` `key.Catch`; the `Base` arrives already deserialized, so the seam mints no transport, no `IOperations` reference, and no second graph walk beyond the package-owned traversal.
- Auto: the geometry fold runs the package-owned `BaseExtensions.Flatten(Base, BaseExtensions.BaseRecursionBreaker?)` deduplicating graph walk, projects each node's `BaseExtensions.TryGetDisplayValue(Base)` display list to its `Mesh` members, and decodes each `Mesh` — the flat `vertices`/`vertexNormals` (`List<double>`, flat `x,y,z`) and length-prefixed `faces` (`List<int>`, each face `[n, i0, … i(n-1)]`) triangulate through a fan over the n-gon, scaled onto the canonical metre frame by `Units.GetConversionFactor(mesh.units, Units.Meters)` so a millimetre or foot Speckle model lands in kernel units; a node that `IsDisplayableObject` is false yet carries non-mesh geometry (`Brep`/`Surface`/`Curve` with no `displayValue`) routes its content to `tessellation#TESSELLATION_BRIDGE` over the GLB rail rather than evaluating a BRep in-process; the `SpeckleProjector` fold lowers every `DataObject` (and its `RhinoObject`/`RevitObject`/`ArchicadObject`/`TeklaObject`/`Civil3dObject`/`AutocadObject` host-object subtypes) onto a rooted seam `Node.Object` carrying the generic `Classification("speckle", speckle_type)` and the host `applicationId` as the 1:1 `ExternalId`, its `DataObject.properties` (`Dictionary<string, object?>`) into one content-keyed `PropertySet` bag node attached by an `Assign.PropertyDefinition` edge, and the `BaseExtensions.TraverseWithPath` paths folding into ONE rooted segment forest whose nearest enclosing host node is each object's `Compose.Contain` parent.
- Receipt: the `ModelLoad` receipt case carries the format key `InterchangeFormat.Glb.Key` proxy for the decoded scene, the codec key `speckle-base`, the `Base.GetTotalChildrenCount()` source object count, and elapsed; the `SpeckleProjector` contributes the host-object `GraphDelta` (its `NodeCount`/`EdgeCount` the change magnitude, the distinct `speckle_type` discriminants the seam `Classification` codes); emission rides the sink port at the composition edge.
- Packages: Speckle.Sdk, Speckle.Objects, SharpGLTF.Core, Rasm.Element, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, Rasm
- Growth: a new Speckle geometry leaf is one arm on the `DisplayMeshes` projection keyed on the `IDisplayValue<T>` payload type; a new host-object discriminant is one `Classification("speckle", speckle_type)` code on the `SpeckleProjector`'s `Node.Object`, never a parallel row family; a new containment reading is one rule on the segment forest the path fold already builds, never a second path index; a non-mesh evaluation never grows a managed Speckle tessellator — it widens the `tessellation#TESSELLATION_BRIDGE` request, never this fold.
- Boundary: `BimIo` is the page boundary capsule and the Speckle arm carries the language-owned statement forms the foreign graph walk requires; the `Base` graph is admitted exactly once — `Flatten` is the single package-owned deduplicating traversal (it caches on `Base.id`), so the seam never re-walks the tree or hand-rolls a `DynamicBase.GetMembers` recursion, and `TryGetDisplayValue`/`IsDisplayableObject` own the displayable-node vocabulary rather than a per-type `is Mesh`/`is Brep` ladder; the Speckle `Mesh.faces` length-prefixed n-gon encoding fans into the canonical triangle-soup `ImportedGeometry` at the boundary (one contiguous vertex/normal/index triple, the allocation point, never a per-face `double[]` proliferation), and a degenerate face (`n < 3`) faults the decode; non-mesh geometry never evaluates in-process — GeometryGym carries no Speckle BRep kernel and the managed branch owns no NURBS evaluator, so a `Brep`/`Surface`/`Curve` with no `displayValue` rides the companion GLB rail exactly as the IFC geometry request does, joining the same content-keyed artifact; `Speckle.Sdk`/`Speckle.Objects` are the OUTSIDE-RHINO concern (`Speckle.Sdk.Dependencies` repacks the SDK's Polly/channel/serialisation-V2 closure), so this arm composes them only in the host-neutral `Rasm.Bim` exchange assembly and the in-Rhino plugin assembly never loads them; the host-object semantic projection is the `SpeckleProjector : IElementProjection` lowering to a seam `GraphDelta` (the generic `Classification("speckle", speckle_type)`, never an IFC class).

```csharp signature
public static partial class BimIo {
    public static Fin<ImportedGeometry> ImportSpeckle(Base root, IClock clock, Op key, Option<BimHooks> hooks = default) =>
        Boundary(key, () => DisplayScene(root, clock.GetCurrentInstant(), hooks, key))
            .Bind(scene => scene.TriangleCount > 0
                ? Fin.Succ(scene)
                : Fin.Fail<ImportedGeometry>(Detail.SpeckleNoDisplay.At(key, root.speckle_type)));

    static ImportedGeometry DisplayScene(Base root, Instant at, Option<BimHooks> hooks, Op key) {
        using var soup = new MeshSoup();
        DecodeStage.Opened.Beat(hooks, key);
        toSeq(root.Flatten()
            .SelectMany(static node => node.TryGetDisplayValue()?.OfType<Mesh>() ?? Enumerable.Empty<Mesh>()))
            .Iter(mesh => { var (v, n, c) = SpeckleBlock(mesh); soup.Baked(v, n, c); });
        // Speckle display meshes arrive world-space, so every block lands identity-placed and the Placed row marks
        // no phase of its own here.
        DecodeStage.Decoded.Beat(hooks, key);
        return soup.ToGeometry(InterchangeFormat.Glb, at, hooks, key);
    }

    // Speckle Mesh -> UNWELDED triangle-soup block the shared MeshSoup folds: each length-prefixed n-gon fans to
    // triangles, each fan corner expands to its own vertex (Speckle faces index the shared vertex list, the seam
    // unwelds), the vertexNormals sampled when present else an up-normal, scaled onto the canonical metre frame by the
    // source unit — PRE-SIZED buffers over the fan count (a Speckle display mesh is world-space, so the block lands
    // identity-placed).
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

// Speckle arm of the seam — the IElementProjection peer of the IFC Projection/semantic#SEMANTIC_PROJECTOR
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
// ElementFault.ProjectorFaulted at the caller's capture boundary (ProjectionAssembly.Assemble's Try.lift funnel, or BimIo.Reimport's key.Catch), never here.
public sealed class SpeckleProjector(Base root) : IElementProjection {
    static readonly BaseRecursionBreaker Descend = static _ => false;

    public Fin<GraphDelta> Project(ProjectionContext ctx) => Fin.Succ(Lower(ctx));

    GraphDelta Lower(ProjectionContext ctx) {
        // One path-carrying deduplicating walk (the package-owned TraverseWithPath, never a DynamicBase recursion):
        // every DataObject gets a neutral rooted id, the path retained so containment is the nearest-ancestor DataObject.
        var hosts = toSeq(root.TraverseWithPath(Descend)
            .Where(static step => step.Item2 is DataObject)
            .Select(static step => (Path: step.Item1, Data: (DataObject)step.Item2, Id: NodeId.Rooted())));
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

    // DataObject -> seam Object.Occurrence node: neutral Classification("speckle", speckle_type), the host
    // applicationId the 1:1 ExternalId (the re-ingest reconcile key), no IFC representation (the display mesh rides the
    // ImportSpeckle path), NotDefined predefined (a Speckle host object carries no IFC predefined token).
    static Node ObjectNode(DataObject data, NodeId id, SchemaSpan span) => new Node.Object(
        Id:              id,
        Kind:            ObjectKind.Occurrence,
        ExternalId:      Optional(data.applicationId),
        Classification:  Classification.Create("speckle", data.speckle_type, "", None, None, None),
        PredefinedType:  PredefinedType.NotDefined,
        ObjectType:      Option<string>.None,   // a Speckle host object carries no predefined discriminant
        Name:            data.name ?? "",
        Tag:             "",
        Representations: RepresentationContentHash.Empty,
        History:         Option<OwnerHistory>.None,
        Span:            span);

    // Host parameter dictionary -> one CONTENT-KEYED PropertySet bag node, so two identical parameter sets dedup
    // to one node; the id mint excludes the bag's own id (ToCanonicalBytes drops it) by minting off a temp seed.
    // OccurrenceWins because a Speckle host object carries no type-driven inheritance.
    static Node.PropertySet BagNode(DataObject data, double tolerance) {
        var values = toMap(data.properties.Select(static pair =>
            (PropertyName.Create(pair.Key), (PropertyValue)new PropertyValue.Text(pair.Value?.ToString() ?? ""))));
        var seed = new Node.PropertySet(NodeId.Rooted(), new PropertyBag(data.speckle_type, values, InheritanceMode.OccurrenceWins, PropertySource.Import));
        return seed with { Id = NodeId.Content(seed.ToCanonicalBytes(tolerance).Span) };
    }

    // Compose.Contain edges from the namespace nesting: a host's parent is its nearest enclosing DataObject, so the
    // Speckle containment tree the flat-row projection dropped rides the neutral Compose edge a Bake fold descends;
    // a root host (no DataObject ancestor) adds none. Containment builds ONE rooted forest over the path SEGMENTS —
    // each host descends its own segments through a (parent-node, segment) node table, so the chain it walks IS its
    // ancestor list and the seat pass and the parent pass share one trie, linear in total path depth with no
    // per-probe allocation. The deleted form was the path-STRING prefix index: it answered a structural question
    // textually, joining every path on an in-band sentinel and re-joining a fresh string per ancestor probe.
    // QuikGraph owns no arm here — every container and AlgorithmExtensions entry consumes the parent relation as
    // INPUT, and deriving that relation is precisely this fold's work. Duplicate paths coalesce last-wins: an
    // ambiguous parent is one parent, never a thrown build, and two hosts at one path are never each other's parent
    // because only STRICT ancestor nodes are probed.
    static Seq<Relationship> Containment(Seq<(string[] Path, DataObject Data, NodeId Id)> hosts) {
        var nodes = new Dictionary<(int Parent, string Segment), int>();
        var owners = new Dictionary<int, NodeId>();
        // Descend is an EFFECT on the shared node table, so the mapped Seq FORCES through Strict before the seat
        // pass reads it — a deferred Seq re-enumerated at the parent pass would mint the same trie a second time.
        var seats = hosts.Map(host => (host.Id, Chain: Descend(host.Path))).Strict();
        seats.Iter(seat => owners[seat.Chain[^1]] = seat.Id);
        return seats.Choose(seat => Enclosing(seat.Chain)
            .Map(parent => (Relationship)new Relationship.Compose(parent, seat.Id, ComposeKind.Contain)));

        // Forest root is ordinal 0 — a host at the empty path seats there and encloses every other — and each
        // segment step mints or reuses its own node, so the chain runs root-first down to the host's own seat.
        int[] Descend(string[] path) {
            var chain = new int[path.Length + 1];
            for (int s = 0; s < path.Length; s++) {
                var step = (chain[s], path[s]);
                chain[s + 1] = nodes.TryGetValue(step, out int held) ? held : nodes[step] = nodes.Count + 1;
            }
            return chain;
        }

        // Nearest enclosing DataObject: the deepest STRICT ancestor node an owner seats on, so the walk stops at the
        // first hit rather than ranking every ancestor.
        Option<NodeId> Enclosing(int[] chain) {
            for (int a = chain.Length - 2; a >= 0; a--) {
                if (owners.TryGetValue(chain[a], out NodeId parent)) { return Some(parent); }
            }
            return None;
        }
    }
}
```

## [04]-[REIMPORT]

- Owner: `BimIo.Reimport` the projector-polymorphic incremental re-ingest — re-projecting a revised source through ANY `IElementProjection` and reconciling it to a prior `ElementGraph` snapshot by `ExternalId`, so a large model's minor revision costs the delta, not the whole graph; `ReimportResult` the receipt carrying the patched `ElementGraph` with the delta-cost `GraphDelta` the reconcile produces in one fold; `Reconcile` the `ExternalId`-keyed structural diff and `Remap` the node/edge id-reidentification; `TypeCandidate`/`ExportTypeCandidates` the reverse type-minting export — one row per ingested `IfcTypeObject` the `Projection/semantic#SEMANTIC_PROJECTOR` `AdmitType` reconciler left UNresolved, projected off that type node's `TypeSignatureSet` bookkeeping bag and its own property bags. Both members read a PROJECTED snapshot rather than foreign bytes, which is why they share this owner: reimport reconciles a re-projection against a prior graph, and the candidate export reads out the reconciliation the first ingest left open.
- Entry: `BimIo.Reimport(IElementProjection projector, ElementGraph prior, ProjectionContext ctx, Op key)` re-projects a revised source (the caller decodes the revised bytes once into the projector — `ImportIfc` → `new SemanticProjector(db, reconciler, profiles)`, or a `Base` → `new SpeckleProjector(root)`) and reconciles the fresh graph to `prior` by `ExternalId` (the IFC `GlobalId` / Speckle `applicationId`), emitting only the added/revised/removed nodes and edges — `Fin<T>` funnels a thrown foreign projector fault to `Projection/fault#FAULT_BAND` `ElementFault.ProjectorFaulted` through `key.Catch` and rails `ElementFault.NodeAbsent` at `Graph/element#ELEMENT_GRAPH` `Apply` on a corrupt delta; the heavy display geometry is NEVER re-tessellated because an unchanged representation content-keys identically on `RepresentationContentHash`, so the incrementality is wholly in the reconcile, the whole-file re-projection notwithstanding. `BimIo.ExportTypeCandidates(ElementGraph graph, Op key)` projects the unreconciled imported types out of ANY projected snapshot — candidacy IS the `SemanticProjector.TypeSignatureSet` bag's PRESENCE, because a resolver hit lands `CanonicalTypeSeed` with no source bag at all, so the export reads the reconciliation verdict itself and never a second trust column; `Fin<T>` rails `Model/faults#FAULT_BAND` `BimFault.ModelRejected` on a signature-bearing type node carrying no `ExternalId` and lifts the `Graph/element#ELEMENT_GRAPH` `Bake` faults unchanged.
- Auto: `Reimport` runs the projector once onto a `Genesis(ctx.Header)` seed to a fresh revised `ElementGraph`, then `Reconcile` remaps each revised rooted `Object` to its prior identity by `ExternalId` (a re-projection mints FRESH neutral Guid-v7 ids, so identity is matched on the stable external id, never the node id) and rewrites every revised node and edge through that id map; the structural diff then partitions: a remapped node absent from `prior` is `AddedNodes`, present with a non-empty `Generator.Equals` `Inequalities` member diff is `RevisedNodes` — the retained `(Before, After)` pair carrying the receipt a consumer replays that same diff over to read the exact `MemberPath` — a prior node absent from the revised set is `RemovedNodes`, and edges diff by structural equality — a non-rooted content-keyed node (Material/PropertySet/...) needs no remap because identical content already shares its `NodeId`, so only rooted Objects with a stable remapped id and changed canonical bytes are revisions; the resulting `GraphDelta` applied to `prior` yields the reconciled `Patched`, the delta IS the change set, never a second diff pass.
- Receipt: the `ReimportResult` carries the patched `ElementGraph` (the prior snapshot advanced by the incremental delta) and the forward `GraphDelta` the `Rasm.Persistence` event log stores — a delta-cost minor revision, the `Rasm.Element/Graph/delta#GRAPH_DELTA` stream appending only the changed nodes/edges, the `GraphDelta.ToCanonicalBytes` content key deduping a re-applied delta; the `Review/diff#MODEL_DIFF` `ElementChange` federation change-set is the SEPARATE review surface, not minted here.
- Packages: Rasm.Element, Generator.Equals, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new re-ingestable source is one more `IElementProjection` the caller hands `Reimport` (the IFC `SemanticProjector`, the `SpeckleProjector`, the `DotbimProjector`, a future Materials/Fabrication projector) — the reconcile is projector-agnostic, keyed only on `ExternalId`, so no second reimport entrypoint; a finer change granularity is one more `MemberPath` the SAME `Inequalities` substrate already yields, so no comparison surface grows; never a parallel delta store and never a re-tessellation of a content-key-matched representation; a new candidate axis is one `TypeCandidate` column read off the type node or its signature bag, the Materials admission fold widening on that same column, never a second export.
- Boundary: `Reconcile` keys on the seam `Object.ExternalId` (the IFC `GlobalId` / Speckle `applicationId`) — a re-projection mints fresh neutral Guid-v7 ids, so matching on the node id treats every element as new; change detection is the ONE `Generator.Equals` `Inequalities` engine `Projection/egress#IFC_EGRESS`, `export#ROUNDTRIP`, and `Review/diff#MODEL_DIFF` already share, so a whole-node canonical-byte compare is the deleted second engine — it re-quantized every measure through the document tolerance to answer a membership question and names no changed member; the `GraphDelta` is the FORWARD event delta the Persistence stream stores, distinct from the `Review/diff#MODEL_DIFF` `ElementChange` review change-set; reimport is ONE polymorphic owner over `IElementProjection`; a content-key-matched representation is never re-tessellated — its `RepresentationContentHash` is identical; the patched value is the one `Graph/element#ELEMENT_GRAPH` `ElementGraph` snapshot, never a parallel delta-model; a corrupt reconcile delta rails `Projection/fault#FAULT_BAND` `ElementFault.NodeAbsent` at `Apply`. `TypeCandidate` is the seam-declared record — `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` owns the one declaration and both this export and the `Rasm.Materials` `Component/component#CATALOGUE` `AdmitImported` fold compose it, so neither package references the other and a locally re-spelled twin is the drift defect the seam homing forecloses; `IIfcTypeReconciler` stays the Bim-declared port both ends compose by contract. Candidate rows are an IN-MEMORY projection minting NO store provenance column: `Rasm.Persistence/Version/provenance#CAUSAL_DAG` `ProvKind.Import` already attributes an imported entity off the changefeed, so a durable lineage leg here mints a second owner of one fact. Candidate identity reads the NODE, never a re-spelled bag row — `ExternalId` the 1:1 `GlobalId` projection, `Classification.Code` the IFC entity name (the `Model/elements#IFC_CLASS` row key IS that name, so a roster hit and a roster miss spell it identically), and `ObjectType`-over-`PredefinedType` the EFFECTIVE token the signature folded a `USERDEFINED` label into — so only the material and profile axes, which have no node column, read the signature bag by the row names `Projection/semantic#SEMANTIC_PROJECTOR` `ImportedSource` authors.

```csharp signature
public sealed record ReimportResult(ElementGraph Patched, GraphDelta Delta);

// TypeCandidate is DECLARED ONCE at the seam — Rasm.Element/Projection/projection#PROJECTION_CONTRACT owns the
// nine-field record — and this page COMPOSES it. Materials lowers the same row onto its railed Component.Of
// construction from its own end, so the reverse-direction loop has one shape and neither package references the
// other: the composition root folds one onto the other. A local re-spelling of the record here was the second
// declaration a field addition had to reach twice, and the two ends agreed only by inspection.

public static partial class BimIo {
    // Incremental re-ingest, projector-polymorphic over ANY IElementProjection (the IFC SemanticProjector, the Speckle
    // SpeckleProjector, a future Materials/Fabrication projector): re-project a revised source to a fresh ElementGraph,
    // reconcile it to the prior snapshot by ExternalId, and emit the delta-cost GraphDelta the Persistence event log
    // stores. The caller decodes the revised bytes once (ImportIfc -> new SemanticProjector(db, reconciler, profiles), or a Base ->
    // new SpeckleProjector(root)) and hands the projector, so reimport never re-decodes a format and stays ONE
    // polymorphic owner — the retired BimModel/ModelDiff patch over GlobalId-keyed BimElement rows is the deleted form
    // (BimModel is retired; the GraphDelta IS the forward event delta, distinct from the Review/diff#MODEL_DIFF review
    // change-set). A thrown foreign fault funnels to ElementFault.ProjectorFaulted through key.Catch; a corrupt
    // reconcile delta naming an absent endpoint rails ElementFault.NodeAbsent at Graph/element#ELEMENT_GRAPH Apply.
    public static Fin<ReimportResult> Reimport(IElementProjection projector, ElementGraph prior, ProjectionContext ctx, Op key) =>
        key.Catch(() => projector.Project(ctx))
            .Map(fresh => fresh.ReplayOnto(ElementGraph.Genesis(ctx.Header)))
            .Map(revised => Reconcile(prior, revised))
            .Bind(delta => prior.Apply(delta, key).Map(patched => new ReimportResult(patched, delta)));

    // ExternalId reconcile: a re-projection mints FRESH rooted ids (neutral Guid v7), so a rooted Object is matched to
    // its prior identity by ExternalId (the IFC GlobalId / Speckle applicationId) and the revised ids remap to the
    // prior ids — an unchanged element keeps its identity and contributes no change. A non-rooted node (Material/
    // PropertySet/...) is content-keyed, so an unchanged one already shares its NodeId and a changed one is a fresh id
    // (add + remove); only a rooted Object with the SAME remapped id and DIFFERING canonical bytes is a RevisedNode.
    // Heavy display geometry is NEVER re-tessellated because RepresentationContentHash content-keys an unchanged
    // representation identically; the forward GraphDelta applied to prior yields the reconciled revised — the delta-cost
    // minor revision, the Review/diff#MODEL_DIFF ElementChange federation surface a SEPARATE concern, not minted here.
    static GraphDelta Reconcile(ElementGraph prior, ElementGraph revised) {
        // Duplicate ExternalIds are a REAL malformed-source long tail (colliding IFC GlobalIds ship in the wild):
        // first-wins TryAdd + a claimed-prior set keep the reconcile TOTAL — the first revised claimant keeps the
        // prior identity, later duplicates keep their fresh ids (an add + a remove, never a wrong merge), where the
        // throwing ToMap builds escaped the Fin rail as an uncaught duplicate-key exception.
        var priorByExternal = prior.ObjectNodes
            .Choose(static o => o.ExternalId.Map(x => (External: x, o.Id)))
            .Fold(Map<string, NodeId>(), static (held, p) => held.TryAdd(p.External, p.Id));
        var remap = revised.ObjectNodes
            .Choose(o => o.ExternalId.Bind(x => priorByExternal.Find(x)).Map(priorId => (o.Id, Prior: priorId)))
            .Fold((Claimed: HashSet<NodeId>(), Held: Map<NodeId, NodeId>()), static (acc, p) =>
                acc.Claimed.Contains(p.Prior) ? acc : (acc.Claimed.Add(p.Prior), acc.Held.Add(p.Id, p.Prior)))
            .Held;
        NodeId Reidentify(NodeId id) => remap.Find(id).IfNone(id);
        var revisedNodes = toSeq(revised.Nodes.Values.Select(n => Remap(n, Reidentify)));
        var revisedEdges = toSeq(revised.Edges.Select(e => Remap(e, Reidentify)));
        var revisedIds = toHashSet(revisedNodes.Map(static n => n.Id));
        var added = revisedNodes.Filter(n => !prior.Nodes.ContainsKey(n.Id));
        var removed = toSeq(prior.Nodes.Keys.Where(id => !revisedIds.Contains(id)));
        // Revision detection is the ONE Generator.Equals change-detection engine this package already runs at
        // Projection/egress#IFC_EGRESS (the OwnerHistory verdict), Exchange/export#ROUNDTRIP (the fidelity metric),
        // and Review/diff#MODEL_DIFF (the AspectDelta rows): each case's generated Equals walks the SAME canonical
        // member set the writer does and exits on the first divergent member instead of projecting two whole
        // canonical-byte buffers only to compare them. That whole-node byte compare was the second engine — it
        // re-quantized every measure through the document tolerance to answer a membership question, and it could
        // report only THAT a node moved. The retained pair IS the receipt: a consumer discriminates the case and
        // replays that CASE comparer's Inequalities over Before/After to read the exact MemberPath that changed —
        // [Equatable] seats per nested case, so the abstract root carries no comparer of its own.
        var revisedPairs = revisedNodes.Choose(n => prior.Nodes.TryGetValue(n.Id, out Node? p)
            && !EqualityComparer<Node>.Default.Equals(p, n)
                ? Some((Before: p, After: n))
                : None);
        // Edge sets diff through hashed membership — the Seq.Contains scan was the deleted O(edges²) form.
        var priorEdges = toHashSet(prior.Edges);
        var revisedEdgeSet = toHashSet(revisedEdges);
        var addedEdges = revisedEdges.Filter(e => !priorEdges.Contains(e));
        var removedEdges = toSeq(prior.Edges.Where(e => !revisedEdgeSet.Contains(e)));
        return new GraphDelta(added, removed, revisedPairs, addedEdges, removedEdges, Some(revised.Header));
    }

    // Remap a node's identity to its prior identity (Object only — a content-keyed node is not in the remap, so the
    // lookup is identity for it), so an ExternalId-matched element keeps its prior NodeId across the re-projection.
    static Node Remap(Node node, Func<NodeId, NodeId> reidentify) =>
        node is Node.Object o ? o with { Id = reidentify(o.Id) } : node;

    // Projection/semantic#SEMANTIC_PROJECTOR IIfcTypeReconciler runs the FORWARD leg, reconciling each ingested
    // IfcTypeObject AGAINST a Materials-minted id; this export runs the REVERSE leg over whatever that leg left
    // unresolved, handing the Materials admission fold rows to mint FROM. Candidacy IS the TypeSignatureSet bag's
    // presence — a resolver hit lands CanonicalTypeSeed with Source None — so filtering on it reads the
    // reconciliation verdict itself rather than a second trust column that only restates it. Bake is the seam's own
    // composed read (memoized per snapshot), so one hop yields a type's attached bags where a per-consumer EdgesAt
    // hand-walk with case tests re-derives them. Provenance stays DERIVED: Rasm.Persistence
    // Version/provenance#CAUSAL_DAG ProvKind.Import attributes an imported entity off the changefeed, so no store
    // provenance column mints here.
    public static Fin<Seq<TypeCandidate>> ExportTypeCandidates(ElementGraph graph, Op key) =>
        from types in graph.ObjectNodes
            .Filter(static o => o.Kind == ObjectKind.Type)
            .Traverse(node => graph.Bake(node.Id, key).Map(baked => (Node: node, Bags: baked.Properties))).As()
        let library = Library(graph.Header.Step)
        from candidates in types
            .Choose(static type => type.Bags
                .Filter(static bag => bag.SetName == SemanticProjector.TypeSignatureSet).Head
                .Map(signature => (type.Node, type.Bags, Signature: signature)))
            .Traverse(pair => Candidate(library, pair.Node, pair.Bags, pair.Signature, key)).As()
        select candidates;

    // Identity axes read off the NODE, so no bag row name is re-spelled for them: ExternalId is the 1:1 GlobalId
    // projection, Classification.Code the IFC entity name (an IfcClass row key IS that name), and
    // ObjectType-over-PredefinedType the EFFECTIVE token — exactly what TypeSignatureOf folds a USERDEFINED
    // ElementType label into, so this reverse read reproduces the forward signature rather than approximating it.
    // Material and profile axes carry no node column, so they alone key the signature bag by the names ImportedSource
    // authors. Signature-bearing type nodes with no ExternalId are corrupt projections, never absent libraries: one
    // seed stamps id and bag together, so one without the other faults rather than exporting a keyless row.
    static Fin<TypeCandidate> Candidate(string library, Node.Object node, Seq<PropertyBag> bags, PropertyBag signature, Op key) =>
        node.ExternalId.Match(
            Some: globalId => Fin.Succ(new TypeCandidate(
                SourceLibrary:      library,
                GlobalId:           globalId,
                IfcEntity:          node.Classification.Code,
                PredefinedToken:    node.ObjectType.IfNone(node.PredefinedType.Token),
                Name:               node.Name,
                Properties:         Rows(bags),
                MaterialName:       Text(signature, SemanticProjector.SignatureRows.MaterialName),
                ProfileDesignation: Text(signature, SemanticProjector.SignatureRows.ProfileDesignation),
                ProfileStandard:    Text(signature, SemanticProjector.SignatureRows.ProfileStandard))),
            None: () => Fin.Fail<TypeCandidate>(Detail.TypeCandidateIdentityMissing.At(key, node.Id.Value.ToString())));

    // Every attached bag BESIDE the bookkeeping one folds into one row map: reconciliation evidence already rides the
    // candidate's typed columns, so re-exporting that bag hands the Materials detail lane a duplicate of the columns
    // it dispatches on. Two Psets sharing a row name is ordinary IFC, so this union upserts — Map.Add throws straight
    // past the rail on the first collision.
    static Map<PropertyName, PropertyValue> Rows(Seq<PropertyBag> bags) =>
        bags.Filter(static bag => bag.SetName != SemanticProjector.TypeSignatureSet)
            .Fold(Map<PropertyName, PropertyValue>(), static (all, bag) =>
                bag.Values.AsIterable().Fold(all, static (held, row) => held.AddOrUpdate(row.Key, row.Value)));

    static Option<string> Text(PropertyBag signature, PropertyName row) =>
        signature.Values.Find(row).Bind(static value => value is PropertyValue.Text text ? Some(text.Value) : None);

    // Source-library identity is the model header's own origin: FILE_NAME's originating_system names the authoring
    // application a vendor library ships from, and FILE_NAME's name field is what a writer leaving that slot blank
    // still fills. Both blank yields the empty key — one unnamed library, never a fabricated vendor name.
    static string Library(StepHeader step) =>
        string.IsNullOrWhiteSpace(step.OriginatingSystem) ? step.Name : step.OriginatingSystem;

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

(none)
