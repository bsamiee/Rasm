using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Rhino.Events;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI;
using Rhino.Render;

namespace Darkroom.Chain;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RenderRun(PixelFrame Input, PixelFrame Formed, ChainScene Scene, HashMap<GuideChannel, PixelFrame> Guides, Seq<(Guid Id, string Name)> After) {
    public PassContext Context(Derivations derivations, Option<OutputDepth> depth) => Scene.Context(Input.Extent, Guides, derivations, rendering: false, depth);
}

public sealed record ChainRequest(PixelFrame Input, ChainState State) {
    private static readonly IProgress<int> Unreported = new Progress<int>();

    public IO<(PixelFrame Frame, PassContext Context)> Formed(PassContext context) =>
        from chain in State.Passes(context)
        let formed = chain.Passes.IsEmpty ? Input : Input.Copy()
        from ran in chain.Passes.TraverseM(step => IO.lift(() => step.Pass.Run(formed, Unreported))).As()
        select (formed, chain.Context);

    public IO<Seq<(Guid Kind, PixelFrame Input)>> Inputs(PassContext context) =>
        from chain in State.Passes(context)
        let formed = Input.Copy()
        from inputs in chain.Passes.TraverseM(step => IO.lift(() => formed.Copy() switch {
            var input => step.Pass.Run(formed, Unreported).Map(_ => (step.Kind.Id, input)),
        })).As()
        select inputs;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class RenderRuns {
    private readonly Atom<Seq<(Guid Rendering, RenderRun Run)>> runs = Atom(Seq<(Guid Rendering, RenderRun Run)>());

    private RenderRuns() => Changed = Subscriptions.Changed(runs).Choose(static _ => Some(unit));

    public static PlugInSlot<RenderRuns> Slot { get; } = new();

    public static Func<IPlugInViews, IO<Unit>> Register { get; } = static views => Slot.Publish(views, new RenderRuns());

    public HostEvent<Unit> Changed { get; }

    public Option<(Guid Rendering, RenderRun Run)> Newest => runs.Value.Head;

    public Option<RenderRun> Find(Guid rendering) => runs.Value.Find(entry => entry.Rendering == rendering).Map(static entry => entry.Run);

    internal IO<Unit> Recorded(Guid rendering, RenderRun run) =>
        runs.SwapIO(entries => (rendering, run).Cons(entries.Filter(entry => entry.Rendering != rendering).Take(SupportOptions.AutoSaveKeepAmount() - 1))).Map(unit);
}
