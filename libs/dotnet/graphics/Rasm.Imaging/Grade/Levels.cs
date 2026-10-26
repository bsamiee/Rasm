using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Levels(SignedMagnitude Black, Exposure White) : IStateRecord<Levels, LevelsParameter>, IPixelStage<Levels> {
    public static Levels Default { get; } = new(SignedMagnitude.Zero, Exposure.Neutral);

    public static Option<PixelPass> Pass(Levels state, PassContext context) =>
        state == Default
            ? None
            : double.Exp2(-state.White) switch {
                var gain => (-state.Black * gain) switch {
                    var offset => Some<PixelPass>(new PixelPass.Color(new LutTable.Affine(Matrix<double>.Build.DenseDiagonal(3, gain), (offset, offset, offset)))),
                },
            };

    public float Level(LevelsParameter parameter) => parameter.Map(black: Black, white: Black + White.Scale);

    public Fin<Levels> Leveled(LevelsParameter parameter, float level) =>
        parameter.Switch(
            (Levels: this, Level: level),
            black: static held =>
                (SignedMagnitude.Validated(held.Level), Exposure.Validated(float.Log2(held.Levels.Level(LevelsParameter.White) - held.Level)))
                    .Apply(static (black, white) => new Levels(black, white))
                    .As(),
            white: static held => Exposure.Validated(float.Log2(held.Level - held.Levels.Black)).Map(white => held.Levels with { White = white }));

    public static Fin<Levels> Auto(PixelFrame frame, Gamut gamut) =>
        LuminanceSamples.Gather(frame, gamut.Luminance).Bind(static samples =>
            (SignedMagnitude.Validated((float)samples.Minimum), Exposure.Validated((float)double.Log2(samples.FrameWhite - samples.Minimum)))
                .Apply(static (black, white) => new Levels(black, white)).As());
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LevelsParameter : IStateParameter<Levels> {
    public static (Lens<Levels, SignedMagnitude> Lens, Presentation<float> Presentation) BlackPoint { get; } = (
        Lens<Levels, SignedMagnitude>.New(static levels => levels.Black, static black => levels => levels with { Black = black }),
        new() { Soft = (-0.1f, 0.1f), Origin = 0f, Decimals = 4 });
    public static Lens<Levels, Exposure> WhitePoint { get; } = Lens<Levels, Exposure>.New(static levels => levels.White, static white => levels => levels with { White = white });

    public static readonly LevelsParameter Black = new("black-level", StateParameter.Bounded(BlackPoint.Lens, BlackPoint.Presentation),
        new("Black", "Scene value mapped to zero, lower values pass unclamped"));
    public static readonly LevelsParameter White = new("white-level", StateParameter.Bounded(WhitePoint),
        new("White", "Span above black mapped to one, in stops"));

    public StateParameter<Levels> Kind { get; }
    public ParameterText Text { get; }
}
