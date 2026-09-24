using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderState {
    public sealed record DepthTest(bool Enabled) : RenderState;

    public sealed record DepthWrite(bool Enabled) : RenderState;

    public sealed record CullFace(CullFaceMode Mode) : RenderState;

    public sealed record Model(Transform Xform) : RenderState;

    public sealed record Projection2d() : RenderState;
}

[ComplexValueObject]
[ValidationError<ValidationFailure>]
public sealed partial class ConduitFilter {
    public Option<bool> SelectedOnly { get; }

    public Seq<Guid> ObjectIds { get; }

    public ObjectType Geometry { get; }

    public ActiveSpace Space { get; }

    public static ConduitFilter Default { get; } = Create(Option<bool>.None, Seq<Guid>(), ObjectType.AnyObject, ActiveSpace.None);

    public static Fin<ConduitFilter> From(Option<bool> selectedOnly, Seq<Guid> objectIds, ObjectType geometry, ActiveSpace space) =>
        Validate(selectedOnly, objectIds, geometry, space, out ConduitFilter? filter) is { } error ? error : filter!;

    static partial void ValidateFactoryArguments(ref ValidationFailure? validationError, ref Option<bool> selectedOnly, ref Seq<Guid> objectIds, ref ObjectType geometry, ref ActiveSpace space) =>
        validationError = space is ActiveSpace.None or ActiveSpace.ModelSpace or ActiveSpace.PageSpace ? null : new UnsupportedSpace(space);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ConduitBinding {
    public sealed record Everywhere() : ConduitBinding;

    public sealed record Viewports(Seq<ViewportTarget> Targets, bool Exclusive) : ConduitBinding;
}

public sealed record FrameContext(bool IsInViewCapture, bool IsPrinting, bool IsDynamicDisplay, int RenderPass, int NestLevel, float DpiScale, Guid ViewportId, uint ChangeCounter);

public sealed record ConduitCallbacks(
    Option<Func<CullObjectEventArgs, IO<bool>>> Cull,
    Option<Func<CalculateBoundingBoxEventArgs, IO<Option<BoundingBox>>>> Bounds,
    Option<Func<CalculateBoundingBoxEventArgs, IO<Option<BoundingBox>>>> BoundsZoomExtents,
    Option<Func<DrawEventArgs, IO<Unit>>> PreObjects,
    Option<Func<DrawObjectEventArgs, IO<bool>>> PreObject,
    Option<Func<DrawEventArgs, IO<Unit>>> PostObjects,
    Option<Func<DrawEventArgs, IO<Unit>>> Foreground,
    Option<Func<DrawEventArgs, IO<Unit>>> Overlay,
    Option<Func<PostProcessFrameBufferEventArgs, IO<Unit>>> PostProcess,
    ConduitFilter Filter,
    ConduitBinding Binding,
    Action<Error> Reject);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class CallbackConduit(ConduitCallbacks callbacks) : DisplayConduit {
    protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e) {
        base.CalculateBoundingBox(e);
        _ = Answers.Answer(callbacks.Bounds.Map(bounds => bounds(e)), callbacks.Reject, static () => Option<BoundingBox>.None).Iter(e.IncludeBoundingBox);
    }

    protected override void CalculateBoundingBoxZoomExtents(CalculateBoundingBoxEventArgs e) {
        base.CalculateBoundingBoxZoomExtents(e);
        _ = Answers.Answer(callbacks.BoundsZoomExtents.Map(bounds => bounds(e)), callbacks.Reject, static () => Option<BoundingBox>.None).Iter(e.IncludeBoundingBox);
    }

    protected override void PreDrawObjects(DrawEventArgs e) {
        base.PreDrawObjects(e);
        _ = Answers.Answer(callbacks.PreObjects.Map(draw => draw(e)), callbacks.Reject, static () => unit);
    }

    protected override void PostDrawObjects(DrawEventArgs e) {
        base.PostDrawObjects(e);
        _ = Answers.Answer(callbacks.PostObjects.Map(draw => draw(e)), callbacks.Reject, static () => unit);
    }

    protected override void DrawForeground(DrawEventArgs e) {
        base.DrawForeground(e);
        _ = Answers.Answer(callbacks.Foreground.Map(draw => draw(e)), callbacks.Reject, static () => unit);
    }

    protected override void DrawOverlay(DrawEventArgs e) {
        base.DrawOverlay(e);
        _ = Answers.Answer(callbacks.Overlay.Map(draw => draw(e)), callbacks.Reject, static () => unit);
    }
}

public sealed class ObjectCullingConduit(Func<CullObjectEventArgs, IO<bool>> hook, Action<Error> reject) : DisplayConduit {
    protected override void ObjectCulling(CullObjectEventArgs e) {
        base.ObjectCulling(e);
        e.CullObject = e.CullObject || Answers.Answer(hook(e), reject, fallback: false);
    }
}

public sealed class PreDrawObjectConduit(Func<DrawObjectEventArgs, IO<bool>> hook, Action<Error> reject) : DisplayConduit {
    protected override void PreDrawObject(DrawObjectEventArgs e) {
        base.PreDrawObject(e);
        e.DrawObject = e.DrawObject && !Answers.Answer(hook(e), reject, fallback: false);
    }
}

public sealed class FrameBufferConduit(Func<PostProcessFrameBufferEventArgs, IO<Unit>> hook, Action<Error> reject) : DisplayConduit {
    protected override void PostProcessFrameBuffer(PostProcessFrameBufferEventArgs e) {
        base.PostProcessFrameBuffer(e);
        _ = Answers.Answer(hook(e), reject, unit);
    }
}

public sealed class ConduitHandle : IDisposable {
    private readonly Disposal disposal;

    private ConduitHandle(Seq<DisplayConduit> conduits, Seq<ViewportRef> rows) =>
        disposal = new(() => {
            _ = conduits.Iter(static conduit => {
                conduit.Enabled = false;
                conduit.UnbindAll();
            });
            _ = Release(rows).Run();
        });

    public static IO<ConduitHandle> Enable(RhinoDoc doc, ConduitCallbacks callbacks) =>
        from rows in callbacks.Binding.Switch(
            doc,
            everywhere: static (_, _) => IO.pure(Seq<ViewportRef>()),
            viewports: static (document, bound) => Resolved(document, bound.Targets, Seq<ViewportRef>()))
        from conduits in GeometryOps.OnFailure(
            IO.lift(() => {
                Seq<DisplayConduit> enabled = (Seq<DisplayConduit>(new CallbackConduit(callbacks))
                        + callbacks.Cull.Map<DisplayConduit>(hook => new ObjectCullingConduit(hook, callbacks.Reject)).ToSeq()
                        + callbacks.PreObject.Map<DisplayConduit>(hook => new PreDrawObjectConduit(hook, callbacks.Reject)).ToSeq()
                        + callbacks.PostProcess.Map<DisplayConduit>(hook => new FrameBufferConduit(hook, callbacks.Reject)).ToSeq())
                    .Map(conduit => Configured(conduit, callbacks, rows))
                    .Strict();
                _ = enabled.Iter(static conduit => conduit.Enabled = true);
                return enabled;
            }),
            Release(rows))
        select new ConduitHandle(conduits, rows);

    public void Dispose() => disposal.Dispose();

    private static IO<Seq<ViewportRef>> Resolved(RhinoDoc doc, Seq<ViewportTarget> pending, Seq<ViewportRef> rows) =>
        pending.Match(
            Empty: () => IO.pure(rows),
            Tail: (next, rest) => GeometryOps.OnFailure(Viewports.ResolveViewports(doc, next), Release(rows))
                .Bind(resolved => Resolved(doc, rest, rows + resolved)));

    private static DisplayConduit Configured(DisplayConduit conduit, ConduitCallbacks callbacks, Seq<ViewportRef> rows) {
        conduit.SetSelectionFilter(callbacks.Filter.SelectedOnly.IsSome, callbacks.Filter.SelectedOnly.Exists(identity));
        conduit.SetObjectIdFilter(callbacks.Filter.ObjectIds);
        conduit.GeometryFilter = callbacks.Filter.Geometry;
        conduit.SpaceFilter = callbacks.Filter.Space;
        Action<RhinoViewport> bind = callbacks.Binding.Switch(everywhere: static _ => false, viewports: static bound => bound.Exclusive) ? conduit.ExclusiveBind : conduit.Bind;
        _ = rows.Iter(row => bind(row.Viewport));
        return conduit;
    }

    private static IO<Unit> Release(Seq<ViewportRef> rows) =>
        Disposal.Release(rows.Filter(static row => row.Detail.IsSome).Map(static row => row.Viewport));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Conduits {
    public static IO<TValue> WithState<TValue>(DisplayPipeline pipeline, Seq<RenderState> state, IO<TValue> draw) =>
        state.FoldBack(draw, (inner, render) => render.Switch(
            (Pipeline: pipeline, Draw: inner),
            depthTest: static (scope, depth) => Disposal.Bracketed(() => scope.Pipeline.PushDepthTesting(depth.Enabled), scope.Pipeline.PopDepthTesting, scope.Draw),
            depthWrite: static (scope, depth) => Disposal.Bracketed(() => scope.Pipeline.PushDepthWriting(depth.Enabled), scope.Pipeline.PopDepthWriting, scope.Draw),
            cullFace: static (scope, cull) => Disposal.Bracketed(() => scope.Pipeline.PushCullFaceMode(cull.Mode), scope.Pipeline.PopCullFaceMode, scope.Draw),
            model: static (scope, model) => Disposal.Bracketed(() => scope.Pipeline.PushModelTransform(model.Xform), scope.Pipeline.PopModelTransform, scope.Draw),
            projection2d: static (scope, _) => Disposal.Bracketed(scope.Pipeline.Push2dProjection, scope.Pipeline.PopProjection, scope.Draw)));

    public static IO<Option<FrameContext>> ReadFrame(DisplayPipeline pipeline) =>
        IO.lift(() => Optional(pipeline.Viewport).Map(viewport => ConduitMapper.ToFrame(pipeline, viewport.Id, viewport.ChangeCounter)));
}

[Mapper]
internal static partial class ConduitMapper {
    internal static partial FrameContext ToFrame(DisplayPipeline pipeline, Guid viewportId, uint changeCounter);
}
