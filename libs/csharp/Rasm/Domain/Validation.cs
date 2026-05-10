using System.Globalization;
using System.Linq;
using Foundation.CSharp.Analyzers.Contracts;
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
public abstract partial record Pair<TA, TB> where TA : notnull where TB : notnull {
    public sealed record Both(TA A, TB B, Requirement RequirementA, Requirement RequirementB) : Pair<TA, TB>;
    public sealed record FirstOnly(TA A, TB B, Requirement Requirement) : Pair<TA, TB>;
}

// --- [OPERATIONS] ------------------------------------------------------------------------------

internal static class Check {
    internal static Validation<Error, TGeometry> Validate<TGeometry>(
        this Context context,
        TGeometry? geometry,
        Requirement requirement) where TGeometry : GeometryBase =>
        (Validation<Error, TGeometry>)(
            Fin.Succ((Context: context, Requirement: requirement)).ToValidation(),
            Optional(geometry).ToValidation(ValidationFault.MissingGeometry())
        ).Apply(static ((Context Context, Requirement Requirement) state, TGeometry candidate) => (
            state.Context,
            state.Requirement,
            Geometry: candidate))
        .Bind(static ((Context Context, Requirement Requirement, TGeometry Geometry) state) =>
            state.Requirement.Checks.Aggregate(
                seed: (Result: Fin.Succ(state.Geometry).ToValidation(), state.Context, state.Geometry),
                func: static (
                    (Validation<Error, TGeometry> Result, Context Context, TGeometry Geometry) accumulator,
                    Rule check) => (
                        Result: (accumulator.Result, check.Apply(
                                context: accumulator.Context,
                                geometry: accumulator.Geometry).ToValidation())
                            .Apply(static (TGeometry candidate, Unit _) => candidate)
                            .As(),
                        accumulator.Context,
                        accumulator.Geometry)).Result);
    internal static Validation<Error, (TA A, TB B)> Validate<TA, TB>(
        this Context context,
        Pair<TA, TB> shape) where TA : notnull where TB : notnull =>
        shape switch {
            Pair<TA, TB>.Both both => (
                    context.ValidateOperand(operand: both.A, requirement: both.RequirementA),
                    context.ValidateOperand(operand: both.B, requirement: both.RequirementB))
                .Apply(static (TA a, TB b) => (A: a, B: b))
                .As(),
            Pair<TA, TB>.FirstOnly firstOnly => (
                    context.ValidateOperand(operand: firstOnly.A, requirement: firstOnly.Requirement),
                    firstOnly.B.ValidateNativeOperand())
                .Apply(static (TA a, TB b) => (A: a, B: b))
                .As(),
            _ => Fin.Fail<(TA A, TB B)>(ValidationFault.MissingGeometry()).ToValidation(),
        };
    internal static Validation<Error, TValue> ValidateOperand<TValue>(
        this Context context,
        TValue operand,
        Requirement requirement) where TValue : notnull =>
        (
            Fin.Succ((Context: context, Requirement: requirement)).ToValidation(),
            Optional(operand).ToValidation(ValidationFault.MissingGeometry())
        ).Apply(static ((Context Context, Requirement Requirement) state, TValue candidate) => (
            state.Context,
            state.Requirement,
            Candidate: candidate))
        .Bind(static ((Context Context, Requirement Requirement, TValue Candidate) state) => state.Candidate switch {
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
public abstract partial record Requirement {
    internal abstract Seq<Rule> Checks { get; }
    public sealed record NoneRequirement : Requirement {
        internal override Seq<Rule> Checks =>
            Seq<Rule>();
    }
    public sealed record BasicRequirement : Requirement {
        internal override Seq<Rule> Checks =>
            Seq<Rule>(Rule.Validity, Rule.UsableBounds);
    }
    public sealed record CurveLengthRequirement : Requirement {
        internal override Seq<Rule> Checks =>
            Rule.BasicChecks.Add(Rule.CurveLengthReadiness);
    }
    public sealed record AreaMassRequirement : Requirement {
        internal override Seq<Rule> Checks =>
            Rule.BasicChecks.Add(Rule.CurveAreaReadiness).Add(Rule.CurveSelfIntersection);
    }
    public sealed record MeshCheckRequirement : Requirement {
        internal override Seq<Rule> Checks =>
            Rule.BasicChecks.Add(Rule.MeshRhinoCheck);
    }
    public sealed record SolidTopologyRequirement : Requirement {
        internal override Seq<Rule> Checks =>
            Rule.BasicChecks
                .Add(Rule.BrepIntegrity)
                .Add(Rule.MeshManifoldReadiness)
                .Add(Rule.BrepSolidReadiness)
                .Add(Rule.MeshRhinoCheck);
    }
    public sealed record VolumeMassRequirement : Requirement {
        internal override Seq<Rule> Checks =>
            Rule.BasicChecks
                .Add(Rule.BrepIntegrity)
                .Add(Rule.MeshManifoldReadiness)
                .Add(Rule.BrepSolidReadiness)
                .Add(Rule.MeshRhinoCheck)
                .Add(Rule.SurfaceSolidReadiness);
    }
    public sealed record SurfaceEvaluationRequirement : Requirement {
        internal override Seq<Rule> Checks =>
            Rule.BasicChecks.Add(Rule.SurfaceDomainReadiness);
    }
    public sealed record StrictStructureRequirement : Requirement {
        internal override Seq<Rule> Checks =>
            Rule.BasicChecks
                .Add(Rule.SurfaceDomainReadiness)
                .Add(Rule.ContinuityReadiness)
                .Add(Rule.PolycurveStructure);
    }
    public sealed record StrictRequirement : Requirement {
        internal override Seq<Rule> Checks =>
            Rule.AllChecks;
    }
    public static readonly Requirement None = new NoneRequirement();
    public static readonly Requirement Basic = new BasicRequirement();
    public static readonly Requirement CurveLength = new CurveLengthRequirement();
    public static readonly Requirement AreaMass = new AreaMassRequirement();
    public static readonly Requirement MeshCheck = new MeshCheckRequirement();
    public static readonly Requirement SolidTopology = new SolidTopologyRequirement();
    public static readonly Requirement VolumeMass = new VolumeMassRequirement();
    public static readonly Requirement SurfaceEvaluation = new SurfaceEvaluationRequirement();
    public static readonly Requirement StrictStructure = new StrictStructureRequirement();
    public static readonly Requirement Strict = new StrictRequirement();
}

internal static class RequirementExtensions {
    internal static bool Has(this Requirement self, Requirement other) =>
        other.Checks.Fold(
            initialState: (Self: self, AllPresent: true),
            f: static ((Requirement Self, bool AllPresent) acc, Rule check) => (
                acc.Self,
                AllPresent: acc.AllPresent && acc.Self.Checks.Fold(
                    initialState: (Target: check, Found: false),
                    f: static ((Rule Target, bool Found) inner, Rule candidate) => (
                        inner.Target,
                        Found: inner.Found || ReferenceEquals(objA: candidate, objB: inner.Target))).Found)).AllPresent;
}

// --- [CHECKS] ----------------------------------------------------------------------------------

[Union]
internal abstract partial record Rule {
    internal abstract string Key { get; }
    internal abstract bool Applies(GeometryBase geometry);
    internal abstract Fin<Unit> Validate(Context context, GeometryBase geometry);
    internal Fin<Unit> Apply(Context context, GeometryBase geometry) =>
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
            false => $"{label}: {log}",
        };
    internal sealed record ValidityCheck : Rule {
        internal override string Key => "rhino-validity";
        internal override bool Applies(GeometryBase geometry) => true;
        internal override Fin<Unit> Validate(Context context, GeometryBase geometry) =>
            geometry.IsValidWithLog(out string log) switch {
                true => Fin.Succ(unit),
                false => Invalid(geometry: geometry, log: log),
            };
    }
    internal sealed record UsableBoundsCheck : Rule {
        internal override string Key => "usable-bounds";
        internal override bool Applies(GeometryBase geometry) => true;
        internal override Fin<Unit> Validate(Context context, GeometryBase geometry) =>
            geometry.GetBoundingBox(accurate: true) switch {
                BoundingBox box => Require(
                    geometry: geometry,
                    condition: box.IsValid && box.IsDegenerate(tolerance: context.Absolute.Value) < 4,
                    log: "Rhino could not compute a usable accurate bounding box."),
            };
    }
    internal sealed record BrepIntegrityCheck : Rule {
        internal override string Key => "brep-integrity";
        internal override bool Applies(GeometryBase geometry) => geometry is Brep;
        internal override Fin<Unit> Validate(Context context, GeometryBase geometry) =>
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
    internal sealed record MeshRhinoCheckCheck : Rule {
        internal override string Key => "mesh-rhino-check";
        internal override bool Applies(GeometryBase geometry) => geometry is Mesh;
        [BoundaryImperativeExemption(
            ruleId: "CSP0001",
            reason: BoundaryImperativeReason.ProtocolRequired,
            ticket: "RASM-MESH-CHECK",
            expiresOnUtc: "2027-12-31T00:00:00Z")]
        internal override Fin<Unit> Validate(Context context, GeometryBase geometry) {
            // BOUNDARY ADAPTER — Mesh.Check requires a TextLog by-ref out and ref MeshCheckParameters; using-local +
            // ref parameter cannot be expressed in pure expression form, so the imperative shape lives at the
            // Rule case boundary and returns a Fin<Unit>.
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
    internal sealed record MeshManifoldReadinessCheck : Rule {
        internal override string Key => "mesh-manifold-readiness";
        internal override bool Applies(GeometryBase geometry) => geometry is Mesh;
        internal override Fin<Unit> Validate(Context context, GeometryBase geometry) =>
            Require(
                geometry: geometry,
                condition: ((Mesh)geometry).IsSolid,
                log: "Mesh is valid Rhino geometry but is not closed and solid enough for volume operations.");
    }
    internal sealed record BrepSolidReadinessCheck : Rule {
        internal override string Key => "brep-solid-readiness";
        internal override bool Applies(GeometryBase geometry) => geometry is Brep;
        internal override Fin<Unit> Validate(Context context, GeometryBase geometry) =>
            Require(
                geometry: geometry,
                condition: ((Brep)geometry).IsSolid,
                log: "Brep is valid Rhino geometry but is not solid enough for volume operations.");
    }
    internal sealed record SurfaceSolidReadinessCheck : Rule {
        internal override string Key => "surface-solid-readiness";
        internal override bool Applies(GeometryBase geometry) => geometry is Surface;
        internal override Fin<Unit> Validate(Context context, GeometryBase geometry) =>
            Require(
                geometry: geometry,
                condition: ((Surface)geometry).IsSolid,
                log: "Surface is valid Rhino geometry but is not solid enough for volume operations.");
    }
    internal sealed record CurveLengthReadinessCheck : Rule {
        internal override string Key => "curve-length-readiness";
        internal override bool Applies(GeometryBase geometry) => geometry is Curve;
        internal override Fin<Unit> Validate(Context context, GeometryBase geometry) =>
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
    internal sealed record CurveAreaReadinessCheck : Rule {
        internal override string Key => "curve-area-readiness";
        internal override bool Applies(GeometryBase geometry) => geometry is Curve;
        internal override Fin<Unit> Validate(Context context, GeometryBase geometry) =>
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
    internal sealed record SurfaceDomainReadinessCheck : Rule {
        internal override string Key => "surface-domain-readiness";
        internal override bool Applies(GeometryBase geometry) => geometry is Surface;
        internal override Fin<Unit> Validate(Context context, GeometryBase geometry) =>
            Require(
                geometry: geometry,
                condition: HasUsableDomain(surface: (Surface)geometry, context: context),
                log: "Surface is valid Rhino geometry but has an unusable UV domain.");
        internal static bool HasUsableDomain(Surface surface, Context context) =>
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
    internal sealed record ContinuityReadinessCheck : Rule {
        internal override string Key => "continuity-readiness";
        internal override bool Applies(GeometryBase geometry) => geometry is Curve or Surface;
        internal override Fin<Unit> Validate(Context context, GeometryBase geometry) =>
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
    internal sealed record PolycurveStructureCheck : Rule {
        internal override string Key => "polycurve-structure";
        internal override bool Applies(GeometryBase geometry) => geometry is PolyCurve;
        internal override Fin<Unit> Validate(Context context, GeometryBase geometry) =>
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
    internal sealed record CurveSelfIntersectionCheck : Rule {
        internal override string Key => "curve-self-intersection";
        internal override bool Applies(GeometryBase geometry) => geometry is Curve;
        internal override Fin<Unit> Validate(Context context, GeometryBase geometry) {
            // BOUNDARY ADAPTER — CurveIntersections owns native unmanaged state and must be disposed via using;
            // its construction lives at the Rule case boundary and returns a Fin<Unit>.
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
    internal static readonly Rule Validity = new ValidityCheck();
    internal static readonly Rule UsableBounds = new UsableBoundsCheck();
    internal static readonly Rule BrepIntegrity = new BrepIntegrityCheck();
    internal static readonly Rule MeshRhinoCheck = new MeshRhinoCheckCheck();
    internal static readonly Rule MeshManifoldReadiness = new MeshManifoldReadinessCheck();
    internal static readonly Rule BrepSolidReadiness = new BrepSolidReadinessCheck();
    internal static readonly Rule SurfaceSolidReadiness = new SurfaceSolidReadinessCheck();
    internal static readonly Rule CurveLengthReadiness = new CurveLengthReadinessCheck();
    internal static readonly Rule CurveAreaReadiness = new CurveAreaReadinessCheck();
    internal static readonly Rule SurfaceDomainReadiness = new SurfaceDomainReadinessCheck();
    internal static readonly Rule ContinuityReadiness = new ContinuityReadinessCheck();
    internal static readonly Rule PolycurveStructure = new PolycurveStructureCheck();
    internal static readonly Rule CurveSelfIntersection = new CurveSelfIntersectionCheck();
    internal static readonly Seq<Rule> BasicChecks =
        Seq<Rule>(Validity, UsableBounds);
    internal static readonly Seq<Rule> AllChecks = Seq<Rule>(
        Validity,
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
        Error.New(message: $"Geometry native operand '{type.Name}' is invalid.");
    internal static Error InvalidGeometry(GeometryBase geometry, Rule check, string log) =>
        Error.New(message: string.IsNullOrWhiteSpace(value: log) switch {
            true => $"Geometry validation failed for {geometry.GetType().Name} under check '{check.Key}'.",
            false => $"Geometry validation failed for {geometry.GetType().Name} under check '{check.Key}': {log}",
        });
}
