# [RASM_CONTEXT]

`Context` binds one `ModelUnit` to the tolerance triad every kernel operation threads. `ModelUnit` admits built-in and custom unit identity with its meters-per-unit scale; `AbsoluteTolerance`, `RelativeTolerance`, and `AngleTolerance` gate the geometric predicates. Fractional tolerance, mesh-intersection tolerance, and cross-context scaling derive from this one bundle.

`Context` is host-neutral by construction: every member is unit- and scalar-driven except the `[BoundaryAdapter]` `Of(RhinoDoc?)` factory, so a federation runtime that never sees a `RhinoDoc` constructs the identical bundle through the scalar and unit factories.

## [01]-[INDEX]

- [02]-[TOLERANCE_TRIAD]: `AbsoluteTolerance`/`RelativeTolerance`/`AngleTolerance` — three `[ValueObject<double>]` owners with admission-time range guards.
- [03]-[MODEL_CONTEXT]: `ModelUnit`, `Context`, the polymorphic `Of` family, the derived tolerance and scale projections, and the UnitsNet unit-bridge seam.

## [02]-[TOLERANCE_TRIAD]

- Owner: three `[ValueObject<double>]` readonly structs, each gating its raw scalar through a `ValidateFactoryArguments` guard admitted exactly once; the interior reads `.Value` and never re-checks.
- Cases: `AbsoluteTolerance` rejects a zero-or-negative distance (cannot gate a geometric predicate); `RelativeTolerance` rejects a fraction at or above one (no tolerance); `AngleTolerance` admits only the finite radian interval its guard bounds.
- Entry: the generated `Create`/`TryCreate`/`Validate` factories; rail admission composes the `validation.md` factory bridge, lifting a rejection into `Fault.OutOfRange` carrying the owner name, the rejected scalar, and the requirement text.
- Law: `KeyMemberName = "Value"` is public so tolerance scalars feed host math without egress ceremony; the owner exists for admission and identity, not to hide the double.
- Boundary: every guard floor is a named `EpsilonPolicy` row and every angular ceiling `Math.Tau`, so one degeneracy floor serves the whole triad and a second epsilon spelled per struct is the deleted form; the values are pure scalars and cross every runtime.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Csp;
using Rasm.Numerics;
using Rhino;

namespace Rasm.Domain;

// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct AbsoluteTolerance {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = (double.IsFinite(d: value), value > EpsilonPolicy.ZeroTolerance) switch {
            (true, true) => null,
            (false, _) => new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"AbsoluteTolerance must be finite (got {value}).")),
            (_, false) => new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"AbsoluteTolerance must be > {EpsilonPolicy.ZeroTolerance}.")),
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
        validationError = (double.IsFinite(d: value), value is > EpsilonPolicy.ZeroTolerance and <= Math.Tau) switch {
            (true, true) => null,
            _ => new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"AngleTolerance must be in (zero-tolerance, 2*pi] radians (got {value}).")),
        };
}
```

## [03]-[MODEL_CONTEXT]

- Owner: `ModelUnit` is the admitted unit regime — defined `UnitSystem`, positive finite meters per unit, required custom name; `Context` binds one `ModelUnit` to the three admitted tolerances.
- Entry: the `Context.Of` family accepts scalar tolerances with `UnitSystem` or `LengthUnit`, derives defaults from either unit carrier, and retains `Millimeters()` as the context-free default; `ScaleTo(Context)` divides the admitted meters-per-unit values after admitting the target; `ModelUnit.Convert(value, from, to, key)` is the ONE dynamic-conversion seam onto the UnitsNet vocabulary (guarded `UnitConverter.TryConvert`, typed refusal on an unregistered pair) and `ModelUnit.Converter<TQuantity>(from, to, key)` its hot-path row caching one `UnitConverter.Default.GetConversionFunction<TQuantity>` delegate per pair — `ScaleTo` stays the one scale owner, so unit identity and cross-context rescale never merge into one hand-kept factor.
- Cases: `UnitSystem` ingress admits defined built-in rows; `LengthUnit` ingress admits built-in and custom rows, preserving custom name and scale; incomplete `CustomUnits`, `Unset`, `None`, and undefined ordinals fail before context construction.
- Auto: `Fractional` (the arc-length tolerance feeding `Curve.GetLength`/`NormalizedLengthParameters`) and `MeshIntersectionTolerance` (host-coefficient-scaled, read by every mesh-intersection call) derive once here; `Fractional` and the default relative tolerance both floor on `EpsilonPolicy.SqrtEpsilon` because they are dimensionless relative gates, while the absolute default rides the host distance default through the admitted unit scale — a bare per-module literal standing for either is unreplayable across operators.
- Law: `Of(RhinoDoc?)` is the document-coupled boundary adapter, projecting the document tolerances and units so custom scale and name survive unchanged.
- Packages: Thinktecture.Runtime.Extensions (`[ValueObject<double>]`), LanguageExt.Core (`Validation`, `Fin`, applicative `Apply`), `Numerics/atoms` (`EpsilonPolicy` — the named epsilon rows every guard floor and relative default read), RhinoCommon (`LengthUnit`, `UnitSystem`, `RhinoDoc`, `RhinoMath` host defaults, `Intersection`), UnitsNet (`UnitConverter`, `IQuantity` — the unit-bridge seam).
- Growth: a new model-space fact (a fourth tolerance, a grid-resolution policy, a document epoch) is one validated slot and one factory argument on the scalar floor, inherited by every derived factory.
- Boundary: `Context` threads explicitly — a parameter on synchronous rails, inside `Env` on `Eff` pipelines (`rails.md` Op law), never a global default; `Analyze.From`/`Analyze.In` (`Analysis/query.md`) forward over the `Of` family, `Env` carrying the constructed `Context`.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Csp;
using Rhino;

namespace Rasm.Domain;

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ModelUnit {
    private ModelUnit(UnitSystem system, double metersPerUnit, Option<string> name) {
        System = system;
        MetersPerUnit = metersPerUnit;
        Name = name;
    }

    public UnitSystem System { get; }
    public double MetersPerUnit { get; }
    public Option<string> Name { get; }

    internal static Fin<ModelUnit> Of(UnitSystem value, Op key) => value switch {
        var unknown when !Enum.IsDefined(value: unknown) =>
            Fin.Fail<ModelUnit>(error: new Fault.InvalidUnitSystem(Units: unknown, Requirement: "must be a defined unit system")),
        UnitSystem.Unset or UnitSystem.None =>
            Fin.Fail<ModelUnit>(error: new Fault.InvalidUnitSystem(Units: value, Requirement: "must be a model unit system")),
        UnitSystem.CustomUnits =>
            Fin.Fail<ModelUnit>(error: new Fault.InvalidUnitSystem(Units: value, Requirement: "must carry custom name and scale")),
        _ => key.Catch(() => Of(value: LengthUnit.FromKnownUnitSystem(knownUnitSystem: value), key: key)),
    };

    internal static Fin<ModelUnit> Of(LengthUnit value, Op key) => key.Catch(() => {
        UnitSystem system = value.ToUnitSystem(metersPerUnit: out double metersPerUnit);
        Option<string> name = system == UnitSystem.CustomUnits
            ? Optional(value.Name).Map(static text => text.Trim()).Filter(static text => text.Length > 0)
            : Option<string>.None;
        return !LengthUnit.IsUnset(in value)
            && !LengthUnit.IsNone(in value)
            && Enum.IsDefined(value: system)
            && system is not UnitSystem.Unset and not UnitSystem.None
            && double.IsFinite(d: metersPerUnit)
            && metersPerUnit > 0d
            && (system != UnitSystem.CustomUnits || name.IsSome)
                ? Fin.Succ(value: new ModelUnit(system: system, metersPerUnit: metersPerUnit, name: name))
                : Fin.Fail<ModelUnit>(error: new Fault.InvalidUnitSystem(
                    Units: system,
                    Requirement: "must carry positive finite scale and custom identity"));
    });

    internal Fin<double> ScaleTo(ModelUnit? target, Op key) =>
        from destination in Optional(target).ToFin(Fail: key.MissingContext())
        let scale = MetersPerUnit / destination.MetersPerUnit
        from admitted in double.IsFinite(d: scale) && scale > 0d
            ? Fin.Succ(value: scale)
            : Fin.Fail<double>(error: key.InvalidResult())
        select admitted;

    // --- [UNIT_BRIDGE]
    // Convert is the ONE seam onto the admitted units vocabulary: a measure leaves with unit identity by resolving here,
    // while ScaleTo above stays the one cross-context scale owner — a consumer hand-multiplying a conversion
    // factor beside a quantity enum is the deleted form. The guarded conversion refuses an unregistered or
    // non-finite pair typed instead of throwing.
    public static Fin<double> Convert(double value, Enum from, Enum to, Op? key = null) =>
        UnitsNet.UnitConverter.TryConvert(value, from, to, out double converted) && double.IsFinite(d: converted)
            ? Fin.Succ(value: converted)
            : Fin.Fail<double>(error: key.OrDefault().InvalidInput());
    // Converter is the hot-path row: one cached conversion delegate resolved once per (from, to) pair off the default
    // registry, so a per-sample projection pays a delegate call, never a registry lookup.
    public static Fin<Func<TQuantity, TQuantity>> Converter<TQuantity>(Enum from, Enum to, Op? key = null) where TQuantity : UnitsNet.IQuantity =>
        key.OrDefault().Catch(() => new Func<TQuantity, TQuantity>(UnitsNet.UnitConverter.Default.GetConversionFunction<TQuantity>(from, to).Invoke));
}

public sealed record Context {
    private Context(AbsoluteTolerance absolute, RelativeTolerance relative, AngleTolerance angle, ModelUnit unit) {
        Absolute = absolute;
        Relative = relative;
        Angle = angle;
        Unit = unit;
    }

    public static Validation<Error, Context> Of(double absolute, double relative, double angle, UnitSystem units) =>
        Build(absolute: absolute, relative: relative, angle: angle, unit: ModelUnit.Of(value: units, key: Op.Of(name: nameof(Context))));

    public static Validation<Error, Context> Of(double absolute, double relative, double angle, LengthUnit units) =>
        Build(absolute: absolute, relative: relative, angle: angle, unit: ModelUnit.Of(value: units, key: Op.Of(name: nameof(Context))));

    public static Validation<Error, Context> Millimeters() => Of(units: UnitSystem.Millimeters);

    public static Validation<Error, Context> Of(UnitSystem units) =>
        Default(unit: ModelUnit.Of(value: units, key: Op.Of(name: nameof(Context))));

    public static Validation<Error, Context> Of(LengthUnit units) =>
        Default(unit: ModelUnit.Of(value: units, key: Op.Of(name: nameof(Context))));

    [BoundaryAdapter]
    public static Validation<Error, Context> Of(RhinoDoc? doc) =>
        Optional(doc).ToValidation<Error>(Fail: new Fault.MissingContext(Key: Op.Of(name: nameof(Context))))
            .Bind(static candidate => Of(
                absolute: candidate.ModelAbsoluteTolerance,
                relative: candidate.ModelRelativeTolerance,
                angle: candidate.ModelAngleToleranceRadians,
                units: candidate.ModelUnits));

    public AbsoluteTolerance Absolute { get; }
    public RelativeTolerance Relative { get; }
    public AngleTolerance Angle { get; }
    public ModelUnit Unit { get; }
    public UnitSystem Units => Unit.System;
    // Fractional is a DIMENSIONLESS arc-length tolerance, so its floor is the named relative-gate row rather than
    // a per-module absolute: sqrt-epsilon is the first-order accuracy a length quadrature can actually deliver,
    // and a bare 1e-8 spelled here is unreplayable and uncomparable against every other relative gate in the corpus.
    public double Fractional => Relative.Value > 0.0 ? Relative.Value : EpsilonPolicy.SqrtEpsilon;
    public double MeshIntersectionTolerance => Absolute.Value * Intersection.MeshIntersectionsTolerancesCoefficient;

    public Fin<double> ScaleTo(Context? target) {
        Op op = Op.Of(name: nameof(ScaleTo));
        return Optional(target).ToFin(Fail: op.MissingContext())
            .Bind(destination => Unit.ScaleTo(target: destination.Unit, key: op));
    }

    private static Validation<Error, Context> Build(
        double absolute,
        double relative,
        double angle,
        Fin<ModelUnit> unit) =>
        (absolute.TryCreateValidated<double, AbsoluteTolerance>(),
         relative.TryCreateValidated<double, RelativeTolerance>(),
         angle.TryCreateValidated<double, AngleTolerance>(),
         unit.ToValidation())
            .Apply(static (a, r, n, u) => new Context(absolute: a, relative: r, angle: n, unit: u))
            .As();

    private static Validation<Error, Context> Default(Fin<ModelUnit> unit) {
        Op op = Op.Of(name: nameof(Context));
        return (from target in unit
                from millimeters in ModelUnit.Of(value: UnitSystem.Millimeters, key: op)
                from scale in millimeters.ScaleTo(target: target, key: op)
                select (Unit: target, Scale: scale))
            .ToValidation()
            .Bind(admitted => Build(
                absolute: RhinoMath.DefaultDistanceToleranceMillimeters * admitted.Scale,
                relative: EpsilonPolicy.SqrtEpsilon,
                angle: RhinoMath.DefaultAngleTolerance,
                unit: Fin.Succ(value: admitted.Unit)));
    }
}
```

## [04]-[DENSITY_BAR]

One admitted unit regime and one context factory family own every model-space ingress.

| [INDEX] | [CONCERN]         | [OWNER]                                                  | [SHAPE]                                     |
| :-----: | :---------------- | :------------------------------------------------------- | :------------------------------------------ |
|  [01]   | tolerance scalars | `AbsoluteTolerance`/`RelativeTolerance`/`AngleTolerance` | generated scalar admission                  |
|  [02]   | unit regime       | `ModelUnit`                                              | built-in/custom identity and metric scale   |
|  [03]   | model context     | `Context`                                                | polymorphic factory and derived projections |

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
