using System.Numerics;
using MathNet.Numerics.Interpolation;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone.Formations;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Tone;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct KneeOffset : IMinMaxValue<KneeOffset> {
    public static KneeOffset MinValue { get; } = new(0.5f);
    public static KneeOffset MaxValue { get; } = new(2f);
    public static KneeOffset Bt2390 => MinValue;

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Floor", SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct DisplayBlack : IMinMaxValue<DisplayBlack> {
    public static DisplayBlack MinValue => Floor;
    public static DisplayBlack MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

public sealed record PeakMapping(Nits TargetPeak, DisplayBlack Black, KneeOffset Knee) : IStateRecord<PeakMapping, PeakParameter, InvalidToneValue> {
    public static PeakMapping Default { get; } = new(Display.Rec2100PqHdr.Peak, DisplayBlack.Floor, KneeOffset.Bt2390);

    public static Fin<Nits> MeasuredPeak(PixelFrame frame, Nits white) =>
        LuminanceSamples.Greatest(frame).Bind(samples =>
            Nits.Validate(samples.FrameWhite * (float)white, provider: null, out Nits peak) is { } error ? error : (Fin<Nits>)peak);

    public Option<PixelPass> Pass(Nits white, Nits sourcePeak) {
        float fitted = float.Min((float)TargetPeak, (float)sourcePeak);
        Vector4 levels = new(new Vector3((float)sourcePeak, fitted, (float)Black * fitted) / (float)white, 1f);
        TransferCurve.Pq.Encode(new Span<Vector4>(ref levels), white);
        (float source, float peak, float floor, float knee) = (levels.X, levels.Y / levels.X, levels.Z / levels.X, (float)Knee);
        (float start, float power) = (((1f + knee) * peak) - knee, float.Min(1f / floor, 4f));
        float gain = 1f / (1f + (floor / peak * float.Pow(1f - peak, power)));
        CubicSpline rolloff = CubicSpline.InterpolateHermiteSorted([start, 1d], [start, peak], [1d, 0d]);

        float Mapped(float signal) =>
            float.Clamp(signal / source, 0f, 1f) switch {
                var x when x > start => (float)rolloff.Interpolate(x),
                var x => x,
            } switch {
                var x => float.Clamp((gain * (x + (floor * float.Pow(1f - x, power)) - floor)) + floor, floor, peak) * source,
            };

        return sourcePeak > TargetPeak || Black != DisplayBlack.Floor
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
[ValidationError<InvalidToneValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PeakParameter : IStateParameter<PeakMapping> {
    public static readonly PeakParameter TargetPeak = new("target-peak", new StateParameter<PeakMapping>.Bounded<Nits, float, InvalidColor>(
        Lens<PeakMapping, Nits>.New(static mapping => mapping.TargetPeak, static peak => mapping => mapping with { TargetPeak = peak }),
        new Presentation<Nits, float> {
            Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(LuminanceUnit.Nit), Soft = ((float)Nits.ReferenceWhite, (float)Nits.MaxValue), Step = 1f, Decimals = 0,
        }));
    public static readonly PeakParameter Black = new("display-black", new StateParameter<PeakMapping>.Bounded<DisplayBlack, float, InvalidToneValue>(
        Lens<PeakMapping, DisplayBlack>.New(static mapping => mapping.Black, static black => mapping => mapping with { Black = black }),
        new Presentation<DisplayBlack, float> { Soft = ((float)DisplayBlack.Floor, 1e-3f), Step = 1e-5f, Decimals = 6 }));
    public static readonly PeakParameter Knee = new("knee-offset", new StateParameter<PeakMapping>.Bounded<KneeOffset, float, InvalidToneValue>(
        Lens<PeakMapping, KneeOffset>.New(static mapping => mapping.Knee, static knee => mapping => mapping with { Knee = knee }),
        new Presentation<KneeOffset, float> { Step = 0.01f, Decimals = 2 }));

    public StateParameter<PeakMapping> Kind { get; }
}
