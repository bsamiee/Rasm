using System.Numerics;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;
using TinyEXR;

namespace Rasm.Imaging.Grade.Balance;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Zero", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGrade>]
public readonly partial struct MixerWeight : IMinMaxValue<MixerWeight> {
    public static MixerWeight MinValue { get; } = new(-float.MaxValue);
    public static MixerWeight MaxValue { get; } = new(float.MaxValue);
    public static MixerWeight Unit { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidGrade? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGrade();
}

public sealed record MixerState(
    MixerWeight RedFromRed, MixerWeight RedFromGreen, MixerWeight RedFromBlue,
    MixerWeight GreenFromRed, MixerWeight GreenFromGreen, MixerWeight GreenFromBlue,
    MixerWeight BlueFromRed, MixerWeight BlueFromGreen, MixerWeight BlueFromBlue)
    : IStateRecord<MixerState, MixerParameter, InvalidGrade>, IPixelStage<MixerState> {
    public static MixerState Default { get; } = new(
        MixerWeight.Unit, MixerWeight.Zero, MixerWeight.Zero,
        MixerWeight.Zero, MixerWeight.Unit, MixerWeight.Zero,
        MixerWeight.Zero, MixerWeight.Zero, MixerWeight.Unit);

    public LutTable.Affine Table => new(
        new ColorMatrix3x3(RedFromRed, RedFromGreen, RedFromBlue, GreenFromRed, GreenFromGreen, GreenFromBlue, BlueFromRed, BlueFromGreen, BlueFromBlue),
        Vector3.Zero);

    public static Option<PixelPass> Pass(MixerState state, PassContext context) =>
        state == Default ? None : Some<PixelPass>(new PixelPass.Color(state.Table));
}

[SmartEnum<string>]
[ValidationError<InvalidGrade>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MixerParameter : IStateParameter<MixerState> {
    public static readonly MixerParameter RedFromRed = Weight("red-from-red", (0, 0), static state => state.RedFromRed, static weight => state => state with { RedFromRed = weight });
    public static readonly MixerParameter RedFromGreen = Weight("red-from-green", (0, 1), static state => state.RedFromGreen, static weight => state => state with { RedFromGreen = weight });
    public static readonly MixerParameter RedFromBlue = Weight("red-from-blue", (0, 2), static state => state.RedFromBlue, static weight => state => state with { RedFromBlue = weight });
    public static readonly MixerParameter GreenFromRed = Weight("green-from-red", (1, 0), static state => state.GreenFromRed, static weight => state => state with { GreenFromRed = weight });
    public static readonly MixerParameter GreenFromGreen = Weight("green-from-green", (1, 1), static state => state.GreenFromGreen, static weight => state => state with { GreenFromGreen = weight });
    public static readonly MixerParameter GreenFromBlue = Weight("green-from-blue", (1, 2), static state => state.GreenFromBlue, static weight => state => state with { GreenFromBlue = weight });
    public static readonly MixerParameter BlueFromRed = Weight("blue-from-red", (2, 0), static state => state.BlueFromRed, static weight => state => state with { BlueFromRed = weight });
    public static readonly MixerParameter BlueFromGreen = Weight("blue-from-green", (2, 1), static state => state.BlueFromGreen, static weight => state => state with { BlueFromGreen = weight });
    public static readonly MixerParameter BlueFromBlue = Weight("blue-from-blue", (2, 2), static state => state.BlueFromBlue, static weight => state => state with { BlueFromBlue = weight });

    public StateParameter<MixerState> Kind { get; }
    public (int Output, int Input) Cell { get; }

    private static MixerParameter Weight(string key, (int Output, int Input) cell, Func<MixerState, MixerWeight> get, Func<MixerWeight, Func<MixerState, MixerState>> set) =>
        new(key, new StateParameter<MixerState>.Bounded<MixerWeight, float, InvalidGrade>(Lens<MixerState, MixerWeight>.New(get, set), new() { Soft = (-2f, 2f), Origin = (float)get(MixerState.Default) }), cell);
}
