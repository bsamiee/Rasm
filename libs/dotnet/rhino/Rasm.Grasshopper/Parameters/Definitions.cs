using Eto.Drawing;
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Rasm.Rhino;

namespace Rasm.Grasshopper.Parameters;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ParameterRow(Func<IPlugInSink, IO<IParameter>> Create) {
    // --- [DECLARE]
    public Option<(string Category, Option<Color> Tint, bool Shown)> Modularity { get; init; }
    public Seq<Func<IParameter, IPlugInSink, IO<Unit>>> Attach { get; init; }

    public static ParameterRow Of(Nomen face, string code, Func<Nomen, IParameter> construct) =>
        new(sink => IO.lift(() => construct(Localized(face, sink))).Bind(parameter => Coded(parameter, code, sink)));

    public static ParameterRow Of<TData>(Nomen face, string code, Func<Nomen, Parameter<TData>> construct, Seq<(string Caption, TData Value)> presets)
        where TData : notnull =>
        new(sink =>
            from parameter in IO.lift(() => construct(Localized(face, sink)))
            from _ in IO.lift(() => Callbacks.Each(
                presets,
                preset => parameter.Presets.Add(RowText.Localize(preset.Caption, sink), "", preset.Value),
                nameof(Presets<>.Add)))
            from coded in Coded(parameter, code, sink)
            select coded);

    // --- [REALIZE]
    public IO<IParameter> Input(ModularInputAdder inputs, IPlugInSink sink) =>
        from parameter in Attached(sink)
        from _ in IO.lift(() => Modularity.Match(
            Some: optional =>
                (optional.Shown ? (Action<IParameter, string, Color, Requirement?>)inputs.Add : inputs.AddHidden)(
                    parameter, RowText.Localize(optional.Category, sink), optional.Tint.IfNone(Colors.Transparent), null),
            None: () => inputs.RegularAdder.Add(parameter, parameter.Requirement)))
        select parameter;

    public IO<IParameter> Output(ModularOutputAdder outputs, IPlugInSink sink) =>
        from parameter in Attached(sink)
        from _ in IO.lift(() => Modularity.Match(
            Some: optional =>
                (optional.Shown ? (Action<IParameter, string, Color>)outputs.Add : outputs.AddHidden)(
                    parameter, RowText.Localize(optional.Category, sink), optional.Tint.IfNone(Colors.Transparent)),
            None: () => outputs.RegularAdder.Add(parameter)))
        select parameter;

    private IO<IParameter> Attached(IPlugInSink sink) =>
        from parameter in Create(sink)
        from attached in Attach.TraverseM(attach => attach(parameter, sink)).As()
        select parameter;

    private static Nomen Localized(Nomen face, IPlugInSink sink) =>
        face.WithName(RowText.Localize(face.Name, sink)).WithInfo(RowText.Localize(face.Info, sink));

    private static IO<IParameter> Coded(IParameter parameter, string code, IPlugInSink sink) =>
        IO.lift(() => parameter.UserName = RowText.Localize(code, sink)).Map(parameter);
}
