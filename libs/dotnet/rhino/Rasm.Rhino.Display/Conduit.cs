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
    public Option<bool> SelectionFilter { get; }

    public Seq<Guid> ObjectIds { get; }

    public ObjectType GeometryFilter { get; }

    public ActiveSpace SpaceFilter { get; }

    public static ConduitFilter Default { get; } = Create(Option<bool>.None, Seq<Guid>(), ObjectType.AnyObject, ActiveSpace.None);

    public static Fin<ConduitFilter> From(Option<bool> selectionFilter, Seq<Guid> objectIds, ObjectType geometryFilter, ActiveSpace spaceFilter) =>
        Validate(selectionFilter, objectIds, geometryFilter, spaceFilter, out ConduitFilter? filter) is { } error ? error : filter!;

    static partial void ValidateFactoryArguments(ref ValidationFailure? validationError, ref Option<bool> selectionFilter, ref Seq<Guid> objectIds, ref ObjectType geometryFilter, ref ActiveSpace spaceFilter) =>
        validationError = spaceFilter is ActiveSpace.None or ActiveSpace.ModelSpace or ActiveSpace.PageSpace ? null : new UnsupportedSpace(spaceFilter);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ConduitBinding {
    public sealed record Everywhere() : ConduitBinding;

    public sealed record Viewports(Seq<ViewportTarget> Targets, bool Exclusive) : ConduitBinding;
}

public sealed record FrameContext(bool IsInViewCapture, bool IsPrinting, bool IsDynamicDisplay, int RenderPass, int NestLevel, float DpiScale, Guid ViewportId, uint ChangeCounter);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class ConduitMapper {
    internal static partial FrameContext ToFrame(DisplayPipeline pipeline, Guid viewportId, uint changeCounter);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(ConduitFilter.SelectionFilter), Justification = "DisplayConduit.SetSelectionFilter")]
    [MapperIgnoreSource(nameof(ConduitFilter.ObjectIds), Justification = "DisplayConduit.SetObjectIdFilter")]
    internal static partial void Update(ConduitFilter filter, DisplayConduit conduit);
}

public static class Conduits {
    // --- [BINDING]
    public static IO<IDisposable> Enable(RhinoDoc doc, Seq<DisplayConduit> conduits, ConduitFilter filter, ConduitBinding binding, Action<Error> reject) =>
        from rows in binding.Switch(
            doc,
            everywhere: static (_, _) => IO.pure(Seq<ViewportRef>()),
            viewports: static (document, bound) => DisposalOps
                .AcquireAll(bound.Targets.Map(target => Viewports.ResolveViewports(document, target)), static held => DisposalOps.Release(held.Flatten()))
                .Map(static held => held.Flatten()))
        from enabled in DisposalOps.OnFailure(IO.lift(() => conduits.Iter(conduit => Configured(conduit, filter, binding, rows))), DisposalOps.Release(rows))
        select (IDisposable)new Disposal(() => {
            _ = conduits.Iter(static conduit => {
                conduit.Enabled = false;
                conduit.UnbindAll();
            });
            _ = Answers.Answer(DisposalOps.Release(rows), reject, unit);
        });

    private static void Configured(DisplayConduit conduit, ConduitFilter filter, ConduitBinding binding, Seq<ViewportRef> rows) {
        conduit.SetSelectionFilter(filter.SelectionFilter.IsSome, filter.SelectionFilter.Exists(identity));
        conduit.SetObjectIdFilter(filter.ObjectIds);
        ConduitMapper.Update(filter, conduit);
        Action<RhinoViewport> bind = binding is ConduitBinding.Viewports { Exclusive: true } ? conduit.ExclusiveBind : conduit.Bind;
        _ = rows.Iter(row => bind(row.Viewport));
        conduit.Enabled = true;
    }

    // --- [PIPELINE]
    public static IO<TValue> WithState<TValue>(DisplayPipeline pipeline, Seq<RenderState> state, IO<TValue> draw) =>
        state.FoldBack(draw, (inner, render) => render.Switch(
            (Pipeline: pipeline, Draw: inner),
            depthTest: static (scope, depth) => IO.lift(() => scope.Pipeline.PushDepthTesting(depth.Enabled)).Bracket(Use: _ => scope.Draw, Fin: _ => IO.lift(scope.Pipeline.PopDepthTesting)),
            depthWrite: static (scope, depth) => IO.lift(() => scope.Pipeline.PushDepthWriting(depth.Enabled)).Bracket(Use: _ => scope.Draw, Fin: _ => IO.lift(scope.Pipeline.PopDepthWriting)),
            cullFace: static (scope, cull) => IO.lift(() => scope.Pipeline.PushCullFaceMode(cull.Mode)).Bracket(Use: _ => scope.Draw, Fin: _ => IO.lift(scope.Pipeline.PopCullFaceMode)),
            model: static (scope, model) => IO.lift(() => scope.Pipeline.PushModelTransform(model.Xform)).Bracket(Use: _ => scope.Draw, Fin: _ => IO.lift(scope.Pipeline.PopModelTransform)),
            projection2d: static (scope, _) => IO.lift(scope.Pipeline.Push2dProjection).Bracket(Use: _ => scope.Draw, Fin: _ => IO.lift(scope.Pipeline.PopProjection))));

    public static IO<Option<FrameContext>> ReadFrame(DisplayPipeline pipeline) =>
        IO.lift(() => Optional(pipeline.Viewport).Map(viewport => ConduitMapper.ToFrame(pipeline, viewport.Id, viewport.ChangeCounter)));
}
