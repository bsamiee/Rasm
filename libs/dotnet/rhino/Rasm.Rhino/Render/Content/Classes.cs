using System.Drawing;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.UI;

namespace Rasm.Rhino.Render.Content;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ContentPreviewRow(
    RenderPlugIn.PreviewRenderTypes Type,
    Func<CreatePreviewEventArgs, IO<Option<Bitmap>>> Content,
    Func<CreateTexture2dPreviewEventArgs, IO<Option<Bitmap>>> Texture);

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedMaterial<TState> : RenderMaterial where TState : notnull {
    // --- [FIELDS]
    private readonly IPlugInViews owner;

    protected DefinedMaterial() => owner = HostedTypes.Owner(GetType());

    // --- [VALUES]
    protected abstract string Caption { get; }

    protected abstract string Help { get; }

    protected abstract string Svg { get; }

    protected abstract TState State { get; }

    protected abstract Material Simulation(TState state);

    // --- [IDENTITY]
    public sealed override string TypeName => RowText.Localize(Caption, owner).Local;

    public sealed override string TypeDescription => RowText.Localize(Help, owner).Local;

    public sealed override bool VirtualIcon(Size size, out Bitmap? bitmap) => (bitmap = DrawingUtilities.BitmapFromSvg(Svg, size.Width, size.Height, adjustForDarkMode: true)) is not null;

    protected sealed override void OnAddUserInterfaceSections() =>
        Callbacks.Run(IO.lift(() => Refused.Unless(AddAutomaticUserInterfaceSection(TypeName, 0), nameof(AddAutomaticUserInterfaceSection))), CallbackSite.Of(owner, this));

    // --- [SIMULATION]
    public sealed override void SimulateMaterial(ref Material simulation, RenderTexture.TextureGeneration tg) {
        Material target = simulation;
        if (!Callbacks.Succeeded(DisposalOps.Within(() => Simulation(State), built => IO.lift(() => target.CopyFrom(built))), CallbackSite.Of(owner, this)))
            base.SimulateMaterial(ref simulation, tg);
    }
}

public abstract class DefinedTexture<TState> : RenderTexture where TState : notnull {
    // --- [FIELDS]
    private readonly IPlugInViews owner;

    protected DefinedTexture() => owner = HostedTypes.Owner(GetType());

    // --- [VALUES]
    protected abstract string Caption { get; }

    protected abstract string Help { get; }

    protected abstract string Svg { get; }

    protected abstract TState State { get; }

    protected abstract Color4f Sample(TState state, Point3d uvw, Vector3d duvwdx, Vector3d duvwdy);

    // --- [IDENTITY]
    public sealed override string TypeName => RowText.Localize(Caption, owner).Local;

    public sealed override string TypeDescription => RowText.Localize(Help, owner).Local;

    public sealed override bool VirtualIcon(Size size, out Bitmap? bitmap) => (bitmap = DrawingUtilities.BitmapFromSvg(Svg, size.Width, size.Height, adjustForDarkMode: true)) is not null;

    protected sealed override void OnAddUserInterfaceSections() =>
        Callbacks.Run(IO.lift(() => Refused.Unless(AddAutomaticUserInterfaceSection(TypeName, 0), nameof(AddAutomaticUserInterfaceSection))), CallbackSite.Of(owner, this));

    // --- [SIMULATION]
    public sealed override TextureEvaluator CreateEvaluator(TextureEvaluatorFlags evaluatorFlags) =>
        new Evaluator(evaluatorFlags, State, evaluatorFlags.HasFlag(TextureEvaluatorFlags.DisableLocalMapping) ? Transform.Identity : LocalMappingTransform, Sample);

    private sealed class Evaluator(TextureEvaluatorFlags flags, TState state, Transform mapping, Func<TState, Point3d, Vector3d, Vector3d, Color4f> sample)
        : TextureEvaluator(flags) {
        public override Color4f GetColor(Point3d uvw, Vector3d duvwdx, Vector3d duvwdy) => sample(state, mapping * uvw, mapping * duvwdx, mapping * duvwdy);
    }
}

public abstract class DefinedEnvironment<TState> : RenderEnvironment where TState : notnull {
    // --- [FIELDS]
    private readonly IPlugInViews owner;

    protected DefinedEnvironment() => owner = HostedTypes.Owner(GetType());

    // --- [VALUES]
    protected abstract string Caption { get; }

    protected abstract string Help { get; }

    protected abstract string Svg { get; }

    protected abstract TState State { get; }

    protected abstract IO<Unit> Simulation(TState state, SimulatedEnvironment simulation, bool isForDataOnly);

    // --- [IDENTITY]
    public sealed override string TypeName => RowText.Localize(Caption, owner).Local;

    public sealed override string TypeDescription => RowText.Localize(Help, owner).Local;

    public sealed override bool VirtualIcon(Size size, out Bitmap? bitmap) => (bitmap = DrawingUtilities.BitmapFromSvg(Svg, size.Width, size.Height, adjustForDarkMode: true)) is not null;

    protected sealed override void OnAddUserInterfaceSections() =>
        Callbacks.Run(IO.lift(() => Refused.Unless(AddAutomaticUserInterfaceSection(TypeName, 0), nameof(AddAutomaticUserInterfaceSection))), CallbackSite.Of(owner, this));

    // --- [SIMULATION]
    public sealed override void SimulateEnvironment(ref SimulatedEnvironment simulation, bool isForDataOnly) {
        SimulatedEnvironment target = simulation;
        if (!Callbacks.Succeeded(Simulation(State, target, isForDataOnly), CallbackSite.Of(owner, this)))
            base.SimulateEnvironment(ref simulation, isForDataOnly);
    }
}
