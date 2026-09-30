using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LightState(
    Guid Id,
    int Index,
    Option<string> Name,
    LightStyle LightStyle,
    bool IsEnabled,
    Point3d Location,
    Vector3d Direction,
    double Intensity,
    Color Diffuse,
    double ShadowIntensity,
    double SpotAngleRadians,
    double HotSpot,
    double Radius,
    Vector3d Length,
    Vector3d Width,
    Vector3d AttenuationVector,
    bool Solo);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LightSource {
    public sealed record PointLight(Point3d Location) : LightSource;

    public sealed record SpotLight(Point3d Location, Vector3d Direction) : LightSource;

    public sealed record DirectionalLight(Point3d Location, Vector3d Direction) : LightSource;

    public sealed record LinearLight(Point3d Location, Vector3d Length) : LightSource;

    public sealed record RectangularLight(Point3d Location, Vector3d Length, Vector3d Width) : LightSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LightEdit {
    public sealed record Rename(Option<string> Name) : LightEdit;

    public sealed record Set(Action<Light> Write) : LightEdit;

    public sealed record Place(LightSource Source) : LightEdit;

    public sealed record Radius(double Value) : LightEdit;

    public sealed record Hardness(double HotSpot) : LightEdit;

    public sealed record SpotAngle(double Radians) : LightEdit;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LightOp {
    public sealed record Add(LightSource Source, Seq<LightEdit> Edits, Option<ObjectAttributes> Attributes) : LightOp;

    public sealed record Modify(ComponentRef Address, Seq<LightEdit> Edits) : LightOp;

    public sealed record Solo(ComponentRef Address, bool On) : LightOp;

    public sealed record Delete(ComponentRef Address, bool Quiet) : LightOp;

    public sealed record Undelete(ComponentRef Address) : LightOp;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
internal static partial class LightMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial LightState ToState(Light light, int index, bool solo);

    [MapValue(nameof(Light.LightStyle), LightStyle.WorldPoint)]
    internal static partial void Update([MappingTarget] Light light, LightSource.PointLight source);

    [MapValue(nameof(Light.LightStyle), LightStyle.WorldSpot)]
    internal static partial void Update([MappingTarget] Light light, LightSource.SpotLight source);

    [MapValue(nameof(Light.LightStyle), LightStyle.WorldDirectional)]
    internal static partial void Update([MappingTarget] Light light, LightSource.DirectionalLight source);

    [MapValue(nameof(Light.LightStyle), LightStyle.WorldLinear)]
    internal static partial void Update([MappingTarget] Light light, LightSource.LinearLight source);

    [MapValue(nameof(Light.LightStyle), LightStyle.WorldRectangular)]
    internal static partial void Update([MappingTarget] Light light, LightSource.RectangularLight source);
}

public static class Lights {
    // --- [READS]
    public static IO<LightState> ReadLight(RhinoDoc doc, LightObject o) =>
        from light in IO.lift(() => Missing.Unless(o.LightGeometry, nameof(LightObject.LightGeometry)))
        from solo in DisposalOps.Using(() => new LightManagerSupportClient(doc.RuntimeSerialNumber), client => IO.lift(() => client.GetLightSolo(light)))
        select LightMapper.ToState(light, o.Index, solo);

    public static IO<Seq<LightObject>> AllLights(RhinoDoc doc) =>
        IO.lift(() => toSeq(doc.Lights).Filter(static light => !light.IsDeleted).Strict());

    public static IO<LightObject> ResolveLight(RhinoDoc doc, ComponentRef address, bool includeDeleted) =>
        IO.lift(() => address.Switch(
                doc.Lights,
                byId: static (lights, byId) => Fin.Succ(Answers.Present(lights.Find(byId.Id, ignoreDeleted: false)).Map(index => lights[index])),
                byIndex: static (lights, byIndex) => Fin.Succ(Some(byIndex.Index).Filter(slot => slot < lights.Count).Map(slot => lights[slot])),
                byName: static (_, _) => Fin.Fail<Option<LightObject>>(new Invalid(nameof(ComponentRef.ByName))))
            .Bind(found => found.Filter(row => includeDeleted || !row.IsDeleted).ToFin(new MissingComponent(doc.Lights.ComponentType, address))));

    // --- [WRITES]
    public static IO<int> ApplyLightOp(RhinoDoc doc, LightOp op) =>
        op.Switch(
            doc,
            add: static (document, add) => DisposalOps.Using(
                static () => new Light(),
                light =>
                    from edited in (Seq<LightEdit>(new LightEdit.Place(add.Source)) + add.Edits).TraverseM(edit => Edited(light, edit)).As()
                    from index in IO.lift(() => Answers.Required(document.Lights.Add(light, add.Attributes.ValueUnsafe()), nameof(LightTable.Add)))
                    select index),
            modify: static (document, modify) =>
                from row in ResolveLight(document, modify.Address, includeDeleted: false)
                from modified in DisposalOps.Using(
                    IO.lift(() => Missing.Unless(row.DuplicateLightGeometry(), nameof(LightObject.DuplicateLightGeometry))),
                    copy =>
                        from edited in modify.Edits.TraverseM(edit => Edited(copy, edit)).As()
                        from accepted in IO.lift(() => Refused.Unless(document.Lights.Modify(row.Index, copy), nameof(LightTable.Modify)))
                        select accepted)
                select row.Index,
            solo: static (document, solo) =>
                from row in ResolveLight(document, solo.Address, includeDeleted: false)
                from light in IO.lift(() => Missing.Unless(row.LightGeometry, nameof(LightObject.LightGeometry)))
                from set in DisposalOps.Using(
                    () => new LightManagerSupportClient(document.RuntimeSerialNumber),
                    client => IO.lift(() => Refused.Unless(client.SetLightSolo(light, solo.On), nameof(LightManagerSupportClient.SetLightSolo))))
                select row.Index,
            delete: static (document, delete) =>
                from row in ResolveLight(document, delete.Address, includeDeleted: false)
                from deleted in IO.lift(() => Refused.Unless(document.Lights.Delete(row.Index, delete.Quiet), nameof(LightTable.Delete)))
                select row.Index,
            undelete: static (document, undelete) =>
                from row in ResolveLight(document, undelete.Address, includeDeleted: true)
                from restored in IO.lift(() => Refused.Unless(document.Lights.Undelete(row.Index), nameof(LightTable.Undelete)))
                select row.Index);

    private static IO<Unit> Edited(Light light, LightEdit edit) =>
        edit.Switch(
            light,
            rename: static (target, rename) => IO.lift(() => { target.Name = Answers.Unset(rename.Name); }),
            set: static (target, set) => IO.lift(() => set.Write(target)),
            place: static (target, place) => IO.lift(() => place.Source.Switch(
                target,
                pointLight: LightMapper.Update,
                spotLight: LightMapper.Update,
                directionalLight: LightMapper.Update,
                linearLight: LightMapper.Update,
                rectangularLight: LightMapper.Update)),
            radius: static (target, radius) =>
                from applies in IO.lift(() => WrongLightStyle.Unless(target.IsPointLight || target.IsSpotLight || target.IsDirectionalLight, target.LightStyle, nameof(LightEdit.Radius)))
                from written in IO.lift(() => { target.Radius = radius.Value; })
                select written,
            hardness: static (target, hardness) =>
                from applies in IO.lift(() => WrongLightStyle.Unless(target.IsSpotLight, target.LightStyle, nameof(LightEdit.Hardness)))
                from written in IO.lift(() => { target.HotSpot = hardness.HotSpot; })
                select written,
            spotAngle: static (target, angle) =>
                from applies in IO.lift(() => WrongLightStyle.Unless(target.IsSpotLight, target.LightStyle, nameof(LightEdit.SpotAngle)))
                from written in IO.lift(() => { target.SpotAngleRadians = angle.Radians; })
                select written);
}
