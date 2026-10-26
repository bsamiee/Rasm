using System.Drawing;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Render;
using Riok.Mapperly.Abstractions;
using UnitsNet;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<RhinoFault>]
public sealed partial class ColorFilter {
    public double Red { get; }
    public double Green { get; }
    public double Blue { get; }

    public (Color4f Light, double Luminance) Light => Conversions.Peaked(Red, Green, Blue);

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref double red, ref double green, ref double blue) =>
        validationError = Math.Min(red, Math.Min(green, blue)) >= 0d && Math.Max(red, Math.Max(green, blue)) is > 0d and < double.PositiveInfinity
            ? null
            : new RhinoFault("light color needs non-negative finite channels with one above zero");
}

[Union]
public abstract partial record LightShape {
    public LightStyle LightStyle => Switch(
        pointLight: static point => point.CameraRelative ? LightStyle.CameraPoint : LightStyle.WorldPoint,
        spotLight: static spot => spot.CameraRelative ? LightStyle.CameraSpot : LightStyle.WorldSpot,
        directionalLight: static directional => directional.CameraRelative ? LightStyle.CameraDirectional : LightStyle.WorldDirectional,
        linearLight: static _ => LightStyle.WorldLinear,
        rectangularLight: static _ => LightStyle.WorldRectangular,
        ambientLight: static _ => LightStyle.Ambient);

    public sealed record PointLight(Point3d Location, double Radius, bool CameraRelative) : LightShape;
    public sealed record SpotLight(Point3d Location, Vector3d Direction, Angle Angle, double HotSpot, double Radius, bool CameraRelative) : LightShape;
    public sealed record DirectionalLight(Point3d Location, Vector3d Direction, Angle Angle, bool CameraRelative) : LightShape;
    public sealed record LinearLight(Point3d Location, Vector3d Length, Vector3d Width) : LightShape;
    public sealed record RectangularLight(Point3d Location, Vector3d Length, Vector3d Width, Vector3d Direction) : LightShape;
    public sealed record AmbientLight() : LightShape;
}

[Union]
public abstract partial record LightColor {
    public sealed record Rgb(Color Diffuse) : LightColor;

    public sealed record Blackbody(UnitsNet.Temperature Kelvin) : LightColor;

    public sealed record Linear(ColorFilter Filter) : LightColor;
}

public sealed record LightSpec(
    Option<string> Name,
    bool Enabled,
    LightShape Shape,
    LightColor Color,
    double Intensity,
    double Watts,
    double ShadowIntensity,
    Vector3d AttenuationVector);

public sealed record LightState(Guid Id, LightSpec Spec);

public sealed record ManagedLight(LightState Light, bool Solo, string Description);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LightOp {
    public sealed record Add(LightSpec Spec, Option<ObjectAttributes> Attributes) : LightOp;

    public sealed record Modify(ComponentRef<LightObject> Address, LightSpec Spec) : LightOp;

    public sealed record Delete(ComponentRef<LightObject> Address, bool Quiet) : LightOp;

    public sealed record Undelete(ComponentRef<LightObject> Address) : LightOp;

    public sealed record Solo(Seq<ComponentRef<LightObject>> Lights, bool On) : LightOp;

    public sealed record Grouping(Seq<ComponentRef<LightObject>> Lights, bool On) : LightOp;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
internal static partial class LightMapper {
    // --- [UPDATES]
    [MapProperty(nameof(LightSpec.Enabled), nameof(Light.IsEnabled))]
    [MapProperty(nameof(LightSpec.Watts), nameof(Light.PowerWatts))]
    [MapperIgnoreSource(nameof(LightSpec.Shape), Justification = "Each shape case writes its own members")]
    [MapperIgnoreSource(nameof(LightSpec.Color), Justification = "Written sets emitted diffuse and scaled intensity")]
    internal static partial void Update(LightSpec spec, Light light, Color diffuse);

    [MapperIgnoreSource(nameof(LightShape.PointLight.CameraRelative), Justification = "LightStyle holds frame")]
    internal static partial void Update([MappingTarget] Light light, LightShape.PointLight point);

    [MapProperty(nameof(LightShape.SpotLight.Angle), nameof(Light.SpotAngleRadians), Use = nameof(Radians))]
    [MapperIgnoreSource(nameof(LightShape.SpotLight.CameraRelative), Justification = "LightStyle holds frame")]
    internal static partial void Update([MappingTarget] Light light, LightShape.SpotLight spot);

    [MapProperty(nameof(LightShape.DirectionalLight.Angle), nameof(Light.Radius), Use = nameof(Degrees))]
    [MapperIgnoreSource(nameof(LightShape.DirectionalLight.CameraRelative), Justification = "LightStyle holds frame")]
    internal static partial void Update([MappingTarget] Light light, LightShape.DirectionalLight directional);

    internal static partial void Update([MappingTarget] Light light, LightShape.LinearLight linear);

    internal static partial void Update([MappingTarget] Light light, LightShape.RectangularLight rectangular);

    internal static partial void Update([MappingTarget] Light light, LightShape.AmbientLight ambient);

    // --- [QUANTITIES]
    [UserMapping(Default = false)]
    private static double Radians(Angle angle) => angle.Radians.ToDouble();
    [UserMapping(Default = false)]
    private static double Degrees(Angle angle) => angle.Degrees.ToDouble();
}

public static class Lights {
    // --- [EMISSION]
    public static IO<float> PreProcessGamma(RhinoDoc doc) =>
        DisposalOps.Within(() => doc.RenderSettings, static settings => DisposalOps.Within(() => settings.LinearWorkflow, static flow =>
            IO.lift(() => flow.PreProcessColors ? flow.PreProcessGamma : 1f)));

    private static (Color Diffuse, double Gain) Emitted((Color4f Light, double Luminance) emission, float gamma) =>
        (Color4f.ApplyGamma(emission.Light, 1f / gamma).AsSystemColor(), 1d / emission.Luminance);

    // --- [ADDRESSES]
    public static IO<LightObject> Find(RhinoDoc doc, ComponentRef<LightObject> address, bool includeDeleted) =>
        from found in IO.lift(() => address.Switch<(LightTable Lights, bool Deleted), Fin<Option<LightObject>>>(
                (doc.Lights, Deleted: includeDeleted),
                byId: static (state, byId) => Conversions.Present(state.Lights.Find(byId.Id, ignoreDeleted: !state.Deleted)).Map(index => state.Lights[index]),
                byIndex: static (state, byIndex) => Conversions.Rows(state.Lights).Find(row => row.Index == byIndex.Index && (state.Deleted || !row.IsDeleted)),
                byName: static (state, _) => Error.New($"{state.Lights.ComponentType} names do not identify one row")))
        from row in IO.lift(found.ToFin(new NotFound(nameof(LightObject), $"{address}")))
        select row;

    // --- [READS]
    public static IO<LightState> Read(RhinoDoc doc, ComponentRef<LightObject> address) =>
        from row in Find(doc, address, includeDeleted: false)
        from state in IO.lift(State(row.LightGeometry))
        select state;

    public static IO<(Seq<Error> Fails, Seq<LightState> Succs)> States(RhinoDoc doc) =>
        IO.lift(() => Conversions.Rows(doc.Lights).Filter(static row => !row.IsDeleted).Map(static row => State(row.LightGeometry)).Strict().Partition());

    public static IO<(Seq<Error> Fails, Seq<ManagedLight> Succs)> Managed(RhinoDoc doc) =>
        DisposalOps.Within(() => new LightManagerSupportClient(doc.RuntimeSerialNumber), client =>
            from lights in IO.lift(() => Conversions.Rows(doc.Lights).Filter(static row => !row.IsDeleted).Map(static row => row.LightGeometry).Strict())
            select lights.Map(light => State(light).Map(state => new ManagedLight(state, client.GetLightSolo(light), client.LightDescription(light)))).Strict().Partition());

    public static Fin<LightState> State(Light light) =>
        Shape(light).Map(shape => new LightState(light.Id, new LightSpec(
            Conversions.Present(light.Name), light.IsEnabled, shape, light.Diffuse, light.Intensity, light.PowerWatts, light.ShadowIntensity, light.AttenuationVector)));

    private static Fin<LightShape> Shape(Light light) =>
        light.LightStyle switch {
            LightStyle.CameraPoint or LightStyle.WorldPoint => new LightShape.PointLight(light.Location, light.Radius, light.LightStyle is LightStyle.CameraPoint),
            LightStyle.CameraSpot or LightStyle.WorldSpot => new LightShape.SpotLight(
                light.Location, light.Direction, Angle.FromRadians(light.SpotAngleRadians), light.HotSpot, light.Radius, light.LightStyle is LightStyle.CameraSpot),
            LightStyle.CameraDirectional or LightStyle.WorldDirectional =>
                new LightShape.DirectionalLight(light.Location, light.Direction, Angle.FromDegrees(light.Radius), light.LightStyle is LightStyle.CameraDirectional),
            LightStyle.WorldLinear => new LightShape.LinearLight(light.Location, light.Length, light.Width),
            LightStyle.WorldRectangular => new LightShape.RectangularLight(light.Location, light.Length, light.Width, light.Direction),
            LightStyle.Ambient => new LightShape.AmbientLight(),
            LightStyle.None => new Refused(nameof(Light.LightStyle), Refused.Invalid),
        };

    // --- [WRITES]
    public static IO<Seq<Guid>> Apply(RhinoDoc doc, LightOp op) =>
        op.Switch(
            doc,
            add: static (document, add) => from gamma in PreProcessGamma(document)
                                           from added in DisposalOps.Within(static () => new Light(), staged =>
                                               from written in Written(staged, add.Spec, gamma)
                                               from index in IO.lift(() => Conversions.Required(document.Lights.Add(staged, add.Attributes.ValueUnsafe()), nameof(LightTable.Add)))
                                               select Seq(document.Lights[index].Id))
                                           select added,
            modify: static (document, modify) => from row in Find(document, modify.Address, includeDeleted: false)
                                                 from gamma in PreProcessGamma(document)
                                                 from modified in DisposalOps.Within(row.DuplicateLightGeometry, staged =>
                                                     from written in Written(staged, modify.Spec, gamma)
                                                     from accepted in IO.lift(() => Refused.Unless(document.Lights.Modify(row.Index, staged), nameof(LightTable.Modify)))
                                                     select Seq(row.Id))
                                                 select modified,
            delete: static (document, delete) => from row in Find(document, delete.Address, includeDeleted: false)
                                                 from deleted in IO.lift(() => Refused.Unless(document.Lights.Delete(row.Index, delete.Quiet), Seq(row.Id), nameof(LightTable.Delete)))
                                                 select deleted,
            undelete: static (document, undelete) => from row in Find(document, undelete.Address, includeDeleted: true)
                                                     from restored in IO.lift(() => Refused.Unless(document.Lights.Undelete(row.Index), Seq(row.Id), nameof(LightTable.Undelete)))
                                                     select restored,
            solo: static (document, solo) => from rows in solo.Lights.TraverseM(address => Find(document, address, includeDeleted: false)).As()
                                             from each in DisposalOps.Within(() => new LightManagerSupportClient(document.RuntimeSerialNumber), client =>
                                                 IO.lift(() => Callbacks.Each(rows, row => client.SetLightSolo(row.LightGeometry, solo.On), nameof(LightManagerSupportClient.SetLightSolo))))
                                             select rows.Map(static row => row.Id).Strict(),
            grouping: static (document, grouping) => from rows in grouping.Lights.TraverseM(address => Find(document, address, includeDeleted: false)).As()
                                                     from joined in DisposalOps.Within(() => new LightManagerSupportClient(document.RuntimeSerialNumber), client =>
                                                         DisposalOps.Within(static () => new LightArray(), lights =>
                                                             from appended in IO.lift(() => rows.Iter(row => lights.Append(row.LightGeometry)))
                                                             from joined in IO.lift(() => (grouping.On ? (Action<LightArray>)client.GroupLights : client.UnGroup)(lights))
                                                             select joined))
                                                     select rows.Map(static row => row.Id).Strict());

    public static IO<Unit> Written(Light staged, LightSpec spec, float gamma) =>
        from emitted in IO.lift(() => spec.Color.Switch<float, (Color Diffuse, double Gain)>(
            gamma,
            rgb: static (_, rgb) => (rgb.Diffuse, 1d),
            blackbody: static (held, blackbody) => Emitted(Conversions.Blackbody((double)blackbody.Kelvin.Kelvins), held),
            linear: static (held, linear) => Emitted(linear.Filter.Light, held)))
        from members in IO.lift(() => LightMapper.Update(spec with { Intensity = spec.Intensity * emitted.Gain }, staged, emitted.Diffuse))
        from shaped in IO.lift(() => spec.Shape.Switch(
            staged,
            pointLight: LightMapper.Update,
            spotLight: LightMapper.Update,
            directionalLight: LightMapper.Update,
            linearLight: LightMapper.Update,
            rectangularLight: LightMapper.Update,
            ambientLight: LightMapper.Update))
        select shaped;
}
