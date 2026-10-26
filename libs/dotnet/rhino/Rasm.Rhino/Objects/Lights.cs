using System.Drawing;
using System.Globalization;
using LanguageExt.ClassInstances;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Render.Settings;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Render;
using Riok.Mapperly.Abstractions;
using UnitsNet;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct LightIntensity : System.Numerics.IMinMaxValue<LightIntensity> {
    public static LightIntensity MinValue { get; } = new(0d);
    public static LightIntensity MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Default", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct LightPower : System.Numerics.IMinMaxValue<LightPower> {
    public static LightPower MinValue { get; } = new(0d);
    public static LightPower MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ShadowIntensity : System.Numerics.IMinMaxValue<ShadowIntensity> {
    public static ShadowIntensity MinValue { get; } = new(0d);
    public static ShadowIntensity MaxValue { get; } = new(1d);
    public static ShadowIntensity Default { get; } = new(1d);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct EmitterRadius : System.Numerics.IMinMaxValue<EmitterRadius> {
    public static EmitterRadius MinValue { get; } = new(0d);
    public static EmitterRadius MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SpotAngle : System.Numerics.IMinMaxValue<SpotAngle> {
    public static SpotAngle MinValue { get; } = new(0d);
    public static SpotAngle MaxValue { get; } = new(Math.PI / 2d);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct HotSpot : System.Numerics.IMinMaxValue<HotSpot> {
    public static HotSpot MinValue { get; } = new(0d);
    public static HotSpot MaxValue { get; } = new(1d);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SunAngle : System.Numerics.IMinMaxValue<SunAngle> {
    public static SunAngle MinValue { get; } = new(0d);
    public static SunAngle MaxValue { get; } = new(Math.PI);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ComplexValueObject]
public sealed partial class ColorFilter {
    public double Red { get; }
    public double Green { get; }
    public double Blue { get; }

    public (Color4f Light, double Luminance) Light => Conversions.Peaked(Red, Green, Blue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double red, ref double green, ref double blue) =>
        validationError = Math.Min(red, Math.Min(green, blue)) >= 0d && Math.Max(red, Math.Max(green, blue)) is > 0d and < double.PositiveInfinity
            ? null
            : new ValidationError("light color needs non-negative finite channels with one above zero");
}

[SmartEnum<string>(SkipIParsable = true, SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LightKey {
    public static readonly LightKey Kelvin = new("rasm-kelvin");
    public static readonly LightKey Rig = new("rasm-rig");
    public static readonly LightKey Slot = new("rasm-rig-slot");
    public static readonly LightKey Role = new("rasm-rig-role");
    public static readonly LightKey Preset = new("rasm-rig-preset");
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

    public sealed record PointLight(Point3d Location, EmitterRadius Radius, bool CameraRelative) : LightShape;
    public sealed record SpotLight(Point3d Location, Vector3d Direction, SpotAngle Angle, HotSpot HotSpot, EmitterRadius Radius, bool CameraRelative) : LightShape;
    public sealed record DirectionalLight(Point3d Location, Vector3d Direction, SunAngle Angle, bool CameraRelative) : LightShape;
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
    LightIntensity Intensity,
    LightPower Watts,
    ShadowIntensity ShadowIntensity,
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

    [MapProperty(nameof(LightShape.SpotLight.Angle), nameof(Light.SpotAngleRadians))]
    [MapperIgnoreSource(nameof(LightShape.SpotLight.CameraRelative), Justification = "LightStyle holds frame")]
    internal static partial void Update([MappingTarget] Light light, LightShape.SpotLight spot);

    [MapProperty(nameof(LightShape.DirectionalLight.Angle), nameof(Light.Radius), Use = nameof(Degrees))]
    [MapperIgnoreSource(nameof(LightShape.DirectionalLight.CameraRelative), Justification = "LightStyle holds frame")]
    internal static partial void Update([MappingTarget] Light light, LightShape.DirectionalLight directional);

    internal static partial void Update([MappingTarget] Light light, LightShape.LinearLight linear);

    internal static partial void Update([MappingTarget] Light light, LightShape.RectangularLight rectangular);

    internal static partial void Update([MappingTarget] Light light, LightShape.AmbientLight ambient);

    // --- [QUANTITIES]
    [UserMapping]
    private static double Scalar(LightIntensity intensity) => intensity;
    [UserMapping]
    private static double Scalar(LightPower watts) => watts;
    [UserMapping]
    private static double Scalar(ShadowIntensity shadow) => shadow;
    [UserMapping]
    private static double Scalar(EmitterRadius radius) => radius;
    [UserMapping]
    private static double Scalar(SpotAngle angle) => angle;
    [UserMapping]
    private static double Scalar(HotSpot hotSpot) => hotSpot;
    [UserMapping]
    private static double Degrees(SunAngle angle) => RhinoMath.ToDegrees(angle);
}

public static class Lights {
    // --- [EMISSION]
    public static IO<Gamma> PreProcessGamma(RhinoDoc doc) =>
        DisposalOps.Within(() => doc.RenderSettings, static settings => DisposalOps.Within(() => settings.LinearWorkflow, static flow =>
            IO.lift(() => flow.PreProcessColors ? Gamma.Validated(flow.PreProcessGamma) : Gamma.Linear)));

    private static (Color Diffuse, double Gain) Emitted((Color4f Light, double Luminance) emission, Gamma gamma) =>
        (Color4f.ApplyGamma(emission.Light, 1f / gamma).AsSystemColor(), 1d / emission.Luminance);

    private static Fin<(LightColor Color, LightIntensity Intensity)> Emission(Light light, Gamma gamma) =>
        Conversions.Present(light.GetUserString(LightKey.Kelvin.Key))
            .Bind(static text => Invariant.Number<double>(text))
            .Filter(static kelvins => kelvins > 0d)
            .Map(kelvins => (Kelvin: UnitsNet.Temperature.FromKelvins(kelvins), Emitted: Emitted(Conversions.Blackbody(kelvins), gamma)))
            .Filter(held => Conversions.Same(held.Emitted.Diffuse, light.Diffuse))
            .Match(
                Some: static held => ((LightColor)held.Kelvin, held.Emitted.Gain),
                None: () => ((LightColor)light.Diffuse, 1d)) switch {
                    var (color, gain) => LightIntensity.Validated(light.Intensity / gain).Map(intensity => (color, intensity)),
                };

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
        from gamma in PreProcessGamma(doc)
        from state in IO.lift(State(row.LightGeometry, gamma))
        select state;

    public static IO<(Seq<Error> Fails, Seq<LightState> Succs)> States(RhinoDoc doc) =>
        from gamma in PreProcessGamma(doc)
        from rows in IO.lift(() => Conversions.Rows(doc.Lights).Filter(static row => !row.IsDeleted).Strict())
        select rows.Map(row => State(row.LightGeometry, gamma)).Partition();

    public static IO<(Seq<Error> Fails, Seq<ManagedLight> Succs)> Managed(RhinoDoc doc) =>
        from gamma in PreProcessGamma(doc)
        from managed in DisposalOps.Within(() => new LightManagerSupportClient(doc.RuntimeSerialNumber), client =>
            from lights in IO.lift(() => Conversions.Rows(doc.Lights).Filter(static row => !row.IsDeleted).Map(static row => row.LightGeometry).Strict())
            select lights.Map(light => State(light, gamma).Map(state => new ManagedLight(state, client.GetLightSolo(light), client.LightDescription(light)))).Strict().Partition())
        select managed;

    public static Fin<LightState> State(Light light, Gamma gamma) =>
        (Shape(light), Emission(light, gamma),
         LightPower.Validated(light.PowerWatts),
         ShadowIntensity.Validated(light.ShadowIntensity))
            .Apply((shape, emission, watts, shadow) => new LightState(light.Id, new LightSpec(
                Conversions.Present(light.Name), light.IsEnabled, shape, emission.Color, emission.Intensity, watts, shadow, light.AttenuationVector)))
            .As()
            .MapFail(error => Error.New(ErrorCodes.ValidationFailed, $"light {light.Id} holds invalid value", error));

    private static Fin<LightShape> Shape(Light light) =>
        light.LightStyle switch {
            LightStyle.CameraPoint or LightStyle.WorldPoint =>
                EmitterRadius.Validated(light.Radius)
                    .Map(radius => (LightShape)new LightShape.PointLight(light.Location, radius, light.LightStyle is LightStyle.CameraPoint)),
            LightStyle.CameraSpot or LightStyle.WorldSpot =>
                (SpotAngle.Validated(light.SpotAngleRadians),
                 HotSpot.Validated(light.HotSpot),
                 EmitterRadius.Validated(light.Radius))
                    .Apply((angle, hotSpot, radius) => (LightShape)new LightShape.SpotLight(light.Location, light.Direction, angle, hotSpot, radius, light.LightStyle is LightStyle.CameraSpot))
                    .As(),
            LightStyle.CameraDirectional or LightStyle.WorldDirectional =>
                SunAngle.Validated(RhinoMath.ToRadians(light.Radius))
                    .Map(angle => (LightShape)new LightShape.DirectionalLight(light.Location, light.Direction, angle, light.LightStyle is LightStyle.CameraDirectional)),
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

    public static IO<Unit> Written(Light staged, LightSpec spec, Gamma gamma) =>
        from emitted in IO.lift(() => spec.Color.Switch<Gamma, (Color Diffuse, double Gain, Option<string> Kelvin)>(
            gamma,
            rgb: static (_, rgb) => (rgb.Diffuse, 1d, None),
            blackbody: static (held, blackbody) => Emitted(Conversions.Blackbody((double)blackbody.Kelvin.Kelvins), held) switch {
                var (diffuse, gain) => (diffuse, gain, Some(((double)blackbody.Kelvin.Kelvins).ToString(CultureInfo.InvariantCulture))),
            },
            linear: static (held, linear) => Emitted(linear.Filter.Light, held) switch {
                var (diffuse, gain) => (diffuse, gain, None),
            }))
        from intensity in IO.lift(LightIntensity.Validated(spec.Intensity * emitted.Gain))
        from members in IO.lift(() => LightMapper.Update(spec with { Intensity = intensity }, staged, emitted.Diffuse))
        from shaped in IO.lift(() => spec.Shape.Switch(
            staged,
            pointLight: LightMapper.Update,
            spotLight: LightMapper.Update,
            directionalLight: LightMapper.Update,
            linearLight: LightMapper.Update,
            rectangularLight: LightMapper.Update,
            ambientLight: LightMapper.Update))
        from keyed in UserStrings.Write(
            UserStrings.Held(staged.GetUserStrings()), staged.SetUserString,
            HashMap<EqStringOrdinalIgnoreCase, string, Option<string>>((LightKey.Kelvin.Key, emitted.Kelvin)))
        select keyed;
}
