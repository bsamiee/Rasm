using System.Runtime.CompilerServices;
using System.Text.Json;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record MixerState(Channels<Channels<SignedMagnitude>> Weights) : IStateRecord<MixerState, MixerParameter>, IPixelStage<MixerState> {
    public static MixerState Default { get; } = new(new Channels<Channels<SignedMagnitude>>(
        new(SignedMagnitude.Unit, SignedMagnitude.Zero, SignedMagnitude.Zero),
        new(SignedMagnitude.Zero, SignedMagnitude.Unit, SignedMagnitude.Zero),
        new(SignedMagnitude.Zero, SignedMagnitude.Zero, SignedMagnitude.Unit)));

    public LutTable.Affine Table =>
        new(Matrix<double>.Build.DenseOfRowArrays(Seq(Weights.Red, Weights.Green, Weights.Blue).Map(static row => new double[] { row.Red, row.Green, row.Blue })));

    public static Option<PixelPass> Pass(MixerState state, PassContext context) =>
        state == Default ? None : Some<PixelPass>(new PixelPass.Color(state.Table));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MixerParameter : IStateParameter<MixerState> {
    public static readonly MixerParameter RedFromRed = Weight(Channels<Channels<SignedMagnitude>>.RedOf, Channels<SignedMagnitude>.RedOf,
        new("Red from red", "Weight of input red in output red"));
    public static readonly MixerParameter RedFromGreen = Weight(Channels<Channels<SignedMagnitude>>.RedOf, Channels<SignedMagnitude>.GreenOf,
        new("Red from green", "Weight of input green in output red"));
    public static readonly MixerParameter RedFromBlue = Weight(Channels<Channels<SignedMagnitude>>.RedOf, Channels<SignedMagnitude>.BlueOf,
        new("Red from blue", "Weight of input blue in output red"));
    public static readonly MixerParameter GreenFromRed = Weight(Channels<Channels<SignedMagnitude>>.GreenOf, Channels<SignedMagnitude>.RedOf,
        new("Green from red", "Weight of input red in output green"));
    public static readonly MixerParameter GreenFromGreen = Weight(Channels<Channels<SignedMagnitude>>.GreenOf, Channels<SignedMagnitude>.GreenOf,
        new("Green from green", "Weight of input green in output green"));
    public static readonly MixerParameter GreenFromBlue = Weight(Channels<Channels<SignedMagnitude>>.GreenOf, Channels<SignedMagnitude>.BlueOf,
        new("Green from blue", "Weight of input blue in output green"));
    public static readonly MixerParameter BlueFromRed = Weight(Channels<Channels<SignedMagnitude>>.BlueOf, Channels<SignedMagnitude>.RedOf,
        new("Blue from red", "Weight of input red in output blue"));
    public static readonly MixerParameter BlueFromGreen = Weight(Channels<Channels<SignedMagnitude>>.BlueOf, Channels<SignedMagnitude>.GreenOf,
        new("Blue from green", "Weight of input green in output blue"));
    public static readonly MixerParameter BlueFromBlue = Weight(Channels<Channels<SignedMagnitude>>.BlueOf, Channels<SignedMagnitude>.BlueOf,
        new("Blue from blue", "Weight of input blue in output blue"));

    public StateParameter<MixerState> Kind { get; }
    public ParameterText Text { get; }

    private static MixerParameter Weight(
        Lens<Channels<Channels<SignedMagnitude>>, Channels<SignedMagnitude>> output, Lens<Channels<SignedMagnitude>, SignedMagnitude> input, ParameterText text, [CallerMemberName] string member = "") =>
        lens(Lens<MixerState, Channels<Channels<SignedMagnitude>>>.New(static state => state.Weights, static weights => state => state with { Weights = weights }), output, input) switch {
            var weight => new(JsonNamingPolicy.KebabCaseLower.ConvertName(member), StateParameter.Bounded(weight, new Presentation<float> { Soft = (-2f, 2f), Origin = (float)weight.Get(MixerState.Default) }), text),
        };
}
