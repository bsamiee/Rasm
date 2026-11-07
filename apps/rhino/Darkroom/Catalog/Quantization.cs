using System.Text.Json.Serialization;
using Darkroom.Chain;
using Rasm.Imaging.Color;
using Rasm.Imaging.Files;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record OutputDither(Dither Method, float Amount) : IPixelStage<OutputDither> {
    public static OutputDither Default { get; } = new(Dither.WhiteNoise, 1f);

    public static Fin<Option<PixelPass>> Pass(OutputDither state, PassContext context) =>
        context.Depth.Bind(static depth => depth.Levels).Map(levels => state.Method.Pass(levels, state.Amount));
}

[SmartEnum<string>]
public sealed partial class GradingLog {
    public static readonly GradingLog AgXLog = new("agx-log", FormationTables.AgXLog);
    public static readonly GradingLog FilmicLog = new("filmic-log", LutSpace.Filmic);

    public LutSpace Space { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic]
[JsonDerivedType(typeof(Encoded), nameof(Encoded))]
[JsonDerivedType(typeof(Log), nameof(Log))]
public abstract partial record LutInput {
    public abstract LutSpace Space { get; }

    public sealed record Encoded(Gamut Gamut, Transfer Transfer) : LutInput {
        public static Encoded Default { get; } = new(Gamut.StandardRgb, Transfer.Gamma.Srgb);

        public override LutSpace Space => LutSpace.Of(new ColorEncoding(Gamut, Transfer));
    }

    public sealed record Log(GradingLog Grading) : LutInput {
        public static Log Default { get; } = new(GradingLog.AgXLog);

        public override LutSpace Space => Grading.Space;
    }
}

public sealed record LutLook(Option<string> Lut, LutInput Input) : IPixelStage<LutLook> {
    public static LutLook Default { get; } = new(None, LutInput.Encoded.Default);

    public static Fin<Option<PixelPass>> Pass(LutLook state, PassContext context) =>
        state.Lut.Traverse(path => context.Derivations.Derived(path, static held => LutFormat.Read(held).RunSafe())
            .Map(table => (PixelPass)new PixelPass.Point(state.Input.Space.Around(new ColorEncoding(context.Working, context.Signal), table).Apply))).As();
}
