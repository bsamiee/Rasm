using System.Numerics;
using System.Text.Json.Serialization;
using Darkroom.Chain;
using Rasm.Imaging.Files;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Warping;
using UnitsNet;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Resampling(Sampling Sampling, WrapMode Wrap) {
    public static Resampling Default { get; } = new(Sampling.Linear, WrapMode.Black);

    public (WrapMode Across, WrapMode Down) Edges => (Wrap, Wrap);
}

public sealed record Twirl(Angle Angle, ShortSideLength Radius, float Falloff, int Arms, Angle ArmOffset, Vector2 Center, Resampling Resampling) : IPixelStage<Twirl> {
    public static Twirl Default { get; } = new(Angle.Zero, ShortSideLength.Create(0.5f), 1f, 0, Angle.Zero, new Vector2(0.5f), Resampling.Default with { Wrap = WrapMode.Periodic });

    public static Fin<Option<PixelPass>> Pass(Twirl state, PassContext context) =>
        state.Angle == Angle.Zero ? None
            : Some<PixelPass>(new PixelPass.Frame(CoordinateMap.Field.Twirl(
                (float)state.Angle.Radians, state.Radius, state.Falloff, state.Arms, (float)state.ArmOffset.Radians, state.Center, state.Resampling.Sampling, state.Resampling.Edges).Apply));
}

[SmartEnum<string>]
public sealed partial class BulgeMode {
    public static readonly BulgeMode Round = new("round", Vector2.One);
    public static readonly BulgeMode Horizontal = new("horizontal", Vector2.UnitX);
    public static readonly BulgeMode Vertical = new("vertical", Vector2.UnitY);

    public Vector2 Axes { get; }
}

public sealed record Bulge(float Amount, BulgeMode Mode, ShortSideLength Radius, Vector2 Center, Resampling Resampling) : IPixelStage<Bulge> {
    public static Bulge Default { get; } = new(0f, BulgeMode.Round, ShortSideLength.Create(0.5f), new Vector2(0.5f), Resampling.Default with { Wrap = WrapMode.Clamp });

    public static Fin<Option<PixelPass>> Pass(Bulge state, PassContext context) =>
        state.Amount is 0f ? None
            : Some<PixelPass>(new PixelPass.Frame(CoordinateMap.Field.Bulge(state.Amount, state.Mode.Axes, state.Radius, state.Center, state.Resampling.Sampling, state.Resampling.Edges).Apply));
}

[SmartEnum<string>]
public sealed partial class WaveFront {
    public static readonly WaveFront Linear = new("linear", None);
    public static readonly WaveFront Radial = new("radial", Some(Angle.Zero));
    public static readonly WaveFront Zigzag = new("zigzag", Some(Angle.FromDegrees(90d)));
    public static readonly WaveFront Pond = new("pond", Some(Angle.FromDegrees(45d)));

    public Option<Angle> Ring { get; }
}

public sealed record WaveWarp(
    WaveFront Front, WaveProfile Profile, int Count, ShortSideLength MinWavelength, ShortSideLength MaxWavelength, float MinAmplitude, float MaxAmplitude, Vector2 Amount,
    Angle Direction, Angle Spread, Angle Phase, ShortSideLength Radius, Vector2 Center, float Rate, int Seed, Timing Timing, Hold Hold, Resampling Resampling) : IPixelStage<WaveWarp> {
    public static WaveWarp Default { get; } = new(
        WaveFront.Linear, WaveProfile.Sine, 5, ShortSideLength.Create(10f / ReferenceFrame.Height), ShortSideLength.Create(120f / ReferenceFrame.Height), 5f / 35f, 1f, Vector2.Zero,
        Angle.Zero, Angle.Zero, Angle.Zero, ShortSideLength.Create(0.5f), new Vector2(0.5f), 0f, 0, Timing.Default, Hold.MinValue, Resampling.Default);

    public static Fin<Option<PixelPass>> Pass(WaveWarp state, PassContext context) =>
        state.Amount == Vector2.Zero || state is { MinAmplitude: 0f, MaxAmplitude: 0f } ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => CoordinateMap.Field.Wave(
                    state.Profile, state.Count, (state.MinWavelength, state.MaxWavelength), (state.MinAmplitude, state.MaxAmplitude), state.Amount,
                    (float)state.Direction.Radians, (float)state.Spread.Radians, (float)state.Phase.Revolutions - (state.Rate * (float)state.Hold.Held(state.Timing.At(context)).TotalSeconds),
                    state.Front.Ring.Map(turn => (turn, state.Radius)), CoordinateHash.Branch(state.Seed, 0u, 8u), state.Center, state.Resampling.Sampling, state.Resampling.Edges)
                .Bind(map => map.Apply(frame, progress))));
}

public sealed record Turbulence(
    NoiseBasis Basis, ShortSideLength ScaleX, ShortSideLength ScaleY, Vector2 Amount, float Rate, int Seed, Timing Timing, Hold Hold, Resampling Resampling) : IPixelStage<Turbulence> {
    public static Turbulence Default { get; } = new(
        NoiseBasis.Perlin.Default, ShortSideLength.Create(ReferenceFrame.GreaterSide / 2f), ShortSideLength.Create(ReferenceFrame.GreaterSide / 2f), Vector2.Zero, 0f,
        0, Timing.Default, Hold.MinValue, Resampling.Default with { Wrap = WrapMode.Clamp });

    public static Fin<Option<PixelPass>> Pass(Turbulence state, PassContext context) =>
        state.Amount == Vector2.Zero ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Basis.Sampler.Bind(sampler => CoordinateMap.Field.Turbulence(
                (at, hash) => sampler(at, hash).Value, (state.ScaleX, state.ScaleY), state.Amount, state.Rate * (float)state.Hold.Held(state.Timing.At(context)).TotalSeconds,
                CoordinateHash.Branch(state.Seed, 0u, 9u), new Vector2(0.5f), state.Resampling.Sampling, state.Resampling.Edges).Apply(frame, progress))));
}

public sealed record MapDisplacement(ShortSideLength Softness, Vector2 Amount, Angle Rotation, Resampling Resampling) : IPixelStage<MapDisplacement> {
    public static MapDisplacement Default { get; } = new(ShortSideLength.Create(0.015f * ReferenceFrame.GreaterSide), Vector2.Zero, Angle.Zero, Resampling.Default) {
        Reading = Some(MapReading.Gradient),
    };

    public Option<string> Source { get; init; }
    public Option<MapReading> Reading { get; init; }
    public Option<ShortSideLength> TileSize { get; init; }

    public static Fin<Option<PixelPass>> Pass(MapDisplacement state, PassContext context) =>
        state.Reading.Match(
            Some: reading => state.Amount == Vector2.Zero ? None
                : Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Source.Traverse(path => Image(path, context)).As().Bind(image =>
                    reading.Pass(image, state.TileSize, state.Softness, state.Amount, (float)state.Rotation.Radians, context.Working.Luminance, (float)context.Exposure,
                        state.Resampling.Sampling, state.Resampling.Wrap).Run(frame, progress)))),
            None: () => state.Source.Map<PixelPass>(path => new PixelPass.Frame((frame, progress) => Image(path, context).Bind(image =>
                new CoordinateMap.STMap(image, state.Resampling.Sampling, state.Resampling.Edges).Apply(frame, progress)))));

    private static Fin<PixelFrame> Image(string path, PassContext context) =>
        context.Derivations.Derived(path, static held => ImageFile.Read(held).RunSafe()).Map(static read => read.Frame);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic]
[JsonDerivedType(typeof(Raindrops), nameof(Raindrops))]
[JsonDerivedType(typeof(Blocks), nameof(Blocks))]
[JsonDerivedType(typeof(Canvas), nameof(Canvas))]
[JsonDerivedType(typeof(Frosted), nameof(Frosted))]
[JsonDerivedType(typeof(TinyLens), nameof(TinyLens))]
public abstract partial record GlassTexture {
    public abstract Func<Vector2, Vector4> Surface(float held, uint field);

    public virtual bool Rests(Glass state) => state is { Strength: 0f, Blur.IsNone: true };

    public sealed record Raindrops(float Density, float Size, float Variation, float Softness, float Lens, float Trails, Angle Direction, float Fall) : GlassTexture {
        public static Raindrops Default { get; } = new(0.5f, 0.5f, 0.5f, 0.05f, 2f, 0.3f, Angle.FromDegrees(-90d), 0f);

        public override Func<Vector2, Vector4> Surface(float held, uint field) =>
            Refraction.Raindrops(Density, Size, Variation, Softness, Lens, Trails, (float)Direction.Radians, Fall * held, field);

        public override bool Rests(Glass state) => Density is 0f;
    }

    public sealed record Blocks : GlassTexture {
        public static Blocks Default { get; } = new();

        public override Func<Vector2, Vector4> Surface(float held, uint field) => Refraction.Blocks;
    }

    public sealed record Canvas : GlassTexture {
        public static Canvas Default { get; } = new();

        public override Func<Vector2, Vector4> Surface(float held, uint field) => Refraction.Canvas;
    }

    public sealed record Frosted(Octaves Octaves) : GlassTexture {
        public static Frosted Default { get; } = new(NoiseBasis.Perlin.Default.Octaves);

        public override Func<Vector2, Vector4> Surface(float held, uint field) => Refraction.Frosted(Octaves, field);
    }

    public sealed record TinyLens : GlassTexture {
        public static TinyLens Default { get; } = new();

        public override Func<Vector2, Vector4> Surface(float held, uint field) => Refraction.TinyLens;
    }
}

public sealed record Glass(
    GlassTexture Texture, ShortSideLength Scale, float Strength, ShortSideLength Smoothness, int Seed, Timing Timing, Hold Hold, Resampling Resampling) : IPixelStage<Glass> {
    public static Glass Default { get; } = new(
        GlassTexture.Raindrops.Default, ShortSideLength.Create(ReferenceFrame.GreaterSide / 2f / 3.8f), 0f, ShortSideLength.Create(3f / ReferenceFrame.Height),
        0, Timing.Default, Hold.MinValue, Resampling.Default) {
        Blur = Some(ShortSideLength.Create(14.6f / ReferenceFrame.Height)),
    };

    public Option<ShortSideLength> Blur { get; init; }

    public static Fin<Option<PixelPass>> Pass(Glass state, PassContext context) =>
        state.Texture.Rests(state) ? None
            : Some<PixelPass>(Refraction.Pass(state.Texture.Surface((float)state.Hold.Held(state.Timing.At(context)).TotalSeconds, CoordinateHash.Branch(state.Seed, 0u, 10u)), state.Scale, state.Strength, state.Smoothness, state.Blur,
                state.Resampling.Sampling, state.Resampling.Wrap));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic]
[JsonDerivedType(typeof(Rectilinear), nameof(Rectilinear))]
[JsonDerivedType(typeof(Equirectangular), nameof(Equirectangular))]
[JsonDerivedType(typeof(Cylindrical), nameof(Cylindrical))]
[JsonDerivedType(typeof(Equidistant), nameof(Equidistant))]
[JsonDerivedType(typeof(Equisolid), nameof(Equisolid))]
[JsonDerivedType(typeof(MirrorBall), nameof(MirrorBall))]
[JsonDerivedType(typeof(Stereographic), nameof(Stereographic))]
[JsonDerivedType(typeof(Panini), nameof(Panini))]
public abstract partial record PanoramaProjection {
    public abstract Fin<Projection> Of(Projection.Rectilinear rendering);

    public sealed record Rectilinear : PanoramaProjection {
        public static Rectilinear Default { get; } = new();

        public override Fin<Projection> Of(Projection.Rectilinear rendering) => rendering;
    }

    public sealed record Equirectangular : PanoramaProjection {
        public static Equirectangular Default { get; } = new();

        public override Fin<Projection> Of(Projection.Rectilinear rendering) => new Projection.Equirectangular(rendering.Extent);
    }

    public sealed record Cylindrical(Angle Sweep, float Height) : PanoramaProjection {
        public static Cylindrical Default { get; } = new(Angle.FromDegrees(180d), 0f);

        public override Fin<Projection> Of(Projection.Rectilinear rendering) =>
            ViewWindow.Centered((float)Sweep.Radians, rendering.Extent).Map(window => (Projection)new Projection.Cylindrical(rendering.Extent, window.Raised(Height)));
    }

    public sealed record Equidistant(Angle Sweep) : PanoramaProjection {
        public static Equidistant Default { get; } = new(Angle.FromDegrees(180d));

        public override Fin<Projection> Of(Projection.Rectilinear rendering) => new Projection.Equidistant(rendering.Extent, Sweep);
    }

    public sealed record Equisolid(Angle Sweep, float FullTurn) : PanoramaProjection {
        public static Equisolid Default { get; } = new(Angle.FromDegrees(180d), 2f * 10.5f / 12f);

        public override Fin<Projection> Of(Projection.Rectilinear rendering) =>
            new Projection.Equisolid(rendering.Extent, Sweep, FullTurn * (double)rendering.Extent.ShortSide / rendering.Extent.Width / 4d);
    }

    public sealed record MirrorBall : PanoramaProjection {
        public static MirrorBall Default { get; } = new();

        public override Fin<Projection> Of(Projection.Rectilinear rendering) => Projection.Equisolid.MirrorBall(rendering.Extent);
    }

    public sealed record Stereographic(Angle Sweep) : PanoramaProjection {
        public static Stereographic Default { get; } = new(Angle.FromDegrees(180d));

        public override Fin<Projection> Of(Projection.Rectilinear rendering) => new Projection.Stereographic(rendering.Extent, Sweep);
    }

    public sealed record Panini(Angle Field, float Distance, float Compression) : PanoramaProjection {
        public static Panini Default { get; } = new(Angle.FromDegrees(60d), 1f, 0f);

        public override Fin<Projection> Of(Projection.Rectilinear rendering) => new Projection.Panini(rendering.Extent, Field, Distance, Compression);
    }
}

public sealed record Panorama(PanoramaProjection Output, Angle Heading, Angle Elevation, Angle Roll, Resampling Resampling) : IPixelStage<Panorama> {
    public static Panorama Default { get; } = new(PanoramaProjection.Rectilinear.Default, Angle.Zero, Angle.Zero, Angle.Zero, Resampling.Default);

    public static Fin<Option<PixelPass>> Pass(Panorama state, PassContext context) =>
        from camera in context.Camera
        from rendering in camera.Lens is Projection.Rectilinear lens ? Some(lens) : None
        where state.Output is not PanoramaProjection.Rectilinear || (state.Heading, state.Elevation, state.Roll) != (Angle.Zero, Angle.Zero, Angle.Zero)
        select (PixelPass)new PixelPass.Frame((frame, progress) =>
            from projection in state.Output.Of(rendering)
            from map in CoordinateMap.Reprojection.Panorama(camera, _ => projection, new PanoramaView(state.Heading, state.Elevation, state.Roll), state.Resampling.Sampling, state.Resampling.Edges)
                .ToFin(new DarkroomFault(nameof(camera.Lens), DarkroomReason.Unrenderable))
            from applied in map.Apply(frame, progress)
            select applied);
}

[SmartEnum<string>]
public sealed partial class PolarConversion {
    public static readonly PolarConversion RectangularToPolar = new("rectangular-to-polar", inverse: true);
    public static readonly PolarConversion PolarToRectangular = new("polar-to-rectangular", inverse: false);

    public bool Inverse { get; }
}

public sealed record PolarCoordinates(PolarConversion Conversion, Vector2 Center, Resampling Resampling) : IPixelStage<PolarCoordinates> {
    public static PolarCoordinates Default { get; } = new(PolarConversion.RectangularToPolar, new Vector2(0.5f), Resampling.Default);

    public static Fin<Option<PixelPass>> Pass(PolarCoordinates state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame(new CoordinateMap.Polar(state.Conversion.Inverse, state.Center, state.Resampling.Sampling, state.Resampling.Edges).Apply));
}

public sealed record Kaleidoscope(int Mirrors, Angle Rotation, Vector2 Center, float Zoom, Resampling Resampling) : IPixelStage<Kaleidoscope> {
    public static Kaleidoscope Default { get; } = new(3, Angle.FromDegrees(90d), new Vector2(0.5f), 1f, Resampling.Default with { Wrap = WrapMode.Mirror });

    public static Fin<Option<PixelPass>> Pass(Kaleidoscope state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
            CoordinateMap.Field.Kaleidoscope(state.Mirrors, (float)state.Rotation.Radians, state.Zoom, state.Center, state.Resampling.Sampling, state.Resampling.Edges)
                .Bind(map => map.Apply(frame, progress))));
}

public sealed record Shake(Vector2 Amplitude, Angle Rotation, float Zoom, float Rate, Octaves Octaves, int Seed, Timing Timing, Hold Hold) {
    public static Shake Default { get; } = new(Vector2.Zero, Angle.Zero, 1f, 5f, NoiseBasis.Perlin.Default.Octaves, 0, Timing.Default, Hold.MinValue);

    public bool Still => (Amplitude, Rotation, Zoom) == (Default.Amplitude, Default.Rotation, Default.Zoom);

    public Fin<Matrix3x2> Inverse(PixelExtent extent, float time) =>
        Pose.Shaken(extent, time, Rate, Amplitude, (float)Rotation.Radians, Zoom, Octaves, CoordinateHash.Branch(Seed, 0u, 11u)).Map(static pose => pose.Inverse);
}

public sealed record ShutterMotion(Shake Shake, Duration Shutter, float Position) {
    public static ShutterMotion Default { get; } = new(Shake.Default, Duration.Zero, 0.5f);

    public bool Open => Shutter != Duration.Zero;

    public Func<float, Fin<Matrix3x2>> Path(Pose motion, Matrix3x2 placed, PassContext context) =>
        ((float)Shutter.TotalSeconds, (float)Shake.Hold.Held(Shake.Timing.At(context)).TotalSeconds) switch {
            var (duration, held) => share => Shake.Inverse(context.Extent, held - (duration * Position) + (duration * share)).Map(shaken => motion.Partial(share - Position).Inverse * shaken * placed),
        };
}

public sealed record AffineTransform(
    Vector2 Offset, Angle Rotation, Vector2 Scale, Angle SkewX, Angle SkewY, Vector2 Pivot, Vector2 Travel, Angle Turn, float Zoom, ShutterMotion Motion, Resampling Resampling)
    : IPixelStage<AffineTransform> {
    public static AffineTransform Default { get; } = new(
        Vector2.Zero, Angle.Zero, Vector2.One, Angle.Zero, Angle.Zero, new Vector2(0.5f), Vector2.Zero, Angle.Zero, 1f, ShutterMotion.Default, Resampling.Default);

    private bool Swept => (Travel, Turn, Zoom) != (Default.Travel, Default.Turn, Default.Zoom) || (Motion.Open && !Motion.Shake.Still);

    private bool Placed => (Offset, Rotation, Scale, SkewX, SkewY) != (Default.Offset, Default.Rotation, Default.Scale, Default.SkewX, Default.SkewY);

    public static Fin<Option<PixelPass>> Pass(AffineTransform state, PassContext context) =>
        state.Swept || state.Placed || !state.Motion.Shake.Still
            ? Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
                from path in state.Path(context)
                from applied in state.Swept
                    ? CoordinateMap.Integrated(context.Integration, share => path((float)share)
                        .Map<(CoordinateMap Map, Vector4 Gains)>(matrix => (state.Planar(matrix), Vector4.One))).Run(frame, progress)
                    : path(0f).Bind(matrix => state.Planar(matrix).Apply(frame, progress))
                select applied))
            : None;

    private Fin<Func<float, Fin<Matrix3x2>>> Path(PassContext context) =>
        (Pivot: context.Extent.Point(Pivot), Side: (float)context.Extent.ShortSide) switch {
            var (pivot, side) => (
                    Pose.Validated(Travel * side, (float)Turn.Radians, new Vector2(Zoom), Vector2.Zero, pivot),
                    Pose.Validated(Offset * side, (float)Rotation.Radians, Scale, new Vector2((float)SkewX.Radians, (float)SkewY.Radians), pivot))
                .Apply((motion, placed) => Motion.Path(motion, placed.Inverse, context))
                .As(),
        };

    private CoordinateMap.Planar Planar(Matrix3x2 matrix) => new(matrix, Resampling.Sampling, Resampling.Edges);
}

public sealed record CameraShake(ShutterMotion Motion, Resampling Resampling) : IPixelStage<CameraShake> {
    public static CameraShake Default { get; } = new(ShutterMotion.Default, Resampling.Default with { Wrap = WrapMode.Periodic });

    public static Fin<Option<PixelPass>> Pass(CameraShake state, PassContext context) =>
        AffineTransform.Pass(AffineTransform.Default with { Motion = state.Motion, Resampling = state.Resampling }, context);
}

public sealed record CornerPin(Vector2 TopLeft, Vector2 TopRight, Vector2 BottomRight, Vector2 BottomLeft, Resampling Resampling) : IPixelStage<CornerPin> {
    public static CornerPin Default { get; } = new(Vector2.Zero, Vector2.UnitX, Vector2.One, Vector2.UnitY, Resampling.Default);

    public static Fin<Option<PixelPass>> Pass(CornerPin state, PassContext context) =>
        state with { Resampling = Default.Resampling } == Default ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
                from quad in FrameQuad.Validated(context.Extent.Point(state.TopLeft), context.Extent.Point(state.TopRight), context.Extent.Point(state.BottomRight), context.Extent.Point(state.BottomLeft))
                from applied in CoordinateMap.Planar.Pinned(quad, context.Extent, state.Resampling.Sampling, state.Resampling.Edges).Apply(frame, progress)
                select applied));
}
