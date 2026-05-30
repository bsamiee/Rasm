using Rasm.Rhino.UI;
using Rasm.TestKit;
using DrawingColor = System.Drawing.Color;

namespace Rasm.Rhino.Tests.UI;

// --- [CONSTANTS] --------------------------------------------------------------------------
internal static class MotionCases {
    internal const double EndpointTolerance = 2e-3;   // Bounce/Elastic anchor endpoints within ~3e-5; poly/sine are exact
    internal const int ChannelTolerance = 2;          // 8-bit sRGB<->OKLab round-trip rounding

    internal static readonly Gen<double> Unit = Gen.Double[0.0, 1.0];
    internal static readonly Gen<DrawingColor> Color =
        Gen.Byte.Select(Gen.Byte, Gen.Byte, Gen.Byte, static (byte r, byte g, byte b, byte a) => DrawingColor.FromArgb(alpha: a, red: r, green: g, blue: b));

    internal static bool ChannelsClose(DrawingColor x, DrawingColor y) =>
        Math.Abs(x.R - y.R) <= ChannelTolerance && Math.Abs(x.G - y.G) <= ChannelTolerance && Math.Abs(x.B - y.B) <= ChannelTolerance && Math.Abs(x.A - y.A) <= ChannelTolerance;

    // Deterministic monotone clock: each GetTimestamp advances a fixed quantum so Step sees a constant dt.
    internal sealed class TickClock(long step) : TimeProvider {
        private long now;
        public override long GetTimestamp() { now += step; return now; }
        public override long TimestampFrequency => TimeSpan.TicksPerSecond;
    }
}

// --- [ALGEBRAIC] --------------------------------------------------------------------------
public sealed class EasingCurveLaws {
    [Fact]
    public void LinearIsIdentityOnTheUnitInterval() =>
        Spec.ForAll(MotionCases.Unit, t => Spec.Holds(condition: Math.Abs(Easing.Linear.Apply(t: t) - t) < 1e-12, label: $"Linear({t}) drifted"));

    // Every Penner curve anchors f(0)=0 and f(1)=1 (overshoot/bounce happens strictly inside the interval).
    [Fact]
    public void EveryEasingAnchorsBothEndpoints() =>
        toSeq(Easing.Items).Iter(static easing => {
            Spec.Holds(condition: Math.Abs(easing.Apply(t: 0.0)) < MotionCases.EndpointTolerance, label: $"{easing}(0) != 0");
            Spec.Holds(condition: Math.Abs(easing.Apply(t: 1.0) - 1.0) < MotionCases.EndpointTolerance, label: $"{easing}(1) != 1");
        });
}

public sealed class OklabInterpolationLaws {
    [Fact]
    public void LerpEndpointsRoundTripThroughOklab() =>
        Spec.ForAll(MotionCases.Color.Select(MotionCases.Color), ((DrawingColor A, DrawingColor B) p) => {
            Spec.Holds(condition: MotionCases.ChannelsClose(MotionVector.Color.Lerp(a: p.A, b: p.B, t: 0.0), p.A), label: $"Lerp@0 != A ({p.A}->{p.B})");
            Spec.Holds(condition: MotionCases.ChannelsClose(MotionVector.Color.Lerp(a: p.A, b: p.B, t: 1.0), p.B), label: $"Lerp@1 != B ({p.A}->{p.B})");
        });

    [Fact]
    public void LerpOfAColourWithItselfIsThatColour() =>
        Spec.ForAll(MotionCases.Color.Select(MotionCases.Unit), ((DrawingColor C, double T) p) =>
            Spec.Holds(condition: MotionCases.ChannelsClose(MotionVector.Color.Lerp(a: p.C, b: p.C, t: p.T), p.C), label: $"Lerp({p.C},{p.C},{p.T}) drifted"));
}

public sealed class ColorMotionVectorLaws {
    [Fact]
    public void ColorDeltaPreservesSignedVelocityUntilProjection() {
        DrawingColor white = DrawingColor.FromArgb(alpha: 255, red: 255, green: 255, blue: 255);
        DrawingColor black = DrawingColor.FromArgb(alpha: 0, red: 0, green: 0, blue: 0);
        MotionColor velocity = MotionVector.Color.Delta(from: white, target: black);
        Spec.Holds(condition: velocity.A < 0.0 && velocity.R < 0.0 && velocity.G < 0.0 && velocity.B < 0.0, label: "color velocity was clamped before integration");
        Spec.Holds(condition: MotionCases.ChannelsClose(MotionVector.Color.Move(value: white, delta: velocity), black), label: "signed velocity did not project to the target color");
    }

    [Fact]
    public void UiTimerRejectsUnsafeIntervals() {
        Spec.Fail(MotionClock.UITimer(intervalSeconds: 0.0));
        Spec.Fail(MotionClock.UITimer(intervalSeconds: double.NaN));
        Spec.Succ(MotionClock.UITimer(intervalSeconds: 1.0 / 60.0));
    }
}

public sealed class DecayRunnerLaws {
    private static DecayRunnerState<double, double> Seed(double velocity, double friction, MotionCases.TickClock clock) =>
        new(Value: 0.0, Velocity: velocity, Friction: friction, Vector: MotionVector.Double, Sink: static _ => { }, Clock: clock, Timestamp: clock.GetTimestamp());

    // Friction-only deceleration: speed is non-increasing every step and the runner reaches rest in finite time.
    [Fact]
    public void VelocityDecaysMonotonicallyToRest() {
        MotionCases.TickClock clock = new(step: TimeSpan.TicksPerSecond / 60);
        DecayRunnerState<double, double> state = Seed(velocity: 12.0, friction: 6.0, clock: clock);
        DecayRunnerState<double, double> stepped = state;
        _ = toSeq(Enumerable.Range(start: 0, count: 600)).Iter(_ => {
            DecayRunnerState<double, double> next = stepped.Step();
            Spec.Holds(condition: Math.Abs(next.Velocity) <= Math.Abs(stepped.Velocity) + 1e-9, label: "decay velocity increased");
            stepped = next;
        });
        Spec.Holds(condition: !stepped.IsActive, label: $"decay never rested (|v|={Math.Abs(stepped.Velocity)})");
        Spec.Holds(condition: Math.Abs(stepped.Velocity) < MotionVector.Double.RestEpsilon, label: "rest velocity above epsilon");
    }

    // A live runner that is still moving stays active (drives the redraw loop until friction wins).
    [Fact]
    public void ActiveWhileVelocityExceedsEpsilon() {
        MotionCases.TickClock clock = new(step: TimeSpan.TicksPerSecond / 60);
        Spec.Holds(condition: Seed(velocity: 5.0, friction: 1.0, clock: clock).IsActive, label: "fast decay reported at rest");
        Spec.Holds(condition: !Seed(velocity: 0.0, friction: 1.0, clock: clock).IsActive, label: "zero-velocity decay reported active");
    }
}
