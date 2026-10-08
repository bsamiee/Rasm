using System.Drawing;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using NodaTime.Text;
using Rasm.Imaging.ColorManagement;
using Rasm.Rhino.Document.Geolocation;
using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Objects;
using Rasm.Rhino.Render.Content;
using Rhino;
using Rhino.Render;

namespace Rasm.Rhino.Render.Scenes;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct MetresPerUnit : System.Numerics.IMinMaxValue<MetresPerUnit> {
    public static MetresPerUnit MinValue { get; } = new(double.BitIncrement(0d));
    public static MetresPerUnit MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

public sealed record SkylightReference(Guid Environment, uint RenderHash, Option<SkylightIntensity> Gain);

internal sealed partial record SceneFile(string Captured, string Key, SceneFile.Body Scene) {
    [Union]
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(PointLight), "point")]
    [JsonDerivedType(typeof(SpotLight), "spot")]
    [JsonDerivedType(typeof(DirectionalLight), "directional")]
    [JsonDerivedType(typeof(LinearLight), "linear")]
    [JsonDerivedType(typeof(RectangularLight), "rectangular")]
    [JsonDerivedType(typeof(AmbientLight), "ambient")]
    internal abstract partial record Shape {
        internal sealed record PointLight(double[] Location, double Radius, bool CameraRelative) : Shape;

        internal sealed record SpotLight(double[] Location, double[] Direction, double Angle, double HotSpot, double Radius, bool CameraRelative) : Shape;

        internal sealed record DirectionalLight(double[] Location, double[] Direction, double Angle, bool CameraRelative) : Shape;

        internal sealed record LinearLight(double[] Location, double[] Length, double[] Width) : Shape;

        internal sealed record RectangularLight(double[] Location, double[] Length, double[] Width, double[] Direction) : Shape;

        internal sealed record AmbientLight() : Shape;
    }

    [Union]
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "authority")]
    [JsonDerivedType(typeof(Watts), "watts")]
    [JsonDerivedType(typeof(Scale), "scale")]
    internal abstract partial record Power {
        internal sealed record Watts(double Value) : Power;

        internal sealed record Scale(double Value) : Power;
    }

    internal sealed record Light(Guid Id, string? Name, bool Enabled, Shape Shape, float[] Emission, Power Power, double Shadow, double[] Falloff);

    internal sealed record Sun(
        bool Enabled, double Intensity, float[] Emission, double Azimuth, double Altitude, double North,
        double Latitude, double Longitude, string StandardOffset, string? Saving, string? Moment);

    internal sealed record Skylight(Guid Environment, uint RenderHash, double? Gain);

    internal sealed record Body(Light[] Lights, Sun Sun, Skylight? Skylight);
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals)]
[JsonSerializable(typeof(SceneFile))]
[JsonSerializable(typeof(SceneFile.Body))]
internal sealed partial class SceneFileContext : JsonSerializerContext;

public sealed record LightScene(
    Instant Captured,
    MetresPerUnit Scale,
    Transfer LightTransfer,
    Seq<LightState> Emitters,
    SunState SunState,
    SunAngles SunAngles,
    Option<SunMoment> Moment,
    Color SunColor,
    Option<SkylightReference> Skylight) {
    // --- [CAPTURE]
    public static IO<LightScene> Capture(RhinoDoc doc, TimeProvider clock) =>
        from captured in IO.lift(clock.GetCurrentInstant)
        from scale in IO.lift(() => Conversions.Validated<MetresPerUnit, double, InvalidRhinoValue>(Quantities.From(1d, doc.ModelUnits).Meters.ToDouble()))
        from transfer in Lights.Gamma(doc)
        from read in Lights.States(doc)
        from _ in unless(read.Fails.IsEmpty, IO.fail<Unit>(Error.Many(read.Fails))).As()
        from sun in Sources.Read(doc, window =>
            from state in SunState.Read(window)
            from angles in SunState.Angles(window)
            from moment in SunState.Moment(window, clock).Read
            from color in IO.lift(() => Sun.ColorFromAltitude(angles.Altitude))
            select (State: state, Angles: angles, Moment: moment, Color: color))
        from skylight in Skylights(doc)
        select new LightScene(captured, scale, transfer, read.Succs, sun.State, sun.Angles, sun.Moment, sun.Color, skylight);

    private static IO<Option<SkylightReference>> Skylights(RhinoDoc doc) =>
        Environments.Current(doc, RenderSettings.EnvironmentUsage.Skylighting).Bind(static environment => environment.Traverse(static lit =>
            from gain in Environments.Intensity(lit)
            from hash in Contents.Hash(lit, CrcRenderHashFlags.Normal, Seq<string>(), None)
            select new SkylightReference(lit.Id, hash, gain)).As());

    // --- [EMISSION]
    private static float[] Emission(LightColor color, Transfer transfer) =>
        color.Switch(
            transfer,
            rgb: static (curve, rgb) => Decoded(rgb.Diffuse, curve),
            blackbody: static (_, blackbody) => Adaptation.Blackbody(Gamut.StandardRgb, blackbody.Kelvin).RgbLinear switch {
                var linear => [(float)linear.R, (float)linear.G, (float)linear.B],
            },
            linear: static (_, linear) => linear.Filter.Unicolour switch {
                var filter => [(float)(filter.RgbLinear.R / filter.Xyz.Y), (float)(filter.RgbLinear.G / filter.Xyz.Y), (float)(filter.RgbLinear.B / filter.Xyz.Y)],
            });

    private static float[] Decoded(Color color, Transfer transfer) {
        Span<System.Numerics.Vector4> light = [new System.Numerics.Vector4(color.R, color.G, color.B, byte.MaxValue) / byte.MaxValue];
        transfer.Decode(light, Nits.ReferenceWhite);
        return new float[] { light[0].X, light[0].Y, light[0].Z };
    }

    // --- [DESCRIPTION]
    public ContentKey Key => Keyed(Body());

    public ReadOnlyMemory<byte> Description() =>
        Body() switch {
            var body => JsonSerializer.SerializeToUtf8Bytes(
                new SceneFile(InstantPattern.ExtendedIso.Format(Captured), Keyed(body).ToString("x32", CultureInfo.InvariantCulture), body),
                SceneFileContext.Default.SceneFile),
        };

    private static ContentKey Keyed(SceneFile.Body body) =>
        ContentKey.Of(KeyDomain.Scene, stream => stream.Rows(toSeq(JsonSerializer.SerializeToUtf8Bytes(body, SceneFileContext.Default.Body)), ContentKeys.Integer));

    private SceneFile.Body Body() =>
        new(
            [.. Emitters.Map(light => Entry(light, Scale.ToValue(), LightTransfer))],
            new SceneFile.Sun(
                SunState.Enabled, SunState.Intensity.ToValue(), Decoded(SunColor, LightTransfer),
                double.DegreesToRadians(SunAngles.Azimuth), double.DegreesToRadians(SunAngles.Altitude), double.DegreesToRadians(SunState.Site.North.ToValue()),
                SunState.Site.Place.Latitude.ToValue(), SunState.Site.Place.Longitude.ToValue(),
                OffsetPattern.GeneralInvariant.Format(SunState.Site.Standard.ToValue()), SunState.Site.Saving.Map(static saving => OffsetPattern.GeneralInvariant.Format(saving.ToValue())).ValueUnsafe(),
                Moment.Map(static moment => moment.ToValue()).ValueUnsafe()),
            Skylight.Map(static reference => new SceneFile.Skylight(reference.Environment, reference.RenderHash, reference.Gain.Map(static gain => gain.ToValue()).ToNullable())).ValueUnsafe());

    private static SceneFile.Light Entry(LightState light, double scale, Transfer transfer) =>
        new(
            light.Id, light.Spec.Name.ValueUnsafe(), light.Spec.Enabled, Shape(light.Spec.Shape, scale), Emission(light.Spec.Color, transfer),
            light.Spec.Watts.ToValue() > 0d ? new SceneFile.Power.Watts(light.Spec.Watts.ToValue()) : new SceneFile.Power.Scale(light.Spec.Intensity.ToValue()),
            light.Spec.ShadowIntensity.ToValue(),
            [light.Spec.AttenuationVector.X, light.Spec.AttenuationVector.Y / scale, light.Spec.AttenuationVector.Z / scale / scale]);

    private static SceneFile.Shape Shape(LightShape shape, double scale) =>
        shape.Switch<double, SceneFile.Shape>(
            scale,
            pointLight: static (s, point) => new SceneFile.Shape.PointLight(Metres((Vector3d)point.Location, s), point.Radius.ToValue() * s, point.CameraRelative),
            spotLight: static (s, spot) => new SceneFile.Shape.SpotLight(
                Metres((Vector3d)spot.Location, s), Metres(spot.Direction, s), spot.Angle.ToValue(), spot.HotSpot.ToValue(), spot.Radius.ToValue() * s, spot.CameraRelative),
            directionalLight: static (s, directional) => new SceneFile.Shape.DirectionalLight(
                Metres((Vector3d)directional.Location, s), Metres(directional.Direction, s), directional.Angle.ToValue(), directional.CameraRelative),
            linearLight: static (s, linear) => new SceneFile.Shape.LinearLight(Metres((Vector3d)linear.Location, s), Metres(linear.Length, s), Metres(linear.Width, s)),
            rectangularLight: static (s, rectangle) => new SceneFile.Shape.RectangularLight(
                Metres((Vector3d)rectangle.Location, s), Metres(rectangle.Length, s), Metres(rectangle.Width, s), Metres(rectangle.Direction, s)),
            ambientLight: static (_, _) => new SceneFile.Shape.AmbientLight());

    private static double[] Metres(Vector3d model, double scale) => [model.X * scale, model.Y * scale, model.Z * scale];
}
