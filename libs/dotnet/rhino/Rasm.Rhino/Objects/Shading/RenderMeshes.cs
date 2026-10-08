using System.Globalization;
using Rasm.Rhino.Document.Shapes;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render.CustomRenderMeshes;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Objects.Shading;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record MeshRequest(MeshType Type, ViewportInfo Viewport, RenderMeshProvider.Flags Flags, Option<PlugIn> Requester, Option<DisplayPipelineAttributes> Display);

public sealed record MeshBuild(Seq<Instance> Instances, ContentKey Inputs, bool Incomplete);

public sealed record MeshProgress(string Text, double Amount, double Target, bool IsComplete);

[SmartEnum<int>]
[ValidationError<InvalidRhinoValue>]
public sealed partial class MeshInterface {
    public static readonly MeshInterface NoInterface = new(-1);
    public static readonly MeshInterface SimpleDialog = new(0);
    public static readonly MeshInterface DetailedDialog = new(1);
    public static readonly MeshInterface Scripted = new(2);
}

[Union]
public abstract partial record MeshRun {
    public sealed record Unattended(bool WorkerThread) : MeshRun;

    public sealed record Dialog(bool Simple) : MeshRun;

    public sealed record Styled(MeshInterface Interface, Transform Xform) : MeshRun;
}

public sealed record MeshBatch(Seq<(Mesh Mesh, ObjectAttributes Attributes)> Rows, MeshInterface Interface);

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedMeshProvider(PlugIn plugIn, IPlugInSink sink, string name) : RenderMeshProvider {
    // --- [IDENTITY]
    public sealed override string Name => RowText.Localize(name, table: Some<object>(sink)).Local;

    public sealed override Guid ProviderId =>
        new(BitConverter.GetBytes((UInt128)ContentKey.Of(KeyDomain.RenderMeshes, stream => stream.Id(plugIn.Id).Id(GetType().GUID))));

    // --- [CALLBACKS]
    public sealed override List<Guid> NonObjectIds =>
        [.. Callbacks.Answer(ProvidedIds, static () => Seq<Guid>(), Site(nameof(NonObjectIds)))];

    public sealed override bool HasCustomRenderMeshes(MeshType mt, ViewportInfo vp, RhinoDoc doc, Guid objectId, ref Flags flags, PlugIn plugin, DisplayPipelineAttributes attrs) =>
        Callbacks.Answer(
            new MeshRequest(mt, vp, flags, Optional(plugin), Optional(attrs)),
            request => Resolved(doc, document => Provides(request, document, objectId)),
            static () => false,
            Site(nameof(HasCustomRenderMeshes)));

    public sealed override RenderMeshes? RenderMeshes(MeshType mt, ViewportInfo vp, RhinoDoc doc, Guid objectId, List<InstanceObject> ancestry, ref Flags flags, RenderMeshes previousPrimitives, PlugIn plugin, DisplayPipelineAttributes attrs) {
        MeshRequest request = new(mt, vp, flags, Optional(plugin), Optional(attrs));
        (Option<RenderMeshes> Meshes, Flags Flags) answer = Callbacks.Answer(
            request,
            held => Resolved(doc, document => Built(held, document, objectId, Conversions.Rows(ancestry), previousPrimitives)),
            () => (Option<RenderMeshes>.None, request.Flags),
            Site(nameof(RenderMeshes)));
        flags = answer.Flags;
        return answer.Meshes.ValueUnsafe();
    }

    public sealed override RenderMeshProviderProgress? Progress(RhinoDoc doc, Guid[] optional_objectIds) =>
        Callbacks.Answer(
            Conversions.Rows(optional_objectIds),
            ids => Progressing(doc, ids).Map(found => found.Map(Reported)),
            static () => Option<RenderMeshProviderProgress>.None,
            Site(nameof(Progress))).ValueUnsafe();

    // --- [HOOKS]
    protected virtual IO<Seq<Guid>> ProvidedIds => IO.pure(Seq<Guid>());

    protected abstract IO<bool> Provides(MeshRequest request, RhinoDoc document, Guid objectId);

    protected abstract IO<MeshBuild> Build(MeshRequest request, RhinoDoc document, Guid objectId, Seq<InstanceObject> ancestry, Seq<Instance> previous);

    protected virtual IO<Option<MeshProgress>> Progressing(RhinoDoc document, Seq<Guid> objectIds) => IO.pure(Option<MeshProgress>.None);

    // --- [REGISTRATION]
    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register(Func<PlugIn, IPlugInSink, DefinedMeshProvider> create) =>
        (plugIn, sink) =>
            from provider in IO.lift(() => create(plugIn, sink))
            from _ in DisposalOps.OnFailure(
                IO.lift(() => MissingGuid.Unless(provider.GetType()).Bind(_ => Refused.Unless(RegisterProvider(provider, plugIn), nameof(RegisterProvider)))),
                IO.lift(provider.Dispose))
            select (IDisposable)provider;

    // --- [STEPS]
    private static IO<A> Resolved<A>(RhinoDoc? doc, Func<RhinoDoc, IO<A>> body) =>
        IO.lift(() => Missing.Unless(doc, nameof(RhinoDoc.FromRuntimeSerialNumber))).Bind(body);

    private IO<(Option<RenderMeshes> Meshes, Flags Flags)> Built(MeshRequest request, RhinoDoc document, Guid objectId, Seq<InstanceObject> ancestry, RenderMeshes previous) =>
        from built in Build(request, document, objectId, ancestry, Conversions.Rows(previous))
        from meshes in IO.lift(() => new RenderMeshes(document, objectId, ProviderId, Hash(previous.Hash, built.Inputs), (uint)request.Flags))
        from _ in DisposalOps.OnFailure(IO.lift(() => built.Instances.Iter(meshes.AddInstance)), IO.lift(meshes.Dispose))
        select (Some(meshes), built.Incomplete ? request.Flags | Flags.Incomplete : request.Flags);

    private uint Hash(uint previous, ContentKey inputs) =>
        (uint)((UInt128)ContentKey.Of(KeyDomain.RenderMeshes, stream => stream.Integer(previous).Id(ProviderId).Integer<UInt128>(inputs)) & uint.MaxValue);

    private RenderMeshProviderProgress Reported(MeshProgress progress) =>
        ProgressMapper.ToProgress(progress, sink, ProviderId);

    private CallbackSite Site(string member) => new(sink, GetType(), member);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class ProgressMapper {
    [MapProperty(nameof(MeshProgress.Text), nameof(RenderMeshProviderProgress.Text), Use = nameof(Localized))]
    internal static partial RenderMeshProviderProgress ToProgress(MeshProgress progress, IPlugInSink sink, Guid providerId);

    [UserMapping(Default = false)]
    private static string Localized(string text, IPlugInSink sink) => RowText.Localize(text, table: Some<object>(sink)).Local;
}

public static class Meshing {
    // --- [CACHES]
    public static IO<Seq<Mesh>> GetMeshes(RhinoObject rhinoObject, MeshType type) =>
        IO.lift(() => Conversions.Rows(rhinoObject.GetMeshes(type))).Bind(static cached => DisposalOps.AcquireAll(cached.Map(Copies.Duplicate), DisposalOps.Release));

    // --- [CUSTOM_MESHES]
    public static IO<RenderMeshes> RenderMeshes(RhinoObject rhinoObject, MeshRequest request, Seq<InstanceObject> ancestry) =>
        Monad.recur(unit, _ =>
            from read in IO.lift(() => Read(rhinoObject, request, ancestry))
            from next in read.Incomplete
                ? IO.lift(read.Meshes.Dispose).Bind(static _ => IO.lift(RhinoApp.Wait)).Map(static _ => Next.Loop<Unit, RenderMeshes>(unit))
                : IO.pure(Next.Done<Unit, RenderMeshes>(read.Meshes))
            select next).As();

    private static Fin<(RenderMeshes Meshes, bool Incomplete)> Read(RhinoObject rhinoObject, MeshRequest request, Seq<InstanceObject> ancestry) {
        RenderMeshProvider.Flags flags = request.Flags;
        return Missing.Unless(
                rhinoObject.RenderMeshes(request.Type, request.Viewport, [.. ancestry], ref flags, request.Requester.ValueUnsafe(), request.Display.ValueUnsafe()),
                nameof(RhinoObject.RenderMeshes))
            .Map(meshes => (Meshes: meshes, Incomplete: flags.HasFlag(RenderMeshProvider.Flags.Incomplete)));
    }

    // --- [PROVIDER_PARAMETERS]
    public static IO<Option<T>> GetCustomRenderMeshParameter<T>(RhinoObject rhinoObject, Guid providerId, string parameterName) where T : notnull =>
        IO.lift(() => Optional(rhinoObject.GetCustomRenderMeshParameter(providerId, parameterName)))
            .Bind(static held => held.Traverse(static value =>
                IO.lift(() => (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture))
                    .Finally(IO.lift(() => Optional(value as IDisposable).Iter(static owned => owned.Dispose())))).As());

    // --- [BATCH]
    public static IO<MeshBatch> MeshObjects(Seq<RhinoObject> rhinoObjects, MeshingParameters parameters, MeshRun run) =>
        Copies.Owned(
            IO.lift(() => run.Switch(
                (Objects: rhinoObjects, Parameters: parameters),
                unattended: static (state, unattended) => (
                    Result: RhinoObject.MeshObjects(state.Objects, state.Parameters, out Mesh[] meshes, out ObjectAttributes[] attributes, unattended.WorkerThread),
                    Meshes: meshes,
                    Attributes: attributes,
                    Interface: Fin.Succ(MeshInterface.NoInterface)),
                dialog: static (state, dialog) => {
                    bool simple = dialog.Simple;
                    Result result = RhinoObject.MeshObjects(state.Objects, ref state.Parameters, ref simple, out Mesh[] meshes, out ObjectAttributes[] attributes);
                    return (Result: result, Meshes: meshes, Attributes: attributes, Interface: Fin.Succ(simple ? MeshInterface.SimpleDialog : MeshInterface.DetailedDialog));
                },
                styled: static (state, styled) => {
                    int style = styled.Interface.Key;
                    Result result = RhinoObject.MeshObjects(state.Objects, ref state.Parameters, ref style, styled.Xform, out Mesh[] meshes, out ObjectAttributes[] attributes);
                    return (Result: result, Meshes: meshes, Attributes: attributes, Interface: Conversions.Validated<MeshInterface, int, InvalidRhinoValue>(style));
                })),
            static made =>
                from accepted in Conversions.FromResult(made.Result, nameof(RhinoObject.MeshObjects))
                from closed in made.Interface
                from meshes in Measurements.Valid(toSeq<Mesh?>(made.Meshes), nameof(RhinoObject.MeshObjects))
                select new MeshBatch(meshes.Zip(Conversions.Rows(made.Attributes), static (mesh, attributes) => (Mesh: mesh, Attributes: attributes)), closed),
            static made => DisposalOps.Release(Conversions.Rows<IDisposable>(made.Meshes) + Conversions.Rows<IDisposable>(made.Attributes)));
}
