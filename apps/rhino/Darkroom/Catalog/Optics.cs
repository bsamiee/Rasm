using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Text.Json.Serialization;
using Darkroom.Chain;
using Emgu.CV;
using PeterO.Numbers;
using Rasm.Imaging.Files;
using Rasm.Imaging.Filtering;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Warping;
using Wacton.Unicolour;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DirtSpecks(
    [property: Display(Name = "Coverage", Description = "Share of the lens covered by dirt")][property: Range(0.01d, 1d)] float Coverage,
    [property: Display(Name = "Scale", Description = "Dirt feature size in frame short sides")][property: Range(0.001d, 0.5d)] ShortSideExtent Scale,
    [property: Display(Name = "Seed", Description = "Number selecting the dirt field")] Seed Seed) {
    public static DirtSpecks Default { get; } = new(0.2f, ShortSideExtent.Create(0.05f * ReferenceFrame.GreaterSide), Seed.MinValue);

    public static PixelFrame Field((DirtSpecks Specks, PixelExtent Extent) key) =>
        PinkSpecks.Field(key.Extent, key.Specks.Scale.Pixels(key.Extent), key.Specks.Coverage, key.Specks.Seed.Field(21u, 0u));
}

public sealed record Bloom(
    [property: Display(Name = "Size", Description = "Spread reach across the frame")][property: Range(0d, 1d)] float Size,
    [property: Display(Name = "Strength", Description = "Share of light the lens scatters with total light kept")][property: Range(0d, 1d)] float Strength,
    [property: Display(Name = "Lens dirt", Description = "Scene-linear dust and smudge image scattered light shows")] string? Dirt,
    [property: Display(Name = "Specks", Description = "Procedural lens dirt in place of the dirt image")] DirtSpecks? Specks,
    [property: Display(Name = "Dirt intensity", Description = "Dirt gain over scattered light")][property: Range(0d, 5d)] float DirtGain) : IPixelStage<Bloom> {
    public static Bloom Default { get; } = new(0.5f, 0.7f * float.Pow(0.15f, 1.5f), Dirt: null, Specks: null, 1f);

    public static Option<PixelPass> Pass(Bloom state, PassContext context) =>
        state.Strength is 0f ? None : Some<PixelPass>(new PixelPass.Frame((frame, progress) => frame.Spread(LightSpread.Light, light => {
            _ = TentPyramid.Spread(light, (int)float.Log2(float.Max(1f, frame.Size.ShortSide * state.Size)) - 1);
            return Optional(state.Specks).Match<Fin<Unit>>(
                Some: specks => TentPyramid.Soiled(light, frame, context.Derivations.Derived((specks, context.Extent), DirtSpecks.Field), state.DirtGain),
                None: () => Optional(state.Dirt)
                    .Map(path => context.Derivations.Derived(path, static held => ImageFile.Load(held).RunSafe()).Map(file => TentPyramid.Soiled(light, frame, file.Frame, state.DirtGain)))
                    .IfNone(unit));
        }, BlendingMode.Mix, state.Strength, progress)));
}

public sealed record VeilingGlare([property: Display(Name = "Size", Description = "Field of view the glare kernel spans, narrower at higher values")][property: Range(0d, 1d)] float Size)
    : IPixelStage<VeilingGlare> {
    public static VeilingGlare Default { get; } = new(0.5f);

    public static Option<PixelPass> Pass(VeilingGlare state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => frame.Spread(LightSpread.Light,
            light => context.Derivations.Held((state.Size, context.Extent, Window: frame.Size),
                static key => Fin.Succ(PointSpread.Veiling(key.Window, double.Lerp(180d, 10d, double.Cbrt(key.Size)) / int.Max(key.Extent.Width, key.Extent.Height))),
                spread => spread.Convolve(light)),
            BlendingMode.Mix, 1f, progress)));
}

public sealed record StarGlare(
    [property: Display(Name = "Aperture", Description = "Lens opening shaping bokeh and diffraction spikes")] Iris Aperture,
    [property: Display(Name = "Core", Description = "First dark ring radius around point lights in frame short sides")][property: Range(1.25d / ReferenceFrame.Height, 64d / ReferenceFrame.Height)] ShortSideLength Radius)
    : IPixelStage<StarGlare> {
    public static StarGlare Default { get; } = new(Iris.Create(6, double.Pi / 3d, 0d, 0d, 1d), ShortSideLength.Create(2f / ReferenceFrame.Height));

    public static Option<PixelPass> Pass(StarGlare state, PassContext context) =>
        state.Radius == ShortSideLength.Neutral || state.Aperture.Obstruction is 1d ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => frame.Spread(LightSpread.Light,
                light => context.Derivations.Held((State: state, context.Extent, Window: frame.Size, context.Working),
                    static key => Fin.Succ(PointSpread.Diffraction(key.State.Aperture, key.State.Radius.Pixels(key.Extent), key.Window, key.Extent, key.Working)),
                    spread => spread.Convolve(light)),
                BlendingMode.Mix, 1f, progress)));
}

public sealed record Ghosts(
    [property: Display(Name = "Highlights", Description = "Highlights the lens reflects")] HighlightKey Highlight,
    [property: Display(Name = "Iterations", Description = "Reflection passes, each adding four ghosts")][property: Range(2, 5)] int Iterations,
    [property: Display(Name = "Color modulation", Description = "Color shift between successive ghosts")][property: Range(0d, 1d)] float Modulation) : IPixelStage<Ghosts> {
    public static Ghosts Default { get; } = new(HighlightKey.Create(1f, 0.1f), 3, 0.25f);

    public static Option<PixelPass> Pass(Ghosts state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => frame.Spread(state.Highlight.Extract,
            light => GhostReflections.Reflected(light, frame, 16f / ReferenceFrame.Height * frame.Extent.ShortSide, state.Iterations, state.Modulation), BlendingMode.Add, 1f, progress)));
}

public sealed record Halation(
    [property: Display(Name = "Highlights", Description = "Highlights that re-expose the film")] HighlightKey Highlight,
    [property: Display(Name = "Radius", Description = "Halo radius in frame short sides")][property: Range(0d, 64d / ReferenceFrame.Height)] ShortSideLength Radius,
    [property: Display(Name = "Color", Description = "Color weighting the halo")][property: JsonConverter(typeof(UnicolourConverter))] Unicolour Color) : IPixelStage<Halation> {
    public static Halation Default { get; } = new(HighlightKey.Create(2f, 1f), ShortSideLength.Create(64f * 0.2f / ReferenceFrame.Height), new Unicolour(ColourSpace.Rgb, 1d, 0x59 / 255d, 0d));

    public static Option<PixelPass> Pass(Halation state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Color.ConvertToConfiguration(context.Working.Configuration).RgbLinear switch {
            var linear => new Vector4((float)linear.R, (float)linear.G, (float)linear.B, 0f) switch {
                var color => frame.Spread(pixel => state.Highlight.Extract(pixel) * color, light => light.Blur(light, state.Radius.Pixels(frame.Extent)), BlendingMode.Add, 1f, progress),
            },
        }));
}

[SmartEnum<string>]
public sealed partial class FocusField {
    public static readonly FocusField Uniform = new("uniform", [], static (state, _, cap) => Some(Bokeh.Uniform(state.Aperture, state.Fringe, cap)));
    public static readonly FocusField Depth = new("depth", [GuideChannel.Depth], static (state, context, cap) =>
        from depth in context.Guides.Find(GuideChannel.Depth)
        from camera in context.Camera
        from field in Bokeh.Depth(depth, camera, state.FStop, cap, context.Extent)
        select field);
    public static readonly FocusField Curvature = new("curvature", [], static (state, context, cap) => Some(state.Focal(context, state.Aspect, 2, cap)));
    public static readonly FocusField Tilt = new("tilt", [], static (state, context, cap) => Some(state.Focal(context, 0d, 1, cap)));

    public Seq<GuideChannel> Guides { get; }

    [UseDelegateFromConstructor]
    public partial Option<Func<Mat, Mat, PixelFrame, Fin<Unit>>> Field(Defocus state, PassContext context, double cap);
}

public sealed record Defocus(
    [property: Display(Name = "Focus", Description = "Field setting blur radius across frame")] FocusField Field,
    [property: Display(Name = "Max blur", Description = "Largest blur radius in frame short sides")][property: Range(0d, 0.1d * ReferenceFrame.GreaterSide)] float Radius,
    [property: Display(Name = "F-stop", Description = "Lens aperture ratio, lower values defocus more")][property: Range(0.5d, 128d)] float FStop,
    [property: Display(Name = "Sharp region", Description = "Center and counterclockwise turn of the sharp band or ellipse")] Placement Placement,
    [property: Display(Name = "Sharp width", Description = "Sharp band half-width")][property: Range(0d, 1d)] ShortSideLength Extent,
    [property: Display(Name = "Aspect", Description = "Sharp ellipse from band at 0% to circle at 100%")][property: Range(0d, 1d)] float Aspect,
    [property: Display(Name = "Feather", Description = "Distance past the sharp region to full blur")][property: Range(0.001d, 1d)] ShortSideExtent Feather,
    [property: Display(Name = "Aperture", Description = "Lens opening shaping bokeh and diffraction spikes")] Iris Aperture,
    [property: Display(Name = "Color shift", Description = "Red and blue bokeh sizes apart from green")][property: Range(-1d, 1d)] float Fringe) : IPixelStage<Defocus> {
    public static Defocus Default { get; } = new(
        FocusField.Depth, 0.02f * ReferenceFrame.GreaterSide, 2.8f, Placement.Default, ShortSideLength.Create(0.15f), 1f, ShortSideExtent.Create(0.25f), Iris.Create(6, double.Pi / 3d, 1d, 0d, 1d), 0f);

    public static Seq<GuideChannel> Channels(Defocus state) => state.Field.Guides;

    public static Option<PixelPass> Pass(Defocus state, PassContext context) =>
        state.Radius is 0f ? None
            : state.Field.Field(state, context, state.Radius * context.Extent.ShortSide).Map<PixelPass>(field => Bokeh.Gathered(state.Aperture, state.Fringe, field));

    public Func<Mat, Mat, PixelFrame, Fin<Unit>> Focal(PassContext context, double aspect, int power, double cap) =>
        Bokeh.Focal(Placement.Frame(context.Extent), Extent, Feather, aspect, power, cap);
}

public sealed record LensDistortion(
    [property: Display(Name = "Lens", Description = "Distortion bending straight lines from or toward center, dispersion spreading red and blue, fit and direction")] RadialDistortion Lens,
    [property: Display(Name = "Wrap", Description = "Source sampling past frame edges")] WrapMode Wrap) : IPixelStage<LensDistortion> {
    public static LensDistortion Default { get; } = new(RadialDistortion.Create(0.05d, 0.05d, LensFraming.Edges, LensDirection.Distort), WrapMode.Clamp);

    public static Option<PixelPass> Pass(LensDistortion state, PassContext context) =>
        state.Lens is { Coefficient: 0d, Dispersion: 0d } ? None : Some<PixelPass>(state.Lens.Pass(state.Wrap));
}

public sealed record AxialAberration(
    [property: Display(Name = "Radius", Description = "Blue defocus radius against green, red at half")][property: Range(0d, 80d / ReferenceFrame.Height)] ShortSideLength Radius,
    [property: Display(Name = "Aperture", Description = "Lens opening shaping bokeh and diffraction spikes")] Iris Aperture) : IPixelStage<AxialAberration> {
    public static AxialAberration Default { get; } = new(ShortSideLength.Create(20f / ReferenceFrame.Height), Defocus.Default.Aperture);

    public static Option<PixelPass> Pass(AxialAberration state, PassContext context) =>
        state.Radius == ShortSideLength.Neutral ? None : Some<PixelPass>(new AxialDefocus(state.Aperture, state.Radius).Pass);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic]
[JsonDerivedType(typeof(Natural), nameof(Natural))]
[JsonDerivedType(typeof(Outline), nameof(Outline))]
public abstract partial record Vignette : IPixelStage<Vignette> {
    public static Vignette Default { get; } = new Natural();

    public static Option<PixelPass> Pass(Vignette state, PassContext context) =>
        state.Switch(context,
            natural: static (run, _) => run.Camera.Filter(static camera => camera.Frustum is Frustum.Perspective)
                .Map<PixelPass>(camera => Pulled(Vector3.Zero, (column, line) => camera.Illumination((column + 0.5d, (double)run.Extent.Height - line - 0.5d)).IfNone(1f))),
            outline: static (run, outline) => Some<PixelPass>(outline.Falloff(run)));

    public sealed record Natural : Vignette;

    public sealed record Outline(
        [property: Display(Name = "Width", Description = "Illuminated region width as share of frame width")][property: Range(0d, 1d)] float Width,
        [property: Display(Name = "Height", Description = "Illuminated region height as share of frame height")][property: Range(0d, 1d)] float Height,
        [property: Display(Name = "Roundness", Description = "Corner rounding from rectangle at 0% to rounded ends at 100%")][property: Range(0d, 1d)] float Roundness,
        [property: Display(Name = "Feather", Description = "Edge softness in frame short sides")][property: Range(0d, 1d)] ShortSideLength Feather,
        [property: Display(Name = "Pull", Description = "Color beyond the outline")][property: JsonConverter(typeof(UnicolourConverter))] Unicolour Pull) : Vignette {
        public static new Outline Default { get; } = new(0.8f, 0.8f, 1f, ShortSideLength.Create(0.2f * float.Sqrt(float.Tau) / 3f), new Unicolour(ColourSpace.RgbLinear, 0d, 0d, 0d));

        public PixelPass.Pointwise Falloff(PassContext context) {
            (PixelExtent extent, EContext math) = (context.Extent, DistanceField.Precision(context.Extent));
            DistanceField field = DistanceField.Box((EFloat.FromDouble(Width * extent.Width / 2d), EFloat.FromDouble(Height * extent.Height / 2d)), Roundness, math);
            double band = ShapeEdge.Band(Feather, extent);
            RgbLinear pull = Pull.ConvertToConfiguration(context.Working.Configuration).RgbLinear;
            return Pulled(new Vector3((float)pull.R, (float)pull.G, (float)pull.B), (column, line) => ShapeEdge.Coverage(field.Sample((
                EFloat.FromDouble(column + 0.5d - (extent.Width / 2d)), EFloat.FromDouble((extent.Height / 2d) - line - 0.5d))).Distance.ToDouble(), band));
        }
    }

    private static PixelPass.Pointwise Pulled(Vector3 pull, Func<long, long, float> weight) =>
        new((row, column, line) => {
            for (int x = 0; x < row.Length; x++)
                row[x] = new(Vector3.Lerp(pull, row[x].AsVector3(), weight(column + x, line)), row[x].W);
        });
}
