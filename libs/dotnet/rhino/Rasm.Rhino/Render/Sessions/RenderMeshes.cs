using System.Globalization;
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

public sealed record MeshBuild(Seq<Instance> Instances, ContentKey Inputs, RenderMeshProvider.Flags Flags);

[SmartEnum<int>(SkipIParsable = true, SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
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
public abstract partial class DefinedMeshProvider(IPlugInViews sink, string name) : RenderMeshProvider {
    // --- [IDENTITY]
    public sealed override string Name => RowText.Localize(name, sink).Local;

    public sealed override Guid ProviderId =>
        ContentKey.Of(KeyDomain.RenderMeshes, stream => stream.Id(sink.PlugIn.Id).Id(GetType().GUID)).Id;

    // --- [CALLBACKS]
    public sealed override List<Guid> NonObjectIds =>
        [.. Callbacks.Answer(this, static provider => provider.ProvidedIds, static () => Seq<Guid>(), new(sink, GetType(), nameof(NonObjectIds)))];

    public sealed override bool HasCustomRenderMeshes(MeshType mt, ViewportInfo vp, RhinoDoc doc, Guid objectId, ref Flags flags, PlugIn plugin, DisplayPipelineAttributes attrs) {
        MeshRequest request = new(mt, vp, flags, Optional(plugin), Optional(attrs));
        (bool Provides, Flags Flags) answer = Callbacks.Answer(
            (Request: request, Document: doc, ObjectId: objectId),
            args => Provides(args.Request, args.Document, args.ObjectId), () => (false, request.Flags), new(sink, GetType(), nameof(HasCustomRenderMeshes)));
        flags = answer.Flags;
        return answer.Provides;
    }

    public sealed override RenderMeshes? RenderMeshes(MeshType mt, ViewportInfo vp, RhinoDoc doc, Guid objectId, List<InstanceObject> ancestry, ref Flags flags, RenderMeshes previousPrimitives, PlugIn plugin, DisplayPipelineAttributes attrs) {
        MeshRequest request = new(mt, vp, flags, Optional(plugin), Optional(attrs));
        (Option<RenderMeshes> Meshes, Flags Flags) answer = Callbacks.Answer(
            DisposalOps.Within(
                (Action<IDisposable> temporary) =>
                    from previous in IO.lift(() => toSeq(previousPrimitives.Select(instance => { temporary(instance.Mesh); temporary(instance); return instance; })).Strict())
                    from built in Build(request, doc, objectId, Conversions.Rows(ancestry), previous, temporary)
                    let providerId = ProviderId
                    let hash = uint.CreateTruncating((UInt128)ContentKey.Of(KeyDomain.RenderMeshes, stream => stream.Integer(previousPrimitives.Hash).Id(providerId).Integer((UInt128)built.Inputs)))
                    from meshes in IO.lift(() => new RenderMeshes(doc, objectId, providerId, hash, (uint)built.Flags))
                    from added in DisposalOps.OnFailure(IO.lift(() => built.Instances.Iter(meshes.AddInstance)), IO.lift(meshes.Dispose))
                    select (Meshes: Some(meshes), built.Flags),
                DisposalOps.Release),
            () => (Option<RenderMeshes>.None, request.Flags),
            new(sink, GetType(), nameof(RenderMeshes)));
        flags = answer.Flags;
        return answer.Meshes.ValueUnsafe();
    }

    public sealed override RenderMeshProviderProgress? Progress(RhinoDoc doc, Guid[] optional_objectIds) =>
        Callbacks.Answer(
            from ids in IO.lift(() => Conversions.Rows(optional_objectIds))
            from found in Progressing(doc, ids)
            select found.Map(progress => ToProgress(progress, RowText.Localize(progress.Text, sink).Local, ProviderId)),
            static () => Option<RenderMeshProviderProgress>.None, new(sink, GetType(), nameof(Progress))).ValueUnsafe();

    private static partial RenderMeshProviderProgress ToProgress(RenderMeshProviderProgress progress, string text, Guid providerId);

    // --- [HOOKS]
    protected virtual IO<Seq<Guid>> ProvidedIds => IO.pure(Seq<Guid>());

    protected abstract IO<(bool Provides, Flags Flags)> Provides(MeshRequest request, RhinoDoc document, Guid objectId);

    protected abstract IO<MeshBuild> Build(MeshRequest request, RhinoDoc document, Guid objectId, Seq<InstanceObject> ancestry, Seq<Instance> previous, Action<IDisposable> retain);

    protected virtual IO<Option<RenderMeshProviderProgress>> Progressing(RhinoDoc document, Seq<Guid> objectIds) => IO.pure(Option<RenderMeshProviderProgress>.None);

    // --- [REGISTRATION]
    public static Func<IPlugInViews, Action<IDisposable>, IO<Unit>> Register(Func<IPlugInViews, DefinedMeshProvider> create) =>
        (sink, retain) => DisposalOps.Acquire(
            owned => from provider in DisposalOps.Acquire(IO.lift(() => create(sink)), owned)
                     from accepted in IO.lift(() => Refused.Unless(RegisterProvider(provider, sink.PlugIn), nameof(RegisterProvider)))
                     select accepted,
            retain);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Meshing {
    // --- [CACHES]
    public static IO<Seq<Mesh>> GetMeshes(RhinoObject rhinoObject, MeshType type, Action<IDisposable> retain) =>
        DisposalOps.Acquire(
            owned => IO.lift(() => Conversions.Rows(rhinoObject.GetMeshes(type)))
                .Bind(cached => cached.TraverseM(mesh => Copies.Duplicate(mesh, owned)).As()),
            retain);

    // --- [CUSTOM_MESHES]
    public static IO<RenderMeshes> RenderMeshes(RhinoObject rhinoObject, MeshRequest request, Seq<InstanceObject> ancestry, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            (from read in DisposalOps.Acquire(IO.lift(() => {
                RenderMeshProvider.Flags flags = request.Flags;
                RenderMeshes? meshes = rhinoObject.RenderMeshes(request.Type, request.Viewport, [.. ancestry], ref flags, request.Requester.ValueUnsafe(), request.Display.ValueUnsafe());
                return (Meshes: meshes, Flags: flags, Release: new Disposal<RenderMeshes?>(meshes, static held => held?.Dispose()));
            }), static read => read.Release, owned)
             from meshes in IO.lift(() => Refused.UnlessFound(read.Meshes, nameof(RhinoObject.RenderMeshes)))
             from canceled in when(read.Flags.HasFlag(RenderMeshProvider.Flags.Canceled), IO.fail<Unit>(Errors.Cancelled)).As()
             from completed in read.Flags.HasFlag(RenderMeshProvider.Flags.Incomplete)
                 ? IO.lift(read.Release.Dispose).Bind(static _ => IO.lift(static () => RhinoApp.Wait())).Map(_ => (Meshes: meshes, read.Flags))
                 : IO.pure((Meshes: meshes, read.Flags))
             select completed)
            .RepeatUntil(static read => !read.Flags.HasFlag(RenderMeshProvider.Flags.Incomplete))
            .Map(static read => read.Meshes),
            retain);

    // --- [PROVIDER_PARAMETERS]
    public static IO<Option<T>> GetCustomRenderMeshParameter<T>(RhinoObject rhinoObject, Guid providerId, string parameterName) where T : notnull =>
        DisposalOps.Within(() => rhinoObject.GetCustomRenderMeshParameter(providerId, parameterName),
            static held => Optional(held).Traverse(static value => IO.lift(() => (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture))).As(),
            static held => DisposalOps.Release(held as IDisposable));

    // --- [BATCH]
    public static IO<MeshBatch> MeshObjects(Seq<RhinoObject> rhinoObjects, MeshingParameters parameters, MeshRun run, Action<IDisposable> retain) =>
        Copies.Owned(
            () => run.Switch(
                (Objects: rhinoObjects, Parameters: parameters),
                unattended: static (state, unattended) => (
                    Result: RhinoObject.MeshObjects(state.Objects, state.Parameters, out Mesh[] meshes, out ObjectAttributes[] attributes, unattended.WorkerThread),
                    Meshes: meshes, Attributes: attributes, Interface: MeshInterface.NoInterface),
                styled: static (state, styled) => {
                    int style = styled.Interface.Key;
                    return (
                        Result: RhinoObject.MeshObjects(state.Objects, ref state.Parameters, ref style, styled.Xform, out Mesh[] meshes, out ObjectAttributes[] attributes),
                        Meshes: meshes, Attributes: attributes, Interface: MeshInterface.Validated(style));
                }),
            static made => from accepted in Conversions.FromResult(made.Result, nameof(RhinoObject.MeshObjects))
                           from closed in made.Interface
                           from meshes in Measurements.Valid(toSeq<Mesh?>(made.Meshes), nameof(RhinoObject.MeshObjects))
                           select new MeshBatch(meshes.Zip(Conversions.Rows(made.Attributes)), closed),
            static made => DisposalOps.Release(Conversions.Rows<IDisposable>(made.Meshes) + Conversions.Rows<IDisposable>(made.Attributes)), retain);
}
