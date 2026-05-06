using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using LanguageExt.Common;
using Rhino;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using static LanguageExt.Prelude;

namespace Core.Domain;

// --- [SERVICES] --------------------------------------------------------------------------------

internal static class GeometryValidation {
    internal static Validation<Error, TGeometry> Validate<TGeometry>(
        this GeometryContext context,
        TGeometry? geometry,
        GeometryRequirement requirement) where TGeometry : GeometryBase =>
        (Validation<Error, TGeometry>)(
            Fin.Succ((Context: context, Requirement: requirement)).ToValidation(),
            Optional(geometry).ToValidation(ValidationFault.MissingGeometry())
        ).Apply(static ((GeometryContext Context, GeometryRequirement Requirement) state, TGeometry candidate) => (
            state.Context,
            state.Requirement,
            Geometry: candidate))
        .Bind(static ((GeometryContext Context, GeometryRequirement Requirement, TGeometry Geometry) state) =>
            GeometryCheck.All.Aggregate(
                seed: (
                    state.Context,
                    state.Requirement,
                    state.Geometry,
                    Result: Fin.Succ(state.Geometry).ToValidation()),
                func: static (
                    (GeometryContext Context, GeometryRequirement Requirement, TGeometry Geometry, Validation<Error, TGeometry> Result) current,
                    GeometryCheck check) => (
                    current.Context,
                    current.Requirement,
                    current.Geometry,
                    (current.Result, check.Apply(
                            context: current.Context,
                            requirement: current.Requirement,
                            geometry: current.Geometry).ToValidation())
                        .Apply(static (TGeometry candidate, Unit _) => candidate)
                        .As())).Result);
}

// --- [MODELS] ----------------------------------------------------------------------------------

public readonly record struct GeometryRequirement {
    private const ushort NoneMask = 0;
    private const ushort RhinoMask = 1 << 0;
    private const ushort BoundsMask = 1 << 1;
    private const ushort CurveLengthMask = 1 << 2;
    private const ushort SolidTopologyMask = 1 << 3;
    private const ushort MeshCheckMask = 1 << 4;
    private const ushort SurfaceDomainMask = 1 << 5;
    private const ushort StrictStructureMask = 1 << 6;
    private const ushort CurveIntersectionMask = 1 << 7;
    private const ushort StrictMask = RhinoMask
        | BoundsMask
        | CurveLengthMask
        | SolidTopologyMask
        | MeshCheckMask
        | SurfaceDomainMask
        | StrictStructureMask
        | CurveIntersectionMask;

    private GeometryRequirement(ushort mask) =>
        Mask = mask;

    private ushort Mask { get; }
    public static GeometryRequirement None => new(mask: NoneMask);
    public static GeometryRequirement Basic => new(mask: RhinoMask | BoundsMask);
    public static GeometryRequirement CurveLength => new(mask: RhinoMask | BoundsMask | CurveLengthMask);
    public static GeometryRequirement AreaMass => new(mask: RhinoMask | BoundsMask | CurveIntersectionMask);
    public static GeometryRequirement VolumeMass => new(mask: RhinoMask | BoundsMask | SolidTopologyMask | MeshCheckMask);
    public static GeometryRequirement SurfaceEvaluation => new(mask: RhinoMask | BoundsMask | SurfaceDomainMask);
    public static GeometryRequirement Strict => new(mask: StrictMask);
    internal static GeometryRequirement MeshCheck => new(mask: RhinoMask | BoundsMask | MeshCheckMask);
    internal static GeometryRequirement SolidTopology => new(mask: RhinoMask | BoundsMask | SolidTopologyMask | MeshCheckMask);
    internal static GeometryRequirement StrictStructure => new(mask: RhinoMask | BoundsMask | SurfaceDomainMask | StrictStructureMask);
    internal bool Includes(GeometryRequirement requirement) =>
        (Mask & requirement.Mask) == requirement.Mask;
}

internal sealed class GeometryCheck {
    internal static readonly GeometryCheck RhinoValidity = new(
        key: "rhino-validity",
        requirement: GeometryRequirement.Basic,
        applies: static (GeometryBase _) => true,
        validate: static (CheckState state) =>
            state.RequireRhinoLog(geometry: state.Geometry));

    internal static readonly GeometryCheck UsableBounds = new(
        key: "usable-bounds",
        requirement: GeometryRequirement.Basic,
        applies: static (GeometryBase _) => true,
        validate: static (CheckState state) =>
            state.Geometry.GetBoundingBox(accurate: true) switch {
                BoundingBox box => state.Require(
                    condition: box.IsValid && box.IsDegenerate(tolerance: state.Context.Absolute.Value) < 4,
                    log: "Rhino could not compute a usable accurate bounding box."),
            });

    internal static readonly GeometryCheck BrepIntegrity = new(
        key: "brep-integrity",
        requirement: GeometryRequirement.SolidTopology,
        applies: static (GeometryBase geometry) => geometry is Brep,
        validate: static (CheckState state) => {
            Brep brep = (Brep)state.Geometry;
            return brep.IsValidTopology(log: out string topologyLog) switch {
                false => state.Invalid(
                    geometry: brep,
                    log: string.IsNullOrWhiteSpace(value: topologyLog) switch {
                        true => "Brep topology",
                        false => string.Create(
                            provider: CultureInfo.InvariantCulture,
                            $"Brep topology: {topologyLog}"),
                    }),
                true => brep.IsValidGeometry(log: out string geometryLog) switch {
                    false => state.Invalid(
                        geometry: brep,
                        log: string.IsNullOrWhiteSpace(value: geometryLog) switch {
                            true => "Brep geometry",
                            false => string.Create(
                                provider: CultureInfo.InvariantCulture,
                                $"Brep geometry: {geometryLog}"),
                        }),
                    true => brep.IsValidTolerancesAndFlags(log: out string toleranceLog) switch {
                        false => state.Invalid(
                            geometry: brep,
                            log: string.IsNullOrWhiteSpace(value: toleranceLog) switch {
                                true => "Brep tolerances and flags",
                                false => string.Create(
                                    provider: CultureInfo.InvariantCulture,
                                    $"Brep tolerances and flags: {toleranceLog}"),
                            }),
                        true => Fin.Succ(unit),
                    },
                },
            };
        });

    internal static readonly GeometryCheck MeshRhinoCheck = new(
        key: "mesh-rhino-check",
        requirement: GeometryRequirement.MeshCheck,
        applies: static (GeometryBase geometry) => geometry is Mesh,
        validate: static (CheckState state) => {
            Mesh mesh = (Mesh)state.Geometry;
            using TextLog textLog = new();
            MeshCheckParameters parameters = MeshCheckParameters.Defaults();
            return (
                mesh.Check(
                    textLog: textLog,
                    parameters: ref parameters),
                mesh.Faces.Count > 0) switch {
                    (true, true) => Fin.Succ(unit),
                    _ => state.Invalid(
                        geometry: mesh,
                        log: textLog.ToString()),
                };
        });

    internal static readonly GeometryCheck MeshManifoldReadiness = new(
        key: "mesh-manifold-readiness",
        requirement: GeometryRequirement.SolidTopology,
        applies: static (GeometryBase geometry) => geometry is Mesh,
        validate: static (CheckState state) =>
            state.Require(
                condition: ((Mesh)state.Geometry).IsSolid,
                log: "Mesh is valid Rhino geometry but is not closed and solid enough for Rasm volume operations."));

    internal static readonly GeometryCheck BrepSolidReadiness = new(
        key: "brep-solid-readiness",
        requirement: GeometryRequirement.SolidTopology,
        applies: static (GeometryBase geometry) => geometry is Brep,
        validate: static (CheckState state) =>
            state.Require(
                condition: ((Brep)state.Geometry).IsSolid,
                log: "Brep is valid Rhino geometry but is not solid enough for Rasm volume operations."));

    internal static readonly GeometryCheck CurveLengthReadiness = new(
        key: "curve-length-readiness",
        requirement: GeometryRequirement.CurveLength,
        applies: static (GeometryBase geometry) => geometry is Curve,
        validate: static (CheckState state) => {
            Curve curve = (Curve)state.Geometry;
            return (
                curve.IsShort(tolerance: state.Context.Absolute.Value),
                curve.GetLength(fractionalTolerance: state.Context.Relative.Value) > state.Context.Absolute.Value
            ) switch {
                (false, true) => Fin.Succ(unit),
                _ => state.Invalid(
                    geometry: curve,
                    log: "Curve is valid Rhino geometry but is below Rasm model-length tolerance."),
            };
        });

    internal static readonly GeometryCheck SurfaceDomainReadiness = new(
        key: "surface-domain-readiness",
        requirement: GeometryRequirement.SurfaceEvaluation,
        applies: static (GeometryBase geometry) => geometry is Surface,
        validate: static (CheckState state) =>
            state.Require(
                condition: SurfaceProfile.From(surface: (Surface)state.Geometry)
                    .HasUsableDomain(context: state.Context),
                log: "Surface is valid Rhino geometry but has an unusable Rasm UV domain."));

    internal static readonly GeometryCheck ContinuityReadiness = new(
        key: "continuity-readiness",
        requirement: GeometryRequirement.StrictStructure,
        applies: static (GeometryBase geometry) => geometry is Curve or Surface,
        validate: static (CheckState state) =>
            state.Geometry switch {
                Surface surface => state.Require(
                    condition: SurfaceProfile.From(surface: surface)
                        .HasUsableContinuity(
                            surface: surface,
                            context: state.Context),
                    log: "Surface is valid Rhino geometry but contains a C1 discontinuity."),
                Curve curve => state.Require(
                    condition: curve.GetNextDiscontinuity(
                        continuityType: Continuity.C1_continuous,
                        t0: curve.Domain.T0,
                        t1: curve.Domain.T1,
                        t: out double _) switch {
                            false => true,
                            true => false,
                        },
                    log: "Curve is valid Rhino geometry but contains a C1 discontinuity."),
                _ => Fin.Succ(unit),
            });

    internal static readonly GeometryCheck PolycurveStructure = new(
        key: "polycurve-structure",
        requirement: GeometryRequirement.StrictStructure,
        applies: static (GeometryBase geometry) => geometry is PolyCurve,
        validate: static (CheckState state) => {
            PolyCurve polyCurve = (PolyCurve)state.Geometry;
            return (polyCurve.HasGap, polyCurve.IsNested) switch {
                (false, false) => Fin.Succ(unit),
                _ => state.Invalid(
                    geometry: polyCurve,
                    log: (polyCurve.HasGap, polyCurve.IsNested) switch {
                        (true, true) => "PolyCurve has gaps between segments and nested polycurves.",
                        (true, false) => "PolyCurve has gaps between segments.",
                        (false, true) => "PolyCurve contains nested polycurves.",
                        _ => "PolyCurve structure is usable.",
                    }),
            };
        });

    internal static readonly GeometryCheck CurveSelfIntersection = new(
        key: "curve-self-intersection",
        requirement: GeometryRequirement.AreaMass,
        applies: static (GeometryBase geometry) => geometry is Curve,
        validate: static (CheckState state) => {
            using CurveIntersections? intersections = Intersection.CurveSelf(
                curve: (Curve)state.Geometry,
                tolerance: state.Context.Absolute.Value);
            return intersections switch {
                CurveIntersections hits => hits.Count switch {
                    0 => Fin.Succ(unit),
                    _ => state.Invalid(geometry: state.Geometry, log: string.Create(
                        provider: CultureInfo.InvariantCulture,
                        $"Rhino found {hits.Count} curve self-intersection event(s).")),
                },
                _ => state.Invalid(
                    geometry: state.Geometry,
                    log: "Rhino curve self-intersection computation failed."),
            };
        });

    internal static readonly Seq<GeometryCheck> All = Seq(
        RhinoValidity,
        UsableBounds,
        BrepIntegrity,
        MeshRhinoCheck,
        MeshManifoldReadiness,
        BrepSolidReadiness,
        CurveLengthReadiness,
        SurfaceDomainReadiness,
        ContinuityReadiness,
        PolycurveStructure,
        CurveSelfIntersection);

    private readonly GeometryRequirement requirement;
    private readonly Func<GeometryBase, bool> applies;
    private readonly Func<CheckState, Fin<Unit>> validate;

    private GeometryCheck(
        string key,
        GeometryRequirement requirement,
        Func<GeometryBase, bool> applies,
        Func<CheckState, Fin<Unit>> validate) {
        Key = key;
        this.requirement = requirement;
        this.applies = applies;
        this.validate = validate;
    }

    internal string Key { get; }

    internal Fin<Unit> Apply(GeometryContext context, GeometryRequirement requirement, GeometryBase geometry) =>
        (requirement.Includes(requirement: this.requirement), applies(arg: geometry)) switch {
            (true, true) => validate(arg: new CheckState(
                Context: context,
                Check: this,
                Geometry: geometry)),
            (true, false) or (false, _) => Fin.Succ(unit),
        };

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct CheckState(
        GeometryContext Context,
        GeometryCheck Check,
        GeometryBase Geometry) {
        internal Fin<Unit> Invalid(GeometryBase geometry, string log) =>
            Fin.Fail<Unit>(ValidationFault.InvalidGeometry(
                geometry: geometry,
                check: Check,
                log: log));

        internal Fin<Unit> Require(bool condition, string log) =>
            condition switch {
                true => Fin.Succ(unit),
                false => Invalid(geometry: Geometry, log: log),
            };

        internal Fin<Unit> RequireRhinoLog(GeometryBase geometry) {
            bool isValid = geometry.IsValidWithLog(out string log);
            return isValid switch {
                true => Fin.Succ(unit),
                false => Invalid(geometry: geometry, log: log),
            };
        }
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct SurfaceProfile(Interval UDomain, Interval VDomain) {
        internal static SurfaceProfile From(Surface surface) =>
            new(
                UDomain: surface.Domain(direction: 0),
                VDomain: surface.Domain(direction: 1));

        internal bool HasUsableDomain(GeometryContext context) =>
            (
                UDomain.IsValid,
                VDomain.IsValid,
                UDomain.Length > context.Absolute.Value,
                VDomain.Length > context.Absolute.Value
            ) switch {
                (true, true, true, true) => true,
                _ => false,
            };

        internal bool HasUsableContinuity(Surface surface, GeometryContext context) =>
            HasUsableDomain(context: context) switch {
                false => true,
                true => (
                    surface.GetNextDiscontinuity(
                        direction: 0,
                        continuityType: Continuity.C1_continuous,
                        t0: UDomain.T0,
                        t1: UDomain.T1,
                        t: out double _),
                    surface.GetNextDiscontinuity(
                        direction: 1,
                        continuityType: Continuity.C1_continuous,
                        t0: VDomain.T0,
                        t1: VDomain.T1,
                        t: out double _)) switch {
                            (false, false) => true,
                            _ => false,
                        },
            };
    }
}

// --- [ERRORS] ----------------------------------------------------------------------------------

internal static class ValidationFault {
    internal static Error MissingGeometry() =>
        Error.New(message: "Geometry input is required.");

    internal static Error InvalidGeometry(GeometryBase geometry, GeometryCheck check, string log) =>
        Error.New(message: string.IsNullOrWhiteSpace(value: log) switch {
            true => string.Create(
                provider: CultureInfo.InvariantCulture,
                $"Geometry validation failed for {geometry.GetType().Name} under check '{check.Key}'."),
            false => string.Create(
                provider: CultureInfo.InvariantCulture,
                $"Geometry validation failed for {geometry.GetType().Name} under check '{check.Key}': {log}"),
        });
}
