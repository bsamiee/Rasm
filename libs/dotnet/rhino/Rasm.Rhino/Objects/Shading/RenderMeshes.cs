using System.Globalization;
using Rasm.Rhino.Document.Shapes;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render.CustomRenderMeshes;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Objects.Shading;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record MeshRequest(MeshType Type, ViewportInfo Viewport, RenderMeshProvider.Flags Flags, Option<PlugIn> Requester, Option<DisplayPipelineAttributes> Display);

public sealed record MeshBuild(Seq<Instance> Instances, ContentKey Inputs, RenderMeshProvider.Flags Flags);

[SmartEnum<int>(SkipIParsable = true, SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public sealed partial class MeshInterface {
    public static readonly MeshInterface NoInterface = new(-1);
    public static readonly MeshInterface SimpleDialog = new(0);
    public static readonly MeshInterface DetailedDialog = new(1);
    public static readonly MeshInterface Scripted = new(2);
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record MeshRun {
    public sealed record Unattended(bool WorkerThread) : MeshRun;

    public sealed record Styled(MeshInterface Interface, Transform Xform) : MeshRun;
}

public sealed record MeshBatch(Seq<(Mesh Mesh, ObjectAttributes Attributes)> Rows, MeshInterface Interface);

// --- [SERVICES] ------------------------------------------------------------------------
[Mapper(UseDeepCloning = true)]
public abstract partial class DefinedMeshProvider(PlugIn plugIn, IPlugInSink sink, string name) : RenderMeshProvider {
    // --- [IDENTITY]
    public sealed override string Name => RowText.Localize(name, table: Some<object>(sink)).Local;

    public sealed override Guid ProviderId =>
        ContentKey.Of(KeyDomain.RenderMeshes, stream => stream.Id(plugIn.Id).Id(GetType().GUID)).Id;

    // --- [CALLBACKS]
    public sealed override List<Guid> NonObjectIds =>
        [.. Callbacks.Answer(this, static provider => provider.ProvidedIds, static () => Seq<Guid>(), new(sink, GetType(), nameof(NonObjectIds)))];

    public sealed override bool HasCustomRenderMeshes(MeshType mt, ViewportInfo vp, RhinoDoc doc, Guid objectId, ref Flags flags, PlugIn plugin, DisplayPipelineAttributes attrs) {
        MeshRequest request = new(mt, vp, flags, Optional(plugin), Optional(attrs));
        (bool Provides, Flags Flags) answer = Callbacks.Answer(
            Provides(request, doc, objectId), () => (false, request.Flags), new(sink, GetType(), nameof(HasCustomRenderMeshes)));
        flags = answer.Flags;
        return answer.Provides;
    }

    public sealed override RenderMeshes? RenderMeshes(MeshType mt, ViewportInfo vp, RhinoDoc doc, Guid objectId, List<InstanceObject> ancestry, ref Flags flags, RenderMeshes previousPrimitives, PlugIn plugin, DisplayPipelineAttributes attrs) {
        MeshRequest request = new(mt, vp, flags, Optional(plugin), Optional(attrs));
        (Option<RenderMeshes> Meshes, Flags Flags) answer = Callbacks.Answer(
            from built in Build(request, doc, objectId, Conversions.Rows(ancestry), Conversions.Rows(previousPrimitives))
            let providerId = ProviderId
            let hash = uint.CreateTruncating((UInt128)ContentKey.Of(KeyDomain.RenderMeshes, stream => stream.Integer(previousPrimitives.Hash).Id(providerId).Integer((UInt128)built.Inputs)))
            from meshes in IO.lift(() => new RenderMeshes(doc, objectId, providerId, hash, (uint)built.Flags))
            from added in DisposalOps.OnFailure(IO.lift(() => built.Instances.Iter(meshes.AddInstance)), IO.lift(meshes.Dispose))
            select (Some(meshes), built.Flags),
            () => (Option<RenderMeshes>.None, request.Flags), new(sink, GetType(), nameof(RenderMeshes)));
        flags = answer.Flags;
        return answer.Meshes.ValueUnsafe();
    }

    public sealed override RenderMeshProviderProgress? Progress(RhinoDoc doc, Guid[] optional_objectIds) =>
        Callbacks.Answer(
            from ids in IO.lift(() => Conversions.Rows(optional_objectIds))
            from found in Progressing(doc, ids)
            select found.Map(progress => ToProgress(progress, RowText.Localize(progress.Text, table: Some<object>(sink)).Local, ProviderId)),
            static () => Option<RenderMeshProviderProgress>.None, new(sink, GetType(), nameof(Progress))).ValueUnsafe();

    private static partial RenderMeshProviderProgress ToProgress(RenderMeshProviderProgress progress, string text, Guid providerId);

    // --- [HOOKS]
    protected virtual IO<Seq<Guid>> ProvidedIds => IO.pure(Seq<Guid>());

    protected abstract IO<(bool Provides, Flags Flags)> Provides(MeshRequest request, RhinoDoc document, Guid objectId);

    protected abstract IO<MeshBuild> Build(MeshRequest request, RhinoDoc document, Guid objectId, Seq<InstanceObject> ancestry, Seq<Instance> previous);

    protected virtual IO<Option<RenderMeshProviderProgress>> Progressing(RhinoDoc document, Seq<Guid> objectIds) => IO.pure(Option<RenderMeshProviderProgress>.None);

    // --- [REGISTRATION]
    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register(Func<PlugIn, IPlugInSink, DefinedMeshProvider> create) =>
        (plugIn, sink) => from provider in IO.lift(() => create(plugIn, sink))
                          from registered in DisposalOps.OnFailure(
                              from identified in IO.lift(() => MissingGuid.Unless(provider.GetType()))
                              from accepted in IO.lift(() => Refused.Unless(RegisterProvider(provider, plugIn), nameof(RegisterProvider)))
                              select accepted,
                              IO.lift(() => provider.Dispose()))
                          select (IDisposable)provider;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Meshing {
    // --- [CACHES]
    public static IO<Seq<Mesh>> GetMeshes(RhinoObject rhinoObject, MeshType type) =>
        IO.lift(() => Conversions.Rows(rhinoObject.GetMeshes(type))).Bind(static cached => DisposalOps.AcquireAll(cached.Map(Copies.Duplicate), DisposalOps.Release));

    // --- [CUSTOM_MESHES]
    public static IO<RenderMeshes> RenderMeshes(RhinoObject rhinoObject, MeshRequest request, Seq<InstanceObject> ancestry) =>
        (from read in IO.lift(() => {
            RenderMeshProvider.Flags flags = request.Flags;
            return Missing.Unless(
                rhinoObject.RenderMeshes(request.Type, request.Viewport, [.. ancestry], ref flags, request.Requester.ValueUnsafe(), request.Display.ValueUnsafe()),
                nameof(RhinoObject.RenderMeshes)).Map(meshes => (Meshes: meshes, Flags: flags));
        })
         from canceled in when(read.Flags.HasFlag(RenderMeshProvider.Flags.Canceled), IO.lift(read.Meshes.Dispose).Bind(static _ => IO.fail<Unit>(Errors.Cancelled))).As()
         from completed in read.Flags.HasFlag(RenderMeshProvider.Flags.Incomplete)
             ? IO.lift(read.Meshes.Dispose).Bind(static _ => IO.lift(static () => RhinoApp.Wait())).Map(_ => read)
             : IO.pure(read)
         select completed)
        .RepeatUntil(static read => !read.Flags.HasFlag(RenderMeshProvider.Flags.Incomplete))
        .Map(static read => read.Meshes);

    // --- [PROVIDER_PARAMETERS]
    public static IO<Option<T>> GetCustomRenderMeshParameter<T>(RhinoObject rhinoObject, Guid providerId, string parameterName) where T : notnull =>
        from held in IO.lift(() => Optional(rhinoObject.GetCustomRenderMeshParameter(providerId, parameterName)))
        from converted in held.Traverse(static value =>
            IO.lift(() => (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture))
                .Finally(DisposalOps.Release(Optional(value as IDisposable).ToSeq()))).As()
        select converted;

    // --- [BATCH]
    public static IO<MeshBatch> MeshObjects(Seq<RhinoObject> rhinoObjects, MeshingParameters parameters, MeshRun run) =>
        Copies.Owned(
            IO.lift(() => run.Switch(
                (Objects: rhinoObjects, Parameters: parameters),
                unattended: static (state, unattended) => (
                    Result: RhinoObject.MeshObjects(state.Objects, state.Parameters, out Mesh[] meshes, out ObjectAttributes[] attributes, unattended.WorkerThread),
                    Meshes: meshes, Attributes: attributes, Interface: Fin.Succ(MeshInterface.NoInterface)),
                styled: static (state, styled) => {
                    int style = styled.Interface.Key;
                    return (
                        Result: RhinoObject.MeshObjects(state.Objects, ref state.Parameters, ref style, styled.Xform, out Mesh[] meshes, out ObjectAttributes[] attributes),
                        Meshes: meshes, Attributes: attributes, Interface: Conversions.Validated<MeshInterface, int, InvalidRhinoValue>(style));
                })),
            static made => from accepted in Conversions.FromResult(made.Result, nameof(RhinoObject.MeshObjects))
                           from closed in made.Interface
                           from meshes in Measurements.Valid(toSeq<Mesh?>(made.Meshes), nameof(RhinoObject.MeshObjects))
                           select new MeshBatch(meshes.Zip(Conversions.Rows(made.Attributes), static (mesh, attributes) => (Mesh: mesh, Attributes: attributes)), closed),
            static made => DisposalOps.Release(Conversions.Rows<IDisposable>(made.Meshes) + Conversions.Rows<IDisposable>(made.Attributes)));
}
