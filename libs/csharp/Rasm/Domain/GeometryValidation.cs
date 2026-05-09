using System.Globalization;
using System.Linq;
using LanguageExt.Common;
using Rhino;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Thinktecture;
using static LanguageExt.Prelude;
namespace Core.Domain;

// --- [MODELS] ----------------------------------------------------------------------------------

[Union]
public abstract partial record GeometryShape<TA, TB> where TA : notnull where TB : notnull {
    public sealed record Pair(TA A, TB B, GeometryRequirement RequirementA, GeometryRequirement RequirementB) : GeometryShape<TA, TB>;
    public sealed record FirstOnly(TA A, TB B, GeometryRequirement Requirement) : GeometryShape<TA, TB>;
}

// --- [OPERATIONS] ------------------------------------------------------------------------------

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
            state.Requirement.Checks.Aggregate(
                seed: (Result: Fin.Succ(state.Geometry).ToValidation(), state.Context, state.Geometry),
                func: static (
                    (Validation<Error, TGeometry> Result, GeometryContext Context, TGeometry Geometry) accumulator,
                    GeometryCheck check) => (
                        Result: (accumulator.Result, check.Apply(
                                context: accumulator.Context,
                                geometry: accumulator.Geometry).ToValidation())
                            .Apply(static (TGeometry candidate, Unit _) => candidate)
                            .As(),
                        accumulator.Context,
                        accumulator.Geometry)).Result);
    internal static Validation<Error, (TA A, TB B)> Validate<TA, TB>(
        this GeometryContext context,
        GeometryShape<TA, TB> shape) where TA : notnull where TB : notnull =>
        shape switch {
            GeometryShape<TA, TB>.Pair pair => (
                    context.ValidateOperand(operand: pair.A, requirement: pair.RequirementA),
                    context.ValidateOperand(operand: pair.B, requirement: pair.RequirementB))
                .Apply(static (TA a, TB b) => (A: a, B: b))
                .As(),
            GeometryShape<TA, TB>.FirstOnly firstOnly => (
                    context.ValidateOperand(operand: firstOnly.A, requirement: firstOnly.Requirement),
                    firstOnly.B.ValidateNativeOperand())
                .Apply(static (TA a, TB b) => (A: a, B: b))
                .As(),
            _ => Fin.Fail<(TA A, TB B)>(ValidationFault.MissingGeometry()).ToValidation(),
        };
    internal static Validation<Error, TValue> ValidateOperand<TValue>(
        this GeometryContext context,
        TValue operand,
        GeometryRequirement requirement) where TValue : notnull =>
        (
            Fin.Succ((Context: context, Requirement: requirement)).ToValidation(),
            Optional(operand).ToValidation(ValidationFault.MissingGeometry())
        ).Apply(static ((GeometryContext Context, GeometryRequirement Requirement) state, TValue candidate) => (
            state.Context,
            state.Requirement,
            Candidate: candidate))
        .Bind(static ((GeometryContext Context, GeometryRequirement Requirement, TValue Candidate) state) => state.Candidate switch {
            GeometryBase geometry => (
                state.Context.Validate(
                    geometry: geometry,
                    requirement: state.Requirement),
                Fin.Succ(state.Candidate).ToValidation()
            ).Apply(static (GeometryBase _, TValue candidate) => candidate)
            .As(),
            _ => state.Candidate.ValidateNativeOperand(),
        })
        .As();
    private static Validation<Error, TValue> ValidateNativeOperand<TValue>(this TValue operand) =>
        operand switch {
            Plane plane => plane.IsValid switch {
                true => Fin.Succ(operand).ToValidation(),
                false => Fin.Fail<TValue>(ValidationFault.InvalidNativeOperand(type: typeof(Plane))).ToValidation(),
            },
            Line line => line.IsValid switch {
                true => Fin.Succ(operand).ToValidation(),
                false => Fin.Fail<TValue>(ValidationFault.InvalidNativeOperand(type: typeof(Line))).ToValidation(),
            },
            Circle circle => circle.IsValid switch {
                true => Fin.Succ(operand).ToValidation(),
                false => Fin.Fail<TValue>(ValidationFault.InvalidNativeOperand(type: typeof(Circle))).ToValidation(),
            },
            Arc arc => arc.IsValid switch {
                true => Fin.Succ(operand).ToValidation(),
                false => Fin.Fail<TValue>(ValidationFault.InvalidNativeOperand(type: typeof(Arc))).ToValidation(),
            },
            Sphere sphere => sphere.IsValid switch {
                true => Fin.Succ(operand).ToValidation(),
                false => Fin.Fail<TValue>(ValidationFault.InvalidNativeOperand(type: typeof(Sphere))).ToValidation(),
            },
            _ => Fin.Succ(operand).ToValidation(),
        };
}

// --- [MODELS] ----------------------------------------------------------------------------------

[Union]
public abstract partial record GeometryRequirement {
    internal abstract Seq<GeometryCheck> Checks { get; }
    public sealed record NoneRequirement : GeometryRequirement {
        internal override Seq<GeometryCheck> Checks =>
            Seq<GeometryCheck>();
    }
    public sealed record BasicRequirement : GeometryRequirement {
        internal override Seq<GeometryCheck> Checks =>
            Seq<GeometryCheck>(GeometryCheck.RhinoValidity, GeometryCheck.UsableBounds);
    }
    public sealed record CurveLengthRequirement : GeometryRequirement {
        internal override Seq<GeometryCheck> Checks =>
            GeometryCheck.BasicChecks.Add(GeometryCheck.CurveLengthReadiness);
    }
    public sealed record AreaMassRequirement : GeometryRequirement {
        internal override Seq<GeometryCheck> Checks =>
            GeometryCheck.BasicChecks.Add(GeometryCheck.CurveAreaReadiness).Add(GeometryCheck.CurveSelfIntersection);
    }
    public sealed record MeshCheckRequirement : GeometryRequirement {
        internal override Seq<GeometryCheck> Checks =>
            GeometryCheck.BasicChecks.Add(GeometryCheck.MeshRhinoCheck);
    }
    public sealed record SolidTopologyRequirement : GeometryRequirement {
        internal override Seq<GeometryCheck> Checks =>
            GeometryCheck.BasicChecks
                .Add(GeometryCheck.BrepIntegrity)
                .Add(GeometryCheck.MeshManifoldReadiness)
                .Add(GeometryCheck.BrepSolidReadiness)
                .Add(GeometryCheck.MeshRhinoCheck);
    }
    public sealed record VolumeMassRequirement : GeometryRequirement {
        internal override Seq<GeometryCheck> Checks =>
            GeometryCheck.BasicChecks
                .Add(GeometryCheck.BrepIntegrity)
                .Add(GeometryCheck.MeshManifoldReadiness)
                .Add(GeometryCheck.BrepSolidReadiness)
                .Add(GeometryCheck.MeshRhinoCheck)
                .Add(GeometryCheck.SurfaceSolidReadiness);
    }
    public sealed record SurfaceEvaluationRequirement : GeometryRequirement {
        internal override Seq<GeometryCheck> Checks =>
            GeometryCheck.BasicChecks.Add(GeometryCheck.SurfaceDomainReadiness);
    }
    public sealed record StrictStructureRequirement : GeometryRequirement {
        internal override Seq<GeometryCheck> Checks =>
            GeometryCheck.BasicChecks
                .Add(GeometryCheck.SurfaceDomainReadiness)
                .Add(GeometryCheck.ContinuityReadiness)
                .Add(GeometryCheck.PolycurveStructure);
    }
    public sealed record StrictRequirement : GeometryRequirement {
        internal override Seq<GeometryCheck> Checks =>
            GeometryCheck.AllChecks;
    }
    public static readonly GeometryRequirement None = new NoneRequirement();
    public static readonly GeometryRequirement Basic = new BasicRequirement();
    public static readonly GeometryRequirement CurveLength = new CurveLengthRequirement();
    public static readonly GeometryRequirement AreaMass = new AreaMassRequirement();
    public static readonly GeometryRequirement MeshCheck = new MeshCheckRequirement();
    public static readonly GeometryRequirement SolidTopology = new SolidTopologyRequirement();
    public static readonly GeometryRequirement VolumeMass = new VolumeMassRequirement();
    public static readonly GeometryRequirement SurfaceEvaluation = new SurfaceEvaluationRequirement();
    public static readonly GeometryRequirement StrictStructure = new StrictStructureRequirement();
    public static readonly GeometryRequirement Strict = new StrictRequirement();
}

internal static class GeometryRequirementExtensions {
    internal static bool Has(this GeometryRequirement self, GeometryRequirement other) =>
        other.Checks.Fold(
            initialState: (Self: self, AllPresent: true),
            f: static ((GeometryRequirement Self, bool AllPresent) acc, GeometryCheck check) => (
                acc.Self,
                AllPresent: acc.AllPresent && acc.Self.Checks.Fold(
                    initialState: (Target: check, Found: false),
                    f: static ((GeometryCheck Target, bool Found) inner, GeometryCheck candidate) => (
                        inner.Target,
                        Found: inner.Found || ReferenceEquals(objA: candidate, objB: inner.Target))).Found)).AllPresent;
}

// --- [CHECKS] ----------------------------------------------------------------------------------

[Union]
internal abstract partial record GeometryCheck {
    internal abstract string Key { get; }
    internal abstract bool Applies(GeometryBase geometry);
    internal abstract Fin<Unit> Validate(GeometryContext context, GeometryBase geometry);
    internal Fin<Unit> Apply(GeometryContext context, GeometryBase geometry) =>
        Applies(geometry: geometry) switch {
            true => Validate(context: context, geometry: geometry),
            false => Fin.Succ(unit),
        };
    internal Fin<Unit> Invalid(GeometryBase geometry, string log) =>
        Fin.Fail<Unit>(error: ValidationFault.InvalidGeometry(
            geometry: geometry,
            check: this,
            log: log));
    internal Fin<Unit> Require(GeometryBase geometry, bool condition, string log) =>
        condition switch {
            true => Fin.Succ(unit),
            false => Invalid(geometry: geometry, log: log),
        };
    private static string Detail(string label, string log) =>
        string.IsNullOrWhiteSpace(value: log) switch {
            true => label,
            false => string.Create(provider: CultureInfo.InvariantCulture, $"{label}: {log}"),
        };
    internal sealed record RhinoValidityCheck : GeometryCheck {
        internal override string Key => "rhino-validity";
        internal override bool Applies(GeometryBase geometry) => true;
        internal override Fin<Unit> Validate(GeometryContext context, GeometryBase geometry) =>
            geometry.IsValidWithLog(out string log) switch {
                true => Fin.Succ(unit),
                false => Invalid(geometry: geometry, log: log),
            };
    }
    internal sealed record UsableBoundsCheck : GeometryCheck {
        internal override string Key => "usable-bounds";
        internal override bool Applies(GeometryBase geometry) => true;
        internal override Fin<Unit> Validate(GeometryContext context, GeometryBase geometry) =>
            geometry.GetBoundingBox(accurate: true) switch {
                BoundingBox box => Require(
                    geometry: geometry,
                    condition: box.IsValid && box.IsDegenerate(tolerance: context.Absolute.Value) < 4,
                    log: "Rhino could not compute a usable accurate bounding box."),
            };
    }
    internal sealed record BrepIntegrityCheck : GeometryCheck {
        internal override string Key => "brep-integrity";
        internal override bool Applies(GeometryBase geometry) => geometry is Brep;
        internal override Fin<Unit> Validate(GeometryContext context, GeometryBase geometry) =>
            geometry switch {
                Brep brep => (
                    Topology: brep.IsValidTopology(log: out string topologyLog),
                    Geometry: brep.IsValidGeometry(log: out string geometryLog),
                    Tolerances: brep.IsValidTolerancesAndFlags(log: out string toleranceLog)
                ) switch {
                    (false, _, _) => Invalid(geometry: geometry, log: Detail(label: "Brep topology", log: topologyLog)),
                    (_, false, _) => Invalid(geometry: geometry, log: Detail(label: "Brep geometry", log: geometryLog)),
                    (_, _, false) => Invalid(geometry: geometry, log: Detail(label: "Brep tolerances and flags", log: toleranceLog)),
                    _ => Fin.Succ(unit),
                },
                _ => Fin.Succ(unit),
            };
    }
    internal sealed record MeshRhinoCheckCheck : GeometryCheck {
        internal override string Key => "mesh-rhino-check";
        internal override bool Applies(GeometryBase geometry) => geometry is Mesh;
        internal override Fin<Unit> Validate(GeometryContext context, GeometryBase geometry) {
            // BOUNDARY ADAPTER — Mesh.Check requires a TextLog by-ref out and ref MeshCheckParameters; using-local +
            // ref parameter cannot be expressed in pure expression form, so the imperative shape lives at the
            // GeometryCheck case boundary and returns a Fin<Unit>.
            using TextLog textLog = new();
            MeshCheckParameters parameters = MeshCheckParameters.Defaults();
            return geometry switch {
                Mesh mesh => (
                    mesh.Check(textLog: textLog, parameters: ref parameters),
                    mesh.Faces.Count > 0) switch {
                        (true, true) => Fin.Succ(unit),
                        _ => Invalid(geometry: mesh, log: textLog.ToString()),
                    },
                _ => Fin.Succ(unit),
            };
        }
    }
    internal sealed record MeshManifoldReadinessCheck : GeometryCheck {
        internal override string Key => "mesh-manifold-readiness";
        internal override bool Applies(GeometryBase geometry) => geometry is Mesh;
        internal override Fin<Unit> Validate(GeometryContext context, GeometryBase geometry) =>
            Require(
                geometry: geometry,
                condition: ((Mesh)geometry).IsSolid,
                log: "Mesh is valid Rhino geometry but is not closed and solid enough for volume operations.");
    }
    internal sealed record BrepSolidReadinessCheck : GeometryCheck {
        internal override string Key => "brep-solid-readiness";
        internal override bool Applies(GeometryBase geometry) => geometry is Brep;
        internal override Fin<Unit> Validate(GeometryContext context, GeometryBase geometry) =>
            Require(
                geometry: geometry,
                condition: ((Brep)geometry).IsSolid,
                log: "Brep is valid Rhino geometry but is not solid enough for volume operations.");
    }
    internal sealed record SurfaceSolidReadinessCheck : GeometryCheck {
        internal override string Key => "surface-solid-readiness";
        internal override bool Applies(GeometryBase geometry) => geometry is Surface;
        internal override Fin<Unit> Validate(GeometryContext context, GeometryBase geometry) =>
            Require(
                geometry: geometry,
                condition: ((Surface)geometry).IsSolid,
                log: "Surface is valid Rhino geometry but is not solid enough for volume operations.");
    }
    internal sealed record CurveLengthReadinessCheck : GeometryCheck {
        internal override string Key => "curve-length-readiness";
        internal override bool Applies(GeometryBase geometry) => geometry is Curve;
        internal override Fin<Unit> Validate(GeometryContext context, GeometryBase geometry) =>
            geometry switch {
                Curve curve => (
                    curve.IsShort(tolerance: context.Absolute.Value),
                    curve.GetLength(fractionalTolerance: context.Relative.Value) > context.Absolute.Value
                ) switch {
                    (false, true) => Fin.Succ(unit),
                    _ => Invalid(geometry: geometry, log: "Curve is valid Rhino geometry but is below model-length tolerance."),
                },
                _ => Fin.Succ(unit),
            };
    }
    internal sealed record CurveAreaReadinessCheck : GeometryCheck {
        internal override string Key => "curve-area-readiness";
        internal override bool Applies(GeometryBase geometry) => geometry is Curve;
        internal override Fin<Unit> Validate(GeometryContext context, GeometryBase geometry) =>
            geometry switch {
                Curve curve => (
                    curve.IsClosed,
                    curve.TryGetPlane(
                        plane: out Plane _,
                        tolerance: context.Absolute.Value)
                ) switch {
                    (true, true) => Fin.Succ(unit),
                    _ => Invalid(geometry: geometry, log: "Curve is valid Rhino geometry but is not closed and planar enough for area operations."),
                },
                _ => Fin.Succ(unit),
            };
    }
    internal sealed record SurfaceDomainReadinessCheck : GeometryCheck {
        internal override string Key => "surface-domain-readiness";
        internal override bool Applies(GeometryBase geometry) => geometry is Surface;
        internal override Fin<Unit> Validate(GeometryContext context, GeometryBase geometry) =>
            Require(
                geometry: geometry,
                condition: HasUsableDomain(surface: (Surface)geometry, context: context),
                log: "Surface is valid Rhino geometry but has an unusable UV domain.");
        internal static bool HasUsableDomain(Surface surface, GeometryContext context) =>
            (
                surface.Domain(direction: 0).IsValid,
                surface.Domain(direction: 1).IsValid,
                surface.Domain(direction: 0).Length > context.Absolute.Value,
                surface.Domain(direction: 1).Length > context.Absolute.Value
            ) switch {
                (true, true, true, true) => true,
                _ => false,
            };
    }
    internal sealed record ContinuityReadinessCheck : GeometryCheck {
        internal override string Key => "continuity-readiness";
        internal override bool Applies(GeometryBase geometry) => geometry is Curve or Surface;
        internal override Fin<Unit> Validate(GeometryContext context, GeometryBase geometry) =>
            geometry switch {
                Surface surface => Require(
                    geometry: geometry,
                    condition: SurfaceDomainReadinessCheck.HasUsableDomain(surface: surface, context: context) switch {
                        false => true,
                        true => (
                            surface.GetNextDiscontinuity(
                                direction: 0,
                                continuityType: Continuity.C1_continuous,
                                t0: surface.Domain(direction: 0).T0,
                                t1: surface.Domain(direction: 0).T1,
                                t: out double _),
                            surface.GetNextDiscontinuity(
                                direction: 1,
                                continuityType: Continuity.C1_continuous,
                                t0: surface.Domain(direction: 1).T0,
                                t1: surface.Domain(direction: 1).T1,
                                t: out double _)) switch {
                                    (false, false) => true,
                                    _ => false,
                                },
                    },
                    log: "Surface is valid Rhino geometry but contains a C1 discontinuity."),
                Curve curve => Require(
                    geometry: geometry,
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
            };
    }
    internal sealed record PolycurveStructureCheck : GeometryCheck {
        internal override string Key => "polycurve-structure";
        internal override bool Applies(GeometryBase geometry) => geometry is PolyCurve;
        internal override Fin<Unit> Validate(GeometryContext context, GeometryBase geometry) =>
            geometry switch {
                PolyCurve polyCurve => (polyCurve.HasGap, polyCurve.IsNested) switch {
                    (false, false) => Fin.Succ(unit),
                    _ => Invalid(geometry: geometry, log: (polyCurve.HasGap, polyCurve.IsNested) switch {
                        (true, true) => "PolyCurve has gaps between segments and nested polycurves.",
                        (true, false) => "PolyCurve has gaps between segments.",
                        (false, true) => "PolyCurve contains nested polycurves.",
                        _ => "PolyCurve structure is usable.",
                    }),
                },
                _ => Fin.Succ(unit),
            };
    }
    internal sealed record CurveSelfIntersectionCheck : GeometryCheck {
        internal override string Key => "curve-self-intersection";
        internal override bool Applies(GeometryBase geometry) => geometry is Curve;
        internal override Fin<Unit> Validate(GeometryContext context, GeometryBase geometry) {
            // BOUNDARY ADAPTER — CurveIntersections owns native unmanaged state and must be disposed via using;
            // its construction lives at the GeometryCheck case boundary and returns a Fin<Unit>.
            using CurveIntersections? intersections = geometry switch {
                Curve curve => Intersection.CurveSelf(curve: curve, tolerance: context.Absolute.Value),
                _ => null,
            };
            return intersections switch {
                CurveIntersections hits => hits.Count switch {
                    0 => Fin.Succ(unit),
                    _ => Invalid(geometry: geometry, log: string.Create(
                        provider: CultureInfo.InvariantCulture,
                        $"Rhino found {hits.Count} curve self-intersection event(s).")),
                },
                _ => geometry switch {
                    Curve _ => Invalid(geometry: geometry, log: "Rhino curve self-intersection computation failed."),
                    _ => Fin.Succ(unit),
                },
            };
        }
    }
    internal static readonly GeometryCheck RhinoValidity = new RhinoValidityCheck();
    internal static readonly GeometryCheck UsableBounds = new UsableBoundsCheck();
    internal static readonly GeometryCheck BrepIntegrity = new BrepIntegrityCheck();
    internal static readonly GeometryCheck MeshRhinoCheck = new MeshRhinoCheckCheck();
    internal static readonly GeometryCheck MeshManifoldReadiness = new MeshManifoldReadinessCheck();
    internal static readonly GeometryCheck BrepSolidReadiness = new BrepSolidReadinessCheck();
    internal static readonly GeometryCheck SurfaceSolidReadiness = new SurfaceSolidReadinessCheck();
    internal static readonly GeometryCheck CurveLengthReadiness = new CurveLengthReadinessCheck();
    internal static readonly GeometryCheck CurveAreaReadiness = new CurveAreaReadinessCheck();
    internal static readonly GeometryCheck SurfaceDomainReadiness = new SurfaceDomainReadinessCheck();
    internal static readonly GeometryCheck ContinuityReadiness = new ContinuityReadinessCheck();
    internal static readonly GeometryCheck PolycurveStructure = new PolycurveStructureCheck();
    internal static readonly GeometryCheck CurveSelfIntersection = new CurveSelfIntersectionCheck();
    internal static readonly Seq<GeometryCheck> BasicChecks =
        Seq<GeometryCheck>(RhinoValidity, UsableBounds);
    internal static readonly Seq<GeometryCheck> AllChecks = Seq<GeometryCheck>(
        RhinoValidity,
        UsableBounds,
        BrepIntegrity,
        MeshRhinoCheck,
        MeshManifoldReadiness,
        BrepSolidReadiness,
        SurfaceSolidReadiness,
        CurveLengthReadiness,
        CurveAreaReadiness,
        SurfaceDomainReadiness,
        ContinuityReadiness,
        PolycurveStructure,
        CurveSelfIntersection);
}

// --- [ERRORS] ----------------------------------------------------------------------------------

internal static class ValidationFault {
    internal static Error MissingGeometry() =>
        Error.New(message: "Geometry input is required.");
    internal static Error InvalidNativeOperand(Type type) =>
        Error.New(message: string.Create(
            provider: CultureInfo.InvariantCulture,
            $"Geometry native operand '{type.Name}' is invalid."));
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
