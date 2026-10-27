using Rasm.RhinoCommon.Document;
using Rasm.RhinoCommon.Document.Tables;
using Rhino;
using Rhino.DocObjects;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.RhinoCommon.Render.Content;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record MaterialScope {
    public sealed record Face(bool Front) : MaterialScope;

    public sealed record Component(ComponentIndex ComponentIndex) : MaterialScope;

    public sealed record ComponentForPlugIn(ComponentIndex ComponentIndex, Guid PlugInId, Option<ObjectAttributes> Attributes) : MaterialScope;
}

public sealed record IndexedMaterial(NamedId Identity, int Index);

public sealed record ResolvedMaterial(Option<IndexedMaterial> Material, Option<NamedId> RenderMaterial);

public sealed record ComponentMaterial(ComponentIndex Component, ResolvedMaterial Material);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class Materials {
    // --- [READS]
    public static IO<ResolvedMaterial> Resolve(RhinoObject rhinoObject, MaterialScope scope) =>
        IO.lift(() => scope.Switch(
            rhinoObject,
            face: static (found, face) => Resolved(found.GetMaterial(face.Front), found.GetRenderMaterial(face.Front)),
            component: static (found, component) => Resolved(found.GetMaterial(component.ComponentIndex), found.GetRenderMaterial(component.ComponentIndex)),
            componentForPlugIn: static (found, component) => component.Attributes.ValueUnsafe() switch {
                var attributes => Resolved(
                    found.GetMaterial(component.ComponentIndex, component.PlugInId, attributes),
                    found.GetRenderMaterial(component.ComponentIndex, component.PlugInId, attributes)),
            }));

    public static IO<Seq<ComponentMaterial>> Components(RhinoObject rhinoObject) =>
        from components in IO.lift(() => Conversions.Rows(rhinoObject.SubobjectMaterialComponents))
        from resolved in components.TraverseM(component => Resolve(rhinoObject, component).Map(material => new ComponentMaterial(component, material))).As()
        select resolved;

    private static ResolvedMaterial Resolved(Material? material, RenderMaterial? render) =>
        new(
            Optional(material).Filter(static row => !row.IsDefaultMaterial).Map(static row => new IndexedMaterial(ToState(row), row.Index)),
            Optional(render).Map(ToState));

    private static partial NamedId ToState(Material material);

    private static partial NamedId ToState(RenderMaterial material);

    // --- [WRITES]
    public static IO<Committed<Unit>> Assign(
        RhinoDoc doc, ObjectTarget target, RenderMaterial material,
        RenderMaterial.AssignToSubFaceChoices subFaces, RenderMaterial.AssignToBlockChoices blocks, bool interactive,
        string name, RedrawPolicy redraw, object owner) =>
        from objects in target.Objects(doc)
        from committed in (from refs in objects.TraverseM(static found => use(() => new ObjRef(found))).As()
                           from assigned in Commits.Commit(doc, RowText.Localize(name, owner).Local, redraw,
                               IO.lift(() => RhinoFault.Unless(material.AssignTo(refs, subFaces, blocks, interactive))))
                           select assigned).Bracket()
        select committed;
}
