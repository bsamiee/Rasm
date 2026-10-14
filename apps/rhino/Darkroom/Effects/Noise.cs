using Rasm.Imaging.Filters.Generators;
using Rasm.Rhino.UI.Rows;

namespace Darkroom.Effects;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class NoiseText {
    public static ParameterText Seed => ParameterText.Of("Seed", "Number selecting the field's pattern");
    public static ParameterText Hold => ParameterText.Of("Hold", "Seconds each drawn pattern holds before the next");
    public static ParameterText Timing => ParameterText.Of("Timing", "Stored time and rate of recorded frame time") with {
        Parameters = ParameterText.Fields<Timing, TimingParameter, InvalidGenerator>(static parameter => parameter.Map(
            clock: ParameterText.Of("Time", "Seconds of the moment a still draws, each recorded frame drawing at its own time"),
            pace: ParameterText.Of("Pace", "Rate effective time advances with recorded frame time"))),
    };

    public static string Profiles(WaveProfile profile) => profile.Map(sine: "Sine", saw: "Saw", triangle: "Triangle", square: "Square");

    public static ParameterText PointCells(PointCellsParameter parameter, string lead = "") =>
        parameter.Map(
            spacing: ParameterText.Of($"{lead}Spacing", "Distance between point cells as a share of the frame's short side"),
            density: ParameterText.Of($"{lead}Density", "Share of cells holding a point"),
            size: ParameterText.Of($"{lead}Size", "Disc radius as a share of the largest radius fitting its cell"),
            variation: ParameterText.Of($"{lead}Variation", "Share by which each point's draw reduces its radius"),
            spread: ParameterText.Of($"{lead}Brightness Spread", "Stops by which a point's draw dims its brightness"),
            softness: ParameterText.Of($"{lead}Softness", "Share of each disc's radius fading at its edge"));

    public static ParameterText Octaves(OctavesParameter parameter, string lead = "") =>
        parameter.Map(
            detail: ParameterText.Of($"{lead}Detail", "Octaves added past the first"),
            roughness: ParameterText.Of($"{lead}Roughness", "Strength of each octave against the one before"),
            lacunarity: ParameterText.Of($"{lead}Lacunarity", "Frequency of each octave against the one before"));

    public static ParameterText Cellular(CellularParameter parameter, string lead = "") =>
        parameter.Map(
            metric: ParameterText.Choice<CellMetric, InvalidGenerator>($"{lead}Metric", "Distance the cells measure", ToneText.MetricText),
            exponent: ParameterText.Of($"{lead}Exponent", "Exponent of the Minkowski distance"),
            randomness: ParameterText.Of($"{lead}Randomness", "Share of its cell each site wanders over"));

    public static ParameterText Basis(NoiseBasisParameter parameter, string lead = "") =>
        parameter.Map(
            basis: ParameterText.Choice<BasisKind, InvalidGenerator>(
                $"{lead}Type", "Noise the field draws",
                static kind => kind.Map(
                    fbm: "fBM", multifractal: "Multifractal", heteroTerrain: "Hetero Terrain", hybridMultifractal: "Hybrid Multifractal",
                    ridgedMultifractal: "Ridged Multifractal", f1: "F1", f2: "F2", smoothF1: "Smooth F1", edgeDistance: "Distance to Edge",
                    gabor: "Gabor", magic: "Magic", wave: "Wave", white: "White Noise")),
            dimensions: ParameterText.Choice<NoiseDimensions, InvalidGenerator>(
                $"{lead}Dimensions", "Number of coordinate axes the noise reads",
                static dimensions => dimensions.Map(one: "1D", two: "2D", three: "3D", four: "4D")),
            octaves: ParameterText.Of($"{lead}Octaves", "Detail of the noise across scales") with {
                Parameters = ParameterText.Fields<Octaves, OctavesParameter, InvalidGenerator>(item => Octaves(item, lead)),
            },
            distortion: ParameterText.Of($"{lead}Distortion", "Warp of the noise by its own signal"),
            cellular: ParameterText.Of($"{lead}Cells", "Cell distance and site placement") with {
                Parameters = ParameterText.Fields<Cellular, CellularParameter, InvalidGenerator>(item => Cellular(item, lead)),
            },
            offset: ParameterText.Of($"{lead}Offset", "Value added to each multifractal octave"),
            gain: ParameterText.Of($"{lead}Gain", "Weight each multifractal octave hands the next"),
            smoothness: ParameterText.Of($"{lead}Smoothness", "Blend between neighboring cells"),
            frequency: ParameterText.Of($"{lead}Frequency", "Gabor cycles per noise unit"),
            anisotropy: ParameterText.Of($"{lead}Anisotropy", "Share of Gabor impulses held at the orientation"),
            orientation: ParameterText.Of($"{lead}Orientation", "Base angle of the Gabor impulses"),
            depth: ParameterText.Of($"{lead}Magic Depth", "Turbulence steps of the magic noise"),
            form: ParameterText.Choice<WaveForm, InvalidGenerator>(
                $"{lead}Wave Type", "Bands or rings the wave noise draws", static form => form.Map(bands: "Bands", rings: "Rings")),
            profile: ParameterText.Choice<WaveProfile, InvalidGenerator>($"{lead}Wave Profile", "Shape of each wave period", Profiles),
            phase: ParameterText.Of($"{lead}Phase Offset", "Turn added to the wave's argument"),
            detailScale: ParameterText.Of($"{lead}Detail Scale", "Frequency of the wave's distortion noise against the wave's"));
}
