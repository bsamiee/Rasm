using Rasm.Rhino.Document;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.Render.CustomRenderMeshes;
using Riok.Mapperly.Abstractions;

[assembly: UseStaticMapper(typeof(Answers))]

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialScope {
    public sealed record Face(bool Front) : MaterialScope;

    public sealed record Component(ComponentIndex ComponentIndex) : MaterialScope;

    public sealed record ComponentForPlugIn(ComponentIndex ComponentIndex, Guid PlugInId) : MaterialScope;

    public sealed record ComponentWithAttributes(ComponentIndex ComponentIndex, Guid PlugInId, ObjectAttributes Attributes) : MaterialScope;
}

public sealed record MaterialIdentity(Guid Id, Option<string> Name, int Index);

public sealed record RenderMaterialIdentity(Guid Id, Option<string> Name);

public sealed record MappingEntry(int Channel, Guid MappingId, Transform Xform);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeshBatchMethod {
    public sealed record Worker(bool UseWorkerThread) : MeshBatchMethod;

    public sealed record Dialog(bool Simple) : MeshBatchMethod;

    public sealed record UiStyle(int Style, Transform Xform) : MeshBatchMethod;
}

public sealed record MeshBatchResult(Seq<(Mesh Mesh, ObjectAttributes Attributes)> Rows, Option<bool> SimpleDialog, Option<int> UiStyle);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class MaterialMapper {
    internal static partial MaterialIdentity ToIdentity(Material material);

    internal static partial RenderMaterialIdentity ToIdentity(RenderMaterial material);
}

public static class Materials {
    // --- [RESOLUTION]
    public static IO<Option<MaterialIdentity>> ResolveMaterial(RhinoObject o, MaterialScope scope) =>
        IO.lift(() => scope.Switch(
                o,
                face: static (target, face) => Missing.Unless(target.GetMaterial(face.Front), nameof(RhinoObject.GetMaterial)),
                component: static (target, component) => target.GetMaterial(component.ComponentIndex),
                componentForPlugIn: static (target, component) => target.GetMaterial(component.ComponentIndex, component.PlugInId),
                componentWithAttributes: static (target, component) => target.GetMaterial(component.ComponentIndex, component.PlugInId, component.Attributes))
            .Map(static material => Some(material)
                .Filter(static resolved => !resolved.IsDefaultMaterial)
                .Map(static resolved => MaterialMapper.ToIdentity(resolved))));

    public static IO<Option<RenderMaterialIdentity>> ResolveRenderMaterial(RhinoObject o, MaterialScope scope) =>
        IO.lift(() => Optional(scope.Switch(
                o,
                face: static (target, face) => target.GetRenderMaterial(face.Front),
                component: static (target, component) => target.GetRenderMaterial(component.ComponentIndex),
                componentForPlugIn: static (target, component) => target.GetRenderMaterial(component.ComponentIndex, component.PlugInId),
                componentWithAttributes: static (target, component) => target.GetRenderMaterial(component.ComponentIndex, component.PlugInId, component.Attributes)))
            .Map(static content => MaterialMapper.ToIdentity(content)));

    // --- [MAPPINGS]
    public static IO<TValue> WithMapping<TValue>(RhinoObject o, int channel, Func<TextureMapping, Transform, IO<TValue>> body) =>
        IO.lift(() => Missing.Unless(o.GetTextureMapping(channel, out Transform xform), nameof(RhinoObject.GetTextureMapping))
                .Map(mapping => (Mapping: mapping, Xform: xform)))
            .Bracket(Use: held => body(held.Mapping, held.Xform), Fin: static held => IO.lift(held.Mapping.Dispose));

    public static IO<Seq<MappingEntry>> Mappings(RhinoObject o) =>
        IO.lift(() => toSeq(o.GetTextureChannels()))
            .Bind(channels => channels.TraverseM(channel => WithMapping(o, channel, (mapping, xform) => IO.pure(new MappingEntry(channel, mapping.Id, xform)))).As());

    public static IO<int> SetMapping(RhinoObject o, int channel, Option<TextureMapping> mapping, Option<Transform> xform) =>
        from onChannel in IO.lift(() => Invalid.Unless(
            mapping.ForAll(held => held.MappingType == TextureMappingType.OcsMapping == (channel == ObjectAttributes.OCSMappingChannelId)),
            nameof(ObjectAttributes.OCSMappingChannelId)))
        from answer in IO.lift(() => xform.Match(
            Some: value => o.SetTextureMapping(channel, mapping.ValueUnsafe(), value),
            None: () => o.SetTextureMapping(channel, mapping.ValueUnsafe())))
        select answer;

    // --- [MESHES]
    public static IO<TValue> WithCachedMeshes<TValue>(RhinoObject o, MeshType type, Func<Seq<Mesh>, IO<TValue>> body) =>
        from cached in IO.lift(() => Answers.Present(o.GetMeshes(type)))
        from value in DisposalOps.Using(DisposalOps.AcquireAll(cached.Map(GeometryOps.Duplicated)), body)
        select value;

    public static IO<TValue> WithRenderMeshes<TValue>(
        RhinoObject o,
        MeshType type,
        Option<ViewportInfo> viewport,
        Seq<InstanceObject> ancestry,
        RenderMeshProvider.Flags flags,
        Option<PlugIn> plugin,
        Option<DisplayPipelineAttributes> attributes,
        Func<RenderMeshes, RenderMeshProvider.Flags, IO<TValue>> body) =>
        IO.lift(() => {
            RenderMeshProvider.Flags answered = flags;
            return Missing.Unless(o.RenderMeshes(type, viewport.ValueUnsafe(), [.. ancestry], ref answered, plugin.ValueUnsafe(), attributes.ValueUnsafe()), nameof(RhinoObject.RenderMeshes))
                .Map(meshes => (Meshes: meshes, Flags: answered));
        }).Bracket(Use: produced => body(produced.Meshes, produced.Flags), Fin: static produced => IO.lift(produced.Meshes.Dispose));

    public static IO<TValue> WithRenderMeshParameters<TValue>(RhinoObject o, bool documentFallback, Func<Option<MeshingParameters>, IO<TValue>> body) =>
        IO.lift(() => Optional(o.GetRenderMeshParameters(documentFallback))).Bracket(Use: body, Fin: static held => DisposalOps.Release(held.ToSeq()));

    public static IO<Unit> SetRenderMeshParameters(RhinoObject o, Option<MeshingParameters> parameters) =>
        IO.lift(() => Refused.Unless(o.SetRenderMeshParameters(parameters.ValueUnsafe()), nameof(RhinoObject.SetRenderMeshParameters)));

    public static IO<Option<IConvertible>> CustomRenderMeshParameter(RhinoObject o, Guid provider, string name) =>
        IO.lift(() => Optional(o.GetCustomRenderMeshParameter(provider, name)));

    public static IO<TValue> WithMeshBatch<TValue>(Seq<RhinoObject> objects, MeshingParameters parameters, MeshBatchMethod method, Func<MeshBatchResult, IO<TValue>> body) =>
        from populated in IO.lift(() => Invalid.Unless(!objects.IsEmpty, nameof(objects)))
        from run in IO.lift(() => method.Switch(
            (Objects: objects, Parameters: parameters),
            worker: static (state, worker) => Fin.Succ(Packed(
                RhinoObject.MeshObjects(state.Objects, state.Parameters, out Mesh[] meshes, out ObjectAttributes[] attributes, worker.UseWorkerThread),
                meshes,
                attributes,
                Option<bool>.None,
                Option<int>.None)),
            dialog: static (state, dialog) => {
                bool simple = dialog.Simple;
                Result result = RhinoObject.MeshObjects(state.Objects, ref state.Parameters, ref simple, out Mesh[] meshes, out ObjectAttributes[] attributes);
                return Fin.Succ(Packed(result, meshes, attributes, Some(simple), Option<int>.None));
            },
            uiStyle: static (state, uiStyle) => Limits.AtLeast(-1).AtMost(2).Check(uiStyle.Style, nameof(MeshBatchMethod.UiStyle.Style)).Map(style => {
                Result result = RhinoObject.MeshObjects(state.Objects, ref state.Parameters, ref style, uiStyle.Xform, out Mesh[] meshes, out ObjectAttributes[] attributes);
                return Packed(result, meshes, attributes, Option<bool>.None, Some(style));
            })))
        from value in DisposalOps.Using(IO.pure((run.Meshes, run.Attributes)), nameof(RhinoObject.MeshObjects), rows =>
            from accepted in IO.lift(() => Answers.FromResult(run.Result, nameof(RhinoObject.MeshObjects)))
            from value in body(new MeshBatchResult(rows, run.SimpleDialog, run.UiStyle))
            select value)
        select value;

    private static (Result Result, Seq<Mesh> Meshes, Seq<ObjectAttributes> Attributes, Option<bool> SimpleDialog, Option<int> UiStyle) Packed(
        Result result,
        Mesh[] meshes,
        ObjectAttributes[] attributes,
        Option<bool> simple,
        Option<int> uiStyle) =>
        (result, toSeq(meshes), toSeq(attributes), simple, uiStyle);
}
