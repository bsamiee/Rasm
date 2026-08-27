# 1. Delete the erased cone projection vocabulary
**From — libs/dotnet/Rasm/.planning/Parametric/projections.md:L162-L176**
```csharp
[SmartEnum<int>]
public sealed partial class ConeProjection {
    public static readonly ConeProjection HalfAngle = new(key: 0, sample: static (cone, _) => Fin.Succ<object>(cone.HalfAngle));
    public static readonly ConeProjection SolidAngle = new(key: 1, sample: static (cone, _) => Fin.Succ<object>(cone.SolidAngle));
    public static readonly ConeProjection Axis = new(key: 2, sample: static (cone, _) => Fin.Succ<object>(cone.Axis));
    public static readonly ConeProjection Apex = new(key: 3, sample: static (cone, _) => Fin.Succ<object>(cone.Apex));
    public static readonly ConeProjection Spread = new(key: 4, sample: static (cone, key) =>
        cone.HalfAngle.Value < Math.PI / 2.0
            ? Fin.Succ<object>(Math.Tan(cone.HalfAngle.Value))
            : Fin.Fail<object>(key.InvalidResult()));
    [UseDelegateFromConstructor] private partial Fin<object> Sample(VectorCone cone, Op key);
    internal Fin<TOut> Project<TOut>(VectorCone cone, Op key) =>
        Sample(cone: cone, key: key).Bind(raw =>
            ResultProjection.Raw<TOut>(raw: raw, context: Option<Context>.None, key: key, owner: typeof(ConeProjection), admits: CapabilitySet<RawAdmission>.None));
}
```
**To**
```csharp
// ConeProjection DELETED
```
**Why**
Four rows erase properties already typed on `VectorCone`; the fifth is one derived scalar. The roster adds keys, delegates, `object`, generic casting, and failure plumbing without a genuine modality.
**Change**
Delete `ConeProjection`. Add `VectorCone.Spread(Op? key = null) : Fin<double>` beside `SolidAngle`, retaining the half-angle guard and tangent expression; callers read the other four members directly.
**Ripples**
`libs/dotnet/Rasm/.planning/Numerics/atoms.md::VectorCone` gains `Spread`; `libs/dotnet/Rasm.Rhino/.planning/Objects/lights.md` replaces its `ConeProjection` dependency with direct `VectorCone.HalfAngle`/`SolidAngle` reads and `Spread` where beam radius is required.
**Delta**
LOC -15; types -1; members -7

# 2. Replace the bounce table and loop with the closed piecewise law
**From — libs/dotnet/Rasm/.planning/Parametric/projections.md:L325-L335**
```csharp
private static double Bounce(double t) => 1.0 - BounceTail(t: 1.0 - t);
private static readonly (double Edge, double Centre, double Lift)[] BounceArcs = [
    (1.0 / 2.75, 0.0, 0.0),
    (2.0 / 2.75, 1.5 / 2.75, 0.75),
    (2.5 / 2.75, 2.25 / 2.75, 0.9375),
    (double.PositiveInfinity, 2.625 / 2.75, 0.984375),
];
private static double BounceTail(double t) {
    int at = 0;
    while (t >= BounceArcs[at].Edge) { at++; }
    return (7.5625 * (t - BounceArcs[at].Centre) * (t - BounceArcs[at].Centre)) + BounceArcs[at].Lift;
}
```
**To**
```csharp
private static double Bounce(double t) => 1.0 - BounceTail(t: 1.0 - t);
private static double BounceTail(double t) => t switch {
    < 1.0 / 2.75 => 7.5625 * t * t,
    < 2.0 / 2.75 => 7.5625 * (t - (1.5 / 2.75)) * (t - (1.5 / 2.75)) + 0.75,
    < 2.5 / 2.75 => 7.5625 * (t - (2.25 / 2.75)) * (t - (2.25 / 2.75)) + 0.9375,
    _ => 7.5625 * (t - (2.625 / 2.75)) * (t - (2.625 / 2.75)) + 0.984375,
};
```
**Why**
The four Penner intervals are the law, not mutable runtime data. A total relational switch deletes the array allocation, index state, and bounds-sensitive loop while preserving edge behavior.
**Change**
State the four arcs directly as a switch expression and delete `BounceArcs`.
**Delta**
LOC -4; types +0; members -1

# 3. Delegate cubic-Bezier inversion to MathNet Brent
**From — libs/dotnet/Rasm/.planning/Parametric/projections.md:L357-L393**
```csharp
public readonly record struct BezierEase(double X1, double Y1, double X2, double Y2, Dimension Probes) {
    public static readonly Dimension ProbeBudget = Dimension.Create(value: 64);

    public static Fin<BezierEase> Of(double x1, double y1, double x2, double y2, Op? key = null, Option<Dimension> probes = default) {
        Op op = key.OrDefault();
        return (op.Finite(value: x1).ToValidation(), op.Finite(value: y1).ToValidation(),
                op.Finite(value: x2).ToValidation(), op.Finite(value: y2).ToValidation())
            .Apply((a, b, c, d) => new BezierEase(
                X1: Math.Clamp(value: a, min: 0.0, max: 1.0), Y1: b,
                X2: Math.Clamp(value: c, min: 0.0, max: 1.0), Y2: d,
                Probes: probes.IfNone(noneValue: ProbeBudget)))
            .As().ToFin();
    }

    public Fin<double> Evaluate(UnitInterval t, Op? key = null) {
        Op op = key.OrDefault();
        (double x1, double x2, double y1, double y2, double target) = (X1, X2, Y1, Y2, t.Value);
        return Range(0, Probes.Value).FoldUntil(
                initialState: (U: target, Lo: 0.0, Hi: 1.0, Settled: Option<double>.None),
                f: (state, _) => {
                    double x = Axis(a: x1, b: x2, u: state.U) - target;
                    if (Math.Abs(value: x) <= EpsilonPolicy.SqrtEpsilon) { return (state.U, state.Lo, state.Hi, Some(state.U)); }
                    (double lo, double hi) = x > 0.0 ? (state.Lo, state.U) : (state.U, state.Hi);
                    double slope = AxisSlope(a: x1, b: x2, u: state.U);
                    return (slope > EpsilonPolicy.ZeroTolerance ? Math.Clamp(value: state.U - (x / slope), min: lo, max: hi) : (lo + hi) / 2.0,
                        lo, hi, Option<double>.None);
                },
                predicate: static state => state.Settled.IsSome)
            .Settled
            .Map(u => Axis(a: y1, b: y2, u: u))
            .ToFin(Fail: op.InvalidResult());
    }
    private static double Axis(double a, double b, double u) =>
        ((((1.0 - (3.0 * b) + (3.0 * a)) * u) + ((3.0 * b) - (6.0 * a))) * u + (3.0 * a)) * u;
    private static double AxisSlope(double a, double b, double u) =>
        (3.0 * (1.0 - (3.0 * b) + (3.0 * a)) * u * u) + (2.0 * ((3.0 * b) - (6.0 * a)) * u) + (3.0 * a);
}
```
**To**
```csharp
public readonly record struct BezierEase(double X1, double Y1, double X2, double Y2) {
    private static readonly Dimension Iterations = Dimension.Create(value: 64);

    public static Fin<BezierEase> Of(double x1, double y1, double x2, double y2, Op? key = null) {
        Op op = key.OrDefault();
        return (op.Finite(x1).ToValidation(), op.Finite(y1).ToValidation(),
                op.Finite(x2).ToValidation(), op.Finite(y2).ToValidation())
            .Apply(static (a, b, c, d) => new BezierEase(
                double.Clamp(a, 0.0, 1.0), b, double.Clamp(c, 0.0, 1.0), d))
            .As().ToFin();
    }

    public Fin<double> Evaluate(UnitInterval t, Op? key = null) {
        Op op = key.OrDefault();
        return MathNet.Numerics.RootFinding.Brent.TryFindRoot(
            u => Axis(X1, X2, u) - t.Value,
            0.0, 1.0, EpsilonPolicy.SqrtEpsilon, Iterations.Value, out double u)
                ? op.Finite(Axis(Y1, Y2, u))
                : Fin.Fail<double>(op.InvalidResult());
    }

    private static double Axis(double a, double b, double u) =>
        ((((1.0 - (3.0 * b) + (3.0 * a)) * u) + ((3.0 * b) - (6.0 * a))) * u + (3.0 * a)) * u;
}
```
**Why**
MathNet owns bounded non-throwing bracketed root finding. The local Newton/bisection state machine duplicates it, exposes an implementation budget in value identity, and retains a derivative used nowhere else.
**Change**
Keep applicative coordinate admission, make the iteration ceiling owner policy, delegate the guaranteed bracket to `Brent.TryFindRoot`, and lift its boolean verdict to `Fin<double>`.
**Delta**
LOC -14; types +0; members -2

# 4. Delete the payloadless cycle vocabularies
**From — libs/dotnet/Rasm/.planning/Parametric/projections.md:L339-L353**
```csharp
[SmartEnum<int>]
public sealed partial class CycleShape {
    public static readonly CycleShape Repeat = new(key: 0, posture: static _ => CyclePosture.Forward);
    public static readonly CycleShape Yoyo = new(key: 1, posture: static iteration => (iteration % 2L) == 1L ? CyclePosture.Reversed : CyclePosture.Forward);
    [UseDelegateFromConstructor] public partial CyclePosture Posture(long iteration);
}

[SmartEnum<int>]
public sealed partial class CyclePosture {
    public static readonly CyclePosture Forward = new(key: 0, continues: true, place: static local => local);
    public static readonly CyclePosture Reversed = new(key: 1, continues: true, place: static local => 1.0 - local);
    public static readonly CyclePosture Completed = new(key: 2, continues: false, place: static local => local);
    public bool Continues { get; }
    [UseDelegateFromConstructor] public partial double Place(double local);
}
```
**To**
```csharp
// CycleShape and CyclePosture DELETED
```
**Why**
`CycleShape` is a payloadless two-case family, while `CyclePosture` stores direction and continuation already determined by alternation and completion. Neither carries independent evidence.
**Change**
Represent repeat versus yoyo as the `Reverses` boolean column on `CyclePlan`; derive placement and continuation in that owner.
**Delta**
LOC -15; types -2; members -8

# 5. Fold phase state into the cycle plan
**From — libs/dotnet/Rasm/.planning/Parametric/projections.md:L395-L423**
```csharp

[StructLayout(LayoutKind.Auto)]
public readonly record struct CyclePhase(long Iteration, UnitInterval Local, CyclePosture Posture) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(Iteration >= 0L, ValidityClaim.UnitInterval(value: Local.Value), Posture is not null);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CyclePlan(Option<int> Count, CycleShape Shape) {
    public static Fin<CyclePlan> Of(Option<int> count, CycleShape shape, Op? key = null) {
        Op op = key.OrDefault();
        return (op.Need(value: shape).ToValidation(),
                count.TraverseM(value => guard(value >= 1, op.InvalidInput()).ToFin().Map(_ => value)).As().ToValidation())
            .Apply(static (traversal, bounded) => new CyclePlan(Count: bounded, Shape: traversal)).As().ToFin();
    }
    public CyclePosture Terminal => Count.Match(Some: bounded => Shape.Posture(iteration: bounded - 1L), None: () => CyclePosture.Forward);
    public Fin<CyclePhase> Phase(Duration elapsed, Duration period, Op key) {
        CyclePlan plan = this;
        return from time in key.Finite(value: elapsed.TotalSeconds).Bind(value => guard(value >= 0.0, key.InvalidInput()).ToFin().Map(_ => value))
               from span in key.Positive(value: period.TotalSeconds)
               from progress in key.AcceptValue(value: time / span)
               let completed = plan.Count.Filter(bounded => progress >= bounded)
               from iteration in completed.Match(
                   Some: bounded => Fin.Succ((long)bounded - 1L),
                   None: () => guard(Math.Floor(d: progress) < long.MaxValue, key.InvalidResult()).ToFin()
                       .Map(_ => checked((long)Math.Floor(d: progress))))
               let facing = plan.Shape.Posture(iteration: iteration)
               from local in key.AcceptValidated<UnitInterval>(candidate: facing.Place(local: completed.IsSome ? 1.0 : progress - iteration))
               select new CyclePhase(Iteration: iteration, Local: local, Posture: completed.IsSome ? CyclePosture.Completed : facing);
    }
}
```
**To**
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct CyclePlan(Option<int> Count, bool Reverses) {
    public static Fin<CyclePlan> Of(Option<int> count, bool reverses, Op? key = null) {
        Op op = key.OrDefault();
        return count.TraverseM(value => guard(value >= 1, op.InvalidInput()).ToFin().Map(_ => value)).As()
            .Map(bounded => new CyclePlan(Count: bounded, Reverses: reverses));
    }

    public UnitInterval Terminal => UnitInterval.Create(
        Reverses && Count.Map(static bounded => (bounded & 1) == 0).IfNone(false) ? 0.0 : 1.0);

    public Fin<(UnitInterval Value, bool Continues)> Phase(Duration elapsed, Duration period, Op key) {
        CyclePlan plan = this;
        return from time in key.Finite(elapsed.TotalSeconds)
                   .Bind(value => guard(value >= 0.0, key.InvalidInput()).ToFin().Map(_ => value))
               from span in key.Positive(period.TotalSeconds)
               from progress in key.Finite(time / span)
               let completed = plan.Count.Filter(bounded => progress >= bounded)
               from iteration in completed.Match(
                   Some: bounded => Fin.Succ((long)bounded - 1L),
                   None: () => guard(double.Floor(progress) < long.MaxValue, key.InvalidResult()).ToFin()
                       .Map(_ => checked((long)double.Floor(progress))))
               let local = completed.IsSome ? 1.0 : progress - iteration
               let placed = plan.Reverses && (iteration & 1L) == 1L ? 1.0 - local : local
               from value in key.AcceptValidated<UnitInterval>(candidate: placed)
               select (Value: value, Continues: completed.IsNone);
    }
}
```
**Why**
`CyclePhase` packages iteration and posture values its sole consumer immediately discards. The plan can return the sampled unit value and continuation verdict directly.
**Change**
Move the `Reverses` column onto `CyclePlan`, derive the terminal value from count parity, and return `(Value, Continues)` from `Phase`.
**Ripples**
`MotionDrive.Admit` passes `row.Cycle.Reverses`; `MotionDrive.Step` reads `phase.Value`/`phase.Continues`; `libs/dotnet/Rasm.Rhino/.planning/Viewport/motion.md::MotionPump.Collapsed` reads `plan.Cycle.Terminal` and removes `CyclePosture` construction.
**Delta**
LOC 0; types -1; members -3

# 6. Make decay admission generator-owned
**From — libs/dotnet/Rasm/.planning/Parametric/projections.md:L561-L594**
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct DecayShape(double Retention) : IValidityEvidence {
    internal static bool Admits(double retention) =>
        Band.Ratio.Admits(value: retention) && Band.Fractional.Admits(value: retention);

    public bool IsValid => Admits(retention: Retention);

    public double Rate => -Math.Log(x: Retention);

    public static Fin<DecayShape> Of(double retention, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in op.Finite(value: retention)
               from bounded in guard(Admits(retention: admitted), op.InvalidInput()).ToFin().Map(_ => admitted)
               select new DecayShape(Retention: bounded);
    }

    public Fin<double> Project(double velocity, Op key) =>
        from initial in key.Finite(value: velocity)
        select initial / Rate;

    public Fin<double> Advance(double origin, double velocity, Duration elapsed, Op key) {
        double rate = Rate;
        return (key.Finite(value: origin).ToValidation(),
                key.Finite(value: velocity).ToValidation(),
                key.Finite(value: elapsed.TotalSeconds).Bind(value => guard(value >= 0.0, key.InvalidInput()).ToFin().Map(_ => value)).ToValidation())
            .Apply((start, initial, time) => start + (initial * (1.0 - Math.Exp(d: -rate * time)) / rate)).As().ToFin();
    }

    public Fin<Duration> Settle(double velocity, PositiveMagnitude epsilon, Op key) =>
        from initial in key.Finite(value: velocity)
        let remaining = Math.Abs(value: initial) / Rate
        from seconds in key.Finite(value: Math.Max(val1: 0.0, val2: Math.Log(x: Math.Max(val1: remaining, val2: epsilon.Value) / epsilon.Value) / Rate))
        select Duration.FromSeconds(seconds);
}
```
**To**
```csharp
[ValueObject<double>(KeyMemberName = nameof(Retention), KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct DecayShape : IDisallowDefaultValue {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double retention) =>
        validationError = Band.Ratio.Guard(label: nameof(Retention), value: ref retention)
            ?? Band.Fractional.Guard(label: nameof(Retention), value: ref retention);

    public double Rate => -Math.Log(Retention);
    public Fin<double> Project(double velocity, Op key) => key.Finite(velocity).Map(initial => initial / Rate);

    public Fin<double> Advance(double origin, double velocity, Duration elapsed, Op key) {
        double rate = Rate;
        return (key.Finite(origin).ToValidation(), key.Finite(velocity).ToValidation(),
                key.Finite(elapsed.TotalSeconds)
                    .Bind(value => guard(value >= 0.0, key.InvalidInput()).ToFin().Map(_ => value)).ToValidation())
            .Apply((start, initial, time) => start + (initial * (1.0 - Math.Exp(-rate * time)) / rate)).As().ToFin();
    }

    public Fin<Duration> Settle(double velocity, PositiveMagnitude epsilon, Op key) =>
        from initial in key.Finite(velocity)
        let remaining = Math.Abs(initial) / Rate
        from seconds in key.Finite(Math.Max(0.0, Math.Log(Math.Max(remaining, epsilon.Value) / epsilon.Value) / Rate))
        select Duration.FromSeconds(seconds);
}
```
**Why**
The public record constructor bypasses its factory and forces downstream `IsValid` probes. A single-scalar Thinktecture value object owns construction, default refusal, equality, conversion, and the open-unit gate once.
**Change**
Use the existing `Band` guards in the generated validation hook and retain only the decay algebra; generated admission replaces `Admits`, `IsValid`, and `Of`.
**Ripples**
`libs/dotnet/Rasm.AppUi/.planning/Theme/motion.md::MotionDecay.Shape` lifts `DecayShape.Validate(Retention, null, out DecayShape?)` through the kernel admission bridge; `MotionScript.Glide` and `MotionDrive.Admit` delete their repeated `DecayShape.IsValid` guards.
**Delta**
LOC -9; types +0; authored members -3

# 7. Correct pace scaling against the refresh ceiling
**From — libs/dotnet/Rasm/.planning/Parametric/projections.md:L918-L923**
```csharp
public Fin<PaceBand> ScaleTo(PositiveMagnitude reference, Op? key = null) {
    double factor = Period.TotalSeconds / reference.Value;
    return key.OrDefault().AcceptValidated<PaceBand>(
        Validate(minimum: Minimum * factor, maximum: Maximum * factor, preferred: Preferred * factor, obj: out PaceBand? scaled),
        scaled);
}
```
**To**
```csharp
public Fin<PaceBand> ScaleTo(PositiveMagnitude ceiling, Op? key = null) {
    double factor = ceiling.Value / Maximum;
    return key.OrDefault().AcceptValidated<PaceBand>(
        Validate(Minimum * factor, Maximum * factor, Preferred * factor, out PaceBand? scaled), scaled);
}
```
**Why**
`reference` is a frames-per-second ceiling, but the current expression divides seconds per frame by that rate, producing a dimensionally invalid factor and near-zero frame rates. `ceiling / Maximum` preserves the band ratios and seats the requested ceiling exactly.
**Change**
Rename the parameter to its actual role and scale every rate by `ceiling / Maximum`.
**Delta**
LOC -1; types +0; members +0

# 8. Keep settle distance absolute across both readers
**From — libs/dotnet/Rasm/.planning/Parametric/projections.md:L926-L933**
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct SettleBand(double Position, double Velocity) : IValidityEvidence {
    public static SettleBand Perceptual { get; } = new(Position: EpsilonPolicy.SqrtEpsilon, Velocity: EpsilonPolicy.SqrtEpsilon);
    public bool Settles(SpringState state, double target) =>
        Math.Abs(value: state.Position - target) <= Position * Math.Max(val1: 1.0, val2: Math.Abs(value: target))
        && Math.Abs(value: state.Velocity) <= Velocity;
    public bool IsValid => ValidityClaim.All(ValidityClaim.Positive(value: Position), ValidityClaim.Positive(value: Velocity));
}
```
**To**
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct SettleBand(double Position, double Velocity) : IValidityEvidence {
    public static SettleBand Perceptual { get; } = new(Position: EpsilonPolicy.SqrtEpsilon, Velocity: EpsilonPolicy.SqrtEpsilon);
    public bool Settles(SpringState state, double target) =>
        Math.Abs(state.Position - target) <= Position && Math.Abs(state.Velocity) <= Velocity;
    public bool IsValid => ValidityClaim.All(ValidityClaim.Positive(Position), ValidityClaim.Positive(Velocity));
}
```
**Why**
`SpringShape.Settle` interprets `Position` as an absolute distance, and consumers declare it as unit progress or device pixels. Multiplying it by target magnitude makes the stop test disagree with the duration projection and turns a half-pixel band at target 1000 into a 500-pixel tolerance.
**Change**
Use the band's position and velocity values directly so both settle readers share one physical meaning.
**Delta**
LOC -1; types +0; members +0

# 9. Move and rename the accessibility vocabulary
**From — libs/dotnet/Rasm/.planning/Parametric/projections.md:L863-L872**
```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MotionConcession : ICapability<MotionConcession> {
    public static readonly MotionConcession ReduceMotion = new(key: "reduce-motion", rank: 0);
    public static readonly MotionConcession IncreaseContrast = new(key: "increase-contrast", rank: 1);
    public static readonly MotionConcession DifferentiateColour = new(key: "differentiate-colour", rank: 2);
    public static readonly MotionConcession ReduceTransparency = new(key: "reduce-transparency", rank: 3);
    public static readonly MotionConcession InvertColors = new(key: "invert-colors", rank: 4);
    public int Rank { get; }
}
```
**To**
```csharp
// MotionConcession DELETED
```
**Why**
Four rows are display accessibility settings rather than motion, and “concession” is a coined synonym. The current seat makes theme and platform consumers depend on the motion module for non-motion facts.
**Change**
Move the keyed roster to `libs/dotnet/Rasm/.planning/Interaction/platform.md` as `Accessibility : ICapability<Accessibility>`; retain the existing keys and ranks.
**Ripples**
Rename the type and import `Rasm.Interaction` in `libs/dotnet/Rasm.AppUi/.planning/Theme/tokens.md`, `libs/dotnet/Rasm.AppUi/.planning/Theme/motion.md`, `libs/dotnet/Rasm.Grasshopper/.planning/Platform/native.md`, and `libs/dotnet/Rasm.Rhino/.planning/Viewport/motion.md`; their capability sets, preference projections, and native probes retain the same rows.
**Delta**
LOC -10 in this module; types -1; members -6 in this module

# 10. Flatten motion samples to consumed evidence
**From — libs/dotnet/Rasm/.planning/Parametric/projections.md:L891-L897**
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MotionSample {
    private MotionSample() { }
    public sealed record Eased(double Value, CyclePosture Posture, MotionPosture Motion) : MotionSample;
    public sealed record Sprung(SpringState State, MotionPosture Motion) : MotionSample;
    public sealed record Glided(double Value, MotionPosture Motion) : MotionSample;
}
```
**To**
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct MotionSample(double Value, Option<double> Velocity);
```
**Why**
Every search-resolved consumer reads one scalar; spring retargeting alone additionally needs velocity. The union mirrors `MotionScript`, copies policy into each sample, and forces a case switch merely to recover the common value.
**Change**
Carry the scalar once and use `Some(velocity)` as spring-retarget evidence; eased and glided samples carry `None`.
**Ripples**
`libs/dotnet/Rasm.Rhino/.planning/Viewport/operations.md::Cameras.Progressed` reads `sample.Value`; `libs/dotnet/Rasm.Rhino/.planning/Viewport/motion.md::MotionPump.Collapsed`, `MotionDrive.Step`, and `MotionDrive.Retarget` construct or consume flat evidence. Grasshopper and Rhino apply closures keep their `MotionSample` parameter.
**Delta**
LOC -5; nested types -3; members -7

# 11. Delete the policy-copy posture wrapper
**From — libs/dotnet/Rasm/.planning/Parametric/projections.md:L935-L936**
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct MotionPosture(CapabilitySet<MotionConcession> Concessions, PaceBand Pace);
```
**To**
```csharp
// MotionPosture DELETED
```
**Why**
`MotionDrive` reads only reduced-motion membership; `PaceBand` is timer and gauge policy and never changes a sampled value. Copying both into each sample duplicates host state without evidence gain.
**Change**
Pass `CapabilitySet<Accessibility>` directly to `MotionDrive.Step`; retain pace on the timer, dispatch, and gauge owners that consume it.
**Ripples**
`libs/dotnet/Rasm.AppUi/.planning/Theme/motion.md::ReducedMotion.Posture` becomes a direct accessibility-set projection; `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/motion.md::CanvasPacer`, `libs/dotnet/Rasm.Grasshopper/.planning/Platform/layers.md::MotionAttachment`, `libs/dotnet/Rasm.Grasshopper/.planning/Platform/native.md`, and `libs/dotnet/Rasm.Rhino/.planning/Viewport/motion.md::MotionPump` pass or store the set and retain pace on existing clock objects.
**Delta**
LOC -2; types -1; members -2

# 12. Sample directly from accessibility and flat evidence
**From — libs/dotnet/Rasm/.planning/Parametric/projections.md:L963-L990**
```csharp
public static Fin<(MotionSample Sample, bool Continues)> Step(MotionScript script, MonotonicBeat beat, MotionPosture posture, Op? key = null) {
    Op op = key.OrDefault();
    return from evidence in guard(beat.IsValid && posture.Pace is not null, op.InvalidInput()).ToFin()
           from sampled in script.Switch(
               state: (Elapsed: Duration.FromTimeSpan(beat.Elapsed),
                       Collapsed: posture.Concessions.Admits(capability: MotionConcession.ReduceMotion),
                       Motion: posture,
                       Key: op),
               eased: static (state, row) => state.Collapsed
                   ? from stop in state.Key.AcceptValidated<UnitInterval>(candidate: row.Cycle.Terminal.Place(local: 1.0))
                     from value in state.Key.Finite(value: row.Curve.Evaluate(t: stop))
                     select ((MotionSample)new MotionSample.Eased(Value: value, Posture: CyclePosture.Completed, Motion: state.Motion), false)
                   : from phase in row.Cycle.Phase(elapsed: state.Elapsed, period: row.Period, key: state.Key)
                     from value in state.Key.Finite(value: row.Curve.Evaluate(t: phase.Local))
                     select ((MotionSample)new MotionSample.Eased(Value: value, Posture: phase.Posture, Motion: state.Motion), phase.Posture.Continues),
               sprung: static (state, row) => state.Collapsed
                   ? Fin.Succ(((MotionSample)new MotionSample.Sprung(State: new SpringState(Position: row.To, Velocity: 0.0), Motion: state.Motion), false))
                   : from live in row.Shape.Evaluate(origin: new SpringState(Position: row.From, Velocity: row.Velocity), target: row.To, elapsed: state.Elapsed, key: state.Key)
                     let settled = row.Band.Settles(state: live, target: row.To)
                     select ((MotionSample)new MotionSample.Sprung(
                         State: settled ? new SpringState(Position: row.To, Velocity: 0.0) : live,
                         Motion: state.Motion), !settled),
               glided: static (state, row) => state.Collapsed
                   ? from rest in row.Decay.Project(velocity: row.Velocity, key: state.Key)
                     select ((MotionSample)new MotionSample.Glided(Value: row.Origin + rest, Motion: state.Motion), false)
                   : from value in row.Decay.Advance(origin: row.Origin, velocity: row.Velocity, elapsed: state.Elapsed, key: state.Key)
                     select ((MotionSample)new MotionSample.Glided(Value: value, Motion: state.Motion), state.Elapsed < row.Bound))
           select sampled;
}
```
**To**
```csharp
public static Fin<(MotionSample Sample, bool Continues)> Step(
    MotionScript script, MonotonicBeat beat, CapabilitySet<Accessibility> accessibility, Op? key = null) {
    Op op = key.OrDefault();
    return from evidence in guard(beat.IsValid, op.InvalidInput()).ToFin()
           from sampled in script.Switch(
               state: (Elapsed: Duration.FromTimeSpan(beat.Elapsed),
                       Reduced: accessibility.Admits(Accessibility.ReduceMotion), Key: op),
               eased: static (state, row) => state.Reduced
                   ? state.Key.Finite(row.Curve.Evaluate(row.Cycle.Terminal))
                       .Map(value => (new MotionSample(value, None), false))
                   : from phase in row.Cycle.Phase(state.Elapsed, row.Period, state.Key)
                     from value in state.Key.Finite(row.Curve.Evaluate(phase.Value))
                     select (new MotionSample(value, None), phase.Continues),
               sprung: static (state, row) => state.Reduced
                   ? Fin.Succ((new MotionSample(row.To, Some(0.0)), false))
                   : from live in row.Shape.Evaluate(new SpringState(row.From, row.Velocity), row.To, state.Elapsed, state.Key)
                     let settled = row.Band.Settles(live, row.To)
                     let sample = settled ? new SpringState(row.To, 0.0) : live
                     select (new MotionSample(sample.Position, Some(sample.Velocity)), !settled),
               glided: static (state, row) => state.Reduced
                   ? row.Decay.Project(row.Velocity, state.Key)
                       .Map(rest => (new MotionSample(row.Origin + rest, None), false))
                   : row.Decay.Advance(row.Origin, row.Velocity, state.Elapsed, state.Key)
                       .Map(value => (new MotionSample(value, None), state.Elapsed < row.Bound)))
           select sampled;
}
```
**Why**
Pace is host policy, not sample input, and the sample union carries no consumed discriminant. Direct accessibility membership plus scalar/optional-velocity evidence preserves all useful capability with fewer types and branches.
**Change**
Import `Rasm.Interaction`, read reduced motion from the canonical set, use the collapsed cycle result, and emit flat samples.
**Ripples**
Update `libs/dotnet/Rasm.Grasshopper/.planning/Canvas/motion.md::CanvasPacer.Advance`, `libs/dotnet/Rasm.Grasshopper/.planning/Platform/layers.md::MotionAttachment.Tick`, and `libs/dotnet/Rasm.Rhino/.planning/Viewport/motion.md::MotionPump.Tick` to pass their accessibility set; remove copied posture arguments from terminal sample construction.
**Delta**
LOC -3; types +0; members +0

# 13. Retarget from optional velocity evidence
**From — libs/dotnet/Rasm/.planning/Parametric/projections.md:L993-L1004**
```csharp
public static Fin<MotionScript> Retarget(MotionScript script, MotionSample from, double to, Op? key = null) {
    Op op = key.OrDefault();
    return from target in op.Finite(value: to)
           from steered in script.Switch(
               state: (Sample: from, Target: target, Key: op),
               eased: static (state, row) => Fin.Fail<MotionScript>(state.Key.Unsupported(inputType: typeof(MotionScript.Eased), outputType: typeof(MotionScript.Sprung))),
               sprung: static (state, row) => state.Sample is MotionSample.Sprung live
                   ? Fin.Succ((MotionScript)new MotionScript.Sprung(
                       Shape: row.Shape, From: live.State.Position, To: state.Target, Velocity: live.State.Velocity, Band: row.Band))
                   : Fin.Fail<MotionScript>(state.Key.InvalidInput()),
               glided: static (state, row) => Fin.Fail<MotionScript>(state.Key.Unsupported(inputType: typeof(MotionScript.Glided), outputType: typeof(MotionScript.Sprung))))
           select steered;
}
```
**To**
```csharp
public static Fin<MotionScript> Retarget(MotionScript script, MotionSample from, double to, Op? key = null) {
    Op op = key.OrDefault();
    return from target in op.Finite(to)
           from steered in script.Switch(
               state: (Sample: from, Target: target, Key: op),
               eased: static (state, _) => Fin.Fail<MotionScript>(
                   state.Key.Unsupported(typeof(MotionScript.Eased), typeof(MotionScript.Sprung))),
               sprung: static (state, row) =>
                   from origin in state.Key.Finite(state.Sample.Value)
                   from velocity in state.Sample.Velocity.ToFin(state.Key.InvalidInput())
                       .Bind(value => state.Key.Finite(value))
                   select (MotionScript)new MotionScript.Sprung(
                       row.Shape, origin, state.Target, velocity, row.Band),
               glided: static (state, _) => Fin.Fail<MotionScript>(
                   state.Key.Unsupported(typeof(MotionScript.Glided), typeof(MotionScript.Sprung))))
           select steered;
}
```
**Why**
Retargeting needs live position and spring velocity, not a second case family mirroring the script. `Option<double>` states exactly when the necessary velocity evidence exists.
**Change**
Keep exhaustive dispatch on the script, require and admit velocity only in the sprung arm, and re-seat the analytic spring from the flat sample.
**Delta**
LOC +1; types +0; members +0; enables task 10's nested-type deletion
