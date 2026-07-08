# [RASM_CONTEXT]

The tolerance/units substrate (`Rasm.Domain`). This page owns the tolerance triad — `AbsoluteTolerance`, `RelativeTolerance`, `AngleTolerance` — and the immutable `Context` bundle every kernel operation threads. `Context` is the model-space fact of record: one validated bundle of the three tolerances plus the `UnitSystem`, constructed through one polymorphic `Of` family, derived from once for fractional and mesh-intersection tolerances. Every closest-point recovery, readiness check, coercion, sampling fraction, and intersection call reads its tolerance from a threaded `Context`, never from an ambient constant or a per-call literal.

`Context` is host-neutral-shaped by construction: every member is unit- and scalar-driven except the single `[BoundaryAdapter]` `Of(RhinoDoc?)` factory — the ONE doc-coupled seam in the whole substrate. Federation runtimes that never see a `RhinoDoc` construct the identical `Context` through the scalar and unit factories.

## [01]-[INDEX]

- [02]-[TOLERANCE_TRIAD]: `AbsoluteTolerance`/`RelativeTolerance`/`AngleTolerance` — three `[ValueObject<double>]` owners with admission-time range guards.
- [03]-[MODEL_CONTEXT]: `Context` — the immutable bundle, the polymorphic `Of` family, the `Millimeters` canonical default, and the `Fractional`/`MeshIntersectionTolerance` derivations.

## [02]-[TOLERANCE_TRIAD]

- Owner: three `[ValueObject<double>]` readonly structs, each with a `ValidateFactoryArguments` admission guard — raw scalars are admitted exactly once; the interior reads `.Value` and never re-checks.
- Cases: `AbsoluteTolerance` — finite and `> RhinoMath.ZeroTolerance` (a zero or negative distance tolerance cannot gate any geometric predicate) · `RelativeTolerance` — finite and in `[0, 1)` (a fractional tolerance at or above one is no tolerance) · `AngleTolerance` — finite and in `(RhinoMath.Epsilon, RhinoMath.TwoPI]` radians.
- Entry: the generated `Create`/`TryCreate`/`Validate` factories; rail admission is the receiver-generic `TryCreateValidated<TVO>` bridge (`validation.md` — the extension-form call supplies the combined type-argument list, receiver width first: `absolute.TryCreateValidated<double, AbsoluteTolerance>()`) lifting the generated `ValidationError` into `Fault.OutOfRange` carrying the owner name, the rejected scalar, and the requirement text.
- Law: `KeyMemberName = "Value"` with public access — tolerance scalars feed host math (`curve.IsShort(tolerance: ctx.Absolute.Value)`) without egress ceremony; the owner exists for admission and identity, not to hide the double.
- Boundary: the guards read `RhinoMath` bounds because the triad gates host geometry; the values themselves are pure scalars and cross every runtime.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Csp;
using Rhino;

namespace Rasm.Domain;

// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct AbsoluteTolerance {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = (double.IsFinite(d: value), value > RhinoMath.ZeroTolerance) switch {
            (true, true) => null,
            (false, _) => new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"AbsoluteTolerance must be finite (got {value}).")),
            (_, false) => new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"AbsoluteTolerance must be > {RhinoMath.ZeroTolerance}.")),
        };
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct RelativeTolerance {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = (double.IsFinite(d: value), value is >= 0.0 and < 1.0) switch {
            (true, true) => null,
            _ => new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"RelativeTolerance must lie in [0,1) (got {value}).")),
        };
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct AngleTolerance {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = (double.IsFinite(d: value), value is > RhinoMath.Epsilon and <= RhinoMath.TwoPI) switch {
            (true, true) => null,
            _ => new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"AngleTolerance must be in (epsilon, 2*pi] radians (got {value}).")),
        };
}
```

## [03]-[MODEL_CONTEXT]

- Owner: `Context` sealed record — private constructor, four read-only slots (`Absolute`, `Relative`, `Angle`, `Units`), two derivations. Construction is only through the validated `Of` family; a `Context` in hand is proof the whole bundle admitted.
- Entry: ONE polymorphic name, four ingress shapes — `Of(double absolute, double relative, double angle, UnitSystem units)` the scalar floor; `Of(UnitSystem units)` the unit-derived default; `Of(RhinoDoc? doc)` the doc boundary adapter; `Millimeters()` the canonical validated default. All return `Validation<Error, Context>`; the scalar floor accumulates every tolerance fault applicatively before reporting.
- Cases: unit gating inside the family — `Unset`/`None` reject as `Fault.InvalidUnitSystem` ("must be a Rhino model unit system"); `CustomUnits` rejects on the unit-derived path ("must be explicit when custom" — a custom unit has no derivable default scale and must arrive through the scalar floor); every other unit derives its absolute default as `RhinoMath.DefaultDistanceToleranceMillimeters × RhinoMath.UnitScale(Millimeters → units)` gated positive-finite.
- Auto: `Fractional` — the arc-length fractional tolerance (`Relative.Value` when positive, else the `1.0e-8` default) feeding `Curve.GetLength`/`NormalizedLengthParameters`; `MeshIntersectionTolerance` — `Absolute.Value × Intersection.MeshIntersectionsTolerancesCoefficient`, the host-coefficient-scaled tolerance every mesh intersection call reads. Both derive once here; a consumer re-deriving either is the deleted form.
- Law: `Of(RhinoDoc?)` is the ONE doc-coupled member in the substrate — it projects `ModelAbsoluteTolerance`/`ModelRelativeTolerance`/`ModelAngleToleranceRadians`/`ModelUnitSystem` and immediately re-enters the scalar floor, so a doc-sourced `Context` and a federation-constructed `Context` are indistinguishable downstream. An absent doc is `Fault.MissingContext`, never a null propagation.
- Law: threading is explicit — `Context` rides as a parameter on the synchronous rails and inside `Env` on `Eff` pipelines (the `rails.md` threading law); no operation reads a global default, and `Millimeters()` exists so a context-free entry can still construct a real validated bundle.
- Packages: Thinktecture.Runtime.Extensions (`[ValueObject<double>]`), LanguageExt.Core (`Validation`, applicative `Apply`), RhinoCommon (`RhinoMath`, `UnitSystem`, `RhinoDoc`, `Intersection` — value reads only).
- Growth: a new model-space fact (a fourth tolerance, a grid resolution policy, a document epoch) is one validated slot plus one factory argument on the scalar floor — every derived factory inherits it; a parallel context record or an optional-parameter tail is the rejected form.
- Boundary: `Analyze.From(RhinoDoc)`/`Analyze.In(...)` (`Analysis/query.md`) are thin forwarders over this family; `Env` carries the constructed `Context`; this factory and that forwarder are the only two members in the corpus that name `RhinoDoc`.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Csp;
using Rhino;

namespace Rasm.Domain;

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record Context {
    private const double DefaultFractionalTolerance = 1.0e-8;
    private Context(AbsoluteTolerance absolute, RelativeTolerance relative, AngleTolerance angle, UnitSystem units) {
        Absolute = absolute;
        Relative = relative;
        Angle = angle;
        Units = units;
    }
    public static Validation<Error, Context> Of(double absolute, double relative, double angle, UnitSystem units) =>
        (absolute.TryCreateValidated<double, AbsoluteTolerance>(),
         relative.TryCreateValidated<double, RelativeTolerance>(),
         angle.TryCreateValidated<double, AngleTolerance>(),
         (units switch {
             UnitSystem.Unset or UnitSystem.None => Fin.Fail<UnitSystem>(error: new Fault.InvalidUnitSystem(Units: units, Requirement: "must be a Rhino model unit system")),
             _ => Fin.Succ(units),
         }).ToValidation())
            .Apply(static (a, r, n, u) => new Context(absolute: a, relative: r, angle: n, units: u))
            .As();
    public static Validation<Error, Context> Millimeters() =>
        Of(
            absolute: RhinoMath.DefaultDistanceToleranceMillimeters,
            relative: DefaultFractionalTolerance,
            angle: RhinoMath.DefaultAngleTolerance,
            units: UnitSystem.Millimeters);
    public static Validation<Error, Context> Of(UnitSystem units) => units switch {
        UnitSystem.Millimeters => Millimeters(),
        UnitSystem.CustomUnits => Fin.Fail<Context>(error: new Fault.InvalidUnitSystem(Units: units, Requirement: "must be explicit when custom")).ToValidation(),
        UnitSystem.Unset or UnitSystem.None => Fin.Fail<Context>(error: new Fault.InvalidUnitSystem(Units: units, Requirement: "must be a Rhino model unit system")).ToValidation(),
        _ => RhinoMath.UnitScale(from: UnitSystem.Millimeters, to: units) switch {
            double scale when RhinoMath.IsValidDouble(x: scale) && scale > RhinoMath.ZeroTolerance => Of(
                absolute: RhinoMath.DefaultDistanceToleranceMillimeters * scale,
                relative: DefaultFractionalTolerance,
                angle: RhinoMath.DefaultAngleTolerance,
                units: units),
            _ => Fin.Fail<Context>(error: new Fault.InvalidUnitSystem(Units: units, Requirement: "must resolve to a positive finite default scale")).ToValidation(),
        },
    };
    [BoundaryAdapter]
    public static Validation<Error, Context> Of(RhinoDoc? doc) =>
        Optional(doc).ToValidation<Error>(Fail: new Fault.MissingContext(Key: Op.Of(name: nameof(Context))))
            .Bind(static candidate => Of(
                absolute: candidate.ModelAbsoluteTolerance,
                relative: candidate.ModelRelativeTolerance,
                angle: candidate.ModelAngleToleranceRadians,
                units: candidate.ModelUnitSystem));
    public AbsoluteTolerance Absolute { get; }
    public RelativeTolerance Relative { get; }
    public AngleTolerance Angle { get; }
    public UnitSystem Units { get; }
    public double Fractional => Relative.Value > 0.0 ? Relative.Value : DefaultFractionalTolerance;
    public double MeshIntersectionTolerance => Absolute.Value * Intersection.MeshIntersectionsTolerancesCoefficient;
}
```

## [04]-[DENSITY_BAR]

One context owner, one factory family; a new model-space fact is one slot, never a sibling bundle.

| [INDEX] | [AXIS/CONCERN]     | [OWNER]                                                | [KIND]                                              | [RAIL]                                     | [CASES] |
| :-----: | :----------------- | :----------------------------------------------------- | :--------------------------------------------------- | :------------------------------------------ | :-----: |
|  [01]   | Tolerance scalars  | `AbsoluteTolerance`/`RelativeTolerance`/`AngleTolerance` | `[ValueObject<double>]` with admission guards        | `TryCreateValidated → Validation<Error, T>` |    3    |
|  [02]   | Model context      | `Context`                                              | sealed record, polymorphic `Of` family + derivations | `Of → Validation<Error, Context>`           |    4    |
