# [RASM_FABRICATION_SOLID_IMPORT]

`SolidImport` admits one detached input `SolidMesh`, one canonical `MeshSpace`, and one evidence-bearing receipt. `SolidFormat` binds each admitted extension to its provider read, `SolidPolicy` ADMITS tessellation through reader posture as one gated value, `SolidWeld` and `SolidFacePolicy` condition triangle soup ahead of measurement, `SolidTopology` proves input structure before kernel admission, and provider handles terminate at ingress. `SolidProjection` carries its own view delegate over the settled receipt.

`MeshSpace`, `HealPlan`, `Heal`, `HealSession`, `Context`, and `Op` arrive settled from the kernel meshing and processing owners; `SourceSnapshot` arrives settled from `Ingress/profile#RAW_ADMISSION` as the sub-domain's ONE byte-to-path materialization. `ContentHash.Of` is the one kernel digest mint every fabrication egress key seeds from. `Process/faults` allocates this lane `IngressProviderUnavailable` over `SourceLocus.OcctShape`, `SourceLocus.ThreeMfObject`, and `SourceLocus.MeshFace`, `IngressGeometryUnfit` for a structurally inadmissible mesh, and `PolicyInadmissible` on `FabConcern.Ingress` for every declared-value refusal. Public entries defer boundary work on `Eff`, and each closes its `Fin` back onto that rail through `ToEff` rather than publishing a nested carrier.

## [01]-[INDEX]

- [02]-[RAW_ADMISSION]: `SolidSource` the one raw gate, `SolidPolicy` admitting tessellation through reader posture, `SolidUnits` the one provider-unit correspondence, `SolidFormat` holding each STEP/IGES/STL/3DM/3MF provider read as a constructor delegate, and provider failures lowered through typed loci.
- [03]-[CANONICAL_OWNER]: `SolidMesh` the sole detached millimeter carrier, weld and face conditioning ahead of measurement, `SolidTopology` incidence, shell, orientation, and genus evidence derived in one sweep and one per-shell pass, and policy-selected repair retaining its session.
- [04]-[PROJECTION_EGRESS]: `SolidProjection` closing egress over canonical space, the detached input snapshot, topology, bounds, units, diagnostics, and repair, reopening no source file.

## [02]-[RAW_ADMISSION]

- Owner: `SolidSource` is the one raw solid gate over a `SolidPath` and a `SolidPolicy`; `SolidFormat` binds each admitted extension to its provider read as a constructor delegate; `SolidTolerance`, `SolidUnitPolicy`, `SolidWeld`, `SolidFacePolicy`, `SolidClosure`, `SolidRepairPolicy`, and `ThreeMfReadMode` carry tessellation, unit, conditioning, closure, repair, and reader posture as ADMITTED values; `SolidUnits` owns the two provider-unit correspondences; `SolidDetached` is the internal carrier every provider read lands on and `SolidProviderEvidence` retains what that provider published.
- Cases: `SolidFormat` closes step · iges · stl · 3dm · 3mf; `SolidUnitPolicy` closes declared · assume · override; `SolidWeld` closes none · quantized; `SolidFacePolicy` closes reject · drop; `SolidClosure` closes surface · manifold · watertight; `SolidRepairPolicy` closes never · dirty · always over its `Applies` predicate column; `ThreeMfReadMode` closes strict · recovery; `ThreeMfExtension` closes the production, beam-lattice, slice, and volumetric namespaces the vendored native carries; `SolidDiagnostic` closes reader · skipped · unsupported · part · native · degenerate.
- Law: `SolidWeld` names what it does. `Quantized` SNAPS vertices onto a declared grid and coalesces the collisions — that is a quantization, not a proximity weld, and two vertices a hair apart across a grid line stay distinct under it; `None` coalesces NOTHING, so an unwelded triangle soup reports every edge as boundary exactly as the source carries it. The prior `Within(tolerance)` spelling promised a within-tolerance fusion the grid never performed and still coalesced under its own `None` case.
- Law: BOTH provider unit correspondences answer on `Option`. A 3MF model unit the binding adds and a 3DM unit the alias adds are the same absence, so neither throws and neither defaults — `SolidUnits` is one table family with one shape. Mapperly is REFUSED here and the refusal is structural: its emission is `global::`-qualified exclusively with no extern-alias machinery, so it cannot address the `R3` unit at all, and splitting one correspondence across two mechanisms is the worse fork.
- Entry: `SolidFormat.Admit(SolidPath)` resolves an extension to its row, and `format.Read(SolidSource, byte[])` is the one provider leg returning `Fin<SolidDetached>` — every provider handle opens and closes inside it.
- Auto: a native declaration is evidence, never implicit scale — `CModel.GetUnit` and `File3dm.Settings.ModelUnitSystem` publish a declared unit while the OCCT formats publish none, so `SolidUnitPolicy.Declared` refuses an absent declaration and `Assume` or `Override` is how an OCCT source admits at all; the 3MF read censuses its component EDGES first and rails `IsDirectedAcyclicGraph` before any traversal runs, so a cyclic component graph refuses typed rather than being caught by an ancestor set threaded down a recursion; a 3DM BRep admits its stored face meshes because the read-side binding exposes no tessellator, and an unmeshed face fails typed rather than silently dropping a solid; `CReader.ReadFromBuffer` parses the same byte snapshot `SourceDigest` identifies, so the digest names exactly what was read.
- Output: one `SolidDetached` — the merged provider-unit `SolidMesh`, the declared `LengthUnit` where the provider published one, and `SolidProviderEvidence` carrying native version, source part count, the exact provider bounds, build identity, and the diagnostic stream.
- Packages: `OcctNet.Wrapper` owns STEP/IGES/STL B-rep read and tessellation; `lib3mf` owns 3MF model, reader, build-item, and component traversal; `Rhino3dm` under `extern alias R3` owns 3DM document and stored face-mesh read; `QuikGraph` rails the component-graph acyclicity; `Thinktecture.Runtime.Extensions` owns the closed policy families and their admission; `UnitsNet` owns every declared length, angle, and area; `LanguageExt.Core` owns the rails and immutable carriers.
- Growth: a new file family is one `SolidFormat` row carrying its extensions and its read delegate; a new provider observation is one `SolidDiagnostic` case; a new 3MF extension probe is one `ThreeMfExtension` row; a new reader posture is one `ThreeMfReadMode` row; a new provider unit is one `SolidUnits` row.
- Exemption: the `using` acquisition of every `OcctShape`, `CModel`, `CReader`, iterator, build item, and `File3dm` is the provider statement kernel — a resource boundary is the law it expresses, not an accumulation.
- Boundary: no provider handle, exception type, or geometry type escapes this cluster; `OcctException` and the lib3mf binding exception lower through `IngressProviderUnavailable` with the captured message, and a 3MF defect carries `SourceLocus.ThreeMfObject` rather than an OCCT-named locus; `OcctRuntime.TryGetNativeVersion` gates OCCT admission so a missing native toolkit refuses typed rather than escaping as a load failure.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
extern alias R3;

using System.Collections.Frozen;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Lib3MF;
using LanguageExt;
using LanguageExt.Common;
using OcctNet.Wrapper;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Collections;
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
using R3Object = R3::Rhino.FileIO.File3dmObject;
using R3Unit = R3::Rhino.UnitSystem;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Ingress;

// --- [RAW_ADMISSION] ----------------------------------------------------------------------
[ValueObject<string>]
[ValidationError<FabricationFault>]
public readonly partial struct SolidPath {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref string value) {
        if (!Witness.Keyed(value)) {
            validationError = IngressFault.Policy("solid-path:blank");
            return;
        }
        value = Path.GetFullPath(value);
    }

    public static Fin<SolidPath> Admit(string value) => Admission.OfValue<SolidPath, string>(value);
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SolidTolerance {
    public Length LinearDeflection { get; }
    public Angle AngularDeflection { get; }
    public Area MinimumTriangleArea { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref Length linearDeflection,
        ref Angle angularDeflection,
        ref Area minimumTriangleArea) {
        if (!(Witness.Positive(linearDeflection.Millimeters)
            && Witness.Positive(angularDeflection.Radians)
            && Witness.Positive(minimumTriangleArea.SquareMillimeters)))
            validationError = IngressFault.Policy("solid-tolerance:non-positive");
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolidUnitPolicy {
    private SolidUnitPolicy() { }
    public sealed record Declared : SolidUnitPolicy;
    public sealed record Assume(LengthUnit Unit) : SolidUnitPolicy;
    public sealed record Override(LengthUnit Unit) : SolidUnitPolicy;
}

[ValueObject<Length>]
[ValidationError<FabricationFault>]
public readonly partial struct SolidWeldGrid {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref Length value) =>
        validationError = Witness.Positive(value.Millimeters) ? null : IngressFault.Policy("solid-weld:non-positive");

    public static Fin<SolidWeldGrid> Admit(Length value) => Admission.OfValue<SolidWeldGrid, Length>(value);
}

// `Quantized` SNAPS onto the declared grid and coalesces the collisions; `None` coalesces nothing at all. Neither
// row performs a proximity weld, so neither is named for one.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolidWeld {
    private SolidWeld() { }
    public sealed record None : SolidWeld;
    public sealed record Quantized(SolidWeldGrid Grid) : SolidWeld;

    public Option<double> GridMm => Switch(
        none: static _ => Option<double>.None,
        quantized: static row => Some(row.Grid.Value.Millimeters));
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
    // The volumetric namespace alone rides `schemas.3mf.io` where every other extension the vendored native carries
    // rides `schemas.microsoft.com`, so the host is per-row rather than a prefix the vocabulary could factor out.
    public static readonly ThreeMfExtension Volumetric =
        new("volumetric", "http://schemas.3mf.io/3dmanufacturing/volumetric/2022/01");

    public string Namespace { get; }
}

// The ONE provider-unit correspondence. Both tables answer on `Option` and neither defaults, so a unit a binding
// release adds refuses at the read rather than resolving to whatever arm a `_` pattern happened to carry.
public static class SolidUnits {
    private static readonly FrozenDictionary<R3Unit, LengthUnit> Rhino = new Dictionary<R3Unit, LengthUnit> {
        [R3Unit.Angstroms] = LengthUnit.Angstrom,
        [R3Unit.Nanometers] = LengthUnit.Nanometer,
        [R3Unit.Microns] = LengthUnit.Micrometer,
        [R3Unit.Millimeters] = LengthUnit.Millimeter,
        [R3Unit.Centimeters] = LengthUnit.Centimeter,
        [R3Unit.Decimeters] = LengthUnit.Decimeter,
        [R3Unit.Meters] = LengthUnit.Meter,
        [R3Unit.Dekameters] = LengthUnit.Decameter,
        [R3Unit.Hectometers] = LengthUnit.Hectometer,
        [R3Unit.Kilometers] = LengthUnit.Kilometer,
        [R3Unit.Gigameters] = LengthUnit.Gigameter,
        [R3Unit.Microinches] = LengthUnit.Microinch,
        [R3Unit.Mils] = LengthUnit.Mil,
        [R3Unit.Inches] = LengthUnit.Inch,
        [R3Unit.Feet] = LengthUnit.Foot,
        [R3Unit.Yards] = LengthUnit.Yard,
        [R3Unit.Miles] = LengthUnit.Mile,
        [R3Unit.AstronomicalUnits] = LengthUnit.AstronomicalUnit,
        [R3Unit.LightYears] = LengthUnit.LightYear,
        [R3Unit.Parsecs] = LengthUnit.Parsec,
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<eModelUnit, LengthUnit> ThreeMf = new Dictionary<eModelUnit, LengthUnit> {
        [eModelUnit.MicroMeter] = LengthUnit.Micrometer,
        [eModelUnit.MilliMeter] = LengthUnit.Millimeter,
        [eModelUnit.CentiMeter] = LengthUnit.Centimeter,
        [eModelUnit.Inch] = LengthUnit.Inch,
        [eModelUnit.Foot] = LengthUnit.Foot,
        [eModelUnit.Meter] = LengthUnit.Meter,
    }.ToFrozenDictionary();

    public static Option<LengthUnit> Of(R3Unit unit) =>
        Rhino.TryGetValue(unit, out LengthUnit mapped) ? Some(mapped) : None;

    public static Option<LengthUnit> Of(eModelUnit unit) =>
        ThreeMf.TryGetValue(unit, out LengthUnit mapped) ? Some(mapped) : None;

    public static double Millimeters(LengthUnit unit) => Length.From(1d, unit).Millimeters;
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class SolidPolicy {
    public SolidTolerance Tolerance { get; }
    public SolidUnitPolicy Units { get; }
    public SolidWeld Weld { get; }
    public SolidFacePolicy Faces { get; }
    public SolidClosure Closure { get; }
    public SolidRepairPolicy Repair { get; }
    public Context Context { get; }
    public Op Key { get; }
    public ThreeMfReadMode ThreeMf { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref SolidTolerance tolerance,
        ref SolidUnitPolicy units,
        ref SolidWeld weld,
        ref SolidFacePolicy faces,
        ref SolidClosure closure,
        ref SolidRepairPolicy repair,
        ref Context context,
        ref Op key,
        ref ThreeMfReadMode threeMf) {
        // A weld grid below the model grid coalesces nothing the kernel would not already treat as coincident, and
        // a watertight demand under `Never` can only be met by a source that was already closed.
        if (weld.GridMm.Exists(grid => grid < context.Absolute.Value))
            validationError = IngressFault.Policy("solid-weld:grid-below-model");
        else if (closure is SolidClosure.Watertight && repair == SolidRepairPolicy.Never)
            validationError = IngressFault.Policy("solid-closure:unrepairable-demand");
    }

    public static Fin<SolidPolicy> Admit(
        SolidTolerance tolerance,
        SolidUnitPolicy units,
        SolidWeld weld,
        SolidFacePolicy faces,
        SolidClosure closure,
        SolidRepairPolicy repair,
        Context context,
        Op key,
        ThreeMfReadMode threeMf) =>
        Validate(tolerance, units, weld, faces, closure, repair, context, key, threeMf, out SolidPolicy policy)
            .Admitted(policy);
}

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
        toSeq(Items).Find(format => format.Extensions.Exists(extension =>
                string.Equals(extension, Path.GetExtension(path.Value), StringComparison.OrdinalIgnoreCase)))
            .ToFin(SolidImport.Fault(path, "solid-format:unsupported"));

    // `TryGetNativeVersion` runs BEFORE the snapshot: a missing OCCT toolkit refuses typed here, where a
    // load failure raised inside the read would survive only as an exception message.
    private static Fin<SolidDetached> ReadOcct(SolidSource source, byte[] payload, Func<string, OcctShape> import) =>
        OcctRuntime.TryGetNativeVersion(out string version, out string nativeError)
        ? Try.lift(() => SourceSnapshot.With(payload, Path.GetExtension(source.Path.Value), path => {
            using OcctShape shape = import(path);
            if (shape.IsNull)
                throw new InvalidDataException("solid-occt:null-shape");
            OcctMesh mesh = shape.Triangulate(
                source.Policy.Tolerance.LinearDeflection.Millimeters,
                source.Policy.Tolerance.AngularDeflection.Radians);
            // `BoundingBox` reads the exact B-rep BEFORE the triangle soup replaces it, so the stock
            // envelope carries the solid's own extents rather than a tessellation-tightened hull.
            OcctBoundingBox exact = shape.BoundingBox;
            return new SolidDetached(
                new SolidMesh(
                    mesh.Vertices.Map(static vertex => new SolidVertex(vertex.X, vertex.Y, vertex.Z)).ToArr(),
                    mesh.TriangleIndices.ToArr()),
                None,
                new SolidProviderEvidence(
                    Some(version), 1,
                    Some(new SolidBounds(
                        new SolidVertex(exact.MinX, exact.MinY, exact.MinZ),
                        new SolidVertex(exact.MaxX, exact.MaxY, exact.MaxZ))),
                    None, Seq<SolidDiagnostic>()));
        })).Run().MapFail(error => SolidImport.Fault(source.Path, error.Message))
        : Fin.Fail<SolidDetached>(SolidImport.Fault(source.Path, nativeError));

    private static Fin<SolidDetached> ReadThreeDm(SolidSource source, byte[] payload) =>
        Try.lift(() => SourceSnapshot.With(payload, ".3dm", path => {
            using R3File document = R3File.ReadWithLog(path, out string log)
                ?? throw new InvalidDataException("solid-3dm:null-document");
            Option<LengthUnit> unit = SolidUnits.Of(document.Settings.ModelUnitSystem);
            Seq<R3Object> objects = toSeq(document.Objects).Strict();
            Seq<SolidDiagnostic> skipped = objects
                .Filter(static row => row.Geometry is not R3Mesh and not R3Brep)
                .Map(static row => (SolidDiagnostic)new SolidDiagnostic.Skipped(row.Geometry.GetType().Name))
                .Distinct().Strict();
            Seq<SolidDiagnostic> diagnostics = Witness.Keyed(log)
                ? skipped.Add(new SolidDiagnostic.Reader(None, log))
                : skipped;
            int parts = objects.Count(static row => row.Geometry is R3Mesh or R3Brep);
            return objects
                .Traverse(row => Geometry(row, source.Path).ToValidation()).As().ToFin()
                .Map(static rows => rows.Bind(identity).Strict())
                .Bind(meshes => SolidImport.Merge(meshes)
                    .ToFin(SolidImport.Unfit(source.Path, "solid-3dm:no-mesh"))
                    .Map(mesh => new SolidDetached(
                        mesh, unit, new SolidProviderEvidence(None, parts, None, None, diagnostics))));
        })).Run()
        .Bind(static result => result)
        .MapFail(error => error.IsExceptional ? SolidImport.Fault(source.Path, error.Message) : error);

    private static Fin<Seq<SolidMesh>> Geometry(R3Object row, SolidPath path) => row.Geometry switch {
        R3Mesh mesh => Fin.Succ(Seq(SolidImport.FromThreeDm(mesh))),
        R3Brep brep => toSeq(brep.Faces).Traverse(face => Optional(face.GetMesh(R3MeshType.Any))
            .ToFin(SolidImport.Unfit(path, "solid-3dm:brep-face-unmeshed"))
            .Map(SolidImport.FromThreeDm).ToValidation()).As().ToFin(),
        _ => Fin.Succ(Seq<SolidMesh>()),
    };

    // 3MF takes the buffer directly, so it never materializes a path at all. The component graph is censused as
    // EDGES and railed acyclic BEFORE any mesh is read, so a cyclic model refuses typed rather than being caught
    // by an ancestor set threaded down a recursion that already opened half the resources.
    private static Fin<SolidDetached> ReadThreeMf(SolidSource source, byte[] payload) => Try.lift(() => {
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
        List<(CObject Resource, Seq<sTransform> Placement, uint Id, Option<string> Identity)> items = [];
        while (iterator.MoveNext()) {
            using CBuildItem item = iterator.GetCurrent();
            CObject resource = item.GetObjectResource();
            // Lib3MF returns an EMPTY string, never null, when the production extension stamped no identity,
            // so the `out bool` carries the only absence signal and an `Optional` wrap admits `""` as present.
            string uuid = item.GetUUID(out bool hasUuid);
            items.Add((
                resource,
                item.HasObjectTransform() ? Seq(item.GetObjectTransform()) : Seq<sTransform>(),
                resource.GetUniqueResourceID(),
                hasUuid ? Some(uuid) : None));
        }
        Seq<SolidDiagnostic> warnings = Range(0, checked((int)reader.GetWarningCount())).Map(index => {
            string warning = reader.GetWarning(checked((uint)index), out uint code);
            return (SolidDiagnostic)new SolidDiagnostic.Reader(Some(code), warning);
        });
        string build = model.GetBuildUUID(out bool hasBuild);
        Seq<SolidDiagnostic> parts = toSeq(items)
            .Map(static row => (SolidDiagnostic)new SolidDiagnostic.Part(row.Id, row.Identity));
        return SolidImport
            .Acyclic(toSeq(items).Map(static row => row.Resource), source.Path)
            .Bind(_ => toSeq(items)
                .Traverse(row => SolidImport.FromThreeMf(row.Resource, row.Placement).ToValidation()).As().ToFin())
            .Bind(rows => SolidImport
                .Merge(rows.Bind(static row => row.Meshes))
                .ToFin(SolidImport.Unfit(source.Path, "solid-3mf:no-build-mesh"))
                .Map(mesh => new SolidDetached(
                    mesh,
                    SolidUnits.Of(model.GetUnit()),
                    new SolidProviderEvidence(
                        Some($"{major}.{minor}.{micro}"), items.Count, None,
                        hasBuild ? Some(build) : None,
                        extensions + parts + rows.Bind(static row => row.Diagnostics) + warnings))));
    }).Run()
        .Bind(static result => result)
        .MapFail(error => error.IsExceptional
            ? SolidImport.Fault(source.Path, error.Message, Some(source.Path.Value))
            : error);
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

internal sealed record SolidDetached(
    SolidMesh Mesh,
    Option<LengthUnit> DeclaredUnit,
    SolidProviderEvidence Evidence);

// The component fold's own result: meshes and the evidence the walk produced, so no mutable list threads down a
// recursion and no arm mutates a caller's accumulator.
internal readonly record struct SolidComponents(Seq<SolidMesh> Meshes, Seq<SolidDiagnostic> Diagnostics) {
    public static readonly SolidComponents Empty = new(Seq<SolidMesh>(), Seq<SolidDiagnostic>());

    public SolidComponents Concat(SolidComponents other) =>
        new(Meshes + other.Meshes, Diagnostics + other.Diagnostics);
}

public static partial class SolidImport {
    // The acyclicity rail. Bare edges are enough — the census walks each build item's component closure once and
    // the gate answers a typed refusal, so the mesh fold below carries no ancestor set and no cycle throw.
    internal static Fin<Unit> Acyclic(Seq<CObject> roots, SolidPath path) =>
        roots.Bind(Edges).Distinct() is { } edges && edges.IsDirectedAcyclicGraph()
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(Unfit(path, "solid-3mf:component-cycle"));

    private static Seq<SEdge<uint>> Edges(CObject resource) =>
        resource is CComponentsObject assembly
            ? Range(0, checked((int)assembly.GetComponentCount())).Bind(index => {
                using CComponent component = assembly.GetComponent(checked((uint)index));
                using CObject child = component.GetObjectResource();
                return Seq(new SEdge<uint>(resource.GetUniqueResourceID(), child.GetUniqueResourceID()))
                    + Edges(child);
            })
            : Seq<SEdge<uint>>();

    // Each child transform PREPENDS, so the fold applies the innermost placement first and every ancestor
    // composes outward; appending seats a nested component in its parent's unrotated frame.
    internal static Fin<SolidComponents> FromThreeMf(CObject resource, Seq<sTransform> transforms) =>
        resource switch {
            CMeshObject mesh => Fin.Succ(new SolidComponents(
                Seq(Transform(FromThreeMf(mesh), transforms)),
                Seq<SolidDiagnostic>(new SolidDiagnostic.Native(
                    mesh.GetUniqueResourceID(), mesh.IsManifoldAndOriented())))),
            CComponentsObject assembly => Range(0, checked((int)assembly.GetComponentCount()))
                .Traverse(index => {
                    using CComponent component = assembly.GetComponent(checked((uint)index));
                    using CObject child = component.GetObjectResource();
                    return FromThreeMf(
                        child,
                        component.HasTransform() ? Seq(component.GetTransform()) + transforms : transforms)
                        .ToValidation();
                }).As().ToFin()
                .Map(static rows => rows.Fold(SolidComponents.Empty, static (state, row) => state.Concat(row))),
            _ => Fin.Succ(new SolidComponents(
                Seq<SolidMesh>(),
                Seq<SolidDiagnostic>(new SolidDiagnostic.Skipped(resource.GetType().Name)))),
        };

    private static SolidMesh FromThreeMf(CMeshObject mesh) {
        mesh.GetVertices(out sPosition[] vertices);
        mesh.GetTriangleIndices(out sTriangle[] triangles);
        return new SolidMesh(
            vertices.Map(static vertex => new SolidVertex(
                vertex.Coordinates[0], vertex.Coordinates[1], vertex.Coordinates[2])).ToArr(),
            triangles.Bind(static triangle => triangle.Indices.Map(static index => checked((int)index))).ToArr());
    }

    private static SolidMesh Transform(SolidMesh mesh, Seq<sTransform> transforms) => new(
        mesh.Vertices.Map(vertex => transforms.Fold(vertex, static (point, transform) => Apply(transform, point))),
        mesh.TriangleIndices);

    // `sTransform.Fields` is 4x3 — three basis rows then the translation row — so a point multiplies as a
    // row vector and `Fields[3]` is the offset; reading it as 3x4 transposes every nested placement.
    private static SolidVertex Apply(sTransform transform, SolidVertex point) => new(
        point.X * transform.Fields[0][0] + point.Y * transform.Fields[1][0] + point.Z * transform.Fields[2][0] + transform.Fields[3][0],
        point.X * transform.Fields[0][1] + point.Y * transform.Fields[1][1] + point.Z * transform.Fields[2][1] + transform.Fields[3][1],
        point.X * transform.Fields[0][2] + point.Y * transform.Fields[1][2] + point.Z * transform.Fields[2][2] + transform.Fields[3][2]);

    internal static SolidMesh FromThreeDm(R3Mesh mesh) => new(
        mesh.Vertices.ToPoint3dArray().Map(static vertex => new SolidVertex(vertex.X, vertex.Y, vertex.Z)).ToArr(),
        mesh.Faces.ToIntArray(asTriangles: true).ToArr());

    // `state.Vertices` is the PRE-append immutable value, so the index rebase never shifts with tuple
    // evaluation order; a mutable accumulator offsets the second mesh by its own vertex count.
    internal static Option<SolidMesh> Merge(Seq<SolidMesh> meshes) {
        if (meshes.IsEmpty)
            return None;
        (Arr<SolidVertex> Vertices, Arr<int> Indices) merged = meshes.Fold(
            State: (Vertices: Arr<SolidVertex>(), Indices: Arr<int>()),
            Folder: static (state, mesh) => (
                state.Vertices.AddRange(mesh.Vertices),
                state.Indices.AddRange(mesh.TriangleIndices.Map(index => checked(index + state.Vertices.Count)))));
        return Some(new SolidMesh(merged.Vertices, merged.Indices));
    }

    internal static Error Fault(SolidPath path, string detail, Option<string> model = default) =>
        FabricationFault.Unavailable(
            model.Match(
                Some: value => new SourceLocus.ThreeMfObject(Path.GetFileName(value), 0),
                None: () => (SourceLocus)Locus(path)),
            detail);

    internal static Error Fault(int face, string reason) =>
        FabricationFault.Unavailable(new SourceLocus.MeshFace(face), reason);

    // A source the provider read cleanly but whose GEOMETRY cannot be admitted is not a provider outage: it is a
    // structural refusal, and its own case is what lets a caller separate "the reader failed" from "the model is
    // not fabricable".
    internal static Error Unfit(SolidPath path, string axis) =>
        new FabricationFault.IngressGeometryUnfit(Locus(path), axis);

    internal static Error Unfit(int face, string axis) =>
        new FabricationFault.IngressGeometryUnfit(new SourceLocus.MeshFace(face), axis);

    // `SourceLocus.OcctShape` carries an `int` shape id the read never publishes, so the path digest folds
    // into the non-negative int range — a stable per-file locus, never an OCCT-assigned identity.
    private static SourceLocus.OcctShape Locus(SolidPath path) =>
        new(unchecked((int)(ContentHash.Of(Encoding.UTF8.GetBytes(path.Value)) & (UInt128)int.MaxValue)));
}
```

## [03]-[CANONICAL_OWNER]

- Owner: `SolidMesh` is the sole detached millimeter carrier; `SolidTopology` owns incidence, shell, orientation, genus, and bounds measurement; `SolidWeldEvidence` retains the conditioning fold; `SolidRepairEvidence` retains the heal session; `SolidImportReceipt` is the settled evidence carrier every projection reads.
- Law: the per-shell census is ONE pass over the triangles and ONE over the edge map, both after the sweep has settled every union. Reading the shell of each edge once per shell made a mesh of `s` shells and `e` edges pay `s * e` comparisons for a partition each edge already names.
- Entry: `SolidImport.Read(SolidSource)` returns one deferred `Eff<SolidImportReceipt>` folding byte read, format admission, provider read, unit resolution, conditioning, measurement, kernel admission, repair, and closure on one rail — the `Fin` closes back onto `Eff` through `ToEff`, so the entry publishes its declared carrier rather than a nested one.
- Auto: measurement runs edge census and the `ForestDisjointSet<int>` triangle-shell merge in one sweep, then derives boundary, non-manifold, and unused counts, per-shell signed volume against a tolerance-cubed floor, Euler characteristic per shell, and genus only where the conditioned mesh is watertight AND oriented, so an open shell reports no genus rather than a fabricated one; `SolidFacePolicy` treats a collapsed, duplicate, or sliver face as data under `Drop` and names it through `SourceLocus.MeshFace` under `Reject`; `InputMesh` and `InputTopology` bind the conditioned snapshot while `Space` and `Repair.Session.FinalStatus` bind the possibly repaired one, so a repair never overwrites the input evidence a rejection cites; `SolidClosure` reads input topology or final heal status, so a healed source satisfies a watertight demand its unhealed input fails.
- Receipt: `SolidImportReceipt` carries the source digest minted from the file bytes through `ContentHash.Of`, the admitted format, `SolidUnitEvidence`, the conditioned input mesh with its weld evidence, input topology, the kernel `MeshSpace`, optional repair evidence, and the provider evidence with its exact bounds rescaled to millimeters.
- Packages: `MeshSpace` owns kernel admission, `HealPlan` and `Heal` own repair and its session; `QuikGraph.Collections.ForestDisjointSet<int>` owns the triangle-to-shell partition and publishes its live `SetCount`; `UnitsNet` carries volume, length, and area evidence; `LanguageExt.Core` owns the rails and immutable carriers.
- Growth: a new measured property is one `SolidTopology` field derived inside the one sweep; a new conditioning rule is one reason arm in the weld fold beside its `SolidDiagnostic.Degenerate` row; a new closure demand is one `SolidClosure` case with one predicate arm.
- Exemption: vertex coalescing, face admission, and the edge-census shell-partition sweep are the bounded statement kernels — each is a single traversal whose incremental index IS the algorithm, and every fold around them is expression-shaped.
- Boundary: `SolidTopology.Measure` refuses typed on a non-finite vertex, a non-triple index count, or an out-of-range index rather than throwing an admission gate; `Native` is the one place a kernel `Mesh` is constructed and no kernel handle travels back out.

```csharp signature
// --- [CANONICAL_OWNER] --------------------------------------------------------------------
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

public sealed record SolidMesh(Arr<SolidVertex> Vertices, Arr<int> TriangleIndices) {
    public int TriangleCount => TriangleIndices.Count / 3;

    public SolidMesh Scale(double factor) => new(Vertices.Map(vertex => vertex.Scale(factor)), TriangleIndices);
}

public sealed record SolidWeldEvidence(SolidWeld Policy, int Before, int After, int DroppedFaces);

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

public sealed record SolidUnitEvidence(
    Option<LengthUnit> Declared,
    SolidUnitPolicy Resolution,
    LengthUnit Canonical,
    double MillimeterScale);

public sealed record SolidRepairEvidence(SolidRepairPolicy Policy, HealSession Session);

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

public static partial class SolidImport {
    public static Eff<SolidImportReceipt> Read(SolidSource source) => Eff.lift(() =>
        from raw in Try.lift(() => File.ReadAllBytes(source.Path.Value)).Run()
            .MapFail(error => Fault(source.Path, error.Message))
        from format in SolidFormat.Admit(source.Path)
        from detached in format.Read(source, raw)
        from scale in Scale(detached.DeclaredUnit, source.Policy.Units, source.Path)
        from welded in Weld(detached.Mesh.Scale(scale), source.Policy, source.Path)
        from topology in SolidTopology.Measure(welded.Mesh, source.Policy.Context, source.Path)
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
        .Bind(static result => result.ToEff())
        .MapFail(error => error.IsExceptional ? Fault(source.Path, error.Message) : error);

    // Vertex coalescing and face admission run before measurement: an unwelded STL reports every edge as boundary,
    // so a closure demand and the repair stage it gates are both unreachable without this conditioning. Under
    // `SolidWeld.None` the remap is the identity and the source's own vertex census survives whole.
    private static Fin<(SolidMesh Mesh, SolidWeldEvidence Evidence, Seq<SolidDiagnostic> Diagnostics)> Weld(
        SolidMesh mesh, SolidPolicy policy, SolidPath path) {
        if (mesh.TriangleIndices.Count % 3 != 0)
            return Fin.Fail<(SolidMesh, SolidWeldEvidence, Seq<SolidDiagnostic>)>(
                Unfit(path, "solid-mesh:triangle-arity"));

        Option<double> grid = policy.Weld.GridMm;
        Dictionary<SolidVertex, int> coalesced = [];
        int[] remap = new int[mesh.Vertices.Count];
        List<SolidVertex> vertices = [];
        for (int index = 0; index < mesh.Vertices.Count; index++) {
            SolidVertex source = mesh.Vertices[index];
            SolidVertex key = grid.Map(source.Snap).IfNone(source);
            if (grid.IsNone) {
                remap[index] = vertices.Count;
                vertices.Add(source);
                continue;
            }
            if (!coalesced.TryGetValue(key, out int mapped)) {
                mapped = vertices.Count;
                coalesced[key] = mapped;
                vertices.Add(key);
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
                return Fin.Fail<(SolidMesh, SolidWeldEvidence, Seq<SolidDiagnostic>)>(Unfit(triangle, reason));
            dropped.Add(new SolidDiagnostic.Degenerate(triangle, reason));
        }

        return Fin.Succ<(SolidMesh, SolidWeldEvidence, Seq<SolidDiagnostic>)>((
            new SolidMesh(vertices.ToArr(), indices.ToArr()),
            new SolidWeldEvidence(policy.Weld, mesh.Vertices.Count, vertices.Count, dropped.Count),
            toSeq(dropped)));
    }

    private static (int A, int B, int C) Sorted(int a, int b, int c) {
        if (a > b) (a, b) = (b, a);
        if (b > c) (b, c) = (c, b);
        if (a > b) (a, b) = (b, a);
        return (a, b, c);
    }

    private static Fin<double> Scale(Option<LengthUnit> declared, SolidUnitPolicy policy, SolidPath path) =>
        policy.Switch(
            state: declared,
            declared: static (unit, _) => unit.Map(SolidUnits.Millimeters),
            assume: static (unit, assumed) => Some(SolidUnits.Millimeters(unit.IfNone(assumed.Unit))),
            @override: static (_, forced) => Some(SolidUnits.Millimeters(forced.Unit)))
        .ToFin(IngressFault.Policy("solid-unit:missing-declaration"));

    private static Fin<Unit> Closure(
        SolidTopology topology,
        SolidClosure closure,
        Option<SolidRepairEvidence> repair,
        SolidPath path) => closure.Switch(
        state: (Topology: topology, Repair: repair, Path: path),
        surface: static (_, _) => Fin.Succ(unit),
        manifold: static (value, _) => value.Topology.NonManifoldEdges == 0 || Healed(value.Repair)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(Unfit(value.Path, "solid-closure:non-manifold")),
        watertight: static (value, _) => (value.Topology.Watertight && value.Topology.Oriented) || Healed(value.Repair)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(Unfit(value.Path, "solid-closure:open")));

    // Running a session is not closing one: the final status itself reports manifold, oriented, and zero
    // boundary and non-manifold edges, or partial heal progress reads as a satisfied closure demand.
    private static bool Healed(Option<SolidRepairEvidence> repair) => repair.Exists(
        static evidence => evidence.Session.IsValid
            && evidence.Session.FinalStatus.Exists(static status => status.IsManifold && status.IsOriented
                && status.BoundaryComponents == 0 && status.NonManifoldEdges == 0));

    private static Fin<(MeshSpace Space, Option<SolidRepairEvidence> Repair)> Repair(
        MeshSpace space, SolidTopology topology, SolidPolicy policy) => policy.Repair.Applies(topology)
            ? HealPlan.Of(space, key: policy.Key)
                .Bind(plan => Heal.Repair(plan, policy.Key))
                .Map(session => (session.Healed, Some(new SolidRepairEvidence(policy.Repair, session))))
            : Fin.Succ((space, Option<SolidRepairEvidence>.None));

    private static Mesh Native(SolidMesh mesh) {
        Mesh native = new();
        mesh.Vertices.Iter(vertex => native.Vertices.Add(vertex.X, vertex.Y, vertex.Z));
        toSeq(Range(0, mesh.TriangleCount)).Iter(triangle => native.Faces.AddFace(
            mesh.TriangleIndices[triangle * 3],
            mesh.TriangleIndices[triangle * 3 + 1],
            mesh.TriangleIndices[triangle * 3 + 2]));
        return native;
    }
}

public sealed partial record SolidTopology {
    private readonly record struct Edge(int A, int B) {
        public static Edge Of(int a, int b) => a < b ? new(a, b) : new(b, a);
    }

    private readonly record struct EdgeUse(int Forward, int Reverse, int Triangle) {
        public int Count => Forward + Reverse;
    }

    // The per-shell census: vertices, edges, faces, boundary edges, and signed volume for one root.
    private readonly record struct Shell(Set<int> Vertices, int Edges, int Faces, int Boundaries, double Volume) {
        public static readonly Shell Empty = new(Set<int>(), 0, 0, 0, 0d);

        public int Euler => Vertices.Count - Edges + Faces;
    }

    // One sweep builds the edge incidence map AND the triangle shell partition: a second pass over the faces would
    // re-derive the shell partition the edge merge already produced. The partition is QuikGraph's
    // `ForestDisjointSet<int>` — union by rank with path compression, and `SetCount` is the live shell census. The
    // per-shell columns then fold in ONE pass over the triangles and ONE over the edge map, both after every union
    // has settled, so no root is resolved more than once per row.
    public static Fin<SolidTopology> Measure(SolidMesh mesh, Context context, SolidPath path) {
        if (mesh.Vertices.IsEmpty || mesh.TriangleIndices.IsEmpty || mesh.TriangleIndices.Count % 3 != 0
            || mesh.Vertices.Exists(static vertex =>
                !double.IsFinite(vertex.X) || !double.IsFinite(vertex.Y) || !double.IsFinite(vertex.Z)))
            return Fin.Fail<SolidTopology>(SolidImport.Unfit(path, "solid-topology:structural"));
        if (Range(0, mesh.TriangleIndices.Count).Exists(slot =>
                mesh.TriangleIndices[slot] < 0 || mesh.TriangleIndices[slot] >= mesh.Vertices.Count))
            return Fin.Fail<SolidTopology>(SolidImport.Unfit(path, "solid-topology:index"));

        ForestDisjointSet<int> shells = new(mesh.TriangleCount);
        for (int triangle = 0; triangle < mesh.TriangleCount; triangle++) shells.MakeSet(triangle);
        Dictionary<Edge, EdgeUse> edges = [];
        for (int triangle = 0; triangle < mesh.TriangleCount; triangle++) {
            int a = mesh.TriangleIndices[triangle * 3];
            int b = mesh.TriangleIndices[triangle * 3 + 1];
            int c = mesh.TriangleIndices[triangle * 3 + 2];
            Add(edges, shells, triangle, a, b);
            Add(edges, shells, triangle, b, c);
            Add(edges, shells, triangle, c, a);
        }

        Map<int, Shell> faced = toSeq(Range(0, mesh.TriangleCount))
            .Fold(Map<int, Shell>(), (state, triangle) => Faced(state, shells.FindSet(triangle), mesh, triangle));
        Map<int, Shell> census = toSeq(edges)
            .Fold(faced, (state, row) => Edged(state, shells.FindSet(row.Value.Triangle), row.Value));

        Seq<Shell> rows = census.Values.ToSeq();
        if (rows.Exists(static shell => !double.IsFinite(shell.Volume)))
            return Fin.Fail<SolidTopology>(SolidImport.Unfit(path, "solid-topology:non-finite-volume"));

        // Volume is signed and cubic, so the zero test rides a tolerance-cubed floor: an absolute epsilon
        // reads a millimetre-scale sliver shell and a metre-scale solid on the same scale.
        double volumeFloor = Math.Pow(context.Absolute.Value, 3d);
        int boundary = edges.Values.Count(static use => use.Count == 1);
        int nonManifold = edges.Values.Count(static use => use.Count > 2);
        int referenced = toSeq(mesh.TriangleIndices).Distinct().Count;
        int inward = rows.Count(shell => shell.Boundaries == 0 && shell.Volume < -volumeFloor);
        int zeroVolume = rows.Count(shell => shell.Boundaries == 0 && Math.Abs(shell.Volume) <= volumeFloor);
        bool oriented = inward == 0 && zeroVolume == 0
            && edges.Values.ForAll(static use => use.Count < 2 || use is { Forward: 1, Reverse: 1 });
        bool watertight = boundary == 0 && nonManifold == 0 && zeroVolume == 0;
        int euler = rows.Fold(0, static (sum, shell) => sum + shell.Euler);
        Seq<SolidVertex> points = toSeq(mesh.Vertices);
        // The head binds on the rail rather than through a collection deref: `Head` answers `Option`, and the
        // structural guard above already excluded the one state that has no head.
        return points.Head
            .ToFin(SolidImport.Unfit(path, "solid-topology:structural"))
            .Map(head => new SolidTopology(
                mesh.Vertices.Count, mesh.TriangleCount, edges.Count, boundary, nonManifold,
                mesh.Vertices.Count - referenced,
                shells.SetCount, inward, zeroVolume, euler,
                watertight && oriented ? Some((2 * shells.SetCount - euler) / 2) : None,
                Volume.FromCubicMillimeters(rows.Fold(0d, static (sum, shell) => sum + shell.Volume)),
                oriented, watertight, Bounds(head, points.Tail)));
    }

    private static Map<int, Shell> Faced(Map<int, Shell> census, int root, SolidMesh mesh, int triangle) {
        int a = mesh.TriangleIndices[triangle * 3];
        int b = mesh.TriangleIndices[triangle * 3 + 1];
        int c = mesh.TriangleIndices[triangle * 3 + 2];
        double volume = Dot(mesh.Vertices[a], Cross(mesh.Vertices[b], mesh.Vertices[c])) / 6d;
        Shell held = census.Find(root).IfNone(Shell.Empty);
        return census.AddOrUpdate(root, held with {
            Vertices = held.Vertices.Add(a).Add(b).Add(c),
            Faces = held.Faces + 1,
            Volume = held.Volume + volume,
        });
    }

    private static Map<int, Shell> Edged(Map<int, Shell> census, int root, EdgeUse use) {
        Shell held = census.Find(root).IfNone(Shell.Empty);
        return census.AddOrUpdate(root, held with {
            Edges = held.Edges + 1,
            Boundaries = held.Boundaries + (use.Count == 1 ? 1 : 0),
        });
    }

    internal static double Area(SolidVertex a, SolidVertex b, SolidVertex c) =>
        0.5d * Length(Cross(Subtract(b, a), Subtract(c, a)));

    private static SolidVertex Subtract(SolidVertex left, SolidVertex right) =>
        new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    private static SolidVertex Cross(SolidVertex left, SolidVertex right) => new(
        left.Y * right.Z - left.Z * right.Y,
        left.Z * right.X - left.X * right.Z,
        left.X * right.Y - left.Y * right.X);

    private static double Dot(SolidVertex left, SolidVertex right) =>
        left.X * right.X + left.Y * right.Y + left.Z * right.Z;

    private static double Length(SolidVertex value) => Math.Sqrt(Dot(value, value));

    private static SolidBounds Bounds(SolidVertex head, Seq<SolidVertex> tail) => tail.Fold(
        State: new SolidBounds(head, head),
        Folder: static (bounds, vertex) => new(
            new SolidVertex(Math.Min(bounds.Minimum.X, vertex.X), Math.Min(bounds.Minimum.Y, vertex.Y), Math.Min(bounds.Minimum.Z, vertex.Z)),
            new SolidVertex(Math.Max(bounds.Maximum.X, vertex.X), Math.Max(bounds.Maximum.Y, vertex.Y), Math.Max(bounds.Maximum.Z, vertex.Z))));

    // Direction is per-USE, not per-edge: a manifold interior edge is traversed once forward and once reverse,
    // so the two counters are what distinguish a consistently oriented pair from a mirrored one.
    private static void Add(Dictionary<Edge, EdgeUse> edges, ForestDisjointSet<int> shells, int triangle, int from, int to) {
        Edge edge = Edge.Of(from, to);
        if (edges.TryGetValue(edge, out EdgeUse use)) {
            shells.Union(triangle, use.Triangle);
            edges[edge] = from < to
                ? use with { Forward = use.Forward + 1 }
                : use with { Reverse = use.Reverse + 1 };
        }
        else {
            edges[edge] = from < to ? new EdgeUse(1, 0, triangle) : new EdgeUse(0, 1, triangle);
        }
    }
}
```

## [04]-[PROJECTION_EGRESS]

- Owner: `SolidProjection` is the closed egress row carrying its own view delegate, and `SolidView` carries each row's result shape.
- Cases: space · input-mesh · input-topology · bounds · units · diagnostics · repair · receipt.
- Entry: `SolidProjection.<row>.Project(SolidImportReceipt)` — the row IS the dispatch, so the eight payload-free request cases and the eight-arm `Switch` that read them both die.
- Auto: the bounds view reads the provider's exact rescaled bounding envelope where one exists and the measured input bounds otherwise, so stock sized from a tessellation-derived bounding envelope never reads tighter than the solid it was sampled from.
- Growth: a new egress is one `SolidProjection` row carrying its delegate and one `SolidView` case.
- Boundary: `SolidRepairEvidence.Session.FinalStatus` carries repaired topology status beside the explicit input snapshot, so both states read off one receipt; exact CAD and 3MF round-trip belongs to a representation-preserving owner, never this triangulating ingress.

```csharp signature
// --- [PROJECTION_EGRESS] ------------------------------------------------------------------
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

[SmartEnum<string>]
public sealed partial class SolidProjection {
    public static readonly SolidProjection Space = new("space",
        static receipt => new SolidView.Space(receipt.Space));
    public static readonly SolidProjection InputMesh = new("input-mesh",
        static receipt => new SolidView.InputMesh(receipt.InputMesh));
    public static readonly SolidProjection InputTopology = new("input-topology",
        static receipt => new SolidView.InputTopology(receipt.InputTopology));
    public static readonly SolidProjection Bounds = new("bounds",
        static receipt => new SolidView.Bounds(receipt.Provider.Exact.IfNone(receipt.InputTopology.Bounds)));
    public static readonly SolidProjection Units = new("units",
        static receipt => new SolidView.Units(receipt.Units));
    public static readonly SolidProjection Diagnostics = new("diagnostics",
        static receipt => new SolidView.Diagnostics(receipt.Provider.Diagnostics));
    public static readonly SolidProjection Repair = new("repair",
        static receipt => new SolidView.Repair(receipt.Repair));
    public static readonly SolidProjection Receipt = new("receipt",
        static receipt => new SolidView.Receipt(receipt));

    [UseDelegateFromConstructor]
    public partial SolidView Project(SolidImportReceipt receipt);
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
