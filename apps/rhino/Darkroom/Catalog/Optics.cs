using System.Numerics;
using System.Text.Json.Serialization;
using Darkroom.Chain;
using Emgu.CV;
using Emgu.CV.Structure;
using Rasm.Imaging.Files;
using Rasm.Imaging.Filtering;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Warping;
using Wacton.Unicolour;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic]
[JsonDerivedType(typeof(Image), nameof(Image))]
[JsonDerivedType(typeof(Specks), nameof(Specks))]
public abstract partial record LensDirt {
    public abstract Fin<PixelFrame> Soil(PassContext context);

    public sealed record Image(string Path) : LensDirt {
        public override Fin<PixelFrame> Soil(PassContext context) =>
            context.Derivations.Derived(Path, static path => ImageFile.Read(path).RunSafe()).Map(static read => read.Frame);
    }

    public sealed record Specks(float Coverage, ShortSideLength Scale, Seed Seed) : LensDirt {
        public static Specks Default { get; } = new(0.2f, ShortSideLength.Create(0.05f * ReferenceFrame.GreaterSide), Seed.MinValue);

        public override Fin<PixelFrame> Soil(PassContext context) =>
            context.Derivations.Derived((Specks: this, context.Extent), static key =>
                NoiseDimensions.Spectral(key.Extent, 2d, key.Specks.Scale.Pixels(key.Extent), key.Specks.Coverage, key.Specks.Seed.Field(21u, 0u)));
    }
}

public sealed record Bloom(float Size, float Strength, float DirtGain) : IPixelStage<Bloom> {
    public static Bloom Default { get; } = new(0.5f, 0.7f * float.Pow(0.15f, 1.5f), 1f);

    public Option<LensDirt> Dirt { get; init; }

    public static Fin<Option<PixelPass>> Pass(Bloom state, PassContext context) =>
        state.Strength is 0f ? None : Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
            from dirt in state.Dirt.Traverse(dirt => dirt.Soil(context)).As()
            from spread in frame.Spread(0f, 0f, light => {
                Glare.Bloom(light, (int)float.Log2(float.Max(1f, frame.Size.ShortSide * state.Size)) - 1);
                foreach (PixelFrame soil in dirt.ToSpan())
                    Glare.Dirt(light, frame, soil, state.DirtGain);
            }, BlendingMode.Normal, state.Strength, [], progress)
            select spread));
}

public sealed record VeilingGlare(float Size) : IPixelStage<VeilingGlare> {
    public static VeilingGlare Default { get; } = new(0.5f);

    public static Fin<Option<PixelPass>> Pass(VeilingGlare state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => context.Derivations.Held((state.Size, context.Extent, Window: frame.Size),
            static key => Fin.Succ(PointSpread.Veiling(key.Window, double.Lerp(180d, 10d, double.Cbrt(key.Size)) / int.Max(key.Extent.Width, key.Extent.Height))),
            spread => frame.Spread(0f, 0f, light => spread.Convolve(light), BlendingMode.Normal, 1f, [], progress))));
}

public sealed record StarGlare(Iris Aperture, ShortSideLength Radius) : IPixelStage<StarGlare> {
    public static StarGlare Default { get; } = new(Iris.Create(6, double.Pi / 3d, 0d, 0d, 1d), ShortSideLength.Create(2f / ReferenceFrame.Height));

    public static Fin<Option<PixelPass>> Pass(StarGlare state, PassContext context) =>
        state.Aperture.Obstruction is 1d ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => context.Derivations.Held((State: state, context.Extent, Window: frame.Size, context.Working),
                static key => Fin.Succ(PointSpread.Diffraction(key.State.Aperture, key.State.Radius.Pixels(key.Extent), key.Window, key.Extent, key.Working)),
                spread => frame.Spread(0f, 0f, light => spread.Convolve(light), BlendingMode.Normal, 1f, [], progress))));
}

public sealed record Ghosts(float Threshold, float Knee, int Iterations, float Modulation) : IPixelStage<Ghosts> {
    public static Ghosts Default { get; } = new(1f, 0.1f, 3, 0.25f);

    public static Fin<Option<PixelPass>> Pass(Ghosts state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => frame.Spread(state.Threshold, state.Knee,
            key => Glare.Ghosts(key, frame, 16f / ReferenceFrame.Height * frame.Extent.ShortSide / 3f, state.Iterations, state.Modulation), BlendingMode.Add, 1f, [], progress)));
}

public sealed record Halation(float Threshold, float Knee, ShortSideLength Radius, Unicolour Color) : IPixelStage<Halation> {
    public static Halation Default { get; } = new(2f, 1f, ShortSideLength.Create(64f * 0.2f / ReferenceFrame.Height), new Unicolour(ColourSpace.Rgb, 1d, 0x59 / 255d, 0d));

    public static Fin<Option<PixelPass>> Pass(Halation state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => RampLight.Light(state.Color, context) switch {
            var linear => frame.Spread(state.Threshold, state.Knee, light => {
                using ScalarArray tint = new(new MCvScalar(linear.X, linear.Y, linear.Z, 0d));
                CvInvoke.Multiply(light, tint, light);
                light.Blur(light, state.Radius.Pixels(frame.Extent) / 3d);
            }, BlendingMode.Add, 1f, [], progress),
        }));
}

[SmartEnum<string>]
public sealed partial class FocusField {
    public static readonly FocusField Uniform = new("uniform", [], static (state, _, cap) => Some(Bokeh.Correlated(state.Aperture, state.Fringe, new Vector4(cap))));
    public static readonly FocusField Depth = new("depth", [GuideChannel.Depth], static (state, context, cap) =>
        from depth in context.Guides.Find(GuideChannel.Depth)
        from camera in context.Camera
        from field in Bokeh.Depth(depth, camera, cap)
        select Bokeh.Gathered(state.Aperture, state.Fringe, field));
    public static readonly FocusField Curvature = new("curvature", [], static (state, context, cap) => Some(Bokeh.Gathered(state.Aperture, state.Fringe, state.Focal(context, state.Aspect, 2, cap))));
    public static readonly FocusField Tilt = new("tilt", [], static (state, context, cap) => Some(Bokeh.Gathered(state.Aperture, state.Fringe, state.Focal(context, 0f, 1, cap))));

    public Seq<GuideChannel> Guides { get; }

    [UseDelegateFromConstructor]
    public partial Option<PixelPass> Pass(Defocus state, PassContext context, float cap);
}

public sealed record Defocus(
    FocusField Field, ShortSideLength Radius, FieldPlacement Placement, ShortSideLength Extent, float Aspect, ShortSideLength Feather, Iris Aperture, float Fringe) : IPixelStage<Defocus> {
    public static Defocus Default { get; } = new(
        FocusField.Depth, ShortSideLength.Create(0.02f * ReferenceFrame.GreaterSide), FieldPlacement.Default, ShortSideLength.Create(0.15f), 1f, ShortSideLength.Create(0.25f),
        Iris.Create(6, double.Pi / 3d, 1d, 0d, 1d), 0f);

    public static Seq<GuideChannel> Channels(Defocus state) => state.Field.Guides;

    public static Fin<Option<PixelPass>> Pass(Defocus state, PassContext context) => state.Field.Pass(state, context, (float)state.Radius.Pixels(context.Extent));

    public Action<PixelFrame, Mat> Focal(PassContext context, float aspect, int power, float cap) =>
        Bokeh.Focal(
            DistanceField.Frame(context.Extent, context.Extent.Point(Placement.Center) + new Vector2(0.5f), Placement.Rotation),
            aspect > 0f ? DistanceField.Ellipse(new Vector2(Extent / aspect, Extent), 2f) : DistanceField.Box(new Vector2(float.PositiveInfinity, Extent), 0f),
            Feather, power, cap);
}

[SmartEnum<string>]
public sealed partial class LensDirection {
    public static readonly LensDirection Distort = new("distort", static (distorted, fitted) => (distorted, fitted));
    public static readonly LensDirection Undistort = new("undistort", static (distorted, fitted) => (fitted, distorted));

    [UseDelegateFromConstructor]
    public partial (Projection.Rectilinear Output, Projection.Rectilinear Input) Paired(Projection.Rectilinear distorted, Projection.Rectilinear fitted);
}

public sealed record LensProfile(LensDistortion Lens, double Dispersion, LensDirection Direction, float Fit, Resampling Resampling) : IPixelStage<LensProfile> {
    public static LensProfile Default { get; } = new(new LensDistortion(-0.05d, 0d, 0d, 0d, 0d), 0.05d, LensDirection.Distort, 0f, Resampling.Default with { Wrap = WrapMode.Clamp });

    public static Fin<Option<PixelPass>> Pass(LensProfile state, PassContext context) =>
        from camera in context.Camera
        from rendering in camera.Lens is Projection.Rectilinear lens ? Some(lens) : None
        where state is not { Lens: { K1: 0d, K2: 0d, P1: 0d, P2: 0d, K3: 0d }, Dispersion: 0d }
        select (PixelPass)new PixelPass.Frame((frame, progress) => state.Dispersion is 0d
            ? state.Map(rendering).Bind(map => map.Apply(frame, progress))
            : state.Placed(rendering).Bind(placed =>
                CoordinateMap.Reprojection.Dispersed(placed.Output, placed.Input, state.Dispersion, state.Resampling.Sampling, state.Resampling.Edges).Run(frame, progress)));

    public Fin<CoordinateMap> Map(Projection.Rectilinear rendering) =>
        Placed(rendering).Map(placed => (CoordinateMap)new CoordinateMap.Reprojection(placed.Output, placed.Input, System.Numerics.Quaternion.Identity, Resampling.Sampling, Resampling.Edges));

    private Fin<(Projection.Rectilinear Output, Projection.Rectilinear Input)> Placed(Projection.Rectilinear rendering) =>
        (rendering with { Distortion = Lens }) switch {
            var distorted => CoordinateMap.Reprojection.Fitted(distorted, Fit).Map(fitted => Direction.Paired(distorted, fitted)),
        };
}

public sealed record AxialAberration(ShortSideLength Radius, Iris Aperture) : IPixelStage<AxialAberration> {
    public static AxialAberration Default { get; } = new(ShortSideLength.Create(20f / ReferenceFrame.Height), Defocus.Default.Aperture);

    public static Fin<Option<PixelPass>> Pass(AxialAberration state, PassContext context) =>
        Some(Bokeh.Correlated(state.Aperture, 0d, new Vector4(0.5f, 0f, 1f, 0f) * (float)state.Radius.Pixels(context.Extent)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic]
[JsonDerivedType(typeof(Natural), nameof(Natural))]
[JsonDerivedType(typeof(Outline), nameof(Outline))]
public abstract partial record Vignette : IPixelStage<Vignette> {
    public static Vignette Default { get; } = new Natural();

    public static Fin<Option<PixelPass>> Pass(Vignette state, PassContext context) =>
        state.Switch(context,
            natural: static (run, _) => run.Camera.Bind(static camera => camera.Lens is Projection.Rectilinear lens ? Some(lens) : None)
                .Map<PixelPass>(static lens => Pulled(Vector3.Zero, (x, y) => lens.Illumination(new Vector2(x + 0.5f, y + 0.5f)).IfNone(1f))),
            outline: static (run, outline) => Some<PixelPass>(outline.Falloff(run)));

    public sealed record Natural : Vignette;

    public sealed record Outline(float Width, float Height, float Roundness, ShortSideLength Feather, Unicolour Pull) : Vignette {
        public static new Outline Default { get; } = new(0.8f, 0.8f, 1f, ShortSideLength.Create(0.2f * float.Sqrt(float.Tau) / 3f), new Unicolour(ColourSpace.RgbLinear, 0d, 0d, 0d));

        public PixelPass.Positional Falloff(PassContext context) {
            PixelExtent extent = context.Extent;
            DistanceField field = DistanceField.Box(extent.Point(new Vector2(Width, Height) / 2f), Roundness);
            float band = DistanceField.Band(Feather, extent);
            Vector2 middle = extent.Point(new Vector2(0.5f));
            return Pulled(RampLight.Light(Pull, context).AsVector3(), (x, y) => DistanceField.Coverage(field.Distance(new Vector2(x + 0.5f, y + 0.5f) - middle), band));
        }
    }

    private static PixelPass.Positional Pulled(Vector3 pull, Func<int, int, float> weight) =>
        new((row, x, y) => {
            for (int i = 0; i < row.Length; i++)
                row[i] = new(Vector3.Lerp(pull, row[i].AsVector3(), weight(x + i, y)), row[i].W);
        });
}
