using System.Drawing;
using System.Globalization;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Render;
using UnitsNet;
using Wacton.Unicolour;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Emission(Color4f Light, double Luminance);

[ComplexValueObject]
[ValidationError<RhinoFault>]
public sealed partial class ColorFilter {
    public double Red { get; }
    public double Green { get; }
    public double Blue { get; }

    public Emission Light => Lights.Peaked(Red, Green, Blue);

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref double red, ref double green, ref double blue) =>
        validationError = Math.Min(red, Math.Min(green, blue)) >= 0d && Math.Max(red, Math.Max(green, blue)) is > 0d and < double.PositiveInfinity
            ? null
            : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"(red, green, blue) = ({red}, {green}, {blue})"), RhinoReason.Outside);
}

[Union]
public abstract partial record LightShape {
    private LightShape(LightStyle style) => Style = style;

    public LightStyle Style { get; }

    internal abstract void Write(Light light);

    public sealed record PointLight(Point3d Location, double Radius, bool CameraRelative) : LightShape(CameraRelative ? LightStyle.CameraPoint : LightStyle.WorldPoint) {
        internal override void Write(Light light) => (light.Location, light.Radius) = (Location, Radius);
    }

    public sealed record SpotLight(Point3d Location, Vector3d Direction, Angle Angle, double HotSpot, double Radius, bool CameraRelative)
        : LightShape(CameraRelative ? LightStyle.CameraSpot : LightStyle.WorldSpot) {
        internal override void Write(Light light) =>
            (light.Location, light.Direction, light.SpotAngleRadians, light.HotSpot, light.Radius) = (Location, Direction, Angle.Radians.ToDouble(), HotSpot, Radius);
    }

    public sealed record DirectionalLight(Point3d Location, Vector3d Direction, Angle Angle, bool CameraRelative)
        : LightShape(CameraRelative ? LightStyle.CameraDirectional : LightStyle.WorldDirectional) {
        internal override void Write(Light light) => (light.Location, light.Direction, light.Radius) = (Location, Direction, Angle.Degrees.ToDouble());
    }

    public sealed record LinearLight(Point3d Location, Vector3d Length, Vector3d Width) : LightShape(LightStyle.WorldLinear) {
        internal override void Write(Light light) => (light.Location, light.Length, light.Width) = (Location, Length, Width);
    }

    public sealed record RectangularLight(Point3d Location, Vector3d Length, Vector3d Width, Vector3d Direction) : LightShape(LightStyle.WorldRectangular) {
        internal override void Write(Light light) => (light.Location, light.Length, light.Width, light.Direction) = (Location, Length, Width, Direction);
    }

    public sealed record AmbientLight() : LightShape(LightStyle.Ambient) {
        internal override void Write(Light light) { }
    }

    public static Option<LightShape> Of(Light light) =>
        light.LightStyle switch {
            LightStyle.CameraPoint or LightStyle.WorldPoint => new PointLight(light.Location, light.Radius, light.LightStyle is LightStyle.CameraPoint),
            LightStyle.CameraSpot or LightStyle.WorldSpot => new SpotLight(
                light.Location, light.Direction, Angle.FromRadians(Quantities.Exact(light.SpotAngleRadians)), light.HotSpot, light.Radius, light.LightStyle is LightStyle.CameraSpot),
            LightStyle.CameraDirectional or LightStyle.WorldDirectional =>
                new DirectionalLight(light.Location, light.Direction, Angle.FromDegrees(Quantities.Exact(light.Radius)), light.LightStyle is LightStyle.CameraDirectional),
            LightStyle.WorldLinear => new LinearLight(light.Location, light.Length, light.Width),
            LightStyle.WorldRectangular => new RectangularLight(light.Location, light.Length, light.Width, light.Direction),
            LightStyle.Ambient => new AmbientLight(),
            LightStyle.None => None,
        };
}

[Union]
public abstract partial record LightColor {
    internal abstract (Color Diffuse, double Gain) Emitted(float gamma);

    public sealed record Rgb(Color Diffuse) : LightColor {
        internal override (Color Diffuse, double Gain) Emitted(float gamma) => (Diffuse, 1d);
    }

    public sealed record Blackbody(Temperature Kelvin) : LightColor {
        internal override (Color Diffuse, double Gain) Emitted(float gamma) => Lights.Emitted(Lights.Blackbody((double)Kelvin.Kelvins), gamma);
    }

    public sealed record Linear(ColorFilter Filter) : LightColor {
        internal override (Color Diffuse, double Gain) Emitted(float gamma) => Lights.Emitted(Filter.Light, gamma);
    }
}

[Union]
public abstract partial record LightPower {
    internal abstract void Write(Light light);

    public sealed record Watts(Power Value) : LightPower {
        internal override void Write(Light light) => (light.PowerWatts, light.PowerLumens, light.PowerCandela) = (Value.Watts.ToDouble(), 0d, 0d);
    }

    public sealed record Lumens(LuminousFlux Value) : LightPower {
        internal override void Write(Light light) => (light.PowerWatts, light.PowerLumens, light.PowerCandela) = (0d, Value.Lumens.ToDouble(), 0d);
    }

    public sealed record Candela(LuminousIntensity Value) : LightPower {
        internal override void Write(Light light) => (light.PowerWatts, light.PowerLumens, light.PowerCandela) = (0d, 0d, Value.Candela.ToDouble());
    }

    public static Option<LightPower> Of(Light light) =>
        Some(light.PowerWatts).Filter(static watts => watts > 0d).Map(static watts => (LightPower)new Watts(Power.FromWatts(Quantities.Exact(watts))))
        || Some(light.PowerLumens).Filter(static lumens => lumens > 0d).Map(static lumens => (LightPower)new Lumens(LuminousFlux.FromLumens(Quantities.Exact(lumens))))
        || Some(light.PowerCandela).Filter(static candela => candela > 0d).Map(static candela => (LightPower)new Candela(LuminousIntensity.FromCandela(Quantities.Exact(candela))));
}

public sealed record LightSpec(Option<string> Name, bool Enabled, LightShape Shape, LightColor Color, double Intensity, Option<LightPower> Power, double ShadowIntensity, Vector3d AttenuationVector);

public sealed record LightState(Guid Id, LightSpec Spec);

public sealed record ManagedLight(LightState Light, bool Solo, string Description);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Lights {
    // --- [EMISSION]
    public static Emission Peaked(double red, double green, double blue) =>
        Peaked(new Unicolour(Configuration.Default, ColourSpace.RgbLinear, red, green, blue));

    public static Emission Blackbody(double kelvins) =>
        Peaked(new Unicolour(Configuration.Default, kelvins, Locus.Blackbody));

    private static Emission Peaked(Unicolour light) =>
        Scaled(Scaled(light).MapToRgbGamut()) switch {
            var peaked => new Emission(new Color4f((float)peaked.RgbLinear.R, (float)peaked.RgbLinear.G, (float)peaked.RgbLinear.B, 1f), peaked.Xyz.Y),
        };

    private static Unicolour Scaled(Unicolour light) =>
        light.RgbLinear switch {
            var rgb => double.Max(double.Abs(rgb.R), double.Max(double.Abs(rgb.G), double.Abs(rgb.B))) switch {
                var peak => new Unicolour(light.Configuration, ColourSpace.RgbLinear, rgb.R / peak, rgb.G / peak, rgb.B / peak),
            },
        };

    internal static (Color Diffuse, double Gain) Emitted(Emission emission, float gamma) =>
        (Color4f.ApplyGamma(emission.Light, 1f / gamma).AsSystemColor(), 1d / emission.Luminance);

    public static IO<float> PreProcessGamma(RhinoDoc doc) =>
        (from settings in use(() => doc.RenderSettings)
         from flow in use(() => settings.LinearWorkflow)
         select flow.PreProcessColors ? flow.PreProcessGamma : 1f).Bracket();

    // --- [STAGE]
    public static Stage<Light> Stage(RhinoDoc doc, Guid id) =>
        Stages.Geometry<Light>() switch {
            var geometry => geometry with {
                Commit = (live, staged) =>
                    from valid in geometry.Commit(live, staged)
                    from modified in Products.Accepted(() => doc.Lights.Modify(id, valid))
                    select valid,
            },
        };

    public static Step<Light> Written(LightSpec spec, float gamma) =>
        Steps.Then<Light>(staged => Written(staged, spec, spec.Color.Emitted(gamma)));

    private static Light Written(Light light, LightSpec spec, (Color Diffuse, double Gain) emitted) {
        (light.LightStyle, light.Name, light.IsEnabled, light.Diffuse, light.Intensity, light.ShadowIntensity, light.AttenuationVector) =
            (spec.Shape.Style, Conversions.Unset(spec.Name), spec.Enabled, emitted.Diffuse, spec.Intensity * emitted.Gain, spec.ShadowIntensity, spec.AttenuationVector);
        spec.Shape.Write(light);
        _ = spec.Power.Iter(power => power.Write(light));
        return light;
    }

    // --- [READS]
    public static Fin<LightState> State(Light light) =>
        Conversions.Required(LightShape.Of(light)).Map(shape => new LightState(light.Id, new LightSpec(
            Conversions.Present(light.Name), light.IsEnabled, shape, new LightColor.Rgb(light.Diffuse), light.Intensity, LightPower.Of(light), light.ShadowIntensity, light.AttenuationVector)));

    public static IO<Seq<LightState>> States(RhinoDoc doc) =>
        IO.lift(() => Conversions.Rows(doc.Lights).Filter(static row => !row.IsDeleted).Map(static row => row.LightGeometry).Strict().Traverse(State).As());

    public static IO<(Seq<Error> Fails, Seq<ManagedLight> Succs)> Managed(RhinoDoc doc) =>
        (from client in use(() => new LightManagerSupportClient(doc.RuntimeSerialNumber))
         from lights in use(client.GetLights)
         select toSeq(Range(0, lights.Count())).Map(index => lights.ElementAt(index))
             .Map(light => State(light).Map(state => new ManagedLight(state, client.GetLightSolo(light), client.LightDescription(light)))).Strict().Partition()).Bracket();

    // --- [WRITES]
    public static IO<Guid> Add(RhinoDoc doc, LightSpec spec, Option<ObjectAttributes> attributes) =>
        (from gamma in PreProcessGamma(doc)
         from staged in use(static () => new Light())
         from written in Written(spec, gamma).Run(staged)
         from index in Products.Accepted(() => doc.Lights.Add(written, attributes.ValueUnsafe()) switch { var added => (added >= 0, added) })
         select doc.Lights[index].Id).Bracket();

    public static IO<Unit> Modify(RhinoDoc doc, Guid id, LightSpec spec) =>
        (from gamma in PreProcessGamma(doc)
         from live in Found(doc, id)
         from modified in Edits.Apply(Stage(doc, id), live.LightGeometry, Seq(Written(spec, gamma)))
         select unit).Bracket();

    public static IO<LightObject> Found(RhinoDoc doc, Guid id) =>
        Products.Accepted(() => doc.Lights.Find(id, ignoreDeleted: true) switch { var index => (index >= 0, index) }).Map(index => doc.Lights[index]);

    // --- [RECONCILE]
    public static IO<Seq<(string Slot, Guid Id, bool Created)>> Reconcile(RhinoDoc doc, KeyName group, Seq<(string Slot, LightSpec Spec)> lights) =>
        from unique in IO.lift(Conversions.Unique(lights, static light => light.Slot, StringComparer.Ordinal))
        from key in IO.lift(() => $"{Session.PlugIn.Id:N}.{group}")
        from held in IO.lift(() => toHashMap(Conversions.Rows(doc.Lights).Filter(static row => !row.IsDeleted)
            .Choose(row => Conversions.Present(row.Attributes.GetUserString(key)).Map(slot => (slot, row.Id)))))
        from placed in lights.TraverseM(light => held.Find(light.Slot).Match(
            Some: id => Modify(doc, id, light.Spec).Map(_ => (light.Slot, Id: id, Created: false)),
            None: () => (from attributes in use(doc.CreateDefaultAttributes)
                         from keyed in Products.Accepted(() => attributes.SetUserString(key, light.Slot))
                         from id in Add(doc, light.Spec, Some(attributes))
                         select (light.Slot, Id: id, Created: true)).Bracket())).As()
        let kept = toHashSet(lights.Map(static light => light.Slot))
        from removed in toSeq<(string Key, Guid Value)>(held).Filter(row => !kept.Contains(row.Key)).TraverseM(row =>
            Found(doc, row.Value).Bind(found => Products.Accepted(() => doc.Lights.Delete(found.Index, quiet: true)))).As()
        select placed;
}
