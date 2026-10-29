using System.Runtime.InteropServices;
using Rasm.Rhino.Objects;
using Rasm.Rhino.Render.Content;
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

    private ClayMeshProvider(uint serial, RenderMaterial clay) : base("Clay") => (this.serial, this.clay) = (serial, clay);

    public static IO<Unit> Register(RhinoDoc document, float albedo) =>
        from material in Products.Of(() => RenderContentType.NewContentFromTypeId(ContentUuids.PhysicallyBasedMaterialType, document) as RenderMaterial).Scoped(Session.Scope)
        from gamma in Lights.PreProcessGamma(document)
        from edited in Contents.Edit(material, RenderContent.ChangeContexts.Program, [
            Steps.Of<RenderContent>(content => content.SetParameter(PhysicallyBased.BaseColor, Color4f.ApplyGamma(new Color4f(albedo, albedo, albedo, 1f), 1f / gamma))),
            Steps.Of<RenderContent>(static content => content.SetParameter(PhysicallyBased.Roughness, 0.5))])
        from provider in Register(() => new ClayMeshProvider(document.RuntimeSerialNumber, material))
        select unit;

    protected override IO<(bool Provides, Flags Flags)> Provides(MeshRequest request, RhinoDoc document, Guid objectId) =>
        IO.lift(() => (Provides: request.Display.IsNone && document.RuntimeSerialNumber == serial, request.Flags));

    protected override IO<MeshBuild> Build(MeshRequest request, RhinoDoc document, Guid objectId, Seq<InstanceObject> ancestry, Seq<Instance> previous) =>
        IO.lift(() => previous.Iter(instance => (instance.Material, instance.IsForcedMaterial) = (clay, true)))
            .Map(_ => new MeshBuild(previous, ContentKey.Of<ClayMeshProvider>(stream => stream.Integer(clay.RenderHash)), request.Flags));
}
