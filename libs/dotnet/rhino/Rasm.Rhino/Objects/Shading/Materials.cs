using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Objects.Shading;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record MaterialScope {
    public sealed record Face(bool Front) : MaterialScope;

    public sealed record Component(ComponentIndex ComponentIndex) : MaterialScope;

    public sealed record ComponentForPlugIn(ComponentIndex ComponentIndex, Guid PlugInId, Option<ObjectAttributes> Attributes) : MaterialScope;
}

public sealed record MaterialIdentity(Guid Id, Option<string> Name, int Index);

public sealed record RenderMaterialIdentity(Guid Id, Option<string> Name);

public sealed record ResolvedMaterial(Option<MaterialIdentity> Material, Option<RenderMaterialIdentity> RenderMaterial);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class MaterialMapper {
    internal static partial MaterialIdentity ToState(Material material);

    internal static partial RenderMaterialIdentity ToState(RenderMaterial material);
}

public static class Materials {
    // --- [READS]
    public static IO<ResolvedMaterial> Resolve(RhinoObject rhinoObject, MaterialScope scope) =>
        IO.lift(() => scope.Switch(
                rhinoObject,
                face: static (found, face) => (Material: found.GetMaterial(face.Front), RenderMaterial: found.GetRenderMaterial(face.Front)),
                component: static (found, component) => (Material: found.GetMaterial(component.ComponentIndex), RenderMaterial: found.GetRenderMaterial(component.ComponentIndex)),
                componentForPlugIn: static (found, component) => (
                    Material: found.GetMaterial(component.ComponentIndex, component.PlugInId, component.Attributes.ValueUnsafe()),
                    RenderMaterial: found.GetRenderMaterial(component.ComponentIndex, component.PlugInId, component.Attributes.ValueUnsafe()))))
            .Map(static read => new ResolvedMaterial(
                Optional(read.Material).Filter(static row => !row.IsDefaultMaterial).Map(MaterialMapper.ToState),
                Optional(read.RenderMaterial).Map(MaterialMapper.ToState)));

    public static IO<Seq<(ComponentIndex Component, ResolvedMaterial Material)>> Components(RhinoObject rhinoObject) =>
        IO.lift(() => toSeq(rhinoObject.SubobjectMaterialComponents))
            .Bind(components => components.TraverseM(component => Resolve(rhinoObject, component).Map(material => (Component: component, Material: material))).As());

    // --- [WRITES]
    public static IO<Committed<Unit>> Assign(
        RhinoDoc doc,
        ObjectTarget target,
        RenderMaterial material,
        RenderMaterial.AssignToSubFaceChoices subFaces,
        RenderMaterial.AssignToBlockChoices blocks,
        string name,
        RedrawPolicy redraw,
        IPlugInSink sink) =>
        from objects in target.Objects(doc)
        from committed in DisposalOps.AcquireAll(objects.Map(static found => IO.lift(() => new ObjRef(found))), DisposalOps.Release).Bracket(
            Use: refs => Commits.Commit(doc, RowText.Localize(name, table: Some<object>(sink)), redraw,
                IO.lift(() => Refused.Unless(material.AssignTo(refs, subFaces, blocks, bInteractive: false), nameof(RenderMaterial.AssignTo)))),
            Fin: DisposalOps.Release)
        select committed;
}
