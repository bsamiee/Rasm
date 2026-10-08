using System.Drawing;
using System.Globalization;
using LanguageExt.ClassInstances;
using Rasm.Imaging.ColorManagement;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Render;
using Riok.Mapperly.Abstractions;
using Wacton.Unicolour;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct LightIntensity : System.Numerics.IMinMaxValue<LightIntensity> {
    public static LightIntensity MinValue { get; } = new(0d);
    public static LightIntensity MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct LightPower : System.Numerics.IMinMaxValue<LightPower> {
    public static LightPower MinValue { get; } = new(0d);
    public static LightPower MaxValue { get; } = new(double.MaxValue);
    public static LightPower Default { get; } = new(0d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct ShadowIntensity : System.Numerics.IMinMaxValue<ShadowIntensity> {
    public static ShadowIntensity MinValue { get; } = new(0d);
    public static ShadowIntensity MaxValue { get; } = new(1d);
    public static ShadowIntensity Default { get; } = new(1d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct EmitterRadius : System.Numerics.IMinMaxValue<EmitterRadius> {
    public static EmitterRadius MinValue { get; } = new(0d);
    public static EmitterRadius MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct SpotAngle : System.Numerics.IMinMaxValue<SpotAngle> {
    public static SpotAngle MinValue { get; } = new(0d);
    public static SpotAngle MaxValue { get; } = new(Math.PI / 2d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct HotSpot : System.Numerics.IMinMaxValue<HotSpot> {
    public static HotSpot MinValue { get; } = new(0d);
    public static HotSpot MaxValue { get; } = new(1d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct SunAngle : System.Numerics.IMinMaxValue<SunAngle> {
    public static SunAngle MinValue { get; } = new(0d);
    public static SunAngle MaxValue { get; } = new(Math.PI);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ComplexValueObject]
[ValidationError<InvalidRhinoValue>]
public sealed partial class ColorFilter {
    public double Red { get; }
    public double Green { get; }
    public double Blue { get; }

    public Unicolour Unicolour => new(Gamut.StandardRgb.Configuration, ColourSpace.RgbLinear, Red, Green, Blue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double red, ref double green, ref double blue) =>
        validationError = Math.Min(red, Math.Min(green, blue)) >= 0d && Math.Max(red, Math.Max(green, blue)) is > 0d and < double.PositiveInfinity
            ? null
            : new InvalidRhinoValue();
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
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
    public abstract LightStyle LightStyle { get; }

    public sealed record PointLight(Point3d Location, EmitterRadius Radius, bool CameraRelative) : LightShape {
        public override LightStyle LightStyle => CameraRelative ? LightStyle.CameraPoint : LightStyle.WorldPoint;
    }

    public sealed record SpotLight(Point3d Location, Vector3d Direction, SpotAngle Angle, HotSpot HotSpot, EmitterRadius Radius, bool CameraRelative) : LightShape {
        public override LightStyle LightStyle => CameraRelative ? LightStyle.CameraSpot : LightStyle.WorldSpot;
    }

    public sealed record DirectionalLight(Point3d Location, Vector3d Direction, SunAngle Angle, bool CameraRelative) : LightShape {
        public override LightStyle LightStyle => CameraRelative ? LightStyle.CameraDirectional : LightStyle.WorldDirectional;
    }

    public sealed record LinearLight(Point3d Location, Vector3d Length, Vector3d Width) : LightShape {
        public override LightStyle LightStyle => LightStyle.WorldLinear;
    }

    public sealed record RectangularLight(Point3d Location, Vector3d Length, Vector3d Width, Vector3d Direction) : LightShape {
        public override LightStyle LightStyle => LightStyle.WorldRectangular;
    }

    public sealed record AmbientLight() : LightShape {
        public override LightStyle LightStyle => LightStyle.Ambient;
    }
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
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source, EnabledConversions = MappingConversionType.ImplicitCast)]
internal static partial class LightMapper {
    [MapProperty(nameof(LightSpec.Enabled), nameof(Light.IsEnabled))]
    [MapProperty(nameof(LightSpec.Intensity), nameof(Light.Intensity), Use = nameof(Gained))]
    [MapProperty(nameof(LightSpec.Watts), nameof(Light.PowerWatts))]
    [MapperIgnoreSource(nameof(LightSpec.Shape), Justification = "Each shape case writes its own members")]
    [MapperIgnoreSource(nameof(LightSpec.Color), Justification = "The emitted diffuse and gain carry the color")]
    internal static partial void Update(LightSpec spec, Light light, Color diffuse, double gain);

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

    private static double Gained(LightIntensity intensity, double gain) => intensity * gain;

    private static double Degrees(SunAngle angle) => RhinoMath.ToDegrees(angle);
}

public static class Lights {
    // --- [EMISSION]
    internal static IO<Transfer> Gamma(RhinoDoc doc) =>
        (from settings in use(() => doc.RenderSettings)
         from flow in use(() => settings.LinearWorkflow)
         from transfer in IO.lift(() => flow.PreProcessColors
             ? Conversions.Validated<GammaExponent, float, InvalidColor>(flow.PreProcessGamma).Map(static exponent => (Transfer)exponent)
             : (Transfer)TransferCurve.Linear)
         select transfer).Bracket();

    private static (Color Diffuse, double Gain) Emitted(Unicolour color, Transfer transfer) {
        double peak = Math.Max(color.RgbLinear.R, Math.Max(color.RgbLinear.G, color.RgbLinear.B));
        Span<System.Numerics.Vector4> light = [new((float)(color.RgbLinear.R / peak), (float)(color.RgbLinear.G / peak), (float)(color.RgbLinear.B / peak), 1f)];
        transfer.Encode(light, Nits.ReferenceWhite);
        return (Color.FromArgb(Lane(light[0].X), Lane(light[0].Y), Lane(light[0].Z)), peak / color.Xyz.Y);
    }

    private static int Lane(float encoded) => (int)MathF.Round(255f * encoded, MidpointRounding.ToEven);

    private static Validation<Error, (LightColor Color, LightIntensity Intensity)> Emission(Light light, Transfer transfer) =>
        Conversions.Present(light.GetUserString(LightKey.Kelvin.Key))
            .Bind(static text => Callbacks.Found(double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double kelvin), kelvin))
            .Bind(static kelvin => Conversions.Validated<ColorTemperature, double, InvalidColor>(kelvin).ToOption())
            .Map(kelvin => (Kelvin: kelvin, Emitted: Emitted(Adaptation.Blackbody(Gamut.StandardRgb, kelvin), transfer)))
            .Filter(held => Conversions.Same(held.Emitted.Diffuse, light.Diffuse))
            .Match(
                Some: static held => ((LightColor)held.Kelvin, held.Emitted.Gain),
                None: () => ((LightColor)light.Diffuse, 1d)) switch {
            var (color, gain) => Conversions.Validated<LightIntensity, double, InvalidRhinoValue>(light.Intensity / gain).ToValidation().Map(intensity => (color, intensity)),
        };

    // --- [ADDRESSES]
    public static IO<LightObject> Find(RhinoDoc doc, ComponentRef<LightObject> address, bool includeDeleted) =>
        IO.lift(() => address.Switch<(LightTable Lights, bool Deleted), Fin<Option<int>>>(
                (Lights: doc.Lights, Deleted: includeDeleted),
                byId: static (state, byId) => Conversions.Present(state.Lights.Find(byId.Id, ignoreDeleted: !state.Deleted)),
                byIndex: static (state, byIndex) => Some(byIndex.Index).Filter(index => index >= 0 && index < state.Lights.Count),
                byName: static (state, _) => new NonUniqueName(state.Lights.ComponentType))
            .Bind(found => found.Map(index => doc.Lights[index]).Filter(row => includeDeleted || !row.IsDeleted).ToFin(new MissingComponent<LightObject>(address))));

    // --- [READS]
    public static IO<LightState> Read(RhinoDoc doc, ComponentRef<LightObject> address) =>
        from row in Find(doc, address, includeDeleted: false)
        from transfer in Gamma(doc)
        from state in IO.lift(State(row.LightGeometry, transfer))
        select state;

    public static IO<(Seq<Error> Fails, Seq<LightState> Succs)> States(RhinoDoc doc) =>
        from transfer in Gamma(doc)
        from rows in IO.lift(() => toSeq(doc.Lights).Filter(static row => !row.IsDeleted).Strict())
        select rows.Map(row => State(row.LightGeometry, transfer)).Partition();

    public static IO<(Seq<Error> Fails, Seq<ManagedLight> Succs)> Managed(RhinoDoc doc) =>
        from transfer in Gamma(doc)
        from managed in (from client in use(() => new LightManagerSupportClient(doc.RuntimeSerialNumber))
                         from read in IO.lift(() => toSeq(doc.Lights).Filter(static row => !row.IsDeleted)
                             .Map(row => State(row.LightGeometry, transfer).Map(state => new ManagedLight(state, client.GetLightSolo(row.LightGeometry), client.LightDescription(row.LightGeometry))))
                             .Strict()
                             .Partition())
                         select read).Bracket()
        select managed;

    internal static Fin<LightState> State(Light light, Transfer transfer) =>
        (Shape(light), Emission(light, transfer),
         Conversions.Validated<LightPower, double, InvalidRhinoValue>(light.PowerWatts).ToValidation(),
         Conversions.Validated<ShadowIntensity, double, InvalidRhinoValue>(light.ShadowIntensity).ToValidation())
            .Apply((shape, emission, watts, shadow) => new LightState(light.Id, new LightSpec(
                Conversions.Present(light.Name), light.IsEnabled, shape, emission.Color, emission.Intensity, watts, shadow, light.AttenuationVector)))
            .As()
            .ToFin()
            .MapFail(error => new InvalidLight(light.Id, error));

    private static Validation<Error, LightShape> Shape(Light light) =>
        light.LightStyle switch {
            LightStyle.CameraPoint or LightStyle.WorldPoint =>
                Conversions.Validated<EmitterRadius, double, InvalidRhinoValue>(light.Radius).ToValidation()
                    .Map(radius => (LightShape)new LightShape.PointLight(light.Location, radius, light.LightStyle == LightStyle.CameraPoint)),
            LightStyle.CameraSpot or LightStyle.WorldSpot =>
                (Conversions.Validated<SpotAngle, double, InvalidRhinoValue>(light.SpotAngleRadians).ToValidation(),
                 Conversions.Validated<HotSpot, double, InvalidRhinoValue>(light.HotSpot).ToValidation(),
                 Conversions.Validated<EmitterRadius, double, InvalidRhinoValue>(light.Radius).ToValidation())
                    .Apply((angle, hotSpot, radius) => (LightShape)new LightShape.SpotLight(light.Location, light.Direction, angle, hotSpot, radius, light.LightStyle == LightStyle.CameraSpot))
                    .As(),
            LightStyle.CameraDirectional or LightStyle.WorldDirectional =>
                Conversions.Validated<SunAngle, double, InvalidRhinoValue>(RhinoMath.ToRadians(light.Radius)).ToValidation()
                    .Map(angle => (LightShape)new LightShape.DirectionalLight(light.Location, light.Direction, angle, light.LightStyle == LightStyle.CameraDirectional)),
            LightStyle.WorldLinear => (LightShape)new LightShape.LinearLight(light.Location, light.Length, light.Width),
            LightStyle.WorldRectangular => (LightShape)new LightShape.RectangularLight(light.Location, light.Length, light.Width, light.Direction),
            LightStyle.Ambient => (LightShape)new LightShape.AmbientLight(),
            LightStyle.None => new InvalidAnswer(nameof(Light.LightStyle)),
        };

    // --- [WRITES]
    public static IO<Seq<Guid>> Apply(RhinoDoc doc, LightOp op) =>
        op.Switch(
            doc,
            add: static (document, add) =>
                from transfer in Gamma(document)
                from index in (from staged in use(static () => new Light())
                               from written in Written(staged, add.Spec, transfer)
                               from landed in IO.lift(() => Conversions.Required(document.Lights.Add(staged, add.Attributes.ValueUnsafe()), nameof(LightTable.Add)))
                               select landed).Bracket()
                select Seq(document.Lights[index].Id),
            modify: static (document, modify) =>
                from row in Find(document, modify.Address, includeDeleted: false)
                from transfer in Gamma(document)
                from landed in (from staged in use(() => row.DuplicateLightGeometry())
                                from written in Written(staged, modify.Spec, transfer)
                                from accepted in IO.lift(() => Refused.Unless(document.Lights.Modify(row.Index, staged), nameof(LightTable.Modify)))
                                select accepted).Bracket()
                select Seq(row.Id),
            delete: static (document, delete) =>
                from row in Find(document, delete.Address, includeDeleted: false)
                from deleted in IO.lift(() => Refused.Unless(document.Lights.Delete(row.Index, delete.Quiet), Seq(row.Id), nameof(LightTable.Delete)))
                select deleted,
            undelete: static (document, undelete) =>
                from row in Find(document, undelete.Address, includeDeleted: true)
                from restored in IO.lift(() => Refused.Unless(document.Lights.Undelete(row.Index), Seq(row.Id), nameof(LightTable.Undelete)))
                select restored,
            solo: static (document, solo) =>
                from rows in solo.Lights.TraverseM(address => Find(document, address, includeDeleted: false)).As()
                from set in (from client in use(() => new LightManagerSupportClient(document.RuntimeSerialNumber))
                             from each in IO.lift(() => Callbacks.Each(rows, row => client.SetLightSolo(row.LightGeometry, solo.On), nameof(LightManagerSupportClient.SetLightSolo)))
                             select each).Bracket()
                select rows.Map(static row => row.Id).Strict(),
            grouping: static (document, grouping) =>
                from rows in grouping.Lights.TraverseM(address => Find(document, address, includeDeleted: false)).As()
                from grouped in (from client in use(() => new LightManagerSupportClient(document.RuntimeSerialNumber))
                                 from lights in use(static () => new LightArray())
                                 from appended in IO.lift(() => rows.Iter(row => lights.Append(row.LightGeometry)))
                                 from joined in IO.lift(() => (grouping.On ? (Action<LightArray>)client.GroupLights : client.UnGroup)(lights))
                                 select joined).Bracket()
                select rows.Map(static row => row.Id).Strict());

    internal static IO<Unit> Written(Light staged, LightSpec spec, Transfer transfer) =>
        from emitted in IO.lift(() => spec.Color.Switch<Transfer, (Color Diffuse, double Gain, Option<string> Kelvin)>(
            transfer,
            rgb: static (_, rgb) => (rgb.Diffuse, 1d, None),
            blackbody: static (curve, blackbody) => Emitted(Adaptation.Blackbody(Gamut.StandardRgb, blackbody.Kelvin), curve) switch {
                var (diffuse, gain) => (diffuse, gain, Some(((double)blackbody.Kelvin).ToString(CultureInfo.InvariantCulture))),
            },
            linear: static (curve, linear) => Emitted(linear.Filter.Unicolour, curve) switch {
                var (diffuse, gain) => (diffuse, gain, None),
            }))
        from members in IO.lift(() => LightMapper.Update(spec, staged, emitted.Diffuse, emitted.Gain))
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
