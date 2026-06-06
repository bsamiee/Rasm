using Grasshopper2.UI.Animation;
using Rasm.Grasshopper.UI;
using Rasm.TestKit;
using GhDuration = Grasshopper2.UI.Animation.Duration;
using GhMotion = Grasshopper2.UI.Animation.Motion;

namespace Rasm.Grasshopper.Tests.UI;

// --- [MODELS] ----------------------------------------------------------------------------
internal static class HandleGens {
    public static SpringHandle<float> Spring(float value, float velocity, float target) =>
        new(cell: Atom(new SpringRunnerState<float>(
                Value: value, Velocity: velocity, Target: target, Config: SpringPreset.Standard.Config,
                Vector: MotionVector.Float, Sink: static _ => { }, Clock: TimeProvider.System, Timestamp: 0L)),
            subscription: Subscription.Empty,
            wake: static () => { });

    public static PulseHandle<float> Pulse(int cyclesRemaining, bool infinite) =>
        new(cell: Atom(new PulseRunnerState<float>(
                Animated: Animated<float>.CreateFinished(0f, MotionVector.Float.Interpolate),
                From: 0f, To: 1f, Duration: GhDuration.Fast, Easing: GhMotion.Linear, Yoyo: false,
                Infinite: infinite, CyclesRemaining: cyclesRemaining, Vector: MotionVector.Float, Sink: static _ => { },
                Canvas: null!)),
            subscription: Subscription.Empty,
            wake: static () => { });
}

// --- [OPERATIONS] ----------------------------------------------------------------------------
public sealed class SpringHandleConvergenceLaws {
    // Hand-classified rest oracle: convergence is physical rest — displacement AND velocity inside RestEpsilon.
    public static TheoryData<float, float, float, bool> Cases => new() {
        { 1f, 0f, 1f, true },          // at target, still
        { 1.0001f, 0f, 1f, true },     // sub-epsilon displacement, still
        { 0f, 0f, 100f, false },       // displaced far, still
        { 1f, 5f, 1f, false },         // at target but moving
        { 50f, 50f, -50f, false },     // displaced and moving
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void IsConvergedTracksRest(float value, float velocity, float target, bool converged) {
        using SpringHandle<float> handle = HandleGens.Spring(value: value, velocity: velocity, target: target);
        Assert.Equal(expected: converged, actual: handle.IsConverged);
    }

    // Physics oracle (not a mirror of the rest predicate): a stable spring driven by explicit dt steps must
    // SETTLE AT the target — proving Step integrates toward Target, then that the rest state is a fixed point.
    [Fact]
    public void SpringPhysicallySettlesAtTarget() {
        SpringRunnerState<float> start = new(
            Value: 0f, Velocity: 0f, Target: 100f, Config: SpringPreset.Smooth.Config,
            Vector: MotionVector.Float, Sink: static _ => { }, Clock: TimeProvider.System, Timestamp: 0L);
        SpringRunnerState<float> rested = Enumerable.Range(start: 0, count: 4000)
            .Aggregate(seed: start, func: static (state, _) => state.Advance(frameDeltaSeconds: 1f / 120f));
        Assert.False(condition: rested.IsActive);
        Assert.True(condition: MathF.Abs(rested.Value - 100f) < MotionVector.Float.RestEpsilon);
        using SpringHandle<float> handle = HandleGens.Spring(value: rested.Value, velocity: rested.Velocity, target: 100f);
        Assert.True(condition: handle.IsConverged);
        Assert.False(condition: rested.Advance(frameDeltaSeconds: 1f / 120f).IsActive);
    }

    [Fact]
    public void RetargetRejectsNonFiniteTargetsBeforeWakingRunner() {
        using SpringHandle<float> handle = HandleGens.Spring(value: 0f, velocity: 0f, target: 1f);
        Spec.FailCategory(result: handle.Retarget(target: float.NaN), category: "Input");
        Assert.Equal(expected: 1f, actual: handle.CurrentTarget);
    }
}

public sealed class PulseHandleCyclingLaws {
    // Cycling iff the runner stays active: cycles remaining, infinite, or an unfinished curve. A settled curve
    // with no remaining finite cycles is the sole resting state.
    public static TheoryData<int, bool, bool> Cases => new() {
        { 0, false, false },   // settled, no cycles left, finite -> rested
        { 2, false, true },    // cycles remaining -> cycling
        { 0, true, true },     // infinite -> cycling
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void IsCyclingTracksActivity(int cyclesRemaining, bool infinite, bool cycling) {
        using PulseHandle<float> handle = HandleGens.Pulse(cyclesRemaining: cyclesRemaining, infinite: infinite);
        Assert.Equal(expected: cycling, actual: handle.IsCycling);
    }
}

public sealed class SpringConfigLaws {
    public static TheoryData<float, float, float> InvalidCases => new() {
        { 0f, 1f, 1f },
        { -1f, 1f, 1f },
        { float.NaN, 1f, 1f },
        { 1f, -0.01f, 1f },
        { 1f, float.PositiveInfinity, 1f },
        { 1f, 1f, 0f },
        { 1f, 1f, float.NegativeInfinity },
    };

    [Theory]
    [MemberData(nameof(InvalidCases))]
    public void RejectsNonPhysicalParameters(float stiffness, float damping, float mass) =>
        Assert.False(condition: SpringConfig.TryCreate(stiffness: stiffness, damping: damping, mass: mass, obj: out _));

    [Fact]
    public void ResponseFactoryMatchesClosedFormOscillatorParameters() {
        SpringConfig config = Spec.SuccValue(
            result: SpringConfig.Response(response: 0.5f, dampingFraction: 0.75f, mass: 2f),
            label: nameof(SpringConfig.Response));
        float omega = (float)(2.0 * Math.PI) / 0.5f;

        Assert.Equal(expected: omega * omega * 2f, actual: config.Stiffness, tolerance: 1e-4f);
        Assert.Equal(expected: 4f * (float)Math.PI * 0.75f * 2f / 0.5f, actual: config.Damping, tolerance: 1e-5f);
        Assert.Equal(expected: 2f, actual: config.Mass);
    }

    [Fact]
    public void PresetCatalogContainsFinitePositiveSpringConfigurations() =>
        Spec.Cases(items: SpringPreset.Items, key: static preset => preset.Key, law: static preset => {
            Assert.True(condition: preset.Config.Stiffness > 0f && float.IsFinite(preset.Config.Stiffness));
            Assert.True(condition: preset.Config.Damping >= 0f && float.IsFinite(preset.Config.Damping));
            Assert.True(condition: preset.Config.Mass > 0f && float.IsFinite(preset.Config.Mass));
        });
}

public sealed class MotionVectorLaws {
    [Fact]
    public void FloatVectorObeysBasicLinearSpaceLaws() =>
        Spec.ForAll(Gen.Double[start: -1000.0, finish: 1000.0].Select(Gen.Double[start: -1000.0, finish: 1000.0], static (double a, double b) => ((float)a, (float)b)), static tuple => {
            (float a, float b) = tuple;
            Assert.Equal(expected: a, actual: MotionVector.Float.Add(a: a, b: MotionVector.Float.Zero));
            Assert.Equal(expected: a - b, actual: MotionVector.Float.Subtract(a: a, b: b));
            Assert.Equal(expected: Math.Abs(a - b), actual: MotionVector.Float.Norm(value: MotionVector.Float.Subtract(a: a, b: b)));
            Assert.Equal(expected: a, actual: MotionVector.Float.Interpolate(value0: a, value1: b, factor: 0.0), tolerance: 1e-4f);
            Assert.Equal(expected: b, actual: MotionVector.Float.Interpolate(value0: a, value1: b, factor: 1.0), tolerance: 1e-4f);
        });

    [Fact]
    public void CompositeVectorsRejectNonFiniteComponents() {
        Assert.False(condition: MotionVector.PointF.IsFinite(value: new PointF(x: float.NaN, y: 0f)));
        Assert.False(condition: MotionVector.SizeF.IsFinite(value: new SizeF(width: 1f, height: float.PositiveInfinity)));
        Assert.False(condition: MotionVector.RectangleF.IsFinite(value: new RectangleF(x: 0f, y: 0f, width: 1f, height: float.NaN)));
        Assert.True(condition: MotionVector.Color.IsFinite(value: Colors.White));
    }
}

public sealed class CosmeticThreadingLaws {
    private static CosmeticIntent.TextLayerCase Fonted =>
        new(Text: "Rasm", Origin: new PointF(4f, 4f), Tint: Colors.White, FontSize: 12f, Duration: GhDuration.Normal, FontFamily: Some("Helvetica Neue"));

    // Regression for the verified PrepareCosmetic bug: a field-mutating with-expression (the shape the mapping
    // now uses for every case) must preserve FontFamily — the manual `new(...)` it replaced silently dropped it.
    [Fact]
    public void FontFamilySurvivesFieldMutatingWith() {
        CosmeticIntent.TextLayerCase mapped = Fonted with { Origin = new PointF(8f, 8f) };
        Assert.Equal(expected: "Helvetica Neue", actual: mapped.FontFamily.IfNone(""));
        Assert.Equal(expected: 8f, actual: mapped.Origin.X);
    }

    // The shared Completion/Keyframe init properties default to None and thread through with{}, surviving
    // a subsequent field mutation — the mechanism CosmeticAttach/BuildCosmeticAnimation read them through.
    [Fact]
    public void CompletionAndKeyframeThreadThroughWith() {
        Assert.True(condition: Fonted.Completion.IsNone && Fonted.Keyframe.IsNone);
        CosmeticIntent enriched = (Fonted with { Completion = Some(static () => unit), Keyframe = Some(Easing.BounceOut) }) with { Origin = new PointF(2f, 2f) };
        Assert.True(condition: enriched.Completion.IsSome);
        Assert.Equal(expected: Easing.BounceOut.Key, actual: enriched.Keyframe.Map(static curve => curve.Key).IfNone(-1));
    }

    // The new public union case transports its SnappingSnapshot payload (guide lines + per-channel labels) intact.
    [Fact]
    public void SnapGuideCaseCarriesSnapshot() {
        SnappingSnapshot snap = new(Dx: 4f, Dy: 0f, Magnitude: 4f, XLabel: Some(new SnapLabel(Text: "left", Point: new PointF(10f, 2f), Anchor: default)), YLabel: Option<SnapLabel>.None, Lines: Seq(new LineF(new PointF(0f, 0f), new PointF(20f, 0f))));
        CosmeticIntent.SnapGuideCase guide = Assert.IsType<CosmeticIntent.SnapGuideCase>(@object: new CosmeticIntent.SnapGuideCase(Snapshot: snap, Style: SnapGuideStyle.Dashed(tint: Colors.Yellow) with { Duration = GhDuration.Fast }));
        Assert.Equal(expected: 1, actual: guide.Snapshot.Lines.Count);
        // The X channel carries its label text + point intact; the absent Y channel stays None (no lossy merge).
        Assert.True(condition: guide.Snapshot.XLabel.IsSome && guide.Snapshot.YLabel.IsNone);
        Assert.Equal(expected: "left", actual: guide.Snapshot.XLabel.Map(static label => label.Text).IfNone(""));
        Assert.Equal(expected: 10f, actual: guide.Snapshot.XLabel.Map(static label => label.Point.X).IfNone(0f));
        // The downstream-tunable style flows: dashed pattern non-empty, overridden duration carried, tint kept.
        Assert.True(condition: guide.Style.Dashes.Length > 0 && guide.Style.Duration == GhDuration.Fast && guide.Style.Tint == Colors.Yellow);
    }

    // Easing SmartEnum exposes all 46 curves; closed-form endpoints anchor the unit interval the keyframe path samples.
    [Fact]
    public void EasingVocabularyAndEndpoints() {
        Assert.Equal(expected: 46, actual: Easing.Items.Count);
        Assert.Equal(expected: 1.0, actual: Easing.BounceOut.Apply(t: 1.0), precision: 9);
        Assert.Equal(expected: 0.0, actual: Easing.QuadIn.Apply(t: 0.0), precision: 9);
    }
}
