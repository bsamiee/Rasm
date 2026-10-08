using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;
using TinyEXR;
using Wacton.Unicolour;

namespace Rasm.Imaging.Tone.Formations;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, ConstructorAccessModifier = AccessModifier.Internal,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct FilmSpan : IMinMaxValue<FilmSpan> {
    public static FilmSpan MinValue { get; } = new(float.BitIncrement(0f));
    public static FilmSpan MaxValue { get; } = new(64f);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(SkipIParsable = true, ConstructorAccessModifier = AccessModifier.Internal,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct CurveFraction : IMinMaxValue<CurveFraction> {
    public static CurveFraction MinValue { get; } = new(float.BitIncrement(0f));
    public static CurveFraction MaxValue { get; } = new(float.BitDecrement(1f));

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(SkipIParsable = true, ConstructorAccessModifier = AccessModifier.Internal,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct Density : IMinMaxValue<Density> {
    public static Density MinValue { get; } = new(float.BitIncrement(0f));
    public static Density MaxValue { get; } = new(149f);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(SkipIParsable = true, ConstructorAccessModifier = AccessModifier.Internal,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct PrimaryScale : IMinMaxValue<PrimaryScale> {
    public static PrimaryScale MinValue { get; } = new(1f);
    public static PrimaryScale MaxValue { get; } = new(float.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(SkipIParsable = true, ConstructorAccessModifier = AccessModifier.Internal,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct HueRotation : IMinMaxValue<HueRotation> {
    public static HueRotation MinValue { get; } = new(float.BitIncrement(-float.Pi / 6f));
    public static HueRotation MaxValue { get; } = new(float.BitDecrement(float.Pi / 6f));

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(SkipIParsable = true, ConstructorAccessModifier = AccessModifier.Internal,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct FilmGain : IMinMaxValue<FilmGain> {
    public static FilmGain MinValue { get; } = new(float.BitIncrement(0f));
    public static FilmGain MaxValue { get; } = new(float.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(SkipIParsable = true, ConstructorAccessModifier = AccessModifier.Internal,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct MidtoneSaturation : IMinMaxValue<MidtoneSaturation> {
    public static MidtoneSaturation MinValue { get; } = new(0f);
    public static MidtoneSaturation MaxValue { get; } = new(float.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(SkipIParsable = true, ConstructorAccessModifier = AccessModifier.Internal,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct BlackOffset : IMinMaxValue<BlackOffset> {
    public static BlackOffset MinValue { get; } = new(-float.MaxValue);
    public static BlackOffset MaxValue { get; } = new(999f);

    public double Fraction => _value / 1000d;

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

public sealed record FilmState(
    Exposure PreExposure, Factor PreFilterRed, Factor PreFilterGreen, Factor PreFilterBlue,
    PrimaryScale RedScale, PrimaryScale GreenScale, PrimaryScale BlueScale,
    HueRotation RedRotation, HueRotation GreenRotation, HueRotation BlueRotation,
    FilmGain RedMultiplier, FilmGain GreenMultiplier, FilmGain BlueMultiplier,
    Exposure SigmoidMinimum, FilmSpan SigmoidSpan, CurveFraction ToeX, CurveFraction ToeY, CurveFraction ShoulderReachX, CurveFraction ShoulderReachY,
    Exposure NegativeExposure, Density NegativeDensity, FilmGain BacklightRed, FilmGain BacklightGreen, FilmGain BacklightBlue,
    Exposure PrintExposure, Density PrintDensity, Option<BlackOffset> BlackPoint,
    Factor PostFilterRed, Factor PostFilterGreen, Factor PostFilterBlue, MidtoneSaturation MidtoneSaturation);

[SmartEnum]
public sealed partial class FilmPreset {
    public static readonly FilmPreset Default = new(new FilmState(
        new Exposure(4.3f), Factor.MaxValue, Factor.MaxValue, Factor.MaxValue,
        new PrimaryScale(1.05f), new PrimaryScale(1.12f), new PrimaryScale(1.045f),
        new HueRotation(float.DegreesToRadians(0.5f)), new HueRotation(float.DegreesToRadians(2f)), new HueRotation(float.DegreesToRadians(0.1f)),
        new FilmGain(1f), new FilmGain(1f), new FilmGain(1f),
        new Exposure(-10f), new FilmSpan(22f - -10f), new CurveFraction(0.44f), new CurveFraction(0.28f),
        new CurveFraction((0.591f - 0.44f) / (1f - 0.44f)), new CurveFraction((0.779f - 0.28f) / (1f - 0.28f)),
        new Exposure(6f), new Density(5f), new FilmGain(1f), new FilmGain(1f), new FilmGain(1f),
        new Exposure(6f), new Density(27.5f), None,
        Factor.MaxValue, Factor.MaxValue, Factor.MaxValue, new MidtoneSaturation(1.02f)));
    public static readonly FilmPreset Nostalgia = new(Default.State with {
        PreExposure = new Exposure(5.563035f),
        RedMultiplier = new FilmGain(1.1f),
        BlueMultiplier = new FilmGain(1.2f),
        SigmoidSpan = new FilmSpan(23f - -10f),
        NegativeExposure = new Exposure(5.8f),
        BacklightRed = new FilmGain(0.99f),
        BacklightGreen = new FilmGain(1.1f),
        BacklightBlue = new FilmGain(1.035989f),
        PrintDensity = new Density(40f),
        BlackPoint = Some(new BlackOffset(-5f)),
        MidtoneSaturation = new MidtoneSaturation(1.1f),
    });
    public static readonly FilmPreset Silver = new(Default.State with {
        PreExposure = new Exposure(3.9f),
        PreFilterRed = new Factor(0.95f),
        PreFilterGreen = new Factor(0.975f),
        BlueMultiplier = new FilmGain(1.06f),
        NegativeExposure = new Exposure(4.7f),
        NegativeDensity = new Density(7f),
        BacklightRed = new FilmGain(0.9992f),
        BacklightGreen = new FilmGain(0.99f),
        PrintExposure = new Exposure(4.7f),
        PrintDensity = new Density(30f),
        BlackPoint = Some(new BlackOffset(0.5f)),
        PostFilterBlue = new Factor(0.96f),
        MidtoneSaturation = new MidtoneSaturation(1f),
    });

    public FilmState State { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidToneValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilmicView {
    public static readonly FilmicView VeryHighContrast = new("very-high-contrast", Some("filmic_to_1.20_1-00.spi1d"));
    public static readonly FilmicView HighContrast = new("high-contrast", Some("filmic_to_0.99_1-0075.spi1d"));
    public static readonly FilmicView MediumHighContrast = new("medium-high-contrast", Some("filmic_to_0-85_1-011.spi1d"));
    public static readonly FilmicView MediumContrast = new("medium-contrast", Some("filmic_to_0-70_1-03.spi1d"));
    public static readonly FilmicView MediumLowContrast = new("medium-low-contrast", Some("filmic_to_0-60_1-04.spi1d"));
    public static readonly FilmicView LowContrast = new("low-contrast", Some("filmic_to_0-48_1-09.spi1d"));
    public static readonly FilmicView VeryLowContrast = new("very-low-contrast", Some("filmic_to_0-35_1-30.spi1d"));
    public static readonly FilmicView Log = new("log", None);

    public Option<string> Curve { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Film {
    private static readonly LogCurve Window = LogCurve.Allocation(-10f, 10f, float.ScaleB(1f, -10));
    private static readonly Vector256<double> Weights = Vector256.Create(0.3d, 0.5d, 0.2d, 0d);
    private static readonly ConditionalWeakTable<FilmState, Lut3D> Baked = [];

    internal static PixelPass Formed(ToneMapping state, PassContext context) =>
        Formation.Formed(
            [.. LutTables.Between(context.Working, Gamut.StandardRgb).Map<Action<Span<Vector4>>>(static step => step.Apply),
             new LutTable.Log(Window).Apply,
             new LutTable.Cube(Baked.GetValue(state.Film, Developed), LutInterpolation.Tetrahedral).Apply],
            new ColorEncoding(Gamut.StandardRgb, TransferCurve.Linear, Nits.ReferenceWhite), context.Display);

    private static Lut3D Developed(FilmState state) {
        Matrix<double> extension = Matrix<double>.Build.DiagonalOfDiagonalArray([state.RedMultiplier, state.GreenMultiplier, state.BlueMultiplier])
            * Matrix<double>.Build.DenseOfRowArrays(
                Seq((Scale: state.RedScale, Rotation: state.RedRotation), (Scale: state.GreenScale, Rotation: state.GreenRotation), (Scale: state.BlueScale, Rotation: state.BlueRotation))
                    .Map(static (primary, index) => new Unicolour(Configuration.Default, ColourSpace.Hsb, (120d * index) + double.RadiansToDegrees(primary.Rotation), 1d / primary.Scale, 1d).Rgb.ToArray()))
                .NormalizeRows(1d);
        (Func<Vector256<double>, Vector256<double>> gamut, Func<Vector256<double>, Vector256<double>> back) = (Transform(extension), Transform(extension.Inverse()));
        (double toeX, double toeY) = (state.ToeX, state.ToeY);
        (double shoulderX, double shoulderY) = (toeX + (state.ShoulderReachX * (1d - toeX)), toeY + (state.ShoulderReachY * (1d - toeY)));
        double slope = (shoulderY - toeY) / (shoulderX - toeX);
        (double low, double span, double offset) = (state.SigmoidMinimum, state.SigmoidSpan, double.Exp2(state.SigmoidMinimum));
        (double negative, Vector256<double> pre, Vector256<double> post) = (
            double.Exp2(state.NegativeExposure),
            Vector256.Create(state.PreFilterRed, state.PreFilterGreen, state.PreFilterBlue, 0d) * double.Exp2(state.PreExposure),
            Vector256.Create(state.PostFilterRed, state.PostFilterGreen, state.PostFilterBlue, 0d));
        Vector256<double> backlight = gamut(Vector256.Create(state.BacklightRed, state.BacklightGreen, state.BacklightBlue, 0d)) * double.Exp2(state.PrintExposure);
        Vector256<double> cap = Transmitted(backlight * double.Exp2(-(float)state.NegativeDensity), state.PrintDensity);
        double black = state.BlackPoint.Match(
            Some: static offset => offset.Fraction,
            None: () => double.Min(Vector256.Dot(Transmitted(backlight, state.PrintDensity) / cap, Weights), BlackOffset.MaxValue.Fraction));
        return Bakes.Lattice(Valid.Value(LatticeSize.Validate(80, provider: null, out LatticeSize edge), edge), [row => {
            foreach (ref Vector4 node in row)
                node = new Vector4(Displayed(Window.Inverse(node.AsVector3())), node.W);
        }]).ThrowIfFail();

        double Sigmoid(double x) =>
            x < toeX ? toeY * double.Pow(x / toeX, slope * toeX / toeY)
            : x < shoulderX ? toeY + (slope * (x - toeX))
            : 1d - ((1d - shoulderY) * double.Pow((1d - x) / (1d - shoulderX), slope * (1d - shoulderX) / (1d - shoulderY)));

        double Transmittance(double stimulus, double density) => double.Exp2(-density * Sigmoid(double.Clamp((double.Log2(stimulus + offset) - low) / span, 0d, 1d)));

        Vector256<double> Transmitted(Vector256<double> stimulus, double density) =>
            Vector256.Create(Transmittance(stimulus[0], density), Transmittance(stimulus[1], density), Transmittance(stimulus[2], density), 1d);

        Vector3 Displayed(Vector3 node) {
            Vector256<double> printed = Transmitted(backlight * Transmitted(gamut(Vector256.Create(node.X, node.Y, node.Z, 0d) * pre) * negative, state.NegativeDensity), state.PrintDensity) / cap;
            double mono = Vector256.Dot(printed, Weights);
            Vector256<double> clipped = Vector256.Clamp(back(printed * (double.Clamp((mono - black) / (1d - black), 0d, 1d) / mono)) * post, Vector256<double>.Zero, Vector256<double>.One);
            (double most, double least) = (double.Max(clipped[0], double.Max(clipped[1], clipped[2])), double.Min(clipped[0], double.Min(clipped[1], clipped[2])));
            Vector256<double> midtone = Vector256.Lerp(
                clipped,
                Vector256.Create(most) - ((Vector256.Create(most) - clipped) * double.MinNumber(state.MidtoneSaturation, most / (most - least))),
                Vector256.Create(double.Max(1d - (double.Abs(Vector256.Dot(clipped, Weights) - 0.5d) / 0.45d), 0d)));
            return new Vector3((float)midtone[0], (float)midtone[1], (float)midtone[2]);
        }
    }

    private static Func<Vector256<double>, Vector256<double>> Transform(Matrix<double> matrix) =>
        (Vector256.Create(matrix[0, 0], matrix[0, 1], matrix[0, 2], 0d), Vector256.Create(matrix[1, 0], matrix[1, 1], matrix[1, 2], 0d), Vector256.Create(matrix[2, 0], matrix[2, 1], matrix[2, 2], 0d)) switch {
            var (red, green, blue) => value => Vector256.Create(Vector256.Dot(red, value), Vector256.Dot(green, value), Vector256.Dot(blue, value), 0d),
        };
}

public static class Filmic {
    internal static PixelPass Formed(ToneMapping state, PassContext context) =>
        state.Filmic.Curve.Match(
            Some: static file => (Graded: Seq(new LutTable.ChannelCurve(EmbeddedTables.Curve(file)).Apply), Signal: Display.Srgb.Encoding),
            None: () => (Graded: Seq<Action<Span<Vector4>>>(), Signal: context.Display.Encoding)) switch {
                var (graded, signal) => Formation.Formed(
                    [.. LutTables.Between(context.Working, LogSpace.FilmicLog.Gamut).Map<Action<Span<Vector4>>>(static step => step.Apply),
                     .. LogSpace.FilmicLog.Into.Map<Action<Span<Vector4>>>(static step => step.Apply),
                     new LutTable.Cube(EmbeddedTables.Lattice("filmic_desat_33.cube"), LutInterpolation.Tetrahedral).Apply,
                     new LutTable.Remap(new RemapBounds.Between(new RemapBound(0f, 0f), new RemapBound(0.66f, 1f), Clamps: false)).Apply,
                     .. graded],
                    signal, context.Display),
            };
}

public static class Tony {
    private static readonly Memo<Lut3D> Table = memo(static () => {
        const uint Mantissa = 0x1FF;
        LatticeSize edge = Valid.Value(LatticeSize.Validate(48, provider: null, out LatticeSize size), size);
        using Stream stream = EmbeddedTables.Open("tony_mc_mapface.dds");
        byte[] texels = GC.AllocateUninitializedArray<byte>(4 * edge * edge * edge);
        stream.Position = 148;
        stream.ReadExactly(texels);
        float[] samples = GC.AllocateUninitializedArray<float>(3 * edge * edge * edge);
        Span<Vector3> nodes = MemoryMarshal.Cast<float, Vector3>(samples.AsSpan());
        for (int node = 0; node < nodes.Length; node++)
            nodes[node] = BinaryPrimitives.ReadUInt32LittleEndian(texels.AsSpan(4 * node)) switch {
                var texel => new Vector3(texel & Mantissa, (texel >> 9) & Mantissa, (texel >> 18) & Mantissa) * float.ScaleB(1f, (int)(texel >> 27) - 24),
            };
        return Lut3D.From(edge, samples, Vector3.Zero, Vector3.One).ThrowIfFail();
    });

    private static readonly Action<Span<Vector4>> Shaped = static row => {
        foreach (ref Vector4 pixel in row)
            pixel = Vector3.Max(pixel.AsVector3(), Vector3.Zero) switch {
                var stimulus => new Vector4(stimulus / (stimulus + Vector3.One), pixel.W),
            };
    };

    internal static PixelPass Formed(ToneMapping _, PassContext context) =>
        Formation.Formed(
            [.. LutTables.Between(context.Working, Gamut.StandardRgb).Map<Action<Span<Vector4>>>(static step => step.Apply), Shaped, new LutTable.Cube(Table.Value, LutInterpolation.Tetrahedral).Apply],
            new ColorEncoding(Gamut.StandardRgb, TransferCurve.Linear, Nits.ReferenceWhite), context.Display);
}
