namespace Rasm.Imaging.ColorManagement;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
internal abstract partial record LogPiece {
    public abstract double Encode(double linear);
    public abstract double Decode(double code);

    public sealed record Logarithm(double Base, double LogSideSlope, double LogSideOffset, double LinSideSlope, double LinSideOffset) : LogPiece {
        public override double Encode(double linear) => (LogSideSlope * double.Log((LinSideSlope * linear) + LinSideOffset, Base)) + LogSideOffset;
        public override double Decode(double code) => (double.Pow(Base, (code - LogSideOffset) / LogSideSlope) - LinSideOffset) / LinSideSlope;
    }

    public sealed record Line(double Slope, double Offset) : LogPiece {
        public override double Encode(double linear) => (Slope * linear) + Offset;
        public override double Decode(double code) => (code - Offset) / Slope;
    }

    public sealed record Power(double Scale, double Shift, double Exponent) : LogPiece {
        public override double Encode(double linear) => Scale * double.Pow(double.Max(linear + Shift, 0d), Exponent);
        public override double Decode(double code) => double.Pow(double.Max(code / Scale, 0d), 1d / Exponent) - Shift;
    }
}

[SmartEnum<string>]
[ValidationError<InvalidColor>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CameraLog {
    public static readonly CameraLog ArriLogC3 = LogCamera("arri-logc3", new(10d, 0.247189638318671, 0.385536998692443, 5.55555555555556, 0.0522722750251688), 0.0105909904954696, None);
    public static readonly CameraLog ArriLogC4 = LogCamera("arri-logc4", new(2d, 0.0647954196341293, -0.295908392682586, 2231.82630906769, 64d), -0.0180569961199113, None);
    public static readonly CameraLog SonySLog2 = ((940d - 64d) / 1023d) switch {
        var legal => LogCamera("sony-slog2", new(10d, 0.432699 * legal, (0.646596 * legal) + (64d / 1023d), 155d / (219d * 0.9), 0.037584), 0d, None),
    };
    public static readonly CameraLog SonySLog3 = LogCamera(
        "sony-slog3", new(10d, 261.5 / 1023d, 420d / 1023d, 1d / (0.18 + 0.01), 0.01 / (0.18 + 0.01)), 0.01125, Some((171.2102946929 - 95d) / 0.01125 / 1023d));
    public static readonly CameraLog CanonLog2 = new(
        "canon-log2",
        new LogPiece.Logarithm(10d, -0.24136077, 0.092864125, -87.099375 / 0.9, 1d),
        [(0d, new LogPiece.Logarithm(10d, 0.24136077, 0.092864125, 87.099375 / 0.9, 1d))]);
    public static readonly CameraLog CanonLog3 = new(
        "canon-log3",
        new LogPiece.Logarithm(10d, -0.36726845, 0.12783901, -14.98325 / 0.9, 1d),
        [(-0.014 * 0.9, new LogPiece.Line(1.9754798 / 0.9, 0.12512219)), (0.014 * 0.9, new LogPiece.Logarithm(10d, 0.36726845, 0.12240537, 14.98325 / 0.9, 1d))]);
    public static readonly CameraLog RedLog3G10 = LogCamera("red-log3g10", new(10d, 0.224282, 0d, 155.975327, (0.01 * 155.975327) + 1d), -0.01, None);
    public static readonly CameraLog PanasonicVLog = LogCamera("panasonic-vlog", new(10d, 0.241514, 0.598206, 1d, 0.00873), 0.01, None);
    public static readonly CameraLog FujifilmFLog = LogCamera("fujifilm-flog", new(10d, 0.344676, 0.790453, 0.555556, 0.009468), 0.00089, Some(8.735631));
    public static readonly CameraLog FujifilmFLog2 = LogCamera("fujifilm-flog2", new(10d, 0.245281, 0.384316, 5.555556, 0.064829), 0.000889, Some(8.799461));
    public static readonly CameraLog DjiDLog = LogCamera("dji-dlog", new(10d, 0.256662970719888, 0.58455504907396, 0.9892, 0.0108), 0.00758078675, None);
    public static readonly CameraLog GoProProtune = new("gopro-protune", new LogPiece.Logarithm(113d, 1d, 0d, 112d, 1d), []);
    public static readonly CameraLog NikonNLog = new(
        "nikon-nlog", new LogPiece.Power(650d / 1023d, 0.0075, 1d / 3d), [(0.328, new LogPiece.Logarithm(double.E, 150d / 1023d, 619d / 1023d, 1d, 0d))]);
    public static readonly CameraLog AppleLog = new(
        "apple-log", new LogPiece.Power(47.28711236, 0.05641088, 2d), [(0.01, new LogPiece.Logarithm(2d, 0.08550479, 0.69336945, 1d, 0.00964052))]);
    public static readonly CameraLog BlackmagicFilmGen5 = LogCamera(
        "blackmagic-film-gen5", new(double.E, 0.0869287606549122, 0.530013339229194, 1d, 0.00549407243225781), 0.005, None);
    public static readonly CameraLog DaVinciIntermediate = LogCamera("davinci-intermediate", new(2d, 0.07329248, 0.07329248 * 7d, 1d, 0.0075), 0.00262409, Some(10.44426855));
    public static readonly CameraLog FilmLightTLog = ((1d - 0.075) / (16d * (0.7107 + (1.2359 * double.Log(128d * 16d))))) switch {
        var linSideOffset => ((1d - 0.075) / double.Log(1d + (128d / linSideOffset))) switch {
            var logSideSlope => LogCamera("filmlight-tlog", new(double.E, logSideSlope, 1d - (logSideSlope * double.Log(128d + linSideOffset)), 1d, linSideOffset), 0d, None),
        },
    };

    private readonly LogPiece first;
    private readonly Seq<(double Break, LogPiece Piece)> joints;

    public double Encode(double linear) => Encoded(joints.AsSpan(), linear);

    public double Decode(double code) => Decoded(joints.AsSpan(), code);

    private double Encoded(ReadOnlySpan<(double Break, LogPiece Piece)> above, double linear) =>
        above.IsEmpty ? first.Encode(linear)
        : linear >= above[^1].Break ? above[^1].Piece.Encode(linear)
        : Encoded(above[..^1], linear);

    private double Decoded(ReadOnlySpan<(double Break, LogPiece Piece)> above, double code) =>
        above.IsEmpty ? first.Decode(code)
        : above[^1].Piece.Decode(code) is var linear && linear >= above[^1].Break ? linear
        : Decoded(above[..^1], code);

    private static CameraLog LogCamera(string key, LogPiece.Logarithm log, double linSideBreak, Option<double> linearSlope) =>
        linearSlope.IfNone(() => log.LogSideSlope * log.LinSideSlope / (((log.LinSideSlope * linSideBreak) + log.LinSideOffset) * double.Log(log.Base))) switch {
            var slope => new(key, new LogPiece.Line(slope, log.Encode(linSideBreak) - (slope * linSideBreak)), [(linSideBreak, log)]),
        };
}
