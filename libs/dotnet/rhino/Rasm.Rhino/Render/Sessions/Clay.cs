using System.Runtime.InteropServices;
using Rasm.RhinoCommon.Objects;
using Rasm.RhinoCommon.Render.Content;
using Rasm.RhinoCommon.UI;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Render;
using Rhino.Render.CustomRenderMeshes;
using Rhino.Render.ParameterNames;

namespace Rasm.RhinoCommon.Render.Sessions;

// --- [SERVICES] ------------------------------------------------------------------------
[Guid("1E61FAC0-CB0C-4245-A299-03464E5F6EDC")]
public sealed class ClayMeshProvider : DefinedMeshProvider {
    private readonly uint serial;
    private readonly RenderMaterial clay;

    private ClayMeshProvider(IPlugInViews owner, uint serial, RenderMaterial clay) : base(owner, "Clay") => (this.serial, this.clay) = (serial, clay);

    public static IO<Unit> Register(IPlugInViews owner, RhinoDoc document, float albedo) =>
        from content in unitIO.Bind(_ => Products.Made(RenderContentType.NewContentFromTypeId(ContentUuids.PhysicallyBasedMaterialType, document)))
        from material in IO.lift(() => Conversions.Cast<RenderMaterial>(content))
        from gamma in Lights.PreProcessGamma(document)
        from written in Contents.WithinContentChange(material, RenderContent.ChangeContexts.Program, IO.lift(() => Callbacks.Each(
            Seq<(string Name, object Value)>(
                (PhysicallyBased.BaseColor, Color4f.ApplyGamma(new Color4f(albedo, albedo, albedo, 1f), 1f / gamma)),
                (PhysicallyBased.Roughness, 0.5)),
            (parameter, index) => RhinoFault.Unless(material.SetParameter(parameter.Name, parameter.Value), index))))
        from provider in DefinedMeshProvider.Register(views => new ClayMeshProvider(views, document.RuntimeSerialNumber, material))(owner)
        select unit;

    protected override IO<MeshProvision> Provides(MeshRequest request, RhinoDoc document, Guid objectId) =>
        IO.lift(() => new MeshProvision(request.Display.IsNone && document.RuntimeSerialNumber == serial, request.Flags));

    protected override IO<MeshBuild> Build(MeshRequest request, RhinoDoc document, Guid objectId, Seq<InstanceObject> ancestry, Seq<Instance> previous) =>
        IO.lift(() => previous.Iter(instance => (instance.Material, instance.IsForcedMaterial) = (clay, true)))
            .Map(_ => new MeshBuild(previous, ContentKey.Of<MeshBuild>(stream => stream.Integer(clay.RenderHash)), request.Flags));
}
