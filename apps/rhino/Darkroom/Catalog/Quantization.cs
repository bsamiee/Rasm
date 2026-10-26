using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Text.Json.Serialization;
using Darkroom.Chain;
using Rasm.Imaging.Color;
using Rasm.Imaging.Files;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record OutputDither([property: Display(Name = "Method", Description = "Noise or diffused error added before integer quantization")] Dither Method)
    : IPixelStage<OutputDither> {
    public static OutputDither Default { get; } = new(Dither.WhiteNoise);

    public static Option<PixelPass> Pass(OutputDither state, PassContext context) => context.Depth.Bind(static depth => depth.Levels).Map(state.Method.Pass);
}

[SmartEnum<string>]
public sealed partial class GradingLog {
    public static readonly GradingLog AgXLog = new("agx-log", static () => FormationTables.AgXLog);
    public static readonly GradingLog FilmicLog = new("filmic-log", static () => LogSpace.Filmic);

    [UseDelegateFromConstructor]
    public partial LogSpace Space();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic]
[JsonDerivedType(typeof(Encoded), nameof(Encoded))]
[JsonDerivedType(typeof(Log), nameof(Log))]
public abstract partial record LutInput {
    public abstract LutSpace Space { get; }

    [Display(Name = "RGB encoding")]
    public sealed record Encoded(
        [property: Display(Name = "Gamut", Description = "Table input and output primaries")] Primaries Gamut,
        [property: Display(Name = "Transfer", Description = "Table input and output encoding")] Transfer Transfer) : LutInput {
        public static Encoded Default { get; } = new(Primaries.StandardRgb, new Transfer.Srgb());

        public override LutSpace Space => new LutSpace.Encoded(new ColorEncoding(Gamut.Gamut, Transfer));
    }

    [Display(Name = "Grading log")]
    public sealed record Log([property: Display(Name = "Grading log", Description = "Log encoding and gamut the table runs in")] GradingLog Grading) : LutInput {
        public static Log Default { get; } = new(GradingLog.AgXLog);

        public override LutSpace Space => new LutSpace.Log(Grading.Space());
    }
}

public sealed record LutLook(
    [property: Display(Name = "LUT", Description = "Table file the look applies")] string? Lut,
    [property: Display(Name = "Space", Description = "Color space the table expects")] LutInput Input) : IPixelStage<LutLook> {
    public static LutLook Default { get; } = new(Lut: null, LutInput.Encoded.Default);

    public static Option<PixelPass> Pass(LutLook state, PassContext context) =>
        Optional(state.Lut).Map(path => context.Derivations.Derived(path, static held => LutFile.Read(held).RunSafe()).Match(
            Succ: file => (Signal: new ColorEncoding(context.Working, context.Signal), state.Input.Space.Bracket) switch {
                var (signal, (space, into, back)) when signal.Transfer is Transfer.Linear && space.Transfer is Transfer.Linear =>
                    (PixelPass)new PixelPass.Color(LutTables.Bracketed(signal.Gamut, space.Gamut, into, file.Table, back)),
                var (signal, (space, into, back)) =>
                    new PixelPass.Color((Action<Span<Vector4>>)Delegate.Combine([.. signal.To(space), new LutTable.Sequence([.. into, file.Table, .. back]).Apply, .. space.To(signal)])!),
            },
            Fail: static error => new PixelPass.Frame((_, _) => error)));
}
