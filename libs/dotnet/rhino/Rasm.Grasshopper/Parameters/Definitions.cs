using Eto.Drawing;
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Rasm.RhinoCommon;

namespace Rasm.Grasshopper.Parameters;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ParameterRow(Func<IGrasshopperPlugin, IO<IParameter>> Create) {
    // --- [DECLARE]
    public Option<(string Category, Option<Color> Tint, bool Shown)> Modularity { get; init; }
    public Seq<Func<IParameter, IGrasshopperPlugin, IO<Unit>>> Attach { get; init; }

    public static ParameterRow Of(Nomen face, string code, Func<Nomen, IParameter> construct) =>
        new(plugin => IO.lift(() => construct(Localized(face, plugin))).Bind(parameter => Coded(parameter, code, plugin)));

    public static ParameterRow Of<TData>(Nomen face, string code, Func<Nomen, Parameter<TData>> construct, Seq<(string Caption, TData Value)> presets)
        where TData : notnull =>
        new(plugin =>
            from parameter in IO.lift(() => construct(Localized(face, plugin)))
            from _ in IO.lift(() => Callbacks.Each(
                presets,
                (preset, index) => GrasshopperFault.Unless(parameter.Presets.Add(RowText.Localize(preset.Caption, plugin), "", preset.Value), index)))
            from coded in Coded(parameter, code, plugin)
            select coded);

    // --- [REALIZE]
    public IO<IParameter> Input(ModularInputAdder inputs, IGrasshopperPlugin plugin) =>
        from parameter in Attached(plugin)
        from _ in IO.lift(() => Modularity.Match(
            Some: optional =>
                (optional.Shown ? (Action<IParameter, string, Color, Requirement?>)inputs.Add : inputs.AddHidden)(
                    parameter, RowText.Localize(optional.Category, plugin), optional.Tint.IfNone(Colors.Transparent), null),
            None: () => inputs.RegularAdder.Add(parameter, parameter.Requirement)))
        select parameter;

    public IO<IParameter> Output(ModularOutputAdder outputs, IGrasshopperPlugin plugin) =>
        from parameter in Attached(plugin)
        from _ in IO.lift(() => Modularity.Match(
            Some: optional =>
                (optional.Shown ? (Action<IParameter, string, Color>)outputs.Add : outputs.AddHidden)(
                    parameter, RowText.Localize(optional.Category, plugin), optional.Tint.IfNone(Colors.Transparent)),
            None: () => outputs.RegularAdder.Add(parameter)))
        select parameter;

    private IO<IParameter> Attached(IGrasshopperPlugin plugin) =>
        from parameter in Create(plugin)
        from attached in Attach.TraverseM(attach => attach(parameter, plugin)).As()
        select parameter;

    private static Nomen Localized(Nomen face, IGrasshopperPlugin plugin) =>
        face.WithName(RowText.Localize(face.Name, plugin)).WithInfo(RowText.Localize(face.Info, plugin));

    private static IO<IParameter> Coded(IParameter parameter, string code, IGrasshopperPlugin plugin) =>
        IO.lift(() => parameter.UserName = RowText.Localize(code, plugin)).Map(parameter);
}
