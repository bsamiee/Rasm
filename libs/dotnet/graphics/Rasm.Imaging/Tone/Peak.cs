using System.Numerics;
using MathNet.Numerics.Interpolation;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Tone;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct KneeOffset : IMinMaxValue<KneeOffset> {
    public static KneeOffset MinValue { get; } = new(0.5f);
    public static KneeOffset MaxValue { get; } = new(2f);
    public static KneeOffset Bt2390 => MinValue;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = Admission.Refusal(value, MinValue, MaxValue);
}

public sealed record PeakMapping(Nits TargetPeak, AxisFraction Black, KneeOffset Knee) : IStateRecord<PeakMapping, PeakParameter> {
    public static PeakMapping Default { get; } = new(Display.Rec2100PqHdr.Peak, AxisFraction.MinValue, KneeOffset.Bt2390);

    public static Fin<Nits> MeasuredPeak(PixelFrame frame, Nits white) =>
        LuminanceSamples.Greatest(frame).Bind(samples => Nits.Validated((float)(samples.FrameWhite * (float)white)));

    public Option<PixelPass> Pass(Nits white, Nits sourcePeak) {
        float fitted = float.Min((float)TargetPeak, (float)sourcePeak);
        Vector4 levels = new(new Vector3((float)sourcePeak, fitted, (float)Black * fitted) / (float)white, 1f);
        TransferCurve.Pq.Encode(new Span<Vector4>(ref levels), white);
        (float source, float peak, float floor, float knee) = (levels.X, levels.Y / levels.X, levels.Z / levels.X, (float)Knee);
        (float start, float power) = (((1f + knee) * peak) - knee, float.Min(1f / floor, 4f));
        float gain = 1f / (1f + (floor / peak * float.Pow(1f - peak, power)));
        CubicSpline rolloff = CubicSpline.InterpolateHermiteSorted([0d, 1d], [start, peak], [1d - start, 0d]);

        float Mapped(float signal) =>
            float.Clamp(signal / source, 0f, 1f) switch {
                var x when x > start => (float)rolloff.Interpolate((x - start) / (1d - start)),
                var x => x,
            } switch {
                var x => float.Clamp((gain * (x + (floor * float.Pow(1f - x, power)) - floor)) + floor, floor, peak) * source,
            };

        return sourcePeak > TargetPeak || Black != AxisFraction.MinValue
            ? Some<PixelPass>(new PixelPass.Color(row => {
                TransferCurve.Pq.Encode(row, white);
                foreach (ref Vector4 pixel in row)
                    pixel = new Vector4(Mapped(pixel.X), Mapped(pixel.Y), Mapped(pixel.Z), pixel.W);
                TransferCurve.Pq.Decode(row, white);
            }))
            : None;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PeakParameter : IStateParameter<PeakMapping> {
    public static readonly PeakParameter TargetPeak = new("target-peak", StateParameter.Bounded(Lens<PeakMapping, Nits>.New(static mapping => mapping.TargetPeak, static peak => mapping => mapping with { TargetPeak = peak }),
    new Presentation<float> {
        Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(LuminanceUnit.Nit), Soft = ((float)Nits.ReferenceWhite, (float)Nits.MaxValue), Step = 1f, Decimals = 0,
    }),
        new("Target peak", "Brightest luminance of the display the highlights roll off into"));
    public static readonly PeakParameter Black = new("display-black", StateParameter.Bounded(Lens<PeakMapping, AxisFraction>.New(static mapping => mapping.Black, static black => mapping => mapping with { Black = black }), AxisFraction.Presentation with { Soft = ((float)AxisFraction.MinValue, 1e-3f), Decimals = 4 }),
        new("Display black", "Display black as a share of the target peak"));
    public static readonly PeakParameter Knee = new("knee-offset", StateParameter.Bounded(Lens<PeakMapping, KneeOffset>.New(static mapping => mapping.Knee, static knee => mapping => mapping with { Knee = knee }), new Presentation<float> { Step = 0.01f, Decimals = 2 }),
        new("Knee offset", "Distance below the target peak where the rolloff begins, its minimum the BT.2390 knee"));

    public StateParameter<PeakMapping> Kind { get; }
    public ParameterText Text { get; }
}
