# [RASM_CONTEXT]

`Context` binds one `ModelUnit` to the tolerance vocabulary every kernel operation threads. `ToleranceLane` is that vocabulary — one row per named gate, each carrying the `Band` it admits through, the `BaseDimensions` signature it federates on, and the derivation that resolves it against a live context. `Tolerance` is the admitted pair a consumer reads; `Context.For(lane)` is the ONE read, answering from the context's overrides first and the lane's own derivation second.

`Context` is host-neutral by construction: every member is unit- and scalar-driven except the `[BoundaryAdapter]` `Of(RhinoDoc?)` factory, so a federation runtime that never sees a `RhinoDoc` constructs the identical bundle through the scalar and unit factories. Kernel operations pass measures as bare `double` — `Tolerance.Value` is a public scalar and `ToleranceLane.Dimension` a signature token, never a UnitsNet quantity; `Rasm.Element` `MeasureValue` remains the branch's dimensioned carrier.

## [01]-[INDEX]

- [02]-[TOLERANCE_LANES]: `ToleranceLane` rows and the `Tolerance` carrier admitted through each row's `Band`.
- [03]-[MODEL_CONTEXT]: `ModelUnit`, `Context`, the polymorphic `Of` family, the lane read, the override ingress, and the UnitsNet unit-bridge seam.
- [04]-[DENSITY_BAR]: owner-per-concern partition.

## [02]-[TOLERANCE_LANES]

- Owner: `ToleranceLane`, a `[SmartEnum<string>]` under an ordinal key policy, is the ONE tolerance vocabulary in the branch; `Tolerance` is the admitted `(lane, value)` pair every consumer reads. Rows absorb the three `[ValueObject<double>]` scalar kinds this page carried — they shared identity regime, admission path, payload timing, and consumer, and their only distinction was a RANGE, which is a policy value that belongs in a row.
- Cases: rows partition by `Band` — model-space distances on `Band.Length`, angular gates on `Band.Angle`, dimensionless fractions on `Band.Ratio`, unbounded convergence residuals and degeneracy elections on `Band.Residual`, device-space gates on `Band.Device`. `Numerics/atoms` owns those bands as range-guard rows; this page composes `Admits`/`Refuse` and declares no bound of its own.
- Entry: `Tolerance.Of(lane, value, key)` is the ONE admission, gating through `lane.Band` and refusing as `KernelFault.OutOfRange` carrying the lane key, the rejected scalar, and the band's own requirement text. `Tolerance` is a `readonly record struct`, NOT a Thinktecture value object, because a generated value object admits one raw key and cannot see the lane whose band decides the range.
- Law: a lane's `Derive` returns the DEFAULT for a context that carries no override, and it derives from an anchor — `Context.Absolute`, `Context.Relative`, `Context.Angle`, or an `EpsilonPolicy` row (`SqrtEpsilon`, `ZeroTolerance`, `CbrtEpsilon`) where the gate is numeric rather than model-scaled — never from a bare magnitude. `Relative` is a model PERCENT, so a lane multiplying by it scales a model distance; the six solver-residual lanes read the numeric anchor directly because a percent of a percent lands under `Band.Residual`'s floor and hands every consumer refusing evidence. Standards-table figures (an ISO 286 grade, an ASTM C216 size class, a shop assembly gate, a perceptual ΔE budget) carry PROVENANCE their own owner holds and enter through `Context.Overrides`; freezing one as a lane default plants a Materials or Fabrication constant in the kernel where no consumer can move it.
- Law: two lanes sharing a derivation are not duplicates. Each lane keys the OVERRIDE and names the vocabulary entry, so a project that tightens `Seam` leaves `Closure` untouched — identical defaults under distinct keys move independently.
- Law: absolute-versus-relative MODE is the lane's `Band` fact, never a carrier axis: a `Band.Ratio` lane's value is a FRACTION the consumer multiplies by its own magnitude (`Relative` scales a model distance, `Probe` scales a probe magnitude into a stencil step), a `Band.Length` lane's value IS the absolute model gate, so the carrier stays `(lane, value)` and a mode discriminant on `Tolerance` re-asserts what `Lane.Band` already declares. `Identity` elects frame identity on the degeneracy anchor rather than the solver anchor because an identity residual is arithmetic noise, not a convergence target; a page-local frame-epsilon or probe-step literal beside either row is the deleted form.
- Law: a device lane with no override derives `0.0`, which sits BELOW its band's closed floor, so the minted `Tolerance` answers `IsValid` false and visibly refuses rather than hand back a plausible-looking radius nobody chose — deriving the floor value itself admits. Hosts that forget to seed their pick radius meet that refusal, and the device pitch is knowledge only the boundary holds.

- Exemption: `Grain`, `Real`, `Duplicate`, and `Joint` sit in bands whose SI reading differs from their consumer's spelling (a radian grain, a ratio real-compare, a distance duplicate gate). `Band` carries the ADMISSION range and `Dimension` the physical signature, so the two axes stay independent by construction.
- Packages: `Numerics/atoms` (`Band` range-guard rows — `Admits`, `Refuse`, `Floor`; `EpsilonPolicy`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`, `[KeyMemberEqualityComparer]`, `[UseDelegateFromConstructor]`), UnitsNet (`BaseDimensions` — the branch federation point), LanguageExt.Core (`Fin`), `Domain/rails` (`Op`, `KernelFault.OutOfRange`, `IValidityEvidence`, `ValidityClaim`).
- Growth: a new gate is ONE row naming its band, its dimension, and its derivation; every consumer reads it through `Context.For` with no signature change anywhere.
- Boundary: `Tolerance.Value` is a public bare `double`, so `context.Absolute.Value`, `context.Angle.Value`, and every host read of the shape compile unchanged across the branch — the compatibility is load-bearing and deliberate, not incidental. `Rasm.Element` owns the dimensioned rendering of a tolerance.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Numerics;
using Rhino.Geometry.Intersect;
using Thinktecture;

namespace Rasm.Domain;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ToleranceLane {
    // --- [ROOT]
    // Root rows carry the three admitted context scalars every other row derives from, resolving off the STORED triad
    // rather than through `For`, so the derivation graph has a floor and no lane can key itself.
    public static readonly ToleranceLane Distance = new(key: "distance", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value);
    public static readonly ToleranceLane Relative = new(key: "relative", band: Band.Ratio, dimension: UnitsNet.BaseDimensions.Dimensionless, derive: static context => context.Relative.Value);
    public static readonly ToleranceLane Angle = new(key: "angle", band: Band.Angle, dimension: UnitsNet.Angle.BaseDimensions, derive: static context => context.Angle.Value);

    // --- [LENGTH]
    public static readonly ToleranceLane Chord = new(key: "chord", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value);
    public static readonly ToleranceLane Deviation = new(key: "deviation", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value);
    public static readonly ToleranceLane Gouge = new(key: "gouge", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value * EpsilonPolicy.SubTolerance);
    public static readonly ToleranceLane Seam = new(key: "seam", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value * context.Relative.Value);
    public static readonly ToleranceLane Closure = new(key: "closure", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value * context.Relative.Value);
    public static readonly ToleranceLane Weld = new(key: "weld", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value);
    public static readonly ToleranceLane Collapse = new(key: "collapse", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value * EpsilonPolicy.SubTolerance);
    public static readonly ToleranceLane Arc = new(key: "arc", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value);
    public static readonly ToleranceLane Offset = new(key: "offset", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value);
    public static readonly ToleranceLane Corner = new(key: "corner", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value);
    public static readonly ToleranceLane Root = new(key: "root", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value * context.Relative.Value);
    public static readonly ToleranceLane Match = new(key: "match", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value);
    public static readonly ToleranceLane Approach = new(key: "approach", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value);
    public static readonly ToleranceLane Filter = new(key: "filter", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value);
    public static readonly ToleranceLane PlaneDistance = new(key: "plane-distance", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value);
    public static readonly ToleranceLane Mollification = new(key: "mollification", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value * context.Relative.Value);
    // `Area` derives as a squared length rather than as an independently tuned magnitude.
    // Squared and coefficient-diminished anchors cross the Length floor on coarse model units (metres: 1e-10), so both
    // derives floor just above the open bound and a REAL gate arrives through `Context.Override` at the owner.
    public static readonly ToleranceLane Area = new(key: "area", band: Band.Length, dimension: UnitsNet.Area.BaseDimensions, derive: static context => Math.Max(context.Absolute.Value * context.Absolute.Value, EpsilonPolicy.SqrtEpsilon * 2.0));
    public static readonly ToleranceLane Length = new(key: "length", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value);
    public static readonly ToleranceLane Project = new(key: "project", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value);
    public static readonly ToleranceLane Neglect = new(key: "neglect", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value * EpsilonPolicy.SubTolerance);
    // Host-coefficient-scaled mesh crossing gate: the coefficient is RhinoCommon's own, so the lane composes it
    // once and the derived `MeshIntersectionTolerance` member this page used to carry deletes with it.
    public static readonly ToleranceLane MeshIntersection = new(key: "mesh-intersection", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => Math.Max(context.Absolute.Value * Intersection.MeshIntersectionsTolerancesCoefficient, EpsilonPolicy.SqrtEpsilon * 2.0));
    // Materials- and Fabrication-sourced length gates. Each derives the MODEL gate here and takes its published
    // figure — a spectral band budget, an irradiance floor, a shop assembly allowance, an ISO 286 grade width —
    // as a context override at the owner that holds the standard.
    public static readonly ToleranceLane Spectral = new(key: "spectral", band: Band.Length, dimension: UnitsNet.BaseDimensions.Dimensionless, derive: static context => context.Absolute.Value * context.Relative.Value);
    public static readonly ToleranceLane Irradiance = new(key: "irradiance", band: Band.Length, dimension: UnitsNet.Irradiance.BaseDimensions, derive: static context => context.Absolute.Value * context.Relative.Value);
    public static readonly ToleranceLane Build = new(key: "build", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value);
    public static readonly ToleranceLane Grade = new(key: "grade", band: Band.Length, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Absolute.Value);

    // --- [ANGLE]
    public static readonly ToleranceLane Orientation = new(key: "orientation", band: Band.Angle, dimension: UnitsNet.Angle.BaseDimensions, derive: static context => context.Angle.Value);
    public static readonly ToleranceLane Torsal = new(key: "torsal", band: Band.Angle, dimension: UnitsNet.Angle.BaseDimensions, derive: static context => context.Angle.Value);
    public static readonly ToleranceLane Symmetry = new(key: "symmetry", band: Band.Angle, dimension: UnitsNet.Angle.BaseDimensions, derive: static context => context.Angle.Value);
    public static readonly ToleranceLane Collinear = new(key: "collinear", band: Band.Angle, dimension: UnitsNet.Angle.BaseDimensions, derive: static context => context.Angle.Value);
    public static readonly ToleranceLane Cocircular = new(key: "cocircular", band: Band.Angle, dimension: UnitsNet.Angle.BaseDimensions, derive: static context => context.Angle.Value);
    // Grain-direction alignment gate (Fabrication nesting/remnant reads it in RADIANS — `GrainToleranceRadians` and
    // its >= π any-orientation guard): an ANGLE lane through and through. Its U5/U6-censused mismatch — the Length
    // band gating an Angle dimension off the Absolute DISTANCE anchor — handed every grain consumer a model-scaled
    // length wearing radians; band, dimension, and derive now agree on the angle root like every row in this group.
    public static readonly ToleranceLane Grain = new(key: "grain", band: Band.Angle, dimension: UnitsNet.Angle.BaseDimensions, derive: static context => context.Angle.Value);
    public static readonly ToleranceLane Real = new(key: "real", band: Band.Angle, dimension: UnitsNet.BaseDimensions.Dimensionless, derive: static context => context.Angle.Value);

    // --- [RATIO]
    public static readonly ToleranceLane Fraction = new(key: "fraction", band: Band.Ratio, dimension: UnitsNet.BaseDimensions.Dimensionless, derive: static context => context.Relative.Value);
    public static readonly ToleranceLane Drift = new(key: "drift", band: Band.Ratio, dimension: UnitsNet.BaseDimensions.Dimensionless, derive: static context => context.Relative.Value);
    public static readonly ToleranceLane Hue = new(key: "hue", band: Band.Ratio, dimension: UnitsNet.BaseDimensions.Dimensionless, derive: static context => context.Relative.Value);
    // Scale uniformity of a similarity frame is a SECOND-ORDER residual, so the relative gate squares.
    public static readonly ToleranceLane ScaleUniformity = new(key: "scale-uniformity", band: Band.Ratio, dimension: UnitsNet.BaseDimensions.Dimensionless, derive: static context => context.Relative.Value * context.Relative.Value);
    public static readonly ToleranceLane Coordinate = new(key: "coordinate", band: Band.Ratio, dimension: UnitsNet.BaseDimensions.Dimensionless, derive: static context => context.Relative.Value);
    // Magnitude-relative differencing step FRACTION (E-B11): a numeric probe — a central-difference Jacobian, a
    // distortion stencil — multiplies its OWN magnitude floor by this value and hands the product to the stencil
    // as its step. The anchor is the cube root of machine epsilon, the truncation/roundoff balance point for a
    // first-derivative central difference; the fraction is numeric rather than model-scaled, so the anchor reads
    // directly like the residual group and a project tunes it through `Context.Override` on this lane alone.
    public static readonly ToleranceLane Probe = new(key: "probe", band: Band.Ratio, dimension: UnitsNet.BaseDimensions.Dimensionless, derive: static _ => EpsilonPolicy.CbrtEpsilon);

    // --- [RESIDUAL]
    // Solver residuals floor on the NUMERIC anchor, never the model percent: an iterate asked to converge below
    // `SqrtEpsilon` never terminates, and a project tuning its solver passes `Context.Override` on the lane.
    public static readonly ToleranceLane Convergence = new(key: "convergence", band: Band.Residual, dimension: UnitsNet.BaseDimensions.Dimensionless, derive: static _ => EpsilonPolicy.SqrtEpsilon);
    public static readonly ToleranceLane Kkt = new(key: "kkt", band: Band.Residual, dimension: UnitsNet.BaseDimensions.Dimensionless, derive: static _ => EpsilonPolicy.SqrtEpsilon);
    public static readonly ToleranceLane Step = new(key: "step", band: Band.Residual, dimension: UnitsNet.BaseDimensions.Dimensionless, derive: static _ => EpsilonPolicy.SqrtEpsilon);
    public static readonly ToleranceLane Residual = new(key: "residual", band: Band.Residual, dimension: UnitsNet.BaseDimensions.Dimensionless, derive: static _ => EpsilonPolicy.SqrtEpsilon);
    public static readonly ToleranceLane Svd = new(key: "svd", band: Band.Residual, dimension: UnitsNet.BaseDimensions.Dimensionless, derive: static _ => EpsilonPolicy.SqrtEpsilon);
    public static readonly ToleranceLane Krylov = new(key: "krylov", band: Band.Residual, dimension: UnitsNet.BaseDimensions.Dimensionless, derive: static _ => EpsilonPolicy.SqrtEpsilon);
    public static readonly ToleranceLane Conservation = new(key: "conservation", band: Band.Residual, dimension: UnitsNet.BaseDimensions.Dimensionless, derive: static context => context.Relative.Value * context.Relative.Value);
    // Frame/transform identity election (E-B11): is a derived rotation degenerately zero, a scale degenerately
    // unity, an offset degenerately absent. The anchor is the DEGENERACY floor `ZeroTolerance` (2^-32), not the
    // solver anchor: an identity residual is ulp noise off exact reads (a unit-factor product, an `Atan2` of
    // declared direction cosines, ~1e-15), while the smallest genuine frame delta a survey grid declares sits
    // orders above — and `Band.Residual`'s open `SeamUlp` floor refuses the bare 1e-12 a page literal would seed,
    // so the anchor-lawful admissible derivation is the one that also carries VALID default evidence.
    public static readonly ToleranceLane Identity = new(key: "identity", band: Band.Residual, dimension: UnitsNet.BaseDimensions.Dimensionless, derive: static _ => EpsilonPolicy.ZeroTolerance);
    public static readonly ToleranceLane Duplicate = new(key: "duplicate", band: Band.Residual, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Relative.Value);
    public static readonly ToleranceLane Joint = new(key: "joint", band: Band.Residual, dimension: UnitsNet.Length.BaseDimensions, derive: static context => context.Relative.Value);

    // --- [DEVICE]
    // No model anchor reaches device space, so the host boundary seeds the real pitch through `Context.Override`.
    // 0.0 sits below the closed `DeviceQuantum` floor, so an un-overridden device lane carries REFUSING evidence by
    // construction — the band's own floor value would admit and hand back a pixel radius nobody chose.
    public static readonly ToleranceLane Guide = new(key: "guide", band: Band.Device, dimension: UnitsNet.BaseDimensions.Dimensionless, derive: static _ => 0.0);
    public static readonly ToleranceLane Pixel = new(key: "pixel", band: Band.Device, dimension: UnitsNet.BaseDimensions.Dimensionless, derive: static _ => 0.0);
    public static readonly ToleranceLane Hit = new(key: "hit", band: Band.Device, dimension: UnitsNet.BaseDimensions.Dimensionless, derive: static _ => 0.0);
    public static readonly ToleranceLane Annotation = new(key: "annotation", band: Band.Device, dimension: UnitsNet.BaseDimensions.Dimensionless, derive: static _ => 0.0);

    public Band Band { get; }

    public UnitsNet.BaseDimensions Dimension { get; }
    [UseDelegateFromConstructor] public partial double Derive(Context context);
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct Tolerance(ToleranceLane Lane, double Value) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(Lane.Band.Admits(value: Value));

    // Each lane's band IS the guard, so admission keeps one body for the whole vocabulary where the deleted triad
    // spelled three. The refusal is `OutOfRange` — the one scalar-range fault — carrying the lane key as the label
    // so a rejection names WHICH gate refused, which a per-type validation error could not.
    public static Fin<Tolerance> Of(ToleranceLane lane, double value, Op key) =>
        lane.Band.Admits(value: value)
            ? Fin.Succ(value: new Tolerance(Lane: lane, Value: value))
            : Fin.Fail<Tolerance>(error: new KernelFault.OutOfRange(
                Label: lane.Key,
                Scalar: value,
                Requirement: lane.Band.Refuse(label: lane.Key, value: value).Message,
                Key: Some(key)));
}
```

## [03]-[MODEL_CONTEXT]

- Owner: `ModelUnit` is the admitted unit regime — defined `UnitSystem`, positive finite meters per unit, required custom name, and the `BaseDimensions` signature the branch federates unit identity on; `Context` binds one `ModelUnit` to the admitted root triad and the lane overrides layered over it.
- Entry: the `Context.Of` family accepts scalar tolerances with `UnitSystem` or `LengthUnit` and derives defaults from either unit carrier, and `Context.Canonical` is the ONE total arm — the millimetre bundle over kernel-owned anchors alone, accessor-backed so its proof runs at first read and a refusal throws at type init (the branch's own registry-proof idiom), which is what a consumer holding no scalars composes instead of an argued unreachable throw; `For(lane)` is the ONE tolerance read; `Override(lane, value, unit)` is the ONE override ingress; `ScaleTo(Context)` divides the admitted meters-per-unit values after admitting the target. `ModelUnit.Convert(value, from, to, key)` is the ONE dynamic-conversion seam onto the UnitsNet vocabulary (guarded `UnitConverter.TryConvert`, typed refusal on an unregistered pair) and `ModelUnit.Converter<TQuantity>(from, to, key)` its hot-path row resolving one delegate per pair onto that same rail.
- Cases: `UnitSystem` ingress admits defined built-in rows; `LengthUnit` ingress admits built-in and custom rows, preserving custom name and scale; incomplete `CustomUnits`, `Unset`, `None`, and undefined ordinals fail before context construction.
- Law: `Override` is where `Convert` earns its consumer. Authoring surfaces publish a lane value in THEIR OWN unit — an ISO 286 grade in micrometres, a shop gate in millimetres — and admission converts into model units through the units registry, gated on `lane.Dimension.Equals(Unit.Dimension)` so a dimensionless lane takes its value raw and a length lane converts. That gate is the branch ruling's federation point made READABLE: both `Dimension` columns have a reader, and the alternative is every consumer hand-multiplying a scale factor beside a quantity enum. `Converter<TQuantity>` serves the per-sample projection Fabrication's GD&T stackup runs, where the registry probe must leave the loop.
- Law: conversion resolves through METRES and the admitted `MetersPerUnit`, never a Rhino-to-UnitsNet unit roster. UnitsNet answers "how many metres is this", the admitted scale answers "how many model units is a metre", and no hand-kept correspondence between two unit vocabularies exists to drift.
- Auto: `Fractional` (the arc-length tolerance feeding `Curve.GetLength`/`NormalizedLengthParameters`) reads `For(ToleranceLane.Fraction)`, so the zero-as-absence ternary this member used to carry dies at the lane's band guard — `Band.Ratio` decides whether zero is admissible once, at admission, instead of every read re-deciding what a stored zero meant.
- Law: `Of(RhinoDoc?)` is the document-coupled boundary adapter, projecting the document tolerances and units so custom scale and name survive unchanged; it seeds exactly the three ROOT lanes and no override.
- Law: `Canonical` is total because it admits NO caller value — every input is a kernel-owned anchor already band-proved, so the `Validation` its `Of` sibling returns has nothing left to report. Callers supplying any value of their own take `Of` and rail; a caller wanting a canonical bundle with one axis moved takes `Canonical.Override(lane, …)`. Re-minting a general unit-named factory over caller arguments (the deleted `Context.Millimeters()`) is the refused form.
- Law: `Build` accumulates. Three scalars and the unit admit INDEPENDENTLY, so a caller with two bad axes learns both — the applicative `.Apply` fan-in, never a bind chain that reports the first defect and hides the rest. `Default` sequences its unit-scale chain on `Fin` because those steps genuinely depend on each other, and crosses to `Validation` exactly once at the fan-in.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` rows), LanguageExt.Core (`Validation`, `Fin`, applicative `Apply`, `HashMap`), `Numerics/atoms` (`Band`, `EpsilonPolicy`), RhinoCommon (`LengthUnit`, `UnitSystem`, `RhinoDoc`, `RhinoMath` host defaults, `Intersection`), UnitsNet (`UnitConverter`, `ConversionFunction`, `IQuantity`, `BaseDimensions`).
- Growth: a new model-space fact (a grid-resolution policy, a document epoch) is one validated slot on the scalar floor, inherited by every derived factory; a new GATE is a `ToleranceLane` row and touches nothing here.
- Boundary: `Context` threads explicitly — a parameter on synchronous rails, inside `Env` on `Eff` pipelines (`rails.md` Op law), never a global default; `Analyze.From`/`Analyze.In` (`Analysis/query.md`) forward over the `Of` family, `Env` carrying the constructed `Context`. `Absolute`/`Relative`/`Angle` survive as stored accessors returning `Tolerance`, so every `.Value` read across the kernel and the host plane compiles unchanged while the three deleted value-object TYPES disappear from every signature.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Numerics;
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
    // Model units federate the branch here: a model unit IS a length regime, and an override's lane dimension compares against this signature before any conversion runs.
    public UnitsNet.BaseDimensions Dimension => UnitsNet.Length.BaseDimensions;

    public static Fin<ModelUnit> Of(UnitSystem value, Op key) => value switch {
        var unknown when !Enum.IsDefined(value: unknown) =>
            Fin.Fail<ModelUnit>(error: new KernelFault.InvalidUnitSystem(Units: unknown, Requirement: "must be a defined unit system")),
        UnitSystem.Unset or UnitSystem.None =>
            Fin.Fail<ModelUnit>(error: new KernelFault.InvalidUnitSystem(Units: value, Requirement: "must be a model unit system")),
        UnitSystem.CustomUnits =>
            Fin.Fail<ModelUnit>(error: new KernelFault.InvalidUnitSystem(Units: value, Requirement: "must carry custom name and scale")),
        _ => key.Catch(() => Of(value: LengthUnit.FromKnownUnitSystem(knownUnitSystem: value), key: key)),
    };

    public static Fin<ModelUnit> Of(LengthUnit value, Op key) => key.Catch(() => {
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
                : Fin.Fail<ModelUnit>(error: new KernelFault.InvalidUnitSystem(
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
    // `In` is the ONE foreign-unit ingress: UnitsNet answers how many METRES the value is and the admitted scale answers
    // how many model units a metre is, so no Rhino-to-UnitsNet unit correspondence exists to drift out of step.
    internal Fin<double> In(double value, Enum unit, Op key) =>
        Convert(value: value, from: unit, to: UnitsNet.Units.LengthUnit.Meter, key: key)
            .Map(metres => metres / MetersPerUnit);

    // `Convert` refuses an unregistered or non-finite pair TYPED instead of throwing; a consumer hand-multiplying a factor beside a quantity enum is the deleted form.
    [BoundaryAdapter]
    public static Fin<double> Convert(double value, Enum from, Enum to, Op? key = null) =>
        UnitsNet.UnitConverter.TryConvert(value, from, to, out double converted) && double.IsFinite(d: converted)
            ? Fin.Succ(value: converted)
            : Fin.Fail<double>(error: key.OrDefault().InvalidInput());
    // `Converter<TQuantity>` holds the hot path: ONE registry probe resolves the (from, to) delegate and a per-sample projection calls it,
    // so the dictionary lookup leaves the loop. The probe is the NON-THROWING twin — `GetConversionFunction` raises
    // on an unregistered pair, and a raise funnelled back onto the rail is exception-style control flow where the
    // registry already answers the same question as a verdict. The registry stores every conversion as the TYPELESS
    // `ConversionFunction` (`IQuantity -> IQuantity`), so the projection re-narrows each result and the per-call
    // cost is one boxed crossing — the delegate identity, not this row, owns that shape.
    [BoundaryAdapter]
    public static Fin<Func<TQuantity, TQuantity>> Converter<TQuantity>(Enum from, Enum to, Op? key = null) where TQuantity : UnitsNet.IQuantity =>
        UnitsNet.UnitConverter.Default.TryGetConversionFunction<TQuantity>(from, to, out UnitsNet.ConversionFunction? conversion)
            ? Fin.Succ<Func<TQuantity, TQuantity>>(value: quantity => (TQuantity)conversion(quantity))
            : Fin.Fail<Func<TQuantity, TQuantity>>(error: key.OrDefault().InvalidInput());
}

public sealed record Context {
    private static readonly Op Key = Op.Of(name: nameof(Context));

    private Context(Tolerance absolute, Tolerance relative, Tolerance angle, ModelUnit unit) {
        Absolute = absolute;
        Relative = relative;
        Angle = angle;
        Unit = unit;
    }

    public static Validation<Error, Context> Of(double absolute, double relative, double angle, UnitSystem units) =>
        Build(absolute: absolute, relative: relative, angle: angle, unit: ModelUnit.Of(value: units, key: Key));

    public static Validation<Error, Context> Of(double absolute, double relative, double angle, LengthUnit units) =>
        Build(absolute: absolute, relative: relative, angle: angle, unit: ModelUnit.Of(value: units, key: Key));

    public static Validation<Error, Context> Of(UnitSystem units) =>
        Default(unit: ModelUnit.Of(value: units, key: Key));

    public static Validation<Error, Context> Of(LengthUnit units) =>
        Default(unit: ModelUnit.Of(value: units, key: Key));

    // THE total arm (E-M22): every input is kernel-owned and band-proved — the millimetre `ModelUnit`, the host
    // default distance and angle, and `DefaultRelative` — so no CALLER value can refuse it and the admission has
    // nothing left to report. Accessor-backed `Lazy` so the proof runs at first read against a filled roster and
    // a refusal throws at type init, which is the branch's own enforcement idiom (`FaultBand.Code`, `Disjoint`,
    // `generated identity admission`) rather than rail control flow. This is the ONE exemption from the `Validation`
    // construction law and it exists so a consumer holding no scalars stops writing an argued unreachable throw:
    // a caller supplying ANY value of its own takes `Of` and rails. The deleted `Context.Millimeters()` was a
    // different shape — an arbitrary-argument factory wearing a unit name — and does not return.
    public static Context Canonical => Whole.Value;
    private static readonly Lazy<Context> Whole = new(
        static () => Of(units: UnitSystem.Millimeters).As().ThrowIfFail(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    [BoundaryAdapter]
    public static Validation<Error, Context> Of(RhinoDoc? doc) =>
        Optional(doc).ToValidation<Error>(Fail: new KernelFault.MissingContext(Key: Key))
            .Bind(static candidate => Of(
                absolute: candidate.ModelAbsoluteTolerance,
                relative: candidate.ModelRelativeTolerance,
                angle: candidate.ModelAngleToleranceRadians,
                units: candidate.ModelUnits));

    public Tolerance Absolute { get; }
    public Tolerance Relative { get; }
    public Tolerance Angle { get; }
    public ModelUnit Unit { get; }
    public UnitSystem Units => Unit.System;
    public HashMap<ToleranceLane, Tolerance> Overrides { get; init; } = HashMap<ToleranceLane, Tolerance>.Empty;

    // THE tolerance read. Overrides first, the lane's own derivation second — so a project that publishes a gate
    // moves every consumer of that lane at once, and a consumer that names no lane cannot reach a tolerance at all.
    // Default mints carry their band VERDICT on `IsValid` — `Of` is the refusing ingress, `For` the evidence-carrying
    // read — so a derivation its own band refuses hands back refusing evidence rather than silent admission.
    public Tolerance For(ToleranceLane lane) =>
        Overrides.Find(key: lane).IfNone(() => new Tolerance(Lane: lane, Value: lane.Derive(context: this)));

    // `Override` is the ingress: a dimensionless lane takes its value raw; a dimensioned lane converts into model units
    // through the ONE units seam, gated on the lane's own signature against the model unit's.
    [BoundaryAdapter]
    public Fin<Context> Override(ToleranceLane lane, double value, Enum unit, Op? key = null) {
        Op op = key.OrDefault();
        Context self = this;
        return from converted in lane.Dimension.Equals(Unit.Dimension)
                   ? Unit.In(value: value, unit: unit, key: op)
                   : Fin.Succ(value: value)
               from admitted in Tolerance.Of(lane: lane, value: converted, key: op)
               select self with { Overrides = self.Overrides.AddOrUpdate(key: lane, value: admitted) };
    }

    public double Fractional => For(lane: ToleranceLane.Fraction).Value;

    public Fin<double> ScaleTo(Context? target) {
        Op op = Op.Of(name: nameof(ScaleTo));
        return Optional(target).ToFin(Fail: op.MissingContext())
            .Bind(destination => Unit.ScaleTo(target: destination.Unit, key: op));
    }

    private static Validation<Error, Context> Build(double absolute, double relative, double angle, Fin<ModelUnit> unit) =>
        (Tolerance.Of(lane: ToleranceLane.Distance, value: absolute, key: Key).ToValidation(),
         Tolerance.Of(lane: ToleranceLane.Relative, value: relative, key: Key).ToValidation(),
         Tolerance.Of(lane: ToleranceLane.Angle, value: angle, key: Key).ToValidation(),
         unit.ToValidation())

            .Apply(static (a, r, n, u) => new Context(absolute: a, relative: r, angle: n, unit: u))
            .As();

    // Rhino's factory `ModelRelativeTolerance` value. RhinoCommon publishes no member for it, and the sense is the
    // MODEL PERCENT the branch ruling binds — `EpsilonPolicy.SqrtEpsilon` is the numeric anchor the residual lanes
    // derive directly, and seating it here drove every `Absolute * Relative` lane below its own band's floor.
    private const double DefaultRelative = 0.01;

    // `Default` rides the HOST distance default through the admitted unit scale, while the relative gate and the
    // angle read their named owners directly — a bare per-module literal standing for either is unreplayable across
    // operators. The unit-scale chain sequences on `Fin` because each step depends on the last; the crossing to
    // `Validation` happens once, at the independent fan-in `Build` owns.
    private static Validation<Error, Context> Default(Fin<ModelUnit> unit) =>
        (from target in unit
         from millimeters in ModelUnit.Of(value: UnitSystem.Millimeters, key: Key)
         from scale in millimeters.ScaleTo(target: target, key: Key)
         select (Unit: target, Scale: scale))
            .ToValidation()
            .Bind(admitted => Build(
                absolute: RhinoMath.DefaultDistanceToleranceMillimeters * admitted.Scale,
                relative: DefaultRelative,
                angle: RhinoMath.DefaultAngleTolerance,
                unit: Fin.Succ(value: admitted.Unit)));

}
```

## [04]-[DENSITY_BAR]

One lane vocabulary, one admitted unit regime, and one context factory family own every model-space ingress.

| [INDEX] | [CONCERN]       | [OWNER]         | [SHAPE]                                            |
| :-----: | :-------------- | :-------------- | :------------------------------------------------- |
|  [01]   | tolerance gates | `ToleranceLane` | keyed rows carrying band, dimension, derivation    |
|  [02]   | admitted scalar | `Tolerance`     | band-gated `(lane, value)` pair with evidence fold |
|  [03]   | unit regime     | `ModelUnit`     | built-in/custom identity, metric scale, dimension  |
|  [04]   | model context   | `Context`       | polymorphic factory, lane read, override ingress   |

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
