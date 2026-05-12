using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Domain;

// --- [TYPES] ----------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
internal readonly partial struct Op;

// --- [MODELS] ---------------------------------------------------------------------------
public sealed record Requirement {
    private Requirement(Seq<Rule> checks) => Checks = checks;
    internal Seq<Rule> Checks { get; }
    internal bool IsEmpty => Checks.IsEmpty;
    internal static Requirement Of(Seq<Rule> rules) => new(checks: rules);
    internal Requirement With(Seq<Rule> more) => new(checks: Checks.Concat(more).ToSeq());
    public static readonly Requirement None = Of(rules: Seq<Rule>());
    public static readonly Requirement Basic = Of(rules: Seq(Rule.Validity, Rule.UsableBounds));
    public static readonly Requirement CurveLength = Basic.With(more: Seq(Rule.CurveLengthReadiness));
    public static readonly Requirement AreaMass = Basic.With(more: Seq(Rule.CurveAreaReadiness, Rule.CurveSelfIntersection));
    public static readonly Requirement MeshCheck = Basic.With(more: Seq(Rule.MeshRhinoCheck));
    public static readonly Requirement SolidTopology = Basic.With(more: Seq(Rule.BrepIntegrity, Rule.MeshManifoldReadiness, Rule.BrepSolidReadiness, Rule.MeshRhinoCheck));
    public static readonly Requirement VolumeMass = SolidTopology.With(more: Seq(Rule.SurfaceSolidReadiness));
    public static readonly Requirement SurfaceEvaluation = Basic.With(more: Seq(Rule.SurfaceDomainReadiness));
    public static readonly Requirement StrictStructure = SurfaceEvaluation.With(more: Seq(Rule.ContinuityReadiness, Rule.PolycurveStructure));
    public static readonly Requirement Strict = Of(rules: toSeq(Rule.Items));
}

// --- [CONSTANTS] ------------------------------------------------------------------------
[SmartEnum<string>]
internal sealed partial class Rule {
    private Func<GeometryBase, bool> Applies { get; }
    private Func<Rule, Context, GeometryBase, Fin<Unit>> Check { get; }
    internal Fin<Unit> Apply(Context context, GeometryBase geometry) =>
        Applies(arg: geometry) switch {
            true => Check(arg1: this, arg2: context, arg3: geometry),
            false => Fin.Succ(unit),
        };
    internal Fin<Unit> Invalid(GeometryBase geometry, string log) =>
        Fin.Fail<Unit>(error: new ValidationFault.InvalidGeometry(Geometry: geometry, Check: this, Log: log));
    internal Fin<Unit> Require(GeometryBase geometry, bool condition, string log) =>
        condition switch { true => Fin.Succ(unit), false => Invalid(geometry: geometry, log: log) };
    private static string Detail(string label, string log) =>
        string.IsNullOrWhiteSpace(value: log) switch { true => label, false => $"{label}: {log}" };
    private static bool HasUsableDomain(Surface surface, Context context) => (
        surface.Domain(direction: 0).IsValid,
        surface.Domain(direction: 1).IsValid,
        surface.Domain(direction: 0).Length > context.Absolute.Value,
        surface.Domain(direction: 1).Length > context.Absolute.Value) switch {
            (true, true, true, true) => true,
            _ => false,
        };
    [BoundaryAdapter]
    private static Fin<Unit> RunMeshCheck(Rule rule, Context context, GeometryBase geometry) {
        using TextLog textLog = new();
        MeshCheckParameters parameters = MeshCheckParameters.Defaults();
        return geometry switch {
            Mesh mesh => (mesh.Check(textLog: textLog, parameters: ref parameters), mesh.Faces.Count > 0) switch {
                (true, true) => Fin.Succ(unit),
                _ => rule.Invalid(geometry: mesh, log: textLog.ToString()),
            },
            _ => Fin.Succ(unit),
        };
    }
    [BoundaryAdapter]
    private static Fin<Unit> RunCurveSelfIntersection(Rule rule, Context context, GeometryBase geometry) {
        using CurveIntersections? intersections = geometry switch {
            Curve curve => Intersection.CurveSelf(curve: curve, tolerance: context.Absolute.Value),
            _ => null,
        };
        return (intersections, geometry) switch {
            (CurveIntersections hits, _) when hits.Count == 0 => Fin.Succ(unit),
            (CurveIntersections hits, _) => rule.Invalid(geometry: geometry, log: string.Create(provider: CultureInfo.InvariantCulture, $"Rhino found {hits.Count} curve self-intersection event(s).")),
            (null, Curve _) => rule.Invalid(geometry: geometry, log: "Rhino curve self-intersection computation failed."),
            _ => Fin.Succ(unit),
        };
    }
    public static readonly Rule Validity = new(key: "rhino-validity", applies: static _ => true, check: static (rule, _, geometry) => geometry.IsValidWithLog(out string log) switch {
        true => Fin.Succ(unit),
        false => rule.Invalid(geometry: geometry, log: log),
    });
    public static readonly Rule UsableBounds = new(key: "usable-bounds", applies: static _ => true, check: static (rule, context, geometry) => rule.Require(
        geometry: geometry,
        condition: geometry.GetBoundingBox(accurate: true) is { IsValid: true } box && box.IsDegenerate(tolerance: context.Absolute.Value) < 4,
        log: "Rhino could not compute a usable accurate bounding box."));
    public static readonly Rule BrepIntegrity = new(key: "brep-integrity", applies: static geometry => geometry is Brep, check: static (rule, _, geometry) => geometry switch {
        Brep brep => brep.IsValidTopology(log: out string topologyLog) switch {
            false => rule.Invalid(geometry: brep, log: Detail(label: "Brep topology", log: topologyLog)),
            true => brep.IsValidGeometry(log: out string geometryLog) switch {
                false => rule.Invalid(geometry: brep, log: Detail(label: "Brep geometry", log: geometryLog)),
                true => brep.IsValidTolerancesAndFlags(log: out string toleranceLog) switch {
                    true => Fin.Succ(unit),
                    false => rule.Invalid(geometry: brep, log: Detail(label: "Brep tolerances and flags", log: toleranceLog)),
                },
            },
        },
        _ => Fin.Succ(unit),
    });
    public static readonly Rule MeshRhinoCheck = new(key: "mesh-rhino-check", applies: static geometry => geometry is Mesh, check: RunMeshCheck);
    public static readonly Rule MeshManifoldReadiness = new(key: "mesh-manifold-readiness", applies: static geometry => geometry is Mesh, check: static (rule, _, geometry) =>
        rule.Require(geometry: geometry, condition: ((Mesh)geometry).IsSolid, log: "Mesh is valid Rhino geometry but is not closed and solid enough for volume operations."));
    public static readonly Rule BrepSolidReadiness = new(key: "brep-solid-readiness", applies: static geometry => geometry is Brep, check: static (rule, _, geometry) =>
        rule.Require(geometry: geometry, condition: ((Brep)geometry).IsSolid, log: "Brep is valid Rhino geometry but is not solid enough for volume operations."));
    public static readonly Rule SurfaceSolidReadiness = new(key: "surface-solid-readiness", applies: static geometry => geometry is Surface, check: static (rule, _, geometry) =>
        rule.Require(geometry: geometry, condition: ((Surface)geometry).IsSolid, log: "Surface is valid Rhino geometry but is not solid enough for volume operations."));
    public static readonly Rule CurveLengthReadiness = new(key: "curve-length-readiness", applies: static geometry => geometry is Curve, check: static (rule, context, geometry) => geometry switch {
        Curve curve => (curve.IsShort(tolerance: context.Absolute.Value), curve.GetLength(fractionalTolerance: context.Relative.Value) > context.Absolute.Value) switch {
            (false, true) => Fin.Succ(unit),
            _ => rule.Invalid(geometry: curve, log: "Curve is valid Rhino geometry but is below model-length tolerance."),
        },
        _ => Fin.Succ(unit),
    });
    public static readonly Rule CurveAreaReadiness = new(key: "curve-area-readiness", applies: static geometry => geometry is Curve, check: static (rule, context, geometry) => geometry switch {
        Curve curve => (curve.IsClosed, curve.TryGetPlane(plane: out Plane _, tolerance: context.Absolute.Value)) switch {
            (true, true) => Fin.Succ(unit),
            _ => rule.Invalid(geometry: curve, log: "Curve is valid Rhino geometry but is not closed and planar enough for area operations."),
        },
        _ => Fin.Succ(unit),
    });
    public static readonly Rule SurfaceDomainReadiness = new(key: "surface-domain-readiness", applies: static geometry => geometry is Surface, check: static (rule, context, geometry) =>
        rule.Require(geometry: geometry, condition: HasUsableDomain(surface: (Surface)geometry, context: context), log: "Surface is valid Rhino geometry but has an unusable UV domain."));
    public static readonly Rule ContinuityReadiness = new(key: "continuity-readiness", applies: static geometry => geometry is Curve or Surface, check: static (rule, context, geometry) => geometry switch {
        Surface surface => rule.Require(geometry: surface, condition: !HasUsableDomain(surface: surface, context: context) || (
            !surface.GetNextDiscontinuity(direction: 0, continuityType: Continuity.C1_continuous, t0: surface.Domain(direction: 0).T0, t1: surface.Domain(direction: 0).T1, t: out double _)
            && !surface.GetNextDiscontinuity(direction: 1, continuityType: Continuity.C1_continuous, t0: surface.Domain(direction: 1).T0, t1: surface.Domain(direction: 1).T1, t: out double _)), log: "Surface is valid Rhino geometry but contains a C1 discontinuity."),
        Curve curve => rule.Require(geometry: curve, condition: !curve.GetNextDiscontinuity(continuityType: Continuity.C1_continuous, t0: curve.Domain.T0, t1: curve.Domain.T1, t: out double _), log: "Curve is valid Rhino geometry but contains a C1 discontinuity."),
        _ => Fin.Succ(unit),
    });
    public static readonly Rule PolycurveStructure = new(key: "polycurve-structure", applies: static geometry => geometry is PolyCurve, check: static (rule, _, geometry) => geometry switch {
        PolyCurve poly => (poly.HasGap, poly.IsNested) switch {
            (false, false) => Fin.Succ(unit),
            (true, true) => rule.Invalid(geometry: poly, log: "PolyCurve has gaps between segments and nested polycurves."),
            (true, false) => rule.Invalid(geometry: poly, log: "PolyCurve has gaps between segments."),
            _ => rule.Invalid(geometry: poly, log: "PolyCurve contains nested polycurves."),
        },
        _ => Fin.Succ(unit),
    });
    public static readonly Rule CurveSelfIntersection = new(key: "curve-self-intersection", applies: static geometry => geometry is Curve, check: RunCurveSelfIntersection);
}

// --- [ERRORS] ---------------------------------------------------------------------------
[Union]
internal abstract partial record OpFault : Error {
    private OpFault() { }
    internal const int UnsupportedCode = 9104;
    public override bool IsExpected => true;
    public override bool IsExceptional => false;
    public override ErrorException ToErrorException() => new WrappedErrorExpectedException(this);
    internal sealed record MissingOperation : OpFault {
        public override string Message => "Geometry operation requires a query.";
    }
    internal sealed record MissingContext(Op Key) : OpFault {
        public override string Message => $"Geometry operation '{Key}' requires a model context.";
    }
    internal sealed record InvalidInput(Op Key) : OpFault {
        public override string Message => $"Geometry operation '{Key}' received invalid Rhino input.";
    }
    internal sealed record InvalidResult(Op Key) : OpFault {
        public override string Message => $"Geometry operation '{Key}' produced no valid Rhino result.";
    }
    internal sealed record Cancelled : OpFault {
        public override string Message => "Geometry operation was cancelled.";
    }
    internal sealed record Unsupported(Op Key, Type GeometryType, Type OutputType) : OpFault {
        public override string Message => $"Geometry operation '{Key}' does not support geometry '{GeometryType.Name}' with output '{OutputType.Name}'.";
        public override int Code => UnsupportedCode;
    }
    internal sealed record ComputationFailed(string Label) : OpFault {
        public override string Message => $"Rhino {Label} computation failed.";
    }
    internal sealed record ComputationUnsupported(string Label, Type GeometryType) : OpFault {
        public override string Message => $"Rhino {Label} computation does not support geometry '{GeometryType.Name}'.";
    }
    internal sealed record PrimitiveNoEdges(Op Key, string Primitive) : OpFault {
        public override string Message => $"Geometry operation '{Key}' rejects '{Primitive}' primitive: no edges.";
    }
    internal sealed record PrimitiveNoVertices(Op Key, string Primitive) : OpFault {
        public override string Message => $"Geometry operation '{Key}' rejects '{Primitive}' primitive: no vertices.";
    }
}
[Union]
internal abstract partial record ValidationFault : Error {
    private ValidationFault() { }
    public override bool IsExpected => true;
    public override bool IsExceptional => false;
    public override ErrorException ToErrorException() => new WrappedErrorExpectedException(this);
    internal sealed record MissingGeometry : ValidationFault {
        public override string Message => "Geometry input is required.";
    }
    internal sealed record InvalidGeometry(GeometryBase Geometry, Rule Check, string Log) : ValidationFault {
        public override string Message => string.IsNullOrWhiteSpace(value: Log) switch {
            true => $"Geometry validation failed for {Geometry.GetType().Name} under check '{Check.Key}'.",
            false => $"Geometry validation failed for {Geometry.GetType().Name} under check '{Check.Key}': {Log}",
        };
    }
}
// Extension surface on Op preserves the 216 `key.X()` ergonomic call sites while
// the construction is now backed by the sealed OpFault DU above. Each call lifts
// into the corresponding OpFault case via its primary constructor.
internal static class OpFaultExtensions {
    internal static Error MissingContext(this Op key) => new OpFault.MissingContext(Key: key);
    internal static Error InvalidInput(this Op key) => new OpFault.InvalidInput(Key: key);
    internal static Error InvalidResult(this Op key) => new OpFault.InvalidResult(Key: key);
    internal static Error Unsupported(this Op key, Type geometryType, Type outputType) =>
        new OpFault.Unsupported(Key: key, GeometryType: geometryType, OutputType: outputType);
    internal static Error PrimitiveNoEdges(this Op key, string primitive) =>
        new OpFault.PrimitiveNoEdges(Key: key, Primitive: primitive);
    internal static Error PrimitiveNoVertices(this Op key, string primitive) =>
        new OpFault.PrimitiveNoVertices(Key: key, Primitive: primitive);
}

// --- [OPERATIONS] -----------------------------------------------------------------------
internal static class Verify {
    internal static Validation<Error, TGeometry> Apply<TGeometry>(this Context context, TGeometry? geometry, Requirement requirement) where TGeometry : GeometryBase =>
        (Fin.Succ((Context: context, Requirement: requirement)).ToValidation(),
         Optional(geometry).ToValidation<Error>(new ValidationFault.MissingGeometry()))
            .Apply(static (state, candidate) => (state.Context, state.Requirement, Candidate: candidate))
            .Bind(static state => state.Requirement.Checks.Aggregate(
                seed: (Acc: Fin.Succ(state.Candidate).ToValidation(), state.Context, state.Candidate),
                func: static (folder, rule) => (
                    Acc: (folder.Acc, rule.Apply(context: folder.Context, geometry: folder.Candidate).ToValidation())
                        .Apply(static (validated, _) => validated).As(), folder.Context, folder.Candidate)).Acc).As();
    internal static Validation<Error, (TA A, TB B)> Pair<TA, TB>(this Context context, TA a, TB b, Requirement requirementA, Requirement requirementB) where TA : notnull where TB : notnull =>
        (context.Operand(value: a, requirement: requirementA),
         context.Operand(value: b, requirement: requirementB))
            .Apply(static (left, right) => (A: left, B: right)).As();
    private static Validation<Error, TValue> Operand<TValue>(this Context context, TValue value, Requirement requirement) where TValue : notnull =>
        (Fin.Succ((Context: context, Requirement: requirement)).ToValidation(),
         Optional(value).ToValidation<Error>(new ValidationFault.MissingGeometry()))
            .Apply(static (state, candidate) => (state.Context, state.Requirement, Candidate: candidate))
            .Bind(static state => state.Candidate switch {
                GeometryBase geometry => (
                    state.Context.Apply(geometry: geometry, requirement: state.Requirement), Fin.Succ(state.Candidate).ToValidation()
                ).Apply(static (_, candidate) => candidate).As(),
                _ => Op.Create(value: "Operand").RequireValid(value: state.Candidate).ToValidation(),
            }).As();
}
