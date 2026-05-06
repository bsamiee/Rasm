using System.Globalization;
using System.Runtime.InteropServices;
using LanguageExt.Common;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace Core.Domain;

// --- [SERVICES] --------------------------------------------------------------------------------

internal static class GeometryValidation {
    internal static Validation<Error, TGeometry> Validate<TGeometry>(
        this GeometryContext context,
        TGeometry? geometry) where TGeometry : GeometryBase =>
        (Validation<Error, TGeometry>)(
            Optional(context).ToValidation(ValidationFault.MissingContext()),
            Optional(geometry).ToValidation(ValidationFault.MissingGeometry())
        ).Apply(static (
                GeometryContext validContext,
                TGeometry candidate) => (
                Context: validContext,
                Geometry: candidate))
        .Bind(static ((GeometryContext Context, TGeometry Geometry) state) =>
            GeometryCheck.For().Aggregate(
                seed: (
                    state.Context,
                    state.Geometry,
                    Result: Fin.Succ(state.Geometry).ToValidation()),
                func: static (
                    (GeometryContext Context, TGeometry Geometry, Validation<Error, TGeometry> Result) current,
                    GeometryCheck check) => (
                    current.Context,
                    current.Geometry,
                    (current.Result, check.Apply(
                            context: current.Context,
                            geometry: current.Geometry).ToValidation())
                        .Apply(static (TGeometry candidate, Unit _) => candidate)
                        .As())).Result);
}

// --- [MODELS] ----------------------------------------------------------------------------------

internal sealed class GeometryCheck {
    internal static readonly GeometryCheck RhinoValidity = new(
        key: "rhino-validity",
        label: nameof(RhinoValidity),
        applies: static (GeometryBase _) => true,
        validate: static (CheckState state) =>
            state.RequireRhinoLog(geometry: state.Geometry));

    internal static readonly GeometryCheck UsableBounds = new(
        key: "usable-bounds",
        label: nameof(UsableBounds),
        applies: static (GeometryBase _) => true,
        validate: static (CheckState state) =>
            state.Geometry.GetBoundingBox(accurate: true) switch {
                BoundingBox box => state.Require(
                    condition: box.IsValid && box.IsDegenerate(tolerance: state.Context.Absolute.Value) < 4,
                    log: "Rhino could not compute a usable accurate bounding box."),
            });

    internal static readonly GeometryCheck AreaReadiness = new(
        key: "area-readiness",
        label: nameof(AreaReadiness),
        applies: static (GeometryBase geometry) => geometry is Curve or Mesh or Brep or Surface,
        validate: static (CheckState state) => {
            using AreaMassProperties? properties = state.Geometry switch {
                Curve curve => AreaMassProperties.Compute(
                    closedPlanarCurve: curve,
                    planarTolerance: state.Context.Absolute.Value),
                Mesh mesh => AreaMassProperties.Compute(mesh: mesh),
                Brep brep => AreaMassProperties.Compute(
                    brep: brep,
                    area: true,
                    firstMoments: false,
                    secondMoments: false,
                    productMoments: false,
                    relativeTolerance: state.Context.Relative.Value,
                    absoluteTolerance: state.Context.Absolute.Value),
                Surface surface => AreaMassProperties.Compute(surface: surface),
                _ => default,
            };
            return state.RequireComputed(
                candidate: properties,
                log: "Rhino AreaMassProperties computation failed.");
        });

    internal static readonly GeometryCheck VolumeReadiness = new(
        key: "volume-readiness",
        label: nameof(VolumeReadiness),
        applies: static (GeometryBase geometry) => geometry is Brep,
        validate: static (CheckState state) => {
            using VolumeMassProperties? properties = state.Geometry switch {
                Brep brep => VolumeMassProperties.Compute(
                    brep: brep,
                    volume: true,
                    firstMoments: false,
                    secondMoments: false,
                    productMoments: false,
                    relativeTolerance: state.Context.Relative.Value,
                    absoluteTolerance: state.Context.Absolute.Value),
                _ => default,
            };
            return state.RequireComputed(
                candidate: properties,
                log: "Rhino VolumeMassProperties computation failed.");
        });

    internal static readonly GeometryCheck BrepIntegrity = new(
        key: "brep-integrity",
        label: nameof(BrepIntegrity),
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
        label: nameof(MeshRhinoCheck),
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
        label: nameof(MeshManifoldReadiness),
        applies: static (GeometryBase geometry) => geometry is Mesh,
        validate: static (CheckState state) =>
            state.Require(
                condition: ((Mesh)state.Geometry).IsManifold(),
                log: "Mesh is valid Rhino geometry but is not manifold enough for Rasm topology operations."));

    internal static readonly GeometryCheck CurveLengthReadiness = new(
        key: "curve-length-readiness",
        label: nameof(CurveLengthReadiness),
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
        label: nameof(SurfaceDomainReadiness),
        applies: static (GeometryBase geometry) => geometry is Surface,
        validate: static (CheckState state) =>
            state.Require(
                condition: SurfaceProfile.From(surface: (Surface)state.Geometry)
                    .HasUsableDomain(context: state.Context),
                log: "Surface is valid Rhino geometry but has an unusable Rasm UV domain."));

    internal static readonly GeometryCheck ContinuityReadiness = new(
        key: "continuity-readiness",
        label: nameof(ContinuityReadiness),
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
        label: nameof(PolycurveStructure),
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

    internal static readonly GeometryCheck NurbsRhinoValidity = new(
        key: "nurbs-rhino-validity",
        label: nameof(NurbsRhinoValidity),
        applies: static (GeometryBase geometry) => geometry is NurbsCurve or NurbsSurface,
        validate: static (CheckState state) =>
            state.RequireRhinoLog(geometry: state.Geometry));

    internal static readonly GeometryCheck ExtrusionRhinoValidity = new(
        key: "extrusion-rhino-validity",
        label: nameof(ExtrusionRhinoValidity),
        applies: static (GeometryBase geometry) => geometry is Extrusion,
        validate: static (CheckState state) =>
            state.RequireRhinoLog(geometry: state.Geometry));

    internal static readonly GeometryCheck CurveSelfIntersection = new(
        key: "curve-self-intersection",
        label: nameof(CurveSelfIntersection),
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
                _ => Fin.Succ(unit),
            };
        });

    private static readonly Seq<GeometryCheck> All = Seq(
        RhinoValidity,
        UsableBounds,
        AreaReadiness,
        VolumeReadiness,
        BrepIntegrity,
        MeshRhinoCheck,
        MeshManifoldReadiness,
        CurveLengthReadiness,
        SurfaceDomainReadiness,
        ContinuityReadiness,
        PolycurveStructure,
        NurbsRhinoValidity,
        ExtrusionRhinoValidity,
        CurveSelfIntersection);

    private readonly Func<GeometryBase, bool> applies;
    private readonly Func<CheckState, Fin<Unit>> validate;

    private GeometryCheck(
        string key,
        string label,
        Func<GeometryBase, bool> applies,
        Func<CheckState, Fin<Unit>> validate) {
        Key = key;
        Label = label;
        this.applies = applies;
        this.validate = validate;
    }

    internal string Key { get; }
    internal string Label { get; }

    internal static Seq<GeometryCheck> For() =>
        All;

    internal Fin<Unit> Apply(GeometryContext context, GeometryBase geometry) =>
        applies(arg: geometry) switch {
            true => validate(arg: new CheckState(
                Context: context,
                Check: this,
                Geometry: geometry)),
            false => Fin.Succ(unit),
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

        internal Fin<Unit> RequireComputed<TDisposable>(TDisposable? candidate, string log)
            where TDisposable : class, IDisposable =>
            candidate switch {
                TDisposable => Fin.Succ(unit),
                _ => Invalid(geometry: Geometry, log: log),
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

    internal static Error MissingContext() =>
        Error.New(message: "Geometry validation requires a model context.");

    internal static Error InvalidGeometry(GeometryBase geometry, GeometryCheck check, string log) =>
        Error.New(message: string.IsNullOrWhiteSpace(value: log) switch {
            true => string.Create(
                provider: CultureInfo.InvariantCulture,
                $"Geometry validation failed for {geometry.GetType().Name} under check '{check.Label}' ({check.Key})."),
            false => string.Create(
                provider: CultureInfo.InvariantCulture,
                $"Geometry validation failed for {geometry.GetType().Name} under check '{check.Label}' ({check.Key}): {log}"),
        });
}
