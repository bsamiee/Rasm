using System.Drawing;
using System.Numerics;
using System.Runtime.Intrinsics;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record WhiteBalanceState(ColorTemperature Temperature, Tint Tint) : IStateRecord<WhiteBalanceState, WhiteBalanceParameter, InvalidGrade>, IPixelStage<WhiteBalanceState> {
    public static WhiteBalanceState Default { get; } = new(ColorTemperature.ViewDefault, Tint.ViewDefault);

    public LutTable.Affine Table(Gamut working) =>
        new(working.Adapting(Adaptation.White(Temperature, Tint), working.Configuration.Xyz.WhitePoint), (0d, 0d, 0d));

    public static Option<PixelPass> Pass(WhiteBalanceState state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Color(state.Table(context.Working)));

    public static Fin<WhiteBalanceState> Sample(PixelFrame frame, Option<Rectangle> region, Gamut working) {
        (long Left, long Top, long Right, long Bottom) bounds = region.Match(
            Some: area => (long.Max(area.Left, frame.Window.Left), long.Max(area.Top, frame.Window.Top), long.Min((long)area.X + area.Width, frame.Window.Right), long.Min((long)area.Y + area.Height, frame.Window.Bottom)),
            None: () => frame.Window);
        int width = (int)long.Max(bounds.Right - bounds.Left, 0L);
        Vector256<double> sum = Vector256<double>.Zero;
        for (long line = bounds.Top; width > 0 && line < bounds.Bottom; line++) {
            foreach (Vector4 pixel in frame.View.Span.GetRowSpan((int)(line - frame.Origin.Y)).Slice((int)(bounds.Left - frame.Origin.X), width))
                sum += pixel.W > 0f && Vector3.AllWhereAllBitsSet(Vector3.IsFinite(pixel.AsVector3())) ? Vector256.Create(pixel.X, pixel.Y, pixel.Z, 1d) : Vector256<double>.Zero;
        }
        return sum[3] is 0d
            ? new EmptyRegion(bounds)
            : Adaptation.Correlate(working, Vector256.Narrow(sum / sum[3], Vector256<double>.Zero).GetLower().AsVector3())
                .Map(static white => new WhiteBalanceState(white.Temperature, white.Tint));
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGrade>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WhiteBalanceParameter : IStateParameter<WhiteBalanceState> {
    public static readonly WhiteBalanceParameter Temperature = new(
        "temperature",
        StateParameter.Bounded<WhiteBalanceState, ColorTemperature, double, InvalidColor>(Lens<WhiteBalanceState, ColorTemperature>.New(static state => state.Temperature, static temperature => state => state with { Temperature = temperature }),
        new() { Unit = ColorTemperature.Unit, Soft = (2000d, 11000d), Step = 1d, Decimals = 0 }));
    public static readonly WhiteBalanceParameter Tint = new(
        "tint",
        StateParameter.Bounded<WhiteBalanceState, Tint, double, InvalidColor>(Lens<WhiteBalanceState, Tint>.New(static state => state.Tint, static tint => state => state with { Tint = tint }),
        new() { Origin = (double)Color.Tint.Neutral }));

    public StateParameter<WhiteBalanceState> Kind { get; }
}
