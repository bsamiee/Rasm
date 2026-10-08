using System.Numerics;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using TinyEXR;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGrade>]
public readonly partial struct BlackLevel : IMinMaxValue<BlackLevel> {
    public static BlackLevel MinValue { get; } = new(-float.MaxValue);
    public static BlackLevel MaxValue { get; } = new(float.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidGrade? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGrade();
}

public sealed record Levels(BlackLevel Black, Exposure White) : IStateRecord<Levels, LevelsParameter, InvalidGrade>, IPixelStage<Levels> {
    public static Levels Default { get; } = new(BlackLevel.Neutral, Exposure.Neutral);

    public static Option<PixelPass> Pass(Levels state, PassContext context) =>
        state == Default
            ? None
            : float.Exp2(-state.White) switch {
                var gain => Some<PixelPass>(new PixelPass.Color(new LutTable.Affine(new ColorMatrix3x3(gain, 0f, 0f, 0f, gain, 0f, 0f, 0f, gain), new Vector3(-state.Black * gain)))),
            };

    public static Fin<Levels> Auto(PixelFrame frame, Gamut gamut) =>
        LuminanceSamples.Gather(frame, gamut.Luminance).Bind(static samples =>
            Exposure.Validate(float.Log2(samples.FrameWhite - samples.Minimum), provider: null, out Exposure white) is { } refused
                ? refused
                : (Fin<Levels>)new Levels(BlackLevel.Create(samples.Minimum), white));
}

[SmartEnum<string>]
[ValidationError<InvalidGrade>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LevelsParameter : IStateParameter<Levels> {
    public static (Lens<Levels, BlackLevel> Lens, Presentation<BlackLevel, float> Presentation) BlackPoint { get; } = (
        Lens<Levels, BlackLevel>.New(static levels => levels.Black, static black => levels => levels with { Black = black }),
        new() { Soft = (-0.1f, 0.1f), Origin = 0f, Decimals = 4 });
    public static (Lens<Levels, Exposure> Lens, Presentation<Exposure, float> Presentation) WhitePoint { get; } = (
        Lens<Levels, Exposure>.New(static levels => levels.White, static white => levels => levels with { White = white }),
        Exposure.Presentation);

    public static readonly LevelsParameter Black = new("black-level", new StateParameter<Levels>.Bounded<BlackLevel, float, InvalidGrade>(BlackPoint.Lens, BlackPoint.Presentation));
    public static readonly LevelsParameter White = new("white-level", new StateParameter<Levels>.Bounded<Exposure, float, InvalidToneValue>(WhitePoint.Lens, WhitePoint.Presentation));

    public StateParameter<Levels> Kind { get; }
}
