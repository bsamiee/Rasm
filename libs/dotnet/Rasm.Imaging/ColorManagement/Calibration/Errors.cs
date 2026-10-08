using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.ColorManagement.Calibration;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    ViewUndefined = 1,
    ChartsDiffer,
    SamplesRankDeficient,
    TransformRankDeficient,
    NeutralUnlit,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record ViewUndefined(FrameQuad Quad) : Expected("chart alignment fits no rectilinear view", (int)Codes.ViewUndefined);

public sealed record ChartsDiffer(ColorChart Source, ColorChart Target) : Expected("source and target sample different charts", (int)Codes.ChartsDiffer);

public sealed record SamplesRankDeficient(int Rank) : Expected("chart samples span fewer than three color directions", (int)Codes.SamplesRankDeficient);

public sealed record TransformRankDeficient(int Rank) : Expected("fitted transform is singular", (int)Codes.TransformRankDeficient);

public sealed record NeutralUnlit(double Source, double Target) : Expected("neutral patch holds no luminance", (int)Codes.NeutralUnlit);
