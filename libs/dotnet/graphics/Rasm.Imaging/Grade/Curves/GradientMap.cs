using System.Numerics;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Grade.Curves;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GradientMap(Ramp Ramp) : IStateRecord<GradientMap, GradientMapParameter, InvalidGrade>, IPixelStage<GradientMap> {
    public static GradientMap Default { get; } = new(Ramp.Grayscale);

    public static Option<PixelPass> Pass(GradientMap state, PassContext context) =>
        (state.Ramp.Tabulate(context.Working), context.Working.Luminance) switch {
            var (table, weights) => Some<PixelPass>(new PixelPass.Color(row => {
                foreach (ref Vector4 pixel in row)
                    pixel = table.Sample((float)double.Cbrt((weights.X * pixel.X) + (weights.Y * pixel.Y) + (weights.Z * pixel.Z))) switch {
                        var mapped => new Vector4(
                            (float)double.Lerp(pixel.X, mapped.X, mapped.W),
                            (float)double.Lerp(pixel.Y, mapped.Y, mapped.W),
                            (float)double.Lerp(pixel.Z, mapped.Z, mapped.W), pixel.W),
                    };
            })),
        };
}

[SmartEnum<string>]
[ValidationError<InvalidGrade>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GradientMapParameter : IStateParameter<GradientMap> {
    public static readonly GradientMapParameter Ramp = new(
        "ramp",
        new StateParameter<GradientMap>.Gradient(
            Lens<GradientMap, Ramp>.New(static map => map.Ramp, static ramp => map => map with { Ramp = ramp })));

    public StateParameter<GradientMap> Kind { get; }
}
