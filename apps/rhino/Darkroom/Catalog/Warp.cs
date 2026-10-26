using Darkroom.Chain;
using PeterO.Numbers;
using Rasm.Imaging.Files;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Warping;
using Rasm.UI;
using UnitsNet.Units;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct TwirlAngle : IPresented<TwirlAngle, float> {
    public static TwirlAngle MinValue { get; } = new(-2f * float.Tau);
    public static TwirlAngle MaxValue { get; } = new(2f * float.Tau);
    public static Presentation<float> Presentation => Presentations.Angle with { Soft = (-float.Tau, float.Tau) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct FalloffPower : IPresented<FalloffPower, float> {
    public static FalloffPower MinValue { get; } = new(0.1f);
    public static FalloffPower MaxValue { get; } = new(8f);
    public static Presentation<float> Presentation => Factor.Presentation;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Uniform", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ArmCount : IPresented<ArmCount, int> {
    public static ArmCount MinValue => Uniform;
    public static ArmCount MaxValue { get; } = new(16);
    public static Presentation<int> Presentation { get; } = new() { Soft = (0, 8) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct WaveCount : IPresented<WaveCount, int> {
    public static WaveCount MinValue { get; } = new(1);
    public static WaveCount MaxValue { get; } = new(64);
    public static Presentation<int> Presentation { get; } = new() { Soft = (1, 16) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct MirrorCount : IPresented<MirrorCount, int> {
    public static MirrorCount MinValue { get; } = new(1);
    public static MirrorCount MaxValue { get; } = new(32);
    public static Presentation<int> Presentation { get; } = new() { Soft = (1, 16) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct DropSize : IPresented<DropSize, float> {
    public static DropSize MinValue { get; } = new(0.01f);
    public static DropSize MaxValue { get; } = new(0.5f);
    public static Presentation<float> Presentation => ProperFraction.Presentation;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SkewAngle : IPresented<SkewAngle, float> {
    public static SkewAngle MaxValue { get; } = new(float.BitDecrement(float.Pi) / 2f);
    public static SkewAngle MinValue { get; } = new(-MaxValue._value);
    public static Presentation<float> Presentation => Presentations.Angle with { Soft = (-float.Atan(3f), float.Atan(3f)) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

public sealed record ViewAngles(PanoramaView View) : IStateRecord<ViewAngles, ViewAnglesParameter> {
    public static ViewAngles Default { get; } = new(default(PanoramaView));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ViewAnglesParameter : IStateParameter<ViewAngles> {
    public static readonly ViewAnglesParameter Heading = new("heading", StateParameter.Bounded(Lens<ViewAngles, SignedAngle>.New(static angles => angles.View.Heading, static heading => angles => new(angles.View with { Heading = heading })), Presentations.Angle),
        new("Heading", "Rightward turn of view"));
    public static readonly ViewAnglesParameter Elevation = new("elevation", StateParameter.Bounded(Lens<ViewAngles, Elevation>.New(static angles => angles.View.Elevation, static elevation => angles => new(angles.View with { Elevation = elevation })), Presentations.Angle),
        new("Elevation", "View upward tilt"));
    public static readonly ViewAnglesParameter Roll = new("roll", StateParameter.Bounded(Lens<ViewAngles, SignedAngle>.New(static angles => angles.View.Roll, static roll => angles => new(angles.View with { Roll = roll })), Presentations.Angle),
        new("Roll", "Counterclockwise turn about view axis"));

    public StateParameter<ViewAngles> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Resampling(Sampling Sampling, WrapMode Wrap) : IStateRecord<Resampling, ResamplingParameter> {
    public static Resampling Default { get; } = new(Sampling.Linear, WrapMode.Black);

    public (WrapMode Across, WrapMode Down) Edges => (Wrap, Wrap);

    public PixelPass.Frame Pass(CoordinateMap map) => new(map.Apply);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ResamplingParameter : IStateParameter<Resampling> {
    public static readonly ResamplingParameter Sampling = new("sampling", new StateParameter<Resampling>.Choice<Sampling>(Lens<Resampling, Sampling>.New(static resampling => resampling.Sampling, static sampling => resampling => resampling with { Sampling = sampling })),
        new("Interpolation", "Filter between source pixels"));
    public static readonly ResamplingParameter Wrap = new("wrap", new StateParameter<Resampling>.Choice<WrapMode>(Lens<Resampling, WrapMode>.New(static resampling => resampling.Wrap, static wrap => resampling => resampling with { Wrap = wrap })),
        new("Extension", "Sampling past frame edges"));

    public StateParameter<Resampling> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Twirl(TwirlAngle Angle, ShortSideLength Radius, FalloffPower Falloff, ArmCount Arms, SignedAngle ArmOffset, Position Center, Resampling Resampling)
    : IStateRecord<Twirl, TwirlParameter>, IPixelStage<Twirl> {
    public static Twirl Default { get; } = new(TwirlAngle.Off, ShortSideLength.Create(0.5f), FalloffPower.Create(1f), ArmCount.Uniform, SignedAngle.Neutral, Position.Default, Resampling.Default with { Wrap = WrapMode.Periodic });

    public static Option<PixelPass> Pass(Twirl state, PassContext context) =>
        state.Angle == TwirlAngle.Off || state.Radius == ShortSideLength.Neutral ? None
            : Some<PixelPass>(state.Resampling.Pass(new Displacement.Twirl(state.Angle, state.Radius, state.Falloff, state.Arms, state.ArmOffset, new FramePosition(state.Center.X, state.Center.Y))
                .Map(context.Extent, state.Resampling.Sampling, state.Resampling.Edges)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TwirlParameter : IStateParameter<Twirl> {
    public static readonly TwirlParameter Angle = new("angle", StateParameter.Bounded(Lens<Twirl, TwirlAngle>.New(static state => state.Angle, static angle => state => state with { Angle = angle })),
        new("Angle", "Counterclockwise turn at center, none at radius"));
    public static readonly TwirlParameter Radius = new("radius", StateParameter.Bounded(Lens<Twirl, ShortSideLength>.New(static state => state.Radius, static radius => state => state with { Radius = radius }), Presentations.Length),
        new("Radius", "Twirl reach in frame short sides"));
    public static readonly TwirlParameter Falloff = new("falloff", StateParameter.Bounded(Lens<Twirl, FalloffPower>.New(static state => state.Falloff, static falloff => state => state with { Falloff = falloff })),
        new("Falloff", "Turn falloff exponent, 1 linear"));
    public static readonly TwirlParameter Arms = new("arms", StateParameter.Bounded(Lens<Twirl, ArmCount>.New(static state => state.Arms, static arms => state => state with { Arms = arms })),
        new("Arms", "Spiral arms the turn gathers into, 0 turning every direction alike"));
    public static readonly TwirlParameter ArmOffset = new("arm-offset", StateParameter.Bounded(Lens<Twirl, SignedAngle>.New(static state => state.ArmOffset, static offset => state => state with { ArmOffset = offset }), Presentations.Angle),
        new("Arm offset", "Arm turn about center"));
    public static readonly TwirlParameter Center = new("center", new StateParameter<Twirl>.Record<Position>(Lens<Twirl, Position>.New(static state => state.Center, static center => state => state with { Center = center })),
        new("Center", "Point the warp centers on"));
    public static readonly TwirlParameter Resampling = new("resampling", new StateParameter<Twirl>.Record<Resampling>(Lens<Twirl, Resampling>.New(static state => state.Resampling, static resampling => state => state with { Resampling = resampling })),
        new("Resampling", "Source filter and edge extension"));

    public StateParameter<Twirl> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Bulge(SignedFraction Amount, BulgeMode Mode, ShortSideLength Radius, Position Center, Resampling Resampling) : IStateRecord<Bulge, BulgeParameter>, IPixelStage<Bulge> {
    public static Bulge Default { get; } = new(SignedFraction.Neutral, BulgeMode.Round, ShortSideLength.Create(0.5f), Position.Default, Resampling.Default with { Wrap = WrapMode.Clamp });

    public static Option<PixelPass> Pass(Bulge state, PassContext context) =>
        state.Amount == SignedFraction.Neutral || state.Radius == ShortSideLength.Neutral ? None
            : Some<PixelPass>(state.Resampling.Pass(new Displacement.Bulge(state.Amount, state.Mode, state.Radius, new FramePosition(state.Center.X, state.Center.Y))
                .Map(context.Extent, state.Resampling.Sampling, state.Resampling.Edges)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BulgeParameter : IStateParameter<Bulge> {
    public static readonly BulgeParameter Amount = new("amount", StateParameter.Bounded(Lens<Bulge, SignedFraction>.New(static state => state.Amount, static amount => state => state with { Amount = amount })),
        new("Amount", "Bulge above 0 and pinch below, ±1 a full hemisphere"));
    public static readonly BulgeParameter Mode = new("mode", new StateParameter<Bulge>.Choice<BulgeMode>(Lens<Bulge, BulgeMode>.New(static state => state.Mode, static mode => state => state with { Mode = mode })),
        new("Mode", "Axes the bulge acts along"));
    public static readonly BulgeParameter Radius = new("radius", StateParameter.Bounded(Lens<Bulge, ShortSideLength>.New(static state => state.Radius, static radius => state => state with { Radius = radius }), Presentations.Length),
        new("Radius", "Bulged disc radius in frame short sides"));
    public static readonly BulgeParameter Center = new("center", new StateParameter<Bulge>.Record<Position>(Lens<Bulge, Position>.New(static state => state.Center, static center => state => state with { Center = center })),
        new("Center", "Point the warp centers on"));
    public static readonly BulgeParameter Resampling = new("resampling", new StateParameter<Bulge>.Record<Resampling>(Lens<Bulge, Resampling>.New(static state => state.Resampling, static resampling => state => state with { Resampling = resampling })),
        new("Resampling", "Source filter and edge extension"));

    public StateParameter<Bulge> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record WaveWarp(
    WaveFront Front, WaveProfile Profile, WaveCount Count, ShortSideExtent MinWavelength, ShortSideExtent MaxWavelength, Fraction MinAmplitude, Fraction MaxAmplitude,
    ShortSideOffset Horizontal, ShortSideOffset Vertical, SignedAngle Direction, SignedAngle Spread, SignedAngle Phase, ShortSideLength Radius, Position Center, Hertz Rate,
    Seed Seed, Timing Timing, Hold Hold, Resampling Resampling)
    : IStateRecord<WaveWarp, WaveWarpParameter>, IPixelStage<WaveWarp> {
    private const uint Stream = 8u;

    public static WaveWarp Default { get; } = new(
        WaveFront.Linear, WaveProfile.Sine, WaveCount.Create(5), ShortSideExtent.Create(10f / ReferenceFrame.Height), ShortSideExtent.Create(120f / ReferenceFrame.Height), Fraction.Create(5f / 35f), Fraction.MaxValue,
        ShortSideOffset.Neutral, ShortSideOffset.Neutral, SignedAngle.Neutral, SignedAngle.Neutral, SignedAngle.Neutral, ShortSideLength.Create(0.5f), Position.Default,
        Hertz.Neutral, Seed.MinValue, Timing.Default, Hold.MinValue, Resampling.Default);

    public static Option<PixelPass> Pass(WaveWarp state, PassContext context) =>
        (state.Horizontal, state.Vertical) == (ShortSideOffset.Neutral, ShortSideOffset.Neutral) || (state.MinAmplitude, state.MaxAmplitude) == (Fraction.MinValue, Fraction.MinValue)
        || (state.Front.Ring.IsSome && state.Radius == ShortSideLength.Neutral)
            ? None
            : Some<PixelPass>(state.Resampling.Pass(new Displacement.Wave(
                state.Front, state.Profile, state.Count, (state.MinWavelength, state.MaxWavelength), (state.MinAmplitude, state.MaxAmplitude), (state.Horizontal, state.Vertical),
                state.Direction, state.Spread, (state.Phase / float.Tau) - (state.Rate * state.Hold.Held(state.Timing.At(context))), state.Radius, new FramePosition(state.Center.X, state.Center.Y),
                state.Seed.Field(Stream, 0u)).Map(context.Extent, state.Resampling.Sampling, state.Resampling.Edges)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WaveWarpParameter : IStateParameter<WaveWarp> {
    public static readonly WaveWarpParameter Front = new("front", new StateParameter<WaveWarp>.Choice<WaveFront>(Lens<WaveWarp, WaveFront>.New(static state => state.Front, static front => state => state with { Front = front })),
        new("Form", "Straight waves or center rings moving outward, around, or both"));
    public static readonly WaveWarpParameter Profile = new("profile", new StateParameter<WaveWarp>.Choice<WaveProfile>(Lens<WaveWarp, WaveProfile>.New(static state => state.Profile, static profile => state => state with { Profile = profile })),
        new("Profile", "Shape of each wave period"));
    public static readonly WaveWarpParameter Count = new("count", StateParameter.Bounded(Lens<WaveWarp, WaveCount>.New(static state => state.Count, static count => state => state with { Count = count })),
        new("Generators", "Waves summed into the field"));
    public static readonly WaveWarpParameter MinWavelength = new("min-wavelength", StateParameter.Bounded(Lens<WaveWarp, ShortSideExtent>.New(static state => state.MinWavelength, static wavelength => state => state with { MinWavelength = wavelength }),
        Presentations.Extent with { Soft = (0.005f, 1f) }),
        new("Wavelength min", "Shortest wave in frame short sides"));
    public static readonly WaveWarpParameter MaxWavelength = new("max-wavelength", StateParameter.Bounded(Lens<WaveWarp, ShortSideExtent>.New(static state => state.MaxWavelength, static wavelength => state => state with { MaxWavelength = wavelength }),
        Presentations.Extent with { Soft = (0.005f, 1f) }),
        new("Wavelength max", "Longest wave in frame short sides"));
    public static readonly WaveWarpParameter MinAmplitude = new("min-amplitude", StateParameter.Bounded(Lens<WaveWarp, Fraction>.New(static state => state.MinAmplitude, static amplitude => state => state with { MinAmplitude = amplitude })),
        new("Amplitude min", "Weakest wave as share of amounts"));
    public static readonly WaveWarpParameter MaxAmplitude = new("max-amplitude", StateParameter.Bounded(Lens<WaveWarp, Fraction>.New(static state => state.MaxAmplitude, static amplitude => state => state with { MaxAmplitude = amplitude })),
        new("Amplitude max", "Strongest wave as share of amounts"));
    public static readonly WaveWarpParameter Horizontal = new("horizontal", StateParameter.Bounded(Lens<WaveWarp, ShortSideOffset>.New(static state => state.Horizontal, static amount => state => state with { Horizontal = amount }), Presentations.Offset with { Soft = (-0.1f, 0.1f) }),
        new("Amount X", "Horizontal move at full strength in frame short sides"));
    public static readonly WaveWarpParameter Vertical = new("vertical", StateParameter.Bounded(Lens<WaveWarp, ShortSideOffset>.New(static state => state.Vertical, static amount => state => state with { Vertical = amount }), Presentations.Offset with { Soft = (-0.1f, 0.1f) }),
        new("Amount Y", "Vertical move at full strength in frame short sides"));
    public static readonly WaveWarpParameter Direction = new("direction", StateParameter.Bounded(Lens<WaveWarp, SignedAngle>.New(static state => state.Direction, static direction => state => state with { Direction = direction }), Presentations.Angle),
        new("Direction", "Straight wave angle counterclockwise from right"));
    public static readonly WaveWarpParameter Spread = new("spread", StateParameter.Bounded(Lens<WaveWarp, SignedAngle>.New(static state => state.Spread, static spread => state => state with { Spread = spread }),
        Presentations.Angle with { Soft = (0f, float.Pi) }),
        new("Spread", "Wave direction fan about travel direction"));
    public static readonly WaveWarpParameter Phase = new("phase", StateParameter.Bounded(Lens<WaveWarp, SignedAngle>.New(static state => state.Phase, static phase => state => state with { Phase = phase }), Presentations.Angle),
        new("Phase", "Turn added to every wave period"));
    public static readonly WaveWarpParameter Radius = new("radius", StateParameter.Bounded(Lens<WaveWarp, ShortSideLength>.New(static state => state.Radius, static radius => state => state with { Radius = radius }), Presentations.Length),
        new("Radius", "Ring reach in frame short sides, fading to none"));
    public static readonly WaveWarpParameter Center = new("center", new StateParameter<WaveWarp>.Record<Position>(Lens<WaveWarp, Position>.New(static state => state.Center, static center => state => state with { Center = center })),
        new("Center", "Point the warp centers on"));
    public static readonly WaveWarpParameter Rate = new("rate", StateParameter.Bounded(Lens<WaveWarp, Hertz>.New(static state => state.Rate, static rate => state => state with { Rate = rate }), Hertz.Presentation with { Soft = (-2f, 2f) }),
        new("Rate", "Wave periods traveled per second"));
    public static readonly WaveWarpParameter Seed = new("seed", StateParameter.Bounded(Lens<WaveWarp, Seed>.New(static state => state.Seed, static seed => state => state with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly WaveWarpParameter Timing = new("timing", new StateParameter<WaveWarp>.Record<Timing>(Lens<WaveWarp, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })),
        new("Timing", "Stored time and rate of recorded frame time"));
    public static readonly WaveWarpParameter Hold = new("hold", StateParameter.Bounded(Lens<WaveWarp, Hold>.New(static state => state.Hold, static hold => state => state with { Hold = hold })),
        new("Hold", "Seconds between pattern redraws"));
    public static readonly WaveWarpParameter Resampling = new("resampling", new StateParameter<WaveWarp>.Record<Resampling>(Lens<WaveWarp, Resampling>.New(static state => state.Resampling, static resampling => state => state with { Resampling = resampling })),
        new("Resampling", "Source filter and edge extension"));

    public StateParameter<WaveWarp> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Turbulence(
    NoiseBasis Basis, ShortSideExtent ScaleX, ShortSideExtent ScaleY, ShortSideOffset Horizontal, ShortSideOffset Vertical, Hertz Rate, Seed Seed, Timing Timing, Hold Hold, Resampling Resampling)
    : IStateRecord<Turbulence, TurbulenceParameter>, IPixelStage<Turbulence> {
    private const uint Stream = 9u;

    public static Turbulence Default { get; } = new(
        NoiseBasis.Default, ShortSideExtent.Create(ReferenceFrame.GreaterSide / 2f), ShortSideExtent.Create(ReferenceFrame.GreaterSide / 2f), ShortSideOffset.Neutral, ShortSideOffset.Neutral, Hertz.Neutral, Seed.MinValue, Timing.Default, Hold.MinValue,
        Resampling.Default with { Wrap = WrapMode.Clamp });

    public static Option<PixelPass> Pass(Turbulence state, PassContext context) =>
        (state.Horizontal, state.Vertical) == (ShortSideOffset.Neutral, ShortSideOffset.Neutral) ? None
            : state.Basis.Sampler switch {
                var sampler => Some<PixelPass>(state.Resampling.Pass(new Displacement.Turbulence(
                    (at, hash) => sampler(at, hash).Value, (state.ScaleX, state.ScaleY), (state.Horizontal, state.Vertical), state.Rate * state.Hold.Held(state.Timing.At(context)),
                    state.Seed.Field(Stream, 0u)).Map(context.Extent, state.Resampling.Sampling, state.Resampling.Edges))),
            };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TurbulenceParameter : IStateParameter<Turbulence> {
    public static readonly TurbulenceParameter Basis = new("basis", new StateParameter<Turbulence>.Variant<NoiseBasis>(Lens<Turbulence, NoiseBasis>.New(static state => state.Basis, static basis => state => state with { Basis = basis })),
        new("Noise", "Displacement field basis"));
    public static readonly TurbulenceParameter ScaleX = new("scale-x", StateParameter.Bounded(Lens<Turbulence, ShortSideExtent>.New(static state => state.ScaleX, static scale => state => state with { ScaleX = scale }),
        Presentations.Extent with { Soft = (0.01f, 2f) }),
        new("Size X", "Horizontal noise feature size in frame short sides"));
    public static readonly TurbulenceParameter ScaleY = new("scale-y", StateParameter.Bounded(Lens<Turbulence, ShortSideExtent>.New(static state => state.ScaleY, static scale => state => state with { ScaleY = scale }),
        Presentations.Extent with { Soft = (0.01f, 2f) }),
        new("Size Y", "Vertical noise feature size in frame short sides"));
    public static readonly TurbulenceParameter Horizontal = new("horizontal", StateParameter.Bounded(Lens<Turbulence, ShortSideOffset>.New(static state => state.Horizontal, static amount => state => state with { Horizontal = amount }), Presentations.Offset with { Soft = (-0.1f, 0.1f) }),
        new("Amount X", "Horizontal move at full strength in frame short sides"));
    public static readonly TurbulenceParameter Vertical = new("vertical", StateParameter.Bounded(Lens<Turbulence, ShortSideOffset>.New(static state => state.Vertical, static amount => state => state with { Vertical = amount }), Presentations.Offset with { Soft = (-0.1f, 0.1f) }),
        new("Amount Y", "Vertical move at full strength in frame short sides"));
    public static readonly TurbulenceParameter Rate = new("rate", StateParameter.Bounded(Lens<Turbulence, Hertz>.New(static state => state.Rate, static rate => state => state with { Rate = rate }), Hertz.Presentation with { Soft = (-2f, 2f) }),
        new("Evolution", "Noise units the field evolves per second"));
    public static readonly TurbulenceParameter Seed = new("seed", StateParameter.Bounded(Lens<Turbulence, Seed>.New(static state => state.Seed, static seed => state => state with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly TurbulenceParameter Timing = new("timing", new StateParameter<Turbulence>.Record<Timing>(Lens<Turbulence, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })),
        new("Timing", "Stored time and rate of recorded frame time"));
    public static readonly TurbulenceParameter Hold = new("hold", StateParameter.Bounded(Lens<Turbulence, Hold>.New(static state => state.Hold, static hold => state => state with { Hold = hold })),
        new("Hold", "Seconds between pattern redraws"));
    public static readonly TurbulenceParameter Resampling = new("resampling", new StateParameter<Turbulence>.Record<Resampling>(Lens<Turbulence, Resampling>.New(static state => state.Resampling, static resampling => state => state with { Resampling = resampling })),
        new("Resampling", "Source filter and edge extension"));

    public StateParameter<Turbulence> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record MapDisplacement(
    Option<ImageFile> Source, Option<MapReading> Reading, Gated<ShortSideExtent> TileSize, ShortSideExtent Softness, ShortSideOffset Horizontal, ShortSideOffset Vertical, SignedAngle Rotation,
    Resampling Resampling)
    : IStateRecord<MapDisplacement, MapDisplacementParameter>, IPixelStage<MapDisplacement> {
    public static MapDisplacement Default { get; } = new(
        None, Some(MapReading.Gradient), new(Enabled: false, ShortSideExtent.Create(1f)), ShortSideExtent.Create(0.015f * ReferenceFrame.GreaterSide), ShortSideOffset.Neutral, ShortSideOffset.Neutral, SignedAngle.Neutral,
        Resampling.Default);

    public static Option<PixelPass> Pass(MapDisplacement state, PassContext context) =>
        state.Reading.Match(
            Some: reading => (state.Horizontal, state.Vertical) == (ShortSideOffset.Neutral, ShortSideOffset.Neutral) ? None
                : Some<PixelPass>(new DisplacementMap(reading, state.TileSize.Active, state.Softness, (state.Horizontal, state.Vertical), state.Rotation)
                    .Pass(state.Source.Map(static image => image.Frame), context.Working.Luminance, context.Exposure, state.Resampling.Sampling, state.Resampling.Wrap)),
            None: () => state.Source.Map(image => (PixelPass)state.Resampling.Pass(new CoordinateMap.Sampled(image.Frame, state.Resampling.Sampling, state.Resampling.Edges))));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MapDisplacementParameter : IStateParameter<MapDisplacement> {
    public static readonly MapDisplacementParameter Source = new("source", new StateParameter<MapDisplacement>.Loaded<ImageFile, ImagePath, string>(Lens<MapDisplacement, Option<ImageFile>>.New(static state => state.Source, static source => state => state with { Source = source }), ImageFile.Load, static image => image.Path),
        new("Map image", "Image file, frame itself when empty"));
    public static readonly MapDisplacementParameter Reading = new("reading", new StateParameter<MapDisplacement>.OptionalChoice<MapReading>(Lens<MapDisplacement, Option<MapReading>>.New(static state => state.Reading, static reading => state => state with { Reading = reading }), "ST map"),
        new("Reading", "Map values moving frame, or map coordinates the frame is sampled at"));
    public static readonly MapDisplacementParameter TileSize = new("tile-size", StateParameter.OptionalBounded(Lens<MapDisplacement, Gated<ShortSideExtent>>.New(static state => state.TileSize, static size => state => state with { TileSize = size }),
        Presentations.Extent with { Soft = (0.05f, 2f) }),
        new("Tile size", "Map tile height in frame short sides, off stretching to fit"));
    public static readonly MapDisplacementParameter Softness = new("softness", StateParameter.Bounded(Lens<MapDisplacement, ShortSideExtent>.New(static state => state.Softness, static softness => state => state with { Softness = softness }),
        Presentations.Extent with { Soft = (0.001f, 0.1f), Track = Some<Func<AxisRange, Axis>>(static range => new Axis.Linear(range)) }),
        new("Softness", "Map luma blur before moving frame"));
    public static readonly MapDisplacementParameter Horizontal = new("horizontal", StateParameter.Bounded(Lens<MapDisplacement, ShortSideOffset>.New(static state => state.Horizontal, static amount => state => state with { Horizontal = amount }), Presentations.Offset with { Soft = (-0.1f, 0.1f) }),
        new("Amount X", "Horizontal move at full strength in frame short sides"));
    public static readonly MapDisplacementParameter Vertical = new("vertical", StateParameter.Bounded(Lens<MapDisplacement, ShortSideOffset>.New(static state => state.Vertical, static amount => state => state with { Vertical = amount }), Presentations.Offset with { Soft = (-0.1f, 0.1f) }),
        new("Amount Y", "Vertical move at full strength in frame short sides"));
    public static readonly MapDisplacementParameter Rotation = new("rotation", StateParameter.Bounded(Lens<MapDisplacement, SignedAngle>.New(static state => state.Rotation, static rotation => state => state with { Rotation = rotation }), Presentations.Angle),
        new("Rotation", "Counterclockwise turn of map moves"));
    public static readonly MapDisplacementParameter Resampling = new("resampling", new StateParameter<MapDisplacement>.Record<Resampling>(Lens<MapDisplacement, Resampling>.New(static state => state.Resampling, static resampling => state => state with { Resampling = resampling })),
        new("Resampling", "Source filter and edge extension"));

    public StateParameter<MapDisplacement> Kind { get; }
    public ParameterText Text { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RefractionTexture {
    public abstract RefractionSurface Surface(float held);

    public virtual bool Rests(Refraction state) => (state.Strength, state.Blur) == (ShortSideOffset.Neutral, ShortSideLength.Neutral);

    public sealed record Raindrops(Fraction Density, DropSize Size, Fraction Variation, Fraction Softness, Magnitude Lens, Fraction Trails, SignedAngle Direction, Drift Fall)
        : RefractionTexture, IStateRecord<Raindrops, RaindropsParameter> {
        public static Raindrops Default { get; } = new(Fraction.Create(0.5f), DropSize.MaxValue, Fraction.Create(0.5f), Fraction.Create(0.05f), Magnitude.Create(2f), Fraction.Create(0.3f), SignedAngle.Down, Drift.Neutral);

        public override RefractionSurface Surface(float held) => new RefractionSurface.Raindrops(Density, Size, Variation, Softness, Lens, Trails, Direction, Fall * held);

        public override bool Rests(Refraction state) => Density == Fraction.MinValue;
    }

    public sealed record Blocks : RefractionTexture, IStateRecord<Blocks> {
        public static Blocks Default { get; } = new();

        public override RefractionSurface Surface(float held) => new RefractionSurface.Blocks();
    }

    public sealed record Canvas : RefractionTexture, IStateRecord<Canvas> {
        public static Canvas Default { get; } = new();

        public override RefractionSurface Surface(float held) => new RefractionSurface.Canvas();
    }

    public sealed record Frosted(Fractal Fractal) : RefractionTexture, IStateRecord<Frosted, FrostedParameter> {
        public static Frosted Default { get; } = new(Fractal.Default);

        public override RefractionSurface Surface(float held) => new RefractionSurface.Frosted(Fractal.Octaves);
    }

    public sealed record TinyLens : RefractionTexture, IStateRecord<TinyLens> {
        public static TinyLens Default { get; } = new();

        public override RefractionSurface Surface(float held) => new RefractionSurface.TinyLens();
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RaindropsParameter : IStateParameter<RefractionTexture.Raindrops> {
    public static readonly RaindropsParameter Density = new("density", StateParameter.Bounded(Lens<RefractionTexture.Raindrops, Fraction>.New(static drops => drops.Density, static density => drops => drops with { Density = density })),
        new("Density", "Share of cells holding a raindrop"));
    public static readonly RaindropsParameter Size = new("size", StateParameter.Bounded(Lens<RefractionTexture.Raindrops, DropSize>.New(static drops => drops.Size, static size => drops => drops with { Size = size })),
        new("Drop size", "Largest raindrop radius as texture cell share"));
    public static readonly RaindropsParameter Variation = new("variation", StateParameter.Bounded(Lens<RefractionTexture.Raindrops, Fraction>.New(static drops => drops.Variation, static variation => drops => drops with { Variation = variation })),
        new("Variation", "Seeded raindrop radius reduction share"));
    public static readonly RaindropsParameter Softness = new("softness", StateParameter.Bounded(Lens<RefractionTexture.Raindrops, Fraction>.New(static drops => drops.Softness, static softness => drops => drops with { Softness = softness })),
        new("Drop softness", "Raindrop radius share blending rim"));
    public static readonly RaindropsParameter Lens = new("lens", StateParameter.Bounded(Lens<RefractionTexture.Raindrops, Magnitude>.New(static drops => drops.Lens, static lens => drops => drops with { Lens = lens }), Magnitude.Presentation with { Soft = (0f, 10f) }),
        new("Lens", "Raindrop magnification"));
    public static readonly RaindropsParameter Trails = new("trails", StateParameter.Bounded(Lens<RefractionTexture.Raindrops, Fraction>.New(static drops => drops.Trails, static trails => drops => drops with { Trails = trails })),
        new("Trails", "Raindrop trail height relative to drop"));
    public static readonly RaindropsParameter Direction = new("direction", StateParameter.Bounded(Lens<RefractionTexture.Raindrops, SignedAngle>.New(static drops => drops.Direction, static direction => drops => drops with { Direction = direction }), Presentations.Angle),
        new("Direction", "Raindrop run angle counterclockwise from right"));
    public static readonly RaindropsParameter Fall = new("fall", StateParameter.Bounded(Lens<RefractionTexture.Raindrops, Drift>.New(static drops => drops.Fall, static fall => drops => drops with { Fall = fall }), Drift.Presentation with { Soft = (-1f, 1f) }),
        new("Fall", "Raindrop travel per second in frame short sides"));

    public StateParameter<RefractionTexture.Raindrops> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FrostedParameter : IStateParameter<RefractionTexture.Frosted> {
    public static readonly FrostedParameter Fractal = new("octaves", new StateParameter<RefractionTexture.Frosted>.Record<Fractal>(Lens<RefractionTexture.Frosted, Fractal>.New(static frosted => frosted.Fractal, static fractal => frosted => frosted with { Fractal = fractal })),
        new("Octaves", "Frost detail across scales"));

    public StateParameter<RefractionTexture.Frosted> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Refraction(
    RefractionTexture Texture, ShortSideExtent Scale, ShortSideOffset Strength, ShortSideExtent Smoothness, ShortSideLength Blur, Seed Seed, Timing Timing, Hold Hold, Resampling Resampling)
    : IStateRecord<Refraction, RefractionParameter>, IPixelStage<Refraction> {
    private const uint Stream = 10u;

    public static Refraction Default { get; } = new(
        RefractionTexture.Raindrops.Default, ShortSideExtent.Create(ReferenceFrame.GreaterSide / 2f / 3.8f), ShortSideOffset.Neutral, ShortSideExtent.Create(3f / ReferenceFrame.Height), ShortSideLength.Create(14.6f / ReferenceFrame.Height),
        Seed.MinValue, Timing.Default, Hold.MinValue, Resampling.Default);

    public static Option<PixelPass> Pass(Refraction state, PassContext context) =>
        state.Texture.Rests(state) ? None
            : Some<PixelPass>(new RefractionField(state.Texture.Surface(state.Hold.Held(state.Timing.At(context))), state.Scale, state.Strength, state.Smoothness, state.Blur,
                state.Seed.Field(Stream, 0u)).Pass(state.Resampling.Sampling, state.Resampling.Wrap));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RefractionParameter : IStateParameter<Refraction> {
    public static readonly RefractionParameter Texture = new("texture", new StateParameter<Refraction>.Variant<RefractionTexture>(Lens<Refraction, RefractionTexture>.New(static state => state.Texture, static texture => state => state with { Texture = texture })),
        new("Texture", "Surface the frame is seen through"));
    public static readonly RefractionParameter Scale = new("scale", StateParameter.Bounded(Lens<Refraction, ShortSideExtent>.New(static state => state.Scale, static scale => state => state with { Scale = scale }),
        Presentations.Extent with { Soft = (0.005f, 1f) }),
        new("Scale", "Texture cell size in frame short sides"));
    public static readonly RefractionParameter Strength = new("strength", StateParameter.Bounded(Lens<Refraction, ShortSideOffset>.New(static state => state.Strength, static strength => state => state with { Strength = strength }), Presentations.Offset with { Soft = (-0.1f, 0.1f) }),
        new("Distortion", "Frame bend through surface, below 0 inverting"));
    public static readonly RefractionParameter Smoothness = new("smoothness", StateParameter.Bounded(Lens<Refraction, ShortSideExtent>.New(static state => state.Smoothness, static smoothness => state => state with { Smoothness = smoothness }),
        Presentations.Extent with { Soft = (0.001f, 0.05f) }),
        new("Smoothness", "Surface blur before slope bends frame"));
    public static readonly RefractionParameter Blur = new("blur", StateParameter.Bounded(Lens<Refraction, ShortSideLength>.New(static state => state.Blur, static blur => state => state with { Blur = blur }),
        Presentations.Length with { Soft = (0f, 0.05f) }),
        new("Blur", "Frame softening through surface"));
    public static readonly RefractionParameter Seed = new("seed", StateParameter.Bounded(Lens<Refraction, Seed>.New(static state => state.Seed, static seed => state => state with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly RefractionParameter Timing = new("timing", new StateParameter<Refraction>.Record<Timing>(Lens<Refraction, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })),
        new("Timing", "Stored time and rate of recorded frame time"));
    public static readonly RefractionParameter Hold = new("hold", StateParameter.Bounded(Lens<Refraction, Hold>.New(static state => state.Hold, static hold => state => state with { Hold = hold })),
        new("Hold", "Seconds between pattern redraws"));
    public static readonly RefractionParameter Resampling = new("resampling", new StateParameter<Refraction>.Record<Resampling>(Lens<Refraction, Resampling>.New(static state => state.Resampling, static resampling => state => state with { Resampling = resampling })),
        new("Resampling", "Source filter and edge extension"));

    public StateParameter<Refraction> Kind { get; }
    public ParameterText Text { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanoramaProjection {
    public abstract Projection Of(Projection.Rectilinear rendering);

    public sealed record Rectilinear : PanoramaProjection, IStateRecord<Rectilinear> {
        public static Rectilinear Default { get; } = new();

        public override Projection Of(Projection.Rectilinear rendering) => rendering;
    }

    public sealed record Equirectangular : PanoramaProjection, IStateRecord<Equirectangular> {
        public static Equirectangular Default { get; } = new();

        public override Projection Of(Projection.Rectilinear rendering) => new Projection.Equirectangular(rendering.Extent);
    }

    public sealed record Cylindrical(SweepAngle Sweep, CylinderHeight Height) : PanoramaProjection, IStateRecord<Cylindrical, CylindricalParameter> {
        public static Cylindrical Default { get; } = new(SweepAngle.HalfTurn, CylinderHeight.Neutral);

        public override Projection Of(Projection.Rectilinear rendering) => new Projection.Cylindrical(rendering.Extent, ViewWindow.Centered(Sweep, rendering.Extent).Raised(Height));
    }

    public sealed record Fisheye(SweepAngle Sweep) : PanoramaProjection, IStateRecord<Fisheye, FisheyeParameter> {
        public static Fisheye Default { get; } = new(SweepAngle.HalfTurn);

        public override Projection Of(Projection.Rectilinear rendering) => new Projection.Fisheye(rendering.Extent, Sweep);
    }

    public sealed record Equisolid(SweepAngle Sweep, FullTurnRadius FullTurn) : PanoramaProjection, IStateRecord<Equisolid, EquisolidParameter> {
        public static Equisolid Default { get; } = new(SweepAngle.HalfTurn, FullTurnRadius.Standard);

        public override Projection Of(Projection.Rectilinear rendering) => new Projection.Equisolid(rendering.Extent, Sweep, FullTurn);
    }

    public sealed record MirrorBall : PanoramaProjection, IStateRecord<MirrorBall> {
        public static MirrorBall Default { get; } = new();

        public override Projection Of(Projection.Rectilinear rendering) => new Projection.MirrorBall(rendering.Extent);
    }

    public sealed record Stereographic(SweepAngle Sweep) : PanoramaProjection, IStateRecord<Stereographic, StereographicParameter> {
        public static Stereographic Default { get; } = new(SweepAngle.HalfTurn);

        public override Projection Of(Projection.Rectilinear rendering) => new Projection.Stereographic(rendering.Extent, Sweep);
    }

    public sealed record Panini(FieldOfView Field, Magnitude Distance, Fraction Compression) : PanoramaProjection, IStateRecord<Panini, PaniniParameter> {
        public static Panini Default { get; } = new(FieldOfView.SixthTurn, Magnitude.Unit, Fraction.MinValue);

        public override Projection Of(Projection.Rectilinear rendering) => new Projection.Panini(rendering.Extent, Field, Distance, Compression);
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CylindricalParameter : IStateParameter<PanoramaProjection.Cylindrical> {
    public static readonly CylindricalParameter Sweep = new("sweep", StateParameter.Bounded(Lens<PanoramaProjection.Cylindrical, SweepAngle>.New(static output => output.Sweep, static sweep => output => output with { Sweep = sweep }), Presentations.Angle),
        new("Sweep", "Angle the output spans across frame"));
    public static readonly CylindricalParameter Height = new("height", StateParameter.Bounded(Lens<PanoramaProjection.Cylindrical, CylinderHeight>.New(static output => output.Height, static height => output => output with { Height = height }),
        new Presentation<float> { Soft = (-10f, 10f), Origin = 0f }),
        new("Height", "Viewing window vertical offset"));

    public StateParameter<PanoramaProjection.Cylindrical> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FisheyeParameter : IStateParameter<PanoramaProjection.Fisheye> {
    public static readonly FisheyeParameter Sweep = new("sweep", StateParameter.Bounded(Lens<PanoramaProjection.Fisheye, SweepAngle>.New(static output => output.Sweep, static sweep => output => output with { Sweep = sweep }), Presentations.Angle),
        new("Sweep", "Angle the output spans across frame"));

    public StateParameter<PanoramaProjection.Fisheye> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EquisolidParameter : IStateParameter<PanoramaProjection.Equisolid> {
    public static readonly EquisolidParameter Sweep = new("sweep", StateParameter.Bounded(Lens<PanoramaProjection.Equisolid, SweepAngle>.New(static output => output.Sweep, static sweep => output => output with { Sweep = sweep }), Presentations.Angle),
        new("Sweep", "Angle the output spans across frame"));
    public static readonly EquisolidParameter FullTurn = new("full-turn", StateParameter.Bounded(Lens<PanoramaProjection.Equisolid, FullTurnRadius>.New(static output => output.FullTurn, static fullTurn => output => output with { FullTurn = fullTurn }),
        new Presentation<float> { Soft = (2f * 0.01f / 12f, 2f * 15f / 12f), Track = Some<Func<AxisRange, Axis>>(static range => new Axis.Log(range, 10f)) }),
        new("Full turn radius", "Full-turn radius as a share of the inscribed circle"));

    public StateParameter<PanoramaProjection.Equisolid> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StereographicParameter : IStateParameter<PanoramaProjection.Stereographic> {
    public static readonly StereographicParameter Sweep = new("sweep", StateParameter.Bounded(Lens<PanoramaProjection.Stereographic, SweepAngle>.New(static output => output.Sweep, static sweep => output => output with { Sweep = sweep }), Presentations.Angle),
        new("Sweep", "Angle the output spans across frame"));

    public StateParameter<PanoramaProjection.Stereographic> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PaniniParameter : IStateParameter<PanoramaProjection.Panini> {
    public static readonly PaniniParameter Field = new("field", StateParameter.Bounded(Lens<PanoramaProjection.Panini, FieldOfView>.New(static output => output.Field, static field => output => output with { Field = field }), Presentations.Angle),
        new("Field of view", "Horizontal angle the output spans across frame"));
    public static readonly PaniniParameter Distance = new("distance", StateParameter.Bounded(Lens<PanoramaProjection.Panini, Magnitude>.New(static output => output.Distance, static distance => output => output with { Distance = distance }),
        Magnitude.Presentation with { Soft = (0f, 1f) }),
        new("Distance", "Projection distance, 0 rectilinear and 1 Panini"));
    public static readonly PaniniParameter Compression = new("compression", StateParameter.Bounded(Lens<PanoramaProjection.Panini, Fraction>.New(static output => output.Compression, static compression => output => output with { Compression = compression })),
        new("Compression", "Vertical compression straightening verticals"));

    public StateParameter<PanoramaProjection.Panini> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Panorama(PanoramaProjection Output, ViewAngles View, Resampling Resampling) : IStateRecord<Panorama, PanoramaParameter>, IPixelStage<Panorama> {
    public static Panorama Default { get; } = new(PanoramaProjection.Rectilinear.Default, ViewAngles.Default, Resampling.Default);

    public static Option<PixelPass> Pass(Panorama state, PassContext context) =>
        from camera in context.Camera
        where state.Output is not PanoramaProjection.Rectilinear || state.View != ViewAngles.Default
        from map in camera.Panorama(state.Output.Of, state.View.View, state.Resampling.Sampling, state.Resampling.Edges)
        select (PixelPass)state.Resampling.Pass(map);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanoramaParameter : IStateParameter<Panorama> {
    public static readonly PanoramaParameter Output = new("output", new StateParameter<Panorama>.Variant<PanoramaProjection>(Lens<Panorama, PanoramaProjection>.New(static state => state.Output, static output => state => state with { Output = output })),
        new("Projection", "Projection the rendered view is placed into"));
    public static readonly PanoramaParameter View = new("view", new StateParameter<Panorama>.Record<ViewAngles>(Lens<Panorama, ViewAngles>.New(static state => state.View, static view => state => state with { View = view })),
        new("View", "Rendered view orientation"));
    public static readonly PanoramaParameter Resampling = new("resampling", new StateParameter<Panorama>.Record<Resampling>(Lens<Panorama, Resampling>.New(static state => state.Resampling, static resampling => state => state with { Resampling = resampling })),
        new("Resampling", "Source filter and edge extension"));

    public StateParameter<Panorama> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record PolarCoordinates(PolarConversion Conversion, Resampling Resampling) : IStateRecord<PolarCoordinates, PolarCoordinatesParameter>, IPixelStage<PolarCoordinates> {
    public static PolarCoordinates Default { get; } = new(PolarConversion.RectangularToPolar, Resampling.Default);

    public static Option<PixelPass> Pass(PolarCoordinates state, PassContext context) =>
        Some<PixelPass>(state.Resampling.Pass(state.Conversion.Map(state.Resampling.Sampling, state.Resampling.Edges)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PolarCoordinatesParameter : IStateParameter<PolarCoordinates> {
    public static readonly PolarCoordinatesParameter Conversion = new("conversion", new StateParameter<PolarCoordinates>.Choice<PolarConversion>(Lens<PolarCoordinates, PolarConversion>.New(static state => state.Conversion, static conversion => state => state with { Conversion = conversion })),
        new("Conversion", "Direction the frame converts between rows and rings"));
    public static readonly PolarCoordinatesParameter Resampling = new("resampling", new StateParameter<PolarCoordinates>.Record<Resampling>(Lens<PolarCoordinates, Resampling>.New(static state => state.Resampling, static resampling => state => state with { Resampling = resampling })),
        new("Resampling", "Source filter and edge extension"));

    public StateParameter<PolarCoordinates> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Kaleidoscope(MirrorCount Mirrors, SignedAngle Rotation, Position Center, Factor Zoom, Resampling Resampling) : IStateRecord<Kaleidoscope, KaleidoscopeParameter>, IPixelStage<Kaleidoscope> {
    public static Kaleidoscope Default { get; } = new(MirrorCount.Create(3), SignedAngle.Up, Position.Default, Factor.Unit, Resampling.Default with { Wrap = WrapMode.Mirror });

    public static Option<PixelPass> Pass(Kaleidoscope state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
            ValidationResults.ToFin(MirrorFold.Validate(state.Mirrors, state.Rotation, new FramePosition(state.Center.X, state.Center.Y), state.Zoom, out MirrorFold? fold), fold)
                .Bind(admitted => admitted.Map(state.Resampling.Sampling, state.Resampling.Edges).Apply(frame, progress))));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KaleidoscopeParameter : IStateParameter<Kaleidoscope> {
    public static readonly KaleidoscopeParameter Mirrors = new("mirrors", StateParameter.Bounded(Lens<Kaleidoscope, MirrorCount>.New(static state => state.Mirrors, static mirrors => state => state with { Mirrors = mirrors })),
        new("Mirrors", "Mirror lines through the center, drawing twice as many wedges"));
    public static readonly KaleidoscopeParameter Rotation = new("rotation", StateParameter.Bounded(Lens<Kaleidoscope, SignedAngle>.New(static state => state.Rotation, static rotation => state => state with { Rotation = rotation }), Presentations.Angle),
        new("Angle", "First mirror line turn, counterclockwise"));
    public static readonly KaleidoscopeParameter Center = new("center", new StateParameter<Kaleidoscope>.Record<Position>(Lens<Kaleidoscope, Position>.New(static state => state.Center, static center => state => state with { Center = center })),
        new("Center", "Point the warp centers on"));
    public static readonly KaleidoscopeParameter Zoom = new("zoom", StateParameter.Bounded(Lens<Kaleidoscope, Factor>.New(static state => state.Zoom, static zoom => state => state with { Zoom = zoom }), Factor.Presentation with { Soft = (1f / 16f, 16f) }),
        new("Zoom", "Wedge source magnification about the center"));
    public static readonly KaleidoscopeParameter Resampling = new("resampling", new StateParameter<Kaleidoscope>.Record<Resampling>(Lens<Kaleidoscope, Resampling>.New(static state => state.Resampling, static resampling => state => state with { Resampling = resampling })),
        new("Resampling", "Source filter and edge extension"));

    public StateParameter<Kaleidoscope> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Shake(ShortSideLength AmplitudeX, ShortSideLength AmplitudeY, SignedAngle Rotation, Factor Zoom, Hertz Rate, Fractal Fractal, Seed Seed, Timing Timing, Hold Hold)
    : IStateRecord<Shake, ShakeParameter> {
    private const uint Stream = 11u;

    public static Shake Default { get; } = new(ShortSideLength.Neutral, ShortSideLength.Neutral, SignedAngle.Neutral, Factor.Unit, Hertz.Create(5f), Fractal.Default, Seed.MinValue, Timing.Default, Hold.MinValue);

    public bool Still => (AmplitudeX, AmplitudeY, Rotation, Zoom) == (Default.AmplitudeX, Default.AmplitudeY, Default.Rotation, Default.Zoom);

    public ShakePath Path => new((AmplitudeX, AmplitudeY), Rotation, Zoom, Rate, Fractal.Octaves, Seed.Field(Stream, 0u));

    public double Opened(PassContext context) => Hold.Held(Timing.At(context));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShakeParameter : IStateParameter<Shake> {
    public static readonly ShakeParameter AmplitudeX = new("amplitude-x", StateParameter.Bounded(Lens<Shake, ShortSideLength>.New(static state => state.AmplitudeX, static amplitude => state => state with { AmplitudeX = amplitude }),
        Presentations.Length with { Soft = (0f, 0.1f) }),
        new("Amplitude X", "Largest horizontal shake in frame short sides"));
    public static readonly ShakeParameter AmplitudeY = new("amplitude-y", StateParameter.Bounded(Lens<Shake, ShortSideLength>.New(static state => state.AmplitudeY, static amplitude => state => state with { AmplitudeY = amplitude }),
        Presentations.Length with { Soft = (0f, 0.1f) }),
        new("Amplitude Y", "Largest vertical shake in frame short sides"));
    public static readonly ShakeParameter Rotation = new("rotation", StateParameter.Bounded(Lens<Shake, SignedAngle>.New(static state => state.Rotation, static rotation => state => state with { Rotation = rotation }),
        Presentations.Angle with { Soft = (0f, float.Pi) }),
        new("Rotation", "Largest shake turn"));
    public static readonly ShakeParameter Zoom = new("zoom", StateParameter.Bounded(Lens<Shake, Factor>.New(static state => state.Zoom, static zoom => state => state with { Zoom = zoom }), Factor.Presentation with { Soft = (1f, 2f) }),
        new("Zoom", "Largest shake zoom factor, in and out alike"));
    public static readonly ShakeParameter Rate = new("rate", StateParameter.Bounded(Lens<Shake, Hertz>.New(static state => state.Rate, static rate => state => state with { Rate = rate }), Hertz.Presentation with { Soft = (0.1f, 20f) }),
        new("Frequency", "Shakes per second"));
    public static readonly ShakeParameter Fractal = new("octaves", new StateParameter<Shake>.Record<Fractal>(Lens<Shake, Fractal>.New(static state => state.Fractal, static fractal => state => state with { Fractal = fractal })),
        new("Octaves", "Shake detail across scales"));
    public static readonly ShakeParameter Seed = new("seed", StateParameter.Bounded(Lens<Shake, Seed>.New(static state => state.Seed, static seed => state => state with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly ShakeParameter Timing = new("timing", new StateParameter<Shake>.Record<Timing>(Lens<Shake, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })),
        new("Timing", "Stored time and rate of recorded frame time"));
    public static readonly ShakeParameter Hold = new("hold", StateParameter.Bounded(Lens<Shake, Hold>.New(static state => state.Hold, static hold => state => state with { Hold = hold })),
        new("Hold", "Seconds between pattern redraws"));

    public StateParameter<Shake> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record ShutterMotion(Shake Shake, ShutterTime Shutter, Fraction Position) : IStateRecord<ShutterMotion, ShutterMotionParameter> {
    public static ShutterMotion Default { get; } = new(Shake.Default, ShutterTime.Instant, Fraction.Create(0.5f));

    public bool Open => Shutter != ShutterTime.Instant;

    public Func<double, Affine> Path(Pose motion, Affine placed, PassContext context) =>
        (Shutter, Position, Shake.Opened(context) - (Shutter * Position), Shake.Path) switch {
            var (duration, position, opened, shake) => share =>
                motion.Partial(share - position).Inverse.Then(shake.Posed(context.Extent, opened + (duration * share)).Inverse).Then(placed),
        };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShutterMotionParameter : IStateParameter<ShutterMotion> {
    public static readonly ShutterMotionParameter Shake = new("shake", new StateParameter<ShutterMotion>.Record<Shake>(Lens<ShutterMotion, Shake>.New(static motion => motion.Shake, static shake => motion => motion with { Shake = shake })),
        new("Shake", "Seeded camera motion over time"));
    public static readonly ShutterMotionParameter Shutter = new("shutter", StateParameter.Bounded(Lens<ShutterMotion, ShutterTime>.New(static motion => motion.Shutter, static shutter => motion => motion with { Shutter = shutter })),
        new("Shutter", "Seconds the exposure samples along its motion"));
    public static readonly ShutterMotionParameter Position = new("shutter-position", StateParameter.Bounded(Lens<ShutterMotion, Fraction>.New(static motion => motion.Position, static position => motion => motion with { Position = position })),
        new("Shutter position", "Exposure share before current time, 0% opening and 100% closing now"));

    public StateParameter<ShutterMotion> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record AffineTransform(
    ShortSideOffset OffsetX, ShortSideOffset OffsetY, SignedAngle Rotation, Factor ScaleX, Factor ScaleY, SkewAngle SkewX, SkewAngle SkewY, Position Pivot,
    ShortSideOffset TravelX, ShortSideOffset TravelY, SignedAngle Turn, Factor Zoom, ShutterMotion Motion, Resampling Resampling)
    : IStateRecord<AffineTransform, AffineTransformParameter>, IPixelStage<AffineTransform> {
    public static AffineTransform Default { get; } = new(
        ShortSideOffset.Neutral, ShortSideOffset.Neutral, SignedAngle.Neutral, Factor.Unit, Factor.Unit, SkewAngle.Neutral, SkewAngle.Neutral, Position.Default,
        ShortSideOffset.Neutral, ShortSideOffset.Neutral, SignedAngle.Neutral, Factor.Unit, ShutterMotion.Default, Resampling.Default);

    public static Option<PixelPass> Pass(AffineTransform state, PassContext context) =>
        (Pivot: state.Pivot.Point(context.Extent), Side: context.Extent.ShortSide) switch {
            var (pivot, side) => state.Motion.Path(
                new Pose((ERational.FromSingle(state.TravelX).Multiply(side), ERational.FromSingle(state.TravelY).Multiply(side)), state.Turn, (state.Zoom, state.Zoom), (0d, 0d), pivot),
                new Pose((ERational.FromSingle(state.OffsetX).Multiply(side), ERational.FromSingle(state.OffsetY).Multiply(side)), state.Rotation, (state.ScaleX, state.ScaleY), (state.SkewX, state.SkewY), pivot).Inverse,
                context) switch {
                    var path when (state.TravelX, state.TravelY, state.Turn, state.Zoom) != (Default.TravelX, Default.TravelY, Default.Turn, Default.Zoom) || (state.Motion.Open && !state.Motion.Shake.Still) =>
                        Some<PixelPass>(Affine.Swept(path, state.Resampling.Sampling, state.Resampling.Wrap)),
                    var path => path(0d) switch {
                        { Still: true } => None,
                        var still => Some<PixelPass>(state.Resampling.Pass(still.Map(state.Resampling.Sampling, state.Resampling.Wrap))),
                    },
                },
        };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AffineTransformParameter : IStateParameter<AffineTransform> {
    public static readonly AffineTransformParameter OffsetX = new("offset-x", StateParameter.Bounded(Lens<AffineTransform, ShortSideOffset>.New(static state => state.OffsetX, static offset => state => state with { OffsetX = offset }), Presentations.Offset),
        new("X", "Horizontal move in frame short sides"));
    public static readonly AffineTransformParameter OffsetY = new("offset-y", StateParameter.Bounded(Lens<AffineTransform, ShortSideOffset>.New(static state => state.OffsetY, static offset => state => state with { OffsetY = offset }), Presentations.Offset),
        new("Y", "Downward move in frame short sides"));
    public static readonly AffineTransformParameter Rotation = new("rotation", StateParameter.Bounded(Lens<AffineTransform, SignedAngle>.New(static state => state.Rotation, static rotation => state => state with { Rotation = rotation }), Presentations.Angle),
        new("Angle", "Counterclockwise turn about pivot"));
    public static readonly AffineTransformParameter ScaleX = new("scale-x", StateParameter.Bounded(Lens<AffineTransform, Factor>.New(static state => state.ScaleX, static scale => state => state with { ScaleX = scale }), Factor.Presentation with { Soft = (1f / 16f, 16f) }),
        new("Scale X", "Horizontal scale about pivot"));
    public static readonly AffineTransformParameter ScaleY = new("scale-y", StateParameter.Bounded(Lens<AffineTransform, Factor>.New(static state => state.ScaleY, static scale => state => state with { ScaleY = scale }), Factor.Presentation with { Soft = (1f / 16f, 16f) }),
        new("Scale Y", "Vertical scale about pivot"));
    public static readonly AffineTransformParameter SkewX = new("skew-x", StateParameter.Bounded(Lens<AffineTransform, SkewAngle>.New(static state => state.SkewX, static skew => state => state with { SkewX = skew })),
        new("Skew X", "Horizontal shear about pivot"));
    public static readonly AffineTransformParameter SkewY = new("skew-y", StateParameter.Bounded(Lens<AffineTransform, SkewAngle>.New(static state => state.SkewY, static skew => state => state with { SkewY = skew })),
        new("Skew Y", "Vertical shear about pivot"));
    public static readonly AffineTransformParameter Pivot = new("pivot", new StateParameter<AffineTransform>.Record<Position>(Lens<AffineTransform, Position>.New(static state => state.Pivot, static pivot => state => state with { Pivot = pivot })),
        new("Pivot", "Point the frame turns, scales, and shears about"));
    public static readonly AffineTransformParameter TravelX = new("travel-x", StateParameter.Bounded(Lens<AffineTransform, ShortSideOffset>.New(static state => state.TravelX, static travel => state => state with { TravelX = travel }), Presentations.Offset),
        new("Travel X", "Horizontal move across exposure, blurring along it"));
    public static readonly AffineTransformParameter TravelY = new("travel-y", StateParameter.Bounded(Lens<AffineTransform, ShortSideOffset>.New(static state => state.TravelY, static travel => state => state with { TravelY = travel }), Presentations.Offset),
        new("Travel Y", "Downward move across exposure, blurring along it"));
    public static readonly AffineTransformParameter Turn = new("turn", StateParameter.Bounded(Lens<AffineTransform, SignedAngle>.New(static state => state.Turn, static turn => state => state with { Turn = turn }), Presentations.Angle),
        new("Spin", "Counterclockwise turn about pivot across exposure"));
    public static readonly AffineTransformParameter Zoom = new("zoom", StateParameter.Bounded(Lens<AffineTransform, Factor>.New(static state => state.Zoom, static zoom => state => state with { Zoom = zoom }), Factor.Presentation with { Soft = (1f / 16f, 16f) }),
        new("Zoom", "Scale about pivot at exposure end"));
    public static readonly AffineTransformParameter Motion = new("exposure", new StateParameter<AffineTransform>.Record<ShutterMotion>(Lens<AffineTransform, ShutterMotion>.New(static state => state.Motion, static motion => state => state with { Motion = motion })),
        new("Exposure", "Camera shake and the shutter sampling motion"));
    public static readonly AffineTransformParameter Resampling = new("resampling", new StateParameter<AffineTransform>.Record<Resampling>(Lens<AffineTransform, Resampling>.New(static state => state.Resampling, static resampling => state => state with { Resampling = resampling })),
        new("Resampling", "Source filter and edge extension"));

    public StateParameter<AffineTransform> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record CameraShake(ShutterMotion Motion, Resampling Resampling) : IStateRecord<CameraShake, CameraShakeParameter>, IPixelStage<CameraShake> {
    public static CameraShake Default { get; } = new(ShutterMotion.Default, Resampling.Default with { Wrap = WrapMode.Periodic });

    public static Option<PixelPass> Pass(CameraShake state, PassContext context) =>
        AffineTransform.Pass(AffineTransform.Default with { Motion = state.Motion, Resampling = state.Resampling }, context);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CameraShakeParameter : IStateParameter<CameraShake> {
    public static readonly CameraShakeParameter Motion = new("exposure", new StateParameter<CameraShake>.Record<ShutterMotion>(Lens<CameraShake, ShutterMotion>.New(static state => state.Motion, static motion => state => state with { Motion = motion })),
        new("Exposure", "Camera shake and the shutter sampling motion"));
    public static readonly CameraShakeParameter Resampling = new("resampling", new StateParameter<CameraShake>.Record<Resampling>(Lens<CameraShake, Resampling>.New(static state => state.Resampling, static resampling => state => state with { Resampling = resampling })),
        new("Resampling", "Source filter and edge extension"));

    public StateParameter<CameraShake> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record CornerPin(Position TopLeft, Position TopRight, Position BottomRight, Position BottomLeft, Resampling Resampling) : IStateRecord<CornerPin, CornerPinParameter>, IPixelStage<CornerPin> {
    public static CornerPin Default { get; } = new(new(FrameAxis.Start, FrameAxis.Start), new(FrameAxis.End, FrameAxis.Start), new(FrameAxis.End, FrameAxis.End), new(FrameAxis.Start, FrameAxis.End), Resampling.Default);

    public static Option<PixelPass> Pass(CornerPin state, PassContext context) =>
        state with { Resampling = Default.Resampling } == Default ? None
            : ValidationResults.ToFin(FrameQuad.Validate(
                    state.TopLeft.Point(context.Extent), state.TopRight.Point(context.Extent), state.BottomRight.Point(context.Extent), state.BottomLeft.Point(context.Extent),
                    out FrameQuad? quad), quad)
                .Match(
                    Succ: admitted => Some<PixelPass>(state.Resampling.Pass(admitted.Pinned(state.Resampling.Sampling, state.Resampling.Edges))),
                    Fail: static error => Some<PixelPass>(new PixelPass.Frame((_, _) => error)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CornerPinParameter : IStateParameter<CornerPin> {
    public static readonly CornerPinParameter TopLeft = new("top-left", new StateParameter<CornerPin>.Record<Position>(Lens<CornerPin, Position>.New(static state => state.TopLeft, static corner => state => state with { TopLeft = corner })),
        new("Upper left", "Upper left corner position in frame"));
    public static readonly CornerPinParameter TopRight = new("top-right", new StateParameter<CornerPin>.Record<Position>(Lens<CornerPin, Position>.New(static state => state.TopRight, static corner => state => state with { TopRight = corner })),
        new("Upper right", "Upper right corner position in frame"));
    public static readonly CornerPinParameter BottomRight = new("bottom-right", new StateParameter<CornerPin>.Record<Position>(Lens<CornerPin, Position>.New(static state => state.BottomRight, static corner => state => state with { BottomRight = corner })),
        new("Lower right", "Lower right corner position in frame"));
    public static readonly CornerPinParameter BottomLeft = new("bottom-left", new StateParameter<CornerPin>.Record<Position>(Lens<CornerPin, Position>.New(static state => state.BottomLeft, static corner => state => state with { BottomLeft = corner })),
        new("Lower left", "Lower left corner position in frame"));
    public static readonly CornerPinParameter Resampling = new("resampling", new StateParameter<CornerPin>.Record<Resampling>(Lens<CornerPin, Resampling>.New(static state => state.Resampling, static resampling => state => state with { Resampling = resampling })),
        new("Resampling", "Source filter and edge extension"));

    public StateParameter<CornerPin> Kind { get; }
    public ParameterText Text { get; }
}
