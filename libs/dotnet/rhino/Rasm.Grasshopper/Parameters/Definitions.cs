using Eto.Drawing;
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.UI;

namespace Rasm.Grasshopper.Parameters;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Modularity(string Category, Option<Color> Tint, bool Shown);

public sealed record ParameterRow(IO<IParameter> Create) {
    // --- [DECLARE]
    public Option<Modularity> Modularity { get; init; }
    public Seq<Func<IParameter, IGrasshopperPlugin, IO<Unit>>> Attach { get; init; }

    public static ParameterRow Of(Nomen face, string code, Func<Nomen, IParameter> construct) =>
        new(IO.lift(() => Coded(construct(face), code)));

    public static ParameterRow Of<TData>(Nomen face, string code, Func<Nomen, Parameter<TData>> construct, Seq<Preset<TData>> presets)
        where TData : notnull =>
        new(
            from parameter in IO.lift(() => construct(face))
            from added in IO.lift(() => presets.Map((preset, index) => GrasshopperFault.Unless(parameter.Presets.Add(preset), index)).Traverse(static added => added).As())
            select Coded(parameter, code));

    // --- [REALIZE]
    public IO<IParameter> Input(ModularInputAdder inputs, IGrasshopperPlugin plugin) =>
        from parameter in Attached(plugin)
        from added in IO.lift(() => Modularity.Match(
            Some: optional =>
                (optional.Shown ? (Action<IParameter, string, Color, Requirement?>)inputs.Add : inputs.AddHidden)(parameter, optional.Category, optional.Tint.IfNone(Colors.Transparent), null),
            None: () => inputs.RegularAdder.Add(parameter, parameter.Requirement)))
        select parameter;

    public IO<IParameter> Output(ModularOutputAdder outputs, IGrasshopperPlugin plugin) =>
        from parameter in Attached(plugin)
        from added in IO.lift(() => Modularity.Match(
            Some: optional => (optional.Shown ? (Action<IParameter, string, Color>)outputs.Add : outputs.AddHidden)(parameter, optional.Category, optional.Tint.IfNone(Colors.Transparent)),
            None: () => outputs.RegularAdder.Add(parameter)))
        select parameter;

    private IO<IParameter> Attached(IGrasshopperPlugin plugin) =>
        from parameter in Create
        from attached in Attach.TraverseM(attach => attach(parameter, plugin)).As()
        select parameter;

    private static IParameter Coded(IParameter parameter, string code) {
        parameter.UserName = code;
        return parameter;
    }
}
