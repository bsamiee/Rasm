using System.Drawing;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using NodaTime.Text;
using Rasm.Rhino.Document;
using Rasm.Rhino.Objects;
using Rasm.Rhino.Render.Content;
using Rhino;
using Rhino.Display;
using Rhino.Render;

namespace Rasm.Rhino.Render.Settings;

// --- [MODELS] --------------------------------------------------------------------------
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

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals, RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(SceneFile))]
[JsonSerializable(typeof(SceneFile.Body))]
internal sealed partial class SceneFileContext : JsonSerializerContext;

public sealed record LightScene {
    private LightScene(Instant captured, SceneFile.Body scene) => (Captured, Scene) = (captured, scene);

    public Instant Captured { get; }

    internal SceneFile.Body Scene { get; }

    // --- [CAPTURE]
    public static IO<LightScene> Capture(RhinoDoc doc, TimeProvider clock) =>
        from captured in IO.lift(clock.GetCurrentInstant)
        let scale = RhinoMath.UnitScale(doc.ModelUnitSystem, UnitSystem.Meters)
        from gamma in Lights.PreProcessGamma(doc)
        from read in Lights.States(doc)
        from sun in RenderSettingsSources.Read(doc, scope =>
            (from held in use(() => scope.Settings.Sun)
             from copy in use(static () => new Sun())
             from entry in IO.lift(() => {
                 copy.CopyFrom(held);
                 copy.Accuracy = Sun.Accuracies.Maximum;
                 return Entry(copy, gamma);
             })
             select entry).Bracket())
        from skylight in Environments.Current(doc, RenderSettings.EnvironmentUsage.Skylighting).Bind(static environment => environment.Traverse(static lit =>
            from gain in Environments.Intensity(lit)
            from hash in Contents.Hash(lit, CrcRenderHashFlags.Normal, [], None)
            select new SceneFile.Skylight(lit.Id, hash, gain.ToNullable())).As())
        select new LightScene(captured, new SceneFile.Body([.. read.Map(light => Entry(light, scale, gamma))], sun, skylight.ValueUnsafe()));

    // --- [EMISSION]
    private static float[] Emitted(LightColor color, float gamma) =>
        color.Switch(
            gamma,
            rgb: static (held, rgb) => Decoded(rgb.Diffuse, held),
            blackbody: static (_, blackbody) => Normalized(Lights.Blackbody((double)blackbody.Kelvin.Kelvins)),
            linear: static (_, linear) => Normalized(linear.Filter.Light));

    private static float[] Normalized(Emission emission) =>
        [(float)(emission.Light.R / emission.Luminance), (float)(emission.Light.G / emission.Luminance), (float)(emission.Light.B / emission.Luminance)];

    private static float[] Decoded(Color color, float gamma) =>
        Color4f.ApplyGamma(new Color4f(color), gamma) switch {
            var light => [light.R, light.G, light.B],
        };

    // --- [DESCRIPTION]
    public ContentKey Key => Keyed(Scene);

    public ReadOnlyMemory<byte> Description() =>
        JsonSerializer.SerializeToUtf8Bytes(
            new SceneFile(InstantPattern.ExtendedIso.Format(Captured), Keyed(Scene).ToString("x32", CultureInfo.InvariantCulture), Scene),
            SceneFileContext.Default.SceneFile);

    private static ContentKey Keyed(SceneFile.Body body) =>
        ContentKey.Of<SceneFile>(stream => stream.Bytes(JsonSerializer.SerializeToUtf8Bytes(body, SceneFileContext.Default.Body)));

    private static SceneFile.Sun Entry(Sun sun, float gamma) =>
        SunSite.Read(sun) switch {
            var site => new SceneFile.Sun(
                sun.Enabled, sun.Intensity, Decoded(Sun.ColorFromAltitude(sun.Altitude), gamma),
                double.DegreesToRadians(sun.Azimuth), double.DegreesToRadians(sun.Altitude), site.North.Radians.ToDouble(),
                site.Place.Latitude.Degrees.ToDouble(), site.Place.Longitude.Degrees.ToDouble(),
                OffsetPattern.GeneralInvariant.Format(site.Standard), site.Saving.Map(static saving => OffsetPattern.GeneralInvariant.Format(Offset.FromTimeSpan(saving.ToTimeSpan()))).ValueUnsafe(),
                LocalDateTimePattern.ExtendedIso.Format(SunSite.Moment(sun))),
        };

    private static SceneFile.Light Entry(LightState light, double scale, float gamma) =>
        new(
            light.Id, light.Spec.Name.ValueUnsafe(), light.Spec.Enabled, Shape(light.Spec.Shape, scale), Emitted(light.Spec.Color, gamma),
            light.Spec.Watts > 0d ? new SceneFile.Power.Watts(light.Spec.Watts) : new SceneFile.Power.Scale(light.Spec.Intensity),
            light.Spec.ShadowIntensity,
            [light.Spec.AttenuationVector.X, light.Spec.AttenuationVector.Y / scale, light.Spec.AttenuationVector.Z / scale / scale]);

    private static SceneFile.Shape Shape(LightShape shape, double scale) =>
        shape.Switch<double, SceneFile.Shape>(
            scale,
            pointLight: static (s, point) => new SceneFile.Shape.PointLight(Meters((Vector3d)point.Location, s), point.Radius * s, point.CameraRelative),
            spotLight: static (s, spot) => new SceneFile.Shape.SpotLight(
                Meters((Vector3d)spot.Location, s), Meters(spot.Direction, s), spot.Angle.Radians.ToDouble(), spot.HotSpot, spot.Radius * s, spot.CameraRelative),
            directionalLight: static (s, directional) => new SceneFile.Shape.DirectionalLight(
                Meters((Vector3d)directional.Location, s), Meters(directional.Direction, s), directional.Angle.Radians.ToDouble(), directional.CameraRelative),
            linearLight: static (s, linear) => new SceneFile.Shape.LinearLight(Meters((Vector3d)linear.Location, s), Meters(linear.Length, s), Meters(linear.Width, s)),
            rectangularLight: static (s, rectangle) => new SceneFile.Shape.RectangularLight(
                Meters((Vector3d)rectangle.Location, s), Meters(rectangle.Length, s), Meters(rectangle.Width, s), Meters(rectangle.Direction, s)),
            ambientLight: static (_, _) => new SceneFile.Shape.AmbientLight());

    private static double[] Meters(Vector3d model, double scale) => [model.X * scale, model.Y * scale, model.Z * scale];
}
