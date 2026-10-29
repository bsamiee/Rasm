using Rasm.Rhino.Document;
using Rhino.Display;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Overlay(BoundingBox Bounds, Action<DisplayPipeline> Draw);

public sealed record RenderState(Func<DisplayPipeline, Action> Enter) {
    // --- [STATES]
    public static RenderState DepthTest(bool enabled) => new(pipeline => Pushed(() => pipeline.PushDepthTesting(enabled), pipeline.PopDepthTesting));

    public static RenderState DepthWrite(bool enabled) => new(pipeline => Pushed(() => pipeline.PushDepthWriting(enabled), pipeline.PopDepthWriting));

    public static RenderState Cull(CullFaceMode mode) => new(pipeline => Pushed(() => pipeline.PushCullFaceMode(mode), pipeline.PopCullFaceMode));

    public static RenderState Model(Transform xform) => new(pipeline => Pushed(() => pipeline.PushModelTransform(xform), pipeline.PopModelTransform));

    public static RenderState Projection2d { get; } = new(static pipeline => Pushed(pipeline.Push2dProjection, pipeline.PopProjection));

    public static RenderState Depth(DepthMode mode) => new(pipeline => Replaced(() => pipeline.DepthMode, held => pipeline.DepthMode = held, mode));

    public static RenderState Bias(ZBiasMode mode) => new(pipeline => Replaced(() => pipeline.ZBiasMode, held => pipeline.ZBiasMode = held, mode));

    public static RenderState Clipped(Plane plane) => new(pipeline => pipeline.AddClippingPlane(plane.Origin, plane.Normal) switch {
        var index => () => pipeline.RemoveClippingPlane(index),
    });

    public static RenderState Attributes(Action<DisplayPipelineAttributes> edit) =>
        new(pipeline => PipelineAttributes.Copy(pipeline.DisplayPipelineAttributes) switch {
            var held => Pushed(() => edit(pipeline.DisplayPipelineAttributes), () => {
                PipelineAttributes.CopyContents(pipeline.DisplayPipelineAttributes, held);
                held.Dispose();
            }),
        });

    // --- [DRAW]
    public Action<DisplayPipeline> Around(Action<DisplayPipeline> draw) =>
        pipeline => {
            Action pop = Enter(pipeline);
            Fin<Unit> drawn = Try.lift(fun(() => draw(pipeline))).Run();
            pop();
            _ = drawn.ThrowIfFail();
        };

    public static Action<DisplayPipeline> Around(Seq<RenderState> states, Action<DisplayPipeline> draw) =>
        states.FoldBack(draw, static (inner, state) => state.Around(inner));

    private static Action Pushed(Action push, Action pop) {
        push();
        return pop;
    }

    private static Action Replaced<T>(Func<T> read, Action<T> write, T value) =>
        read() switch {
            var prior => Pushed(() => write(value), () => write(prior)),
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Overlays {
    // --- [RETAINED]
    public static IO<Unit> Show(Atom<Seq<Overlay>> overlays, Func<Action<DrawEventArgs>, ConduitChannel> stage, ConduitBinding binding, RedrawPolicy redraw) =>
        from enabled in Conduits.Enable(
            new ConduitDefinition(Seq(
                ConduitChannel.BoundingBox(_ => Conversions.Present(overlays.Value.Fold(BoundingBox.Empty, static (box, overlay) => BoundingBox.Union(box, overlay.Bounds)))),
                stage(args => overlays.Value.Iter(overlay => overlay.Draw(args.Display))))) { Binding = binding },
            redraw)
        from redrawn in Subscriptions.Changed(overlays).Attach(_ =>
            binding.Documents.Bind(docs => Products.Each(docs.Map(doc => Commits.WithinRedraw(doc, redraw, IO.pure(unit))))).Map(unit))
        select unit;
}
