using System.Collections.Generic;
using System.Globalization;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Modeling;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render.CustomRenderMeshes;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record MeshRequest(MeshType Type, ViewportInfo Viewport, RenderMeshProvider.Flags Flags, Option<PlugIn> Requester, Option<DisplayPipelineAttributes> Display);

public sealed record MeshProvision(bool Provides, RenderMeshProvider.Flags Flags);

public sealed record MeshBuild(Seq<Instance> Instances, ContentKey Inputs, RenderMeshProvider.Flags Flags);

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record MeshRun {
    public sealed record Unattended(bool WorkerThread) : MeshRun;

    public sealed record Styled(int Interface, Transform Xform) : MeshRun;
}

public sealed record MeshRow(Mesh Mesh, ObjectAttributes Attributes);

public sealed record MeshBatch(Seq<MeshRow> Rows, Option<int> Interface);

// --- [SERVICES] ------------------------------------------------------------------------
[Mapper(UseDeepCloning = true)]
public abstract partial class DefinedMeshProvider(IPlugInViews owner, string name) : RenderMeshProvider {
    // --- [IDENTITY]
    public sealed override string Name => RowText.Localize(name, owner).Local;

    public sealed override Guid ProviderId => ContentKey.Of<MeshBuild>(stream => stream.Id(owner.PlugIn.Id).Id(GetType().GUID)).Id;

    // --- [CALLBACKS]
    public sealed override List<Guid> NonObjectIds => [.. owner.Answer(ProvidedIds, failed: Seq<Guid>())];

    public sealed override bool HasCustomRenderMeshes(MeshType mt, ViewportInfo vp, RhinoDoc doc, Guid objectId, ref Flags flags, PlugIn plugin, DisplayPipelineAttributes attrs) {
        MeshRequest request = new(mt, vp, flags, Optional(plugin), Optional(attrs));
        MeshProvision answer = owner.Answer(Provides(request, doc, objectId), failed: new MeshProvision(Provides: false, request.Flags));
        flags = answer.Flags;
        return answer.Provides;
    }

    public sealed override RenderMeshes? RenderMeshes(MeshType mt, ViewportInfo vp, RhinoDoc doc, Guid objectId, List<InstanceObject> ancestry, ref Flags flags, RenderMeshes previousPrimitives, PlugIn plugin, DisplayPipelineAttributes attrs) {
        MeshRequest request = new(mt, vp, flags, Optional(plugin), Optional(attrs));
        MeshAnswer answer = owner.Answer(
            from previous in IO.lift(() => toSeq(previousPrimitives).Strict())
            from lent in previous.TraverseM(static instance => use(() => instance.Mesh) >> use(() => instance)).As()
            from built in Build(request, doc, objectId, Conversions.Rows(ancestry), previous)
            let providerId = ProviderId
            let hash = uint.CreateTruncating((UInt128)ContentKey.Of<MeshBuild>(stream => stream.Integer(previousPrimitives.Hash).Id(providerId).Integer((UInt128)built.Inputs)))
            from meshes in IO.lift(() => new RenderMeshes(doc, objectId, providerId, hash, (uint)built.Flags))
            from added in IO.lift(() => built.Instances.Iter(meshes.AddInstance)).Catch(error => IO.lift(meshes.Dispose) >> IO.fail<Unit>(error))
            select new MeshAnswer(Some(meshes), built.Flags),
            failed: new MeshAnswer(None, request.Flags));
        flags = answer.Flags;
        return answer.Meshes.ValueUnsafe();
    }

    public sealed override RenderMeshProviderProgress? Progress(RhinoDoc doc, Guid[] optional_objectIds) =>
        owner.Answer(
            from ids in IO.lift(() => Conversions.Rows(optional_objectIds))
            from found in Progressing(doc, ids)
            select found.Map(progress => ToProgress(progress, RowText.Localize(progress.Text, owner).Local, ProviderId)),
            failed: Option<RenderMeshProviderProgress>.None).ValueUnsafe();

    private static partial RenderMeshProviderProgress ToProgress(RenderMeshProviderProgress progress, string text, Guid providerId);

    private sealed record MeshAnswer(Option<RenderMeshes> Meshes, Flags Flags);

    // --- [HOOKS]
    protected virtual IO<Seq<Guid>> ProvidedIds => IO.pure(Seq<Guid>());

    protected abstract IO<MeshProvision> Provides(MeshRequest request, RhinoDoc document, Guid objectId);

    protected abstract IO<MeshBuild> Build(MeshRequest request, RhinoDoc document, Guid objectId, Seq<InstanceObject> ancestry, Seq<Instance> previous);

    protected virtual IO<Option<RenderMeshProviderProgress>> Progressing(RhinoDoc document, Seq<Guid> objectIds) => IO.pure(Option<RenderMeshProviderProgress>.None);

    // --- [REGISTRATION]
    public static Func<IPlugInViews, IO<Unit>> Register(Func<IPlugInViews, DefinedMeshProvider> create) =>
        owner =>
            from provider in use(() => create(owner))
            from accepted in IO.lift(() => RhinoFault.Unless(RegisterProvider(provider, owner.PlugIn)))
            select accepted;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Meshing {
    // --- [CACHES]
    public static IO<Seq<Mesh>> GetMeshes(RhinoObject rhinoObject, MeshType type) =>
        IO.lift(() => Conversions.Rows(rhinoObject.GetMeshes(type))).Bind(static cached => cached.TraverseM(Copies.Duplicate).As());

    // --- [CUSTOM_MESHES]
    public static IO<RenderMeshes> RenderMeshes(RhinoObject rhinoObject, MeshRequest request, Seq<InstanceObject> ancestry) =>
        (from read in IO.lift(() => Rendered(rhinoObject, request, ancestry))
         from settled in read.Flags.HasFlag(RenderMeshProvider.Flags.Canceled)
             ? IO.lift(read.Meshes.Dispose) >> IO.fail<MeshRead>(Errors.Cancelled)
             : read.Flags.HasFlag(RenderMeshProvider.Flags.Incomplete)
                 ? IO.lift(read.Meshes.Dispose) >> IO.lift(static () => RhinoApp.Wait()) >> IO.pure(read)
                 : IO.pure(read)
         select settled)
        .RepeatUntil(static read => !read.Flags.HasFlag(RenderMeshProvider.Flags.Incomplete))
        .Bind(static read => use(() => read.Meshes));

    private static Fin<MeshRead> Rendered(RhinoObject rhinoObject, MeshRequest request, Seq<InstanceObject> ancestry) {
        RenderMeshProvider.Flags flags = request.Flags;
        return Conversions.Found(rhinoObject.RenderMeshes(request.Type, request.Viewport, [.. ancestry], ref flags, request.Requester.ValueUnsafe(), request.Display.ValueUnsafe()))
            .Map(meshes => new MeshRead(meshes, flags));
    }

    // --- [PROVIDER_PARAMETERS]
    public static IO<Option<T>> GetCustomRenderMeshParameter<T>(RhinoObject rhinoObject, Guid providerId, string parameterName) where T : notnull =>
        IO.lift(() => rhinoObject.GetCustomRenderMeshParameter(providerId, parameterName)).Bind(static held =>
            IO.lift(() => Optional(held).Map(static value => (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture)))
                .Finally(IO.lift(() => (held as IDisposable)?.Dispose())));

    // --- [BATCH]
    public static IO<MeshBatch> MeshObjects(Seq<RhinoObject> rhinoObjects, MeshingParameters parameters, MeshRun run) =>
        from made in IO.lift(() => run.Switch(
            (Objects: rhinoObjects, Parameters: parameters),
            unattended: static (state, unattended) =>
                (Ended: Results.Ended(RhinoObject.MeshObjects(state.Objects, state.Parameters, out Mesh[] meshes, out ObjectAttributes[] attributes, unattended.WorkerThread)),
                    Meshes: Products.Made(meshes), Attributes: Products.Made(attributes), Style: Option<int>.None),
            styled: static (state, styled) => {
                int style = styled.Interface;
                return (
                    Ended: Results.Ended(RhinoObject.MeshObjects(state.Objects, ref state.Parameters, ref style, styled.Xform, out Mesh[] meshes, out ObjectAttributes[] attributes)),
                    Meshes: Products.Made(meshes), Attributes: Products.Made(attributes), Style: Some(style));
            }))
        from meshes in made.Meshes
        from attributes in made.Attributes
        from ended in IO.lift(made.Ended)
        select new MeshBatch(meshes.Zip(attributes, static (mesh, row) => new MeshRow(mesh, row)), made.Style);

    private sealed record MeshRead(RenderMeshes Meshes, RenderMeshProvider.Flags Flags);
}
