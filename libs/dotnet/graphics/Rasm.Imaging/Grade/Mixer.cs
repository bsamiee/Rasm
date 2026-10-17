using System.Runtime.CompilerServices;
using System.Text.Json;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record MixerState(
    SignedMagnitude RedFromRed, SignedMagnitude RedFromGreen, SignedMagnitude RedFromBlue,
    SignedMagnitude GreenFromRed, SignedMagnitude GreenFromGreen, SignedMagnitude GreenFromBlue,
    SignedMagnitude BlueFromRed, SignedMagnitude BlueFromGreen, SignedMagnitude BlueFromBlue)
    : IStateRecord<MixerState, MixerParameter>, IPixelStage<MixerState> {
    public static MixerState Default { get; } = new(
        SignedMagnitude.Unit, SignedMagnitude.Zero, SignedMagnitude.Zero,
        SignedMagnitude.Zero, SignedMagnitude.Unit, SignedMagnitude.Zero,
        SignedMagnitude.Zero, SignedMagnitude.Zero, SignedMagnitude.Unit);

    public LutTable.Affine Table => new(
        Matrix<double>.Build.DenseOfRowMajor(3, 3, [
            RedFromRed, RedFromGreen, RedFromBlue,
            GreenFromRed, GreenFromGreen, GreenFromBlue,
            BlueFromRed, BlueFromGreen, BlueFromBlue,
        ]));

    public static Option<PixelPass> Pass(MixerState state, PassContext context) =>
        state == Default ? None : Some<PixelPass>(new PixelPass.Color(state.Table));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MixerParameter : IStateParameter<MixerState> {
    public static readonly MixerParameter RedFromRed = Weight(static state => state.RedFromRed, static weight => state => state with { RedFromRed = weight },
        new("Red from red", "Weight of input red in output red"));
    public static readonly MixerParameter RedFromGreen = Weight(static state => state.RedFromGreen, static weight => state => state with { RedFromGreen = weight },
        new("Red from green", "Weight of input green in output red"));
    public static readonly MixerParameter RedFromBlue = Weight(static state => state.RedFromBlue, static weight => state => state with { RedFromBlue = weight },
        new("Red from blue", "Weight of input blue in output red"));
    public static readonly MixerParameter GreenFromRed = Weight(static state => state.GreenFromRed, static weight => state => state with { GreenFromRed = weight },
        new("Green from red", "Weight of input red in output green"));
    public static readonly MixerParameter GreenFromGreen = Weight(static state => state.GreenFromGreen, static weight => state => state with { GreenFromGreen = weight },
        new("Green from green", "Weight of input green in output green"));
    public static readonly MixerParameter GreenFromBlue = Weight(static state => state.GreenFromBlue, static weight => state => state with { GreenFromBlue = weight },
        new("Green from blue", "Weight of input blue in output green"));
    public static readonly MixerParameter BlueFromRed = Weight(static state => state.BlueFromRed, static weight => state => state with { BlueFromRed = weight },
        new("Blue from red", "Weight of input red in output blue"));
    public static readonly MixerParameter BlueFromGreen = Weight(static state => state.BlueFromGreen, static weight => state => state with { BlueFromGreen = weight },
        new("Blue from green", "Weight of input green in output blue"));
    public static readonly MixerParameter BlueFromBlue = Weight(static state => state.BlueFromBlue, static weight => state => state with { BlueFromBlue = weight },
        new("Blue from blue", "Weight of input blue in output blue"));

    public StateParameter<MixerState> Kind { get; }
    public ParameterText Text { get; }

    private static MixerParameter Weight(Func<MixerState, SignedMagnitude> get, Func<SignedMagnitude, Func<MixerState, MixerState>> set, ParameterText text, [CallerMemberName] string member = "") =>
        new(JsonNamingPolicy.KebabCaseLower.ConvertName(member), StateParameter.Bounded(Lens<MixerState, SignedMagnitude>.New(get, set), new Presentation<float> { Soft = (-2f, 2f), Origin = (float)get(MixerState.Default) }), text);
}
