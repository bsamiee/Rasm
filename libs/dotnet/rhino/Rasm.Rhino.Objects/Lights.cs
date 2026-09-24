using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Objects;

// --- [TYPES] ---------------------------------------------------------------------------
public enum PowerUnit { Intensity = 0, Watts = 1, Lumens = 2, Candela = 3 }

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LightState(
    Guid Id,
    int Index,
    Option<string> Name,
    LightStyle LightStyle,
    bool IsEnabled,
    Point3d Location,
    Vector3d Direction,
    Vector3d PerpendicularDirection,
    CoordinateSystem CoordinateSystem,
    double Intensity,
    double PowerWatts,
    double PowerLumens,
    double PowerCandela,
    Color Diffuse,
    Color Ambient,
    Color Specular,
    double ShadowIntensity,
    double SpotAngleRadians,
    double SpotExponent,
    double HotSpot,
    double Radius,
    Vector3d Length,
    Vector3d Width,
    Vector3d AttenuationVector,
    Light.Attenuation AttenuationType);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LightSource {
    public sealed record PointLight(Point3d Location) : LightSource;

    public sealed record SpotLight(Point3d Location, Vector3d Direction, double SpotAngleRadians, double HotSpot) : LightSource;

    public sealed record DirectionalLight(Point3d Location, Vector3d Direction) : LightSource;

    public sealed record LinearLight(Point3d Location, Vector3d Length) : LightSource;

    public sealed record RectangularLight(Point3d Location, Vector3d Length, Vector3d Width) : LightSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LightEdit {
    public sealed record Rename(Option<string> Name) : LightEdit;

    public sealed record Enable(bool On) : LightEdit;

    public sealed record Power(PowerUnit Unit, double Magnitude) : LightEdit;

    public sealed record Colors(Color Diffuse, Option<Color> Ambient, Option<Color> Specular) : LightEdit;

    public sealed record ShadowIntensity(double Intensity) : LightEdit;

    public sealed record Place(LightSource Source) : LightEdit;

    public sealed record AttenuationType(Light.Attenuation Kind) : LightEdit;

    public sealed record AttenuationVector(Vector3d Coefficients) : LightEdit;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LightOp {
    public sealed record Add(LightSource Source, Option<string> Name, Option<ObjectAttributes> Attributes) : LightOp;

    public sealed record Modify(int Index, Seq<LightEdit> Edits) : LightOp;

    public sealed record Delete(int Index, bool Quiet) : LightOp;

    public sealed record Undelete(int Index) : LightOp;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Lights {
    // --- [READS]
    public static IO<LightState> ReadLight(LightObject o) =>
        IO.lift(() => Missing.Unless(o.LightGeometry, nameof(LightObject.LightGeometry)).Map(light => new LightState(
            light.Id,
            o.Index,
            Answers.Present(light.Name),
            light.LightStyle,
            light.IsEnabled,
            light.Location,
            light.Direction,
            light.PerpendicularDirection,
            light.CoordinateSystem,
            light.Intensity,
            light.PowerWatts,
            light.PowerLumens,
            light.PowerCandela,
            light.Diffuse,
            light.Ambient,
            light.Specular,
            light.ShadowIntensity,
            light.SpotAngleRadians,
            light.SpotExponent,
            light.HotSpot,
            light.Radius,
            light.Length,
            light.Width,
            light.AttenuationVector,
            light.AttenuationType)));

    public static IO<Seq<LightObject>> AllLights(RhinoDoc doc) =>
        IO.lift(() => toSeq(doc.Lights).Filter(static light => !light.IsDeleted).Strict());

    public static IO<Option<LightObject>> ResolveLight(RhinoDoc doc, ComponentRef address) =>
        TableOps.Find<LightObject>(
            address,
            id => Answers.Present(doc.Lights.Find(id, ignoreDeleted: true)).Bind(index => Optional(doc.Lights.FindIndex(index))),
            index => Optional(doc.Lights.FindIndex(index)),
            name => Optional(doc.Lights.FindName(name)));

    // --- [WRITES]
    public static IO<int> ApplyLightOp(RhinoDoc doc, LightOp op) =>
        op.Switch(
            doc,
            add: static (document, add) => Added(document, add),
            modify: static (document, modify) =>
                from index in IO.lift(() => Answers.NonNegative(modify.Index, nameof(LightTable.Modify)))
                from row in IO.lift(() => Missing.Unless(document.Lights.FindIndex(index), nameof(LightTable.FindIndex)))
                from geometry in IO.lift(() => Missing.Unless(row.LightGeometry, nameof(LightObject.LightGeometry)))
                from world in IO.lift(() => Accepts(
                    geometry.LightStyle,
                    nameof(Light.LightStyle),
                    LightStyle.WorldPoint,
                    LightStyle.WorldSpot,
                    LightStyle.WorldDirectional,
                    LightStyle.WorldLinear,
                    LightStyle.WorldRectangular))
                from modified in GeometryOps.WithGeometry(geometry, DuplicateMode.Duplicate, copy =>
                    from edited in modify.Edits.TraverseM(edit => Edited(copy, edit)).As()
                    from accepted in IO.lift(() => Refused.Unless(document.Lights.Modify(index, copy), nameof(LightTable.Modify)))
                    select accepted)
                select index,
            delete: static (document, delete) =>
                from index in IO.lift(() => Answers.NonNegative(delete.Index, nameof(LightTable.Delete)))
                from deleted in IO.lift(() => Refused.Unless(document.Lights.Delete(index, delete.Quiet), nameof(LightTable.Delete)))
                select index,
            undelete: static (document, undelete) =>
                from index in IO.lift(() => Answers.NonNegative(undelete.Index, nameof(LightTable.Undelete)))
                from restored in IO.lift(() => Refused.Unless(document.Lights.Undelete(index), nameof(LightTable.Undelete)))
                select index);

    private static IO<int> Added(RhinoDoc doc, LightOp.Add add) =>
        Disposal.Using(
            () => new Light { LightStyle = Style(add.Source), IsEnabled = true },
            light =>
                from placed in IO.lift(() => Placed(light, add.Source))
                from named in IO.lift(() => { _ = add.Name.Iter(name => light.Name = name); })
                from index in TableOps.WithAttributes(doc, add.Attributes, attributes => IO.lift(() => Answers.NonNegative(doc.Lights.Add(light, attributes), nameof(LightTable.Add))))
                select index);

    private static LightStyle Style(LightSource source) =>
        source.Map(
            pointLight: LightStyle.WorldPoint,
            spotLight: LightStyle.WorldSpot,
            directionalLight: LightStyle.WorldDirectional,
            linearLight: LightStyle.WorldLinear,
            rectangularLight: LightStyle.WorldRectangular);

    private static void Placed(Light light, LightSource source) =>
        source.Switch(
            light,
            pointLight: LightMapper.Update,
            spotLight: LightMapper.Update,
            directionalLight: LightMapper.Update,
            linearLight: LightMapper.Update,
            rectangularLight: LightMapper.Update);

    private static IO<Unit> Edited(Light light, LightEdit edit) =>
        from write in IO.lift(() => Validated(light.LightStyle, edit))
        from written in IO.lift(() => write(light))
        select written;

    private static Fin<Action<Light>> Validated(LightStyle style, LightEdit edit) =>
        edit.Switch(
            style,
            rename: static (_, rename) => Fin.Succ<Action<Light>>(target => target.Name = rename.Name.IfNone("")),
            enable: static (_, enable) => Fin.Succ<Action<Light>>(target => target.IsEnabled = enable.On),
            power: static (current, power) => Invalid.Unless<Action<Light>>(
                double.IsFinite(power.Magnitude) && (power.Magnitude >= 0.0),
                target => _ = power.Unit switch {
                    PowerUnit.Watts => target.PowerWatts = power.Magnitude,
                    PowerUnit.Lumens => target.PowerLumens = power.Magnitude,
                    PowerUnit.Candela => target.PowerCandela = power.Magnitude,
                    PowerUnit.Intensity => target.Intensity = power.Magnitude,
                },
                nameof(LightEdit.Power.Magnitude)),
            colors: static (current, colors) => Fin.Succ<Action<Light>>(target => {
                target.Diffuse = colors.Diffuse;
                _ = colors.Ambient.Iter(color => target.Ambient = color);
                _ = colors.Specular.Iter(color => target.Specular = color);
            }),
            shadowIntensity: static (_, shadow) => Fin.Succ<Action<Light>>(target => target.ShadowIntensity = shadow.Intensity),
            place: static (current, place) => Accepts(current, nameof(LightEdit.Place), Style(place.Source)).Map<Action<Light>>(_ => target => Placed(target, place.Source)),
            attenuationType: static (_, attenuation) => Fin.Succ<Action<Light>>(target => target.AttenuationType = attenuation.Kind),
            attenuationVector: static (_, attenuation) => Fin.Succ<Action<Light>>(target => target.SetAttenuation(attenuation.Coefficients.X, attenuation.Coefficients.Y, attenuation.Coefficients.Z)));

    private static Fin<Unit> Accepts(LightStyle style, string member, params LightStyle[] accepted) =>
        toSeq(accepted).Exists(item => item == style) ? unit : new WrongLightStyle(style, member);
}

[Mapper]
internal static partial class LightMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update([MappingTarget] Light light, LightSource.PointLight source);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update([MappingTarget] Light light, LightSource.SpotLight source);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update([MappingTarget] Light light, LightSource.DirectionalLight source);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update([MappingTarget] Light light, LightSource.LinearLight source);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update([MappingTarget] Light light, LightSource.RectangularLight source);
}
