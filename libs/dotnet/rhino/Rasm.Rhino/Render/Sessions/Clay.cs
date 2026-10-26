using System.Runtime.InteropServices;
using Rasm.Rhino.Objects;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Render;
using Rhino.Render.CustomRenderMeshes;
using Rhino.Render.ParameterNames;

namespace Rasm.Rhino.Render.Sessions;

// --- [SERVICES] ------------------------------------------------------------------------
[Guid("1E61FAC0-CB0C-4245-A299-03464E5F6EDC")]
public sealed class ClayMeshProvider : DefinedMeshProvider {
    private readonly uint serial;
    private readonly RenderMaterial clay;

    private ClayMeshProvider(IPlugInViews sink, uint serial, RenderMaterial clay) : base(sink, "Clay") => (this.serial, this.clay) = (serial, clay);

    public static IO<Unit> Register(IPlugInViews sink, RhinoDoc document, float albedo, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from content in DisposalOps.Acquire(IO.lift(() => Refused.UnlessFound(
                RenderContentType.NewContentFromTypeId(ContentUuids.PhysicallyBasedMaterialType, document), nameof(RenderContentType.NewContentFromTypeId))), owned)
            from material in IO.lift(() => Conversions.Cast<RenderMaterial>(content))
            from gamma in Lights.PreProcessGamma(document)
            from written in DisposalOps.Within(
                () => { material.BeginChange(RenderContent.ChangeContexts.Program); return material; },
                held => IO.lift(() => Callbacks.Each(
                    Seq<(string Name, object Value)>(
                        (PhysicallyBased.BaseColor, Color4f.ApplyGamma(new Color4f(albedo, albedo, albedo, 1f), 1f / gamma)),
                        (PhysicallyBased.Roughness, 0.5)),
                    parameter => held.SetParameter(parameter.Name, parameter.Value),
                    nameof(RenderContent.SetParameter))),
                static held => IO.lift(held.EndChange))
            from provider in DefinedMeshProvider.Register(owner => new ClayMeshProvider(owner, document.RuntimeSerialNumber, material))(sink, owned)
            select unit, retain);

    protected override IO<(bool Provides, Flags Flags)> Provides(MeshRequest request, RhinoDoc document, Guid objectId) =>
        IO.lift(() => (request.Display.IsNone && document.RuntimeSerialNumber == serial, request.Flags));

    protected override IO<MeshBuild> Build(MeshRequest request, RhinoDoc document, Guid objectId, Seq<InstanceObject> ancestry, Seq<Instance> previous, Action<IDisposable> retain) =>
        IO.lift(() => previous.Iter(instance => (instance.Material, instance.IsForcedMaterial) = (clay, true)))
            .Map(_ => new MeshBuild(previous, ContentKey.Of(KeyDomain.RenderMeshes, stream => stream.Integer(clay.RenderHash)), Flags: request.Flags));
}
