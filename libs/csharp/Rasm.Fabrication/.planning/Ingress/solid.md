# [RASM_FABRICATION_SOLID_IMPORT]

`SolidImport` admits one detached input `SolidMesh`, one canonical `MeshSpace`, and one evidence-bearing receipt. `SolidFormat` binds each extension to its provider read; `SolidUnitPolicy` resolves millimeters; `SolidWeld` and `SolidFacePolicy` condition triangle soup; and provider handles terminate at ingress. `SolidTopology` proves input structure before kernel admission. `Read` defers boundary work on `Eff<SolidImportReceipt>`, and `SolidProjection` parameterizes egress.

## [01]-[INDEX]

| [INDEX] | [OWNER]              | [OWNS]                                            |
| :-----: | :------------------- | :------------------------------------------------ |
|  [01]   | `SolidSource`        | admitted path and complete import policy          |
|  [02]   | `SolidFormat`        | STEP/IGES/STL/3DM/3MF admission and provider read |
|  [03]   | `SolidMesh`          | detached vertices, triangles, scale, and welding  |
|  [04]   | `SolidTopology`      | incidence, shell, orientation, and genus evidence |
|  [05]   | `SolidImportReceipt` | content, provider, topology, repair, and space    |
|  [06]   | `SolidProjection`    | parameterized solid egress                        |

## [02]-[RAW_ADMISSION]

`SolidSource` is the only raw solid gate. `SolidPolicy` carries tessellation, units, weld, face admission, repair, closure, kernel tolerance, evidence key, and 3MF reader posture. `SolidFormat` carries its provider read as a constructor delegate, so a new file family is one row and no reconstructable provider vocabulary survives beside the format owner.

Native declarations are evidence, never implicit scale. `CModel.GetUnit` and `File3dm.Settings.ModelUnitSystem` provide declared units; OCCT wrapper formats carry no verified managed unit member and therefore require `SolidUnitPolicy.Assume` or `SolidUnitPolicy.Override`. `SolidUnitPolicy.Declared` rejects an absent or unitless declaration.

Provider failures carry their native detail. `OcctException` and the lib3mf binding exception lower through `IngressProviderUnavailable` with the captured message, and a 3MF defect carries `SourceLocus.ThreeMfObject` rather than an OCCT-named locus. `CReader.ReadFromBuffer` parses the same byte snapshot `SourceDigest` identifies.

## [03]-[CANONICAL_OWNER]

`SolidMesh` is the sole detached carrier. Every provider lowers to millimeter vertices with triangle indices before `MeshSpace.Of`; no provider geometry escapes. 3DM BReps admit stored face meshes from `BrepFace.GetMesh(MeshType.Any)` because Rhino3dm exposes no verified tessellator; an unmeshed face fails typed.

Conditioning precedes measurement because repair depends on measured defects. `SolidWeld.Within` fuses coincident vertices on its admitted grid, allowing unwelded STL triangles to reach a watertight verdict. `SolidFacePolicy` treats degenerate and duplicate triangles as data: `Drop` records typed evidence, while `Reject` names the face through `SourceLocus.MeshFace`.

`SolidTopology` records incidence, shell volume and orientation, Euler characteristic, genus, and bounds for the conditioned input. Watertightness reads boundary, non-manifold, and zero-volume shell counts; unused vertices remain separate evidence. `InputMesh` and `InputTopology` bind that snapshot, while `Space` and `Repair.Session.FinalStatus` bind the possibly repaired snapshot. `SolidClosure` reads input topology or final heal status. Edge census and union-find form the bounded statement kernel.

`SolidRepairPolicy` selects repair behavior as data. `HealPlan.Of` and `Heal.Repair` own repair, and `SolidRepairEvidence` retains the session. `OcctRuntime.TryGetNativeVersion` gates OCCT admission, while exact `OcctShape.BoundingBox` evidence preserves the stock envelope. `Wrapper.GetSpecificationVersion` probes 3MF extensions; `CMeshObject.IsManifoldAndOriented` records native evidence; and transforms apply before merge. Native iterator and handle scopes form the provider statement boundary.

## [04]-[PROJECTION_EGRESS]

`SolidProjection` closes egress over canonical space, detached input mesh, input topology, bounds, units, diagnostics, repair, and receipt. `SolidRepairEvidence.Session.FinalStatus` carries repaired topology status beside that explicit input snapshot. Projection never reopens the source file or reconstructs a BRep. Exact CAD and 3MF round-trip belongs to a representation-preserving owner, not this triangulating ingress.

## [05]-[IMPLEMENTATION]

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
extern alias R3;

using System.Runtime.InteropServices;
using Lib3MF;
using LanguageExt;
using LanguageExt.Common;
using OcctNet.Wrapper;
using Rasm.Domain;
using Rasm.Fabrication.Process;
using Rasm.Meshing;
using Rasm.Processing;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using R3Brep = R3::Rhino.Geometry.Brep;
using R3File = R3::Rhino.FileIO.File3dm;
using R3Mesh = R3::Rhino.Geometry.Mesh;
using R3MeshType = R3::Rhino.Geometry.MeshType;
using R3Unit = R3::Rhino.UnitSystem;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Ingress;

// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<string>]
public readonly partial struct SolidPath {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            validationError = new ValidationError(message: "solid-path:blank");
            return;
        }
        value = Path.GetFullPath(value);
    }
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SolidTolerance {
    public Length LinearDeflection { get; }
    public Angle AngularDeflection { get; }
    public Area MinimumTriangleArea { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Length linearDeflection,
        ref Angle angularDeflection,
        ref Area minimumTriangleArea) => validationError =
            double.IsFinite(linearDeflection.Millimeters) && linearDeflection.Millimeters > 0d
            && double.IsFinite(angularDeflection.Radians) && angularDeflection.Radians > 0d
            && double.IsFinite(minimumTriangleArea.SquareMillimeters) && minimumTriangleArea.SquareMillimeters > 0d
                ? null
                : new ValidationError(message: "solid-tolerance:non-positive");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolidUnitPolicy {
    private SolidUnitPolicy() { }
    public sealed record Declared : SolidUnitPolicy;
    public sealed record Assume(LengthUnit Unit) : SolidUnitPolicy;
    public sealed record Override(LengthUnit Unit) : SolidUnitPolicy;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolidWeld {
    private SolidWeld() { }
    public sealed record None : SolidWeld;
    public sealed record Within(SolidWeldTolerance Tolerance) : SolidWeld;
}

[ValueObject<Length>]
public readonly partial struct SolidWeldTolerance {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Length value) =>
        validationError = double.IsFinite(value.Millimeters) && value.Millimeters > 0d
            ? null
            : new ValidationError(message: "solid-weld:non-positive");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolidFacePolicy {
    private SolidFacePolicy() { }
    public sealed record Reject : SolidFacePolicy;
    public sealed record Drop : SolidFacePolicy;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolidClosure {
    private SolidClosure() { }
    public sealed record Surface : SolidClosure;
    public sealed record Manifold : SolidClosure;
    public sealed record Watertight : SolidClosure;
}

[SmartEnum<string>]
public sealed partial class SolidRepairPolicy {
    public static readonly SolidRepairPolicy Never = new("never", static _ => false);
    public static readonly SolidRepairPolicy Dirty = new("dirty", static topology => !topology.Watertight || !topology.Oriented);
    public static readonly SolidRepairPolicy Always = new("always", static _ => true);

    [UseDelegateFromConstructor]
    public partial bool Applies(SolidTopology topology);
}

[SmartEnum<string>]
public sealed partial class ThreeMfReadMode {
    public static readonly ThreeMfReadMode Strict = new("strict", true);
    public static readonly ThreeMfReadMode Recovery = new("recovery", false);

    public bool RejectWarnings { get; }
}

[SmartEnum<string>]
public sealed partial class ThreeMfExtension {
    public static readonly ThreeMfExtension Production =
        new("production", "http://schemas.microsoft.com/3dmanufacturing/production/2015/06");
    public static readonly ThreeMfExtension BeamLattice =
        new("beamlattice", "http://schemas.microsoft.com/3dmanufacturing/beamlattice/2017/02");
    public static readonly ThreeMfExtension Slice =
        new("slice", "http://schemas.microsoft.com/3dmanufacturing/slice/2015/07");

    public string Namespace { get; }
}

public sealed record SolidPolicy(
    SolidTolerance Tolerance,
    SolidUnitPolicy Units,
    SolidWeld Weld,
    SolidFacePolicy Faces,
    SolidClosure Closure,
    SolidRepairPolicy Repair,
    Context Context,
    Op Key,
    ThreeMfReadMode ThreeMf,
    Func<OcctBoundingBox, SolidBounds> OcctBounds);

public sealed record SolidSource(SolidPath Path, SolidPolicy Policy);

[SmartEnum<string>]
public sealed partial class SolidFormat {
    public static readonly SolidFormat Step = new(
        "step", Arr(".step", ".stp"), static (source, payload) => ReadOcct(source, payload, static path => OcctShape.ImportStep(path)));
    public static readonly SolidFormat Iges = new(
        "iges", Arr(".iges", ".igs"), static (source, payload) => ReadOcct(source, payload, static path => OcctShape.ImportIges(path)));
    public static readonly SolidFormat Stl = new(
        "stl", Arr(".stl"), static (source, payload) => ReadOcct(source, payload, static path => OcctShape.ImportStl(path)));
    public static readonly SolidFormat ThreeDm = new("3dm", Arr(".3dm"), ReadThreeDm);
    public static readonly SolidFormat ThreeMf = new("3mf", Arr(".3mf"), ReadThreeMf);

    public Arr<string> Extensions { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<SolidDetached> Read(SolidSource source, byte[] payload);

    public static Fin<SolidFormat> Admit(SolidPath path) =>
        Items.Find(format => format.Extensions.Exists(extension =>
                string.Equals(extension, Path.GetExtension(path.Value), StringComparison.OrdinalIgnoreCase)))
            .ToFin(SolidImport.Fault(path, "solid-format:unsupported"));

    static Fin<SolidDetached> ReadOcct(SolidSource source, byte[] payload, Func<string, OcctShape> import) {
        if (!OcctRuntime.TryGetNativeVersion(out string version, out string nativeError))
            return Fin.Fail<SolidDetached>(SolidImport.Fault(source.Path, nativeError));
        return Try.lift(() => Snapshot(source, payload, path => {
            using OcctShape shape = import(path);
            if (shape.IsNull)
                throw new InvalidDataException("solid-occt:null-shape");
            OcctMesh mesh = shape.Triangulate(
                source.Policy.Tolerance.LinearDeflection.Millimeters,
                source.Policy.Tolerance.AngularDeflection.Radians);
            OcctBoundingBox exact = shape.BoundingBox;
            return new SolidDetached(
                new SolidMesh(
                    mesh.Vertices.Map(static vertex => new SolidVertex(vertex.X, vertex.Y, vertex.Z)).ToArr(),
                    mesh.TriangleIndices.ToArr()),
                None,
                new SolidProviderEvidence(
                    Some(version), 1, Some(source.Policy.OcctBounds(exact)), None, Seq<SolidDiagnostic>()));
        })).Run().MapFail(error => SolidImport.Fault(source.Path, error.Message));
    }

    static Fin<SolidDetached> ReadThreeDm(SolidSource source, byte[] payload) => Try.lift<Fin<SolidDetached>>(() =>
        Snapshot(source, payload, path => {
            using R3File document = R3File.ReadWithLog(path, out string log)
                ?? throw new InvalidDataException("solid-3dm:null-document");
            Option<LengthUnit> unit = SolidImport.ThreeDmUnit(document.Settings.ModelUnitSystem);
            Seq<R3::Rhino.FileIO.File3dmObject> objects = toSeq(document.Objects).Strict();
            Seq<SolidDiagnostic> diagnostics = objects
                .Filter(row => row.Geometry is not R3Mesh and not R3Brep)
                .Map(row => (SolidDiagnostic)new SolidDiagnostic.Skipped(row.Geometry.GetType().Name))
                .Distinct().Strict();
            if (!string.IsNullOrWhiteSpace(log))
                diagnostics = diagnostics.Add(new SolidDiagnostic.Reader(None, log));
            int parts = objects.Count(static row => row.Geometry is R3Mesh or R3Brep);
            return objects.Traverse(row => (row.Geometry switch {
                    R3Mesh mesh => Fin.Succ(Seq(SolidImport.FromThreeDm(mesh))).ToValidation(),
                    R3Brep brep => toSeq(brep.Faces).Traverse(face => Optional(face.GetMesh(R3MeshType.Any))
                        .ToFin(Error.New("solid-3dm:brep-face-unmeshed"))
                        .Map(SolidImport.FromThreeDm).ToValidation()).As(),
                    _ => Fin.Succ(Seq<SolidMesh>()).ToValidation(),
                })).As().ToFin()
                .Map(static rows => rows.Bind(identity).Strict())
                .Bind(meshes => SolidImport.Merge(meshes).ToFin(Error.New("solid-3dm:no-mesh"))
                    .Map(mesh => new SolidDetached(
                        mesh, unit, new SolidProviderEvidence(None, parts, None, None, diagnostics))));
        })).Run().Bind(static result => result).MapFail(error => SolidImport.Fault(source.Path, error.Message));

    static T Snapshot<T>(SolidSource source, byte[] payload, Func<string, T> read) {
        string path = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid():N}{Path.GetExtension(source.Path.Value)}");
        try {
            File.WriteAllBytes(path, payload);
            return read(path);
        }
        finally {
            File.Delete(path);
        }
    }

    static Fin<SolidDetached> ReadThreeMf(SolidSource source, byte[] payload) => Try.lift(() => {
        Wrapper.GetLibraryVersion(out uint major, out uint minor, out uint micro);
        Seq<SolidDiagnostic> extensions = toSeq(ThreeMfExtension.Items).Choose(extension => {
            Wrapper.GetSpecificationVersion(extension.Namespace, out bool supported, out uint _, out uint _);
            return supported
                ? Option<SolidDiagnostic>.None
                : Some<SolidDiagnostic>(new SolidDiagnostic.Unsupported(extension.Key));
        });
        using CModel model = Wrapper.CreateModel();
        using CReader reader = model.QueryReader(ThreeMf.Key);
        reader.SetStrictModeActive(source.Policy.ThreeMf.RejectWarnings);
        reader.ReadFromBuffer(payload);
        using CBuildItemIterator iterator = model.GetBuildItems();
        List<SolidMesh> meshes = [];
        List<SolidDiagnostic> items = [];
        int parts = 0;
        while (iterator.MoveNext()) {
            using CBuildItem item = iterator.GetCurrent();
            using CObject resource = item.GetObjectResource();
            Seq<sTransform> transforms = item.HasObjectTransform()
                ? Seq(item.GetObjectTransform())
                : Seq<sTransform>();
            meshes.AddRange(SolidImport.FromThreeMf(resource, transforms, Set<uint>(), items));
            items.Add(new SolidDiagnostic.Part(
                resource.GetUniqueResourceID(), Optional(item.GetUUID(out bool hasUuid)).Filter(_ => hasUuid)));
            parts++;
        }
        Seq<SolidDiagnostic> warnings = Range(0, checked((int)reader.GetWarningCount())).Map(index => {
            string warning = reader.GetWarning(checked((uint)index), out uint code);
            return (SolidDiagnostic)new SolidDiagnostic.Reader(Some(code), warning);
        });
        return new SolidDetached(
            SolidImport.Merge(toSeq(meshes)).IfNone(() => throw new InvalidDataException("solid-3mf:no-build-mesh")),
            Some(SolidImport.ThreeMfUnit(model.GetUnit())),
            new SolidProviderEvidence(
                Some($"{major}.{minor}.{micro}"), parts, None,
                Optional(model.GetBuildUUID(out bool hasBuild)).Filter(_ => hasBuild),
                extensions.Concat(toSeq(items)).Concat(warnings)));
    }).Run().MapFail(error => SolidImport.Fault(source.Path, error.Message, Some(source.Path.Value)));
}

public readonly record struct SolidVertex(double X, double Y, double Z) {
    public SolidVertex Scale(double factor) => new(X * factor, Y * factor, Z * factor);
    public SolidVertex Snap(double grid) => new(
        Math.Round(X / grid) * grid, Math.Round(Y / grid) * grid, Math.Round(Z / grid) * grid);
}

public readonly record struct SolidBounds(SolidVertex Minimum, SolidVertex Maximum) {
    public SolidVertex Extent => new(
        Maximum.X - Minimum.X, Maximum.Y - Minimum.Y, Maximum.Z - Minimum.Z);

    public SolidBounds Scale(double factor) => new(Minimum.Scale(factor), Maximum.Scale(factor));

    public Length Diagonal => Length.FromMillimeters(Math.Sqrt(
        Math.Pow(Extent.X, 2d) + Math.Pow(Extent.Y, 2d) + Math.Pow(Extent.Z, 2d)));

}

public sealed partial record SolidTopology(
    int Vertices,
    int Triangles,
    int Edges,
    int BoundaryEdges,
    int NonManifoldEdges,
    int UnusedVertices,
    int Shells,
    int InwardShells,
    int ZeroVolumeShells,
    int EulerCharacteristic,
    Option<int> Genus,
    Volume SignedVolume,
    bool Oriented,
    bool Watertight,
    SolidBounds Bounds);

public sealed record SolidWeldEvidence(SolidWeld Policy, int Before, int After, int DroppedFaces);

public sealed record SolidMesh(Arr<SolidVertex> Vertices, Arr<int> TriangleIndices) {
    public int TriangleCount => TriangleIndices.Count / 3;

    public SolidMesh Scale(double factor) => new(
        Vertices.Map(vertex => vertex.Scale(factor)),
        TriangleIndices);
}

public sealed record SolidProviderEvidence(
    Option<string> NativeVersion,
    int SourceParts,
    Option<SolidBounds> Exact,
    Option<string> BuildIdentity,
    Seq<SolidDiagnostic> Diagnostics);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolidDiagnostic {
    private SolidDiagnostic() { }
    public sealed record Reader(Option<uint> Code, string Message) : SolidDiagnostic;
    public sealed record Skipped(string Geometry) : SolidDiagnostic;
    public sealed record Unsupported(string Extension) : SolidDiagnostic;
    public sealed record Part(uint Resource, Option<string> Identity) : SolidDiagnostic;
    public sealed record Native(uint Resource, bool ManifoldAndOriented) : SolidDiagnostic;
    public sealed record Degenerate(int Face, string Reason) : SolidDiagnostic;
}

public sealed record SolidUnitEvidence(
    Option<LengthUnit> Declared,
    SolidUnitPolicy Resolution,
    LengthUnit Canonical,
    double MillimeterScale);

internal sealed record SolidDetached(
    SolidMesh Mesh,
    Option<LengthUnit> DeclaredUnit,
    SolidProviderEvidence Evidence);

public sealed record SolidImportReceipt(
    UInt128 SourceDigest,
    SolidFormat Format,
    SolidUnitEvidence Units,
    SolidMesh InputMesh,
    SolidWeldEvidence Weld,
    SolidTopology InputTopology,
    MeshSpace Space,
    Option<SolidRepairEvidence> Repair,
    SolidProviderEvidence Provider);

public sealed record SolidRepairEvidence(SolidRepairPolicy Policy, HealSession Session);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolidProjection {
    private SolidProjection() { }
    public sealed record Space : SolidProjection;
    public sealed record InputMesh : SolidProjection;
    public sealed record InputTopology : SolidProjection;
    public sealed record Bounds : SolidProjection;
    public sealed record Units : SolidProjection;
    public sealed record Diagnostics : SolidProjection;
    public sealed record Repair : SolidProjection;
    public sealed record Receipt : SolidProjection;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolidView {
    private SolidView() { }
    public sealed record Space(MeshSpace Value) : SolidView;
    public sealed record InputMesh(SolidMesh Value) : SolidView;
    public sealed record InputTopology(SolidTopology Value) : SolidView;
    public sealed record Bounds(SolidBounds Value) : SolidView;
    public sealed record Units(SolidUnitEvidence Value) : SolidView;
    public sealed record Diagnostics(Seq<SolidDiagnostic> Value) : SolidView;
    public sealed record Repair(Option<SolidRepairEvidence> Value) : SolidView;
    public sealed record Receipt(SolidImportReceipt Value) : SolidView;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class SolidImport {
    public static Eff<SolidImportReceipt> Read(SolidSource source) => Eff.lift(() =>
        from raw in Try.lift(() => File.ReadAllBytes(source.Path.Value)).Run()
            .MapFail(error => Fault(source.Path, error.Message))
        from format in SolidFormat.Admit(source.Path)
        from detached in format.Read(source, raw)
        from scale in Scale(detached.DeclaredUnit, source.Policy.Units, source.Path)
        from welded in Weld(detached.Mesh.Scale(scale), source.Policy, source.Path)
        from topology in SolidTopology.Measure(welded.Mesh, source.Policy.Context)
            .MapFail(error => Fault(source.Path, error.Message))
        from space in MeshSpace.Of(Native(welded.Mesh), source.Policy.Context, key: source.Policy.Key)
        from admitted in Repair(space, topology, source.Policy)
        from _ in Closure(topology, source.Policy.Closure, admitted.Repair, source.Path)
        let provider = detached.Evidence with {
            Exact = detached.Evidence.Exact.Map(bounds => bounds.Scale(scale)),
            Diagnostics = detached.Evidence.Diagnostics.Concat(welded.Diagnostics),
        }
        select new SolidImportReceipt(
            ContentHash.Of(raw), format,
            new SolidUnitEvidence(detached.DeclaredUnit, source.Policy.Units, LengthUnit.Millimeter, scale),
            welded.Mesh, welded.Evidence, topology,
            admitted.Space, admitted.Repair,
            provider))
        .MapFail(error => error.IsExceptional ? Fault(source.Path, error.Message) : error);

    public static SolidView Project(SolidImportReceipt receipt, SolidProjection projection) => projection.Switch(
        state: receipt,
        space: static value => new SolidView.Space(value.Space),
        inputMesh: static value => new SolidView.InputMesh(value.InputMesh),
        inputTopology: static value => new SolidView.InputTopology(value.InputTopology),
        bounds: static value => new SolidView.Bounds(value.Provider.Exact.IfNone(value.InputTopology.Bounds)),
        units: static value => new SolidView.Units(value.Units),
        diagnostics: static value => new SolidView.Diagnostics(value.Provider.Diagnostics),
        repair: static value => new SolidView.Repair(value.Repair),
        receipt: static value => new SolidView.Receipt(value));

    // Vertex fusion and face admission run before measurement: an unwelded STL reports every edge as boundary,
    // so a closure demand and the repair stage it gates are both unreachable without this conditioning.
    static Fin<(SolidMesh Mesh, SolidWeldEvidence Evidence, Seq<SolidDiagnostic> Diagnostics)> Weld(
        SolidMesh mesh, SolidPolicy policy, SolidPath path) =>
        Try.lift<Fin<(SolidMesh, SolidWeldEvidence, Seq<SolidDiagnostic>)>>(() => {
        if (mesh.TriangleIndices.Count % 3 != 0)
            return Fin.Fail<(SolidMesh, SolidWeldEvidence, Seq<SolidDiagnostic>)>(
                Fault(path, "solid-mesh:triangle-arity"));
        double grid = policy.Weld switch {
            SolidWeld.Within within => within.Tolerance.Value.Millimeters,
            _ => 0d,
        };
        Dictionary<SolidVertex, int> fused = [];
        int[] remap = new int[mesh.Vertices.Count];
        List<SolidVertex> vertices = [];
        for (int index = 0; index < mesh.Vertices.Count; index++) {
            SolidVertex key = grid > 0d ? mesh.Vertices[index].Snap(grid) : mesh.Vertices[index];
            if (!fused.TryGetValue(key, out int mapped)) {
                mapped = vertices.Count;
                fused[key] = mapped;
                vertices.Add(grid > 0d ? key : mesh.Vertices[index]);
            }
            remap[index] = mapped;
        }

        HashSet<(int A, int B, int C)> seen = [];
        List<int> indices = [];
        List<SolidDiagnostic> dropped = [];
        for (int triangle = 0; triangle < mesh.TriangleCount; triangle++) {
            int a = remap[mesh.TriangleIndices[triangle * 3]];
            int b = remap[mesh.TriangleIndices[triangle * 3 + 1]];
            int c = remap[mesh.TriangleIndices[triangle * 3 + 2]];
            string? reason = a == b || b == c || a == c
                ? "solid-face:collapsed"
                : !seen.Add(Sorted(a, b, c))
                    ? "solid-face:duplicate"
                    : SolidTopology.Area(vertices[a], vertices[b], vertices[c]) is var area
                        && (!double.IsFinite(area) || area < policy.Tolerance.MinimumTriangleArea.SquareMillimeters)
                        ? "solid-face:sliver"
                        : null;
            if (reason is null) {
                indices.AddRange([a, b, c]);
                continue;
            }
            if (policy.Faces is SolidFacePolicy.Reject)
                return Fin.Fail<(SolidMesh, SolidWeldEvidence, Seq<SolidDiagnostic>)>(Fault(triangle, reason));
            dropped.Add(new SolidDiagnostic.Degenerate(triangle, reason));
        }

        return Fin.Succ<(SolidMesh, SolidWeldEvidence, Seq<SolidDiagnostic>)>((
            new SolidMesh(vertices.ToArr(), indices.ToArr()),
            new SolidWeldEvidence(policy.Weld, mesh.Vertices.Count, vertices.Count, dropped.Count),
            toSeq(dropped)));
    }).Run().MapFail(error => Fault(path, error.Message)).Bind(static result => result);

    static (int A, int B, int C) Sorted(int a, int b, int c) {
        if (a > b) (a, b) = (b, a);
        if (b > c) (b, c) = (c, b);
        if (a > b) (a, b) = (b, a);
        return (a, b, c);
    }

    static Fin<double> Scale(Option<LengthUnit> declared, SolidUnitPolicy policy, SolidPath path) => policy.Switch(
        state: declared,
        declared: static unit => unit.ToFin(Error.New("solid-unit:missing"))
            .Map(value => Length.From(1d, value).Millimeters),
        assume: static (unit, assumed) => Fin.Succ(Length.From(1d, unit.IfNone(assumed.Unit)).Millimeters),
        @override: static (_, forced) => Fin.Succ(Length.From(1d, forced.Unit).Millimeters))
        .MapFail(error => Fault(path, error.Message));

    static Fin<Unit> Closure(
        SolidTopology topology,
        SolidClosure closure,
        Option<SolidRepairEvidence> repair,
        SolidPath path) => closure.Switch(
        state: (Topology: topology, Repair: repair, Path: path),
        surface: static _ => Fin.Succ(unit),
        manifold: static value => value.Topology.NonManifoldEdges == 0 || Healed(value.Repair)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(Fault(value.Path, "solid-closure:non-manifold")),
        watertight: static value => (value.Topology.Watertight && value.Topology.Oriented) || Healed(value.Repair)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(Fault(value.Path, "solid-closure:open")));

    static bool Healed(Option<SolidRepairEvidence> repair) => repair.Exists(
        static evidence => evidence.Session.IsValid
            && evidence.Session.FinalStatus.Exists(static status => status.IsManifold && status.IsOriented
                && status.BoundaryComponents == 0 && status.NonManifoldEdges == 0));

    static Fin<(MeshSpace Space, Option<SolidRepairEvidence> Repair)> Repair(
        MeshSpace space, SolidTopology topology, SolidPolicy policy) => policy.Repair.Applies(topology)
            ? HealPlan.Of(space, key: policy.Key)
                .Bind(plan => Heal.Repair(plan, policy.Key))
                .Map(session => (session.Healed, Some(new SolidRepairEvidence(policy.Repair, session))))
            : Fin.Succ((space, Option<SolidRepairEvidence>.None));

    static Mesh Native(SolidMesh mesh) {
        Mesh native = new();
        mesh.Vertices.Iter(vertex => native.Vertices.Add(vertex.X, vertex.Y, vertex.Z));
        toSeq(Enumerable.Range(0, mesh.TriangleCount)).Iter(triangle => native.Faces.AddFace(
            mesh.TriangleIndices[triangle * 3],
            mesh.TriangleIndices[triangle * 3 + 1],
            mesh.TriangleIndices[triangle * 3 + 2]));
        return native;
    }

    internal static Seq<SolidMesh> FromThreeMf(
        CObject resource, Seq<sTransform> transforms, Set<uint> ancestors, List<SolidDiagnostic> diagnostics) {
        uint id = resource.GetUniqueResourceID();
        if (ancestors.Contains(id))
            throw new InvalidDataException($"solid-3mf:component-cycle:{id}");
        Set<uint> path = ancestors.Add(id);
        switch (resource) {
            case CMeshObject mesh:
                diagnostics.Add(new SolidDiagnostic.Native(id, mesh.IsManifoldAndOriented()));
                return Seq(Transform(FromThreeMf(mesh), transforms));
            case CComponentsObject assembly:
                return Range(0, checked((int)assembly.GetComponentCount())).Bind(index => {
                    using CComponent component = assembly.GetComponent(checked((uint)index));
                    using CObject child = component.GetObjectResource();
                    Seq<sTransform> nested = component.HasTransform()
                        ? Seq(component.GetTransform()).Concat(transforms)
                        : transforms;
                    return FromThreeMf(child, nested, path, diagnostics);
                });
            default:
                diagnostics.Add(new SolidDiagnostic.Skipped(resource.GetType().Name));
                return Seq<SolidMesh>();
        }
    }

    static SolidMesh FromThreeMf(CMeshObject mesh) {
        mesh.GetVertices(out sPosition[] vertices);
        mesh.GetTriangleIndices(out sTriangle[] triangles);
        return new SolidMesh(
            vertices.Map(static vertex => new SolidVertex(
                vertex.Coordinates[0], vertex.Coordinates[1], vertex.Coordinates[2])).ToArr(),
            triangles.Bind(static triangle => triangle.Indices.Map(static index => checked((int)index))).ToArr());
    }

    static SolidMesh Transform(SolidMesh mesh, Seq<sTransform> transforms) => new(
        mesh.Vertices.Map(vertex => transforms.Fold(vertex, static (point, transform) => Apply(transform, point))),
        mesh.TriangleIndices);

    static SolidVertex Apply(sTransform transform, SolidVertex point) => new(
        point.X * transform.Fields[0][0] + point.Y * transform.Fields[1][0] + point.Z * transform.Fields[2][0] + transform.Fields[3][0],
        point.X * transform.Fields[0][1] + point.Y * transform.Fields[1][1] + point.Z * transform.Fields[2][1] + transform.Fields[3][1],
        point.X * transform.Fields[0][2] + point.Y * transform.Fields[1][2] + point.Z * transform.Fields[2][2] + transform.Fields[3][2]);

    internal static SolidMesh FromThreeDm(R3Mesh mesh) => new(
        mesh.Vertices.ToPoint3dArray().Map(static vertex => new SolidVertex(vertex.X, vertex.Y, vertex.Z)).ToArr(),
        mesh.Faces.ToIntArray(asTriangles: true).ToArr());

    internal static Option<SolidMesh> Merge(Seq<SolidMesh> meshes) {
        if (meshes.IsEmpty)
            return None;
        (Arr<SolidVertex> Vertices, Arr<int> Indices) state = meshes.Fold(
            State: (Vertices: Arr<SolidVertex>(), Indices: Arr<int>()),
            Folder: static (state, mesh) => (
                state.Vertices.AddRange(mesh.Vertices),
                state.Indices.AddRange(mesh.TriangleIndices.Map(index => checked(index + state.Vertices.Count)))));
        return Some(new SolidMesh(state.Vertices, state.Indices));
    }

    internal static Option<LengthUnit> ThreeDmUnit(R3Unit unit) => unit switch {
        R3Unit.Angstroms => LengthUnit.Angstrom,
        R3Unit.Nanometers => LengthUnit.Nanometer,
        R3Unit.Microns => LengthUnit.Micrometer,
        R3Unit.Millimeters => LengthUnit.Millimeter,
        R3Unit.Centimeters => LengthUnit.Centimeter,
        R3Unit.Decimeters => LengthUnit.Decimeter,
        R3Unit.Meters => LengthUnit.Meter,
        R3Unit.Dekameters => LengthUnit.Decameter,
        R3Unit.Hectometers => LengthUnit.Hectometer,
        R3Unit.Kilometers => LengthUnit.Kilometer,
        R3Unit.Gigameters => LengthUnit.Gigameter,
        R3Unit.Microinches => LengthUnit.Microinch,
        R3Unit.Mils => LengthUnit.Mil,
        R3Unit.Inches => LengthUnit.Inch,
        R3Unit.Feet => LengthUnit.Foot,
        R3Unit.Yards => LengthUnit.Yard,
        R3Unit.Miles => LengthUnit.Mile,
        R3Unit.AstronomicalUnits => LengthUnit.AstronomicalUnit,
        R3Unit.LightYears => LengthUnit.LightYear,
        R3Unit.Parsecs => LengthUnit.Parsec,
        _ => None,
    };

    internal static LengthUnit ThreeMfUnit(eModelUnit unit) => unit switch {
        eModelUnit.MicroMeter => LengthUnit.Micrometer,
        eModelUnit.MilliMeter => LengthUnit.Millimeter,
        eModelUnit.CentiMeter => LengthUnit.Centimeter,
        eModelUnit.Inch => LengthUnit.Inch,
        eModelUnit.Foot => LengthUnit.Foot,
        eModelUnit.Meter => LengthUnit.Meter,
        _ => throw new InvalidDataException($"solid-3mf:unit:{unit}"),
    };

    internal static Error Fault(SolidPath path, string detail, Option<string> model = default) =>
        new FabricationFault.IngressProviderUnavailable(
            model.Match(
                Some: value => new SourceLocus.ThreeMfObject(Path.GetFileName(value), 0),
                None: () => (SourceLocus)Locus(path)),
            detail);

    internal static Error Fault(int face, string reason) =>
        new FabricationFault.IngressProviderUnavailable(new SourceLocus.MeshFace(face), reason);

    static SourceLocus.OcctShape Locus(SolidPath path) =>
        new(unchecked((int)(
            ContentHash.Of(System.Text.Encoding.UTF8.GetBytes(path.Value)) & (UInt128)int.MaxValue)));
}

public sealed partial record SolidTopology {
    readonly record struct Edge(int A, int B) {
        public static Edge Of(int a, int b) => a < b ? new(a, b) : new(b, a);
    }
    readonly record struct EdgeUse(int Forward, int Reverse, int Triangle) {
        public int Count => Forward + Reverse;
    }

    public static Fin<SolidTopology> Measure(SolidMesh mesh, Context context) => Try.lift(() => {
        if (mesh.Vertices.IsEmpty || mesh.TriangleIndices.IsEmpty || mesh.TriangleIndices.Count % 3 != 0
            || mesh.Vertices.Exists(static vertex => !double.IsFinite(vertex.X) || !double.IsFinite(vertex.Y) || !double.IsFinite(vertex.Z)))
            throw new InvalidDataException("solid-topology:structural");

        int[] parent = Enumerable.Range(0, mesh.TriangleCount).ToArray();
        Dictionary<Edge, EdgeUse> edges = [];
        HashSet<int> referenced = [];
        List<(int Triangle, double Volume)> volumes = [];
        for (int triangle = 0; triangle < mesh.TriangleCount; triangle++) {
            int a = mesh.TriangleIndices[triangle * 3];
            int b = mesh.TriangleIndices[triangle * 3 + 1];
            int c = mesh.TriangleIndices[triangle * 3 + 2];
            if (a < 0 || b < 0 || c < 0 || a >= mesh.Vertices.Count || b >= mesh.Vertices.Count || c >= mesh.Vertices.Count)
                throw new InvalidDataException($"solid-topology:index:{triangle}");
            SolidVertex va = mesh.Vertices[a];
            SolidVertex vb = mesh.Vertices[b];
            SolidVertex vc = mesh.Vertices[c];
            Add(edges, parent, triangle, a, b);
            Add(edges, parent, triangle, b, c);
            Add(edges, parent, triangle, c, a);
            referenced.Add(a);
            referenced.Add(b);
            referenced.Add(c);
            volumes.Add((triangle, Dot(va, Cross(vb, vc)) / 6d));
        }

        int boundary = edges.Values.Count(static use => use.Count == 1);
        int nonManifold = edges.Values.Count(static use => use.Count > 2);
        int unused = mesh.Vertices.Count - referenced.Count;
        Arr<(int Root, double Volume)> shellVolumes = volumes.GroupBy(row => Find(parent, row.Triangle))
            .Map(static group => (group.Key, group.Sum(static row => row.Volume))).ToArr();
        Dictionary<int, int> shellBoundaries = edges.GroupBy(edge => Find(parent, edge.Value.Triangle))
            .ToDictionary(
                static group => group.Key,
                static group => group.Count(edge => edge.Value.Count == 1));
        if (shellVolumes.Exists(static shell => !double.IsFinite(shell.Volume)))
            throw new InvalidDataException("solid-topology:non-finite-volume");
        double volumeFloor = Math.Pow(context.Absolute.Value, 3d);
        int inward = shellVolumes.Count(shell => shellBoundaries.GetValueOrDefault(shell.Root) == 0
            && shell.Volume < -volumeFloor);
        int zeroVolume = shellVolumes.Count(shell => shellBoundaries.GetValueOrDefault(shell.Root) == 0
            && Math.Abs(shell.Volume) <= volumeFloor);
        double signedVolume = shellVolumes.Sum(static shell => shell.Volume);
        bool oriented = inward == 0 && zeroVolume == 0
            && edges.Values.ForAll(static use => use.Count < 2 || use is { Forward: 1, Reverse: 1 });
        bool watertight = boundary == 0 && nonManifold == 0 && zeroVolume == 0;
        int euler = Enumerable.Range(0, mesh.TriangleCount)
            .GroupBy(triangle => Find(parent, triangle))
            .Sum(group => group
                .SelectMany(triangle => new[] {
                    mesh.TriangleIndices[triangle * 3],
                    mesh.TriangleIndices[triangle * 3 + 1],
                    mesh.TriangleIndices[triangle * 3 + 2],
                })
                .Distinct()
                .Count()
                - edges.Count(edge => Find(parent, edge.Value.Triangle) == group.Key)
                + group.Count());
        return new SolidTopology(
            mesh.Vertices.Count, mesh.TriangleCount, edges.Count, boundary, nonManifold, unused,
            shellVolumes.Count, inward, zeroVolume,
            euler,
            watertight && oriented ? Some((2 * shellVolumes.Count - euler) / 2) : None,
            Volume.FromCubicMillimeters(signedVolume), oriented, watertight,
            Bounds(mesh.Vertices));
    }).Run();

    internal static double Area(SolidVertex a, SolidVertex b, SolidVertex c) =>
        0.5d * Length(Cross(Subtract(b, a), Subtract(c, a)));

    static SolidVertex Subtract(SolidVertex left, SolidVertex right) =>
        new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    static SolidVertex Cross(SolidVertex left, SolidVertex right) => new(
        left.Y * right.Z - left.Z * right.Y,
        left.Z * right.X - left.X * right.Z,
        left.X * right.Y - left.Y * right.X);

    static double Dot(SolidVertex left, SolidVertex right) => left.X * right.X + left.Y * right.Y + left.Z * right.Z;
    static double Length(SolidVertex value) => Math.Sqrt(Dot(value, value));

    static SolidBounds Bounds(Arr<SolidVertex> vertices) => vertices.Tail.Fold(
        State: new SolidBounds(vertices.Head, vertices.Head),
        Folder: static (bounds, vertex) => new(
            new SolidVertex(Math.Min(bounds.Minimum.X, vertex.X), Math.Min(bounds.Minimum.Y, vertex.Y), Math.Min(bounds.Minimum.Z, vertex.Z)),
            new SolidVertex(Math.Max(bounds.Maximum.X, vertex.X), Math.Max(bounds.Maximum.Y, vertex.Y), Math.Max(bounds.Maximum.Z, vertex.Z))));

    static void Add(Dictionary<Edge, EdgeUse> edges, int[] parent, int triangle, int from, int to) {
        Edge edge = Edge.Of(from, to);
        if (edges.TryGetValue(edge, out EdgeUse use)) {
            Union(parent, triangle, use.Triangle);
            edges[edge] = from < to
                ? use with { Forward = use.Forward + 1 }
                : use with { Reverse = use.Reverse + 1 };
        }
        else {
            edges[edge] = from < to
                ? new EdgeUse(1, 0, triangle)
                : new EdgeUse(0, 1, triangle);
        }
    }

    static int Find(int[] parent, int value) {
        while (parent[value] != value) {
            parent[value] = parent[parent[value]];
            value = parent[value];
        }
        return value;
    }

    static void Union(int[] parent, int left, int right) {
        int a = Find(parent, left);
        int b = Find(parent, right);
        if (a != b)
            parent[b] = a;
    }
}
```

## [06]-[RESEARCH]

- `OcctBoundingBox` component member spelling (`MinX`/`MaxX` versus a `Corner`/`Size` pair) needs `assay api query --key occtnet.wrapper --symbol OcctNet.Wrapper.OcctBoundingBox`; `SolidPolicy.OcctBounds` is the provider adapter for the uncatalogued accessor family.
- `Lib3MF.CBuildItem.GetUUID(out bool)` and `CModel.GetBuildUUID(out bool)` return shapes are catalogued as production-extension identity but not verified for nullability; `SolidDiagnostic.Part` binds the verified return once decompiled.
