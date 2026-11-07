using Rasm.Rhino.Document;
using Rhino.Display;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record Overlay(BoundingBox Bounds, Action<DisplayPipeline> Draw);

public sealed record RenderState(Action<DisplayPipeline, Action<DisplayPipeline>> Draw) {

    // --- [STATES]

    public static RenderState DepthTest(bool enabled) => Pushed(pipeline => pipeline.PushDepthTesting(enabled), static pipeline => pipeline.PopDepthTesting());

    public static RenderState DepthWrite(bool enabled) => Pushed(pipeline => pipeline.PushDepthWriting(enabled), static pipeline => pipeline.PopDepthWriting());

    public static RenderState Cull(CullFaceMode mode) => Pushed(pipeline => pipeline.PushCullFaceMode(mode), static pipeline => pipeline.PopCullFaceMode());

    public static RenderState Model(Transform xform) => Pushed(pipeline => pipeline.PushModelTransform(xform), static pipeline => pipeline.PopModelTransform());

    public static RenderState Projection2d { get; } = Pushed(static pipeline => pipeline.Push2dProjection(), static pipeline => pipeline.PopProjection());

    public static RenderState Depth(DepthMode mode) => Replaced(static pipeline => pipeline.DepthMode, static (pipeline, held) => pipeline.DepthMode = held, mode);

    public static RenderState Bias(ZBiasMode mode) => Replaced(static pipeline => pipeline.ZBiasMode, static (pipeline, held) => pipeline.ZBiasMode = held, mode);

    public static RenderState Clipped(Plane plane) => new((pipeline, draw) => {
        int index = pipeline.AddClippingPlane(plane.Origin, plane.Normal);
        try {
            draw(pipeline);
        } finally {
            pipeline.RemoveClippingPlane(index);
        }
    });

    public static RenderState Attributes(Action<DisplayPipelineAttributes> edit) =>
        new((pipeline, draw) => {
            DisplayPipelineAttributes attributes = pipeline.DisplayPipelineAttributes;
            using DisplayPipelineAttributes held = PipelineAttributes.Copy(attributes);
            try {
                edit(attributes);
                draw(pipeline);
            } finally {
                PipelineAttributes.CopyContents(attributes, held);
            }
        });

    // --- [DRAWING]

    public Action<DisplayPipeline> Around(Action<DisplayPipeline> draw) => pipeline => Draw(pipeline, draw);

    public static Action<DisplayPipeline> Around(Seq<RenderState> states, Action<DisplayPipeline> draw) =>
        states.FoldBack(draw, static (inner, state) => state.Around(inner));

    private static RenderState Pushed(Action<DisplayPipeline> push, Action<DisplayPipeline> pop) =>
        new((pipeline, draw) => {
            push(pipeline);
            try {
                draw(pipeline);
            } finally {
                pop(pipeline);
            }
        });

    private static RenderState Replaced<T>(Func<DisplayPipeline, T> read, Action<DisplayPipeline, T> write, T value) =>
        new((pipeline, draw) => {
            T prior = read(pipeline);
            try {
                write(pipeline, value);
                draw(pipeline);
            } finally {
                write(pipeline, prior);
            }
        });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Overlays {
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
