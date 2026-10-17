using System.Drawing;
using System.Globalization;
using LanguageExt.ClassInstances;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Render;
using Riok.Mapperly.Abstractions;
using Wacton.Unicolour;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct LightIntensity : System.Numerics.IMinMaxValue<LightIntensity> {
    public static LightIntensity MinValue { get; } = new(0d);
    public static LightIntensity MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = Admission.Refusal(value, MinValue, MaxValue);
}

[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Default", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct LightPower : System.Numerics.IMinMaxValue<LightPower> {
    public static LightPower MinValue { get; } = new(0d);
    public static LightPower MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = Admission.Refusal(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ShadowIntensity : System.Numerics.IMinMaxValue<ShadowIntensity> {
    public static ShadowIntensity MinValue { get; } = new(0d);
    public static ShadowIntensity MaxValue { get; } = new(1d);
    public static ShadowIntensity Default { get; } = new(1d);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = Admission.Refusal(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct EmitterRadius : System.Numerics.IMinMaxValue<EmitterRadius> {
    public static EmitterRadius MinValue { get; } = new(0d);
    public static EmitterRadius MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = Admission.Refusal(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SpotAngle : System.Numerics.IMinMaxValue<SpotAngle> {
    public static SpotAngle MinValue { get; } = new(0d);
    public static SpotAngle MaxValue { get; } = new(Math.PI / 2d);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = Admission.Refusal(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct HotSpot : System.Numerics.IMinMaxValue<HotSpot> {
    public static HotSpot MinValue { get; } = new(0d);
    public static HotSpot MaxValue { get; } = new(1d);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = Admission.Refusal(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SunAngle : System.Numerics.IMinMaxValue<SunAngle> {
    public static SunAngle MinValue { get; } = new(0d);
    public static SunAngle MaxValue { get; } = new(Math.PI);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = Admission.Refusal(value, MinValue, MaxValue);
}

[ComplexValueObject]
public sealed partial class ColorFilter {
    public double Red { get; }
    public double Green { get; }
    public double Blue { get; }

    public Unicolour Unicolour => Adaptation.Peaked(new Unicolour(Gamut.StandardRgb.Configuration, ColourSpace.RgbLinear, Red, Green, Blue));

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

    public sealed record Blackbody(ColorTemperature Kelvin) : LightColor;

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
    [MapperIgnoreSource(nameof(LightSpec.Color), Justification = "Written supplies the emitted diffuse and scaled intensity")]
    internal static partial void Update(LightSpec spec, Light light, Color diffuse);

    [MapperIgnoreSource(nameof(LightShape.PointLight.CameraRelative), Justification = "LightStyle carries the frame")]
    internal static partial void Update([MappingTarget] Light light, LightShape.PointLight point);

    [MapProperty(nameof(LightShape.SpotLight.Angle), nameof(Light.SpotAngleRadians))]
    [MapperIgnoreSource(nameof(LightShape.SpotLight.CameraRelative), Justification = "LightStyle carries the frame")]
    internal static partial void Update([MappingTarget] Light light, LightShape.SpotLight spot);

    [MapProperty(nameof(LightShape.DirectionalLight.Angle), nameof(Light.Radius), Use = nameof(Degrees))]
    [MapperIgnoreSource(nameof(LightShape.DirectionalLight.CameraRelative), Justification = "LightStyle carries the frame")]
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
    public static IO<Transfer> Gamma(RhinoDoc doc) =>
        DisposalOps.Within(() => doc.RenderSettings, static settings => DisposalOps.Within(() => settings.LinearWorkflow, static flow =>
            IO.lift(() => flow.PreProcessColors
                ? PositiveMagnitude.Validated(flow.PreProcessGamma).Map(static exponent => (Transfer)exponent)
                : (Transfer)TransferCurve.Linear)));

    private static (Color Diffuse, double Gain) Emitted(Unicolour color, Transfer transfer) {
        Unicolour peaked = Adaptation.Peaked(color);
        Span<System.Numerics.Vector4> light = [new((float)peaked.RgbLinear.R, (float)peaked.RgbLinear.G, (float)peaked.RgbLinear.B, 1f)];
        transfer.Encode(light, Nits.ReferenceWhite);
        return (Color.FromArgb(Lane(light[0].X), Lane(light[0].Y), Lane(light[0].Z)), 1d / peaked.Xyz.Y);

        static int Lane(float encoded) => (int)MathF.Round(byte.MaxValue * encoded, MidpointRounding.ToEven);
    }

    private static Validation<Error, (LightColor Color, LightIntensity Intensity)> Emission(Light light, Transfer transfer) =>
        Conversions.Present(light.GetUserString(LightKey.Kelvin.Key))
            .Bind(static text => Invariant.Number<double>(text))
            .Bind(static kelvin => ColorTemperature.Validated(kelvin).ToOption())
            .Map(kelvin => (Kelvin: kelvin, Emitted: Emitted(Adaptation.Blackbody(Gamut.StandardRgb, kelvin), transfer)))
            .Filter(held => Conversions.Same(held.Emitted.Diffuse, light.Diffuse))
            .Match(
                Some: static held => ((LightColor)held.Kelvin, held.Emitted.Gain),
                None: () => ((LightColor)light.Diffuse, 1d)) switch {
                    var (color, gain) => LightIntensity.Validated(light.Intensity / gain).ToValidation().Map(intensity => (color, intensity)),
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
        from transfer in Gamma(doc)
        from state in IO.lift(State(row.LightGeometry, transfer))
        select state;

    public static IO<(Seq<Error> Fails, Seq<LightState> Succs)> States(RhinoDoc doc) =>
        from transfer in Gamma(doc)
        from rows in IO.lift(() => Conversions.Rows(doc.Lights).Filter(static row => !row.IsDeleted).Strict())
        select rows.Map(row => State(row.LightGeometry, transfer)).Partition();

    public static IO<(Seq<Error> Fails, Seq<ManagedLight> Succs)> Managed(RhinoDoc doc) =>
        from transfer in Gamma(doc)
        from managed in DisposalOps.Within(() => new LightManagerSupportClient(doc.RuntimeSerialNumber), client =>
            from lights in IO.lift(() => Conversions.Rows(doc.Lights).Filter(static row => !row.IsDeleted).Map(static row => row.LightGeometry).Strict())
            select lights.Map(light => State(light, transfer).Map(state => new ManagedLight(state, client.GetLightSolo(light), client.LightDescription(light)))).Strict().Partition())
        select managed;

    public static Fin<LightState> State(Light light, Transfer transfer) =>
        (Shape(light), Emission(light, transfer),
         LightPower.Validated(light.PowerWatts).ToValidation(),
         ShadowIntensity.Validated(light.ShadowIntensity).ToValidation())
            .Apply((shape, emission, watts, shadow) => new LightState(light.Id, new LightSpec(
                Conversions.Present(light.Name), light.IsEnabled, shape, emission.Color, emission.Intensity, watts, shadow, light.AttenuationVector)))
            .As()
            .ToFin()
            .MapFail(error => Error.New(ErrorCodes.ValidationFailed, $"light {light.Id} holds a value outside its type", error));

    private static Validation<Error, LightShape> Shape(Light light) =>
        light.LightStyle switch {
            LightStyle.CameraPoint or LightStyle.WorldPoint =>
                EmitterRadius.Validated(light.Radius).ToValidation()
                    .Map(radius => (LightShape)new LightShape.PointLight(light.Location, radius, light.LightStyle is LightStyle.CameraPoint)),
            LightStyle.CameraSpot or LightStyle.WorldSpot =>
                (SpotAngle.Validated(light.SpotAngleRadians).ToValidation(),
                 HotSpot.Validated(light.HotSpot).ToValidation(),
                 EmitterRadius.Validated(light.Radius).ToValidation())
                    .Apply((angle, hotSpot, radius) => (LightShape)new LightShape.SpotLight(light.Location, light.Direction, angle, hotSpot, radius, light.LightStyle is LightStyle.CameraSpot))
                    .As(),
            LightStyle.CameraDirectional or LightStyle.WorldDirectional =>
                SunAngle.Validated(RhinoMath.ToRadians(light.Radius)).ToValidation()
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
            add: static (document, add) => from transfer in Gamma(document)
                                           from added in DisposalOps.Within(static () => new Light(), staged =>
                                               from written in Written(staged, add.Spec, transfer)
                                               from index in IO.lift(() => Conversions.Required(document.Lights.Add(staged, add.Attributes.ValueUnsafe()), nameof(LightTable.Add)))
                                               select Seq(document.Lights[index].Id))
                                           select added,
            modify: static (document, modify) => from row in Find(document, modify.Address, includeDeleted: false)
                                                 from transfer in Gamma(document)
                                                 from modified in DisposalOps.Within(row.DuplicateLightGeometry, staged =>
                                                     from written in Written(staged, modify.Spec, transfer)
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

    public static IO<Unit> Written(Light staged, LightSpec spec, Transfer transfer) =>
        from emitted in IO.lift(() => spec.Color.Switch<Transfer, (Color Diffuse, double Gain, Option<string> Kelvin)>(
            transfer,
            rgb: static (_, rgb) => (rgb.Diffuse, 1d, None),
            blackbody: static (curve, blackbody) => Emitted(Adaptation.Blackbody(Gamut.StandardRgb, blackbody.Kelvin), curve) switch {
                var (diffuse, gain) => (diffuse, gain, Some(((double)blackbody.Kelvin).ToString(CultureInfo.InvariantCulture))),
            },
            linear: static (curve, linear) => Emitted(linear.Filter.Unicolour, curve) switch {
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
