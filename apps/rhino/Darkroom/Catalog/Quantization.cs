using System.Numerics;
using Darkroom.Chain;
using Rasm.Imaging.Color;
using Rasm.Imaging.Files;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.UI;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record OutputDither(Dither Method) : IStateRecord<OutputDither, OutputDitherParameter>, IPixelStage<OutputDither> {
    public static OutputDither Default { get; } = new(Dither.WhiteNoise);

    public static Option<PixelPass> Pass(OutputDither state, PassContext context) => context.Depth.Bind(static depth => depth.Levels).Map(state.Method.Pass);
}

[SmartEnum<string>]
public sealed partial class OutputDitherParameter : IStateParameter<OutputDither> {
    public static readonly OutputDitherParameter Method = new("method", new StateParameter<OutputDither>.Choice<Dither>(Lens<OutputDither, Dither>.New(static dither => dither.Method, static method => _ => new(method))),
        new("Method", "Noise or diffused error added before integer quantization"));

    public StateParameter<OutputDither> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
public sealed partial class GradingLog {
    public static readonly GradingLog AgXLog = new("agx-log", static () => FormationTables.AgXLog);
    public static readonly GradingLog FilmicLog = new("filmic-log", static () => LogSpace.Filmic);

    [UseDelegateFromConstructor]
    public partial LogSpace Space();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LutInput {
    public abstract LutSpace Space { get; }

    public sealed record Encoded(Primaries Gamut, Transfer Transfer) : LutInput, IStateRecord<Encoded, EncodedParameter> {
        public static Encoded Default { get; } = new(Primaries.StandardRgb, new Transfer.Srgb());
        public static string Caption => "RGB encoding";

        public override LutSpace Space => new LutSpace.Encoded(new ColorEncoding(Gamut.Gamut, Transfer));
    }

    public sealed record Log(GradingLog Grading) : LutInput, IStateRecord<Log, LogParameter> {
        public static Log Default { get; } = new(GradingLog.AgXLog);
        public static string Caption => "Grading log";

        public override LutSpace Space => new LutSpace.Log(Grading.Space());
    }
}

[SmartEnum<string>]
public sealed partial class EncodedParameter : IStateParameter<LutInput.Encoded> {
    public static readonly EncodedParameter Gamut = new("gamut", new StateParameter<LutInput.Encoded>.Choice<Primaries>(
        Lens<LutInput.Encoded, Primaries>.New(static space => space.Gamut, static gamut => space => space with { Gamut = gamut })),
        new("Gamut", "Table input and output primaries"));
    public static readonly EncodedParameter Transfer = new("transfer", new StateParameter<LutInput.Encoded>.Keyed<Transfer, string>(
        Lens<LutInput.Encoded, Transfer>.New(static space => space.Transfer, static transfer => space => space with { Transfer = transfer })),
        new("Transfer", "Table input and output encoding"));

    public StateParameter<LutInput.Encoded> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
public sealed partial class LogParameter : IStateParameter<LutInput.Log> {
    public static readonly LogParameter Space = new("space", new StateParameter<LutInput.Log>.Choice<GradingLog>(
        Lens<LutInput.Log, GradingLog>.New(static space => space.Grading, static grading => _ => new(grading))),
        new("Grading log", "Log encoding and gamut the table runs in"));

    public StateParameter<LutInput.Log> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record LutLook(Option<LutFile> Lut, LutInput Input) : IStateRecord<LutLook, LutLookParameter>, IPixelStage<LutLook> {
    public static LutLook Default { get; } = new(None, LutInput.Encoded.Default);

    public static Option<PixelPass> Pass(LutLook state, PassContext context) =>
        state.Lut.Map(file => (Signal: new ColorEncoding(context.Working, context.Signal), Bracket: state.Input.Space.Bracket) switch {
            var (signal, (space, into, back)) when signal.Transfer is Transfer.Linear && space.Transfer is Transfer.Linear =>
                (PixelPass)new PixelPass.Color(LutTables.Bracketed(signal.Gamut, space.Gamut, into, file.Table, back)),
            var (signal, (space, into, back)) =>
                new PixelPass.Color((Action<Span<Vector4>>)Delegate.Combine([.. signal.To(space), (Action<Span<Vector4>>)new LutTable.Sequence([.. into, file.Table, .. back]).Apply, .. space.To(signal)])!),
        });
}

[SmartEnum<string>]
public sealed partial class LutLookParameter : IStateParameter<LutLook> {
    public static readonly LutLookParameter Lut = new("lut", new StateParameter<LutLook>.Loaded<LutFile, LutPath, string>(
        Lens<LutLook, Option<LutFile>>.New(static look => look.Lut, static lut => look => look with { Lut = lut }), LutFile.Read, static file => file.Path),
        new("LUT", "Table file the look applies"));
    public static readonly LutLookParameter Input = new("space", new StateParameter<LutLook>.Variant<LutInput>(
        Lens<LutLook, LutInput>.New(static look => look.Input, static input => look => look with { Input = input })),
        new("Space", "Color space the table expects"));

    public StateParameter<LutLook> Kind { get; }
    public ParameterText Text { get; }
}
