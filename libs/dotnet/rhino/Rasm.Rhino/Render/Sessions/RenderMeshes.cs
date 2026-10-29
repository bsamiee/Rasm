using System.Collections.Generic;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render.CustomRenderMeshes;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record MeshRequest(MeshType Type, ViewportInfo Viewport, RenderMeshProvider.Flags Flags, Option<PlugIn> Requester, Option<DisplayPipelineAttributes> Display);

public sealed record MeshBuild(Seq<Instance> Instances, ContentKey Inputs, RenderMeshProvider.Flags Flags);

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedMeshProvider(string name) : RenderMeshProvider {
    // --- [IDENTITY]
    public sealed override string Name => LocalText.Of(name).Local;

    // --- [CALLBACKS]
    public sealed override List<Guid> NonObjectIds => [.. Session.Run(ProvidedIds, failed: Seq<Guid>())];

    public sealed override bool HasCustomRenderMeshes(MeshType mt, ViewportInfo vp, RhinoDoc doc, Guid objectId, ref Flags flags, PlugIn plugin, DisplayPipelineAttributes attrs) {
        MeshRequest request = new(mt, vp, flags, Optional(plugin), Optional(attrs));
        (bool Provides, Flags Flags) answer = Session.Run(Provides(request, doc, objectId), failed: (Provides: false, request.Flags));
        flags = answer.Flags;
        return answer.Provides;
    }

    public sealed override RenderMeshes? RenderMeshes(
        MeshType mt, ViewportInfo vp, RhinoDoc doc, Guid objectId, List<InstanceObject> ancestry, ref Flags flags, RenderMeshes previousPrimitives, PlugIn plugin, DisplayPipelineAttributes attrs) {
        MeshRequest request = new(mt, vp, flags, Optional(plugin), Optional(attrs));
        (Option<RenderMeshes> Meshes, Flags Flags) answer = Session.Run(
            from previous in IO.lift(() => toSeq(previousPrimitives).Strict())
            from built in Build(request, doc, objectId, Conversions.Rows(ancestry), previous)
            let hash = uint.CreateTruncating((UInt128)ContentKey.Of<DefinedMeshProvider>(stream => stream.Integer(previousPrimitives.Hash).Id(ProviderId).Integer((UInt128)built.Inputs)))
            from meshes in Products.Transfer(() => new RenderMeshes(doc, objectId, ProviderId, hash, (uint)built.Flags), held => IO.lift(() => built.Instances.Iter(held.AddInstance)).Map(_ => held))
            select (Some(meshes), built.Flags),
            failed: (None, request.Flags));
        flags = answer.Flags;
        return answer.Meshes.ValueUnsafe();
    }

    public sealed override RenderMeshProviderProgress? Progress(RhinoDoc doc, Guid[] optional_objectIds) =>
        Session.Run(
            Progressing(doc, Conversions.Rows(optional_objectIds)).Map(found => found.Map(progress => {
                progress.Text = LocalText.Of(progress.Text).Local;
                progress.ProviderId = ProviderId;
                return progress;
            })),
            failed: Option<RenderMeshProviderProgress>.None).ValueUnsafe();

    public sealed override object? GetParameter(RhinoDoc doc, Guid objectId, string parameterName) =>
        Session.Run(Parameter(doc, objectId, parameterName), failed: Option<object>.None).ValueUnsafe();

    public sealed override void SetParameter(RhinoDoc doc, Guid objectId, string parameterName, object value) =>
        _ = Session.Run(Parametrized(doc, objectId, parameterName, value));

    // --- [HOOKS]
    protected virtual IO<Seq<Guid>> ProvidedIds => IO.pure(Seq<Guid>());

    protected abstract IO<(bool Provides, Flags Flags)> Provides(MeshRequest request, RhinoDoc document, Guid objectId);

    protected abstract IO<MeshBuild> Build(MeshRequest request, RhinoDoc document, Guid objectId, Seq<InstanceObject> ancestry, Seq<Instance> previous);

    protected virtual IO<Option<RenderMeshProviderProgress>> Progressing(RhinoDoc document, Seq<Guid> objectIds) => IO.pure(Option<RenderMeshProviderProgress>.None);

    protected virtual IO<Option<object>> Parameter(RhinoDoc document, Guid objectId, string name) => IO.pure(Option<object>.None);

    protected virtual IO<Unit> Parametrized(RhinoDoc document, Guid objectId, string name, object value) => IO.fail<Unit>(new RhinoFault($"{Name} parameter {name}", RhinoReason.Unsupported));

    // --- [REGISTRATION]
    public static IO<Unit> Register(Func<DefinedMeshProvider> create) =>
        (from provider in use(create)
         from accepted in Products.Accepted(() => RegisterProvider(provider, Session.PlugIn))
         select accepted).Scoped(Session.Scope);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Meshing {
    // --- [CUSTOM_MESHES]
    public static IO<RenderMeshes> RenderMeshes(RhinoObject rhinoObject, MeshRequest request, Seq<InstanceObject> ancestry) =>
        Products.Of(() => Rendered(rhinoObject, request, ancestry))
            .Bind(static read => read.Out.HasFlag(RenderMeshProvider.Flags.Canceled)
                ? release(read.Value) >> IO.fail<(RenderMeshes Value, RenderMeshProvider.Flags Out)>(Errors.Cancelled)
                : read.Out.HasFlag(RenderMeshProvider.Flags.Incomplete)
                    ? release(read.Value) >> IO.lift(static () => RhinoApp.Wait()) >> IO.pure(read)
                    : IO.pure(read))
            .RepeatUntil(static read => !read.Out.HasFlag(RenderMeshProvider.Flags.Incomplete))
            .Map(static read => read.Value);

    private static (RenderMeshes? Value, RenderMeshProvider.Flags Out) Rendered(RhinoObject rhinoObject, MeshRequest request, Seq<InstanceObject> ancestry) {
        RenderMeshProvider.Flags flags = request.Flags;
        return (rhinoObject.RenderMeshes(request.Type, request.Viewport, [.. ancestry], ref flags, request.Requester.ValueUnsafe(), request.Display.ValueUnsafe()), flags);
    }

    // --- [PARAMETERS]
    public static IO<Option<T>> GetCustomRenderMeshParameter<T>(RhinoObject rhinoObject, Guid providerId, string parameterName) where T : notnull =>
        Internals.Unwrapped<T>(() => rhinoObject.GetCustomRenderMeshParameter(providerId, parameterName), parameterName);
}
